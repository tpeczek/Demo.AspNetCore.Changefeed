using MongoDB.Driver;

namespace Demo.AspNetCore.Changefeed.Services.MongoDB
{
    internal interface IMongoClientSingletonProvider
    {
        MongoClient MongoClientSingleton { get; }
    }
}
