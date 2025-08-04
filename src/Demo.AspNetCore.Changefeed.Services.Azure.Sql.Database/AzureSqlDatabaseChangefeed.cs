using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Sql.Database
{
    internal class AzureSqlDatabaseChangefeed<T> : IChangefeed<T>
    {
        private static readonly string ALTER_TABLE_ENABLE_CHANGE_TRACKING = "ALTER TABLE [{0}].[{1}] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON)";
        private static readonly string ALTER_TABLE_DISABLE_CHANGE_TRACKING = "ALTER TABLE [{0}].[{1}] DISABLE CHANGE_TRACKING";
        private static readonly string SELECT_CHANGE_TRACKING_CURRENT_VERSION = "SELECT CHANGE_TRACKING_CURRENT_VERSION() AS [Value]";
        private static readonly string SELECT_CHANGES = "SELECT T.* FROM [{0}].[{1}] T INNER JOIN CHANGETABLE(CHANGES [{0}].[{1}], {3}) AS CT ON T.[{2}] = CT.[{2}]";

        private readonly string _schema;
        private readonly string _tableName;
        private readonly string _primaryKeyColumnName;
        private readonly Type _dbContextType;
        private readonly AzureSqlDatabaseOptions _options;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public AzureSqlDatabaseChangefeed(string schema, string tableName, string primaryKeyColumnName, Type dbContextType, AzureSqlDatabaseOptions options, IServiceScopeFactory serviceScopeFactory)
        {
            _schema = schema;
            _tableName = tableName;
            _primaryKeyColumnName = primaryKeyColumnName;
            _dbContextType = dbContextType;
            _options = options;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async IAsyncEnumerable<T> FetchFeed([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var serviceScope = _serviceScopeFactory.CreateScope();
            DbContext context = (DbContext)serviceScope.ServiceProvider.GetRequiredService(_dbContextType);

            await context.Database.ExecuteSqlRawAsync(String.Format(ALTER_TABLE_ENABLE_CHANGE_TRACKING, _schema, _tableName));
            long synchronizationVersion = await context.Database.SqlQueryRaw<long>(SELECT_CHANGE_TRACKING_CURRENT_VERSION).FirstAsync();

            while (!cancellationToken.IsCancellationRequested)
            {
                long nextSynchronizationVersion = await context.Database.SqlQueryRaw<long>(SELECT_CHANGE_TRACKING_CURRENT_VERSION).FirstAsync();

                await foreach (T change in context.Database.SqlQueryRaw<T>(String.Format(SELECT_CHANGES, _schema, _tableName, _primaryKeyColumnName, synchronizationVersion)).AsAsyncEnumerable())
                {
                    yield return change;
                }

                synchronizationVersion = nextSynchronizationVersion;

                await Task.Delay(_options.PollInterval, cancellationToken);
            }

            context.Database.ExecuteSqlRaw(String.Format(ALTER_TABLE_DISABLE_CHANGE_TRACKING, _schema, _tableName));

            yield break;
        }
    }
}
