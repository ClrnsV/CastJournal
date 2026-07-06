using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Domain.Entities;

public class RevokedToken
{
    public Guid Id { get; set; }
    public string Jti { get; set; } = string.Empty;   // The token's unique ID (jti claim)
    public string UserId { get; set; } = string.Empty;
    public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }  // Same as the token's original expiry — purge safely later
}