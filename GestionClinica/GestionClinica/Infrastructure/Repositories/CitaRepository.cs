using GestionClinica.Domain.Entities;
using GestionClinica.Domain.Repositories;
using GestionClinica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionClinica.Infrastructure.Repositories;
public class CitaRepository : ICitaRepository
{
    private readonly ClinicaDbContext _db;
    public CitaRepository(ClinicaDbContext db) => _db = db;

    public async Task<int> CreateAsync(Cita c) { _db.Citas.Add(c); await _db.SaveChangesAsync(); return c.Id; }
    public Task<Cita?> GetByIdAsync(int id) => _db.Citas.FindAsync(id).AsTask();
    public async Task UpdateAsync(Cita c) { _db.Citas.Update(c); await _db.SaveChangesAsync(); }

    public async Task<IEnumerable<Cita>> ListByMedicoAsync(int idMedico)
        => await _db.Citas.AsNoTracking()
            .Where(x => x.IdMedico == idMedico)
            .OrderBy(x => x.Fecha).ToListAsync();
    public async Task<IEnumerable<Cita>> ListByPacienteAsync(int idPaciente)
    => await _db.Citas.AsNoTracking()
        .Where(x => x.IdPaciente == idPaciente)
        .OrderBy(x => x.Fecha)
        .ToListAsync();

    public Task<Cita?> GetByIdForPacienteAsync(int idPaciente, int idCita)
        => _db.Citas.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == idCita && x.IdPaciente == idPaciente)!;

    public Task<bool> HaySolapeAsync(int idMedico, DateTime fecha)
        => _db.Citas.AsNoTracking().AnyAsync(x => x.IdMedico == idMedico && x.Fecha == fecha);

    public async Task<IEnumerable<Cita>> ListByMedicoAsync(int idMedico, DateTime? fecha)
    {
        var q = _db.Citas.AsNoTracking().Where(c => c.IdMedico == idMedico);
        if (fecha.HasValue)
        {
            var inicio = fecha.Value.Date;
            var fin = inicio.AddDays(1).AddTicks(-1);
            q = q.Where(c => c.Fecha >= inicio && c.Fecha <= fin);
        }
        return await q.OrderBy(c => c.Fecha).ToListAsync();
    }
    public async Task<IEnumerable<Cita>> ListAllAsync()
    => await _db.Citas.AsNoTracking().OrderBy(c => c.Fecha).ToListAsync();
}
