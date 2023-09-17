using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Cosmos;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services.CosmosDB
{
    internal class CosmosDbChangefeed<T> : IChangefeed<T>
    {
        private static DateTime _startTime = DateTime.Now;

        private readonly Container _container;
        private readonly Container _leaseContainer;
        private readonly TimeSpan _pollInterval;

        public CosmosDbChangefeed(Container container, Container leaseContainer, TimeSpan pollInterval)
        {
            _container = container;
            _leaseContainer = leaseContainer;
            _pollInterval = pollInterval;
        }

        public async IAsyncEnumerable<T> FetchFeed([EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<T> changesToProcess = null;
            SemaphoreSlim changesToProcessSignal = new SemaphoreSlim(0, 1);
            SemaphoreSlim changesProcessedSignal = new SemaphoreSlim(0, 1);            

            ChangeFeedProcessor changeFeedProcessor = _container.GetChangeFeedProcessorBuilder<T>($"{typeof(T).Name}_ChangeFeedProcessor", async (changes, cancellationToken) =>
            {
                if ((changes != null) && changes.Count > 0)
                {
                    changesToProcess = changes;

                    changesToProcessSignal.Release();

                    try
                    {
                        await changesProcessedSignal.WaitAsync(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    { }
                }
            })
            .WithInstanceName("Demo.AspNetCore.Changefeed")
            .WithStartTime(_startTime)
            .WithPollInterval(_pollInterval)
            .WithLeaseContainer(_leaseContainer)
            .Build();

            await changeFeedProcessor.StartAsync();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await changesToProcessSignal.WaitAsync(cancellationToken);

                    foreach (T item in changesToProcess)
                    {
                        yield return item;
                    }
                    changesToProcess = null;

                    changesProcessedSignal.Release();
                }
            }
            finally
            {
                await changeFeedProcessor.StopAsync();
            }
        }
    }
}
