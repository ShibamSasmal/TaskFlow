using System;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using TaskManager.API.Services.Interfaces;
using UglyToad.PdfPig;

namespace TaskManager.API.Services
{
    public class ResumeParserService : IResumeParserService
    {
        public string ExtractText(Stream fileStream, string contentType)
        {
            if (contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
            {
                return ExtractFromPdf(fileStream);
            }
            else if (contentType.Contains("word", StringComparison.OrdinalIgnoreCase) || 
                     contentType.Contains("officedocument", StringComparison.OrdinalIgnoreCase) ||
                     contentType.Contains("octet-stream", StringComparison.OrdinalIgnoreCase))
            {
                // Fall back to checking or just try DOCX parsing
                try
                {
                    return ExtractFromDocx(fileStream);
                }
                catch (Exception)
                {
                    // If it failed, try PDF parsing as a fallback
                    try
                    {
                        return ExtractFromPdf(fileStream);
                    }
                    catch
                    {
                        throw new NotSupportedException("Could not parse file. Ensure it is a valid PDF or DOCX document.");
                    }
                }
            }
            
            throw new NotSupportedException($"Content type '{contentType}' is not supported. Please upload a PDF or DOCX file.");
        }

        private string ExtractFromPdf(Stream pdfStream)
        {
            if (pdfStream.CanSeek)
            {
                pdfStream.Position = 0;
            }

            using var document = PdfDocument.Open(pdfStream);
            var sb = new StringBuilder();
            foreach (var page in document.GetPages())
            {
                sb.AppendLine(page.Text);
            }
            return sb.ToString();
        }

        private string ExtractFromDocx(Stream docxStream)
        {
            if (docxStream.CanSeek)
            {
                docxStream.Position = 0;
            }

            using var doc = WordprocessingDocument.Open(docxStream, false);
            return doc.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
        }
    }
}
