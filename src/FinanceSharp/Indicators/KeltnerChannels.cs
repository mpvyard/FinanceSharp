/*
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
    /// 	 This indicator creates a moving average (middle band) with an upper band and lower band
    /// 	 fixed at k average true range multiples away from the middle band.  
    /// </summary>
    public class KeltnerChannels : BarIndicator {
        private readonly double _k;

        /// <summary>
        /// 	 Gets the middle band of the channel
        /// </summary>
        public IndicatorBase MiddleBand { get; }

        /// <summary>
        /// 	 Gets the upper band of the channel
        /// </summary>
        public IndicatorBase UpperBand { get; }

        /// <summary>
        /// 	 Gets the lower band of the channel
        /// </summary>
        public IndicatorBase LowerBand { get; }

        /// <summary>
        /// 	 Gets the average true range
        /// </summary>
        public IndicatorBase AverageTrueRange { get; }


        /// <summary>
        /// 	 Initializes a new instance of the KeltnerChannels class
        /// </summary>
        /// <param name="period">The period of the average true range and moving average (middle band)</param>
        /// <param name="k">The number of multiplies specifying the distance between the middle band and upper or lower bands</param>
        /// <param name="movingAverageType">The type of moving average to be used</param>
        public KeltnerChannels(int period, double k, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : this($"KC({period},{k})", period, k, movingAverageType) { }

        /// <summary>
        /// 	 Initializes a new instance of the KeltnerChannels class
        /// </summary>
        /// <param name="name">The name of this indicator</param>
        /// <param name="period">The period of the average true range and moving average (middle band)</param>
        /// <param name="k">The number of multiples specifying the distance between the middle band and upper or lower bands</param>
        /// <param name="movingAverageType">The type of moving average to be used</param>
        public KeltnerChannels(string name, int period, double k, MovingAverageType movingAverageType = MovingAverageType.Simple)
            : base(name) {
            _k = k;
            WarmUpPeriod = period;

            //Initialise ATR and SMA
            AverageTrueRange = new AverageTrueRange(name + "_AverageTrueRange", period, MovingAverageType.Simple);
            MiddleBand = movingAverageType.AsIndicator(name + "_MiddleBand", period);

            //Compute Lower Band
            LowerBand = new FunctionalIndicator(name + "_LowerBand",
                (time, input) => MiddleBand.IsReady ? MiddleBand - AverageTrueRange * _k : Constants.Zero,
                lowerBand => MiddleBand.IsReady,
                () => MiddleBand.Reset()
            );

            //Compute Upper Band
            UpperBand = new FunctionalIndicator(name + "_UpperBand",
                (time, input) => MiddleBand.IsReady ? MiddleBand + AverageTrueRange * _k : Constants.Zero,
                upperBand => MiddleBand.IsReady,
                () => MiddleBand.Reset()
            );
        }

        /// <summary>
        /// 	 Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady => MiddleBand.IsReady && UpperBand.IsReady && LowerBand.IsReady && AverageTrueRange.IsReady;

        /// <summary>
        /// 	 Required period, in data points, for the indicator to be ready and fully initialized.
        /// </summary>
        public override int WarmUpPeriod { get; }

        /// <summary>
        /// 	 Resets this indicator to its initial state
        /// </summary>
        public override void Reset() {
            AverageTrueRange.Reset();
            MiddleBand.Reset();
            UpperBand.Reset();
            LowerBand.Reset();
            base.Reset();
        }

        /// <summary>
        /// 	 Computes the next value for this indicator from the given state.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="input">The TradeBar to this indicator on this time step</param>
        /// <returns>A new value for this indicator</returns>
        protected override DoubleArray Forward(long time, DoubleArray input) {
            AverageTrueRange.Update(time, input);

            var typicalPrice = (input[HighIdx] + input[LowIdx] + input[CloseIdx]) / 3d;
            MiddleBand.Update(time, typicalPrice);

            // poke the upper/lower bands, they actually don't use the input, they compute
            // based on the ATR and the middle band
            LowerBand.Update(time, input);
            UpperBand.Update(time, input);
            return MiddleBand;
        }
    }
}