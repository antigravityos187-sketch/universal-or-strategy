# BWAVE-REFACTOR LaneA — Architecture Plan

**Epic**: BWAVE-REFACTOR LaneA  
**Phase**: 1 (Architecture)  
**Status**: PLAN_COMPLETE  
**Date**: 2026-08-25  
**Architect**: ptt-architect  

---

## RULES CATALOG GATE

**Gate result: PASS**

P0 rules checked against all files in scope:

| Rule | Pattern | Files Checked | Result |
|------|---------|---------------|--------|
| JS-021 | `lock\s*\(` | TradeCopierPanel.cs, TradeCopierWindow.cs | 0 hits |
| JS-001 | `throw\s+new\s+\w+Exception\(` | TradeCopierPanel.cs, TradeCopierWindow.cs | 0 hits |
| JS-002 | `return\s+null\s*;` | TradeCopierPanel.cs, TradeCopierWindow.cs | 0 hits |
| JS-010 | Public constructor without factory | No new constructors added | N/A |
| JS-033 | `async\s+void\s+\w+\(` | No async void added | N/A |
| JS-036 | `byte\[\]\s*=\s*new\s+byte\[` | Not applicable | N/A |
| JS-037 | `new\s+byte\[\d+\]` without ArrayPool | Not applicable | N/A |

No P0 violations in scope. Gate: **PASS** — work may proceed.

---

## LANE-SPLIT GATE

**Q1. Are all 3 tickets within the same method or within 50 lines of each other?**
- A-1: `.bob/custom_modes.yaml` (YAML config, ptt-verifier roleDefinition ~line 8790)
- A-2: `src/PropTraderTools/TradeCopierPanel.cs` (`BuildBufferedButtonsRow`, ~line 1155)
- A-3: `src/PropTraderTools/TradeCopierWindow.cs` (`OnAddRule`, ~line 902)

Three different files across different types (YAML vs C# vs C#). Not within the same method or within 50 lines.
**Q1 = NO**

**Q2. Does any ticket's design depend on another ticket's final design?**
- A-1 is a YAML-only edit with no C# dependencies.
- A-2 changes a brush field in TradeCopierPanel.cs, independent of Window.cs.
- A-3 adds a SaveRules call in TradeCopierWindow.cs, independent of Panel.cs.
No cross-ticket design dependency exists.
**Q2 = NO**

**Q3. Does each ticket have standalone value if the others are blocked?**
- A-1: Yes — lizard-based CYC measurement improves verification quality independently.
- A-2: Yes — teal button background regression is a user-visible visual fix.
- A-3: Yes — SaveRules-after-AddRule prevents data loss independently.
**Q3 = YES**

**Q4. Does each ticket have an independent SIM/verification path?**
- A-1: YAML-only — no NT8 sync, no F5. Verify by reading updated YAML for lizard command.
- A-2: TradeCopierPanel.cs — dotnet build 0 errors, NT8 sync 18/18 OK, visual check.
- A-3: TradeCopierWindow.cs — dotnet build 0 errors, NT8 sync 18/18 OK, persistence test.
**Q4 = YES**

**Gate derivation: Q1=NO AND Q2=NO AND Q3=YES AND Q4=YES**

## LANE-SPLIT GATE RESULT: LANES-APPROVED

---

## Key NT8 Facts (embedded per protocol — not used in this epic but mandatory)

- `AtmStrategyChangeStopTarget()` — StrategyBase-only. NOT AddOnBase. Cancel+resubmit is the AddOn pattern.
- `AtmStrategyCreate()` — StrategyBase-only. NOT AddOnBase.
- `Account.Change()` — AddOnBase available but silent no-op on ATM-owned brackets.
- `Account.Cancel()` + `Account.CreateOrder()` + `Submit()` — AddOnBase available. Correct bracket-change pattern.

None of these APIs are involved in any LaneA ticket.

---

## Ticket A-1: Fix CYC Measurement in ptt-verifier, ptt-engineer, ptt-architect Modes

### Problem Statement

The current ptt-verifier `WHAT YOU HUNT` section (`.bob/custom_modes.yaml` line ~8791) describes CYC enforcement as:

```
COMPLEXITY (P1):
  Any method with CYC > 8 (count decision points: if/else/for/while/case/&&/||)
```

This relies on the engineer manually counting branch points and self-reporting the CYC score. Manual counts are error-prone and untestable. The ptt-engineer MANDATORY 7 SCANS section (line ~8656) has no lizard-based CYC command.

### Root Cause

The CYC check was designed as a narrative instruction, not a mechanically verifiable scan. The engineer "counts" branches mentally or by reading code. This produces inconsistent results across sessions.

### Fix Approach

Add a mandatory `lizard` shell command to the ptt-verifier COMPLEXITY section and to the ptt-engineer MANDATORY 7 SCANS section. Also update the ptt-architect COMPLEXITY rule description to reference lizard as the authoritative CYC tool.

The lizard command (provided in ticket scope):

```powershell
$files = Get-ChildItem src/PropTraderTools/ -Filter "*.cs" -Recurse |
  Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }
lizard $files --csv 2>&1 |
  ConvertFrom-Csv -Header @("NLOC","CCN","Tokens","Params","Length","Location","MethodName","MethodLongName","StartLine","EndLine") |
  Where-Object { [int]$_.CCN -gt 8 } |
  Sort-Object { [int]$_.CCN } -Descending
```

Acceptance: Output must be empty (zero rows). Any row = violation.

### Method Signatures / Changes

**File**: `.bob/custom_modes.yaml`

No C# methods involved. YAML roleDefinition text edits only.

**Changes to ptt-verifier roleDefinition** (after the existing `COMPLEXITY (P1):` section):
- Replace "count decision points: if/else/for/while/case/&&/||" with the mandatory lizard command block shown above.
- Add note: "Output MUST be empty — any row with CCN > 8 = VERIFY_FAIL. Do NOT rely on manual branch counting."

**Changes to ptt-engineer roleDefinition** (in MANDATORY 7 SCANS section):
- After SCAN-07, add:
  ```
  CYC-SCAN: lizard src/PropTraderTools/ --csv ... (filter CCN > 8) -> 0 rows required
  ```
- Update the COMPLEXITY bullet to reference lizard as the verification tool.

**Changes to ptt-architect roleDefinition** (in COMPLEXITY section):
- Add reference: "CYC verified via lizard tool — not manual counting. Use lizard command from ptt-verifier."

### CYC Analysis

No C# methods modified. CYC constraint not applicable to YAML edits.

### Acceptance Criteria

1. `.bob/custom_modes.yaml` ptt-verifier roleDefinition contains the full lizard command block.
2. `.bob/custom_modes.yaml` ptt-engineer roleDefinition contains the lizard CYC-SCAN reference.
3. `.bob/custom_modes.yaml` ptt-architect roleDefinition references lizard for CYC verification.
4. No other YAML section modified.

### Files Touched

- `.bob/custom_modes.yaml` — roleDefinition sections for ptt-verifier, ptt-engineer, ptt-architect

### NT8 Sync Required

NO — `.bob/custom_modes.yaml` is not in `src/PropTraderTools/`. No F5 required.

### SCAN-07 Checklist

Scans apply to `.cs` files only. No `.cs` files modified in A-1.

| Scan | Applies | Result |
|------|---------|--------|
| SCAN-01 | No (YAML) | N/A |
| SCAN-02 | No (YAML) | N/A |
| SCAN-03 | No (YAML) | N/A |
| SCAN-04 | No (YAML) | N/A |
| SCAN-05 | No (YAML) | N/A |
| SCAN-06 | No (YAML) | N/A |
| SCAN-07 | No (YAML) | N/A |

Verification: Read updated YAML and confirm lizard command is present in all three modes.

---

## Ticket A-2: DW-LaneA-06 — BuildArrowCluster Teal Button Background Regression

### Problem Statement

In `TradeCopierPanel.cs`, `BuildBufferedButtonsRow` constructs six arrow-cluster buttons using a `specs` array (lines 1144–1160). The `Bg` field in the specs tuple is `BrushInactive` for **all** buttons including the four teal-type ones:

```csharp
// Line 1157-1160 — teal buttons, but Bg = BrushInactive:
(FormatBuffer("BE",      _beBuffer), BrushInactive, true, ..., b => _beBtn2      = b, _beRowPanel),
(FormatGlobalBeBuffer(...),           BrushInactive, true, ..., b => _globalBeBtn2= b, _beRowPanel),
(FormatBuffer("Quick",   _quickT1),   BrushInactive, true, ..., b => _quickBtn    = b, _quickRowPanel),
(FormatBuffer("Quick ALL", ...),      BrushInactive, true, ..., b => _quickAllBtn = b, _quickRowPanel),
```

At line 1197: `btn.Background = s.Bg;` — this applies `BrushInactive` (grey) to all buttons. The teal buttons (`_beBtn2`, `_globalBeBtn2`, `_quickBtn`, `_quickAllBtn`) receive only `BrushTeal` for their border and foreground (lines 1192-1194), but their background remains grey. The comment at line 1197 says `// AFTER style -- explicit brush wins (DW-LaneA-06 fix)` — this was a partial fix that corrected the ordering (background set after style) but left the background color wrong for teal buttons.

### Root Cause

The `Bg` field for teal button specs was never updated from `BrushInactive` to `BrushTeal`. The partial fix (DW-LaneA-06 first pass) ensured `btn.Background = s.Bg` runs after `SetResourceReference(StyleProperty, ...)` so the explicit brush wins over the style default, but `s.Bg` was still `BrushInactive`.

### Fix Approach

Change the `Bg` field for the four teal-type button specs from `BrushInactive` to `BrushTeal`. `BrushTeal` is already defined as a `static readonly SolidColorBrush` at line 326:

```csharp
private static readonly SolidColorBrush BrushTeal = MakeBrush(13, 148, 136); // teal-600
```

The `MakeBrush` helper calls `brush.Freeze()` ensuring the brush is immutable and thread-safe (JS-008 compliant). No new brush definition required.

**Exact change in specs array** (lines 1157-1160):
```csharp
// BEFORE:
(FormatBuffer("BE",      _beBuffer),                                          BrushInactive, true, ...),
(FormatGlobalBeBuffer("BE ALL", ...), BrushInactive, true, ...),
(FormatBuffer("Quick",   _quickT1),   BrushInactive, true, ...),
(FormatBuffer("Quick ALL", ...),      BrushInactive, true, ...),

// AFTER:
(FormatBuffer("BE",      _beBuffer),                                          BrushTeal, true, ...),
(FormatGlobalBeBuffer("BE ALL", ...), BrushTeal, true, ...),
(FormatBuffer("Quick",   _quickT1),   BrushTeal, true, ...),
(FormatBuffer("Quick ALL", ...),      BrushTeal, true, ...),
```

No changes to the non-teal rows (Trim, Flatten) which keep `BrushInactive`.

### Method Signatures

**Method modified**: `BuildBufferedButtonsRow(StackPanel root)` — private void, no signature change.

The `specs` array type remains:
```csharp
var specs = new (
    string Content,
    System.Windows.Media.Brush Bg,
    bool Teal,
    RoutedEventHandler Up,
    RoutedEventHandler Dn,
    RoutedEventHandler Main,
    System.Action<Button> Store,
    Panel Target
)[] { ... };
```

The `Bg` field type is `System.Windows.Media.Brush` — `BrushTeal` (a `SolidColorBrush`, which extends `Brush`) is a valid assignment. No type change required.

### CYC Analysis

`BuildBufferedButtonsRow` CYC before and after:
- `base(1)` + `foreach(1)` + `if(s.Teal)(1)` = **3**
- Fix only changes the `Bg` value in specs array entries, adds no branches.
- CYC after fix: **3** (unchanged). ✓ ≤ 8.

### Acceptance Criteria

1. `_beBtn2`, `_globalBeBtn2`, `_quickBtn`, `_quickAllBtn` render with `BrushTeal` background.
2. `_trimBtn2`, `_flattenBtn2` continue to render with `BrushInactive` background.
3. `dotnet build` produces 0 errors.
4. NT8 sync 18/18 OK.
5. All 7 scans clean (see checklist below).

### Files Touched

- `src/PropTraderTools/TradeCopierPanel.cs` — lines 1157-1160 only (Bg field in 4 teal specs)

### Jane Street DNA

- JS-008: `BrushTeal` is `static readonly SolidColorBrush`, `Freeze()`d via `MakeBrush()`. Zero allocation on re-render. ✓
- JS-021: No lock() added or modified. ✓
- No hex color literal (uses `MakeBrush(13, 148, 136)` not `#0d9488`). SCAN-04 clean. ✓
- No FontFamily. SCAN-03 clean. ✓
- No DateTime.Now. SCAN-06 clean. ✓

### SCAN-07 Checklist

| Scan | Command | Expected |
|------|---------|----------|
| SCAN-01 | `grep -r "lock(" src/PropTraderTools/` | 0 results |
| SCAN-02 | `Get-Content src/PropTraderTools/*.cs \| Where-Object {$_ -match '[^\x00-\x7F]'}` | 0 results |
| SCAN-03 | `Select-String -Path src/PropTraderTools/*.cs -Pattern "FontFamily"` | 0 results |
| SCAN-04 | `Select-String -Path src/PropTraderTools/*.cs -Pattern "#[0-9A-Fa-f]{6}"` | 0 results |
| SCAN-05 | Verify all CreateOrder calls use "PTT-" prefix | 0 violations |
| SCAN-06 | `Select-String -Path src/PropTraderTools/*.cs -Pattern "DateTime\.Now[^U]"` | 0 results |
| SCAN-07 | `Select-String -Path src/PropTraderTools/*.cs -Pattern "\block\s*\("` | 0 results |

### xUnit Tests

No new business logic introduced — this is a pure UI brush assignment change. No new [Fact] tests required. Visual regression confirmed by NT8 F5 + UI inspection per acceptance criterion 1.

---

## Ticket A-3: DW-C39-09 — SaveRules Not Called After OnAddRule

### Problem Statement

In `TradeCopierWindow.cs`, `OnAddRule` (lines 902-906) adds a dynamic rule row and gates buttons, but does NOT call `SaveRules()`:

```csharp
// Current (lines 902-906):
private void OnAddRule(object sender, RoutedEventArgs e)
{
    _rulesPanel.Children.Add(BuildDynamicRuleRow());
    ApplyFeatureFlags(CopyEngine.Instance.Flags); // gate newly-added buttons
}
```

When the user clicks "Add Rule", the row appears in the UI. However, because `SaveRules()` is not called, the rule is not persisted to disk immediately. If NT8 restarts before `OnClosed` fires (where `SaveRules` at line 190 is the only other call site), the rule is lost.

### Root Cause

`OnAddRule` was implemented to only handle UI state (add row + apply flags). The persistence step was omitted. Every other state-mutation path that changes engine rules calls `SaveRules` through `OnClosed` (on window close). But for freshly-added rules, the window may not be closed before an NT8 restart, crash, or F5 recompile.

Note: `OnRowApply` (line 1093-1110) calls `_engine.AddRule(...)` which registers the rule in `CopyEngine`, but also does NOT call `SaveRules()`. The ticket scope is specifically `OnAddRule`. The `OnRowApply` path is not in scope for this ticket (it is a separate DW item if needed).

### Fix Approach

Add `CopyEngine.Instance.SaveRules();` as the last statement in `OnAddRule`, after `ApplyFeatureFlags`:

```csharp
// After fix:
private void OnAddRule(object sender, RoutedEventArgs e)
{
    _rulesPanel.Children.Add(BuildDynamicRuleRow());
    ApplyFeatureFlags(CopyEngine.Instance.Flags); // gate newly-added buttons
    CopyEngine.Instance.SaveRules();              // DW-C39-09: persist immediately
}
```

**Threading safety**: `OnAddRule` is a WPF click event handler, always invoked on the UI thread. `SaveRules()` is already called from `OnClosed` (line 190) which is also a UI-thread WPF lifecycle method. The call is safe on the UI thread.

**SaveRules signature** (CopyEngine.cs line 6353):
```csharp
public void SaveRules(string overridePath = null)
```

Called with no arguments (uses default path). Matches the existing call pattern at line 190.

### Method Signatures

**Method modified**: `OnAddRule(object sender, RoutedEventArgs e)` — private void, event handler, no signature change.

### CYC Analysis

`OnAddRule` CYC before and after:
- Before: **1** (straight-line, no branches)
- After: **1** (adding a method call statement adds no branch)
- CYC after fix: **1** (unchanged). ✓ ≤ 8.

### Acceptance Criteria

1. After clicking "Add Rule", then restarting NT8, the added rule row persists.
2. `dotnet build` produces 0 errors.
3. NT8 sync 18/18 OK.
4. All 7 scans clean (see checklist below).

### Files Touched

- `src/PropTraderTools/TradeCopierWindow.cs` — `OnAddRule` method body only (add one line)

### Jane Street DNA

- JS-021: No lock() added. ✓
- JS-001: No throw added. ✓
- JS-002: No null return. ✓
- No hex colors, no FontFamily, no DateTime.Now. ✓
- `SaveRules()` is a pure file I/O call — no concurrency concern on UI thread. ✓

### SCAN-07 Checklist

| Scan | Command | Expected |
|------|---------|----------|
| SCAN-01 | `grep -r "lock(" src/PropTraderTools/` | 0 results |
| SCAN-02 | `Get-Content src/PropTraderTools/*.cs \| Where-Object {$_ -match '[^\x00-\x7F]'}` | 0 results |
| SCAN-03 | `Select-String -Path src/PropTraderTools/*.cs -Pattern "FontFamily"` | 0 results |
| SCAN-04 | `Select-String -Path src/PropTraderTools/*.cs -Pattern "#[0-9A-Fa-f]{6}"` | 0 results |
| SCAN-05 | Verify all CreateOrder calls use "PTT-" prefix | 0 violations |
| SCAN-06 | `Select-String -Path src/PropTraderTools/*.cs -Pattern "DateTime\.Now[^U]"` | 0 results |
| SCAN-07 | `Select-String -Path src/PropTraderTools/*.cs -Pattern "\block\s*\("` | 0 results |

### xUnit Tests

**[Fact] Test: `OnAddRule_CallsSaveRules_RulePersistsAcrossRestart`**

What it asserts:
- Given: A `TradeCopierWindow` instance with a `_rulesPanel` (mocked or real WPF panel)
- When: `OnAddRule` is invoked (via reflection or WPF button click simulation)
- Then: `CopyEngine.Instance.SaveRules()` is called (verifiable via file system — rule data file is written/updated)

Implementation note: Since `OnAddRule` is a private event handler, direct xUnit testing requires either:
a) Making the method `internal` and using `[InternalsVisibleTo]`, or
b) Verifying indirectly by checking that the rules persist file is written.

The simpler approach: trigger the button click in a UI test harness and verify the rules file is updated. This is an integration test concept — the specific test infrastructure is left to the engineer per the existing test patterns in the codebase.

---

## Component Summary

| Ticket | File | Method | Change Type | CYC Before | CYC After |
|--------|------|--------|-------------|-----------|-----------|
| A-1 | `.bob/custom_modes.yaml` | N/A (YAML) | Text update (lizard cmd) | N/A | N/A |
| A-2 | `TradeCopierPanel.cs` | `BuildBufferedButtonsRow` | Brush value in specs array | 3 | 3 |
| A-3 | `TradeCopierWindow.cs` | `OnAddRule` | Add `SaveRules()` call | 1 | 1 |

## Data Flow Summary

```
A-1: YAML edit -> ptt-verifier/engineer/architect instruction update
     -> CYC now mechanically verified via lizard (not manual counting)

A-2: BuildBufferedButtonsRow
     specs array [Bg=BrushTeal for teal rows]
     -> foreach loop
     -> btn.Background = s.Bg (BrushTeal for _beBtn2, _globalBeBtn2, _quickBtn, _quickAllBtn)
     -> teal background rendered correctly

A-3: User clicks "Add Rule"
     -> OnAddRule fires
     -> _rulesPanel.Children.Add(BuildDynamicRuleRow())
     -> ApplyFeatureFlags(CopyEngine.Instance.Flags)
     -> CopyEngine.Instance.SaveRules()   [NEW - DW-C39-09 fix]
     -> rules file written to disk
     -> rule survives NT8 restart
```

## Threading Model

| Ticket | Thread Context | Dispatcher.InvokeAsync Needed? |
|--------|---------------|-------------------------------|
| A-1 | N/A (YAML) | No |
| A-2 | UI thread (construction) | No — already on UI thread |
| A-3 | UI thread (click handler) | No — already on UI thread; SaveRules safe on UI thread |

## NT8 Sync Requirements

| Ticket | Sync Required | F5 Required |
|--------|--------------|------------|
| A-1 | No | No |
| A-2 | Yes — 18/18 | Yes |
| A-3 | Yes — 18/18 | Yes |

---

**Plan Status: PLAN_COMPLETE**
