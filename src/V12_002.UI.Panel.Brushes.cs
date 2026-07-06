// Build 1105: V12_001 panel port -- frozen WPF brush palette
using System.Windows.Media;

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class V12_002
    {
        #region Panel Brush Color Constants

        // Neutral / Background
        private const byte BG_DEEP_R = 5;
        private const byte BG_DEEP_G = 5;
        private const byte BG_DEEP_B = 8;
        private const byte BG_SLATE_R = 15;
        private const byte BG_SLATE_G = 23;
        private const byte BG_SLATE_B = 42;
        private const byte BORDER_SLATE_R = 30;
        private const byte BORDER_SLATE_G = 41;
        private const byte BORDER_SLATE_B = 59;
        private const byte BTN_BG_R = 23;
        private const byte BTN_BG_G = 23;
        private const byte BTN_BG_B = 28;
        private const byte BTN_BORDER_R = 45;
        private const byte BTN_BORDER_G = 45;
        private const byte BTN_BORDER_B = 55;

        // Text
        private const byte TEXT_PRI_R = 220;
        private const byte TEXT_PRI_G = 220;
        private const byte TEXT_PRI_B = 220;
        private const byte TEXT_DIM_R = 115;
        private const byte TEXT_DIM_G = 115;
        private const byte TEXT_DIM_B = 125;

        // Cyan Accent
        private const byte CYAN_ACCENT_R = 34;
        private const byte CYAN_ACCENT_G = 211;
        private const byte CYAN_ACCENT_B = 238;

        // Green Signal
        private const byte GREEN_BG_R = 6;
        private const byte GREEN_BG_G = 78;
        private const byte GREEN_BG_B = 59;
        private const byte GREEN_FG_R = 74;
        private const byte GREEN_FG_G = 222;
        private const byte GREEN_FG_B = 128;
        private const byte GREEN_BDR_R = 5;
        private const byte GREEN_BDR_G = 150;
        private const byte GREEN_BDR_B = 105;

        // Red Signal
        private const byte RED_BG_R = 127;
        private const byte RED_BG_G = 29;
        private const byte RED_BG_B = 29;
        private const byte RED_FG_R = 252;
        private const byte RED_FG_G = 165;
        private const byte RED_FG_B = 165;
        private const byte RED_BDR_R = 220;
        private const byte RED_BDR_G = 38;
        private const byte RED_BDR_B = 38;

        // Orange Signal
        private const byte ORANGE_BG_R = 124;
        private const byte ORANGE_BG_G = 45;
        private const byte ORANGE_BG_B = 18;
        private const byte ORANGE_FG_R = 251;
        private const byte ORANGE_FG_G = 146;
        private const byte ORANGE_FG_B = 60;
        private const byte ORANGE_BDR_R = 234;
        private const byte ORANGE_BDR_G = 88;
        private const byte ORANGE_BDR_B = 12;

        // Yellow Signal
        private const byte YELLOW_BG_R = 113;
        private const byte YELLOW_BG_G = 63;
        private const byte YELLOW_BG_B = 18;
        private const byte YELLOW_FG_R = 250;
        private const byte YELLOW_FG_G = 204;
        private const byte YELLOW_FG_B = 21;
        private const byte YELLOW_BDR_R = 202;
        private const byte YELLOW_BDR_G = 138;
        private const byte YELLOW_BDR_B = 4;

        // Pink Signal
        private const byte PINK_BG_R = 131;
        private const byte PINK_BG_G = 24;
        private const byte PINK_BG_B = 67;
        private const byte PINK_FG_R = 244;
        private const byte PINK_FG_G = 114;
        private const byte PINK_FG_B = 182;
        private const byte PINK_BDR_R = 219;
        private const byte PINK_BDR_G = 39;
        private const byte PINK_BDR_B = 119;

        // Cyan Signal
        private const byte CYAN_BG_R = 22;
        private const byte CYAN_BG_G = 78;
        private const byte CYAN_BG_B = 99;
        private const byte CYAN_FG_R = 34;
        private const byte CYAN_FG_G = 211;
        private const byte CYAN_FG_B = 238;
        private const byte CYAN_BDR_R = 8;
        private const byte CYAN_BDR_G = 145;
        private const byte CYAN_BDR_B = 178;

        // Purple Signal
        private const byte PURPLE_FG_R = 168;
        private const byte PURPLE_FG_G = 85;
        private const byte PURPLE_FG_B = 247;

        #endregion

        #region Panel Brushes

        private static SolidColorBrush PanelBrush(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        private static readonly SolidColorBrush BgDeep = PanelBrush(BG_DEEP_R, BG_DEEP_G, BG_DEEP_B);
        private static readonly SolidColorBrush BgSlate = PanelBrush(BG_SLATE_R, BG_SLATE_G, BG_SLATE_B);
        private static readonly SolidColorBrush BorderSlate = PanelBrush(BORDER_SLATE_R, BORDER_SLATE_G, BORDER_SLATE_B);
        private static readonly SolidColorBrush BtnBg = PanelBrush(BTN_BG_R, BTN_BG_G, BTN_BG_B);
        private static readonly SolidColorBrush BtnBorder = PanelBrush(BTN_BORDER_R, BTN_BORDER_G, BTN_BORDER_B);
        private static readonly SolidColorBrush TextPri = PanelBrush(TEXT_PRI_R, TEXT_PRI_G, TEXT_PRI_B);
        private static readonly SolidColorBrush TextDim = PanelBrush(TEXT_DIM_R, TEXT_DIM_G, TEXT_DIM_B);
        private static readonly SolidColorBrush CyanAccent = PanelBrush(CYAN_ACCENT_R, CYAN_ACCENT_G, CYAN_ACCENT_B);

        private static readonly SolidColorBrush GreenBg = PanelBrush(GREEN_BG_R, GREEN_BG_G, GREEN_BG_B);
        private static readonly SolidColorBrush GreenFg = PanelBrush(GREEN_FG_R, GREEN_FG_G, GREEN_FG_B);
        private static readonly SolidColorBrush GreenBdr = PanelBrush(GREEN_BDR_R, GREEN_BDR_G, GREEN_BDR_B);

        private static readonly SolidColorBrush RedBg = PanelBrush(RED_BG_R, RED_BG_G, RED_BG_B);
        private static readonly SolidColorBrush RedFg = PanelBrush(RED_FG_R, RED_FG_G, RED_FG_B);
        private static readonly SolidColorBrush RedBdr = PanelBrush(RED_BDR_R, RED_BDR_G, RED_BDR_B);

        private static readonly SolidColorBrush OrangeBg = PanelBrush(ORANGE_BG_R, ORANGE_BG_G, ORANGE_BG_B);
        private static readonly SolidColorBrush OrangeFg = PanelBrush(ORANGE_FG_R, ORANGE_FG_G, ORANGE_FG_B);
        private static readonly SolidColorBrush OrangeBdr = PanelBrush(ORANGE_BDR_R, ORANGE_BDR_G, ORANGE_BDR_B);

        private static readonly SolidColorBrush YellowBg = PanelBrush(YELLOW_BG_R, YELLOW_BG_G, YELLOW_BG_B);
        private static readonly SolidColorBrush YellowFg = PanelBrush(YELLOW_FG_R, YELLOW_FG_G, YELLOW_FG_B);
        private static readonly SolidColorBrush YellowBdr = PanelBrush(YELLOW_BDR_R, YELLOW_BDR_G, YELLOW_BDR_B);

        private static readonly SolidColorBrush PinkBg = PanelBrush(PINK_BG_R, PINK_BG_G, PINK_BG_B);
        private static readonly SolidColorBrush PinkFg = PanelBrush(PINK_FG_R, PINK_FG_G, PINK_FG_B);
        private static readonly SolidColorBrush PinkBdr = PanelBrush(PINK_BDR_R, PINK_BDR_G, PINK_BDR_B);

        private static readonly SolidColorBrush CyanBg = PanelBrush(CYAN_BG_R, CYAN_BG_G, CYAN_BG_B);
        private static readonly SolidColorBrush CyanFg = PanelBrush(CYAN_FG_R, CYAN_FG_G, CYAN_FG_B);
        private static readonly SolidColorBrush CyanBdr = PanelBrush(CYAN_BDR_R, CYAN_BDR_G, CYAN_BDR_B);

        private static readonly SolidColorBrush PurpleFg = PanelBrush(PURPLE_FG_R, PURPLE_FG_G, PURPLE_FG_B);

        private static SolidColorBrush TextPrimary
        {
            get { return TextPri; }
        }
        private static SolidColorBrush TextMuted
        {
            get { return TextDim; }
        }

        private static SolidColorBrush GreenBorder
        {
            get { return GreenBdr; }
        }
        private static SolidColorBrush RedBorder
        {
            get { return RedBdr; }
        }
        private static SolidColorBrush OrangeBorder
        {
            get { return OrangeBdr; }
        }
        private static SolidColorBrush YellowBorder
        {
            get { return YellowBdr; }
        }
        private static SolidColorBrush PinkBorder
        {
            get { return PinkBdr; }
        }
        private static SolidColorBrush CyanBorder
        {
            get { return CyanBdr; }
        }

        #endregion
    }
}
