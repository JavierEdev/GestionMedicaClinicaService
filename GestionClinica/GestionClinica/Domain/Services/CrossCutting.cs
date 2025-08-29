namespace GestionClinica.Domain.Services;

public interface IEmailService { Task EnviarAsync(string to, string subject, string body); }
public interface IAuditLogService { Task WriteAsync(string area, string action, object payload); }
public interface IUnitOfWork { Task BeginAsync(); Task CommitAsync(); Task RollbackAsync(); }
