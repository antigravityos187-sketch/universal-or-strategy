// Legacy Chart Buttons Indicator
// Remote control overlay that sends IPC commands to UniversalORStrategyV12

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;

namespace NinjaTrader.NinjaScript.Indicators
{
    public class LegacyChartButtonsIndicator : Indicator
    {
        private const string IpcHost = "127.0.0.1";

        private Border panel;
        private bool panelCreated;
        private bool rmaClickArmed;
        private bool momoArmed;

        private static readonly SolidColorBrush PanelBg = new SolidColorBrush(Color.FromRgb(12, 16, 24));
        private static readonly SolidColorBrush PanelBorder = new SolidColorBrush(Color.FromRgb(30, 41, 59));
        private static readonly SolidColorBrush GreenBg = new SolidColorBrush(Color.FromRgb(6, 78, 59));
        private static readonly SolidColorBrush GreenFg = new SolidColorBrush(Color.FromRgb(74, 222, 128));
        private static readonly SolidColorBrush RedBg = new SolidColorBrush(Color.FromRgb(69, 10, 10));
        private static readonly SolidColorBrush RedFg = new SolidColorBrush(Color.FromRgb(248, 113, 113));
        private static readonly SolidColorBrush OrangeBg = new SolidColorBrush(Color.FromRgb(124, 45, 18));
        private static readonly SolidColorBrush OrangeFg = new SolidColorBrush(Color.FromRgb(251, 146, 60));
        private static readonly SolidColorBrush SlateBg = new SolidColorBrush(Color.FromRgb(23, 23, 23));
        private static readonly SolidColorBrush SlateFg = new SolidColorBrush(Color.FromRgb(212, 212, 212));

        static LegacyChartButtonsIndicator()
        {
            PanelBg.Freeze();
            PanelBorder.Freeze();
            GreenBg.Freeze();
            GreenFg.Freeze();
            RedBg.Freeze();
            RedFg.Freeze();
            OrangeBg.Freeze();
            OrangeFg.Freeze();
            SlateBg.Freeze();
            SlateFg.Freeze();
        }

        [NinjaScriptProperty]
        [Display(Name = "IPC Port", Description = "TCP Port for IPC commands (Default: 5000)", Order = 0, GroupName = "Parameters")]
        public int IpcPort { get; set; }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Legacy chart overlay buttons (IPC remote control)";
                Name = "Legacy Chart Buttons";
                Calculate = Calculate.OnPriceChange;
                IsOverlay = true;
                DisplayInDataBox = false;
                DrawOnPricePanel = false;
                IsSuspendedWhileInactive = false;
                IpcPort = 5000;
            }
            else if (State == State.Historical)
            {
                if (ChartControl != null)
                    ChartControl.Dispatcher.InvokeAsync(CreatePanel);
            }
            else if (State == State.Terminated)
            {
                if (ChartControl != null)
                    ChartControl.Dispatcher.InvokeAsync(RemovePanel);
            }
        }

        private void CreatePanel()
        {
            if (panelCreated || ChartControl == null) return;

            try
            {
                panel = new Border
                {
                    Background = PanelBg,
                    BorderBrush = PanelBorder,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(6),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(6, 10, 0, 0)
                };

                StackPanel root = new StackPanel { Orientation = Orientation.Vertical };

                TextBlock header = new TextBlock
                {
                    Text = "LEGACY CONTROLS",
                    Foreground = SlateFg,
                    FontWeight = FontWeights.Bold,
                    FontSize = 10,
                    Margin = new Thickness(2, 0, 2, 6)
                };
                root.Children.Add(header);

                UniformGrid row1 = new UniformGrid { Columns = 4, Margin = new Thickness(0, 0, 0, 4) };
                row1.Children.Add(CreateCommandButton("LONG", GreenBg, GreenFg, () => SendCommand("LONG")));
                row1.Children.Add(CreateCommandButton("SHORT", RedBg, RedFg, () => SendCommand("SHORT")));
                row1.Children.Add(CreateCommandButton("OR L", SlateBg, SlateFg, () => SendCommand("OR_LONG")));
                row1.Children.Add(CreateCommandButton("OR S", SlateBg, SlateFg, () => SendCommand("OR_SHORT")));
                root.Children.Add(row1);

                UniformGrid row2 = new UniformGrid { Columns = 4, Margin = new Thickness(0, 0, 0, 4) };
                row2.Children.Add(CreateCommandButton("B+2", SlateBg, SlateFg, () => SendCommand("BE_PLUS_2")));
                row2.Children.Add(CreateCommandButton("TRIM25", SlateBg, SlateFg, () => SendCommand("TRIM_25")));
                row2.Children.Add(CreateCommandButton("TRIM50", SlateBg, SlateFg, () => SendCommand("TRIM_50")));
                row2.Children.Add(CreateCommandButton("CANCEL", SlateBg, SlateFg, () => SendCommand("CANCEL_ALL")));
                root.Children.Add(row2);

                UniformGrid row3 = new UniformGrid { Columns = 4, Margin = new Thickness(0, 0, 0, 4) };
                row3.Children.Add(CreateCommandButton("T1", SlateBg, SlateFg, () => SendCommand("CLOSE_T1")));
                row3.Children.Add(CreateCommandButton("T2", SlateBg, SlateFg, () => SendCommand("CLOSE_T2")));
                row3.Children.Add(CreateCommandButton("T3", SlateBg, SlateFg, () => SendCommand("CLOSE_T3")));
                row3.Children.Add(CreateCommandButton("T4", SlateBg, SlateFg, () => SendCommand("CLOSE_T4")));
                root.Children.Add(row3);

                UniformGrid row4 = new UniformGrid { Columns = 2, Margin = new Thickness(0, 0, 0, 4) };
                row4.Children.Add(CreateToggleButton("RMA CLICK", SlateBg, SlateFg, () =>
                {
                    rmaClickArmed = !rmaClickArmed;
                    string cmd = rmaClickArmed ? "SET_RMA_MODE|ON" : "SET_RMA_MODE|OFF";
                    SendCommand(cmd, includeSymbol: false);
                }));
                row4.Children.Add(CreateToggleButton("MOMO", SlateBg, SlateFg, () =>
                {
                    momoArmed = !momoArmed;
                    SendCommand("MODE_MOMO");
                }));
                root.Children.Add(row4);

                Button flatten = CreateCommandButton("FLATTEN", OrangeBg, OrangeFg, () => SendCommand("FLATTEN"));
                flatten.Margin = new Thickness(0, 2, 0, 0);
                root.Children.Add(flatten);

                panel.Child = root;
                UserControlCollection.Add(panel);
                panelCreated = true;
            }
            catch (Exception ex)
            {
                Print("LegacyChartButtonsIndicator CreatePanel error: " + ex.Message);
            }
        }

        private void RemovePanel()
        {
            if (panel != null && panelCreated)
            {
                try { UserControlCollection.Remove(panel); } catch { }
            }
            panelCreated = false;
            panel = null;
        }

        private Button CreateCommandButton(string text, Brush bg, Brush fg, Action onClick)
        {
            Button btn = new Button
            {
                Content = text,
                Background = bg,
                Foreground = fg,
                BorderBrush = PanelBorder,
                BorderThickness = new Thickness(1),
                Height = 26,
                Margin = new Thickness(1),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand
            };
            btn.Click += (s, e) => onClick?.Invoke();
            return btn;
        }

        private Button CreateToggleButton(string text, Brush bg, Brush fg, Action onClick)
        {
            Button btn = CreateCommandButton(text, bg, fg, onClick);
            btn.FontSize = 9;
            return btn;
        }

        private void SendCommand(string action, bool includeSymbol = true)
        {
            string symbol = Instrument?.MasterInstrument?.Name;
            string payload = includeSymbol && !string.IsNullOrWhiteSpace(symbol)
                ? string.Format("{0}|{1}", action, symbol)
                : action;

            Task.Run(() =>
            {
                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        client.Connect(IpcHost, IpcPort);
                        using (NetworkStream stream = client.GetStream())
                        {
                            byte[] data = Encoding.UTF8.GetBytes(payload + "\n");
                            stream.Write(data, 0, data.Length);
                            stream.Flush();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Print("LegacyChartButtonsIndicator IPC error: " + ex.Message);
                }
            });
        }
    }
}
