using System.Text.RegularExpressions;
using GestionClinica.Common;
using GestionClinica.Domain.DTOs;
using GestionClinica.Domain.Entities;
using GestionClinica.Domain.Repositories;
using GestionClinica.Domain.Services;

namespace GestionClinica.Application.Services;

public class MedicoService : IMedicoService
{
    private readonly IMedicoRepository _medicos;
    private readonly ICitaRepository _citas;
    private readonly IAuditLogService _log;

    public MedicoService(IMedicoRepository medicos, ICitaRepository citas, IAuditLogService log)
        => (_medicos, _citas, _log) = (medicos, citas, log);

    private static readonly Regex ColegiadoRegex = new(@"^COL-\d{6}$");

    public async Task<int> RegistrarAsync(MedicoCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NumeroColegiado) || !ColegiadoRegex.IsMatch(dto.NumeroColegiado))
            throw new InvalidOperationException("El número de colegiado debe tener el formato COL-######.");

        if (await _medicos.ExistsNumeroColegiadoAsync(dto.NumeroColegiado))
            throw new InvalidOperationException("El número de colegiado ya existe.");

        var medico = new Medico
        {
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            NumeroColegiado = dto.NumeroColegiado,
            Especialidad = dto.Especialidad,
            Telefono = dto.Telefono,
            Correo = dto.Correo,
            HorarioLaboral = "08:00-17:00"
        };

        var id = await _medicos.CreateAsync(medico);
        await _log.WriteAsync("Medico", "Create", new { id, dto });
        return id;
    }

    public async Task<IEnumerable<MedicoListVm>> ListarAsync()
    {
        var list = await _medicos.ListAllAsync();
        return list.Select(m => new MedicoListVm(m.Id, $"{m.Nombres} {m.Apellidos}", m.Especialidad));
    }

    public async Task<MedicoDetailVm?> ObtenerMedicoAsync(int id)
    {
        var m = await _medicos.GetByIdAsync(id);
        if (m is null) return null;

        var horario = string.IsNullOrWhiteSpace(m.HorarioLaboral) ? "08:00-17:00" : m.HorarioLaboral;

        return new MedicoDetailVm(
            m.Id, m.Nombres, m.Apellidos, m.NumeroColegiado,
            m.Especialidad, m.Telefono, m.Correo, horario
        );
    }

    public async Task<IEnumerable<MedicoEspecialidadVm>> MedicosPorEspecialidadAsync(string especialidad)
    {
        var medicos = await _medicos.SearchByEspecialidadAsync(especialidad);
        return medicos.Select(m => new MedicoEspecialidadVm(
            m.Id, $"{m.Nombres} {m.Apellidos}", m.Especialidad, m.HorarioLaboral)
        );
    }

    private static IEnumerable<string> SlotsFijos()
    {
        for (var h = 8; h < 17; h++)
            yield return $"{h:D2}:00";
    }

    public async Task<IEnumerable<DiaDisponibilidadVm>> DisponibilidadPorRangoAsync(int idMedico, DateTime fecha)
    {
        _ = await _medicos.GetByIdAsync(idMedico) ?? throw new KeyNotFoundException("Médico no existe");

        var diaDate = fecha.Date;
        var citas = await _citas.ListByMedicoAsync(idMedico, diaDate);
        var ocupadas = new HashSet<DateTime>(
            citas.Select(c => DateTime.SpecifyKind(c.Fecha, DateTimeKind.Unspecified))
        );

        var horas = new List<string>();
        for (var h = 8; h < 17; h++)
        {
            var slot = new DateTime(diaDate.Year, diaDate.Month, diaDate.Day, h, 0, 0);
            if (!ocupadas.Contains(slot))
                horas.Add($"{h:D2}:00");
        }

        return new[] { new DiaDisponibilidadVm(DateOnly.FromDateTime(diaDate), horas) };
    }

    public Task<IEnumerable<ProductividadMedicaDto>> ReporteProductividadAsync(DateTime desde, DateTime hasta)
        => _medicos.GetProductividadAsync(desde, hasta);

    public async Task<MedicoDetailVm> ActualizarAsync(int id, MedicoUpdateDto dto)
    {
        var m = await _medicos.GetByIdAsync(id) ?? throw new KeyNotFoundException("Médico no existe");

        if (string.IsNullOrWhiteSpace(dto.NumeroColegiado) || !ColegiadoRegex.IsMatch(dto.NumeroColegiado))
            throw new InvalidOperationException("El número de colegiado debe tener el formato COL-######.");

        if (await _medicos.ExistsNumeroColegiadoExceptIdAsync(dto.NumeroColegiado, id))
            throw new InvalidOperationException("El número de colegiado ya existe.");

        m.Nombres = dto.Nombres;
        m.Apellidos = dto.Apellidos;
        m.NumeroColegiado = dto.NumeroColegiado;
        m.Especialidad = dto.Especialidad;
        m.Telefono = dto.Telefono;
        m.Correo = dto.Correo;

        await _medicos.UpdateAsync(m);
        await _log.WriteAsync("Medico", "Update", new { id, dto });

        var horario = string.IsNullOrWhiteSpace(m.HorarioLaboral) ? "08:00-17:00" : m.HorarioLaboral;
        return new MedicoDetailVm(m.Id, m.Nombres, m.Apellidos, m.NumeroColegiado, m.Especialidad, m.Telefono, m.Correo, horario);
    }

    public async Task<bool> EliminarAsync(int id)
    {
        _ = await _medicos.GetByIdAsync(id) ?? throw new KeyNotFoundException("Médico no existe");

        if (await _medicos.HasDependenciasAsync(id))
            throw new InvalidOperationException("No se puede eliminar el médico porque tiene citas y/o consultas asociadas.");

        await _medicos.DeleteAsync(id);
        await _log.WriteAsync("Medico", "Delete", new { id });
        return true;
    }
}
