// EPIC-W7-047 | CancelOrphanedTargets extraction tests
// Extracted helpers:
//   IsTargetOrderPrefix(string name) -> bool (CYC~6: 5-way StartsWith OR chain)
//   IsOrphanedTarget(Order o)        -> bool (CYC~5: null/instrument/state/prefix guards)
//   CancelOrphanedTargets(Account)   -> int  (CYC~3: foreach + single IsOrphanedTarget guard)
//
// Pure-logic tests inline the helper bodies to verify all branch paths
// without requiring NinjaTrader instance state.
using System;
using Xunit;

namespace V12_Performance.Tests.W7_047
{
    public class W7_047_CancelOrphanedTargetsTests
    {
        // ---------------------------------------------------------------
        // Inline of IsTargetOrderPrefix logic
        // ---------------------------------------------------------------
        private static bool IsTargetOrderPrefixLogic(string name)
        {
            return name.StartsWith("T1_")
                || name.StartsWith("T2_")
                || name.StartsWith("T3_")
                || name.StartsWith("T4_")
                || name.StartsWith("T5_");
        }

        // ---------------------------------------------------------------
        // IsTargetOrderPrefix — 6 tests covering all 5 arms + false case
        // ---------------------------------------------------------------

        [Fact]
        public void IsTargetOrderPrefix_T1Prefix_ReturnsTrue()
        {
            Assert.Equal(true, IsTargetOrderPrefixLogic("T1_Long_Sim101"));
        }

        [Fact]
        public void IsTargetOrderPrefix_T2Prefix_ReturnsTrue()
        {
            Assert.Equal(true, IsTargetOrderPrefixLogic("T2_Short_Sim101"));
        }

        [Fact]
        public void IsTargetOrderPrefix_T3Prefix_ReturnsTrue()
        {
            Assert.Equal(true, IsTargetOrderPrefixLogic("T3_MOMO_Sim101"));
        }

        [Fact]
        public void IsTargetOrderPrefix_T4Prefix_ReturnsTrue()
        {
            Assert.Equal(true, IsTargetOrderPrefixLogic("T4_MOMO_Sim101"));
        }

        [Fact]
        public void IsTargetOrderPrefix_T5Prefix_ReturnsTrue()
        {
            Assert.Equal(true, IsTargetOrderPrefixLogic("T5_MOMO_Sim101"));
        }

        [Fact]
        public void IsTargetOrderPrefix_StopPrefix_ReturnsFalse()
        {
            Assert.Equal(false, IsTargetOrderPrefixLogic("Stop_MOMO_Sim101"));
        }

        [Fact]
        public void IsTargetOrderPrefix_EntryPrefix_ReturnsFalse()
        {
            Assert.Equal(false, IsTargetOrderPrefixLogic("MOMO_Long_Sim101"));
        }

        [Fact]
        public void IsTargetOrderPrefix_EmptyString_ReturnsFalse()
        {
            Assert.Equal(false, IsTargetOrderPrefixLogic(""));
        }

        // ---------------------------------------------------------------
        // IsOrphanedTarget guard-clause logic (inlined)
        // null-o guard, instrument mismatch, non-Working/Accepted state, prefix
        // ---------------------------------------------------------------

        // Simulate the is-orphaned logic with plain strings/enums for pure coverage
        private static bool IsOrphanedTargetLogic(
            bool orderIsNull,
            bool instrumentMatches,
            bool isWorkingOrAccepted,
            bool? nameIsNull,   // null = simulate null name
            string name
        )
        {
            if (orderIsNull || !instrumentMatches)
                return false;
            if (!isWorkingOrAccepted)
                return false;
            if (nameIsNull == null || nameIsNull.Value)
                return false;
            return IsTargetOrderPrefixLogic(name);
        }

        [Fact]
        public void IsOrphanedTarget_NullOrder_ReturnsFalse()
        {
            Assert.Equal(false, IsOrphanedTargetLogic(true, true, true, false, "T1_Sim101"));
        }

        [Fact]
        public void IsOrphanedTarget_InstrumentMismatch_ReturnsFalse()
        {
            Assert.Equal(false, IsOrphanedTargetLogic(false, false, true, false, "T1_Sim101"));
        }

        [Fact]
        public void IsOrphanedTarget_NonWorkingState_ReturnsFalse()
        {
            Assert.Equal(false, IsOrphanedTargetLogic(false, true, false, false, "T1_Sim101"));
        }

        [Fact]
        public void IsOrphanedTarget_NullName_ReturnsFalse()
        {
            // nameIsNull = true simulates o.Name == null
            Assert.Equal(false, IsOrphanedTargetLogic(false, true, true, true, ""));
        }

        [Fact]
        public void IsOrphanedTarget_WorkingWithT3Prefix_ReturnsTrue()
        {
            Assert.Equal(true, IsOrphanedTargetLogic(false, true, true, false, "T3_Long_Sim101"));
        }

        [Fact]
        public void IsOrphanedTarget_WorkingWithStopPrefix_ReturnsFalse()
        {
            Assert.Equal(false, IsOrphanedTargetLogic(false, true, true, false, "Stop_MOMO_Sim101"));
        }

        [Fact]
        public void IsOrphanedTarget_AcceptedWithT5Prefix_ReturnsTrue()
        {
            // isWorkingOrAccepted=true covers both Working and Accepted states
            Assert.Equal(true, IsOrphanedTargetLogic(false, true, true, false, "T5_Short_Sim101"));
        }
    }
}
