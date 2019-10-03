using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents.Client;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.CosmosDB
{
    internal class CosmosDbChangefeed<T> : IChangefeed<T>
    {
        private static DateTime _startTime = DateTime.Now;

        private readonly DocumentClient _documentClient;
        private readonly TimeSpan _feedPollDelay;

        private readonly Uri _collectionUri;
        private IEnumerator<T> _collectionChangeFeedEnumerator;
        private IDocumentQuery<Document> _collectionChangeFeedQuery;
        private int _collectionPartitionKeyRangeIndex;
        private List<PartitionKeyRange> _collectionPartitionKeyRanges;
        private readonly Dictionary<string, string> _collectionPartitionKeyRangesCheckpoints = new Dictionary<string, string>();

        public CosmosDbChangefeed(DocumentClient documentClient, Uri collectionUri, TimeSpan feedPollDelay)
        {
            _documentClient = documentClient;
            _collectionUri = collectionUri;
            _feedPollDelay = feedPollDelay;
        }

        public async IAsyncEnumerable<T> FetchFeed([EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await ReadCollectionPartitionKeyRanges();

                while (CreateDocumentChangeFeedQueryForNextPartitionKeyRange())
                {
                    while (await ExecuteCollectionChangeFeedQueryNextResultAsync(cancellationToken))
                    {
                        while (MoveCollectionChangeFeedEnumeratorNext())
                        {
                            yield return _collectionChangeFeedEnumerator.Current;
                        }
                    }
                }

                await WaitForNextPoll(cancellationToken);
            }
        }

        private async Task ReadCollectionPartitionKeyRanges()
        {
            List<PartitionKeyRange> collectionPartitionKeyRanges = new List<PartitionKeyRange>();

            string collectionPartitionKeyRangesResponseContinuation = null;
            do
            {
                FeedResponse<PartitionKeyRange> collectionPartitionKeyRangesResponse = await _documentClient.ReadPartitionKeyRangeFeedAsync(_collectionUri, new FeedOptions
                {
                    RequestContinuation = collectionPartitionKeyRangesResponseContinuation
                });

                collectionPartitionKeyRanges.AddRange(collectionPartitionKeyRangesResponse);
                collectionPartitionKeyRangesResponseContinuation = collectionPartitionKeyRangesResponse.ResponseContinuation;
            }
            while (collectionPartitionKeyRangesResponseContinuation != null);

            _collectionPartitionKeyRanges = collectionPartitionKeyRanges;
            _collectionPartitionKeyRangeIndex = -1;
        }

        private bool CreateDocumentChangeFeedQueryForNextPartitionKeyRange()
        {
            if ((++_collectionPartitionKeyRangeIndex) < _collectionPartitionKeyRanges.Count)
            {
                string collectionPartitionKeyRangeCheckpoint = null;
                _collectionPartitionKeyRangesCheckpoints.TryGetValue(_collectionPartitionKeyRanges[_collectionPartitionKeyRangeIndex].Id, out collectionPartitionKeyRangeCheckpoint);

                _collectionChangeFeedQuery = _documentClient.CreateDocumentChangeFeedQuery(_collectionUri, new ChangeFeedOptions
                {
                    PartitionKeyRangeId = _collectionPartitionKeyRanges[_collectionPartitionKeyRangeIndex].Id,
                    RequestContinuation = collectionPartitionKeyRangeCheckpoint,
                    MaxItemCount = -1,
                    StartTime = _startTime
                });

                return true;
            }

            return false;
        }

        private async Task<bool> ExecuteCollectionChangeFeedQueryNextResultAsync(CancellationToken cancellationToken)
        {
            if (_collectionChangeFeedQuery.HasMoreResults && !cancellationToken.IsCancellationRequested)
            {
                FeedResponse<T> collectionChangeFeedResponse = await _collectionChangeFeedQuery.ExecuteNextAsync<T>(cancellationToken);
                _collectionPartitionKeyRangesCheckpoints[_collectionPartitionKeyRanges[_collectionPartitionKeyRangeIndex].Id] = collectionChangeFeedResponse.ResponseContinuation;

                _collectionChangeFeedEnumerator = collectionChangeFeedResponse.GetEnumerator();

                return true;
            }

            return false;
        }

        private bool MoveCollectionChangeFeedEnumeratorNext()
        {
            if (_collectionChangeFeedEnumerator.MoveNext())
            {
                return true;
            }

            _collectionChangeFeedEnumerator.Dispose();
            _collectionChangeFeedEnumerator = null;

            return false;
        }

        private Task WaitForNextPoll(CancellationToken cancellationToken)
        {
            if ((_collectionPartitionKeyRanges != null) && !cancellationToken.IsCancellationRequested)
            {
                return Task.Delay(_feedPollDelay, cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}
