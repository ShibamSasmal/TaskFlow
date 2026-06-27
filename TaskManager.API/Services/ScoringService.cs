using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs.Resume;
using TaskManager.API.Services.Interfaces;

namespace TaskManager.API.Services
{
    public class ScoringService : IScoringService
    {
        private readonly AppDbContext _context;

        public ScoringService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(int OverallScore, string Grade, ScoreBreakdownDto Breakdown)> ComputeScoreAsync(
            List<string> detectedSkillNames, 
            string targetRole, 
            string resumeText)
        {
            var breakdown = new ScoreBreakdownDto();

            // 1. Fetch skills mapped to this target role from DB
            var roleTemplate = await _context.RoleTemplates
                .Include(rt => rt.RoleSkills)
                .ThenInclude(rs => rs.Skill)
                .FirstOrDefaultAsync(rt => rt.RoleName.ToLower() == targetRole.ToLower());

            // A. Skill Coverage (40%) and Preferred Skills (20%)
            if (roleTemplate != null)
            {
                var requiredSkills = roleTemplate.RoleSkills
                    .Where(rs => rs.IsRequired)
                    .Select(rs => rs.Skill.Name)
                    .ToList();

                var preferredSkills = roleTemplate.RoleSkills
                    .Where(rs => !rs.IsRequired)
                    .Select(rs => rs.Skill.Name)
                    .ToList();

                // Compute required skills coverage (out of 40 points)
                if (requiredSkills.Count > 0)
                {
                    int matchedRequiredCount = requiredSkills
                        .Count(req => detectedSkillNames.Contains(req, StringComparer.OrdinalIgnoreCase));
                    breakdown.SkillCoverage = (int)Math.Round((double)matchedRequiredCount / requiredSkills.Count * 40);
                }
                else
                {
                    breakdown.SkillCoverage = 40; // Default if none required
                }

                // Compute preferred skills coverage (out of 20 points)
                if (preferredSkills.Count > 0)
                {
                    int matchedPreferredCount = preferredSkills
                        .Count(pref => detectedSkillNames.Contains(pref, StringComparer.OrdinalIgnoreCase));
                    breakdown.PreferredSkills = (int)Math.Round((double)matchedPreferredCount / preferredSkills.Count * 20);
                }
                else
                {
                    breakdown.PreferredSkills = 20; // Default if none preferred
                }
            }
            else
            {
                // Fallback score if no template found
                breakdown.SkillCoverage = 30;
                breakdown.PreferredSkills = 15;
            }

            // B. Resume Structure (20%)
            // Check presence of key sections
            int structureScore = 0;
            string lowerText = resumeText.ToLower();

            bool hasContact = lowerText.Contains("contact") || lowerText.Contains("email") || lowerText.Contains("phone") || lowerText.Contains("address") || lowerText.Contains("github") || lowerText.Contains("linkedin");
            bool hasSummary = lowerText.Contains("summary") || lowerText.Contains("profile") || lowerText.Contains("objective") || lowerText.Contains("about me");
            bool hasExperience = lowerText.Contains("experience") || lowerText.Contains("work") || lowerText.Contains("employment") || lowerText.Contains("history") || lowerText.Contains("projects");
            bool hasEducation = lowerText.Contains("education") || lowerText.Contains("degree") || lowerText.Contains("university") || lowerText.Contains("college") || lowerText.Contains("academic");

            if (hasContact) structureScore += 5;
            if (hasSummary) structureScore += 5;
            if (hasExperience) structureScore += 5;
            if (hasEducation) structureScore += 5;
            breakdown.ResumeStructure = structureScore;

            // C. Content Quality (10%)
            int qualityScore = 0;
            // Check for action verbs: led, developed, managed, implemented, created, designed, achieved, optimized
            var actionVerbs = new[] { "led", "developed", "managed", "implemented", "created", "designed", "achieved", "optimized", "delivered", "coordinated", "analyzed" };
            int actionVerbCount = actionVerbs.Count(verb => Regex.IsMatch(resumeText, $@"\b{verb}\b", RegexOptions.IgnoreCase));
            if (actionVerbCount >= 3) qualityScore += 5;

            // Check for quantifiable metrics (e.g. percentages, money, counts like "10%", "$5k", "5 members")
            var metricsRegex = new Regex(@"\b(\d+%\b|\$\d+|\d+\s*k\b|\d+\s*members\b|\d+\s*users\b|million|billion)", RegexOptions.IgnoreCase);
            int metricsMatches = metricsRegex.Matches(resumeText).Count;
            if (metricsMatches >= 2) qualityScore += 5;

            breakdown.ContentQuality = qualityScore;

            // D. Format Compliance (10%)
            int formatScore = 0;
            // Word count check: typical resume 200 - 1500 words
            var wordCount = resumeText.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount >= 200 && wordCount <= 1500) formatScore += 5;

            // Contact details compliance check
            bool hasEmailMatch = Regex.IsMatch(resumeText, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
            bool hasPhoneMatch = Regex.IsMatch(resumeText, @"\+?\d[\d -]{7,}\d");
            if (hasEmailMatch && hasPhoneMatch) formatScore += 5;

            breakdown.FormatCompliance = formatScore;

            // Compute total score (max 100)
            int overallScore = breakdown.SkillCoverage + 
                               breakdown.PreferredSkills + 
                               breakdown.ResumeStructure + 
                               breakdown.ContentQuality + 
                               breakdown.FormatCompliance;

            overallScore = Math.Clamp(overallScore, 0, 100);

            // Compute Grade
            string grade = "F";
            if (overallScore >= 85) grade = "A";
            else if (overallScore >= 70) grade = "B";
            else if (overallScore >= 55) grade = "C";
            else if (overallScore >= 40) grade = "D";

            return (overallScore, grade, breakdown);
        }
    }
}
