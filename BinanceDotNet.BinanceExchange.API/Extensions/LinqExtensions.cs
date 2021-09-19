using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BinanceDotNet.BinanceExchange.API.Extensions
{
    public static class LinqExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource? FirstOrDefault<TSource>(this List<TSource> source)
               => source switch
               {
                   { Count: 0 } => default,
                   _ => source[0]
               };
    }
}
