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
using FinanceSharp.Data;
using Python.Runtime;
using static FinanceSharp.Constants;
using FinanceSharp.Data;

namespace FinanceSharp.Indicators {
    /// <summary>
    /// 	 The Acceleration Bands created by Price Headley plots upper and lower envelope bands around a moving average.
    /// </summary>
    /// <seealso cref="Indicators.IndicatorBase{DoubleArray}" />
    public class AccelerationBands : BarIndicator {
        private readonly double _width;

        /// <summary>
        /// 	 Gets the type of moving average
        /// </summary>
        public MovingAverageType MovingAverageType { get; }

        /// <summary>
        /// 	 Gets the middle acceleration band (moving average)
        /// </summary>
        public IndicatorBase MiddleBand { get; }

        /// <summary>
        /// 	 Gets the upper acceleration band  (High * ( 1 + Width * (High - Low) / (High + Low)))
        /// </summary>
        public IndicatorBase UpperBand { get; }

        /// <summary>
        /// 	 Gets the lower acceleration band  (Low * (1 - Width * (High - Low)/ (High + Low)))
        /// </summary>
        public IndicatorBase LowerBand { get; }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="AccelerationBands" /> class.
        /// </summary>
        /// <param name="name">The name of this indicator.</param>
        /// <param name="period">The period of the three moving average (middle, upper and lower band).</param>
        /// <param name="width">A coefficient specifying the distance between the middle band and upper or lower bands.</param>
        /// <param name="movingAverageType">Type of the moving average.</param>
        public AccelerationBands(string name, int period, double width,
            MovingAverageType movingAverageType = MovingAverageType.Simple)
            : base(name) {
            WarmUpPeriod = period;
            _width = width;
            MovingAverageType = movingAverageType;
            MiddleBand = movingAverageType.AsIndicator(name + "_MiddleBand", period);
            LowerBand = movingAverageType.AsIndicator(name + "_LowerBand", period);
            UpperBand = movingAverageType.AsIndicator(name + "_UpperBand", period);
        }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="AccelerationBands" /> class.
        /// </summary>
        /// <param name="period">The period of the three moving average (middle, upper and lower band).</param>
        /// <param name="width">A coefficient specifying the distance between the middle band and upper or lower bands.</param>
        public AccelerationBands(int period, double width)
            : this($"ABANDS({period},{width})", period, width) { }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="AccelerationBands" /> class.
        /// </summary>
        /// <param name="period">The period of the three moving average (middle, upper and lower band).</param>
        public AccelerationBands(int period)
            : this(period, 4) { }

        /// <summary>
        /// 	 Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady => MiddleBand.IsReady && LowerBand.IsReady && UpperBand.IsReady;

        /// <summary>
        /// 	 Required period, in data points, for the indicator to be ready and fully initialized.
        /// </summary>
        public int WarmUpPeriod { get; }

        /// <summary>
        /// 	 Resets this indicator to its initial state
        /// </summary>
        public override void Reset() {
            MiddleBand.Reset();
            LowerBand.Reset();
            UpperBand.Reset();
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
            var coeff = _width * (input[HighIdx] - input[LowIdx]) / (input[HighIdx] + input[LowIdx]);
            LowerBand.Update(time, input[LowIdx] * (1 - coeff));
            UpperBand.Update(time, input[HighIdx] * (1 + coeff));
            MiddleBand.Update(time, input[CloseIdx]);

            return MiddleBand;
        }
    }
}