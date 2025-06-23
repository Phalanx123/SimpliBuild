using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using simpliBuild.Interfaces;
using simpliBuild.SWMS.Model;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using simpliBuild.Utils;

namespace simpliBuild;

public class SimpliSWMSProjectClient : ISimpliSWMSProjectClient
{
    private readonly HttpClient _httpClient;
  private  readonly JsonSerializerOptions _jsonOptions;
  private readonly ILogger<SimpliSWMSProjectClient> _logger;
    public SimpliSWMSProjectClient(HttpClient httpClient, IOptions<JsonSerializerOptions> jsonOptions, ILogger<SimpliSWMSProjectClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = jsonOptions.Value;
    }


    public async Task<SimpliProject> UpdateProjectById(
        Guid projectId,
        SimpliProject project,
        Guid organisationId
    ) {
        var payload = new
        {
            name     = project.Name,
            address1 = project.Address1,
            address2 = string.IsNullOrWhiteSpace(project.Address2)? null:project.Address2,
            suburb   = project.Suburb,
            state    = project.State?.GetDescription(),
            postcode = project.Postcode,
            code     = project.Code
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"projects/{projectId}"
        );
        request.Headers.Add("X-Organisation-Id", organisationId.ToString());
        request.Content = JsonContent.Create(payload, options: _jsonOptions);

        // log outgoing JSON
        var json = await request.Content.ReadAsStringAsync();
        _logger.LogDebug("PATCH {Uri} payload: {Json}", request.RequestUri, json);

        using var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "PATCH {Uri} failed ({StatusCode}): {ErrorBody}",
                request.RequestUri, response.StatusCode, errorBody
            );
            throw new InvalidOperationException(
                $"API {(int)response.StatusCode} {response.ReasonPhrase}: {errorBody}"
            );
        }

        // success
        return await response.Content
                   .ReadFromJsonAsync<SimpliProject>(_jsonOptions)
               ?? throw new InvalidOperationException(
                   "Failed to deserialize updated SimpliProject."
               );
    }
}