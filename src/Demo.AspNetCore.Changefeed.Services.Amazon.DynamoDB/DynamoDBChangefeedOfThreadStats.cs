using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Amazon.DynamoDB
{
    internal class DynamoDBChangefeedOfThreadStats : IChangefeed<ThreadStats>
    {
        private readonly string _streamArn;
        private readonly TimeSpan _pollInterval;
        private readonly DynamoDBOptions _options;

        public DynamoDBChangefeedOfThreadStats(string streamArn, TimeSpan pollInterval, DynamoDBOptions options)
        {
            _streamArn = streamArn;
            _pollInterval = pollInterval;
            _options = options;
        }

        public async IAsyncEnumerable<ThreadStats> FetchFeed(CancellationToken cancellationToken = default)
        {
            AmazonDynamoDBStreamsConfig dynamoDBStreamsConfig = new AmazonDynamoDBStreamsConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(_options.RegionSystemName)
            };
            var dynamoDBStreamsClient = new AmazonDynamoDBStreamsClient(dynamoDBStreamsConfig);

            string exclusiveStartShardId = null;
            string exclusiveStartShardSequenceNumber = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                string lastEvaluatedShardId = exclusiveStartShardId;

                do
                {
                    var describeStreamResult = await dynamoDBStreamsClient.DescribeStreamAsync(new DescribeStreamRequest
                    {
                        StreamArn = _streamArn,
                        ExclusiveStartShardId = lastEvaluatedShardId
                    });

                    for (int shardIndex = 0; (shardIndex < describeStreamResult.StreamDescription.Shards.Count) && !cancellationToken.IsCancellationRequested; shardIndex++)
                    {
                        string shardId = describeStreamResult.StreamDescription.Shards[shardIndex].ShardId;
                        
                        var getShardIteratorResponse = await dynamoDBStreamsClient.GetShardIteratorAsync(new GetShardIteratorRequest
                        {
                            StreamArn = _streamArn,
                            ShardId = shardId,
                            SequenceNumber = exclusiveStartShardSequenceNumber,
                            ShardIteratorType = (exclusiveStartShardSequenceNumber is null) ? ShardIteratorType.TRIM_HORIZON : ShardIteratorType.AFTER_SEQUENCE_NUMBER
                        });
                        string shardIterator = getShardIteratorResponse.ShardIterator;

                        while ((shardIterator is not null) && !cancellationToken.IsCancellationRequested)
                        {
                            var getRecordsResponse = await dynamoDBStreamsClient.GetRecordsAsync(shardIterator);

                            if (getRecordsResponse.Records.Count == 0)
                            {
                                break;
                            }

                            for (int recordIndex = 0; (recordIndex < getRecordsResponse.Records.Count) && !cancellationToken.IsCancellationRequested; recordIndex++)
                            {
                                exclusiveStartShardSequenceNumber = getRecordsResponse.Records[recordIndex].Dynamodb.SequenceNumber;
                                yield return new ThreadStats
                                    {
                                        MaxThreads = Int32.Parse(getRecordsResponse.Records[recordIndex].Dynamodb.NewImage[nameof(ThreadStats.MaxThreads)].N),
                                        MinThreads = Int32.Parse(getRecordsResponse.Records[recordIndex].Dynamodb.NewImage[nameof(ThreadStats.MinThreads)].N),
                                        WorkerThreads = Int32.Parse(getRecordsResponse.Records[recordIndex].Dynamodb.NewImage[nameof(ThreadStats.WorkerThreads)].N)
                                    };                                    
                            }

                            shardIterator = getRecordsResponse.NextShardIterator;

                            if (shardIterator is null)
                            {
                                exclusiveStartShardId = shardId;
                                exclusiveStartShardSequenceNumber = null;
                            }
                        }
                    }

                    lastEvaluatedShardId = describeStreamResult.StreamDescription.LastEvaluatedShardId;

                } while ((lastEvaluatedShardId is not null) && !cancellationToken.IsCancellationRequested);

                await Task.Delay(_pollInterval, cancellationToken);
            }
        }
    }
}
