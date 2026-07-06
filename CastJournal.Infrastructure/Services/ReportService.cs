using CastJournal.Application.DTOs.Reports;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using CastJournal.Domain.Enums;
using CastJournal.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CastJournal.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;

    public ReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SystemReportDto> GenerateReportAsync(SystemReportFilterDto filter)
    {
        var report = new SystemReportDto
        {
            ReportType = filter.Type.ToString()
        };

        switch (filter.Type)
        {
            case ReportType.UserActivity:
                report.UserActivity = await BuildUserActivityReportAsync(filter);
                report.HasData = report.UserActivity.TotalUsers > 0;
                break;

            case ReportType.CatchActivity:
                report.CatchActivity = await BuildCatchActivityReportAsync(filter);
                report.HasData = report.CatchActivity.TotalCatchesAllTime > 0;
                break;

            case ReportType.SpeciesOverview:
                report.SpeciesOverview = await BuildSpeciesOverviewReportAsync(filter);
                report.HasData = report.SpeciesOverview.TotalSpecies > 0;
                break;
        }

        if (!report.HasData)
            report.Message = "No report data available for the selected criteria.";

        return report;
    }

    private async Task<UserActivityReportDto> BuildUserActivityReportAsync(SystemReportFilterDto filter)
    {
        var users = await _context.Users.ToListAsync();
        var totalUsers = users.Count;

        var newUsersInPeriod = users.Count(u =>
            (!filter.StartDate.HasValue || u.CreatedAt >= filter.StartDate) &&
            (!filter.EndDate.HasValue || u.CreatedAt <= filter.EndDate));

        // Role counts via the Identity join tables — avoids calling UserManager per-user.
        var adminUserIds = await (
            from ur in _context.UserRoles
            join r in _context.Roles on ur.RoleId equals r.Id
            where r.Name == "Admin"
            select ur.UserId
        ).ToListAsync();

        var mostActive = await _context.Catches
            .GroupBy(c => c.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(filter.TopCount)
            .ToListAsync();

        var mostActiveUserIds = mostActive.Select(m => m.UserId).ToList();
        var mostActiveUsers = await _context.Users
            .Where(u => mostActiveUserIds.Contains(u.Id))
            .ToListAsync();

        var topAnglers = mostActive.Select(m =>
        {
            var user = mostActiveUsers.FirstOrDefault(u => u.Id == m.UserId);
            return new TopAnglerDto
            {
                UserId = m.UserId,
                UserName = user?.UserName,
                Email = user?.Email,
                CatchCount = m.Count
            };
        }).ToList();

        return new UserActivityReportDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = users.Count(u => u.IsActive),
            InactiveUsers = users.Count(u => !u.IsActive),
            NewUsersInPeriod = newUsersInPeriod,
            AdminCount = adminUserIds.Count,
            RegularUserCount = totalUsers - adminUserIds.Count,
            MostActiveUsers = topAnglers
        };
    }

    private async Task<CatchActivityReportDto> BuildCatchActivityReportAsync(SystemReportFilterDto filter)
    {
        var totalCatchesAllTime = await _context.Catches.CountAsync();

        var periodQuery = _context.Catches.AsQueryable();
        if (filter.StartDate.HasValue) periodQuery = periodQuery.Where(c => c.CatchDate >= filter.StartDate);
        if (filter.EndDate.HasValue) periodQuery = periodQuery.Where(c => c.CatchDate <= filter.EndDate);

        var periodCatches = await periodQuery.Include(c => c.Species).ToListAsync();

        var topSpecies = periodCatches
            .Where(c => c.Species != null)
            .GroupBy(c => c.Species!.CommonName)
            .Select(g => new SystemSpeciesBreakdownDto { SpeciesName = g.Key, Count = g.Count() })
            .OrderByDescending(s => s.Count)
            .Take(filter.TopCount)
            .ToList();

        var topAnglerGroups = periodCatches
            .GroupBy(c => c.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(filter.TopCount)
            .ToList();

        var anglerIds = topAnglerGroups.Select(a => a.UserId).ToList();
        var anglerUsers = await _context.Users.Where(u => anglerIds.Contains(u.Id)).ToListAsync();

        var topAnglers = topAnglerGroups.Select(a =>
        {
            var user = anglerUsers.FirstOrDefault(u => u.Id == a.UserId);
            return new TopAnglerDto
            {
                UserId = a.UserId,
                UserName = user?.UserName,
                Email = user?.Email,
                CatchCount = a.Count
            };
        }).ToList();

        var monthlyTrend = periodCatches
            .GroupBy(c => new { c.CatchDate.Year, c.CatchDate.Month })
            .Select(g => new SystemMonthlyTrendDto { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        return new CatchActivityReportDto
        {
            TotalCatchesAllTime = totalCatchesAllTime,
            TotalCatchesInPeriod = periodCatches.Count,
            TotalWeightInPeriod = periodCatches.Sum(c => c.Weight ?? 0),
            AverageWeightInPeriod = periodCatches.Any(c => c.Weight.HasValue)
                ? periodCatches.Where(c => c.Weight.HasValue).Average(c => c.Weight!.Value)
                : null,
            TopSpecies = topSpecies,
            TopAnglers = topAnglers,
            MonthlyTrend = monthlyTrend
        };
    }

    private async Task<SpeciesOverviewReportDto> BuildSpeciesOverviewReportAsync(SystemReportFilterDto filter)
    {
        var allSpecies = await _context.Species.Include(s => s.Catches).ToListAsync();

        var mostCaught = allSpecies
            .Select(s => new SystemSpeciesBreakdownDto { SpeciesName = s.CommonName, Count = s.Catches.Count })
            .Where(s => s.Count > 0)
            .OrderByDescending(s => s.Count)
            .Take(filter.TopCount)
            .ToList();

        var neverCaught = allSpecies
            .Where(s => s.Catches.Count == 0)
            .Select(s => s.CommonName)
            .ToList();

        return new SpeciesOverviewReportDto
        {
            TotalSpecies = allSpecies.Count,
            ApprovedSpecies = allSpecies.Count(s => s.IsApproved),
            PendingSpecies = allSpecies.Count(s => !s.IsApproved),
            MostCaughtSpecies = mostCaught,
            SpeciesNeverCaught = neverCaught
        };
    }
}