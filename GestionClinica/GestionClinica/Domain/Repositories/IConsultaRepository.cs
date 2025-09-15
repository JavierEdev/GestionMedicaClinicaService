using GestionClinica.Domain.Entities;

namespace GestionClinica.Domain.Repositories;

public interface IConsultaRepository
{
    Task<ConsultaMedica?> GetByIdAsync(int idConsulta);
    Task<ConsultaMedica?> GetByCitaIdAsync(int idCita);
}
