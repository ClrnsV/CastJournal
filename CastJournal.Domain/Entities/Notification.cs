using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    public string Type { get; set; } = string.Empty;   // stored as string, matches AuditAction pattern
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public string? RelatedEntityType { get; set; }      // e.g. "Catch", "Species" — optional deep-link target
    public string? RelatedEntityId { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}