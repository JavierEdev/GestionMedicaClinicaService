using GestionClinica.Domain.Entities;

namespace GestionClinica.Domain.Repositories;

public interface ICitaRepository
{
    Task<int> CreateAsync(Cita c);
    Task<Cita?> GetByIdAsync(int id);
    Task UpdateAsync(Cita c);
    Task<IEnumerable<Cita>> ListByMedicoAsync(int idMedico, DateTime? fecha);
    Task<IEnumerable<Cita>> ListAllAsync();
    Task<IEnumerable<Cita>> ListByPacienteAsync(int idPaciente);
    Task<Cita?> GetByIdForPacienteAsync(int idPaciente, int idCita);
    Task<bool> HaySolapeAsync(int idMedico, DateTime fecha);

}
