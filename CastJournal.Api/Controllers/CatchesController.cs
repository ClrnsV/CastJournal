using AutoMapper;
using CastJournal.Application.DTOs.Catches;
using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CastJournal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CatchesController : ControllerBase
{
    private readonly ICatchService _catchService;
    private readonly IMapper _mapper;

    public CatchesController(ICatchService catchService, IMapper mapper)
    {
        _catchService = catchService;
        _mapper = mapper;
    }

    // POST: api/catches
    [HttpPost]
    public async Task<ActionResult<CatchDto>> CreateCatch([FromBody] CreateCatchDto dto)
    {
        // Temporary hardcoded userId for testing
        string testUserId = "test-user-123";

        var result = await _catchService.CreateCatchAsync(dto, testUserId);
        return CreatedAtAction(nameof(GetCatch), new { id = result.Id }, result);
    }

    // GET: api/catches
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CatchDto>>> GetCatches()
    {
        string testUserId = "test-user-123";
        var catches = await _catchService.GetUserCatchesAsync(testUserId);
        return Ok(catches);
    }

    // GET: api/catches/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<CatchDto>> GetCatch(Guid id)
    {
        string testUserId = "test-user-123";
        var catchRecord = await _catchService.GetCatchByIdAsync(id, testUserId);

        if (catchRecord == null)
            return NotFound();

        return Ok(catchRecord);
    }
}