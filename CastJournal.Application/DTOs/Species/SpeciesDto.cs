using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.DTOs.Species;

public class SpeciesDto
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