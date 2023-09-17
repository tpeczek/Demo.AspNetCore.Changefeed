using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Demo.AspNetCore.Changefeed.Middlewares
{
    internal static class EndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapWebSocketConnections(this IEndpointRouteBuilder endpoints, string pattern)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            RequestDelegate pipeline = endpoints.CreateApplicationBuilder()
               .UseMiddleware<WebSocketConnectionsMiddleware>()
               .Build();

            return endpoints.Map(pattern, pipeline);
        }
    }
}
