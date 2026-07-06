using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CastJournal.Application.DTOs.Reports;

public class SystemReportDto
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public bool HasData { get; set; }
    public string? Message { get; set; }   // populated when HasData is false

    public UserActivityReportDto? UserActivity { get; set; }
    public CatchActivityReportDto? CatchActivity { get; set; }
    public SpeciesOverviewReportDto? SpeciesOverview { get; set; }
}