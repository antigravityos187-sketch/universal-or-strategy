# NT8-COMPILER-RULES — NinjaTrader 8 NinjaScript Compiler Constraints
# Version: 1.9 (B53 -- NT8-055 added 2026-08-10)
# Source: PTT Trade Copier blocks B1-B53 (hard compiler errors, runtime crashes, confirmed workarounds)
# Audience: AGENTS ONLY — every rule here was discovered by hitting the actual NT8 Roslyn compiler
# Format: NT8-NNN | SEVERITY | ONE-LINE BAN | DO / DONT | FIX
# Update protocol: append a new rule block whenever a new compiler error or runtime crash is
#   confirmed in NT8. NEVER delete a rule. Update STATUS field only.
# Gate: agents touching ANY .cs file in src/PropTraderTools/ MUST read this file first.

---

## HOW TO READ THIS FILE

Each rule has:
  NT8-NNN   — rule ID, stable forever
  SEVERITY  — P0 = instant build break / crash | P1 = silent wrong behaviour | P2 = style/risk
  CONFIRMED — block where the error was first hit (B1..B10)
  ERROR     — exact CS-code or runtime message produced by the WRONG pattern
  BANNED    — the exact C# construct to never write
  SAFE      — the exact replacement that compiles and works in NT8
  SCAN      — grep/regex to detect violations before committing

---

## CATEGORY: C# LANGUAGE FEATURES NOT SUPPORTED IN NT8 ROSLYN

### NT8-001 | P0 | `{ get; init; }` IS BANNED
CONFIRMED: B7, B8 (CS0518 + CS8341)
ERROR: CS0518 "Predefined type 'System.Runtime.CompilerServices.IsExternalInit' is not defined or imported"
       CS8341 "Only auto-implemented properties can have initializers."
CAUSE: `init` accessor requires C# 9 + .NET 5+ System.Runtime.CompilerServices.IsExternalInit.
       NT8 uses .NET Framework 4.8 with a pre-C#9 Roslyn build. IsExternalInit does not exist.

BANNED:
  internal Account Foo { get; init; }
  public bool Bar { get; init; }

SAFE:
  // Option A: private set + explicit constructor
  internal Account Foo { get; private set; }
  internal Foo(Account foo) { Foo = foo; }

  // Option B: readonly field (for structs where mutability is truly banned)
  internal readonly Account Foo;

SCAN: `\{\s*get;\s*init;\s*\}`

---

### NT8-002 | P0 | `abstract record` / `sealed record` IS BANNED
CONFIRMED: B8 (CS0518 on positional record constructor)
ERROR: CS0518 "IsExternalInit not defined" — positional record constructors synthesise
       compiler-generated init properties internally even when you don't write `init` yourself.
CAUSE: Any `record` with positional parameters generates IsExternalInit usage under the hood.
       NT8's Roslyn version does not have this type.

BANNED:
  public abstract record FollowerAtmMode { ... }
  public sealed record Inherit() : FollowerAtmMode;
  public sealed record Named(string TemplateName) : FollowerAtmMode;

SAFE:
  // Replace abstract record with abstract class + explicit constructors
  public abstract class FollowerAtmMode
  {
      private FollowerAtmMode() { }                 // prevent external subclassing
      public sealed class Inherit : FollowerAtmMode { public Inherit() : base() { } }
      public sealed class Market  : FollowerAtmMode { public Market()  : base() { } }
      public sealed class Named   : FollowerAtmMode
      {
          public string TemplateName { get; private set; }
          public Named(string name) : base() { TemplateName = name; }
      }
  }

NOTE: The nested-class pattern preserves the discriminated union semantics of records.
      Pattern-matching with `is Named n` still works.

SCAN: `\babstract\s+record\b|\bsealed\s+record\b`

---

### NT8-003 | P0 | `volatile double` IS BANNED
CONFIRMED: B8/B9 (CS0677)
ERROR: CS0677 "The volatile modifier cannot be used with fields of type 'double'"
CAUSE: CLR volatile is restricted to reference types and integers up to 32 bits.
       `double` is 64-bit — not allowed.

BANNED:
  private volatile double _lastAtr = 0.0;

SAFE:
  // Remove volatile. Add comment explaining thread-safety reasoning.
  // In NT8 (64-bit process on x64), double reads/writes are naturally atomic on
  // aligned memory. For guaranteed visibility, use Interlocked or a lock-free
  // approach if strict happens-before is needed.
  private double _lastAtr = 0.0;   // written by OnBarUpdate, read by GetSuggestedQty — x64 atomic

  // If strict visibility is required (cross-core):
  private long _lastAtrBits = 0L;   // volatile long wrapping double bits
  // write: Interlocked.Exchange(ref _lastAtrBits, BitConverter.DoubleToInt64Bits(value));
  // read:  BitConverter.Int64BitsToDouble(Interlocked.Read(ref _lastAtrBits));

SCAN: `volatile\s+double`

---

### NT8-004 | P0 | `System.Collections.Immutable` IS NOT AVAILABLE IN NT8 NINJASCRIPT
CONFIRMED: B8 (CS0246 when NinjaScript compiler resolves types)
ERROR: CS0246 "The type or namespace name 'ImmutableDictionary' could not be found"
CAUSE: NT8's NinjaScript compiler does NOT include System.Collections.Immutable in its
       reference assembly set. It is available in the Linting.csproj (dotnet build passes)
       but the NT8 in-process compiler rejects it at Script compile time.

BANNED (in files deployed to NT8 AddOns folder):
  using System.Collections.Immutable;
  ImmutableDictionary<string, Foo> _map = ImmutableDictionary<string, Foo>.Empty;

SAFE:
  // Use Dictionary<K,V> written ONCE at construction time (logically immutable).
  // Document the single-writer contract in a comment.
  private readonly Dictionary<string, FollowerAtmMode> _atmTemplates
      = new Dictionary<string, FollowerAtmMode>();
  // Populated only in constructor/factory. Never mutated after construction.

  // For copy-on-mutate (SetItem equivalent):
  private Dictionary<K,V> CopyWith(Dictionary<K,V> src, K key, V val)
  {
      var next = new Dictionary<K,V>(src);
      next[key] = val;
      return next;
  }

SCAN: `ImmutableDictionary|System\.Collections\.Immutable`

---

### NT8-005 | P0 | `readonly struct` WITH AUTO-PROPERTY `{ get; private set; }` IS BANNED
CONFIRMED: B8 (CS8341)
ERROR: CS8341 "Auto-implemented properties of readonly structs must have a getter but no setter."
CAUSE: A `readonly struct` cannot have a private setter on auto-properties — the setter
       would allow mutation after construction which violates struct readonly contract.

BANNED:
  internal readonly struct FollowerBinding
  {
      internal Account FollowerAccount { get; private set; }   // CS8341
  }

SAFE:
  // Option A: readonly field (simplest)
  internal readonly struct FollowerBinding
  {
      internal readonly Account FollowerAccount;
      internal FollowerBinding(Account acc) { FollowerAccount = acc; }
  }

  // Option B: drop readonly from struct, keep private set
  internal struct FollowerBinding
  {
      internal Account FollowerAccount { get; private set; }
      internal FollowerBinding(Account acc) { FollowerAccount = acc; }
  }

SCAN: `readonly\s+struct` (then check each property for `private set`)

---

### NT8-006 | P1 | `ConcurrentBag<T>.Any()` REQUIRES EXPLICIT `using System.Linq`
CONFIRMED: B8
ERROR: CS1061 "'ConcurrentBag<T>' does not contain a definition for 'Any'"
CAUSE: `Any()` is a LINQ extension method. In NT8 NinjaScript files, `using System.Linq`
       is not auto-included — it must be explicitly declared.

BANNED:
  if (_rules.Any()) { ... }   // without `using System.Linq` at file top

SAFE:
  // Add at file top:
  using System.Linq;
  // Then .Any(), .FirstOrDefault(), .Where() etc. all work.

  // Or avoid LINQ entirely (zero-allocation):
  if (_rules.Count > 0) { ... }   // for List/ConcurrentBag where Count is O(1)

SCAN: `\.Any\(\)` (verify `using System.Linq` is present in the same file)

---

## CATEGORY: NT8 API CONSTRAINTS

### NT8-007 | P0 | `Account.CreateOrder` — ARGUMENT 12 IS `CustomOrder`, NOT `string`
CONFIRMED: B8 (CS1503 argument type mismatch)
ERROR: CS1503 "Argument 12: cannot convert from 'string' to 'NinjaTrader.Cbi.CustomOrder'"
CAUSE: The 12-argument overload of Account.CreateOrder takes a CustomOrder object as the
       final argument, not a plain string. The ATM strategy name is a SEPARATE parameter
       at position 11 (zero-indexed 10).

BANNED:
  acc.CreateOrder(instr, action, type, entry, tif, qty, 0, stop,
                  null, "PTT-Copy", DateTime.MaxValue, "MyAtmTemplate");  // last arg: string

SAFE:
  acc.CreateOrder(instr, action, type, entry, tif, qty, 0, stop,
                  null, "PTT-Copy", DateTime.MaxValue,
                  (NinjaTrader.Cbi.CustomOrder)null);   // last arg: cast null to CustomOrder

  // Signature reference (12 args, zero-indexed):
  //  0  Instrument instrument
  //  1  OrderAction orderAction
  //  2  OrderType orderType
  //  3  OrderEntry orderEntry
  //  4  TimeInForce timeInForce
  //  5  int quantity
  //  6  double limitPrice
  //  7  double stopPrice
  //  8  string oco
  //  9  string signalName          -- must start with "PTT-"
  // 10  DateTime gtd
  // 11  CustomOrder customOrder    -- pass (CustomOrder)null

SCAN: `acc\.CreateOrder\(` — verify last arg is `(NinjaTrader.Cbi.CustomOrder)null` not a string

---

### NT8-008 | P0 | `Chart.ChartControl` PROPERTY DOES NOT EXIST
CONFIRMED: B8 (CS1061)
ERROR: CS1061 "'Chart' does not contain a definition for 'ChartControl'"
CAUSE: The NT8 Chart window class does not expose a `ChartControl` property directly.
       ChartControl is a child in the visual tree, not a named property.

BANNED:
  var cc = chart.ChartControl;

SAFE:
  // Walk the visual tree to find ChartControl
  var cc = FindVisualChild<ChartControl>(chart);
  // FindVisualChild<T> is the depth-first helper already in TradeCopierAddOn.cs

SCAN: `chart\.ChartControl`

---

### NT8-009 | P0 | `ChartControl.GetValueByY()` DOES NOT EXIST IN THIS NT8 VERSION
CONFIRMED: B8 (CS1061)
ERROR: CS1061 "'ChartControl' does not contain a definition for 'GetValueByY'"
CAUSE: This method is documented in some NT8 API references but is absent in the actual
       NT8 version used by PTT. Call site must be stubbed.

BANNED:
  double price = chartControl.GetValueByY(mouseY);

SAFE:
  // Stub to 0.0 and document the deferred work item
  double price = 0.0;   // STUB: GetValueByY not available in this NT8 build (DW-B8-04)
  // Implement via pixel-to-price conversion using chart scale if needed in future.

SCAN: `\.GetValueByY\(`

---

### NT8-010 | P0 | `State.SetDefaults` IN INDICATOR SUBCLASS MUST BE FULLY QUALIFIED
CONFIRMED: B9 (CS0103 ambiguous name)
ERROR: CS0103 "The name 'SetDefaults' does not exist in the current context" OR
       ambiguous reference between NinjaScript State enum and local usage.
CAUSE: Inside a class that inherits from `Indicator`, bare `State.SetDefaults` is
       ambiguous — the compiler cannot resolve whether `State` means the enum or
       something else in scope.

BANNED (inside Indicator subclass OnStateChange):
  if (State == State.SetDefaults) { ... }   // may fail depending on using directives

SAFE:
  if (State == NinjaTrader.NinjaScript.State.SetDefaults) { ... }
  if (State == NinjaTrader.NinjaScript.State.DataLoaded)  { ... }
  if (State == NinjaTrader.NinjaScript.State.Terminated)  { ... }

SCAN: `\bState\.(SetDefaults|DataLoaded|Terminated|Configure)\b` — verify full namespace present

---

### NT8-011 | P0 | `Add(ATR(Period))` IN INDICATOR `OnStateChange` DataLoaded IS INVALID
CONFIRMED: B9 (runtime NullReferenceException / no data)
ERROR: No compiler error — silently produces wrong output. ATR values are always 0 or NaN.
CAUSE: In a headless/detached Indicator (not attached to chart bars via NinjaScripts.Add),
       `Add(ATR(Period))` registers a data series but BarsArray is not wired. The ATR
       indicator is never fed price data so it never calculates.

BANNED (in headless Indicator managed by AddOn):
  protected override void OnStateChange()
  {
      if (State == State.DataLoaded)
          Add(ATR(Period));   // WRONG: Add() for sub-indicators requires chart bar wiring
  }

SAFE:
  // In OnBarUpdate, call the ATR indicator directly on the current bar:
  protected override void OnBarUpdate()
  {
      double atrValue = ATR(Period)[0];   // Correct: called per-bar on current BarsArray
      _lastAtr = atrValue;
  }

SCAN: `Add\(ATR\(` inside `OnStateChange`

---

### NT8-012 | P1 | `FrameworkElementFactory` CANNOT ADD `ColumnDefinitions` BEFORE INSTANTIATION
CONFIRMED: B10-UI-01 (runtime — columns not created, layout broken)
ERROR: No compiler error — columns silently never appear. All cells collapse to column 0.
CAUSE: `FrameworkElementFactory` builds a template that is only instantiated when a
       DataTemplate is applied. `ColumnDefinitions` are a run-time collection — they
       cannot be populated via `FrameworkElementFactory` calls at template-definition time.

BANNED:
  var gridFactory = new FrameworkElementFactory(typeof(Grid));
  gridFactory.AddHandler(FrameworkElement.LoadedEvent, ...);
  // trying to add ColumnDefinitions here — no API exists on FrameworkElementFactory for this

SAFE:
  // Use the Loaded event on the instantiated Grid to add ColumnDefinitions at runtime:
  var gridFactory = new FrameworkElementFactory(typeof(Grid));
  gridFactory.AddHandler(FrameworkElement.LoadedEvent,
      new RoutedEventHandler(OnRowGridLoaded));

  private void OnRowGridLoaded(object sender, RoutedEventArgs e)
  {
      var grid = (Grid)sender;
      if (grid.ColumnDefinitions.Count > 0) return;   // idempotency guard
      grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
      grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
      // ... add remaining columns
      // then set Grid.Column attached property on each child:
      var child = (FrameworkElement)grid.Children[0];
      Grid.SetColumn(child, 0);
  }

SCAN: `FrameworkElementFactory.*Grid` — verify Loaded event handler present, not inline column adds

---

### NT8-013 | P0 | `DateTime.Now` FOR ORDER EXPIRY IS WRONG
CONFIRMED: B1 (confirmed as pattern; always use MaxValue for GTC orders in NT8)
ERROR: No compiler error — order expires immediately or in unexpected ways at broker.
CAUSE: NT8 order GTC (good-til-cancelled) requires `DateTime.MaxValue` as the expiry
       parameter to Account.CreateOrder. Using `DateTime.Now` or any real timestamp
       submits a day-order that expires after that moment.

BANNED:
  acc.CreateOrder(..., DateTime.Now, ...);
  acc.CreateOrder(..., DateTime.UtcNow, ...);
  acc.CreateOrder(..., DateTime.Now.AddDays(1), ...);

SAFE:
  acc.CreateOrder(..., DateTime.MaxValue, ...);   // GTC — never expires

SCAN: `DateTime\.Now[^U]|DateTime\.UtcNow` inside CreateOrder call

---

### NT8-014 | P1 | SIGNAL NAME IN `CreateOrder` MUST START WITH `"PTT-"`
CONFIRMED: B7 (functional constraint — orders without prefix are not tracked by CopyEngine)
ERROR: No compiler error — order placed but CopyEngine.OnOrderUpdate never routes it
       because the signal name filter (`order.Name.StartsWith("PTT-")`) misses it.

BANNED:
  acc.CreateOrder(..., "Copy", ...);
  acc.CreateOrder(..., "Stop", ...);
  acc.CreateOrder(..., "", ...);

SAFE:
  acc.CreateOrder(..., "PTT-Copy", ...);
  acc.CreateOrder(..., "PTT-Click", ...);
  acc.CreateOrder(..., "PTT-Mirror-Close", ...);
  acc.CreateOrder(..., "PTT-BE-Stop", ...);
  acc.CreateOrder(..., "PTT-Tighten", ...);

SCAN: `acc\.CreateOrder` — verify 9th argument (index 9 zero-based) starts with "PTT-"

---

### NT8-015 | P0 | `AtrSizingEngine` (Indicator SUBCLASS) MUST NOT BE `sealed`
CONFIRMED: B9 (runtime crash — NT8 Indicator infrastructure requires unsealed class)
ERROR: NinjaScript compiler rejects sealed Indicator subclasses in some contexts.
       NT8 may need to generate a proxy/wrapper subclass internally.

BANNED:
  public sealed class AtrSizingEngine : Indicator { ... }

SAFE:
  public class AtrSizingEngine : Indicator { ... }   // no sealed modifier

SCAN: `sealed class.*Indicator|sealed class.*NinjaScript`

---

### NT8-016 | P0 | `TradeCopierWindow` MUST NOT BE `sealed`
CONFIRMED: B3 (runtime — window fails to open when sealed)
ERROR: No compiler error — window simply does not appear.
CAUSE: NT8 WPF Window infrastructure (or XAML tooling) may attempt to derive from or
       proxy the Window class. Sealed breaks this.

BANNED:
  public sealed class TradeCopierWindow : Window { ... }

SAFE:
  public class TradeCopierWindow : Window { ... }

SCAN: `sealed class.*Window`

---

### NT8-017 | P1 | `volatile bool` CROSS-THREAD STATE FIELDS ARE MANDATORY (JS-023)
CONFIRMED: B7, B8, B9 (race conditions without volatile)
ERROR: No compiler error — stale cached reads on other CPU cores; non-deterministic behaviour.
CAUSE: NT8 AddOn runs on multiple threads: UI thread (WPF), market data thread
       (OnBarUpdate/MarketDataUpdate), order routing thread (OnOrderUpdate). Any bool/int
       field read on one thread and written on another MUST be volatile.

BANNED:
  private bool _clickArmed = false;   // read on market thread, written on UI thread
  private int  _copyModeValue = 0;    // read on order thread, written on UI thread

SAFE:
  private volatile bool _clickArmed    = false;   // JS-023: UI writes, market reads
  private volatile int  _copyModeValue = 0;       // JS-023: UI writes, order thread reads
  private volatile bool _atrEnabled    = false;   // JS-023

SCAN: `private bool _|private int _` in class fields — check if accessed from >1 thread;
      if yes, must be `volatile`

---

### NT8-018 | P1 | `lock()` IS BANNED — USE `volatile` + `ConcurrentDictionary`/`ConcurrentBag`
CONFIRMED: B1 (JS-021 — performance + deadlock risk in NT8 callback threads)
ERROR: No compiler error — potential deadlock; NT8 callback threads must not block.

BANNED:
  private readonly object _lock = new object();
  lock (_lock) { _state = newState; }

SAFE:
  private volatile int _stateValue = 0;   // int backing for enum
  // Write: _stateValue = (int)newState;  (single writer)
  // Read:  var s = (MyEnum)_stateValue;  (multiple readers)

  // For collections: ConcurrentDictionary or ConcurrentBag
  private ConcurrentDictionary<string, Foo> _map = new ConcurrentDictionary<string, Foo>();
  private ConcurrentBag<CopyRule> _rules = new ConcurrentBag<CopyRule>();

SCAN: `lock\s*\(` (non-comment)

---

### NT8-019 | P0 | `async void` IS BANNED IN NT8 CALLBACK METHODS
CONFIRMED: B1 (JS-033)
ERROR: No compiler error — unhandled exceptions in async void crash the NT8 process silently.
CAUSE: NT8 lifecycle methods (`OnOrderUpdate`, `OnBarUpdate`, etc.) are called synchronously
       by the NT8 runtime. Making them async void means exceptions escape the call site.

BANNED:
  protected override async void OnOrderUpdate(...) { await Task.Delay(1); }
  private async void OnBreakEvenClick(object s, RoutedEventArgs e) { await ...; }

SAFE:
  // All NT8 callback overrides must be synchronous void.
  // For deferred UI work: Dispatcher.InvokeAsync (fire-and-forget on UI thread is safe).
  protected override void OnOrderUpdate(...) { Dispatcher.InvokeAsync(() => UpdateUI()); }

SCAN: `async void`

---

### NT8-020 | P1 | `SolidColorBrush` MUST BE FROZEN BEFORE USE IN NT8 WPF
CONFIRMED: B7 (runtime InvalidOperationException across threads)
ERROR: InvalidOperationException "Cannot modify a frozen object" OR cross-thread access error.
CAUSE: WPF brushes created on one thread and used on another throw if not frozen.
       In NT8, UI updates may occur on threads other than the brush-creation thread.

BANNED:
  button.Background = new SolidColorBrush(Color.FromRgb(34, 197, 94));

SAFE:
  // Use a helper that always freezes:
  private static SolidColorBrush MakeBrush(byte r, byte g, byte b)
  {
      var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
      brush.Freeze();
      return brush;
  }
  button.Background = MakeBrush(34, 197, 94);

SCAN: `new SolidColorBrush\(` without `.Freeze()` on the next line

---

### NT8-021 | P1 | `Account.All` MUST NOT BE ACCESSED IN CONSTRUCTORS OR FIELD INITIALIZERS
CONFIRMED: B5 (NullReferenceException at startup)
ERROR: NullReferenceException — Account.All is null before NT8 initializes.
CAUSE: NT8 account infrastructure is not ready at AddOn construction time.
       Account.All is only populated after NT8 has fully loaded.

BANNED:
  public class MyAddOn : AddOnBase
  {
      private readonly IEnumerable<Account> _accs = Account.All;   // CRASH
  }

SAFE:
  // Access Account.All only inside event handlers that fire after NT8 is loaded:
  protected override void OnWindowCreated(Window w)
  {
      foreach (var acc in Account.All) { ... }   // Safe here
  }

SCAN: `Account\.All` outside of event handlers/lifecycle overrides

---

### NT8-022 | P1 | `WPF KeyBinding WITH LETTER KEYS` IS REJECTED BY NT8
CONFIRMED: B4 (KeyBinding silently ignored)
ERROR: No compiler error — keyboard shortcut simply never fires.
CAUSE: NT8 intercepts keyboard input for its own trading shortcuts. Letter-key bindings
       added via WPF KeyBinding are swallowed before they reach the AddOn's Window.

BANNED:
  new KeyBinding(command, new KeyGesture(Key.B, ModifierKeys.Shift));

SAFE:
  // Use buttons only — no keyboard shortcuts for trading actions.
  // If keyboard shortcuts are needed, use NT8's own hotkey API, not WPF KeyBinding.

SCAN: `KeyBinding|KeyGesture` (flag for review — may be intentional in non-trading contexts)

---

### NT8-023 | P1 | `NTWindow` CANNOT BE EMBEDDED AS A `UserControl`
CONFIRMED: B5 (layout broken — NTWindow is a Window, not a control)
ERROR: No compiler error — window appears as a separate floating window, not embedded.

BANNED:
  public class TradeCopierPanel : NTWindow { ... }   // for injectable panel

SAFE:
  public class TradeCopierPanel : UserControl { ... }   // injectable into Grid rows

SCAN: `: NTWindow` (check if intended as injectable panel — if yes, change to UserControl)

---

### NT8-024 | P1 | `NTWindow` AS STANDALONE WINDOW BASE CAUSES WINDOW-NOT-APPEARING
CONFIRMED: B6 (window opens but is not visible)
ERROR: No compiler error — window object exists but never renders.
CAUSE: NTWindow has internal NT8 lifecycle hooks that conflict with direct `new Window().Show()`.

BANNED:
  public class TradeCopierWindow : NTWindow { ... }

SAFE:
  public class TradeCopierWindow : System.Windows.Window { ... }

SCAN: `: NTWindow\b`

---

### NT8-025 | P1 | `NTMenuItem.Header` IS NOT ALWAYS A `string` — USE `.ToString()`
CONFIRMED: B6 (null reference / wrong comparison)
ERROR: No compiler error — deduplication check silently fails; menu item added twice.
CAUSE: NT8 may set `MenuItem.Header` to a `TextBlock` object rather than a plain string.
       `mi.Header as string` returns null when Header is TextBlock.

BANNED:
  if (mi.Header as string == "Trade Copier") return;

SAFE:
  var hdr = mi.Header != null ? mi.Header.ToString() : string.Empty;
  if (hdr == "Trade Copier") return;

  // Better: use a volatile bool guard instead of header comparison:
  private static volatile bool _menuWired = false;

SCAN: `\.Header\s+as\s+string`

---

### NT8-026 | P1 | TRAILING STOP ORDER DETECTED BY `order.TrailPrice > 0`
CONFIRMED: B9/B10 (behavioral — incorrect handling without this check)
ERROR: No compiler error — acc.Change(StopPrice) on trailing stop may freeze the trail silently.
CAUSE: NT8 trailing stop orders are `OrderType.StopMarket` orders with `TrailPrice > 0`.
       Calling acc.Change() with only a new StopPrice on such an order has UNDEFINED behaviour
       (trail may freeze). Must detect and handle separately.

BANNED:
  order.StopPrice = newStop;
  acc.Change(new Order[] { order });   // if order.TrailPrice > 0: trail may be killed

SAFE:
  bool isTrailing = order.TrailPrice > 0;
  if (isTrailing)
  {
      // Option B (safe default): skip Mode 2 relay for trailing stops
      // Option B for BE: cancel + replace with fixed StopMarket
      acc.Cancel(new Order[] { order });
      acc.CreateOrder(instr, action, OrderType.StopMarket, OrderEntry.Manual,
                      TimeInForce.Day, qty, 0, newStop, null, "PTT-BE-Stop",
                      DateTime.MaxValue, (NinjaTrader.Cbi.CustomOrder)null);
  }
  else
  {
      order.StopPrice = newStop;
      acc.Change(new Order[] { order });
  }

SCAN: `acc\.Change\(new Order` — verify IsTrailingStop guard present before call

---

### NT8-027 | P1 | `Instrument.MarketData` SUBSCRIPTION FROM ADDON CONTEXT — VERIFY BEFORE USE
CONFIRMED: B9/B10 (GAP-002 — unverified as of B10 start)
ERROR: No compiler error — MarketDataUpdate handler may never fire if subscription does not
       work in AddOnBase context (no NinjaScriptBase lifecycle).
STATUS: PENDING SIM101 VERIFICATION (GAP-002 test not yet run)
CAUSE: `NinjaTrader.Data.Instrument.GetInstrument(name).MarketData.MarketDataUpdate` event
       is documented as available but its behaviour from an AddOnBase subclass (no OnBarUpdate,
       no OnStateChange lifecycle) is unconfirmed.

SAFE PATTERN (once confirmed):
  var dataInstr = NinjaTrader.Data.Instrument.GetInstrument(instr.FullName);
  if (dataInstr != null)
      dataInstr.MarketData.MarketDataUpdate += OnPriceTick;

  // Handler fires on market data thread — volatile field reads only, Dispatcher.InvokeAsync for UI

FALLBACK (if MarketDataUpdate does NOT fire from AddOn):
  // Use Account.AccountItemUpdate (UnrealizedPnL changes proxy for price movement)
  // — coarser resolution, fires per P&L tick not per price tick

UPDATE THIS RULE: after running GAP-002 Sim101 test, fill in confirmed behaviour.
SCAN: `MarketData\.MarketDataUpdate` — check if confirmed by GAP-002 test result

---

### NT8-028 | P1 | `HEX COLOR STRING LITERALS` ARE BANNED — USE `MakeBrush(r,g,b)`
CONFIRMED: B7 (risk of NT8 WPF parsing failure + violates team scan)
ERROR: No compiler error — may cause WPF XAML parse failure in some NT8 versions.

BANNED:
  button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22c55e"));

SAFE:
  button.Background = MakeBrush(34, 197, 94);   // #22c55e expressed as decimal RGB

SCAN: `"#[0-9A-Fa-f]{6}"` in string literals (not in comments)

---

### NT8-029 | P1 | `TICK ALIGNMENT` IS MANDATORY ON ALL STOP/LIMIT PRICES
CONFIRMED: B4 (order rejection by broker/sim if price not on tick boundary)
ERROR: No compiler error — order may be rejected or silently moved by broker to nearest tick.

BANNED:
  double stopPrice = entryPrice - 0.03;   // arbitrary offset, may not be on tick

SAFE:
  double raw = entryPrice - (bufferTicks * instrument.MasterInstrument.TickSize);
  double stopPrice = Math.Round(raw / instrument.MasterInstrument.TickSize)
                   * instrument.MasterInstrument.TickSize;

SCAN: manual review — check all stop/limit price calculations for tick alignment

---

### NT8-030 | P0 | `OnWindowCreated(Window)` FIRES FOR EVERY NT8 WINDOW — GUARD WITH VOLATILE BOOL
CONFIRMED: B6 (menu item added N times — one per window open)
ERROR: No compiler error — menu items multiply; injection runs on wrong window types.

BANNED:
  protected override void OnWindowCreated(Window window)
  {
      AddMenuItems(window);   // called for EVERY window: Control Center, Chart, etc.
  }

SAFE:
  private static volatile bool _menuWired = false;

  protected override void OnWindowCreated(Window window)
  {
      if (_menuWired) return;               // idempotency guard
      var cc = window as ControlCenter;
      if (cc == null) return;               // only act on Control Center
      AddMenuItems(cc);
      _menuWired = true;
  }

SCAN: `OnWindowCreated` — verify idempotency guard present before any state mutation

---


### NT8-031 | P0 | `OrderState.PendingSubmit` DOES NOT EXIST IN NT8
CONFIRMED: B18 (CS0117 — F5 gate failure)
ERROR: CS0117 "'OrderState' does not contain a definition for 'PendingSubmit'"
CAUSE: NT8's `OrderState` enum does not include `PendingSubmit`. Standard .NET/NinjaTrader 7
       docs may list it but NT8 Roslyn build host does not expose it. Only `Initialized`,
       `Working`, `Cancelled`, `Filled`, `PartFilled`, `Rejected`, `Unknown` exist in NT8.

BANNED:
  if (order.OrderState != OrderState.Working &&
      order.OrderState != OrderState.Initialized &&
      order.OrderState != OrderState.PendingSubmit)   // CS0117 — PendingSubmit does not exist
      continue;

SAFE:
  if (order.OrderState != OrderState.Working &&
      order.OrderState != OrderState.Initialized)     // Initialized covers pre-ack state
      continue;

NOTE: `Initialized` is the NT8 pre-acknowledgement state that covers what `PendingSubmit`
      represents in other platforms. Using `Initialized` alone is sufficient.

SCAN: `OrderState\.PendingSubmit`

---


### NT8-032 | P1 | `MarketData.Ask` / `MarketData.Bid` / `MarketData.Last` ARE `MarketDataEventArgs` — USE `.Price`
CONFIRMED: B12 (B19 documentation pass — usage confirmed working in CopyEngine.cs:1179-1180)
ERROR: No compiler error — type confusion causes CS1061 "does not contain a definition for 'Price'"
       if the field is treated as a raw double instead of a MarketDataEventArgs object.
CAUSE: `Instrument.MarketData.Ask`, `.Bid`, and `.Last` all return `MarketDataEventArgs` objects
       (same type). The actual double price is the `.Price` property of the returned object.
       The object reference itself may be null if data has not yet populated the field
       (pre-market, stale subscription, or instrument not yet active in a chart session).

BANNED:
  double ask = instrument.MarketData.Ask;              // CS1061 — Ask is MarketDataEventArgs, not double
  if (instrument.MarketData.Ask > 0) ...               // CS0019 — cannot compare object to double

SAFE:
  // Always use the full null-guard chain before accessing .Price:
  var md  = instrument.MarketData;
  if (md == null) return 0.0;
  var ask = md.Ask;
  if (ask == null) return 0.0;
  return ask.Price;                                    // double — confirmed working pattern

  // Pattern confirmed in CopyEngine.cs (B12) — existing production code at ~line 1179-1180:
  //   instrument.MarketData.Bid.Price
  //   instrument.MarketData.Ask.Price
  // Used from UI thread (OnTrimClick, OnFlattenClick, DispatchShortcut) — synchronous snapshot read.
  // No subscription required for snapshot reads; field is populated once instrument is active.

SCAN: `MarketData\.(Ask|Bid|Last)[^.]` — catches missing .Price (direct use of object without .Price)

---

### NT8-041 | P2 | `ChartControl.Charts` NOT ACCESSIBLE VIA REFLECTION IN NT8
CONFIRMED: B17 (runtime -- reflection returns null)
ERROR: ChartControl.Charts property NOT FOUND via Reflection at runtime.
       GetType().GetProperty("Charts") returns null.
CAUSE: NT8 .NET 4.8 does not expose Charts as a public reflection-visible property on
       ChartControl in the AddOn compilation context.
       GetType().GetProperty("Charts") returns null.

BANNED:
  // Attempting to enumerate Charts via reflection:
  var chartsProp = chartControl.GetType().GetProperty("Charts");
  var charts = chartsProp?.GetValue(chartControl);   // chartsProp is null -- NullReferenceException

SAFE:
  // Use visual tree walk -- always available in AddOnBase context:
  var chart = FindVisualChild<Chart>(visualTreeRoot);
  // Or to find all charts: walk all top-level NT8 windows and cast to Chart.
  // FindVisualChild<T> is in TradeCopierAddOn.cs (the depth-first helper).

SCAN: GetProperty.*Charts

---

### NT8-042 | P0 | `Dispatcher.InvokeAsync` FROM AddOn CONTEXT IS NOT AVAILABLE IN NT8
CONFIRMED: B23 (CS0117, CS1061 -- all 3 known dispatcher paths fail)
ERROR: CS0117 "'Globals' does not contain a definition for 'Application'"
       CS1061 "'GeneralOptions' does not contain a definition for 'Dispatcher'"
       CS1061 "'Dispatcher' does not contain a definition for 'InvokeAsync'" (System.Windows variant)
CAUSE: NT8 NinjaScript AddOn runs inside NT8's process under a restricted Roslyn build.
       None of the three dispatcher paths compile:
         (1) NinjaTrader.Core.Globals.GeneralOptions.Dispatcher  -- CS1061 (no Dispatcher on GeneralOptions)
         (2) NinjaTrader.Core.Globals.Application.Dispatcher     -- CS0117 (no Application on Globals)
         (3) System.Windows.Application.Current.Dispatcher.InvokeAsync() -- CS1061 (InvokeAsync absent)
       The WPF Dispatcher type in NT8's .NET 4.8 host does not expose InvokeAsync from AddOn context.

BANNED:
  NinjaTrader.Core.Globals.GeneralOptions.Dispatcher.InvokeAsync(() => { ... });
  NinjaTrader.Core.Globals.Application.Dispatcher.InvokeAsync(() => { ... });
  System.Windows.Application.Current.Dispatcher.InvokeAsync(() => { ... });

SAFE:
  // Wrap the call in try/catch -- NT8 catches the NullRef and logs it.
  // UI-thread marshaling from AddOn context requires Dispatcher.BeginInvoke (not InvokeAsync)
  // via System.Windows.Threading.Dispatcher -- UNCONFIRMED, pending B24 research.
  try
  {
      follower.CreateOrder( ... );
      return true;
  }
  catch (Exception ex)
  {
      StatusUpdate?.Invoke("PTT-Copy error: " + ex.Message);
      return false;
  }

SCAN: \.Dispatcher\.InvokeAsync|Globals\.Application|GeneralOptions\.Dispatcher

---

### NT8-043 | P0 | NULL-CONDITIONAL COMPOUND ASSIGNMENT (`?.` with `-=`) IS BANNED IN NT8
CONFIRMED: B23 (CS8370 / parse error)
ERROR: error CS8370: Feature 'null-conditional assignment' is not available in C# 7.3.
       Please use language version 9.0 or greater.
CAUSE: NT8 compiles under C# 7.3 (pre-Roslyn null-conditional assignment). The `?.` operator
       on the LEFT side of `-=` or `+=` requires C# 9.0+. NT8's host is locked at 7.3.

BANNED:
  acc?.AccountItemUpdate -= OnPendingBeAccountUpdate;   // CS8370

SAFE:
  if (acc != null)
      acc.AccountItemUpdate -= OnPendingBeAccountUpdate;

SCAN: \?\.\w+\s*[-+]=

---

### NT8-044 | P0 | `StringComparison` REQUIRES `using System;` IN NT8 NINJASCRIPT
CONFIRMED: B24 (CS0103 at F5 compile)
ERROR: CS0103 "The name 'StringComparison' does not exist in the current context"
CAUSE: `StringComparison` is in the `System` namespace. NT8's NinjaScript compiler does NOT
       auto-inject `using System;` the way a full SDK project does. Any use of
       `StringComparison.OrdinalIgnoreCase`, `StringComparison.Ordinal`, etc. requires an
       explicit `using System;` at the top of the file. The same applies to other common
       System-namespace types: `Math`, `Environment`, `Convert`, `Console`, etc.

BANNED (without using System at top of file):
  string.Equals(a.Name, text, StringComparison.OrdinalIgnoreCase)  // CS0103

SAFE:
  // Add at top of file (before all other using statements):
  using System;

  // Then all StringComparison values resolve:
  string.Equals(a.Name, text, StringComparison.OrdinalIgnoreCase)  // OK
  string.Equals(a.Name, text, StringComparison.Ordinal)            // OK

NOTE: NT8 NinjaScript files auto-include NinjaTrader.* namespaces but NOT System.*.
      Always add `using System;` explicitly whenever you use:
        StringComparison, Math, Environment, Convert, EventArgs, Exception, etc.

SCAN: `StringComparison` without `using System;` in file preamble

---

### NT8-045 | P1 | `AtmStrategy.AtmStrategyTemplates` NOT AVAILABLE IN LINTING DLL — USE FILESYSTEM PATH
CONFIRMED: B43 (CS0117 in Linting project — property not exposed in NinjaTrader.Custom.dll backup)
ERROR: CS0117 "'AtmStrategy' does not contain a definition for 'AtmStrategyTemplates'"
CAUSE: `NinjaTrader.NinjaScript.AtmStrategy.AtmStrategyTemplates` is a static property available
       inside NT8's internal NinjaScript runtime (F5 compilation context), but it is NOT exposed
       in the `NinjaTrader.Custom.dll` referenced by the external Linting .csproj.
       This is the same class-boundary issue as NT8-009 (GetValueByY absent from external DLL).
       The property DOES exist and works correctly at NT8 F5 compile time.

BANNED (in code that must compile in the Linting .csproj):
  foreach (var t in NinjaTrader.NinjaScript.AtmStrategy.AtmStrategyTemplates)
      cb.Items.Add(t.Name);

SAFE:
  // Use filesystem path -- NT8 stores ATM templates as XML files in:
  // Documents\NinjaTrader 8\templates\AtmStrategy\<TemplateName>.xml
  // Wrapped in try/catch so it degrades gracefully if directory is missing.
  try
  {
      string atmDir = System.IO.Path.Combine(
          System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
          "NinjaTrader 8", "templates", "AtmStrategy");
      if (System.IO.Directory.Exists(atmDir))
          foreach (var f in System.IO.Directory.GetFiles(atmDir, "*.xml"))
              cb.Items.Add(System.IO.Path.GetFileNameWithoutExtension(f));
  }
  catch { }

NOTE: This filesystem approach works in BOTH the Linting .csproj (MSBuild) AND NT8's F5 runtime.
      Template names from the filesystem match exactly the names NT8 uses in its internal templates list.
      The directory path is guaranteed by NT8's template management infrastructure.

SCAN: `AtmStrategyTemplates` — replace with filesystem path pattern above.

---

## CATEGORY: AGENT UPDATE PROTOCOL

### HOW TO ADD A NEW RULE

When a new NT8 compiler error or runtime crash is confirmed in a PTT block:

1. Assign next ID: NT8-NNN (increment from last rule above)
2. Set SEVERITY: P0 (build/crash), P1 (silent wrong behaviour), P2 (style/risk)
3. Set CONFIRMED: block where hit (B11, B12, ...)
4. Fill in ERROR, CAUSE, BANNED, SAFE, SCAN fields
5. Add rule to the bottom of the relevant CATEGORY section
6. Update the version line and date at the top of this file
7. Append a summary row to the INDEX TABLE below

---

## INDEX TABLE — ALL RULES AT A GLANCE

| ID      | Severity | Summary | Confirmed |
|---------|----------|---------|-----------|
| NT8-001 | P0 | `{ get; init; }` banned — IsExternalInit not in NT8 Roslyn | B7/B8 |
| NT8-002 | P0 | `abstract record` / `sealed record` banned — record constructors emit IsExternalInit | B8 |
| NT8-003 | P0 | `volatile double` banned — CLR only allows volatile on <=32-bit / refs | B8/B9 |
| NT8-004 | P0 | `System.Collections.Immutable` not available in NT8 NinjaScript | B8 |
| NT8-005 | P0 | `readonly struct` with `{ get; private set; }` banned (CS8341) | B8 |
| NT8-006 | P1 | `ConcurrentBag.Any()` requires explicit `using System.Linq` | B8 |
| NT8-007 | P0 | `CreateOrder` arg 12 is `CustomOrder`, not `string` | B8 |
| NT8-008 | P0 | `Chart.ChartControl` property does not exist — use FindVisualChild | B8 |
| NT8-009 | P0 | `ChartControl.GetValueByY()` absent in this NT8 build — stub it | B8 |
| NT8-010 | P0 | `State.SetDefaults` in Indicator must be fully namespace-qualified | B9 |
| NT8-011 | P0 | `Add(ATR(Period))` in headless Indicator OnStateChange is invalid | B9 |
| NT8-012 | P1 | `FrameworkElementFactory` cannot add ColumnDefinitions before instantiation | B10-UI-01 |
| NT8-013 | P0 | `DateTime.Now` for CreateOrder expiry — use `DateTime.MaxValue` | B1 |
| NT8-014 | P1 | CreateOrder signal name must start with `"PTT-"` | B7 |
| NT8-015 | P0 | `AtrSizingEngine : Indicator` must not be sealed | B9 |
| NT8-016 | P0 | `TradeCopierWindow : Window` must not be sealed | B3 |
| NT8-017 | P1 | Cross-thread bool/int fields must be `volatile` (JS-023) | B7/B9 |
| NT8-018 | P1 | `lock()` banned — use volatile + ConcurrentDictionary/ConcurrentBag | B1 |
| NT8-019 | P0 | `async void` banned in NT8 callback methods | B1 |
| NT8-020 | P1 | `SolidColorBrush` must be `.Freeze()`d before cross-thread use | B7 |
| NT8-021 | P1 | `Account.All` banned in constructors/field initializers | B5 |
| NT8-022 | P1 | WPF `KeyBinding` with letter keys silently ignored by NT8 | B4 |
| NT8-023 | P1 | `NTWindow` as UserControl base — use `UserControl` instead | B5 |
| NT8-024 | P1 | `NTWindow` as standalone Window base causes window-not-appearing | B6 |
| NT8-025 | P1 | `NTMenuItem.Header as string` returns null — use `.ToString()` | B6 |
| NT8-026 | P1 | Trailing stop `order.TrailPrice > 0` — detect before acc.Change() | B9/B10 |
| NT8-027 | P1 | `Instrument.MarketData` from AddOn context — verify before use (GAP-002 PENDING) | B9/B10 |
| NT8-028 | P1 | Hex color string literals banned — use `MakeBrush(r,g,b)` | B7 |
| NT8-029 | P1 | Tick alignment mandatory on all stop/limit prices | B4 |
| NT8-030 | P0 | `OnWindowCreated` fires for every NT8 window — guard with volatile bool | B6 |
| NT8-031 | P0 | `OrderState.PendingSubmit` does not exist in NT8 — use `Initialized` only | B18 |
| NT8-032 | P1 | `MarketData.Ask/.Bid/.Last` are `MarketDataEventArgs` — always use `.Price`; full null-guard chain required | B12/B19 |
| NT8-041 | P2 | `ChartControl.Charts` NOT accessible via Reflection -- use FindVisualChild<Chart> | B17 |
| NT8-042 | P0 | `Dispatcher.InvokeAsync` not available in NT8 AddOn context -- all 3 paths fail CS0117/CS1061 | B23 |
| NT8-043 | P0 | Null-conditional compound assignment (`acc?.Event -= handler`) banned -- C# 7.3 limit | B23 |
| NT8-044 | P0 | `StringComparison` requires explicit `using System;` -- NT8 does not auto-inject System namespace | B24 |
| NT8-045 | P1 | `AtmStrategy.AtmStrategyTemplates` not in Linting DLL -- use filesystem path `Documents\NinjaTrader 8\templates\AtmStrategy\*.xml` | B43 |
| NT8-046 | P1 | `acc.Change()` on ATM slot orders (Stop1/Stop2) silently overridden by ATM engine | B32 |
| NT8-047 | P1 | ATM slot order name pattern: Stop1/Stop2 with FromEntrySignal==null | B32 |
| NT8-048 | P2 | Native "Breakeven ATM strategy" hotkey exists in Tools -> Keyboard Shortcuts -- zero code needed | B33 |
| NT8-049 | P0 | `CreateOrder` limitPrice/stopPrice swapped silently; wrong account; wrong qty -- 3 bugs from B33 live test | B33 |
| NT8-050 | P0 | `Account.Positions[Instrument]` CS1503 -- use `FindPosition(acc, instr)` instead | B33 |
| NT8-051 | P1 | NT8 sim (Sim101/Sim102) does NOT auto-cancel ATM brackets after position flat -- must call `CancelStaleBrackets` | B33 |

---

### NT8-046 | P1 | acc.Change() on ATM-owned slot orders (Stop1/Stop2) -- CORRECTED in B32
ORIGINAL CLAIM (B31 2026-07-17): single-array acc.Change(new[]{order}) works on ATM-owned stops.
CORRECTION (B32 live test 2026-07-19): WRONG for ATM strategy slot orders (Stop1, Stop2, Stop3...).

CONFIRMED B32: Account.Change() has ONE overload: Change(IEnumerable<Order>).
(Verified: [NinjaTrader.Cbi.Account].GetMethods() | Where Name -eq "Change" -- single result)
There is no multi-param overload. The "banned multi-param" in the original rule does not exist.

ROOT CAUSE: NT8 ATM engine intercepts ALL acc.Change() calls on orders it owns.
            It re-applies its own managed price on the next ATM tick. No exception thrown.
            order.StopPrice property reverts immediately on C# object (local property revert).
            Stop flickers briefly on chart then snaps back -- ATM intercept signature.

SCOPE: Affects orders named Stop1/Stop2/Stop3... with FromEntrySignal == null (ATM slot orders).
       Does NOT affect PTT-created follower bracket orders (FromEntrySignal != null).
       SyncFollowerBracket works because it targets PTT-created orders, not ATM slot orders.

BANNED:
  acc.Change(new Order[]{order})  -- on ATM slot orders (Stop1/Stop2/Stop3...) -- silently overridden
  acc.Cancel(new Order[]{stop}); acc.CreateOrder(...)  -- destroys OCO link

SAFE:
  order.StopPrice = newPrice;
  acc.Change(new Order[] { order });
  // ONLY on PTT-created follower bracket orders (order.FromEntrySignal != null).
  // Confirmed: SyncFollowerBracket CopyEngine.cs L621-624 (PTT-created orders only).

FIX (DW-B32-07): (1) BreakEven(Account leader,...) -- leader direct MoveStop call removed.
                 ATM manages its own BE via built-in template logic.
                 (2) MoveStopToBreakEven inner loop -- guard skips Stop\d+ ATM slot orders.

SCAN: TryCreateStopWithRetry|acc\.Cancel\(new Order\[\]

---

### NT8-047 | P1 | ATM slot order name pattern -- Stop1/Stop2/Stop3
CONFIRMED: B32 (Director live test 2026-07-19)
NT8 ATM strategies name their managed stop slots as Stop1, Stop2, Stop3...
These have FromEntrySignal == null (ATM-owned, no PTT entry signal).
Detect pattern: Name.StartsWith("Stop") && char.IsDigit(Name[4]) && FromEntrySignal == null

BANNED: acc.Change() on these orders -- ATM engine overrides silently.
SAFE:   Skip in any change loop. Use acc.CreateOrder() for new PTT-managed stops on followers only.

SCAN: order\.Name.*Stop\d

---

### NT8-048 | P2 | NT8 NATIVE "BREAKEVEN ATM STRATEGY" HOTKEY ACTION EXISTS
CONFIRMED: B33 (Director research 2026-07-20 -- zero code, confirmed working)
NT8 has a built-in "Breakeven ATM strategy" action in Tools -> Keyboard Shortcuts -> Order Entry.
Assigns to any key combo. Calls ATM engine internally -- moves ATM-owned stop to entry immediately.
No arm logic (instant fire on keypress). Works on any ATM trade without any AddOn code.
Does NOT use acc.Change() -- bypasses NT8-046 entirely (ATM engine handles the move internally).

USE: Manual instant BE without any AddOn code. Assign to e.g. Ctrl+B in NT8 settings.
NOT A REPLACEMENT: Does not support armed/delayed BE (wait for price cross then fire).
                   For armed BE, the AddOn SubmitBeStop approach (DW-B33-01) is required.
DISCOVERY: Director confirmed while investigating NT8-046 workarounds in B33.

SCAN: N/A -- this is a discovery note, not a code pattern to scan.

---

### NT8-050 | P0 | `Account.Positions[Instrument]` DOES NOT COMPILE -- USE `FindPosition(acc, instr)`
CONFIRMED: B33 (CS1503 at F5 compile -- Argument 1: cannot convert from 'NinjaTrader.Cbi.Instrument' to 'int')
ERROR: CS1503 "Argument 1: cannot convert from 'NinjaTrader.Cbi.Instrument' to 'int'"
CAUSE: NT8's `Account.Positions` collection exposes an int indexer (by position slot index),
       NOT a typed Instrument indexer. Passing an Instrument object causes CS1503 silently
       accepted by IDE autocomplete but rejected by NT8 Roslyn at F5.

BANNED:
  var pos = acc.Positions[instr];           // CS1503 -- Instrument is not int

SAFE:
  var pos = FindPosition(acc, instr);       // existing helper in CopyEngine.cs L1383
  // FindPosition iterates acc.Positions and matches by Instrument object reference.
  // Returns null if no position; use IsFlat(pos) for null+zero guard.

  // Or inline:
  Position pos = null;
  foreach (Position p in acc.Positions)
      if (p.Instrument == instr) { pos = p; break; }

SCAN: `\.Positions\[instr\]|\\.Positions\[instrument\]`

---

### NT8-049 | P0 | CreateOrder argument order -- limitPrice/stopPrice swap is a silent bug
CONFIRMED: B33 (Director live test 2026-07-20 -- order appeared with Limit=0, wrong account, qty=13)
ERROR: No compiler error. NT8 silently accepts the malformed call.
SYMPTOM: Order tab shows State=Cancel, Limit=0, Stop not set, qty=total-of-all-accounts.
         Stop never triggers. Position stays open. Orphan order stuck in Cancel state.

CAUSE (3 bugs confirmed in B33 first attempt):
  Bug 1 -- arg order: limitPrice is arg6, stopPrice is arg7. Engineer swapped them.
            bePrice went into arg6 (limitPrice slot) and 0 went into arg7 (stopPrice slot).
            NT8 accepted silently. Result: limit order with limitPrice=bePrice, stopPrice=0.
  Bug 2 -- account scope: SubmitBeStop was called inside a foreach-all-accounts loop.
            Order submitted to Sim102 (wrong account) instead of leader account only.
  Bug 3 -- qty source: qty was passed in as a parameter summed from outer loop.
            Should be read directly from leaderAcc.Positions[instr].Quantity inside method.

BANNED:
  // WRONG -- limitPrice and stopPrice swapped:
  acc.CreateOrder(instr, action, OrderType.StopMarket, OrderEntry.Manual,
                  TimeInForce.Day, qty, bePrice, 0, ...);  // bePrice at arg6 = LIMIT slot

  // WRONG -- called inside foreach-all-accounts loop:
  foreach (var acc in allAccounts)
      SubmitBeStop(acc, instr, bePrice, qty);  // submits to every account

  // WRONG -- qty passed from outer context (may include all accounts):
  SubmitBeStop(leaderAcc, instr, bePrice, totalQty);

SAFE:
  // CORRECT -- limitPrice=0 at arg6, stopPrice=bePrice at arg7:
  leaderAcc.CreateOrder(
      instr, direction, OrderType.StopMarket, OrderEntry.Manual,
      TimeInForce.Day, pos.Quantity,  // qty from leader position directly
      0,        // arg6: limitPrice -- always 0 for StopMarket
      bePrice,  // arg7: stopPrice  -- the actual stop price
      "", "PTT-BE-Stop", DateTime.MaxValue,
      (NinjaTrader.Cbi.CustomOrder)null);

  // CORRECT -- called once for leader only, qty from leader's live position:
  var pos = leaderAcc.Positions[instr];
  if (pos == null || pos.Quantity == 0) return;
  SubmitBeStop(leaderAcc, instr, bePrice);  // no qty param -- reads from position inside

SCAN: `acc\.CreateOrder` -- verify arg6 is 0 (limitPrice) and arg7 is stopPrice variable.
      Verify SubmitBeStop called once (leader only), never inside foreach-all-accounts.
      Verify qty comes from leaderAcc.Positions[instr].Quantity inside SubmitBeStop, not passed in.

---

### NT8-051 | P1 | NT8 sim accounts do NOT auto-cancel ATM bracket orders when position goes flat
CONFIRMED: B33 (Director live test 2026-07-21 -- Stop1/Stop2/Target1/Target2 remained Working after PTT-BE-Stop fill)
ERROR: No compiler error. Runtime behaviour differs between sim and live broker.
SYMPTOM: After PTT-BE-Stop fills and position goes flat, Orders tab still shows Stop1/Stop2/Target1/Target2
         in Working state for Sim101/Sim102. If market reopens, bracket orders can create unwanted position.
CAUSE: NT8 internal sim accounts (Sim101/Sim102) do NOT replicate live broker behaviour of
       auto-cancelling ATM bracket orders when position quantity reaches zero.
       Real broker connections (Rithmic, NinjaTrader Brokerage, IBKR) DO auto-cancel.
       This affects ANY ATM-owned bracket order on a sim account -- not specific to PTT.

BANNED:
  // WRONG -- assuming NT8 sim will clean up bracket orders automatically:
  // (no code -- the error is not calling anything, i.e. relying on NT8 to clean up)

SAFE:
  // Explicitly cancel all Working/Accepted orders for the account+instrument after position goes flat.
  // Exclude the PTT-created stop itself (it just filled; cancelling it would error).
  // Pattern: hook into the same flat-position detection used by OrphanCancelGuard.
  private void CancelStaleBrackets(Account leaderAcc, Instrument instr)
  {
      if (leaderAcc == null || instr == null) return;
      var stale = leaderAcc.Orders
          .Where(o => o.Instrument?.FullName == instr.FullName
                   && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
                   && o.Name != "PTT-BE-Stop")
          .ToList();
      if (stale.Count == 0) return;
      try { leaderAcc.Cancel(stale.ToArray()); }
      catch (Exception ex) { /* log */ }
  }
  // Call from TryFirePositionState after OrphanCancelGuard when !hasPos.

NOTE: This only applies to sim accounts. Real broker connections auto-cancel. AddOn cannot
      distinguish sim from live at the API level, but calling Cancel on already-cancelled
      bracket orders on a real broker is a no-op (NT8 ignores cancel requests for non-Working orders).
      Therefore CancelStaleBrackets is safe to call in all cases.

SCAN: After any PTT-BE fill on sim account, verify Orders tab empties. If brackets remain:
      confirm CancelStaleBrackets is hooked to the flat-position detection path.


---

## CATEGORY: NT8 STRATEGY SUBCLASS — NAME COLLISION / STATIC MEMBER ACCESS

### NT8-052 | P0 | `State == State.SetDefaults` / `Calculate == Calculate.OnBarClose` IS BANNED in `Strategy` subclass
CONFIRMED: B42 (CS0176 — first hit writing PttFollowerStrategy.cs 2026-08-05)
ERROR: CS0176 "Member 'NinjaTrader.NinjaScript.NinjaScriptBase.State' cannot be accessed with an instance reference; qualify it with a type name instead"
       CS0176 "Member 'NinjaTrader.NinjaScript.NinjaScriptBase.Calculate' cannot be accessed with an instance reference; qualify it with a type name instead"
CAUSE: In `Strategy` (and `Indicator`) subclasses, `State` and `Calculate` are BOTH an instance
       property (inherited) AND a namespace-level enum. The NT8 Roslyn build resolves the bare
       name as the instance property, then emits CS0176 when it appears on the right-hand side
       of a comparison (where a static/enum value is expected).
       This is the same root cause as NT8-010 (Indicator variant) but manifests identically
       in Strategy subclasses.

BANNED:
  // In any class that inherits NinjaTrader.NinjaScript.StrategyBase:
  if (State == State.SetDefaults) { ... }          // CS0176 on right-hand State
  if (State == State.Realtime) { ... }              // CS0176
  Calculate = Calculate.OnBarClose;                 // CS0176 on right-hand Calculate
  if (Calculate == Calculate.OnEachTick) { ... }    // CS0176

SAFE:
  using NinjaTrader.NinjaScript;
  // Then qualify EVERY right-hand use with the full enum type:
  if (State == NinjaTrader.NinjaScript.State.SetDefaults) { ... }
  if (State == NinjaTrader.NinjaScript.State.Realtime) { ... }
  Calculate = NinjaTrader.NinjaScript.Calculate.OnBarClose;
  if (Calculate == NinjaTrader.NinjaScript.Calculate.OnEachTick) { ... }

NOTE: The LEFT-HAND `State` (instance property) is fine as-is. Only the RIGHT-HAND enum
      values need full qualification. This affects every Strategy subclass in the codebase
      the moment `OnStateChange()` or `Calculate` is used — which is always.

SCAN: `State == State\.` or `Calculate == Calculate\.` or `Calculate = Calculate\.`
      in any *.cs file under src/PropTraderTools/ that inherits Strategy.

---

## CATEGORY: NT8 ADDON vs STRATEGY API BOUNDARY

### NT8-053 | P1 | `CustomOrder` overlay at arg12 of `Account.CreateOrder()` IS SILENTLY IGNORED from AddOn context
CONFIRMED: B42 / ARCH-BRACKET-03 probe (live Sim101 test 2026-08-05)
ERROR: No compiler error. No runtime exception. NT8 silently accepts the order and discards the
       CustomOrder overlay entirely.
SYMPTOM: After `Account.CreateOrder(..., new CustomOrder { IsAutoTrailEnabled=true, AutoTrailSteps=[...] })`
         fills successfully in AddOn context, `WorkingBuys=0 WorkingSells=0` — zero bracket legs spawned.
         Same pattern works correctly from StrategyBase context.
CAUSE: The NT8 broker adapter processes `CustomOrder` overlays only when the call originates from
       a `StrategyBase` execution pipeline (State.Realtime tick). From `AddOnBase` (button-click /
       event handler / non-strategy thread), the overlay is received but not processed — NT8 has
       no attached strategy session to bind the ATM bracket lifecycle to.
       This is the same reason `AtmStrategyCreate()` is not available on `AddOnBase` at all.

BANNED:
  // In AddOnBase / CopyEngine / any non-Strategy class:
  leaderAcc.CreateOrder(instr, action, OrderType.Market, OrderEntry.Manual,
      TimeInForce.Day, qty, 0, 0, "", "PTT-Copy", DateTime.MaxValue,
      new NinjaTrader.Cbi.CustomOrder { IsAutoTrailEnabled = true, AutoTrailSteps = new[] { ... } });
  // ^ CustomOrder is silently DROPPED. No brackets will appear.

SAFE:
  // Option A (AddOn only orders — no brackets): pass (NinjaTrader.Cbi.CustomOrder)null always.
  leaderAcc.CreateOrder(instr, action, OrderType.Market, OrderEntry.Manual,
      TimeInForce.Day, qty, 0, 0, "", "PTT-Copy", DateTime.MaxValue,
      (NinjaTrader.Cbi.CustomOrder)null);

  // Option B (brackets required): use a companion PTTFollowerStrategy instance.
  //   - PTTFollowerStrategy inherits StrategyBase.
  //   - It subscribes to PttBus.FillSignal published by CopyEngine after CreateOrder fills.
  //   - Inside OnMarketData (StrategyBase context), it calls AtmStrategyCreate() which
  //     DOES spawn real ATM brackets — brackets are bound to the strategy's account.
  //   See: src/PropTraderTools/Features/PttFollowerStrategy.cs (B42)

NOTE: The arg12 cast to (CustomOrder)null is still required syntactically (NT8-007).
      The silence of NT8 when a non-null CustomOrder is passed from AddOn is the *additional*
      hazard: there is no error, just missing brackets. This makes the bug extremely hard to
      diagnose without a controlled probe.

SCAN: In non-Strategy .cs files: `new NinjaTrader.Cbi.CustomOrder` or `new CustomOrder {`
      followed by `IsAutoTrailEnabled` — confirm this is never called from AddOn context.

---

## CATEGORY: NT8 BIN/CUSTOM COMPILE SCOPE — XUNIT / TEST ASSEMBLY CONTAMINATION

### NT8-054 | P0 | xUnit test files in `bin\Custom\` cause CS0246 / CS0103 build-break
CONFIRMED: B42 (CS0246 / CS0103 — NT8 compiler scanned B42Tests.cs after hard-link sync 2026-08-05)
ERROR: CS0246 "The type or namespace name 'Xunit' could not be found (are you missing a using directive or an assembly reference?)"
       CS0246 "The type or namespace name 'FactAttribute' could not be found"
       CS0103 "The name 'Assert' does not exist in the current context"
CAUSE: NT8's internal Roslyn host compiles EVERY .cs file found under
       `%Documents%\NinjaTrader 8\bin\Custom\` and its subdirectories.
       xUnit assemblies (xunit.core, xunit.assert) are not referenced by NT8's compiler context.
       Any file that uses `using Xunit;`, `[Fact]`, or `Assert.*` will break the entire NT8 build.

BANNED:
  // WRONG -- placing any test file that references xUnit in the NT8 bin\Custom tree:
  // C:\Users\<user>\Documents\NinjaTrader 8\bin\Custom\AddOns\PropTraderTools\B42Tests.cs
  // C:\Users\<user>\Documents\NinjaTrader 8\bin\Custom\AddOns\PropTraderTools\CopyEngineTests.cs

SAFE:
  // Keep test files in the wave workspace only (src/PropTraderTools/).
  // Exclude them from the hard-link / deploy script via $DeployExcludes:
  //
  // In scripts\verify_links.ps1:
  $DeployExcludes = @("CopyEngineTests.cs", "B42Tests.cs", "B43Tests.cs", "B44Tests.cs")
  //
  // These files are compiled by the .csproj (LSP / CI only) but NEVER synced to NT8 bin.
  // The PropTraderTools.csproj <Compile Include="..."> entries for test files are still
  // needed for the IDE to resolve xUnit types and provide IntelliSense.

NOTE: This applies to ANY file referencing assemblies not in the NT8 compiler reference list:
      xUnit, NUnit, MSTest, Moq, FluentAssertions, BenchmarkDotNet, etc.
      The rule is: if the .csproj needs a NuGet package reference to compile it, it CANNOT
      go into bin\Custom.

SCAN: In scripts\verify_links.ps1 — confirm $DeployExcludes contains all *Tests.cs files.
      After any hard-link sync (`verify_links.ps1 -Fix`), confirm no *Tests.cs appears under
      `%Documents%\NinjaTrader 8\bin\Custom\AddOns\PropTraderTools\`.

### NT8-055 | P1 | `AtmStrategyCreate` is NOT accessible as a static from AddOn (non-StrategyBase) code
CONFIRMED: B53 (CS7036 -- Linting DLL build 2026-08-10)
ERROR: CS7036 "There is no argument given that corresponds to the required parameter 'limitPrice' of
       'StrategyBase.AtmStrategyCreate(OrderAction, OrderType, double, double, TimeInForce, string, string, string, Action<ErrorCode, string>)'"
CAUSE: `NinjaTrader.NinjaScript.AtmStrategy.AtmStrategyCreate` is an INSTANCE method on `StrategyBase`
       in the Linting DLL (`NinjaTrader.Custom.dll` backup). AddOn code (e.g. CopyEngine, which does
       NOT extend StrategyBase) cannot call it as a static. The 2-arg and 3-arg static signatures used
       by Strategy-side code do not exist in the Linting DLL reference surface. The method resolves
       to the 9-arg StrategyBase instance method and fails to match any call site from a non-strategy class.

BANNED:
  // Inside an AddOn class (NOT extending StrategyBase / NinjaScriptBase):
  NinjaTrader.NinjaScript.AtmStrategy.AtmStrategyCreate(templateName, string.Empty);
  // or
  NinjaTrader.NinjaScript.AtmStrategy.AtmStrategyCreate(templateName, entryOrderId, callback);

SAFE (PENDING DIRECTOR RESOLUTION):
  // Gate the call with #if NT8_ADDON_ATM until the correct AddOn ATM API surface is confirmed.
  // The Director must identify whether:
  //   (a) An AddOn can trigger ATM via NinjaTrader.NinjaScript.AtmStrategy (different DLL path), or
  //   (b) ATM must be triggered via a different API (e.g. Account.CreateOrder for bracket legs), or
  //   (c) PttFollowerStrategy (gated by B53 T3) remains the only valid ATM entry point.
  //
  // In B53, the call is gated:
  //   #if NT8_ADDON_ATM
  //   NinjaTrader.NinjaScript.AtmStrategy.AtmStrategyCreate(templateName, string.Empty);
  //   #endif

ESCALATION: B53-LaneA F5-GATE-01 is BLOCKED until Director resolves the correct API.
            Record resolution as NT8-055-RESOLVED with the correct call pattern.

SCAN: grep -r "AtmStrategyCreate" src/PropTraderTools/ | grep -v "#if NT8_ADDON_ATM"
      Any unguarded AtmStrategyCreate call = BLOCKED build risk.
