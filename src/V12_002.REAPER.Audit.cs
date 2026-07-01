// V12 REAPER Audit Module -- Fleet position audit, desync detection, and emergency flatten
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class V12_002 : Strategy
    {
        #region V12 REAPER Audit Logic

        private void AuditApexPositions()
        {
            bool shouldLog = (DateTime.UtcNow - lastReaperLog).TotalSeconds >= 30;
            int auditedCount = 0;
            int activeCount = 0;

            foreach (Account acct in Account.All)
            {
                if (IsFleetAccount(acct))
                {
                    auditedCount++;
                    if (AuditSingleFleetAccount(acct, shouldLog))
                    {
                        activeCount++;
                    }
                }
            }

            // V12.12: Explicitly audit the Master account if not covered by the prefix filter.
            bool masterAudited = IsFleetAccount(Account);
            if (!masterAudited)
            {
                auditedCount++;
                if (AuditMasterAccountIfNeeded(shouldLog))
                {
                    activeCount++;
                }
            }

            if (shouldLog)
            {
                if (activeCount == 0)
                {
                    Print($"[REAPER] Heartbeat: All {auditedCount} accounts flat.");
                }
                else
                {
                    Print($"[REAPER] Heartbeat: {activeCount}/{auditedCount} accounts with positions.");
                }
                lastReaperLog = DateTime.UtcNow;
            }

            AuditIpcCommandQueue(shouldLog);
            AuditIpcHardeningMetrics(shouldLog);
        }

        private void AuditIpcCommandQueue(bool shouldLog)
        {
            int queueDepth = GetPhotonDispatchRingDepth();
            int threshold = 1600; // 80% of 2000 capacity

            if (queueDepth >= threshold)
            {
                string msg = string.Format(
                    "[REAPER][IPC] Queue depth critical: {0}/{1} (threshold: {2})",
                    queueDepth,
                    2000,
                    threshold
                );
                Print(msg);

                // TODO: Trigger backpressure NACK (Epic 4 Ticket 03)
            }
            else if (shouldLog && queueDepth > 0)
            {
                Print(string.Format("[REAPER][IPC] Queue depth: {0}", queueDepth));
            }
        }

        /// <summary>
        /// EPIC-4 Ticket 03: Monitor IPC hardening metrics (rate limiter, circuit breakers).
        /// CYC: 4
        /// </summary>
        private void AuditIpcHardeningMetrics(bool shouldLog)
        {
            // Rate limiter status
            int nackCount = Volatile.Read(ref _ipcBackpressureNackCount);
            if (nackCount > 0 && shouldLog)
            {
                Print(string.Format("[REAPER][IPC] Backpressure NACKs: {0}", nackCount));
            }

            // Circuit breaker status - malformed payloads
            if (_ipcMalformedCircuitBreaker.IsOpen)
            {
                Print("[REAPER][IPC] Circuit breaker OPEN - malformed payload threshold exceeded");

                // Attempt reset if timeout elapsed
                if (_ipcMalformedCircuitBreaker.TryReset())
                {
                    Print("[REAPER][IPC] Circuit breaker RESET");
                }
            }

            // Allowlist bypass attempts
            if (_ipcAllowlistBypassDetector.IsOpen)
            {
                Print("[REAPER][IPC] SECURITY ALERT: Allowlist bypass attempts detected");
                // TODO: Trigger client disconnect (Phase 5)
            }
        }

        // Build 935 [REAPER-B935-003]: Per-account audit logic extracted from AuditApexPositions.
        // Returns true if the account has non-zero state (for heartbeat counter).
        // W7-082-T8: Refactored to final dispatcher (CYC=6). All complexity in extracted helpers.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AuditSingleFleetAccount(Account acct, bool shouldLog)
        {
            AuditFleet_CalculateExpectedActual(
                acct,
                shouldLog,
                out int actualQty,
                out int expectedQty,
                out string expectedKey,
                out bool syncPending,
                out bool inFillGrace,
                out bool hasState,
                out List<FollowerBracketFSM> accountFsms,
                out Position pos
            );

            if (expectedQty != actualQty)
                return AuditFleet_HandleDesyncBranch(
                    acct,
                    shouldLog,
                    expectedQty,
                    actualQty,
                    syncPending,
                    inFillGrace,
                    accountFsms,
                    hasState
                );

            AuditFleet_ProcessOrphanFsmLoop(accountFsms, acct.Name, actualQty);

            if (actualQty != 0)
                AuditFleet_HandleNakedPosition(acct, pos, actualQty, expectedKey, shouldLog);

            return hasState;
        }

        // W7-082-T2: Extract outer desync branch tree from AuditSingleFleetAccount. CYC=5.
        private bool AuditFleet_HandleDesyncBranch(
            Account acct,
            bool shouldLog,
            int expectedQty,
            int actualQty,
            bool syncPending,
            bool inFillGrace,
            List<FollowerBracketFSM> accountFsms,
            bool hasState
        )
        {
            if (actualQty == 0 && expectedQty != 0)
            {
                return AuditFleet_HandleDesyncRepair(
                    acct,
                    shouldLog,
                    expectedQty,
                    actualQty,
                    syncPending,
                    inFillGrace,
                    accountFsms,
                    hasState
                );
            }

            AuditFleet_EvaluateCriticalDesync(acct, shouldLog, expectedQty, actualQty, hasState);
            return hasState;
        }

        // W7-082-T3: Extract critical-desync evaluation and flatten dispatch. CYC=5.
        // Single responsibility: evaluate isCriticalDesync, route to grace check or minor log.
        private void AuditFleet_EvaluateCriticalDesync(
            Account acct,
            bool shouldLog,
            int expectedQty,
            int actualQty,
            bool hasState
        )
        {
            bool isCriticalDesync =
                (actualQty != 0 && expectedQty == 0)
                || (Math.Sign(actualQty) != Math.Sign(expectedQty) && expectedQty != 0);

            if (isCriticalDesync)
            {
                bool shouldDefer = AuditFleet_CheckPositionPassGrace(acct, shouldLog, actualQty, expectedQty);
                if (!shouldDefer)
                    AuditFleet_HandleCriticalDesyncFlatten(acct, shouldLog, expectedQty, actualQty);
            }
            else if (shouldLog)
            {
                AuditFleet_LogMinorDesync(acct.Name, expectedQty, actualQty);
            }
        }

        // W7-082-T4: Extract orphan FSM detection loop from AuditSingleFleetAccount. CYC=3.
        // Single concern: iterate accountFsms and call DetectOrphanFSM for each.
        private void AuditFleet_ProcessOrphanFsmLoop(
            List<FollowerBracketFSM> accountFsms,
            string acctName,
            int actualQty
        )
        {
            // [BUILD 981 DIAGNOSTIC]: Detect orphaned FSM positions after grace period.
            foreach (var fsm in accountFsms)
            {
                DetectOrphanFSM(fsm.EntryName, acctName, actualQty, activePositions);
            }
        }

        // W7-082-T5: Cold-path minor desync log [NoInlining] to keep off hot instruction cache. CYC=2.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AuditFleet_LogMinorDesync(string acctName, int expectedQty, int actualQty)
        {
            Print("[REAPER] Minor Desync on " + acctName + ": Expected=" + expectedQty + ", Actual=" + actualQty);
        }

        // Build 935 [REAPER-B935-003]: Extracted from AuditSingleFleetAccount -- Handle ghost position repair.
        // Ghost position = actual=0 but expected!=0 (follower failed to fill, or stop hit before fill).
        private bool AuditFleet_HandleDesyncRepair(
            Account acct,
            bool shouldLog,
            int expectedQty,
            int actualQty,
            bool syncPending,
            bool inFillGrace,
            List<FollowerBracketFSM> accountFsms,
            bool hasState
        )
        {
            // GHOST-FIX-3: Skip repair for Master -- it uses no FollowerBracketFSM -- repair path not applicable.
            if (acct.Name == Account.Name)
            {
                if (shouldLog)
                {
                    Print($"[REAPER] {acct.Name} is the Master account -- skipping follower repair.");
                }
                return hasState;
            }

            if (syncPending || inFillGrace)
            {
                if (shouldLog)
                {
                    string reason = syncPending ? "dispatch sync pending" : "fill grace active";
                    Print($"[REAPER] {acct.Name}: repair deferred ({reason}) while expected={expectedQty}, actual=0.");
                }
                return hasState;
            }

            string repairKey;
            if (EnqueueReaperRepairCandidate(acct, shouldLog, expectedQty, accountFsms, out repairKey))
            {
                // B957/E1: Clear in-flight guard if TriggerCustomEvent fails, preventing permanent lockout.
                try
                {
                    TriggerCustomEvent(o => ProcessReaperRepairQueue(), null);
                }
                catch (Exception repairTriggerEx)
                {
                    _repairInFlight.TryRemove(repairKey, out _); // [Build 968]
                    Print(
                        "[REAPER] TriggerCustomEvent failed for "
                            + repairKey
                            + ": "
                            + repairTriggerEx.Message
                            + " -- in-flight cleared."
                    );
                }
            }

            return hasState;
        }

        // Build 935 [REAPER-B935-004]: Extracted from AuditSingleFleetAccount -- Check Position Pass grace.
        // Position Pass grace = 10s window after reconnect where actualQty!=0 but expectedQty==0 (FSM not yet created).
        // Returns true if critical desync should be deferred (still in grace window).
        private bool AuditFleet_CheckPositionPassGrace(Account acct, bool shouldLog, int actualQty, int expectedQty)
        {
            // Build 999: Position Pass grace -- defer critical desync when account failed Phase 5 Position Pass.
            if (actualQty != 0 && expectedQty == 0)
            {
                DateTime ppFailedTime;
                if (_positionPassFailedFirstSeen.TryGetValue(acct.Name, out ppFailedTime))
                {
                    double graceElapsed = (DateTime.UtcNow - ppFailedTime).TotalSeconds;
                    if (graceElapsed < 10.0)
                    {
                        if (shouldLog)
                        {
                            Print(
                                string.Format(
                                    "[REAPER] {0}: Position Pass grace ({1:F1}s/10s) -- deferring critical desync. Stop replace in progress.",
                                    acct.Name,
                                    graceElapsed
                                )
                            );
                        }
                        return true;
                    }
                    _positionPassFailedFirstSeen.TryRemove(acct.Name, out _);
                    Print(
                        string.Format(
                            "[REAPER] {0}: Position Pass grace expired ({1:F1}s) -- firing critical desync.",
                            acct.Name,
                            graceElapsed
                        )
                    );
                }
            }
            return false;
        }

        // Build 935 [REAPER-B935-005]: Extracted from AuditSingleFleetAccount -- Handle critical desync flatten.
        private void AuditFleet_HandleCriticalDesyncFlatten(
            Account acct,
            bool shouldLog,
            int expectedQty,
            int actualQty
        )
        {
            if (shouldLog)
            {
                Print($"[REAPER] * CRITICAL DESYNC on {acct.Name}: Expected={expectedQty}, Actual={actualQty}");
            }
            if (AutoFlattenDesync)
            {
                if (shouldLog)
                {
                    Print($"[REAPER] * QUEUING FLATTEN for {acct.Name} - Emergency Re-sync!");
                }
                if (EnqueueReaperFlattenCandidate(acct))
                {
                    try
                    {
                        TriggerCustomEvent(o => ProcessReaperFlattenQueue(), null);
                    }
                    catch (Exception _flatTriggerEx)
                    {
                        _reaperFlattenInFlight.TryRemove(acct.Name + "_" + Instrument.FullName, out _);
                        Print(
                            "[REAPER] TriggerCustomEvent failed for flatten of "
                                + acct.Name
                                + ": "
                                + _flatTriggerEx.Message
                                + " -- in-flight cleared, will re-detect next cycle"
                        );
                    }
                }
            }
        }

        // Build 935 [REAPER-B935-006]: Extracted from AuditSingleFleetAccount -- Handle naked position audit.
        private void AuditFleet_HandleNakedPosition(
            Account acct,
            Position pos,
            int actualQty,
            string expectedKey,
            bool shouldLog
        )
        {
            bool hasWorkingStop = AuditFleet_CheckWorkingStop(acct);

            if (!hasWorkingStop)
            {
                if (
                    DetectNakedPosition(
                        acct,
                        pos,
                        actualQty,
                        expectedKey,
                        shouldLog,
                        pendingStopReplacements,
                        activePositions
                    )
                )
                {
                    try
                    {
                        TriggerCustomEvent(e => ProcessReaperNakedStopQueue(), null);
                    }
                    catch (Exception tcEx)
                    {
                        ClearNakedStopInFlight(expectedKey);
                        Print(
                            string.Format(
                                "[REAPER][NAKED_STOP] TriggerCustomEvent failed for {0}: {1} -- in-flight cleared.",
                                acct.Name,
                                tcEx.Message
                            )
                        );
                    }
                }
            }
            else
            {
                ClearNakedPositionGrace(acct.Name);
            }
        }

        // W7-084-T6: Refactored parent AuditFleet_CalculateExpectedActual to orchestrate helpers. CYC=6.
        private void AuditFleet_CalculateExpectedActual(
            Account acct,
            bool shouldLog,
            out int actualQty,
            out int expectedQty,
            out string expectedKey,
            out bool syncPending,
            out bool inFillGrace,
            out bool hasState,
            out List<FollowerBracketFSM> accountFsms,
            out Position pos
        )
        {
            AuditFleet_ResolvePosition(acct, out actualQty, out pos);
            AuditFleet_CollectFsmState(acct, out accountFsms, out int fsmExpectedQty);
            AuditFleet_ReconcileStaleFsms(accountFsms, acct.Name, actualQty, ref fsmExpectedQty);
            AuditFleet_ClearPositionPassState(acct.Name, fsmExpectedQty);
            AuditFleet_AssembleOutputs(
                acct.Name,
                actualQty,
                fsmExpectedQty,
                out expectedKey,
                out expectedQty,
                out syncPending,
                out inFillGrace,
                out hasState
            );
            if (shouldLog && hasState)
            {
                Print($"[REAPER] {acct.Name}: Expected={expectedQty}, Actual={actualQty}");
            }
        }

        // W7-084-T1: Resolve broker position quantity and position reference. CYC=3.
        private void AuditFleet_ResolvePosition(Account acct, out int actualQty, out Position pos)
        {
            pos = acct.Positions.FirstOrDefault(p => p.Instrument.FullName == Instrument.FullName);
            actualQty = 0;
            if (pos != null && pos.MarketPosition != MarketPosition.Flat)
            {
                actualQty = pos.MarketPosition == MarketPosition.Long ? pos.Quantity : -pos.Quantity;
            }
        }

        // W7-084-T2: Collect FSM list and expected quantity from FSM authority. CYC=2.
        private void AuditFleet_CollectFsmState(
            Account acct,
            out List<FollowerBracketFSM> accountFsms,
            out int fsmExpectedQty
        )
        {
            // Build 1105: FSM is the SOLE authority for follower expected position.
            accountFsms = _followerBrackets.Values.Where(f => f.AccountName == acct.Name).ToList();
            fsmExpectedQty = GetFsmExpectedPosition(acct.Name);
        }

        // W7-084-T3: Reconcile stale/orphaned FSMs (cold error-recovery path). CYC=4.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AuditFleet_ReconcileStaleFsms(
            List<FollowerBracketFSM> accountFsms,
            string accountName,
            int actualQty,
            ref int fsmExpectedQty
        )
        {
            foreach (var f in accountFsms)
            {
                if (f.State == FollowerBracketState.Active && f.EntryOrder == null)
                {
                    if (actualQty != 0)
                    {
                        fsmExpectedQty += actualQty;
                    }
                    else
                    {
                        FollowerBracketFSM staleFsm;
                        if (TryTerminateFollowerBracket(f.EntryName, out staleFsm))
                        {
                            Print(
                                string.Format(
                                    "[REAPER-C7] Stale Active FSM for {0} on {1} (broker flat) -- auto-terminating",
                                    f.EntryName,
                                    accountName
                                )
                            );
                        }
                    }
                }
            }
        }

        // W7-084-T4: Clear position-pass state when FSM has recovered to non-zero expected. CYC=2.
        private void AuditFleet_ClearPositionPassState(string accountName, int fsmExpectedQty)
        {
            if (fsmExpectedQty != 0)
            {
                _positionPassFailedFirstSeen.TryRemove(accountName, out _);
            }
        }

        // W7-084-T5: Assemble all output parameters for callers of AuditFleet_CalculateExpectedActual. CYC=3.
        private void AuditFleet_AssembleOutputs(
            string accountName,
            int actualQty,
            int fsmExpectedQty,
            out string expectedKey,
            out int expectedQty,
            out bool syncPending,
            out bool inFillGrace,
            out bool hasState
        )
        {
            // AUTHORITY: Use FSM state from now on
            expectedKey = ExpKey(accountName);
            expectedQty = fsmExpectedQty;
            syncPending = _dispatchSyncPendingExpKeys.ContainsKey(expectedKey); // [B967-FIX-02]
            inFillGrace = IsReaperFillGraceActive(expectedKey);
            hasState = expectedQty != 0 || actualQty != 0;
        }

        private bool EnqueueReaperRepairCandidate(
            Account acct,
            bool shouldLog,
            int expectedQty,
            List<FollowerBracketFSM> accountFsms,
            out string repairKey
        )
        {
            if (_isTerminating)
            {
                repairKey = null;
                return false;
            }
            repairKey = acct.Name + "_" + Instrument.FullName;
            if (!_repairInFlight.TryAdd(repairKey, 0))
            {
                if (shouldLog)
                {
                    Print($"[REAPER] {acct.Name} repair already in-flight -- skipping.");
                }
                return false;
            }

            bool hasWorkingEntry = accountFsms.Any(f =>
                f.State == FollowerBracketState.Submitted || f.State == FollowerBracketState.Accepted
            );

            if (!hasWorkingEntry)
            {
                if (shouldLog)
                {
                    Print(
                        $"[REAPER] * REPAIR CANDIDATE: {acct.Name} is Flat, expected={expectedQty}. Enqueuing repair."
                    );
                }
                _reaperRepairQueue.Enqueue(acct.Name);
                return true;
            }

            _repairInFlight.TryRemove(repairKey, out _);
            return false;
        }

        private bool EnqueueReaperFlattenCandidate(Account acct)
        {
            if (_isTerminating)
                return false;
            string flattenKey = acct.Name + "_" + Instrument.FullName;
            if (!_reaperFlattenInFlight.TryAdd(flattenKey, 0))
            {
                return false;
            }
            _reaperFlattenQueue.Enqueue(acct.Name);
            return true;
        }

        // W7-087-T1: Named stop-order predicate extracted from AuditFleet_CheckWorkingStop. CYC=5.
        private bool IsWorkingStopOrderForInstrument(Order o)
        {
            return o.Instrument?.FullName == Instrument?.FullName
                && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
                && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
                && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover);
        }

        // W7-141-T1: Working stop order predicate for instrument. CYC=7.
        private bool IsWorkingStopOrder(Order o)
        {
            return o.Instrument?.FullName == Instrument?.FullName
                && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
                && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
                && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover);
        }

        // W7-087 + W7-141 refactored parent: AuditFleet_CheckWorkingStop. CYC=1.
        private bool AuditFleet_CheckWorkingStop(Account acct)
        {
            // Build 1108.003 [D3]: Snapshot broker orders before iteration. orderSnapshot
            var orders = acct.Orders.ToArray();
            return orders.Any(IsWorkingStopOrderForInstrument);
        }

        // Build 1111.007-reaper-t1: EnqueueReaperNakedStopCandidate extracted to V12_002.REAPER.NakedPosition.cs as DetectNakedPosition

        private void TerminateFsmsForAccount(string accountName)
        {
            foreach (var kvp in _followerBrackets.ToArray())
            {
                FollowerBracketFSM fsm = kvp.Value;
                if (fsm == null || fsm.AccountName != accountName)
                {
                    continue;
                }

                FollowerBracketFSM removedFsm;
                if (TryTerminateFollowerBracket(kvp.Key, out removedFsm))
                {
                    Print(string.Format("[FSM-C3] Terminated FSM {0} for {1} (flatten)", kvp.Key, accountName));
                }
            }
        }

        // Build 935 [REAPER-B935-007]: Extracted from AuditMasterAccountIfNeeded -- Calculate master position state.
        private void AuditMaster_CalculatePositionState(
            bool shouldLog,
            out Position masterPos,
            out int masterActualQty,
            out int masterExpectedQty,
            out string masterExpectedKey,
            out bool hasState
        )
        {
            masterPos = Account.Positions.FirstOrDefault(p => p.Instrument.FullName == Instrument.FullName);
            masterActualQty = 0;
            if (masterPos != null && masterPos.MarketPosition != MarketPosition.Flat)
            {
                masterActualQty =
                    masterPos.MarketPosition == MarketPosition.Long ? masterPos.Quantity : -masterPos.Quantity;
            }

            masterExpectedQty = 0;
            masterExpectedKey = ExpKey(Account.Name);
            expectedPositions.TryGetValue(masterExpectedKey, out masterExpectedQty);

            hasState = masterExpectedQty != 0 || masterActualQty != 0;
            if (shouldLog && hasState)
            {
                Print($"[REAPER] {Account.Name} (Master): Expected={masterExpectedQty}, Actual={masterActualQty}");
            }
        }

        // Build 935 [REAPER-B935-008]: Handle desync and flatten.
        // W7-085-T3: Refactored to delegate to helpers (CYC=5).
        private void AuditMaster_HandleDesyncFlatten(bool shouldLog, int masterActualQty, int masterExpectedQty)
        {
            if (masterExpectedQty != masterActualQty)
            {
                if (masterActualQty == 0 && masterExpectedQty != 0)
                {
                    AuditMaster_HandleGhostFlatLog(shouldLog, masterActualQty, masterExpectedQty);
                }
                else if (AuditMaster_CheckExpectedActual(shouldLog, masterActualQty, masterExpectedQty))
                {
                    if (shouldLog)
                    {
                        Print($"[REAPER] QUEUING FLATTEN for {Account.Name} (Master) - Emergency Re-sync!");
                    }
                    if (EnqueueReaperMasterFlatten())
                    {
                        string flattenKey = Account.Name + "_" + Instrument.FullName;
                        AuditMaster_TriggerFlattenEvent(flattenKey);
                    }
                }
            }
        }

        // W7-085-T1: Safely dispatch flatten event to Actor queue. CYC=3.
        private void AuditMaster_TriggerFlattenEvent(string flattenKey)
        {
            try
            {
                TriggerCustomEvent(o => ProcessReaperFlattenQueue(), null);
            }
            catch (Exception _mFlatTriggerEx)
            {
                _reaperFlattenInFlight.TryRemove(flattenKey, out _);
                Print(
                    "[REAPER] TriggerCustomEvent failed for master flatten: "
                        + _mFlatTriggerEx.Message
                        + " -- in-flight cleared, will re-detect next cycle"
                );
            }
        }

        // W7-085-T2: Cold-path ghost-flat detection and logging [NoInlining]. CYC=2.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AuditMaster_HandleGhostFlatLog(bool shouldLog, int masterActualQty, int masterExpectedQty)
        {
            if (masterActualQty == 0 && masterExpectedQty != 0)
            {
                if (shouldLog)
                {
                    Print(
                        $"[REAPER] {Account.Name} (Master) is Flat (Target/Stop hit). Expected was {masterExpectedQty}."
                    );
                }
            }
        }

        // Build 935 [REAPER-B935-009]: Handle naked position detection.
        // W7-081-T4: Refactored to call extracted helpers (CYC=3).
        private void AuditMaster_HandleNakedPosition(Position masterPos, int masterActualQty, string masterExpectedKey)
        {
            if (masterActualQty != 0)
            {
                if (!AuditMaster_HasWorkingStopOrder())
                {
                    DateTime masterFirstSeen;
                    if (!_nakedPositionFirstSeen.TryGetValue(Account.Name, out masterFirstSeen))
                    {
                        int graceSeconds = (NakedPositionGraceSec >= 5) ? NakedPositionGraceSec : 5;
                        AuditMaster_StartNakedGraceWindow(masterActualQty, graceSeconds);
                    }
                    else
                    {
                        AuditMaster_TriggerNakedStopIfGraceExpired(
                            masterPos,
                            masterActualQty,
                            masterExpectedKey,
                            masterFirstSeen
                        );
                    }
                }
                else
                {
                    _nakedPositionFirstSeen.TryRemove(Account.Name, out _);
                }
            }
        }

        // W7-031-T1: Stop-order detection predicate (parameterized, for use when orders array is pre-snapshotted). CYC=6.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool AuditMaster_HasWorkingStopOrder(Order[] masterOrders)
        {
            return masterOrders.Any(o =>
                o.Instrument?.FullName == Instrument?.FullName
                && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
                && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
                && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover)
            );
        }

        // W7-081-T1: Hot-path stop-order predicate (parameterless) [AggressiveInlining]. CYC=1.
        // H13-FIX: Snapshot to prevent InvalidOperationException from UI thread updates.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AuditMaster_HasWorkingStopOrder()
        {
            var masterOrders = Account.Orders.ToArray();
            return masterOrders.Any(o =>
                o.Instrument?.FullName == Instrument?.FullName
                && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
                && (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
                && (o.OrderAction == OrderAction.Sell || o.OrderAction == OrderAction.BuyToCover)
            );
        }

        // W7-031-T2: Grace-window initialization (cold path) [NoInlining]. CYC=1.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AuditMaster_InitNakedPositionGrace(int masterActualQty, int graceSeconds)
        {
            _nakedPositionFirstSeen[Account.Name] = DateTime.UtcNow;
            Print(
                string.Format(
                    "[REAPER][NAKED_POSITION] {0} (Master): {1}ct naked -- starting {2}s grace window.",
                    Account.Name,
                    masterActualQty,
                    graceSeconds
                )
            );
        }

        // W7-081-T2: Cold-path grace-window initialiser [NoInlining]. CYC=1.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AuditMaster_StartNakedGraceWindow(int masterActualQty, int graceSeconds)
        {
            _nakedPositionFirstSeen[Account.Name] = DateTime.UtcNow;
            Print(
                string.Format(
                    "[REAPER][NAKED_POSITION] {0} (Master): {1}ct naked -- starting {2}s grace window.",
                    Account.Name,
                    masterActualQty,
                    graceSeconds
                )
            );
        }

        // W7-031-T3: Naked stop dispatch (cold path) [NoInlining]. CYC=4.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AuditMaster_DispatchNakedStop(
            Position masterPos,
            int masterActualQty,
            string masterExpectedKey,
            DateTime masterFirstSeen
        )
        {
            if (EnqueueReaperMasterNakedStop(masterPos, masterActualQty, masterExpectedKey, masterFirstSeen))
            {
                try
                {
                    TriggerCustomEvent(e => ProcessReaperNakedStopQueue(), null);
                }
                catch (Exception tcEx)
                {
                    _reaperNakedStopInFlight.TryRemove(masterExpectedKey, out _);
                    Print(
                        string.Format(
                            "[REAPER][NAKED_STOP] TriggerCustomEvent failed for {0} (Master): {1} -- in-flight cleared.",
                            Account.Name,
                            tcEx.Message
                        )
                    );
                }
            }
        }

        // W7-081-T3: Cold-path emergency stop trigger [NoInlining]. CYC=3. Max helper for W7-081.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AuditMaster_TriggerNakedStopIfGraceExpired(
            Position masterPos,
            int masterActualQty,
            string masterExpectedKey,
            DateTime masterFirstSeen
        )
        {
            int graceSeconds = (NakedPositionGraceSec >= 5) ? NakedPositionGraceSec : 5;
            if (EnqueueReaperMasterNakedStop(masterPos, masterActualQty, masterExpectedKey, masterFirstSeen))
            {
                try
                {
                    TriggerCustomEvent(e => ProcessReaperNakedStopQueue(), null);
                }
                catch (Exception tcEx)
                {
                    _reaperNakedStopInFlight.TryRemove(masterExpectedKey, out _);
                    Print(
                        string.Format(
                            "[REAPER][NAKED_STOP] TriggerCustomEvent failed for {0} (Master): {1} -- in-flight cleared.",
                            Account.Name,
                            tcEx.Message
                        )
                    );
                }
            }
        }

        // Build 935 [REAPER-B935-004]: Audit the Master account when it isn't covered by AccountPrefix.
        private bool AuditMasterAccountIfNeeded(bool shouldLog)
        {
            Position masterPos;
            int masterActualQty;
            int masterExpectedQty;
            string masterExpectedKey;
            bool hasState;

            AuditMaster_CalculatePositionState(
                shouldLog,
                out masterPos,
                out masterActualQty,
                out masterExpectedQty,
                out masterExpectedKey,
                out hasState
            );
            AuditMaster_HandleDesyncFlatten(shouldLog, masterActualQty, masterExpectedQty);
            AuditMaster_HandleNakedPosition(masterPos, masterActualQty, masterExpectedKey);

            return hasState;
        }

        // W7-083-T4: Refactored parent AuditMaster_CheckExpectedActual to delegate to helpers. CYC=4.
        private bool AuditMaster_CheckExpectedActual(bool shouldLog, int masterActualQty, int masterExpectedQty)
        {
            bool inFillGrace = AuditMaster_IsInFillGrace();
            bool isCriticalDesync = !inFillGrace && AuditMaster_IsCriticalDesync(masterActualQty, masterExpectedQty);
            if (shouldLog)
            {
                AuditMaster_LogDesyncState(isCriticalDesync, inFillGrace, masterExpectedQty, masterActualQty);
            }
            if (isCriticalDesync && AutoFlattenDesync)
            {
                return true;
            }
            return false;
        }

        // W7-083-T1: Hot-path fill grace predicate [AggressiveInlining]. CYC=2.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AuditMaster_IsInFillGrace()
        {
            long stampTicks = Interlocked.Read(ref _lastExpectedPositionSetTicks);
            return stampTicks > 0 && (DateTime.UtcNow.Ticks - stampTicks) < ReaperFillGraceTicks;
        }

        // W7-083-T2: Hot-path critical desync predicate [AggressiveInlining]. CYC=3.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool AuditMaster_IsCriticalDesync(int masterActualQty, int masterExpectedQty)
        {
            return (masterActualQty != 0 && masterExpectedQty == 0)
                || (Math.Sign(masterActualQty) != Math.Sign(masterExpectedQty) && masterExpectedQty != 0);
        }

        // W7-083-T3: Cold-path desync logging sink [NoInlining]. CYC=3.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AuditMaster_LogDesyncState(
            bool isCriticalDesync,
            bool inFillGrace,
            int masterExpectedQty,
            int masterActualQty
        )
        {
            if (inFillGrace)
            {
                Print($"[REAPER] {Account.Name} (Master): Fill grace active -- desync check suppressed.");
                return;
            }
            if (isCriticalDesync)
            {
                Print(
                    $"[REAPER] CRITICAL DESYNC on {Account.Name} (Master): Expected={masterExpectedQty}, Actual={masterActualQty}"
                );
                return;
            }
            Print(
                $"[REAPER] Minor Desync on {Account.Name} (Master): Expected={masterExpectedQty}, Actual={masterActualQty}"
            );
        }

        private bool EnqueueReaperMasterFlatten()
        {
            if (_isTerminating)
                return false;
            string flattenKey = Account.Name + "_" + Instrument.FullName;
            if (!_reaperFlattenInFlight.TryAdd(flattenKey, 0))
            {
                return false;
            }
            _reaperFlattenQueue.Enqueue(Account.Name);
            return true;
        }

        private bool EnqueueReaperMasterNakedStop(
            Position masterPos,
            int masterActualQty,
            string masterExpectedKey,
            DateTime masterFirstSeen
        )
        {
            if (_isTerminating)
                return false;
            if (
                (DateTime.UtcNow - masterFirstSeen).TotalSeconds
                >= ((NakedPositionGraceSec >= 5) ? NakedPositionGraceSec : 5)
            )
            {
                if (!_reaperNakedStopInFlight.TryAdd(masterExpectedKey, 0))
                {
                    return false;
                }
                Print(
                    string.Format(
                        "[REAPER][NAKED_POSITION] {0} (Master): {1}ct CONFIRMED naked after {2:F1}s grace. Queuing emergency hard stop.",
                        Account.Name,
                        masterActualQty,
                        (DateTime.UtcNow - masterFirstSeen).TotalSeconds
                    )
                );
                _reaperNakedStopQueue.Enqueue((Account.Name, masterPos.MarketPosition, Math.Abs(masterActualQty)));
                return true;
            }

            return false;
        }

        /// <summary>
        /// V12.17 FIX: Processes queued flatten requests on the strategy thread.
        /// </summary>
        private void ProcessReaperFlattenQueue()
        {
            string accountName;
            while (_reaperFlattenQueue.TryDequeue(out accountName))
            {
                try
                {
                    Account targetAcct = ProcessReaperFlatten_FindAccount(accountName);

                    if (targetAcct != null)
                    {
                        ProcessReaperFlatten_CancelWorkingOrders(targetAcct, accountName);
                        ProcessReaperFlatten_ClosePositions(targetAcct, accountName);
                        ProcessReaperFlatten_TerminateFsms(accountName);
                        Print($"[REAPER] ? MARSHAL-FLATTEN (Unmanaged) executed on strategy thread for {accountName}");
                    }
                    else
                    {
                        Print($"[REAPER] [X] Could not find account '{accountName}' for marshal-flatten");
                    }
                }
                catch (Exception ex)
                {
                    Print($"[REAPER] [X] MARSHAL-FLATTEN FAILED for {accountName}: {ex.Message}");
                }
                finally
                {
                    _reaperFlattenInFlight.TryRemove(accountName + "_" + Instrument.FullName, out _);
                }
            }
        }

        private Account ProcessReaperFlatten_FindAccount(string accountName)
        {
            Account targetAcct = null;
            foreach (Account acct in Account.All)
            {
                if (acct.Name == accountName)
                {
                    targetAcct = acct;
                    break;
                }
            }

            if (targetAcct == null && Account.Name == accountName)
                targetAcct = Account;

            return targetAcct;
        }

        // W7-086: Refactored parent ProcessReaperFlatten_CancelWorkingOrders. CYC=2.
        private void ProcessReaperFlatten_CancelWorkingOrders(Account targetAcct, string accountName)
        {
            var ordersToCancel = BuildCancelOrderList(targetAcct);
            ExecuteCancelOrders(ordersToCancel, targetAcct, accountName);
        }

        // W7-086-T1: Null guard + instrument check + 4-branch OrderState predicate. CYC=6.
        private bool IsOrderCancellable(Order order)
        {
            if (order == null)
                return false;
            if (order.Instrument.FullName != Instrument.FullName)
                return false;
            return order.OrderState == OrderState.Working
                || order.OrderState == OrderState.Submitted
                || order.OrderState == OrderState.Accepted
                || order.OrderState == OrderState.ChangePending;
        }

        // W7-086-T2: Snapshot + iterate + filter into staging list. CYC=3.
        private List<Order> BuildCancelOrderList(Account targetAcct)
        {
            // H14-FIX: Snapshot broker orders before iteration to prevent collection-modified exception.
            var snapshot = targetAcct.Orders.ToArray();
            var ordersToCancel = new List<Order>();
            foreach (var order in snapshot)
            {
                if (IsOrderCancellable(order))
                    ordersToCancel.Add(order);
            }
            return ordersToCancel;
        }

        // W7-086-T3: Count guard + dispatch loop + diagnostic print. CYC=4.
        private void ExecuteCancelOrders(List<Order> ordersToCancel, Account targetAcct, string accountName)
        {
            if (ordersToCancel.Count > 0)
            {
                foreach (var order in ordersToCancel)
                    CancelOrderOnAccount(order, targetAcct);
                Print("[REAPER] Emergency Cancel: " + ordersToCancel.Count + " orders on " + accountName);
            }
        }

        private void ProcessReaperFlatten_ClosePositions(Account targetAcct, string accountName)
        {
            var accountPositions = targetAcct.Positions.ToArray();
            foreach (Position position in accountPositions)
            {
                if (
                    position.Instrument.FullName != Instrument.FullName
                    || position.MarketPosition == MarketPosition.Flat
                )
                {
                    continue;
                }

                int qty = position.Quantity;
                string signalName = "ReaperFlatten_" + position.MarketPosition.ToString();

                if (targetAcct == this.Account)
                {
                    if (position.MarketPosition == MarketPosition.Long)
                    {
                        SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, qty, 0, 0, "", signalName);
                    }
                    else
                    {
                        SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, qty, 0, 0, "", signalName);
                    }
                }
                else
                {
                    OrderAction closeAction =
                        position.MarketPosition == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;
                    Order closeOrder = targetAcct.CreateOrder(
                        Instrument,
                        closeAction,
                        OrderType.Market,
                        TimeInForce.Gtc,
                        qty,
                        0,
                        0,
                        "",
                        signalName,
                        null
                    );
                    targetAcct.Submit(new[] { closeOrder });
                }
                Print($"[REAPER] ? Emergency Market Close: {qty} contracts on {accountName}");
            }
        }

        private void ProcessReaperFlatten_TerminateFsms(string accountName)
        {
            TerminateFsmsForAccount(accountName);
        }

        #endregion
    }
}
