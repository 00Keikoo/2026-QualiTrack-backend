namespace QualiTrack.Services;

public class LocalStorageService : IStorageService
{
    private readonly string _uploadPath;
    
    public LocalStorageService(IWebHostEnvironment env)
    {
        _uploadPath = Path.Combine(env.ContentRootPath, "uploads");
        Directory.CreateDirectory(_uploadPath);
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(_uploadPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"uploads/{fileName}";
    }

    public string GetPresignedUrl(string key, int expiryHours = 24)
    {
        return $"/{key}";
    }

    public async Task DeleteFileAsync(string key)
    {
        var fileName = Path.GetFileName(key);
        var filePath = Path.Combine(_uploadPath, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}