# Phase 2: Architecture Plan — EPIC-W7-037

## Method Under Extraction

- **Method:** `SymmetryNormalizeTradeType`
- **Source File:** `src/V12_002.Symmetry.Replace.cs`
- **Lines:** 322–341
- **Visibility:** `private`
- **Return type:** `string`
- **Original CYC:** 9 (project-canonical; McCabe-strict = 10)

### jcodemunch get_context_bundle result

`get_context_bundle` returned error "Symbol(s) not found" for symbol_id `SymmetryNormalizeTradeType`. Fallback `search_symbols` succeeded and resolved two entries:

| Symbol ID | File | Line | Signature |
|---|---|---|---|
| `src/V12_002.Symmetry.Replace.cs::V12_002.SymmetryNormalizeTradeType#method` | `src/V12_002.Symmetry.Replace.cs` | 322 | `private string SymmetryNormalizeTradeType(string raw)` |
| `src-vm-backup/V12_002.Symmetry.Replace.cs::V12_002.SymmetryNormalizeTradeType#method` | `src-vm-backup/V12_002.Symmetry.Replace.cs` | 322 | `private string SymmetryNormalizeTradeType(string raw)` |

Key finding: method is a private leaf (no callees); pure functional transformation returning a canonical string from `{"GENERIC","TREND","RETEST","FFMA","MOMO","RMA","OR"}`.

### jcodemunch get_call_hierarchy result

- **Symbol resolved:** `src/V12_002.Symmetry.Replace.cs::V12_002.SymmetryNormalizeTradeType#method`
- **Callers (depth 1, AST-resolved):** 1
  - `SymmetryInferTradeType` — `src/V12_002.Symmetry.Replace.cs:304` (AST-resolved)
- **Additional callers (from source grep, 00-hotspots.md):**
  - `SymmetryGuardBeginDispatch` — `src/V12_002.Symmetry.cs:146`
  - `SymmetryFindDispatchForMasterFill` — `src/V12_002.Symmetry.cs:332`
- **Callees:** 0 (leaf method)
- **Depth reached:** 1

### jcodemunch get_dependency_graph result

- **Direction:** both, depth 1
- **Node count:** 1 (file has no resolved import/importer edges in graph)
- **Edges:** 0
- **Finding:** Partial-class architecture — `V12_002.Symmetry.Replace.cs` is a partial class file; cross-file coupling exists at source level (callers in `V12_002.Symmetry.cs`) but import-graph edges are not resolved due to partial class split. Blast radius confined to `src/V12_002.Symmetry.Replace.cs` for the extraction itself.

### jcodemunch get_extraction_candidates result

- **Candidates returned:** 0
- **Reason:** Index complexity data not fully populated for this file (partial class split). Manual analysis from 00-hotspots.md used as authoritative source. Two extraction candidates confirmed from source: `IsOrTradeType` and `NormalizeTradeTypeKernel`.

---

## Sequential Thinking Summary

**Thought 5 (final verdict):**

All 5 Jane Street rules satisfied. CYC<=8: max projected CYC = 7 (`NormalizeTradeTypeKernel`), all methods <= 8. Single-responsibility: `IsOrTradeType` handles ONLY the 3-predicate OR classification; `NormalizeTradeTypeKernel` handles ONLY the sequential prefix-match dispatch on an already-uppercased string; parent handles ONLY null-guard + uppercasing + delegation. Actor/Enqueue — not applicable; this is a pure functional transformation with no state mutations or lock blocks. Illegal states unrepresentable — the method returns one of a fixed canonical set; unrecognized input safely falls back to "GENERIC". Zero-allocation hot path — all helpers are `private static`, take a string by value, return a string literal constant; no LINQ, no closures, no new heap allocations beyond the existing `ToUpperInvariant`. Final plan: extract `IsOrTradeType` (CYC 3) and `NormalizeTradeTypeKernel` (CYC 7), parent becomes CYC 2. `max_cyc_projected = 7`. `extraction_count = 2`.

**Chain summary (thoughts 1–4):**

1. CYC 9 confirmed from source; compound OR branch (3 predicates on one line) is the primary driver; method is a pure-functional leaf accepting raw and pre-normalized strings.
2. Strategy B (2 helpers: `IsOrTradeType` + `NormalizeTradeTypeKernel`) chosen over strategy A (1 helper) for superior single-responsibility alignment; both strategies meet CYC <= 8.
3. CYC projections verified per method: `IsOrTradeType` = 3, `NormalizeTradeTypeKernel` = 7, parent = 2; all <= 8.
4. Risk analysis: prefix-priority ordering invariant (RETEST > TREND/FFMA/MOMO > RMA > OR) must be preserved exactly in kernel; extraction is safe since parent signature is unchanged; blast radius stays within single file.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `IsOrTradeType` | Encapsulates the three-predicate OR classification: `t.StartsWith("OR", Ordinal) \|\| t.Contains("ORLONG") \|\| t.Contains("ORSHORT")`. Returns `bool`. Makes the OR-type detection independently testable and removes the compound expression from the parent. | 3 |
| `NormalizeTradeTypeKernel` | Takes the already-uppercased string `t` and performs the sequential prefix-match chain: TREND → RETEST → FFMA → MOMO → RMA → `IsOrTradeType` → GENERIC. Returns the canonical trade type string. Separates null-guard + transformation concern (parent) from classification concern (kernel). | 7 |

### Method Signatures

```csharp
private static bool IsOrTradeType(string t)

private static string NormalizeTradeTypeKernel(string t)
```

### Resulting Parent Body

```csharp
private string SymmetryNormalizeTradeType(string raw)
{
    if (string.IsNullOrEmpty(raw))
        return "GENERIC";

    string t = raw.ToUpperInvariant();
    return NormalizeTradeTypeKernel(t);
}
```

### NormalizeTradeTypeKernel Body (design)

```csharp
private static string NormalizeTradeTypeKernel(string t)
{
    if (t.StartsWith("TREND",  StringComparison.Ordinal)) return "TREND";
    if (t.StartsWith("RETEST", StringComparison.Ordinal)) return "RETEST";
    if (t.StartsWith("FFMA",   StringComparison.Ordinal)) return "FFMA";
    if (t.StartsWith("MOMO",   StringComparison.Ordinal)) return "MOMO";
    if (t.StartsWith("RMA",    StringComparison.Ordinal)) return "RMA";
    if (IsOrTradeType(t))                                  return "OR";
    return "GENERIC";
}
```

### IsOrTradeType Body (design)

```csharp
private static bool IsOrTradeType(string t)
{
    return t.StartsWith("OR", StringComparison.Ordinal)
        || t.Contains("ORLONG")
        || t.Contains("ORSHORT");
}
```

---

## Parent Method After Extraction

- **Remaining logic:** Null/empty guard returning `"GENERIC"` + `ToUpperInvariant` transformation + tail call to `NormalizeTradeTypeKernel`.
- **Projected CYC:** 2 (base path + 1 null-guard branch)

---

## max_cyc_projected: 7
## extraction_count: 2

---

## Jane Street Alignment

| Rule | Status |
|---|---|
| CYC<=8 achieved | YES — max projected CYC = 7 across all 3 methods |
| Single-responsibility per helper | YES — `IsOrTradeType` = OR predicate only; `NormalizeTradeTypeKernel` = prefix classification only; parent = null-guard + delegation only |
| Lock-free/Actor pattern preserved | YES — pure functional method; no state mutations, no lock blocks; Actor/Enqueue pattern not applicable |
| Illegal states unrepresentable | YES — return values are string literals from the fixed canonical set `{"GENERIC","TREND","RETEST","FFMA","MOMO","RMA","OR"}`; no new invalid return paths introduced |
| Zero-allocation hot path | YES — all helpers are `private static`; no closures, no LINQ, no heap allocations beyond the existing `ToUpperInvariant` |
| V12.23 No Scope Creep | YES — helpers added as `private static` to same partial class in same file; no cross-file changes; no caller modifications |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Epic** | EPIC-W7-037 |
| **Bobcoins Used** | 4 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **jcodemunch tools called** | `get_context_bundle` (fallback to `search_symbols`), `get_call_hierarchy`, `get_dependency_graph`, `get_extraction_candidates` |
| **sequential-thinking calls** | 5 |
| **Output** | `docs/brain/EPIC-W7-037/02-architecture-plan.md` |
