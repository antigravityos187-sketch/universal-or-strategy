# Phase 4: Ticket Definitions — EPIC-W7-147

**agent_name:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Epic:** EPIC-W7-147
**Method:** `ProcessQueuedExecution_HandleFleetOCO`
**Source:** `src/V12_002.UI.Compliance.cs` (lines 698–727)
**Original CYC:** 15 | **Target CYC:** ≤ 8 | **Max Projected CYC:** 5
**dna_verdict:** PASS | **extraction_count:** 3 | **ticket_count:** 4

---

## Sequential Thinking Evidence

**ST-thought-1 (Branch Analysis):** CYC=15 deconstructed into three extraction zones:
(A) 4-&&+1-|| compound guard (actionability check — ~5 CYC),
(B) StartsWith-based name classifier with nested && guards (classification — ~5 CYC),
(C) dispatch routing to HandleFleetStopFill / HandleFleetTargetFill (dispatch — ~4 CYC).
Parent orchestration retains: try/catch + if(guard) + 3 local variable declarations + 3 helper calls = CYC 3.

**ST-thought-2 (Helper Design):** Three private helpers designed with single-responsibility alignment:
`IsOcoOrderActionable` (guard, CYC=5), `GetOcoOrderFleetType` (classifier, CYC=5, zero-allocation enum return),
`DispatchOcoFleetOrder` (dispatch, CYC=4). Supporting enum `OcoFleetOrderType { Stop, Target, Unknown }` added.
All helpers private to same partial class; no new public API surface introduced.

**ST-thought-3 (Verification):** All 4 methods project CYC ≤ 5. Max CYC = 5 ≤ 8 threshold.
dna_verdict=PASS constraints confirmed: no lock() blocks, ASCII-only literals, no scope creep (V12.23),
caller signature unchanged, illegal states unrepresentable via enum, zero-allocation hot path preserved.
Ticket breakdown approved: T1 (enum + guard), T2 (classifier), T3 (dispatcher + parent refactor), T4 (xUnit tests).

---

## Ticket Definitions

### T1 — Add OcoFleetOrderType Enum and Extract IsOcoOrderActionable Guard

**ID:** T1
**Title:** Extract actionability guard into `IsOcoOrderActionable` + add `OcoFleetOrderType` enum
**File:** `src/V12_002.UI.Compliance.cs`
**Phase 5 Mode:** v12-engineer (Bob CLI)

**Description:**
Add the private enum `OcoFleetOrderType { Stop, Target, Unknown }` to the partial class in
`src/V12_002.UI.Compliance.cs`. Then extract the 4-&&+1-|| compound guard currently inlined
in `ProcessQueuedExecution_HandleFleetOCO` (lines 698–727) into a new private helper
`IsOcoOrderActionable(QueuedAccountExecution item)`.

The extracted guard must validate:
1. `item.EventArgs.Execution?.Order != null`
2. `item.Account != null`
3. `IsFleetAccount(item.Account)` returns true
4. Order state is `Filled` or `PartFilled`

The parent method retains a simple `if (IsOcoOrderActionable(item)) { ... }` call.

**Extraction Target:**
```
Inline guard in ProcessQueuedExecution_HandleFleetOCO (lines ~700-706)
→ private bool IsOcoOrderActionable(QueuedAccountExecution item)
```

**Acceptance Criteria:**
- [ ] `private enum OcoFleetOrderType { Stop, Target, Unknown }` added to partial class
- [ ] `private bool IsOcoOrderActionable(QueuedAccountExecution item)` exists in `src/V12_002.UI.Compliance.cs`
- [ ] All 4+1 predicate conditions from the original compound guard are present in the helper
- [ ] Parent method calls `if (IsOcoOrderActionable(item))` — no inline guard remains
- [ ] No `lock()` blocks introduced
- [ ] All string literals are ASCII-only
- [ ] Build passes (`dotnet build` — zero errors)
- [ ] CSharpier check passes (`dotnet csharpier check src/`)

**CYC Target:**
- `IsOcoOrderActionable`: projected CYC = 5 ≤ 8 ✓
- Parent (partial, pre-T3): CYC reduced from 15 toward ≤ 8

---

### T2 — Extract GetOcoOrderFleetType Classifier

**ID:** T2
**Title:** Extract OCO name classifier into `GetOcoOrderFleetType` (zero-allocation value-type return)
**File:** `src/V12_002.UI.Compliance.cs`
**Phase 5 Mode:** v12-engineer (Bob CLI)
**Depends on:** T1 (OcoFleetOrderType enum must exist)

**Description:**
Extract the string-based branching logic (`StartsWith("Stop_")` / `StartsWith("T") && Length>2 && [2]=='_'`)
from `ProcessQueuedExecution_HandleFleetOCO` into a new private method
`GetOcoOrderFleetType(string ocoName)` returning `OcoFleetOrderType`.

The classifier must implement:
- If `ocoName.StartsWith("Stop_")` → return `OcoFleetOrderType.Stop`
- Else if `ocoName.StartsWith("T") && ocoName.Length > 2 && ocoName[2] == '_'` → return `OcoFleetOrderType.Target`
- Else → return `OcoFleetOrderType.Unknown`

This is a pure classification function with no side effects. The return value is a value-type enum
(zero heap allocation on the hot path).

**Extraction Target:**
```
if/else-if name classification block in ProcessQueuedExecution_HandleFleetOCO (lines ~710-720)
→ private OcoFleetOrderType GetOcoOrderFleetType(string ocoName)
```

**Acceptance Criteria:**
- [ ] `private OcoFleetOrderType GetOcoOrderFleetType(string ocoName)` exists
- [ ] Returns `OcoFleetOrderType.Stop` for names starting with `"Stop_"`
- [ ] Returns `OcoFleetOrderType.Target` for names matching `T{n}_` pattern (StartsWith("T") && Length>2 && [2]=='_')
- [ ] Returns `OcoFleetOrderType.Unknown` for all other names
- [ ] No side effects — pure classifier (no calls to HandleFleetStopFill or HandleFleetTargetFill)
- [ ] Returns value-type enum (no boxing, no allocation)
- [ ] No `lock()` blocks introduced
- [ ] Build passes (`dotnet build` — zero errors)
- [ ] CSharpier check passes

**CYC Target:**
- `GetOcoOrderFleetType`: projected CYC = 5 ≤ 8 ✓
  (1 base + 1 if + 1 else-if + 1 Length guard + 1 char-index guard)

---

### T3 — Extract DispatchOcoFleetOrder and Refactor Parent to CYC ≤ 8

**ID:** T3
**Title:** Extract dispatch into `DispatchOcoFleetOrder` + complete parent method refactor (CYC 15 → 3)
**File:** `src/V12_002.UI.Compliance.cs`
**Phase 5 Mode:** v12-engineer (Bob CLI)
**Depends on:** T1, T2 (enum, guard helper, and classifier must exist)

**Description:**
Extract the routing dispatch (calls to `HandleFleetStopFill` / `HandleFleetTargetFill`) into a new private
method `DispatchOcoFleetOrder(OcoFleetOrderType orderType, QueuedAccountExecution item, Order ocoOrder,
Account ocoAcct, string ocoName)`. Then update the parent method body to use all three helpers.

`DispatchOcoFleetOrder` logic:
- `if (orderType == OcoFleetOrderType.Stop)` → call `HandleFleetStopFill(item, ocoOrder, ocoAcct, ocoName)`
- `else if (orderType == OcoFleetOrderType.Target)` → call `HandleFleetTargetFill(item, ocoOrder, ocoAcct, ocoName)`
- `else` → log unknown type (ASCII-only log string)

Final parent body after extraction:
```csharp
private void ProcessQueuedExecution_HandleFleetOCO(QueuedAccountExecution item)
{
    try
    {
        if (IsOcoOrderActionable(item))
        {
            Order ocoOrder = item.EventArgs.Execution?.Order;
            Account ocoAcct = item.Account;
            string ocoName = ocoOrder.Name ?? "";
            OcoFleetOrderType orderType = GetOcoOrderFleetType(ocoName);
            DispatchOcoFleetOrder(orderType, item, ocoOrder, ocoAcct, ocoName);
        }
    }
    catch (Exception ex)
    {
        Print(string.Format("[1104.1 OCO] Fleet OCO error: {0}", ex.Message));
    }
}
```

**Extraction Target:**
```
Dispatch branching in ProcessQueuedExecution_HandleFleetOCO (lines ~710-724)
→ private void DispatchOcoFleetOrder(OcoFleetOrderType orderType, QueuedAccountExecution item,
     Order ocoOrder, Account ocoAcct, string ocoName)
```

**Acceptance Criteria:**
- [ ] `private void DispatchOcoFleetOrder(OcoFleetOrderType orderType, QueuedAccountExecution item, Order ocoOrder, Account ocoAcct, string ocoName)` exists
- [ ] Dispatcher routes Stop → `HandleFleetStopFill`, Target → `HandleFleetTargetFill`, Unknown → log-only
- [ ] Parent method body matches the refactored skeleton above (CYC = 3)
- [ ] No classification logic remains in parent (no StartsWith calls in parent)
- [ ] No inline guard logic remains in parent (no && compound predicates in parent)
- [ ] Caller `ProcessQueuedExecution` (line 787) continues to compile — signature `ProcessQueuedExecution_HandleFleetOCO(QueuedAccountExecution item)` unchanged
- [ ] No `lock()` blocks introduced
- [ ] All string literals ASCII-only (e.g. `"[1104.1 OCO] Fleet OCO error: {0}"`)
- [ ] Build passes (`dotnet build` — zero errors)
- [ ] CSharpier check passes
- [ ] `deploy-sync.ps1` executed to re-synchronize NinjaTrader hard links

**CYC Target:**
- `DispatchOcoFleetOrder`: projected CYC = 4 ≤ 8 ✓ (1 base + 1 Stop case + 1 Target case + 1 default)
- `ProcessQueuedExecution_HandleFleetOCO` (parent): projected CYC = 3 ≤ 8 ✓ (1 base + 1 if + 1 catch)

---

### T4 — xUnit Tests for Extracted Helpers

**ID:** T4
**Title:** Add xUnit [Fact] tests for `IsOcoOrderActionable`, `GetOcoOrderFleetType`, `DispatchOcoFleetOrder`
**File:** `tests/V12_Performance.Tests/Core/` (new test file)
**Phase 5 Mode:** v12-engineer (Bob CLI)
**Depends on:** T1, T2, T3 (all helpers must exist)

**Description:**
Write xUnit tests covering the three extracted helper methods. Tests must use `[Fact]` and `Assert.Equal` /
`Assert.True` / `Assert.False`. No NUnit or MSTest attributes permitted (V12 Test Framework Mandate V12.32).

Test scenarios:

**IsOcoOrderActionable:**
- Null order → returns false
- Null account → returns false
- Non-fleet account → returns false
- Wrong order state (e.g. Working) → returns false
- Valid fleet account + Filled state → returns true
- Valid fleet account + PartFilled state → returns true

**GetOcoOrderFleetType:**
- `"Stop_BES"` → `OcoFleetOrderType.Stop`
- `"Stop_"` (exact prefix) → `OcoFleetOrderType.Stop`
- `"T2_BES"` → `OcoFleetOrderType.Target`
- `"T9_X"` → `OcoFleetOrderType.Target`
- `"LIMIT_BES"` → `OcoFleetOrderType.Unknown`
- `""` (empty string) → `OcoFleetOrderType.Unknown`
- `"TX"` (Length = 2, not > 2) → `OcoFleetOrderType.Unknown`

**DispatchOcoFleetOrder:**
- `OcoFleetOrderType.Stop` → verifies `HandleFleetStopFill` invocation path
- `OcoFleetOrderType.Target` → verifies `HandleFleetTargetFill` invocation path
- `OcoFleetOrderType.Unknown` → no handler call, no exception thrown

**Acceptance Criteria:**
- [ ] Test file created in `tests/V12_Performance.Tests/Core/`
- [ ] All test methods use `[Fact]` attribute (xUnit)
- [ ] All assertions use `Assert.Equal`, `Assert.True`, or `Assert.False`
- [ ] No NUnit attributes (`[Test]`, `[TestCase]`) or MSTest attributes (`[TestMethod]`)
- [ ] Minimum 10 test cases covering all three helpers
- [ ] `dotnet test` passes — all new tests green
- [ ] Build passes (`dotnet build` — zero errors)

**CYC Target:** N/A (test helpers; not subject to CYC threshold)

---

## Extraction Summary

| Ticket | Helper | Projected CYC | Concern | Depends On |
|--------|--------|--------------|---------|------------|
| T1 | `OcoFleetOrderType` enum + `IsOcoOrderActionable` | 5 | Guard / Enum | — |
| T2 | `GetOcoOrderFleetType` | 5 | Classifier | T1 |
| T3 | `DispatchOcoFleetOrder` + parent refactor | 4 (dispatcher) / 3 (parent) | Dispatch | T1, T2 |
| T4 | xUnit tests | N/A | Test coverage | T1, T2, T3 |

**CYC Reduction:** 15 → max 5 (67% reduction)
**Max Projected CYC:** 5 ≤ 8 ✓
**Jane Street Alignment:** CYC ≤ 8, single-responsibility, lock-free, illegal-states unrepresentable, zero-allocation

---

## Agent Tracking

| Field | Value |
|---|---|
| **agent_name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-147 |
| **Method** | `ProcessQueuedExecution_HandleFleetOCO` |
| **Original CYC** | 15 |
| **Max CYC Projected** | 5 |
| **Ticket Count** | 4 |
| **Extraction Count** | 3 |
| **dna_verdict** | PASS |
| **sequential-thinking calls** | 4 (1 probe + 3 analysis thoughts) |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **Generated** | 2026-06-29T01:20:00Z |
| **Lane** | P4-L9 |
