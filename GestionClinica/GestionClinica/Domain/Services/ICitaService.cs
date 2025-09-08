using GestionClinica.Domain.DTOs;

namespace GestionClinica.Domain.Services;

public interface ICitaService
{
    Task<CitaCreatedVm> AgendarAsync(CitaCreateDto dto);
    Task<CitaCancelledVm> CancelarAsync(int idCita, string razon);
    Task<CitaRescheduledVm> ReprogramarAsync(int idCita, DateTime nuevaFecha, string? motivo);
    Task<IEnumerable<CitaVm>> CitasMedico(int idMedico);
    Task<IEnumerable<CitaVm>> CitasPorMedicoEnDiaAsync(int idMedico, DateTime fecha);
    Task<IEnumerable<CitaVm>> CitasPorPacienteAsync(int idPaciente);
    Task<CitaVm?> ObtenerCitaDePacienteAsync(int idPaciente, int idCita);
    Task<IEnumerable<CitaVm>> ListarTodasAsync();

}
