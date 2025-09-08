using GestionClinica.Domain.Entities;

namespace GestionClinica.Domain.Repositories;

public interface IPacienteRepository
{
    Task<Paciente?> GetByIdAsync(int id);
}
