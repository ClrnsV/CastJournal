using System.Net.Http.Json;
using CastJournal.Application.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace CastJournal.Infrastructure.Services;
public class BrevoEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly EmailSettings _settings;

    public BrevoEmailService(HttpClient httpClient, IOptions<EmailSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink)
    {
        var payload = new
        {
            sender = new { email = _settings.SenderEmail, name = _settings.SenderName },
            to = new[] { new { email = toEmail, name = toName } },
            subject = "Reset your CastJournal password",
            htmlContent = $@"
                <p>Hi {toName},</p>
                <p>We received a request to reset your CastJournal password. Click the link below to choose a new one:</p>
                <p><a href=""{resetLink}"">Reset your password</a></p>
                <p>This link expires in 1 hour. If you didn't request this, you can safely ignore this email.</p>"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("api-key", _settings.ApiKey);
        request.Headers.Add("accept", "application/json");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"Brevo send failed ({(int)response.StatusCode}): {body}");
        }
    }
}