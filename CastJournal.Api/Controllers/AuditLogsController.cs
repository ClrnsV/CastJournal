using CastJournal.Application.DTOs.AuditLogs;
using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CastJournal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    // GET api/auditlogs?userId=&action=&entityType=&startDate=&endDate=&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] AuditLogFilterDto filter)
    {
        var result = await _auditService.GetLogsAsync(filter);
        return Ok(result);
    }
}