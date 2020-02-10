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
using System.Collections.Generic;
using System.Globalization;
using FinanceSharp.Delegates;
using MathNet.Numerics.Providers.LinearAlgebra.OpenBlas;
using static FinanceSharp.Constants;


namespace FinanceSharp.Indicators {
    /// <summary>
    /// 	 Provides extension methods for Indicator
    /// </summary>
    public static class IndicatorExtensions {
        /// <summary>
        /// 	 Configures the second indicator to receive automatic updates from the first by attaching an event handler
        /// 	 to first.Updated
        /// </summary>
        /// <param name="second">The indicator that receives data from the first</param>
        /// <param name="first">The indicator that sends data via <see cref="IUpdatable.Updated"/> even to the second</param>
        /// <param name="waitForFirstToReady">True to only send updates to the second if first.IsReady returns true, false to alway send updates to second</param>
        /// <returns>The reference to the second indicator to allow for method chaining</returns>
        public static T Of<T>(this T second, IUpdatable first, bool waitForFirstToReady = true)
            where T : IUpdatable {
            if (waitForFirstToReady) {
                first.Updated += (time, consolidated) => {
                    // only send the data along if we're ready
                    if (first.IsReady) {
                        second.Update(time, consolidated);
                    }
                };
            } else {
                first.Updated += (time, consolidated)
                    => second.Update(time, consolidated);
            }

            first.Resetted += sender => second.Reset();

            return second;
        }

        /// <summary>
        /// 	 Configures the second indicator to receive automatic updates from the first by attaching an event handler
        /// 	 to first.Updated
        /// </summary>
        /// <param name="second">The indicator that receives data from the first</param>
        /// <param name="first">The indicator that sends data via <see cref="IUpdatable.Updated"/> even to the second</param>
        /// <param name="waitForFirstToReady">True to only send updates to the second if first.IsReady returns true, false to alway send updates to second</param>
        /// <returns>The reference to the second indicator to allow for method chaining</returns>
        public static T Then<T>(this IUpdatable first, T second, bool waitForFirstToReady = true)
            where T : IUpdatable {
            if (waitForFirstToReady) {
                first.Updated += (time, consolidated) => {
                    // only send the data along if we're ready
                    if (first.IsReady) {
                        second.Update(time, consolidated);
                    }
                };
            } else {
                first.Updated += (time, consolidated)
                    => second.Update(time, consolidated);
            }

            first.Resetted += sender => second.Reset();

            return second;
        }

        private static string ResolveName(IUpdatable first, string argumentedName) {
            if (!(string.IsNullOrEmpty(argumentedName)))
                return argumentedName;
            if (first is IIndicator ind) {
                argumentedName = ($"function({ind.Name})");
            } else {
                argumentedName = ($"function({first.GetType().Name})");
            }

            return argumentedName;
        }

        /// <summary>
        ///     Performs a math <paramref name="op"/> over <see cref="DoubleArray.Value"/> or value from <paramref name="selector"/>. The result is stored in a <see cref="Identity"/>.
        /// </summary>
        /// <param name="first">The indicator that sends data via <see cref="IUpdatable.Updated"/> even to the math <paramref name="op"/></param>
        /// <param name="op">The operation to perform on <see cref="DoubleArray.Value"/>.</param>
        /// <param name="selector">A selector to choose what <see cref="double"/> to pass to math <paramref name="op"/>. By default <see cref="DoubleArray.Value"/> is used.</param>
        /// <param name="waitForFirstToReady">First must be ready in order to push the updates forward.</param>
        /// <param name="name">Name of the new returned <see cref="Identity"/> representing the <paramref name="op"/>.</param>
        public static Identity Function(this IUpdatable first, UnaryFunctionHandler op, SelectorFunctionHandler selector = null, bool waitForFirstToReady = true, string name = null) {
            if (op == null) throw new ArgumentNullException(nameof(op));

            var idn = new Identity(ResolveName(first, name));
            if (selector == null) {
                if (waitForFirstToReady) {
                    first.Updated += (time, updated) => {
                        if (first.IsReady)
                            idn.Update(time, op(updated.Value));
                    };
                } else
                    first.Updated += (time, updated) => idn.Update(time, op(updated.Value));
            } else {
                if (waitForFirstToReady) {
                    first.Updated += (time, updated) => {
                        if (first.IsReady)
                            idn.Update(time, new DoubleArrayScalar(op(selector(updated))));
                    };
                } else
                    first.Updated += (time, updated) => idn.Update(time, new DoubleArrayScalar(op(selector(updated))));
            }

            first.Resetted += sender => idn.Reset();

            return idn;
        }

        /// <summary>
        ///     Performs a math <paramref name="op"/> over <see cref="DoubleArray"/> or the value from <paramref name="selector"/>. The result is stored in a <see cref="Identity"/>.
        /// </summary>
        /// <param name="first">The indicator that sends data via <see cref="IUpdatable.Updated"/> even to the math <paramref name="op"/></param>
        /// <param name="op">The operation to perform on the <see cref="DoubleArray"/> passed from <paramref name="first"/>.</param>
        /// <param name="selector">A selector to choose what <see cref="DoubleArray"/> to pass to math <paramref name="op"/>. By default, the unchanged <see cref="DoubleArray"/> is used.</param>
        /// <param name="waitForFirstToReady">First must be ready in order to push the updates forward.</param>
        /// <param name="name">Name of the new returned <see cref="Identity"/> representing the <paramref name="op"/>.</param>
        public static Identity Function(this IUpdatable first, UnaryArrayFunctionHandler op, ArraySelectorFunctionHandler selector = null, bool waitForFirstToReady = true, string name = null) {
            if (op == null) throw new ArgumentNullException(nameof(op));

            var idn = new Identity(ResolveName(first, name));
            if (selector == null) {
                if (waitForFirstToReady) {
                    first.Updated += (time, updated) => {
                        if (first.IsReady)
                            idn.Update(time, op(updated));
                    };
                } else
                    first.Updated += (time, updated) => idn.Update(time, op(updated));
            } else {
                if (waitForFirstToReady) {
                    first.Updated += (time, updated) => {
                        if (first.IsReady)
                            idn.Update(time, op(selector(updated)));
                    };
                } else
                    first.Updated += (time, updated) => idn.Update(time, op(selector(updated)));
            }

            first.Resetted += sender => idn.Reset();

            return idn;
        }

        /// <summary>
        /// 	 Will collect updates from <paramref name="first"/> into the returned <see cref="List{T}"/>.
        /// </summary>
        /// <param name="first">The indicator that sends data via <see cref="IUpdatable.Updated"/> even to the second</param>
        /// <param name="waitForFirstToReady">True to only send updates to the second if first.IsReady returns true, false to alway send updates to second</param>
        /// <param name="resetListOnIndicatorReset">Should the returned <see cref="List{T}"/> be cleared when <paramref name="first"/> is Resetted.</param>
        public static List<DoubleArray> ThenToList(this IUpdatable first, bool waitForFirstToReady = true, bool resetListOnIndicatorReset = false) {
            var ret = new List<DoubleArray>();
            if (waitForFirstToReady) {
                first.Updated += (time, consolidated) => {
                    // only send the data along if we're ready
                    if (first.IsReady) {
                        ret.Add(consolidated);
                    }
                };
            } else {
                first.Updated += (time, consolidated)
                    => ret.Add(consolidated);
            }

            if (resetListOnIndicatorReset)
                first.Resetted += sender => ret.Clear();

            return ret;
        }


        /// <summary>
        /// 	 Creates a new CompositeIndicator such that the result will be average of a first indicator weighted by a second one
        /// </summary>
        /// <param name="value">Indicator that will be averaged</param>
        /// <param name="weight">Indicator that provides the average weights</param>
        /// <param name="period">Average period</param>
        /// <returns>Indicator that results of the average of first by weights given by second</returns>
        public static CompositeIndicator WeightedBy<TWeight>(this IndicatorBase value, TWeight weight, int period)
            where TWeight : IndicatorBase {
            var x = new WindowIdentity(period);
            var y = new WindowIdentity(period);
            var numerator = new Sum("Sum_xy", period);
            var denominator = new Sum("Sum_y", period);

            value.Updated += (time, consolidated) => {
                x.Update(time, consolidated);
                if (x.Samples == y.Samples) {
                    numerator.Update(time, consolidated * y.Current[0, C]);
                }
            };

            weight.Updated += (time, consolidated) => {
                y.Update((long) time, (DoubleArray) consolidated);
                if (x.Samples == y.Samples) {
                    numerator.Update(time, consolidated * x.Current[0, C]);
                }

                denominator.Update((long) time, (DoubleArray) consolidated);
            };

            value.Resetted += sender => weight.Reset();

            return numerator.Over(denominator);
        }

        /// <summary>
        /// 	 Creates a new CompositeIndicator such that the result will be the sum of the left and the constant
        /// </summary>
        /// <remarks>
        /// 	 value = left + constant
        /// </remarks>
        /// <param name="left">The left indicator</param>
        /// <param name="constant">The addend</param>
        /// <returns>The sum of the left and right indicators</returns>
        public static CompositeIndicator Plus(this IndicatorBase left, double constant) {
            var constantIndicator = new ConstantIndicator(constant.ToString(CultureInfo.InvariantCulture), constant);
            return left.Plus(constantIndicator);
        }

        /// <summary>
        /// 	 Creates a new CompositeIndicator such that the result will be the sum of the left and right
        /// </summary>
        /// <remarks>
        /// 	 value = left + right
        /// </remarks>
        /// <param name="left">The left indicator</param>
        /// <param name="right">The right indicator</param>
        /// <returns>The sum of the left and right indicators</returns>
        public static CompositeIndicator Plus(this IndicatorBase left, IndicatorBase right) {
            return new CompositeIndicator(left, right, (l, r) => l + r);
        }

        /// <summary>
        /// 	 Creates a new CompositeIndicator such that the result will be the sum of the left and right
        /// </summary>
        /// <remarks>
        /// 	 value = left + right
        /// </remarks>
        /// <param name="left">The left indicator</param>
        /// <param name="right">The right indicator</param>
        /// <param name="name">The name of this indicator</param>
        /// <returns>The sum of the left and right indicators</returns>
        public static CompositeIndicator Plus(this IndicatorBase left, IndicatorBase right, string name) {
            return new CompositeIndicator(name, left, right, (l, r) => (DoubleArray) l + r);
        }

        /// <summary>
        /// 	 Creates a new CompositeIndicator such that the result will be the difference of the left and constant
        /// </summary>
        /// <remarks>
        /// 	 value = left - constant
        /// </remarks>
        /// <param name="left">The left indicator</param>
        /// <param name="constant">The subtrahend</param>
        /// <returns>The difference of the left and right indicators</returns>
        public static CompositeIndicator Minus(this IndicatorBase left, double constant) {
            var constantIndicator = new ConstantIndicator(constant.ToString(CultureInfo.InvariantCulture), constant);
            return left.Minus(constantIndicator);
        }

        /// <summary>
        /// 	 Creates a new CompositeIndicator such that the result will be the difference of the left and right
        /// </summary>
        /// <remarks>
        /// 	 value = left - right
        /// </remarks>
        /// <param name="left">The left indicator</param>
        /// <param name="right">The right indicator</param>
        /// <returns>The difference of the left and right indicators</returns>
        public static CompositeIndicator Minus(this IndicatorBase left, IndicatorBase right) {
            return new CompositeIndicator(left, right, (l, r) => (DoubleArray) l - r);
        }

        /// <summary>
        /// 	 Creates a new CompositeIndicator such that the result will be the difference of the left and right
        /// </summary>
        /// <remarks>
        /// 	 value = left - right
        /// </remarks>
        /// <param name="left">The left indicator</param>
        /// <param name="right">The right indicator</param>
        /// <param name="name">The name of this indicator</param>
        /// <returns>The difference of the left and right indicators</returns>
        public static CompositeIndicator Minus(this IndicatorBase left, IndicatorBase right, string name) {
            return new CompositeIndicator(name, left, right, (l, r) => (DoubleArray) l - r);
        }

        /// <summary>
        /// 	 Creates a new CompositeIndicator such that the result will be the ratio of the left to the constant
        /// </summary>
        /// <remarks>
        /// 	 value = left/constant
        /// </remarks>
        /// <param name="left">The left indicator</param>
        /// <param name="constant">The constant value denominator</param>
        /// <returns>The ratio of the left to the right indicator</returns>
        public static CompositeIndicator Over(this IndicatorBase left, double constant) {
            var constantIndicator = new ConstantIndicator(constant.ToString(CultureInfo.InvariantCulture), constant);
            return left.Over(constantIndicator);
        }

        /// <summary>
        /// 	 Creates a new CompositeIndicator such that the result will be the ratio of the left to the right
        /// </summary>
        /// <remarks>
        /// 	 value = left/right
        /// </remarks>
        /// <param name="left">The left indicator</param>
        /// <param name="right">The right indicator</param>
        /// <returns>The ratio of the left to the right indicator</returns>
        public static CompositeIndicator Over(this IndicatorBase left, IndicatorBase right) {
            return new CompositeIndicator(left, right, (l, r) => r == Constants.Zero ? new IndicatorResult(Constants.Zero, IndicatorStatus.MathError) : new IndicatorResult((DoubleArray) ((DoubleArray) l / r)));
        }

        /// <summary>
        /// 	 Creates a new CompositeIndicator such that the result will be the ratio of the left to the right
        /// </summary>
        /// <remarks>
        /// 	 value = left/right
        /// </remarks>
        /// <param name="left">The left indicator</param>
        /// <param name="right">The right indicator</param>
        /// <param name="name">The name of this indicator</param>
        /// <returns>The ratio of the left to the right indicator</returns>
        public static CompositeIndicator Over(this IndicatorBase left, IndicatorBase right, string name) {
            return new CompositeIndicator(name, left, right, (l, r) => r == Constants.Zero ? new IndicatorResult(Constants.Zero, IndicatorStatus.MathError) : new IndicatorResult((DoubleArray) ((DoubleArray) l / r)));
        }

        /// <summary>
        /// 	 Creates a new CompositeIndicator such that the result will be the product of the left and the constant
        /// </summary>
        /// <remarks>
        /// 	 value = left*constant
        /// </remarks>
        /// <param name="left">The left indicator</param>
        /// <param name="constant">The constant value to multiple by</param>
        /// <returns>The product of the left to the right indicators</returns>
        public static CompositeIndicator Times(this IndicatorBase left, double constant) {
            var constantIndicator = new ConstantIndicator(constant.ToString(CultureInfo.InvariantCulture), constant);
            return left.Times(constantIndicator);
        }

        /// <summary>
        /// 	 Creates a new CompositeIndicator such that the result will be the product of the left to the right
        /// </summary>
        /// <remarks>
        /// 	 value = left*right
        /// </remarks>
        /// <param name="left">The left indicator</param>
        /// <param name="right">The right indicator</param>
        /// <returns>The product of the left to the right indicators</returns>
        public static CompositeIndicator Times(this IndicatorBase left, IndicatorBase right) {
            return new CompositeIndicator(left, right, (l, r) => (DoubleArray) l * r);
        }

        /// <summary>
        /// 	 Creates a new CompositeIndicator such that the result will be the product of the left to the right
        /// </summary>
        /// <remarks>
        /// 	 value = left*right
        /// </remarks>
        /// <param name="left">The left indicator</param>
        /// <param name="right">The right indicator</param>
        /// <param name="name">The name of this indicator</param>
        /// <returns>The product of the left to the right indicators</returns>
        public static CompositeIndicator Times(this IndicatorBase left, IndicatorBase right, string name) {
            return new CompositeIndicator(name, left, right, (l, r) => (IndicatorResult) ((DoubleArray) l * r));
        }

        /// <summary>Creates a new ExponentialMovingAverage indicator with the specified period and smoothingFactor from the left indicator
        /// </summary>
        /// <param name="left">The ExponentialMovingAverage indicator will be created using the data from left</param>
        /// <param name="period">The period of the ExponentialMovingAverage indicators</param>
        /// <param name="smoothingFactor">The percentage of data from the previous value to be carried into the next value</param>
        /// <param name="waitForFirstToReady">True to only send updates to the second if left.IsReady returns true, false to alway send updates</param>
        /// <returns>A reference to the ExponentialMovingAverage indicator to allow for method chaining</returns>
        public static ExponentialMovingAverage EMA(this IndicatorBase left, int period, double? smoothingFactor = null, bool waitForFirstToReady = true) {
            double k = smoothingFactor.HasValue ? k = smoothingFactor.Value : ExponentialMovingAverage.SmoothingFactorDefault(period);
            return new ExponentialMovingAverage($"EMA{period}_Of_{left.Name}", period, k).Of(left, waitForFirstToReady);
        }

        /// <summary>Creates a new Maximum indicator with the specified period from the left indicator
        /// </summary>
        /// <param name="left">The Maximum indicator will be created using the data from left</param>
        /// <param name="period">The period of the Maximum indicator</param>
        /// <param name="waitForFirstToReady">True to only send updates to the second if left.IsReady returns true, false to alway send updates</param>
        /// <returns>A reference to the Maximum indicator to allow for method chaining</returns>
        public static Maximum MAX(this IIndicator left, int period, bool waitForFirstToReady = true) {
            return new Maximum($"MAX{period}_Of_{left.Name}", period).Of(left, waitForFirstToReady);
        }

        /// <summary>Creates a new Minimum indicator with the specified period from the left indicator
        /// </summary>
        /// <param name="left">The Minimum indicator will be created using the data from left</param>
        /// <param name="period">The period of the Minimum indicator</param>
        /// <param name="waitForFirstToReady">True to only send updates to the second if left.IsReady returns true, false to alway send updates</param>
        /// <returns>A reference to the Minimum indicator to allow for method chaining</returns>
        public static Minimum MIN(this IndicatorBase left, int period, bool waitForFirstToReady = true) {
            return new Minimum($"MIN{period}_Of_{left.Name}", period).Of(left, waitForFirstToReady);
        }

        /// <summary>Initializes a new instance of the SimpleMovingAverage class with the specified name and period from the left indicator
        /// </summary>
        /// <param name="left">The SimpleMovingAverage indicator will be created using the data from left</param>
        /// <param name="period">The period of the SMA</param>
        /// <param name="waitForFirstToReady">True to only send updates to the second if first.IsReady returns true, false to alway send updates to second</param>
        /// <returns>The reference to the SimpleMovingAverage indicator to allow for method chaining</returns>
        public static SimpleMovingAverage SMA(this IndicatorBase left, int period, bool waitForFirstToReady = true) {
            return new SimpleMovingAverage($"SMA{period}_Of_{left.Name}", period).Of(left, waitForFirstToReady);
        }
    }
}