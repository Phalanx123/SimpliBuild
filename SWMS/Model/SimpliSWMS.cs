using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace simpliBuild.SWMS.Model
{
    public class SimpliSWMS
    {
        /// <summary>
        /// SWMS ID
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }
        /// <summary>
        /// Name of SWMS
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// Simpli SWMS Code
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }
        /// <summary>
        /// SWMS Status
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }
        /// <summary>
        /// Not sure
        /// </summary>
        public string? RevisionStatus { get; set; }
        /// <summary>
        /// Current Revision
        /// </summary>
        [JsonPropertyName("revision")]
        public int Revision { get; set; }
        
        [NotMapped]
        public List<SimpliWorkerSWMS>? Workers { get; set; }


    }
}
