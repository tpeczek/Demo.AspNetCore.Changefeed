using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using Demo.AspNetCore.RethinkDB.Services;

namespace Demo.AspNetCore.RethinkDB.Middlewares
{
    internal class WebSocketConnectionsMiddleware
    {
        private IWebSocketConnectionsService _connectionsService;

        public WebSocketConnectionsMiddleware(RequestDelegate next, IWebSocketConnectionsService connectionsService)
        {
            _connectionsService = connectionsService ?? throw new ArgumentNullException(nameof(connectionsService));
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                WebSocketConnection webSocketConnection = new WebSocketConnection(webSocket);

                _connectionsService.AddConnection(webSocketConnection);

                await webSocketConnection.WaitUntilCloseAsync();

                if (webSocketConnection.CloseStatus.HasValue)
                {
                    await webSocket.CloseAsync(webSocketConnection.CloseStatus.Value, webSocketConnection.CloseStatusDescription, CancellationToken.None);
                }

                _connectionsService.RemoveConnection(webSocketConnection.Id);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}
