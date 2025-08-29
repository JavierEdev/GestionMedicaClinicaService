namespace GestionClinica.Domain.Entities;
public class Medico
{
    public int Id { get; set; }
    public string Nombres { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string? NumeroColegiado { get; set; }
    public string Especialidad { get; set; } = null!;
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? HorarioLaboral { get; set; }
}
