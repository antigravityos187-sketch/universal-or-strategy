# Phase 4: Ticket Definitions — EPIC-W7-007

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Inputs:** docs/brain/EPIC-W7-007/02-architecture-plan.md + docs/brain/EPIC-W7-007/03-audit-report.md

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `GetTargetDistribution` |
| **Class** | `V12_PureLogic` (static) |
| **Source File** | `src/V12_002.PureLogic.cs` |
| **CYC (jCodemunch index)** | 6 |
| **CYC (scope analysis)** | 4 |
| **Target CYC** | <= 8 |
| **DNA Verdict** | PASS |

---

## jCodemunch Evidence

### get_symbol_complexity

```json
{
  "symbol_id": "src/V12_002.PureLogic.cs::V12_PureLogic.GetTargetDistribution#method",
  "name": "GetTargetDistribution",
  "kind": "method",
  "file": "src/V12_002.PureLogic.cs",
  "line": 19,
  "cyclomatic": 6,
  "max_nesting": 2,
  "param_count": 2,
  "lines": 30,
  "assessment": "medium"
}
```

### get_extraction_candidates

```json
{
  "file": "src/V12_002.PureLogic.cs",
  "candidates": [],
  "min_complexity": 5,
  "min_callers": 2
}
```

**Note:** No candidates returned — same known limitation as Phase 2 (pure static class with no import-graph edges). Manual extraction plan from Phase 2 architecture applies.

---

## ticket_count: 2

---

## Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | T1 |
| **helper_name** | `ComputeSlotQuantity` |
| **concern** | Computes the integer contract quantity for a single distribution bucket slot; owns the scalp-preference ternary assignment (`baseQty + (slot < remainder ? 1 : 0)`) extracted per Jane Street "Extract Loop Body" rule |
| **signature** | `private static int ComputeSlotQuantity(int baseQty, int slot, int remainder)` |
| **lines_to_move** | Loop body from `for (int i = 0; i < count; i++)` block: `buckets[i] = baseQty + (i < remainder ? 1 : 0)` — replace with `buckets[i] = ComputeSlotQuantity(baseQty, i, remainder)` |
| **cyc_reduction** | 1 (ternary predicate `i < remainder` moves from parent to helper) |
| **projected_helper_cyc** | 2 (base=1 + ternary=1) |

### Ticket 1 — Pre/Post Pseudocode

**Before (parent loop body):**
```csharp
for (int i = 0; i < count; i++)
{
    buckets[i] = baseQty + (i < remainder ? 1 : 0);
}
```

**After (parent loop body):**
```csharp
for (int i = 0; i < count; i++)
{
    buckets[i] = ComputeSlotQuantity(baseQty, i, remainder);
}
```

**New helper:**
```csharp
private static int ComputeSlotQuantity(int baseQty, int slot, int remainder)
{
    return baseQty + (slot < remainder ? 1 : 0);
}
```

---

## Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | T2 |
| **helper_name** | `ValidateAndAdjustBucketSum` |
| **concern** | Audits post-distribution bucket sum integrity and applies in-place panic-adjustment correction for integer-division rounding edge cases; isolates the invariant-enforcement path from the distribution loop |
| **signature** | `private static void ValidateAndAdjustBucketSum(int[] buckets, int contracts, int count)` |
| **lines_to_move** | Entire post-loop audit block: `int sum = buckets.Sum(); if (sum != contracts) { buckets[count - 1] += (contracts - sum); }` — replace with single call `ValidateAndAdjustBucketSum(buckets, contracts, count);` |
| **cyc_reduction** | 1 (if-guard `sum != contracts` moves from parent to helper) |
| **projected_helper_cyc** | 2 (base=1 + if-guard=1) |

### Ticket 2 — Pre/Post Pseudocode

**Before (parent post-loop block):**
```csharp
int sum = buckets.Sum();
if (sum != contracts)
{
    buckets[count - 1] += (contracts - sum);
}
```

**After (parent post-loop call):**
```csharp
ValidateAndAdjustBucketSum(buckets, contracts, count);
```

**New helper:**
```csharp
private static void ValidateAndAdjustBucketSum(int[] buckets, int contracts, int count)
{
    int sum = buckets.Sum();
    if (sum != contracts)
    {
        buckets[count - 1] += (contracts - sum);
    }
}
```

---

## CYC Projection Summary

| Symbol | Role | Projected CYC | CYC <= 8? |
|---|---|---|---|
| `ComputeSlotQuantity` | Helper T1 | 2 | YES |
| `ValidateAndAdjustBucketSum` | Helper T2 | 2 | YES |
| `GetTargetDistribution` (parent, post-all) | Parent | 3–4 | YES |

**projected_parent_cyc_after_all: 4** (conservative index-aligned; architecture plan projects 3)

All values satisfy CYC <= 8 (Jane Street strict standard). Extraction plan is valid.

---

## Sequential Thinking Validation

3-thought chain completed:
- **Thought 1:** Ticket count decision — 2 tickets justified; one per distinct concern per Jane Street "Extract Loop Body" and single-responsibility rules.
- **Thought 2:** Per-ticket line mapping — loop body (T1) and audit block (T2) cleanly separable with no shared state beyond the `buckets` array passed by reference.
- **Thought 3:** CYC verification — all helpers at CYC=2, parent at CYC 3–4 post-extraction; all <= 8 under both architecture-plan and jCodemunch-index measurement. PASS.

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC <= 8 (max projected = 4) | PASS |
| Single-responsibility per helper | PASS |
| Extract Loop Body applied (T1) | PASS |
| Invariant isolation applied (T2) | PASS |
| Lock-free preserved | PASS |
| Public signature unchanged | PASS |
| Zero heap allocations added | PASS |

---

## Agent Tracking

```
Agent Name:     v12-phase4-tickets
Bobcoins Used:  1.0
Execution Time: 2026-06-29T01:20:00Z
Epic:           EPIC-W7-007
Wave:           7
Phase:          4
ticket_count:   2
Input:          docs/brain/EPIC-W7-007/02-architecture-plan.md + docs/brain/EPIC-W7-007/03-audit-report.md
Output:         docs/brain/EPIC-W7-007/04-tickets.md
Method:         V12_PureLogic.GetTargetDistribution
Source:         src/V12_002.PureLogic.cs
CYC Index:      6
CYC Projected:  4 (parent post-all), 2 (each helper)
DNA Verdict:    PASS
MCP Tools:      resolve_repo, get_symbol_complexity, get_extraction_candidates, sequentialthinking(3)
```
