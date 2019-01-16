using System.Threading;
using System.Threading.Tasks;

namespace Demo.AspNetCore.Changefeed.Services.Abstractions
{
    public interface IChangefeed<out T>
    {
        T CurrentNewValue { get; }

        Task<bool> MoveNextAsync(CancellationToken cancelToken = default(CancellationToken));
    }
}
