using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Sql.Database
{
    public static class SqlServiceCollectionExtensions
    {
        private const string CONNECTION_STRING_CONFIGURATION_KEY = "AzureSqlDatabase:ConnectionString";

        public static IServiceCollection AddAzureSqlDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            string? connectionString = configuration[CONNECTION_STRING_CONFIGURATION_KEY];

            services.Configure<AzureSqlDatabaseOptions>(options =>
            {
                options.PollInterval = TimeSpan.FromSeconds(1);
                options.ConnectionString = connectionString;
            });

            services.AddDbContext<ThreadStatsDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.TryAddSingleton<IThreadStatsChangefeedDbService, ThreadStatsAzureSqlDatabaseService>();

            return services;
        }
    }
}
