namespace CastJournal.Application.DTOs.Search;
public class UserSearchResultDto
{
    public string Id { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsFollowedByCurrentUser { get; set; }
}