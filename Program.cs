using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectSweater.Configuration;
using ProjectSweater.Services;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Polly;

namespace ProjectSweater
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostCtx, cfg) =>
                {
                    //Default behavior is sufficient; however it can be overridden here.
                })
                .ConfigureServices((hostCtx, services) =>
                {
                    var cfg = hostCtx.Configuration;
                    services.Configure<ServiceConfig>(cfg.GetSection("ServiceConfig"));
                    services.Configure<List<Recommendation>>(cfg.GetSection("Recommendations"));
                    services.AddHttpClient<OpenWeatherMapApiService>()
                        .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(500)));
                    services.AddHostedService<ApplicationService>();
                });
    }
}
