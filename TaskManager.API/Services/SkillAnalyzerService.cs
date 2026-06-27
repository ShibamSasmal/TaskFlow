using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs.Resume;
using TaskManager.API.Services.Interfaces;
using TaskManager.API.Services.ML;

namespace TaskManager.API.Services
{
    public class SkillAnalyzerService : ISkillAnalyzerService
    {
        private readonly AppDbContext _context;
        private readonly SkillPredictor _predictor;

        public SkillAnalyzerService(AppDbContext context, SkillPredictor predictor)
        {
            _context = context;
            _predictor = predictor;
        }

        public async Task<List<DetectedSkillDto>> AnalyzeSkillsAsync(string resumeText)
        {
            var detectedSkills = new Dictionary<string, DetectedSkillDto>(StringComparer.OrdinalIgnoreCase);

            // 1. Get all skills and categories from database
            var skills = await _context.Skills
                .Include(s => s.Category)
                .ToListAsync();

            // 2. Exact keyword matching using name and aliases
            foreach (var skill in skills)
            {
                var searchTerms = new List<string> { skill.Name };
                if (!string.IsNullOrEmpty(skill.Aliases))
                {
                    var aliases = skill.Aliases.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => a.Trim());
                    searchTerms.AddRange(aliases);
                }

                foreach (var term in searchTerms)
                {
                    string pattern;
                    if (term.EndsWith("#") || term.EndsWith("++") || term.Contains("."))
                    {
                        pattern = $@"(?i)(?<=^|\s|\p{{P}}){Regex.Escape(term)}(?=$|\s|\p{{P}})";
                    }
                    else
                    {
                        pattern = $@"(?i)\b{Regex.Escape(term)}\b";
                    }

                    if (Regex.IsMatch(resumeText, pattern))
                    {
                        if (!detectedSkills.ContainsKey(skill.Name))
                        {
                            detectedSkills.Add(skill.Name, new DetectedSkillDto
                            {
                                Name = skill.Name,
                                Category = skill.Category.Name,
                                Confidence = 1.0f
                            });
                        }
                        break;
                    }
                }
            }

            // 3. ML.NET model prediction matching
            try
            {
                var predictions = _predictor.PredictSkills(resumeText);
                foreach (var pred in predictions)
                {
                    var matchedSkill = skills.FirstOrDefault(s => 
                        s.Name.Equals(pred.PredictedLabel, StringComparison.OrdinalIgnoreCase) ||
                        (s.Aliases != null && s.Aliases.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Any(a => a.Trim().Equals(pred.PredictedLabel, StringComparison.OrdinalIgnoreCase)))
                    );

                    if (matchedSkill != null)
                    {
                        if (detectedSkills.TryGetValue(matchedSkill.Name, out var existing))
                        {
                            if (pred.Confidence > existing.Confidence)
                            {
                                existing.Confidence = pred.Confidence;
                            }
                        }
                        else
                        {
                            detectedSkills.Add(matchedSkill.Name, new DetectedSkillDto
                            {
                                Name = matchedSkill.Name,
                                Category = matchedSkill.Category.Name,
                                Confidence = pred.Confidence
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ML.NET Prediction Error] {ex.Message}");
            }

            return detectedSkills.Values.ToList();
        }
    }
}
