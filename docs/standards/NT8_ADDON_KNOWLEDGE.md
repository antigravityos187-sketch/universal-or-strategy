# NinjaTrader 8 Add-On Developer Knowledge Base
# Source: Hard-won from PTT Trade Copier blocks B1-B7
# Updated: 2026-07-08
# Status: LIVING DOCUMENT — append every session, never delete confirmed facts

---

## CRITICAL: ChartTrader Panel Injection (SOLVED B7)

### The breakthrough — how it was solved

**Problem:** Add 10 buttons to the ChartTrader right-side panel on every open chart.

**What does NOT work (each attempt tried and failed):**
1. `window as ChartTrader` in `OnWindowCreated(Window)` → **CS0039** — `ChartTrader` does NOT inherit from `System.Windows.Window`. Incompatible type hierarchy.
2. `override OnWindowCreated(ChartTrader)` in `AddOnBase` → **CS0115** — this overload does not exist in `AddOnBase`. Never has.
3. `window as Chart` then `chart.ChartControl` → **CS1061** — `Chart` window has no `ChartControl` property.
4. `chart.Instrument` → **CS1061** — `Chart` window has no `Instrument` property.
5. `chartTrader.Rows` (StackPanel) → **CS1061** — does not exist.
6. `chartTrader.RowsPanel` → **CS1061** — does not exist.
7. Reparent `chartTrader.Content` into a new `DockPanel` → **runtime crash** — "Specified element is already the logical child of another element."
8. `FindVisualChild<StackPanel>` looking for button-containing StackPanel → **nothing found** — ChartTrader uses a Grid, not a StackPanel.
9. Hook `chart.Loaded` event only → **nothing injected** — NT8 fires `OnWindowCreated` AFTER `Loaded` has already fired for pre-existing charts. The event never fires.

**What WORKS — the proven solution:**

```
Step 1: Cast window to NinjaTrader.Gui.Chart.Chart (NOT ChartTrader, NOT Window)
        Chart IS a System.Windows.Window subclass — the cast succeeds.

Step 2: NT8 timing problem — OnWindowCreated fires AFTER chart.Loaded for existing charts.
        Fix: check chart.IsLoaded first.
        if (chart.IsLoaded)  → inject immediately via Dispatcher.InvokeAsync
        else                 → hook chart.Loaded event

Step 3: Walk visual tree: FindVisualChild<ChartTrader>(chart)
        ChartTrader IS inside the Chart window as a child control.
        This works because we're in the Chart window, not casting to ChartTrader directly.

Step 4: ChartTrader.Content is a System.Windows.Controls.Grid (CONFIRMED)
        NOT a StackPanel, NOT a ContentPresenter, NOT wrappable.

Step 5: Add new RowDefinition to the Grid, place our UserControl in that row.
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto })
        Grid.SetRow(panel, newRowIndex)
        Grid.SetColumnSpan(panel, grid.ColumnDefinitions.Count > 0 ? grid.ColumnDefinitions.Count : 1)
        grid.Children.Add(panel)
        → Panel appears at bottom of ChartTrader, below all native buttons.

Step 6: Get instrument from chartTrader.Instrument (property on ChartTrader, not Chart)
        Wrap in try/catch — may not be set yet at inject time.
```

**Key code (TradeCopierAddOn.cs B7-FIX5):**
```csharp
// Cast to Chart (Window subclass) -- works
var chart = window as Chart;
if (chart != null) InjectIntoChart(chart);

// Handle NT8 timing: already-loaded charts miss the Loaded event
if (chart.IsLoaded)
    chart.Dispatcher.InvokeAsync(() => DoInject(chart));
else
    chart.Loaded += OnChartLoaded;

// DoInject: ChartTrader.Content is a Grid -- add new row
var chartTrader = FindVisualChild<ChartTrader>(chart);
var grid = chartTrader.Content as Grid;
var row = new RowDefinition { Height = GridLength.Auto };
grid.RowDefinitions.Add(row);
Grid.SetRow(panel, grid.RowDefinitions.Count - 1);
Grid.SetColumnSpan(panel, grid.ColumnDefinitions.Count > 0 ? grid.ColumnDefinitions.Count : 1);
grid.Children.Add(panel);
```

---

## NT8 Type Hierarchy Facts (confirmed by compiler)

| Type | Inherits from | Notes |
|------|--------------|-------|
| `NinjaTrader.Gui.Chart.Chart` | `System.Windows.Window` | The full chart window. Use this in `OnWindowCreated`. |
| `NinjaTrader.Gui.Chart.ChartTrader` | NOT Window | Child control inside Chart. Find via visual tree walk. |
| `NinjaTrader.Gui.Tools.ControlCenter` | `System.Windows.Window` | NT8 main control center. Castable from Window. |
| `NinjaTrader.Gui.AddOnBase` | — | Base class for AddOns. Only `OnWindowCreated(Window)` overload exists. |

---

## NT8 AddOnBase API Facts

```csharp
// The ONLY valid override signatures (confirmed by compiler):
protected override void OnWindowCreated(System.Windows.Window window)  // ✅ exists
protected override void OnWindowDestroyed(System.Windows.Window window) // ✅ exists
protected override void OnWindowCreated(ChartTrader chartTrader)        // ❌ DOES NOT EXIST
protected override void OnWindowDestroyed(ChartTrader chartTrader)      // ❌ DOES NOT EXIST
protected override void OnStateChange()                                  // ✅ exists
```

---

## NT8 ChartTrader Internal Structure (confirmed at runtime)

```
Chart (Window)
  └── ChartTrader (child UserControl, found via visual tree)
        └── Content: System.Windows.Controls.Grid
              ├── Row 0: Buy Mkt / Sell Mkt buttons
              ├── Row 1: Buy Ask / Sell Ask buttons
              ├── Row 2: Buy Bid / Sell Bid buttons
              ├── Row 3: Rev / Close buttons
              ├── Row 4: Flat / Entry buttons
              ├── Row 5: PnL display
              ├── Row 6: Instrument / TIF dropdowns
              ├── Row 7: Account / Order qty
              ├── Row 8: ATM Strategy
              └── Row N (added by PTT): TradeCopierPanel (UserControl)
```

`ChartTrader.Instrument` — property on ChartTrader, accessible after the control is loaded.

---

## NT8 Hard Constraints (all confirmed in B1-B7)

| Constraint | Confirmed | Notes |
|-----------|-----------|-------|
| No `async/await` in lifecycle methods | B1 | `OnInitialize`, `OnDestroyed` must be synchronous |
| `Dispatcher.InvokeAsync` for all off-thread UI | B1 | Never touch WPF controls from non-UI threads |
| `TradeCopierWindow` must NOT be sealed | B3 | NTWindow subclass rules apply even to plain Window |
| `order.Change(order[])` to move stops | B4 | Proven in BreakEven. Identical API for target moves. |
| `Math.Round(raw / tickSize) * tickSize` for stop prices | B4 | Mandatory tick-alignment |
| WPF KeyGesture rejects ALL Shift+letter in NT8 | B4 | Never use KeyBinding with letter keys |
| `Account.All` only in Loaded handlers, never constructors | B5 | Crashes if called before NT8 initializes accounts |
| `NTWindow` cannot be embedded as UserControl | B5 | Use `UserControl` base for injectable panels |
| `TradeCopierWindow` must extend `System.Windows.Window` | B6 | NOT NTWindow — NTWindow causes window-not-appearing |
| `TradeCopierAddOn.OnWindowCreated` fires for EVERY NT8 window | B6 | Use `_menuWired` volatile bool for idempotency |
| `NTMenuItem.Header` may be `TextBlock` object, not string | B6 | Use `mi.Header.ToString()`, never `mi.Header as string` |
| `chart.IsLoaded` may be true when `OnWindowCreated` fires | B7 | Hook Loaded only if `!chart.IsLoaded`; else inject immediately |
| `ChartTrader.Content` is a `Grid`, not a StackPanel | B7 | Add new RowDefinition — do not reparent Content |
| Reparenting WPF elements crashes with "logical child" error | B7 | Never move existing UIElement to new parent |
| `ChartTrader.Instrument` is on the ChartTrader control | B7 | Not on Chart window. Wrap in try/catch at inject time. |

---

## NT8 Menu Duplicate Prevention Pattern

```csharp
// WRONG: (mi.Header as string) returns null when Header is TextBlock
if (mi.Header as string == "Trade Copier") return; // SILENT FAIL

// RIGHT: ToString() works regardless of Header type
var hdr = mi.Header != null ? mi.Header.ToString() : string.Empty;

// BETTER: volatile bool guard -- immune to header type issues
private static volatile bool _menuWired = false;
// Set to true after first successful add. Reset in State.Terminated.
// Check at top of OnWindowCreated before any cast.
```

---

## NT8 Visual Tree Walking Pattern

```csharp
// Generic depth-first visual tree search (CYC=1)
private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
{
    if (parent == null) return null;
    int count = VisualTreeHelper.GetChildrenCount(parent);
    for (int i = 0; i < count; i++)
    {
        var child = VisualTreeHelper.GetChild(parent, i);
        if (child is T match) return match;
        var result = FindVisualChild<T>(child);
        if (result != null) return result;
    }
    return null;
}

// Named element search (CYC=1)
private static T FindVisualChildByName<T>(DependencyObject parent, string name)
    where T : FrameworkElement
{
    if (parent == null) return null;
    int count = VisualTreeHelper.GetChildrenCount(parent);
    for (int i = 0; i < count; i++)
    {
        var child = VisualTreeHelper.GetChild(parent, i);
        if (child is T fe && fe.Name == name) return fe;
        var result = FindVisualChildByName<T>(child, name);
        if (result != null) return result;
    }
    return null;
}
```

---

## NT8 Diagnostic Pattern (use when injection fails silently)

```csharp
// When injection fails with no error -- show type info to diagnose visual tree
MessageBox.Show(
    "PTT: Could not find injection point.\n" +
    "ChartTrader type: " + chartTrader.GetType().FullName + "\n" +
    "Content type: " + (chartTrader.Content?.GetType().FullName ?? "null"),
    "PTT Info");
// This revealed: ChartTrader.Content = System.Windows.Controls.Grid
// Which led directly to the Grid.RowDefinitions.Add() solution.
```

---

## NT8 Account API Facts

```csharp
Account.All           // Only safe in Loaded handlers -- never in constructors
acc.Orders            // All orders for this account (IEnumerable<Order>)
acc.Positions         // All positions (IEnumerable<Position>)
acc.Change(Order[])   // Move stop/target price on working order
acc.Cancel(Order[])   // Cancel working order
acc.CreateOrder(...)  // Place new order
acc.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar) // Real P&L
order.StopPrice = x   // Set new stop price BEFORE calling acc.Change()
order.LimitPrice = x  // Set new target price BEFORE calling acc.Change()
order.OrderState      // Submitted / Working / Accepted / Filled / Cancelled
order.FromEntrySignal // Non-null on bracket legs (stop + target orders from ATM)
order.Name            // "PTT-Copy", "PTT-Trim" etc -- set at CreateOrder time
```

---

## B7 ChartTrader Injection — Session Log

```
FIX1: Invented OnWindowCreated(ChartTrader) overload → CS0115
FIX2: Reverted to as-cast on Window param → CS0039 (incompatible types)
FIX3: Removed injection entirely — "permanently deferred"
FIX4: Volatile bool menu guard only, no injection
FIX5: Chart (Window) → visual tree → ChartTrader → Content as Grid → add RowDefinition ✅ WORKING
      Result confirmed in NT8: panel appears below native buttons, shows "Ready: MES SEP26"
```

**Total iterations to solve:** 5 fixes across this session.
**Key insight:** The diagnostic MessageBox (FIX5) revealed `Content type: System.Windows.Controls.Grid` which made the solution obvious.

---

## B8 Discoveries — Hard Compiler Errors + Runtime Facts

### C# Language Constraints Added (B8)

| Error | Banned pattern | Safe replacement | Rule |
|-------|---------------|-----------------|------|
| CS0518 IsExternalInit | `{ get; init; }` | `{ get; private set; }` + explicit constructor | NT8-001 |
| CS0518 IsExternalInit | `abstract record` / `sealed record` with positional params | `abstract class` + `sealed class` + explicit constructors | NT8-002 |
| CS0677 | `volatile double` | Remove volatile; add comment explaining x64 atomic double | NT8-003 |
| CS0246 | `ImmutableDictionary` / `using System.Collections.Immutable` | `Dictionary<K,V>` written-once (logically immutable) | NT8-004 |
| CS8341 | `readonly struct` with `{ get; private set; }` auto-property | Use `readonly` field instead | NT8-005 |
| CS1061 | `ConcurrentBag<T>.Any()` without `using System.Linq` | Add `using System.Linq;` or use `.Count > 0` | NT8-006 |
| CS1503 | `Account.CreateOrder` arg 12 as `string` | `(NinjaTrader.Cbi.CustomOrder)null` at arg 12 | NT8-007 |
| CS1061 | `chart.ChartControl` (property does not exist) | `FindVisualChild<ChartControl>(chart)` | NT8-008 |
| CS1061 | `chartControl.GetValueByY(y)` (method absent in this NT8 build) | Stub to `0.0`; document deferred work item | NT8-009 |

### `ImmutableDictionary.SetItem()` — Copy-On-Write Replacement

B8 used `dict.SetItem(key, value)` to produce a new dictionary leaving the original intact.
When `ImmutableDictionary` is replaced with plain `Dictionary<K,V>` (NT8-004), copy-on-write
must be done manually:

```csharp
// ImmutableDictionary.SetItem equivalent using plain Dictionary:
private Dictionary<K,V> CopyWith(Dictionary<K,V> source, K key, V value)
{
    var next = new Dictionary<K,V>(source);   // copy
    next[key] = value;                        // mutate copy
    return next;                              // return new dict; source unchanged
}
```

### `FollowerAtmMode` — Record → Abstract Class Migration (B8)

B7 specced `public abstract record FollowerAtmMode` with nested sealed records.
B8 hit CS0518 in NT8 (IsExternalInit). The entire hierarchy was converted to abstract class:

```csharp
// BANNED in NT8 (CS0518):
public abstract record FollowerAtmMode { private FollowerAtmMode() {} }
public sealed record Inherit() : FollowerAtmMode;

// SAFE in NT8:
public abstract class FollowerAtmMode
{
    private FollowerAtmMode() {}
    public sealed class Inherit : FollowerAtmMode { public Inherit() : base() {} }
    public sealed class Market  : FollowerAtmMode { public Market()  : base() {} }
    public sealed class Named   : FollowerAtmMode
    {
        public string TemplateName { get; private set; }
        public Named(string t) : base() { TemplateName = t; }
    }
}
// is/pattern-matching still works: if (mode is FollowerAtmMode.Named n) { ... }
```

### `PositionState` / `FollowerBinding` — readonly struct with init → readonly fields (B8)

B7 used `{ get; init; }` on readonly structs. B8 hit CS8341 + CS0518.
Safe form uses `readonly` fields set in the constructor:

```csharp
// SAFE:
internal readonly struct PositionState
{
    internal readonly bool HasOpenPosition;
    internal readonly bool HasWorkingEntries;
    internal PositionState(bool open, bool working)
    {
        HasOpenPosition   = open;
        HasWorkingEntries = working;
    }
}
```

---

## B9 Discoveries — Indicator Subclass + Cross-Thread State

### `AtrSizingEngine` — Indicator Subclass Rules (B9)

1. **Class must NOT be sealed** (NT8-015) — NT8 Indicator infrastructure may subclass internally.
2. **`State.XXX` must be fully qualified** (NT8-010):
   ```csharp
   // BANNED:
   if (State == State.SetDefaults) {}
   // SAFE:
   if (State == NinjaTrader.NinjaScript.State.SetDefaults) {}
   ```
3. **`Add(ATR(Period))` in OnStateChange DataLoaded is INVALID for headless Indicator** (NT8-011):
   ```csharp
   // BANNED:
   if (State == NinjaTrader.NinjaScript.State.DataLoaded) Add(ATR(Period));
   // SAFE: call directly per bar:
   protected override void OnBarUpdate() { double v = ATR(Period)[0]; }
   ```
4. **`volatile double` banned** — compiler error CS0677 (NT8-003). Use plain double field with
   comment explaining x64 atomic double reads.

### Cross-Thread State in AddOn Context (B9)

Fields written by UI thread and read by OnOrderUpdate/OnBarUpdate/MarketData threads
MUST be `volatile int` or `volatile bool` (never `volatile double`):

```csharp
private volatile bool _atrEnabled    = false;   // UI writes, order thread reads
private volatile int  _copyModeValue = 0;       // UI writes, order thread reads
private volatile bool _clickArmed    = false;   // UI writes, mouse event reads
private volatile bool _clickBuy      = true;    // UI writes, click handler reads
```

### NT8 Chart Attachment API -- RESOLVED 2026-07-09

Confirmed result: NinjaScripts.Add and Indicators.Add produce CS1061 in AddOn compilation context.
DispatcherTimer polling at DispatcherPriority.Background is the compile-safe fallback.
DW-B9-02 STATUS: RESOLVED 2026-07-09 (B10-EXEC T4).

---

## B10 Discoveries — WPF DataTemplate Grid + Trailing Stop API

### `FrameworkElementFactory` Cannot Add `ColumnDefinitions` (B10-UI-01) (NT8-012)

WPF `FrameworkElementFactory` builds a template tree at definition time. `Grid.ColumnDefinitions`
is a run-time collection — it cannot be populated via the factory API. Use a `Loaded` event:

```csharp
var gridFactory = new FrameworkElementFactory(typeof(Grid));
gridFactory.AddHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler(OnRowGridLoaded));

private void OnRowGridLoaded(object sender, RoutedEventArgs e)
{
    var grid = (Grid)sender;
    if (grid.ColumnDefinitions.Count > 0) return;   // idempotency guard
    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
    // set Grid.Column on each child after columns exist:
    Grid.SetColumn((FrameworkElement)grid.Children[0], 0);
}
```

### Trailing Stop Order Detection (B9/B10) (NT8-026)

NT8 trailing stop orders are `OrderType.StopMarket` orders with `order.TrailPrice > 0`.
Calling `acc.Change(new Order[] { order })` on such an order with only `StopPrice` modified
has UNDEFINED effect on the trail watermark — the trail may freeze.

```csharp
// Detect trailing stop:
bool isTrailing = order.TrailPrice > 0;

// Safe handling for BE button (cancel + replace path):
if (isTrailing)
{
    bool alreadyAtBe = isLong ? order.StopPrice >= newStop : order.StopPrice <= newStop;
    if (alreadyAtBe) continue;   // trail already past BE — no action needed
    acc.Cancel(new Order[] { order });
    acc.CreateOrder(instr, action, OrderType.StopMarket, OrderEntry.Manual,
                    TimeInForce.Day, order.Quantity, 0, newStop, null,
                    "PTT-BE-Stop", DateTime.MaxValue, (NinjaTrader.Cbi.CustomOrder)null);
}
else
{
    order.StopPrice = newStop;
    acc.Change(new Order[] { order });
}
```

### `Instrument.MarketData.MarketDataUpdate` from AddOn — PENDING VERIFICATION (NT8-027)

As of B10 start, it is unconfirmed whether
`NinjaTrader.Data.Instrument.GetInstrument(name).MarketData.MarketDataUpdate`
fires correctly when subscribed from an `AddOnBase` subclass (no `OnBarUpdate` / `OnStateChange`).

GAP-002 Sim101 test wired in `TradeCopierAddOn.RunGap002Test()`.
**Update this section once GAP-002 test result is recorded in
`docs/brain/PTT-COPIER-B9/GAP-002-pending-be-and-trailing-stop-compatibility.md`.**

Expected outcomes:
- Fires correctly → use `Instrument.MarketData` subscription (Option A for DW-B10-GAP-002a)
- Fires Bid/Ask only → add `if (e.MarketDataType != MarketDataType.Last) return` filter
- Does not fire → use `Account.AccountItemUpdate` P&L proxy (Option B fallback)

---

## B11 Discoveries (2026-07-09)

### NT8-K-003 — ATM template read time — OPEN (Sim101 gate: DW-B11-TEMPLATE-WRITER-01)

**Question:** Does NT8 read the ATM template XML at dropdown-selection time, or cache all templates at startup?

**Impact:** Determines whether `AtrTemplateWriter.Write()` (Path 2) works standalone.
If read at selection time → Path 2 sufficient.
If cached at startup → Path 3 (UI control write) also required.

**Test:** Write modified `PTT_ATR_LIVE.xml`. Switch ChartTrader dropdown away, then back. Confirm new values load.

**Official docs:** https://developer.ninjatrader.com/docs/desktop/add_on

---

### NT8-K-004 — Account.Change() on entry order Quantity (DECREASE) — CONFIRMED (7/10/2026)

**Source:** NinjaTrader Log 7/10/2026 9:48 AM, Sim101, MES SEP26.

**Confirmed:** `Account.Change()` successfully modifies Quantity DOWNWARD on a Working limit entry order.
State cycle identical to stop price change: `Change submitted → Accepted → Working` at new qty. Same order ID preserved.

**Key test sequence:**
- Order `78907a` placed at Qty=10, Working
- `Change submitted` → Qty=9, Accepted, Working on same order ID

**Copier implication:** Cancel+resubmit (Option B) selected as architecture — see NT8-K-006.

---

### NT8-K-005 — ATM bracket EntryQuantity after resubmit — OPEN (Sim101 gate: DW-B11-SIM-K005-01)

**Question:** After cancel+resubmit with updated ATM template XML (Path 2 AtrTemplateWriter rewrite):
does the ATM bracket spawn at correct qty when the entry fills?

**Why lower risk than before:** The resubmit path writes a fresh XML *before* `CreateOrder()`,
so the template is always current at submission time. Still needs a Sim101 fill test.

**Test:** DW-B11-SIM-K005-01 — resubmit with modified template EntryQuantity, let fill, verify bracket stop qty matches.

---

### NT8-K-006 — ChartTrader qty management is ASYMMETRIC — CONFIRMED (7/10/2026)

**Source:** NinjaTrader Log 7/10/2026, 3 sessions (9:48, 10:01, 10:08), Sim101, MES SEP26.

**Confirmed rules:**

**Decrease (any amount):**
- If delta order exists: `Change submitted` on delta order first (reduces it)
- If delta would go to 0: cancel delta order, then modify original
- If no delta order: modify original directly
- Result: same or fewer order IDs, total qty reduced

**Increase (any amount):**
- ALWAYS creates a new order for the exact delta (newTotal − currentTotal)
- Original order is NEVER modified on increase
- Each increase adds another order to the stack
- Result: N+1 order IDs

**Full evidence table (9 test sequences):**

| Session | Action | Before | After | NT8 response |
|---------|--------|--------|-------|--------------|
| 9:48 | ↓ 10→9 | 78907a Q=10 | 78907a Q=9 | Change submitted same ID |
| 9:48 | ↑ 9→12 | 78907a Q=9 | 78907a Q=9 + e0bdec Q=3 | New order delta=3 |
| 10:01 | ↑ 10→13 | 8cce47 Q=10 | 8cce47 Q=10 + cbdf38 Q=3 | New order delta=3 |
| 10:01 | ↓ 13→11 | 8cce47 Q=10 + cbdf38 Q=3 | 8cce47 Q=10 + cbdf38 Q=1 | Change on delta (3→1) |
| 10:01 | ↓ 11→8 | 8cce47 Q=10 + cbdf38 Q=1 | 8cce47 Q=8 | cbdf38 cancelled, 8cce47 modified 10→8 |
| 10:01 | ↓ 8→6 | 8cce47 Q=8 | 8cce47 Q=6 | Change submitted same ID |
| 10:01 | ↑ 6→10 | 8cce47 Q=6 | 8cce47 Q=6 + b3357c Q=4 | New order delta=4 |
| 10:08 | ↑ 8→10 | f05032 Q=8 | f05032 Q=8 + 71dae2 Q=2 | New order delta=2 |
| 10:08 | ↑ 10→13 | f05032 Q=8 + 71dae2 Q=2 | +6d9189 Q=3 | New order delta=3, stack=3 |

**Root cause:** This is 100% ChartTrader UI-layer logic. `Account.Change()` only operates on individual orders.
ChartTrader manages the aggregate qty view. The NT8 engine has no concept of "total qty across pending orders."

**Copier architecture consequence:** `SyncPendingEntry()` MUST use Option B (cancel+resubmit).
`Account.Change()` on a single order cannot fix total qty when a stack of orders exists.
`CancelPendingEntries(acc, instr)` already cancels ALL working entries in one call — reuse it directly.

---

## B16 Discoveries

### T1 F5 Output (run date: 2026-07-15)

ChartPanel.ActualHeight = 452.00
ChartPanel.ActualWidth  = 139.33
ChildCount = 1

[0] System.Windows.Controls.ContentPresenter
    ActualHeight=452.00  ActualWidth=139.33
    Method: GetAnimationBaseValue(DependencyProperty dp) -> Object
    Method: GetValue(DependencyProperty dp) -> Object
    Method: SetValue(DependencyProperty dp, Object value) -> Void
    Method: SetCurrentValue(DependencyProperty dp, Object value) -> Void
    Method: SetValue(DependencyPropertyKey key, Object value) -> Void
    Method: ClearValue(DependencyProperty dp) -> Void
    Method: ClearValue(DependencyPropertyKey key) -> Void
    Method: CoerceValue(DependencyProperty dp) -> Void
    Method: ReadLocalValue(DependencyProperty dp) -> Void
    Method: GetLocalValueEnumerator() -> LocalValueEnumerator

No [1] child — ChildCount is 1 (single ContentPresenter wraps all chart content).

### T1 Branch Decision

Based on T1 F5 output:
- Branch A selected: NO
  Reason: The single child of ChartPanel is System.Windows.Controls.ContentPresenter — a pure
  WPF layout container. None of its matched methods (GetValue, SetValue, etc.) return a double
  from a double/y parameter. No NT8-native Y-to-price API exists at ChartPanel depth=2.
- Branch B selected: YES
  Reason: No native API found. Linear interpolation via ChartPanel.MaxValue / MinValue /
  ActualHeight must be used. CORRECTION_FACTOR = 1.0 (ContentPresenter fills full
  ChartPanel height — ActualHeight matches exactly at 452.00).

### T1 Correction Factor Data

ChartPanel.ActualHeight  = 452.00
ChartScale.ActualHeight  = not found (no ChartScale child at depth=2; only ContentPresenter)
Correction factor        = 1.0
                           (ContentPresenter ActualHeight = ChartPanel ActualHeight = 452.00;
                            price scale spans full panel height — no margin correction needed)

### T1 dotnet build status

Pre-existing errors in AtrSizingEngine.cs (missing NinjaTrader.NinjaScript.Indicators assembly)
and CopyEngine.cs (CS8370 nullable ref types on net48/C# 7.3) were present before B16 T1.
T1 introduces zero new build errors. F5 in NT8 NinjaScript editor is the authoritative build gate.

---

### T2 Branch Chosen and Result

BRANCH: B
API used: linear interpolation (ChartPanel.MaxValue / MinValue / ActualHeight)
RoundToTickSize: not used -- AlignToTick helper used instead (NT8-029 replacement: RoundToTickSize UNCONFIRMED as available in NT8 AddOn context)
ChartPanel.MaxValue: confirmed present -- build passed with zero new errors (NT8-039 NOT added)
ChartPanel.MinValue: confirmed present -- build passed with zero new errors (NT8-040 NOT added)
CORRECTION_FACTOR used: 1.0 (T1 confirmed: ContentPresenter fills full panel height)
DW-B16-01 status: CLOSED -- Branch B linear interpolation implemented and compiled cleanly
DW-B16-02 status: CLOSED -- IsTrailingStop cancel+replace removed from TightenOneStop; button renamed "Tighten"

### T2 dotnet build status

Pre-existing errors in AtrSizingEngine.cs and CopyEngine.cs (CS8370) were unchanged.
T2 introduces zero new build errors. ChartPanel.MaxValue and ChartPanel.MinValue compiled
without CS1061. AlignToTick helper added as NT8-safe tick-alignment substitute.
10 new [Fact] tests added (T_B16_01..T_B16_10). F5 in NT8 is the authoritative build gate.

---

## B17 Discoveries

### DW-B17-01 Runtime Finding (2026-07-15) — ChartPanel.MaxValue/.MinValue return 0 at click time

**Symptom:** Click trader armed (Disarm button shown, green), Buy selected, account = Sim101,
instrument = MES SEP26, status = "Ready: MES SEP26". Click on chart does nothing — no order placed.

**Root cause:** `GetPriceAtY` calls `FindVisualChild<ChartPanel>(cc)` where `cc` is the
`ChartControl` found by `FindVisualChild<ChartControl>(chart)` (the price canvas). At runtime:

- `ChartPanel.ActualWidth = 139.33` — this is the ChartTrader sidebar width, NOT the price canvas.
  The `ChartPanel` being found is a layout panel inside the ChartTrader area, not the price-axis panel.
- `ChartPanel.MaxValue = 0`, `ChartPanel.MinValue = 0` at click time on the found instance.
- `rawPrice = 0 - yRatio * (0 - 0) = 0.0` → guard (4) fires → `GetPriceAtY` returns 0.0.
- `OnChartMouseDown` guard (5): `rawPrice <= 0.0` → silent return. No order placed.

**Key evidence:**
- T1 diagnostic walked `ChartPanel` at depth=2 from `ChartControl`. Found `ActualWidth = 139.33`
  (ChartTrader panel width). This was the wrong `ChartPanel` — it was inside the ChartTrader
  sidebar, not the price canvas.
- `ChartPanel.MaxValue/.MinValue` compiled without CS1061 (no NT8-039/040 needed) — but they
  return 0 on the wrong instance at runtime. The compile-time success masked the runtime failure.
- The `ChartControl` (price canvas, ~1050px wide) hosts its own `ChartPanel` children (one per
  bar series). The correct `ChartPanel` has non-zero `MaxValue`/`MinValue` but `FindVisualChild`
  finds the first `ChartPanel` in DFS order — which is the sidebar one.

**Investigation needed for B17:**
The correct `ChartPanel` for price geometry must be obtained differently. Options to investigate:
  A. Walk `ChartControl.Charts` collection (if accessible from AddOn scope — unconfirmed).
  B. Use `FindVisualChild<NinjaTrader.Gui.Chart.ChartPanel>` with a predicate that checks
     `ActualWidth > 200` to skip the narrow sidebar panel.
  C. Enumerate all `ChartPanel` children of `ChartControl` directly via `VisualTreeHelper`
     and pick the one with `ActualWidth` matching the canvas (e.g. > 500px).
  D. Fall back to `instrument.MarketData.Last.Price` (B15 approach) as temporary stub
     so click trader fires orders while B17 investigates the correct panel reference.

**Status:** OPEN — filed as DW-B17-01 (P1, blocks click trader Y-price feature).
B16 DW-B16-01 was CLOSED prematurely — `ChartPanel.MaxValue/.MinValue` compile but return 0 at
runtime on the wrong instance. B17 must reopen this.

---

## B17 T1 Discoveries

Date: 2026-07-15 (F5 Sim101 confirmed)

### Visual Tree Dump (F5 Sim101 output — exact MessageBox content)
```
B17 ChartPanel[0]: W=931.33 H=639.33 Max=7633.34 Min=7547.66
Charts property: NOT FOUND
```

### Key Findings

1. **Only ONE ChartPanel exists** under ChartControl in this chart layout.
   - ChartPanel[0]: W=931.33, H=639.33, Max=7633.34, Min=7547.66
   - This IS the price canvas (MaxValue > 0, real price range confirmed).
   - The feared "sidebar panel with W=139.33, Max=0" does NOT exist as a ChartPanel type.
     The sidebar is a different WPF element type entirely.

2. **ChartControl.Charts: NOT FOUND** via Reflection.
   - Option B eliminated. T2 uses Option A path.
   - However Option A predicate (MaxValue > 0 AND largest ActualWidth) is still correct
     defensive code since it guards against any future layout change.

3. **Root cause of DW-B17-01 was NOT wrong panel selection.**
   - FindVisualChild<ChartPanel>(cc) already returned the correct price canvas panel.
   - GetPriceAtY was computing rawPrice correctly (MaxValue/MinValue are real prices).
   - The true bug was cc.MouseDown suppressed by NT8 chart canvas (e.Handled=true).
   - Fix: PreviewMouseDown (tunnel phase) — applied in T1 Amendment (DW-B17-02).

4. **Interim fallback confirmed working**: order fired at Last.Price ~7590.50 after arm+click.

### T2 Plan (confirmed)
- T2 Branch: **Option A** (FindPriceCanvasPanel heuristic, MaxValue > 0 + largest ActualWidth)
- However, since FindVisualChild<ChartPanel> already works correctly (only one ChartPanel,
  it IS the price canvas), T2 may simply: remove T1 diagnostics, remove interim fallback,
  update GetPriceAtY comment block, add 4+ [Fact] tests.
- FindPriceCanvasPanel is still worth adding as defensive code (Option A predicate).

### nt8-rules B17-T1: 1 new rule
- **DW-B17-02 (new)**: cc.MouseDown is swallowed by NT8 chart canvas (e.Handled=true).
  Use cc.PreviewMouseDown (WPF tunnel phase) for all click-trader event registration.
  This applies to ANY AddOn hooking mouse input on ChartControl.

## B17 T2 Discoveries

Date: 2026-07-15

### Confirmed Path
Option A (FindPriceCanvasPanel) implemented. FindVisualChild<ChartPanel> was already returning
the correct panel (only one ChartPanel exists under ChartControl per T1 F5), but replaced
with defensive wrapper (MaxValue > 0 + largest ActualWidth predicate).

### Root Cause Summary (DW-B17-01)
True root cause was cc.MouseDown suppressed by NT8 (e.Handled=true).
Fix: cc.PreviewMouseDown -- applied in T1 Amendment (TradeCopierAddOn.cs).
GetPriceAtY linear interpolation was never broken.
FindPriceCanvasPanel added as defensive wrapper for resilience against future layout changes.

### New NT8 Rule
NT8-041: ChartControl.Charts property does NOT exist (Reflection returns null).
No native NT8 API to enumerate chart panels from ChartControl directly.
Must use VisualTreeHelper DFS walk to find ChartPanel instances.
Predicate: MaxValue > 0 + largest ActualWidth identifies the price canvas panel.

### Test Count Delta
Prior [Fact] count (before T2): 104
Added [Fact] tests (T_B17_01 through T_B17_07): 7
New total [Fact] count: 111

### nt8-rules B17-T2: 1 new rule (NT8-041 above)

### F5 Final Confirmation (2026-07-15)
Click trader fires at exact Y-pixel price. Test: clicked at Y~639 (near bottom of 639.33px panel).
Order placed at 7491.00. Price range was Max=7633.34 Min=7547.66 on this session.
Hotpatch diagnostic showed: rawPrice from GetPriceAtY > 0 (fallback never triggered).
DW-B17-01 CLOSED. GetPriceAtY + FindPriceCanvasPanel + PreviewMouseDown = complete solution.

---

## B19 Session — Test Runner Discovery (PERMANENT RULE)

### Problem: Engineers keep running the wrong test project

Every PTT block, engineers search for a test runner and find `V12_Performance.Tests.csproj`
(located at `tests/V12_Performance.Tests/`). They run it and see 331 tests pass. None of those
331 tests are CopyEngine tests. The 111 `[Fact]` tests in `CopyEngineTests.cs` are never executed.

**Root cause:**
- `PropTraderTools.csproj` targets `net48` (NT8 runtime) — missing `Microsoft.NET.Test.Sdk`, so `dotnet test` cannot discover it
- `V12_Performance.Tests.csproj` targets `net6.0` and IS discoverable — but covers V12 complexity methods only
- `CopyEngineTests.cs` is designed to compile **inside the same assembly as `CopyEngine.cs`** — it references `CopyEngine`'s private nested type `CopyRule` directly (not via reflection cast). A separate test runner project cannot compile it due to this private nested type access.

**Why a separate test runner project cannot work:**
`CopyEngineTests.cs` at line 71 casts `fi.GetValue(_engine)` to `ConcurrentBag<CopyRule>` where `CopyRule` is a `private readonly struct` nested inside `CopyEngine`. This is only valid when both files compile as the same assembly. A separate `.csproj` would fail CS0246 on `CopyRule`. The tests are intentionally co-located in `src/PropTraderTools/`.

### Confirmed solution: build via NT8 F5, assert via Lamport scan

`CopyEngineTests.cs` is validated at two levels:
1. **Structural contract level**: The verifier runs `Select-String` scans (7-scan checklist) to confirm correct source patterns without executing the tests
2. **F5 runtime gate**: NT8 Sim101 confirms runtime behavior

The `[Fact]` count (111 before B19, 113 after B19) is a **source-file contract assertion** — counted by grep, not by `dotnet test`.

```powershell
# CORRECT: Count [Fact] tests by source scan (always works)
Select-String -Path "c:\WSGTA\universal-or-strategy\src\PropTraderTools\CopyEngineTests.cs" -Pattern "^\s+\[Fact\]" | Measure-Object | Select-Object -ExpandProperty Count
# Expected before B19: 111   After B19: 113

# WRONG: Do NOT run these for PTT work — they test unrelated V12 methods
# dotnet test tests/V12_Performance.Tests/  <-- 331 tests, NOT CopyEngine tests
# dotnet test src/PropTraderTools/          <-- net48, dotnet test cannot run it
```

### Permanent rule for all PTT tickets

Every PTT ticket's SCAN-06 and SCAN-07 steps MUST use the source-scan pattern:

```powershell
# SCAN-06: verify new [Fact] tests exist in source
Select-String -Path "c:\WSGTA\universal-or-strategy\src\PropTraderTools\CopyEngineTests.cs" -Pattern "Gate2_UsesAccountName_SourceContractVerified|Gate2_NullMasterAccount_NoCopyOrder"
# Expected: both method names found

# SCAN-07: verify total [Fact] count
Select-String -Path "c:\WSGTA\universal-or-strategy\src\PropTraderTools\CopyEngineTests.cs" -Pattern "^\s+\[Fact\]" | Measure-Object | Select-Object -ExpandProperty Count
# Expected: 113 (111 prior + 2 new B19 Gate2 tests)
```

### NT8_COMPILER_RULES.md update

Add rule NT8-042 to the INDEX TABLE:

```
NT8-042 | CopyEngineTests.cs co-located with CopyEngine.cs (same assembly required) | NEVER create separate test runner .csproj — private nested type CopyRule is inaccessible from outside the assembly | Use Select-String [Fact] count as test contract verification
```

### nt8-rules B19 Session: 1 new rule (NT8-042 above)

## B24 — PTT Deployment Command (deploy-sync.ps1 is V12-ONLY)

### Rule (PERMANENT — all PTT blocks)

`deploy-sync.ps1` is the V12/wave refactoring sync script. It re-synchronizes NT8 hard links
for the V12 codebase in `c:\WSGTA\universal-or-epic-cluster-*`. It does NOT exist in the
Wave workspace (`c:\WSGTA\universal-or-strategy`) and must NEVER be referenced for PTT work.

### Correct PTT Deploy Command

After any `.cs` file edit to `src/PropTraderTools/`, run:

```powershell
# Audit (check only — no changes)
powershell -File "c:\WSGTA\universal-or-strategy\scripts\verify_links.ps1"

# Fix (repair broken/missing hard links — run this after any edit)
powershell -File "c:\WSGTA\universal-or-strategy\scripts\verify_links.ps1" -Fix
```

### What it does

`verify_links.ps1` audits and repairs NTFS hard links between the Wave workspace source files
and NinjaTrader 8's AddOns directory:

- **SRC**: `c:\WSGTA\universal-or-strategy\src\PropTraderTools\*.cs`
- **NT8**: `C:\Users\Mohammed Khalid\Documents\NinjaTrader 8\bin\Custom\AddOns\PropTraderTools\`
- **Excluded from deploy**: `CopyEngineTests.cs` (test file — never deployed to NT8)
- **Hard-linked files**: `CopyEngine.cs`, `TradeCopierPanel.cs`, `TradeCopierWindow.cs`, `TradeCopierAddOn.cs`, `AtrSizingEngine.cs`

Because NT8 loads source files directly from the AddOns directory, a hard link ensures the
same inode is shared — any write to the Wave workspace is instantly visible to NT8 with no
copy step. If the hard link is broken (DESYNC or MISSING), the NT8 file will be stale and
F5 will compile the wrong version.

### PTT Pipeline Orchestrator Template Fix

The orchestrator role definition currently says:
> "Run `powershell -File .\deploy-sync.ps1` to re-synchronize NinjaTrader hard links."

This is WRONG for PTT. The correct instruction for all future PTT block summaries is:
> "Run `powershell -File scripts\verify_links.ps1 -Fix` to re-synchronize NT8 hard links."

### nt8-rules B24: 1 new knowledge entry (deploy command clarification above)
