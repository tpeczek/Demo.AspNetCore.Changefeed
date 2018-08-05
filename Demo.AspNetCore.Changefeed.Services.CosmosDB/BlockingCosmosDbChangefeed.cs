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
    internal class BlockingCosmosDbChangefeed<T> : IChangefeed<T>
    {
        private static Task<bool> _taskFromTrue = Task.FromResult(true);
        private static Task<bool> _taskFromFalse = Task.FromResult(false);

        private static DateTime _startTime = DateTime.Now;

        private readonly DocumentClient _documentClient;
        
        private readonly TimeSpan _feedPollDelay;

        private readonly Uri _collectionUri;
        private readonly Dictionary<string, string> _collectionPartitionKeyRangesCheckpoints = new Dictionary<string, string>();

        private IEnumerator<T> _changefeedEnumerator;

        public T CurrentNewValue { get; set; } = default(T);

        public BlockingCosmosDbChangefeed(DocumentClient documentClient, Uri collectionUri, TimeSpan feedPollDelay)
        {
            _documentClient = documentClient;
            _collectionUri = collectionUri;
            _feedPollDelay = feedPollDelay;
        }

        public Task<bool> MoveNextAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            if (_changefeedEnumerator == null)
            {
                _changefeedEnumerator = GetChangefeed(cancelToken).GetEnumerator();
            }

            if (_changefeedEnumerator.MoveNext())
            {
                CurrentNewValue = _changefeedEnumerator.Current;
                return _taskFromTrue;
            }

            return _taskFromFalse;
        }

        private async Task<List<PartitionKeyRange>> GetCollectionPartitionKeyRanges()
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

            return collectionPartitionKeyRanges;
        }

        private IEnumerable<T> GetChangefeed(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                foreach (PartitionKeyRange collectionPartitionKeyRange in GetCollectionPartitionKeyRanges().Result)
                {
                    if (!cancelToken.IsCancellationRequested)
                    {
                        string collectionPartitionKeyRangeCheckpoint = null;
                        _collectionPartitionKeyRangesCheckpoints.TryGetValue(collectionPartitionKeyRange.Id, out collectionPartitionKeyRangeCheckpoint);

                        IDocumentQuery<Document> collectionChangeFeedQuery = _documentClient.CreateDocumentChangeFeedQuery(_collectionUri, new ChangeFeedOptions
                        {
                            PartitionKeyRangeId = collectionPartitionKeyRange.Id,
                            RequestContinuation = collectionPartitionKeyRangeCheckpoint,
                            MaxItemCount = -1,
                            StartTime = _startTime
                        });

                        while (collectionChangeFeedQuery.HasMoreResults && !cancelToken.IsCancellationRequested)
                        {
                            FeedResponse<T> collectionChangeFeedResponse = collectionChangeFeedQuery.ExecuteNextAsync<T>(cancelToken).Result;
                            _collectionPartitionKeyRangesCheckpoints[collectionPartitionKeyRange.Id] = collectionChangeFeedResponse.ResponseContinuation;

                            foreach (T changedDocument in collectionChangeFeedResponse)
                            {
                                if (cancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                yield return changedDocument;
                            }
                        }
                    }
                }

                if (!cancelToken.IsCancellationRequested)
                {
                    Task.Delay(_feedPollDelay, cancelToken).Wait();
                }
            }
        }
    }
}
