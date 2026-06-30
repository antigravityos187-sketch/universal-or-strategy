# Phase 4: Implementation Tickets — EPIC-W7-051

<!-- metadata: ticket_count=4 epic_id=EPIC-W7-051 wave=7 method=UpdateStopOrder source=src/V12_002.Trailing.StopUpdate.cs -->

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic ID** | EPIC-W7-051 |
| **Source File** | `src/V12_002.Trailing.StopUpdate.cs` |
| **Method** | `UpdateStopOrder` |
| **CYC (original)** | 6 |
| **CYC (max projected)** | 5 |
| **ticket_count** | 4 |
| **extraction_count** | 4 |
| **dna_verdict** | PASS |
| **jcodemunch tools called** | `resolve_repo`, `get_symbol_complexity`, `get_extraction_candidates` |
| **sequential-thinking calls** | 4 |
| **Output** | `docs/brain/EPIC-W7-051/04-tickets.md` |
| **Execution Time** | 2026-06-29T01:15:00Z |

---

## Extraction Summary

The `UpdateStopOrder` method (CYC=6, 56-line body) routes stop-order state updates through
four distinct paths by inline if/return chains. This epic surgically extracts the routing
logic into named helpers, introduces a `StopRouteDecision` enum that makes illegal routing
states unrepresentable, and reduces the parent method to a pure orchestrator (CYC=3).

**Constraint:** `UpdateStopOrder` signature MUST NOT change (15 call sites across 7 files).

| Ticket | Name | CYC Target | Type |
|---|---|---|---|
| W7-051-T1 | `StopRouteDecision` enum | 0 (no logic) | New type |
| W7-051-T2 | `IsStalePendingReplacement` | 3 | Extraction |
| W7-051-T3 | `ResolveStopRoute` | 5 | Extraction |
| W7-051-T4 | `DispatchToHandler` + parent refactor | 5 (parent=3) | Extraction |

**Execution order:** T1 → T2 → T3 → T4 (each ticket depends on the prior).

---

## W7-051-T1 — Add `StopRouteDecision` Enum

### Context

`UpdateStopOrder` currently encodes four routing paths as implicit branches in an if/return
chain. There is no named type for the routing decision, making illegal states representable
and preventing the compiler from enforcing exhaustive switch coverage. This ticket adds the
`StopRouteDecision` enum — a zero-logic type that is the prerequisite for all subsequent
extraction tickets.

### Implementation

Add the following nested type to `V12_002` within `src/V12_002.Trailing.StopUpdate.cs`,
placed immediately before the `UpdateStopOrder` method signature:

```csharp
// W7-051-T1: Makes illegal stop-routing states unrepresentable.
// Compiler enforces exhaustive switch coverage in DispatchToHandler (W7-051-T4).
private enum StopRouteDecision
{
    StalePending,    // stale-pending replacement detected; age > STALE_PENDING_FAST_PATH_SEC
    UpdatePending,   // order state is CancelPending or Submitted; update existing pending
    ReplaceWorking,  // order state is Working or Accepted; initiate replacement
    CreateDirect     // no active stop found via dict lookup; create new stop directly
}
```

No other code changes in this ticket. The enum is private to `V12_002`.

### Acceptance Criteria

- [ ] `StopRouteDecision` enum declared with exactly 4 values: `StalePending`, `UpdatePending`, `ReplaceWorking`, `CreateDirect`
- [ ] Enum is `private` and nested within the `V12_002` partial class in `src/V12_002.Trailing.StopUpdate.cs`
- [ ] No other files are modified
- [ ] Build passes: `dotnet build src/` — zero errors, zero warnings introduced
- [ ] CSharpier check passes: `dotnet csharpier check src/` — zero formatting issues
- [ ] ASCII-only: no Unicode, emoji, or curly quotes in new code

### Jane Street Alignment

| Principle | Status |
|---|---|
| Illegal states unrepresentable | Achieved — 4-value enum replaces implicit routing branches |
| Zero logic in type definition | Yes — enum values only, no methods |
| No scope creep (V12.23) | Single file, one type added |

---

## W7-051-T2 — Extract `IsStalePendingReplacement` Predicate

### Context

Inside `UpdateStopOrder`, lines 92–97 (approximate) check whether a stale-pending replacement
exists and whether it has aged beyond `STALE_PENDING_FAST_PATH_SEC`. This age-check predicate
is an independent unit of logic (CYC=3) that can be extracted to a pure private method, making
it independently testable and removing 2 decision branches from the parent.

This is a **pure predicate**: it performs only read operations on `pendingStopReplacements`
and `DateTime` arithmetic. Zero heap allocation (struct DateTime on stack).

### Implementation

Add `IsStalePendingReplacement` to `V12_002` in `src/V12_002.Trailing.StopUpdate.cs`:

```csharp
// W7-051-T2: Pure predicate. CYC=3. Zero allocation (stack DateTime arithmetic).
// Returns true when a stale pending replacement exists whose age exceeds the fast-path threshold.
private bool IsStalePendingReplacement(string entryName)
{
    if (!pendingStopReplacements.TryGetValue(entryName, out var pendingRecord))
    {
        return false;
    }

    double pendingAgeSeconds = (DateTime.UtcNow - pendingRecord.RecordedAt).TotalSeconds;
    return pendingAgeSeconds > STALE_PENDING_FAST_PATH_SEC;
}
```

No changes to `UpdateStopOrder` in this ticket — the predicate is added but not yet wired in.
Wiring occurs in T3 (`ResolveStopRoute`).

### Acceptance Criteria

- [ ] `IsStalePendingReplacement(string entryName)` declared as `private bool` in `V12_002` in target file
- [ ] Method reads `pendingStopReplacements` via `TryGetValue` and computes age using `DateTime.UtcNow`
- [ ] Returns `false` when entry not found; returns age comparison result otherwise
- [ ] No modification to `UpdateStopOrder` or any other existing method
- [ ] No other files modified
- [ ] Build passes: `dotnet build src/` — zero errors
- [ ] CSharpier check passes: `dotnet csharpier check src/`
- [ ] Cyclomatic complexity = 3 (1 base + 1 TryGetValue branch + 1 age comparison)
- [ ] ASCII-only identifiers and string literals

### Jane Street Alignment

| Principle | Status |
|---|---|
| CYC = 3 (well within ≤8) | Yes |
| Single responsibility | Yes — one concern: stale-pending age check |
| Zero allocation | Yes — `DateTime` arithmetic is stack-only |
| Lock-free | Yes — read-only dictionary access, no locking |
| Independently testable | Yes — pure predicate with no side effects |

---

## W7-051-T3 — Extract `ResolveStopRoute` Classifier

### Context

`UpdateStopOrder` contains three inline if/return routing branches that classify the stop
order's current state into one of four handling paths. These branches are complex enough
(CYC=5 in isolation) to warrant extraction as a named classifier method. After extraction,
`UpdateStopOrder` no longer contains any routing logic inline — it delegates to
`ResolveStopRoute` and receives a `StopRouteDecision` enum value.

This method depends on T1 (`StopRouteDecision` enum) and T2 (`IsStalePendingReplacement`).

### Implementation

Add `ResolveStopRoute` to `V12_002` in `src/V12_002.Trailing.StopUpdate.cs`:

```csharp
// W7-051-T3: Stop-route classifier. CYC=5. Returns StopRouteDecision enum.
// Encapsulates all routing logic extracted from UpdateStopOrder.
private StopRouteDecision ResolveStopRoute(string entryName, Order currentStop)
{
    if (IsStalePendingReplacement(entryName))
    {
        return StopRouteDecision.StalePending;
    }

    if (currentStop.OrderState == OrderState.CancelPending
        || currentStop.OrderState == OrderState.Submitted)
    {
        return StopRouteDecision.UpdatePending;
    }

    if (currentStop.OrderState == OrderState.Working
        || currentStop.OrderState == OrderState.Accepted)
    {
        return StopRouteDecision.ReplaceWorking;
    }

    return StopRouteDecision.CreateDirect;
}
```

No changes to `UpdateStopOrder` or `IsStalePendingReplacement` in this ticket.
Wiring of `ResolveStopRoute` into `UpdateStopOrder` occurs in T4.

### Acceptance Criteria

- [ ] `ResolveStopRoute(string entryName, Order currentStop)` declared as `private StopRouteDecision` in `V12_002` in target file
- [ ] Method calls `IsStalePendingReplacement(entryName)` (T2 dependency)
- [ ] Checks `OrderState.CancelPending`, `OrderState.Submitted`, `OrderState.Working`, `OrderState.Accepted`
- [ ] Returns `StopRouteDecision.CreateDirect` as fall-through default
- [ ] No modification to `UpdateStopOrder` or any other existing method
- [ ] No other files modified
- [ ] Build passes: `dotnet build src/` — zero errors
- [ ] CSharpier check passes: `dotnet csharpier check src/`
- [ ] Cyclomatic complexity = 5 (1 base + 1 IsStalePendingReplacement branch + 2 OrderState branches + 1 compound || in first branch)
- [ ] ASCII-only identifiers

### Jane Street Alignment

| Principle | Status |
|---|---|
| CYC = 5 (well within ≤8) | Yes |
| Single responsibility | Yes — one concern: classify stop routing decision |
| Illegal states unrepresentable | Yes — returns `StopRouteDecision` enum, exhaustively handled |
| Lock-free | Yes — read-only OrderState check, no locking |
| Independently testable | Yes — no side effects, deterministic return |

---

## W7-051-T4 — Extract `DispatchToHandler` + Refactor Parent to CYC=3

### Context

This is the final extraction ticket. It extracts `DispatchToHandler` — a switch delegate that
routes a `StopRouteDecision` to the correct pre-existing private handler — and simultaneously
refactors `UpdateStopOrder` to use `ResolveStopRoute` (T3) and `DispatchToHandler` (this
ticket) in place of all inline routing branches.

After this ticket, `UpdateStopOrder` is a pure orchestrator: guard → validate → resolve →
dispatch → catch (CYC=3). The `DispatchToHandler` method itself has CYC=5 (1 base + 4 switch
cases). This is the only ticket that modifies `UpdateStopOrder`'s body.

**Critical constraint:** `UpdateStopOrder` signature MUST NOT change:
`private void UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)`

### Implementation

**Step A — Add `DispatchToHandler`** to `V12_002` in `src/V12_002.Trailing.StopUpdate.cs`:

```csharp
// W7-051-T4: Switch delegate. CYC=5. Routes StopRouteDecision to pre-existing handlers.
// All handler methods pre-exist; this method adds no new logic.
private void DispatchToHandler(
    StopRouteDecision route,
    string entryName,
    PositionInfo pos,
    Order currentStop,
    double validatedStopPrice,
    int newTrailLevel)
{
    switch (route)
    {
        case StopRouteDecision.StalePending:
            HandleStalePendingReplacement(entryName, pos, currentStop, validatedStopPrice, newTrailLevel);
            break;
        case StopRouteDecision.UpdatePending:
            UpdateExistingPendingReplacement(entryName, pos, currentStop, validatedStopPrice, newTrailLevel);
            break;
        case StopRouteDecision.ReplaceWorking:
            InitiateStopReplacement(entryName, pos, currentStop, validatedStopPrice, newTrailLevel);
            break;
        case StopRouteDecision.CreateDirect:
            CreateDirectStopOrder(entryName, pos, validatedStopPrice, newTrailLevel);
            break;
    }
}
```

**Step B — Refactor `UpdateStopOrder` body** (signature unchanged):

```csharp
// Signature unchanged: 15 call sites across 7 files are unaffected.
private void UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)
{
    try
    {
        if (!stopOrders.TryGetValue(entryName, out var currentStop))
        {
            return;
        }

        double validatedStopPrice = ValidateStopPrice(
            pos.Direction, newStopPrice, newTrailLevel, pos.EntryPrice);

        StopRouteDecision route = ResolveStopRoute(entryName, currentStop);
        DispatchToHandler(route, entryName, pos, currentStop, validatedStopPrice, newTrailLevel);
    }
    catch (Exception ex)
    {
        HandleUpdateException(entryName, pos, ex);
    }
}
```

### Acceptance Criteria

- [ ] `DispatchToHandler` declared as `private void` with parameters `(StopRouteDecision route, string entryName, PositionInfo pos, Order currentStop, double validatedStopPrice, int newTrailLevel)`
- [ ] `DispatchToHandler` switch covers all 4 `StopRouteDecision` cases; no default branch needed (compiler guarantees exhaustion)
- [ ] `UpdateStopOrder` signature is bit-for-bit identical to pre-extraction: `private void UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)`
- [ ] `UpdateStopOrder` body calls `ResolveStopRoute` and `DispatchToHandler`; contains no inline OrderState comparisons
- [ ] `UpdateStopOrder` CYC = 3 post-refactor (1 base + 1 guard + 1 catch)
- [ ] `DispatchToHandler` CYC = 5 (1 base + 4 switch cases)
- [ ] No files other than `src/V12_002.Trailing.StopUpdate.cs` are modified
- [ ] Build passes: `dotnet build src/` — zero errors, zero warnings
- [ ] CSharpier check passes: `dotnet csharpier check src/`
- [ ] Lint passes: `powershell -File .\scripts\lint.ps1` — no new violations
- [ ] ASCII-only identifiers and literals

### Jane Street Alignment

| Principle | Status |
|---|---|
| CYC = 3 for parent `UpdateStopOrder` | Yes — pure orchestrator after extraction |
| CYC = 5 for `DispatchToHandler` (≤8) | Yes |
| Illegal states unrepresentable | Yes — switch on closed `StopRouteDecision` enum |
| Lock-free / Actor pattern preserved | Yes — Enqueue/Interlocked remain in existing sibling helpers, untouched |
| No scope creep (V12.23) | Yes — single file, no caller files modified |
| Signature stability | Yes — 15 call sites across 7 files are unaffected |

---

## Dependency Graph

```
T1 (StopRouteDecision enum)
 └─ T2 (IsStalePendingReplacement) ← reads pendingStopReplacements
     └─ T3 (ResolveStopRoute) ← calls T2, returns T1 enum
         └─ T4 (DispatchToHandler + UpdateStopOrder refactor) ← uses T1 enum + T3 classifier
```

## Final CYC Table

| Method | CYC Before | CYC After |
|---|---|---|
| `UpdateStopOrder` | 6 | 3 |
| `IsStalePendingReplacement` | — | 3 (new) |
| `ResolveStopRoute` | — | 5 (new) |
| `DispatchToHandler` | — | 5 (new) |
| `StopRouteDecision` | — | 0 (enum, no logic) |
| **max_cyc** | **6** | **5** |
