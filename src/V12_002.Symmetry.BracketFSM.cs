// Build 982: BracketFSM (Shadow Mode) - Phase 2 Definitions
// V12 Symmetry Module - Follower Bracket Finite State Machine
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
        #region BracketFSM Definitions

        /// <summary>
        /// Phase 2: Follower Bracket States for Shadow Mode.
        /// Tracks the lifecycle of a follower bracket from strategic intent to terminal state.
        /// </summary>
        private enum FollowerBracketState
        {
            None, // Initial state
            PendingSubmit, // Strategic intent to submit, pre-submission validation/anchoring
            Submitted, // acct.Submit() called, awaiting broker ack
            Accepted, // Broker acknowledged (OrderState.Accepted/Working)
            Active, // Entry filled, protective bracket (Stop + Targets) live
            Replacing, // In-flight two-phase cancel+resubmit (MOVE-SYNC FSM active)
            Modifying, // Price change (trailing) in flight, awaiting confirm
            Filled, // Final: Position closed via Stop or Target fill
            Cancelled, // Final: All orders cancelled
            Rejected, // Final: Broker rejected (requires audit)
            Disconnected, // Temporary: Account connection lost, FSM frozen
        }

        /// <summary>
        /// Classifies a fill event signal into Stop, Target, or Entry kind.
        /// Eliminates stringly-typed comparisons at call sites (illegal states unrepresentable).
        /// </summary>
        private enum FillSignalKind
        {
            Entry,
            Stop,
            Target,
        }

        /// <summary>
        /// Consolidated FSM Container for a single Follower Bracket (Entry + Stop + Targets).
        /// Replaces the dictionary-scatter pattern with a single source of truth.
        /// </summary>
        private class FollowerBracketFSM
        {
            public string AccountName;
            public string EntryName; // Links to Master Position key (fleetEntryName)
            public string OcoGroupId; // Shared ID for broker OCO
            public FollowerBracketState State = FollowerBracketState.None;
            public int RemainingContracts;
            public string ReplacingCancelOrderId;
            public DateTime LastUpdateUtc = DateTime.UtcNow;

            public Order EntryOrder;
            public Order StopOrder;
            public Order[] Targets = new Order[5]; // Index 0-4 for T1-T5

            // Shadow Mode Diagnostics
            public bool IsInSync = true;
            public string LastBrokerError;

            // Metadata for reconciliation
            public double ExpectedEntryPrice;
            public double ExpectedStopPrice;
            public double[] ExpectedTargetPrices = new double[5];
        }

        /// <summary>
        /// Actor Mailbox Message for lock-free account event processing.
        /// Enqueued by account threads, consumed by strategy thread.
        /// </summary>
        public struct AccountEvent
        {
            public string AccountAlias;
            public string OrderId;
            public OrderState NewState;
            public double FillPrice;
            public int FilledQty;
            public long TimestampTicks;
            public string SignalName; // Optional: helps with un-tracked order matching
            public string ErrorMessage;
        }

        #endregion

        #region BracketFSM Logic (Actor Consumer)

        /// <summary>
        /// Consumes queued account events from the strategy thread.
        /// Called from OnBarUpdate or OnOrderUpdate via TriggerCustomEvent.
        /// Renamed from DrainAccountMailbox to avoid duplicate with Lifecycle.cs shutdown flush.
        /// </summary>
        private void ProcessAccountMailbox()
        {
            if (!EnsureStartupReady("ProcessAccountMailbox"))
                return;

            int processed = 0;
            const int MAX_PER_DRAIN = 100;

            while (processed < MAX_PER_DRAIN && _accountMailbox.TryDequeue(out var evt))
            {
                ProcessBracketEvent(evt);
                processed++;
            }
        }

        // ---------------------------------------------------------------------------
        // W7-066 / W7-122: RemoveFsmOrderIdMappings helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Removes a single Order's OrderId from the FSM lookup map if present.
        /// Structurally prevents null or empty OrderId from reaching TryRemove.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveSingleOrderMapping(Order order)
        {
            if (order != null && !string.IsNullOrEmpty(order.OrderId))
                _orderIdToFsmKey.TryRemove(order.OrderId, out _);
        }

        /// <summary>
        /// Removes a replacing-cancel OrderId from the FSM lookup map if present.
        /// Single-responsibility: handles the bare string field that has no Order wrapper.
        /// </summary>
        private void RemoveReplacingCancelMapping(string cancelOrderId)
        {
            if (!string.IsNullOrEmpty(cancelOrderId))
                _orderIdToFsmKey.TryRemove(cancelOrderId, out _);
        }

        /// <summary>
        /// Removes all target Order ids from the FSM lookup map.
        /// Isolated iteration kernel -- delegates per-element to RemoveSingleOrderMapping.
        /// </summary>
        private void RemoveTargetOrderMappings(Order[] targets)
        {
            if (targets == null)
                return;

            foreach (Order target in targets)
                RemoveSingleOrderMapping(target);
        }

        private void RemoveFsmOrderIdMappings(FollowerBracketFSM fsm)
        {
            if (fsm == null)
                return;

            RemoveSingleOrderMapping(fsm.EntryOrder);
            RemoveReplacingCancelMapping(fsm.ReplacingCancelOrderId);
            RemoveSingleOrderMapping(fsm.StopOrder);
            RemoveTargetOrderMappings(fsm.Targets);
        }

        private bool TryTerminateFollowerBracket(string entryName, out FollowerBracketFSM removedFsm)
        {
            removedFsm = null;
            if (string.IsNullOrEmpty(entryName))
                return false;
            if (!_followerBrackets.TryRemove(entryName, out removedFsm))
                return false;

            RemoveFsmOrderIdMappings(removedFsm);
            return true;
        }

        private void SetFsmReplacing(string fleetEntryName, string cancelOrderId)
        {
            if (string.IsNullOrEmpty(fleetEntryName) || string.IsNullOrEmpty(cancelOrderId))
                return;

            FollowerBracketFSM fsm;
            if (!_followerBrackets.TryGetValue(fleetEntryName, out fsm) || fsm == null)
                return;

            fsm.State = FollowerBracketState.Replacing;
            fsm.ReplacingCancelOrderId = cancelOrderId;
            fsm.LastUpdateUtc = DateTime.UtcNow;
            Print(string.Format("[FSM-C2] {0} -> Replacing (cancelId={1})", fleetEntryName, cancelOrderId));
        }

        /// <summary>
        /// Resolves AccountEvent to FollowerBracketFSM via 3-tier lookup strategy.
        /// Tier 1: O(1) OrderId map lookup (primary).
        /// Tier 2: SignalName parsing and matching (secondary).
        /// Tier 3: O(N) fallback scan across all FSMs (last resort).
        /// Back-fills OrderId map when found via fallback for future O(1) access.
        /// </summary>
        /// <summary>
        /// Tier 1: O(1) primary lookup via OrderId map.
        /// </summary>
        private FollowerBracketFSM ResolveFsm_ByOrderId(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
                return null;

            if (_orderIdToFsmKey.TryGetValue(orderId, out var entryName))
            {
                _followerBrackets.TryGetValue(entryName, out var fsm);
                return fsm;
            }

            return null;
        }

        /// <summary>
        /// Tier 2: Secondary lookup via SignalName parsing with backfill.
        /// Signal names are like "Stop_Fleet_Apex_1" or "T1_Fleet_Apex_1".
        /// The fleetEntryName is the part after the first underscore.
        /// </summary>
        private FollowerBracketFSM ResolveFsm_BySignalName(string signalName, string orderId)
        {
            if (string.IsNullOrEmpty(signalName))
                return null;

            int firstUnder = signalName.IndexOf('_');
            if (firstUnder >= 0 && firstUnder < signalName.Length - 1)
            {
                string fleetEntryName = signalName.Substring(firstUnder + 1);
                if (_followerBrackets.TryGetValue(fleetEntryName, out var fsm))
                {
                    // Back-fill the OrderId map if we found it via signal
                    if (!string.IsNullOrEmpty(orderId))
                        _orderIdToFsmKey[orderId] = fleetEntryName;

                    return fsm;
                }
            }

            return null;
        }

        // ---------------------------------------------------------------------------
        // W7-064: ResolveFsm_ByScan helper
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Per-FSM slot scan (StopOrder -> Targets[0-4] -> EntryOrder) with
        /// _orderIdToFsmKey backfill on match. Dead-code bool foundT removed.
        /// </summary>
        private FollowerBracketFSM MatchOrderInFsm(FollowerBracketFSM f, string orderId)
        {
            if (f.StopOrder != null && f.StopOrder.OrderId == orderId)
            {
                _orderIdToFsmKey[orderId] = f.EntryName;
                return f;
            }

            for (int i = 0; i < 5; i++)
            {
                if (f.Targets[i] != null && f.Targets[i].OrderId == orderId)
                {
                    _orderIdToFsmKey[orderId] = f.EntryName;
                    return f;
                }
            }

            if (f.EntryOrder != null && f.EntryOrder.OrderId == orderId)
            {
                _orderIdToFsmKey[orderId] = f.EntryName;
                return f;
            }

            return null;
        }

        /// <summary>
        /// Tier 3: Last-resort O(N) scan with backfill.
        /// Scan order: StopOrder -> Targets[0-4] -> EntryOrder.
        /// </summary>
        private FollowerBracketFSM ResolveFsm_ByScan(string accountAlias, string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
                return null;

            foreach (var f in _followerBrackets.Values)
            {
                if (f.AccountName != accountAlias)
                    continue;

                var match = MatchOrderInFsm(f, orderId);
                if (match != null)
                    return match;
            }

            return null;
        }

        /// <summary>
        /// 3-tier FSM resolution router: OrderId (O(1)) -> SignalName -> Scan (O(N)).
        /// </summary>
        private FollowerBracketFSM ResolveFsmFromEvent(AccountEvent evt)
        {
            // Tier 1: O(1) OrderId lookup (primary)
            FollowerBracketFSM fsm = ResolveFsm_ByOrderId(evt.OrderId);
            if (fsm != null)
                return fsm;

            // Tier 2: SignalName parsing (secondary)
            fsm = ResolveFsm_BySignalName(evt.SignalName, evt.OrderId);
            if (fsm != null)
                return fsm;

            // Tier 3: O(N) scan (last resort)
            fsm = ResolveFsm_ByScan(evt.AccountAlias, evt.OrderId);
            return fsm;
        }

        /// <summary>
        /// Validates FSM event preconditions: FSM resolution and metadata guard.
        /// Returns false if event should be ignored (FSM not found or guard failed).
        /// </summary>
        private bool ValidateFsmEventPreconditions(AccountEvent evt, out FollowerBracketFSM fsm)
        {
            fsm = ResolveFsmFromEvent(evt);
            if (fsm == null)
                return false;
            if (!MetadataGuardFsmEvent(evt, fsm))
                return false;
            return true;
        }

        /// <summary>
        /// Transitions FSM to Accepted state if currently Submitted or PendingSubmit.
        /// No-op if FSM is in any other state (idempotent).
        /// </summary>
        private void TransitionToAccepted(FollowerBracketFSM fsm)
        {
            if (fsm.State == FollowerBracketState.Submitted || fsm.State == FollowerBracketState.PendingSubmit)
                fsm.State = FollowerBracketState.Accepted;
        }

        /// <summary>
        /// Transitions FSM to Cancelled state, with special handling for Replace-cycle cancels.
        /// If FSM is in Replacing state and the cancelled order matches ReplacingCancelOrderId,
        /// the cancel is absorbed (FSM stays Replacing). Otherwise, transitions to Cancelled.
        /// </summary>
        private void TransitionToCancelled(AccountEvent evt, FollowerBracketFSM fsm)
        {
            if (
                fsm.State == FollowerBracketState.Replacing
                && string.Equals(fsm.ReplacingCancelOrderId, evt.OrderId, StringComparison.Ordinal)
            )
            {
                Print("[FSM-C2] Replace-cycle cancel absorbed -- FSM stays Replacing");
            }
            else
            {
                fsm.State = FollowerBracketState.Cancelled;
            }
        }

        /// <summary>
        /// Transitions FSM to Rejected state and captures broker error message.
        /// Terminal state - no further transitions possible.
        /// </summary>
        private void TransitionToRejected(AccountEvent evt, FollowerBracketFSM fsm)
        {
            fsm.State = FollowerBracketState.Rejected;
            fsm.LastBrokerError = evt.ErrorMessage;
        }

        /// <summary>
        /// Logs FSM state transitions for Shadow Mode observability.
        /// Updates LastUpdateUtc timestamp when state changes.
        /// No-op if state unchanged (idempotent).
        /// </summary>
        private void LogFsmTransition(FollowerBracketFSM fsm, FollowerBracketState oldState, AccountEvent evt)
        {
            if (fsm.State != oldState)
            {
                fsm.LastUpdateUtc = DateTime.UtcNow;
                Print(
                    string.Format(
                        "[FSM-SHADOW] {0} Transition: {1} -> {2} | Event={3} | Order={4}",
                        fsm.EntryName,
                        oldState,
                        fsm.State,
                        evt.NewState,
                        evt.SignalName
                    )
                );
            }
        }

        // ---------------------------------------------------------------------------
        // W7-065 / W7-120: HandleFsmFilled helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns true when the signal name matches a stop-order fill prefix.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsStopSignal(string name) =>
            !string.IsNullOrEmpty(name) && (name.StartsWith("Stop_") || name.StartsWith("S_"));

        /// <summary>
        /// Returns true when the signal name matches any of the five target-order fill prefixes.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsTargetSignal(string name) =>
            !string.IsNullOrEmpty(name)
            && (
                name.StartsWith("T1_")
                || name.StartsWith("T2_")
                || name.StartsWith("T3_")
                || name.StartsWith("T4_")
                || name.StartsWith("T5_")
            );

        // ---------------------------------------------------------------------------
        // W7-102: FillSignalKind classifier
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Classifies a fill signal name into a FillSignalKind enum value.
        /// Replaces stringly-typed StartsWith checks at the call site.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FillSignalKind ClassifyFillSignalType(string signalName)
        {
            if (IsStopSignal(signalName))
                return FillSignalKind.Stop;
            if (IsTargetSignal(signalName))
                return FillSignalKind.Target;
            return FillSignalKind.Entry;
        }

        /// <summary>
        /// Decrements remaining contracts and transitions FSM state to Filled or Active.
        /// Called only for Stop and Target fill events (caller contract).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ApplyFillContracts(FollowerBracketFSM fsm, int filledQty)
        {
            fsm.RemainingContracts = Math.Max(0, fsm.RemainingContracts - Math.Max(0, filledQty));
            fsm.State = fsm.RemainingContracts <= 0 ? FollowerBracketState.Filled : FollowerBracketState.Active;
        }

        /// <summary>
        /// Handles Filled/PartFilled events with stop/target detection and contract tracking.
        /// Updates FSM state based on remaining contracts after fill.
        /// </summary>
        private void HandleFsmFilled(AccountEvent evt, FollowerBracketFSM fsm)
        {
            FillSignalKind kind = ClassifyFillSignalType(evt.SignalName);

            if (kind == FillSignalKind.Stop || kind == FillSignalKind.Target)
            {
                ApplyFillContracts(fsm, evt.FilledQty);
            }
            else if (fsm.State == FollowerBracketState.Accepted || fsm.State == FollowerBracketState.Submitted)
            {
                // Entry filled -> Bracket is now ACTIVE
                fsm.State = FollowerBracketState.Active;
            }
        }

        // W9-L8-002: Dictionary dispatch -- replaces switch in ProcessBracketEvent.
        // static readonly = immutable after class init; thread-safe for concurrent reads (no lock).
        // Action<V12_002, AccountEvent, FollowerBracketFSM>: explicit self avoids closure capture
        // in static field initializer (partial class pattern, per lock-free-patterns.md).
        private static readonly Dictionary<
            OrderState,
            Action<V12_002, AccountEvent, FollowerBracketFSM>
        > _bracketDispatch = new Dictionary<OrderState, Action<V12_002, AccountEvent, FollowerBracketFSM>>
        {
            { OrderState.Accepted, (self, e, f) => self.TransitionToAccepted(f) },
            { OrderState.Working, (self, e, f) => self.TransitionToAccepted(f) },
            { OrderState.Filled, (self, e, f) => self.HandleFsmFilled(e, f) },
            { OrderState.PartFilled, (self, e, f) => self.HandleFsmFilled(e, f) },
            { OrderState.Cancelled, (self, e, f) => self.TransitionToCancelled(e, f) },
            { OrderState.Rejected, (self, e, f) => self.TransitionToRejected(e, f) },
        };

        /// <summary>
        /// Core FSM transition logic. Driven exclusively by broker confirmations.
        /// Shadow Mode: Observes reality and logs divergences.
        /// </summary>
        private void ProcessBracketEvent(AccountEvent evt)
        {
            if (!ValidateFsmEventPreconditions(evt, out FollowerBracketFSM fsm))
                return;

            FollowerBracketState oldState = fsm.State;

            if (_bracketDispatch.TryGetValue(evt.NewState, out var handler))
                handler(this, evt, fsm);

            LogFsmTransition(fsm, oldState, evt);
        }

        // ---------------------------------------------------------------------------
        // W7-069: GetFsmExpectedPosition helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns true when a FollowerBracketState is non-terminal (contributes to expected position).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsActiveFollowerState(FollowerBracketState state)
        {
            return state switch
            {
                FollowerBracketState.Active
                or FollowerBracketState.Accepted
                or FollowerBracketState.Submitted
                or FollowerBracketState.PendingSubmit
                or FollowerBracketState.Replacing
                or FollowerBracketState.Modifying => true,
                _ => false,
            };
        }

        /// <summary>
        /// Computes the signed quantity contribution of an entry order.
        /// Caller contract: entryOrder != null (guarded at call site).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ComputeEntrySignedQuantity(Order entryOrder)
        {
            int sign =
                (entryOrder.OrderAction == OrderAction.Buy || entryOrder.OrderAction == OrderAction.BuyToCover)
                    ? 1
                    : -1;
            return entryOrder.Quantity * sign;
        }

        /// <summary>
        /// Computes the net expected position for a given account by summing all
        /// non-terminal FollowerBracketFSMs. This is the SOLE authority for
        /// follower expected position (Build 1105).
        /// Master account does NOT use FSMs -- use expectedPositions dict for master.
        /// </summary>
        private int GetFsmExpectedPosition(string accountName)
        {
            int sum = 0;
            foreach (var kvp in _followerBrackets)
            {
                FollowerBracketFSM f = kvp.Value;
                if (f == null || f.AccountName != accountName)
                    continue;
                if (!IsActiveFollowerState(f.State))
                    continue;
                if (f.EntryOrder != null)
                    sum += ComputeEntrySignedQuantity(f.EntryOrder);
                else if (f.State == FollowerBracketState.Active)
                {
                    // Hydrated Active FSM -- caller handles fallback to broker position
                }
            }
            return sum;
        }

        #endregion
    }
}
