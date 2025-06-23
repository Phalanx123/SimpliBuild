using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

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
        public required string Name { get; set; }

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
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StateOrRegion? State { get; set; }

        /// <summary>
        /// Project Postcode
        /// </summary>
        [JsonPropertyName("postcode")]
        public string? Postcode { get; set; }

        [JsonPropertyName("country")] 
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Country? Country { get; set; }

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
        /// Organisation ID
        /// </summary>
        [JsonPropertyName("organisationId")]
        public Guid? OrganisationId { get; set; }

        /// <summary>
        /// SWMS relating to this project
        /// </summary>
        [JsonPropertyName("swms")]
        public IEnumerable<SimpliSWMS>? SWMS { get; set; }
    }

    public enum Country
    {
        [Description("Australia")] Australia = 1,
        [Description("New Zealand")] NewZealand = 2
    }


    public enum StateOrRegion
    {
        [Description("New South Wales")]
        NewSouthWales = 1,
        [Description("Queensland")]
        Queensland = 2,
        [Description("South Australia")]
        SouthAustralia = 3,
        [Description("Tasmania")]
        Tasmania = 4,
        [Description("Victoria")]
        Victoria = 5,
        [Description("Western Australia")]
        WesternAustralia = 6,
        [Description("Australian Capital Territory")]
        AustralianCapitalTerritory = 7,
        // New Zealand regions start from here
        [Description("Bay of Plenty")]
        BayOfPlenty = 11,
        [Description("Auckland")]
        Auckland = 12,
        [Description("Canterbury")]
        Canterbury = 13,
        [Description("Hawke's Bay")]
        HawkesBay = 14,
        [Description("Gisborne")]
        Gisborne = 15,
        [Description("Marlborough")]
        Marlborough = 16,
        [Description("Manawatu-Wanganui")]
        ManawatuWanganui = 17,
        [Description("Northland")]
        Northland = 18,
        [Description("Nelson")]
        Nelson = 19,
        [Description("Southland")]
        Southland = 20,
        [Description("Otago")]
        Otago = 21,
        [Description("Tasman")]
        Tasman = 22,
        [Description("Taranaki")]
        Taranaki = 23,
        [Description("Waikato")]
        Waikato = 24,
        [Description("Wellington")]
        Wellington = 25,
        [Description("West Coast")]
        WestCoast = 26
    }
}