using BinanceBot.Market;

using BinanceExchange.API.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BinanceBot.Common
{
    public class BootstrapConsole : Bootstrap
    {
        public override void AdditionalServices(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddTransient<IMarketBot, MarketMakerBot>();
            });
        }
    }
}
