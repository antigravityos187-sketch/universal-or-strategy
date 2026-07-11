// PTT-COPIER-B9-T2 / B10-T4 -- TradeCopierAddOn.cs
// B10-T4: chart attachment uses DispatcherTimer (1s polling) as compile-safe fallback.
// AddOnBase entry point.
// 1. Registers "Trade Copier" in the NT8 Control Center New menu (once only).
// 2. Injects TradeCopierPanel into every chart's ChartTrader panel area.
//
// CHARTTRADER INJECTION APPROACH (FIX5):
//   ChartTrader is NOT a Window -- it is a UserControl inside NinjaTrader.Gui.Chart.Chart.
//   NinjaTrader.Gui.Chart.Chart IS a System.Windows.Window subclass.
//   Strategy: cast window to NinjaTrader.Gui.Chart.Chart, hook its Loaded event,
//   then walk the visual tree to find the StackPanel named "RootPanel" or "Rows"
//   that sits inside ChartTrader, and append our UserControl to it.
//   Fallback: if named panel not found, wrap Chart.Content in a DockPanel.
//
// Jane Street rules: JS-021 (no lock), JS-023 (volatile bool for menu guard)
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;

namespace PropTraderTools
{
    public class TradeCopierAddOn : AddOnBase
    {
        // JS-023: volatile bool prevents duplicate menu wiring across reloads
        private static volatile bool _menuWired = false;

        // Track injected panels keyed by Chart window to detach on close
        private static readonly ConcurrentDictionary<Chart, TradeCopierPanel> _panels
            = new ConcurrentDictionary<Chart, TradeCopierPanel>();

        // B9 T1 -- ATR engine instances keyed by Chart window
        private static readonly ConcurrentDictionary<Chart, AtrSizingEngine> _atrEngines
            = new ConcurrentDictionary<Chart, AtrSizingEngine>();

        // B9 T2 -- Click trader handler registry (ADV-001 CORRECTED: TryRemove-first)
        private static readonly ConcurrentDictionary<Chart, TradeCopierPanel> _clickHandlers
            = new ConcurrentDictionary<Chart, TradeCopierPanel>();

        // B10 T4: WPF overlay label injected into ChartTrader panel (most-recently-attached chart)
        // Single-writer UI thread only. Null when no overlay has been built yet.
        private TextBlock _atrOverlayLabel = null;

        // B10 T4: polling timer for ATR computation fallback (when bar event not available)
        // Fires engine.ManualOnBarUpdate every 1 second on UI thread as a safe fallback.
        private DispatcherTimer _atrPollTimer = null;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Prop Trader Tools -- Trade Copier";
                Name        = "TradeCopierAddOn";
            }
            if (State == State.Terminated)
                _menuWired = false;
        }

        protected override void OnWindowCreated(System.Windows.Window window)
        {
            // Surface 1: Control Center menu (once only)
            if (!_menuWired)
            {
                var cc = window as ControlCenter;
                if (cc != null)
                {
                    WireControlCenterMenu(cc);
                    return;
                }
            }

            // Surface 2: Chart window contains ChartTrader as a child control
            var chart = window as Chart;
            if (chart != null)
                InjectIntoChart(chart);   // instance call: StartAtrEngine needs _atrOverlayLabel
        }

        protected override void OnWindowDestroyed(System.Windows.Window window)
        {
            var chart = window as Chart;
            if (chart == null) return;
            StopAtrEngine(chart);           // instance: unsubscribes AtrUpdated
            UnregisterClickTrader(chart);   // B9 T2: clean up click handler
            TradeCopierPanel panel;
            if (_panels.TryRemove(chart, out panel))
                panel.Detach();
        }

        // --- Menu wiring ---

        private static void WireControlCenterMenu(ControlCenter cc)
        {
            NTMenuItem newMenu = null;
            foreach (var item in cc.MainMenu)
            {
                var mi  = item as NTMenuItem;
                if (mi == null) continue;
                var hdr = mi.Header != null ? mi.Header.ToString() : string.Empty;
                if (hdr.StartsWith("New")) { newMenu = mi; break; }
            }
            if (newMenu == null) return;

            // Remove ALL "Trade Copier" entries before adding a fresh one.
            // The survivor scan approach fails because cross-domain MenuItem casts are
            // unreliable (NTMenuItem vs MenuItem vs other WPF types in Items collection).
            // Remove-then-add is simpler and guarantees exactly one entry regardless of
            // how many prior domain reloads accumulated stale entries.
            for (int i = newMenu.Items.Count - 1; i >= 0; i--)
            {
                var mi  = newMenu.Items[i] as System.Windows.Controls.MenuItem;
                if (mi == null) continue;
                if (mi.Header != null && mi.Header.ToString() == "Trade Copier")
                    newMenu.Items.RemoveAt(i);
            }

            var entry = new NTMenuItem { Header = "Trade Copier" };
            entry.Click += OnMenuItemClick;
            newMenu.Items.Add(entry);
            _menuWired = true;
        }

        private static void OnMenuItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var win = new TradeCopierWindow();
                win.Topmost = true;
                win.Show();
                win.Activate();
                win.Focus();
                win.Topmost = false;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    "Trade Copier failed to open:\n\n" + ex.Message + "\n\n" + ex.StackTrace,
                    "PTT Error");
            }
        }

        // --- Chart injection ---

        // Instance method: chains to DoInject which calls StartAtrEngine (instance).
        private void InjectIntoChart(Chart chart)
        {
            // NT8 timing: OnWindowCreated may fire AFTER the window is already loaded.
            // If IsLoaded is already true, inject immediately on the dispatcher.
            // If not yet loaded, hook the Loaded event.
            if (chart.IsLoaded)
            {
                chart.Dispatcher.InvokeAsync(() => DoInject(chart));
            }
            else
            {
                chart.Loaded += OnChartLoaded;
            }
        }

        // Instance event handler: must capture 'this' to call DoInject as instance method.
        private void OnChartLoaded(object sender, RoutedEventArgs e)
        {
            var chart = sender as Chart;
            if (chart == null) return;
            chart.Loaded -= OnChartLoaded;
            chart.Dispatcher.InvokeAsync(() => DoInject(chart));
        }

        // B10 T4: StartAtrEngine -- instance method (not static).
        // Replaces IMPL-NOTE-1 stub. Attempts chart attachment via event-based fallback
        // (Step 3: always compiles). NinjaScripts.Add / Indicators.Add require runtime
        // verification which is deferred -- fallback is the safe, compile-guaranteed path.
        //
        // CHART-ATTACH-RESULT: event-based fallback (Step 3) -- compile-safe for NT8 .NET 4.8.
        // chart.NinjaScripts.Add and chart.Indicators.Add are not available at design time
        // in the AddOn compilation context (CS1061 errors in NT8 Roslyn). Fallback chosen.
        // Verified: 2026-07-09
        //
        // CYC=4: chart null(1), instr null(2), attachment try(3), overlay build(4)
        private void StartAtrEngine(Chart chart, NinjaTrader.Cbi.Instrument instr)
        {
            if (chart == null) return;                        // guard (1)
            if (instr  == null) return;                       // guard (2)
            var engine = new AtrSizingEngine();
            double pointValue = instr.MasterInstrument?.PointValue ?? 5.0;
            engine.SetParameters(150.0, pointValue);
            _atrEngines[chart] = engine;

            // STEP 3 (event-based fallback -- compile-safe DispatcherTimer, 1s polling).
            // chart.NinjaScripts.Add / Indicators.Add / BarsArray are not accessible in
            // AddOnBase compilation scope (NT8 Roslyn design-time limitation).
            // DispatcherTimer is WPF standard and always compiles in AddOnBase context.
            if (_atrPollTimer == null)                        // guard (3): create timer once
            {
                _atrPollTimer = new DispatcherTimer(DispatcherPriority.Background)
                {
                    Interval = System.TimeSpan.FromSeconds(1)
                };
                _atrPollTimer.Tick += (s, e2) =>
                {
                    try { engine.ManualOnBarUpdate(); }
                    catch (System.Exception) { /* NT8 context not ready; next tick will retry */ }
                };
                _atrPollTimer.Start();
            }

            CopyEngine.Instance.SetAtrEngine(engine, enabled: false); // disabled until user enables

            // WPF OVERLAY: inject ATR display into ChartTrader panel (guard 4)
            var chartTraderRoot = ResolveChartTraderPanel(chart);
            if (chartTraderRoot != null)                      // guard (4)
            {
                BuildAtrOverlayRow(chartTraderRoot);
                engine.AtrUpdated += OnAtrUpdated;
            }
        }

        // B10 T4: instance StopAtrEngine -- unsubscribes AtrUpdated and stops poll timer.
        // CYC=3 -- TryRemove guard + engine event cleanup + timer cleanup
        private void StopAtrEngine(Chart chart)
        {
            AtrSizingEngine engine;
            if (!_atrEngines.TryRemove(chart, out engine)) return; // guard (1)
            if (engine != null)
                engine.AtrUpdated -= OnAtrUpdated;                 // unsubscribe event
            if (_atrPollTimer != null)                             // guard (2): stop poll timer
            {
                _atrPollTimer.Stop();
                _atrPollTimer = null;
            }
            CopyEngine.Instance.SetAtrEngine(null, enabled: false); // guard (3): clear reference
        }

        // B10 T4: traverse chart's visual tree to locate the ChartTrader root Panel.
        // Returns null if ChartTrader is not found -- callers skip overlay gracefully.
        // CYC=2: null guard(1), FindVisualChild result check(2)
        private Panel ResolveChartTraderPanel(Chart chart)
        {
            if (chart == null) return null;                    // guard (1)
            var chartTrader = FindVisualChild<ChartTrader>(chart);
            if (chartTrader == null) return null;              // guard (2)
            return chartTrader.Content as Panel;
        }

        // B10 T4: build ATR overlay row and inject into ChartTrader panel.
        // Creates a Border containing a TextBlock with ASCII placeholder text.
        // No font-family, no hardcoded hex colors set on any element.
        // CYC=1: straight-line widget construction; no branches.
        private void BuildAtrOverlayRow(Panel chartTraderRoot)
        {
            var border = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(2),
                Padding         = new Thickness(4, 2, 4, 2),
                Margin          = new Thickness(2)
            };
            _atrOverlayLabel = new TextBlock
            {
                Text = "ATR=-.-- pts -> stopTicks=-- -> qty=--"
            };
            border.Child = _atrOverlayLabel;
            chartTraderRoot.Children.Add(border);
        }

        // B10 T4: update ATR overlay label via Application.Current.Dispatcher.InvokeAsync.
        // Called from OnAtrUpdated which fires on the bar-close background thread.
        // AddOnBase does not inherit from DispatcherObject, so use Application.Current.Dispatcher.
        // CYC=2: null guard on _atrOverlayLabel(1), Dispatcher.InvokeAsync update(2)
        internal void UpdateAtrOverlay(string atrDisplay)
        {
            if (_atrOverlayLabel == null) return;             // guard (1): overlay may not exist
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() => // guard (2): marshal to UI thread
                _atrOverlayLabel.Text = atrDisplay);
        }

        // B10 T4: AtrUpdated event handler -- subscribed in StartAtrEngine.
        // Fires on AtrSizingEngine bar-close thread; UpdateAtrOverlay marshals via Dispatcher.
        // CYC=1: straight-line delegation.
        private void OnAtrUpdated(string display)
        {
            UpdateAtrOverlay(display);
        }

        // B9 T2: CYC=2 -- null guard + TryRemove branch (ADV-001 CORRECTED: TryRemove-first)
        // Removes old handler BEFORE adding new one to prevent ghost handler accumulation.
        // NT8: Chart.ChartControl not accessible from AddOn -- find via visual tree.
        internal static void RegisterClickTrader(Chart chart, TradeCopierPanel panel)
        {
            if (chart == null) return;                                         // guard (1)
            var cc = FindVisualChild<ChartControl>(chart);
            TradeCopierPanel old;
            if (_clickHandlers.TryRemove(chart, out old) && cc != null)        // guard (2): remove old first
                cc.MouseDown -= old.OnChartMouseDown;
            _clickHandlers[chart] = panel;                                     // store new
            if (cc != null) cc.MouseDown += panel.OnChartMouseDown;           // hook new
        }

        // B9 T2: CYC=2 -- TryRemove guard + null ChartControl guard
        internal static void UnregisterClickTrader(Chart chart)
        {
            TradeCopierPanel panel;
            if (!_clickHandlers.TryRemove(chart, out panel)) return;           // guard (1)
            var cc = FindVisualChild<ChartControl>(chart);
            if (cc == null) return;                                            // guard (2)
            cc.MouseDown -= panel.OnChartMouseDown;
        }

        // Instance method: calls StartAtrEngine (instance) for B10 T4 overlay support.
        private void DoInject(Chart chart)
        {
            // Atomic slot claim -- first caller wins, all subsequent calls return immediately.
            // Replaces the old ContainsKey guard which was blind to prior AddOn instances.
            if (!_panels.TryAdd(chart, null)) return;

            try
            {
                var chartTrader = FindVisualChild<ChartTrader>(chart);
                if (chartTrader == null)
                {
                    _panels.TryRemove(chart, out _);
                    return;
                }

                // Purge ALL stale TradeCopierPanel rows from prior domain reloads.
                // NT8 reuses the ChartTrader Grid across F5 recompiles.
                // Each reload: _panels resets to empty, TryAdd succeeds, DoInject runs,
                // appends another row. After N reloads there are N panel rows + N extra
                // RowDefinitions. Fix: remove every panel-named child AND its RowDefinition
                // before adding a fresh one. GetType().Name survives domain reload (string).
                var grid = chartTrader.Content as System.Windows.Controls.Grid;
                if (grid != null)
                {
                    // Collect stale children first -- cannot remove during enumeration.
                    var stale = new System.Collections.Generic.List<UIElement>();
                    foreach (UIElement child in grid.Children)
                    {
                        if (child.GetType().Name == "TradeCopierPanel")
                            stale.Add(child);
                    }
                    foreach (var old in stale)
                    {
                        int staleRow = System.Windows.Controls.Grid.GetRow(old);
                        grid.Children.Remove(old);
                        // Remove the RowDefinition that was added for this panel row.
                        // Guard: only remove rows beyond the original NT8 rows (index > 0).
                        if (staleRow > 0 && staleRow < grid.RowDefinitions.Count)
                            grid.RowDefinitions.RemoveAt(staleRow);
                    }
                }

                // No survivor -- fresh inject.
                var panel = new TradeCopierPanel();

                NinjaTrader.Cbi.Instrument instr = null;
                try
                {
                    instr = chartTrader.Instrument;
                    if (instr != null) panel.SetInstrument(instr);
                }
                catch { }

                StartAtrEngine(chart, instr);
                panel.SetChart(chart);

                // Wire leader account from ChartTrader account ComboBox.
                WireLeaderAccount(chartTrader, panel);

                if (grid != null)
                {
                    var row = new RowDefinition { Height = System.Windows.GridLength.Auto };
                    grid.RowDefinitions.Add(row);
                    System.Windows.Controls.Grid.SetRow(panel, grid.RowDefinitions.Count - 1);
                    System.Windows.Controls.Grid.SetColumnSpan(
                        panel,
                        grid.ColumnDefinitions.Count > 0 ? grid.ColumnDefinitions.Count : 1);
                    grid.Children.Add(panel);
                    _panels[chart] = panel;
                    return;
                }

                MessageBox.Show(
                    "PTT: ChartTrader.Content is not a Grid.\n" +
                    "Content type: " + (chartTrader.Content?.GetType().FullName ?? "null"),
                    "PTT Info");
            }
            catch (System.Exception ex)
            {
                _panels.TryRemove(chart, out _);
                MessageBox.Show(
                    "PTT ChartTrader inject error:\n\n" + ex.Message + "\n\n" + ex.StackTrace,
                    "PTT Error");
            }
        }

        // CYC=3: null guard (1) + SelectedItem cast (2) + SelectionChanged subscription (3)
        // Finds the ChartTrader account ComboBox via visual tree and wires it to SetLeaderAccount.
        // Called on fresh inject AND on adopt -- ensures _leaderAccount is always populated.
        // NT8-023: lambda captures only accountCombo + panel (same visual tree lifetime -- safe).
        // Do NOT capture chart or chartTrader in the lambda.
        private static void WireLeaderAccount(ChartTrader chartTrader, TradeCopierPanel panel)
        {
            var accountCombo = FindVisualChild<ComboBox>(chartTrader);
            if (accountCombo == null) return;

            // Set immediately from current selection
            var current = accountCombo.SelectedItem as NinjaTrader.Cbi.Account;
            if (current != null) panel.SetLeaderAccount(current);

            // Keep live as user switches accounts
            accountCombo.SelectionChanged += (s, e) =>
            {
                var acc = accountCombo.SelectedItem as NinjaTrader.Cbi.Account;
                panel.SetLeaderAccount(acc);
            };
        }

        // --- Visual tree helpers (CYC=1 each) ---

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T match) return match;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private static T FindVisualChildByName<T>(DependencyObject parent, string name)
            where T : FrameworkElement
        {
            if (parent == null) return null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T fe && fe.Name == name) return fe;
                var result = FindVisualChildByName<T>(child, name);
                if (result != null) return result;
            }
            return null;
        }

        // -- DIAG: GAP-001d + GAP-002 Sim101 tests (REMOVE AFTER TESTS) --
        // Called from TradeCopierPanel diag buttons. Output via MessageBox (no Print API in AddOn).

        // GAP-001d: does acc.Change(StopPrice) on a trailing stop preserve or kill the trail?
        // Finds the first working StopMarket on the given account, snapshots StopPrice BEFORE
        // calling acc.Change(), waits 600ms, snapshots AFTER. Shows result in a MessageBox.
        // User must then watch NT8 Order Flow to observe whether stop moves with price.
        // CYC=3: order null guard (1) + Task.Delay ContinueWith (2) + updated null guard (3)
        internal static void RunGap001dTest(NinjaTrader.Cbi.Account acc, NinjaTrader.Cbi.Instrument instr)
        {
            NinjaTrader.Cbi.Order trailingStop = null;
            foreach (var o in acc.Orders)
            {
                if (o.OrderState == NinjaTrader.Cbi.OrderState.Working
                 && o.OrderType  == NinjaTrader.Cbi.OrderType.StopMarket)
                {
                    trailingStop = o;
                    break;
                }
            }

            if (trailingStop == null)
            {
                MessageBox.Show(
                    "GAP-001d: no working StopMarket order found on " + acc.Name + ".\n\n"
                    + "Enter a position with a TRAILING STOP ATM first, let price move 10+ ticks in favour, then click GAP-001d again.",
                    "GAP-001d");
                return;
            }

            double stopBefore  = trailingStop.StopPrice;
            string nameBefore  = trailingStop.Name ?? "(no name)";
            double tickSize    = instr.MasterInstrument.TickSize;

            // Move stop 2 ticks toward current price via acc.Change()
            trailingStop.StopPrice = stopBefore + (2.0 * tickSize);
            string changeResult = "OK";
            try
            {
                acc.Change(new NinjaTrader.Cbi.Order[] { trailingStop });
            }
            catch (System.Exception ex)
            {
                changeResult = "THREW: " + ex.Message;
            }

            double stopAfterImmediate = trailingStop.StopPrice;

            // Wait 600ms then snapshot AFTER -- NT8 order engine needs a moment to confirm
            System.Threading.Tasks.Task.Delay(600).ContinueWith(_ =>
            {
                NinjaTrader.Cbi.Order updated = null;
                foreach (var o in acc.Orders)
                {
                    if (o.OrderState == NinjaTrader.Cbi.OrderState.Working
                     && o.OrderType  == NinjaTrader.Cbi.OrderType.StopMarket)
                    {
                        updated = o;
                        break;
                    }
                }

                string afterLine;
                if (updated == null)
                    afterLine = "AFTER (600ms): order gone (filled or cancelled)";
                else
                    afterLine = "AFTER (600ms): StopPrice=" + updated.StopPrice.ToString("F4")
                              + "  Name=" + (updated.Name ?? "(no name)");

                string msg =
                    "GAP-001d RESULT\n"
                    + "Account: " + acc.Name + "\n\n"
                    + "BEFORE:  StopPrice=" + stopBefore.ToString("F4") + "  Name=" + nameBefore + "\n"
                    + "acc.Change(): " + changeResult + "\n"
                    + afterLine + "\n\n"
                    + "NOW: let price move 5+ more ticks in favour.\n"
                    + "Watch the StopMarket order in NT8 Order Flow:\n"
                    + "  If stop price moves with price -> TRAIL IS ALIVE\n"
                    + "  If stop price stays frozen    -> TRAIL IS DEAD\n\n"
                    + "Paste these lines + your observation into the director session.";

                System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    MessageBox.Show(msg, "GAP-001d"));
            });
        }

        // GAP-002: does Account.MarketReplayConnect (or position.Instrument subscription) fire price ticks
        // from AddOn context? Tests via Account.AccountItemUpdate as the available price proxy.
        // Shows a 10-second subscription result summary in a MessageBox.
        // CYC=2: acc null guard (1) + item filter branch (2)
        private static volatile int _gap002TickCount = 0;
        private static NinjaTrader.Cbi.Account _gap002Account = null;

        internal static void RunGap002Test(NinjaTrader.Cbi.Instrument cbiInstr)
        {
            // GAP-002 tests whether Instrument.MarketData is accessible from AddOn context.
            // NT8 NinjaScript AddOn does not expose NinjaTrader.Data.Instrument directly.
            // Instead we test the closest available hook: Account.AccountItemUpdate firing
            // on UnrealizedPnL changes (which are price-driven, per-tick on open positions).
            // This is Option B from the GAP-002 spec. If it fires, pending BE can use it.
            NinjaTrader.Cbi.Account testAcc = null;
            if (Account.All != null)
            {
                foreach (var a in Account.All)
                {
                    if (a.Name.IndexOf("Sim", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        testAcc = a;
                        break;
                    }
                }
            }

            if (testAcc == null)
            {
                MessageBox.Show(
                    "GAP-002: no Sim account found in Account.All.\n"
                    + "Open a Sim101 account in NT8 first.",
                    "GAP-002");
                return;
            }

            _gap002TickCount  = 0;
            _gap002Account    = testAcc;
            testAcc.AccountItemUpdate += OnGap002AccountUpdate;

            MessageBox.Show(
                "GAP-002: subscribed to AccountItemUpdate on " + testAcc.Name + ".\n\n"
                + "Now enter or hold a Sim101 position on " + cbiInstr.FullName + " and let price move.\n"
                + "Each UnrealizedPnL change fires a tick event.\n\n"
                + "After 10 ticks (or 30 seconds), click GAP-002 again to see the tick count.\n"
                + "If tick count is 0 after price moves -> Option B does NOT fire on flat account.",
                "GAP-002 armed");
        }

        private static void OnGap002AccountUpdate(object sender, AccountItemEventArgs e)
        {
            if (e.AccountItem != AccountItem.UnrealizedProfitLoss) return;  // filter (1)
            System.Threading.Interlocked.Increment(ref _gap002TickCount);
            int count = _gap002TickCount;
            if (count < 10) return;                                         // branch (2)

            // 10 ticks received -- unsubscribe and report
            var acc = sender as NinjaTrader.Cbi.Account;
            if (acc != null)
                acc.AccountItemUpdate -= OnGap002AccountUpdate;
            _gap002Account = null;

            int final = count;
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                MessageBox.Show(
                    "GAP-002 RESULT: AccountItemUpdate (UnrealizedPnL) fired " + final + " times.\n\n"
                    + "This confirms Option B (AccountItemUpdate price proxy) IS available\n"
                    + "in AddOn context for Pending BE price watching.\n\n"
                    + "Paste this result into the director session.",
                    "GAP-002 result"));
        }
        // -- END DIAG --
    }
}
