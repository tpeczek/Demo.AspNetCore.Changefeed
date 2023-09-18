using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using Demo.AspNetCore.Changefeed.Services.Abstractions;
using System.Runtime.CompilerServices;

namespace Demo.AspNetCore.Changefeed.Services.Mongo
{
    internal class MongoChangefeed<T> : IChangefeed<T>
    {
        private readonly IMongoCollection<T> _collection;
        private readonly TimeSpan _moveNextDelay;

        public MongoChangefeed(IMongoCollection<T> collection, TimeSpan moveNextDelay)
        {
            _collection = collection;
            _moveNextDelay = moveNextDelay;
        }

        public async IAsyncEnumerable<T> FetchFeed([EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            IAsyncCursor<ChangeStreamDocument<T>> changefeed = await _collection.WatchAsync(
                new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup },
                cancellationToken: cancellationToken
            );

            while (!cancellationToken.IsCancellationRequested)
            {
                while (await changefeed.MoveNextAsync(cancellationToken))
                {
                    IEnumerator<ChangeStreamDocument<T>>  changefeedCurrentEnumerator = changefeed.Current.GetEnumerator();

                    while (changefeedCurrentEnumerator.MoveNext())
                    {
                        if (changefeedCurrentEnumerator.Current.OperationType == ChangeStreamOperationType.Insert)
                        {
                            yield return changefeedCurrentEnumerator.Current.FullDocument;
                        }
                    }
                }

                await Task.Delay(_moveNextDelay, cancellationToken);
            }
        }
    }
}
