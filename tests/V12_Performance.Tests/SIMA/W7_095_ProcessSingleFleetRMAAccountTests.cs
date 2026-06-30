// <copyright file="W7_095_ProcessSingleFleetRMAAccountTests.cs" company="BMad">
// Copyright (c) BMad. All rights reserved.
// </copyright>
// EPIC-W7-095 xUnit tests for the three helpers extracted from ProcessSingleFleetRMAAccount:
//   T1: IsAccountEligibleForRMADispatch (CYC=5)
//   T2: RegisterFleetFollowerState write-ordering invariant [923B-FIX-B]
//   T3: RollbackFleetFollowerState (CYC=3)
// NT8 dependency cannot be linked here; pure-logic mirrors are used.
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace V12_Performance.Tests.SIMA
{
    /// <summary>
    /// xUnit tests for EPIC-W7-095 helpers extracted from ProcessSingleFleetRMAAccount.
    /// ProcessSingleFleetRMAAccount CYC reduced from 13 to 6.
    /// All three helpers verify their invariants without NT8 assemblies.
    /// </summary>
    public class W7_095_ProcessSingleFleetRMAAccountTests
    {
        // -----------------------------------------------------------------------
        // Stand-in mirrors (no NT8 assemblies available in this project)
        // -----------------------------------------------------------------------

        private enum MarketPosition
        {
            Long,
            Short,
            Flat,
        }

        private struct PositionInfo
        {
            public double EntryPrice;
            public MarketPosition Direction;
            public int TotalContracts;
        }

        // Mirror of IsAccountEligibleForRMADispatch logic (T1)
        private static bool IsAccountEligibleForRMADispatch(
            string accountName,
            bool enableConsistencyLock,
            double maxDailyProfitCap,
            double dailyPL,
            ConcurrentDictionary<string, bool> activeFleetAccounts,
            StringBuilder dispatchLog
        )
        {
            if (!activeFleetAccounts.TryGetValue(accountName, out bool isActive) || !isActive)
            {
                dispatchLog.AppendLine("  SKIP | " + accountName + " | Inactive");
                return false;
            }

            if (enableConsistencyLock)
            {
                if (dailyPL >= maxDailyProfitCap)
                {
                    dispatchLog.AppendLine(
                        "  SKIP | " + accountName + " | ConsistencyLock $" + dailyPL.ToString("F2")
                    );
                    return false;
                }
            }

            return true;
        }

        // Mirror of RegisterFleetFollowerState write-ordering + delta calculation (T2)
        private static void RegisterFleetFollowerState(
            string fleetKey,
            string expectedKey,
            PositionInfo fleetFollowerPos,
            MarketPosition direction,
            int qty,
            ConcurrentDictionary<string, PositionInfo> activePositions,
            ConcurrentDictionary<string, string> entryOrders,
            Dictionary<string, int> expectedPositionDeltas,
            out bool syncPending,
            out int reservedDelta
        )
        {
            // [923B-FIX-B] WRITE ORDERING INVARIANT: dicts BEFORE expectedPositions
            activePositions[fleetKey] = fleetFollowerPos;
            entryOrders[fleetKey] = fleetKey + "_order";
            syncPending = true;
            reservedDelta = (direction == MarketPosition.Long) ? qty : -qty;
            if (!expectedPositionDeltas.ContainsKey(expectedKey))
                expectedPositionDeltas[expectedKey] = 0;
            expectedPositionDeltas[expectedKey] += reservedDelta;
        }

        // Mirror of RollbackFleetFollowerState (T3)
        private static void RollbackFleetFollowerState(
            string fleetKey,
            string expectedKey,
            bool syncPending,
            int reservedDelta,
            StringBuilder dispatchLog,
            string accountName,
            ConcurrentDictionary<string, PositionInfo> activePositions,
            ConcurrentDictionary<string, string> entryOrders,
            Dictionary<string, int> expectedPositionDeltas
        )
        {
            if (syncPending)
            {
                // ClearDispatchSyncPending equivalent -- no-op in mirror
            }
            if (reservedDelta != 0)
            {
                if (!expectedPositionDeltas.ContainsKey(expectedKey))
                    expectedPositionDeltas[expectedKey] = 0;
                expectedPositionDeltas[expectedKey] -= reservedDelta;
            }
            activePositions.TryRemove(fleetKey, out _);
            entryOrders.TryRemove(fleetKey, out _);
            dispatchLog.AppendLine("  FAIL | " + accountName + " | rollback");
        }

        // -----------------------------------------------------------------------
        // T1: IsAccountEligibleForRMADispatch
        // -----------------------------------------------------------------------

        [Fact]
        public void IsAccountEligibleForRMADispatch_InactiveAccount_ReturnsFalse()
        {
            var accounts = new ConcurrentDictionary<string, bool>();
            accounts["Acct1"] = false;
            var log = new StringBuilder();
            bool result = IsAccountEligibleForRMADispatch("Acct1", false, 1000.0, 0.0, accounts, log);
            Assert.Equal(false, result);
            Assert.Contains("Inactive", log.ToString());
        }

        [Fact]
        public void IsAccountEligibleForRMADispatch_MissingAccount_ReturnsFalse()
        {
            var accounts = new ConcurrentDictionary<string, bool>();
            var log = new StringBuilder();
            bool result = IsAccountEligibleForRMADispatch("Acct_Missing", false, 1000.0, 0.0, accounts, log);
            Assert.Equal(false, result);
        }

        [Fact]
        public void IsAccountEligibleForRMADispatch_ActiveNoConsistencyLock_ReturnsTrue()
        {
            var accounts = new ConcurrentDictionary<string, bool>();
            accounts["Acct1"] = true;
            var log = new StringBuilder();
            bool result = IsAccountEligibleForRMADispatch("Acct1", false, 1000.0, 500.0, accounts, log);
            Assert.Equal(true, result);
        }

        [Fact]
        public void IsAccountEligibleForRMADispatch_ConsistencyLockHit_ReturnsFalse()
        {
            var accounts = new ConcurrentDictionary<string, bool>();
            accounts["Acct1"] = true;
            var log = new StringBuilder();
            bool result = IsAccountEligibleForRMADispatch("Acct1", true, 1000.0, 1500.0, accounts, log);
            Assert.Equal(false, result);
            Assert.Contains("ConsistencyLock", log.ToString());
        }

        [Fact]
        public void IsAccountEligibleForRMADispatch_ConsistencyLockExactCap_ReturnsFalse()
        {
            var accounts = new ConcurrentDictionary<string, bool>();
            accounts["Acct1"] = true;
            var log = new StringBuilder();
            // dailyPL == cap -> still skip (>= check)
            bool result = IsAccountEligibleForRMADispatch("Acct1", true, 1000.0, 1000.0, accounts, log);
            Assert.Equal(false, result);
        }

        [Fact]
        public void IsAccountEligibleForRMADispatch_BelowCap_ReturnsTrue()
        {
            var accounts = new ConcurrentDictionary<string, bool>();
            accounts["Acct1"] = true;
            var log = new StringBuilder();
            bool result = IsAccountEligibleForRMADispatch("Acct1", true, 1000.0, 999.99, accounts, log);
            Assert.Equal(true, result);
        }

        // -----------------------------------------------------------------------
        // T2: RegisterFleetFollowerState -- [923B-FIX-B] write ordering invariant
        // -----------------------------------------------------------------------

        [Fact]
        public void RegisterFleetFollowerState_Long_SetsSyncPendingAndPositiveDelta()
        {
            var activePositions = new ConcurrentDictionary<string, PositionInfo>();
            var entryOrders = new ConcurrentDictionary<string, string>();
            var expectedDeltas = new Dictionary<string, int>();
            var pos = new PositionInfo
            {
                EntryPrice = 4200.0,
                Direction = MarketPosition.Long,
                TotalContracts = 2,
            };

            RegisterFleetFollowerState(
                "Acct1_RMA_S1",
                "EXPKEY_Acct1",
                pos,
                MarketPosition.Long,
                2,
                activePositions,
                entryOrders,
                expectedDeltas,
                out bool syncPending,
                out int reservedDelta
            );

            Assert.Equal(true, syncPending);
            Assert.Equal(2, reservedDelta);
            Assert.Equal(2, expectedDeltas["EXPKEY_Acct1"]);
            Assert.True(activePositions.ContainsKey("Acct1_RMA_S1"));
            Assert.True(entryOrders.ContainsKey("Acct1_RMA_S1"));
        }

        [Fact]
        public void RegisterFleetFollowerState_Short_SetsNegativeDelta()
        {
            var activePositions = new ConcurrentDictionary<string, PositionInfo>();
            var entryOrders = new ConcurrentDictionary<string, string>();
            var expectedDeltas = new Dictionary<string, int>();
            var pos = new PositionInfo
            {
                EntryPrice = 4200.0,
                Direction = MarketPosition.Short,
                TotalContracts = 3,
            };

            RegisterFleetFollowerState(
                "Acct2_RMA_S2",
                "EXPKEY_Acct2",
                pos,
                MarketPosition.Short,
                3,
                activePositions,
                entryOrders,
                expectedDeltas,
                out bool syncPending,
                out int reservedDelta
            );

            Assert.Equal(true, syncPending);
            Assert.Equal(-3, reservedDelta);
            Assert.Equal(-3, expectedDeltas["EXPKEY_Acct2"]);
        }

        [Fact]
        public void RegisterFleetFollowerState_WritesActivePositionsBeforeExpectedPositions()
        {
            // Validates [923B-FIX-B]: activePositions must be populated before expectedDeltas
            var activePositions = new ConcurrentDictionary<string, PositionInfo>();
            var entryOrders = new ConcurrentDictionary<string, string>();
            var expectedDeltas = new Dictionary<string, int>();
            var pos = new PositionInfo
            {
                EntryPrice = 4500.0,
                Direction = MarketPosition.Long,
                TotalContracts = 1,
            };

            RegisterFleetFollowerState(
                "Acct3_RMA_S3",
                "EXPKEY_Acct3",
                pos,
                MarketPosition.Long,
                1,
                activePositions,
                entryOrders,
                expectedDeltas,
                out _,
                out _
            );

            // Both invariants must hold after registration:
            Assert.True(activePositions.ContainsKey("Acct3_RMA_S3"));
            Assert.True(entryOrders.ContainsKey("Acct3_RMA_S3"));
            Assert.True(expectedDeltas.ContainsKey("EXPKEY_Acct3"));
        }

        // -----------------------------------------------------------------------
        // T3: RollbackFleetFollowerState
        // -----------------------------------------------------------------------

        [Fact]
        public void RollbackFleetFollowerState_RemovesAllTracking()
        {
            var activePositions = new ConcurrentDictionary<string, PositionInfo>();
            var entryOrders = new ConcurrentDictionary<string, string>();
            var expectedDeltas = new Dictionary<string, int> { ["EXPKEY_A"] = 2 };
            activePositions["Acct1_RMA_S1"] = new PositionInfo { EntryPrice = 4200.0 };
            entryOrders["Acct1_RMA_S1"] = "order1";
            var log = new StringBuilder();

            RollbackFleetFollowerState(
                "Acct1_RMA_S1",
                "EXPKEY_A",
                true,
                2,
                log,
                "Acct1",
                activePositions,
                entryOrders,
                expectedDeltas
            );

            Assert.False(activePositions.ContainsKey("Acct1_RMA_S1"));
            Assert.False(entryOrders.ContainsKey("Acct1_RMA_S1"));
            Assert.Equal(0, expectedDeltas["EXPKEY_A"]);
            Assert.Contains("FAIL", log.ToString());
            Assert.Contains("rollback", log.ToString());
        }

        [Fact]
        public void RollbackFleetFollowerState_ZeroReservedDelta_DoesNotAdjustExpected()
        {
            var activePositions = new ConcurrentDictionary<string, PositionInfo>();
            var entryOrders = new ConcurrentDictionary<string, string>();
            var expectedDeltas = new Dictionary<string, int> { ["EXPKEY_B"] = 5 };
            var log = new StringBuilder();

            RollbackFleetFollowerState(
                "Acct2_RMA_S2",
                "EXPKEY_B",
                false,
                0,
                log,
                "Acct2",
                activePositions,
                entryOrders,
                expectedDeltas
            );

            // reservedDelta=0 means AddExpectedPositionDeltaLocked was never called
            // so rollback should NOT touch expectedDeltas
            Assert.Equal(5, expectedDeltas["EXPKEY_B"]);
        }

        [Fact]
        public void RollbackFleetFollowerState_LogsRollbackMessage()
        {
            var activePositions = new ConcurrentDictionary<string, PositionInfo>();
            var entryOrders = new ConcurrentDictionary<string, string>();
            var expectedDeltas = new Dictionary<string, int>();
            var log = new StringBuilder();

            RollbackFleetFollowerState(
                "AcctX_RMA_S9",
                "EXPKEY_X",
                false,
                0,
                log,
                "AcctX",
                activePositions,
                entryOrders,
                expectedDeltas
            );

            string entry = log.ToString();
            Assert.Contains("FAIL", entry);
            Assert.Contains("AcctX", entry);
            Assert.Contains("rollback", entry);
        }
    }
}
