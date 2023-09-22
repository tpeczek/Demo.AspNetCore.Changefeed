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
        private readonly BlobServiceClient _blobServiceClient;

        public ThreadStatsBlobService(IOptions<BlobOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            TokenCredential azureCredential = new DefaultAzureCredential();
            _blobServiceClient = new(_options.ServiceUri, azureCredential);
        }

        public Task EnsureDatabaseCreatedAsync()
        {
            BlobContainerClient threadStatsContainerClient = _blobServiceClient.GetBlobContainerClient(THREAD_STATS_CONTAINER_NAME);

            return threadStatsContainerClient.CreateIfNotExistsAsync();
        }

        public Task<IChangefeed<ThreadStats>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IChangefeed<ThreadStats>>(new BlobChangefeed<ThreadStats>(
                THREAD_STATS_CONTAINER_NAME,
                TimeSpan.FromSeconds(1),
                _blobServiceClient
            ));
        }

        public Task InsertThreadStatsAsync(ThreadStats threadStats)
        {
            BlobContainerClient threadStatsContainerClient = _blobServiceClient.GetBlobContainerClient(THREAD_STATS_CONTAINER_NAME);
            BlobClient threadStatsBlobClient = threadStatsContainerClient.GetBlobClient(Guid.NewGuid().ToString());

            return threadStatsBlobClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(threadStats))));
        }
    }
}
