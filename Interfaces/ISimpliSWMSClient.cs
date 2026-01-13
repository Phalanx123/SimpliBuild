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

    Task<SimpliWorkerCreatedResponse> CreateWorker(CreateSimpliWorkerRequest createWorker, CancellationToken ct);
}