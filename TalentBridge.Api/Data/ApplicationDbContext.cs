using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TalentBridge.Api.Models;

namespace TalentBridge.Api.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<RecruiterProfile> RecruiterProfiles => Set<RecruiterProfile>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobSkill> JobSkills => Set<JobSkill>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<ApplicationTimeline> ApplicationTimelines => Set<ApplicationTimeline>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── ApplicationUser ────────────────────────────────────────────
        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            e.Property(u => u.Role).HasMaxLength(20).IsRequired();
        });

        // ── CandidateProfile ───────────────────────────────────────────
        builder.Entity<CandidateProfile>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.User)
                .WithOne(u => u.CandidateProfile)
                .HasForeignKey<CandidateProfile>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(c => c.UserId).IsUnique();
            e.Property(c => c.FullName).HasMaxLength(200).IsRequired();
            e.Property(c => c.Headline).HasMaxLength(300);
            e.Property(c => c.Location).HasMaxLength(200);
        });

        // ── RecruiterProfile ───────────────────────────────────────────
        builder.Entity<RecruiterProfile>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.User)
                .WithOne(u => u.RecruiterProfile)
                .HasForeignKey<RecruiterProfile>(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(r => r.UserId).IsUnique();
            e.Property(r => r.FullName).HasMaxLength(200).IsRequired();
            e.Property(r => r.CompanyName).HasMaxLength(300).IsRequired();
        });

        // ── Skill ──────────────────────────────────────────────────────
        builder.Entity<Skill>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => s.Name).IsUnique();
            e.Property(s => s.Name).HasMaxLength(100).IsRequired();
            e.Property(s => s.Category).HasMaxLength(100).IsRequired();
        });

        // ── CandidateSkill (composite PK) ──────────────────────────────
        builder.Entity<CandidateSkill>(e =>
        {
            e.HasKey(cs => new { cs.CandidateProfileId, cs.SkillId });
            e.HasOne(cs => cs.CandidateProfile)
                .WithMany(c => c.CandidateSkills)
                .HasForeignKey(cs => cs.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cs => cs.Skill)
                .WithMany(s => s.CandidateSkills)
                .HasForeignKey(cs => cs.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Job ────────────────────────────────────────────────────────
        builder.Entity<Job>(e =>
        {
            e.HasKey(j => j.Id);
            e.HasOne(j => j.RecruiterProfile)
                .WithMany(r => r.Jobs)
                .HasForeignKey(j => j.RecruiterProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(j => j.Title).HasMaxLength(300).IsRequired();
            e.Property(j => j.Location).HasMaxLength(200).IsRequired();
            e.Property(j => j.SalaryMin).HasColumnType("decimal(18,2)");
            e.Property(j => j.SalaryMax).HasColumnType("decimal(18,2)");
            e.Property(j => j.JobType).HasConversion<string>();
            e.Property(j => j.Status).HasConversion<string>();
        });

        // ── JobSkill (composite PK) ────────────────────────────────────
        builder.Entity<JobSkill>(e =>
        {
            e.HasKey(js => new { js.JobId, js.SkillId });
            e.HasOne(js => js.Job)
                .WithMany(j => j.JobSkills)
                .HasForeignKey(js => js.JobId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(js => js.Skill)
                .WithMany(s => s.JobSkills)
                .HasForeignKey(js => js.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── JobApplication ─────────────────────────────────────────────
        builder.Entity<JobApplication>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.CandidateProfile)
                .WithMany(c => c.Applications)
                .HasForeignKey(a => a.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Restrict); // don't wipe applications if job deleted
            // A candidate can only apply once per job
            e.HasIndex(a => new { a.CandidateProfileId, a.JobId }).IsUnique();
            e.Property(a => a.Status).HasConversion<string>();
            e.Property(a => a.MatchScore).HasDefaultValue(0.0);
        });

        // ── ApplicationTimeline ────────────────────────────────────────
        builder.Entity<ApplicationTimeline>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasOne(t => t.Application)
                .WithMany(a => a.Timelines)
                .HasForeignKey(t => t.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(t => t.Status).HasConversion<string>();
            e.Property(t => t.Notes).HasMaxLength(1000);
        });

        // ── Notification ───────────────────────────────────────────────
        builder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(n => n.Title).HasMaxLength(300).IsRequired();
            e.Property(n => n.Message).HasMaxLength(2000).IsRequired();
            e.Property(n => n.Type).HasConversion<string>();
        });
    }
}
