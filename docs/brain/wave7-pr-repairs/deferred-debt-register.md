# Wave 7 Deferred Debt Register

**Purpose**: Out-of-scope pre-existing OKF violations discovered during Phase 7 PR repair
lanes. These are NOT regressions introduced by Wave 7 -- they were already present in main
before the wave started. They are captured here so the next wave or a dedicated debt-reduction
epic can address them without re-discovery triage.

**Format**: One row per finding. Append only -- never delete rows.
**Written by**: wave-orch-phase7-lane instances (STEP 5a) when they encounter a pre-existing
violation while verifying a fix in a file they were NOT permitted to touch (out-of-scope).

---

## Register

| ID | PR | File | Lines | Violation | OKF Rule | Discovered In | Priority |
|----|----|----|-------|-----------|----------|---------------|----------|
| DD-001 | PR-20 | src/V12_002.Trailing.StopUpdate.cs | 39, 96, 142, 316, 393 | DateTime.Now (pre-existing, not introduced by wave7) | Rule 3 -- FSM Determinism | verify-NEW-F1a.md | P1 |
| DD-002 | PR-21 | src/V12_002.UI.IPC.cs | 297-340 | SA1204: private static methods/fields (GlobalCommandsSet, IsGlobalCommand, IsMicroContractAlias, IsRoutingAlias, IsStrategyKeyword) appear after non-static methods at lines 144-296 -- pre-existing, not in PR-21 diff | Rule 6 -- Complexity / Naming | grep output during SA1204 sweep | P4 |
| DD-003 | PR-21 | src/V12_002.UI.Compliance.cs | 67 | SA1204: private static bool IsValidTradeExecution appears after non-static methods at lines 48-66 -- pre-existing | Rule 6 -- Complexity / Naming | grep output during SA1204 sweep | P4 |
| DD-004 | PR-21 | src/V12_002.IPC.Hardening.cs | 325, 339 | SA1204: private static readonly SqlInjectionPatterns and PathTraversalPatterns appear after instance methods at lines 174-318 -- pre-existing | Rule 6 -- Complexity / Naming | grep output during SA1204 sweep | P4 |
| DD-005 | PR-21 | src/V12_002.UI.IPC.Commands.Fleet.cs | 374 | Null dereference: CancelAll_IsOrderCancellable dereferences order.Instrument.FullName without null guard on Instrument -- Order.Instrument can be null in NinjaTrader (CodeRabbit + Cubic P2 finding) | Rule 5 -- Production Safety | CodeRabbit review run b5196128; Cubic violation #1 at line 372 | P3 |
| DD-006 | PR-21 | src/V12_002.UI.Compliance.cs | 590-595 | Missing StringComparison.Ordinal: IsTargetOrderPrefix uses StartsWith("T1_") etc. without explicit StringComparison -- culture-unsafe for fixed internal prefixes (Sourcery + Cubic P3) | Rule 6 -- Complexity / Naming | Sourcery comment; Cubic violation at line 590 | P4 |
| DD-007 | PR-21 | src/V12_002.UI.IPC.cs | 425 | Silent exception drop: empty catch around TriggerCustomEvent in ProcessIpcCommands silently discards queue-drain failures -- no error path visible (Cubic P2) | Rule 5 -- Production Safety | Cubic review ai_pr_review_1783026062317 violation #1 at line 425 | P2 |
| DD-008 | PR-22 | src/V12_002.SIMA.Lifecycle.cs | 229 | Account.All enumerated without .ToArray() snapshot in HydrateFleetAccountPositions -- broker-thread mutation causes InvalidOperationException | Rule 5 -- Production Safety (independent_tracking) | CR review 2026-07-02T23:21:53Z + grep output L3 | P2 |
| DD-009 | PR-22 | src/V12_002.SIMA.Lifecycle.cs | 655 | Account.All enumerated without .ToArray() snapshot in HydrateFromOpenPositions -- broker-thread mutation causes InvalidOperationException | Rule 5 -- Production Safety (independent_tracking) | CR review 2026-07-02T23:21:53Z + grep output L3 | P2 |
| DD-010 | PR-22 | src/V12_002.SIMA.Lifecycle.cs | 1469-1471 | SweepAccountOrders iterates acct.Orders.ToArray() but no null guard for ord before IsOrderInstrumentMatch(ord) -- null ord entry throws NullReferenceException, swallowed by catch{} silently aborting remaining cancels | Rule 5 -- Production Safety (defense in depth) | Cubic review 2026-07-02T23:24:12Z violation #2 | P3 |
| DD-011 | PR-22 | src/V12_002.SIMA.Flatten.cs | 476-481 | EmergencyFlattenCollectWorkingOrders active-state whitelist (Working/Submitted/Accepted/ChangePending/ChangeSubmitted) diverges from IsTerminalOrderState used by ProcessFlattenWorkItem_CancelOrders -- PartFilled/Initialized/TriggerPending/Unknown orders not collected, leaving unmanaged exposure live despite DEAD-01 kill-switch | Rule 5 -- Production Safety (defense in depth) | Cubic review 2026-07-02T21:12:43Z violation #3 | P3 |
| DD-012 | PR-22 | src/V12_002.SIMA.Lifecycle.cs | 120-128 | SA1503: single-line if bodies without braces in DrainPhotonRingOnShutdown (ringSlot.ReservedDelta check at 120, sbIdx bounds check at 127) | Rule 12 -- Naming/Style (SA1503) | CR review 2026-07-04T01:55:58Z inline comment | P4 |

---

## Priority Key

| Level | Meaning |
|-------|---------|
| P0 | lock() call -- hard ban, wave 8 must fix before merge |
| P1 | DateTime.Now in production logic -- determinism violation |
| P2 | Missing .ToArray() snapshot on enumerable collections |
| P3 | Null dereference without guard (p.Instrument, o.Name, etc.) |
| P4 | SA1503 braces, SA1204 ordering, comment inaccuracy |

---

## Usage for Future Waves

Before starting a wave that touches any file in this register:
1. Check this register for the file's entry.
2. Include the deferred violations in that epic's Phase 0 hotspot analysis.
3. Mark each row `resolved: wave_N commit_sha` when fixed.

This register is the authoritative source for known pre-existing debt found during bot reviews.
It supplements `complexity_audit.py` (which finds CYC > 8) with *qualitative* OKF rule violations
that static analysis tools do not catch (DateTime.Now, missing .ToArray(), missing null guards).
