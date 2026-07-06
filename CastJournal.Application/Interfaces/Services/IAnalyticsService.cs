using CastJournal.Application.DTOs.Analytics;

namespace CastJournal.Application.Interfaces.Services;

public interface IAnalyticsService
{
    Task<AnalyticsDto> GetAnalyticsAsync(string userId, AnalyticsFilterDto filter);
}