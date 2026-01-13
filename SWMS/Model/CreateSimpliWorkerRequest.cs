using System.Text.Json.Serialization;

namespace simpliBuild.SWMS.Model;

public class CreateSimpliWorkerRequest
{
    [JsonPropertyName("firstName")] public required string FirstName { get; init; }

    [JsonPropertyName("lastName")] public required string LastName { get; init; }

    [JsonPropertyName("email")] public required string Email { get; init; }

    [JsonPropertyName("employerBusinessName")]
    public required string EmployerBusinessName { get; init; }

    [JsonPropertyName("preferredLanguage")]
    public string PreferredLanguage { get; set; } = "english";

    [JsonPropertyName("mobile")] public required string Mobile { get; init; }
}