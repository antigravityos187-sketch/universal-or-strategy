# EPIC-W7-016 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-016/01-scope-boundary.md

---

## 1. Original Method Profile (MCP-Confirmed)

| Field | Value |
|---|---|
| **Method** | `TryHandleFleet_CancelAll` |
| **File** | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| **Line** | 177 |
| **End Line** | 232 |
| **CYC (task list)** | 21 |
| **CYC (MCP-confirmed)** | 19 |
| **Max Nesting** | 5 |
| **Lines** | 56 |
| **Params** | 2 (`string action`, `string cmdId`) |
| **Assessment** | HIGH complexity |
| **Signature** | `private bool TryHandleFleet_CancelAll(string action, string cmdId)` |
| **Caller Count** | 1 (`TryHandleFleetCommand`) |

---

## 2. Source Analysis

```csharp
// Lines 177-232 — full method body as retrieved by MCP get_symbol_source
private bool TryHandleFleet_CancelAll(string action, string cmdId)
{
    if (action != "CANCEL_ALL")          // guard branch +1
        return false;

    if (!MetadataGuardDuplicate(cmdId, action))  // guard branch +1
        return true;

    if (EnableSIMA)                      // SIMA split +1
    {
        int masterCancelled = CancelAll_ProcessMasterAccount();
        int fleetCancelled  = CancelAll_ProcessFleetAccounts();
        int totalCancelled  = masterCancelled + fleetCancelled;
        Print($"[SIMA] CANCEL_ALL -> Cancelled {totalCancelled} orders ...");
    }
    else
    {
        int cancelled = 0;
        foreach (Order order in Account.Orders)              // +1
        {
            if (order != null
                && order.Instrument.FullName == Instrument.FullName
                && (
                    order.OrderState == OrderState.Working          // +1
                    || order.OrderState == OrderState.Accepted      // +1
                    || order.OrderState == OrderState.Submitted     // +1
                    || order.OrderState == OrderState.ChangePending // +1
                    || order.OrderState == OrderState.ChangeSubmitted // +1
                ))
            {
                string oName = order.Name;
                if (
                    oName.StartsWith("Stop_")  // +1
                    || oName.StartsWith("S_")  // +1
                    || oName.StartsWith("T1_") // +1
                    || oName.StartsWith("T2_") // +1
                    || oName.StartsWith("T3_") // +1
                    || oName.StartsWith("T4_") // +1
                    || oName.StartsWith("T5_") // +1
                )
                    continue;

                CancelOrderOnAccount(order, order.Account);
                cancelled++;
            }
        }
        Print($"[V12] CANCEL_ALL -> Cancelled {cancelled} pending entry orders");
    }

    return true;
}
```

**Complexity Drivers:**
- 2 early-exit guard branches
- 1 SIMA/non-SIMA routing branch
- 5-way `OrderState` OR predicate (inside non-SIMA foreach)
- 7-way `StartsWith` bracket-order-name predicate (inside non-SIMA foreach)
- Max nesting depth = 5 (method → foreach → if compound → if startsWith → continue)

---

## 3. Extraction Plan

| # | Helper Name | Responsibility | Lines Moved | Projected CYC | Jane Street Annotation |
|---|---|---|---|---|---|
| 1 | `CancelAll_IsActiveOrderState` | Pure predicate: returns `true` if `order.OrderState` is in `{Working, Accepted, Submitted, ChangePending, ChangeSubmitted}` | ~6 lines | **6** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — hot path, zero-alloc |
| 2 | `CancelAll_IsBracketOrderName` | Pure predicate: returns `true` if order name starts with any bracket/stop/target prefix (`Stop_`, `S_`, `T1_`–`T5_`) | ~8 lines | **8** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — hot path, zero-alloc, called inside foreach |
| 3 | `CancelAll_NonSimaPath` | Executes the non-SIMA cancel loop: iterates `Account.Orders`, applies both predicates, calls `CancelOrderOnAccount`, logs result | ~18 lines | **4** | `[MethodImpl(MethodImplOptions.NoInlining)]` — cold path, contains logging |

### Helper Signatures

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool CancelAll_IsActiveOrderState(Order order)
// Returns: true if order.OrderState is Working|Accepted|Submitted|ChangePending|ChangeSubmitted
// CYC: 6 (5 OR conditions + base)

[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool CancelAll_IsBracketOrderName(string orderName)
// Returns: true if orderName starts with Stop_|S_|T1_|T2_|T3_|T4_|T5_
// CYC: 8 (7 OR conditions + base)

[MethodImpl(MethodImplOptions.NoInlining)]
private void CancelAll_NonSimaPath()
// Iterates Account.Orders, cancels non-bracket active entry orders, prints result
// CYC: 4 (foreach + null/instrument check + IsActiveOrderState + IsBracketOrderName continue)
```

---

## 4. Parent After Extraction

```csharp
private bool TryHandleFleet_CancelAll(string action, string cmdId)
{
    if (action != "CANCEL_ALL")                  // +1
        return false;

    if (!MetadataGuardDuplicate(cmdId, action))  // +1
        return true;

    if (EnableSIMA)                              // +1
    {
        int masterCancelled = CancelAll_ProcessMasterAccount();
        int fleetCancelled  = CancelAll_ProcessFleetAccounts();
        int totalCancelled  = masterCancelled + fleetCancelled;
        Print($"[SIMA] CANCEL_ALL -> Cancelled {totalCancelled} orders ...");
    }
    else
    {
        CancelAll_NonSimaPath();                 // delegated — 0 new branches
    }

    return true;
}
```

**Parent Projected CYC: 4** (base=1 + 3 branches)

---

## 5. Complexity Summary

| Symbol | Before | After | Status |
|---|---|---|---|
| `TryHandleFleet_CancelAll` (parent) | 19 | 4 | ✅ PASS |
| `CancelAll_IsActiveOrderState` (new) | — | 6 | ✅ PASS |
| `CancelAll_IsBracketOrderName` (new) | — | 8 | ✅ PASS |
| `CancelAll_NonSimaPath` (new) | — | 4 | ✅ PASS |

**max_cyc_projected: 8** ✅ (meets Jane Street <= 8 requirement)

---

## 6. Jane Street Alignment Notes

### carl_cook (zero-alloc hot path)
- `CancelAll_IsActiveOrderState` and `CancelAll_IsBracketOrderName` are marked `[AggressiveInlining]` — they are pure predicates called inside the `foreach` loop (hot path), zero-allocation, no LINQ.
- `CancelAll_NonSimaPath` uses `[NoInlining]` — it contains a `Print()` logging call (cold path), so inlining would pollute the JIT hot-path register pressure.
- No LINQ used anywhere. All iteration via `foreach` over `Account.Orders` (direct collection walk).

### gjengset (lock-free)
- No new `lock()` blocks introduced. All state reads (`Account.Orders`, `order.OrderState`) are pre-existing patterns.
- No shared mutable state in the extracted helpers — all helpers are pure read-only predicates or delegate to existing order-cancel infrastructure.
- `CancelAll_NonSimaPath` does not add any synchronization concerns.

### trading_billions (single responsibility + defense in depth)
- Each helper has exactly one responsibility:
  - `CancelAll_IsActiveOrderState` → tells you if an order is cancellable
  - `CancelAll_IsBracketOrderName` → tells you if an order is a bracket/stop/target (must be preserved)
  - `CancelAll_NonSimaPath` → executes the non-SIMA cancel loop
- The SIMA/non-SIMA dual path is made explicit: both paths now symmetrically delegate to named helpers (`CancelAll_ProcessMasterAccount` + `CancelAll_ProcessFleetAccounts` for SIMA; `CancelAll_NonSimaPath` for non-SIMA).
- Defense in depth: the null-check `order != null` is preserved inside `CancelAll_NonSimaPath` (not assumed away by helpers).

---

## 7. Scope Compliance (V12.23 No Scope Creep)

- ✅ Scope boundary verdict from Phase 1.5: **PASS**
- ✅ Only `TryHandleFleet_CancelAll` is modified
- ✅ All new helpers are `private` methods in the same class/partial
- ✅ No caller modifications (`TryHandleFleetCommand` untouched)
- ✅ No cross-file changes
- ✅ Method signature unchanged: `private bool TryHandleFleet_CancelAll(string action, string cmdId)`

---

## 8. MCP Evidence

| Tool | Query | Key Result |
|---|---|---|
| `resolve_repo` | `/home/malhitticrypto/universal-or-strategy` | `indexed=true`, `symbol_count=5147` |
| `search_symbols` | `TryHandleFleet_CancelAll` | Found at `src/V12_002.UI.IPC.Commands.Fleet.cs:177` |
| `get_symbol_complexity` | `TryHandleFleet_CancelAll` | `cyclomatic=19`, `max_nesting=5`, `param_count=2`, `lines=56`, `assessment=high` |
| `get_symbol_source` | `TryHandleFleet_CancelAll` | Full source retrieved, lines 177-232, content_hash `e3d815c3...` |
| `get_call_hierarchy` | `TryHandleFleet_CancelAll` | 1 caller (`TryHandleFleetCommand`), 18 callees including `MetadataGuardDuplicate`, `CancelAll_ProcessMasterAccount`, `CancelAll_ProcessFleetAccounts`, `CancelOrderOnAccount` |
| `get_dependency_graph` | `src/V12_002.UI.IPC.Commands.Fleet.cs` | 1 node, 0 cross-file edges (partial class, no explicit imports) |

---

## 9. Sequential Thinking Evidence

**Thought 1 — Branch Point Enumeration:**
Identified 16 branch points: 2 guard branches, 1 SIMA split, 5-way `OrderState` OR (5 branches), 7-way `StartsWith` OR (7 branches). Total CYC=19 confirmed. Key insight: the 5+7 = 12 OR-branches inside the non-SIMA path are the primary complexity drivers and can be extracted into pure predicates with no behavioral change.

**Thought 2 — Extraction Strategy:**
Designed 3 helpers aligned to single-responsibility principle. `CancelAll_IsActiveOrderState` (CYC=6) extracts the 5-way OrderState predicate. `CancelAll_IsBracketOrderName` (CYC=8) extracts the 7-way StartsWith predicate. `CancelAll_NonSimaPath` (CYC=4) extracts the full non-SIMA foreach loop using both predicates. Parent reduces to CYC=4 (3 branches + base). Jane Street annotations applied: AggressiveInlining for hot-path predicates, NoInlining for cold logging path.

**Thought 3 — Validation:**
All 4 symbols (parent + 3 helpers) verified CYC <= 8. max_cyc_projected = 8 (CancelAll_IsBracketOrderName = exactly 8, boundary-compliant). Architecture symmetry noted: the SIMA path already uses named delegates (`CancelAll_ProcessMasterAccount`, `CancelAll_ProcessFleetAccounts`); extracting `CancelAll_NonSimaPath` mirrors this pattern, making the dual-path design explicit. Hypothesis VERIFIED.

---

## 10. Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.0 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-016 |
| **MCP Tools Used** | resolve_repo, search_symbols, get_symbol_complexity, get_symbol_source, get_call_hierarchy, get_dependency_graph |
| **Sequential Thinking Thoughts** | 3 |
| **max_cyc_projected** | 8 |
| **boundary_verdict** | PASS (from Phase 1.5) |
