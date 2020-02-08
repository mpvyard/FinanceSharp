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
using System.Diagnostics.Contracts;

namespace FinanceSharp {
    public delegate double BinaryFunctionHandler(double lhs, double rhs);

    public delegate double UnaryFunctionHandler(double value);

    public delegate void ForFunctionHandler(double value);

    public delegate void ReferenceForFunctionHandler(ref double value);

    public abstract unsafe partial class DoubleArray {
        /// <summary>
        ///     Performs a binary function on lhs and rhs.
        /// </summary>
        /// <param name="rhs">The rhs of the equation, 'this' is lhs.</param>
        /// <param name="function">The function to call for every value in this array.</param>
        /// <returns></returns>
        public virtual DoubleArray Function(DoubleArray rhs, BinaryFunctionHandler function) {
            var lhs = this;
            if (lhs.IsScalar && lhs.Properties == 1 && rhs.IsScalar && lhs.Properties == 1)
                return function(lhs.Value, rhs.Value);

            int offset = 0;
            if (lhs.IsScalar && lhs.Properties == 1) {
                var ret = rhs.Clone();
                var len = ret.Count;
                var propsRhs = rhs.Properties;
                var lhs_val = lhs.Value;
                fixed (double* rhs_ptr = rhs, ret_ptr = ret)
                    for (int i = 0; i < len; i++, offset += propsRhs)
                        ret_ptr[offset] = function(lhs_val, rhs_ptr[offset]);

                return ret;
            } else if (rhs.IsScalar && rhs.Properties == 1) {
                var ret = lhs.Clone();
                var len = ret.Count;
                var propsLhs = lhs.Properties;
                var rhsVal = rhs.Value;
                fixed (double* lhs_ptr = lhs, ret_ptr = ret)
                    for (int i = 0; i < len; i++, offset += propsLhs)
                        ret_ptr[offset] = function(lhs_ptr[offset], rhsVal);

                return ret;
            } else {
                var ret = lhs.Clone();
                var len = ret.Count;
                var propsLhs = lhs.Properties;
                var propsRhs = rhs.Properties;
                var rhsOffset = 0;
                fixed (double* lhs_ptr = rhs, rhs_ptr = rhs, ret_ptr = ret)
                    for (int i = 0; i < len; i++, offset += propsLhs, rhsOffset += propsRhs)
                        ret_ptr[offset] = function(lhs_ptr[offset], rhs_ptr[rhsOffset]);


                return ret;
            }
        }

        /// <summary>
        ///     Performs a binary function on lhs and rhs on a specific property (axis).
        /// </summary>
        /// <param name="rhs"></param>
        /// <param name="function">The function to call for every value in this array.</param>
        /// <returns></returns>
        public virtual DoubleArray Function(DoubleArray rhs, int property, BinaryFunctionHandler function) {
            var lhs = this;
            if (lhs.IsScalar && lhs.Properties > property && rhs.IsScalar && rhs.Properties > property)
                return lhs[property] * rhs[property];

            int offset = property;
            if (lhs.IsScalar && lhs.Properties > property) {
                var ret = rhs.Clone();
                var len = ret.Count;
                var propsRhs = rhs.Properties;
                var lhs_val = lhs[property];
                fixed (double* rhs_ptr = rhs, ret_ptr = ret)
                    for (int i = 0; i < len; i++, offset += propsRhs)
                        ret_ptr[offset] = function(lhs_val, rhs_ptr[offset]);

                return ret;
            } else if (rhs.IsScalar && rhs.Properties > property) {
                var ret = lhs.Clone();
                var len = ret.Count;
                var propsLhs = lhs.Properties;
                var rhsVal = rhs[property];
                fixed (double* lhs_ptr = lhs, ret_ptr = ret)
                    for (int i = 0; i < len; i++, offset += propsLhs)
                        ret_ptr[offset] = function(lhs_ptr[offset], rhsVal);

                return ret;
            } else {
                var ret = lhs.Clone();
                var len = ret.Count;
                var propsLhs = lhs.Properties;
                var propsRhs = rhs.Properties;
                var rhsOffset = property;
                fixed (double* lhs_ptr = rhs, rhs_ptr = rhs, ret_ptr = ret)
                    for (int i = 0; i < len; i++, offset += propsLhs, rhsOffset += propsRhs)
                        ret_ptr[offset] = function(lhs_ptr[offset], rhs_ptr[rhsOffset]);


                return ret;
            }
        }

        public virtual DoubleArray Function(UnaryFunctionHandler function, bool copy = true) {
            var @this = (copy ? this.Clone() : this);
            fixed (double* src = @this) {
                var len = @this.Count;
                var props = @this.Properties;
                int offset = 0;
                for (int i = 0; i < len; i++, offset += props) {
                    src[offset] = function(src[offset]);
                }
            }

            return @this;
        }

        public virtual DoubleArray Function(int property, UnaryFunctionHandler function, bool copy = true) {
            var @this = (copy ? this.Clone() : this);

            fixed (double* src = @this) {
                var len = @this.Count;
                var props = @this.Properties;
                int offset = property;
                for (int i = 0; i < len; i++, offset += props) {
                    src[offset] = function(src[offset]);
                }
            }

            return @this;
        }

        /// <summary>
        ///     Iterates this array efficiently.
        /// </summary>
        /// <param name="function">The function to call for every value in this array.</param>
        public virtual void ForEach(ForFunctionHandler function) {
            fixed (double* src = this) {
                var cnt = LinearLength;
                for (int i = 0; i < cnt; i++) {
                    function(src[i]);
                }
            }
        }

        public virtual void ForEach(int property, ForFunctionHandler function) {
            if (property >= Properties)
                throw new ArgumentOutOfRangeException(nameof(property));
            if (Properties == 1) {
                ForEach(function);
            } else {
                fixed (double* src = this) {
                    var cnt = Count;
                    var prps = Properties;
                    for (int i = property; i < cnt; i += prps) {
                        function(src[i]);
                    }
                }
            }
        }

        public abstract void ForEach(ReferenceForFunctionHandler function);

        [Pure]
        public virtual double Sum(int property) {
            double sum = 0;
            ForEach(property, value => sum += value);
            return sum;
        }

        [Pure]
        public virtual double Sum() {
            double sum = 0;
            ForEach(value => sum += value);
            return sum;
        }

        [Pure]
        public virtual double Mean(int property) {
            return Sum(property) / Count;
        }

        [Pure]
        public virtual double Mean() {
            return Sum() / Count;
        }

        [Pure]
        public virtual double Median(int property) {
            return this[((Count + 1) / 2) * Properties + property];
        }

        [Pure]
        public virtual double Median() {
            return this[(Count + 1) / 2];
        }
    }
}