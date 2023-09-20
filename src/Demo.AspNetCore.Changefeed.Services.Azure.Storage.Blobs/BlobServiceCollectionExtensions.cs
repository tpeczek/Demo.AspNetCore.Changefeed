using System;
using Demo.AspNetCore.Changefeed.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Storage.Blobs
{
    public static class BlobServiceCollectionExtensions
    {
        private const string SERVICE_URI_CONFIGURATION_KEY = "AzureStorageBlobs:ServiceUri";

        public static IServiceCollection AddBlob(this IServiceCollection services, IConfiguration configuration)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.Configure<BlobOptions>(options =>
            {
                options.ServiceUri = new Uri(configuration[SERVICE_URI_CONFIGURATION_KEY]);
            });
            services.TryAddSingleton<IThreadStatsChangefeedDbService, ThreadStatsBlobService>();

            return services;
        }
    }
}
