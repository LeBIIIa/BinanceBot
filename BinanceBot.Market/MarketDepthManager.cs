﻿using BinanceExchange.API.Client;
using BinanceExchange.API.Helpers;
using BinanceExchange.API.Models.Response;
using BinanceExchange.API.Websockets;

using System;
using System.Threading.Tasks;

namespace BinanceBot.Market
{
    /// <summary>
    /// <see cref="MarketDepth"/> manager
    /// </summary>
    /// <remarks>
    /// How to manage a local order book correctly [1]: 
    ///    1. Open a stream to wss://stream.binance.com:9443/ws/bnbbtc@depth
    ///    2. Buffer the events you receive from the stream
    ///    3. Get a depth snapshot from https://www.binance.com/api/v3/depth?symbol=BNBBTC&amp;limit=1000
    ///    4. Drop any event where u is less or equal lastUpdateId in the snapshot
    ///    5. The first processed should have U less or equal lastUpdateId+1 AND u equal or greater lastUpdateId+1
    ///    6. While listening to the stream, each new event's U should be equal to the previous event's u+1
    ///    7. The data in each event is the absolute quantity for a price level
    ///    8. If the quantity is 0, remove the price level
    ///    9. Receiving an event that removes a price level that is not in your local order book can happen and is normal.
    /// Reference:
    ///     1. https://github.com/binance-exchange/binance-official-api-docs/blob/master/web-socket-streams.md#how-to-manage-a-local-order-book-correctly
    /// </remarks>
    public class MarketDepthManager
    {
        private readonly IBinanceRestClient _restClient;
        private readonly IBinanceWebSocketClient _webSocketClient;


        /// <summary>
        /// Create instance of <see cref="MarketDepthManager"/>
        /// </summary>
        /// <param name="binanceRestClient">Binance REST client</param>
        /// <param name="webSocketClient">Binance WebSocket client</param>
        /// <exception cref="ArgumentNullException"><paramref name="binanceRestClient"/> cannot be <see langword="null"/></exception>
        /// <exception cref="ArgumentNullException"><paramref name="webSocketClient"/> cannot be <see langword="null"/></exception>
        public MarketDepthManager(IBinanceRestClient binanceRestClient, IBinanceWebSocketClient webSocketClient)
        {
            _restClient = binanceRestClient ?? ThrowHelper.ArgumentNullException<IBinanceRestClient>(nameof(binanceRestClient));
            _webSocketClient = webSocketClient ?? ThrowHelper.ArgumentNullException<IBinanceWebSocketClient>(nameof(webSocketClient));
        }


        /// <summary>
        /// Build <see cref="MarketDepth"/>
        /// </summary>
        /// <param name="marketDepth">Market depth</param>
        /// <param name="limit">Limit of returned orders count</param>
        public async Task BuildAsync(MarketDepth marketDepth, int limit = 100)
        {
            if (marketDepth == null)
                ThrowHelper.ArgumentNullException(nameof(marketDepth));
            if (limit <= 0)
                ThrowHelper.ArgumentOutOfRangeException(nameof(limit));

            OrderBookResponse orderBook = await _restClient.GetOrderBookAsync(marketDepth.Symbol, false, limit);

            marketDepth.UpdateDepth(orderBook.Asks, orderBook.Bids, orderBook.LastUpdateId);
        }


        /// <summary>
        /// Stream <see cref="MarketDepth"/> updates
        /// </summary>
        /// <param name="marketDepth">Market depth</param>
        public void StreamUpdates(MarketDepth marketDepth)
        {
            if (marketDepth == null)
                ThrowHelper.ArgumentNullException(nameof(marketDepth));

            _webSocketClient.ConnectToDepthWebSocket(
                marketDepth.Symbol,
                marketData => marketDepth.UpdateDepth(marketData.AskDepthDeltas, marketData.BidDepthDeltas, marketData.UpdateId));
        }
    }
}