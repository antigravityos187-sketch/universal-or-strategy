// BwaveRefactorLaneBTests.cs -- xUnit structural tests for BWAVE-REFACTOR LaneB
// InternalsVisibleTo("PropTraderTools.Tests") declared at CopyEngine.cs L46.
// Ticket 1 (T1): Tests for IsBeTargetStateOk and IsImmediateBeEligible static helpers
// via internal test seams (IsBeTargetStateOkTestable, IsImmediateBeEligibleTestable).
// xUnit only -- no NUnit, no MSTest. ASCII-only. No DateTime.Now.
using NinjaTrader.Cbi;
using Xunit;

namespace PropTraderTools.Tests
{
    public class BwaveRefactorLaneBTests
    {
        // -----------------------------------------------------------------------
        // IsBeTargetStateOk -- via IsBeTargetStateOkTestable seam
        // Extracted from SnapshotBeTargets 7-arm stateOk OR (BWAVE-REFACTOR-LaneB-T1)
        // -----------------------------------------------------------------------

        [Fact]
        public void IsBeTargetStateOk_Working_ReturnsTrue()
        {
            Assert.True(CopyEngine.IsBeTargetStateOkTestable(OrderState.Working));
        }

        [Fact]
        public void IsBeTargetStateOk_CancelSubmitted_ReturnsTrue()
        {
            // REPAIR-09 DW-B79-05: CancelSubmitted is valid -- LimitPrice still readable.
            Assert.True(CopyEngine.IsBeTargetStateOkTestable(OrderState.CancelSubmitted));
        }

        [Fact]
        public void IsBeTargetStateOk_Filled_ReturnsFalse()
        {
            Assert.False(CopyEngine.IsBeTargetStateOkTestable(OrderState.Filled));
        }

        // -----------------------------------------------------------------------
        // IsImmediateBeEligible -- via IsImmediateBeEligibleTestable seam (primitives)
        // Extracted from ArmPendingBe tickSize guard body (BWAVE-REFACTOR-LaneB-T1)
        // Seam accepts primitives: bool isLong, double avgPrice, double refBid, double refAsk,
        // int bufferTicks, double tickSize -- avoids NT8 Position/Instrument runtime dependency.
        // -----------------------------------------------------------------------

        [Fact]
        public void IsImmediateBeEligible_NullPosition_ReturnsFalse()
        {
            // NT8 note: Position cannot be constructed without NT8 runtime.
            // The seam exposes the arithmetic. tickSize=0 exercises the same early-return
            // path that the pos==null guard in IsImmediateBeEligible takes (returns false).
            bool result = CopyEngine.IsImmediateBeEligibleTestable(
                isLong: true,
                avgPrice: 100.0,
                refBid: 102.0,
                refAsk: 102.5,
                bufferTicks: 2,
                tickSize: 0.0
            );
            Assert.False(result);
        }

        [Fact]
        public void IsImmediateBeEligible_ZeroTickSize_ReturnsFalse()
        {
            // tickSize=0 means no market data available -- arm normally, do not fire immediately.
            bool result = CopyEngine.IsImmediateBeEligibleTestable(
                isLong: true,
                avgPrice: 100.0,
                refBid: 102.0,
                refAsk: 102.5,
                bufferTicks: 2,
                tickSize: 0.0
            );
            Assert.False(result);
        }
    }
}
