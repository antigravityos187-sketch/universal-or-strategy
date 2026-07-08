# EPIC-W7-084 — Phase 4: Ticket Generation

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:07:00Z
**Input:** docs/brain/EPIC-W7-084/02-architecture-plan.md, docs/brain/EPIC-W7-084/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Method** | `AuditFleet_CalculateExpectedActual` |
| **File** | `src/V12_002.REAPER.Audit.cs` |
| **CYC Baseline** | 382 |
| **CYC Target** | <= 8 |
| **max_cyc_projected** | 6 |
| **extraction_count** | 5 |
| **ticket_count** | 9 |
| **dna_verdict** | PASS |
| **Lane** | P4-L5 |

---

## Tickets

---

### W7-084-T1: Extract `AuditFleet_ResolvePosition`

**Type:** extraction
**Priority:** P1
**CYC Impact:** N/A → 3 (new helper)

#### Description

Extract the broker position resolution logic from `AuditFleet_CalculateExpectedActual` into a
new private same-file helper method `AuditFleet_ResolvePosition`. This helper is responsible
for resolving the net position quantity from the account's position list via FSM state and
setting both the `actualQty` and `pos` out-parameters.

**Target signature:**
```csharp
private void AuditFleet_ResolvePosition(
    Account acct,
    out int actualQty,
    out Position pos
)
```

**Extracted logic:**
- `pos = acct.Positions.FirstOrDefault(p => p.Instrument == ...)`
- `actualQty = 0;`
- `if (pos != null && pos.MarketPosition != MarketPosition.Flat)` then set `actualQty` as signed qty

**Location:** `src/V12_002.REAPER.Audit.cs` (same file, private method)

#### Acceptance Criteria

- [ ] New private method `AuditFleet_ResolvePosition` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] Method signature matches plan exactly (3 params: `Account acct`, `out int actualQty`, `out Position pos`)
- [ ] Method contains zero `lock()` blocks (lock-free mandate)
- [ ] `pos` and `actualQty` are correctly assigned in all branches (non-null and flat cases covered)
- [ ] CYC of extracted helper = 3
- [ ] No change to any caller outside `AuditFleet_CalculateExpectedActual`

**CYC Impact:** extraction cyc=3 (new helper); parent CYC reduced by ~76 branch paths

---

### W7-084-T2: Extract `AuditFleet_CollectFsmState`

**Type:** extraction
**Priority:** P1
**CYC Impact:** N/A → 2 (new helper)

#### Description

Extract the FSM collection and expected-quantity resolution into a new private helper
`AuditFleet_CollectFsmState`. This helper populates the FSM list for the given account
and queries the FSM authority for the expected position quantity via `GetFsmExpectedPosition`.

**Target signature:**
```csharp
private void AuditFleet_CollectFsmState(
    Account acct,
    out List<FollowerBracketFSM> accountFsms,
    out int fsmExpectedQty
)
```

**Extracted logic:**
- `accountFsms = _followerBrackets.Values.Where(f => f.AccountName == acct.Name).ToList()`
- `fsmExpectedQty = GetFsmExpectedPosition(acct.Name)`

**Location:** `src/V12_002.REAPER.Audit.cs` (same file, private method)

#### Acceptance Criteria

- [ ] New private method `AuditFleet_CollectFsmState` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] Method signature matches plan exactly (3 params)
- [ ] `accountFsms` is populated from `_followerBrackets.Values` filtered by account name
- [ ] `fsmExpectedQty` is set via `GetFsmExpectedPosition(acct.Name)`
- [ ] Method contains zero `lock()` blocks
- [ ] CYC of extracted helper = 2

**CYC Impact:** extraction cyc=2 (new helper); contributes to parent CYC reduction (382→6)

---

### W7-084-T3: Extract `AuditFleet_ReconcileStaleFsms` [NoInlining]

**Type:** extraction
**Priority:** P1
**CYC Impact:** N/A → 4 (new helper, max complexity helper in this epic)

#### Description

Extract the stale/orphaned FSM reconciliation logic into a new private helper
`AuditFleet_ReconcileStaleFsms`. This method handles the cold error-recovery path:
Active FSMs that have no `EntryOrder` reference (can occur after a strategy restart).
Per Jane Street `carl_cook` KB rule, cold error-recovery paths must be decorated with
`[MethodImpl(MethodImplOptions.NoInlining)]` to prevent JIT inlining and maintain
independent stacktraceability.

**Target signature:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void AuditFleet_ReconcileStaleFsms(
    List<FollowerBracketFSM> accountFsms,
    string accountName,
    int actualQty,
    ref int fsmExpectedQty
)
```

**Extracted logic:**
- `foreach (var f in accountFsms)` iterates all account FSMs
- `if (f.State == FsmState.Active && f.EntryOrder == null)` detects stale FSMs
- Branch `if (actualQty != 0)`: `fsmExpectedQty += actualQty` (hydrated recovery)
- Branch `else`: `TryTerminateFollowerBracket(...)` + `Print(...)` (stale cleanup)

**`ref int fsmExpectedQty`:** mutation semantics for adjusting expected quantity without allocation.

**Location:** `src/V12_002.REAPER.Audit.cs` (same file, private method)

#### Acceptance Criteria

- [ ] New private method `AuditFleet_ReconcileStaleFsms` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] `[MethodImpl(MethodImplOptions.NoInlining)]` attribute is present on the method
- [ ] Method uses `ref int fsmExpectedQty` parameter (not `out`) to allow mutation
- [ ] All 3 branches handled: normal FSM, hydrated stale, orphaned stale
- [ ] Method contains zero `lock()` blocks
- [ ] CYC of extracted helper = 4 (max helper in this epic)
- [ ] `using System.Runtime.CompilerServices;` present in file if not already

**CYC Impact:** extraction cyc=4 (max helper); cold path isolation via [NoInlining]

---

### W7-084-T4: Extract `AuditFleet_ClearPositionPassState`

**Type:** extraction
**Priority:** P1
**CYC Impact:** N/A → 2 (new helper)

#### Description

Extract the position-pass state cleanup logic into a new private helper
`AuditFleet_ClearPositionPassState`. This helper removes the per-account failure-first-seen
timestamp from `_positionPassFailedFirstSeen` (a `ConcurrentDictionary`) when the FSM
has recovered to a non-zero expected quantity, clearing the alarm state.

**Target signature:**
```csharp
private void AuditFleet_ClearPositionPassState(
    string accountName,
    int fsmExpectedQty
)
```

**Extracted logic:**
- `if (fsmExpectedQty != 0)` => `_positionPassFailedFirstSeen.TryRemove(accountName, out _)`

**Location:** `src/V12_002.REAPER.Audit.cs` (same file, private method)

#### Acceptance Criteria

- [ ] New private method `AuditFleet_ClearPositionPassState` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] Uses `TryRemove` on `_positionPassFailedFirstSeen` (lock-free ConcurrentDictionary method)
- [ ] Conditional guard `if (fsmExpectedQty != 0)` preserved
- [ ] Method contains zero `lock()` blocks
- [ ] CYC of extracted helper = 2

**CYC Impact:** extraction cyc=2 (new helper); contributes to parent CYC reduction

---

### W7-084-T5: Extract `AuditFleet_AssembleOutputs`

**Type:** extraction
**Priority:** P1
**CYC Impact:** N/A → 3 (new helper)

#### Description

Extract the output assembly logic into a new private helper `AuditFleet_AssembleOutputs`.
This helper is a pure assignment method: it maps resolved state variables into the 5
out-parameters that callers of `AuditFleet_CalculateExpectedActual` consume. It must have
no observable side effects beyond assigning the out-parameters.

**Target signature:**
```csharp
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
```

**Extracted logic:**
- `expectedKey = ExpKey(accountName)`
- `expectedQty = fsmExpectedQty`
- `syncPending = _dispatchSyncPendingExpKeys.ContainsKey(expectedKey)`
- `inFillGrace = IsReaperFillGraceActive(expectedKey)`
- `hasState = expectedQty != 0 || actualQty != 0`

**Location:** `src/V12_002.REAPER.Audit.cs` (same file, private method)

#### Acceptance Criteria

- [ ] New private method `AuditFleet_AssembleOutputs` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] All 5 out-parameters are assigned in every code path
- [ ] `hasState` uses logical-OR of both `expectedQty != 0` and `actualQty != 0`
- [ ] Method contains zero side effects beyond out-parameter assignment
- [ ] Method contains zero `lock()` blocks
- [ ] CYC of extracted helper = 3

**CYC Impact:** extraction cyc=3 (new helper); final output assembly isolated from orchestrator

---

### W7-084-T6: Refactor Parent `AuditFleet_CalculateExpectedActual` to Orchestrate Helpers

**Type:** refactor
**Priority:** P1 (depends on T1-T5)
**CYC Impact:** 382 → 6

#### Description

After all 5 helper extractions (T1-T5) are complete, replace the body of
`AuditFleet_CalculateExpectedActual` with a pure orchestrator that delegates to each helper
in sequence. The parent method's public signature (9 parameters) must remain unchanged so
that the single call site in `AuditSingleFleetAccount` compiles without modification.

**Replacement body:**
```csharp
AuditFleet_ResolvePosition(acct, out actualQty, out pos);
AuditFleet_CollectFsmState(acct, out accountFsms, out int fsmExpectedQty);
AuditFleet_ReconcileStaleFsms(accountFsms, acct.Name, actualQty, ref fsmExpectedQty);
AuditFleet_ClearPositionPassState(acct.Name, fsmExpectedQty);
AuditFleet_AssembleOutputs(acct.Name, actualQty, fsmExpectedQty,
    out expectedKey, out expectedQty, out syncPending, out inFillGrace, out hasState);
if (shouldLog && hasState)
{
    Print($"[REAPER] {acct.Name}: Expected={expectedQty}, Actual={actualQty}");
}
```

**Location:** `src/V12_002.REAPER.Audit.cs`

#### Acceptance Criteria

- [ ] Parent method `AuditFleet_CalculateExpectedActual` signature is unchanged (9 params)
- [ ] Parent body contains exactly 5 helper calls + 1 conditional logging block
- [ ] `AuditSingleFleetAccount` call site compiles without modification
- [ ] Build passes: `dotnet build src/` returns 0 errors
- [ ] CYC of parent after refactor = 6 (5 calls + if-shouldLog + if-hasState = ~6 paths)
- [ ] All 9 out-parameters are assigned via helpers before the logging block
- [ ] Method contains zero `lock()` blocks

**CYC Impact:** parent cyc 382→6 (98.4% reduction); max_cyc_projected=6

---

### W7-084-T7: Verify Lock-Free Mandate — Zero `lock()` Blocks

**Type:** verification
**Priority:** P0 (depends on T1-T6)
**CYC Impact:** none (audit only)

#### Description

Run a static scan to confirm that none of the 6 methods introduced or modified in this epic
contain any `lock()` statement. The V12 Lock-Free Actor Pattern mandates that all state
mutations use `ConcurrentDictionary` methods or atomic primitives — never `lock()` blocks.
This ticket is a blocking gate: the epic cannot proceed to Phase 5 execution if any `lock()`
is found.

**Methods to scan:**
1. `AuditFleet_CalculateExpectedActual` (refactored parent)
2. `AuditFleet_ResolvePosition`
3. `AuditFleet_CollectFsmState`
4. `AuditFleet_ReconcileStaleFsms`
5. `AuditFleet_ClearPositionPassState`
6. `AuditFleet_AssembleOutputs`

**Scan command:**
```bash
grep -n "lock(" src/V12_002.REAPER.Audit.cs
```
Expected: zero matches.

#### Acceptance Criteria

- [ ] `grep -n "lock(" src/V12_002.REAPER.Audit.cs` returns zero matches
- [ ] `search_ast` with pattern `call:lock` on file returns `total_matches=0`
- [ ] All state access uses `ConcurrentDictionary` methods (`TryRemove`, `ContainsKey`, etc.)
- [ ] Result documented in ticket completion report

**CYC Impact:** cyc=0 (verification only); lock-free mandate confirmed

---

### W7-084-T8: Verify CYC Compliance — All Methods <= 8

**Type:** verification
**Priority:** P0 (depends on T1-T6)
**CYC Impact:** none (audit only)

#### Description

Run a cyclomatic complexity audit to confirm all 6 methods (5 new helpers + refactored parent)
meet the Jane Street strict threshold of CYC <= 8. The target max_cyc_projected is 6. Any
method exceeding CYC=8 is a P0 blocker and must be re-extracted before Phase 5 execution.

**Expected CYC values after extraction:**

| Method | Expected CYC |
|---|---|
| `AuditFleet_CalculateExpectedActual` | 6 |
| `AuditFleet_ResolvePosition` | 3 |
| `AuditFleet_CollectFsmState` | 2 |
| `AuditFleet_ReconcileStaleFsms` | 4 |
| `AuditFleet_ClearPositionPassState` | 2 |
| `AuditFleet_AssembleOutputs` | 3 |
| **max_cyc_projected** | **6** |

**Verification command:**
```bash
python scripts/complexity_audit.py --file src/V12_002.REAPER.Audit.cs
```

#### Acceptance Criteria

- [ ] `AuditFleet_CalculateExpectedActual` CYC = 6 (within <= 8 threshold)
- [ ] All 5 helper methods CYC <= 4 (well within <= 8 threshold)
- [ ] max_cyc_projected = 6 confirmed
- [ ] No method in `src/V12_002.REAPER.Audit.cs` exceeds CYC = 8 post-refactor
- [ ] Complexity audit output saved to ticket completion report

**CYC Impact:** cyc=0 (verification only); compliance with Jane Street CYC<=8 confirmed

---

### W7-084-T9: Update EPIC-W7-084 Manifest

**Type:** housekeeping
**Priority:** P2 (depends on T7 + T8)
**CYC Impact:** none

#### Description

Update `docs/brain/EPIC-W7-084/manifest.json` to record Phase 5 (ticket execution) completion
status once all code-writing and verification tickets (T1-T8) have passed. The manifest is the
authoritative state tracker for the EPIC-W7-084 lifecycle. Setting `phase_5.status = "completed"`
signals to the Phase 5.V orchestrator that verification can proceed.

**Fields to update:**
- `phases.phase_5.status` = `"completed"`
- `phases.phase_5.output` = `"ticket-1-completion.md"` (or aggregated)
- `phases.phase_5.completed_at` = ISO timestamp
- `phases.phase_5.cyc_after` = `6`
- `phases.phase_5.lock_free_verified` = `true`
- `phases.phase_5.cyc_compliant` = `true`

#### Acceptance Criteria

- [ ] `phases.phase_5.status` = `"completed"` in `docs/brain/EPIC-W7-084/manifest.json`
- [ ] `phases.phase_5.cyc_after` = `6` recorded
- [ ] `phases.phase_5.lock_free_verified` = `true` recorded
- [ ] `phases.phase_5.cyc_compliant` = `true` recorded
- [ ] `phases.phase_4.ticket_count` = `9` recorded (this phase's ticket count updated)
- [ ] Manifest is valid JSON (parseable without errors)

**CYC Impact:** cyc=0 (housekeeping); manifest updated to reflect 100% epic completion

---

## Ticket Summary

| Ticket | Type | CYC Impact | Depends On |
|---|---|---|---|
| W7-084-T1 | extraction | N/A → 3 | — |
| W7-084-T2 | extraction | N/A → 2 | — |
| W7-084-T3 | extraction [NoInlining] | N/A → 4 | — |
| W7-084-T4 | extraction | N/A → 2 | — |
| W7-084-T5 | extraction | N/A → 3 | — |
| W7-084-T6 | refactor | 382 → 6 | T1-T5 |
| W7-084-T7 | verification | 0 (audit) | T1-T6 |
| W7-084-T8 | verification | 0 (audit) | T1-T6 |
| W7-084-T9 | housekeeping | 0 | T7, T8 |

**max_cyc_projected = 6** | **extraction_count = 5** | **CYC reduction: 382 → 6 (98.4%)**

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 0.5 |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-084 |
| **Method** | AuditFleet_CalculateExpectedActual |
| **CYC Baseline** | 382 |
| **max_cyc_projected** | 6 |
| **extraction_count** | 5 |
| **ticket_count** | 9 |
| **Sequential Thoughts** | 4 |
| **Output** | docs/brain/EPIC-W7-084/04-tickets.md |
