using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Storage.Blobs
{
    internal class ThreadStatsBlobService : IThreadStatsChangefeedDbService
    {
        private const string THREAD_STATS_CONTAINER_NAME = "threadstats";

        private readonly BlobOptions _options;
        private readonly Lazy<BlobServiceClient> _blobServiceClientFactory;

        public ThreadStatsBlobService(IOptions<BlobOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _blobServiceClientFactory = new Lazy<BlobServiceClient>(GetBlobServiceClient);
        }

        public Task EnsureDatabaseCreatedAsync()
        {
            BlobContainerClient threadStatsContainerClient = _blobServiceClientFactory.Value.GetBlobContainerClient(THREAD_STATS_CONTAINER_NAME);

            return threadStatsContainerClient.CreateIfNotExistsAsync();
        }

        public Task<IChangefeed<ThreadStats>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IChangefeed<ThreadStats>>(new BlobChangefeed<ThreadStats>(
                THREAD_STATS_CONTAINER_NAME,
                TimeSpan.FromSeconds(1),
                _blobServiceClientFactory.Value
            ));
        }

        public Task InsertThreadStatsAsync(ThreadStats threadStats)
        {
            BlobContainerClient threadStatsContainerClient = _blobServiceClientFactory.Value.GetBlobContainerClient(THREAD_STATS_CONTAINER_NAME);
            BlobClient threadStatsBlobClient = threadStatsContainerClient.GetBlobClient(Guid.NewGuid().ToString());

            return threadStatsBlobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(threadStats))));
        }

        private BlobServiceClient GetBlobServiceClient()
        {
            TokenCredential azureCredential = new DefaultAzureCredential();

            BlobServiceClient blobServiceClient = new (_options.ServiceUri, azureCredential);

            return blobServiceClient;
        }
    }
}
