using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.DTOs.Backup;

public class BackupDataDto
{
    public string Version { get; set; } = "1.0";
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public List<BackupUserDto> Users { get; set; } = new();
    public List<BackupSpeciesDto> Species { get; set; } = new();
    public List<BackupLocationDto> FishingLocations { get; set; } = new();
    public List<BackupContentCategoryDto> ContentCategories { get; set; } = new();
    public List<BackupCatchDto> Catches { get; set; } = new();
}

public class BackupUserDto
{
    public string Id { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public bool IsActive { get; set; }
}

public class BackupSpeciesDto
{
    public Guid Id { get; set; }
    public string CommonName { get; set; } = string.Empty;
    public string? ScientificName { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? AverageSize { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsApproved { get; set; }
}

public class BackupLocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string WaterType { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public class BackupContentCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class BackupCatchDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid SpeciesId { get; set; }
    public Guid? LocationId { get; set; }
    public DateTime CatchDate { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public string? GearUsed { get; set; }
    public string? BaitUsed { get; set; }
    public string? FishingMethod { get; set; }
    public string? Notes { get; set; }
    public string? WeatherConditions { get; set; }
    public bool IsPublic { get; set; }
    public bool IsDeleted { get; set; }
    public List<BackupMediaDto> Media { get; set; } = new();
}

public class BackupMediaDto
{
    public Guid Id { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}