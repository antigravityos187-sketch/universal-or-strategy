# NT8 Full NinjaScript Reference

> **Auto-generated**: 2026-08-10 12:12 UTC
> **Source**: https://developer.ninjatrader.com/docs/desktop/
> **Policy**: Append-only — never delete. Re-scrape to refresh sections.
> **Scope**: AddOn API + Account + Order + ATM Strategy + NinjaScript Strategy

---

## Table of Contents

- **1. AddOn Framework**
  - [AddOn Overview](#add-on)
  - [Developing AddOns](#developing-add-ons)
- **2. Account Class (AddOn Primary Interface)**
  - [Account Class](#account-class)
  - [Account Property (Strategy)](#account)
  - [AccountItem Enum](#accountitem)
  - [AccountItemEventArgs](#accountitemeventargs)
  - [AccountItemUpdate Event](#accountitemupdate)
  - [Accounts.CancelAllOrders()](#accounts-cancelallorders)
  - [AccountStatusUpdate Event](#accountstatusupdate)
- **3. Order Class**
  - [Order Class Overview](#order)
  - [CancelOrder()](#cancelorder)
  - [ChangeOrder()](#changeorder)
  - [CancelAllOrders()](#cancelallorders)
- **4. ATM Strategy Methods**
  - [ATM Strategy Methods Overview](#atm-strategy-methods)
  - [AtmStrategy Class](#atmstrategy)
  - [AtmStrategyCreate()](#atmstrategycreate)
  - [AtmStrategyClose()](#atmstrategyclose)
  - [AtmStrategyCancelEntryOrder()](#atmstrategycancelentryorder)
  - [AtmStrategyChangeEntryOrder()](#atmstrategychangeentryorder)
  - [AtmStrategyChangeStopTarget()](#atmstrategychangestoptarget)
  - [GetAtmStrategyEntryOrderStatus()](#getatmstrategyentryorderstatus)
  - [GetAtmStrategyMarketPosition()](#getatmstrategymarketposition)
  - [AtmStrategySelector (UI Control)](#atmstrategyselector)
- **5. Strategy Overview (NinjaScript)**
  - [Strategy Overview](#strategy-overview)
  - [StrategyBase Class](#strategybase)
- **6. AddOn Controls**
  - [AccountSelector Control](#accountselector)

---

# 1. AddOn Framework

---

## AddOn Overview

> **URL**: https://developer.ninjatrader.com/docs/desktop/add_on
> **Slug**: `add_on`

Custom Add Ons can be used to extend NinjaTrader's functionality. The methods and properties covered in this section are unique to custom Add On development.

For more information on the Add On development process please see [this](developing_add_ons) article.

{% table %}

---

* [NinjaTrader Controls](controls)
* This section contains controls that are native NinjaTrader controls.

---

* [Account](account_class)
* The Account class can be used to subscribe to account related events as well as accessing account related information.

---

* [BarsRequest](barsrequest)
* BarsRequest can be used to request [Bars](bars) data and subscribe to real-time Bars data events.

---

* [Connection](connection_class)
* The Connection class can be used to monitor connection related events as well as accessing connection related information.

---

* [IInstrumentProvider Interface](iinstrumentprovider_interface)
* When creating your [NTTabPage](nttabpage), if you wish to use the [instrument link](https://ninjatrader.com/support/helpGuides/nt8/NT%20HelpGuide%20English.html?linking_windows.htm), be sure to implement the IInstrumentProvider interface.

---

* [IIntervalProvider Interface](iintervalprovider_interface)
* When creating your [NTTabPage](nttabpage), if you wish to use the [interval link](https://ninjatrader.com/support/helpGuides/nt8/NT%20HelpGuide%20English.html?linking_windows.htm), be sure to implement the IIntervalProvider interface.

---

* [INTTabFactory Interface](inttabfactory_interface)
* If you wish to have tab page functionality like adding, removing, moving, duplicating tabs you must create a class which implements the INTTabFactory interface.

---

* [IWorkspacePersistence Interface](iworkspacepersistence_interface)
* When creating your [NTWindow](ntwindow), be sure to implement the IWorkspacePersistence interface as well for the ability to save and restore your window with NinjaTrader workspaces.

---

* [NTTabPage Class](nttabpage)
* This is where the actual content for tabs inside the custom add on [NTWindow](ntwindow) can be defined.

---

* [Alert and Debug Concepts](alert_and_debug_concepts)
* In most scenarios you can use the NinjaScript provided methods for triggering alerts and debugging functionality. However, when building your own custom objects, you may find yourself wanting to use this functionality outside the NinjaScript scope.

---

* [AtmStrategy](atmstrategy)
* AtmStrategy contains properties and methods used to manage [ATM Strategies](atm_strategy_methods).

---

* [ControlCenter](controlcenter)
* ControlCenter is a XAML-defined class containing the layout and properties of the Control Center window.

---

* [FundamentalData](fundamentaldata)
* FundamentalData is used to access fundamental snapshot data and for subscribing to fundamental data events.

---

* [MarketData](marketdata)
* MarketData can be used to access snapshot market data and for subscribing to market data events.

---

* [MarketDepth](marketdepth)
* MarketDepth can be used to access snapshot market depth and for subscribing to market depth events.

---

* [NewsItems](newsitems)
* NewsItems can be used to store news articles.

---

* [NewsSubscription](newssubscription)
* NewsSubscription can be used for subscribing to News events.

---

* [NTMenuItem](ntmenuitem)
* NTMenuItem is used to create new menu entries.

---

* [NTWindow](ntwindow)
* The NTWindow class defines parent windows for custom window creation. Instances of NTWindow act as containers for instances of [NTTabPage](nttabpage), in which UI elements and their related logic are contained.

---

* [NumericTextBox](numerictextbox)
* NumericTextBox provides functionality for numeric text boxes to capture user input.

---

* [OnWindowCreated()](onwindowcreated)
* This method is called whenever a new [NTWindow](ntwindow) is created.

---

* OnWindowDestroyed()
* This method is called whenever a new [NTWindow](ntwindow) is destroyed.

---

* [OnWindowRestored()](onwindowrestored)
* This method is used to recall any custom XElement data from the workspace by referencing a window.

---

* [OnWindowSaved()](onwindowsaved)
* This method is used to save any custom XElement data associated with your window.

---

* [StartAtmStrategy()(startatmstrategy)
* StartAtmStrategy can be used to submit entry orders with ATM strategies.

---

* [StrategyBase](strategybase)
* StrategyBase contains properties and methods for managing a [Strategy](strategy) object, and is the base class from which [AtmStrategy](atmstrategy) derives.

---

* [PropagateInstrumentChange()](propagateinstrumentchange)
* In an [NTWindow](ntwindow), PropagateInstrumentChange() sends an Instrument to other windows with the same Instrument Linking color configured.

---

* [PropagateIntervalChange()](propagateintervalchange)
* In an [NTWindow](ntwindow), PropagateIntervalChange() sends an interval to other windows with the same Interval Linking color configured.

---

* [TabControl](tabcontrol)
* The TabControl class provides functionality for working with [NTTabPage](nttabpage) objects within an [NTWindow](ntwindow).

---

* [TabControlManager](tabcontrolmanager)
* The TabControlManager class can be used to set or check several properties of a [TabControl](tabcontrol) object.
{% /table %}b:[["$","h1",null,{"className":"docTitle","children":"Add On"}],["$","$L19",null,{"source":"$1a"}]]

---

## Developing AddOns

> **URL**: https://developer.ninjatrader.com/docs/desktop/developing_add_ons
> **Slug**: `developing_add_ons`

## Add Ons Overview

Add Ons are incredibly powerful **NinjaScript** objects that let you create unprecedented tools which are seamlessly integrated (visually and functionally) into **NinjaTrader**. Experienced programmers can leverage the information available through the framework to create exciting new windows and utilities that can give users an incredible edge over the markets.

## How to make Add Ons

The process to make an Add On is fairly simple once the structure is understood. A few questions should be answered to determine how to build your Add On:

1. Where should the entry point for the Add On be? E.g. Should it be launched from the Control Center menus? Should it be launched from a Chart?
2. Should the Add On leverage the tab functionality available in **NinjaTrader**?
3. Should the Add On leverage the window linking functionality available in **NinjaTrader**?
4. Should the Add On be persisted in **NinjaTrader** workspaces?

Once the functionality of your Add On is determined you can use the following building blocks to create your Add On:

{% table %}
* Property/Class
* Description
---
* **AddOnBase**
* This is where you create the entry point for the Add On.
---
* **NTWindow**
* This is where you define the parent window container for your Add On. Tabs would reside within this parent window should you choose. This is also where workspace persistence would be created.
---
* **NTTabPage**
* This is where you define the content of each tab that resides inside **NTWindow**. This is also where you create the window linking functionality.
---
* Class implementing the **INTTabFactory** interface
* This is necessary to ensure proper tab functionality like adding, removing, moving tabs around in your **NTWindow**.
---
{% /table %}

The general flow goes from **AddOnBase** > **NTWindow** > **INTTabFactory** > **NTTabPage**.

**AddOnBase** determines the user entry point and then creates the event handler to create the **NTWindow**. **NTWindow** calls the tab factory which then brings in the **NTTabPage** content in the form of tabs into **NTWindow**.b:[["$","h1",null,{"className":"docTitle","children":"Developing Add Ons"}],["$","$L19",null,{"source":"$1a"}]]

# 2. Account Class (AddOn Primary Interface)

---

## Account Class

> **URL**: https://developer.ninjatrader.com/docs/desktop/account_class
> **Slug**: `account_class`

## Definition

The Account class can be used to subscribe to account-related events as well as access account-related information.

{% callout type="note" %}

Also happens when rewinding/fast forwarding Playback connections.

{% /callout %}

## Static Account Class Properties

{% table %}

* Property
* Description

---

* **All**
* A collection of Account objects

---

* **[AccountStatusUpdate](accountstatusupdate)**
* Event handler for account status updates

---

* **[SimulationAccountReset](simulationaccountreset)**
* Event handler for resets on sim accounts
{% /table %}

## Methods and Properties From Account instances

{% table %}

* Property
* Description

---

* **[AccountItem](accountitem)**
* Represents various account variables used to reflect values the status of the account

---

* **[AccountItemUpdate](accountitemupdate)**
* Event handler for changes to account values

---

* **[Cancel()](cancel)**
* Cancels specified order(s) on the account

---

* **[CancelAllOrders()](accounts_cancelallorders)**
* Cancels all orders of an instrument on the account

---

* **[Change()](change)**
* Changes specified order(s) on the account

---

* **[Connection](connection)**
* A Connection representing the connection this account is associated with

---

* **[CreateOrder()](createorder)**
* Creates orders for the account that need to be submitted via Submit()

---

* **[Denomination](denomination)**
* A Currency representing the denomination currency of this connection

---

* **[Executions](execution)**
* A collection of executions on this account

---

* **[ExecutionUpdate](executionupdate)**
* Event handler for when new executions come in, an existing execution is amended, or an execution is removed

---

* **[Flatten()](flatten)**
* Flattens the account on specified instrument(s)

---

* **[Get()](get)**
* Returns the value of an **[AccountItem](accountitem)**

---

* **[Name](name_account)**
* A string representing the name of this account

---

* **[Orders](orders)**
* A collection of orders on this account

---

* **[OrderUpdate](orderupdate)**
* Event handler for changes to orders

---

* **[Positions](positions)**
* A collection of positions on this account

---

* **[PositionUpdate](positionupdate)**
* Event handler for changes to positions

---

* **[Strategies](strategies)**
* A collection of strategies on this account

---

* **[Submit()](submit)**
* Submits specified order(s)
{% /table %}

## Example

```csharp
private Account myAccount;

protected override void OnStateChange()
{
    if (State == State.SetDefaults)
    {
        // Find our Sim101 account
        lock (Account.All)
            myAccount = Account.All.FirstOrDefault(a => a.Name == "Sim101");

        // Subscribe to static events. Remember to unsubscribe with -= when you are done
        Account.AccountStatusUpdate += OnAccountStatusUpdate;

        if (myAccount != null)
        {
            // Print some information about our account using the AccountItem indexer
            Print(string.Format("Account Name: {0} Connection Name: {1} Cash Value {2}",
                myAccount.Name,
                myAccount.Connection.Options.Name,
                myAccount.Get(AccountItem.CashValue, Currency.UsDollar)));

            // Print the prices of the executions on our account
            lock (myAccount.Executions)
                foreach (Execution execution in myAccount.Executions)
                    Print("Price: " + execution.Price);

            // Subscribe to events. Remember to unsubscribe with -= when you are done
            myAccount.AccountItemUpdate += OnAccountItemUpdate;
            myAccount.ExecutionUpdate += OnExecutionUpdate;
        }
    }
    else if (State == State.Terminated)
    {
        // Unsubscribe to events
        myAccount.AccountItemUpdate -= OnAccountItemUpdate;
        myAccount.ExecutionUpdate -= OnExecutionUpdate;
        Account.AccountStatusUpdate -= OnAccountStatusUpdate;
    }
}

private void OnAccountStatusUpdate(object sender, AccountStatusEventArgs e)
{
    // Do something with the account status update
}

private void OnAccountItemUpdate(object sender, AccountItemEventArgs e)
{
    // Do something with the account item update
}

private void OnExecutionUpdate(object sender, ExecutionEventArgs e)
{
    // Do something with the execution update
}
```b:[["$","h1",null,{"className":"docTitle","children":"Account Class"}],["$","$L19",null,{"source":"$1a"}]]

---

## Account Property (Strategy)

> **URL**: https://developer.ninjatrader.com/docs/desktop/account
> **Slug**: `account`

## Definition

Represents the real-world or simulation **Account** configured for the strategy.

## Property Value

An [Account](account_class) object configured for the strategy.

## Syntax

`Account`

## Examples

```csharp
// Displays text on chart indicating what account the strategy is applied to
Draw.TextFixed(this, "tag1", "Strategy is applied to " + Account.Name, TextPosition.BottomRight);
```

---

## AccountItem Enum

> **URL**: https://developer.ninjatrader.com/docs/desktop/accountitem
> **Slug**: `accountitem`

# Definition

Represents various account variables used to reflect values the status of the account. Each account connected in NinjaTrader will have it's own unique AccountItem values.

{% callout type="note" %}

For strategies, see also [OnAccountItemUpdate](onaccountitemupdate). For other objects, you can also subscribe to the [AccountItemUpdate](accountitemupdate) stream.
{% /callout %}

## Syntax

`AccountItem`

## Parameters

* AccountItem.BuyingPower
* AccountItem.CashValue
* AccountItem.Commission
* AccountItem.ExcessIntradayMargin
* AccountItem.ExcessInitialMargin
* AccountItem.ExcessMaintenanceMargin
* AccountItem.ExcessPositionMargin
* AccountItem.Fee
* AccountItem.GrossRealizedProfitLoss
* AccountItem.InitialMargin
* AccountItem.IntradayMargin
* AccountItem.LongOptionValue
* AccountItem.LookAheadMaintenanceMargin
* AccountItem.LongStockValue
* AccountItem.MaintenanceMargin
* AccountItem.NetLiquidation
* AccountItem.NetLiquidationByCurrency
* AccountItem.PositionMargin
* AccountItem.RealizedProfitLoss
* AccountItem.ShortOptionValue
* AccountItem.ShortStockValue
* AccountItem.SodCashValue
* AccountItem.SodLiquidatingValue
* AccountItem.UnrealizedProfitLoss
* AccountItem.TotalCashBalanceb:[["$","h1",null,{"className":"docTitle","children":"AccountItem"}],["$","$L19",null,{"source":"$1a"}]]

---

## AccountItemEventArgs

> **URL**: https://developer.ninjatrader.com/docs/desktop/accountitemeventargs
> **Slug**: `accountitemeventargs`

## Definition

**AccountItemEventArgs** contains **Account**-related information to be passed as an argument to the **OnAccountItemUpdate** event.

{% callout type="note" %}

For a complete, working example of this class in use, download framework example located on our **Developing AddOns Overview**.

{% /callout %}

The properties listed below are accessible from an instance of AccountItemEventArgs:

{% table %}

* Account
* The Account for which OnAccountItemUpdate() was called

---

* AccountItem
* The **AccountItem** which has updated, resulting in the call to OnAccountItemUpdate()

---

* Currency
* The currency of the Account in question

---

* Time
* A DateTime object representing the time at which the change occurred

---

* Value
* The new value of the updated AccountItems
{% /table %}

## Example

```csharp
// This method is fired on any change of an AccountItem
private void OnAccountItemUpdate(object sender, AccountItemEventArgs e)
{
    /* Dispatcher.InvokeAsync() is needed for multi-threading considerations. When processing events outside of the UI thread, and we want to
    influence the UI .InvokeAsync() allows us to do so. It can also help prevent the UI thread from locking up on long operations. */
    Dispatcher.InvokeAsync(() =>
    {
        //Print which AccountItem changed, on which account, and the new value, using
        outputBox.AppendText(string.Format("{0}Account: {1}{0}AccountItem: {2}{0}Value: {3}",
            Environment.NewLine,
            e.Account.Name,
            e.AccountItem,
            e.Value));
    });
}b:[["$","h1",null,{"className":"docTitle","children":"AccountItemEventArgs"}],["$","$L19",null,{"source":"$1a"}]]

---

## AccountItemUpdate Event

> **URL**: https://developer.ninjatrader.com/docs/desktop/accountitemupdate
> **Slug**: `accountitemupdate`

## Definition

AccountItemUpdate is used for subscribing to account item update events.

{% callout type="note" %}

Remember to unsubscribe if you are no longer using the subscription.
{% /callout %}

## Syntax

`AccountItemUpdate`

## Example

```csharp
/* Example of subscribing/unsubscribing to account item update events from an Add On. The concept can be carried over
to any NinjaScript object you may be working on. */
public class MyAddOnTab : NTTabPage
{
     private Account account;
     public MyAddOnTab()
     {
          // Find our Sim101 account
         lock (Account.All)
               account = Account.All.FirstOrDefault(a => a.Name == "Sim101");

          // Subscribe to account item updates
         if (account != null)
               account.AccountItemUpdate += OnAccountItemUpdate;
     }

     // This method is fired on any change of an account value
     private void OnAccountItemUpdate(object sender, AccountItemEventArgs e)
     {
          // Output the account item
          NinjaTrader.Code.Output.Process(string.Format("Account: {0} AccountItem: {1} Value: {2}",
               e.Account.Name, e.AccountItem, e.Value), PrintTo.OutputTab1);
     }

     // Called by TabControl when tab is being removed or window is closed
     public override void Cleanup()
     {
          // Make sure to unsubscribe to the account item subscription
         if (account != null)
               account.AccountItemUpdate -= OnAccountItemUpdate;
     }

     // Other required NTTabPage members left out for demonstration purposes. Be sure to add them in your own code.
}b:[["$","h1",null,{"className":"docTitle","children":"AccountItemUpdate"}],["$","$L19",null,{"source":"$1a"}]]

---

## Accounts.CancelAllOrders()

> **URL**: https://developer.ninjatrader.com/docs/desktop/accounts_cancelallorders
> **Slug**: `accounts_cancelallorders`

## Definition

Cancels all [Order](order) of an instrument.

## Syntax

`CancelAllOrders(Instrument instrument)`

## Parameters

{% table %}

---

* instrument
* Instrument of the orders to be cancelled

---

{% /table %}

## Example

```csharp
private Account myAccount;

protected override void OnStateChange()
{
    if (State == State.SetDefaults)
    {
        // Initialize myAccount
    }
}

private void OnExecutionUpdate(object sender, ExecutionEventArgs e)
{
    // Cancel all orders if an execution is triggered after 9pm
    if (e.Time > new DateTime(now.Year, now.Month, now.Day, 21, 0, 0))
        myAccount.CancelAllOrders(e.Execution.Instrument);
}
```

---

## AccountStatusUpdate Event

> **URL**: https://developer.ninjatrader.com/docs/desktop/accountstatusupdate
> **Slug**: `accountstatusupdate`

## Definition

AccountStatusUpdate can be used for subscribing to account status events from all accounts.

{% callout type="note" %}

Remember to unsubscribe if you are no longer using the subscription.
{% /callout %}

## Syntax

`AccountStatusUpdate`

## Examples

```csharp
/* Example of subscribing/unsubscribing to account status update events from an Add On. The concept can be carried over
to any NinjaScript object you may be working on. */
public class MyAddOnTab : NTTabPage
{
     public MyAddOnTab()
     {
          // Subscribe to account status updates
          Account.AccountStatusUpdate += OnAccountStatusUpdate;
     }

     // This method is fired on any status change of any account
     private void OnAccountStatusUpdate(object sender, AccountStatusEventArgs e)
     {
          // Output the account name and status
          NinjaTrader.Code.Output.Process(string.Format("Account: {0} Status: {1}",
               e.Account.Name, e.Status), PrintTo.OutputTab1);
     }

     // Called by TabControl when tab is being removed or window is closed
     public override void Cleanup()
     {
          // Make sure to unsubscribe to the account status subscription
          Account.AccountStatusUpdate -= OnAccountStatusUpdate;
     }

     // Other required NTTabPage members left out for demonstration purposes. Be sure to add them in your own code.
}b:[["$","h1",null,{"className":"docTitle","children":"AccountStatusUpdate"}],["$","$L19",null,{"source":"$1a"}]]

# 3. Order Class

---

## Order Class Overview

> **URL**: https://developer.ninjatrader.com/docs/desktop/order
> **Slug**: `order`

## Definition

Represents a read only interface that exposes information regarding an order.

* An Order object returned from calling an order method is dynamic in that its properties will always reflect the current state of an order.
* The property **<`order`>.OrderId** is NOT a unique value, since it can change throughout an order's lifetime. Please see the [Advance Order Handling](advanced_order_handling) section on "Transitioning order references from historical to live" for details on how to handle.
* The property **<`order`>.Oco** WILL be appended with a suffix when the strategy transitions from historical to real-time to ensure the OCO id is unique across multiple strategies for live orders.
* To check for equality you can compare Order objects directly.

## Methods and Properties

{% table %}

* Parameter
* Description

---

* Account
* The [Account](account_class) the order resides

---

* AverageFillPrice
* A double value representing the average fill price of an order

---

* Filled
* An int value representing the filled amount of an order

---

* FromEntrySignal
* A string representing the user defined fromEntrySignal parameter on an order

---

* Gtd
* A [DateTime](http://msdn2.microsoft.com/en-us/library/system.datetime.aspx) structure representing when the order will be canceled

---

* HasOverfill
* A bool value representing if the order is an overfill. For use when using [Unmanaged orders](unmanaged_approach) and [IgnoreOverFill](ignoreoverfill)

---

* Instrument
* An [Instrument](instrument) value representing the instrument of an order

---

* IsBacktestOrder
* A bool that indicates if the order was generated while processing historical data. For use with [GetRealtimeOrder()](getrealtimeorder) when transitioning historical order objects to live order objects when strategies transition to from State.Historical to State.Realtime.

---

* IsLiveUntilCancelled
* A bool that when true, indicates the order will be canceled by [managed order handling](managed_approach) at expiration

---

* IsTerminalState()
* A static method used to determine if the an order's OrderState is in considered terminal and no longer active

---

* LimitPrice
* A double value representing the limit price of an order

---

* LimitPriceChanged
* A double value representing the new limit price of an order. Used with [Account.Change()](change)

---

* Name
* A string representing the name of an order which can be provided by the entry or exit signal name

---

* Oco
* A string representing the OCO (one cancels other) id of an order

---

* OrderAction
* Represents the action of the order. Possible values are:
  * OrderAction.Buy
  * OrderAction.BuyToCover
  * OrderAction.Sell
  * OrderAction.SellShort

---

* OrderId
* A string representing the broker issued order id value (this value can change)

---

* OrderState
* The current state of the order. See the order state values table below

---

* OrderType
* The type of order submitted. Possible values are:
  * OrderType.Limit
  * OrderType.Market
  * OrderType.MIT
  * OrderType.StopMarket
  * OrderType.StopLimit

---

* Quantity
* An int value representing the quantity of an order

---

* QuantityChanged
* An int value representing the new quantity of an order. Used with [Account.Change()](change)

---

* StopPrice
* A double value representing the stop price of an order

---

* StopPriceChanged
* A double value representing the new stop price of an order. Used with [Account.Change()](change)

---

* Time
* A [DateTime](http://msdn2.microsoft.com/en-us/library/system.datetime.aspx) structure representing the last time the order changed state

---

* TimeInForce
* Determines the life of the order. Possible values are:
  * TimeInForce.Day
  * TimeInForce.Gtc

---

* ToString()
* A string representation of an order

---

{% /table %}

## OrderState Values

{% table %}

* Order State
* Description

---

* OrderState.Initialized
* Order is initialized in NinjaTrader

---

* OrderState.Submitted
* Order is submitted to the broker

---

* OrderState.Accepted
* Order is accepted by the broker or exchange

---

* OrderState.TriggerPending
* Order is pending submission

---

* OrderState.Working
* Order is working in the exchange queue

---

* OrderState.ChangePending
* Order change is pending in NinjaTrader

---

* OrderState.ChangeSubmitted
* Order change is submitted to the broker

---

* OrderState.CancelPending
* Order cancellation is pending in NinjaTrader

---

* OrderState.CancelSubmitted
* Order cancellation is submitted to the broker

---

* OrderState.Cancelled
* Order cancellation is confirmed by the exchange

---

* OrderState.Rejected
* Order is rejected

---

* OrderState.PartFilled
* Order is partially filled

---

* OrderState.Filled
* Order is completely filled

---

* OrderState.Unknown
* An unknown order state. Default if broker does not report current order state.

---

{% /table %}

{% callout type="note" %}

Critical: In a historical backtest, orders will always reach a "Working" state. In real-time, some stop orders may only reach "Accepted" state if they are simulated/held on a brokers server.

{% /callout %}

## Examples

```csharp
private Order entryOrder = null;
  
protected override void OnBarUpdate()
{
   if (entryOrder == null && Close[0] > Open[0])
       EnterLong("myEntryOrder");
}
  
protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
{
   // Assign entryOrder in OnOrderUpdate() to ensure the assignment occurs when expected.
   // This is more reliable than assigning Order objects in OnBarUpdate, as the assignment is not guaranteed to be complete if it is referenced immediately after submitting
   if (order.Name == "myEntryOrder")
      entryOrder = order;
  
   if (entryOrder != null && entryOrder == order)
   {
       Print(order.ToString());
       if (order.OrderState == OrderState.Filled)
           entryOrder = null;
   }
}
```b:[["$","h1",null,{"className":"docTitle","children":"Order"}],["$","$L19",null,{"source":"$1a"}]]

---

## CancelOrder()

> **URL**: https://developer.ninjatrader.com/docs/desktop/cancelorder
> **Slug**: `cancelorder`

## Definition

Cancels a specified order. This method is reserved for experienced programmers that fully understand the concepts of advanced order handling.

{% callout type="note" %}

Notes:

1. This method sends a cancel request to the broker and does not guarantee that an order is completely cancelled. Most of the time you can expect your order to come back 100% cancelled.
2. An order can be completely filled or part filled in the time that you send the cancel request and the time the exchange receives the request. Check the **OnOrderUpdate()** method for the state of an order you attempted to cancel.
{% /callout %}

## Syntax

`CancelOrder(Order order)`

{% callout type="warning" %}

Warning: If you have existing historical `order` references which have transitioned to real-time, you MUST update the order object reference to the newly submitted real-time order; otherwise errors may occur as you attempt to cancel the order. You may use the `GetRealtimeOrder()` helper method to assist in this transition.

{% /callout %}

## Parameters

{% table %}

* Parameter
* Description

---

* **order**
* An **Order** object representing the order you wish to cancel.

---

{% /table %}

## Examples

```csharp
private Order myEntryOrder = null;
private int barNumberOfOrder = 0;

protected override void OnBarUpdate()
{
    // Submit an entry order at the low of a bar
    if (myEntryOrder == null)
    {
        // use 'live until canceled' limit order to prevent default managed order handling which would expire at end of bar
        EnterLongLimit(0, true, 1, Low[0], "Long Entry");
        barNumberOfOrder = CurrentBar;
    }

    // If more than 5 bars has elapsed, cancel the entry order
    if (CurrentBar > barNumberOfOrder + 5)
        CancelOrder(myEntryOrder);
}

protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled,
    double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
{
    // Assign entryOrder in OnOrderUpdate() to ensure the assignment occurs when expected.
    // This is more reliable than assigning Order objects in OnBarUpdate, as the assignment is not guaranteed to be complete if it is referenced immediately after submitting
    if (order.Name == "Long Entry")
        myEntryOrder = order;

    // Evaluates for all updates to myEntryOrder.
    if (myEntryOrder != null && myEntryOrder == order)
    {
        // Check if myEntryOrder is cancelled.
        if (myEntryOrder.OrderState == OrderState.Cancelled)
        {
            // Reset myEntryOrder back to null
            myEntryOrder = null;
        }
    }
}
```b:[["$","h1",null,{"className":"docTitle","children":"CancelOrder()"}],["$","$L19",null,{"source":"$1a"}]]

---

## ChangeOrder()

> **URL**: https://developer.ninjatrader.com/docs/desktop/changeorder
> **Slug**: `changeorder`

## Definition

Amends a specified **Order**.

{% callout type="note" %}

This method is only relevant for Managed orders with IsLiveUntilCancelled set to true and Unmanaged orders.

{% /callout %}

## Syntax

`ChangeOrder(Order order, int quantity, double limitPrice, double stopPrice)`

{% callout type="warning" %}

If you have existing historical `order` references which have transitioned to real-time, you MUST update the order object reference to the newly submitted real-time order; otherwise errors may occur as you attempt to change the order. You may use the `GetRealtimeOrder()` helper method to assist in this transition.

{% /callout %}

## Parameters

{% table %}

---

* order
* **Order object** of the order you wish to amend

---

* quantity
* Order quantity

---

* limitPrice
* Order limit price. Use "0" should this parameter be irrelevant for the OrderType being submitted.

---

* stopPrice
* Order stop price. Use "0" should this parameter be irrelevant for the OrderType being submitted.

---

{% /table %}

## Examples

```csharp
private Order stopOrder = null;

protected override void OnBarUpdate()
{
    // Raise stop loss to breakeven when you are at least 4 ticks in profit
    if (stopOrder != null && stopOrder.StopPrice < Position.AveragePrice && Close[0] >= Position.AveragePrice + 4 * TickSize)
        ChangeOrder(stopOrder, stopOrder.Quantity, 0, Position.AveragePrice);
}
```b:[["$","h1",null,{"className":"docTitle","children":"ChangeOrder()"}],["$","$L19",null,{"source":"$1a"}]]

---

## CancelAllOrders()

> **URL**: https://developer.ninjatrader.com/docs/desktop/cancelallorders
> **Slug**: `cancelallorders`

## Definition

Cancels all orders for the specified instrument on the connection.

## Syntax

`<connection>.CancelAllOrders(Instrument instrument)`

{% table %}

---

* instrument
* An Instrument object used to identify the instrument for which to cancel orders

---

{% /table %}

## Examples

```csharp
private Account myAccount;

protected override void OnStateChange()
{
    if (State == State.SetDefaults)
    {
        // Initialize myAccount
    }
}

private void OnExecutionUpdate(object sender, ExecutionEventArgs e)
{
    // Cancel all orders if an execution is triggered after 9pm
    if (e.Time > new DateTime(now.Year, now.Month, now.Day, 21, 0, 0))
        myAccount.CancelAllOrders(e.Execution.Instrument);
}
```

# 4. ATM Strategy Methods

---

## ATM Strategy Methods Overview

> **URL**: https://developer.ninjatrader.com/docs/desktop/atm_strategy_methods
> **Slug**: `atm_strategy_methods`

## ATM Strategy Methods

From a NinjaScript strategy it is possible to use ATM strategies to manage your positions. Benefit of such an approach is that you can use the NinjaScript strategy to generate automated entry signals and once entered, you can delegate exit management to an ATM strategy which allows you degrees of manual control over how to close out of a trade.

For more information please see the [Using ATM Strategies](using_atm_strategies).

## ATM Strategy Management

* [AtmStrategyCancelEntryOrder()](atmstrategycancelentryorder)
* [AtmStrategyChangeEntryOrder()](atmstrategychangeentryorder)
* [AtmStrategyChangeStopTarget()](atmstrategychangestoptarget)
* [AtmStrategyClose()](atmstrategyclose)
* [AtmStrategyCreate()](atmstrategycreate)

## ATM Strategy Monitoring

* [GetAtmStrategyEntryOrderStatus()](getatmstrategyentryorderstatus)
* [GetAtmStrategyMarketPosition()](getatmstrategymarketposition)
* [GetAtmStrategyPositionAveragePrice()](getatmstrategypositionaverageprice)
* [GetAtmStrategyPositionQuantity()](getatmstrategypositionquantity)
* [GetAtmStrategyRealizedProfitLoss()](getatmstrategyrealizedprofitloss)
* [GetAtmStrategyStopTargetOrderStatus()](getatmstrategystoptargetorderstatus)
* [GetAtmStrategyUniqueId()](getatmstrategyuniqueid)
* [GetAtmStrategyUnrealizedProfitLoss()](getatmstrategyunrealizedprofitloss)b:[["$","h1",null,{"className":"docTitle","children":"ATM Strategy Methods"}],["$","$L19",null,{"source":"$1a"}]]

---

## AtmStrategy Class

> **URL**: https://developer.ninjatrader.com/docs/desktop/atmstrategy
> **Slug**: `atmstrategy`

AtmStrategy contains properties and methods used to manage [ATM Strategies](atm_strategy_methods). When working with an [AtmStrategySelector](atmstrategyselector), selected objects can be case to AtmStrategy to obtain or change their properties.

{% callout type="note" %}

1. For a complete, working example of this class in use, download framework example located on our [Developing AddOns Overview](developing_add_ons)
2. For more information on working with the ATM strategies programmatically in general, please see the [Using ATM Strategies](using_atm_strategies) section.
{% /callout %}

## Example

```csharp
// Using AtmStrategy to handle user selections in an ATM Strategy Selector
myAtmStrategySelector.SelectionChanged += (o, args) =>
{
   if (myAtmStrategySelector.SelectedItem == null)
       return;
   if (args.AddedItems.Count > 0)
   {
       // Change the selected TIF in a TIF selector based on what is selected in the ATM Strategy Selector
       NinjaTrader.NinjaScript.AtmStrategy selectedAtmStrategy = args.AddedItems[0] as NinjaTrader.NinjaScript.AtmStrategy;
       if (selectedAtmStrategy != null)
       {
           myTifSelector.SelectedTif = selectedAtmStrategy.TimeInForce;
       }
   }
};
```b:[["$","h1",null,{"className":"docTitle","children":"AtmStrategy"}],["$","$L19",null,{"source":"$1a"}]]

---

## AtmStrategyCreate()

> **URL**: https://developer.ninjatrader.com/docs/desktop/atmstrategycreate
> **Slug**: `atmstrategycreate`

## Definition

Submits an entry order that will execute a specified ATM Strategy.

{% callout type="note" %}

Please review the section on using [ATM Strategies](using_atm_strategies). This method is NOT backtestable and will NOT execute on historical data. See the [AtmStrategyCancelEntryOrder()](atmstrategycancelentryorder) to cancel an entry order. See the [AtmStrategyChangeEntryOrder()](atmstrategychangeentryorder) to change the price of the entry order. The ATM Strategy will be created asyncronous on the hosting NinjaScripts UI Thread, a callback is provided solely to check when the ATM Strategy is started on that thread - accessing for example price data in that outside OnBarUpdate() context is not possible. Please see the SampleATMStrategy build into NinjaTrader for example usage.

{% /callout %}

## Method Return Value

This method does not return a value

## Syntax

`AtmStrategyCreate(OrderAction action, OrderType orderType, double limitPrice, double stopPrice, TimeInForce timeInForce, string orderId, string strategyTemplateName, string atmStrategyId, Action<ErrorCode, string> callback)`

## Parameters

{% table %}

---
* action
* Sets if the entry order is a buy or sell order. Possible values are: OrderAction.Buy, OrderAction.Sell

---

* orderType
* Sets the order type of the entry order. Possible values are: OrderType.Limit, OrderType.Market, OrderType.MIT, OrderType.StopMarket, OrderType.StopLimit

---
* limitPrice
* The limit price of the order

---

* stopPrice
* The stop price of the order

---

* timeInForce
* Sets the time in force of the entry order. Possible values are: TimeInForce.Day, TimeInForce.Gtc

---

* orderId
* The unique identifier for the entry order

---

* strategyTemplateName
* Specifies which strategy template will be used

---

* atmStrategyId
* The unique identifier for the ATM strategy

---

* callback
* The callback action is used to check that the ATM Strategy is successfully started

---

{% /table %}

{% callout type="note" %}

Tip: Unlike NinjaScript Strategy orders (both [managed](managed_approach) and [unmanaged](unmanaged_approach)), ATM strategies generated by the AtmStrategyCreate() method can then be managed manually by any order entry window such as the SuperDOM or within your NinjaScript strategy.

{% /callout %}

## Examples

```csharp
private string atmStrategyId;
private string atmStrategyOrderId;
private bool   isAtmStrategyCreated = false;

protected override void OnBarUpdate()
{
   if (State < State.Realtime)
       return;

   if (Close[0] > SMA(20)[0])
   {
       atmStrategyId = GetAtmStrategyUniqueId();
       atmStrategyOrderId = GetAtmStrategyUniqueId();

       AtmStrategyCreate(OrderAction.Buy, OrderType.Market, 0, 0, TimeInForce.Day,
           atmStrategyOrderId, "MyTemplate", atmStrategyId, (atmCallbackErrorCode, atmCallbackId) => {

           // checks that the call back is returned for the current atmStrategyId stored
           if (atmCallbackId == atmStrategyId)
           {
               // check the atm call back for any error codes
               if (atmCallbackErrorCode == ErrorCode.NoError)
               {
                   // if no error, set private bool to true to indicate the atm strategy is created
                   isAtmStrategyCreated = true;
               }
           }
       });
   }

   if(isAtmStrategyCreated)
   {
       // atm logic
   }

   else if(!isAtmStrategyCreated)
   {
       // custom handling for a failed atm Strategy
   }
}
```b:[["$","h1",null,{"className":"docTitle","children":"AtmStrategyCreate()"}],["$","$L19",null,{"source":"$1a"}]]

---

## AtmStrategyClose()

> **URL**: https://developer.ninjatrader.com/docs/desktop/atmstrategyclose
> **Slug**: `atmstrategyclose`

## Definition

Cancels any working orders and closes any open position of a strategy using the default [ATM strategy close behavior](https://ninjatrader.com/support/helpGuides/nt8/NT%20HelpGuide%20English.html?closing_a_position_or_atm_stra.htm).

## Method Return Value

Returns true if the specified ATM strategy was found; otherwise false.

{% callout type="note" %}

A method return value of true in NO WAY indicates that the strategy in fact is closed. It indicates that the the specified ATM strategy was found and the internal close routine was triggered.
{% /callout %}

## Syntax

`AtmStrategyClose(string atmStrategyId)`

## Parameters

{% table %}

---

* atmStrategyId
* The unique identifier for the ATM strategy

---

{% /table %}

## Examples

```csharp
protected override void OnBarUpdate()
{
     // Check for valid condition and create an ATM Strategy
     if (GetAtmStrategyUnrealizedProfitLoss("idValue") > 500)
         AtmStrategyClose("idValue");
}
```

---

## AtmStrategyCancelEntryOrder()

> **URL**: https://developer.ninjatrader.com/docs/desktop/atmstrategycancelentryorder
> **Slug**: `atmstrategycancelentryorder`

## Definition

Cancels the specified entry order determined by the string "orderId" parameter.

{% callout type="note" %}

1. This method is intended ONLY for orders submitted as [Atm Entry Orders](atmstrategycreate) and assumes the [OrderState](getatmstrategyentryorderstatus) is NOT terminal (i.e., Cancelled, Filled, Rejected, Unknown).
2. If the specified order does not exist, the method returns false and an error is logged.
{% /callout %}

## Method Return Value

Returns true if the specified order was found; otherwise false.

## Syntax

`AtmStrategyCancelEntryOrder(string orderId)`

{% callout type="warning" %}

This method should ONLY be called once the strategy [State](state) has reached State.Realtime

{% /callout %}

## Parameters

{% table %}

* orderId

---

* The unique identifier for the entry order
{% /table %}

## Examples

```csharp
protected override void OnBarUpdate()
{
 Â  // ATM strategy methods only work during real-time
 Â  if (State != State.Realtime)
 Â  Â  return;
Â 
 Â  string[] entryOrder = GetAtmStrategyEntryOrderStatus("orderId");
Â 
 Â  // checks if the entry order exists
 Â  // and the order state is not already cancelled/filled/rejected
 Â  if (entryOrder.Length > 0 && entryOrder[2] == "Working")
 Â  {
 Â  Â  AtmStrategyCancelEntryOrder("orderId");
 Â  }
}
```b:[["$","h1",null,{"className":"docTitle","children":"AtmStrategyCancelEntryOrder()"}],["$","$L19",null,{"source":"$1a"}]]

---

## AtmStrategyChangeEntryOrder()

> **URL**: https://developer.ninjatrader.com/docs/desktop/atmstrategychangeentryorder
> **Slug**: `atmstrategychangeentryorder`

## Definition

Changes the price of the specified entry order.

## Method Return Value

Returns true if the specified order was found; otherwise false.

## Syntax

`AtmStrategyChangeEntryOrder(double limitPrice, double stopPrice, string orderId)`

## Parameters

{% table %}

---

* limitPrice
* Order limit price

---

* stopPrice
* Order stop price

---

* orderId
* The unique identifier for the entry order

---

{% /table %}

## Examples

```csharp
protected override void OnBarUpdate()
{
     AtmStrategyChangeEntryOrder(GetCurrentBid(), 0, "orderIdValue");
}
```

---

## AtmStrategyChangeStopTarget()

> **URL**: https://developer.ninjatrader.com/docs/desktop/atmstrategychangestoptarget
> **Slug**: `atmstrategychangestoptarget`

## Definition

Changes the price of the specified order of the specified ATM strategy.

## Method Return Value

Returns true if the specified order was found; otherwise false.

## Syntax

`AtmStrategyChangeStopTarget(double limitPrice, double stopPrice, string orderName, string atmStrategyId)`

## Parameters

{% table %}

---

* limitPrice
* Order limit price

---

* stopPrice
* Order stop price

---

* orderName
* The order name such as "Stop1" or "Target2"

---

* atmStrategyId
* The unique identifier for the ATM strategy

---

{% /table %}

## Examples

```csharp
protected override void OnBarUpdate()
{
     AtmStrategyChangeStopTarget(0, SMA(10)[0], "Stop1", "AtmIdValue");
}
```

---

## GetAtmStrategyEntryOrderStatus()

> **URL**: https://developer.ninjatrader.com/docs/desktop/getatmstrategyentryorderstatus
> **Slug**: `getatmstrategyentryorderstatus`

## Definition

Gets the current state of the specified entry order.

{% callout type="note" %}

If the method can't find the specified order, an empty array is returned.

{% /callout %}

## Method Return Value

A string[] array holding three elements that represent average fill price, filled amount and order state.

## Syntax

`GetAtmStrategyEntryOrderStatus(string orderId)`

## Parameters

{% table %}

---

* **orderId**
* The unique identifier for the entry order

---

{% /table %}

## Examples

```csharp
protected override void OnBarUpdate()
{
     string[] entryOrder = GetAtmStrategyEntryOrderStatus("orderId");

     // Check length to ensure that returned array holds order information
     if (entryOrder.Length > 0)
     {
         Print("Average fill price is " + entryOrder[0].ToString());
         Print("Filled amount is " + entryOrder[1].ToString());
         Print("Current state is " + entryOrder[2].ToString());
     }

```

---

## GetAtmStrategyMarketPosition()

> **URL**: https://developer.ninjatrader.com/docs/desktop/getatmstrategymarketposition
> **Slug**: `getatmstrategymarketposition`

## Definition

Gets the current market position of the specified ATM Strategy.

{% callout type="note" %}

Notes:

1. Changes to positions will not be reflected till at least the next **OnBarUpdate()** event after an order fill.
2. If the ATM Strategy does not exist then **MarketPosition.Flat** returns.
3. Please note this provides access to the current ATM strategy position, which should not be confused with the NinjaScript strategy position or account position. For more information please see the [Using ATM Strategies](using_atm_strategies) section.
{% /callout %}

## Method Return Value

* **MarketPosition.Flat**
* **MarketPosition.Long**
* **MarketPosition.Short**

## Syntax

`GetAtmStrategyMarketPosition(string atmStrategyId)`

## Parameters

{% table %}

---

* **atmStrategyId**
* The unique identifier for the ATM strategy
{% /table %}

## Examples

```csharp
protected override void OnBarUpdate()
{
    // Check if flat
    if (GetAtmStrategyMarketPosition("id") == MarketPosition.Flat)
        Print("ATM Strategy position is currently flat");
}
```b:[["$","h1",null,{"className":"docTitle","children":"GetAtmStrategyMarketPosition()"}],["$","$L19",null,{"source":"$1a"}]]

---

## AtmStrategySelector (UI Control)

> **URL**: https://developer.ninjatrader.com/docs/desktop/atmstrategyselector
> **Slug**: `atmstrategyselector`

## Definition

AtmStrategySelector is an UI element users can interact with for selecting ATM Strategies.

## Events and Properties

{% table %}

---

* Cleanup()
* Disposes of the AtmStrategySelector  (Note: calling the **NTTabPage base.Cleanup()** is sufficient to clean up this control)

---

* CustomPropertiesChanged
* Event handler for when properties have changed on the ATM strategy

---

* Id
* A string identifying the ATM Strategy selector

---

* SelectedAtmStrategy
* Returns an AtmStrategy representing the selected ATM strategy

---

* SelectionChanged
* Event handler for when the selected ATM strategy has changed
{% /table %}

## Examples

This example demonstrates how to use the ATM strategy selector and properly link its behavior with the quantity up/down and TIF selectors.

## Examples

```csharp
private QuantityUpDown qudSelector;
private TifSelector tifSelector;
private AtmStrategy.AtmStrategySelector atmStrategySelector;

private DependencyObject LoadXAML()
{
    qudSelector = LogicalTreeHelper.FindLogicalNode(pageContent, "qudSelector") as QuantityUpDown;
    tifSelector = LogicalTreeHelper.FindLogicalNode(pageContent, "tifSelector") as TifSelector;
    tifSelector.SetBinding(TifSelector.AccountProperty, new Binding { Source = accountSelector, Path = new PropertyPath("SelectedAccount") });
    tifSelector.SelectionChanged += (o, args) =>
    {
        if (atmStrategySelector.SelectedAtmStrategy != null)
            atmStrategySelector.SelectedAtmStrategy.TimeInForce = tifSelector.SelectedTif;
    };
    atmStrategySelector = LogicalTreeHelper.FindLogicalNode(pageContent, "atmStrategySelector") as AtmStrategy.AtmStrategySelector;
    atmStrategySelector.Id = Guid.NewGuid().ToString("N");
    if (atmStrategySelector != null)
        atmStrategySelector.CustomPropertiesChanged += OnAtmCustomPropertiesChanged;
    atmStrategySelector.SetBinding(AtmStrategy.AtmStrategySelector.AccountProperty, new Binding { Source = accountSelector, Path = new PropertyPath("SelectedAccount") });
    atmStrategySelector.SelectionChanged += (o, args) =>
    {
        if (atmStrategySelector.SelectedItem == null)
            return;
        if (args.AddedItems.Count > 0)
        {
            AtmStrategy selectedAtmStrategy = args.AddedItems[0] as AtmStrategy;
            if (selectedAtmStrategy != null)
                tifSelector.SelectedTif = selectedAtmStrategy.TimeInForce;
        }
    };
}

private void OnAtmCustomPropertiesChanged(object sender, NinjaScript.AtmStrategy.CustomPropertiesChangedEventArgs args)
{
    tifSelector.SelectedTif = args.NewTif;
    qudSelector.Value = args.NewQuantity;
}

public override void Cleanup()
{
    base.Cleanup();
}
```

```xml
<atmstrategy:atmstrategyselector grid.column="2" grid.row="12" linkedquantity="{Binding ElementName=qudSelector, Path=Value, Mode=OneWay}" x:name="atmStrategySelector">
    <atmstrategy:atmstrategyselector.margin>
        <thickness bottom="0" left="{StaticResource MarginButtonLeft}" right="{StaticResource MarginBase}" top="{StaticResource MarginControl}"></thickness>
    </atmstrategy:atmstrategyselector.margin>
</atmstrategy:atmstrategyselector>
```b:[["$","h1",null,{"className":"docTitle","children":"AtmStrategySelector"}],["$","$L19",null,{"source":"$1a"}]]

# 5. Strategy Overview (NinjaScript)

---

## Strategy Overview

> **URL**: https://developer.ninjatrader.com/docs/desktop/strategy_overview
> **Slug**: `strategy_overview`

## Strategy Overview

* [Backtesting NinjaScript Strategies with an intrabar granularity](backtesting_ninjascript_strategies_with_an_intrabar_granularity)
* [Entering on one time frame and exiting on another](entering_on_one_time_frame_and_exiting_on_another)
* [Getting PnL from an ATM strategy](getting-pnl-from-an-atm-strategy)
* [Halting a Strategy Once User Defined Conditions Are Met](halting_a_strategy_once_user_defined_conditions_are_met)
* [Keeping orders alive](keeping_orders_alive)
* [Modifying the price of stop loss and profit target orders](modifying_the_price_of_stop_loss_and_profit_target_orders)
* [Monitoring for and trading a breakout](monitoring_for_and_trading_a_breakout)
* [Monitoring Stop-Loss and Profit Target Orders](monitoring_stop_loss_and_profit_target_orders)
* [Plotting from within a NinjaScript Strategy](plotting_from_within_a_ninjascript_strategy)
* [Removing draw objects from the chart](removing_draw_objects_from_the_chart)
* [Resetting values at the beginning of new trading sessions](resetting_values_at_the_beginning_of_new_trading_sessions)
* [Rounding values to the nearest tick size](rounding_values_to_the_nearest_tick_size)
* [Scaling out of a position](scaling_out_of_a_position)
* [Separating logic to either calculate once on bar close or on every tick](separating_logic_to_either_calculate_once_on_bar_close_or_on_every_tick)
* [Stopping a strategy after consecutive losers](stopping_a_strategy_after_consecutive_losers)
* [Trading crossovers](trading_crossovers)
* [Using a time filter to limit trading hours](using_a_time_filter_to_limit_trading_hours)
* [Using CancelOrder() method to cancel orders](using_cancelorder_method_to_ca)
* [Using multiple entry/exit signals simultaneously](using_multiple_entry_exit_signals_simultaneously)
* [Using OnOrderUpdate() and OnExecution() methods to submit protective orders](using_onorderupdate_and_onexec)
* [Using IsRising and IsFalling conditions in the Strategy Builder](using_isrising_and_isfalling_conditions_in_the_strategy_builder)
* [Using trade performance statistics for money management](using_trade_performance_statistics_for_money_management)

---

## StrategyBase Class

> **URL**: https://developer.ninjatrader.com/docs/desktop/strategybase
> **Slug**: `strategybase`

StrategyBase contains properties and methods for managing a **Strategy** object, and is the base class from which **AtmStrategy** derives.

{% callout type="note" %}

Note: For a complete, working example of this class in use, download framework example located on our [Developing AddOns Overview](developing_add_ons).

{% /callout %}

## Examples

```csharp
// A button called acctStratButton in an NTTabPage displays all ATM and NinjaScript strategies configured on a selected Account when clicked
private void OnButtonClick(object sender, RoutedEventArgs e)
{
 Â  Button button = sender as Button;
 Â  Â 
 Â  if (button != null && ReferenceEquals(button, acctStratButton))
 Â  {
 Â  Â  Â  // When the button is pressed, iterate through all ATM and NinjaScript strategies
 Â  Â  Â  // This comprises all which are active, recovered upon last connect, or deactivated since last connect
 Â  Â  Â  // First, lock the Strategies collection to avoid in-flight changes to the collection affecting our output
 Â  Â  Â  lock (accountSelector.SelectedAccount.Strategies)
 Â  Â  Â  Â  Â  // Iterate through the Strategies collection in the selected Account
 Â  Â  Â  Â  Â  foreach (StrategyBase strategy in accountSelector.SelectedAccount.Strategies)
 Â  Â  Â  Â  Â  Â  Â  outputBox.AppendText(string.Format("{0}Name: {1}{0}ATM Template Name: {2}{0}Instrument: {3}{0}State: {4}{0}Category: {5}{0}",
 Â  Â  Â  Â  Â  Â  Â  Â  Â  Environment.NewLine,
 Â  Â  Â  Â  Â  Â  Â  Â  Â  strategy.Name,
 Â  Â  Â  Â  Â  Â  Â  Â  Â  strategy.Template,
 Â  Â  Â  Â  Â  Â  Â  Â  Â  strategy.Instruments[0].FullName, Â  Â  Â 
 Â  Â  Â  Â  Â  Â  Â  Â  Â  strategy.State,
 Â  Â  Â  Â  Â  Â  Â  Â  Â  strategy.Category));
 Â  }
}
```b:[["$","h1",null,{"className":"docTitle","children":"StrategyBase"}],["$","$L19",null,{"source":"$1a"}]]

# 6. AddOn Controls

---

## AccountSelector Control

> **URL**: https://developer.ninjatrader.com/docs/desktop/accountselector
> **Slug**: `accountselector`

## Definition

AccountSelector can be used as an UI element users can interact with for selecting accounts.

## Events and Properties

{% table %}

* Method/Property
* Description

---

* Cleanup()
* Disposes of the AccountSelector (Note: calling the **NTTabPage base.Cleanup()** is sufficient to clean up this control)

---

* SelectedAccount
* Returns an **Account** representing the selected account

---

* SelectionChanged
* Event handler for when the selected account has changed
{% /table %}

## Examples

```csharp
/* Example of subscribing/unsubscribing to market data from an Add On. The concept can be carried over
to any NinjaScript object you may be working on. */
public class MyAddOnTab : NTTabPage
{
     private AccountSelector accountSelector

     public MyAddOnTab()
     {
         // Note: pageContent (not demonstrated in this example) is the page content of the XAML
         // Find account selector
         accountSelector = LogicalTreeHelper.FindLogicalNode(pageContent, "accountSelector") as AccountSelector;

         // When the account selector's selection changes, unsubscribe and resubscribe
         accountSelector.SelectionChanged += (o, args) =>
         {
             if (accountSelector.SelectedAccount != null)
             {
                 // Unsubscribe to any prior account subscriptions
                 accountSelector.SelectedAccount.AccountItemUpdate -= OnAccountItemUpdate;
                 accountSelector.SelectedAccount.ExecutionUpdate -= OnExecutionUpdate;
                 accountSelector.SelectedAccount.OrderUpdate -= OnOrderUpdate;
                 accountSelector.SelectedAccount.PositionUpdate -= OnPositionUpdate;

                 // Subscribe to new account subscriptions
                 accountSelector.SelectedAccount.AccountItemUpdate   += OnAccountItemUpdate;
                 accountSelector.SelectedAccount.ExecutionUpdate     += OnExecutionUpdate;
                 accountSelector.SelectedAccount.OrderUpdate         += OnOrderUpdate;
                 accountSelector.SelectedAccount.PositionUpdate      += OnPositionUpdate;
             }
         };
     }

     // Called by TabControl when tab is being removed or window is closed
     public override void Cleanup()
     {
         // Clean up our resources
         base.Cleanup();
     }

     // Other required NTTabPage members left out for demonstration purposes. Be sure to add them in your own code.
}
```

```xaml
<page
	xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
	xmlns:accountdata="clr-namespace:NinjaTrader.Gui.AccountData;assembly=NinjaTrader.Gui"
	xmlns:accountperformance="clr-namespace:NinjaTrader.Gui.AccountPerformance;assembly=NinjaTrader.Gui"
	xmlns:atmstrategy="clr-namespace:NinjaTrader.Gui.NinjaScript.AtmStrategy;assembly=NinjaTrader.Gui"
	xmlns:tools="clr-namespace:NinjaTrader.Gui.Tools;assembly=NinjaTrader.Gui"
	xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
	<grid>
		<tools:accountselector horizontalalignment="Left" verticalalignment="Top" x:name="accountSelector"></tools:accountselector>
	</grid>
</page>
```b:[["$","h1",null,{"className":"docTitle","children":"AccountSelector"}],["$","$L19",null,{"source":"$1a"}]]

---

## Scrape Statistics

- Pages scraped: 26
- Pages failed: 0
- Total pages attempted: 26
- Scraped: 2026-08-10 12:12 UTC
