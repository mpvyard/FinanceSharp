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

namespace FinanceSharp.Indicators {
    /// <summary>
    /// 	 The Detrended Price Oscillator is an indicator designed to remove trend from price
    /// 	 and make it easier to identify cycles.
    /// 	 DPO does not extend to the last date because it is based on a displaced moving average.
    /// 	 Is estimated as Price {X/2 + 1} periods ago less the X-period simple moving average.
    /// 	 E.g.DPO(20) equals price 11 days ago less the 20-day SMA.
    /// </summary>
    /// <seealso cref="IndicatorBase{DoubleArray}" />
    public class DetrendedPriceOscillator : IndicatorBase {
        private readonly Delay _priceLag;
        private readonly SimpleMovingAverage _sma;

        /// <summary>
        /// 	 Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady => _sma.IsReady && _priceLag.IsReady;

        /// <summary>
        /// 	 Required period, in data points, for the indicator to be ready and fully initialized.
        /// </summary>
        public override int WarmUpPeriod { get; }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="DetrendedPriceOscillator" /> class.
        /// </summary>
        /// <param name="name">The name for the indicator.</param>
        /// <param name="period">The number of periods to calculate the DPO.</param>
        public DetrendedPriceOscillator(string name, int period)
            : base(name) {
            var lagPeriod = period / 2 + 1;
            _priceLag = new Delay(lagPeriod);
            _sma = new SimpleMovingAverage(period);
            WarmUpPeriod = period;
        }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="DetrendedPriceOscillator" /> class.
        /// </summary>
        /// <param name="period">The number of periods to calculate the DPO.</param>
        public DetrendedPriceOscillator(int period)
            : this($"DPO({period})", period) { }

        /// <summary>
        /// 	 Resets this indicator to its initial state
        /// </summary>
        public override void Reset() {
            _priceLag.Reset();
            _sma.Reset();
            base.Reset();
        }

        /// <summary>
        /// 	 Computes the next value of this indicator from the given state
        /// </summary>
        /// <param name="time"></param>
        /// <param name="input">The input given to the indicator</param>
        /// <returns>
        /// 	 A new value for this indicator
        /// </returns>
        protected override DoubleArray Forward(long time, DoubleArray input) {
            _priceLag.Update(time, input);
            _sma.Update(time, input);
            return _priceLag - _sma;
        }
    }
}