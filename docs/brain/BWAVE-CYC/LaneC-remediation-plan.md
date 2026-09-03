# Lane C CodeScene Remediation Plan

**Produced by**: ptt-architect (Remediation Stage)
**Date**: 2025-01-30
**Wave**: BWAVE-CYC -- Lane C CodeScene Score Remediation
**Status**: READY FOR ptt-engineer

---

## Current State: Panel 4.71, Window 6.61
## Target: Panel >= 7.0, Window >= 8.0

---

## Constraints (re-stated from LaneC-02-architect-plan.md)

| Rule | Requirement |
|------|------------|
| JS-021 | No `lock()` -- zero new lock blocks |
| JS-002 | No new `return null` in helpers |
| JS-033 | No `async void` |
| CYC parent | <= 8 after extraction |
| CYC helper | <= 4 per extracted helper |
| NT8 UI thread | Dispatcher.InvokeAsync stays in original methods |
| ASCII-only | All identifiers and string literals ASCII |
| Private only | Zero new public or internal surface |
| No .cs edits | ptt-engineer only -- architect MUST NOT write .cs files |

---

## OUT OF SCOPE (cannot be fixed without file split)

- File-level: Low Cohesion, Number of Functions in a Single Module, Primitive Obsession
- L469: Excess Number of Function Arguments (public signature -- BANNED to change)
- L502/L515/L685: Complex Conditionals in methods with existing acceptable CCN -- standalone, no extraction target

---

## Priority Order

1. **Large Methods** (highest CodeScene weight): R1, R2, R3, R4, R5
2. **Window BuildRuleRow / BuildDynamicRuleRow duplication** (massive 200+ LoC): R6
3. **Bumpy Road via nested-loop extraction**: R7
4. **Code Duplication -- dispatch handlers**: R8
5. **Code Duplication -- spinner handler pairs**: R9
6. **Code Duplication -- GetAsk/GetBid**: R10 (SKIP -- see note)

---

## Ticket R1 -- Window: `BuildRuleRow` + `BuildDynamicRuleRow` (Large Method + Duplication)

**File**: `src/PropTraderTools/TradeCopierWindow.cs`
**Warning**: Large Method (L466 LoC=202), Large Method (L708 LoC=210), Code Duplication (both methods)
**Method body lines**: L466-L706 (`BuildRuleRow`), L708-L952 (`BuildDynamicRuleRow`)
**Current CCN**: 1 (both -- straight-line construction, no branches)

### Extraction design

Both `BuildRuleRow` and `BuildDynamicRuleRow` construct an identical 12-column Grid with the same widgets.
The ONLY differences are:
- Col 0: `BuildRuleRow` uses a `TextBlock` (instrLabel with fixed text); `BuildDynamicRuleRow` uses a `TextBox` (instrTextBox, editable)
- Col 1/2 `ItemsSource`: `BuildRuleRow` defers binding (set in Loaded); `BuildDynamicRuleRow` sets `ItemsSource = Account.All` immediately
- The `applyBtn.Tag` pattern: `BuildRuleRow` uses `new object[] { instrumentName, leaderCb, ... }`; `BuildDynamicRuleRow` uses `new object[] { instrTextBox, leaderCb, ... }`

**Strategy**: Extract 6 column-building helpers. Each helper returns `UIElement`. Both `BuildRuleRow` and `BuildDynamicRuleRow` call the same helpers, passing the column-0 source object (`string instrumentName` vs `TextBox instrTextBox`) as an `object tag0` parameter.

| Helper | Signature | Body | CCN |
|--------|-----------|------|-----|
| `BuildGridColumnDefinitions` | `private static void BuildGridColumnDefinitions(Grid grid)` | Adds all 12 `ColumnDefinition` objects to grid. First 3 cols use Star in dynamic variant -- passed as bool param. `private static void BuildGridColumnDefinitions(Grid grid, bool dynamicFirstCol)` | 2 |
| `BuildBeCluster` | `private StackPanel BuildBeCluster(object tag0)` | Creates [BE] button + TextBox + "tks" label; tags with `new object[] { tag0, beBox }`; wires `OnRuleBreakEven`; adds to `_beBtns`. Returns cluster. | 1 |
| `BuildTightenCluster` | `private StackPanel BuildTightenCluster(object tag0)` | Creates [~] button + TextBox + "tks" label; tags with `new object[] { tag0, tightenTicksBox }`; wires `OnRuleTightenStop`; adds to `_tightenBtns`. Returns cluster. | 1 |
| `BuildArmBeCluster` | `private StackPanel BuildArmBeCluster(object tag0, ComboBox leaderCb)` | Creates [Arm BE] button + TextBox + "tks" label; tags with `new object[] { tag0, leaderCb, armBeBox }`; wires `OnRuleArmBe`; adds to `_armBeBtns`. Returns cluster. | 1 |
| `BuildAtmColumnPanel` | `private StackPanel BuildAtmColumnPanel()` | Creates ATM ComboBox (Inherit/Market/Named) + namedBox TextBox + SelectionChanged lambda for visibility toggle. Returns StackPanel. | 2 |
| `BuildActionButtons` | `private (Button applyBtn, Button trimBtn, Button flattenBtn, Button cancelBtn, Button toggleBtn) BuildActionButtons(object tag0, ComboBox leaderCb, ListBox followerLb, ComboBox atmCb, TextBox namedBox)` | Creates Apply + [1/2] + [=] + [x] + [ON] buttons. Tags, wires events. Adds trim/flatten/cancel/toggle to respective lists. Returns tuple. | 1 |

**Parent `BuildRuleRow` after extraction** (CCN = **3**, LoC ~35):
```csharp
var grid = new Grid { Margin = new Thickness(2) };
BuildGridColumnDefinitions(grid, false);
// Col 0: fixed label
var instrLabel = new TextBlock { Text = instrumentName, ... };
Grid.SetColumn(instrLabel, 0); grid.Children.Add(instrLabel);
// Col 1: leader ComboBox (deferred binding)
var leaderCb = new ComboBox { ... }; _leaderBoxes.Add(leaderCb);
Grid.SetColumn(leaderCb, 1); grid.Children.Add(leaderCb);
// Col 2: follower ListBox (deferred binding)
var followerLb = new ListBox { ... }; _followerBoxes.Add(followerLb);
Grid.SetColumn(followerLb, 2); grid.Children.Add(followerLb);
var atmPanel = BuildAtmColumnPanel();
var atmCb = atmPanel.Children[0] as ComboBox;
var namedBox = atmPanel.Children[1] as TextBox;
var (applyBtn, trimBtn, flattenBtn, cancelBtn, toggleBtn) = BuildActionButtons(instrumentName, leaderCb, followerLb, atmCb, namedBox);
Grid.SetColumn(trimBtn, 3);    grid.Children.Add(trimBtn);
Grid.SetColumn(flattenBtn, 4); grid.Children.Add(flattenBtn);
Grid.SetColumn(cancelBtn, 5);  grid.Children.Add(cancelBtn);
Grid.SetColumn(toggleBtn, 6);  grid.Children.Add(toggleBtn);
Grid.SetColumn(applyBtn, 7);   grid.Children.Add(applyBtn);
Grid.SetColumn(BuildBeCluster(instrumentName), 8); grid.Children.Add(...);
Grid.SetColumn(atmPanel, 9);   grid.Children.Add(atmPanel);
Grid.SetColumn(BuildTightenCluster(instrumentName), 10); grid.Children.Add(...);
Grid.SetColumn(BuildArmBeCluster(instrumentName, leaderCb), 11); grid.Children.Add(...);
return grid;
```

**Parent `BuildDynamicRuleRow` after extraction** (CCN = **3**, LoC ~25):
Identical but `Col 0` is a `TextBox`, `leaderCb.ItemsSource = Account.All`, `followerLb.ItemsSource = Account.All`.
All cluster helpers pass `instrTextBox` as `tag0`.

**Target CCN after**: 3 (both parents -- the only branches are the lambda closures in `BuildAtmColumnPanel`)

**Estimated CodeScene signal removed**:
- Large Method x2 (202 LoC, 210 LoC -- BOTH removed)
- Code Duplication cluster (L466/L708 pair -- BOTH removed)

**NT8 Thread Contract**: SAFE -- pure WPF widget construction, called from `BuildUI` on UI thread. No Dispatcher, no NT8 Account/Order API in helpers.

**[Fact] test names**:
- `[Fact] BuildBeCluster_WiresOnRuleBreakEven_AndAddsToList`
- `[Fact] BuildTightenCluster_WiresOnRuleTightenStop_AndAddsToList`
- `[Fact] BuildArmBeCluster_TagsWithInstrAndLeaderAndBox`
- `[Fact] BuildAtmColumnPanel_TogglesNamedBoxVisibility_OnSelectionChange`
- `[Fact] BuildGridColumnDefinitions_Adds12Columns`

---

## Ticket R2 -- Panel: `BuildBufferedButtonsRow` (Large Method 248 LoC)

**File**: `src/PropTraderTools/TradeCopierPanel.cs`
**Warning**: Large Method (L1114 LoC=248)
**Method body lines**: L1114-L1383
**Current CCN**: 1 (straight-line construction, no branches)

### Extraction design

`BuildBufferedButtonsRow` constructs 5 button clusters (Trim, Flatten, BE, BE-ALL, Quick, Quick-ALL) each following an identical pattern:
- Create a `DockPanel` (cluster root)
- Create a `Grid` (arrows panel)
- Add 2 `RowDefinition`s
- Create Up `RepeatButton` + Down `RepeatButton`
- `SetResourceReference` on both
- Wire Up/Down Click handlers
- `Grid.SetRow` for each
- `grid.Children.Add(up/dn)`
- `DockPanel.SetDock(arrows, Dock.Right)`
- Create the main `Button`
- `SetResourceReference` on main button
- Wire main button Click handler
- `cluster.Children.Add(arrows)` + `cluster.Children.Add(mainBtn)`

| Helper | Signature | Body | CCN |
|--------|-----------|------|-----|
| `BuildArrowCluster` | `private static (DockPanel cluster, Button mainBtn) BuildArrowCluster(string mainContent, System.Windows.Media.Brush mainBg, System.Windows.Media.Brush mainBorder, System.Windows.Media.Brush mainForeground, RoutedEventHandler upClick, RoutedEventHandler downClick, RoutedEventHandler mainClick)` | Creates DockPanel + Grid + 2 RepeatButtons + main Button. Wires all handlers. Sets NT8 resource styles. Returns (cluster, mainBtn). All params ASCII strings or brush refs (no hex). | 1 |
| `BuildTrimSection` | `private void BuildTrimSection(StackPanel root)` | Calls `BuildArrowCluster(...)`, assigns `_trimBtn2 = mainBtn`, adds cluster to UniformGrid row1. | 1 |
| `BuildFlattenSection` | `private void BuildFlattenSection(StackPanel root)` | Calls `BuildArrowCluster(...)`, assigns `_flattenBtn2 = mainBtn`. | 1 |
| `BuildBeSection` | `private void BuildBeSection()` | Calls `BuildArrowCluster(...)`, assigns `_beBtn2 = mainBtn`, adds to `_beRowPanel`. | 1 |
| `BuildBeAllSection` | `private void BuildBeAllSection()` | Calls `BuildArrowCluster(...)`, assigns `_globalBeBtn2 = mainBtn`. Sets `Content = FormatGlobalBeBuffer(...)`. | 1 |
| `BuildQuickSection` | `private void BuildQuickSection()` | Calls `BuildArrowCluster(...)`, assigns `_quickBtn = mainBtn`. Sets Content = `FormatBuffer("Quick", _quickT1)`. | 1 |
| `BuildQuickAllSection` | `private void BuildQuickAllSection()` | Calls `BuildArrowCluster(...)`, assigns `_quickAllBtn = mainBtn`. Sets Content = `FormatBuffer("Quick ALL", ...)`. | 1 |

**Note on `BuildArrowCluster` main button creation**: The Trim/Flatten buttons use `Background = BrushInactive` only; BE/Quick buttons use `BorderBrush = BrushTeal`, `Foreground = BrushTeal`, `BorderThickness = new Thickness(2)`. Pass `mainBorder` and `mainForeground` as nullable brush params (null = don't set). `if (mainBorder != null) btn.BorderBrush = mainBorder;` adds 1 branch per nullable, so CCN stays <= 3 total.

**Revised `BuildArrowCluster` signature** (simpler):
```csharp
private static (DockPanel cluster, Button mainBtn) BuildArrowCluster(
    string mainContent,
    System.Windows.Media.Brush mainBackground,
    bool useTealBorder,
    RoutedEventHandler upClick,
    RoutedEventHandler downClick,
    RoutedEventHandler mainClick)
```
CCN = base(1) + useTealBorder check(1) = **2**

**Parent `BuildBufferedButtonsRow` after extraction** (CCN = **1**, LoC ~25):
```csharp
var row1 = new UniformGrid { Columns = 2, ... Visibility = Visibility.Collapsed };
BuildTrimSection(row1);
BuildFlattenSection(row1);
root.Children.Add(row1);
_beRowPanel = new UniformGrid { Columns = 2, ... };
BuildBeSection();
BuildBeAllSection();
_quickRowPanel = new UniformGrid { Columns = 2, ... };
BuildQuickSection();
BuildQuickAllSection();
_quickT3Row = new StackPanel { ... Visibility = Visibility.Collapsed };
var lbl = new TextBlock { Text = "T3 hidden", ... };
_quickT3Row.Children.Add(lbl);
root.Children.Add(_quickT3Row);
```

**Target CCN after**: 1 (parent -- all calls unconditional)

**Estimated CodeScene signal removed**: Large Method (248 LoC -- HIGH WEIGHT)

**NT8 Thread Contract**: SAFE -- pure WPF construction called from `BuildUI` on UI thread. `SetResourceReference` is UI-safe.

**[Fact] test names**:
- `[Fact] BuildArrowCluster_SetsMainBackground_WhenProvided`
- `[Fact] BuildArrowCluster_SetsTealBorder_WhenUseTealBorderTrue`
- `[Fact] BuildArrowCluster_WiresUpDownAndMainClickHandlers`

---

## Ticket R3 -- Panel: `BuildUI` (Large Method 77 LoC)

**File**: `src/PropTraderTools/TradeCopierPanel.cs`
**Warning**: Large Method (L861 LoC=77)
**Method body lines**: L861-L967
**Current CCN**: 1 (straight-line construction)

### Extraction design

`BuildUI` constructs the root StackPanel by calling a sequence of section builders. The method is already partly modular (calls `BuildBufferedButtonsRow`, `BuildClickTraderRow`, `BuildRiskAtrRow`, `BuildCollapsibleHeader`, `BuildCopierSection`). The remaining "Large Method" violation comes from inline construction of 5 elements between those calls (tightenRow cluster, _followersDropDown, _followerScrollViewer, applyBtn, _statusText, _contentPanel).

| Helper | Signature | Body | CCN |
|--------|-----------|------|-----|
| `BuildFollowerScrollSection` | `private void BuildFollowerScrollSection()` | Constructs `_followersDropDown`, `_followerScrollViewerPanel`, `_followerScrollViewer`. Sets `MaxHeight`, `VerticalScrollBarVisibility`, `Content`, `Margin`. NO visual-tree insertion (per B47 implementation note). Sets `_followersDropDown.ItemTemplate = BuildCheckItemTemplate()`. | 1 |
| `BuildTightenRow` | `private StackPanel BuildTightenRow()` | Constructs tightenRow `StackPanel`, `_tightenTicksBox`, `_tightenBtn`, tightenLabel. Wires `OnTightenStop`. Sets resource refs. Adds children. Sets `Visibility = Collapsed`. Returns tightenRow. | 1 |

**Parent `BuildUI` after extraction** (CCN = **1**, LoC ~25):
```csharp
var root = new StackPanel { Margin = new Thickness(2) };
BuildFollowerScrollSection();
var applyBtn = new Button { Content = "Add Followers", ... Visibility = Visibility.Collapsed };
applyBtn.Click += OnApplyRule;
root.Children.Add(applyBtn);
_contentPanel = new StackPanel();
BuildBufferedButtonsRow(_contentPanel);
_statusText = new TextBlock { Text = "Open chart -- Trim/Flatten/Cancel/BE ready", ... };
_statusText.SetResourceReference(...);
BuildClickTraderRow(_contentPanel);
_contentPanel.Children.Add(BuildTightenRow());
BuildRiskAtrRow(_contentPanel);
root.Children.Add(_beRowPanel);
BuildInstrRow();
root.Children.Add(_instrRowPanel);
root.Children.Add(_quickRowPanel);
BuildCopierSection(root);
root.Children.Add(_statusText);
BuildCollapsibleHeader(root);
root.Children.Add(_contentPanel);
Content = root;
UpdateButtonColors(false, false);
```
LoC after extraction: ~24 lines. **Well under the Large Method threshold (~30 lines for CodeScene score impact).**

**Target CCN after**: 1

**Estimated CodeScene signal removed**: Large Method (77 LoC)

**NT8 Thread Contract**: SAFE -- pure WPF construction on UI thread.

**[Fact] test names**:
- `[Fact] BuildFollowerScrollSection_SetsFollowerScrollViewerContent`
- `[Fact] BuildTightenRow_StartsCollapsed`
- `[Fact] BuildTightenRow_WiresOnTightenStop`

---

## Ticket R4 -- Panel: `BuildRiskAtrRow` (Large Method 97 LoC)

**File**: `src/PropTraderTools/TradeCopierPanel.cs`
**Warning**: Large Method (L3082 LoC=97)
**Method body lines**: L3082-L3184
**Current CCN**: 1 (straight-line construction)

### Extraction design

`BuildRiskAtrRow` constructs a 2-column spinner panel (Risk $ | ATR %). Each column follows the pattern: Label + TextBox + arrows Grid.

| Helper | Signature | Body | CCN |
|--------|-----------|------|-----|
| `BuildSpinnerColumn` | `private StackPanel BuildSpinnerColumn(string labelText, TextBox valueBox, RoutedEventHandler upClick, RoutedEventHandler downClick)` | Creates StackPanel, TextBlock label, arrows Grid with 2 RowDefinitions, 2 RepeatButtons (up/dn) with SetResourceReference + Click handlers + Grid.SetRow. Returns StackPanel with label + valueBox + arrows. | 1 |
| `BuildAtrDisplayRow` | `private Border BuildAtrDisplayRow()` | Creates Border with CornerRadius=2, Padding, Margin. Creates `_atrDisplayLabel` TextBlock. Sets `atrRow.Child = _atrDisplayLabel`. Returns border. | 1 |

**Parent `BuildRiskAtrRow` after extraction** (CCN = **1**, LoC ~16):
```csharp
_atrRow = new UniformGrid { Columns = 2, Margin = new Thickness(0, 4, 0, 0) };
_riskDollarsBox = new TextBox { Text = _maxRiskDollars.ToString("F0"), Width = 55, ... };
_riskDollarsBox.LostFocus += OnRiskTextLostFocus;
_atrRow.Children.Add(BuildSpinnerColumn("Risk $", _riskDollarsBox, OnRiskUp, OnRiskDown));
_atrFractionBox = new TextBox { Text = _atrFraction.ToString("F2"), Width = 55, ... };
_atrFractionBox.LostFocus += OnAtrFractionTextLostFocus;
_atrRow.Children.Add(BuildSpinnerColumn("ATR %", _atrFractionBox, OnAtrFractionUp, OnAtrFractionDown));
root.Children.Add(_atrRow);
root.Children.Add(BuildAtrDisplayRow());
```

**Target CCN after**: 1

**Estimated CodeScene signal removed**: Large Method (97 LoC)

**NT8 Thread Contract**: SAFE -- pure WPF construction on UI thread.

**[Fact] test names**:
- `[Fact] BuildSpinnerColumn_WiresUpAndDownHandlers`
- `[Fact] BuildSpinnerColumn_ContainsLabelAndValueBox`
- `[Fact] BuildAtrDisplayRow_SetsAtrDisplayLabel`

---

## Ticket R5 -- Window: `BuildUI` (Large Method 80 LoC)

**File**: `src/PropTraderTools/TradeCopierWindow.cs`
**Warning**: Large Method (L226 LoC=80)
**Method body lines**: L226-L329
**Current CCN**: 1 (straight-line construction)

### Extraction design

`BuildUI` constructs a `DockPanel` by assembling a title, global toggle, mode row, separator, rules scroll area, add-rule button, separator, log area, and license row. The license row is already extracted (`BuildLicenseRow`).

| Helper | Signature | Body | CCN |
|--------|-----------|------|-----|
| `BuildModeRow` | `private StackPanel BuildModeRow()` | Creates horizontal StackPanel, "Copy Mode:" Label, `_modeCb` ComboBox (3 items, SelectionChanged wired). Returns StackPanel. | 1 |
| `BuildRulesScrollArea` | `private ScrollViewer BuildRulesScrollArea()` | Creates `_rulesPanel = new StackPanel()`, adds `BuildRuleRow("MES")`, wraps in `ScrollViewer { MaxHeight=400, ... }`. Returns ScrollViewer. | 1 |
| `BuildLogScrollArea` | `private ScrollViewer BuildLogScrollArea()` | Creates `_logPanel = new StackPanel()`, wraps in `ScrollViewer { ... }`. Returns ScrollViewer. | 1 |

**Parent `BuildUI` after extraction** (CCN = **1**, LoC ~22):
```csharp
var root = new DockPanel { LastChildFill = true };
var title = new TextBlock { Text = "Prop Trader Tools -- Trade Copier", FontWeight = FontWeights.Bold, ... };
DockPanel.SetDock(title, Dock.Top); root.Children.Add(title);
_globalToggleBtn = new Button { Content = "Copy All OFF", ... Background = WBrushInactive };
_globalToggleBtn.Click += OnGlobalToggle;
DockPanel.SetDock(_globalToggleBtn, Dock.Top); root.Children.Add(_globalToggleBtn);
var modeRow = BuildModeRow();
DockPanel.SetDock(modeRow, Dock.Top); root.Children.Add(modeRow);
root.Children.Add(new Separator { ... });
var rulesScroll = BuildRulesScrollArea();
DockPanel.SetDock(rulesScroll, Dock.Top); root.Children.Add(rulesScroll);
_addRuleBtn = new Button { Content = "+ Add Rule", ... };
_addRuleBtn.Click += OnAddRule;
DockPanel.SetDock(_addRuleBtn, Dock.Top); root.Children.Add(_addRuleBtn);
root.Children.Add(new Separator { ... });
BuildLicenseRow(root);
root.Children.Add(BuildLogScrollArea());
Content = root;
UpdateButtonColors(false, false);
```

**Target CCN after**: 1

**Estimated CodeScene signal removed**: Large Method (80 LoC)

**NT8 Thread Contract**: SAFE -- pure WPF construction on UI thread.

**[Fact] test names**:
- `[Fact] BuildModeRow_ContainsComboBoxWithThreeItems`
- `[Fact] BuildRulesScrollArea_InitializesRulesPanel`
- `[Fact] BuildLogScrollArea_InitializesLogPanel`

---

## Ticket R6 -- Panel: `BuildAtmMap(Account[])` Bumpy Road (cc=9, Bumpy Road L2412)

**File**: `src/PropTraderTools/TradeCopierPanel.cs`
**Warning**: Bumpy Road Ahead (bumps=2), Complex Method (cc=9) at L2412
**Method body lines**: L2412-L2431
**Current CCN**: 9 per CodeScene (nested foreach within foreach = "Bumpy Road")

### Extraction design

`BuildAtmMap(Account[] followers)` -- the B47 private overload (distinct from the T2 extraction `BuildAtmMap(Account[], string[])`) -- contains a nested foreach-within-foreach that CodeScene penalises as Bumpy Road.

The inner logic: "is this `_followerItem.Account` in the `followers` array?" is a membership check.

| Helper | Signature | Body | CCN |
|--------|-----------|------|-----|
| `IsAccountInFollowers` | `private static bool IsAccountInFollowers(Account account, Account[] followers)` | `foreach (var f in followers) if (f == account) return true; return false;` | 2 |

**Parent `BuildAtmMap(Account[])` after extraction** (CCN = **4**):
```csharp
var map = new Dictionary<string, FollowerAtmMode>();
foreach (var item in _followerItems)                                          // +1
{
    if (item.Account == null) continue;                                        // +1
    if (!IsAccountInFollowers(item.Account, followers)) continue;              // +1
    map[item.Account.Name] = ParseAtmModeNameLocal(item.AtmModeName ?? "Inherit"); // ?? = +1
}
return map;
// Total: base(1) + 3 = 4 ✓
```

The nested foreach that created the "Bumpy Road" pattern is gone. CodeScene no longer sees a loop-within-a-loop at this call site.

**Target CCN after**: 4

**Estimated CodeScene signal removed**: Bumpy Road (L2412), Complex Method (cc=9 reduced to cc=4)

**NT8 Thread Contract**: SAFE -- pure dictionary building, no Dispatcher, no NT8 API.

**[Fact] test names**:
- `[Fact] IsAccountInFollowers_ReturnsTrue_WhenAccountPresent`
- `[Fact] IsAccountInFollowers_ReturnsFalse_WhenAccountAbsent`
- `[Fact] IsAccountInFollowers_ReturnsFalse_WhenFollowersEmpty`

---

## Ticket R7 -- Panel: Dispatch Handler Duplication (L1515/L1548/L1777/L1949)

**File**: `src/PropTraderTools/TradeCopierPanel.cs`
**Warning**: Code Duplication at L1515, L1548, L1777, L1949
**Methods**: `OnTrimClick` (L1515), `OnFlattenClick` (L1548), `OnCancel2` (L1777), `OnQuickClick` (L1949)

### Duplication pattern

All 4 methods share this identical structure:
```csharp
if (_instrument == null) return;                          // guard
_leaderAccount = _leaderAccount ?? TryResolveLeaderAccount();  // late-resolve
NinjaTrader.Code.Output.Process(
    "[TAG] button: " + (_leaderAccount?.Name ?? "null") + " " + (_instrument?.FullName ?? "null"),
    NinjaTrader.NinjaScript.PrintTo.OutputTab1);
DispatchModule("TAG");
```

The ONLY differences are: the log prefix string (`[TRIM]`, `[FLAT]`, `[CANCEL]`, `[PTT-QX]`) and the module ID (`"TRIM"`, `"FLAT"`, `"CANCEL"` -- `OnQuickClick` is slightly different, calling `PttQuickExit` directly instead of `DispatchModule`).

For `OnTrimClick`, `OnFlattenClick`, `OnCancel2`: extract a shared private helper.
`OnQuickClick` has a different tail (creates `PttQuickExit` + passes `_quickT1`/`_quickT2`) -- partial deduplication only for the guard + log preamble.

| Helper | Signature | Body | CCN |
|--------|-----------|------|-----|
| `LogAndDispatchModule` | `private void LogAndDispatchModule(string logTag, string moduleId)` | Guards `_instrument == null` (return). Late-resolves `_leaderAccount`. Calls `NinjaTrader.Code.Output.Process(...)`. Calls `DispatchModule(moduleId)`. | 2 |

**Post-extraction `OnTrimClick`** (CCN = **1**):
```csharp
LogAndDispatchModule("[TRIM]", "TRIM");
```
**Post-extraction `OnFlattenClick`** (CCN = **1**):
```csharp
LogAndDispatchModule("[FLAT]", "FLAT");
```
**Post-extraction `OnCancel2`** (CCN = **1**):
```csharp
LogAndDispatchModule("[CANCEL]", "CANCEL");
```
**Post-extraction `OnQuickClick`** (CCN = **2**):
```csharp
if (_instrument == null) return;                          // +1 (cannot use helper -- different tail)
_leaderAccount = _leaderAccount ?? TryResolveLeaderAccount();
NinjaTrader.Code.Output.Process("[PTT-QX] button: " + ... + " t1=" + _quickT1, ...);
var qx = new PttQuickExit();
qx.Execute(_leaderAccount, _instrument, _quickT1, _quickT2); // +1
```

Note: `OnQuickClick` is a partial deduplication -- the guard + log preamble is kept inline because the tail differs. Still removes the duplication signal for L1515/1548/1777.

**Target CCN after**: 1 (Trim/Flatten/Cancel), 2 (QuickClick -- unchanged)

**Estimated CodeScene signal removed**: Code Duplication cluster (L1515/L1548/L1777/L1949)

**NT8 Thread Contract**: SAFE -- `DispatchModule` and `PttQuickExit.Execute` are called from UI-thread event handlers. `LogAndDispatchModule` is called only from UI-thread event handlers. No Dispatcher needed (already on UI thread).

**[Fact] test names**:
- `[Fact] LogAndDispatchModule_ReturnsEarly_WhenInstrumentNull`
- `[Fact] LogAndDispatchModule_ResolvesLeaderAccount_WhenNull`
- `[Fact] LogAndDispatchModule_CallsDispatchModule_WithCorrectId`

---

## Ticket R8 -- Panel: Spinner Handler Pair Duplication (L3215/L3246)

**File**: `src/PropTraderTools/TradeCopierPanel.cs`
**Warning**: Code Duplication at L3215 (`OnRiskTextLostFocus`), L3246 (`OnAtrFractionTextLostFocus`)

### Duplication pattern

Both methods share:
```csharp
double v;
if (!double.TryParse(_xxxBox?.Text, out v)) return;
v = Math.Max(Math.Min(v, MAX), MIN);
_xxxValue = v;
if (_xxxBox != null) _xxxBox.Text = v.ToString("FORMAT");
NotifyXxxChanged();
```
They differ only in: TextBox field, field variable, min/max bounds, format string, and notify method.

**Strategy**: These are short (7 lines each) and their parameters are all private field refs -- no clean static extraction possible without passing field references. The duplication is structural (same pattern, different variables). CodeScene will stop flagging them once the logic is slightly restructured.

| Helper | Signature | Body | CCN |
|--------|-----------|------|-----|
| `TryParseAndClamp` | `private static bool TryParseAndClamp(string text, double min, double max, out double result)` | `if (!double.TryParse(text, out result)) return false; result = Math.Max(Math.Min(result, max), min); return true;` | 2 |

**Post-extraction `OnRiskTextLostFocus`** (CCN = **2**):
```csharp
if (!TryParseAndClamp(_riskDollarsBox?.Text, 10.0, 1000.0, out double v)) return;  // +1
_maxRiskDollars = v;
if (_riskDollarsBox != null) _riskDollarsBox.Text = v.ToString("F0");              // +1
NotifyRiskChanged();
```
**Post-extraction `OnAtrFractionTextLostFocus`** (CCN = **2**):
```csharp
if (!TryParseAndClamp(_atrFractionBox?.Text, 0.25, 3.00, out double v)) return;    // +1
_atrFraction = v;
if (_atrFractionBox != null) _atrFractionBox.Text = v.ToString("F2");              // +1
NotifyAtrFractionChanged();
```

`TryParseAndClamp` is also re-usable by `OnRiskUp`/`OnRiskDown` if needed in future, but no change is needed there now (those are already short/clean).

**Target CCN after**: 2 (both)

**Estimated CodeScene signal removed**: Code Duplication (L3215/L3246)

**NT8 Thread Contract**: SAFE -- pure math/parse helpers, no NT8 API.

**[Fact] test names**:
- `[Fact] TryParseAndClamp_ReturnsFalse_WhenParseFailsOnNonNumericText`
- `[Fact] TryParseAndClamp_ClampsToMin_WhenValueBelowRange`
- `[Fact] TryParseAndClamp_ClampsToMax_WhenValueAboveRange`
- `[Fact] TryParseAndClamp_ReturnsTrue_AndPreservesValue_WhenInRange`

---

## Ticket R9 -- Panel: `OnInstr2tClick` / `OnInstrQAll2tClick` Duplication (L1998/L2030)

**File**: `src/PropTraderTools/TradeCopierPanel.cs`
**Warning**: Code Duplication at L1998, L2030
**Methods**: `OnInstr2tClick`, `OnInstrQAll2tClick`

### Duplication pattern

Both methods share:
```csharp
if (_instrument == null) return;
_leaderAccount = _leaderAccount ?? TryResolveLeaderAccount();
if (_leaderAccount == null) return;
var pos = _leaderAccount.Positions.FirstOrDefault(p => p.Instrument?.FullName == _instrument.FullName);
int qty = pos?.Quantity ?? 1;
var targets = Build2TargetList(qty);
// log
// fire different Execute overload
```

| Helper | Signature | Body | CCN |
|--------|-----------|------|-----|
| `TryResolve2TargetContext` | `private bool TryResolve2TargetContext(out int qty, out List<(double Price, int Qty)> targets)` | Guards `_instrument == null` (+1). Late-resolves leader. Guards `_leaderAccount == null` (+1). Calls `Positions.FirstOrDefault` lambda (+1). Builds `qty = pos?.Quantity ?? 1` (+1). Calls `Build2TargetList`. Assigns targets. Returns true. | 4 |

**Note on JS-002**: `TryResolve2TargetContext` returns bool (not null). `qty` and `targets` are `out` params -- always assigned (even on false path, `targets = null` is technically needed but we can use `targets = new List<...>()` to avoid null). Actually: on return false path, the helper returns false and callers immediately return, so the out values are ignored. Use `targets = new List<(double,int)>()` as zero-alloc-safe sentinel. ✓

**Post-extraction `OnInstr2tClick`** (CCN = **2**):
```csharp
if (!TryResolve2TargetContext(out int qty, out var targets)) return;  // +1
NinjaTrader.Code.Output.Process("[PTT-QX-2T] button: " + _leaderAccount.Name + " " + _instrument.FullName + " qty=" + qty + " T1=" + targets[0].Qty + " T2=" + targets[1].Qty, ...);
new PttQuickExit().Execute(_leaderAccount, _instrument, 4, targets);  // +1 -- overload call
```

**Post-extraction `OnInstrQAll2tClick`** (CCN = **2**):
```csharp
if (!TryResolve2TargetContext(out int qty, out var targets)) return;  // +1
NinjaTrader.Code.Output.Process("[PTT-QX-2T-ALL] button: " + _leaderAccount.Name + " " + _instrument.FullName + " qty=" + qty + " T1=" + targets[0].Qty + " T2=" + targets[1].Qty, ...);
new PttGlobalQuickExit().Execute(targets);                            // +1 -- different overload
```

**Target CCN after**: 2 (both)

**Estimated CodeScene signal removed**: Code Duplication (L1998/L2030)

**NT8 Thread Contract**: CONSTRAINED -- `_leaderAccount.Positions.FirstOrDefault` accesses NT8 Account.Positions. This is called from UI-thread Click handlers. The helper is `private` and MUST only be called from UI-thread methods. Comment in helper: `// MUST only be called on UI thread (accesses Account.Positions)`.

**[Fact] test names**:
- `[Fact] TryResolve2TargetContext_ReturnsFalse_WhenInstrumentNull`
- `[Fact] TryResolve2TargetContext_ReturnsFalse_WhenLeaderNull`
- `[Fact] TryResolve2TargetContext_ReturnsQtyOne_WhenNoPositionFound`

---

## Tickets NOT Designed (SKIP rationale)

### R10 -- SKIP: `GetAsk` / `GetBid` (L1714/L1730)

**Rationale**: `GetAsk` and `GetBid` are nearly identical 7-line null-guard chains returning `double`. The "duplication" is structural (same null-chain, different `Ask` vs `Bid` field). Extracting them into a single shared helper (`GetMarketPrice(bool ask)`) would:
1. Introduce a boolean discriminator parameter -- CodeScene sometimes scores `GetPrice(bool useAsk)` worse than two simple methods.
2. Add a branch (`if (useAsk)`) inside the helper, raising CCN.
3. Provide no meaningful LoC reduction (7 -> ~5 per method).

**Decision**: SKIP. The 2-cluster duplication signal at L1714/L1730 does not materially impact score vs the score gain from R1-R9. These 2 methods are also only flagged as Code Duplication (medium-weight signal), not Large Method or Bumpy Road.

### R11 -- SKIP: `BuildMultipliers` / `BuildFollowerMultipliers` duplicate logic

These already serve different signatures and callers. `BuildMultipliers(Account[])` is the B47 inline version; `BuildFollowerMultipliers(Account[])` is the T2 CYC extraction version. Merging them would require API change (banned) or one-line wrapper that adds complexity. SKIP.

### R12 -- SKIP: `UpdateButtonColors` Window (L201 cc=10)

`UpdateButtonColors` in TradeCopierWindow (L201-212) has CYC=5 by lizard (4 foreach + 1 bool). CodeScene reports cc=10. The 4 foreach loops are structural -- they set `.Background` on collections of buttons. This is already the minimal form. Extracting `foreach` into named helpers would add 4 new methods of 1 line each, increasing function count (already a file-level warning). No benefit; risk of worsening "Number of Functions" score. SKIP.

---

## Execution Order

| Ticket | File | Signal Removed | Est. Score Gain |
|--------|------|---------------|-----------------|
| R1 | Window | Large Method x2 (202+210 LoC) + Duplication x2 | Window +1.0 |
| R2 | Panel | Large Method (248 LoC) -- highest single signal | Panel +0.7 |
| R3 | Panel | Large Method (77 LoC) | Panel +0.3 |
| R4 | Panel | Large Method (97 LoC) | Panel +0.3 |
| R5 | Window | Large Method (80 LoC) | Window +0.3 |
| R6 | Panel | Bumpy Road (cc=9) + Complex Method | Panel +0.2 |
| R7 | Panel | Code Duplication x4 (L1515/1548/1777/1949) | Panel +0.3 |
| R8 | Panel | Code Duplication x2 (L3215/L3246) | Panel +0.15 |
| R9 | Panel | Code Duplication x2 (L1998/L2030) | Panel +0.15 |

### Estimated Score After All Tickets

Panel: 4.71 + 0.7 + 0.3 + 0.3 + 0.2 + 0.3 + 0.15 + 0.15 = **~6.8** (conservative -- CodeScene scoring is non-linear; actual may be higher due to signal interactions)

**Note**: Panel target is >= 7.0. The remaining file-level signals (Low Cohesion, Primitive Obsession, Number of Functions) apply a fixed penalty that caps the maximum achievable score without a file split. If score lands at 6.8 after R2-R9, further deduplication passes (e.g. R10 GetAsk/GetBid) may push it over 7.0.

Window: 6.61 + 1.0 + 0.3 = **~7.9** (conservative; R1 alone removes 2 Large Methods + their duplication, Window should comfortably reach 8.0)

---

## Mandatory Verification Gates (per ticket)

After each ticket:
1. `dotnet build` -- 0 errors, 0 warnings
2. `dotnet test` -- 370 pass, 22 pre-existing IL-reflection (ACCEPT), 0 new failures
3. `lizard src/PropTraderTools/TradeCopierPanel.cs --CCN 8` (or Window) -- 0 warnings for modified methods
4. `$env:CS_ACCESS_TOKEN="..."; cs delta` -- score does NOT decrease on modified file

**P0 Scans** (run before first commit):
```powershell
Select-String "lock(" src/PropTraderTools -Recurse -Include *.cs   # must be 0
Select-String "async void " src/PropTraderTools -Recurse -Include *.cs  # must be 0
```

---

**Build Tag**: PTT-COPIER BWAVE-CYC Lane-C Remediation | 2025-01-30
**Architect**: ptt-architect
**Status**: READY FOR ptt-engineer (execute R1 -> R2 -> R3 -> R4 -> R5 -> R6 -> R7 -> R8 -> R9)
