using System.Threading;
using System.Threading.Tasks;
using RethinkDb.Driver.Net;
using RethinkDb.Driver.Model;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.RethinkDB
{
    internal class ThreadStatsRethinkDbChangefeed : IThreadStatsChangefeed
    {
        private readonly Cursor<Change<ThreadStats>> _changefeed;

        public ThreadStats CurrentNewValue { get { return _changefeed.Current.NewValue; } }

        public ThreadStatsRethinkDbChangefeed(Cursor<Change<ThreadStats>> changefeed)
        {
            _changefeed = changefeed;
        }

        public Task<bool> MoveNextAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            return _changefeed.MoveNextAsync(cancelToken);
        }
    }
}
