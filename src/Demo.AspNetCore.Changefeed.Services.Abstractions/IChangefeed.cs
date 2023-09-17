using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.AspNetCore.Changefeed.Services.Abstractions
{
    public interface IChangefeed<out T>
    {
        IAsyncEnumerable<T> FetchFeed(CancellationToken cancellationToken = default);
    }
}
