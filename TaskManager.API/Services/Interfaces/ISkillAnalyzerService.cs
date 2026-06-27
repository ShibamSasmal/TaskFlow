using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.API.DTOs.Resume;

namespace TaskManager.API.Services.Interfaces
{
    public interface ISkillAnalyzerService
    {
        Task<List<DetectedSkillDto>> AnalyzeSkillsAsync(string resumeText);
    }
}
