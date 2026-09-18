using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using simpliBuild;
using simpliBuild.Configuration; // your root namespace
using SimpliBuild.Handlers; // for SimpliAuthHandler
using simpliBuild.Interfaces;
using simpliBuild.Services; // for SimpliOptions

namespace SimpliBuild.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers SimpliSWMS HTTP client, auth handler and related services.
        /// </summary>
        public static IServiceCollection AddSimpliSwmsApiClient(this IServiceCollection services)
        {
            services.AddTransient<ITokenService, TokenService>();
            // bind configuration to SimpliOptions
            services.Configure<SimpliSWMSOptions>(options =>
                options.BaseUrl = "https://api-prod.simpliswms.com.au"
            );
            services.Configure<JsonSerializerOptions>(options =>
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.PropertyNameCaseInsensitive = true;
            });

            services.AddHttpClient<SimpliClient>((sp, client) =>
                {
                    var opts = sp.GetRequiredService<IOptions<SimpliSWMSOptions>>().Value;
                    client.BaseAddress = new Uri(opts.BaseUrl);
                })
                .AddHttpMessageHandler<SimpliAuthHandler>();
            services.AddTransient<SimpliAuthHandler>();
            // Check-in runs the SWMS gate inline, and the mobile app gives up on a clock request after
            // 20s (queuing it as offline). The default 100s HttpClient timeout would let a hung
            // SimpliSWMS outlast that, so cap it well inside the phone's budget - a timeout here surfaces
            // as a fail-open "SimpliSWMS unavailable" check-in instead of a phantom offline queue entry.
            services.AddHttpClient<ISimpliSWMSClient, SimpliSwmsClient>((sp, client) =>
                {
                    var opts = sp.GetRequiredService<IOptions<SimpliSWMSOptions>>().Value;
                    client.BaseAddress = new Uri(opts.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(10);
                })
                .AddHttpMessageHandler<SimpliAuthHandler>();
            services.AddHttpClient<ISimpliSWMSProjectClient, SimpliSwmsProjectClient>((sp, client) =>
                {
                    var opts = sp.GetRequiredService<IOptions<SimpliSWMSOptions>>().Value;
                    client.BaseAddress = new Uri(opts.BaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(10);
                })
                .AddHttpMessageHandler<SimpliAuthHandler>();

            return services;
        }
    }
}