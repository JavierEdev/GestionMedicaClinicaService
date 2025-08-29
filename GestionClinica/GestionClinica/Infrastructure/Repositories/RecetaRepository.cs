using GestionClinica.Domain.Entities;
using GestionClinica.Domain.Repositories;
using GestionClinica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionClinica.Infrastructure.Repositories;
public class RecetaRepository : IRecetaRepository
{
    private readonly ClinicaDbContext _db;
    public RecetaRepository(ClinicaDbContext db) => _db = db;

    public async Task<IEnumerable<int>> CreateManyAsync(IEnumerable<RecetaMedica> recetas)
    {
        _db.Recetas.AddRange(recetas);
        await _db.SaveChangesAsync();
        return recetas.Select(x => x.Id).ToList();
    }

    public Task<RecetaMedica?> GetByIdAsync(int idReceta)
        => _db.Recetas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == idReceta)!;

    public async Task<IEnumerable<RecetaMedica>> ListByConsultaAsync(int idConsulta)
        => await _db.Recetas.AsNoTracking()
            .Where(x => x.IdConsulta == idConsulta)
            .OrderBy(x => x.Id)
            .ToListAsync();

    public async Task<IEnumerable<(RecetaMedica r, ConsultaMedica c, Medico m, Paciente p)>> ListHistoricoPacienteAsync(int idPaciente)
    {
        var q =
            from r in _db.Recetas.AsNoTracking()
            join c in _db.Consultas.AsNoTracking() on r.IdConsulta equals c.Id
            join m in _db.Medicos.AsNoTracking() on c.IdMedico equals m.Id
            join p in _db.Pacientes.AsNoTracking() on c.IdPaciente equals p.Id
            where c.IdPaciente == idPaciente
            orderby c.Fecha
            select new { r, c, m, p };

        var list = await q.ToListAsync();
        return list.Select(x => (x.r, x.c, x.m, x.p));
    }

    public async Task UpdateAsync(RecetaMedica r)
    {
        _db.Recetas.Update(r);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int idReceta)
    {
        await _db.Recetas.Where(x => x.Id == idReceta).ExecuteDeleteAsync();
    }

}
