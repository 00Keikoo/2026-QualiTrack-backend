using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.Models;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly AppDbContext _db;

    public UserController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/users?role=QualityManager
    [HttpGet]
    [Authorize(Roles = "Admin,QualityManager")]
    public async Task<IActionResult> GetUsers([FromQuery] string? role)
    {
        var query = _db.Users
            .Where(u => u.EmailVerified == true)
            .AsQueryable();

        if (!string.IsNullOrEmpty(role))
        {
            var normalizedRole = role == UserRoles.AuditorInternal ?
              UserRoles.AuditorInternal : role;
            if (normalizedRole == UserRoles.AuditorInternal)
            {
                query = query.Where(u => u.Role == UserRoles.AuditorInternal);
            }
            else
            {
                query = query.Where(u => u.Role == normalizedRole);
            }
        }

        var users = await query
            .Select(u => new { u.Id, u.FullName, u.Email, u.Role, u.Status })
            .ToListAsync();

        return Ok(new { message = "Daftar user berhasil diambil", total = users.Count, data = users });
    }

    // GET /api/users/auditors
    [HttpGet("auditors")]
    [Authorize(Roles = "Admin,QualityManager")]
    public async Task<IActionResult> GetAuditors()
    {
        var auditors = await _db.Users
            .Where(u => u.EmailVerified == true && (u.Role == UserRoles.AuditorInternal ||
                u.Role == UserRoles.QualityManager))
            .Select(u => new { u.Id, u.FullName, u.Role })
            .ToListAsync();

        return Ok(new
        {
            message = "Daftar AuditorInternal berhasil diambil",
            total = auditors.Count,
            data = auditors
        });
    }

    // GET /api/users/pic-candidates
    [HttpGet("pic-candidates")]
    [Authorize(Roles = "Admin,QualityManager,Auditor")]
    public async Task<IActionResult> GetPicCandidates()
    {
        var pics = await _db.Users
            .Where(u => u.EmailVerified == true && u.Status == "Active" && u.Role == "Auditee")
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync();

        return Ok(new { data = pics });
    }
}
