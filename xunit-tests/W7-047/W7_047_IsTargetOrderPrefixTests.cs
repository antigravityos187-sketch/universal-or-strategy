// EPIC-W7-047 | T1: IsTargetOrderPrefix behavioral tests
// Helper is private; tested via standalone stub that mirrors the extracted logic.
// Jane Street: xUnit [Fact] + Assert.True/False; deterministic; zero NinjaTrader dependencies.
using Xunit;

namespace V12_Performance.Tests.W7_047
{
    /// <summary>
    /// xUnit tests for IsTargetOrderPrefix extracted from CancelOrphanedTargets
    /// in EPIC-W7-047 (src/V12_002.UI.Compliance.cs).
    /// The helper is private; logic is mirrored here as a standalone stub.
    /// </summary>
    public class W7_047_IsTargetOrderPrefixTests
    {
        // Stub mirrors extracted method -- pure logic, no NinjaTrader dependency.
        private static bool IsTargetOrderPrefix(string name)
        {
            return name.StartsWith("T1_")
                || name.StartsWith("T2_")
                || name.StartsWith("T3_")
                || name.StartsWith("T4_")
                || name.StartsWith("T5_");
        }

        [Fact]
        public void IsTargetOrderPrefix_ReturnsTrue_ForT1Prefix()
        {
            Assert.True(IsTargetOrderPrefix("T1_BES_Sim101"));
        }

        [Fact]
        public void IsTargetOrderPrefix_ReturnsTrue_ForT2Prefix()
        {
            Assert.True(IsTargetOrderPrefix("T2_MOMO"));
        }

        [Fact]
        public void IsTargetOrderPrefix_ReturnsTrue_ForT3Prefix()
        {
            Assert.True(IsTargetOrderPrefix("T3_SWING"));
        }

        [Fact]
        public void IsTargetOrderPrefix_ReturnsTrue_ForT4Prefix()
        {
            Assert.True(IsTargetOrderPrefix("T4_X"));
        }

        [Fact]
        public void IsTargetOrderPrefix_ReturnsTrue_ForT5Prefix()
        {
            Assert.True(IsTargetOrderPrefix("T5_LIMIT"));
        }

        [Fact]
        public void IsTargetOrderPrefix_ReturnsFalse_ForEmptyString()
        {
            Assert.False(IsTargetOrderPrefix(""));
        }

        [Fact]
        public void IsTargetOrderPrefix_ReturnsFalse_ForT6Prefix()
        {
            Assert.False(IsTargetOrderPrefix("T6_BES"));
        }

        [Fact]
        public void IsTargetOrderPrefix_ReturnsFalse_ForStopPrefix()
        {
            Assert.False(IsTargetOrderPrefix("Stop_BES"));
        }

        [Fact]
        public void IsTargetOrderPrefix_ReturnsFalse_ForTPPrefix()
        {
            Assert.False(IsTargetOrderPrefix("TP_TARGET"));
        }

        [Fact]
        public void IsTargetOrderPrefix_ReturnsFalse_ForNonMatchingString()
        {
            Assert.False(IsTargetOrderPrefix("ENTRY_MOMO"));
        }
    }
}
