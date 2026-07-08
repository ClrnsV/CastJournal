using CastJournal.Application.DTOs.Backup;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using CastJournal.Domain.Enums;
using CastJournal.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CastJournal.Infrastructure.Services;

public class BackupService : IBackupService
{
    private readonly ApplicationDbContext _context;

    public BackupService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> CreateBackupAsync()
    {
        var backup = new BackupDataDto
        {
            Users = await _context.Users.Select(u => new BackupUserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName,
                IsActive = u.IsActive
            }).ToListAsync(),

            Species = await _context.Species.Select(s => new BackupSpeciesDto
            {
                Id = s.Id,
                CommonName = s.CommonName,
                ScientificName = s.ScientificName,
                Category = s.Category,
                Description = s.Description,
                AverageSize = s.AverageSize,
                ImageUrl = s.ImageUrl,
                IsApproved = s.IsApproved
            }).ToListAsync(),

            FishingLocations = await _context.FishingLocations.Select(l => new BackupLocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                WaterType = l.WaterType.ToString(),
                IsPublic = l.IsPublic,
                UserId = l.UserId
            }).ToListAsync(),

            ContentCategories = await _context.ContentCategories.Select(c => new BackupContentCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Type = c.Type.ToString(),
                IsActive = c.IsActive
            }).ToListAsync(),

            Catches = await _context.Catches.Include(c => c.Media).Select(c => new BackupCatchDto
            {
                Id = c.Id,
                UserId = c.UserId,
                SpeciesId = c.SpeciesId,
                LocationId = c.LocationId,
                CatchDate = c.CatchDate,
                Weight = c.Weight,
                Length = c.Length,
                GearUsed = c.GearUsed,
                BaitUsed = c.BaitUsed,
                FishingMethod = c.FishingMethod,
                Notes = c.Notes,
                WeatherConditions = c.WeatherConditions,
                IsPublic = c.IsPublic,
                IsDeleted = c.IsDeleted,
                Media = c.Media.Select(m => new BackupMediaDto
                {
                    Id = m.Id,
                    MediaUrl = m.MediaUrl,
                    FileName = m.FileName,
                    MediaType = m.MediaType.ToString(),
                    OrderIndex = m.OrderIndex
                }).ToList()
            }).ToListAsync()
        };

        var json = JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });
        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    // KNOWN LIMITATION: for existing catches, only scalar fields are reconciled on restore.
    // CatchMedia is not diffed/reconciled against the backup for pre-existing catches —
    // only newly-created catches get their full media list restored. Acceptable for now
    // since this only matters when restoring an old backup over actively-changing data.
    public async Task<RestoreResultDto> RestoreBackupAsync(BackupDataDto backup)
    {
        var result = new RestoreResultDto();

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Order matters — Species and Locations must exist before Catches reference them.
            foreach (var s in backup.Species)
            {
                var existing = await _context.Species.FindAsync(s.Id);
                if (existing == null)
                {
                    _context.Species.Add(new Species
                    {
                        Id = s.Id,
                        CommonName = s.CommonName,
                        ScientificName = s.ScientificName,
                        Category = s.Category,
                        Description = s.Description,
                        AverageSize = s.AverageSize,
                        ImageUrl = s.ImageUrl,
                        IsApproved = s.IsApproved
                    });
                }
                else
                {
                    existing.CommonName = s.CommonName;
                    existing.ScientificName = s.ScientificName;
                    existing.Category = s.Category;
                    existing.Description = s.Description;
                    existing.AverageSize = s.AverageSize;
                    existing.ImageUrl = s.ImageUrl;
                    existing.IsApproved = s.IsApproved;
                }
                result.SpeciesRestored++;
            }

            foreach (var l in backup.FishingLocations)
            {
                var existing = await _context.FishingLocations.FindAsync(l.Id);
                if (existing == null)
                {
                    _context.FishingLocations.Add(new FishingLocation
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Description = l.Description,
                        Latitude = l.Latitude,
                        Longitude = l.Longitude,
                        WaterType = Enum.Parse<WaterType>(l.WaterType),
                        IsPublic = l.IsPublic,
                        UserId = l.UserId
                    });
                }
                else
                {
                    existing.Name = l.Name;
                    existing.Description = l.Description;
                    existing.Latitude = l.Latitude;
                    existing.Longitude = l.Longitude;
                    existing.WaterType = Enum.Parse<WaterType>(l.WaterType);
                    existing.IsPublic = l.IsPublic;
                }
                result.LocationsRestored++;
            }

            foreach (var c in backup.ContentCategories)
            {
                var existing = await _context.ContentCategories.FindAsync(c.Id);
                if (existing == null)
                {
                    _context.ContentCategories.Add(new ContentCategory
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Type = Enum.Parse<CategoryType>(c.Type),
                        IsActive = c.IsActive
                    });
                }
                else
                {
                    existing.Name = c.Name;
                    existing.Description = c.Description;
                    existing.IsActive = c.IsActive;
                }
                result.CategoriesRestored++;
            }

            foreach (var c in backup.Catches)
            {
                var existing = await _context.Catches.Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == c.Id);
                if (existing == null)
                {
                    var newCatch = new Catch
                    {
                        Id = c.Id,
                        UserId = c.UserId,
                        SpeciesId = c.SpeciesId,
                        LocationId = c.LocationId,
                        CatchDate = c.CatchDate,
                        Weight = c.Weight,
                        Length = c.Length,
                        GearUsed = c.GearUsed,
                        BaitUsed = c.BaitUsed,
                        FishingMethod = c.FishingMethod,
                        Notes = c.Notes,
                        WeatherConditions = c.WeatherConditions,
                        IsPublic = c.IsPublic,
                        IsDeleted = c.IsDeleted
                    };
                    foreach (var m in c.Media)
                    {
                        newCatch.Media.Add(new CatchMedia
                        {
                            Id = m.Id,
                            MediaUrl = m.MediaUrl,
                            FileName = m.FileName,
                            MediaType = Enum.Parse<MediaType>(m.MediaType),
                            OrderIndex = m.OrderIndex
                        });
                    }
                    _context.Catches.Add(newCatch);
                }
                else
                {
                    existing.Weight = c.Weight;
                    existing.Length = c.Length;
                    existing.GearUsed = c.GearUsed;
                    existing.BaitUsed = c.BaitUsed;
                    existing.FishingMethod = c.FishingMethod;
                    existing.Notes = c.Notes;
                    existing.WeatherConditions = c.WeatherConditions;
                    existing.IsPublic = c.IsPublic;
                    existing.IsDeleted = c.IsDeleted;
                }
                result.CatchesRestored++;
            }

            // Users are update-only by design — no password hash in the backup means we
            // cannot safely recreate a deleted account with working login credentials.
            foreach (var u in backup.Users)
            {
                var existing = await _context.Users.FindAsync(u.Id);
                if (existing == null)
                {
                    result.UsersSkipped++;
                    result.Warnings.Add($"User {u.Email} (Id: {u.Id}) no longer exists and was not recreated (backups do not store passwords).");
                    continue;
                }

                existing.FullName = u.FullName;
                existing.IsActive = u.IsActive;
                result.UsersUpdated++;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return result;
    }

    public async Task<CleanupResultDto> CleanupAsync(int deletedCatchRetentionDays, int auditLogRetentionDays)
    {
        var result = new CleanupResultDto();

        var catchCutoff = DateTime.UtcNow.AddDays(-deletedCatchRetentionDays);
        var staleCatches = await _context.Catches
            .Where(c => c.IsDeleted && c.DeletedAt != null && c.DeletedAt < catchCutoff)
            .ToListAsync();
        _context.Catches.RemoveRange(staleCatches);
        result.DeletedCatchesPurged = staleCatches.Count;

        var expiredTokens = await _context.RevokedTokens
            .Where(t => t.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();
        _context.RevokedTokens.RemoveRange(expiredTokens);
        result.ExpiredTokensPurged = expiredTokens.Count;

        var auditCutoff = DateTime.UtcNow.AddDays(-auditLogRetentionDays);
        var oldLogs = await _context.AuditLogs
            .Where(a => a.Timestamp < auditCutoff)
            .ToListAsync();
        _context.AuditLogs.RemoveRange(oldLogs);
        result.OldAuditLogsPurged = oldLogs.Count;

        await _context.SaveChangesAsync();
        return result;
    }
}