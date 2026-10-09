using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QualiTrack.Models;

namespace QualiTrack.Services;

public class PdfReportService(IStorageService storage, IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
{
    public async Task<byte[]> GenerateAuditReport(AuditSession session)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var schedule = session.Schedule;
        var plan = schedule.AuditPlan;
        var responses = session.Responses?.ToList() ?? [];
        var findings = session.Findings?.ToList() ?? [];

        // Pre-download semua evidence images
        var evidenceImages = new Dictionary<string, byte[]>();
        foreach(var f in findings)
        {
            foreach(var ev in f.Evidences ?? [])
            {
                if (!evidenceImages.ContainsKey(ev.StoragePath))
                {
                    var bytes = await GetImageBytesAsync(ev.StoragePath);
                    if(bytes != null)
                        evidenceImages[ev.StoragePath] = bytes;
                }
            }
        }

        foreach(var r in responses)
        {
            foreach(var ev in r.Evidences ?? [])
            {
                if (!evidenceImages.ContainsKey(ev.StoragePath))
                {
                    var bytes = await GetImageBytesAsync(ev.StoragePath);
                    if(bytes != null)
                        evidenceImages[ev.StoragePath] = bytes;
                }
                
            }
        }

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Content().Element(content => ComposeAll(content, session, schedule, plan, responses, findings, evidenceImages));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("QualiTrack — Halaman ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private async Task<byte[]?> GetImageBytesAsync(string storagePath)
    {
        try
        {
            // Local - baca dari disk
            var localPath = Path.Combine(env.ContentRootPath, "uploads", Path.GetFileName(storagePath));
            if(File.Exists(localPath))
                return await File.ReadAllBytesAsync(localPath);

            // S3 - download via presignet URL
            var url = storage.GetPresignedUrl(storagePath);
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);
            if(response.IsSuccessStatusCode)
                return await response.Content.ReadAsByteArrayAsync();

            return null;
        }
        catch
        {
            return null;
        }
    }

    private void ComposeAll(IContainer container, AuditSession session, AuditSchedule schedule, AuditPlan plan, List<AuditResponse> responses, List<Finding> findings, Dictionary<string, byte[]> evidenceImages)
    {
        container.Column(col =>
        {
            // ===== HEADER =====
            col.Item().Background("#1a3a5c").Padding(16).Column(h =>
            {
                h.Item().Text("QUALITRACK - AUDIT REPORT")
                    .FontSize(18).Bold().FontColor("#ffffff").AlignCenter();
                h.Item().PaddingTop(4).Text($"Internal Audit Report - {plan.Standard}")
                    .FontSize(11).FontColor("#adc8e6").AlignCenter();
            });

            col.Item().PaddingTop(12).Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                void InfoRow(string label, string value)
                {
                    table.Cell().Padding(4).Text(label).SemiBold();
                    table.Cell().Padding(4).Text(value);
                }

                InfoRow("Audit ID", $"{plan.Id.ToString()[..8].ToUpper()}");
                InfoRow("Department", schedule.Department);
                InfoRow("AuditorInternal", schedule.AuditorName);
                InfoRow("Audit Date", schedule.ScheduledDate.ToString("dd MMM yyyy"));
                InfoRow("Standard", plan.Standard);
                InfoRow("Audit Status", session.Status.ToString());
            });

            // ===== AUDIT SUMMARY =====
            if (!string.IsNullOrEmpty(session.Summary?.Content))
            {
                col.Item().PaddingTop(12).Column(s =>
                {
                    s.Item().Background("#f0f4f8").Padding(8).Text("Audit Summary")
                        .FontSize(12).Bold();
                    s.Item().Padding(8).Text(session.Summary!.Content);
                });
            }

            // ===== CHECKLIST RESULT =====
            col.Item().PaddingTop(16).Column(s =>
            {
                s.Item().Background("#f0f4f8").Padding(8).Text("Checklist Result")
                    .FontSize(12).Bold();

                if (!responses.Any())
                {
                    s.Item().Padding(8).Text("Belum ada jawaban checklist.").Italic();
                }
                else
                {
                    s.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);  // Checklist
                            c.RelativeColumn(1);  // Result
                        });

                        // Header
                        table.Header(h =>
                        {
                            static IContainer HCell(IContainer c) =>
                                c.Background("#1a3a5c").Padding(6);
                            static TextStyle HText() =>
                                TextStyle.Default.FontColor("#ffffff").FontSize(9).Bold();

                            h.Cell().Element(HCell).Text("Checklist").Style(HText());
                            h.Cell().Element(HCell).Text("Result").Style(HText());
                        });

                        for (int i = 0; i < responses.Count; i++)
                        {
                            var r = responses[i];
                            var bg = i % 2 == 0 ? "#f9f9f9" : "#ffffff";
                            var resultColor = r.Answer == ResponseAnswer.Conform ? "#2e7d32"
                                : r.Answer == ResponseAnswer.NotConform ? "#c62828" : "#757575";
                            var resultText = r.Answer == ResponseAnswer.Conform ? "PASS"
                                : r.Answer == ResponseAnswer.NotConform ? "FAIL" : "N/A";

                            IContainer DCell(IContainer c) => c.Background(bg).Padding(5);

                            table.Cell().Element(DCell).Text(r.ChecklistItem?.Question ?? "-");
                            table.Cell().Element(DCell).Text(resultText).FontColor(resultColor).Bold();
                        }
                    });
                }
            });

            // ===== EVIDENCE DOCUMENTATION =====
            var passResponses = responses.Where(r => r.Answer == ResponseAnswer.Conform && r.Evidences != null && r.Evidences.Any()).ToList();
            var failFindings = findings.Where(f => f.Category == FindingCategory.MajorNC || f.Category == FindingCategory.MinorNC || f.Category == FindingCategory.OFI).ToList();

            if (passResponses.Any() || failFindings.Any())
            {
                col.Item().PaddingTop(16).Column(s =>
                {
                    s.Item().Background("#f0f4f8").Padding(8).Text("Evidence Documentation")
                        .FontSize(12).Bold();

                    // PASS — foto saja
                    if (passResponses.Any())
                    {
                        s.Item().PaddingTop(8).Text("✓ Conforming Evidence").FontSize(10).Bold().FontColor("#2e7d32");
                        foreach (var r in passResponses)
                        {
                            s.Item().PaddingTop(6).Column(rc =>
                            {
                                rc.Item().Text(r.ChecklistItem?.Question ?? "-").FontSize(9).FontColor("#555555");
                                foreach (var ev in r.Evidences!)
                                {
                                    evidenceImages.TryGetValue(ev.StoragePath, out var imgBytes);
                                    if(imgBytes != null)
                                        rc.Item().PaddingTop(4).Height(180).Image(imgBytes).FitArea();
                                    else
                                        rc.Item().Text($"📎 {ev.FileName}").FontSize(9).FontColor("#1565c0");
                                }
                            });
                        }
                    }

                    // FAIL — foto + category + note (dari finding description)
                    if (failFindings.Any())
                    {
                        s.Item().PaddingTop(12).Text("✗ Non-Conforming Findings").FontSize(10).Bold().FontColor("#c62828");
                        foreach (var f in failFindings)
                        {
                            s.Item().PaddingTop(8).Column(fc =>
                            {
                                fc.Item().Text(f.Title).FontSize(11).Bold().FontColor("#c62828");
                                fc.Item().Text($"Category: {f.Category}").FontSize(9).FontColor("#555555");
                                fc.Item().PaddingTop(2).Text($"Note: {f.Description}").FontSize(9);

                                var findingEvidences = f.Evidences?.ToList() ?? [];
                                if (findingEvidences.Any())
                                {
                                    fc.Item().PaddingTop(4).Text("Evidence:").FontSize(9).SemiBold();
                                    foreach(var ev in findingEvidences)
                                    {
                                        evidenceImages.TryGetValue(ev.StoragePath, out var imageBytes);
                                        if(imageBytes != null)
                                        {
                                            fc.Item().PaddingTop(4).Height(180).Image(imageBytes).FitArea();
                                        }
                                        else
                                        {
                                            fc.Item().Text($"📎 {ev.FileName}").FontSize(9).FontColor("#1565c0");
                                        }
                                    }
                                }
                            });
                        }
                    }
                });
            }

            // ===== RECOMMENDATION =====
            // col.Item().PaddingTop(16).Column(s =>
            // {
            //     s.Item().Text("Recommendation").FontSize(14).Bold();
            //     s.Item().LineHorizontal(1).LineColor("#1a3a5c");
            //     s.Item().PaddingTop(8).Column(rc =>
            //     {
            //         if (!failFindings.Any())
            //         {
            //             rc.Item().Text("Tidak ada temuan signifikan. Pertahankan standar yang ada.").Italic();
            //         }
            //         else
            //         {
            //             for (int i = 0; i < failFindings.Count; i++)
            //             {
            //                 var f = failFindings[i];
            //                 rc.Item().Text($"{i + 1}. {f.Description}");
            //             }
            //         }
            //     });
            // });
        });
    }
}
