using BinanceExchange.API.Models.Response.Error;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace BinanceExchange.API.Helpers
{
    [ExcludeFromCodeCoverage]
    public static class ThrowHelper
    {
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ArgumentException(string paramName, string? message = default)
            => throw new ArgumentException(paramName: paramName, message: message);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ArgumentNullException(string paramName, string? message = default)
            => throw new ArgumentNullException(paramName: paramName, message: message);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T ArgumentNullException<T>(string paramName, string? message = default)
            => throw new ArgumentNullException(paramName: paramName, message: message);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ref readonly T ArgumentNullExceptionRef<T>(string paramName, string? message = default)
            => throw new ArgumentNullException(paramName: paramName, message: message);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void NotImplementedException(string? message = default)
            => throw new NotImplementedException(message);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static T NotImplementedException<T>(string? message = default)
            => throw new NotImplementedException(message);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ArgumentOutOfRangeException(string paramName, string? message = default)
            => throw new ArgumentOutOfRangeException(paramName: paramName, message: message);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ArgumentOutOfRangeException(string paramName, object? actualValue, string? message = default)
            => throw new ArgumentOutOfRangeException(paramName: paramName, actualValue: actualValue, message: message);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T ArgumentOutOfRangeException<T>(string paramName, string? message = default)
            => throw new ArgumentOutOfRangeException(paramName: paramName, message: message);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ref readonly T ArgumentOutOfRangeExceptionRef<T>(string paramName, string? message = default)
            => throw new ArgumentOutOfRangeException(paramName: paramName, message: message);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void InvalidOperationException(string? message = default)
            => throw new InvalidOperationException(message);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T InvalidOperationException<T>()
            => throw new InvalidOperationException();

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void NotSupportedException()
            => throw new NotSupportedException();

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T NotSupportedException<T>()
            => throw new NotSupportedException();

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ObjectDisposedException(string objectName)
            => throw new ObjectDisposedException(objectName);

        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void BinanceException(string? message = default, BinanceError? errorDetails = default)
            => throw new BinanceException(message, errorDetails);
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Exception(string? message = default, Exception? innerException = default)
            => throw new Exception(message, innerException);
    }
}
