// PTT-COPIER-B57-T1 -- CopyEngine.cs
// B57 T1 CHANGES:
//   1. SendCopy: capture CreateOrder return value + call follower.Submit(new[]{order}).
//      Root cause fix: AddOn CreateOrder leaves order at Initialized; Submit() sends to exchange.
//      Pattern: same as SubmitBeStop (line ~400). DW-B57-01.
// PTT-COPIER B57 | submit-after-create-order | 2026-08-10
// PTT-COPIER-B56-LaneA-T1 -- CopyEngine.cs
// B56 T1 CHANGES:
//   1. Added IsDispatchTriggerState(OrderState) -- internal static predicate, CYC=2. (DW-B56-01 Gap 1)
//   2. DispatchCopy Gate 3: replaced raw Submitted check with IsDispatchTriggerState. (DW-B56-01 Gap 1)
//   3. OnOrderUpdate Cancelled block: propagate leader cancel to follower entry orders. (DW-B56-01 Gap 2)
// PTT-COPIER B56 | limit-order-gate3-fix + leader-cancel-propagation | 2026-08-09
// PTT-COPIER-B14-T1 -- CopyEngine.cs
// B14 T1 CHANGES:
//   1. Added trail BE fields (B27: replaced with _trailBeSlots + _trailBeLastPnlBits per DW-B27-01).
//   2. Added ArmTrailBe(Instrument, Account, int) -- CYC=4.
//   3. Added DisarmTrailBe(Account) -- CYC=4.
//   4. Added OnTrailBeAccountUpdate -- CYC=5, fires on NT8 account bg thread.
// PTT-COPIER-B12-T3 -- CopyEngine.cs
// B12 T3 CHANGES:
//   1. Added UpdateMaxRisk(double) -- pass-through to _atrEngine. CYC=2.
//   2. Added UpdateAtrFraction(double) -- pass-through to _atrEngine.SetAtrFraction. CYC=2.
// PTT-COPIER-B12-T1 -- CopyEngine.cs
// B12 T1 CHANGES:
//   1. Added Trim(Instrument, int, double) overload -- limit exit ceil(qty/2). "PTT-TrimLimit".
//   2. Added Flatten(Instrument, int, double) overload -- limit exit full qty. "PTT-FlattenLimit".
//   3. Added PTT-prefix Gate 0.5 at top of DispatchCopy -- prevents cascade copy of PTT- signals.
//      CYC: 7 -> 8 (AT LIMIT; PASS).
// PTT-COPIER-B10-T3 -- CopyEngine.cs
// Pure logic singleton. Zero UI references. Both surfaces share this instance.
// Jane Street rules: JS-001, JS-003, JS-008, JS-010, JS-021, JS-023, JS-025
// B8 T1: per-account qty multiplier (DW-B7-01)
// B8 T2: FollowerAtmMode behavioral wiring (DW-B7-03)
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;

namespace PropTraderTools
{
    // V01: Binding record for _orderMap inner collection
    // JS-003: readonly struct prevents field transposition
    internal struct FollowerBinding
    {
        internal Account FollowerAccount     { get; private set; }
        internal string  FromEntrySignalName { get; private set; }

        internal FollowerBinding(Account account, string signalName)
        {
            FollowerAccount     = account;
            FromEntrySignalName = signalName;
        }
    }

    // V05: Position truth snapshot -- JS-003 (readonly struct prevents bool transposition)
    public struct PositionState
    {
        public bool HasOpenPosition   { get; private set; }
        public bool HasWorkingEntries { get; private set; }

        public PositionState(bool hasOpen, bool hasWorking)
        {
            HasOpenPosition   = hasOpen;
            HasWorkingEntries = hasWorking;
        }
    }

    // V06: ATM mode discriminated union -- JS-003 + JS-010
    // NT8 Roslyn: records with positional params generate IsExternalInit (CS0518). Use abstract class instead.
    public abstract class FollowerAtmMode
    {
        private FollowerAtmMode() { }   // JS-010: private base constructor -- no external subclassing
        public sealed class Inherit  : FollowerAtmMode { public Inherit() { } }   // B7 default
        public sealed class Market   : FollowerAtmMode { public Market()  { } }   // pure market
        public sealed class Named    : FollowerAtmMode
        {
            public string TemplateName { get; }
            public Named(string templateName) { TemplateName = templateName; }
        }
    }
    // B8: SendCopy switch + UI dropdown wired in T2.

    // B9 T3 -- Copy mode discriminated union (JS-023: volatile int backing for thread-safe reads/writes)
    internal enum CopyMode { Signal = 0, Mirror = 1, Clone = 2 }

    internal sealed class CopyEngine : ICopyEngine
    {
        // --- Singleton ---
        private static readonly CopyEngine _instance = new CopyEngine();
        public static CopyEngine Instance => _instance;

        // --- State ---
        private volatile bool _isCopyEnabled; // JS-023
        // B9 T1 -- ATR sizing engine integration (JS-023: volatile, ADV-002 fix)
        private volatile bool            _atrEnabled = false;   // JS-023
        private volatile AtrSizingEngine _atrEngine  = null;    // write/read on UI thread only
        // B9 T3 -- Mirror mode (JS-023: volatile int backing for CopyMode enum)
        private volatile int _copyModeValue = 0;   // 0=Signal (default), 1=Mirror
        // B50 -- _cloneAtmCache: volatile string holds ATM template name captured at Clone mode activation.
        // volatile string: reference-type writes are atomic on CLR 4.0+ (JS-023 compliant).
        // NT8-003: volatile double/float BANNED -- string is safe.
        private volatile string _cloneAtmCache = string.Empty;
        // B39 -- _globalBe: singleton reference to shared Global BE execution engine.
        // Lazily initialized; Panel and Window read via GlobalBe property (UI thread only).
        // JS-023: volatile null-check safe for singleton reads on CLR 4.0+.
        private PttGlobalBreakEven _globalBe = null;
        // B62: value changed from long (timestamp) to double (last dispatched LimitPrice).
        // Enables drag detection: same orderId + different price = leader dragged.
        // JS-025: ConcurrentDictionary is lock-free.
        private readonly ConcurrentDictionary<string, double> _dedupCache = new ConcurrentDictionary<string, double>(); // JS-025
        private ConcurrentBag<CopyRule> _rules = new ConcurrentBag<CopyRule>(); // Change 1: removed readonly
        private double _dailyCapFloor = -500.0; // Change 4

        // B27 -- Per-account BE slot structs (DW-B27-01: replaces singleton fields).
        // NT8-001: 'readonly' fields, NOT init setters. NT8-005: NOT 'readonly struct'.
        // NT8-004: struct in ConcurrentDictionary<string,TSlot> confirmed safe in NT8.
        private struct PendingBeSlot
        {
            internal readonly Account    Account;
            internal readonly Instrument Instrument;
            internal readonly int        BufferTicks;
            internal PendingBeSlot(Account a, Instrument i, int b)
            { Account = a; Instrument = i; BufferTicks = b; }
        }

        private struct TrailBeSlot
        {
            internal readonly Account    Account;
            internal readonly Instrument Instrument;
            internal readonly int        BufferTicks;
            internal TrailBeSlot(Account a, Instrument i, int b)
            { Account = a; Instrument = i; BufferTicks = b; }
        }

        // B27 -- Pending BE slot dictionary (DW-B27-01: replaces 4 singleton fields).
        // Key = account.Name. JS-021: TryGetValue/TryRemove/AddOrUpdate are lock-free.
        private readonly ConcurrentDictionary<string, PendingBeSlot> _pendingBeSlots
            = new ConcurrentDictionary<string, PendingBeSlot>();

        // B27 -- Trail BE slot dictionary (DW-B27-01: replaces 5 singleton fields).
        // LastPnlBits lives in _trailBeLastPnlBits (separate dict) because struct values
        // in ConcurrentDictionary are value types -- Interlocked CAS requires a ref to a
        // field, impossible on a boxed struct. NT8-003: no volatile on long.
        private readonly ConcurrentDictionary<string, TrailBeSlot>   _trailBeSlots
            = new ConcurrentDictionary<string, TrailBeSlot>();
        private readonly ConcurrentDictionary<string, long>           _trailBeLastPnlBits
            = new ConcurrentDictionary<string, long>();

        // V01: order map for follower bracket lookup
        // JS-025: ConcurrentDictionary (atomic GetOrAdd) + ConcurrentBag (lock-free Add/iterate)
        // JS-021: NO lock keyword anywhere
        private readonly ConcurrentDictionary<string, ConcurrentBag<FollowerBinding>> _orderMap
            = new ConcurrentDictionary<string, ConcurrentBag<FollowerBinding>>();

        // --- Status event ---
        internal event Action<string> StatusUpdate;

        // V05: position state change notification for UI surfaces
        // Fired from TryFirePositionState -- before Gate 1 (fires even when copy is disabled)
        public event Action<string, PositionState> PositionStateChanged;

        // B10 T2 -- Pending BE fired notification (fires on NT8 account bg thread; Panel marshals to UI)
        internal event Action<string, string> PendingBeFired;

        // B20-LANE-A T2: Copy ON/OFF sync event (DW-B17-SYNC-01)
        // Plain delegate field -- NOT lock-guarded (JS-021). Fired from SetEnabled on every toggle.
        // Lane C wires TradeCopierPanel and TradeCopierWindow subscribers.
        public event Action<bool> CopyEnabledChanged;

        // --- Nested structs ---

        private readonly struct CopyRule
        {
            internal readonly string Instrument;
            internal readonly Account MasterAccount;
            internal readonly Account[] FollowerAccounts;
            internal readonly bool Enabled; // Change 2

            // B8 T1: per-follower quantity multiplier (parallel to FollowerAccounts[])
            // null = all followers default to 1x. readonly int[] on readonly struct (JS-008).
            internal readonly int[] FollowerMultipliers;

            // V07: ATM template map per follower account -- logically immutable (written once at construction)
            // NT8 constraint: System.Collections.Immutable not available; use readonly Dictionary (never mutated after ctor).
            // NT8 constraint: readonly struct requires readonly field, not auto-property with setter (CS8341).
            internal readonly Dictionary<string, FollowerAtmMode> FollowerAtmTemplates;

            // B10 T3: TightenTicks -- default 5 ticks; stored on readonly struct (JS-008).
            // NT8-001: readonly field on readonly struct (not init-only property).
            // Backward compat: DtoToRule converts dto.TightenTicks > 0 ? dto.TightenTicks : 5.
            internal readonly int TightenTicks;

            // B8 T1: updated private constructor (adds multipliers + atmTemplates parameters)
            // B10 T3: updated to include tightenTicks
            private CopyRule(
                string instrument,
                Account master,
                Account[] followers,
                bool enabled,
                int[] multipliers,
                Dictionary<string, FollowerAtmMode> atmTemplates,
                int tightenTicks)
            {
                Instrument          = instrument;
                MasterAccount       = master;
                FollowerAccounts    = followers;
                Enabled             = enabled;
                FollowerMultipliers = multipliers;
                FollowerAtmTemplates = atmTemplates ?? new Dictionary<string, FollowerAtmMode>();
                TightenTicks        = tightenTicks > 0 ? tightenTicks : 5;
            }

            // B8 T1: updated factory -- new optional params preserve backward compat with all 27 existing tests
            // B10 T3: adds tightenTicks optional param (default 5)
            internal static CopyRule Create(
                string instrument,
                Account master,
                Account[] followers,
                bool enabled = true,
                int[] multipliers = null,
                Dictionary<string, FollowerAtmMode> atmTemplates = null,
                int tightenTicks = 5)
                => new CopyRule(instrument, master, followers, enabled, multipliers,
                    atmTemplates ?? new Dictionary<string, FollowerAtmMode>(), tightenTicks);
        }

        private readonly struct CopySignal
        {
            internal readonly OrderAction Action;
            internal readonly OrderType Type;
            internal readonly int Quantity;
            internal readonly double LimitPrice;
            internal readonly string OrderId;

            private CopySignal(OrderAction action, OrderType type, int qty, double limitPrice, string orderId)
            {
                Action = action;
                Type = type;
                Quantity = qty;
                LimitPrice = limitPrice;
                OrderId = orderId;
            }

            internal static CopySignal Create(
                OrderAction action,
                OrderType type,
                int qty,
                double limitPrice,
                string orderId
            ) => new CopySignal(action, type, qty, limitPrice, orderId);
        }

        private readonly struct TrimSignal
        {
            // NO qty field -- correctness by construction (JS-003)
            // Each account reads its own live position independently
            internal readonly DateTime UtcTime;
            internal readonly string Instrument;

            private TrimSignal(string instrument)
            {
                UtcTime = DateTime.UtcNow;
                Instrument = instrument;
            }

            internal static TrimSignal Create(string instrument) => new TrimSignal(instrument);
        }

        // --- Private constructor (singleton, prevents external instantiation -- JS-010) ---
        private CopyEngine() { }

        // --- Public API ---

        internal void SetEnabled(bool enabled)
        {
            _isCopyEnabled = enabled;
            StatusUpdate?.Invoke("Copy " + (enabled ? "ON" : "OFF"));
            CopyEnabledChanged?.Invoke(enabled);
        }

        // B54 -- IsEnabled: read-only view of _isCopyEnabled (JS-023: volatile bool read).
        // CYC=1. Used by TradeCopierPanel.OnLoaded snap and TradeCopierWindow.OnLoaded snap.
        public bool IsEnabled => _isCopyEnabled;

        // B39 -- GlobalBe: shared Global BE engine. Lazy-init on first access (UI thread only).
        // CYC=2 (null check + assignment).
        // JS-021: no lock -- CLR object reference assignment is atomic on 64-bit.
        // JS-002: always returns non-null; new PttGlobalBreakEven() as fallback.
        public PttGlobalBreakEven GlobalBe
        {
            get
            {
                if (_globalBe == null)
                    _globalBe = new PttGlobalBreakEven();
                return _globalBe;
            }
        }

        // Change 5: SetDailyCapFloor added immediately after SetEnabled
        internal void SetDailyCapFloor(double floor)
        {
            _dailyCapFloor = floor;
        }

        // B9 T1: CYC=1 -- straight-line assignment
        internal void SetAtrEngine(AtrSizingEngine engine, bool enabled)
        {
            _atrEngine  = engine;
            _atrEnabled = enabled;
        }

        // B12 T3 -- UpdateMaxRisk: pass-through to _atrEngine. Null-guarded. CYC=2.
        internal void UpdateMaxRisk(double maxRiskDollars)
        {
            if (_atrEngine == null) return;            // (1)
            _atrEngine.UpdateMaxRisk(maxRiskDollars);  // (2)
        }

        // B12 T3 -- UpdateAtrFraction: pass-through to _atrEngine.SetAtrFraction. Null-guarded. CYC=2.
        internal void UpdateAtrFraction(double fraction)
        {
            if (_atrEngine == null) return;            // (1)
            _atrEngine.SetAtrFraction(fraction);       // (2)
        }

        // B9 T3: CYC=1 -- straight-line cast and assign
        internal void SetCopyMode(CopyMode mode)
        {
            _copyModeValue = (int)mode;
        }

        // B9 T3: CYC=1 -- straight-line cast and return
        internal CopyMode GetCopyMode()
        {
            return (CopyMode)_copyModeValue;
        }

        // B58 ICopyEngine -- RelayBe: fan out pre-calculated BE price to all follower accounts.
        // BeEventArgs.BePrice is already computed by PttGlobalBreakEven/BE module before firing.
        // B66 DW-B66-BE-01: e.IsLong passed to SubmitBeStop (was relying on re-read inside method -- race).
        // CYC=2 (1 base + 1 foreach branch). JS-021: no lock -- AllAccounts snapshot; SubmitBeStop lock-free.
        // JS-002: void method, no return null. JS-033: synchronous void.
        public void RelayBe(BeEventArgs e)
        {
            foreach (var acc in AllAccounts(e.Instrument))
                SubmitBeStop(acc, e.Instrument, e.BePrice, e.IsLong);
        }

        // B58 ICopyEngine -- RelayTrim: delegate to Trim(Instrument) fan-out. CYC=1.
        // Trim(Instrument) at line 1006 iterates AllAccounts and calls TrimOneAccount per account.
        // JS-021: no lock. JS-002: void, no return null.
        public void RelayTrim(TrimEventArgs e) => Trim(e.Instrument);

        // B58 ICopyEngine -- RelayFlatten: delegate to Flatten(Instrument) fan-out. CYC=1.
        // Flatten(Instrument) at line 1012 iterates AllAccounts and calls FlattenOneAccount per account.
        // JS-021: no lock. JS-002: void, no return null.
        public void RelayFlatten(FlatEventArgs e) => Flatten(e.Instrument);

        // B58 ICopyEngine -- RelayCancel: delegate to CancelPendingEntries(Instrument) fan-out. CYC=1.
        // CancelPendingEntries(Instrument) at line 1192 iterates AllAccounts and calls CancelOneAccount.
        // JS-021: no lock. JS-002: void, no return null.
        public void RelayCancel(CancelEventArgs e) => CancelPendingEntries(e.Instrument);

        // B50 -- SetCloneAtmCache: CYC=1. Stores ATM template name for Clone mode dispatch.
        // Called from TradeCopierPanel.OnCloneModeClick after reading leader's current ATM template.
        // JS-023: volatile string write is atomic.
        internal void SetCloneAtmCache(string value)
        {
            _cloneAtmCache = value ?? string.Empty;
        }

        // B50 -- GetCloneAtmMode: CYC=2. Returns Named(cache) if cache non-empty, else Inherit.
        // Called by ResolveAtmMode when CopyMode == Clone.
        // JS-002: never returns null -- returns Inherit as fallback.
        internal FollowerAtmMode GetCloneAtmMode()
        {
            var cache = _cloneAtmCache;
            if (cache != null && cache.Length > 0)  // branch (1)
                return new FollowerAtmMode.Named(cache);
            return new FollowerAtmMode.Inherit();
        }

        // B56-LaneB: CYC=2 -- yield distinct instrument names for UI refresh after LoadRules.
        // JS-021: no lock -- ConcurrentBag foreach is lock-free.
        // JS-002: returns empty IEnumerable (not null) when _rules is empty.
        internal IEnumerable<string> GetRuleInstruments()
        {
            var seen = new HashSet<string>();
            foreach (var r in _rules)
                if (seen.Add(r.Instrument))
                    yield return r.Instrument;
        }

        // ── B56 BUILD-FIX stubs (pre-existing callers referenced these before they were added) ──

        // IsFollowerAccount: returns true if acc is a follower in any rule.
        // Called by PttBreakEven + PttGlobalQuickExit to skip follower accounts.
        // CYC=3: null guard(1) + foreach(2) + inner foreach(3). JS-021: no lock.
        internal bool IsFollowerAccount(Account acc)
        {
            if (acc == null) return false;                          // (1)
            foreach (var rule in _rules)                           // (2)
                foreach (var f in rule.FollowerAccounts)           // (3)
                    if (f != null && f.Name == acc.Name) return true;
            return false;
        }

        // GetQuickTicksForInstrument: returns (t1,t2) quick-exit tick defaults for an instrument.
        // Delegates to InstrumentDefaults -- rule-specific overrides deferred to future block.
        // CYC=2: null guard(1) + delegate(2). JS-002: returns tuple (not null).
        internal (int t1, int t2) GetQuickTicksForInstrument(NinjaTrader.Cbi.Instrument instr)
        {
            if (instr == null) return (4, 8);                      // (1)
            return InstrumentDefaults.GetQuickTicks(               // (2)
                instr.MasterInstrument?.Name ?? string.Empty);
        }

        // IsAtmBracketName: true if name is a standard NT8 ATM bracket order name.
        // NT8-REF: NT8_FULL_REFERENCE.md line 1631: "The order name such as 'Stop1' or 'Target2'"
        // CYC=1: expression body -- no if-branches in method body (Roslyn convention).
        // JS-021: no lock. JS-001: no throw. ASCII-only string literals.
        internal static bool IsAtmBracketName(string name) =>
            name == "Stop1" || name == "Stop2" || name == "Target1" || name == "Target2";

        // IsQxCancelCandidate: returns true if order should be cancelled by CancelQxBrackets.
        // Covers: ATM bracket names (via IsAtmBracketName), PTT-QX-* prefix, PTT-BE-* prefix.
        // CYC=5: 1 (base) + 4 if-branches. Roslyn: || inside single if = 1 decision point.
        // JS-021: no lock. JS-001: no throw. JS-002: returns bool (never null). ASCII-only.
        internal static bool IsQxCancelCandidate(Order o)
        {
            if (o == null || o.Name == null) return false;                               // (1)
            if (IsAtmBracketName(o.Name)) return true;                                   // (2)
            if (o.Name.StartsWith("PTT-QX-", StringComparison.Ordinal)) return true;    // (3)
            if (o.Name.StartsWith("PTT-BE-", StringComparison.Ordinal)) return true;    // (4)
            return false;
        }

        // CancelQxBrackets: cancel all Working/Initialized/Accepted ATM-bracket + PTT-* orders on acc for instr.
        // Called by PttQuickExit.Execute() before re-placing new bracket.
        // CYC=6: null guard(1) + foreach(2) + stateOk(3) + instrument check(4) + IsQxCancelCandidate(5) + staleCount(6).
        // JS-021: no lock. Predicate logic in IsQxCancelCandidate (CYC=5) + IsAtmBracketName (CYC=1).
        internal void CancelQxBrackets(Account acc, NinjaTrader.Cbi.Instrument instr)
        {
            if (acc == null || instr == null) return;              // (1)
            var stale = new System.Collections.Generic.List<Order>();
            foreach (Order o in acc.Orders)                        // (2)
            {
                bool stateOk = o.OrderState == OrderState.Working
                            || o.OrderState == OrderState.Initialized
                            || o.OrderState == OrderState.Accepted;
                if (!stateOk) continue;                            // (3)
                if (o.Instrument == null || o.Instrument.FullName != instr.FullName) continue;
                if (IsQxCancelCandidate(o))                           // (5) widened via helper
                    stale.Add(o);
            }
            if (stale.Count == 0) return;
            try { acc.Cancel(stale.ToArray()); }
            catch { }
        }

        // NextQxOcoId: monotonic OCO group ID for Quick Exit bracket pairs.
        // Uses Interlocked.Increment on _qxOcoSeq (thread-safe, no lock).
        // CYC=1: straight expression. JS-021: no lock -- Interlocked.
        private int _qxOcoSeq = 0;
        internal string NextQxOcoId()
            => "PTT-QX-" + System.Threading.Interlocked.Increment(ref _qxOcoSeq).ToString("D5");

        // B66 DW-B66-BE-01: SubmitBeStop -- submit a StopMarket order at bePrice for acc+instr.
        // FIX: isLong is now a parameter -- callers pass direction at their own snapshot-read time.
        // Removed: internal pos.MarketPosition re-read (was racing with NT8 position update lag --
        //   NT8_FULL_REFERENCE.md line 1721: "Changes to positions will not be reflected till at
        //   least the next OnBarUpdate() event after an order fill.").
        // B65 precedent: same race fixed in TryDispatchLeaderFlat (CopyEngine.cs lines 651-654).
        // CYC=7 (strict McCabe): null-guard(1) + pos-loop(2) + inner-if(3) + pos-null-guard(4)
        //         + ternary-dir(5) + if-order-null(6) + base(1) = 7. JS-021: no lock.
        // JS-001: no throw. JS-002: void. JS-033: synchronous void.
        internal void SubmitBeStop(Account acc, NinjaTrader.Cbi.Instrument instr, double bePrice, bool isLong)
        {
            if (acc == null || instr == null) return;              // (1)
            NinjaTrader.Cbi.Position pos = null;
            foreach (NinjaTrader.Cbi.Position p in acc.Positions) // (2)
                if (p.Instrument == instr) { pos = p; break; }    // (3)
            if (pos == null || pos.Quantity == 0) return;          // (4)
            OrderAction dir = isLong ? OrderAction.Sell : OrderAction.BuyToCover; // (5)
            try                                                    // (6) CreateOrder call
            {
                var order = acc.CreateOrder(
                    instr, dir, OrderType.StopMarket,
                    OrderEntry.Manual, TimeInForce.Gtc,
                    pos.Quantity, 0, bePrice,
                    string.Empty, "PTT-BE-Stop",
                    DateTime.MaxValue,
                    (NinjaTrader.Cbi.CustomOrder)null);
                if (order != null)                                 // (6) inner if
                    acc.Submit(new[] { order });
            }
            catch { }
        }

        // ArmAllPendingBe: arm pending break-even for all non-follower accounts.
        // Called by PttGlobalBreakEven.Execute(int bufferTicks).
        // CYC=4: foreach(1) + follower skip(2) + pos loop(3) + flat skip(4). JS-021: no lock.
        internal void ArmAllPendingBe(int bufferTicks)
        {
            foreach (Account acc in Account.All)                   // (1)
            {
                if (IsFollowerAccount(acc)) continue;              // (2) skip followers
                foreach (NinjaTrader.Cbi.Position pos in acc.Positions)  // (3)
                {
                    if (pos == null || pos.Quantity == 0) continue; // (4) skip flat
                    bool isLong = pos.MarketPosition == MarketPosition.Long;
                    double tick = pos.Instrument.MasterInstrument?.TickSize ?? 0.25;
                    double bePrice = Math.Round(
                        (pos.AveragePrice + (isLong ? bufferTicks : -bufferTicks) * tick) / tick
                    ) * tick;
                    SubmitBeStop(acc, pos.Instrument, bePrice, isLong);
                }
            }
        }

        // ── end B56 BUILD-FIX stubs ──

        // B9 T1: CYC=2 -- returns engine value when enabled; 1 otherwise
        internal int GetSuggestedQty(NinjaTrader.Cbi.Instrument instrument)
        {
            if (_atrEnabled && _atrEngine != null)
                return _atrEngine.GetSuggestedQty();
            return 1;
        }

        // Change 7: SetRuleEnabled added after SetDailyCapFloor
        internal void SetRuleEnabled(string instrument, bool enabled)
        {
            var snapshot = new List<CopyRule>(_rules);
            _rules = new ConcurrentBag<CopyRule>();
            foreach (var r in snapshot)
            {
                var updated =
                    r.Instrument == instrument
                        ? CopyRule.Create(r.Instrument, r.MasterAccount, r.FollowerAccounts, enabled,
                            r.FollowerMultipliers, r.FollowerAtmTemplates, r.TightenTicks)
                        : r;
                _rules.Add(updated);
            }
        }

        // Original 3-arg overload -- PRESERVED UNCHANGED (backward compat with all 27 existing tests)
        internal void AddRule(string instrument, Account master, Account[] followers)
        {
            _rules.Add(CopyRule.Create(instrument, master, followers));
        }

        // B8 T1: new 5-arg overload -- adds multipliers + ATM map at apply time
        // B23 T1 (DW-B22-ADDRULE-ACCUMULATE-01): replace-not-append for same (instrument, leader).
        // ConcurrentBag rebuild pattern -- no lock (JS-021). Same pattern as SetFollowerMultiplier.
        // CYC=4: foreach(1) + string == (2) + name == (3) + continue(4 -- implicit else branch).
        internal void AddRule(
            string instrument,
            Account master,
            Account[] followers,
            int[] multipliers,
            Dictionary<string, FollowerAtmMode> atmMap)
        {
            var snapshot = new List<CopyRule>(_rules);
            _rules = new ConcurrentBag<CopyRule>();
            foreach (var r in snapshot)
            {
                if (r.Instrument == instrument && r.MasterAccount?.Name == master?.Name)
                    continue;
                _rules.Add(r);
            }
            _rules.Add(CopyRule.Create(instrument, master, followers, true, multipliers, atmMap));
        }

        // B8 T1: post-create mutation of a single follower's multiplier
        // ConcurrentBag rebuild pattern -- no lock (JS-021)
        // Clamps multiplier to [1, 10] for safety
        internal void SetFollowerMultiplier(string instrument, int followerIndex, int multiplier)
        {
            int clamped = Math.Max(1, Math.Min(10, multiplier));
            var snapshot = new List<CopyRule>(_rules);
            _rules = new ConcurrentBag<CopyRule>();
            foreach (var r in snapshot)
            {
                if (r.Instrument != instrument)
                {
                    _rules.Add(r);
                    continue;
                }
                var newMults = BuildUpdatedMultipliers(r.FollowerMultipliers, followerIndex, clamped,
                    r.FollowerAccounts?.Length ?? 0);
                _rules.Add(CopyRule.Create(r.Instrument, r.MasterAccount, r.FollowerAccounts, r.Enabled,
                    newMults, r.FollowerAtmTemplates, r.TightenTicks));
            }
        }

        // Helper for SetFollowerMultiplier -- builds a new multiplier array with one entry updated.
        // CYC=3 (null guard + bounds guard + copy loop). No throw, no return null.
        private static int[] BuildUpdatedMultipliers(int[] existing, int index, int value, int count)
        {
            int len = count > 0 ? count : (existing != null ? existing.Length : 0);
            if (len == 0)
                return existing;
            var result = new int[len];
            for (int i = 0; i < len; i++)
                result[i] = (existing != null && i < existing.Length) ? existing[i] : 1;
            if (index >= 0 && index < len)
                result[index] = value;
            return result;
        }

        internal void Subscribe()
        {
            foreach (Account acc in Account.All)
                acc.OrderUpdate += OnOrderUpdate;
        }

        internal void Unsubscribe()
        {
            foreach (Account acc in Account.All)
                acc.OrderUpdate -= OnOrderUpdate;
        }

        // --- Hot path: restructured CYC=7 (B7-F0) ---
        private void OnOrderUpdate(object sender, OrderEventArgs e)
        {
            // Pre-gate: fire position state unconditionally (even when copy disabled)
            TryFirePositionState(e);
            // B62: evict dedup on terminal states so orderId is not permanently blocked.
            EvictDedup(e.Order.OrderId.ToString(), e.Order.OrderState);

            // Gate 1: enabled check
            if (!_isCopyEnabled)
                return;

            // Gate 2: find matching rule -- instrument AND master account must match
            CopyRule? matchedRule = null;
            foreach (var rule in _rules)
            {
                if (e.Order.Instrument.FullName == rule.Instrument && e.Order.Account.Name == rule.MasterAccount?.Name)
                {
                    matchedRule = rule;
                    break;
                }
            }

            if (matchedRule == null)
                return;

            // Gate 2.5: per-rule enable check
            if (!matchedRule.Value.Enabled)
                return;

            // B9 T3 -- Mirror mode relay (inserted after Gate 2.5, before Gate B)
            if ((CopyMode)_copyModeValue == CopyMode.Mirror)
                MirrorOrderUpdate(e.Order, matchedRule.Value);

            // B56 T1: propagate leader cancel to follower entry orders.
            // Fires when leader order is cancelled -- cancels all Initialized/Working
            // follower entry orders for this instrument via CancelOneAccount.
            // Placed BEFORE Gate B so bracket orders are not affected (they have their own path).
            if (e.Order.OrderState == OrderState.Cancelled)
            {
                foreach (var acc in matchedRule.Value.FollowerAccounts)
                {
                    if (acc == null) continue;
                    CancelOneAccount(acc, e.Order.Instrument);
                }
                return;
            }

            // DW-B60-01: leader went flat -- propagate close to followers
            if (TryDispatchLeaderFlat(
                    e.Order.Account, e.Order.Instrument, e.Order.OrderState, e.Order.Name,
                    matchedRule.Value,
                    IsFollowerAccount, HasOpenPosition, FlattenOneAccount)) return;

            // Gate B: bracket drag detection -- divert to HandleBracketChange path
            if (IsWorkingBracket(e.Order))
            {
                if (e.Order.FromEntrySignal != null)
                    PopulateOrderMap(e.Order.FromEntrySignal, e.Order.Account);
                HandleBracketChange(e.Order, matchedRule.Value);
                return;
            }

            // Gate C (B62/B66-LaneC): entry drag detection -- same orderId + new price = leader dragged.
            // Fires when state is Accepted or Working (the two states that carry updated price post-drag).
            // Widened in B66-LaneC to accept StopLimit in addition to Limit (DW-B64-01 fix).
            // NT8: StopLimit.LimitPrice==0 always; drag price lives in StopPrice -- use GetOrderPrice().
            // _dedupCache.TryGetValue: orderId was previously dispatched; compare stored price.
            if ((e.Order.OrderType == OrderType.Limit || e.Order.OrderType == OrderType.StopLimit)
                && (e.Order.OrderState == OrderState.Accepted || e.Order.OrderState == OrderState.Working))
            {
                double currentPrice = GetOrderPrice(e.Order);
                if (_dedupCache.TryGetValue(e.Order.OrderId.ToString(), out double storedPrice)
                    && Math.Abs(currentPrice - storedPrice) >= (e.Order.Instrument?.MasterInstrument?.TickSize ?? 0.01))
                {
                    HandleEntryChange(e.Order, matchedRule.Value);
                    return;
                }
            }

            // No bracket, no drag -- normal copy dispatch
            DispatchCopy(e.Order, matchedRule.Value);
        }

        // --- B9 T3: Mirror mode methods ---

        // CYC=2 -- Filled state check + IsBracketLeg check (AND short-circuit = 2 decision points)
        // TESTABILITY: internal static with primitive parameters -- directly testable without NT8 runtime.
        internal static bool ShouldMirrorClose(OrderState state, bool isBracketLeg)
        {
            return state == OrderState.Filled && isBracketLeg;
        }

        // CYC=3 -- null guard + ShouldMirrorClose branch + IsWorkingBracket branch
        private void MirrorOrderUpdate(Order masterOrder, CopyRule rule)
        {
            if (masterOrder == null) return;                                          // guard (1)
            bool isBracket = IsBracketLeg(masterOrder);
            if (ShouldMirrorClose(masterOrder.OrderState, isBracket))                // branch (2)
            {
                MirrorClose(masterOrder, rule);
                return;
            }
            if (IsWorkingBracket(masterOrder))                                       // branch (3)
                HandleBracketChange(masterOrder, rule);  // reuse existing -- no duplication
        }

        // CYC=4 -- instr null guard + foreach loop + acc null guard + pos null/qty guard
        // JS-001: try/catch around CreateOrder -- no throw in hot path.
        // NT8 constraint: "PTT-Mirror-Close" signal name starts with "PTT-".
        private void MirrorClose(Order masterOrder, CopyRule rule)
        {
            var instr = masterOrder.Instrument;
            if (instr == null) return;                                                // guard (1)
            foreach (var acc in rule.FollowerAccounts)                               // loop (2)
            {
                if (acc == null) continue;                                            // guard (3)
                var pos = FindPosition(acc, instr);
                if (pos == null || pos.Quantity == 0) continue;                      // guard (4)
                var action = pos.MarketPosition == MarketPosition.Long
                    ? OrderAction.Sell : OrderAction.BuyToCover;                     // ternary: not a branch
                try
                {
                    acc.CreateOrder(instr, action, OrderType.Market,
                        OrderEntry.Manual, TimeInForce.Gtc,  // B29 fix: Gtc matches ATM bracket TIF
                        pos.Quantity, 0, 0, null,
                        "PTT-Mirror-Close",    // signal name starts with "PTT-" (NT8 constraint)
                        DateTime.MaxValue, null);
                    StatusUpdate?.Invoke(acc.Name + ": mirror-close " + pos.Quantity);
                }
                catch (Exception ex)
                {
                    StatusUpdate?.Invoke("PTT-Mirror-Close error: " + ex.Message);
                }
            }
        }

        // B56 T1: IsDispatchTriggerState -- CYC=2. True for states that trigger follower placement.
        // Market orders fire Submitted; AddOn limit orders fire Accepted (skip Submitted).
        // JS-002: returns bool (not null). JS-021: no lock. NT8 confirmed state set.
        // TESTABILITY: internal static with primitive OrderState param -- directly testable without NT8 runtime.
        //   Same pattern as ShouldMirrorClose(OrderState state, bool isBracketLeg).
        internal static bool IsDispatchTriggerState(OrderState state)
            => state == OrderState.Submitted   // market orders
            || state == OrderState.Accepted;   // limit orders (AddOn path)

        // B59 T1: IsExitSignalName -- CYC=6. Returns true for names that must not trigger follower copy.
        // Covers: (1) PTT- own signals; (2) NT8 Close button; (3) NT8 Flatten; (4) NT8 Rev reversal;
        //         (5) NT8 "Exit..." prefix family. JS-001: no throw. JS-002: returns bool.
        // TESTABILITY: internal static with string param -- directly testable without NT8 runtime.
        internal static bool IsExitSignalName(string name)
        {
            if (name == null)                                              return false;
            if (name.StartsWith("PTT-",  StringComparison.Ordinal))       return true;
            if (name == "Close")                                           return true;
            if (name == "Flatten")                                         return true;
            if (name.StartsWith("Rev", StringComparison.Ordinal))         return true;
            if (name.StartsWith("Exit", StringComparison.Ordinal))        return true;
            return false;
        }

        // B65 T1: IsNativeExitName -- CYC=6. Returns true for NT8 built-in exit order names ONLY.
        // Distinct from IsExitSignalName: does NOT cover PTT- prefixed signals.
        // Rationale: Only native NT8 exits (Close/Flatten/Rev/Exit) can arrive in OnOrderUpdate
        // while the leader position has not yet updated -- see NT8_FULL_REFERENCE.md line 1721:
        //   "Changes to positions will not be reflected till at least the next OnBarUpdate() event
        //    after an order fill."
        // For these names, bypass the hasOpenPosition guard in TryDispatchLeaderFlat to avoid
        // the position-race and propagate the close immediately to followers (DW-B65-01 fix).
        // NT8-VERIFY-03/04: "IsNativeExitName" confirmed NOT present in NT8 Custom codebase.
        // JS-001: no throw. JS-002: returns bool (never null). JS-021: no lock.
        internal static bool IsNativeExitName(string name)
        {
            if (name == null)                                              return false;
            if (name == "Close")                                           return true;
            if (name == "Flatten")                                         return true;
            if (name.StartsWith("Rev",  StringComparison.Ordinal))        return true;
            if (name.StartsWith("Exit", StringComparison.Ordinal))        return true;
            return false;
        }



        // --- B7-F0: Bracket mirroring methods ---

        // B8 T1: DispatchCopy -- index-tracking loop replaces plain foreach.
        // CYC=8 (at limit). GetMultiplier + scaled signal per follower.
        // JS-001: no throw in hot path. JS-021: no lock.
        private void DispatchCopy(Order order, CopyRule rule)
        {
            // Gate 0.5: block PTT- cascade AND known NT8 exit signal names (B59). CYC: 7->8 (unchanged).
            if (IsExitSignalName(order.Name)) return;

            // Gate 3: must be a dispatch-trigger state (Submitted for market; Accepted for AddOn limit)
            if (!IsDispatchTriggerState(order.OrderState))
                return;

            // Gate 4: market or limit order type only
            bool isMarket = order.OrderType == OrderType.Market;
            bool isLimit  = order.OrderType == OrderType.Limit;
            if (!isMarket && !isLimit)
                return;

            // Gate 5: dedup -- reject duplicate event for same orderId
            // B62: pass limitPrice as second arg (price-keyed dedup).
            if (IsDedup(order.OrderId.ToString(), order.LimitPrice))
                return;

            // All gates passed -- build base signal
            var baseSignal = CopySignal.Create(
                order.OrderAction,
                order.OrderType,
                order.Quantity,
                order.LimitPrice,
                order.OrderId.ToString()
            );

            // B9 T1: ATR base qty -- overrides signal qty when ATR enabled, else uses signal qty
            int baseQty = _atrEnabled ? GetSuggestedQty(order.Instrument) : baseSignal.Quantity;

            // B8 T1: index-tracking loop applies per-follower multiplier
            int idx = 0;
            foreach (var acc in rule.FollowerAccounts)
            {
                if (acc == null) { idx++; continue; }
                if (!PassesDailyCapCheck(acc)) { idx++; continue; }
                int mult = GetMultiplier(rule, idx);
                var scaledSignal = CopySignal.Create(
                    baseSignal.Action,
                    baseSignal.Type,
                    baseQty * mult,
                    baseSignal.LimitPrice,
                    baseSignal.OrderId);
                var mode = ResolveAtmMode(rule, acc.Name);
                SendCopy(acc, order.Instrument, in scaledSignal, mode);
                idx++;
            }
        }

        // B8 T1: bounds-safe multiplier retrieval. CYC=3 (null guard + bounds guard + clamp).
        // Returns int >= 1; never returns < 1; never throws; never returns null (value type).
        // JS-001: no throw. JS-002: returns int (null impossible for value type).
        private static int GetMultiplier(CopyRule rule, int followerIndex)
        {
            if (rule.FollowerMultipliers == null)
                return 1;
            if (followerIndex < 0 || followerIndex >= rule.FollowerMultipliers.Length)
                return 1;
            int v = rule.FollowerMultipliers[followerIndex];
            return v < 1 ? 1 : (v > 10 ? 10 : v);
        }

        // CYC=3. Gate predicate for bracket detection in OnOrderUpdate.
        // B63: Accepted added -- NT8 bracket orders fire Accepted before (or instead of) Working.
        // NT8_FULL_REFERENCE.md line 1005: "some stop orders may only reach Accepted state".
        // Extending to Accepted is safe: SyncFollowerBracket price-delta guard absorbs double-fire.
        // JS-021: no lock. JS-001: no throw.
        internal static bool IsWorkingBracket(Order order)
        {
            return (order.OrderState == OrderState.Working
                    || order.OrderState == OrderState.Accepted)
                   && IsBracketLegStatic(order);
        }

        // B10 T1 -- IsTrailingStop: trailing stop detection predicate.
        // CYC=1: single return expression.
        // NT8: Order.TrailPrice does not exist (CS1061). Use OrderType.StopMarket as proxy.
        // Callers guard order != null before calling (IsStopAlreadyAtBe already has null guard; loop filters).
        private static bool IsTrailingStop(Order order)
        {
            // NT8: Order.TrailPrice does not exist. Trailing stops are StopMarket orders;
            // downstream logic (TightenStop cancel+replace path) handles trail correctly.
            return order.OrderType == OrderType.StopMarket;
        }

        // B10 T1 -- IsStopAlreadyAtBe: idempotency guard.
        // CYC=2: long branch(1), short branch(2). Guards against double-BE submissions.
        private static bool IsStopAlreadyAtBe(Order order, double newStop, bool isLong)
        {
            if (order == null)
                return false;
            if (isLong)
                return order.StopPrice >= newStop;
            return order.StopPrice <= newStop;
        }

        // B10 T1 -- SyncFollowerBracket: extracted inner loop body from HandleBracketChange.
        // CYC=5: fo null(1), price delta(2), TrailPrice>0(3), isStop branch(4), try block(0).
        // JS-001: try/catch around acc.Change() -- no throw in hot path.
        // DW-B9-GAP-001a: trailing stop follower orders are skipped (Option B: skip is safer).
        private void SyncFollowerBracket(Account acc, Order leaderOrder,
            bool isStop, double newPrice, double tickSize)
        {
            var fo = FindFollowerBracketOrder(acc, leaderOrder.FromEntrySignal, isStop);
            if (fo == null)                                                                // (1)
                return;

            double currentPrice = isStop ? fo.StopPrice : fo.LimitPrice;
            if (Math.Abs(newPrice - currentPrice) < tickSize)                             // (2)
                return;

            if (isStop && IsTrailingStop(fo))                                             // (3)
            {
                StatusUpdate?.Invoke("HandleBracketChange: skip trailing stop " + fo.Name);
                return;
            }

            try
            {
                if (isStop)                                                                // (4)
                    fo.StopPrice = newPrice;
                else
                    fo.LimitPrice = newPrice;
                acc.Change(new Order[] { fo });
                StatusUpdate?.Invoke(acc.Name + ": bracket synced " + (isStop ? "stop" : "target") + " -> " + newPrice);
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke(acc.Name + ": bracket sync error: " + ex.Message);
            }
        }

        // B10 T1 -- HandleBracketChange: delegates inner loop body to SyncFollowerBracket.
        // CYC=6: isStop(1), instr null(2), tickSize(3), rawPrice(4), foreach acc(5), acc null(6).
        // JS-001: try/catch inside SyncFollowerBracket -- no throw in hot path.
        // JS-021: no lock -- _orderMap uses ConcurrentDictionary (atomic).
        // V02: tick-rounded price applied BEFORE price-delta guard (preserved in SyncFollowerBracket).
        private void HandleBracketChange(Order leaderOrder, CopyRule rule)
        {
            bool isStop = IsStopLeg(leaderOrder);                                          // (1)

            var instrument = leaderOrder.Instrument;
            if (instrument == null)                                                        // (2)
                return;

            double tickSize = instrument.MasterInstrument?.TickSize ?? 0.0;               // (3)
            double rawPrice = isStop ? leaderOrder.StopPrice : leaderOrder.LimitPrice;    // (4)
            // V02: tick rounding applied BEFORE price-delta guard
            double newPrice = tickSize > 0
                ? Math.Round(rawPrice / tickSize) * tickSize
                : rawPrice;

            foreach (var acc in rule.FollowerAccounts)                                    // (5)
            {
                if (acc == null)                                                           // (6)
                    continue;
                SyncFollowerBracket(acc, leaderOrder, isStop, newPrice, tickSize);
            }
        }

        // CYC=4. Returns first matching working bracket order for the follower.
        // V03: return type is Order? (nullable) -- null contract explicit (JS-002 compliant).
        // V01: matching by FromEntrySignal name -- not leg-type scan.
        private Order? FindFollowerBracketOrder(Account follower, string fromEntrySignalName, bool isStop)
        {
            foreach (var order in follower.Orders.ToList())                                     // (1) branch
            {
                if (order.FromEntrySignal != fromEntrySignalName)                              // (1) branch
                    continue;
                if (order.OrderState != OrderState.Working)                                    // (1) branch
                    continue;
                if (isStop)
                {
                    if (order.OrderType == OrderType.StopMarket || order.OrderType == OrderType.StopLimit) // (1) branch
                        return order;
                }
                else
                {
                    if (order.OrderType == OrderType.Limit && !IsStopLeg(order))
                        return order;
                }
            }
            return null;
        }

        // CYC=2. Returns StopPrice for StopLimit orders, LimitPrice for all others.
        // NT8 fact: StopLimit.LimitPrice==0 always; drag price lives in StopPrice (Fact 1).
        // B66-LaneC: DW-B64-01 fix -- GetOrderPrice used in Gate C and HandleEntryChange.
        // JS-021: no lock. JS-001: no throw. Pure computation. Zero heap allocation (JS-036).
        private static double GetOrderPrice(Order order)
            => order.OrderType == OrderType.StopLimit ? order.StopPrice : order.LimitPrice;

        // CYC=2. Sets StopPrice for StopLimit follower orders, LimitPrice for all others.
        // NT8: for Account.Change() on StopLimit, assign StopPrice not LimitPrice
        //   (NT8_FULL_REFERENCE.md lines 898-899, Fact 2).
        // B66-LaneC: DW-B64-01 fix -- SetFollowerPrice replaces direct fo.LimitPrice assignment.
        // JS-021: no lock. JS-001: no throw. Pure field assignment.
        private static void SetFollowerPrice(Order fo, double newPrice)
        {
            if (fo.OrderType == OrderType.StopLimit)
                fo.StopPrice = newPrice;
            else
                fo.LimitPrice = newPrice;
        }

        // CYC=3: foreach (1), instrument guard (2), state+type+name compound guard (3).
        // B66-LaneC: widened state to Working||Accepted, type to Limit||StopLimit (DW-B64-01).
        // NT8: broker-simulated StopLimit may stay in Accepted (NT8_FULL_REFERENCE.md line 1005).
        // JS-002: returns null when not found -- callers must null-guard.
        private static Order? FindFollowerEntryOrder(Account follower, Instrument instrument)
        {
            foreach (var order in follower.Orders.ToList())                       // (1)
            {
                if (order.Instrument != instrument)                               // (2)
                    continue;
                if ((order.OrderState == OrderState.Working || order.OrderState == OrderState.Accepted) // (3)
                    && (order.OrderType == OrderType.Limit || order.OrderType == OrderType.StopLimit)
                    && order.Name == "PTT-Copy")
                    return order;
            }
            return null;
        }

        // B62/B66-LaneC: sync a leader entry drag to all follower working PTT-Copy entry orders.
        // B66-LaneC: widened to StopLimit via GetOrderPrice/SetFollowerPrice helpers (DW-B64-01).
        // Triggered by Gate C when leader's entry orderId is already in dedup cache but price changed.
        // CYC=6: instr null (1), tickSize ternary (2), foreach acc (3), acc null (4), fo null (5), price delta guard (6).
        // JS-001: try/catch around acc.Change() -- no throw in hot path.
        // JS-021: no lock -- _dedupCache is ConcurrentDictionary (lock-free).
        private void HandleEntryChange(Order leaderOrder, CopyRule rule)
        {
            var instrument = leaderOrder.Instrument;
            if (instrument == null)                                                    // (1)
                return;

            double tickSize = instrument.MasterInstrument?.TickSize ?? 0.0;           // (2)
            double rawPrice = GetOrderPrice(leaderOrder); // B66-LaneC: StopLimit price in StopPrice
            double newPrice = tickSize > 0
                ? Math.Round(rawPrice / tickSize) * tickSize
                : rawPrice;

            // Update stored price in dedup cache to track latest leader price.
            _dedupCache[leaderOrder.OrderId.ToString()] = newPrice;

            foreach (var acc in rule.FollowerAccounts)                                // (3)
            {
                if (acc == null)                                                       // (4)
                    continue;

                var fo = FindFollowerEntryOrder(acc, instrument);
                if (fo == null)                                                        // (5)
                    continue;

                double currentPrice = GetOrderPrice(fo); // B66-LaneC: StopLimit price in StopPrice
                if (tickSize > 0 && Math.Abs(newPrice - currentPrice) < tickSize)    // (6)
                    continue;

                try
                {
                    SetFollowerPrice(fo, newPrice); // B66-LaneC: StopLimit -> fo.StopPrice (NT8_FULL_REFERENCE.md lines 898-899)
                    acc.Change(new Order[] { fo });
                    StatusUpdate?.Invoke(acc.Name + ": entry dragged -> " + newPrice);
                }
                catch (Exception ex)
                {
                    StatusUpdate?.Invoke(acc.Name + ": entry drag error: " + ex.Message);
                }
            }
        }

        // CYC=2. Records (signal, follower) association in _orderMap for future bracket lookups.
        // JS-025: ConcurrentDictionary.GetOrAdd is atomic -- no lock needed.
        // Engineer Note #1: dedup guard prevents duplicate bindings on repeated Working state events.
        private void PopulateOrderMap(string fromEntrySignalName, Account followerAccount)
        {
            var bag = _orderMap.GetOrAdd(
                fromEntrySignalName,
                _ => new ConcurrentBag<FollowerBinding>());
            // Dedup guard: prevent accumulating duplicate bindings on repeated Working state events
            if (!bag.Any(b => b.FollowerAccount?.Name == followerAccount?.Name))         // (1) branch
                bag.Add(new FollowerBinding(followerAccount, fromEntrySignalName));
        }

        // CYC=2. Fires PositionStateChanged only on states that alter position truth.
        // Called BEFORE Gate 1 -- fires even when copy is disabled.
        // JS-003: PositionState readonly struct captured by value in event args (no aliasing).
        private void TryFirePositionState(OrderEventArgs e)
        {
            // Fire only on states that change position truth (NOT Working -- prevents spurious updates)
            var state = e.OrderState;
            if (state != OrderState.Filled &&
                state != OrderState.PartFilled &&
                state != OrderState.Cancelled &&
                state != OrderState.Rejected)                              // (1) branch
                return;

            if (e.Order?.Instrument?.FullName == null)                     // (1) branch
                return;

            string instr   = e.Order.Instrument.FullName;
            bool hasPos     = HasOpenPosition(e.Order.Account, e.Order.Instrument);
            bool hasEntries = HasWorkingEntries(e.Order.Account, e.Order.Instrument);
            PositionStateChanged?.Invoke(instr, new PositionState(hasPos, hasEntries));
        }

        // CYC=2. Thin wrapper over FindPosition.
        private bool HasOpenPosition(Account acc, Instrument instrument)
        {
            var pos = FindPosition(acc, instrument);                        // (1) branch
            if (pos == null)
                return false;
            return pos.Quantity > 0;
        }

        // B65 T1: TryDispatchLeaderFlat -- CYC=7 (strict McCabe: loop + null guard + 4 early returns + IsNativeExitName branch).
        // (1) state guard, (2) follower guard, (3) open-position race-safe guard, (4) foreach follower.
        // Guard (3) change: bypass hasOpenPosition when orderName is a native NT8 exit.
        // Rationale: NT8_FULL_REFERENCE.md line 1721 -- position state is not updated until the next
        // OnBarUpdate() after an order fill. When leader fills a native close order (Name="Close",
        // "Flatten", "Exit*", "Rev*"), position still shows open even though the order is filled.
        // Bypassing the guard here ensures followers are flattened immediately (DW-B65-01 fix).
        // JS-021: no lock. JS-001: no throw. JS-002: no null return.
        private static bool TryDispatchLeaderFlat(
            Account account, Instrument instrument, OrderState state, string orderName,
            CopyRule rule,
            Func<Account, bool> isFollower,
            Func<Account, Instrument, bool> hasOpenPosition,
            Action<Account, Instrument> flattenOne)
        {
            if (state != OrderState.Filled && state != OrderState.Cancelled) return false; // (1)
            if (isFollower(account)) return false;                                           // (2)
            if (!IsNativeExitName(orderName) && hasOpenPosition(account, instrument)) return false; // (3)
            foreach (var acc in rule.FollowerAccounts)                                       // (4)
            {
                if (acc == null) continue;
                flattenOne(acc, instrument);
            }
            return true;
        }

        // CYC=3. Returns true if any working non-bracket order exists for the instrument.
        private bool HasWorkingEntries(Account acc, Instrument instrument)
        {
            foreach (var order in acc.Orders)                               // (1) branch
            {
                if (order.Instrument != instrument)                         // (1) branch
                    continue;
                if (order.OrderState != OrderState.Working)                 // (1) branch
                    continue;
                if (!IsBracketLeg(order))
                    return true;
            }
            return false;
        }

        // B8 T2: SendCopy -- mode dispatch (CYC=5).
        // signalName is ALWAYS "PTT-Copy" for ALL modes -- PTT- prefix invariant never violated.
        // For Named mode the ATM template name is passed as the final 'atm' parameter of CreateOrder.
        // JS-001: catch logs and returns false -- no throw in dispatch path.
        // JS-002: mode=null treated as Inherit (no null return, no throw).
        // B23 T1: Dispatcher marshal added
        private bool SendCopy(Account follower, Instrument instrument, in CopySignal signal, FollowerAtmMode mode)
        {
            OrderType orderType  = signal.Type;
            double    limitPrice = signal.LimitPrice;
            string    signalName = "PTT-Copy";    // SCAN-05: PTT- prefix mandatory for ALL modes

            if (mode is FollowerAtmMode.Market)   // branch (1)
            {
                orderType  = OrderType.Market;
                limitPrice = 0;
            }
            // Inherit: use original signal values unchanged (no branch needed)

            string atmTemplate = mode is FollowerAtmMode.Named named // branch (2)
                ? named.TemplateName
                : null;

            try                                   // branch (3)
            {
                // NT8 AddOn constraint: 12-arg CreateOrder requires CustomOrder as arg12, not string.
                // Named ATM mode is not applicable from AddOn context -- pass null CustomOrder.
                // B23 T1 (DW-B22-NULLREF-01): NullRef on non-active-chart accounts caught here.
                // Dispatcher.InvokeAsync not reliably available in NT8 AddOn context (NT8-042).
                // Try/catch is the safe fallback: logs the error, returns false, does not crash.
                // B57 fix (DW-B57-01): capture order and call Submit() -- CreateOrder alone leaves
                // the order at Initialized state; Submit() sends it to the exchange (-> Working).
                var order = follower.CreateOrder(
                    instrument,
                    signal.Action,
                    orderType,
                    OrderEntry.Manual,
                    TimeInForce.Gtc,  // B29 fix: Day orders expire mid-session on overnight futures
                    signal.Quantity,
                    limitPrice,
                    0,
                    null,
                    signalName,
                    DateTime.MaxValue,
                    (NinjaTrader.Cbi.CustomOrder)null
                );
                if (order != null)
                    follower.Submit(new[] { order });
                return true;
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke("PTT-Copy error: " + ex.Message);
                return false;
            }
        }

        // B8 T2: GetAtmMode -- bounds-safe ATM mode retrieval. CYC=2.
        // Returns Inherit if accountName not found -- never null, never throws (JS-001, JS-002).
        private static FollowerAtmMode GetAtmMode(CopyRule rule, string accountName)
        {
            FollowerAtmMode mode;
            if (rule.FollowerAtmTemplates.TryGetValue(accountName ?? string.Empty, out mode)) // branch (1)
                return mode;
            return new FollowerAtmMode.Inherit();
        }

        // B50 -- ResolveAtmMode: CYC=2. Mode-aware ATM dispatch router.
        // Clone mode uses shared _cloneAtmCache; Signal/Mirror modes delegate to GetAtmMode (per-rule).
        // Replaces direct GetAtmMode call in DispatchCopy inner loop.
        // JS-002: never returns null -- all branches return a FollowerAtmMode subtype.
        private FollowerAtmMode ResolveAtmMode(CopyRule rule, string accountName)
        {
            if (GetCopyMode() == CopyMode.Clone)  // branch (1)
                return GetCloneAtmMode();
            return GetAtmMode(rule, accountName);
        }

        // B8 T2: ParseAtmModeName -- deserializes "Inherit"|"Market"|"Named:XXX" to FollowerAtmMode.
        // CYC=3. Returns Inherit for null/empty/unrecognized input -- never null, never throws.
        internal static FollowerAtmMode ParseAtmModeName(string name)
        {
            if (string.IsNullOrEmpty(name))           // branch (1)
                return new FollowerAtmMode.Inherit();
            if (name == "Market")                      // branch (2)
                return new FollowerAtmMode.Market();
            if (name.StartsWith("Named:"))             // branch (3)
                return new FollowerAtmMode.Named(name.Substring(6));
            return new FollowerAtmMode.Inherit();
        }

        // B8 T2: AtmModeToString -- serializes FollowerAtmMode to "Inherit"|"Market"|"Named:XXX".
        // CYC=3. Sealed hierarchy is exhaustive -- all variants covered.
        internal static string AtmModeToString(FollowerAtmMode mode)
        {
            if (mode is FollowerAtmMode.Market)             // branch (1)
                return "Market";
            if (mode is FollowerAtmMode.Named namedMode)    // branch (2)
                return "Named:" + namedMode.TemplateName;
            return "Inherit";                               // branch (3): Inherit or null-fallback
        }

        // B8 T2: SetAtmMode -- post-create mutation of a single follower's ATM mode.
        // ConcurrentBag rebuild pattern -- no lock (JS-021).
        // ImmutableDictionary.SetItem returns a NEW dictionary (immutable -- no mutation).
        internal void SetAtmMode(string instrument, string followerAccountName, FollowerAtmMode mode)
        {
            var snapshot = new List<CopyRule>(_rules);
            _rules = new ConcurrentBag<CopyRule>();
            foreach (var r in snapshot)
            {
                if (r.Instrument != instrument)
                {
                    _rules.Add(r);
                    continue;
                }
                var newMap = new Dictionary<string, FollowerAtmMode>(r.FollowerAtmTemplates);
                newMap[followerAccountName] = mode;
                _rules.Add(CopyRule.Create(r.Instrument, r.MasterAccount, r.FollowerAccounts, r.Enabled,
                    r.FollowerMultipliers, newMap, r.TightenTicks));
            }
        }

        internal void Trim(Instrument instrument)
        {
            foreach (var acc in AllAccounts(instrument))
                TrimOneAccount(acc, instrument);
        }

        internal void Flatten(Instrument instrument)
        {
            foreach (var acc in AllAccounts(instrument))
                FlattenOneAccount(acc, instrument);
        }

        // B28 T1 -- Trim(Account,Instrument): leader-account overload. Fixes DW-B28-02.
        // CYC=4: (1) leader null guard, (2) leader direct call, (3) foreach, (4) acc==leader skip.
        internal void Trim(Account leader, Instrument instrument)
        {
            if (leader == null)
            {
                StatusUpdate?.Invoke("PTT-Trim: leader null -- skipping");
                return;
            }
            TrimOneAccount(leader, instrument);
            foreach (var acc in AllAccounts(instrument))
            {
                if (acc == leader) continue;
                TrimOneAccount(acc, instrument);
            }
        }

        // B28 T1 -- Flatten(Account,Instrument): leader-account overload. Fixes DW-B28-02.
        // CYC=4: (1) leader null guard, (2) leader direct call, (3) foreach, (4) acc==leader skip.
        internal void Flatten(Account leader, Instrument instrument)
        {
            if (leader == null)
            {
                StatusUpdate?.Invoke("PTT-Flatten: leader null -- skipping");
                return;
            }
            FlattenOneAccount(leader, instrument);
            foreach (var acc in AllAccounts(instrument))
            {
                if (acc == leader) continue;
                FlattenOneAccount(acc, instrument);
            }
        }

        // B28 T1 -- CancelPendingEntries(Account,Instrument): leader-account overload. Fixes DW-B28-02.
        // CYC=4: (1) leader null guard, (2) leader direct call, (3) foreach, (4) acc==leader skip.
        internal void CancelPendingEntries(Account leader, Instrument instrument)
        {
            if (leader == null)
            {
                StatusUpdate?.Invoke("PTT-Cancel: leader null -- skipping");
                return;
            }
            CancelOneAccount(leader, instrument);
            foreach (var acc in AllAccounts(instrument))
            {
                if (acc == leader) continue;
                CancelOneAccount(acc, instrument);
            }
        }

        // B28 T1 -- Trim(Account,Instrument,int,double,double): leader-account limit overload.
        // CYC=5: (1) leader null guard, (2) ask/bid/buffer guard, (3) leader direct call, (4) foreach, (5) acc==leader skip.
        internal void Trim(Account leader, Instrument instrument, int exitBuffer, double ask, double bid)
        {
            if (leader == null)
            {
                StatusUpdate?.Invoke("PTT-TrimLimit: leader null -- skipping");
                return;
            }
            if (ask <= 0 || bid <= 0 || exitBuffer == 0) { Trim(leader, instrument); return; }
            TrimOneAccountLimit(leader, instrument, exitBuffer, ask, bid);
            foreach (var acc in AllAccounts(instrument))
            {
                if (acc == leader) continue;
                TrimOneAccountLimit(acc, instrument, exitBuffer, ask, bid);
            }
        }

        // B28 T1 -- Flatten(Account,Instrument,int,double,double): leader-account limit overload.
        // CYC=5: (1) leader null guard, (2) ask/bid/buffer guard, (3) leader direct call, (4) foreach, (5) acc==leader skip.
        internal void Flatten(Account leader, Instrument instrument, int exitBuffer, double ask, double bid)
        {
            if (leader == null)
            {
                StatusUpdate?.Invoke("PTT-FlattenLimit: leader null -- skipping");
                return;
            }
            if (ask <= 0 || bid <= 0 || exitBuffer == 0) { Flatten(leader, instrument); return; }
            FlattenOneAccountLimit(leader, instrument, exitBuffer, ask, bid);
            foreach (var acc in AllAccounts(instrument))
            {
                if (acc == leader) continue;
                FlattenOneAccountLimit(acc, instrument, exitBuffer, ask, bid);
            }
        }

        // B28 T1 -- TrimOneAccount: per-account market trim helper. CYC=3.
        // (1) pos null/qty guard, (2) action ternary, (3) try/catch CreateOrder.
        // JS-001: no rethrow. JS-021: no lock. ASCII: PTT-Trim signal name.
        private void TrimOneAccount(Account acc, Instrument instrument)
        {
            var pos = FindPosition(acc, instrument);
            if (pos == null || pos.Quantity == 0)
            {
                StatusUpdate?.Invoke(acc.Name + ": flat skip");
                return;
            }
            int trimQty = (int)Math.Ceiling(pos.Quantity / 2.0);
            var action = pos.MarketPosition == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;
            try
            {
                acc.CreateOrder(
                    instrument, action, OrderType.Market, OrderEntry.Manual,
                    TimeInForce.Gtc, trimQty, 0, 0, null, "PTT-Trim",
                    DateTime.MaxValue, null);
                StatusUpdate?.Invoke(acc.Name + ": trim " + trimQty);
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke("PTT-Trim error: " + ex.Message);
            }
        }

        // B28 T1 -- FlattenOneAccount: per-account market flatten helper. CYC=3.
        // (1) pos null/qty guard, (2) action ternary, (3) try/catch CreateOrder.
        private void FlattenOneAccount(Account acc, Instrument instrument)
        {
            var pos = FindPosition(acc, instrument);
            if (pos == null || pos.Quantity == 0)
            {
                StatusUpdate?.Invoke(acc.Name + ": flat skip");
                return;
            }
            var action = pos.MarketPosition == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;
            try
            {
                acc.CreateOrder(
                    instrument, action, OrderType.Market, OrderEntry.Manual,
                    TimeInForce.Gtc, pos.Quantity, 0, 0, null, "PTT-Flatten",
                    DateTime.MaxValue, null);
                StatusUpdate?.Invoke(acc.Name + ": flatten " + pos.Quantity);
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke("PTT-Flatten error: " + ex.Message);
            }
        }

        // B29 fix -- ComputeLimitPx: aggressive exit anchor.
        // Long exits (Sell Limit) post at bid - buffer (at/below market → fills immediately).
        // Short exits (BuyToCover) post at ask + buffer (at/above market → fills immediately).
        // DW-B29-01: original used ask+buffer for long, placing passive limit ABOVE market (never filled).
        // CYC=1: single ternary. No NT8 deps, no state, no nulls.
        // internal static -- CopyEngineTests.cs calls CopyEngine.ComputeLimitPx(...) directly.
        internal static double ComputeLimitPx(bool isLong, double ask, double bid, int exitBuffer, double tickSize)
            => isLong
                ? bid - exitBuffer * tickSize
                : ask + exitBuffer * tickSize;

        // B19 T1 -- Trim 4-arg: exit half position at limit price anchored to ask (long) or bid (short).
        // Long: Sell Limit @ ask + exitBuffer*tick.   Short: BuyToCover @ bid - exitBuffer*tick.
        // NT8-007: arg 12 = (NinjaTrader.Cbi.CustomOrder)null.
        // NT8-014: signal name = "PTT-TrimLimit".
        // NT8-032: ask/bid are MarketDataEventArgs.Price doubles (callers obtain via GetAsk()/GetBid()).
        // CYC=6: (1+2) compound ask/bid guard, (3) exitBuffer guard, (4) foreach, (5+6) pos null||qty guard.
        // JS-001: try/catch wraps acc.CreateOrder -- no rethrow.
        internal void Trim(Instrument instrument, int exitBuffer, double ask, double bid)
        {
            if (ask <= 0 || bid <= 0 || exitBuffer == 0) { Trim(instrument); return; }
            foreach (var acc in AllAccounts(instrument))
                TrimOneAccountLimit(acc, instrument, exitBuffer, ask, bid);
        }

        // B19 T1 -- Flatten 4-arg: exit full position at limit price anchored to ask (long) or bid (short).
        // NT8-007: arg 12 = (NinjaTrader.Cbi.CustomOrder)null.
        // NT8-014: signal name = "PTT-FlattenLimit".
        internal void Flatten(Instrument instrument, int exitBuffer, double ask, double bid)
        {
            if (ask <= 0 || bid <= 0 || exitBuffer == 0) { Flatten(instrument); return; }
            foreach (var acc in AllAccounts(instrument))
                FlattenOneAccountLimit(acc, instrument, exitBuffer, ask, bid);
        }

        internal void CancelPendingEntries(Instrument instrument)
        {
            foreach (var acc in AllAccounts(instrument))
                CancelOneAccount(acc, instrument);
        }

        // B28 T1 -- CancelOneAccount: per-account pending cancel helper. CYC=4.
        // (1) foreach orders, (2) instrument filter, (3) OrderState guard, (4) IsBracketLeg guard.
        // Preserves B18 T3 fix: also cancels Initialized orders (DW-B18-CANCEL-01).
        private void CancelOneAccount(Account acc, Instrument instrument)
        {
            foreach (var order in acc.Orders.ToList())
            {
                if (order.Instrument != instrument) continue;
                // B18 T3: DW-B18-CANCEL-01 -- also cancel Initialized orders.
                if (order.OrderState != OrderState.Working &&
                    order.OrderState != OrderState.Initialized)
                    continue;
                if (IsBracketLeg(order)) continue;
                try
                {
                    acc.Cancel(new Order[] { order });
                    StatusUpdate?.Invoke(acc.Name + ": entry pulled " + order.OrderId);
                }
                catch (Exception ex)
                {
                    StatusUpdate?.Invoke("PTT-Cancel error: " + ex.Message);
                }
            }
        }

        // HOTFIX-F4 -- CancelStaleExitOrders: cancels any working PTT limit exit orders by signal name.
        // Called by TrimOneAccountLimit and FlattenOneAccountLimit before posting a new limit.
        // Prevents stale PTT-TrimLimit/PTT-FlattenLimit orders competing with ATM Close or
        // a second button click, which caused "Close operation timed out" popup in NT8.
        // CYC=3: foreach(1), name filter(2), try/catch(3). JS-021: ToList() snapshot.
        private void CancelStaleExitOrders(Account acc, Instrument instrument, string signalName)
        {
            foreach (var order in acc.Orders.ToList())
            {
                if (order.Instrument != instrument) continue;                        // (1)
                if (order.Name != signalName) continue;                              // (2)
                if (order.OrderState != OrderState.Working &&
                    order.OrderState != OrderState.Initialized) continue;
                try                                                                  // (3)
                {
                    acc.Cancel(new Order[] { order });
                    StatusUpdate?.Invoke(acc.Name + ": stale exit pulled " + order.Name);
                }
                catch (Exception ex)
                {
                    StatusUpdate?.Invoke("PTT-CancelStale error: " + ex.Message);
                }
            }
        }



        // B28 T1 -- TrimOneAccountLimit: per-account limit trim helper. CYC=3.
        // (1) pos null/qty guard, (2) isLong ternary, (3) try/catch CreateOrder.
        // NT8-007: arg12 = (NinjaTrader.Cbi.CustomOrder)null.
        // HOTFIX-F4: cancel any stale PTT-TrimLimit orders before posting new one.
        // Stale limits from a prior click stay live on the book and compete with ATM Close.
        private void TrimOneAccountLimit(Account acc, Instrument instrument,
            int exitBuffer, double ask, double bid)
        {
            CancelStaleExitOrders(acc, instrument, "PTT-TrimLimit");    // HOTFIX-F4
            var pos = FindPosition(acc, instrument);
            if (pos == null || pos.Quantity == 0)
            {
                StatusUpdate?.Invoke(acc.Name + ": flat skip");
                return;
            }
            int trimQty = (int)Math.Ceiling(pos.Quantity / 2.0);
            bool isLong = pos.MarketPosition == MarketPosition.Long;
            var action  = isLong ? OrderAction.Sell : OrderAction.BuyToCover;
            double tickSize = instrument.MasterInstrument.TickSize;
            double limitPx  = ComputeLimitPx(isLong, ask, bid, exitBuffer, tickSize);
            try
            {
                acc.CreateOrder(
                    instrument, action, OrderType.Limit, OrderEntry.Manual,
                    TimeInForce.Gtc, trimQty, limitPx, 0, null, "PTT-TrimLimit",
                    DateTime.MaxValue, (NinjaTrader.Cbi.CustomOrder)null);
                StatusUpdate?.Invoke(acc.Name + ": trim-limit " + trimQty + " @ " + limitPx);
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke("PTT-TrimLimit error: " + ex.Message);
            }
        }

        // B28 T1 -- FlattenOneAccountLimit: per-account limit flatten helper. CYC=3.
        // (1) pos null/qty guard, (2) isLong ternary, (3) try/catch CreateOrder.
        // NT8-007: arg12 = (NinjaTrader.Cbi.CustomOrder)null.
        // HOTFIX-F4: cancel any stale PTT-FlattenLimit orders before posting new one.
        // Stale limits from a prior click stay live on the book and compete with ATM Close.
        private void FlattenOneAccountLimit(Account acc, Instrument instrument,
            int exitBuffer, double ask, double bid)
        {
            CancelStaleExitOrders(acc, instrument, "PTT-FlattenLimit"); // HOTFIX-F4
            var pos = FindPosition(acc, instrument);
            if (pos == null || pos.Quantity == 0)
            {
                StatusUpdate?.Invoke(acc.Name + ": flat skip");
                return;
            }
            bool isLong = pos.MarketPosition == MarketPosition.Long;
            var action  = isLong ? OrderAction.Sell : OrderAction.BuyToCover;
            double tickSize = instrument.MasterInstrument.TickSize;
            double limitPx  = ComputeLimitPx(isLong, ask, bid, exitBuffer, tickSize);
            try
            {
                acc.CreateOrder(
                    instrument, action, OrderType.Limit, OrderEntry.Manual,
                    TimeInForce.Gtc, pos.Quantity, limitPx, 0, null, "PTT-FlattenLimit",
                    DateTime.MaxValue, (NinjaTrader.Cbi.CustomOrder)null);
                StatusUpdate?.Invoke(acc.Name + ": flatten-limit " + pos.Quantity + " @ " + limitPx);
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke("PTT-FlattenLimit error: " + ex.Message);
            }
        }

        // B62: price-keyed dedup. Stores LimitPrice (double) instead of timestamp (long).
        // First call for orderId: TryAdd succeeds -> not a dup -> dispatch.
        // Repeat call same orderId: TryAdd fails -> true dup -> skip.
        // Drag detection is handled by Gate C BEFORE this is called -- drag events never reach IsDedup.
        // Eviction is handled by EvictDedup on terminal states (Filled/Cancelled/Rejected).
        // CYC=2: TryAdd false-path (1) + early return.
        // JS-025: ConcurrentDictionary.TryAdd is lock-free.
        private bool IsDedup(string orderId, double limitPrice)
        {
            if (!_dedupCache.TryAdd(orderId, limitPrice))
                return true;

            return false;
        }

        // B62: evict dedup entry when order reaches terminal state (Filled/Cancelled/Rejected).
        // Called unconditionally from OnOrderUpdate pre-gate, after TryFirePositionState.
        // Ensures evicted orderId can be re-used for the next fresh order on the same instrument.
        // CYC=2: terminal-state guard (1) + TryRemove (no branch).
        // JS-025: ConcurrentDictionary.TryRemove is lock-free.
        internal void EvictDedup(string orderId, OrderState state)
        {
            if (state != OrderState.Filled && state != OrderState.Cancelled && state != OrderState.Rejected)
                return;

            _dedupCache.TryRemove(orderId, out _);
        }

        private IEnumerable<Account> AllAccounts(Instrument instrument)
        {
            var rule = FindRule(instrument);
            if (rule == null)
                yield break;

            yield return rule.Value.MasterAccount;
            foreach (var acc in rule.Value.FollowerAccounts)
            {
                if (acc != null)
                    yield return acc;
            }
        }

        /// <summary>
        /// Finds the copy rule for the given instrument.
        /// </summary>
        /// <returns>
        /// Matching <see cref="CopyRule"/>, or <c>null</c> if no rule exists for this instrument.
        /// Callers MUST null-check the return value.
        /// </returns>
        private CopyRule? FindRule(Instrument instrument)
        {
            if (instrument == null)
                return null; // Change 8: null guard
            foreach (var rule in _rules)
            {
                if (rule.Instrument == instrument.FullName)
                    return rule;
            }
            return null;
        }

        // Change 6: Replace PassesDailyCapCheck stub with real P&L check
        private bool PassesDailyCapCheck(Account acc)
        {
            double pnl = acc.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
            if (pnl == double.MinValue)
                return true;
            return pnl > _dailyCapFloor;
        }

        private static bool IsFlat(NinjaTrader.Cbi.Position pos)
        {
            return pos == null || pos.Quantity == 0;
        }

        // B25 T1 -- DW-B25-01: ATM bracket stops use name format "12s Buy STP".
        // FromEntrySignal is null for ATM orders. No "Stop" prefix. STP suffix is the only discriminator.
        // CYC: 2 + 1 (STP clause) = 3. OrdinalIgnoreCase: consistent with WireLeaderAccount (B24 Lane A).
        private static bool IsStopLeg(Order order)
        {
            return order.FromEntrySignal != null
                || (order.Name != null && order.Name.StartsWith("Stop"))
                || (order.Name != null && order.Name.EndsWith("STP", StringComparison.OrdinalIgnoreCase));
        }

        // Static version of IsBracketLeg for use in static method IsWorkingBracket
        private static bool IsBracketLegStatic(Order order)
        {
            return order.FromEntrySignal != null
                || (
                    order.Name != null
                    && (
                        order.Name.StartsWith("Stop")
                        || order.Name.StartsWith("Target")
                        || order.Name.StartsWith("PTT-")
                    )
                );
        }

        // B29 fix: removed "PTT-" from IsBracketLeg.
        // IsBracketLeg is used by CancelOneAccount to skip bracket stops/targets.
        // PTT- exit orders (PTT-Trim, PTT-Flatten, PTT-BE-Stop, PTT-Tighten-Stop) are NOT brackets --
        // they should be cancelable by the Cancel button.
        // Copy-cascade prevention for PTT- orders is handled separately by Gate 0.5 in DispatchCopy.
        private bool IsBracketLeg(Order order)
        {
            return order.FromEntrySignal != null
                || (
                    order.Name != null
                    && (
                        order.Name.StartsWith("Stop")
                        || order.Name.StartsWith("Target")
                    )
                );
        }

        private Position FindPosition(Account acc, Instrument instrument)
        {
            foreach (Position p in acc.Positions)
                if (p.Instrument == instrument) return p;
            return null;
        }

        // B58 -- FindPositionPublic: thin wrapper over private FindPosition for panel access.
        // CYC=1. Returns null if no position (pre-existing FindPosition behavior -- not new).
        // JS-002: null return is pre-existing contract of FindPosition, not a new null-return site.
        internal Position FindPositionPublic(Account acc, Instrument instrument)
            => FindPosition(acc, instrument);

        // B58 -- SnapshotTargetsPublic: collects Working orders with PTT-QX-T or PTT-TGT- prefix.
        // CYC=3 (1 base + foreach + prefix check). Returns List<Order> -- panel uses .Count.
        // JS-002: never returns null -- returns empty List if no matches.
        // JS-021: acc.Orders iteration; no lock required (NT8 AddOn read-only enumeration).
        internal List<Order> SnapshotTargetsPublic(Account acc, Instrument instr)
        {
            var result = new List<Order>();
            if (acc == null || instr == null) return result;             // (1) null guard
            foreach (Order o in acc.Orders)                              // (2) foreach
            {
                if (o.Instrument != instr) continue;
                if (o.OrderState != OrderState.Working) continue;
                string n = o.Name ?? string.Empty;
                if (n.StartsWith("PTT-QX-T", StringComparison.Ordinal)  // (3) prefix check
                 || n.StartsWith("PTT-TGT-", StringComparison.Ordinal))
                    result.Add(o);
            }
            return result;
        }

        // B31 -- MoveStopToBreakEven: order.StopPrice + acc.Change(new Order[]{order}) in-place.
        // B31 CONFIRMED: order-level Change() preserves ATM OCO link (Director live test 2026-07-17).
        // CYC=6: IsFlat(1), tickSize guard(2), foreach(3), working(4), stop type(5), isStopLeg(6).
        // JS-001: try/catch around acc.Change() -- no throw in hot path.
        private void MoveStopToBreakEven(Account acc, Instrument instrument, int bufferTicks)
        {
            var pos = FindPosition(acc, instrument);
            if (IsFlat(pos))                                                               // (1)
            {
                StatusUpdate?.Invoke(acc.Name + ": flat skip");
                return;
            }
            double tickSize = instrument.MasterInstrument.TickSize;
            bool isLong = pos.MarketPosition == MarketPosition.Long;
            double direction = isLong ? 1.0 : -1.0;
            double raw = pos.AveragePrice + direction * bufferTicks * tickSize;
            double newStop = Math.Round(raw / tickSize) * tickSize;
            foreach (var order in acc.Orders.ToList())                                     // (3)
            {
                if (order.Instrument != instrument)                                        // (2) -- instrument filter
                    continue;
                if (order.OrderState != OrderState.Working)                                // (4)
                    continue;
                // DW-B25-01: accept StopLimit (ATM bracket) as well as StopMarket (direct).
                // Precedent: TightenStop L1234-1235 uses this exact two-type pattern.
                // acc.Change() on StopLimit is safe -- NT8 recalculates LimitPrice from original offset.
                if (order.OrderType != OrderType.StopMarket &&                             // (5)
                    order.OrderType != OrderType.StopLimit)
                    continue;
                if (!IsStopLeg(order))                                                     // (6)
                    continue;
                // B10 T1: idempotency guard -- skip if stop is already at or past BE level
                if (IsStopAlreadyAtBe(order, newStop, isLong))
                    continue;
                // B31: in-place move -- same pattern as SyncFollowerBracket (L621-624).
                // NT8-046: property-set + single-array acc.Change() works on ATM-owned stops.
                StatusUpdate?.Invoke(acc.Name + ": BE moving stop -> " + newStop);
                try
                {
                    order.StopPrice = newStop;
                    acc.Change(new Order[] { order });
                    StatusUpdate?.Invoke(acc.Name + ": BE stop moved @ " + newStop);
                }
                catch (Exception ex)
                {
                    StatusUpdate?.Invoke(acc.Name + ": BE Change() failed -- " + ex.Message);
                }
            }
        }

        internal void BreakEven(Instrument instrument, int bufferTicks)
        {
            foreach (var acc in AllAccounts(instrument))
                MoveStopToBreakEven(acc, instrument, bufferTicks);
        }

        // B24 T1 -- BreakEven(Account,Instrument,int): fires leader directly, no rule needed.
        // CYC=4: null guard(1), MoveStop leader(no branch), foreach acc(2), acc==leader skip(3).
        // JS-021: no lock. JS-002: null leader fires StatusUpdate + early return.
        internal void BreakEven(Account leader, Instrument instrument, int bufferTicks)
        {
            if (leader == null)                                      // (1) null guard
            {
                StatusUpdate?.Invoke("PTT-BE: leader null -- BE skipped");
                return;
            }
            MoveStopToBreakEven(leader, instrument, bufferTicks);   // leader direct, no rule needed
            foreach (var acc in AllAccounts(instrument))            // (2) follower fan-out
            {
                if (acc == leader) continue;                        // (3) skip duplicate
                MoveStopToBreakEven(acc, instrument, bufferTicks);
            }
        }



        // B10 T3 -- TightenStop: moves all working stops on follower accounts to currentPrice +/- N ticks.
        // JS-001: try/catch inside TightenOneStop -- no throw in hot path.
        // JS-021: no lock -- AllAccounts iterates ConcurrentBag (lock-free).
        // DW-B9-GAP-001c: T3 production implementation.
        // B30: body delegated to TightenOneAccountStops (DW-B30-02, DW-B30-04).
        // CYC=2: (1) rule null guard, (2) foreach.
        internal void TightenStop(Instrument instrument, int ticks)
        {
            var rule = FindRule(instrument);
            if (rule == null)                                                               // (1)
                return;
            foreach (var acc in AllAccounts(instrument))                                   // (2)
                TightenOneAccountStops(acc, instrument, ticks);
        }

        // B10 T3 -- TightenOneStop: applies tighten to a single stop order.
        // B31: in-place price move via order.StopPrice + acc.Change(new Order[]{order}).
        // CYC=2: null guard(1), alreadyTighter(2). tightenAction ternary removed.
        private void TightenOneStop(Account acc, Instrument instr,
            Order order, double targetPrice, double tickSize)
        {
            if (order == null)                                                          // (1)
                return;
            bool isLong = order.OrderAction == OrderAction.Sell;  // stop-sell = long pos
            bool alreadyTighter = isLong
                ? order.StopPrice >= targetPrice
                : order.StopPrice <= targetPrice;
            if (alreadyTighter)                                                         // (2)
                return;
            // B31 NT8-046: property-set + single-array acc.Change() -- preserves ATM OCO.
            try
            {
                order.StopPrice = targetPrice;
                acc.Change(new Order[] { order });
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke(acc.Name + ": Tighten Change() failed -- " + ex.Message);
            }
        }

        // B30 -- ShouldTightenOrder: order-filter predicate for TightenOneAccountStops.
        // CYC=4: (1) Working check, (2) StopMarket||StopLimit, (3) instrument match, (4) IsStopLeg.
        // JS-021: no lock. JS-001: no throw.
        private static bool ShouldTightenOrder(Order order, Instrument instrument)
        {
            if (order.OrderState != OrderState.Working)
                return false;                                                               // (1)
            if (order.OrderType != OrderType.StopMarket &&
                order.OrderType != OrderType.StopLimit)
                return false;                                                               // (2)
            if (order.Instrument != instrument)
                return false;                                                               // (3)
            if (!IsStopLeg(order))
                return false;                                                               // (4)
            return true;
        }

        // B30 -- GetRefPrice: resolves bid/ask reference price for tighten-stop calculation.
        // CYC=4: (1) bid>0 &&, (2) ask>0, (3) outer ?:, (4) inner isLong ?:.
        // DW-B30-04: NT8 null-conditional (?.) prevents NullReferenceException when MarketData unsubscribed.
        private static double GetRefPrice(Instrument instrument, bool isLong)
        {
            double bid = instrument.MarketData?.Bid?.Price ?? 0.0;
            double ask = instrument.MarketData?.Ask?.Price ?? 0.0;
            return bid > 0 && ask > 0                                                      // (1)(2)
                ? (isLong ? ask : bid)                                                     // (3)(4)
                : 0.0;
        }

        // B30 -- TightenOneAccountStops: per-account stop-tighten helper. DW-B30-02.
        // CYC=5: (1) IsFlat guard, (2) refPrice==0 guard, (3) isLong ternary (target dir), (4) foreach, (5) !ShouldTightenOrder.
        // JS-021: no lock -- ToList() snapshot prevents iterator invalidation.
        // JS-002: no return null -- log "PTT-Tighten: no market data" on zero price.
        private void TightenOneAccountStops(Account acc, Instrument instrument, int tightenTicks)
        {
            var pos = FindPosition(acc, instrument);
            if (IsFlat(pos))                                                               // (1)
                return;
            bool isLong = pos.MarketPosition == MarketPosition.Long;
            double tickSize = instrument.MasterInstrument.TickSize;
            double refPrice = GetRefPrice(instrument, isLong);
            if (refPrice == 0.0)                                                           // (2)
            {
                StatusUpdate?.Invoke("PTT-Tighten: no market data -- " + acc.Name);
                return;
            }
            double targetPrice = isLong                                                    // (3)
                ? refPrice - tightenTicks * tickSize
                : refPrice + tightenTicks * tickSize;
            foreach (var order in acc.Orders.ToList())                                     // (4)
            {
                if (!ShouldTightenOrder(order, instrument))                                // (5)
                    continue;
                TightenOneStop(acc, instrument, order, targetPrice, tickSize);
            }
        }

        // B30 -- TightenStop(Account,Instrument,int): leader-direct overload. Fixes DW-B30-02.
        // CYC=4: (1) leader null guard, (2) leader direct call, (3) foreach, (4) acc==leader skip.
        // Pattern: identical to Trim(Account,Instrument) / Flatten(Account,Instrument) from B28.
        // JS-021: no lock. JS-002: no return null -- StatusUpdate log on null leader.
        internal void TightenStop(Account leader, Instrument instrument, int tightenTicks)
        {
            if (leader == null)                                                            // (1)
            {
                StatusUpdate?.Invoke("PTT-Tighten: leader null -- skipping");
                return;
            }
            TightenOneAccountStops(leader, instrument, tightenTicks);                     // (2)
            foreach (var acc in AllAccounts(instrument))                                   // (3)
            {
                if (acc == leader) continue;                                               // (4)
                TightenOneAccountStops(acc, instrument, tightenTicks);
            }
        }

        // B27 -- ArmPendingBe: arms the pending BE watcher using acc.AccountItemUpdate.
        // CYC=4: instr null(1), acc null+emit(2), pos flat+emit(3), slot upsert(4).
        // DW-B30-05: StatusUpdate on null-leader and flat-position paths (previously silent).
        // DW-B27-01: slot dict replaces four singleton fields -- per-account, no data races.
        // JS-021: no lock -- ConcurrentDictionary indexer write is lock-free.
        internal void ArmPendingBe(Instrument instr, Account masterAcc, int bufferTicks)
        {
            if (instr == null)                                  // (1)
                return;
            if (masterAcc == null)                              // (2)
            {
                StatusUpdate?.Invoke("PTT-BE: leader null -- skipped");
                return;
            }
            var pos = FindPosition(masterAcc, instr);
            if (IsFlat(pos))                                    // (3)
            {
                StatusUpdate?.Invoke("PTT-BE: no open position for " + masterAcc.Name);
                return;
            }
            _pendingBeSlots[masterAcc.Name] = new PendingBeSlot(masterAcc, instr, bufferTicks); // (4)
            masterAcc.AccountItemUpdate += OnPendingBeAccountUpdate;
        }

        // B27 -- DisarmPendingBe: disarms the pending BE watcher atomically.
        // CYC=3: leader null guard(1), TryRemove check(2), acc null guard(3).
        // DW-B27-01: reads Account from slot -- no stale singleton reference.
        // JS-021: no lock -- ConcurrentDictionary.TryRemove is atomic.
        // NT8-043: explicit if (acc != null) guard -- no ?.Event -= pattern.
        internal void DisarmPendingBe(Account leader)
        {
            if (leader == null)                                                       // (1)
            {
                StatusUpdate?.Invoke("DisarmPendingBe: leader null -- no-op");
                return;
            }
            if (!_pendingBeSlots.TryRemove(leader.Name, out var slot))               // (2)
                return;
            if (slot.Account != null)                                                 // (3)
                slot.Account.AccountItemUpdate -= OnPendingBeAccountUpdate;
        }

        // B40 -- IsPendingSlotsEmpty: CYC=1. Lock-free read of ConcurrentDictionary.IsEmpty.
        // Called by TradeCopierPanel BE ALL armed/wait flow to determine gate state.
        // JS-021: ConcurrentDictionary.IsEmpty is lock-free.
        internal bool IsPendingSlotsEmpty() => _pendingBeSlots.IsEmpty;

        // B27 -- ArmTrailBe: arms the continuous trail watcher using acc.AccountItemUpdate.
        // CYC=4: instr null(1), acc null(2), pos flat(3), slot upsert(4).
        // DW-B27-01: slot dicts replace five singleton fields -- per-account, no data races.
        // JS-021: no lock -- ConcurrentDictionary indexer writes are lock-free.
        // NT8-003: BitConverter bits in ConcurrentDictionary<string,long>; AddOrUpdate provides barrier.
        internal void ArmTrailBe(Instrument instr, Account masterAcc, int bufferTicks)
        {
            if (instr == null)                                    // (1)
                return;
            if (masterAcc == null)                                // (2)
                return;
            var pos = FindPosition(masterAcc, instr);
            if (IsFlat(pos))                                      // (3)
                return;
            double currentPnl = masterAcc.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar);
            if (currentPnl == double.MinValue) currentPnl = 0.0;
            long pnlBits = BitConverter.DoubleToInt64Bits(currentPnl);
            _trailBeSlots[masterAcc.Name]       = new TrailBeSlot(masterAcc, instr, bufferTicks); // (4)
            _trailBeLastPnlBits[masterAcc.Name] = pnlBits;
            masterAcc.AccountItemUpdate += OnTrailBeAccountUpdate;
        }

        // B27 -- DisarmTrailBe: disarms the trail watcher atomically.
        // CYC=3: leader null guard(1), TryRemove check(2), acc null guard(3).
        // DW-B27-01: reads Account from slot -- no stale singleton reference.
        // JS-021: no lock -- ConcurrentDictionary.TryRemove is atomic.
        // NT8-043: explicit if (acc != null) guard -- no ?.Event -= pattern.
        // Idempotent: safe to call when already Off or with null leader.
        internal void DisarmTrailBe(Account leader)
        {
            if (leader == null)                                                       // (1)
            {
                StatusUpdate?.Invoke("DisarmTrailBe: leader null -- no-op");
                return;
            }
            if (!_trailBeSlots.TryRemove(leader.Name, out var slot))                 // (2)
                return;
            if (slot.Account != null)                                                 // (3)
                slot.Account.AccountItemUpdate -= OnTrailBeAccountUpdate;
            _trailBeLastPnlBits.TryRemove(leader.Name, out _);
        }

        // B27 -- OnTrailBeAccountUpdate: continuous AccountItemUpdate callback for auto-trail.
        // Fires on NT8 account background thread -- NO UI calls inside this method.
        // CYC=6: item filter(1), armed check(2), pnl improvement(3), CAS win(4), slot update+BreakEven(5).
        // JS-021: no lock -- AddOrUpdate is lock-free CAS.
        // NT8-003: ConcurrentDictionary AddOrUpdate provides CAS barrier (long bits, no forbidden keyword).
        // JS-001: BreakEven internally wraps acc.Change() in try/catch; no rethrow here.
        // STAYS SUBSCRIBED until DisarmTrailBe() is called -- unlike OnPendingBeAccountUpdate (one-shot).
        private void OnTrailBeAccountUpdate(object sender, NinjaTrader.Cbi.AccountItemEventArgs e)
        {
            if (e.AccountItem != AccountItem.UnrealizedProfitLoss)                          // (1)
                return;
            string accName = (sender as NinjaTrader.Cbi.Account)?.Name ?? string.Empty;
            if (!_trailBeSlots.TryGetValue(accName, out var slot))                          // (2)
                return;
            double newPnl = e.Value;
            if (!_trailBeLastPnlBits.TryGetValue(accName, out long oldBits))                // (3a)
                return;
            double oldPnl = BitConverter.Int64BitsToDouble(oldBits);
            if (newPnl <= oldPnl)                                                            // (3b)
                return;
            long newBits = BitConverter.DoubleToInt64Bits(newPnl);
            long actual  = _trailBeLastPnlBits.AddOrUpdate(                                 // (4)
                accName, newBits, (k, cur) => cur < newBits ? newBits : cur);
            if (actual != newBits)                                                           // lost race
                return;
            _trailBeSlots.AddOrUpdate(                                                       // (5)
                accName,
                new TrailBeSlot(slot.Account, slot.Instrument, slot.BufferTicks + 1),
                (k, old) => new TrailBeSlot(old.Account, old.Instrument, old.BufferTicks + 1));
            BreakEven(slot.Account, slot.Instrument, slot.BufferTicks + 1);
        }

        // B27 -- OnPendingBeAccountUpdate: price-based trigger for pending BE (one-shot).
        // Fires on NT8 account background thread -- NO UI calls inside this method.
        // CYC=8: item filter(1), armed+slot(2), pos flat(3), tickSize(4), last<=0(5), triggered(6), CAS claim(7).
        // JS-021: no lock -- TryGetValue/TryRemove are lock-free.
        // NT8-003: no volatile. B23 T1 (DW-B22-BE-TRIGGER-01): price-based, immune to commission fees.
        // sender is the NT8 Account object in AccountItemUpdate callbacks.
        private void OnPendingBeAccountUpdate(object sender, NinjaTrader.Cbi.AccountItemEventArgs e)
        {
            if (e.AccountItem != AccountItem.UnrealizedProfitLoss)                          // (1)
                return;
            string accName = (sender as NinjaTrader.Cbi.Account)?.Name ?? string.Empty;
            if (!_pendingBeSlots.TryGetValue(accName, out var slot))                        // (2)
                return;
            var acc   = slot.Account;
            var instr = slot.Instrument;
            var buf   = slot.BufferTicks;
            var pos   = FindPosition(acc, instr);
            if (IsFlat(pos))                                                                 // (3)
                return;
            double tickSize = instr?.MasterInstrument?.TickSize ?? 0.0;
            if (tickSize <= 0.0)                                                             // (4)
                return;
            // HOTFIX-F2: Last.Price is 0 on Sim accounts and stale on reconnect.
            // Use Bid for long (price must reach entry from below) and Ask for short.
            // Falls back to Ask/Bid respectively if primary is 0 -- never blocks on 0.
            bool isLong   = pos.MarketPosition == MarketPosition.Long;
            double refBid = instr?.MarketData?.Bid?.Price ?? 0.0;
            double refAsk = instr?.MarketData?.Ask?.Price ?? 0.0;
            double refPx  = isLong
                ? (refBid > 0 ? refBid : refAsk)   // long: use bid; fallback ask
                : (refAsk > 0 ? refAsk : refBid);   // short: use ask; fallback bid
            if (refPx <= 0.0)                                                                // (5)
                return;
            double target = pos.AveragePrice + (isLong ? 1.0 : -1.0) * buf * tickSize;
            bool triggered = isLong ? (refPx >= target) : (refPx <= target);
            if (!triggered)                                                                  // (6)
                return;
            if (!_pendingBeSlots.TryRemove(accName, out var removed))                       // (7) atomic claim
                return;
            if (removed.Account != null)
                removed.Account.AccountItemUpdate -= OnPendingBeAccountUpdate;
            BreakEven(removed.Account, removed.Instrument, removed.BufferTicks);
            PendingBeFired?.Invoke(removed.Instrument?.FullName ?? string.Empty,
                                   removed.Account?.Name ?? string.Empty);
        }

        // -- B6: Persistence field -------------------------------------------

        private volatile bool _persistenceLoaded = false;

        // -- B6/B8: Serialization DTO classes -----------------------------------

        [Serializable]
        private sealed class CopyRuleDto
        {
            public string InstrumentName { get; set; } = string.Empty;
            public string MasterAccountName { get; set; } = string.Empty;
            public string[] FollowerAccountNames { get; set; } = new string[0];
            public bool IsEnabled { get; set; } = true;
            // B8 T1: per-follower quantity multipliers (parallel to FollowerAccountNames[])
            // Default empty = all followers 1x. Backward compat: null on B6/B7 XML = treat as all-1s.
            public int[] FollowerMultipliers { get; set; } = new int[0];
            // B8 T2 (pre-declared here for single DTO edit pass per plan S3.1):
            // FollowerAtmModeNames serializes "Inherit"|"Market"|"Named:XXX" per follower.
            public string[] FollowerAtmModeNames { get; set; } = new string[0];
            // B10 T3: TightenTicks serialization -- default 0 here (DtoToRule converts 0 to 5).
            // Backward compat: old XML without this element -> XmlSerializer sets to 0 -> DtoToRule maps to 5.
            public int TightenTicks { get; set; } = 0;
        }

        [Serializable]
        private sealed class CopyRulesContainer
        {
            public List<CopyRuleDto> Rules { get; set; } = new List<CopyRuleDto>();
            // B54 -- persists copy-enabled state so F5 cycle restores button color correctly.
            // NT8-001: { get; set; } (not init accessor). XmlSerializer requires public { set; }.
            public bool CopyEnabled { get; set; } = false;
        }

        // -- B6: Path helper (CYC=1) -----------------------------------------

        private static string GetPersistencePath(string overridePath = null)
        {
            return overridePath
                ?? Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "PropTraderTools", "copy_rules.xml");
        }

        // -- B6/B8: Conversion helpers -----------------------------------------

        // B8 T1: RuleToDto -- emits FollowerMultipliers and FollowerAtmModeNames arrays
        // B10 T3: also emits TightenTicks
        private static CopyRuleDto RuleToDto(CopyRule rule)
        {
            var followerNames = new string[rule.FollowerAccounts.Length];
            for (int i = 0; i < rule.FollowerAccounts.Length; i++)
                followerNames[i] = rule.FollowerAccounts[i] != null ? rule.FollowerAccounts[i].Name : string.Empty;

            // B8 T1: serialize multipliers parallel to account names
            var mults = new int[rule.FollowerAccounts.Length];
            for (int i = 0; i < rule.FollowerAccounts.Length; i++)
                mults[i] = (rule.FollowerMultipliers != null && i < rule.FollowerMultipliers.Length)
                    ? rule.FollowerMultipliers[i] : 1;

            // B8 T2: serialize ATM mode names using AtmModeToString + GetAtmMode per follower
            var atmNames = new string[rule.FollowerAccounts.Length];
            for (int i = 0; i < rule.FollowerAccounts.Length; i++)
            {
                string accName = rule.FollowerAccounts[i] != null ? rule.FollowerAccounts[i].Name : string.Empty;
                atmNames[i] = AtmModeToString(GetAtmMode(rule, accName));
            }

            return new CopyRuleDto
            {
                InstrumentName       = rule.Instrument,
                MasterAccountName    = rule.MasterAccount != null ? rule.MasterAccount.Name : string.Empty,
                FollowerAccountNames = followerNames,
                IsEnabled            = rule.Enabled,
                FollowerMultipliers  = mults,
                FollowerAtmModeNames = atmNames,
                TightenTicks         = rule.TightenTicks,  // B10 T3: emit tighten ticks
            };
        }

        // B8 T1: DtoToRule -- reads FollowerMultipliers null-safely (B6/B7 XML backward compat)
        private static CopyRule DtoToRule(CopyRuleDto dto)
        {
            Account master = null;
            foreach (var acc in Account.All)
            {
                if (acc.Name == dto.MasterAccountName)
                {
                    master = acc;
                    break;
                }
            }

            var followers = new Account[dto.FollowerAccountNames.Length];
            for (int i = 0; i < dto.FollowerAccountNames.Length; i++)
            {
                foreach (var acc in Account.All)
                {
                    if (acc.Name == dto.FollowerAccountNames[i])
                    {
                        followers[i] = acc;
                        break;
                    }
                }
            }

            // B8 T1: null-safe multiplier read (B6/B7 XML has no FollowerMultipliers element)
            int[] multipliers = null;
            if (dto.FollowerMultipliers != null && dto.FollowerMultipliers.Length > 0)
                multipliers = dto.FollowerMultipliers;

            // B8 T2: parse ATM mode names null-safely; build Dictionary (backward compat with B6/B7 XML)
            var atmMap = new Dictionary<string, FollowerAtmMode>();
            if (dto.FollowerAtmModeNames != null)
            {
                for (int i = 0; i < dto.FollowerAtmModeNames.Length && i < dto.FollowerAccountNames.Length; i++)
                {
                    string accName = dto.FollowerAccountNames[i];
                    if (!string.IsNullOrEmpty(accName))
                        atmMap[accName] = ParseAtmModeName(dto.FollowerAtmModeNames[i]);
                }
            }

            // B10 T3: backward compat -- old XML has no TightenTicks element, XmlSerializer sets to 0.
            // DtoToRule converts: 0 -> default 5. Any positive value is preserved as-is.
            int tightenTicks = dto.TightenTicks > 0 ? dto.TightenTicks : 5;

            return CopyRule.Create(dto.InstrumentName, master, followers, dto.IsEnabled, multipliers, atmMap,
                tightenTicks);
        }

        // -- B6: Public persistence API --------------------------------------

        /// <summary>
        /// Serializes the current rule set to an XML file.
        /// Called from TradeCopierWindow.OnDestroyed() on the NT main thread.
        /// Swallows IOException to prevent NT shutdown crash on I/O failure.
        /// No lock keyword -- called only from NT main thread at shutdown.
        /// CYC = 2 (try/catch = 1 branch)
        /// </summary>
        public void SaveRules(string overridePath = null)
        {
            try
            {
                var path = GetPersistencePath(overridePath);
                var dir = Path.GetDirectoryName(path);
                if (dir != null)
                    Directory.CreateDirectory(dir);

                var container = new CopyRulesContainer();
                foreach (var rule in _rules)
                    container.Rules.Add(RuleToDto(rule));
                container.CopyEnabled = _isCopyEnabled;  // B54: persist enabled state

                var serializer = new XmlSerializer(typeof(CopyRulesContainer));
                var xml = string.Empty;
                using (var writer = new System.IO.StringWriter())
                {
                    serializer.Serialize(writer, container);
                    xml = writer.ToString();
                }
                File.WriteAllText(path, xml);
            }
            catch (Exception)
            {
                // Swallow IO/serialization errors -- must not crash NT on shutdown
            }
        }

        /// <summary>
        /// Deserializes rules from an XML file and adds them to _rules via ConcurrentBag.Add().
        /// Called from TradeCopierWindow.OnInitialize() on the NT main thread.
        /// No-op if the file does not exist or has already been loaded.
        /// No lock keyword -- called once at startup; _rules is ConcurrentBag (thread-safe Add).
        /// CYC = 4 (loaded guard + File.Exists guard + try/catch + foreach)
        /// </summary>
        public void LoadRules(string overridePath = null)
        {
            if (_persistenceLoaded)
                return;
            _persistenceLoaded = true;

            var path = GetPersistencePath(overridePath);
            if (!File.Exists(path))
                return;

            try
            {
                var xml = File.ReadAllText(path);
                var serializer = new XmlSerializer(typeof(CopyRulesContainer));
                using (var reader = new System.IO.StringReader(xml))
                {
                    var container = (CopyRulesContainer)serializer.Deserialize(reader);
                    if (container != null && container.Rules != null)
                    {
                        foreach (var dto in container.Rules)
                            _rules.Add(DtoToRule(dto));
                        _isCopyEnabled = container.CopyEnabled;             // B54: restore enabled state
                        CopyEnabledChanged?.Invoke(_isCopyEnabled);         // B54: sync UI buttons
                    }
                }
            }
            catch (Exception)
            {
                // Swallow deserialization errors -- missing/corrupt file is non-fatal
            }
        }
    }
}
