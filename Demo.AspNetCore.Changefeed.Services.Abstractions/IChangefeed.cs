using System.Threading;
using System.Threading.Tasks;

namespace Demo.AspNetCore.Changefeed.Services.Abstractions
{
    public interface IChangefeed<T>
    {
        T CurrentNewValue { get; }

        Task<bool> MoveNextAsync(CancellationToken cancelToken = default(CancellationToken));
    }
}
