using BinanceExchange.API.Client;
using BinanceExchange.API.Helpers;
using BinanceExchange.API.Models.WebSocket;
using BinanceExchange.API.Models.WebSocket.Interfaces;
using BinanceExchange.API.Utility;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Security.Authentication;

using WebSocketSharp;

namespace BinanceExchange.API.Websockets
{
    /// <summary>
    ///     Abstract class for creating WebSocketClients
    /// </summary>
    public abstract class AbstractBinanceWebSocketClient : IBinanceWebSocketClient
    {
        private bool disposedValue;
        private readonly ILogger<IBinanceWebSocketClient> _logger;
        private SslProtocols SupportedProtocols { get; } = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;

        /// <summary>
        ///     Used for deletion on the fly
        /// </summary>
        protected Dictionary<Guid, BinanceWebSocket> ActiveWebSockets = new();
        protected List<BinanceWebSocket> AllSockets = new();

        /// <summary>
        ///     Base WebSocket URI for Binance API
        /// </summary>
        private readonly string BaseWebsocketUri = "wss://stream.binance.com:9443/ws";

        protected AbstractBinanceWebSocketClient(ILogger<IBinanceWebSocketClient> logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Connect to the Depth WebSocket
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="messageEventHandler"></param>
        /// <returns></returns>
        public Guid ConnectToDepthWebSocket(string symbol,
            BinanceWebSocketMessageHandler<BinanceDepthData> messageEventHandler)
        {
            Guard.AgainstNullOrEmpty(symbol, nameof(symbol));
            _logger.LogDebug("Connecting to Depth Web Socket");
            var endpoint = new Uri($"{BaseWebsocketUri}/{symbol.ToLower()}@depth");
            return CreateBinanceWebSocket(endpoint, messageEventHandler);
        }

        private Guid CreateBinanceWebSocket<T>(Uri endpoint, BinanceWebSocketMessageHandler<T> messageEventHandler)
            where T : IWebSocketResponse
        {
            var websocket = new BinanceWebSocket(endpoint.AbsoluteUri);
            websocket.OnOpen += (sender, e) => { _logger.LogDebug($"WebSocket Opened:{endpoint.AbsoluteUri}"); };
            websocket.OnMessage += (sender, e) =>
            {
                _logger.LogDebug($"WebSocket Message Received on: {endpoint.AbsoluteUri}");

                T data = JsonConvert.DeserializeObject<T>(e.Data)!;
                messageEventHandler(data);
            };
            websocket.OnError += (sender, e) =>
            {
                _logger.LogDebug(e.Exception, $"WebSocket Error on {endpoint.AbsoluteUri}:");
                CloseWebSocketInstance(websocket.Id, true);
                ThrowHelper.Exception("Binance WebSocket failed", new Exception()
                {
                    Data =
                    {
                        {"ErrorEventArgs", e}
                    }
                });
            };

            if (!ActiveWebSockets.ContainsKey(websocket.Id)) ActiveWebSockets.Add(websocket.Id, websocket);

            AllSockets.Add(websocket);
            websocket.SslConfiguration.EnabledSslProtocols = SupportedProtocols;
            websocket.Connect();

            return websocket.Id;
        }

        /// <summary>
        ///     Close a specific WebSocket instance using the Guid provided on creation
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fromError"></param>
        public void CloseWebSocketInstance(Guid id, bool fromError = false)
        {
            if (ActiveWebSockets.ContainsKey(id))
            {
                var ws = ActiveWebSockets[id];
                ActiveWebSockets.Remove(id);
                if (!fromError) ws.Close(CloseStatusCode.PolicyViolation);
            }
            else
            {
                ThrowHelper.InvalidOperationException($"No Websocket exists with the Id {id.ToString()}");
            }
        }

        /// <summary>
        ///     Checks whether a specific WebSocket instance is active or not using the Guid provided on creation
        /// </summary>
        public bool IsAlive(Guid id)
        {
            if (!ActiveWebSockets.ContainsKey(id))
                ThrowHelper.InvalidOperationException($"No Websocket exists with the Id {id.ToString()}");

            return ActiveWebSockets[id].IsAlive;
        }

        protected virtual void Dispose(bool disposing)
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}