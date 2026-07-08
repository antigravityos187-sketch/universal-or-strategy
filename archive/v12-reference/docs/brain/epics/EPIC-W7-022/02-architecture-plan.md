# EPIC-W7-022 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29
**Input:** docs/brain/EPIC-W7-022/01-scope-boundary.md

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-022 |
| **Method** | `PropagateMaster_IdentifyMove` |
| **File** | `src/V12_002.Orders.Callbacks.Propagation.cs` |
| **CYC (precomputed)** | 0 (stub artifact — unreliable) |
| **CYC (MCP verified)** | **5** |
| **Assessment** | medium (jCodemunch) |
| **Lines** | 39 (lines 82–120) |
| **Max Nesting** | 2 |
| **Param Count** | 6 |
| **Decision** | **NO EXTRACTION NEEDED** |
| **max_cyc_projected** | **5** |
| **Tickets Required** | 0 |

---

## Phase 1.5 Boundary Verdict

Phase 1.5 boundary verdict: **PASS**. Scope is limited to `PropagateMaster_IdentifyMove` in
`src/V12_002.Orders.Callbacks.Propagation.cs`. One caller (`PropagateMasterPriceMove`) confirmed
unchanged. Phase 1.5 assumed CYC=9 based on precomputed data; actual MCP-verified CYC=5.

---

## MCP Code Analysis

### Symbol Source

```csharp
private bool PropagateMaster_IdentifyMove(
    Order masterOrder,
    out string masterEntryName,
    out bool isEntryMove,
    out bool isStopMove,
    out bool isTargetMove,
    out int masterTargetNum
)
{
    // --- Step 1: Identify master position and move type via object identity ---
    masterEntryName = null;
    isEntryMove = false;
    isStopMove = false;
    isTargetMove = false;
    masterTargetNum = 0;

    // Scan entry orders
    if (ScanOrderDictionaryForMaster(entryOrders, masterOrder, out masterEntryName))
    {
        isEntryMove = true;
        return true;
    }

    // Scan stop orders
    if (ScanOrderDictionaryForMaster(stopOrders, masterOrder, out masterEntryName))
    {
        isStopMove = true;
        return true;
    }

    // Scan target orders (1-5)
    if (ScanTargetDictionariesForMaster(masterOrder, out masterEntryName, out masterTargetNum))
    {
        isTargetMove = true;
        return true;
    }

    return false; // Not a tracked master order
}
```

### Branch Enumeration

| # | Branch | CYC Contribution |
|---|---|---|
| 1 | Method entry / default path | 1 |
| 2 | `if (ScanOrderDictionaryForMaster(entryOrders, ...))` | +1 |
| 3 | `if (ScanOrderDictionaryForMaster(stopOrders, ...))` | +1 |
| 4 | `if (ScanTargetDictionariesForMaster(...))` | +1 |
| 5 | Tool-reported extra path (early returns) | +1 |
| **Total** | | **5** |

### Call Hierarchy

**Callers (depth=1):**
- `PropagateMasterPriceMove` (line 37, same file) — 1 caller total

**Callees (depth=1):**
- `ScanOrderDictionaryForMaster` — called twice (entry + stop scanning)
- `ScanTargetDictionariesForMaster` — called once (target 1-5 scanning)

**Callees (depth=2, inferred):**
- `activePositions` (src/V12_002.cs:199)
- `GetTargetOrdersDictionary` (src/V12_002.UI.Callbacks.cs:1039)

---

## Sequential Thinking Architecture Decision

### Thought 1: CYC Analysis
Actual CYC=5. The method has 3 sequential `if` guards with early-return pattern. Max nesting=2.
Each branch delegates to an existing scanning helper — no inline logic embedded.
The precomputed CYC=0 was a stub indexing artifact; real complexity is confirmed low.

### Thought 2: Extraction Decision
CYC=5 is **<= 8** (Jane Street strict standard). **No extraction needed.**

The method already satisfies:
- **carl_cook**: Uses `out` parameters (zero-alloc). No LINQ. No heap allocation.
- **gjengset**: No `lock()` blocks. Clean linear control flow.
- **trading_billions**: Single responsibility (order-move type identification only). Each branch CYC <= 8.

The method IS the single-responsibility helper. Its design is already optimal.

### Thought 3: Projection Validation
`max_cyc_projected = 5` (method unchanged). No new helpers introduced. No new CYC added.
Validation: 5 <= 8 — **PASS**.

---

## Extraction Plan

**Plan Type:** NO_EXTRACTION

The method satisfies V12 CYC <= 8 standard as-is. No helper extraction is required.
No tickets will be generated for Phase 5. Phase 3 (audit) and Phase 4 (tickets) may be
marked as compliant-skip.

### Rationale

| Criterion | Status |
|---|---|
| CYC <= 8 (Jane Street) | PASS — CYC=5 |
| Single responsibility | PASS — identifies move type only |
| Zero-alloc (carl_cook) | PASS — out params, no LINQ |
| No lock() (gjengset) | PASS — no synchronization primitives |
| Delegation pattern (trading_billions) | PASS — delegates scanning to helpers |
| Early-return pattern | PASS — avoids deep nesting |

---

## Method Signatures (No New Helpers)

No new helper methods are required. The existing delegation chain is:

```
PropagateMaster_IdentifyMove
    -> ScanOrderDictionaryForMaster(entryOrders, ...)   [existing]
    -> ScanOrderDictionaryForMaster(stopOrders, ...)    [existing]
    -> ScanTargetDictionariesForMaster(...)             [existing]
```

---

## Risk Assessment

| Risk | Level | Mitigation |
|---|---|---|
| CYC regression on future edits | Low | Existing structure already minimal |
| 6 out-params cognitive overhead | Low | Acceptable; single-call site caller |
| ScanOrderDictionaryForMaster called twice | Low | Intentional; different dictionary args |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Phase** | 2 |
| **MCP Tools Used** | resolve_repo, search_symbols, get_symbol_complexity, get_symbol_source, get_call_hierarchy, get_dependency_graph |
| **Sequential Thinking Thoughts** | 3 |
| **CYC Source** | MCP verified (jCodemunch get_symbol_complexity) |
| **Bobcoins Used** | 1.0 |
| **Decision** | NO_EXTRACTION — CYC=5 <= 8, compliant as-is |
| **max_cyc_projected** | 5 |
| **Tickets Required** | 0 |
