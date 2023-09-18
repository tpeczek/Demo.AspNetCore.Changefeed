using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Cosmos
{
    public static class CosmosServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmos(this IServiceCollection services, Action<CosmosOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.Configure(configureOptions);
            services.TryAddSingleton<IThreadStatsChangefeedDbService, ThreadStatsCosmosService>();

            return services;
        }
    }
}
