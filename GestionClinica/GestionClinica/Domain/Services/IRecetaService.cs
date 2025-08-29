using GestionClinica.Domain.DTOs;

namespace GestionClinica.Domain.Services;
public interface IRecetaService
{
    Task<IEnumerable<int>> GenerarAsync(RecetaCreateDto dto);
    Task<RecetaVm?> ObtenerAsync(int idReceta);
    Task<IEnumerable<RecetaVm>> ListarPorConsultaAsync(int idConsulta);
    Task<IEnumerable<MedicamentoHistoricoVm>> HistoricoPorPacienteAsync(int idPaciente);
    Task<byte[]> ImprimirPdfAsync(int idReceta);
    Task<RecetaVm> ActualizarAsync(int idReceta, RecetaUpdateDto dto);
    Task<bool> EliminarAsync(int idReceta);

}
