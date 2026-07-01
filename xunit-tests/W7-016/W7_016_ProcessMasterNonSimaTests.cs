using System.Collections.Generic;
using Xunit;

namespace V12_Performance.Tests.Core
{
    // [EPIC-W7-016] Tests for CancelAll_ProcessMasterNonSima extracted helper.
    // Mirrors the pure predicate logic: iterate orders, skip non-cancellable,
    // skip bracket orders, cancel and count. No NinjaTrader dependencies.
    public class W7_016_ProcessMasterNonSimaTests
    {
        // Standalone predicate: mirrors CancelAll_IsBracketOrder (W7-015)
        private static bool IsBracketOrder(string oName)
        {
            return oName.StartsWith("Stop_")
                || oName.StartsWith("S_")
                || oName.StartsWith("T1_")
                || oName.StartsWith("T2_")
                || oName.StartsWith("T3_")
                || oName.StartsWith("T4_")
                || oName.StartsWith("T5_");
        }

        // Mirrors CancelAll_ProcessMasterNonSima logic with injectable predicates.
        // (bool isCancellable, string name) represents each order's observable state.
        private static int ProcessMasterNonSima(IEnumerable<(bool isCancellable, string name)> orders)
        {
            int cancelled = 0;
            foreach (var order in orders)
            {
                if (!order.isCancellable)
                    continue;
                if (IsBracketOrder(order.name))
                    continue;
                cancelled++;
            }
            return cancelled;
        }

        [Fact]
        public void ProcessMasterNonSima_ReturnsZero_WhenNoOrders()
        {
            int result = ProcessMasterNonSima(new List<(bool, string)>());
            Assert.Equal(0, result);
        }

        [Fact]
        public void ProcessMasterNonSima_ReturnsZero_WhenAllOrdersNotCancellable()
        {
            var orders = new List<(bool, string)>
            {
                (false, "Entry_BES"),
                (false, "Entry_SES"),
            };
            Assert.Equal(0, ProcessMasterNonSima(orders));
        }

        [Fact]
        public void ProcessMasterNonSima_ReturnsZero_WhenAllOrdersAreBracketOrders()
        {
            var orders = new List<(bool, string)>
            {
                (true, "Stop_BES"),
                (true, "T1_BES"),
                (true, "T2_BES"),
            };
            Assert.Equal(0, ProcessMasterNonSima(orders));
        }

        [Fact]
        public void ProcessMasterNonSima_CountsOnly_NonBracketCancellableOrders()
        {
            var orders = new List<(bool, string)>
            {
                (true, "Entry_BES"),    // cancellable, not bracket -> count
                (true, "Stop_BES"),    // cancellable, bracket -> skip
                (false, "Entry_SES"),  // not cancellable -> skip
                (true, "Entry_SES2"),  // cancellable, not bracket -> count
            };
            Assert.Equal(2, ProcessMasterNonSima(orders));
        }

        [Fact]
        public void ProcessMasterNonSima_CountsAll_WhenAllCancellableNonBracket()
        {
            var orders = new List<(bool, string)>
            {
                (true, "Entry_A"),
                (true, "Entry_B"),
                (true, "Entry_C"),
            };
            Assert.Equal(3, ProcessMasterNonSima(orders));
        }

        [Fact]
        public void ProcessMasterNonSima_SkipsBracketPrefix_S_Underscore()
        {
            var orders = new List<(bool, string)>
            {
                (true, "S_BES"),   // bracket prefix S_ -> skip
                (true, "Entry_X"), // not bracket -> count
            };
            Assert.Equal(1, ProcessMasterNonSima(orders));
        }
    }
}
