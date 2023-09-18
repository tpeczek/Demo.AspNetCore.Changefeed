using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Options;
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
        private const string THREAD_STATS_LEASE_CONTAINER_NAME = "ThreadStatsLease";
        private const string PARTITION_KEY_PROPERTY = "partionKey";
        private const string PARTITION_KEY_PATH = "/" + PARTITION_KEY_PROPERTY;
        private const string PARTITION_KEY_VALUE = "1";

        private readonly CosmosClient _cosmosClient;
        private readonly SemaphoreSlim _ensureDatabaseCreatedSemaphore = new SemaphoreSlim(1, 1);
        private Container _threadStatsContainer;
        private Container _threadStatsLeaseContainer;
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

        private Container ThreadStatsLeaseContainer
        {
            get
            {
                if (_threadStatsLeaseContainer is null)
                {
                    _threadStatsLeaseContainer = _cosmosClient.GetContainer(DATABASE_NAME, THREAD_STATS_LEASE_CONTAINER_NAME);
                }

                return _threadStatsLeaseContainer;
            }
        }

        public ThreadStatsCosmosService(IOptions<CosmosOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (String.IsNullOrWhiteSpace(options.Value.EndpointUrl))
            {
                throw new ArgumentNullException(nameof(CosmosOptions.EndpointUrl));
            }

            if (String.IsNullOrWhiteSpace(options.Value.AuthorizationKey))
            {
                throw new ArgumentNullException(nameof(CosmosOptions.AuthorizationKey));
            }

            CosmosClientBuilder clientBuilder = new CosmosClientBuilder(options.Value.EndpointUrl, options.Value.AuthorizationKey);
            _cosmosClient = clientBuilder
                .WithConnectionModeDirect()
                .Build();
        }

        public async Task EnsureDatabaseCreatedAsync()
        {
            await _ensureDatabaseCreatedSemaphore.WaitAsync();

            Database database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(DATABASE_NAME);
            await database.CreateContainerIfNotExistsAsync(THREAD_STATS_CONTAINER_NAME, PARTITION_KEY_PATH);
            await database.CreateContainerIfNotExistsAsync(THREAD_STATS_LEASE_CONTAINER_NAME, "/id");

            _ensureDatabaseCreatedSemaphore.Release();
        }

        public Task<IChangefeed<ThreadStats>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IChangefeed<ThreadStats>>(new CosmosChangefeed<ThreadStatsItem>(
                ThreadStatsContainer,
                ThreadStatsLeaseContainer,
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
