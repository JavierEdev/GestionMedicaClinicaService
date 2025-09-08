using GestionClinica.Domain.Services;
using Microsoft.EntityFrameworkCore.Storage;

namespace GestionClinica.Infrastructure.Persistence;
public class EfUnitOfWork : IUnitOfWork
{
    private readonly ClinicaDbContext _db;
    private IDbContextTransaction? _tx;
    public EfUnitOfWork(ClinicaDbContext db) => _db = db;
    public async Task BeginAsync() => _tx = await _db.Database.BeginTransactionAsync();
    public async Task CommitAsync() { if (_tx != null) { await _tx.CommitAsync(); await _tx.DisposeAsync(); } }
    public async Task RollbackAsync() { if (_tx != null) { await _tx.RollbackAsync(); await _tx.DisposeAsync(); } }
}
