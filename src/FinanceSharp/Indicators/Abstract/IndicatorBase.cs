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

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace FinanceSharp.Indicators {
    /// <summary>
    /// 	 Provides a base type for all indicators
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToDetailedString) + "()}")]
    public abstract partial class IndicatorBase : IIndicator {
        /// <summary>
        /// 	 Event handler that fires after this indicator is updated
        /// </summary>
        public event UpdatedHandler Updated;

        /// <summary>
        ///     Event handler that fires after this updatable is reset.
        /// </summary>
        public event ResettedHandler Resetted;

        /// <summary>
        /// 	 Initializes a new instance of the Indicator class using the specified name.
        /// </summary>
        /// <param name="name">The name of this indicator</param>
        protected IndicatorBase(string name) {
            Name = name;
            Current = Constants.Zero;
        }

        /// <summary>
        /// 	 Gets a name for this indicator
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 	 Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public abstract bool IsReady { get; }

        /// <summary>
        /// 	 Gets the current state of this indicator. If the state has not been updated
        /// 	 then the time on the value will equal DateTime.MinValue.
        /// </summary>
        public virtual DoubleArray Current { get; protected set; }

        /// <summary>
        ///     The time of the currently stored data in milliseconds-epoch.
        /// </summary>
        public virtual long CurrentTime { get; protected set; }

        /// <summary>
        ///     The number of items <see cref="IUpdatable.Current"/> will have.
        /// </summary>
        public virtual int OutputCount { get; protected set; } = 1;

        /// <summary>
        ///     The number of properties <see cref="IUpdatable.Current"/> will have.
        /// </summary>
        public virtual int Properties => IndicatorValue.Properties;

        /// <summary>
        ///     The number of properties of input argument of <see cref="IUpdatable.Update"/> must have.
        /// </summary>
        public virtual int InputProperties => Properties;

        /// <summary>
        /// 	 Gets the number of samples processed by this indicator
        /// </summary>
        public long Samples { get; protected set; }

        /// <summary>
        /// 	 Required period, in data points (number of updates), for the indicator to be ready and fully initialized.
        /// </summary>
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public virtual int WarmUpPeriod { get; }

        /// <summary>
        /// 	 Updates this consolidator with the specified data
        /// </summary>
        /// <param name="data">The new data for the consolidator</param>
        public void Update<TStruct>(long time, TStruct data) where TStruct : unmanaged, DataStruct {
            Update(time, DoubleArray.From(data));
        }

        /// <summary>
        /// 	 Updates this consolidator with the specified data
        /// </summary>
        /// <param name="data">The new data for the consolidator</param>
        internal void Update<TStruct>(DateTime time, TStruct data) where TStruct : unmanaged, DataStruct {
            Update(time.ToEpochTime(), DoubleArray.From(data));
        }

        /// <summary>
        /// 	 Updates this consolidator with the specified data
        /// </summary>
        /// <param name="data">The new data for the consolidator</param>
        internal void Update<TStruct>((DateTime Time, TStruct Value) tuple) where TStruct : unmanaged, DataStruct {
            Update(tuple.Time.ToEpochTime(), DoubleArray.From(tuple.Value));
        }

        /// <summary>
        /// 	 Updates this consolidator with the specified data
        /// </summary>
        /// <param name="data">The new data for the consolidator</param>
        internal void Update<TStruct>((long Time, TStruct Value) tuple) where TStruct : unmanaged, DataStruct {
            Update(tuple.Time, new DoubleArrayStructScalar<TStruct>(in tuple.Value));
        }

        /// <summary>
        /// 	 Updates the state of this indicator with the given value and returns true
        /// 	 if this indicator is ready, false otherwise
        /// </summary>
        /// <param name="time">The time associated with the value</param>
        /// <param name="value">The value to use to update this indicator</param>
        /// <returns>True if this indicator is ready, false otherwise</returns>
        public void Update(long time, double value) {
            Update(time, new DoubleArrayScalar(value));
        }

        /// <summary>
        /// 	 Updates the state of this indicator with the given value and returns true
        /// 	 if this indicator is ready, false otherwise
        /// </summary>
        /// <param name="time">The time associated with the value</param>
        /// <param name="value">The value to use to update this indicator</param>
        /// <returns>True if this indicator is ready, false otherwise</returns>
        internal void Update(DateTime time, double value) {
            Update(time.ToEpochTime(), new DoubleArrayScalar(value));
        }

        /// <summary>
        /// 	 Updates the state of this indicator with the given value and returns true
        /// 	 if this indicator is ready, false otherwise
        /// </summary>
        /// <param name="time"></param>
        /// <param name="input">The value to use to update this indicator</param>
        /// <returns>True if this indicator is ready, false otherwise</returns>
        public virtual void Update(long time, DoubleArray input) {
            // compute a new value and update our previous time
            Samples++;

            Current = Forward(time, input);
            CurrentTime = time;

            // let others know we've produced a new data point
            OnUpdated(time, Current);
        }

        /// <summary>
        /// 	 Resets this indicator to its initial state
        /// </summary>
        public virtual void Reset() {
            Samples = 0;
            Current = Constants.Zero;
            CurrentTime = 0;
            Resetted?.Invoke(this);
        }

        /// <summary>
        /// 	 Triggers event <see cref="Resetted"/>.
        /// </summary>
        protected void OnReset() {
            Resetted?.Invoke(this);
        }


        /// <summary>
        /// 	 Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        /// 	 true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj) {
            switch (obj) {
                case IndicatorBase ind:
                    return Equals(Current, ind.Current);
                case ValueType v:
                    return Equals(Current, obj);
                case null:
                default:
                    return false;
            }

            // this implementation acts as a liason to prevent inconsistency between the operators
            // == and != against primitive types. the core impl for equals between two indicators
            // is still reference equality, however, when comparing value types (floats/int, ect..)
            // we'll use value type semantics on Current.Value
            // because of this, we shouldn't need to override GetHashCode as well since we're still
            // solely relying on reference semantics (think hashset/dictionary impls)
            //TODO!: wtf is going on here
            if (ReferenceEquals(obj, null)) return false;
            var type = obj.GetType();

            while (type != null && type != typeof(object)) {
                if (ReferenceEquals(this, obj))
                    return true;

                type = type.BaseType;
            }

            try {
                // the obj is not an indicator, so let's check for value types, try converting to double
                var converted = obj.ConvertInvariant<double>();
                return Current == converted;
            } catch (InvalidCastException) {
                // conversion failed, return false
                return false;
            }
        }

        /// <summary>
        /// 	 ToString Overload for Indicator Base
        /// </summary>
        /// <returns>String representation of the indicator</returns>
        public override string ToString() {
            return Current.ToString();
        }

        /// <summary>
        /// 	 Provides a more detailed string of this indicator in the form of {Name} - {Value}
        /// </summary>
        /// <returns>A detailed string of this indicator's current state</returns>
        public string ToDetailedString() {
            return $"{Name} - {this}";
        }

        /// <summary>
        /// 	 Computes the next value of this indicator from the given state
        /// </summary>
        /// <param name="time"></param>
        /// <param name="input">The input given to the indicator</param>
        /// <returns>A new value for this indicator</returns>
        protected abstract DoubleArray Forward(long time, DoubleArray input);

        /// <summary>
        /// 	 Event invocator for the Updated event
        /// </summary>
        /// <param name="consolidated">This is the new piece of data produced by this indicator</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnUpdated(long time, DoubleArray consolidated) {
            Updated?.Invoke(time, consolidated);
        }

        /// <summary>
        ///     Asserts for DEBUG runs..
        /// </summary>
        [Conditional("DEBUG"), DebuggerHidden]
        protected void AssertTrue(bool condition) {
            if (!condition)
                throw new Exception();
        }

        /// <summary>
        ///     Asserts for DEBUG runs..
        /// </summary>
        [Conditional("DEBUG"), DebuggerHidden]
        protected void AssertTrue(bool condition, string reason) {
            if (!condition)
                throw new Exception(reason);
        }
    }
}