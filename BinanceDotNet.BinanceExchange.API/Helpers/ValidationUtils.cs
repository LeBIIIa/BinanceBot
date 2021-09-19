using BinanceExchange.API.Helpers;

using System.Diagnostics.CodeAnalysis;

namespace BinanceDotNet.BinanceExchange.API.Helpers
{
    internal static class ValidationUtils
    {
        public static void ArgumentNotNull([NotNull] object? value, string parameterName)
        {
            if (value == null)
            {
                ThrowHelper.ArgumentNullException(parameterName);
            }
        }
    }
}
