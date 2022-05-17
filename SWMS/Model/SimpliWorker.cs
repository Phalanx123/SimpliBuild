using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace simpliBuild.SWMS.Model
{
    public class SimpliWorker
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("employerBusinessName")]
        public string? EmployerBusinessName { get; set; }

        [JsonPropertyName("mobile")]
        public string? Mobile { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// SWMS workers have been invited to
        /// </summary>
        [JsonPropertyName("workerSWMS")]
        public List<SimpliWorkerSWMS>? WorkerSWMS { get; set; }

      
        public SimpliWorker(string id)
        {
            (Id) = (id);
        }
        [JsonConstructor]
        public SimpliWorker()
        {
            
        }
    }

    public enum SWMSWorkerAction
    {
        Activate,
        Deactivate,
        ResendInvitation
    }
}