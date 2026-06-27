using System;
using System.Collections.Generic;

namespace TaskManager.API.DTOs.Resume
{
    public class AnalysisResponseDto
    {
        public Guid AnalysisId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string TargetRole { get; set; } = string.Empty;
        public int OverallScore { get; set; }
        public string ScoreGrade { get; set; } = string.Empty;
        public List<DetectedSkillDto> DetectedSkills { get; set; } = new();
        public List<MissingSkillDto> MissingSkills { get; set; } = new();
        public ScoreBreakdownDto ScoreBreakdown { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
    }

    public class DetectedSkillDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public float Confidence { get; set; }
    }

    public class MissingSkillDto
    {
        public string Name { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty; // "Required" | "Preferred"
        public string Reason { get; set; } = string.Empty;
        public string ResourceUrl { get; set; } = string.Empty;
    }

    public class ScoreBreakdownDto
    {
        public int SkillCoverage { get; set; }
        public int PreferredSkills { get; set; }
        public int ResumeStructure { get; set; }
        public int ContentQuality { get; set; }
        public int FormatCompliance { get; set; }
    }
}
