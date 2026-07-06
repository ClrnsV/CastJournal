using CastJournal.Domain.Enums;

namespace CastJournal.Application.DTOs.AuditLogs;

public class AuditLogFilterDto
{
    public string? UserId { get; set; }
    public AuditAction? Action { get; set; }
    public string? EntityType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}