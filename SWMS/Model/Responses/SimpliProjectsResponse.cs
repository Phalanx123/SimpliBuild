using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace simpliBuild.SWMS.Model.Responses;

public class SimpliProjectsResponse
{
    
        [JsonPropertyName("totalResults")]
        public int TotalResults { get; set; }

        [JsonPropertyName("currentResultsSize")]
        public int CurrentResultsSize { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("data")]
        public SimpliProjectsListResponse? Data { get; set; }
    
}

public class SimpliProjectsListResponse
{

    [JsonPropertyName("projects")] public List<SimpliProject> Projects { get; set; } = [];
}