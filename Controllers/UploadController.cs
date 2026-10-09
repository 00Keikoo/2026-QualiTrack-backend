using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.Models;
using QualiTrack.Services;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/uploads")]
public class UploadController(AppDbContext db, IStorageService storage ) : ControllerBase
{
    private readonly string[] _allowedTypes = ["image/jpeg", "image/png", "image/jpg", "application/pdf"];
    private const int MaxImageWidth = 1280;
    private const int JpegQuality = 75;
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    [HttpPost("finding/{findingId}")]
    public async Task<IActionResult> UploadForFinding(Guid findingId, IFormFile file)
    {
        var finding = await db.Findings.FindAsync(findingId);
        if (finding is null) return NotFound("Finding tidak ditemukan");

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File tidak boleh kosong" });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { message = $"File terlalu besar. Maksimal {MaxFileSizeBytes / (1024 * 1024)}MB" });

        if (!_allowedTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Format file tidak didukung. Gunakan JPG, PNG atau PDF" });

        var key = await storage.UploadFileAsync(file);
        if (key is null) return BadRequest(new { message = "Gagal menyimpan file" });

        var evidence = new EvidenceFile
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            StoragePath = key,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            UploadedAt = DateTime.UtcNow,
            FindingId = findingId
        };

        db.EvidenceFiles.Add(evidence);
        await db.SaveChangesAsync();

        return Ok(new {
            fileId = evidence.Id,
            fileName = evidence.FileName,
            url = storage.GetPresignedUrl(key),
        });
    }

    [HttpPost("capa-action/{actionId}")]
    public async Task<IActionResult> UploadForCapaAction(Guid actionId, IFormFile file)
    {
        var action = await db.CAPAActions.FindAsync(actionId);
        if (action is null) return NotFound("CAPA Action tidak ditemukan");

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File tidak boleh kosong" });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { message = $"File terlalu besar. Maksimal 10MB" });

        if (!_allowedTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Format file tidak didukung. Gunakan JPG, PNG, atau PDF" });

        var key = await storage.UploadFileAsync(file);
        if (key is null) return BadRequest(new { message = "Gagal menyimpan file" });

        var evidence = new EvidenceFile
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            StoragePath = key,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            UploadedAt = DateTime.UtcNow,
            CapaActionId = actionId
        };

        db.EvidenceFiles.Add(evidence);
        await db.SaveChangesAsync();

        return Ok(new {
            fileId = evidence.Id,
            fileName = evidence.FileName,
            url = storage.GetPresignedUrl(key),
            originalSize = file.Length,
            compressedSize = file.Length,
            savedPercent = 0
        });
    }

    [HttpGet("finding/{findingId}")]
    public async Task<IActionResult> GetFindingFiles(Guid findingId)
    {
        var files = await db.EvidenceFiles
            .Where(e => e.FindingId == findingId)
            .Select(e => new
            {
                id = e.Id,
                fileName = e.FileName,
                url = storage.GetPresignedUrl(e.StoragePath),
            })
            .ToListAsync();

        return Ok(files);
    }

    [HttpDelete("{fileId}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid fileId)
    {
        var evidence = await db.EvidenceFiles.FindAsync(fileId);
        if (evidence is null) return NotFound(new { message = "File tidak ditemukan" });

        if (!string.IsNullOrEmpty(evidence.StoragePath))
            await storage.DeleteFileAsync(evidence.StoragePath);

        db.EvidenceFiles.Remove(evidence);
        await db.SaveChangesAsync();

        return Ok(new { message = "File berhasil dihapus", fileId = fileId });
    }

    [HttpGet("file/{fileId}")]
    [Authorize]
    public async Task<IActionResult> GetFileById(Guid fileId)
    {
        var evidence = await db.EvidenceFiles.FindAsync(fileId);
        if(evidence is null) return NotFound(new { message = "File tidak ditemukan"});
        return Ok(new
        {
            fileId = evidence.Id,
            fileName = evidence.FileName,
            url = storage.GetPresignedUrl(evidence.StoragePath),
            ContentType = evidence.ContentType,
            fileSizeBytes = evidence.FileSizeBytes,
            uploadedAt = evidence.UploadedAt
        });
    }

    // POST /api/uploads/audit-response/{responseId}
    [HttpPost("audit-response/{responseId}")]
    [Authorize]
    public async Task<IActionResult> UploadForAuditResponse(Guid responseId, IFormFile file)
    {
        var response = await db.AuditResponses.FindAsync(responseId);
        if (response is null) return NotFound(new { message = "Response tidak ditemukan" });

        if (response.Answer != ResponseAnswer.Conform)
            return BadRequest(new { message = "Upload evidence hanya untuk jawaban PASS" });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File tidak boleh kosong" });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { message = $"File terlalu besar. Maksimal {MaxFileSizeBytes / (1024 * 1024)}MB" });

        if (!_allowedTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Format file tidak didukung. Gunakan JPG, PNG, atau PDF" });

        var key = await storage.UploadFileAsync(file);
        if (key is null) return BadRequest(new { message = "Gagal menyimpan file" });

        var evidence = new EvidenceFile
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            StoragePath = key,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            UploadedAt = DateTime.UtcNow,
            AuditResponseId = responseId
        };

        db.EvidenceFiles.Add(evidence);
        await db.SaveChangesAsync();

        return Ok(new
        {
            fileId = evidence.Id,
            fileName = evidence.FileName,
            url = storage.GetPresignedUrl(key)
        });
    }

    // GET /api/uploads/audit-response/{responseId}
    [HttpGet("audit-response/{responseId}")]
    [Authorize]
    public async Task<IActionResult> GetAuditResponseFiles(Guid responseId)
    {
        var files = await db.EvidenceFiles
            .Where(e => e.AuditResponseId == responseId)
            .Select(e => new
            {
                id = e.Id,
                fileName = e.FileName,
                url = storage.GetPresignedUrl(e.StoragePath)
            })
            .ToListAsync();

        return Ok(files);
    }
}
