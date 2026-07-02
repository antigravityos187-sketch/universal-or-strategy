// Build 971: UI.IPC.Commands.Config -- HandleTrimCommand, HandleConfigCommand, TryApplyConfig*, HandleToggleAccountCommand
// V12 UI.IPC Module (Extracted)
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
        #region IPC Commands Config

        // W7-153 T2: Constructs "Trim_" + signalName, truncated to 50 chars. CYC=2.
        private string BuildTrimSignalName(string signalName)
        {
            string trimSig = "Trim_" + signalName;
            if (trimSig.Length > 50)
                trimSig = trimSig.Substring(0, 50);
            return trimSig;
        }

        // W7-153 T3: SIMA fleet follower order path. CYC=1.
        private void SubmitSimaTrimOrder(PositionInfo pos, OrderAction trimAction, int rawQty, double percent)
        {
            string trimSig = BuildTrimSignalName(pos.SignalName);
            Order trimOrder = pos.ExecutingAccount.CreateOrder(
                Instrument,
                trimAction,
                OrderType.Market,
                TimeInForce.Gtc,
                rawQty,
                0,
                0,
                "",
                trimSig,
                null
            );
            pos.ExecutingAccount.Submit(new[] { trimOrder });
            Print(
                string.Format(
                    "[SIMA] TRIM {0}%: Follower {1} -> {2} closing {3} contracts",
                    (int)(percent * 100),
                    pos.SignalName,
                    pos.ExecutingAccount.Name,
                    rawQty
                )
            );
        }

        // W7-153 T4: NinjaTrader unmanaged order path. CYC=1.
        private void SubmitUnmanagedTrimOrder(PositionInfo pos, OrderAction trimAction, int rawQty, double percent)
        {
            Print(
                string.Format(
                    "IPC Trim: Closing {0} of {1} contracts for {2} ({3:P0})",
                    rawQty,
                    pos.RemainingContracts,
                    pos.SignalName,
                    percent
                )
            );
            SubmitOrderUnmanaged(0, trimAction, OrderType.Market, rawQty, 0, 0, "", "Trim_" + pos.SignalName);
        }

        // W7-153 T1: Safe trim quantity. Returns -1 when trim is impossible. CYC=3.
        private int ComputeSafeTrimQty(int remaining, double percent)
        {
            // V10.3.1 FIX: Math.Max(1, ...) ensures we always trim at least 1 contract.
            int rawQty = Math.Max(1, (int)Math.Floor(remaining * percent));
            if (remaining - rawQty < 1)
                rawQty = remaining - 1;
            if (rawQty >= 1 && (remaining - rawQty) >= 1)
                return rawQty;
            return -1;
        }

        // W7-153 T5: Per-position trim orchestration. CYC=6.
        private void TrimSinglePosition(PositionInfo pos, double percent)
        {
            if (pos.RemainingContracts <= 1)
            {
                Print(
                    string.Format("IPC Trim SKIPPED: {0} has only 1 contract - use FLATTEN to close", pos.SignalName)
                );
                return;
            }

            int rawQty = ComputeSafeTrimQty(pos.RemainingContracts, percent);
            if (rawQty == -1)
            {
                Print(
                    string.Format(
                        "IPC Trim SKIPPED: {0} contracts for {1} - cannot satisfy {2:P0} trim with 1+ remaining",
                        pos.RemainingContracts,
                        pos.SignalName,
                        percent
                    )
                );
                return;
            }

            OrderAction trimAction = pos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;

            // [1102Z-F]: Route to fleet follower account when applicable
            if (EnableSIMA && pos.IsFollower && pos.ExecutingAccount != null)
                SubmitSimaTrimOrder(pos, trimAction, rawQty, percent);
            else
                SubmitUnmanagedTrimOrder(pos, trimAction, rawQty, percent);
        }

        private void HandleTrimCommand(string action, string[] parts)
        {
            double percent = action == "TRIM_50" ? 0.5 : 0.25;
            // V12.1101E [A-3/SK-02]: Snapshot .Values before iterating.
            // [1102Z-F]: TRIM now routes to pos.ExecutingAccount for fleet followers.
            foreach (var pos in activePositions.Values.ToArray())
                TrimSinglePosition(pos, percent);
        }

        /// <summary>
        /// Handles CONFIG -- syncs T1-T5 values/types, stop multiplier, risk, and target count.
        /// Format: CONFIG|Mode|COUNT:3;T1:1.0;T1TYPE:Points;T2:0.5;T2TYPE:ATR;...
        /// Build 945: Refactored into sub-handlers to reduce cyclomatic complexity (CS-R1140).
        /// </summary>
        private void HandleConfigCommand(string[] parts)
        {
            // V12 PRO: Parse the full config sync from side panel
            if (parts.Length <= 2)
            {
                return;
            }

            string configMode = parts[1];
            string configContent = parts[2];
            string[] settingsItems = configContent.Split(';');
            foreach (string setting in settingsItems)
            {
                if (string.IsNullOrEmpty(setting))
                {
                    continue;
                }
                string[] kv = setting.Split(':');
                if (kv.Length < 2)
                {
                    continue;
                }
                string key = kv[0].ToUpperInvariant();
                string val = kv[1];
                if (TryApplyConfigTargets(key, val))
                {
                    continue;
                }
                if (TryApplyConfigRisk(key, val, configMode))
                {
                    continue;
                }
                TryApplyConfigMode(key, val);
            }
            // Build 1106: Update current mode's profile cache so mode-switch remembers these values
            string currentMode = GetCurrentConfigMode();
            _modeProfiles[currentMode] = SnapshotCurrentConfig();

            Print(string.Format("[V12] Sync All CONFIG ({0}) Applied: {1}", configMode, configContent));
            MarkStickyDirty(); // Build 1103: Persist config sync
        }

        /// <summary>Build 945: Config sub-handler -- target values and types (T1-T5, COUNT, CIT).</summary>
        private bool TryApplyConfigTargets(string key, string val)
        {
            if (TryApplyConfigTarget_Value(key, val))
            {
                return true;
            }
            if (TryApplyConfigTarget_Type(key, val))
            {
                return true;
            }
            return TryApplyConfigTarget_Count(key, val);
        }

        // W7-017 T1: Map key string "T1"-"T5" to slot index. CYC=6.
        private bool TryResolveTargetKeyIndex(string key, out int index)
        {
            if (key == "T1")
            {
                index = 1;
                return true;
            }
            if (key == "T2")
            {
                index = 2;
                return true;
            }
            if (key == "T3")
            {
                index = 3;
                return true;
            }
            if (key == "T4")
            {
                index = 4;
                return true;
            }
            if (key == "T5")
            {
                index = 5;
                return true;
            }
            index = -1;
            return false;
        }

        // W7-017 T2 / W7-152 T1 core: Parse val as double, validate multiplier. CYC=3.
        private bool TryParseAndValidateTargetValue(string val, out double parsed, out string rejectReason)
        {
            rejectReason = null;
            if (!double.TryParse(val, out parsed))
                return false;
            if (!ValidateIpcMultiplier(parsed, out rejectReason))
                return false;
            return true;
        }

        // W7-017 T3: Dispatch by index to correct TargetNValue property. CYC=6.
        private void ApplyTargetValueByIndex(int index, double value)
        {
            switch (index)
            {
                case 1:
                    Target1Value = value;
                    break;
                case 2:
                    Target2Value = value;
                    break;
                case 3:
                    Target3Value = value;
                    break;
                case 4:
                    Target4Value = value;
                    break;
                case 5:
                    Target5Value = value;
                    break;
            }
        }

        // W7-152 T1: Validated target value application via index. CYC=3.
        private void ApplyValidatedTargetValue(int index, string val, string key)
        {
            if (!TryParseAndValidateTargetValue(val, out double parsed, out string rejectReason))
            {
                if (rejectReason != null)
                    Print($"[IPC REJECT] {key} value rejected: {rejectReason}");
                return;
            }
            ApplyTargetValueByIndex(index, parsed);
        }

        // W7-017 parent after all extractions. CYC=5.
        private bool TryApplyConfigTarget_Value(string key, string val)
        {
            if (key == "CIT")
            {
                ChaseIfTouchPoints = val;
                return true;
            }

            if (!TryResolveTargetKeyIndex(key, out int index))
                return false;

            if (!double.TryParse(val, out double v))
                return true;

            string vmReason;
            if (!ValidateIpcMultiplier(v, out vmReason))
            {
                Print($"[IPC REJECT] {key} value {v} rejected: {vmReason}");
                return true;
            }

            ApplyTargetValueByIndex(index, v);
            return true;
        }

        private bool TryApplyConfigTarget_Type(string key, string val)
        {
            var setter = GetTargetTypeSetter(key);
            if (setter == null)
                return false;
            if (TryParseTargetMode(val, out var parsed))
                setter(parsed);
            return true;
        }

        private Action<TargetMode> GetTargetTypeSetter(string key)
        {
            if (key == "T1TYPE")
                return v => T1Type = v;
            if (key == "T2TYPE")
                return v => T2Type = v;
            if (key == "T3TYPE")
                return v => T3Type = v;
            if (key == "T4TYPE")
                return v => T4Type = v;
            if (key == "T5TYPE")
                return v => T5Type = v;
            return null;
        }

        private bool TryApplyConfigTarget_Count(string key, string val)
        {
            if (key != "COUNT")
            {
                return false;
            }

            if (int.TryParse(val, out int v))
            {
                // FIX-B [Build 1102Z]: Clamp + lock to prevent IPC race with SIMA dispatch loop.
                int clamped = Math.Max(1, Math.Min(5, v));
                activeTargetCount = clamped;
            }
            return true;
        }

        /// <summary>Build 945: Config sub-handler -- risk parameters (STR, MAX).</summary>
        private bool TryApplyConfigRisk(string key, string val, string configMode)
        {
            if (key == "STR")
            {
                if (double.TryParse(val, out double v))
                {
                    string vmReason;
                    if (!ValidateIpcMultiplier(v, out vmReason))
                    {
                        Print($"[IPC REJECT] STR multiplier {v} rejected: {vmReason}");
                    }
                    else if (configMode == "RMA")
                    {
                        RMAStopATRMultiplier = v;
                    }
                    else
                    {
                        StopMultiplier = v;
                    }
                }
                return true;
            }
            if (key == "MAX")
            {
                if (double.TryParse(val, out double v))
                {
                    MaxRiskAmount = v;
                    RiskPerTrade = v;
                }
                return true;
            }
            return false;
        }

        /// <summary>Build 945: Config sub-handler -- mode flags (TRMA, RRMA).</summary>
        private bool TryApplyConfigMode(string key, string val)
        {
            if (key == "TRMA")
            {
                isTrendRmaMode = (val == "1");
                return true;
            }
            if (key == "RRMA")
            {
                isRetestRmaMode = (val == "1");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles TOGGLE_ACCOUNT -- enables or disables a specific account in the fleet.
        /// Build 935 [B935-P1]: Resolves UI aliases (F01, F02) via ResolveAccountName before
        /// writing to activeFleetAccounts. Returns early with a rejection log on null resolve.
        /// Format: TOGGLE_ACCOUNT|&lt;alias_or_name&gt;|&lt;0|1&gt;
        /// </summary>
        private void HandleToggleAccountCommand(string[] parts)
        {
            if (parts.Length <= 2)
            {
                Print($"V12 IPC REJECT: TOGGLE_ACCOUNT requires 3 parts, got {parts.Length}");
                return;
            }

            // Build 935 [B935-P1]: Resolve alias -> real account name. Guard null before any broker call.
            string resolvedName = ResolveAccountName(parts[1]);
            if (resolvedName == null)
            {
                // ResolveAccountName already logged the rejection; add caller context.
                Print($"V12 IPC REJECT: TOGGLE_ACCOUNT aborted -- unresolvable alias '{parts[1]}'");
                return;
            }

            bool active = parts[2] == "1";
            // V12.1101E [A-2]: Lock IPC writes to activeFleetAccounts -- this dict is also
            // read by the strategy thread (ExecuteMultiAccountMarket) without a lock.
            activeFleetAccounts[resolvedName] = active;
            Print($"[V12.2] TOGGLE_ACCOUNT: {resolvedName} (resolved from '{parts[1]}') | Active={active}");
            MarkStickyDirty(); // Build 1103: Persist fleet toggle
        }

        /// <summary>
        /// Build 942 [FIX-2]: Handles DIAG_FLEET and DIAG_IPC commands.
        /// Extracted from ProcessIpcCommands to reduce cyclomatic complexity (DeepSource CS-R1140).
        /// </summary>
        private bool TryHandleDiagCommand(string action, string[] parts)
        {
            if (action == "DIAG_FLEET")
            {
                HandleFleetCommand(action, parts);
                return true;
            }
            if (action == "DIAG_IPC")
            {
                // T-Q1: Toggle catch logging flag
                _diagIpc = !_diagIpc;
                Print("[DIAG_IPC] Catch logging: " + (_diagIpc ? "ENABLED" : "DISABLED"));

                Print("[DIAG_IPC] Invalid UTF-8 count   : " + _ipcInvalidUtf8Count);
                Print("[DIAG_IPC] Allowlist reject count: " + _ipcAllowlistRejectCount);
                Print("[DIAG_IPC] Hardening reject count: " + _ipcHardeningRejectCount);
                Print("[DIAG_IPC] Queue depth peak      : " + _ipcQueueDepthPeak);
                return true;
            }
            return false;
        }

        // Build 943: Sub-handlers extracted from ProcessIpcCommands (CS-R1140)

        /// <summary>
        /// Handles mode-switching commands: SET_RMA_MODE, SYNC_MODE, MKT_SYNC,
        /// SYNC_ALL, SET_MODE, MODE_*, EXEC_*, FFMA_DISARM.
        /// </summary>
        #endregion
    }
}
