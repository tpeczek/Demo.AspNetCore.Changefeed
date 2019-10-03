using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Lib.AspNetCore.ServerSentEvents;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed.Services
{
    internal class ThreadStatsChangefeedBackgroundService : BackgroundService
    {
        private readonly IThreadStatsChangefeedDbService _threadStatsChangefeedDbService;
        private readonly IServerSentEventsService _serverSentEventsService;
        private readonly IWebSocketConnectionsService _webSocketConnectionsService;

        public ThreadStatsChangefeedBackgroundService(IThreadStatsChangefeedDbService threadStatsChangefeedDbService, IServerSentEventsService serverSentEventsService, IWebSocketConnectionsService webSocketConnectionsService)
        {
            _threadStatsChangefeedDbService = threadStatsChangefeedDbService;
            _serverSentEventsService = serverSentEventsService;
            _webSocketConnectionsService = webSocketConnectionsService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IChangefeed<ThreadStats> threadStatsChangefeed = await _threadStatsChangefeedDbService.GetThreadStatsChangefeedAsync(stoppingToken);

            await foreach (ThreadStats threadStatsChange in threadStatsChangefeed.FetchFeed(stoppingToken))
            {
                string newThreadStats = threadStatsChange.ToString();
                await Task.WhenAll(
                    _serverSentEventsService.SendEventAsync(newThreadStats),
                    _webSocketConnectionsService.SendToAllAsync(newThreadStats)
                );
            }
        }
    }
}
