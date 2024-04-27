using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Cosmos
{
    public static class CosmosServiceCollectionExtensions
    {
        private const string DOCUMENT_ENDPOINT_CONFIGURATION_KEY = "AzureCosmos:DocumentEndpoint";

        public static IServiceCollection AddCosmos(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.Configure<CosmosOptions>(options =>
            {
                options.DocumentEndpoint = configuration[DOCUMENT_ENDPOINT_CONFIGURATION_KEY];
            });
            services.TryAddSingleton<IThreadStatsChangefeedDbService, ThreadStatsCosmosService>();

            return services;
        }
    }
}
