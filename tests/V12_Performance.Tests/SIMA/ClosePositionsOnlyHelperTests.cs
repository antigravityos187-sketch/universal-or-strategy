using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Xunit;

namespace V12_Performance.Tests.SIMA
{
    /// <summary>
    /// Unit tests for W7-100 extracted helpers:
    /// EnqueueFleetAccountFlattenOps, EnqueueMasterAccountFallbackFlatten, TriggerOrFallbackFlattenExecution.
    /// Mirror-based tests -- no NinjaTrader assembly dependency.
    /// EPIC-W7-100 T1 + T2 + T3 TDD Safety Net.
    /// </summary>
    public class ClosePositionsOnlyHelperTests
    {
        // ---------------------------------------------------------------------------
        // Mirror types -- mirror NinjaTrader types without assembly dependency
        // ---------------------------------------------------------------------------

        private sealed class FlattenWorkItem
        {
            public string AccountName { get; set; } = "";
            public bool CancelOnly { get; set; }
            public bool ZombieSweepOnly { get; set; }
            public bool IsMaster { get; set; }
            public string Source { get; set; } = "";
        }

        private sealed class FakeAccount
        {
            public string Name { get; set; } = "";
            public bool IsFleet { get; set; }
            public int PositionCount { get; set; }
        }

        // ---------------------------------------------------------------------------
        // Mirror of EnqueueFleetAccountFlattenOps logic
        // ---------------------------------------------------------------------------

        private static int EnqueueFleetAccountFlattenOps(
            FakeAccount[] snapshot,
            ConcurrentQueue<FlattenWorkItem> queue
        )
        {
            int enqueued = 0;
            foreach (FakeAccount acct in snapshot)
            {
                if (!acct.IsFleet)
                    continue;
                queue.Enqueue(
                    new FlattenWorkItem
                    {
                        AccountName = acct.Name,
                        CancelOnly = false,
                        ZombieSweepOnly = true,
                        IsMaster = false,
                        Source = "ClosePositionsOnly",
                    }
                );
                enqueued++;
            }
            return enqueued;
        }

        // ---------------------------------------------------------------------------
        // Mirror of EnqueueMasterAccountFallbackFlatten logic
        // ---------------------------------------------------------------------------

        private static int EnqueueMasterAccountFallbackFlatten(
            bool masterCovered,
            int masterPositionCount,
            string masterAccountName,
            ConcurrentQueue<FlattenWorkItem> queue
        )
        {
            int enqueued = 0;
            if (!masterCovered && masterPositionCount > 0)
            {
                queue.Enqueue(
                    new FlattenWorkItem
                    {
                        AccountName = masterAccountName,
                        CancelOnly = false,
                        ZombieSweepOnly = true,
                        IsMaster = true,
                        Source = "ClosePositionsOnly_Master",
                    }
                );
                enqueued++;
            }
            return enqueued;
        }

        // ---------------------------------------------------------------------------
        // Mirror of TriggerOrFallbackFlattenExecution decision logic
        // Returns "triggered", "fallback_invalid", "fallback_general", or "no_ops"
        // ---------------------------------------------------------------------------

        private static string TriggerOrFallbackDecision(
            bool queueIsEmpty,
            bool throwInvalid,
            bool throwGeneral
        )
        {
            if (!queueIsEmpty)
            {
                if (throwInvalid)
                    return "fallback_invalid";
                if (throwGeneral)
                    return "fallback_general";
                return "triggered";
            }
            return "no_ops";
        }

        // ===========================================================================
        // T1 tests: EnqueueFleetAccountFlattenOps
        // ===========================================================================

        [Fact]
        public void EnqueueFleetAccountFlattenOps_OnlyFleetAccounts_AreEnqueued()
        {
            var queue = new ConcurrentQueue<FlattenWorkItem>();
            var snapshot = new[]
            {
                new FakeAccount { Name = "Fleet1", IsFleet = true },
                new FakeAccount { Name = "NonFleet", IsFleet = false },
                new FakeAccount { Name = "Fleet2", IsFleet = true },
            };

            int enqueued = EnqueueFleetAccountFlattenOps(snapshot, queue);

            Assert.Equal(2, enqueued);
            Assert.Equal(2, queue.Count);
        }

        [Fact]
        public void EnqueueFleetAccountFlattenOps_NoFleetAccounts_EnqueuesZero()
        {
            var queue = new ConcurrentQueue<FlattenWorkItem>();
            var snapshot = new[]
            {
                new FakeAccount { Name = "NonFleet1", IsFleet = false },
                new FakeAccount { Name = "NonFleet2", IsFleet = false },
            };

            int enqueued = EnqueueFleetAccountFlattenOps(snapshot, queue);

            Assert.Equal(0, enqueued);
            Assert.Equal(0, queue.Count);
        }

        [Fact]
        public void EnqueueFleetAccountFlattenOps_EmptySnapshot_EnqueuesZero()
        {
            var queue = new ConcurrentQueue<FlattenWorkItem>();
            var snapshot = Array.Empty<FakeAccount>();

            int enqueued = EnqueueFleetAccountFlattenOps(snapshot, queue);

            Assert.Equal(0, enqueued);
        }

        [Fact]
        public void EnqueueFleetAccountFlattenOps_EnqueuedItems_HaveZombieSweepOnlyTrue()
        {
            var queue = new ConcurrentQueue<FlattenWorkItem>();
            var snapshot = new[] { new FakeAccount { Name = "Fleet1", IsFleet = true } };

            EnqueueFleetAccountFlattenOps(snapshot, queue);

            queue.TryDequeue(out var item);
            Assert.Equal(true, item!.ZombieSweepOnly);
        }

        [Fact]
        public void EnqueueFleetAccountFlattenOps_EnqueuedItems_HaveIsMasterFalse()
        {
            var queue = new ConcurrentQueue<FlattenWorkItem>();
            var snapshot = new[] { new FakeAccount { Name = "Fleet1", IsFleet = true } };

            EnqueueFleetAccountFlattenOps(snapshot, queue);

            queue.TryDequeue(out var item);
            Assert.Equal(false, item!.IsMaster);
        }

        [Fact]
        public void EnqueueFleetAccountFlattenOps_EnqueuedItems_HaveCancelOnlyFalse()
        {
            var queue = new ConcurrentQueue<FlattenWorkItem>();
            var snapshot = new[] { new FakeAccount { Name = "Fleet1", IsFleet = true } };

            EnqueueFleetAccountFlattenOps(snapshot, queue);

            queue.TryDequeue(out var item);
            Assert.Equal(false, item!.CancelOnly);
        }

        // ===========================================================================
        // T2 tests: EnqueueMasterAccountFallbackFlatten
        // ===========================================================================

        [Fact]
        public void EnqueueMasterAccountFallbackFlatten_NotCoveredWithPositions_EnqueuesOne()
        {
            var queue = new ConcurrentQueue<FlattenWorkItem>();

            int enqueued = EnqueueMasterAccountFallbackFlatten(
                masterCovered: false,
                masterPositionCount: 1,
                masterAccountName: "MasterAcct",
                queue
            );

            Assert.Equal(1, enqueued);
            Assert.Equal(1, queue.Count);
        }

        [Fact]
        public void EnqueueMasterAccountFallbackFlatten_MasterCovered_DoesNotEnqueue()
        {
            var queue = new ConcurrentQueue<FlattenWorkItem>();

            int enqueued = EnqueueMasterAccountFallbackFlatten(
                masterCovered: true,
                masterPositionCount: 3,
                masterAccountName: "MasterAcct",
                queue
            );

            Assert.Equal(0, enqueued);
            Assert.Equal(0, queue.Count);
        }

        [Fact]
        public void EnqueueMasterAccountFallbackFlatten_NotCoveredNoPositions_DoesNotEnqueue()
        {
            var queue = new ConcurrentQueue<FlattenWorkItem>();

            int enqueued = EnqueueMasterAccountFallbackFlatten(
                masterCovered: false,
                masterPositionCount: 0,
                masterAccountName: "MasterAcct",
                queue
            );

            Assert.Equal(0, enqueued);
            Assert.Equal(0, queue.Count);
        }

        [Fact]
        public void EnqueueMasterAccountFallbackFlatten_EnqueuedItem_HasIsMasterTrue()
        {
            var queue = new ConcurrentQueue<FlattenWorkItem>();

            EnqueueMasterAccountFallbackFlatten(
                masterCovered: false,
                masterPositionCount: 1,
                masterAccountName: "MasterAcct",
                queue
            );

            queue.TryDequeue(out var item);
            Assert.Equal(true, item!.IsMaster);
        }

        [Fact]
        public void EnqueueMasterAccountFallbackFlatten_EnqueuedItem_HasZombieSweepOnlyTrue()
        {
            var queue = new ConcurrentQueue<FlattenWorkItem>();

            EnqueueMasterAccountFallbackFlatten(
                masterCovered: false,
                masterPositionCount: 1,
                masterAccountName: "MasterAcct",
                queue
            );

            queue.TryDequeue(out var item);
            Assert.Equal(true, item!.ZombieSweepOnly);
        }

        // ===========================================================================
        // T3 tests: TriggerOrFallbackFlattenExecution decision paths
        // ===========================================================================

        [Fact]
        public void TriggerOrFallbackDecision_QueueNotEmpty_NormalPath_ReturnsTriggered()
        {
            string result = TriggerOrFallbackDecision(
                queueIsEmpty: false,
                throwInvalid: false,
                throwGeneral: false
            );
            Assert.Equal("triggered", result);
        }

        [Fact]
        public void TriggerOrFallbackDecision_QueueNotEmpty_InvalidOperationException_ReturnsFallbackInvalid()
        {
            string result = TriggerOrFallbackDecision(
                queueIsEmpty: false,
                throwInvalid: true,
                throwGeneral: false
            );
            Assert.Equal("fallback_invalid", result);
        }

        [Fact]
        public void TriggerOrFallbackDecision_QueueNotEmpty_GeneralException_ReturnsFallbackGeneral()
        {
            string result = TriggerOrFallbackDecision(
                queueIsEmpty: false,
                throwInvalid: false,
                throwGeneral: true
            );
            Assert.Equal("fallback_general", result);
        }

        [Fact]
        public void TriggerOrFallbackDecision_QueueEmpty_ReturnsNoOps()
        {
            string result = TriggerOrFallbackDecision(
                queueIsEmpty: true,
                throwInvalid: false,
                throwGeneral: false
            );
            Assert.Equal("no_ops", result);
        }
    }
}
