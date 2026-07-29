namespace MediTrack.Mvc.Services;

public interface IFileUploadService
{
    Task<string> SaveProductImageAsync(IFormFile file);
    Task<bool> DeleteFileAsync(string filePath);
}
