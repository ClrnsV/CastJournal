using System;

namespace CastJournal.Application.DTOs.Follow;

public class FollowUserDto
{
    public string Id { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsFollowedByCurrentUser { get; set; }
}