// V12.11 FLEET SYMMETRY & SAFETY HARDENING - Single-Instance Multi-Account Copy Trading Engine
// Based on UniversalORStrategyV10_3.cs (BUILD 1702)
// SIMA Architecture: One strategy instance on Master account broadcasts to all Apex accounts
//
// SAFETY: This file was auto-generated. Original V10_3 file unchanged.
//
// Key Features:
//   - Account Loop execution (Account.All iteration)
//   - IPC command distribution to multiple accounts
//   - Reaper Audit thread for position verification
//   - [SIMA] logging prefix for all multi-account operations
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;  // V8.30: Thread-safe collections
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;  // V8.30: For .Values.Contains() on ConcurrentDictionary
using System.Text;
using System.Globalization;
using System.Threading;  // V8.30: For Interlocked operations
using System.Threading.Tasks; // V12.2: For Task.Run in async operations
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;  // V11: For UniformGrid
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;  // V11: For Ellipse in header
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
    public class UniversalORStrategyV12_Dev_FIX_CHART_VISIBILITY : Strategy
    {
        #region Variables

        // OR tracking
        private double sessionHigh;
        private double sessionLow;
        private double sessionMid;
        private double sessionRange;
        private bool isInORWindow;
        private bool orComplete;
        private DateTime orStartDateTime;
        private DateTime orEndDateTime;
        private DateTime sessionStartDateTime;
        private DateTime lastResetDate;
        private int orStartBarIndex;
        private int orEndBarIndex;

        // Instrument info
        private double tickSize;
        private double pointValue;
        private int minContracts;

        // ATR Indicator for RMA
        private ATR atrIndicator;
        private double currentATR;
        private double lastKnownPrice;  // Track current price for UI events

        // V8.2: EMA indicators for TREND trades
        private EMA ema9;
        private EMA ema15;
        // V11: Additional EMAs for Telemetry & RMA Anchors
        private EMA ema30;
        private EMA ema65;
        private EMA ema200;

        // V11: Thread-safe Value Cache for UI Telemetry
        private double _ema9Val;
        private double _ema15Val;
        private double _ema30Val;
        private double _ema65Val;
        private double _ema200Val;
        private double _orHighVal;
        private double _orLowVal;

        // V8.7: RSI indicator for FFMA trades
        private RSI rsiIndicator;

        // V12.2: ATR Sizing & Risk Management
        private double MaxRiskAmount = 200.0;
        private ConcurrentDictionary<string, bool> activeFleetAccounts = new ConcurrentDictionary<string, bool>();

        // Position tracking - multi-target system
        // V8.30: Replaced Dictionary with ConcurrentDictionary for thread-safe access
        private ConcurrentDictionary<string, PositionInfo> activePositions;
        private ConcurrentDictionary<string, Order> entryOrders;
        private ConcurrentDictionary<string, Order> stopOrders;
        private ConcurrentDictionary<string, Order> target1Orders;
        private ConcurrentDictionary<string, Order> target2Orders;
        private ConcurrentDictionary<string, Order> target3Orders;  // v5.13: New T3 orders
        private ConcurrentDictionary<string, Order> target4Orders;  // v5.13: New T4 orders (Runner)

        // V8.11: Track pending stop replacements to fix duplicate stop bug
        // V8.30: Replaced Dictionary with ConcurrentDictionary for thread-safe access
        private ConcurrentDictionary<string, PendingStopReplacement> pendingStopReplacements;

        // RMA Mode tracking
        private bool isRMAModeActive;
        private bool isRKeyHeld;
        private bool isRMAButtonClicked;  // One-shot mode from button

        // V8.2: TREND Mode tracking
        private bool isTRENDModeActive;
        private bool pendingTRENDEntry;  // V8.2 FIX: Flag to execute TREND in OnBarUpdate when BarsInProgress=0
        private ConcurrentDictionary<string, string> linkedTRENDEntries;  // V8.30: Thread-safe - Links E1 and E2 by group ID

        // V8.4: RETEST Mode tracking
        private bool isRetestModeActive;

        // V8.6: MOMO Mode tracking
        private bool isMOMOModeActive;

        // V8.7: FFMA Mode tracking (Far From Moving Average)
        private bool isFFMAModeArmed;
        private double ffmaEntryBarHigh;   // Store entry candle high for stop (short)
        private double ffmaEntryBarLow;    // Store entry candle low for stop (long)

        // V11 Logic State
        private bool isTrendRmaMode = false; // False = STD (All-in), True = RMA (9/15 Split)
        private bool isRetestRmaMode = false; // V12: RETEST RMA toggle state

        // V12.2 Hybrid Sync: Logic State
        private bool isTosSyncMode = false;
        private bool isLongArmed = false;
        private bool isShortArmed = false;
        private DateTime lastArmedTime = DateTime.MinValue;

        // V11: RMA Anchor Logic
        public enum RmaAnchorType { Ema30, Ema65, Ema200, OrHigh, OrLow, Manual }
        private RmaAnchorType currentRmaAnchor = RmaAnchorType.Ema65; // Default to 65
        private double lastMnlPrice = 0;
        private double cachedMnlPrice = 0; // Thread-safe cache
        private bool isMnlArmed = false;

        private DateTime lastStopManagementTime; // V8.13: Stop management throttling (100ms)

        // V8.30: Circuit breaker state - prevents cascade when too many pending replacements
        private volatile int pendingReplacementCount = 0;
        private const int CIRCUIT_BREAKER_THRESHOLD = 5;
        private volatile bool circuitBreakerActive = false;
        private DateTime circuitBreakerActivatedTime = DateTime.MinValue;

        // V8.30: DrawORBox throttling - prevents chart update saturation
        private DateTime lastDrawORBoxTime = DateTime.MinValue;
        private const int DRAW_ORBOX_THROTTLE_MS = 200;

        // V8.30: Adaptive throttling based on tick frequency
        private int tickCountInLastSecond = 0;
        private DateTime lastTickCountReset = DateTime.MinValue;
        private int adaptiveThrottleMs = 100;


        // V9.1.8 IPC Integration
        private TcpListener ipcListener;
        private Thread ipcThread;
        private volatile bool isIpcRunning;
        private readonly object ipcLock = new object();
        private ConcurrentQueue<string> ipcCommandQueue;
        // V12.2: Multi-Client Support
        private ConcurrentBag<TcpClient> connectedClients;

        // V12 SIMA: Multi-Account Execution Engine
        private string _accountPrefix = "Apex"; // Default prefix for Apex accounts
        private Thread reaperThread;
        private volatile bool isReaperRunning;
        private volatile bool isFlattenRunning; // V12.8: Guard to pause Reaper during flatten
        private ConcurrentDictionary<string, int> expectedPositions; // AccountName -> Expected Quantity (+ long, - short)
        private int simaAccountCount = 0; // Cached count of detected Apex accounts
        private DateTime lastReaperLog = DateTime.MinValue;

        // V12.1 Properties (Internal Variables)
        private bool ReaperAuditEnabled = true;
        private int ReaperIntervalMs = 1000;

        // V12.1: Apex Compliance Tracking
        private ConcurrentDictionary<string, double> accountDailyProfit = new ConcurrentDictionary<string, double>();
        private ConcurrentDictionary<string, double> accountTotalProfit = new ConcurrentDictionary<string, double>();
        private string complianceLogPath;
        private DateTime lastComplianceLog = DateTime.MinValue;
        private ConcurrentDictionary<string, int> accountTradeCount = new ConcurrentDictionary<string, int>();
        private ConcurrentDictionary<string, int> accountDailyTradeCount = new ConcurrentDictionary<string, int>();
        private ConcurrentDictionary<string, double> accountEquityPeak = new ConcurrentDictionary<string, double>();
        private ConcurrentDictionary<string, double> accountMaxDrawdown = new ConcurrentDictionary<string, double>();
        private ConcurrentDictionary<string, ConcurrentDictionary<int, byte>> accountTradingDays = new ConcurrentDictionary<string, ConcurrentDictionary<int, byte>>();
        private ConcurrentDictionary<string, DateTime> accountLastSummaryDate = new ConcurrentDictionary<string, DateTime>();
        private string dailySummaryCsvPath;
        private DateTime lastDailySummaryCheck = DateTime.MinValue;
        private readonly object dailySummaryLock = new object();

        // CIT (Chase If Touch) — uses ChaseIfTouchPoints property (NinjaScriptProperty)

        #endregion

        #region Position Info Class

        private class PositionInfo
        {
            public string SignalName;
            public MarketPosition Direction;
            public int TotalContracts;
            public int T1Contracts;   // v5.13: 20% - Fixed 1pt quick profit
            public int T2Contracts;   // v5.13: 30% - 0.5x ATR
            public int T3Contracts;   // v5.13: 30% - 1.0x ATR
            public int T4Contracts;   // v5.13: 20% - Runner/Trail
            public int RemainingContracts;
            public double EntryPrice;
            public double InitialStopPrice;
            public double CurrentStopPrice;
            public double Target1Price;  // v5.13: Fixed 1pt
            public double Target2Price;  // v5.13: 0.5x ATR
            public double Target3Price;  // v5.13: 1.0x ATR
            public bool EntryFilled;
            public bool T1Filled;
            public bool T2Filled;
            public bool T3Filled;       // v5.13: New flag
            public bool BracketSubmitted;
            public double ExtremePriceSinceEntry;
            public int CurrentTrailLevel;
            public bool IsRMATrade;  // Flag to identify RMA trades
            public bool ManualBreakevenArmed;  // Manual breakeven button clicked
            public bool ManualBreakevenTriggered;  // Manual breakeven has executed
            public int TicksSinceEntry;  // v5.13: Tick counter for frequency-based trailing

            // V8.2: TREND trade tracking
            public bool IsTRENDTrade;           // Flag for TREND trades
            public bool IsTRENDEntry1;          // True if this is the 9 EMA entry (1/3)
            public bool IsTRENDEntry2;          // True if this is the 15 EMA entry (2/3)
            public string LinkedTRENDGroup;    // Links Entry1 and Entry2 together
            public bool Entry1TrailActivated;  // V8.2: True when E1 switches from fixed stop to EMA9 trail

            // V8.4: RETEST trade tracking
            public bool IsRetestTrade;          // Flag for RETEST trades
            public bool RetestTrailActivated;   // V8.4: True when retest switches from fixed stop to 9 EMA trail

            // V8.6: MOMO trade tracking
            public bool IsMOMOTrade;            // Flag for MOMO trades

            // V8.7: FFMA trade tracking
            public bool IsFFMATrade;            // Flag for FFMA trades

            // V12.1: SIMA Multi-Account tracking
            public Account ExecutingAccount;    // The account this position belongs to (null = Master)
            public bool IsFollower;             // True if this is a SIMA follower position
        }

        // V8.11: Class to track pending stop replacements
        // V8.30: Added CreatedTime for timeout support
        private class PendingStopReplacement
        {
            public string EntryName;
            public int Quantity;
            public double StopPrice;
            public MarketPosition Direction;
            public Order OldOrder;  // Track the old order being cancelled
            public DateTime CreatedTime;  // V8.30: Timeout support - clean up stale replacements
        }

        // V8.22: Thread-Safe UI Snapshot Struct
        // Decouples UI thread from Strategy thread to prevent "Collection moved" or race conditions
        public struct PositionDisplayInfo
        {
            public string TradeType;
            public string Direction;
            public double EntryPrice;
            public double StopPrice;
            public int RemainingContracts;
            public bool EntryFilled;
            public bool ManualBreakevenArmed;
            public bool ManualBreakevenTriggered;
        }

        // V12.11: Compliance snapshot for UI thread
        private struct ComplianceSnapshot
        {
            public bool Enabled;
            public bool HasAccounts;
            public string AccountName;
            public int TradeCount;
            public int UniqueDays;
            public double MaxDrawdown;
            public string ConsistencyText;
            public int ConsistencySeverity;
            public string PayoutText;
            public int PayoutSeverity;
            public string DrawdownText;
            public int DrawdownSeverity;
        }

        #endregion

        #region Enums

        public enum ORTimeframeType
        {
            Minutes_1 = 1,
            Minutes_5 = 5,
            Minutes_10 = 10,
            Minutes_15 = 15
        }

        #endregion

        #region Properties - Session Settings

        [NinjaScriptProperty]
        [Display(Name = "IPC Port", Description = "TCP Port for V9 Remote (Default: 5000)", Order = 0, GroupName = "1. Session Settings")]
        public int IpcPort { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Session Start", Description = "Trading session start time (OR begins here)", Order = 1, GroupName = "1. Session Settings")]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        public DateTime SessionStart { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Session End", Description = "Trading session end time (box ends here)", Order = 2, GroupName = "1. Session Settings")]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        public DateTime SessionEnd { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "OR Timeframe", Description = "Duration of Opening Range window", Order = 3, GroupName = "1. Session Settings")]
        public ORTimeframeType ORTimeframe { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Time Zone", Description = "Time zone for session times", Order = 4, GroupName = "1. Session Settings")]
        [TypeConverter(typeof(TimeZoneConverter))]
        public string SelectedTimeZone { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Display P/L in Points", Description = "If true, shows unrealized P/L in points. If false, shows currency ($).", Order = 5, GroupName = "1. Session Settings")]
        public bool DisplayProfitInPoints { get; set; }

        #endregion

        #region Properties - Risk Management

        public double RiskPerTrade { get; set; }

        [Browsable(false)]
        public double ReducedRiskPerTrade { get; set; }

        [Browsable(false)]
        public double StopThresholdPoints { get; set; }

        public int MESMinimum { get; set; }

        [Browsable(false)]
        public int MESMaximum { get; set; }

        public int MGCMinimum { get; set; }

        [Browsable(false)]
        public int MGCMaximum { get; set; }

        #endregion

        #region Properties - Stop Loss

        public double StopMultiplier { get; set; }

        public double MinimumStop { get; set; }

        public double MaximumStop { get; set; }

        #endregion

        #region Properties - Profit Targets

        [Range(0.25, 5.0)]
        public double Target1FixedPoints { get; set; }

        public double Target2Multiplier { get; set; }

        public double Target3Multiplier { get; set; }

        public int T1ContractPercent { get; set; }

        public int T2ContractPercent { get; set; }

        public int T3ContractPercent { get; set; }

        public int T4ContractPercent { get; set; }

        #endregion

        #region Properties - Trailing Stops

        public double BreakEvenTriggerPoints { get; set; }

        public int BreakEvenOffsetTicks { get; set; }

        public double Trail1TriggerPoints { get; set; }

        public double Trail1DistancePoints { get; set; }

        public double Trail2TriggerPoints { get; set; }

        public double Trail2DistancePoints { get; set; }

        public double Trail3TriggerPoints { get; set; }

        public double Trail3DistancePoints { get; set; }

        [Range(1, 10)]
        public int ManualBreakevenBuffer { get; set; }

        #endregion

        #region Properties - Display

        [NinjaScriptProperty]
        [Display(Name = "Show Mid Line", Description = "Show middle line in OR box", Order = 1, GroupName = "6. Display")]
        public bool ShowMidLine { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Box Opacity (%)", Description = "Transparency of OR box (0-100)", Order = 2, GroupName = "6. Display")]
        [Range(0, 100)]
        public int BoxOpacity { get; set; }

        #endregion

        #region Properties - RMA Settings

        [NinjaScriptProperty]
        [Display(Name = "RMA Enabled", Description = "Enable RMA (Shift+Click) entry mode", Order = 1, GroupName = "7. RMA Settings")]
        public bool RMAEnabled { get; set; }

        [Range(1, 100)]
        public int RMAATRPeriod { get; set; }

        [Range(0.1, 5.0)]
        public double RMAStopATRMultiplier { get; set; }

        [Range(0.1, 5.0)]
        public double RMAT1ATRMultiplier { get; set; }

        [Range(0.1, 5.0)]
        public double RMAT2ATRMultiplier { get; set; }

        #endregion

        // V12 SIMA: "8. Copy Trading" group removed - use EnableSIMA in "13. SIMA Settings" instead

        #region Properties - TREND Settings (V8.2)

        [NinjaScriptProperty]
        [Display(Name = "TREND Enabled", Description = "Enable TREND (9/15 EMA) entry mode", Order = 1, GroupName = "9. TREND Settings")]
        public bool TRENDEnabled { get; set; }

        [Range(0.5, 3.0)]
        public double TRENDEntry1ATRMultiplier { get; set; }

        [Range(0.5, 3.0)]
        public double TRENDEntry2ATRMultiplier { get; set; }

        #endregion

        #region Properties - RETEST Settings (V8.4)

        [NinjaScriptProperty]
        [Display(Name = "RETEST Enabled", Description = "Enable RETEST entry mode (limit at OR High/Low)", Order = 1, GroupName = "10. RETEST Settings")]
        public bool RetestEnabled { get; set; }

        [Range(0.5, 3.0)]
        public double RetestATRMultiplier { get; set; }

        #endregion

        #region Properties - MOMO Settings (V8.6)

        [NinjaScriptProperty]
        [Display(Name = "MOMO Enabled", Description = "Enable MOMO (click-to-stop) entry mode", Order = 1, GroupName = "11. MOMO Settings")]
        public bool MOMOEnabled { get; set; }

        [Range(0.25, 5.0)]
        public double MOMOStopPoints { get; set; }

        #endregion

        #region Properties - FFMA Settings (V8.7)

        [NinjaScriptProperty]
        [Display(Name = "FFMA Enabled", Description = "Enable FFMA (mean reversion) entry mode", Order = 1, GroupName = "12. FFMA Settings")]
        public bool FFMAEnabled { get; set; }

        [Range(1.0, 50.0)]
        public double FFMAEMADistance { get; set; }

        [Range(50, 100)]
        public int FFMARSIOverbought { get; set; }

        [Range(0, 50)]
        public int FFMARSIOversold { get; set; }

        #endregion

        #region Properties - SIMA Settings (V12)

        [NinjaScriptProperty]
        [Display(Name = "Account Prefix", Description = "Only trade accounts containing this string (e.g., 'Apex')", Order = 1, GroupName = "13. SIMA Settings")]
        public string AccountPrefix { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable SIMA", Description = "When ON, commands broadcast to ALL matching accounts. When OFF, single-account mode.", Order = 2, GroupName = "13. SIMA Settings")]
        public bool EnableSIMA { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Path B (Fixed Brackets)", Description = "When ON, all trades use fixed stops/targets across the fleet.", Order = 5, GroupName = "13. SIMA Settings")]
        public bool EnablePathB { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Auto-Flatten Desync", Description = "When ON, Reaper will automatically flatten accounts that don't match expected position.", Order = 6, GroupName = "13. SIMA Settings")]
        public bool AutoFlattenDesync { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Path B Stop (Points)", Description = "Fixed stop distance for Path B trades", Order = 7, GroupName = "13. SIMA Settings")]
        [Range(0.25, 100.0)]
        public double PathBStopPoints { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Path B Target (Points)", Description = "Fixed target distance for Path B trades", Order = 8, GroupName = "13. SIMA Settings")]
        [Range(0.25, 100.0)]
        public double PathBTargetPoints { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Chase If Touch (Points)", Description = "Distance to chase limit orders if price touches them (set to 0 to disable)", Order = 9, GroupName = "13. SIMA Settings")]
        public string ChaseIfTouchPoints { get; set; }
#endregion

        #region Properties - Apex Compliance (V12)

        [NinjaScriptProperty]
        [Display(Name = "Enable Compliance Hub", Description = "Log performance and track Apex payout rules", Order = 1, GroupName = "14. Apex Compliance")]
        public bool EnableComplianceHub { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Consistency Threshold (%)", Description = "Maximum percentage a single day can contribute to total profit (Default 30%)", Order = 2, GroupName = "14. Apex Compliance")]
        [Range(10, 50)]
        public int ConsistencyThreshold { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Consistency Lock", Description = "Automatically prevent trading on accounts that hit their consistency limit for the day", Order = 3, GroupName = "14. Apex Compliance")]
        public bool EnableConsistencyLock { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Max Daily Profit ($) Cap", Description = "Stop trading an account for the day if it reaches this profit amount (to guard consistency)", Order = 4, GroupName = "14. Apex Compliance")]
        [Range(100, 10000)]
        public double MaxDailyProfitCap { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Payout Min Days", Description = "Minimum unique trading days required for payout eligibility", Order = 5, GroupName = "14. Apex Compliance")]
        [Range(1, 30)]
        public int PayoutMinTradingDays { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Payout Min Profit ($)", Description = "Minimum total profit required for payout eligibility", Order = 6, GroupName = "14. Apex Compliance")]
        [Range(0, 100000)]
        public double PayoutMinProfit { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Trailing Drawdown Limit ($)", Description = "Trailing drawdown threshold in dollars for buffer warnings", Order = 7, GroupName = "14. Apex Compliance")]
        [Range(0, 100000)]
        public double TrailingDrawdownLimit { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Trailing DD Warning Buffer ($)", Description = "Warn when within this buffer of trailing drawdown", Order = 8, GroupName = "14. Apex Compliance")]
        [Range(0, 1000)]
        public double TrailingDrawdownWarningBuffer { get; set; }

        #endregion

        #region Time Zone Converter

        public class TimeZoneConverter : TypeConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[] { "Eastern", "Central", "Mountain", "Pacific", "UTC" });
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                return value is string str ? str : base.ConvertFrom(context, culture, value);
            }
        }

        #endregion

        #region OnStateChange

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Universal OR Strategy V12 - Dev Build (Chart Visibility Fix)";
                Name = "UniversalORStrategyV12_Dev_FIX_CHART_VISIBILITY";
                Calculate = Calculate.OnPriceChange;  // CRITICAL FIX: Updates on every price tick for real-time trailing
                EntriesPerDirection = 10;
                EntryHandling = EntryHandling.UniqueEntries;
                IsExitOnSessionCloseStrategy = false;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                StartBehavior = StartBehavior.ImmediatelySubmit;
                TimeInForce = TimeInForce.Gtc;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                IsUnmanaged = true;
                TraceOrders = true;
                DrawOnPricePanel = true;
                IsOverlay = true;

                // Session defaults (NY Open)
                SessionStart = DateTime.Parse("09:30");
                SessionEnd = DateTime.Parse("16:00");
                ORTimeframe = ORTimeframeType.Minutes_5;
                SelectedTimeZone = "Eastern";

                // Risk defaults
                RiskPerTrade = 200;
                ReducedRiskPerTrade = 200;
                StopThresholdPoints = 5.0;
                MESMinimum = 1;
                MESMaximum = 30;
                MGCMinimum = 1;
                MGCMaximum = 15;

                // Stop defaults
                StopMultiplier = 0.5;
                MinimumStop = 1.0;
                MaximumStop = 15.0;  // V8.31: Increased from 8.0
                IpcPort = 5001;


                // v5.13: 4-Target System - T1=Fixed 1pt, T2-T4=ATR-based
                Target1FixedPoints = 1.0;   // T1 = Fixed 1 point quick scalp
                Target2Multiplier = 0.5;    // T2 = 0.5x ATR
                Target3Multiplier = 1.0;    // T3 = 1.0x ATR
                T1ContractPercent = 20;     // 20% quick scalp
                T2ContractPercent = 30;     // 30%
                T3ContractPercent = 30;     // 30%
                T4ContractPercent = 20;     // 20% runner

                // Trailing stop defaults
                BreakEvenTriggerPoints = 2.0;
                BreakEvenOffsetTicks = 2;
                Trail1TriggerPoints = 3.0;
                Trail1DistancePoints = 2.0;
                Trail2TriggerPoints = 4.0;
                Trail2DistancePoints = 1.5;
                Trail3TriggerPoints = 5.0;
                Trail3DistancePoints = 1.0;
                ManualBreakevenBuffer = 2;

                // Display
                ShowMidLine = true;
                BoxOpacity = 20;

                // RMA defaults
                RMAEnabled = true;
                RMAATRPeriod = 14;
                RMAStopATRMultiplier = 1.1;
                RMAT1ATRMultiplier = 0.5;
                RMAT2ATRMultiplier = 1.0;

                // V8.2: TREND defaults (V8.31: E1 now uses ATR from live EMA9)
                TRENDEnabled = true;
                TRENDEntry1ATRMultiplier = 1.1;   // V8.31: 1.1x ATR stop from live 9 EMA (was fixed 2pt)
                TRENDEntry2ATRMultiplier = 1.1;   // 1.1x ATR trailing for 15 EMA entry

                // V8.4: RETEST defaults
                RetestEnabled = true;
                RetestATRMultiplier = 1.1;        // 1.1x ATR for both stop and trail

                // V8.6: MOMO defaults
                MOMOEnabled = true;
                MOMOStopPoints = 0.5;             // Fixed 0.5pt stop for MOMO trades

                // V8.7: FFMA defaults
                FFMAEnabled = true;
                FFMAEMADistance = 10.0;           // 10 points from 9 EMA
                FFMARSIOverbought = 80;
                FFMARSIOversold = 20;

                // V12 SIMA defaults
                AccountPrefix = "Apex";
                EnableSIMA = false; // SAFETY: Default to OFF
                ReaperAuditEnabled = true;
                ReaperIntervalMs = 1000;          // 1 second audit cycle
                EnablePathB = false;
                AutoFlattenDesync = false;
                PathBStopPoints = 10.0;
                PathBTargetPoints = 15.0;
                ChaseIfTouchPoints = "0";

                // Apex Compliance defaults
                EnableComplianceHub = true;
                ConsistencyThreshold = 30;
                EnableConsistencyLock = false;
                MaxDailyProfitCap = 1500; // Default $1500 cap for consistency
                PayoutMinTradingDays = 10;
                PayoutMinProfit = 2600; // Common Apex 50K payout threshold (adjust per account)
                TrailingDrawdownLimit = 2500; // Common Apex 50K trailing DD
                TrailingDrawdownWarningBuffer = 200;
            }
            else if (State == State.Configure)
            {
                // V8.30: Initialize thread-safe collections
                // ConcurrentDictionary(concurrencyLevel, initialCapacity)
                activePositions = new ConcurrentDictionary<string, PositionInfo>(2, 4);
                entryOrders = new ConcurrentDictionary<string, Order>(2, 4);
                stopOrders = new ConcurrentDictionary<string, Order>(2, 4);
                target1Orders = new ConcurrentDictionary<string, Order>(2, 4);
                target2Orders = new ConcurrentDictionary<string, Order>(2, 4);
                target3Orders = new ConcurrentDictionary<string, Order>(2, 4);  // v5.13
                target4Orders = new ConcurrentDictionary<string, Order>(2, 4);  // v5.13

                // V8.2: TREND linked entries tracking
                // V8.30: Thread-safe dictionary
                linkedTRENDEntries = new ConcurrentDictionary<string, string>(2, 4);

                // V8.11: Initialize pending stop replacements tracking
                // V8.30: Thread-safe dictionary
                pendingStopReplacements = new ConcurrentDictionary<string, PendingStopReplacement>(2, 4);


                // IPC Queue
                ipcCommandQueue = new ConcurrentQueue<string>();

                // V12 SIMA: Initialize expected positions tracking
                expectedPositions = new ConcurrentDictionary<string, int>(2, 20); // Up to 20 accounts

                // V12.1: Initialize Compliance Hub
                string logsDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NinjaTrader 8", "SIMA_Logs");
                if (!System.IO.Directory.Exists(logsDir)) System.IO.Directory.CreateDirectory(logsDir);
                complianceLogPath = System.IO.Path.Combine(logsDir, "ApexPerformance.json");
                dailySummaryCsvPath = System.IO.Path.Combine(logsDir, "DailySummaries.csv");
                EnsureDailySummaryCsv();

                // Add 5-min data series for ATR (index 1)
                AddDataSeries(BarsPeriodType.Minute, 5);
            }
            else if (State == State.DataLoaded)
            {
                tickSize = Instrument.MasterInstrument.TickSize;
                pointValue = Instrument.MasterInstrument.PointValue;
                lastKnownPrice = 0; // V11 FIX: Reset price on load to prevent stale data (e.g. MES->MGC switch)

                string symbol = Instrument.MasterInstrument.Name;
                if (symbol.Contains("MES") || symbol.Contains("ES"))
                    minContracts = MESMinimum;
                else if (symbol.Contains("MGC") || symbol.Contains("GC"))
                    minContracts = MGCMinimum;
                else
                    minContracts = 1;

                // Initialize ATR indicator on 5-min bars (BarsArray[1])
                atrIndicator = ATR(BarsArray[1], RMAATRPeriod);

                // V8.2: Initialize EMA indicators for TREND trades
                // Using simple form - default is primary bars series
                ema9 = EMA(9);
                ema15 = EMA(15);
                // V11: Telemetry & Multi-Anchor EMAs
                ema30 = EMA(30);
                ema65 = EMA(65);
                ema200 = EMA(200);
                
                // V8.7: Initialize RSI for FFMA trades
                rsiIndicator = RSI(14, 3);
                
                // V8.2 DEBUG: Verify EMA periods are correct
                Print(string.Format("EMA INIT DEBUG: ema9.Period={0} ema15.Period={1}", ema9.Period, ema15.Period));

                ResetOR();

                Print(string.Format("UniversalORStrategy V12.11 | {0} | Tick: {1} | PV: ${2}", symbol, tickSize, pointValue));
                Print(string.Format("Session: {0} - {1} {2} | OR: {3} min",
                    SessionStart.ToString("HH:mm"), SessionEnd.ToString("HH:mm"), SelectedTimeZone, (int)ORTimeframe));
                Print(string.Format("OR Targets: T1={0}pt T2={1}xOR T3={2}xOR | Stop={3}xOR", Target1FixedPoints, Target2Multiplier, Target3Multiplier, StopMultiplier));
                Print(string.Format("RMA: Enabled={0} ATR({1}) Stop={2}xATR T1={3}xATR T2={4}xATR",
                    RMAEnabled, RMAATRPeriod, RMAStopATRMultiplier, RMAT1ATRMultiplier, RMAT2ATRMultiplier));
                Print("V12.9 REPAIRED: Definitive Chart-Click Fix + Logic Refresh");
                Print(string.Format("TREND: Enabled={0} E1Stop={1}xATR E2Trail={2}xATR", TRENDEnabled, TRENDEntry1ATRMultiplier, TRENDEntry2ATRMultiplier));
                Print(string.Format("FFMA: Enabled={0} Distance={1}pt RSI={2}/{3}", FFMAEnabled, FFMAEMADistance, FFMARSIOversold, FFMARSIOverbought));
                Print(string.Format("V12 SIMA: {0} | AccountPrefix: \"{1}\"", EnableSIMA ? "ENABLED - Fleet mode" : "DISABLED - Single account", AccountPrefix));

            }
            else if (State == State.Realtime)
            {
                // V12.2 HEADLESS SAFETY: Start core services even if ChartControl is null (for background execution)
                StartIpcServer();

                if (EnableSIMA)
                {
                    EnumerateApexAccounts();
                    if (ReaperAuditEnabled)
                        StartReaperAudit();
                }

                // V10.3: Subscribe to external signals for multi-chart sync
                SignalBroadcaster.OnExternalCommand += HandleExternalSignal;

                if (ChartControl != null)
                {
                    ChartControl.Dispatcher.InvokeAsync(() =>
                    {
                        AttachHotkeys();
                        AttachChartClickHandler();
                        Print("REALTIME - Hotkeys: L=Long, S=Short, Shift+Click=RMA, F=Flatten");
                    });
                }
            }
            else if (State == State.Terminated)
            {
                if (ChartControl != null)
                {
                    ChartControl.Dispatcher.InvokeAsync(() =>
                    {
                        DetachHotkeys();
                        DetachChartClickHandler();
                    });
                }

                // Stop IPC Server
                StopIpcServer();
                
                // V12 SIMA: Stop Reaper audit thread
                StopReaperAudit();
                
                // V12.7: Always unsubscribe from account updates (subscribed for fleet bracket management)
                if (EnableSIMA)
                {
                    foreach (Account acct in Account.All)
                    {
                        if (acct.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
                            acct.ExecutionUpdate -= OnAccountExecutionUpdate;
                    }
                }
                
                // V10.3: Unsubscribe
                SignalBroadcaster.OnExternalCommand -= HandleExternalSignal;

                // Clear references
                activePositions?.Clear();
                entryOrders?.Clear();
                stopOrders?.Clear();
                target1Orders?.Clear();
                target2Orders?.Clear();
                target3Orders?.Clear();  // v5.13
                target4Orders?.Clear();  // v5.13
                accountDailyProfit?.Clear();
                accountTotalProfit?.Clear();
                accountTradeCount?.Clear();
                accountDailyTradeCount?.Clear();
                accountEquityPeak?.Clear();
                accountMaxDrawdown?.Clear();
                accountTradingDays?.Clear();
                accountLastSummaryDate?.Clear();

            }
        }

        #endregion

        #region OnMarketData - V10.1: Process IPC on every tick for real-time responsiveness

        protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
        {
            // Only process on primary instrument
            if (marketDataUpdate.MarketDataType == MarketDataType.Last)
            {
                // Update last known price for real-time tracking
                lastKnownPrice = marketDataUpdate.Price;
                
                // Process IPC commands immediately on every tick
                // This ensures Remote App buttons work even outside session time
                ProcessIpcCommands();
            }
        }

        #endregion

        #region OnBarUpdate

        protected override void OnBarUpdate()
        {
            // Only process primary series
            if (BarsInProgress != 0) return;
            if (CurrentBar < 5) return;

            try
            {
                // Update last known price for UI events
                lastKnownPrice = Close[0];

                // V12.11: Daily summary roll-over (throttled)
                if (EnableComplianceHub)
                {
                    DateTime nowInZone = GetComplianceNow();
                    if ((nowInZone - lastDailySummaryCheck).TotalSeconds >= 30)
                    {
                        List<Account> complianceAccounts = GetComplianceAccounts();
                        if (complianceAccounts.Count > 0)
                            MaybeFinalizeDailySummaries(nowInZone, complianceAccounts);
                    }
                }

                // V8.21: Reduced log volume - OR buildings and updates are handled via DrawORBox and UpdateDisplay

                // Process IPC Commands
                ProcessIpcCommands();

                // CIT Logic
                ManageCIT();

                // V8.2 FIX: Process pending TREND entry (deferred from button click)
                if (pendingTRENDEntry)
                {
                    ExecuteTRENDEntry();
                }

                // Update ATR value from 5-min bars
                if (BarsArray[1] != null && BarsArray[1].Count > RMAATRPeriod)
                {
                    currentATR = atrIndicator[0];
                }

                // V11: Update Telemetry Cache (Thread-safe for UI)
                _ema9Val = ema9[0];
                _ema15Val = ema15[0];
                _ema30Val = ema30[0];
                _ema65Val = ema65[0];
                _ema200Val = ema200[0];
                _orHighVal = sessionHigh;
                _orLowVal = sessionLow;

                // CRITICAL FIX: Convert from LOCAL timezone (PC) to selected timezone
                DateTime barTimeInZone = ConvertToSelectedTimeZone(Time[0]);
                TimeSpan currentTime = barTimeInZone.TimeOfDay;
                TimeSpan sessionStartTime = SessionStart.TimeOfDay;
                TimeSpan sessionEndTime = SessionEnd.TimeOfDay;

                // Calculate OR end time based on session start + timeframe
                TimeSpan orEndTime = sessionStartTime.Add(TimeSpan.FromMinutes((int)ORTimeframe));

                // Detect if session crosses midnight (e.g. 21:00 to 07:00)
                bool sessionCrossesMidnight = sessionEndTime < sessionStartTime;

                // V11: Draw MNL Anchor Line if active
                if (currentRmaAnchor == RmaAnchorType.Manual && cachedMnlPrice > 0)
                {
                    Draw.HorizontalLine(this, "MNL_Line", cachedMnlPrice, Brushes.Magenta, DashStyleHelper.Dash, 2);
                }
                else
                {
                    RemoveDrawObject("MNL_Line");
                }
                
                // Smart reset logic - only reset at NEW SESSION START
                bool shouldReset = false;

                if (sessionCrossesMidnight)
                {
                    // For overnight sessions: only reset at session start
                    if (currentTime >= sessionStartTime && currentTime < sessionStartTime.Add(TimeSpan.FromMinutes(10)))
                    {
                        if (barTimeInZone.Date != lastResetDate)
                        {
                            shouldReset = true;
                        }
                    }
                }
                else
                {
                    // For regular sessions: reset when date changes AFTER session ends
                    if (barTimeInZone.Date != lastResetDate && currentTime >= sessionStartTime)
                    {
                        shouldReset = true;
                    }
                }

                if (shouldReset)
                {
                    ResetOR();
                    lastResetDate = barTimeInZone.Date;
                    Print(string.Format("Session Reset: {0} at {1} {2}",
                        barTimeInZone.Date.ToShortDateString(), currentTime, SelectedTimeZone));
                }

                // Build OR during window
                if (currentTime > sessionStartTime && currentTime <= orEndTime)
                {
                    if (!isInORWindow)
                    {
                        Print(string.Format("OR WINDOW START: {0} (Bar time in {1})",
                            barTimeInZone.ToString("MM/dd/yyyy HH:mm:ss"), SelectedTimeZone));
                    }

                    isInORWindow = true;
                    sessionHigh = Math.Max(sessionHigh, High[0]);
                    sessionLow = Math.Min(sessionLow, Low[0]);
                    sessionRange = sessionHigh - sessionLow;
                    sessionMid = (sessionHigh + sessionLow) / 2.0;

                    if (orStartDateTime == DateTime.MinValue)
                    {
                        orStartDateTime = Time[0];
                        sessionStartDateTime = Time[0];
                        orStartBarIndex = CurrentBar;
                        Print(string.Format("OR Start tracked - Bar {0}", CurrentBar));
                    }
                }

                // Mark OR complete when the last bar of the window closes
                if (currentTime >= orEndTime && !orComplete && orStartBarIndex > 0)
                {
                    isInORWindow = false;
                    orComplete = true;
                    orEndDateTime = Time[0];
                    orEndBarIndex = CurrentBar;

                    Print(string.Format("OR COMPLETE at {0}: H={1:F2} L={2:F2} M={3:F2} R={4:F2}",
                        barTimeInZone.ToString("HH:mm:ss"), sessionHigh, sessionLow, sessionMid, sessionRange));
                    Print(string.Format("OR Targets: T1=+{0:F2} T2=+{1:F2} Stop=-{2:F2}",
                        Target1FixedPoints, sessionRange * Target2Multiplier, CalculateORStopDistance()));

                    // V8.30: Always draw immediately when OR completes (important event)
                    DrawORBox();
                    lastDrawORBoxTime = DateTime.Now;
                }

                // Update box if OR complete
                bool inActiveSession = false;
                if (sessionCrossesMidnight)
                {
                    inActiveSession = (currentTime >= sessionStartTime || currentTime <= sessionEndTime);
                }
                else
                {
                    inActiveSession = (currentTime >= sessionStartTime && currentTime <= sessionEndTime);
                }

                // V8.30: Throttle DrawORBox updates to prevent chart saturation
                if (orComplete && sessionHigh != double.MinValue && inActiveSession)
                {
                    if ((DateTime.Now - lastDrawORBoxTime).TotalMilliseconds >= DRAW_ORBOX_THROTTLE_MS)
                    {
                        DrawORBox();
                        lastDrawORBoxTime = DateTime.Now;
                    }
                }

                // Position sync check
                SyncPositionState();

                // Manage trailing stops - NOW CALLED ON EVERY PRICE CHANGE!
                if (activePositions.Count > 0)
                {
                    ManageTrailingStops();
                    ManageCIT();
                }

                // V8.7: Check FFMA conditions when armed
                if (isFFMAModeArmed && FFMAEnabled)
                {
                    CheckFFMAConditions();
                }

                UpdateDisplay();
            }
            catch (Exception ex)
            {
                Print("ERROR OnBarUpdate: " + ex.Message);
            }
        }

        #endregion

        #region FFMA Entry Logic (V8.7)

        /// <summary>
        /// V8.7: Check FFMA conditions and execute on reversal candle
        /// SHORT: RSI > 80 + price 10+ pts above 9 EMA + RED candle
        /// LONG: RSI < 20 + price 10+ pts below 9 EMA + GREEN candle
        /// </summary>
        private void CheckFFMAConditions()
        {
            if (!isFFMAModeArmed || !FFMAEnabled) return;
            if (ema9 == null || rsiIndicator == null || currentATR <= 0) return;
            if (CurrentBar < 20) return;

            try
            {
                double ema9Value = ema9[0];
                double rsiValue = rsiIndicator[0];
                double currentPrice = Close[0];
                double distanceFromEMA = currentPrice - ema9Value;

                bool isGreenCandle = Close[0] > Open[0];
                bool isRedCandle = Close[0] < Open[0];

                // SHORT SETUP: RSI > 80 + Price far ABOVE EMA + RED reversal candle
                if (rsiValue > FFMARSIOverbought && distanceFromEMA >= FFMAEMADistance && isRedCandle)
                {
                    Print(string.Format("FFMA SHORT TRIGGERED: RSI={0:F1} > {1} | Distance={2:F2}pts > {3}pts | RED candle",
                        rsiValue, FFMARSIOverbought, distanceFromEMA, FFMAEMADistance));
                    ExecuteFFMAEntry(MarketPosition.Short);
                    return;
                }

                // LONG SETUP: RSI < 20 + Price far BELOW EMA + GREEN reversal candle
                if (rsiValue < FFMARSIOversold && distanceFromEMA <= -FFMAEMADistance && isGreenCandle)
                {
                    Print(string.Format("FFMA LONG TRIGGERED: RSI={0:F1} < {1} | Distance={2:F2}pts (below by {3}pts) | GREEN candle",
                        rsiValue, FFMARSIOversold, distanceFromEMA, FFMAEMADistance));
                    ExecuteFFMAEntry(MarketPosition.Long);
                    return;
                }
            }
            catch (Exception ex)
            {
                Print("ERROR CheckFFMAConditions: " + ex.Message);
            }
        }

        /// <summary>
        /// V8.7: Execute FFMA market order with entry candle high/low as stop
        /// Uses same target system as RMA (T1-T4)
        /// </summary>
        private void ExecuteFFMAEntry(MarketPosition direction)
        {
            try
            {
                double entryPrice = Close[0];  // Market order at current price
                
                // Stop at entry candle high (short) or low (long)
                double stopPrice = direction == MarketPosition.Long ? Low[0] : High[0];
                double stopDistance = Math.Min(Math.Abs(entryPrice - stopPrice), MaximumStop); // V8.31: Use MaximumStop

                // Validate stop distance
                if (stopDistance < tickSize * 2)
                {
                    Print(string.Format("FFMA: Stop too tight ({0:F2}pts) - using 2 tick minimum", stopDistance));
                    stopPrice = direction == MarketPosition.Long 
                        ? entryPrice - (tickSize * 2) 
                        : entryPrice + (tickSize * 2);
                    stopDistance = tickSize * 2;
                }

                // Calculate targets (same as RMA: T1 fixed, T2/T3 ATR-based, T4 runner)
                double target1Price = direction == MarketPosition.Long
                    ? entryPrice + Target1FixedPoints
                    : entryPrice - Target1FixedPoints;

                double target2Price = direction == MarketPosition.Long
                    ? entryPrice + (currentATR * RMAT1ATRMultiplier)
                    : entryPrice - (currentATR * RMAT1ATRMultiplier);
                double target3Price = direction == MarketPosition.Long
                    ? entryPrice + (currentATR * RMAT2ATRMultiplier)
                    : entryPrice - (currentATR * RMAT2ATRMultiplier);

                // Calculate position size based on ATR stop
                int contracts = CalculatePositionSize(stopDistance);

                // 4-target distribution
                int t1Qty, t2Qty, t3Qty, t4Qty;
                GetTargetDistribution(contracts, out t1Qty, out t2Qty, out t3Qty, out t4Qty);

                string timestamp = DateTime.Now.ToString("HHmmss");
                string signalName = direction == MarketPosition.Long ? "FFMALong" : "FFMAShort";
                string entryName = signalName + "_" + timestamp;

                PositionInfo pos = new PositionInfo
                {
                    SignalName = entryName,
                    Direction = direction,
                    TotalContracts = contracts,
                    T1Contracts = t1Qty,
                    T2Contracts = t2Qty,
                    T3Contracts = t3Qty,
                    T4Contracts = t4Qty,
                    RemainingContracts = contracts,
                    EntryPrice = entryPrice,
                    InitialStopPrice = stopPrice,
                    CurrentStopPrice = stopPrice,
                    Target1Price = target1Price,
                    Target2Price = target2Price,
                    Target3Price = target3Price,
                    EntryFilled = false,
                    T1Filled = false,
                    T2Filled = false,
                    T3Filled = false,
                    BracketSubmitted = false,
                    ExtremePriceSinceEntry = entryPrice,
                    CurrentTrailLevel = 0,
                    IsRMATrade = false,
                    IsFFMATrade = true
                };

                activePositions[entryName] = pos;

                // V12.13-D: Notify connected panel clients of position entry
                string syncMsg = string.Format("POSITION_ENTERED|FFMA|{0}", contracts);
                SendResponseToRemote(syncMsg);


                // Submit MARKET order (immediate execution)
                Order entryOrder = direction == MarketPosition.Long
                    ? SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Market, contracts, 0, 0, "", entryName)
                    : SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Market, contracts, 0, 0, "", entryName);

                entryOrders[entryName] = entryOrder;

                Print(string.Format("FFMA MARKET ORDER: {0} {1}@MARKET | Stop: {2:F2} (candle {3})",
                    signalName, contracts, stopPrice, direction == MarketPosition.Long ? "low" : "high"));
                Print(string.Format("FFMA TARGETS: T1:{0}@{1:F2} | T2:{2}@{3:F2} | T3:{4}@{5:F2} | T4:{6}@trail",
                    t1Qty, target1Price, t2Qty, target2Price, t3Qty, target3Price, t4Qty));

                // V12 SIMA: Dispatch to fleet (replaces legacy slave broadcast)
                if (EnableSIMA)
                {
                    ExecuteSmartDispatchEntry("FFMA", direction == MarketPosition.Long ? OrderAction.Buy : OrderAction.SellShort, contracts, entryPrice);
                }

                // Disarm FFMA after execution (one-shot)
                DeactivateFFMAMode();
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                Print("ERROR ExecuteFFMAEntry: " + ex.Message);
            }
        }

        private void DeactivateFFMAMode()
        {
            isFFMAModeArmed = false;
            UpdateFFMAButtonDisplay();
        }

        private void UpdateFFMAButtonDisplay()
        {
            // Legacy chart UI removed; no visual updates.
        }

        #endregion

        #region Drawing - Box Instead of Rays

        private void DrawORBox()
        {
            if (sessionHigh == double.MinValue || sessionLow == double.MaxValue) return;
            if (orStartDateTime == DateTime.MinValue || orEndDateTime == DateTime.MinValue) return;

            try
            {
                int areaOpacity = BoxOpacity;

                DateTime orStartInZone = ConvertToSelectedTimeZone(orStartDateTime);
                TimeSpan sessionStartTime = SessionStart.TimeOfDay;
                TimeSpan sessionEndTime = SessionEnd.TimeOfDay;

                // Detect overnight session (e.g., 21:00 to 16:00)
                bool sessionCrossesMidnight = sessionEndTime < sessionStartTime;

                // Calculate session end date
                DateTime sessionEndInZone;
                if (sessionCrossesMidnight)
                {
                    // Overnight session: end time is NEXT day
                    sessionEndInZone = new DateTime(
                        orStartInZone.Year,
                        orStartInZone.Month,
                        orStartInZone.Day,
                        sessionEndTime.Hours,
                        sessionEndTime.Minutes,
                        sessionEndTime.Seconds
                    ).AddDays(1);  // ADD ONE DAY for overnight sessions!
                }
                else
                {
                    // Same-day session: end time is same day
                    sessionEndInZone = new DateTime(
                        orStartInZone.Year,
                        orStartInZone.Month,
                        orStartInZone.Day,
                        sessionEndTime.Hours,
                        sessionEndTime.Minutes,
                        sessionEndTime.Seconds
                    );
                }

                TimeZoneInfo targetZone;
                switch (SelectedTimeZone)
                {
                    case "Eastern":
                        targetZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                        break;
                    case "Central":
                        targetZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                        break;
                    case "Mountain":
                        targetZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                        break;
                    case "Pacific":
                        targetZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                        break;
                    default:
                        targetZone = TimeZoneInfo.Local;
                        break;
                }

                DateTime boxEndTime = TimeZoneInfo.ConvertTime(sessionEndInZone, targetZone, TimeZoneInfo.Local);

                    Draw.Rectangle(this, "ORBox", false,
                    orStartDateTime, sessionHigh,
                    boxEndTime, sessionLow,
                    Brushes.DodgerBlue, Brushes.DodgerBlue, areaOpacity);

                if (ShowMidLine)
                {
                    Draw.Line(this, "ORMid", false,
                        orStartDateTime, sessionMid,
                        boxEndTime, sessionMid,
                        Brushes.Yellow, DashStyleHelper.Dash, 1);
                }
            }
            catch (Exception ex)
            {
                Print("ERROR DrawORBox: " + ex.Message);
            }
        }

        private void ResetOR()
        {
            sessionHigh = double.MinValue;
            sessionLow = double.MaxValue;
            sessionMid = 0;
            sessionRange = 0;
            isInORWindow = false;
            orComplete = false;
            orStartDateTime = DateTime.MinValue;
            orEndDateTime = DateTime.MinValue;
            sessionStartDateTime = DateTime.MinValue;
            orStartBarIndex = 0;
            orEndBarIndex = 0;

            RemoveDrawObjects();
        }

        #endregion

        #region Helpers

        private DateTime ConvertToSelectedTimeZone(DateTime localTime)
        {
            try
            {
                TimeZoneInfo targetZone;
                switch (SelectedTimeZone)
                {
                    case "Eastern":
                        targetZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                        break;
                    case "Central":
                        targetZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                        break;
                    case "Mountain":
                        targetZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
                        break;
                    case "Pacific":
                        targetZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                        break;
                    case "UTC":
                        targetZone = TimeZoneInfo.Utc;
                        break;
                    default:
                        return localTime;
                }

                return TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, targetZone);
            }
            catch (Exception ex)
            {
                Print("ERROR ConvertToSelectedTimeZone: " + ex.Message);
                return localTime;
            }
        }


        private void RemoveDrawObjects()
        {
            RemoveDrawObject("ORBox");
            RemoveDrawObject("ORMid");
        }

        #endregion

        #region OR Entry Logic

        private void ExecuteLong()
        {
            // V12.2 Hybrid Sync: Manual Interception
            if (isTosSyncMode)
            {
                if (isLongArmed)
                {
                    // DOUBLE-CLICK BYPASS: If already armed, fire immediately
                    Print("[SYNC] Double-Click Bypass Triggered -> Executing LONG immediately (No ToS Handshake)");
                    isLongArmed = false;
                    // Proceed to entry logic below
                }
                else
                {
                    isLongArmed = true;
                    isShortArmed = false; // Mutually exclusive for simplicity
                    lastArmedTime = DateTime.Now;
                    Print("[SYNC] LONG ENTRY ARMED. Waiting for ToS handshake signal...");
                    UpdateDisplay();
                    return;
                }
            }

            if (!orComplete || sessionRange == 0)
            {
                Print("Cannot enter Long - OR not ready");
                return;
            }

            double entryPrice = sessionHigh + (3 * tickSize);
            double stopDistance = CalculateORStopDistance();
            double stopPrice = entryPrice - stopDistance;

            EnterORPosition(MarketPosition.Long, entryPrice, stopPrice);
        }

        private void ExecuteShort()
        {
            // V12.2 Hybrid Sync: Manual Interception
            if (isTosSyncMode)
            {
                if (isShortArmed)
                {
                    // DOUBLE-CLICK BYPASS: If already armed, fire immediately
                    Print("[SYNC] Double-Click Bypass Triggered -> Executing SHORT immediately (No ToS Handshake)");
                    isShortArmed = false;
                    // Proceed to entry logic below
                }
                else
                {
                    isShortArmed = true;
                    isLongArmed = false; // Mutually exclusive
                    lastArmedTime = DateTime.Now;
                    Print("[SYNC] SHORT ENTRY ARMED. Waiting for ToS handshake signal...");
                    UpdateDisplay();
                    return;
                }
            }

            if (!orComplete || sessionRange == 0)
            {
                Print("Cannot enter Short - OR not ready");
                return;
            }

            double entryPrice = sessionLow - (3 * tickSize);
            double stopDistance = CalculateORStopDistance();
            double stopPrice = entryPrice + stopDistance;

            EnterORPosition(MarketPosition.Short, entryPrice, stopPrice);
        }

        private void EnterORPosition(MarketPosition direction, double entryPrice, double stopPrice)
        {
            try
            {
                // v5.13 FIX: Validate entry price before submitting StopMarket order
                // For LONG: entry must be ABOVE current price (breakout up)
                // For SHORT: entry must be BELOW current price (breakout down)
                // Use lastKnownPrice for real-time accuracy (Close[0] can be stale)
                double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
                if (direction == MarketPosition.Long && entryPrice <= currentPrice)
                {
                    Print(string.Format("OR ENTRY BLOCKED: Long entry {0:F2} already below market {1:F2} - too late for breakout",
                        entryPrice, currentPrice));
                    return;
                }
                if (direction == MarketPosition.Short && entryPrice >= currentPrice)
                {
                    Print(string.Format("OR ENTRY BLOCKED: Short entry {0:F2} already above market {1:F2} - too late for breakout",
                        entryPrice, currentPrice));
                    return;
                }

                double stopDistance = CalculateORStopDistance();
                int contracts = CalculatePositionSize(stopDistance);

                // v5.13: 4-target system with 20/30/30/20 split
                int t1Qty, t2Qty, t3Qty, t4Qty;
                GetTargetDistribution(contracts, out t1Qty, out t2Qty, out t3Qty, out t4Qty);

                Print(string.Format("POSITION SIZE: {0} contracts → T1:{1}(20%) T2:{2}(30%) T3:{3}(30%) T4:{4}(20%)", 
                    contracts, t1Qty, t2Qty, t3Qty, t4Qty));

                string signalName = direction == MarketPosition.Long ? "ORLong" : "ORShort";
                string timestamp = DateTime.Now.ToString("HHmmss");
                string entryName = signalName + "_" + timestamp;

                // v5.13: T1 = Fixed 1 point profit (quick scalp)
                double target1Price = direction == MarketPosition.Long
                    ? entryPrice + Target1FixedPoints
                    : entryPrice - Target1FixedPoints;

                // v5.13: T2 = 0.5x OR RANGE (using sessionRange, NOT ATR for OR trades)
                double target2Price = direction == MarketPosition.Long
                    ? entryPrice + (sessionRange * Target2Multiplier)
                    : entryPrice - (sessionRange * Target2Multiplier);

                // v5.13: T3 = 1.0x OR RANGE (using sessionRange, NOT ATR for OR trades)
                double target3Price = direction == MarketPosition.Long
                    ? entryPrice + (sessionRange * Target3Multiplier)
                    : entryPrice - (sessionRange * Target3Multiplier);

                PositionInfo pos = new PositionInfo
                {
                    SignalName = entryName,
                    Direction = direction,
                    TotalContracts = contracts,
                    T1Contracts = t1Qty,
                    T2Contracts = t2Qty,
                    T3Contracts = t3Qty,
                    T4Contracts = t4Qty,
                    RemainingContracts = contracts,
                    EntryPrice = entryPrice,
                    InitialStopPrice = stopPrice,
                    CurrentStopPrice = stopPrice,
                    Target1Price = target1Price,
                    Target2Price = target2Price,
                    Target3Price = target3Price,
                    EntryFilled = false,
                    T1Filled = false,
                    T2Filled = false,
                    T3Filled = false,
                    BracketSubmitted = false,
                    ExtremePriceSinceEntry = entryPrice,
                    CurrentTrailLevel = 0,
                    IsRMATrade = false
                };

                activePositions[entryName] = pos;

                // V12.13-D: Notify connected panel clients of position entry
                string syncMsg = string.Format("POSITION_ENTERED|OR|{0}", contracts);
                SendResponseToRemote(syncMsg);

                // Submit entry order as stop market (breakout entry)
                Order entryOrder = direction == MarketPosition.Long
                    ? SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.StopMarket, contracts, 0, entryPrice, "", entryName)
                    : SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.StopMarket, contracts, 0, entryPrice, "", entryName);

                entryOrders[entryName] = entryOrder;

                Print(string.Format("OR ENTRY ORDER: {0} {1}@{2:F2} | Stop: {3:F2} | OR Range: {4:F2}",
                    signalName, contracts, entryPrice, stopPrice, sessionRange));
                Print(string.Format("TARGETS: T1:{0}@{1:F2}(+{2:F2}pt) | T2:{3}@{4:F2}(+{5:F2}OR) | T3:{6}@{7:F2}(+{8:F2}OR) | T4:{9}@trail",
                    t1Qty, target1Price, Target1FixedPoints,
                    t2Qty, target2Price, sessionRange * Target2Multiplier,
                    t3Qty, target3Price, sessionRange * Target3Multiplier, t4Qty));

                // V12 SIMA: Dispatch to fleet (replaces legacy slave broadcast)
                if (EnableSIMA)
                {
                    ExecuteSmartDispatchEntry("OR", direction == MarketPosition.Long ? OrderAction.Buy : OrderAction.SellShort, contracts, entryPrice, OrderType.Limit);
                }

                UpdateDisplay();
            }
            catch (Exception ex)
            {
                Print("ERROR EnterORPosition: " + ex.Message);
            }
        }

        private double CalculateORStopDistance()
        {
            // v5.13: Use ATR for OR stop (same as RMA) instead of OR range
            if (currentATR <= 0) return MinimumStop;

            double calculatedStop = currentATR * StopMultiplier;  // 0.5x ATR
            return Math.Max(MinimumStop, Math.Min(calculatedStop, MaximumStop)); // V8.31: Use MaximumStop
        }

        #endregion

        // V12 SIMA: BroadcastEntrySignal and V8 Copy Trading region removed.
        // Trade copying is replaced by direct Account.All iteration in ExecuteSmartDispatchEntry.
        // SignalBroadcaster is retained ONLY for IPC app relay (HandleExternalSignal).

        // V11: Trend RMA (9/15 Split) Logic
        private void ExecuteTrendSplitEntry()
        {
            if (currentATR <= 0)
            {
                Print("Cannot execute TREND RMA - ATR not ready");
                return;
            }

            // Logic: EMA 9 vs EMA 15 Alignment determines Trend Direction
            // If EMA 9 > EMA 15 -> Uptrend -> Enter Long (Buy Limit at 9 & 15)
            // If EMA 9 < EMA 15 -> Downtrend -> Enter Short (Sell Limit at 9 & 15)
            
            double e9 = ema9[0];
            double e15 = ema15[0];
            
            bool isLongTrend = e9 > e15;
            
            // Calculate Position Sizes (Total Quantity split 1/3 and 2/3)
            // e.g. 3 contracts -> 1 at EMA9, 2 at EMA15
            int totalQty = DefaultQuantity;
            int qty9 = Math.Max(1, totalQty / 3);
            int qty15 = totalQty - qty9;
            
            string orderIdBase = "TRMA_" + DateTime.Now.Ticks;
            
            if (isLongTrend)
            {
                // Buy Limits at EMA 9 and 15
                // Note: If price is currently below EMA (deep pullback), these act as Stop Limits?
                // No, Limit orders buy at Price OR BETTER.
                // If we submit Limit Buy at EMA9 and Price < EMA9, it fills instantly at Market (Better Price).
                // If Price > EMA9, it rests as a pending Limit order.
                // This is correct behavior for Trend Pullbacks.
                
                EnterLongLimit(0, true, qty9, e9, orderIdBase + "_9");
                EnterLongLimit(0, true, qty15, e15, orderIdBase + "_15");
                Print(string.Format("Trend RMA LONG: {0} @ {1:F2}, {2} @ {3:F2}", qty9, e9, qty15, e15));
            }
            else
            {
                // Sell Limits at EMA 9 and 15
                // If Price > EMA (deep rally), Limit Sell fills instantly (Better Price).
                // If Price < EMA, it rests as pending Limit.
                
                EnterShortLimit(0, true, qty9, e9, orderIdBase + "_9");
                EnterShortLimit(0, true, qty15, e15, orderIdBase + "_15");
                Print(string.Format("Trend RMA SHORT: {0} @ {1:F2}, {2} @ {3:F2}", qty9, e9, qty15, e15));
            }
        }

        #region RMA Entry Logic

        // V11: Helper to get price of currently selected RMA Anchor
        private double GetRmaAnchorPrice()
        {
            switch (currentRmaAnchor)
            {
                case RmaAnchorType.Ema30: return ema30[0];
                case RmaAnchorType.Ema65: return ema65[0];
                case RmaAnchorType.Ema200: return ema200[0];
                case RmaAnchorType.OrHigh: return sessionHigh;
                case RmaAnchorType.OrLow: return sessionLow;
                case RmaAnchorType.Manual: 
                    // Use thread-safe cache
                    return cachedMnlPrice;
            }
            return ema65[0]; // Default
        }

        private void ExecuteRMAEntry(double clickPrice, MarketPosition? forcedDirection = null)
        {
            if (currentATR <= 0)
            {
                Print(string.Format("[RMA REJECT] ATR not ready. Check if 5-min bars (BarsArray[1]) are loaded and strategy has been running for {0} bars.", RMAATRPeriod));
                return;
            }

            try
            {
                // V11 FIX: Robust Check for Stale Price
                double currentPrice = Close[0];
                if (lastKnownPrice > 0)
                {
                     double diff = Math.Abs(lastKnownPrice - currentPrice);
                     if (diff / currentPrice < 0.05) currentPrice = lastKnownPrice;
                }

                // V11: Dynamic Anchor Direction Logic (UNUSED for Direction SafeGuard)
                double anchorPrice = GetRmaAnchorPrice();
                double refPrice = anchorPrice > 0 ? anchorPrice : currentPrice;
                
                MarketPosition direction;

                // V11 SAFEGUARD: Always enforce Limit Order Logic relative to Market
                // If Click > Market -> Short (Sell Limit Above)
                // If Click < Market -> Long (Buy Limit Below)
                // This prevents "Accidental Market Fills" if Anchor logic or stale data gets confused
                if (clickPrice > currentPrice) direction = MarketPosition.Short;
                else direction = MarketPosition.Long;
                
                // Only use forcedDirection if it MATCHES the Safe Logic (or if prices are super close)
                if (forcedDirection.HasValue && forcedDirection.Value != direction)
                {
                    Print(string.Format("RMA SAFEGUARD: Ignoring forced {0} because Click {1} vs Market {2} implies {3}", 
                        forcedDirection.Value, clickPrice, currentPrice, direction));
                }

                Print(string.Format("RMA Entry: Click={0:F2}, Market={1:F2}, Direction={2}", 
                    clickPrice, currentPrice, direction));

                // Calculate RMA stop and targets using ATR
                double stopDistance = currentATR * RMAStopATRMultiplier;
                stopDistance = Math.Min(stopDistance, 12.0); // V8.26: Increased Cap

                double entryPrice = clickPrice;
                double stopPrice = direction == MarketPosition.Long
                    ? entryPrice - stopDistance
                    : entryPrice + stopDistance;

                // v5.13: T1 = Fixed 1 point profit (same as OR, not ATR-based)
                double target1Price = direction == MarketPosition.Long
                    ? entryPrice + Target1FixedPoints
                    : entryPrice - Target1FixedPoints;

                // v5.13: T2 = 0.5x ATR (using RMA multiplier)
                double target2Price = direction == MarketPosition.Long
                    ? entryPrice + (currentATR * RMAT1ATRMultiplier)
                    : entryPrice - (currentATR * RMAT1ATRMultiplier);

                // v5.13: T3 = 1.0x ATR (using RMA multiplier)
                double target3Price = direction == MarketPosition.Long
                    ? entryPrice + (currentATR * RMAT2ATRMultiplier)
                    : entryPrice - (currentATR * RMAT2ATRMultiplier);

                int contracts = CalculatePositionSize(stopDistance);
                int t1Qty, t2Qty, t3Qty, t4Qty;
                GetTargetDistribution(contracts, out t1Qty, out t2Qty, out t3Qty, out t4Qty);

                string signalName = direction == MarketPosition.Long ? "RMALong" : "RMAShort";
                string timestamp = DateTime.Now.ToString("HHmmss");
                string entryName = signalName + "_" + timestamp;

                PositionInfo pos = new PositionInfo
                {
                    SignalName = entryName,
                    Direction = direction,
                    TotalContracts = contracts,
                    T1Contracts = t1Qty,
                    T2Contracts = t2Qty,
                    T3Contracts = t3Qty,
                    T4Contracts = t4Qty,
                    RemainingContracts = contracts,
                    EntryPrice = entryPrice,
                    InitialStopPrice = stopPrice,
                    CurrentStopPrice = stopPrice,
                    Target1Price = target1Price,
                    Target2Price = target2Price,
                    Target3Price = target3Price,
                    EntryFilled = false,
                    T1Filled = false,
                    T2Filled = false,
                    T3Filled = false,
                    BracketSubmitted = false,
                    ExtremePriceSinceEntry = entryPrice,
                    CurrentTrailLevel = 0,
                    IsRMATrade = true
                };

                // Submit LIMIT order at clicked price (RMA uses limit entries)
                Order entryOrder = direction == MarketPosition.Long
                    ? SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Limit, contracts, entryPrice, 0, "", entryName)
                    : SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Limit, contracts, entryPrice, 0, "", entryName);

                if (entryOrder != null)
                {
                    entryOrders[entryName] = entryOrder;
                    activePositions[entryName] = pos; // Only add to panel if order submitted
                    
                    // DEBUG: Visual Confirmation
                    Draw.Text(this, "Debug_" + entryName, "ORDER SUBMITTED", 0, entryPrice, Brushes.Yellow);
                    Draw.Line(this, "Line_" + entryName, 0, entryPrice, 10, entryPrice, Brushes.Yellow);
                }
                else
                {
                    Print("[ERROR] SubmitOrderUnmanaged returned NULL");
                    Draw.Text(this, "Debug_Fail_" + entryName, "ORDER FAILED", 0, entryPrice, Brushes.Red);
                }

                Print(string.Format("RMA ENTRY ORDER: {0} {1}@{2:F2} | ATR: {3:F2}", signalName, contracts, entryPrice, currentATR));
                Print(string.Format("RMA TARGETS: T1:{0}@{1:F2}(+{2:F2}pt) | T2:{3}@{4:F2} | T3:{5}@{6:F2} | T4:{7}@trail",
                    t1Qty, target1Price, Target1FixedPoints,
                    t2Qty, target2Price, t3Qty, target3Price, t4Qty));

                // V12 SIMA: Dispatch to fleet (replaces legacy slave broadcast)
                if (EnableSIMA)
                {
                    ExecuteSmartDispatchEntry("RMA", direction == MarketPosition.Long ? OrderAction.Buy : OrderAction.SellShort, contracts, entryPrice, OrderType.Limit);
                }

                // Deactivate RMA mode after entry (one-shot)
                DeactivateRMAMode();
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                Print("ERROR ExecuteRMAEntry: " + ex.Message);
            }
        }

        /// <summary>
        /// V10.1: Custom RMA entry for IPC commands - forces direction and uses specified price
        /// </summary>
        private void ExecuteRMAEntryCustom(double price, MarketPosition direction)
        {
            if (currentATR <= 0)
            {
                Print("IPC RMACustom Ignored: ATR not available");
                return;
            }

            try
            {
                double stopDistance = currentATR * RMAStopATRMultiplier;
                stopDistance = Math.Min(stopDistance, 12.0); // Cap

                double entryPrice = Instrument.MasterInstrument.RoundToTickSize(price);
                double stopPrice = direction == MarketPosition.Long
                    ? entryPrice - stopDistance
                    : entryPrice + stopDistance;

                double target1Price = direction == MarketPosition.Long
                    ? entryPrice + Target1FixedPoints
                    : entryPrice - Target1FixedPoints;

                double target2Price = direction == MarketPosition.Long
                    ? entryPrice + (currentATR * RMAT1ATRMultiplier)
                    : entryPrice - (currentATR * RMAT1ATRMultiplier);

                double target3Price = direction == MarketPosition.Long
                    ? entryPrice + (currentATR * RMAT2ATRMultiplier)
                    : entryPrice - (currentATR * RMAT2ATRMultiplier);

                int contracts = CalculatePositionSize(stopDistance);
                int t1Qty, t2Qty, t3Qty, t4Qty;
                GetTargetDistribution(contracts, out t1Qty, out t2Qty, out t3Qty, out t4Qty);

                string signalName = direction == MarketPosition.Long ? "IPCLong" : "IPCShort";
                string entryName = signalName + "_" + DateTime.Now.ToString("HHmmss");

                PositionInfo pos = new PositionInfo
                {
                    SignalName = entryName,
                    Direction = direction,
                    TotalContracts = contracts,
                    T1Contracts = t1Qty,
                    T2Contracts = t2Qty,
                    T3Contracts = t3Qty,
                    T4Contracts = t4Qty,
                    RemainingContracts = contracts,
                    EntryPrice = entryPrice,
                    InitialStopPrice = stopPrice,
                    CurrentStopPrice = stopPrice,
                    Target1Price = target1Price,
                    Target2Price = target2Price,
                    Target3Price = target3Price,
                    IsRMATrade = true
                };

                activePositions[entryName] = pos;

                // Execute as MARKET order for IPC commands to ensure immediate fill (V9 style)
                if (direction == MarketPosition.Long)
                    SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Market, contracts, 0, 0, "", entryName);
                else
                    SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Market, contracts, 0, 0, "", entryName);

                Print(string.Format("IPC EXEC: {0} {1} contracts at MKT (Ref: {2:F2})", direction, contracts, entryPrice));

                // V12.1: Smart Dispatch to SIMA Fleet
                if (EnableSIMA)
                {
                    ExecuteSmartDispatchEntry("RMA_IPC", direction == MarketPosition.Long ? OrderAction.Buy : OrderAction.SellShort, contracts, entryPrice, OrderType.Limit);
                }
            }
            catch (Exception ex)
            {
                Print("Error ExecuteRMAEntryCustom: " + ex.Message);
            }
        }

        private void ActivateRMAMode()
        {
            isRMAModeActive = true;
            UpdateRMAModeDisplay();
        }

        private void DeactivateRMAMode()
        {
            isRMAModeActive = false;
            isRMAButtonClicked = false;
            isRKeyHeld = false;
            UpdateRMAModeDisplay();

            // V12.14: Broadcast RMA deactivation to panel
            string deactivateConfig = string.Format(
                "CONFIG|OR|COUNT:{0};T1:{1};T2:{2};T3:{3};STR:{4};MAX:{5};",
                minContracts, Target1FixedPoints, Target2Multiplier, Target3Multiplier,
                StopMultiplier, MaxRiskAmount);
            SendResponseToRemote(deactivateConfig);
            Print("V12.14: DeactivateRMAMode - CONFIG broadcast sent");
        }

        #endregion

        #region MOMO Entry Logic (V8.6)

        /// <summary>
        /// V8.6: Execute MOMO (Momentum) trade using Stop Market orders
        /// OPPOSITE direction from RMA:
        /// - Click ABOVE price = Stop Market LONG (buy when price rises to click level)
        /// - Click BELOW price = Stop Market SHORT (sell when price drops to click level)
        /// Uses same targets/trails as RMA but with fixed 0.5pt stop
        /// </summary>
        private void ExecuteMOMOEntry(double clickPrice)
        {
            if (!MOMOEnabled)
            {
                Print("MOMO mode is disabled");
                return;
            }

            if (currentATR <= 0)
            {
                Print("Cannot execute MOMO entry - ATR not available yet");
                return;
            }

            try
            {
                // Use last known price from OnBarUpdate (Close[0] may be stale in UI events)
                double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];

                // MOMO Direction: OPPOSITE from RMA!
                // Click ABOVE current price = LONG (stop buy triggers when price rises)
                // Click BELOW current price = SHORT (stop sell triggers when price drops)
                MarketPosition direction;
                if (clickPrice > currentPrice)
                {
                    direction = MarketPosition.Long;
                    Print(string.Format("MOMO: Click above price ({0:F2} > {1:F2}) = LONG stop entry", clickPrice, currentPrice));
                }
                else
                {
                    direction = MarketPosition.Short;
                    Print(string.Format("MOMO: Click below price ({0:F2} < {1:F2}) = SHORT stop entry", clickPrice, currentPrice));
                }

                // MOMO uses FIXED 0.5pt stop (not ATR-based)
                double stopDistance = Math.Min(MOMOStopPoints, MaximumStop); // V8.31: Use MaximumStop

                double entryPrice = clickPrice;
                double stopPrice = direction == MarketPosition.Long
                    ? entryPrice - stopDistance
                    : entryPrice + stopDistance;

                // Same targets as RMA (ATR-based)
                // T1 = Fixed 1 point profit (same as RMA)
                double target1Price = direction == MarketPosition.Long
                    ? entryPrice + Target1FixedPoints
                    : entryPrice - Target1FixedPoints;

                // T2 = 0.5x ATR (using RMA multiplier)
                double target2Price = direction == MarketPosition.Long
                    ? entryPrice + (currentATR * RMAT1ATRMultiplier)
                    : entryPrice - (currentATR * RMAT1ATRMultiplier);

                // T3 = 1.0x ATR (using RMA multiplier)
                double target3Price = direction == MarketPosition.Long
                    ? entryPrice + (currentATR * RMAT2ATRMultiplier)
                    : entryPrice - (currentATR * RMAT2ATRMultiplier);

                int contracts = CalculatePositionSize(stopDistance);
                int t1Qty, t2Qty, t3Qty, t4Qty;
                GetTargetDistribution(contracts, out t1Qty, out t2Qty, out t3Qty, out t4Qty);

                string signalName = direction == MarketPosition.Long ? "MOMOLong" : "MOMOShort";
                string timestamp = DateTime.Now.ToString("HHmmss");
                string entryName = signalName + "_" + timestamp;

                PositionInfo pos = new PositionInfo
                {
                    SignalName = entryName,
                    Direction = direction,
                    TotalContracts = contracts,
                    T1Contracts = t1Qty,
                    T2Contracts = t2Qty,
                    T3Contracts = t3Qty,
                    T4Contracts = t4Qty,
                    RemainingContracts = contracts,
                    EntryPrice = entryPrice,
                    InitialStopPrice = stopPrice,
                    CurrentStopPrice = stopPrice,
                    Target1Price = target1Price,
                    Target2Price = target2Price,
                    Target3Price = target3Price,
                    EntryFilled = false,
                    T1Filled = false,
                    T2Filled = false,
                    T3Filled = false,
                    BracketSubmitted = false,
                    ExtremePriceSinceEntry = entryPrice,
                    CurrentTrailLevel = 0,
                    IsRMATrade = false,
                    IsMOMOTrade = true  // V8.6: Mark as MOMO trade
                };

                activePositions[entryName] = pos;

                // Submit STOP MARKET order at clicked price (MOMO uses stop entries, not limit!)
                Order entryOrder = direction == MarketPosition.Long
                    ? SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.StopMarket, contracts, 0, entryPrice, "", entryName)
                    : SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.StopMarket, contracts, 0, entryPrice, "", entryName);

                entryOrders[entryName] = entryOrder;

                Print(string.Format("MOMO ENTRY ORDER: {0} {1}@{2:F2} STOP | Stop: {3:F2}pt", signalName, contracts, entryPrice, stopDistance));
                Print(string.Format("MOMO TARGETS: T1:{0}@{1:F2}(+{2:F2}pt) | T2:{3}@{4:F2} | T3:{5}@{6:F2} | T4:{7}@trail",
                    t1Qty, target1Price, Target1FixedPoints,
                    t2Qty, target2Price, t3Qty, target3Price, t4Qty));

                // V12 SIMA: Dispatch to fleet (replaces legacy slave broadcast)
                if (EnableSIMA)
                {
                    ExecuteSmartDispatchEntry("MOMO", direction == MarketPosition.Long ? OrderAction.Buy : OrderAction.SellShort, contracts, entryPrice);
                }

                // Deactivate MOMO mode after entry (one-shot)
                DeactivateMOMOMode();
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                Print("ERROR ExecuteMOMOEntry: " + ex.Message);
            }
        }

        private void ActivateMOMOMode()
        {
            // Deactivate RMA if active (mutually exclusive)
            if (isRMAModeActive)
            {
                DeactivateRMAMode();
            }
            isMOMOModeActive = true;
            UpdateMOMOModeDisplay();
        }

        private void DeactivateMOMOMode()
        {
            isMOMOModeActive = false;
            UpdateMOMOModeDisplay();
        }

        private void UpdateMOMOModeDisplay()
        {
            // Legacy chart UI removed; no visual updates.
        }

        #endregion

        #region TREND Entry Logic (V8.2)

        /// <summary>
        /// V8.2: Execute TREND trade with dual limit orders
        /// Entry 1 (1/3) at 9 EMA with fixed 2pt stop
        /// Entry 2 (2/3) at 15 EMA with 1.1x ATR trailing stop off EMA15
        /// </summary>
        private void ExecuteTRENDEntry()
        {
            // V8.2 FIX: Only execute when on primary series (BarsInProgress=0)
            // This ensures we get correct EMA values from BarsArray[0]
            if (BarsInProgress != 0)
            {
                pendingTRENDEntry = true;
                Print("TREND entry deferred to next primary bar update (BarsInProgress=" + BarsInProgress + ")");
                return;
            }
            
            // Clear pending flag since we're executing now
            pendingTRENDEntry = false;
            
            if (!TRENDEnabled)
            {
                Print("TREND mode is disabled");
                return;
            }

            if (currentATR <= 0 || ema9 == null || ema15 == null)
            {
                Print("Cannot execute TREND entry - indicators not ready");
                return;
            }

            // V11: Trend RMA (9/15 Split) Mode
            if (isTrendRmaMode)
            {
                ExecuteTrendSplitEntry();
                return;
            }

            // V8.2: Ensure we have enough bars for EMA calculation
            if (CurrentBar < 20)
            {
                Print("Cannot execute TREND entry - not enough bars (CurrentBar=" + CurrentBar + ")");
                return;
            }
            try
            {
                // V8.2: Simple check for enough bars
                if (CurrentBar < 20)
                {
                    Print("Cannot execute TREND entry - not enough bars (CurrentBar=" + CurrentBar + ")");
                    return;
                }

                // Get current tick price for direction determination
                double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
                
                // V8.2: Use stored EMA instances (now guaranteed BarsInProgress=0)
                if (ema9 == null || ema15 == null)
                {
                    Print("Cannot execute TREND entry - EMA indicators not initialized");
                    return;
                }
                
                // V8.10: Use [0] (live tick) for real-time EMA values since Calculate.OnPriceChange updates EMAs on every tick
                double ema9Value = ema9[0];
                double ema15Value = ema15[0];

                // V8.10 DEBUG
                Print(string.Format("TREND DEBUG: ema9[0]={0:F2} ema15[0]={1:F2} Price={2:F2}", ema9Value, ema15Value, currentPrice));
                Print(string.Format("TREND DEBUG: Close[0]={0:F2} CurrentBar={1} BarsInProgress={2}", 
                    Close[0], CurrentBar, BarsInProgress));

                // Sanity check: EMAs should be different
                if (Math.Abs(ema9Value - ema15Value) < tickSize * 2)
                {
                    Print(string.Format("WARNING: EMAs very close ({0:F2} vs {1:F2})", ema9Value, ema15Value));
                }

                // Direction: EMA below price = LONG (buying pullback), EMA above = SHORT
                MarketPosition direction;
                if (ema9Value < currentPrice)
                {
                    direction = MarketPosition.Long;
                    Print(string.Format("TREND: EMA9 below price ({0:F2} < {1:F2}) = LONG setup", ema9Value, currentPrice));
                }
                else
                {
                    direction = MarketPosition.Short;
                    Print(string.Format("TREND: EMA9 above price ({0:F2} > {1:F2}) = SHORT setup", ema9Value, currentPrice));
                }

                // V8.31: Both E1 and E2 now use ATR-based stops from live EMAs
                double e1MultTrend = isTrendRmaMode ? RMAStopATRMultiplier : TRENDEntry1ATRMultiplier;
                double e2MultTrend = isTrendRmaMode ? RMAStopATRMultiplier : TRENDEntry2ATRMultiplier;
                
                double e1StopDist = Math.Min(currentATR * e1MultTrend, MaximumStop); // V8.31: ATR-based, MaxStop cap
                double e2StopDist = Math.Min(currentATR * e2MultTrend, MaximumStop); // V8.31: MaxStop cap
                
                // Weighted average stop distance for the group
                double weightedStopDist = (e1StopDist * (1.0/3.0)) + (e2StopDist * (2.0/3.0));
                
                int totalContracts = CalculatePositionSize(weightedStopDist);

                // Split: 1/3 at 9 EMA, 2/3 at 15 EMA
                int entry1Qty = (int)Math.Ceiling(totalContracts / 3.0);
                int entry2Qty = totalContracts - entry1Qty;

                if (entry1Qty < 1) entry1Qty = 1;
                if (entry2Qty < 1) entry2Qty = 1;
                
                // Final validation: totalContracts = sum of entries
                totalContracts = entry1Qty + entry2Qty;

                Print(string.Format("TREND RISK: Risk=${0} | E1Stop={1:F2} | E2Stop={2:F2} | WeightedDist={3:F2} | TotalQty={4}",
                    MaxRiskAmount, e1StopDist, e2StopDist, weightedStopDist, totalContracts));
                Print(string.Format("TREND SPLIT: E1Qty={0} (1/3) | E2Qty={1} (2/3)", entry1Qty, entry2Qty));

                string timestamp = DateTime.Now.ToString("HHmmss");
                string trendGroupId = "TREND_" + timestamp;
                string entry1Name = trendGroupId + "_E1";
                string entry2Name = trendGroupId + "_E2";

                // V8.31: ENTRY 1: 1/3 at 9 EMA with ATR-based stop from live EMA9
                double entry1Price = ema9Value;
                double e1AtrStop = currentATR * e1MultTrend;  // V8.31: ATR-based stop
                double stop1Price = direction == MarketPosition.Long
                    ? ema9Value - e1AtrStop  // V8.31: Stop is 1.1x ATR below live EMA9
                    : ema9Value + e1AtrStop; // V8.31: Stop is 1.1x ATR above live EMA9

                // ENTRY 2: 2/3 at 15 EMA with ATR trailing stop
                double entry2Price = ema15Value;
                double stop2Price = direction == MarketPosition.Long
                    ? ema15Value - (currentATR * e2MultTrend)
                    : ema15Value + (currentATR * e2MultTrend);

                // Create position info for Entry 1
                PositionInfo pos1 = CreateTRENDPosition(entry1Name, direction, entry1Price, stop1Price, 
                    entry1Qty, true, trendGroupId, isTrendRmaMode);
                activePositions[entry1Name] = pos1;

                // Create position info for Entry 2
                PositionInfo pos2 = CreateTRENDPosition(entry2Name, direction, entry2Price, stop2Price,
                    entry2Qty, false, trendGroupId, isTrendRmaMode);
                activePositions[entry2Name] = pos2;

                // Link the entries together
                linkedTRENDEntries[entry1Name] = entry2Name;
                linkedTRENDEntries[entry2Name] = entry1Name;

                // Submit Entry 1 limit order
                Order entryOrder1 = direction == MarketPosition.Long
                    ? SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Limit, entry1Qty, entry1Price, 0, "", entry1Name)
                    : SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Limit, entry1Qty, entry1Price, 0, "", entry1Name);
                entryOrders[entry1Name] = entryOrder1;

                // Submit Entry 2 limit order
                Order entryOrder2 = direction == MarketPosition.Long
                    ? SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Limit, entry2Qty, entry2Price, 0, "", entry2Name)
                    : SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Limit, entry2Qty, entry2Price, 0, "", entry2Name);
                entryOrders[entry2Name] = entryOrder2;

                Print(string.Format("TREND ORDERS PLACED: {0} Total={1} contracts",
                    direction == MarketPosition.Long ? "LONG" : "SHORT", totalContracts));
                Print(string.Format("  E1: {0}@{1:F2} (EMA9) | Stop: {2:F2} ({3}xATR from EMA9)",
                    entry1Qty, ema9Value, stop1Price, TRENDEntry1ATRMultiplier));
                Print(string.Format("  E2: {0}@{1:F2} (EMA15) | Stop: {2:F2} ({3}xATR trail)",
                    entry2Qty, ema15Value, stop2Price, TRENDEntry2ATRMultiplier));

                // V12.1: Smart Dispatch to SIMA Fleet
                if (EnableSIMA)
                {
                    // For Trend trades, followers get the full totalContracts qty split by the dispatcher
                    ExecuteSmartDispatchEntry("TREND", direction == MarketPosition.Long ? OrderAction.Buy : OrderAction.SellShort, totalContracts, currentPrice);
                }

                // Deactivate TREND mode after placing orders
                DeactivateTRENDMode();
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                Print("ERROR ExecuteTRENDEntry: " + ex.Message);
            }
        }

        private PositionInfo CreateTRENDPosition(string entryName, MarketPosition direction, 
            double entryPrice, double stopPrice, int contracts, bool isEntry1, string groupId, bool isRma)
        {
            // V8.2 FIX: TREND uses same multi-target system as RMA
            // T1: 1pt fixed, T2: 0.5x ATR, T3: 1x ATR, T4: Runner
            double target1Price = direction == MarketPosition.Long
                ? entryPrice + Target1FixedPoints
                : entryPrice - Target1FixedPoints;
            double target2Price = direction == MarketPosition.Long
                ? entryPrice + (currentATR * RMAT1ATRMultiplier)
                : entryPrice - (currentATR * RMAT1ATRMultiplier);
            double target3Price = direction == MarketPosition.Long
                ? entryPrice + (currentATR * RMAT2ATRMultiplier)
                : entryPrice - (currentATR * RMAT2ATRMultiplier);

            // V8.2 FIX: Calculate contract distribution (same as RMA)
            int t1Qty, t2Qty, t3Qty, t4Qty;

            if (contracts == 1)
            {
                t1Qty = 1; t2Qty = 0; t3Qty = 0; t4Qty = 0;
            }
            else if (contracts == 2)
            {
                t1Qty = 1; t2Qty = 0; t3Qty = 0; t4Qty = 1;
            }
            else if (contracts == 3)
            {
                t1Qty = 1; t2Qty = 1; t3Qty = 0; t4Qty = 1;
            }
            else if (contracts == 4)
            {
                t1Qty = 1; t2Qty = 1; t3Qty = 1; t4Qty = 1;
            }
            else
            {
                // 5+ contracts: Use percentage split
                t1Qty = (int)Math.Floor(contracts * T1ContractPercent / 100.0);
                t2Qty = (int)Math.Floor(contracts * T2ContractPercent / 100.0);
                t3Qty = (int)Math.Floor(contracts * T3ContractPercent / 100.0);
                t4Qty = contracts - t1Qty - t2Qty - t3Qty;

                if (t1Qty < 1) { t1Qty = 1; t4Qty = contracts - t1Qty - t2Qty - t3Qty; }
                if (t2Qty < 1) { t2Qty = 1; t4Qty = contracts - t1Qty - t2Qty - t3Qty; }
                if (t3Qty < 1) { t3Qty = 1; t4Qty = contracts - t1Qty - t2Qty - t3Qty; }
                if (t4Qty < 1) t4Qty = 1;
            }

            Print(string.Format("TREND POSITION: {0} contracts → T1:{1} T2:{2} T3:{3} Runner:{4}", 
                contracts, t1Qty, t2Qty, t3Qty, t4Qty));

            return new PositionInfo
            {
                SignalName = entryName,
                Direction = direction,
                TotalContracts = contracts,
                T1Contracts = t1Qty,
                T2Contracts = t2Qty,
                T3Contracts = t3Qty,
                T4Contracts = t4Qty,
                RemainingContracts = contracts,
                EntryPrice = entryPrice,
                InitialStopPrice = stopPrice,
                CurrentStopPrice = stopPrice,
                Target1Price = target1Price,
                Target2Price = target2Price,
                Target3Price = target3Price,
                EntryFilled = false,
                T1Filled = false,
                T2Filled = false,
                T3Filled = false,
                BracketSubmitted = false,
                ExtremePriceSinceEntry = entryPrice,
                CurrentTrailLevel = 0,
                IsRMATrade = isRma,
                IsTRENDTrade = true,
                IsTRENDEntry1 = isEntry1,
                IsTRENDEntry2 = !isEntry1,
                LinkedTRENDGroup = groupId
            };
        }

        // V8.4: Execute RETEST entry - auto-detects direction based on price vs OR Mid
        private void ExecuteRetestEntry()
        {
            if (!RetestEnabled)
            {
                Print("RETEST mode is disabled");
                return;
            }

            if (!orComplete)
            {
                Print("Cannot execute RETEST - OR not complete yet");
                return;
            }

            if (currentATR <= 0)
            {
                Print("Cannot execute RETEST entry - ATR not available yet");
                return;
            }

            try
            {
                // Use last known price for direction determination
                double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];

                // Auto-detect direction: Price > OR Mid = LONG, Price < OR Mid = SHORT
                MarketPosition direction;
                double entryPrice;

                if (currentPrice > sessionMid)
                {
                    direction = MarketPosition.Long;
                    entryPrice = sessionHigh;  // Entry at OR High (NO buffer)
                    Print(string.Format("RETEST: Price above OR Mid ({0:F2} > {1:F2}) = LONG at OR High {2:F2}",
                        currentPrice, sessionMid, entryPrice));
                }
                else
                {
                    direction = MarketPosition.Short;
                    entryPrice = sessionLow;   // Entry at OR Low (NO buffer)
                    Print(string.Format("RETEST: Price below OR Mid ({0:F2} < {1:F2}) = SHORT at OR Low {2:F2}",
                        currentPrice, sessionMid, entryPrice));
                }

                // Calculate stop and targets using ATR
                double multToUse = isRetestRmaMode ? RMAStopATRMultiplier : RetestATRMultiplier;
                double stopDistance = Math.Min(currentATR * multToUse, MaximumStop); // V8.31: Use MaximumStop

                double stopPrice = direction == MarketPosition.Long
                    ? entryPrice - stopDistance
                    : entryPrice + stopDistance;

                // T1 = Fixed 1 point profit (same as RMA)
                double target1Price = direction == MarketPosition.Long
                    ? entryPrice + Target1FixedPoints
                    : entryPrice - Target1FixedPoints;

                // T2 = 0.5x ATR
                double target2Price = direction == MarketPosition.Long
                    ? entryPrice + (currentATR * RMAT1ATRMultiplier)
                    : entryPrice - (currentATR * RMAT1ATRMultiplier);

                // T3 = 1.0x ATR
                double target3Price = direction == MarketPosition.Long
                    ? entryPrice + (currentATR * RMAT2ATRMultiplier)
                    : entryPrice - (currentATR * RMAT2ATRMultiplier);

                int contracts = CalculatePositionSize(stopDistance);
                int t1Qty, t2Qty, t3Qty, t4Qty;
                GetTargetDistribution(contracts, out t1Qty, out t2Qty, out t3Qty, out t4Qty);

                string signalName = direction == MarketPosition.Long ? "RetestLong" : "RetestShort";
                string timestamp = DateTime.Now.ToString("HHmmss");
                string entryName = signalName + "_" + timestamp;

                PositionInfo pos = new PositionInfo
                {
                    SignalName = entryName,
                    Direction = direction,
                    TotalContracts = contracts,
                    T1Contracts = t1Qty,
                    T2Contracts = t2Qty,
                    T3Contracts = t3Qty,
                    T4Contracts = t4Qty,
                    RemainingContracts = contracts,
                    EntryPrice = entryPrice,
                    InitialStopPrice = stopPrice,
                    CurrentStopPrice = stopPrice,
                    Target1Price = target1Price,
                    Target2Price = target2Price,
                    Target3Price = target3Price,
                    EntryFilled = false,
                    T1Filled = false,
                    T2Filled = false,
                    T3Filled = false,
                    BracketSubmitted = false,
                    ExtremePriceSinceEntry = entryPrice,
                    CurrentTrailLevel = 0,
                    IsRMATrade = isRetestRmaMode,
                    IsTRENDTrade = false,
                    IsRetestTrade = true,              // V8.4: Mark as retest trade
                    RetestTrailActivated = false       // V8.4: Trail not activated yet
                };

                activePositions[entryName] = pos;

                // Submit LIMIT order at OR High/Low (NO buffer)
                Order entryOrder = direction == MarketPosition.Long
                    ? SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Limit, contracts, entryPrice, 0, "", entryName)
                    : SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Limit, contracts, entryPrice, 0, "", entryName);

                entryOrders[entryName] = entryOrder;

                Print(string.Format("RETEST ENTRY ORDER: {0} {1}@{2:F2} | ATR: {3:F2}", signalName, contracts, entryPrice, currentATR));
                Print(string.Format("RETEST STOP: {0:F2} ({1:F2}x ATR = {2:F2}pts)",
                    stopPrice, RetestATRMultiplier, stopDistance));
                Print(string.Format("RETEST TARGETS: T1:{0}@{1:F2}(+{2:F2}pt) | T2:{3}@{4:F2} | T3:{5}@{6:F2} | T4:{7}@trail",
                    t1Qty, target1Price, Target1FixedPoints,
                    t2Qty, target2Price, t3Qty, target3Price, t4Qty));

                // V12.1: Smart Dispatch to SIMA Fleet
                if (EnableSIMA)
                {
                    ExecuteSmartDispatchEntry("RETEST", direction == MarketPosition.Long ? OrderAction.Buy : OrderAction.SellShort, contracts, entryPrice);
                }

                // Deactivate RETEST mode after entry (one-shot)
                DeactivateRetestMode();
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                Print("ERROR ExecuteRetestEntry: " + ex.Message);
            }
        }

        private void ActivateTRENDMode()
        {
            isTRENDModeActive = true;
            UpdateTRENDModeDisplay();
        }

        private void DeactivateTRENDMode()
        {
            isTRENDModeActive = false;
            UpdateTRENDModeDisplay();
        }

        private void UpdateTRENDModeDisplay()
        {
            // Legacy chart UI removed; no visual updates.
        }

        // V8.4: RETEST mode management
        private void ActivateRetestMode()
        {
            isRetestModeActive = true;
            UpdateRetestModeDisplay();
        }

        private void DeactivateRetestMode()
        {
            isRetestModeActive = false;
            UpdateRetestModeDisplay();
        }

        private void UpdateRetestModeDisplay()
        {
            // Legacy chart UI removed; no visual updates.
        }

        #endregion

        #region Order Management

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice,
            int quantity, int filled, double averageFillPrice, OrderState orderState,
            DateTime time, ErrorCode error, string nativeError)
        {
            try
            {
                string orderName = order.Name;

                // Entry filled
                if (entryOrders.Values.Contains(order) && orderState == OrderState.Filled)
                {
                    // V8.30: Thread-safe snapshot iteration
                    foreach (var kvp in activePositions.ToArray())
                    {
                        string entryName = kvp.Key;
                        PositionInfo pos = kvp.Value;

                        // V8.30: Verify position still exists
                        if (!activePositions.ContainsKey(entryName)) continue;

                        // V8.30: Thread-safe check
                        if (entryOrders.TryGetValue(entryName, out var entryOrder) && entryOrder == order && !pos.EntryFilled)
                        {
                            pos.EntryFilled = true;

                            // Store intended entry price for slippage calculation
                            double intendedEntryPrice = pos.EntryPrice;

                            string tradeType = pos.IsRMATrade ? "RMA" : "OR";
                            if (pos.IsMOMOTrade) tradeType = "MOMO"; // V8.22: Logging
                            if (pos.IsFFMATrade) tradeType = "FFMA";
                            if (pos.IsTRENDTrade) tradeType = "TREND";
                            if (pos.IsRetestTrade) tradeType = "RETEST";

                            Print(string.Format("{0} ENTRY FILLED: {1} {2} @ {3:F2} (intended: {4:F2})",
                                tradeType,
                                pos.Direction == MarketPosition.Long ? "LONG" : "SHORT",
                                pos.TotalContracts,
                                averageFillPrice,
                                intendedEntryPrice));

                            // V8.22: UNIVERSAL STOP CAP FIX
                            // Determine the intended stop distance
                            double stopDistance = 0;

                            if (pos.IsRMATrade)
                            {
                                // For RMA, use current ATR to be precise
                                Print(string.Format("🔍 DIAGNOSTIC: RMA Entry Filled. Raw ATR used: {0:F4} | Multiplier: {1:F2} | Calc Stop: {2:F4} pts", 
                                    currentATR, RMAStopATRMultiplier, currentATR * RMAStopATRMultiplier));
                                stopDistance = currentATR * RMAStopATRMultiplier;
                                
                                // Recalculate RMA targets based on fill
                                // v5.13 FIX: T1 uses FIXED points, T2/T3 use ATR
                                double t2Distance = currentATR * RMAT1ATRMultiplier;  // 0.5x ATR
                                double t3Distance = currentATR * RMAT2ATRMultiplier;  // 1.0x ATR

                                // T1 = Fixed 1pt (NOT ATR-based)
                                pos.Target1Price = pos.Direction == MarketPosition.Long
                                    ? averageFillPrice + Target1FixedPoints
                                    : averageFillPrice - Target1FixedPoints;
                                // T2 = 0.5x ATR
                                pos.Target2Price = pos.Direction == MarketPosition.Long
                                    ? averageFillPrice + t2Distance
                                    : averageFillPrice - t2Distance;
                                // T3 = 1.0x ATR
                                pos.Target3Price = pos.Direction == MarketPosition.Long
                                    ? averageFillPrice + t3Distance
                                    : averageFillPrice - t3Distance;
                            }
                            else
                            {
                                // For other trades, use the distance from the intended setup
                                stopDistance = Math.Abs(pos.InitialStopPrice - intendedEntryPrice);
                            }

                            // GLOBAL SAFETY CAP: Absolutely NO stop > 8.0 points
                            double originalDist = stopDistance;
                            stopDistance = Math.Min(stopDistance, 12.0);
                            
                            if (stopDistance < originalDist)
                            {
                                Print(string.Format("CRITICAL: {0} Stop capped at 12.0 pts (Calculated: {1:F2} pts)", 
                                    tradeType, originalDist));
                            }

                            // Re-anchor stop to ACTUAL fill price
                            pos.InitialStopPrice = pos.Direction == MarketPosition.Long
                                ? averageFillPrice - stopDistance
                                : averageFillPrice + stopDistance;
                            pos.CurrentStopPrice = pos.InitialStopPrice;

                            if (Math.Abs(averageFillPrice - intendedEntryPrice) > tickSize)
                            {
                                Print(string.Format("{0} PRICES ADJUSTED for fill slippage: Stop={1:F2} (Dist={2:F2})",
                                    tradeType, pos.InitialStopPrice, stopDistance));
                            }

                            // Update to actual fill price
                            pos.EntryPrice = averageFillPrice;
                            pos.ExtremePriceSinceEntry = averageFillPrice;

                            SubmitBracketOrders(entryName, pos);
                        }
                    }
                }

                // Target 1 filled
                if (target1Orders.Values.Contains(order) && orderState == OrderState.Filled)
                {
                    // V8.30: Thread-safe snapshot iteration
                    foreach (var kvp in activePositions.ToArray())
                    {
                        if (!activePositions.ContainsKey(kvp.Key)) continue;
                        if (target1Orders.TryGetValue(kvp.Key, out var t1Order) && t1Order == order)
                        {
                            PositionInfo pos = kvp.Value;
                            pos.T1Filled = true;
                            pos.RemainingContracts -= pos.T1Contracts;
                            // V8.11: Added entry name to logging
                            Print(string.Format("T1 FILLED ({0}): {1} contracts @ {2:F2} | Remaining: {3}",
                                kvp.Key, pos.T1Contracts, averageFillPrice, pos.RemainingContracts));

                            // Update stop quantity
                            UpdateStopQuantity(kvp.Key, pos);
                            break;
                        }
                    }
                }

                // Target 2 filled
                if (target2Orders.Values.Contains(order) && orderState == OrderState.Filled)
                {
                    // V8.30: Thread-safe snapshot iteration
                    foreach (var kvp in activePositions.ToArray())
                    {
                        if (!activePositions.ContainsKey(kvp.Key)) continue;
                        if (target2Orders.TryGetValue(kvp.Key, out var t2Order) && t2Order == order)
                        {
                            PositionInfo pos = kvp.Value;
                            pos.T2Filled = true;
                            pos.RemainingContracts -= pos.T2Contracts;
                            // V8.11: Added entry name to logging
                            Print(string.Format("T2 FILLED ({0}): {1} contracts @ {2:F2} | Remaining: {3}",
                                kvp.Key, pos.T2Contracts, averageFillPrice, pos.RemainingContracts));

                            // Update stop quantity
                            UpdateStopQuantity(kvp.Key, pos);
                            break;
                        }
                    }
                }

                // v5.13: Target 3 filled
                if (target3Orders.Values.Contains(order) && orderState == OrderState.Filled)
                {
                    // V8.30: Thread-safe snapshot iteration
                    foreach (var kvp in activePositions.ToArray())
                    {
                        if (!activePositions.ContainsKey(kvp.Key)) continue;
                        if (target3Orders.TryGetValue(kvp.Key, out var t3Order) && t3Order == order)
                        {
                            PositionInfo pos = kvp.Value;
                            pos.T3Filled = true;
                            pos.RemainingContracts -= pos.T3Contracts;
                            // V8.11: Added entry name to logging
                            Print(string.Format("T3 FILLED ({0}): {1} contracts @ {2:F2} | Remaining: {3} (T4 runner)",
                                kvp.Key, pos.T3Contracts, averageFillPrice, pos.RemainingContracts));

                            // Update stop quantity - only T4 runner remains
                            UpdateStopQuantity(kvp.Key, pos);
                            break;
                        }
                    }
                }

                // Stop filled - position closed
                // V8.2 FIX: Check both by object reference AND by order name prefix
                // This handles trailed stops that have DateTime.Ticks suffix in their name
                if (orderState == OrderState.Filled && orderName.StartsWith("Stop_"))
                {
                    // Try exact object match first
                    bool foundByReference = false;
                    if (stopOrders.Values.Contains(order))
                    {
                        // V8.30: Thread-safe snapshot iteration
                        foreach (var kvp in activePositions.ToArray())
                        {
                            if (!activePositions.ContainsKey(kvp.Key)) continue;
                            if (stopOrders.TryGetValue(kvp.Key, out var stopOrder) && stopOrder == order)
                            {
                                PositionInfo pos = kvp.Value;
                                Print(string.Format("STOP FILLED: {0} contracts @ {1:F2}", pos.RemainingContracts, averageFillPrice));
                                CleanupPosition(kvp.Key);
                                foundByReference = true;
                                break;
                            }
                        }
                    }

                    // V8.2 FIX: Fallback - match by order name prefix
                    // Order name format: "Stop_TREND_175232_E2_12345678" - extract "TREND_175232_E2"
                    if (!foundByReference)
                    {
                        // Extract entry name from stop order name (removes "Stop_" prefix and optional "_timestamp" suffix)
                        string stopPrefix = "Stop_";
                        string entryNameFromOrder = orderName.Substring(stopPrefix.Length);
                        // Remove timestamp suffix if present (format: _123456789012345)
                        int lastUnderscore = entryNameFromOrder.LastIndexOf('_');
                        if (lastUnderscore > 0 && entryNameFromOrder.Length - lastUnderscore > 10)
                        {
                            entryNameFromOrder = entryNameFromOrder.Substring(0, lastUnderscore);
                        }

                        // V8.30: Thread-safe access
                        if (activePositions.TryGetValue(entryNameFromOrder, out var pos))
                        {
                            Print(string.Format("STOP FILLED (by name): {0} contracts @ {1:F2}", pos.RemainingContracts, averageFillPrice));
                            CleanupPosition(entryNameFromOrder);
                        }
                    }
                }

                // Order rejected
                if (orderState == OrderState.Rejected)
                {
                    Print(string.Format("ORDER REJECTED: {0} | Error: {1}", orderName, nativeError));

                    // CRITICAL v5.8: Check if this was a stop order rejection
                    if (stopOrders.Values.Contains(order))
                    {
                        Print(string.Format("⚠️ CRITICAL: Stop order REJECTED: {0}", orderName));

                        // V8.30: Thread-safe snapshot iteration
                        foreach (var kvp in activePositions.ToArray())
                        {
                            if (!activePositions.ContainsKey(kvp.Key)) continue;
                            if (stopOrders.TryGetValue(kvp.Key, out var stopOrder) && stopOrder == order)
                            {
                                PositionInfo pos = kvp.Value;
                                Print(string.Format("⚠️ Position {0} is UNPROTECTED: {1} {2} contracts @ {3:F2}",
                                    kvp.Key,
                                    pos.Direction == MarketPosition.Long ? "LONG" : "SHORT",
                                    pos.RemainingContracts,
                                    pos.EntryPrice));

                                // V12.11: Remove stale rejected stop, then re-submit directly
                                // Cannot use UpdateStopQuantity — it early-exits if stopOrders is empty (line 3044)
                                // and the cancel-replace flow doesn't apply to a rejected (non-working) order.
                                Print(string.Format("Attempting to re-submit stop for {0}...", kvp.Key));
                                stopOrders.TryRemove(kvp.Key, out _);
                                CreateNewStopOrder(kvp.Key, pos.RemainingContracts, pos.CurrentStopPrice, pos.Direction);
                                break;
                            }
                        }
                    }

                    // V12.11: Target order rejected - remove stale reference from dictionary
                    RemoveGhostTargetRef(order, "REJECTED");
                }

                // V12: Entry order price changed
                // This detects when user drags the order line to a new price
                if (entryOrders.Values.Contains(order) && (orderState == OrderState.Accepted || orderState == OrderState.Working))
                {
                    // V8.30: Thread-safe snapshot iteration
                    foreach (var kvp in activePositions.ToArray())
                    {
                        if (!activePositions.ContainsKey(kvp.Key)) continue;
                        string entryName = kvp.Key;
                        PositionInfo pos = kvp.Value;

                        // V8.30: Thread-safe check
                        if (entryOrders.TryGetValue(entryName, out var entryOrd) && entryOrd == order && !pos.EntryFilled)
                        {
                            // Get the new price from the order (limit orders use limitPrice, stop orders use stopPrice)
                            double newPrice = limitPrice > 0 ? limitPrice : stopPrice;
                            
                            // Check if price changed (with tick tolerance)
                            if (Math.Abs(newPrice - pos.EntryPrice) > tickSize * 0.5)
                            {
                                double oldPrice = pos.EntryPrice;
                                pos.EntryPrice = newPrice;
                                
                                Print(string.Format("V12: Entry order MOVED: {0} | {1:F2} → {2:F2}", entryName, oldPrice, newPrice));
                                
                                // V12 SIMA: Legacy slave broadcast removed
                            }
                            break;
                        }
                    }
                }

                // V8.11: Stop order cancelled - check for pending replacement
                if (orderName.StartsWith("Stop_") && orderState == OrderState.Cancelled)
                {
                    // V8.30: Thread-safe snapshot iteration with TryRemove
                    foreach (var kvp in pendingStopReplacements.ToArray())
                    {
                        string entryName = kvp.Key;
                        PendingStopReplacement pending = kvp.Value;

                        // V8.24 FIX: REMOVED recursive 'Contains' check. STRICT object match only.
                        if (activePositions.ContainsKey(entryName) && pending.OldOrder == order)
                        {
                            Print(string.Format("STOP CANCELLED (confirmed): {0} | Creating replacement...", entryName));

                            // Create the replacement stop
                            CreateNewStopOrder(entryName, pending.Quantity, pending.StopPrice, pending.Direction);

                            // V8.30: Thread-safe removal with count decrement
                            if (pendingStopReplacements.TryRemove(entryName, out _))
                            {
                                Interlocked.Decrement(ref pendingReplacementCount);
                            }
                            break;
                        }
                        else if (!activePositions.ContainsKey(entryName))
                        {
                            Print(string.Format("STOP CANCELLED: {0} ignored (position already closed/cleaned)", entryName));
                            // V8.30: Thread-safe removal with count decrement
                            if (pendingStopReplacements.TryRemove(entryName, out _))
                            {
                                Interlocked.Decrement(ref pendingReplacementCount);
                            }
                            break;
                        }
                    }
                }

                // V12: Entry order cancelled
                if (entryOrders.Values.Contains(order) && orderState == OrderState.Cancelled)
                {
                    // V8.30: Thread-safe snapshot iteration
                    foreach (var kvp in activePositions.ToArray())
                    {
                        if (!activePositions.ContainsKey(kvp.Key)) continue;
                        string entryName = kvp.Key;
                        PositionInfo pos = kvp.Value;

                        if (entryOrders.TryGetValue(entryName, out var entryOrder) && entryOrder == order && !pos.EntryFilled)
                        {
                            Print(string.Format("V12: Entry order CANCELLED: {0}", entryName));

                            // Clean up local state
                            CleanupPosition(entryName);
                            break;
                        }
                    }
                }

                // V12.11: Target order cancelled - remove stale reference from dictionary
                // NOTE: If CleanupPosition already removed this order, Values.Contains returns false (safe/idempotent).
                if (orderState == OrderState.Cancelled)
                {
                    RemoveGhostTargetRef(order, "CANCELLED");
                }
            }
            catch (Exception ex)
            {
                Print("ERROR OnOrderUpdate: " + ex.Message);
            }
        }

        protected override void OnPositionUpdate(Position position, double averagePrice, int quantity, MarketPosition marketPosition)
        {
            try
            {
                // Check for EXTERNAL close (position went flat from outside strategy)
                if (marketPosition == MarketPosition.Flat)
                {
                    // V8.22: Even if activePositions is empty (strategy restart), we should scan for orphans
                    if (activePositions.Count == 0)
                    {
                        Print("EXTERNAL CLOSE/RESTART DETECTED - Scanning for orphaned bracket orders...");
                        ReconcileOrphanedOrders("Position went flat");
                        return;
                    }

                    // Check if we still have any positions that think they're filled
                    List<string> positionsToCleanup = new List<string>();

                    // V8.30: Thread-safe snapshot iteration
                    foreach (var kvp in activePositions.ToArray())
                    {
                        if (!activePositions.ContainsKey(kvp.Key)) continue;
                        PositionInfo pos = kvp.Value;
                        if (pos.EntryFilled && pos.RemainingContracts > 0)
                        {
                            Print("EXTERNAL CLOSE DETECTED - Position went flat. Cancelling orphaned orders...");

                            // V8.30: Thread-safe order access
                            if (stopOrders.TryGetValue(kvp.Key, out var stopOrder))
                            {
                                if (stopOrder != null && (stopOrder.OrderState == OrderState.Working || stopOrder.OrderState == OrderState.Accepted))
                                {
                                    CancelOrder(stopOrder);
                                }
                            }

                            // Cancel orphaned target orders
                            if (target1Orders.TryGetValue(kvp.Key, out var t1Order))
                            {
                                if (t1Order != null && (t1Order.OrderState == OrderState.Working || t1Order.OrderState == OrderState.Accepted))
                                {
                                    CancelOrder(t1Order);
                                }
                            }

                            if (target2Orders.TryGetValue(kvp.Key, out var t2Order))
                            {
                                if (t2Order != null && (t2Order.OrderState == OrderState.Working || t2Order.OrderState == OrderState.Accepted))
                                {
                                    CancelOrder(t2Order);
                                }
                            }

                            // v5.13: Cancel T3/T4 orphaned orders
                            if (target3Orders.TryGetValue(kvp.Key, out var t3Order))
                            {
                                if (t3Order != null && (t3Order.OrderState == OrderState.Working || t3Order.OrderState == OrderState.Accepted))
                                {
                                    CancelOrder(t3Order);
                                }
                            }

                            positionsToCleanup.Add(kvp.Key);
                        }
                    }

                    // REMOVED v5.7: DO NOT cancel unrelated pending entry orders!
                    // The old logic here cancelled ALL pending entries when position went flat,
                    // which incorrectly cancelled opposite-side OR entries (e.g., ORShort when ORLong closed)
                    // Pending entries should remain active - they are independent trades!

                    // Clean up positions
                    foreach (string key in positionsToCleanup)
                    {
                        CleanupPosition(key);
                    }

                    if (positionsToCleanup.Count > 0)
                    {
                        Print("Cleanup complete - Strategy still running, ready for new entries.");
                    }
                }
            }
            catch (Exception ex)
            {
                Print("ERROR OnPositionUpdate: " + ex.Message);
            }
        }

        private void SubmitBracketOrders(string entryName, PositionInfo pos)
        {
            if (pos.BracketSubmitted) return;

            try
            {
                // Validate stop price
                double validatedStopPrice = ValidateStopPrice(pos.Direction, pos.InitialStopPrice);

                // Submit initial stop for all contracts
                Order stopOrder = pos.Direction == MarketPosition.Long
                    ? SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.StopMarket, pos.TotalContracts, 0, validatedStopPrice, "", "Stop_" + entryName)
                    : SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.StopMarket, pos.TotalContracts, 0, validatedStopPrice, "", "Stop_" + entryName);

                stopOrders[entryName] = stopOrder;

                // Submit T1 limit order ONLY if T1 quantity > 0 AND TotalContracts > 1
                // V8.15: For 1-contract trades, we treat it as a runner (no initial target)
                if (pos.T1Contracts > 0 && pos.TotalContracts > 1)
                {
                    Order t1Order = pos.Direction == MarketPosition.Long
                        ? SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Limit, pos.T1Contracts, pos.Target1Price, 0, "", "T1_" + entryName)
                        : SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Limit, pos.T1Contracts, pos.Target1Price, 0, "", "T1_" + entryName);

                    target1Orders[entryName] = t1Order;
                }
                else if (pos.TotalContracts == 1)
                {
                    Print(string.Format("V8.15: 1-contract trade detected for {0}. Treating as RUNNER (no initial target).", entryName));
                }

                // Submit T2 limit order ONLY if T2 quantity > 0
                if (pos.T2Contracts > 0)
                {
                    Order t2Order = pos.Direction == MarketPosition.Long
                        ? SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Limit, pos.T2Contracts, pos.Target2Price, 0, "", "T2_" + entryName)
                        : SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Limit, pos.T2Contracts, pos.Target2Price, 0, "", "T2_" + entryName);

                    target2Orders[entryName] = t2Order;
                }

                // v5.13: Submit T3 limit order ONLY if T3 quantity > 0
                if (pos.T3Contracts > 0)
                {
                    Order t3Order = pos.Direction == MarketPosition.Long
                        ? SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Limit, pos.T3Contracts, pos.Target3Price, 0, "", "T3_" + entryName)
                        : SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Limit, pos.T3Contracts, pos.Target3Price, 0, "", "T3_" + entryName);

                    target3Orders[entryName] = t3Order;
                }

                // NOTE: T4 (runner) has no limit order - it trails with stop

                pos.BracketSubmitted = true;
                pos.CurrentStopPrice = validatedStopPrice;

                // Build bracket summary message with all 4 targets
                StringBuilder bracketMsg = new StringBuilder();
                string tradeType = pos.IsRMATrade ? "RMA" : "OR";
                bracketMsg.AppendFormat("{0} BRACKET V8.0: Stop@{1:F2}", tradeType, validatedStopPrice);
                if (pos.T1Contracts > 0)
                    bracketMsg.AppendFormat(" | T1:{0}@{1:F2}(+{2}pt)", pos.T1Contracts, pos.Target1Price, Target1FixedPoints);
                if (pos.T2Contracts > 0)
                    bracketMsg.AppendFormat(" | T2:{0}@{1:F2}", pos.T2Contracts, pos.Target2Price);
                if (pos.T3Contracts > 0)
                    bracketMsg.AppendFormat(" | T3:{0}@{1:F2}", pos.T3Contracts, pos.Target3Price);
                if (pos.T4Contracts > 0)
                    bracketMsg.AppendFormat(" | T4:{0}@trail", pos.T4Contracts);

                Print(bracketMsg.ToString());
            }
            catch (Exception ex)
            {
                Print("ERROR SubmitBracketOrders: " + ex.Message);
            }
        }

        private void UpdateStopQuantity(string entryName, PositionInfo pos)
        {
            if (!stopOrders.ContainsKey(entryName)) return;
            if (pos.RemainingContracts <= 0) return;

            try
            {
                Order currentStop = stopOrders[entryName];

                // V8.11 FIX: Store pending replacement BEFORE cancelling
                // This ensures we only create a new stop when the old one is confirmed cancelled
                if (currentStop != null && (currentStop.OrderState == OrderState.Working || currentStop.OrderState == OrderState.Accepted))
                {
                    // V8.31: Check if there's already a pending replacement to prevent duplicates
                    if (pendingStopReplacements.ContainsKey(entryName))
                    {
                        // Just update the quantity, don't create a new pending
                        if (pendingStopReplacements.TryGetValue(entryName, out var existingPending))
                        {
                            existingPending.Quantity = pos.RemainingContracts;
                            Print(string.Format("V8.31: Updated existing pending replacement for {0} to {1} contracts", entryName, pos.RemainingContracts));
                        }
                        return;
                    }

                    // Store the replacement info
                    var newPending = new PendingStopReplacement
                    {
                        EntryName = entryName,
                        Quantity = pos.RemainingContracts,
                        StopPrice = pos.CurrentStopPrice,
                        Direction = pos.Direction,
                        OldOrder = currentStop,
                        CreatedTime = DateTime.Now  // V8.31: Added for timeout support
                    };

                    // V8.31: Thread-safe add
                    if (pendingStopReplacements.TryAdd(entryName, newPending))
                    {
                        Interlocked.Increment(ref pendingReplacementCount);
                    }

                    // Cancel old stop - replacement will be created in OnOrderUpdate when confirmed
                    CancelOrder(currentStop);
                    Print(string.Format("STOP CANCEL PENDING: {0} | Will replace with {1} contracts @ {2:F2}",
                        entryName, pos.RemainingContracts, pos.CurrentStopPrice));
                }
                else
                {
                    // No existing stop to cancel, create new one directly
                    CreateNewStopOrder(entryName, pos.RemainingContracts, pos.CurrentStopPrice, pos.Direction);
                }
            }
            catch (Exception ex)
            {
                Print(string.Format("⚠️ ERROR UpdateStopQuantity for {0}: {1}", entryName, ex.Message));
                Print(string.Format("⚠️ POSITION MAY BE UNPROTECTED: {0} contracts", pos.RemainingContracts));
            }
        }

        // V8.11: Helper method to create a new stop order
        // V8.31: Added guard to prevent duplicate stop creation
        private void CreateNewStopOrder(string entryName, int quantity, double stopPrice, MarketPosition direction)
        {
            try
            {
                // V8.31: Check if a working stop already exists for this entry to prevent duplicates
                if (stopOrders.TryGetValue(entryName, out var existingStop))
                {
                    if (existingStop != null && (existingStop.OrderState == OrderState.Working || existingStop.OrderState == OrderState.Accepted))
                    {
                        Print(string.Format("V8.31: SKIPPING duplicate stop creation for {0} - stop already working", entryName));
                        return;
                    }
                }

                Order newStop = null;
                OrderAction exitAction = direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;

                // V12.3: Route to correct account (fleet follower vs local)
                if (activePositions.TryGetValue(entryName, out var pos) && pos.IsFollower && pos.ExecutingAccount != null)
                {
                    // Fleet follower: use Account API
                    string sigName = "S_" + entryName;
                    if (sigName.Length > 50) sigName = sigName.Substring(0, 50);
                    newStop = pos.ExecutingAccount.CreateOrder(Instrument, exitAction,
                        OrderType.StopMarket, TimeInForce.Gtc, quantity, 0, stopPrice, sigName, sigName, null);
                    pos.ExecutingAccount.Submit(new[] { newStop });
                }
                else
                {
                    // Local: use SubmitOrderUnmanaged with truncated signal name
                    string suffix = (DateTime.Now.Ticks % 100000000).ToString();
                    string sigName = "S_" + entryName + "_" + suffix;
                    if (sigName.Length > 50) sigName = sigName.Substring(0, 50);
                    newStop = SubmitOrderUnmanaged(0, exitAction, OrderType.StopMarket, quantity, 0, stopPrice, "", sigName);
                }

                if (newStop == null)
                {
                    Print(string.Format("⚠️ CRITICAL ERROR: Stop order submission returned NULL for {0}!", entryName));
                    Print(string.Format("⚠️ POSITION UNPROTECTED: {0} {1} contracts @ {2:F2}",
                        direction == MarketPosition.Long ? "LONG" : "SHORT", quantity, stopPrice));

                    // Attempt to flatten position immediately
                    Print(string.Format("⚠️ Attempting emergency flatten for {0}...", entryName));
                    FlattenPositionByName(entryName);
                    return;
                }

                stopOrders[entryName] = newStop;
                Print(string.Format("STOP QTY UPDATED: {0} contracts @ {1:F2} (Order: {2})",
                    quantity, stopPrice, newStop.Name));
            }
            catch (Exception ex)
            {
                Print(string.Format("⚠️ ERROR CreateNewStopOrder for {0}: {1}", entryName, ex.Message));
            }
        }

        private double ValidateStopPrice(MarketPosition direction, double desiredStopPrice)
        {
            double currentPrice = Close[0];
            double minDistance = 2 * tickSize;

            if (direction == MarketPosition.Long)
            {
                if (desiredStopPrice >= currentPrice)
                {
                    double validStop = currentPrice - minDistance;
                    Print(string.Format("STOP VALIDATION: Adjusted LONG stop from {0:F2} to {1:F2} (was at/above market)",
                        desiredStopPrice, validStop));
                    return validStop;
                }
            }
            else
            {
                if (desiredStopPrice <= currentPrice)
                {
                    double validStop = currentPrice + minDistance;
                    Print(string.Format("STOP VALIDATION: Adjusted SHORT stop from {0:F2} to {1:F2} (was at/below market)",
                        desiredStopPrice, validStop));
                    return validStop;
                }
            }

            return desiredStopPrice;
        }

        #endregion

        #region Trailing Stops

        private void ManageTrailingStops()
        {
            DateTime now = DateTime.Now;

            // V8.30: Adaptive throttle calculation - adjusts based on tick frequency
            tickCountInLastSecond++;
            if ((now - lastTickCountReset).TotalSeconds >= 1)
            {
                // Adjust throttle based on tick frequency
                if (tickCountInLastSecond > 50)
                    adaptiveThrottleMs = Math.Min(500, adaptiveThrottleMs + 50); // Increase throttle under load
                else if (tickCountInLastSecond < 20)
                    adaptiveThrottleMs = Math.Max(100, adaptiveThrottleMs - 25); // Decrease throttle when calm

                tickCountInLastSecond = 0;
                lastTickCountReset = now;
            }

            // V8.30: Use adaptive throttle instead of fixed 100ms
            if ((now - lastStopManagementTime).TotalMilliseconds < adaptiveThrottleMs)
                return;

            lastStopManagementTime = now;

            // V8.30: Clean up stale pending replacements (5-second timeout)
            CleanupStalePendingReplacements();

            // V8.30: Circuit breaker check - pause trailing when too many pending replacements
            if (circuitBreakerActive)
            {
                if ((now - circuitBreakerActivatedTime).TotalSeconds > 2)
                {
                    circuitBreakerActive = false;
                    Print("V8.30: Circuit breaker RESET - trailing stops resumed");
                }
                else
                {
                    return; // Skip trailing stop updates while circuit breaker is active
                }
            }

            // V8.30: Thread-safe snapshot iteration - prevents "Collection was modified" exception
            var positionSnapshot = activePositions.ToArray();
            foreach (var kvp in positionSnapshot)
            {
                string entryName = kvp.Key;
                PositionInfo pos = kvp.Value;

                // V8.30: Verify position still exists (may have been removed by callback thread)
                if (!activePositions.ContainsKey(entryName)) continue;

                if (!pos.EntryFilled || !pos.BracketSubmitted) continue;

                // Increment tick counter on every call
                pos.TicksSinceEntry++;

                // Update extreme price
                if (pos.Direction == MarketPosition.Long)
                    pos.ExtremePriceSinceEntry = Math.Max(pos.ExtremePriceSinceEntry, Close[0]);
                else
                    pos.ExtremePriceSinceEntry = Math.Min(pos.ExtremePriceSinceEntry, Close[0]);

                // V8.2: TREND Entry 1 - starts with fixed 2pt stop, switches to EMA9 trail when price crosses EMA
                if (pos.IsTRENDTrade && pos.IsTRENDEntry1)
                {
                    // V8.2: Use stored ema9 instance
                    double tickPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
                    double ema9Live = ema9 != null ? ema9[0] : Close[0];
                    double currentPrice = tickPrice;
                    
                    // Check if price has crossed EMA9 in our favor
                    bool priceInFavor = pos.Direction == MarketPosition.Long
                        ? currentPrice > ema9Live  // LONG: price above EMA9
                        : currentPrice < ema9Live; // SHORT: price below EMA9

                    // If not yet trailing and price crossed EMA in our favor, activate trailing
                    if (!pos.Entry1TrailActivated && priceInFavor)
                    {
                        pos.Entry1TrailActivated = true;
                        Print(string.Format("TREND E1: Switching to EMA9 trail (Price={0:F2} crossed EMA9={1:F2})",
                            currentPrice, ema9Live));
                    }

                    // If trailing is activated, manage the EMA9 trail
                    if (pos.Entry1TrailActivated)
                    {
                        double trendStop = pos.Direction == MarketPosition.Long
                            ? ema9Live - (currentATR * TRENDEntry1ATRMultiplier)  // V8.31: Uses E1 specific multiplier
                            : ema9Live + (currentATR * TRENDEntry1ATRMultiplier);

                        bool shouldUpdate = pos.Direction == MarketPosition.Long
                            ? trendStop > pos.CurrentStopPrice
                            : trendStop < pos.CurrentStopPrice;

                        if (shouldUpdate)
                        {
                            UpdateStopOrder(entryName, pos, trendStop, pos.CurrentTrailLevel);
                            // Print(string.Format("TREND E1 TRAIL: Stop moved to {0:F2} (EMA9={1:F2} - {2}xATR)",
                            //    trendStop, ema9Live, TRENDEntry2ATRMultiplier));
                        }
                    }
                    continue; // Skip normal trailing logic for TREND E1
                }

                // V8.2: TREND Entry 2 uses EMA15 trailing stop (1.1x ATR from live EMA15)
                if (pos.IsTRENDTrade && pos.IsTRENDEntry2 && !pos.IsRMATrade)
                {
                    // V8.2: Use stored ema15 instance
                    double ema15Live = ema15 != null ? ema15[0] : Close[0];
                    
                    double trendStop = pos.Direction == MarketPosition.Long
                        ? ema15Live - (currentATR * TRENDEntry2ATRMultiplier)
                        : ema15Live + (currentATR * TRENDEntry2ATRMultiplier);

                    bool shouldUpdate = pos.Direction == MarketPosition.Long
                        ? trendStop > pos.CurrentStopPrice
                        : trendStop < pos.CurrentStopPrice;

                    if (shouldUpdate)
                    {
                        UpdateStopOrder(entryName, pos, trendStop, pos.CurrentTrailLevel);
                        Print(string.Format("TREND E2 TRAIL: Stop moved to {0:F2} (EMA15={1:F2} - {2}xATR)", 
                            trendStop, ema15Live, TRENDEntry2ATRMultiplier));
                    }
                    continue; // Skip normal trailing logic for TREND E2
                }

                // V8.4: RETEST trade - Phase 1: Wait for price to cross 9 EMA, Phase 2: Trail at 9 EMA
                if (pos.IsRetestTrade && !pos.IsRMATrade)
                {
                    double tickPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
                    double ema9Live = ema9 != null ? ema9[0] : Close[0];
                    double currentPrice = tickPrice;

                    // Phase 1: Wait for price to cross EMA9 in our favor
                    if (!pos.RetestTrailActivated)
                    {
                        bool priceInFavor = pos.Direction == MarketPosition.Long
                            ? currentPrice > ema9Live  // LONG: price above EMA9
                            : currentPrice < ema9Live; // SHORT: price below EMA9

                        if (priceInFavor)
                        {
                            pos.RetestTrailActivated = true;
                            Print(string.Format("RETEST: Switching to EMA9 trail (Price={0:F2} crossed EMA9={1:F2})",
                                currentPrice, ema9Live));
                        }
                        // Stay at fixed stop until price crosses EMA
                        continue;
                    }

                    // Phase 2: Trail at 9 EMA - 1.1x ATR (locked in, only moves favorably)
                    double retestStop = pos.Direction == MarketPosition.Long
                        ? ema9Live - (currentATR * RetestATRMultiplier)
                        : ema9Live + (currentATR * RetestATRMultiplier);

                    // Only update if better than current stop
                    bool shouldUpdate = pos.Direction == MarketPosition.Long
                        ? retestStop > pos.CurrentStopPrice
                        : retestStop < pos.CurrentStopPrice;

                    if (shouldUpdate)
                    {
                        UpdateStopOrder(entryName, pos, retestStop, pos.CurrentTrailLevel);
                        Print(string.Format("RETEST TRAIL: Stop moved to {0:F2} (EMA9={1:F2} - {2}xATR)",
                            retestStop, ema9Live, RetestATRMultiplier));
                    }
                    continue; // Skip normal trailing logic for RETEST
                }

                double profitPoints = pos.Direction == MarketPosition.Long
                    ? pos.ExtremePriceSinceEntry - pos.EntryPrice
                    : pos.EntryPrice - pos.ExtremePriceSinceEntry;

                double newStopPrice = pos.CurrentStopPrice;
                int newTrailLevel = pos.CurrentTrailLevel;

                // MANUAL BREAKEVEN - Check FIRST before automatic trailing
                // This allows user to "arm" breakeven early and it auto-triggers when price reaches threshold
                if (pos.ManualBreakevenArmed && !pos.ManualBreakevenTriggered)
                {
                    double beThreshold = pos.EntryPrice + (ManualBreakevenBuffer * tickSize);
                    bool thresholdReached = false;

                    if (pos.Direction == MarketPosition.Long)
                    {
                        thresholdReached = Close[0] >= beThreshold;
                    }
                    else // Short
                    {
                        beThreshold = pos.EntryPrice - (ManualBreakevenBuffer * tickSize);
                        thresholdReached = Close[0] <= beThreshold;
                    }

                    if (thresholdReached)
                    {
                        // Move stop to breakeven + buffer
                        double manualBEStop = pos.Direction == MarketPosition.Long
                            ? pos.EntryPrice + (ManualBreakevenBuffer * tickSize)
                            : pos.EntryPrice - (ManualBreakevenBuffer * tickSize);

                        // Only move if it's better than current stop
                        bool shouldMove = pos.Direction == MarketPosition.Long
                            ? manualBEStop > pos.CurrentStopPrice
                            : manualBEStop < pos.CurrentStopPrice;

                        if (shouldMove)
                        {
                            newStopPrice = manualBEStop;
                            newTrailLevel = 1; // Same as automatic breakeven
                            pos.ManualBreakevenTriggered = true;
                            Print(string.Format("★ MANUAL BREAKEVEN TRIGGERED: {0} → Stop moved to {1:F2} (Entry + {2} tick)", 
                                entryName, manualBEStop, ManualBreakevenBuffer));
                        }
                    }
                }

                // v5.13 FREQUENCY CONTROL: Determine if we should check trailing based on current level
                // BE (level 0-1) and T3 (level 4) = every tick
                // T1 (level 2) and T2 (level 3) = every OTHER tick
                
                bool shouldCheckTrailing = true; // Default: check every tick
                
                // Determine current active level based on profit
                if (profitPoints >= Trail3TriggerPoints && pos.T1Filled && pos.T2Filled)
                {
                    // At T3 level (5+ points) - Check EVERY tick
                    shouldCheckTrailing = true;
                }
                else if (profitPoints >= Trail2TriggerPoints && pos.T1Filled)
                {
                    // At T2 level (4-4.99 points) - Check every OTHER tick
                    shouldCheckTrailing = (pos.TicksSinceEntry % 2 == 0);
                }
                else if (profitPoints >= Trail1TriggerPoints)
                {
                    // At T1 level (3-3.99 points) - Check every OTHER tick
                    shouldCheckTrailing = (pos.TicksSinceEntry % 2 == 0);
                }
                else
                {
                    // At BE level or below (0-2.99 points) - Check EVERY tick
                    shouldCheckTrailing = true;
                }

                // Only proceed with trailing logic if frequency check passes
                if (!shouldCheckTrailing)
                    continue;

                // Trail 3 (highest priority) - At 5 points, trail by 1 point
                // V8.22: Strictly profit based (no target dependencies)
                if (profitPoints >= Trail3TriggerPoints)
                {
                    double trail3Stop = pos.Direction == MarketPosition.Long
                        ? pos.ExtremePriceSinceEntry - Trail3DistancePoints
                        : pos.ExtremePriceSinceEntry + Trail3DistancePoints;

                    if (pos.Direction == MarketPosition.Long && trail3Stop > pos.CurrentStopPrice)
                    {
                        newStopPrice = trail3Stop;
                        newTrailLevel = 4; // Level 4 = Trail 3
                    }
                    else if (pos.Direction == MarketPosition.Short && trail3Stop < pos.CurrentStopPrice)
                    {
                        newStopPrice = trail3Stop;
                        newTrailLevel = 4;
                    }
                }
                // Trail 2 - At 4 points, trail by 1.5 points
                else if (profitPoints >= Trail2TriggerPoints && pos.CurrentTrailLevel < 3)
                {
                    double trail2Stop = pos.Direction == MarketPosition.Long
                        ? pos.ExtremePriceSinceEntry - Trail2DistancePoints
                        : pos.ExtremePriceSinceEntry + Trail2DistancePoints;

                    if (pos.Direction == MarketPosition.Long && trail2Stop > pos.CurrentStopPrice)
                    {
                        newStopPrice = trail2Stop;
                        newTrailLevel = 3; // Level 3 = Trail 2
                    }
                    else if (pos.Direction == MarketPosition.Short && trail2Stop < pos.CurrentStopPrice)
                    {
                        newStopPrice = trail2Stop;
                        newTrailLevel = 3;
                    }
                }
                // Trail 1 - At 3 points, trail by 2 points
                else if (profitPoints >= Trail1TriggerPoints && pos.CurrentTrailLevel < 2)
                {
                    double trail1Stop = pos.Direction == MarketPosition.Long
                        ? pos.ExtremePriceSinceEntry - Trail1DistancePoints
                        : pos.ExtremePriceSinceEntry + Trail1DistancePoints;

                    if (pos.Direction == MarketPosition.Long && trail1Stop > pos.CurrentStopPrice)
                    {
                        newStopPrice = trail1Stop;
                        newTrailLevel = 2; // Level 2 = Trail 1
                    }
                    else if (pos.Direction == MarketPosition.Short && trail1Stop < pos.CurrentStopPrice)
                    {
                        newStopPrice = trail1Stop;
                        newTrailLevel = 2;
                    }
                }
                // Break-even - At 2 points, move to BE +1 tick
                else if (profitPoints >= BreakEvenTriggerPoints && pos.CurrentTrailLevel < 1)
                {
                    double beStop = pos.Direction == MarketPosition.Long
                        ? pos.EntryPrice + (BreakEvenOffsetTicks * tickSize)
                        : pos.EntryPrice - (BreakEvenOffsetTicks * tickSize);

                    if (pos.Direction == MarketPosition.Long && beStop > pos.CurrentStopPrice)
                    {
                        newStopPrice = beStop;
                        newTrailLevel = 1;
                    }
                    else if (pos.Direction == MarketPosition.Short && beStop < pos.CurrentStopPrice)
                    {
                        newStopPrice = beStop;
                        newTrailLevel = 1;
                    }
                }

                // V8.21: Check if stop price actually changed by more than 1 tick before updating
                // This prevents redundant "micro-updates" that saturate the order system
                if (Math.Abs(newStopPrice - pos.CurrentStopPrice) < tickSize * 0.9)
                    continue;

                // Update stop if needed
                if (newStopPrice != pos.CurrentStopPrice)
                {
                    UpdateStopOrder(entryName, pos, newStopPrice, newTrailLevel);
                }
            }

            // V12.10: FLEET SYMMETRY SYNC PASS
            // When SIMA is enabled, force followers to match the Leader's trail level.
            // Followers calculate stops relative to their OWN entry prices but are triggered
            // by the Leader's profit progress. This prevents slippage-induced desync.
            if (EnableSIMA)
            {
                // Phase 1: Find the highest trail level among leader positions, by direction
                int leaderLongMaxLevel = 0;
                int leaderShortMaxLevel = 0;

                foreach (var kvp in positionSnapshot)
                {
                    PositionInfo ldr = kvp.Value;
                    if (ldr.IsFollower || !ldr.EntryFilled || !ldr.BracketSubmitted) continue;

                    if (ldr.Direction == MarketPosition.Long)
                        leaderLongMaxLevel = Math.Max(leaderLongMaxLevel, ldr.CurrentTrailLevel);
                    else if (ldr.Direction == MarketPosition.Short)
                        leaderShortMaxLevel = Math.Max(leaderShortMaxLevel, ldr.CurrentTrailLevel);
                }

                // V12.11: Diagnostic — log leader trail levels for fleet sync visibility
                if (leaderLongMaxLevel > 0 || leaderShortMaxLevel > 0)
                    Print($"[SIMA] Fleet Sync: Leader trail levels — Long={leaderLongMaxLevel}, Short={leaderShortMaxLevel}");

                // Phase 2: Sync lagging followers UP to the leader's level
                if (leaderLongMaxLevel > 0 || leaderShortMaxLevel > 0)
                {
                    foreach (var kvp in positionSnapshot)
                    {
                        string entryName2 = kvp.Key;
                        PositionInfo fol = kvp.Value;

                        if (!fol.IsFollower) continue;
                        if (!fol.EntryFilled || !fol.BracketSubmitted) continue;
                        if (!activePositions.ContainsKey(entryName2)) continue;

                        int targetLevel = (fol.Direction == MarketPosition.Long)
                            ? leaderLongMaxLevel
                            : leaderShortMaxLevel;

                        // V12.11: Guard — skip if no leader exists for this direction (targetLevel==0)
                        if (targetLevel == 0) continue;

                        // Only sync UP — never regress a follower already at a higher level
                        if (fol.CurrentTrailLevel >= targetLevel) continue;

                        double syncStopPrice = CalculateStopForLevel(fol, targetLevel);

                        // Only move if it's a more protective stop
                        bool isBetter = (fol.Direction == MarketPosition.Long)
                            ? syncStopPrice > fol.CurrentStopPrice
                            : syncStopPrice < fol.CurrentStopPrice;

                        if (isBetter)
                        {
                            UpdateStopOrder(entryName2, fol, syncStopPrice, targetLevel);
                            Print(string.Format("FLEET SYNC: {0} synced to Level {1} -> Stop {2:F2} (Leader advanced)",
                                entryName2, targetLevel, syncStopPrice));
                        }
                    }
                }
            }
        }

        // V8.30: Clean up stale pending replacements that are older than 5 seconds
        // Prevents memory leak and ensures positions remain protected
        private void CleanupStalePendingReplacements()
        {
            DateTime now = DateTime.Now;

            // V8.30: Safe iteration with snapshot
            foreach (var kvp in pendingStopReplacements.ToArray())
            {
                if ((now - kvp.Value.CreatedTime).TotalSeconds > 5)
                {
                    if (pendingStopReplacements.TryRemove(kvp.Key, out var pending))
                    {
                        Interlocked.Decrement(ref pendingReplacementCount);
                        Print(string.Format("V8.30: Stale pending replacement REMOVED for {0} (>5sec old)", kvp.Key));

                        // If position still exists and needs protection, create emergency stop
                        if (activePositions.TryGetValue(kvp.Key, out var pos) && pos.EntryFilled && pos.RemainingContracts > 0)
                        {
                            Print(string.Format("V8.30: Creating EMERGENCY replacement stop for {0}", kvp.Key));
                            CreateNewStopOrder(kvp.Key, pending.Quantity, pending.StopPrice, pending.Direction);
                        }
                    }
                }
            }
        }

        // V10 Bridge: Wrapper for IPC MoveStopsToBreakevenPlusOne
        private void ChangeStop(string entryName, double newStopPrice)
        {
            if (activePositions.TryGetValue(entryName, out PositionInfo pos))
            {
                UpdateStopOrder(entryName, pos, newStopPrice, 1); // 1 = BE level
            }
        }

        private void UpdateStopOrder(string entryName, PositionInfo pos, double newStopPrice, int newTrailLevel)
        {
            // V8.30: Thread-safe check using TryGetValue
            if (!stopOrders.TryGetValue(entryName, out var currentStop)) return;

            Order newStop = null;

            try
            {
                double validatedStopPrice = ValidateStopPrice(pos.Direction, newStopPrice);

                // V8.30: Thread-safe update using TryGetValue to avoid TOCTOU race
                if (pendingStopReplacements.TryGetValue(entryName, out var existingPending))
                {
                    // Update the pending replacement atomically (pending is a reference type)
                    existingPending.StopPrice = validatedStopPrice;
                    existingPending.Quantity = pos.RemainingContracts;
                    pos.CurrentStopPrice = validatedStopPrice;
                    pos.CurrentTrailLevel = newTrailLevel;
                    return;
                }

                // V8.11 FIX: Store pending replacement BEFORE cancelling
                // V8.12 FIX: Also handle CancelPending and PendingSubmit states to prevent race condition
                // V8.30: Added CreatedTime for timeout support and circuit breaker tracking
                if (currentStop != null && (currentStop.OrderState == OrderState.CancelPending || currentStop.OrderState == OrderState.Submitted))
                {
                    // Order is already being cancelled or submitted - queue the new stop price
                    var newPending = new PendingStopReplacement
                    {
                        EntryName = entryName,
                        Quantity = pos.RemainingContracts,
                        StopPrice = validatedStopPrice,
                        Direction = pos.Direction,
                        OldOrder = currentStop,
                        CreatedTime = DateTime.Now  // V8.30: Timeout support
                    };

                    // V8.30: Thread-safe add or update
                    if (pendingStopReplacements.TryAdd(entryName, newPending))
                    {
                        // V8.30: Track count for circuit breaker
                        int currentCount = Interlocked.Increment(ref pendingReplacementCount);
                        if (currentCount >= CIRCUIT_BREAKER_THRESHOLD && !circuitBreakerActive)
                        {
                            circuitBreakerActive = true;
                            circuitBreakerActivatedTime = DateTime.Now;
                            Print(string.Format("V8.30: CIRCUIT BREAKER ACTIVATED - {0} pending replacements (threshold: {1})",
                                currentCount, CIRCUIT_BREAKER_THRESHOLD));
                        }
                    }
                    else if (pendingStopReplacements.TryGetValue(entryName, out var pending))
                    {
                        // Just update the pending price
                        pending.StopPrice = validatedStopPrice;
                    }

                    pos.CurrentStopPrice = validatedStopPrice;
                    pos.CurrentTrailLevel = newTrailLevel;
                    Print(string.Format("V8.12: Stop update queued for {0} (current state: {1})", entryName, currentStop.OrderState));
                    return;
                }

                if (currentStop != null && (currentStop.OrderState == OrderState.Working || currentStop.OrderState == OrderState.Accepted))
                {
                    var newPending = new PendingStopReplacement
                    {
                        EntryName = entryName,
                        Quantity = pos.RemainingContracts,
                        StopPrice = validatedStopPrice,
                        Direction = pos.Direction,
                        OldOrder = currentStop,
                        CreatedTime = DateTime.Now  // V8.30: Timeout support
                    };

                    // V8.30: Thread-safe add
                    if (pendingStopReplacements.TryAdd(entryName, newPending))
                    {
                        int currentCount = Interlocked.Increment(ref pendingReplacementCount);
                        if (currentCount >= CIRCUIT_BREAKER_THRESHOLD && !circuitBreakerActive)
                        {
                            circuitBreakerActive = true;
                            circuitBreakerActivatedTime = DateTime.Now;
                            Print(string.Format("V8.30: CIRCUIT BREAKER ACTIVATED - {0} pending replacements", currentCount));
                        }
                    }

                    if (pos.ExecutingAccount != null)
                    {
                        pos.ExecutingAccount.Cancel(new[] { currentStop });
                    }
                    else
                    {
                        CancelOrder(currentStop);
                    }
                    pos.CurrentStopPrice = validatedStopPrice;
                    pos.CurrentTrailLevel = newTrailLevel;

                    string levelName = newTrailLevel <= 0 ? "Initial" : (newTrailLevel == 1 ? "BE" : "T" + (newTrailLevel - 1));
                    Print(string.Format("STOP UPDATED: {0} → {1:F2} (Level: {2})", entryName, validatedStopPrice, levelName));
                    return;
                }

                // No existing stop or not in a cancellable state - create directly
                if (pos.ExecutingAccount != null)
                {
                    newStop = pos.ExecutingAccount.CreateOrder(Instrument, pos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover, 
                        OrderType.StopMarket, TimeInForce.Gtc, pos.RemainingContracts, 0, validatedStopPrice, "Stop_" + entryName, "Stop_" + entryName, null);
                    pos.ExecutingAccount.Submit(new[] { newStop });
                    stopOrders[entryName] = newStop;
                }
                else
                {
                    // V12.3: Truncate signal name to stay under 50-char NinjaTrader limit
                    string suffix = (DateTime.Now.Ticks % 100000000).ToString();
                    string stopSigName = "S_" + entryName + "_" + suffix;
                    if (stopSigName.Length > 50) stopSigName = stopSigName.Substring(0, 50);
                    OrderAction stopExitAction = pos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;
                    newStop = SubmitOrderUnmanaged(0, stopExitAction, OrderType.StopMarket, pos.RemainingContracts, 0, validatedStopPrice, "", stopSigName);

                    if (newStop != null) stopOrders[entryName] = newStop;
                }

                if (newStop == null)
                {
                    Print(string.Format("⚠️ CRITICAL ERROR: Stop order submission returned NULL for {0}!", entryName));
                    Print(string.Format("⚠️ POSITION UNPROTECTED: {0} {1} contracts @ {2:F2}",
                        pos.Direction == MarketPosition.Long ? "LONG" : "SHORT",
                        pos.RemainingContracts,
                        pos.EntryPrice));
                    Print(string.Format("⚠️ Attempted stop price: {0:F2} | Current price: {1:F2}", validatedStopPrice, Close[0]));

                    Print(string.Format("⚠️ Attempting emergency flatten for {0}...", entryName));
                    FlattenPositionByName(entryName);
                    return;
                }

                stopOrders[entryName] = newStop;
                pos.CurrentStopPrice = validatedStopPrice;
                pos.CurrentTrailLevel = newTrailLevel;

                string levelName2 = newTrailLevel == 1 ? "BE" : "T" + (newTrailLevel - 1);
                Print(string.Format("STOP UPDATED: {0} → {1:F2} (Level: {2})", entryName, validatedStopPrice, levelName2));

            }
            catch (Exception ex)
            {
                Print(string.Format("⚠️ ERROR UpdateStopOrder for {0}: {1}", entryName, ex.Message));
                Print(string.Format("⚠️ POSITION MAY BE UNPROTECTED: {0} contracts", pos.RemainingContracts));
                
                // Attempt emergency flatten
                try
                {
                    Print(string.Format("⚠️ Attempting emergency flatten for {0}...", entryName));
                    FlattenPositionByName(entryName);
                }
                catch (Exception flattenEx)
                {
                    Print(string.Format("⚠️⚠️ EMERGENCY FLATTEN FAILED: {0}", flattenEx.Message));
                }
            }
        }

        // V12.10: Fleet Symmetry — calculates the correct stop price for a given trail level
        // using the position's own entry/extreme prices. Pure calculation, no side effects.
        private double CalculateStopForLevel(PositionInfo pos, int level)
        {
            bool isLong = (pos.Direction == MarketPosition.Long);
            switch (level)
            {
                case 1: // Breakeven
                    return isLong
                        ? pos.EntryPrice + (BreakEvenOffsetTicks * tickSize)
                        : pos.EntryPrice - (BreakEvenOffsetTicks * tickSize);
                case 2: // Trail 1
                    return isLong
                        ? pos.ExtremePriceSinceEntry - Trail1DistancePoints
                        : pos.ExtremePriceSinceEntry + Trail1DistancePoints;
                case 3: // Trail 2
                    return isLong
                        ? pos.ExtremePriceSinceEntry - Trail2DistancePoints
                        : pos.ExtremePriceSinceEntry + Trail2DistancePoints;
                case 4: // Trail 3
                    return isLong
                        ? pos.ExtremePriceSinceEntry - Trail3DistancePoints
                        : pos.ExtremePriceSinceEntry + Trail3DistancePoints;
                default:
                    return pos.CurrentStopPrice; // No change
            }
        }

        private void OnBreakevenButtonClick()
        {
            try
            {
                if (activePositions.Count == 0)
                {
                    Print("BREAKEVEN: No active positions");
                    return;
                }

                // V8.30: Thread-safe snapshot iteration for UI button handler
                var posSnapshot = activePositions.ToArray();

                // Check if any positions are already triggered (can't toggle after trigger)
                bool anyTriggered = false;
                foreach (var kvp in posSnapshot)
                {
                    if (kvp.Value.ManualBreakevenTriggered)
                    {
                        anyTriggered = true;
                        break;
                    }
                }

                if (anyTriggered)
                {
                    Print("BREAKEVEN: Already triggered - cannot toggle");
                    return;
                }

                // Check current state - if any armed, disarm all; if none armed, arm all
                bool anyArmed = false;
                foreach (var kvp in posSnapshot)
                {
                    if (kvp.Value.ManualBreakevenArmed)
                    {
                        anyArmed = true;
                        break;
                    }
                }

                // Toggle: if armed, disarm; if disarmed, arm
                foreach (var kvp in posSnapshot)
                {
                    if (!activePositions.ContainsKey(kvp.Key)) continue;
                    PositionInfo pos = kvp.Value;
                    if (pos.EntryFilled && !pos.ManualBreakevenTriggered)
                    {
                        if (anyArmed)
                        {
                            // Disarm
                            pos.ManualBreakevenArmed = false;
                            Print(string.Format("BREAKEVEN DISARMED: {0}", kvp.Key));
                        }
                        else
                        {
                            // Arm
                            pos.ManualBreakevenArmed = true;
                            Print(string.Format("BREAKEVEN ARMED: {0} - Will trigger at Entry + {1} tick(s)",
                                kvp.Key, ManualBreakevenBuffer));
                        }
                    }
                }

                UpdateDisplay();
            }
            catch (Exception ex)
            {
                Print("ERROR OnBreakevenButtonClick: " + ex.Message);
            }
        }

        #endregion

        #region Position Sync

        private void SyncPositionState()
        {
            List<string> toRemove = new List<string>();

            // V8.30: Thread-safe snapshot iteration
            foreach (var kvp in activePositions.ToArray())
            {
                PositionInfo pos = kvp.Value;
                if (pos.EntryFilled && pos.RemainingContracts <= 0)
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (string key in toRemove)
            {
                CleanupPosition(key);
            }
        }

        /// <summary>
        /// V12 SIMA: Chase If Touch - iterates the unified entryOrders dictionary which contains
        /// BOTH local and fleet follower limit orders. When price touches a working limit,
        /// the order is converted to market so it fills immediately.
        /// Local orders: ChangeOrder(). Follower orders: cancel + resubmit via ExecutingAccount.
        /// </summary>
        private void ManageCIT()
        {
            if (activePositions.Count == 0 && entryOrders.Count == 0) return;
            if (string.IsNullOrEmpty(ChaseIfTouchPoints) || ChaseIfTouchPoints == "0") return;

            double citOffset = 0;
            if (!double.TryParse(ChaseIfTouchPoints, out citOffset)) return;

            // Iterate ALL entry orders in the unified dictionary (local + every fleet account)
            foreach (var kvp in entryOrders.ToArray())
            {
                string key = kvp.Key;
                Order order = kvp.Value;
                if (order == null || order.OrderState != OrderState.Working) continue;
                if (order.OrderType != OrderType.Limit) continue; // only chase limit entries

                double currentPrice = (order.OrderAction == OrderAction.Buy) ? High[0] : Low[0];
                double limitPrice = order.LimitPrice;

                bool triggerChase = (order.OrderAction == OrderAction.Buy)
                    ? (currentPrice >= limitPrice)
                    : (currentPrice <= limitPrice);

                if (!triggerChase) continue;

                // Determine local vs follower
                PositionInfo pos = null;
                activePositions.TryGetValue(key, out pos);
                bool isFollower = pos != null && pos.IsFollower && pos.ExecutingAccount != null;

                try
                {
                    if (isFollower)
                    {
                        // Fleet follower: cancel limit, resubmit as market via account API
                        Account followerAcct = pos.ExecutingAccount;
                        Print($"[CIT] FLEET chase: {key} on {followerAcct.Name} | Limit {limitPrice:F2} -> MKT @ {currentPrice:F2}");

                        followerAcct.Cancel(new[] { order });

                        Order mktOrder = followerAcct.CreateOrder(Instrument, order.OrderAction, OrderType.Market,
                            TimeInForce.Gtc, order.Quantity, 0, 0, "", "CIT_" + key, null);
                        followerAcct.Submit(new[] { mktOrder });

                        entryOrders[key] = mktOrder; // update reference
                    }
                    else
                    {
                        // Local account: ChangeOrder converts limit to market
                        Print($"[CIT] LOCAL chase: {key} | Limit {limitPrice:F2} -> MKT @ {currentPrice:F2}");
                        ChangeOrder(order, order.Quantity, 0, 0);
                    }
                }
                catch (Exception ex)
                {
                    Print($"[CIT] ERROR chasing {key}: {ex.Message}");
                }
            }
        }

        private void FlattenAll()
        {
            try
            {
                // V10 GHOST FIX: Scan for actual live position even if activePositions is empty
                int liveQty = 0;
                MarketPosition liveDir = MarketPosition.Flat;
                if (Position != null)
                {
                    liveQty = Position.Quantity;
                    liveDir = Position.MarketPosition;
                }

                if (activePositions.Count == 0 && liveQty > 0)
                {
                     Print(string.Format("FLATTEN GHOST: Closing ORPHANED position of {0} contracts", liveQty));
                     if (liveDir == MarketPosition.Long)
                         SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, liveQty, 0, 0, "", "Flatten_Ghost");
                     else
                         SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, liveQty, 0, 0, "", "Flatten_Ghost");
                     
                     return; 
                }

                if (activePositions.Count == 0 && Position.MarketPosition == MarketPosition.Flat)
                {
                    Print("FLATTEN: No active positions to close");
                    // Still run SIMA flatten just in case of desync
                    if (EnableSIMA) FlattenAllApexAccounts();
                    return;
                }

                Print("FLATTEN: Closing all positions...");

                // 1. Local Flatten (Standard NT Exit)
                if (Position.MarketPosition != MarketPosition.Flat)
                {
                    foreach (var entryName in activePositions.Keys.ToList())
                    {
                        var pos = activePositions[entryName];
                        if (pos.RemainingContracts > 0)
                        {
                            if (pos.Direction == MarketPosition.Long)
                                ExitLong(pos.RemainingContracts, "Flatten", entryName);
                            else
                                ExitShort(pos.RemainingContracts, "Flatten", entryName);
                        }
                    }
                }

                // 2. Clear all pending entry orders on Master
                foreach (var entryOrder in entryOrders.Values)
                {
                    if (entryOrder != null && (entryOrder.OrderState == OrderState.Working || entryOrder.OrderState == OrderState.Accepted))
                        CancelOrder(entryOrder);
                }

                // 3. Flatten SIMA Fleet
                if (EnableSIMA)
                {
                    FlattenAllApexAccounts();
                }

                // V12.2: Reset Sync State
                isLongArmed = false;
                isShortArmed = false;

                List<string> positionsToCleanup = new List<string>();

                // V8.30: Thread-safe snapshot iteration
                foreach (var kvp in activePositions.ToArray())
                {
                    if (!activePositions.ContainsKey(kvp.Key)) continue;
                    PositionInfo pos = kvp.Value;
                    string entryName = kvp.Key;

                    if (pos.EntryFilled)
                    {
                        Print(string.Format("FLATTEN: Closing filled {0} position",
                            pos.Direction == MarketPosition.Long ? "LONG" : "SHORT"));

                        // V8.31: Cancel ALL bracket orders comprehensively
                        // Cancel stop order (may have multiple from rapid trailing)
                        if (stopOrders.TryGetValue(entryName, out var stopOrder))
                        {
                            if (stopOrder != null && (stopOrder.OrderState == OrderState.Working || stopOrder.OrderState == OrderState.Accepted || stopOrder.OrderState == OrderState.Submitted))
                            {
                                CancelOrder(stopOrder);
                                Print(string.Format("FLATTEN: Cancelling stop for {0}", entryName));
                            }
                        }

                        // V8.31: Also clear any pending stop replacements to prevent orphaned stops
                        if (pendingStopReplacements.TryRemove(entryName, out _))
                        {
                            Interlocked.Decrement(ref pendingReplacementCount);
                            Print(string.Format("V8.31: Cleared pending stop replacement for {0}", entryName));
                        }

                        // Cancel T1 order
                        if (target1Orders.TryGetValue(entryName, out var t1Order))
                        {
                            if (t1Order != null && (t1Order.OrderState == OrderState.Working || t1Order.OrderState == OrderState.Accepted || t1Order.OrderState == OrderState.Submitted))
                                CancelOrder(t1Order);
                        }

                        // Cancel T2 order
                        if (target2Orders.TryGetValue(entryName, out var t2Order))
                        {
                            if (t2Order != null && (t2Order.OrderState == OrderState.Working || t2Order.OrderState == OrderState.Accepted || t2Order.OrderState == OrderState.Submitted))
                                CancelOrder(t2Order);
                        }

                        // V8.31: Cancel T3 order
                        if (target3Orders.TryGetValue(entryName, out var t3Order))
                        {
                            if (t3Order != null && (t3Order.OrderState == OrderState.Working || t3Order.OrderState == OrderState.Accepted || t3Order.OrderState == OrderState.Submitted))
                                CancelOrder(t3Order);
                        }

                        // V8.31: Cancel T4 order
                        if (target4Orders.TryGetValue(entryName, out var t4Order))
                        {
                            if (t4Order != null && (t4Order.OrderState == OrderState.Working || t4Order.OrderState == OrderState.Accepted || t4Order.OrderState == OrderState.Submitted))
                                CancelOrder(t4Order);
                        }

                        // V8.28 FIX: Use LIVE position quantity instead of cached RemainingContracts
                        int livePositionQty = 0;
                        try
                        {
                            if (Position != null && Position.MarketPosition != MarketPosition.Flat)
                                livePositionQty = Position.Quantity;
                        }
                        catch (Exception pEx) { Print("Flatten Error reading Position: " + pEx.Message); }
                        
                        // Use the smaller of cached and live to avoid overselling
                        // V10 DIAGNOSTIC: Print values
                        Print(string.Format("FLATTEN DIAGNOSTIC: Entry={0} Cached={1} Live={2}", entryName, pos.RemainingContracts, livePositionQty));

                        // V10 FLATTEN FIX: Trust cached contracts if live is 0 (latency protection)
                        // If cached says we have contracts, we close them.
                        int flattenQty = pos.RemainingContracts;
                        
                        if (livePositionQty > 0)
                        {
                             // If NinjaTrader agrees we have a position, use the smaller to act safe? 
                             // No, if real position is smaller, we might be over-closing.
                             // But if real is larger, we under-close.
                             // Let's stick to closing what we know we opened.
                             flattenQty = pos.RemainingContracts;
                        }

                        // Submit market order to close position
                        if (flattenQty > 0)
                        {
                            Order flattenOrder = pos.Direction == MarketPosition.Long
                                ? SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, flattenQty, 0, 0, "", "Flatten_" + entryName)
                                : SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, flattenQty, 0, 0, "", "Flatten_" + entryName);

                            if (flattenOrder == null) Print("FLATTEN ERROR: SubmitOrderUnmanaged returned NULL");
                            else Print(string.Format("FLATTEN SENT: {0} {1} contracts", pos.Direction == MarketPosition.Long ? "SELL" : "BUY", flattenQty));
                        }
                        else
                        {
                             Print("FLATTEN SKIPPED: Qty is 0");
                        }

                        positionsToCleanup.Add(entryName);
                    }
                    else
                    {
                        // Cancel pending entry order
                        if (entryOrders.ContainsKey(entryName))
                        {
                            Order entryOrder = entryOrders[entryName];
                            if (entryOrder != null && (entryOrder.OrderState == OrderState.Working || entryOrder.OrderState == OrderState.Accepted))
                            {
                                CancelOrder(entryOrder);
                                Print(string.Format("FLATTEN: Cancelled pending {0} entry order @ {1:F2}",
                                    pos.Direction == MarketPosition.Long ? "LONG" : "SHORT", pos.EntryPrice));
                            }
                        }
                        positionsToCleanup.Add(entryName);
                    }
                }

                foreach (string key in positionsToCleanup)
                {
                    CleanupPosition(key);
                }

                UpdateDisplay();
            }
            catch (Exception ex)
            {
                Print("ERROR FlattenAll: " + ex.Message);
            }
        }

        private void FlattenPositionByName(string entryName)
        {
            if (!activePositions.TryGetValue(entryName, out var pos)) return;

            if (pos.EntryFilled && pos.RemainingContracts > 0)
            {
                Print(string.Format("⚠️ EMERGENCY FLATTEN: Closing {0} position due to stop order failure", entryName));

                // V12.3: Determine if this is a fleet follower or local position
                bool isFleetFollower = pos.IsFollower && pos.ExecutingAccount != null;

                // V8.31: Cancel ALL bracket orders first to prevent race conditions
                // V12.3: Use Account.Cancel for fleet followers, CancelOrder for local
                if (stopOrders.TryGetValue(entryName, out var stopOrder) && stopOrder != null)
                {
                    if (stopOrder.OrderState == OrderState.Working || stopOrder.OrderState == OrderState.Accepted)
                    {
                        if (isFleetFollower) pos.ExecutingAccount.Cancel(new[] { stopOrder });
                        else CancelOrder(stopOrder);
                    }
                }
                if (target1Orders.TryGetValue(entryName, out var t1Order) && t1Order != null)
                {
                    if (t1Order.OrderState == OrderState.Working || t1Order.OrderState == OrderState.Accepted)
                    {
                        if (isFleetFollower) pos.ExecutingAccount.Cancel(new[] { t1Order });
                        else CancelOrder(t1Order);
                    }
                }
                if (target2Orders.TryGetValue(entryName, out var t2Order) && t2Order != null)
                {
                    if (t2Order.OrderState == OrderState.Working || t2Order.OrderState == OrderState.Accepted)
                    {
                        if (isFleetFollower) pos.ExecutingAccount.Cancel(new[] { t2Order });
                        else CancelOrder(t2Order);
                    }
                }
                if (target3Orders.TryGetValue(entryName, out var t3Order) && t3Order != null)
                {
                    if (t3Order.OrderState == OrderState.Working || t3Order.OrderState == OrderState.Accepted)
                    {
                        if (isFleetFollower) pos.ExecutingAccount.Cancel(new[] { t3Order });
                        else CancelOrder(t3Order);
                    }
                }
                if (target4Orders.TryGetValue(entryName, out var t4Order) && t4Order != null)
                {
                    if (t4Order.OrderState == OrderState.Working || t4Order.OrderState == OrderState.Accepted)
                    {
                        if (isFleetFollower) pos.ExecutingAccount.Cancel(new[] { t4Order });
                        else CancelOrder(t4Order);
                    }
                }

                // V8.31: Clear pending replacements
                if (pendingStopReplacements.TryRemove(entryName, out _))
                {
                    Interlocked.Decrement(ref pendingReplacementCount);
                }

                int flattenQty = pos.RemainingContracts;
                OrderAction flattenAction = pos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;

                // V12.3: Route flatten order to correct account
                Order flattenOrder = null;
                if (isFleetFollower)
                {
                    // Fleet follower: flatten on the follower's own account
                    string sigName = "EF_" + entryName;
                    if (sigName.Length > 50) sigName = sigName.Substring(0, 50);
                    flattenOrder = pos.ExecutingAccount.CreateOrder(Instrument, flattenAction,
                        OrderType.Market, TimeInForce.Gtc, flattenQty, 0, 0, "", sigName, null);
                    pos.ExecutingAccount.Submit(new[] { flattenOrder });
                }
                else
                {
                    // Local: use SubmitOrderUnmanaged (use live position qty for accuracy)
                    try
                    {
                        if (Position != null && Position.MarketPosition != MarketPosition.Flat)
                            flattenQty = Math.Max(flattenQty, Position.Quantity);
                    }
                    catch { }

                    string sigName = "EF_" + entryName;
                    if (sigName.Length > 50) sigName = sigName.Substring(0, 50);
                    flattenOrder = SubmitOrderUnmanaged(0, flattenAction, OrderType.Market, flattenQty, 0, 0, "", sigName);
                }

                if (flattenOrder != null)
                {
                    Print(string.Format("Emergency flatten order submitted on {0}: {1} {2} contracts at MARKET",
                        isFleetFollower ? pos.ExecutingAccount.Name : "LOCAL",
                        pos.Direction == MarketPosition.Long ? "SELL" : "BUY",
                        flattenQty));
                }
                else
                {
                    Print(string.Format("⚠️⚠️⚠️ CRITICAL: Emergency flatten order FAILED for {0}!", entryName));
                    Print("⚠️⚠️⚠️ MANUAL INTERVENTION REQUIRED - Close position manually in NinjaTrader!");
                }
            }
        }


        private void CleanupPosition(string entryName)
        {
            // V8.17 EMERGENCY FIX: Move removal to TOP to prevent recursion
            // V8.30: Use atomic TryRemove for thread-safe removal
            if (!activePositions.TryRemove(entryName, out _)) return;

            int cancelledStops = 0;
            int cancelledTargets = 0;
            int cancelledEntries = 0;

            // V8.17 FIX: Use explicit dictionary-based cancellation instead of scanning ALL Account.Orders
            // V8.30: Use TryRemove for thread-safe atomic removal

            // Cancel stop order
            if (stopOrders.TryRemove(entryName, out var stopOrder))
            {
                if (stopOrder != null && (stopOrder.OrderState == OrderState.Working || stopOrder.OrderState.ToString().Contains("Pending")))
                {
                    CancelOrder(stopOrder);
                    cancelledStops++;
                }
            }

            // Cancel T1
            if (target1Orders.TryRemove(entryName, out var t1Order))
            {
                if (t1Order != null && (t1Order.OrderState == OrderState.Working || t1Order.OrderState.ToString().Contains("Pending")))
                {
                    CancelOrder(t1Order);
                    cancelledTargets++;
                }
            }

            // Cancel T2
            if (target2Orders.TryRemove(entryName, out var t2Order))
            {
                if (t2Order != null && (t2Order.OrderState == OrderState.Working || t2Order.OrderState.ToString().Contains("Pending")))
                {
                    CancelOrder(t2Order);
                    cancelledTargets++;
                }
            }

            // Cancel T3
            if (target3Orders.TryRemove(entryName, out var t3Order))
            {
                if (t3Order != null && (t3Order.OrderState == OrderState.Working || t3Order.OrderState.ToString().Contains("Pending")))
                {
                    CancelOrder(t3Order);
                    cancelledTargets++;
                }
            }

            // Cancel T4/Entry
            if (entryOrders.TryRemove(entryName, out var eOrder))
            {
                if (eOrder != null && (eOrder.OrderState == OrderState.Working || eOrder.OrderState.ToString().Contains("Pending")))
                {
                    CancelOrder(eOrder);
                    cancelledEntries++;
                }
            }

            // V8.30: Thread-safe removal with count decrement for pending replacements
            if (pendingStopReplacements.TryRemove(entryName, out _))
            {
                Interlocked.Decrement(ref pendingReplacementCount);
            }
            target4Orders.TryRemove(entryName, out _);

            // Log cleanup summary
            if (cancelledStops > 0 || cancelledTargets > 0 || cancelledEntries > 0)
            {
                Print(string.Format("CLEANUP SUMMARY for {0}: Stops={1} Targets={2} Entries={3}", 
                    entryName, cancelledStops, cancelledTargets, cancelledEntries));
            }

            UpdateDisplay();
        }

        /// <summary>
        /// V12.11: Remove a target order reference from its dictionary when it reaches a terminal state.
        /// Position remains live — stop continues protecting. No stop qty adjustment needed.
        /// </summary>
        private void RemoveGhostTargetRef(Order order, string reason)
        {
            var targetDicts = new (ConcurrentDictionary<string, Order> dict, string label)[]
            {
                (target1Orders, "T1"),
                (target2Orders, "T2"),
                (target3Orders, "T3"),
                (target4Orders, "T4"),
            };

            foreach (var (dict, label) in targetDicts)
            {
                if (!dict.Values.Contains(order)) continue;
                foreach (var kvp in dict.ToArray())
                {
                    if (dict.TryGetValue(kvp.Key, out var stored) && stored == order)
                    {
                        dict.TryRemove(kvp.Key, out _);
                        Print(string.Format("V12.11: {0} {1} - removed ghost ref for {2}", label, reason, kvp.Key));
                        break;
                    }
                }
                return; // Order can only be in one dictionary
            }
        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {
            try
            {
                if (execution == null || execution.Order == null) return;

                string orderName = execution.Order.Name;
                if (string.IsNullOrEmpty(orderName)) return;

                // V12.11: Compliance tracking for single-account mode
                if (EnableComplianceHub && !EnableSIMA)
                {
                    TrackTradeEntry(Account, execution);
                    UpdateAccountMetricsFromAccount(Account);
                    LogApexPerformance();
                }

                // Helper: Extract entry name from order name (removes prefix and optional timestamp suffix)
                Func<string, string, string> extractEntryName = (name, prefix) =>
                {
                    if (!name.StartsWith(prefix)) return "";
                    string entryPart = name.Substring(prefix.Length);
                    // Strip timestamp suffix if present (format: _123456789012345)
                    int lastUnderscore = entryPart.LastIndexOf('_');
                    if (lastUnderscore > 0 && entryPart.Length - lastUnderscore > 10)
                        entryPart = entryPart.Substring(0, lastUnderscore);
                    return entryPart;
                };

                // ============================================================
                // 1. STOP LOSS FILL - Manual OCO: Cancel all remaining targets
                // ============================================================
                if (orderName.StartsWith("Stop_"))
                {
                    string entryName = extractEntryName(orderName, "Stop_");
                    if (!string.IsNullOrEmpty(entryName) && activePositions.TryGetValue(entryName, out PositionInfo pos))
                    {
                        // Decrement RemainingContracts by the filled quantity
                        pos.RemainingContracts -= quantity;

                        Print(string.Format("STOP FILLED: {0} @ {1:F2}. Cancelling targets.", quantity, price));

                        // Manual OCO: Cancel all remaining profit targets immediately
                        int cancelledTargets = 0;

                        // Cancel T1
                        if (target1Orders.TryRemove(entryName, out var t1Order))
                        {
                            if (t1Order != null && (t1Order.OrderState == OrderState.Working || t1Order.OrderState == OrderState.Accepted))
                            {
                                CancelOrder(t1Order);
                                cancelledTargets++;
                            }
                        }

                        // Cancel T2
                        if (target2Orders.TryRemove(entryName, out var t2Order))
                        {
                            if (t2Order != null && (t2Order.OrderState == OrderState.Working || t2Order.OrderState == OrderState.Accepted))
                            {
                                CancelOrder(t2Order);
                                cancelledTargets++;
                            }
                        }

                        // Cancel T3
                        if (target3Orders.TryRemove(entryName, out var t3Order))
                        {
                            if (t3Order != null && (t3Order.OrderState == OrderState.Working || t3Order.OrderState == OrderState.Accepted))
                            {
                                CancelOrder(t3Order);
                                cancelledTargets++;
                            }
                        }

                        // Cancel T4 if present
                        if (target4Orders.TryRemove(entryName, out var t4Order))
                        {
                            if (t4Order != null && (t4Order.OrderState == OrderState.Working || t4Order.OrderState == OrderState.Accepted))
                            {
                                CancelOrder(t4Order);
                                cancelledTargets++;
                            }
                        }

                        if (cancelledTargets > 0)
                        {
                            Print(string.Format("OCO: Cancelled {0} target orders for {1}", cancelledTargets, entryName));
                        }

                        // Remove stop order reference
                        stopOrders.TryRemove(entryName, out _);

                        // Clean up pending replacements if any
                        if (pendingStopReplacements.TryRemove(entryName, out _))
                        {
                            Interlocked.Decrement(ref pendingReplacementCount);
                        }

                        // If position is fully closed, remove from activePositions
                        if (pos.RemainingContracts <= 0)
                        {
                            activePositions.TryRemove(entryName, out _);
                            entryOrders.TryRemove(entryName, out _);
                            Print(string.Format("Position {0} fully closed by stop.", entryName));
                        }

                        UpdateDisplay();
                    }
                }

                // ============================================================
                // 2. TARGET 1 FILL - Reduce stop quantity
                // ============================================================
                else if (orderName.StartsWith("T1_"))
                {
                    string entryName = extractEntryName(orderName, "T1_");
                    if (!string.IsNullOrEmpty(entryName) && activePositions.TryGetValue(entryName, out PositionInfo pos))
                    {
                        // Decrement RemainingContracts by the filled quantity
                        pos.RemainingContracts -= quantity;
                        pos.T1Filled = true;

                        Print(string.Format("TARGET FILLED: {0} @ {1:F2}. Reducing stop. Remaining: {2}",
                            quantity, price, pos.RemainingContracts));

                        // Update stop quantity to match new position size
                        if (pos.RemainingContracts > 0)
                        {
                            UpdateStopQuantity(entryName, pos);
                        }
                        else
                        {
                            // Position fully closed, cancel stop
                            if (stopOrders.TryRemove(entryName, out var stopOrder))
                            {
                                if (stopOrder != null && (stopOrder.OrderState == OrderState.Working || stopOrder.OrderState == OrderState.Accepted))
                                {
                                    CancelOrder(stopOrder);
                                    Print(string.Format("OCO: Cancelled stop for fully closed position {0}", entryName));
                                }
                            }
                            activePositions.TryRemove(entryName, out _);
                        }

                        // Remove T1 order reference
                        target1Orders.TryRemove(entryName, out _);
                        UpdateDisplay();
                    }
                }

                // ============================================================
                // 3. TARGET 2 FILL - Reduce stop quantity
                // ============================================================
                else if (orderName.StartsWith("T2_"))
                {
                    string entryName = extractEntryName(orderName, "T2_");
                    if (!string.IsNullOrEmpty(entryName) && activePositions.TryGetValue(entryName, out PositionInfo pos))
                    {
                        // Decrement RemainingContracts by the filled quantity
                        pos.RemainingContracts -= quantity;
                        pos.T2Filled = true;

                        Print(string.Format("TARGET FILLED: {0} @ {1:F2}. Reducing stop. Remaining: {2}",
                            quantity, price, pos.RemainingContracts));

                        // Update stop quantity to match new position size
                        if (pos.RemainingContracts > 0)
                        {
                            UpdateStopQuantity(entryName, pos);
                        }
                        else
                        {
                            // Position fully closed, cancel stop
                            if (stopOrders.TryRemove(entryName, out var stopOrder))
                            {
                                if (stopOrder != null && (stopOrder.OrderState == OrderState.Working || stopOrder.OrderState == OrderState.Accepted))
                                {
                                    CancelOrder(stopOrder);
                                    Print(string.Format("OCO: Cancelled stop for fully closed position {0}", entryName));
                                }
                            }
                            activePositions.TryRemove(entryName, out _);
                        }

                        // Remove T2 order reference
                        target2Orders.TryRemove(entryName, out _);
                        UpdateDisplay();
                    }
                }

                // ============================================================
                // 4. TARGET 3 FILL - Reduce stop quantity
                // ============================================================
                else if (orderName.StartsWith("T3_"))
                {
                    string entryName = extractEntryName(orderName, "T3_");
                    if (!string.IsNullOrEmpty(entryName) && activePositions.TryGetValue(entryName, out PositionInfo pos))
                    {
                        // Decrement RemainingContracts by the filled quantity
                        pos.RemainingContracts -= quantity;
                        pos.T3Filled = true;

                        Print(string.Format("TARGET FILLED: {0} @ {1:F2}. Reducing stop. Remaining: {2}",
                            quantity, price, pos.RemainingContracts));

                        // Update stop quantity to match new position size
                        if (pos.RemainingContracts > 0)
                        {
                            UpdateStopQuantity(entryName, pos);
                        }
                        else
                        {
                            // Position fully closed, cancel stop
                            if (stopOrders.TryRemove(entryName, out var stopOrder))
                            {
                                if (stopOrder != null && (stopOrder.OrderState == OrderState.Working || stopOrder.OrderState == OrderState.Accepted))
                                {
                                    CancelOrder(stopOrder);
                                    Print(string.Format("OCO: Cancelled stop for fully closed position {0}", entryName));
                                }
                            }
                            activePositions.TryRemove(entryName, out _);
                        }

                        // Remove T3 order reference
                        target3Orders.TryRemove(entryName, out _);
                        UpdateDisplay();
                    }
                }

                // ============================================================
                // 5. TRIM EXECUTION - V10.3.1: Enhanced Stop Integrity
                // ============================================================
                // 🔥 CRITICAL: When a TRIM executes, we MUST reduce the stop order quantity
                // to match the new position size. If we don't, hitting the stop after a trim
                // would close more contracts than we hold, creating an unintended REVERSE position.
                // Example: Long 4 contracts, stop at 4. Trim 2 (now Long 2). If stop stays at 4,
                // getting stopped out would SELL 4 (close 2 + go SHORT 2) = DISASTER.
                else if (orderName.StartsWith("Trim_"))
                {
                    string entryName = extractEntryName(orderName, "Trim_");
                    if (!string.IsNullOrEmpty(entryName) && activePositions.TryGetValue(entryName, out PositionInfo pos))
                    {
                        // Track previous quantity for logging
                        int previousQty = pos.RemainingContracts;

                        // Deduct ONLY the execution quantity (handle partial fills correctly)
                        pos.RemainingContracts -= quantity;

                        Print(string.Format("TRIM EXECUTION: {0} contracts closed for {1}. Position: {2} → {3}",
                            quantity, entryName, previousQty, pos.RemainingContracts));

                        // V10.3.1 FIX: MANDATORY stop quantity reduction to prevent reverse position
                        if (pos.RemainingContracts > 0)
                        {
                            Print(string.Format("STOP INTEGRITY: Reducing stop quantity from {0} to {1} for {2}",
                                previousQty, pos.RemainingContracts, entryName));
                            UpdateStopQuantity(entryName, pos);
                        }
                        else
                        {
                            // Position fully closed by trim, cancel stop
                            Print(string.Format("TRIM FLATTEN: Position {0} fully closed. Cancelling stop.", entryName));
                            if (stopOrders.TryRemove(entryName, out var stopOrder))
                            {
                                if (stopOrder != null && (stopOrder.OrderState == OrderState.Working || stopOrder.OrderState == OrderState.Accepted))
                                {
                                    CancelOrder(stopOrder);
                                }
                            }

                            // Also clean up any pending replacements
                            if (pendingStopReplacements.TryRemove(entryName, out _))
                            {
                                Interlocked.Decrement(ref pendingReplacementCount);
                            }

                            activePositions.TryRemove(entryName, out _);
                        }

                        UpdateDisplay();
                    }
                }
            }
            catch (Exception ex)
            {
                Print("Error OnExecutionUpdate: " + ex.Message);
            }
        }

        private void ReconcileOrphanedOrders(string reason)
        {
            try
            {
                if (Account == null) return;

                bool foundOrphans = false;
                foreach (Order order in Account.Orders)
                {
                    if (order == null) continue;
                    
                    // Only look at working orders
                    if (order.OrderState != OrderState.Working && order.OrderState != OrderState.Accepted)
                        continue;

                    // V8.27 CRITICAL FIX: Only process orders for THIS instrument
                    // This prevents cross-instrument cancellation when running multiple strategy instances
                    if (order.Instrument.FullName != Instrument.FullName)
                        continue;

                    // Check if this order has one of our prefix signatures
                    string name = order.Name;
                    if (name.StartsWith("Stop_") || name.StartsWith("T1_") || name.StartsWith("T2_") || 
                        name.StartsWith("T3_") || name.StartsWith("T4_") || name.StartsWith("Flatten_") || name.StartsWith("Trim_"))
                    {
                        // Check if we actually have an active position for this
                        string entryName = "";
                        if (name.Contains("_"))
                        {
                            int firstUnderscore = name.IndexOf('_');
                            entryName = name.Substring(firstUnderscore + 1);
                            // Strip timestamp if present
                            int lastUnderscore = entryName.LastIndexOf('_');
                            if (lastUnderscore > 0 && entryName.Length - lastUnderscore > 10)
                                entryName = entryName.Substring(0, lastUnderscore);
                        }

                        // V10 FIX: Handle TRIM execution state update - MOVED TO OnExecutionUpdate

                        if (string.IsNullOrEmpty(entryName) || !activePositions.ContainsKey(entryName))
                        {
                            Print(string.Format("ORPHANED ORDER DETECTED ({0}): {1} | Cancelling...", reason, name));
                            CancelOrder(order);
                            foundOrphans = true;
                        }
                    }
                }

                if (foundOrphans)
                    Print("Orphaned order reconciliation complete.");
            }
            catch (Exception ex)
            {
                Print("ERROR ReconcileOrphanedOrders: " + ex.Message);
            }
        }

        #endregion

        #region UI

        private void UpdateDisplay()
        {
            // Legacy chart UI removed; IPC/indicator drives manual controls now.
        }

        private void UpdateRMAModeDisplay()
        {
            // Legacy chart UI removed; no visual updates.
        }

        private void AttachHotkeys()
        {
            if (ChartControl?.OwnerChart != null)
            {
                ChartControl.OwnerChart.PreviewKeyDown += OnKeyDown;
                ChartControl.OwnerChart.PreviewKeyUp += OnKeyUp;
            }
        }

        private void DetachHotkeys()
        {
            if (ChartControl?.OwnerChart != null)
            {
                ChartControl.OwnerChart.PreviewKeyDown -= OnKeyDown;
                ChartControl.OwnerChart.PreviewKeyUp -= OnKeyUp;
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
                        UpdateRMAModeDisplay();

                        // V12.14: Broadcast RMA deactivation to panel
                        string deactivateConfig = string.Format(
                            "CONFIG|OR|COUNT:{0};T1:{1};T2:{2};T3:{3};STR:{4};MAX:{5};",
                            minContracts, Target1FixedPoints, Target2Multiplier, Target3Multiplier,
                            StopMultiplier, MaxRiskAmount);
                        SendResponseToRemote(deactivateConfig);
                        Print("V12.14: RMA auto-deactivated after entry - CONFIG broadcast sent");
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

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            // No longer using R key for RMA
        }

        #endregion

        #region Stop Management Helpers (V11)

        /// <summary>
        /// Moves all active position stops to Breakeven + Offset Points.
        /// If offset is 0, it is pure breakeven.
        /// </summary>
        private void MoveStopsToBreakevenWithOffset(double offsetPoints)
        {
            try
            {
                foreach (var kvp in activePositions.ToArray())
                {
                    PositionInfo pos = kvp.Value;
                    string entryName = kvp.Key;

                    if (!pos.EntryFilled || pos.RemainingContracts <= 0) continue;

                    double newStopPrice;
                    if (pos.Direction == MarketPosition.Long)
                        newStopPrice = pos.EntryPrice + offsetPoints;
                    else
                        newStopPrice = pos.EntryPrice - offsetPoints;

                    // Round to tick size
                    newStopPrice = Instrument.MasterInstrument.RoundToTickSize(newStopPrice);

                    // Only move stop if it's a better price (profit-protecting direction)
                    bool isBetter = (pos.Direction == MarketPosition.Long && newStopPrice > pos.CurrentStopPrice)
                                 || (pos.Direction == MarketPosition.Short && newStopPrice < pos.CurrentStopPrice);

                    if (!isBetter)
                    {
                        Print(string.Format("BE+{0}: Stop already better for {1}. Current={2:F2}, Request={3:F2}",
                            offsetPoints, entryName, pos.CurrentStopPrice, newStopPrice));
                        continue;
                    }

                    // V12.10: Use UpdateStopOrder for proper Master/Follower routing
                    // (ChangeOrder only works for Master — followers were silently skipped)
                    UpdateStopOrder(entryName, pos, newStopPrice, 1);
                    pos.ManualBreakevenTriggered = true;
                    Print(string.Format("BE+{0} MOVED: {1} Stop -> {2:F2}", offsetPoints, entryName, newStopPrice));
                }
            }
            catch (Exception ex)
            {
                Print("ERROR MoveStopsToBreakevenWithOffset: " + ex.Message);
            }
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
                                string mode = isRMAModeActive ? "RMA" : "OR";
                                string configResponse = string.Format(
                                    "CONFIG|{0}|COUNT:{1};T1:{2};T1TYPE:Points;T2:{3};T2TYPE:ATR;T3:{4};T3TYPE:ATR;T4:0;T4TYPE:ATR;T5:0;T5TYPE:ATR;STR:0;STRTYPE:Tick;MAX:0;CIT:{5};OT:Limit;\n",
                                    mode, minContracts, Target1FixedPoints, Target2Multiplier, Target3Multiplier, ChaseIfTouchPoints ?? "0");
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
                foreach (var pos in activePositions.Values)
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
                    else if (action == "BE_PLUS_2" || action == "BE_PLUS_1") // V11: Handle both (B+2 is default now, Legacy B+1 supported)
                    {
                        MoveStopsToBreakevenWithOffset(2.0); // 2 Points Fixed Offset
                    }
                    else if (action == "BE")
                    {
                        MoveStopsToBreakevenWithOffset(0); // Classic BE
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
                        // Cancel all working/pending orders without touching live positions
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
                                            acct.Cancel(new[] { order });
                                            cancelled++;
                                        }
                                    }
                                }
                            }
                            Print($"[SIMA] CANCEL_ALL → Cancelled {cancelled} working orders (local + fleet)");
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
                                    CancelOrder(order);
                                    cancelled++;
                                }
                            }
                            Print($"[V12] CANCEL_ALL → Cancelled {cancelled} working orders");
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
                            activeFleetAccounts[acctName] = active;
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
                        Print("[DIAG] ═══════════════════════════════════════════════════");
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
                        Print("[DIAG] ═══════════════════════════════════════════════════");
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
                            UpdateRMAModeDisplay();
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
                    // V12.5: SET_TARGETS|count - Bidirectional sync for target count buttons
                    else if (action == "SET_TARGETS")
                    {
                        if (parts.Length > 1 && int.TryParse(parts[1], out int targetCount))
                        {
                            minContracts = targetCount;
                            Print(string.Format("V12.5: SET_TARGETS = {0} contracts", targetCount));

                            // Build CONFIG message to broadcast
                            string mode = isRMAModeActive ? "RMA" : "ORB";
                            string configMsg = string.Format(
                                "CONFIG|{0}|COUNT:{1};T1:{2};T2:{3};T3:{4};STR:{5};MAX:{6};",
                                mode, targetCount, Target1FixedPoints, Target2Multiplier, Target3Multiplier,
                                StopMultiplier, MaxRiskAmount);

                            // V12.13-D: Broadcast CONFIG to all connected panel clients
                            SendResponseToRemote(configMsg);

                            Print(string.Format("V12.5: CONFIG Broadcast -> {0}", configMsg));
                        }
                    }
                    // V12.5: SET_MODE|mode - Bidirectional sync for config mode buttons
                    else if (action == "SET_MODE")
                    {
                        if (parts.Length > 1)
                        {
                            string newMode = parts[1].Trim().ToUpper();

                            // Update internal mode state
                            if (newMode == "RMA")
                                isRMAModeActive = true;
                            else if (newMode == "ORB" || newMode == "OR")
                                isRMAModeActive = false;

                            Print(string.Format("V12.5: SET_MODE = {0}", newMode));

                            // Build CONFIG message to broadcast
                            string configMsg = string.Format(
                                "CONFIG|{0}|COUNT:{1};T1:{2};T2:{3};T3:{4};STR:{5};MAX:{6};MODE:{0};",
                                newMode, minContracts, Target1FixedPoints, Target2Multiplier, Target3Multiplier,
                                StopMultiplier, MaxRiskAmount);

                            // V12.13-D: Broadcast CONFIG to all connected panel clients
                            SendResponseToRemote(configMsg);

                            Print(string.Format("V12.5: CONFIG Broadcast -> {0}", configMsg));
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

                            // Update UI to reflect the new manual price
                            UpdateDisplay();
                        }
                        else
                        {
                            Print(string.Format("IPC SET_MANUAL_PRICE: Invalid price format in command: {0}", command));
                        }
                    }
                    else if (action.StartsWith("MODE_") || action.StartsWith("EXEC_"))
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
        }
        catch
        {
            // Client likely disconnected, will be handled by reading loop or next cleanup
        }
    }
}

        // V12.13-D: SendToExternalApp REMOVED — it connected to port 5001 (the strategy's own listener),
        // causing infinite flood loops. All callers now use SendResponseToRemote() or direct client stream writes.


        private void MoveStopsToBreakevenPlusOne()
        {
            foreach (var pos in activePositions.Values)
            {
                if (pos.RemainingContracts > 0)
                {
                    double newStop = pos.Direction == MarketPosition.Long
                        ? pos.EntryPrice + (1 * tickSize)
                        : pos.EntryPrice - (1 * tickSize);

                    ChangeStop(pos.SignalName, newStop);
                    Print(string.Format("IPC BE+1: Moved stop for {0} to {1:F2}", pos.SignalName, newStop));
                }
            }
        }

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
             if (action == "MODE_RMA") isRMAModeActive = !isRMAModeActive;
             else if (action == "MODE_MOMO") isMOMOModeActive = !isMOMOModeActive;
             else if (action == "MODE_FFMA") Print("FFMA Mode Toggle received (Logic Pending)"); // V11 Support
             
             // V11 Trend & Retest RMA Toggles
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

             else if (action == "EXEC_TREND" || action == "EXEC_TREND_RMA") ExecuteTRENDEntry();
             else if (action == "EXEC_RETEST" || action == "EXEC_RETEST_PLUS" || action == "EXEC_RETEST_MINUS") ExecuteRetestEntry();
             
             // V11: Add FFMA/MOMO execution via IPC if added to remote
             else if (action == "EXEC_MOMO") ExecuteMOMOEntry(lastKnownPrice);
             
             Print(string.Format("IPC Mode Toggle: {0} | RMA={1} MOMO={2} TrendRMA={3} RetestRMA={4}", 
                action, isRMAModeActive, isMOMOModeActive, isTrendRmaMode, isRetestRmaMode));
             UpdateDisplay();
        }

        /// <summary>
        /// V12 SIMA: Helper struct to rank accounts by Daily P/L
        /// </summary>
        private struct AccountRankInfo
        {
            public Account Account;
            public double DailyPL;
            public string Name;
        }

        /// <summary>
        /// V12 SIMA: Returns the list of Apex accounts sorted by Daily P/L (Lowest to Highest)
        /// </summary>
        private List<AccountRankInfo> GetSortedAccountFleet()
        {
            List<AccountRankInfo> fleet = new List<AccountRankInfo>();

            foreach (Account acct in Account.All)
            {
                if (acct.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    double dailyPL = acct.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
                    fleet.Add(new AccountRankInfo { Account = acct, DailyPL = dailyPL, Name = acct.Name });
                }
            }

            // Sort by P/L ascending (Lowest P/L first)
            return fleet.OrderBy(a => a.DailyPL).ToList();
        }

        private void SetRmaAnchorFromIpc(string anchorStr)
        {
            try
            {
                if (anchorStr == "EMA30") currentRmaAnchor = RmaAnchorType.Ema30;
                else if (anchorStr == "EMA65") currentRmaAnchor = RmaAnchorType.Ema65;
                else if (anchorStr == "EMA200") currentRmaAnchor = RmaAnchorType.Ema200;
                else if (anchorStr == "OR_HIGH") currentRmaAnchor = RmaAnchorType.OrHigh;
                else if (anchorStr == "OR_LOW") currentRmaAnchor = RmaAnchorType.OrLow;
                else if (anchorStr == "MANUAL") currentRmaAnchor = RmaAnchorType.Manual;
                
                Print("IPC SET ANCHOR: " + anchorStr);
                
                // Refresh UI to show selected anchor (Green Highlight)
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                Print("Error SetRmaAnchorFromIpc: " + ex.Message);
            }
        }

        #endregion

        #region V12 SIMA Multi-Account Execution Engine

        /// <summary>
        /// V12 SIMA: Execute a Smart Dispatched trade across the fleet.
        /// Logic:
        ///   - Signal = TREND: Lowest P/L account gets TREND targets, others get RMA targets.
        ///   - Signal = RMA/OR/MOMO: All accounts get RMA targets.
        /// Accounts use FIXED brackets (Path B) for zero trail lag.
        /// </summary>
        private void ExecuteSmartDispatchEntry(string tradeType, OrderAction action, int quantity, double entryPrice, OrderType entryOrderType = OrderType.Market)
        {
            // V12.2: Diagnostic logging for copy trading troubleshooting
            Print($"[DISPATCH] ExecuteSmartDispatchEntry called: {tradeType} | EnableSIMA={EnableSIMA} | OrderType={entryOrderType}");

            if (!EnableSIMA)
            {
                Print("[DISPATCH] ⚠️ SIMA DISABLED - Enable in strategy parameters to copy trade");
                return;
            }

            List<AccountRankInfo> fleet = GetSortedAccountFleet();

            // V12.2: Log fleet state for diagnostics
            int activeCount = 0;
            foreach (var acct in fleet)
            {
                if (activeFleetAccounts.TryGetValue(acct.Name, out bool isActive) && isActive)
                    activeCount++;
            }
            Print($"[DISPATCH] Fleet: {fleet.Count} total accounts | {activeCount} ACTIVE in Fleet Manager");

            if (fleet.Count == 0)
            {
                Print("[DISPATCH] ⚠️ NO APEX ACCOUNTS DETECTED - Check AccountPrefix setting");
                return;
            }

            if (activeCount == 0)
            {
                Print("[DISPATCH] ⚠️ NO ACCOUNTS ENABLED - Toggle accounts ON in Fleet Manager panel");
            }

            int trendCount = 0;
            int rmaCount = 0;

            for (int i = 0; i < fleet.Count; i++)
            {
                Account acct = fleet[i].Account;
                
                // V12.1: Skip Master account if its order was already placed by the caller
                if (acct == this.Account) continue;

                // V12.8: Skip accounts NOT registered or disabled in Fleet Manager UI
                if (!activeFleetAccounts.TryGetValue(acct.Name, out bool isActive) || !isActive)
                {
                    Print($"[SIMA] Fleet Dispatch: {acct.Name} SKIPPED (Inactive in Fleet Manager)");
                    continue;
                }

                // Consistency Lock Check (Shared logic)
                if (EnableConsistencyLock)
                {
                    if (fleet[i].DailyPL >= MaxDailyProfitCap)
                    {
                        Print($"[DISPATCH] 🔒 SKIPPING {acct.Name} - Consistency Lock Active (${fleet[i].DailyPL:F2})");
                        continue;
                    }
                }

                // V12: Followers ALWAYS use RMA multipliers for point-based trails (User Req)
                bool useRmaForFollower = true; 
                
                double stopMult = useRmaForFollower ? RMAStopATRMultiplier : (tradeType == "TREND" && i == 0 ? TRENDEntry1ATRMultiplier : RMAStopATRMultiplier);
                double t1Price = Target1FixedPoints;
                double t2Mult = useRmaForFollower ? RMAT1ATRMultiplier : (tradeType == "TREND" && i == 0 ? Target3Multiplier : RMAT1ATRMultiplier);
                double t3Mult = useRmaForFollower ? RMAT2ATRMultiplier : (tradeType == "TREND" && i == 0 ? (Target3Multiplier * 1.5) : RMAT2ATRMultiplier);

                // Calculate fixed prices
                double stopDist = Math.Min(currentATR * stopMult, MaximumStop);
                double t2Dist = currentATR * t2Mult;
                double t3Dist = currentATR * t3Mult;

                double stopPrice = (action == OrderAction.Buy) ? entryPrice - stopDist : entryPrice + stopDist;
                double t1TargetPrice = (action == OrderAction.Buy) ? entryPrice + t1Price : entryPrice - t1Price;
                double t2TargetPrice = (action == OrderAction.Buy) ? entryPrice + t2Dist : entryPrice - t2Dist;

                // Rounding
                stopPrice = Instrument.MasterInstrument.RoundToTickSize(stopPrice);
                t1TargetPrice = Instrument.MasterInstrument.RoundToTickSize(t1TargetPrice);
                t2TargetPrice = Instrument.MasterInstrument.RoundToTickSize(t2TargetPrice);

                try
                {
                    string ocoId = tradeType + "_" + DateTime.Now.Ticks + "_" + i;
                    string fleetEntryName = "Fleet_" + acct.Name + "_" + tradeType + "_" + i;

                    // V12.3: Entry uses caller-specified order type (Limit for RMA, Market for MOMO/TREND)
                    double limitPx = (entryOrderType == OrderType.Limit) ? entryPrice : 0;
                    bool isMarketEntry = (entryOrderType == OrderType.Market);
                    Order entry = acct.CreateOrder(Instrument, action, entryOrderType, TimeInForce.Gtc, quantity, limitPx, 0, ocoId, tradeType, null);

                    // V12.7: For Limit entries, defer bracket submission until fill.
                    // For Market entries, submit entry + stop + target together (instant fill expected).
                    Order stop = null;
                    Order target = null;
                    if (isMarketEntry)
                    {
                        stop = acct.CreateOrder(Instrument, action == OrderAction.Buy ? OrderAction.Sell : OrderAction.BuyToCover,
                            OrderType.StopMarket, TimeInForce.Gtc, quantity, 0, stopPrice, ocoId, "Stop_" + tradeType, null);
                        target = acct.CreateOrder(Instrument, action == OrderAction.Buy ? OrderAction.Sell : OrderAction.BuyToCover,
                            OrderType.Limit, TimeInForce.Gtc, quantity, t2TargetPrice, 0, ocoId, "Target_" + tradeType, null);
                    }

                    // V12.1: Track Follower Position for Active Trailing Stop Management
                    PositionInfo fleetPos = new PositionInfo
                    {
                        SignalName = fleetEntryName,
                        Direction = action == OrderAction.Buy ? MarketPosition.Long : MarketPosition.Short,
                        TotalContracts = quantity,
                        RemainingContracts = quantity,
                        EntryPrice = entryPrice,
                        InitialStopPrice = stopPrice,
                        CurrentStopPrice = stopPrice,
                        Target1Price = t1TargetPrice,
                        Target2Price = t2TargetPrice,
                        Target3Price = t2TargetPrice, // Simplified for followers
                        ExecutingAccount = acct,
                        IsFollower = true,
                        IsRMATrade = true,          // Enforce Point-Based Trailing for all followers
                        IsTRENDTrade = (tradeType == "TREND"),
                        IsRetestTrade = (tradeType == "RETEST"),
                        EntryFilled = isMarketEntry, // V12.3: Only true for Market entries; Limit waits for fill
                        BracketSubmitted = isMarketEntry, // V12.7: Brackets deferred for Limit entries
                        TicksSinceEntry = 0,
                        ExtremePriceSinceEntry = entryPrice,
                        CurrentTrailLevel = 0
                    };

                    activePositions[fleetEntryName] = fleetPos;
                    entryOrders[fleetEntryName] = entry; // V12.3: Track entry for CIT chase
                    if (stop != null) stopOrders[fleetEntryName] = stop;
                    if (target != null) target2Orders[fleetEntryName] = target;

                    // V12.7: Submit only entry for Limit, full bracket for Market
                    if (isMarketEntry)
                        acct.Submit(new[] { entry, stop, target });
                    else
                        acct.Submit(new[] { entry });

                    int delta = (action == OrderAction.Buy) ? quantity : -quantity;
                    expectedPositions.AddOrUpdate(acct.Name, delta, (k, v) => v + delta);

                    rmaCount++;
                }
                catch (Exception ex)
                {
                    Print($"[DISPATCH] ✗ FAILED on {acct.Name}: {ex.Message}");
                }
            }

            Print($"[DISPATCH] COMPLETED: {trendCount} Trend / {rmaCount} RMA Trades Assigned");
        }

        /// <summary>
        /// V12 SIMA: Enumerate and log all connected accounts matching the AccountPrefix
        /// </summary>
        private void EnumerateApexAccounts()
        {
            simaAccountCount = 0;
            Print("[SIMA] ═══════════════════════════════════════════════════");
            Print("[SIMA] V12.11 - Fleet Symmetry & Safety Hardening Initializing");
            Print($"[SIMA] Account Prefix Filter: \"{AccountPrefix}\"");
            Print("[SIMA] ───────────────────────────────────────────────────");

            foreach (Account acct in Account.All)
            {
                if (acct.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    simaAccountCount++;
                    expectedPositions[acct.Name] = 0; // Initialize expected position as flat
                    accountDailyProfit[acct.Name] = 0; // Initialize daily profit
                    EnsureAccountComplianceTracking(acct.Name, GetComplianceNow());
                    activeFleetAccounts[acct.Name] = false; // V12.8 SIMA: Default to INACTIVE — wait for Fleet Manager / IPC to enable
                    
                    // V12.7: Always subscribe to execution updates for fleet bracket management
                    // (Also used by ComplianceHub for P/L tracking)
                    acct.ExecutionUpdate += OnAccountExecutionUpdate;
                    if (EnableComplianceHub)
                    {
                        Print($"[SIMA] ✓ {acct.Name} | COMPLIANCE MONITORING ACTIVE");
                    }
                    else
                    {
                        Print($"[SIMA] #{simaAccountCount}: {acct.Name} | Connected: {acct.Connection?.Status == ConnectionStatus.Connected} | Fleet: INACTIVE (awaiting IPC enable)");
                    }
                }
            }

            Print("[SIMA] ───────────────────────────────────────────────────");
            Print($"[SIMA] TOTAL ACCOUNTS DETECTED: {simaAccountCount} | ALL INACTIVE by default");
            Print("[SIMA] Use Fleet Manager or IPC TOGGLE_ACCOUNT to enable specific accounts");
            Print("[SIMA] ═══════════════════════════════════════════════════");
        }

        /// <summary>
        /// V12 SIMA: Execute a market order across ALL accounts matching the prefix
        /// </summary>
        private void ExecuteMultiAccountMarket(OrderAction action, int quantity, string signalName)
        {
            if (!EnableSIMA) return;

            int successCount = 0;
            int failCount = 0;

            foreach (Account acct in Account.All)
            {
                if (acct.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // V12.8: Fleet Active Check — skip accounts NOT registered or disabled
                    if (!activeFleetAccounts.TryGetValue(acct.Name, out bool isActive) || !isActive)
                    {
                        Print($"[SIMA] Fleet Dispatch: {acct.Name} SKIPPED (Inactive in Fleet Manager)");
                        continue;
                    }

                    try
                    {
                        // V12.1: Consistency Lock Check
                        if (EnableConsistencyLock)
                        {
                            double dailyPL = acct.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
                            if (dailyPL >= MaxDailyProfitCap)
                            {
                                Print($"[SIMA] 🔒 SKIPPING {acct.Name} - Consistency Lock Active (Day P/L: ${dailyPL:F2})");
                                continue;
                            }
                        }

                        Order order = acct.CreateOrder(Instrument, action, OrderType.Market,
                            TimeInForce.Gtc, quantity, 0, 0, "", signalName, null);
                        acct.Submit(new[] { order });

                        int delta = (action == OrderAction.Buy || action == OrderAction.BuyToCover) ? quantity : -quantity;
                        expectedPositions.AddOrUpdate(acct.Name, delta, (k, v) => v + delta);

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        Print($"[SIMA] ✗ FAILED on {acct.Name}: {ex.Message}");
                    }
                }
            }
            Print($"[SIMA] BROADCAST: {action} {quantity} | {successCount} OK / {failCount} FAIL");
        }

        /// <summary>
        /// V12 SIMA: Execute a Market Entry + Fixed Target/Stop across ALL accounts (Path B)
        /// Uses true broker-side OCO brackets for each account
        /// </summary>
        private void ExecuteMultiAccountBracket(OrderAction action, int quantity, string signalName, double stopPoints, double targetPoints)
        {
            if (!EnableSIMA) return;

            int successCount = 0;
            double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];

            foreach (Account acct in Account.All)
            {
                if (acct.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    try
                    {
                        // V12.1: Consistency Lock Check
                        if (EnableConsistencyLock)
                        {
                            double dailyPL = acct.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
                            if (dailyPL >= MaxDailyProfitCap)
                            {
                                Print($"[PATH B] 🔒 SKIPPING {acct.Name} - Consistency Lock Active (Day P/L: ${dailyPL:F2})");
                                continue;
                            }
                        }

                        // 1. Calculate Prices
                        double stopPrice = action == OrderAction.Buy ? currentPrice - stopPoints : currentPrice + stopPoints;
                        double targetPrice = action == OrderAction.Buy ? currentPrice + targetPoints : currentPrice - targetPoints;
                        
                        // Round to nearest tick
                        stopPrice = Math.Round(stopPrice / tickSize) * tickSize;
                        targetPrice = Math.Round(targetPrice / tickSize) * tickSize;

                        // 2. Create Bracket
                        string ocoId = action.ToString() + "_" + DateTime.Now.Ticks;
                        
                        Order entry = acct.CreateOrder(Instrument, action, OrderType.Market, TimeInForce.Gtc, quantity, 0, 0, ocoId, signalName, null);
                        
                        Order stop = acct.CreateOrder(Instrument, action == OrderAction.Buy ? OrderAction.Sell : OrderAction.BuyToCover, 
                            OrderType.StopMarket, TimeInForce.Gtc, quantity, 0, stopPrice, ocoId, "Stop_" + signalName, null);
                        
                        Order target = acct.CreateOrder(Instrument, action == OrderAction.Buy ? OrderAction.Sell : OrderAction.BuyToCover, 
                            OrderType.Limit, TimeInForce.Gtc, quantity, targetPrice, 0, ocoId, "Target_" + signalName, null);

                        // 3. Submit as Atomic Group (Broker OCO)
                        acct.Submit(new[] { entry, stop, target });

                        int delta = (action == OrderAction.Buy) ? quantity : -quantity;
                        expectedPositions.AddOrUpdate(acct.Name, delta, (k, v) => v + delta);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        Print($"[SIMA] ✗ BRACKET FAILED on {acct.Name}: {ex.Message}");
                    }
                }
            }
            Print($"[SIMA] PATH B BROADCAST: {successCount} Brackets Submitted");
        }

        /// <summary>
        /// V12 SIMA: Master Flatten - closes local position and broadcasts to the entire fleet
        /// </summary>
        // Duplicate FlattenAll removed - consolidated into line 4387 version

        /// <summary>
        /// V12 SIMA: RMA Entry V2 - Places limit entry + bracket on the local chart account,
        /// then iterates Account.All to place the same order on every fleet account matching AccountPrefix.
        /// CRITICAL: Every account's entry order is registered in entryOrders AND activePositions
        /// with a unique key (accountName + "_RMA") so ManageCIT can chase the entire fleet.
        /// </summary>
        private void ExecuteRMAEntryV2(double price, MarketPosition direction)
        {
            try
            {
                int qty = Math.Max(1, minContracts);

                // Calculate Stops & Targets using V12 RMA Logic
                double stopDist = Math.Min(currentATR * RMAStopATRMultiplier, MaximumStop);
                double t1Dist = Target1FixedPoints;
                double t2Dist = currentATR * RMAT1ATRMultiplier;

                double stopPrice = (direction == MarketPosition.Long) ? price - stopDist : price + stopDist;
                double t1Price = (direction == MarketPosition.Long) ? price + t1Dist : price - t1Dist;
                double t2Price = (direction == MarketPosition.Long) ? price + t2Dist : price - t2Dist;

                stopPrice = Instrument.MasterInstrument.RoundToTickSize(stopPrice);
                t1Price = Instrument.MasterInstrument.RoundToTickSize(t1Price);
                t2Price = Instrument.MasterInstrument.RoundToTickSize(t2Price);

                string baseSignal = "RMA_" + DateTime.Now.Ticks;
                OrderAction entryAction = (direction == MarketPosition.Long) ? OrderAction.Buy : OrderAction.SellShort;
                OrderAction exitAction = (direction == MarketPosition.Long) ? OrderAction.Sell : OrderAction.BuyToCover;

                Print($"[SIMA RMA V2] {direction} @ {price} | Stop: {stopPrice} | T1: {t1Price} | T2: {t2Price} | Qty: {qty}");

                // ═══════════════════════════════════════════════════════
                // 1. LOCAL ACCOUNT: SubmitOrderUnmanaged (chart-visible)
                // ═══════════════════════════════════════════════════════
                string localKey = baseSignal;
                Order entryOrder = SubmitOrderUnmanaged(0, entryAction, OrderType.Limit, qty, price, 0, "", localKey);
                if (entryOrder != null)
                {
                    entryOrders[localKey] = entryOrder;

                    PositionInfo pos = new PositionInfo
                    {
                        SignalName = localKey,
                        Direction = direction,
                        TotalContracts = qty,
                        RemainingContracts = qty,
                        EntryPrice = price,
                        InitialStopPrice = stopPrice,
                        CurrentStopPrice = stopPrice,
                        Target1Price = t1Price,
                        Target2Price = t2Price,
                        EntryFilled = false,
                        BracketSubmitted = false, // V12.7: Brackets deferred until entry fills
                        IsRMATrade = true
                    };
                    activePositions[localKey] = pos;

                    // V12.11: Register Master account in expectedPositions (was missing — caused false Reaper desyncs)
                    int localDelta = (direction == MarketPosition.Long) ? qty : -qty;
                    expectedPositions.AddOrUpdate(Account.Name, localDelta, (k, v) => v + localDelta);
                    Print($"[SIMA] Master expectedPositions updated: {Account.Name} delta={localDelta}");

                    // V12.7: Do NOT submit stop/target here — they will be submitted by
                    // SubmitBracketOrders() when the entry limit fills in OnOrderUpdate.
                    // Submitting them now would cause instant fills on marketable targets.

                    Print($"[SIMA RMA V2] LOCAL ENTRY ONLY (Limit): {localKey} | Brackets deferred until fill");
                }
                else
                {
                    Print("[SIMA RMA V2] ERROR: Local entry returned null");
                }

                // ═══════════════════════════════════════════════════════
                // 2. SIMA FLEET: Iterate Account.All for followers
                // ═══════════════════════════════════════════════════════
                if (!EnableSIMA)
                {
                    Print("[SIMA RMA V2] ⚠️ EnableSIMA is FALSE - Fleet dispatch SKIPPED. Enable SIMA in strategy parameters or send SET_SIMA|ON via IPC.");
                    return;
                }

                int fleetOk = 0;
                int fleetSkip = 0;

                foreach (Account acct in Account.All)
                {
                    if (acct.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    if (acct == this.Account) continue; // local already done

                    // V12.8: Fleet Manager toggle — skip if account NOT registered or explicitly disabled
                    if (!activeFleetAccounts.TryGetValue(acct.Name, out bool isActive) || !isActive)
                    {
                        Print($"[SIMA] Fleet Dispatch: {acct.Name} SKIPPED (Inactive in Fleet Manager)");
                        fleetSkip++;
                        continue;
                    }

                    // Consistency Lock
                    if (EnableConsistencyLock)
                    {
                        double dailyPL = acct.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
                        if (dailyPL >= MaxDailyProfitCap)
                        {
                            Print($"[SIMA RMA V2] SKIP {acct.Name} - ConsistencyLock (${dailyPL:F2})");
                            fleetSkip++;
                            continue;
                        }
                    }

                    try
                    {
                        string fleetKey = acct.Name + "_RMA_" + baseSignal;
                        string ocoId = fleetKey;

                        // V12.10: Submit ENTRY ONLY — brackets deferred until fill (unified with leader)
                        Order fEntry = acct.CreateOrder(Instrument, entryAction, OrderType.Limit,
                            TimeInForce.Gtc, qty, price, 0, ocoId, fleetKey, null);

                        acct.Submit(new[] { fEntry });

                        // Register in unified dictionaries so CIT + trailing works for this account
                        entryOrders[fleetKey] = fEntry;
                        activePositions[fleetKey] = new PositionInfo
                        {
                            SignalName = fleetKey,
                            Direction = direction,
                            TotalContracts = qty,
                            RemainingContracts = qty,
                            EntryPrice = price,
                            InitialStopPrice = stopPrice,
                            CurrentStopPrice = stopPrice,
                            Target1Price = t1Price,
                            Target2Price = t2Price,
                            Target3Price = t2Price,
                            EntryFilled = false,
                            IsRMATrade = true,
                            IsFollower = true,
                            ExecutingAccount = acct,
                            BracketSubmitted = false,   // V12.10: deferred — OnAccountExecutionUpdate submits on fill
                            ExtremePriceSinceEntry = price,
                            CurrentTrailLevel = 0
                        };
                        // stopOrders and target2Orders set by OnAccountExecutionUpdate on fill

                        int delta = (direction == MarketPosition.Long) ? qty : -qty;
                        expectedPositions.AddOrUpdate(acct.Name, delta, (k, v) => v + delta);

                        fleetOk++;
                    }
                    catch (Exception ex)
                    {
                        Print($"[SIMA RMA V2] FAIL {acct.Name}: {ex.Message}");
                    }
                }

                Print($"[SIMA RMA V2] Fleet: {fleetOk} dispatched, {fleetSkip} skipped");
            }
            catch (Exception ex)
            {
                Print($"[SIMA RMA V2] ERROR: {ex.Message}");
            }
        }



        private void FlattenAllApexAccounts()
        {
            if (!EnableSIMA)
            {
                Print("[SIMA] DISABLED - Using single-account flatten");
                FlattenAll(); // Call consolidated flatten
                return;
            }

            isFlattenRunning = true; // V12.8: Guard for Reaper + OnAccountExecutionUpdate
            try
            {
                Print("[SIMA] ══════ GLOBAL FLATTEN START ══════");
                int flattenCount = 0;
                int totalCount = 0;

                // V12.9: Flatten ALL matching accounts regardless of Fleet Manager status.
                // This is a safety mechanism — "Flatten All" must always be able to close everything.
                foreach (Account acct in Account.All)
                {
                    if (acct.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        totalCount++;
                        try
                        {
                            // Collect instruments with open positions on this account
                            List<Instrument> instrumentsToFlatten = new List<Instrument>();
                            foreach (Position position in acct.Positions)
                            {
                                if (position.MarketPosition != MarketPosition.Flat)
                                {
                                    instrumentsToFlatten.Add(position.Instrument);
                                }
                            }

                            if (instrumentsToFlatten.Count > 0)
                            {
                                acct.Flatten(instrumentsToFlatten);
                                flattenCount++;
                                Print($"[SIMA] ✓ Flattened {instrumentsToFlatten.Count} position(s) on {acct.Name}");
                            }

                            // Reset expected position
                            expectedPositions[acct.Name] = 0;
                        }
                        catch (Exception ex)
                        {
                            Print($"[SIMA] ✗ FLATTEN FAILED on {acct.Name}: {ex.Message}");
                        }
                    }
                }

                // V12.11: Explicitly flatten the Master account if it was NOT covered by the prefix filter.
                // Bug fix: If Master is "Sim101" and AccountPrefix is "Apex", the loop above skips it entirely.
                bool masterCovered = Account.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!masterCovered)
                {
                    totalCount++;
                    try
                    {
                        List<Instrument> masterInstruments = new List<Instrument>();
                        foreach (Position position in Account.Positions)
                        {
                            if (position.MarketPosition != MarketPosition.Flat)
                            {
                                masterInstruments.Add(position.Instrument);
                            }
                        }

                        if (masterInstruments.Count > 0)
                        {
                            Account.Flatten(masterInstruments);
                            flattenCount++;
                            Print($"[SIMA] V12.11 Master flatten: {masterInstruments.Count} position(s) on {Account.Name} (outside prefix filter)");
                        }

                        expectedPositions[Account.Name] = 0;
                    }
                    catch (Exception ex)
                    {
                        Print($"[SIMA] V12.11 Master FLATTEN FAILED on {Account.Name}: {ex.Message}");
                    }
                }

                Print($"[SIMA] ══════ GLOBAL FLATTEN COMPLETE: {flattenCount} flattened across {totalCount} accounts ══════");
            }
            finally
            {
                isFlattenRunning = false; // V12.8: Always release guard, even on exception
            }
        }

        /// <summary>
        /// V12 SIMA: Start the Reaper audit background thread
        /// </summary>
        private void StartReaperAudit()
        {
            if (isReaperRunning) return;

            isReaperRunning = true;
            reaperThread = new Thread(ReaperLoop)
            {
                IsBackground = true,
                Name = "V12_Reaper_Audit"
            };
            reaperThread.Start();
            Print("[REAPER] Audit thread STARTED - interval: " + ReaperIntervalMs + "ms");
        }

        /// <summary>
        /// V12 SIMA: Stop the Reaper audit background thread
        /// </summary>
        private void StopReaperAudit()
        {
            if (!isReaperRunning) return;

            isReaperRunning = false;
            try
            {
                if (reaperThread != null && reaperThread.IsAlive)
                {
                    reaperThread.Join(2000); // Wait up to 2 seconds
                }
            }
            catch { }
            Print("[REAPER] Audit thread STOPPED");
        }

        /// <summary>
        /// V12 SIMA: Reaper main loop - audits positions every ReaperIntervalMs
        /// </summary>
        private void ReaperLoop()
        {
            Print("[REAPER] Loop started - monitoring account positions...");

            while (isReaperRunning)
            {
                try
                {
                    Thread.Sleep(ReaperIntervalMs);
                    if (!isReaperRunning) break;

                    // V12.8: Pause auditing while a flatten is actively running to prevent race conditions
                    if (isFlattenRunning) continue;

                    AuditApexPositions();
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Print("[REAPER] ERROR: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// V12 SIMA: Audit all Apex account positions for desync
        /// If any account has a position that doesn't match expected, log it
        /// </summary>
        private void AuditApexPositions()
        {
            // Throttle logging to once per 30 seconds
            bool shouldLog = (DateTime.Now - lastReaperLog).TotalSeconds >= 30;
            int auditedCount = 0;
            int activeCount = 0;

            foreach (Account acct in Account.All)
            {
                if (acct.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    auditedCount++;

                    // Get actual position on this instrument
                    Position pos = acct.Positions.FirstOrDefault(p => p.Instrument.FullName == Instrument.FullName);
                    int actualQty = 0;

                    if (pos != null && pos.MarketPosition != MarketPosition.Flat)
                    {
                        actualQty = pos.MarketPosition == MarketPosition.Long ? pos.Quantity : -pos.Quantity;
                    }

                    // Compare with expected
                    int expectedQty = 0;
                    expectedPositions.TryGetValue(acct.Name, out expectedQty);

                    // V12.9: Only log individual accounts when they have non-zero state (reduces spam)
                    if (shouldLog && (expectedQty != 0 || actualQty != 0))
                    {
                        Print($"[REAPER] {acct.Name}: Expected={expectedQty}, Actual={actualQty}");
                        activeCount++;
                    }

                    // Desync detection (V12.1 Path B: Hybrid Recovery)
                    if (expectedQty != actualQty)
                    {
                        // V12.1: Filter Legal Desyncs
                        // If Follower is FLAT but Master is POSITIVE (Expected), this is a "Legal Pull" (Path B target hit).
                        // We do NOT flatten or panic here.
                        if (actualQty == 0 && expectedQty != 0)
                        {
                            if (shouldLog) Print($"[REAPER] {acct.Name} is Flat (Path B Target/Stop hit). Master still active.");
                            continue; 
                        }

                        // CRITICAL: Opposite direction or Ghost position (Active but shouldn't be)
                        bool isCriticalDesync = (actualQty != 0 && expectedQty == 0) || (Math.Sign(actualQty) != Math.Sign(expectedQty) && expectedQty != 0);

                        if (isCriticalDesync)
                        {
                            // V12.8: Throttle CRITICAL DESYNC logging to same shouldLog cadence to prevent output spam
                            if (shouldLog)
                                Print($"[REAPER] 🚨 CRITICAL DESYNC on {acct.Name}: Expected={expectedQty}, Actual={actualQty}");

                            if (AutoFlattenDesync)
                            {
                                if (shouldLog)
                                    Print($"[REAPER] 💀 AUTO-FLATTENING {acct.Name} - Emergency Re-sync!");
                                try
                                {
                                    acct.Flatten(new[] { Instrument });
                                    expectedPositions[acct.Name] = 0;
                                }
                                catch (Exception ex)
                                {
                                    Print($"[REAPER] ✗ FAILED to flatten {acct.Name}: {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            // Minor qty mismatch or other non-critical state
                            if (shouldLog) Print($"[REAPER] Minor Desync on {acct.Name}: Expected={expectedQty}, Actual={actualQty}");
                        }
                    }
                }
            }

            // V12.11: Explicitly audit the Master account if it was NOT covered by the prefix filter.
            // Bug fix: Master "Sim101" with AccountPrefix "Apex" was invisible to the Reaper.
            bool masterAudited = Account.Name.IndexOf(AccountPrefix, StringComparison.OrdinalIgnoreCase) >= 0;
            if (!masterAudited)
            {
                auditedCount++;

                Position masterPos = Account.Positions.FirstOrDefault(p => p.Instrument.FullName == Instrument.FullName);
                int masterActualQty = 0;
                if (masterPos != null && masterPos.MarketPosition != MarketPosition.Flat)
                {
                    masterActualQty = masterPos.MarketPosition == MarketPosition.Long ? masterPos.Quantity : -masterPos.Quantity;
                }

                int masterExpectedQty = 0;
                expectedPositions.TryGetValue(Account.Name, out masterExpectedQty);

                if (shouldLog && (masterExpectedQty != 0 || masterActualQty != 0))
                {
                    Print($"[REAPER] {Account.Name} (Master): Expected={masterExpectedQty}, Actual={masterActualQty}");
                    activeCount++;
                }

                if (masterExpectedQty != masterActualQty)
                {
                    if (masterActualQty == 0 && masterExpectedQty != 0)
                    {
                        if (shouldLog) Print($"[REAPER] {Account.Name} (Master) is Flat (Target/Stop hit). Expected was {masterExpectedQty}.");
                    }
                    else
                    {
                        bool isCriticalDesync = (masterActualQty != 0 && masterExpectedQty == 0) || (Math.Sign(masterActualQty) != Math.Sign(masterExpectedQty) && masterExpectedQty != 0);

                        if (isCriticalDesync)
                        {
                            if (shouldLog)
                                Print($"[REAPER] CRITICAL DESYNC on {Account.Name} (Master): Expected={masterExpectedQty}, Actual={masterActualQty}");

                            if (AutoFlattenDesync)
                            {
                                if (shouldLog)
                                    Print($"[REAPER] AUTO-FLATTENING {Account.Name} (Master) - Emergency Re-sync!");
                                try
                                {
                                    Account.Flatten(new[] { Instrument });
                                    expectedPositions[Account.Name] = 0;
                                }
                                catch (Exception ex)
                                {
                                    Print($"[REAPER] FAILED to flatten {Account.Name} (Master): {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            if (shouldLog) Print($"[REAPER] Minor Desync on {Account.Name} (Master): Expected={masterExpectedQty}, Actual={masterActualQty}");
                        }
                    }
                }
            }

            if (shouldLog)
            {
                // V12.9: Single summary line instead of 12 "Expected=0, Actual=0" per cycle
                if (activeCount == 0)
                    Print($"[REAPER] Heartbeat: All {auditedCount} accounts flat.");
                else
                    Print($"[REAPER] Heartbeat: {activeCount}/{auditedCount} accounts with positions.");
                lastReaperLog = DateTime.Now;
            }
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

            lock (dailySummaryLock)
            {
                EnsureDailySummaryCsv();
                System.IO.File.AppendAllText(dailySummaryCsvPath, line + Environment.NewLine);
            }
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
                            if (activePositions.TryGetValue(fleetKey, out var pos) && pos.IsFollower && !pos.BracketSubmitted && !pos.EntryFilled)
                            {
                                pos.EntryFilled = true;
                                pos.EntryPrice = e.Execution.Price; // Update to actual fill price

                                // Submit deferred brackets on the follower's account
                                Account acct = pos.ExecutingAccount;
                                if (acct != null)
                                {
                                    OrderAction exitAction = pos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;
                                    double validatedStop = ValidateStopPrice(pos.Direction, pos.CurrentStopPrice);
                                    string ocoId = "B_" + fleetKey;

                                    Order fStop = acct.CreateOrder(Instrument, exitAction, OrderType.StopMarket,
                                        TimeInForce.Gtc, pos.TotalContracts, 0, validatedStop, ocoId, "Stop_" + fleetKey.Substring(0, Math.Min(fleetKey.Length, 40)), null);
                                    Order fTarget = acct.CreateOrder(Instrument, exitAction, OrderType.Limit,
                                        TimeInForce.Gtc, pos.TotalContracts, pos.Target2Price, 0, ocoId, "Tgt_" + fleetKey.Substring(0, Math.Min(fleetKey.Length, 40)), null);

                                    acct.Submit(new[] { fStop, fTarget });
                                    stopOrders[fleetKey] = fStop;
                                    target2Orders[fleetKey] = fTarget;
                                    pos.BracketSubmitted = true;

                                    Print($"[SIMA V12.7] Fleet brackets submitted on fill: {fleetKey} | Stop: {validatedStop:F2} | Target: {pos.Target2Price:F2}");
                                }
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

                System.IO.File.WriteAllText(complianceLogPath, sb.ToString());
                lastComplianceLog = DateTime.Now;
            }
            catch (Exception ex)
            {
                Print("[COMPLIANCE] ERROR writing log: " + ex.Message);
            }
        }

        #endregion

        #region V12.2 Position Sizing Helpers

        private int CalculatePositionSize(double stopDistance)
        {
            if (stopDistance <= 0) return Math.Max(1, minContracts);

            double riskToUse = MaxRiskAmount;

            double stopDistanceInDollars = stopDistance * pointValue;
            if (stopDistanceInDollars <= 0) return Math.Max(1, minContracts);

            int contracts = (int)Math.Floor(riskToUse / stopDistanceInDollars);
            contracts = Math.Max(1, contracts);

            Print($"[V12.2 SIZING] Stop={stopDistance:F2} | Risk=${riskToUse:F0} | Contracts={contracts}");
            return contracts;
        }

        private void GetTargetDistribution(int contracts, out int t1, out int t2, out int t3, out int t4)
        {
            if (contracts <= 1)
            {
                t1 = 1; t2 = 0; t3 = 0; t4 = 0;
            }
            else if (contracts == 2)
            {
                t1 = 1; t2 = 0; t3 = 0; t4 = 1;
            }
            else if (contracts == 3)
            {
                t1 = 1; t2 = 1; t3 = 0; t4 = 1;
            }
            else if (contracts == 4)
            {
                t1 = 1; t2 = 1; t3 = 1; t4 = 1;
            }
            else
            {
                t1 = (int)Math.Floor(contracts * T1ContractPercent / 100.0);
                t2 = (int)Math.Floor(contracts * T2ContractPercent / 100.0);
                t3 = (int)Math.Floor(contracts * T3ContractPercent / 100.0);
                t4 = contracts - t1 - t2 - t3;

                // Enforce minimums for T1 and Runner
                if (t1 < 1) t1 = 1;
                if (t1 < 1) t1 = 1;
                if (t4 < 1) t4 = 1;
            }
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

                UpdateDisplay();
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

                UpdateDisplay();
            }
            catch (Exception ex)
            {
                Print(string.Format("ERROR ExecuteRunnerAction ({0}): {1}", action, ex.Message));
            }
        }

    }
}
// V12.9 REPAIRED - Single-Instance Multi-Account Copy Trading Engine
