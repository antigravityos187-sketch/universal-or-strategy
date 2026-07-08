using Xunit;

namespace V12_Performance.Tests.Core
{
    /// <summary>
    /// Unit tests for IsClickWithinChartBounds helper logic.
    /// EPIC-W7-046 T1: UI Safety Fence predicate extracted from HandleChartClick_ConvertPrice.
    /// Tests pure boolean bounds check: returns true if within [0..panelW] x [0..panelH].
    /// Jane Street: will_wilson -- xUnit [Fact] + Assert.Equal(); deterministic; test each helper independently.
    /// Note: System.Windows.Point is WPF-only; replaced with a plain (X,Y) record for cross-platform testing.
    /// </summary>
    public class IsClickWithinChartBoundsTests
    {
        // -----------------------------------------------------------------------
        // Plain-value substitute for System.Windows.Point (WPF not available in net6.0 on Linux).
        // Matches the fields accessed in IsClickWithinChartBounds: .X and .Y.
        // -----------------------------------------------------------------------
        private readonly record struct TestPoint(double X, double Y);

        // -----------------------------------------------------------------------
        // Mirror of the extracted private helper -- pure predicate, no NT8 deps.
        // Signature mirrors: private bool IsClickWithinChartBounds(Point mouseInPanel, double panelW, double panelH)
        // -----------------------------------------------------------------------
        private static bool IsClickWithinChartBounds(TestPoint mouseInPanel, double panelW, double panelH)
        {
            return !(
                mouseInPanel.X < 0
                || mouseInPanel.X > panelW
                || mouseInPanel.Y < 0
                || mouseInPanel.Y > panelH
            );
        }

        // -----------------------------------------------------------------------
        // Happy-path: inside bounds
        // -----------------------------------------------------------------------

        [Fact]
        public void InsideBounds_ReturnsTrue()
        {
            var point = new TestPoint(50, 50);
            Assert.Equal(true, IsClickWithinChartBounds(point, 100, 100));
        }

        [Fact]
        public void Origin_ReturnsTrue()
        {
            // X==0, Y==0 is the top-left corner -- inclusive lower bound
            var point = new TestPoint(0, 0);
            Assert.Equal(true, IsClickWithinChartBounds(point, 100, 100));
        }

        [Fact]
        public void AtMaxBoundary_ReturnsTrue()
        {
            // X==panelW, Y==panelH is the bottom-right corner -- inclusive upper bound
            var point = new TestPoint(100, 100);
            Assert.Equal(true, IsClickWithinChartBounds(point, 100, 100));
        }

        // -----------------------------------------------------------------------
        // Out-of-bounds: each of the four predicates triggers a false
        // -----------------------------------------------------------------------

        [Fact]
        public void NegativeX_ReturnsFalse()
        {
            var point = new TestPoint(-1, 50);
            Assert.Equal(false, IsClickWithinChartBounds(point, 100, 100));
        }

        [Fact]
        public void XExceedsPanelW_ReturnsFalse()
        {
            var point = new TestPoint(101, 50);
            Assert.Equal(false, IsClickWithinChartBounds(point, 100, 100));
        }

        [Fact]
        public void NegativeY_ReturnsFalse()
        {
            var point = new TestPoint(50, -1);
            Assert.Equal(false, IsClickWithinChartBounds(point, 100, 100));
        }

        [Fact]
        public void YExceedsPanelH_ReturnsFalse()
        {
            var point = new TestPoint(50, 101);
            Assert.Equal(false, IsClickWithinChartBounds(point, 100, 100));
        }
    }
}

// Made with Bob (EPIC-W7-046 T1)
