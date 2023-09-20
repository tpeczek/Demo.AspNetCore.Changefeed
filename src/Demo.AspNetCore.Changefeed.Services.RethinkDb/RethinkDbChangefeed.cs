using System.Threading;
using RethinkDb.Driver.Net;
using RethinkDb.Driver.Model;
using Demo.AspNetCore.Changefeed.Services.Abstractions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Demo.AspNetCore.Changefeed.Services.RethinkDb
{
    internal class RethinkDbChangefeed<T> : IChangefeed<T>
    {
        private readonly Cursor<Change<T>> _changefeed;

        public RethinkDbChangefeed(Cursor<Change<T>> changefeed)
        {
            _changefeed = changefeed;
        }

        public async IAsyncEnumerable<T> FetchFeed([EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            while (await _changefeed.MoveNextAsync(cancellationToken))
            {
                yield return _changefeed.Current.NewValue;
            }
        }
    }
}
