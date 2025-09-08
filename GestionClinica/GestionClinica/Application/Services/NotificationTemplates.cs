using System.Globalization;
using GestionClinica.Domain.Entities;

namespace GestionClinica.Application.Services;

public static class NotificationTemplates
{
    private static string D(DateTime dt) => dt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    private static string H(DateTime dt) => dt.ToString("HH:mm", CultureInfo.InvariantCulture);
    private static string NombreCompleto(string? n, string? a) => $"{n} {a}".Trim();

    public static (string subject, string html) CitaCreada(
        Paciente pac, Medico med, DateTime fecha)
    {
        var subj = "Cita Clínica - Confirmación";
        var body = $@"
<p>Estimado/a {NombreCompleto(pac.Nombres, pac.Apellidos)},</p>
<p>Se generó una cita para usted el día <b>{D(fecha)}</b> a las <b>{H(fecha)}</b> con el médico <b>{NombreCompleto(med.Nombres, med.Apellidos)}</b> (especialidad: {med.Especialidad}).</p>
<p>Por favor presentarse 30 minutos antes.</p>
<p>¡Feliz día!</p>";
        return (subj, body);
    }

    public static (string subject, string html) CitaCancelada(
        Paciente pac, Medico med, DateTime fecha, string razon)
    {
        var subj = "Cancelación de Cita";
        var body = $@"
<p>Estimado/a {NombreCompleto(pac.Nombres, pac.Apellidos)},</p>
<p>Su cita del día <b>{D(fecha)}</b> a las <b>{H(fecha)}</b> con el médico <b>{NombreCompleto(med.Nombres, med.Apellidos)}</b> fue <b>cancelada</b>.</p>
<p>Motivo: {razon}</p>
<p>Si desea generar otra cita, contáctenos.</p>";
        return (subj, body);
    }

    public static (string subject, string html) CitaReprogramada(
        Paciente pac, Medico med, DateTime fechaAnterior, DateTime nuevaFecha, string? motivo)
    {
        var subj = "Reprogramación de Cita";
        var body = $@"
<p>Estimado/a {NombreCompleto(pac.Nombres, pac.Apellidos)},</p>
<p>Su cita originalmente programada para el día <b>{D(fechaAnterior)}</b> a las <b>{H(fechaAnterior)}</b> con el médico <b>{NombreCompleto(med.Nombres, med.Apellidos)}</b> ha sido <b>reprogramada</b>.</p>
<p>Nueva fecha: <b>{D(nuevaFecha)}</b> a las <b>{H(nuevaFecha)}</b>.</p>
<p>Motivo: {motivo ?? "-"}</p>";
        return (subj, body);
    }
}
