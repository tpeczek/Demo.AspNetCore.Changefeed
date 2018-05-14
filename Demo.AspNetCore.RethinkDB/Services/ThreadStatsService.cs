using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Demo.AspNetCore.RethinkDB.Services
{
    internal class ThreadStatsService : IHostedService
    {
        private readonly IRethinkDbService _rethinkDbService;

        private Task _threadStatsTask;
        private CancellationTokenSource _threadStatsCancellationTokenSource;

        public ThreadStatsService(IRethinkDbService rethinkDbService)
        {
            _rethinkDbService = rethinkDbService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _threadStatsCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _threadStatsTask = GatherThreadStatsAsync(_threadStatsCancellationTokenSource.Token);

            return _threadStatsTask.IsCompleted ? _threadStatsTask : Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_threadStatsTask != null)
            {
                _threadStatsCancellationTokenSource.Cancel();

                await Task.WhenAny(_threadStatsTask, Task.Delay(-1, cancellationToken));

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private async Task GatherThreadStatsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ThreadPool.GetAvailableThreads(out var workerThreads, out var _);
                ThreadPool.GetMinThreads(out var minThreads, out var _);
                ThreadPool.GetMaxThreads(out var maxThreads, out var _);

                _rethinkDbService.InsertThreadStats(new ThreadStats
                {
                    WorkerThreads = workerThreads,
                    MinThreads = minThreads,
                    MaxThreads = maxThreads
                });

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
