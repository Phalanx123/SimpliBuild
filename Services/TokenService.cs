// simpliBuild|Services|TokenService.cs
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using simpliBuild.Configuration;
using simpliBuild.Exceptions;
using simpliBuild.Interfaces;
using simpliBuild.SWMS.Model;

namespace simpliBuild.Services
{
    public class TokenService : ITokenService
    {
        readonly string _basicAuth;
        readonly ILogger<TokenService> _logger;
        static readonly SemaphoreSlim _semaphore = new(1,1);
        SimpliAccessToken? _cachedToken;

        public TokenService(
            IOptions<SimpliSWMSOptions> opts,
            ILogger<TokenService> logger)
        {
            var o = opts.Value;
            _basicAuth = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{o.ApiKey}:{o.ApiSecret}")
            );
            _logger = logger;
        }

        public async Task<SimpliAccessToken> GetTokenAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_cachedToken is null || _cachedToken.HasExpired)
                {
                    _logger.LogInformation("Fetching new SimpliSWMS token");
                    var client  = new RestClient("https://api-prod.simpliswms.com.au/swms-api/v1/");
                    var request = new RestRequest("oauth/token", Method.Post)
                        .AddHeader("Authorization", $"Basic {_basicAuth}");
                    var resp    = await client.ExecuteAsync<SimpliAccessToken>(request);
                    if (resp.ErrorException != null)
                        throw new SimpliBuildTokenRetrievalException("Token fetch failed", resp.ErrorException);

                    _cachedToken = resp.Data 
                        ?? throw new SimpliBuildTokenNullException("No token in response");
                    _logger.LogInformation("Token acquired");
                }
                return _cachedToken;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}