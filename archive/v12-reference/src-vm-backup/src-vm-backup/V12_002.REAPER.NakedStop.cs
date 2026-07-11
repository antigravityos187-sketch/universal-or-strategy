// V12 REAPER Emergency Stop -- Naked-position hard stop protection (Build 1102R)
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Strategies;

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class V12_002 : Strategy
    {
        #region V12 REAPER Emergency Stop

        /// <summary>
        /// Build 1102R: Processes queued naked-position emergency stop requests on the strategy thread.
        /// Called via TriggerCustomEvent from the Reaper background thread.
        /// Submits a StopMarket order at MaximumStop ticks from current close to protect the naked position.
        /// </summary>
        private void ProcessReaperNakedStopQueue()
        {
            while (_reaperNakedStopQueue.TryDequeue(out var item))
            {
                try
                {
                    Account acct = Account.All.FirstOrDefault(a => a.Name == item.AccountName);
                    if (acct == null)
                    {
                        Print(
                            string.Format("[REAPER][NAKED_STOP] Account {0} not found -- skipping.", item.AccountName)
                        );
                        ClearNakedStopInFlight(ExpKey(item.AccountName)); // NEW: Accessor method
                        continue;
                    }

                    // NEW: Call extracted helper with ATR floor fix
                    var (stopPrice, closeAction) = CalculateEmergencyStopPrice(item.Direction, Close[0], item.Qty);

                    string signalName = "EMERGENCY_STOP_" + item.AccountName;
                    Order emergencyStop = acct.CreateOrder(
                        Instrument,
                        closeAction,
                        OrderType.StopMarket,
                        TimeInForce.Gtc,
                        item.Qty,
                        0,
                        stopPrice,
                        "",
                        signalName,
                        null
                    );

                    acct.Submit(new[] { emergencyStop });

                    ClearNakedStopInFlight(ExpKey(item.AccountName)); // NEW: Accessor method
                    Print(
                        string.Format(
                            "[REAPER][EMERGENCY_STOP] Submitted StopMarket for {0}: {1} {2}ct @ {3:F2} (Dist={4:F2})",
                            item.AccountName,
                            closeAction,
                            item.Qty,
                            stopPrice,
                            Math.Abs(Close[0] - stopPrice)
                        )
                    );
                }
                catch (Exception ex)
                {
                    ClearNakedStopInFlight(ExpKey(item.AccountName)); // NEW: Accessor method
                    Print(string.Format("[REAPER][EMERGENCY_STOP_FAIL] {0}: {1}", item.AccountName, ex.Message));
                }
            }
        }

        #endregion
    }
}
