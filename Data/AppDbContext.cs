using Microsoft.EntityFrameworkCore;
using QualiTrack.Models;
using QualiTrack.DTOs;

namespace QualiTrack.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    static AppDbContext()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
    public DbSet<AuditPlan> AuditPlans => Set<AuditPlan>();
    public DbSet<AuditSchedule> AuditSchedules => Set<AuditSchedule>();
    public DbSet<Checklist> Checklists => Set<Checklist>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<AuditSession> AuditSessions => Set<AuditSession>();
    public DbSet<AuditResponse> AuditResponses => Set<AuditResponse>();
    public DbSet<Finding> Findings => Set<Finding>();
    public DbSet<CAPA> CAPAs => Set<CAPA>();
    public DbSet<CAPAAction> CAPAActions => Set<CAPAAction>();
    public DbSet<CloseOutVerification> CloseOutVerifications => Set<CloseOutVerification>();
    public DbSet<EvidenceFile> EvidenceFiles => Set<EvidenceFile>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditSummary> AuditSummaries => Set<AuditSummary>();
    public DbSet<SpcAnalysis> SpcAnalyses => Set<SpcAnalysis>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Finding>()
            .HasOne(f => f.Capa)
            .WithOne(c => c.Finding)
            .HasForeignKey<CAPA>(c => c.FindingId);

        mb.Entity<CAPA>()
            .HasOne(c => c.CloseOut)
            .WithOne(v => v.Capa)
            .HasForeignKey<CloseOutVerification>(v => v.CapaId);

        mb.Entity<Finding>().Property(f => f.Category).HasConversion<string>();
        mb.Entity<Finding>().Property(f => f.Status).HasConversion<string>();
        mb.Entity<CAPA>().Property(c => c.Status).HasConversion<string>();
        mb.Entity<AuditSession>().Property(s => s.Status).HasConversion<string>();
        mb.Entity<AuditResponse>().Property(r => r.Answer).HasConversion<string>();

        mb.Entity<Finding>().HasIndex(f => f.Status);
        mb.Entity<CAPA>().HasIndex(c => new { c.Status, c.Deadline });
        mb.Entity<AuditSession>().HasIndex(s => s.ScheduleId);
        mb.Entity<AuditSession>().HasIndex(s => new { s.Status, s.CompletedAt });
        mb.Entity<EvidenceFile>()
            .HasOne<Finding>()
            .WithMany(f => f.Evidences)
            .HasForeignKey(e => e.FindingId)
            .IsRequired(false);

        // Email harus unik
        mb.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}
