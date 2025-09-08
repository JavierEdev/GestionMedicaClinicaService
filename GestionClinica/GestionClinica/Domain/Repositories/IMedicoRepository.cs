using GestionClinica.Domain.Entities;
using GestionClinica.Domain.Services;
using GestionClinica.Domain.DTOs;

namespace GestionClinica.Domain.Repositories;

public interface IMedicoRepository
{
    Task<Medico?> GetByIdAsync(int id);
    Task<IEnumerable<Medico>> SearchByEspecialidadAsync(string especialidad);
    Task<bool> ExistsNumeroColegiadoAsync(string numero);
    Task<int> CreateAsync(Medico m);
    Task<IEnumerable<ProductividadMedicaDto>> GetProductividadAsync(DateTime desde, DateTime hasta);
    Task<IEnumerable<Medico>> ListAllAsync();
    Task UpdateAsync(Medico m);
    Task<bool> HasDependenciasAsync(int idMedico);
    Task DeleteAsync(int idMedico);
    Task<bool> ExistsNumeroColegiadoExceptIdAsync(string numero, int excludeId);
}
