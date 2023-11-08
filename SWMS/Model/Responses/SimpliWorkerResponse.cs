using System.Text.Json.Serialization;

namespace simpliBuild.SWMS.Model.Responses;

public class SimpliWorkerResponse : SimpliResponse
{

    /// <summary>
    /// Request Id
    /// </summary>
    [JsonPropertyName("requestId")]
    public required string RequestId { get; set; }

    /// <summary>
    /// Data
    /// </summary>
    [JsonPropertyName("data")]
    public SimpliWorker? Worker { get; set; }

    [JsonPropertyName("error")] public SimpliError? Error { get; set; }
}