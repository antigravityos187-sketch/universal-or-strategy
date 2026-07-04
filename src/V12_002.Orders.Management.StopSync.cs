// Build 971: Orders.Management.StopSync -- RefreshActivePositionOrders, UpdateStopQuantity, CreateNewStopOrder, RestoreCascadedTargets, ValidateStopPrice [Build 971] Group >400 lines -- future refactor candidate
// V12 Orders.Management Module (Extracted)
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
        #region Orders Management Stop Sync

        private void RefreshActivePositionOrders()
        {
            var snapshot = ValidateAndSnapshotPositions();
            if (snapshot == null)
                return;

            int refreshed = 0;
            foreach (var kvp in snapshot)
            {
                string entryName = kvp.Key;
                PositionInfo pos = kvp.Value;

                for (int targetNum = 1; targetNum <= 5; targetNum++)
                    RefreshActivePositionOrders_ProcessTarget(entryName, pos, targetNum, ref refreshed);
            }

            Print(
                string.Format(
                    "[SYNC_ALL] Complete. Positions scanned: {0} | Actions taken: {1}",
                    snapshot.Count,
                    refreshed
                )
            );
        }

        // [Phase 7 OVERRUN] Extracted: inner loop body of RefreshActivePositionOrders (CYC 13->4)
        // Handles one target slot: guard checks, ChangePending skip, runner vs limit dispatch.
        private void RefreshActivePositionOrders_ProcessTarget(
            string entryName,
            PositionInfo pos,
            int targetNum,
            ref int refreshed
        )
        {
            if (IsTargetFilled(pos, targetNum))
                return;

            int targetQty = GetTargetContracts(pos, targetNum);
            if (targetQty <= 0)
                return;

            var targetDict = GetTargetOrdersDictionary(targetNum);
            if (targetDict == null)
                return;

            Order existingOrder;
            targetDict.TryGetValue(entryName, out existingOrder);

            if (RefreshActivePositionOrders_IsChangePending(existingOrder, targetNum, entryName))
                return;

            bool hasWorkingOrder = RefreshActivePositionOrders_IsWorking(existingOrder);

            if (IsRunnerTarget(targetNum))
            {
                SyncRunnerTarget(entryName, pos, targetNum, targetDict, existingOrder, ref refreshed);
                return;
            }

            SyncLimitTarget(
                entryName,
                pos,
                targetNum,
                targetQty,
                targetDict,
                existingOrder,
                hasWorkingOrder,
                ref refreshed
            );
        }

        private bool RefreshActivePositionOrders_IsChangePending(Order order, int targetNum, string entryName)
        {
            if (order == null)
                return false;
            if (order.OrderState != OrderState.ChangePending)
                return false;
            Print(string.Format("[SYNC_ALL] T{0} {1}: ChangePending -- skipping", targetNum, entryName));
            return true;
        }

        private bool RefreshActivePositionOrders_IsWorking(Order order) =>
            order != null && (order.OrderState == OrderState.Working || order.OrderState == OrderState.Accepted);

        private List<KeyValuePair<string, PositionInfo>> ValidateAndSnapshotPositions()
        {
            if (activePositions == null || activePositions.IsEmpty)
            {
                Print("[SYNC_ALL] No active positions to refresh.");
                return null;
            }

            List<KeyValuePair<string, PositionInfo>> snapshot = activePositions.ToList();
            List<KeyValuePair<string, PositionInfo>> filtered = new List<KeyValuePair<string, PositionInfo>>();

            foreach (var kvp in snapshot)
            {
                PositionInfo pos = kvp.Value;

                if (!pos.EntryFilled || pos.RemainingContracts <= 0)
                    continue;

                if (pos.IsFollower)
                {
                    Print(string.Format("[SYNC_ALL] Skipping follower position {0}", kvp.Key));
                    continue;
                }

                filtered.Add(kvp);
            }

            return filtered;
        }

        private void SyncRunnerTarget(
            string entryName,
            PositionInfo pos,
            int targetNum,
            ConcurrentDictionary<string, Order> targetDict,
            Order existingOrder,
            ref int refreshed
        )
        {
            bool hasWorkingOrder =
                existingOrder != null
                && (existingOrder.OrderState == OrderState.Working || existingOrder.OrderState == OrderState.Accepted);

            if (!hasWorkingOrder)
                return;

            try
            {
                CancelOrderSafe(existingOrder, pos);
                // B957: Do NOT TryRemove from targetDict here -- the cancel is async.
                // The broker-confirmed terminal callback will perform the removal under stateLock
                // once confirmed, preventing premature cleanup before the cancel is acknowledged.
                Print(
                    string.Format(
                        "[SYNC_ALL] T{0} {1}: Limit cancel requested -> now Runner (awaiting broker confirm)",
                        targetNum,
                        entryName
                    )
                );
                refreshed++;
            }
            catch (Exception ex)
            {
                Print(
                    string.Format("[SYNC_ALL] T{0} {1}: CancelOrder failed -- {2}", targetNum, entryName, ex.Message)
                );
            }
        }

        private void SetTargetPrice(PositionInfo pos, int targetNum, double price)
        {
            switch (targetNum)
            {
                case 1:
                    pos.Target1Price = price;
                    break;
                case 2:
                    pos.Target2Price = price;
                    break;
                case 3:
                    pos.Target3Price = price;
                    break;
                case 4:
                    pos.Target4Price = price;
                    break;
                case 5:
                    pos.Target5Price = price;
                    break;
                default:
                    break;
            }
        }

        private void SyncLimitTarget_Reprice(
            string entryName,
            PositionInfo pos,
            int targetNum,
            Order existingOrder,
            double newPrice,
            ref int refreshed
        )
        {
            if (Math.Abs(existingOrder.LimitPrice - newPrice) < tickSize)
            {
                Print(
                    string.Format(
                        "[SYNC_ALL] T{0} {1}: Price unchanged at {2:F2} -- no action",
                        targetNum,
                        entryName,
                        newPrice
                    )
                );
                return;
            }

            try
            {
                ChangeOrder(existingOrder, existingOrder.Quantity, newPrice, 0);
                SetTargetPrice(pos, targetNum, newPrice);
                Print(string.Format("[SYNC_ALL] T{0} {1}: Repriced -> {2:F2}", targetNum, entryName, newPrice));
                refreshed++;
            }
            catch (Exception ex)
            {
                Print(
                    string.Format("[SYNC_ALL] T{0} {1}: ChangeOrder failed -- {2}", targetNum, entryName, ex.Message)
                );
            }
        }

        private void SyncLimitTarget_Submit(
            string entryName,
            PositionInfo pos,
            int targetNum,
            int targetQty,
            ConcurrentDictionary<string, Order> targetDict,
            double newPrice,
            ref int refreshed
        )
        {
            OrderAction exitAction = pos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;
            try
            {
                Order newLimit = SubmitOrderUnmanaged(
                    0,
                    exitAction,
                    OrderType.Limit,
                    targetQty,
                    newPrice,
                    0,
                    "",
                    "T" + targetNum + "_" + entryName
                );

                if (newLimit != null)
                {
                    targetDict[entryName] = newLimit;
                    SetTargetPrice(pos, targetNum, newPrice);
                    Print(
                        string.Format(
                            "[SYNC_ALL] T{0} {1}: New limit submitted @ {2:F2} qty={3}",
                            targetNum,
                            entryName,
                            newPrice,
                            targetQty
                        )
                    );
                    refreshed++;
                }
                else
                {
                    Print(
                        string.Format(
                            "[SYNC_ALL] T{0} {1}: SubmitOrderUnmanaged returned null @ {2:F2}",
                            targetNum,
                            entryName,
                            newPrice
                        )
                    );
                }
            }
            catch (Exception ex)
            {
                Print(string.Format("[SYNC_ALL] T{0} {1}: Submit failed -- {2}", targetNum, entryName, ex.Message));
            }
        }

        private void SyncLimitTarget(
            string entryName,
            PositionInfo pos,
            int targetNum,
            int targetQty,
            ConcurrentDictionary<string, Order> targetDict,
            Order existingOrder,
            bool hasWorkingOrder,
            ref int refreshed
        )
        {
            // Build 1102Y [P-06]: Role-aware reprice -- RMA/SIMA positions use stamped role; others use slot-based.
            double newPrice = CalculateTargetPriceFromPos(pos.Direction, pos.EntryPrice, pos, targetNum);
            if (newPrice <= 0)
            {
                Print(
                    string.Format(
                        "[SYNC_ALL] T{0} {1}: Calculated price invalid ({2:F2}) -- skipped",
                        targetNum,
                        entryName,
                        newPrice
                    )
                );
                return;
            }

            if (hasWorkingOrder)
                SyncLimitTarget_Reprice(entryName, pos, targetNum, existingOrder, newPrice, ref refreshed);
            else
                SyncLimitTarget_Submit(entryName, pos, targetNum, targetQty, targetDict, newPrice, ref refreshed);
        }

        /// <summary>
        /// [Phase 7 NEW-2] Helper: Handle stale pending replacement detection and purge
        /// Extracted from UpdateStopQuantity to reduce complexity (CYC 25->15)
        /// </summary>
        /// <returns>True if stale pending was purged and should re-initiate, False if updated existing pending</returns>
        private bool UpdateStopQuantity_HandleStalePending(
            string entryName,
            PendingStopReplacement existingPendingQty,
            int remainingContracts
        )
        {
            // Build 1104.2: Staleness fast-path -- purge stale pending and re-initiate
            // Fix #1: Cache DateTime.UtcNow for determinism (Jane Street: Microsecond Latency)
            DateTime now = DateTime.UtcNow;
            double pendingAgeSeconds = (now - existingPendingQty.CreatedTime).TotalSeconds;
            if (pendingAgeSeconds > STALE_PENDING_FAST_PATH_SEC)
            {
                if (pendingStopReplacements.TryRemove(entryName, out _))
                    Interlocked.Decrement(ref pendingReplacementCount);
                Print(
                    string.Format(
                        "[1104.2] Stale pending purged for {0} ({1:F1}s). Re-initiating stop resize.",
                        entryName,
                        pendingAgeSeconds
                    )
                );
                return true; // Signal to re-initiate
            }
            else
            {
                // V12 Round 11: Immutable struct reassignment pattern (readonly struct requires new instance)
                var updatedPending = new PendingStopReplacement
                {
                    EntryName = existingPendingQty.EntryName,
                    Quantity = remainingContracts, // Updated quantity
                    StopPrice = existingPendingQty.StopPrice,
                    Direction = existingPendingQty.Direction,
                    OldOrder = existingPendingQty.OldOrder,
                    CreatedTime = existingPendingQty.CreatedTime,
                    CapturedTargets = existingPendingQty.CapturedTargets,
                    BracketRestorationNeeded = existingPendingQty.BracketRestorationNeeded,
                };
                pendingStopReplacements[entryName] = updatedPending; // Reassign to dictionary
                Print(
                    string.Format(
                        "V8.31: Updated existing pending replacement for {0} to {1} contracts",
                        entryName,
                        remainingContracts
                    )
                );
                return false; // Signal early return
            }
        }

        /// <summary>
        /// [Phase 7 NEW-2] Helper: Create and store pending replacement info
        /// Extracted from UpdateStopQuantity to reduce complexity (CYC 25->15)
        /// </summary>
        private void UpdateStopQuantity_CreateReplacement(
            string entryName,
            int remainingContracts,
            double currentStopPrice,
            MarketPosition direction,
            Order currentStop
        )
        {
            // Store the replacement info
            var newPending = new PendingStopReplacement
            {
                EntryName = entryName,
                Quantity = remainingContracts,
                StopPrice = currentStopPrice,
                Direction = direction,
                OldOrder = currentStop,
                CreatedTime = DateTime.UtcNow, // V8.31: Added for timeout support
            };

            // V8.31: Thread-safe add
            if (pendingStopReplacements.TryAdd(entryName, newPending))
            {
                Interlocked.Increment(ref pendingReplacementCount);
            }
        }

        /// <summary>
        /// [Phase 7 NEW-2] Helper: Cancel old stop and print replacement info
        /// Extracted from UpdateStopQuantity to reduce complexity (CYC 25->15)
        /// </summary>
        private void UpdateStopQuantity_CancelAndReplace(string entryName, Order currentStop, PositionInfo pos)
        {
            // Cancel old stop - replacement will be created in OnOrderUpdate when confirmed
            CancelOrderForReplace(currentStop, pos);
            Print(
                string.Format(
                    "STOP CANCEL PENDING: {0} | Will replace with {1} contracts @ {2:F2}",
                    entryName,
                    pos.RemainingContracts,
                    pos.CurrentStopPrice
                )
            );
        }

        /// <summary>
        /// [Phase 7 NEW-2 Round 7] Helper: Check if order is in active/pending state.
        /// Reduces complex conditional branches (CodeScene: 5->3 branches).
        /// </summary>
        private bool IsOrderActiveOrPending(Order order)
        {
            return order.OrderState == OrderState.Working
                || order.OrderState == OrderState.Accepted
                || order.OrderState == OrderState.ChangeSubmitted;
        }

        /// <summary>
        /// [Phase 7 NEW-2 Round 10] Helper: Check if dictionary contains active stop order.
        /// Reduces cognitive complexity (nested condition extraction).
        /// </summary>
        private bool HasActiveStopInDictionary(string entryName)
        {
            if (!stopOrders.TryGetValue(entryName, out Order stopOrder))
            {
                return false;
            }
            return IsOrderActiveOrPending(stopOrder)
                && (stopOrder.OrderType == OrderType.StopMarket || stopOrder.OrderType == OrderType.StopLimit);
        }

        /// <summary>
        /// [Phase 7 NEW-2 Round 10] Helper: Check if Account.Orders contains active stop with suffix.
        /// Reduces cognitive complexity (nested loop extraction).
        /// </summary>
        private bool IsProtectiveStopOrder(Order o)
        {
            return o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit;
        }

        private bool HasActiveStopInAccountOrders(string suffix, string entryName)
        {
            string prefix = "S_" + entryName + "_";
            foreach (Order o in Account.Orders)
            {
                if (IsOrderActiveOrPending(o) && IsProtectiveStopOrder(o) && IsStopOrderForEntry(o, suffix, prefix))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// [Phase 7 NEW-2 Round 12] Helper: Check if order name matches entry stop naming patterns.
        /// Supports both legacy "_entryName" suffix and new "S_entryName_" prefix formats.
        /// Reduces complex conditional branches (CodeScene: 3->2 branches).
        /// </summary>
        private bool IsStopOrderForEntry(Order o, string suffix, string prefix)
        {
            return o.Name.EndsWith(suffix) || o.Name.StartsWith(prefix);
        }

        private void UpdateStopQuantity_HandleEmergencyFlatten(string entryName, int remainingContracts)
        {
            // P0-1: GRADUATED RESPONSE - Only flatten if position truly lacks stop protection
            // Jane Street Principle #4: Fail-Fast - verify state before emergency action

            // [Round 10] Extracted nested checks to helpers (Cognitive 19->15)
            bool hasActiveStop = false;
            string suffix = string.Concat("_", entryName);

            try
            {
                hasActiveStop = HasActiveStopInDictionary(entryName) || HasActiveStopInAccountOrders(suffix, entryName);
            }
            catch
            {
                // If order enumeration fails, assume unprotected (fail-safe)
                hasActiveStop = false;
            }

            if (!hasActiveStop)
            {
                Print(
                    string.Format(
                        "(!) POSITION UNPROTECTED: {0} contracts - emergency flatten required",
                        remainingContracts
                    )
                );

                // Attempt emergency flatten to protect the position
                try
                {
                    FlattenPositionByName(entryName);
                }
                catch (Exception flatEx)
                {
                    Print(
                        string.Format(
                            "(!) CRITICAL: Emergency flatten also failed for {0}: {1}",
                            entryName,
                            flatEx.ToString()
                        )
                    );
                }
            }
            else
            {
                Print(
                    string.Format(
                        "(!) Active stop still protecting {0} - quirk was transient, no flatten needed",
                        entryName
                    )
                );
            }
        }

        /// <summary>
        /// Updates the stop order quantity after a partial target fill.
        /// </summary>
        /// <remarks>
        /// V12.Audit [C-08]: Callers MUST ensure the <paramref name="pos"/> reference is
        /// obtained from the NinjaTrader dispatch thread or from within a callback that is
        /// already serialized by that actor. Passing a stale <paramref name="pos"/> can
        /// result in the stop being undersized relative to actual remaining contracts.
        /// DO NOT use lock(stateLock) for internal logic - this pattern is BANNED.
        /// </remarks>
        /// <summary>
        /// [Phase 7 NEW-2 Round 10] Helper: Validate preconditions for stop quantity update.
        /// Reduces cognitive complexity (early return pattern extraction).
        /// </summary>
        private bool ShouldSkipStopQuantityUpdate(string entryName, PositionInfo pos)
        {
            if (!stopOrders.TryGetValue(entryName, out _))
            {
                return true;
            }
            if (pos.RemainingContracts <= 0)
            {
                return true;
            }
            // V12.41: No trailing/updates before entry fill is confirmed
            if (!pos.EntryFilled)
            {
                return true;
            }
            return false;
        }

        private void UpdateStopQuantity(string entryName, PositionInfo pos)
        {
            // V12.Hardening [RISK-01]: Atomic update guard
            // Actor/dispatch-thread serialization prevents dirty reads of pos.RemainingContracts
            if (ShouldSkipStopQuantityUpdate(entryName, pos))
                return;

            // [Phase 7 OVERRUN] Extracted: try/catch body to reduce CYC
            UpdateStopQuantity_Execute(entryName, pos);
        }

        // [Phase 7 OVERRUN] Extracted: core execution body of UpdateStopQuantity
        private void UpdateStopQuantity_Execute(string entryName, PositionInfo pos)
        {
            try
            {
                Order currentStop = stopOrders[entryName];

                // V8.11 FIX: Store pending replacement BEFORE cancelling
                // This ensures we only create a new stop when the old one is confirmed cancelled
                if (
                    currentStop != null
                    && (currentStop.OrderState == OrderState.Working || currentStop.OrderState == OrderState.Accepted)
                )
                {
                    // V8.31: Check if there's already a pending replacement to prevent duplicates
                    if (pendingStopReplacements.TryGetValue(entryName, out var existingPendingQty))
                    {
                        // [Phase 7 NEW-2] Extracted: Handle stale pending detection
                        bool shouldReInitiate = UpdateStopQuantity_HandleStalePending(
                            entryName,
                            existingPendingQty,
                            pos.RemainingContracts
                        );
                        if (!shouldReInitiate)
                            return;
                    }

                    // [Phase 7 NEW-2] Extracted: Create replacement info
                    UpdateStopQuantity_CreateReplacement(
                        entryName,
                        pos.RemainingContracts,
                        pos.CurrentStopPrice,
                        pos.Direction,
                        currentStop
                    );

                    // [Phase 7 NEW-2] Extracted: Cancel and print
                    UpdateStopQuantity_CancelAndReplace(entryName, currentStop, pos);
                }
                else
                {
                    // No existing stop to cancel, create new one directly
                    // V12.41: Pass the entry name for stricter validation
                    CreateNewStopOrder(entryName, pos.RemainingContracts, pos.CurrentStopPrice, pos.Direction);
                }
            }
            catch (InvalidOperationException ex) when (IsKnownNtOrderException(ex))
            {
                UpdateStopQuantity_HandleCatchCleanup(
                    entryName,
                    pos.RemainingContracts,
                    string.Format("(!) WARNING UpdateStopQuantity for {0} (known quirk): {1}", entryName, ex.Message)
                );
            }
            catch (Exception ex)
            {
                // Do NOT rethrow - position safety requires stop order attempt to complete
                UpdateStopQuantity_HandleCatchCleanup(
                    entryName,
                    pos.RemainingContracts,
                    string.Format("(!) CRITICAL UpdateStopQuantity for {0}: {1}", entryName, ex.ToString())
                );
            }
        }

        // [Phase 7 OVERRUN] Extracted: when-filter predicate for known NT order operation exceptions
        private static bool IsKnownNtOrderException(InvalidOperationException ex) =>
            ex.Message.Contains("SubmitOrderUnmanaged")
            || ex.Message.Contains("CreateOrder")
            || ex.Message.Contains("CancelOrder");

        // [Phase 7 OVERRUN] Extracted: shared catch-block cleanup for UpdateStopQuantity
        // Removes orphaned pendingStopReplacements entry, decrements counter, prints log, triggers emergency flatten.
        private void UpdateStopQuantity_HandleCatchCleanup(string entryName, int remainingContracts, string logMessage)
        {
            // P0-3: Clean orphaned pendingStopReplacements entry (Jane Street Principle #1: Correctness by Construction)
            if (pendingStopReplacements.TryRemove(entryName, out _))
                Interlocked.Decrement(ref pendingReplacementCount);

            Print(logMessage);

            UpdateStopQuantity_HandleEmergencyFlatten(entryName, remainingContracts);
        }

        // V8.11: Helper method to create a new stop order
        // V8.31: Added guard to prevent duplicate stop creation
        private void CreateNewStopOrder(
            string entryName,
            int quantity,
            double stopPrice,
            MarketPosition direction,
            bool isRecovery = false
        )
        {
            try
            {
                // Phase 1: Validate preconditions (zombie guard, duplicate stop guard, recovery mode)
                var (canProceed, pos) = ValidateStopOrderPreconditions(entryName, quantity, direction, isRecovery);
                if (!canProceed)
                    return;

                // Phase 2: Submit to broker (fleet vs local routing, OCO linking)
                Order newStop = SubmitStopOrderToBroker(entryName, quantity, stopPrice, direction, pos);
                if (newStop == null)
                {
                    CreateNewStopOrder_HandleNullResult(entryName, quantity, stopPrice, direction);
                    return;
                }

                // A1-1: B966 -- Enqueue actor pipeline (was naked stateLock write)
                var _en966 = entryName;
                var _ns966 = newStop;
                Enqueue(ctx =>
                {
                    ctx.stopOrders[_en966] = _ns966;
                });

                CreateNewStopOrder_LogLatencyAndSuccess(entryName, quantity, stopPrice, newStop);
            }
            catch (InvalidOperationException ex)
                when (ex.Message.Contains("SubmitOrderUnmanaged") || ex.Message.Contains("CreateOrder"))
            {
                Print(
                    string.Format("(!) WARNING CreateNewStopOrder for {0} (known quirk): {1}", entryName, ex.Message)
                );
                CreateNewStopOrder_FlattenSafe(entryName);
            }
            catch (Exception ex)
            {
                Print(string.Format("(!) CRITICAL CreateNewStopOrder for {0}: {1}", entryName, ex.ToString()));
                CreateNewStopOrder_FlattenSafe(entryName);
                // Do NOT rethrow - position safety requires stop order attempt to complete
            }
        }

        // [Phase 7 OVERRUN] Extracted: null stop result handling
        private void CreateNewStopOrder_HandleNullResult(
            string entryName,
            int quantity,
            double stopPrice,
            MarketPosition direction
        )
        {
            Print(string.Format("(!) CRITICAL ERROR: Stop order submission returned NULL for {0}!", entryName));
            Print(
                string.Format(
                    "(!) POSITION UNPROTECTED: {0} {1} contracts @ {2:F2}",
                    direction == MarketPosition.Long ? "LONG" : "SHORT",
                    quantity,
                    stopPrice
                )
            );
            Print(string.Format("(!) Attempting emergency flatten for {0}...", entryName));
            FlattenPositionByName(entryName);
        }

        // [Phase 7 OVERRUN] Extracted: latency audit log + success log
        private void CreateNewStopOrder_LogLatencyAndSuccess(
            string entryName,
            int quantity,
            double stopPrice,
            Order newStop
        )
        {
            if (pendingStopReplacements.TryGetValue(entryName, out var pendingForLatency))
            {
                double ocoLatencyMs = (DateTime.Now - pendingForLatency.CreatedTime).TotalMilliseconds;
                Print(
                    string.Format(
                        "[LATENCY_AUDIT] Target Fill -> Stop Cancel Delta: {0:F1}ms (Entry: {1})",
                        ocoLatencyMs,
                        entryName
                    )
                );
            }
            Print(
                string.Format(
                    "STOP QTY UPDATED: {0} contracts @ {1:F2} (Order: {2})",
                    quantity,
                    stopPrice,
                    newStop.Name
                )
            );
        }

        // [Phase 7 OVERRUN] Extracted: safe flatten with catch for both catch blocks in CreateNewStopOrder
        private void CreateNewStopOrder_FlattenSafe(string entryName)
        {
            Print(string.Format("(!) Attempting emergency flatten for {0} due to stop creation failure...", entryName));
            try
            {
                FlattenPositionByName(entryName);
            }
            catch (Exception flatEx)
            {
                Print(
                    string.Format(
                        "(!) CRITICAL: Emergency flatten also failed for {0}: {1}",
                        entryName,
                        flatEx.ToString()
                    )
                );
            }
            // Do NOT rethrow - position safety requires stop order attempt to complete
        }

        /// <summary>
        /// Validates preconditions for stop order creation: zombie guard, duplicate stop guard, recovery mode.
        /// </summary>
        /// <returns>
        /// Tuple: (canProceed, pos)
        /// - canProceed: false if any guard blocks creation, true if validation passes
        /// - pos: The validated PositionInfo (needed for broker routing)
        /// </returns>
        private (bool canProceed, PositionInfo pos) ValidateStopOrderPreconditions(
            string entryName,
            int quantity,
            MarketPosition direction,
            bool isRecovery
        )
        {
            // V12.41 ZOMBIE GUARD: Block stop creation if position is flat or entry not filled
            if (activePositions.TryGetValue(entryName, out var targetPos))
            {
                if (targetPos.RemainingContracts <= 0)
                {
                    Print(
                        string.Format(
                            "[STOP_GUARD] BLOCKED zombie stop for {0} - Position is FLAT (Remaining=0)",
                            entryName
                        )
                    );
                    return (false, null);
                }
                if (!targetPos.EntryFilled)
                {
                    Print(string.Format("[STOP_GUARD] BLOCKED early stop for {0} - Fill not yet confirmed", entryName));
                    return (false, null);
                }
            }
            else
            {
                Print(string.Format("[STOP_GUARD] BLOCKED orphan stop for {0} - No tracking record found", entryName));
                return (false, null);
            }

            // V12.Phase7 [C-06]: Check if any live stop already exists for this entry (Working, Accepted,
            // ChangePending, or ChangeSubmitted). Without ChangePending guard, a ChangeOrder in flight
            // causes a second stop to be created -- leading to stacked stops that can reverse the position.
            if (stopOrders.TryGetValue(entryName, out var existingStop))
            {
                if (
                    existingStop != null
                    && (
                        existingStop.OrderState == OrderState.Working
                        || existingStop.OrderState == OrderState.Accepted
                        || existingStop.OrderState == OrderState.ChangePending
                        || existingStop.OrderState == OrderState.ChangeSubmitted
                    )
                )
                {
                    if (isRecovery)
                    {
                        // Build 1104.2: Recovery mode -- stale tracked stop may be phantom at broker.
                        // Force-cancel and clear reference to allow fresh stop submission.
                        Print(
                            string.Format(
                                "[1104.2] Recovery: force-cancelling phantom stop for {0} (state={1})",
                                entryName,
                                existingStop.OrderState
                            )
                        );
                        PositionInfo recoveryPos;
                        activePositions.TryGetValue(entryName, out recoveryPos);
                        CancelOrderSafe(existingStop, recoveryPos);
                        stopOrders.TryRemove(entryName, out _);
                    }
                    else
                    {
                        Print(
                            string.Format(
                                "V12.Phase7: SKIPPING duplicate stop for {0} -- existing stop state={1}",
                                entryName,
                                existingStop.OrderState
                            )
                        );
                        return (false, null);
                    }
                }
            }

            return (true, targetPos);
        }

        /// <summary>
        /// Submits stop order to broker with fleet vs local routing and emergency flatten on failure.
        /// </summary>
        /// <returns>Order object or null if submission fails</returns>
        private Order SubmitStopOrderToBroker(
            string entryName,
            int quantity,
            double stopPrice,
            MarketPosition direction,
            PositionInfo pos
        )
        {
            // V12.Phase7 [C-04]: Round stop price to valid tick boundary.
            // CreateNewStopOrder receives raw prices that may not be tick-aligned.
            // Off-tick prices are rejected by the broker, leaving the position unprotected.
            stopPrice = Instrument.MasterInstrument.RoundToTickSize(stopPrice);

            Order newStop = null;
            OrderAction exitAction = direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;

            // V12.3: Route to correct account (fleet follower vs local)
            if (pos.IsFollower && pos.ExecutingAccount != null)
            {
                // Build 950: Re-link replacement stop to broker OCO bracket.
                string _b950OcoId = pos.OcoGroupId ?? string.Empty;

                // Fleet follower: use Account API
                string sigName = "S_" + entryName;
                if (sigName.Length > 50)
                    sigName = sigName.Substring(0, 50);

                newStop = pos.ExecutingAccount.CreateOrder(
                    Instrument,
                    exitAction,
                    OrderType.StopMarket,
                    TimeInForce.Gtc,
                    quantity,
                    0,
                    stopPrice,
                    _b950OcoId,
                    sigName,
                    null
                );

                // B957: Guard against null CreateOrder and Submit throws to prevent unprotected position.
                if (newStop == null)
                {
                    Print(
                        string.Format("[STOP_GUARD] CreateOrder returned null for follower {0}. Flattening.", entryName)
                    );
                    FlattenPositionByName(entryName);
                    return null;
                }

                try
                {
                    pos.ExecutingAccount.Submit(new[] { newStop });
                }
                catch (Exception submitEx)
                {
                    Print(
                        string.Format(
                            "[STOP_GUARD] Submit threw for follower {0}: {1}. Flattening.",
                            entryName,
                            submitEx.Message
                        )
                    );
                    FlattenPositionByName(entryName);
                    return null;
                }
            }
            else
            {
                // Build 950: Re-link replacement stop to broker OCO bracket.
                string _b950OcoId = pos.OcoGroupId ?? string.Empty;

                // Local: use SubmitOrderUnmanaged with truncated signal name
                string suffix = (DateTime.Now.Ticks % 100000000).ToString();
                string sigName = "S_" + entryName + "_" + suffix;
                if (sigName.Length > 50)
                    sigName = sigName.Substring(0, 50);

                newStop = SubmitOrderUnmanaged(
                    0,
                    exitAction,
                    OrderType.StopMarket,
                    quantity,
                    0,
                    stopPrice,
                    _b950OcoId,
                    sigName
                );
            }

            return newStop;
        }

        // Build 950: Re-submit profit targets that were OCO-cascade-cancelled during stop replacement.
        // Runs on strategy thread via TriggerCustomEvent. Checks Order.OrderState directly on the
        // captured Order object -- avoids dict-timing races with RemoveGhostOrderRef.
        private bool TryLoadActivePosition(string entryName, TargetSnapshot[] capturedTargets, out PositionInfo pos)
        {
            pos = null;
            if (capturedTargets == null || capturedTargets.Length == 0)
                return false;
            if (!activePositions.TryGetValue(entryName, out pos))
                return false;
            if (!pos.EntryFilled)
                return false;
            if (pos.RemainingContracts <= 0)
                return false;
            return true;
        }

        private static bool ShouldRestoreTarget(TargetSnapshot snap)
        {
            if (snap == null)
                return false;
            if (snap.CapturedOrder == null)
                return false;
            if (snap.CapturedOrder.OrderState == OrderState.Cancelled)
                return true;
            if (snap.CapturedOrder.OrderState == OrderState.Rejected)
                return true;
            return false;
        }

        private Order SubmitFollowerTarget(
            string entryName,
            TargetSnapshot snap,
            OrderAction exitAction,
            double restoredPrice,
            string bracketOcoId,
            Account executingAccount
        )
        {
            string tSig = SymmetryTrim("T" + snap.TargetNum + "_" + entryName, 40);
            Order tOrd = executingAccount.CreateOrder(
                Instrument,
                exitAction,
                OrderType.Limit,
                TimeInForce.Gtc,
                snap.Qty,
                restoredPrice,
                0,
                bracketOcoId,
                tSig,
                null
            );
            if (tOrd == null)
                return null;
            executingAccount.Submit(new[] { tOrd });
            return tOrd;
        }

        private Order SubmitLeaderTarget(
            TargetSnapshot snap,
            OrderAction exitAction,
            double restoredPrice,
            string bracketOcoId
        )
        {
            string tSig = "T" + snap.TargetNum + "_" + snap.TargetNum;
            return SubmitOrderUnmanaged(0, exitAction, OrderType.Limit, snap.Qty, restoredPrice, 0, bracketOcoId, tSig);
        }

        private void RestoreCascadedTargets(string entryName, TargetSnapshot[] capturedTargets)
        {
            PositionInfo pos;
            if (!TryLoadActivePosition(entryName, capturedTargets, out pos))
                return;

            OrderAction exitAction = pos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;
            string bracketOcoId = pos.OcoGroupId ?? string.Empty;

            foreach (TargetSnapshot snap in capturedTargets)
            {
                if (!ShouldRestoreTarget(snap))
                    continue;

                double restoredPrice = Instrument.MasterInstrument.RoundToTickSize(snap.Price);
                Order newTarget =
                    (pos.IsFollower && pos.ExecutingAccount != null)
                        ? SubmitFollowerTarget(
                            entryName,
                            snap,
                            exitAction,
                            restoredPrice,
                            bracketOcoId,
                            pos.ExecutingAccount
                        )
                        : SubmitLeaderTarget(snap, exitAction, restoredPrice, bracketOcoId);

                var tDict = GetTargetOrdersDictionary(snap.TargetNum);
                if (tDict == null)
                    continue;

                if (newTarget != null)
                {
                    tDict[entryName] = newTarget;
                    Print(
                        string.Format(
                            "[B950] Target T{0} restored for {1} @ {2:F2} qty={3}",
                            snap.TargetNum,
                            entryName,
                            restoredPrice,
                            snap.Qty
                        )
                    );
                }
                else
                {
                    Print(string.Format("[B950] WARN: Target T{0} restore NULL for {1}", snap.TargetNum, entryName));
                }
            }
        }

        /// <summary>
        /// Adjusts LONG stop price when it violates market safety rules.
        /// Handles BE Shield (level 1 + entryPrice) and standard adjustment paths.
        /// </summary>
        private double Validate_LongIsIllegalAdjust(
            double desiredStopPrice,
            double currentPrice,
            int level,
            double entryPrice,
            double minDistance
        )
        {
            // For BE (Level 1), only adjust if stop is STRICTLY above market (illegal).
            // Equality is allowed for BE to prevent safety pull-back on the threshold cross.
            bool isIllegal = (level == 1) ? (desiredStopPrice > currentPrice) : (desiredStopPrice >= currentPrice);

            if (isIllegal)
            {
                if (level == 1 && entryPrice > 0)
                {
                    // [Build 1102J] Entry Shield: for BE moves, clamp directly to entry price floor.
                    // Do NOT snap to current market -- that drags the stop into negative territory.
                    double resultStop = entryPrice;
                    Print(
                        string.Format(
                            "[1102J] STOP VALIDATION: BE SHIELD clamped LONG stop from {0:F2} to entry floor {1:F2}",
                            desiredStopPrice,
                            resultStop
                        )
                    );
                    return resultStop;
                }
                else
                {
                    double resultStop = currentPrice - (level == 1 ? 0 : minDistance);
                    Print(
                        string.Format(
                            "STOP VALIDATION: Adjusted LONG stop from {0:F2} to {1:F2} (Level {2} {3} market)",
                            desiredStopPrice,
                            resultStop,
                            level,
                            (level == 1 ? "above" : "at/above")
                        )
                    );
                    return resultStop;
                }
            }

            return desiredStopPrice;
        }

        /// <summary>
        /// Adjusts SHORT stop price when it violates market safety rules.
        /// Handles BE Shield (level 1 + entryPrice) and standard adjustment paths.
        /// </summary>
        private double Validate_ShortIsIllegalAdjust(
            double desiredStopPrice,
            double currentPrice,
            int level,
            double entryPrice,
            double minDistance
        )
        {
            bool isIllegal = (level == 1) ? (desiredStopPrice < currentPrice) : (desiredStopPrice <= currentPrice);

            if (isIllegal)
            {
                if (level == 1 && entryPrice > 0)
                {
                    // [Build 1102J] Entry Shield: for BE moves, clamp directly to entry price floor.
                    // Do NOT snap to current market -- that drags the stop into negative territory.
                    double resultStop = entryPrice;
                    Print(
                        string.Format(
                            "[1102J] STOP VALIDATION: BE SHIELD clamped SHORT stop from {0:F2} to entry floor {1:F2}",
                            desiredStopPrice,
                            resultStop
                        )
                    );
                    return resultStop;
                }
                else
                {
                    double resultStop = currentPrice + (level == 1 ? 0 : minDistance);
                    Print(
                        string.Format(
                            "STOP VALIDATION: Adjusted SHORT stop from {0:F2} to {1:F2} (Level {2} {3} market)",
                            desiredStopPrice,
                            resultStop,
                            level,
                            (level == 1 ? "below" : "at/below")
                        )
                    );
                    return resultStop;
                }
            }

            return desiredStopPrice;
        }

        private double ValidateStopPrice(
            MarketPosition direction,
            double desiredStopPrice,
            int level = 0,
            double entryPrice = 0
        )
        {
            // V12.41: Use real-time price instead of stale bar Close[0]
            double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
            double tickSize = Instrument.MasterInstrument.TickSize;

            // [V12.1102E] RELAXED SAFETY: For Manual BE (Level 1), allow zero-tick distance from market.
            double minDistance = (level == 1) ? 0 : (2 * tickSize);

            double resultStop =
                direction == MarketPosition.Long
                    ? Validate_LongIsIllegalAdjust(desiredStopPrice, currentPrice, level, entryPrice, minDistance)
                    : Validate_ShortIsIllegalAdjust(desiredStopPrice, currentPrice, level, entryPrice, minDistance);

            // [Build 1102H] Profit Floor: secondary backstop.
            resultStop = ValidateStopPrice_ApplyProfitFloor(direction, resultStop, level, entryPrice);

            // V12.Phase7 [C-04]: Always round to valid tick boundary before returning.
            return Instrument.MasterInstrument.RoundToTickSize(resultStop);
        }

        // [Phase 7 OVERRUN] Extracted: profit floor guard for ValidateStopPrice
        private double ValidateStopPrice_ApplyProfitFloor(
            MarketPosition direction,
            double resultStop,
            int level,
            double entryPrice
        )
        {
            if (level != 1 || entryPrice <= 0)
                return resultStop;
            if (direction == MarketPosition.Long && resultStop < entryPrice)
                return entryPrice;
            if (direction == MarketPosition.Short && resultStop > entryPrice)
                return entryPrice;
            return resultStop;
        }

        #endregion
    }
}
