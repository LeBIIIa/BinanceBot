using BinanceExchange.API.Helpers;

using Newtonsoft.Json;

using System;
using System.Globalization;

namespace BinanceExchange.API.Converter
{
    public class StringDecimalConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal) || objectType == typeof(decimal?);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            return ThrowHelper.NotImplementedException<object>();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value != null)
                writer.WriteValue(((decimal)value).ToString("F6", CultureInfo.InvariantCulture));
        }
    }
}
