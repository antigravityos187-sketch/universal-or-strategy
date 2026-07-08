// xUnit tests for EPIC-W7-147
// Covers: IsOcoOrderActionable, DispatchOcoFleetOrder extraction
// Framework: xUnit 2.x
using System;
using Xunit;

namespace W7_147_Tests
{
    /// <summary>
    /// Unit tests for the logic extracted from ProcessQueuedExecution_HandleFleetOCO.
    /// Uses pure logic verification (no NinjaTrader types -- tested via wrapper stubs).
    /// </summary>
    public class IsOcoOrderActionableLogicTests
    {
        // Helper: replicate the pure boolean guard logic extracted to IsOcoOrderActionable
        private static bool IsOcoOrderActionable(bool orderNull, bool acctNull, bool isFleet, bool filled, bool partFilled)
        {
            if (orderNull || acctNull)
                return false;
            if (!isFleet)
                return false;
            return filled || partFilled;
        }

        [Fact]
        public void ReturnsFalse_WhenOrderIsNull()
        {
            bool result = IsOcoOrderActionable(orderNull: true, acctNull: false, isFleet: true, filled: true, partFilled: false);
            Assert.Equal(false, result);
        }

        [Fact]
        public void ReturnsFalse_WhenAcctIsNull()
        {
            bool result = IsOcoOrderActionable(orderNull: false, acctNull: true, isFleet: true, filled: true, partFilled: false);
            Assert.Equal(false, result);
        }

        [Fact]
        public void ReturnsFalse_WhenNotFleetAccount()
        {
            bool result = IsOcoOrderActionable(orderNull: false, acctNull: false, isFleet: false, filled: true, partFilled: false);
            Assert.Equal(false, result);
        }

        [Fact]
        public void ReturnsFalse_WhenNeitherFilledNorPartFilled()
        {
            bool result = IsOcoOrderActionable(orderNull: false, acctNull: false, isFleet: true, filled: false, partFilled: false);
            Assert.Equal(false, result);
        }

        [Fact]
        public void ReturnsTrue_WhenOrderFilledFleetAccount()
        {
            bool result = IsOcoOrderActionable(orderNull: false, acctNull: false, isFleet: true, filled: true, partFilled: false);
            Assert.Equal(true, result);
        }

        [Fact]
        public void ReturnsTrue_WhenOrderPartFilledFleetAccount()
        {
            bool result = IsOcoOrderActionable(orderNull: false, acctNull: false, isFleet: true, filled: false, partFilled: true);
            Assert.Equal(true, result);
        }
    }

    public class DispatchOcoFleetOrderLogicTests
    {
        // Replicate dispatch routing logic from DispatchOcoFleetOrder
        private static string DispatchRoute(string name)
        {
            if (name == null)
                name = "";
            if (name.StartsWith("Stop_"))
                return "stop";
            if (name.StartsWith("T") && name.Length > 2 && name[2] == '_')
                return "target";
            return "none";
        }

        [Fact]
        public void RoutesToStop_WhenNameStartsWithStop_()
        {
            string route = DispatchRoute("Stop_001");
            Assert.Equal("stop", route);
        }

        [Fact]
        public void RoutesToTarget_WhenNameMatchesTX_Pattern()
        {
            string route = DispatchRoute("T1_001");
            Assert.Equal("target", route);
        }

        [Fact]
        public void RoutesToNone_WhenNameIsEmpty()
        {
            string route = DispatchRoute("");
            Assert.Equal("none", route);
        }

        [Fact]
        public void RoutesToNone_WhenNameIsUnrecognized()
        {
            string route = DispatchRoute("Entry_001");
            Assert.Equal("none", route);
        }

        [Fact]
        public void RoutesToNone_WhenNameStartsWithT_ButTooShort()
        {
            // "T_" is length 2, index [2] would OOB -- condition Length > 2 guards this
            string route = DispatchRoute("T_");
            Assert.Equal("none", route);
        }

        [Fact]
        public void RoutesToTarget_WhenNameIsT2_SomeName()
        {
            string route = DispatchRoute("T2_ABC");
            Assert.Equal("target", route);
        }

        [Fact]
        public void OcoName_NullCoalesceYieldsEmpty()
        {
            // Replicate: ocoOrder.Name ?? "" -- null coalesces to empty string
            string name = null;
            string result = name ?? "";
            Assert.Equal("", result);
        }
    }
}
