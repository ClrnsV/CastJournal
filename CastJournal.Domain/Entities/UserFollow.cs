using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CastJournal.Domain.Entities;
public class UserFollow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FollowerId { get; set; } = string.Empty;    // the user doing the following
    public string FollowingId { get; set; } = string.Empty;   // the user being followed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? Follower { get; set; }
    public User? Following { get; set; }
}