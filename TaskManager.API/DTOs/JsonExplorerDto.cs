using System.Collections.Generic;

namespace TaskManager.API.DTOs
{
    public class FormatRequest
    {
        public string Json { get; set; } = string.Empty;
        public bool Minify { get; set; }
    }

    public class ValidateRequest
    {
        public string Json { get; set; } = string.Empty;
        public bool DetectDuplicates { get; set; }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public int? Line { get; set; }
        public int? Column { get; set; }
        public string? Token { get; set; }
    }

    public class CompareRequest
    {
        public string SourceJson { get; set; } = string.Empty;
        public string TargetJson { get; set; } = string.Empty;
    }

    public class DiffLine
    {
        public string Type { get; set; } = "unchanged"; // "added", "deleted", "modified", "unchanged"
        public int? SourceLineNumber { get; set; }
        public int? TargetLineNumber { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class SchemaValidationRequest
    {
        public string DataJson { get; set; } = string.Empty;
        public string SchemaJson { get; set; } = string.Empty;
    }

    public class SchemaValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ConversionRequest
    {
        public string Content { get; set; } = string.Empty;
    }

    public class JsonStats
    {
        public int ObjectCount { get; set; }
        public int ArrayCount { get; set; }
        public int KeyCount { get; set; }
        public int MaxDepth { get; set; }
        public long TotalSize { get; set; }
    }

    public class MaskRequest
    {
        public string Json { get; set; } = string.Empty;
        public List<string> MaskTypes { get; set; } = new(); // "email", "phone", "card"
    }

    public class CodeGenRequest
    {
        public string Json { get; set; } = string.Empty;
        public string Language { get; set; } = "csharp"; // "csharp", "typescript", "sql"
        public string RootObjectName { get; set; } = "RootObject";
    }

    public class CodeGenResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
    }
}
