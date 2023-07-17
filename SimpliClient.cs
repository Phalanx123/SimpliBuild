
using Microsoft.Extensions.Logging;
using OneOf;
using RestSharp;
using RestSharp.Serializers.Json;
using simpliBuild.SWMS.Model;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static RFC7807Result;

namespace simpliBuild;
public class SimpliClient
{
    private readonly string ApiKey;
    private readonly string ApiSecret;
    private readonly string Api;
    private readonly ILogger _logger;
    public SimpliAccessToken? AccessToken;
    private static readonly SemaphoreSlim semaphore = new(1, 1);
    public static RestClient Client { get; set; }
         = new RestClient(@"https://api-prod.simpliswms.com.au/swms-api/v1/");

    public SimpliClient(string apiKey, string apiSecret, ILogger<SimpliClient> simpliClientLogger)
    {
        ApiKey = apiKey;
        ApiSecret = apiSecret;
        Api = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(ApiKey + ":" + ApiSecret));
        _logger=simpliClientLogger;
    }
    public async Task GetAuthTokenAsync()
    {
        await semaphore.WaitAsync();
        try
        {
            if (AccessToken == null || AccessToken.HasExpired)
            {
                _logger.LogInformation("Access token is null or expired. Retrieving new token...");

                var request = new RestRequest("/oauth/token", Method.Post).AddHeader("Authorization", Api);

                var result = await Client.ExecuteAsync<SimpliAccessToken>(request);

                if (result.ErrorException != null)
                {
                    const string message = "Error retrieving response. Check inner details for more info.";
                    var simpliException = new ApplicationException(message, result.ErrorException);
                    _logger.LogError(simpliException, "Exception occurred while retrieving access token.");
                    throw simpliException;
                }

                AccessToken = result.Data;
                _logger.LogInformation("New access token retrieved successfully.");
            }

            if (AccessToken == null)
            {
                _logger.LogError("No SimpliSWMS token received.");
                throw new AccessViolationException("No SimpliSWMS token received");
            }
        }
        finally
        {
            semaphore.Release();
        }
    }


    public async Task<OneOf<SimpliWorkerCreatedResponse, ProblemDetails>> CreateWorker(SimpliWorker simpliWorker)
    {
        RestRequest? request = null;

        try
        {
            await GetAuthTokenAsync();
            request = GenerateWorkerCreationRequest(simpliWorker);
            var result = await Client.ExecuteAsync(request);
            return ProcessWorkerCreationResult(result);
        }
        catch (Exception ex)
        {
            if (request != null)
                return GenerateExceptionProblemDetails(request, ex);
            else
                throw; // rethrow the original exception if request object is null
        }
    }

    private RestRequest GenerateWorkerCreationRequest(SimpliWorker simpliWorker)
    {
        var request = new RestRequest("workers", Method.Post);
        request.AddHeader("authorization", AccessToken!.AccessToken);
        request.AddJsonBody(
            new
            {
                email = simpliWorker.Email,
                mobile = simpliWorker.Mobile,
                employerBusinessName = simpliWorker.EmployerBusinessName,
                firstName = simpliWorker.FirstName,
                lastName = simpliWorker.LastName
            });
        return request;
    }

    private OneOf<SimpliWorkerCreatedResponse, ProblemDetails> ProcessWorkerCreationResult(RestResponse result)
    {
        if (result.Content == null)
        {
            return GenerateContentNullProblemDetails(result.Request);
        }

        if(result.StatusCode== HttpStatusCode.Conflict)
        {
            return GenerateConflictProblemDetails(result);
        }

        var worker = JsonSerializer.Deserialize<SimpliWorkerCreatedResponse>(result.Content);

        if (worker?.Worker == null || worker.Worker.Id == null)
        {
            return GenerateInvalidWorkerProblemDetails(result.Request);
        }

        return worker;
    }

    private ProblemDetails GenerateConflictProblemDetails(RestResponse response)
    {

        var problemDetails = new ProblemDetails
        {
            Type = new Uri("about:blank"),
            Title = "Conflict",
            Status = HttpStatusCode.Conflict,
            Detail = response.ErrorMessage!,
            Instance = Client.BuildUri(response.Request)
        };
        _logger.LogCritical(problemDetails.Detail);
        return problemDetails;
    }

    private ProblemDetails GenerateContentNullProblemDetails(RestRequest request)
    {
        var problemDetails = new ProblemDetails
        {
            Type = new Uri("about:blank"),
            Title = "Content Null",
            Status = HttpStatusCode.InternalServerError,
            Detail = "The response content is null.",
            Instance = Client.BuildUri(request)
        };
        _logger.LogCritical(problemDetails.Detail);
        return problemDetails;
    }

    private ProblemDetails GenerateInvalidWorkerProblemDetails(RestRequest request)
    {
        var problemDetails = new ProblemDetails
        {
            Type = new Uri("about:blank"),
            Title = "Invalid Worker",
            Status = HttpStatusCode.InternalServerError,
            Detail = "The created worker is null or its ID is null.",
            Instance = Client.BuildUri(request)
        };
        _logger.LogCritical(problemDetails.Detail);
        return problemDetails;
    }

    private ProblemDetails GenerateExceptionProblemDetails(RestRequest request, Exception ex)
    {
        var problemDetails = new ProblemDetails
        {
            Type = new Uri("about:blank"),
            Title = "Internal Server Error",
            Status = HttpStatusCode.InternalServerError,
            Detail = $"An unexpected error occurred while processing the request.",
            Instance = Client.BuildUri(request)
        };
        _logger.LogCritical(problemDetails.Detail, ex);
        return problemDetails;
    }

    /// <summary>
    /// Gets workers
    /// </summary>
    /// <param name="includeSWMS">Include details of signed SWMS</param>
    /// <param name="id">Worker ID</param>
    /// <param name="keyword">Searches First Name, Last Name, Company etc</param>
    /// <param name="attributes">Which fields to return, firstName, lastName, company, id</param>
    /// <param name="offset">Skip how many results</param>
    /// <param name="limit">Return how many</param>
    /// <returns></returns>
    /// <exception cref="AccessViolationException"></exception>
    public async Task<SimpliWorkersResponse?> GetWorkers(bool includeSWMS = false, string id = "", string? keyword = default, string[]? attributes = default, int offset = 0, int limit = 100)
    {

        await GetAuthTokenAsync();
        var request = new RestRequest("workers", Method.Get);
        request.AddHeader("authorization", AccessToken!.AccessToken);

        if (!string.IsNullOrWhiteSpace(id))
            request.AddQueryParameter("id", id);
        if (!string.IsNullOrEmpty(keyword))
            request.AddQueryParameter("keyword", keyword);
        if (attributes != null)
            request.AddQueryParameter("attributes", string.Concat(",", attributes));
        request.AddQueryParameter("includeSWMS", includeSWMS.ToString());
        request.AddQueryParameter("offset", offset.ToString());
        request.AddQueryParameter("limit", limit.ToString());


        var result = await Client.ExecuteAsync(request);
        if (result.Content == null)
            throw new Exception("Content received from SimpliSWMS is empty");

        var workersResponse = JsonSerializer.Deserialize<SimpliWorkersResponse>(result.Content);
        return workersResponse;
    }

    /// <summary>
    /// Performs an action on the worker
    /// </summary>
    /// <param name="swmsId">Id of SWMS</param>
    /// <param name="workerId">Id of Worker</param>
    /// <param name="action">Type of action to perform</param>
    /// <returns></returns>
    /// <exception cref="AccessViolationException">Issue with token</exception>
    /// <exception cref="ArgumentException">Action is invalid</exception>
    public async Task<SimpliPerformActionOnSWMSWorkerResponse> PerformActionOnWorker(string swmsId, string workerId, SWMSWorkerAction action)
    {
        await GetAuthTokenAsync();

        string? selectedAction;
        if (action == SWMSWorkerAction.Activate)
            selectedAction = "activate";
        else if (action == SWMSWorkerAction.Deactivate)
            selectedAction = "deactivate";
        else if (action == SWMSWorkerAction.ResendInvitation)
            selectedAction = "resend-invitation";
        else
            throw new ArgumentException(null, nameof(action));

        var request = new RestRequest($"swms/{swmsId}/{workerId}", Method.Post);
        request.AddHeader("authorization", AccessToken!.AccessToken);
        request.AddQueryParameter("action", selectedAction);


        var result = await Client.ExecuteAsync(request);
        SimpliPerformActionOnSWMSWorkerResponse workersResponse = JsonSerializer.Deserialize<SimpliPerformActionOnSWMSWorkerResponse>(result.Content!)!;
        return workersResponse!;
    }

    /// <summary>
    /// Gets a specific project
    /// </summary>
    /// <param name="projectId">Project ID of project</param>
    /// <returns></returns>
    /// <exception cref="AccessViolationException">Issue with token</exception>
    public async Task<OneOf<SimpliProjectResponse, ProblemDetails>> GetProject(Guid projectId, bool includeSWMS = false, bool includeArchived = false)
    {
        var request = new RestRequest($"/projects/{projectId}", Method.Get);
        try
        {
            await GetAuthTokenAsync();

            request.AddHeader("authorization", AccessToken!.AccessToken);
            if (includeSWMS)
                request.AddQueryParameter(nameof(includeSWMS), true);
            var result = await Client.ExecuteAsync(request);

            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return GenerateNotFoundProblemDetails(request, projectId);
            }

            if (!result.IsSuccessful)
            {
                return GenerateServerErrorProblemDetails(request);
            }

            var serialResult = JsonSerializer.Deserialize<SimpliProjectResponse>(result.Content!);

            if (!includeArchived && serialResult?.Project?.SWMS != null)
                serialResult.Project.SWMS = serialResult.Project.SWMS.Where(x => x.Status != "Archived");

            _logger.LogInformation($"Successfully retrieved project with ID '{projectId}'.");
            return serialResult!;
        }
        catch (Exception ex)
        {
            return GenerateExceptionProblemDetails(request, ex);
        }
    }

    private ProblemDetails GenerateNotFoundProblemDetails(RestRequest request, Guid projectId)
    {
        var problemDetails = new ProblemDetails
        {
            Type = new Uri("about:blank"),
            Title = "Project Not Found",
            Status = HttpStatusCode.NotFound,
            Detail = $"Project with ID '{projectId}' not found.",
            Instance = Client.BuildUri(request)
        };
        _logger.LogInformation(problemDetails.Detail);
        return problemDetails;
    }

    private ProblemDetails GenerateServerErrorProblemDetails(RestRequest request)
    {
        var problemDetails = new ProblemDetails
        {
            Type = new Uri("about:blank"),
            Title = "Internal Server Error",
            Status = HttpStatusCode.InternalServerError,
            Detail = "An unexpected error occurred on the SimpliSWMS server.",
            Instance = Client.BuildUri(request)
        };
        _logger.LogCritical(problemDetails.Detail);
        return problemDetails;
    }

    /// <summary>
    ///     Adds the project to SimpliSWMS
    /// </summary>
    /// <param name="project">SimpliSWMS Project</param>
    /// <returns>Response</returns>
    public async Task<SimpliProjectResponse?> CreateProject(SimpliProject simpliProject)
    {

        await GetAuthTokenAsync();

        var request = new RestRequest("projects", Method.Post);
        request.AddHeader("authorization", AccessToken!.AccessToken);

        request.AddJsonBody(
          new
          {
              name = simpliProject.Name,
              address1 = simpliProject.Address1,
              address2 = simpliProject.Address2,
              suburb = simpliProject.Suburb,
              state = simpliProject.State,
              postcode = simpliProject.PostCode,
              code = simpliProject.Code,
              organisationId = simpliProject.OrganisationID
          });
        var result = await Client.ExecuteAsync(request);
        SimpliProjectResponse? projectResponse = JsonSerializer.Deserialize<SimpliProjectResponse>(result.Content!);

        if (projectResponse?.Project != null && projectResponse.Project.Id == null)
            throw new Exception();
        return projectResponse;
    }

    public async Task<bool> InviteWorkerToSWMS(string swmsId, string workerId, bool sendInvitation = false)
    {
        await GetAuthTokenAsync();
        var request = new RestRequest($"swms/{swmsId}/invite", Method.Put);
        request.AddHeader("authorization", AccessToken!.AccessToken);
        request.AddQueryParameter(nameof(workerId), workerId);
        request.AddQueryParameter(nameof(sendInvitation), sendInvitation);

        //Adds the project
        var result = await Client.ExecuteAsync(request);
        //TODO {"error":{"status":400,"code":"40002","message":"Workers is unknown","field":"Workers"}}
        SimpliWorkerInvitedToSwmsResponse? response = JsonSerializer.Deserialize<SimpliWorkerInvitedToSwmsResponse>(result.Content!);
        if (response!.Error != null)
            return false;
        return response!.Data.IsSuccessful;
    }
}
