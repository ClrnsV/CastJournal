using CastJournal.Application.DTOs.Comments;
using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CastJournal.API.Controllers;

[Route("api/catches/{catchId}/comments")]
[ApiController]
[Authorize]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    private string? GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    // GET api/catches/{catchId}/comments?page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetComments(Guid catchId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var currentUserId = GetCurrentUserId();
        var result = await _commentService.GetCommentsAsync(catchId, currentUserId, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(Guid catchId, [FromBody] CreateCommentDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var (comment, error) = await _commentService.AddCommentAsync(catchId, userId, dto.Content);
        return comment != null ? Ok(comment) : BadRequest(error);
    }

    [HttpPut("{commentId}")]
    public async Task<IActionResult> UpdateComment(Guid catchId, Guid commentId, [FromBody] UpdateCommentDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var (succeeded, error) = await _commentService.UpdateCommentAsync(commentId, userId, dto.Content);
        return succeeded ? NoContent() : BadRequest(error);
    }

    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(Guid catchId, Guid commentId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var (succeeded, error) = await _commentService.DeleteCommentAsync(commentId, userId);
        return succeeded ? NoContent() : BadRequest(error);
    }
}