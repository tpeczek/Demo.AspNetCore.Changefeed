using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Demo.AspNetCore.RethinkDB.Services
{
    internal static class ServiceCollectionExtensions
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
            services.TryAddTransient<IRethinkDbService, RethinkDbService>();

            return services;
        }

        public static IServiceCollection AddWebSocketConnections(this IServiceCollection services)
        {
            services.TryAddSingleton<IWebSocketConnectionsService, WebSocketConnectionsService>();

            return services;
        }

        public static IServiceCollection AddThreadStats(this IServiceCollection services)
        {
            services.AddSingleton<IHostedService, ThreadStatsGatherBackgroundService>();
            services.AddSingleton<IHostedService, ThreadStatsChangefeedBackgroundService>();

            return services;
        }
    }
}
