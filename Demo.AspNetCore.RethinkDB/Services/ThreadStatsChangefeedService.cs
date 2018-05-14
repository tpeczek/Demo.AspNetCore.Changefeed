using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Lib.AspNetCore.ServerSentEvents;

namespace Demo.AspNetCore.RethinkDB.Services
{
    internal class ThreadStatsChangefeedService : IHostedService
    {
        private readonly IRethinkDbService _rethinkDbService;
        private readonly IServerSentEventsService _serverSentEventsService;
        private readonly IWebSocketConnectionsService _webSocketConnectionsService;

        private Task _exposeThreadStatsTask;
        private CancellationTokenSource _exposeThreadStatsCancellationTokenSource;

        public ThreadStatsChangefeedService(IRethinkDbService rethinkDbService, IServerSentEventsService serverSentEventsService, IWebSocketConnectionsService webSocketConnectionsService)
        {
            _rethinkDbService = rethinkDbService;
            _serverSentEventsService = serverSentEventsService;
            _webSocketConnectionsService = webSocketConnectionsService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _exposeThreadStatsCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _exposeThreadStatsTask = ExposeThreadStatsChangefeedAsync(_exposeThreadStatsCancellationTokenSource.Token);

            return _exposeThreadStatsTask.IsCompleted ? _exposeThreadStatsTask : Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_exposeThreadStatsTask != null)
            {
                _exposeThreadStatsCancellationTokenSource.Cancel();

                await Task.WhenAny(_exposeThreadStatsTask, Task.Delay(-1, cancellationToken));

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private async Task ExposeThreadStatsChangefeedAsync(CancellationToken cancellationToken)
        {
            var threadStatsChangefeed = await _rethinkDbService.GetThreadStatsChangefeedAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested && (await threadStatsChangefeed.MoveNextAsync(cancellationToken)))
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
