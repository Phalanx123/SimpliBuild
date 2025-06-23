// simpliBuild|Services|SimpliClient.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneOf;
using simpliBuild.Configuration;
using simpliBuild.Exceptions;
using simpliBuild.SWMS.Model;
using simpliBuild.SWMS.Model.Responses;
using simpliBuild.Utils;

namespace SimpliBuild;

public class SimpliClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SimpliClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

   
    public SimpliClient(
        HttpClient httpClient,
        IOptions<SimpliSWMSOptions> opts,
        ILogger<SimpliClient> logger,
        IOptions<JsonSerializerOptions> jsonOptions
    )
    {
        _httpClient = httpClient;
        _logger = logger;
        var o = opts.Value;
      
        _jsonOptions = jsonOptions.Value;
      
    }
    

    public async Task<OneOf<SimpliWorkerCreatedResponse, RFC7807Result.ProblemDetails>> CreateWorker(
        SimpliWorker simpliWorker)
    {

        var payload = new
        {
            email = simpliWorker.Email,
            mobile = simpliWorker.Mobile,
            employerBusinessName = simpliWorker.EmployerBusinessName,
            firstName = simpliWorker.FirstName,
            lastName = simpliWorker.LastName,
            preferredLanguage = simpliWorker.PreferredLanguage
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "workers")
        {
            Content = JsonContent.Create(payload, options: _jsonOptions)
        };

        using var resp = await _httpClient.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        if (resp.StatusCode == HttpStatusCode.Conflict)
            return GenerateProblemDetails(req, HttpStatusCode.Conflict, "Conflict creating worker", body);

        if (!resp.IsSuccessStatusCode)
            return GenerateProblemDetails(req, HttpStatusCode.InternalServerError, "Error creating worker", body);

        var result = JsonSerializer.Deserialize<SimpliWorkerCreatedResponse>(body, _jsonOptions);

        if (result.Worker?.Id is null)
            return GenerateProblemDetails(req, HttpStatusCode.InternalServerError, "Worker ID missing", body);

        return result;
    }

    public async Task<OneOf<SimpliWorkerCreatedResponse, RFC7807Result.ProblemDetails>> UpdateWorker(
        SimpliWorker simpliWorker, Guid workerId)
    {


        var payload = new
        {
            email = simpliWorker.Email,
            mobile = simpliWorker.Mobile,
            employerBusinessName = simpliWorker.EmployerBusinessName,
            firstName = simpliWorker.FirstName,
            lastName = simpliWorker.LastName,
            preferredLanguage = simpliWorker.PreferredLanguage
        };

        using var req = new HttpRequestMessage(HttpMethod.Patch, $"workers/{workerId}")
        {
            Content = JsonContent.Create(payload, options: _jsonOptions)
        };


        using var resp = await _httpClient.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            return GenerateProblemDetails(req, HttpStatusCode.InternalServerError, "Error updating worker", body);

        var result = JsonSerializer.Deserialize<SimpliWorkerCreatedResponse>(body, _jsonOptions);

        return result;
    }

    public async Task<SimpliResponses.SimpliResponse> GetWorkers(
        bool includeSWMS = false,
        string? keyword = null,
        string[]? attributes = null,
        int offset = 0,
        int limit = 100
    )
    {


        var uri = QueryHelpers.AddQueryString("workers", new Dictionary<string, string?>
        {
            ["includeSWMS"] = includeSWMS.ToString(),
            ["keyword"] = keyword,
            ["attributes"] = attributes is null ? null : string.Join(",", attributes),
            ["offset"] = offset.ToString(),
            ["limit"] = limit.ToString()
        });

        using var req = new HttpRequestMessage(HttpMethod.Get, uri);
     

        using var resp = await _httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<SimpliResponses.SimpliResponse>(_jsonOptions)
               ?? throw new SimpliBuildContentException("Failed to parse workers list");
    }

    public async Task<SimpliWorkerResponse> GetWorker(Guid id, bool includeSWMS)
    {


        var uri = QueryHelpers.AddQueryString($"workers/{id}", new Dictionary<string, string?>
        {
            ["includeSWMS"] = includeSWMS.ToString()
        });

        using var req = new HttpRequestMessage(HttpMethod.Get, uri);


        using var resp = await _httpClient.SendAsync(req);
        if (resp.StatusCode == HttpStatusCode.NotFound)
            throw new SimpliBuildApiException($"Worker {id} not found");

        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<SimpliWorkerResponse>(_jsonOptions)
               ?? throw new SimpliBuildContentException("Failed to parse worker");
    }

    public async Task<SimpliPerformActionOnSWMSWorkerResponse> PerformActionOnWorker(
        string swmsId, Guid workerId, SWMSWorkerAction action
    )
    {


        var act = action switch
        {
            SWMSWorkerAction.Activate => "activate",
            SWMSWorkerAction.Deactivate => "deactivate",
            SWMSWorkerAction.ResendInvitation => "resend-invitation",
            _ => throw new ArgumentException("Invalid action", nameof(action))
        };
        var encodedAction = WebUtility.UrlEncode(act);
        var uri = $"swms/{swmsId}/{workerId}?action={encodedAction}";
        using var req = new HttpRequestMessage(HttpMethod.Post, uri);

        using var resp = await _httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<SimpliPerformActionOnSWMSWorkerResponse>(_jsonOptions)
               ?? throw new SimpliBuildContentException("Failed to parse action response");
    }

    public async Task<OneOf<SimpliProjectsResponse, RFC7807Result.ProblemDetails>> GetProjects(
        Guid? organisationId, string? keyword, string? attributes, int offset = 0, int limit = 100
    )
    {


        var query = new Dictionary<string, string?>
        {
            ["keyword"] = keyword,
            ["attributes"] = attributes,
            ["offset"] = offset.ToString(),
            ["limit"] = limit.ToString()
        };
        var uri = QueryHelpers.AddQueryString("projects", query);

        using var req = new HttpRequestMessage(HttpMethod.Get, uri);
    
        if (organisationId != null)
            req.Headers.Add("X-Organisation-Id", organisationId.ToString());

        using var resp = await _httpClient.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            return GenerateProblemDetails(req, HttpStatusCode.InternalServerError, "Error fetching projects", body);

        var result = JsonSerializer.Deserialize<SimpliProjectsResponse>(body, _jsonOptions);
        
        return result;
    }

    public async Task<OneOf<SimpliProjectResponse, RFC7807Result.ProblemDetails>> GetProject(
        Guid projectId, Guid organisationId, bool includeSWMS = false, bool includeArchived = false
    )
    {


        var query = new Dictionary<string, string?>
        {
            ["includeSWMS"] = includeSWMS.ToString(),
            ["includeArchived"] = includeArchived.ToString()
        };
        var uri = QueryHelpers.AddQueryString($"projects/{projectId}", query);

        using var req = new HttpRequestMessage(HttpMethod.Get, uri);
 
        req.Headers.Add("X-Organisation-Id", organisationId.ToString());

        using var resp = await _httpClient.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        if (resp.StatusCode == HttpStatusCode.NotFound)
            return GenerateProblemDetails(req, HttpStatusCode.NotFound, "Project not found", body);

        if (!resp.IsSuccessStatusCode)
            return GenerateProblemDetails(req, HttpStatusCode.InternalServerError, "Error fetching project", body);

        var result = JsonSerializer.Deserialize<SimpliProjectResponse>(body, _jsonOptions);
        
        if (!includeArchived && result?.Project?.SWMS != null)
        {
            result.Project.SWMS = 
                result.Project.SWMS
                    .Where(x => x.Status != "Archived")
                    .ToList();
        }

        return result;
    }

    public async Task<SimpliProjectResponse?> CreateProject(SimpliProject simpliProject, Guid organisationId)
    {


        var bodyDict = new Dictionary<string, object>
        {
            ["name"] = simpliProject.Name,
            ["address1"] = simpliProject.Address1,
            ["suburb"] = simpliProject.Suburb,
            ["state"] = simpliProject.State.GetDescription(),
            ["country"] = simpliProject.Country.GetDescription(),
            ["postcode"] = simpliProject.Postcode
        };
        if (!string.IsNullOrWhiteSpace(simpliProject.Address2))
            bodyDict["address2"] = simpliProject.Address2;
        if (!string.IsNullOrWhiteSpace(simpliProject.Code))
            bodyDict["code"] = simpliProject.Code;

        using var req = new HttpRequestMessage(HttpMethod.Post, "projects")
        {
            Content = JsonContent.Create(bodyDict, options: _jsonOptions)
        };

        req.Headers.Add("X-Organisation-Id", organisationId.ToString());

        using var resp = await _httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<SimpliProjectResponse>(_jsonOptions);
    }

    public async Task<bool> InviteWorkerToSWMS(string swmsId, Guid workerId, bool sendInvitation = false)
    {
        var sendValue = WebUtility.UrlEncode(sendInvitation.ToString().ToLowerInvariant());
        var swmsEscaped    = WebUtility.UrlEncode(swmsId);
        var workerEscaped  = WebUtility.UrlEncode(workerId.ToString());

        var uri = $"swms/{swmsEscaped}/invite/{workerEscaped}?sendInvitation={sendValue}";
        
        using var req = new HttpRequestMessage(HttpMethod.Put, uri);
  

        using var resp = await _httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        var inviteResp = await resp.Content.ReadFromJsonAsync<SimpliWorkerInvitedToSwmsResponse>(_jsonOptions);
        return inviteResp?.Data.IsSuccessful ?? false;
    }

    private RFC7807Result.ProblemDetails GenerateProblemDetails(
        HttpRequestMessage req,
        HttpStatusCode status,
        string title,
        string detail
    ) => new RFC7807Result.ProblemDetails
    {
        Type = new Uri("about:blank"),
        Title = title,
        Status = status,
        Detail = detail,
        Instance = new Uri(req.RequestUri?.ToString())
    };
}