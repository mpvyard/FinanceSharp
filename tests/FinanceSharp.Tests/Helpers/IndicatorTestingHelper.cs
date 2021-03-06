﻿using System;
using System.Linq;

namespace FinanceSharp.Tests.Helpers {
    public static class IndicatorTestingHelper {
        public static void Feed(this IUpdatable updatable, int count, double start, double stepSize, long? timeStart = null, long timeStepMilliseconds = 1000) {
            if (updatable == null) throw new ArgumentNullException(nameof(updatable));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            var time = timeStart ?? new DateTime(2000, 1, 1).ToEpochTime();
            for (int i = 0; i < count; i++, start += stepSize, time += timeStepMilliseconds) {
                updatable.Update(time, start);
            }
        }

        public static void FeedToReady(this IUpdatable updatable, double start = 1d, double stepSize = 0.1d, long? timeStart = null, long timeStepMilliseconds = 1000) {
            if (updatable == null) throw new ArgumentNullException(nameof(updatable));
            if (updatable == null) throw new ArgumentNullException(nameof(updatable));
            var time = timeStart ?? new DateTime(2000, 1, 1).ToEpochTime();
            for (; !updatable.IsReady; start += stepSize, time += timeStepMilliseconds) {
                updatable.Update(time, start);
            }
        }

        public static void FeedToReady(this IUpdatable updatable, IUpdatable[] mustBeReady, double start = 1d, double stepSize = 0.1d, long? timeStart = null, long timeStepMilliseconds = 1000) {
            if (updatable == null) throw new ArgumentNullException(nameof(updatable));
            if (mustBeReady == null) throw new ArgumentNullException(nameof(mustBeReady));
            var time = timeStart ?? new DateTime(2000, 1, 1).ToEpochTime();
            for (; !mustBeReady.All(v => v.IsReady); start += stepSize, time += timeStepMilliseconds) {
                updatable.Update(time, start);
            }
        }


        public static void FeedTradeBar(this IUpdatable updatable, int count, double start, double stepSize, long? timeStart = null, long timeStepMilliseconds = 1000) {
            if (updatable == null) throw new ArgumentNullException(nameof(updatable));
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            var time = timeStart ?? new DateTime(2000, 1, 1).ToEpochTime();
            for (int i = 0; i < count; i++, start += stepSize, time += timeStepMilliseconds) {
                updatable.Update(time, DoubleArray.From(new TradeBarValue(start + 0.03, start + 0.05, start - 0.05, start - 0.03, 1)));
            }
        }

        public static void FeedTradeBarToReady(this IUpdatable updatable, double start = 1d, double stepSize = 0.1d, long? timeStart = null, long timeStepMilliseconds = 1000) {
            if (updatable == null) throw new ArgumentNullException(nameof(updatable));
            if (updatable == null) throw new ArgumentNullException(nameof(updatable));
            var time = timeStart ?? new DateTime(2000, 1, 1).ToEpochTime();
            for (; !updatable.IsReady; start += stepSize, time += timeStepMilliseconds) {
                updatable.Update(time, DoubleArray.From(new TradeBarValue(start + 0.03, start + 0.05, start - 0.05, start - 0.03, 1)));
            }
        }

        public static void FeedTradeBarToReady(this IUpdatable updatable, IUpdatable[] mustBeReady, double start = 1d, double stepSize = 0.1d, long? timeStart = null, long timeStepMilliseconds = 1000) {
            if (updatable == null) throw new ArgumentNullException(nameof(updatable));
            if (mustBeReady == null) throw new ArgumentNullException(nameof(mustBeReady));
            var time = timeStart ?? new DateTime(2000, 1, 1).ToEpochTime();
            for (; !mustBeReady.All(v => v.IsReady); start += stepSize, time += timeStepMilliseconds) {
                updatable.Update(time, DoubleArray.From(new TradeBarValue(start + 0.03, start + 0.05, start - 0.05, start - 0.03, 1)));
            }
        }
    }
}