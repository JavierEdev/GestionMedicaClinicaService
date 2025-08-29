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

    public CitaService(ICitaRepository c, IMedicoRepository m, IPacienteRepository p, IEmailService e, IAuditLogService log)
        => (_citas, _medicos, _pacientes, _email, _log) = (c, m, p, e, log);

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

        var fechaLocal = dto.Fecha.Kind == DateTimeKind.Utc
            ? TimeZoneInfo.ConvertTimeFromUtc(dto.Fecha, TimeZoneInfo.Local)
            : dto.Fecha;

        if (!DentroHorarioFijo(fechaLocal))
            throw new InvalidOperationException("Fecha/hora fuera del horario laboral del médico (08:00-17:00).");

        if (await _citas.HaySolapeAsync(dto.IdMedico, fechaLocal))
            throw new InvalidOperationException("Médico ocupado en ese horario.");

        var id = await _citas.CreateAsync(new Cita
        {
            IdPaciente = dto.IdPaciente,
            IdMedico = dto.IdMedico,
            Fecha = DateTime.SpecifyKind(fechaLocal, DateTimeKind.Unspecified),
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
}

