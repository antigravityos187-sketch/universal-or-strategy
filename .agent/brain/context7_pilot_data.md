# NinjaTrader 8 NinjaScript API — Simulated Index v1.0

> **Feed Type**: Simulated (Web Search Proxy)
> **Date**: 2026-02-10
> **Coverage**: 12 Critical Classes / ~40 Methods / ~60 Properties
> **Purpose**: Eliminate "Coding Slippage" for V12 Restoration

---

## 1. Order Class
**Namespace**: `NinjaTrader.Cbi`
**Description**: Read-only, dynamic interface to an order's current state.

### Properties
| Property | Type | Notes |
|---|---|---|
| `Account` | `Account` | Account the order resides on |
| `AverageFillPrice` | `double` | Average fill price |
| `Filled` | `int` | Quantity filled |
| `FromEntrySignal` | `string` | User-defined entry signal name |
| `Gtd` | `DateTime` | Good Till Date |
| `HasOverfill` | `bool` | Used with `IgnoreOverFill` |
| `Instrument` | `Instrument` | Instrument object |
| `IsBacktestOrder` | `bool` | True if from historical processing |
| `IsLiveUntilCancelled` | `bool` | Survives managed order expiration |
| `LimitPrice` | `double` | Limit price |
| `Name` | `string` | **Key for strategy mgmt** |
| `Oco` | `string` | OCO group ID (suffixed on hist→live transition) |
| `OrderId` | `string` | ⚠️ **Not unique across lifetime** |
| `OrderState` | `OrderState` | Current state enum |
| `StopPrice` | `double` | Stop price |
| `TimeInForce` | `TimeInForce` | Day, Gtc, Gtd |
| `Token` | `string` | Internal tracking token |

### OrderState Enum Values
`Initialized` → `Submitted` → `Accepted` → `Working` → `PartFilled` → `Filled`
`TriggerPending`, `ChangePending`, `ChangeSubmitted`, `CancelPending`, `CancelSubmitted` → `Cancelled`
`Rejected`, `Unknown`

### Key Method
- `IsTerminalState(OrderState)` — Static. Returns true for Filled/Cancelled/Rejected/Unknown.

---

## 2. Account Class
**Namespace**: `NinjaTrader.Cbi`

### Static Properties
| Property | Description |
|---|---|
| `Account.All` | Collection of ALL configured accounts |
| `AccountStatusUpdate` | Event: account status changes |
| `SimulationAccountReset` | Event: sim account resets |

### Instance Properties
| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Account name ("Sim101", "Apex1234") |
| `Orders` | `OrderCollection` | All orders on this account |
| `Positions` | `PositionCollection` | All positions |
| `Executions` | `ExecutionCollection` | All executions |
| `Strategies` | `StrategyCollection` | Running strategies |
| `Connection` | `Connection` | Data connection provider |
| `Denomination` | `Currency` | Account currency |

### Instance Methods
| Method | Signature | Notes |
|---|---|---|
| `Cancel()` | `Cancel(IEnumerable<Order>)` | Cancel specific orders |
| `CancelAllOrders()` | `CancelAllOrders(Instrument)` | Cancel all for instrument |
| `CreateOrder()` | Complex params | Unmanaged: creates Order object |
| `Submit()` | `Submit(IEnumerable<Order>)` | Submits created orders |
| `Flatten()` | `Flatten(IEnumerable<Instrument>)` | Close all positions |
| `Get()` | `Get(AccountItem, Currency)` | Get account value |

### Event Handlers
`OrderUpdate`, `ExecutionUpdate`, `PositionUpdate`, `AccountItemUpdate`

---

## 3. Strategy Class
**Namespace**: `NinjaTrader.NinjaScript.Strategies`
**Inherits**: `StrategyBase`

### Lifecycle: OnStateChange()
| State | When | Use For |
|---|---|---|
| `SetDefaults` | Every init (even UI browse) | Set default property values, keep lightweight |
| `Configure` | After user confirms settings | Add data series, configure resources |
| `DataLoaded` | All data series loaded | Initialize indicators (ATR, RSI) |
| `Historical` | Processing historical bars | — |
| `Transition` | Hist→Live crossover | — |
| `Realtime` | Live data processing | — |
| `Terminated` | Strategy removed/disabled | **Cleanup resources, unsubscribe events** |

### Core Event Methods
| Method | Trigger | Key Notes |
|---|---|---|
| `OnBarUpdate()` | Each bar update (or tick) | Use `BarsInProgress` for multi-series |
| `OnOrderUpdate()` | Order state changes | Track order references here |
| `OnExecutionUpdate()` | Order fills | Best for fill-based logic, use pass-by-value params |
| `OnPositionUpdate()` | Position changes | Fires after OnExecutionUpdate |

### Key Properties
| Property | Type | Notes |
|---|---|---|
| `Account` | `Account` | Trading account |
| `BarsRequiredToTrade` | `int` | Min historical bars before trading |
| `DefaultQuantity` | `int` | Default order size |
| `IsUnmanaged` | `bool` | Enables unmanaged order mode |
| `Position` | `Position` | Current strategy position |
| `Positions` | `Position[]` | Multi-instrument positions |

---

## 4. OnOrderUpdate() — Deep Dive
**Signature**: `OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)`

### Critical Patterns
```csharp
// Track order references
private Order entryOrder = null;

protected override void OnOrderUpdate(Order order, ...)
{
    if (order.Name == "MyEntry")
        entryOrder = order;

    // Handle hist→live transition
    if (order == entryOrder && order.IsBacktestOrder)
        entryOrder = GetRealtimeOrder(order);

    // Detect cancellation
    if (order == entryOrder && orderState == OrderState.Cancelled)
        entryOrder = null; // Clear ghost reference
}
```

### ⚠️ Ghost Order Prevention
- Always null-check order references after `Cancelled`/`Rejected`
- Use `GetRealtimeOrder()` during hist→live transition
- `OrderId` can change — **never use as persistent key**

---

## 5. OnExecutionUpdate() — Deep Dive
**Signature**: `OnExecutionUpdate(Order order, Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)`

### Execution Object Properties
| Property | Type |
|---|---|
| `Account` | `Account` |
| `Commission` | `double` |
| `ExecutionId` | `string` |
| `Instrument` | `Instrument` |
| `MarketPosition` | `MarketPosition` |
| `Name` | `string` |
| `Order` | `Order` |
| `Price` | `double` |
| `Quantity` | `int` |
| `Time` | `DateTime` |

### Best Practice
> Use **pass-by-value** parameters (not the Execution object properties) for Rithmic/IB compatibility. Simultaneous fills can cause stale object state.

---

## 6. Position Class
**Namespace**: `NinjaTrader.Cbi`

### Properties
| Property | Type | Notes |
|---|---|---|
| `MarketPosition` | `MarketPosition` | `.Flat`, `.Long`, `.Short` |
| `Quantity` | `int` | Current position size |
| `AveragePrice` | `double` | Average entry price |
| `Instrument` | `Instrument` | — |

### Multi-Instrument
`Positions[barsInProgressIndex].MarketPosition`

### Account-Level Position
`PositionAccount.MarketPosition` — reflects **entire account** position, not just strategy.

---

## 7. Order Management Methods

### Managed Approach
| Method | Syntax | Notes |
|---|---|---|
| `CancelOrder()` | `CancelOrder(Order order)` | Sends cancel request (not guaranteed) |
| `ChangeOrder()` | `ChangeOrder(Order, int qty, double limit, double stop)` | Amends existing order |
| `SetStopLoss()` | `SetStopLoss(CalculationMode, double value)` | Auto-managed stop |
| `SetProfitTarget()` | `SetProfitTarget(CalculationMode, double value)` | Auto-managed target |
| `SetTrailStop()` | `SetTrailStop(CalculationMode, double value)` | Auto-managed trail |

### CalculationMode Enum
`Currency`, `Percentage`, `Pips`, `Price`, `Ticks`

### ⚠️ Conflict Rule
`SetStopLoss()` overrides `SetTrailStop()` / `SetParabolicStop()` for the same `fromEntrySignal`.

### Unmanaged Approach
| Method | Notes |
|---|---|
| `SubmitOrderUnmanaged()` | Returns `Order` object. Must retain reference. |
| `GetRealtimeOrder()` | **Must call once per order in OnOrderUpdate** during hist→live |
| `IsUnmanaged = true` | Set in `OnStateChange` / `State.SetDefaults` |

---

## 8. Instrument Class
**Namespace**: `NinjaTrader.Cbi`

| Property | Type | Notes |
|---|---|---|
| `Name` | `string` | Symbol name |
| `TickSize` | `double` | Min price increment |
| `PointValue` | `double` | Dollar value per point |
| `InstrumentType` | `InstrumentType` | Stock, Futures, Forex, etc. |
| `MasterInstrument` | `MasterInstrument` | Master config (TickSize, Currency) |
| `Currency` | `Currency` | Trading currency |
| `Description` | `string` | Written description |

> ⚠️ Do NOT access Instrument properties before `State.DataLoaded`.

---

## 9. Indicators: ATR & RSI

### ATR (Average True Range)
```csharp
// Syntax
ATR(int period)
ATR(ISeries<double> input, int period)

// Usage
double currentATR = ATR(14)[0];  // Current bar ATR(14)
double prevATR = ATR(14)[1];     // Prior bar
```

### RSI (Relative Strength Index)
```csharp
// Syntax
RSI(int period, int smooth)
RSI(ISeries<double> input, int period, int smooth)

// Usage
double currentRSI = RSI(14, 3)[0];
```

### Initialization Pattern
```csharp
private ATR atrIndicator;
private RSI rsiIndicator;

protected override void OnStateChange()
{
    if (State == State.DataLoaded)
    {
        atrIndicator = ATR(14);
        rsiIndicator = RSI(14, 3);
    }
}
```

---

## 10. Draw Methods
**Namespace**: `NinjaTrader.NinjaScript.DrawingTools`

| Method | Returns | Use Case |
|---|---|---|
| `Draw.Text()` | `Text` | Text at bar/price coordinate |
| `Draw.TextFixed()` | `TextFixed` | Fixed position text (TopRight, BottomLeft, etc.) |
| `Draw.Line()` | `Line` | Line between two points |
| `Draw.Region()` | `Region` | Shaded area between series |
| `Draw.RegionHighlightY()` | `RegionHighlightY` | Y-axis highlight |

### Tag System
- Unique `tag` string = distinct object
- Reusing same tag updates the existing object
- Append `CurrentBar` for per-bar objects

### Performance Warning
> For 20+ draw objects, use `OnRender()` with SharpDX instead.

---

## 11. AddOn / Panel Development
**Namespace**: `NinjaTrader.NinjaScript.AddOns`

### Architecture
```
AddOnBase → NTWindow → NTTabPage (UI Content)
                     → INTTabFactory (Tab Management)
```

### NTWindow
- Parent container with NinjaTrader styling
- Implement `IWorkspacePersistence` for workspace save/restore

### NTTabPage
- Defines tab content (UI + logic)
- `IInstrumentProvider` for instrument linking
- Methods: `Cleanup()`, `GetHeaderPart()`, `Restore()`, `Save()`
- ⚠️ XAML with inline event handlers will NOT load

### UserControlCollection (Chart Overlay)
- Observable collection on top of `ChartControl`
- Access only after `State.Historical`
- **Must use `Dispatcher`** for UI threading
- Dispose all resources in `State.Terminated`

---

## 12. V12-Specific Cross-Reference

### SIMA (Fleet Management)
- `Account.All` → iterate all accounts
- Filter by `Account.Name` prefix
- `Account.Orders` → monitor per-account orders
- `Account.Flatten()` → emergency position close

### Ghost Order Prevention
- `Order.OrderId` is **mutable** — never use as dictionary key
- Use `Order.Token` or internal tracking `Dictionary<Order, string>`
- Null references on `OrderState.Cancelled` / `OrderState.Rejected`
- Call `GetRealtimeOrder()` exactly once per order on hist→live

### CIT (Chase-If-Touch)
- `ChangeOrder()` to update limit price on touch
- Monitor via `OnOrderUpdate()` for state confirmation

### Deferred Brackets
- Submit stops/targets in `OnExecutionUpdate()` after fill confirmation
- Use `fromEntrySignal` to link brackets to specific entries

### Reaper Audit
- Compare `Position.MarketPosition` across `Account.All`
- Use `Account.Flatten()` for drift correction

### IPC (Panel Communication)
- File-based I/O with `System.IO.File.ReadAllText()` / `WriteAllText()`
- Use `Dispatcher` for UI thread safety
- `UserControlCollection` for chart-embedded panels
