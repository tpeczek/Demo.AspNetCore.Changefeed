using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Demo.AspNetCore.RethinkDB.Services
{
    internal class WebSocketConnectionsService : IWebSocketConnectionsService
    {
        private readonly ConcurrentDictionary<Guid, WebSocketConnection> _connections = new ConcurrentDictionary<Guid, WebSocketConnection>();

        public void AddConnection(WebSocketConnection connection)
        {
            _connections.TryAdd(connection.Id, connection);
        }

        public void RemoveConnection(Guid connectionId)
        {
            WebSocketConnection connection;

            _connections.TryRemove(connectionId, out connection);
        }

        public Task SendToAllAsync(string message)
        {
            List<Task> connectionsTasks = new List<Task>();
            foreach (WebSocketConnection connection in _connections.Values)
            {
                connectionsTasks.Add(connection.SendAsync(Encoding.UTF8.GetBytes(message)));
            }

            return Task.WhenAll(connectionsTasks);
        }
    }
}
