using System.Threading;
using System.Threading.Tasks;

namespace Demo.AspNetCore.Changefeed.Services.Abstractions
{
    public interface IThreadStatsChangefeedDbService
    {
        Task EnsureDatabaseCreatedAsync();

        Task InsertThreadStatsAsync(ThreadStats threadStats);

        Task<IChangefeed<ThreadStats>> GetThreadStatsChangefeedAsync(CancellationToken cancellationToken);
    }
}
