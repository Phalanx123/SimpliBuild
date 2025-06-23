using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SimpliBuild;
using simpliBuild.Configuration;
using simpliBuild.Interfaces;
using simpliBuild.SWMS.Model;
using simpliBuild.SWMS.Model.Responses;

namespace simpliBuild;

public class SimpliSWMSClient : ISimpliSWMSClient
{
    private readonly HttpClient _httpClient;
private readonly SimpliClient _simpliClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public SimpliSWMSClient(IOptions<SimpliSWMSOptions> options, HttpClient httpClient, SimpliClient simpliClient)
    {
        _httpClient = httpClient;
        _simpliClient = simpliClient;

        // configure base address and required headers
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        
        // construct Basic Auth header from ApiKey and ApiSecret
        var credentials = $"{options.Value.ApiKey}:{options.Value.ApiSecret}";
        var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
       
    }

    /// <summary>
    /// Retrieves a list of workers assigned to the given SWMS ID.
    /// </summary>
    /// <param name="swmsId">GUID of the SWMS document</param>
    /// <param name="organisationId">
    /// Optional: UUIDv4 of the organisation or business unit. If null, primary organisation is used.
    /// </param>
    public async Task<SimpliWorkersResponse> GetWorkersBySwmsIdAsync(
        Guid swmsId,
        Guid? organisationId = null)
    {
        // Build request URL: "swms/{id}/workers"
        var requestUri = $"swms/{swmsId}/workers";
  

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        // If the caller provided an organisationId, include header
        if (organisationId.HasValue)
        {
            request.Headers.Add("X-Organisation-Id", organisationId.Value.ToString());
        }

        // Include Bearer token if your API uses OAuth2 (replace with actual token logic if needed)
        // request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // Deserialize JSON into our envelope type
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<SimpliWorkersResponse>(
            responseStream,
            _jsonOptions);

        if (result is null)
        {
            // In the unlikely case JSON is malformed
            throw new InvalidOperationException("Failed to deserialize SimpliSWMS response.");
        }

        return result;
    }
}