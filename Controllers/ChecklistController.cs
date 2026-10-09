using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.DTOs;
using QualiTrack.Filters;
using QualiTrack.Models;

namespace QualiTrack.Controllers;

[ApiController]
[Route("api/checklists")]
[Authorize]
[ValidateModel]
public class ChecklistController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal")]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? isoStandardId,
        [FromQuery] Guid? departmentId,
        [FromQuery] bool? isSystemTemplate)
    {
        var query = db.Checklists
            .AsNoTracking()
            .Include(c => c.IsoStandard)
            .Include(c => c.DepartmentNavigation)
            .Include(c => c.Items)
            .AsQueryable();

        if (isoStandardId.HasValue)
            query = query.Where(c => c.IsoStandardId == isoStandardId.Value);

        if (departmentId.HasValue)
            query = query.Where(c => c.DepartmentId == departmentId.Value);

        if (isSystemTemplate.HasValue)
            query = query.Where(c => c.IsSystemTemplate == isSystemTemplate.Value);

        var templates = await query
            .OrderBy(c => c.Title)
            .Select(c => new ChecklistTemplateListDto
            {
                Id = c.Id,
                Title = c.Title,
                IsoStandardId = c.IsoStandardId,
                IsoStandardName = c.IsoStandard != null ? c.IsoStandard.Name : c.Standard,
                DepartmentId = c.DepartmentId,
                DepartmentName = c.DepartmentNavigation != null ? c.DepartmentNavigation.Name : c.Department,
                IsSystemTemplate = c.IsSystemTemplate,
                ItemCount = c.Items.Count,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(templates);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,QualityManager,AuditorInternal")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var template = await db.Checklists
            .AsNoTracking()
            .Include(c => c.IsoStandard)
            .Include(c => c.DepartmentNavigation)
            .Include(c => c.Items)
            .Where(c => c.Id == id)
            .Select(c => new ChecklistTemplateDetailDto
            {
                Id = c.Id,
                Title = c.Title,
                IsoStandardId = c.IsoStandardId,
                IsoStandardName = c.IsoStandard != null ? c.IsoStandard.Name : c.Standard,
                DepartmentId = c.DepartmentId,
                DepartmentName = c.DepartmentNavigation != null ? c.DepartmentNavigation.Name : c.Department,
                IsSystemTemplate = c.IsSystemTemplate,
                Items = c.Items.OrderBy(i => i.OrderIndex).Select(i => new ChecklistItemDto
                {
                    Id = i.Id,
                    Question = i.Question,
                    Description = i.Description,
                    ClauseRef = i.ClauseRef,
                    OrderIndex = i.OrderIndex
                }).ToList(),
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (template is null)
            return NotFound(new { message = "Checklist template tidak ditemukan" });

        return Ok(template);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateChecklistTemplateDto dto)
    {
        var isoStandardExists = await db.IsoStandards
            .AnyAsync(iso => iso.Id == dto.IsoStandardId && iso.IsActive);

        if (!isoStandardExists)
            return BadRequest(new { message = "ISO Standard tidak ditemukan atau tidak aktif" });

        var departmentExists = await db.Departments
            .AnyAsync(d => d.Id == dto.DepartmentId && d.IsActive);

        if (!departmentExists)
            return BadRequest(new { message = "Department tidak ditemukan atau tidak aktif" });

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var isoStandard = await db.IsoStandards.FindAsync(dto.IsoStandardId);
            var department = await db.Departments.FindAsync(dto.DepartmentId);

            var checklist = new Checklist
            {
                Title = dto.Title.Trim(),
                IsoStandardId = dto.IsoStandardId,
                Standard = isoStandard!.Code,
                DepartmentId = dto.DepartmentId,
                Department = department!.Name,
                IsSystemTemplate = false,
                CreatedAt = DateTime.UtcNow
            };

            db.Checklists.Add(checklist);
            await db.SaveChangesAsync();

            if (dto.Items.Count > 0)
            {
                foreach (var itemDto in dto.Items)
                {
                    var item = new ChecklistItem
                    {
                        ChecklistId = checklist.Id,
                        Question = itemDto.Question.Trim(),
                        Description = string.IsNullOrWhiteSpace(itemDto.Description)
                            ? null
                            : itemDto.Description.Trim(),
                        ClauseRef = itemDto.ClauseRef.Trim(),
                        OrderIndex = itemDto.OrderIndex
                    };
                    db.ChecklistItems.Add(item);
                }
                await db.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = checklist.Id },
                new { id = checklist.Id, message = "Checklist template berhasil dibuat" }
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChecklistDto dto)
    {
        var checklist = await db.Checklists
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (checklist is null)
            return NotFound(new { message = "Checklist template tidak ditemukan" });

        if (dto.Items.Count == 0)
            return BadRequest(new { message = "Template harus memiliki minimal 1 item" });

        var keptIds = dto.Items
            .Where(i => i.Id.HasValue)
            .Select(i => i.Id!.Value)
            .ToHashSet();
        var removedItems = checklist.Items
            .Where(i => !keptIds.Contains(i.Id))
            .ToList();

        if (removedItems.Count > 0)
        {
            var removedIds = removedItems.Select(i => i.Id).ToList();
            var usedInAudit = await db.AuditResponses
                .AnyAsync(r => removedIds.Contains(r.ChecklistItemId));

            var usedInFinding = await db.Findings
                .AnyAsync(f => f.ChecklistItemId.HasValue && removedIds.Contains(f.ChecklistItemId.Value));

            if (usedInAudit || usedInFinding)
            {
                var usedQuestions = removedItems.Select(i => $"\"{i.Question}\"");
                return BadRequest(new
                {
                    message = $"Item berikut sudah digunakan dalam audit dan tidak dapat dihapus:{string.Join(", ", usedQuestions)}"
                });
            }
            db.ChecklistItems.RemoveRange(removedItems);
        }

        checklist.Title = dto.Title.Trim();
        checklist.Standard = dto.Standard.Trim();
        checklist.Department = dto.Department.Trim();

        for (var index = 0; index < dto.Items.Count; index++)
        {
            var itemDto = dto.Items[index];
            var existing = itemDto.Id.HasValue
                ? checklist.Items.FirstOrDefault(i => i.Id == itemDto.Id.Value)
                : null;

            if (itemDto.Id.HasValue && existing is null)
                return BadRequest(new { message = $"Item {itemDto.Id} bukan bagian dari checklist ini" });

            if (existing is null)
            {
                db.ChecklistItems.Add(new ChecklistItem
                {
                    ChecklistId = checklist.Id,
                    Question = itemDto.Question.Trim(),
                    Description = string.IsNullOrWhiteSpace(itemDto.Description)
                      ? null
                      : itemDto.Description.Trim(),
                    ClauseRef = itemDto.ClauseRef.Trim(),
                    OrderIndex = index + 1
                });
            }
            else
            {
                existing.Question = itemDto.Question.Trim();
                existing.Description = string.IsNullOrWhiteSpace(itemDto.Description)
                    ? null
                    : itemDto.Description.Trim();
                existing.ClauseRef = itemDto.ClauseRef.Trim();
                existing.OrderIndex = index + 1;
            }
        }

        await db.SaveChangesAsync();
        return await GetById(id);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var checklist = await db.Checklists.FindAsync(id);
        if (checklist is null)
            return NotFound(new { message = "Checklist template tidak ditemukan" });

        db.Checklists.Remove(checklist);
        await db.SaveChangesAsync();

        return Ok(new { message = "Checklist template berhasil dihapus" });
    }

    [HttpGet("{id:guid}/items")]
    public async Task<IActionResult> GetItems(Guid id)
    {
        var checklist = await db.Checklists
            .AsNoTracking()
            .Include(c => c.Items.OrderBy(i => i.OrderIndex))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (checklist is null)
            return NotFound(new { message = "Checklist template tidak ditemukan" });

        return Ok(new
        {
            checklistId = checklist.Id,
            title = checklist.Title,
            isSystemTemplate = checklist.IsSystemTemplate,
            totalItems = checklist.Items.Count,
            items = checklist.Items.Select(i => new
            {
                i.Id,
                i.Question,
                i.Description,
                i.ClauseRef,
                i.OrderIndex
            })
        });
    }
}
