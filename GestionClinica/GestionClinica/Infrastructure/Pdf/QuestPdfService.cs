using GestionClinica.Domain.DTOs;
using GestionClinica.Domain.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionClinica.Infrastructure.Pdf;

public class QuestPdfService : IPdfService
{
    public byte[] GenerateRecetaPdf(RecetaDetalleVm m)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var accent = Colors.Blue.Medium;
        var gray = Colors.Grey.Medium;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header().Column(col =>
                {
                    col.Item().Text(m.ClinicaNombre)
                        .FontSize(22).SemiBold().FontColor(accent)
                        .AlignCenter();

                    col.Item().Text("Receta Médica")
                        .FontSize(16).SemiBold()
                        .AlignCenter();

                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor(accent);
                });

                page.Content().Column(col =>
                {
                    col.Item().PaddingVertical(6).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Paciente: {m.PacienteNombre}").SemiBold();
                            c.Item().Text($"Consulta #: {m.IdConsulta}");
                            c.Item().Text($"Fecha consulta: {m.FechaConsulta:dd/MM/yyyy HH:mm}");
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Médico: {m.MedicoNombre}").SemiBold();
                            c.Item().Text($"Especialidad: {m.Especialidad}");
                        });
                    });

                    if (!string.IsNullOrWhiteSpace(m.Diagnostico) || !string.IsNullOrWhiteSpace(m.Observaciones))
                    {
                        col.Item().PaddingVertical(4).Text("Diagnóstico y observaciones").SemiBold().FontColor(accent);
                        col.Item().Background(Colors.Grey.Lighten5).Padding(8).Column(cc =>
                        {
                            if (!string.IsNullOrWhiteSpace(m.Diagnostico))
                                cc.Item().Text($"Diagnóstico: {m.Diagnostico}").FontColor(gray);
                            if (!string.IsNullOrWhiteSpace(m.Observaciones))
                                cc.Item().Text($"Observaciones: {m.Observaciones}").FontColor(gray);
                        });
                    }

                    col.Item().PaddingVertical(8).Text("Indicación farmacológica").SemiBold().FontColor(accent);

                    col.Item().Background(Colors.Grey.Lighten5).Padding(12).Column(cc =>
                    {
                        cc.Item().Text($"Medicamento: {m.Medicamento}").FontSize(14).SemiBold();
                        cc.Item().Text($"Dosis: {m.Dosis}");
                        cc.Item().Text($"Frecuencia: {m.Frecuencia}");
                        cc.Item().Text($"Duración: {m.Duracion}");
                    });

                    col.Item().PaddingTop(10).LineHorizontal(0.5f);

                    col.Item().AlignRight().Text(text =>
                    {
                        text.Span("Id Receta: ").SemiBold();
                        text.Span($"{m.IdReceta}");
                    });
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span(m.ClinicaNombre + "  •  ").SemiBold();
                    txt.Span($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });
        });

        return doc.GeneratePdf();
    }
}
