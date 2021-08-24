using BinanceExchange.API.Client;

using Moq;

using Serilog;

namespace BinanceExchange.Tests.Client.BinanceClient
{
    public class BinanceClientBaseTests
    {
        protected BinanceRestClient ConcreteBinanceClient;
        protected BinanceClientConfiguration DefaultClientConfiguration;

        protected Mock<ILogger> MockLogger;
        protected Mock<IAPIProcessor> MockAPIProcessor;

        public BinanceClientBaseTests()
        {
            
            MockLogger = new Mock<ILogger>();
            MockAPIProcessor = new Mock<IAPIProcessor>();

            DefaultClientConfiguration = new BinanceClientConfiguration()
            {
                ApiKey = "APIKEY",
                SecretKey = "SECRETKEY"
            };

            ConcreteBinanceClient = new BinanceRestClient(DefaultClientConfiguration, MockAPIProcessor.Object);
        }
    }
}
