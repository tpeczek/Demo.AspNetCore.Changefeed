using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Demo.AspNetCore.Changefeed.Configuration;
using Demo.AspNetCore.Changefeed.Services.Azure.Cosmos;
using Demo.AspNetCore.Changefeed.Services.Azure.Storage.Blobs;
using Demo.AspNetCore.Changefeed.Services.Mongo;
using Demo.AspNetCore.Changefeed.Services.RethinkDb;

namespace Demo.AspNetCore.Changefeed.Services
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChangefeed(this IServiceCollection services, IConfiguration configuration)
        {
            switch (configuration.GetChangefeedService())
            {
                case ChangefeedServices.AzureCosmos:
                    services.AddCosmos(configuration);
                    break;
                case ChangefeedServices.AzureStorageBlobs:
                    services.AddBlob(configuration);
                    break;
                case ChangefeedServices.Mongo:
                    services.AddMongo(options =>
                    {
                        options.ConnectionString = "mongodb://localhost:27017";
                    });
                    break;
                case ChangefeedServices.RethinkDb:
                    services.AddRethinkDb(options =>
                    {
                        options.HostnameOrIp = "127.0.0.1";
                    });
                    break;
                default:
                    throw new NotSupportedException($"Not supported changefeed type.");
            }

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
