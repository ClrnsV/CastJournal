using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CastJournal.Application.DTOs.Reports;

public class UserActivityReportDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int NewUsersInPeriod { get; set; }
    public int AdminCount { get; set; }
    public int RegularUserCount { get; set; }
    public List<TopAnglerDto> MostActiveUsers { get; set; } = new();
}

public class TopAnglerDto
{
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public int CatchCount { get; set; }
}