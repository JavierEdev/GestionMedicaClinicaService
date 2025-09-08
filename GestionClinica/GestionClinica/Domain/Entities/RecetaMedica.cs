namespace GestionClinica.Domain.Entities;

public class RecetaMedica
{
    public int Id { get; set; }
    public int IdConsulta { get; set; }
    public string Medicamento { get; set; } = null!;
    public string Dosis { get; set; } = null!;
    public string Frecuencia { get; set; } = null!;
    public string Duracion { get; set; } = null!;
}
