using BinanceExchange.API.Client;
using BinanceExchange.API.Websockets;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Serilog;

using System;
using System.IO;

using static BinanceExchange.API.Client.Endpoints;

namespace BinanceExchange.API.Helpers
{
    public abstract class Bootstrap
    {
        public abstract void AdditionalServices(IHostBuilder hostBuilder, IConfiguration configuration);
        public IHostBuilder ConfigureServices(IConfiguration configuration)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                    {
                        services.Configure<BinanceClientConfiguration>(configuration.GetSection("BinanceConfiguration"));
                        services.AddSingleton(configuration);
                        services.AddSingleton(RequestClient.GetInstance(Log.Logger));
                        services.AddSingleton(provider => new General(provider.GetService<IOptions<BinanceClientConfiguration>>()!));
                        services.AddSingleton(provider => new MarketData(provider.GetService<IOptions<BinanceClientConfiguration>>()!));
                        services.AddSingleton(provider => new Account(provider.GetService<IOptions<BinanceClientConfiguration>>()!));
                        services.AddScoped<IAPIProcessor, APIProcessor>();
                        services.AddScoped<IBinanceRestClient, BinanceRestClient>();
                        services.AddScoped<IBinanceWebSocketClient, BinanceWebSocketClient>();
                    });
            AdditionalServices(host, configuration);

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
                .CreateLogger();

            // Initiated the denpendency injection container
            return ConfigureServices(configuration).UseSerilog().Build();
        }
        static void ConfigSetup(IConfigurationBuilder builder)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_Environment");
            builder.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
        }
    }
}
