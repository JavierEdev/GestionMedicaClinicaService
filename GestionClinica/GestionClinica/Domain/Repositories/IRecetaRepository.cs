using GestionClinica.Domain.Entities;

namespace GestionClinica.Domain.Repositories;
public interface IRecetaRepository
{
    Task<IEnumerable<int>> CreateManyAsync(IEnumerable<RecetaMedica> recetas);
    Task<RecetaMedica?> GetByIdAsync(int idReceta);
    Task<IEnumerable<RecetaMedica>> ListByConsultaAsync(int idConsulta);
    Task<IEnumerable<(RecetaMedica r, ConsultaMedica c, Medico m, Paciente p)>> ListHistoricoPacienteAsync(int idPaciente);
    Task UpdateAsync(RecetaMedica r);
    Task DeleteAsync(int idReceta);
}
