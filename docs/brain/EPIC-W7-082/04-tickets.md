# EPIC-W7-082 — Phase 4: Ticket Generation

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T23:30:00Z
**Input:** docs/brain/EPIC-W7-082/02-architecture-plan.md + docs/brain/EPIC-W7-082/03-audit-report.md

---

## Epic Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-082 |
| **Method** | `AuditSingleFleetAccount` |
| **Source File** | `src/V12_002.REAPER.Audit.cs` |
| **Lane** | P4-L5 |
| **Original CYC** | 90 (HARDEST EPIC IN WAVE 7) |
| **Target CYC (parent)** | 6 |
| **max_cyc_projected** | 8 (Jane Street threshold) |
| **extraction_count** | 11 (5 pre-existing + 6 new) |
| **DNA Verdict (Phase 3)** | PASS — 0 violations |
| **ticket_count** | 11 |
| **Sequential Thinking Thoughts** | 7 |

---

## CYC Reduction Overview

| Helper | Type | Projected CYC |
|---|---|---|
| `AuditSingleFleetAccount` (parent, after) | Dispatcher | **6** |
| `AuditFleet_CalculateExpectedActual` | Pre-existing | **7** |
| `AuditFleet_HandleDesyncRepair` | Pre-existing | **6** |
| `AuditFleet_CheckPositionPassGrace` | Pre-existing | **6** |
| `AuditFleet_HandleCriticalDesyncFlatten` | Pre-existing | **7** |
| `AuditFleet_HandleNakedPosition` | Pre-existing | **8** |
| `AuditFleet_HandleDesyncBranch` | New | **5** |
| `AuditFleet_EvaluateCriticalDesync` | New | **5** |
| `AuditFleet_ProcessOrphanFsmLoop` | New | **3** |
| `AuditFleet_LogMinorDesync` | New [NoInlining] | **2** |
| `AuditFleet_ResolveSyncState` | New | **4** |
| `AuditFleet_BuildStateSnapshot` | New | **4** |

**Total CYC reduction:** 90 → 6 (parent) + distributed across 11 helpers, all <= 8.

---

## Execution Order

Tickets must be executed in sequence. Each code-changing ticket requires a passing build before the next ticket starts.

```
T1 (read-only audit) → T2 → T3 → T4 → T5 → T6 → T7 → T8 → T9 (read-only) → T10 (read-only) → T11
```

---

## Tickets

---

### W7-082-T1: Verify Pre-Existing Helpers Are Correctly Integrated

**Type:** Read-Only Audit (no code changes)
**Priority:** P0 — must pass before any extraction begins

**Description:**
Before any extraction work starts, confirm that all 5 pre-existing `AuditFleet_*` helpers exist in
`src/V12_002.REAPER.Audit.cs` and are being correctly called from `AuditSingleFleetAccount`.
These helpers were confirmed by jcodemunch `get_file_outline` and `get_context_bundle` in Phase 2.
This ticket is read-only: if any helper is missing or miscalled, raise a blocker before proceeding
to T2.

Pre-existing helpers to verify (jcodemunch-verified names from Phase 2 architecture plan):
1. `AuditFleet_CalculateExpectedActual` (lines 382-451, CYC=7) — populates all out-params
2. `AuditFleet_HandleDesyncRepair` (lines 196-249, CYC=6) — ghost position handler
3. `AuditFleet_CheckPositionPassGrace` (lines 254-291, CYC=6) — grace period circuit breaker
4. `AuditFleet_HandleCriticalDesyncFlatten` (lines 295-331, CYC=7) — critical desync flatten
5. `AuditFleet_HandleNakedPosition` (lines 335-380, CYC=8) — naked position detection

**Acceptance Criteria:**
- [ ] All 5 pre-existing helpers are present in `src/V12_002.REAPER.Audit.cs`
- [ ] `AuditFleet_CalculateExpectedActual` is called from `AuditSingleFleetAccount` with correct out-params (actualQty, expectedQty, expectedKey, syncPending, inFillGrace, hasState, accountFsms, pos)
- [ ] Each pre-existing helper has CYC <= 8 (verified by `python scripts/complexity_audit.py`)
- [ ] `dotnet build src/` passes with 0 errors (baseline confirmation)
- [ ] No pre-existing helper contains a `lock()` block (run `grep -n "lock(" src/V12_002.REAPER.Audit.cs`)
- [ ] `Thread.MemoryBarrier()` is present before cross-thread field reads in `AuditFleet_CalculateExpectedActual` (gjengset cache-line pattern)
- [ ] Blocker raised if any helper is missing or any AC fails

**CYC Impact:** 0 (read-only ticket — no code changes)

---

### W7-082-T2: Extract AuditFleet_HandleDesyncBranch

**Type:** Code Extraction
**Priority:** P0 — highest CYC reduction, must be first extraction
**Depends on:** W7-082-T1 (PASS)

**Description:**
Extract the outer `if (expectedQty != actualQty)` branch tree from `AuditSingleFleetAccount`
into a new private helper `AuditFleet_HandleDesyncBranch`. This single extraction delivers the
largest CYC reduction from the parent method (estimated ~40+ CYC points removed from parent).

The helper owns: ghost-position path, `isCriticalDesync` compound evaluation, grace-defer routing,
critical flatten dispatch, and minor desync logging branch. It returns `hasState` as a pass-through
boolean so the parent can return it directly.

**Signature:**
```csharp
private bool AuditFleet_HandleDesyncBranch(
    Account acct,
    bool shouldLog,
    int expectedQty,
    int actualQty,
    bool syncPending,
    bool inFillGrace,
    List<FollowerBracketFSM> accountFsms,
    bool hasState)
```

**Location:** Insert after line 527 in `src/V12_002.REAPER.Audit.cs` (after last pre-existing helper).

**Parent call site after extraction:**
```csharp
if (expectedQty != actualQty)
    return AuditFleet_HandleDesyncBranch(
        acct, shouldLog, expectedQty, actualQty,
        syncPending, inFillGrace, accountFsms, hasState);
```

**Jane Street Requirements:**
- No LINQ in body (carl_cook zero-alloc)
- No string interpolation — use `LogBuffer.Format` for any log calls (carl_cook)
- Uses `EnqueueReaperRepairCandidate` / `EnqueueReaperFlattenCandidate` for state mutations (V12 lock-free actor)
- NO `lock()` blocks anywhere in method body (V12 lock-free mandate)

**Acceptance Criteria:**
- [ ] New method `AuditFleet_HandleDesyncBranch` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] Signature matches specification exactly (return type bool, all 8 parameters)
- [ ] Parent `AuditSingleFleetAccount` now calls `AuditFleet_HandleDesyncBranch` at the desync branch
- [ ] CYC of `AuditFleet_HandleDesyncBranch` <= 5 (verified by `python scripts/complexity_audit.py`)
- [ ] No `lock()` blocks in the new method (`grep -n "lock(" src/V12_002.REAPER.Audit.cs` returns 0 matches in new method)
- [ ] No LINQ usage in new method body
- [ ] `dotnet build src/` passes with 0 errors
- [ ] `dotnet csharpier check src/` passes with 0 issues
- [ ] Pre-existing helpers remain unmodified (no scope creep)

**CYC Impact:** Parent CYC 90 → ~50 (estimated ~40 CYC points removed). New helper CYC = 5.

---

### W7-082-T3: Extract AuditFleet_EvaluateCriticalDesync

**Type:** Code Extraction (sub-extraction from T2 result)
**Priority:** P1
**Depends on:** W7-082-T2 (PASS)

**Description:**
Extract the `isCriticalDesync` compound-boolean evaluation plus grace-defer routing and critical
flatten dispatch from the newly created `AuditFleet_HandleDesyncBranch` into a new helper
`AuditFleet_EvaluateCriticalDesync`. This is a sub-extraction: the source is the T2 helper, not
the original parent method.

This helper embodies the defense-in-depth circuit-breaker pattern (trading_billions): it isolates
the DECISION logic (is this critical?) from the ACTION logic (flatten). It calls
`AuditFleet_CheckPositionPassGrace` (pre-existing grace-period gate) and
`AuditFleet_HandleCriticalDesyncFlatten` (pre-existing action) but owns only the routing decision.

**Signature:**
```csharp
private void AuditFleet_EvaluateCriticalDesync(
    Account acct,
    bool shouldLog,
    int expectedQty,
    int actualQty,
    bool hasState)
```

**Jane Street Requirements:**
- Single-responsibility: evaluates and routes only — does NOT perform the flatten action itself (trading_billions defense-in-depth)
- No LINQ (carl_cook zero-alloc)
- NO `lock()` blocks (V12 lock-free mandate)
- Circuit-breaker: `AuditFleet_CheckPositionPassGrace` must be the gate — never bypass it

**Acceptance Criteria:**
- [ ] New method `AuditFleet_EvaluateCriticalDesync` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] Signature matches specification exactly (void return, 5 parameters)
- [ ] `AuditFleet_HandleDesyncBranch` now calls `AuditFleet_EvaluateCriticalDesync` at the isCriticalDesync branch
- [ ] `AuditFleet_CheckPositionPassGrace` is called inside `AuditFleet_EvaluateCriticalDesync` as the grace gate (circuit-breaker pattern preserved)
- [ ] CYC of `AuditFleet_EvaluateCriticalDesync` <= 5
- [ ] No `lock()` blocks in new method
- [ ] `dotnet build src/` passes with 0 errors
- [ ] `dotnet csharpier check src/` passes with 0 issues

**CYC Impact:** `AuditFleet_HandleDesyncBranch` CYC reduced by ~3 points (desync evaluation branches removed). New helper CYC = 5.

---

### W7-082-T4: Extract AuditFleet_ProcessOrphanFsmLoop

**Type:** Code Extraction
**Priority:** P1
**Depends on:** W7-082-T2 (PASS) — extracted from parent, not from T2 result

**Description:**
Extract the `foreach (var fsm in accountFsms) DetectOrphanFSM(...)` loop from
`AuditSingleFleetAccount` (the parent method, after T2 removal of desync branch) into a new helper
`AuditFleet_ProcessOrphanFsmLoop`. This isolates loop-iteration complexity from the parent
dispatcher's decision tree.

Single concern: iterate accountFsms and call DetectOrphanFSM for each. No decisions other than
the implicit loop iteration.

**Signature:**
```csharp
private void AuditFleet_ProcessOrphanFsmLoop(
    List<FollowerBracketFSM> accountFsms,
    string acctName,
    int actualQty)
```

**Parent call site after extraction:**
```csharp
AuditFleet_ProcessOrphanFsmLoop(accountFsms, acct.Name, actualQty);
```

**Jane Street Requirements:**
- Single-concern loop wrapper only (trading_billions single-responsibility)
- No LINQ (carl_cook zero-alloc) — use explicit foreach
- NO `lock()` blocks (V12 lock-free mandate)

**Acceptance Criteria:**
- [ ] New method `AuditFleet_ProcessOrphanFsmLoop` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] Signature matches specification exactly (void return, 3 parameters)
- [ ] Parent `AuditSingleFleetAccount` calls `AuditFleet_ProcessOrphanFsmLoop(accountFsms, acct.Name, actualQty)`
- [ ] The foreach DetectOrphanFSM loop is fully moved out of the parent
- [ ] CYC of `AuditFleet_ProcessOrphanFsmLoop` <= 3
- [ ] No LINQ in new method (explicit foreach required)
- [ ] No `lock()` blocks in new method
- [ ] `dotnet build src/` passes with 0 errors
- [ ] `dotnet csharpier check src/` passes with 0 issues

**CYC Impact:** Parent CYC further reduced (~3 CYC points from loop). New helper CYC = 3.

---

### W7-082-T5: Extract AuditFleet_LogMinorDesync [NoInlining]

**Type:** Code Extraction (cold-path isolation)
**Priority:** P1
**Depends on:** W7-082-T2 (PASS) — sub-extraction from AuditFleet_HandleDesyncBranch

**Description:**
Extract the minor desync logging branch (`else if (shouldLog) Print(...)`) from
`AuditFleet_HandleDesyncBranch` into a dedicated cold-path helper `AuditFleet_LogMinorDesync`.

CRITICAL: This method MUST be decorated with `[MethodImpl(MethodImplOptions.NoInlining)]`.
Per carl_cook zero-alloc pattern: cold logging paths must be kept off the hot-path instruction
cache. Without NoInlining, the JIT may inline this into the hot audit path, polluting the
instruction cache and adding 10-30ns latency.

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void AuditFleet_LogMinorDesync(
    string acctName,
    int expectedQty,
    int actualQty)
```

**ASCII-Only Mandate:** All `Print(...)` string literals in this method must contain ONLY ASCII
characters (0x00-0x7F). No Unicode, no emoji, no curly quotes, no em-dashes.

**Jane Street Requirements:**
- `[MethodImpl(MethodImplOptions.NoInlining)]` REQUIRED — cold path off hot instruction cache (carl_cook)
- ASCII-only strings in all Print(...) calls (V12 ASCII-Only Compliance)
- No string interpolation — use `LogBuffer.Format` or direct string concatenation
- CYC=2 maximum: single shouldLog branch + one Print call

**Acceptance Criteria:**
- [ ] New method `AuditFleet_LogMinorDesync` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] `[MethodImpl(MethodImplOptions.NoInlining)]` attribute is present on the method (NOT AggressiveInlining)
- [ ] Signature matches specification exactly (void return, 3 parameters)
- [ ] `AuditFleet_HandleDesyncBranch` calls `AuditFleet_LogMinorDesync(acct.Name, expectedQty, actualQty)` in the minor desync else-if branch
- [ ] All `Print(...)` string literals contain only ASCII characters (verified by `grep -P '[\x80-\xFF]'` returning 0 matches in method body)
- [ ] No string interpolation (`$"..."`) in method body
- [ ] CYC of `AuditFleet_LogMinorDesync` <= 2
- [ ] No `lock()` blocks in new method
- [ ] `dotnet build src/` passes with 0 errors
- [ ] `dotnet csharpier check src/` passes with 0 issues

**CYC Impact:** `AuditFleet_HandleDesyncBranch` CYC reduced by ~1-2 points. New helper CYC = 2.

---

### W7-082-T6: Extract AuditFleet_ResolveSyncState

**Type:** Code Extraction (sub-extraction from pre-existing helper)
**Priority:** P1
**Depends on:** W7-082-T1 (PASS) — sub-extraction from AuditFleet_CalculateExpectedActual

**Description:**
Extract the `syncPending` and `inFillGrace` resolution logic from the pre-existing helper
`AuditFleet_CalculateExpectedActual` into a new helper `AuditFleet_ResolveSyncState`. This reduces
`AuditFleet_CalculateExpectedActual`'s own CYC independently, keeping it well under 8.

IMPORTANT: This is a sub-extraction from `AuditFleet_CalculateExpectedActual` — NOT from the
parent `AuditSingleFleetAccount`. The pre-existing helper's external signature must remain
unchanged.

**Signature:**
```csharp
private void AuditFleet_ResolveSyncState(
    Account acct,
    bool shouldLog,
    out bool syncPending,
    out bool inFillGrace)
```

**Jane Street Requirements:**
- Out-parameters maintain zero-heap-allocation pattern (carl_cook Left-Right read path)
- No LINQ (carl_cook zero-alloc)
- NO `lock()` blocks (V12 lock-free mandate)
- `IsReaperFillGraceActive` must be called inside this helper (confirmed callee at depth 2 by jcodemunch call hierarchy)

**Acceptance Criteria:**
- [ ] New method `AuditFleet_ResolveSyncState` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] Signature matches specification (void return, 2 in-params, 2 out-params)
- [ ] `AuditFleet_CalculateExpectedActual` calls `AuditFleet_ResolveSyncState` with correct parameters
- [ ] `AuditFleet_CalculateExpectedActual` external signature is UNCHANGED (no scope creep into callers)
- [ ] CYC of `AuditFleet_ResolveSyncState` <= 4
- [ ] `IsReaperFillGraceActive` is called within `AuditFleet_ResolveSyncState` (not removed)
- [ ] No `lock()` blocks in new method
- [ ] `dotnet build src/` passes with 0 errors
- [ ] `dotnet csharpier check src/` passes with 0 issues

**CYC Impact:** `AuditFleet_CalculateExpectedActual` CYC reduced by ~4 points (sync resolution branches extracted). New helper CYC = 4.

---

### W7-082-T7: Extract AuditFleet_BuildStateSnapshot

**Type:** Code Extraction (sub-extraction from pre-existing helper)
**Priority:** P1
**Depends on:** W7-082-T6 (PASS) — sub-extraction from AuditFleet_CalculateExpectedActual (now smaller after T6)

**Description:**
Extract the FSM registry lookup and snapshot assembly logic from the pre-existing helper
`AuditFleet_CalculateExpectedActual` into a new helper `AuditFleet_BuildStateSnapshot`. This
isolates the `hasState`, `accountFsms`, `pos`, and `expectedKey` assembly logic from the
syncPending/fillGrace resolution already extracted in T6.

**Signature:**
```csharp
private void AuditFleet_BuildStateSnapshot(
    Account acct,
    bool shouldLog,
    out bool hasState,
    out List<FollowerBracketFSM> accountFsms,
    out Position pos,
    out string expectedKey)
```

**Jane Street Requirements:**
- Out-parameters maintain zero-heap-allocation pattern (carl_cook Left-Right read path)
- FSM state reads must use `GetFsmExpectedPosition` (confirmed callee at depth 2 in jcodemunch hierarchy)
- No LINQ (carl_cook zero-alloc)
- NO `lock()` blocks (V12 lock-free mandate)
- `Thread.MemoryBarrier()` before reading shared FSM state fields (gjengset cache-line pattern)

**Acceptance Criteria:**
- [ ] New method `AuditFleet_BuildStateSnapshot` exists in `src/V12_002.REAPER.Audit.cs`
- [ ] Signature matches specification (void return, 2 in-params, 4 out-params)
- [ ] `AuditFleet_CalculateExpectedActual` calls `AuditFleet_BuildStateSnapshot` with correct parameters
- [ ] `AuditFleet_CalculateExpectedActual` external signature is UNCHANGED
- [ ] `GetFsmExpectedPosition` is called within `AuditFleet_BuildStateSnapshot`
- [ ] `Thread.MemoryBarrier()` present before shared FSM state reads (gjengset)
- [ ] CYC of `AuditFleet_BuildStateSnapshot` <= 4
- [ ] No `lock()` blocks in new method
- [ ] `dotnet build src/` passes with 0 errors
- [ ] `dotnet csharpier check src/` passes with 0 issues

**CYC Impact:** `AuditFleet_CalculateExpectedActual` CYC further reduced by ~4 points. New helper CYC = 4.

---

### W7-082-T8: Refactor Parent AuditSingleFleetAccount to Final Dispatcher (CYC 90 → 6)

**Type:** Code Refactor (final parent cleanup)
**Priority:** P0 — final integration ticket
**Depends on:** W7-082-T2, T3, T4, T5, T6, T7 (all PASS)

**Description:**
With all 6 new helpers extracted (T2-T7), finalize the parent `AuditSingleFleetAccount` method
to match the clean 14-line dispatcher body defined in the architecture plan. Add
`[MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute — the sub-10-line dispatcher body
qualifies per carl_cook zero-alloc inlining guidance.

**Final Parent Body (from architecture plan):**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool AuditSingleFleetAccount(Account acct, bool shouldLog)
{
    AuditFleet_CalculateExpectedActual(
        acct, shouldLog,
        out int actualQty, out int expectedQty, out string expectedKey,
        out bool syncPending, out bool inFillGrace, out bool hasState,
        out List<FollowerBracketFSM> accountFsms, out Position pos);

    if (expectedQty != actualQty)
        return AuditFleet_HandleDesyncBranch(
            acct, shouldLog, expectedQty, actualQty,
            syncPending, inFillGrace, accountFsms, hasState);

    AuditFleet_ProcessOrphanFsmLoop(accountFsms, acct.Name, actualQty);

    if (actualQty != 0)
        AuditFleet_HandleNakedPosition(acct, pos, actualQty, expectedKey, shouldLog);

    return hasState;
}
```

**Jane Street Requirements:**
- `[MethodImpl(MethodImplOptions.AggressiveInlining)]` on parent dispatcher (carl_cook hot-path inlining)
- Signature UNCHANGED: `private bool AuditSingleFleetAccount(Account acct, bool shouldLog)`
- No residual inline logic — all complexity delegated to helpers
- ASCII-only in any remaining Print/Log calls
- NO `lock()` blocks (V12 lock-free mandate)

**Acceptance Criteria:**
- [ ] `AuditSingleFleetAccount` body matches the 14-line dispatcher pattern above (or functionally equivalent)
- [ ] `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute is present on the method
- [ ] Method signature `private bool AuditSingleFleetAccount(Account acct, bool shouldLog)` is UNCHANGED
- [ ] All 6 new helpers are called: `AuditFleet_HandleDesyncBranch`, `AuditFleet_EvaluateCriticalDesync` (called from T2 result), `AuditFleet_ProcessOrphanFsmLoop`, `AuditFleet_LogMinorDesync` (called from T2 result), `AuditFleet_ResolveSyncState` (called from pre-existing), `AuditFleet_BuildStateSnapshot` (called from pre-existing)
- [ ] Parent CYC = 6 (verified by `python scripts/complexity_audit.py` — exactly 2 if-branches + 1 method-call return = 6 decision points)
- [ ] No `lock()` blocks in parent method body
- [ ] `dotnet build src/` passes with 0 errors
- [ ] `dotnet csharpier check src/` passes with 0 issues
- [ ] Caller `AuditApexPositions` still compiles without modification (blast radius check)

**CYC Impact:** Parent CYC 90 → **6**. Total CYC reduction = 84 points distributed across 11 helpers. This is the primary success metric for EPIC-W7-082.

---

### W7-082-T9: Verify Lock-Free Mandate — Zero lock() Blocks in All New Helpers

**Type:** Read-Only Verification
**Priority:** P0 — V12 DNA mandatory check
**Depends on:** W7-082-T8 (PASS)

**Description:**
Run a targeted lock-free audit across all 6 newly extracted helpers. The V12 Lock-Free Actor
Pattern mandates zero `lock()` blocks anywhere in the file, with all state mutations using the
`Enqueue` model (`EnqueueReaperRepairCandidate`, `EnqueueReaperFlattenCandidate`). The Phase 3
DNA audit confirmed 0 lock() blocks in the baseline — verify this invariant is preserved
after all extractions.

**Helpers to verify:**
1. `AuditFleet_HandleDesyncBranch`
2. `AuditFleet_EvaluateCriticalDesync`
3. `AuditFleet_ProcessOrphanFsmLoop`
4. `AuditFleet_LogMinorDesync`
5. `AuditFleet_ResolveSyncState`
6. `AuditFleet_BuildStateSnapshot`

**Commands:**
```bash
grep -n "lock(" src/V12_002.REAPER.Audit.cs
grep -n "Monitor.Enter" src/V12_002.REAPER.Audit.cs
grep -n "Mutex" src/V12_002.REAPER.Audit.cs
```

**Additional Enqueue pattern verification:**
```bash
grep -n "EnqueueReaper" src/V12_002.REAPER.Audit.cs
```
Confirm `EnqueueReaperRepairCandidate` and `EnqueueReaperFlattenCandidate` are present and called
from within the new helpers (not removed or replaced with direct state mutation).

**Volatile field verification (gjengset cache-line pattern):**
- Confirm `_repairInFlight` field has `volatile` keyword
- Confirm `_reaperFlattenInFlight` field has `volatile` keyword

**Acceptance Criteria:**
- [ ] `grep -n "lock(" src/V12_002.REAPER.Audit.cs` returns 0 matches
- [ ] `grep -n "Monitor.Enter" src/V12_002.REAPER.Audit.cs` returns 0 matches
- [ ] `grep -n "Mutex" src/V12_002.REAPER.Audit.cs` returns 0 matches
- [ ] `EnqueueReaperRepairCandidate` call present in `src/V12_002.REAPER.Audit.cs` (state mutation via Enqueue)
- [ ] `EnqueueReaperFlattenCandidate` call present in `src/V12_002.REAPER.Audit.cs` (state mutation via Enqueue)
- [ ] `_repairInFlight` field declared with `volatile` modifier
- [ ] `_reaperFlattenInFlight` field declared with `volatile` modifier
- [ ] All findings documented in ticket completion report

**CYC Impact:** 0 (verification-only ticket)

---

### W7-082-T10: Verify CYC Compliance — All Methods <= 8 (max_cyc_projected=8)

**Type:** Read-Only Verification
**Priority:** P0 — Jane Street threshold compliance
**Depends on:** W7-082-T8 (PASS)

**Description:**
Run `python scripts/complexity_audit.py` against `src/V12_002.REAPER.Audit.cs` and confirm
all 12 methods in scope are at or below the Jane Street threshold of CYC <= 8.

This is the primary quality gate for EPIC-W7-082. With the original method at CYC=90, reducing
to max_cyc_projected=8 represents an 89% complexity reduction and full V12 DNA compliance.

**Methods to verify (12 total):**

| Method | Type | Target CYC |
|---|---|---|
| `AuditSingleFleetAccount` | Parent (dispatcher) | <= 6 |
| `AuditFleet_CalculateExpectedActual` | Pre-existing | <= 7 |
| `AuditFleet_HandleDesyncRepair` | Pre-existing | <= 6 |
| `AuditFleet_CheckPositionPassGrace` | Pre-existing | <= 6 |
| `AuditFleet_HandleCriticalDesyncFlatten` | Pre-existing | <= 7 |
| `AuditFleet_HandleNakedPosition` | Pre-existing | <= 8 |
| `AuditFleet_HandleDesyncBranch` | New | <= 5 |
| `AuditFleet_EvaluateCriticalDesync` | New | <= 5 |
| `AuditFleet_ProcessOrphanFsmLoop` | New | <= 3 |
| `AuditFleet_LogMinorDesync` | New [NoInlining] | <= 2 |
| `AuditFleet_ResolveSyncState` | New | <= 4 |
| `AuditFleet_BuildStateSnapshot` | New | <= 4 |

**Commands:**
```bash
python scripts/complexity_audit.py
dotnet build src/
```

**Acceptance Criteria:**
- [ ] `python scripts/complexity_audit.py` reports 0 methods exceeding CYC 8 in `src/V12_002.REAPER.Audit.cs`
- [ ] `AuditSingleFleetAccount` CYC == 6 (confirmed by audit script)
- [ ] `AuditFleet_HandleNakedPosition` CYC <= 8 (boundary condition, max allowed)
- [ ] All 6 new helpers report CYC at or below their target values in the table above
- [ ] All 5 pre-existing helpers report CYC at or below their target values
- [ ] `dotnet build src/` passes with 0 errors (final build confirmation)
- [ ] Results captured in ticket completion report with exact CYC values per method

**CYC Impact:** 0 (verification-only ticket). Confirms total CYC reduction of 84 points (90 → 6).

---

### W7-082-T11: Update Manifest and Run deploy-sync.ps1

**Type:** Administrative / Infrastructure
**Priority:** P1 — mandatory V12 hard-link sync
**Depends on:** W7-082-T10 (PASS)

**Description:**
Finalize the EPIC-W7-082 lifecycle by updating `docs/brain/EPIC-W7-082/manifest.json` with
Phase 5 completion data and running `deploy-sync.ps1` to re-synchronize NinjaTrader hard links.
Per V12 Architecture Mandate, every `src/` modification MUST be followed by `deploy-sync.ps1`.

**Commands:**
```bash
powershell -File ./deploy-sync.ps1
dotnet build src/
```

**Manifest updates required:**
- `phases.phase_5.status` = `"completed"`
- `phases.phase_5.output` = `"ticket-1-completion.md"` through `"ticket-11-completion.md"`
- `phases.phase_5.completed_at` = ISO 8601 timestamp
- `phases.phase_5.cyc_achieved` = 6 (parent)
- `phases.phase_5.max_cyc_achieved` = 8
- `phases.phase_5.extraction_count` = 11
- `phases.phase_5.lock_free_verified` = true
- `phases.phase_5.deploy_sync_run` = true

**Acceptance Criteria:**
- [ ] `powershell -File ./deploy-sync.ps1` exits with code 0 (no hard-link sync errors)
- [ ] `dotnet build src/` passes with 0 errors post-sync (NinjaTrader hard-link integrity confirmed)
- [ ] `docs/brain/EPIC-W7-082/manifest.json` updated with Phase 5 completion fields
- [ ] `manifest.json` field `phases.phase_5.cyc_achieved` = 6
- [ ] `manifest.json` field `phases.phase_5.lock_free_verified` = true
- [ ] `manifest.json` field `phases.phase_5.deploy_sync_run` = true
- [ ] All ticket completion reports (`ticket-N-completion.md`) exist for tickets T1-T11

**CYC Impact:** 0 (administrative ticket)

---

## Summary

| Ticket | Type | CYC Impact | Depends On |
|---|---|---|---|
| W7-082-T1 | Read-only audit | 0 | — |
| W7-082-T2 | Extraction | Parent ~90→~50, new helper CYC=5 | T1 |
| W7-082-T3 | Sub-extraction | HandleDesyncBranch -3, new CYC=5 | T2 |
| W7-082-T4 | Extraction | Parent further reduced, new CYC=3 | T2 |
| W7-082-T5 | Sub-extraction [NoInlining] | HandleDesyncBranch -2, new CYC=2 | T2 |
| W7-082-T6 | Sub-extraction (from pre-existing) | CalculateExpectedActual -4, new CYC=4 | T1 |
| W7-082-T7 | Sub-extraction (from pre-existing) | CalculateExpectedActual -4, new CYC=4 | T6 |
| W7-082-T8 | Parent refactor + finalization | Parent CYC 90→**6** | T2,T3,T4,T5,T6,T7 |
| W7-082-T9 | Lock-free verification | 0 | T8 |
| W7-082-T10 | CYC compliance verification | 0 | T8 |
| W7-082-T11 | Manifest + deploy-sync | 0 | T10 |

**Total extraction tickets:** 6 (T2-T7)
**Total verification tickets:** 3 (T1, T9, T10)
**Total administrative tickets:** 1 (T11)
**Overall ticket count:** 11

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 2.1 |
| **Execution Time** | 2026-06-29T23:30:00Z |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-082 |
| **Method** | AuditSingleFleetAccount |
| **Original CYC** | 90 |
| **max_cyc_projected** | 8 |
| **parent_cyc_projected** | 6 |
| **extraction_count** | 11 |
| **ticket_count** | 11 |
| **Sequential Thinking Thoughts** | 7 |
| **MCP Tools Used** | resolve_repo, sequentialthinking |
| **Input Artifacts** | 02-architecture-plan.md, 03-audit-report.md |
| **Output** | docs/brain/EPIC-W7-082/04-tickets.md |
| **Status** | completed |
