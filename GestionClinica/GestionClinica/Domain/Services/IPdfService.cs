using GestionClinica.Domain.DTOs;

namespace GestionClinica.Domain.Services;
public interface IPdfService
{
    byte[] GenerateRecetaPdf(RecetaDetalleVm modelo);
}
