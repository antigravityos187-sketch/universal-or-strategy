# BWAVE-REFACTOR LaneA — Tickets

**Epic**: BWAVE-REFACTOR LaneA  
**Phase**: 3 (Ticket Generation)  
**Status**: TICKETS_COMPLETE  
**Date**: 2026-08-25  
**Architect**: ptt-architect  
**Input**: `docs/brain/BWAVE-REFACTOR/LaneA/02-architecture-plan.md` (PLAN_COMPLETE / REVIEW_PASS)

> **Ticket A-1 is DONE on main — SKIP. This file covers Ticket A-2 and Ticket A-3 only.**

---

## Ticket A-2: DW-LaneA-06 — BuildArrowCluster Teal Button Background Regression

### 1. Ticket ID and Title

**ID**: A-2  
**Title**: Fix `BrushInactive` → `BrushTeal` for four teal button specs in `BuildBufferedButtonsRow`

---

### 2. Spec Req IDs

**DW-LaneA-06** — Teal buttons (`_beBtn2`, `_globalBeBtn2`, `_quickBtn`, `_quickAllBtn`) render with grey (`BrushInactive`) background instead of teal (`BrushTeal`) background. Root cause: `Bg` field in specs array never updated from `BrushInactive` to `BrushTeal`.

---

### 3. File(s) Touched

- `src/PropTraderTools/TradeCopierPanel.cs`

No other file is modified.

---

### 4. Method Signature(s)

```csharp
private void BuildBufferedButtonsRow(StackPanel root)
```

No signature change. Internal specs array values only.

---

### 5. Precise Change Description

**Location**: `TradeCopierPanel.cs`, lines 1157–1160 (the four teal button rows of the `specs` array inside `BuildBufferedButtonsRow`).

**What to change**: In each of the four teal button spec entries, replace the `Bg` argument `BrushInactive` with `BrushTeal`.

The engineer must locate the four spec entries by their `Store` lambdas (`b => _beBtn2 = b`, `b => _globalBeBtn2 = b`, `b => _quickBtn = b`, `b => _quickAllBtn = b`) and change ONLY the `Bg` positional argument from `BrushInactive` to `BrushTeal`.

**Before (four teal rows)**:
```csharp
(FormatBuffer("BE",       _beBuffer),              BrushInactive, true, ..., b => _beBtn2       = b, _beRowPanel),
(FormatGlobalBeBuffer(...),                         BrushInactive, true, ..., b => _globalBeBtn2 = b, _beRowPanel),
(FormatBuffer("Quick",    _quickT1),                BrushInactive, true, ..., b => _quickBtn     = b, _quickRowPanel),
(FormatBuffer("Quick ALL", ...),                    BrushInactive, true, ..., b => _quickAllBtn  = b, _quickRowPanel),
```

**After (four teal rows)**:
```csharp
(FormatBuffer("BE",       _beBuffer),              BrushTeal,     true, ..., b => _beBtn2       = b, _beRowPanel),
(FormatGlobalBeBuffer(...),                         BrushTeal,     true, ..., b => _globalBeBtn2 = b, _beRowPanel),
(FormatBuffer("Quick",    _quickT1),                BrushTeal,     true, ..., b => _quickBtn     = b, _quickRowPanel),
(FormatBuffer("Quick ALL", ...),                    BrushTeal,     true, ..., b => _quickAllBtn  = b, _quickRowPanel),
```

**Do NOT touch**:
- `_trimBtn2` row — stays `BrushInactive`
- `_flattenBtn2` row — stays `BrushInactive`
- Any other code in the method
- `BrushTeal` definition at line 326 (already correct, do not modify)

`BrushTeal` is defined as:
```csharp
private static readonly SolidColorBrush BrushTeal = MakeBrush(13, 148, 136); // teal-600
```
It is already `Freeze()`d via `MakeBrush()`. No new brush definition required.

---

### 6. CYC Pre-Check

| | Value |
|---|---|
| **CYC before** | 3 (`base(1)` + `foreach(1)` + `if(s.Teal)(1)`) |
| **CYC after** | 3 (no branches added — value substitution only) |
| **Threshold** | ≤ 8 ✓ |

No new `if`, `for`, `while`, `case`, `&&`, `||` added.

---

### 7. JS Rules

| Rule ID | Category | Constraint Applied |
|---------|----------|--------------------|
| **JS-008** | Performance | `BrushTeal` is `static readonly SolidColorBrush`, `Freeze()`d — zero allocation on re-render. No new heap object created. |
| **JS-021** | Concurrency | No `lock()` added or modified. |
| **JS-066** | Code Review | Diff targets exactly 4 lines changed in one method. Well under 10k char diff limit. |
| **JS-096** | Philosophy | `BrushTeal` is already the legal teal representation — using `BrushInactive` for teal buttons was an illegal state. Fix makes the state representation correct at construction. |

SCAN-04 (hex literals): `MakeBrush(13, 148, 136)` uses integer RGB, not `#0d9488`. No hex literal introduced.

---

### 8. Acceptance Criteria

1. `_beBtn2`, `_globalBeBtn2`, `_quickBtn`, `_quickAllBtn` render with `BrushTeal` background in NT8 UI.
2. `_trimBtn2` and `_flattenBtn2` continue to render with `BrushInactive` background (unchanged).
3. `dotnet build src/PropTraderTools/` produces **0 errors, 0 warnings** introduced by this change.
4. NT8 sync: `powershell -File scripts\ptt-sync-and-verify.ps1` completes with **18/18 OK, 0 MISMATCH**.
5. F5 in NinjaTrader 8 compiles without error.
6. All 7 scans pass (see SCAN checklist below).
7. No other button backgrounds, styles, or foregrounds change.

---

### 9. NT8 Sync Required

**YES** — `TradeCopierPanel.cs` is in `src/PropTraderTools/` and must be synced to the NT8 AddIn directory.

Run after build:
```powershell
powershell -File scripts\ptt-sync-and-verify.ps1
```
Required result: **18/18 OK** — any MISMATCH line = ticket incomplete.

---

### 10. F5 Required

**YES** — Press F5 in NinjaTrader 8 after sync to recompile the AddIn. Confirm green compile before closing ticket.

---

### 11. 7-Scan Checklist

| Scan | Command | Expected |
|------|---------|----------|
| SCAN-01 | `grep -r "lock(" src/PropTraderTools/` | 0 results |
| SCAN-02 | `Get-Content src/PropTraderTools/*.cs \| Where-Object {$_ -match '[^\x00-\x7F]'}` | 0 results |
| SCAN-03 | `Select-String -Path src/PropTraderTools/*.cs -Pattern "FontFamily"` | 0 results |
| SCAN-04 | `Select-String -Path src/PropTraderTools/*.cs -Pattern "#[0-9A-Fa-f]{6}"` | 0 results |
| SCAN-05 | Verify all CreateOrder calls use "PTT-" prefix | 0 violations |
| SCAN-06 | `Select-String -Path src/PropTraderTools/*.cs -Pattern "DateTime\.Now[^U]"` | 0 results |
| SCAN-07 (CYC) | See lizard command below | 0 rows CCN > 8 |

**SCAN-07 full lizard command**:
```powershell
$files = Get-ChildItem src/PropTraderTools/ -Filter "*.cs" -Recurse |
  Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }
lizard $files --csv 2>&1 |
  ConvertFrom-Csv -Header @("NLOC","CCN","Tokens","Params","Length","Location","MethodName","MethodLongName","StartLine","EndLine") |
  Where-Object { [int]$_.CCN -gt 8 } |
  Sort-Object { [int]$_.CCN } -Descending
```
Expected: **0 rows output**. Any row = BUILD_FAIL.

---

---

## Ticket A-3: DW-C39-09 — SaveRules Not Called After OnAddRule

### 1. Ticket ID and Title

**ID**: A-3  
**Title**: Add `CopyEngine.Instance.SaveRules()` as final statement in `OnAddRule` to persist rules immediately on creation

---

### 2. Spec Req IDs

**DW-C39-09** — `OnAddRule` in `TradeCopierWindow.cs` adds a rule row to the UI and gates buttons but does not call `SaveRules()`. If NT8 restarts before `OnClosed` fires, the freshly-added rule is lost. Fix: call `SaveRules()` immediately after `ApplyFeatureFlags`.

---

### 3. File(s) Touched

- `src/PropTraderTools/TradeCopierWindow.cs`

No other file is modified.

---

### 4. Method Signature(s)

```csharp
private void OnAddRule(object sender, RoutedEventArgs e)
```

No signature change. One statement added to the method body.

---

### 5. Precise Change Description

**Location**: `TradeCopierWindow.cs`, method `OnAddRule` (approximately line 902–906).

**What to change**: Add `CopyEngine.Instance.SaveRules();` as the **last** statement in the method body, on the line immediately after `ApplyFeatureFlags(CopyEngine.Instance.Flags);`. Add the inline comment `// DW-C39-09: persist immediately`.

**Before**:
```csharp
private void OnAddRule(object sender, RoutedEventArgs e)
{
    _rulesPanel.Children.Add(BuildDynamicRuleRow());
    ApplyFeatureFlags(CopyEngine.Instance.Flags); // gate newly-added buttons
}
```

**After**:
```csharp
private void OnAddRule(object sender, RoutedEventArgs e)
{
    _rulesPanel.Children.Add(BuildDynamicRuleRow());
    ApplyFeatureFlags(CopyEngine.Instance.Flags); // gate newly-added buttons
    CopyEngine.Instance.SaveRules();              // DW-C39-09: persist immediately
}
```

**Do NOT touch**:
- `OnClosed` at line 190 — existing `SaveRules()` call there is unrelated and stays
- `OnRowApply` — out of scope for this ticket
- Any other method in the file
- `SaveRules` definition in `CopyEngine.cs` (no changes)

`SaveRules` signature for reference (do not modify):
```csharp
// CopyEngine.cs line 6353
public void SaveRules(string overridePath = null)
```
Called with no arguments — matches existing call pattern at `OnClosed` line 190.

**Threading note**: `OnAddRule` is a WPF click event handler, always invoked on the UI thread. `SaveRules()` is already called from `OnClosed` on the UI thread. No `Dispatcher.InvokeAsync` needed.

---

### 6. CYC Pre-Check

| | Value |
|---|---|
| **CYC before** | 1 (straight-line, no branches) |
| **CYC after** | 1 (method call statement adds no branch) |
| **Threshold** | ≤ 8 ✓ |

No new `if`, `for`, `while`, `case`, `&&`, `||` added.

---

### 7. JS Rules

| Rule ID | Category | Constraint Applied |
|---------|----------|--------------------|
| **JS-021** | Concurrency | No `lock()` added. `SaveRules()` is UI-thread safe (WPF click handler context). |
| **JS-001** | Type Safety | No exception thrown. `SaveRules()` is void; failures are silent I/O errors, not thrown exceptions. |
| **JS-002** | Type Safety | No null return added. |
| **JS-033** | Concurrency | Method is `private void` event handler (WPF pattern) — this is the only permitted `void` handler form per JS-033 carve-out for event handlers. |
| **JS-066** | Code Review | Diff is exactly 1 line added in one method. Well under 10k char limit. |

---

### 8. Acceptance Criteria

1. After clicking "Add Rule" in the TradeCopierWindow, then restarting NT8 (F5 recompile or full restart), the added rule row persists and is visible on next open.
2. `dotnet build src/PropTraderTools/` produces **0 errors, 0 warnings** introduced by this change.
3. NT8 sync: `powershell -File scripts\ptt-sync-and-verify.ps1` completes with **18/18 OK, 0 MISMATCH**.
4. F5 in NinjaTrader 8 compiles without error.
5. All 7 scans pass (see SCAN checklist below).
6. `OnClosed` save behavior is unchanged — rules still saved on window close as before.
7. xUnit test `OnAddRule_CallsSaveRules_RulePersistsAcrossRestart` passes (see test spec below).

---

### 9. NT8 Sync Required

**YES** — `TradeCopierWindow.cs` is in `src/PropTraderTools/` and must be synced to the NT8 AddIn directory.

Run after build:
```powershell
powershell -File scripts\ptt-sync-and-verify.ps1
```
Required result: **18/18 OK** — any MISMATCH line = ticket incomplete.

---

### 10. F5 Required

**YES** — Press F5 in NinjaTrader 8 after sync to recompile the AddIn. Confirm green compile before closing ticket.

---

### 11. 7-Scan Checklist

| Scan | Command | Expected |
|------|---------|----------|
| SCAN-01 | `grep -r "lock(" src/PropTraderTools/` | 0 results |
| SCAN-02 | `Get-Content src/PropTraderTools/*.cs \| Where-Object {$_ -match '[^\x00-\x7F]'}` | 0 results |
| SCAN-03 | `Select-String -Path src/PropTraderTools/*.cs -Pattern "FontFamily"` | 0 results |
| SCAN-04 | `Select-String -Path src/PropTraderTools/*.cs -Pattern "#[0-9A-Fa-f]{6}"` | 0 results |
| SCAN-05 | Verify all CreateOrder calls use "PTT-" prefix | 0 violations |
| SCAN-06 | `Select-String -Path src/PropTraderTools/*.cs -Pattern "DateTime\.Now[^U]"` | 0 results |
| SCAN-07 (CYC) | See lizard command below | 0 rows CCN > 8 |

**SCAN-07 full lizard command**:
```powershell
$files = Get-ChildItem src/PropTraderTools/ -Filter "*.cs" -Recurse |
  Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\' }
lizard $files --csv 2>&1 |
  ConvertFrom-Csv -Header @("NLOC","CCN","Tokens","Params","Length","Location","MethodName","MethodLongName","StartLine","EndLine") |
  Where-Object { [int]$_.CCN -gt 8 } |
  Sort-Object { [int]$_.CCN } -Descending
```
Expected: **0 rows output**. Any row = BUILD_FAIL.

---

### xUnit Test Specification

**Test name**: `OnAddRule_CallsSaveRules_RulePersistsAcrossRestart`

**What it asserts**:
- **Given**: A `TradeCopierWindow` or integration test harness with access to the rules persistence file path used by `CopyEngine.Instance.SaveRules()`.
- **When**: `OnAddRule` is invoked (via `internal` + `[InternalsVisibleTo]` reflection, or via a WPF button click in a UI test harness).
- **Then**: The rules file on disk is written/updated within the test. A subsequent instantiation of `CopyEngine` that loads the rules file from disk finds the newly-added rule present.

**Implementation options for engineer** (choose the approach that matches existing test patterns in the codebase):

Option A — File system check (simpler):
```csharp
[Fact]
public void OnAddRule_CallsSaveRules_RulePersistsAcrossRestart()
{
    // Arrange
    var rulesFilePath = CopyEngine.Instance.GetRulesFilePath(); // or known test path
    var mtime_before = File.GetLastWriteTimeUtc(rulesFilePath);

    // Act — invoke OnAddRule (via reflection or internal accessor)
    // var window = new TradeCopierWindow(...);
    // window.InvokeOnAddRule(); // internal method

    // Assert
    var mtime_after = File.GetLastWriteTimeUtc(rulesFilePath);
    Assert.True(mtime_after > mtime_before, "SaveRules must write the rules file after OnAddRule");
}
```

Option B — Rule count check:
```csharp
[Fact]
public void OnAddRule_CallsSaveRules_RulePersistsAcrossRestart()
{
    // Arrange: record rule count before
    // Act: trigger OnAddRule
    // Assert: reload engine from disk, rule count increased by 1
}
```

The engineer selects the option consistent with how existing xUnit tests in `tests/` access internal WPF methods. The test MUST NOT use NUnit or MSTest attributes — `[Fact]` (xUnit) only.

---

## Ticket Summary

| Ticket | Spec Req | File | Method | Change | CYC Before | CYC After | NT8 Sync | F5 |
|--------|----------|------|--------|--------|-----------|-----------|----------|----|
| A-2 | DW-LaneA-06 | `TradeCopierPanel.cs` | `BuildBufferedButtonsRow(StackPanel root)` | 4× `BrushInactive` → `BrushTeal` in specs `Bg` field | 3 | 3 | 18/18 | Yes |
| A-3 | DW-C39-09 | `TradeCopierWindow.cs` | `OnAddRule(object sender, RoutedEventArgs e)` | Add `CopyEngine.Instance.SaveRules();` after `ApplyFeatureFlags` | 1 | 1 | 18/18 | Yes |

---

**TICKETS_COMPLETE**
