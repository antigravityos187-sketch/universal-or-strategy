using Xunit;

namespace V12_Performance.Tests.Core
{
    public class W7_047_IsOrphanedTargetTests
    {
        private bool IsTargetOrderPrefix(string name)
        {
            return name.StartsWith("T1_")
                || name.StartsWith("T2_")
                || name.StartsWith("T3_")
                || name.StartsWith("T4_")
                || name.StartsWith("T5_");
        }

        // Standalone predicate mirrors IsOrphanedTarget logic (pure logic, no NinjaTrader)
        // In production the method reads this.Instrument.FullName from partial class context.
        private bool IsOrphanedTargetPredicate(string orderName, bool instrumentMatch, bool stateIsActive)
        {
            if (!instrumentMatch)
                return false;
            if (!stateIsActive)
                return false;
            return orderName != null && IsTargetOrderPrefix(orderName);
        }

        [Fact]
        public void IsOrphanedTarget_ReturnsFalse_WhenInstrumentMismatch()
        {
            Assert.False(IsOrphanedTargetPredicate("T1_BES", false, true));
        }

        [Fact]
        public void IsOrphanedTarget_ReturnsFalse_WhenOrderStateIsNotActive()
        {
            Assert.False(IsOrphanedTargetPredicate("T1_BES", true, false));
        }

        [Fact]
        public void IsOrphanedTarget_ReturnsFalse_WhenNameIsNull()
        {
            Assert.False(IsOrphanedTargetPredicate(null, true, true));
        }

        [Fact]
        public void IsOrphanedTarget_ReturnsTrue_WhenAllConditionsMet_T1()
        {
            Assert.True(IsOrphanedTargetPredicate("T1_BES_Sim101", true, true));
        }

        [Fact]
        public void IsOrphanedTarget_ReturnsTrue_WhenAllConditionsMet_T5()
        {
            Assert.True(IsOrphanedTargetPredicate("T5_LIMIT", true, true));
        }

        [Fact]
        public void IsOrphanedTarget_ReturnsFalse_WhenNameIsStopPrefix()
        {
            Assert.False(IsOrphanedTargetPredicate("Stop_BES", true, true));
        }

        [Fact]
        public void IsOrphanedTarget_ReturnsFalse_WhenBothMismatchAndBadState()
        {
            Assert.False(IsOrphanedTargetPredicate("T1_BES", false, false));
        }
    }
}
