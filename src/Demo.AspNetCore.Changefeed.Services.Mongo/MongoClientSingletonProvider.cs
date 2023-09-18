using System;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Demo.AspNetCore.Changefeed.Services.Mongo
{
    internal class MongoClientSingletonProvider : IMongoClientSingletonProvider
    {
        public MongoClient MongoClientSingleton { get; }

        public MongoClientSingletonProvider(IOptions<MongoOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (String.IsNullOrWhiteSpace(options.Value.ConnectionString))
            {
                throw new ArgumentNullException(nameof(MongoOptions.ConnectionString));
            }

            MongoClientSingleton = new MongoClient(options.Value.ConnectionString);
        }
    }
}
