using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcelDataReader;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Models;
using System.Text;
using System.Security.Claims;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SpcController(AppDbContext db) : ControllerBase
{
    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze(
        IFormFile file,
        [FromForm] double lsl,
        [FromForm] double usl,
        [FromForm] string productName,
        [FromForm] string? description)
    {
        //Validasi file
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "File tidak boleh kosong" });

        var allowedExtensions = new[] { ".xlsx", ".xls" };
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(ext))
            return BadRequest(new { message = "File harus berformat .xlsx atau .xls" });

        //Validasi LSL dan USL
        if (lsl >= usl)
            return BadRequest(new { message = "LSL harus lebih kecil dari USL" });

        if (string.IsNullOrEmpty(productName))
            return BadRequest(new { message = "ProductName wajib diisi" });

        //Parse data dari Excel
        List<double> data;
        try
        {
            data = await ParseExcelAsync(file);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Gagal membaca file Excel: {ex.Message}" });
        }

        if (data.Count < 2)
            return BadRequest(new { message = "Data minimal 2 nilai untuk analisis SPC" });

        //Hitung Mean
        var mean = data.Average();

        //Hitung Standard Deviation (Sample SD — dibagi n-1)
        var variance = data.Sum(x => Math.Pow(x - mean, 2)) / (data.Count - 1);
        var stdDev = Math.Sqrt(variance);

        //Hitung UCL dan LCL (Mean ± 3*StdDev)
        var ucl = mean + 3 * stdDev;
        var lcl = mean - 3 * stdDev;

        //Hitung Cp
        var cp = (usl - lsl) / (6 * stdDev);

        //Hitung Cpk
        var cpkUpper = (usl - mean) / (3 * stdDev);
        var cpkLower = (mean - lsl) / (3 * stdDev);
        var cpk = Math.Min(cpkUpper, cpkLower);

        var isUnstable = data.Any(x => x >ucl || x < lcl);
        var outOfControlPoints = data.Where(x => x > ucl || x < lcl).ToList();

        //Tentukan status
        var status = isUnstable ? "Process Unstable"
            : cpk < 1.00 ? "Not Capable"
            : cpk < 1.33 ? "Marginal"
            : "Process Capable";

        //Simpan ke database
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var analysis = new SpcAnalysis
        {
            Id = Guid.NewGuid(),
            ProductName = productName,
            Description = description,
            Lsl = lsl,
            Usl = usl,
            Mean = Math.Round(mean, 4),
            StandardDeviation = Math.Round(stdDev, 4),
            Ucl = Math.Round(ucl, 4),
            Lcl = Math.Round(lcl, 4),
            Cp = Math.Round(cp, 4),
            Cpk = Math.Round(cpk, 4),
            Status = status,
            IsStable = !isUnstable,
            DataCount = data.Count,
            AnalyzedAt = DateTime.UtcNow,
            AnalyzedById = userId
        };

        db.SpcAnalyses.Add(analysis);
        await db.SaveChangesAsync();

        return Ok(new SpcResultDto
        {
            Id = analysis.Id,
            ProductName = analysis.ProductName,
            Description = analysis.Description,
            Mean = analysis.Mean,
            StandardDeviation = analysis.StandardDeviation,
            Ucl = analysis.Ucl,
            Lcl = analysis.Lcl,
            Lsl = analysis.Lsl,
            Usl = analysis.Usl,
            Cp = analysis.Cp,
            Cpk = analysis.Cpk,
            Status = analysis.Status,
            IsStable = analysis.IsStable,
            DataCount = analysis.DataCount,
            AnalyzedAt = analysis.AnalyzedAt,
            Data = data.Select(d => Math.Round(d, 4)).ToList()
        });
    }

    //GET /api/Spc/history?period=3m&productName=xxx
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string period = "3m",
        [FromQuery] string? productName = null,
        [FromQuery] string? status = null)
    {
        var now = DateTime.UtcNow;
        var startDate = period switch
        {
            "3m" => now.AddMonths(-3),
            "6m" => now.AddMonths(-6),
            "1y" => now.AddYears(-1),
            "all" => DateTime.MinValue,
            _ => now.AddMonths(-3)
        };

        var query = db.SpcAnalyses
            .Where(s => s.AnalyzedAt >= startDate)
            .AsQueryable();

        if (!string.IsNullOrEmpty(productName))
            query = query.Where(s => s.ProductName.Contains(productName));

        if (!string.IsNullOrEmpty(status))
            query = query.Where(s => s.Status == status);

        var data = await query
            .OrderByDescending(s => s.AnalyzedAt)
            .Select(s => new SpcHistoryDto
            {
                Id = s.Id,
                ProductName = s.ProductName,
                Description = s.Description,
                Cp = s.Cp,
                Cpk = s.Cpk,
                Status = s.Status,
                IsStable = s.IsStable,
                AnalyzedAt = s.AnalyzedAt
            })
            .ToListAsync();

        return Ok(new
        {
            period,
            startDate,
            endDate = now,
            total = data.Count,
            data
        });
    }

    //GET /api/Spc/{id} — detail satu analisis
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var analysis = await db.SpcAnalyses.FindAsync(id);
        if (analysis is null)
            return NotFound(new { message = "Analisis SPC tidak ditemukan" });

        return Ok(new SpcResultDto
        {
            Id = analysis.Id,
            ProductName = analysis.ProductName,
            Description = analysis.Description,
            Mean = analysis.Mean,
            StandardDeviation = analysis.StandardDeviation,
            Ucl = analysis.Ucl,
            Lcl = analysis.Lcl,
            Lsl = analysis.Lsl,
            Usl = analysis.Usl,
            Cp = analysis.Cp,
            Cpk = analysis.Cpk,
            Status = analysis.Status,
            DataCount = analysis.DataCount,
            AnalyzedAt = analysis.AnalyzedAt,
            IsStable = analysis.IsStable,
            Data = []  // Data raw tidak disimpan di DB
        });
    }

    private async Task<List<double>> ParseExcelAsync(IFormFile file)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var data = new List<double>();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var reader = ExcelReaderFactory.CreateReader(stream);
        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = true
            }
        });

        if (dataSet.Tables.Count == 0)
            throw new Exception("File Excel kosong");

        var table = dataSet.Tables[0];

        foreach (System.Data.DataRow row in table.Rows)
        {
            var cell = row[0];
            if (cell == null || cell == DBNull.Value) continue;

            if (double.TryParse(cell.ToString(), out var value))
                data.Add(value);
        }

        if (!data.Any())
            throw new Exception("Tidak ada data numerik ditemukan di kolom pertama");

        return data;
    }
}