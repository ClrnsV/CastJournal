using System;

namespace CastJournal.Application.DTOs.Comments;
public class CommentDto
{
    public Guid Id { get; set; }
    public Guid CatchId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserFullName { get; set; }
    public string? UserAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsOwnedByCurrentUser { get; set; }
}