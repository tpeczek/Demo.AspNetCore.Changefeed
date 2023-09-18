using System;
using Demo.AspNetCore.Changefeed.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Storage.Blobs
{
    public static class BlobServiceCollectionExtensions
    {
        public static IServiceCollection AddBlob(this IServiceCollection services, Action<BlobOptions> configureOptions)
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
            services.TryAddSingleton<IThreadStatsChangefeedDbService, ThreadStatsBlobService>();

            return services;
        }
    }
}
