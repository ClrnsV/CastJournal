using CastJournal.Domain.Entities.Base;
using CastJournal.Domain.Enums;

namespace CastJournal.Domain.Entities;

public class FishingLocation : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public WaterType WaterType { get; set; }
    public bool IsPublic { get; set; } = false;

    public string UserId { get; set; } = string.Empty;

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation Properties
    public User? User { get; set; }
    public ICollection<Catch> Catches { get; set; } = new List<Catch>();
}