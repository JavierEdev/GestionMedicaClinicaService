using GestionClinica.Domain.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace GestionClinica.Infrastructure.Notifications;

public class SmtpSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Secure { get; set; } = "StartTls";
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = "";
}

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _cfg;
    public SmtpEmailService(IOptions<SmtpSettings> cfg) => _cfg = cfg.Value;

    public async Task EnviarAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient();
        var secure = _cfg.Secure?.ToLowerInvariant() switch
        {
            "sslonconnect" => SecureSocketOptions.SslOnConnect,
            "starttls" => SecureSocketOptions.StartTls,
            "none" => SecureSocketOptions.None,
            _ => SecureSocketOptions.StartTls
        };

        await client.ConnectAsync(_cfg.Host, _cfg.Port, secure);

        if (!string.IsNullOrWhiteSpace(_cfg.User))
            await client.AuthenticateAsync(_cfg.User, _cfg.Password);

        var msg = new MimeMessage();
        msg.From.Add(MailboxAddress.Parse(_cfg.From));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;
        msg.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

        await client.SendAsync(msg);
        await client.DisconnectAsync(true);
    }
}
