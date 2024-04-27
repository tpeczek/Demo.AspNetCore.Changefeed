using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Storage.Blobs
{
    public static class BlobServiceCollectionExtensions
    {
        private const string SERVICE_URI_CONFIGURATION_KEY = "AzureStorageBlobs:ServiceUri";

        public static IServiceCollection AddBlob(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.Configure<BlobOptions>(options =>
            {
                options.ServiceUri = new Uri(configuration[SERVICE_URI_CONFIGURATION_KEY]);
            });
            services.TryAddSingleton<IThreadStatsChangefeedDbService, ThreadStatsBlobService>();

            return services;
        }
    }
}
