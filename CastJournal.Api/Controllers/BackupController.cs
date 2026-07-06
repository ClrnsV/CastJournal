using CastJournal.Application.DTOs.Backup;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CastJournal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class BackupController : ControllerBase
{
    private readonly IBackupService _backupService;
    private readonly IAuditService _auditService;

    public BackupController(IBackupService backupService, IAuditService auditService)
    {
        _backupService = backupService;
        _auditService = auditService;
    }

    private string? GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    // GET api/backup/export
    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var fileContent = await _backupService.CreateBackupAsync();
        var fileName = $"CastJournal_Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

        await _auditService.LogAsync(
            User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue(ClaimTypes.Email),
            AuditAction.BackupCreated, details: fileName, ipAddress: GetClientIp());

        return File(fileContent, "application/json", fileName);
    }

    // POST api/backup/restore  (multipart/form-data, field name "file")
    [HttpPost("restore")]
    public async Task<IActionResult> Restore(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No backup file provided.");

        BackupDataDto? backup;
        try
        {
            using var stream = file.OpenReadStream();
            backup = await JsonSerializer.DeserializeAsync<BackupDataDto>(stream);
        }
        catch (JsonException)
        {
            return BadRequest("The uploaded file is not a valid CastJournal backup (invalid JSON).");
        }

        if (backup == null)
            return BadRequest("The uploaded file could not be read.");

        var result = await _backupService.RestoreBackupAsync(backup);

        await _auditService.LogAsync(
            User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue(ClaimTypes.Email),
            AuditAction.BackupRestored,
            details: $"Restored: {result.SpeciesRestored} species, {result.LocationsRestored} locations, " +
                     $"{result.CategoriesRestored} categories, {result.CatchesRestored} catches, " +
                     $"{result.UsersUpdated} users updated, {result.UsersSkipped} users skipped",
            ipAddress: GetClientIp());

        return Ok(result);
    }

    // POST api/backup/cleanup?deletedCatchRetentionDays=90&auditLogRetentionDays=365
    [HttpPost("cleanup")]
    public async Task<IActionResult> Cleanup(int deletedCatchRetentionDays = 90, int auditLogRetentionDays = 365)
    {
        var result = await _backupService.CleanupAsync(deletedCatchRetentionDays, auditLogRetentionDays);

        await _auditService.LogAsync(
            User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue(ClaimTypes.Email),
            AuditAction.DataCleanup,
            details: $"Purged {result.DeletedCatchesPurged} catches, {result.ExpiredTokensPurged} tokens, {result.OldAuditLogsPurged} audit logs",
            ipAddress: GetClientIp());

        return Ok(result);
    }
}