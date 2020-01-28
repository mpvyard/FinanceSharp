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
using FinanceSharp.Data;
using static FinanceSharp.Constants;
using FinanceSharp.Data;


namespace FinanceSharp.Indicators {
    /// <summary>
    /// 	 This indicator computes the Heikin-Ashi bar (HA)
    /// 	 The Heikin-Ashi bar is calculated using the following formulas:
    /// 	 HA_Close[0] = (Open[0] + High[0] + Low[0] + Close[0]) / 4
    /// 	 HA_Open[0] = (HA_Open[1] + HA_Close[1]) / 2
    /// 	 HA_High[0] = MAX(High[0], HA_Open[0], HA_Close[0])
    /// 	 HA_Low[0] = MIN(Low[0], HA_Open[0], HA_Close[0])
    /// </summary>
    public class HeikinAshi : BarIndicator {
        /// <summary>
        /// 	 Gets the Heikin-Ashi Open
        /// </summary>
        public IndicatorBase Open { get; }

        /// <summary>
        /// 	 Gets the Heikin-Ashi High
        /// </summary>
        public IndicatorBase High { get; }

        /// <summary>
        /// 	 Gets the Heikin-Ashi Low
        /// </summary>
        public IndicatorBase Low { get; }

        /// <summary>
        /// 	 Gets the Heikin-Ashi Close
        /// </summary>
        public IndicatorBase Close { get; }

        /// <summary>
        /// 	 Gets the Heikin-Ashi Volume
        /// </summary>
        public IndicatorBase Volume { get; }

        /// <summary>
        /// 	 Gets the Heikin-Ashi current TradeBar
        /// </summary>
        public TradeBarValue CurrentBar => new TradeBarValue(Open.Current.Value, High.Current.Value, Low.Current.Value, Close.Current.Value, Volume.Current.Value);

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="HeikinAshi"/> class using the specified name.
        /// </summary> 
        /// <param name="name">The name of this indicator</param>
        public HeikinAshi(string name)
            : base(name) {
            Open = new Identity(name + "_Open");
            High = new Identity(name + "_High");
            Low = new Identity(name + "_Low");
            Close = new Identity(name + "_Close");
            Volume = new Identity(name + "_Volume");
        }

        /// <summary>
        /// 	 Initializes a new instance of the <see cref="HeikinAshi"/> class.
        /// </summary> 
        public HeikinAshi()
            : this("HA") { }

        /// <summary>
        /// 	 Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady => Samples > 1;

        /// <summary>
        /// 	 Required period, in data points, for the indicator to be ready and fully initialized.
        /// </summary>
        public int WarmUpPeriod => 2;

        /// <summary>
        /// 	 Computes the next value of this indicator from the given state
        /// </summary>
        /// <param name="time"></param>
        /// <param name="input">The input given to the indicator</param>
        /// <returns> A new value for this indicator </returns>
        protected override DoubleArray Forward(long time, DoubleArray input) {
            if (!IsReady) {
                Open.Update(time, (input.Open + input.Close) / 2);
                Close.Update(time, (input.Open + input.High + input.Low + input.Close) / 4);
                High.Update(time, input.High);
                Low.Update(time, input.Low);
            } else {
                Open.Update(time, (Open + Close) / 2);
                Close.Update(time, (input.Open + input.High + input.Low + input.Close) / 4);
                High.Update(time, Math.Max(input.High, Math.Max(Open.Current.Value, Close.Current.Value)));
                Low.Update(time, Math.Min(input.Low, Math.Min(Open.Current.Value, Close.Current.Value)));
            }

            return Close.Current.Value;
        }

        /// <summary>
        /// 	 Resets this indicator to its initial state
        /// </summary>
        public override void Reset() {
            Open.Reset();
            High.Reset();
            Low.Reset();
            Close.Reset();
            Volume.Reset();
            base.Reset();
        }
    }
}