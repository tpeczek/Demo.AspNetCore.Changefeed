using MongoDB.Driver;

namespace Demo.AspNetCore.Changefeed.Services.Mongo
{
    internal interface IMongoClientSingletonProvider
    {
        MongoClient MongoClientSingleton { get; }
    }
}
