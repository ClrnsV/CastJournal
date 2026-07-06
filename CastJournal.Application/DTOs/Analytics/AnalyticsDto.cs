using CastJournal.Application.DTOs.Catches;


namespace CastJournal.Application.DTOs.Analytics;

public class AnalyticsDto
{
    public int TotalCatches { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal? AverageWeight { get; set; }
    public decimal? AverageLength { get; set; }

    public CatchDto? PersonalBestByWeight { get; set; }
    public CatchDto? PersonalBestByLength { get; set; }

    public List<SpeciesBreakdownDto> TopSpecies { get; set; } = new();
    public List<LocationBreakdownDto> TopLocations { get; set; } = new();
    public List<MethodBreakdownDto> MethodBreakdown { get; set; } = new();
    public List<MonthlyTrendDto> MonthlyTrend { get; set; } = new();
}

public class SpeciesBreakdownDto
{
    public string SpeciesName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalWeight { get; set; }
}

public class LocationBreakdownDto
{
    public string LocationName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MethodBreakdownDto
{
    public string Method { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MonthlyTrendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
    public decimal TotalWeight { get; set; }
}