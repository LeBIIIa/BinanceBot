using BinanceExchange.API.Helpers;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using static BinanceDotNet.BinanceExchange.API.Extensions.LinqExtensions;

namespace BinanceDotNet.BinanceExchange.API.Helpers
{
    internal static class StringUtils
    {
        private enum SeparatedCaseState
        {
            Start,
            Lower,
            Upper,
            NewWord
        }

        public const string CarriageReturnLineFeed = "\r\n";
        public const string Empty = "";
        public const char CarriageReturn = '\r';
        public const char LineFeed = '\n';
        public const char Tab = '\t';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty([NotNullWhen(false)] string? value)
        {
            return string.IsNullOrEmpty(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatWith(this string format, IFormatProvider provider, object? arg0)
        {
            return format.FormatWith(provider, new object?[] { arg0 });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatWith(this string format, IFormatProvider provider, object? arg0, object? arg1)
        {
            return format.FormatWith(provider, new object?[] { arg0, arg1 });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatWith(this string format, IFormatProvider provider, object? arg0, object? arg1, object? arg2)
        {
            return format.FormatWith(provider, new object?[] { arg0, arg1, arg2 });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatWith(this string format, IFormatProvider provider, object? arg0, object? arg1, object? arg2, object? arg3)
        {
            return format.FormatWith(provider, new object?[] { arg0, arg1, arg2, arg3 });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string FormatWith(this string format, IFormatProvider provider, params object?[] args)
        {
            // leave this a private to force code to use an explicit overload
            // avoids stack memory being reserved for the object array
            ValidationUtils.ArgumentNotNull(format, nameof(format));

            return string.Format(provider, format, args);
        }

        /// <summary>
        /// Determines whether the string is all white space. Empty string will return <c>false</c>.
        /// </summary>
        /// <param name="s">The string to test whether it is all white space.</param>
        /// <returns>
        /// 	<c>true</c> if the string is all white space; otherwise, <c>false</c>.
        /// </returns>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteSpace(string s)
        {
            if (s == null)
            {
                ThrowHelper.ArgumentNullException(nameof(s));
            }

            if (s.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < s.Length; i++)
            {
                if (!char.IsWhiteSpace(s[i]))
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringWriter CreateStringWriter(int capacity)
        {
            StringBuilder sb = new(capacity);
            StringWriter sw = new(sb, CultureInfo.InvariantCulture);

            return sw;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToCharAsUnicode(char c, char[] buffer)
        {
            buffer[0] = '\\';
            buffer[1] = 'u';
            buffer[2] = MathUtils.IntToHex((c >> 12) & '\x000f');
            buffer[3] = MathUtils.IntToHex((c >> 8) & '\x000f');
            buffer[4] = MathUtils.IntToHex((c >> 4) & '\x000f');
            buffer[5] = MathUtils.IntToHex(c & '\x000f');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TSource? ForgivingCaseSensitiveFind<TSource>(this List<TSource> source, Func<TSource, string> valueSelector, string testValue)
        {
            if (source == null)
            {
                ThrowHelper.ArgumentNullException(nameof(source));
            }
            if (valueSelector == null)
            {
                ThrowHelper.ArgumentNullException(nameof(valueSelector));
            }

            List<TSource> caseInsensitiveResults = new(source.Count);
            foreach (var s in source)
            {
                if (string.Equals(valueSelector(s), testValue, StringComparison.OrdinalIgnoreCase))
                    caseInsensitiveResults.Add(s);
            }
            if (caseInsensitiveResults.Count <= 1)
            {
                return caseInsensitiveResults.FirstOrDefault();
            }
            else
            {
                // multiple results returned. now filter using case sensitivity
                List<TSource> caseSensitiveResults = new(source.Count);
                foreach (var s in source)
                {
                    if (string.Equals(valueSelector(s), testValue, StringComparison.Ordinal))
                        caseSensitiveResults.Add(s);
                }
                return caseSensitiveResults.FirstOrDefault();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToCamelCase(string s)
        {
            if (IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    // if the next character is a space, which is not considered uppercase 
                    // (otherwise we wouldn't be here...)
                    // we want to ensure that the following:
                    // 'FOO bar' is rewritten as 'foo bar', and not as 'foO bar'
                    // The code was written in such a way that the first word in uppercase
                    // ends when if finds an uppercase letter followed by a lowercase letter.
                    // now a ' ' (space, (char)32) is considered not upper
                    // but in that case we still want our current character to become lowercase
                    if (char.IsSeparator(chars[i + 1]))
                    {
                        chars[i] = ToLower(chars[i]);
                    }

                    break;
                }

                chars[i] = ToLower(chars[i]);
            }

            return new string(chars);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char ToLower(char c)
        {
            c = char.ToLowerInvariant(c);
            return c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToSnakeCase(string s) => ToSeparatedCase(s, '_');

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToKebabCase(string s) => ToSeparatedCase(s, '-');

        private static string ToSeparatedCase(string s, char separator)
        {
            if (IsNullOrEmpty(s))
            {
                return s;
            }

            StringBuilder sb = new();
            SeparatedCaseState state = SeparatedCaseState.Start;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ')
                {
                    if (state != SeparatedCaseState.Start)
                    {
                        state = SeparatedCaseState.NewWord;
                    }
                }
                else if (char.IsUpper(s[i]))
                {
                    switch (state)
                    {
                        case SeparatedCaseState.Upper:
                            bool hasNext = (i + 1 < s.Length);
                            if (i > 0 && hasNext)
                            {
                                char nextChar = s[i + 1];
                                if (!char.IsUpper(nextChar) && nextChar != separator)
                                {
                                    sb.Append(separator);
                                }
                            }
                            break;
                        case SeparatedCaseState.Lower:
                        case SeparatedCaseState.NewWord:
                            sb.Append(separator);
                            break;
                    }

                    char c = char.ToLowerInvariant(s[i]);
                    sb.Append(c);

                    state = SeparatedCaseState.Upper;
                }
                else if (s[i] == separator)
                {
                    sb.Append(separator);
                    state = SeparatedCaseState.Start;
                }
                else
                {
                    if (state == SeparatedCaseState.NewWord)
                    {
                        sb.Append(separator);
                    }

                    sb.Append(s[i]);
                    state = SeparatedCaseState.Lower;
                }
            }

            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHighSurrogate(char c)
        {
            return char.IsHighSurrogate(c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLowSurrogate(char c)
        {
            return char.IsLowSurrogate(c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith(this string source, char value)
        {
            return (source.Length > 0 && source[0] == value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWith(this string source, char value)
        {
            return (source.Length > 0 && source[^1] == value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Trim(this string s, int start, int length)
        {
            // References: https://referencesource.microsoft.com/#mscorlib/system/string.cs,2691
            // https://referencesource.microsoft.com/#mscorlib/system/string.cs,1226
            if (s == null)
            {
                ThrowHelper.ArgumentNullException(nameof(s));
            }
            if (start < 0)
            {
                ThrowHelper.ArgumentOutOfRangeException(nameof(start));
            }
            if (length < 0)
            {
                ThrowHelper.ArgumentOutOfRangeException(nameof(length));
            }
            int end = start + length - 1;
            if (end >= s.Length)
            {
                ThrowHelper.ArgumentOutOfRangeException(nameof(length));
            }
            for (; start < end; start++)
            {
                if (!char.IsWhiteSpace(s[start]))
                {
                    break;
                }
            }
            for (; end >= start; end--)
            {
                if (!char.IsWhiteSpace(s[end]))
                {
                    break;
                }
            }
            return s.Substring(start, end - start + 1);
        }
    }
}
