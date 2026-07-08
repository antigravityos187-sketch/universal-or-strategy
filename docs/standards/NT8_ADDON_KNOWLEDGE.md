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
