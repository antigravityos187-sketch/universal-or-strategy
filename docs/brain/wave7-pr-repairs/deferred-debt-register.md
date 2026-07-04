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
| DD-002 | PR-20 | src/V12_002.Orders.Management.StopSync.cs | 968 | DateTime.Now in suffix generation (production stop-order naming) | Rule 3 -- FSM Determinism | triage NEW-F1a sweep | P1 |
| DD-003 | PR-20 | src/V12_002.Trailing.cs | 215 | DateTime.Now in ManageTrail_AdaptiveThrottleTick (hot-path tick handler) | Rule 3 -- FSM Determinism | triage NEW-F1a sweep | P1 |
| DD-004 | PR-20 | src/V12_002.Orders.Callbacks.AccountOrders.cs | 98, 135, 177, 253, 265, 605, 697, 740, 770 | Pre-existing underscore method names (ProcessAccountOrder_, TryFindOrder_, HandleMatchedFollower_ group not in NEW-F4 scope) | Rule 12 -- Naming Conventions | NEW-F4 planner scope check | P4 |
| DD-005 | PR-20 | src/V12_002.Orders.Callbacks.cs | 209, 224, 236, 468, 564, 605, 752, 791, 818 | Underscore method names throughout (HandleOrderState_, HandleSecondaryOrderFilled_, HandleOrderCancelled_) | Rule 12 -- Naming Conventions | NEW-F4 sweep of PR-diff files | P4 |
| DD-006 | PR-20 | src/V12_002.Orders.Management.StopSync.cs | 64, 108, 118, 214, 251, 345, 398, 428, 499, 598, 670, 733, 754, 783 | Underscore method names throughout (RefreshActivePositionOrders_, SyncLimitTarget_, UpdateStopQuantity_, CreateNewStopOrder_) | Rule 12 -- Naming Conventions | NEW-F4 sweep of PR-diff files | P4 |
| DD-007 | PR-20 | src/V12_002.Orders.Management.StopSync.cs | 915, 965 | Underscore-prefixed local variable _b950OcoId (should be b950OcoId) | Rule 12 -- Naming Conventions | pre-existing underscore local scan | P4 |
| DD-008 | PR-20 | src/V12_002.Trailing.cs | 42, 68, 85, 106, 126, 149, 195, 200, 205, 212, 259, 279, 334, 364, 420, 455, 462, 513, 533, 576, 596, 623 | Underscore method names + _shouldExit local (ManageTrail_, FleetSync_, TrailHandler_) | Rule 12 -- Naming Conventions | NEW-F4 sweep of PR-diff files | P4 |

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
