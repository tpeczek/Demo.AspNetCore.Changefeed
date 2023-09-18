using System.Threading;
using System.Threading.Tasks;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Storage.Blobs
{
    internal class ThreadStatsBlobService : IThreadStatsChangefeedDbService
    {
        public Task EnsureDatabaseCreatedAsync()
        {
            return Task.CompletedTask;
        }

        public Task<IChangefeed<ThreadStats>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IChangefeed<ThreadStats>>(new BlobChangefeed<ThreadStats>());
        }

        public Task InsertThreadStatsAsync(ThreadStats threadStats)
        {
            return Task.CompletedTask;
        }
    }
}
