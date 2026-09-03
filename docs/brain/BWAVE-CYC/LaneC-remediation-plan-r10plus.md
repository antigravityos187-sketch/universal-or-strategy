# Lane C CodeScene Remediation Plan -- R10+ (Post-R1-R9)

**Produced by**: ptt-architect (Remediation Stage -- R10+ Pass)
**Date**: 2025-08-11
**Wave**: BWAVE-CYC -- Lane C CodeScene Score Remediation (second pass)
**Status**: READY FOR ptt-engineer
**Prerequisite**: R1-R9 implemented, build passes, lizard 0 warnings.

---

## Current State (post-R1-R9)

| File | Score | Target | Gap |
|------|-------|--------|-----|
| `TradeCopierPanel.cs` | 6.08 / 10 | >= 7.0 | +0.92 |
| `TradeCopierWindow.cs` | 7.43 / 10 | >= 8.0 | +0.57 |
| `TradeCopierAddOn.cs` | 10.00 / 10 | 10.0 | PASS -- no work |

---

## Constraints (re-stated, non-negotiable)

| Rule | Requirement |
|------|-------------|
| JS-021 | No `lock()` -- zero new lock blocks |
| JS-002 | No new `return null` for helpers that must return values (use bool/out pattern) |
| JS-033 | No `async void` |
| CYC parent | <= 8 after extraction |
| CYC helper | <= 4 per new extracted helper |
| NT8 UI thread | Dispatcher.InvokeAsync stays in original methods; no NT8 Account/Order API in helpers callable off-thread |
| ASCII-only | All identifiers and string literals ASCII (no Unicode emoji, no curly quotes) |
| Private only | Zero new public or internal surface |
| No .cs edits | ptt-architect MUST NOT write .cs files. ptt-engineer only. |

---

## Warnings Marked SKIP (with rationale)

| Warning | Reason |
|---------|--------|
| Panel L502 `FindPriceCanvasPanel()` Complex Conditional | 3-part compound already minimal. Extraction adds a helper for zero CYC gain. Function count cost exceeds score benefit. |
| Panel L515 (same method as L502) | Duplicate report of same compound -- same rationale as L502. |
| Panel L685 `CancelOrphanBracketsOnFlat()` Complex Conditional | 2-line method. The `&&` chain is already minimal form. No further extraction possible. |
| Panel L1164 `BuildArrowCluster()` Excess Args (6) | Private static, already the R2 extracted helper. All 6 args are structurally necessary (content, bg, teal, up, dn, main). A builder struct adds a new named type -- worse for "Number of Functions" than keeping 6 args. |
| Panel L1623/L1639 `GetAsk()`/`GetBid()` Code Duplication | Two 9-line null-guard chains. A shared `GetMarketPrice(bool useAsk)` adds a bool discriminator branch and makes intent less clear. Prior plan R10 already evaluated this as SKIP. Score impact is medium-weight only. SKIP confirmed. |
| Panel L2026 `FindWorkingOrder()` cc=9 (CodeScene) | Lizard CYC=2. CodeScene's scoring counts compound conditions and multiple exits differently. The method is already at minimal form: null guard + foreach + 3 continue guards. No further extraction without adding a predicate helper of CYC=3 that saves nothing. SKIP. |
| Window L201 `UpdateButtonColors()` cc=10 (CodeScene) | CYC=5 per lizard. 5 sequential foreach loops on button collections. Already at minimal form post-R1. Extracting 4 single-line foreach helpers adds 4 new methods and WORSENS the "Number of Functions" file-level penalty. SKIP confirmed (same conclusion as prior plan). |
| Window L750 `BuildActionButtons()` Excess Args (5) | Private method, already the R1 extracted helper. All 5 args are necessary (tag0, leaderCb, followerLb, atmPanel, grid). Same reasoning as Panel L1164. SKIP. |

---

## Ticket R10 -- Panel: `Detach()` Bumpy Road + Complex Method (cc=10)

**File**: `src/PropTraderTools/TradeCopierPanel.cs`
**Warning**: Bumpy Road Ahead (bumps=2), Complex Method (cc=10) at L577
**Method body lines (current)**: L577-L625

### Code observed (actual)

```csharp
public void Detach()
{
    _engine.Unsubscribe();
    if (_currentChart != null)
        TradeCopierAddOn.UnregisterClickTrader(_currentChart);     // branch 1
    _engine.StatusUpdate -= OnStatusUpdate;
    // ... 5 more event unsubscribes ...
    foreach (var item in _followerItems)                           // branch 2 (Bumpy Road #1)
        if (item.Account != null)                                   // branch 3
            item.Account.AccountItemUpdate -= OnAccountItemUpdate;
    _engine.DisarmPendingBe(_leaderAccount);
    _engine.CopyEnabledChanged -= OnCopyEnabledChanged;
    if (_leaderAccount != null)                                    // branch 4
    {
        _leaderAccount.OrderUpdate -= OnLeaderOrderUpdate;
        _leaderAccount.PositionUpdate -= OnLeaderPositionUpdate;
    }
    if (_accountCombo != null && _accountComboSelectionChanged != null) // branch 5+6
    {
        _accountCombo.SelectionChanged -= _accountComboSelectionChanged;
        ...
    }
    if (Account.All != null)                                       // branch 7 (Bumpy Road #2)
        foreach (var acc in Account.All)                           // branch 8
            CopyEngine.Instance.DisarmPendingBe(acc);
    foreach (IPttModule m in _modules)                             // branch 9
        m.Teardown();
    _modules.Clear();
    ...
    CopyEngine.Instance.FeatureFlagsChanged -= OnFeatureFlagsChanged;
}
```
**Current CYC (CodeScene-estimated)**: 10. Two nested foreach patterns = "Bumpy Road".

### Extraction design

Extract 2 helpers that hold the two "bumpy" foreach blocks:

| Helper | Signature | Body | CYC |
|--------|-----------|------|-----|
| `UnsubscribeFollowerItems` | `private void UnsubscribeFollowerItems()` | `foreach (var item in _followerItems) if (item.Account != null) item.Account.AccountItemUpdate -= OnAccountItemUpdate;` | 2 |
| `DisarmAllAccounts` | `private static void DisarmAllAccounts()` | `if (Account.All == null) return; foreach (var acc in Account.All) CopyEngine.Instance.DisarmPendingBe(acc);` | 2 |

**Parent `Detach()` after extraction** (CYC = **5**):
```csharp
public void Detach()
{
    _engine.Unsubscribe();
    if (_currentChart != null)                                      // +1
        TradeCopierAddOn.UnregisterClickTrader(_currentChart);
    _engine.StatusUpdate -= OnStatusUpdate;
    // ... 5 event unsubscribes (straight-line) ...
    UnsubscribeFollowerItems();                                     // call (0 branches)
    _engine.DisarmPendingBe(_leaderAccount);
    _engine.CopyEnabledChanged -= OnCopyEnabledChanged;
    if (_leaderAccount != null)                                     // +1
    {
        _leaderAccount.OrderUpdate -= OnLeaderOrderUpdate;
        _leaderAccount.PositionUpdate -= OnLeaderPositionUpdate;
    }
    if (_accountCombo != null && _accountComboSelectionChanged != null) // +2
    {
        _accountCombo.SelectionChanged -= _accountComboSelectionChanged;
        _accountCombo = null;
        _accountComboSelectionChanged = null;
    }
    _instrument = null; _leaderAccount = null;
    DisarmAllAccounts();                                            // call (0 branches)
    foreach (IPttModule m in _modules) m.Teardown();               // +1
    _modules.Clear(); _allAccounts.Clear();
    CopyEngine.Instance.FeatureFlagsChanged -= OnFeatureFlagsChanged;
}
// Total: base(1) + 5 branches = CYC 6 (conservative). Bumpy Road eliminated.
```

**Target CCN after**: Detach ~6, helpers <=2 each.

**Signals removed**: Bumpy Road Ahead (bumps=2), Complex Method (cc=10 -> ~6).

**NT8 Thread Contract**: SAFE -- `Detach()` is called from `NTPanel.OnWindowDestroyed` on the UI thread. Both helpers are called only from `Detach()`. `UnsubscribeFollowerItems` accesses `_followerItems` (UI-thread-owned list) and `Account.AccountItemUpdate` (NT8 Account event, safe from UI thread). `DisarmAllAccounts` reads `Account.All` (NT8 safe on UI thread) and calls `CopyEngine.Instance.DisarmPendingBe` (lock-free enqueue). Comment required in both helpers: `// MUST only be called from Detach() on UI thread`.

**[Fact] test names**:
- `[Fact] UnsubscribeFollowerItems_DoesNotThrow_WhenFollowerItemsContainsNullAccount`
- `[Fact] UnsubscribeFollowerItems_ProcessesAllItems_InFollowerItemsList`
- `[Fact] DisarmAllAccounts_DoesNotThrow_WhenAccountAllIsNull`
- `[Fact] DisarmAllAccounts_CallsDisarmPendingBe_ForEachAccount`

---

## Ticket R11 -- Panel: `BuildBufferedButtonsRow` 6x Code Duplication (L1212-L1282)

**File**: `src/PropTraderTools/TradeCopierPanel.cs`
**Warning**: Code Duplication x6 at L1212, L1226, L1240, L1254, L1268, L1282
**Methods**: `BuildTrimSection`, `BuildFlattenSection`, `BuildBeSection`, `BuildBeAllSection`, `BuildQuickSection`, `BuildQuickAllSection`

### Duplication pattern observed

All 6 methods share identical structural shape (3-4 lines each):
```csharp
private void BuildXxxSection(...)
{
    var (cluster, mainBtn) = BuildArrowCluster(<content>, <bg>, <teal>, <upH>, <dnH>, <mainH>);
    _xxxBtn = mainBtn;
    <panel>.Children.Add(cluster);
}
```
The ONLY differences: content string, useTealBorder bool, 3 click handlers, button field assignment, target panel.
CodeScene detects these 6 methods as a duplicate clone cluster because they share the same structural template.

### Extraction design

**Strategy**: Remove all 6 wrapper methods. Replace with a data-driven loop inside `BuildBufferedButtonsRow` using a local `ValueTuple` array of `ArrowClusterSpec`. The loop calls `BuildArrowCluster` directly and uses an `Action<Button>` to assign the result to the correct field. This removes 6 methods (helping "Number of Functions" file-level signal) and eliminates the clone cluster.

```csharp
// Inside BuildBufferedButtonsRow (replaces 6 section-builder calls):
var specs = new (
    string Content, Brush Bg, bool Teal,
    RoutedEventHandler Up, RoutedEventHandler Dn, RoutedEventHandler Main,
    Action<Button> Store, Panel Target
)[]
{
    (FormatBuffer("Trim",    _trimBuffer),    BrushInactive, false, OnTrimUp,     OnTrimDown,     OnTrimClick,     b => _trimBtn2    = b, row1),
    (FormatBuffer("Flatten", _flattenBuffer), BrushInactive, false, OnFlattenUp,  OnFlattenDown,  OnFlattenClick,  b => _flattenBtn2 = b, row1),
    (FormatBuffer("BE",      _beBuffer),      BrushInactive, true,  OnBeUp,       OnBeDown,       OnBeClick,       b => _beBtn2      = b, _beRowPanel),
    (FormatGlobalBeBuffer("BE ALL", ...), BrushInactive, true,  OnGlobalBeUp, OnGlobalBeDown, OnGlobalBeClick, b => _globalBeBtn2 = b, _beRowPanel),
    (FormatBuffer("Quick",   _quickT1),       BrushInactive, true,  OnQuickUp,    OnQuickDown,    OnQuickClick,    b => _quickBtn     = b, _quickRowPanel),
    (FormatBuffer("Quick ALL",...),           BrushInactive, true,  OnQuickAllUp, OnQuickAllDown, OnQuickAllClick, b => _quickAllBtn  = b, _quickRowPanel),
};
foreach (var s in specs)
{
    var (cluster, btn) = BuildArrowCluster(s.Content, s.Bg, s.Teal, s.Up, s.Dn, s.Main);
    s.Store(btn);
    s.Target.Children.Add(cluster);
}
```

**Parent `BuildBufferedButtonsRow` after**: CYC = 1(base) + 1(foreach) = **2**. LoC ~35 (array initialization + loop = compact, no duplication).

**6 helper methods removed**: `BuildTrimSection`, `BuildFlattenSection`, `BuildBeSection`, `BuildBeAllSection`, `BuildQuickSection`, `BuildQuickAllSection`. Net function count reduced by 6.

**Target CCN after**: BuildBufferedButtonsRow CYC=2. Removed methods no longer exist.

**Signals removed**: Code Duplication x6 (L1212/L1226/L1240/L1254/L1268/L1282). Also improves "Number of Functions" file-level signal (net -6 methods).

**NT8 Thread Contract**: SAFE -- `BuildBufferedButtonsRow` is called from `BuildUI` on the UI thread. All delegates (OnTrimUp, etc.) are wired as event handlers to UI elements. No NT8 Account/Order API. `Action<Button>` lambdas capture `this` for field assignment -- executed synchronously on UI thread during construction.

**[Fact] test names**:
- `[Fact] BuildBufferedButtonsRow_AssignsTrimBtn2_AfterConstruction`
- `[Fact] BuildBufferedButtonsRow_AssignsAllSixButtonFields_NonNull`
- `[Fact] BuildBufferedButtonsRow_UsesTealBorder_ForBeBeAllQuickQuickAll`
- `[Fact] BuildBufferedButtonsRow_AddsClusterToCorrectPanel_ForEachSection`

---

## Ticket R12 -- Panel: `OnInstr2tClick`/`OnInstrQAll2tClick` Log Duplication (L1921/L1944)

**File**: `src/PropTraderTools/TradeCopierPanel.cs`
**Warning**: Code Duplication at L1921, L1944

### Duplication pattern observed

Both `OnInstr2tClick` (L1921) and `OnInstrQAll2tClick` (L1944) share an identical multi-line log statement after calling `TryResolve2TargetContext`:

```csharp
NinjaTrader.Code.Output.Process(
    "[PTT-QX-2T] button: "           // differs only in "[PTT-QX-2T]" vs "[PTT-QX-2T-ALL]"
    + _leaderAccount.Name + " " + _instrument.FullName
    + " qty=" + qty
    + " T1=" + targets[0].Qty + " T2=" + targets[1].Qty,
    NinjaTrader.NinjaScript.PrintTo.OutputTab1);
```

### Extraction design

| Helper | Signature | Body | CYC |
|--------|-----------|------|-----|
| `LogQxTwoTarget` | `private void LogQxTwoTarget(string prefix, int qty, List<(double Price, int Qty)> targets)` | Calls `NinjaTrader.Code.Output.Process` with formatted string. Reads `_leaderAccount.Name` and `_instrument.FullName`. | 1 |

**Post-extraction `OnInstr2tClick`** (CYC = **2**):
```csharp
if (!TryResolve2TargetContext(out int qty, out var targets)) return;  // +1
LogQxTwoTarget("[PTT-QX-2T]", qty, targets);
new PttQuickExit().Execute(_leaderAccount, _instrument, 4, targets); // +1
```

**Post-extraction `OnInstrQAll2tClick`** (CYC = **2**):
```csharp
if (!TryResolve2TargetContext(out int qty, out var targets)) return;  // +1
LogQxTwoTarget("[PTT-QX-2T-ALL]", qty, targets);
new PttGlobalQuickExit().Execute(targets);                            // +1
```

**Target CCN after**: 2 (both parents), 1 (helper).

**Signals removed**: Code Duplication (L1921/L1944).

**NT8 Thread Contract**: SAFE -- `NinjaTrader.Code.Output.Process` is thread-safe. `LogQxTwoTarget` reads `_leaderAccount` and `_instrument` (non-null guaranteed by `TryResolve2TargetContext`). Called from UI-thread Click handlers only.

**[Fact] test names**:
- `[Fact] LogQxTwoTarget_DoesNotThrow_WithValidPrefixAndTargetList`
- `[Fact] LogQxTwoTarget_IncludesPrefixAndQty_InFormattedOutput`

---

## Ticket R13 -- Panel: `BuildMultipliers` / `BuildFollowerMultipliers` Duplication (L2343/L2835)

**File**: `src/PropTraderTools/TradeCopierPanel.cs`
**Warning**: Code Duplication at L2343, L2835

### Duplication pattern observed

`BuildMultipliers(Account[] followers)` at L2343 (returns `int[]`) and `BuildFollowerMultipliers(Account[] followers)` at L2835 (returns `(int[], string[])`) share an identical double-loop inner body:

```csharp
// Both methods (same structural clone):
for (int i = 0; i < followers.Length; i++)
{
    foreach (var item in _followerItems)
    {
        if (item.Account != followers[i]) continue;
        multipliers[i] = item.Multiplier > 0 ? item.Multiplier : 1;
        break;
    }
}
```

`BuildFollowerMultipliers` additionally collects `atmNames[i] = item.AtmModeName ?? "Inherit"` in the same loop.
`BuildMultipliers` only needs the multipliers (it discards ATM names).

### Extraction design

Replace `BuildMultipliers` body with a 1-line delegation to `BuildFollowerMultipliers`:

```csharp
// R13: BuildMultipliers as delegation wrapper -- removes duplicate double-loop. CYC=1.
private int[] BuildMultipliers(Account[] followers)
{
    var (mults, _) = BuildFollowerMultipliers(followers);
    return mults;
}
```

`BuildFollowerMultipliers` (L2835) is UNCHANGED -- it remains the canonical implementation.

**Target CCN after**: BuildMultipliers CYC=1, BuildFollowerMultipliers CYC unchanged.

**Signals removed**: Code Duplication (L2343/L2835).

**NT8 Thread Contract**: SAFE -- pure data transformation. No NT8 API. Called from `OnApplyRule` on UI thread.

**[Fact] test names**:
- `[Fact] BuildMultipliers_ReturnsMultipliersArray_MatchingBuildFollowerMultipliers`
- `[Fact] BuildMultipliers_ReturnsDefaultOne_WhenItemMultiplierIsZero`

---

## Ticket R14 -- Panel: `OnFollowerAtmTemplateComboLoaded` Bumpy Road (L2493)

**File**: `src/PropTraderTools/TradeCopierPanel.cs`
**Warning**: Bumpy Road Ahead (bumps=2) at L2493

### Code observed (actual)

`OnFollowerAtmTemplateComboLoaded` (L2493) contains a foreach loop scanning `_atmComboRefs` to detect if `cb` is already tracked, followed by an `if (!alreadyTracked)` block:

```csharp
bool alreadyTracked = false;
foreach (var wr in _atmComboRefs)                        // Bumpy Road -- loop #1
    if (wr.TryGetTarget(out var existing) && existing == cb)
    {
        alreadyTracked = true;
        break;
    }
if (!alreadyTracked)                                     // condition after loop
{
    _atmComboRefs.Add(new WeakReference<ComboBox>(cb));
    if (CopyEngine.Instance.GetCopyMode() == CopyMode.Clone)  // inner branch
        cb.Visibility = Visibility.Collapsed;
}
cb.Items.Add("(none)");
// ... etc.
```

The "Bumpy Road" pattern: a loop followed immediately by an if that tests the loop's accumulator variable.

### Extraction design

| Helper | Signature | Body | CYC |
|--------|-----------|------|-----|
| `IsAtmComboAlreadyTracked` | `private bool IsAtmComboAlreadyTracked(ComboBox cb)` | `foreach (var wr in _atmComboRefs) if (wr.TryGetTarget(out var e) && e == cb) return true; return false;` | 2 |

**Parent `OnFollowerAtmTemplateComboLoaded` after extraction** (CYC = **3**):
```csharp
var cb = sender as ComboBox;
if (cb == null) return;                                    // +1
if (cb.Items.Count > 0) return;                            // +1
if (!IsAtmComboAlreadyTracked(cb))                         // +1
{
    _atmComboRefs.Add(new WeakReference<ComboBox>(cb));
    if (CopyEngine.Instance.GetCopyMode() == CopyMode.Clone)
        cb.Visibility = Visibility.Collapsed;
}
cb.Items.Add("(none)");
string leaderTemplate = GetLeaderAtmTemplateName(_currentChart);
PopulateAtmComboItems(cb, leaderTemplate, out int defaultIdx);
cb.SelectedIndex = defaultIdx;
ApplyAtmAutoSelect(cb, defaultIdx);
// Total: base(1) + 3 = 4 (the CopyMode check inside the if is now within the if block, counted once).
```

**Target CCN after**: OnFollowerAtmTemplateComboLoaded CYC=4, helper CYC=2. Bumpy Road eliminated.

**Signals removed**: Bumpy Road Ahead (L2493).

**NT8 Thread Contract**: SAFE -- `IsAtmComboAlreadyTracked` reads `_atmComboRefs` (UI-thread-owned `List<WeakReference<ComboBox>>`). Called only from `OnFollowerAtmTemplateComboLoaded` which fires on the WPF UI thread (DataTemplate Loaded event). Comment: `// MUST only be called on UI thread (_atmComboRefs is UI-thread-owned)`.

**[Fact] test names**:
- `[Fact] IsAtmComboAlreadyTracked_ReturnsFalse_WhenComboBoxNotInTrackedList`
- `[Fact] IsAtmComboAlreadyTracked_ReturnsTrue_WhenComboBoxAlreadyInTrackedList`
- `[Fact] IsAtmComboAlreadyTracked_ReturnsFalse_WhenTrackedListIsEmpty`

---

## Ticket R15 -- Window: `BuildRuleRow`/`BuildDynamicRuleRow` Duplication (L478/L529)

**File**: `src/PropTraderTools/TradeCopierWindow.cs`
**Warning**: Code Duplication at L478, L529

### Duplication pattern observed

After R1 refactoring, both methods are ~28 lines and share an identical call sequence:
```csharp
var grid = new Grid { Margin = new Thickness(2) };
BuildGridColumnDefinitions(grid, <bool>);
// Col 0: <TextBlock OR TextBox> -- the ONLY structural difference
var leaderCb = new ComboBox { ... }; leaderCb.ItemTemplate = ...; // (+ optional ItemsSource binding)
_leaderBoxes.Add(leaderCb); Grid.SetColumn(leaderCb, 1); grid.Children.Add(leaderCb);
var followerLb = BuildFollowerListBox(); // (+ optional ItemsSource binding)
_followerBoxes.Add(followerLb); Grid.SetColumn(followerLb, 2); grid.Children.Add(followerLb);
var atmPanel = BuildAtmColumnPanel();
BuildActionButtons(<col0>, leaderCb, followerLb, atmPanel, grid);
var beCluster = BuildBeCluster(<col0>); Grid.SetColumn(beCluster, 8); grid.Children.Add(beCluster);
Grid.SetColumn(atmPanel, 9); grid.Children.Add(atmPanel);
var tightenCluster = BuildTightenCluster(<col0>); Grid.SetColumn(tightenCluster, 10); grid.Children.Add(tightenCluster);
var armBeCluster = BuildArmBeCluster(<col0>, leaderCb); Grid.SetColumn(armBeCluster, 11); grid.Children.Add(armBeCluster);
return grid;
```
`<col0>` is `instrumentName` (string) in `BuildRuleRow` and `instrTextBox` (TextBox) in `BuildDynamicRuleRow`.
`<bool>` for `BuildGridColumnDefinitions` is `false` / `true`.
Optional `ItemsSource` binding: only in `BuildDynamicRuleRow`.

### Extraction design

| Helper | Signature | Body | CYC |
|--------|-----------|------|-----|
| `BuildRuleRowCore` | `private Grid BuildRuleRowCore(UIElement col0Element, bool bindAccountsNow)` | Creates Grid, calls BuildGridColumnDefinitions(grid, bindAccountsNow). Adds col0Element at column 0. Creates leaderCb+followerLb, adds to tracking lists. If bindAccountsNow: sets ItemsSource=Account.All on both. Calls 4 cluster/button helpers with col0Element. Inserts all into grid. Returns grid. | 2 |

**Parent `BuildRuleRow` after extraction** (CYC = **1**):
```csharp
private Grid BuildRuleRow(string instrumentName)
{
    var instrLabel = new TextBlock
    {
        Text = instrumentName,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(2),
    };
    return BuildRuleRowCore(instrLabel, false);
}
```

**Parent `BuildDynamicRuleRow` after extraction** (CYC = **1**):
```csharp
private Grid BuildDynamicRuleRow()
{
    var instrTextBox = new TextBox
    {
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(2),
        MinWidth = 45,
    };
    return BuildRuleRowCore(instrTextBox, true);
}
```

**`BuildRuleRowCore` body outline**:
```csharp
private Grid BuildRuleRowCore(UIElement col0Element, bool bindAccountsNow)    // CYC=2
{
    var grid = new Grid { Margin = new Thickness(2) };
    BuildGridColumnDefinitions(grid, bindAccountsNow);
    Grid.SetColumn(col0Element, 0); grid.Children.Add(col0Element);
    var leaderCb = new ComboBox { Margin = new Thickness(2) };
    leaderCb.ItemTemplate = BuildAccountDisplayTemplate();
    _leaderBoxes.Add(leaderCb);
    Grid.SetColumn(leaderCb, 1); grid.Children.Add(leaderCb);
    var followerLb = BuildFollowerListBox();
    _followerBoxes.Add(followerLb);
    Grid.SetColumn(followerLb, 2); grid.Children.Add(followerLb);
    if (bindAccountsNow)                                           // +1
    {
        leaderCb.ItemsSource = Account.All;
        followerLb.ItemsSource = Account.All;
    }
    var atmPanel = BuildAtmColumnPanel();
    BuildActionButtons(col0Element, leaderCb, followerLb, atmPanel, grid);
    var beCluster = BuildBeCluster(col0Element);
    Grid.SetColumn(beCluster, 8); grid.Children.Add(beCluster);
    Grid.SetColumn(atmPanel, 9); grid.Children.Add(atmPanel);
    var tightenCluster = BuildTightenCluster(col0Element);
    Grid.SetColumn(tightenCluster, 10); grid.Children.Add(tightenCluster);
    var armBeCluster = BuildArmBeCluster(col0Element, leaderCb);
    Grid.SetColumn(armBeCluster, 11); grid.Children.Add(armBeCluster);
    return grid;
}
```

**Target CCN after**: BuildRuleRow=1, BuildDynamicRuleRow=1, BuildRuleRowCore=2.

**Signals removed**: Code Duplication (L478/L529).

**NT8 Thread Contract**: SAFE -- `BuildRuleRowCore` is pure WPF construction. `Account.All` binding is read-only (setting ItemsSource). Called only from UI-thread `BuildRulesScrollArea` / `OnAddRule`. No Dispatcher needed.

**[Fact] test names**:
- `[Fact] BuildRuleRowCore_Returns12ColumnGrid_Always`
- `[Fact] BuildRuleRowCore_SetsLeaderComboItemsSource_WhenBindAccountsNowTrue`
- `[Fact] BuildRuleRowCore_DoesNotSetLeaderComboItemsSource_WhenBindAccountsNowFalse`
- `[Fact] BuildRuleRowCore_AddsLeaderComboToLeaderBoxesList`

---

## Ticket R16 -- Window: `BuildBeCluster`/`BuildTightenCluster`/`BuildArmBeCluster` Duplication (L620/L653/L686)

**File**: `src/PropTraderTools/TradeCopierWindow.cs`
**Warning**: Code Duplication at L620, L653, L686

### Duplication pattern observed

All 3 cluster-builder methods share an identical structural template:
```csharp
var cluster = new StackPanel { Orientation = Orientation.Horizontal };
var box = new TextBox { Text = "<default>", Width = <W>, VerticalContentAlignment = VerticalAlignment.Center, Margin = ... };
var btn = new Button { Content = "<label>", Margin = ..., Background = WBrushInactive };
var tksLabel = new TextBlock { Text = "tks", VerticalAlignment = VerticalAlignment.Center, Margin = ... };
btn.Tag = <tag-array>;     // ONLY structural difference: tag contents
btn.Click += <handler>;    // ONLY structural difference: handler
<_list>.Add(btn);          // ONLY structural difference: target tracking list
cluster.Children.Add(btn); cluster.Children.Add(box); cluster.Children.Add(tksLabel);
return cluster;
```

The "tks" label, `StackPanel{Horizontal}`, `TextBox`, `Button{Background=WBrushInactive}` are identical across all 3.

### Extraction design

| Helper | Signature | Body | CYC |
|--------|-----------|------|-----|
| `BuildClusterBase` | `private static (StackPanel cluster, Button btn, TextBox box) BuildClusterBase(string btnContent, string boxDefault, int boxWidth)` | Creates StackPanel + TextBox + Button(WBrushInactive) + "tks" TextBlock. Adds all to cluster. Returns tuple. | 1 |

**Post-extraction `BuildBeCluster`** (CYC = **1**):
```csharp
private StackPanel BuildBeCluster(object tag0)
{
    var (cluster, btn, beBox) = BuildClusterBase("[BE]", "2", 28);
    btn.Tag = new object[] { tag0, beBox };
    btn.Click += OnRuleBreakEven;
    _beBtns.Add(btn);
    return cluster;
}
```

**Post-extraction `BuildTightenCluster`** (CYC = **1**):
```csharp
private StackPanel BuildTightenCluster(object tag0)
{
    var (cluster, btn, ticksBox) = BuildClusterBase("[~]", "5", 28);
    btn.Tag = new object[] { tag0, ticksBox };
    btn.Click += OnRuleTightenStop;
    _tightenBtns.Add(btn);
    return cluster;
}
```

**Post-extraction `BuildArmBeCluster`** (CYC = **1**):
```csharp
private StackPanel BuildArmBeCluster(object tag0, ComboBox leaderCb)
{
    var (cluster, btn, armBeBox) = BuildClusterBase("[Arm BE]", "2", 30);
    btn.Tag = new object[] { tag0, leaderCb, armBeBox };
    btn.Click += OnRuleArmBe;
    _armBeBtns.Add(btn);
    return cluster;
}
```

**Target CCN after**: BuildClusterBase=1, all 3 cluster methods=1 each.

**Signals removed**: Code Duplication (L620/L653/L686). Net function count: +1 (BuildClusterBase) -0 (3 helpers kept but simplified) = +1 total new method.

**NT8 Thread Contract**: SAFE -- `BuildClusterBase` is `static`, pure WPF widget construction. No NT8 API. Called from `BuildRuleRow`/`BuildDynamicRuleRow` on UI thread.

**[Fact] test names**:
- `[Fact] BuildClusterBase_ReturnsClusterWithThreeChildren`
- `[Fact] BuildClusterBase_SetsBoxDefaultText_Correctly`
- `[Fact] BuildClusterBase_SetsButtonBackground_ToWBrushInactive`
- `[Fact] BuildClusterBase_SetsBoxWidth_ToSuppliedValue`

---

## Ticket R17 -- Window: `OnRuleBreakEven`/`OnRuleTightenStop` Duplication (L955/L1017)

**File**: `src/PropTraderTools/TradeCopierWindow.cs`
**Warning**: Code Duplication at L955, L1017

### Duplication pattern observed

Both `OnRuleBreakEven` (L955) and `OnRuleTightenStop` (L1017) share the same guard chain:
```csharp
var tag = (sender as Button)?.Tag as object[];
if (tag == null) return;                           // guard 1
string name = tag[0] is TextBox tb ? tb.Text : tag[0] as string;
if (string.IsNullOrEmpty(name)) return;            // guard 2
var instr = FindInstrument(name);
if (instr == null) return;                         // guard 3
// ... then different: parse ticks, call engine method
```

### Extraction design

| Helper | Signature | Body | CYC |
|--------|-----------|------|-----|
| `TryResolveRuleTarget` | `private bool TryResolveRuleTarget(object sender, out object[] tag, out string name, out Instrument instr)` | Null tag guard (+1). ExtractNameFromTag. Null/empty name guard (+1). FindInstrument call. Null instr guard (+1). Return true on success. | 4 |

Note on JS-002: `tag` is `Array.Empty<object>()`, `name` is `string.Empty`, `instr` is `null` on false paths. Callers check bool result and return immediately on `false` -- the `null` out for instr is never dereferenced.

**Post-extraction `OnRuleBreakEven`** (CYC = **2**):
```csharp
if (!TryResolveRuleTarget(sender, out var tag, out _, out var instr)) return;  // +1
int ticks = TryParseBeTicksFromTag(tag);
_engine.BreakEven(instr, ticks);                                               // +1
```

**Post-extraction `OnRuleTightenStop`** (CYC = **2**):
```csharp
if (!TryResolveRuleTarget(sender, out var tag, out _, out var instr)) return;  // +1
int ticks = TryParseTightenTicksFromTag(tag);
_engine.TightenStop(instr, ticks);                                             // +1
```

**Target CCN after**: TryResolveRuleTarget=4, both event handlers=2.

**Signals removed**: Code Duplication (L955/L1017).

**NT8 Thread Contract**: CONSTRAINED -- `TryResolveRuleTarget` calls `FindInstrument(name)` which calls `Instrument.GetInstrument(name)`. This MUST only be called from the UI thread (WPF Button Click handlers). Comment required: `// MUST only be called on UI thread (calls Instrument.GetInstrument via FindInstrument)`.

**[Fact] test names**:
- `[Fact] TryResolveRuleTarget_ReturnsFalse_WhenSenderTagIsNull`
- `[Fact] TryResolveRuleTarget_ReturnsFalse_WhenExtractedNameIsEmpty`
- `[Fact] TryResolveRuleTarget_ReturnsFalse_WhenInstrumentNotFound`
- `[Fact] TryResolveRuleTarget_ReturnsTrue_WhenTagNameAndInstrumentAllPresent`

---

## Ticket R18 -- Window: `BuildAtmMapFromTag` Complex Method + Complex Conditionals (L1056/L1062/L1065)

**File**: `src/PropTraderTools/TradeCopierWindow.cs`
**Warning**: Complex Method (cc=9) at L1056, Complex Conditional at L1062, Complex Conditional at L1065

### Code observed (actual)

```csharp
private static Dictionary<string, FollowerAtmMode> BuildAtmMapFromTag(
    object[] tag, List<Account> followers)
{
    var atmMap = new Dictionary<string, FollowerAtmMode>();
    if (tag.Length > 3 && tag[3] is ComboBox atmCb && atmCb.SelectedItem is string atmSel)  // L1062: compound (3 conditions)
    {
        string atmMode = atmSel;
        if (atmMode == "Named"
            && tag.Length > 4
            && tag[4] is TextBox namedBox                           // L1065: compound (4 conditions)
            && namedBox.Text.Length > 0)
            atmMode = "Named:" + namedBox.Text;
        var mode = CopyEngine.ParseAtmModeName(atmMode);
        foreach (var acc in followers)
            atmMap[acc.Name] = mode;
    }
    return atmMap;
}
```

CodeScene reports cc=9 and two Complex Conditional flags because of the two compound boolean expressions.

### Extraction design

| Helper | Signature | Body | CYC |
|--------|-----------|------|-----|
| `TryGetAtmSelection` | `private static bool TryGetAtmSelection(object[] tag, out string atmSel)` | `if (tag.Length > 3 && tag[3] is ComboBox atmCb && atmCb.SelectedItem is string sel) { atmSel = sel; return true; } atmSel = string.Empty; return false;` | 4 |
| `IsNamedAtmMode` | `private static bool IsNamedAtmMode(string atmMode, object[] tag, out TextBox namedBox)` | `namedBox = null; if (atmMode != "Named") return false; if (tag.Length <= 4) return false; namedBox = tag[4] as TextBox; return namedBox != null;` | 4 |

**Post-extraction `BuildAtmMapFromTag`** (CYC = **5**):
```csharp
private static Dictionary<string, FollowerAtmMode> BuildAtmMapFromTag(
    object[] tag, List<Account> followers)
{
    var atmMap = new Dictionary<string, FollowerAtmMode>();
    if (!TryGetAtmSelection(tag, out string atmSel)) return atmMap;   // +1
    string atmMode = atmSel;
    if (IsNamedAtmMode(atmMode, tag, out var namedBox)                // +1
        && namedBox.Text.Length > 0)                                  // +1
        atmMode = "Named:" + namedBox.Text;
    var mode = CopyEngine.ParseAtmModeName(atmMode);
    foreach (var acc in followers)                                    // +1
        atmMap[acc.Name] = mode;
    return atmMap;
    // Total: base(1) + 4 = 5. Complex Conditionals eliminated.
}
```

Note on `IsNamedAtmMode` CYC: 3 guards (atmMode!="Named", length<=4, as-cast null-check). CYC = 1+3 = 4. ✓ <=4.
Note on `TryGetAtmSelection` CYC: 1 compound with 3 `&&` conditions = 1+3 = 4. ✓ <=4.

**Target CCN after**: BuildAtmMapFromTag=5, TryGetAtmSelection=4, IsNamedAtmMode=4. All <=8 (parent), <=4 (helpers). Complex Method cc=9 → 5; both Complex Conditional signals removed.

**Signals removed**: Complex Method cc=9 (L1056), Complex Conditional x2 (L1062/L1065).

**NT8 Thread Contract**: SAFE -- all pure `object[]` tag parsing. No NT8 API. `CopyEngine.ParseAtmModeName` is a static string parser. Called from `OnRowApply` (UI thread button Click handler).

**[Fact] test names**:
- `[Fact] TryGetAtmSelection_ReturnsFalse_WhenTagTooShort`
- `[Fact] TryGetAtmSelection_ReturnsFalse_WhenTag3IsNotComboBox`
- `[Fact] TryGetAtmSelection_ReturnsTrue_WhenComboBoxHasStringSelection`
- `[Fact] IsNamedAtmMode_ReturnsFalse_WhenModeIsNotNamed`
- `[Fact] IsNamedAtmMode_ReturnsFalse_WhenTag4IsNotTextBox`
- `[Fact] IsNamedAtmMode_ReturnsTrue_WhenModeIsNamedAndTag4IsTextBox`

---

## Execution Order

Execute Panel tickets first (largest gap), then Window.

| # | Ticket | File | Signals Removed | Est. Panel Gain | Est. Window Gain |
|---|--------|------|-----------------|-----------------|------------------|
| 1 | R10 | Panel | Complex Method cc=10, Bumpy Road x2 | +0.20 | -- |
| 2 | R11 | Panel | Code Duplication x6 (cluster), Number of Functions -6 | +0.30 | -- |
| 3 | R12 | Panel | Code Duplication x2 (L1921/L1944) | +0.10 | -- |
| 4 | R13 | Panel | Code Duplication x2 (L2343/L2835) | +0.10 | -- |
| 5 | R14 | Panel | Bumpy Road x1 (L2493) | +0.05 | -- |
| 6 | R15 | Window | Code Duplication x2 (L478/L529) | -- | +0.15 |
| 7 | R16 | Window | Code Duplication x3 (L620/L653/L686) | -- | +0.22 |
| 8 | R17 | Window | Code Duplication x2 (L955/L1017) | -- | +0.12 |
| 9 | R18 | Window | Complex Method cc=9, Complex Conditional x2 | -- | +0.12 |

### Estimated Scores After All Tickets

**Panel**: 6.08 + 0.20 + 0.30 + 0.10 + 0.10 + 0.05 = **~6.83** (conservative)
  Optimistic (CodeScene cluster effect): **~7.10**
  Note: R11 removes a 6-method clone cluster AND reduces function count -- double signal. Likely higher than +0.30 linear estimate.

**Window**: 7.43 + 0.15 + 0.22 + 0.12 + 0.12 = **~8.04** ✓ Exceeds target 8.0.

**Risk**: Panel may land in 6.8-7.1 range. If Panel < 7.0 after R10-R14, the GetAsk/GetBid extraction (prev-skipped) should be reconsidered as a follow-up: `private double GetMarketPrice(Func<NinjaTrader.Cbi.MarketDataEventArgs> selector)` -- though this allocates. The safer follow-up is `private NinjaTrader.Cbi.MarketDataEventArgs TryGetMarketData()` (shared null-guard chain) + `GetAsk`/`GetBid` call it. Revisit only if Panel < 7.0 after R10-R14.

---

## 7-Scan Checklist (SCAN-01 through SCAN-07)

Run these checks after every ticket before committing.

```powershell
# SCAN-01: No lock() in new or modified methods
Select-String "lock\(" src/PropTraderTools/TradeCopierPanel.cs
Select-String "lock\(" src/PropTraderTools/TradeCopierWindow.cs
# Expected: 0 results in new/modified methods

# SCAN-02: No async void (non-event-handler)
Select-String "async void " src/PropTraderTools/TradeCopierPanel.cs
Select-String "async void " src/PropTraderTools/TradeCopierWindow.cs
# Expected: 0 results in new helpers

# SCAN-03: No return null in non-sentinel helpers
# Check: TryResolveRuleTarget, TryGetAtmSelection, IsNamedAtmMode, BuildRuleRowCore
# all return bool or Grid -- never null Grid returned.
Select-String "return null" src/PropTraderTools/TradeCopierPanel.cs
Select-String "return null" src/PropTraderTools/TradeCopierWindow.cs
# Expected: existing occurrences only, NO new ones in R10-R18 helpers

# SCAN-04: ASCII-only identifiers and string literals
$content = Get-Content src/PropTraderTools/TradeCopierPanel.cs -Raw
$content | Select-String "[^\x00-\x7F]"
$content = Get-Content src/PropTraderTools/TradeCopierWindow.cs -Raw
$content | Select-String "[^\x00-\x7F]"
# Expected: 0 new non-ASCII characters (existing Unicode arrows in RepeatButton content are pre-existing)

# SCAN-05: CYC <= 8 (parent), <= 4 (helpers)
lizard src/PropTraderTools/TradeCopierPanel.cs --CCN 8
lizard src/PropTraderTools/TradeCopierWindow.cs --CCN 8
# Expected: 0 warnings on modified methods
# For helper CYC <= 4:
lizard src/PropTraderTools/TradeCopierPanel.cs --CCN 4 | Select-String "UnsubscribeFollowerItems|DisarmAllAccounts|LogQxTwoTarget|IsAtmComboAlreadyTracked"
lizard src/PropTraderTools/TradeCopierWindow.cs --CCN 4 | Select-String "BuildRuleRowCore|BuildClusterBase|TryResolveRuleTarget|TryGetAtmSelection|IsNamedAtmMode"
# Expected: 0 warnings

# SCAN-06: No new public or internal methods
Select-String "public|internal" src/PropTraderTools/TradeCopierPanel.cs | Select-String "UnsubscribeFollowerItems|DisarmAllAccounts|LogQxTwoTarget|IsAtmComboAlreadyTracked"
Select-String "public|internal" src/PropTraderTools/TradeCopierWindow.cs | Select-String "BuildRuleRowCore|BuildClusterBase|TryResolveRuleTarget|TryGetAtmSelection|IsNamedAtmMode"
# Expected: 0 results

# SCAN-07: Build + test
dotnet build src/PropTraderTools/ --configuration Release
# Expected: 0 errors, 0 warnings
dotnet test src/PropTraderTools/Tests/ --no-build --configuration Release
# Expected: all tests pass, 0 new failures
```

---

## Mandatory Verification Gates (per ticket)

After each ticket, before moving to next:
1. `dotnet build` -- 0 errors, 0 warnings
2. `dotnet test` -- all tests pass, 0 new failures
3. `lizard src/PropTraderTools/<File>.cs --CCN 8` -- 0 warnings on modified methods
4. CodeScene delta check: `$env:CS_ACCESS_TOKEN="pat_eyJpZCI6ODg1OTIsInJhbmQiOiJXcXBwWEU4ZUdvcHJDTnNXWUdaM2FFbmtVUzJ6UjV4QSJ9"; cs delta` -- score does NOT decrease on modified file

---

**Build Tag**: PTT-COPIER BWAVE-CYC Lane-C Remediation R10+ | 2025-08-11
**Architect**: ptt-architect
**Status**: READY FOR ptt-engineer (execute R10 -> R11 -> R12 -> R13 -> R14 -> R15 -> R16 -> R17 -> R18)
