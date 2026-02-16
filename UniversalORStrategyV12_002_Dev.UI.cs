// V12.16 MODULAR: UI, IPC & Compliance Module (Extracted from main file)
// Contains: UI handlers, IPC Integration, Apex Compliance Hub, Position Sizing
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Strategies;
using System.Net;
using System.Net.Sockets;

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class UniversalORStrategyV12_002_Dev : Strategy
    {
        #region UI

        // V12.44: UpdateRMAModeDisplay() removed — legacy chart UI no-op
        // V12.44: OnKeyUp() removed — empty handler, R key no longer used for RMA
        // V12.44: UpdateDisplay() permanently removed — all 28+ call sites cleaned

        private void AttachHotkeys()
        {
            if (ChartControl?.OwnerChart != null)
            {
                ChartControl.OwnerChart.PreviewKeyDown += OnKeyDown;
            }
        }

        private void DetachHotkeys()
        {
            if (ChartControl?.OwnerChart != null)
            {
                ChartControl.OwnerChart.PreviewKeyDown -= OnKeyDown;
            }
        }

        private void AttachChartClickHandler()
        {
            if (ChartControl != null)
            {
                ChartControl.PreviewMouseLeftButtonDown += OnChartClick;
            }
        }

        private void DetachChartClickHandler()
        {
            if (ChartControl != null)
            {
                ChartControl.PreviewMouseLeftButtonDown -= OnChartClick;
            }
        }

        /// <summary>
        /// V8.6: Click-to-Price handler for RMA and MOMO entries
        /// RMA uses Limit orders (click above = short, click below = long)
        /// MOMO uses Stop Market orders (click above = long, click below = short)
        /// </summary>
        private void OnChartClick(object sender, MouseButtonEventArgs e)
        {
            // Check if Shift is held OR RMA/MOMO button mode is active
            bool shiftHeld = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool rmaActive = (RMAEnabled && (shiftHeld || isRMAModeActive));
            bool momoActive = (MOMOEnabled && isMOMOModeActive);

            if (!rmaActive && !momoActive) return;

            try
            {
                if (ChartControl == null || ChartPanel == null) return;

                double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];

                // ═══════════════════════════════════════════════════════════════════
                // V12.4: ChartPanel-based price conversion (PROVEN WORKING)
                // ChartPanel.H includes time axis - effective price area is ~67% of height
                // ═══════════════════════════════════════════════════════════════════
                Point mouseInPanel = e.GetPosition(ChartPanel as System.Windows.IInputElement);

                double panelHeight = ChartPanel.H;
                double maxPrice = ChartPanel.MaxValue;
                double minPrice = ChartPanel.MinValue;
                double priceRange = maxPrice - minPrice;

                // CRITICAL: ChartPanel.H includes time axis at bottom
                // The actual price plotting area is approximately 67% of total panel height
                double effectivePriceHeight = panelHeight * 0.667;

                // Clamp Y to valid range
                double yInPanel = mouseInPanel.Y;
                if (yInPanel < 0) yInPanel = 0;
                if (yInPanel > effectivePriceHeight) yInPanel = effectivePriceHeight;

                // Convert: Y=0 is top (maxPrice), Y=effectivePriceHeight is bottom (minPrice)
                double yRatio = yInPanel / effectivePriceHeight;
                double clickPrice = maxPrice - (yRatio * priceRange);

                string modeLabel = momoActive ? "MOMO" : "RMA";
                Print(string.Format("{0} v12.4 CLICK: panelY={1:F1}, effH={2:F1}, ratio={3:F3}, price={4:F2} (Market={5:F2})",
                    modeLabel, mouseInPanel.Y, effectivePriceHeight, yRatio, clickPrice, currentPrice));

                // Round to tick size
                clickPrice = Instrument.MasterInstrument.RoundToTickSize(clickPrice);

                // Validate price is within chart range
                if (clickPrice < minPrice - priceRange || clickPrice > maxPrice + priceRange)
                {
                    Print(string.Format("{0}: Click price {1:F2} outside valid range [{2:F2} - {3:F2}]",
                        modeLabel, clickPrice, minPrice, maxPrice));
                    return;
                }

                if (momoActive)
                {
                    ExecuteMOMOEntry(clickPrice);
                }
                else
                {
                    MarketPosition direction = (clickPrice > currentPrice) ? MarketPosition.Short : MarketPosition.Long;
                    ExecuteRMAEntryV2(clickPrice, direction);

                    if (isRMAButtonClicked)
                    {
                        isRMAButtonClicked = false;
                        isRMAModeActive = false;

                        // V12.43: Lightweight deactivation — only signal mode change, don't clobber config
                        SendResponseToRemote("SET_RMA_MODE|OFF");
                        Print("V12.43: RMA auto-deactivated after entry (lightweight signal, no CONFIG clobber)");
                    }
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Print("ERROR OnChartClick: " + ex.Message);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Basic hotkeys
            if (e.Key == Key.L) { ExecuteLong(); e.Handled = true; }
            else if (e.Key == Key.S) { ExecuteShort(); e.Handled = true; }
            else if (e.Key == Key.F) { FlattenAll(); e.Handled = true; }

            // v5.12: T1 Actions (1 + letter)
            else if (Keyboard.IsKeyDown(Key.D1) || Keyboard.IsKeyDown(Key.NumPad1))
            {
                if (e.Key == Key.M) { ExecuteTargetAction("T1", "market"); e.Handled = true; }
                else if (e.Key == Key.O) { ExecuteTargetAction("T1", "1point"); e.Handled = true; }
                else if (e.Key == Key.W) { ExecuteTargetAction("T1", "2point"); e.Handled = true; }
                else if (e.Key == Key.K) { ExecuteTargetAction("T1", "marketprice"); e.Handled = true; }
                else if (e.Key == Key.B) { ExecuteTargetAction("T1", "breakeven"); e.Handled = true; }
                else if (e.Key == Key.C) { ExecuteTargetAction("T1", "cancel"); e.Handled = true; }
            }

            // v5.12: T2 Actions (2 + letter)
            else if (Keyboard.IsKeyDown(Key.D2) || Keyboard.IsKeyDown(Key.NumPad2))
            {
                if (e.Key == Key.M) { ExecuteTargetAction("T2", "market"); e.Handled = true; }
                else if (e.Key == Key.O) { ExecuteTargetAction("T2", "1point"); e.Handled = true; }
                else if (e.Key == Key.W) { ExecuteTargetAction("T2", "2point"); e.Handled = true; }
                else if (e.Key == Key.K) { ExecuteTargetAction("T2", "marketprice"); e.Handled = true; }
                else if (e.Key == Key.B) { ExecuteTargetAction("T2", "breakeven"); e.Handled = true; }
                else if (e.Key == Key.C) { ExecuteTargetAction("T2", "cancel"); e.Handled = true; }
            }

            // v5.12: Runner Actions (3 + letter)
            else if (Keyboard.IsKeyDown(Key.D3) || Keyboard.IsKeyDown(Key.NumPad3))
            {
                if (e.Key == Key.M) { ExecuteRunnerAction("market"); e.Handled = true; }
                else if (e.Key == Key.O) { ExecuteRunnerAction("stop1pt"); e.Handled = true; }
                else if (e.Key == Key.W) { ExecuteRunnerAction("stop2pt"); e.Handled = true; }
                else if (e.Key == Key.B) { ExecuteRunnerAction("stopbe"); e.Handled = true; }
                else if (e.Key == Key.P) { ExecuteRunnerAction("lock50"); e.Handled = true; }  // P for Profit
                else if (e.Key == Key.D) { ExecuteRunnerAction("disabletrail"); e.Handled = true; }
            }

            // RMA uses Shift+Click (R conflicts with NT search, Ctrl conflicts with chart drag)
        }

        #endregion

        #region IPC Integration (V9.1.8)

        private void StartIpcServer()
        {
            if (isIpcRunning) return;

            try
            {
                StopIpcServer(); // Ensure clean start

                isIpcRunning = true;
                ipcCommandQueue = new ConcurrentQueue<string>();

                // V12.2: Multi-Client Support
                connectedClients = new ConcurrentBag<TcpClient>();

                ipcThread = new Thread(ListenForRemote);
                ipcThread.IsBackground = true;
                ipcThread.Name = "V10_IPC_Server";
                ipcThread.Start();

                Print(string.Format("IPC SERVER SUCCESS: Listening on Port {0} (Multi-Client)", IpcPort));
            }
            catch (Exception ex)
            {
                Print("ERROR StartIpcServer: " + ex.Message);
            }
        }

        private void ListenForRemote()
        {
            try
            {
                ipcListener = new TcpListener(IPAddress.Any, IpcPort);
                ipcListener.Start();

                while (isIpcRunning)
                {
                    if (!ipcListener.Pending())
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    // Accept new client
                    TcpClient client = ipcListener.AcceptTcpClient();
                    connectedClients.Add(client);
                    Print("V12 IPC: New Client Connected");

                    // V12.13-D: Send REQUEST_FLEET_STATE directly to the newly connected client
                    // (Previously called SendToExternalApp which connected back to port 5001 = self, causing infinite flood loop)
                    try
                    {
                        byte[] reqBytes = Encoding.UTF8.GetBytes("REQUEST_FLEET_STATE|ALL\n");
                        NetworkStream ns = client.GetStream();
                        ns.Write(reqBytes, 0, reqBytes.Length);
                        ns.Flush();
                        Print("V12 IPC: Sent REQUEST_FLEET_STATE to new client");
                    }
                    catch (Exception ex)
                    {
                        Print("V12 IPC: Failed to send fleet state request: " + ex.Message);
                    }

                    // Handle client in a separate task
                    Task.Run(() => HandleClient(client));
                }
            }
            catch (Exception)
            {
                isIpcRunning = false;
                Print("[V12.2] IPC Listener Status: Stopped/Error");
            }
            finally
            {
                if (ipcListener != null)
                {
                    try { ipcListener.Stop(); } catch { }
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    System.Text.StringBuilder lineBuffer = new System.Text.StringBuilder();
                    byte[] buffer = new byte[4096];

                    while (isIpcRunning && client.Connected)
                    {
                         if (!stream.DataAvailable)
                        {
                            Thread.Sleep(50);
                            continue;
                        }

                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break; // Disconnected

                        string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        lineBuffer.Append(chunk);

                        string accumulated = lineBuffer.ToString();
                        int lastNewline = accumulated.LastIndexOf('\n');
                        if (lastNewline < 0) continue;

                        string completeLines = accumulated.Substring(0, lastNewline);
                        lineBuffer.Clear();
                        if (lastNewline + 1 < accumulated.Length)
                        {
                            lineBuffer.Append(accumulated.Substring(lastNewline + 1));
                        }

                        string[] commands = completeLines.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string rawCmd in commands)
                        {
                            string message = rawCmd.Trim();
                            if (string.IsNullOrEmpty(message)) continue;

                            // Handle GET_LAYOUT (Synchronous Response to THIS client only)
                            if (message.StartsWith("GET_LAYOUT"))
                            {
                                // V12.Hardening: Read strategy state under stateLock to prevent data race with strategy thread
                                string configResponse;
                                lock (stateLock)
                                {
                                    string mode = isRMAModeActive ? "RMA" : "OR";
                                    configResponse = string.Format(
                                        "CONFIG|{0}|COUNT:{1};T1:{2};T1TYPE:Points;T2:{3};T2TYPE:ATR;T3:{4};T3TYPE:ATR;T4:0;T4TYPE:ATR;T5:0;T5TYPE:ATR;STR:{5};STRTYPE:Tick;MAX:{6};CIT:{7};OT:Limit;TRMA:{8};RRMA:{9};\n",
                                        mode, minContracts, Target1FixedPoints, Target2Multiplier, Target3Multiplier,
                                        StopMultiplier, MaxRiskAmount, ChaseIfTouchPoints ?? "0",
                                        isTrendRmaMode ? "1" : "0", isRetestRmaMode ? "1" : "0");
                                }
                                byte[] responseBytes = Encoding.UTF8.GetBytes(configResponse);
                                stream.Write(responseBytes, 0, responseBytes.Length);
                                stream.Flush();
                                continue;
                            }

                            // Enqueue for processing
                            ipcCommandQueue.Enqueue(message);
                            Print(string.Format("V12.1 IPC ENQUEUE: {0}", message));

                            // Trigger processing
                            try
                            {
                                TriggerCustomEvent(o => ProcessIpcCommands(), null);
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Print("V12 IPC Client Error: " + ex.Message);
            }
            finally
            {
                // Remove client from bag (rebuild bag exclusion) - ConcurrentBag doesn't have Remove,
                // effectively we just let it connect/disconnect. The SendResponse will handle dead clients.
                Print("V12 IPC: Client Disconnected");
                client.Close();
            }
        }

        private void StopIpcServer()
        {
            try
            {
                isIpcRunning = false;
                if (ipcListener != null)
                {
                    ipcListener.Stop();
                    ipcListener = null;
                }
                if (ipcThread != null && ipcThread.IsAlive)
                {
                    ipcThread.Join(500);
                }
            }
            catch { }
        }

        private void HandleExternalSignal(object sender, SignalBroadcaster.ExternalCommandSignal e)
        {
            // V10.3: Only non-winners (secondary charts) need to handle the broadcast
            // The port winner already enqueued the message locally in ListenForRemote
            if (ipcCommandQueue != null && !isIpcRunning)
            {
                Print(string.Format("V10.3 DEBUG: {0} received broadcast: {1}", Instrument.MasterInstrument.Name, e.Command));
                ipcCommandQueue.Enqueue(e.Command);

                // Force instant processing for secondary charts (so they don't wait for a tick)
                try { TriggerCustomEvent(o => ProcessIpcCommands(), null); } catch { }
            }
        }

        private void ProcessIpcCommands()
        {
            if (ipcCommandQueue == null || ipcCommandQueue.IsEmpty) return;

            while (ipcCommandQueue.TryDequeue(out string command))
            {
                try
                {
                    string[] parts = command.Split('|');
                    string action = parts[0];
                    string targetSymbol = parts.Length > 1 ? parts[1] : "Global";

                    // V12.9: Global commands bypass symbol filter entirely — these are account/fleet-level, not instrument-level
                    bool isGlobalCommand = action == "TOGGLE_ACCOUNT" || action == "SET_SIMA" ||
                                           action == "GET_FLEET" || action == "DIAG_FLEET" || action == "CANCEL_ALL" ||
                                           action == "FLATTEN" || action == "SYNC_ALL" || action == "REQUEST_FLEET_STATE";

                    // V10.3: Robust Symbol Matching (Matches MGC to GC/MGC, MES to ES/MES, etc.)
                    string mySym = Instrument.MasterInstrument.Name.ToUpper();
                    string myFull = Instrument.FullName.ToUpper();
                    string target = targetSymbol.Trim().ToUpper();

                    bool isForMe = isGlobalCommand ||  // V12.9: SIMA/Fleet commands always pass through
                                   target == "GLOBAL" ||
                                   target == "ALL" ||  // V12.13: Universal broadcast target (FLATTEN|ALL, REQUEST_FLEET_STATE|ALL)
                                   target == "ON" || target == "OFF" ||  // V12.4: Mode toggle commands (SET_RMA_MODE|ON)
                                   target == "RMA" || target == "ORB" || target == "OR" || target == "MOMO" || // V12.6: Mode-switch keywords are global
                                   mySym == target ||
                                   mySym.StartsWith(target) || // "MES" matches "MES 03-26"
                                   target.StartsWith(mySym) || // "GC" matches "GC/MGC"
                                   myFull.Contains(target) ||
                                   (target == "MES" && mySym.Contains("ES")) || // Robustness for MES/ES
                                   (target == "MYM" && mySym.Contains("YM")) || // Robustness for MYM/YM
                                   (target == "MGC" && mySym.Contains("GC"));   // Robustness for MGC/GC

                    // V12.2: Global IPC Diagnostic Log
                    Print(string.Format("V12 IPC: Received '{0}' for '{1}'. For Me? {2} (My Symbol: {3}){4}",
                        action, target, isForMe, mySym, isGlobalCommand ? " [GLOBAL CMD]" : ""));

                    if (!isForMe)
                    {
                        // Quiet ignore if it's clearly for another instrument
                        continue;
                    }

                    Print(string.Format("{0:HH:mm:ss} | IPC Executing {1} for {2}", DateTime.Now, action, Instrument.MasterInstrument.Name));

            if (action == "TRIM_25" || action == "TRIM_50")
            {
                double percent = action == "TRIM_50" ? 0.5 : 0.25;
                // V12.1101E [A-3/SK-02]: Snapshot .Values before iterating — ConcurrentDictionary.Values
                // is not safe to iterate while OnOrderUpdate/OnExecutionUpdate may remove entries.
                foreach (var pos in activePositions.Values.ToArray())
                {
                    if (pos.RemainingContracts > 1)  // V10.3.1: Need at least 2 contracts to trim (leaves 1+ after)
                    {
                        // V10.3.1 FIX: Improved Floor logic for small positions (e.g., MGC 2-lot)
                        // Math.Max(1, ...) ensures we always trim at least 1 contract when trimming
                        // This fixes the issue where 2 * 0.25 = 0.5 → Floor = 0 (no trim)
                        int rawQty = Math.Max(1, (int)Math.Floor(pos.RemainingContracts * percent));
                        int remainingAfterTrim = pos.RemainingContracts - rawQty;

                        // Safety check: Ensure at least 1 contract remains after trim (never flatten via trim)
                        if (remainingAfterTrim < 1)
                        {
                            rawQty = pos.RemainingContracts - 1;  // Adjust to leave exactly 1 contract
                        }

                        // Only execute if we're actually trimming something and leaving position open
                        if (rawQty >= 1 && (pos.RemainingContracts - rawQty) >= 1)
                        {
                            Print(string.Format("IPC Trim: Closing {0} of {1} contracts for {2} ({3:P0})",
                                rawQty, pos.RemainingContracts, pos.SignalName, percent));

                            if (pos.Direction == MarketPosition.Long)
                                SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, rawQty, 0, 0, "", "Trim_" + pos.SignalName);
                            else
                                SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, rawQty, 0, 0, "", "Trim_" + pos.SignalName);
                        }
                        else
                        {
                            Print(string.Format("IPC Trim SKIPPED: {0} contracts for {1} - cannot satisfy {2:P0} trim with 1+ remaining",
                                pos.RemainingContracts, pos.SignalName, percent));
                        }
                    }
                    else
                    {
                        // 1-contract positions cannot be trimmed (would flatten)
                        Print(string.Format("IPC Trim SKIPPED: {0} has only 1 contract - use FLATTEN to close", pos.SignalName));
                    }
                }
            }
                    else if (action == "CONFIG")
                    {
                        // V12 PRO: Parse the full config sync from side panel
                        // Format: CONFIG|Mode|COUNT:3;T1:1.0;T1TYPE:Points;T2:0.5;T2TYPE:ATR;...
                        if (parts.Length > 2)
                        {
                            string configMode = parts[1];
                            string configContent = parts[2];
                            string[] settingsItems = configContent.Split(';');
                            foreach (string setting in settingsItems)
                            {
                                if (string.IsNullOrEmpty(setting)) continue;
                                string[] kv = setting.Split(':');
                                if (kv.Length < 2) continue;
                                string key = kv[0].ToUpper();
                                string val = kv[1];

                                if (key == "T1") { if (double.TryParse(val, out double v)) Target1FixedPoints = v; }
                                else if (key == "CIT") { ChaseIfTouchPoints = val; }
                                else if (key == "T2") {
                                    if (double.TryParse(val, out double v)) {
                                        if (configMode == "RMA") RMAT1ATRMultiplier = v; else Target2Multiplier = v;
                                    }
                                }
                                else if (key == "T3") {
                                    if (double.TryParse(val, out double v)) {
                                        if (configMode == "RMA") RMAT2ATRMultiplier = v; else Target3Multiplier = v;
                                    }
                                }
                                else if (key == "STR") {
                                    if (double.TryParse(val, out double v)) {
                                        if (configMode == "RMA") RMAStopATRMultiplier = v; else StopMultiplier = v;
                                    }
                                }
                                else if (key == "MAX") {
                                    if (double.TryParse(val, out double v)) {
                                        MaxRiskAmount = v;
                                        RiskPerTrade = v;
                                    }
                                }
                                else if (key == "COUNT") {
                                    if (int.TryParse(val, out int v)) {
                                        minContracts = v;
                                    }
                                }
                            }
                            Print(string.Format("[V12] Sync All CONFIG ({0}) Applied: {1}", configMode, configContent));
                        }
                    }
                    else if (action == "SET_TRAIL")
                    {
                        // V12 PRO: Dynamic trail - move stop to current price +/- distance
                        if (parts.Length >= 2 && double.TryParse(parts[1], out double trailDistance))
                        {
                            if (activePositions.Count == 0)
                            {
                                Print("[V12] SET_TRAIL: No active positions");
                            }
                            else
                            {
                                double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
                                int trailCount = 0;

                                foreach (var kvp in activePositions.ToArray())
                                {
                                    if (!activePositions.ContainsKey(kvp.Key)) continue;
                                    PositionInfo pos = kvp.Value;
                                    string entryName = kvp.Key;

                                    if (!pos.EntryFilled) continue;

                                    // Calculate new stop: Longs = Price - Distance, Shorts = Price + Distance
                                    double newStopPrice = pos.Direction == MarketPosition.Long
                                        ? currentPrice - trailDistance
                                        : currentPrice + trailDistance;

                                    newStopPrice = Instrument.MasterInstrument.RoundToTickSize(newStopPrice);
                                    UpdateStopOrder(entryName, pos, newStopPrice, pos.CurrentTrailLevel);
                                    trailCount++;
                                    Print(string.Format("[V12] SET_TRAIL: {0} → Stop @ {1:F2} (Price: {2:F2}, Dist: {3})",
                                        entryName, newStopPrice, currentPrice, trailDistance));
                                }

                                Print(string.Format("[V12] SET_TRAIL COMPLETE: Updated {0} position(s) with {1} pt trail", trailCount, trailDistance));
                            }
                        }
                        else
                        {
                            Print("[V12] SET_TRAIL: Invalid distance parameter");
                        }
                    }
                    else if (action == "SET_CIT")
                    {
                        if (parts.Length >= 2)
                        {
                            ChaseIfTouchPoints = parts[1].Trim();
                            Print($"[V12] CIT updated: {ChaseIfTouchPoints}");
                        }
                    }
                    else if (action == "BE" || action == "BE_CUSTOM" || action == "BE_PLUS_2" || action == "BE_PLUS_1") // V12.23: +BE_CUSTOM with dynamic ticks
                    {
                        double beOffset;
                        if (action == "BE_CUSTOM" && parts.Length >= 2)
                        {
                            // V12.23: Dynamic ticks from panel input — syncs auto-trail BE too
                            int customTicks;
                            if (!int.TryParse(parts[1].Trim(), out customTicks) || customTicks < 0)
                                customTicks = BreakEvenOffsetTicks; // fallback to default
                            BreakEvenOffsetTicks = customTicks; // V12.23: Sync auto-trail + fleet symmetry
                            beOffset = customTicks * tickSize;
                        }
                        else if (action == "BE" || action == "BE_PLUS_2")
                            beOffset = BreakEvenOffsetTicks * tickSize;
                        else
                            beOffset = 1 * tickSize; // Legacy BE_PLUS_1
                        MoveStopsToBreakevenWithOffset(beOffset);
                    }
                    else if (action == "FLATTEN_ONLY")
                    {
                        // V12.21: Flatten Only (Close Positions) - preserve pending orders
                        if (EnableSIMA)
                        {
                            Print("[SIMA] IPC FLATTEN_ONLY → Closing all open positions (Pending orders preserved)");
                            ClosePositionsOnlyApexAccounts(); // V12.21: Use new non-cancelling helper
                        }
                        else
                        {
                            Print("[V12] FLATTEN_ONLY → Closing all open positions (Pending orders preserved)");
                            // CloseAllPositions(); // Native NT8 method closes positions and cancels orders usually? 
                            // NT8 Flatten() cancels orders. We must use Close() on each position instead.
                            
                            foreach (Position pos in Account.Positions)
                            {
                                if (pos.Instrument.FullName == Instrument.FullName && pos.MarketPosition != MarketPosition.Flat)
                                {
                                    if (pos.MarketPosition == MarketPosition.Long)
                                        SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, pos.Quantity, 0, 0, "", "FlattenOnly_ExitLong");
                                    else
                                        SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, pos.Quantity, 0, 0, "", "FlattenOnly_ExitShort");
                                }
                            }
                        }
                    }
                    else if (action == "FLATTEN")
                    {
                        // V12 SIMA: Use multi-account flatten when enabled
                        if (EnableSIMA)
                        {
                            Print("[SIMA] IPC FLATTEN → Broadcasting to all Apex accounts");
                            FlattenAllApexAccounts();
                        }
                        else
                        {
                            FlattenAll();
                        }
                    }
                    else if (action == "CANCEL_ALL")
                    {
                        // V12.13c: Only cancels pending entry orders (stops/targets on active positions are preserved)
                        if (EnableSIMA)
                        {
                            int cancelled = 0;

                            // ── V12.10: Cancel local account orders FIRST ──
                            foreach (Order order in Account.Orders)
                            {
                                if (order != null && order.Instrument.FullName == Instrument.FullName &&
                                    (order.OrderState == OrderState.Working ||
                                     order.OrderState == OrderState.Accepted ||
                                     order.OrderState == OrderState.Submitted))
                                {
                                    // V12.13c: Skip stops and targets on active positions — only cancel pending entries
                                    string oName = order.Name;
                                    if (oName.StartsWith("Stop_") || oName.StartsWith("S_") ||
                                        oName.StartsWith("T1_") || oName.StartsWith("T2_") ||
                                        oName.StartsWith("T3_") || oName.StartsWith("T4_"))
                                        continue;

                                    CancelOrder(order);
                                    cancelled++;
                                }
                            }

                            // ── Fleet accounts ──
                            foreach (Account acct in Account.All)
                            {
                                if (acct.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    if (acct == this.Account) continue; // already cancelled above
                                    foreach (Order order in acct.Orders)
                                    {
                                        if (order != null && order.Instrument.FullName == Instrument.FullName &&
                                            (order.OrderState == OrderState.Working ||
                                             order.OrderState == OrderState.Accepted ||
                                             order.OrderState == OrderState.Submitted))
                                        {
                                            // V12.13c: Skip stops and targets — only cancel pending entries
                                            string oName = order.Name;
                                            if (oName.StartsWith("Stop_") || oName.StartsWith("S_") ||
                                                oName.StartsWith("T1_") || oName.StartsWith("T2_") ||
                                                oName.StartsWith("T3_") || oName.StartsWith("T4_"))
                                                continue;

                                            acct.Cancel(new[] { order });
                                            cancelled++;
                                        }
                                    }
                                }
                            }
                            Print($"[SIMA] CANCEL_ALL → Cancelled {cancelled} pending entry orders (local + fleet)");
                        }
                        else
                        {
                            int cancelled = 0;
                            foreach (Order order in Account.Orders)
                            {
                                if (order != null && order.Instrument.FullName == Instrument.FullName &&
                                    (order.OrderState == OrderState.Working ||
                                     order.OrderState == OrderState.Accepted ||
                                     order.OrderState == OrderState.Submitted))
                                {
                                    // V12.13c: Skip stops and targets — only cancel pending entries
                                    string oName = order.Name;
                                    if (oName.StartsWith("Stop_") || oName.StartsWith("S_") ||
                                        oName.StartsWith("T1_") || oName.StartsWith("T2_") ||
                                        oName.StartsWith("T3_") || oName.StartsWith("T4_"))
                                        continue;

                                    CancelOrder(order);
                                    cancelled++;
                                }
                            }
                            Print($"[V12] CANCEL_ALL → Cancelled {cancelled} pending entry orders");
                        }

                        // V12.13b: Clean up position state after cancel-all
                        // Positions with unfilled entries should be fully cleaned up
                        foreach (var kvp in activePositions.ToArray())
                        {
                            string entryName = kvp.Key;
                            PositionInfo pos = kvp.Value;
                            if (!pos.EntryFilled)
                            {
                                CleanupPosition(entryName);
                                Print(string.Format("V12.13b: CANCEL_ALL cleaned unfilled position: {0}", entryName));
                            }
                        }
                    }
                    else if (action == "LONG" || action == "SHORT")
                    {
                        // V12.2: Handle Sync Mode
                        if (isTosSyncMode)
                        {
                            bool armed = (action == "LONG") ? isLongArmed : isShortArmed;
                            if (!armed)
                            {
                                Print($"[SYNC] ToS Signal IGNORED: {action} received but {action} is not ARMED locally.");
                                continue;
                            }
                            else
                            {
                                Print($"[SYNC] ToS Handshake Received -> Executing {action} Fleet Entry");
                                // Reset armed flag after firing
                                if (action == "LONG") isLongArmed = false; else isShortArmed = false;
                            }
                        }

                        // V12 SIMA: Broadcast to all Apex accounts when enabled
                        if (EnableSIMA)
                        {
                            OrderAction orderAction = action == "LONG" ? OrderAction.Buy : OrderAction.SellShort;
                            int qty = Math.Max(1, minContracts);

                            if (EnablePathB)
                            {
                                Print($"[SIMA] PATH B {action} → Broadcasting {qty} contracts with FIXED BRACKETS to all Apex accounts");
                                ExecuteMultiAccountBracket(orderAction, qty, "PATHB_" + action, PathBStopPoints, PathBTargetPoints);
                            }
                            else
                            {
                                Print($"[SIMA] IPC {action} → Broadcasting {qty} contracts to all Apex accounts");
                                ExecuteMultiAccountMarket(orderAction, qty, "SIMA_" + action);
                            }
                        }
                        else
                        {
                            // Original single-account logic
                            MarketPosition direction = action == "LONG" ? MarketPosition.Long : MarketPosition.Short;
                            double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
                            ExecuteRMAEntryV2(currentPrice, direction);
                        }
                    }
                    // V10.3: OR Breakout Entry Commands
                    else if (action == "OR_LONG")
                    {
                        // V12.2: Handle Sync Mode
                        if (isTosSyncMode)
                        {
                            if (isLongArmed)
                            {
                                Print("[SYNC] ToS Handshake Received -> Executing OR_LONG");
                                ExecuteLong();
                                isLongArmed = false;
                            }
                            else
                            {
                                Print("[SYNC] ToS Signal IGNORED: OR_LONG received but Long is not ARMED locally.");
                            }
                        }
                        else
                        {
                            ExecuteLong();
                            Print("V10.3: OR_LONG executed via IPC");
                        }
                    }
                    else if (action == "OR_SHORT")
                    {
                        // V12.2: Handle Sync Mode
                        if (isTosSyncMode)
                        {
                            if (isShortArmed)
                            {
                                Print("[SYNC] ToS Handshake Received -> Executing OR_SHORT");
                                ExecuteShort();
                                isShortArmed = false;
                            }
                            else
                            {
                                Print("[SYNC] ToS Signal IGNORED: OR_SHORT received but Short is not ARMED locally.");
                            }
                        }
                        else
                        {
                            ExecuteShort();
                            Print("V10.3: OR_SHORT executed via IPC");
                        }
                    }
                    // V10.3: Target-Specific Close Commands
                    else if (action.StartsWith("CLOSE_T"))
                    {
                        int targetNum = 0;
                        if (action.Length > 7 && int.TryParse(action.Substring(7, 1), out targetNum))
                        {
                            FlattenSpecificTarget(targetNum);
                        }
                    }
                    // V14: MOVE_TARGET command - Surgical target price adjustment
                    else if (action.StartsWith("MOVE_TARGET"))
                    {
                        // Format: MOVE_TARGET|T1|1pt  or  MOVE_TARGET|T2|2pt
                        if (parts.Length >= 3)
                        {
                            string targetId = parts[1].Trim().ToUpper(); // "T1", "T2", etc.
                            string distance = parts[2].Trim().ToLower(); // "1pt" or "2pt"
                            
                            // Parse distance
                            double profitPoints = 0;
                            if (distance == "1pt") profitPoints = 1.0;
                            else if (distance == "2pt") profitPoints = 2.0;
                            else
                            {
                                Print($"[V14] MOVE_TARGET: Invalid distance '{distance}' - expected '1pt' or '2pt'");
                                continue;
                            }
                            
                            // Extract target number (T1 -> 1, T2 -> 2, etc.)
                            int targetNum = 0;
                            if (targetId.Length >= 2 && targetId.StartsWith("T"))
                            {
                                if (!int.TryParse(targetId.Substring(1), out targetNum) || targetNum < 1 || targetNum > 5)
                                {
                                    Print($"[V14] MOVE_TARGET: Invalid target '{targetId}' - expected T1-T5");
                                    continue;
                                }
                            }
                            else
                            {
                                Print($"[V14] MOVE_TARGET: Invalid target format '{targetId}'");
                                continue;
                            }
                            
                            Print($"[V14] MOVE_TARGET: Command received for {targetId} to +{profitPoints}pt profit");
                            
                            // Call the move handler (implemented in Orders.cs)
                            MoveSpecificTarget(targetNum, profitPoints);
                        }
                        else
                        {
                            Print("[V14] MOVE_TARGET: Invalid format - expected MOVE_TARGET|TX|1pt or MOVE_TARGET|TX|2pt");
                        }
                    }
                    else if (action.StartsWith("GET_FLEET"))
                    {
                        // Broadcast account names to the Remote App for UI mapping
                        StringBuilder sb = new StringBuilder("CONFIG|FLEET");
                        foreach (var acct in Account.All)
                        {
                            if (acct.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                sb.Append($"|{acct.Name}");
                            }
                        }
                        SendResponseToRemote(sb.ToString());
                        Print("[SIMA] GET_FLEET → Responded with account list");
                    }
                    else if (action.StartsWith("SET_MAX_RISK"))
                    {
                        if (parts.Length > 2 && double.TryParse(parts[2], out double val))
                        {
                            MaxRiskAmount = val;
                            RiskPerTrade = val; // Sync legacy property
                            Print($"[V12.2] SET_MAX_RISK: {val}");
                        }
                    }
                    else if (action.StartsWith("TOGGLE_ACCOUNT"))
                    {
                        if (parts.Length > 2)
                        {
                            string acctName = parts[1];
                            bool active = parts[2] == "1";
                            // V12.1101E [A-2]: Lock IPC writes to activeFleetAccounts — this dict is also
                            // read by the strategy thread (ExecuteMultiAccountMarket) without a lock.
                            lock (stateLock)
                            {
                                activeFleetAccounts[acctName] = active;
                            }
                            Print($"[V12.2] TOGGLE_ACCOUNT: {acctName} | Active={active}");
                        }
                    }
                    // V12.6: SET_SIMA|ON or SET_SIMA|OFF - Remote SIMA toggle from external panel
                    else if (action == "SET_SIMA")
                    {
                        if (parts.Length > 1)
                        {
                            bool enable = parts[1].Trim().ToUpper() == "ON";
                            EnableSIMA = enable;
                            Print($"V12.6: SET_SIMA = {enable}");

                            // Re-enumerate accounts when enabling to ensure fleet is populated
                            if (enable && simaAccountCount == 0)
                            {
                                EnumerateApexAccounts();
                            }
                        }
                    }
                    // V12.2: Diagnostic command to check fleet state
                    else if (action == "DIAG_FLEET")
                    {
                        Print("[DIAG] ══════════════════════════════════════════════════");
                        Print($"[DIAG] EnableSIMA = {EnableSIMA}");
                        Print($"[DIAG] AccountPrefix = \"{AccountPrefix}\"");
                        int total = 0;
                        int active = 0;
                        foreach (Account acct in Account.All)
                        {
                            if (acct.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                total++;
                                bool isActive = false;
                                activeFleetAccounts.TryGetValue(acct.Name, out isActive);
                                if (isActive) active++;
                                Print($"[DIAG]   {acct.Name} -> {(isActive ? "✓ ACTIVE" : "✗ INACTIVE")}");
                            }
                        }
                        Print($"[DIAG] TOTAL: {total} accounts | {active} ACTIVE");
                        Print("[DIAG] ══════════════════════════════════════════════════");
                    }
                    else if (action.StartsWith("SET_ANCHOR"))
                    {
                        // V11: SET_ANCHOR|EMA30|Global
                        if (parts.Length > 2)
                        {
                            string anchorStr = parts[1];
                            SetRmaAnchorFromIpc(anchorStr);
                        }
                    }
                    // V12.4: SET_RMA_MODE|ON or SET_RMA_MODE|OFF - Toggle chart-click RMA mode from Panel
                    else if (action == "SET_RMA_MODE")
                    {
                        if (parts.Length > 1)
                        {
                            bool enable = parts[1].Trim().ToUpper() == "ON";
                            isRMAModeActive = enable;
                            isRMAButtonClicked = enable;
                            Print(string.Format("V12.4: SET_RMA_MODE = {0} (Chart-Click RMA {1})", enable, enable ? "ENABLED" : "DISABLED"));
                        }
                    }
                    // V12.2: SYNC_MODE|{MODE} - Relay mode sync from chart panel to external app
                    else if (action == "SYNC_MODE")
                    {
                        if (parts.Length > 1)
                        {
                            string syncMode = parts[1].Trim().ToUpper();
                            // V12.13-D: Broadcast SYNC_MODE to all connected panel clients
                            SendResponseToRemote($"SYNC_MODE|{syncMode}");
                            Print(string.Format("V12.2: SYNC_MODE Relay -> {0}", syncMode));
                        }
                    }
                    // V12.5: SET_TARGETS|count - Panel is sole source of truth
                    else if (action == "SET_TARGETS")
                    {
                        if (parts.Length > 1 && int.TryParse(parts[1], out int targetCount))
                        {
                            minContracts = targetCount;
                            Print(string.Format("V12.25: SET_TARGETS = {0} contracts (no CONFIG echo)", targetCount));
                            // V12.25: CONFIG broadcast REMOVED — Panel is sole source of truth.
                            // Sending CONFIG back here caused the Ping-Pong overwrite bug.
                        }
                    }
                    // V12.5: SET_MODE|mode - Panel is sole source of truth
                    else if (action == "SET_MODE")
                    {
                        if (parts.Length > 1)
                        {
                            string newMode = parts[1].Trim().ToUpper();

                            // V12.20: Atomic mode transition — prevents partial state reads during switch
                            lock (stateLock)
                            {
                                isRMAModeActive = false;
                                isRMAButtonClicked = false;
                                isRetestModeActive = false;
                                isTRENDModeActive = false;
                                isMOMOModeActive = false;
                                isFFMAModeArmed = false;

                                if (newMode == "RMA")
                                {
                                    isRMAModeActive = true;
                                    isRMAButtonClicked = true;
                                }
                                else if (newMode == "RETEST")
                                {
                                    isRetestModeActive = true;
                                }
                                else if (newMode == "TREND")
                                {
                                    isTRENDModeActive = true;
                                }
                                else if (newMode == "MOMO")
                                {
                                    isMOMOModeActive = true;
                                }
                                else if (newMode == "FFMA")
                                {
                                    isFFMAModeArmed = true;
                                }
                                // ORB/OR = all modes off (already deactivated above)
                            }

                            Print(string.Format("V12.25: SET_MODE = {0} | RMA={1} RETEST={2} TREND={3} MOMO={4} FFMA={5} (no CONFIG echo)",
                                newMode, isRMAModeActive, isRetestModeActive, isTRENDModeActive, isMOMOModeActive, isFFMAModeArmed));

                            // V12.25: CONFIG broadcast REMOVED — Panel is sole source of truth.
                            // Sending CONFIG back here caused the Ping-Pong overwrite bug.
                        }
                    }
                    // V12.25: SET_LEADER_ACCOUNT|accountName — Panel tells strategy which account is the leader
                    else if (action == "SET_LEADER_ACCOUNT")
                    {
                        if (parts.Length > 1)
                        {
                            string newLeader = parts[1].Trim();
                            Print($"V12.25 IPC: Leader Account synced to [{newLeader}]");
                        }
                    }

                    else if (action == "SET_MANUAL_PRICE")
                    {
                        // Format: SET_MANUAL_PRICE|<price>|<symbol> - price is in parts[1] (after split by |)
                        // Note: The command comes as "SET_MANUAL_PRICE" with price in parts[1] if sent as SET_MANUAL_PRICE|1234.50|MGC
                        if (parts.Length > 1 && double.TryParse(parts[1], out double manualPrice))
                        {
                            cachedMnlPrice = manualPrice;
                            currentRmaAnchor = RmaAnchorType.Manual;
                            isMnlArmed = true;

                            Print(string.Format("IPC SET_MANUAL_PRICE: {0:F2} | Anchor set to MANUAL", manualPrice));
                        }
                        else
                        {
                            Print(string.Format("IPC SET_MANUAL_PRICE: Invalid price format in command: {0}", command));
                        }
                    }
                    // V12.27: Manual entry commands from Contextual UI Submit button
                    else if (action == "TREND_MANUAL_LIMIT")
                    {
                        // Format: TREND_MANUAL_LIMIT|LONG|1234.50
                        if (parts.Length > 2)
                        {
                            string dir = parts[1].Trim().ToUpper();
                            MarketPosition mp = dir == "LONG" ? MarketPosition.Long : MarketPosition.Short;
                            if (double.TryParse(parts[2], out double price) && price > 0)
                            {
                                Print(string.Format("V12.27 IPC: TREND_MANUAL_LIMIT {0} @ {1:F2}", dir, price));
                                ExecuteTRENDManualEntry(price, mp);
                            }
                            else
                            {
                                Print(string.Format("V12.27 IPC: TREND_MANUAL_LIMIT invalid price: {0}", command));
                            }
                        }
                    }
                    else if (action == "RETEST_MANUAL_LIMIT")
                    {
                        // Format: RETEST_MANUAL_LIMIT|LONG|1234.50
                        if (parts.Length > 2)
                        {
                            string dir = parts[1].Trim().ToUpper();
                            MarketPosition mp = dir == "LONG" ? MarketPosition.Long : MarketPosition.Short;
                            if (double.TryParse(parts[2], out double price) && price > 0)
                            {
                                Print(string.Format("V12.27 IPC: RETEST_MANUAL_LIMIT {0} @ {1:F2}", dir, price));
                                ExecuteRetestManualEntry(price, mp);
                            }
                            else
                            {
                                Print(string.Format("V12.27 IPC: RETEST_MANUAL_LIMIT invalid price: {0}", command));
                            }
                        }
                    }
                    else if (action == "FFMA_MANUAL_LIMIT")
                    {
                        // Format: FFMA_MANUAL_LIMIT|LONG|1234.50
                        if (parts.Length > 2)
                        {
                            string dir = parts[1].Trim().ToUpper();
                            MarketPosition mp = dir == "LONG" ? MarketPosition.Long : MarketPosition.Short;
                            if (double.TryParse(parts[2], out double price) && price > 0)
                            {
                                Print(string.Format("V12.27 IPC: FFMA_MANUAL_LIMIT {0} @ {1:F2}", dir, price));
                                ExecuteFFMALimitEntry(price, mp);
                            }
                            else
                            {
                                Print(string.Format("V12.27 IPC: FFMA_MANUAL_LIMIT invalid price: {0}", command));
                            }
                        }
                    }
                    else if (action == "FFMA_MANUAL_MARKET")
                    {
                        // V12.27: M.FFMA button — instant market, direction toward 9 EMA
                        Print("V12.27 IPC: FFMA_MANUAL_MARKET — auto-direction toward EMA9");
                        ExecuteFFMAManualMarketEntry();
                    }
                    else if (action.StartsWith("MODE_") || action.StartsWith("EXEC_") || action == "FFMA_DISARM")
                    {
                        ToggleStrategyMode(action);
                    }
                    // V12: GET_LAYOUT handler (primary response is in ListenForRemote, this is fallback logging)
                    else if (action == "GET_LAYOUT")
                    {
                        string mode = isRMAModeActive ? "RMA" : "OR";
                        Print(string.Format("V12 GET_LAYOUT: Mode={0} Count={1} T1={2}pt T2={3}xATR T3={4}xATR",
                            mode, minContracts, Target1FixedPoints, Target2Multiplier, Target3Multiplier));
                    }
                }
                catch (Exception ex)
                {
                    Print("Error ProcessIpcCommands: " + ex.Message);
                }
            }
        }

        private void SendResponseToRemote(string response)
        {
            if (connectedClients == null) return;

            // Diagnostic: Log what we are sending and to how many clients
            if (response.Contains("SYNC_TARGET_STATE"))
                 Print($"V14 IPC: Broadcasting {response} to {connectedClients.Count} clients");

            byte[] responseBytes = Encoding.UTF8.GetBytes(response + "\n");
            List<TcpClient> disconnectedClients = new List<TcpClient>();

            foreach (var client in connectedClients)
            {
                try
                {
                    if (client.Connected && client.GetStream().CanWrite)
                    {
                        client.GetStream().Write(responseBytes, 0, responseBytes.Length);
                        client.GetStream().Flush();
                    }
                    else
                    {
                        Print("V14 IPC: Client disconnected or stream not writable.");
                    }
                }
                catch (Exception ex)
                {
                    Print($"V14 IPC: Send Error - {ex.Message}");
                    // Client likely disconnected, will be handled by reading loop or next cleanup
                }
            }
        }

        // V12.13-D: SendToExternalApp REMOVED — it connected to port 5001 (the strategy's own listener),
        // causing infinite flood loops. All callers now use SendResponseToRemote() or direct client stream writes.
        // V12.44: MoveStopsToBreakevenPlusOne() removed — dead code, replaced by MoveStopsToBreakevenWithOffset()

        /// <summary>
        /// V10.3: Close a specific target (T1, T2, T3, or T4) at market for all active positions
        /// Cancels working limit order and submits market order to close
        /// </summary>
        private void FlattenSpecificTarget(int targetNumber)
        {
            try
            {
                foreach (var kvp in activePositions.ToArray())
                {
                    if (!activePositions.ContainsKey(kvp.Key)) continue;
                    PositionInfo pos = kvp.Value;
                    string entryName = kvp.Key;

                    if (!pos.EntryFilled || pos.RemainingContracts <= 0) continue;

                    int qtyToClose = 0;
                    ConcurrentDictionary<string, Order> targetDict = null;
                    string targetName = "";

                    switch (targetNumber)
                    {
                        case 1: qtyToClose = pos.T1Contracts; targetDict = target1Orders; targetName = "T1"; break;
                        case 2: qtyToClose = pos.T2Contracts; targetDict = target2Orders; targetName = "T2"; break;
                        case 3: qtyToClose = pos.T3Contracts; targetDict = target3Orders; targetName = "T3"; break;
                        case 4: qtyToClose = pos.T4Contracts; targetDict = target4Orders; targetName = "T4"; break;
                        default:
                            Print(string.Format("V10.3: Invalid target number {0}", targetNumber));
                            return;
                    }

                    if (qtyToClose <= 0)
                    {
                        Print(string.Format("V10.3: {0} has no contracts to close for {1}", targetName, entryName));
                        continue;
                    }

                    // Cancel existing limit order if working
                    if (targetDict != null && targetDict.TryGetValue(entryName, out Order targetOrder))
                    {
                        if (targetOrder != null && (targetOrder.OrderState == OrderState.Working ||
                            targetOrder.OrderState == OrderState.Accepted ||
                            targetOrder.OrderState == OrderState.Submitted))
                        {
                            CancelOrder(targetOrder);
                            Print(string.Format("V10.3: Cancelled {0} limit order for {1}", targetName, entryName));
                        }
                    }

                    // Submit market order to close the target contracts
                    if (pos.Direction == MarketPosition.Long)
                        SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, qtyToClose, 0, 0, "",
                            string.Format("Close{0}_{1}", targetName, entryName));
                    else
                        SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, qtyToClose, 0, 0, "",
                            string.Format("Close{0}_{1}", targetName, entryName));

                    Print(string.Format("V10.3: Closing {0} ({1} contracts) at market for {2}", targetName, qtyToClose, entryName));
                }
            }
            catch (Exception ex)
            {
                Print("ERROR FlattenSpecificTarget: " + ex.Message);
            }
        }

        private void ToggleStrategyMode(string action)
        {
             // V12.20: Atomic flag mutations
             lock (stateLock)
             {
                 if (action == "MODE_RMA") isRMAModeActive = !isRMAModeActive;
                 else if (action == "MODE_MOMO") isMOMOModeActive = !isMOMOModeActive;
                 else if (action == "MODE_FFMA")
                 {
                     isFFMAModeArmed = true;
                     Print("V12.24: FFMA AUTO armed — reversal scanner active");
                 }
                 else if (action == "MODE_M")
                 {
                     Print("V12.24: MODE_M received — immediate FFMA entry pending");
                 }
                 else if (action == "FFMA_DISARM")
                 {
                     isFFMAModeArmed = false;
                     Print("V12.24: FFMA disarmed via panel ResetExecutionMode");
                 }
                 else if (action == "MODE_TREND_RMA")
                 {
                     isTrendRmaMode = true;
                     Print("IPC: TREND RMA Mode Enabled");
                 }
                 else if (action == "MODE_TREND_STD")
                 {
                     isTrendRmaMode = false;
                     Print("IPC: TREND Standard Mode Enabled");
                 }
                 else if (action == "MODE_RETEST_RMA")
                 {
                     isRetestRmaMode = true;
                     Print("IPC: RETEST RMA Mode Enabled");
                 }
                 else if (action == "MODE_RETEST_STD")
                 {
                     isRetestRmaMode = false;
                     Print("IPC: RETEST Standard Mode Enabled");
                 }
             }

             // Execution calls stay outside lock (they do their own order management)
             if (action == "EXEC_TREND" || action == "EXEC_TREND_RMA") ExecuteTRENDEntry();
             else if (action == "EXEC_RETEST" || action == "EXEC_RETEST_PLUS" || action == "EXEC_RETEST_MINUS") ExecuteRetestEntry();
             else if (action == "EXEC_MOMO") ExecuteMOMOEntry(lastKnownPrice);
             else if (action == "MODE_M")
             {
                 // V12.24: Immediate market entry using FFMA trade DNA
                 double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
                 double ema9Value = _ema9Val;
                 MarketPosition direction = currentPrice > ema9Value ? MarketPosition.Short : MarketPosition.Long;
                 Print(string.Format("V12.24: MODE_M firing — Price={0:F2} vs EMA9={1:F2} → {2}", currentPrice, ema9Value, direction));
                 ExecuteFFMAEntry(direction);
             }

             Print(string.Format("IPC Mode Toggle: {0} | RMA={1} MOMO={2} TrendRMA={3} RetestRMA={4} FFMA={5}",
                action, isRMAModeActive, isMOMOModeActive, isTrendRmaMode, isRetestRmaMode, isFFMAModeArmed));
        }


        #endregion

        #region Apex Compliance Hub Logic (V12.1)

        private DateTime GetComplianceNow()
        {
            return ConvertToSelectedTimeZone(DateTime.Now);
        }

        private int GetTradingDayKey(DateTime timeInZone)
        {
            return timeInZone.Year * 10000 + timeInZone.Month * 100 + timeInZone.Day;
        }

        private void EnsureAccountComplianceTracking(string accountName, DateTime nowInZone)
        {
            if (string.IsNullOrEmpty(accountName)) return;
            accountDailyProfit.TryAdd(accountName, 0);
            accountTotalProfit.TryAdd(accountName, 0);
            accountTradeCount.TryAdd(accountName, 0);
            accountDailyTradeCount.TryAdd(accountName, 0);
            accountEquityPeak.TryAdd(accountName, 0);
            accountMaxDrawdown.TryAdd(accountName, 0);
            accountTradingDays.TryAdd(accountName, new ConcurrentDictionary<int, byte>());
            accountLastSummaryDate.TryAdd(accountName, nowInZone.Date);
        }

        private void TrackTradeEntry(Account acct, Execution execution)
        {
            if (acct == null || execution == null || execution.Order == null) return;
            if (execution.Order.OrderState != OrderState.Filled) return;

            OrderAction action = execution.Order.OrderAction;
            if (action != OrderAction.Buy && action != OrderAction.SellShort) return;

            if (EnableSIMA && acct.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) < 0) return;

            DateTime nowInZone = GetComplianceNow();
            EnsureAccountComplianceTracking(acct.Name, nowInZone);

            accountTradeCount.AddOrUpdate(acct.Name, 1, (k, v) => v + 1);
            accountDailyTradeCount.AddOrUpdate(acct.Name, 1, (k, v) => v + 1);

            int dayKey = GetTradingDayKey(nowInZone);
            var days = accountTradingDays.GetOrAdd(acct.Name, _ => new ConcurrentDictionary<int, byte>());
            days.TryAdd(dayKey, 1);
        }

        private void UpdateEquityDrawdown(string accountName, double balance)
        {
            double peak = accountEquityPeak.AddOrUpdate(accountName, balance, (k, v) => Math.Max(v, balance));
            double drawdown = Math.Max(0, peak - balance);
            accountMaxDrawdown.AddOrUpdate(accountName, drawdown, (k, v) => Math.Max(v, drawdown));
        }

        private void UpdateAccountMetricsFromAccount(Account acct)
        {
            if (acct == null) return;
            if (EnableSIMA && acct.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) < 0) return;

            DateTime nowInZone = GetComplianceNow();
            EnsureAccountComplianceTracking(acct.Name, nowInZone);

            double dailyPL = acct.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
            accountDailyProfit[acct.Name] = dailyPL;

            double balance = acct.Get(AccountItem.CashValue, Currency.UsDollar);
            UpdateEquityDrawdown(acct.Name, balance);
        }

        private int GetUniqueTradingDays(string accountName)
        {
            if (accountTradingDays.TryGetValue(accountName, out var days))
                return days.Count;
            return 0;
        }

        private void EnsureDailySummaryCsv()
        {
            if (string.IsNullOrEmpty(dailySummaryCsvPath)) return;

            if (!System.IO.File.Exists(dailySummaryCsvPath))
            {
                lock (dailySummaryLock)
                {
                    if (!System.IO.File.Exists(dailySummaryCsvPath))
                    {
                        string header = "Date,Account,DailyPL,DailyTrades,TotalProfit,TotalTrades,MaxDrawdown,UniqueDays";
                        System.IO.File.WriteAllText(dailySummaryCsvPath, header + Environment.NewLine);
                    }
                }
            }
        }

        private void AppendDailySummary(DateTime summaryDate, string accountName, double dailyPL, int dailyTrades,
            double totalProfit, int totalTrades, double maxDrawdown, int uniqueDays)
        {
            if (string.IsNullOrEmpty(dailySummaryCsvPath)) return;

            string safeName = (accountName ?? string.Empty).Replace("\"", "\"\"");
            string line = string.Format(CultureInfo.InvariantCulture,
                "{0},\"{1}\",{2:F2},{3},{4:F2},{5},{6:F2},{7}",
                summaryDate.ToString("yyyy-MM-dd"), safeName, dailyPL, dailyTrades, totalProfit, totalTrades, maxDrawdown, uniqueDays);

            // V12.40 FREEZE FIX: Ensure CSV header exists (fast, no I/O if already created)
            lock (dailySummaryLock)
            {
                EnsureDailySummaryCsv();
            }

            // V12.40 FREEZE FIX: Fire-and-forget async write — never blocks UI thread
            string pathCopy = dailySummaryCsvPath;
            string lineCopy = line + Environment.NewLine;
            Task.Run(() =>
            {
                try { System.IO.File.AppendAllText(pathCopy, lineCopy); }
                catch { /* swallow — daily summary is best-effort */ }
            });
        }

        private void FinalizeDailySummaryForAccount(string accountName, DateTime summaryDate)
        {
            if (string.IsNullOrEmpty(accountName)) return;

            double dailyPL = accountDailyProfit.TryGetValue(accountName, out var dp) ? dp : 0;
            int dailyTrades = accountDailyTradeCount.TryGetValue(accountName, out var dt) ? dt : 0;
            int totalTrades = accountTradeCount.TryGetValue(accountName, out var tt) ? tt : 0;
            double maxDrawdown = accountMaxDrawdown.TryGetValue(accountName, out var dd) ? dd : 0;
            int uniqueDays = GetUniqueTradingDays(accountName);

            double totalProfit = accountTotalProfit.AddOrUpdate(accountName, dailyPL, (k, v) => v + dailyPL);
            AppendDailySummary(summaryDate, accountName, dailyPL, dailyTrades, totalProfit, totalTrades, maxDrawdown, uniqueDays);
        }

        private void MaybeFinalizeDailySummaries(DateTime nowInZone, List<Account> accounts)
        {
            if (string.IsNullOrEmpty(dailySummaryCsvPath)) return;

            if ((nowInZone - lastDailySummaryCheck).TotalSeconds < 30) return;
            lastDailySummaryCheck = nowInZone;

            foreach (Account acct in accounts)
            {
                if (acct == null) continue;
                EnsureAccountComplianceTracking(acct.Name, nowInZone);

                DateTime lastDate = accountLastSummaryDate.GetOrAdd(acct.Name, nowInZone.Date);
                if (nowInZone.Date > lastDate.Date)
                {
                    FinalizeDailySummaryForAccount(acct.Name, lastDate);
                    accountDailyProfit[acct.Name] = 0;
                    accountDailyTradeCount[acct.Name] = 0;
                    accountLastSummaryDate[acct.Name] = nowInZone.Date;
                }
            }
        }

        private List<Account> GetComplianceAccounts()
        {
            List<Account> accounts = new List<Account>();

            if (EnableSIMA)
            {
                foreach (Account acct in Account.All)
                {
                    if (acct.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
                        accounts.Add(acct);
                }
            }
            else
            {
                if (Account != null)
                    accounts.Add(Account);
            }

            return accounts;
        }

        private ComplianceSnapshot BuildComplianceSnapshot()
        {
            ComplianceSnapshot snapshot = new ComplianceSnapshot
            {
                Enabled = EnableComplianceHub,
                HasAccounts = false,
                AccountName = "--",
                TradeCount = 0,
                UniqueDays = 0,
                MaxDrawdown = 0,
                ConsistencyText = "CONSISTENCY: --",
                ConsistencySeverity = 0,
                PayoutText = "PAYOUT: --",
                PayoutSeverity = 0,
                DrawdownText = "DD BUFFER: --",
                DrawdownSeverity = 0
            };

            if (!EnableComplianceHub)
                return snapshot;

            List<Account> accounts = GetComplianceAccounts();
            if (accounts.Count == 0)
                return snapshot;

            DateTime nowInZone = GetComplianceNow();
            MaybeFinalizeDailySummaries(nowInZone, accounts);

            double highestConsistencyRatio = 0;
            string consistencyAccount = "--";
            bool consistencyViolation = false;

            bool payoutEligibleAll = true;
            double worstPayoutScore = -1;
            int payoutDaysRemaining = 0;
            double payoutProfitRemaining = 0;
            string payoutAccount = "--";

            double minDrawdownBuffer = double.PositiveInfinity;
            string drawdownAccount = "--";

            string focusAccount = "--";
            double focusDrawdownBuffer = double.MaxValue;
            double focusTotalProfit = double.MinValue;

            foreach (Account acct in accounts)
            {
                if (acct == null) continue;
                EnsureAccountComplianceTracking(acct.Name, nowInZone);

                UpdateAccountMetricsFromAccount(acct);

                double dailyPL = accountDailyProfit.TryGetValue(acct.Name, out var dp) ? dp : 0;
                double totalProfit = accountTotalProfit.GetOrAdd(acct.Name, 0) + dailyPL;

                double ratio = (totalProfit > 0 && dailyPL > 0) ? (dailyPL / totalProfit) * 100.0 : 0.0;
                if (ratio > highestConsistencyRatio)
                {
                    highestConsistencyRatio = ratio;
                    consistencyAccount = acct.Name;
                }
                if (ratio >= ConsistencyThreshold && dailyPL > 0)
                    consistencyViolation = true;

                int uniqueDays = GetUniqueTradingDays(acct.Name);
                bool payoutEligible = uniqueDays >= PayoutMinTradingDays && totalProfit >= PayoutMinProfit;
                if (!payoutEligible)
                {
                    payoutEligibleAll = false;
                    int daysRemaining = Math.Max(0, PayoutMinTradingDays - uniqueDays);
                    double profitRemaining = Math.Max(0, PayoutMinProfit - totalProfit);
                    double score = (daysRemaining * 100000.0) + profitRemaining;
                    if (score > worstPayoutScore)
                    {
                        worstPayoutScore = score;
                        payoutDaysRemaining = daysRemaining;
                        payoutProfitRemaining = profitRemaining;
                        payoutAccount = acct.Name;
                    }
                }

                double balance = acct.Get(AccountItem.CashValue, Currency.UsDollar);
                double peak = accountEquityPeak.TryGetValue(acct.Name, out var pk) ? pk : balance;
                double buffer = TrailingDrawdownLimit > 0 ? balance - (peak - TrailingDrawdownLimit) : double.PositiveInfinity;

                if (buffer < minDrawdownBuffer)
                {
                    minDrawdownBuffer = buffer;
                    drawdownAccount = acct.Name;
                }

                if (TrailingDrawdownLimit > 0)
                {
                    if (buffer < focusDrawdownBuffer)
                    {
                        focusDrawdownBuffer = buffer;
                        focusAccount = acct.Name;
                    }
                }

                if (TrailingDrawdownLimit <= 0 && totalProfit > focusTotalProfit)
                {
                    focusTotalProfit = totalProfit;
                    focusAccount = acct.Name;
                }
            }

            if (focusAccount == "--" && accounts.Count > 0)
                focusAccount = accounts[0].Name;

            snapshot.HasAccounts = true;
            snapshot.AccountName = focusAccount;
            snapshot.TradeCount = accountTradeCount.TryGetValue(focusAccount, out var tc) ? tc : 0;
            snapshot.UniqueDays = GetUniqueTradingDays(focusAccount);
            snapshot.MaxDrawdown = accountMaxDrawdown.TryGetValue(focusAccount, out var md) ? md : 0;

            if (consistencyViolation)
            {
                snapshot.ConsistencySeverity = 2;
                snapshot.ConsistencyText = string.Format("CONSISTENCY: VIOLATION {0:F0}% ({1})", highestConsistencyRatio, consistencyAccount);
            }
            else
            {
                snapshot.ConsistencySeverity = 0;
                snapshot.ConsistencyText = string.Format("CONSISTENCY: OK {0:F0}%", highestConsistencyRatio);
            }

            if (payoutEligibleAll)
            {
                snapshot.PayoutSeverity = 0;
                snapshot.PayoutText = "PAYOUT: ELIGIBLE";
            }
            else
            {
                snapshot.PayoutSeverity = 1;
                snapshot.PayoutText = string.Format("PAYOUT: NEED {0}D / ${1:F0} ({2})", payoutDaysRemaining, payoutProfitRemaining, payoutAccount);
            }

            if (TrailingDrawdownLimit <= 0 || double.IsInfinity(minDrawdownBuffer))
            {
                snapshot.DrawdownSeverity = 0;
                snapshot.DrawdownText = "DD BUFFER: N/A";
            }
            else
            {
                if (minDrawdownBuffer <= 0)
                    snapshot.DrawdownSeverity = 2;
                else if (minDrawdownBuffer <= TrailingDrawdownWarningBuffer)
                    snapshot.DrawdownSeverity = 1;
                else
                    snapshot.DrawdownSeverity = 0;

                string bufferText = minDrawdownBuffer.ToString("F0");
                string accountTag = snapshot.DrawdownSeverity > 0 ? string.Format(" ({0})", drawdownAccount) : "";
                snapshot.DrawdownText = string.Format("DD BUFFER: ${0}{1}", bufferText, accountTag);
            }

            return snapshot;
        }

        /// <summary>
        /// Triggered when ANY of the 20 Apex accounts has an execution (entry or exit)
        /// </summary>
        private void OnAccountExecutionUpdate(object sender, ExecutionEventArgs e)
        {
            if (e == null) return;

            // V12.8: Block entire handler during mass flatten to prevent UI thread freeze
            if (isFlattenRunning) return;

            // V12.8: Only log per-execution when ComplianceHub is active
            if (EnableComplianceHub)
                Print(string.Format("[COMPLIANCE] Execution Update received for account."));

            if (EnableComplianceHub)
            {
                Account execAccount = sender as Account;
                if (execAccount != null)
                {
                    TrackTradeEntry(execAccount, e.Execution);
                    UpdateAccountMetricsFromAccount(execAccount);
                }
            }

            // V12.7: Check if this fill is for a fleet entry with deferred brackets
            try
            {
                Order filledOrder = e.Execution?.Order;
                if (filledOrder != null && filledOrder.OrderState == OrderState.Filled)
                {
                    foreach (var kvp in entryOrders.ToArray())
                    {
                        if (kvp.Value == filledOrder)
                        {
                            string fleetKey = kvp.Key;
                            if (activePositions.TryGetValue(fleetKey, out var pos) && pos.IsFollower && !pos.EntryFilled)
                            {
                                double fleetFillPrice = e.Execution != null ? e.Execution.Price : 0;
                                SymmetryGuardOnFollowerFill(fleetKey, pos, fleetFillPrice);
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Print($"[SIMA V12.7] Error in fleet bracket submission: {ex.Message}");
            }

            // Update the compliance log with latest balances (only if ComplianceHub is on and not mass-flattening)
            if (EnableComplianceHub && !isFlattenRunning)
                LogApexPerformance();
        }

        /// <summary>
        /// Writes current account health to a JSON file for the WPF Remote App to read
        /// </summary>
        private void LogApexPerformance()
        {
            if (!EnableComplianceHub || string.IsNullOrEmpty(complianceLogPath)) return;

            // Throttle logging to once per 5 seconds to prevent disk thrashing during heavy fills
            if ((DateTime.Now - lastComplianceLog).TotalSeconds < 5) return;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("{");
                sb.AppendLine("  \"Timestamp\": \"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\",");
                sb.AppendLine("  \"Instrument\": \"" + Instrument.FullName + "\",");
                sb.AppendLine("  \"Accounts\": [");

                List<Account> accounts = GetComplianceAccounts();
                DateTime nowInZone = GetComplianceNow();
                MaybeFinalizeDailySummaries(nowInZone, accounts);

                int count = 0;
                foreach (Account acct in accounts)
                {
                    if (acct == null) continue;

                    if (count > 0) sb.Append(",\n");

                    UpdateAccountMetricsFromAccount(acct);

                    // Basic metrics from NinjaTrader Account object
                    double balance = acct.Get(AccountItem.CashValue, Currency.UsDollar);
                    double dailyPL = accountDailyProfit.TryGetValue(acct.Name, out var dp) ? dp : 0;
                    double totalProfit = accountTotalProfit.GetOrAdd(acct.Name, 0) + dailyPL;
                    int tradeCount = accountTradeCount.TryGetValue(acct.Name, out var tc) ? tc : 0;
                    int uniqueDays = GetUniqueTradingDays(acct.Name);
                    double maxDrawdown = accountMaxDrawdown.TryGetValue(acct.Name, out var dd) ? dd : 0;

                    sb.AppendLine("    {");
                    sb.AppendLine("      \"Name\": \"" + acct.Name + "\",");
                    sb.AppendLine("      \"Balance\": " + balance.ToString("F2") + ",");
                    sb.AppendLine("      \"DailyPL\": " + dailyPL.ToString("F2") + ",");
                    sb.AppendLine("      \"TotalProfit\": " + totalProfit.ToString("F2") + ",");
                    sb.AppendLine("      \"TradeCount\": " + tradeCount + ",");
                    sb.AppendLine("      \"UniqueDays\": " + uniqueDays + ",");
                    sb.AppendLine("      \"MaxDrawdown\": " + maxDrawdown.ToString("F2") + ",");
                    bool isConnected = acct.Connection?.Status == ConnectionStatus.Connected;
                    sb.AppendLine("      \"Connection\": \"" + (isConnected ? "Connected" : "Disconnected") + "\"");
                    sb.Append("    }");
                    count++;
                }

                sb.AppendLine("\n  ]");
                sb.AppendLine("}");

                // V12.40 FREEZE FIX: Fire-and-forget async write — never blocks UI thread
                string jsonPayload = sb.ToString();
                string path = complianceLogPath;
                lastComplianceLog = DateTime.Now;
                Task.Run(() =>
                {
                    try { System.IO.File.WriteAllText(path, jsonPayload); }
                    catch { /* swallow — compliance log is best-effort */ }
                });
            }
            catch (Exception ex)
            {
                Print("[COMPLIANCE] ERROR writing log: " + ex.Message);
            }
        }

        #endregion

        #region V12.30 ATR Auto-Sizing Engine

        /// <summary>
        /// V12.30: ATR Auto-Sizing Engine — Core Sizing Method
        /// 1. stopDistanceRaw → Ceiling to whole point
        /// 2. Quantity → Floor(MaxRisk / (ceilingStop * pointValue))
        /// 3. Clamp to [minContracts, max]
        /// </summary>
        private int CalculatePositionSize(double stopDistanceRaw)
        {
            if (stopDistanceRaw <= 0) return Math.Max(1, minContracts);

            // STEP 1: CEILING to whole POINT (e.g. 2.3 → 3.0, 4.0 → 4.0)
            double stopPoints = Math.Ceiling(stopDistanceRaw);

            double riskToUse = MaxRiskAmount;
            double stopDollars = stopPoints * pointValue;
            if (stopDollars <= 0) return Math.Max(1, minContracts);

            // STEP 2: FLOOR the quantity (never exceed $MaxRisk)
            int contracts = (int)Math.Floor(riskToUse / stopDollars);

            // V12.1101E [B-9]: Clamp to [minContracts, maxContracts] — prevents runaway sizing on
            // tiny ATR values (e.g., flat market) from hitting broker limits or compliance thresholds.
            contracts = Math.Max(minContracts, Math.Min(contracts, maxContracts));

            Print($"[V12.30 SIZING] RawStop={stopDistanceRaw:F2} → Ceiling={stopPoints:F0}pt | Risk=${riskToUse:F0} | StopDollars=${stopDollars:F0} | Qty={contracts} | Clamp=[{minContracts},{maxContracts}]");
            return contracts;
        }

        /// <summary>
        /// V12.30: ATR Auto-Sizing Engine — Centralized Stop Distance Calculator
        /// Returns ATR-based stop rounded UP to nearest whole point.
        /// Replaces all inline "currentATR * multiplier" patterns.
        /// </summary>
        private double CalculateATRStopDistance(double atrMultiplier)
        {
            if (currentATR <= 0) return MinimumStop;

            double rawStop = currentATR * atrMultiplier;
            double ceilingStop = Math.Ceiling(rawStop);  // Round UP to whole point
            return Math.Max(MinimumStop, Math.Min(ceilingStop, MaximumStop));
        }

        private void GetTargetDistribution(int contracts, out int t1, out int t2, out int t3, out int t4)
        {
            // V12.40 PRIORITY FILL: Always respect user percentages.
            // For small lots (≤4), the percentage math naturally assigns 0 to lower-priority targets.
            // T4 (runner) gets the remainder — guaranteed ≥1 when contracts > 1.

            // Single contract = pure runner (T4), no split possible
            if (contracts <= 1)
            {
                t1 = 0; t2 = 0; t3 = 0; t4 = contracts;
                return;
            }

            t1 = (int)Math.Floor(contracts * T1ContractPercent / 100.0);
            t2 = (int)Math.Floor(contracts * T2ContractPercent / 100.0);
            t3 = (int)Math.Floor(contracts * T3ContractPercent / 100.0);

            // V12.1101E [MATH-01]: CAP running sum BEFORE computing T4.
            // The old approach computed t4 = contracts - t1 - t2 - t3, which goes negative when
            // percentages sum > 100%. The post-hoc clamp then floored t4 to 0 while t1+t2+t3
            // already consumed contracts+N slots — creating phantom contract totals.
            // Fix: reduce T3 → T2 → T1 proportionally so t4 is always ≥ 1 before subtraction.
            int runningSum = t1 + t2 + t3;
            int maxAllowed = contracts - 1; // always reserve ≥1 slot for T4 runner
            if (runningSum > maxAllowed)
            {
                int excess = runningSum - maxAllowed;
                // Shed from lowest-priority first: T3 → T2 → T1
                t3 = Math.Max(0, t3 - excess);
                excess = (t1 + t2 + t3) - maxAllowed;
                if (excess > 0) { t2 = Math.Max(0, t2 - excess); }
                excess = (t1 + t2 + t3) - maxAllowed;
                if (excess > 0) { t1 = Math.Max(1, t1 - excess); }
            }
            t4 = contracts - t1 - t2 - t3; // guaranteed ≥ 1

            // Ensure T1 gets at least 1 (the quick scalp anchor)
            if (t1 < 1) { t1 = 1; t4 = Math.Max(0, t4 - 1); }

            // Runner (T4) must get at least 1 on multi-contract trades
            if (t4 < 1)
            {
                // Steal from lowest-priority filled target: T3 → T2
                if (t3 > 0) { t3--; t4 = 1; }
                else if (t2 > 0) { t2--; t4 = 1; }
                else { t1 = contracts - 1; t2 = 0; t3 = 0; t4 = 1; }
            }

            // Safety floors: ensure no negatives remain after any edge-case adjustments
            t1 = Math.Max(0, t1); t2 = Math.Max(0, t2); t3 = Math.Max(0, t3); t4 = Math.Max(0, t4);
        }

        /// <summary>
        /// V12.45: Live Sync Engine — Updates unfilled entry orders when ATR
        /// causes ceiling-stop or floor-qty to change.
        /// FLICKER PROTECTION HARDENING:
        /// 1. Order State Guard: Only sync orders in Accepted/Working state (blocks ChangePending)
        /// 2. Tick-Aware Threshold: Uses tickSize instead of hardcoded 0.01
        /// 3. Retry Cooldown: 500ms pause after ChangeOrder failure to prevent broker hammering
        /// </summary>
        private DateTime _lastSyncFailureTime = DateTime.MinValue;  // V12.45: Retry cooldown tracker

        private void SyncPendingOrders()
        {
            if (currentATR <= 0) return;

            // V12.45 RETRY COOLDOWN: If a ChangeOrder failed recently, back off for 500ms
            // This prevents rapid-fire rejections that can cascade into broker throttling
            if ((DateTime.Now - _lastSyncFailureTime).TotalMilliseconds < 500) return;

            foreach (var kvp in activePositions.ToArray())
            {
                PositionInfo pos = kvp.Value;
                string entryName = kvp.Key;

                // Only sync UNFILLED entries
                if (pos.EntryFilled) continue;

                // Skip modes that don't use ATR-based stops
                if (pos.IsFFMATrade || pos.IsMOMOTrade) continue;

                // Get the entry order
                Order entryOrder;
                if (!entryOrders.TryGetValue(entryName, out entryOrder)) continue;
                if (entryOrder == null) continue;

                // V12.45 ORDER STATE GUARD: Only modify orders in stable states
                // Accepted = broker acknowledged, waiting for fill
                // Working  = actively in the order book
                // ChangePending = a ChangeOrder is already in-flight — DO NOT send another
                OrderState currentState = entryOrder.OrderState;
                if (currentState != OrderState.Accepted && currentState != OrderState.Working)
                {
                    if (currentState == OrderState.ChangePending)
                        Print($"[V12.45 SYNC] SKIP {entryName}: ChangeOrder already in-flight (ChangePending)");
                    continue;
                }

                // Determine ATR multiplier for this trade type
                double atrMult = GetATRMultiplierForPosition(pos);
                double newStopDist = CalculateATRStopDistance(atrMult);
                int    newQty     = CalculatePositionSize(newStopDist);

                // V12.45 TICK-AWARE FLICKER CHECK: Use tickSize for meaningful comparison
                // Only sync if ceiling stop changed by at least 1 tick OR quantity changed
                double oldCeilingStop = Math.Ceiling(Math.Abs(pos.EntryPrice - pos.CurrentStopPrice));
                double newCeilingStop = newStopDist;

                double stopDelta = Math.Abs(newCeilingStop - oldCeilingStop);
                if (stopDelta < tickSize && newQty == pos.TotalContracts)
                    continue;  // No material change — skip

                // SYNC: Update quantity and stop
                try
                {
                    double newStopPrice = pos.Direction == MarketPosition.Long
                        ? pos.EntryPrice - newStopDist
                        : pos.EntryPrice + newStopDist;

                    // Update the entry order quantity via ChangeOrder
                    if (newQty != entryOrder.Quantity)
                    {
                        ChangeOrder(entryOrder, newQty, entryOrder.LimitPrice, entryOrder.StopPrice);
                    }

                    // Update position info
                    pos.TotalContracts = newQty;
                    pos.CurrentStopPrice = newStopPrice;
                    pos.InitialStopPrice = newStopPrice;

                    // Recalculate target distribution
                    int t1, t2, t3, t4;
                    GetTargetDistribution(newQty, out t1, out t2, out t3, out t4);
                    pos.T1Contracts = t1;
                    pos.T2Contracts = t2;
                    pos.T3Contracts = t3;
                    pos.T4Contracts = t4;
                    pos.RemainingContracts = newQty;

                    Print($"[V12.45 SYNC] {entryName}: Stop {oldCeilingStop:F0}→{newCeilingStop:F0}pt | Qty {entryOrder.Quantity}→{newQty} | ATR={currentATR:F2}");
                }
                catch (Exception ex)
                {
                    // V12.45 RETRY COOLDOWN: Record failure time to prevent hammering
                    _lastSyncFailureTime = DateTime.Now;
                    Print($"[V12.45 SYNC] ERROR syncing {entryName}: {ex.Message} — cooldown 500ms");
                }
            }
        }

        /// <summary>
        /// V12.30: Returns the ATR multiplier for a given position type.
        /// Used by SyncPendingOrders to determine which multiplier to recalculate with.
        /// </summary>
        private double GetATRMultiplierForPosition(PositionInfo pos)
        {
            if (pos.IsRMATrade) return RMAStopATRMultiplier;
            if (pos.IsTRENDTrade)
            {
                if (pos.IsTRENDEntry1)
                    return isTrendRmaMode ? RMAStopATRMultiplier : TRENDEntry1ATRMultiplier;
                return isTrendRmaMode ? RMAStopATRMultiplier : TRENDEntry2ATRMultiplier;
            }
            if (pos.IsRetestTrade)
                return isRetestRmaMode ? RMAStopATRMultiplier : RetestATRMultiplier; // V12.Hardening: was isTrendRmaMode (typo)
            return StopMultiplier; // ORB default
        }

        #endregion

        // v5.12: Execute target actions (T1 or T2)
        private void ExecuteTargetAction(string targetType, string action)
        {
            try
            {
                if (activePositions.Count == 0)
                {
                    Print(string.Format("{0} ACTION: No active positions", targetType));
                    return;
                }

                // V8.30: Thread-safe snapshot iteration
                foreach (var kvp in activePositions.ToArray())
                {
                    if (!activePositions.ContainsKey(kvp.Key)) continue;
                    PositionInfo pos = kvp.Value;
                    string entryName = kvp.Key;

                    if (!pos.EntryFilled)
                    {
                        Print(string.Format("{0} ACTION: Position {1} not filled yet", targetType, entryName));
                        continue;
                    }

                    // V8.30: Use ConcurrentDictionary reference directly
                    ConcurrentDictionary<string, Order> targetOrders = (targetType == "T1") ? target1Orders : (targetType == "T2" ? target2Orders : target3Orders);
                    int targetContracts = (targetType == "T1") ? pos.T1Contracts : (targetType == "T2" ? pos.T2Contracts : pos.T3Contracts);
                    bool targetFilled = (targetType == "T1") ? pos.T1Filled : (targetType == "T2" ? pos.T2Filled : pos.T3Filled);

                    if (targetFilled)
                    {
                        Print(string.Format("{0} ACTION: {1} already filled for {2}", targetType, targetType, entryName));
                        continue;
                    }

                    double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];

                    switch (action)
                    {
                        case "market":
                            // Fill target at market NOW
                            // V8.30: Thread-safe removal
                            if (targetOrders.TryRemove(entryName, out var existingOrder))
                            {
                                CancelOrder(existingOrder);
                            }

                            Order marketOrder = pos.Direction == MarketPosition.Long
                                ? SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, targetContracts, 0, 0, "", targetType + "_Market_" + entryName)
                                : SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, targetContracts, 0, 0, "", targetType + "_Market_" + entryName);

                            Print(string.Format("★ {0} MARKET FILL: {1} - Closing {2} contracts at market", targetType, entryName, targetContracts));
                            break;

                        case "1point":
                            // V8.18: Absolute profit target (Entry + 1 point)
                            double newPrice1pt = pos.Direction == MarketPosition.Long
                                ? pos.EntryPrice + 1.0
                                : pos.EntryPrice - 1.0;
                            newPrice1pt = Instrument.MasterInstrument.RoundToTickSize(newPrice1pt);

                            Print(string.Format("★ {0} → 1 POINT PROFIT: {1} - New target @ {2:F2} (Entry was {3:F2})",
                                targetType, entryName, newPrice1pt, pos.EntryPrice));

                            MoveTargetOrder(entryName, targetType, newPrice1pt, targetContracts, pos.Direction);
                            break;

                        case "2point":
                            // V8.18: Absolute profit target (Entry + 2 points)
                            double newPrice2pt = pos.Direction == MarketPosition.Long
                                ? pos.EntryPrice + 2.0
                                : pos.EntryPrice - 2.0;
                            newPrice2pt = Instrument.MasterInstrument.RoundToTickSize(newPrice2pt);

                            Print(string.Format("★ {0} → 2 POINTS PROFIT: {1} - New target @ {2:F2} (Entry was {3:F2})",
                                targetType, entryName, newPrice2pt, pos.EntryPrice));

                            MoveTargetOrder(entryName, targetType, newPrice2pt, targetContracts, pos.Direction);
                            break;

                        case "marketprice":
                            // Move target to current market price (instant fill)
                            double marketPrice = Instrument.MasterInstrument.RoundToTickSize(currentPrice);
                            MoveTargetOrder(entryName, targetType, marketPrice, targetContracts, pos.Direction);
                            Print(string.Format("★ {0} → MARKET PRICE: {1} - New target @ {2:F2}", targetType, entryName, marketPrice));
                            break;

                        case "breakeven":
                            // Move target to breakeven (entry price)
                            MoveTargetOrder(entryName, targetType, pos.EntryPrice, targetContracts, pos.Direction);
                            Print(string.Format("★ {0} → BREAKEVEN: {1} - New target @ {2:F2}", targetType, entryName, pos.EntryPrice));
                            break;

                        case "cancel":
                            // Cancel target order - let contracts run
                            // V8.30: Thread-safe removal
                            if (targetOrders.TryRemove(entryName, out var cancelOrder))
                            {
                                CancelOrder(cancelOrder);
                                Print(string.Format("★ {0} CANCELLED: {1} - {2} contracts will run with stop", targetType, entryName, targetContracts));
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Print(string.Format("ERROR ExecuteTargetAction ({0}, {1}): {2}", targetType, action, ex.Message));
            }
        }

        private void MoveTargetOrder(string entryName, string targetType, double newPrice, int quantity, MarketPosition direction)
        {
            // V8.30: Use ConcurrentDictionary reference directly
            ConcurrentDictionary<string, Order> targetOrders = (targetType == "T1") ? target1Orders : (targetType == "T2" ? target2Orders : target3Orders);

            // V8.30: Thread-safe cancel existing target order
            if (targetOrders.TryRemove(entryName, out var existingTarget))
            {
                CancelOrder(existingTarget);
            }

            // Submit new target order at new price
            Order newTargetOrder = direction == MarketPosition.Long
                ? SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Limit, quantity, newPrice, 0, "", targetType + "_" + entryName)
                : SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Limit, quantity, newPrice, 0, "", targetType + "_" + entryName);

            if (newTargetOrder != null)
            {
                targetOrders[entryName] = newTargetOrder;
            }
        }

        // v5.12: Execute runner actions
        private void ExecuteRunnerAction(string action)
        {
            try
            {
                if (activePositions.Count == 0)
                {
                    Print("RUNNER ACTION: No active positions");
                    return;
                }

                // V8.30: Thread-safe snapshot iteration
                foreach (var kvp in activePositions.ToArray())
                {
                    if (!activePositions.ContainsKey(kvp.Key)) continue;
                    PositionInfo pos = kvp.Value;
                    string entryName = kvp.Key;

                    if (!pos.EntryFilled)
                    {
                        Print(string.Format("RUNNER ACTION: Position {0} not filled yet", entryName));
                        continue;
                    }

                    // Calculate runner contracts (remaining after T1 and T2)
                    int runnerContracts = pos.RemainingContracts;
                    if (runnerContracts <= 0)
                    {
                        Print(string.Format("RUNNER ACTION: No runner contracts for {0}", entryName));
                        continue;
                    }

                    double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];

                    switch (action)
                    {
                        case "market":
                            // Close runner at market
                            Order runnerMarketOrder = pos.Direction == MarketPosition.Long
                                ? SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, runnerContracts, 0, 0, "", "Runner_Market_" + entryName)
                                : SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, runnerContracts, 0, 0, "", "Runner_Market_" + entryName);

                            Print(string.Format("★ RUNNER MARKET CLOSE: {0} - Closing {1} contracts at market", entryName, runnerContracts));
                            break;

                        case "stop1pt":
                            // V8.19: Absolute profit lock (Entry + 1 point)
                            double newStop1pt = pos.Direction == MarketPosition.Long
                                ? pos.EntryPrice + 1.0
                                : pos.EntryPrice - 1.0;
                            newStop1pt = Instrument.MasterInstrument.RoundToTickSize(newStop1pt);

                            // Safety: Only move if it's better than current stop or entry-relative profit-lock
                            UpdateStopOrder(entryName, pos, newStop1pt, pos.CurrentTrailLevel);
                            Print(string.Format("★ RUNNER STOP → 1 PT PROFIT LOCK: {0} - Stop @ {1:F2} (Entry was {2:F2})", entryName, newStop1pt, pos.EntryPrice));
                            break;

                        case "stop2pt":
                            // V8.19: Absolute profit lock (Entry + 2 points)
                            double newStop2pt = pos.Direction == MarketPosition.Long
                                ? pos.EntryPrice + 2.0
                                : pos.EntryPrice - 2.0;
                            newStop2pt = Instrument.MasterInstrument.RoundToTickSize(newStop2pt);

                            UpdateStopOrder(entryName, pos, newStop2pt, pos.CurrentTrailLevel);
                            Print(string.Format("★ RUNNER STOP → 2 PT PROFIT LOCK: {0} - Stop @ {1:F2} (Entry was {2:F2})", entryName, newStop2pt, pos.EntryPrice));
                            break;

                        case "stopbe":
                            // Move stop to breakeven
                            UpdateStopOrder(entryName, pos, pos.EntryPrice, 1);
                            Print(string.Format("★ RUNNER STOP → BREAKEVEN: {0} - Stop @ {1:F2}", entryName, pos.EntryPrice));
                            break;

                        case "lock50":
                            // Lock 50% of current profit
                            double unrealizedProfit = pos.Direction == MarketPosition.Long
                                ? currentPrice - pos.EntryPrice
                                : pos.EntryPrice - currentPrice;
                            double lock50Stop = pos.Direction == MarketPosition.Long
                                ? pos.EntryPrice + (unrealizedProfit * 0.5)
                                : pos.EntryPrice - (unrealizedProfit * 0.5);
                            lock50Stop = Instrument.MasterInstrument.RoundToTickSize(lock50Stop);
                            UpdateStopOrder(entryName, pos, lock50Stop, pos.CurrentTrailLevel);
                            Print(string.Format("★ RUNNER LOCK 50%: {0} - Stop @ {1:F2} (profit: {2:F2})", entryName, lock50Stop, unrealizedProfit));
                            break;

                        case "disabletrail":
                            // Disable trailing - keep stop where it is
                            pos.CurrentTrailLevel = 999; // Set to high number to prevent further trailing
                            Print(string.Format("★ RUNNER TRAILING DISABLED: {0} - Stop fixed @ {1:F2}", entryName, pos.CurrentStopPrice));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Print(string.Format("ERROR ExecuteRunnerAction ({0}): {1}", action, ex.Message));
            }
        }

    }
}
