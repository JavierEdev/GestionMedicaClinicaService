namespace GestionClinica.Domain.Entities;

public class ProcedimientoMedico
{
    public int IdProcedimiento { get; set; }
    public int IdConsulta { get; set; } 
    public string? Procedimiento { get; set; }
    public string? Descripcion { get; set; }
}
