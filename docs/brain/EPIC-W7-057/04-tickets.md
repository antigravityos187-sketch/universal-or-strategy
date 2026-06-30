# Phase 4: Ticket Definitions — EPIC-W7-057

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-057 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Method** | `SymmetryGuardTryResolveFollower` |
| **Source File** | `src/V12_002.Symmetry.Follower.cs` |
| **Original CYC** | 12 |
| **ticket_count** | 4 |
| **projected_parent_cyc_after_all** | 7 |
| **max_cyc_across_all_symbols** | 7 |
| **DNA Audit Verdict (Phase 3)** | PASS |

---

## Sequential Thinking Validation

4-thought chain completed:
- **Thought 1**: Ticket count = 4 (3 extraction tickets + 1 test ticket; one concern per ticket).
- **Thought 2**: Per-ticket line ranges, helper signatures, and projected CYC values established.
- **Thought 3**: All CYC projections verified — max(7, 4, 3, 3) = 7 ≤ 8 constraint satisfied.
- **Thought 4**: Final confirmation — 4 tickets ready, lock-free ADR-019 preserved, no scope creep.

---

## jcodemunch MCP Evidence

| Tool | Result |
|---|---|
| `resolve_repo` | `antigravityos187-sketch/universal-or-strategy` — indexed, 5147 symbols |
| `get_symbol_complexity` | Symbol not in index (C# partial class pattern — expected; Phase 2 source analysis authoritative) |
| `get_extraction_candidates` | 0 candidates returned (intra-class calls invisible to import graph — expected per Phase 2) |

---

## CYC Projection Table

| Symbol | Role | Projected CYC | ≤ 8? |
|---|---|---|---|
| `SymmetryGuardTryResolveFollower` | Parent (after all extractions) | 7 | ✅ |
| `TryResolveDispatchContext` | Extracted helper — Ticket 1 | 4 | ✅ |
| `TryResolveAnchorSnapshot` | Extracted helper — Ticket 2 | 3 | ✅ |
| `IsSlippageWithinTolerance` | Extracted helper — Ticket 3 | 3 | ✅ |
| **max** | | **7** | ✅ |

---

## TICKET-1: Extract `TryResolveDispatchContext`

| Field | Value |
|---|---|
| **Ticket ID** | TICKET-1 |
| **Type** | Extraction |
| **Target File** | `src/V12_002.Symmetry.Follower.cs` |
| **Lines to Extract (approx)** | 133–153 (dispatch context lookup block) |
| **Projected Helper CYC** | 4 |
| **Projected Parent CYC (after this ticket)** | 10 (3 decisions removed from parent) |

### Description

Extract the compound `TryGetValue` dispatch context lookup guard from `SymmetryGuardTryResolveFollower` into a new private method `TryResolveDispatchContext`. This block performs:
1. `symmetryFleetEntryToDispatch.TryGetValue(fleetEntryName, ...)` — first dictionary lookup
2. `symmetryDispatchById.TryGetValue(...)` — second dictionary lookup (with id from step 1)
3. If either lookup fails and `nowUtc - pending.FillTime > AnchorWait`: call `SymmetryGuardSkipFollower("Missing dispatch context")` and return `true` (skip, do not retry)
4. If either lookup fails but within timeout: return `false` (caller should not proceed)

### New Method Signature

```csharp
private bool TryResolveDispatchContext(
    string fleetEntryName,
    PositionInfo pos,
    PendingFollowerFill pending,
    DateTime nowUtc,
    out SymmetryDispatchContext ctx)
```

### Decision Points (CYC = 4)

| # | Condition |
|---|---|
| 1 | `!symmetryFleetEntryToDispatch.TryGetValue(fleetEntryName, out ...)` |
| 2 | `!symmetryDispatchById.TryGetValue(id, out ctx)` |
| 3 | `(nowUtc - pending.FillTime) > AnchorWait` (timeout branch) |

### Parent Call Site Replacement

```csharp
SymmetryDispatchContext ctx;
if (!TryResolveDispatchContext(fleetEntryName, pos, pending, nowUtc, out ctx))
    return false;
```

### Constraints

- All dictionary reads use `ConcurrentDictionary.TryGetValue` — lock-free per ADR-019. No new `lock()` blocks.
- `out ctx` enforces that `ctx` is only accessible to the caller when the method returns `true` (illegal-states-unrepresentable).
- ASCII-only string literals in any `Print`/`SymmetryGuardSkipFollower` calls.
- Helper is `private` — no public API surface added.

---

## TICKET-2: Extract `TryResolveAnchorSnapshot`

| Field | Value |
|---|---|
| **Ticket ID** | TICKET-2 |
| **Type** | Extraction |
| **Target File** | `src/V12_002.Symmetry.Follower.cs` |
| **Lines to Extract (approx)** | 155–172 (anchor snapshot resolution block) |
| **Projected Helper CYC** | 3 |
| **Projected Parent CYC (after this ticket)** | 8 (2 additional decisions removed from parent) |

### Description

Extract the anchor snapshot resolution guard from `SymmetryGuardTryResolveFollower` into a new private method `TryResolveAnchorSnapshot`. This block:
1. Reads `ctx.Anchor` via `Interlocked.CompareExchange` (ADR-019 lock-free atomic read)
2. Checks `snapshot.IsResolved`
3. If not resolved and `AnchorWait` elapsed: call `SymmetryGuardSkipFollower("Master anchor timeout")` and return `true`
4. If not resolved within timeout: return `false`
5. Returns `masterAnchor` price via `out` parameter on success

### New Method Signature

```csharp
private bool TryResolveAnchorSnapshot(
    string fleetEntryName,
    PositionInfo pos,
    PendingFollowerFill pending,
    DateTime nowUtc,
    SymmetryDispatchContext ctx,
    out double masterAnchor)
```

### Decision Points (CYC = 3)

| # | Condition |
|---|---|
| 1 | `!snapshot.IsResolved` |
| 2 | `(nowUtc - pending.FillTime) > AnchorWait` (timeout branch) |

### Parent Call Site Replacement

```csharp
double masterAnchor;
if (!TryResolveAnchorSnapshot(fleetEntryName, pos, pending, nowUtc, ctx, out masterAnchor))
    return false;
```

### Constraints

- Must use `Interlocked.CompareExchange` for `ctx.Anchor` read — ADR-019 lock-free atomic contract.
- `out masterAnchor` enforces that the price is only accessible after guard returns `true`.
- No new `lock()` blocks.
- ASCII-only string literals.
- Helper is `private`.

---

## TICKET-3: Extract `IsSlippageWithinTolerance`

| Field | Value |
|---|---|
| **Ticket ID** | TICKET-3 |
| **Type** | Extraction |
| **Target File** | `src/V12_002.Symmetry.Follower.cs` |
| **Lines to Extract (approx)** | 174–195 (slippage calculation and breach check block) |
| **Projected Helper CYC** | 3 |
| **Projected Parent CYC (after this ticket)** | 7 (final residual: 6 decisions + base) |

### Description

Extract the slippage calculation and breach evaluation guard from `SymmetryGuardTryResolveFollower` into a new private method `IsSlippageWithinTolerance`. This block:
1. Computes `slippagePoints` = `Math.Abs(pending.FleetFillPrice - masterAnchor)`
2. Derives `slippageTicks` from `slippagePoints` and tick size
3. Derives `slippageUsdPerContract` from `slippageTicks` and contract dollar value
4. Evaluates compound breach: `slippageTicks > SymmetryMaxSlippageTicks || slippageUsdPerContract > SymmetryMaxSlippageUsdPerContract`
5. On breach: calls `SymmetryGuardSkipFollower("Slippage Buffer breach...")` and returns `false`
6. Within tolerance: returns `true`

### New Method Signature

```csharp
private bool IsSlippageWithinTolerance(
    string fleetEntryName,
    PositionInfo pos,
    PendingFollowerFill pending,
    double masterAnchor)
```

### Decision Points (CYC = 3)

| # | Condition |
|---|---|
| 1 | `slippageTicks > SymmetryMaxSlippageTicks` (left side of `\|\|`) |
| 2 | `slippageUsdPerContract > SymmetryMaxSlippageUsdPerContract` (right side of `\|\|`) |

### Parent Call Site Replacement

```csharp
if (!IsSlippageWithinTolerance(fleetEntryName, pos, pending, masterAnchor))
    return true;
```

### Constraints

- No heap allocations: all computations use local value-type arithmetic. Zero-allocation hot-path preserved.
- No `lock()` blocks.
- ASCII-only string literals in `SymmetryGuardSkipFollower` message.
- Helper is `private`.

---

## TICKET-4: xUnit Tests for Extracted Helpers

| Field | Value |
|---|---|
| **Ticket ID** | TICKET-4 |
| **Type** | Test |
| **Target File** | `tests/V12_Performance.Tests/Core/SymmetryFollowerExtractTests.cs` (new file) |
| **Test Count** | 7 |
| **Framework** | xUnit `[Fact]` + `Assert.Equal()` / `Assert.True()` / `Assert.False()` — NO NUnit, NO MSTest |

### Test Cases

| Test Method | Helper Under Test | Scenario |
|---|---|---|
| `TryResolveDispatchContext_MissingEntry_TimeoutElapsed_SkipsAndReturnsTrue` | `TryResolveDispatchContext` | Fleet entry not found in dictionary; `nowUtc - pending.FillTime > AnchorWait` — expect skip + return `true` |
| `TryResolveDispatchContext_MissingEntry_WithinTimeout_ReturnsFalse` | `TryResolveDispatchContext` | Fleet entry not found; within `AnchorWait` window — expect return `false` |
| `TryResolveAnchorSnapshot_NotResolved_TimeoutElapsed_SkipsAndReturnsTrue` | `TryResolveAnchorSnapshot` | `snapshot.IsResolved = false`; `nowUtc` past `AnchorWait` — expect skip + return `true` |
| `TryResolveAnchorSnapshot_NotResolved_WithinTimeout_ReturnsFalse` | `TryResolveAnchorSnapshot` | `snapshot.IsResolved = false`; within `AnchorWait` — expect return `false` |
| `IsSlippageWithinTolerance_TicksBreach_SkipsAndReturnsFalse` | `IsSlippageWithinTolerance` | `slippageTicks > SymmetryMaxSlippageTicks` — expect skip call + return `false` |
| `IsSlippageWithinTolerance_UsdBreach_SkipsAndReturnsFalse` | `IsSlippageWithinTolerance` | `slippageUsdPerContract > SymmetryMaxSlippageUsdPerContract` — expect skip call + return `false` |
| `IsSlippageWithinTolerance_WithinBothLimits_ReturnsTrue` | `IsSlippageWithinTolerance` | Both values within limits — expect return `true`, no skip call |

### Constraints

- Use xUnit `[Fact]` attribute only — no `[Theory]`, no NUnit `[Test]`, no MSTest `[TestMethod]`.
- `Assert.True` / `Assert.False` for return values; `Assert.Equal` for out-param verification.
- No Unicode characters in any test string literals.
- File must be in `tests/V12_Performance.Tests/Core/` — existing test project.

---

## Execution Order

```
TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4
```

Tickets 1–3 must complete in order (each extraction simplifies the parent). Ticket 4 (tests) must follow all three extractions, as the helpers must exist before they can be tested.

---

## Jane Street Alignment

| Principle | Status |
|---|---|
| CYC ≤ 8 mandatory | ✅ max_cyc_projected = 7 across all symbols |
| Single-responsibility per helper | ✅ Each helper encapsulates exactly one guard predicate |
| No `lock()` blocks | ✅ ADR-019 preserved; `Interlocked.CompareExchange` + `ConcurrentDictionary.TryGetValue` only |
| Illegal states unrepresentable | ✅ `out` params enforce `ctx`/`masterAnchor` only accessible after guard returns `true` |
| Zero-allocation hot paths | ✅ No heap allocations in guard predicates |
| ASCII-only string literals | ✅ Required in all new method bodies |
| ONE method per epic (V12.23) | ✅ Only `SymmetryGuardTryResolveFollower` targeted |
| xUnit `[Fact]` only — no NUnit/MSTest | ✅ 7 `[Fact]` test cases planned |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-057 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:35:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 5 (1 probe + 4 ticket validation thoughts) |
| **ticket_count** | 4 |
| **projected_parent_cyc_after_all** | 7 |
| **max_cyc_across_all_symbols** | 7 |
| **DNA audit verdict (Phase 3)** | PASS |
| **Input artifacts** | docs/brain/EPIC-W7-057/02-architecture-plan.md, docs/brain/EPIC-W7-057/03-audit-report.md |
| **Output artifact** | docs/brain/EPIC-W7-057/04-tickets.md |
