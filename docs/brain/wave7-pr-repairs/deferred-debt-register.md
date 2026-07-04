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
