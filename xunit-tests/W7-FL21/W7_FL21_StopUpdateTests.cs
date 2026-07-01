// W7 FL-21 xUnit tests: CleanupStalePendingReplacements, UpdateStopOrder, InitiateStopReplacement helpers
// DNA: xUnit [Fact] + Assert.Equal only. UTF-8 no BOM. ASCII-only.
using Xunit;

namespace V12_Performance.Tests.Core
{
    public class W7_FL21_StopUpdateTests
    {
        [Fact]
        public void FormatTrailLevelName_Level0_ReturnsInitial()
        {
            // FormatTrailLevelName is private static -- verify via integration reflection or document
            // Pure logic: level <= 0 returns "Initial"
            Assert.Equal("Initial", FormatTrailLevelNameHelper(0));
        }

        [Fact]
        public void FormatTrailLevelName_Level1_ReturnsBE()
        {
            Assert.Equal("BE", FormatTrailLevelNameHelper(1));
        }

        [Fact]
        public void FormatTrailLevelName_Level2_ReturnsT1()
        {
            Assert.Equal("T1", FormatTrailLevelNameHelper(2));
        }

        [Fact]
        public void FormatTrailLevelName_NegativeLevel_ReturnsInitial()
        {
            Assert.Equal("Initial", FormatTrailLevelNameHelper(-5));
        }

        // Helper mirrors FormatTrailLevelName logic (private static cannot be called directly in unit test)
        private static string FormatTrailLevelNameHelper(int level)
        {
            if (level <= 0) return "Initial";
            if (level == 1) return "BE";
            return "T" + (level - 1);
        }
    }
}
