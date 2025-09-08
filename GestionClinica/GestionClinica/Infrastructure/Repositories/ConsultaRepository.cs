using GestionClinica.Domain.Entities;
using GestionClinica.Domain.Repositories;
using GestionClinica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionClinica.Infrastructure.Repositories;

public class ConsultaRepository : IConsultaRepository
{
    private readonly ClinicaDbContext _db;
    public ConsultaRepository(ClinicaDbContext db) => _db = db;

    public Task<ConsultaMedica?> GetByIdAsync(int idConsulta)
        => _db.Consultas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == idConsulta)!;
}
