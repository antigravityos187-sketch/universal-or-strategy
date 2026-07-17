// PTT-COPIER-B17-T1 -- TradeCopierPanel.cs
// B17 T1 CHANGES:
//   1. Added _b17DiagDone volatile bool field (fire-once diagnostic gate).
//   2. Added EnumerateAllChartPanels(ChartControl cc): walks visual tree, probes Charts via Reflection, shows MessageBox.
//   3. Modified OnChartMouseDown: calls EnumerateAllChartPanels once via _b17DiagDone gate.
//   4. Added GetRefPrice() fallback for rawPrice when ClickTrader price returns 0.
//   [AMEND] TradeCopierAddOn.cs: MouseDown -> PreviewMouseDown (Director auth, DW-B17-02).

// PTT-COPIER-B15-T2 -- TradeCopierPanel.cs
// B15 T2 CHANGES:
//   1. Removed _chartDiagDone volatile bool field (T1 diagnostic cleanup).
//   2. Removed DumpReflectionPath(ChartControl cc, StringBuilder sb) method (T1 diagnostic cleanup).
//   3. Removed DumpVisualTree(ChartControl cc, StringBuilder sb) method (T1 diagnostic cleanup).
//   4. Removed DumpChartControlTree(ChartControl cc) method (T1 diagnostic cleanup).
//   5. Reverted SetChart(Chart chart) to CYC=1 (removed DumpChartControlTree call).
//   6. Added GetPriceAtY(ChartControl cc, double y) private static method (CYC=4).
//   7. Modified OnChartMouseDown: replaced 0.0 stub + suppression line with real lookup.
//      Final CYC=6. Click-trader Y-to-price lookup CLOSED (B15-T2).
// PTT-COPIER-B15-T1 -- TradeCopierPanel.cs (CLEANUP -- diagnostic removed in T2)
// PTT-COPIER-B14-T1 -- TradeCopierPanel.cs
// B14 T1 CHANGES:
//   1. Modified OnBeConnected: added ArmTrailBe call after BreakEven (CYC=3).
//   2. Modified OnBeClick Connected case: added DisarmTrailBe alongside DisarmPendingBe.
//   3. Modified Detach(): added DisarmTrailBe alongside DisarmPendingBe (cleanup path).
// PTT-COPIER-B12-T3 -- TradeCopierPanel.cs
// B12 T3 CHANGES:
//   1. Added _maxRiskDollars, _atrFraction (plain double), _riskDollarsBox, _atrFractionBox fields.
//   2. Added BuildRiskAtrRow() -- Risk $ + ATR % spinners inside _contentPanel (last row).
//   3. Added OnRiskUp/Down, OnRiskTextLostFocus, OnAtrFractionUp/Down, OnAtrFractionTextLostFocus.
//   4. Added NotifyRiskChanged(), NotifyAtrFractionChanged().
//   5. Modified BuildUI() to call BuildRiskAtrRow at end of _contentPanel.
// PTT-COPIER-B12-T1 -- TradeCopierPanel.cs
// B12 T1 CHANGES:
//   1. Added _trimBuffer, _flattenBuffer, _beBuffer (plain int), _beState (BeState), new Button refs.
//   2. Added BeState enum (Idle/Armed/Connected) -- 3-state FSM.
//   3. Added BrushConnected frozen brush (blue, RGB 59/130/246).
//   4. Added BuildBufferedButtonsRow() -- 3-row buffered button section inside _contentPanel.
//   5. Added FormatBuffer() static helper.
//   6. Added OnTrimUp/Down/Click, OnFlattenUp/Down/Click, OnBeUp/Down/Click handlers.
//   7. Added UpdateBeLabel(), UpdateBeVisuals(BeState), OnBeConnected(string), GetRefPrice().
//   8. Added OnCopyToggle, OnCancel2.
//   9. Removed _beArmBtn/_beArmState/_beArmBufferBox; removed BuildBeArmRow/OnBEArmClick/
//      UpdateBEArmVisuals/FlashBeFired; replaced OnPendingBeFiredDispatch target.
//  10. Modified BuildUI() to wrap rows in _contentPanel; adds BufferedButtonsRow at [4.0].
//  11. Modified DispatchShortcut Key.T/Key.F to pass GetRefPrice() and buffer.
//  12. Added _isCollapsed, _collapseToggleBtn, _contentPanel (T2 fields, declared here for T2).
//      NOTE: _contentPanel referenced by T2 (BuildCollapsibleHeader/OnCollapseClick) -- T2 fields
//      are declared here so that T1 can wrap rows in _contentPanel.
// PTT-COPIER-B11-T1 -- TradeCopierPanel.cs
// ChartTrader row injection surface. UserControl embedded in ChartTrader Grid (B7).
// Zero order creation. All order flow through CopyEngine.
// Jane Street rules: JS-001, JS-021, JS-023 -- no lock, Dispatcher.InvokeAsync only.
//
// B7 CHANGES:
//   1. Followers ListBox + ScrollViewer + "Followers" label REMOVED.
//   2. Replaced with a checkmark ComboBox (_followersDropDown).
//      Row layout (left to right): account name | daily P&L | checkmark.
//      Header shows "N selected" live count.
//      Design Pillar "Live Map": label is always the live state, never a prompt.
//   3. FollowerItem nested class: Account + IsSelected + live DailyPnlText/DailyPnlColor.
//      INotifyPropertyChanged drives P&L TextBlock binding -- no polling, no timer.
//      P&L color: green (+), red (-), dim ($0) per Live Map pillar Layer 2.
//   4. GetSelectedFollowers() iterates _followerItems -- no ListBox.SelectedItems.
//   5. Leader ComboBox absent by design -- ChartTrader Account IS the leader (B7-FIX6).
//   6. acc.AccountItemUpdate fires live P&L push from NT8 (AccountItem.RealizedProfitLoss).
//      Subscribed in OnLoaded, unsubscribed in Detach(). Dispatcher.InvokeAsync on callback.
//   7. B7-F1: Semantic button color coding (Layer 2 + Layer 3 live state via PositionStateChanged).
//      V08: canonical RGB per PTT_DESIGN_PILLAR. MakeBrush(r,g,b) -- no hex literals.
//
// B8 T1 CHANGES:
//   1. FollowerItem: added Multiplier int property (default 1, range [1,10]).
//   2. BuildCheckItemTemplate: added [mult TextBox w=30] before [checkmark].
//   3. OnFollowerMultiplierChanged: new handler -- parses int, clamps [1,10], sets item.Multiplier.
//   4. OnApplyRule: collects multipliers[] from _followerItems; calls engine.AddRule 5-arg overload.
//   5. ParseAtmModeNameLocal: pre-declared for T2; T1 passes ImmutableDictionary.Empty via atmMap.
//
// B8 T2 CHANGES:
//   1. FollowerItem: added AtmModeName string property (default "Inherit").
//   2. BuildCheckItemTemplate: added [ATM ComboBox w=80] before [checkmark].
//   3. OnFollowerAtmComboLoaded: new handler -- populates items synchronously.
//   4. OnFollowerAtmModeChanged: new handler -- sets item.AtmModeName.
//   5. OnApplyRule: collects AtmModeName per follower; builds ImmutableDictionary<string, FollowerAtmMode>.
//
// B9 T2 CHANGES:
//   1. Added _clickArmed, _clickBuy volatile bool fields (JS-023).
//   2. Added _currentChart, _armBtn, _buyToggle, _sellToggle UI fields.
//   3. SetChart(): stores chart reference for click trader arm/disarm.
//   4. BuildClickTraderRow(): appends [Buy] [Sell] [Arm] row to root StackPanel.
//   5. OnArmClick: toggles _clickArmed, calls RegisterClickTrader/UnregisterClickTrader.
//   6. UpdateArmVisuals: updates Arm button label + background color (MakeBrush, no hex).
//   7. OnChartMouseDown: fires limit order on chart click when armed (CYC=4).
//   8. OnBuyToggleClick / OnSellToggleClick: set _clickBuy volatile flag.
//   9. Detach(): unregisters click trader on panel teardown.
//
// B11 T1 CHANGES:
//   1. SetStatusText(): internal helper for SIM101 diag text display.
//   2. OnChartKeyDown(): PreviewKeyDown handler for Ctrl+Shift shortcut dispatch.
//   3. DispatchShortcut(): switch-based dispatch to engine methods (T/F/C/B).
//   4. Removed BuildDiagRow, OnDiagGap001d, OnDiagGap002 (DW-B10-01 CLOSED).
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;

namespace PropTraderTools
{
    public class TradeCopierPanel : UserControl
    {
        // -- state ----------------------------------------------------------------
        private CopyEngine  _engine;
        private Instrument  _instrument;
        private Account     _leaderAccount;   // Set by TradeCopierAddOn from ChartTrader.Account
        private ComboBox    _accountCombo;    // B30-B: stored at WireAccountCombo for Detach unsubscribe
        private EventHandler _accountComboSelectionChanged;  // B30-B: named handler for leak-free Detach
        private TextBlock   _statusText;
        private bool        _copyEnabled;
        private TextBox     _beBufferBox;

        // Checkmark dropdown
        private ComboBox                   _followersDropDown;
        private readonly List<FollowerItem> _followerItems = new List<FollowerItem>();

        // B9 T2 -- Click trader (JS-023: volatile cross-thread fields)
        private volatile bool    _clickArmed  = false;
        private volatile bool    _clickBuy    = true;    // true=Buy, false=SellShort
        private          Chart   _currentChart = null;   // single-writer UI thread
        private          Button        _armBtn     = null;
        private          ToggleButton  _buyToggle  = null;
        private          ToggleButton  _sellToggle = null;

        // B9 T3 -- Copy mode selector radio buttons
        private RadioButton _signalModeBtn = null;
        private RadioButton _mirrorModeBtn = null;

        // B10 T3 -- Tighten Stop fields (UI-thread-only)
        private Button  _tightenBtn      = null;
        private TextBox _tightenTicksBox = null;

        // B12 T1 -- Buffered button state (plain int; UI-thread-only; no volatile per NT8-003)
        private int  _trimBuffer     = 1;
        private int  _flattenBuffer  = 1;
        private int  _beBuffer       = 1;

        // B12 T1 -- BE 3-state FSM (UI-thread-only; no volatile)
        private BeState _beState = BeState.Idle;

        // B12 T1 -- Button refs for buffered section
        private Button  _trimBtn2;
        private Button  _flattenBtn2;
        private Button  _beBtn2;
        private Button  _cancelBtn2;
        private Button  _copyToggleBtn2;

        // B12 T2 -- Collapse state and refs (plain bool; UI-thread-only; no volatile per NT8-003)
        private bool       _isCollapsed        = false;
        private Button     _collapseToggleBtn;
        private StackPanel _contentPanel;

        // B12 T3 -- Risk/ATR spinners (plain double; UI-thread-only; no volatile per NT8-003)
        private double  _maxRiskDollars = 200.0;
        private double  _atrFraction    = 0.75;
        private TextBox _riskDollarsBox;
        private TextBox _atrFractionBox;

        // B20-LANE-C T5 -- ATR display label (owned by Panel; set in BuildRiskAtrRow; nulled on GC after purge)
        private TextBlock _atrDisplayLabel;

        // B12 T1 -- Frozen semantic brush for BE CONNECTED border (MakeBrush = Freeze()d, JS-008)
        // RGB (59, 130, 246) = blue. No hex string literal (JS-008).
        private static readonly SolidColorBrush BrushConnected = MakeBrush(59, 130, 246);

        // -- frozen semantic brushes (JS-008: MakeBrush calls Freeze()) --
        private static SolidColorBrush MakeBrush(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        // Canonical semantic button brushes (V08: corrected RGB per PTT_DESIGN_PILLAR lines 192-198)
        // JS-008: all Freeze()d via MakeBrush(), static readonly = zero allocation on re-render
        private static readonly SolidColorBrush BrushActive   = MakeBrush( 34, 197,  94);  // green  #22c55e
        private static readonly SolidColorBrush BrushDanger   = MakeBrush(239,  68,  68);  // red    #ef4444
        private static readonly SolidColorBrush BrushCaution  = MakeBrush(245, 158,  11);  // amber  #f59e0b
        private static readonly SolidColorBrush BrushInactive = MakeBrush( 55,  65,  81);  // grey   #4b5563

        // -- nested type ----------------------------------------------------------
        private sealed class FollowerItem : INotifyPropertyChanged
        {
            // Frozen brush constants (JS-008) -- shared across all instances
            private static readonly SolidColorBrush BrushPos = MakeBrush(34,  197, 94);  // green
            private static readonly SolidColorBrush BrushNeg = MakeBrush(239, 68,  68);  // red
            private static readonly SolidColorBrush BrushDim = MakeBrush(107, 114, 128); // grey

            // Cached PropertyChangedEventArgs -- zero alloc per fire
            private static readonly PropertyChangedEventArgs PnlTextArgs  =
                new PropertyChangedEventArgs(nameof(DailyPnlText));
            private static readonly PropertyChangedEventArgs PnlColorArgs =
                new PropertyChangedEventArgs(nameof(DailyPnlColor));

            public Account Account    { get; set; }
            public bool    IsSelected { get; set; }

            // B8 T1: per-follower quantity multiplier -- default 1x, range [1,10]
            public int Multiplier { get; set; } = 1;

            // B8 T2: per-follower ATM mode name -- default "Inherit"
            public string AtmModeName { get; set; } = "Inherit";

            private string _dailyPnlText  = "$0.00";
            private Brush  _dailyPnlColor;  // set in constructor

            public FollowerItem()
            {
                _dailyPnlColor = BrushDim;  // dim until first AccountItemUpdate fires
            }

            public string DailyPnlText
            {
                get => _dailyPnlText;
                private set
                {
                    _dailyPnlText = value;
                    PropertyChanged?.Invoke(this, PnlTextArgs);
                }
            }

            public Brush DailyPnlColor
            {
                get => _dailyPnlColor;
                private set
                {
                    _dailyPnlColor = value;
                    PropertyChanged?.Invoke(this, PnlColorArgs);
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            // Called on UI thread via Dispatcher.InvokeAsync -- no lock needed
            public void UpdatePnl(double value)
            {
                string sign  = value > 0 ? "+" : "";
                DailyPnlText  = sign + "$" + value.ToString("0.00");
                DailyPnlColor = value > 0 ? BrushPos : value < 0 ? BrushNeg : (Brush)BrushDim;
            }

            // B20-LANE-C T3 -- DW-B17-ACCOUNT-NAME-01: strip !<suffix> at display layer only.
            // Raw Account.Name is never modified. ?[0] guards null propagation when Account or Name
            // is null. Split("!")[0] without ?[0] is UNSAFE (NullReferenceException). CYC=1.
            public override string ToString() => Account?.Name?.Split('!')?[0] ?? "";
        }

        // B12 T1 -- BE 3-state FSM enum. UI-thread-only; no volatile backing needed.
        private enum BeState
        {
            Idle,       // BE button shows "BE +N" -- inactive
            Armed,      // After first click; engine.ArmPendingBe called; amber border
            Connected   // After engine fires pending BE; blue border; live repricing active
        }

        // -- construction ---------------------------------------------------------
        public TradeCopierPanel()
        {
            _engine = CopyEngine.Instance;
            _engine.StatusUpdate += OnStatusUpdate;
            BuildUI();
            // Defer Account.All population -- NT8 may not have it ready at construct time
            Loaded += OnLoaded;
        }

        // -- public surface (called by TradeCopierAddOn) --------------------------

        // B9 T2: Store chart reference for click trader. CYC=1 (straight-line).
        public void SetChart(Chart chart)
        {
            _currentChart = chart;
        }

        // B17 T2: Linear interpolation via ChartPanel.MaxValue / MinValue / ActualHeight.
        // B17 fix: FindPriceCanvasPanel replaces FindVisualChild<ChartPanel> (DFS first-match
        // returned ChartTrader sidebar: Width~139, MaxValue=0 -> rawPrice=0 -> no order placed).
        // FindPriceCanvasPanel selects widest ChartPanel with MaxValue>0 = price canvas.
        // CORRECTION_FACTOR = 1.0 (B16 T1 confirmed ContentPresenter.ActualHeight = ChartPanel.ActualHeight).
        // NT8-029 replacement: RoundToTickSize absent -- AlignToTick via Math.Round AwayFromZero.
        // CYC=5: cc null(1), panel null(2), height<=0(3), raw<=0(4), instrument null(5).
        private static double GetPriceAtY(ChartControl cc, double y, Instrument instrument)
        {
            if (cc == null) return 0.0;                                        // guard (1)

            var panel = FindPriceCanvasPanel(cc);    // B17 T2: heuristic selects widest ChartPanel with MaxValue>0
            if (panel == null) return 0.0;                                     // guard (2)

            double panelH = panel.ActualHeight;
            if (panelH <= 0.0) return 0.0;                                     // guard (3): no divide by zero

            // CORRECTION_FACTOR = 1.0: T1 confirmed ContentPresenter fills full ChartPanel height.
            const double CORRECTION_FACTOR = 1.0;

            double maxVal   = panel.MaxValue;
            double minVal   = panel.MinValue;
            double yRatio   = y / (panelH * CORRECTION_FACTOR);
            double rawPrice = maxVal - yRatio * (maxVal - minVal);

            if (rawPrice <= 0.0) return 0.0;                                   // guard (4): sanity

            if (instrument == null) return 0.0;                                // guard (5)
            return AlignToTick(rawPrice, instrument.MasterInstrument.TickSize);
        }

        // B16 T2: Pure-math linear Y-to-price interpolation helper.
        // Internal static for xUnit test access via Reflection.
        // Formula: rawPrice = maxVal - (y / (panelH * correctionFactor)) * (maxVal - minVal)
        // CYC=2: height guard(1), raw guard(2).
        internal static double LinearYToPrice(
            double y, double panelH, double maxVal, double minVal, double correctionFactor)
        {
            if (panelH <= 0.0) return 0.0;                                     // guard (1)
            double yRatio   = y / (panelH * correctionFactor);
            double rawPrice = maxVal - yRatio * (maxVal - minVal);
            if (rawPrice <= 0.0) return 0.0;                                   // guard (2)
            return rawPrice;
        }

        // B16 T2: Pure-math tick alignment helper.
        // Mirrors NT8-native RoundToTickSize semantics via Math.Round AwayFromZero.
        // Internal static for xUnit test access via Reflection.
        // CYC=2: tickSize guard(1), straight-line(2).
        internal static double AlignToTick(double raw, double tickSize)
        {
            if (tickSize <= 0.0) return raw;                                    // guard (1)
            return Math.Round(raw / tickSize, MidpointRounding.AwayFromZero) * tickSize;
        }

        // B17 T2 Option A: Walk full visual tree under root; return the ChartPanel with
        // MaxValue > 0 and largest ActualWidth. Reliably selects the price canvas panel
        // rather than the ChartTrader sidebar (Width~139, MaxValue=0 -- DFS first-match victim).
        // T1 F5 confirmed: only one ChartPanel exists (W=931.33, Max=7633.34) -- returns it directly.
        // CYC=5: root null(1), while loop(2), type+predicate(3), for loop(4), child null(5).
        private static ChartPanel FindPriceCanvasPanel(DependencyObject root)
        {
            if (root == null) return null;                                 // guard (1)
            ChartPanel best  = null;
            double     bestW = 0.0;
            var        stack = new Stack<DependencyObject>();
            stack.Push(root);

            while (stack.Count > 0)                                        // branch (2): loop
            {
                var node = stack.Pop();
                var cp = node as ChartPanel;
                if (cp != null && cp.MaxValue > 0 && cp.ActualWidth > bestW)  // branch (3): predicate
                {
                    best  = cp;
                    bestW = cp.ActualWidth;
                }
                int n = VisualTreeHelper.GetChildrenCount(node);
                for (int i = 0; i < n; i++)                                // branch (4): child loop
                {
                    var child = VisualTreeHelper.GetChild(node, i) as DependencyObject;
                    if (child != null) stack.Push(child);                  // branch (5): null guard
                }
            }
            return best;
        }


        public void SetInstrument(Instrument instrument)
        {
            _instrument = instrument;
            if (_statusText != null && instrument != null)
                _statusText.Text = "Ready: " + instrument.FullName + " -- select followers to copy";
        }

        public void SetLeaderAccount(Account account)
        {
            _leaderAccount = account;
        }

        // B30-B: WireAccountCombo -- called by TradeCopierAddOn.WireLeaderAccount instead of
        // anonymous lambda. Stores the ComboBox ref and named handler so Detach() can unsubscribe.
        // Fixes memory leak DW-B30-03: anonymous lambda captured panel, preventing GC.
        // CYC=1 (straight-line assignment + subscription, no branches). JS-021: no lock.
        public void WireAccountCombo(ComboBox combo)
        {
            _accountCombo = combo;
            _accountComboSelectionChanged = (s, e) =>
                _leaderAccount = _accountCombo.SelectedItem as NinjaTrader.Cbi.Account;
            combo.SelectionChanged += _accountComboSelectionChanged;
        }

        // B30-B: TryResolveLeaderAccount -- late-resolve the leader account when _leaderAccount
        // was null at inject time (ComboBox not yet populated). Uses stored _accountCombo ref.
        // CYC=2: null-conditional combo check(1), pattern-match Account cast(2).
        // JS-002: returns null (not throw) -- callers use null as a no-op sentinel.
        private NinjaTrader.Cbi.Account TryResolveLeaderAccount()
        {
            if (_accountCombo?.SelectedItem is NinjaTrader.Cbi.Account acc) return acc;
            return null;
        }

        public void Detach()
        {
            // B9 T2: unregister click trader before clearing state
            if (_currentChart != null)
                TradeCopierAddOn.UnregisterClickTrader(_currentChart);
            _engine.StatusUpdate              -= OnStatusUpdate;
            _engine.PositionStateChanged      -= OnPositionStateChanged;
            _engine.PendingBeFired            -= OnPendingBeFiredDispatch;
            foreach (var item in _followerItems)
                if (item.Account != null)
                    item.Account.AccountItemUpdate -= OnAccountItemUpdate;
            _engine.DisarmPendingBe(_leaderAccount);
            _engine.DisarmTrailBe(_leaderAccount);   // B14 T1
            _engine.CopyEnabledChanged -= OnCopyEnabledChanged;
            // B30-B: unsubscribe ComboBox SelectionChanged to prevent memory leak (DW-B30-03).
            if (_accountCombo != null && _accountComboSelectionChanged != null)
                _accountCombo.SelectionChanged -= _accountComboSelectionChanged;
            _accountCombo = null;
            _accountComboSelectionChanged = null;
            _instrument    = null;
            _leaderAccount = null;
        }

        // -- Layer 3 live state (V04) -- called on UI thread only -----------------
        // B12 T1: updated to use new _copyToggleBtn2, _flattenBtn2, _cancelBtn2, _trimBtn2, _beBtn2.
        // CYC=5: 5 ternary branches, no control flow.
        private void UpdateButtonColors(bool hasPosition, bool hasEntries)
        {
            if (_copyToggleBtn2 != null) _copyToggleBtn2.Background = _copyEnabled ? BrushActive   : BrushInactive;
            if (_flattenBtn2    != null) _flattenBtn2.Background    = hasPosition  ? BrushDanger   : BrushInactive;
            if (_cancelBtn2     != null) _cancelBtn2.Background     = hasEntries   ? BrushDanger   : BrushInactive;
            if (_trimBtn2       != null) _trimBtn2.Background       = hasPosition  ? BrushCaution  : BrushInactive;
            if (_beBtn2         != null && _beState == BeState.Idle) _beBtn2.Background = hasPosition ? BrushActive : BrushInactive;
        }

        // CYC=1: single null+instrument filter guard.
        // JS-023: marshals onto UI thread via Dispatcher.InvokeAsync.
        // JS-003: PositionState is a readonly struct -- captured by value in closure.
        private void OnPositionStateChanged(string instr, PositionState state)
        {
            if (_instrument == null || _instrument.FullName != instr) return;
            Dispatcher.InvokeAsync(() => UpdateButtonColors(state.HasOpenPosition, state.HasWorkingEntries));
        }

        // -- private: deferred account population ---------------------------------
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            _engine.PositionStateChanged += OnPositionStateChanged;
            _engine.PendingBeFired       += OnPendingBeFiredDispatch;
            _followerItems.Clear();
            if (Account.All == null) return;
            foreach (var acc in Account.All)
            {
                _followerItems.Add(new FollowerItem { Account = acc, IsSelected = false });
                acc.AccountItemUpdate += OnAccountItemUpdate;
            }
            if (_followersDropDown != null)
                _followersDropDown.ItemsSource = _followerItems;
            UpdateDropDownHeader();
            // B13 T2: push initial panel values to AtrSizingEngine at startup.
            // CopyEngine.UpdateAtrFraction / UpdateMaxRisk are null-guarded;
            // if _atrEngine is null (not yet attached) they are silent no-ops.
            NotifyRiskChanged();
            NotifyAtrFractionChanged();
            _engine.CopyEnabledChanged += OnCopyEnabledChanged;
        }

        // -- live P&L push from NT8 -----------------------------------------------
        // Fires on background thread -- must Dispatcher.InvokeAsync before touching UI/items
        private void OnAccountItemUpdate(object sender, AccountItemEventArgs e)
        {
            if (e.AccountItem != AccountItem.RealizedProfitLoss) return;
            var acc = sender as Account;
            if (acc == null) return;
            double val = e.Value;
            foreach (var item in _followerItems)
            {
                if (item.Account != acc) continue;
                Dispatcher.InvokeAsync(() => item.UpdatePnl(val));
                break;
            }
        }

        // -- UI construction -------------------------------------------------------
        // B12 T1: restructured -- rows wrapped in _contentPanel; buffered buttons at [4.0];
        //         old 4-column actionGrid and dead toggle buttons removed.
        private void BuildUI()
        {
            var root = new StackPanel { Margin = new Thickness(2) };

            // --- Followers checkmark dropdown (stays above _contentPanel) ---
            _followersDropDown = new ComboBox
            {
                Margin     = new Thickness(0, 0, 0, 2),
                IsEditable = false,
                Text       = "0 selected"
            };
            _followersDropDown.ItemTemplate = BuildCheckItemTemplate();
            root.Children.Add(_followersDropDown);

            // --- Apply Rule button ---
            var applyBtn = new Button { Content = "Add Followers", Margin = new Thickness(0, 2, 0, 2) };
            applyBtn.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            applyBtn.Click += OnApplyRule;
            root.Children.Add(applyBtn);

            // --- Separator ---
            var sep = new Border { Height = 1, Margin = new Thickness(0, 2, 0, 2) };
            sep.SetResourceReference(Border.BorderBrushProperty, "NTBrushes.BorderBrush");
            sep.BorderThickness = new Thickness(0, 1, 0, 0);
            root.Children.Add(sep);

            // B12 T2: Collapse header row (above _contentPanel; always visible)
            BuildCollapsibleHeader(root);

            // B12 T1/T2: _contentPanel wraps all collapsible content rows
            _contentPanel = new StackPanel();

            // [4.0] B12 T1: Buffered button section (Trim | Flatten | Cancel | BE | Copy toggle)
            BuildBufferedButtonsRow(_contentPanel);

            // --- Status line ---
            _statusText = new TextBlock { Text = "Open chart -- Trim/Flatten/Cancel/BE ready", Margin = new Thickness(0, 2, 0, 0) };
            _statusText.SetResourceReference(TextBlock.ForegroundProperty, "NTBrushes.SubtleBrush");
            _contentPanel.Children.Add(_statusText);

            // B9 T2: Click Trader row
            BuildClickTraderRow(_contentPanel);

            // B9 T3: Copy mode row (Signal / Mirror radio buttons)
            BuildModeRow(_contentPanel);

            // B10 T3: Tighten Stop cluster (button + ticks TextBox)
            var tightenRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin      = new Thickness(0, 4, 0, 0)
            };
            _tightenTicksBox = new TextBox
            {
                Text  = "5",
                Width = 30,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            _tightenBtn = new Button
            {
                Content    = "Tighten",
                Margin     = new Thickness(0, 0, 4, 0),
                Background = BrushInactive
            };
            _tightenBtn.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            _tightenBtn.Click += OnTightenStop;
            var tightenLabel = new TextBlock
            {
                Text              = "tks",
                VerticalAlignment = VerticalAlignment.Center,
                Margin            = new Thickness(2, 0, 0, 0)
            };
            tightenLabel.SetResourceReference(TextBlock.ForegroundProperty, "NTBrushes.SubtleBrush");
            tightenRow.Children.Add(_tightenBtn);
            tightenRow.Children.Add(_tightenTicksBox);
            tightenRow.Children.Add(tightenLabel);
            _contentPanel.Children.Add(tightenRow);

            // B12 T3: Risk $ + ATR % spinner row (last row in _contentPanel)
            BuildRiskAtrRow(_contentPanel);

            root.Children.Add(_contentPanel);
            Content = root;

            // V04: ensure consistent initial state
            UpdateButtonColors(false, false);
        }

        // B9 T2: Appends [Buy] [Sell] toggle pair and [Arm] button row to root StackPanel.
        // CYC=1 (straight-line widget construction, no branches).
        private void BuildClickTraderRow(StackPanel root)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin      = new Thickness(0, 4, 0, 0)
            };

            _buyToggle = new ToggleButton
            {
                Content   = "Buy",
                IsChecked = true,
                Width     = 45,
                Height    = 22
            };
            _buyToggle.SetResourceReference(Control.StyleProperty, "NTToggleButtonStyle");

            _sellToggle = new ToggleButton
            {
                Content = "Sell",
                Width   = 45,
                Height  = 22
            };
            _sellToggle.SetResourceReference(Control.StyleProperty, "NTToggleButtonStyle");

            _buyToggle.Click  += OnBuyToggleClick;
            _sellToggle.Click += OnSellToggleClick;

            _armBtn = new Button
            {
                Content    = "Arm",
                Width      = 48,
                Height     = 22,
                Margin     = new Thickness(6, 0, 0, 0),
                Background = MakeBrush(28, 33, 51)
            };
            _armBtn.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            _armBtn.Click += OnArmClick;

            row.Children.Add(_buyToggle);
            row.Children.Add(_sellToggle);
            row.Children.Add(_armBtn);
            root.Children.Add(row);
        }

        // B12 T1 -- OnPendingBeFiredDispatch: marshals PendingBeFired from NT8 account bg thread to UI.
        // B12 T1: replaced FlashBeFired call with OnBeConnected call.
        // CYC=1: straight-line Dispatcher.InvokeAsync, no branches.
        // Called on NT8 account background thread -- never touch UI directly here.
        private void OnPendingBeFiredDispatch(string instr, string accountName)
        {
            Dispatcher.InvokeAsync(() => OnBeConnected(instr, accountName));
        }

        // B12 T1 -- BuildBufferedButtonsRow: builds 3-row buffered button section inside _contentPanel.
        // CYC=1: straight-line construction, no branches.
        // Row 1: Trim cluster | Flatten cluster
        // Row 2: Cancel | BE cluster
        // Row 3: Copy toggle (full width)
        private void BuildBufferedButtonsRow(StackPanel root)
        {
            // Row 1: Trim | Flatten
            var row1 = new UniformGrid { Columns = 2, Margin = new Thickness(0, 2, 0, 2) };

            // Col 0: Trim cluster
            var trimCluster = new DockPanel { LastChildFill = true };
            var trimArrows = new Grid();
            trimArrows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            trimArrows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            var trimUp = new System.Windows.Controls.Primitives.RepeatButton { Content = "\u25B2", Width = 18, Height = 12 };
            var trimDn = new System.Windows.Controls.Primitives.RepeatButton { Content = "\u25BC", Width = 18, Height = 12 };
            trimUp.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            trimDn.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            trimUp.Click += OnTrimUp;
            trimDn.Click += OnTrimDown;
            Grid.SetRow(trimUp, 0);
            Grid.SetRow(trimDn, 1);
            trimArrows.Children.Add(trimUp);
            trimArrows.Children.Add(trimDn);
            DockPanel.SetDock(trimArrows, Dock.Right);
            _trimBtn2 = new Button { Content = FormatBuffer("Trim", _trimBuffer), Background = BrushInactive };
            _trimBtn2.Click += OnTrimClick;
            trimCluster.Children.Add(trimArrows);
            trimCluster.Children.Add(_trimBtn2);
            row1.Children.Add(trimCluster);

            // Col 1: Flatten cluster
            var flatCluster = new DockPanel { LastChildFill = true };
            var flatArrows = new Grid();
            flatArrows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            flatArrows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            var flatUp = new System.Windows.Controls.Primitives.RepeatButton { Content = "\u25B2", Width = 18, Height = 12 };
            var flatDn = new System.Windows.Controls.Primitives.RepeatButton { Content = "\u25BC", Width = 18, Height = 12 };
            flatUp.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            flatDn.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            flatUp.Click += OnFlattenUp;
            flatDn.Click += OnFlattenDown;
            Grid.SetRow(flatUp, 0);
            Grid.SetRow(flatDn, 1);
            flatArrows.Children.Add(flatUp);
            flatArrows.Children.Add(flatDn);
            DockPanel.SetDock(flatArrows, Dock.Right);
            _flattenBtn2 = new Button { Content = FormatBuffer("Flatten", _flattenBuffer), Background = BrushInactive };
            _flattenBtn2.Click += OnFlattenClick;
            flatCluster.Children.Add(flatArrows);
            flatCluster.Children.Add(_flattenBtn2);
            row1.Children.Add(flatCluster);

            root.Children.Add(row1);

            // Row 2: Cancel | BE cluster
            var row2 = new UniformGrid { Columns = 2, Margin = new Thickness(0, 2, 0, 2) };

            // Col 0: Cancel
            _cancelBtn2 = new Button { Content = "Cancel", Background = BrushInactive };
            _cancelBtn2.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            _cancelBtn2.Click += OnCancel2;
            row2.Children.Add(_cancelBtn2);

            // Col 1: BE cluster
            var beCluster = new DockPanel { LastChildFill = true };
            var beArrows = new Grid();
            beArrows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            beArrows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            var beUp = new System.Windows.Controls.Primitives.RepeatButton { Content = "\u25B2", Width = 18, Height = 12 };
            var beDn = new System.Windows.Controls.Primitives.RepeatButton { Content = "\u25BC", Width = 18, Height = 12 };
            beUp.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            beDn.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            beUp.Click += OnBeUp;
            beDn.Click += OnBeDown;
            Grid.SetRow(beUp, 0);
            Grid.SetRow(beDn, 1);
            beArrows.Children.Add(beUp);
            beArrows.Children.Add(beDn);
            DockPanel.SetDock(beArrows, Dock.Right);
            _beBtn2 = new Button { Content = FormatBuffer("BE", _beBuffer), Background = BrushInactive };
            _beBtn2.Click += OnBeClick;
            beCluster.Children.Add(beArrows);
            beCluster.Children.Add(_beBtn2);
            row2.Children.Add(beCluster);

            root.Children.Add(row2);

            // Row 3: Copy toggle (full width)
            _copyToggleBtn2 = new Button
            {
                Content    = "\u25CF COPY OFF",
                Background = BrushInactive,
                Margin     = new Thickness(0, 2, 0, 2)
            };
            _copyToggleBtn2.Click += OnCopyToggle;
            root.Children.Add(_copyToggleBtn2);
        }

        // B12 T1 -- FormatBuffer: formats buffer label for display on a button. CYC=1. Static, no state.
        // Example: FormatBuffer("Trim", 1) -> "Trim +1"
        private static string FormatBuffer(string name, int ticks)
        {
            return name + " +" + ticks;
        }

        // B12 T1 -- OnTrimUp: increment _trimBuffer, clamp, update label. CYC=1.
        private void OnTrimUp(object sender, RoutedEventArgs e)
        {
            _trimBuffer = Math.Max(Math.Min(_trimBuffer + 1, 20), 0);   // no Math.Clamp (NT8 .NET 4.8)
            if (_trimBtn2 != null) _trimBtn2.Content = FormatBuffer("Trim", _trimBuffer);
        }

        // B12 T1 -- OnTrimDown: decrement _trimBuffer, clamp, update label. CYC=1.
        private void OnTrimDown(object sender, RoutedEventArgs e)
        {
            _trimBuffer = Math.Max(Math.Min(_trimBuffer - 1, 20), 0);
            if (_trimBtn2 != null) _trimBtn2.Content = FormatBuffer("Trim", _trimBuffer);
        }

        // B19 T1 -- OnTrimClick: calls engine Trim overload with ask+bid anchors or market fallback. CYC=4.
        // B30-B: leader resolved late via _leaderAccount ?? TryResolveLeaderAccount() (DW-B30-03).
        private void OnTrimClick(object sender, RoutedEventArgs e)
        {
            if (_instrument == null) return;                                               // (1)
            var leader = _leaderAccount ?? TryResolveLeaderAccount();                      // B30-B
            double ask = GetAsk();
            double bid = GetBid();
            if (ask <= 0 || bid <= 0 || _trimBuffer == 0)                                 // (2)(3)
                _engine.Trim(leader, _instrument);
            else                                                                           // (4)
                _engine.Trim(leader, _instrument, _trimBuffer, ask, bid);
        }

        // B12 T1 -- OnFlattenUp: increment _flattenBuffer, clamp, update label. CYC=1.
        private void OnFlattenUp(object sender, RoutedEventArgs e)
        {
            _flattenBuffer = Math.Max(Math.Min(_flattenBuffer + 1, 20), 0);
            if (_flattenBtn2 != null) _flattenBtn2.Content = FormatBuffer("Flatten", _flattenBuffer);
        }

        // B12 T1 -- OnFlattenDown: decrement _flattenBuffer, clamp, update label. CYC=1.
        private void OnFlattenDown(object sender, RoutedEventArgs e)
        {
            _flattenBuffer = Math.Max(Math.Min(_flattenBuffer - 1, 20), 0);
            if (_flattenBtn2 != null) _flattenBtn2.Content = FormatBuffer("Flatten", _flattenBuffer);
        }

        // B19 T1 -- OnFlattenClick: calls engine Flatten overload with ask+bid anchors or market fallback. CYC=4.
        // B30-B: leader resolved late via _leaderAccount ?? TryResolveLeaderAccount() (DW-B30-03).
        private void OnFlattenClick(object sender, RoutedEventArgs e)
        {
            if (_instrument == null) return;                                               // (1)
            var leader = _leaderAccount ?? TryResolveLeaderAccount();                      // B30-B
            double ask = GetAsk();
            double bid = GetBid();
            if (ask <= 0 || bid <= 0 || _flattenBuffer == 0)                              // (2)(3)
                _engine.Flatten(leader, _instrument);
            else                                                                           // (4)
                _engine.Flatten(leader, _instrument, _flattenBuffer, ask, bid);
        }

        // B12 T1 -- OnBeUp: increment _beBuffer, clamp, live reprice if Connected. CYC=2.
        private void OnBeUp(object sender, RoutedEventArgs e)
        {
            _beBuffer = Math.Max(Math.Min(_beBuffer + 1, 20), 0);       // no Math.Clamp
            UpdateBeLabel();
            if (_beState == BeState.Connected && _instrument != null)   // (2)
                _engine.BreakEven(_leaderAccount, _instrument, _beBuffer);
        }

        // B12 T1 -- OnBeDown: decrement _beBuffer, clamp, live reprice if Connected. CYC=2.
        private void OnBeDown(object sender, RoutedEventArgs e)
        {
            _beBuffer = Math.Max(Math.Min(_beBuffer - 1, 20), 0);
            UpdateBeLabel();
            if (_beState == BeState.Connected && _instrument != null)
                _engine.BreakEven(_leaderAccount, _instrument, _beBuffer);
        }

        // B12 T1 -- OnBeClick: 3-state FSM transition. CYC=5.
        // B30-B: leader resolved late via _leaderAccount ?? TryResolveLeaderAccount() (DW-B30-03).
        private void OnBeClick(object sender, RoutedEventArgs e)
        {
            if (_instrument == null) return;                                               // (1)
            var leader = _leaderAccount ?? TryResolveLeaderAccount();                      // B30-B
            if (leader == null) return;                                                    // (2)
            switch (_beState)
            {
                case BeState.Idle:                // (3)
                    _engine.ArmPendingBe(_instrument, leader, _beBuffer);
                    _beState = BeState.Armed;
                    UpdateBeVisuals(BeState.Armed);
                    break;
                case BeState.Armed:               // (4)
                    _engine.DisarmPendingBe(leader);
                    _beState = BeState.Idle;
                    UpdateBeVisuals(BeState.Idle);
                    break;
                case BeState.Connected:           // (5)
                    _engine.DisarmPendingBe(leader);
                    _engine.DisarmTrailBe(leader);          // B14 T1 -- disarm continuous trail
                    _beState = BeState.Idle;
                    UpdateBeVisuals(BeState.Idle);
                    break;
            }
        }

        // B12 T1 -- UpdateBeLabel: sets _beBtn2 label. CYC=1.
        private void UpdateBeLabel()
        {
            if (_beBtn2 != null) _beBtn2.Content = FormatBuffer("BE", _beBuffer);
        }

        // B12 T1 -- UpdateBeVisuals: sets BE button border and content per state. CYC=3.
        private void UpdateBeVisuals(BeState state)
        {
            if (_beBtn2 == null) return;
            switch (state)
            {
                case BeState.Idle:                                                    // (1)
                    _beBtn2.Content    = FormatBuffer("BE", _beBuffer);
                    _beBtn2.Background = BrushInactive;
                    break;
                case BeState.Armed:                                                   // (2)
                    _beBtn2.Content    = "BE Armed";
                    _beBtn2.Background = BrushCaution;
                    break;
                case BeState.Connected:                                               // (3)
                    _beBtn2.Content    = "BE Live";
                    _beBtn2.Background = BrushConnected;
                    break;
            }
        }

        // B14 T1 -- extended: arm continuous trail watcher after initial BE placement.
        // CYC=3: _beBtn2 null(1), _instrument null(2), _leaderAccount null(3-inline with _instrument check).
        private void OnBeConnected(string instr, string accountName)
        {
            if (_beBtn2 == null) return;                                              // (1)
            if (_leaderAccount == null || _leaderAccount.Name != accountName) return;
            // DW-B26-02: only update state for the panel whose account fired BE
            _beState = BeState.Connected;                                             // (2)
            UpdateBeVisuals(BeState.Connected);
            if (_instrument != null)
            {
                _engine.BreakEven(_leaderAccount, _instrument, _beBuffer);
                if (_leaderAccount != null)
                    _engine.ArmTrailBe(_instrument, _leaderAccount, _beBuffer);      // B14 T1
            }
        }

        // B19 T1 -- GetAsk: returns current ask price from _instrument.MarketData.Ask.Price.
        // NT8-032: MarketData.Ask is MarketDataEventArgs; .Price is the double value.
        // Replaces GetRefPrice() (which used md.Last.Price -- wrong anchor). CYC=4.
        private double GetAsk()
        {
            if (_instrument == null) return 0.0;                   // (1) guard
            var md = _instrument.MarketData;
            if (md == null)   return 0.0;                          // (2) guard
            var ask = md.Ask;
            if (ask == null)  return 0.0;                          // (3) guard
            return ask.Price;                                      // (4) double
        }

        // B19 T1 -- GetBid: returns current bid price from _instrument.MarketData.Bid.Price.
        // NT8-032: MarketData.Bid is MarketDataEventArgs; .Price is the double value.
        // Mirrors GetAsk() null-guard chain exactly. CYC=4.
        private double GetBid()
        {
            if (_instrument == null) return 0.0;                   // (1) guard
            var md = _instrument.MarketData;
            if (md == null)   return 0.0;                          // (2) guard
            var bid = md.Bid;
            if (bid == null)  return 0.0;                          // (3) guard
            return bid.Price;                                      // (4) double
        }

        // B12 T1 -- OnCopyToggle: toggles _copyEnabled. CYC=2.
        private void OnCopyToggle(object sender, RoutedEventArgs e)
        {
            _copyEnabled = !_copyEnabled;                                             // (1)
            _engine.SetEnabled(_copyEnabled);
            if (_copyToggleBtn2 == null) return;
            _copyToggleBtn2.Content    = _copyEnabled ? "\u25CF COPY ON" : "\u25CF COPY OFF";  // (2)
            _copyToggleBtn2.Background = _copyEnabled ? BrushActive : BrushInactive;
        }

        // B20-LANE-C T3 -- OnCopyEnabledChanged: syncs Panel copy state from engine event.
        // CYC=2: null guard (1) + Dispatcher.InvokeAsync UI update (2).
        // JS-021: no lock. JS-023: Dispatcher.InvokeAsync for UI thread marshaling.
        private void OnCopyEnabledChanged(bool enabled)
        {
            _copyEnabled = enabled;
            if (_copyToggleBtn2 == null) return;
            Dispatcher.InvokeAsync(() =>
            {
                _copyToggleBtn2.Content    = enabled ? "\u25CF COPY ON" : "\u25CF COPY OFF";
                _copyToggleBtn2.Background = enabled ? BrushActive : BrushInactive;
            });
        }

        // B12 T1 -- OnCancel2: cancels pending entries. CYC=2.
        // B30-B: leader resolved late via _leaderAccount ?? TryResolveLeaderAccount() (DW-B30-03).
        private void OnCancel2(object sender, RoutedEventArgs e)
        {
            if (_instrument == null) return;                                               // (1)
            var leader = _leaderAccount ?? TryResolveLeaderAccount();                      // B30-B
            if (leader != null) _engine.CancelPendingEntries(leader, _instrument);        // (2)
        }

        // B12 T2 -- BuildCollapsibleHeader: builds collapse header row. CYC=1.
        private void BuildCollapsibleHeader(StackPanel root)
        {
            _collapseToggleBtn = new Button
            {
                Content = "\u25BC Position Tools",
                Margin  = new Thickness(0, 0, 0, 2)
            };
            _collapseToggleBtn.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            _collapseToggleBtn.Click += OnCollapseClick;
            root.Children.Add(_collapseToggleBtn);
        }

        // B12 T2 -- OnCollapseClick: toggles _isCollapsed and sets _contentPanel.Visibility. CYC=2.
        private void OnCollapseClick(object sender, RoutedEventArgs e)
        {
            _isCollapsed = !_isCollapsed;                                              // (1)
            if (_contentPanel != null)                                                 // (2)
                _contentPanel.Visibility = _isCollapsed ? Visibility.Collapsed : Visibility.Visible;
            if (_collapseToggleBtn != null)
                _collapseToggleBtn.Content = _isCollapsed ? "\u25B2 Position Tools" : "\u25BC Position Tools";
        }

        // B10 T3 -- OnTightenStop: tighten stop button click handler.
        // CYC=4: instrument null(1), parse fallback(2), leader null branch(3), engine overload(4).
        // B30-B: uses leader overload when leader is available; falls back to all-accounts overload.
        // NT8-034: no Math.Clamp (.NET 4.8 version constraint -- not the NT8-003 volatile ban).
        // JS-021: no lock -- _engine.TightenStop iterates ConcurrentBag (lock-free).
        private void OnTightenStop(object sender, RoutedEventArgs e)
        {
            if (_instrument == null)                               // (1)
                return;
            var leader = _leaderAccount ?? TryResolveLeaderAccount();  // B30-B: late resolve
            int ticks = int.TryParse(_tightenTicksBox?.Text, out var t)  // (2)
                ? Math.Max(1, Math.Min(500, t))   // clamp 1-500: no Math.Clamp (.NET 4.8 ban)
                : 5;
            if (leader != null)                                    // (3)
                _engine.TightenStop(leader, _instrument, ticks);   // B30-A leader overload (4)
            else
                _engine.TightenStop(_instrument, ticks);           // fallback: all accounts
        }



        // B9 T3: Appends "Mode: [Signal] [Mirror]" radio button row to root StackPanel.
        // CYC=1 (straight-line widget construction, no branches).
        private void BuildModeRow(StackPanel root)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin      = new Thickness(0, 4, 0, 0)
            };
            var lbl = new Label
            {
                Content           = "Mode:",
                Width             = 42,
                VerticalAlignment = VerticalAlignment.Center
            };
            _signalModeBtn = new RadioButton
            {
                Content           = "Signal",
                IsChecked         = true,
                Margin            = new Thickness(4, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            _mirrorModeBtn = new RadioButton
            {
                Content           = "Mirror",
                Margin            = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            _signalModeBtn.Click += OnSignalModeClick;
            _mirrorModeBtn.Click += OnMirrorModeClick;
            row.Children.Add(lbl);
            row.Children.Add(_signalModeBtn);
            row.Children.Add(_mirrorModeBtn);
            root.Children.Add(row);
        }

        // B9 T3: CYC=1 -- straight-line engine call
        private void OnSignalModeClick(object sender, RoutedEventArgs e)
        {
            CopyEngine.Instance.SetCopyMode(CopyMode.Signal);
        }

        // B9 T3: CYC=1 -- straight-line engine call
        private void OnMirrorModeClick(object sender, RoutedEventArgs e)
        {
            CopyEngine.Instance.SetCopyMode(CopyMode.Mirror);
        }

        // B8 T1+T2: Row layout (left to right):
        //   [account name] [daily P&L] [mult TextBox w=30] [ATM ComboBox w=80] [checkmark]
        // P&L text color: green(+) / red(-) / dim($0) per Live Map pillar Layer 2.
        // Binding: DailyPnlText + DailyPnlColor update via INotifyPropertyChanged on FollowerItem.
        // B10-UI-01: Row factory uses Grid (not StackPanel) so all 6 columns align
        // vertically across rows regardless of account name length.
        // ColumnDefinitions added at runtime via OnRowGridLoaded (WPF FEF limitation).
        // CYC=1 (no branches -- pure factory construction).
        private DataTemplate BuildCheckItemTemplate()
        {
            var template = new DataTemplate(typeof(FollowerItem));

            var gridFactory = new FrameworkElementFactory(typeof(Grid));
            gridFactory.AddHandler(FrameworkElement.LoadedEvent,
                new RoutedEventHandler(OnRowGridLoaded));

            // [1] Account name -- Col 0: star width, ellipsis trimming
            var nameFactory = new FrameworkElementFactory(typeof(TextBlock));
            nameFactory.SetValue(Grid.ColumnProperty, 0);
            nameFactory.SetBinding(TextBlock.TextProperty, new Binding("Account.Name"));
            nameFactory.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
            nameFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);

            // [2] Daily P&L -- Col 1: 62px fixed, right-aligned, color-coded
            var pnlFactory = new FrameworkElementFactory(typeof(TextBlock));
            pnlFactory.SetValue(Grid.ColumnProperty, 1);
            pnlFactory.SetBinding(TextBlock.TextProperty,       new Binding("DailyPnlText"));
            pnlFactory.SetBinding(TextBlock.ForegroundProperty, new Binding("DailyPnlColor"));
            pnlFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
            pnlFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);

            // [3] B8 T1: Multiplier TextBox -- Col 2: 30px fixed
            // Fires on WPF UI thread -- no Dispatcher needed (JS-023 compliant)
            var multFactory = new FrameworkElementFactory(typeof(TextBox));
            multFactory.SetValue(Grid.ColumnProperty, 2);
            multFactory.SetValue(TextBox.TextProperty, "1");
            multFactory.SetValue(TextBox.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            multFactory.AddHandler(TextBox.TextChangedEvent,
                new TextChangedEventHandler(OnFollowerMultiplierChanged));

            // [4] B8 T2 + B9 T3: ATM Mode ComboBox -- Col 3: 80px fixed
            // ItemsSource populated in OnFollowerAtmComboLoaded to avoid DataTemplate timing issues.
            // B9 T3: OnFollowerAtmModeChanged_WithNamedBox handles AtmModeName update AND namedBox visibility.
            var atmFactory = new FrameworkElementFactory(typeof(ComboBox));
            atmFactory.SetValue(Grid.ColumnProperty, 3);
            atmFactory.AddHandler(ComboBox.LoadedEvent,
                new RoutedEventHandler(OnFollowerAtmComboLoaded));
            atmFactory.AddHandler(ComboBox.SelectionChangedEvent,
                new SelectionChangedEventHandler(OnFollowerAtmModeChanged_WithNamedBox));

            // [5] B9 T3: Named ATM inline TextBox -- Col 4: 80px fixed, hidden until "Named" selected
            var namedBoxFactory = new FrameworkElementFactory(typeof(TextBox));
            namedBoxFactory.SetValue(Grid.ColumnProperty, 4);
            namedBoxFactory.SetValue(TextBox.VisibilityProperty, Visibility.Collapsed);
            namedBoxFactory.SetValue(TextBox.ToolTipProperty, "ATM template name");

            // [6] Checkmark -- Col 5: 20px fixed, centered
            var chkFactory = new FrameworkElementFactory(typeof(CheckBox));
            chkFactory.SetValue(Grid.ColumnProperty, 5);
            chkFactory.SetBinding(CheckBox.IsCheckedProperty,
                new Binding("IsSelected") { Mode = BindingMode.TwoWay });
            chkFactory.AddHandler(CheckBox.ClickEvent, new RoutedEventHandler(OnFollowerChecked));
            chkFactory.SetValue(CheckBox.VerticalAlignmentProperty, VerticalAlignment.Center);
            chkFactory.SetValue(CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center);

            gridFactory.AppendChild(nameFactory);
            gridFactory.AppendChild(pnlFactory);
            gridFactory.AppendChild(multFactory);
            gridFactory.AppendChild(atmFactory);
            gridFactory.AppendChild(namedBoxFactory);
            gridFactory.AppendChild(chkFactory);
            template.VisualTree = gridFactory;
            return template;
        }

        // B10-UI-01: Loaded handler for Grid rows materialized from BuildCheckItemTemplate.
        // Adds 6 ColumnDefinitions with exact widths from the column spec.
        // Tag=true guard prevents re-entry on re-layout (CYC branch 2).
        // CYC=2: type+null guard (branch 1) + already-configured guard (branch 2).
        private void OnRowGridLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Grid grid) return;               // branch 1: type + null guard
            if (grid.Tag is bool) return;                      // branch 2: already-configured guard
            grid.Tag = true;

            grid.ColumnDefinitions.Add(new ColumnDefinition
                { Width = new GridLength(1, GridUnitType.Star), MinWidth = 80 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(62) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
        }

        // B8 T1: handler for multiplier TextBox text change.
        // Fires on WPF UI thread. Parses int, clamps [1,10], sets item.Multiplier.
        // CYC=3 (sender null guard + parse guard + clamp). No Dispatcher needed.
        private void OnFollowerMultiplierChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null) return;
            var item = tb.DataContext as FollowerItem;
            if (item == null) return;
            if (!int.TryParse(tb.Text, out int parsed)) return;
            item.Multiplier = parsed < 1 ? 1 : (parsed > 10 ? 10 : parsed);
        }

        // B8 T2: ATM ComboBox Loaded handler -- populates items synchronously.
        // Fires on WPF UI thread. Sets ItemsSource = {"Inherit","Market","Named"}, SelectedIndex=0.
        // CYC=1 (null guard only).
        private void OnFollowerAtmComboLoaded(object sender, RoutedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb == null) return;
            cb.ItemsSource   = new[] { "Inherit", "Market", "Named" };
            cb.SelectedIndex = 0;
        }

        // B8 T2: ATM ComboBox selection change handler.
        // Fires on WPF UI thread. Sets item.AtmModeName to the selected string.
        // CYC=3 (cb null guard + item null guard + no-selection guard).
        private void OnFollowerAtmModeChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb == null) return;
            var item = cb.DataContext as FollowerItem;
            if (item == null) return;
            var selected = cb.SelectedItem as string;
            if (selected == null) return;
            item.AtmModeName = selected;
        }

        // B9 T3: ATM ComboBox SelectionChanged handler with Named ATM inline TextBox show/hide.
        // Replaces plain OnFollowerAtmModeChanged for namedBoxFactory-wired rows.
        // CYC=4 (cb null guard + panel null guard + namedBox null guard + "Named" branch).
        private void OnFollowerAtmModeChanged_WithNamedBox(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb == null) return;                                 // guard (1)
            var item = cb.DataContext as FollowerItem;
            if (item == null) return;                               // guard (2)
            var selected = cb.SelectedItem as string ?? string.Empty;
            item.AtmModeName = selected;

            // Find sibling TextBox (namedBox) in the same Grid row (B10-UI-01: StackPanel -> Grid)
            var grid = cb.Parent as Grid;
            if (grid == null) return;                               // guard (3)
            TextBox namedBox = null;
            foreach (var child in grid.Children)
            {
                if (child is TextBox tb && tb.ToolTip?.ToString() == "ATM template name")
                {
                    namedBox = tb;
                    break;
                }
            }
            if (namedBox == null) return;                           // guard (4)
            namedBox.Visibility = selected == "Named"
                ? Visibility.Visible : Visibility.Collapsed;       // branch (4)
            if (selected != "Named")
                namedBox.Text = string.Empty;
        }

        // -- B9 T2: Click trader event handlers ------------------------------------

        // CYC=1 -- straight-line volatile write
        private void OnBuyToggleClick(object sender, RoutedEventArgs e)
        {
            _clickBuy           = true;
            _sellToggle.IsChecked = false;
        }

        // CYC=1 -- straight-line volatile write
        private void OnSellToggleClick(object sender, RoutedEventArgs e)
        {
            _clickBuy          = false;
            _buyToggle.IsChecked = false;
        }

        // CYC=2 -- null guard (1) + _clickArmed branch (2)
        private void OnArmClick(object sender, RoutedEventArgs e)
        {
            if (_currentChart == null) return;          // guard (1)
            _clickArmed = !_clickArmed;                 // volatile toggle
            if (_clickArmed)                            // branch (2)
                TradeCopierAddOn.RegisterClickTrader(_currentChart, this);
            else
                TradeCopierAddOn.UnregisterClickTrader(_currentChart);
            UpdateArmVisuals(_clickArmed);
        }

        // CYC=2 -- null guard (1) + armed branch (2)
        // Called on UI thread from OnArmClick -- no Dispatcher needed.
        private void UpdateArmVisuals(bool armed)
        {
            if (_armBtn == null) return;                // guard (1)
            _armBtn.Content    = armed ? "Disarm" : "Arm";      // branch (2)
            _armBtn.Background = armed
                ? MakeBrush(34, 197, 94)    // green -- decimal RGB, no hex (JS-008)
                : MakeBrush(28, 33, 51);    // dark surface color
        }

        // CYC=6 -- five guards + ternary; try/catch does NOT add CYC.
        // B17 T2: FindPriceCanvasPanel selects price canvas (MaxValue>0, widest panel).
        // B17 Amendment: PreviewMouseDown wired in TradeCopierAddOn (tunnel phase -- NT8 suppresses MouseDown).
        // F5 confirmed 2026-07-15: order placed at exact Y-pixel price (7491.00). GetPriceAtY correct.
        // JS-023: _clickArmed / _clickBuy are volatile reads (no lock needed).
        // NT8 constraint: "PTT-Click" signal name starts with "PTT-".
        internal void OnChartMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_clickArmed)           return;         // guard (1)
            if (_leaderAccount == null) return;         // guard (2)
            if (_instrument    == null) return;         // guard (3)
            var chartControl = sender as ChartControl;
            if (chartControl   == null) return;         // guard (4)

            Point  mousePos  = e.GetPosition(chartControl);
            double rawPrice  = GetPriceAtY(chartControl, mousePos.Y, _instrument);
            if (rawPrice <= 0.0) return;                                 // guard (5): no valid price
            double tickSize  = _instrument.MasterInstrument.TickSize;
            double price     = Math.Round(rawPrice / tickSize) * tickSize;
            bool   isBuy     = _clickBuy;                  // volatile read
            int    qty       = CopyEngine.Instance.GetSuggestedQty(_instrument);
            var    action    = isBuy ? OrderAction.Buy : OrderAction.SellShort;

            try
            {
                _leaderAccount.CreateOrder(
                    _instrument, action,
                    OrderType.Limit,
                    OrderEntry.Manual,
                    TimeInForce.Day,
                    qty, price, 0, null,
                    "PTT-Click",
                    DateTime.MaxValue,
                    (NinjaTrader.Cbi.CustomOrder)null);
            }
            catch (Exception ex)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (_statusText != null)
                        _statusText.Text = "PTT-Click error: " + ex.Message;
                });
            }
        }

        // -- event handlers --------------------------------------------------------
        private void OnFollowerChecked(object sender, RoutedEventArgs e)
        {
            UpdateDropDownHeader();
        }

        private void UpdateDropDownHeader()
        {
            int count = 0;
            foreach (var item in _followerItems)
                if (item.IsSelected) count++;
            if (_followersDropDown != null)
                _followersDropDown.Text = count + " selected";
        }

        private void OnTrim(object sender, RoutedEventArgs e)
        {
            if (_instrument != null) _engine.Trim(_leaderAccount, _instrument);
        }

        private void OnFlatten(object sender, RoutedEventArgs e)
        {
            if (_instrument != null) _engine.Flatten(_leaderAccount, _instrument);
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            if (_instrument != null) _engine.CancelPendingEntries(_leaderAccount, _instrument);
        }

        private Account[] GetSelectedFollowers()
        {
            var list = new List<Account>();
            foreach (var item in _followerItems)
                if (item.IsSelected && item.Account != null) list.Add(item.Account);
            return list.ToArray();
        }

        // B8 T1+T2: OnApplyRule -- collects multipliers[] and ATM modes per follower; calls 5-arg AddRule.
        private void OnApplyRule(object sender, RoutedEventArgs e)
        {
            if (_leaderAccount == null)
            {
                if (_statusText != null) _statusText.Text = "No leader -- select account in ChartTrader.";
                return;
            }
            if (_instrument == null)
            {
                if (_statusText != null) _statusText.Text = "No instrument -- open a chart first.";
                return;
            }
            var followers = GetSelectedFollowers();
            if (followers.Length == 0)
            {
                if (_statusText != null) _statusText.Text = "Select follower account(s).";
                return;
            }

            // B8 T1+T2: collect per-follower multipliers and ATM mode names parallel to followers array
            var multipliers = new int[followers.Length];
            var atmNames    = new string[followers.Length];
            for (int i = 0; i < followers.Length; i++)
            {
                foreach (var item in _followerItems)
                {
                    if (item.Account != followers[i]) continue;
                    multipliers[i] = item.Multiplier > 0 ? item.Multiplier : 1;
                    atmNames[i]    = item.AtmModeName ?? "Inherit";
                    break;
                }
            }

            // B8 T2: build Dictionary<string, FollowerAtmMode> from collected ATM names
            var atmMap = new Dictionary<string, FollowerAtmMode>();
            for (int i = 0; i < followers.Length; i++)
            {
                if (followers[i] != null)
                    atmMap[followers[i].Name] = ParseAtmModeNameLocal(atmNames[i]);
            }

            _engine.AddRule(_instrument.FullName, _leaderAccount, followers, multipliers, atmMap);
            if (_statusText != null)
                _statusText.Text = "Rule: " + _instrument.FullName + " leader=" + _leaderAccount.Name;
        }

        // B8 T2: ParseAtmModeNameLocal -- private static helper that mirrors CopyEngine.ParseAtmModeName.
        // Keeps Panel self-contained without exposing engine internals. CYC=3.
        private static FollowerAtmMode ParseAtmModeNameLocal(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new FollowerAtmMode.Inherit();
            if (name == "Market")
                return new FollowerAtmMode.Market();
            if (name.StartsWith("Named:"))
                return new FollowerAtmMode.Named(name.Substring(6));
            return new FollowerAtmMode.Inherit();
        }

        private void OnStatusUpdate(string line)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (_statusText != null) _statusText.Text = line;
            });
        }

        // B11 T1: SIM101 temporary status text helper.
        // Called from TradeCopierAddOn.OnChartKeyDiag via Dispatcher.InvokeAsync.
        // Sets _statusText.Text directly on the UI thread.
        // CYC=1: null guard only.
        internal void SetStatusText(string text)
        {
            if (_statusText == null) return;
            _statusText.Text = text;
        }

        // B11 T1: chart.PreviewKeyDown handler wired by TradeCopierAddOn.HookKeyShortcut().
        // Fires on WPF UI thread -- no Dispatcher needed.
        // CYC=3: instrument null guard (1), modifier guard (2), delegate to DispatchShortcut (3).
        // Jane Street: guard-early, zero branches in the hot dispatch path.
        internal void OnChartKeyDown(object sender, KeyEventArgs e)
        {
            if (_instrument == null) return;                   // guard (1)
            if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift))
                != (ModifierKeys.Control | ModifierKeys.Shift)) return;  // guard (2)
            DispatchShortcut(e.Key);                           // guard (3): delegate
        }

        // B11 T1: Jane Street switch preferred over if/else chain.
        // Cases: T=Trim, F=Flatten, C=CancelPendingEntries, B=BreakEven.
        // B19 T1 -- DispatchShortcut: keyboard shortcuts dispatch to engine methods.
        // Calls EXISTING CopyEngine public methods -- no new CopyEngine code added.
        // CYC=5: switch entry (1) + 4 case arms (2,3,4,5).
        // BE path reads _beBufferBox.Text for buffer ticks (UI-thread-safe; PreviewKeyDown is on UI thread).
        // Key.T: Trim limit @ ask + buffer*tick (long) or bid - buffer*tick (short). Falls back to market on zero ask/bid.
        // Key.F: Flatten limit @ ask + buffer*tick (long) or bid - buffer*tick (short). Same fallback.
        private void DispatchShortcut(Key key)
        {
            switch (key)
            {
                case Key.T: _engine.Trim(_leaderAccount, _instrument, _trimBuffer, GetAsk(), GetBid());       break;
                case Key.F: _engine.Flatten(_leaderAccount, _instrument, _flattenBuffer, GetAsk(), GetBid()); break;
                case Key.C: _engine.CancelPendingEntries(_leaderAccount, _instrument);               break;
                case Key.B:
                    int buf = 2;
                    int.TryParse(_beBufferBox.Text, out buf);
                    _engine.BreakEven(_leaderAccount, _instrument, buf);
                    break;
            }
        }

        // B12 T3 -- BuildRiskAtrRow: builds Risk $ + ATR % spinner row. CYC=1: straight-line construction.
        // Called from BuildUI() at end of _contentPanel.
        private void BuildRiskAtrRow(StackPanel root)
        {
            var grid = new UniformGrid { Columns = 2, Margin = new Thickness(0, 4, 0, 0) };

            // Col 0 -- Risk $ spinner
            var col0 = new StackPanel { Orientation = Orientation.Horizontal };
            var riskLabel = new TextBlock
            {
                Text              = "Risk $",
                VerticalAlignment = VerticalAlignment.Center,
                Margin            = new Thickness(0, 0, 4, 0)
            };
            riskLabel.SetResourceReference(TextBlock.ForegroundProperty, "NTBrushes.SubtleBrush");
            _riskDollarsBox = new TextBox
            {
                Text  = _maxRiskDollars.ToString("F0"),
                Width = 55,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            _riskDollarsBox.SetResourceReference(Control.StyleProperty, "NTTextBoxStyle");
            _riskDollarsBox.LostFocus += OnRiskTextLostFocus;
            var riskArrows = new Grid();
            riskArrows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            riskArrows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            var riskUp = new System.Windows.Controls.Primitives.RepeatButton { Content = "\u25B2", Height = 12 };
            var riskDn = new System.Windows.Controls.Primitives.RepeatButton { Content = "\u25BC", Height = 12 };
            riskUp.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            riskDn.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            riskUp.Click += OnRiskUp;
            riskDn.Click += OnRiskDown;
            Grid.SetRow(riskUp, 0);
            Grid.SetRow(riskDn, 1);
            riskArrows.Children.Add(riskUp);
            riskArrows.Children.Add(riskDn);
            col0.Children.Add(riskLabel);
            col0.Children.Add(_riskDollarsBox);
            col0.Children.Add(riskArrows);
            grid.Children.Add(col0);

            // Col 1 -- ATR % spinner
            var col1 = new StackPanel { Orientation = Orientation.Horizontal };
            var atrLabel = new TextBlock
            {
                Text              = "ATR %",
                VerticalAlignment = VerticalAlignment.Center,
                Margin            = new Thickness(0, 0, 4, 0)
            };
            atrLabel.SetResourceReference(TextBlock.ForegroundProperty, "NTBrushes.SubtleBrush");
            _atrFractionBox = new TextBox
            {
                Text  = _atrFraction.ToString("F2"),
                Width = 55,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            _atrFractionBox.SetResourceReference(Control.StyleProperty, "NTTextBoxStyle");
            _atrFractionBox.LostFocus += OnAtrFractionTextLostFocus;
            var atrArrows = new Grid();
            atrArrows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            atrArrows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12) });
            var atrUp = new System.Windows.Controls.Primitives.RepeatButton { Content = "\u25B2", Height = 12 };
            var atrDn = new System.Windows.Controls.Primitives.RepeatButton { Content = "\u25BC", Height = 12 };
            atrUp.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            atrDn.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            atrUp.Click += OnAtrFractionUp;
            atrDn.Click += OnAtrFractionDown;
            Grid.SetRow(atrUp, 0);
            Grid.SetRow(atrDn, 1);
            atrArrows.Children.Add(atrUp);
            atrArrows.Children.Add(atrDn);
            col1.Children.Add(atrLabel);
            col1.Children.Add(_atrFractionBox);
            col1.Children.Add(atrArrows);
            grid.Children.Add(col1);

            root.Children.Add(grid);

            var atrRow = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(2),
                Padding         = new Thickness(4, 2, 4, 2),
                Margin          = new Thickness(2)
            };
            _atrDisplayLabel = new TextBlock { Text = "ATR=-.-- pts -> stopTicks=-- -> qty=--" };
            atrRow.Child = _atrDisplayLabel;
            root.Children.Add(atrRow);
        }

        // B20-LANE-C T5 -- SetAtrText: updates ATR display label from UpdateAtrOverlay via Dispatcher.InvokeAsync.
        // CYC=2: null guard (1) + Text assignment (2). Runs on UI thread only (caller uses InvokeAsync).
        // JS-021: no lock. Caller (TradeCopierAddOn.UpdateAtrOverlay) dispatches to UI thread before calling.
        public void SetAtrText(string display)
        {
            if (_atrDisplayLabel == null) return;
            _atrDisplayLabel.Text = display;
        }

        // B12 T3 -- OnRiskUp: increment _maxRiskDollars, clamp, push. CYC=1.
        private void OnRiskUp(object sender, RoutedEventArgs e)
        {
            _maxRiskDollars = Math.Max(Math.Min(_maxRiskDollars + 25.0, 1000.0), 10.0);  // no Math.Clamp (NT8 .NET 4.8)
            if (_riskDollarsBox != null) _riskDollarsBox.Text = _maxRiskDollars.ToString("F0");
            NotifyRiskChanged();
        }

        // B12 T3 -- OnRiskDown: decrement _maxRiskDollars, clamp, push. CYC=1.
        private void OnRiskDown(object sender, RoutedEventArgs e)
        {
            _maxRiskDollars = Math.Max(Math.Min(_maxRiskDollars - 25.0, 1000.0), 10.0);  // no Math.Clamp
            if (_riskDollarsBox != null) _riskDollarsBox.Text = _maxRiskDollars.ToString("F0");
            NotifyRiskChanged();
        }

        // B12 T3 -- OnRiskTextLostFocus: parse + clamp + push. CYC=3.
        private void OnRiskTextLostFocus(object sender, RoutedEventArgs e)
        {
            double v;
            if (!double.TryParse(_riskDollarsBox?.Text, out v)) return;              // (1) parse guard
            v = Math.Max(Math.Min(v, 1000.0), 10.0);                                 // (2) clamp
            _maxRiskDollars = v;
            if (_riskDollarsBox != null) _riskDollarsBox.Text = v.ToString("F0");   // normalise display
            NotifyRiskChanged();                                                      // (3) push
        }

        // B12 T3 -- OnAtrFractionUp: increment _atrFraction, clamp, push. CYC=1.
        private void OnAtrFractionUp(object sender, RoutedEventArgs e)
        {
            _atrFraction = Math.Max(Math.Min(_atrFraction + 0.05, 3.00), 0.25);     // no Math.Clamp
            if (_atrFractionBox != null) _atrFractionBox.Text = _atrFraction.ToString("F2");
            NotifyAtrFractionChanged();
        }

        // B12 T3 -- OnAtrFractionDown: decrement _atrFraction, clamp, push. CYC=1.
        private void OnAtrFractionDown(object sender, RoutedEventArgs e)
        {
            _atrFraction = Math.Max(Math.Min(_atrFraction - 0.05, 3.00), 0.25);     // no Math.Clamp
            if (_atrFractionBox != null) _atrFractionBox.Text = _atrFraction.ToString("F2");
            NotifyAtrFractionChanged();
        }

        // B12 T3 -- OnAtrFractionTextLostFocus: parse + clamp + push. CYC=3.
        private void OnAtrFractionTextLostFocus(object sender, RoutedEventArgs e)
        {
            double v;
            if (!double.TryParse(_atrFractionBox?.Text, out v)) return;             // (1) parse guard
            v = Math.Max(Math.Min(v, 3.00), 0.25);                                  // (2) clamp
            _atrFraction = v;
            if (_atrFractionBox != null) _atrFractionBox.Text = v.ToString("F2");  // normalise display
            NotifyAtrFractionChanged();                                              // (3) push
        }

        // B12 T3 -- NotifyRiskChanged: delegates to CopyEngine.UpdateMaxRisk. CYC=2.
        private void NotifyRiskChanged()
        {
            if (_engine == null) return;             // (1)
            _engine.UpdateMaxRisk(_maxRiskDollars);  // (2)
        }

        // B12 T3 -- NotifyAtrFractionChanged: delegates to CopyEngine.UpdateAtrFraction. CYC=2.
        private void NotifyAtrFractionChanged()
        {
            if (_engine == null) return;              // (1)
            _engine.UpdateAtrFraction(_atrFraction);  // (2)
        }
    }
}
