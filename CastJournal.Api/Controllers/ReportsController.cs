using CastJournal.Application.DTOs.Reports;
using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CastJournal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // GET api/reports?type=CatchActivity&startDate=2026-01-01&endDate=2026-06-30&topCount=5
    [HttpGet]
    public async Task<IActionResult> GetReport([FromQuery] SystemReportFilterDto filter)
    {
        var report = await _reportService.GenerateReportAsync(filter);
        return Ok(report);
    }
}