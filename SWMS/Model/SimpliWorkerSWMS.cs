using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace simpliBuild.SWMS.Model
{
    public class SimpliWorkerSWMS
    {
        /// <summary>
        /// SWMS
        /// </summary>
        [JsonPropertyName("swms")]
        [DisallowNull, NotNull]

        public SimpliSWMS SWMS { get; set; }

        /// <summary>
        /// Worker
        /// </summary>
        public virtual SimpliWorker Worker { get; set; }

        /// <summary>
        /// SimpliSWMS Project
        /// </summary>
        [JsonPropertyName("project")]
        public SimpliProject Project { get; set; }
        /// <summary>
        /// Date sent to worker
        /// </summary>
        [JsonPropertyName("sentAt")]
        public DateTime SentAt { get; set; }
        /// <summary>
        /// Date signed by worker
        /// </summary>

        [JsonPropertyName("signedAt")]
        public DateTime? SignedAt { get; set; }
        /// <summary>
        /// Revision Signed by Worker
        /// </summary>
        [JsonPropertyName("signedRevision")]
        public int? SignedRevision { get; set; }
        /// <summary>
        /// Full name of worker
        /// </summary>
        [JsonPropertyName("signedFullName")]
        public string? SignedFullName { get; set; }
        /// <summary>
        /// Status
        /// </summary>

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonConstructor]
        public SimpliWorkerSWMS(SimpliSWMS swms, SimpliWorker worker, SimpliProject project, string status)
        {
            (SWMS, Worker, Project, Status) = (swms, worker, project, status);
        }
    }
}
