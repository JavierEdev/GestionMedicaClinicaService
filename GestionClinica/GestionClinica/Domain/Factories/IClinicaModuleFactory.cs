using GestionClinica.Domain.Services;

namespace GestionClinica.Domain.Factories;
public interface IClinicaModuleFactory
{
    ICitaService CreateCitaService();
    IEmailService CreateEmailService();
    IAuditLogService CreateAuditLogService();
    IUnitOfWork CreateUnitOfWork();
    IMedicoService CreateMedicoService();
    IRecetaService CreateRecetaService();
}