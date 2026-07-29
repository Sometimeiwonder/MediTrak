namespace MediTrack.Mvc.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;

    public FileUploadService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveProductImageAsync(IFormFile file)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(ext))
            throw new InvalidOperationException("File type is not allowed. Allowed: .jpg, .jpeg, .png, .webp");

        if (file.Length > 2 * 1024 * 1024)
            throw new InvalidOperationException("File is too large. Maximum size is 2MB.");

        var safeName = $"{Guid.NewGuid():N}{ext}";
        var folder = Path.Combine(_environment.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(folder);

        var path = Path.Combine(folder, safeName);
        using var stream = new FileStream(path, FileMode.CreateNew);
        await file.CopyToAsync(stream);

        return $"/uploads/products/{safeName}";
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return Task.FromResult(false);

        var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
