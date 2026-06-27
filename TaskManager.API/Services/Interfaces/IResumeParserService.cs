using System.IO;

namespace TaskManager.API.Services.Interfaces
{
    public interface IResumeParserService
    {
        string ExtractText(Stream fileStream, string contentType);
    }
}
