using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Amazon.DynamoDB
{
    internal class ThreadStatsDynamoDBService : IThreadStatsChangefeedDbService, IDisposable
    {
        private const string THREAD_STATS_TABLE_NAME = "ThreadStats";
        private const string PARTITION_KEY_ATTRUBUTE = "PartionKey";
        private const string PARTITION_KEY_VALUE = "1";

        private readonly DynamoDBOptions _options;
        private readonly AmazonDynamoDBClient _dynamoDBClient;
        private readonly SemaphoreSlim _ensureDatabaseCreatedSemaphore = new SemaphoreSlim(1, 1);

        private bool _disposed = false;

        public ThreadStatsDynamoDBService(IOptions<DynamoDBOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            AmazonDynamoDBConfig dynamoDBClientConfig = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(_options.RegionSystemName)
            };
            _dynamoDBClient = new AmazonDynamoDBClient(dynamoDBClientConfig);
        }

        public async Task EnsureDatabaseCreatedAsync()
        {
            await _ensureDatabaseCreatedSemaphore.WaitAsync();

            try
            {
                _ = await _dynamoDBClient.DescribeTableAsync(THREAD_STATS_TABLE_NAME);
            }
            catch (ResourceNotFoundException)
            {
                var createTableRequest = new CreateTableRequest(
                    THREAD_STATS_TABLE_NAME,
                    [
                        new () { AttributeName = PARTITION_KEY_ATTRUBUTE, KeyType = KeyType.HASH }
                    ],
                    [
                        new () { AttributeName = PARTITION_KEY_ATTRUBUTE, AttributeType = ScalarAttributeType.S },
                    ],
                    new ProvisionedThroughput { ReadCapacityUnits = 1, WriteCapacityUnits = 1 }
                )
                {
                    StreamSpecification = new StreamSpecification { StreamEnabled = true, StreamViewType = StreamViewType.NEW_IMAGE }
                };

                _ = await _dynamoDBClient.CreateTableAsync(createTableRequest);

                TableStatus status;
                do
                {
                    await Task.Delay(2000);

                    var describeTableResponse = await _dynamoDBClient.DescribeTableAsync(THREAD_STATS_TABLE_NAME);
                    status = describeTableResponse.Table.TableStatus;
                }
                while (status != TableStatus.ACTIVE);
            }

            _ensureDatabaseCreatedSemaphore.Release();
        }

        public async Task<IChangefeed<ThreadStats>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken)
        {
            var describeTableResponse = await _dynamoDBClient.DescribeTableAsync(THREAD_STATS_TABLE_NAME);

            return new DynamoDBChangefeedOfThreadStats(
                describeTableResponse.Table.LatestStreamArn,
                TimeSpan.FromSeconds(1),
                _options
            );
        }

        public async Task InsertThreadStatsAsync(ThreadStats threadStats)
        {
            _ = await _dynamoDBClient.PutItemAsync(
                THREAD_STATS_TABLE_NAME,
                new Dictionary<string, AttributeValue>
                {
                    [PARTITION_KEY_ATTRUBUTE] = new AttributeValue { S = PARTITION_KEY_VALUE },
                    [nameof(ThreadStats.MaxThreads)] = new AttributeValue { N = threadStats.MaxThreads.ToString() },
                    [nameof(ThreadStats.MinThreads)] = new AttributeValue { N = threadStats.MinThreads.ToString() },
                    [nameof(ThreadStats.WorkerThreads)] = new AttributeValue { N = threadStats.WorkerThreads.ToString() }
                }
            );
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _dynamoDBClient.Dispose();

                GC.SuppressFinalize(this);

                _disposed = true;
            }
        }
    }
}
