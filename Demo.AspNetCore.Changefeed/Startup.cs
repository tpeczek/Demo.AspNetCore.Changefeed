using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Lib.AspNetCore.ServerSentEvents;
using Demo.AspNetCore.Changefeed.Middlewares;
using Demo.AspNetCore.Changefeed.Services;
using Demo.AspNetCore.Changefeed.Services.CosmosDB;
using Demo.AspNetCore.Changefeed.Services.MongoDB;
using Demo.AspNetCore.Changefeed.Services.RethinkDB;
using Demo.AspNetCore.Changefeed.Services.Abstractions;

namespace Demo.AspNetCore.Changefeed
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            #region Cosmos DB
            services.AddCosmosDb(options =>
            {
                options.EndpointUrl = "https://localhost:8081";
                options.AuthorizationKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            });
            #endregion

            #region MongoDB
            //services.AddMongoDb(options =>
            //{
            //    options.ConnectionString = "mongodb://localhost:27017";
            //});
            #endregion

            #region RethinkDB
            //services.AddRethinkDb(options =>
            //{
            //    options.HostnameOrIp = "127.0.0.1";
            //});
            #endregion

            services.AddServerSentEvents();

            services.AddWebSocketConnections();

            services.AddThreadStats();
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets()
                .UseStaticFiles()
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapServerSentEvents("/thread-stats-changefeed");
                    endpoints.MapWebSocketConnections("/thread-stats-changefeed-fallback");
                })
                .Run(async (context) =>
                {
                    await context.Response.WriteAsync("-- Demo.AspNetCore.Changefeed --");
                });
        }
    }
}
