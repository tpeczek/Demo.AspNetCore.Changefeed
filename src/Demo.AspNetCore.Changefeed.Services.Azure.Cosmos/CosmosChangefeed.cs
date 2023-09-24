using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Cosmos;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Cosmos
{
    internal class CosmosChangefeed<T> : IChangefeed<T>
    {
        private static DateTime _startTime = DateTime.UtcNow;

        private readonly Container _container;
        private readonly TimeSpan _pollInterval;

        public CosmosChangefeed(Container container, TimeSpan pollInterval)
        {
            _container = container;
            _pollInterval = pollInterval;
        }

        public async IAsyncEnumerable<T> FetchFeed([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            FeedIterator<T> changeFeedIterator = _container.GetChangeFeedIterator<T>(ChangeFeedStartFrom.Time(_startTime), ChangeFeedMode.LatestVersion);

            while (changeFeedIterator.HasMoreResults && !cancellationToken.IsCancellationRequested)
            {
                FeedResponse<T> changeFeedResponse = await changeFeedIterator.ReadNextAsync(cancellationToken);

                if (changeFeedResponse.StatusCode == HttpStatusCode.NotModified)
                {
                    await Task.Delay(_pollInterval, cancellationToken);
                }
                else
                {
                    foreach (T item in changeFeedResponse)
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}
