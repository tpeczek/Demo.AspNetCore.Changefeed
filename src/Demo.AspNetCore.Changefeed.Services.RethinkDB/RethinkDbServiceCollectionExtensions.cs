using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.RethinkDB
{
    public static class RethinkDbServiceCollectionExtensions
    {
        public static IServiceCollection AddRethinkDb(this IServiceCollection services, Action<RethinkDbOptions> configureOptions)
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
            services.TryAddSingleton<IRethinkDbSingletonProvider, RethinkDbSingletonProvider>();
            services.TryAddTransient<IThreadStatsChangefeedDbService, ThreadStatsRethinkDbService>();

            return services;
        }
    }
}
