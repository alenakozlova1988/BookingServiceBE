using BookingService.Application.Models.Kratos;
using System.Text.Json.Serialization;

public class KratosSession
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("identity")]
    public KratosIdentity Identity { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }
}

// Models/KratosIdentity.cs

