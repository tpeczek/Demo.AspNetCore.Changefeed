using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lib.AspNetCore.ServerSentEvents;
using Demo.AspNetCore.Changefeed.Services;
using Demo.AspNetCore.Changefeed.Middlewares;

namespace Demo.AspNetCore.Changefeed
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddChangefeed(Configuration)
                .AddServerSentEvents()
                .AddWebSocketConnections()
                .AddThreadStats();
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
