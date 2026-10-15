using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.DTOs.Locations;

public class CreateLocationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double Latitude { get; set; }

    [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double Longitude { get; set; }

    public string WaterType { get; set; } = "Freshwater";
    public bool IsPublic { get; set; } = false;
}