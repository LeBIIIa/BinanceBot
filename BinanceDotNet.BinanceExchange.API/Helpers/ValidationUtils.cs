using BinanceExchange.API.Helpers;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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
