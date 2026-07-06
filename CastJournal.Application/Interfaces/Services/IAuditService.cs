using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.AuditLogs;
using CastJournal.Domain.Enums;

namespace CastJournal.Application.Interfaces.Services;

public interface IAuditService
{
    Task LogAsync(string? userId, string? userEmail, AuditAction action,
        string? entityType = null, string? entityId = null, string? details = null, string? ipAddress = null);

    Task<PagedResult<AuditLogDto>> GetLogsAsync(AuditLogFilterDto filter);
}