﻿using BinanceExchange.API.Enums;
using BinanceExchange.API.Extensions;
using BinanceExchange.API.Helpers;

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BinanceExchange.API.Client
{
    public class RequestClient
    {
        private static RequestClient? requestClient = null;

        private readonly string APIHeader = "X-MBX-APIKEY";
        private readonly HttpClient HttpClient;
        private readonly SemaphoreSlim _rateSemaphore;
        private const int _limit = 10;

        /// <summary>
        ///     Number of seconds the for the Limit of requests (10 seconds for 10 requests etc)
        /// </summary>
        private const int SecondsLimit = 10;

        private bool RateLimitingEnabled;
        private readonly Stopwatch Stopwatch;
        private int _concurrentRequests;
        private TimeSpan _timestampOffset;
        private TimeSpan _cacheTime;

        private readonly object LockObject = new();

        private readonly Serilog.ILogger _logger;

        protected internal RequestClient(Serilog.ILogger logger)
        {
            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };
            HttpClient = new HttpClient(httpClientHandler);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _rateSemaphore = new SemaphoreSlim(_limit, _limit);
            Stopwatch = new Stopwatch();
            _logger = logger;
        }

        public static RequestClient GetInstance(Serilog.ILogger logger)
        {
            if (requestClient == null)
                requestClient = new RequestClient(logger);

            return requestClient;

        }

        /// <summary>
        ///     Used to adjust the client timestamp
        /// </summary>
        /// <param name="time">TimeSpan to adjust timestamp by</param>
        public void SetTimestampOffset(TimeSpan time)
        {
            _timestampOffset = time;
            _logger.Debug($"Timestamp offset is now : {time}");
        }

        /// <summary>
        ///     Set the cache expiry time
        /// </summary>
        /// <param name="time"></param>
        public void SetCacheTime(TimeSpan time)
        {
            _cacheTime = time;
            HttpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { NoCache = false, MaxAge = _cacheTime };
        }

        /// <summary>
        ///     Sets whether Rate limiting is enabled or disabled
        /// </summary>
        /// <param name="enabled"></param>
        public void SetRateLimiting(bool enabled)
        {
            var set = enabled ? "enabled" : "disabled";
            RateLimitingEnabled = enabled;
            _logger.Debug($"Rate Limiting has been {set}");
        }

        /// <summary>
        ///     Assigns a new seconds limit
        /// </summary>
        /// <param name="key">Your API Key</param>
        public void SetAPIKey(string key)
        {
            if (HttpClient.DefaultRequestHeaders.Contains(APIHeader))
                lock (LockObject)
                {
                    if (HttpClient.DefaultRequestHeaders.Contains(APIHeader))
                        HttpClient.DefaultRequestHeaders.Remove(APIHeader);
                }

            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(APIHeader, new[] { key });
        }

        /// <summary>
        ///     Create a generic GetRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetRequest(Uri endpoint)
        {
            _logger.Debug($"Creating a GET Request to {endpoint.AbsoluteUri}");
            return await CreateRequest(endpoint);
        }

        /// <summary>
        ///     Creates a generic GET request that is signed
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SignedGetRequest(Uri endpoint, string _, string secretKey,
            string signatureRawData, long receiveWindow = 5000)
        {
            _logger.Debug($"Creating a SIGNED GET Request to {endpoint.AbsoluteUri}");
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            _logger.Debug($"Concat URL for request: {uri.AbsoluteUri}");
            return await CreateRequest(uri, HttpVerb.GET);
        }

        /// <summary>
        ///     Create a generic PostRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostRequest(Uri endpoint)
        {
            _logger.Debug($"Creating a POST Request to {endpoint.AbsoluteUri}");
            return await CreateRequest(endpoint, HttpVerb.POST);
        }

        /// <summary>
        ///     Create a generic DeleteRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> DeleteRequest(Uri endpoint)
        {
            _logger.Debug($"Creating a DELETE Request to {endpoint.AbsoluteUri}");
            return await CreateRequest(endpoint, HttpVerb.DELETE);
        }

        /// <summary>
        ///     Create a generic PutRequest to the specified endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PutRequest(Uri endpoint)
        {
            _logger.Debug($"Creating a PUT Request to {endpoint.AbsoluteUri}");
            return await CreateRequest(endpoint, HttpVerb.PUT);
        }

        /// <summary>
        ///     Creates a generic GET request that is signed
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SignedPostRequest(Uri endpoint, string _, string secretKey,
            string signatureRawData, long receiveWindow = 5000)
        {
            _logger.Debug($"Creating a SIGNED POST Request to {endpoint.AbsoluteUri}");
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            return await CreateRequest(uri, HttpVerb.POST);
        }

        /// <summary>
        ///     Creates a generic DELETE request that is signed
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SignedDeleteRequest(Uri endpoint, string _, string secretKey,
            string signatureRawData, long receiveWindow = 5000)
        {
            _logger.Debug($"Creating a SIGNED DELETE Request to {endpoint.AbsoluteUri}");
            var uri = CreateValidUri(endpoint, secretKey, signatureRawData, receiveWindow);
            return await CreateRequest(uri, HttpVerb.DELETE);
        }

        /// <summary>
        ///     Creates a valid Uri with signature
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="secretKey"></param>
        /// <param name="signatureRawData"></param>
        /// <param name="receiveWindow"></param>
        /// <returns></returns>
        private Uri CreateValidUri(Uri endpoint, string secretKey, string signatureRawData, long receiveWindow)
        {
            string timestamp =
            DateTime.UtcNow.AddMilliseconds(_timestampOffset.TotalMilliseconds).ConvertToUnixTime().ToString();
            var qsDataProvided = !string.IsNullOrEmpty(signatureRawData);
            var argEnding = $"timestamp={timestamp}&recvWindow={receiveWindow}";
            var adjustedSignature = !string.IsNullOrEmpty(signatureRawData)
                ? $"{signatureRawData[1..]}&{argEnding}"
                : $"{argEnding}";
            var hmacResult = CreateHMACSignature(secretKey, adjustedSignature);
            var connector = !qsDataProvided ? "?" : "&";
            var uri = new Uri($"{endpoint}{connector}{argEnding}&signature={hmacResult}");
            return uri;
        }

        /// <summary>
        ///     Creates a HMACSHA256 Signature based on the key and total parameters
        /// </summary>
        /// <param name="key">The secret key</param>
        /// <param name="totalParams">URL Encoded values that would usually be the query string for the request</param>
        /// <returns></returns>
        private static string CreateHMACSignature(string key, string totalParams)
        {
            var messageBytes = Encoding.UTF8.GetBytes(totalParams);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var hash = new HMACSHA256(keyBytes);
            var computedHash = hash.ComputeHash(messageBytes);
            return BitConverter.ToString(computedHash).Replace("-", "").ToLower();
        }

        /// <summary>
        ///     Makes a request to the specified Uri, only if it hasn't exceeded the call limit
        /// </summary>
        /// <param name="endpoint">Endpoint to request</param>
        /// <param name="verb"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> CreateRequest(Uri endpoint, HttpVerb verb = HttpVerb.GET)
        {
            Task<HttpResponseMessage>? task = null;

            if (RateLimitingEnabled)
            {
                await _rateSemaphore.WaitAsync();
                if (Stopwatch.Elapsed.Seconds >= SecondsLimit || _rateSemaphore.CurrentCount == 0 ||
                    _concurrentRequests == _limit)
                {
                    var seconds = (SecondsLimit - Stopwatch.Elapsed.Seconds) * 1000;
                    var sleep = seconds > 0 ? seconds : seconds * -1;
                    Thread.Sleep(sleep);
                    _concurrentRequests = 0;
                    Stopwatch.Restart();
                }

                ++_concurrentRequests;
            }

            var taskFunction = new Func<Task<HttpResponseMessage>, Task<HttpResponseMessage>>(t =>
            {
                if (!RateLimitingEnabled) return t;
                _rateSemaphore.Release();
                if (_rateSemaphore.CurrentCount != _limit || Stopwatch.Elapsed.Seconds < SecondsLimit) return t;
                Stopwatch.Restart();
                --_concurrentRequests;
                return t;
            });

            switch (verb)
            {
                case HttpVerb.GET:
                    task = await HttpClient.GetAsync(endpoint)
                        .ContinueWith(taskFunction);
                    break;
                case HttpVerb.POST:
                    task = await HttpClient.PostAsync(endpoint, null!)
                        .ContinueWith(taskFunction);
                    break;
                case HttpVerb.DELETE:
                    task = await HttpClient.DeleteAsync(endpoint)
                        .ContinueWith(taskFunction);
                    break;
                case HttpVerb.PUT:
                    task = await HttpClient.PutAsync(endpoint, null!)
                        .ContinueWith(taskFunction);
                    break;
                default:
                    ThrowHelper.ArgumentOutOfRangeException(nameof(verb), verb, null);
                    break;
            }

            return await task;
        }
    }
}