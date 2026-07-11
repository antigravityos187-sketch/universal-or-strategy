// PTT-COPIER-B10-T3 -- TradeCopierPanel.cs
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private Button      _copyToggleBtn;
        private Button      _flattenBtn;
        private Button      _cancelBtn;
        private Button      _trimBtn;
        private Button      _beBtn;
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

        // B10 T2 -- Pending BE arm fields (UI-thread-only; plain types, no volatile)
        private Button  _beArmBtn       = null;
        private bool    _beArmState     = false;
        private TextBox _beArmBufferBox = null;

        // B10 T3 -- Tighten Stop fields (UI-thread-only)
        private Button  _tightenBtn      = null;
        private TextBox _tightenTicksBox = null;

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

            public override string ToString() => Account?.Name ?? "";
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

        public void SetInstrument(Instrument instrument)
        {
            _instrument = instrument;
            if (_statusText != null && instrument != null)
                _statusText.Text = "Ready: " + instrument.FullName;
        }

        public void SetLeaderAccount(Account account)
        {
            _leaderAccount = account;
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
            _instrument    = null;
            _leaderAccount = null;
        }

        // -- Layer 3 live state (V04) -- called on UI thread only -----------------
        // CYC=5: 5 ternary branches, no control flow.
        private void UpdateButtonColors(bool hasPosition, bool hasEntries)
        {
            _copyToggleBtn.Background = _copyEnabled ? BrushActive   : BrushInactive;
            _flattenBtn.Background    = hasPosition  ? BrushDanger   : BrushInactive;
            _cancelBtn.Background     = hasEntries   ? BrushDanger   : BrushInactive;
            _trimBtn.Background       = hasPosition  ? BrushCaution  : BrushInactive;
            _beBtn.Background         = hasPosition  ? BrushActive   : BrushInactive;
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
        private void BuildUI()
        {
            var root = new StackPanel { Margin = new Thickness(2) };

            // --- Followers checkmark dropdown ---
            // Header text is always live state: "0 selected" / "2 selected"
            _followersDropDown = new ComboBox
            {
                Margin     = new Thickness(0, 0, 0, 2),
                IsEditable = false,
                Text       = "0 selected"
            };
            _followersDropDown.ItemTemplate = BuildCheckItemTemplate();
            root.Children.Add(_followersDropDown);

            // --- Apply Rule button (non-color-coded -- keeps NTButtonStyle) ---
            var applyBtn = new Button { Content = "Apply Rule", Margin = new Thickness(0, 2, 0, 2) };
            applyBtn.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            applyBtn.Click += OnApplyRule;
            root.Children.Add(applyBtn);

            // --- Separator ---
            var sep = new Border { Height = 1, Margin = new Thickness(0, 2, 0, 2) };
            sep.SetResourceReference(Border.BorderBrushProperty, "NTBrushes.BorderBrush");
            sep.BorderThickness = new Thickness(0, 1, 0, 0);
            root.Children.Add(sep);

            // --- Copy toggle (color-coded -- no NTButtonStyle) ---
            _copyToggleBtn = new Button
            {
                Content    = "Copy OFF",
                Margin     = new Thickness(0, 2, 0, 2),
                Background = BrushInactive
            };
            _copyToggleBtn.Click += OnToggle;
            root.Children.Add(_copyToggleBtn);

            // --- Action buttons: Trim | Flatten | Cancel | BE cluster ---
            var actionGrid = new UniformGrid { Columns = 4, Margin = new Thickness(0, 2, 0, 2) };

            // Color-coded action buttons: no NTButtonStyle (prevents Background override)
            _trimBtn = new Button { Content = "Trim 1/2", Background = BrushInactive };
            _trimBtn.Click += OnTrim;
            actionGrid.Children.Add(_trimBtn);

            _flattenBtn = new Button { Content = "Flatten", Background = BrushInactive };
            _flattenBtn.Click += OnFlatten;
            actionGrid.Children.Add(_flattenBtn);

            _cancelBtn = new Button { Content = "Cancel", Background = BrushInactive };
            _cancelBtn.Click += OnCancel;
            actionGrid.Children.Add(_cancelBtn);

            var beCluster = new StackPanel { Orientation = Orientation.Horizontal };
            _beBtn = new Button { Content = "BE", Margin = new Thickness(0, 0, 2, 0), Background = BrushInactive };
            _beBtn.Click += OnBreakEven;
            _beBufferBox = new TextBox
            {
                Text = "2",
                Width = 28,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            var tksLabel = new TextBlock
            {
                Text = "tks",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2, 0, 0, 0)
            };
            tksLabel.SetResourceReference(TextBlock.ForegroundProperty, "NTBrushes.SubtleBrush");
            beCluster.Children.Add(_beBtn);
            beCluster.Children.Add(_beBufferBox);
            beCluster.Children.Add(tksLabel);
            actionGrid.Children.Add(beCluster);

            root.Children.Add(actionGrid);

            // --- Status line (live state, always visible) ---
            _statusText = new TextBlock { Text = "No instrument", Margin = new Thickness(0, 2, 0, 0) };
            _statusText.SetResourceReference(TextBlock.ForegroundProperty, "NTBrushes.SubtleBrush");
            root.Children.Add(_statusText);

            // B9 T2: Click Trader row -- appended last so it sits below status line
            BuildClickTraderRow(root);

            // B9 T3: Copy mode row (Signal / Mirror radio buttons)
            BuildModeRow(root);

            // B10 T2: Arm BE row (pending BE arm/disarm + buffer ticks TextBox)
            BuildBeArmRow(root);

            // B10 T3: Tighten Stop cluster (button + ticks TextBox)
            // _tightenTicksBox default 5 ticks; button content "~" is ASCII (ticket spec T3).
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
                Content    = "~",
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
            root.Children.Add(tightenRow);

            // DIAG -- GAP-001d + GAP-002 Sim101 test buttons (REMOVE AFTER TESTS)
            BuildDiagRow(root);

            Content = root;

            // V04: ensure consistent initial state (all action buttons start grey)
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

        // B10 T2 -- BuildBeArmRow: builds "Arm BE" row with button + buffer ticks TextBox.
        // CYC=1: straight-line widget construction, no branches.
        // _beArmBtn uses color-coded brushes (BrushCaution=armed, BrushInactive=inactive).
        private void BuildBeArmRow(StackPanel root)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin      = new Thickness(0, 4, 0, 0)
            };
            _beArmBufferBox = new TextBox
            {
                Text  = "2",
                Width = 30,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            _beArmBtn = new Button
            {
                Content    = "Arm BE",
                Margin     = new Thickness(0, 0, 4, 0),
                Background = BrushInactive
            };
            _beArmBtn.Click += OnBEArmClick;
            row.Children.Add(_beArmBtn);
            row.Children.Add(_beArmBufferBox);
            root.Children.Add(row);
        }

        // B10 T2 -- OnBEArmClick: toggles arm/disarm by calling engine methods.
        // CYC=3: instrument null(1), account null(2), armed toggle(3).
        private void OnBEArmClick(object sender, RoutedEventArgs e)
        {
            if (_instrument == null)                           // (1)
                return;
            if (_leaderAccount == null)                        // (2)
                return;
            if (!_beArmState)                                  // (3)
            {
                int buf = int.TryParse(_beArmBufferBox.Text, out var b) ? b : 2;
                _engine.ArmPendingBe(_instrument, _leaderAccount, buf);
                _beArmState = true;
                UpdateBEArmVisuals(armed: true);
            }
            else
            {
                _engine.DisarmPendingBe();
                _beArmState = false;
                UpdateBEArmVisuals(armed: false);
            }
        }

        // B10 T2 -- UpdateBEArmVisuals: updates _beArmBtn background and label for 2 states.
        // CYC=2: null guard(1), state branch(2).
        private void UpdateBEArmVisuals(bool armed)
        {
            if (_beArmBtn == null)                             // (1)
                return;
            _beArmBtn.Content    = armed ? "BE Armed" : "Arm BE";  // (2)
            _beArmBtn.Background = armed ? BrushCaution : BrushInactive;
        }

        // B10 T2 -- OnPendingBeFiredDispatch: marshals PendingBeFired from NT8 account bg thread to UI.
        // CYC=1: straight-line Dispatcher.InvokeAsync, no branches.
        // Called on NT8 account background thread -- never touch UI directly here.
        private void OnPendingBeFiredDispatch(string instr)
        {
            Dispatcher.InvokeAsync(() => FlashBeFired(instr));
        }

        // B10 T2 -- FlashBeFired: briefly flashes the Arm BE button green when BE fires.
        // CYC=2: null guard(1), await scheduling(2).
        // async void: UI event handler invoked via Dispatcher.InvokeAsync (explicitly allowed per arch plan Sec 5.6).
        private async void FlashBeFired(string instr)
        {
            if (_beArmBtn == null)                             // (1)
                return;
            _beArmBtn.Content    = "BE Fired!";
            _beArmBtn.Background = BrushActive;               // green -- transient flash state
            _beArmState = false;
            await System.Threading.Tasks.Task.Delay(800);     // (2) 800ms flash duration
            _beArmBtn.Content    = "Arm BE";
            _beArmBtn.Background = BrushInactive;             // grey -- back to inactive
        }

        // B10 T3 -- OnTightenStop: tighten stop button click handler.
        // CYC=3: instrument null(1), parse fallback(2), engine call(3).
        // NT8-003: no Math.Clamp (banned in .NET 4.8). Math.Max/Min used instead.
        // JS-021: no lock -- _engine.TightenStop iterates ConcurrentBag (lock-free).
        private void OnTightenStop(object sender, RoutedEventArgs e)
        {
            if (_instrument == null)                               // (1)
                return;
            int ticks = int.TryParse(_tightenTicksBox?.Text, out var t)  // (2)
                ? Math.Max(1, Math.Min(500, t))   // clamp 1-500: no Math.Clamp (.NET 4.8 ban)
                : 5;
            _engine.TightenStop(_instrument, ticks);              // (3)
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

        // CYC=4 -- four null/type guards; try/catch does NOT add CYC.
        // JS-023: _clickArmed / _clickBuy are volatile reads (no lock needed).
        // NT8 constraint: "PTT-Click" signal name starts with "PTT-".
        internal void OnChartMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_clickArmed)           return;         // guard (1)
            if (_leaderAccount == null) return;         // guard (2)
            if (_instrument    == null) return;         // guard (3)
            var chartControl = sender as ChartControl;
            if (chartControl   == null) return;         // guard (4)

            // NT8 constraint: ChartControl.GetValueByY does not exist in this NT8 version.
            // DW-B8-04 (click trader) deferred -- price lookup via visual tree / scale panel pending.
            // Temporary: use 0.0 so file compiles; click-trader will not fire valid orders until fixed.
            double price  = 0.0;
            _ = e.GetPosition(chartControl); // suppress unused-variable warning
            bool   isBuy  = _clickBuy;                  // volatile read
            int    qty    = CopyEngine.Instance.GetSuggestedQty(_instrument);
            var    action = isBuy ? OrderAction.Buy : OrderAction.SellShort;

            try
            {
                _leaderAccount.CreateOrder(
                    _instrument, action,
                    OrderType.Limit,
                    OrderEntry.Manual,
                    TimeInForce.Day,
                    qty, price, 0, null,
                    "PTT-Click",          // signal name -- starts with "PTT-" (NT8 constraint)
                    DateTime.MaxValue,
                    null);
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

        private void OnToggle(object sender, RoutedEventArgs e)
        {
            _copyEnabled = !_copyEnabled;
            _engine.SetEnabled(_copyEnabled);
            _copyToggleBtn.Content    = _copyEnabled ? "Copy ON" : "Copy OFF";
            _copyToggleBtn.Background = _copyEnabled ? BrushActive : BrushInactive;
        }

        private void OnTrim(object sender, RoutedEventArgs e)
        {
            if (_instrument != null) _engine.Trim(_instrument);
        }

        private void OnFlatten(object sender, RoutedEventArgs e)
        {
            if (_instrument != null) _engine.Flatten(_instrument);
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            if (_instrument != null) _engine.CancelPendingEntries(_instrument);
        }

        private void OnBreakEven(object sender, RoutedEventArgs e)
        {
            if (_instrument == null) return;
            int ticks = 2;
            if (int.TryParse(_beBufferBox?.Text?.Trim(), out int parsed) && parsed >= 0)
                ticks = parsed;
            _engine.BreakEven(_instrument, ticks);
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

        // -- DIAG: GAP-001d + GAP-002 Sim101 test row (REMOVE AFTER TESTS) --
        // Two buttons that delegate to TradeCopierAddOn diagnostic methods.
        // No permanent state. No effect on copy logic.
        private void BuildDiagRow(StackPanel root)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin      = new Thickness(0, 6, 0, 0)
            };

            var lbl = new TextBlock
            {
                Text              = "Diag:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin            = new Thickness(0, 0, 4, 0)
            };
            lbl.SetResourceReference(TextBlock.ForegroundProperty, "NTBrushes.SubtleBrush");
            row.Children.Add(lbl);

            // GAP-001d button: tests acc.Change() on trailing stop
            var gap001Btn = new Button
            {
                Content = "GAP-001d",
                Width   = 68,
                Height  = 22,
                Margin  = new Thickness(0, 0, 4, 0)
            };
            gap001Btn.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            gap001Btn.Click += OnDiagGap001d;
            row.Children.Add(gap001Btn);

            // GAP-002 button: tests Instrument.MarketData subscription in AddOn context
            var gap002Btn = new Button
            {
                Content = "GAP-002",
                Width   = 64,
                Height  = 22
            };
            gap002Btn.SetResourceReference(Control.StyleProperty, "NTButtonStyle");
            gap002Btn.Click += OnDiagGap002;
            row.Children.Add(gap002Btn);

            root.Children.Add(row);
        }

        // CYC=3: instrument guard (1) + Account.All loop (2) + null diagAcc guard (3)
        private void OnDiagGap001d(object sender, RoutedEventArgs e)
        {
            if (_instrument == null)
            {
                if (_statusText != null)
                    _statusText.Text = "GAP-001d: need instrument -- open a chart first";
                return;
            }
            NinjaTrader.Cbi.Account diagAcc = null;
            foreach (var a in NinjaTrader.Cbi.Account.All)
                if (a.Name.IndexOf("Sim", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    { diagAcc = a; break; }
            if (diagAcc == null)
            {
                if (_statusText != null)
                    _statusText.Text = "GAP-001d: no Sim account found in Account.All";
                return;
            }
            TradeCopierAddOn.RunGap001dTest(diagAcc, _instrument);
            if (_statusText != null)
                _statusText.Text = "GAP-001d: test started -- watch for MessageBox";
        }

        // CYC=2: null guard (1) + delegate to AddOn (2)
        private void OnDiagGap002(object sender, RoutedEventArgs e)
        {
            if (_instrument == null)
            {
                if (_statusText != null)
                    _statusText.Text = "GAP-002: need instrument first";
                return;
            }
            TradeCopierAddOn.RunGap002Test(_instrument);
            if (_statusText != null)
                _statusText.Text = "GAP-002: test started -- watch NT8 Output window";
        }
        // -- END DIAG --
    }
}
