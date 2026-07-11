# EPIC-W7-129 — Phase 4: Ticket Definitions

**Agent Name:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Input Artifacts:** `02-architecture-plan.md`, `03-audit-report.md`
**DNA Verdict:** PASS (from Phase 3)

---

## Target Method

| Field | Value |
|---|---|
| **Method** | `SymmetryGuardTryResolveFollower` |
| **File** | `src/V12_002.Symmetry.Follower.cs` |
| **Lines** | 129–246 |
| **CYC Baseline** | 16 |
| **CYC Target (parent)** | ≤ 8 |
| **Extractions Planned** | 2 helpers |
| **max_cyc_projected** | 8 |

---

## MCP Evidence (Phase 4 Probe)

| Tool | Result |
|---|---|
| `resolve_repo` | LIVE — 5,147 symbols, indexed 2026-06-29T01:05:21Z |
| `get_symbol_complexity(SymmetryGuardTryResolveFollower)` | Not found in index (partial-class C# limitation — expected). CYC=16 sourced from Phase 2 architecture plan (validated by Phase 3 audit). |
| `get_extraction_candidates(src/V12_002.Symmetry.Follower.cs)` | Empty (complexity data not populated for partial-class file). Architecture plan CYC data used as authoritative source per Phase 3 PASS verdict. |

**Authoritative CYC source:** Phase 2 architecture plan + Phase 3 audit confirmation.
- CYC baseline: **16**
- Helper 1 (`SymmetryGuardResolveDispatchContext`) extraction CYC: **5**
- Helper 2 (`SymmetryGuardEvaluateSlippage`) extraction CYC: **5**
- Parent post-extraction CYC: **8**

---

## Sequential Thinking Summary

| Thought | Focus | Conclusion |
|---|---|---|
| 1 (probe) | Initialization | MCP live. Architecture plan + audit loaded. 2-helper extraction plan confirmed. |
| 2 (MCP results) | Complexity tool response | Index partial-class limitation; Phase 2/3 CYC data authoritative. 3 impl + 1 verify = 4 tickets. |
| 3 (acceptance criteria) | Per-ticket criteria design | T-1: helper 1 extraction, CYC=5. T-2: helper 2 extraction, CYC=5. T-3: parent refactor + out-param forwarding, CYC=8. T-4: build+verify+sync. |
| 4 (final validation) | Structure review | 4 tickets, strictly sequential. All Jane Street constraints mapped. Output valid. |

---

## Ticket Summary

| Ticket ID | Type | Description | CYC Target | File |
|---|---|---|---|---|
| `EPIC-W7-129-T1` | extraction | Add `SymmetryGuardResolveDispatchContext` helper | ≤ 5 | `src/V12_002.Symmetry.Follower.cs` |
| `EPIC-W7-129-T2` | extraction | Add `SymmetryGuardEvaluateSlippage` helper | ≤ 5 | `src/V12_002.Symmetry.Follower.cs` |
| `EPIC-W7-129-T3` | refactor | Refactor parent to call both helpers; forward out params to Print | ≤ 8 | `src/V12_002.Symmetry.Follower.cs` |
| `EPIC-W7-129-T4` | verification | Build, format, complexity audit, deploy-sync | ≤ 8 (all) | `src/V12_002.Symmetry.Follower.cs` |

**Execution order:** T1 → T2 → T3 → T4 (strictly sequential; each ticket depends on the previous completing successfully).

---

## Ticket Definitions

---

### EPIC-W7-129-T1

**ID:** `EPIC-W7-129-T1`
**Type:** `extraction`
**File:** `src/V12_002.Symmetry.Follower.cs`
**Depends On:** Phase 3 DNA audit PASS (already confirmed)
**CYC Target:** ≤ 5

#### Description

Add the `SymmetryGuardResolveDispatchContext` private helper method to the partial class in
[`src/V12_002.Symmetry.Follower.cs`](src/V12_002.Symmetry.Follower.cs). This is extraction
ticket 1 of 2. Extract the tri-clause ConcurrentDictionary dispatch lookup guard (lines 135–157 of
`SymmetryGuardTryResolveFollower`) into a standalone `[AggressiveInlining]` helper.

#### Signature (exact)

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool SymmetryGuardResolveDispatchContext(
    string fleetEntryName,
    PendingFollowerFill pending,
    DateTime nowUtc,
    out SymmetryDispatchContext ctx,
    out bool timedOut
)
```

#### Extracted Logic (lines 135–157 of parent)

- Tri-OR guard: `!symmetryFleetEntryToDispatch.TryGetValue(...)` `||` `!symmetryDispatchById.TryGetValue(...)` `||` `ctx == null`
- Timeout-skip on missing context: `if (nowUtc - pending.QueuedUtc >= SymmetryAnchorWait)` → call `SymmetryGuardSkipFollower`, set `timedOut = true`
- Returns `true` when context resolved; `false` when caller should wait/skip

#### CYC Budget

| Branch | +CYC |
|---|---|
| Base | 1 |
| Tri-OR (`\|\|` × 2) | +3 |
| Timeout `if` | +1 |
| **Total** | **5** |

#### Acceptance Criteria

- [ ] Method `SymmetryGuardResolveDispatchContext` exists in `src/V12_002.Symmetry.Follower.cs` (same partial class as parent)
- [ ] Signature matches architecture plan exactly (5 parameters; `out SymmetryDispatchContext ctx`; `out bool timedOut`)
- [ ] `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute applied
- [ ] Zero `lock()` blocks in helper body — only `ConcurrentDictionary.TryGetValue` calls (ADR-019 lock-free contract)
- [ ] All string literals are ASCII-only (`"Missing dispatch context"`)
- [ ] No LINQ used in helper body
- [ ] cyc (cyclomatic complexity) of helper ≤ 5 as verified by `python scripts/complexity_audit.py`
- [ ] `dotnet build src/` exits with zero errors after this ticket
- [ ] Parent method `SymmetryGuardTryResolveFollower` is **not yet modified** in this ticket (extraction only — parent modification is T3)

---

### EPIC-W7-129-T2

**ID:** `EPIC-W7-129-T2`
**Type:** `extraction`
**File:** `src/V12_002.Symmetry.Follower.cs`
**Depends On:** `EPIC-W7-129-T1` completed
**CYC Target:** ≤ 5

#### Description

Add the `SymmetryGuardEvaluateSlippage` private helper method to the partial class in
[`src/V12_002.Symmetry.Follower.cs`](src/V12_002.Symmetry.Follower.cs). This is extraction
ticket 2 of 2. Extract the dual-ternary slippage initializers and OR breach predicate (lines 183–200 of
`SymmetryGuardTryResolveFollower`) into a standalone `[AggressiveInlining]` helper with zero-alloc
`out` parameters.

#### Signature (exact)

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool SymmetryGuardEvaluateSlippage(
    string fleetEntryName,
    PositionInfo pos,
    PendingFollowerFill pending,
    double masterAnchor,
    out double slippageTicks,
    out double slippageUsdPerContract
)
```

#### Extracted Logic (lines 183–200 of parent)

- Slippage magnitude: `double slippagePoints = Math.Abs(pending.FleetFillPrice - masterAnchor)`
- Tick slippage ternary: `slippageTicks = tickSize > 0 ? slippagePoints / tickSize : 0.0`
- USD slippage ternary: `slippageUsdPerContract = pointValue > 0 ? slippagePoints * pointValue : 0.0`
- Dual-threshold OR breach predicate: `slippageTicks > SymmetryMaxSlippageTicks || slippageUsdPerContract > SymmetryMaxSlippageUsdPerContract`
- Breach path: calls `SymmetryGuardSkipFollower` with formatted ASCII reason string, returns `false`
- Clean path: returns `true`

#### CYC Budget

| Branch | +CYC |
|---|---|
| Base | 1 |
| `tickSize > 0 ?` ternary | +1 |
| `pointValue > 0 ?` ternary | +1 |
| `\|\|` in breach predicate | +1 |
| `if (breach)` | +1 |
| **Total** | **5** |

#### Acceptance Criteria

- [ ] Method `SymmetryGuardEvaluateSlippage` exists in `src/V12_002.Symmetry.Follower.cs` (same partial class)
- [ ] Signature matches architecture plan exactly (6 parameters; `out double slippageTicks`; `out double slippageUsdPerContract`)
- [ ] `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute applied
- [ ] Zero `lock()` blocks in helper body — pure arithmetic + `Math.Abs` (ADR-019 lock-free contract)
- [ ] No LINQ used in helper body
- [ ] All string literals ASCII-only (`"Slippage Buffer breach vs Master {0:F2}"`)
- [ ] No heap allocation in helper body — `out double` params, no boxing
- [ ] cyc (cyclomatic complexity) of helper ≤ 5 as verified by `python scripts/complexity_audit.py`
- [ ] `dotnet build src/` exits with zero errors after this ticket
- [ ] Parent method `SymmetryGuardTryResolveFollower` is **not yet modified** in this ticket (extraction only — parent modification is T3)

---

### EPIC-W7-129-T3

**ID:** `EPIC-W7-129-T3`
**Type:** `refactor`
**File:** `src/V12_002.Symmetry.Follower.cs`
**Depends On:** `EPIC-W7-129-T1` and `EPIC-W7-129-T2` both completed
**CYC Target:** ≤ 8

#### Description

Refactor `SymmetryGuardTryResolveFollower` (lines 129–246 of
[`src/V12_002.Symmetry.Follower.cs`](src/V12_002.Symmetry.Follower.cs)) to delegate its two
primary complexity drivers to the extracted helpers. This is the parent refactor ticket that completes
the cyc reduction from 16 → 8. The parent signature must remain unchanged (no caller modifications
required). The `out` params `slippageTicks` and `slippageUsdPerContract` produced by
`SymmetryGuardEvaluateSlippage` must be forwarded to the existing `Print` call at method end.

#### Changes Required in Parent

1. Replace the tri-clause dispatch lookup block (lines 135–157) with a single call to
   `SymmetryGuardResolveDispatchContext(fleetEntryName, pending, nowUtc, out var ctx, out var timedOut)`.
2. Replace the dual-ternary slippage block + breach predicate (lines 183–200) with a single call to
   `SymmetryGuardEvaluateSlippage(fleetEntryName, pos, pending, masterAnchor, out var slippageTicks, out var slippageUsdPerContract)`.
3. Ensure the existing `Print(...)` call at method end receives `slippageTicks` and
   `slippageUsdPerContract` — these must be declared as `var` from the helper call above and forwarded
   without re-computation.
4. Retain the bracket routing fork (lines 208–233) inline: `if (pos.BracketSubmitted)`,
   `alreadyAnchored` compound `&&` check, `if (alreadyAnchored)` — these 4 CYC branches are retained
   to reach parent total = 8.

#### Post-Extraction Parent CYC Budget

| Branch | +CYC |
|---|---|
| Base | 1 |
| `if (!SymmetryGuardResolveDispatchContext(...))` | +1 |
| `if (!isResolved)` | +1 |
| `if (nowUtc - pending.QueuedUtc >= SymmetryAnchorWait)` | +1 |
| `if (!SymmetryGuardEvaluateSlippage(...))` | +1 |
| `if (pos.BracketSubmitted)` | +1 |
| `&&` in `alreadyAnchored` compound | +1 |
| `if (alreadyAnchored)` | +1 |
| **Parent total** | **8 ≤ 8 ✓** |

#### Callers — No Modification Required

| Caller | File | Line | Impact |
|---|---|---|---|
| `SymmetryGuardOnFollowerFill` | `src/V12_002.Symmetry.Follower.cs` | 17 | None — parent signature unchanged |
| `SymmetryGuardProcessPendingFollowerFills` | `src/V12_002.Symmetry.Follower.cs` | 97 | None — parent signature unchanged |
| `SymmetryGuardTryResolveFollowersForDispatch` | `src/V12_002.Symmetry.Replace.cs` | 187 | None — parent signature unchanged |

#### Acceptance Criteria

- [ ] `SymmetryGuardTryResolveFollower` body calls `SymmetryGuardResolveDispatchContext` for context lookup
- [ ] `SymmetryGuardTryResolveFollower` body calls `SymmetryGuardEvaluateSlippage` for slippage evaluation
- [ ] `out` params `slippageTicks` and `slippageUsdPerContract` from `SymmetryGuardEvaluateSlippage` are forwarded to the existing `Print` call at method end (no re-computation of slippage in parent)
- [ ] Parent method signature `private bool SymmetryGuardTryResolveFollower(string fleetEntryName, PositionInfo pos, PendingFollowerFill pending, DateTime nowUtc)` is **unchanged**
- [ ] All 3 callers (`SymmetryGuardOnFollowerFill`, `SymmetryGuardProcessPendingFollowerFills`, `SymmetryGuardTryResolveFollowersForDispatch`) remain unmodified
- [ ] Zero `lock()` blocks introduced
- [ ] cyc (cyclomatic complexity) of parent ≤ 8 as verified by `python scripts/complexity_audit.py`
- [ ] `dotnet build src/` exits with zero errors after this ticket
- [ ] V12.23 No Scope Creep: no changes outside `src/V12_002.Symmetry.Follower.cs` in this ticket

---

### EPIC-W7-129-T4

**ID:** `EPIC-W7-129-T4`
**Type:** `verification`
**File:** `src/V12_002.Symmetry.Follower.cs`
**Depends On:** `EPIC-W7-129-T3` completed
**CYC Target:** ≤ 8 (all three methods)

#### Description

Final build, format, complexity audit, and NinjaTrader hard-link sync for EPIC-W7-129. This
verification ticket closes the extraction epic for `SymmetryGuardTryResolveFollower` (cyc reduction
from 16 to 8). All quality gates must pass before this ticket is marked complete.

#### Steps

1. `dotnet csharpier format src/` — enforce braces and line endings
2. `dotnet csharpier check src/` — verify zero formatting issues
3. `dotnet build src/` — verify zero compilation errors
4. `python scripts/complexity_audit.py` — confirm cyc ≤ 8 for all three methods:
   - `SymmetryGuardResolveDispatchContext` → target ≤ 5
   - `SymmetryGuardEvaluateSlippage` → target ≤ 5
   - `SymmetryGuardTryResolveFollower` (parent) → target ≤ 8
5. `powershell -File .\scripts\build_readiness.ps1` — all 13 local quality gates
6. `powershell -File .\deploy-sync.ps1` — re-sync NinjaTrader hard links after src/ edit

#### Acceptance Criteria

- [ ] `dotnet csharpier check src/` exits with zero issues
- [ ] `dotnet build src/` exits with zero errors and zero warnings in `src/V12_002.Symmetry.Follower.cs`
- [ ] `python scripts/complexity_audit.py` reports:
  - `SymmetryGuardResolveDispatchContext` cyc ≤ 5 ✓
  - `SymmetryGuardEvaluateSlippage` cyc ≤ 5 ✓
  - `SymmetryGuardTryResolveFollower` cyc ≤ 8 ✓ (cyc baseline was 16; cyc reduction = 8 points)
- [ ] `powershell -File .\scripts\build_readiness.ps1` passes all blocking gates (checks 1–5, 8–9)
- [ ] `powershell -File .\deploy-sync.ps1` completes without DIFF GUARD failure
- [ ] All three callers compile without modification: `SymmetryGuardOnFollowerFill`, `SymmetryGuardProcessPendingFollowerFills`, `SymmetryGuardTryResolveFollowersForDispatch`
- [ ] Zero new `lock()` blocks introduced in any modified file (forensic scan: `grep -r "lock(" src/`)
- [ ] ASCII-only strings confirmed in both helpers (no Unicode/emoji/curly quotes)
- [ ] Ticket completion report written to `docs/brain/EPIC-W7-129/ticket-4-completion.md`

---

## Jane Street Compliance Map

| Rule (Source) | T1 | T2 | T3 | T4 |
|---|---|---|---|---|
| `carl_cook`: `AggressiveInlining` on hot-path helpers | ✓ Required | ✓ Required | N/A | ✓ Verified |
| `carl_cook`: zero-alloc `out` params, no boxing | ✓ Required | ✓ Required | ✓ Forwarded | ✓ Verified |
| `carl_cook`: no LINQ | ✓ Enforced | ✓ Enforced | ✓ Enforced | ✓ Audited |
| `gjengset`: no `lock()` blocks | ✓ Enforced | ✓ Enforced | ✓ Enforced | ✓ Forensic scan |
| `gjengset`: `ConcurrentDictionary` lock-free (ADR-019) | ✓ Required | N/A | ✓ Preserved | ✓ Verified |
| `trading_billions`: single responsibility per helper | ✓ Context only | ✓ Slippage only | N/A | ✓ Audited |
| `trading_billions`: cyc ≤ 8 per method | ≤ 5 | ≤ 5 | ≤ 8 | ✓ All verified |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-129 |
| **Lane** | P4-L8 |
| **Input Artifacts** | `02-architecture-plan.md`, `03-audit-report.md` |
| **Output Artifact** | `04-tickets.md` |
| **MCP Tools Used** | `resolve_repo`, `get_symbol_complexity`, `get_extraction_candidates`, `sequentialthinking` (4 thoughts) |
| **Sequential Thinking Steps** | 4 (probe + MCP results + acceptance criteria design + final validation) |
| **Ticket Count** | 4 |
| **Extraction Tickets** | 2 (`EPIC-W7-129-T1`, `EPIC-W7-129-T2`) |
| **Refactor Tickets** | 1 (`EPIC-W7-129-T3`) |
| **Verification Tickets** | 1 (`EPIC-W7-129-T4`) |
| **CYC Baseline** | 16 |
| **CYC Target (parent)** | ≤ 8 |
| **max_cyc_projected** | 8 |
| **DNA Verdict** | PASS |
| **Bobcoins Used** | ~1.2 |
