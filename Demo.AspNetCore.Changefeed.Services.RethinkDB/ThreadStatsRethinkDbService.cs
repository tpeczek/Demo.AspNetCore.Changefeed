using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RethinkDb.Driver.Net;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.RethinkDB
{
    internal class ThreadStatsRethinkDbService : IThreadStatsChangefeedDbService
    {
        private const string DATABASE_NAME = "Demo_AspNetCore_RethinkDB";
        private const string THREAD_STATS_TABLE_NAME = "ThreadStats";

        private readonly RethinkDb.Driver.RethinkDB _rethinkDbSingleton;
        private readonly Connection _rethinkDbConnection;

        public ThreadStatsRethinkDbService(IRethinkDbSingletonProvider rethinkDbSingletonProvider)
        {
            if (rethinkDbSingletonProvider == null)
            {
                throw new ArgumentNullException(nameof(rethinkDbSingletonProvider));
            }

            _rethinkDbSingleton = rethinkDbSingletonProvider.RethinkDbSingleton;
            _rethinkDbConnection = rethinkDbSingletonProvider.RethinkDbConnection;
        }

        public void EnsureDatabaseCreated()
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
        }

        public void InsertThreadStats(ThreadStats threadStats)
        {
            _rethinkDbSingleton.Db(DATABASE_NAME).Table(THREAD_STATS_TABLE_NAME).Insert(threadStats).Run(_rethinkDbConnection);
        }

        public async Task<IChangefeed<ThreadStats>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken)
        {
            return new RethinkDbChangefeed<ThreadStats>(
                await _rethinkDbSingleton.Db(DATABASE_NAME).Table(THREAD_STATS_TABLE_NAME).Changes().RunChangesAsync<ThreadStats>(_rethinkDbConnection, cancellationToken)
            );
        }
    }
}
