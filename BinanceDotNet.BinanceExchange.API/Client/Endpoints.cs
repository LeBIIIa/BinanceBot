using BinanceExchange.API.Helpers;

using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Request;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using System;
using System.Linq;
using System.Net;

namespace BinanceExchange.API.Client
{
    internal static class Endpoints
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal
        };

        /// <summary>
        ///     Defaults to V1
        /// </summary>
        /// <summary>
        ///     Defaults to API binance domain (https)
        /// </summary>
        private const string BaseUrl = "https://api.binance.com/api";


        private static string GenerateQueryStringFromData(IRequest request)
        {
            if (request == null) ThrowHelper.Exception("No request data provided - query string can't be created");

            //TODO: Refactor to not require double JSON loop
            var obj = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(request, Settings),
                Settings)!;

            return string.Join("&", obj.Children()
                .Cast<JProperty>()
                .Where(j => j.Value != null)
                .Select(j => j.Name + "=" + WebUtility.UrlEncode(j.Value.ToString())));
        }


        public static class General
        {
            private static readonly string ApiVersion = "v1";

            /// <summary>
            ///     Test connectivity to the Rest API.
            /// </summary>
            public static BinanceEndpointData TestConnectivity =>
                new(new Uri($"{BaseUrl}/{ApiVersion}/ping"), EndpointSecurityType.None);

            /// <summary>
            ///     Test connectivity to the Rest API and get the current server time.
            /// </summary>
            public static BinanceEndpointData ServerTime =>
                new(new Uri($"{BaseUrl}/{ApiVersion}/time"), EndpointSecurityType.None);

            /// <summary>
            ///     Current exchange trading rules and symbol information.
            /// </summary>
            public static BinanceEndpointData ExchangeInfo =>
                new(new Uri($"{BaseUrl}/{ApiVersion}/exchangeInfo"), EndpointSecurityType.None);
        }

        public static class MarketData
        {
            private static readonly string ApiVersion = "v1";

            /// <summary>
            ///     Latest price for all symbols.
            /// </summary>
            public static BinanceEndpointData AllSymbolsPriceTicker =>
                new(new Uri($"{BaseUrl}/{ApiVersion}/ticker/allPrices"),
                    EndpointSecurityType.ApiKey);

            /// <summary>
            ///     Best price/qty on the order book for all symbols.
            /// </summary>
            public static BinanceEndpointData SymbolsOrderBookTicker =>
                new(new Uri($"{BaseUrl}/{ApiVersion}/ticker/allBookTickers"),
                    EndpointSecurityType.ApiKey);

            /// <summary>
            ///     Gets the order book with all bids and asks
            /// </summary>
            public static BinanceEndpointData OrderBook(string symbol, int limit, bool useCache = false)
            {
                return new(new Uri($"{BaseUrl}/{ApiVersion}/depth?symbol={symbol}&limit={limit}"),
                    EndpointSecurityType.None, useCache);
            }

        }

        public static class Account
        {
            private static readonly string ApiVersion = "v3";

            public static BinanceEndpointData NewOrder(CreateOrderRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new(new Uri($"{BaseUrl}/{ApiVersion}/order?{queryString}"),
                    EndpointSecurityType.Signed);
            }

            public static BinanceEndpointData NewOrderTest(CreateOrderRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new(new Uri($"{BaseUrl}/{ApiVersion}/order/test?{queryString}"),
                    EndpointSecurityType.Signed);
            }


            public static BinanceEndpointData CancelOrder(CancelOrderRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new(new Uri($"{BaseUrl}/{ApiVersion}/order?{queryString}"),
                    EndpointSecurityType.Signed);
            }

            public static BinanceEndpointData CurrentOpenOrders(CurrentOpenOrdersRequest request)
            {
                var queryString = GenerateQueryStringFromData(request);
                return new(new Uri($"{BaseUrl}/{ApiVersion}/openOrders?{queryString}"),
                    EndpointSecurityType.Signed);
            }
        }
    }
}