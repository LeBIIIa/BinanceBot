using BinanceDotNet.BinanceExchange.API.Helpers;

using BinanceExchange.API.Enums;
using BinanceExchange.API.Helpers;
using BinanceExchange.API.Models.Request;
using BinanceExchange.API.Utility;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace BinanceExchange.API.Client
{
    public class Endpoints
    {
        /// <summary>
        ///     Defaults to V1
        /// </summary>
        /// <summary>
        ///     Defaults to API binance domain (https)
        /// </summary>
        protected readonly string BaseUrl;

        public Endpoints(IOptions<BinanceClientConfiguration> options)
        {
            Guard.AgainstNull(options);
            Guard.AgainstNullOrEmpty(options.Value.BaseUrl);
            BaseUrl = options.Value.BaseUrl;
        }

        protected static string GenerateQueryStringFromData(IRequest request)
        {
            if (request == null) ThrowHelper.Exception("No request data provided - query string can't be created");

            StringBuilder sb = new();
            PropertyInfo[] properties = request.GetType().GetProperties();
            foreach(PropertyInfo p in properties)
            {
                object o = p.GetValue(request);
                if (o != null)
                {
                    sb.Append(StringUtils.ToCamelCase(p.Name));
                    sb.Append('=');
                    sb.Append(WebUtility.UrlEncode(o.ToString()));
                }
            }

            return sb.ToString();
        }

        public class General : Endpoints
        {
            private static readonly string ApiVersion = "v3";

            public General(IOptions<BinanceClientConfiguration> options) : base(options) { }

            /// <summary>
            ///     Test connectivity to the Rest API.
            /// </summary>
            public BinanceEndpointData TestConnectivity =>
                new(new Uri($"{BaseUrl}/{ApiVersion}/ping"), EndpointSecurityType.None);

            /// <summary>
            ///     Test connectivity to the Rest API and get the current server time.
            /// </summary>
            public BinanceEndpointData ServerTime =>
                new(new Uri($"{BaseUrl}/{ApiVersion}/time"), EndpointSecurityType.None);

            /// <summary>
            ///     Current exchange trading rules and symbol information.
            /// </summary>
            public BinanceEndpointData ExchangeInfo =>
                new(new Uri($"{BaseUrl}/{ApiVersion}/exchangeInfo"), EndpointSecurityType.None);
        }

        public class MarketData : Endpoints
        {
            private static readonly string ApiVersion = "v1";
            public MarketData(IOptions<BinanceClientConfiguration> options) : base(options) { }

            /// <summary>
            ///     Latest price for all symbols.
            /// </summary>
            public BinanceEndpointData AllSymbolsPriceTicker =>
                new(new Uri($"{BaseUrl}/{ApiVersion}/ticker/allPrices"),
                    EndpointSecurityType.ApiKey);

            /// <summary>
            ///     Best price/qty on the order book for all symbols.
            /// </summary>
            public BinanceEndpointData SymbolsOrderBookTicker =>
                new(new Uri($"{BaseUrl}/{ApiVersion}/ticker/allBookTickers"),
                    EndpointSecurityType.ApiKey);

            /// <summary>
            ///     Gets the order book with all bids and asks
            /// </summary>
            public BinanceEndpointData OrderBook(string symbol, int limit, bool useCache = false)
            {
                return new(new Uri($"{BaseUrl}/{ApiVersion}/depth?symbol={symbol}&limit={limit}"),
                    EndpointSecurityType.None, useCache);
            }

        }

        public class Account : Endpoints
        {
            private static readonly string ApiVersion = "v3";

            public Account(IOptions<BinanceClientConfiguration> options) : base(options) { }

            public BinanceEndpointData NewOrder(CreateOrderRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new(new Uri($"{BaseUrl}/{ApiVersion}/order?{queryString}"),
                    EndpointSecurityType.Signed);
            }

            public BinanceEndpointData NewOrderTest(CreateOrderRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new(new Uri($"{BaseUrl}/{ApiVersion}/order/test?{queryString}"),
                    EndpointSecurityType.Signed);
            }


            public BinanceEndpointData CancelOrder(CancelOrderRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new(new Uri($"{BaseUrl}/{ApiVersion}/order?{queryString}"),
                    EndpointSecurityType.Signed);
            }

            public BinanceEndpointData CurrentOpenOrders(CurrentOpenOrdersRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new(new Uri($"{BaseUrl}/{ApiVersion}/openOrders?{queryString}"),
                    EndpointSecurityType.Signed);
            }
        }
    }
}