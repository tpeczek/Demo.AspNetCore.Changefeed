using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Mongo
{
    internal class ThreadStatsMongoService : IThreadStatsChangefeedDbService
    {
        private const string DATABASE_NAME = "Demo_AspNetCore_Changefeed_MongoDB";
        private const string THREAD_STATS_COLLECTION_NAME = "ThreadStats";

        private readonly MongoClient _mongoClientSingleton;
        private readonly IMongoDatabase _threadStatsDatabase;
        private readonly IMongoCollection<MongoThreadStats> _threadStatsCollection;

        public ThreadStatsMongoService(IMongoClientSingletonProvider mongoClientSingletonProvider)
        {
            if (mongoClientSingletonProvider == null)
            {
                throw new ArgumentNullException(nameof(mongoClientSingletonProvider));
            }

            _mongoClientSingleton = mongoClientSingletonProvider.MongoClientSingleton;
            _threadStatsDatabase = _mongoClientSingleton.GetDatabase(DATABASE_NAME);
            _threadStatsCollection = _threadStatsDatabase.GetCollection<MongoThreadStats>(THREAD_STATS_COLLECTION_NAME);
        }

        public Task EnsureDatabaseCreatedAsync()
        {
            return Task.CompletedTask;
        }

        public Task<IChangefeed<ThreadStats>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IChangefeed<ThreadStats>>(new MongoChangefeed<MongoThreadStats>(
                _threadStatsCollection,
                TimeSpan.FromSeconds(1)
            ));
        }

        public Task InsertThreadStatsAsync(ThreadStats threadStats)
        {
            return _threadStatsCollection.InsertOneAsync(new MongoThreadStats(threadStats));
        }
    }
}
