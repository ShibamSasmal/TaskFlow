using System.Collections.Generic;
using TaskManager.API.DTOs;

namespace TaskManager.API.Services.Interfaces
{
    public interface IJsonExplorerService
    {
        string Format(string json, bool minify);
        ValidationResult Validate(string json, bool detectDuplicates);
        List<DiffLine> Compare(string sourceJson, string targetJson);
        string GenerateSchema(string json);
        SchemaValidationResult ValidateSchema(string dataJson, string schemaJson);
        string XmlToJson(string xml);
        string CsvToJson(string csv);
        string YamlToJson(string yaml);
        string ExcelToJson(byte[] fileBytes);
        JsonStats GetStatistics(string json);
        string Flatten(string json);
        string Unflatten(string json);
        string MaskSensitiveData(string json, List<string> maskTypes);
        string GenerateCode(string json, string language, string rootName);
    }
}
