namespace CastJournal.Infrastructure.Services;
public class EmailSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = "CastJournal";
    public string FrontendBaseUrl { get; set; } = string.Empty; // used to build the reset link
}