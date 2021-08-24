using BinanceExchange.API.Helpers;

using BinanceExchange.API.Enums;

namespace BinanceBot.Market
{
    /// <summary>
    /// <see cref="MarketDepth"/> quote representing bid or ask
    /// </summary>
    public class Quote
    {
        public Quote(decimal price, decimal volume, OrderSide direction)
        {
            if (price <= 0) ThrowHelper.ArgumentOutOfRangeException(nameof(price));
            if (volume <= 0) ThrowHelper.ArgumentOutOfRangeException(nameof(volume));

            Price = price;
            Volume = volume;
            Direction = direction;
        }


        /// <summary>
        /// Quote price
        /// </summary>
        public decimal Price { get; }

        /// <summary>
        /// Quote volume
        /// </summary>
        public decimal Volume { get; }


        /// <summary>
        /// Direction (buy or sell)
        /// </summary>
        public OrderSide Direction { get; }
    }
}
