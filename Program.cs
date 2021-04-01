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
                    //use built-in options pattern for configuration DI. this allows run-time updates of config values.
                    services.Configure<ServiceConfig>(cfg.GetSection("ServiceConfig"));
                    services.Configure<List<Recommendation>>(cfg.GetSection("Recommendations"));
                    //use typed httpclient for efficient disposal
                    services.AddHttpClient<OpenWeatherMapApiService>()
                        //use polly to handle some basic retries for internet hiccups
                        .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(500)));
                    //Hosted service that takes user input is a bit odd but allows use of DI
                    services.AddHostedService<ApplicationService>();
                });
    }
}
