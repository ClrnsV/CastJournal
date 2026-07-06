using CastJournal.Domain.Enums;

namespace CastJournal.Application.DTOs.Reports;

public class SystemReportFilterDto
{
    public ReportType Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TopCount { get; set; } = 5;   // how many entries in "top species", "top anglers", etc.
}