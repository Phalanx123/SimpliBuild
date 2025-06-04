using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace simpliBuild.SWMS.Model.Responses;

public class SimpliWorkersResponse : SimpliResponse
{
    /// <summary>
    /// Data
    /// </summary>
    [JsonPropertyName("data")]
    public List<SimpliWorkerSWMS>? Workers { get; set; }

    [JsonPropertyName("error")] public SimpliError? Error { get; set; }
}