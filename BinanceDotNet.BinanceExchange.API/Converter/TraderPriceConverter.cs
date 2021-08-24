using BinanceExchange.API.Helpers;

using BinanceExchange.API.Models.Response;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BinanceExchange.API.Converter
{
    public class TraderPriceConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            ThrowHelper.NotImplementedException();
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            var tradePrices = JArray.Load(reader);
            var list = new List<TradeResponse>();
            foreach (var tradePrice in tradePrices)
            {
                var price = tradePrice.ElementAt(0).ToObject<decimal>();
                var quantity = tradePrice.ElementAt(1).ToObject<decimal>();
                list.Add(new TradeResponse
                {
                    Price = price,
                    Quantity = quantity
                });
            }

            return list;
        }

        public override bool CanConvert(Type objectType)
        {
            return ThrowHelper.NotImplementedException<bool>();
        }
    }
}