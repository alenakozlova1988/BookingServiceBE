using System.Text.Json.Serialization;

namespace BookingService.Application.Dto;

public class KratosRegistrationWebhookModel
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;
}