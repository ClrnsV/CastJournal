using CastJournal.Application.DTOs.Backup;

namespace CastJournal.Application.Interfaces.Services;

public interface IBackupService
{
    Task<byte[]> CreateBackupAsync();
    Task<RestoreResultDto> RestoreBackupAsync(BackupDataDto backup);
    Task<CleanupResultDto> CleanupAsync(int deletedCatchRetentionDays, int auditLogRetentionDays);
}