using System;

namespace CastJournal.Domain.Entities;
public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;   // stored hashed, never plaintext
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
}