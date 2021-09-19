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
        public static async Task Main(string[] _)
        {
            var host = new BootstrapConsole().AppStartup();
            BinanceRestClient binanceRestClient = ActivatorUtilities.CreateInstance<BinanceRestClient>(host.Services);

            // create bot
            NaiveMarketMakerStrategy mms = ActivatorUtilities.CreateInstance<NaiveMarketMakerStrategy>(host.Services);
            BinanceWebSocketClient binanceWebSocket = ActivatorUtilities.CreateInstance<BinanceWebSocketClient>(host.Services, binanceRestClient);
            MarketMakerBot bot = ActivatorUtilities.CreateInstance<MarketMakerBot>(host.Services, mms, binanceRestClient, binanceWebSocket);

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
