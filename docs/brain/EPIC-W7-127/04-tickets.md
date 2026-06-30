# EPIC-W7-127 — Phase 4: Ticket Definitions

**Agent Name:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:25:00Z
**Input:** docs/brain/EPIC-W7-127/02-architecture-plan.md + docs/brain/EPIC-W7-127/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Epic ID** | EPIC-W7-127 |
| **Target Method** | `SymmetryGuardOnFollowerFill` |
| **Source File** | `src/V12_002.Symmetry.Follower.cs` |
| **CYC Baseline** | 16 (MCP confirmed: cyclomatic=16, max_nesting=6, lines=72, assessment=high) |
| **CYC Target** | <= 8 (max_cyc_projected=6) |
| **DNA Verdict** | PASS (Phase 3) |
| **Ticket Count** | 4 |
| **Extraction Strategy** | 3 helper extractions + 1 verification ticket |
| **Dependency Chain** | T1 → T2 → T3 → T4 |

---

## MCP Evidence

| Tool | Result |
|---|---|
| `resolve_repo` | repo=antigravityos187-sketch/universal-or-strategy, indexed=true, symbol_count=5147 |
| `get_symbol_complexity` | cyclomatic=16, max_nesting=6, param_count=3, lines=72, assessment="high" |
| `get_extraction_candidates` | candidates=[] (event-dispatched method; no multi-file callers — extraction rationale from Phase 2 stands) |
| `sequentialthinking` | 4 thoughts: complexity drivers confirmed, ticket structure validated, acceptance criteria designed, plan finalized |

---

## Sequential Thinking Summary

- **Thought 1** — CYC=16 confirmed, 3 extraction zones identified, single-file atomic surgery chosen
- **Thought 2** — MCP hard-validated cyclomatic=16/max_nesting=6; empty extraction_candidates consistent with event-dispatch architecture; 3+1 ticket structure determined
- **Thought 3** — Acceptance criteria designed per ticket: CYC targets, JIT attributes, ADR-019 constraints, call-order constraint (Helper 2 before Helper 3)
- **Thought 4** — Final validation: 4 tickets, sequential dependency T1→T2→T3→T4, all constraints mapped, ready to produce

---

## Ticket Definitions

---

### TICKET EPIC-W7-127-T1

| Field | Value |
|---|---|
| **ID** | EPIC-W7-127-T1 |
| **Type** | extraction |
| **Title** | Extract `ValidateAndInitFollowerPos` — Helper 1 (CYC=4) |
| **File** | `src/V12_002.Symmetry.Follower.cs` |
| **Priority** | P1 — must complete before T2 |
| **CYC Target** | 4 |
| **JIT Attribute** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| **Depends On** | None (first ticket in chain) |

**Description:**

Extract the null/flag guard and initialization logic from lines 22–25 of `SymmetryGuardOnFollowerFill` into a new private helper method `ValidateAndInitFollowerPos`. This extraction isolates the hot-path entry guard so its CYC contribution is separated from the dominant Zone B complexity. The cyc budget for this helper is exactly 4.

**Source Lines:** 22–25 (Zone A — null+IsFollower guard + RemainingContracts init)

**Extracted Signature:**

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool ValidateAndInitFollowerPos(PositionInfo followerPos)
```

**Acceptance Criteria:**

- [ ] `ValidateAndInitFollowerPos` exists as a `private` method in `src/V12_002.Symmetry.Follower.cs`
- [ ] Decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] Returns `bool`; returns `false` on null `followerPos`, `!followerPos.IsFollower`, or `RemainingContracts <= 0`
- [ ] Sets `followerPos.EntryFilled = true` unconditionally when validation passes
- [ ] May set `followerPos.RemainingContracts` as needed by original init logic
- [ ] CYC of extracted method = **4** (1 base + 1 null || + 1 !IsFollower + 1 <=0)
- [ ] No `lock()` blocks introduced
- [ ] No Unicode/emoji in any string literal
- [ ] Build passes: `dotnet build src/` zero errors

---

### TICKET EPIC-W7-127-T2

| Field | Value |
|---|---|
| **ID** | EPIC-W7-127-T2 |
| **Type** | extraction |
| **Title** | Extract `TryApplyPreCheckAnchorAndSubmit` — Helper 2 (CYC=6, ADR-019 critical) |
| **File** | `src/V12_002.Symmetry.Follower.cs` |
| **Priority** | P1 — must complete before T3 |
| **CYC Target** | 6 |
| **JIT Attribute** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| **Depends On** | T1 |

**Description:**

Extract the dominant complexity zone (lines 30–70, the `!BracketSubmitted` block) into `TryApplyPreCheckAnchorAndSubmit`. This helper owns the ANCHOR-01 double-map TryGetValue path, the ADR-019 lock-free `AnchorSnapshot` read via `Interlocked.CompareExchange`, the anchor readiness check, and the `shouldSubmitImmediately` fork that calls either `SymmetryGuardSubmitFollowerBracket` or defers with a `Print`. This is the highest-CYC helper at 6, and the hottest path — called on every fill attempt with `!BracketSubmitted`. The cyc budget is 6.

**Source Lines:** 30–70 (Zone B — ANCHOR-01 + AnchorSnapshot + submit/defer logic)

**Extracted Signature:**

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private void TryApplyPreCheckAnchorAndSubmit(
    string fleetEntryName,
    PositionInfo followerPos
)
```

**CRITICAL Constraints:**

- `Interlocked.CompareExchange` call for `AnchorSnapshot` read MUST remain inside this helper (ADR-019 lock-free mandate)
- `shouldSubmitImmediately` local variable MUST be declared and owned internally — not passed from parent
- This helper MUST be called BEFORE `EnqueueAndTryResolveFollower` (T3) in the parent body — temporal state coupling: Helper 2 sets state on `followerPos` that Helper 3 reads

**Acceptance Criteria:**

- [ ] `TryApplyPreCheckAnchorAndSubmit` exists as a `private` method in `src/V12_002.Symmetry.Follower.cs`
- [ ] Decorated with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`
- [ ] Returns `void`
- [ ] Contains `Interlocked.CompareExchange` call for `AnchorSnapshot` read (ADR-019 lock-free)
- [ ] `shouldSubmitImmediately` declared as local variable inside helper (not a parameter)
- [ ] Calls `SymmetryGuardApplyMasterAnchor` and `SymmetryGuardSubmitFollowerBracket` (or defers with `Print`)
- [ ] Performs double-map `TryGetValue` on `symmetryFleetEntryToDispatch` and `symmetryDispatchById` (ANCHOR-01)
- [ ] CYC of extracted method = **6** (1 + 1 TryGetValue1 + 1 TryGetValue2 + 1 anchorReady + 1 preCheckAnchor>0 + 1 shouldSubmit)
- [ ] No `lock()` blocks introduced
- [ ] No Unicode/emoji in any string literal
- [ ] Build passes: `dotnet build src/` zero errors

---

### TICKET EPIC-W7-127-T3

| Field | Value |
|---|---|
| **ID** | EPIC-W7-127-T3 |
| **Type** | extraction |
| **Title** | Extract `EnqueueAndTryResolveFollower` + Simplify Parent Body (CYC=3+3) |
| **File** | `src/V12_002.Symmetry.Follower.cs` |
| **Priority** | P1 |
| **CYC Target** | Helper 3 = 3, Parent = 3 |
| **JIT Attribute** | `[MethodImpl(MethodImplOptions.NoInlining)]` (Helper 3); `[MethodImpl(MethodImplOptions.AggressiveInlining)]` (Parent) |
| **Depends On** | T1, T2 |

**Description:**

Extract the queue construction and resolution path (lines 72–88) into `EnqueueAndTryResolveFollower`. This helper is the cold path — queue mutation side-effects should be isolated from JIT view via `[NoInlining]`. It owns `PendingFollowerFill` construction, the `ConcurrentDictionary` write to `symmetryPendingFollowerFills`, the call to `SymmetryGuardTryResolveFollower`, and the conditional `TryRemove`. Simultaneously, rewrite the parent `SymmetryGuardOnFollowerFill` body to its simplified 3-call form, confirming parent CYC=3.

**Source Lines (Helper 3):** 72–88 (Zone C — PendingFollowerFill + dict write + TryResolve + TryRemove)

**Extracted Signature:**

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void EnqueueAndTryResolveFollower(
    string fleetEntryName,
    PositionInfo followerPos,
    double followerFillPrice
)
```

**Post-Extraction Parent Body:**

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool SymmetryGuardOnFollowerFill(
    string fleetEntryName,
    PositionInfo followerPos,
    double followerFillPrice
)
{
    if (!ValidateAndInitFollowerPos(followerPos))
        return false;

    if (!followerPos.BracketSubmitted)
        TryApplyPreCheckAnchorAndSubmit(fleetEntryName, followerPos);

    EnqueueAndTryResolveFollower(fleetEntryName, followerPos, followerFillPrice);

    return true;
}
```

**CRITICAL Constraint:** Call order — `TryApplyPreCheckAnchorAndSubmit` MUST appear before `EnqueueAndTryResolveFollower` in parent body. Swapping would break temporal state: Helper 2 writes state that Helper 3 reads.

**Acceptance Criteria:**

- [ ] `EnqueueAndTryResolveFollower` exists as a `private` method in `src/V12_002.Symmetry.Follower.cs`
- [ ] Decorated with `[MethodImpl(MethodImplOptions.NoInlining)]`
- [ ] Returns `void`
- [ ] Constructs `PendingFollowerFill` with ternary (preserving original conditional logic)
- [ ] Writes to `symmetryPendingFollowerFills` `ConcurrentDictionary` (lock-free write preserved)
- [ ] Calls `SymmetryGuardTryResolveFollower`
- [ ] Performs conditional `TryRemove` on `symmetryPendingFollowerFills`
- [ ] CYC of Helper 3 = **3** (1 + 1 ternary + 1 TryResolve if)
- [ ] `SymmetryGuardOnFollowerFill` parent body matches pseudocode above
- [ ] `SymmetryGuardOnFollowerFill` CYC = **3** (1 + 1 ValidateAndInit check + 1 !BracketSubmitted)
- [ ] Call order in parent: Helper 2 invoked before Helper 3
- [ ] No `lock()` blocks introduced
- [ ] No Unicode/emoji in any string literal
- [ ] Caller `HandleFleetEntryFill` signature unchanged — no call site modification required
- [ ] Build passes: `dotnet build src/` zero errors

---

### TICKET EPIC-W7-127-T4

| Field | Value |
|---|---|
| **ID** | EPIC-W7-127-T4 |
| **Type** | verification |
| **Title** | Verify Full Extraction — CYC Regression Gate + xUnit Smoke Test |
| **File** | `src/V12_002.Symmetry.Follower.cs` |
| **Priority** | P2 — post-extraction gate |
| **CYC Target** | max_cyc_all <= 8, parent <= 3 |
| **JIT Attribute** | N/A |
| **Depends On** | T1, T2, T3 (all extraction tickets complete) |

**Description:**

Post-extraction verification ticket. Runs the complexity audit, build check, and xUnit smoke test to confirm the full cyc reduction from 16 → 3 (parent) is intact with max_cyc_projected=6. Also confirms no regression in the calling path (`HandleFleetEntryFill` → `SymmetryGuardOnFollowerFill`) and that ADR-019 lock-free invariants are preserved.

**Acceptance Criteria:**

- [ ] `python scripts/complexity_audit.py` reports `SymmetryGuardOnFollowerFill` CYC = **3**
- [ ] `python scripts/complexity_audit.py` reports `ValidateAndInitFollowerPos` CYC = **4**
- [ ] `python scripts/complexity_audit.py` reports `TryApplyPreCheckAnchorAndSubmit` CYC = **6**
- [ ] `python scripts/complexity_audit.py` reports `EnqueueAndTryResolveFollower` CYC = **3**
- [ ] max_cyc_projected across all 4 symbols = **6** (all <= 8, Jane Street strict standard met)
- [ ] `grep -r "lock(" src/V12_002.Symmetry.Follower.cs` = zero results
- [ ] `dotnet build src/` produces zero errors and zero new warnings
- [ ] xUnit `[Fact]` test exercises `SymmetryGuardOnFollowerFill` via a fill-path integration: (a) null followerPos returns false, (b) IsFollower=false returns false, (c) valid followerPos with BracketSubmitted=false triggers anchor path, (d) valid followerPos with BracketSubmitted=true skips anchor path
- [ ] `powershell -File .\deploy-sync.ps1` executes successfully (NinjaTrader hard-link sync)
- [ ] CYC reduction confirmed: 16 (baseline) → 3 (parent) = **-13 points**

---

## Extraction CYC Summary

| Symbol | Type | CYC Baseline | CYC Target | JIT Attribute |
|---|---|---|---|---|
| `SymmetryGuardOnFollowerFill` | parent | 16 | **3** | `AggressiveInlining` |
| `ValidateAndInitFollowerPos` | extraction | N/A (new) | **4** | `AggressiveInlining` |
| `TryApplyPreCheckAnchorAndSubmit` | extraction | N/A (new) | **6** | `AggressiveInlining` |
| `EnqueueAndTryResolveFollower` | extraction | N/A (new) | **3** | `NoInlining` |
| **max_cyc_projected** | — | — | **6** | — |

---

## Risk Register (Phase 4 Additions)

| Risk | Severity | Mitigation |
|---|---|---|
| Call order violation (Helper 3 before Helper 2) | High | T3 acceptance criterion explicitly mandates Helper 2 before Helper 3 in parent body |
| `Interlocked.CompareExchange` moved out of Helper 2 | High | T2 acceptance criterion requires `Interlocked.CompareExchange` stays inside `TryApplyPreCheckAnchorAndSubmit` |
| `shouldSubmitImmediately` passed as parameter instead of local | Medium | T2 acceptance criterion requires it be owned internally |
| Partial extraction causing intermediate build failure | Medium | Tickets T1, T2, T3 each end with build-passes criterion; sequential dependency enforced |

---

## Files Touched

| File | Change Type | Tickets |
|---|---|---|
| `src/V12_002.Symmetry.Follower.cs` | Modify — extract 3 private helpers, simplify parent body | T1, T2, T3 |

No other files modified. No interface changes. No public API changes. V12.23 No Scope Creep: PASS.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Phase** | 4 |
| **Wave** | 7 |
| **Lane** | P4-L8 |
| **Bobcoins Used** | 1.5 |
| **MCP Tools Used** | resolve_repo, get_symbol_complexity, get_extraction_candidates, sequentialthinking (x4) |
| **Sequential Thinking Steps** | 4 (complexity confirmed, ticket structure, acceptance criteria, final validation) |
| **Ticket Count** | 4 (T1=extraction, T2=extraction, T3=extraction, T4=verification) |
| **CYC Baseline** | 16 (MCP confirmed: cyclomatic=16, max_nesting=6, lines=72) |
| **max_cyc_projected** | 6 |
| **parent_cyc_projected** | 3 |
| **CYC Reduction** | 16 → 3 (parent), -13 points |
| **DNA Verdict** | PASS (from Phase 3) |
| **Output Path** | docs/brain/EPIC-W7-127/04-tickets.md |
