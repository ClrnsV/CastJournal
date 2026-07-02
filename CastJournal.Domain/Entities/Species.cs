using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CastJournal.Domain.Entities.Base;

namespace CastJournal.Domain.Entities;

public class Species : BaseEntity
{
    public string CommonName { get; set; } = string.Empty;
    public string? ScientificName { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? AverageSize { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsApproved { get; set; } = true;

    // Navigation
    public ICollection<Catch> Catches { get; set; } = new List<Catch>();
}