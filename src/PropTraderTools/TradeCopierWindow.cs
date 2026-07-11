// PTT-COPIER-B10-T3 -- TradeCopierWindow.cs
// Plain WPF Window Add-On surface. Rule management, status log, global on/off.
// FIX: Account.All removed from constructor/BuildUI -- only bound in Loaded handler.
// FIX: Shift+B KeyBinding removed -- WPF KeyGesture rejects Shift+letter in NT8 host.
// All order submission routes through CopyEngine. No order calls in this file.
// Jane Street rules: JS-021 (no lock), JS-023 (volatile via engine), SCAN-01..07
// B7-F1: Semantic button color coding (Layer 2 + Layer 3 live state via PositionStateChanged).
// B7-F5: ScrollViewer wrapping _rulesPanel (MaxHeight=400).
// V08: canonical RGB per PTT_DESIGN_PILLAR. MakeWinBrush(r,g,b) -- no hex literals.
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;

namespace PropTraderTools
{
    public class TradeCopierWindow : Window
    {
        private CopyEngine _engine;
        private Button _globalToggleBtn;
        private StackPanel _logPanel;
        private StackPanel _rulesPanel;
        private bool _copyEnabled;
        private const int MaxLogLines = 50;

        // All rule-row account controls collected here so Loaded can bind Account.All
        private readonly List<ComboBox>  _leaderBoxes   = new List<ComboBox>();
        private readonly List<ListBox>   _followerBoxes = new List<ListBox>();

        // Per-rule button tracking for UpdateButtonColors iteration (Engineer Note #3)
        // Precedent: _leaderBoxes / _followerBoxes (existing pattern in this file)
        // Accessed exclusively on UI thread -- no locking required (JS-021)
        private readonly List<Button> _flattenBtns  = new List<Button>();
        private readonly List<Button> _cancelBtns   = new List<Button>();
        private readonly List<Button> _trimBtns     = new List<Button>();
        private readonly List<Button> _beBtns       = new List<Button>();
        // B10 T3: tighten stop button tracking -- not position-state-colored; tracked for cleanup.
        private readonly List<Button> _tightenBtns  = new List<Button>();

        // -- frozen semantic brushes (JS-008: MakeWinBrush calls Freeze()) --------
        // "Win" prefix avoids collision with potential Window base-class members
        private static SolidColorBrush MakeWinBrush(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        // Canonical semantic brushes (V08: corrected RGB per PTT_DESIGN_PILLAR lines 192-198)
        private static readonly SolidColorBrush WBrushActive   = MakeWinBrush( 34, 197,  94);  // green  #22c55e
        private static readonly SolidColorBrush WBrushDanger   = MakeWinBrush(239,  68,  68);  // red    #ef4444
        private static readonly SolidColorBrush WBrushCaution  = MakeWinBrush(245, 158,  11);  // amber  #f59e0b
        private static readonly SolidColorBrush WBrushInactive = MakeWinBrush( 55,  65,  81);  // grey   #4b5563

        public TradeCopierWindow()
        {
            Title                 = "Trade Copier";
            Width                 = 720;
            Height                = 520;
            MinWidth              = 540;
            MinHeight             = 380;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode            = ResizeMode.CanResizeWithGrip;

            _engine = CopyEngine.Instance;

            try
            {
                BuildUI();
            }
            catch (Exception ex)
            {
                // BuildUI must never throw -- if it does surface it immediately
                MessageBox.Show("PTT BuildUI error:\n\n" + ex.Message + "\n\n" + ex.StackTrace, "Trade Copier");
                return;
            }

            Loaded += OnLoaded;
            Closed += OnWindowClosed;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Bind Account.All now -- NT8 guarantees accounts are populated by Loaded
            try
            {
                foreach (var cb in _leaderBoxes)
                    cb.ItemsSource = Account.All;
                foreach (var lb in _followerBoxes)
                    lb.ItemsSource = Account.All;
            }
            catch (Exception ex)
            {
                MessageBox.Show("PTT account bind error:\n\n" + ex.Message, "Trade Copier");
            }

            try
            {
                _engine.StatusUpdate          += OnStatusUpdate;
                _engine.PositionStateChanged  += OnPositionStateChanged;
                _engine.Subscribe();
                CopyEngine.Instance.LoadRules();
            }
            catch (Exception ex)
            {
                MessageBox.Show("PTT init error:\n\n" + ex.Message + "\n\n" + ex.StackTrace, "Trade Copier");
            }
        }

        // V04: unsubscribe PositionStateChanged on close to prevent ghost callbacks / memory leaks
        private void OnWindowClosed(object sender, EventArgs e)
        {
            _engine.PositionStateChanged -= OnPositionStateChanged;
        }

        protected override void OnClosed(EventArgs e)
        {
            try { CopyEngine.Instance.SaveRules(); } catch { }
            _engine.StatusUpdate -= OnStatusUpdate;
            _engine.Unsubscribe();
            base.OnClosed(e);
        }

        // -- Layer 3 live state (V04) -- called on UI thread only -----------------
        // CYC=5: global toggle + 4 foreach iterations (one branch each).
        // Must run on UI thread -- always invoked via Dispatcher.InvokeAsync from OnPositionStateChanged.
        private void UpdateButtonColors(bool hasPosition, bool hasEntries)
        {
            _globalToggleBtn.Background = _copyEnabled ? WBrushActive : WBrushInactive;
            foreach (var btn in _flattenBtns) btn.Background = hasPosition ? WBrushDanger   : WBrushInactive;
            foreach (var btn in _cancelBtns)  btn.Background = hasEntries  ? WBrushDanger   : WBrushInactive;
            foreach (var btn in _trimBtns)    btn.Background = hasPosition ? WBrushCaution  : WBrushInactive;
            foreach (var btn in _beBtns)      btn.Background = hasPosition ? WBrushActive   : WBrushInactive;
        }

        // CYC=1: single null guard -- Window shows all rules, no per-instrument filter.
        // JS-023: marshals onto UI thread via Dispatcher.InvokeAsync.
        // JS-003: PositionState is a readonly struct -- captured by value in closure.
        private void OnPositionStateChanged(string instr, PositionState state)
        {
            if (instr == null) return;
            Dispatcher.InvokeAsync(() => UpdateButtonColors(state.HasOpenPosition, state.HasWorkingEntries));
        }

        private void BuildUI()
        {
            var root = new DockPanel { LastChildFill = true };

            // --- Title ---
            var titleBlock = new TextBlock
            {
                Text       = "Prop Trader Tools -- Trade Copier",
                FontWeight = FontWeights.Bold,
                Margin     = new Thickness(6, 4, 4, 2)
            };
            DockPanel.SetDock(titleBlock, Dock.Top);
            root.Children.Add(titleBlock);

            // --- Global toggle (color-coded -- no NTButtonStyle) ---
            _globalToggleBtn = new Button
            {
                Content    = "Copy All OFF",
                Margin     = new Thickness(6, 2, 6, 2),
                Padding    = new Thickness(8, 3, 8, 3),
                Background = WBrushInactive
            };
            _globalToggleBtn.Click += OnGlobalToggle;
            DockPanel.SetDock(_globalToggleBtn, Dock.Top);
            root.Children.Add(_globalToggleBtn);

            // B9 T3 -- Copy mode ComboBox (Signal / Mirror)
            var modeRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(6, 2, 6, 2) };
            var modeLabel = new Label { Content = "Copy Mode:", VerticalAlignment = VerticalAlignment.Center, Padding = new Thickness(0, 0, 4, 0) };
            var modeCb = new ComboBox { Width = 120, VerticalAlignment = VerticalAlignment.Center };
            modeCb.Items.Add("Signal (default)");
            modeCb.Items.Add("Mirror");
            modeCb.SelectedIndex = 0;
            modeCb.SelectionChanged += OnCopyModeComboChanged;
            modeRow.Children.Add(modeLabel);
            modeRow.Children.Add(modeCb);
            DockPanel.SetDock(modeRow, Dock.Top);
            root.Children.Add(modeRow);

            // --- Separator ---
            var sep1 = new Separator { Margin = new Thickness(0, 2, 0, 2) };
            DockPanel.SetDock(sep1, Dock.Top);
            root.Children.Add(sep1);

            // --- Rules area (B7-F5: wrapped in ScrollViewer MaxHeight=400) ---
            _rulesPanel = new StackPanel();
            _rulesPanel.Children.Add(BuildRuleRow("MES"));

            var rulesScroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 400,
                Content   = _rulesPanel
            };
            // DockPanel.SetDock on ScrollViewer (outer wrapper), NOT on _rulesPanel
            DockPanel.SetDock(rulesScroll, Dock.Top);
            root.Children.Add(rulesScroll);

            // --- Add Rule button ---
            var addRuleBtn = new Button
            {
                Content = "+ Add Rule",
                Margin  = new Thickness(6, 2, 6, 2),
                Padding = new Thickness(8, 3, 8, 3)
            };
            addRuleBtn.Click += OnAddRule;
            DockPanel.SetDock(addRuleBtn, Dock.Top);
            root.Children.Add(addRuleBtn);

            // --- Separator ---
            var sep2 = new Separator { Margin = new Thickness(0, 2, 0, 2) };
            DockPanel.SetDock(sep2, Dock.Top);
            root.Children.Add(sep2);

            // --- Log (fills remaining space) ---
            var logPanel = new StackPanel { Orientation = Orientation.Vertical };
            _logPanel = logPanel;
            var logScroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = logPanel,
                Margin  = new Thickness(4)
            };
            // LastChildFill = true on DockPanel means this gets all remaining space
            root.Children.Add(logScroll);
            Content = root;

            // V04: ensure consistent initial state (all action buttons start grey)
            UpdateButtonColors(false, false);
        }

        private Grid BuildRuleRow(string instrumentName)
        {
            var grid = new Grid { Margin = new Thickness(2) };

            // Cols: instr | leader | follower | [1/2] | [=] | [x] | [ON] | Apply | BE-cluster | ATM
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });  // B8 T2: ATM ComboBox
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });  // B10 T3: Tighten cluster

            // Col 0: instrument label
            var instrLabel = new TextBlock
            {
                Text = instrumentName,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2)
            };
            Grid.SetColumn(instrLabel, 0);
            grid.Children.Add(instrLabel);

            // Col 1: leader ComboBox -- ItemsSource set in Loaded
            var leaderCb = new ComboBox { Margin = new Thickness(2) };
            _leaderBoxes.Add(leaderCb);
            Grid.SetColumn(leaderCb, 1);
            grid.Children.Add(leaderCb);

            // Col 2: follower ListBox -- ItemsSource set in Loaded
            var followerLb = new ListBox
            {
                SelectionMode = SelectionMode.Extended,
                MaxHeight     = 80,
                Margin        = new Thickness(2)
            };
            _followerBoxes.Add(followerLb);
            var followerScroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 80,
                Content   = followerLb
            };
            Grid.SetColumn(followerScroll, 2);
            grid.Children.Add(followerScroll);

            // Col 3: Trim (color-coded -- no NTButtonStyle)
            var trimBtn = new Button { Content = "[1/2]", Tag = instrumentName, Margin = new Thickness(2), Background = WBrushInactive };
            trimBtn.Click += OnRuleTrim;
            _trimBtns.Add(trimBtn);
            Grid.SetColumn(trimBtn, 3);
            grid.Children.Add(trimBtn);

            // Col 4: Flatten (color-coded)
            var flattenBtn = new Button { Content = "[=]", Tag = instrumentName, Margin = new Thickness(2), Background = WBrushInactive };
            flattenBtn.Click += OnRuleFlatten;
            _flattenBtns.Add(flattenBtn);
            Grid.SetColumn(flattenBtn, 4);
            grid.Children.Add(flattenBtn);

            // Col 5: Cancel (color-coded)
            var cancelBtn = new Button { Content = "[x]", Tag = instrumentName, Margin = new Thickness(2), Background = WBrushInactive };
            cancelBtn.Click += OnRuleCancel;
            _cancelBtns.Add(cancelBtn);
            Grid.SetColumn(cancelBtn, 5);
            grid.Children.Add(cancelBtn);

            // Col 6: per-rule toggle (always active/colored -- starts WBrushActive = [ON])
            var toggleBtn = new Button { Content = "[ON]", Tag = instrumentName, Margin = new Thickness(2), Background = WBrushActive };
            toggleBtn.Click += OnRuleToggle;
            Grid.SetColumn(toggleBtn, 6);
            grid.Children.Add(toggleBtn);

            // B8 T2: Col 9 -- ATM mode ComboBox -- created BEFORE applyBtn so tag can reference it.
            // signalName for CreateOrder is always "PTT-Copy"; this selects order type override only.
            var atmCb = new ComboBox { Width = 80, Margin = new Thickness(2) };
            atmCb.Items.Add("Inherit");
            atmCb.Items.Add("Market");
            atmCb.Items.Add("Named");
            atmCb.SelectedIndex = 0;

            // B9 T3: Named ATM inline TextBox -- appears when "Named" is selected
            var namedBox = new TextBox { Width = 80, Visibility = Visibility.Collapsed, ToolTip = "ATM template name", Margin = new Thickness(2) };
            atmCb.SelectionChanged += (s, e2) =>
            {
                var sel = (s as ComboBox)?.SelectedItem?.ToString() ?? string.Empty;
                namedBox.Visibility = sel == "Named" ? Visibility.Visible : Visibility.Collapsed;
                if (sel != "Named") namedBox.Text = string.Empty;
            };
            // B9 T3: Col 9 -- ATM mode ComboBox + Named ATM TextBox (stacked vertically in column)
            var atmColPanel = new StackPanel { Orientation = Orientation.Vertical };
            atmColPanel.Children.Add(atmCb);
            atmColPanel.Children.Add(namedBox);
            Grid.SetColumn(atmColPanel, 9);
            grid.Children.Add(atmColPanel);

            // Col 7: Apply (non-color-coded -- standard button)
            // B8 T2: tag[3] = atmCb, B9 T3: tag[4] = namedBox so OnRowApply can read Named ATM text.
            var applyBtn = new Button { Content = "Apply", Margin = new Thickness(2) };
            applyBtn.Tag = new object[] { instrumentName, leaderCb, followerLb, atmCb, namedBox };
            applyBtn.Click += OnRowApply;
            Grid.SetColumn(applyBtn, 7);
            grid.Children.Add(applyBtn);

            // Col 8: Break Even cluster (BE button color-coded)
            var beCluster = new StackPanel { Orientation = Orientation.Horizontal };
            var beBtn     = new Button { Content = "[BE]", Margin = new Thickness(2), Background = WBrushInactive };
            var beBox     = new TextBox { Text = "2", Width = 28, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(2) };
            var tksLabel  = new TextBlock { Text = "tks", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(1, 0, 2, 0) };
            beBtn.Tag     = new object[] { instrumentName, beBox };
            beBtn.Click  += OnRuleBreakEven;
            _beBtns.Add(beBtn);
            beCluster.Children.Add(beBtn);
            beCluster.Children.Add(beBox);
            beCluster.Children.Add(tksLabel);
            Grid.SetColumn(beCluster, 8);
            grid.Children.Add(beCluster);

            // B10 T3: Col 10 -- Tighten Stop cluster ([~] button + TextBox + "tks" label).
            // Tag = "instrumentName|5" (rule name | default ticks). OnRuleTightenStop reads tag.
            // NTButtonStyle: tighten is non-color-coded (not position-state-colored).
            var tightenCluster10 = new StackPanel { Orientation = Orientation.Horizontal };
            var tightenTicksBox10 = new TextBox { Text = "5", Width = 28,
                VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(2) };
            var tightenBtn10 = new Button { Content = "[~]", Margin = new Thickness(2), Background = WBrushInactive };
            var tightenTksLbl10 = new TextBlock { Text = "tks",
                VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(1, 0, 2, 0) };
            tightenBtn10.Tag    = new object[] { instrumentName, tightenTicksBox10 };
            tightenBtn10.Click += OnRuleTightenStop;
            _tightenBtns.Add(tightenBtn10);
            tightenCluster10.Children.Add(tightenBtn10);
            tightenCluster10.Children.Add(tightenTicksBox10);
            tightenCluster10.Children.Add(tightenTksLbl10);
            Grid.SetColumn(tightenCluster10, 10);
            grid.Children.Add(tightenCluster10);

            return grid;
        }

        private Grid BuildDynamicRuleRow()
        {
            var grid = new Grid { Margin = new Thickness(2) };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });  // B8 T2: ATM ComboBox
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });  // B10 T3: Tighten cluster

            // Col 0: instrument TextBox
            var instrTextBox = new TextBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin   = new Thickness(2),
                MinWidth = 45
            };
            Grid.SetColumn(instrTextBox, 0);
            grid.Children.Add(instrTextBox);

            // Col 1: leader ComboBox -- ItemsSource bound immediately (window is already loaded)
            var leaderCb = new ComboBox { ItemsSource = Account.All, Margin = new Thickness(2) };
            Grid.SetColumn(leaderCb, 1);
            grid.Children.Add(leaderCb);

            // Col 2: follower ListBox -- bound immediately
            var followerLb = new ListBox
            {
                SelectionMode = SelectionMode.Extended,
                ItemsSource   = Account.All,
                MaxHeight     = 80,
                Margin        = new Thickness(2)
            };
            var followerScroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                MaxHeight = 80,
                Content   = followerLb
            };
            Grid.SetColumn(followerScroll, 2);
            grid.Children.Add(followerScroll);

            // Col 3: Trim (color-coded)
            var trimBtn = new Button { Content = "[1/2]", Tag = instrTextBox, Margin = new Thickness(2), Background = WBrushInactive };
            trimBtn.Click += OnRuleTrim;
            _trimBtns.Add(trimBtn);
            Grid.SetColumn(trimBtn, 3);
            grid.Children.Add(trimBtn);

            // Col 4: Flatten (color-coded)
            var flattenBtn = new Button { Content = "[=]", Tag = instrTextBox, Margin = new Thickness(2), Background = WBrushInactive };
            flattenBtn.Click += OnRuleFlatten;
            _flattenBtns.Add(flattenBtn);
            Grid.SetColumn(flattenBtn, 4);
            grid.Children.Add(flattenBtn);

            // Col 5: Cancel (color-coded)
            var cancelBtn = new Button { Content = "[x]", Tag = instrTextBox, Margin = new Thickness(2), Background = WBrushInactive };
            cancelBtn.Click += OnRuleCancel;
            _cancelBtns.Add(cancelBtn);
            Grid.SetColumn(cancelBtn, 5);
            grid.Children.Add(cancelBtn);

            // Col 6: toggle (starts WBrushActive = [ON])
            var toggleBtn = new Button { Content = "[ON]", Tag = instrTextBox, Margin = new Thickness(2), Background = WBrushActive };
            toggleBtn.Click += OnRuleToggle;
            Grid.SetColumn(toggleBtn, 6);
            grid.Children.Add(toggleBtn);

            // Col 7: Apply (non-color-coded)
            // B8 T2 + B9 T3: tag[3]=atmCb, tag[4]=namedBox for OnRowApply Named ATM text
            var atmCbDyn = new ComboBox { Width = 80, Margin = new Thickness(2) };
            atmCbDyn.Items.Add("Inherit");
            atmCbDyn.Items.Add("Market");
            atmCbDyn.Items.Add("Named");
            atmCbDyn.SelectedIndex = 0;

            // B9 T3: Named ATM inline TextBox for dynamic rows
            var namedBoxDyn = new TextBox { Width = 80, Visibility = Visibility.Collapsed, ToolTip = "ATM template name", Margin = new Thickness(2) };
            atmCbDyn.SelectionChanged += (s, e2) =>
            {
                var sel = (s as ComboBox)?.SelectedItem?.ToString() ?? string.Empty;
                namedBoxDyn.Visibility = sel == "Named" ? Visibility.Visible : Visibility.Collapsed;
                if (sel != "Named") namedBoxDyn.Text = string.Empty;
            };
            var applyBtn = new Button { Content = "Apply", Margin = new Thickness(2) };
            applyBtn.Tag   = new object[] { instrTextBox, leaderCb, followerLb, atmCbDyn, namedBoxDyn };
            applyBtn.Click += OnRowApply;
            Grid.SetColumn(applyBtn, 7);
            grid.Children.Add(applyBtn);

            // Col 8: Break Even cluster (BE button color-coded)
            var beCluster = new StackPanel { Orientation = Orientation.Horizontal };
            var beBtn     = new Button { Content = "[BE]", Margin = new Thickness(2), Background = WBrushInactive };
            var beBox     = new TextBox { Text = "2", Width = 28, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(2) };
            var tksLabel  = new TextBlock { Text = "tks", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(1, 0, 2, 0) };
            beBtn.Tag     = new object[] { instrTextBox, beBox };
            beBtn.Click  += OnRuleBreakEven;
            _beBtns.Add(beBtn);
            beCluster.Children.Add(beBtn);
            beCluster.Children.Add(beBox);
            beCluster.Children.Add(tksLabel);
            Grid.SetColumn(beCluster, 8);
            grid.Children.Add(beCluster);

            // B8 T2 + B9 T3: Col 9 -- ATM mode ComboBox + Named ATM TextBox (stacked vertically in column)
            var atmColPanel = new StackPanel { Orientation = Orientation.Vertical };
            atmColPanel.Children.Add(atmCbDyn);
            atmColPanel.Children.Add(namedBoxDyn);
            Grid.SetColumn(atmColPanel, 9);
            grid.Children.Add(atmColPanel);

            // B10 T3: Col 10 -- Tighten Stop cluster for dynamic rows.
            // Tag[0] = instrTextBox, Tag[1] = tightenTicksBox -- OnRuleTightenStop reads ticks from tag[1].
            var tightenClusterDyn = new StackPanel { Orientation = Orientation.Horizontal };
            var tightenTicksBoxDyn = new TextBox { Text = "5", Width = 28,
                VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(2) };
            var tightenBtnDyn = new Button { Content = "[~]", Margin = new Thickness(2), Background = WBrushInactive };
            var tightenTksLblDyn = new TextBlock { Text = "tks",
                VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(1, 0, 2, 0) };
            tightenBtnDyn.Tag    = new object[] { instrTextBox, tightenTicksBoxDyn };
            tightenBtnDyn.Click += OnRuleTightenStop;
            _tightenBtns.Add(tightenBtnDyn);
            tightenClusterDyn.Children.Add(tightenBtnDyn);
            tightenClusterDyn.Children.Add(tightenTicksBoxDyn);
            tightenClusterDyn.Children.Add(tightenTksLblDyn);
            Grid.SetColumn(tightenClusterDyn, 10);
            grid.Children.Add(tightenClusterDyn);

            return grid;
        }

        // B9 T3: CYC=2 -- null guard (1) + index-based ternary (straight-line, counts as 1 branch = CYC=2)
        private void OnCopyModeComboChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb == null) return;                                              // guard (1)
            CopyEngine.Instance.SetCopyMode(
                cb.SelectedIndex == 1 ? CopyMode.Mirror : CopyMode.Signal);    // branch (2)
        }

        private void OnGlobalToggle(object sender, RoutedEventArgs e)
        {
            _copyEnabled = !_copyEnabled;
            _engine.SetEnabled(_copyEnabled);
            _globalToggleBtn.Content    = _copyEnabled ? "Copy All ON" : "Copy All OFF";
            _globalToggleBtn.Background = _copyEnabled ? WBrushActive  : WBrushInactive;
        }

        private void OnAddRule(object sender, RoutedEventArgs e)
        {
            _rulesPanel.Children.Add(BuildDynamicRuleRow());
        }

        private void OnRuleTrim(object sender, RoutedEventArgs e)
        {
            var btn      = sender as Button;
            string name  = btn?.Tag is TextBox tb ? tb.Text : btn?.Tag as string;
            var instr    = FindInstrument(name);
            if (instr != null) _engine.Trim(instr);
        }

        private void OnRuleFlatten(object sender, RoutedEventArgs e)
        {
            var btn     = sender as Button;
            string name = btn?.Tag is TextBox tb ? tb.Text : btn?.Tag as string;
            var instr   = FindInstrument(name);
            if (instr != null) _engine.Flatten(instr);
        }

        private void OnRuleCancel(object sender, RoutedEventArgs e)
        {
            var btn     = sender as Button;
            string name = btn?.Tag is TextBox tb ? tb.Text : btn?.Tag as string;
            var instr   = FindInstrument(name);
            if (instr != null) _engine.CancelPendingEntries(instr);
        }

        private void OnRuleToggle(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            string name   = btn.Tag is TextBox tb ? tb.Text : btn.Tag as string;
            bool newState = (string)btn.Content == "[ON]" ? false : true;
            btn.Content    = newState ? "[ON]" : "[OFF]";
            btn.Background = newState ? WBrushActive : WBrushInactive;
            _engine.SetRuleEnabled(name, newState);
        }

        private void OnRuleBreakEven(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button)?.Tag as object[];
            if (tag == null) return;
            string name = tag[0] is TextBox tb ? tb.Text : tag[0] as string;
            if (string.IsNullOrEmpty(name)) return;
            int ticks = 2;
            if (tag.Length > 1 && tag[1] is TextBox beBox)
                if (int.TryParse(beBox.Text?.Trim(), out int parsed) && parsed >= 0)
                    ticks = parsed;
            var instr = FindInstrument(name);
            if (instr != null) _engine.BreakEven(instr, ticks);
        }

        // B10 T3 -- OnRuleTightenStop: tighten stop click handler for rule rows.
        // CYC=4: tag null(1), name empty(2), instr null(3), engine call(4).
        // Reads rule name from tag[0] (string or TextBox), ticks from tag[1] (TextBox).
        // NT8-003: no Math.Clamp. Math.Max/Min clamp 1-500. JS-021: no lock.
        private void OnRuleTightenStop(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button)?.Tag as object[];
            if (tag == null)                                           // (1)
                return;
            string name = tag[0] is TextBox tb0 ? tb0.Text : tag[0] as string;
            if (string.IsNullOrEmpty(name))                            // (2)
                return;
            var instr = FindInstrument(name);
            if (instr == null)                                         // (3)
                return;
            int ticks = 5;
            if (tag.Length > 1 && tag[1] is TextBox ticksBox)
                if (int.TryParse(ticksBox.Text?.Trim(), out int parsed))
                    ticks = Math.Max(1, Math.Min(500, parsed));        // clamp: no Math.Clamp (.NET 4.8 ban)
            _engine.TightenStop(instr, ticks);                        // (4)
        }

        // B8 T2: OnRowApply -- reads ATM ComboBox selection (tag[3]) and builds ATM map.
        // signalName for CreateOrder is always "PTT-Copy" -- ATM mode is applied by engine.
        // CYC=5 (tag null + name empty + leader null + followers empty + atm foreach).
        private void OnRowApply(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button)?.Tag as object[];
            if (tag == null) return;
            string name = tag[0] is TextBox tb ? tb.Text : tag[0] as string;
            if (string.IsNullOrEmpty(name)) return;
            var leaderCb   = tag[1] as ComboBox;
            var leader     = leaderCb?.SelectedItem as Account;
            var followerLb = tag[2] as ListBox;
            var followers  = new List<Account>();
            if (followerLb != null)
                foreach (var item in followerLb.SelectedItems)
                    if (item is Account acc) followers.Add(acc);
            if (leader == null || followers.Count == 0) return;

            // B8 T2 + B9 T3: read ATM mode from tag[3]; if "Named", append tag[4] namedBox text
            var atmMap = new Dictionary<string, FollowerAtmMode>();
            if (tag.Length > 3 && tag[3] is ComboBox atmCb && atmCb.SelectedItem is string atmSel)
            {
                string atmMode = atmSel;
                // B9 T3: when Named, append the textbox value as "Named:templateName"
                if (atmMode == "Named" && tag.Length > 4 && tag[4] is TextBox namedBox && namedBox.Text.Length > 0) // branch +1
                    atmMode = "Named:" + namedBox.Text;
                var mode = CopyEngine.ParseAtmModeName(atmMode);
                foreach (var acc in followers)
                    atmMap[acc.Name] = mode;
            }

            // Multipliers default to all-1s for Window surface (Panel handles per-follower multipliers)
            var multipliers = new int[followers.Count];
            for (int i = 0; i < multipliers.Length; i++) multipliers[i] = 1;

            _engine.AddRule(name, leader, followers.ToArray(), multipliers, atmMap);
        }

        private void OnStatusUpdate(string line)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (_logPanel == null) return;
                var tb = new TextBlock
                {
                    Text = DateTime.UtcNow.ToString("HH:mm:ss") + "  " + line
                };
                _logPanel.Children.Insert(0, tb);
                while (_logPanel.Children.Count > MaxLogLines)
                    _logPanel.Children.RemoveAt(_logPanel.Children.Count - 1);
            });
        }

        private Instrument FindInstrument(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            try   { return Instrument.GetInstrument(name); }
            catch { return null; }
        }

    }
}
