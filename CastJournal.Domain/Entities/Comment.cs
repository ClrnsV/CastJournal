using System;

namespace CastJournal.Domain.Entities;
public class Comment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CatchId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }

    public Catch? Catch { get; set; }
    public User? User { get; set; }
}