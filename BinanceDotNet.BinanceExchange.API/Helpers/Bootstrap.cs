using BinanceExchange.API.Client;
using BinanceExchange.API.Websockets;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using System;
using System.IO;

namespace BinanceExchange.API.Helpers
{
    public abstract class Bootstrap
    {
        public abstract void AdditionalServices(IHostBuilder hostBuilder);
        public IHostBuilder ConfigureServices(IConfiguration configuration)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                    {
                        services.AddSingleton(configuration);
                        services.AddSingleton(RequestClient.GetInstance(Log.Logger));
                        services.AddTransient<IBinanceRestClient, BinanceRestClient>();
                        services.AddTransient<IBinanceWebSocketClient, AbstractBinanceWebSocketClient>();
                        services.AddTransient<IBinanceWebSocketClient, BinanceWebSocketClient>();
                        services.AddTransient<IAPIProcessor, APIProcessor>();
                    });
            AdditionalServices(host);

            return host;
        }
        public IHost AppStartup()
        {
            var builder = new ConfigurationBuilder();
            ConfigSetup(builder);
            IConfiguration configuration = builder.Build();

            // defining Serilog configs
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            // Initiated the denpendency injection container 
            var host = ConfigureServices(configuration).UseSerilog().Build();

            return host;
        }
        static void ConfigSetup(IConfigurationBuilder builder)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            builder.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
        }
    }
}
