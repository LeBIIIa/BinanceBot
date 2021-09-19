using BinanceBot.Market;

using BinanceExchange.API.Helpers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BinanceBot.Common
{
    public class BootstrapConsole : Bootstrap
    {
        public override void AdditionalServices(IHostBuilder hostBuilder, IConfiguration configuration)
        {
            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddTransient<IMarketBot, MarketMakerBot>();
                services.Configure<MarketStrategyConfiguration>(configuration.GetSection("StrategyConfiguration"));
            });
        }
    }
}
