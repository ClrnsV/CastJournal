using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? UserId { get; set; }        // null for anonymous events like a failed login
    public string? UserEmail { get; set; }     // denormalized so history survives even if the account is later deleted

    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }    // e.g. "Catch", "User", "Species"
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
}