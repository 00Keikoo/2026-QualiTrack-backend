using Microsoft.EntityFrameworkCore;
using QualiTrack.Models;

namespace QualiTrack.Data;

public static class CompiledQueries
{
    public static readonly Func<AppDbContext, Guid, Task<User?>> GetUserById =
        EF.CompileAsyncQuery((AppDbContext db, Guid id) =>
            db.Users.FirstOrDefault(u => u.Id == id));

    public static readonly Func<AppDbContext, string, Task<User?>> GetUserByEmail =
        EF.CompileAsyncQuery((AppDbContext db, string email) =>
            db.Users.FirstOrDefault(u => u.Email == email));

    public static readonly Func<AppDbContext, Guid, Task<CAPA?>> GetCapaWithDetails =
        EF.CompileAsyncQuery((AppDbContext db, Guid id) =>
            db.CAPAs
                .Include(c => c.Actions)
                    .ThenInclude(a => a.DoneBy)
                .Include(c => c.CloseOut)
                    .ThenInclude(co => co!.VerifiedBy)
                .Include(c => c.Pic)
                .Include(c => c.Status)
                .Include(c => c.Finding!)
                    .ThenInclude(f => f.Category!)
                .FirstOrDefault(c => c.Id == id));

    public static readonly Func<AppDbContext, string, bool, Task<CapaStatus?>> GetCapaStatusByName =
        EF.CompileAsyncQuery((AppDbContext db, string name, bool isActive) =>
            db.CapaStatuses.FirstOrDefault(s => s.Name == name && s.IsActive == isActive));

    public static readonly Func<AppDbContext, Guid, Task<Finding?>> GetFindingById =
        EF.CompileAsyncQuery((AppDbContext db, Guid id) =>
            db.Findings.FirstOrDefault(f => f.Id == id));

    public static readonly Func<AppDbContext, Task<List<SpcUnit>>> GetActiveSpcUnits =
        EF.CompileAsyncQuery((AppDbContext db) =>
            db.SpcUnits
                .Where(u => u.IsActive)
                .OrderBy(u => u.Name)
                .ToList());

    public static readonly Func<AppDbContext, Task<List<IsoStandard>>> GetActiveIsoStandards =
        EF.CompileAsyncQuery((AppDbContext db) =>
            db.IsoStandards
                .Where(i => i.IsActive)
                .OrderByDescending(i => i.CreatedAt)
                .ToList());
}
