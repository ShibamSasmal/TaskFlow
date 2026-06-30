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
    public class SuggestionService : ISuggestionService
    {
        private readonly AppDbContext _context;

        // Static dictionary for popular learning resources
        private static readonly Dictionary<string, string> SkillUrls = new(StringComparer.OrdinalIgnoreCase)
        {
            { "C#", "https://learn.microsoft.com/en-us/dotnet/csharp/" },
            { "ASP.NET Core", "https://learn.microsoft.com/en-us/aspnet/core/" },
            { "Angular", "https://angular.dev/" },
            { "TypeScript", "https://www.typescriptlang.org/docs/" },
            { "SQL Server", "https://learn.microsoft.com/en-us/sql/sql-server/" },
            { "PostgreSQL", "https://www.postgresql.org/docs/" },
            { "SQL", "https://www.w3schools.com/sql/" },
            { "Docker", "https://docs.docker.com/get-started/" },
            { "Kubernetes", "https://kubernetes.io/docs/home/" },
            { "React", "https://react.dev/" },
            { "Git", "https://git-scm.com/doc" },
            { "Python", "https://docs.python.org/3/" },
            { "Azure", "https://learn.microsoft.com/en-us/azure/" },
            { "AWS", "https://aws.amazon.com/developer/language/net/" },
            { "Redis", "https://redis.io/documentation" },
            { "Microservices", "https://learn.microsoft.com/en-us/dotnet/architecture/microservices/" },
            { "RxJS", "https://rxjs.dev/" },
            { "HTML", "https://developer.mozilla.org/en-US/docs/Web/HTML" },
            { "CSS", "https://developer.mozilla.org/en-US/docs/Web/CSS" },
            { "CI/CD", "https://resources.github.com/devops/fundamentals/ci-cd/" },
            { "DevOps", "https://azure.microsoft.com/en-us/solutions/devops/what-is-devops/" },
            { "Machine Learning", "https://developers.google.com/machine-learning/crash-course" },
            { "PyTorch", "https://pytorch.org/docs/stable/index.html" },
            { "TensorFlow", "https://www.tensorflow.org/api_docs" },
            { "Scikit-learn", "https://scikit-learn.org/stable/documentation.html" },
            { "Pandas", "https://pandas.pydata.org/docs/" },
            { "NumPy", "https://numpy.org/doc/" }
        };

        public SuggestionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<MissingSkillDto> MissingSkills, List<string> Suggestions)> GenerateSuggestionsAsync(
            List<string> detectedSkillNames, 
            string targetRole, 
            string resumeText)
        {
            var missingSkills = new List<MissingSkillDto>();
            var suggestions = new List<string>();

            // 1. Compute missing skills
            var roleTemplate = await _context.RoleTemplates
                .Include(rt => rt.RoleSkills)
                .ThenInclude(rs => rs.Skill)
                .FirstOrDefaultAsync(rt => rt.RoleName.ToLower() == targetRole.ToLower());

            if (roleTemplate != null)
            {
                var targetSkills = roleTemplate.RoleSkills.ToList();

                foreach (var roleSkill in targetSkills)
                {
                    var isDetected = detectedSkillNames.Contains(roleSkill.Skill.Name, StringComparer.OrdinalIgnoreCase);
                    
                    if (!isDetected)
                    {
                        var name = roleSkill.Skill.Name;
                        var priority = roleSkill.IsRequired ? "Required" : "Preferred";
                        var reason = roleSkill.IsRequired 
                            ? $"Required skill benchmark for {targetRole} positions." 
                            : $"Highly recommended preferred skill for {targetRole} applicants.";

                        // Lookup resource URL
                        if (!SkillUrls.TryGetValue(name, out var url))
                        {
                            url = $"https://www.google.com/search?q=learn+{Uri.EscapeDataString(name)}";
                        }

                        missingSkills.Add(new MissingSkillDto
                        {
                            Name = name,
                            Priority = priority,
                            Reason = reason,
                            ResourceUrl = url
                        });
                    }
                }
            }

            // 2. Generate general suggestions
            string lowerText = resumeText.ToLower();

            // Contact Info
            bool hasEmailMatch = Regex.IsMatch(resumeText, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
            bool hasPhoneMatch = Regex.IsMatch(resumeText, @"\+?\d[\d -]{7,}\d");
            if (!hasEmailMatch || !hasPhoneMatch)
            {
                suggestions.Add("Ensure both email address and phone number are clearly visible in your contact section.");
            }

            // Structural sections
            if (!lowerText.Contains("summary") && !lowerText.Contains("profile") && !lowerText.Contains("objective"))
            {
                suggestions.Add("Include a brief professional summary (3-4 lines) at the top highlighting your core value.");
            }
            if (!lowerText.Contains("experience") && !lowerText.Contains("work") && !lowerText.Contains("employment") && !lowerText.Contains("history"))
            {
                suggestions.Add("Add a detailed professional experience section listing your roles, company names, and dates.");
            }
            if (!lowerText.Contains("education") && !lowerText.Contains("degree") && !lowerText.Contains("university"))
            {
                suggestions.Add("List your academic history, degrees, and graduation dates under an 'Education' section.");
            }

            // Length
            var wordCount = resumeText.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount < 200)
            {
                suggestions.Add("Your resume content seems too short. Elaborate on project details, scope of work, and specific responsibilities.");
            }
            else if (wordCount > 1500)
            {
                suggestions.Add("Your resume is very long. Consolidate your bullet points and aim for a concise 1 to 2 page resume (500-1000 words).");
            }

            // Action Verbs
            var actionVerbs = new[] { "led", "developed", "managed", "implemented", "created", "designed", "achieved", "optimized" };
            int actionVerbCount = actionVerbs.Count(verb => Regex.IsMatch(resumeText, $@"\b{verb}\b", RegexOptions.IgnoreCase));
            if (actionVerbCount < 3)
            {
                suggestions.Add("Begin your experience bullet points with strong action verbs (e.g. 'designed system', 'optimized database') rather than passive phrases.");
            }

            // Metrics
            var metricsRegex = new Regex(@"\b(\d+%\b|\$\d+|\d+\s*k\b|\d+\s*members\b|\d+\s*users\b|million|billion)", RegexOptions.IgnoreCase);
            int metricsMatches = metricsRegex.Matches(resumeText).Count;
            if (metricsMatches < 2)
            {
                suggestions.Add("Incorporate quantifiable impact (e.g. 'improved performance by 30%', 'reduced loading time by 2s') to highlight measurable accomplishments.");
            }

            // Custom addition suggestions if missing skills are detected
            if (missingSkills.Any(m => m.Priority == "Required"))
            {
                var reqSkillsList = string.Join(", ", missingSkills.Where(m => m.Priority == "Required").Take(3).Select(m => m.Name));
                suggestions.Add($"Consider adding projects or experience demonstrating skills in: {reqSkillsList} to align with {targetRole} expectations.");
            }

            return (missingSkills.OrderBy(m => m.Priority == "Required" ? 0 : 1).ToList(), suggestions);
        }
    }
}
