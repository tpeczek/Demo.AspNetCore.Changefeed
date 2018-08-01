using System.Threading;
using System.Threading.Tasks;

namespace Demo.AspNetCore.Changefeed.Services.Abstractions
{
    public interface IThreadStatsChangefeed
    {
        ThreadStats CurrentNewValue { get; }

        Task<bool> MoveNextAsync(CancellationToken cancelToken = default(CancellationToken));
    }
}
