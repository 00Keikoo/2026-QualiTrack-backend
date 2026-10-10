using Microsoft.EntityFrameworkCore;
using QualiTrack.Models;
using QualiTrack.DTOs;
using Amazon.S3.Model;

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
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<SpcUnit> SpcUnits => Set<SpcUnit>();
    public DbSet<IsoStandard> IsoStandards => Set<IsoStandard>();
    public DbSet<AdminActivityLog> AdminActivityLogs => Set<AdminActivityLog>();
    public DbSet<FindingCategory> FindingCategories => Set<FindingCategory>();
    public DbSet<CapaStatus> CapaStatuses => Set<CapaStatus>();
    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();
    

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

        mb.Entity<Finding>().Property(f => f.Status).HasConversion<string>();

        mb.Entity<Finding>()
            .HasOne(f => f.Category)
            .WithMany(c => c.Findings)
            .HasForeignKey(f => f.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<CAPA>()
            .HasOne(c => c.Status)
            .WithMany(s => s.Capas)
            .HasForeignKey(c => c.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Finding>().HasIndex(f => f.Status);
        mb.Entity<Finding>().HasIndex(f => f.CategoryId);
        mb.Entity<CAPA>().HasIndex(c => new { c.StatusId, c.Deadline });
        mb.Entity<CAPA>().HasIndex(c => c.StatusId);

        mb.Entity<AuditSession>().Property(s => s.Status).HasConversion<string>();
        mb.Entity<AuditResponse>().Property(r => r.Answer).HasConversion<string>();
        mb.Entity<AuditSession>().HasIndex(s => s.ScheduleId);
        mb.Entity<AuditSession>().HasIndex(s => new { s.Status, s.CompletedAt });

        mb.Entity<EvidenceFile>()
            .HasOne<Finding>()
            .WithMany(f => f.Evidences)
            .HasForeignKey(e => e.FindingId)
            .IsRequired(false);

        // Simpan data points SPC sebagai JSON
        mb.Entity<SpcAnalysis>()
            .Property(s => s.DataPoints)
            .HasColumnType("jsonb");
        
        mb.Entity<Checklist>()
            .HasOne(c => c.IsoStandard)
            .WithMany(i => i.Checklists)
            .HasForeignKey(c => c.IsoStandardId)
            .IsRequired(false);

        mb.Entity<Checklist>()
            .HasOne(c => c.DepartmentNavigation)
            .WithMany(d => d.Checklists)
            .HasForeignKey(c => c.DepartmentId)
            .IsRequired(false);

        // Department name dan code harus unik
        mb.Entity<Department>().HasIndex(d => d.Name).IsUnique();
        mb.Entity<Department>().HasIndex(d => d.Code).IsUnique();

        // Email harus unik
        mb.Entity<User>().HasIndex(u => u.Email).IsUnique();

        // IsoStandard code harus unik
        mb.Entity<IsoStandard>().HasIndex(i => i.Code).IsUnique();
        mb.Entity<SpcUnit>().HasIndex(s => s.Name).IsUnique();

        mb.Entity<FindingCategory>().HasIndex(c => c.Name).IsUnique();
        mb.Entity<CapaStatus>().HasIndex(s => s.Name).IsUnique();
        mb.Entity<SystemConfig>().HasIndex(c => c.Key).IsUnique();
    }
}
