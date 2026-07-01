using System;
using Xunit;

namespace V12_Performance.Tests.SIMA
{
    /// <summary>
    /// Unit tests for IsTerminalOrderState and IsZombieTargetOrder extracted helpers.
    /// EPIC-W7-098 T1 + T2 TDD Safety Net.
    /// Validates pure-predicate logic extracted from ProcessFlattenWorkItem_CancelOrders.
    /// Since V12_002 is a NinjaTrader Strategy (not directly instantiable), logic is
    /// verified via standalone mirror functions that reproduce the exact branch structure.
    /// OrderState is mirrored locally -- NinjaTrader.Cbi not available in standalone .NET build.
    /// </summary>
    public class FlattenHelperTests
    {
        // ---------------------------------------------------------------------------
        // Local mirror of NinjaTrader.Cbi.OrderState (subset used by helpers)
        // ---------------------------------------------------------------------------

        private enum OrderState
        {
            Accepted,
            Cancelled,
            CancelPending,
            CancelSubmitted,
            Filled,
            Rejected,
            Submitted,
            Working,
        }

        // ---------------------------------------------------------------------------
        // Mirror: IsTerminalOrderState (CYC=6 -- base 1 + 5 OR conditions)
        // ---------------------------------------------------------------------------

        private static bool IsTerminalOrderState(OrderState state)
        {
            return state == OrderState.Cancelled
                || state == OrderState.CancelPending
                || state == OrderState.CancelSubmitted
                || state == OrderState.Filled
                || state == OrderState.Rejected;
        }

        // ---------------------------------------------------------------------------
        // Mirror: IsZombieTargetOrder (CYC=7 -- base 1 + 6 StartsWith OR conditions)
        // ---------------------------------------------------------------------------

        private static bool IsZombieTargetOrder(string orderName)
        {
            return orderName.StartsWith("EMERGENCY_STOP_", StringComparison.OrdinalIgnoreCase)
                || orderName.StartsWith("T1_", StringComparison.OrdinalIgnoreCase)
                || orderName.StartsWith("T2_", StringComparison.OrdinalIgnoreCase)
                || orderName.StartsWith("T3_", StringComparison.OrdinalIgnoreCase)
                || orderName.StartsWith("T4_", StringComparison.OrdinalIgnoreCase)
                || orderName.StartsWith("T5_", StringComparison.OrdinalIgnoreCase);
        }

        // ---------------------------------------------------------------------------
        // IsTerminalOrderState -- terminal states return true
        // ---------------------------------------------------------------------------

        [Fact]
        public void IsTerminalOrderState_Cancelled_ReturnsTrue()
        {
            Assert.Equal(true, IsTerminalOrderState(OrderState.Cancelled));
        }

        [Fact]
        public void IsTerminalOrderState_CancelPending_ReturnsTrue()
        {
            Assert.Equal(true, IsTerminalOrderState(OrderState.CancelPending));
        }

        [Fact]
        public void IsTerminalOrderState_CancelSubmitted_ReturnsTrue()
        {
            Assert.Equal(true, IsTerminalOrderState(OrderState.CancelSubmitted));
        }

        [Fact]
        public void IsTerminalOrderState_Filled_ReturnsTrue()
        {
            Assert.Equal(true, IsTerminalOrderState(OrderState.Filled));
        }

        [Fact]
        public void IsTerminalOrderState_Rejected_ReturnsTrue()
        {
            Assert.Equal(true, IsTerminalOrderState(OrderState.Rejected));
        }

        // ---------------------------------------------------------------------------
        // IsTerminalOrderState -- non-terminal states return false
        // ---------------------------------------------------------------------------

        [Fact]
        public void IsTerminalOrderState_Working_ReturnsFalse()
        {
            Assert.Equal(false, IsTerminalOrderState(OrderState.Working));
        }

        [Fact]
        public void IsTerminalOrderState_Submitted_ReturnsFalse()
        {
            Assert.Equal(false, IsTerminalOrderState(OrderState.Submitted));
        }

        [Fact]
        public void IsTerminalOrderState_Accepted_ReturnsFalse()
        {
            Assert.Equal(false, IsTerminalOrderState(OrderState.Accepted));
        }

        // ---------------------------------------------------------------------------
        // IsZombieTargetOrder -- matching prefixes return true
        // ---------------------------------------------------------------------------

        [Fact]
        public void IsZombieTargetOrder_EmergencyStop_ReturnsTrue()
        {
            Assert.Equal(true, IsZombieTargetOrder("EMERGENCY_STOP_ACCT1"));
        }

        [Fact]
        public void IsZombieTargetOrder_T1_ReturnsTrue()
        {
            Assert.Equal(true, IsZombieTargetOrder("T1_LongEntry"));
        }

        [Fact]
        public void IsZombieTargetOrder_T2_ReturnsTrue()
        {
            Assert.Equal(true, IsZombieTargetOrder("T2_Bracket"));
        }

        [Fact]
        public void IsZombieTargetOrder_T3_ReturnsTrue()
        {
            Assert.Equal(true, IsZombieTargetOrder("T3_Stop"));
        }

        [Fact]
        public void IsZombieTargetOrder_T4_ReturnsTrue()
        {
            Assert.Equal(true, IsZombieTargetOrder("T4_Target"));
        }

        [Fact]
        public void IsZombieTargetOrder_T5_ReturnsTrue()
        {
            Assert.Equal(true, IsZombieTargetOrder("T5_Limit"));
        }

        // ---------------------------------------------------------------------------
        // IsZombieTargetOrder -- case-insensitivity
        // ---------------------------------------------------------------------------

        [Fact]
        public void IsZombieTargetOrder_LowerCaseT1_ReturnsTrue()
        {
            Assert.Equal(true, IsZombieTargetOrder("t1_entry"));
        }

        [Fact]
        public void IsZombieTargetOrder_LowerCaseEmergencyStop_ReturnsTrue()
        {
            Assert.Equal(true, IsZombieTargetOrder("emergency_stop_xyz"));
        }

        // ---------------------------------------------------------------------------
        // IsZombieTargetOrder -- non-matching prefixes return false
        // ---------------------------------------------------------------------------

        [Fact]
        public void IsZombieTargetOrder_ManualOrder_ReturnsFalse()
        {
            Assert.Equal(false, IsZombieTargetOrder("ManualEntry_001"));
        }

        [Fact]
        public void IsZombieTargetOrder_FlattenOrder_ReturnsFalse()
        {
            Assert.Equal(false, IsZombieTargetOrder("Flatten_MasterLong"));
        }

        [Fact]
        public void IsZombieTargetOrder_T6Prefix_ReturnsFalse()
        {
            Assert.Equal(false, IsZombieTargetOrder("T6_ShouldNotMatch"));
        }
    }
}
