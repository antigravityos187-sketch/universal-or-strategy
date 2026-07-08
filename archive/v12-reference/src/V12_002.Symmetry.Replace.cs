// Build 971: Symmetry.Replace -- SymmetryGuardRetargetExistingFollowerBracket, ReplaceExistingFollowerTarget, SkipFollower
// V12 Symmetry Module (Extracted)
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class V12_002 : Strategy
    {
        #region Symmetry Replace

        private void SymmetryGuardRetargetExistingFollowerBracket(string fleetEntryName, PositionInfo pos)
        {
            UpdateStopOrder(fleetEntryName, pos, pos.CurrentStopPrice, pos.CurrentTrailLevel);
            SymmetryGuardReplaceExistingFollowerTarget(fleetEntryName, pos, 1, target1Orders);
            SymmetryGuardReplaceExistingFollowerTarget(fleetEntryName, pos, 2, target2Orders);
            SymmetryGuardReplaceExistingFollowerTarget(fleetEntryName, pos, 3, target3Orders);
            SymmetryGuardReplaceExistingFollowerTarget(fleetEntryName, pos, 4, target4Orders);
            SymmetryGuardReplaceExistingFollowerTarget(fleetEntryName, pos, 5, target5Orders);
        }

        // W7-128-T1: IsOrderLive -- 4-way OrderState hot-path predicate (CYC=4)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsOrderLive(Order order) =>
            order.OrderState == OrderState.Working
            || order.OrderState == OrderState.Accepted
            || order.OrderState == OrderState.Submitted
            || order.OrderState == OrderState.ChangePending;

        // W7-128-T2: TryCancelStaleTarget -- stale entry cleanup cold path (CYC=6)
        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool TryCancelStaleTarget(
            string fleetEntryName,
            PositionInfo pos,
            int targetNumber,
            ConcurrentDictionary<string, Order> dict,
            bool isFilled,
            bool isRunner,
            int qty
        )
        {
            if (isFilled || isRunner || qty <= 0)
            {
                if (dict.TryGetValue(fleetEntryName, out var staleTarget) && staleTarget != null)
                {
                    if (IsOrderLive(staleTarget))
                        pos.ExecutingAccount.Cancel(new[] { staleTarget });
                    dict.TryRemove(fleetEntryName, out _);
                }
                return true;
            }
            return false;
        }

        // W7-128-T3: BuildFollowerTargetReplaceSpec -- spec construction cold path (CYC=3)
        [MethodImpl(MethodImplOptions.NoInlining)]
        private FollowerTargetReplaceSpec? BuildFollowerTargetReplaceSpec(
            string fleetEntryName,
            PositionInfo pos,
            int targetNumber,
            string targetTag,
            int qty
        )
        {
            double newPrice = GetTargetPrice(pos, targetNumber);
            if (newPrice <= 0)
                return null;
            OrderAction exitAction = pos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;
            return new FollowerTargetReplaceSpec
            {
                EntryName = fleetEntryName,
                TargetNum = targetNumber,
                NewTargetPrice = Instrument.MasterInstrument.RoundToTickSize(newPrice),
                Quantity = qty,
                ExitAction = exitAction,
                TargetAccount = pos.ExecutingAccount,
                CancellingOrderId = null,
            };
        }

        // W7-128-T4: SymmetryGuardReplaceExistingFollowerTarget -- 7-branch orchestrator (CYC=7)
        private void SymmetryGuardReplaceExistingFollowerTarget(
            string fleetEntryName,
            PositionInfo pos,
            int targetNumber,
            ConcurrentDictionary<string, Order> dict
        )
        {
            if (pos.ExecutingAccount == null)
                return;

            string targetTag = "T" + targetNumber;
            bool isRunner = IsRunnerTarget(targetNumber);
            bool isFilled = IsTargetFilled(pos, targetNumber);
            int qty = GetTargetContracts(pos, targetNumber);

            if (TryCancelStaleTarget(fleetEntryName, pos, targetNumber, dict, isFilled, isRunner, qty))
                return;

            if (!dict.TryGetValue(fleetEntryName, out var oldTarget) || oldTarget == null)
                return;

            if (!IsOrderLive(oldTarget))
                return;

            // Build 1004 [DNA-FIX]: Replace raw Cancel+stateLock-gated Submit with FollowerTargetReplaceSpec
            // two-phase FSM. Mirror pattern from Trailing.Breakeven.cs Build 957 C1.
            // Phase 1 (here): store spec and cancel only.
            // Phase 2 (automatic): AccountOrders.cs lines 352-382 detects cancel confirm by CancellingOrderId,
            // fires TriggerCustomEvent -> SubmitFollowerTargetReplacement() in Propagation.cs.
            string signalName = SymmetryTrim(targetTag + "_" + fleetEntryName, 40);
            var tSpec = BuildFollowerTargetReplaceSpec(fleetEntryName, pos, targetNumber, targetTag, qty);
            if (tSpec == null)
                return;

            _followerTargetReplaceSpecs[signalName] = tSpec;
            // A1-2: Stamp REAPER grace window before cancel to suppress false desync during replace gap.
            StampReaperMoveGrace();
            pos.ExecutingAccount.Cancel(new[] { oldTarget });
        }

        private void SymmetryGuardSkipFollower(
            string fleetEntryName,
            PositionInfo pos,
            double fleetFillPrice,
            double slippageTicks,
            double slippageUsdPerContract,
            string reason
        )
        {
            Print(
                string.Format(
                    "[SYMMETRY_GUARD] SKIP | {0} | {1} | FleetFill={2:F2} | Slip={3:F1} ticks (${4:F2}/ct)",
                    fleetEntryName,
                    reason,
                    fleetFillPrice,
                    slippageTicks,
                    slippageUsdPerContract
                )
            );

            // Build 1004 [DNA-FIX]: Replace the old stateLock path with Enqueue actor write (no internal locks).
            // TotalContracts snapshot captured before lambda to prevent closure mutation.
            int _skipContractsSnap = pos.TotalContracts;
            Enqueue(ctx =>
            {
                pos.EntryFilled = true;
                if (pos.RemainingContracts <= 0)
                    pos.RemainingContracts = Math.Max(1, _skipContractsSnap);
            });

            FlattenPositionByName(fleetEntryName);
            CleanupPosition(fleetEntryName);
            SymmetryGuardForgetEntry(fleetEntryName);
        }

        // W7-002-T1: Build follower worklist from ADR-019 immutable snapshot (CYC=7)
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SymmetryGuardBuildFollowerWorklist_FromSnapshot(string dispatchId, List<string> worklist)
        {
            if (!symmetryDispatchById.TryGetValue(dispatchId, out var ctx) || ctx == null)
                return;

            // ADR-019: ctx.Followers is an immutable string[] snapshot published via Interlocked.CompareExchange.
            // Build follower worklist from the snapshot -- zero-alloc, lock-free.
            string[] followerSnapshot = ctx.Followers;
            foreach (string fleetEntryName in followerSnapshot)
            {
                if (string.IsNullOrEmpty(fleetEntryName))
                    continue;
                if (!symmetryFleetEntryToDispatch.TryGetValue(fleetEntryName, out var linkedDispatch))
                    continue;
                if (!string.Equals(linkedDispatch, dispatchId, StringComparison.Ordinal))
                    continue;
                if (!symmetryPendingFollowerFills.ContainsKey(fleetEntryName))
                    continue;

                worklist.Add(fleetEntryName);
            }
        }

        // W7-002-T2: Legacy fallback scan for followers missed by snapshot (CYC=5)
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SymmetryGuardBuildFollowerWorklist_FromLegacyScan(string dispatchId, List<string> worklist)
        {
            // ADR-019: Preserve the legacy dispatch-map scan to catch followers missing from the local snapshot.
            foreach (var kvp in symmetryPendingFollowerFills.ToArray())
            {
                string fleetEntryName = kvp.Key;
                if (!symmetryFleetEntryToDispatch.TryGetValue(fleetEntryName, out var linkedDispatch))
                    continue;
                if (!string.Equals(linkedDispatch, dispatchId, StringComparison.Ordinal))
                    continue;
                if (worklist.Contains(fleetEntryName))
                    continue;

                worklist.Add(fleetEntryName);
            }
        }

        // W7-002-T3: Resolve a single follower entry end-to-end (CYC=5)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SymmetryGuardResolveFollowerEntry(string fleetEntryName, DateTime nowUtc)
        {
            if (!symmetryPendingFollowerFills.TryGetValue(fleetEntryName, out var pending))
                return;

            // V12.Phase8 [F-04]: Guard activePositions read with stateLock to prevent
            // torn observations concurrent with ExecuteSmartDispatchEntry commits/removals.
            activePositions.TryGetValue(fleetEntryName, out PositionInfo pos);
            if (pos != null && pos.IsFollower)
            {
                if (SymmetryGuardTryResolveFollower(fleetEntryName, pos, pending, nowUtc))
                    symmetryPendingFollowerFills.TryRemove(fleetEntryName, out _);
            }
        }

        // W7-002 parent: SymmetryGuardTryResolveFollowersForDispatch orchestrator (CYC=4)
        private void SymmetryGuardTryResolveFollowersForDispatch(string dispatchId, DateTime nowUtc)
        {
            if (string.IsNullOrEmpty(dispatchId))
                return;

            var followersToResolve = new List<string>();

            SymmetryGuardBuildFollowerWorklist_FromSnapshot(dispatchId, followersToResolve);
            SymmetryGuardBuildFollowerWorklist_FromLegacyScan(dispatchId, followersToResolve);

            foreach (string fleetEntryName in followersToResolve)
                SymmetryGuardResolveFollowerEntry(fleetEntryName, nowUtc);
        }

        // W7-044-T1 / W7-121: IsFollowerEntryLive -- OrderState predicate (CYC=4)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsFollowerEntryLive(Order order) =>
            order.OrderState == OrderState.Working
            || order.OrderState == OrderState.Submitted
            || order.OrderState == OrderState.Accepted;

        // W7-044-T2: TryResolveCascadeContext -- double dict resolution (CYC=3)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryResolveCascadeContext(string masterEntryName, out string[] followers)
        {
            followers = Array.Empty<string>();
            if (!symmetryMasterEntryToDispatch.TryGetValue(masterEntryName, out string dispatchId))
                return false;
            if (!symmetryDispatchById.TryGetValue(dispatchId, out var ctx))
                return false;
            followers = ctx.Followers; // ADR-019: immutable snapshot, lock-free read
            return true;
        }

        // W7-044-T3 / W7-121-003: TryCancelFollowerEntry -- per-follower guard chain + cancel (CYC=7)
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void TryCancelFollowerEntry(string followerName)
        {
            if (!activePositions.TryGetValue(followerName, out var pos))
                return;
            if (!entryOrders.TryGetValue(followerName, out var order))
                return;
            if (order == null)
                return;
            // DeltaExpectedPositionLocked deferred to OnAccountOrderUpdate
            // confirmed-cancel to prevent REAPER desync
            if (!IsFollowerEntryLive(order))
                return;

            Print(
                string.Format(
                    "[CASCADE] Cancelling follower entry: {0} (Acc: {1})",
                    followerName,
                    pos.ExecutingAccount != null ? pos.ExecutingAccount.Name : "Master"
                )
            );
            CancelOrderSafe(order, pos);
            // A2-3: DeltaExpectedPositionLocked deferred to OnAccountOrderUpdate confirmed-cancel
            // to prevent REAPER desync if the follower was microseconds from filling (Build 960 audit fix).
        }

        /// <summary>
        /// Build 929 Fix3 [P1]: PR #2 Image 3 -- Capture follower list before cleanup.
        /// Cancels all follower entry orders linked to this master BEFORE CleanupPosition
        /// destroys the dispatch map. Without this, followers stay alive as zombie Limit orders.
        /// </summary>
        // W7-044-T4 / W7-121-004 parent: SymmetryGuardCascadeFollowerCleanup orchestrator (CYC=3)
        private void SymmetryGuardCascadeFollowerCleanup(string masterEntryName)
        {
            if (!TryResolveCascadeContext(masterEntryName, out string[] followers))
                return;

            Print(
                string.Format(
                    "[CASCADE] Master {0} cancelled -- terminating {1} linked follower(s).",
                    masterEntryName,
                    followers.Length
                )
            );

            foreach (string followerName in followers)
                TryCancelFollowerEntry(followerName);
        }

        private void SymmetryGuardForgetEntry(string entryName)
        {
            if (string.IsNullOrEmpty(entryName))
                return;

            symmetryPendingFollowerFills.TryRemove(entryName, out _);
            symmetryMasterEntryToDispatch.TryRemove(entryName, out _);

            if (
                symmetryFleetEntryToDispatch.TryRemove(entryName, out var dispatchId)
                && symmetryDispatchById.TryGetValue(dispatchId, out var ctx)
            )
            {
                // ADR-019: FollowerEntries.Remove is superseded by the atomic CAS-loop publisher in Symmetry.cs.
                // Forget-on-remove is a no-op here: the CAS loop publishes a new snapshot that excludes
                // removed entries when the next follower set change occurs. Entry will be pruned by SymmetryGuardPruneDispatches.
                // Direct remove is intentionally omitted -- mutating the immutable snapshot array is incorrect.
            }
        }

        // W7-131-T1: HasActiveFollowers -- pure read over snapshot (CYC=3)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasActiveFollowers(SymmetryDispatchContext ctx)
        {
            // ADR-019: ctx.Followers is an immutable string[] snapshot -- lock-free iteration.
            // activePositions is a ConcurrentDictionary; ContainsKey is thread-safe without a lock.
            foreach (string follower in ctx.Followers)
            {
                if (activePositions.ContainsKey(follower))
                    return true;
            }
            return false;
        }

        // W7-131-T2: ShouldPruneDispatch -- eviction policy predicate (CYC=4)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ShouldPruneDispatch(SymmetryDispatchContext ctx, DateTime nowUtc) =>
            nowUtc - ctx.CreatedUtc > SymmetryDispatchTtl || (ctx.Anchor.IsResolved && !HasActiveFollowers(ctx));

        // W7-131-T3: TryPruneDispatchEntry -- per-entry prune action (CYC=3)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryPruneDispatchEntry(string key, SymmetryDispatchContext ctx, DateTime nowUtc)
        {
            if (ctx == null)
                return;
            if (ShouldPruneDispatch(ctx, nowUtc))
                symmetryDispatchById.TryRemove(key, out _);
        }

        // W7-131 parent: SymmetryGuardPruneDispatches orchestrator (CYC=2)
        private void SymmetryGuardPruneDispatches()
        {
            DateTime nowUtc = DateTime.UtcNow;
            foreach (var kvp in symmetryDispatchById.ToArray())
                TryPruneDispatchEntry(kvp.Key, kvp.Value, nowUtc);
        }

        private string SymmetryInferTradeType(string entryName, PositionInfo pos)
        {
            if (pos != null)
            {
                if (pos.IsTRENDTrade)
                    return "TREND";
                if (pos.IsRetestTrade)
                    return "RETEST";
                if (pos.IsFFMATrade)
                    return "FFMA";
                if (pos.IsMOMOTrade)
                    return "MOMO";
                if (pos.IsRMATrade)
                    return "RMA";
            }
            return SymmetryNormalizeTradeType(entryName);
        }

        // W7-037-T1 / W7-132-T1: IsOrTradeType -- OR trade type predicate (CYC=3)
        private static bool IsOrTradeType(string t) =>
            t.StartsWith("OR", StringComparison.Ordinal) || t.Contains("ORLONG") || t.Contains("ORSHORT");

        // W7-037-T2: NormalizeTradeTypeKernel -- classification chain (CYC=7)
        private static string NormalizeTradeTypeKernel(string t)
        {
            if (t.StartsWith("TREND", StringComparison.Ordinal))
                return "TREND";
            if (t.StartsWith("RETEST", StringComparison.Ordinal))
                return "RETEST";
            if (t.StartsWith("FFMA", StringComparison.Ordinal))
                return "FFMA";
            if (t.StartsWith("MOMO", StringComparison.Ordinal))
                return "MOMO";
            if (t.StartsWith("RMA", StringComparison.Ordinal))
                return "RMA";
            if (IsOrTradeType(t))
                return "OR";
            return "GENERIC";
        }

        // W7-037 parent: SymmetryNormalizeTradeType orchestrator (CYC=2)
        private string SymmetryNormalizeTradeType(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return "GENERIC";

            string t = raw.ToUpperInvariant();
            return NormalizeTradeTypeKernel(t);
        }

        private static string SymmetryTrim(string text, int maxLen)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            return text.Length <= maxLen ? text : text.Substring(0, maxLen);
        }

        #endregion
    }
}
