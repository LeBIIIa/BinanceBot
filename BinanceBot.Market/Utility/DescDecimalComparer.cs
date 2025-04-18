﻿using System.Collections.Generic;

namespace BinanceBot.Market.Utility
{
    /// <summary>
    /// Descending decimal comparer
    /// </summary>
    internal class DescendingDecimalComparer : IComparer<decimal>
    {
        public int Compare(decimal x, decimal y) =>
            decimal.Compare(x, y) * -1;
    }
}