using System.Threading;
using System.Threading.Tasks;
using RethinkDb.Driver.Net;
using RethinkDb.Driver.Model;

namespace Demo.AspNetCore.RethinkDB.Services
{
    internal interface IRethinkDbService
    {
        void EnsureDatabaseCreated();

        void InsertThreadStats(ThreadStats threadStats);

        Task<Cursor<Change<ThreadStats>>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken);
    }
}
