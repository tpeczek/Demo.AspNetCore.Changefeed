using System;
using System.Threading.Tasks;

namespace Demo.AspNetCore.RethinkDB.Services
{
    internal interface IWebSocketConnectionsService
    {
        void AddConnection(WebSocketConnection connection);

        void RemoveConnection(Guid connectionId);

        Task SendToAllAsync(string message);
    }
}
