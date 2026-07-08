using System.ComponentModel.DataAnnotations;

namespace CastJournal.Application.DTOs.Catches;

public class CreateCatchDto
{
    public Guid SpeciesId { get; set; }
    public Guid? LocationId { get; set; }
    public DateTime CatchDate { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Weight must be greater than zero.")]
    public decimal? Weight { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Length must be greater than zero.")]
    public decimal? Length { get; set; }

    public string? GearUsed { get; set; }
    public string? BaitUsed { get; set; }
    public string? FishingMethod { get; set; }
    public string? Notes { get; set; }
    public string? WeatherConditions { get; set; }
    public bool IsPublic { get; set; } = false;
}