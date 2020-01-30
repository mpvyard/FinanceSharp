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
using static FinanceSharp.Constants;
using FinanceSharp.Data;


namespace FinanceSharp.Indicators.CandlestickPatterns {
    /// <summary>
    /// 	 Spinning Top candlestick pattern indicator
    /// </summary>
    /// <remarks>
    /// 	 Must have:
    /// - small real body
    /// - shadows longer than the real body
    /// 	 The meaning of "short" is specified with SetCandleSettings
    /// 	 The returned value is positive(+1) when white or negative(-1) when black;
    /// 	 it does not mean bullish or bearish
    /// </remarks>
    public class SpinningTop : CandlestickPattern {
        private readonly int _bodyShortAveragePeriod;

        private double _bodyShortPeriodTotal;

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="SpinningTop"/> class using the specified name.
        /// </summary>
        /// <param name="name">The name of this indicator</param>
        public SpinningTop(string name)
            : base(name, CandleSettings.Get(CandleSettingType.BodyShort).AveragePeriod + 1) {
            _bodyShortAveragePeriod = CandleSettings.Get(CandleSettingType.BodyShort).AveragePeriod;
        }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="SpinningTop"/> class.
        /// </summary>
        public SpinningTop()
            : this("SPINNINGTOP") { }

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
                if (Samples >= Period - _bodyShortAveragePeriod) {
                    _bodyShortPeriodTotal += GetCandleRange(CandleSettingType.BodyShort, input);
                }

                return Constants.Zero;
            }

            double value;
            if (GetRealBody(input) < GetCandleAverage(CandleSettingType.BodyShort, _bodyShortPeriodTotal, input) &&
                GetUpperShadow(input) > GetRealBody(input) &&
                GetLowerShadow(input) > GetRealBody(input)
            )
                value = (int) GetCandleColor(input);
            else
                value = Constants.Zero;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _bodyShortPeriodTotal += GetCandleRange(CandleSettingType.BodyShort, input) -
                                     GetCandleRange(CandleSettingType.BodyShort, window[_bodyShortAveragePeriod]);

            return value;
        }

        /// <summary>
        /// 	 Resets this indicator to its initial state
        /// </summary>
        public override void Reset() {
            _bodyShortPeriodTotal = Constants.Zero;
            base.Reset();
        }
    }
}