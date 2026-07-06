using CastJournal.Domain.Entities.Base;

namespace CastJournal.Domain.Entities;

public class Catch : BaseEntity, IAuditableEntity
{
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
    public bool IsPublic { get; set; } = false;

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation Properties
    public User? User { get; set; }
    public Species? Species { get; set; }
    public FishingLocation? Location { get; set; }
    public ICollection<CatchMedia> Media { get; set; } = new List<CatchMedia>();
}