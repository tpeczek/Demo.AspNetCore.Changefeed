using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Amazon.DynamoDB
{
    public static class DynamoDBServiceCollectionExtensions
    {
        private const string REGION_SYSTEM_NAME_CONFIGURATION_KEY = "AmazonDynamoDB:RegionSystemName";

        public static IServiceCollection AddDynamoDB(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services.Configure<DynamoDBOptions>(options =>
            {
                options.RegionSystemName = configuration[REGION_SYSTEM_NAME_CONFIGURATION_KEY];
            });
            services.TryAddSingleton<IThreadStatsChangefeedDbService, ThreadStatsDynamoDBService>();

            return services;
        }
    }
}
