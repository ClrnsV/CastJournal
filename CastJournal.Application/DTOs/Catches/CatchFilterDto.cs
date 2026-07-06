namespace CastJournal.Application.DTOs.Catches;

public class CatchFilterDto
{
    public Guid? SpeciesId { get; set; }
    public Guid? LocationId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinWeight { get; set; }
    public decimal? MaxWeight { get; set; }
    public string? GearUsed { get; set; }
    public string? BaitUsed { get; set; }
    public string? FishingMethod { get; set; }
    public string? SearchTerm { get; set; }      // free-text search across notes/gear/bait/method/species/location

    public string? SortBy { get; set; }          // "catchdate" (default), "weight", "length"
    public bool? SortDescending { get; set; }    // default true (newest first)

    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 20;
}