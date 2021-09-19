using BinanceExchange.API.Helpers;

using BinanceExchange.API.Models.Request;
using BinanceExchange.API.Models.Response;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BinanceBot.Market
{
    /// <summary>
    /// Base Market Bot
    /// </summary>
    /// <typeparam name="TStrategy"></typeparam>
    public abstract class BaseMarketBot<TStrategy> :
        IMarketBot, IDisposable
        where TStrategy : class, IMarketStrategy
    {
        protected readonly ILogger<IMarketBot> Logger;
        protected readonly IConfiguration Config;

        protected BaseMarketBot(ILogger<IMarketBot> logger, IConfiguration config, TStrategy marketStrategy)
        {
            string symbol = config["CommonSettings:Symbol"];
            Symbol = symbol ?? ThrowHelper.ArgumentNullException<string>(nameof(symbol));
            MarketStrategy = marketStrategy ?? ThrowHelper.ArgumentNullException<TStrategy>(nameof(marketStrategy));
            Logger = logger;
            Config = config;
        }
        protected BaseMarketBot(ILogger<IMarketBot> logger, IConfiguration config, string symbol, TStrategy marketStrategy)
        {
            Symbol = symbol ?? ThrowHelper.ArgumentNullException<string>(nameof(symbol));
            MarketStrategy = marketStrategy ?? ThrowHelper.ArgumentNullException<TStrategy>(nameof(marketStrategy));
            Logger = logger;
            Config = config;
        }


        public string Symbol { get; }

        public TStrategy MarketStrategy { get; }


        public abstract Task RunAsync();

        public abstract void Stop();

        public abstract Task ValidateConnectionAsync();

        public abstract Task<List<OrderResponse>> GetOpenedOrdersAsync(string symbol);

        public abstract Task CancelOrdersAsync(List<OrderResponse> orders);

        public abstract Task<BaseCreateOrderResponse?> CreateOrderAsync(CreateOrderRequest order);


        public abstract void Dispose();
    }
}