using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services
{
    internal class ThreadStatsGatherBackgroundService : BackgroundService
    {
        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;

        public ThreadStatsGatherBackgroundService(IThreadStatsChangefeedDbService threadStatsChangefeedDbService)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ThreadPool.GetAvailableThreads(out var workerThreads, out var _);
                ThreadPool.GetMinThreads(out var minThreads, out var _);
                ThreadPool.GetMaxThreads(out var maxThreads, out var _);

                await _threadStatsChangefeedDbService.InsertThreadStatsAsync(new ThreadStats
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
