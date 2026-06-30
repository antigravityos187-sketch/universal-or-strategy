# Phase 4 Tickets — EPIC-W7-026

**Epic**: EPIC-W7-026
**Method**: ProcessQueuedAccountOrder
**Source File**: V12_002.Orders.Callbacks.AccountOrders.cs
**Original CYC**: 17
**Wave**: 7 | **Phase**: 4

---

## Ticket Summary

ticket_count: 3

---

## Tickets

### Ticket 1

**ticket_id**: T1
**helper_name**: `IsValidQueuedOrderForThisInstrument`
**concern**: Validate that the queued item and its nested EventArgs/Order are non-null and that the order's instrument matches the current strategy instrument. Collapses two sequential early-return guard clauses into a single named boolean predicate.
**lines_to_move**: The two consecutive `if`-return guard blocks at the top of `ProcessQueuedAccountOrder` (lines 1054–~1060):
  1. `if (item?.EventArgs?.Order == null) return;` — null-safety guard
  2. `if (!Instrument.MasterInstrument.Name.Equals(item.EventArgs.Order.Instrument.MasterInstrument.Name, StringComparison.OrdinalIgnoreCase)) return;` — instrument filter guard

Both guards are moved verbatim into the helper body; parent is replaced with:
```csharp
if (!IsValidQueuedOrderForThisInstrument(item))
    return;
```

**Signature**:
```csharp
private bool IsValidQueuedOrderForThisInstrument(QueuedAccountOrderUpdate item)
```

**cyc_reduction**: 2 (removes 2 if-return decision nodes from parent)
**projected_helper_cyc**: 3 (2 decision nodes + base 1)
**xUnit test requirement**: `[Fact]` tests covering (a) null item, (b) null EventArgs, (c) null Order, (d) instrument mismatch → all return `false`; (e) valid item matching instrument → returns `true`.

---

### Ticket 2

**ticket_id**: T2
**helper_name**: `TryMatchFollowerPositionInSnapshot`
**concern**: Scan the pre-allocated position snapshot array to find the first matching follower position for the given order. Applies a stale-key guard, a compound 3-predicate filter (`IsFollowerPosition`, null check, account name match), and `TryFindOrderInPosition` identity search. Populates `matchedEntry` and `matchedPos` via `out` parameters; returns `true` on first match, `false` if no match found.
**lines_to_move**: The entire `foreach` scan loop body over the snapshot array (lines ~1075–~1095):
  1. Stale-key guard: `if (string.IsNullOrEmpty(kvp.Key)) continue;`
  2. Compound filter: `if (!IsFollowerPosition(kvp.Value) || kvp.Value == null || kvp.Value.Account?.Name != acctName) continue;`
  3. Identity search: `if (TryFindOrderInPosition(kvp.Value, order.Name, out …)) { matchedEntry = kvp.Key; matchedPos = kvp.Value; return true; }`
  4. Post-loop: `matchedEntry = string.Empty; matchedPos = null; return false;`

**Signature**:
```csharp
private bool TryMatchFollowerPositionInSnapshot(
    QueuedAccountOrderUpdate item,
    Order order,
    KeyValuePair<string, PositionInfo>[] snapshot,
    out string matchedEntry,
    out PositionInfo matchedPos)
```

**cyc_reduction**: 6 (removes `foreach`=1 + stale-key guard=1 + IsFollower predicate=1 + null check=1 + account match=1 + TryFindOrder result=1 from parent)
**projected_helper_cyc**: 7 (foreach=1, stale-key=1, IsFollower=1, null check=1, account match=1, TryFindOrder result=1, base=1)
**xUnit test requirement**: `[Fact]` tests covering (a) empty snapshot → returns `false`, out params null/empty; (b) snapshot with stale key (null/empty) → skipped, returns `false`; (c) snapshot with non-follower position → skipped, returns `false`; (d) account mismatch → skipped, returns `false`; (e) matching follower with matching order → returns `true`, out params populated; (f) multiple entries, match on second → returns `true` with correct entry.

---

### Ticket 3

**ticket_id**: T3
**helper_name**: `DispatchMatchedFollowerResult`
**concern**: Route the result of the snapshot scan to either `HandleMatchedFollowerOrder` (matched path) or `ExecuteFollowerCascadeCleanup` (orphan/unmatched path) based on whether `matchedEntry` and `matchedPos` are valid. Makes the cascade fallback explicit and auditable; eliminates inline if-else dispatch from parent.
**lines_to_move**: The dispatch if-else block at the bottom of `ProcessQueuedAccountOrder` (lines ~1096–~1101):
```csharp
if (!string.IsNullOrEmpty(matchedEntry) && matchedPos != null)
    HandleMatchedFollowerOrder(matchedEntry, matchedPos, order, acctName, reason);
else
    ExecuteFollowerCascadeCleanup(order, acctName, reason, snapshot);
```

**Signature**:
```csharp
private void DispatchMatchedFollowerResult(
    string matchedEntry,
    PositionInfo matchedPos,
    Order order,
    string acctName,
    string reason,
    KeyValuePair<string, PositionInfo>[] snapshot)
```

**cyc_reduction**: 2 (removes matched-check=1 + if-else branch=1 from parent; the single combined condition `!IsNullOrEmpty && != null` counts as 2 decision nodes)
**projected_helper_cyc**: 4 (IsNullOrEmpty check=1, null-ref check=1, if-else branch=1, base=1)
**xUnit test requirement**: `[Fact]` tests covering (a) empty matchedEntry → `ExecuteFollowerCascadeCleanup` called; (b) null matchedPos → `ExecuteFollowerCascadeCleanup` called; (c) valid matchedEntry + matchedPos → `HandleMatchedFollowerOrder` called with correct arguments.

---

## Extraction Summary

| Method | Pre-Extraction CYC | Post-Extraction CYC | Threshold | Status |
|---|---|---|---|---|
| `IsValidQueuedOrderForThisInstrument` (new) | — | **3** | ≤ 8 | ✅ PASS |
| `TryMatchFollowerPositionInSnapshot` (new) | — | **7** | ≤ 8 | ✅ PASS |
| `DispatchMatchedFollowerResult` (new) | — | **4** | ≤ 8 | ✅ PASS |
| `ProcessQueuedAccountOrder` (parent) | 17 | **4** | ≤ 8 | ✅ PASS |

**projected_parent_cyc_after_all**: 4

CYC reduction: 17 → 4 in parent (−13). max_cyc_projected across all artifacts = **7** (`TryMatchFollowerPositionInSnapshot`). Jane Street strict threshold of 8 satisfied on all methods.

### Execution Order

Tickets MUST be executed in sequence: T1 → T2 → T3. Each ticket's extraction is a prerequisite for verifying the parent's final CYC. Apply `dotnet csharpier format src/` after all three extractions before pushing.

### Post-Extraction Verification Checklist

- [ ] `dotnet build` — zero errors
- [ ] `dotnet test` — 100% pass (xUnit only: `[Fact]`, `Assert.Equal()`)
- [ ] `python scripts/complexity_audit.py` — `ProcessQueuedAccountOrder` CYC ≤ 8
- [ ] `dotnet csharpier check src/` — zero formatting issues
- [ ] `grep -n "lock(" src/V12_002.Orders.Callbacks.AccountOrders.cs` — zero matches
- [ ] ASCII scan on new helpers — zero non-ASCII characters
- [ ] `powershell -File .\deploy-sync.ps1` — NinjaTrader hard-link sync

---

## Jane Street Compliance Summary

| Rule | Status |
|---|---|
| CYC ≤ 8 for all methods (parent + helpers) | **PASS** — max 7 |
| Single-responsibility per helper | **PASS** — guard / scan / dispatch |
| Lock-free / Actor pattern preserved | **PASS** — no `lock()` blocks |
| ASCII-only string literals | **PASS** — no Unicode or curly quotes |
| xUnit tests only (`[Fact]`/`Assert.Equal()`) | **REQUIRED** — each ticket must include tests |
| No scope creep (V12.23) | **PASS** — all helpers private, same partial class, no external interface changes |
| Zero cross-file blast radius | **PASS** — Phase 2/3 confirmed via `find_references` = 0 cross-file edges |

---

## Agent Tracking

- **Agent Name**: v12-phase4-tickets
- **Wave**: 7
- **Phase**: 4
- **Epic**: EPIC-W7-026
- **Method**: ProcessQueuedAccountOrder
- **Source File**: `src/V12_002.Orders.Callbacks.AccountOrders.cs`
- **Original CYC**: 17
- **ticket_count**: 3
- **projected_parent_cyc_after_all**: 4
- **max_cyc_projected**: 7
- **dna_verdict**: PASS (from Phase 3)
- **jcodemunch tools called**: `resolve_repo`, `get_symbol_complexity`, `get_extraction_candidates`
- **sequential-thinking calls**: 4 (1 probe + 3 validation thoughts)
- **Generated**: 2026-07-01T00:00:00Z
