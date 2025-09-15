using GestionClinica.Common;
using GestionClinica.Domain.DTOs;
using GestionClinica.Domain.Entities;
using GestionClinica.Domain.Repositories;
using GestionClinica.Domain.Services;

namespace GestionClinica.Application.Services;

public class CitaService : ICitaService
{
    private readonly ICitaRepository _citas;
    private readonly IMedicoRepository _medicos;
    private readonly IEmailService _email;
    private readonly IAuditLogService _log;
    private readonly IPacienteRepository _pacientes;
    private readonly IConsultaRepository _consultas;
    private readonly IMedicoService _medicoSvc;

    public CitaService(ICitaRepository c, IMedicoRepository m, IPacienteRepository p,
                       IEmailService e, IAuditLogService log,
                       IConsultaRepository consultas, IMedicoService medicoSvc)
        => (_citas, _medicos, _pacientes, _email, _log, _consultas, _medicoSvc)
         = (c, m, p, e, log, consultas, medicoSvc);

    private static IEnumerable<string> SlotsFijos()
    {
        for (var h = 8; h < 17; h++)
            yield return $"{h:D2}:00";
    }

    private static bool DentroHorarioFijo(DateTime fecha)
    {
        var hora = fecha.TimeOfDay;
        return hora >= TimeSpan.FromHours(8) && hora < TimeSpan.FromHours(17);
    }

    public async Task<CitaCreatedVm> AgendarAsync(CitaCreateDto dto)
    {
        var medico = await _medicos.GetByIdAsync(dto.IdMedico) ?? throw new KeyNotFoundException("Médico no existe");
        var paciente = await _pacientes.GetByIdAsync(dto.IdPaciente) ?? throw new KeyNotFoundException("Paciente no existe");

        var fechaLocal = DateTime.SpecifyKind(dto.Fecha, DateTimeKind.Unspecified);

        if (!DentroHorarioFijo(fechaLocal))
            throw new InvalidOperationException("Fecha/hora fuera del horario laboral del médico (08:00-17:00).");

        if (await _citas.HaySolapeAsync(dto.IdMedico, fechaLocal))
            throw new InvalidOperationException("Médico ocupado en ese horario.");

        var id = await _citas.CreateAsync(new Cita
        {
            IdPaciente = dto.IdPaciente,
            IdMedico = dto.IdMedico,
            Fecha = fechaLocal,
            Estado = "confirmada"
        });

        if (!string.IsNullOrWhiteSpace(paciente.Correo))
        {
            var (subj, body) = NotificationTemplates.CitaCreada(paciente, medico, fechaLocal);
            await _email.EnviarAsync(paciente.Correo!, subj, body);
        }

        await _log.WriteAsync("Cita", "Create", new { id, dto });

        return new CitaCreatedVm(id, dto.IdPaciente, dto.IdMedico, fechaLocal);
    }

    public async Task<CitaCancelledVm> CancelarAsync(int idCita, string razon)
    {
        var c = await _citas.GetByIdAsync(idCita) ?? throw new KeyNotFoundException("Cita no existe");
        c.Estado = "cancelada";
        c.RazonCancelacion = razon;
        await _citas.UpdateAsync(c);

        var pac = await _pacientes.GetByIdAsync(c.IdPaciente);
        var med = await _medicos.GetByIdAsync(c.IdMedico) ?? new Medico { Nombres = "(Desconocido)", Apellidos = "" };
        if (!string.IsNullOrWhiteSpace(pac?.Correo))
        {
            var (subj, body) = NotificationTemplates.CitaCancelada(pac!, med, c.Fecha, razon);
            await _email.EnviarAsync(pac!.Correo!, subj, body);
        }

        await _log.WriteAsync("Cita", "Cancel", new { idCita, razon });
        return new CitaCancelledVm(idCita, razon);
    }

    public async Task<CitaRescheduledVm> ReprogramarAsync(int idCita, DateTime nuevaFecha, string? motivo)
    {
        var c = await _citas.GetByIdAsync(idCita) ?? throw new KeyNotFoundException("Cita no existe");

        var nuevaLocal = nuevaFecha.Kind == DateTimeKind.Utc
            ? TimeZoneInfo.ConvertTimeFromUtc(nuevaFecha, TimeZoneInfo.Local)
            : nuevaFecha;

        if (!DentroHorarioFijo(nuevaLocal))
            throw new InvalidOperationException("Nueva fecha/hora fuera del horario laboral (08:00-17:00).");

        if (await _citas.HaySolapeAsync(c.IdMedico, nuevaLocal))
            throw new InvalidOperationException("Médico ocupado en ese horario.");

        var anterior = c.Fecha;
        c.Fecha = DateTime.SpecifyKind(nuevaLocal, DateTimeKind.Unspecified);
        c.Estado = "reprogramada";
        c.RazonCancelacion = motivo;
        await _citas.UpdateAsync(c);

        var pac = await _pacientes.GetByIdAsync(c.IdPaciente);
        var med = await _medicos.GetByIdAsync(c.IdMedico) ?? new Medico { Nombres = "(Desconocido)", Apellidos = "" };
        if (!string.IsNullOrWhiteSpace(pac?.Correo))
        {
            var (subj, body) = NotificationTemplates.CitaReprogramada(pac!, med, anterior, c.Fecha, motivo);
            await _email.EnviarAsync(pac!.Correo!, subj, body);
        }

        await _log.WriteAsync("Cita", "Reschedule", new { idCita, nuevaFecha, motivo });
        return new CitaRescheduledVm(idCita, c.Fecha, motivo);
    }

    private static readonly TimeSpan Slot = TimeSpan.FromMinutes(30);

    public async Task<IEnumerable<MedicoEspecialidadVm>> MedicosPorEspecialidadAsync(string especialidad)
    {
        var medicos = await _medicos.SearchByEspecialidadAsync(especialidad);
        var list = new List<MedicoEspecialidadVm>();

        foreach (var m in medicos)
        {
            var horario = string.IsNullOrWhiteSpace(m.HorarioLaboral)
                ? "08:00-17:00"
                : m.HorarioLaboral;

            list.Add(new MedicoEspecialidadVm(
                m.Id,
                $"{m.Nombres} {m.Apellidos}",
                m.Especialidad,
                horario
            ));
        }

        return list;
    }

    public async Task<IEnumerable<CitaVm>> CitasPorMedicoEnDiaAsync(int idMedico, DateTime fecha)
    {
        var dia = fecha.Date;
        var citas = await _citas.ListByMedicoAsync(idMedico, dia);
        return citas.Select(c => new CitaVm(
            c.Id, c.IdPaciente, c.IdMedico, c.Fecha, c.Estado
        ));
    }

    public async Task<IEnumerable<CitaVm>> CalendarioMedicoHistorialAsync(int idMedico)
    {
        var citas = await _citas.ListByMedicoAsync(idMedico, null);
        return citas.Select(c => new CitaVm(c.Id, c.IdPaciente, c.IdMedico, c.Fecha, c.Estado));
    }

    public async Task<IEnumerable<CitaVm>> CitasPorPacienteAsync(int idPaciente)
    {
        var citas = await _citas.ListByPacienteAsync(idPaciente);
        return citas.Select(c => new CitaVm(c.Id, c.IdPaciente, c.IdMedico, c.Fecha, c.Estado));
    }

    public async Task<CitaVm?> ObtenerCitaDePacienteAsync(int idPaciente, int idCita)
    {
        var c = await _citas.GetByIdForPacienteAsync(idPaciente, idCita);
        return c is null ? null : new CitaVm(c.Id, c.IdPaciente, c.IdMedico, c.Fecha, c.Estado);
    }
    public async Task<IEnumerable<CitaVm>> CitasMedico(int idMedico)
    {
        var citas = await _citas.ListByMedicoAsync(idMedico, null);
        return citas.Select(c => new CitaVm(c.Id, c.IdPaciente, c.IdMedico, c.Fecha, c.Estado));
    }
    public async Task<IEnumerable<CitaVm>> ListarTodasAsync()
    {
        var citas = await _citas.ListAllAsync();
        return citas.Select(c => new CitaVm(c.Id, c.IdPaciente, c.IdMedico, c.Fecha, c.Estado));
    }
    public async Task<IEnumerable<CitaListadoVm>> ListarTodasDetalladoAsync()
    {
        var rows = await _citas.ListAllDetailedAsync();
        return rows.Select(x => new CitaListadoVm(
            x.c.Id,
            x.p.Id,
            $"{x.p.Nombres} {x.p.Apellidos}",
            x.m.Id,
            $"{x.m.Nombres} {x.m.Apellidos}",
            x.m.Especialidad ?? string.Empty,
            x.c.Fecha,
            x.c.Estado
        ));
    }

    public async Task<CitaReasignadaVm> ReasignarMedicoAsync(int idCita, int nuevoMedicoId)
    {
        if (idCita <= 0 || nuevoMedicoId <= 0)
            throw new ArgumentException("Parámetros inválidos.");

        var cita = await _citas.GetByIdAsync(idCita) ?? throw new KeyNotFoundException("Cita no existe");
        if (string.Equals(cita.Estado, "cancelada", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("La cita está cancelada.");

        var consulta = await _consultas.GetByCitaIdAsync(idCita);
        if (consulta is not null)
            throw new InvalidOperationException("La cita ya tiene consulta asociada; no se puede reasignar el médico.");

        var nuevo = await _medicos.GetByIdAsync(nuevoMedicoId)
                   ?? throw new KeyNotFoundException("Médico nuevo no existe");

        var medicoActual = await _medicos.GetByIdAsync(cita.IdMedico)
                          ?? throw new KeyNotFoundException("Médico actual no existe");

        var especialidad = medicoActual.Especialidad ?? string.Empty;
        var medicosMismaEsp = await _medicoSvc.MedicosPorEspecialidadAsync(especialidad);
        var coincideEspecialidad = medicosMismaEsp.Any(m => m.Id == nuevoMedicoId);
        if (!coincideEspecialidad)
            throw new InvalidOperationException("Especialidad incompatible: el nuevo médico no pertenece a la misma especialidad.");

        var fecha = cita.Fecha.Date;
        var disponibilidad = await _medicoSvc.DisponibilidadPorRangoAsync(nuevoMedicoId, fecha);
        var dia = disponibilidad.FirstOrDefault(d => d.Fecha == DateOnly.FromDateTime(fecha));
        var horaCita = cita.Fecha.ToString("HH:mm");

        if (dia is null || !dia.HorasDisponibles.Contains(horaCita))
            throw new InvalidOperationException("El nuevo médico no está disponible en la fecha y hora de la cita.");

        var idMedicoAnterior = cita.IdMedico;
        cita.IdMedico = nuevoMedicoId;
        await _citas.UpdateAsync(cita);

        await _log.WriteAsync("Cita", "ReassignDoctor", new { idCita, idMedicoAnterior, nuevoMedicoId });

        return new CitaReasignadaVm(
            cita.Id,
            cita.IdPaciente,
            idMedicoAnterior,
            nuevoMedicoId,
            cita.Fecha,
            cita.Estado
        );
    }

    public async Task<IEnumerable<MedicoEspecialidadVm>> MedicosDisponiblesParaCitaAsync(int idCita)
    {
        var cita = await _citas.GetByIdAsync(idCita) ?? throw new KeyNotFoundException("Cita no existe");
        if (string.Equals(cita.Estado, "cancelada", StringComparison.OrdinalIgnoreCase))
            return Enumerable.Empty<MedicoEspecialidadVm>();

        var medicoActual = await _medicos.GetByIdAsync(cita.IdMedico)
                          ?? throw new KeyNotFoundException("Médico actual no existe");

        var especialidad = medicoActual.Especialidad ?? string.Empty;

        var medicosMismaEsp = await _medicoSvc.MedicosPorEspecialidadAsync(especialidad);
        medicosMismaEsp = medicosMismaEsp.Where(m => m.Id != medicoActual.Id);

        var fecha = cita.Fecha.Date;
        var horaCita = cita.Fecha.ToString("HH:mm");
        var diaCita = DateOnly.FromDateTime(fecha);

        var disponibles = new List<MedicoEspecialidadVm>();
        foreach (var m in medicosMismaEsp)
        {
            var disp = await _medicoSvc.DisponibilidadPorRangoAsync(m.Id, fecha);
            var dia = disp.FirstOrDefault(d => d.Fecha == diaCita);
            if (dia is not null && dia.HorasDisponibles.Contains(horaCita))
                disponibles.Add(m);
        }

        return disponibles;
    }

}

