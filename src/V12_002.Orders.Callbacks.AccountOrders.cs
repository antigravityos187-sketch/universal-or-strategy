// Build 971: Orders.Callbacks.AccountOrders -- OnAccountOrderUpdate, ProcessAccountOrderQueue, TryFindOrderInPosition, HandleMatchedFollowerOrder, ExecuteFollowerCascadeCleanup, ProcessQueuedAccountOrder
// V12 Orders.Callbacks Module (Extracted)
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class V12_002 : Strategy
    {
        #region Orders Callbacks Account Orders

        private void OnAccountOrderUpdate(object sender, OrderEventArgs e)
        {
            if (e == null || e.Order == null)
                return;

            Order order = e.Order;
            Account acct = sender as Account;
            if (acct == null)
                return;

            EnqueueFleetMailboxIfApplicable(acct, order);

            if (!IsOrderForThisInstrument(order))
                return;

            DispatchAccountOrderExpectedUpdate(acct, order);
            ProcessAccountOrder_EnqueueTerminalUpdate(sender, e, order);
        }

        // Phase 2: Enqueue into Actor Mailbox for FSM processing (Shadow Mode).
        // Only runs for fleet accounts whose order matches this strategy's instrument.
        private void EnqueueFleetMailboxIfApplicable(Account acct, Order order)
        {
            if (IsFleetAccount(acct) && order.Instrument != null && order.Instrument.FullName == Instrument.FullName)
            {
                _accountMailbox.Enqueue(
                    new AccountEvent
                    {
                        AccountAlias = acct.Name,
                        OrderId = order.OrderId,
                        NewState = order.OrderState,
                        FillPrice = order.AverageFillPrice,
                        FilledQty = order.Filled,
                        TimestampTicks = DateTime.UtcNow.Ticks,
                        SignalName = order.Name,
                        ErrorMessage = "",
                    }
                );
            }
        }

        // Returns true only if order belongs to this strategy's instrument.
        // Returning false signals the caller to skip further processing.
        private bool IsOrderForThisInstrument(Order order)
        {
            return order.Instrument != null && order.Instrument.FullName == Instrument.FullName;
        }

        // Build 1000 / Build 1104.1: Route expectedPositions update to master or fleet handler.
        // Called only after IsOrderForThisInstrument returns true.
        private void DispatchAccountOrderExpectedUpdate(Account acct, Order order)
        {
            if (acct == this.Account && order.Instrument != null && order.Instrument.FullName == Instrument.FullName)
                ProcessAccountOrder_UpdateMasterExpected(order);
            // Build 1104.1: Fleet account expectedPositions tracking (symmetric with Master).
            // Without this, expectedPositions stays stale after fleet stop/target fills,
            // causing REAPER to see Expected != Actual and trigger false flattens.
            else if (IsFleetAccount(acct))
                ProcessAccountOrder_UpdateFleetExpected(order, acct);
        }

        private void ProcessAccountOrder_UpdateMasterExpected(Order order)
        {
            if (order.OrderState == OrderState.Filled || order.OrderState == OrderState.PartFilled)
            {
                if (order.Name.StartsWith("Stop_"))
                    HandleMasterStopFill();
                else if (order.Name.StartsWith("T") && order.Name.Contains("_"))
                    HandleMasterTargetFill(order);
            }
        }

        private void HandleMasterStopFill()
        {
            _nakedPositionFirstSeen.TryRemove(Account.Name, out _);
            var mExpKey = ExpKey(Account.Name);
            Enqueue(ctx => ctx.SetExpectedPositionLocked(mExpKey, 0));
        }

        private void HandleMasterTargetFill(Order order)
        {
            int filledQty = order.Filled;
            var mExpKey = ExpKey(Account.Name);
            Enqueue(ctx =>
            {
                if (ctx.expectedPositions != null && ctx.expectedPositions.TryGetValue(mExpKey, out int currentExp))
                {
                    int newExp = 0;
                    if (currentExp > 0)
                        newExp = Math.Max(0, currentExp - filledQty);
                    else if (currentExp < 0)
                        newExp = Math.Min(0, currentExp + filledQty);

                    ctx.SetExpectedPositionLocked(mExpKey, newExp);
                }
            });
        }

        private void ProcessAccountOrder_UpdateFleetExpected(Order order, Account acct)
        {
            if (order.OrderState == OrderState.Filled || order.OrderState == OrderState.PartFilled)
            {
                if (order.Name.StartsWith("Stop_"))
                    HandleFleetStopFill(acct);
                else if (order.Name.StartsWith("T") && order.Name.Contains("_"))
                    HandleFleetTargetFill(order, acct);
            }
        }

        // Build 971 [CYC-REDUCE]: Extracted from ProcessAccountOrder_UpdateFleetExpected.
        // Fleet stop filled: position closing. Zero expectedPositions.
        private void HandleFleetStopFill(Account acct)
        {
            _nakedPositionFirstSeen.TryRemove(acct.Name, out _);
            var fExpKey = ExpKey(acct.Name);
            Enqueue(ctx => ctx.SetExpectedPositionLocked(fExpKey, 0));
        }

        // Build 971 [CYC-REDUCE]: Extracted from ProcessAccountOrder_UpdateFleetExpected.
        // Fleet target filled: delta-decrement expectedPositions.
        private void HandleFleetTargetFill(Order order, Account acct)
        {
            int fFilledQty = order.Filled;
            var fExpKey = ExpKey(acct.Name);
            Enqueue(ctx =>
            {
                if (ctx.expectedPositions != null && ctx.expectedPositions.TryGetValue(fExpKey, out int fCurrentExp))
                {
                    int fNewExp;
                    if (fCurrentExp > 0)
                        fNewExp = Math.Max(0, fCurrentExp - fFilledQty);
                    else if (fCurrentExp < 0)
                        fNewExp = Math.Min(0, fCurrentExp + fFilledQty);
                    else
                        fNewExp = 0;
                    ctx.SetExpectedPositionLocked(fExpKey, fNewExp);
                }
            });
        }

        private void ProcessAccountOrder_EnqueueTerminalUpdate(object sender, OrderEventArgs e, Order order)
        {
            if (
                order.OrderState != OrderState.Cancelled
                && order.OrderState != OrderState.Rejected
                && order.OrderState != OrderState.Unknown
            )
            {
                return;
            }

            // V12.1101E [TM-01]: Marshal broker-thread callback to strategy thread before mutating strategy state.
            _accountOrderQueue.Enqueue(new QueuedAccountOrderUpdate { Account = sender as Account, EventArgs = e });
            try
            {
                TriggerCustomEvent(o => ProcessAccountOrderQueue(), null);
            }
            catch (Exception ex)
            {
                if (_diagFleet)
                    Print("[FLEET_CATCH] OnAccountOrderUpdate trigger failed: " + ex.Message);
            }
        }

        // Build 935 [R-02]: Cap per-drain budget to prevent strategy-thread starvation
        // under high-velocity broker event bursts. Mirrors IpcMaxCommandsPerDrain pattern.
        private const int MaxAccountOrdersPerDrain = 8;

        // Build 971 [CYC-REDUCE]: Extracted from ProcessAccountOrderQueue to remove 3 duplicate
        // try/catch/if-_diagFleet patterns (6 CYC). Schedules a reprocess on the strategy thread.
        private void TriggerQueueReprocessSafe(string errorTag)
        {
            try
            {
                TriggerCustomEvent(o => ProcessAccountOrderQueue(), null);
            }
            catch (Exception ex)
            {
                if (_diagFleet)
                    Print("[FLEET_CATCH] " + errorTag + ": " + ex.Message);
            }
        }

        private void ProcessAccountOrderQueue()
        {
            // Build 1109 [FREEZE-PROOF]: Queue depth warning
            int oqDepth = _accountOrderQueue.Count;
            if (oqDepth > 50)
                Print("[ORDER_WARN] Account order queue depth=" + oqDepth);
            // V12.Phase7 [THREAD-01a]: Buffer-and-wait during flatten (symmetric with ProcessAccountExecutionQueue).
            if (isFlattenRunning)
            {
                TriggerQueueReprocessSafe("ProcessAccountOrderQueue flatten gate failed");
                return;
            }

            int drainedCount = 0;
            QueuedAccountOrderUpdate item;
            while (drainedCount < MaxAccountOrdersPerDrain && _accountOrderQueue.TryDequeue(out item))
            {
                if (isFlattenRunning)
                {
                    _accountOrderQueue.Enqueue(item);
                    TriggerQueueReprocessSafe("ProcessAccountOrderQueue drain loop failed");
                    return;
                }
                drainedCount++;
                ProcessQueuedAccountOrder(item);
            }
            // If items remain after budget exhausted, reschedule for next strategy-thread slice.
            if (!_accountOrderQueue.IsEmpty)
                TriggerQueueReprocessSafe("ProcessAccountOrderQueue reschedule failed");
        }

        // Build 1111.007-phase7-tW2 [T-W2]: Helper for Entry/Stop/T1 predicate (ref-equality short-circuit).
        // Preserves asymmetric pattern: ref-first then OrderId fallback. NO order null guard (H10).
        private bool TryFindOrder_MatchesEntryStopOrT1(
            ConcurrentDictionary<string, Order> dict,
            string entryKey,
            Order order
        )
        {
            return dict.TryGetValue(entryKey, out var tracked)
                && (tracked == order || (tracked != null && tracked.OrderId == order.OrderId));
        }

        // Build 1111.007-phase7-tW2 [T-W2]: Helper for T2-T5 predicate (OrderId-only equality).
        // Preserves asymmetric pattern: NO ref-equality check (H9). NO order null guard (H10).
        private bool TryFindOrder_MatchesT2ThroughT5(
            ConcurrentDictionary<string, Order> dict,
            string entryKey,
            Order order
        )
        {
            return dict.TryGetValue(entryKey, out var tracked) && tracked != null && tracked.OrderId == order.OrderId;
        }

        // Build 1111.007-phase7-tW2 [T-W2]: Returns true if 'order' belongs to 'entryKey' position.
        // Reduced from CYC=25 to CYC=8 via two-helper extraction. Preserves exact short-circuit order (B7/H11).
        private bool TryFindOrderInPosition(Order order, string entryKey, out string matchedEntry)
        {
            matchedEntry = null;
            // Sequential 7-step check preserving exact short-circuit order (B7/H11):
            // Entry/Stop/T1 use ref-first helper; T2-T5 use id-only helper (H9 asymmetry).
            if (TryFindOrder_MatchesEntryStopOrT1(entryOrders, entryKey, order))
            {
                matchedEntry = entryKey;
                return true;
            }
            if (TryFindOrder_MatchesEntryStopOrT1(stopOrders, entryKey, order))
            {
                matchedEntry = entryKey;
                return true;
            }
            if (TryFindOrder_MatchesEntryStopOrT1(target1Orders, entryKey, order))
            {
                matchedEntry = entryKey;
                return true;
            }
            if (TryFindOrder_MatchesT2ThroughT5(target2Orders, entryKey, order))
            {
                matchedEntry = entryKey;
                return true;
            }
            if (TryFindOrder_MatchesT2ThroughT5(target3Orders, entryKey, order))
            {
                matchedEntry = entryKey;
                return true;
            }
            if (TryFindOrder_MatchesT2ThroughT5(target4Orders, entryKey, order))
            {
                matchedEntry = entryKey;
                return true;
            }
            if (TryFindOrder_MatchesT2ThroughT5(target5Orders, entryKey, order))
            {
                matchedEntry = entryKey;
                return true;
            }
            return false;
        }

        private bool OrdersMatchByRefOrId(Order trackedOrder, Order order)
        {
            return trackedOrder == order
                || (trackedOrder != null && order != null && trackedOrder.OrderId == order.OrderId);
        }

        private bool TryFindMasterEntryForOrder(
            Order order,
            KeyValuePair<string, PositionInfo>[] snapshot,
            out string masterEntryName
        )
        {
            masterEntryName = null;
            if (order == null || snapshot == null)
                return false;

            foreach (var kvp in snapshot)
            {
                PositionInfo pos = kvp.Value;
                if (pos == null || pos.IsFollower)
                    continue;

                Order trackedEntry;
                if (entryOrders.TryGetValue(kvp.Key, out trackedEntry) && OrdersMatchByRefOrId(trackedEntry, order))
                {
                    masterEntryName = kvp.Key;
                    return true;
                }
            }

            return false;
        }

        private bool TryGetDispatchFollowerEntries(string masterEntryName, out string[] followerEntries)
        {
            followerEntries = null;
            if (string.IsNullOrEmpty(masterEntryName))
                return false;

            string dispatchId;
            SymmetryDispatchContext ctx;
            if (
                !symmetryMasterEntryToDispatch.TryGetValue(masterEntryName, out dispatchId)
                || !symmetryDispatchById.TryGetValue(dispatchId, out ctx)
                || ctx == null
            )
            {
                return false;
            }

            followerEntries = ctx.Followers;

            return followerEntries != null && followerEntries.Length > 0;
        }

        private bool IsMasterReplaceCascadeCancellation(
            Order order,
            KeyValuePair<string, PositionInfo>[] snapshot,
            out string masterEntryName,
            out string[] dispatchFollowers
        )
        {
            masterEntryName = null;
            dispatchFollowers = null;

            if (order == null || order.OrderState != OrderState.Cancelled || order.Account != this.Account)
                return false;

            if (!TryFindMasterEntryForOrder(order, snapshot, out masterEntryName))
                return false;

            if (!TryGetDispatchFollowerEntries(masterEntryName, out dispatchFollowers))
                return false;

            foreach (string followerEntry in dispatchFollowers)
            {
                if (IsFollowerSpecMatchForMasterReplace(followerEntry, masterEntryName))
                    return true;
            }

            return false;
        }

        // Helper: checks if the follower replace spec in _followerReplaceSpecs matches
        // the given follower/master pair and is in an active replace state.
        private bool IsFollowerSpecMatchForMasterReplace(string followerEntry, string masterEntryName)
        {
            FollowerReplaceSpec spec;
            if (!_followerReplaceSpecs.TryGetValue(followerEntry, out spec) || spec == null)
                return false;

            if (spec.State != FollowerReplaceState.PendingCancel && spec.State != FollowerReplaceState.Submitting)
                return false;

            if (!string.Equals(spec.MasterSignalName, masterEntryName, StringComparison.Ordinal))
                return false;

            if (!string.Equals(spec.SignalName, followerEntry, StringComparison.Ordinal))
                return false;

            return true;
        }

        // H06: Top-level follower cancellation processor (state-agnostic).
        // Returns true if cancellation was handled (caller should return early).
        // Checks: PendingCancel FSM, Target Replace FSM, Stop Replacement, PendingCleanup purge.
        private bool ProcessFollowerCancellationSafe(
            string matchedEntry,
            PositionInfo matchedPos,
            Order order,
            string acctName,
            string reason
        )
        {
            if (order == null || order.OrderState != OrderState.Cancelled)
            {
                return false;
            }

            // Check 1: PendingCancel entry replacement FSM
            FollowerReplaceSpec fsm;
            if (IsPendingCancelFsmMatch(matchedEntry, order, out fsm))
            {
                return HandleMatchedFollower_PendingCancelReplace(matchedEntry, order, acctName);
            }

            // Check 2: Target replacement FSM
            string tFsmMatchKey;
            if (TryFindTargetReplaceSpec(order, out tFsmMatchKey))
            {
                return HandleMatchedFollower_TargetReplaceCancel(order);
            }

            // Check 3: Stop replacement (follower stops arrive via OnAccountOrderUpdate)
            if (order.Name.StartsWith("Stop_") || order.Name.StartsWith("S_"))
            {
                if (HandleMatchedFollower_StopReplacement(order))
                    return true;

                // Check 4: PendingCleanup purge for terminal stops
                HandleMatchedFollowerPendingCleanupPurge(order);
                Print(
                    string.Format(
                        "[SIMA] Follower order terminal: {0} on {1} ({2}) | Id={3}",
                        order.Name,
                        acctName,
                        reason,
                        order.OrderId
                    )
                );
                RemoveGhostOrderRef(order, reason);
                return true;
            }

            return false;
        }

        // H06a: Returns true when the compound PendingCancel FSM condition is satisfied.
        private bool IsPendingCancelFsmMatch(string matchedEntry, Order order, out FollowerReplaceSpec fsm)
        {
            return _followerReplaceSpecs.TryGetValue(matchedEntry, out fsm)
                && fsm != null
                && fsm.State == FollowerReplaceState.PendingCancel
                && fsm.CancellingOrderId == order.OrderId;
        }

        // H06b: Scans _followerTargetReplaceSpecs for an entry whose CancellingOrderId matches order.
        private bool TryFindTargetReplaceSpec(Order order, out string matchKey)
        {
            foreach (var tKvp in _followerTargetReplaceSpecs.ToArray())
            {
                if (tKvp.Value.CancellingOrderId == order.OrderId)
                {
                    matchKey = tKvp.Key;
                    return true;
                }
            }
            matchKey = null;
            return false;
        }

        // Build 935 [R-01]: Handles a follower order positively matched to an active position.
        // Entry-not-filled -> rollback + desync label. Entry-filled or stop/target -> ghost log + cleanup.
        private void HandleMatchedFollowerOrder(
            string matchedEntry,
            PositionInfo matchedPos,
            Order order,
            string acctName,
            string reason
        )
        {
            if (ProcessFollowerCancellationSafe(matchedEntry, matchedPos, order, acctName, reason))
                return;

            if (IsEntryOrderMatch(matchedEntry, matchedPos, order, out _))
            {
                entryOrders.TryRemove(matchedEntry, out _);
                if (!IsAnyFollowerBracketActive(acctName))
                {
                    if (!ShouldRescuePendingCancelSpec(matchedEntry, order))
                        return;
                }
                HandleEntryNotFilledRollback(matchedEntry, acctName);
            }
            else
            {
                HandleTerminalFollowerOrder(order, acctName, reason);
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining
        )]
        private bool IsEntryOrderMatch(string matchedEntry, PositionInfo matchedPos, Order order, out Order entryOrder)
        {
            return entryOrders.TryGetValue(matchedEntry, out entryOrder)
                && (entryOrder == order || (entryOrder != null && entryOrder.OrderId == order.OrderId))
                && !matchedPos.EntryFilled;
        }

        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining
        )]
        private bool IsAnyFollowerBracketActive(string acctName)
        {
            return _followerBrackets.Values.Any(f =>
                f != null
                && f.AccountName == acctName
                && (f.State == FollowerBracketState.Active || f.State == FollowerBracketState.Accepted)
            );
        }

        private bool ShouldRescuePendingCancelSpec(string matchedEntry, Order order)
        {
            if (
                _followerReplaceSpecs.TryGetValue(matchedEntry, out var fsmGuard)
                && fsmGuard.State == FollowerReplaceState.PendingCancel
                && fsmGuard.CancellingOrderId == order.OrderId
            )
            {
                Print(
                    "[META-PURGE GUARD] Rescuing PendingCancel spec "
                        + matchedEntry
                        + " despite no active FSM. Delegating to resubmit path."
                );
                return true;
            }
            _followerReplaceSpecs.TryRemove(matchedEntry, out _);
            return false;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void HandleEntryNotFilledRollback(string matchedEntry, string acctName)
        {
            HandleMatchedFollower_DeltaRollback(matchedEntry);
            Print(
                string.Format("[SIMA] Follower entry cancelled: {0} on {1}. Reaper monitoring.", matchedEntry, acctName)
            );
            Draw.TextFixed(
                this,
                "SIMA_DESYNC_" + acctName,
                "(!) FOLLOWER DESYNC: " + acctName,
                TextPosition.TopLeft,
                Brushes.Red,
                new SimpleFont("Arial", 11),
                Brushes.Transparent,
                Brushes.Transparent,
                50
            );
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void HandleTerminalFollowerOrder(Order order, string acctName, string reason)
        {
            Print(
                string.Format(
                    "[SIMA] Follower order terminal: {0} on {1} ({2}) | Id={3}",
                    order.Name,
                    acctName,
                    reason,
                    order.OrderId
                )
            );
            RemoveGhostOrderRef(order, reason);
        }

        private bool HandleMatchedFollower_PendingCancelReplace(string matchedEntry, Order order, string acctName)
        {
            // Build 947 FSM: if this cancel was our PendingCancel, submit replacement instead of DESYNC
            FollowerReplaceSpec fsm;
            if (
                _followerReplaceSpecs.TryGetValue(matchedEntry, out fsm)
                && fsm.State == FollowerReplaceState.PendingCancel
                && fsm.CancellingOrderId == order.OrderId
            )
            {
                // Fill-during-gap guard: if master already has a live filled position, let REAPER handle
                // Phase 10 [B960-AUDIT]: synchronization wrapper removed. Both this path
                // via ProcessQueuedAccountOrder via TriggerCustomEvent and PropagateFollowerEntryReplace
                // are serialized on the NinjaTrader strategy thread. No concurrent field access is possible.
                int qty = 0;
                double price = 0;
                string acctNameCapture = acctName;
                string sigName = fsm.SignalName;
                FollowerReplaceSpec fsmCapture = fsm;

                bool masterFilled = IsMasterFilledDuringWait(fsm);

                if (!masterFilled)
                {
                    qty = fsm.PendingQty;
                    price = fsm.PendingPrice;
                    acctNameCapture = fsm.AccountName;
                    sigName = fsm.SignalName;
                    fsmCapture = fsm;
                    fsm.State = FollowerReplaceState.Submitting;
                }

                if (masterFilled)
                    return RouteMasterFilledToRepair(fsm, acctName);

                bool replacementScheduled = false;
                try
                {
                    TriggerCustomEvent(
                        o =>
                        {
                            // [P2 FSM CONSISTENCY]: Re-read price/qty from spec at execution time.
                            // ATR tick absorption may have updated PendingPrice/PendingQty after the
                            // lambda was scheduled -- using stale captures would submit wrong values.
                            SubmitFollowerReplacement(
                                sigName,
                                acctNameCapture,
                                fsmCapture.PendingPrice,
                                fsmCapture.PendingQty,
                                fsmCapture
                            );
                            _followerReplaceSpecs.TryRemove(sigName, out _);
                        },
                        null
                    );
                    replacementScheduled = true;
                }
                catch (Exception ex)
                {
                    Print("[FSM] TriggerCustomEvent failed for " + sigName + ": " + ex.Message);
                    _followerReplaceSpecs.TryRemove(sigName, out _);
                }
                if (replacementScheduled)
                    return true; // FSM-controlled replace cancel -- reservation stays live until resubmit completes.
            }

            return false;
        }

        private bool IsMasterFilledDuringWait(FollowerReplaceSpec fsm)
        {
            PositionInfo masterPos = null;
            return !string.IsNullOrEmpty(fsm.MasterSignalName)
                && activePositions.TryGetValue(fsm.MasterSignalName, out masterPos)
                && masterPos != null
                && masterPos.EntryFilled
                && masterPos.RemainingContracts > 0;
        }

        private bool RouteMasterFilledToRepair(FollowerReplaceSpec fsm, string acctName)
        {
            Print(
                "[FSM] Master filled during cancel wait -- routing " + fsm.SignalName + " to repair instead of replace."
            );
            _followerReplaceSpecs.TryRemove(fsm.SignalName, out _);
            string masterFilledExpKey = ExpKey(acctName);
            ClearDispatchSyncPending(masterFilledExpKey);
            _reaperRepairQueue.Enqueue(acctName);
            ProcessReaperRepairQueue();
            return true;
        }

        private bool HandleMatchedFollower_TargetReplaceCancel(Order order)
        {
            // T04: Snapshot moved to caller (ProcessFollowerCancellationSafe/ProcessFollowerCancellationUnconditional).
            // This method now expects the FSM spec to already be found by the caller.
            // The redundant search below is removed to eliminate double allocation.

            // B957/C1: Process follower TARGET replace FSM spec.
            // If this cancel was part of a two-phase target replacement, submit the new order
            // and return -- no delta rollback needed (position remains open, just target moved).
            FollowerTargetReplaceSpec tSpec = null;
            string tFsmMatchKey = null;

            // T04: Single search using snapshot from caller's context
            var snapshot = _followerTargetReplaceSpecs.ToArray();
            foreach (var tKvp in snapshot)
            {
                if (tKvp.Value.CancellingOrderId == order.OrderId)
                {
                    tSpec = tKvp.Value;
                    tFsmMatchKey = tKvp.Key;
                    break;
                }
            }

            if (tSpec != null && tFsmMatchKey != null)
            {
                _followerTargetReplaceSpecs.TryRemove(tFsmMatchKey, out _);
                FollowerTargetReplaceSpec captured = tSpec;
                string capturedKey = tFsmMatchKey;
                try
                {
                    TriggerCustomEvent(o => SubmitFollowerTargetReplacement(capturedKey, captured), null);
                }
                catch (Exception tFsmEx)
                {
                    Print("[FSM_TGT] TriggerCustomEvent failed for " + capturedKey + ": " + tFsmEx.Message);
                }
                return true; // FSM-controlled target cancel -- skip delta rollback, not a real desync
            }

            return false;
        }

        private void HandleMatchedFollower_DeltaRollback(string matchedEntry)
        {
            // A2-3: Direction-aware delta rollback on CONFIRMED cancel -- deferred from SymmetryGuardCascadeFollowerCleanup
            // to prevent REAPER desync on microsecond fill race (Build 960 audit fix).
            PositionInfo cancelledFollowerPos;
            if (activePositions.TryGetValue(matchedEntry, out cancelledFollowerPos) && cancelledFollowerPos != null)
            {
                if (cancelledFollowerPos.ExecutingAccount == null)
                {
                    Print(
                        "[B983-D2] HandleMatchedFollowerOrder: ExecutingAccount null for "
                            + matchedEntry
                            + " -- skipping ExpKey delta and sync barrier ops to avoid master domain bleed."
                    );
                }
                else
                {
                    string cancelAcctKey = cancelledFollowerPos.ExecutingAccount.Name;
                    int cancelDelta =
                        (cancelledFollowerPos.Direction == MarketPosition.Long)
                            ? -cancelledFollowerPos.TotalContracts
                            : cancelledFollowerPos.TotalContracts;
                    DeltaExpectedPositionLocked(ExpKey(cancelAcctKey), cancelDelta);
                    // B957/D2: Release the SIMA dispatch-sync barrier for this account. Without this, the barrier
                    // remains permanently blocked after a follower cancel, starving future dispatches.
                    _dispatchSyncPendingExpKeys.TryRemove(ExpKey(cancelAcctKey), out _); // [B967-FIX-02]
                }
            }
        }

        private bool HandleMatchedFollower_StopReplacement(Order order)
        {
            // Build 950: Follower stop replacement -- mirrors HandleOrderCancelled master path.
            // Follower stop cancels arrive via OnAccountOrderUpdate (not OnOrderUpdate), so
            // HandleOrderCancelled never fires for them. Match pendingStopReplacements here.
            if (order.Name.StartsWith("Stop_") || order.Name.StartsWith("S_"))
            {
                foreach (var psr in pendingStopReplacements.ToArray())
                {
                    if (IsMatchingStopReplacement(psr.Value.OldOrder, order))
                    {
                        ExecuteStopReplacementIfActive(psr.Key, psr.Value);
                        if (pendingStopReplacements.TryRemove(psr.Key, out _))
                            Interlocked.Decrement(ref pendingReplacementCount);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsMatchingStopReplacement(Order psrOldOrder, Order order)
        {
            return psrOldOrder == order || (psrOldOrder != null && psrOldOrder.OrderId == order.OrderId);
        }

        private void ExecuteStopReplacementIfActive(string key, PendingStopReplacement psrValue)
        {
            PositionInfo rPos;
            if (activePositions.TryGetValue(key, out rPos))
            {
                int rQty = rPos.RemainingContracts;
                if (rQty > 0)
                {
                    CreateNewStopOrder(key, rQty, psrValue.StopPrice, psrValue.Direction);
                    if (psrValue.BracketRestorationNeeded && psrValue.CapturedTargets != null)
                    {
                        TargetSnapshot[] snap = psrValue.CapturedTargets;
                        TriggerCustomEvent(o => RestoreCascadedTargets(key, snap), null);
                    }
                }
            }
        }

        private void HandleMatchedFollowerPendingCleanupPurge(Order order)
        {
            // A2-2: Deferred PendingCleanup purge -- follower stop terminal (Build 960 audit fix).
            if (order.Name.StartsWith("Stop_") || order.Name.StartsWith("S_"))
                PurgeFollowerStopScanStopOrders(order);
        }

        // A2-2 helper: scans stopOrders for a matching order and purges if PendingCleanup complete.
        private void PurgeFollowerStopScanStopOrders(Order order)
        {
            foreach (var sc in stopOrders.ToArray())
            {
                if (sc.Value == order)
                {
                    PositionInfo scPos;
                    if (
                        activePositions.TryGetValue(sc.Key, out scPos)
                        && scPos != null
                        && scPos.PendingCleanup
                        && scPos.RemainingContracts <= 0
                    )
                    {
                        stopOrders.TryRemove(sc.Key, out _);
                        activePositions.TryRemove(sc.Key, out _);
                        SymmetryGuardForgetEntry(sc.Key);
                        Print("[A2-2] Deferred PendingCleanup purge (follower stop terminal): " + sc.Key);
                    }
                    break;
                }
            }
        }

        // Build 935 [R-01]: SIMA cascade cleanup for unmatched master-cancel events.
        // Receives pre-computed snapshot -- eliminates the second activePositions.ToArray() allocation.
        private void ExecuteFollowerCascadeCleanup(
            bool enableSima,
            Order order,
            string reason,
            KeyValuePair<string, PositionInfo>[] snapshot
        )
        {
            // V12.18 SIMA CASCADE: If a master-account order was cancelled,
            // check if any follower positions share the same base signal and tear them down.
            if (enableSima && order.OrderState == OrderState.Cancelled && order.Account == this.Account)
            {
                string masterEntryName;
                string[] dispatchFollowers;
                if (
                    ExecuteFollowerCascadeSuppressMasterReplace(
                        order,
                        reason,
                        snapshot,
                        out masterEntryName,
                        out dispatchFollowers
                    )
                )
                    return;

                string orderSignal = order.Name;
                Dictionary<string, PositionInfo> snapshotByKey = new Dictionary<string, PositionInfo>();
                foreach (var kvp in snapshot)
                    snapshotByKey[kvp.Key] = kvp.Value;

                IEnumerable<string> followerKeys = ExecuteFollowerCascadeResolveFollowers(
                    orderSignal,
                    masterEntryName,
                    dispatchFollowers,
                    snapshot
                );

                foreach (string followerKey in followerKeys)
                    ExecuteFollowerCascadeProcessFollower(followerKey, snapshotByKey, masterEntryName, orderSignal);
            }
            RemoveGhostOrderRef(order, reason);
        }

        private void ExecuteFollowerCascadeProcessFollower(
            string followerKey,
            Dictionary<string, PositionInfo> snapshotByKey,
            string masterEntryName,
            string orderSignal
        )
        {
            PositionInfo cascadePos;
            if (!snapshotByKey.TryGetValue(followerKey, out cascadePos) || cascadePos == null || !cascadePos.IsFollower)
                return;

            string cascadeAcctName = cascadePos.ExecutingAccount != null ? cascadePos.ExecutingAccount.Name : "NULL";

            // [BUILD 984] [FIX-A]: Skip cascade teardown if this follower has an in-flight Replace FSM.
            // A chart-drag cancel on the master reaches this path. Destroying the follower here zeroes
            // expectedPositions mid-replace; the replacement fill then triggers REAPER Critical Desync
            // (actualQty != 0, expectedQty == 0) -> Emergency Flatten.
            FollowerReplaceSpec b948FsmSpec;
            if (_followerReplaceSpecs.TryGetValue(followerKey, out b948FsmSpec))
            {
                Print(
                    string.Format(
                        "[FSM-GUARD] SKIP cascade teardown for {0} on {1}: in-flight Replace FSM (state={2}). Chart-drag suppressed.",
                        followerKey,
                        cascadeAcctName,
                        b948FsmSpec.State
                    )
                );
                return;
            }

            if (!cascadePos.EntryFilled)
                ExecuteFollowerCascadeCleanupUnfilled(masterEntryName, orderSignal, followerKey, cascadePos);
            else
                ExecuteFollowerCascadeEmergencyFlattenFilled(masterEntryName, orderSignal, followerKey, cascadePos);
        }

        private bool ExecuteFollowerCascadeSuppressMasterReplace(
            Order order,
            string reason,
            KeyValuePair<string, PositionInfo>[] snapshot,
            out string masterEntryName,
            out string[] dispatchFollowers
        )
        {
            if (IsMasterReplaceCascadeCancellation(order, snapshot, out masterEntryName, out dispatchFollowers))
            {
                Print(
                    string.Format("[FSM] Suppressing cascade teardown for master replace cancel: {0}", masterEntryName)
                );
                RemoveGhostOrderRef(order, reason);
                return true;
            }

            return false;
        }

        private IEnumerable<string> ExecuteFollowerCascadeResolveFollowers(
            string orderSignal,
            string masterEntryName,
            string[] dispatchFollowers,
            KeyValuePair<string, PositionInfo>[] snapshot
        )
        {
            if (!string.IsNullOrEmpty(masterEntryName) && dispatchFollowers != null && dispatchFollowers.Length > 0)
                return dispatchFollowers;

            // [BUILD 984] [FIX-B]: Delimiter-anchored match replaces bidirectional .Contains().
            // Bidirectional .Contains() caused accidental cascade of unrelated positions:
            // e.g. signal "OR" matched "Fleet_Apex_RETEST_OR_1" incidentally.
            // Anchoring on underscores prevents substring contamination across signal families.
            return snapshot
                .Where(kvp =>
                    kvp.Value != null
                    && kvp.Value.IsFollower
                    && (
                        kvp.Key == orderSignal
                        || kvp.Key.Contains("_" + orderSignal + "_")
                        || kvp.Key.EndsWith("_" + orderSignal)
                    )
                )
                .Select(kvp => kvp.Key)
                .ToArray();
        }

        private void ExecuteFollowerCascadeCleanupUnfilled(
            string masterEntryName,
            string orderSignal,
            string followerKey,
            PositionInfo cascadePos
        )
        {
            string cascadeAcctName = cascadePos.ExecutingAccount != null ? cascadePos.ExecutingAccount.Name : "NULL";

            Print(
                string.Format(
                    "[GHOST_FIX] SIMA CASCADE: Master cancel of {0} triggers follower teardown for {1} on {2}",
                    !string.IsNullOrEmpty(masterEntryName) ? masterEntryName : orderSignal,
                    followerKey,
                    cascadeAcctName
                )
            );
            CleanupPosition(followerKey);

            if (cascadePos.ExecutingAccount != null)
            {
                int rollbackDelta =
                    (cascadePos.Direction == MarketPosition.Long)
                        ? -cascadePos.TotalContracts
                        : cascadePos.TotalContracts;
                int currentExp = 0;
                expectedPositions.TryGetValue(ExpKey(cascadeAcctName), out currentExp);
                if (currentExp == 0)
                {
                    Print(
                        string.Format(
                            "[GHOST_FIX] SKIP cascade delta for {0}: expectedPositions already 0 (purge-race guard). Delta suppressed.",
                            cascadeAcctName
                        )
                    );
                }
                else
                {
                    DeltaExpectedPositionLocked(ExpKey(cascadeAcctName), rollbackDelta);
                }
                ClearDispatchSyncPending(ExpKey(cascadeAcctName));
                try
                {
                    RemoveDrawObject("SIMA_DESYNC_" + cascadeAcctName);
                }
                catch (Exception ex)
                {
                    if (_diagFleet)
                        Print("[FLEET_CATCH] ExecuteFollowerCascade desync cleanup failed: " + ex.Message);
                }
            }
        }

        private void ExecuteFollowerCascadeEmergencyFlattenFilled(
            string masterEntryName,
            string orderSignal,
            string followerKey,
            PositionInfo cascadePos
        )
        {
            string cascadeAcctName = cascadePos.ExecutingAccount != null ? cascadePos.ExecutingAccount.Name : "NULL";

            Print(
                string.Format(
                    "[DEAD-01] CASCADE-FILLED: Master cancel {0} -- follower {1} on {2} is FILLED. Issuing emergency flatten.",
                    !string.IsNullOrEmpty(masterEntryName) ? masterEntryName : orderSignal,
                    followerKey,
                    cascadeAcctName
                )
            );
            if (cascadePos.ExecutingAccount != null)
            {
                Account filledFollowerAcct = cascadePos.ExecutingAccount;
                TriggerCustomEvent(o => EmergencyFlattenSingleFleetAccount(filledFollowerAcct), null);
            }
        }

        // H06: State-agnostic cancellation processor for follower orders.
        // Processes cancellations BEFORE matched-entry gate to handle stale-state scenarios.
        // Returns true if cancellation was handled (caller should skip normal flow).
        private bool ProcessFollowerCancellationUnconditional(Order order, string acctName, string reason)
        {
            if (order == null || order.OrderState != OrderState.Cancelled)
                return false;

            if (TryHandleReplaceSpecCancellation(order, acctName))
                return true;

            if (TryHandleTargetReplaceCancellation(order))
                return true;

            return HandleStopOrderCancellation(order, acctName, reason);
        }

        // Extracted: Check 1 -- PendingCancel entry replacement FSM loop
        private bool TryHandleReplaceSpecCancellation(Order order, string acctName)
        {
            var replaceSpecsSnapshot = _followerReplaceSpecs.ToArray();
            foreach (var kvp in replaceSpecsSnapshot)
            {
                FollowerReplaceSpec fsm = kvp.Value;
                if (fsm == null)
                    continue;
                if (fsm.State == FollowerReplaceState.PendingCancel && fsm.CancellingOrderId == order.OrderId)
                {
                    string matchedEntry = kvp.Key;
                    return HandleMatchedFollower_PendingCancelReplace(matchedEntry, order, acctName);
                }
            }
            return false;
        }

        // Extracted: Check 2 -- Target replacement FSM loop
        private bool TryHandleTargetReplaceCancellation(Order order)
        {
            var targetReplaceSpecsSnapshot = _followerTargetReplaceSpecs.ToArray();
            foreach (var tKvp in targetReplaceSpecsSnapshot)
            {
                if (tKvp.Value.CancellingOrderId == order.OrderId)
                    return HandleMatchedFollower_TargetReplaceCancel(order);
            }
            return false;
        }

        // Extracted: Check 3+4 -- Stop replacement and terminal cleanup
        // P2-FIX (Iteration 4): null guard preserved before order.Name access
        private bool HandleStopOrderCancellation(Order order, string acctName, string reason)
        {
            if (order.Name == null || (!order.Name.StartsWith("Stop_") && !order.Name.StartsWith("S_")))
                return false;

            if (HandleMatchedFollower_StopReplacement(order))
                return true;

            HandleMatchedFollowerPendingCleanupPurge(order);
            Print(
                string.Format(
                    "[SIMA] Follower order terminal: {0} on {1} ({2}) | Id={3}",
                    order.Name,
                    acctName,
                    reason,
                    order.OrderId
                )
            );
            RemoveGhostOrderRef(order, reason);
            return true;
        }

        private void ProcessQueuedAccountOrder(QueuedAccountOrderUpdate item)
        {
            if (!IsValidQueuedOrderForThisInstrument(item))
                return;

            Order order = item.EventArgs.Order;
            string reason = order.OrderState.ToString().ToUpper();
            string acctName = item.Account != null ? item.Account.Name : "UNKNOWN";
            Print(
                string.Format(
                    "[GHOST-AUDIT] OnAccountOrderUpdate: {0} | State={1} | Acct={2}",
                    order.Name,
                    reason,
                    acctName
                )
            );

            // H06: Process cancellations BEFORE matched-entry gate (state-agnostic path)
            if (ProcessFollowerCancellationUnconditional(order, acctName, reason))
                return;

            // Build 935 [R-01]: Single snapshot -- reused by both identity search and cascade cleanup,
            // eliminating the second activePositions.ToArray() allocation in the cascade path.
            var snapshot = activePositions.ToArray();

            TryMatchFollowerPositionInSnapshot(
                item,
                order,
                snapshot,
                out string matchedEntry,
                out PositionInfo matchedPos
            );
            DispatchMatchedFollowerResult(matchedEntry, matchedPos, order, acctName, reason, snapshot);
        }

        private bool IsValidQueuedOrderForThisInstrument(QueuedAccountOrderUpdate item)
        {
            if (item?.EventArgs?.Order == null)
                return false;
            if (
                !Instrument.MasterInstrument.Name.Equals(
                    item.EventArgs.Order.Instrument.MasterInstrument.Name,
                    StringComparison.OrdinalIgnoreCase
                )
            )
                return false;
            return true;
        }

        private bool TryMatchFollowerPositionInSnapshot(
            QueuedAccountOrderUpdate item,
            Order order,
            KeyValuePair<string, PositionInfo>[] snapshot,
            out string matchedEntry,
            out PositionInfo matchedPos
        )
        {
            matchedEntry = string.Empty;
            matchedPos = null;
            foreach (var kvp in snapshot)
            {
                if (string.IsNullOrEmpty(kvp.Key))
                    continue;
                PositionInfo pos = kvp.Value;
                if (ShouldSkipSnapshotEntry(pos, item))
                    continue;
                if (TryFindOrderInPosition(order, kvp.Key, out matchedEntry))
                {
                    matchedPos = pos;
                    return true;
                }
            }
            matchedEntry = string.Empty;
            matchedPos = null;
            return false;
        }

        private bool ShouldSkipSnapshotEntry(PositionInfo pos, QueuedAccountOrderUpdate item)
        {
            if (pos == null || !IsFollowerPosition(pos))
                return true;
            return pos.Account?.Name != item.Account?.Name;
        }

        private void DispatchMatchedFollowerResult(
            string matchedEntry,
            PositionInfo matchedPos,
            Order order,
            string acctName,
            string reason,
            KeyValuePair<string, PositionInfo>[] snapshot
        )
        {
            if (!string.IsNullOrEmpty(matchedEntry) && matchedPos != null)
                HandleMatchedFollowerOrder(matchedEntry, matchedPos, order, acctName, reason);
            else
                ExecuteFollowerCascadeCleanup(EnableSIMA, order, reason, snapshot);
        }

        #endregion
    }
}
