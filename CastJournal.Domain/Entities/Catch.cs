using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CastJournal.Domain.Entities.Base;

namespace CastJournal.Domain.Entities;

public class Catch : BaseEntity, IAuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid SpeciesId { get; set; }
    public Guid? LocationId { get; set; }

    public DateTime CatchDate { get; set; }
    public decimal? Weight { get; set; }        // in kg or lbs
    public decimal? Length { get; set; }        // in cm or inches
    public string? GearUsed { get; set; }
    public string? BaitUsed { get; set; }
    public string? FishingMethod { get; set; }
    public string? Notes { get; set; }
    public string? WeatherConditions { get; set; }

    public bool IsPublic { get; set; } = false;

    // Navigation Properties
    public User? User { get; set; }
    public Species? Species { get; set; }
    public FishingLocation? Location { get; set; }
    public ICollection<CatchMedia> Media { get; set; } = new List<CatchMedia>();
}