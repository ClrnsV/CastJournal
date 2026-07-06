namespace CastJournal.Application.DTOs.Users;

public class ServiceResult
{
    public bool Succeeded { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();

    public static ServiceResult Success() => new() { Succeeded = true };
    public static ServiceResult Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors };
}