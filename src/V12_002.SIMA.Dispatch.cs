// Build 971: SIMA Dispatch -- ExecuteSmartDispatchEntry
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
        #region V12 SIMA Dispatch

        /// <summary>
        /// V12 SIMA: Execute a Smart Dispatched trade across the fleet.
        /// Logic:
        ///   - Signal = TREND: Lowest P/L account gets TREND targets, others get RMA targets.
        ///   - Signal = RMA/OR/MOMO: All accounts get RMA targets.
        /// Accounts use FIXED brackets (Path B) for zero trail lag.
        /// </summary>
        private void ExecuteSmartDispatchEntry(
            string tradeType,
            OrderAction action,
            int quantity,
            double entryPrice,
            OrderType entryOrderType = OrderType.Market,
            params string[] masterEntryNames
        )
        {
            // V12.Phase8 [F-03]: Lock-free gate guard -- non-blocking (Build 1109 freeze-proof).
            // Interlocked.CompareExchange returns instantly. If contended, defer to next strategy-thread cycle.
            if (Interlocked.CompareExchange(ref _simaToggleState, 1, 0) != 0)
            {
                Print("[DISPATCH] Semaphore contended -- deferring dispatch (non-blocking)");
                string _defTradeType = tradeType;
                OrderAction _defAction = action;
                int _defQty = quantity;
                double _defPrice = entryPrice;
                OrderType _defOrderType = entryOrderType;
                string[] _defMasterNames = masterEntryNames;
                try
                {
                    TriggerCustomEvent(
                        o =>
                            ExecuteSmartDispatchEntry(
                                _defTradeType,
                                _defAction,
                                _defQty,
                                _defPrice,
                                _defOrderType,
                                _defMasterNames
                            ),
                        null
                    );
                }
                catch
                {
                    Print("[DISPATCH] Deferred retry scheduling failed");
                }
                return;
            }

            Dispatch_InitializeLatencyTracking(
                out var sw,
                out var t0Ticks,
                out var tLoopStartTicks,
                out var dispatchLog
            );

            try
            {
                if (!Dispatch_ValidatePreconditions(tradeType, action, quantity, entryPrice))
                    return;

                Dispatch_ResolveFleetSnapshot(
                    tradeType,
                    action,
                    quantity,
                    entryPrice,
                    masterEntryNames,
                    out var fleet,
                    out var activeAccountSnapshot,
                    out var dispatchTargetCount,
                    out var symmetryDispatchId
                );
                if (fleet.Count == 0)
                    return;

                Dispatch_ProcessFleetLoop(
                    fleet,
                    activeAccountSnapshot,
                    dispatchTargetCount,
                    symmetryDispatchId,
                    tradeType,
                    action,
                    quantity,
                    entryPrice,
                    entryOrderType,
                    sw,
                    tLoopStartTicks,
                    dispatchLog
                );

                Dispatch_FinalizeAndReport(sw, t0Ticks, tLoopStartTicks, dispatchLog);
            }
            catch (Exception ex)
            {
                Print("[DISPATCH] CRITICAL ERROR in ExecuteSmartDispatchEntry: " + ex.Message);
            }
            finally
            {
                // V12.Phase8 [F-03]: Always release the SIMA toggle gate via Interlocked.Exchange.
                Interlocked.Exchange(ref _simaToggleState, 0);
            }
        }

        private bool Dispatch_ValidatePreconditions(
            string tradeType,
            OrderAction action,
            int quantity,
            double entryPrice
        )
        {
            // V12.2: Diagnostic logging for copy trading troubleshooting
            Print($"[DISPATCH] ExecuteSmartDispatchEntry called: {tradeType} | EnableSIMA={EnableSIMA}");

            if (!EnableSIMA)
            {
                Print("[DISPATCH] [ERR] SIMA DISABLED - Enable in strategy parameters to copy trade");
                return false;
            }

            // EMERGENCY FIX [H-12]: Abort dispatch if flatten is in progress to prevent re-entry race.
            if (isFlattenRunning)
            {
                Print("[DISPATCH] (!) Aborting dispatch -- flatten in progress (isFlattenRunning=true)");
                return false;
            }

            // Phase 6 [MG-D1]: MetadataGuard -- reject duplicate dispatch signals.
            // Composite fingerprint prevents the same trade from dispatching twice within 10s.
            string dispatchSig = LogBuffer.Format("SD_{0}_{1}_{2}_{3:F2}", tradeType, action, quantity, entryPrice);
            if (!MetadataGuardDuplicate(dispatchSig, "SmartDispatch"))
            {
                Print("[DISPATCH] (!) Duplicate dispatch rejected by MetadataGuard");
                return false;
            }

            return true;
        }

        private void Dispatch_InitializeLatencyTracking(
            out Stopwatch sw,
            out long t0Ticks,
            out long tLoopStartTicks,
            out StringBuilder dispatchLog
        )
        {
            // [Phase 7.2 LATENCY] T0: Start immediately after semaphore acquired, before any work.
            sw = Stopwatch.StartNew();
            t0Ticks = sw.ElapsedTicks;
            tLoopStartTicks = sw.ElapsedTicks;
            dispatchLog = new StringBuilder(DISPATCH_LOG_INITIAL_CAPACITY);
            dispatchLog.AppendLine(
                LogBuffer.Format(
                    "[LATENCY] Loop start at {0:F3} ms from entry",
                    (tLoopStartTicks - t0Ticks) * 1000.0 / Stopwatch.Frequency
                )
            );
        }

        private int Dispatch_ProcessFleetLoop(
            List<AccountRankInfo> fleet,
            HashSet<string> activeAccountSnapshot,
            int dispatchTargetCount,
            string symmetryDispatchId,
            string tradeType,
            OrderAction action,
            int quantity,
            double entryPrice,
            OrderType entryOrderType,
            Stopwatch sw,
            long tLoopStartTicks,
            StringBuilder dispatchLog
        )
        {
            int rmaCount = 0;

            for (int i = 0; i < fleet.Count; i++)
            {
                Account acct = fleet[i].Account;

                // V12.1: Skip Master account if its order was already placed by the caller
                if (acct == this.Account)
                    continue;

                // Build 935 [SIMA-B935-001]: Inactive + H-13 + consistency lock delegated to ShouldSkipFleetAccount.
                if (ShouldSkipFleetAccount(acct, fleet[i], activeAccountSnapshot, dispatchLog))
                    continue;

                // [GREPTILE-P0]: Circuit breaker fast-exit BEFORE allocation.
                // Prevents wasteful CreateOrder + PositionInfo heap allocations when breaker is tripped.
                // Jane Street Alignment: Zero-tolerance for allocation-before-guard patterns.
                if (Volatile.Read(ref _reaperCircuitBreakerTripped) == 1)
                {
                    dispatchLog.AppendLine($"[DISPATCH] CB tripped - skipping {acct.Name} (no allocation)");
                    continue;
                }

                int reservedDelta = 0;
                bool registeredForCleanup = false;
                bool syncPending = false;
                string fleetEntryName = null;
                string expectedKey = null;
                try
                {
                    bool _builtOk = Dispatch_BuildFollowerOrders(
                        tradeType,
                        action,
                        quantity,
                        entryPrice,
                        entryOrderType,
                        acct,
                        i,
                        symmetryDispatchId,
                        dispatchTargetCount,
                        dispatchLog,
                        out PositionInfo fleetPos,
                        out Order entry,
                        out fleetEntryName,
                        out expectedKey,
                        out string ocoId,
                        out int followerQty,
                        out int ft1,
                        out int ft2,
                        out int ft3,
                        out int ft4,
                        out int ft5,
                        out double stopPrice,
                        out double t1TargetPrice,
                        out double t2TargetPrice,
                        out double t3TargetPrice,
                        out double t4TargetPrice,
                        out double t5TargetPrice
                    );
                    if (!_builtOk)
                        continue;
                    bool isMarketEntry = (entryOrderType == OrderType.Market);

                    // V12.7: Submit only entry for Limit; market entries include stop + non-runner targets.
                    if (isMarketEntry)
                    {
                        Dispatch_PublishMarketBracketToPhoton(
                            acct,
                            action,
                            entry,
                            fleetPos,
                            fleetEntryName,
                            expectedKey,
                            ocoId,
                            followerQty,
                            entryPrice,
                            stopPrice,
                            dispatchTargetCount,
                            symmetryDispatchId,
                            dispatchLog,
                            ref syncPending,
                            ref reservedDelta,
                            ref registeredForCleanup
                        );
                    }
                    else
                    {
                        Dispatch_PublishLimitEntryToPhoton(
                            acct,
                            action,
                            fleetPos,
                            entry,
                            fleetEntryName,
                            expectedKey,
                            followerQty,
                            dispatchLog,
                            ref syncPending,
                            ref reservedDelta,
                            ref registeredForCleanup
                        );
                    }

                    rmaCount++;
                }
                catch (Exception ex)
                {
                    if (syncPending)
                    {
                        ClearDispatchSyncPending(expectedKey);
                        syncPending = false;
                    }

                    if (reservedDelta != 0)
                        AddExpectedPositionDeltaLocked(expectedKey, -reservedDelta);

                    if (registeredForCleanup)
                    {
                        // V12.Phase8 [F-01]: Full tracking-dict cleanup on Submit failure.
                        activePositions.TryRemove(fleetEntryName, out _);
                        entryOrders.TryRemove(fleetEntryName, out _);
                        stopOrders.TryRemove(fleetEntryName, out _);
                        for (int tNum = 1; tNum <= 5; tNum++)
                        {
                            var targetDict = GetTargetOrdersDictionary(tNum);
                            if (targetDict != null)
                                targetDict.TryRemove(fleetEntryName, out _);
                        }
                    }
                    // Phase 6: Clean up proactive FSM on dispatch failure (no-op if not yet created)
                    if (!string.IsNullOrEmpty(fleetEntryName))
                        _followerBrackets.TryRemove(fleetEntryName, out _);

                    dispatchLog.AppendLine($"[DISPATCH] [X] FAILED on {acct.Name}: {ex.Message}");
                }
            }

            return rmaCount;
        }

        private void Dispatch_FinalizeAndReport(
            Stopwatch sw,
            long t0Ticks,
            long tLoopStartTicks,
            StringBuilder dispatchLog
        )
        {
            // V14.2 FIX-F7: Pump prime checks BOTH ring and legacy queue
            if ((_photonDispatchRing != null && !_photonDispatchRing.IsEmpty) || !_pendingFleetDispatches.IsEmpty)
                try
                {
                    TriggerCustomEvent(o => PumpFleetDispatch(), null);
                }
                catch (Exception ex)
                {
                    if (_diagFleet)
                        Print("[FLEET_CATCH] ExecuteSmartDispatchEntry pump prime failed: " + ex.Message);
                }

            // [Phase 7.2 LATENCY] T_Final: Fleet loop complete (setup+enqueue only; no blocking Submit) -- stop clock, flush forensic report.
            sw.Stop();
            long tFinalTicks = sw.ElapsedTicks;
            double totalMs = tFinalTicks * 1000.0 / Stopwatch.Frequency;
            double setupMs = (tLoopStartTicks - t0Ticks) * 1000.0 / Stopwatch.Frequency;
            double loopMs = (tFinalTicks - tLoopStartTicks) * 1000.0 / Stopwatch.Frequency;

            var report = new StringBuilder(1024);
            report.AppendLine("+==============================================================+");
            report.AppendLine("|          (+/-)  FORENSIC PULSE REPORT  Phase 7.2 Latency       |");
            report.AppendLine("+==============================================================+");
            report.AppendLine("|  TYPE | ACCOUNT                       | ORDER TYPE   |   RTT  |");
            report.AppendLine("+==============================================================+");
            report.Append(dispatchLog.ToString());
            report.AppendLine("+--------------------------------------------------------------+");
            report.AppendLine("|  TIMING SUMMARY                                              |");
            report.AppendLine("+--------------------------------------------------------------+");
            report.AppendLine(
                LogBuffer.Format("|  Setup Phase:  {0,8:F3} ms  |  Fleet Loop:  {1,8:F3} ms       |", setupMs, loopMs)
            );
            report.AppendLine(
                LogBuffer.Format("|  Total Elapsed: {0,8:F3} ms                                  |", totalMs)
            );
            report.AppendLine("+==============================================================+");
            Print(report.ToString().TrimEnd());
        }

        private void Dispatch_ResolveFleetSnapshot(
            string tradeType,
            OrderAction action,
            int quantity,
            double entryPrice,
            string[] masterEntryNames,
            out List<AccountRankInfo> fleet,
            out HashSet<string> activeAccountSnapshot,
            out int dispatchTargetCount,
            out string symmetryDispatchId
        )
        {
            // V12.Audit [Q3-002]: Snapshot fleet active state under stateLock to prevent UI race.
            // The UI/IPC thread can toggle activeFleetAccounts between TryGetValue and Submit,
            // so we capture a consistent set of active account names once before the dispatch loop.
            // FIX-B [Build 1102Z]: Snapshot activeTargetCount atomically with the fleet snapshot.
            // The IPC SET_TARGET_COUNT command writes activeTargetCount on the TCP listener thread,
            // so a live read inside the fleet loop (line below) can produce a different bound for
            // different accounts. Capturing once here ensures all fleet accounts submit identical
            // target counts for this dispatch.
            // P0-3 FIX: Replace LINQ with manual iteration to avoid hot-path allocation
            activeAccountSnapshot = new HashSet<string>();
            foreach (var kvp in activeFleetAccounts)
            {
                if (kvp.Value)
                    activeAccountSnapshot.Add(kvp.Key);
            }
            dispatchTargetCount = Math.Max(1, Math.Min(5, activeTargetCount));

            fleet = GetSortedAccountFleet();
            int activeCount = activeAccountSnapshot.Count;

            // V12.2: Log fleet state for diagnostics
            Print($"[DISPATCH] Fleet: {fleet.Count} total accounts | {activeCount} ACTIVE in Fleet Manager");
            if (fleet.Count == 0)
            {
                Print("[DISPATCH] [ERR] NO APEX ACCOUNTS DETECTED - Check AccountPrefix setting");
                symmetryDispatchId = null;
                return;
            }

            if (activeCount == 0)
            {
                Print("[DISPATCH] [ERR] NO ACCOUNTS ENABLED - Toggle accounts ON in Fleet Manager panel");
            }

            symmetryDispatchId = SymmetryGuardBeginDispatch(tradeType, action, quantity, entryPrice);
            if (masterEntryNames != null)
            {
                foreach (string masterEntryName in masterEntryNames)
                {
                    if (!string.IsNullOrEmpty(masterEntryName))
                        SymmetryGuardRegisterMasterEntry(symmetryDispatchId, masterEntryName);
                }
            }
        }

        /// <summary>
        /// Build follower orders for a single account in the fleet dispatch loop.
        /// Calculates prices, quantities, and creates the entry order object.
        /// Returns false if account should be skipped (e.g., position conflicts).
        /// </summary>
        private bool Dispatch_BuildFollowerOrders(
            string tradeType,
            OrderAction action,
            int quantity,
            double entryPrice,
            OrderType entryOrderType,
            Account acct,
            int accountIndex,
            string symmetryDispatchId,
            int dispatchTargetCount,
            StringBuilder dispatchLog,
            out PositionInfo fleetPos,
            out Order entry,
            out string fleetEntryName,
            out string expectedKey,
            out string ocoId,
            out int followerQty,
            out int ft1,
            out int ft2,
            out int ft3,
            out int ft4,
            out int ft5,
            out double stopPrice,
            out double t1TargetPrice,
            out double t2TargetPrice,
            out double t3TargetPrice,
            out double t4TargetPrice,
            out double t5TargetPrice
        )
        {
            // Initialize all out parameters
            fleetPos = null;
            entry = null;
            fleetEntryName = null;
            expectedKey = null;
            ocoId = null;
            // P0-5 FIX: Apply fleet parity scaling to follower quantity
            followerQty = quantity * FleetParityMultiplier;
            ft1 = ft2 = ft3 = ft4 = ft5 = 0;
            stopPrice = t1TargetPrice = 0;
            t2TargetPrice = t3TargetPrice = t4TargetPrice = t5TargetPrice = 0;

            // Generate unique fleet entry name
            fleetEntryName = LogBuffer.Format("Fleet_{0}_{1}_{2}", acct.Name, tradeType, accountIndex);

            // P0-4 FIX: Use ExpKey helper for consistent dictionary key generation
            expectedKey = ExpKey(acct.Name);

            // P0-5 FIX: Use fleetEntryName as key for activePositions (not acct.Name)
            if (!activePositions.TryGetValue(fleetEntryName, out fleetPos))
            {
                fleetPos = new PositionInfo();
                activePositions[fleetEntryName] = fleetPos;
            }

            // P0-6 FIX: Generate unique OCO ID per fleet account (include fleetEntryName)
            ocoId = LogBuffer.Format("V12_{0}_{1}", fleetEntryName, DateTime.UtcNow.Ticks);

            // Create entry order signal name
            string entrySig = SymmetryTrim("Entry_" + fleetEntryName, 40);

            // Create the entry order
            entry = acct.CreateOrder(
                Instrument,
                action,
                entryOrderType,
                TimeInForce.Gtc,
                followerQty,
                entryOrderType == OrderType.Limit ? entryPrice : 0,
                0,
                ocoId,
                entrySig,
                null
            );

            if (entry == null)
            {
                Print($"[DISPATCH] [X] CreateOrder returned null for {acct.Name}");
                return false;
            }

            // P0-2/P1-1 FIX: Restore ATR-based bracket calculation (not hardcoded ticks)
            // Calculate stop distance using ATR-based logic
            double atrMultiplier = StopMultiplier; // Use configured stop multiplier
            double atrStopDistance = CalculateATRStopDistance(atrMultiplier);
            
            MarketPosition direction;
            if (action == OrderAction.Buy)
            {
                stopPrice = entryPrice - atrStopDistance;
                direction = MarketPosition.Long;
            }
            else
            {
                stopPrice = entryPrice + atrStopDistance;
                direction = MarketPosition.Short;
            }

            // Round stop to tick size
            stopPrice = Instrument.MasterInstrument.RoundToTickSize(stopPrice);

            // Calculate all target prices based on dispatchTargetCount
            t1TargetPrice = CalculateTargetPrice(direction, entryPrice, 1);
            t1TargetPrice = Instrument.MasterInstrument.RoundToTickSize(t1TargetPrice);
            
            if (dispatchTargetCount >= 2)
            {
                t2TargetPrice = CalculateTargetPrice(direction, entryPrice, 2);
                t2TargetPrice = Instrument.MasterInstrument.RoundToTickSize(t2TargetPrice);
            }
            
            if (dispatchTargetCount >= 3)
            {
                t3TargetPrice = CalculateTargetPrice(direction, entryPrice, 3);
                t3TargetPrice = Instrument.MasterInstrument.RoundToTickSize(t3TargetPrice);
            }
            
            if (dispatchTargetCount >= 4)
            {
                t4TargetPrice = CalculateTargetPrice(direction, entryPrice, 4);
                t4TargetPrice = Instrument.MasterInstrument.RoundToTickSize(t4TargetPrice);
            }
            
            if (dispatchTargetCount >= 5)
            {
                t5TargetPrice = CalculateTargetPrice(direction, entryPrice, 5);
                t5TargetPrice = Instrument.MasterInstrument.RoundToTickSize(t5TargetPrice);
            }

            // Set target quantities (distribute across targets based on dispatchTargetCount)
            GetTargetDistribution(followerQty, out ft1, out ft2, out ft3, out ft4, out ft5, dispatchTargetCount);

            // Persist bracket data to fleetPos BEFORE publish
            fleetPos.InitialStopPrice = stopPrice;
            fleetPos.CurrentStopPrice = stopPrice;
            fleetPos.Target1Price = t1TargetPrice;
            fleetPos.Target2Price = t2TargetPrice;
            fleetPos.Target3Price = t3TargetPrice;
            fleetPos.Target4Price = t4TargetPrice;
            fleetPos.Target5Price = t5TargetPrice;
            fleetPos.T1Contracts = ft1;
            fleetPos.T2Contracts = ft2;
            fleetPos.T3Contracts = ft3;
            fleetPos.T4Contracts = ft4;
            fleetPos.T5Contracts = ft5;

            return true;
        }

        /// <summary>
        /// Phase 7 NEW-3: Thin router for market bracket dispatch.
        /// Delegates entry, stop, and target publishing to focused helpers.
        /// Target CYC: <=4 (reduced from 21).
        /// </summary>
        private void Dispatch_PublishMarketBracketToPhoton(
            Account acct,
            OrderAction action,
            Order entry,
            PositionInfo fleetPos,
            string fleetEntryName,
            string expectedKey,
            string ocoId,
            int followerQty,
            double entryPrice,
            double stopPrice,
            int dispatchTargetCount,
            string symmetryDispatchId,
            StringBuilder dispatchLog,
            ref bool syncPending,
            ref int reservedDelta,
            ref bool registeredForCleanup
        )
        {
            var ordersToSubmit = new List<Order> { entry };
            OrderAction exitAction = action == OrderAction.Buy ? OrderAction.Sell : OrderAction.BuyToCover;

            // Publish entry order
            // Entry already added to ordersToSubmit by caller - no additional MMIO publish needed

            // Publish stop order
            Order stop = PublishPhoton_StopOrder(
                acct,
                exitAction,
                fleetPos,
                fleetEntryName,
                ocoId,
                stopPrice,
                ref ordersToSubmit
            );

            // [Round 4 Fix] P1 CRITICAL: Check for null stop order
            if (stop == null)
            {
                Print($"[PublishPhoton_FleetOrders] Stop creation failed for {fleetEntryName} - aborting dispatch");

                // Rollback: Remove from registeredForCleanup if already added
                if (registeredForCleanup)
                {
                    registeredForCleanup = false;
                }

                return; // Do NOT proceed to dictionary registration
            }

            // Publish target orders
            var stagedTargets = PublishPhoton_TargetOrders(
                acct,
                exitAction,
                fleetPos,
                fleetEntryName,
                ocoId,
                dispatchTargetCount,
                dispatchLog,
                ref ordersToSubmit,
                out int nonRunnerLimitQty,
                out int runnerQty
            );

            // Register tracking dictionaries
            RegisterTrackingDictionaries(
                fleetEntryName,
                fleetPos,
                entry,
                stop,
                stagedTargets,
                expectedKey,
                ref syncPending,
                ref registeredForCleanup
            );

            // Initialize FSM
            InitializeFollowerBracketFSM(acct, fleetEntryName, followerQty, entry, stop, ocoId, stagedTargets);

            // P1-1 FIX: Register follower with symmetry guard for fleet tracking
            SymmetryGuardRegisterFollower(symmetryDispatchId, fleetEntryName);

            // Reserve expected quantity
            reservedDelta = (action == OrderAction.Buy) ? followerQty : -followerQty;
            AddExpectedPositionDeltaLocked(expectedKey, reservedDelta);

            // V14.2 [ADR-012]: Zero-allocation dispatch via PhotonPool + SPSC ring
            var (_proxyOrders, _poolSlotIndex) = ClaimPhotonPoolSlot();

            FleetDispatchSlot _slot = PopulatePhotonSlot(
                entry,
                stop,
                stagedTargets,
                _proxyOrders,
                _poolSlotIndex,
                entryPrice,
                stopPrice,
                followerQty,
                dispatchTargetCount,
                action,
                reservedDelta
            );

            // REAPER-EXPANSION Ticket 2: Circuit breaker check
            if (
                !TryIncrementDispatchCountWithCircuitBreaker(
                    ref syncPending,
                    expectedKey,
                    ref reservedDelta,
                    _poolSlotIndex,
                    fleetEntryName,
                    out bool circuitBreakerTripped
                )
            )
            {
                return;
            }

            // Enqueue to ring or fallback
            int _orderIdx = 2 + stagedTargets.Count; // entry + stop + targets
            EnqueueToPhotonRing(
                ref _slot,
                _proxyOrders,
                _poolSlotIndex,
                _orderIdx,
                acct,
                fleetEntryName,
                expectedKey,
                reservedDelta
            );

            syncPending = false;
            reservedDelta = 0;
            registeredForCleanup = false;

            LogDispatchCompletion(
                dispatchLog,
                acct,
                ordersToSubmit.Count,
                fleetEntryName,
                fleetPos.TotalContracts,
                nonRunnerLimitQty,
                runnerQty
            );
        }

        /// <summary>
        /// Phase 7 CCN-19 Helper 1: Log dispatch completion to StringBuilder.
        /// Target CYC: <=1.
        /// </summary>
        private void LogDispatchCompletion(
            StringBuilder dispatchLog,
            Account acct,
            int orderCount,
            string fleetEntryName,
            int totalContracts,
            int nonRunnerLimitQty,
            int runnerQty
        )
        {
            dispatchLog.AppendLine(
                LogBuffer.Format("  QUEUE | {0,-28} | Market+{1}orders | PENDING", acct.Name, orderCount)
            );
            dispatchLog.AppendLine(
                LogBuffer.Format(
                    "[SIMA STOP_AUDIT] QUEUED {0}: StopQty={1} NonRunnerLimits={2} RunnerQty={3}",
                    fleetEntryName,
                    totalContracts,
                    nonRunnerLimitQty,
                    runnerQty
                )
            );
        }

        /// <summary>
        /// Phase 7 NEW-3 Helper 2: Create and publish stop order to MMIO.
        /// Target CYC: <=5.
        /// </summary>
        private Order PublishPhoton_StopOrder(
            Account acct,
            OrderAction exitAction,
            PositionInfo fleetPos,
            string fleetEntryName,
            string ocoId,
            double stopPrice,
            ref List<Order> ordersToSubmit
        )
        {
            double validatedStop = ValidateStopPrice(fleetPos.Direction, stopPrice);
            string stopSig = SymmetryTrim("Stop_" + fleetEntryName, 40);
            Order stop = acct.CreateOrder(
                Instrument,
                exitAction,
                OrderType.StopMarket,
                TimeInForce.Gtc,
                Math.Max(1, fleetPos.TotalContracts),
                0,
                validatedStop,
                ocoId,
                stopSig,
                null
            );
            if (stop == null)
            {
                Print($"[PublishPhoton_StopOrder] CreateOrder returned null for {fleetEntryName}");
                return null;
            }
            ordersToSubmit.Add(stop);
            return stop;
        }

        /// <summary>
        /// [Round 4 Fix] P2: Extract single target order creation logic
        /// Reduces PublishPhoton_TargetOrders complexity (LOC 72->30)
        /// Target CYC: <=5
        /// </summary>
        private Order CreateSingleTargetOrder(
            Account acct,
            OrderAction exitAction,
            string fleetEntryName,
            int targetQty,
            double targetPrice,
            string ocoId,
            int targetNum
        )
        {
            string targetSig = SymmetryTrim("T" + targetNum + "_" + fleetEntryName, 40);
            Order target = acct.CreateOrder(
                Instrument,
                exitAction,
                OrderType.Limit,
                TimeInForce.Gtc,
                targetQty,
                targetPrice,
                0,
                ocoId,
                targetSig,
                null
            );
            return target;
        }

        /// <summary>
        /// Phase 7 NEW-3 Helper 3: Create and publish target orders to MMIO.
        /// Target CYC: <=5.
        /// </summary>
        private List<StagedTarget> PublishPhoton_TargetOrders(
            Account acct,
            OrderAction exitAction,
            PositionInfo fleetPos,
            string fleetEntryName,
            string ocoId,
            int dispatchTargetCount,
            StringBuilder dispatchLog,
            ref List<Order> ordersToSubmit,
            out int nonRunnerLimitQty,
            out int runnerQty
        )
        {
            nonRunnerLimitQty = 0;
            runnerQty = 0;
            var stagedTargets = new List<StagedTarget>(5);

            for (int targetNum = 1; targetNum <= dispatchTargetCount; targetNum++)
            {
                int targetQty = GetTargetContracts(fleetPos, targetNum);
                if (targetQty <= 0)
                {
                    continue;
                }

                if (IsRunnerTarget(targetNum))
                {
                    runnerQty += targetQty;
                    continue;
                }

                double targetPrice = GetTargetPrice(fleetPos, targetNum);
                if (targetPrice <= 0)
                {
                    dispatchLog.AppendLine(
                        string.Format(
                            "[SIMA TARGET_SKIP] T{0} for {1} has qty={2} but invalid price={3:F2}; skipped",
                            targetNum,
                            fleetEntryName,
                            targetQty,
                            targetPrice
                        )
                    );
                    continue;
                }

                Order target = CreateSingleTargetOrder(
                    acct,
                    exitAction,
                    fleetEntryName,
                    targetQty,
                    targetPrice,
                    ocoId,
                    targetNum
                );

                if (target == null)
                {
                    dispatchLog.AppendLine($"[Target {targetNum}] CreateOrder returned null - skipping");
                    continue;
                }

                stagedTargets.Add(
                    new StagedTarget
                    {
                        Num = targetNum,
                        Price = targetPrice,
                        Order = target,
                    }
                );

                ordersToSubmit.Add(target);
                nonRunnerLimitQty += targetQty;
            }

            return stagedTargets;
        }

        /// <summary>
        /// Phase 7 CCN-19 Helper 2: Register orders in tracking dictionaries.
        /// Target CYC: <=4.
        /// </summary>
        private void RegisterTrackingDictionaries(
            string fleetEntryName,
            PositionInfo fleetPos,
            Order entry,
            Order stop,
            List<StagedTarget> stagedTargets,
            string expectedKey,
            ref bool syncPending,
            ref bool registeredForCleanup
        )
        {
            activePositions[fleetEntryName] = fleetPos;
            entryOrders[fleetEntryName] = entry;
            stopOrders[fleetEntryName] = stop;
            foreach (var st in stagedTargets)
            {
                var targetDict = GetTargetOrdersDictionary(st.Num);
                if (targetDict != null)
                    targetDict[fleetEntryName] = st.Order;
            }
            registeredForCleanup = true;
            MarkDispatchSyncPending(expectedKey);
            syncPending = true;
        }

        /// <summary>
        /// Phase 7 CCN-19 Helper 3: Initialize FollowerBracketFSM if not exists.
        /// Target CYC: <=3.
        /// </summary>
        private void InitializeFollowerBracketFSM(
            Account acct,
            string fleetEntryName,
            int followerQty,
            Order entry,
            Order stop,
            string ocoId,
            List<StagedTarget> stagedTargets
        )
        {
            // Phase 6 [FSM-P1]: Proactive FSM
            if (!_followerBrackets.ContainsKey(fleetEntryName))
            {
                var proFsm = new FollowerBracketFSM
                {
                    AccountName = acct.Name,
                    EntryName = fleetEntryName,
                    State = FollowerBracketState.PendingSubmit,
                    RemainingContracts = followerQty,
                    EntryOrder = entry,
                    ExpectedEntryPrice = entry.LimitPrice > 0 ? entry.LimitPrice : 0,
                    StopOrder = stop,
                    ExpectedStopPrice = stop != null ? stop.StopPrice : 0,
                    OcoGroupId = ocoId,
                    LastUpdateUtc = DateTime.UtcNow,
                };
                foreach (var st in stagedTargets)
                {
                    if (st.Num >= 1 && st.Num <= 5)
                    {
                        proFsm.Targets[st.Num - 1] = st.Order;
                        proFsm.ExpectedTargetPrices[st.Num - 1] = st.Price;
                    }
                }
                _followerBrackets.TryAdd(fleetEntryName, proFsm);
            }
        }

        /// <summary>
        /// Phase 7 CCN-19 Helper 4: Claim photon pool slot with fallback to heap allocation.
        /// Target CYC: <=3.
        /// </summary>
        private (Order[] Orders, int SlotIndex) ClaimPhotonPoolSlot()
        {
            var claimed = _photonPool.Claim();
            if (claimed.Orders != null)
            {
                return (claimed.Orders, claimed.SlotIndex);
            }
            else
            {
                TrackPhotonPoolExhausted();
                Print("[PHOTON] Pool exhausted -- fallback to heap alloc");
                return (new Order[MaxOrdersPerSlot], -1);
            }
        }

        /// <summary>
        /// Phase 7 CCN-19 Helper 5: Populate FleetDispatchSlot with orders and metadata.
        /// Target CYC: <=2.
        /// </summary>
        private FleetDispatchSlot PopulatePhotonSlot(
            Order entry,
            Order stop,
            List<StagedTarget> stagedTargets,
            Order[] proxyOrders,
            int poolSlotIndex,
            double entryPrice,
            double stopPrice,
            int followerQty,
            int dispatchTargetCount,
            OrderAction action,
            int reservedDelta
        )
        {
            int orderIdx = 0;
            proxyOrders[orderIdx++] = entry;
            proxyOrders[orderIdx++] = stop;
            foreach (var st in stagedTargets)
                proxyOrders[orderIdx++] = st.Order;

            FleetDispatchSlot slot = new FleetDispatchSlot
            {
                EntryPrice = entryPrice,
                StopPrice = stopPrice,
                SignalTicks = DateTime.UtcNow.Ticks,
                PoolSlotIndex = poolSlotIndex,
                OrderCount = orderIdx,
                Quantity = followerQty,
                TargetCount = dispatchTargetCount,
                Action = (int)action,
                ReservedDelta = reservedDelta,
            };
            slot.Shadow = ComputeFleetDispatchShadow(ref slot, _photonShadowSalt);

            return slot;
        }

        /// <summary>
        /// Phase 7 CCN-19 Helper 6: Enqueue slot to SPSC ring or fallback to ConcurrentQueue.
        /// Target CYC: <=5.
        /// </summary>
        private void EnqueueToPhotonRing(
            ref FleetDispatchSlot slot,
            Order[] proxyOrders,
            int poolSlotIndex,
            int orderCount,
            Account acct,
            string fleetEntryName,
            string expectedKey,
            int reservedDelta
        )
        {
            // v28.0 blittable slot + sideband-first publish
            if (poolSlotIndex >= 0)
            {
                _photonSideband[poolSlotIndex].Account = acct;
                _photonSideband[poolSlotIndex].FleetEntryName = fleetEntryName;
                _photonSideband[poolSlotIndex].ExpectedKey = expectedKey;
                Thread.MemoryBarrier();
            }

            if (poolSlotIndex >= 0 && _photonDispatchRing.TryEnqueue(ref slot))
            {
                TrackPhotonEnqueue();
                // MMIO mirror best-effort write-through
                if (_photonMmioMirror != null)
                {
                    try
                    {
                        _photonMmioMirror.TryPublish(ref slot);
                    }
                    catch (Exception ex)
                    {
                        if (_diagIpc)
                            Print("[IPC_CATCH] Dispatch_PublishMarketBracketToPhoton MMIO failed: " + ex.Message);
                    }
                }
            }
            else
            {
                // Ring full or pool exhausted -- fallback to ConcurrentQueue
                if (poolSlotIndex >= 0)
                {
                    TrackPhotonRingFull();
                    Print("[PHOTON] Ring full -- fallback to ConcurrentQueue");
                    Order[] legacyOrders = new Order[orderCount];
                    Array.Copy(proxyOrders, legacyOrders, orderCount);
                    _photonPool.ReleaseByIndex(poolSlotIndex);
                    _photonSideband[poolSlotIndex] = default(FleetDispatchSideband);
                    proxyOrders = legacyOrders;
                }
                _pendingFleetDispatches.Enqueue(
                    new FleetDispatchRequest
                    {
                        Account = acct,
                        Orders = proxyOrders,
                        FleetEntryName = fleetEntryName,
                        ExpectedKey = expectedKey,
                        ReservedDelta = reservedDelta,
                        SignalTicks = DateTime.UtcNow.Ticks,
                    }
                );
            }
        }
        /// <summary>
        /// P2-3 FIX: Centralized enqueue helper for limit entry dispatch.
        /// Matches market path pattern to prevent divergence.
        /// Target CYC: <=5.
        /// </summary>
        private void EnqueueLimitEntryToPhotonRing(
            ref FleetDispatchSlot slot,
            Order[] proxyOrders,
            int poolSlotIndex,
            Order entry,
            Account acct,
            string fleetEntryName,
            string expectedKey,
            int reservedDelta
        )
        {
            if (poolSlotIndex >= 0 && _photonDispatchRing.TryEnqueue(ref slot))
            {
                TrackPhotonEnqueue();
                if (_photonMmioMirror != null)
                {
                    try
                    {
                        _photonMmioMirror.TryPublish(ref slot);
                    }
                    catch (Exception ex)
                    {
                        if (_diagIpc)
                            Print("[IPC_CATCH] Dispatch_BuildFollowerOrders MMIO failed: " + ex.Message);
                    }
                }
            }
            else
            {
                if (poolSlotIndex >= 0)
                {
                    TrackPhotonRingFull();
                    Order[] legacyOrdersLmt = new Order[] { entry };
                    _photonPool.ReleaseByIndex(poolSlotIndex);
                    _photonSideband[poolSlotIndex] = default(FleetDispatchSideband);
                    proxyOrders = legacyOrdersLmt;
                }
                _pendingFleetDispatches.Enqueue(
                    new FleetDispatchRequest
                    {
                        Account = acct,
                        Orders = proxyOrders,
                        FleetEntryName = fleetEntryName,
                        ExpectedKey = expectedKey,
                        ReservedDelta = reservedDelta,
                        SignalTicks = DateTime.UtcNow.Ticks,
                    }
                );
            }
        }


        private void Dispatch_PublishLimitEntryToPhoton(
            Account acct,
            OrderAction action,
            PositionInfo fleetPos,
            Order entry,
            string fleetEntryName,
            string expectedKey,
            int followerQty,
            StringBuilder dispatchLog,
            ref bool syncPending,
            ref int reservedDelta,
            ref bool registeredForCleanup
        )
        {
            // V12.Phantom-Fix [FIX-1]: Register tracking dicts BEFORE updating expectedPositions.
            // REAPER runs on a background thread; if it fires between the expectedPositions
            // update and the dict commit (the old T1->T3 race), it observes non-zero expected
            // with no entry in entryOrders -> hasWorkingEntry=false -> phantom repair queued.
            // Registering dicts first guarantees REAPER always finds the blocking entry.
            // B966: Enqueue NOT applied -- ordering invariant: dict BEFORE expectedPositions update (Phantom-Fix).
            // ConcurrentDictionary single-writes are thread-safe here.
            activePositions[fleetEntryName] = fleetPos;
            entryOrders[fleetEntryName] = entry; // V12.3: Track entry for CIT chase
            registeredForCleanup = true;
            MarkDispatchSyncPending(expectedKey);
            syncPending = true;

            // Phase 6 [FSM-P1]: Proactive FSM for limit entry (entry-only, no brackets).
            if (!_followerBrackets.ContainsKey(fleetEntryName))
            {
                var proFsm = new FollowerBracketFSM
                {
                    AccountName = acct.Name,
                    EntryName = fleetEntryName,
                    State = FollowerBracketState.PendingSubmit,
                    RemainingContracts = followerQty,
                    EntryOrder = entry,
                    ExpectedEntryPrice = entry.LimitPrice > 0 ? entry.LimitPrice : 0,
                    LastUpdateUtc = DateTime.UtcNow,
                };
                _followerBrackets.TryAdd(fleetEntryName, proFsm);
            }

            reservedDelta = (action == OrderAction.Buy) ? followerQty : -followerQty;
            AddExpectedPositionDeltaLocked(expectedKey, reservedDelta);

            int _poolSlotIndexLmt = -1;
            Order[] _proxyOrdersLmt = null;
            {
                var _claimedLmt = _photonPool.Claim();
                if (_claimedLmt.Orders != null)
                {
                    _proxyOrdersLmt = _claimedLmt.Orders;
                    _poolSlotIndexLmt = _claimedLmt.SlotIndex;
                }
                else
                {
                    TrackPhotonPoolExhausted();
                    _proxyOrdersLmt = new Order[MaxOrdersPerSlot];
                    _poolSlotIndexLmt = -1;
                }
            }
            _proxyOrdersLmt[0] = entry;

            if (_poolSlotIndexLmt >= 0)
            {
                _photonSideband[_poolSlotIndexLmt].Account = acct;
                _photonSideband[_poolSlotIndexLmt].FleetEntryName = fleetEntryName;
                _photonSideband[_poolSlotIndexLmt].ExpectedKey = expectedKey;
                Thread.MemoryBarrier();
            }

            FleetDispatchSlot _slotLmt = new FleetDispatchSlot
            {
                EntryPrice = entry.LimitPrice > 0 ? entry.LimitPrice : 0,
                StopPrice = 0,
                SignalTicks = DateTime.UtcNow.Ticks,
                PoolSlotIndex = _poolSlotIndexLmt,
                OrderCount = 1,
                Quantity = followerQty,
                TargetCount = 0,
                Action = (int)action,
                ReservedDelta = reservedDelta,
            };
            _slotLmt.Shadow = ComputeFleetDispatchShadow(ref _slotLmt, _photonShadowSalt);

            // REAPER-EXPANSION Ticket 2: Circuit breaker check with atomic CAS loop
            if (
                !TryIncrementDispatchCountWithCircuitBreaker(
                    ref syncPending,
                    expectedKey,
                    ref reservedDelta,
                    _poolSlotIndexLmt,
                    fleetEntryName,
                    out bool circuitBreakerTrippedLmt
                )
            )
            {
                return; // Circuit breaker tripped, state already rolled back
            }

            // P2-3 FIX: Use centralized enqueue helper (matches market path pattern)
            EnqueueLimitEntryToPhotonRing(
                ref _slotLmt,
                _proxyOrdersLmt,
                _poolSlotIndexLmt,
                entry,
                acct,
                fleetEntryName,
                expectedKey,
                reservedDelta
            );
            syncPending = false;
            reservedDelta = 0;
            registeredForCleanup = false;

            dispatchLog.AppendLine(LogBuffer.Format("  QUEUE | {0,-28} | Limit        | PENDING", acct.Name));
        }

        /// <summary>
        /// P2-3: Circuit breaker gate for fleet dispatch with atomic CAS loop.
        /// Atomically increments _pendingFleetDispatchCount if below threshold.
        /// If threshold exceeded or breaker already tripped, rolls back state and returns false.
        /// CRITICAL: syncPending and reservedDelta are passed by ref to allow rollback mutations to propagate to caller.
        /// </summary>
        /// <param name="syncPending">Sync pending flag (passed by ref, may be reset to false on rollback).</param>
        /// <param name="expectedKey">Account key for position tracking.</param>
        /// <param name="reservedDelta">Reserved position delta (passed by ref, may be reset to 0 on rollback).</param>
        /// <param name="poolSlotIndex">Photon pool slot index for cleanup on rollback.</param>
        /// <param name="fleetEntryName">Fleet entry name for state cleanup on rollback.</param>
        /// <param name="circuitBreakerTripped">Output flag indicating if circuit breaker was tripped</param>
        /// <returns>True if dispatch allowed; false if circuit breaker tripped.</returns>
        /// <remarks>
        /// Callers MUST NOT reuse syncPending or reservedDelta values after this method returns false.
        /// All state mutations during rollback are atomic (lock-free).
        /// </remarks>
        private bool TryIncrementDispatchCountWithCircuitBreaker(
            ref bool syncPending,
            string expectedKey,
            ref int reservedDelta,
            int poolSlotIndex,
            string fleetEntryName,
            out bool circuitBreakerTripped
        )
        {
            circuitBreakerTripped = false;
            int currentCount;
            int newCount;
            do
            {
                currentCount = Volatile.Read(ref _pendingFleetDispatchCount);
                if (currentCount >= REAPER_MAX_PENDING_DISPATCHES)
                {
                    // Trip circuit breaker if not already tripped
                    if (Interlocked.CompareExchange(ref _reaperCircuitBreakerTripped, 1, 0) == 0)
                    {
                        Print(
                            LogBuffer.Format(
                                "[REAPER][CIRCUIT_BREAKER] TRIPPED: Queue depth={0} exceeds threshold={1} -- rejecting dispatch",
                                currentCount,
                                REAPER_MAX_PENDING_DISPATCHES
                            )
                        );
                    }
                    // Rollback state
                    RollbackCircuitBreakerState(
                        ref syncPending,
                        expectedKey,
                        ref reservedDelta,
                        poolSlotIndex,
                        fleetEntryName
                    );
                    circuitBreakerTripped = true;
                    return false;
                }
                // Circuit breaker already tripped - reject silently
                if (Volatile.Read(ref _reaperCircuitBreakerTripped) == 1)
                {
                    RollbackCircuitBreakerState(
                        ref syncPending,
                        expectedKey,
                        ref reservedDelta,
                        poolSlotIndex,
                        fleetEntryName
                    );
                    circuitBreakerTripped = true;
                    return false;
                }
                newCount = currentCount + 1;
            } while (
                Interlocked.CompareExchange(ref _pendingFleetDispatchCount, newCount, currentCount) != currentCount
            );

            return true; // Successfully incremented
        }

        /// <summary>
        /// P2-3: Rollback helper for circuit breaker state cleanup.
        /// Atomically resets all dispatch-related state when circuit breaker trips.
        /// </summary>
        /// <param name="syncPending">Sync pending flag (passed by ref, will be reset to false if true).</param>
        /// <param name="expectedKey">Account key for position tracking rollback.</param>
        /// <param name="reservedDelta">Reserved position delta (passed by ref, will be reset to 0 if non-zero).</param>
        /// <param name="poolSlotIndex">Photon pool slot index to release (if >= 0).</param>
        /// <param name="fleetEntryName">Fleet entry name for complete state cleanup (if not null).</param>
        /// <remarks>
        /// All operations are lock-free and atomic. This method is called when:
        /// 1. Circuit breaker threshold is exceeded
        /// 2. Circuit breaker is already tripped
        /// Ensures no partial state remains after a rejected dispatch.
        /// </remarks>
        private void RollbackCircuitBreakerState(
            ref bool syncPending,
            string expectedKey,
            ref int reservedDelta,
            int poolSlotIndex,
            string fleetEntryName
        )
        {
            // Unconditional state resets (P0 race condition fix)
            if (syncPending)
            {
                ClearDispatchSyncPending(expectedKey);
                syncPending = false;
            }

            if (reservedDelta != 0)
            {
                AddExpectedPositionDeltaLocked(expectedKey, -reservedDelta);
                reservedDelta = 0;
            }

            if (poolSlotIndex >= 0)
            {
                _photonPool.ReleaseByIndex(poolSlotIndex);
                _photonSideband[poolSlotIndex] = default(FleetDispatchSideband);
            }
            // P2-4 Fix: Complete state rollback
            if (fleetEntryName != null)
            {
                activePositions.TryRemove(fleetEntryName, out _);
                entryOrders.TryRemove(fleetEntryName, out _);
                stopOrders.TryRemove(fleetEntryName, out _);
                for (int tNum = 1; tNum <= 5; tNum++)
                {
                    var targetDict = GetTargetOrdersDictionary(tNum);
                    if (targetDict != null)
                        targetDict.TryRemove(fleetEntryName, out _);
                }
                _followerBrackets.TryRemove(fleetEntryName, out _);
            }
        }

        #endregion
    }
}
