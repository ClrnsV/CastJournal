using AutoMapper;
using CastJournal.Application.DTOs.Analytics;
using CastJournal.Application.DTOs.Catches;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;
namespace CastJournal.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ICatchRepository _catchRepository;
    private readonly IMapper _mapper;

    public AnalyticsService(ICatchRepository catchRepository, IMapper mapper)
    {
        _catchRepository = catchRepository;
        _mapper = mapper;
    }

    public async Task<AnalyticsDto> GetAnalyticsAsync(string userId, AnalyticsFilterDto filter)
    {
        var catches = await _catchRepository.GetForAnalyticsAsync(userId, filter.StartDate, filter.EndDate);

        var result = new AnalyticsDto
        {
            TotalCatches = catches.Count
        };

        // Alternative flow: no catch data available → limited analytics displayed
        if (catches.Count == 0)
            return result;

        var withWeight = catches.Where(c => c.Weight.HasValue).ToList();
        var withLength = catches.Where(c => c.Length.HasValue).ToList();

        result.TotalWeight = withWeight.Sum(c => c.Weight!.Value);
        result.AverageWeight = withWeight.Count > 0 ? withWeight.Average(c => c.Weight!.Value) : null;
        result.AverageLength = withLength.Count > 0 ? withLength.Average(c => c.Length!.Value) : null;

        var bestByWeight = withWeight.OrderByDescending(c => c.Weight).FirstOrDefault();
        var bestByLength = withLength.OrderByDescending(c => c.Length).FirstOrDefault();

        result.PersonalBestByWeight = bestByWeight != null ? _mapper.Map<CatchDto>(bestByWeight) : null;
        result.PersonalBestByLength = bestByLength != null ? _mapper.Map<CatchDto>(bestByLength) : null;

        result.TopSpecies = catches
            .Where(c => c.Species != null)
            .GroupBy(c => c.Species!.CommonName)
            .Select(g => new SpeciesBreakdownDto
            {
                SpeciesName = g.Key,
                Count = g.Count(),
                TotalWeight = g.Where(c => c.Weight.HasValue).Sum(c => c.Weight!.Value)
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        result.TopLocations = catches
            .Where(c => c.Location != null)
            .GroupBy(c => c.Location!.Name)
            .Select(g => new LocationBreakdownDto
            {
                LocationName = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(l => l.Count)
            .ToList();

        result.MethodBreakdown = catches
            .Where(c => !string.IsNullOrWhiteSpace(c.FishingMethod))
            .GroupBy(c => c.FishingMethod!)
            .Select(g => new MethodBreakdownDto
            {
                Method = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(m => m.Count)
            .ToList();

        result.MonthlyTrend = catches
            .GroupBy(c => new { c.CatchDate.Year, c.CatchDate.Month })
            .Select(g => new MonthlyTrendDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count(),
                TotalWeight = g.Where(c => c.Weight.HasValue).Sum(c => c.Weight!.Value)
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        return result;
    }
}