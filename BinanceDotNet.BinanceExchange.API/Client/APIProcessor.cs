using BinanceExchange.API.Helpers;

using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Response.Error;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BinanceExchange.API.Client
{
    /// <summary>
    ///     The API Processor is the chief piece of functionality responsible for handling and creating requests to the API
    /// </summary>
    internal class APIProcessor : IAPIProcessor
    {
        private readonly string _apiKey;
        private readonly string _secretKey;
        private TimeSpan _cacheTime;
        private readonly ILogger<IAPIProcessor> _logger;
        private readonly RequestClient _requestClient;

        public APIProcessor(ILogger<IAPIProcessor> logger, RequestClient requestClient, string apiKey, string secretKey)
        {
            _apiKey = apiKey;
            _secretKey = secretKey;

            _logger = logger;
            _requestClient = requestClient;
        }

        /// <summary>
        ///     Set the cache expiry time
        /// </summary>
        /// <param name="time"></param>
        public void SetCacheTime(TimeSpan time)
        {
            _cacheTime = time;
        }

        /// <summary>
        ///     Processes a GET request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessGetRequest<T>(BinanceEndpointData endpoint, int receiveWindow = 5000)
            where T : class
        {
            var fullKey = $"{typeof(T).Name}-{endpoint.Uri.AbsoluteUri}";

            HttpResponseMessage? message = null;
            switch (endpoint.SecurityType)
            {
                case EndpointSecurityType.ApiKey:
                case EndpointSecurityType.None:
                    message = await _requestClient.GetRequest(endpoint.Uri);
                    break;
                case EndpointSecurityType.Signed:
                    message = await _requestClient.SignedGetRequest(endpoint.Uri, _apiKey, _secretKey,
                        endpoint.Uri.Query, receiveWindow);
                    break;
                default:
                    ThrowHelper.ArgumentOutOfRangeException(nameof(endpoint.SecurityType));
                    break;
            }

            return await HandleResponse<T>(message, endpoint.ToString(), fullKey);
        }

        /// <summary>
        ///     Processes a DELETE request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessDeleteRequest<T>(BinanceEndpointData endpoint, int receiveWindow = 5000)
            where T : class
        {
            var fullKey = $"{typeof(T).Name}-{endpoint.Uri.AbsoluteUri}";

            HttpResponseMessage? message = null;
            switch (endpoint.SecurityType)
            {
                case EndpointSecurityType.ApiKey:
                    message = await _requestClient.DeleteRequest(endpoint.Uri);
                    break;
                case EndpointSecurityType.Signed:
                    message = await _requestClient.SignedDeleteRequest(endpoint.Uri, _apiKey, _secretKey,
                        endpoint.Uri.Query, receiveWindow);
                    break;
                case EndpointSecurityType.None:
                default:
                    ThrowHelper.ArgumentOutOfRangeException(nameof(endpoint.SecurityType));
                    break;
            }

            return await HandleResponse<T>(message, endpoint.ToString(), fullKey);
        }

        /// <summary>
        ///     Processes a POST request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessPostRequest<T>(BinanceEndpointData endpoint, int receiveWindow = 5000)
            where T : class
        {
            var fullKey = $"{typeof(T).Name}-{endpoint.Uri.AbsoluteUri}";

            HttpResponseMessage? message = null;
            switch (endpoint.SecurityType)
            {
                case EndpointSecurityType.ApiKey:
                    message = await _requestClient.PostRequest(endpoint.Uri);
                    break;
                case EndpointSecurityType.None:
                    ThrowHelper.ArgumentOutOfRangeException(nameof(endpoint.SecurityType));
                    break;
                case EndpointSecurityType.Signed:
                    message = await _requestClient.SignedPostRequest(endpoint.Uri, _apiKey, _secretKey,
                        endpoint.Uri.Query, receiveWindow);
                    break;
                default:
                    ThrowHelper.ArgumentOutOfRangeException(nameof(endpoint.SecurityType));
                    break;
            }

            return await HandleResponse<T>(message, endpoint.ToString(), fullKey);
        }

        /// <summary>
        ///     Processes a PUT request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<T> ProcessPutRequest<T>(BinanceEndpointData endpoint, int receiveWindow = 5000)
            where T : class
        {
            var fullKey = $"{typeof(T).Name}-{endpoint.Uri.AbsoluteUri}";

            HttpResponseMessage? message = null;
            switch (endpoint.SecurityType)
            {
                case EndpointSecurityType.ApiKey:
                    message = await _requestClient.PutRequest(endpoint.Uri);
                    break;
                case EndpointSecurityType.None:
                case EndpointSecurityType.Signed:
                default:
                    ThrowHelper.ArgumentOutOfRangeException(nameof(endpoint.SecurityType));
                    break;
            }

            return await HandleResponse<T>(message, endpoint.ToString(), fullKey);
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage message, string requestMessage, string fullCacheKey)
            where T : class
        {
            if (message.IsSuccessStatusCode)
            {
                var messageJson = await message.Content.ReadAsStringAsync();
                T? messageObject = null;
                try
                {
                    messageObject = JsonConvert.DeserializeObject<T>(messageJson);
                }
                catch (Exception ex)
                {
                    var deserializeErrorMessage =
                        $"Unable to deserialize message from: {requestMessage}. Exception: {ex.Message}";
                    _logger.LogError(deserializeErrorMessage);
                    ThrowHelper.BinanceException(deserializeErrorMessage, new BinanceError
                    {
                        RequestMessage = requestMessage,
                        Message = ex.Message
                    });
                }

                _logger.LogDebug($"Successful Message Response={messageJson}");

                if (messageObject == null) ThrowHelper.Exception("Unable to deserialize to provided type");

                return messageObject;
            }

            var errorJson = await message.Content.ReadAsStringAsync();
            var errorObject = JsonConvert.DeserializeObject<BinanceError>(errorJson);
            if (errorObject == null) ThrowHelper.BinanceException("Unexpected Error whilst handling the response", null);
            errorObject.RequestMessage = requestMessage;
            var exception = CreateBinanceException(message.StatusCode, errorObject);
            _logger.LogError($"Error Message Received:{errorJson}", exception);
            throw exception;
        }

        private BinanceException CreateBinanceException(HttpStatusCode statusCode, BinanceError errorObject)
        {
            if (statusCode == HttpStatusCode.GatewayTimeout) return new BinanceTimeoutException(errorObject);
            var parsedStatusCode = (int)statusCode;
            if (parsedStatusCode >= 400 && parsedStatusCode <= 500) return new BinanceBadRequestException(errorObject);
            return parsedStatusCode >= 500
                ? new BinanceServerException(errorObject)
                : new BinanceException("Binance API Error", errorObject);
        }
    }
}