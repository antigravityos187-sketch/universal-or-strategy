# EPIC-W7-123 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:25:00Z
**Input:** docs/brain/EPIC-W7-123/02-architecture-plan.md + docs/brain/EPIC-W7-123/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-123 |
| **Method** | `HandleMatchedFollowerOrder` |
| **File** | `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **Original CYC** | 14 |
| **max_cyc_projected** | 5 |
| **ticket_count** | 7 |
| **DNA Verdict** | PASS |
| **Extraction Count** | 5 helpers |

---

## Ticket Execution Order

Tickets T1–T5 are independent extractions and may execute in a single Phase 5 pass.
T6 (parent rewrite) depends on T1–T5. T7 (xUnit tests) depends on T1–T3.

```
T1, T2, T3, T4, T5  →  T6  →  T7
```

---

## Ticket T1 — Extract `IsEntryOrderMatch`

| Field | Value |
|---|---|
| **ID** | EPIC-W7-123-T1 |
| **Type** | extraction |
| **File** | `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **CYC Target** | ≤ 5 |
| **Jane Street** | `[AggressiveInlining]` (hot-path zero-alloc predicate) |
| **Depends On** | None |

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool IsEntryOrderMatch(
    string matchedEntry,
    PositionInfo matchedPos,
    Order order,
    out Order entryOrder
)
```

**Extracted Body:**
```csharp
return entryOrders.TryGetValue(matchedEntry, out entryOrder)
    && (entryOrder == order || (entryOrder != null && entryOrder.OrderId == order.OrderId))
    && !matchedPos.EntryFilled;
```

**Acceptance Criteria:**
1. Method `IsEntryOrderMatch` exists as `private` in the same partial class.
2. Cyclomatic complexity (cyc) of `IsEntryOrderMatch` ≤ 5 (verified by `complexity_audit.py`).
3. `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute applied.
4. Zero `lock()` blocks introduced.
5. All string literals are ASCII-only.
6. `dotnet build` passes with zero errors after extraction.
7. The `out Order entryOrder` parameter correctly receives the dictionary value.

---

## Ticket T2 — Extract `IsAnyFollowerBracketActive`

| Field | Value |
|---|---|
| **ID** | EPIC-W7-123-T2 |
| **Type** | extraction |
| **File** | `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **CYC Target** | ≤ 5 |
| **Jane Street** | `[AggressiveInlining]` (hot-path LINQ over `ConcurrentDictionary`) |
| **Depends On** | None |

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool IsAnyFollowerBracketActive(string acctName)
```

**Extracted Body:**
```csharp
return _followerBrackets.Values.Any(f =>
    f != null
    && f.AccountName == acctName
    && (f.State == FollowerBracketState.Active || f.State == FollowerBracketState.Accepted)
);
```

**Acceptance Criteria:**
1. Method `IsAnyFollowerBracketActive` exists as `private` in the same partial class.
2. Cyclomatic complexity (cyc) of `IsAnyFollowerBracketActive` ≤ 5 (verified by `complexity_audit.py`).
3. `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute applied.
4. Zero `lock()` blocks introduced — `ConcurrentDictionary.Values.Any()` used only (lock-free read).
5. All string literals are ASCII-only.
6. `dotnet build` passes with zero errors after extraction.
7. Pure read predicate — no writes to `_followerBrackets` inside this method.

---

## Ticket T3 — Extract `ShouldRescuePendingCancelSpec`

| Field | Value |
|---|---|
| **ID** | EPIC-W7-123-T3 |
| **Type** | extraction |
| **File** | `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **CYC Target** | ≤ 4 |
| **Jane Street** | Standard inline (semi-cold meta/error path) |
| **Depends On** | None |

**Signature:**
```csharp
private bool ShouldRescuePendingCancelSpec(string matchedEntry, Order order)
```

**Extracted Body:**
```csharp
if (_followerReplaceSpecs.TryGetValue(matchedEntry, out var fsmGuard)
    && fsmGuard.State == FollowerReplaceState.PendingCancel
    && fsmGuard.CancellingOrderId == order.OrderId)
{
    Print("[META-PURGE GUARD] Rescuing PendingCancel spec " + matchedEntry
        + " despite no active FSM. Delegating to resubmit path.");
    return true;
}
_followerReplaceSpecs.TryRemove(matchedEntry, out _);
return false;
```

**Acceptance Criteria:**
1. Method `ShouldRescuePendingCancelSpec` exists as `private` in the same partial class.
2. Cyclomatic complexity (cyc) of `ShouldRescuePendingCancelSpec` ≤ 4 (verified by `complexity_audit.py`).
3. Build 973 / Build 947 meta-purge guard semantics preserved exactly:
   - `true` path → rescued spec, `Print` log emitted, no `TryRemove`.
   - `false` path → orphaned spec removed via `_followerReplaceSpecs.TryRemove`.
4. Zero `lock()` blocks introduced.
5. All string literals are ASCII-only (no Unicode, no curly quotes).
6. `dotnet build` passes with zero errors after extraction.

---

## Ticket T4 — Extract `HandleEntryNotFilledRollback`

| Field | Value |
|---|---|
| **ID** | EPIC-W7-123-T4 |
| **Type** | extraction |
| **File** | `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **CYC Target** | ≤ 1 |
| **Jane Street** | `[NoInlining]` (cold path — UI rendering + logging) |
| **Depends On** | None |

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void HandleEntryNotFilledRollback(string matchedEntry, string acctName)
```

**Extracted Body:**
```csharp
HandleMatchedFollower_DeltaRollback(matchedEntry);
Print(string.Format(
    "[SIMA] Follower entry cancelled: {0} on {1}. Reaper monitoring.",
    matchedEntry, acctName));
Draw.TextFixed(
    this,
    "SIMA_DESYNC_" + acctName,
    "(!) FOLLOWER DESYNC: " + acctName,
    TextPosition.TopLeft,
    Brushes.Red,
    new SimpleFont("Arial", 11),
    Brushes.Transparent,
    Brushes.Transparent,
    50);
```

**Acceptance Criteria:**
1. Method `HandleEntryNotFilledRollback` exists as `private` in the same partial class.
2. Cyclomatic complexity (cyc) of `HandleEntryNotFilledRollback` ≤ 1 (linear path, verified by `complexity_audit.py`).
3. `[MethodImpl(MethodImplOptions.NoInlining)]` attribute applied (cold path with UI rendering).
4. `HandleMatchedFollower_DeltaRollback` is called before any logging or UI operations.
5. `Draw.TextFixed` parameters match Phase 2 specification exactly (font="Arial", size=11, color=Red, opacity=50).
6. All string literals are ASCII-only.
7. `dotnet build` passes with zero errors after extraction.

---

## Ticket T5 — Extract `HandleTerminalFollowerOrder`

| Field | Value |
|---|---|
| **ID** | EPIC-W7-123-T5 |
| **Type** | extraction |
| **File** | `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **CYC Target** | ≤ 1 |
| **Jane Street** | `[NoInlining]` (cold path — logging + ghost-ref cleanup) |
| **Depends On** | None |

**Signature:**
```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void HandleTerminalFollowerOrder(Order order, string acctName, string reason)
```

**Extracted Body:**
```csharp
Print(string.Format(
    "[SIMA] Follower order terminal: {0} on {1} ({2}) | Id={3}",
    order.Name, acctName, reason, order.OrderId));
RemoveGhostOrderRef(order, reason);
```

**Acceptance Criteria:**
1. Method `HandleTerminalFollowerOrder` exists as `private` in the same partial class.
2. Cyclomatic complexity (cyc) of `HandleTerminalFollowerOrder` ≤ 1 (linear path, verified by `complexity_audit.py`).
3. `[MethodImpl(MethodImplOptions.NoInlining)]` attribute applied (cold path with logging/cleanup).
4. `Print` is called before `RemoveGhostOrderRef` (logging precedes mutation).
5. `string.Format` format string matches Phase 2 specification exactly (4 positional args).
6. All string literals are ASCII-only.
7. `dotnet build` passes with zero errors after extraction.

---

## Ticket T6 — Rewrite Parent `HandleMatchedFollowerOrder` Body

| Field | Value |
|---|---|
| **ID** | EPIC-W7-123-T6 |
| **Type** | refactor |
| **File** | `src/V12_002.Orders.Callbacks.AccountOrders.cs` |
| **CYC Target** | ≤ 5 |
| **Jane Street** | Defense in depth — 3-layer guard preserved; single-responsibility body |
| **Depends On** | T1, T2, T3, T4, T5 |

**Rewritten Body:**
```csharp
private void HandleMatchedFollowerOrder(
    string matchedEntry,
    PositionInfo matchedPos,
    Order order,
    string acctName,
    string reason
)
{
    if (ProcessFollowerCancellationSafe(matchedEntry, matchedPos, order, acctName, reason))
        return;

    if (IsEntryOrderMatch(matchedEntry, matchedPos, order, out _))
    {
        entryOrders.TryRemove(matchedEntry, out _);
        if (!IsAnyFollowerBracketActive(acctName))
        {
            if (!ShouldRescuePendingCancelSpec(matchedEntry, order))
                return;
        }
        HandleEntryNotFilledRollback(matchedEntry, acctName);
    }
    else
    {
        HandleTerminalFollowerOrder(order, acctName, reason);
    }
}
```

**Acceptance Criteria:**
1. `HandleMatchedFollowerOrder` body replaced with the 5-helper orchestration body shown above.
2. Cyclomatic complexity (cyc) of `HandleMatchedFollowerOrder` ≤ 5 (verified by `complexity_audit.py`).
3. All 3 callers (`ProcessQueuedAccountOrder`, `ProcessAccountOrderQueue`, `ProcessAccountOrder_EnqueueTerminalUpdate`) unchanged — no signature modification.
4. 3-layer defense preserved in order: `ProcessFollowerCancellationSafe` → `IsAnyFollowerBracketActive` → `ShouldRescuePendingCancelSpec`.
5. Zero `lock()` blocks.
6. `entryOrders.TryRemove` still called inside the `IsEntryOrderMatch` true branch (cleanup preserved).
7. `dotnet build` passes with zero errors.
8. `complexity_audit.py` reports max CYC ≤ 5 for all 6 methods (parent + 5 helpers).

---

## Ticket T7 — xUnit Tests for Boolean Predicate Helpers

| Field | Value |
|---|---|
| **ID** | EPIC-W7-123-T7 |
| **Type** | test |
| **File** | `tests/V12_Performance.Tests/Core/EPICW7123_HandleMatchedFollowerOrderTests.cs` |
| **CYC Target** | N/A (test file) |
| **Jane Street** | `[Fact]` + `Assert.True` / `Assert.False` — xUnit only, no NUnit/MSTest |
| **Depends On** | T1, T2, T3 |

**Test Cases Required:**

| Test Name | Target Method | Scenario | Expected |
|---|---|---|---|
| `IsEntryOrderMatch_ReturnsTrue_WhenEntryExistsAndOrderIdMatches` | `IsEntryOrderMatch` | Key present, OrderId match, EntryFilled=false | `Assert.True` |
| `IsEntryOrderMatch_ReturnsFalse_WhenKeyMissing` | `IsEntryOrderMatch` | Key absent in `entryOrders` | `Assert.False` |
| `IsEntryOrderMatch_ReturnsFalse_WhenEntryFilled` | `IsEntryOrderMatch` | EntryFilled=true | `Assert.False` |
| `IsAnyFollowerBracketActive_ReturnsTrue_WhenActiveStatePresent` | `IsAnyFollowerBracketActive` | Bracket with State=Active and matching acctName | `Assert.True` |
| `IsAnyFollowerBracketActive_ReturnsFalse_WhenNoBracketsForAccount` | `IsAnyFollowerBracketActive` | No brackets for acctName | `Assert.False` |
| `ShouldRescuePendingCancelSpec_ReturnsTrue_WhenPendingCancelOrderIdMatches` | `ShouldRescuePendingCancelSpec` | Spec in PendingCancel state, CancellingOrderId matches | `Assert.True` |
| `ShouldRescuePendingCancelSpec_ReturnsFalse_AndRemovesSpec_WhenNoMatch` | `ShouldRescuePendingCancelSpec` | No matching spec | `Assert.False` + spec removed |

**Acceptance Criteria:**
1. Test file uses xUnit framework ONLY (`[Fact]`, `Assert.True`, `Assert.False`, `Assert.Equal`).
2. NEVER NUnit (`[Test]`, `Assert.That`) or MSTest (`[TestMethod]`).
3. All 7 test cases above implemented.
4. `dotnet test` passes with 7/7 tests green.
5. Test file ASCII-only string literals.
6. No test introduces `lock()` or shared mutable state between tests.

---

## CYC Reduction Summary

| Method | Original CYC | Projected CYC | Reduction |
|---|---|---|---|
| `HandleMatchedFollowerOrder` | 14 | 5 | -9 |
| `IsEntryOrderMatch` | — | 5 | extraction |
| `IsAnyFollowerBracketActive` | — | 5 | extraction |
| `ShouldRescuePendingCancelSpec` | — | 4 | extraction |
| `HandleEntryNotFilledRollback` | — | 1 | extraction |
| `HandleTerminalFollowerOrder` | — | 1 | extraction |
| **max_cyc_projected** | **14** | **5** | **64%** |

**cyc compliance:** All methods ≤ 8 (Jane Street strict standard). max_cyc_projected = 5.

---

## MCP Evidence

### `resolve_repo`
- Repo: `antigravityos187-sketch/universal-or-strategy`
- Indexed: true | Symbol count: 5,147 | File count: 2,000 | Backend: sqlite

### `get_symbol_complexity` — HandleMatchedFollowerOrder
- Result: symbol not found in index
- Reason: C# partial class AST gap — method body complexity not persisted in jCodemunch index for this file. Consistent with Phase 2 findings (`get_extraction_candidates` also returned empty). Complexity data sourced from Phase 0 hotspot analysis (CYC=14 confirmed).

### `get_extraction_candidates` — src/V12_002.Orders.Callbacks.AccountOrders.cs
- Result: `candidates=[]` (same AST index gap — no per-method complexity rows for this file)
- Verdict: Fresh extraction opportunity confirmed for all 5 helpers (no pre-existing splits).

---

## Sequential Thinking Evidence

| Thought | Focus | Outcome |
|---|---|---|
| 1 | Ticket count and structure strategy — 7 tickets for 5 extractions + 1 parent rewrite + 1 test | Mapped all 5 helpers to extraction tickets; T6=refactor; T7=test |
| 2 | Dependency chain validation — T1–T5 parallel, T6 depends on T1–T5, T7 depends on T1–T3 | Execution order confirmed; CYC targets per ticket assigned |
| 3 | Final validation — ticket_count=7, required keywords present, acceptance criteria verifiable | All checks pass; document ready to write |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 2.0 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **Lane** | P4-L8 |
| **Epic** | EPIC-W7-123 |
| **ticket_count** | 7 |
| **extraction_count** | 5 |
| **cyc_original** | 14 |
| **max_cyc_projected** | 5 |
| **cyc_reduction** | 64% |
| **MCP Tools Called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **Sequential Thoughts** | 3 |
