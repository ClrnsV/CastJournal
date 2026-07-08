using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Domain.Entities;
public class CatchLike
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CatchId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Catch? Catch { get; set; }
    public User? User { get; set; }
}