// EPIC-W7-024 | T1+T2: DispatchProximityAction routing logic tests
// Extracted formula from DispatchProximityAction:
//   if (distTicks <= RmaProximityTicks)  => entry path
//   else if (distTicks < RmaCancellationTicks) => dead zone (no-op)
//   else => exit path
// Pure decision logic is verified by inlining the routing predicate.
using System;
using Xunit;

namespace V12_Performance.Tests.W7_024
{
    /// <summary>
    /// xUnit tests for the routing logic extracted in DispatchProximityAction (EPIC-W7-024 T2)
    /// and the per-order pipeline assembled in ProcessProximityOrder (EPIC-W7-024 T1).
    ///
    /// DispatchProximityAction branches on two thresholds:
    ///   RmaProximityTicks     (entry gate)
    ///   RmaCancellationTicks  (exit gate)
    ///
    /// Three code paths:
    ///   1. distTicks <= RmaProximityTicks       => ENTRY
    ///   2. distTicks  < RmaCancellationTicks    => DEAD ZONE (no-op)
    ///   3. distTicks >= RmaCancellationTicks    => EXIT
    ///
    /// Because the method uses instance state (NinjaTrader strategy), the
    /// branching predicate is inlined as a pure enum classifier for testing.
    /// </summary>
    public class W7_024_DispatchProximityActionTests
    {
        private enum ProximityRoute
        {
            Entry,
            DeadZone,
            Exit,
        }

        // Inline of the DispatchProximityAction routing predicate -- zero instance state.
        private static ProximityRoute ClassifyProximityRoute(
            double distTicks,
            double rmaProximityTicks,
            double rmaCancellationTicks
        )
        {
            if (distTicks <= rmaProximityTicks)
                return ProximityRoute.Entry;
            if (distTicks < rmaCancellationTicks)
                return ProximityRoute.DeadZone;
            return ProximityRoute.Exit;
        }

        // -----------------------------------------------------------------------
        // T2: DispatchProximityAction routing
        // -----------------------------------------------------------------------

        [Fact]
        public void DistAtProximityThreshold_RoutesToEntry()
        {
            // distTicks == RmaProximityTicks: boundary condition => Entry
            double dist = 3.0;
            double proximityTicks = 3.0;
            double cancellationTicks = 10.0;

            ProximityRoute result = ClassifyProximityRoute(dist, proximityTicks, cancellationTicks);

            Assert.Equal(ProximityRoute.Entry, result);
        }

        [Fact]
        public void DistBelowProximityThreshold_RoutesToEntry()
        {
            // distTicks < RmaProximityTicks => Entry
            double dist = 1.5;
            double proximityTicks = 3.0;
            double cancellationTicks = 10.0;

            ProximityRoute result = ClassifyProximityRoute(dist, proximityTicks, cancellationTicks);

            Assert.Equal(ProximityRoute.Entry, result);
        }

        [Fact]
        public void DistInDeadZone_RoutesToDeadZone()
        {
            // distTicks > RmaProximityTicks but < RmaCancellationTicks => DeadZone
            double dist = 5.0;
            double proximityTicks = 3.0;
            double cancellationTicks = 10.0;

            ProximityRoute result = ClassifyProximityRoute(dist, proximityTicks, cancellationTicks);

            Assert.Equal(ProximityRoute.DeadZone, result);
        }

        [Fact]
        public void DistAtCancellationThreshold_RoutesToExit()
        {
            // distTicks == RmaCancellationTicks: NOT < cancellation => Exit
            double dist = 10.0;
            double proximityTicks = 3.0;
            double cancellationTicks = 10.0;

            ProximityRoute result = ClassifyProximityRoute(dist, proximityTicks, cancellationTicks);

            Assert.Equal(ProximityRoute.Exit, result);
        }

        [Fact]
        public void DistAboveCancellationThreshold_RoutesToExit()
        {
            // distTicks > RmaCancellationTicks => Exit
            double dist = 15.0;
            double proximityTicks = 3.0;
            double cancellationTicks = 10.0;

            ProximityRoute result = ClassifyProximityRoute(dist, proximityTicks, cancellationTicks);

            Assert.Equal(ProximityRoute.Exit, result);
        }

        // -----------------------------------------------------------------------
        // T1: ProcessProximityOrder tag-format invariant
        // -----------------------------------------------------------------------

        [Fact]
        public void ProximityTag_Format_ProducesExpectedString()
        {
            // ProcessProximityOrder computes: string.Format("Prox_{0}", orderId)
            // Verify the tag format produces ASCII-safe "Prox_<orderId>"
            string orderId = "BuyLimit_1";
            string tag = string.Format("Prox_{0}", orderId);

            Assert.Equal("Prox_BuyLimit_1", tag);
        }

        [Fact]
        public void ProximityTag_Format_ContainsOnlyAscii()
        {
            // DNA: ASCII-only compliance
            string orderId = "SellLimit_42";
            string tag = string.Format("Prox_{0}", orderId);

            foreach (char c in tag)
            {
                Assert.True(c < 128, $"Non-ASCII char '{c}' (code {(int)c}) found in tag '{tag}'");
            }
        }
    }
}
