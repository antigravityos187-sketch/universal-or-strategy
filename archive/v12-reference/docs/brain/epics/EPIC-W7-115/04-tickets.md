# EPIC-W7-115 — Phase 4 Tickets

**Method**: `SweepTrackedOrders`
**Source**: `src/V12_002.SIMA.Lifecycle.cs`
**CYC**: 34 (original) → 1 (parent after extraction)
**Lane**: P4-L7
**Wave**: 7
**DNA Verdict**: PASS (from Phase 3 audit)

---

## Ticket Summary

| # | Ticket | Type | Helper Signature | CYC Target | Dependencies |
|---|--------|------|-----------------|-----------|--------------|
| 1 | Extract `BuildTrackedSweepDicts` | extraction | `private ConcurrentDictionary<string, Order>[] BuildTrackedSweepDicts(bool force)` | ≤2 | None |
| 2 | Extract `IsTrackedOrderCancellable` | extraction | `private bool IsTrackedOrderCancellable(Order ord)` | ≤5 | None |
| 3 | Extract `CancelTrackedOrderSafe` | extraction | `private bool CancelTrackedOrderSafe(Order ord)` | ≤2 | None |
| 4 | Extract `SweepTrackedDictOrders` | extraction | `private int SweepTrackedDictOrders(ConcurrentDictionary<string, Order> dict)` | ≤5 | Tickets 2, 3 |
| 5 | Extract `SweepAllTrackedDicts` | extraction | `private int SweepAllTrackedDicts(ConcurrentDictionary<string, Order>[] dicts)` | ≤3 | Ticket 4 |
| 6 | Refactor parent `SweepTrackedOrders` | refactor | `private int SweepTrackedOrders(bool force)` | ≤1 | Tickets 1, 5 |

---

## Ticket 1 — Extract `BuildTrackedSweepDicts`

**Type**: extraction
**Target CYC**: ≤2
**Dependencies**: None (independent — can execute in parallel with Tickets 2 and 3)
**Execution Order**: First wave (parallel with Tickets 2, 3)

### Responsibility
Encapsulates the force-flag dict-selection semantic. When `force=true`, returns all 7 tracking
dictionaries (`entryOrders`, `stopOrders`, `target1Orders`–`target5Orders`). When `force=false`,
returns only `{ entryOrders }`. This is the safety-critical branch that determines sweep scope.

### Signature
```csharp
private ConcurrentDictionary<string, Order>[] BuildTrackedSweepDicts(bool force)
```

### Implementation Sketch
```csharp
private ConcurrentDictionary<string, Order>[] BuildTrackedSweepDicts(bool force)
{
    return force
        ? new[] { entryOrders, stopOrders, target1Orders, target2Orders,
                  target3Orders, target4Orders, target5Orders }
        : new[] { entryOrders };
}
```

### Acceptance Criteria
- [ ] Method added as `private` in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] `force=true` returns array of 7 dictionaries
- [ ] `force=false` returns array of 1 dictionary (`entryOrders`)
- [ ] No `lock()` blocks
- [ ] xUnit `[Fact]` test verifies both branches (force=true length=7, force=false length=1)
- [ ] CYC ≤ 2 confirmed by `complexity_audit.py`

### CYC Breakdown
- Base path: 1
- One ternary conditional: +1
- **Total: 2**

---

## Ticket 2 — Extract `IsTrackedOrderCancellable`

**Type**: extraction
**Target CYC**: ≤5
**Dependencies**: None (independent — can execute in parallel with Tickets 1 and 3)
**Execution Order**: First wave (parallel with Tickets 1, 3)

### Responsibility
Pure predicate. Rewrites the original inverted `&&`-chain (which skips non-live orders) into a
readable positive `||`-chain. Returns `true` if the order is in a live, cancellable state.
Makes the valid-state set explicit — single maintenance point when new `OrderState` values are added.

### Signature
```csharp
private bool IsTrackedOrderCancellable(Order ord)
```

### Implementation Sketch
```csharp
private bool IsTrackedOrderCancellable(Order ord)
{
    return ord.OrderState == OrderState.Working
        || ord.OrderState == OrderState.Accepted
        || ord.OrderState == OrderState.Submitted
        || ord.OrderState == OrderState.ChangePending
        || ord.OrderState == OrderState.ChangeSubmitted;
}
```

### Acceptance Criteria
- [ ] Method added as `private` in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] Returns `true` for all 5 live states: `Working`, `Accepted`, `Submitted`, `ChangePending`, `ChangeSubmitted`
- [ ] Returns `false` for all terminal/other states
- [ ] Pure function — no side effects, no mutation
- [ ] No `lock()` blocks
- [ ] xUnit `[Fact]` tests cover each `true` branch and at least one `false` branch
- [ ] CYC ≤ 5 confirmed

### CYC Breakdown
- Base path: 1
- OR branch per state value (4 additional conditions): +4
- **Total: 5**

---

## Ticket 3 — Extract `CancelTrackedOrderSafe`

**Type**: extraction
**Target CYC**: ≤2
**Dependencies**: None (independent — can execute in parallel with Tickets 1 and 2)
**Execution Order**: First wave (parallel with Tickets 1, 2)

### Responsibility
Wraps the try/catch cancel call. Invokes `CancelOrderOnAccount(ord, ord.Account)`, swallows
broker exceptions (prevents shutdown abort on transient broker errors), and returns a `bool`
indicating success. Replaces inline `try { ... trackedCancels++; } catch { }` with a named,
testable unit.

### Signature
```csharp
private bool CancelTrackedOrderSafe(Order ord)
```

### Implementation Sketch
```csharp
private bool CancelTrackedOrderSafe(Order ord)
{
    try
    {
        CancelOrderOnAccount(ord, ord.Account);
        return true;
    }
    catch
    {
        return false;
    }
}
```

### Acceptance Criteria
- [ ] Method added as `private` in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] Calls `CancelOrderOnAccount(ord, ord.Account)` on success path
- [ ] Returns `true` on success, `false` on any exception
- [ ] Exception is swallowed (no rethrow) — broker fault tolerance preserved
- [ ] No `lock()` blocks
- [ ] xUnit `[Fact]` tests verify success path returns `true` and exception path returns `false`
- [ ] CYC ≤ 2 confirmed

### CYC Breakdown
- Base path: 1
- try/catch alternate path: +1
- **Total: 2**

---

## Ticket 4 — Extract `SweepTrackedDictOrders`

**Type**: extraction
**Target CYC**: ≤5
**Dependencies**: Ticket 2 (`IsTrackedOrderCancellable`), Ticket 3 (`CancelTrackedOrderSafe`)
**Execution Order**: Second wave (after Tickets 2 and 3 complete)

### Responsibility
Inner sweep for a single dictionary. Iterates `dict.ToArray()` (actor-safe snapshot), null-guards
each `Order` value, calls `IsTrackedOrderCancellable`, calls `CancelTrackedOrderSafe` on live orders,
and accumulates the cancel count. Encapsulates the full inner-loop logic.

### Signature
```csharp
private int SweepTrackedDictOrders(ConcurrentDictionary<string, Order> dict)
```

### Implementation Sketch
```csharp
private int SweepTrackedDictOrders(ConcurrentDictionary<string, Order> dict)
{
    int count = 0;
    foreach (var kvp in dict.ToArray())
    {
        var ord = kvp.Value;
        if (ord == null) continue;
        if (!IsTrackedOrderCancellable(ord)) continue;
        if (CancelTrackedOrderSafe(ord)) count++;
    }
    return count;
}
```

### Acceptance Criteria
- [ ] Method added as `private` in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] Uses `dict.ToArray()` for actor-safe concurrent read (preserves original pattern)
- [ ] Null-guards `kvp.Value` before use
- [ ] Calls `IsTrackedOrderCancellable` before attempting cancel
- [ ] Calls `CancelTrackedOrderSafe` for cancellable orders
- [ ] Accumulates and returns cancel count
- [ ] No `lock()` blocks
- [ ] xUnit `[Fact]` test verifies: null entry skipped, non-cancellable skipped, cancellable counted
- [ ] CYC ≤ 5 confirmed

### CYC Breakdown
- Base path: 1
- `foreach` loop: +1
- null-guard `continue`: +1
- `IsTrackedOrderCancellable` guard: +1
- `CancelTrackedOrderSafe` result check: +1
- **Total: 5**

---

## Ticket 5 — Extract `SweepAllTrackedDicts`

**Type**: extraction
**Target CYC**: ≤3
**Dependencies**: Ticket 4 (`SweepTrackedDictOrders`)
**Execution Order**: Third wave (after Ticket 4 completes)

### Responsibility
Outer sweep orchestrating all tracking dictionaries. Iterates the dict array with a null-guard,
delegates each non-null dict to `SweepTrackedDictOrders`, accumulates total cancel count across
all dicts, and returns the aggregate. This is the only method that touches the multi-dict iteration.

### Signature
```csharp
private int SweepAllTrackedDicts(ConcurrentDictionary<string, Order>[] dicts)
```

### Implementation Sketch
```csharp
private int SweepAllTrackedDicts(ConcurrentDictionary<string, Order>[] dicts)
{
    int total = 0;
    foreach (var dict in dicts)
    {
        if (dict == null) continue;
        total += SweepTrackedDictOrders(dict);
    }
    return total;
}
```

### Acceptance Criteria
- [ ] Method added as `private` in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] Iterates all dicts in the input array
- [ ] Null-guards each `dict` entry before delegating
- [ ] Delegates to `SweepTrackedDictOrders` per dict
- [ ] Accumulates and returns total cancel count across all dicts
- [ ] No `lock()` blocks
- [ ] xUnit `[Fact]` test: null dict entry skipped, counts aggregated correctly across 2+ dicts
- [ ] CYC ≤ 3 confirmed

### CYC Breakdown
- Base path: 1
- `foreach` loop: +1
- null-guard `continue`: +1
- **Total: 3**

---

## Ticket 6 — Refactor Parent `SweepTrackedOrders`

**Type**: refactor
**Target CYC**: ≤1
**Dependencies**: Ticket 1 (`BuildTrackedSweepDicts`), Ticket 5 (`SweepAllTrackedDicts`)
**Execution Order**: Final wave (after all 5 extractions complete)

### Responsibility
Replace the original 45-line body with a 2-line orchestration call. Delegates dict-selection to
`BuildTrackedSweepDicts` and multi-dict sweep to `SweepAllTrackedDicts`. Caller
(`CancelAllV12GtcOrders` at line 1296) remains unmodified — signature unchanged.

### Before (45-line body, CYC=34)
Original body with nested foreach, inline guard chain, and inline try/catch.

### After (2-line orchestration body, CYC=1)
```csharp
private int SweepTrackedOrders(bool force)
{
    var dicts = BuildTrackedSweepDicts(force);
    return SweepAllTrackedDicts(dicts);
}
```

### Acceptance Criteria
- [ ] Parent body replaced with 2-line orchestration
- [ ] Signature `private int SweepTrackedOrders(bool force)` UNCHANGED
- [ ] Return type `int` UNCHANGED
- [ ] Caller `CancelAllV12GtcOrders` (line 1296) UNMODIFIED
- [ ] Calls `BuildTrackedSweepDicts(force)` and `SweepAllTrackedDicts(dicts)`
- [ ] No `lock()` blocks
- [ ] Build passes: `dotnet build src/` zero errors
- [ ] CSharpier check passes: `dotnet csharpier check src/`
- [ ] xUnit integration test: `SweepTrackedOrders(force: true)` and `SweepTrackedOrders(force: false)` return expected cancel counts
- [ ] CYC = 1 confirmed (no branches in parent)

### CYC Breakdown
- Base path: 1
- No branches, no conditionals, no loops in parent
- **Total: 1**

---

## Execution Order Summary

```
Wave A (parallel):  Ticket 1, Ticket 2, Ticket 3
Wave B (serial):    Ticket 4  (requires Tickets 2 + 3)
Wave C (serial):    Ticket 5  (requires Ticket 4)
Wave D (serial):    Ticket 6  (requires Tickets 1 + 5)
```

---

## CYC Reduction Summary

| Method | Before | After | Reduction |
|--------|--------|-------|-----------|
| `SweepTrackedOrders` (parent) | 34 | 1 | -33 |
| `BuildTrackedSweepDicts` (new) | — | 2 | — |
| `IsTrackedOrderCancellable` (new) | — | 5 | — |
| `CancelTrackedOrderSafe` (new) | — | 2 | — |
| `SweepTrackedDictOrders` (new) | — | 5 | — |
| `SweepAllTrackedDicts` (new) | — | 3 | — |
| **max_cyc** | **34** | **5** | **-29** |

---

## Jane Street Alignment

| Principle | Ticket Coverage |
|-----------|----------------|
| CYC ≤ 8 (all methods) | All 6 tickets enforce ≤ individual targets; max=5 |
| Single-responsibility per helper | Each ticket owns exactly one logical concern |
| Lock-free / Actor pattern | No `lock()` in any ticket |
| Illegal states unrepresentable | Ticket 2: `IsTrackedOrderCancellable` makes valid states explicit |
| Zero-allocation hot-paths | Only `new[]` in Ticket 1 (not hot path); `dict.ToArray()` preserved |
| Caller unmodified | Ticket 6 explicitly preserves `CancelAllV12GtcOrders` call site |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Epic ID** | EPIC-W7-115 |
| **Wave** | 7 |
| **Phase** | 4 |
| **Method** | `SweepTrackedOrders` |
| **Source** | `src/V12_002.SIMA.Lifecycle.cs` |
| **Original CYC** | 34 |
| **Ticket Count** | 6 |
| **Max Projected CYC** | 5 |
| **Parent CYC After** | 1 |
| **DNA Verdict** | PASS |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 |
| **Input** | docs/brain/EPIC-W7-115/02-architecture-plan.md, docs/brain/EPIC-W7-115/03-audit-report.md |
| **Output** | docs/brain/EPIC-W7-115/04-tickets.md |
