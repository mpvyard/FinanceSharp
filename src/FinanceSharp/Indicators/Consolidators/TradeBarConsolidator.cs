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


namespace FinanceSharp.Consolidators {
    /// <summary>
    /// 	 A data consolidator that can make bigger bars from smaller ones over a given
    /// 	 time span or a count of pieces of data.
    ///
    /// 	 Use this consolidator to turn data of a lower resolution into data of a higher resolution,
    /// 	 for example, if you subscribe to minute data but want to have a 15 minute bar.
    /// </summary>
    public class TradeBarConsolidator : TradeBarConsolidatorBase {
        /// <summary>
        ///     The number of properties <see cref="DataConsolidator.Current"/> will have.
        /// </summary>
        public override int Properties => TradeBarValue.Properties;

        /// <summary>
        ///     The number of properties of input argument of <see cref="IUpdatable.Update"/> must have.
        /// </summary>
        public override int InputProperties => Properties;

        /// <summary>
        /// 	 Creates a consolidator to produce a new 'TradeBar' representing the period
        /// </summary>
        /// <param name="period">The minimum span of time before emitting a consolidated bar</param>
        public TradeBarConsolidator(TimeSpan period)
            : base(period) { }

        /// <summary>
        /// 	 Creates a consolidator to produce a new 'TradeBar' representing the last count pieces of data
        /// </summary>
        /// <param name="maxCount">The number of pieces to accept before emitting a consolidated bar</param>
        public TradeBarConsolidator(int maxCount)
            : base(maxCount) { }

        /// <summary>
        /// 	 Creates a consolidator to produce a new 'TradeBar' representing the last count pieces of data or the period, whichever comes first
        /// </summary>
        /// <param name="maxCount">The number of pieces to accept before emitting a consolidated bar</param>
        /// <param name="period">The minimum span of time before emitting a consolidated bar</param>
        public TradeBarConsolidator(int maxCount, TimeSpan period)
            : base(maxCount, period) { }

        /// <summary>
        /// 	 Aggregates the new 'data' into the 'workingBar'. The 'workingBar' will be
        /// 	 null following the event firing
        /// </summary>
        /// <param name="workingBar">The bar we're building, null if the event was just fired and we're starting a new trade bar</param>
        /// <param name="data">The new data</param>
        protected override void AggregateBar(ref long workingTime, ref DoubleArray workingBar, long time, DoubleArray data) {
            if (workingBar == null) {
                workingBar = new DoubleArrayStructScalar<TradeBarValue>(new TradeBarValue(data.Close, data.High, data.Low, data.Open, data.Volume));
                workingTime = GetRoundedBarTime(time);
            } else {
                //Aggregate the working bar
                workingBar.Close = data.Close;
                workingBar.Volume += data.Volume;
                if (data.Low < workingBar.Low) workingBar.Low = data.Low;
                if (data.High > workingBar.High) workingBar.High = data.High;
            }
        }
    }
}