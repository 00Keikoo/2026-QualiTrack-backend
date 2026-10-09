// DTOs/AuditSummaryDto.cs
namespace QualiTrack.DTOs;

public class CreateAuditSummaryDto
{
    public string Content { get; set; } = string.Empty;
}

public class AuditSummaryResponseDto
{
    public Guid Id { get; set; }
    public Guid AuditSessionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}