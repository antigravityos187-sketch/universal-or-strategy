// V12 REAPER Repair Engine -- Re-issues missed entry orders for desynced follower accounts
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class V12_002 : Strategy
    {
        #region V12 REAPER Repair Engine

        /// <summary>
        /// V12.Phase8.2: Processes queued repair requests on the strategy thread.
        /// Re-issues the original entry order for a desynced follower account.
        /// Build 935: Per-repair logic extracted to ExecuteReaperRepair (CS-R1140 compliance).
        /// </summary>
        private void ProcessReaperRepairQueue()
        {
            string accountName;
            while (_reaperRepairQueue.TryDequeue(out accountName))
                ExecuteReaperRepair(accountName);
        }

        /// <summary>
        /// Phase7-T1: Validates repair eligibility - flatten state, PositionInfo lookup, orphan self-heal.
        /// Returns false if repair should abort.
        /// </summary>
        private bool ValidateRepairEligibility(
            string accountName,
            out PositionInfo repairPos,
            out string repairEntryName
        )
        {
            repairPos = null;
            repairEntryName = null;

            // A3-2: Abort immediately if a flatten is in progress (Build 960 audit fix)
            if (isFlattenRunning)
            {
                Print("[REAPER REPAIR] Aborted -- flatten in progress.");
                return false;
            }

            // NEW: Delegate orphan check to OrphanSafety module
            if (
                !ValidateRepairEligibility_OrphanCheck(accountName, activePositions, out repairPos, out repairEntryName)
            )
            {
                return false; // Orphan detected, self-heal triggered if threshold reached
            }

            ClearOrphanRepairCount(accountName); // NEW: Accessor method (success path)
            return true;
        }

        /// <summary>
        /// Phase7-T1: Calculates repair order prices based on OrderType.
        /// </summary>
        private void CalculateRepairOrderPrices(
            OrderType orderType,
            double entryPrice,
            out double limitPrice,
            out double stopPrice
        )
        {
            limitPrice = 0;
            stopPrice = 0;

            if (orderType == OrderType.Limit)
            {
                limitPrice = entryPrice;
            }
            else if (orderType == OrderType.StopMarket)
            {
                stopPrice = entryPrice;
            }
            else if (orderType == OrderType.StopLimit)
            {
                limitPrice = entryPrice;
                stopPrice = entryPrice;
            }
        }

        /// <summary>
        /// Phase7-T1: Validates repair risk bounds - ATR-derived hard bound + legacy Market order tick fence.
        /// Returns false if repair exceeds risk limits.
        /// </summary>
        private bool ValidateRepairRiskBounds(
            string accountName,
            OrderType orderType,
            double entryPrice,
            double currentPrice
        )
        {
            if (currentPrice <= 0)
            {
                Print($"[REAPER] REPAIR BLOCKED: invalid currentPrice={currentPrice:F4} for {accountName}.");
                return false;
            }

            if (!TryGetRepairDistanceLimitPoints(out double repairLimitPoints))
            {
                Print($"[REAPER] REPAIR BLOCKED: unable to derive repair distance bound for {accountName}.");
                return false;
            }

            double hardBoundDiff = Math.Abs(currentPrice - entryPrice);
            if (hardBoundDiff > repairLimitPoints)
            {
                Print(
                    $"[REAPER] REPAIR BLOCKED: {accountName} {orderType} exceeds hard bound. "
                        + $"Current={currentPrice:F2}, Entry={entryPrice:F2}, Diff={hardBoundDiff:F4} > Limit={repairLimitPoints:F4}."
                );
                return false;
            }

            // 2. Safety Fence: enforce only when repair submits a Market order.
            if (orderType == OrderType.Market)
            {
                // Legacy market-fence check retained as a secondary guard.
                double priceDiff = Math.Abs(currentPrice - entryPrice);
                double fenceDistance = RepairTickFence * tickSize;

                if (priceDiff > fenceDistance)
                {
                    Print(
                        $"[REAPER] REPAIR BLOCKED: Price fence exceeded for {accountName}. "
                            + $"Current={currentPrice:F2}, Entry={entryPrice:F2}, "
                            + $"Diff={priceDiff:F4} > Fence={fenceDistance:F4} ({RepairTickFence} ticks). "
                            + $"Adjust RepairTickFence if you want to force entry."
                    );
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Phase7-T1: Submits repair order with authorization validation.
        /// Checks FSM state, dispatch reservation, metadata guard, then creates and submits order.
        /// </summary>
        private void SubmitRepairOrderWithAuthorization(
            string accountName,
            PositionInfo repairPos,
            string repairEntryName,
            OrderType orderType,
            double limitPrice,
            double stopPrice
        )
        {
            if (!TryResolveRepairAccount(repairPos, accountName, out Account targetAcct))
                return;
            if (
                !CreateRepairOrder(
                    targetAcct,
                    repairPos,
                    orderType,
                    limitPrice,
                    stopPrice,
                    repairEntryName,
                    out Order repairEntry
                )
            )
                return;
            bool hasActiveFsm = HasActiveFsmForAccount(accountName);
            if (!ResolveRepairAuthorization(accountName, hasActiveFsm))
                return;
            if (!MetadataGuardRepairAuthorized(accountName, "ExecuteReaperRepair"))
                return;
            PrepareAndRegisterRepairOrder(repairPos, repairEntryName, repairEntry);
            targetAcct.Submit(new[] { repairEntry });
            LogRepairOrderSubmitted(
                accountName,
                repairEntryName,
                repairEntry.OrderAction,
                repairEntry.Quantity,
                orderType,
                repairPos
            );
        }

        // T-088-01: CYC=2. Resolves the executing account from repairPos.
        // Returns false (with null out) if account is unresolvable.
        private bool TryResolveRepairAccount(PositionInfo repairPos, string accountName, out Account targetAcct)
        {
            if (repairPos.ExecutingAccount == null)
            {
                Print("[REAPER REPAIR] [FAIL] ExecutingAccount is null for " + accountName);
                targetAcct = null;
                return false;
            }

            targetAcct = repairPos.ExecutingAccount;
            return true;
        }

        // T-088-02: CYC=3. Creates the repair order via targetAcct.CreateOrder.
        // Returns false (with null out) if CreateOrder returns null.
        private bool CreateRepairOrder(
            Account targetAcct,
            PositionInfo repairPos,
            OrderType orderType,
            double limitPrice,
            double stopPrice,
            string repairSignal,
            out Order repairEntry
        )
        {
            OrderAction action = repairPos.Direction == MarketPosition.Long ? OrderAction.Buy : OrderAction.SellShort;
            repairEntry = targetAcct.CreateOrder(
                Instrument,
                action,
                orderType,
                TimeInForce.Gtc,
                repairPos.TotalContracts,
                limitPrice,
                stopPrice,
                "",
                repairSignal,
                null
            );

            if (repairEntry == null)
            {
                Print("[REAPER REPAIR] [FAIL] CreateOrder returned null for " + targetAcct.Name);
                return false;
            }

            return true;
        }

        // T-088-03: CYC=5. Returns true if any active FSM bracket belongs to accountName.
        // Note: ConcurrentDictionary TOCTOU is pre-existing, out of scope per V12.23.
        private bool HasActiveFsmForAccount(string accountName)
        {
            return _followerBrackets.Values.Any(f =>
                f != null
                && f.AccountName == accountName
                && (
                    f.State == FollowerBracketState.Active
                    || f.State == FollowerBracketState.Accepted
                    || f.State == FollowerBracketState.Submitted
                    || f.State == FollowerBracketState.Replacing
                )
            );
        }

        // T-088-04: CYC=5. Returns true when repair is authorized to proceed.
        // If hasActiveFsm is true, immediately authorized. Otherwise checks dispatch reservation
        // or active follower position as fallback. Prints guard messages.
        // Note: TOCTOU risk on dispatch/position checks is pre-existing, out of scope per V12.23.
        private bool ResolveRepairAuthorization(string accountName, bool hasActiveFsm)
        {
            if (hasActiveFsm)
                return true;

            bool dispatchPending = _dispatchSyncPendingExpKeys.ContainsKey(ExpKey(accountName));
            bool hasActivePositionEntry = activePositions.Values.Any(p =>
                p.IsFollower && p.ExecutingAccount != null && p.ExecutingAccount.Name == accountName
            );

            if (!dispatchPending && !hasActivePositionEntry)
            {
                Print(
                    string.Format(
                        "[FSM-RACE GUARD ABORT] {0}: no FSM, no dispatch reservation, no position -- aborted",
                        accountName
                    )
                );
                return false;
            }

            Print(string.Format("[FSM-RACE GUARD] {0}: no FSM -- dispatch/position fallback authorized", accountName));
            return true;
        }

        // T-088-05: CYC=1. Marks bracket as not yet submitted and registers the order in entryOrders.
        // Note: stale entryOrders on Submit failure (H3) is pre-existing, out of scope per V12.23.
        // BracketSubmitted thread-safety is pre-existing, out of scope per V12.23.
        private void PrepareAndRegisterRepairOrder(PositionInfo repairPos, string repairEntryName, Order repairEntry)
        {
            repairPos.BracketSubmitted = false;
            entryOrders[repairEntryName] = repairEntry;
        }

        // T-088-06: CYC=2. Logs a successful repair order submission.
        // ASCII-only string literals throughout.
        private void LogRepairOrderSubmitted(
            string accountName,
            string repairEntryName,
            OrderAction action,
            int quantity,
            OrderType orderType,
            PositionInfo repairPos
        )
        {
            Print(
                "[REAPER REPAIR] [OK] Repair order submitted for "
                    + accountName
                    + " under key="
                    + repairEntryName
                    + ": "
                    + action
                    + " "
                    + quantity
                    + " "
                    + orderType
                    + " "
                    + (orderType == OrderType.Market ? "@ Market" : "@ " + repairPos.EntryPrice.ToString("F2"))
                    + " (original entry="
                    + repairPos.EntryPrice.ToString("F2")
                    + ")"
            );
        }

        // Build 935 [REAPER-B935-005]: Single-repair body extracted from ProcessReaperRepairQueue.
        // Threading: runs on strategy thread (via TriggerCustomEvent). All stateLock usages unchanged.
        // Build 1111.007-phase7-t1: Extracted to 4 sub-methods (CYC 32-><10).
        private void ExecuteReaperRepair(string accountName)
        {
            string repairKey = accountName + "_" + Instrument.FullName;
            try
            {
                // Phase7-T1: Pure dispatcher - orchestrates validation chain with early returns
                if (!ValidateRepairEligibility(accountName, out PositionInfo repairPos, out string repairEntryName))
                    return;

                OrderType repairOrderType = repairPos.EntryOrderType;
                double repairEntryPrice = Instrument.MasterInstrument.RoundToTickSize(repairPos.EntryPrice);

                CalculateRepairOrderPrices(
                    repairOrderType,
                    repairEntryPrice,
                    out double repairLimitPrice,
                    out double repairStopPrice
                );

                double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
                if (!ValidateRepairRiskBounds(accountName, repairOrderType, repairEntryPrice, currentPrice))
                    return;

                SubmitRepairOrderWithAuthorization(
                    accountName,
                    repairPos,
                    repairEntryName,
                    repairOrderType,
                    repairLimitPrice,
                    repairStopPrice
                );

                // Clear DESYNC chart label (inlined - below 15 LOC minimum)
                try
                {
                    string desyncTag = "SIMA_DESYNC_" + accountName;
                    RemoveDrawObject(desyncTag);
                }
                catch
                {
                    // Swallow: RemoveDrawObject may throw if chart object doesn't exist or chart is disposed
                }
            }
            catch (Exception ex)
            {
                Print($"[REAPER REPAIR] [FAIL] FAILED for {accountName}: {ex.Message}"); // [Build 969]
            }
            // [Build 969.3] - Top-level finally guarantees _repairInFlight cleanup on ALL exit paths.
            _repairInFlight.TryRemove(repairKey, out _);
        }

        #endregion
    }
}
