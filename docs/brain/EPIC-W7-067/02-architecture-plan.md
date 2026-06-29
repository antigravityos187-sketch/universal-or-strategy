# Phase 2: Architecture Plan — EPIC-W7-067

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-067/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `SymmetryFindDispatchForMasterFill`
- **Source File:** `src/V12_002.Symmetry.cs` (lines 326–352)
- **Original CYC:** 8
- **Strategy:** HOLD-THE-LINE (CYC=8 is at ceiling; 0 extractions required)

### Full Source (from jcodemunch get_context_bundle)

```csharp
private SymmetryDispatchContext SymmetryFindDispatchForMasterFill(
    string tradeType,
    MarketPosition direction,
    DateTime fillTimeUtc
)
{
    string norm = SymmetryNormalizeTradeType(tradeType);
    SymmetryDispatchContext best = null;

    foreach (var kvp in symmetryDispatchById.ToArray())
    {
        SymmetryDispatchContext ctx = kvp.Value;
        if (ctx == null || ctx.Anchor.IsResolved)
            continue;
        if (ctx.Direction != direction)
            continue;
        if (!string.Equals(ctx.TradeType, norm, StringComparison.Ordinal))
            continue;
        if (fillTimeUtc - ctx.CreatedUtc > SymmetryDispatchTtl)
            continue;

        if (best == null || ctx.CreatedUtc < best.CreatedUtc)
            best = ctx;
    }

    return best;
}
```

### jcodemunch get_context_bundle result

- Symbol resolved: `src/V12_002.Symmetry.cs::V12_002.SymmetryFindDispatchForMasterFill#method`
- Kind: `method`, Line 326–352
- Signature: `private SymmetryDispatchContext SymmetryFindDispatchForMasterFill(string tradeType, MarketPosition direction, DateTime fillTimeUtc)`
- Docstring: (none)
- Full source retrieved: 27 lines, all branches confirmed inline (no nested lambdas or closures)
- Imports: System, System.Collections.Concurrent, System.Collections.Generic, System.Linq, System.Threading, NinjaTrader.Cbi, NinjaTrader.NinjaScript

### jcodemunch get_call_hierarchy result

- **Direction:** both, depth=2
- **Callers (depth 1):** `SymmetryGuardOnMasterFill` (src/V12_002.Symmetry.cs:258, AST-resolved)
- **Caller count:** 1
- **Callees (depth 1):**
  - `SymmetryNormalizeTradeType` (src/V12_002.Symmetry.Replace.cs:322, AST-inferred) — trade-type normalizer
  - `symmetryDispatchById` (src/V12_002.Symmetry.cs:118, AST-resolved) — ConcurrentDictionary field read
- **Depth reached:** 1 (no further callers/callees at depth 2)
- Full call chain: `ValidateAndPrepareEntryFill (Orders.Callbacks.cs:368) → SymmetryGuardOnMasterFill (Symmetry.cs:258) → SymmetryFindDispatchForMasterFill (Symmetry.cs:326)`

### jcodemunch get_dependency_graph result

- **File:** `src/V12_002.Symmetry.cs`
- **Direction:** both, depth=1
- **Import edges:** 0 (no explicit module-level imports resolved in graph)
- **Importer edges:** 0 (partial class / same-file pattern — no cross-file import edges detected)
- **Interpretation:** `V12_002.Symmetry.cs` is a partial class file. Its symbols reference `V12_002.Symmetry.Replace.cs` via method calls (SymmetryNormalizeTradeType) rather than explicit `using` imports, which are shared at the project level. No external module dependency risk for this extraction.

### jcodemunch get_extraction_candidates result

- **Candidates found:** 0
- **min_complexity:** 3, **min_callers:** 1
- **Interpretation:** No symbol in `src/V12_002.Symmetry.cs` met the criteria (complexity >= 3 AND callers >= 1 from multiple files). This confirms jcodemunch independently agrees no extraction is warranted for this method at current complexity levels.

---

## Sequential Thinking Summary

**Final thought (Thought 5):**

EPIC-W7-067 Phase 2 Architecture Plan — FINAL VERDICT: **HOLD-THE-LINE (zero extractions, zero CYC changes).**

Rationale:
1. `SymmetryFindDispatchForMasterFill` has CYC=8. The project ceiling is CYC<=8. It passes.
2. Phase 1 explicitly determined 0 extractions. The four skip-predicates must stay inline due to ordering constraints: the null/resolved guard must precede direction and type checks (which dereference `ctx`), and the TTL check should follow type filtering to avoid timestamp arithmetic on discarded contexts. Any reordering risks NPE or stale-dispatch selection.
3. The oldest-wins fold (`best == null || ctx.CreatedUtc < best.CreatedUtc`) contributes CYC=2 and must stay inline to preserve the null-start semantics — LINQ `MinBy` on an empty sequence throws, making the current explicit null-start pattern the only safe approach.
4. The only actionable performance improvement (eliminate `ToArray()` allocation via mandatory pre-mapping in the caller) is a caller-side change in `SymmetryGuardOnMasterFill`, which is **out of scope** per V12.23.
5. Phase 2 confirms: method is within ceiling, 0 extractions, all Jane Street rules satisfied.

**Metrics:** extraction_count=0, max_cyc_projected=8, parent_cyc_after=8, new_helpers=none

---

## Extraction Plan

**No helper extractions are planned.** The method already satisfies CYC<=8 with no violations.

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| *(none)* | *(no extraction required — CYC=8 is at ceiling, not over ceiling)* | N/A |

---

## Parent Method After Extraction

- **Remaining logic:** Unchanged. Full method body retained as-is. No lines removed or moved.
- **Projected CYC:** 8
- **CYC delta:** 0 (hold-the-line)

The eight independent paths remain:
1. Method entry (base path) — +1
2. `foreach` loop body entered — +1
3. `ctx == null || ctx.Anchor.IsResolved` null/resolved guard — +1
4. `ctx.Direction != direction` direction mismatch — +1
5. `!string.Equals(ctx.TradeType, norm, ...)` trade-type mismatch — +1
6. `fillTimeUtc - ctx.CreatedUtc > SymmetryDispatchTtl` TTL expired — +1
7. `best == null` first qualifying candidate — +1
8. `ctx.CreatedUtc < best.CreatedUtc` subsequent older candidate — +1

---

## max_cyc_projected: 8
## extraction_count: 0

---

## Deferred Work (Out of Scope — V12.23)

The following structural improvement was identified in Phase 1 but is out of scope for this epic:

> **Make the `symmetryMasterEntryToDispatch` pre-mapping mandatory at dispatch-time** in
> `SymmetryGuardOnMasterFill`, so that `SymmetryFindDispatchForMasterFill` is only invoked
> as a defensive fallback and never on the latency-sensitive hot fill-callback path.
>
> This eliminates the `ConcurrentDictionary.ToArray()` heap allocation for all normal fills.
> Scope boundary: caller-side change, requires a separate epic targeting `SymmetryGuardOnMasterFill`.

---

## Jane Street Alignment

| Rule | Status | Evidence |
|---|---|---|
| **CYC<=8 achieved** | YES | Method CYC=8, at ceiling, no violation |
| **Single-responsibility per helper** | N/A | No helpers extracted; parent method has single responsibility: linear scan for oldest unresolved matching context |
| **Lock-free/Actor pattern preserved** | YES | Method is read-only. `symmetryDispatchById.ToArray()` provides lock-free snapshot. No `lock()` blocks introduced or present. CAS-loop mutation lives in caller (out of scope). |
| **Illegal states unrepresentable** | YES | Return type `SymmetryDispatchContext` (nullable reference): null signals no-match, non-null signals unique unresolved context. No invalid intermediate states possible in this read-only scan. |
| **Zero-allocation hot paths** | ACCEPTED DEFERRAL | `ToArray()` allocation is by-design for fallback path. Elimination deferred to caller-side epic. No new allocations introduced by this plan. |
| **Guard clause ordering preserved** | YES | Four-predicate cascade order (null/resolved → direction → trade-type → TTL) maintained, protecting against NPE on field dereferences |
| **Oldest-wins selection preserved** | YES | `best == null || ctx.CreatedUtc < best.CreatedUtc` min-by fold retained — H-11 duplicate-dispatch guard intact |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Epic** | EPIC-W7-067 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **jcodemunch tools called** | resolve_repo, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates, search_symbols |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 0 |
| **max_cyc_projected** | 8 |
