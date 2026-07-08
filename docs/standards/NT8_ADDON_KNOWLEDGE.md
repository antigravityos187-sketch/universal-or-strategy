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

### NT8 Chart Attachment API for Indicator — UNRESOLVED (DW-B9-02)

B9 `StartAtrEngine` has a comment: `// IMPL-NOTE-1: NT8 Indicator attachment deferred`.
The correct API to attach a headless Indicator to a Chart's bar data is NOT yet confirmed.
Candidates to try at B10 T4:
- `chart.NinjaScripts.Add(engine)` — most likely
- `chart.Indicators.Add(engine)` — alternative NT8 collection name
- Event-based fallback: subscribe to `chart.BarsArray[0].Bars.BarUpdate` and manually
  call `engine.OnBarUpdate()` — avoids chart attachment entirely

**Do NOT implement any of these paths until B10 T4 tests on Sim101.**

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
