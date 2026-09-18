using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace simpliBuild.SWMS.Model.Responses;

public class SimpliWorkerResponse : SimpliResponse
{
    /// <summary>
    /// Data
    /// </summary>
    [JsonPropertyName("data")]
    public SimpliWorker? Worker { get; set; }
}