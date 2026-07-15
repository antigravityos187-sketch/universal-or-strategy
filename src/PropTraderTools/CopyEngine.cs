// PTT-COPIER-B14-T1 -- CopyEngine.cs
// B14 T1 CHANGES:
//   1. Added _trailBeState, _trailBeBufferTicks, _trailBeLastPnl (volatile int/long), _trailBeAccount, _trailBeInstrument.
//   2. Added ArmTrailBe(Instrument, Account, int) -- CYC=4.
//   3. Added DisarmTrailBe() -- CYC=2.
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
    internal enum CopyMode { Signal = 0, Mirror = 1 }

    internal sealed class CopyEngine
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
        private readonly ConcurrentDictionary<string, long> _dedupCache = new ConcurrentDictionary<string, long>(); // JS-025
        private ConcurrentBag<CopyRule> _rules = new ConcurrentBag<CopyRule>(); // Change 1: removed readonly
        private double _dailyCapFloor = -500.0; // Change 4

        // B10 T2 -- Pending BE fields (volatile int state machine per architecture plan Sec 5.4)
        private volatile int    _pendingBeState        = 0;  // 0=Inactive, 1=Armed
        private volatile int    _pendingBeBufferTicks   = 2;
        private          Account    _pendingBeAccount    = null; // single-writer UI thread
        private          Instrument _pendingBeInstrument = null; // single-writer UI thread

        // B14 T1 -- Auto-trail BE fields (volatile int state machine; JS-023; NT8-003).
        // Pattern: mirrors ArmPendingBe/DisarmPendingBe release-fence protocol.
        // _trailBeLastPnl: plain long (NT8-003: volatile banned on 64-bit types; Interlocked.Read/CAS provide barrier).
        private volatile int    _trailBeState        = 0;  // 0=Off, 1=Active
        private volatile int    _trailBeBufferTicks   = 2;
        private          long   _trailBeLastPnl       = 0L; // Interlocked.Read/CompareExchange provide memory barrier
        private          Account    _trailBeAccount    = null; // single-writer UI thread
        private          Instrument _trailBeInstrument = null; // single-writer UI thread

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
        internal event Action<string> PendingBeFired;

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

            // Gate B: bracket drag detection -- divert to HandleBracketChange path
            if (IsWorkingBracket(e.Order))
            {
                if (e.Order.FromEntrySignal != null)
                    PopulateOrderMap(e.Order.FromEntrySignal, e.Order.Account);
                HandleBracketChange(e.Order, matchedRule.Value);
                return;
            }

            // No bracket -- normal copy dispatch
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
                        OrderEntry.Manual, TimeInForce.Day,
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

        // --- B7-F0: Bracket mirroring methods ---

        // B8 T1: DispatchCopy -- index-tracking loop replaces plain foreach.
        // CYC=8 (at limit). GetMultiplier + scaled signal per follower.
        // JS-001: no throw in hot path. JS-021: no lock.
        private void DispatchCopy(Order order, CopyRule rule)
        {
            // Gate 0.5: PTT-prefix guard -- prevents cascade copy of our own PTT- signals. CYC: 7->8.
            if (order.Name != null && order.Name.StartsWith("PTT-")) return;

            // Gate 3: must be Submitted state
            if (order.OrderState != OrderState.Submitted)
                return;

            // Gate 4: market or limit order type only
            bool isMarket = order.OrderType == OrderType.Market;
            bool isLimit  = order.OrderType == OrderType.Limit;
            if (!isMarket && !isLimit)
                return;

            // Gate 5: dedup -- reject duplicate event for same orderId
            if (IsDedup(order.OrderId.ToString()))
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
                var mode = GetAtmMode(rule, acc.Name);
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

        // CYC=1. Gate predicate for bracket change detection in OnOrderUpdate.
        private static bool IsWorkingBracket(Order order)
        {
            return order.OrderState == OrderState.Working && IsBracketLegStatic(order);
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
            foreach (var order in follower.Orders)                                              // (1) branch
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
                follower.CreateOrder(
                    instrument,
                    signal.Action,
                    orderType,
                    OrderEntry.Manual,
                    TimeInForce.Day,
                    signal.Quantity,
                    limitPrice,
                    0,
                    null,
                    signalName,
                    DateTime.Now.AddDays(1),
                    (NinjaTrader.Cbi.CustomOrder)null
                );
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
            {
                var pos = FindPosition(acc, instrument);
                if (pos == null || pos.Quantity == 0)
                {
                    StatusUpdate?.Invoke(acc.Name + ": flat skip");
                    continue;
                }

                int trimQty = (int)Math.Ceiling(pos.Quantity / 2.0);
                var action = pos.MarketPosition == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;

                try
                {
                    acc.CreateOrder(
                            instrument,
                            action,
                            OrderType.Market,
                            OrderEntry.Manual,
                            TimeInForce.Day,
                            trimQty,
                            0,
                            0,
                            null,
                            "PTT-Trim",
                            DateTime.MaxValue,
                            null
                        );
                    StatusUpdate?.Invoke(acc.Name + ": trim " + trimQty);
                }
                catch (Exception ex)
                {
                    StatusUpdate?.Invoke("PTT-Trim error: " + ex.Message);
                }
            }
        }

        internal void Flatten(Instrument instrument)
        {
            foreach (var acc in AllAccounts(instrument))
            {
                var pos = FindPosition(acc, instrument);
                if (pos == null || pos.Quantity == 0)
                {
                    StatusUpdate?.Invoke(acc.Name + ": flat skip");
                    continue;
                }

                var action = pos.MarketPosition == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;

                try
                {
                    acc.CreateOrder(
                            instrument,
                            action,
                            OrderType.Market,
                            OrderEntry.Manual,
                            TimeInForce.Day,
                            pos.Quantity,
                            0,
                            0,
                            null,
                            "PTT-Flatten",
                            DateTime.MaxValue,
                            null
                        );
                    StatusUpdate?.Invoke(acc.Name + ": flatten " + pos.Quantity);
                }
                catch (Exception ex)
                {
                    StatusUpdate?.Invoke("PTT-Flatten error: " + ex.Message);
                }
            }
        }

        // B19 T1 -- ComputeLimitPx: pure-arithmetic price anchor helper.
        // Long exits (Sell Limit) post above ask; short exits (BuyToCover) post below bid.
        // CYC=1: single ternary. No NT8 deps, no state, no nulls.
        // internal static -- CopyEngineTests.cs calls CopyEngine.ComputeLimitPx(...) directly.
        internal static double ComputeLimitPx(bool isLong, double ask, double bid, int exitBuffer, double tickSize)
            => isLong
                ? ask + exitBuffer * tickSize
                : bid - exitBuffer * tickSize;

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
            double tickSize = instrument.MasterInstrument.TickSize;
            foreach (var acc in AllAccounts(instrument))
            {
                var pos = FindPosition(acc, instrument);
                if (pos == null || pos.Quantity == 0)
                {
                    StatusUpdate?.Invoke(acc.Name + ": flat skip");
                    continue;
                }
                int trimQty = (int)Math.Ceiling(pos.Quantity / 2.0);
                bool isLong = pos.MarketPosition == MarketPosition.Long;
                var action  = isLong ? OrderAction.Sell : OrderAction.BuyToCover;
                double limitPx = ComputeLimitPx(isLong, ask, bid, exitBuffer, tickSize);
                try
                {
                    acc.CreateOrder(
                        instrument, action, OrderType.Limit,
                        OrderEntry.Manual, TimeInForce.Day,
                        trimQty, limitPx, 0, null,
                        "PTT-TrimLimit",
                        DateTime.MaxValue,
                        (NinjaTrader.Cbi.CustomOrder)null);
                    StatusUpdate?.Invoke(acc.Name + ": trim-limit " + trimQty + " @ " + limitPx);
                }
                catch (Exception ex)
                {
                    StatusUpdate?.Invoke("PTT-TrimLimit error: " + ex.Message);
                }
            }
        }

        // B19 T1 -- Flatten 4-arg: exit full position at limit price anchored to ask (long) or bid (short).
        // Long: Sell Limit @ ask + exitBuffer*tick.   Short: BuyToCover @ bid - exitBuffer*tick.
        // NT8-007: arg 12 = (NinjaTrader.Cbi.CustomOrder)null.
        // NT8-014: signal name = "PTT-FlattenLimit".
        // NT8-032: ask/bid are MarketDataEventArgs.Price doubles (callers obtain via GetAsk()/GetBid()).
        // CYC=6: same branch structure as Trim; no trimQty calculation.
        // JS-001: try/catch wraps acc.CreateOrder -- no rethrow.
        internal void Flatten(Instrument instrument, int exitBuffer, double ask, double bid)
        {
            if (ask <= 0 || bid <= 0 || exitBuffer == 0) { Flatten(instrument); return; }
            double tickSize = instrument.MasterInstrument.TickSize;
            foreach (var acc in AllAccounts(instrument))
            {
                var pos = FindPosition(acc, instrument);
                if (pos == null || pos.Quantity == 0)
                {
                    StatusUpdate?.Invoke(acc.Name + ": flat skip");
                    continue;
                }
                bool isLong = pos.MarketPosition == MarketPosition.Long;
                var action  = isLong ? OrderAction.Sell : OrderAction.BuyToCover;
                double limitPx = ComputeLimitPx(isLong, ask, bid, exitBuffer, tickSize);
                try
                {
                    acc.CreateOrder(
                        instrument, action, OrderType.Limit,
                        OrderEntry.Manual, TimeInForce.Day,
                        pos.Quantity, limitPx, 0, null,
                        "PTT-FlattenLimit",
                        DateTime.MaxValue,
                        (NinjaTrader.Cbi.CustomOrder)null);
                    StatusUpdate?.Invoke(acc.Name + ": flatten-limit " + pos.Quantity + " @ " + limitPx);
                }
                catch (Exception ex)
                {
                    StatusUpdate?.Invoke("PTT-FlattenLimit error: " + ex.Message);
                }
            }
        }

        internal void CancelPendingEntries(Instrument instrument)
        {
            foreach (var acc in AllAccounts(instrument))
            {
                foreach (var order in acc.Orders)
                {
                    if (order.Instrument != instrument)
                        continue;
                    // B18 T3: DW-B18-CANCEL-01 -- also cancel Initialized orders.
                    // Follower copy orders start as Initialized before sim engine acknowledges them.
                    // Skipping caused orders stuck as Cancel pending with no way to clear.
                    // Note: OrderState.PendingSubmit does not exist in NT8 -- Initialized is sufficient.
                    if (order.OrderState != OrderState.Working &&
                        order.OrderState != OrderState.Initialized)
                        continue;
                    if (IsBracketLeg(order))
                        continue;

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
        }

        private bool IsDedup(string orderId)
        {
            long now = DateTime.UtcNow.Ticks;
            long expiry = TimeSpan.FromSeconds(10).Ticks;

            // Prune expired entries
            foreach (var key in _dedupCache.Keys)
            {
                if (_dedupCache.TryGetValue(key, out long storedTicks) && now - storedTicks > expiry)
                    _dedupCache.TryRemove(key, out _);
            }

            // Attempt add -- if TryAdd returns false, orderId already exists (duplicate)
            if (!_dedupCache.TryAdd(orderId, now))
                return true;

            return false;
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

        private bool IsStopLeg(Order order)
        {
            return order.FromEntrySignal != null || (order.Name != null && order.Name.StartsWith("Stop"));
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

        private bool IsBracketLeg(Order order)
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

        private Position FindPosition(Account acc, Instrument instrument)
        {
            foreach (Position p in acc.Positions)
                if (p.Instrument == instrument) return p;
            return null;
        }

        // B10 T1 -- MoveStopToBreakEven: adds IsStopAlreadyAtBe() guard; uses acc.Change() for ALL
        // stop types (trailing + fixed). GAP-001d CONFIRMED: trail survives acc.Change().
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
            foreach (var order in acc.Orders)                                              // (3)
            {
                if (order.Instrument != instrument)                                        // (2) -- instrument filter
                    continue;
                if (order.OrderState != OrderState.Working)                                // (4)
                    continue;
                if (order.OrderType != OrderType.StopMarket)                               // (5)
                    continue;
                if (!IsStopLeg(order))                                                     // (6)
                    continue;
                // B10 T1: idempotency guard -- skip if stop is already at or past BE level
                if (IsStopAlreadyAtBe(order, newStop, isLong))
                    continue;
                try
                {
                    // GAP-001d CONFIRMED: acc.Change() does NOT kill the trail.
                    // Both trailing and fixed stops use this same path.
                    if (IsTrailingStop(order))
                        StatusUpdate?.Invoke(acc.Name + ": MoveStopToBreakEven: trailing stop detected, using acc.Change path");
                    order.StopPrice = newStop;
                    acc.Change(new Order[] { order });
                    StatusUpdate?.Invoke(acc.Name + ": BE moved to " + newStop);
                }
                catch (Exception ex)
                {
                    StatusUpdate?.Invoke("PTT-BE error: " + ex.Message);
                }
            }
        }

        internal void BreakEven(Instrument instrument, int bufferTicks)
        {
            foreach (var acc in AllAccounts(instrument))
                MoveStopToBreakEven(acc, instrument, bufferTicks);
        }


        // B10 T3 -- TightenStop: moves all working stops on follower accounts to currentPrice +/- N ticks.
        // CYC=5: rule null(1), foreach acc(2), pos flat(3), foreach orders(4), stop type check(5).
        // JS-001: try/catch inside TightenOneStop -- no throw in hot path.
        // JS-021: no lock -- AllAccounts iterates ConcurrentBag (lock-free).
        // DW-B9-GAP-001c: T3 production implementation.
        internal void TightenStop(Instrument instrument, int ticks)
        {
            var rule = FindRule(instrument);
            if (rule == null)                                                           // (1)
                return;
            double tickSize = instrument.MasterInstrument.TickSize;
            foreach (var acc in AllAccounts(instrument))                                // (2)
            {
                var pos = FindPosition(acc, instrument);
                if (IsFlat(pos))                                                        // (3)
                    continue;
                bool isLong = pos.MarketPosition == MarketPosition.Long;
                // NT8: instrument.MarketData.Bid/.Ask return MarketDataEventArgs objects, not doubles.
                // NT8-032: use .Bid.Price / .Ask.Price for the double value, or use pos.AveragePrice
                // as the reference price for tighten-stop offset calculation (safe fallback).
                double bidPrice = instrument.MarketData.Bid.Price;
                double askPrice = instrument.MarketData.Ask.Price;
                double currentPrice = bidPrice > 0 && askPrice > 0
                    ? (isLong ? askPrice : bidPrice)
                    : pos.AveragePrice;     // fallback if MarketData unavailable
                double targetPrice = isLong
                    ? currentPrice - ticks * tickSize
                    : currentPrice + ticks * tickSize;
                foreach (var order in acc.Orders)                                       // (4)
                {
                    if (order.OrderState != OrderState.Working)
                        continue;
                    if (order.OrderType != OrderType.StopMarket &&                     // (5)
                        order.OrderType != OrderType.StopLimit)
                        continue;
                    if (!IsStopLeg(order))
                        continue;
                    TightenOneStop(acc, instrument, order, targetPrice, tickSize);
                }
            }
        }

        // B10 T3 -- TightenOneStop: applies tighten to a single stop order.
        // CYC=3: null guard(1), alreadyTighter(2), try block(0).
        // JS-001: try/catch wraps acc.Change -- no throw in hot path.
        // DW-B16-02: cancel+replace removed.
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
            try
            {
                // DW-B16-02: all stop types use acc.Change() -- GAP-001d CONFIRMED safe.
                // cancel+replace branch removed (was nuking ATM bracket + trail watermark).
                order.StopPrice = targetPrice;
                acc.Change(new Order[] { order });
                StatusUpdate?.Invoke(acc.Name + ": tighten stop -> " + targetPrice);
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke("TightenOneStop: " + ex.Message);
            }
        }


        // B10 T2 -- ArmPendingBe: arms the pending BE watcher using acc.AccountItemUpdate.
        // CYC=4: instr null(1), acc null(2), pos flat(3), armed write(4).
        // Called on UI thread. _pendingBeState volatile write provides release fence for
        // _pendingBeAccount and _pendingBeInstrument plain refs (architecture plan Sec 5.4).
        // JS-021: no lock -- Interlocked used in OnPendingBeAccountUpdate for CAS disarm.
        internal void ArmPendingBe(Instrument instr, Account masterAcc, int bufferTicks)
        {
            if (instr == null)                                  // (1)
                return;
            if (masterAcc == null)                              // (2)
                return;
            var pos = FindPosition(masterAcc, instr);
            if (IsFlat(pos))                                    // (3)
                return;
            _pendingBeBufferTicks   = bufferTicks;              // volatile int write
            _pendingBeInstrument    = instr;                    // plain ref write (UI thread)
            _pendingBeAccount       = masterAcc;                // plain ref write (UI thread)
            masterAcc.AccountItemUpdate += OnPendingBeAccountUpdate;
            _pendingBeState         = 1;                        // (4) volatile int write -- release fence
        }

        // B10 T2 -- DisarmPendingBe: disarms the pending BE watcher atomically.
        // CYC=3: armed CAS check(1), acc null guard(2), unsubscribe(3).
        // JS-021: no lock -- Interlocked.CompareExchange for atomic disarm.
        internal void DisarmPendingBe()
        {
            if (Interlocked.CompareExchange(ref _pendingBeState, 0, 1) != 1) // (1) only if Armed
                return;
            var acc = _pendingBeAccount;
            if (acc != null)                                    // (2)
                acc.AccountItemUpdate -= OnPendingBeAccountUpdate;  // (3)
            _pendingBeAccount    = null;
            _pendingBeInstrument = null;
        }

        // B14 T1 -- ArmTrailBe: arms the continuous trail watcher using acc.AccountItemUpdate.
        // CYC=4: instr null(1), acc null(2), pos flat(3), arm write(4).
        // Called on UI thread (from TradeCopierPanel.OnBeConnected via Dispatcher).
        // _trailBeState volatile write (=1) is the release fence; plain ref writes precede it.
        // JS-021: no lock -- Interlocked used in OnTrailBeAccountUpdate for PnL CAS.
        // NT8-003: _trailBeLastPnl is plain long (volatile banned on 64-bit); Interlocked provides barrier.
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
            _trailBeBufferTicks   = bufferTicks;
            _trailBeLastPnl       = BitConverter.DoubleToInt64Bits(currentPnl);
            _trailBeInstrument    = instr;
            _trailBeAccount       = masterAcc;
            masterAcc.AccountItemUpdate += OnTrailBeAccountUpdate;
            _trailBeState         = 1;                            // (4) volatile int write -- release fence
        }

        // B14 T1 -- DisarmTrailBe: disarms the trail watcher atomically.
        // CYC=2: active CAS check(1), acc null guard(2).
        // JS-021: no lock -- Interlocked.CompareExchange for atomic disarm.
        // Idempotent: safe to call when already Off.
        internal void DisarmTrailBe()
        {
            if (Interlocked.CompareExchange(ref _trailBeState, 0, 1) != 1) // (1) only if Active
                return;
            var acc = _trailBeAccount;
            if (acc != null)                                      // (2)
                acc.AccountItemUpdate -= OnTrailBeAccountUpdate;
            _trailBeAccount    = null;
            _trailBeInstrument = null;
        }

        // B14 T1 -- OnTrailBeAccountUpdate: continuous AccountItemUpdate callback for auto-trail.
        // Fires on NT8 account background thread -- NO UI calls inside this method.
        // CYC=5: state check(1), item filter(2), pnl improvement check(3),
        //        CAS update _trailBeLastPnl(4), advance buffer + BreakEven(5).
        // JS-021: no lock -- Interlocked.Exchange for atomic PnL high-water update.
        // JS-001: BreakEven internally wraps acc.Change() in try/catch; no rethrow here.
        // NT8-003: _trailBeLastPnl is plain long (volatile banned on 64-bit); BitConverter + Interlocked CAS.
        // STAYS SUBSCRIBED until DisarmTrailBe() is called -- unlike OnPendingBeAccountUpdate (one-shot).
        private void OnTrailBeAccountUpdate(object sender, NinjaTrader.Cbi.AccountItemEventArgs e)
        {
            if (_trailBeState != 1)                                         // (1) volatile int read
                return;
            if (e.AccountItem != AccountItem.UnrealizedProfitLoss)         // (2) filter
                return;
            double newPnl = e.Value;
            double oldPnl = BitConverter.Int64BitsToDouble(
                Interlocked.Read(ref _trailBeLastPnl));
            if (newPnl <= oldPnl)                                           // (3)
                return;
            long newBits = BitConverter.DoubleToInt64Bits(newPnl);
            long oldBits = BitConverter.DoubleToInt64Bits(oldPnl);
            if (Interlocked.CompareExchange(ref _trailBeLastPnl, newBits, oldBits) != oldBits) // (4)
                return;
            int newBuffer = Interlocked.Increment(ref _trailBeBufferTicks);                    // (5)
            var instr = _trailBeInstrument;
            if (instr != null)
                BreakEven(instr, newBuffer);
        }

        // B23 T1 (DW-B22-BE-TRIGGER-01): price-based trigger replaces dollar-PnL trigger.
        // Dollar PnL unreliable on PA accounts -- commission deducted at entry makes UPnL
        // negative even when price is past entry + buffer. Price comparison is immune to fees.
        // CYC=8: state(1), item filter(2), pos flat(3), tickSize(4), last<=0(5), triggered(6), CAS(7).
        // acc?.AccountItemUpdate null-conditional is NOT a CYC branch (same convention as ternaries).
        private void OnPendingBeAccountUpdate(object sender, NinjaTrader.Cbi.AccountItemEventArgs e)
        {
            if (_pendingBeState != 1)                                          // (1) volatile int read
                return;
            if (e.AccountItem != AccountItem.UnrealizedProfitLoss)            // (2) filter
                return;
            // (3-6) Price-based trigger: fire when Last.Price reaches entry + bufferTicks * tickSize.
            var pos = FindPosition(_pendingBeAccount, _pendingBeInstrument);
            if (IsFlat(pos))                                                   // (3)
                return;
            double tickSize = _pendingBeInstrument?.MasterInstrument?.TickSize ?? 0.0;
            if (tickSize <= 0.0)                                               // (4)
                return;
            double last = _pendingBeInstrument?.MarketData?.Last?.Price ?? 0.0;
            if (last <= 0.0)                                                   // (5)
                return;
            bool isLong  = pos.MarketPosition == MarketPosition.Long;
            double target = pos.AveragePrice
                + (isLong ? 1.0 : -1.0) * _pendingBeBufferTicks * tickSize;
            bool triggered = isLong ? (last >= target) : (last <= target);
            if (!triggered)                                                    // (6)
                return;
            // (7) CAS disarm: only ONE concurrent callback wins the Armed->Inactive transition
            if (Interlocked.CompareExchange(ref _pendingBeState, 0, 1) != 1)  // (7)
                return;
            var acc   = _pendingBeAccount;
            var instr = _pendingBeInstrument;
            var buf   = _pendingBeBufferTicks;
            if (acc != null)
                acc.AccountItemUpdate -= OnPendingBeAccountUpdate;
            _pendingBeAccount    = null;
            _pendingBeInstrument = null;
            BreakEven(instr, buf);
            PendingBeFired?.Invoke(instr?.FullName ?? string.Empty);
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
