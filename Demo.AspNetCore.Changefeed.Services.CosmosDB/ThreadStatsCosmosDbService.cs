using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.CosmosDB
{
    internal class ThreadStatsCosmosDbService : IThreadStatsChangefeedDbService
    {
        private const string DATABASE_ID = "Demo_AspNetCore_Changefeed_CosmosDB";
        private const string THREAD_STATS_COLLECTION_ID = "ThreadStats";

        private readonly DocumentClient _documentClientSingleton;
        private readonly Uri _threadStatsCollectionUri = UriFactory.CreateDocumentCollectionUri(DATABASE_ID, THREAD_STATS_COLLECTION_ID);

        public ThreadStatsCosmosDbService(IDocumentClientSingletonProvider documentClientSingletonProvider)
        {
            if (documentClientSingletonProvider == null)
            {
                throw new ArgumentNullException(nameof(documentClientSingletonProvider));
            }

            _documentClientSingleton = documentClientSingletonProvider.DocumentClientSingleton;
        }

        public void EnsureDatabaseCreated()
        {
            _documentClientSingleton.CreateDatabaseIfNotExistsAsync(new Database { Id = DATABASE_ID }).Wait();
            _documentClientSingleton.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DATABASE_ID), new DocumentCollection { Id = THREAD_STATS_COLLECTION_ID }).Wait();
        }

        public Task<IChangefeed<ThreadStats>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IChangefeed<ThreadStats>>(new CosmosDbChangefeed<ThreadStats>());
        }

        public Task InsertThreadStatsAsync(ThreadStats threadStats)
        {
            return _documentClientSingleton.CreateDocumentAsync(_threadStatsCollectionUri, threadStats);
        }
    }
}
