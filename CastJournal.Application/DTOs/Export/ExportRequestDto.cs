using CastJournal.Domain.Enums;

namespace CastJournal.Application.DTOs.Export;

public class ExportRequestDto
{
    public ExportFormat Format { get; set; } = ExportFormat.Csv;
    public bool IncludeAnalytics { get; set; } = false;

    // Same shape as CatchFilterDto (minus paging) — lets users export
    // exactly what they searched for in UC-14's Advanced Search & Filters.
    public Guid? SpeciesId { get; set; }
    public Guid? LocationId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinWeight { get; set; }
    public decimal? MaxWeight { get; set; }
    public string? GearUsed { get; set; }
    public string? BaitUsed { get; set; }
    public string? FishingMethod { get; set; }
    public string? SearchTerm { get; set; }
}