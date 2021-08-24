using BinanceExchange.API.Models.Response;

using Newtonsoft.Json;

using System.Runtime.Serialization;

namespace BinanceExchange.API.Models.WebSocket
{
    [DataContract]
    public class BalanceResponseData : IBalanceResponse
    {
        [JsonProperty(PropertyName = "a")]
        public string? Asset { get; set; }

        [JsonProperty(PropertyName = "f")]
        public decimal Free { get; set; }

        [JsonProperty(PropertyName = "l")]
        public decimal Locked { get; set; }
    }
}