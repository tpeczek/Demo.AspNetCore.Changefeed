using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Demo.AspNetCore.Changefeed.Services
{
    internal static class ServiceCollectionExtensions
    {
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
