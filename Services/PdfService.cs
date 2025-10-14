using System.IO;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RelatoriosTI.API.Models;

namespace RelatoriosTI.API.Services
{
    public class PdfService
    {
        public Task<byte[]> GerarPdf(DadosRelatorio relatorio, EstatisticasRelatorio estatisticas)
        {
            var stats = estatisticas;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Column(col =>
                        {
                            col.Item().AlignCenter().Text($"Relatório de TI - {relatorio.HotelNome}")
                                .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
                            col.Item().AlignCenter().Text($"Gerado em {relatorio.DataGeracao:dd/MM/yyyy}")
                                .FontSize(12).FontColor(Colors.Grey.Medium);
                        });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(col =>
                        {
                            col.Spacing(15);

                            // Licenças
                            col.Item().Text("Licenças").Bold().FontSize(14);
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Antivírus").FontSize(9).FontColor(Colors.Grey.Darken2);
                                    c.Item().Text(relatorio.QtdLicencasAntivirus.ToString()).Bold().FontSize(24).FontColor(Colors.Blue.Medium);
                                });

                                row.Spacing(10);

                                row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Office 365 Standard").FontSize(9).FontColor(Colors.Grey.Darken2);
                                    c.Item().Text(relatorio.QtdLicencasOffice365Standard.ToString()).Bold().FontSize(24).FontColor(Colors.Green.Medium);
                                });

                                row.Spacing(10);

                                row.RelativeItem().Background(Colors.Purple.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Office 365 Basic").FontSize(9).FontColor(Colors.Grey.Darken2);
                                    c.Item().Text(relatorio.QtdLicencasOffice365Basic.ToString()).Bold().FontSize(24).FontColor(Colors.Purple.Medium);
                                });
                            });

                            // Preventivas e Renovações
                            col.Item().Text("Status e Renovações").Bold().FontSize(14);
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Preventivas").Bold().FontSize(11);
                                    c.Item().Text(relatorio.StatusPreventivas)
                                        .FontColor(relatorio.StatusPreventivas == "Em dia" ? Colors.Green.Medium : Colors.Red.Medium);
                                });

                                row.Spacing(10);

                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Renovações").Bold().FontSize(11);
                                    c.Item().Text($"Antivírus: {relatorio.DataRenovacaoAntivirus:dd/MM/yyyy}").FontSize(9);
                                    c.Item().Text($"Office 365: {relatorio.DataRenovacaoOffice365:dd/MM/yyyy}").FontSize(9);
                                    if (!string.IsNullOrEmpty(relatorio.OutrosContratos))
                                        c.Item().Text(relatorio.OutrosContratos).FontSize(8).FontColor(Colors.Grey.Medium);
                                });
                            });

                            // Estatísticas de Chamados
                            col.Item().Text("Resumo de Chamados").Bold().FontSize(14);
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Background(Colors.Grey.Lighten3).Padding(10).AlignCenter().Column(c =>
                                {
                                    c.Item().Text("Total").FontSize(9);
                                    c.Item().Text(stats.TotalChamados.ToString()).Bold().FontSize(20);
                                });
                                row.Spacing(5);
                                row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).AlignCenter().Column(c =>
                                {
                                    c.Item().Text("Tempo Médio (h)").FontSize(9);
                                    c.Item().Text(stats.TempoMedioResolucao.ToString("0.0")).Bold().FontSize(20);
                                });
                                row.Spacing(5);
                                row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).AlignCenter().Column(c =>
                                {
                                    c.Item().Text("Tipos").FontSize(9);
                                    c.Item().Text(stats.ChamadosPorTipo.Count.ToString()).Bold().FontSize(20);
                                });
                                row.Spacing(5);
                                row.RelativeItem().Background(Colors.Purple.Lighten4).Padding(10).AlignCenter().Column(c =>
                                {
                                    c.Item().Text("Solicitantes").FontSize(9);
                                    c.Item().Text(stats.ChamadosPorSolicitante.Count.ToString()).Bold().FontSize(20);
                                });
                            });

                            // Chamados por Analista
                            col.Item().Text("Chamados por Analista").Bold().FontSize(14);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Analista").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Qtd").Bold();
                                });

                                foreach (var item in stats.ChamadosPorAnalista)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Analista);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(item.Quantidade.ToString()).Bold();
                                }
                            });

                            // Chamados por Tipo
                            col.Item().PageBreak();
                            col.Item().Text("Chamados por Tipo de Atendimento").Bold().FontSize(14);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Tipo").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Qtd").Bold();
                                });

                                foreach (var item in stats.ChamadosPorTipo)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Tipo);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(item.Quantidade.ToString()).Bold();
                                }
                            });

                            // Chamados por Solicitante
                            col.Item().Text("Chamados por Solicitante").Bold().FontSize(14);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Solicitante").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Local").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Qtd").Bold();
                                });

                                foreach (var item in stats.ChamadosPorSolicitante)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Solicitante);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Local);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(item.Quantidade.ToString()).Bold();
                                }
                            });

                            // Detalhes dos Tickets
                            col.Item().PageBreak();
                            col.Item().Text("Detalhes dos Tickets").Bold().FontSize(14);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);    // Ticket
                                    columns.RelativeColumn(3);    // Assunto
                                    columns.RelativeColumn(2);    // Solicitante
                                    columns.RelativeColumn(2);    // Analista
                                    columns.RelativeColumn(1);    // Status
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Ticket").Bold().FontSize(8);
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Assunto").Bold().FontSize(8);
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Solicitante").Bold().FontSize(8);
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Analista").Bold().FontSize(8);
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Status").Bold().FontSize(8);
                                });

                                foreach (var ticket in relatorio.Tickets)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ticket.NumeroTicket).FontSize(8);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ticket.Assunto ?? "Sem assunto").FontSize(7);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ticket.Solicitante).FontSize(8);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ticket.Analista).FontSize(8).FontColor(Colors.Blue.Medium);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(ticket.StatusTicket).FontSize(8);
                                }
                            });

                            // Melhorias para o Próximo Mês
                            if (!string.IsNullOrEmpty(relatorio.MelhoriasProximoMes))
                            {
                                col.Item().PageBreak();
                                col.Item().Text("Melhorias Planejadas para o Próximo Mês").Bold().FontSize(14);
                                col.Item().Background(Colors.Grey.Lighten4).Padding(10).Text(relatorio.MelhoriasProximoMes).FontSize(10);
                            }
                        });

                    // Footer: aplica estilo antes do Text(...) para evitar encadeamento em void
                    page.Footer()
                        .AlignCenter()
                        .DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium))
                        .Text(x =>
                        {
                            x.Span("Relatório gerado em: ");
                            x.Span(relatorio.DataGeracao.ToString("dd/MM/yyyy HH:mm"));
                            x.Span(" | Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return Task.FromResult(stream.ToArray());
        }
    }
}
