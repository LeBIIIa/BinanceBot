using BinanceExchange.API.Helpers;

namespace BinanceExchange.API.Utility
{
    internal static class Guard
    {
        public static void AgainstNullOrEmpty(string? param, string? name = null)
        {
            if (string.IsNullOrEmpty(param))
                ThrowHelper.ArgumentNullException(name ?? "The Guarded argument was null or empty.");
        }

        public static void AgainstNull(object? param, string? name = null)
        {
            if (param == null)
                ThrowHelper.ArgumentNullException(name ?? "The Guarded argument was null.");
        }
    }
}