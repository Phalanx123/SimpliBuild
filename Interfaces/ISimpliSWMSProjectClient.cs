using System;
using System.Threading.Tasks;
using simpliBuild.SWMS.Model;

namespace simpliBuild.Interfaces;

public interface ISimpliSWMSProjectClient
{
    /// <summary>
    /// Updates a project by its ID.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="project"></param>
    /// <param name="organisationId"></param>
    /// <returns></returns>
    Task<SimpliProject> UpdateProjectById(Guid projectId, SimpliProject project, Guid organisationId);
}