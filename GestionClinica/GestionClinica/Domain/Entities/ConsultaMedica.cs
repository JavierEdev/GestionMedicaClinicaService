namespace GestionClinica.Domain.Entities;

public class ConsultaMedica
{
    public int Id { get; set; }
    public int IdMedico { get; set; }
    public int IdPaciente { get; set; }
    public DateTime Fecha { get; set; }
    public string? MotivoConsulta { get; set; }
    public string? Diagnostico { get; set; }
    public string? Observaciones { get; set; }

}