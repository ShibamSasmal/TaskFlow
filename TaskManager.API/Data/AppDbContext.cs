using Microsoft.EntityFrameworkCore;
using TaskManager.API.Models;

namespace TaskManager.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; } = null!;
        public DbSet<TaskItem> Tasks { get; set; } = null!;
        public DbSet<SkillCategory> SkillCategories { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<RoleTemplate> RoleTemplates { get; set; } = null!;
        public DbSet<RoleSkill> RoleSkills { get; set; } = null!;
        public DbSet<ResumeAnalysisResult> ResumeAnalysisResults { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Entity Configurations
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Task Entity Configurations
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Priority).HasConversion<int>().IsRequired();
                entity.Property(t => t.Status).HasConversion<int>().IsRequired();
                entity.Property(t => t.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relationships
                entity.HasOne(t => t.User)
                      .WithMany(u => u.Tasks)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SkillCategory Configuration
            modelBuilder.Entity<SkillCategory>(entity =>
            {
                entity.HasKey(sc => sc.Id);
                entity.Property(sc => sc.Name).IsRequired().HasMaxLength(100);
            });

            // Skill Configuration
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
                entity.HasOne(s => s.Category)
                      .WithMany(c => c.Skills)
                      .HasForeignKey(s => s.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // RoleTemplate Configuration
            modelBuilder.Entity<RoleTemplate>(entity =>
            {
                entity.HasKey(rt => rt.Id);
                entity.Property(rt => rt.RoleName).IsRequired().HasMaxLength(100);
                entity.HasIndex(rt => rt.RoleName).IsUnique();
            });

            // RoleSkill Configuration (Composite Key)
            modelBuilder.Entity<RoleSkill>(entity =>
            {
                entity.HasKey(rs => new { rs.RoleTemplateId, rs.SkillId });

                entity.HasOne(rs => rs.RoleTemplate)
                      .WithMany(rt => rt.RoleSkills)
                      .HasForeignKey(rs => rs.RoleTemplateId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rs => rs.Skill)
                      .WithMany()
                      .HasForeignKey(rs => rs.SkillId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ResumeAnalysisResult Configuration
            modelBuilder.Entity<ResumeAnalysisResult>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.FileName).IsRequired().HasMaxLength(255);
                entity.Property(r => r.TargetRole).IsRequired().HasMaxLength(100);
                entity.Property(r => r.ScoreGrade).IsRequired().HasMaxLength(2);
                entity.Property(r => r.DetectedSkillsJson).IsRequired().HasColumnType("jsonb");
                entity.Property(r => r.MissingSkillsJson).IsRequired().HasColumnType("jsonb");
                entity.Property(r => r.ScoreBreakdownJson).IsRequired().HasColumnType("jsonb");
                entity.Property(r => r.SuggestionsJson).IsRequired().HasColumnType("jsonb");

                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
