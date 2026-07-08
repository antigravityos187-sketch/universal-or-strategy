# EPIC-W7-121 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:30:00Z
**Input artifacts:** 02-architecture-plan.md · 03-audit-report.md

---

## Summary

Surgical extraction of `SymmetryGuardCascadeFollowerCleanup` (CYC=10) into 3 private helper
methods in `src/V12_002.Symmetry.Replace.cs`. All extraction tickets operate within the same
partial class — zero cross-file changes. DNA verdict: **PASS**.

| Metric | Value |
|--------|-------|
| Original CYC | 10 |
| max_cyc_projected | 7 |
| Extraction count | 3 |
| Ticket count | 4 |
| Lane | P4-L8 |

---

## MCP Evidence (Phase 4)

### jCodemunch: resolve_repo
- **Repo:** `antigravityos187-sketch/universal-or-strategy`
- **Status:** loadable, indexed
- **Symbol count:** 5147 | **File count:** 2000

### jCodemunch: get_symbol_complexity
- **Query:** `SymmetryGuardCascadeFollowerCleanup`
- **Result:** Symbol not found in index (private method, same-partial-class caller — consistent
  with Phase 2 MCP evidence: "Extraction driven by CYC reduction mandate, not caller demand")
- **Baseline authority:** Phase 2 `get_context_bundle` confirmed source body at line 198;
  branch inventory documents CYC=10 exhaustively (9 branches + base)

### jCodemunch: get_extraction_candidates
- **File:** `src/V12_002.Symmetry.Replace.cs`
- **Result:** `candidates: []` (0 callers detected across file boundaries — expected for private method)
- **Interpretation:** Consistent with Phase 2 finding; extraction proceeds on CYC mandate

### Sequential Thinking (3 thoughts)
1. Mapped sub-concerns → 4-ticket breakdown (T1 context resolver, T2 logger, T3 follower cancel, T4 integration+test)
2. Validated empty MCP returns against Phase 2 documentation; confirmed CYC=10 baseline stands
3. Final verification: all 4 ticket acceptance criteria satisfy CYC targets, dependency chain is correct (T1+T2+T3 parallel → T4)

---

## Ticket Index

| Ticket ID | Type | Method | CYC Target | Depends On |
|-----------|------|--------|-----------|------------|
| TICKET-W7-121-001 | extraction | `TryResolveSymmetryCascadeContext` | 3 | — |
| TICKET-W7-121-002 | extraction | `LogCascadeCancellationStart` | 1 | — |
| TICKET-W7-121-003 | extraction | `TryCancelFollowerEntry` | 7 | — |
| TICKET-W7-121-004 | integration + test | `SymmetryGuardCascadeFollowerCleanup` (parent) | 3 | 001, 002, 003 |

---

## TICKET-W7-121-001

**ID:** TICKET-W7-121-001
**Type:** extraction
**Priority:** P1
**Epic:** EPIC-W7-121
**Wave:** 7
**Lane:** P4-L8

### Method to Extract

```
TryResolveSymmetryCascadeContext(string masterEntryName, out SymmetryDispatchContext ctx)
  → returns bool
```

### File

`src/V12_002.Symmetry.Replace.cs` — same partial class `V12_002`, no cross-file changes

### Description

Extract the two-hop dispatch lookup chain from `SymmetryGuardCascadeFollowerCleanup` into a new
private helper. The helper performs `masterEntryName → dispatchId → SymmetryDispatchContext`
resolution using two `ConcurrentDictionary.TryGetValue` calls (lock-free per ADR-019). Returns
`false` on any miss, allowing the parent to early-return with a single guard.

### Implementation

```csharp
private bool TryResolveSymmetryCascadeContext(
    string masterEntryName,
    out SymmetryDispatchContext ctx)
{
    ctx = default;
    if (!symmetryMasterEntryToDispatch.TryGetValue(masterEntryName, out var dispatchId))
        return false;
    if (!symmetryDispatchById.TryGetValue(dispatchId, out ctx))
        return false;
    return true;
}
```

### Acceptance Criteria

- [ ] Method `TryResolveSymmetryCascadeContext` exists in `src/V12_002.Symmetry.Replace.cs`
- [ ] Signature: `private bool TryResolveSymmetryCascadeContext(string masterEntryName, out SymmetryDispatchContext ctx)`
- [ ] CYC target: **3** (base 1 + 2 if-return branches)
- [ ] Zero `lock()` blocks — both `TryGetValue` calls use `ConcurrentDictionary` (lock-free, ADR-019)
- [ ] No new heap allocations introduced (out-param, stack-only)
- [ ] Build passes with zero errors after change

### CYC Breakdown

| Branch | Type | Contribution |
|--------|------|-------------|
| base | — | +1 |
| `if (!TryGetValue(masterEntryName...))` | if-return | +1 |
| `if (!TryGetValue(dispatchId...))` | if-return | +1 |
| **Total** | | **3** |

### DNA Guardrails

- Lock-free: `ConcurrentDictionary.TryGetValue` — no new synchronization (ADR-019)
- ASCII-only: all identifiers and comments ASCII
- Scope: single file, single partial class

---

## TICKET-W7-121-002

**ID:** TICKET-W7-121-002
**Type:** extraction
**Priority:** P1
**Epic:** EPIC-W7-121
**Wave:** 7
**Lane:** P4-L8

### Method to Extract

```
LogCascadeCancellationStart(string masterEntryName, int followerCount)
  → void, [MethodImpl(NoInlining)]
```

### File

`src/V12_002.Symmetry.Replace.cs` — same partial class `V12_002`, no cross-file changes

### Description

Extract the cold-path cascade-start diagnostic log into a standalone private helper annotated
`[MethodImpl(MethodImplOptions.NoInlining)]` per the carl_cook JIT-out-of-line pattern. The JIT
will never inline this string formatting overhead into the hot dispatch loop. Pure side-effect —
no state mutation, no new heap allocation beyond the transient format string.

### Implementation

```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
private void LogCascadeCancellationStart(string masterEntryName, int followerCount)
{
    Print(string.Format(
        "[CASCADE] Master {0} cancelled -- terminating {1} linked follower(s).",
        masterEntryName,
        followerCount));
}
```

### Acceptance Criteria

- [ ] Method `LogCascadeCancellationStart` exists in `src/V12_002.Symmetry.Replace.cs`
- [ ] Signature: `private void LogCascadeCancellationStart(string masterEntryName, int followerCount)`
- [ ] `[MethodImpl(MethodImplOptions.NoInlining)]` attribute present (carl_cook cold-path pattern)
- [ ] CYC target: **1** (no branches — pure Print call)
- [ ] Print template is ASCII-only: `[CASCADE] Master {0} cancelled -- terminating {1} linked follower(s).`
- [ ] No Unicode, emoji, or curly quotes in string literal
- [ ] Build passes with zero errors after change

### CYC Breakdown

| Branch | Type | Contribution |
|--------|------|-------------|
| base | — | +1 |
| **Total** | | **1** |

### DNA Guardrails

- ASCII-only: print template verified ASCII in Phase 3 audit
- carl_cook cold-path: `[NoInlining]` keeps format string off hot path
- Scope: single file, single partial class

---

## TICKET-W7-121-003

**ID:** TICKET-W7-121-003
**Type:** extraction
**Priority:** P1
**Epic:** EPIC-W7-121
**Wave:** 7
**Lane:** P4-L8

### Method to Extract

```
TryCancelFollowerEntry(string followerName)
  → void
```

### File

`src/V12_002.Symmetry.Replace.cs` — same partial class `V12_002`, no cross-file changes

### Description

Extract the entire per-follower cancellation body from the `foreach` loop in
`SymmetryGuardCascadeFollowerCleanup` into a private helper. Responsibility: "cancel one
eligible follower entry." Contains all per-follower complexity — guard chain (2× TryGetValue +
null check), compound-OR OrderState eligibility test, conditional cancel invocation, and account
name log. Preserves full defense-in-depth (no guard is removed or weakened).

The inline audit comment `// DeltaExpectedPositionLocked deferred to OnAccountOrderUpdate
confirmed-cancel to prevent REAPER desync` must remain inside this method body.

### Implementation

```csharp
private void TryCancelFollowerEntry(string followerName)
{
    if (!activePositions.TryGetValue(followerName, out var pos))
        return;
    if (!entryOrders.TryGetValue(followerName, out var order))
        return;
    if (order == null)
        return;
    // DeltaExpectedPositionLocked deferred to OnAccountOrderUpdate
    // confirmed-cancel to prevent REAPER desync
    if (order.OrderState == OrderState.Working
        || order.OrderState == OrderState.Submitted
        || order.OrderState == OrderState.Accepted)
    {
        string accountName = pos.ExecutingAccount != null
            ? pos.ExecutingAccount.Name
            : "Master";
        CancelOrderSafe(order, accountName);
        Print(string.Format(
            "[CASCADE] Cancelling follower entry {0} on account {1}.",
            followerName,
            accountName));
    }
}
```

### Acceptance Criteria

- [ ] Method `TryCancelFollowerEntry` exists in `src/V12_002.Symmetry.Replace.cs`
- [ ] Signature: `private void TryCancelFollowerEntry(string followerName)`
- [ ] CYC target: **7** (base 1 + 3 guard-returns + 2 OR-condition branches + 1 ternary)
- [ ] All 3 guard-return branches preserved: `!activePositions.TryGetValue`, `!entryOrders.TryGetValue`, `order == null`
- [ ] Compound-OR eligibility: `OrderState.Working || OrderState.Submitted || OrderState.Accepted`
- [ ] Ternary for account name: `pos.ExecutingAccount != null ? pos.ExecutingAccount.Name : "Master"`
- [ ] Inline audit comment for REAPER desync present in method body
- [ ] Zero `lock()` blocks — uses `ConcurrentDictionary.TryGetValue` (lock-free, ADR-019)
- [ ] Build passes with zero errors after change

### CYC Breakdown

| Branch | Type | Contribution |
|--------|------|-------------|
| base | — | +1 |
| `if (!activePositions.TryGetValue(...))` | guard-return | +1 |
| `if (!entryOrders.TryGetValue(...))` | guard-return | +1 |
| `if (order == null)` | guard-return | +1 |
| `... == OrderState.Working \|\| ... == OrderState.Submitted` | compound OR | +1 |
| `\|\| ... == OrderState.Accepted` | compound OR continuation | +1 |
| `pos.ExecutingAccount != null ? ... : "Master"` | ternary | +1 |
| **Total** | | **7** |

### DNA Guardrails

- Lock-free: `ConcurrentDictionary.TryGetValue` — no new synchronization (ADR-019)
- Defense in depth: all guards preserved; no eligibility condition weakened
- trading_billions: single responsibility — cancel one follower entry

---

## TICKET-W7-121-004

**ID:** TICKET-W7-121-004
**Type:** integration + test
**Priority:** P1
**Epic:** EPIC-W7-121
**Wave:** 7
**Lane:** P4-L8
**Depends On:** TICKET-W7-121-001, TICKET-W7-121-002, TICKET-W7-121-003

### Target Method

```
SymmetryGuardCascadeFollowerCleanup(string masterEntryName)
  → void (parent, rewired)
```

### File

`src/V12_002.Symmetry.Replace.cs` — same partial class `V12_002`, no cross-file changes

### Description

Rewire the parent `SymmetryGuardCascadeFollowerCleanup` to delegate to the 3 extracted helpers
(Tickets 001–003). Replace the inlined branches with the helper calls per the architecture plan.
Then add an xUnit `[Fact]` test verifying the extracted CYC reduction. Final state: parent
CYC=3, max_cyc_projected=7, build clean.

### Implementation (parent body after extraction)

```csharp
private void SymmetryGuardCascadeFollowerCleanup(string masterEntryName)
{
    if (!TryResolveSymmetryCascadeContext(masterEntryName, out var ctx))
        return;

    string[] followers = ctx.Followers; // ADR-019: immutable snapshot, lock-free

    LogCascadeCancellationStart(masterEntryName, followers.Length);

    foreach (string followerName in followers)
        TryCancelFollowerEntry(followerName);
}
```

### Acceptance Criteria

- [ ] `SymmetryGuardCascadeFollowerCleanup` body matches the 4-statement form above exactly
- [ ] Parent CYC target: **3** (base 1 + if-TryResolve guard + foreach)
- [ ] All 3 helper methods present and callable within same partial class
- [ ] max_cyc_projected = **7** (TryCancelFollowerEntry) — no helper exceeds 7
- [ ] `ctx.Followers` accessed as immutable snapshot (ADR-019 comment preserved)
- [ ] xUnit `[Fact]` test added in `tests/` verifying the 3 extracted helpers compile and are callable
- [ ] Test uses `Assert.Equal()` — never NUnit / MSTest
- [ ] `dotnet build` passes with zero errors
- [ ] `dotnet csharpier check src/` passes with zero formatting issues
- [ ] No `lock()` blocks introduced anywhere in the 4-method set

### CYC Breakdown (parent after extraction)

| Branch | Type | Contribution |
|--------|------|-------------|
| base | — | +1 |
| `if (!TryResolveSymmetryCascadeContext(...))` | if-return | +1 |
| `foreach (string followerName in followers)` | loop | +1 |
| **Total** | | **3** |

### DNA Guardrails

- Lock-free: `ctx.Followers` is immutable `string[]` snapshot (ADR-019); no new locking
- ASCII-only: all identifiers and comments ASCII
- CYC mandate: max_cyc_projected=7 <= 8 (Jane Street strict standard)
- xUnit only: `[Fact]` + `Assert.Equal()` per TEST_FRAMEWORK_PROTOCOL.md

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-121 |
| **Method** | SymmetryGuardCascadeFollowerCleanup |
| **Original CYC** | 10 |
| **max_cyc_projected** | 7 |
| **Extraction count** | 3 |
| **Ticket count** | 4 |
| **DNA Verdict (Phase 3)** | PASS |
| **MCP: get_symbol_complexity** | not found in index (private method — expected, see Phase 2) |
| **MCP: get_extraction_candidates** | 0 candidates (private method — expected, see Phase 2) |
| **Sequential Thinking** | 3 thoughts completed |
