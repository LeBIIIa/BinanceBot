using BinanceBot.Common;
using BinanceBot.Market;

using BinanceExchange.API.Client;
using BinanceExchange.API.Models.Response;
using BinanceExchange.API.Websockets;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

using Serilog;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace BinanceBot.MarketViewer.Console
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            const string token = "ETHBTC";

            var host = new BootstrapConsole().AppStartup();
            var binanceRestClient = ActivatorUtilities.CreateInstance<BinanceRestClient>(host.Services, new BinanceClientConfiguration
            {
                ApiKey = "<your_api_key>",
                SecretKey = "<your_secret_key>"
            });

            var marketDepth = new MarketDepth(token);

            await TestConnection(binanceRestClient);

            marketDepth.MarketDepthChanged += (sender, e) =>
            {
                int n = 20;

                System.Console.Clear();

                System.Console.WriteLine("Price : Volume");
                System.Console.WriteLine(
                    JsonConvert.SerializeObject(
                        new
                        {
                            LastUpdate = e.UpdateTime,
                            Asks = e.Asks.Take(n).Reverse().Select(s => $"{s.Price} : {s.Volume}"),
                            Bids = e.Bids.Take(n).Select(s => $"{s.Price} : {s.Volume}")
                        },
                        Formatting.Indented));

                System.Console.WriteLine("Press Enter to stop streaming market depth...");

                System.Console.SetCursorPosition(0, 0);
            };

            var binanceWebSocket = ActivatorUtilities.CreateInstance<BinanceWebSocketClient>(host.Services, binanceRestClient);
            var marketDepthManager = ActivatorUtilities.CreateInstance<MarketDepthManager>(host.Services, binanceRestClient, binanceWebSocket);

            // build order book
            await marketDepthManager.BuildAsync(marketDepth);
            // stream order book updates
            marketDepthManager.StreamUpdates(marketDepth);


            System.Console.WriteLine("Press Enter to exit...");
            System.Console.ReadLine();
        }

        private static async Task TestConnection(IBinanceRestClient binanceRestClient)
        {
            Log.Logger.Information("Testing connection...");
            IResponse testConnectResponse = await binanceRestClient.TestConnectivityAsync();
            if (testConnectResponse != null)
            {
                ServerTimeResponse serverTimeResponse = await binanceRestClient.GetServerTimeAsync();
                Log.Logger.Information($"Connection is established. Approximate ping time: {DateTime.UtcNow.Subtract(serverTimeResponse.ServerTime).TotalMilliseconds:F0} ms");
            }
        }
    }
}
