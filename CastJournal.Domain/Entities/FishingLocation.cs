using CastJournal.Domain.Entities.Base;
using CastJournal.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public User? User { get; set; }

    public ICollection<Catch> Catches { get; set; } = new List<Catch>();
}