using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace simpliBuild.SWMS.Model.Responses;

public class SimpliResponses
{
    public class SimpliResponse
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
        public PaginatedResponse? Data { get; set; }

        [JsonPropertyName("error")] public SimpliError? Error { get; set; }
    }

    public class PaginatedResponse
    {
        [JsonPropertyName("totalResults")] 
        public int TotalResults { get; set; }

        [JsonPropertyName("currentResultsSize")]
        public int CurrentResultsSize { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("limit")] 
        public int Limit { get; set; }
        
        [JsonPropertyName("workers")]
        public List<SimpliWorker>? Workers { get; set; }
    }
}