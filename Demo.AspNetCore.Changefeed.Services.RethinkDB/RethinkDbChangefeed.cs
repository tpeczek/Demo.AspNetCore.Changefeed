using System.Threading;
using System.Threading.Tasks;
using RethinkDb.Driver.Net;
using RethinkDb.Driver.Model;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.RethinkDB
{
    internal class RethinkDbChangefeed<T> : IChangefeed<T>
    {
        private readonly Cursor<Change<T>> _changefeed;

        public T CurrentNewValue { get { return _changefeed.Current.NewValue; } }

        public RethinkDbChangefeed(Cursor<Change<T>> changefeed)
        {
            _changefeed = changefeed;
        }

        public Task<bool> MoveNextAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            return _changefeed.MoveNextAsync(cancelToken);
        }
    }
}
