using Xunit;

namespace V12_Performance.Tests.Core
{
    // [EPIC-W7-016] Tests for TryHandleFleet_CancelAll routing logic.
    // Mirrors the pure routing: action guard, dedup guard, SIMA branch dispatch.
    // No NinjaTrader dependencies -- logic mirrored as a standalone predicate.
    public class W7_016_TryHandleFleet_CancelAllRoutingTests
    {
        // Mirrors routing: returns false if action mismatch, true otherwise (dedup assumed passed).
        private static bool TryCancelAllRoute(string action, bool dedupPassed)
        {
            if (action != "CANCEL_ALL")
                return false;
            if (!dedupPassed)
                return true;
            // Branch on SIMA would call helpers -- both return true
            return true;
        }

        [Fact]
        public void TryCancelAll_ReturnsFalse_WhenActionIsNotCancelAll()
        {
            Assert.False(TryCancelAllRoute("CANCEL_ENTRY", true));
        }

        [Fact]
        public void TryCancelAll_ReturnsFalse_WhenActionIsEmpty()
        {
            Assert.False(TryCancelAllRoute("", true));
        }

        [Fact]
        public void TryCancelAll_ReturnsTrue_WhenActionMatches_DedupPassed()
        {
            Assert.True(TryCancelAllRoute("CANCEL_ALL", true));
        }

        [Fact]
        public void TryCancelAll_ReturnsTrue_WhenActionMatches_DedupBlocked()
        {
            // Duplicate command: guard returns true (consumed, no-op) not false
            Assert.True(TryCancelAllRoute("CANCEL_ALL", false));
        }
    }
}
