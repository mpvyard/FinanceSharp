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
using System.Diagnostics;

namespace FinanceSharp {
    public abstract unsafe partial class DoubleArray {
        /// <summary>
        ///     Wraps given <paramref name="value"/> in a scalar DoubleArray.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <returns>A new DoubleArray holding <paramref name="value"/>.</returns>
        [DebuggerStepThrough]
        public static DoubleArray From(double value) {
            return new DoubleArrayScalar(value);
        }

        /// <summary>
        ///     Copies or wraps given <paramref name="array"/> into a <see cref="DoubleArray"/>.
        /// </summary>
        /// <param name="array">An array</param>
        /// <param name="copy">If true, <paramref name="array"/>'s contents will be copied to a newly allocated memory block, otherwise pinned and referenced. </param>
        /// <param name="properties">How many properties are for every item in the array.</param>
        [DebuggerStepThrough]
        public static DoubleArray From(double[] array, bool copy, int properties = 1) {
            return new DoubleArrayManaged(copy ? (double[]) array.Clone() : array, properties);
        }

        /// <summary>
        ///     Copies or wraps given <paramref name="array"/> into a <see cref="DoubleArray"/>.
        /// </summary>
        /// <param name="array">An array</param>
        /// <param name="copy">If true, <paramref name="array"/>'s contents will be copied via <see cref="DoubleArray.Clone"/>, otherwise pinned and referenced. </param>
        [DebuggerStepThrough]
        public static DoubleArray From(double[,] array, bool copy) {
            return new DoubleArray2DManaged(copy ? (double[,]) array.Clone() : array);
        }

        /// <summary>
        ///     Converts <paramref name="array"/> to double[,] (always copies) and wraps in DoubleArray.
        /// </summary>
        /// <param name="array">The array to convert and wrap.</param>
        [DebuggerStepThrough]
        public static DoubleArray From(double[][] array) {
            return new DoubleArray2DManaged(array);
        }

        /// <summary>
        ///     Copies or wraps given <paramref name="array"/> into a <see cref="DoubleArray"/>.
        /// </summary>
        /// <param name="array">An array</param>
        /// <param name="copy">If true, <paramref name="array"/>'s contents will be copied to a newly allocated memory block, otherwise pinned and referenced. </param>
        [DebuggerStepThrough]
        public static DoubleArray From<TStruct>(TStruct[] array, bool copy) where TStruct : unmanaged, DataStruct {
            return copy ? new DoubleArrayStruct<TStruct>((TStruct[]) array.Clone()) : new DoubleArrayStruct<TStruct>(array);
        }

        /// <summary>
        ///     Wraps given <paramref name="array"/> into a <see cref="DoubleArray"/>.
        /// </summary>
        /// <typeparam name="TStruct">An unmanaged structure inherieting DataStruct.</typeparam>
        /// <param name="values">An array of values.</param>
        [DebuggerStepThrough]
        public static DoubleArray From<TStruct>(params TStruct[] values) where TStruct : unmanaged, DataStruct {
            return new DoubleArrayStruct<TStruct>(values);
        }

        /// <summary>
        ///     Wraps a copy of given <typeparamref name="TStruct"/>.
        /// </summary>
        /// <typeparam name="TStruct">An unmanaged structure inherieting DataStruct.</typeparam>
        /// <param name="scalar">A value to wrap.</param>
        [DebuggerStepThrough]
        public static DoubleArray From<TStruct>(TStruct scalar) where TStruct : unmanaged, DataStruct {
            return new DoubleArrayStructScalar<TStruct>(ref scalar);
        }

        /// <summary>
        ///     Wraps a copy of given <typeparamref name="TStruct"/>.
        /// </summary>
        /// <typeparam name="TStruct">An unmanaged structure inherieting DataStruct.</typeparam>
        /// <param name="scalar">A value to wrap.</param>
        [DebuggerStepThrough]
        public static DoubleArray From<TStruct>(ref TStruct scalar) where TStruct : unmanaged, DataStruct {
            return new DoubleArrayStructScalar<TStruct>(ref scalar);
        }

        /// <summary>
        ///     Wraps given <paramref name="pointer"/> with <see cref="DoubleArrayUnmanaged"/>
        /// </summary>
        /// <param name="pointer">Address of the memory block storing doubles.</param>
        /// <param name="count">Number of items in given array</param>
        /// <param name="properties">Number of properties for every item in given array.</param>
        /// <param name="zeroValues">Fill the memory block with zeros</param>
        /// <param name="disposer">Deallocator called when this object is disposed. Can be null.</param>
        [DebuggerStepThrough]
        public static DoubleArray From(void* pointer, int count, int properties, bool zeroValues = true, Action disposer = null) {
            return new DoubleArrayUnmanaged((double*) pointer, count, properties, zeroValues, disposer);
        }

        /// <summary>
        ///     Wraps given <paramref name="pointer"/> with <see cref="DoubleArrayUnmanaged"/>
        /// </summary>
        /// <param name="pointer">Address of the memory block storing doubles.</param>
        /// <param name="count">Number of items in given array</param>
        /// <param name="properties">Number of properties for every item in given array.</param>
        /// <param name="zeroValues">Fill the memory block with zeros</param>
        /// <param name="disposer">Deallocator called when this object is disposed. Can be null.</param>
        [DebuggerStepThrough]
        public static DoubleArray From(double* pointer, int count, int properties, bool zeroValues = true, Action disposer = null) {
            return new DoubleArrayUnmanaged(pointer, count, properties, zeroValues, disposer);
        }
    }
}