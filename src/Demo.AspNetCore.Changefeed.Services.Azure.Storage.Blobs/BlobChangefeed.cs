using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.ChangeFeed;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Storage.Blobs
{
    internal class BlobChangefeed<T> : IChangefeed<T>
    {
        private readonly string _container;
        private readonly TimeSpan _pollInterval;
        private readonly BlobServiceClient _blobServiceClient;
        
        public BlobChangefeed(string container, TimeSpan pollInterval, BlobServiceClient blobServiceClient)
        {
            _container = container;
            _pollInterval = pollInterval;
            _blobServiceClient = blobServiceClient;
        }

        public async IAsyncEnumerable<T> FetchFeed([EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            string? continuationToken = null;

            TokenCredential azureCredential = new DefaultAzureCredential();

            BlobChangeFeedClient changeFeedClient = _blobServiceClient.GetChangeFeedClient();

            while (!cancellationToken.IsCancellationRequested)
            {
                IAsyncEnumerator<Page<BlobChangeFeedEvent>> changeFeedEnumerator = changeFeedClient
                    .GetChangesAsync(continuationToken)
                    .AsPages()
                    .GetAsyncEnumerator();

                while (await changeFeedEnumerator.MoveNextAsync())
                {
                    foreach (BlobChangeFeedEvent changeFeedEvent in changeFeedEnumerator.Current.Values)
                    {
                        if ((changeFeedEvent.EventType == BlobChangeFeedEventType.BlobCreated) && changeFeedEvent.Subject.StartsWith($"/blobServices/default/containers/{_container}"))
                        {
                            BlobClient createdBlobClient = new BlobClient(changeFeedEvent.EventData.Uri, azureCredential);
                            if (await createdBlobClient.ExistsAsync())
                            {
                                MemoryStream blobContentStream = new MemoryStream((int)changeFeedEvent.EventData.ContentLength);
                                await createdBlobClient.DownloadToAsync(blobContentStream);
                                blobContentStream.Seek(0, SeekOrigin.Begin);

                                yield return JsonSerializer.Deserialize<T>(blobContentStream);
                            }
                        }
                    }

                    continuationToken = changeFeedEnumerator.Current.ContinuationToken;
                }

                await Task.Delay(_pollInterval, cancellationToken);
            }
        }
    }
}
