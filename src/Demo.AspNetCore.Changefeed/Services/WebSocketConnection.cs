using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace Demo.AspNetCore.Changefeed.Services
{
    internal class WebSocketConnection
    {
        private const int RECEIVE_PAYLOAD_BUFFER_SIZE = 4 * 1024;

        private WebSocket _webSocket;

        public Guid Id => Guid.NewGuid();

        public WebSocketCloseStatus? CloseStatus { get; private set; } = null;

        public string CloseStatusDescription { get; private set; } = null;

        public WebSocketConnection(WebSocket webSocket)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
        }

        public Task SendAsync(byte[] message)
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                ArraySegment<byte> messageSegment = new ArraySegment<byte>(message, 0, message.Length);

                return _webSocket.SendAsync(messageSegment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public async Task WaitUntilCloseAsync()
        {
            try
            {
                byte[] receivePayloadBuffer = new byte[RECEIVE_PAYLOAD_BUFFER_SIZE];
                WebSocketReceiveResult webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
                while (webSocketReceiveResult.MessageType != WebSocketMessageType.Close)
                {
                    while (!webSocketReceiveResult.EndOfMessage)
                    {
                        webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
                    }

                    webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
                }

                CloseStatus = webSocketReceiveResult.CloseStatus.Value;
                CloseStatusDescription = webSocketReceiveResult.CloseStatusDescription;
            }
            catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            { }
        }
    }
}
