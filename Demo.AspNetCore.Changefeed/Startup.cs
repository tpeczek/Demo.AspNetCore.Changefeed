using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Lib.AspNetCore.ServerSentEvents;
using Demo.AspNetCore.Changefeed.Services;
using Demo.AspNetCore.Changefeed.Services.RethinkDB;
using Demo.AspNetCore.Changefeed.Services.CosmosDB;
using Demo.AspNetCore.Changefeed.Services.Abstractions;
using Demo.AspNetCore.Changefeed.Middlewares;

namespace Demo.AspNetCore.Changefeed
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddRethinkDb(options =>
            //{
            //    options.HostnameOrIp = "127.0.0.1";
            //});

            services.AddCosmosDb(options =>
            {
                options.EndpointUrl = "https://localhost:8081";
                options.AuthorizationKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            });

            services.AddServerSentEvents();

            services.AddWebSocketConnections();

            services.AddThreadStats();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            services.GetService<IThreadStatsChangefeedDbService>().EnsureDatabaseCreated();

            app.MapServerSentEvents("/thread-stats-changefeed");

            app.UseWebSockets().MapWebSocketConnections("/thread-stats-changefeed-fallback");

            app.UseStaticFiles();

            app.UseMvc();

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("-- Demo.AspNetCore.RethinkDB --");
            });
        }
    }
}
