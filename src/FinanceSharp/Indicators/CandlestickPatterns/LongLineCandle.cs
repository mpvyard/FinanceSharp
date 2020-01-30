﻿/*
 * All Rights reserved to Ebby Technologies LTD @ Eli Belash, 2020.
 * Original code by QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/
using System;
using static FinanceSharp.Constants;
using FinanceSharp.Data;


namespace FinanceSharp.Indicators.CandlestickPatterns {
    /// <summary>
    /// 	 Long Line Candle candlestick pattern indicator
    /// </summary>
    /// <remarks>
    /// 	 Must have:
    /// - long real body
    /// - short upper and lower shadow
    /// 	 The meaning of "long" and "short" is specified with SetCandleSettings
    /// 	 The returned value is positive(+1) when white(bullish), negative(-1) when black(bearish)
    /// </remarks>
    public class LongLineCandle : CandlestickPattern {
        private readonly int _bodyLongAveragePeriod;
        private readonly int _shadowShortAveragePeriod;

        private double _bodyLongPeriodTotal;
        private double _shadowShortPeriodTotal;

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="LongLineCandle"/> class using the specified name.
        /// </summary>
        /// <param name="name">The name of this indicator</param>
        public LongLineCandle(string name)
            : base(name, Math.Max(CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod, CandleSettings.Get(CandleSettingType.ShadowShort).AveragePeriod) + 1) {
            _bodyLongAveragePeriod = CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod;
            _shadowShortAveragePeriod = CandleSettings.Get(CandleSettingType.ShadowShort).AveragePeriod;
        }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="LongLineCandle"/> class.
        /// </summary>
        public LongLineCandle()
            : this("LONGLINECANDLE") { }

        /// <summary>
        /// 	 Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady {
            get { return Samples >= Period; }
        }

        /// <summary>
        /// 	 Computes the next value of this indicator from the given state
        /// </summary>
        /// <param name="timeWindow"></param>
        /// <param name="window">The window of data held in this indicator</param>
        /// <param name="time"></param>
        /// <param name="input"></param>
        /// <returns>A new value for this indicator</returns>
        protected override DoubleArray Forward(IReadOnlyWindow<long> timeWindow, IReadOnlyWindow<DoubleArray> window, long time, DoubleArray input) {
            if (!IsReady) {
                if (Samples >= Period - _bodyLongAveragePeriod) {
                    _bodyLongPeriodTotal += GetCandleRange(CandleSettingType.BodyLong, input);
                }

                if (Samples >= Period - _shadowShortAveragePeriod) {
                    _shadowShortPeriodTotal += GetCandleRange(CandleSettingType.ShadowShort, input);
                }

                return Constants.Zero;
            }

            double value;
            if (GetRealBody(input) > GetCandleAverage(CandleSettingType.BodyLong, _bodyLongPeriodTotal, input) &&
                GetUpperShadow(input) < GetCandleAverage(CandleSettingType.ShadowShort, _shadowShortPeriodTotal, input) &&
                GetLowerShadow(input) < GetCandleAverage(CandleSettingType.ShadowShort, _shadowShortPeriodTotal, input)
            )
                value = (int) GetCandleColor(input);
            else
                value = Constants.Zero;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _bodyLongPeriodTotal += GetCandleRange(CandleSettingType.BodyLong, input) -
                                    GetCandleRange(CandleSettingType.BodyLong, window[_bodyLongAveragePeriod]);

            _shadowShortPeriodTotal += GetCandleRange(CandleSettingType.ShadowShort, input) -
                                       GetCandleRange(CandleSettingType.ShadowShort, window[_shadowShortAveragePeriod]);

            return value;
        }

        /// <summary>
        /// 	 Resets this indicator to its initial state
        /// </summary>
        public override void Reset() {
            _bodyLongPeriodTotal = Constants.Zero;
            _shadowShortPeriodTotal = Constants.Zero;
            base.Reset();
        }
    }
}