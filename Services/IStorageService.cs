namespace QualiTrack.Services;

public interface IStorageService
{
    Task<string> UploadFileAsync(IFormFile file);
    Task DeleteFileAsync(string key);

    string GetPresignedUrl(string key, int expiryHours = 24);
}