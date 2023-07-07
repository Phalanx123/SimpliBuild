using OneOf;
using System;
using System.Net;
using System.Text.Json.Serialization;

public class RFC7807Result
{
    public class ProblemDetails
    {
        [JsonPropertyName("type")]
        public Uri Type { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("status")]
        public HttpStatusCode? Status { get; set; }

        [JsonPropertyName("detail")]
        public string Detail { get; set; }

        [JsonPropertyName("instance")]
        public Uri Instance { get; set; }
    }

    public class SuccessDetails
    {
        public HttpStatusCode Status { get; set; }
        public object Data { get; set; }
    }

    public OneOf<SuccessDetails, ProblemDetails> Result { get; set; }
}
