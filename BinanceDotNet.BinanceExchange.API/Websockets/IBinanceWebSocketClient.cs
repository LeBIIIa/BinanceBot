using BinanceExchange.API.Models.WebSocket;

using System;

namespace BinanceExchange.API.Websockets
{
    /// <summary>
    /// Binance WebSocket Client
    /// </summary>
    /// <see href="https://github.com/binance-exchange/binance-official-api-docs/blob/master/web-socket-streams.md"/>
    public interface IBinanceWebSocketClient : IDisposable
    {
        /// <summary>
        /// Connect to the Depth WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        Guid ConnectToDepthWebSocket(string symbol, BinanceWebSocketMessageHandler<BinanceDepthData> messageEventHandler);

        /// <summary>
        /// Close a specific WebSocket instance using the Guid provided on creation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fromError"></param>
        void CloseWebSocketInstance(Guid id, bool fromError = false);
    }
}
