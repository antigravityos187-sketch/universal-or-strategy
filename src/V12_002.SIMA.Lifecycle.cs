// Build 971: SIMA Lifecycle -- ApplySimaState, EnumerateApexAccounts, Hydrate*, CancelAll*, Sweep*
// V12 SIMA Module (Extracted)
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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
        #region V12 SIMA Lifecycle

        private void ProcessApplySimaState(bool enabled)
        {
            // V12.Audit [H-10]: If a previous toggle timed out, attempt retry now.
            // We re-enter with the same `enabled` argument that was pending.
            // If the semaphore is still held this call will time out again, setting the flag once more.
            if (_simaTogglePending != 0)
                Print("[SIMA LIFECYCLE] Retrying previously timed-out toggle (pending retry flag was set).");

            // Measure lifecycle semaphore contention because this wait runs on the actor path
            // and can stall queue drain when SIMA toggles overlap with other work.
            Stopwatch waitTimer = Stopwatch.StartNew();
            // Build 1109 [FREEZE-PROOF]: Non-blocking gate via Interlocked.CompareExchange.
            // If contended, defer to next strategy-thread cycle via TriggerCustomEvent.
            if (Interlocked.CompareExchange(ref _simaToggleState, 1, 0) != 0)
            {
                waitTimer.Stop();
                _simaTogglePending = 1;
                bool _defEnabled = enabled;
                Print("[SIMA_WARN] Toggle gate contended -- scheduling non-blocking retry");
                try
                {
                    TriggerCustomEvent(o => ProcessApplySimaState(_defEnabled), null);
                }
                catch { }
                return;
            }
            try
            {
                waitTimer.Stop();
                if (waitTimer.Elapsed.TotalMilliseconds >= 25.0)
                    Print(
                        string.Format(
                            "[LATENCY] [SIMA LIFECYCLE] Toggle gate wait: {0:F1}ms",
                            waitTimer.Elapsed.TotalMilliseconds
                        )
                    );

                if (enabled)
                    ProcessInitializeSIMA();
                else
                    ProcessShutdownSIMA();

                EnableSIMA = enabled;
                // V12.Audit [H-10]: Toggle completed successfully -- clear any pending-retry flag.
                _simaTogglePending = 0;
            }
            finally
            {
                Interlocked.Exchange(ref _simaToggleState, 0);
            }
        }

        private void ProcessInitializeSIMA()
        {
            EnumerateApexAccounts(); // Unsubs first (idempotent), then re-subscribes + hydrates
            if (ReaperAuditEnabled)
                StartReaperAudit();
            Print("[SIMA LIFECYCLE] SIMA ENABLED -- fleet enumerated, Reaper started");
        }

        private void ProcessShutdownSIMA()
        {
            CancelAllV12GtcOrders(false); // [BUILD 948] GTC sweep before teardown -- skip accounts with open positions
            StopReaperAudit();
            UnsubscribeFromFleetAccounts();
            // v28.0 shutdown drain: sideband-aware, XorShadow-free (we do not verify on shutdown;
            // we just need to release pool + roll back delta). Sideband entries are zeroed after.
            {
                FleetDispatchSlot ringSlot;
                while (_photonDispatchRing != null && _photonDispatchRing.TryDequeue(out ringSlot))
                {
                    int _sbIdx = ringSlot.PoolSlotIndex;
                    string _expectedKey =
                        (_sbIdx >= 0 && _sbIdx < _photonSideband.Length) ? _photonSideband[_sbIdx].ExpectedKey : null;
                    if (ringSlot.ReservedDelta != 0 && _expectedKey != null)
                        AddExpectedPositionDelta(_expectedKey, -ringSlot.ReservedDelta);
                    if (_expectedKey != null)
                        ClearDispatchSyncPending(_expectedKey);
                    if (_sbIdx >= 0)
                    {
                        _photonPool.ReleaseByIndex(_sbIdx);
                        if (_sbIdx < _photonSideband.Length)
                            _photonSideband[_sbIdx] = default(FleetDispatchSideband);
                    }
                }
                Print("[SIMA] Photon ring cleared on shutdown with delta rollback.");
            }
            // A3-1: Drain ghost dispatch queue on SIMA disable (Build 960 audit fix)
            // B957/F2: Rollback ReservedDelta and clear dispatch-sync barrier for each discarded request.
            {
                FleetDispatchRequest ignored;
                while (_pendingFleetDispatches.TryDequeue(out ignored))
                {
                    if (ignored.ReservedDelta != 0)
                        AddExpectedPositionDelta(ignored.ExpectedKey, -ignored.ReservedDelta);
                    ClearDispatchSyncPending(ignored.ExpectedKey);
                }
                Print("[SIMA] Dispatch queue cleared on shutdown with delta rollback.");
            }
            Print("[SIMA LIFECYCLE] SIMA DISABLED -- Reaper stopped, handlers unsubscribed");
        }

        private void EnumerateApexAccounts()
        {
            UnsubscribeFromFleetAccounts(); // V12.1101E [A-4]: Always unsub first -- idempotent guard against handler accumulation
            simaAccountCount = 0;
            Print("[SIMA] ===================================================");
            Print("[SIMA] V12.12 - Fleet Symmetry & Safety Hardening Initializing");
            Print($"[SIMA] Account Prefix Filter: \"{AccountPrefix}\"");
            Print("[SIMA] ---------------------------------------------------");

            foreach (Account acct in Account.All)
            {
                if (IsFleetAccount(acct))
                {
                    simaAccountCount++;
                    // Build 1105: Only init expectedPositions for master during enumeration.
                    // Follower REAPER audit truth is owned by FSM.
                    if (acct.Name == Account.Name)
                    {
                        var _acct966init = ExpKey(acct.Name);
                        SetExpectedPosition(_acct966init, 0);
                    }
                    accountDailyProfit[acct.Name] = 0; // Initialize daily profit
                    EnsureAccountComplianceTracking(acct.Name, GetComplianceNow());
                    activeFleetAccounts[acct.Name] = false; // V12.8 SIMA: Default to INACTIVE -- wait for Fleet Manager / IPC to enable

                    // V12.7: Always subscribe to execution updates for fleet bracket management
                    // (Also used by ComplianceHub for P/L tracking)
                    acct.ExecutionUpdate += OnAccountExecutionUpdate;
                    acct.OrderUpdate += OnAccountOrderUpdate;
                    _subscribedAccountNames.Add(acct.Name); // V12.Phase6 [UNSUB-TRACK]: Track for deterministic unsubscribe
                    if (EnableComplianceHub)
                    {
                        Print($"[SIMA] [OK] {acct.Name} | COMPLIANCE MONITORING ACTIVE");
                    }
                    else
                    {
                        Print(
                            $"[SIMA] #{simaAccountCount}: {acct.Name} | Connected: {acct.Connection?.Status == ConnectionStatus.Connected} | Fleet: INACTIVE (awaiting IPC enable)"
                        );
                    }
                }
            }

            Print("[SIMA] ---------------------------------------------------");
            Print($"[SIMA] TOTAL ACCOUNTS DETECTED: {simaAccountCount} | ALL INACTIVE by default");
            Print("[SIMA] FLEET INACTIVE - MANUAL ENABLE REQUIRED"); // V12.Phase10 [DEFAULT-FIX]
            Print("[SIMA] ===================================================");

            // Build 1103: Apply persisted fleet toggles from sticky state file.
            // Must run AFTER enumeration (dict populated) but BEFORE hydration (expected positions).
            ApplyPendingStickyFleetToggles();

            // V12.Phase6 [HYDRATE]: Seed expectedPositions from live broker state
            HydrateExpectedPositionsFromBroker();

            // [BUILD 948] Adopt any working broker orders into tracking dicts; sets _orderAdoptionComplete = true
            HydrateWorkingOrdersFromBroker();

            // Build 1103: Enrich reconstructed positions with persisted trail state.
            // Must run AFTER Phase 3 (activePositions populated) so position keys exist.
            EnrichTrailStateFromSticky();
        }

        /// <summary>
        /// V12.Phase6 [HYDRATE]: Reads actual broker positions for each fleet account and seeds
        /// expectedPositions accordingly. Prevents false Reaper CRITICAL DESYNC alerts when the
        /// strategy restarts while accounts hold open positions.
        /// </summary>
        private void HydrateExpectedPositionsFromBroker()
        {
            int hydratedCount = 0;
            foreach (Account acct in Account.All)
            {
                if (!IsFleetAccount(acct))
                    continue;

                try
                {
                    // [939-P0]: Snapshot Positions to prevent broker-thread mutation during iteration.
                    foreach (Position pos in acct.Positions.ToArray())
                    {
                        if (
                            pos != null
                            && pos.Instrument != null
                            && pos.Instrument.FullName == Instrument.FullName
                            && pos.MarketPosition != MarketPosition.Flat
                        )
                        {
                            int qty = pos.MarketPosition == MarketPosition.Long ? pos.Quantity : -pos.Quantity;
                            // Build 980 [Nexus]: Route expected position seed through the Actor queue
                            var capturedAcct = acct.Name;
                            var capturedQty = qty;
                            Enqueue(ctx =>
                                ctx.AddOrUpdateExpectedPosition(ExpKey(capturedAcct), capturedQty, v => capturedQty)
                            );
                            Print(
                                $"[SIMA HYDRATE] {acct.Name}: Seeded expected={qty} from broker ({pos.MarketPosition} {pos.Quantity})"
                            );
                            hydratedCount++;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Print($"[SIMA HYDRATE] WARNING: Could not read positions for {acct.Name}: {ex.Message}");
                }
            }
            if (hydratedCount > 0)
                Print($"[SIMA HYDRATE] Hydrated {hydratedCount} account(s) with live broker positions");

            // Build 993: Hydrate master account (mirrors AuditMasterAccountIfNeeded pattern).
            // IsFleetAccount excludes master -- must be handled separately, same as REAPER audit.
            bool masterIsFleet993 = IsFleetAccount(Account);
            if (!masterIsFleet993)
            {
                try
                {
                    foreach (Position pos in Account.Positions.ToArray())
                    {
                        if (
                            pos != null
                            && pos.Instrument?.FullName == Instrument.FullName
                            && pos.MarketPosition != MarketPosition.Flat
                        )
                        {
                            int qty = pos.MarketPosition == MarketPosition.Long ? pos.Quantity : -pos.Quantity;
                            var capturedQty993 = qty;
                            Enqueue(ctx =>
                                ctx.AddOrUpdateExpectedPosition(
                                    ExpKey(Account.Name),
                                    capturedQty993,
                                    v => capturedQty993
                                )
                            );
                            Print(
                                string.Format(
                                    "[SIMA HYDRATE] {0} (Master): Seeded expected={1} from broker ({2} {3})",
                                    Account.Name,
                                    qty,
                                    pos.MarketPosition,
                                    pos.Quantity
                                )
                            );
                            hydratedCount++;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Print(
                        string.Format(
                            "[SIMA HYDRATE] WARNING: Could not read positions for {0} (Master): {1}",
                            Account.Name,
                            ex.Message
                        )
                    );
                }
            }
        }

        /// <summary>
        /// Build 948 [FIX-B]: Re-adopt working broker orders into tracking dicts after restart or reconnect.
        /// Derives the original entry key by stripping the well-known order-name prefix (e.g. "Stop_" -> stopOrders).
        /// Sets _orderAdoptionComplete = true when done so REAPER can resume auditing.
        /// MUST be called on the strategy thread (via TriggerCustomEvent when initiated from a callback).
        /// Actor-serialized lifecycle and reconnect paths update tracking dicts on the Ordered Actor Thread.
        /// </summary>
        private void HydrateWorkingOrdersFromBroker()
        {
            int adoptedCount = AdoptFleetOrders();

            // Build 993: Adopt master account bracket orders (mirrors fleet loop; no FSM creation for master).
            // IsFleetAccount excludes master -- must be handled separately.
            bool masterIsFleetForOrders993 = IsFleetAccount(Account);
            if (!masterIsFleetForOrders993)
            {
                try
                {
                    adoptedCount += AdoptMasterOrders();
                }
                catch (Exception ex)
                {
                    Print(
                        string.Format(
                            "[SIMA HYDRATE] WARNING: Could not adopt orders for {0} (Master): {1}",
                            Account.Name,
                            ex.Message
                        )
                    );
                }
            }

            // Build 1108.003 [D2-A]: Reconstruct master activePositions from adopted bracket orders + broker.
            // Filled master positions have bracket orders but no working entry order to hydrate from.
            if (!masterIsFleetForOrders993)
            {
                try
                {
                    MarketPosition masterMP = MarketPosition.Flat;
                    int masterQty = 0;
                    double masterAvgPrice = 0;
                    foreach (Position brokerPos in Account.Positions.ToArray())
                    {
                        if (
                            brokerPos != null
                            && brokerPos.Instrument != null
                            && brokerPos.Instrument.FullName == Instrument.FullName
                            && brokerPos.MarketPosition != MarketPosition.Flat
                        )
                        {
                            masterMP = brokerPos.MarketPosition;
                            masterQty = brokerPos.Quantity;
                            masterAvgPrice = brokerPos.AveragePrice;
                            break;
                        }
                    }

                    if (masterMP != MarketPosition.Flat && masterQty > 0)
                    {
                        foreach (var stopKvp in stopOrders.ToArray())
                        {
                            string key = stopKvp.Key;
                            if (key.StartsWith("Fleet_", StringComparison.OrdinalIgnoreCase))
                                continue;
                            if (activePositions.ContainsKey(key))
                                continue;

                            Order adoptedStop = stopKvp.Value;
                            double stopPrice = adoptedStop != null ? adoptedStop.StopPrice : 0;

                            int t1Qty,
                                t2Qty,
                                t3Qty,
                                t4Qty,
                                t5Qty;
                            GetTargetDistribution(masterQty, out t1Qty, out t2Qty, out t3Qty, out t4Qty, out t5Qty);

                            bool trendMnlMatch = key.StartsWith("TrendMnl", StringComparison.OrdinalIgnoreCase);
                            Print(
                                string.Format(
                                    "[SIMA HYDRATE] Master stop key audit for {0}: TrendMnlStartsWith={1}",
                                    key,
                                    trendMnlMatch
                                )
                            );

                            var pos = new PositionInfo
                            {
                                SignalName = key,
                                Direction = masterMP,
                                TotalContracts = masterQty,
                                RemainingContracts = masterQty,
                                EntryPrice = masterAvgPrice,
                                InitialStopPrice = stopPrice,
                                CurrentStopPrice = stopPrice,
                                EntryOrderType = OrderType.Market,
                                EntryFilled = true,
                                IsFollower = false,
                                ExecutingAccount = null,
                                BracketSubmitted = true,
                                ExtremePriceSinceEntry = masterAvgPrice,
                                CurrentTrailLevel = 0,
                                OcoGroupId = "V12_" + GetStableHash(key),
                                T1Contracts = t1Qty,
                                T2Contracts = t2Qty,
                                T3Contracts = t3Qty,
                                T4Contracts = t4Qty,
                                T5Contracts = t5Qty,
                            };

                            pos.IsMOMOTrade = key.StartsWith("MOMO", StringComparison.OrdinalIgnoreCase);
                            pos.IsTRENDTrade =
                                trendMnlMatch || key.StartsWith("TRMA_", StringComparison.OrdinalIgnoreCase);
                            pos.IsRetestTrade = key.StartsWith("Retest", StringComparison.OrdinalIgnoreCase);
                            pos.IsRMATrade =
                                key.StartsWith("TRMA_", StringComparison.OrdinalIgnoreCase) || pos.IsRetestTrade;
                            pos.IsFFMATrade = key.StartsWith("FFMA", StringComparison.OrdinalIgnoreCase);
                            if (pos.IsMOMOTrade)
                                pos.IsRMATrade = false;

                            activePositions[key] = pos;
                            Print(
                                string.Format(
                                    "[SIMA HYDRATE] Reconstructed master position for {0} | Dir={1} Qty={2} AvgPx={3} StopPx={4}",
                                    key,
                                    masterMP,
                                    masterQty,
                                    masterAvgPrice,
                                    stopPrice
                                )
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    Print(
                        string.Format("[SIMA HYDRATE] WARNING: Master position reconstruction failed: {0}", ex.Message)
                    );
                }
            }

            // Phase 5: Rebuild FSMs from adopted orders before enabling REAPER
            HydrateFSMsFromWorkingOrders();

            _orderAdoptionComplete = true;
            if (adoptedCount > 0)
                Print(
                    string.Format(
                        "[SIMA HYDRATE] Adopted {0} working order(s) from broker -- adoption complete.",
                        adoptedCount
                    )
                );
            else
                Print("[SIMA HYDRATE] No working orders to adopt -- adoption complete.");
        }

        /// <summary>
        /// Maps NinjaTrader OrderState to V12 FollowerBracketState.
        /// Pure function - no side effects, deterministic mapping.
        /// </summary>
        /// <param name="entryState">NinjaTrader order state</param>
        /// <returns>Corresponding FSM state, or null if terminal state (skip FSM creation)</returns>
        /// <remarks>
        /// Terminal states (Cancelled, Rejected, etc.) return null to signal caller to skip FSM creation.
        /// This preserves the original behavior where terminal orders are ignored.
        /// </remarks>
        private FollowerBracketState? MapOrderStateToFSMState(OrderState entryState)
        {
            if (entryState == OrderState.Filled || entryState == OrderState.PartFilled)
            {
                return FollowerBracketState.Active;
            }
            else if (entryState == OrderState.Accepted)
            {
                return FollowerBracketState.Accepted;
            }
            else if (
                entryState == OrderState.Working
                || entryState == OrderState.Submitted
                || entryState == OrderState.Initialized
                || entryState == OrderState.ChangePending
                || entryState == OrderState.ChangeSubmitted
            )
            {
                return FollowerBracketState.Submitted;
            }
            else
            {
                return null; // Terminal state - skip FSM creation
            }
        }

        /// <summary>
        /// Factory method to construct FollowerBracketFSM instance.
        /// Centralizes FSM initialization logic.
        /// </summary>
        /// <param name="entryKey">FSM key (entry order name)</param>
        /// <param name="accountName">Account name from PositionInfo</param>
        /// <param name="entryOrder">Entry order (may be null for position pass)</param>
        /// <param name="state">Initial FSM state</param>
        /// <param name="remainingContracts">Remaining contracts</param>
        /// <returns>Initialized FSM instance</returns>
        private FollowerBracketFSM BuildFSM(
            string entryKey,
            string accountName,
            Order entryOrder,
            FollowerBracketState state,
            int remainingContracts
        )
        {
            return new FollowerBracketFSM
            {
                AccountName = accountName,
                EntryName = entryKey,
                State = state,
                RemainingContracts = remainingContracts,
                LastUpdateUtc = DateTime.UtcNow,
                EntryOrder = entryOrder,
            };
        }

        /// <summary>
        /// Resolves remaining contracts for FSM hydration based on state and position.
        /// Pure function: returns order quantity for non-Active states, position quantity for Active.
        /// </summary>
        /// <param name="state">FSM state</param>
        /// <param name="orderQuantity">Order quantity</param>
        /// <param name="positionQuantity">Live position quantity (nullable)</param>
        /// <returns>Remaining contracts to hydrate</returns>
        private int ResolveRemainingContracts(FollowerBracketState state, int orderQuantity, int? positionQuantity)
        {
            int remainingContracts = Math.Max(0, orderQuantity);
            if (state == FollowerBracketState.Active && positionQuantity.HasValue)
            {
                remainingContracts = Math.Abs(positionQuantity.Value);
            }
            return remainingContracts;
        }

        /// <summary>
        /// Registers FSM in tracking dictionaries and updates counters.
        /// Centralizes dictionary update logic for easier auditing.
        /// </summary>
        /// <param name="entryKey">FSM key</param>
        /// <param name="fsm">FSM to register</param>
        /// <param name="entryOrder">Entry order (may be null for position pass)</param>
        /// <param name="ordersIndexed">Counter (incremented if entry order linked)</param>
        /// <param name="fsmCreated">Counter (always incremented)</param>
        private void RegisterFSM(
            string entryKey,
            FollowerBracketFSM fsm,
            Order entryOrder,
            ref int ordersIndexed,
            ref int fsmCreated
        )
        {
            _followerBrackets.TryAdd(entryKey, fsm);

            if (entryOrder != null && !string.IsNullOrEmpty(entryOrder.OrderId))
            {
                _orderIdToFsmKey[entryOrder.OrderId] = entryKey;
                ordersIndexed++;
            }

            fsmCreated++;
        }

        /// <summary>
        /// Links a target order to FSM and indexes it in _orderIdToFsmKey.
        /// Eliminates repetitive target linking logic.
        /// </summary>
        /// <param name="fsm">FSM to update (passed by ref for struct mutation)</param>
        /// <param name="entryKey">FSM key for order ID mapping</param>
        /// <param name="targetIndex">Target slot index (0-4)</param>
        /// <param name="targetOrders">Target order dictionary</param>
        /// <param name="ordersIndexed">Counter (incremented if order linked)</param>
        private void LinkTargetOrderToFSM(
            ref FollowerBracketFSM fsm,
            string entryKey,
            int targetIndex,
            ConcurrentDictionary<string, Order> targetOrders,
            ref int ordersIndexed
        )
        {
            Order targetOrd;
            if (targetOrders.TryGetValue(entryKey, out targetOrd) && targetOrd != null)
            {
                fsm.Targets[targetIndex] = targetOrd;
                if (!string.IsNullOrEmpty(targetOrd.OrderId))
                {
                    _orderIdToFsmKey[targetOrd.OrderId] = entryKey;
                    ordersIndexed++;
                }
            }
        }

        /// <summary>
        /// Finds live position for Active state FSM hydration.
        /// Returns null if no matching position found.
        /// </summary>
        /// <param name="pi">PositionInfo containing executing account</param>
        /// <returns>Live position or null</returns>
        private Position FindLivePosition(PositionInfo pi)
        {
            if (pi.ExecutingAccount == null)
                return null;

            return pi
                .ExecutingAccount.Positions.ToArray()
                .FirstOrDefault(p =>
                    p != null
                    && p.Instrument != null
                    && p.Instrument.FullName == Instrument.FullName
                    && p.MarketPosition != MarketPosition.Flat
                );
        }

        /// <summary>
        /// Phase 5 Position Pass: Creates FSMs for accounts with open positions but terminal entry orders.
        /// Handles edge case where entry order is cancelled/rejected but position remains open.
        /// Implements REAPER grace window (5 minutes) for failed stop order key recovery.
        /// </summary>
        private int HydrateFromOpenPositions(
            ConcurrentDictionary<string, Order> stopOrders,
            ConcurrentDictionary<string, Order> target1Orders,
            ConcurrentDictionary<string, Order> target2Orders,
            ConcurrentDictionary<string, Order> target3Orders,
            ConcurrentDictionary<string, Order> target4Orders,
            ConcurrentDictionary<string, Order> target5Orders,
            ref int ordersIndexed,
            ref int fsmCreated
        )
        {
            int positionFsmCreated = 0;
            foreach (Account acct in Account.All)
            {
                if (!IsFleetAccount(acct))
                    continue;

                // Do we already have an FSM for this account?
                if (
                    _followerBrackets.Values.Any(f =>
                        string.Equals(f.AccountName, acct.Name, StringComparison.OrdinalIgnoreCase)
                    )
                )
                    continue;

                // Is there an open position for this instrument in this account?
                Position acctPos = acct.Positions.FirstOrDefault(p =>
                    p.Instrument.FullName == Instrument.FullName && p.MarketPosition != MarketPosition.Flat
                );
                if (acctPos == null)
                    continue;

                // Scan stopOrders for any entryKey belonging to this account
                string recoveredKey = null;
                Order recoveredStop = null;
                foreach (var stopKvp in stopOrders.ToArray())
                {
                    Order stopCand = stopKvp.Value;
                    if (stopCand == null)
                        continue;
                    if (stopCand.Account == null)
                        continue;

                    // If the stop order's original account matches our current iteration account
                    if (string.Equals(stopCand.Account.Name, acct.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        recoveredKey = stopKvp.Key;
                        recoveredStop = stopCand;
                        break;
                    }
                }

                if (recoveredKey == null)
                {
                    Print(
                        string.Format(
                            "[SIMA] Phase 5 Position Pass: WARNING -- open position on {0} but no stopOrders key found. FSM not created. REAPER grace window started.",
                            acct.Name
                        )
                    );
                    // Build 999: Mark account for REAPER grace window -- defer critical desync up to 10s.
                    // CancelPending stop (stop-replace mid-flight at disable) causes this warning.
                    // The replace cycle resolves within seconds; grace prevents premature flatten cascade.
                    _positionPassFailedFirstSeen[acct.Name] = DateTime.UtcNow;
                    continue;
                }

                // Idempotent guard
                if (_followerBrackets.ContainsKey(recoveredKey))
                    continue;

                var fsm = new FollowerBracketFSM
                {
                    AccountName = acct.Name,
                    EntryName = recoveredKey,
                    State = FollowerBracketState.Active,
                    RemainingContracts = Math.Abs(acctPos.Quantity),
                    LastUpdateUtc = DateTime.UtcNow,
                    EntryOrder = null, // Terminal entry order
                };

                // Link stop order
                if (recoveredStop != null)
                {
                    fsm.StopOrder = recoveredStop;
                    if (!string.IsNullOrEmpty(recoveredStop.OrderId))
                    {
                        _orderIdToFsmKey[recoveredStop.OrderId] = recoveredKey;
                        ordersIndexed++;
                    }
                }

                // Link target orders
                Order targetOrd;
                if (target1Orders.TryGetValue(recoveredKey, out targetOrd) && targetOrd != null)
                {
                    fsm.Targets[0] = targetOrd;
                    if (!string.IsNullOrEmpty(targetOrd.OrderId))
                    {
                        _orderIdToFsmKey[targetOrd.OrderId] = recoveredKey;
                        ordersIndexed++;
                    }
                }
                if (target2Orders.TryGetValue(recoveredKey, out targetOrd) && targetOrd != null)
                {
                    fsm.Targets[1] = targetOrd;
                    if (!string.IsNullOrEmpty(targetOrd.OrderId))
                    {
                        _orderIdToFsmKey[targetOrd.OrderId] = recoveredKey;
                        ordersIndexed++;
                    }
                }
                if (target3Orders.TryGetValue(recoveredKey, out targetOrd) && targetOrd != null)
                {
                    fsm.Targets[2] = targetOrd;
                    if (!string.IsNullOrEmpty(targetOrd.OrderId))
                    {
                        _orderIdToFsmKey[targetOrd.OrderId] = recoveredKey;
                        ordersIndexed++;
                    }
                }
                if (target4Orders.TryGetValue(recoveredKey, out targetOrd) && targetOrd != null)
                {
                    fsm.Targets[3] = targetOrd;
                    if (!string.IsNullOrEmpty(targetOrd.OrderId))
                    {
                        _orderIdToFsmKey[targetOrd.OrderId] = recoveredKey;
                        ordersIndexed++;
                    }
                }
                if (target5Orders.TryGetValue(recoveredKey, out targetOrd) && targetOrd != null)
                {
                    fsm.Targets[4] = targetOrd;
                    if (!string.IsNullOrEmpty(targetOrd.OrderId))
                    {
                        _orderIdToFsmKey[targetOrd.OrderId] = recoveredKey;
                        ordersIndexed++;
                    }
                }

                _followerBrackets.TryAdd(recoveredKey, fsm);

                positionFsmCreated++;
                fsmCreated++;

                Print(
                    string.Format(
                        "[SIMA] Phase 5 Position Pass: Created FSM for {0} (key={1})",
                        acct.Name,
                        recoveredKey
                    )
                );
            }

            return positionFsmCreated;
        }

        /// <summary>
        /// Phase 5: Rebuilds _followerBrackets and _orderIdToFsmKey from already-adopted
        /// working orders. Called from HydrateWorkingOrdersFromBroker() before the
        /// adoption-complete gate is set. Idempotent -- safe to call on every reconnect.
        /// </summary>
        private void HydrateFSMsFromWorkingOrders()
        {
            int fsmCreated = 0;
            int ordersIndexed = 0;

            Print("[SIMA] Phase 5 FSM Hydration: Starting entry order pass...");

            // Entry Order Pass
            foreach (var kvp in entryOrders.ToArray())
            {
                string entryKey = kvp.Key;
                Order entryOrder = kvp.Value;
                if (entryOrder == null)
                    continue;

                // Skip master account entries
                PositionInfo pi;
                if (!activePositions.TryGetValue(entryKey, out pi) || !pi.IsFollower)
                    continue;
                if (pi.ExecutingAccount == null)
                    continue;

                // Idempotent guard
                if (_followerBrackets.ContainsKey(entryKey))
                    continue;

                // Map state
                FollowerBracketState? state = MapOrderStateToFSMState(entryOrder.OrderState);
                if (state == null)
                    continue; // Terminal state - skip FSM creation

                // Resolve contracts
                Position livePosition = null;
                if (state.Value == FollowerBracketState.Active)
                {
                    livePosition = FindLivePosition(pi);
                }

                int remainingContracts = ResolveRemainingContracts(
                    state.Value,
                    entryOrder.Quantity,
                    livePosition?.Quantity
                );

                // Build FSM
                var fsm = BuildFSM(entryKey, pi.ExecutingAccount.Name, entryOrder, state.Value, remainingContracts);

                // Link stop order
                Order stopOrd;
                if (stopOrders.TryGetValue(entryKey, out stopOrd) && stopOrd != null)
                {
                    fsm.StopOrder = stopOrd;
                    if (!string.IsNullOrEmpty(stopOrd.OrderId))
                    {
                        _orderIdToFsmKey[stopOrd.OrderId] = entryKey;
                        ordersIndexed++;
                    }
                }

                // Link target orders
                LinkTargetOrderToFSM(ref fsm, entryKey, 0, target1Orders, ref ordersIndexed);
                LinkTargetOrderToFSM(ref fsm, entryKey, 1, target2Orders, ref ordersIndexed);
                LinkTargetOrderToFSM(ref fsm, entryKey, 2, target3Orders, ref ordersIndexed);
                LinkTargetOrderToFSM(ref fsm, entryKey, 3, target4Orders, ref ordersIndexed);
                LinkTargetOrderToFSM(ref fsm, entryKey, 4, target5Orders, ref ordersIndexed);

                // Register FSM
                RegisterFSM(entryKey, fsm, entryOrder, ref ordersIndexed, ref fsmCreated);
            }

            Print(
                string.Format(
                    "[SIMA] Phase 5 FSM Hydration (Entry Pass): {0} FSMs created, {1} order IDs indexed.",
                    fsmCreated,
                    ordersIndexed
                )
            );

            // Position Pass
            int positionFsmCreated = HydrateFromOpenPositions(
                stopOrders,
                target1Orders,
                target2Orders,
                target3Orders,
                target4Orders,
                target5Orders,
                ref ordersIndexed,
                ref fsmCreated
            );

            Print(
                string.Format(
                    "[SIMA] Phase 5 FSM Hydration (Position Pass): {0} Active FSMs created from open positions.",
                    positionFsmCreated
                )
            );

            Print(
                string.Format(
                    "[SIMA] Phase 5 FSM Hydration: {0} FSMs created, {1} order IDs indexed.",
                    fsmCreated,
                    ordersIndexed
                )
            );
        }

        /// <summary>
        /// Adopts working orders from all fleet accounts into tracking dictionaries.
        /// ACTOR-SERIALIZED: Must be called on strategy thread (via EnumerateApexAccounts).
        /// THREAD-SAFETY: Single-write operations to ConcurrentDictionary are safe.
        /// ORDERING: Must execute BEFORE FSM hydration reads these dictionaries.
        /// LATENCY: Cold path (startup/reconnect only). Target <100ms for 50 accounts.
        /// Jane Street bounded-latency principle applies to hot paths (per-tick), not cold paths.
        /// If production latency exceeds 500ms, create EPIC-CCN-54: Latency Optimization.
        /// </summary>
        /// <returns>Count of orders adopted (for logging)</returns>
        private int AdoptFleetOrders()
        {
            // [FREEZE-PROOF] Snapshot Account.All to prevent InvalidOperationException
            // if broker reconnects or modifies the collection during iteration.
            // Pattern verified in V12_002.SIMA.Flatten.cs line 51 and V12_002.SIMA.Fleet.cs line 489.
            Account[] accountSnapshot = Account.All.ToArray();
            int adoptedCount = 0;

            foreach (Account acct in accountSnapshot)
            {
                if (!IsFleetAccount(acct))
                    continue;
                try
                {
                    foreach (Order ord in acct.Orders.ToArray())
                    {
                        if (ord.Instrument?.FullName != Instrument?.FullName)
                            continue;
                        // [Codex P2] Include all live in-flight states -- Submitted/ChangePending/ChangeSubmitted
                        // can be active during an in-flight FSM replace at reconnect time.
                        // Setting _orderAdoptionComplete=true while these are skipped leaves REAPER
                        // auditing against incomplete order tracking and can fire false repair cycles.
                        if (
                            ord.OrderState != OrderState.Working
                            && ord.OrderState != OrderState.Accepted
                            && ord.OrderState != OrderState.Submitted
                            && ord.OrderState != OrderState.ChangePending
                            && ord.OrderState != OrderState.ChangeSubmitted
                        )
                            continue;

                        string name = ord.Name ?? string.Empty;
                        string classification = ClassifyOrderByPrefix(name);
                        if (classification == null)
                            continue; // Skip unrecognized orders

                        // Route to appropriate dictionary based on classification
                        ConcurrentDictionary<string, Order> targetDict = RouteOrderToTargetDict(
                            classification,
                            name,
                            out string key,
                            out string dictName
                        );

                        targetDict[key] = ord;

                        // [Build 980 Nexus] Rebuild activePositions structs so Rehydration does not lead to divergent REAPER audits.
                        if (targetDict == entryOrders && !activePositions.ContainsKey(key))
                        {
                            PositionInfo pos = RebuildFleetPositionFromEntry(ord, key);
                            activePositions[key] = pos;
                            Print(
                                string.Format(
                                    "[SIMA HYDRATE] Rebuilt activePositions struct for {0} | DNA: IsMOMO={1} IsRMA={2} IsTREND={3} IsRetest={4}",
                                    key,
                                    pos.IsMOMOTrade,
                                    pos.IsRMATrade,
                                    pos.IsTRENDTrade,
                                    pos.IsRetestTrade
                                )
                            );
                        }
                        else
                        {
                            // [Build 980 Phase 3]: Force-sync TotalContracts and ExecutingAccount if struct already exists.
                            PositionInfo existingPos;
                            if (activePositions.TryGetValue(key, out existingPos))
                            {
                                existingPos.TotalContracts = ord.Quantity;
                                existingPos.ExecutingAccount = acct;
                                Print(
                                    string.Format(
                                        "[SIMA HYDRATE] Force-synced TotalContracts={0} ExecutingAccount={1} for {2}",
                                        ord.Quantity,
                                        acct.Name,
                                        key
                                    )
                                );
                            }
                        }

                        Print(string.Format("[SIMA HYDRATE] Adopted working order {0} into {1}", name, dictName));
                        adoptedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Print(string.Format("[HYDRATE-ERROR] Fleet adoption failed for {0}: {1}", acct.Name, ex.Message));
                }
            }

            return adoptedCount;
        }

        /// <summary>
        /// Routes order to appropriate tracking dictionary based on classification.
        /// Extracts dictionary key from order name using classification-specific logic.
        /// Pure function - no side effects, deterministic output.
        /// </summary>
        /// <param name="classification">Order classification from ClassifyOrderByPrefix()</param>
        /// <param name="orderName">Full order name (e.g., "Stop_MOMO_001", "T1_TREND_002")</param>
        /// <param name="key">Output: Extracted dictionary key (e.g., "MOMO_001")</param>
        /// <param name="dictName">Output: Dictionary name for logging (e.g., "stopOrders")</param>
        /// <returns>Target ConcurrentDictionary reference, or null if classification invalid</returns>
        internal ConcurrentDictionary<string, Order> RouteOrderToTargetDict(
            string classification,
            string orderName,
            out string key,
            out string dictName
        )
        {
            ConcurrentDictionary<string, Order> targetDict = null;
            key = null;
            dictName = null;

            switch (classification)
            {
                case "stop":
                    targetDict = stopOrders;
                    key = orderName.StartsWith("Stop_", StringComparison.OrdinalIgnoreCase)
                        ? orderName.Substring(5)
                        : orderName.Substring(2);
                    dictName = "stopOrders";
                    break;
                case "target1":
                    targetDict = target1Orders;
                    key = orderName.Substring(3);
                    dictName = "target1Orders";
                    break;
                case "target2":
                    targetDict = target2Orders;
                    key = orderName.Substring(3);
                    dictName = "target2Orders";
                    break;
                case "target3":
                    targetDict = target3Orders;
                    key = orderName.Substring(3);
                    dictName = "target3Orders";
                    break;
                case "target4":
                    targetDict = target4Orders;
                    key = orderName.Substring(3);
                    dictName = "target4Orders";
                    break;
                case "target5":
                    targetDict = target5Orders;
                    key = orderName.Substring(3);
                    dictName = "target5Orders";
                    break;
                case "entry":
                    targetDict = entryOrders;
                    key = orderName;
                    dictName = "entryOrders";
                    break;
            }

            return targetDict;
        }

        /// <summary>
        /// Reconstructs a PositionInfo struct from a fleet entry order.
        /// Pure function - reads from entryOrders and stopOrders dictionaries.
        /// No state mutations, no concurrency concerns.
        /// </summary>
        /// <param name="entryOrder">Fleet entry order (prefix "Fleet_")</param>
        /// <param name="key">Position key (order name)</param>
        /// <returns>PositionInfo struct with position details</returns>
        private PositionInfo RebuildFleetPositionFromEntry(Order entryOrder, string key)
        {
            // Determine MarketPosition (preserve exact logic)
            MarketPosition mp =
                (entryOrder.OrderAction == OrderAction.Buy || entryOrder.OrderAction == OrderAction.BuyToCover)
                    ? MarketPosition.Long
                    : MarketPosition.Short;

            // Calculate entry price (LimitPrice fallback to StopPrice, then AverageFillPrice)
            double ePrice =
                entryOrder.LimitPrice != 0
                    ? entryOrder.LimitPrice
                    : (entryOrder.StopPrice != 0 ? entryOrder.StopPrice : entryOrder.AverageFillPrice);

            // Populate PositionInfo struct (preserve all fields from original)
            var pos = new PositionInfo
            {
                SignalName = key,
                Direction = mp,
                TotalContracts = entryOrder.Quantity,
                RemainingContracts = entryOrder.Quantity,
                EntryPrice = ePrice,
                InitialStopPrice = 0,
                CurrentStopPrice = 0,
                EntryOrderType = entryOrder.OrderType,
                EntryFilled = false,
                IsFollower = key.StartsWith("Fleet_", StringComparison.OrdinalIgnoreCase),
                ExecutingAccount = entryOrder.Account,
                BracketSubmitted = false,
                ExtremePriceSinceEntry = ePrice,
                CurrentTrailLevel = 0,
                OcoGroupId = "V12_" + GetStableHash(key),
            };

            // Get standard target distribution
            int t1Qty,
                t2Qty,
                t3Qty,
                t4Qty,
                t5Qty;
            GetTargetDistribution(entryOrder.Quantity, out t1Qty, out t2Qty, out t3Qty, out t4Qty, out t5Qty);
            pos.T1Contracts = t1Qty;
            pos.T2Contracts = t2Qty;
            pos.T3Contracts = t3Qty;
            pos.T4Contracts = t4Qty;
            pos.T5Contracts = t5Qty;

            // [Build 980 Phase 3]: Reconstruct trade DNA from signal name -- lost across restart.
            // Fleet entry names follow pattern: Fleet_<AcctName>_<TradeType>_<index>
            pos.IsMOMOTrade = key.IndexOf("_MOMO_", StringComparison.OrdinalIgnoreCase) >= 0;
            pos.IsRMATrade =
                key.IndexOf("_RMA_", StringComparison.OrdinalIgnoreCase) >= 0
                || key.IndexOf("_TREND_RMA_", StringComparison.OrdinalIgnoreCase) >= 0;
            pos.IsTRENDTrade = key.IndexOf("_TREND_", StringComparison.OrdinalIgnoreCase) >= 0;
            pos.IsRetestTrade = key.IndexOf("_RETEST_", StringComparison.OrdinalIgnoreCase) >= 0;
            if (pos.IsMOMOTrade)
                pos.IsRMATrade = false; // MOMO overrides generic RMA flag

            return pos;
        }

        /// <summary>
        /// Adopts working orders from the master account into tracking dictionaries.
        /// ACTOR-SERIALIZED: Must be called on strategy thread (via EnumerateApexAccounts).
        /// THREAD-SAFETY: Single-write operations to ConcurrentDictionary are safe.
        /// ORDERING: Must execute AFTER fleet adoption (Phase 1) and BEFORE FSM hydration (Phase 5).
        /// </summary>
        /// <returns>Count of orders adopted (for logging)</returns>
        private int AdoptMasterOrders()
        {
            int adoptedCount = 0;

            // Single account loop (master account only)
            foreach (Order ord in Account.Orders.ToArray())
            {
                if (ord.Instrument?.FullName != Instrument?.FullName)
                    continue;

                // State guard (includes master unknown state)
                // Build 994: Also accept Unknown -- NT8 Sim marks previous-session orders as Unknown.
                if (
                    ord.OrderState != OrderState.Working
                    && ord.OrderState != OrderState.Accepted
                    && ord.OrderState != OrderState.Submitted
                    && ord.OrderState != OrderState.ChangePending
                    && ord.OrderState != OrderState.ChangeSubmitted
                    && ord.OrderState != OrderState.Unknown
                )
                    continue;

                // Use shared classification helper (eliminates duplication)
                string name = ord.Name ?? string.Empty;
                string classification = ClassifyOrderByPrefix(name);
                if (classification == null || classification == "entry")
                    continue; // Skip unrecognized orders and Fleet_ entries (master has no Fleet_ orders)

                // Build dictionary key
                string key = name.StartsWith("Stop_", StringComparison.OrdinalIgnoreCase)
                    ? name.Substring(5)
                    : name.Substring(2);

                // Route to appropriate dictionary based on classification
                switch (classification)
                {
                    case "stop":
                        stopOrders[key] = ord;
                        break;
                    case "target1":
                        target1Orders[key] = ord;
                        break;
                    case "target2":
                        target2Orders[key] = ord;
                        break;
                    case "target3":
                        target3Orders[key] = ord;
                        break;
                    case "target4":
                        target4Orders[key] = ord;
                        break;
                    case "target5":
                        target5Orders[key] = ord;
                        break;
                }
                adoptedCount++;
            }

            return adoptedCount;
        }

        /// <summary>
        /// Classifies an order by its name prefix to determine target dictionary.
        /// Pure function - no state mutations, no concurrency concerns.
        /// </summary>
        /// <param name="orderName">Order name (e.g., "Stop_AAPL_123", "T1_AAPL_456")</param>
        /// <returns>Dictionary key: "stop", "target1"-"target5", "entry", or null if unrecognized</returns>
        private string ClassifyOrderByPrefix(string orderName)
        {
            if (string.IsNullOrEmpty(orderName))
                return null;

            // 8-way prefix classification (preserve exact logic from original if/else chains)
            if (orderName.StartsWith("Stop_", StringComparison.OrdinalIgnoreCase))
                return "stop";
            else if (orderName.StartsWith("S_", StringComparison.OrdinalIgnoreCase))
                return "stop";
            else if (orderName.StartsWith("T1_", StringComparison.OrdinalIgnoreCase))
                return "target1";
            else if (orderName.StartsWith("T2_", StringComparison.OrdinalIgnoreCase))
                return "target2";
            else if (orderName.StartsWith("T3_", StringComparison.OrdinalIgnoreCase))
                return "target3";
            else if (orderName.StartsWith("T4_", StringComparison.OrdinalIgnoreCase))
                return "target4";
            else if (orderName.StartsWith("T5_", StringComparison.OrdinalIgnoreCase))
                return "target5";
            else if (orderName.StartsWith("Fleet_", StringComparison.OrdinalIgnoreCase))
                return "entry";
            else
                return null; // Unrecognized prefix
        }

        /// <summary>
        /// Build 948 [FIX-A]: Sweep and cancel all V12-managed GTC orders before SIMA disable or strategy terminate.
        /// Phase 1 scans tracked order dicts; Phase 2 scans broker order lists for any V12-prefixed orders.
        /// force=true: cancel regardless of open positions (strategy terminate).
        /// force=false: skip accounts that have an open position for this instrument (SIMA disable -- prevent naked accounts).
        /// </summary>
        private void CancelAllV12GtcOrders(bool force)
        {
            int trackedCancels = SweepTrackedOrders(force);
            int brokerCancels = SweepBrokerOrders(force);
            Print(
                string.Format(
                    "[BUILD 948] GTC sweep: cancelled {0} tracked + {1} broker-scanned orders",
                    trackedCancels,
                    brokerCancels
                )
            );
        }

        /// <summary>Phase 1: cancel orders held in strategy tracking dictionaries.</summary>
        private int SweepTrackedOrders(bool force)
        {
            // Build 990: Semantic separation -- force=false (SIMA disable) cancels only entry orders.
            // Bracket orders (stop/target) are GTC with the broker and must remain to protect live positions.
            // force=true (strategy terminate) cancels all tracked orders.
            var trackedDicts = force
                ? new ConcurrentDictionary<string, Order>[]
                {
                    entryOrders,
                    stopOrders,
                    target1Orders,
                    target2Orders,
                    target3Orders,
                    target4Orders,
                    target5Orders,
                }
                : new ConcurrentDictionary<string, Order>[] { entryOrders };

            int trackedCancels = 0;
            foreach (var dict in trackedDicts)
            {
                if (dict == null)
                    continue;
                foreach (var kvp in dict.ToArray())
                {
                    Order ord = kvp.Value;
                    if (ord == null)
                        continue;
                    if (
                        ord.OrderState != OrderState.Working
                        && ord.OrderState != OrderState.Accepted
                        && ord.OrderState != OrderState.Submitted
                        && ord.OrderState != OrderState.ChangePending
                        && ord.OrderState != OrderState.ChangeSubmitted
                    )
                        continue;
                    try
                    {
                        CancelOrderOnAccount(ord, ord.Account);
                        trackedCancels++;
                    }
                    catch { }
                }
            }
            return trackedCancels;
        }

        /// <summary>
        /// Phase 2: broker-level scan to catch V12 orders not held in tracking dicts.
        /// Build 990: Semantic separation -- force=false only targets entry-signal prefixes.
        /// Bracket prefixes (Stop_, S_, T1_-T5_) are excluded on soft disable to protect live positions.
        /// </summary>
        private int SweepBrokerOrders(bool force)
        {
            int brokerCancels = 0;
            // Build 990: Semantic separation -- force=false (SIMA disable) only targets entry-signal prefixes.
            // Bracket prefixes (Stop_, S_, T1_-T5_) are excluded on soft disable to protect live positions.
            var v12Prefixes = force
                ? new[]
                {
                    "Stop_",
                    "S_",
                    "T1_",
                    "T2_",
                    "T3_",
                    "T4_",
                    "T5_",
                    "Fleet_",
                    "RMA",
                    "Trend",
                    "MOMO",
                    "OR",
                    "RETEST",
                    "FFMA",
                }
                : new[] { "Fleet_", "RMA", "Trend", "MOMO", "OR", "RETEST", "FFMA" };

            foreach (Account acct in Account.All)
            {
                if (!IsFleetAccount(acct))
                    continue;
                try
                {
                    foreach (Order ord in acct.Orders.ToArray())
                    {
                        if (ord.Instrument?.FullName != Instrument?.FullName)
                            continue;
                        if (
                            ord.OrderState != OrderState.Working
                            && ord.OrderState != OrderState.Accepted
                            && ord.OrderState != OrderState.Submitted
                            && ord.OrderState != OrderState.ChangePending
                            && ord.OrderState != OrderState.ChangeSubmitted
                        )
                            continue;
                        string ordName = ord.Name ?? string.Empty;
                        bool isV12 = false;
                        for (int pi = 0; pi < v12Prefixes.Length; pi++)
                        {
                            if (ordName.StartsWith(v12Prefixes[pi], StringComparison.OrdinalIgnoreCase))
                            {
                                isV12 = true;
                                break;
                            }
                        }
                        if (!isV12)
                            continue;

                        // [FIX-FF]: Explicit bracket exclusion on soft disable.
                        // Bracket orders protect live positions -- never cancel them during
                        // SIMA disable or soft terminate. Defensive guard against naming drift.
                        if (!force)
                        {
                            bool isBracketOrder =
                                ordName.StartsWith("Stop_", StringComparison.OrdinalIgnoreCase)
                                || ordName.StartsWith("S_", StringComparison.OrdinalIgnoreCase)
                                || ordName.StartsWith("T1_", StringComparison.OrdinalIgnoreCase)
                                || ordName.StartsWith("T2_", StringComparison.OrdinalIgnoreCase)
                                || ordName.StartsWith("T3_", StringComparison.OrdinalIgnoreCase)
                                || ordName.StartsWith("T4_", StringComparison.OrdinalIgnoreCase)
                                || ordName.StartsWith("T5_", StringComparison.OrdinalIgnoreCase)
                                || ordName.StartsWith("Target_", StringComparison.OrdinalIgnoreCase);
                            if (isBracketOrder)
                            {
                                Print(
                                    string.Format(
                                        "[FIX-FF] Protected bracket order from sweep: {0} on {1}",
                                        ordName,
                                        acct.Name
                                    )
                                );
                                continue;
                            }
                        }

                        try
                        {
                            acct.Cancel(new[] { ord });
                            brokerCancels++;
                        }
                        catch { }
                    }
                }
                catch { }
            }
            return brokerCancels;
        }

        #endregion
    }
}
