using GestionClinica.Domain.DTOs;
using GestionClinica.Domain.Entities;
using GestionClinica.Domain.Repositories;
using GestionClinica.Domain.Services;

namespace GestionClinica.Application.Services;

public class RecetaService : IRecetaService
{
    private readonly IRecetaRepository _recetas;
    private readonly IConsultaRepository _consultas;
    private readonly IMedicoRepository _medicos;
    private readonly IPacienteRepository _pacientes;
    private readonly IPdfService _pdf;

    public RecetaService(
        IRecetaRepository recetas,
        IConsultaRepository consultas,
        IMedicoRepository medicos,
        IPacienteRepository pacientes,
        IPdfService pdf)
        => (_recetas, _consultas, _medicos, _pacientes, _pdf)
         = (recetas, consultas, medicos, pacientes, pdf);

    public async Task<IEnumerable<int>> GenerarAsync(RecetaCreateDto dto)
    {
        _ = await _consultas.GetByIdAsync(dto.IdConsulta)
            ?? throw new KeyNotFoundException("Consulta no existe");

        var recetas = dto.Items.Select(i => new RecetaMedica
        {
            IdConsulta = dto.IdConsulta,
            Medicamento = i.Medicamento,
            Dosis = i.Dosis,
            Frecuencia = i.Frecuencia,
            Duracion = i.Duracion
        }).ToList();

        var ids = await _recetas.CreateManyAsync(recetas);
        return ids;
    }

    public async Task<RecetaVm?> ObtenerAsync(int idReceta)
    {
        var r = await _recetas.GetByIdAsync(idReceta);
        return r is null ? null : new RecetaVm(r.Id, r.IdConsulta, r.Medicamento, r.Dosis, r.Frecuencia, r.Duracion);
    }

    public async Task<IEnumerable<RecetaVm>> ListarPorConsultaAsync(int idConsulta)
    {
        var list = await _recetas.ListByConsultaAsync(idConsulta);
        return list.Select(r => new RecetaVm(r.Id, r.IdConsulta, r.Medicamento, r.Dosis, r.Frecuencia, r.Duracion));
    }

    public async Task<IEnumerable<MedicamentoHistoricoVm>> HistoricoPorPacienteAsync(int idPaciente)
    {
        var rows = await _recetas.ListHistoricoPacienteAsync(idPaciente);
        return rows.Select(x => new MedicamentoHistoricoVm(
            x.c.Fecha, x.r.Id, x.r.Medicamento, x.r.Dosis, x.r.Frecuencia, x.r.Duracion,
            x.c.Id, x.m.Id, $"{x.m.Nombres} {x.m.Apellidos}"));
    }

    public async Task<byte[]> ImprimirPdfAsync(int idReceta)
    {
        var r = await _recetas.GetByIdAsync(idReceta) ?? throw new KeyNotFoundException("Receta no existe");
        var c = await _consultas.GetByIdAsync(r.IdConsulta) ?? throw new KeyNotFoundException("Consulta no existe");
        var m = await _medicos.GetByIdAsync(c.IdMedico) ?? throw new KeyNotFoundException("Médico no existe");
        var p = await _pacientes.GetByIdAsync(c.IdPaciente) ?? throw new KeyNotFoundException("Paciente no existe");

        var det = new RecetaDetalleVm(
            r.Id,
            r.IdConsulta,
            p.Id,
            m.Id,
            c.Fecha,
            r.Medicamento,
            r.Dosis,
            r.Frecuencia,
            r.Duracion,
            $"{m.Nombres} {m.Apellidos}",
            $"{p.Nombres} {p.Apellidos}",
            m.Especialidad ?? "",
            c.Diagnostico,
            c.Observaciones,
            "Clinica Seminario Proyecto"
        );

        return _pdf.GenerateRecetaPdf(det);
    }
    public async Task<RecetaVm> ActualizarAsync(int idReceta, RecetaUpdateDto dto)
    {
        var r = await _recetas.GetByIdAsync(idReceta) ?? throw new KeyNotFoundException("Receta no existe");

        r.Medicamento = dto.Medicamento;
        r.Dosis = dto.Dosis;
        r.Frecuencia = dto.Frecuencia;
        r.Duracion = dto.Duracion;

        await _recetas.UpdateAsync(r);

        return new RecetaVm(r.Id, r.IdConsulta, r.Medicamento, r.Dosis, r.Frecuencia, r.Duracion);
    }

    public async Task<bool> EliminarAsync(int idReceta)
    {
        _ = await _recetas.GetByIdAsync(idReceta) ?? throw new KeyNotFoundException("Receta no existe");
        await _recetas.DeleteAsync(idReceta);
        return true;
    }

}
