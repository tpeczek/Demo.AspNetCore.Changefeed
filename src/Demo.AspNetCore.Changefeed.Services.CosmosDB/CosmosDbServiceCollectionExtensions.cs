using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.CosmosDB
{
    public static class CosmosDbServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosDb(this IServiceCollection services, Action<CosmosDbOptions> configureOptions)
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
            services.TryAddSingleton<IThreadStatsChangefeedDbService, ThreadStatsCosmosDbService>();

            return services;
        }
    }
}
