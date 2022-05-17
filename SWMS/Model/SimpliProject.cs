using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace simpliBuild.SWMS.Model
{
  public class SimpliProject
    {
        /// <summary>
        /// Project ID
        /// </summary>
        [JsonPropertyName("id")] 
        public Guid? Id { get; set; }

        /// <summary>
        /// Project Name/Description
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Project Address Line 1
        /// </summary>
        [JsonPropertyName("address1")] 
        public string? Address1 { get; set; }

        /// <summary>
        /// Project Address Line 2
        /// </summary>
        [JsonPropertyName("address2")] 
        public string? Address2 { get; set; }

        /// <summary>
        /// Project Suburb
        /// </summary>

        [JsonPropertyName("suburb")] 
        public string? Suburb { get; set; }

        /// <summary>
        /// Project State
        /// </summary>
        [JsonPropertyName("state")] 
        public string? State { get; set; }

        /// <summary>
        /// Project Postcode
        /// </summary>
        [JsonPropertyName("postcode")] 
        public string? PostCode { get; set; }

        /// <summary>
        /// Date/Time project was created
        /// </summary>
        [JsonPropertyName("createdAt")] 
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Project Code
        /// </summary>
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        /// <summary>
        /// Who archived the project
        /// </summary>
        [JsonPropertyName("archivedBy")]
        public Guid? ArchivedBy { get; set; }

        /// <summary>
        /// ID of the organisation that created it
        /// </summary>
        [JsonPropertyName("organisationId")] 
        public string? OrganisationID { get; set; }

        /// <summary>
        /// SWMS relating to this project
        /// </summary>
                [JsonPropertyName("swms")]
        public IEnumerable<SimpliSWMS>? SWMS { get; set; }
    }
}
