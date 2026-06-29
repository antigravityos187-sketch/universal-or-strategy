# EPIC-W7-150 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-150/01-scope-boundary.md

---

## Target Method Table

| Field | Value |
|---|---|
| **Method** | `ProcessQueuedExecution_HandleFleetBrackets` |
| **File** | `src/V12_002.UI.Compliance.cs` |
| **Line** | 486–517 |
| **Signature** | `private void ProcessQueuedExecution_HandleFleetBrackets(QueuedAccountExecution item)` |
| **CYC Baseline** | 10 |
| **CYC Target** | <= 8 |
| **CYC Over Threshold** | 2 |
| **Caller Count** | 1 (`ProcessQueuedExecution`, line 787, same file) |
| **Scope Boundary** | PASS (01-scope-boundary.md) |

---

## Complexity Drivers

The method body (31 lines) was analyzed via `get_symbol_source`. CYC=10 is accounted as follows:

| # | Code Pattern | CYC Contribution | Location |
|---|---|---|---|
| 1 | Base complexity | +1 | (always) |
| 2 | `if (filledOrder != null && filledOrder.OrderState == OrderState.Filled)` — `&&` | +2 | Line 490 (if + &&) |
| 3 | `foreach (var kvp in entryOrders.ToArray())` | +1 | Line 493 |
| 4 | `if (kvp.Value == filledOrder)` | +1 | Line 495 |
| 5 | `if (activePositions.TryGetValue(...) && pos.IsFollower && !pos.EntryFilled)` | +3 | Line 500–504 (if + && + &&) |
| 6 | `item.EventArgs.Execution != null ? price : 0` — ternary | +1 | Line 506 |
| 7 | `catch (Exception ex)` block | +1 | Line 513 |
| **Total** | | **10** | |

**Primary hotspot**: The triple-compound follower eligibility guard (driver #5, +3 CYC) is the dominant contributor and the primary extraction target. By replacing it with a single boolean method call, the parent loses 2 CYC points (the two `&&` operators; the `if` wrapper becomes a single conditional on the helper return value).

**Secondary target**: The cold `catch`/`Print` logging path (driver #7) is a Jane Street carl_cook mandatory extraction (`NoInlining` cold path rule).

---

## Extraction Plan

| Helper | Responsibility | Extracted From | CYC Projected | Modifier |
|---|---|---|---|---|
| `TryGetEligibleFollowerPosition(string fleetKey, out V12Position pos)` | Evaluates the compound follower eligibility guard: `TryGetValue && IsFollower && !EntryFilled`. Returns `bool`. Hot path. | Compound `if` at lines 500–504 | 3 | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| `LogFleetBracketError(Exception ex)` | Cold error logging: wraps `Print(string.Format("[SIMA V12.7] Error...", ex.Message))`. Cold path — never called on success path. | `catch` block body at lines 514–516 | 1 | `[MethodImpl(MethodImplOptions.NoInlining)]` |

### Extraction Detail

#### Helper 1: `TryGetEligibleFollowerPosition`

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool TryGetEligibleFollowerPosition(string fleetKey, out V12Position pos)
{
    return activePositions.TryGetValue(fleetKey, out pos)
        && pos.IsFollower
        && !pos.EntryFilled;
}
```

**Parent delta**: Replaces `if (activePositions.TryGetValue(fleetKey, out var pos) && pos.IsFollower && !pos.EntryFilled)` with `if (TryGetEligibleFollowerPosition(fleetKey, out var pos))`. Removes 2 `&&` operators from parent = -2 CYC.

**Caller site after refactor**:
```csharp
if (TryGetEligibleFollowerPosition(fleetKey, out var pos))
{
    double fleetFillPrice = item.EventArgs.Execution != null
        ? item.EventArgs.Execution.Price
        : 0;
    SymmetryGuardOnFollowerFill(fleetKey, pos, fleetFillPrice);
}
```

#### Helper 2: `LogFleetBracketError`

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void LogFleetBracketError(Exception ex)
{
    Print(string.Format("[SIMA V12.7] Error in fleet bracket submission: {0}", ex.Message));
}
```

**Parent delta**: The `catch` clause itself stays; body becomes a single method call. No CYC reduction in parent (catch itself contributes +1 regardless), but satisfies Jane Street cold-logging extraction mandate.

**Caller site after refactor**:
```csharp
catch (Exception ex)
{
    LogFleetBracketError(ex);
}
```

---

## Max CYC Projected Table

| Symbol | CYC Before | CYC After | Status |
|---|---|---|---|
| `ProcessQueuedExecution_HandleFleetBrackets` | 10 | **8** | PASS (<= 8) |
| `TryGetEligibleFollowerPosition` (new) | — | **3** | PASS (<= 8) |
| `LogFleetBracketError` (new) | — | **1** | PASS (<= 8) |
| **MAX CYC PROJECTED** | | **8** | **PASS** |

### CYC Arithmetic (Parent)

```
Baseline:                           10
- Extract TryGetEligibleFollower:   -2  (removes 2x && from compound if)
= Final parent CYC:                  8  [THRESHOLD MET]
```

---

## Jane Street KB Compliance Table

| Rule | Source | Application | Status |
|---|---|---|---|
| Extract cold logging out-of-line; `NoInlining` cold | carl_cook | `LogFleetBracketError` decorated `[MethodImpl(NoInlining)]` | PASS |
| `AggressiveInlining` hot path | carl_cook | `TryGetEligibleFollowerPosition` decorated `[MethodImpl(AggressiveInlining)]` | PASS |
| Zero-alloc hot path | carl_cook | No new allocations; `out` parameter reuses stack slot; no `string.Format` on hot path | PASS |
| Avoid LINQ on hot path | carl_cook | No new LINQ introduced; existing `ToArray()` is pre-existing (not in scope) | PASS |
| No new `lock()` blocks | gjengset | Pure extract-method refactor; no new synchronization primitives | PASS |
| Single responsibility per helper | trading_billions | Each helper has exactly one concern (eligibility check vs cold logging) | PASS |
| Each helper CYC <= 8 | trading_billions | Max helper CYC = 3; all <= 8 | PASS |
| Parent CYC <= 8 after extraction | trading_billions | Parent CYC = 8 | PASS |

---

## MCP Evidence

### Repo Resolution
- **Tool**: `mcp__jcodemunch-mcp__resolve_repo`
- **Result**: `antigravityos187-sketch/universal-or-strategy` — indexed, 5147 symbols, 2000 files
- **Status**: Confirmed

### Symbol Source
- **Tool**: `mcp__jcodemunch-mcp__get_symbol_source`
- **Symbol ID**: `src/V12_002.UI.Compliance.cs::V12_002.ProcessQueuedExecution_HandleFleetBrackets#method`
- **Line range**: 486–517 (31 lines)
- **Freshness**: fresh
- **Content hash**: `750d0eb6c4b2875fc5590b7d6b470aa675c8f54d310c08cd3710be141cf99554`

### Call Hierarchy
- **Tool**: `mcp__jcodemunch-mcp__get_call_hierarchy` (direction=callers, depth=1)
- **Caller count**: 1
- **Only caller**: `ProcessQueuedExecution` at line 787 (`src/V12_002.UI.Compliance.cs`) — resolution: `ast_resolved`
- **Callee count**: 0 (direct callees not tracked at this depth)
- **Blast radius confirmed**: Single file, single upstream caller — no signature changes required

### Scope Boundary
- **File**: `docs/brain/EPIC-W7-150/01-scope-boundary.md`
- **Verdict**: PASS
- **V12.23 compliance**: All 6 checks passed

---

## Sequential Thinking Evidence

Three thoughts executed via `mcp__sequential-thinking__sequentialthinking`:

| Thought | Focus | Conclusion |
|---|---|---|
| 1 | Complexity driver enumeration from source code analysis | 7 distinct CYC contributors identified; primary hotspot = triple-compound `&&` guard (+3 CYC) |
| 2 | Extraction strategy selection (tested 3 alternatives) | 2-helper plan: eligibility guard extraction (- 2 CYC parent) + cold logging (Jane Street mandatory); parent CYC reaches 8 |
| 3 | CYC validation across all symbols | All projected CYCs verified: parent=8, helper1=3, helper2=1; max=8; all <= 8 threshold |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-150 |
| **MCP Tools Used** | resolve_repo, get_symbol_source, search_symbols, get_call_hierarchy, sequentialthinking (3x) |
| **Max CYC Projected** | 8 |
| **Extraction Count** | 2 |
| **Jane Street Rules Applied** | carl_cook (3), gjengset (1), trading_billions (3) |
