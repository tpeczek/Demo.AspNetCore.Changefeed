using MongoDB.Bson;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Mongo
{
    internal class MongoThreadStats : ThreadStats
    {
        public ObjectId Id { get; set; }

        public MongoThreadStats(ThreadStats threadStats)
        {
            WorkerThreads = threadStats.WorkerThreads;
            MinThreads = threadStats.MinThreads;
            MaxThreads = threadStats.MaxThreads;

        }
    }
}
