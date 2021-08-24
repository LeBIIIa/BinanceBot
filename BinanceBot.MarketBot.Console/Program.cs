using BinanceBot.Common;
using BinanceBot.Market;

using BinanceExchange.API.Client;
using BinanceExchange.API.Websockets;

using Microsoft.Extensions.DependencyInjection;

using System.Threading.Tasks;


namespace BinanceBot.MarketBot.Console
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // set bot settings
            const string token = "ETHBTC";

            var host = new BootstrapConsole().AppStartup();
            var binanceRestClient = ActivatorUtilities.CreateInstance<BinanceRestClient>(host.Services, new BinanceClientConfiguration
            {
                ApiKey = "<your_api_key>",
                SecretKey = "<your_secret_key>"
            });

            var strategyConfig = new MarketStrategyConfiguration
            {
                MinOrderVolume = 1.0M,
                MaxOrderVolume = 50.0M,
                TradeWhenSpreadGreaterThan = .02M
            };


            // create bot
            NaiveMarketMakerStrategy mms = ActivatorUtilities.CreateInstance<NaiveMarketMakerStrategy>(host.Services, strategyConfig);
            BinanceWebSocketClient binanceWebSocket = ActivatorUtilities.CreateInstance<BinanceWebSocketClient>(host.Services, binanceRestClient);
            MarketMakerBot bot = ActivatorUtilities.CreateInstance<MarketMakerBot>(host.Services, token, mms, binanceRestClient, binanceWebSocket);

            // start bot
            try
            {
                await bot.RunAsync();

                System.Console.WriteLine("Press Enter to stop bot...");
                System.Console.ReadLine();
            }
            finally
            {
                bot.Stop();
            }

            System.Console.WriteLine("Press Enter to exit...");
            System.Console.ReadLine();
        }
    }
}
