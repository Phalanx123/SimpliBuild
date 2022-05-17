using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace simpliBuild.SWMS.Model
{
    public abstract class SimpliResponse
    {
        [JsonPropertyName("requestId")] public string RequestId { get; set; }

        [JsonPropertyName("error")] public SimpliError Error { get; set; }

        [JsonPropertyName("content")] public string Content { get; set; }
    }

    public class SimpliProjectResponse : SimpliResponse
    {
        [JsonPropertyName("data")] public SimpliProject Project { get; set; }
    }

    public class SimpliWorkerCreatedResponse : SimpliResponse
    {        
        [JsonPropertyName("data")] public SimpliWorker Worker { get; set; }
    }
    public class SimpliWorkerInvitedToSwmsResponse : SimpliResponse
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }

    public class SimpliWorkersResponse : SimpliResponse
    {
        [JsonPropertyName("data")] public List<SimpliWorker>? Workers { get; set; }
    }

    /// <summary>
    /// Did the action work?
    /// </summary>
    public class SimpliPerformActionOnSWMSWorkerResponse : SimpliResponse
    {
        [JsonPropertyName("data")] public SimpliPerformActionOnSWMSWorker? PerformActionOnSWMSWorker { get;set;}
    }

    public class SimpliError
    {
        [JsonPropertyName("status")] 
        public string Status { get; set; }

        [JsonPropertyName("code")] 
        public string Code { get; set; }

        [JsonPropertyName("message")] 
        public string Message { get; set; }

        [JsonPropertyName("details")] 
        public string Details { get; set; }
    }

    public class Data
    {
        /// <summary>
        /// Update Successful
        /// </summary>
       [JsonPropertyName("isSuccess")]
        public bool IsSuccessful { get; set; }
    }
}
