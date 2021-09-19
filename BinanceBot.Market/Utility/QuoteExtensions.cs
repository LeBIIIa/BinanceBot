using BinanceExchange.API.Enums;

using System.Collections.Generic;

namespace BinanceBot.Market.Utility
{
    internal static class QuoteExtensions
    {
        public static List<Quote> ToQuotes(this IDictionary<decimal, decimal> source, OrderSide direction)
        {
            if (source != null)
            {
                List<Quote> result = new(source.Count);
                foreach (var item in source)
                {
                    result.Add(new Quote(item.Key, item.Value, direction));
                }
                return result;
            }
            else
                return new List<Quote>();
        }
    }
}