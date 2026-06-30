using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CsvHelper;
using YamlDotNet.Serialization;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using TaskManager.API.DTOs;
using TaskManager.API.Services.Interfaces;

namespace TaskManager.API.Services
{
    public class JsonExplorerService : IJsonExplorerService
    {
        public string Format(string json, bool minify)
        {
            var token = JToken.Parse(json);
            return JsonConvert.SerializeObject(token, minify ? Formatting.None : Formatting.Indented);
        }

        public ValidationResult Validate(string json, bool detectDuplicates)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new ValidationResult { IsValid = false, ErrorMessage = "JSON cannot be empty." };
                }

                if (detectDuplicates)
                {
                    var settings = new JsonLoadSettings
                    {
                        DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
                        CommentHandling = CommentHandling.Ignore,
                        LineInfoHandling = LineInfoHandling.Load
                    };
                    using (var reader = new JsonTextReader(new StringReader(json)))
                    {
                        JToken.ReadFrom(reader, settings);
                    }
                }
                else
                {
                    using (var reader = new JsonTextReader(new StringReader(json)))
                    {
                        JToken.ReadFrom(reader);
                    }
                }
                return new ValidationResult { IsValid = true };
            }
            catch (JsonReaderException ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = ex.Message,
                    Line = ex.LineNumber,
                    Column = ex.LinePosition,
                    Token = ex.Path
                };
            }
            catch (Exception ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public List<DiffLine> Compare(string sourceJson, string targetJson)
        {
            string formattedSource;
            string formattedTarget;

            try { formattedSource = Format(sourceJson, false); } catch { formattedSource = sourceJson; }
            try { formattedTarget = Format(targetJson, false); } catch { formattedTarget = targetJson; }

            var sourceLines = formattedSource.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var targetLines = formattedTarget.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            int m = sourceLines.Length;
            int n = targetLines.Length;
            int[,] dp = new int[m + 1, n + 1];

            for (int i = 0; i <= m; i++)
            {
                for (int j = 0; j <= n; j++)
                {
                    if (i == 0 || j == 0)
                        dp[i, j] = 0;
                    else if (sourceLines[i - 1] == targetLines[j - 1])
                        dp[i, j] = dp[i - 1, j - 1] + 1;
                    else
                        dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                }
            }

            var diffList = new List<DiffLine>();
            int x = m, y = n;
            while (x > 0 || y > 0)
            {
                if (x > 0 && y > 0 && sourceLines[x - 1] == targetLines[y - 1])
                {
                    diffList.Insert(0, new DiffLine
                    {
                        Type = "unchanged",
                        SourceLineNumber = x,
                        TargetLineNumber = y,
                        Content = sourceLines[x - 1]
                    });
                    x--;
                    y--;
                }
                else if (y > 0 && (x == 0 || dp[x, y - 1] >= dp[x - 1, y]))
                {
                    diffList.Insert(0, new DiffLine
                    {
                        Type = "added",
                        SourceLineNumber = null,
                        TargetLineNumber = y,
                        Content = targetLines[y - 1]
                    });
                    y--;
                }
                else if (x > 0 && (y == 0 || dp[x, y - 1] < dp[x - 1, y]))
                {
                    diffList.Insert(0, new DiffLine
                    {
                        Type = "deleted",
                        SourceLineNumber = x,
                        TargetLineNumber = null,
                        Content = sourceLines[x - 1]
                    });
                    x--;
                }
            }

            return diffList;
        }

        public string GenerateSchema(string json)
        {
            var schema = NJsonSchema.JsonSchema.FromSampleJson(json);
            return schema.ToJson(Formatting.Indented);
        }

        public SchemaValidationResult ValidateSchema(string dataJson, string schemaJson)
        {
            var schema = NJsonSchema.JsonSchema.FromJsonAsync(schemaJson).GetAwaiter().GetResult();
            var errors = schema.Validate(dataJson);
            var result = new SchemaValidationResult
            {
                IsValid = errors.Count == 0
            };
            foreach (var error in errors)
            {
                result.Errors.Add($"{error.Path}: {error.Kind} - {error.Property}");
            }
            return result;
        }

        public string XmlToJson(string xml)
        {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            return JsonConvert.SerializeXmlNode(doc, Formatting.Indented);
        }

        public string CsvToJson(string csv)
        {
            using var reader = new StringReader(csv);
            using var csvReader = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);
            var records = csvReader.GetRecords<dynamic>().ToList();
            return JsonConvert.SerializeObject(records, Formatting.Indented);
        }

        public string YamlToJson(string yaml)
        {
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize<object>(new StringReader(yaml));
            var serializer = new SerializerBuilder()
                .JsonCompatible()
                .Build();
            var jsonText = serializer.Serialize(yamlObject);
            return Format(jsonText, false);
        }

        public string ExcelToJson(byte[] fileBytes)
        {
            using var memStream = new MemoryStream(fileBytes);
            using var document = SpreadsheetDocument.Open(memStream, false);
            var workbookPart = document.WorkbookPart;
            if (workbookPart == null) return "[]";

            var sheets = workbookPart.Workbook.Sheets;
            var list = new List<Dictionary<string, object?>>();

            foreach (Sheet sheet in sheets.Cast<Sheet>())
            {
                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
                var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
                if (sheetData == null) continue;

                var rows = sheetData.Elements<Row>().ToList();
                if (rows.Count == 0) continue;

                var headers = new List<string>();
                var headerRow = rows[0];
                foreach (Cell cell in headerRow.Elements<Cell>())
                {
                    headers.Add(GetCellValue(workbookPart, cell));
                }

                for (int i = 1; i < rows.Count; i++)
                {
                    var row = rows[i];
                    var dict = new Dictionary<string, object?>();
                    var cells = row.Elements<Cell>().ToList();
                    for (int j = 0; j < headers.Count; j++)
                    {
                        var cell = j < cells.Count ? cells[j] : null;
                        var val = cell != null ? GetCellValue(workbookPart, cell) : string.Empty;
                        dict[headers[j]] = val;
                    }
                    list.Add(dict);
                }
                break; // Only parse the first sheet
            }
            return JsonConvert.SerializeObject(list, Formatting.Indented);
        }

        private string GetCellValue(WorkbookPart workbookPart, Cell cell)
        {
            var val = cell.CellValue?.Text ?? string.Empty;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                if (stringTable != null)
                {
                    val = stringTable.SharedStringTable.ElementAt(int.Parse(val)).InnerText;
                }
            }
            return val;
        }

        public JsonStats GetStatistics(string json)
        {
            var stats = new JsonStats
            {
                TotalSize = Encoding.UTF8.GetByteCount(json)
            };
            try
            {
                var token = JToken.Parse(json);
                int objCount = 0;
                int arrCount = 0;
                int keyCount = 0;
                int maxDepth = 0;

                TraverseToken(token, 1, ref objCount, ref arrCount, ref keyCount, ref maxDepth);

                stats.ObjectCount = objCount;
                stats.ArrayCount = arrCount;
                stats.KeyCount = keyCount;
                stats.MaxDepth = maxDepth;
            }
            catch
            {
                // Return defaults
            }
            return stats;
        }

        private void TraverseToken(JToken token, int depth, ref int objCount, ref int arrCount, ref int keyCount, ref int maxDepth)
        {
            if (depth > maxDepth) maxDepth = depth;

            if (token is JObject obj)
            {
                objCount++;
                foreach (var prop in obj.Properties())
                {
                    keyCount++;
                    TraverseToken(prop.Value, depth + 1, ref objCount, ref arrCount, ref keyCount, ref maxDepth);
                }
            }
            else if (token is JArray arr)
            {
                arrCount++;
                foreach (var item in arr)
                {
                    TraverseToken(item, depth + 1, ref objCount, ref arrCount, ref keyCount, ref maxDepth);
                }
            }
        }

        public string Flatten(string json)
        {
            var obj = JsonConvert.DeserializeObject(json);
            var flatDict = new Dictionary<string, object?>();
            FlattenToken(string.Empty, (JToken)obj!, flatDict);
            return JsonConvert.SerializeObject(flatDict, Formatting.Indented);
        }

        private void FlattenToken(string prefix, JToken token, Dictionary<string, object?> dict)
        {
            if (token.Type == JTokenType.Object)
            {
                foreach (var prop in ((JObject)token).Properties())
                {
                    var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                    FlattenToken(key, prop.Value, dict);
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                int index = 0;
                foreach (var item in ((JArray)token))
                {
                    var key = $"{prefix}[{index}]";
                    FlattenToken(key, item, dict);
                    index++;
                }
            }
            else
            {
                dict[prefix] = ((JValue)token).Value;
            }
        }

        public string Unflatten(string json)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object?>>(json);
            if (dict == null) return "{}";
            
            var root = new JObject();
            foreach (var kvp in dict)
            {
                JContainer current = root;
                var path = kvp.Key;
                var parts = ParsePathParts(path);
                
                for (int i = 0; i < parts.Count; i++)
                {
                    var part = parts[i];
                    var isLast = i == parts.Count - 1;
                    
                    if (part.IsArray)
                    {
                        JArray arr;
                        if (current is JObject obj)
                        {
                            if (obj.TryGetValue(part.Name, out var existing))
                            {
                                arr = (JArray)existing;
                            }
                            else
                            {
                                arr = new JArray();
                                obj.Add(part.Name, arr);
                            }
                        }
                        else
                        {
                            arr = (JArray)current;
                        }
                        
                        while (arr.Count <= part.Index)
                        {
                            arr.Add(JValue.CreateNull());
                        }
                        
                        if (isLast)
                        {
                            arr[part.Index] = kvp.Value != null ? JToken.FromObject(kvp.Value) : JValue.CreateNull();
                        }
                        else
                        {
                            var nextPart = parts[i + 1];
                            if (arr[part.Index] == null || arr[part.Index].Type == JTokenType.Null)
                            {
                                arr[part.Index] = nextPart.IsArray ? new JArray() : new JObject();
                            }
                            current = (JContainer)arr[part.Index];
                        }
                    }
                    else
                    {
                        if (current is JObject obj)
                        {
                            if (isLast)
                            {
                                obj[part.Name] = kvp.Value != null ? JToken.FromObject(kvp.Value) : JValue.CreateNull();
                            }
                            else
                            {
                                var nextPart = parts[i + 1];
                                if (!obj.TryGetValue(part.Name, out var existing) || existing.Type == JTokenType.Null)
                                {
                                    existing = nextPart.IsArray ? new JArray() : new JObject();
                                    obj[part.Name] = existing;
                                }
                                current = (JContainer)existing;
                            }
                        }
                    }
                }
            }
            return JsonConvert.SerializeObject(root, Formatting.Indented);
        }

        private class PathPart
        {
            public string Name { get; set; } = string.Empty;
            public bool IsArray { get; set; }
            public int Index { get; set; }
        }

        private List<PathPart> ParsePathParts(string path)
        {
            var list = new List<PathPart>();
            var dotParts = path.Split('.');
            foreach (var dotPart in dotParts)
            {
                var bracketIndex = dotPart.IndexOf('[');
                if (bracketIndex >= 0)
                {
                    var name = dotPart.Substring(0, bracketIndex);
                    if (!string.IsNullOrEmpty(name))
                    {
                        list.Add(new PathPart { Name = name, IsArray = false });
                    }
                    
                    var remain = dotPart.Substring(bracketIndex);
                    var matches = Regex.Matches(remain, @"\[(\d+)\]");
                    foreach (Match match in matches)
                    {
                        var idx = int.Parse(match.Groups[1].Value);
                        list.Add(new PathPart { Name = string.Empty, IsArray = true, Index = idx });
                    }
                }
                else
                {
                    list.Add(new PathPart { Name = dotPart, IsArray = false });
                }
            }
            
            var result = new List<PathPart>();
            for (int i = 0; i < list.Count; i++)
            {
                var current = list[i];
                if (current.IsArray && i > 0 && !list[i - 1].IsArray)
                {
                    var last = result.Last();
                    last.IsArray = true;
                    last.Index = current.Index;
                }
                else
                {
                    result.Add(current);
                }
            }
            
            return result;
        }

        public string MaskSensitiveData(string json, List<string> maskTypes)
        {
            var token = JToken.Parse(json);
            MaskToken(token, maskTypes);
            return JsonConvert.SerializeObject(token, Formatting.Indented);
        }

        private void MaskToken(JToken token, List<string> maskTypes)
        {
            if (token is JObject obj)
            {
                foreach (var prop in obj.Properties())
                {
                    if (prop.Value is JValue val && val.Type == JTokenType.String)
                    {
                        var strVal = val.Value<string>() ?? string.Empty;
                        var masked = false;
                        if (maskTypes.Contains("email") && IsEmail(strVal))
                        {
                            prop.Value = MaskEmail(strVal);
                            masked = true;
                        }
                        if (!masked && maskTypes.Contains("phone") && IsPhone(strVal))
                        {
                            prop.Value = MaskPhone(strVal);
                            masked = true;
                        }
                        if (!masked && maskTypes.Contains("card") && IsCardNumber(strVal))
                        {
                            prop.Value = MaskCard(strVal);
                            masked = true;
                        }
                    }
                    else
                    {
                        MaskToken(prop.Value, maskTypes);
                    }
                }
            }
            else if (token is JArray arr)
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    var item = arr[i];
                    if (item is JValue val && val.Type == JTokenType.String)
                    {
                        var strVal = val.Value<string>() ?? string.Empty;
                        var masked = false;
                        if (maskTypes.Contains("email") && IsEmail(strVal))
                        {
                            arr[i] = MaskEmail(strVal);
                            masked = true;
                        }
                        if (!masked && maskTypes.Contains("phone") && IsPhone(strVal))
                        {
                            arr[i] = MaskPhone(strVal);
                            masked = true;
                        }
                        if (!masked && maskTypes.Contains("card") && IsCardNumber(strVal))
                        {
                            arr[i] = MaskCard(strVal);
                            masked = true;
                        }
                    }
                    else
                    {
                        MaskToken(item, maskTypes);
                    }
                }
            }
        }

        private bool IsEmail(string val) => val.Contains("@") && val.Contains(".");
        private bool IsPhone(string val) => Regex.IsMatch(val, @"^\+?[0-9\-\s\(\)]{7,20}$");
        private bool IsCardNumber(string val) => Regex.IsMatch(val, @"^\d{4}[\-\s]?\d{4}[\-\s]?\d{4}[\-\s]?\d{4}$") || (val.Length >= 13 && val.Length <= 19 && long.TryParse(val, out _));

        private string MaskEmail(string email)
        {
            var idx = email.IndexOf('@');
            if (idx <= 1) return "***" + email;
            return email.Substring(0, 2) + "***" + email.Substring(idx);
        }

        private string MaskPhone(string phone)
        {
            if (phone.Length <= 4) return "****";
            return phone.Substring(0, phone.Length - 4) + "****";
        }

        private string MaskCard(string card)
        {
            if (card.Length <= 4) return "****";
            return "**** **** **** " + card.Substring(card.Length - 4);
        }

        public string GenerateCode(string json, string language, string rootName)
        {
            try
            {
                var token = JToken.Parse(json);
                if (language.ToLower() == "csharp")
                {
                    var classes = new Dictionary<string, string>();
                    GenerateCSharpClass(token, rootName, classes);
                    return string.Join("\r\n\r\n", classes.Values);
                }
                else if (language.ToLower() == "typescript")
                {
                    var interfaces = new Dictionary<string, string>();
                    GenerateTypeScriptInterface(token, rootName, interfaces);
                    return string.Join("\r\n\r\n", interfaces.Values);
                }
                else if (language.ToLower() == "sql")
                {
                    return GenerateSqlTable(token, rootName);
                }
            }
            catch (Exception ex)
            {
                return $"// Error generating code: {ex.Message}";
            }
            return "// Unsupported language";
        }

        private void GenerateCSharpClass(JToken token, string className, Dictionary<string, string> classes)
        {
            if (classes.ContainsKey(className)) return;

            var sb = new StringBuilder();
            sb.AppendLine($"public class {className}");
            sb.AppendLine("{");

            if (token is JObject obj)
            {
                foreach (var prop in obj.Properties())
                {
                    var name = Capitalize(prop.Name);
                    var type = GetCSharpType(prop.Value, name, classes);
                    sb.AppendLine($"    public {type} {name} {{ get; set; }}");
                }
            }
            else if (token is JArray arr && arr.Count > 0)
            {
                var name = className + "Item";
                var type = GetCSharpType(arr[0], name, classes);
                sb.AppendLine($"    public List<{type}> Items {{ get; set; }} = new();");
            }

            sb.AppendLine("}");
            classes[className] = sb.ToString();
        }

        private string GetCSharpType(JToken token, string propName, Dictionary<string, string> classes)
        {
            switch (token.Type)
            {
                case JTokenType.Integer: return "int";
                case JTokenType.Float: return "double";
                case JTokenType.Boolean: return "bool";
                case JTokenType.Date: return "DateTime";
                case JTokenType.String: return "string";
                case JTokenType.Null: return "object";
                case JTokenType.Object:
                    GenerateCSharpClass(token, propName, classes);
                    return propName;
                case JTokenType.Array:
                    var arr = (JArray)token;
                    if (arr.Count == 0) return "List<object>";
                    var itemType = GetCSharpType(arr[0], propName + "Item", classes);
                    return $"List<{itemType}>";
                default: return "object";
            }
        }

        private void GenerateTypeScriptInterface(JToken token, string interfaceName, Dictionary<string, string> interfaces)
        {
            if (interfaces.ContainsKey(interfaceName)) return;

            var sb = new StringBuilder();
            sb.AppendLine($"export interface {interfaceName} {{");

            if (token is JObject obj)
            {
                foreach (var prop in obj.Properties())
                {
                    var name = prop.Name;
                    var type = GetTypeScriptType(prop.Value, interfaceName + Capitalize(name), interfaces);
                    sb.AppendLine($"    {name}: {type};");
                }
            }
            else if (token is JArray arr && arr.Count > 0)
            {
                var name = interfaceName + "Item";
                var type = GetTypeScriptType(arr[0], name, interfaces);
                sb.AppendLine($"    items: {type}[];");
            }

            sb.AppendLine("}");
            interfaces[interfaceName] = sb.ToString();
        }

        private string GetTypeScriptType(JToken token, string propName, Dictionary<string, string> interfaces)
        {
            switch (token.Type)
            {
                case JTokenType.Integer:
                case JTokenType.Float: return "number";
                case JTokenType.Boolean: return "boolean";
                case JTokenType.Date: return "Date";
                case JTokenType.String: return "string";
                case JTokenType.Null: return "any";
                case JTokenType.Object:
                    GenerateTypeScriptInterface(token, propName, interfaces);
                    return propName;
                case JTokenType.Array:
                    var arr = (JArray)token;
                    if (arr.Count == 0) return "any[]";
                    var itemType = GetTypeScriptType(arr[0], propName + "Item", interfaces);
                    return $"{itemType}[]";
                default: return "any";
            }
        }

        private string GenerateSqlTable(JToken token, string tableName)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE {tableName} (");
            sb.AppendLine("    id SERIAL PRIMARY KEY,");

            if (token is JObject obj)
            {
                var lines = new List<string>();
                foreach (var prop in obj.Properties())
                {
                    var type = GetSqlType(prop.Value);
                    lines.Add($"    {prop.Name.ToLower()} {type}");
                }
                sb.AppendLine(string.Join(",\r\n", lines));
            }
            else if (token is JArray arr && arr.Count > 0 && arr[0] is JObject itemObj)
            {
                var lines = new List<string>();
                foreach (var prop in itemObj.Properties())
                {
                    var type = GetSqlType(prop.Value);
                    lines.Add($"    {prop.Name.ToLower()} {type}");
                }
                sb.AppendLine(string.Join(",\r\n", lines));
            }
            else
            {
                sb.AppendLine("    value TEXT");
            }

            sb.AppendLine(");");
            return sb.ToString();
        }

        private string GetSqlType(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Integer: return "INT";
                case JTokenType.Float: return "NUMERIC";
                case JTokenType.Boolean: return "BOOLEAN";
                case JTokenType.Date: return "TIMESTAMP";
                case JTokenType.String: return "VARCHAR(255)";
                default: return "TEXT";
            }
        }

        private string Capitalize(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return char.ToUpper(str[0]) + str.Substring(1);
        }
    }
}
