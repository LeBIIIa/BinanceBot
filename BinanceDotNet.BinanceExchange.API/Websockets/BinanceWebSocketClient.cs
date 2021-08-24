
using BinanceExchange.API.Client;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;

using WebSocketSharp;

namespace BinanceExchange.API.Websockets
{
    /// <summary>
    ///     A Disposable instance of the BinanceWebSocketClient is used when wanting to open a connection to retrieve data
    ///     through the WebSocket protocol.
    ///     Implements IDisposable so that cleanup is managed
    /// </summary>
    public class BinanceWebSocketClient : AbstractBinanceWebSocketClient, IBinanceWebSocketClient
    {
        public BinanceWebSocketClient(ILogger<IBinanceWebSocketClient> logger, IBinanceRestClient binanceRestClient) :
            base(logger, binanceRestClient)
        { }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AllSockets
                    .ForEach(ws =>
                    {
                        if (ws.IsAlive) ws.Close(CloseStatusCode.Normal);
                    });

                AllSockets = new List<BinanceWebSocket>();
                ActiveWebSockets = new Dictionary<Guid, BinanceWebSocket>();
            }
        }
    }
}