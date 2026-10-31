namespace QualiTrack.Services;

public class FileValidationService
{
    private static readonly HashSet<string> AllowedExtensions = new(
        StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".pdf"
    };

    private bool ValidateMagicBytes(IFormFile file, string extension)
    {
        using var stream = file.OpenReadStream();
        var buffer = new byte[8];
        var bytesRead = stream.Read(buffer, 0, buffer.Length);

        if (bytesRead < 4)
            return false;

        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
            ".png" => buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47,
            ".pdf" => buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46, // %PDF
            _ => true // Default allow untuk types lain
        };
    }
    
    public (bool IsValid, string ErrorMessage) Validate(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return (false, "File tidak boleh kosong.");
        
        const long maxSize = 10 * 1024 * 1024;
        if (file.Length > maxSize)
            return (false, $"Ukuran file maksimal 10 MB. File Anda {file.Length / (1024 * 1024)} MB.");
        
        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            return (false, $"Ekstensi file tidak diizinkan. Gunakan: {string.Join(", ", AllowedExtensions)}.");
        
        // Validasi magic bytes untuk prevent spoofing
        if (!ValidateMagicBytes(file, extension))
            return (false, "File tidak sesuai dengan tipe yang dideklarasikan.");
        
        return (true, string.Empty);
    }
    
    public static string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        return $"{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
    }
}
