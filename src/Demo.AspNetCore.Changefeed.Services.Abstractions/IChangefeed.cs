using System.Threading;
using System.Collections.Generic;

namespace Demo.AspNetCore.Changefeed.Services.Abstractions
{
    public interface IChangefeed<out T>
    {
        IAsyncEnumerable<T> FetchFeed(CancellationToken cancellationToken = default);
    }
}
