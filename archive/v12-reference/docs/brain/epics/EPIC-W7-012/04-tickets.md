# EPIC-W7-012 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29
**Inputs:**
- `docs/brain/EPIC-W7-012/02-architecture-plan.md`
- `docs/brain/EPIC-W7-012/03-audit-report.md`

---

## Method Under Refactor

| Field | Value |
|---|---|
| **Method** | `SyncPanelConfigFromSnapshot` |
| **File** | `src/V12_002.UI.Panel.StateSync.cs` |
| **Signature** | `private void SyncPanelConfigFromSnapshot(UIStateSnapshot snapshot)` |
| **Lines** | 460–512 (53 lines) |
| **CYC (MCP confirmed)** | 19 (cyclomatic=19, assessment="high", max_nesting=3, param_count=1) |
| **Only Caller** | `UpdatePanelState` (same file, line 13) |
| **DNA Verdict (Phase 3)** | PASS — zero violations |

---

## ticket_count: 3

---

## Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | 1 |
| **helper_name** | `SyncTargetValueControls` |
| **concern** | Sync the 5 target-value text controls (svT1Val..svT5Val) — null-guard each control then assign `.Text = FormatPanelDouble(config.TargetNValue)` |
| **signature** | `private void SyncTargetValueControls(UIConfigSnapshot config)` |
| **inlining_hint** | `[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]` |
| **lines_to_move** | Lines ~462–471: the five `if (svT1Val != null) svT1Val.Text = ...` through `if (svT5Val != null) svT5Val.Text = ...` blocks |
| **cyc_reduction** | 5 (removes 5 null-guard branches from parent) |
| **projected_helper_cyc** | 6 (1 base + 5 null guards) ✅ ≤ 8 |
| **call_site_in_parent** | `SyncTargetValueControls(config);` — replace extracted block with this single call |

### Helper Body

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

---

## Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | 2 |
| **helper_name** | `SyncTargetTypeControls` |
| **concern** | Sync the 5 target-type combo controls (svT1Type..svT5Type) — null-guard each control then call `SetComboSelection(..., GetPanelTargetModeText(...))` |
| **signature** | `private void SyncTargetTypeControls(UIConfigSnapshot config)` |
| **inlining_hint** | `[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]` |
| **lines_to_move** | Lines ~473–482: the five `if (svT1Type != null) SetComboSelection(...)` through `if (svT5Type != null) SetComboSelection(...)` blocks |
| **cyc_reduction** | 5 (removes 5 null-guard branches from parent) |
| **projected_helper_cyc** | 6 (1 base + 5 null guards) ✅ ≤ 8 |
| **call_site_in_parent** | `SyncTargetTypeControls(config);` — replace extracted block with this single call |

### Helper Body

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

---

## Ticket 3

| Field | Value |
|---|---|
| **ticket_id** | 3 |
| **helper_name** | `SyncScalarControls` |
| **concern** | Sync the 4 scalar controls (strVal, maxVal, citVal, svStrType) — null-guard each control then assign text or combo selection; includes IsNullOrEmpty ternary for citVal and Mode=="ORB" ternary for svStrType |
| **signature** | `private void SyncScalarControls(UIConfigSnapshot config, UIStateSnapshot snapshot)` |
| **inlining_hint** | `[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]` |
| **lines_to_move** | Lines ~484–505: the four null-guard blocks for strVal (FormatPanelDouble), maxVal (FormatPanelDouble), citVal (IsNullOrEmpty ternary → "0"), and svStrType (Mode=="ORB" ternary → "OR"/"ATR") |
| **cyc_reduction** | 6 (removes 4 null-guard branches + 2 ternary branches from parent) |
| **projected_helper_cyc** | 7 (1 base + 4 null guards + 2 ternaries) ✅ ≤ 8 |
| **call_site_in_parent** | `SyncScalarControls(config, snapshot);` — replace extracted block with this single call |

### Helper Body

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

---

## Parent Method After All Extractions

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

**projected_parent_cyc_after_all: 2** (1 base + 1 null-coalescing `??`) ✅ ≤ 8

---

## CYC Projection Summary

| Symbol | CYC Before | CYC After | Meets ≤ 8? |
|---|---|---|---|
| `SyncPanelConfigFromSnapshot` (parent) | 19 | **2** | ✅ PASS |
| `SyncTargetValueControls` | N/A (new) | **6** | ✅ PASS |
| `SyncTargetTypeControls` | N/A (new) | **6** | ✅ PASS |
| `SyncScalarControls` | N/A (new) | **7** | ✅ PASS |

**max_cyc_projected: 7**

---

## Sequential Thinking Evidence

### Thought 1 — Ticket Count
Rule: one ticket = one extracted helper = one concern. Phase 2 defines 3 helpers with 3 distinct concerns (T-val text, T-type combos, scalar controls). **ticket_count = 3.**

### Thought 2 — Per-Ticket Detail
- Ticket 1: lines ~462-471, removes 5 T-val guard branches → helper CYC 6
- Ticket 2: lines ~473-482, removes 5 T-type guard branches → helper CYC 6
- Ticket 3: lines ~484-505, removes 4 guard + 2 ternary branches → helper CYC 7
- Parent retains: base + `??` → CYC 2

### Thought 3 — CYC Verification
All 4 symbols ≤ 8 post-extraction. Max helper CYC = 7. Parent CYC = 2. Hypothesis verified with HIGH confidence.

---

## MCP Evidence

| Tool | Result |
|---|---|
| `resolve_repo` | indexed=true, symbol_count=5147, file_count=2000 |
| `get_symbol_complexity` | cyclomatic=19, max_nesting=3, param_count=1, lines=53, assessment="high" |
| `get_extraction_candidates` | candidates=[] (min_callers=2 not met — method has 1 caller; extraction driven by CYC, not multi-caller) |
| `search_symbols` | Confirmed symbol at `src/V12_002.UI.Panel.StateSync.cs`, line 460 |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 2.0 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **ticket_count** | 3 |
| **max_cyc_projected** | 7 |
| **projected_parent_cyc_after_all** | 2 |
| **Status** | COMPLETE |
