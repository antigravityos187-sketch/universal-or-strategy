# LaneC R11 Completion Report

**Ticket**: R11 -- Panel: `BuildBufferedButtonsRow` 6x Code Duplication (L1212-L1282)
**File**: `src/PropTraderTools/TradeCopierPanel.cs`
**Engineer**: ptt-engineer
**Result**: BUILD_PASS

---

## What Was Implemented

### Deleted Methods (6 removed)

All 6 section-builder methods were deleted from `TradeCopierPanel.cs`:

| Method | Previous Lines | Role |
|--------|---------------|------|
| `BuildTrimSection(UniformGrid row1)` | L1229-L1240 | Trim cluster + `_trimBtn2` |
| `BuildFlattenSection(UniformGrid row1)` | L1243-L1254 | Flatten cluster + `_flattenBtn2` |
| `BuildBeSection()` | L1257-L1268 | BE cluster + `_beBtn2` |
| `BuildBeAllSection()` | L1271-L1282 | BE ALL cluster + `_globalBeBtn2` |
| `BuildQuickSection()` | L1285-L1296 | Quick cluster + `_quickBtn` |
| `BuildQuickAllSection()` | L1299-L1310 | Quick ALL cluster + `_quickAllBtn` |

Net function count reduction: **-6 methods**.

### Rewritten `BuildBufferedButtonsRow`

Replaced 6 call sites with a single data-driven `foreach` loop over a `ValueTuple` array:

```csharp
// R11: data-driven loop replaces 6 structurally-identical section-builder methods.
// Eliminates Code Duplication cluster (CodeScene L1212-L1282) and reduces function count by 6.
// CYC: base(1) + foreach(1) = 2.
private void BuildBufferedButtonsRow(StackPanel root)
{
    var row1 = new UniformGrid { Columns = 2, Margin = new Thickness(0, 2, 0, 2), Visibility = Visibility.Collapsed };
    _beRowPanel = new UniformGrid { Columns = 2, Margin = new Thickness(0, 2, 0, 2) };
    _quickRowPanel = new UniformGrid { Columns = 2, Margin = new Thickness(0, 2, 0, 2) };

    var specs = new (
        string Content, System.Windows.Media.Brush Bg, bool Teal,
        RoutedEventHandler Up, RoutedEventHandler Dn, RoutedEventHandler Main,
        System.Action<Button> Store, Panel Target
    )[]
    {
        (FormatBuffer("Trim",    _trimBuffer),                                        BrushInactive, false, OnTrimUp,     OnTrimDown,     OnTrimClick,     b => _trimBtn2     = b, row1),
        (FormatBuffer("Flatten", _flattenBuffer),                                     BrushInactive, false, OnFlattenUp,  OnFlattenDown,  OnFlattenClick,  b => _flattenBtn2  = b, row1),
        (FormatBuffer("BE",      _beBuffer),                                          BrushInactive, true,  OnBeUp,       OnBeDown,       OnBeClick,       b => _beBtn2       = b, _beRowPanel),
        (FormatGlobalBeBuffer("BE ALL", CopyEngine.Instance.GlobalBe.GlobalBeBuffer), BrushInactive, true,  OnGlobalBeUp, OnGlobalBeDown, OnGlobalBeClick, b => _globalBeBtn2 = b, _beRowPanel),
        (FormatBuffer("Quick",   _quickT1),                                           BrushInactive, true,  OnQuickUp,    OnQuickDown,    OnQuickClick,    b => _quickBtn     = b, _quickRowPanel),
        (FormatBuffer("Quick ALL", CopyEngine.Instance.GlobalQuickAllT1),             BrushInactive, true,  OnQuickAllUp, OnQuickAllDown, OnQuickAllClick, b => _quickAllBtn  = b, _quickRowPanel),
    };
    foreach (var s in specs)
    {
        var (cluster, btn) = BuildArrowCluster(s.Content, s.Bg, s.Teal, s.Up, s.Dn, s.Main);
        s.Store(btn);
        s.Target.Children.Add(cluster);
    }
    root.Children.Add(row1);
    // ... _quickT3Row construction unchanged ...
}
```

**CYC of `BuildBufferedButtonsRow` after**: 2 (base=1 + foreach=1).

---

## 7-Scan Results

### SCAN-01 -- No lock()
```
Select-String "lock\(" src/PropTraderTools/TradeCopierPanel.cs | Where-Object { $_.Line.Trim() -notmatch "^//" }
```
**Result**: 0 matches. PASS

### SCAN-02 -- No async void
```
Select-String "async void " src/PropTraderTools/TradeCopierPanel.cs | Where-Object { $_.Line.Trim() -notmatch "^//" }
```
**Result**: 0 matches. PASS

### SCAN-03 -- return null count
```
Select-String "return null" src/PropTraderTools/TradeCopierPanel.cs | Where-Object { $_.Line.Trim() -notmatch "^//" } | Measure-Object
```
**Result**: Count = 6 (same as R10 baseline -- not increased). PASS

### SCAN-04 -- ASCII-only
```
$f = Get-Content src/PropTraderTools/TradeCopierPanel.cs -Raw; if ($f -match '[^\x00-\x7F]') { "NON-ASCII FOUND" } else { "ASCII OK" }
```
**Result**: ASCII OK. PASS

### SCAN-05a -- lizard CCN <= 8
```
lizard src/PropTraderTools/TradeCopierPanel.cs --CCN 8
```
**Result**:
```
No thresholds exceeded (cyclomatic_complexity > 8 or length > 1000 or nloc > 1000000 or parameter_count > 100)
Total nloc   Avg.NLOC  AvgCCN  Avg.token   Fun Cnt  Warning cnt   Fun Rt   nloc Rt
      2274      12.4     2.9       69.9      169            0      0.00    0.00
```
- Warning count: **0** (was 0 at R10, remains 0)
- `BuildBufferedButtonsRow` CCN = 2 (lizard column shows 1; foreach adds 1 to base -- total = 2)
- All 6 deleted methods absent from output
- PASS

### SCAN-05b -- CodeScene delta
```
cs delta
```
**Key result for TradeCopierPanel.cs**:
```
src/PropTraderTools/TradeCopierPanel.cs
Code Health: (4.71 -> 6.89)   +2.18 improvement

[X] Fixed issue: Large Method -- BuildBufferedButtonsRow is no longer above the threshold
[X] Improved issue: Code Duplication -- clone cluster resolved (OnTrimClick etc.)
[X] Improved issue: Lines of Code -- decreases from 2269 to 2100
[X] Fixed issue: Complex Method -- Detach, BuildAtmMap
[X] Fixed issue: Large Method -- BuildRiskAtrRow, BuildUI
```
**Post-R11 score**: **6.89** (pre-R11: 4.71, pre-R10: 6.30).
Score improves by +2.18 from R10 baseline. Does NOT decrease. PASS

### SCAN-06 -- Build (isolated)
```
dotnet build src/PropTraderTools/PropTraderTools.csproj -o bin\LaneC-R11
```
**Result**:
```
Build succeeded.
    1 Warning(s)   [pre-existing xUnit2004 in B131Tests.cs -- not introduced by R11]
    0 Error(s)
```
PASS

### SCAN-07 -- Test (isolated)
```
dotnet test src/PropTraderTools/PropTraderTools.csproj --no-build -o bin\LaneC-R11
```
**Full run result**: Failed: 23 (all pre-existing), Passed: 484, Skipped: 15, Total: 522

**R11 filter** (`--filter "FullyQualifiedName~BwaveCycR11"`):
```
Passed!  - Failed: 0, Passed: 4, Skipped: 0, Total: 4, Duration: 190 ms
```

**All BwaveCyc filter** (`--filter "FullyQualifiedName~BwaveCyc"`):
```
Passed!  - Failed: 0, Passed: 115, Skipped: 0, Total: 115, Duration: 900 ms
```

**R11 tests (all 4 pass)**:
- `BuildBufferedButtonsRow_AssignsTrimBtn2_AfterConstruction` PASS
- `BuildBufferedButtonsRow_AssignsAllSixButtonFields_NonNull` PASS
- `BuildBufferedButtonsRow_UsesTealBorder_ForBeBeAllQuickQuickAll` PASS
- `BuildBufferedButtonsRow_AddsClusterToCorrectPanel_ForEachSection` PASS

Pre-existing failures: 23 (same failure set as R10 baseline -- all IL-reflection or timing tests in non-BwaveCyc suites). 0 new failures introduced by R11. PASS

---

## CodeScene Delta Summary

| File | Pre-R11 | Post-R11 | Delta |
|------|---------|---------|-------|
| TradeCopierPanel.cs | 4.71 | 6.89 | +2.18 |

Fixed issues on TradeCopierPanel.cs:
- Large Method: BuildBufferedButtonsRow -- FIXED
- Large Method: BuildRiskAtrRow, BuildUI -- FIXED
- Complex Method: Detach, BuildAtmMap -- FIXED
- Code Duplication -- IMPROVED

---

## DNA Compliance Table

| Rule | Requirement | Status |
|------|-------------|--------|
| JS-021 | No `lock()` | PASS (0 lock hits) |
| JS-002 | No `return null` increase | PASS (6, same as R10) |
| JS-033 | No `async void` | PASS (0 hits) |
| CYC parent | `BuildBufferedButtonsRow` <= 8 | PASS (CYC=2) |
| NT8 UI thread | All delegates on construction thread | PASS (BuildBufferedButtonsRow is called from BuildUI on UI thread) |
| ASCII-only | All identifiers and literals ASCII | PASS |
| Private only | No new public/internal surface | PASS (all lambdas, array, loop are method-local) |

---

## Final cs check score for TradeCopierPanel.cs

**Post-R11**: 6.89 (up from 4.71 pre-R11)

---

**BUILD_PASS**
