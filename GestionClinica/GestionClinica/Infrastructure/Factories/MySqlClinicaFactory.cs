using GestionClinica.Domain.Factories;
using GestionClinica.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GestionClinica.Infrastructure.Factories;
public class MySqlClinicaFactory : IClinicaModuleFactory
{
    private readonly IServiceProvider _sp;
    public MySqlClinicaFactory(IServiceProvider sp) => _sp = sp;

    public ICitaService CreateCitaService() => _sp.GetRequiredService<ICitaService>();
    public IEmailService CreateEmailService() => _sp.GetRequiredService<IEmailService>();
    public IAuditLogService CreateAuditLogService() => _sp.GetRequiredService<IAuditLogService>();
    public IUnitOfWork CreateUnitOfWork() => _sp.GetRequiredService<IUnitOfWork>();
    public IMedicoService CreateMedicoService() => _sp.GetRequiredService<IMedicoService>();
    public IRecetaService CreateRecetaService() => _sp.GetRequiredService<IRecetaService>();

}
