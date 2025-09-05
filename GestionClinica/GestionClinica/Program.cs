using GestionClinica.Application.Services;
using GestionClinica.Domain.Factories;
using GestionClinica.Domain.Repositories;
using GestionClinica.Domain.Services;
using GestionClinica.Infrastructure.Factories;
using GestionClinica.Infrastructure.Logging;
using GestionClinica.Infrastructure.Notifications;
using GestionClinica.Infrastructure.Pdf;
using GestionClinica.Infrastructure.Persistence;
using GestionClinica.Infrastructure.Repositories;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;


builder.Services.AddDbContext<ClinicaDbContext>(opt =>
{
    var cs = cfg.GetConnectionString("MySql")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:MySql en appsettings.");
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs));
});

const string CorsPolicy = "SpaDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CorsPolicy, policy =>
    {
        policy.WithOrigins(
                  "http://localhost:5173",
                  "https://localhost:5173"
               )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ClinicaDbContext>(opt =>
{
    var cs = cfg.GetConnectionString("MySql")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:MySql en appsettings.");
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs));
});


builder.Services.AddScoped<IMedicoRepository, MedicoRepository>();
builder.Services.AddScoped<ICitaRepository, CitaRepository>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IRecetaRepository, RecetaRepository>();
builder.Services.AddScoped<IConsultaRepository, ConsultaRepository>();


builder.Services.AddScoped<ICitaService, CitaService>();
builder.Services.AddScoped<IMedicoService, MedicoService>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IRecetaService, RecetaService>();


builder.Services.Configure<SmtpSettings>(cfg.GetSection("Smtp"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddSingleton<IAuditLogService, NoOpAuditLogService>();
builder.Services.AddSingleton<IPdfService, QuestPdfService>();
builder.Services.AddScoped<IClinicaModuleFactory, MySqlClinicaFactory>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddMySql(cfg.GetConnectionString("MySql")!, name: "mysql", failureStatus: HealthStatus.Unhealthy);


var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health/db");
app.UseSwagger(); app.UseSwaggerUI();
app.Run();
