using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.AuditLogs;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using CastJournal.Domain.Enums;
using CastJournal.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CastJournal.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(string? userId, string? userEmail, AuditAction action,
        string? entityType = null, string? entityId = null, string? details = null, string? ipAddress = null)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = action.ToString(),
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Audit writes should never block or crash the request that triggered them —
            // but we should still know when this fails.
            _logger.LogError(ex, "Failed to write audit log for action {Action} by user {UserId}", action, userId);
        }
    }

    public async Task<PagedResult<AuditLogDto>> GetLogsAsync(AuditLogFilterDto filter)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.UserId))
            query = query.Where(a => a.UserId == filter.UserId);

        if (filter.Action.HasValue)
            query = query.Where(a => a.Action == filter.Action.Value.ToString());

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
            query = query.Where(a => a.EntityType == filter.EntityType);

        if (filter.StartDate.HasValue)
            query = query.Where(a => a.Timestamp >= filter.StartDate);

        if (filter.EndDate.HasValue)
            query = query.Where(a => a.Timestamp <= filter.EndDate);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                UserId = a.UserId,
                UserEmail = a.UserEmail,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Details = a.Details,
                IpAddress = a.IpAddress
            })
            .ToListAsync();

        return new PagedResult<AuditLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }
}