
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
        private bool disposedValue;

        public BinanceWebSocketClient(ILogger<IBinanceWebSocketClient> logger) :
            base(logger)
        { }

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    AllSockets
                        .ForEach(ws =>
                        {
                            if (ws.IsAlive) ws.Close(CloseStatusCode.Normal);
                        });

                    AllSockets.Clear();
                    ActiveWebSockets.Clear();
                }
                disposedValue = true;
            }
        }
    }
}