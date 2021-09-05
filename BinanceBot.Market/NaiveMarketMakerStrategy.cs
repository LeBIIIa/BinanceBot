using BinanceExchange.API.Enums;
using BinanceExchange.API.Helpers;

using Microsoft.Extensions.Logging;

namespace BinanceBot.Market
{
    /// <summary>
    /// Market Maker strategy (naive version)
    /// </summary>
    public class NaiveMarketMakerStrategy : IMarketStrategy
    {
        private readonly MarketStrategyConfiguration _marketStrategyConfig;
        private readonly ILogger<IMarketStrategy> _logger;


        public NaiveMarketMakerStrategy(ILogger<IMarketStrategy> logger, MarketStrategyConfiguration marketStrategyConfig)
        {
            _marketStrategyConfig = marketStrategyConfig ?? ThrowHelper.ArgumentNullException<MarketStrategyConfiguration>(nameof(marketStrategyConfig));
            _logger = logger;
        }


        /// <summary>
        /// Process new best <see cref="MarketDepthPair"/> 
        /// </summary>
        /// <param name="marketPair">Best ask-bid pair</param>
        /// <returns>Recommended price-volume pair or <see langword="null"/></returns>
        public Quote Process(MarketDepthPair marketPair)
        {
            if (marketPair == null)
                ThrowHelper.ArgumentNullException(nameof(marketPair));
            if (!marketPair.IsFullPair)
                return null;


            Quote quote = null;


            _logger.LogInformation($"Best ask / bid: {marketPair.Ask.Price} / {marketPair.Bid.Price}. Update Id: {marketPair.UpdateTime}.");

            // get price spreads (in percent)
            decimal actualSpread = marketPair.PriceSpread.Value / marketPair.MediumPrice.Value * 100; // spread_relative = spread_absolute/price * 100
            decimal expectedSpread = _marketStrategyConfig.TradeWhenSpreadGreaterThan;

            _logger.LogInformation($"Spread absolute / relative: {marketPair.PriceSpread} / {actualSpread:F3}%. Update Id: {marketPair.UpdateTime}.");


            if (actualSpread >= expectedSpread)
            {
                // compute new order price
                decimal extra = marketPair.MediumPrice.Value * (actualSpread - expectedSpread) / 100; // extra = medium_price * (spread_actual - spread_expected)
                decimal orderPrice = marketPair.Bid.Price + extra; // new_price = best_bid + extra

                // compute order volume
                decimal volumeSpread = marketPair.VolumeSpread.Value;
                decimal orderVolume = volumeSpread > _marketStrategyConfig.MaxOrderVolume
                    ? _marketStrategyConfig.MaxOrderVolume // set max volume
                    : (volumeSpread < _marketStrategyConfig.MinOrderVolume
                        ? _marketStrategyConfig.MinOrderVolume // set min volume
                        : volumeSpread);


                // return new price-volume pair
                quote = new Quote(orderPrice, orderVolume, OrderSide.Buy);
            }


            return quote;
        }
    }
}