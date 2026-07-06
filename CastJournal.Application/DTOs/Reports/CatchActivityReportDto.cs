using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.DTOs.Reports;

public class CatchActivityReportDto
{
    public int TotalCatchesAllTime { get; set; }
    public int TotalCatchesInPeriod { get; set; }
    public decimal TotalWeightInPeriod { get; set; }
    public decimal? AverageWeightInPeriod { get; set; }
    public List<SystemSpeciesBreakdownDto> TopSpecies { get; set; } = new();
    public List<TopAnglerDto> TopAnglers { get; set; } = new();
    public List<SystemMonthlyTrendDto> MonthlyTrend { get; set; } = new();
}

public class SystemSpeciesBreakdownDto
{
    public string SpeciesName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class SystemMonthlyTrendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
}
