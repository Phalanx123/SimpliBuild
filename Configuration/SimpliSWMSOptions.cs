namespace simpliBuild.Configuration;

public class SimpliSWMSOptions
{
    public string BaseUrl { get; set; } = "https://api-prod.simpliswms.com.au/swms-api/v1/";
    public required string ApiKey { get; set; }
    public required string ApiSecret { get; set; }
}