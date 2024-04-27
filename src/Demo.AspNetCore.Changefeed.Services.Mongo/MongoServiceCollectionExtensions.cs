using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Mongo
{
    public static class MongoServiceCollectionExtensions
    {
        private const string CONNECTION_STRING_CONFIGURATION_KEY = "Mongo:ConnectionString";

        public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.Configure<MongoOptions>(options =>
            {
                options.ConnectionString = configuration[CONNECTION_STRING_CONFIGURATION_KEY];
            });
            services.TryAddSingleton<IThreadStatsChangefeedDbService, ThreadStatsMongoService>();

            return services;
        }
    }
}
