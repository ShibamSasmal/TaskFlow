using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs.Resume;
using TaskManager.API.Models;
using TaskManager.API.Services.Interfaces;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IResumeParserService _parserService;
        private readonly ISkillAnalyzerService _analyzerService;
        private readonly IScoringService _scoringService;
        private readonly ISuggestionService _suggestionService;

        public ResumeController(
            AppDbContext context,
            IResumeParserService parserService,
            ISkillAnalyzerService analyzerService,
            IScoringService scoringService,
            ISuggestionService suggestionService)
        {
            _context = context;
            _parserService = parserService;
            _analyzerService = analyzerService;
            _scoringService = scoringService;
            _suggestionService = suggestionService;
        }

        private Guid? CurrentUserId
        {
            get
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                {
                    return null;
                }
                return userId;
            }
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze(IFormFile file, [FromForm] string targetRole)
        {
            // 1. Validation
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded.");
            }

            if (string.IsNullOrEmpty(targetRole))
            {
                return BadRequest("Target role is required.");
            }

            // File size validation (5 MB limit)
            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return BadRequest("File size exceeds the maximum limit of 5 MB.");
            }

            // File extension validation
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".pdf" && extension != ".docx")
            {
                return BadRequest("Unsupported file type. Please upload a PDF or DOCX file.");
            }

            try
            {
                // 2. Extract raw text
                string extractedText;
                using (var stream = file.OpenReadStream())
                {
                    extractedText = _parserService.ExtractText(stream, file.ContentType);
                }

                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    return UnprocessableEntity("Unable to extract text content from the uploaded resume.");
                }

                // 3. Analyze Skills
                var detectedSkills = await _analyzerService.AnalyzeSkillsAsync(extractedText);
                var detectedSkillNames = detectedSkills.Select(s => s.Name).ToList();

                // 4. Compute Scores
                var (overallScore, grade, scoreBreakdown) = await _scoringService.ComputeScoreAsync(detectedSkillNames, targetRole, extractedText);

                // 5. Generate Suggestions & Missing Skills
                var (missingSkills, suggestions) = await _suggestionService.GenerateSuggestionsAsync(detectedSkillNames, targetRole, extractedText);

                // 6. Map to entity model and save to DB
                var analysisResult = new ResumeAnalysisResult
                {
                    Id = Guid.NewGuid(),
                    FileName = file.FileName,
                    TargetRole = targetRole,
                    OverallScore = overallScore,
                    ScoreGrade = grade,
                    DetectedSkillsJson = JsonSerializer.Serialize(detectedSkills),
                    MissingSkillsJson = JsonSerializer.Serialize(missingSkills),
                    ScoreBreakdownJson = JsonSerializer.Serialize(scoreBreakdown),
                    SuggestionsJson = JsonSerializer.Serialize(suggestions),
                    ProcessedAt = DateTime.UtcNow,
                    UserId = CurrentUserId
                };

                _context.ResumeAnalysisResults.Add(analysisResult);
                await _context.SaveChangesAsync();

                // 7. Return mapped DTO response
                var response = new AnalysisResponseDto
                {
                    AnalysisId = analysisResult.Id,
                    FileName = analysisResult.FileName,
                    TargetRole = analysisResult.TargetRole,
                    OverallScore = analysisResult.OverallScore,
                    ScoreGrade = analysisResult.ScoreGrade,
                    DetectedSkills = detectedSkills,
                    MissingSkills = missingSkills,
                    ScoreBreakdown = scoreBreakdown,
                    Suggestions = suggestions,
                    ProcessedAt = analysisResult.ProcessedAt
                };

                return Ok(response);
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Resume Analysis Error]: {ex}");
                return StatusCode(500, "An internal server error occurred while processing the resume.");
            }
        }

        [HttpGet("roles")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.RoleTemplates
                .Select(rt => rt.RoleName)
                .OrderBy(r => r)
                .ToListAsync();

            return Ok(new { roles });
        }

        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetHistory()
        {
            var userId = CurrentUserId;
            if (userId == null)
            {
                return Unauthorized();
            }

            var history = await _context.ResumeAnalysisResults
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ProcessedAt)
                .Select(r => new
                {
                    AnalysisId = r.Id,
                    r.FileName,
                    r.TargetRole,
                    r.OverallScore,
                    r.ScoreGrade,
                    r.ProcessedAt
                })
                .ToListAsync();

            return Ok(history);
        }

        [HttpGet("analysis/{id}")]
        public async Task<IActionResult> GetAnalysis(Guid id)
        {
            var result = await _context.ResumeAnalysisResults.FindAsync(id);
            if (result == null)
            {
                return NotFound($"Resume analysis with ID '{id}' was not found.");
            }

            // Optional: If user is authenticated, ensure they own it (unless it's a guest scan)
            var userId = CurrentUserId;
            if (result.UserId != null && result.UserId != userId)
            {
                return Forbid();
            }

            try
            {
                var response = new AnalysisResponseDto
                {
                    AnalysisId = result.Id,
                    FileName = result.FileName,
                    TargetRole = result.TargetRole,
                    OverallScore = result.OverallScore,
                    ScoreGrade = result.ScoreGrade,
                    DetectedSkills = JsonSerializer.Deserialize<List<DetectedSkillDto>>(result.DetectedSkillsJson) ?? new(),
                    MissingSkills = JsonSerializer.Deserialize<List<MissingSkillDto>>(result.MissingSkillsJson) ?? new(),
                    ScoreBreakdown = JsonSerializer.Deserialize<ScoreBreakdownDto>(result.ScoreBreakdownJson) ?? new(),
                    Suggestions = JsonSerializer.Deserialize<List<string>>(result.SuggestionsJson) ?? new(),
                    ProcessedAt = result.ProcessedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JSON Deserialization Error]: {ex}");
                return StatusCode(500, "Error loading analysis result details.");
            }
        }
    }
}
