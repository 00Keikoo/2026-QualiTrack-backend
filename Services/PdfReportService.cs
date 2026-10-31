using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Docnet.Core;
using Docnet.Core.Models;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Logging;
using QualiTrack.Constants;
using QualiTrack.Models;
using System.Collections.Concurrent;
using System.Threading;

namespace QualiTrack.Services;

public class PdfReportMetadata
{
    public int PageCount { get; set; }
    public long FileSizeBytes { get; set; }
    public string FileSizeFormatted { get; set; } = string.Empty;
}
public class PdfReportService(
    IStorageService storage,
    IWebHostEnvironment env,
    IHttpClientFactory httpClientFactory,
    ILogger<PdfReportService> logger)
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _pdfLocks = new();
    public async Task<byte[]> GenerateAuditReport(AuditSession session)
    {
        if (session.Schedule == null)
            throw new InvalidOperationException($"Session {session.Id} tidak memiliki Schedule");
        
        if (session.Schedule.AuditPlan == null)
            throw new InvalidOperationException($"Schedule untuk session {session.Id} tidak memiliki AuditPlan");

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
            // Cek storage type untuk determine strategy
            if (storage is LocalStorageService)
            {
                // LocalStorageService: baca langsung dari disl
                var fileName = Path.GetFileName(storagePath);
                var localPath = Path.Combine(env.ContentRootPath, "uploads", fileName);
                if (File.Exists(localPath))
                    return await File.ReadAllBytesAsync(localPath);
                logger.LogWarning("Local file not found: {Path}", localPath);
                return null;
            }
            else
            {
                // S3StorageService: download via presigned URL
                var url = storage.GetPresignedUrl(storagePath);
                var client = httpClientFactory.CreateClient();
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsByteArrayAsync();
                
                logger.LogWarning("Failed to download from S3: {Url}, Status: {StatusCode}",
                    url, response.StatusCode);
                
                return null;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Exception while fetching image from {StoragePath}", storagePath);
            return null;
        }
    }

    private void ComposeAll(IContainer container, AuditSession session, AuditSchedule schedule, AuditPlan plan, List<AuditResponse> responses, List<Finding> findings, Dictionary<string, byte[]> evidenceImages)
    {
        container.Column(col =>
        {
            //HEADER
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

            //AUDIT SUMMARY
            if (!string.IsNullOrEmpty(session.Summary?.Content))
            {
                col.Item().PaddingTop(12).Column(s =>
                {
                    s.Item().Background("#f0f4f8").Padding(8).Text("Audit Summary")
                        .FontSize(12).Bold();
                    s.Item().Padding(8).Text(session.Summary!.Content);
                });
            }

            //CHECKLIST RESULT
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

            //EVIDENCE DOCUMENTATION
            var passResponses = responses.Where(r => r.Answer == ResponseAnswer.Conform && r.Evidences != null && r.Evidences.Any()).ToList();
            var failFindings = findings.Where(f =>
                f.Category != null && (
                    f.Category.Name == DefaultCategories.Major || 
                    f.Category.Name == DefaultCategories.Minor ||
                    f.Category.Name == DefaultCategories.Observation
                )).ToList();

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
                                fc.Item().Text($"Category: {f.Category?.Name ?? "Unknown"}").FontSize(9).FontColor("#555555");
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

    public async Task<PdfReportMetadata> GetOrGeneratePdfMetadata(Guid sessionId, AuditSession? session = null)
    {
        var pdfPath = GetPdfFilePath(sessionId);

        var lockObj = _pdfLocks.GetOrAdd(sessionId, _ => new SemaphoreSlim(1, 1));
        var shouldRegenerate = !File.Exists(pdfPath);

        // Invalidasi cache jika session.UpdateAt lebih baru dari file
        if (!shouldRegenerate && session != null)
        {
            var fileInfo = new FileInfo(pdfPath);
            var sessionLastModified = session.CompletedAt ?? session.StartedAt;
            if (fileInfo.LastWriteTimeUtc < sessionLastModified)
            {
                shouldRegenerate = true;
            }
        }

        if (shouldRegenerate)
            await GenerateAndSavePdfAsync(sessionId, session, pdfPath);

        return await ExtractPdfMetadataAsync(pdfPath);
    }

    private string GetPdfFilePath(Guid sessionId)
    {
        var fileName = $"{FileConstants.AuditReportFilePrefix}{sessionId}{FileConstants.AuditReportFileExtension}";
        var privateFolder = Path.Combine(env.ContentRootPath, "private-reports");
        Directory.CreateDirectory(privateFolder);
        return Path.Combine(privateFolder, fileName);
    }

    private async Task GenerateAndSavePdfAsync(Guid sessionId, AuditSession? session, string pdfPath)
    {
        if (session == null )
        {
            logger.LogError("Cannot generate PDF for session {SessionId}: session object is null", sessionId);
            throw new ArgumentNullException(nameof(session), $"Session object is required to generate PDF for session {sessionId}");
        }

        try
        {
            var pdfBytes = await GenerateAuditReport(session);

            // Write ke temp file dulu, baru rename (atomic operation)
            var tempPath = $"{pdfPath}.tmp";
            await File.WriteAllBytesAsync(tempPath, pdfBytes);

            // File.Move atomic di most filesystems
            File.Move(tempPath, pdfPath, overwrite: true);
            logger.LogInformation("PDF generated succesfully for session {SessionId} at {Path}", sessionId, pdfPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate PDF for session {SessionId}", sessionId);
            throw;
        }
    }

    private async Task<PdfReportMetadata> ExtractPdfMetadataAsync(string pdfPath)
    {
        try
        {
            var fileInfo = new FileInfo(pdfPath);
            var fileSizeBytes = fileInfo.Length;
            var pdfBytes = await File.ReadAllBytesAsync(pdfPath);
            var pageCount = CountPdfPages(pdfBytes);
            return new PdfReportMetadata
            {
                PageCount = pageCount,
                FileSizeBytes = fileSizeBytes,
                FileSizeFormatted = FormatFileSize(fileSizeBytes)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract metadata from PDF at {Path}", pdfPath);
            throw;
        }
    }

    private static int CountPdfPages(byte[] pdfBytes)
    {
        using var docReader = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(1080, 1920));
        return docReader.GetPageCount();
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB"};
        double size = bytes;
        int order = 0;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
