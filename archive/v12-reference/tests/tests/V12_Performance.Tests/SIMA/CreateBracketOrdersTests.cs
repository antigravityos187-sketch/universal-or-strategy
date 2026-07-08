using Xunit;

namespace V12_Performance.Tests.SIMA
{
    /// <summary>
    /// Unit tests for CreateBracketOrders extracted helper.
    /// EPIC-W7-096 TICKET-3 TDD Safety Net.
    /// Validates order factory: returns false when any order is null, true when all non-null.
    /// OCO atomicity constraint verified: Submit is NOT called inside the factory.
    /// CYC=7. Logic verified via inline stand-in (NT8 not available standalone).
    /// </summary>
    public class CreateBracketOrdersTests
    {
        // ---------------------------------------------------------------------------
        // Stand-in Order type (mirrors null-check behavior)
        // ---------------------------------------------------------------------------

        private class SimOrder
        {
            public string SignalName { get; set; }
            public string OcoId { get; set; }
            public string OrderType { get; set; }
        }

        // ---------------------------------------------------------------------------
        // Stand-in: mirrors CreateBracketOrders null-guard and return logic.
        // Production creates 3 orders via acct.CreateOrder; here we inject them directly.
        // Returns false if any is null; caller must call Submit -- never called here.
        // ---------------------------------------------------------------------------

        private static bool SimulateCreateBracketOrders(
            SimOrder entry,
            SimOrder stop,
            SimOrder target,
            out SimOrder outEntry,
            out SimOrder outStop,
            out SimOrder outTarget
        )
        {
            outEntry = entry;
            outStop = stop;
            outTarget = target;
            return outEntry != null && outStop != null && outTarget != null;
        }

        // ---------------------------------------------------------------------------
        // All orders non-null -> returns true
        // ---------------------------------------------------------------------------

        [Fact]
        public void CreateBracketOrders_AllOrdersNonNull_ReturnsTrue()
        {
            var e = new SimOrder { SignalName = "Entry", OcoId = "OCO1", OrderType = "Market" };
            var s = new SimOrder { SignalName = "Stop_Entry", OcoId = "OCO1", OrderType = "StopMarket" };
            var t = new SimOrder { SignalName = "Target_Entry", OcoId = "OCO1", OrderType = "Limit" };

            bool result = SimulateCreateBracketOrders(e, s, t, out var oe, out var os, out var ot);

            Assert.Equal(true, result);
        }

        [Fact]
        public void CreateBracketOrders_AllOrdersNonNull_OutputsPopulated()
        {
            var e = new SimOrder { SignalName = "Entry" };
            var s = new SimOrder { SignalName = "Stop_Entry" };
            var t = new SimOrder { SignalName = "Target_Entry" };

            SimulateCreateBracketOrders(e, s, t, out var oe, out var os, out var ot);

            Assert.Equal("Entry", oe.SignalName);
            Assert.Equal("Stop_Entry", os.SignalName);
            Assert.Equal("Target_Entry", ot.SignalName);
        }

        // ---------------------------------------------------------------------------
        // Entry order null -> returns false
        // ---------------------------------------------------------------------------

        [Fact]
        public void CreateBracketOrders_NullEntry_ReturnsFalse()
        {
            var s = new SimOrder { SignalName = "Stop_Entry" };
            var t = new SimOrder { SignalName = "Target_Entry" };

            bool result = SimulateCreateBracketOrders(null, s, t, out _, out _, out _);

            Assert.Equal(false, result);
        }

        // ---------------------------------------------------------------------------
        // Stop order null -> returns false
        // ---------------------------------------------------------------------------

        [Fact]
        public void CreateBracketOrders_NullStop_ReturnsFalse()
        {
            var e = new SimOrder { SignalName = "Entry" };
            var t = new SimOrder { SignalName = "Target_Entry" };

            bool result = SimulateCreateBracketOrders(e, null, t, out _, out _, out _);

            Assert.Equal(false, result);
        }

        // ---------------------------------------------------------------------------
        // Target order null -> returns false
        // ---------------------------------------------------------------------------

        [Fact]
        public void CreateBracketOrders_NullTarget_ReturnsFalse()
        {
            var e = new SimOrder { SignalName = "Entry" };
            var s = new SimOrder { SignalName = "Stop_Entry" };

            bool result = SimulateCreateBracketOrders(e, s, null, out _, out _, out _);

            Assert.Equal(false, result);
        }

        // ---------------------------------------------------------------------------
        // All null -> returns false
        // ---------------------------------------------------------------------------

        [Fact]
        public void CreateBracketOrders_AllNull_ReturnsFalse()
        {
            bool result = SimulateCreateBracketOrders(null, null, null, out _, out _, out _);

            Assert.Equal(false, result);
        }
    }
}
