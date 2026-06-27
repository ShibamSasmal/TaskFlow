using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.API.Models
{
    public class ResumeAnalysisResult
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string TargetRole { get; set; } = string.Empty;

        public int OverallScore { get; set; }

        [Required]
        [MaxLength(2)]
        public string ScoreGrade { get; set; } = string.Empty;

        [Required]
        public string DetectedSkillsJson { get; set; } = "[]";

        [Required]
        public string MissingSkillsJson { get; set; } = "[]";

        [Required]
        public string ScoreBreakdownJson { get; set; } = "{}";

        [Required]
        public string SuggestionsJson { get; set; } = "[]";

        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        // Relation to AppUser (if logged in, nullable)
        public Guid? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public AppUser? User { get; set; }
    }
}
