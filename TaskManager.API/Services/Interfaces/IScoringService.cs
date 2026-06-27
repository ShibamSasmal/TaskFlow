using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.API.DTOs.Resume;

namespace TaskManager.API.Services.Interfaces
{
    public interface IScoringService
    {
        Task<(int OverallScore, string Grade, ScoreBreakdownDto Breakdown)> ComputeScoreAsync(
            List<string> detectedSkillNames, 
            string targetRole, 
            string resumeText);
    }
}
