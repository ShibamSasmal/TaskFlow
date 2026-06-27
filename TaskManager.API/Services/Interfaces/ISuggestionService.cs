using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.API.DTOs.Resume;

namespace TaskManager.API.Services.Interfaces
{
    public interface ISuggestionService
    {
        Task<(List<MissingSkillDto> MissingSkills, List<string> Suggestions)> GenerateSuggestionsAsync(
            List<string> detectedSkillNames, 
            string targetRole, 
            string resumeText);
    }
}
