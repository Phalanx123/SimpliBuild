using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using simpliBuild.SWMS.Model;
using simpliBuild.SWMS.Model.Responses;

namespace simpliBuild.Interfaces;

public interface ISimpliSWMSClient
{
    /// <summary>
    /// Gets a list of workers assigned to the given SWMS ID.
    /// </summary>
    /// <param name="swmsID"></param>
    /// <param name="organisationId"></param>
    /// <returns></returns>
    Task<SimpliWorkersResponse> GetWorkersBySwmsIdAsync(Guid swmsID, Guid? organisationId = null);

    /// <summary>
    /// Gets one page of the organisation's workers, newest paging state carried by the caller.
    /// </summary>
    /// <param name="offset">Zero-based index of the first worker to return.</param>
    /// <param name="limit">How many workers to return, capped by SimpliSWMS at 100.</param>
    /// <param name="organisationId">
    /// Optional: UUIDv4 of the organisation or business unit. If null, the primary organisation is used.
    /// </param>
    /// <param name="ct"></param>
    Task<SimpliResponses.SimpliResponse> GetWorkersAsync(int offset = 0, int limit = 100,
        Guid? organisationId = null, CancellationToken ct = default);

    Task<SimpliWorkerCreatedResponse> CreateWorker(CreateSimpliWorkerRequest createWorker, CancellationToken ct);
}