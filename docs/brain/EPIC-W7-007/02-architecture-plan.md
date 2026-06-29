# Phase 2: Architecture Plan — EPIC-W7-007

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-007/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `GetTargetDistribution`
- **Source File:** `src/V12_002.PureLogic.cs`
- **Original CYC:** 4
- **Class:** `V12_PureLogic` (static)
- **Signature:** `public static int[] GetTargetDistribution(int contracts, int targetCount)`

### jcodemunch get_context_bundle result

Symbol resolved via ID `src/V12_002.PureLogic.cs::V12_PureLogic.GetTargetDistribution#method`. Full source confirmed:

```csharp
public static int[] GetTargetDistribution(int contracts, int targetCount)
{
    if (contracts <= 0)
    {
        return new int[5];
    }

    // Clamp count to [1, 5]
    int count = Math.Max(1, Math.Min(5, targetCount));

    int[] buckets = new int[5];
    int baseQty = contracts / count;
    int remainder = contracts % count;

    // Distribute base and remainder (scalp preference: extras go to T1 first)
    for (int i = 0; i < count; i++)
    {
        buckets[i] = baseQty + (i < remainder ? 1 : 0);
    }

    // Audit: Ensure sum matches input
    int sum = buckets.Sum();
    if (sum != contracts)
    {
        // Panic adjustment (should not happen with integer division logic above)
        buckets[count - 1] += (contracts - sum);
    }

    return buckets;
}
```

Docstring: `IS-01: Iron Shield Target Distribution [V12.BEYOND-BUG] — Deterministically divides contracts into a bucketed distribution.`

### jcodemunch get_call_hierarchy result

- **Callers (depth=2):** 0 returned by import graph (pure static class; C# call references not captured in import-graph index). Prior phase analysis (Phase 0/1) confirmed **17 call sites across 11 files** via grep — all read-only consumers of the returned `int[]`.
- **Callees (depth=2):** 0 — method uses only BCL primitives (`Math.Max`, `Math.Min`, `buckets.Sum()`).
- **Direction:** Both. No dynamic dispatch. No virtual calls.

### jcodemunch get_dependency_graph result

- **File:** `src/V12_002.PureLogic.cs`
- **Direction:** Both (imports + importers)
- **Depth:** 1
- **Imports:** None (no tracked file-level imports — pure static utility, BCL only)
- **Importers:** None tracked at file graph level (consumers import via namespace, not tracked as edges)
- **Summary:** Fully self-contained pure-logic file with no external file dependencies.

### jcodemunch get_extraction_candidates result

- **Candidates returned:** 0
- **Reason:** `get_extraction_candidates` requires `min_complexity=3` AND `min_callers=1` simultaneously resolved via import graph. Since the import graph shows no edges for this pure static file, no candidates are surfaced. CYC=4 is also below the tool's effective extraction threshold.
- **Resolution:** Manual analysis (context bundle + sequential thinking) used to design extraction plan per Jane Street rules.

---

## Sequential Thinking Summary

**5-step chain completed. Final verdict (Thought 5):**

EXTRACTION PLAN FINALIZED:

**Helper 1: `ComputeSlotQuantity`**
Signature: `private static int ComputeSlotQuantity(int baseQty, int slot, int remainder)`
Responsibility: Computes the contract quantity for a single distribution bucket slot by applying the scalp-preference ternary assignment. Encapsulates the loop body per Jane Street "Extract Loop Body" rule.
Body: `return baseQty + (slot < remainder ? 1 : 0);`
Projected CYC: 2 (base=1 + ternary=1)

**Helper 2: `ValidateAndAdjustBucketSum`**
Signature: `private static void ValidateAndAdjustBucketSum(int[] buckets, int contracts, int count)`
Responsibility: Audits bucket sum integrity post-distribution and applies in-place panic-adjustment correction if integer-division rounding drifts from input contracts. Isolates the audit concern from the distribution loop.
Projected CYC: 2 (base=1 + if-guard=1)

**Parent after extraction:**
Remaining logic: guard-if (contracts <= 0), count clamping, bucket allocation, for-loop delegating to `ComputeSlotQuantity`, call to `ValidateAndAdjustBucketSum`, return.
Projected CYC: 3 (base=1 + guard-if=1 + for-loop-head=1)

All helpers + parent CYC <= 8. Jane Street alignment: FULL PASS.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `ComputeSlotQuantity(int baseQty, int slot, int remainder)` | Returns the integer quantity for one distribution bucket slot (loop body extraction — applies scalp-preference ternary: `baseQty + (slot < remainder ? 1 : 0)`) | 2 |
| `ValidateAndAdjustBucketSum(int[] buckets, int contracts, int count)` | Audits bucket sum vs. input contracts post-distribution; applies in-place correction for integer-division edge cases (panic-adjustment isolation) | 2 |

---

## Parent Method After Extraction

**Remaining logic:**
1. Guard clause: `if (contracts <= 0) return new int[5];`
2. Count clamping: `int count = Math.Max(1, Math.Min(5, targetCount));`
3. Buffer allocation: `int[] buckets = new int[5];`
4. Division: `int baseQty = contracts / count; int remainder = contracts % count;`
5. Distribution loop: `for (int i = 0; i < count; i++) { buckets[i] = ComputeSlotQuantity(baseQty, i, remainder); }`
6. Audit call: `ValidateAndAdjustBucketSum(buckets, contracts, count);`
7. Return: `return buckets;`

**Projected CYC:** 3 (base + guard-if + for-loop head — ternary and audit-if moved to helpers)

---

## max_cyc_projected: 3
## extraction_count: 2

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved | YES — max projected CYC = 3 across all methods |
| Single-responsibility per helper | YES — `ComputeSlotQuantity` owns slot arithmetic only; `ValidateAndAdjustBucketSum` owns audit+correction only |
| Lock-free/Actor pattern preserved | YES — method is pure static with no state mutations and no lock() blocks |
| Illegal states unrepresentable | YES — `ValidateAndAdjustBucketSum` explicitly encapsulates the invariant check; the panic-adjustment path is isolated and named, making the invariant contract visible |
| Zero-allocation hot paths | YES — both helpers are static, accept primitives/in-place arrays, return primitives; no heap allocations added |
| Extract Loop Body applied | YES — `ComputeSlotQuantity` is the extracted loop body per Jane Street pattern |
| Extract to Named Helpers | YES — both helpers have single-purpose names reflecting exact concern |
| Caller signature unchanged | YES — `GetTargetDistribution(int contracts, int targetCount)` signature identical; all 17 call sites unaffected |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **Phase** | 2 |
| **Wave** | 7 |
| **jcodemunch tools called** | resolve_repo, get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates, search_symbols (fallback) |
| **sequential-thinking calls** | 5 |
| **extraction_count** | 2 |
| **max_cyc_projected** | 3 |
| **Input** | docs/brain/EPIC-W7-007/01-scope-boundary.md |
| **Output** | docs/brain/EPIC-W7-007/02-architecture-plan.md |
