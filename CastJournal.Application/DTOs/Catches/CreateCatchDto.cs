using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.DTOs.Catches;

public class CreateCatchDto
{
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
}