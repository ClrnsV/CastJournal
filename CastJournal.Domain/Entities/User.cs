using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CastJournal.Domain.Entities.Base;
using Microsoft.AspNetCore.Identity;

namespace CastJournal.Domain.Entities;

public class User : IdentityUser
{
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PreferredFishingMethods { get; set; }

    // Google account's stable "sub" claim, set the first time someone
    // signs in with Google (either on a brand-new account, or linked
    // onto an existing email/password account with the same email).
    public string? GoogleId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public ICollection<Catch> Catches { get; set; } = new List<Catch>();
    public ICollection<FishingLocation> FishingLocations { get; set; } = new List<FishingLocation>();
}