using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CastJournal.Application.DTOs.Profile;
public class PublicProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PreferredFishingMethods { get; set; }
    public DateTime MemberSince { get; set; }
    public int PublicCatchCount { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public bool IsFollowedByCurrentUser { get; set; }
}