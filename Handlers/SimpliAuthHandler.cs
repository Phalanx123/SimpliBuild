using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using simpliBuild;
using simpliBuild.Interfaces;
using simpliBuild.Services;

namespace SimpliBuild.Handlers;

public class SimpliAuthHandler : DelegatingHandler
{
    private readonly ITokenService _tokenSvc;
    private readonly ILogger<SimpliAuthHandler> _logger;

    public SimpliAuthHandler(ITokenService tokenSvc, ILogger<SimpliAuthHandler> logger)
    {
        _tokenSvc = tokenSvc;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage req, CancellationToken ct)
    {
        var token = await _tokenSvc.GetTokenAsync();
        req.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);
        return await base.SendAsync(req, ct);
    }
}