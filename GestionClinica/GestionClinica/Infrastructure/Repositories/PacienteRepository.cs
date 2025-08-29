using GestionClinica.Domain.Entities;
using GestionClinica.Domain.Repositories;
using GestionClinica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionClinica.Infrastructure.Repositories;

public class PacienteRepository : IPacienteRepository
{
    private readonly ClinicaDbContext _db;
    public PacienteRepository(ClinicaDbContext db) => _db = db;
    public Task<Paciente?> GetByIdAsync(int id)
        => _db.Pacientes.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id)!;
}
