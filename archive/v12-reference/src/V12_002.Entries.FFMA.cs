// V12.Phase7 MODULAR: FFMA Entry Node (Split from Entries.cs -- Phase 7 Partition)
// Contains: CheckFFMAConditions, ExecuteFFMAEntry, DeactivateFFMAMode,
//           ExecuteFFMALimitEntry, ExecuteFFMAManualMarketEntry
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
        #region FFMA Entry Logic (V8.7)

        /// <summary>
        /// V8.7: Check FFMA conditions and execute on reversal candle
        /// SHORT: RSI > 80 + price 10+ pts above 9 EMA + RED candle
        /// LONG: RSI < 20 + price 10+ pts below 9 EMA + GREEN candle
        /// </summary>
        private void CheckFFMAConditions()
        {
            if (!CheckFFMAGuards())
                return;
            try
            {
                double ema9Value = ema9[0];
                double rsiValue = rsiIndicator[0];
                double currentPrice = Close[0];
                double distanceFromEMA = currentPrice - ema9Value;
                if (TryExecuteFFMAShort(rsiValue, distanceFromEMA, currentPrice))
                    return;
                TryExecuteFFMALong(rsiValue, distanceFromEMA, currentPrice);
            }
            catch (Exception ex)
            {
                Print("ERROR CheckFFMAConditions: " + ex.Message);
            }
        }

        // T1: Guard checks extracted from CheckFFMAConditions
        private bool CheckFFMAGuards()
        {
            if (!isFFMAModeArmed || !FFMAEnabled)
                return false;
            if (ema9 == null || rsiIndicator == null || currentATR <= 0)
                return false;
            if (CurrentBar < 20)
                return false;
            return true;
        }

        // T2: Shared stop distance calculation for SHORT and LONG setups
        private double ComputeFFMAStopDistance(double currentPrice, double candleExtreme)
        {
            double stopDistance = Math.Min(Math.Abs(currentPrice - candleExtreme), MaximumStop);
            if (stopDistance < tickSize * 2)
                stopDistance = tickSize * 2;
            return stopDistance;
        }

        // T3: SHORT setup execution -- RSI > overbought + price far above EMA + RED candle
        private bool TryExecuteFFMAShort(double rsiValue, double distanceFromEMA, double currentPrice)
        {
            if (!(rsiValue > FFMARSIOverbought && distanceFromEMA >= FFMAEMADistance && Close[0] < Open[0]))
                return false;
            Print(
                string.Format(
                    "FFMA SHORT TRIGGERED: RSI={0:F1} > {1} | Distance={2:F2}pts > {3}pts | RED candle",
                    rsiValue,
                    FFMARSIOverbought,
                    distanceFromEMA,
                    FFMAEMADistance
                )
            );
            double stopDistance = ComputeFFMAStopDistance(currentPrice, High[0]);
            int contracts = CalculatePositionSize(stopDistance);
            ExecuteFFMAEntry(MarketPosition.Short, contracts);
            return true;
        }

        // T4: LONG setup execution -- RSI < oversold + price far below EMA + GREEN candle
        private bool TryExecuteFFMALong(double rsiValue, double distanceFromEMA, double currentPrice)
        {
            if (!(rsiValue < FFMARSIOversold && distanceFromEMA <= -FFMAEMADistance && Close[0] > Open[0]))
                return false;
            Print(
                string.Format(
                    "FFMA LONG TRIGGERED: RSI={0:F1} < {1} | Distance={2:F2}pts (below by {3}pts) | GREEN candle",
                    rsiValue,
                    FFMARSIOversold,
                    distanceFromEMA,
                    FFMAEMADistance
                )
            );
            double stopDistance = ComputeFFMAStopDistance(currentPrice, Low[0]);
            int contracts = CalculatePositionSize(stopDistance);
            ExecuteFFMAEntry(MarketPosition.Long, contracts);
            return true;
        }

        /// <summary>
        /// V8.7: Execute FFMA market order with entry candle high/low as stop
        /// Uses same target system as RMA (T1-T5)
        /// </summary>
        private void ExecuteFFMAEntry(MarketPosition direction, int contracts)
        {
            // V12.Phase7 [C-09]: Compliance enforcement gate -- abort if drawdown or daily cap breached.
            if (!IsOrderAllowed())
                return;
            // V12.Phase6 [FLATTEN-GUARD]: Prevent order submission during active flatten
            if (isFlattenRunning)
                return;

            try
            {
                double entryPrice = Close[0]; // Market order at current price

                if (!TryComputeFFMAStop(direction, entryPrice, out double stopPrice))
                    return;

                ComputeFFMATargets(
                    direction,
                    entryPrice,
                    contracts,
                    out double target1Price,
                    out double target2Price,
                    out double target3Price,
                    out double target4Price,
                    out double target5Price,
                    out int t1Qty,
                    out int t2Qty,
                    out int t3Qty,
                    out int t4Qty,
                    out int t5Qty
                );

                PositionInfo pos = BuildFFMAPositionInfo(
                    direction,
                    contracts,
                    entryPrice,
                    stopPrice,
                    target1Price,
                    target2Price,
                    target3Price,
                    target4Price,
                    target5Price,
                    t1Qty,
                    t2Qty,
                    t3Qty,
                    t4Qty,
                    t5Qty,
                    out string entryName
                );

                if (!SubmitFFMAOrderAndRegister(direction, contracts, entryPrice, entryName, pos))
                    return;

                // Disarm FFMA after execution (one-shot)
                DeactivateFFMAMode();
            }
            catch (Exception ex)
            {
                Print("ERROR ExecuteFFMAEntry: " + ex.Message);
            }
        }

        // W9-L7-005 T1: Stop price computation + validation guards.
        // Returns false when stop is invalid (abort sentinel).
        private bool TryComputeFFMAStop(MarketPosition direction, double entryPrice, out double stopPrice)
        {
            stopPrice = direction == MarketPosition.Long ? Low[0] : High[0];
            double stopDistance = Math.Min(Math.Abs(entryPrice - stopPrice), MaximumStop); // V8.31: Use MaximumStop

            if (stopDistance < tickSize * 2)
            {
                Print(string.Format("FFMA: Stop too tight ({0:F2}pts) - using 2 tick minimum", stopDistance));
                stopPrice =
                    direction == MarketPosition.Long ? entryPrice - (tickSize * 2) : entryPrice + (tickSize * 2);
                stopDistance = tickSize * 2;
            }

            if (stopDistance <= 0)
            {
                Print("[FFMA REJECT] Stop distance is zero (doji candle or tickSize=0). Aborting entry.");
                return false;
            }

            // V12.Phase6 [TICK-01]: Round all prices to valid tick increments before order submission
            stopPrice = Instrument.MasterInstrument.RoundToTickSize(stopPrice);
            return true;
        }

        // W9-L7-005 T2: Universal Ladder target prices + 5-way quantity distribution.
        private void ComputeFFMATargets(
            MarketPosition direction,
            double entryPrice,
            int contracts,
            out double t1Price,
            out double t2Price,
            out double t3Price,
            out double t4Price,
            out double t5Price,
            out int t1Qty,
            out int t2Qty,
            out int t3Qty,
            out int t4Qty,
            out int t5Qty
        )
        {
            t1Price = CalculateTargetPrice(direction, entryPrice, 1);
            t2Price = CalculateTargetPrice(direction, entryPrice, 2);
            t3Price = CalculateTargetPrice(direction, entryPrice, 3);
            t4Price = CalculateTargetPrice(direction, entryPrice, 4);
            t5Price = CalculateTargetPrice(direction, entryPrice, 5);
            GetTargetDistribution(contracts, out t1Qty, out t2Qty, out t3Qty, out t4Qty, out t5Qty);
        }

        // W9-L7-005 T3: Signal naming + PositionInfo construction.
        private PositionInfo BuildFFMAPositionInfo(
            MarketPosition direction,
            int contracts,
            double entryPrice,
            double stopPrice,
            double t1Price,
            double t2Price,
            double t3Price,
            double t4Price,
            double t5Price,
            int t1Qty,
            int t2Qty,
            int t3Qty,
            int t4Qty,
            int t5Qty,
            out string entryName
        )
        {
            string signalName = direction == MarketPosition.Long ? "FFMALong" : "FFMAShort";
            entryName = signalName + "_" + DateTime.UtcNow.ToString("HHmmssffff");
            return new PositionInfo
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
                Target1Price = t1Price,
                Target2Price = t2Price,
                Target3Price = t3Price,
                Target4Price = t4Price,
                Target5Price = t5Price,
                EntryFilled = false,
                T1Filled = false,
                T2Filled = false,
                T3Filled = false,
                BracketSubmitted = false,
                ExtremePriceSinceEntry = entryPrice,
                CurrentTrailLevel = 0,
                EntryOrderType = OrderType.Market,
                IsRMATrade = false,
                IsFFMATrade = true,
                OcoGroupId = "V12_" + GetStableHash(entryName),
            };
        }

        // W9-L7-005 T4: Market order submission, null guard, state enqueue, panel notification, SIMA dispatch.
        // Returns false on null order (abort).
        private bool SubmitFFMAOrderAndRegister(
            MarketPosition direction,
            int contracts,
            double entryPrice,
            string entryName,
            PositionInfo pos
        )
        {
            string signalName = direction == MarketPosition.Long ? "FFMALong" : "FFMAShort";

            Order entryOrder =
                direction == MarketPosition.Long
                    ? SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Market, contracts, 0, 0, "", entryName)
                    : SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Market, contracts, 0, 0, "", entryName);

            if (entryOrder == null)
            {
                Print("[ENTRY_ABORT] FFMA SubmitOrderUnmanaged returned null for " + entryName + ". Rolling back.");
                return false;
            }

            var en966ap = entryName;
            var p966ap = pos;
            Enqueue(ctx =>
            {
                ctx.activePositions[en966ap] = p966ap;
            });

            var en966 = entryName;
            var eo966 = entryOrder;
            Enqueue(ctx =>
            {
                ctx.entryOrders[en966] = eo966;
            });

            // B957: Notify panel only after confirmed submit (not before). Prevents premature IPC notification.
            string syncMsg = string.Format("POSITION_ENTERED|FFMA|{0}", contracts);
            SendResponseToRemote(syncMsg);

            Print(
                string.Format(
                    "FFMA MARKET ORDER: {0} {1}@MARKET | Stop: {2:F2} (candle {3})",
                    signalName,
                    contracts,
                    pos.InitialStopPrice,
                    direction == MarketPosition.Long ? "low" : "high"
                )
            );
            Print(
                string.Format(
                    "FFMA TARGETS: T1:{0}@{1:F2} | T2:{2}@{3:F2} | T3:{4}@{5:F2} | T4:{6}@{7:F2} | T5:{8}@{9:F2} (Runner targets trail-only)",
                    pos.T1Contracts,
                    pos.Target1Price,
                    pos.T2Contracts,
                    pos.Target2Price,
                    pos.T3Contracts,
                    pos.Target3Price,
                    pos.T4Contracts,
                    pos.Target4Price,
                    pos.T5Contracts,
                    pos.Target5Price
                )
            );

            // V12 SIMA: Dispatch to fleet (replaces legacy slave broadcast)
            if (EnableSIMA)
            {
                ExecuteSmartDispatchEntry(
                    "FFMA",
                    direction == MarketPosition.Long ? OrderAction.Buy : OrderAction.SellShort,
                    contracts,
                    entryPrice,
                    OrderType.Market,
                    entryName
                );
            }

            return true;
        }

        private void DeactivateFFMAMode()
        {
            isFFMAModeArmed = false;
            // V12.24: Notify panel to reset FFMA Smart Toggle visual
            SendResponseToRemote("FFMA_DISARMED");
            Print("V12.24: FFMA disarmed -- sent FFMA_DISARMED to panel");
        }

        #endregion

        #region FFMA Manual Entry Methods (V12.27)

        /// <summary>
        /// V12.27: FFMA manual entry using Limit Order at user-specified price.
        /// Uses ATR-based stop (same as standard FFMA but with Limit instead of Market).
        /// </summary>
        // T5: Stop distance validation for FFMA_LIMIT -- clamps to 2-tick minimum, rejects zero.
        // Returns false when caller must abort; writes validated values back via ref params.
        private bool ValidateAndAdjustFFMALimitStop(
            MarketPosition direction,
            double entryPrice,
            ref double stopDistance,
            ref double stopPrice
        )
        {
            if (stopDistance < tickSize * 2)
            {
                Print(
                    string.Format("V12.27 FFMA_LIMIT: Stop too tight ({0:F2}pts) - using 2 tick minimum", stopDistance)
                );
                stopPrice = Instrument.MasterInstrument.RoundToTickSize(
                    direction == MarketPosition.Long ? entryPrice - (tickSize * 2) : entryPrice + (tickSize * 2)
                );
                stopDistance = tickSize * 2;
            }
            if (stopDistance <= 0)
            {
                Print("[FFMA_LIMIT REJECT] Stop distance is zero after ATR calc. Aborting entry.");
                return false;
            }
            return true;
        }

        private void ExecuteFFMALimitEntry(double manualPrice, MarketPosition direction, int contracts)
        {
            // V12.Phase7 [C-09]: Compliance enforcement gate.
            if (!IsOrderAllowed())
                return;
            // V12.Phase6 [FLATTEN-GUARD]: Prevent order submission during active flatten
            if (isFlattenRunning)
                return;
            if (currentATR <= 0)
            {
                Print("V12.27 FFMA_LIMIT: Ignored - ATR not available");
                return;
            }
            try
            {
                if (
                    !BuildFFMALimitPrices(
                        manualPrice,
                        direction,
                        out double entryPrice,
                        out double stopDistance,
                        out double stopPrice
                    )
                )
                    return;
                if (!ExecuteFFMALimitCoreAndDispatch(direction, contracts, entryPrice, stopDistance, stopPrice))
                    return;
                DeactivateFFMAMode();
            }
            catch (Exception ex)
            {
                Print("ERROR ExecuteFFMALimitEntry: " + ex.Message);
            }
        }

        private bool ExecuteFFMALimitCoreAndDispatch(
            MarketPosition direction,
            int contracts,
            double entryPrice,
            double stopDistance,
            double stopPrice
        )
        {
            BuildFFMALimitTargets(
                direction,
                entryPrice,
                contracts,
                out double t1Price,
                out double t2Price,
                out double t3Price,
                out double t4Price,
                out double t5Price,
                out int t1Qty,
                out int t2Qty,
                out int t3Qty,
                out int t4Qty,
                out int t5Qty
            );
            PositionInfo pos = BuildFFMALimitPositionInfo(
                direction,
                contracts,
                entryPrice,
                stopPrice,
                t1Price,
                t2Price,
                t3Price,
                t4Price,
                t5Price,
                t1Qty,
                t2Qty,
                t3Qty,
                t4Qty,
                t5Qty,
                out string entryName
            );
            if (
                !SubmitFFMALimitOrderAndEnqueue(
                    direction,
                    contracts,
                    entryPrice,
                    stopPrice,
                    entryName,
                    pos,
                    t1Price,
                    t2Price,
                    t3Price,
                    t4Price,
                    t5Price,
                    t1Qty,
                    t2Qty,
                    t3Qty,
                    t4Qty,
                    t5Qty
                )
            )
                return false;
            if (EnableSIMA)
                ExecuteSmartDispatchEntry(
                    "FFMA_MNL",
                    direction == MarketPosition.Long ? OrderAction.Buy : OrderAction.SellShort,
                    contracts,
                    entryPrice,
                    OrderType.Limit,
                    entryName
                );
            return true;
        }

        private bool BuildFFMALimitPrices(
            double manualPrice,
            MarketPosition direction,
            out double entryPrice,
            out double stopDistance,
            out double stopPrice
        )
        {
            entryPrice = Instrument.MasterInstrument.RoundToTickSize(manualPrice);
            // V12.27: ATR-based stop (mirrors standard FFMA but won't use candle high/low since manual)
            stopDistance = CalculateATRStopDistance(RMAStopATRMultiplier); // V12.30: Ceiling-rounded
            stopPrice = Instrument.MasterInstrument.RoundToTickSize(
                direction == MarketPosition.Long ? entryPrice - stopDistance : entryPrice + stopDistance
            );
            return ValidateAndAdjustFFMALimitStop(direction, entryPrice, ref stopDistance, ref stopPrice);
        }

        private void BuildFFMALimitTargets(
            MarketPosition direction,
            double entryPrice,
            int contracts,
            out double t1Price,
            out double t2Price,
            out double t3Price,
            out double t4Price,
            out double t5Price,
            out int t1Qty,
            out int t2Qty,
            out int t3Qty,
            out int t4Qty,
            out int t5Qty
        )
        {
            // Universal Ladder: T(n)Type dropdown drives all target pricing.
            t1Price = CalculateTargetPrice(direction, entryPrice, 1);
            t2Price = CalculateTargetPrice(direction, entryPrice, 2);
            t3Price = CalculateTargetPrice(direction, entryPrice, 3);
            t4Price = CalculateTargetPrice(direction, entryPrice, 4);
            t5Price = CalculateTargetPrice(direction, entryPrice, 5);
            // contracts input passed directly by UI/IPC (No-Blink compliance)
            GetTargetDistribution(contracts, out t1Qty, out t2Qty, out t3Qty, out t4Qty, out t5Qty);
        }

        private PositionInfo BuildFFMALimitPositionInfo(
            MarketPosition direction,
            int contracts,
            double entryPrice,
            double stopPrice,
            double t1Price,
            double t2Price,
            double t3Price,
            double t4Price,
            double t5Price,
            int t1Qty,
            int t2Qty,
            int t3Qty,
            int t4Qty,
            int t5Qty,
            out string entryName
        )
        {
            string signalName = direction == MarketPosition.Long ? "FFMAMnlLong" : "FFMAMnlShort";
            entryName = signalName + "_" + DateTime.UtcNow.ToString("HHmmssffff");
            return new PositionInfo
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
                Target1Price = t1Price,
                Target2Price = t2Price,
                Target3Price = t3Price,
                Target4Price = t4Price,
                Target5Price = t5Price,
                EntryFilled = false,
                T1Filled = false,
                T2Filled = false,
                T3Filled = false,
                BracketSubmitted = false,
                ExtremePriceSinceEntry = entryPrice,
                CurrentTrailLevel = 0,
                EntryOrderType = OrderType.Limit,
                IsRMATrade = false,
                IsFFMATrade = true,
                OcoGroupId = "V12_" + GetStableHash(entryName),
            };
        }

        private bool SubmitFFMALimitOrderAndEnqueue(
            MarketPosition direction,
            int contracts,
            double entryPrice,
            double stopPrice,
            string entryName,
            PositionInfo pos,
            double t1Price,
            double t2Price,
            double t3Price,
            double t4Price,
            double t5Price,
            int t1Qty,
            int t2Qty,
            int t3Qty,
            int t4Qty,
            int t5Qty
        )
        {
            // V12.27: Submit LIMIT order (not Market like standard FFMA)
            Order entryOrder =
                direction == MarketPosition.Long
                    ? SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Limit, contracts, entryPrice, 0, "", entryName)
                    : SubmitOrderUnmanaged(
                        0,
                        OrderAction.SellShort,
                        OrderType.Limit,
                        contracts,
                        entryPrice,
                        0,
                        "",
                        entryName
                    );

            // A1-1/A2-1: Null-abort rollback + stateLock wrap (Build 960 audit fix)
            if (entryOrder == null)
            {
                Print(
                    "[ENTRY_ABORT] FFMA_LIMIT SubmitOrderUnmanaged returned null for " + entryName + ". Rolling back."
                );
                return false;
            }

            {
                var en966ap = entryName;
                var p966ap = pos;
                Enqueue(ctx =>
                {
                    ctx.activePositions[en966ap] = p966ap;
                });
            }
            {
                var en966 = entryName;
                var eo966 = entryOrder;
                Enqueue(ctx =>
                {
                    ctx.entryOrders[en966] = eo966;
                });
            }

            Print(
                string.Format(
                    "V12.27 FFMA_LIMIT: {0} {1}@{2:F2} LIMIT | Stop: {3:F2} | ATR-based",
                    direction,
                    contracts,
                    entryPrice,
                    stopPrice
                )
            );
            Print(
                string.Format(
                    "V12.27 FFMA_LIMIT TARGETS: T1:{0}@{1:F2} | T2:{2}@{3:F2} | T3:{4}@{5:F2} | T4:{6}@{7:F2} | T5:{8}@{9:F2}",
                    t1Qty,
                    t1Price,
                    t2Qty,
                    t2Price,
                    t3Qty,
                    t3Price,
                    t4Qty,
                    t4Price,
                    t5Qty,
                    t5Price
                )
            );
            return true;
        }

        /// <summary>
        /// V12.27: FFMA Manual Market entry -- instant market order, direction toward 9 EMA.
        /// Stop at entry candle high/low (same as Auto FFMA).
        /// </summary>
        private bool ValidateFFMAManualMarketPreconditions()
        {
            // V12.Phase7 [C-09]: Compliance enforcement gate.
            if (!IsOrderAllowed())
                return false;
            // V12.Phase6 [FLATTEN-GUARD]: Prevent order submission during active flatten
            if (isFlattenRunning)
                return false;
            if (currentATR <= 0)
            {
                Print("V12.27 FFMA_MANUAL_MARKET: Ignored - ATR not available");
                return false;
            }
            if (ema9 == null)
            {
                Print("V12.27 FFMA_MANUAL_MARKET: Ignored - EMA9 not initialized");
                return false;
            }
            return true;
        }

        private MarketPosition DetermineFFMAManualMarketDirection(double currentPrice, double ema9Value)
        {
            // V12.27: Direction always toward 9 EMA
            // Price below EMA9 = LONG (price moving up toward EMA)
            // Price above EMA9 = SHORT (price moving down toward EMA)
            if (currentPrice < ema9Value)
            {
                Print(
                    string.Format(
                        "V12.27 FFMA_MANUAL_MARKET: Price below EMA9 ({0:F2} < {1:F2}) = LONG toward EMA",
                        currentPrice,
                        ema9Value
                    )
                );
                return MarketPosition.Long;
            }
            Print(
                string.Format(
                    "V12.27 FFMA_MANUAL_MARKET: Price above EMA9 ({0:F2} > {1:F2}) = SHORT toward EMA",
                    currentPrice,
                    ema9Value
                )
            );
            return MarketPosition.Short;
        }

        private void ExecuteFFMAManualMarketEntry(int contracts)
        {
            if (!ValidateFFMAManualMarketPreconditions())
                return;

            try
            {
                double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
                double ema9Value = ema9[0];
                MarketPosition direction = DetermineFFMAManualMarketDirection(currentPrice, ema9Value);
                double entryPrice = currentPrice;

                double stopPrice = CalcFFMAManualStopPrice(direction, entryPrice);
                if (double.IsNaN(stopPrice))
                    return;

                CalcFFMAManualTargetPrices(
                    direction,
                    entryPrice,
                    contracts,
                    out double target1Price,
                    out double target2Price,
                    out double target3Price,
                    out double target4Price,
                    out double target5Price,
                    out int t1Qty,
                    out int t2Qty,
                    out int t3Qty,
                    out int t4Qty,
                    out int t5Qty
                );

                string signalName = direction == MarketPosition.Long ? "FFMAMnlMktLong" : "FFMAMnlMktShort";
                string entryName = signalName + "_" + DateTime.UtcNow.ToString("HHmmssffff");

                PositionInfo pos = BuildFFMAManualPositionInfo(
                    entryName,
                    direction,
                    contracts,
                    entryPrice,
                    stopPrice,
                    target1Price,
                    target2Price,
                    target3Price,
                    target4Price,
                    target5Price,
                    t1Qty,
                    t2Qty,
                    t3Qty,
                    t4Qty,
                    t5Qty
                );

                if (!SubmitFFMAManualMarketOrder(direction, contracts, entryName, pos))
                    return;

                LogFFMAManualMarketEntry(pos, ema9Value);

                if (EnableSIMA)
                {
                    ExecuteSmartDispatchEntry(
                        "FFMA_MNL_MKT",
                        direction == MarketPosition.Long ? OrderAction.Buy : OrderAction.SellShort,
                        contracts,
                        entryPrice,
                        OrderType.Market,
                        entryName
                    );
                }

                DeactivateFFMAMode();
            }
            catch (Exception ex)
            {
                Print("ERROR ExecuteFFMAManualMarketEntry: " + ex.Message);
            }
        }

        // -- Block-A: stop price calculation + minimum-tick and zero-distance guards
        private double CalcFFMAManualStopPrice(MarketPosition direction, double entryPrice)
        {
            double stopPrice = Instrument.MasterInstrument.RoundToTickSize(
                direction == MarketPosition.Long ? Low[0] : High[0]
            );
            double stopDistance = Math.Min(Math.Abs(entryPrice - stopPrice), MaximumStop);

            if (stopDistance < tickSize * 2)
            {
                Print(
                    string.Format(
                        "V12.27 FFMA_MANUAL_MARKET: Stop too tight ({0:F2}pts) - using 2 tick minimum",
                        stopDistance
                    )
                );
                stopPrice = Instrument.MasterInstrument.RoundToTickSize(
                    direction == MarketPosition.Long ? entryPrice - (tickSize * 2) : entryPrice + (tickSize * 2)
                );
                stopDistance = tickSize * 2;
            }

            // V12.44: Final stop-distance guard -- prevent CalculatePositionSize(0) -> div-by-zero
            if (stopDistance <= 0)
            {
                Print("[FFMA_MANUAL_MARKET REJECT] Stop distance is zero (doji candle?). Aborting entry.");
                return double.NaN;
            }

            return stopPrice;
        }

        // -- Block-B: five-level target price ladder + quantity distribution
        private void CalcFFMAManualTargetPrices(
            MarketPosition direction,
            double entryPrice,
            int contracts,
            out double t1Price,
            out double t2Price,
            out double t3Price,
            out double t4Price,
            out double t5Price,
            out int t1Qty,
            out int t2Qty,
            out int t3Qty,
            out int t4Qty,
            out int t5Qty
        )
        {
            t1Price = CalculateTargetPrice(direction, entryPrice, 1);
            t2Price = CalculateTargetPrice(direction, entryPrice, 2);
            t3Price = CalculateTargetPrice(direction, entryPrice, 3);
            t4Price = CalculateTargetPrice(direction, entryPrice, 4);
            t5Price = CalculateTargetPrice(direction, entryPrice, 5);
            GetTargetDistribution(contracts, out t1Qty, out t2Qty, out t3Qty, out t4Qty, out t5Qty);
        }

        // -- Block-C: PositionInfo 30-field factory -- pure construction, zero decisions
        private PositionInfo BuildFFMAManualPositionInfo(
            string entryName,
            MarketPosition direction,
            int contracts,
            double entryPrice,
            double stopPrice,
            double target1Price,
            double target2Price,
            double target3Price,
            double target4Price,
            double target5Price,
            int t1Qty,
            int t2Qty,
            int t3Qty,
            int t4Qty,
            int t5Qty
        )
        {
            return new PositionInfo
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
                EntryOrderType = OrderType.Market,
                IsRMATrade = false,
                IsFFMATrade = true,
                OcoGroupId = "V12_" + GetStableHash(entryName),
            };
        }

        // -- Block-E: market order submission + null-abort + FSM state enqueue
        private bool SubmitFFMAManualMarketOrder(
            MarketPosition direction,
            int contracts,
            string entryName,
            PositionInfo pos
        )
        {
            Order entryOrder =
                direction == MarketPosition.Long
                    ? SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Market, contracts, 0, 0, "", entryName)
                    : SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Market, contracts, 0, 0, "", entryName);

            // A1-1/A2-1: Null-abort rollback (Build 960 audit fix)
            if (entryOrder == null)
            {
                Print(
                    "[ENTRY_ABORT] FFMA_MANUAL_MARKET SubmitOrderUnmanaged returned null for "
                        + entryName
                        + ". Rolling back."
                );
                return false;
            }

            var en966ap = entryName;
            var p966ap = pos;
            Enqueue(ctx =>
            {
                ctx.activePositions[en966ap] = p966ap;
            });

            var en966 = entryName;
            var eo966 = entryOrder;
            Enqueue(ctx =>
            {
                ctx.entryOrders[en966] = eo966;
            });

            return true;
        }

        // -- Block-D: diagnostic Print log calls -- cold path, no decisions
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void LogFFMAManualMarketEntry(PositionInfo pos, double ema9Value)
        {
            Print(
                string.Format(
                    "V12.27 FFMA_MANUAL_MARKET: {0} {1}@MARKET | Stop: {2:F2} (candle {3}) | Toward EMA9={4:F2}",
                    pos.Direction,
                    pos.TotalContracts,
                    pos.InitialStopPrice,
                    pos.Direction == MarketPosition.Long ? "low" : "high",
                    ema9Value
                )
            );
            Print(
                string.Format(
                    "V12.27 FFMA_MANUAL_MARKET TARGETS: T1:{0}@{1:F2} | T2:{2}@{3:F2} | T3:{4}@{5:F2} | T4:{6}@{7:F2} | T5:{8}@{9:F2}",
                    pos.T1Contracts,
                    pos.Target1Price,
                    pos.T2Contracts,
                    pos.Target2Price,
                    pos.T3Contracts,
                    pos.Target3Price,
                    pos.T4Contracts,
                    pos.Target4Price,
                    pos.T5Contracts,
                    pos.Target5Price
                )
            );
        }

        #endregion
    }
}
