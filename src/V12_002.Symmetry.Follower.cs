// Build 971: Symmetry.Follower -- SymmetryGuardOnFollowerFill, IsAnchorPending, ProcessPendingFollowerFills, TryResolveFollower, ApplyMasterAnchor, SubmitFollowerBracket
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
        #region Symmetry Follower

        // W7-127-T1: ValidateAndInitFollowerPos -- null/flag guard + RemainingContracts init
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateAndInitFollowerPos(PositionInfo followerPos)
        {
            if (followerPos == null || !followerPos.IsFollower)
                return false;
            followerPos.EntryFilled = true;
            if (followerPos.RemainingContracts <= 0)
                followerPos.RemainingContracts = Math.Max(1, followerPos.TotalContracts);
            return true;
        }

        // W7-127-T2 / W7-042-T1: TryApplyPreCheckAnchorAndSubmit -- ANCHOR-01 double-map lookup + AnchorSnapshot + submit/defer
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryApplyPreCheckAnchorAndSubmit(string fleetEntryName, PositionInfo followerPos)
        {
            if (followerPos.BracketSubmitted)
                return;

            bool shouldSubmitImmediately = false;
            // [ANCHOR-01] V12.Phase7.1: Pre-check master anchor before initial bracket submission.
            if (
                symmetryFleetEntryToDispatch.TryGetValue(fleetEntryName, out var preCheckId)
                && symmetryDispatchById.TryGetValue(preCheckId, out var preCheckCtx)
            )
            {
                // ADR-019: AnchorSnapshot is published atomically via Interlocked.CompareExchange.
                // IsResolved and MasterAnchorPrice are read from a single immutable snapshot -- lock-free.
                AnchorSnapshot preCheckSnapshot = preCheckCtx.Anchor;
                bool anchorReady = preCheckSnapshot.IsResolved;
                double preCheckAnchor = preCheckSnapshot.MasterAnchorPrice;
                if (anchorReady && preCheckAnchor > 0)
                {
                    Print(
                        string.Format(
                            "[ANCHOR-01] Pre-applying master anchor {0:F2} for {1} -- bracket will use master fill price",
                            preCheckAnchor,
                            fleetEntryName
                        )
                    );
                    SymmetryGuardApplyMasterAnchor(followerPos, preCheckAnchor);
                    shouldSubmitImmediately = true;
                }
            }

            if (shouldSubmitImmediately)
            {
                SymmetryGuardSubmitFollowerBracket(fleetEntryName, followerPos);
            }
            else
            {
                Print(
                    string.Format(
                        "[ANCHOR-GATE] Delaying follower bracket for {0} until master anchor resolves.",
                        fleetEntryName
                    )
                );
            }
        }

        // W7-127-T3 / W7-042-T2: EnqueueAndTryResolveFollower -- PendingFollowerFill enqueue + immediate try-resolve
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void EnqueueAndTryResolveFollower(
            string fleetEntryName,
            PositionInfo followerPos,
            double followerFillPrice
        )
        {
            var pending = new PendingFollowerFill
            {
                FleetEntryName = fleetEntryName,
                FleetFillPrice = followerFillPrice > 0 ? followerFillPrice : followerPos.EntryPrice,
                QueuedUtc = DateTime.UtcNow,
            };
            symmetryPendingFollowerFills[fleetEntryName] = pending;
            if (SymmetryGuardTryResolveFollower(fleetEntryName, followerPos, pending, DateTime.UtcNow))
                symmetryPendingFollowerFills.TryRemove(fleetEntryName, out _);
        }

        // W7-127 parent after T1+T2+T3: CYC=3
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool SymmetryGuardOnFollowerFill(
            string fleetEntryName,
            PositionInfo followerPos,
            double followerFillPrice
        )
        {
            if (!ValidateAndInitFollowerPos(followerPos))
                return false;

            if (!followerPos.BracketSubmitted)
                TryApplyPreCheckAnchorAndSubmit(fleetEntryName, followerPos);

            EnqueueAndTryResolveFollower(fleetEntryName, followerPos, followerFillPrice);

            return true;
        }

        private bool SymmetryGuardIsAnchorPending(string entryName)
        {
            if (string.IsNullOrEmpty(entryName))
                return false;
            return symmetryPendingFollowerFills.ContainsKey(entryName);
        }

        private void SymmetryGuardProcessPendingFollowerFills()
        {
            if (symmetryPendingFollowerFills.IsEmpty)
            {
                SymmetryGuardPruneDispatches();
                return;
            }

            DateTime nowUtc = DateTime.UtcNow;
            foreach (var kvp in symmetryPendingFollowerFills.ToArray())
            {
                string fleetEntryName = kvp.Key;
                PendingFollowerFill pending = kvp.Value;

                // V12.Phase8 [F-04]: Guard activePositions read with stateLock to prevent
                // torn observations concurrent with ExecuteSmartDispatchEntry commits/removals.
                PositionInfo pos = null;
                activePositions.TryGetValue(fleetEntryName, out pos);
                if (pos == null || !pos.IsFollower)
                {
                    symmetryPendingFollowerFills.TryRemove(fleetEntryName, out _);
                    SymmetryGuardForgetEntry(fleetEntryName);
                    continue;
                }

                if (SymmetryGuardTryResolveFollower(fleetEntryName, pos, pending, nowUtc))
                    symmetryPendingFollowerFills.TryRemove(fleetEntryName, out _);
            }

            SymmetryGuardPruneDispatches();
        }

        // W7-057-T1: TryResolveDispatchContext -- tri-clause ConcurrentDictionary dispatch lookup guard
        private bool TryResolveDispatchContext(
            string fleetEntryName,
            PositionInfo pos,
            PendingFollowerFill pending,
            DateTime nowUtc,
            out SymmetryDispatchContext ctx
        )
        {
            ctx = null;
            if (
                !symmetryFleetEntryToDispatch.TryGetValue(fleetEntryName, out var dispatchId)
                || !symmetryDispatchById.TryGetValue(dispatchId, out ctx)
                || ctx == null
            )
            {
                if (nowUtc - pending.QueuedUtc >= SymmetryAnchorWait)
                {
                    SymmetryGuardSkipFollower(
                        fleetEntryName,
                        pos,
                        pending.FleetFillPrice,
                        0,
                        0,
                        "Missing dispatch context"
                    );
                    return true;
                }
                return false;
            }
            return true;
        }

        // W7-057-T2: TryResolveAnchorSnapshot -- anchor snapshot resolution + timeout guard
        private bool TryResolveAnchorSnapshot(
            string fleetEntryName,
            PositionInfo pos,
            PendingFollowerFill pending,
            DateTime nowUtc,
            SymmetryDispatchContext ctx,
            out double masterAnchor
        )
        {
            // ADR-019: AnchorSnapshot is published atomically via Interlocked.CompareExchange.
            // IsResolved and MasterAnchorPrice are read from a single immutable snapshot -- lock-free.
            AnchorSnapshot snapshot = ctx.Anchor;
            masterAnchor = snapshot.MasterAnchorPrice;
            if (!snapshot.IsResolved)
            {
                if (nowUtc - pending.QueuedUtc >= SymmetryAnchorWait)
                {
                    SymmetryGuardSkipFollower(
                        fleetEntryName,
                        pos,
                        pending.FleetFillPrice,
                        0,
                        0,
                        "Master anchor timeout"
                    );
                    return true;
                }
                return false;
            }
            return true;
        }

        // W7-057-T3: IsSlippageWithinTolerance -- slippage arithmetic + compound breach check
        private bool IsSlippageWithinTolerance(
            string fleetEntryName,
            PositionInfo pos,
            PendingFollowerFill pending,
            double masterAnchor
        )
        {
            double slippagePoints = Math.Abs(pending.FleetFillPrice - masterAnchor);
            double slippageTicks = tickSize > 0 ? slippagePoints / tickSize : 0.0;
            double slippageUsdPerContract = pointValue > 0 ? slippagePoints * pointValue : 0.0;
            bool breach =
                slippageTicks > SymmetryMaxSlippageTicks || slippageUsdPerContract > SymmetryMaxSlippageUsdPerContract;
            if (breach)
            {
                SymmetryGuardSkipFollower(
                    fleetEntryName,
                    pos,
                    pending.FleetFillPrice,
                    slippageTicks,
                    slippageUsdPerContract,
                    string.Format("Slippage Buffer breach vs Master {0:F2}", masterAnchor)
                );
                return false;
            }
            return true;
        }

        // W7-057/W7-129 parent after extractions: CYC=8
        private bool SymmetryGuardTryResolveFollower(
            string fleetEntryName,
            PositionInfo pos,
            PendingFollowerFill pending,
            DateTime nowUtc
        )
        {
            if (!TryResolveDispatchContext(fleetEntryName, pos, pending, nowUtc, out var ctx))
                return false;

            if (!TryResolveAnchorSnapshot(fleetEntryName, pos, pending, nowUtc, ctx, out var masterAnchor))
                return false;

            if (!IsSlippageWithinTolerance(fleetEntryName, pos, pending, masterAnchor))
                return true;

            // [ANCHOR-02] V12.Phase7.1: Capture entry price before anchor application.
            double priorEntryPrice = pos.EntryPrice;
            SymmetryGuardApplyMasterAnchor(pos, masterAnchor);

            if (pos.BracketSubmitted)
            {
                bool alreadyAnchored = tickSize > 0 && Math.Abs(priorEntryPrice - masterAnchor) < tickSize;
                if (alreadyAnchored)
                {
                    Print(
                        string.Format(
                            "[ANCHOR-02] Bracket already anchor-aligned for {0} (prior={1:F2} anchor={2:F2}) -- retarget skipped",
                            fleetEntryName,
                            priorEntryPrice,
                            masterAnchor
                        )
                    );
                }
                else
                {
                    SymmetryGuardRetargetExistingFollowerBracket(fleetEntryName, pos);
                }
            }
            else
            {
                SymmetryGuardSubmitFollowerBracket(fleetEntryName, pos);
            }

            double slippagePoints2 = Math.Abs(pending.FleetFillPrice - masterAnchor);
            double finalSlippageTicks = tickSize > 0 ? slippagePoints2 / tickSize : 0.0;
            double finalSlippageUsd = pointValue > 0 ? slippagePoints2 * pointValue : 0.0;
            Print(
                string.Format(
                    "[SYMMETRY_GUARD] ANCHORED | {0} | Master={1:F2} Fleet={2:F2} Slip={3:F1} ticks (${4:F2}/ct) | Scalp Anchor T1={5:F2} | Runner Targets=Trail",
                    fleetEntryName,
                    masterAnchor,
                    pending.FleetFillPrice,
                    finalSlippageTicks,
                    finalSlippageUsd,
                    pos.Target1Price
                )
            );

            return true;
        }

        private void SymmetryGuardApplyMasterAnchor(PositionInfo pos, double masterAnchor)
        {
            double anchor = Instrument.MasterInstrument.RoundToTickSize(masterAnchor);

            // V12.Phase8 [F-04]: Acquire stateLock for the entire anchor update to prevent
            // torn reads from Trailing.cs observing partial price state (e.g., new stop but old targets).
            double oldBase = pos.EntryPrice > 0 ? pos.EntryPrice : anchor;

            double stopDist = Math.Abs(oldBase - pos.InitialStopPrice);
            if (stopDist <= 0)
                stopDist = Math.Abs(oldBase - pos.CurrentStopPrice);

            double t1Dist = Math.Abs(pos.Target1Price - oldBase);
            double t2Dist = Math.Abs(pos.Target2Price - oldBase);
            double t3Dist = Math.Abs(pos.Target3Price - oldBase);
            double t4Dist = Math.Abs(pos.Target4Price - oldBase);
            double t5Dist = Math.Abs(pos.Target5Price - oldBase);

            double stop = pos.Direction == MarketPosition.Long ? anchor - stopDist : anchor + stopDist;
            double t1 = pos.Direction == MarketPosition.Long ? anchor + t1Dist : anchor - t1Dist;
            double t2 = pos.Direction == MarketPosition.Long ? anchor + t2Dist : anchor - t2Dist;
            double t3 = pos.Direction == MarketPosition.Long ? anchor + t3Dist : anchor - t3Dist;
            double t4 = pos.Direction == MarketPosition.Long ? anchor + t4Dist : anchor - t4Dist;
            double t5 = pos.Direction == MarketPosition.Long ? anchor + t5Dist : anchor - t5Dist;

            pos.EntryPrice = anchor;
            pos.ExtremePriceSinceEntry = anchor;

            pos.InitialStopPrice = Instrument.MasterInstrument.RoundToTickSize(stop);
            pos.CurrentStopPrice = pos.InitialStopPrice;
            pos.Target1Price = Instrument.MasterInstrument.RoundToTickSize(t1);
            pos.Target2Price = Instrument.MasterInstrument.RoundToTickSize(t2);
            pos.Target3Price = Instrument.MasterInstrument.RoundToTickSize(t3);
            pos.Target4Price = Instrument.MasterInstrument.RoundToTickSize(t4);
            pos.Target5Price = Instrument.MasterInstrument.RoundToTickSize(t5);
        }

        // W7-126-T1 / AggressiveInlining: ResolveOcoGroupId -- OCO group ID resolution ternary
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ResolveOcoGroupId(PositionInfo pos)
        {
            return !string.IsNullOrEmpty(pos.OcoGroupId) ? pos.OcoGroupId : ("SG_" + DateTime.UtcNow.Ticks.ToString());
        }

        // W7-043-T1: SymmetryGuardBuildStopOrder -- stop order construction
        private Order SymmetryGuardBuildStopOrder(
            PositionInfo pos,
            Account acct,
            OrderAction exitAction,
            double validatedStop,
            string ocoId,
            string fleetEntryName
        )
        {
            string stopSig = SymmetryTrim("Stop_" + fleetEntryName, 40);
            return acct.CreateOrder(
                Instrument,
                exitAction,
                OrderType.StopMarket,
                TimeInForce.Gtc,
                Math.Max(1, pos.TotalContracts),
                0,
                validatedStop,
                ocoId,
                stopSig,
                null
            );
        }

        // W7-043-T2 / W7-126-T2: SymmetryGuardStageTargetOrders -- target slot iteration
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SymmetryGuardStageTargetOrders(
            PositionInfo pos,
            Account acct,
            OrderAction exitAction,
            string ocoId,
            string fleetEntryName,
            List<(int targetNum, Order order)> stagedTargets,
            List<Order> ordersToSubmit,
            out int nonRunnerLimitQty,
            out int runnerQty
        )
        {
            nonRunnerLimitQty = 0;
            runnerQty = 0;
            for (int targetNum = 1; targetNum <= 5; targetNum++)
            {
                int targetQty = GetTargetContracts(pos, targetNum);
                if (targetQty <= 0)
                    continue;
                if (IsRunnerTarget(targetNum))
                {
                    runnerQty += targetQty;
                    continue;
                }
                double targetPrice = GetTargetPrice(pos, targetNum);
                if (targetPrice <= 0)
                {
                    Print(
                        string.Format(
                            "[SYMMETRY TARGET_SKIP] T{0} for {1} has qty={2} but invalid price={3:F2}; skipped",
                            targetNum,
                            fleetEntryName,
                            targetQty,
                            targetPrice
                        )
                    );
                    continue;
                }
                // [PARITY-01] V12.Phase7.1: Explicit tick rounding on limit price.
                double roundedTargetPrice = Instrument.MasterInstrument.RoundToTickSize(targetPrice);
                string targetSig = SymmetryTrim("T" + targetNum + "_" + fleetEntryName, 40);
                Order target = acct.CreateOrder(
                    Instrument,
                    exitAction,
                    OrderType.Limit,
                    TimeInForce.Gtc,
                    targetQty,
                    roundedTargetPrice,
                    0,
                    ocoId,
                    targetSig,
                    null
                );
                stagedTargets.Add((targetNum, target));
                ordersToSubmit.Add(target);
                nonRunnerLimitQty += targetQty;
            }
        }

        // W7-043-T3: SymmetryGuardInitFollowerBracketFSM -- FSM factory
        private FollowerBracketFSM SymmetryGuardInitFollowerBracketFSM(
            Account acct,
            string fleetEntryName,
            string ocoId,
            PositionInfo pos,
            Order stop,
            double validatedStop,
            List<(int targetNum, Order order)> stagedTargets
        )
        {
            var fsm = new FollowerBracketFSM
            {
                AccountName = acct.Name,
                EntryName = fleetEntryName,
                OcoGroupId = ocoId,
                State = FollowerBracketState.PendingSubmit,
                RemainingContracts = pos.TotalContracts,
                StopOrder = stop,
                ExpectedStopPrice = validatedStop,
            };
            for (int i = 0; i < 5; i++)
                fsm.ExpectedTargetPrices[i] = 0;
            foreach (var (tNum, tOrder) in stagedTargets)
            {
                if (tNum >= 1 && tNum <= 5)
                {
                    fsm.Targets[tNum - 1] = tOrder;
                    fsm.ExpectedTargetPrices[tNum - 1] = tOrder.LimitPrice;
                }
            }
            return fsm;
        }

        // W7-126/W7-043 parent after extractions: CYC=6
        private void SymmetryGuardSubmitFollowerBracket(string fleetEntryName, PositionInfo pos)
        {
            if (pos.BracketSubmitted)
                return;
            Account acct = pos.ExecutingAccount;
            if (acct == null)
                return;

            OrderAction exitAction = pos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;
            double validatedStop = ValidateStopPrice(pos.Direction, pos.CurrentStopPrice);
            // Build 936 [FIX-2]: Use deterministic OcoGroupId from PositionInfo for broker-native OCO bracket protection.
            string ocoId = ResolveOcoGroupId(pos);

            var ordersToSubmit = new List<Order>();
            var stagedTargets = new List<(int targetNum, Order order)>();

            Order stop = SymmetryGuardBuildStopOrder(pos, acct, exitAction, validatedStop, ocoId, fleetEntryName);
            SymmetryGuardStageTargetOrders(
                pos,
                acct,
                exitAction,
                ocoId,
                fleetEntryName,
                stagedTargets,
                ordersToSubmit,
                out int nonRunnerLimitQty,
                out int runnerQty
            );

            // Atomic commit before broker submission prevents REAPER race.
            FollowerBracketFSM fsm = SymmetryGuardInitFollowerBracketFSM(
                acct,
                fleetEntryName,
                ocoId,
                pos,
                stop,
                validatedStop,
                stagedTargets
            );
            _followerBrackets[fleetEntryName] = fsm;

            // B966: Enqueue stop write so it flows through actor pipeline (strategy thread, drains synchronously).
            ordersToSubmit.Insert(0, stop);
            {
                var _fen966 = fleetEntryName;
                var _s966 = stop;
                Enqueue(ctx =>
                {
                    ctx.stopOrders[_fen966] = _s966;
                });
            }
            foreach (var (targetNum, order) in stagedTargets)
                GetTargetOrdersDictionary(targetNum)[fleetEntryName] = order;

            fsm.State = FollowerBracketState.Submitted;
            fsm.LastUpdateUtc = DateTime.UtcNow;

            acct.Submit(ordersToSubmit.ToArray());
            pos.BracketSubmitted = true;
            Print(
                string.Format(
                    "[SYMMETRY STOP_AUDIT] OK {0}: StopQty={1} NonRunnerLimits={2} RunnerQty={3}",
                    fleetEntryName,
                    pos.TotalContracts,
                    nonRunnerLimitQty,
                    runnerQty
                )
            );
        }

        #endregion
    }
}
