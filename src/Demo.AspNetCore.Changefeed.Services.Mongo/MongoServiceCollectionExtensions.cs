using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Mongo
{
    public static class MongoServiceCollectionExtensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services, Action<MongoOptions> configureOptions)
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
            services.TryAddSingleton<IMongoClientSingletonProvider, MongoClientSingletonProvider>();
            services.TryAddTransient<IThreadStatsChangefeedDbService, ThreadStatsMongoService>();

            return services;
        }
    }
}
