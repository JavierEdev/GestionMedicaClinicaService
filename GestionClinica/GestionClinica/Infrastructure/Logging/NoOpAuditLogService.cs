using GestionClinica.Domain.Services;

namespace GestionClinica.Infrastructure.Logging;

public class NoOpAuditLogService : IAuditLogService
{
    public Task WriteAsync(string area, string action, object payload)
        => Task.CompletedTask;
}
