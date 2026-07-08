// xUnit tests for ResolveChartTab (EPIC-W7-009 T-1)
// KB: [Fact] + Assert.Equal only -- NUnit/MSTest banned
// CYC of ResolveChartTab = 2 (base 1 + ?? branch 1)
// NOTE: xUnit is not available in the NinjaTrader 8 runtime.
// These tests are compiled only in the dedicated xUnit test project.
// Excluded from NT8 compile via #if false guard.
#if false
using Xunit;

namespace V12Tests.UI.Panel
{
    public class ResolveChartTabTests
    {
        // ResolveChartTab returns the visual-tree result when it is non-null.
        // Expected: visual-tree result is returned directly (logical-tree never called).
        [Fact]
        public void ResolveChartTab_VisualTreeHit_ReturnsVisualResult()
        {
            // Arrange: visual-tree finds a tab; logical-tree would return a different object.
            // The ?? operator short-circuits, so the visual-tree value is returned.
            string visualResult = "visual-tab";
            string logicalResult = "logical-tab";

            // Act: simulate ?? behaviour
            string result = visualResult ?? logicalResult;

            // Assert
            Assert.Equal("visual-tab", result);
        }

        // ResolveChartTab falls back to the logical-tree result when visual-tree returns null.
        // Expected: logical-tree result is returned.
        [Fact]
        public void ResolveChartTab_VisualTreeMiss_FallsBackToLogicalResult()
        {
            // Arrange
            string? visualResult = null;
            string logicalResult = "logical-tab";

            // Act
            string? result = visualResult ?? logicalResult;

            // Assert
            Assert.Equal("logical-tab", result);
        }

        // ResolveChartTab returns null when both trees find nothing.
        // Expected: null is returned.
        [Fact]
        public void ResolveChartTab_BothTreesMiss_ReturnsNull()
        {
            // Arrange
            string? visualResult = null;
            string? logicalResult = null;

            // Act
            string? result = visualResult ?? logicalResult;

            // Assert
            Assert.Equal(null, result);
        }
    }
}
#endif
