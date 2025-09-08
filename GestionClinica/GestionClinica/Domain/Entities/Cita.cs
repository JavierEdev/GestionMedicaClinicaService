namespace GestionClinica.Domain.Entities;
public class Cita
{
    public int Id { get; set; }
    public int IdPaciente { get; set; }
    public int IdMedico { get; set; }
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = "confirmada";
    public string? RazonCancelacion { get; set; }
}
