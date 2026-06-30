using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs;
using TaskManager.API.Services.Interfaces;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require JWT authentication for portfolio completeness
    public class JsonExplorerController : ControllerBase
    {
        private readonly IJsonExplorerService _jsonService;

        public JsonExplorerController(IJsonExplorerService jsonService)
        {
            _jsonService = jsonService;
        }

        [HttpPost("format")]
        public IActionResult Format([FromBody] FormatRequest request)
        {
            try
            {
                var result = _jsonService.Format(request.Json, request.Minify);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("minify")]
        public IActionResult Minify([FromBody] FormatRequest request)
        {
            try
            {
                var result = _jsonService.Format(request.Json, true);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("validate")]
        public IActionResult Validate([FromBody] ValidateRequest request)
        {
            var result = _jsonService.Validate(request.Json, request.DetectDuplicates);
            return Ok(result);
        }

        [HttpPost("compare")]
        public IActionResult Compare([FromBody] CompareRequest request)
        {
            try
            {
                var diff = _jsonService.Compare(request.SourceJson, request.TargetJson);
                return Ok(diff);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("schema")]
        public IActionResult GenerateSchema([FromBody] FormatRequest request)
        {
            try
            {
                var schema = _jsonService.GenerateSchema(request.Json);
                return Ok(new { schema });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("validate-schema")]
        public IActionResult ValidateSchema([FromBody] SchemaValidationRequest request)
        {
            try
            {
                var result = _jsonService.ValidateSchema(request.DataJson, request.SchemaJson);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("xml-to-json")]
        public IActionResult XmlToJson([FromBody] ConversionRequest request)
        {
            try
            {
                var result = _jsonService.XmlToJson(request.Content);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("csv-to-json")]
        public IActionResult CsvToJson([FromBody] ConversionRequest request)
        {
            try
            {
                var result = _jsonService.CsvToJson(request.Content);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("yaml-to-json")]
        public IActionResult YamlToJson([FromBody] ConversionRequest request)
        {
            try
            {
                var result = _jsonService.YamlToJson(request.Content);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("excel-to-json")]
        public async Task<IActionResult> ExcelToJson(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No file uploaded or file is empty." });
            }

            try
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var result = _jsonService.ExcelToJson(ms.ToArray());
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("statistics")]
        public IActionResult Statistics([FromBody] FormatRequest request)
        {
            try
            {
                var stats = _jsonService.GetStatistics(request.Json);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("flatten")]
        public IActionResult Flatten([FromBody] FormatRequest request)
        {
            try
            {
                var result = _jsonService.Flatten(request.Json);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("unflatten")]
        public IActionResult Unflatten([FromBody] FormatRequest request)
        {
            try
            {
                var result = _jsonService.Unflatten(request.Json);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("mask")]
        public IActionResult Mask([FromBody] MaskRequest request)
        {
            try
            {
                var result = _jsonService.MaskSensitiveData(request.Json, request.MaskTypes);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("codegen")]
        public IActionResult Codegen([FromBody] CodeGenRequest request)
        {
            try
            {
                var code = _jsonService.GenerateCode(request.Json, request.Language, request.RootObjectName);
                return Ok(new CodeGenResponse
                {
                    Code = code,
                    Language = request.Language
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("proxy")]
        public async Task<IActionResult> ProxyGet([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("URL query parameter is required.");
            }

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                
                // Return content as raw JSON (or text if it is not JSON)
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/json";
                return Content(content, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error fetching remote API response: {ex.Message}" });
            }
        }
    }
}
