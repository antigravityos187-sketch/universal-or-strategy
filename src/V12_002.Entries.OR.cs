// V12.Phase7 MODULAR: OR Entry Node (Split from Entries.cs -- Phase 7 Partition)
// Contains: ExecuteLong, ExecuteShort, EnterORPosition, CalculateORStopDistance
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class V12_002 : Strategy
    {
        #region OR Entry Logic

        private void ExecuteLong(int contracts)
        {
            // V12.Phase7 [C-09]: Compliance enforcement gate -- abort if drawdown or daily cap breached.
            if (!IsOrderAllowed())
                return;
            if (contracts <= 0)
            {
                Print(string.Format("[OR] ExecuteLong received invalid contracts={0}. Aborting entry.", contracts));
                return;
            }

            // V12.2 Hybrid Sync: Manual Interception
            if (isTosSyncMode)
            {
                if (isLongArmed)
                {
                    // DOUBLE-CLICK BYPASS: If already armed, fire immediately
                    Print("[SYNC] Double-Click Bypass Triggered -> Executing LONG immediately (No ToS Handshake)");
                    isLongArmed = false;
                    // Proceed to entry logic below
                }
                else
                {
                    isLongArmed = true;
                    isShortArmed = false; // Mutually exclusive for simplicity
                    lastArmedTime = DateTime.Now;
                    Print("[SYNC] LONG ENTRY ARMED. Waiting for ToS handshake signal...");
                    return;
                }
            }

            if (!orComplete || sessionRange == 0)
            {
                Print("Cannot enter Long - OR not ready");
                return;
            }

            double entryPrice = Instrument.MasterInstrument.RoundToTickSize(sessionHigh + (3 * tickSize));
            double stopDistance = CalculateORStopDistance();
            double stopPrice = Instrument.MasterInstrument.RoundToTickSize(entryPrice - stopDistance);

            EnterORPosition(MarketPosition.Long, entryPrice, stopPrice, contracts);
        }

        private void ExecuteShort(int contracts)
        {
            // V12.Phase7 [C-09]: Compliance enforcement gate -- abort if drawdown or daily cap breached.
            if (!IsOrderAllowed())
                return;
            if (contracts <= 0)
            {
                Print(string.Format("[OR] ExecuteShort received invalid contracts={0}. Aborting entry.", contracts));
                return;
            }

            // V12.2 Hybrid Sync: Manual Interception
            if (isTosSyncMode)
            {
                if (isShortArmed)
                {
                    // DOUBLE-CLICK BYPASS: If already armed, fire immediately
                    Print("[SYNC] Double-Click Bypass Triggered -> Executing SHORT immediately (No ToS Handshake)");
                    isShortArmed = false;
                    // Proceed to entry logic below
                }
                else
                {
                    isShortArmed = true;
                    isLongArmed = false; // Mutually exclusive
                    lastArmedTime = DateTime.Now;
                    Print("[SYNC] SHORT ENTRY ARMED. Waiting for ToS handshake signal...");
                    return;
                }
            }

            if (!orComplete || sessionRange == 0)
            {
                Print("Cannot enter Short - OR not ready");
                return;
            }

            double entryPrice = Instrument.MasterInstrument.RoundToTickSize(sessionLow - (3 * tickSize));
            double stopDistance = CalculateORStopDistance();
            double stopPrice = Instrument.MasterInstrument.RoundToTickSize(entryPrice + stopDistance);

            EnterORPosition(MarketPosition.Short, entryPrice, stopPrice, contracts);
        }

        private bool IsOREntryAllowed(int contracts)
        {
            if (!IsOrderAllowed())
                return false;
            if (isFlattenRunning)
                return false;
            if (contracts <= 0)
            {
                Print(string.Format("[OR] EnterORPosition received invalid contracts={0}. Aborting entry.", contracts));
                return false;
            }
            return true;
        }

        private bool IsORBreakoutPriceValid(MarketPosition direction, double entryPrice)
        {
            double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
            if (direction == MarketPosition.Long && entryPrice <= currentPrice)
            {
                Print(
                    string.Format(
                        "OR ENTRY BLOCKED: Long entry {0:F2} already below market {1:F2} - too late for breakout",
                        entryPrice,
                        currentPrice
                    )
                );
                return false;
            }
            if (direction == MarketPosition.Short && entryPrice >= currentPrice)
            {
                Print(
                    string.Format(
                        "OR ENTRY BLOCKED: Short entry {0:F2} already above market {1:F2} - too late for breakout",
                        entryPrice,
                        currentPrice
                    )
                );
                return false;
            }
            return true;
        }

        private Order SubmitORStopMarketOrder(
            MarketPosition direction,
            int contracts,
            double entryPrice,
            string entryName
        )
        {
            if (direction == MarketPosition.Long)
                return SubmitOrderUnmanaged(
                    0,
                    OrderAction.Buy,
                    OrderType.StopMarket,
                    contracts,
                    0,
                    entryPrice,
                    "",
                    entryName
                );
            return SubmitOrderUnmanaged(
                0,
                OrderAction.SellShort,
                OrderType.StopMarket,
                contracts,
                0,
                entryPrice,
                "",
                entryName
            );
        }

        private void EnterORPosition(MarketPosition direction, double entryPrice, double stopPrice, int contracts)
        {
            if (!IsOREntryAllowed(contracts))
                return;

            try
            {
                // v5.13 FIX: Validate entry price before submitting StopMarket order
                // For LONG: entry must be ABOVE current price (breakout up)
                // For SHORT: entry must be BELOW current price (breakout down)
                // Use lastKnownPrice for real-time accuracy (Close[0] can be stale)
                if (!IsORBreakoutPriceValid(direction, entryPrice))
                    return;

                // V12.1101E: 5-target system with priority fill distribution
                int t1Qty,
                    t2Qty,
                    t3Qty,
                    t4Qty,
                    t5Qty;
                GetTargetDistribution(contracts, out t1Qty, out t2Qty, out t3Qty, out t4Qty, out t5Qty);

                Print(
                    string.Format(
                        "POSITION SIZE: {0} contracts -> T1:{1} T2:{2} T3:{3} T4:{4} T5:{5}",
                        contracts,
                        t1Qty,
                        t2Qty,
                        t3Qty,
                        t4Qty,
                        t5Qty
                    )
                );

                string entryName = BuildOREntryName(direction);

                // Universal Ladder: T(n)Type dropdown drives all target pricing.
                double target1Price = CalculateTargetPrice(direction, entryPrice, 1);
                double target2Price = CalculateTargetPrice(direction, entryPrice, 2);
                double target3Price = CalculateTargetPrice(direction, entryPrice, 3);
                double target4Price = CalculateTargetPrice(direction, entryPrice, 4);
                double target5Price = CalculateTargetPrice(direction, entryPrice, 5);

                PositionInfo pos = new PositionInfo
                {
                    SignalName = entryName,
                    Direction = direction,
                    TotalContracts = contracts,
                    T1Contracts = t1Qty,
                    T2Contracts = t2Qty,
                    T3Contracts = t3Qty,
                    T4Contracts = t4Qty,
                    T5Contracts = t5Qty,
                    RemainingContracts = contracts,
                    EntryPrice = entryPrice,
                    InitialStopPrice = stopPrice,
                    CurrentStopPrice = stopPrice,
                    Target1Price = target1Price,
                    Target2Price = target2Price,
                    Target3Price = target3Price,
                    Target4Price = target4Price,
                    Target5Price = target5Price,
                    EntryFilled = false,
                    T1Filled = false,
                    T2Filled = false,
                    T3Filled = false,
                    BracketSubmitted = false,
                    ExtremePriceSinceEntry = entryPrice,
                    CurrentTrailLevel = 0,
                    EntryOrderType = OrderType.StopMarket,
                    IsRMATrade = false,
                    OcoGroupId = "V12_" + GetStableHash(entryName),
                };
                ApplyTargetLadderGuard(pos);

                // V12.13-D: Notify connected panel clients of position entry
                string syncMsg = string.Format("POSITION_ENTERED|OR|{0}", contracts);
                SendResponseToRemote(syncMsg);

                // Build 1102Y-V3 [MS-03]: Register Master's expected position BEFORE StopMarket entry.
                int masterDeltaOR = ComputeORMasterDelta(direction, contracts);
                EnqueueORExpectedDelta(masterDeltaOR);

                // Submit entry order as stop market (breakout entry)
                Order entryOrder = SubmitORStopMarketOrder(direction, contracts, entryPrice, entryName);

                // A1-1/A2-1: Null-abort rollback (Build 960 audit fix)
                if (entryOrder == null)
                {
                    HandleNullOREntry(entryName, masterDeltaOR);
                    return;
                }
                {
                    var _en966ap = entryName;
                    var _p966ap = pos;
                    Enqueue(ctx =>
                    {
                        ctx.activePositions[_en966ap] = _p966ap;
                    });
                }
                {
                    var _en966 = entryName;
                    var _eo966 = entryOrder;
                    Enqueue(ctx =>
                    {
                        ctx.entryOrders[_en966] = _eo966;
                    });
                }

                string signalName = GetORSignalName(direction);
                Print(
                    string.Format(
                        "OR ENTRY ORDER: {0} {1}@{2:F2} | Stop: {3:F2} | OR Range: {4:F2}",
                        signalName,
                        contracts,
                        entryPrice,
                        stopPrice,
                        sessionRange
                    )
                );
                Print(
                    string.Format(
                        "TARGETS: T1:{0}@{1:F2} | T2:{2}@{3:F2} | T3:{4}@{5:F2} | T4:{6}@{7:F2} | T5:{8}@{9:F2} (Runner targets trail-only)",
                        t1Qty,
                        target1Price,
                        t2Qty,
                        target2Price,
                        t3Qty,
                        target3Price,
                        t4Qty,
                        target4Price,
                        t5Qty,
                        target5Price
                    )
                );

                // V12 SIMA: Dispatch to fleet (replaces legacy slave broadcast)
                DispatchSIMAEntry(direction, contracts, entryPrice);
            }
            catch (Exception ex)
            {
                Print("ERROR EnterORPosition: " + ex.Message);
            }
        }

        // Extracted helper: returns "ORLong_<timestamp>" or "ORShort_<timestamp>".
        // Removes a ternary branch from EnterORPosition to reduce CYC.
        private string BuildOREntryName(MarketPosition direction)
        {
            string signalName = direction == MarketPosition.Long ? "ORLong" : "ORShort";
            string timestamp = DateTime.Now.ToString("HHmmssffff");
            return signalName + "_" + timestamp;
        }

        // Extracted helper: returns "ORLong" or "ORShort" label used in log output.
        // Keeps CYC-contributing ternary out of EnterORPosition.
        private string GetORSignalName(MarketPosition direction)
        {
            return direction == MarketPosition.Long ? "ORLong" : "ORShort";
        }

        // Extracted helper: computes signed master delta for the Order Ledger.
        // Long entries add positive delta; short entries subtract.
        // Removes a ternary branch from EnterORPosition to reduce CYC.
        private int ComputeORMasterDelta(MarketPosition direction, int contracts)
        {
            return direction == MarketPosition.Long ? contracts : -contracts;
        }

        // Extracted helper: enqueues an expected position delta on the Actor.
        // Encapsulates the Enqueue/ExpKey boilerplate for Order Ledger updates.
        private void EnqueueORExpectedDelta(int delta)
        {
            var _aek966 = ExpKey(Account.Name);
            var _aed966 = delta;
            Enqueue(ctx => ctx.AddExpectedPositionDeltaLocked(_aek966, _aed966));
        }

        // Extracted helper: handles null-order rollback path (Build 960 / MS-03).
        // Rolls back the Order Ledger reservation and logs the abort reason.
        // Removes the null-check body from EnterORPosition to reduce CYC.
        private void HandleNullOREntry(string entryName, int masterDeltaOR)
        {
            // Build 1102Y-V3 [MS-03 ROLLBACK]: Submit failed -- undo Order Ledger reservation.
            EnqueueORExpectedDelta(-masterDeltaOR);
            Print(
                "[ENTRY_ABORT] OR SubmitOrderUnmanaged returned NULL for "
                    + entryName
                    + " -- Master expected rolled back. Fleet dispatch aborted."
            );
        }

        // Extracted helper: dispatches SIMA fleet order when SIMA is enabled.
        // [923A-P0-OR]: StopMarket prevents immediate "marketable limit" fill.
        // OR Long entry price is ABOVE current market; a Limit order there is immediately
        // marketable on Apex/Tradovate (fills at current ask). StopMarket activates only
        // when price actually reaches/breaks the OR High/Low -- matching master behavior.
        // Removes the if(EnableSIMA) branch and its internal ternary from EnterORPosition.
        private void DispatchSIMAEntry(MarketPosition direction, int contracts, double entryPrice)
        {
            if (!EnableSIMA)
                return;
            OrderAction action = direction == MarketPosition.Long ? OrderAction.Buy : OrderAction.SellShort;
            ExecuteSmartDispatchEntry("OR", action, contracts, entryPrice, OrderType.StopMarket);
        }

        private double CalculateORStopDistance()
        {
            // v5.13: Use ATR for OR stop (same as RMA) instead of OR range
            if (currentATR <= 0)
                return MinimumStop;

            double calculatedStop = CalculateATRStopDistance(StopMultiplier); // V12.30: Ceiling-rounded
            return calculatedStop;
        }

        #endregion
    }
}
