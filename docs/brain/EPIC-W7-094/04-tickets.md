# EPIC-W7-094 — Phase 4: Implementation Tickets
# ExecuteMultiAccountMarket — Surgical Extraction

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Input:** docs/brain/EPIC-W7-094/02-architecture-plan.md + docs/brain/EPIC-W7-094/03-audit-report.md
**DNA Verdict (Phase 3):** PASS — violations: []

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-094 |
| **Source Method** | `ExecuteMultiAccountMarket` |
| **Source File** | `src/V12_002.SIMA.Execution.cs` |
| **Baseline CYC** | 17 |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 4 |
| **max_helper_cyc_projected** | 6 |
| **CYC Threshold** | 8 |

All 3 tickets are surgical extractions within `src/V12_002.SIMA.Execution.cs`. No other files are touched. The `EnableSIMA` and `isFlattenRunning` volatile guards MUST remain as the first two statements in the residual `ExecuteMultiAccountMarket` body (volatile-read ordering guarantee — gjengset rule). These guards are **not** extracted by any ticket.

---

## CYC Reduction Ledger

| Ticket | Helper | CYC Reduction (from parent) | Projected Helper CYC |
|--------|--------|-----------------------------|----------------------|
| T1 | `ShouldSkipFleetAccountMarket` | 4 | 4 |
| T2 | `ExecuteMarketOrderForAccount` | 6 | 6 |
| T3 | `BuildMarketExecutionReport` | 3 | 3 |
| **Total extracted** | | **13** | |
| **Residual parent** | `ExecuteMultiAccountMarket` | | **4** |

CYC math: 17 (baseline) − 13 (extracted) = **4** (residual) ✅

---

## TICKET-1: Extract ShouldSkipFleetAccountMarket

**ticket_id:** EPIC-W7-094-T1
**helper_name:** `ShouldSkipFleetAccountMarket`
**concern:** Pure account filter predicate — decides whether a given account should be skipped before any order submission attempt

### Signature

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool ShouldSkipFleetAccountMarket(Account acct, out string reason)
```

### Lines to Move

Extraction moves the following decision nodes out of the `foreach` body in `ExecuteMultiAccountMarket` into this helper:

1. `IsFleetAccount` prefix string filter — `V12_002.IsFleetAccount(acct)` call + branch (+1 CYC)
2. `activeFleetAccounts` compound OR membership check — two OR-joined conditions (+2 CYC)
3. `EnableConsistencyLock` daily-P&L ceiling check — read-only flag branch (+1 CYC)

The `out string reason` parameter captures the skip rationale for diagnostic logging. Returns `true` when the account must be skipped.

### CYC Budget

| Metric | Value |
|--------|-------|
| **cyc_reduction** (nodes removed from parent) | 4 |
| **projected_helper_cyc** | 4 |
| **Attribute** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| **Risk** | LOW — pure predicate, no I/O, no allocation, no exception path |

### Constraints

- No `lock()` blocks (gjengset rule)
- No LINQ, no heap allocation (carl_cook zero-alloc)
- `AggressiveInlining` valid: no catch block, no loop, pure filter logic
- Returns `bool` only; no side effects on shared state

### xUnit Test Hook

```csharp
[Fact] public void ShouldSkipFleetAccountMarket_NonFleetAccount_ReturnsFalse()
[Fact] public void ShouldSkipFleetAccountMarket_ConsistencyLockBreach_ReturnsTrue()
[Fact] public void ShouldSkipFleetAccountMarket_InactiveFleet_ReturnsTrue()
```

---

## TICKET-2: Extract ExecuteMarketOrderForAccount

**ticket_id:** EPIC-W7-094-T2
**helper_name:** `ExecuteMarketOrderForAccount`
**concern:** Single-account order submission with position-delta reservation and deterministic catch-rollback

### Signature

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void ExecuteMarketOrderForAccount(
    Account acct,
    OrderAction action,
    int quantity,
    ref int successCount,
    ref int failCount,
    ref StringBuilder reportBuilder)
```

### Lines to Move

Extraction moves the following nodes out of the `foreach` body:

1. `reservedDelta` pre-computation (signed delta assignment BEFORE `CreateOrder` call — **race fix**: ensures rollback path in `catch` always has correct delta even if `CreateOrder` throws)
2. `AddExpectedPositionDeltaLocked` reservation call
3. `try` block entry (+1 CYC — exception path)
4. `CreateOrder` call inside try
5. Null-guard on returned `Order` object (+1 CYC)
6. Direction-conditional `reservedDelta` ternary — buy=+delta, sell=−delta (+1 CYC)
7. `Submit` call inside try
8. `catch` block: `if (reservedDelta != 0) rollback()` (+2 CYC — catch entry + null guard)
9. `successCount` / `failCount` accumulation via `ref` params

### CYC Budget

| Metric | Value |
|--------|-------|
| **cyc_reduction** (nodes removed from parent) | 6 |
| **projected_helper_cyc** | 6 |
| **Attribute** | `[MethodImpl(MethodImplOptions.NoInlining)]` |
| **Risk** | HIGH — contains the reservedDelta race fix and ref param boundary |

### Constraints

- `reservedDelta` MUST be assigned BEFORE `CreateOrder` call — not after (race fix from Phase 2)
- `NoInlining` mandatory: JIT cannot safely inline exception-handler-bearing methods
- `ref` params for `successCount`, `failCount`, `reportBuilder` — avoids heap closure and boxing (carl_cook)
- No new `lock()` blocks; `AddExpectedPositionDeltaLocked` carries its own synchronization
- `Account.All.ToArray()` snapshot taken in residual parent BEFORE the foreach — do not re-enumerate inside this helper

### xUnit Test Hook

```csharp
[Fact] public void ExecuteMarketOrderForAccount_SuccessPath_IncrementsSuccessCount()
[Fact] public void ExecuteMarketOrderForAccount_CreateOrderThrows_RollbacksReservedDelta()
[Fact] public void ExecuteMarketOrderForAccount_NullOrder_IncrementsFailCount()
```

---

## TICKET-3: Extract BuildMarketExecutionReport

**ticket_id:** EPIC-W7-094-T3
**helper_name:** `BuildMarketExecutionReport`
**concern:** Forensic report assembly — cold post-loop path that constructs the execution summary string

### Signature

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private string BuildMarketExecutionReport(
    int successCount,
    int failCount,
    string instrument)
```

### Lines to Move

Extraction moves the following nodes out of the post-loop section of `ExecuteMultiAccountMarket`:

1. `StringBuilder` construction and 16-line forensic field assembly
2. `LogBuffer.Format` overload dispatch — runtime overload selection (+1 CYC) → `src/V12_002.Perf.LogBuffer.cs:28`
3. Pass annotation branch — conditional success marker append (+1 CYC)
4. Fail annotation branch — conditional failure marker append (+1 CYC)
5. `StampAccountFillGrace` call → `src/V12_002.REAPER.cs:56`
6. Final `return` of assembled string

### CYC Budget

| Metric | Value |
|--------|-------|
| **cyc_reduction** (nodes removed from parent) | 3 |
| **projected_helper_cyc** | 3 |
| **Attribute** | `[MethodImpl(MethodImplOptions.NoInlining)]` |
| **Risk** | LOW — cold path, string-only, no shared state mutations |

### Constraints

- `StringBuilder` allocation is confined to this cold helper — NOT in the hot foreach body (carl_cook zero-alloc on hot path)
- `NoInlining` appropriate: string-allocating cold path; inlining would bloat the hot-path JIT frame
- Returns `string`; no side effects on caller state
- Called once per `ExecuteMultiAccountMarket` invocation, after the foreach loop completes

### xUnit Test Hook

```csharp
[Fact] public void BuildMarketExecutionReport_AllSuccess_ReturnsPassAnnotation()
[Fact] public void BuildMarketExecutionReport_AnyFail_ReturnsFailAnnotation()
[Fact] public void BuildMarketExecutionReport_MixedResult_ContainsBothCounts()
```

---

## Residual ExecuteMultiAccountMarket (Post-Extraction)

After all 3 tickets are applied, the residual parent method retains exactly:

```
1. if (!EnableSIMA) return;                    // volatile guard — MUST be first statement
2. if (isFlattenRunning) return;               // volatile guard — MUST be second statement
3. var snapshot = Account.All.ToArray();       // one pre-loop allocation
4. foreach (var acct in snapshot)              // +1 CYC (foreach)
5.     if (ShouldSkipFleetAccountMarket(...))  // extracted T1 — single call
6.         continue;
7.     ExecuteMarketOrderForAccount(...);       // extracted T2 — single call
8. var report = BuildMarketExecutionReport();  // extracted T3 — single call
```

**Residual CYC: 4** = baseline(1) + EnableSIMA guard(1) + isFlattenRunning guard(1) + foreach(1)

**projected_parent_cyc_after_all: 4** ✅

---

## Sequential Thinking Evidence

**Thought 1 — CYC Decomposition:**
Mapped all 17 CYC nodes to 4 drivers. Confirmed 3 extraction targets and residual=4. Ticket cyc_reduction values: T1=4, T2=6, T3=3. Sum=13. Parent: 17-13=4 ✅.

**Thought 2 — Ticket Ordering + Dependency Constraints:**
Validated extraction order T1→T2→T3. Confirmed reservedDelta race fix scoped to T2. Confirmed volatile guards stay in parent across all tickets. Validated ref param pattern for zero-alloc cross-call semantics.

**Thought 3 — Final CYC Validation:**
All helper CYC values ≤ 8 threshold: Skip=4, Execute=6, Report=3. Residual=4. max_helper_cyc=6 ✅. 3 tickets correctly partitioned. No scope creep. DNA PASS confirmed from Phase 3.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-094 |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 4 |
| **max_helper_cyc_projected** | 6 |
| **DNA Verdict (from Phase 3)** | PASS |
| **Jane Street KB** | carl_cook + gjengset + trading_billions applied |
| **Sequential Thinking Calls** | 3 |
| **jCodemunch Tools Called** | resolve_repo |
| **Output** | docs/brain/EPIC-W7-094/04-tickets.md |
