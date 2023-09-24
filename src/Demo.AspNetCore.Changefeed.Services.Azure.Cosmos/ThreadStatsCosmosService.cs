using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Azure.Core;
using Azure.Identity;
using Newtonsoft.Json;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Cosmos
{
    internal class ThreadStatsCosmosService : IThreadStatsChangefeedDbService, IDisposable
    {
        private class ThreadStatsItem : ThreadStats
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = PARTITION_KEY_PROPERTY)]
            public string Partition { get; set; }

            public ThreadStatsItem()
            { }

            public ThreadStatsItem(ThreadStats threadStats)
            {
                Id = Guid.NewGuid().ToString();
                Partition = PARTITION_KEY_VALUE;
                MaxThreads = threadStats.MaxThreads;
                MinThreads = threadStats.MinThreads;
                WorkerThreads = threadStats.WorkerThreads;
            }
        }

        private const string DATABASE_NAME = "Demo_AspNetCore_Changefeed_CosmosDB";
        private const string THREAD_STATS_CONTAINER_NAME = "ThreadStats";
        private const string PARTITION_KEY_PROPERTY = "partionKey";
        private const string PARTITION_KEY_PATH = "/" + PARTITION_KEY_PROPERTY;
        private const string PARTITION_KEY_VALUE = "1";

        private readonly CosmosOptions _options;
        private readonly CosmosClient _cosmosClient;
        private readonly SemaphoreSlim _ensureDatabaseCreatedSemaphore = new SemaphoreSlim(1, 1);
        private Container _threadStatsContainer;
        private readonly PartitionKey _partitionKey = new PartitionKey(PARTITION_KEY_VALUE);

        private bool _disposed = false;

        private Container ThreadStatsContainer
        {
            get
            {
                if (_threadStatsContainer is null)
                {
                    _threadStatsContainer = _cosmosClient.GetContainer(DATABASE_NAME, THREAD_STATS_CONTAINER_NAME);
                }

                return _threadStatsContainer;
            }
        }

        public ThreadStatsCosmosService(IOptions<CosmosOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            TokenCredential azureCredential = new DefaultAzureCredential();
            _cosmosClient = new CosmosClient(_options.DocumentEndpoint, azureCredential, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct
            });
        }

        public async Task EnsureDatabaseCreatedAsync()
        {
            await _ensureDatabaseCreatedSemaphore.WaitAsync();

            Database database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(DATABASE_NAME);
            await database.CreateContainerIfNotExistsAsync(THREAD_STATS_CONTAINER_NAME, PARTITION_KEY_PATH);

            _ensureDatabaseCreatedSemaphore.Release();
        }

        public Task<IChangefeed<ThreadStats>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IChangefeed<ThreadStats>>(new CosmosChangefeed<ThreadStatsItem>(
                ThreadStatsContainer,
                TimeSpan.FromSeconds(1)
            ));
        }

        public Task InsertThreadStatsAsync(ThreadStats threadStats)
        {
            return ThreadStatsContainer.CreateItemAsync<ThreadStatsItem>(new ThreadStatsItem(threadStats), _partitionKey);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cosmosClient.Dispose();

                GC.SuppressFinalize(this);

                _disposed = true;
            }
        }
    }
}
