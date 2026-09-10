using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.Comments;

namespace CastJournal.Application.Interfaces.Services;
public interface ICommentService
{
    Task<(CommentDto? Comment, string? Error)> AddCommentAsync(Guid catchId, string userId, string content);
    Task<(bool Succeeded, string? Error)> UpdateCommentAsync(Guid commentId, string userId, string content);
    Task<(bool Succeeded, string? Error)> DeleteCommentAsync(Guid commentId, string userId);
    Task<PagedResult<CommentDto>> GetCommentsAsync(Guid catchId, string? currentUserId, int page, int pageSize);
}