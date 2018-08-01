using System.Threading;
using System.Threading.Tasks;

namespace Demo.AspNetCore.Changefeed.Services.Abstractions
{
    public interface IThreadStatsChangefeedDbService
    {
        void EnsureDatabaseCreated();

        void InsertThreadStats(ThreadStats threadStats);

        Task<IChangefeed<ThreadStats>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken);
    }
}
