﻿using System;

namespace BinanceBot.Market
{
    /// <summary>
    /// Market Strategy configuration
    /// </summary>
    /// <remarks>
    /// <see cref="MarketStrategyConfiguration"/> limits must not contradict Stock limits.
    /// Binance limits: <see href="https://api.binance.com/api/v1/exchangeInfo"/>
    /// </remarks>
    public class MarketStrategyConfiguration
    {
        /// <summary>
        /// Start trading when spread greater than that values (in percentage point)
        /// </summary>
        public decimal TradeWhenSpreadGreaterThan { get; set; }


        #region Order limits
        /// <summary>
        /// Minimal order volume
        /// </summary>
        public decimal MinOrderVolume { get; set; }

        /// <summary>
        /// Maximum order volume
        /// </summary>
        public decimal MaxOrderVolume { get; set; }
        #endregion


        #region Day limits (not usage now, but usefull in future)
        /// <summary>
        /// Minimal order volume
        /// </summary>
        public decimal MinVolumePerDay { get; set; }

        /// <summary>
        /// Maximum order volume
        /// </summary>
        public decimal MaxVolumePerDay { get; set; }
        #endregion


        #region Behaviour settings (not usage now, but usefull in future)
        public bool CancelOrdersWhenStopping { get; set; } = true;

        public TimeSpan ReceiveWindow { get; set; } = TimeSpan.FromSeconds(5);

        public (TimeSpan From, TimeSpan To) WorkingTime { get; set; }
        #endregion
    }
}