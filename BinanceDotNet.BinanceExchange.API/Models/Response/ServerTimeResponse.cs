﻿using BinanceExchange.API.Converter;

using Newtonsoft.Json;

using System;
using System.Runtime.Serialization;

namespace BinanceExchange.API.Models.Response
{
    /// <summary>
    ///     The current server time
    /// </summary>
    [DataContract]
    public class ServerTimeResponse : IResponse
    {
        [DataMember(Order = 1)]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime ServerTime { get; set; }
    }
}