namespace QualiTrack.DTOs;

public class CreateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    // Opsional. Kalau kosong / tidak valid, program menentukan warna otomatis dari nama department.
    public string? Color { get; set; }
}

public class UpdateDepartmentDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    // Opsional. Kalau kosong, warna lama dipertahankan.
    public string? Color { get; set; }
}

public class DepartmentResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    // URL siap pakai (lokal: /uploads/xxx, S3: presigned URL). Null = department belum punya logo.
    public string? LogoUrl { get; set; }
    public string Color { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
