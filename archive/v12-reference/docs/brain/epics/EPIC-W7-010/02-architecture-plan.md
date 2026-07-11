# Phase 2: Architecture Plan — EPIC-W7-010

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-010/01-scope-boundary.md

---

## Method Under Extraction

- **Method:** `ShowModeSpecificControls`
- **Source File:** `src/V12_002.UI.Panel.Handlers.cs`
- **Lines:** 690–719
- **Original CYC:** 8

### jcodemunch get_context_bundle result

Symbol resolved as `src/V12_002.UI.Panel.Handlers.cs::V12_002.ShowModeSpecificControls#method`.
Source is a pure dispatch switch (7 named cases + default), each arm delegates to one void helper.
Docstring confirms post-EPIC-CCN-15 refactor: `[EPIC-CCN-15] Refactored to dispatch-only pattern (CYC 8, Jane Street ultra-aligned)`.
Imports: System, System.Collections.Generic, System.Windows, WPF controls — no external assemblies.

### jcodemunch get_call_hierarchy result

- **Callers (depth=2):**
  - Depth 1: `UpdateContextualUI` (line 654, same file) — direct and sole caller
  - Depth 2: `SelectConfigMode` (line 591, same file) — transitive caller
- **Callees (depth=1):**
  - `ShowOrbControls` (line 724), `ShowRmaControls` (line 732), `ShowRetestControls` (line 738),
    `ShowMomoControls` (line 744), `ShowFfmaControls` (line 750), `ShowTrendControls` (line 760),
    `ShowMnlControls` (line 766) — all 7 leaf helpers, all in same file, all AST-resolved

### jcodemunch get_dependency_graph result

- File: `src/V12_002.UI.Panel.Handlers.cs`
- Import edges: 0 | Importer edges: 0
- Pattern: partial class — cross-file relationships resolved at compile time, not via import edges
- Blast radius is fully contained within the target file

### jcodemunch get_extraction_candidates result

- No extraction candidates returned (min_complexity=3, min_callers=1)
- Confirms all helpers are already extracted; individual arms have CYC 1 (single statement each)
- This validates that the switch body itself is the only remaining complexity concentration point

---

## Sequential Thinking Summary

**Thought 1** — Method is a pure dispatch switch at CYC=8 (at the ceiling). Already post-EPIC-CCN-15. The architectural decision: maintain switch as-is (CYC stays 8, passes but no headroom) vs. dictionary-dispatch (CYC drops to 2, future-safe). Both options achieve CYC<=8.

**Thought 2** — Dictionary-dispatch refactor (Option B) designed: new private field `_modeControlMap` of type `Dictionary<string, Action>`, new helper `InitializeModeControlMap()` builds the map once. Parent uses `TryGetValue` with ORB fallback. Parent CYC drops to 2. Helper CYC = 1.

**Thought 3** — CYC projections verified: `ShowModeSpecificControls` after refactor = CYC 2 (base + 1 if-branch); `InitializeModeControlMap` = CYC 1 (linear init, no branches). max_cyc_projected = 2.

**Thought 4** — All 5 Jane Street rules verified: (1) CYC<=8 achieved (max=2), (2) single-responsibility per method, (3) no lock() blocks or Actor/Enqueue concerns (pure UI dispatch), (4) illegal-state risk preserved via TryGetValue fallback (same ORB default behavior), (5) zero-allocation hot path (dictionary built once, TryGetValue is O(1) hash lookup with no heap allocation on dispatch).

**Thought 5 — Final Verdict** — Option B (dictionary-dispatch) selected. Extraction count: 1 (`InitializeModeControlMap`). max_cyc_projected: 2. All Jane Street rules satisfied. Scope confined to `src/V12_002.UI.Panel.Handlers.cs` (same partial class). No signature changes, no caller modifications.

---

## Extraction Plan

| Helper Method Name | Responsibility | Projected CYC |
|---|---|---|
| `InitializeModeControlMap()` | Build and populate the private `Dictionary<string, Action>` mapping mode strings to their corresponding `ShowXxxControls` delegate; called once during initialization | 1 |

### New Field

```csharp
private Dictionary<string, Action> _modeControlMap;
```

### Refactored Parent Body

```csharp
// [EPIC-W7-010] Dictionary-dispatch replaces 8-arm switch (CYC 8 -> 2)
private void ShowModeSpecificControls(string mode)
{
    if (!_modeControlMap.TryGetValue(mode, out var show))
        show = ShowOrbControls;
    show();
}
```

### New Helper Body

```csharp
private void InitializeModeControlMap()
{
    _modeControlMap = new Dictionary<string, Action>
    {
        { "ORB",    ShowOrbControls    },
        { "RMA",    ShowRmaControls    },
        { "RETEST", ShowRetestControls },
        { "MOMO",   ShowMomoControls   },
        { "FFMA",   ShowFfmaControls   },
        { "TREND",  ShowTrendControls  },
        { "MNL",    ShowMnlControls    }
    };
}
```

`InitializeModeControlMap()` must be called from the class initialization path (e.g., `OnStateChange` or constructor) before `ShowModeSpecificControls` is first invoked.

---

## Parent Method After Extraction

- **Remaining logic:** Single `TryGetValue` dispatch with ORB fallback — pure lookup and invoke
- **Projected CYC:** 2 (1 base + 1 if-branch for key miss)

---

## max_cyc_projected: 2
## extraction_count: 1

---

## Jane Street Alignment

| Rule | Status | Notes |
|---|---|---|
| CYC<=8 achieved | YES | Parent CYC 2, Helper CYC 1; max=2 |
| Single-responsibility per helper | YES | `InitializeModeControlMap` does exactly one thing: build the action map |
| Lock-free/Actor pattern preserved | YES | No state mutations; pure UI visibility dispatch; no lock() introduced or present |
| Illegal states unrepresentable | YES (improved) | TryGetValue with explicit ORB fallback makes the default case visible in code rather than implicit in switch default; contract preserved |
| Zero-allocation hot paths | YES | Dictionary built once at init; TryGetValue is O(1) hash, no heap allocation per dispatch call |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:10:00Z |
| **jcodemunch tools called** | resolve_repo, get_context_bundle (via search_symbols fallback), get_call_hierarchy, get_dependency_graph, get_extraction_candidates |
| **sequential-thinking calls** | 5 |
| **Epic** | EPIC-W7-010 |
| **Wave** | 7 |
| **Phase** | 2 — Architecture Planning |
| **Method** | `ShowModeSpecificControls` |
| **File** | `src/V12_002.UI.Panel.Handlers.cs` |
| **CYC Before** | 8 |
| **CYC After (max projected)** | 2 |
| **Extraction Count** | 1 |
| **Pattern Applied** | Replace Switch/If-Chains with Lookup Tables + Extract Named Helper Methods |
