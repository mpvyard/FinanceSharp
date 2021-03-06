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


namespace FinanceSharp.Indicators {
    /// <summary>
    /// 	 This indicator computes the Accumulation/Distribution (AD)
    /// 	 The Accumulation/Distribution is calculated using the following formula:
    /// 	 AD = AD + ((Close - Low) - (High - Close)) / (High - Low) * Volume
    /// </summary>
    public class AccumulationDistribution : TradeBarIndicator {
        /// <summary>
        /// 	 Initializes a new instance of the <see cref="AccumulationDistribution"/> class using the specified name.
        /// </summary>
        public AccumulationDistribution()
            : this("AD") { }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="AccumulationDistribution"/> class using the specified name.
        /// </summary>
        /// <param name="name">The name of this indicator</param>
        public AccumulationDistribution(string name)
            : base(name) { }

        /// <summary>
        /// 	 Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady => Samples > 0;

        /// <summary>
        /// 	 Required period, in data points, for the indicator to be ready and fully initialized.
        /// </summary>
        public int WarmUpPeriod => 1;

        /// <summary>
        /// 	 Computes the next value of this indicator from the given state
        /// </summary>
        /// <param name="time"></param>
        /// <param name="input">The input given to the indicator</param>
        /// <returns>A new value for this indicator</returns>
        protected override DoubleArray Forward(long time, DoubleArray input) {
            var range = input[HighIdx] - input[LowIdx];
            return Current[CloseIdx] + (range > 0 ? ((input[CloseIdx] - input[LowIdx]) - (input[HighIdx] - input[CloseIdx])) / range * input[VolumeIdx] : Constants.Zero);
        }
    }
}