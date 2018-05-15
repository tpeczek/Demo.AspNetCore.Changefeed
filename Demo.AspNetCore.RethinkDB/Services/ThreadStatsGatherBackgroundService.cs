using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Demo.AspNetCore.RethinkDB.Services
{
    internal class ThreadStatsGatherBackgroundService : BackgroundService
    {
        private readonly IRethinkDbService _rethinkDbService;

        public ThreadStatsGatherBackgroundService(IRethinkDbService rethinkDbService)
        {
            _rethinkDbService = rethinkDbService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
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

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
