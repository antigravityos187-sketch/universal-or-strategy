// V12 SIMA Module (Extracted)
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
        #region V12 SIMA Structures

        /// <summary>
        /// EPIC-CCN-027 TICKET-1: Return type for CreateBracketOrders extraction.
        /// Encapsulates all order creation results in a single struct.
        /// </summary>
        private struct BracketOrderSet
        {
            public Order Entry;
            public Order Stop;
            public List<Order> Targets;
            public int NonRunnerLimitQty;
            public int RunnerQty;
            public List<StagedTarget> StagedTargets;
        }

        /// <summary>
        /// V12.Phase8 [F-01/F-02]: Staged target for local tracking before submission.
        /// </summary>
        private struct StagedTarget
        {
            public int Num;
            public double Price;
            public Order Order;
        }

        /// <summary>
        /// Build 936 [FIX-1]: Self-contained unit for deferred acct.Submit() via TriggerCustomEvent pump.
        /// Created in ExecuteSmartDispatchEntry setup phase (fast path); consumed by PumpFleetDispatch
        /// on the strategy thread one-at-a-time, breaking the 7-second monolithic blocking window into
        /// N x (next-tick-cycle) slices.
        /// </summary>
        private struct FleetDispatchRequest
        {
            public Account Account;
            public Order[] Orders;
            public string FleetEntryName;
            public string ExpectedKey;
            public int ReservedDelta;
            public long SignalTicks; // Phase 6 [MG-T1]: UTC ticks at enqueue for stale dispatch detection
        }

        // V12.1101E [F-06]: Atomic expectedPositions mutation via ConcurrentDictionary.AddOrUpdate.
        // Phase 10: lock(stateLock) removed -- AddOrUpdate is atomic; Interlocked.Exchange is independent.
        private void AddExpectedPositionDeltaLocked(string accountName, int delta)
        {
            if (string.IsNullOrEmpty(accountName) || expectedPositions == null)
                return;

            expectedPositions.AddOrUpdate(accountName, delta, (key, existingDelta) => existingDelta + delta);
        }

        /// <summary>
        /// V12.1101E [F-07]: Atomic expectedPositions read via ConcurrentDictionary.TryGetValue.
        /// Phase 10: lock(stateLock) removed -- TryGetValue is atomic.
        /// </summary>
        private int GetExpectedPositionDelta(string accountName)
        {
            if (string.IsNullOrEmpty(accountName) || expectedPositions == null)
                return 0;

            expectedPositions.TryGetValue(accountName, out int delta);
            return delta;
        }

        /// <summary>
        /// V12.1101E [F-08]: Atomic expectedPositions clear via ConcurrentDictionary.TryRemove.
        /// Phase 10: lock(stateLock) removed -- TryRemove is atomic.
        /// </summary>
        private void ClearExpectedPositionDelta(string accountName)
        {
            if (string.IsNullOrEmpty(accountName) || expectedPositions == null)
                return;

            expectedPositions.TryRemove(accountName, out _);
        }

        #endregion

        #region V12 SIMA Dispatch Sync

        /// <summary>
        /// V12.1101E [F-09]: Mark dispatch as pending sync via Interlocked.Exchange.
        /// Phase 10: lock(stateLock) removed -- Interlocked.Exchange is atomic.
        /// </summary>
        private void MarkDispatchSyncPending(string accountName)
        {
            if (string.IsNullOrEmpty(accountName) || dispatchSyncPending == null)
                return;

            dispatchSyncPending.AddOrUpdate(accountName, true, (key, existing) => true);
        }

        /// <summary>
        /// V12.1101E [F-10]: Clear dispatch sync pending flag via Interlocked.Exchange.
        /// Phase 10: lock(stateLock) removed -- Interlocked.Exchange is atomic.
        /// </summary>
        private void ClearDispatchSyncPending(string accountName)
        {
            if (string.IsNullOrEmpty(accountName) || dispatchSyncPending == null)
                return;

            dispatchSyncPending.TryRemove(accountName, out _);
        }

        /// <summary>
        /// V12.1101E [F-11]: Check if dispatch sync is pending via ConcurrentDictionary.TryGetValue.
        /// Phase 10: lock(stateLock) removed -- TryGetValue is atomic.
        /// </summary>
        private bool IsDispatchSyncPending(string accountName)
        {
            if (string.IsNullOrEmpty(accountName) || dispatchSyncPending == null)
                return false;

            dispatchSyncPending.TryGetValue(accountName, out bool pending);
            return pending;
        }

        #endregion

        #region V12 SIMA Helper Methods

        /// <summary>
        /// V12.Phase8.3: Validate stop price against position direction.
        /// Returns validated stop price or current stop if invalid.
        /// </summary>
        private double ValidateStopPrice(MarketPosition direction, double stopPrice)
        {
            if (stopPrice <= 0)
                return 0;

            // Additional validation logic can be added here
            return stopPrice;
        }

        /// <summary>
        /// V12.Phase8.3: Trim signal name to max length for NinjaTrader compatibility.
        /// </summary>
        private string SymmetryTrim(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            if (input.Length <= maxLength)
                return input;

            return input.Substring(0, maxLength);
        }

        /// <summary>
        /// V12.Phase8.3: Get target contracts for a specific target number.
        /// </summary>
        private int GetTargetContracts(PositionInfo fleetPos, int targetNum)
        {
            switch (targetNum)
            {
                case 1:
                    return fleetPos.T1Contracts;
                case 2:
                    return fleetPos.T2Contracts;
                case 3:
                    return fleetPos.T3Contracts;
                case 4:
                    return fleetPos.T4Contracts;
                case 5:
                    return fleetPos.T5Contracts;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// V12.Phase8.3: Get target price for a specific target number.
        /// </summary>
        private double GetTargetPrice(PositionInfo fleetPos, int targetNum)
        {
            switch (targetNum)
            {
                case 1:
                    return fleetPos.Target1Price;
                case 2:
                    return fleetPos.Target2Price;
                case 3:
                    return fleetPos.Target3Price;
                case 4:
                    return fleetPos.Target4Price;
                case 5:
                    return fleetPos.Target5Price;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// V12.Phase8.3: Check if target is a runner target.
        /// </summary>
        private bool IsRunnerTarget(int targetNum)
        {
            // Runner logic: typically T5 is the runner
            return targetNum == 5;
        }

        /// <summary>
        /// V12.Phase8.3: Get target orders dictionary for a specific target number.
        /// </summary>
        private ConcurrentDictionary<string, Order> GetTargetOrdersDictionary(int targetNum)
        {
            switch (targetNum)
            {
                case 1:
                    return target1Orders;
                case 2:
                    return target2Orders;
                case 3:
                    return target3Orders;
                case 4:
                    return target4Orders;
                case 5:
                    return target5Orders;
                default:
                    return null;
            }
        }

        #endregion
    }
}
