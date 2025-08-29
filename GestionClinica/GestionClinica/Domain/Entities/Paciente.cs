namespace GestionClinica.Domain.Entities;

public class Paciente
{
    public int Id { get; set; } 
    public string? Nombres { get; set; }
    public string? Apellidos { get; set; }
    public string? Correo { get; set; }
}
