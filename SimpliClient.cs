using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
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
    private readonly JsonSerializerOptions _jsonOptions;


    public SimpliClient(
        HttpClient httpClient,
        IOptions<SimpliSWMSOptions> opts,
        IOptions<JsonSerializerOptions> jsonOptions
    )
    {
        _httpClient = httpClient;
        var o = opts.Value;

        _jsonOptions = jsonOptions.Value;
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

        using var req = new HttpRequestMessage(HttpMethod.Patch, $"workers/{workerId}");
        req.Content = JsonContent.Create(payload, options: _jsonOptions);


        using var resp = await _httpClient.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            return GenerateProblemDetails(req, HttpStatusCode.InternalServerError, "Error updating worker", body);

        var result = JsonSerializer.Deserialize<SimpliWorkerCreatedResponse>(body, _jsonOptions);

        return result;
    }

    public async Task<SimpliResponses.SimpliResponse> GetWorkers(
        bool includeSwms = false,
        string? keyword = null,
        string[]? attributes = null,
        int offset = 0,
        int limit = 100,
        Guid? organisationId = null
    )
    {
        var uri = QueryHelpers.AddQueryString("workers", new Dictionary<string, string?>
        {
            ["includeSWMS"] = includeSwms.ToString(),
            ["keyword"] = keyword,
            ["attributes"] = attributes is null ? null : string.Join(",", attributes),
            ["offset"] = offset.ToString(),
            ["limit"] = limit.ToString()
        });

        using var req = new HttpRequestMessage(HttpMethod.Get, uri);
        if (organisationId.HasValue)
            req.Headers.Add("X-Organisation-Id", organisationId.Value.ToString());


        using var resp = await _httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<SimpliResponses.SimpliResponse>(_jsonOptions)
               ?? throw new SimpliBuildContentException("Failed to parse workers list");
    }

    public async Task<SimpliWorkerResponse> GetWorker(Guid id, bool includeSwms, Guid? organisationId = null)
    {
        var uri = QueryHelpers.AddQueryString($"workers/{id}", new Dictionary<string, string?>
        {
            ["includeSWMS"] = includeSwms.ToString()
        });

        using var req = new HttpRequestMessage(HttpMethod.Get, uri);
        if (organisationId.HasValue)
            req.Headers.Add("X-Organisation-Id", organisationId.Value.ToString());


        using var resp = await _httpClient.SendAsync(req);
        if (resp.StatusCode == HttpStatusCode.NotFound)
            throw new SimpliBuildApiException($"Worker {id} not found");

        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<SimpliWorkerResponse>(_jsonOptions)
               ?? throw new SimpliBuildContentException("Failed to parse worker");
    }

    public async Task<SimpliPerformActionOnSWMSWorkerResponse> PerformActionOnWorker(
        string swmsId, Guid workerId, SWMSWorkerAction action, Guid? organisationId = null
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
        if (organisationId.HasValue)
            req.Headers.Add("X-Organisation-Id", organisationId.Value.ToString());

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
        Guid projectId, Guid organisationId, bool includeSwms = false, bool includeArchived = false
    )
    {
        var query = new Dictionary<string, string?>
        {
            ["includeSWMS"] = includeSwms.ToString(),
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
            ["address1"] = simpliProject.Address1 ?? string.Empty,
            ["suburb"] = simpliProject.Suburb ?? string.Empty,
            ["state"] = simpliProject.State != null ? simpliProject.State.GetDescription() : string.Empty,
            ["country"] = simpliProject.Country != null ? simpliProject.Country.GetDescription() : string.Empty,
            ["postcode"] = simpliProject.Postcode ?? string.Empty
        };
        if (!string.IsNullOrWhiteSpace(simpliProject.Address2))
            bodyDict["address2"] = simpliProject.Address2;
        if (!string.IsNullOrWhiteSpace(simpliProject.Code))
            bodyDict["code"] = simpliProject.Code;

        using var req = new HttpRequestMessage(HttpMethod.Post, "projects");
        req.Content = JsonContent.Create(bodyDict, options: _jsonOptions);

        req.Headers.Add("X-Organisation-Id", organisationId.ToString());

        using var resp = await _httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync<SimpliProjectResponse>(_jsonOptions);
    }

    public async Task<bool> InviteWorkerToSwms(string swmsId, Guid workerId, bool sendInvitation = false, Guid? organisationId = null)
    {
        var sendValue = WebUtility.UrlEncode(sendInvitation.ToString().ToLowerInvariant());
        var swmsEscaped = WebUtility.UrlEncode(swmsId);
        var workerEscaped = WebUtility.UrlEncode(workerId.ToString());

        var uri = $"swms/{swmsEscaped}/invite/{workerEscaped}?sendInvitation={sendValue}";

        // Deliberately not scoped with X-Organisation-Id like the other endpoints here - SimpliSWMS
        // 400s ("Workers is unknown") on this endpoint when the header's organisation doesn't match
        // the one the worker/SWMS actually lives under, which happens whenever the caller only knows
        // the destination job's business unit rather than the SWMS's actual one. Removed once before
        // for the same reason (see git history) and re-added by mistake.
        using var req = new HttpRequestMessage(HttpMethod.Put, uri);

        using var resp = await _httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();

        var inviteResp = await resp.Content.ReadFromJsonAsync<SimpliWorkerInvitedToSwmsResponse>(_jsonOptions);

        // SimpliSWMS returns HTTP 200 with data.isSuccess=false for requests it still processes and
        // dispatches - e.g. inviting a worker who's already an existing participant still sends the
        // notification. isSuccess doesn't reliably indicate the request failed, so the top-level
        // error field is the real failure signal here, consistent with every other endpoint in this
        // client (see PerformActionOnWorker).
        return inviteResp is not null && inviteResp.Error is null;
    }

    private RFC7807Result.ProblemDetails GenerateProblemDetails(
        HttpRequestMessage req,
        HttpStatusCode status,
        string title,
        string detail
    ) => new()
    {
        Type = new Uri("about:blank"),
        Title = title,
        Status = status,
        Detail = detail,
        Instance = new Uri(req.RequestUri?.ToString())
    };
}