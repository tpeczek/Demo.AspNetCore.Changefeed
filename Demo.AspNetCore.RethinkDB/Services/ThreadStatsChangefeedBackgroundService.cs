using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Lib.AspNetCore.ServerSentEvents;

namespace Demo.AspNetCore.RethinkDB.Services
{
    internal class ThreadStatsChangefeedBackgroundService : BackgroundService
    {
        private readonly IRethinkDbService _rethinkDbService;
        private readonly IServerSentEventsService _serverSentEventsService;
        private readonly IWebSocketConnectionsService _webSocketConnectionsService;

        public ThreadStatsChangefeedBackgroundService(IRethinkDbService rethinkDbService, IServerSentEventsService serverSentEventsService, IWebSocketConnectionsService webSocketConnectionsService)
        {
            _rethinkDbService = rethinkDbService;
            _serverSentEventsService = serverSentEventsService;
            _webSocketConnectionsService = webSocketConnectionsService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var threadStatsChangefeed = await _rethinkDbService.GetThreadStatsChangefeedAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested && (await threadStatsChangefeed.MoveNextAsync(stoppingToken)))
            {
                string newThreadStats = threadStatsChangefeed.Current.NewValue.ToString();
                await Task.WhenAll(
                    _serverSentEventsService.SendEventAsync(newThreadStats),
                    _webSocketConnectionsService.SendToAllAsync(newThreadStats)
                );
            }
        }
    }
}
