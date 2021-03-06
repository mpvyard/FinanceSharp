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


namespace FinanceSharp.Indicators.CandlestickPatterns {
    /// <summary>
    /// 	 Counterattack candlestick pattern
    /// </summary>
    /// <remarks>
    /// 	 Must have:
    /// - first candle: long black (white)
    /// - second candle: long white(black) with close equal to the prior close
    /// 	 The meaning of "equal" and "long" is specified with SetCandleSettings
    /// 	 The returned value is positive(+1) when bullish or negative(-1) when bearish;
    /// 	 The user should consider that counterattack is significant in a trend, while this function does not consider it
    /// </remarks>
    public class Counterattack : CandlestickPattern {
        private readonly int _equalAveragePeriod;
        private readonly int _bodyLongAveragePeriod;

        private double _equalPeriodTotal;
        private DoubleArray _bodyLongPeriodTotal = new DoubleArray2DManaged(1, 2);

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="Counterattack"/> class using the specified name.
        /// </summary>
        /// <param name="name">The name of this indicator</param>
        public Counterattack(string name)
            : base(name, Math.Max(CandleSettings.Get(CandleSettingType.Equal).AveragePeriod, CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod) + 1 + 1) {
            _equalAveragePeriod = CandleSettings.Get(CandleSettingType.Equal).AveragePeriod;
            _bodyLongAveragePeriod = CandleSettings.Get(CandleSettingType.BodyLong).AveragePeriod;
        }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="Counterattack"/> class.
        /// </summary>
        public Counterattack()
            : this("COUNTERATTACK") { }

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
                if (Samples >= Period - _equalAveragePeriod) {
                    _equalPeriodTotal += GetCandleRange(CandleSettingType.Equal, window[1]);
                }

                if (Samples >= Period - _bodyLongAveragePeriod) {
                    _bodyLongPeriodTotal[1] += GetCandleRange(CandleSettingType.BodyLong, window[1]);
                    _bodyLongPeriodTotal[0] += GetCandleRange(CandleSettingType.BodyLong, input);
                }

                return Constants.Zero;
            }

            double value;
            if (
                // opposite candles
                (int) GetCandleColor(window[1]) == -(int) GetCandleColor(input) &&
                // 1st long
                GetRealBody(window[1]) > GetCandleAverage(CandleSettingType.BodyLong, _bodyLongPeriodTotal[1], window[1]) &&
                // 2nd long
                GetRealBody(input) > GetCandleAverage(CandleSettingType.BodyLong, _bodyLongPeriodTotal[0], input) &&
                // equal closes
                input[CloseIdx] <= window[1].Close + GetCandleAverage(CandleSettingType.Equal, _equalPeriodTotal, window[1]) &&
                input[CloseIdx] >= window[1].Close - GetCandleAverage(CandleSettingType.Equal, _equalPeriodTotal, window[1])
            )
                value = (int) GetCandleColor(input);
            else
                value = Constants.Zero;

            // add the current range and subtract the first range: this is done after the pattern recognition 
            // when avgPeriod is not 0, that means "compare with the previous candles" (it excludes the current candle)

            _equalPeriodTotal += GetCandleRange(CandleSettingType.Equal, input) -
                                 GetCandleRange(CandleSettingType.Equal, window[_equalAveragePeriod + 1]);

            for (var i = 1; i >= 0; i--) {
                _bodyLongPeriodTotal[i] += GetCandleRange(CandleSettingType.BodyLong, window[i]) -
                                           GetCandleRange(CandleSettingType.BodyLong, window[i + _bodyLongAveragePeriod]);
            }

            return value;
        }

        /// <summary>
        /// 	 Resets this indicator to its initial state
        /// </summary>
        public override void Reset() {
            _equalPeriodTotal = 0;
            _bodyLongPeriodTotal = new DoubleArray2DManaged(1, 2);
            base.Reset();
        }
    }
}