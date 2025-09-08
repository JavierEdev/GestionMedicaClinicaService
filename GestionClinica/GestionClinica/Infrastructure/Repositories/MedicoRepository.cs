using GestionClinica.Domain.DTOs;
using GestionClinica.Domain.Entities;
using GestionClinica.Domain.Repositories;
using GestionClinica.Domain.Services;
using GestionClinica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionClinica.Infrastructure.Repositories;

public class MedicoRepository : IMedicoRepository
{
    private readonly ClinicaDbContext _db;
    public MedicoRepository(ClinicaDbContext db) => _db = db;

    public Task<Medico?> GetByIdAsync(int id)
        => _db.Medicos.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id)!;

    public async Task<IEnumerable<Medico>> SearchByEspecialidadAsync(string especialidad)
        => await _db.Medicos.AsNoTracking().Where(m => m.Especialidad == especialidad).ToListAsync();

    public Task<bool> ExistsNumeroColegiadoAsync(string numero)
        => _db.Medicos.AsNoTracking().AnyAsync(m => m.NumeroColegiado == numero);

    public async Task<int> CreateAsync(Medico m)
    {
        _db.Medicos.Add(m);
        await _db.SaveChangesAsync();
        return m.Id;
    }
    public async Task<IEnumerable<Medico>> ListAllAsync()
        => await _db.Medicos.AsNoTracking()
            .OrderBy(m => m.Apellidos).ThenBy(m => m.Nombres)
            .ToListAsync();

    public async Task<IEnumerable<ProductividadMedicaDto>> GetProductividadAsync(DateTime desde, DateTime hasta)
    {
        var consultasPorMedico = await _db.Consultas
            .Where(c => c.Fecha >= desde && c.Fecha <= hasta)
            .GroupBy(c => c.IdMedico)
            .Select(g => new { IdMedico = g.Key, Consultas = g.Count() })
            .ToListAsync();

        var procsPorMedico = await
            (from c in _db.Consultas
             where c.Fecha >= desde && c.Fecha <= hasta
             join p in _db.Procedimientos on c.Id equals p.IdConsulta
             group p by c.IdMedico into g
             select new { IdMedico = g.Key, Procedimientos = g.Count() })
            .ToListAsync();

        var indexConsultas = consultasPorMedico.ToDictionary(x => x.IdMedico, x => x.Consultas);
        var indexProcs = procsPorMedico.ToDictionary(x => x.IdMedico, x => x.Procedimientos);

        var medicos = await _db.Medicos.AsNoTracking().ToListAsync();

        var res = new List<ProductividadMedicaDto>();
        foreach (var m in medicos)
        {
            var c = indexConsultas.TryGetValue(m.Id, out var cc) ? cc : 0;
            var p = indexProcs.TryGetValue(m.Id, out var pp) ? pp : 0;
            if (c == 0 && p == 0) continue;
            res.Add(new ProductividadMedicaDto(m.Id, $"{m.Nombres} {m.Apellidos}", c, p));
        }
        return res;
    }

    public async Task UpdateAsync(Medico m)
    {
        _db.Medicos.Update(m);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> HasDependenciasAsync(int idMedico)
    {
        var tieneCitas = await _db.Citas.AsNoTracking().AnyAsync(c => c.IdMedico == idMedico);
        var tieneConsultas = await _db.Consultas.AsNoTracking().AnyAsync(c => c.IdMedico == idMedico);
        return tieneCitas || tieneConsultas;
    }

    public async Task DeleteAsync(int idMedico)
    {
        await _db.Medicos.Where(m => m.Id == idMedico).ExecuteDeleteAsync();
    }

    public Task<bool> ExistsNumeroColegiadoExceptIdAsync(string numero, int excludeId)
        => _db.Medicos.AsNoTracking().AnyAsync(m => m.NumeroColegiado == numero && m.Id != excludeId);
}
