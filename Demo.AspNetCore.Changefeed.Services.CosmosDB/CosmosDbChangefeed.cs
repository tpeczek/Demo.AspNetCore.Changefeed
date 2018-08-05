using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
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

        public T CurrentNewValue { get; set; } = default(T);

        public CosmosDbChangefeed(DocumentClient documentClient, Uri collectionUri, TimeSpan feedPollDelay)
        {
            _documentClient = documentClient;
            _collectionUri = collectionUri;
            _feedPollDelay = feedPollDelay;
        }

        public async Task<bool> MoveNextAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            while (!cancelToken.IsCancellationRequested)
            {
                if (MoveCollectionChangeFeedEnumeratorNext())
                {
                    return true;
                }

                if (await ExecuteCollectionChangeFeedQueryNextResultAsync(cancelToken))
                {
                    continue;
                }

                if (CreateDocumentChangeFeedQueryForNextPartitionKeyRange(cancelToken))
                {
                    continue;
                }

                await WaitForNextPoll(cancelToken);

                await ReadCollectionPartitionKeyRanges(cancelToken);
            }

            return false;
        }

        private bool MoveCollectionChangeFeedEnumeratorNext()
        {
            if (_collectionChangeFeedEnumerator != null)
            {
                if (_collectionChangeFeedEnumerator.MoveNext())
                {
                    CurrentNewValue = _collectionChangeFeedEnumerator.Current;
                    return true;
                }

                _collectionChangeFeedEnumerator.Dispose();
                _collectionChangeFeedEnumerator = null;
            }

            return false;
        }

        private async Task<bool> ExecuteCollectionChangeFeedQueryNextResultAsync(CancellationToken cancelToken)
        {
            if ((_collectionChangeFeedQuery != null) && _collectionChangeFeedQuery.HasMoreResults && !cancelToken.IsCancellationRequested)
            {
                FeedResponse<T> collectionChangeFeedResponse = await _collectionChangeFeedQuery.ExecuteNextAsync<T>(cancelToken);
                _collectionPartitionKeyRangesCheckpoints[_collectionPartitionKeyRanges[_collectionPartitionKeyRangeIndex].Id] = collectionChangeFeedResponse.ResponseContinuation;

                _collectionChangeFeedEnumerator = collectionChangeFeedResponse.GetEnumerator();

                return true;
            }

            return false;
        }

        private bool CreateDocumentChangeFeedQueryForNextPartitionKeyRange(CancellationToken cancelToken)
        {
            if ((_collectionPartitionKeyRanges != null) && ((++_collectionPartitionKeyRangeIndex) < _collectionPartitionKeyRanges.Count) && !cancelToken.IsCancellationRequested)
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

        private Task WaitForNextPoll(CancellationToken cancelToken)
        {
            if ((_collectionPartitionKeyRanges != null) && !cancelToken.IsCancellationRequested)
            {
                return Task.Delay(_feedPollDelay, cancelToken);
            }

            return Task.CompletedTask;
        }

        private async Task ReadCollectionPartitionKeyRanges(CancellationToken cancelToken)
        {
            if (!cancelToken.IsCancellationRequested)
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
        }
    }
}
