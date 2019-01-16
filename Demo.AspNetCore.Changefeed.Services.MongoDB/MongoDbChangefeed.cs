using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.MongoDB
{
    internal class MongoDbChangefeed<T> : IChangefeed<T>
    {
        private readonly IMongoCollection<T> _collection;
        private readonly TimeSpan _moveNextDelay;
        private IAsyncCursor<ChangeStreamDocument<T>> _changefeed;
        private IEnumerator<ChangeStreamDocument<T>> _changefeedCurrentEnumerator;

        public T CurrentNewValue { get; set; } = default(T);

        public MongoDbChangefeed(IMongoCollection<T> collection, TimeSpan moveNextDelay)
        {
            _collection = collection;
            _moveNextDelay = moveNextDelay;
        }

        public async Task<bool> MoveNextAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            while (!cancelToken.IsCancellationRequested)
            {
                if (MoveChangeFeedCurrentEnumeratorNext())
                {
                    return true;
                }

                if (await MoveChangeFeedNext(cancelToken))
                {
                    continue;
                }

                await Task.Delay(_moveNextDelay, cancelToken);
            }

            return false;
        }

        private bool MoveChangeFeedCurrentEnumeratorNext()
        {
            if (_changefeed is null)
            {
                return false;
            }

            if (_changefeedCurrentEnumerator is null)
            {
                _changefeedCurrentEnumerator = _changefeed.Current.GetEnumerator();
            }

            while (_changefeedCurrentEnumerator.MoveNext())
            {
                if (_changefeedCurrentEnumerator.Current.OperationType == ChangeStreamOperationType.Insert)
                {
                    CurrentNewValue = _changefeedCurrentEnumerator.Current.FullDocument;
                    return true;
                }
            }

            _changefeedCurrentEnumerator = null;

            return false;
        }

        private async Task<bool> MoveChangeFeedNext(CancellationToken cancelToken)
        {
            if (_changefeed is null)
            {
                _changefeed = await _collection.WatchAsync(
                    new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup },
                    cancellationToken: cancelToken
                );
            }

            return await _changefeed.MoveNextAsync(cancelToken);
        }
    }
}
