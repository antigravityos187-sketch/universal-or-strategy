// xUnit tests for SubmitAndRegisterFleetOrders extracted helpers (EPIC-W7-061 / EPIC-W7-104)
// KB: [Fact] + Assert.Equal only -- NUnit/MSTest banned
// T1: UpdateFleetFsmState -- FSM state transition from PendingSubmit to Submitted (CYC=3)
// T2: RegisterOrderIdsToFsmKey -- order ID to FSM key registration loop (CYC=3)
// NT8 dependency cannot be linked here; pure-logic mirrors are used.

using System.Collections.Concurrent;
using Xunit;

namespace V12Tests.SIMA.Fleet
{
    /// <summary>
    /// xUnit tests for EPIC-W7-061 / EPIC-W7-104 extracted helpers.
    /// T1: UpdateFleetFsmState -- PendingSubmit -> Submitted transition.
    /// T2: RegisterOrderIdsToFsmKey -- maps order IDs to fleet entry name.
    /// </summary>
    public class W7_061_SubmitAndRegisterTests
    {
        // -----------------------------------------------------------------------
        // Stand-in enum mirror (FollowerBracketState defined in NT8 assembly)
        // -----------------------------------------------------------------------

        private enum FollowerBracketState
        {
            Inactive,
            PendingSubmit,
            Submitted,
            Active,
            Cancelling,
        }

        // -----------------------------------------------------------------------
        // Minimal FSM mirror (FollowerBracketFSM stub)
        // -----------------------------------------------------------------------

        private sealed class FollowerBracketFSM
        {
            public FollowerBracketState State { get; set; }
        }

        // -----------------------------------------------------------------------
        // Minimal Order mirror (NinjaTrader.Cbi.Order stub)
        // -----------------------------------------------------------------------

        private sealed class Order
        {
            public string OrderId { get; set; }
        }

        // -----------------------------------------------------------------------
        // Mirror of UpdateFleetFsmState (T1) -- CYC=3
        // Exact structural mirror of the extracted private method.
        // -----------------------------------------------------------------------

        private static void UpdateFleetFsmState(
            string fleetEntryName,
            ConcurrentDictionary<string, FollowerBracketFSM> followerBrackets
        )
        {
            FollowerBracketFSM pFsm;
            if (
                followerBrackets.TryGetValue(fleetEntryName, out pFsm)
                && pFsm != null
                && pFsm.State == FollowerBracketState.PendingSubmit
            )
            {
                pFsm.State = FollowerBracketState.Submitted;
            }
        }

        // -----------------------------------------------------------------------
        // Mirror of RegisterOrderIdsToFsmKey (T2) -- CYC=3
        // Exact structural mirror of the extracted private method.
        // -----------------------------------------------------------------------

        private static void RegisterOrderIdsToFsmKey(
            string fleetEntryName,
            Order[] orders,
            int orderCount,
            ConcurrentDictionary<string, FollowerBracketFSM> followerBrackets,
            ConcurrentDictionary<string, string> orderIdToFsmKey
        )
        {
            FollowerBracketFSM fsm;
            if (followerBrackets.TryGetValue(fleetEntryName, out fsm))
            {
                for (int i = 0; i < orderCount; i++)
                {
                    var ord = orders[i];
                    if (ord != null && !string.IsNullOrEmpty(ord.OrderId))
                        orderIdToFsmKey[ord.OrderId] = fleetEntryName;
                }
            }
        }

        // -----------------------------------------------------------------------
        // T1 tests: UpdateFleetFsmState
        // -----------------------------------------------------------------------

        [Fact]
        public void UpdateFleetFsmState_PendingSubmit_TransitionsToSubmitted()
        {
            var fsms = new ConcurrentDictionary<string, FollowerBracketFSM>();
            var fsm = new FollowerBracketFSM { State = FollowerBracketState.PendingSubmit };
            fsms["fleet1"] = fsm;

            UpdateFleetFsmState("fleet1", fsms);

            Assert.Equal(FollowerBracketState.Submitted, fsm.State);
        }

        [Fact]
        public void UpdateFleetFsmState_AlreadySubmitted_NoChange()
        {
            var fsms = new ConcurrentDictionary<string, FollowerBracketFSM>();
            var fsm = new FollowerBracketFSM { State = FollowerBracketState.Submitted };
            fsms["fleet1"] = fsm;

            UpdateFleetFsmState("fleet1", fsms);

            Assert.Equal(FollowerBracketState.Submitted, fsm.State);
        }

        [Fact]
        public void UpdateFleetFsmState_ActiveState_NoChange()
        {
            var fsms = new ConcurrentDictionary<string, FollowerBracketFSM>();
            var fsm = new FollowerBracketFSM { State = FollowerBracketState.Active };
            fsms["fleet1"] = fsm;

            UpdateFleetFsmState("fleet1", fsms);

            Assert.Equal(FollowerBracketState.Active, fsm.State);
        }

        [Fact]
        public void UpdateFleetFsmState_KeyMissing_DoesNotThrow()
        {
            var fsms = new ConcurrentDictionary<string, FollowerBracketFSM>();

            UpdateFleetFsmState("nonexistent", fsms);

            Assert.Equal(0, fsms.Count);
        }

        [Fact]
        public void UpdateFleetFsmState_NullFsmValue_GuardPreventsNullRef()
        {
            var fsms = new ConcurrentDictionary<string, FollowerBracketFSM>();
            fsms["fleet1"] = null;

            // pFsm != null guard prevents NullReferenceException
            UpdateFleetFsmState("fleet1", fsms);

            Assert.Equal(true, fsms.ContainsKey("fleet1"));
        }

        // -----------------------------------------------------------------------
        // T2 tests: RegisterOrderIdsToFsmKey
        // -----------------------------------------------------------------------

        [Fact]
        public void RegisterOrderIdsToFsmKey_ValidOrders_MapsAllIds()
        {
            var fsms = new ConcurrentDictionary<string, FollowerBracketFSM>();
            fsms["fleet1"] = new FollowerBracketFSM { State = FollowerBracketState.PendingSubmit };
            var orderMap = new ConcurrentDictionary<string, string>();
            var orders = new Order[]
            {
                new Order { OrderId = "ORD-001" },
                new Order { OrderId = "ORD-002" },
            };

            RegisterOrderIdsToFsmKey("fleet1", orders, 2, fsms, orderMap);

            Assert.Equal("fleet1", orderMap["ORD-001"]);
            Assert.Equal("fleet1", orderMap["ORD-002"]);
        }

        [Fact]
        public void RegisterOrderIdsToFsmKey_OrderCountLessThanArray_OnlySubsetMapped()
        {
            var fsms = new ConcurrentDictionary<string, FollowerBracketFSM>();
            fsms["fleet1"] = new FollowerBracketFSM { State = FollowerBracketState.PendingSubmit };
            var orderMap = new ConcurrentDictionary<string, string>();
            var orders = new Order[]
            {
                new Order { OrderId = "ORD-001" },
                new Order { OrderId = "ORD-002" },
            };

            RegisterOrderIdsToFsmKey("fleet1", orders, 1, fsms, orderMap);

            Assert.Equal("fleet1", orderMap["ORD-001"]);
            Assert.Equal(false, orderMap.ContainsKey("ORD-002"));
        }

        [Fact]
        public void RegisterOrderIdsToFsmKey_NullOrderEntry_SkipsNull()
        {
            var fsms = new ConcurrentDictionary<string, FollowerBracketFSM>();
            fsms["fleet1"] = new FollowerBracketFSM { State = FollowerBracketState.PendingSubmit };
            var orderMap = new ConcurrentDictionary<string, string>();
            var orders = new Order[]
            {
                null,
                new Order { OrderId = "ORD-002" },
            };

            RegisterOrderIdsToFsmKey("fleet1", orders, 2, fsms, orderMap);

            Assert.Equal(1, orderMap.Count);
            Assert.Equal("fleet1", orderMap["ORD-002"]);
        }

        [Fact]
        public void RegisterOrderIdsToFsmKey_EmptyOrderId_SkipsEmpty()
        {
            var fsms = new ConcurrentDictionary<string, FollowerBracketFSM>();
            fsms["fleet1"] = new FollowerBracketFSM { State = FollowerBracketState.PendingSubmit };
            var orderMap = new ConcurrentDictionary<string, string>();
            var orders = new Order[]
            {
                new Order { OrderId = string.Empty },
                new Order { OrderId = "ORD-002" },
            };

            RegisterOrderIdsToFsmKey("fleet1", orders, 2, fsms, orderMap);

            Assert.Equal(1, orderMap.Count);
            Assert.Equal("fleet1", orderMap["ORD-002"]);
        }

        [Fact]
        public void RegisterOrderIdsToFsmKey_FsmKeyMissing_NoRegistration()
        {
            var fsms = new ConcurrentDictionary<string, FollowerBracketFSM>();
            var orderMap = new ConcurrentDictionary<string, string>();
            var orders = new Order[] { new Order { OrderId = "ORD-001" } };

            RegisterOrderIdsToFsmKey("missing_fleet", orders, 1, fsms, orderMap);

            Assert.Equal(0, orderMap.Count);
        }
    }
}
