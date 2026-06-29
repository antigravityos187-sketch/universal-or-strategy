# EPIC-W7-012 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T01:10:00Z
**Input:** docs/brain/EPIC-W7-012/01-scope-boundary.md

---

## Original Method

| Field | Value |
|---|---|
| **Method** | `SyncPanelConfigFromSnapshot` |
| **File** | `src/V12_002.UI.Panel.StateSync.cs` |
| **Lines** | 460–512 (53 lines) |
| **Signature** | `private void SyncPanelConfigFromSnapshot(UIStateSnapshot snapshot)` |
| **CYC (confirmed MCP)** | 19 |
| **Max Nesting** | 3 |
| **Params** | 1 (`UIStateSnapshot snapshot`) |
| **Only Caller** | `UpdatePanelState` (same file, line 13) |

---

## Complexity Driver Analysis

The CYC=19 breaks down as follows:

| Driver | Count | CYC contribution |
|---|---|---|
| Base | 1 | +1 |
| `if (svT1Val..svT5Val != null)` null guards | 5 | +5 |
| `if (svT1Type..svT5Type != null)` null guards | 5 | +5 |
| `if (strVal != null)`, `if (maxVal != null)`, `if (citVal != null)`, `if (svStrType != null)` | 4 | +4 |
| Ternary inside `citVal` block (`IsNullOrEmpty`) | 1 | +1 |
| Ternary inside `svStrType` block (`Mode=="ORB"`) | 1 | +1 |
| Null-coalescing `??` on `snapshot.Config` | 1 | +1 |
| **Total** | | **19** |

The 5+5 T-val / T-type guards are parallel-structured repetitive branches — prime extraction targets.
The 4 scalar guards handle distinct controls but can be consolidated into a single helper.

---

## Extraction Plan

### Extracted Helpers

| Helper Name | Signature | Responsibility | Lines Moved | Projected CYC |
|---|---|---|---|---|
| `SyncTargetValueControls` | `private void SyncTargetValueControls(UIConfigSnapshot config)` | Null-guard + assign `.Text = FormatPanelDouble(...)` for svT1Val..svT5Val | 10 lines (~462-471) | **6** |
| `SyncTargetTypeControls` | `private void SyncTargetTypeControls(UIConfigSnapshot config)` | Null-guard + `SetComboSelection(..., GetPanelTargetModeText(...))` for svT1Type..svT5Type | 10 lines (~473-482) | **6** |
| `SyncScalarControls` | `private void SyncScalarControls(UIConfigSnapshot config, UIStateSnapshot snapshot)` | Null-guard blocks for strVal, maxVal, citVal (with IsNullOrEmpty ternary), svStrType (with Mode=="ORB" ternary) | 22 lines (~484-505) | **7** |

### Parent After Extraction

```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
private void SyncPanelConfigFromSnapshot(UIStateSnapshot snapshot)
{
    UIConfigSnapshot config = snapshot.Config ?? new UIConfigSnapshot();
    SyncTargetValueControls(config);
    SyncTargetTypeControls(config);
    SyncScalarControls(config, snapshot);
    int count = Math.Max(1, Math.Min(5, snapshot.TargetCount));
    _panelLastSyncedTargetCount = count;
    SyncCountChipVisuals(count);
    UpdateTargetVisibility(count);
}
```

**Parent CYC after extraction:** 2 (base 1 + null-coalescing `??` = 2)

---

## CYC Projection Summary

| Symbol | CYC Before | CYC After | Meets <= 8? |
|---|---|---|---|
| `SyncPanelConfigFromSnapshot` (parent) | 19 | 2 | PASS |
| `SyncTargetValueControls` | N/A (new) | 6 | PASS |
| `SyncTargetTypeControls` | N/A (new) | 6 | PASS |
| `SyncScalarControls` | N/A (new) | 7 | PASS |

**max_cyc_projected: 7**

---

## Helper Method Signatures (Full)

### `SyncTargetValueControls`

```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
private void SyncTargetValueControls(UIConfigSnapshot config)
{
    if (svT1Val != null)
        svT1Val.Text = FormatPanelDouble(config.Target1Value);
    if (svT2Val != null)
        svT2Val.Text = FormatPanelDouble(config.Target2Value);
    if (svT3Val != null)
        svT3Val.Text = FormatPanelDouble(config.Target3Value);
    if (svT4Val != null)
        svT4Val.Text = FormatPanelDouble(config.Target4Value);
    if (svT5Val != null)
        svT5Val.Text = FormatPanelDouble(config.Target5Value);
}
```

**CYC: 6** (1 base + 5 null guards)

### `SyncTargetTypeControls`

```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
private void SyncTargetTypeControls(UIConfigSnapshot config)
{
    if (svT1Type != null)
        SetComboSelection(svT1Type, GetPanelTargetModeText(config.Target1Type));
    if (svT2Type != null)
        SetComboSelection(svT2Type, GetPanelTargetModeText(config.Target2Type));
    if (svT3Type != null)
        SetComboSelection(svT3Type, GetPanelTargetModeText(config.Target3Type));
    if (svT4Type != null)
        SetComboSelection(svT4Type, GetPanelTargetModeText(config.Target4Type));
    if (svT5Type != null)
        SetComboSelection(svT5Type, GetPanelTargetModeText(config.Target5Type));
}
```

**CYC: 6** (1 base + 5 null guards)

### `SyncScalarControls`

```csharp
[System.Runtime.CompilerServices.MethodImpl(
    System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
private void SyncScalarControls(UIConfigSnapshot config, UIStateSnapshot snapshot)
{
    if (strVal != null)
    {
        strVal.Text = FormatPanelDouble(config.StopValue);
    }

    if (maxVal != null)
    {
        maxVal.Text = FormatPanelDouble(config.MaxRiskValue);
    }

    if (citVal != null)
    {
        citVal.Text = string.IsNullOrEmpty(config.ChaseIfTouchPoints) ? "0" : config.ChaseIfTouchPoints;
    }

    if (svStrType != null)
    {
        SetComboSelection(
            svStrType,
            string.Equals(snapshot.Mode, "ORB", StringComparison.OrdinalIgnoreCase) ? "OR" : "ATR"
        );
    }
}
```

**CYC: 7** (1 base + 4 null guards + 2 ternaries)

---

## Jane Street Alignment Notes

| Principle | Application |
|---|---|
| **carl_cook: zero-alloc hot path** | No new allocations introduced; `config` reference passed by value (reference type, no copy overhead). `new UIConfigSnapshot()` only on null path (cold). |
| **carl_cook: extract cold logging out-of-line** | Null-guard branches are defense-in-depth safety checks; moved out of parent with `NoInlining` to keep parent hot path small. |
| **carl_cook: AggressiveInlining hot / NoInlining cold** | Parent coordinator marked `AggressiveInlining`; helpers marked `NoInlining` (each helper is a cold branch-heavy call). |
| **carl_cook: avoid LINQ** | No LINQ anywhere — plain imperative null-guard + assignment. |
| **gjengset: no new lock() blocks** | No locks added. UI sync is single-threaded (NinjaTrader UI thread). |
| **trading_billions: single responsibility per helper** | `SyncTargetValueControls` = T-val text only. `SyncTargetTypeControls` = T-type combos only. `SyncScalarControls` = scalar stop/max/cit/str controls only. |
| **trading_billions: each helper CYC <= 8** | Max helper CYC = 7. All helpers <= 8. ✅ |
| **trading_billions: defense in depth** | All null guards preserved inside helpers — not removed. UI control references remain safely guarded. |

---

## MCP Evidence

| Tool | Parameters | Result |
|---|---|---|
| `mcp__jcodemunch-mcp__resolve_repo` | `path="/home/malhitticrypto/universal-or-strategy"` | Indexed: true, symbols: 5147, files: 2000 |
| `mcp__jcodemunch-mcp__get_symbol_source` | `symbol_id="src/V12_002.UI.Panel.StateSync.cs::V12_002.SyncPanelConfigFromSnapshot#method"` | Source confirmed: lines 460-512, CYC=19, signature `private void SyncPanelConfigFromSnapshot(UIStateSnapshot snapshot)` |
| `mcp__jcodemunch-mcp__get_call_hierarchy` | `symbol_id="src/V12_002.UI.Panel.StateSync.cs::V12_002.SyncPanelConfigFromSnapshot#method"`, `depth=2`, `direction=both` | Callers: 1 (`UpdatePanelState` at line 13, same file). Callees: `FormatPanelDouble`, `SetComboSelection`, `GetPanelTargetModeText`, `SyncCountChipVisuals`, `UpdateTargetVisibility` |
| `mcp__jcodemunch-mcp__get_dependency_graph` | `file="src/V12_002.UI.Panel.StateSync.cs"`, `direction=both`, `depth=1` | 1 node, 0 edges — file is self-contained partial class, no cross-file import edges |

---

## Sequential Thinking Evidence

### Thought 1 — Complexity Drivers Analysis
Enumerated all 19 CYC branches:
- Base: 1
- T-val null guards (svT1Val..svT5Val): 5
- T-type null guards (svT1Type..svT5Type): 5
- Scalar null guards (strVal, maxVal, citVal, svStrType): 4
- Ternaries inside citVal + svStrType blocks: 2
- Null-coalescing on snapshot.Config: 1
- Total: 19 — confirmed match with precomputed CYC=19

### Thought 2 — Extraction Strategy
Designed 3 helpers aligned with Jane Street KB:
- `SyncTargetValueControls(UIConfigSnapshot config)` — takes 5 T-val branches, CYC=6
- `SyncTargetTypeControls(UIConfigSnapshot config)` — takes 5 T-type branches, CYC=6
- `SyncScalarControls(UIConfigSnapshot config, UIStateSnapshot snapshot)` — takes 4 scalar guards + 2 ternaries, CYC=7
- Parent after: CYC=2 (base + `??` operator only)

### Thought 3 — CYC Validation
Verified all helpers <= 8 (max is 7). Parent drops from 19 to 2.
Hypothesis verified: extraction plan is sound, no further iterations required.
Confidence: HIGH.

---

## Boundary Compliance (V12.23)

- **Scope:** `SyncPanelConfigFromSnapshot` body only + 3 new private helpers in same file
- **No callers modified:** `UpdatePanelState` is sole caller, signature unchanged
- **No interface changes:** All helpers are `private`, same partial class
- **No cross-file impact:** Confirmed by dependency graph (0 import edges)
- **Inherited from Phase 1.5 boundary_verdict:** PASS

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **max_cyc_projected** | 7 |
| **Status** | COMPLETE |
