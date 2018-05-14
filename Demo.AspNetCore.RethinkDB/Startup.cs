using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Lib.AspNetCore.ServerSentEvents;
using Demo.AspNetCore.RethinkDB.Services;
using Demo.AspNetCore.RethinkDB.Middlewares;

namespace Demo.AspNetCore.RethinkDB
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRethinkDb(options =>
            {
                options.HostnameOrIp = "127.0.0.1";
            });

            services.AddServerSentEvents();

            services.AddWebSocketConnections();

            services.AddThreadStats();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider services)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            services.GetService<IRethinkDbService>().EnsureDatabaseCreated();

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
