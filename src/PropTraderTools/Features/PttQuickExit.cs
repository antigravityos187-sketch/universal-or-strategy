// PTT-COPIER-B41 -- PttQuickExit.cs
// Quick Exit: per-instrument bracket swap (1-chart scope).
// B41: 2 classes -- PttQuickExit (per-chart execution) + InstrumentDefaults (static tick mappings).
// Jane Street rules: JS-001 (no throw), JS-002 (no return null), JS-021 (no lock),
// JS-033 (no async void). OCO counter delegated to CopyEngine.NextQxOcoId().

using System;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;

namespace PropTraderTools
{
    /// <summary>
    /// PttQuickExit: per-chart Quick Exit bracket swap.
    /// Button scope: leader + all followers for _instrument (this chart's instrument).
    /// Idempotent: press again any time to replace PTT-QX orders with new t1/t2 offsets.
    /// </summary>
    internal sealed class PttQuickExit
    {
        // B41: OCO sequence counter lives in CopyEngine.NextQxOcoId() -- the true NT8 AddOn singleton.
        // A static field here is insufficient: NT8 can isolate class loads per chart/panel context,
        // giving each context its own static _qxSeq starting at 0 -> always "PTT-QX-00001".
        // CopyEngine._instance is a static readonly field on the AddOn-level class -- one instance
        // for the entire NT8 process regardless of how many panels or charts are open.

        /// <summary>
        /// Execute: per-chart Quick Exit bracket swap.
        /// CYC=5: null/flat guard(1) + snapshotStop guard(2) + isLong(3) + T1-null(4) + T2-null(5).
        /// JS-001: no throw -- logs instead. JS-021: no lock -- CopyEngine.NextQxOcoId uses Interlocked.
        /// NT8-007: CreateOrder arg12 = (CustomOrder)null. NT8-013: DateTime.MaxValue for GTC.
        /// NT8-014: signal name = "PTT-QX-*". NT8-049: Limit arg6=limitPrice, arg7=0; StopMarket arg6=0, arg7=stopPrice.
        /// </summary>
        internal void Execute(Account leader, Instrument instr, int t1Ticks, int t2Ticks)
        {
            // Step 1: null/flat guard
            Position pos = null;
            if (leader != null)
                foreach (Position p in leader.Positions)
                    if (p.Instrument == instr) { pos = p; break; }
            if (pos == null || pos.Quantity == 0)
            {
                NinjaTrader.Code.Output.Process(
                    "PTT-QX: flat skip -- " + (leader != null ? leader.Name : "NULL"),
                    NinjaTrader.NinjaScript.PrintTo.OutputTab1);
                return;
            }

            // Step 2: SnapshotStopPrice -- capture current stop price before cancel
            double snapshotStop = SnapshotStopPrice(leader, instr);

            // Step 3: CancelStaleBrackets -- cancel ATM bracket + previous PTT-QX orders
            CopyEngine.Instance?.CancelQxBrackets(leader, instr);

            // Step 4: OCO ID -- monotonic sequence from CopyEngine singleton, unique per press per session
            string ocoId = CopyEngine.Instance?.NextQxOcoId() ?? ("PTT-QX-" + Guid.NewGuid().ToString("N").Substring(0, 8));

            // Step 5: compute prices and quantities
            bool isLong = pos.MarketPosition == MarketPosition.Long;
            double entryPx = pos.AveragePrice;
            double tick = instr.MasterInstrument?.TickSize ?? 0.25;

            // T1 price: entryPx + (isLong ? +t1Ticks : -t1Ticks) * tick
            double rawT1 = isLong
                ? entryPx + t1Ticks * tick
                : entryPx - t1Ticks * tick;
            double t1Price = Math.Round(rawT1 / tick) * tick;

            // T2 price: entryPx + (isLong ? +t2Ticks : -t2Ticks) * tick
            double rawT2 = isLong
                ? entryPx + t2Ticks * tick
                : entryPx - t2Ticks * tick;
            double t2Price = Math.Round(rawT2 / tick) * tick;

            // Quantity split: T1 = ceil(qty/2), T2 = qty - T1
            int t1Qty = (int)Math.Ceiling(pos.Quantity / 2.0);
            int t2Qty = pos.Quantity - t1Qty;

            // B41 HOTFIX: Two independent OCO groups so T1 fill does not orphan the T2 residual.
            // Single-OCO design: T1 fills -> OCO cancels Stop+T2 -> 5 lots unprotected.
            // Two-OCO design:
            //   Group A (ocoId):  PTT-QX-Stop (t1Qty)  + PTT-QX-T1 (t1Qty)
            //   Group B (ocoId2): PTT-QX-Stop2 (t2Qty) + PTT-QX-T2 (t2Qty)
            // T1 fills -> OCO-A cancels Stop only (t1Qty stop). Stop2+T2 in OCO-B remain live.
            // T2 fills -> OCO-B cancels Stop2. Position fully exited, both stops cleaned up.
            // CancelQxBrackets still cancels all PTT-QX-* via the PTT-QX- prefix filter.
            string ocoId2 = CopyEngine.Instance?.NextQxOcoId()
                ?? ("PTT-QX-" + Guid.NewGuid().ToString("N").Substring(0, 8));

            // Step 6a: Submit PTT-QX-Stop for T1 slice (OCO group A)
            if (snapshotStop > 0)
            {
                try
                {
                    var stopOrd = leader.CreateOrder(
                        instr,
                        isLong ? OrderAction.Sell : OrderAction.BuyToCover,
                        OrderType.StopMarket,
                        OrderEntry.Manual,
                        TimeInForce.Gtc,
                        t1Qty,          // covers T1 slice only
                        0,
                        snapshotStop,
                        ocoId,
                        "PTT-QX-Stop",
                        DateTime.MaxValue,
                        (CustomOrder)null);
                    if (stopOrd != null)
                        leader.Submit(new[] { stopOrd });
                    else
                        NinjaTrader.Code.Output.Process("PTT-QX: Stop CreateOrder returned null -- position unprotected", NinjaTrader.NinjaScript.PrintTo.OutputTab1);
                }
                catch (Exception ex)
                {
                    NinjaTrader.Code.Output.Process("PTT-QX: Stop submit exception -- " + ex.Message, NinjaTrader.NinjaScript.PrintTo.OutputTab1);
                }
            }

            // Step 6b: Submit PTT-QX-Stop2 for T2 slice (OCO group B) -- only if t2Qty > 0
            if (snapshotStop > 0 && t2Qty > 0)
            {
                try
                {
                    var stop2Ord = leader.CreateOrder(
                        instr,
                        isLong ? OrderAction.Sell : OrderAction.BuyToCover,
                        OrderType.StopMarket,
                        OrderEntry.Manual,
                        TimeInForce.Gtc,
                        t2Qty,          // covers T2 slice only
                        0,
                        snapshotStop,
                        ocoId2,
                        "PTT-QX-Stop2",
                        DateTime.MaxValue,
                        (CustomOrder)null);
                    if (stop2Ord != null)
                        leader.Submit(new[] { stop2Ord });
                    else
                        NinjaTrader.Code.Output.Process("PTT-QX: Stop2 CreateOrder returned null", NinjaTrader.NinjaScript.PrintTo.OutputTab1);
                }
                catch (Exception ex)
                {
                    NinjaTrader.Code.Output.Process("PTT-QX: Stop2 submit exception -- " + ex.Message, NinjaTrader.NinjaScript.PrintTo.OutputTab1);
                }
            }

            // Step 7: Submit PTT-QX-T1 (Limit, OCO group A)
            try
            {
                var t1Ord = leader.CreateOrder(
                    instr,
                    isLong ? OrderAction.Sell : OrderAction.BuyToCover,
                    OrderType.Limit,
                    OrderEntry.Manual,
                    TimeInForce.Gtc,
                    t1Qty,
                    t1Price,
                    0,
                    ocoId,
                    "PTT-QX-T1",
                    DateTime.MaxValue,
                    (CustomOrder)null);
                if (t1Ord != null)
                    leader.Submit(new[] { t1Ord });
                else
                    NinjaTrader.Code.Output.Process("PTT-QX: T1 CreateOrder returned null -- skip", NinjaTrader.NinjaScript.PrintTo.OutputTab1);
            }
            catch (Exception ex)
            {
                NinjaTrader.Code.Output.Process("PTT-QX: T1 submit exception -- " + ex.Message, NinjaTrader.NinjaScript.PrintTo.OutputTab1);
            }

            // Step 8: Submit PTT-QX-T2 (Limit, OCO group B) -- only if t2Qty > 0
            if (t2Qty > 0)
            {
                try
                {
                    var t2Ord = leader.CreateOrder(
                        instr,
                        isLong ? OrderAction.Sell : OrderAction.BuyToCover,
                        OrderType.Limit,
                        OrderEntry.Manual,
                        TimeInForce.Gtc,
                        t2Qty,
                        t2Price,
                        0,
                        ocoId2,
                        "PTT-QX-T2",
                        DateTime.MaxValue,
                        (CustomOrder)null);
                    if (t2Ord != null)
                        leader.Submit(new[] { t2Ord });
                    else
                        NinjaTrader.Code.Output.Process("PTT-QX: T2 CreateOrder returned null", NinjaTrader.NinjaScript.PrintTo.OutputTab1);
                }
                catch (Exception ex)
                {
                    NinjaTrader.Code.Output.Process("PTT-QX: T2 submit exception -- " + ex.Message, NinjaTrader.NinjaScript.PrintTo.OutputTab1);
                }
            }

            // Step 9: raise PttBus.QuickExitFired (Card B: includes TickSize for window back-calc)
            PttBus.RaiseQuickExit(this, new QuickExitEventArgs(
                instr, entryPx, t1Price, t2Price, isLong, ocoId, tick));
        }

        /// <summary>
        /// SnapshotStopPrice: returns the stop price of any Working/Accepted stop order for this instrument.
        /// CYC=2: foreach(1), stop-type check(2). JS-002: returns double 0.0 (not null).
        /// </summary>
        private static double SnapshotStopPrice(Account acc, Instrument instr)
        {
            foreach (var o in acc.Orders)
            {
                if (o.Instrument != instr) continue;                                     // (1)
                if (o.OrderState != OrderState.Working && o.OrderState != OrderState.Accepted) continue;
                if (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)  // (2)
                    return o.StopPrice;
            }
            return 0.0;
        }
    }

    /// <summary>
    /// InstrumentDefaults: per-instrument Quick Exit tick defaults.
    /// Called from CopyEngine.GetDefaultQuickTicks() and PttQuickExit fallback paths.
    /// </summary>
    internal static class InstrumentDefaults
    {
        /// <summary>
        /// GetQuickTicks: returns (T1 ticks, T2 ticks) for the given master instrument name.
        /// MES: (4, 8) = 1pt/2pt at 0.25 tick. MGC: (2, 4) = 0.2pt/0.4pt at 0.1 tick.
        /// Default: MES ticks (4, 8). CYC=3 (null/empty guard + MES check + MGC check).
        /// JS-002: returns tuple (never returns null). ASCII-only strings.
        /// </summary>
        internal static (int t1, int t2) GetQuickTicks(string masterName)
        {
            if (string.IsNullOrEmpty(masterName)) return (4, 8);                         // (1)
            if (masterName.StartsWith("MES")) return (4, 8);                             // (2)
            if (masterName.StartsWith("MGC")) return (2, 4);                             // (3)
            return (4, 8);
        }
    }
}
