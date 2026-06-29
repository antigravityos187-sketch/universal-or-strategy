# Phase 2: Architecture Plan -- EPIC-W7-152

**Agent:** v12-phase2-architecture
**Wave:** 7 | **Phase:** 2 -- Architecture Planning
**Generated:** 2026-06-29T01:30:00Z

---

## Method Under Extraction

- **Method:** `TryApplyConfigTarget_Value`
- **Source File:** `src/V12_002.UI.IPC.Commands.Config.cs`
- **Original CYC:** 17 (manual static analysis from 00-hotspots.md; precomputed=0 is a tooling stub)
- **Lines:** 209-297

### jcodemunch get_context_bundle result

Symbol resolved via `get_context_bundle` using ID `src/V12_002.UI.IPC.Commands.Config.cs::V12_002.TryApplyConfigTarget_Value#method`. Full source confirmed: 89-line method containing 5 sequential `if (key == "Tn")` arms (T1-T5), each with a nested `double.TryParse` + `ValidateIpcMultiplier` + assignment triple, plus one `if (key == "CIT")` arm for `ChaseIfTouchPoints = val`. Method signature: `private bool TryApplyConfigTarget_Value(string key, string val)`. Return type `bool` -- returns `true` on key match, `false` if no key matched.

### jcodemunch get_call_hierarchy result

- **Direct caller (depth 1):** `TryApplyConfigTargets` (line 196, same file) -- `ast_resolved`
- **Indirect caller (depth 2):** `HandleConfigCommand` (line 153, same file) -- `ast_resolved`
- **Callees (depth 1):** `ValidateIpcMultiplier` (src/V12_002.UI.IPC.cs line 134) -- `ast_inferred`
- **Caller count:** 2 (1 direct, 1 transitive) | **Callee count:** 2 (both resolve to `ValidateIpcMultiplier` -- src and src-vm-backup versions)

### jcodemunch get_dependency_graph result

File `src/V12_002.UI.IPC.Commands.Config.cs` has **0 import edges and 0 importer edges** at depth=1 per index. Node count: 1. Edge count: 0. The file is a self-contained partial class -- all dependencies (namespaces, NinjaTrader APIs) are resolved via using directives rather than tracked file imports. No cross-file blast radius from a dependency perspective.

### jcodemunch get_extraction_candidates result

Tool returned **0 candidates** for `min_complexity=3, min_callers=1`. Index lacks per-symbol complexity data for this file (complexity stored as 0 at index time). Manual analysis via `get_context_bundle` source is authoritative. Extraction plan derived from source inspection.

---

## Sequential Thinking Summary

**Final thought (thought 5/5):**

EXTRACTION PLAN (confirmed):
1. Add `private static readonly Dictionary<string, Action<double>> _numericTargetMap` field initialized with T1->Target1Value, T2->Target2Value, T3->Target3Value, T4->Target4Value, T5->Target5Value lambdas. Replaces the 5-arm if-chain (eliminates 4 CYC points from key dispatch).
2. Extract private helper method `ApplyValidatedTargetValue(string val, string label, Action<double> assign)`. Implements the TryParse + ValidateIpcMultiplier + assign triple using guard clauses (early returns). CYC = 3.
3. Rewrite parent `TryApplyConfigTarget_Value` to: (a) guard CIT first (early return), (b) dictionary TryGetValue lookup, (c) call `ApplyValidatedTargetValue` if key found. CYC = 3.

Jane Street alignment final check: CYC<=8 PASS on all symbols (max=3). Extract Guard Clauses: YES. Replace if-chains with Lookup Tables: YES (Dictionary dispatch). Single-responsibility per helper: YES. Lock-free: YES. ASCII-only: YES. ONE method per epic: YES. xUnit tests required for `ApplyValidatedTargetValue` in Phase 5.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `ApplyValidatedTargetValue(string val, string label, Action<double> assign)` | Parse `val` as double; if parse succeeds, call `ValidateIpcMultiplier`; if valid call `assign(v)`; else `Print` rejection message. Guard-clause style with early returns. | 3 |
| `_numericTargetMap` (static readonly field) | `Dictionary<string, Action<double>>` mapping `"T1"` through `"T5"` to their respective `TargetNValue` property setters. Eliminates all 5 key-dispatch if-arms from the parent. | N/A (field) |

### Rewritten parent method (pseudocode)

```csharp
private bool TryApplyConfigTarget_Value(string key, string val)
{
    if (key == "CIT") { ChaseIfTouchPoints = val; return true; }
    if (_numericTargetMap.TryGetValue(key, out Action<double> assign))
    {
        ApplyValidatedTargetValue(val, key, assign);
        return true;
    }
    return false;
}
```

### Extracted helper (pseudocode)

```csharp
private void ApplyValidatedTargetValue(string val, string label, Action<double> assign)
{
    if (!double.TryParse(val, out double v)) return;
    string vmReason;
    if (!ValidateIpcMultiplier(v, out vmReason))
    {
        Print($"[IPC REJECT] {label} value {v} rejected: {vmReason}");
        return;
    }
    assign(v);
}
```

### Static field declaration (pseudocode)

```csharp
private static readonly Dictionary<string, Action<double>> _numericTargetMap =
    new Dictionary<string, Action<double>>
    {
        { "T1", v => Target1Value = v },
        { "T2", v => Target2Value = v },
        { "T3", v => Target3Value = v },
        { "T4", v => Target4Value = v },
        { "T5", v => Target5Value = v },
    };
```

> **Note:** `Target1Value`..`Target5Value` are instance properties. The lambdas capture `this` implicitly, so the field must be non-static (instance field), initialized in the class body or constructor. Phase 5 engineer to confirm NinjaTrader partial class conventions and use an instance field or lazy initialization if static capture is not supported.

---

## Parent Method After Extraction

- **Remaining logic:** CIT guard (early return), dictionary dispatch via `_numericTargetMap.TryGetValue`, call to `ApplyValidatedTargetValue`, fall-through `return false`
- **Projected CYC:** 3 (base=1 + CIT-guard=1 + TryGetValue-guard=1)

---

## max_cyc_projected: 3
## extraction_count: 1

---

## Jane Street Alignment

- **CYC<=8 achieved:** YES -- all symbols at CYC 3 (parent=3, helper=3)
- **Single-responsibility per helper:** YES -- `ApplyValidatedTargetValue` does exactly one thing: parse, validate, assign
- **Lock-free/Actor pattern preserved:** YES -- no lock blocks present or introduced; IPC handler thread model unchanged
- **Illegal states unrepresentable:** YES -- dispatch table ensures only valid numeric-target keys reach `ApplyValidatedTargetValue`; CIT separated; unknown keys return false without reaching any property assignment
- **Extract Guard Clauses:** YES -- TryParse and ValidateIpcMultiplier checks use early returns instead of nested if-else
- **Replace if-chains with Lookup Tables:** YES -- 5-arm `if (key == "Tn")` chain replaced by `Dictionary<string, Action<double>>` dispatch table (0 runtime branches for key lookup)
- **ASCII-only string literals:** YES -- `[IPC REJECT]` prefix and all string literals are ASCII-only
- **ONE method per epic:** YES -- only `TryApplyConfigTarget_Value` is modified; no sibling methods touched
- **xUnit [Fact] tests required (Phase 5):** `ApplyValidatedTargetValue` -- 4 test cases: (1) parse-fail (no-op), (2) validate-fail (Print rejection, no assign), (3) validate-success (assign called), (4) CIT key handled by parent (assign ChaseIfTouchPoints)

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 2.1 |
| **Execution Time** | 2026-06-29T01:30:00Z |
| **jcodemunch tools called** | get_context_bundle, get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **MCP Repo** | antigravityos187-sketch/universal-or-strategy |
| **Input CYC (hotspots)** | 17 |
| **max_cyc_projected** | 3 |
| **extraction_count** | 1 |
| **boundary_verdict** | PASS (from Phase 1.5) |
