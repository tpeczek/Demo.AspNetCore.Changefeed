using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.Azure.Storage.Blobs
{
    internal class BlobChangefeed<T> : IChangefeed<T>
    {
        public async IAsyncEnumerable<T> FetchFeed([EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            yield break;
        }
    }
}
