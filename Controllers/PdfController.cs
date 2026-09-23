using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.Services;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PdfController(AppDbContext db, PdfReportService pdfService, IStorageService storage) : ControllerBase
{
    // Masa berlaku link share (maksimal presigned URL S3 = 7 hari)
    private const int ShareLinkExpiryHours = 24 * 7;

    [HttpGet("audit-report/{sessionId}")]
    public async Task<IActionResult> GenerateAuditReport(Guid sessionId)
    {
        var pdf = await BuildAuditReport(sessionId);
        if (pdf is null)
            return NotFound(new { message = "Sesi audit tidak ditemukan" });

        return File(pdf, "application/pdf", $"audit-report-{sessionId}.pdf");
    }

    // GET /api/Pdf/audit-report/{sessionId}/link
    // Generate PDF, simpan ke storage, lalu kembalikan URL untuk view / download / share
    [HttpGet("audit-report/{sessionId}/link")]
    public async Task<IActionResult> GenerateAuditReportLink(Guid sessionId)
    {
        var pdf = await BuildAuditReport(sessionId);
        if (pdf is null)
            return NotFound(new { message = "Sesi audit tidak ditemukan" });

        using var stream = new MemoryStream(pdf);
        var file = new FormFile(stream, 0, pdf.Length, "file", $"audit-report-{sessionId}.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        var key = await storage.UploadFileAsync(file);
        var pdfUrl = storage.GetPresignedUrl(key, ShareLinkExpiryHours);

        return Ok(new { pdfUrl });
    }

    private async Task<byte[]?> BuildAuditReport(Guid sessionId)
    {
        var session = await db.AuditSessions
            .Include(s => s.Schedule)
                .ThenInclude(s => s.AuditPlan)
            .Include(s => s.Schedule)
                .ThenInclude(s => s.Auditor)
            .Include(s => s.Findings)
                .ThenInclude(f => f.Capa)
                    .ThenInclude(c => c!.Pic)
            .Include(s => s.Findings)
                .ThenInclude(f => f.Evidences)
            .Include(s => s.Responses)
                .ThenInclude(r => r.ChecklistItem)
            .Include(s => s.Responses)
                .ThenInclude(r => r.Evidences)
            .Include(s => s.Summary)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session is null)
            return null;

        return await pdfService.GenerateAuditReport(session);
    }
}
