namespace CastJournal.Application.DTOs.Users;

public class UserFilterDto
{
    public string? SearchTerm { get; set; }   // matches username/email/fullname
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 20;
}