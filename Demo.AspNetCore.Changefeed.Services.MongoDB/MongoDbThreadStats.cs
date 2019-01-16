using MongoDB.Bson;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.MongoDB
{
    internal class MongoDbThreadStats : ThreadStats
    {
        public ObjectId Id { get; set; }

        public MongoDbThreadStats(ThreadStats threadStats)
        {
            WorkerThreads = threadStats.WorkerThreads;
            MinThreads = threadStats.MinThreads;
            MaxThreads = threadStats.MaxThreads;

        }
    }
}
