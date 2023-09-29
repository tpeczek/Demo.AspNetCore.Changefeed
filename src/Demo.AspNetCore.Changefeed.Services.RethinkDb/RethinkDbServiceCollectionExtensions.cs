using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.RethinkDb
{
    public static class RethinkDbServiceCollectionExtensions
    {
        private const string HOSTNAME_CONFIGURATION_KEY = "RethinkDb:Hostname";

        public static IServiceCollection AddRethinkDb(this IServiceCollection services, IConfiguration configuration)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.Configure<RethinkDbOptions>(options =>
            {
                options.Hostname = configuration[HOSTNAME_CONFIGURATION_KEY];
            });
            services.TryAddSingleton<IThreadStatsChangefeedDbService, ThreadStatsRethinkDbService>();

            return services;
        }
    }
}
