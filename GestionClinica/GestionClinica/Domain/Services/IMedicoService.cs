using GestionClinica.Domain.DTOs;

namespace GestionClinica.Domain.Services;

public interface IMedicoService
{
    Task<int> RegistrarAsync(MedicoCreateDto dto);
    Task<IEnumerable<MedicoListVm>> ListarAsync();
    Task<MedicoDetailVm?> ObtenerMedicoAsync(int id);
    Task<IEnumerable<MedicoEspecialidadVm>> MedicosPorEspecialidadAsync(string especialidad);
    Task<IEnumerable<DiaDisponibilidadVm>> DisponibilidadPorRangoAsync(int idMedico, DateTime fecha);
    Task<IEnumerable<ProductividadMedicaDto>> ReporteProductividadAsync(DateTime desde, DateTime hasta);
    Task<MedicoDetailVm> ActualizarAsync(int id, MedicoUpdateDto dto);
    Task<bool> EliminarAsync(int id);

}
