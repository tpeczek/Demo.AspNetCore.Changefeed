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
using Demo.AspNetCore.Changefeed.Services.Amazon.DynamoDB;

namespace Demo.AspNetCore.Changefeed.Services
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChangefeed(this IServiceCollection services, IConfiguration configuration)
        {
            switch (configuration.GetChangefeedService())
            {
                case ChangefeedServices.RethinkDb:
                    services.AddRethinkDb(configuration);
                    break;
                case ChangefeedServices.AzureCosmos:
                    services.AddCosmos(configuration);
                    break;
                case ChangefeedServices.Mongo:
                    services.AddMongo(configuration);
                    break;
                case ChangefeedServices.AzureStorageBlobs:
                    services.AddBlob(configuration);
                    break;
                case ChangefeedServices.AmazonDynamoDB:
                    services.AddDynamoDB(configuration);
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
