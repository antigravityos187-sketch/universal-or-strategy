# Phase 2: Architecture Plan — EPIC-W7-088

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T03:10:00Z
**Input:** docs/brain/EPIC-W7-088/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `SubmitRepairOrderWithAuthorization`
- **Source File:** [`src/V12_002.REAPER.Repair.cs`](../../src/V12_002.REAPER.Repair.cs:147)
- **Class:** `V12_002 : Strategy` (NinjaTrader partial class)
- **Original CYC:** 34
- **Lines:** 147–241 (95 lines of logic)

### jcodemunch get_context_bundle / search_symbols result

Symbol confirmed at `src/V12_002.REAPER.Repair.cs::V12_002.SubmitRepairOrderWithAuthorization#method` (line 147).
Full source retrieved. Docstring: _"Submits repair order with authorization validation. Checks FSM state, dispatch reservation, metadata guard, then creates and submits order."_

Signature (unchanged after refactor):
```csharp
private void SubmitRepairOrderWithAuthorization(
    string accountName,
    PositionInfo repairPos,
    string repairEntryName,
    OrderType orderType,
    double limitPrice,
    double stopPrice
)
```

### jcodemunch get_call_hierarchy result

| Direction | Symbol | File | Depth | Resolution |
|---|---|---|---|---|
| Caller (depth 1) | `ExecuteReaperRepair` | `src/V12_002.REAPER.Repair.cs:246` | 1 | ast_resolved |
| Caller (depth 2) | `ProcessReaperRepairQueue` | `src/V12_002.REAPER.Repair.cs:21` | 2 | ast_resolved |
| Callee | `_dispatchSyncPendingExpKeys` | `src/V12_002.cs:687` | 1 | ast_inferred |
| Callee | `ExpKey` | `src/V12_002.SIMA.cs:209` | 1 | ast_inferred |
| Callee | `LogBuffer.Format` | `src/V12_002.Perf.LogBuffer.cs:28` | 1 | ast_inferred |
| Callee | `MetadataGuardRepairAuthorized` | `src/V12_002.MetadataGuard.cs:164` | 1 | ast_inferred |

**1 direct caller** (`ExecuteReaperRepair`). Signature must remain identical.

### jcodemunch get_dependency_graph result

File-level dependency graph returned 0 edges — `src/V12_002.REAPER.Repair.cs` is a C# partial class fragment. Its dependencies (fields `_followerBrackets`, `_dispatchSyncPendingExpKeys`, `activePositions`, `entryOrders`) are declared in `src/V12_002.cs` and resolved at compile time via the partial class mechanism, not through file-level imports. Cross-file blast radius = 0 import edges, but 32 files share mutable state (per hotspot analysis H1–H5).

### jcodemunch get_extraction_candidates result

No candidates returned (min_callers=1, min_complexity=3). This is consistent with the file being a partial class fragment — the index cannot resolve internal callee counts across partial class boundaries. Extraction plan is driven by branch analysis of the full source (obtained via get_symbol_source).

---

## Sequential Thinking Summary

**Thought 1:** Method CYC 34 arises from 6 distinct logical segments: account resolution + null guard, order creation + null guard, FSM state LINQ scan (4-condition OR), FSM-race guard fallback (dispatch-pending + activePositions), MetadataGuard check, and state mutation + Submit + log. Expect 6 helpers.

**Thought 2:** Guard clause extraction — `TryResolveRepairAccount` (CYC 2) isolates account null-check; `CreateRepairOrder` (CYC 3) wraps direction/quantity resolution + CreateOrder call + null-check on result. These eliminate the first 3 branches from the parent.

**Thought 3:** FSM authorization extraction — `HasActiveFsmForAccount` (CYC 5) encapsulates the 4-state LINQ predicate; `ResolveRepairAuthorization` (CYC 5) encapsulates the `!hasActiveFsm` fallback path (dispatch-pending check, activePositions scan, abort guard, fallback Print). These eliminate 8 branches from the parent.

**Thought 4:** Submission and logging extraction — `PrepareAndRegisterRepairOrder` (CYC 1) handles `BracketSubmitted = false` + `entryOrders[...]` write; `LogRepairOrderSubmitted` (CYC 2) handles the formatted Print with ternary. Parent reduced to 4-branch orchestration, CYC = 5.

**Thought 5:** Jane Street alignment verified. All helpers ≤ 8. No locks introduced. No new allocations on hot paths. TOCTOU (H1) and stale-entryOrders (H3) are pre-existing risks — out of scope per V12.23. Signature unchanged. max_cyc_projected = 5, extraction_count = 6. PASS.

---

## Extraction Plan

| # | Helper Method Name | Responsibility | Extracted Branches | Projected CYC |
|---|---|---|---|---|
| 1 | `TryResolveRepairAccount` | Null-checks `repairPos.ExecutingAccount`; assigns `targetAcct`; prints failure and returns `false` on null | 1 (null guard) | **2** |
| 2 | `CreateRepairOrder` | Resolves `OrderAction` from `Direction`, reads `TotalContracts`, calls `targetAcct.CreateOrder(...)`, null-checks result, prints failure | 2 (direction ternary + null guard) | **3** |
| 3 | `HasActiveFsmForAccount` | LINQ scan of `_followerBrackets.Values` for `Active`, `Accepted`, `Submitted`, or `Replacing` state on the given account | 5 (LINQ null + 4-state OR) | **5** |
| 4 | `ResolveRepairAuthorization` | When no active FSM: checks `_dispatchSyncPendingExpKeys.ContainsKey`, scans `activePositions`, aborts if neither exists; prints guard messages | 4 (hasActiveFsm + ContainsKey + Any + abort AND) | **5** |
| 5 | `PrepareAndRegisterRepairOrder` | Resets `repairPos.BracketSubmitted = false`; writes `entryOrders[repairEntryName] = repairEntry` | 0 (pure mutation) | **1** |
| 6 | `LogRepairOrderSubmitted` | Formats and emits the success `Print(...)` with `OrderType.Market` ternary | 1 (orderType ternary) | **2** |

All 6 helpers are `private` methods in the same partial class file (`src/V12_002.REAPER.Repair.cs`). No cross-file extraction.

---

## Parent Method After Extraction

**Remaining orchestration logic (8 lines of actual code):**

```csharp
private void SubmitRepairOrderWithAuthorization(
    string accountName, PositionInfo repairPos, string repairEntryName,
    OrderType orderType, double limitPrice, double stopPrice)
{
    if (!TryResolveRepairAccount(repairPos, accountName, out Account targetAcct))    // branch 1
        return;
    if (!CreateRepairOrder(targetAcct, repairPos, orderType, limitPrice, stopPrice,
        repairEntryName, out Order repairEntry))                                       // branch 2
        return;
    bool hasActiveFsm = HasActiveFsmForAccount(accountName);
    if (!ResolveRepairAuthorization(accountName, hasActiveFsm))                       // branch 3
        return;
    if (!MetadataGuardRepairAuthorized(accountName, "ExecuteReaperRepair"))           // branch 4
        return;
    PrepareAndRegisterRepairOrder(repairPos, repairEntryName, repairEntry);
    targetAcct.Submit(new[] { repairEntry });
    LogRepairOrderSubmitted(accountName, repairEntryName, repairEntry.OrderAction,
        repairEntry.Quantity, orderType, repairPos);
}
```

- **Remaining logic:** Pure orchestration — guard gates → prepare → submit → log
- **Projected CYC:** **5** (4 if-return guards = 4 branches + 1 baseline)

---

## max_cyc_projected: 5
## extraction_count: 6

---

## Helper Method Signatures

```csharp
// Helper 1
private bool TryResolveRepairAccount(
    PositionInfo repairPos,
    string accountName,
    out Account targetAcct)

// Helper 2
private bool CreateRepairOrder(
    Account targetAcct,
    PositionInfo repairPos,
    OrderType orderType,
    double limitPrice,
    double stopPrice,
    string repairSignal,
    out Order repairEntry)

// Helper 3
private bool HasActiveFsmForAccount(string accountName)

// Helper 4
private bool ResolveRepairAuthorization(string accountName, bool hasActiveFsm)

// Helper 5
private void PrepareAndRegisterRepairOrder(
    PositionInfo repairPos,
    string repairEntryName,
    Order repairEntry)

// Helper 6
private void LogRepairOrderSubmitted(
    string accountName,
    string repairEntryName,
    OrderAction action,
    int quantity,
    OrderType orderType,
    PositionInfo repairPos)
```

---

## Risk Notes for Phase 5 Implementation

| Risk | Hotspot | Action |
|---|---|---|
| TOCTOU double FSM scan (H1) | `HasActiveFsmForAccount` + `MetadataGuardRepairAuthorized` re-query same dict | **Out of scope** per V12.23. Document in helper docstring. Do NOT fix in this epic. |
| Stale entryOrders on submit failure (H3) | `PrepareAndRegisterRepairOrder` writes before Submit | **Out of scope** per V12.23. Existing behavior preserved. |
| `BracketSubmitted` mutation (H4) | `PrepareAndRegisterRepairOrder` | Preserved as-is. Document thread-safety expectation in docstring. |

---

## Jane Street Alignment

| Principle | Status |
|---|---|
| CYC ≤ 8 achieved | **YES** — max helper CYC = 5, parent CYC = 5 |
| Single-responsibility per helper | **YES** — each helper has exactly one named concern |
| Lock-free / Actor pattern preserved | **YES** — no new `lock()` blocks; `ConcurrentDictionary` writes preserved as-is per B966 comment |
| Illegal states unrepresentable | **YES** — `bool` return pattern from helpers enforces null/null-order preconditions structurally; downstream helpers unreachable with invalid state |
| Zero-allocation hot paths | **YES** — no new heap allocations; string formatting confined to `LogRepairOrderSubmitted` (success path only) |

---

## Scope Boundary Compliance (V12.23)

- Single method targeted: `SubmitRepairOrderWithAuthorization` ✅
- All helpers are `private` in same partial class file ✅
- No caller modifications (`ExecuteReaperRepair` unaffected) ✅
- No sibling method modifications ✅
- No cross-file refactoring ✅

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | ~20 |
| **Execution Time** | 2026-06-29T03:10:00Z |
| **Wave** | 7 |
| **Phase** | 2 |
| **jcodemunch tools called** | search_symbols, get_symbol_source, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 6 |
| **max_cyc_projected** | 5 |
| **parent_cyc_projected** | 5 |
