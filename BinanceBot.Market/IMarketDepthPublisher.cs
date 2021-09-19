using BinanceExchange.API.Helpers;

using System;
using System.Collections.Generic;

namespace BinanceBot.Market
{
    /// <summary>
    /// Publisher of <see cref="MarketDepth"/> events
    /// </summary>
    public interface IMarketDepthPublisher
    {
        /// <summary>
        /// Order book was changed
        /// </summary>
        event EventHandler<MarketDepthChangedEventArgs> MarketDepthChanged;

        /// <summary>
        /// Best <seealso cref="MarketDepthPair"/> was changed
        /// </summary>
        event EventHandler<MarketBestPairChangedEventArgs> MarketBestPairChanged;
    }



    /// <summary>
    /// Order book changed event args
    /// </summary>
    public sealed class MarketBestPairChangedEventArgs : EventArgs
    {
        public MarketBestPairChangedEventArgs(MarketDepthPair marketBestPair)
        {
            MarketBestPair = marketBestPair ?? ThrowHelper.ArgumentNullException<MarketDepthPair>(nameof(marketBestPair));
        }

        public MarketDepthPair MarketBestPair { get; }
    }


    /// <summary>
    /// Best <seealso cref="MarketDepthPair"/> changed event args
    /// </summary>
    public sealed class MarketDepthChangedEventArgs : EventArgs
    {
        public MarketDepthChangedEventArgs(List<Quote> asks, List<Quote> bids, long updateTime)
        {
            if (updateTime <= 0) ThrowHelper.ArgumentOutOfRangeException(nameof(updateTime));

            Asks = asks;
            Bids = bids;
            UpdateTime = updateTime;
        }


        public List<Quote> Asks { get; }

        public List<Quote> Bids { get; }

        public long UpdateTime { get; }
    }
}