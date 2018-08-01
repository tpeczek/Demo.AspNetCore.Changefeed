using System.Threading;
using System.Threading.Tasks;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.CosmosDB
{
    internal class CosmosDbChangefeed<T> : IChangefeed<T>
    {
        public T CurrentNewValue => default(T);

        public Task<bool> MoveNextAsync(CancellationToken cancelToken = default(CancellationToken))
        {
            return Task.FromResult(false);
        }
    }
}
