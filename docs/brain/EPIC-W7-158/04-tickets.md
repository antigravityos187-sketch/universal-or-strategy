# Phase 4: Ticket Definitions — EPIC-W7-158

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Inputs:**
- `docs/brain/EPIC-W7-158/02-architecture-plan.md`
- `docs/brain/EPIC-W7-158/03-audit-report.md`

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-158 |
| **Method** | `SyncModeChipVisuals` |
| **Source File** | `src/V12_002.UI.Panel.StateSync.cs` |
| **Original CYC** | 9 |
| **ticket_count** | **2** |
| **projected_parent_cyc_after_all** | **2** |
| **max_cyc_projected** | **6** |
| **dna_verdict** | PASS (from Phase 3) |

---

## Ticket Definitions

---

### Ticket 1

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-158-T1 |
| **helper_name** | `ResolveActiveModeButton` |
| **signature** | `private Button ResolveActiveModeButton(string mode)` |
| **concern** | Extract the switch statement that maps a mode string to its corresponding WPF Button reference. Returns `modeOrbButton` as default (via `?? "ORB"` normalization). Pure mapping function — no side effects, no state mutations. |
| **lines_to_move** | Segment 2 of `SyncModeChipVisuals` (~11 lines): `Button activeButton = null;` declaration + entire `switch ((mode ?? "ORB").ToUpperInvariant())` block with 5 case arms + default arm. |
| **cyc_reduction** | −5 (removes 5 switch case branches from parent) |
| **projected_helper_cyc** | **6** (base=1 + case "RMA"=1 + case "RETEST"=1 + case "MOMO"=1 + case "FFMA"=1 + case "TREND"=1) |
| **parent_cyc_after_this_ticket** | 4 (base=1 + foreach=1 + null-guard-in-loop=1 + post-switch null-guard=1) |
| **threshold_check** | helper CYC 6 <= 8 ✅ |

**Implementation Steps:**

1. Add new private method `ResolveActiveModeButton(string mode)` to the same partial class in `src/V12_002.UI.Panel.StateSync.cs`.
2. Move the `switch ((mode ?? "ORB").ToUpperInvariant())` block (with all 6 arms) into the new method body.
3. Return `activeButton` from the new method (return type `Button`).
4. In `SyncModeChipVisuals`, replace the moved switch block with: `Button activeButton = ResolveActiveModeButton(mode);`
5. Verify build passes. Verify `UpdatePanelState` caller is unaffected.

**Extracted Method Body:**

```csharp
private Button ResolveActiveModeButton(string mode)
{
    switch ((mode ?? "ORB").ToUpperInvariant())
    {
        case "RMA":    return modeRmaButton;
        case "RETEST": return modeRetestButton;
        case "MOMO":   return modeMomoButton;
        case "FFMA":   return modeFfmaButton;
        case "TREND":  return modeTrendButton;
        default:       return modeOrbButton;
    }
}
```

**Parent After T1 (intermediate state):**

```csharp
private void SyncModeChipVisuals(string mode)
{
    foreach (Button btn in new[] { modeOrbButton, modeRmaButton, modeRetestButton,
                                    modeMomoButton, modeFfmaButton, modeTrendButton })
    {
        if (btn == null) continue;
        btn.Background = BtnBg;
        btn.Foreground = TextMuted;
        btn.BorderBrush = BtnBorder;
    }
    Button activeButton = ResolveActiveModeButton(mode);
    if (activeButton != null)
    {
        activeButton.Background = CyanBg;
        activeButton.Foreground = CyanFg;
        activeButton.BorderBrush = CyanBorder;
    }
}
```

---

### Ticket 2

| Field | Value |
|---|---|
| **ticket_id** | EPIC-W7-158-T2 |
| **helper_name** | `ResetModeChipStyles` |
| **signature** | `private void ResetModeChipStyles()` |
| **concern** | Extract the foreach reset loop that iterates all 6 mode buttons, skips null references, and resets `Background`, `Foreground`, and `BorderBrush` to default brush values (`BtnBg`, `TextMuted`, `BtnBorder`). |
| **lines_to_move** | Segment 1 of `SyncModeChipVisuals` (~7 lines): entire `foreach (Button btn in new[] { ... })` block including the `if (btn == null) continue;` null guard and the 3 property assignments. |
| **cyc_reduction** | −2 (removes foreach iteration branch + null-guard branch from parent) |
| **projected_helper_cyc** | **3** (base=1 + foreach=1 + if(btn==null)=1) |
| **parent_cyc_after_this_ticket** | 2 (base=1 + post-switch null-guard=1) |
| **threshold_check** | helper CYC 3 <= 8 ✅ |

**Implementation Steps:**

1. Add new private method `ResetModeChipStyles()` to the same partial class in `src/V12_002.UI.Panel.StateSync.cs`.
2. Move the entire foreach block (including null guard and 3 property assignments) into the new method body.
3. In `SyncModeChipVisuals`, replace the moved foreach block with: `ResetModeChipStyles();`
4. Verify build passes. Verify `UpdatePanelState` caller is unaffected.

**Extracted Method Body:**

```csharp
private void ResetModeChipStyles()
{
    foreach (Button btn in new[] { modeOrbButton, modeRmaButton, modeRetestButton,
                                    modeMomoButton, modeFfmaButton, modeTrendButton })
    {
        if (btn == null) continue;
        btn.Background = BtnBg;
        btn.Foreground = TextMuted;
        btn.BorderBrush = BtnBorder;
    }
}
```

**Parent After T1 + T2 (final state):**

```csharp
private void SyncModeChipVisuals(string mode)
{
    ResetModeChipStyles();
    Button activeButton = ResolveActiveModeButton(mode);
    if (activeButton != null)
    {
        activeButton.Background = CyanBg;
        activeButton.Foreground = CyanFg;
        activeButton.BorderBrush = CyanBorder;
    }
}
```

**Final parent CYC:** base(1) + post-switch null-guard(1) = **2** ✅

---

## CYC Projection Summary

| Method | Role | Projected CYC | Threshold | Status |
|---|---|---|---|---|
| `SyncModeChipVisuals` (parent, post both extractions) | Orchestrator | **2** | <= 8 | ✅ PASS |
| `ResolveActiveModeButton(string mode)` | Switch mapper | **6** | <= 8 | ✅ PASS |
| `ResetModeChipStyles()` | Reset pass | **3** | <= 8 | ✅ PASS |
| **max_cyc_projected** | | **6** | <= 8 | ✅ **PASS** |

Original CYC reduced from **9** → max **6** (33% reduction). All three methods well within Jane Street threshold.

---

## MCP Evidence

### STEP 0a — resolve_repo

```json
{
  "found": true,
  "indexed": true,
  "repo": "antigravityos187-sketch/universal-or-strategy",
  "symbol_count": 5147,
  "file_count": 2000,
  "status": "loadable",
  "source_root": "/home/malhitticrypto/universal-or-strategy",
  "indexed_at": "2026-06-29T01:05:21.006184"
}
```

### STEP 2 — get_symbol_complexity (SyncModeChipVisuals)

```json
{
  "error": "Symbol 'SyncModeChipVisuals' not found in index."
}
```

**Note:** Symbol not found in index — consistent with Phase 2 finding that the jCodemunch index does not track private methods from same-file intra-class calls. CYC=9 confirmed from Phase 0 hotspot analysis and validated in Phase 2 architecture plan via manual decomposition.

### STEP 3 — get_extraction_candidates (src/V12_002.UI.Panel.StateSync.cs)

```json
{
  "repo": "antigravityos187-sketch/universal-or-strategy",
  "file": "src/V12_002.UI.Panel.StateSync.cs",
  "candidates": [],
  "min_complexity": 5,
  "min_callers": 2
}
```

**Note:** Empty candidates — consistent with Phase 2 finding that the index requires min_callers=2 for extraction candidates. The target method has 1 caller (intra-class). Extraction plan derived from Phase 2 architecture plan (get_context_bundle source + CYC decomposition).

---

## Sequential Thinking Evidence

### Thought 1 — How many tickets?

From the architecture plan (Phase 2) and audit report (Phase 3), `SyncModeChipVisuals` has CYC=9 and needs to be reduced to <=8. The plan identifies 2 extractions:
1. `ResolveActiveModeButton(string mode)` — extracts the switch block (5 case arms)
2. `ResetModeChipStyles()` — extracts the foreach reset loop with null guard

Each extraction is a distinct, independent surgical operation targeting a separate logical segment of the parent method. Per V12 protocol: one ticket per extracted helper. **ticket_count = 2.**

### Thought 2 — For each ticket: helper name, extracted logic, projected CYC

**TICKET 1: Extract ResolveActiveModeButton**
- Helper name: `ResolveActiveModeButton`
- Concern: Map mode string to WPF Button reference via switch statement
- Lines to move: activeButton declaration + switch block with 5 arms (~11 lines)
- CYC contribution removed from parent: −5 (5 switch case arms)
- Projected helper CYC: base(1) + 5 switch arms = **6** ✅
- Parent CYC after this ticket: 9 − 5 = **4**

**TICKET 2: Extract ResetModeChipStyles**
- Helper name: `ResetModeChipStyles`
- Concern: Iterate 6 mode buttons, skip nulls, reset brush properties
- Lines to move: foreach loop block (~7 lines)
- CYC contribution removed from parent: −2 (foreach + null guard)
- Projected helper CYC: base(1) + foreach(1) + null-guard(1) = **3** ✅
- Parent CYC after this ticket: 4 − 2 = **2**

### Thought 3 — Verify parent and all helpers CYC <= 8

- `SyncModeChipVisuals` (post both): base(1) + null-guard(1) = **CYC 2** <= 8 ✅
- `ResolveActiveModeButton`: base(1) + 5 arms = **CYC 6** <= 8 ✅
- `ResetModeChipStyles`: base(1) + foreach(1) + null-guard(1) = **CYC 3** <= 8 ✅
- max_cyc_projected = **6** <= 8 ✅
- projected_parent_cyc_after_all = **2** <= 8 ✅

**Verification: PASS. All methods satisfy Jane Street CYC <= 8 threshold.**

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 5 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-158 |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 ticket breakdown thoughts) |
| **ticket_count** | 2 |
| **Output** | docs/brain/EPIC-W7-158/04-tickets.md |
