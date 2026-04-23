using System.Text.Json;
using System.Text.Json.Serialization;
namespace BookingService.Application.Models.Kratos;

public class KratosIdentity
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("traits")]
    public JsonDocument Traits { get; set; } // Используйте JsonDocument для динамики

    [JsonPropertyName("schema_id")]
    public string SchemaId { get; set; }

    [JsonPropertyName("schema_url")]
    public string SchemaUrl { get; set; }
}