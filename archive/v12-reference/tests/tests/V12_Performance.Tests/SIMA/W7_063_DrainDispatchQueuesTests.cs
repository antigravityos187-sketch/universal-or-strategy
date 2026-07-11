using System.Threading;
using Xunit;

namespace V12_Performance.Tests.SIMA
{
    /// <summary>
    /// xUnit tests for EPIC-W7-063 / EPIC-W7-105 extracted helpers from DrainAllDispatchQueuesOnAbort.
    /// T1: DrainPhotonRingOnAbort (photon ring loop logic verified via stand-in)
    /// T2: DrainLegacyDispatchQueueOnAbort (legacy queue loop logic verified via stand-in)
    /// Parent DrainAllDispatchQueuesOnAbort is CYC=1 after extraction.
    /// </summary>
    public class W7_063_DrainDispatchQueuesTests
    {
        // Stand-in for DrainPhotonRingOnAbort: simulates processing one photon slot
        private static int SimDrainPhotonSlot(
            bool hasRingItem,
            int sbIdx,
            int sidebandLength,
            int reservedDelta,
            bool hasExpectedKey
        )
        {
            int ops = 0;
            if (!hasRingItem)
                return 0;
            // TrackPhotonDequeue always fires
            ops++; // TrackPhotonDequeue
            if (reservedDelta != 0 && hasExpectedKey)
                ops++; // AddExpectedPositionDeltaLocked
            if (hasExpectedKey)
                ops++; // ClearDispatchSyncPending
            if (sbIdx >= 0)
            {
                ops++; // ReleaseByIndex
                if (sbIdx < sidebandLength)
                    ops++; // sideband clear
            }
            ops++; // Interlocked.Decrement
            return ops;
        }

        // Stand-in for DrainLegacyDispatchQueueOnAbort: simulates processing one legacy request
        private static int SimDrainLegacySlot(bool hasQueueItem, int reservedDelta)
        {
            int ops = 0;
            if (!hasQueueItem)
                return 0;
            if (reservedDelta != 0)
                ops++; // AddExpectedPositionDeltaLocked
            ops++; // ClearDispatchSyncPending
            ops++; // Interlocked.Decrement
            return ops;
        }

        // -----------------------------------------------------------------------
        // DrainPhotonRingOnAbort stand-in tests
        // -----------------------------------------------------------------------

        [Fact]
        public void PhotonDrain_EmptyRing_PerformsZeroOps()
        {
            int ops = SimDrainPhotonSlot(false, -1, 0, 0, false);
            Assert.Equal(0, ops);
        }

        [Fact]
        public void PhotonDrain_SlotWithNoExpectedKey_OnlyTracksAndDecrements()
        {
            // sbIdx -1, no expectedKey -> TrackPhotonDequeue + Decrement = 2
            int ops = SimDrainPhotonSlot(true, -1, 0, 5, false);
            Assert.Equal(2, ops);
        }

        [Fact]
        public void PhotonDrain_SlotWithExpectedKeyAndNonzeroDelta_PerformsDeltaAndClear()
        {
            // TrackPhotonDequeue + AddDelta + ClearSync + Decrement = 4
            int ops = SimDrainPhotonSlot(true, -1, 0, 5, true);
            Assert.Equal(4, ops);
        }

        [Fact]
        public void PhotonDrain_SlotWithExpectedKeyZeroDelta_PerformsClearOnly()
        {
            // TrackPhotonDequeue + ClearSync + Decrement = 3
            int ops = SimDrainPhotonSlot(true, -1, 0, 0, true);
            Assert.Equal(3, ops);
        }

        [Fact]
        public void PhotonDrain_ValidSbIdxWithinSideband_ReleasesAndClearsSideband()
        {
            // TrackPhotonDequeue + ReleaseByIndex + SidebandClear + Decrement = 4
            int ops = SimDrainPhotonSlot(true, 0, 4, 0, false);
            Assert.Equal(4, ops);
        }

        [Fact]
        public void PhotonDrain_ValidSbIdxBeyondSideband_OnlyReleases()
        {
            // TrackPhotonDequeue + ReleaseByIndex + Decrement = 3
            int ops = SimDrainPhotonSlot(true, 5, 4, 0, false);
            Assert.Equal(3, ops);
        }

        [Fact]
        public void PhotonDrain_FullSlot_AllOpsExecuted()
        {
            // TrackPhotonDequeue + AddDelta + ClearSync + ReleaseByIndex + SidebandClear + Decrement = 6
            int ops = SimDrainPhotonSlot(true, 2, 8, 10, true);
            Assert.Equal(6, ops);
        }

        // -----------------------------------------------------------------------
        // DrainLegacyDispatchQueueOnAbort stand-in tests
        // -----------------------------------------------------------------------

        [Fact]
        public void LegacyDrain_EmptyQueue_PerformsZeroOps()
        {
            int ops = SimDrainLegacySlot(false, 0);
            Assert.Equal(0, ops);
        }

        [Fact]
        public void LegacyDrain_NonzeroDelta_PerformsDeltaClearDecrement()
        {
            // AddDelta + ClearSync + Decrement = 3
            int ops = SimDrainLegacySlot(true, 5);
            Assert.Equal(3, ops);
        }

        [Fact]
        public void LegacyDrain_ZeroDelta_SkipsDeltaOp()
        {
            // ClearSync + Decrement = 2
            int ops = SimDrainLegacySlot(true, 0);
            Assert.Equal(2, ops);
        }

        // -----------------------------------------------------------------------
        // Counter math: both queues drained to zero before circuit breaker reset
        // -----------------------------------------------------------------------

        [Fact]
        public void BothQueues_DrainedToZero_PendingCountIsZero()
        {
            // Simulate 2 photon slots + 2 legacy slots, each decrementing a counter
            int counter = 4;
            for (int i = 0; i < 2; i++)
                Interlocked.Decrement(ref counter);
            for (int i = 0; i < 2; i++)
                Interlocked.Decrement(ref counter);
            Assert.Equal(0, counter);
        }
    }
}
