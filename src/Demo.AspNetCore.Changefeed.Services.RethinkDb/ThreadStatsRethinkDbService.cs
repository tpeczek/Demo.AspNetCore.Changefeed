using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RethinkDb.Driver.Net;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.RethinkDb
{
    internal class ThreadStatsRethinkDbService : IThreadStatsChangefeedDbService, IDisposable
    {
        private const string DATABASE_NAME = "Demo_AspNetCore_Changefeed_RethinkDB";
        private const string THREAD_STATS_TABLE_NAME = "ThreadStats";

        private readonly RethinkDbOptions _options;
        private readonly global::RethinkDb.Driver.RethinkDB _rethinkDbSingleton;
        private readonly Connection _rethinkDbConnection;

        private bool _disposed = false;

        public ThreadStatsRethinkDbService(IOptions<RethinkDbOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            _rethinkDbSingleton = global::RethinkDb.Driver.RethinkDB.R;

            var rethinkDbConnectionBuilder = _rethinkDbSingleton.Connection().Hostname(options.Value.Hostname);

            if (_options.DriverPort.HasValue)
            {
                rethinkDbConnectionBuilder.Port(options.Value.DriverPort.Value);
            }

            if (_options.Timeout.HasValue)
            {
                rethinkDbConnectionBuilder.Timeout(options.Value.Timeout.Value);
            }

            _rethinkDbConnection = rethinkDbConnectionBuilder.Connect();
        }

        public Task EnsureDatabaseCreatedAsync()
        {
            if (!((string[])_rethinkDbSingleton.DbList().Run(_rethinkDbConnection).ToObject<string[]>()).Contains(DATABASE_NAME))
            {
                _rethinkDbSingleton.DbCreate(DATABASE_NAME).Run(_rethinkDbConnection);
            }

            var database = _rethinkDbSingleton.Db(DATABASE_NAME);
            if (!((string[])database.TableList().Run(_rethinkDbConnection).ToObject<string[]>()).Contains(THREAD_STATS_TABLE_NAME))
            {
                database.TableCreate(THREAD_STATS_TABLE_NAME).Run(_rethinkDbConnection);
            }

            return Task.CompletedTask;
        }

        public Task InsertThreadStatsAsync(ThreadStats threadStats)
        {
            _rethinkDbSingleton.Db(DATABASE_NAME).Table(THREAD_STATS_TABLE_NAME).Insert(threadStats).Run(_rethinkDbConnection);

            return Task.CompletedTask;
        }

        public async Task<IChangefeed<ThreadStats>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken)
        {
            return new RethinkDbChangefeed<ThreadStats>(
                await _rethinkDbSingleton.Db(DATABASE_NAME).Table(THREAD_STATS_TABLE_NAME).Changes().RunChangesAsync<ThreadStats>(_rethinkDbConnection, cancellationToken)
            );
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _rethinkDbConnection.Dispose();

                GC.SuppressFinalize(this);

                _disposed = true;
            }
        }
    }
}
