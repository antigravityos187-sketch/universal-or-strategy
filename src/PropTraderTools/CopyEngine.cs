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

// B113 test seam: grants PropTraderTools.Tests access to internal members
// (_qxPendingFollowerCleanup, TryCleanupReArmedAtmBracket).
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PropTraderTools.Tests")]

namespace PropTraderTools
{
    // V01: Binding record for _orderMap inner collection
    // JS-003: readonly struct prevents field transposition
    internal struct FollowerBinding
    {
        internal Account FollowerAccount { get; private set; }
        internal string FromEntrySignalName { get; private set; }

        internal FollowerBinding(Account account, string signalName)
        {
            FollowerAccount = account;
            FromEntrySignalName = signalName;
        }
    }

    // V05: Position truth snapshot -- JS-003 (readonly struct prevents bool transposition)
    public struct PositionState
    {
        public bool HasOpenPosition { get; private set; }
        public bool HasWorkingEntries { get; private set; }

        public PositionState(bool hasOpen, bool hasWorking)
        {
            HasOpenPosition = hasOpen;
            HasWorkingEntries = hasWorking;
        }
    }

    // V06: ATM mode discriminated union -- JS-003 + JS-010
    // NT8 Roslyn: records with positional params generate IsExternalInit (CS0518). Use abstract class instead.
    public abstract class FollowerAtmMode
    {
        private FollowerAtmMode() { } // JS-010: private base constructor -- no external subclassing

        public sealed class Inherit : FollowerAtmMode
        {
            public Inherit() { }
        } // B7 default

        public sealed class Market : FollowerAtmMode
        {
            public Market() { }
        } // pure market

        public sealed class Named : FollowerAtmMode
        {
            public string TemplateName { get; }

            // HOTFIX-B66-ATM-OBJ: AtmObject carries the live ChartTrader.AtmStrategy instance.
            // When non-null, SendCopyWithAtm uses StartAtmStrategy(atm, order) object overload
            // instead of StartAtmStrategy(string, order) -- avoids reading .Name (returns class name).
            public NinjaTrader.NinjaScript.AtmStrategy AtmObject { get; }

            public Named(string templateName)
            {
                TemplateName = templateName;
                AtmObject = null;
            }

            public Named(string templateName, NinjaTrader.NinjaScript.AtmStrategy atmObj)
            {
                TemplateName = templateName;
                AtmObject = atmObj;
            }
        }
    }

    // B8: SendCopy switch + UI dropdown wired in T2.

    // B9 T3 -- Copy mode discriminated union (JS-023: volatile int backing for thread-safe reads/writes)
    internal enum CopyMode
    {
        Signal = 0,
        Mirror = 1,
        Clone = 2,
    }

    internal sealed class CopyEngine : ICopyEngine
    {
        // --- Singleton ---
        private static readonly CopyEngine _instance = new CopyEngine();
        public static CopyEngine Instance => _instance;

        // --- State ---
        private volatile bool _isCopyEnabled; // JS-023

        // B9 T1 -- ATR sizing engine integration (JS-023: volatile, ADV-002 fix)
        private volatile bool _atrEnabled = false; // JS-023
        private volatile AtrSizingEngine _atrEngine = null; // write/read on UI thread only

        // B9 T3 -- Mirror mode (JS-023: volatile int backing for CopyMode enum)
        private volatile int _copyModeValue = 0; // 0=Signal (default), 1=Mirror

        // B50 -- _cloneAtmCache: volatile string holds ATM template name for display/logging only.
        // volatile string: reference-type writes are atomic on CLR 4.0+ (JS-023 compliant).
        // NT8-003: volatile double/float BANNED -- string is safe.
        private volatile string _cloneAtmCache = string.Empty;

        // HOTFIX-B66-ATM-OBJ: volatile reference to live ChartTrader.AtmStrategy object.
        // Captured at Clone mode click; used in StartAtmStrategy(atm, order) object overload.
        // volatile object reference write/read: atomic on CLR 4.0+ (JS-023 compliant).
        private volatile NinjaTrader.NinjaScript.AtmStrategy _cloneAtmObject = null;

        // BGTM-1: Feature flags -- volatile reference (atomic on CLR 4.0+, JS-023 compliant).
        // SetFlags called from UI thread only. Read from UI thread only.
        private volatile FeatureFlags _flags = FeatureFlags.Starter();

        /// <summary>Current feature flags snapshot.</summary>
        public FeatureFlags Flags => _flags;

        /// <summary>Fires on UI thread when license activation changes flags.</summary>
        public event Action<FeatureFlags> FeatureFlagsChanged;

        // BGTM-1: Assign flags and broadcast. CYC=1. JS-021: no lock.
        internal void SetFlags(FeatureFlags f)
        {
            _flags = f;
            FeatureFlagsChanged?.Invoke(f);
        }

        // B39 -- _globalBe: singleton reference to shared Global BE execution engine.
        // Lazily initialized; Panel and Window read via GlobalBe property (UI thread only).
        // JS-023: volatile null-check safe for singleton reads on CLR 4.0+.
        private PttGlobalBreakEven _globalBe = null;

        // B62: value changed from long (timestamp) to double (last dispatched LimitPrice).
        // Enables drag detection: same orderId + different price = leader dragged.
        // JS-025: ConcurrentDictionary is lock-free.
        private readonly ConcurrentDictionary<string, double> _dedupCache =
            new ConcurrentDictionary<string, double>(); // JS-025

        // DW-B91-A: per-orderId dispatch guard -- survives EvictDedup terminal-state eviction.
        // After DispatchCopy commits a copy dispatch for orderId, TryAdd records it here.
        // On a second dispatch-triggering event for the same orderId (e.g. Rithmic re-submit),
        // ContainsKey returns true before DispatchCopy can fire again.
        // Eviction is co-located with _dedupCache eviction in EvictDedup -- both cleared on
        // Filled/Cancelled/Rejected so the slot is reclaimed when the order lifecycle closes.
        // Key = order.OrderId.ToString(). Value = byte (minimum footprint -- presence-only set).
        // JS-021: ConcurrentDictionary.ContainsKey and TryAdd are lock-free atomic operations.
        // JS-025: ConcurrentDictionary is the canonical lock-free set pattern.
        private readonly ConcurrentDictionary<string, byte> _entryDispatchedOrders =
            new ConcurrentDictionary<string, byte>();

        // DW-B136 Gap B: leader order ID -> follower Order objects dispatched for that leader order.
        // Key = leader order.OrderId.ToString() (same format as _dedupCache and _entryDispatchedOrders).
        // Value = ConcurrentBag<Order> of follower Order objects submitted for this leader order.
        // Used by TryCancelFollowerEntries to scope cancel to the specific leader order being cancelled.
        // JS-021: no lock. JS-025: ConcurrentDictionary + ConcurrentBag (lock-free).
        // JS-001: only cancel calls are wrapped in try/catch in CancelScopedFollowerEntries.
        // Eviction: TryRemove called in CancelScopedFollowerEntries (cancel path) after iterating the bag.
        // NOTE: EvictDedup does NOT touch this map -- see execution-order note in LaneB-02-architecture-plan.md.
        internal readonly ConcurrentDictionary<string, ConcurrentBag<Order>> _followerCopyMap =
            new ConcurrentDictionary<string, ConcurrentBag<Order>>();

        // DW-B92: count of PTT-BE-Target-* fills per account for this trade slot.
        // Incremented synchronously in OnOrderUpdate BEFORE the OCO cancel event
        // arrives, eliminating the HasFilledBeTarget acc.Orders scan race.
        // Key = acc.Name. Cleared in TryEvictFollowerBeSlot on position flat.
        // JS-025: ConcurrentDictionary is lock-free. JS-021: no lock.
        private readonly ConcurrentDictionary<string, int> _filledBeTargetCount =
            new ConcurrentDictionary<string, int>();
        private ConcurrentBag<CopyRule> _rules = new ConcurrentBag<CopyRule>(); // Change 1: removed readonly

        // B127: lazy-resolve cache -- name -> Account. Populated on first successful resolve in
        // AllAccounts(). Lock-free: ConcurrentDictionary TryGetValue + TryAdd (JS-021 compliant).
        // Cleared on each LoadRules() call to handle account reconnect / session restart scenarios.
        // readonly: ConcurrentDictionary is a reference type; .Clear() works on the instance.
        private readonly ConcurrentDictionary<string, Account> _resolvedFollowers =
            new ConcurrentDictionary<string, Account>(StringComparer.Ordinal);
        private double _dailyCapFloor = -500.0; // Change 4

        // B27 -- Per-account BE slot structs (DW-B27-01: replaces singleton fields).
        // NT8-001: 'readonly' fields, NOT init setters. NT8-005: NOT 'readonly struct'.
        // NT8-004: struct in ConcurrentDictionary<string,TSlot> confirmed safe in NT8.
        private struct PendingBeSlot
        {
            internal readonly Account Account;
            internal readonly Instrument Instrument;
            internal readonly int BufferTicks;

            internal PendingBeSlot(Account a, Instrument i, int b)
            {
                Account = a;
                Instrument = i;
                BufferTicks = b;
            }
        }

        private struct TrailBeSlot
        {
            internal readonly Account Account;
            internal readonly Instrument Instrument;
            internal readonly int BufferTicks;

            internal TrailBeSlot(Account a, Instrument i, int b)
            {
                Account = a;
                Instrument = i;
                BufferTicks = b;
            }
        }

        // B27 -- Pending BE slot dictionary (DW-B27-01: replaces 4 singleton fields).
        // Key = account.Name. JS-021: TryGetValue/TryRemove/AddOrUpdate are lock-free.
        private readonly ConcurrentDictionary<string, PendingBeSlot> _pendingBeSlots =
            new ConcurrentDictionary<string, PendingBeSlot>();

        // B27 -- Trail BE slot dictionary (DW-B27-01: replaces 5 singleton fields).
        // LastPnlBits lives in _trailBeLastPnlBits (separate dict) because struct values
        // in ConcurrentDictionary are value types -- Interlocked CAS requires a ref to a
        // field, impossible on a boxed struct. NT8-003: no volatile on long.
        private readonly ConcurrentDictionary<string, TrailBeSlot> _trailBeSlots =
            new ConcurrentDictionary<string, TrailBeSlot>();
        private readonly ConcurrentDictionary<string, long> _trailBeLastPnlBits =
            new ConcurrentDictionary<string, long>();

        // DW-B79-06: PendingFollowerBeSlot -- event-driven deferred BE for QX->BE-ALL race fix.
        // Registered by MoveStopToBreakEven when targets=0 on a follower account (first call only).
        // Consumed atomically by TryFireFollowerBeRetry in OnOrderUpdate when a PTT-QX-T* order
        // transitions to Working -- fires MoveStopToBreakEven at the exact correct moment,
        // not at an arbitrary 350ms offset. Eliminates the QX->BE-ALL race permanently.
        // Key = acc.Name. JS-021: ConcurrentDictionary TryRemove is lock-free atomic claim.
        // NT8-004: struct in ConcurrentDictionary confirmed safe in NT8.
        private struct PendingFollowerBeSlot
        {
            internal readonly Account Account;
            internal readonly Instrument Instrument;
            internal readonly int BufferTicks;

            internal PendingFollowerBeSlot(Account a, Instrument i, int b)
            {
                Account = a;
                Instrument = i;
                BufferTicks = b;
            }
        }

        private readonly ConcurrentDictionary<
            string,
            PendingFollowerBeSlot
        > _pendingFollowerBeSlots = new ConcurrentDictionary<string, PendingFollowerBeSlot>();

        // DW-B79-08 v3: per-account retry attempt counter for TryReplacePttBeBrackets.
        // Prevents unbounded retry storm when NT8's ATM sweep repeatedly cancels every PTT-BE-*
        // order that TryReplacePttBeBrackets re-places (each re-place gets swept again).
        // Limit: 3 attempts per account per trade. Reset by TryEvictFollowerBeSlot when flat.
        // Key = acc.Name. JS-021: ConcurrentDictionary write is lock-free. JS-023: int values.
        private readonly ConcurrentDictionary<string, int> _beReplaceAttempts =
            new ConcurrentDictionary<string, int>();

        // DW-B105: QX-ALL intent guard. Set per follower account by PttGlobalQuickExit.ExecuteOne
        // before CancelQxBrackets, cleared after. TryReplacePttBeBrackets returns early if set.
        // ConcurrentDictionary: JS-021 lock-free. Key = acc.Name (string). Value = bool (unused).
        internal readonly ConcurrentDictionary<string, bool> _qxCancelInProgress =
            new ConcurrentDictionary<string, bool>();

        // B113 DW-B117: cancel-after cleanup map. Set by PttGlobalQuickExit.ExecuteOne
        // immediately after executor.Execute for follower accounts. OnOrderUpdate reads this
        // to cancel native ATM Target* one-for-one as each PTT-QX-T* confirms Working.
        // Key = acc.Name. Value = (instrument, expiry=UtcNow+2s).
        // JS-021: ConcurrentDictionary -- lock-free. No lock() anywhere.
        internal readonly ConcurrentDictionary<
            string,
            (Instrument Instr, DateTime Expiry)
        > _qxPendingFollowerCleanup = new ConcurrentDictionary<string, (Instrument, DateTime)>();

        // HOTFIX-MSTBE-OCO-REUSE: monotonic counter for BE OCO IDs -- never reuse a cancelled OCO ID.
        // DW-B40-OCO-02 pattern from PttBreakEven._beOcoSeq. JS-023: volatile int allowed.
        // HOTFIX-BEALL-OCO-SEQ-SHARED-01: shared by BOTH MoveStopToBreakEven AND PttBreakEven.Execute
        // so the two paths never collide on the same OCO ID. PttBreakEven calls NextBeOcoSeq() instead
        // of its own _beOcoSeq, ensuring global uniqueness across all BE code paths.
        // DW-B89-01 SEED FIX: XOR Environment.TickCount with low 31 bits of DateTime.UtcNow.Ticks.
        // NT8 keeps cancelled OCO IDs for the entire NT8 session. When NT8 recompiles an AddOn
        // within a running session, CopyEngine is GC'd and re-created. TickCount alone can repeat
        // within the same millisecond on fast recompile. XOR with Ticks (100ns resolution) ensures
        // post-recompile seed is statistically unique. Math.Abs: XOR can set sign bit; wraps safely.
        // JS-023: volatile int. Interlocked.Increment in NextBeOcoSeq() unchanged. No lock added.
        private volatile int _mstbeOcoSeq = Math.Abs(
            Environment.TickCount ^ (int)(DateTime.UtcNow.Ticks & 0x7FFFFFFF)
        );

        internal int NextBeOcoSeq() => System.Threading.Interlocked.Increment(ref _mstbeOcoSeq);

        // HOTFIX-B76-POSSTATE-DEDUP-01: atomic hasPos dedup per instrument.
        // Value is int[1]: 0=False, 1=True, 2=unknown (initial).
        // int[] gives a stable heap ref so Interlocked.Exchange can operate on [0].
        // GetOrAdd allocates the array once per instrument on first fill (not per-fill).
        // Interlocked.Exchange is the only write -- one winner per transition, all others
        // see the new value already written and return without invoking. JS-021: lock-free.
        private readonly ConcurrentDictionary<string, int[]> _lastHasPos =
            new ConcurrentDictionary<string, int[]>();

        // B119: DW-B128 -- reversal entry guard.
        // Keyed by instrument FullName, value is the last OrderAction dispatched for that instrument.
        // ConcurrentDictionary: thread-safe without lock(). JS-021: no lock.
        private readonly ConcurrentDictionary<string, OrderAction> _lastLeaderDirection =
            new ConcurrentDictionary<string, OrderAction>();

        // V01: order map for follower bracket lookup
        // JS-025: ConcurrentDictionary (atomic GetOrAdd) + ConcurrentBag (lock-free Add/iterate)
        // JS-021: NO lock keyword anywhere
        private readonly ConcurrentDictionary<string, ConcurrentBag<FollowerBinding>> _orderMap =
            new ConcurrentDictionary<string, ConcurrentBag<FollowerBinding>>();

        // --- Status event ---
        internal event Action<string> StatusUpdate;

        // V05: position state change notification for UI surfaces
        // Fired from TryFirePositionState -- before Gate 1 (fires even when copy is disabled)
        public event Action<string, PositionState> PositionStateChanged;

        // B10 T2 -- Pending BE fired notification (fires on NT8 account bg thread; Panel marshals to UI)
        internal event Action<string, string> PendingBeFired;
        internal event Action<string, string> PendingBeArmed; // HOTFIX-BEALL-SYNC-01: instr, accName
        internal event Action<int> GlobalBeBufferChanged; // HOTFIX-BEALL-BUFFER-SYNC-01: fires on buffer +/- spin, int = new buffer value

        // CS0070 fix: event can only be raised from declaring class (CopyEngine). PttGlobalBreakEven calls this relay.
        internal void RaiseBeBufferChanged(int newValue) =>
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                GlobalBeBufferChanged?.Invoke(newValue)
            ); // HOTFIX-DISPATCH-FIX-01: fire on app UI thread

        // HOTFIX-QUICKALL-SINGLETON-01: Quick ALL tick buffer -- singleton so all panels share the same value.
        // JS-023: volatile int allowed. NT8-003: volatile double banned -- not used here.
        private volatile int _globalQuickAllT1 = 4; // default 4 ticks (same as per-panel default)
        internal int GlobalQuickAllT1 => _globalQuickAllT1;

        internal void IncrementQuickAll()
        {
            if (_globalQuickAllT1 < 99)
                _globalQuickAllT1++;
            int v = _globalQuickAllT1;
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                GlobalQuickAllBufferChanged?.Invoke(v)
            ); // HOTFIX-DISPATCH-FIX-01
        }

        internal void DecrementQuickAll()
        {
            if (_globalQuickAllT1 > 1)
                _globalQuickAllT1--;
            int v = _globalQuickAllT1;
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                GlobalQuickAllBufferChanged?.Invoke(v)
            ); // HOTFIX-DISPATCH-FIX-01
        }

        internal event Action<int> GlobalQuickAllBufferChanged; // HOTFIX-QUICKALL-SINGLETON-01

        // HOTFIX-BEALL-DISARM-SYNC-01: broadcast so all panels reset BE ALL visual to Idle on disarm.
        // Fired from OnGlobalBeClick disarm path AND from UpdateButtonColors HOTFIX-F3 branch (position close).
        internal event Action GlobalBeAllDisarmed;

        internal void RaiseBeAllDisarmed() => GlobalBeAllDisarmed?.Invoke();

        // B20-LANE-A T2: Copy ON/OFF sync event (DW-B17-SYNC-01)
        // Plain delegate field -- NOT lock-guarded (JS-021). Fired from SetEnabled on every toggle.
        // Lane C wires TradeCopierPanel and TradeCopierWindow subscribers.
        public event Action<bool> CopyEnabledChanged;

        // B132 LaneB diagnostic gate -- set to false to disable all TP1-TP4 Print calls.
        // Remove this field and all TryLogDragTrace / TryLogSFBTrace calls when DW-B138 is confirmed fixed.
        // JS-021: static bool read is lock-free (no torn reads on bool). Not volatile (diagnostic only).
        private static bool _diagnosticMode = true;

        // --- Nested structs ---

        internal readonly struct CopyRule
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

            // B127: original follower account names parallel to FollowerAccounts[] -- enables
            // lazy re-resolve in AllAccounts() for null slots at DtoToRule/LoadRules time.
            // JS-008: readonly array field on readonly struct -- reference is immutable, compliant.
            // DW-PTT-BE-FIX-01.
            internal readonly string[] FollowerAccountNames;

            // B8 T1: updated private constructor (adds multipliers + atmTemplates parameters)
            // B10 T3: updated to include tightenTicks
            // B127: updated to include followerAccountNames (8th param, DW-PTT-BE-FIX-01)
            private CopyRule(
                string instrument,
                Account master,
                Account[] followers,
                bool enabled,
                int[] multipliers,
                Dictionary<string, FollowerAtmMode> atmTemplates,
                int tightenTicks,
                string[] followerAccountNames // NEW B127: 8th param
            )
            {
                Instrument = instrument;
                MasterAccount = master;
                FollowerAccounts = followers;
                Enabled = enabled;
                FollowerMultipliers = multipliers;
                FollowerAtmTemplates = atmTemplates ?? new Dictionary<string, FollowerAtmMode>();
                TightenTicks = tightenTicks > 0 ? tightenTicks : 5;
                // B127: derive names from accounts when not supplied explicitly (backward compat).
                // DtoToRule supplies explicit names (covering null-account slots).
                // All other callers pass null -- names are derived from resolved Account references.
                FollowerAccountNames = followerAccountNames ?? DeriveFollowerNames(followers);
            }

            // B8 T1: updated factory -- new optional params preserve backward compat with all existing tests
            // B10 T3: adds tightenTicks optional param (default 5)
            // B127: adds followerAccountNames optional param (default null = derive from followers[])
            internal static CopyRule Create(
                string instrument,
                Account master,
                Account[] followers,
                bool enabled = true,
                int[] multipliers = null,
                Dictionary<string, FollowerAtmMode> atmTemplates = null,
                int tightenTicks = 5,
                string[] followerAccountNames = null // NEW B127: 8th optional param; null = derive in ctor
            ) =>
                new CopyRule(
                    instrument,
                    master,
                    followers,
                    enabled,
                    multipliers,
                    atmTemplates ?? new Dictionary<string, FollowerAtmMode>(),
                    tightenTicks,
                    followerAccountNames // passed through; null triggers DeriveFollowerNames in ctor
                );

            // B127: derives follower name strings from Account[] for backward-compat callers.
            // Returns empty array for null/empty input. Never returns null (JS-002 convention).
            // CYC=2: null/length guard (1) + for loop (1).
            // JS-021: no lock. JS-001: no throw. ASCII-only.
            private static string[] DeriveFollowerNames(Account[] followers)
            {
                if (followers == null || followers.Length == 0)
                    return Array.Empty<string>();
                var names = new string[followers.Length];
                for (int i = 0; i < followers.Length; i++)
                    names[i] = followers[i]?.Name ?? string.Empty;
                return names;
            }
        }

        private readonly struct CopySignal
        {
            internal readonly OrderAction Action;
            internal readonly OrderType Type;
            internal readonly int Quantity;
            internal readonly double LimitPrice;
            internal readonly string OrderId;

            private CopySignal(
                OrderAction action,
                OrderType type,
                int qty,
                double limitPrice,
                string orderId
            )
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
            SaveRules(); // DW-B98-A: persist enabled state immediately so F5 restores it
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

        // B9 T1: CYC=1 -- straight-line assignment; BGTM-1: AtrSizing gate CYC=2.
        internal void SetAtrEngine(AtrSizingEngine engine, bool enabled)
        {
            if (!_flags.AtrSizing && enabled)
                enabled = false;
            _atrEngine = engine;
            _atrEnabled = enabled;
        }

        // B12 T3 -- UpdateMaxRisk: pass-through to _atrEngine. Null-guarded. CYC=2.
        internal void UpdateMaxRisk(double maxRiskDollars)
        {
            if (_atrEngine == null)
                return; // (1)
            _atrEngine.UpdateMaxRisk(maxRiskDollars); // (2)
        }

        // B12 T3 -- UpdateAtrFraction: pass-through to _atrEngine.SetAtrFraction. Null-guarded. CYC=2.
        internal void UpdateAtrFraction(double fraction)
        {
            if (_atrEngine == null)
                return; // (1)
            _atrEngine.SetAtrFraction(fraction); // (2)
        }

        // B9 T3: CYC=1 -- straight-line cast and assign; BGTM-1: MirrorMode gate CYC=2.
        internal void SetCopyMode(CopyMode mode)
        {
            if (!_flags.MirrorMode && mode == CopyMode.Mirror)
            {
                StatusUpdate?.Invoke("Mirror mode requires Elite tier");
                return;
            }
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
        // B68 DW-B68-01: CancelQxBrackets added before SubmitBeStop -- clears stale ATM brackets
        //   (Stop1/Stop2/Target1/Target2) on each account before the new BE stop is placed.
        //   No new McCabe branch: the cancel is a void call in the loop body, not an if-branch.
        // CYC=2 (unchanged: 1 base + 1 foreach branch). JS-021: no lock. JS-002: void. JS-033: synchronous.
        public void RelayBe(BeEventArgs e)
        {
            foreach (var acc in AllAccounts(e.Instrument))
            {
                NinjaTrader.Code.Output.Process(
                    "[BE] RelayBe: "
                        + acc.Name
                        + " @ "
                        + e.BePrice.ToString("F2")
                        + " isLong="
                        + e.IsLong,
                    NinjaTrader.NinjaScript.PrintTo.OutputTab1
                );
                CancelQxBrackets(acc, e.Instrument);
                SubmitBeStop(acc, e.Instrument, e.BePrice, e.IsLong);
            }
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

        // B50 -- SetCloneAtmCache: CYC=1. Stores ATM template name string for display/logging only.
        // Called from TradeCopierPanel.OnCloneModeClick alongside SetCloneAtmObjectCache.
        // JS-023: volatile string write is atomic.
        internal void SetCloneAtmCache(string value)
        {
            _cloneAtmCache = value ?? string.Empty;
        }

        // HOTFIX-B66-ATM-OBJ: SetCloneAtmObjectCache -- stores live AtmStrategy object for Clone dispatch.
        // Called from TradeCopierPanel.OnCloneModeClick after FindVisualChild<ChartTrader>.AtmStrategy.
        // JS-023: volatile reference write is atomic on CLR 4.0+.
        // CYC=1. JS-001: no throw. JS-002: null is valid (means None selected).
        internal void SetCloneAtmObjectCache(NinjaTrader.NinjaScript.AtmStrategy atmObj)
        {
            _cloneAtmObject = atmObj;
        }

        // B50 -- GetCloneAtmMode: CYC=2. Returns Named(obj) if object cached, Named(string) if string only, else Inherit.
        // Primary: use _cloneAtmObject (live AtmStrategy) so StartAtmStrategy(atm,order) overload is used.
        // Fallback: _cloneAtmCache string (for non-Clone-via-ChartTrader scenarios).
        // JS-002: never returns null -- returns Inherit as fallback.
        internal FollowerAtmMode GetCloneAtmMode()
        {
            var atmObj = _cloneAtmObject;
            if (atmObj != null) // branch (1) -- preferred: object overload
                return new FollowerAtmMode.Named(_cloneAtmCache, atmObj);
            var cache = _cloneAtmCache;
            if (cache != null && cache.Length > 0) // branch (2) -- fallback: string overload
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

        // HOTFIX-B67-CHECKBOX-RESTORE: returns saved follower account names for a given instrument+master.
        // Called from TradeCopierPanel.OnLoaded to restore IsSelected checkboxes after NT8 restart.
        // CYC=2: foreach rules(1) + foreach followers(2). JS-021: no lock. JS-002: returns empty set not null.
        internal HashSet<string> GetSavedFollowerNames(string instrument, string masterName)
        {
            var result = new HashSet<string>();
            foreach (var rule in _rules)
            {
                if (rule.Instrument != instrument || rule.MasterAccount?.Name != masterName)
                    continue;
                foreach (var f in rule.FollowerAccounts)
                    if (f?.Name != null)
                        result.Add(f.Name);
            }
            return result;
        }

        // --- B56 BUILD-FIX stubs (pre-existing callers referenced these before they were added) ---

        // IsFollowerAccount: returns true if acc is a follower in any rule.
        // Called by PttBreakEven + PttGlobalQuickExit to skip follower accounts.
        // CYC=8: null guard(1) + foreach(2) + for-i(3) + f-not-null(4) +
        //        f-null(5) + names-not-null(6) + i-in-range(7) + name-match(8).
        // JS-021: no lock. B121: null-slot fallback to FollowerAccountNames[i].
        internal bool IsFollowerAccount(Account acc)
        {
            if (acc == null)
                return false;
            foreach (var rule in _rules)
                for (int i = 0; i < rule.FollowerAccounts.Length; i++)
                {
                    var f = rule.FollowerAccounts[i];
                    if (f != null && f.Name == acc.Name)
                        return true;
                    if (
                        f == null
                        && rule.FollowerAccountNames != null
                        && i < rule.FollowerAccountNames.Length
                        && rule.FollowerAccountNames[i] == acc.Name
                    )
                        return true;
                }
            return false;
        }

        // GetQuickTicksForInstrument: returns (t1,t2) quick-exit tick defaults for an instrument.
        // Delegates to InstrumentDefaults -- rule-specific overrides deferred to future block.
        // CYC=2: null guard(1) + delegate(2). JS-002: returns tuple (not null).
        internal (int t1, int t2) GetQuickTicksForInstrument(NinjaTrader.Cbi.Instrument instr)
        {
            if (instr == null)
                return (4, 8); // (1)
            return InstrumentDefaults.GetQuickTicks( // (2)
                instr.MasterInstrument?.Name ?? string.Empty
            );
        }

        // IsAtmBracketName: true if name is a standard NT8 ATM bracket order name.
        // NT8-REF: NT8_FULL_REFERENCE.md line 1631: "The order name such as 'Stop1' or 'Target2'"
        // HOTFIX-ATM-T3-CANCEL-01: widened from hardcoded 4 names to generic Stop1-Stop9 / Target1-Target9.
        // Hardcoded Stop1/Stop2/Target1/Target2 missed Stop3..Stop9 and Target3..Target9 for
        // ATM strategies with 3+ targets (e.g. "MES $200 SL6" with 3 targets).
        // CYC=1: expression body -- Roslyn counts the compound bool expression as 1 decision point.
        // JS-021: no lock. JS-001: no throw. ASCII-only string literals.
        internal static bool IsAtmBracketName(string name) =>
            !string.IsNullOrEmpty(name)
            && (
                (
                    name.StartsWith("Stop", StringComparison.Ordinal)
                    && name.Length > 4
                    && char.IsDigit(name[4])
                )
                || (
                    name.StartsWith("Target", StringComparison.Ordinal)
                    && name.Length > 6
                    && char.IsDigit(name[6])
                )
            );

        // IsBeDisarmCandidate: CYC=4. Returns true when order is a PTT-BE-Stop fill on a non-null instrument.
        // Called by TryFireFollowerBeDisarm to guard the leader-check loop.
        // JS-001: no throw. JS-002: returns bool. JS-021: no lock. TESTABILITY: internal static.
        internal static bool IsBeDisarmCandidate(Order order)
        {
            if (order == null)
                return false; // (1)
            if (order.OrderState != OrderState.Filled)
                return false; // (2)
            if (
                order.Name == null
                || !order.Name.StartsWith("PTT-BE-Stop", StringComparison.Ordinal)
            )
                return false; // (3)
            return order.Instrument?.FullName != null; // (4)
        }

        // IsPttEntryOrderCancelTrigger: CYC=3. Returns true when a follower entry cancel should trigger re-place.
        // Matches HOTFIX-B66-COPY-REPLACE + HOTFIX-B66-NATIVE-ATM: "PTT-Copy" or "Entry" + Cancelled + LimitPrice>0.
        // Called from pre-Gate-1 block in OnOrderUpdate.
        // JS-001: no throw. JS-002: returns bool (never null). JS-021: no lock. ASCII-only.
        // TESTABILITY: internal static, Order parameter, no NT8 runtime deps beyond field reads.
        internal static bool IsPttEntryOrderCancelTrigger(Order order)
        {
            if (order == null)
                return false; // (1)
            if (order.OrderState != OrderState.Cancelled)
                return false; // (2)
            if (order.Name != "PTT-Copy" && order.Name != "Entry")
                return false; // (3)
            return order.LimitPrice > 0 && order.Instrument?.FullName != null;
        }

        // IsQxCancelCandidate: returns true if order should be cancelled by CancelQxBrackets.
        // Covers: ATM bracket names (via IsAtmBracketName), PTT-QX-* prefix, PTT-BE-* prefix,
        //         PTT-Copy* prefix (B70 DW-B70-02: follower copy-dispatched entry orders).
        // CYC=7: 1 (base) + 6 if-branches. JS-021: no lock. JS-001: no throw. ASCII-only.
        internal static bool IsQxCancelCandidate(Order o)
        {
            if (o == null || o.Name == null)
                return false; // (1)
            if (IsAtmBracketName(o.Name))
                return true; // (2)
            if (o.Name.StartsWith("PTT-QX-", StringComparison.Ordinal))
                return true; // (3)
            if (o.Name.StartsWith("PTT-BE-", StringComparison.Ordinal))
                return true; // (4)
            if (o.Name.StartsWith("PTT-Copy", StringComparison.Ordinal))
                return true; // (5) B70 DW-B70-02
            if (o.Name == "Entry")
                return true; // (6) DW-B93: Named ATM follower entry Limit
            return false;
        }

        // CancelQxBrackets: cancel all active ATM-bracket + PTT-* orders on acc for instr.
        // Called by PttQuickExit.Execute() before re-placing new bracket.
        // HOTFIX-QX-DOUBLE-01: Added TriggerPending -- ATM brackets spend time here before Submitted.
        //   NT8_FULL_REFERENCE.md line 946: TriggerPending = "Order is pending submission."
        //   Without this, clicking Quick All immediately after an ATM fill leaves bracket orders
        //   in TriggerPending state uncancelled -> new PTT-QX brackets stack on top -> double brackets.
        // CYC=6: null guard(1) + foreach(2) + stateOk(5 branches, Roslyn=1)(3) + instrument check(4) + IsQxCancelCandidate(5) + staleCount(6).
        // JS-021: no lock. Predicate logic in IsQxCancelCandidate (CYC=5) + IsAtmBracketName (CYC=1).
        internal void CancelQxBrackets(Account acc, NinjaTrader.Cbi.Instrument instr)
        {
            if (acc == null || instr == null)
                return; // (1)
            var stale = new System.Collections.Generic.List<Order>();
            foreach (Order o in acc.Orders) // (2)
            {
                bool stateOk =
                    o.OrderState == OrderState.Working
                    || o.OrderState == OrderState.Initialized
                    || o.OrderState == OrderState.Accepted
                    || o.OrderState == OrderState.Submitted // B71: catch ATM brackets placed <800ms ago
                    || o.OrderState == OrderState.TriggerPending; // HOTFIX-QX-DOUBLE-01: pre-submit state
                if (!stateOk)
                    continue; // (3)
                if (o.Instrument == null || o.Instrument.FullName != instr.FullName)
                    continue;
                if (IsQxCancelCandidate(o)) // (5) widened via helper
                    stale.Add(o);
            }
            if (stale.Count == 0)
                return;
            stale.RemoveAll(o =>
                o.OrderState == OrderState.Filled || o.OrderState == OrderState.Cancelled
            ); // DW-B79-09: race guard
            try
            {
                acc.Cancel(stale.ToArray());
            }
            catch { }
        }

        // B77 DW-B77-01: BuildQxSnapshot -- capture point-in-time set of cancellable QX orders.
        // Called by PttQuickExit.Execute() BEFORE CancelQxBrackets to record which orders existed
        // at snapshot time. Only orders in this set may be cancelled by the 3-param overload.
        // Prevents the race window where newly-submitted PTT-QX orders (from the Submit loop) are
        // caught by a second CancelQxBrackets call that was queued before the Submit loop ran.
        // CYC=4: null-guard(1) + foreach(2) + stateOk-and-instrument(3) + IsQxCancelCandidate(4).
        // JS-021: no lock. HashSet<Order> is local; NT8 dispatcher is serial (single-threaded dispatch).
        // JS-002: returns new empty HashSet<Order>() on null input -- never returns null.
        // JS-001: no throw. JS-033: synchronous static. ASCII-only.
        internal static System.Collections.Generic.HashSet<NinjaTrader.Cbi.Order> BuildQxSnapshot(
            NinjaTrader.Cbi.Account acc,
            NinjaTrader.Cbi.Instrument instr
        )
        {
            if (acc == null || instr == null) // (1)
                return new System.Collections.Generic.HashSet<NinjaTrader.Cbi.Order>(); // never null -- JS-002
            var result = new System.Collections.Generic.HashSet<NinjaTrader.Cbi.Order>();
            foreach (Order o in acc.Orders) // (2)
            {
                bool stateOk =
                    o.OrderState == OrderState.Working
                    || o.OrderState == OrderState.Initialized
                    || o.OrderState == OrderState.Accepted
                    || o.OrderState == OrderState.Submitted
                    || o.OrderState == OrderState.TriggerPending;
                if (!stateOk)
                    continue; // (3)
                if (o.Instrument == null || o.Instrument.FullName != instr.FullName)
                    continue;
                if (IsQxCancelCandidate(o)) // (4)
                    result.Add(o);
            }
            NinjaTrader.Code.Output.Process(
                "[PTT-QX] snapshot: " + result.Count + " cancellable orders for " + instr.FullName,
                NinjaTrader.NinjaScript.PrintTo.OutputTab1
            );
            return result;
        }

        // B77 DW-B77-02: CancelQxBrackets 3-param overload -- snapshot-gated cancel.
        // Identical to the 2-param overload except: an order is only added to stale if it
        // is contained in snapshot. Orders not in snapshot (submitted after snapshot was
        // taken = this cycle's new orders) are skipped, preventing the race window.
        // snapshot == null fallback: behaves identically to the 2-param overload (cancels all).
        // CYC=7: null-guard(1) + foreach(2) + stateOk(3) + instrument-filter(4) + snapshot-filter(5)
        //         + IsQxCancelCandidate(6) + stale-count(7). Budget <= 8. PASS.
        // JS-021: no lock. HashSet<Order> passed by reference, consumed synchronously on caller thread.
        // JS-001: no throw. JS-002: void return. JS-033: synchronous void. ASCII-only.
        internal void CancelQxBrackets(
            NinjaTrader.Cbi.Account acc,
            NinjaTrader.Cbi.Instrument instr,
            System.Collections.Generic.HashSet<NinjaTrader.Cbi.Order> snapshot
        )
        {
            if (acc == null || instr == null)
                return; // (1)
            var stale = new System.Collections.Generic.List<Order>();
            int raceSkipped = 0;
            foreach (Order o in acc.Orders) // (2)
            {
                bool stateOk =
                    o.OrderState == OrderState.Working
                    || o.OrderState == OrderState.Initialized
                    || o.OrderState == OrderState.Accepted
                    || o.OrderState == OrderState.Submitted
                    || o.OrderState == OrderState.TriggerPending;
                if (!stateOk)
                    continue; // (3)
                if (o.Instrument == null || o.Instrument.FullName != instr.FullName)
                    continue; // (4)
                if (snapshot != null && !snapshot.Contains(o))
                {
                    raceSkipped++;
                    continue;
                } // (5)
                if (IsQxCancelCandidate(o)) // (6)
                    stale.Add(o);
            }
            NinjaTrader.Code.Output.Process(
                "[PTT-QX] cancel: "
                    + stale.Count
                    + " queued, "
                    + raceSkipped
                    + " race-skipped on "
                    + acc.Name,
                NinjaTrader.NinjaScript.PrintTo.OutputTab1
            );
            if (stale.Count == 0)
                return; // (7)
            stale.RemoveAll(o =>
                o.OrderState == OrderState.Filled || o.OrderState == OrderState.Cancelled
            ); // DW-B79-09: race guard
            try
            {
                acc.Cancel(stale.ToArray());
            }
            catch { }
        }

        // B69 DW-B69-01: CancelAllAccountOrders -- cancel every active order on acc for instr
        // before submitting a market flatten. No name filter -- all order names cancelled.
        // NT8 precedent: @2Custom-0909edcc EmergencyFlattenSingleFleetAccount [938-EF-GUARD]:
        //   "Step 1: Cancel ALL working orders on this instrument for this account."
        //   States: Working|Submitted|Accepted|ChangePending.
        // CYC=4: null-guard(1) + foreach(2) + stateOk-4terms(3) + instrument-name(4). JS-021: no lock.
        // JS-001: no throw. JS-002: void. ASCII-only.
        internal void CancelAllAccountOrders(Account acc, NinjaTrader.Cbi.Instrument instr)
        {
            if (acc == null || instr == null)
                return; // (1)
            var toCancel = new System.Collections.Generic.List<Order>();
            foreach (Order o in acc.Orders) // (2)
            {
                bool stateOk =
                    o.OrderState == OrderState.Working
                    || o.OrderState == OrderState.Initialized
                    || o.OrderState == OrderState.Submitted
                    || o.OrderState == OrderState.Accepted;
                if (!stateOk)
                    continue; // (3)
                if (o.Instrument == null || o.Instrument.FullName != instr.FullName)
                    continue; // (4)
                toCancel.Add(o);
            }
            // DW-B79-04: belt-and-suspenders race guard -- discard orders that
            // transitioned to terminal state between snapshot and cancel call.
            toCancel.RemoveAll(o =>
                o.OrderState == OrderState.Filled || o.OrderState == OrderState.Cancelled
            );
            if (toCancel.Count == 0)
                return;
            try
            {
                acc.Cancel(toCancel);
            }
            catch { }
        }

        // B68 DW-B68-01: CancelQxBracketsForFollowers -- cancel stale brackets on all followers.
        // Called by PttGlobalQuickExit.Execute before placing new PTT-QX-* orders on the leader.
        // Ensures follower ATM brackets (Stop1/Stop2/Target1/Target2) and prior PTT-QX-*/PTT-BE-*
        // orders do not persist as stale orders alongside new QX bracket pairs.
        // CYC=5: instr-null-guard(1) + rule-null-guard(2) + foreach(3) + acc-null-guard(4) + delegate(5).
        // JS-021: no lock. JS-001: no throw. JS-002: void. JS-033: synchronous void.
        // NT8-REF: Account.Cancel -- via CancelQxBrackets (existing, tested, line 462).
        internal void CancelQxBracketsForFollowers(NinjaTrader.Cbi.Instrument instr)
        {
            if (instr == null)
                return; // (1)
            var rule = FindRule(instr);
            if (rule == null)
                return; // (2)
            foreach (var acc in rule.Value.FollowerAccounts) // (3)
            {
                if (acc == null)
                    continue; // (4)
                CancelQxBrackets(acc, instr); // (5)
            }
        }

        // NextQxOcoId: monotonic OCO group ID for Quick Exit bracket pairs.
        // Uses Interlocked.Increment on _qxOcoSeq (thread-safe, no lock).
        // CYC=1: straight expression. JS-021: no lock -- Interlocked.
        // B70 DW-B70-01: seed with TickCount & 0x7FFF (0..32767) to avoid ID reuse on session reconnect.
        private int _qxOcoSeq = Environment.TickCount & 0x7FFF;

        internal string NextQxOcoId() =>
            "PTT-QX-" + System.Threading.Interlocked.Increment(ref _qxOcoSeq).ToString("D5");

        // B66 DW-B66-BE-01: SubmitBeStop -- submit a StopMarket order at bePrice for acc+instr.
        // FIX: isLong is now a parameter -- callers pass direction at their own snapshot-read time.
        // Removed: internal pos.MarketPosition re-read (was racing with NT8 position update lag --
        //   NT8_FULL_REFERENCE.md line 1721: "Changes to positions will not be reflected till at
        //   least the next OnBarUpdate() event after an order fill.").
        // B65 precedent: same race fixed in TryDispatchLeaderFlat (CopyEngine.cs lines 651-654).
        // CYC=7 (strict McCabe): null-guard(1) + pos-loop(2) + inner-if(3) + pos-null-guard(4)
        //         + ternary-dir(5) + if-order-null(6) + base(1) = 7. JS-021: no lock.
        // JS-001: no throw. JS-002: void. JS-033: synchronous void.
        // B69 DW-B69-02: pos-find uses FullName comparison (not reference equality).
        // NT8: same contract can exist as 2 different Instrument objects across account contexts.
        internal void SubmitBeStop(
            Account acc,
            NinjaTrader.Cbi.Instrument instr,
            double bePrice,
            bool isLong
        )
        {
            if (acc == null || instr == null)
                return; // (1)
            NinjaTrader.Cbi.Position pos = null;
            foreach (NinjaTrader.Cbi.Position p in acc.Positions) // (2)
                if (
                    p.Instrument != null // (3)
                    && p.Instrument.FullName == instr.FullName
                )
                {
                    pos = p;
                    break;
                }
            if (pos == null || pos.Quantity == 0)
                return; // (4)
            OrderAction dir = isLong ? OrderAction.Sell : OrderAction.BuyToCover; // (5)
            try // (6) CreateOrder call
            {
                var order = acc.CreateOrder(
                    instr,
                    dir,
                    OrderType.StopMarket,
                    OrderEntry.Manual,
                    TimeInForce.Gtc,
                    pos.Quantity,
                    0,
                    bePrice,
                    string.Empty,
                    "PTT-BE-Stop",
                    DateTime.MaxValue,
                    (NinjaTrader.Cbi.CustomOrder)null
                );
                if (order != null) // (6) inner if
                {
                    acc.Submit(new[] { order });
                    NinjaTrader.Code.Output.Process(
                        "[BE] SubmitBeStop: "
                            + dir
                            + " "
                            + pos.Quantity
                            + " @ "
                            + bePrice.ToString("F2")
                            + " on "
                            + acc.Name,
                        NinjaTrader.NinjaScript.PrintTo.OutputTab1
                    );
                }
            }
            catch { }
        }

        // ArmAllPendingBe: arm pending break-even watcher for all non-follower accounts.
        // Called by PttGlobalBreakEven.Execute(int bufferTicks).
        // HOTFIX-BE-ALL-01: was calling SubmitBeStop (immediate new order, skipped followers,
        // never wrote to _pendingBeSlots so IsPendingSlotsEmpty stayed true and panel stayed purple).
        // Fix: delegate to ArmPendingBe -- same path as per-chart BE button.
        // When the pending trigger fires it calls BreakEven(Account,...) which fans out to
        // followers via AllAccounts() -> MoveStopToBreakEven. No SubmitBeStop needed here.
        // CYC=3: foreach(1) + follower skip(2) + delegate(3). JS-021: no lock.
        internal void ArmAllPendingBe(int bufferTicks)
        {
            foreach (Account acc in Account.All) // (1)
            {
                if (IsFollowerAccount(acc))
                    continue; // (2) skip followers
                foreach (NinjaTrader.Cbi.Position pos in acc.Positions) // (3)
                {
                    if (pos == null || pos.Quantity == 0)
                        continue; // (4) skip flat
                    ArmPendingBe(pos.Instrument, acc, bufferTicks); // (5) HOTFIX-BE-ALL-01
                }
            }
        }

        // --- end B56 BUILD-FIX stubs ---

        // B9 T1: CYC=2 -- returns engine value when enabled; 1 otherwise. BGTM-1: AtrSizing gate CYC=3.
        internal int GetSuggestedQty(NinjaTrader.Cbi.Instrument instrument)
        {
            if (!_flags.AtrSizing)
                return 1;
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
                        ? CopyRule.Create(
                            r.Instrument,
                            r.MasterAccount,
                            r.FollowerAccounts,
                            enabled,
                            r.FollowerMultipliers,
                            r.FollowerAtmTemplates,
                            r.TightenTicks,
                            r.FollowerAccountNames // B127: preserve names through enabled/disabled rebuild
                        )
                        : r;
                _rules.Add(updated);
            }
        }

        // Original 3-arg overload -- PRESERVED UNCHANGED (backward compat with all 27 existing tests)
        // BGTM-1: MultiRule gate CYC=2.
        internal void AddRule(string instrument, Account master, Account[] followers)
        {
            if (!_flags.MultiRule && _rules.Count >= 1)
            {
                StatusUpdate?.Invoke(
                    "Multi-rule requires Pro. Upgrade at proptradertools.com/pricing"
                );
                return;
            }
            _rules.Add(CopyRule.Create(instrument, master, followers));
        }

        // B8 T1: new 5-arg overload -- adds multipliers + ATM map at apply time
        // B23 T1 (DW-B22-ADDRULE-ACCUMULATE-01): replace-not-append for same (instrument, leader).
        // ConcurrentBag rebuild pattern -- no lock (JS-021). Same pattern as SetFollowerMultiplier.
        // CYC=4: foreach(1) + string == (2) + name == (3) + continue(4 -- implicit else branch). BGTM-1: MultiRule gate CYC=5.
        internal void AddRule(
            string instrument,
            Account master,
            Account[] followers,
            int[] multipliers,
            Dictionary<string, FollowerAtmMode> atmMap
        )
        {
            if (!_flags.MultiRule && _rules.Count >= 1)
            {
                StatusUpdate?.Invoke(
                    "Multi-rule requires Pro. Upgrade at proptradertools.com/pricing"
                );
                return;
            }
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
                var newMults = BuildUpdatedMultipliers(
                    r.FollowerMultipliers,
                    followerIndex,
                    clamped,
                    r.FollowerAccounts?.Length ?? 0
                );
                _rules.Add(
                    CopyRule.Create(
                        r.Instrument,
                        r.MasterAccount,
                        r.FollowerAccounts,
                        r.Enabled,
                        newMults,
                        r.FollowerAtmTemplates,
                        r.TightenTicks,
                        r.FollowerAccountNames // B127: preserve names through multiplier rebuild
                    )
                );
            }
        }

        // Helper for SetFollowerMultiplier -- builds a new multiplier array with one entry updated.
        // CYC=3 (null guard + bounds guard + copy loop). No throw, no return null.
        private static int[] BuildUpdatedMultipliers(
            int[] existing,
            int index,
            int value,
            int count
        )
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

        // --- Hot path: CYC=8 (B75-LaneA second pass). All sub-blocks extracted to helpers. ---
        private void OnOrderUpdate(object sender, OrderEventArgs e)
        {
            // B62: evict dedup on terminal states so orderId is not permanently blocked.
            EvictDedup(e.Order.OrderId.ToString(), e.Order.OrderState);
            TryLogDragTrace(e.Order);

            // HOTFIX-FLAT-DISARM-FOLLOWER: extracted to TryFireFollowerBeDisarm (CYC=8).
            // Fires PositionStateChanged when a follower PTT-BE-Stop fills. JS-021: no lock.
            TryFireFollowerBeDisarm(e);

            // DW-B79-06: event-driven BE retry -- fires MoveStopToBreakEven the instant a
            // PTT-QX-T* order goes Working on a follower with a pending BE slot. Zero timing.
            TryFireFollowerBeRetry(e);

            // DW-B79-06: evict stale BE retry slot when follower position closes via any path.
            TryEvictFollowerBeSlot(e);

            // B135 DW-B134-OCO: sweep orphaned PTT-drag orders when follower position goes flat.
            TrySweptPttDragOrphans(e);

            // DW-B79-08: PTT-BE bracket wipe recovery.
            // Root cause confirmed 2026-08-19: when leader re-enters after QX->BE-ALL, NT8's
            // StartAtmStrategy sweep cancels ALL follower working orders -- including PTT-BE-* brackets.
            // Recovery: on PTT-BE-Stop-* cancel with follower position still open, re-call
            // MoveStopToBreakEven(isRetry:true) to re-place the OCO pairs.
            // Only triggers on PTT-BE-Stop-* (not PTT-BE-Target-* which correctly cancels on stop fill).
            // One re-call per OCO pair (Stop cancel = one trigger per pair).
            // DW-B92: record PTT-BE-Target-* fill BEFORE OCO partner cancel arrives.
            if (
                e.Order.OrderState == OrderState.Filled
                && e.Order.Name != null
                && e.Order.Name.StartsWith("PTT-BE-Target-", StringComparison.Ordinal)
                && e.Order.Account != null
            )
            {
                _filledBeTargetCount.AddOrUpdate(e.Order.Account.Name, 1, (_, prev) => prev + 1);
            }

            if (
                e.Order.OrderState == OrderState.Cancelled
                && e.Order.Name != null
                && e.Order.Name.StartsWith("PTT-BE-", StringComparison.Ordinal)
            )
            {
                NinjaTrader.Code.Output.Process(
                    "[BE-DIAG-CANCEL] "
                        + (e.Order.Account?.Name ?? "?")
                        + " PTT-BE order cancelled: "
                        + e.Order.Name
                        + " instr="
                        + (e.Order.Instrument?.FullName ?? "?"),
                    NinjaTrader.NinjaScript.PrintTo.OutputTab1
                );
                if (
                    e.Order.Name.StartsWith("PTT-BE-Stop-", StringComparison.Ordinal)
                    && !HasFilledBeTargetFast(e.Order.Account)
                ) // DW-B90: skip if OCO cancel from target fill; DW-B92: race-free counter
                    TryReplacePttBeBrackets(e.Order);
            }

            // B113 DW-B117: cancel-after -- cancel each native ATM bracket one-for-one
            // as the corresponding PTT-QX-T* order confirms Working. Extracted to helper
            // to keep OnOrderUpdate CYC within budget.
            TryCleanupReArmedAtmBracket(e);

            // HOTFIX-B66-COPY-REPLACE / HOTFIX-B66-NATIVE-ATM: re-place follower entry when NT8-ATM
            // cancel sweep wipes it during bracket arming. Predicate logic in IsPttEntryOrderCancelTrigger.
            if (IsPttEntryOrderCancelTrigger(e.Order))
                ReplaceFollowerCopyOnAtmCancel(e.Order);

            // Gate 1: enabled check
            if (!_isCopyEnabled)
                return;

            // Gate 2: find matching rule -- instrument AND master account must match.
            // Extracted to FindMatchingRule (CYC=3).
            CopyRule? matchedRule = FindMatchingRule(e.Order);

            // Gate 2 + 2.5: combined null/disabled check (single McCabe point).
            if (matchedRule == null || !matchedRule.Value.Enabled)
                return;

            // BUG-BE-RESET fix: fire position state ONLY for leader account+instrument orders.
            TryFirePositionState(e);

            // B9 T3 -- Mirror mode relay (inserted after Gate 2.5, before Gate B)
            if ((CopyMode)_copyModeValue == CopyMode.Mirror)
                MirrorOrderUpdate(e.Order, matchedRule.Value);

            // B56 T1: propagate leader cancel to follower entry orders.
            // Extracted to TryCancelFollowerEntries (CYC=4). Includes HOTFIX-B63-COPY-CANCEL-01 guard.
            if (TryCancelFollowerEntries(e.Order, matchedRule.Value))
                return;

            // DW-B60-01: leader went flat -- propagate close to followers
            if (
                TryDispatchLeaderFlat(
                    e.Order.Account,
                    e.Order.Instrument,
                    e.Order.OrderState,
                    e.Order.Name,
                    matchedRule.Value,
                    IsFollowerAccount,
                    HasOpenPosition,
                    FlattenOneAccount
                )
            )
                return;

            // Gate B+C: bracket drag then entry drag -- consolidated into TryHandleDrag (one branch here).
            if (TryHandleDrag(e.Order, matchedRule.Value))
                return;

            // No bracket, no drag -- normal copy dispatch
            DispatchCopy(e.Order, matchedRule.Value);
        }

        // TryFireFollowerBeDisarm: CYC=4. Fires PositionStateChanged when a follower PTT-BE-Stop fills.
        // Called from pre-Gate-1 in OnOrderUpdate. HOTFIX-FLAT-DISARM-FOLLOWER.
        // Guards delegated to IsBeDisarmCandidate (CYC=4) to keep total CYC<=8 per method.
        // JS-021: no lock. JS-001: no throw. JS-002: void.
        private void TryFireFollowerBeDisarm(OrderEventArgs e)
        {
            if (!IsBeDisarmCandidate(e.Order))
                return; // (1)

            bool isLeader = false;
            foreach (var r in _rules) // (2)
            {
                if (e.Order.Account.Name == r.MasterAccount?.Name)
                {
                    isLeader = true;
                    break;
                } // (3)
            }
            if (!isLeader) // (4)
            {
                // Follower PTT-BE stop filled -- fire position state so panel resets BE visual.
                bool hasPos = HasOpenPosition(e.Order.Account, e.Order.Instrument);
                bool hasEntries = HasWorkingEntries(e.Order.Account, e.Order.Instrument);
                PositionStateChanged?.Invoke(
                    e.Order.Instrument.FullName,
                    new PositionState(hasPos, hasEntries)
                );
            }
        }

        // TryFireFollowerBeRetry: CYC=6. DW-B79-06 event-driven BE retry.
        // Fires MoveStopToBreakEven exactly once when a trigger order transitions to Working
        // on a follower account that registered a _pendingFollowerBeSlots entry.
        //
        // Trigger names (guard 2):
        //   PTT-QX-T* -- QX path: QX targets going Working after QX->BE-ALL sequence.
        //   Target1..Target9 -- DW-B79-08 v4: plain new-entry ATM targets going Working.
        //     Fires AFTER the new ATM has fully settled, at the correct stable moment --
        //     avoids MoveStopToBreakEven(isRetry) running acc.Cancel while sweep is active.
        //
        // DW-B82-01: reset _beReplaceAttempts on slot consumption.
        //   The counter was only reset in TryEvictFollowerBeSlot (position-close path), which
        //   races with the QX exit path. QX's 3 PTT-BE-Stop-* cancels exhaust the 3-attempt
        //   limit in one trade; all subsequent trades get "max 3 attempts" immediately and no
        //   brackets are ever placed. Fix: always reset when a slot is atomically claimed here.
        //
        // CYC=6: (1) null guard, (2a) PTT-QX-T prefix check, (2b) Target1..Target9 check,
        //        (3) state guard, (4) TryRemove atomic claim, (5) flat guard.
        // JS-021: ConcurrentDictionary ops are lock-free -- only one caller wins per slot.
        // JS-001: no throw. JS-002: void. ASCII-only.
        private void TryFireFollowerBeRetry(OrderEventArgs e)
        {
            var o = e?.Order;
            if (o == null || o.Name == null || o.Account == null)
                return; // (1)
            bool isPttQxT =
                o.Name.StartsWith("PTT-QX-T", StringComparison.Ordinal) // (2a)
                && o.Name.Length > 8
                && char.IsDigit(o.Name[8]);
            bool isAtmTgt =
                o.Name.StartsWith("Target", StringComparison.Ordinal) // (2b) DW-B79-08 v4
                && o.Name.Length > 6
                && char.IsDigit(o.Name[6]);
            if (!isPttQxT && !isAtmTgt)
                return;
            if (
                o.OrderState != OrderState.Working // (3)
                && o.OrderState != OrderState.Accepted
            )
                return;
            if (!_pendingFollowerBeSlots.TryRemove(o.Account.Name, out var slot)) // (4) atomic claim
                return;
            _beReplaceAttempts.TryRemove(o.Account.Name, out _); // DW-B82-01: reset on slot consumption
            if (IsFlat(FindPosition(slot.Account, slot.Instrument))) // (5)
                return;
            NinjaTrader.Code.Output.Process(
                "[BE-RETRY] "
                    + o.Account.Name
                    + " "
                    + o.Name
                    + " Working -- event-driven BE retry firing",
                NinjaTrader.NinjaScript.PrintTo.OutputTab1
            );
            MoveStopToBreakEven(slot.Account, slot.Instrument, slot.BufferTicks, isRetry: true);
        }

        // TryEvictFollowerBeSlot: CYC=6. DW-B79-06 stale-slot cleanup.
        // Clears _pendingFollowerBeSlots AND _beReplaceAttempts when follower slot is terminal.
        // DW-B79-08 v8: decouple attempt-counter reset from slot existence.
        //   v3 bug: guard (2) checked ContainsKey(_pendingFollowerBeSlots) before resetting
        //   _beReplaceAttempts. After the 500ms fallback timer consumed the slot via TryRemove,
        //   the slot was gone. On next re-entry, guard (2) returned early and _beReplaceAttempts
        //   stayed at 3 permanently -- blocking all recovery on subsequent trades.
        //   Fix: always reset _beReplaceAttempts when flat, regardless of slot existence.
        //   The slot eviction remains guarded (only remove if it exists), but the counter
        //   reset is unconditional on flat -- it costs nothing when counter is already 0.
        // DW-B81-01: also evict on PTT-BE-Stop Rejected.
        //   Root: when BE fires immediately after entry on a short, market can tick below entry
        //   before NT8 accepts the follower PTT-BE-Stop. NT8 rejects with "Buy stop below market".
        //   TryEvictFollowerBeSlot only fired on Filled -- leaving a stranded slot in
        //   _pendingFollowerBeSlots. Next BE press: TryAdd in TryReplacePttBeBrackets finds
        //   existing slot -> returns early -> NO bracket placed -> follower gets stop-only, no targets.
        //   Fix: evict slot AND reset attempt counter on Rejected for PTT-BE-Stop specifically.
        //   Flat-guard bypassed for Rejected: rejection can happen while position is still open
        //   (stop rejected but trade still live). The retry (500ms fallback) must be free to
        //   re-register a fresh slot.
        // CYC=6: null-guard(1) + terminal-state-guard(2) + name-guard(3) + follower-guard(4)
        //        + flat-guard-for-filled(5) + slotEvicted-gate(6). JS-021: no lock.
        // JS-021: ConcurrentDictionary ops are lock-free. JS-001: no throw. JS-002: void. ASCII.
        private void TryEvictFollowerBeSlot(OrderEventArgs e)
        {
            var o = e?.Order;
            if (o == null)
                return; // (1) null guard
            bool isFilled = o.OrderState == OrderState.Filled; // (2a)
            bool isRejected =
                o.OrderState == OrderState.Rejected // (2b) DW-B81-01
                && o.Name == "PTT-BE-Stop"; // (3) PTT-BE-Stop only
            if (!isFilled && !isRejected)
                return;
            _entryDispatchedOrders.Clear(); // DW-B95: instrument-level reset -- fires for ALL accounts (leader + follower)
            if (!IsFollowerAccount(o.Account))
                return; // (4) follower-only evictions below
            if (isFilled && !IsFlat(FindPosition(o.Account, o.Instrument)))
                return; // (5) flat-guard for Filled only
            string accName = o.Account?.Name ?? string.Empty;
            bool slotEvicted = _pendingFollowerBeSlots.TryRemove(accName, out _); // DW-B79-04: capture for log gate
            _beReplaceAttempts.TryRemove(accName, out _); // ALWAYS reset on terminal
            _filledBeTargetCount.TryRemove(accName, out _); // DW-B92: clear on flat
            if (slotEvicted) // DW-B79-04: only log if slot was present
            {
                string reason = isRejected ? "PTT-BE-Stop Rejected" : "position closed";
                NinjaTrader.Code.Output.Process(
                    "[BE-RETRY] "
                        + accName
                        + " "
                        + reason
                        + " -- evicted BE slot + reset attempt counter",
                    NinjaTrader.NinjaScript.PrintTo.OutputTab1
                );
            }
        }

        // B135 DW-B134-OCO: sweep orphaned PTT-drag orders when follower position goes flat.
        // PTT-TGT-Drag and PTT-STP-Drag are standalone (oco="") -- not in any NT8 ATM OCO group.
        // When ATM fills naturally, NT8 only cancels OCO-linked (green) orders; PTT-drag orders survive.
        // Fire on Filled + follower + flat -- same pattern as TryEvictFollowerBeSlot (L1538).
        // CYC=5: base(1) + o null guard(1) + Filled guard(1) + follower guard(1) + flat guard(1) = 5.
        // JS-021: no lock. JS-001: no throw. JS-002: void. ASCII-only.
        private void TrySweptPttDragOrphans(OrderEventArgs e)
        {
            var o = e?.Order;
            if (o == null) // (1)
                return;
            if (o.OrderState != OrderState.Filled) // (2)
                return;
            if (!IsFollowerAccount(o.Account)) // (3)
                return;
            if (!IsFlat(FindPosition(o.Account, o.Instrument))) // (4)
                return;
            CancelPttDragOrphansForAccount(o.Account, o.Instrument);
        }

        // B135 DW-B134-OCO: test seam -- delegates to TrySweptPttDragOrphans for xUnit test access.
        internal void TrySweptPttDragOrphansTestable(OrderEventArgs e) => TrySweptPttDragOrphans(e);

        // B135 DW-B134-OCO: cancel all Working PTT-TGT-Drag and PTT-STP-Drag orders for this account+instrument.
        // Called ONLY when position is confirmed flat (TrySweptPttDragOrphans gate).
        // acc.Orders.ToList() is safe in OnOrderUpdate callback thread (existing pattern: L2322).
        // try/catch: absorbs ErrorCode.UnableToCancelOrder (existing pattern: SyncAtmFollowerBracket L2259-2266).
        // CYC=5: base(1) + foreach(1) + state guard(1) + instr guard(1) + name guard(1) = 5.
        // JS-021: no lock. JS-001: try/catch -- no throw in hot path. JS-002: void. ASCII-only.
        // NT8-014: "PTT-TGT-Drag" confirmed L2362, "PTT-STP-Drag" confirmed L2281. acc.Cancel confirmed AddOnBase.
        private void CancelPttDragOrphansForAccount(Account acc, Instrument instr)
        {
            foreach (var o in acc.Orders.ToList()) // (1)
            {
                if (o.OrderState != OrderState.Working) // (2)
                    continue;
                if (o.Instrument?.FullName != instr?.FullName) // (3)
                    continue;
                if (o.Name != "PTT-TGT-Drag" && o.Name != "PTT-STP-Drag") // (4)
                    continue;
                try
                {
                    acc.Cancel(new Order[] { o });
                    StatusUpdate?.Invoke(acc.Name + ": PTT drag sweep: cancelled " + o.Name);
                }
                catch (Exception ex)
                {
                    StatusUpdate?.Invoke(acc.Name + ": PTT drag sweep cancel error: " + ex.Message);
                }
            }
        }

        // B135 DW-B134-OCO: test seam -- delegates to CancelPttDragOrphansForAccount for xUnit test access.
        internal void CancelPttDragOrphansForAccountTestable(Account acc, Instrument instr) =>
            CancelPttDragOrphansForAccount(acc, instr);

        // QueueBeRetryFallback: CYC=1. Configurable-delay DispatcherTimer fallback for the event-driven BE retry.
        // DW-B79-06/07: fires MoveStopToBreakEven(isRetry:true) if TryFireFollowerBeRetry missed
        // the trigger event (PTT-QX-T already Cancelled before slot was registered).
        // DW-B79-08 v6: delayMs parameter (default 200ms for QX path, 500ms for ATM-sweep path).
        //   200ms path (QX): event-driven missed because QX target already Cancelled before slot registered.
        //   500ms path (ATM sweep): new ATM brackets go Working BEFORE PTT-BE-Stop cancel arrives,
        //     so TryFireFollowerBeRetry sees no slot. 500ms fires after ATM fully settled.
        // DW-B79-08 v7: timer MUST be created on the WPF UI dispatcher thread.
        //   OnOrderUpdate fires on NT8's order-update background thread. A DispatcherTimer created
        //   on a background thread uses that thread's dispatcher which never pumps -- Tick never fires.
        //   Fix: wrap timer construction in Application.Current.Dispatcher.InvokeAsync so it runs
        //   on the WPF UI thread whose dispatcher pumps normally. Same pattern as RaiseBeBufferChanged.
        // TryRemove is the atomic claim gate: if the event-driven path already consumed the slot,
        // TryRemove returns false and this callback is a no-op. Exactly one path wins.
        // CYC=1: straight sequence (no branches in method body -- timer lambda is not a branch here).
        // JS-021: no lock. JS-001: no throw. JS-033: Tick is not async void. ASCII-only.
        private void QueueBeRetryFallback(
            Account acc,
            Instrument instrument,
            int bufferTicks,
            int delayMs = 200
        )
        {
            var capturedAcc = acc;
            var capturedInstr = instrument;
            var capturedBuf = bufferTicks;
            // DW-B79-08 v7: marshal onto WPF UI thread so DispatcherTimer.Tick actually fires.
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var timer = new System.Windows.Threading.DispatcherTimer(
                    System.Windows.Threading.DispatcherPriority.Background
                )
                {
                    Interval = System.TimeSpan.FromMilliseconds(delayMs),
                };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    if (_pendingFollowerBeSlots.TryRemove(capturedAcc.Name, out var slot))
                    {
                        bool flat = IsFlat(FindPosition(slot.Account, slot.Instrument));
                        NinjaTrader.Code.Output.Process(
                            "[BE-RETRY] "
                                + capturedAcc.Name
                                + " -- fallback timer fired, flat="
                                + flat,
                            NinjaTrader.NinjaScript.PrintTo.OutputTab1
                        );
                        if (!flat)
                            MoveStopToBreakEven(
                                slot.Account,
                                slot.Instrument,
                                slot.BufferTicks,
                                isRetry: true
                            );
                    }
                    else
                    {
                        NinjaTrader.Code.Output.Process(
                            "[BE-RETRY] "
                                + capturedAcc.Name
                                + " -- fallback TryRemove=false (slot already consumed or evicted)",
                            NinjaTrader.NinjaScript.PrintTo.OutputTab1
                        );
                    }
                };
                timer.Start();
            });
        }

        // FindMatchingRule: CYC=3. Finds the CopyRule whose Instrument and MasterAccount match the order.
        // Returns null if no rule matches. Called from Gate 2 in OnOrderUpdate.
        // JS-021: no lock. JS-002: null return is Option-style (caller guards immediately).
        private CopyRule? FindMatchingRule(Order order)
        {
            foreach (var rule in _rules)
            {
                if (
                    order.Instrument.FullName == rule.Instrument
                    && order.Account.Name == rule.MasterAccount?.Name
                )
                    return rule;
            }
            return null;
        }

        // TryCancelFollowerEntries: CYC=4 (was 6). Propagates leader cancel to scoped follower entry orders.
        // Returns true if Cancelled state was handled (caller should return immediately).
        // HOTFIX-B63-COPY-CANCEL-01: ATM bracket cancels are skipped via IsAtmBracketName guard.
        // DW-B103: PTT exit bracket OCO-cancels return false (do not wipe follower brackets).
        // DW-B136 Gap B: delegates to CancelScopedFollowerEntries (order-ID scoped, not instrument-scoped).
        // JS-021: no lock. JS-001: no throw.
        private bool TryCancelFollowerEntries(Order order, CopyRule rule)
        {
            if (order.OrderState != OrderState.Cancelled)
                return false;
            if (IsAtmBracketName(order.Name))
                return true; // HOTFIX-B63-COPY-CANCEL-01
            if (
                order.Name != null
                && (
                    order.Name.StartsWith("PTT-QX-", StringComparison.Ordinal)
                    || order.Name.StartsWith("PTT-BE-", StringComparison.Ordinal)
                )
            )
                return false; // DW-B103: OCO-cancel of PTT exit bracket must not wipe follower brackets
            // DW-B136 Gap B: scope cancel to specific leader order, not all instrument entries.
            // Single-entry best practice: one leader entry per instrument at a time is the supported
            // workflow. This fix prevents collateral cancel when the constraint is violated (two
            // simultaneous entries). The constraint documentation in the spec and UI tooltip is preserved.
            // Note: rule param is unused post-fix; preserved for call-site stability (one call site: L1361).
            CancelScopedFollowerEntries(order.OrderId.ToString());
            return true;
        }

        // DW-B136 Gap B: record follower Order under the leader orderId that triggered the copy.
        // Called from SendCopy and SendCopyWithAtm after follower.Submit (or StartAtmStrategy) succeeds.
        // Key: leaderOrderId (same string as in _dedupCache and _entryDispatchedOrders).
        // Value: ConcurrentBag<Order> -- thread-safe add, no lock().
        // CYC=1: no branches. JS-021: lock-free (ConcurrentDictionary.GetOrAdd + ConcurrentBag.Add).
        // JS-001: no throw. JS-002: void.
        internal void RecordFollowerCopy(string leaderOrderId, Order followerOrder)
        {
            var bag = _followerCopyMap.GetOrAdd(leaderOrderId, _ => new ConcurrentBag<Order>());
            bag.Add(followerOrder);
        }

        // DW-B136 Gap B: cancel only follower orders recorded under the given leader order ID.
        // Replaces the instrument-scoped sweep in TryCancelFollowerEntries (CancelOneAccount).
        // Called from TryCancelFollowerEntries AFTER EvictDedup has already fired in OnOrderUpdate
        // (L1277 vs L1361). The map entry for leaderOrderId must still be present at this point --
        // EvictDedup does NOT touch _followerCopyMap (see LaneB-02-architecture-plan.md Section 4d).
        // CYC=5:
        //   (1) TryGetValue miss guard
        //   (2) foreach bag
        //   (3) OrderState guard (Working || Initialized)
        //   (4) try body
        //   (5) catch
        // JS-021: no lock. JS-001: catch logs, no rethrow. JS-002: void.
        // NT8: fo.Account.Cancel(Order[]) valid from AddOn context (NT8_ADDON_KNOWLEDGE.md line 222).
        // Eviction: TryRemove called after loop -- sole eviction point on cancel path.
        internal void CancelScopedFollowerEntries(string leaderOrderId)
        {
            if (!_followerCopyMap.TryGetValue(leaderOrderId, out var bag)) // (1)
                return;
            foreach (var fo in bag) // (2)
            {
                if ( // (3)
                    fo.OrderState != OrderState.Working
                    && fo.OrderState != OrderState.Initialized
                )
                    continue;
                try // (4)
                {
                    fo.Account.Cancel(new Order[] { fo });
                    StatusUpdate?.Invoke(
                        fo.Account.Name + ": scoped cancel orderId=" + leaderOrderId
                    );
                }
                catch (Exception ex) // (5)
                {
                    StatusUpdate?.Invoke("PTT-ScopedCancel error: " + ex.Message);
                }
            }
            _followerCopyMap.TryRemove(leaderOrderId, out _); // DW-B136 Gap B: evict after use (sole eviction point)
        }

        // TryHandleBracketDrag: CYC=3. Gate B bracket drag detection -- diverts to HandleBracketChange.
        // Returns true if handled (caller should return immediately).
        // JS-021: no lock. JS-001: no throw.
        private bool TryHandleBracketDrag(Order order, CopyRule rule)
        {
            if (_diagnosticMode)
                NinjaTrader.Code.Output.Process(
                    "[TP2-DRAG] IsWorkingBracket="
                        + IsWorkingBracket(order)
                        + " name="
                        + (order.Name ?? "null")
                        + " state="
                        + order.OrderState,
                    NinjaTrader.NinjaScript.PrintTo.OutputTab1
                );
            if (!IsWorkingBracket(order))
                return false;
            if (order.FromEntrySignal != null)
                PopulateOrderMap(order.FromEntrySignal, order.Account);
            HandleBracketChange(order, rule);
            return true;
        }

        // B132 LaneB diagnostic. Set _diagnosticMode=false to disable. Remove when DW-B138 confirmed fixed.
        // CYC=4: (1) if-guard, (2) &&, (3) ||.
        // JS-021: no lock. JS-001: no throw. NT8 Output.Process is safe from any thread.
        private void TryLogDragTrace(Order order)
        {
            if (
                _diagnosticMode
                && (IsWorkingBracket(order) || order.OrderState == OrderState.ChangeSubmitted)
            )
                NinjaTrader.Code.Output.Process(
                    "[TP1-OOU] name="
                        + (order.Name ?? "null")
                        + " state="
                        + order.OrderState
                        + " signal="
                        + (order.FromEntrySignal ?? "null")
                        + " acct="
                        + (order.Account?.Name ?? "?"),
                    NinjaTrader.NinjaScript.PrintTo.OutputTab1
                );
        }

        // B132 LaneB diagnostic. Set _diagnosticMode=false to disable. Remove when DW-B138 confirmed fixed.
        // CYC=2: (1) if-guard.
        // JS-021: no lock. acc.Orders.ToList() is NT8-safe on order-update thread.
        private void TryLogSFBTrace(Account acc, Order leaderOrder, bool isStop, Order? fo)
        {
            if (!_diagnosticMode)
                return;
            var ordList = acc.Orders.ToList();
            NinjaTrader.Code.Output.Process(
                "[TP4-SFB] acc="
                    + acc.Name
                    + " leaderName="
                    + (leaderOrder.Name ?? "null")
                    + " isStop="
                    + isStop
                    + " fo="
                    + (fo?.Name ?? "NULL")
                    + " followerOrders=["
                    + string.Join(",", ordList.Select(o => (o.Name ?? "?") + ":" + o.OrderState))
                    + "]",
                NinjaTrader.NinjaScript.PrintTo.OutputTab1
            );
        }

        // TryHandleDrag: CYC=3. Combines bracket drag (Gate B) and entry drag (Gate C) into one dispatch.
        // Returns true if either gate consumed the event (caller should return).
        // Consolidates the two consecutive if-dispatch calls in OnOrderUpdate to one branch.
        // JS-021: no lock. JS-001: no throw. JS-002: returns bool.
        private bool TryHandleDrag(Order order, CopyRule rule)
        {
            if (TryHandleBracketDrag(order, rule))
                return true; // (1)
            if (TryHandleEntryDrag(order, rule))
                return true; // (2)
            return false;
        }

        // TryHandleEntryDrag: CYC=7. Gate C entry drag detection -- same orderId + new price = dragged.
        // Returns true if handled (caller should return immediately).
        // B62/B66-LaneC: accepts Limit and StopLimit. HOTFIX-B65-GATE-C-FILL-GUARD-01: Filled==0 guard.
        // DW-B64-01: re-inserts new price before HandleEntryChange removes old key.
        // JS-021: no lock. JS-001: no throw.
        private bool TryHandleEntryDrag(Order order, CopyRule rule)
        {
            if (order.OrderType != OrderType.Limit && order.OrderType != OrderType.StopLimit)
                return false;
            if (order.OrderState != OrderState.Accepted && order.OrderState != OrderState.Working)
                return false;
            if (order.Filled != 0)
                return false; // HOTFIX-B65-GATE-C-FILL-GUARD-01
            double currentPrice = GetOrderPrice(order);
            if (!_dedupCache.TryGetValue(order.OrderId.ToString(), out double storedPrice))
                return false;
            if (
                Math.Abs(currentPrice - storedPrice)
                < (order.Instrument?.MasterInstrument?.TickSize ?? 0.01)
            )
                return false;
            // DW-B64-01 fix: re-insert new price BEFORE HandleEntryChange removes old key.
            // HandleEntryChange calls TryRemove(orderId) -- without this line the second state
            // transition (Working after Accepted) sees no cache entry and falls through to
            // DispatchCopy, placing a duplicate PTT-Copy order on the follower.
            _dedupCache[order.OrderId.ToString()] = currentPrice;
            HandleEntryChange(order, rule);
            return true;
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
            if (masterOrder == null)
                return; // guard (1)
            bool isBracket = IsBracketLeg(masterOrder);
            if (ShouldMirrorClose(masterOrder.OrderState, isBracket)) // branch (2)
            {
                MirrorClose(masterOrder, rule);
                return;
            }
            if (IsWorkingBracket(masterOrder)) // branch (3)
                HandleBracketChange(masterOrder, rule); // reuse existing -- no duplication
        }

        // CYC=4 -- instr null guard + foreach loop + acc null guard + pos null/qty guard
        // JS-001: try/catch around CreateOrder -- no throw in hot path.
        // NT8 constraint: "PTT-Mirror-Close" signal name starts with "PTT-".
        private void MirrorClose(Order masterOrder, CopyRule rule)
        {
            var instr = masterOrder.Instrument;
            if (instr == null)
                return; // guard (1)
            foreach (var acc in rule.FollowerAccounts) // loop (2)
            {
                if (acc == null)
                    continue; // guard (3)
                var pos = FindPosition(acc, instr);
                if (pos == null || pos.Quantity == 0)
                    continue; // guard (4)
                var action =
                    pos.MarketPosition == MarketPosition.Long
                        ? OrderAction.Sell
                        : OrderAction.BuyToCover; // ternary: not a branch
                try
                {
                    acc.CreateOrder(
                        instr,
                        action,
                        OrderType.Market,
                        OrderEntry.Manual,
                        TimeInForce.Gtc, // B29 fix: Gtc matches ATM bracket TIF
                        pos.Quantity,
                        0,
                        0,
                        null,
                        "PTT-Mirror-Close", // signal name starts with "PTT-" (NT8 constraint)
                        DateTime.MaxValue,
                        null
                    );
                    StatusUpdate?.Invoke(acc.Name + ": mirror-close " + pos.Quantity);
                }
                catch (Exception ex)
                {
                    StatusUpdate?.Invoke("PTT-Mirror-Close error: " + ex.Message);
                }
            }
        }

        // B56 T1: IsDispatchTriggerState -- CYC=2. True for the ONE state that triggers follower placement.
        // HOTFIX-MARKET-DEDUP-01: Market orders must dispatch on Submitted ONLY.
        //   NT8/Rithmic changes OrderId from GUID (Submitted) to numeric (Accepted).
        //   Without type-awareness, both states pass the dedup cache with different keys -> double dispatch.
        // AddOn limit orders skip Submitted; arrive first as Accepted -- dispatch on Accepted (AddOn path).
        // ChartTrader limit orders skip Accepted entirely; Working is first event -- dispatch on Working (DW-B96).
        // _dedupCache in DispatchCopy (Gate 5) prevents double-dispatch for AddOn path:
        //   Accepted keys the orderId; subsequent Working event deduped by IsDedup -> early return.
        // JS-002: returns bool (not null). JS-021: no lock.
        // TESTABILITY: internal static with primitive params -- directly testable without NT8 runtime.
        internal static bool IsDispatchTriggerState(OrderState state, OrderType type) =>
            (type == OrderType.Market && state == OrderState.Submitted) // Market: GUID-keyed, Submitted only
            || (
                type == OrderType.Limit
                && (
                    state == OrderState.Accepted // AddOn path (unchanged)
                    || state == OrderState.Working
                )
            ); // ChartTrader path (DW-B96)

        // B59 T1: IsExitSignalName -- CYC=6. Returns true for names that must not trigger follower copy.
        // Covers: (1) PTT- own signals; (2) NT8 Close button; (3) NT8 Flatten; (4) NT8 Rev reversal;
        //         (5) NT8 "Exit..." prefix family; (6) NT8 ATM bracket Target1..Target9 (B78 DW-B78-01).
        // "Entry" is NOT blocked -- Gate 2 already limits dispatch to master account only.
        // Follower "Entry" orders (SendCopyWithAtm) never pass Gate 2, so no cascade is possible.
        // Stop1..Stop9 are StopMarket type -- already blocked by Gate 4 before reaching this check.
        // Only Target1..Target9 (Limit type) need explicit filtering here.
        // JS-001: no throw. JS-002: returns bool.
        // TESTABILITY: internal static with string param -- directly testable without NT8 runtime.
        internal static bool IsExitSignalName(string name)
        {
            if (name == null)
                return false;
            if (name.StartsWith("PTT-", StringComparison.Ordinal))
                return true; // (1)
            if (name == "Close")
                return true; // (2)
            if (name == "Flatten")
                return true; // (3)
            if (name.StartsWith("Rev", StringComparison.Ordinal))
                return true; // (4)
            if (name.StartsWith("Exit", StringComparison.Ordinal))
                return true; // (5)
            // B78 DW-B78-01: ATM profit-target brackets (Target1..Target9) must not trigger follower copy.
            // Pattern: "Target" prefix + digit at index 6 (same pattern as IsAtmBracketName + SnapshotTargetOrders).
            if (
                name.Length > 6
                && name.StartsWith("Target", StringComparison.Ordinal)
                && char.IsDigit(name[6])
            )
                return true; // (6)
            // NOTE: "Entry" is intentionally NOT blocked here.
            // Gate 2 already filters to master account only -- follower "Entry" orders never reach DispatchCopy.
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
            if (name == null)
                return false;
            if (name == "Close")
                return true;
            if (name == "Flatten")
                return true;
            if (name.StartsWith("Rev", StringComparison.Ordinal))
                return true;
            if (name.StartsWith("Exit", StringComparison.Ordinal))
                return true;
            return false;
        }

        // IsNonFlatDispatchName: CYC=3. Returns true when orderName must NOT trigger follower flatten.
        // Combines HOTFIX-B63-FLATTEN-01 (PTT- prefix), HOTFIX-B64-ENTRY-FLATTEN-01 ("Entry"),
        // and DW-B94 (ATM bracket names Stop1..Stop9 / Target1..Target9).
        // ATM bracket cancel events arrive during NT8 position update gap (NT8_FULL_REFERENCE line 1721)
        // and must never trigger a follower flatten -- the position is still live.
        // JS-001: no throw. JS-002: returns bool. JS-021: no lock. ASCII-only.
        internal static bool IsNonFlatDispatchName(string orderName)
        {
            if (orderName != null && orderName.StartsWith("PTT-", StringComparison.Ordinal))
                return true; // (1)
            if (orderName == "Entry")
                return true; // (2)
            if (IsAtmBracketName(orderName))
                return true; // (3) DW-B94: Stop1..Stop9 / Target1..Target9 -- ATM cancel must not flatten followers
            return false;
        }

        // --- B7-F0: Bracket mirroring methods ---

        // B8 T1: DispatchCopy -- index-tracking loop replaces plain foreach.
        // CYC=8 (at limit). GetMultiplier + scaled signal per follower.
        // JS-001: no throw in hot path. JS-021: no lock.
        private void DispatchCopy(Order order, CopyRule rule)
        {
            // Gate 0.5: block PTT- cascade AND known NT8 exit signal names (B59). CYC: 7->8 (unchanged).
            if (IsExitSignalName(order.Name))
                return;

            // Gate 3: must be a dispatch-trigger state (Submitted for market; Accepted for AddOn limit)
            if (!IsDispatchTriggerState(order.OrderState, order.OrderType)) // HOTFIX-MARKET-DEDUP-01
                return;

            // Gate 4: market or limit order type only
            bool isMarket = order.OrderType == OrderType.Market;
            bool isLimit = order.OrderType == OrderType.Limit;
            if (!isMarket && !isLimit)
                return;

            // Gate 5: dedup -- reject duplicate event for same orderId (B62: price-keyed dedup).
            // DW-B91-A: IsEntryDispatched extends dedup across EvictDedup eviction boundary.
            // Compound OR: single McCabe branch -- DispatchCopy CYC stays at 8.
            // Short-circuit: IsEntryDispatched only called when IsDedup returns false (new event).
            //   - IsDedup false + IsEntryDispatched false: first time -- TryAdd marks dispatched, proceed.
            //   - IsDedup false + IsEntryDispatched true:  eviction-bypass attempt -- blocked.
            //   - IsDedup true:  duplicate same-event -- blocked, IsEntryDispatched not called.
            var orderId = order.OrderId.ToString();
            if (IsDedup(orderId, order.LimitPrice) || IsEntryDispatched(orderId))
                return;

            // All gates passed -- build base signal
            var baseSignal = CopySignal.Create(
                order.OrderAction,
                order.OrderType,
                order.Quantity,
                order.LimitPrice,
                orderId
            );

            // B9 T1: ATR base qty -- overrides signal qty when ATR enabled, else uses signal qty
            int baseQty = _atrEnabled ? GetSuggestedQty(order.Instrument) : baseSignal.Quantity;

            // B119: DW-B128 -- snapshot instrument and last direction once before the loop.
            // TryGetValue is O(1) and allocation-free on ConcurrentDictionary.
            OrderAction currentAction = order.OrderAction;
            var instr = order.Instrument;
            bool hasLastDirection = _lastLeaderDirection.TryGetValue(
                instr.FullName,
                out OrderAction lastAction
            );

            // B8 T1: index-tracking loop applies per-follower multiplier
            int idx = 0;
            foreach (var acc in rule.FollowerAccounts)
            {
                // Merged null + cap guard. Compound || = 1 McCabe branch (per project convention L1802).
                // CYC budget: replaces 2 separate branches with 1 compound, freeing one slot for the guard below.
                if (acc == null || !PassesDailyCapCheck(acc))
                {
                    idx++;
                    continue;
                }

                // B119: DW-B128 reversal entry guard.
                // Only fires when: (a) a prior direction exists for this instrument, AND
                //                  (b) current direction differs from last, AND
                //                  (c) this follower is flat (no open position).
                // On first entry (hasLastDirection=false) guard cannot fire -- copy always proceeds.
                bool followerIsFlat = IsFlat(FindPosition(acc, instr));
                if (
                    hasLastDirection
                    && IsReversalToFlatFollower(currentAction, lastAction, followerIsFlat)
                )
                {
                    NinjaTrader.Code.Output.Process(
                        "[PTT-COPY-GUARD] skip reversal entry: "
                            + acc.Name
                            + " "
                            + instr.FullName
                            + " follower flat",
                        NinjaTrader.NinjaScript.PrintTo.OutputTab1
                    );
                    idx++;
                    continue;
                }

                int mult = GetMultiplier(rule, idx);
                var scaledSignal = CopySignal.Create(
                    baseSignal.Action,
                    baseSignal.Type,
                    baseQty * mult,
                    baseSignal.LimitPrice,
                    baseSignal.OrderId
                );
                var mode = ResolveAtmMode(rule, acc.Name);
                NinjaTrader.Code.Output.Process(
                    "[PTT-COPY] dispatch: "
                        + scaledSignal.Action
                        + " x"
                        + scaledSignal.Quantity
                        + " "
                        + order.Instrument.FullName
                        + " -> "
                        + acc.Name
                        + " mult="
                        + mult
                        + " mode="
                        + mode.GetType().Name
                        + " name="
                        + (order.Name ?? "null"),
                    NinjaTrader.NinjaScript.PrintTo.OutputTab1
                );
                if (mode is FollowerAtmMode.Named namedAtm) // HOTFIX-B66-NATIVE-ATM: Named mode -> native ATM
                    SendCopyWithAtm(acc, order.Instrument, in scaledSignal, namedAtm);
                else
                    SendCopy(acc, order.Instrument, in scaledSignal, mode);
                idx++;
            }

            // B119: DW-B128 -- record direction dispatched for this instrument.
            // Write happens AFTER the loop so all followers in this dispatch see the same lastAction.
            _lastLeaderDirection[instr.FullName] = currentAction;
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
            return (
                    order.OrderState == OrderState.Working
                    || order.OrderState == OrderState.Accepted
                ) && IsBracketLegStatic(order);
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

        // DW-B134: true if order name has STP suffix (NT8 ATM bracket stops: "Buy STP", "Sell STP").
        // DW-B137: extended to cover Stop1/Stop2/Stop3 and Target1/Target2/Target3 (MES $200 SL 6 ATM).
        // Mirrors IsBracketLegStatic STP+Stop+Target clauses. Made internal static for test access.
        // Option A safety: grep confirms 0 CreateOrder calls use "Stop*"/"Target*" prefixed names.
        // CYC=1: expression body. JS-021: no lock. JS-001: no throw. ASCII-only.
        internal static bool IsAtmSTPOrder(Order order) =>
            order.Name != null
            && (
                order.Name.EndsWith("STP", StringComparison.OrdinalIgnoreCase)
                || order.Name.StartsWith("Stop", StringComparison.OrdinalIgnoreCase)
                || order.Name.StartsWith("Target", StringComparison.OrdinalIgnoreCase)
            );

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
        // DW-B134/DW-B137: CYC=7: fo null(1), price delta(2), ATM STP(3), ATM TGT(3b), IsTrailingStop(4), isStop branch(5), [CYC from branching=7].
        // JS-001: try/catch around acc.Change() -- no throw in hot path.
        // DW-B9-GAP-001a: trailing stop follower orders are skipped (Option B: skip is safer).
        // DW-B134: ATM STP brackets (EndsWith "STP") require cancel+resubmit -- acc.Change() is no-op.
        private void SyncFollowerBracket(
            Account acc,
            Order leaderOrder,
            bool isStop,
            double newPrice,
            double tickSize
        )
        {
            var fo = FindFollowerBracketOrder(
                acc,
                leaderOrder.FromEntrySignal,
                isStop,
                leaderOrder.Name
            );
            TryLogSFBTrace(acc, leaderOrder, isStop, fo);
            if (fo == null) // (1)
                return;

            double currentPrice = isStop ? fo.StopPrice : fo.LimitPrice;
            if (Math.Abs(newPrice - currentPrice) < tickSize) // (2)
                return;

            // DW-B134: ATM STP path -- cancel+resubmit before IsTrailingStop guard.
            // DW-B137: ATM TGT path -- cancel+resubmit for target brackets (acc.Change() no-op).
            // IsTrailingStop fires on StopMarket orders; ATM STP brackets ARE StopMarket.
            // Without branch (3), IsTrailingStop would return early and skip stop sync.
            if (isStop && IsAtmSTPOrder(fo)) // (3) DW-B134 + DW-B137
            {
                SyncAtmFollowerBracket(acc, fo, newPrice);
                return;
            }
            if (!isStop && IsAtmSTPOrder(fo)) // (3b) DW-B137: ATM target cancel+resubmit
            {
                SyncAtmFollowerTarget(acc, fo, newPrice, leaderOrder);
                return;
            }

            if (isStop && IsTrailingStop(fo)) // (4)
            {
                StatusUpdate?.Invoke("HandleBracketChange: skip trailing stop " + fo.Name);
                return;
            }

            try
            {
                if (isStop) // (4)
                    fo.StopPrice = newPrice;
                else
                    fo.LimitPrice = newPrice;
                acc.Change(new Order[] { fo });
                StatusUpdate?.Invoke(
                    acc.Name
                        + ": bracket synced "
                        + (isStop ? "stop" : "target")
                        + " -> "
                        + newPrice
                );
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke(acc.Name + ": bracket sync error: " + ex.Message);
            }
        }

        // DW-B134: cancel+resubmit for ATM-owned STP brackets.
        // acc.Change() is a no-op on ATM-engine brackets (confirmed CopyEngine.cs L3598-3601).
        // Pattern mirrors MoveStopToBreakEven cancel+resubmit (L3598+).
        // CYC=6: (1) acc null guard, (2) fo null guard, (3) IsNoPriceChange guard [T2 B137],
        //        (4) Block A catch, (5) Block B catch, (6) newStop null guard.
        // T4 B137: CancelExistingPttStpDrag(acc, fo) call added before Block A (DW-B151 pre-sweep).
        //   Method call adds 0 McCabe branches. CancelExistingPttStpDrag CYC counted in that method.
        // T2 B137: DW-B147/DW-B149 IsNoPriceChange guard added after fo null check.
        // Two independent try/catch blocks -- exception handlers add 0 McCabe branches each (per codebase convention L2301).
        // JS-021: no lock. JS-001: two independent try/catch -- no throw in hot path.
        //   Block A (Cancel): if Cancel throws, Block B still executes (independent isolation).
        //   Block B (CreateOrder+Submit): naked-position risk eliminated by isolation from Block A.
        // NT8-049: StopMarket arg6=0 (limitPrice), arg7=newPrice (stopPrice).
        // NT8-013: Core.Globals.MaxDate for gtd. NT8-007: (CustomOrder)null.
        // NT8-014: order name starts with "PTT-".
        // OQ-03: cancel of follower ATM bracket is SAFE -- Gate 2 (FindMatchingRule L1609)
        //        returns null for follower account orders, blocking TryCancelFollowerEntries.
        private void SyncAtmFollowerBracket(Account acc, Order fo, double newPrice)
        {
            if (acc == null) // (1)
                return;
            if (fo == null) // (2)
                return;
            if (IsNoPriceChange(fo.StopPrice, newPrice)) // (3) T2 B137 DW-B147/DW-B149 guard
                return;

            CancelExistingPttStpDrag(acc, fo); // T4 B137 Block A-Prime pre-sweep (DW-B151)

            // Block A -- Cancel only. Independent: if Cancel throws, Block B still runs.
            try
            {
                acc.Cancel(new Order[] { fo });
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke(acc.Name + ": STP cancel error: " + ex.Message);
            }

            // Block B -- CreateOrder + Submit only. Runs regardless of Block A outcome.
            try
            {
                var newStop = acc.CreateOrder(
                    fo.Instrument,
                    fo.OrderAction,
                    OrderType.StopMarket,
                    OrderEntry.Automated,
                    TimeInForce.Day,
                    fo.Quantity,
                    0,
                    newPrice,
                    "",
                    "PTT-STP-Drag",
                    NinjaTrader.Core.Globals.MaxDate,
                    (NinjaTrader.Cbi.CustomOrder)null
                );
                if (newStop == null) // (3)
                {
                    StatusUpdate?.Invoke(acc.Name + ": ATM STP CreateOrder returned null");
                    return;
                }
                acc.Submit(new[] { newStop });
                StatusUpdate?.Invoke(acc.Name + ": ATM STP resubmit -> " + newPrice);
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke(acc.Name + ": STP create error: " + ex.Message);
            }
        }

        // CYC=5: base(1) + ||(1) + ||(1) + ||(1) + ||(1) = 5.
        // Pure state predicate -- no side effects. Static.
        // Returns true for all non-terminal states where a PTT-STP-Drag may still be cancelled.
        // Submitted: order en-route to broker. Working: live in exchange. Accepted: acked by broker.
        // CancelPending: cancel dispatched by NT8, not yet acked by broker.
        // CancelSubmitted: cancel acked by broker, not yet confirmed by exchange.
        // B139: DW-B152-B fix -- closes cancel-in-flight race (CancelPending/CancelSubmitted gap).
        // JS-002: bool return, no null. ASCII-only. No DateTime. No lock.
        private static bool IsPttStpDragCancellable(Order o) =>
            o.OrderState == OrderState.Submitted
            || o.OrderState == OrderState.Working
            || o.OrderState == OrderState.Accepted
            || o.OrderState == OrderState.CancelPending
            || o.OrderState == OrderState.CancelSubmitted;

        // CYC=1: pure delegation to IsPttStpDragCancellable.
        // Test seam for xUnit access. InternalsVisibleTo("PropTraderTools.Tests") granted at L46.
        internal static bool IsPttStpDragCancellableTestable(Order o) =>
            IsPttStpDragCancellable(o);

        // CYC=6: base(1) + foreach(1) + if(1) + &&Name(1) + &&Instrument(1) + ?.(1) = 6.
        // B139: DW-B152-B fix -- IsPttStpDragCancellable extracted to include CancelPending||CancelSubmitted.
        // OrderState filter now covers: Submitted||Working||Accepted||CancelPending||CancelSubmitted.
        // acc.Cancel() on CancelPending/CancelSubmitted is idempotent; rejection absorbed by try/catch.
        // JS-021: no lock. JS-001: try/catch -- no rethrow. JS-002: void return.
        // acc.Orders.ToList(): thread-safe snapshot. ASCII-only. No DateTime.
        private void CancelExistingPttStpDrag(Account acc, Order fo)
        {
            foreach (var o in acc.Orders.ToList())
            {
                if (
                    IsPttStpDragCancellable(o)
                    && o.Name == "PTT-STP-Drag"
                    && o.Instrument?.FullName == fo.Instrument?.FullName
                )
                {
                    try
                    {
                        acc.Cancel(new Order[] { o });
                    }
                    catch (Exception ex)
                    {
                        StatusUpdate?.Invoke(acc.Name + ": STP pre-cancel error: " + ex.Message);
                    }
                }
            }
        }

        // Test seam for xUnit access. InternalsVisibleTo("PropTraderTools.Tests") granted at L46.
        // CYC=1: pure delegation to CancelExistingPttStpDrag.
        internal void CancelExistingPttStpDragTestable(Account acc, Order fo) =>
            CancelExistingPttStpDrag(acc, fo);

        // DW-B137: cancel+resubmit for ATM-owned target brackets (Limit type).
        // acc.Change() is a no-op on ATM-engine brackets (confirmed B129 SIM gate 2026-08-31).
        // Pattern mirrors SyncAtmFollowerBracket (DW-B134/B129 LaneB).
        // DW-B139 fix: Block A-Prime pre-sweep cancels prior Working PTT-TGT-Drag before Block B.
        // CYC=8: (1) acc null, (2) fo null, (3) IsNoPriceChange guard [T2],
        //        (4) foreach A-Prime, (5) OrderState==Working, (6) Name=="PTT-TGT-Drag",
        //        (7) catch A-Prime, (8) Block A catch.
        // AT LIMIT. T2 B137: DW-B147/DW-B149 guard. T1 B137: Phase C -> ExecutePhaseCStopReplacement.
        // Two independent try/catch blocks -- Block A isolates Cancel; Block B isolates CreateOrder+Submit.
        // JS-021: no lock. JS-001: two independent try/catch -- no throw in hot path.
        // NT8-049: Limit order arg5=limitPrice (newPrice), arg6=0 (stopPrice unused for Limit).
        // NT8-013: Core.Globals.MaxDate for GTC. NT8-007: (CustomOrder)null.
        // NT8-014: order name starts with "PTT-" ("PTT-TGT-Drag").
        // OQ-03: cancel of follower ATM target bracket SAFE -- Gate 2 (FindMatchingRule L1609)
        //        returns null for follower account orders, blocking TryCancelFollowerEntries.
        private void SyncAtmFollowerTarget(
            Account acc,
            Order fo,
            double newPrice,
            Order? leaderOrder = null
        )
        {
            if (acc == null) // (1)
                return;
            if (fo == null) // (2)
                return;
            if (IsNoPriceChange(fo.LimitPrice, newPrice)) // (3) T2 B137 DW-B147/DW-B149 guard
                return;

            // Block A-Prime -- cancel any existing PTT-TGT-Drag for this instrument on the follower.
            // Prevents accumulation of Working PTT-TGT-Drag orders on repeated drag events (DW-B139).
            // JS-001: try/catch -- no throw in hot path. JS-021: no lock -- acc.Orders iteration safe.
            foreach (var o in acc.Orders.ToList())
            {
                if (
                    o.OrderState == OrderState.Working
                    && o.Name == "PTT-TGT-Drag"
                    && o.Instrument?.FullName == fo.Instrument?.FullName
                )
                {
                    try
                    {
                        acc.Cancel(new Order[] { o });
                    }
                    catch (Exception ex)
                    {
                        StatusUpdate?.Invoke(acc.Name + ": TGT pre-cancel error: " + ex.Message);
                    }
                }
            }

            // Block A -- Cancel only. Independent: if Cancel throws, Block B still runs.
            try
            {
                acc.Cancel(new Order[] { fo });
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke(acc.Name + ": TGT cancel error: " + ex.Message);
            }

            // Block B -- CreateOrder + Submit only. Runs regardless of Block A outcome.
            try
            {
                var newTarget = acc.CreateOrder(
                    fo.Instrument,
                    fo.OrderAction,
                    OrderType.Limit,
                    OrderEntry.Automated,
                    TimeInForce.Day,
                    fo.Quantity,
                    newPrice,
                    0,
                    "",
                    "PTT-TGT-Drag",
                    NinjaTrader.Core.Globals.MaxDate,
                    (NinjaTrader.Cbi.CustomOrder)null
                );
                if (newTarget == null) // (3)
                {
                    StatusUpdate?.Invoke(acc.Name + ": ATM TGT CreateOrder returned null");
                    return;
                }
                acc.Submit(new[] { newTarget });
                StatusUpdate?.Invoke(acc.Name + ": ATM TGT resubmit -> " + newPrice);
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke(acc.Name + ": TGT create error: " + ex.Message);
            }

            ExecutePhaseCStopReplacement(acc, fo, leaderOrder); // T1 B137: Phase C extracted
        }

        // B132 LaneA -- DeriveLeaderBracketIndex: parse integer suffix from leader order name.
        // e.g. "Target3" -> 3, "Stop99" -> 99, null -> 0.
        // CYC=3: (1) null/empty guard; (2) int.TryParse; (3) n <= 0 guard. JS-002: returns 0 on all failure paths.
        private static int DeriveLeaderBracketIndex(Order? leaderOrder)
        {
            if (leaderOrder == null || string.IsNullOrEmpty(leaderOrder.Name)) // (1)
                return 0;
            var name = leaderOrder.Name;
            var i = name.Length - 1;
            while (i >= 0 && char.IsDigit(name[i]))
                i--;
            if (i == name.Length - 1) // no trailing digit
                return 0;
            if (!int.TryParse(name.Substring(i + 1), out var n)) // (2)
                return 0;
            if (n <= 0) // (3)
                return 0;
            return n;
        }

        // B132 LaneA -- FindLeaderStopPrice: scan leader account orders for Working "Stop{N}".
        // Returns StopPrice of the match, or 0.0 if not found.
        // CYC=5: (1) null account; (2) zero index; (3) foreach; (4) name match; (5) state==Working.
        // JS-002: returns 0.0 on all failure paths. JS-021: no lock (NT8 Orders collection is thread-safe).
        private static double FindLeaderStopPrice(Account? leaderAccount, int bracketIndex)
        {
            if (leaderAccount == null) // (1)
                return 0.0;
            if (bracketIndex <= 0) // (2)
                return 0.0;
            var targetName = "Stop" + bracketIndex.ToString();
            foreach (var order in leaderAccount.Orders.ToList()) // (3)
            {
                if (
                    order.Name == targetName // (4)
                    && order.OrderState == OrderState.Working
                ) // (5)
                    return order.StopPrice;
            }
            return 0.0;
        }

        // B132 LaneA -- CreateFollowerReplacementStop: place PTT-STP-Drag StopMarket on followerAcc.
        // CYC=4: (1) stopPrice guard; (2) try block; (3) newStop null check; (4) catch.
        // JS-001: catch+return -- no rethrow. JS-002: void method. NT8-014: "PTT-STP-Drag" PTT- prefix.
        // oco="": PTT-STP-Drag is NOT part of any NT8 ATM OCO group (NT8_FULL_REFERENCE.md L2118).
        private void CreateFollowerReplacementStop(
            Account followerAcc,
            Instrument instr,
            int qty,
            OrderAction stopAction,
            double stopPrice
        )
        {
            if (stopPrice <= 0.0) // (1)
            {
                StatusUpdate?.Invoke(followerAcc?.Name + ": PTT-STP-Drag skipped: stopPrice <= 0");
                return;
            }
            try // (2)
            {
                var newStop = followerAcc.CreateOrder(
                    instr,
                    stopAction,
                    OrderType.StopMarket,
                    OrderEntry.Automated,
                    TimeInForce.Day,
                    qty,
                    0,
                    stopPrice,
                    "",
                    "PTT-STP-Drag",
                    NinjaTrader.Core.Globals.MaxDate,
                    (NinjaTrader.Cbi.CustomOrder)null
                );
                if (newStop == null) // (3)
                {
                    StatusUpdate?.Invoke(
                        followerAcc.Name + ": PTT-STP-Drag: CreateOrder returned null"
                    );
                    return;
                }
                followerAcc.Submit(new[] { newStop });
                StatusUpdate?.Invoke(
                    followerAcc.Name + ": PTT-STP-Drag placed @ " + stopPrice.ToString()
                );
            }
            catch (Exception ex) // (4)
            {
                StatusUpdate?.Invoke(followerAcc.Name + ": PTT-STP-Drag error: " + ex.Message);
            }
        }

        // CYC=2. Extracted Phase C block from SyncAtmFollowerTarget (T1 extraction -- B137).
        // Replaces inline Phase C code (L2439-2442 pre-B137):
        //   DeriveLeaderBracketIndex + FindLeaderStopPrice + CreateFollowerReplacementStop.
        // McCabe branches: base(1) + leaderOrder?.Account null-conditional(1) = CYC=2.
        // Extraction reduces SyncAtmFollowerTarget from CYC=8 to CYC=7 (removes ?. branch from parent).
        // ZERO behavior change. JS-021: no lock. JS-001: delegates to CreateFollowerReplacementStop catch.
        // JS-002: void return. ASCII-only. No DateTime. No FontFamily.
        private void ExecutePhaseCStopReplacement(Account acc, Order fo, Order? leaderOrder)
        {
            int bracketIdx = DeriveLeaderBracketIndex(leaderOrder);
            double stp = FindLeaderStopPrice(leaderOrder?.Account, bracketIdx);
            CreateFollowerReplacementStop(acc, fo.Instrument, fo.Quantity, fo.OrderAction, stp);
        }

        // B10 T1 -- HandleBracketChange: delegates inner loop body to SyncFollowerBracket.
        // CYC=6: isStop(1), instr null(2), tickSize(3), rawPrice(4), foreach acc(5), acc null(6).
        // JS-001: try/catch inside SyncFollowerBracket -- no throw in hot path.
        // JS-021: no lock -- _orderMap uses ConcurrentDictionary (atomic).
        // V02: tick-rounded price applied BEFORE price-delta guard (preserved in SyncFollowerBracket).
        private void HandleBracketChange(Order leaderOrder, CopyRule rule)
        {
            bool isStop = IsStopLeg(leaderOrder); // (1)

            var instrument = leaderOrder.Instrument;
            if (instrument == null) // (2)
                return;

            double tickSize = instrument.MasterInstrument?.TickSize ?? 0.0; // (3)
            double rawPrice = isStop ? leaderOrder.StopPrice : leaderOrder.LimitPrice; // (4)
            // V02: tick rounding applied BEFORE price-delta guard
            double newPrice = tickSize > 0 ? Math.Round(rawPrice / tickSize) * tickSize : rawPrice;
            if (_diagnosticMode)
                NinjaTrader.Code.Output.Process(
                    "[TP3-HBC] isStop="
                        + isStop
                        + " leaderName="
                        + (leaderOrder.Name ?? "null")
                        + " rawPrice="
                        + rawPrice
                        + " newPrice="
                        + newPrice
                        + " followerCount="
                        + rule.FollowerAccounts.Length,
                    NinjaTrader.NinjaScript.PrintTo.OutputTab1
                );

            foreach (var acc in rule.FollowerAccounts) // (5)
            {
                if (acc == null) // (6)
                    continue;
                SyncFollowerBracket(acc, leaderOrder, isStop, newPrice, tickSize);
            }
        }

        // B131 DW-B138: predicate encapsulating signal-first / name-fallback match logic.
        // B133 DW-B142: null-guard added to branch (1) -- prevents null==null false-positive (ATM drag cancel-all bug).
        // CYC=3: (1) signal equality check, (2) leaderName null guard, (3) name equality check.
        // JS-021: no lock (static, no shared state). JS-001: no throw. JS-002: returns bool (no null).
        // ASCII-only. DateTime.UtcNow not used (no time logic).
        internal static bool SignalOrNameMatches(
            Order order,
            string? signalName,
            string? leaderName
        )
        {
            if (signalName != null && order.FromEntrySignal == signalName) // (1) primary: signal equality (null-guarded)
                return true;
            if (leaderName == null) // (2) no fallback available
                return false;
            return order.Name == leaderName; // (3) ATM Name-based fallback
        }

        // CYC=4. Returns first matching working bracket order for the follower.
        // V04 B131 DW-B138: leaderName param added -- ATM Name-based fallback when FromEntrySignal null/empty.
        // V03: return type is Order? (nullable) -- null contract explicit (JS-002 compliant).
        // V01: matching by FromEntrySignal name -- not leg-type scan.
        // JS-021: no lock. JS-001: no throw. JS-002: Order? makes null contract explicit.
        // DW-B143: state filter extended to include OrderState.Accepted (was Working-only).
        // DW-B144: state filter extended to include OrderState.Submitted (B134 fix).
        // Accepted orders are broker-confirmed but not yet exchange-Working -- cancel is safe.
        // Submitted orders are live (non-terminal) -- cancel absorbs ErrorCode.UnableToCancelOrder.
        private Order? FindFollowerBracketOrder(
            Account follower,
            string? fromEntrySignalName,
            bool isStop,
            string? leaderName = null
        ) =>
            FindFollowerBracketOrder(
                follower.Orders.ToList(),
                fromEntrySignalName,
                isStop,
                leaderName
            );

        // CYC=7 (post-B136). AT LIMIT RESOLVED; headroom = 1.
        // foreach(1) + OrderPassesBracketGate guard(1) + state filter(3) + isStop(1) + type match(1) = 7.
        // DW-B143: Accepted added. DW-B144: Submitted added. DW-B145: leaderName exact guard. DW-B146: MatchesLeaderName helper. DW-B148: OrderPassesBracketGate fused guard (B136).
        // JS-021: no lock. JS-001: no throw. JS-002: Order? null contract unchanged.
        private Order? FindFollowerBracketOrder(
            IEnumerable<Order> orders,
            string? fromEntrySignalName,
            bool isStop,
            string? leaderName = null
        )
        {
            foreach (var order in orders) // (1) branch
            {
                if (!OrderPassesBracketGate(order, fromEntrySignalName, leaderName, isStop)) // (1) branch -- B136 DW-B148: fused guard (ATM path routes to MatchesLeaderName)
                    continue;
                if (
                    order.OrderState != OrderState.Working // (3) branches -- B134 DW-B144: Submitted added
                    && order.OrderState != OrderState.Accepted
                    && order.OrderState != OrderState.Submitted
                )
                    continue;
                if (isStop) // (1) branch
                {
                    if (
                        order.OrderType == OrderType.StopMarket
                        || order.OrderType == OrderType.StopLimit
                    ) // (1) branch
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

        // B131 DW-B138: test seam -- delegates to internal methods for xUnit test access.
        // InternalsVisibleTo("PropTraderTools.Tests") granted at top of file (L46).
        internal static bool SignalOrNameMatchesTestable(
            Order order,
            string? signalName,
            string? leaderName
        ) => SignalOrNameMatches(order, signalName, leaderName);

        // B135 DW-B146: PTT-prefix fallback -- after first drag, original ATM bracket is Cancelled;
        // replacement is "PTT-TGT-Drag" (target) or "PTT-STP-Drag" (stop).
        // FindFollowerBracketOrder must recognise these as the incumbent bracket on repeated drags.
        // CYC=5: base(1) + leaderName null(1) + name==(1) + !isStop&&TGT(1) + isStop&&STP(1) = 5.
        // JS-021: no lock (static, no shared state). JS-001: no throw. JS-002: returns bool.
        // ASCII-only. "PTT-TGT-Drag" and "PTT-STP-Drag" are ASCII.
        private static bool MatchesLeaderName(Order order, string? leaderName, bool isStop)
        {
            if (leaderName == null) // (1) no constraint -- pass through
                return true;
            if (order.Name == leaderName) // (2) exact ATM name match
                return true;
            if (!isStop && order.Name == "PTT-TGT-Drag") // (3) replacement target match
                return true;
            if (isStop && order.Name == "PTT-STP-Drag") // (4) replacement stop match
                return true;
            return false;
        }

        // B135 DW-B146: test seam -- delegates to MatchesLeaderName for xUnit test access.
        // InternalsVisibleTo("PropTraderTools.Tests") granted at L46.
        internal static bool MatchesLeaderNameTestable(
            Order order,
            string? leaderName,
            bool isStop
        ) => MatchesLeaderName(order, leaderName, isStop);

        // CYC=1. Pure predicate: returns true when currentPrice == newPrice (no price change occurred).
        // Used as early-return guard in SyncAtmFollowerBracket and SyncAtmFollowerTarget to suppress
        // spurious cancel+resubmit cycles caused by ARM events (DW-B147) or ChangeSubmitted races (DW-B149).
        // CYC=1: pure expression method body, no branches.
        // JS-021: no lock (static, no shared state). JS-001: no throw. JS-002: returns bool, not null.
        // JS-036: stack-only, zero allocation. ASCII-only. No DateTime. No FontFamily.
        private static bool IsNoPriceChange(double currentPrice, double newPrice) =>
            currentPrice == newPrice;

        // Test seam for xUnit access. InternalsVisibleTo("PropTraderTools.Tests") granted at L46.
        internal static bool IsNoPriceChangeTestable(double currentPrice, double newPrice) =>
            IsNoPriceChange(currentPrice, newPrice);

        // B136 DW-B148: fused bracket-gate predicate -- replaces the sequential SignalOrNameMatches +
        // MatchesLeaderName guard pair in FindFollowerBracketOrder.
        // Signal path (signalName != null): exclusive signal-match only. Preserves original signal
        //   exclusivity -- orders from a different entry signal are rejected even if name matches.
        // ATM path (signalName == null): delegates to MatchesLeaderName, which passes exact ATM name
        //   (e.g. "Target3") AND PTT-prefix replacements ("PTT-TGT-Drag", "PTT-STP-Drag").
        //   This is the fix: PTT-TGT-Drag now reaches MatchesLeaderName and returns true.
        // CYC=2: base(1) + if(!string.IsNullOrEmpty(signalName))(1) = 2. Well within <= 8.
        // T3 B137 DW-B150: condition changed from (signalName != null) to (!string.IsNullOrEmpty(signalName)).
        // Empty string now routes to ATM path (MatchesLeaderName), not signal path.
        // Root cause fixed: leaderOrder.FromEntrySignal="" (NT8 ATM bracket state-transition event)
        //   was routing to signal path, comparing null == "" = FALSE, returning fo=NULL.
        //   After fix: !IsNullOrEmpty("") = false -> ATM path -> MatchesLeaderName -> Stop3 found.
        // JS-021: no lock (static, no shared state). JS-001: no throw. JS-002: returns bool.
        // ASCII-only. No DateTime. No FontFamily. No hex color literals.
        private static bool OrderPassesBracketGate(
            Order order,
            string? signalName,
            string? leaderName,
            bool isStop
        )
        {
            if (!string.IsNullOrEmpty(signalName)) // (1) signal path: non-empty only -- null OR "" = ATM path [T3 B137 DW-B150]
                return order.FromEntrySignal == signalName;
            return MatchesLeaderName(order, leaderName, isStop); // ATM path: exact name OR PTT-prefix
        }

        // B136 DW-B148: test seam -- delegates to OrderPassesBracketGate for xUnit test access.
        // InternalsVisibleTo("PropTraderTools.Tests") granted at L46.
        internal static bool OrderPassesBracketGateTestable(
            Order order,
            string? signalName,
            string? leaderName,
            bool isStop
        ) => OrderPassesBracketGate(order, signalName, leaderName, isStop);

        internal Order? FindFollowerBracketOrderTestable(
            Account follower,
            string? fromEntrySignalName,
            bool isStop,
            string? leaderName = null
        ) => FindFollowerBracketOrder(follower, fromEntrySignalName, isStop, leaderName);

        // B133 LaneB DW-B143: list-injection test seam -- Account.Orders is sealed in NT8.
        // Accepts IEnumerable<Order> directly so xUnit tests can inject stub order lists.
        // InternalsVisibleTo("PropTraderTools.Tests") granted at top of file (L46).
        internal Order? FindFollowerBracketOrderTestable(
            IEnumerable<Order> orders,
            string? fromEntrySignalName,
            bool isStop,
            string? leaderName = null
        ) => FindFollowerBracketOrder(orders, fromEntrySignalName, isStop, leaderName);

        // B132 LaneA: test seams for DW-B141 helper methods.
        // InternalsVisibleTo("PropTraderTools.Tests") granted at L46.
        internal static int DeriveLeaderBracketIndexTestable(Order? leaderOrder) =>
            DeriveLeaderBracketIndex(leaderOrder);

        internal static double FindLeaderStopPriceTestable(
            Account? leaderAccount,
            int bracketIndex
        ) => FindLeaderStopPrice(leaderAccount, bracketIndex);

        // CYC=2. Returns StopPrice for StopLimit orders, LimitPrice for all others.
        // NT8 fact: StopLimit.LimitPrice==0 always; drag price lives in StopPrice (Fact 1).
        // B66-LaneC: DW-B64-01 fix -- GetOrderPrice used in Gate C and HandleEntryChange.
        // JS-021: no lock. JS-001: no throw. Pure computation. Zero heap allocation (JS-036).
        private static double GetOrderPrice(Order order) =>
            order.OrderType == OrderType.StopLimit ? order.StopPrice : order.LimitPrice;

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
        // HOTFIX-CLONE-DRAG: widened name to "PTT-Copy" OR "Entry" (Clone mode uses "Entry").
        // NT8: broker-simulated StopLimit may stay in Accepted (NT8_FULL_REFERENCE.md line 1005).
        // JS-002: returns null when not found -- callers must null-guard.
        private static Order? FindFollowerEntryOrder(Account follower, Instrument instrument)
        {
            foreach (var order in follower.Orders.ToList()) // (1)
            {
                if (order.Instrument != instrument) // (2)
                    continue;
                if (
                    (
                        order.OrderState == OrderState.Working
                        || order.OrderState == OrderState.Accepted
                    ) // (3)
                    && (
                        order.OrderType == OrderType.Limit || order.OrderType == OrderType.StopLimit
                    )
                    && (order.Name == "PTT-Copy" || order.Name == "Entry")
                )
                    return order;
            }
            return null;
        }

        // B62/B66-LaneC/B67-LaneB: sync a leader entry drag to all follower working PTT-Copy entry orders.
        // B67-LaneB: DW-B67-02 -- replaced acc.Change() with Cancel+CreateOrder+Submit.
        //   acc.Change() on Apex/Rithmic is a silent broker-side no-op for pre-fill entry orders.
        //   Pattern from @2Custom PropagateMasterEntryMove (FIX-PM-02, FIX-PM-02b).
        //   NT8_FULL_REFERENCE.md lines 898-899: StopLimit price in StopPrice, not LimitPrice.
        //   limitPx = fo.OrderType == StopLimit ? 0 : newPrice
        //   stopPx  = fo.OrderType == StopLimit ? newPrice : 0
        // Triggered by Gate C when leader's entry orderId is already in dedup cache but price changed.
        // CYC=7: instr null(1) + tickSize ternary(2) + foreach acc(3) + acc null(4)
        //   + fo null(5) + price delta guard(6) + order null guard in CreateOrder(7).
        // JS-001: no throw in hot path. JS-021: no lock. JS-002: void.
        private void HandleEntryChange(Order leaderOrder, CopyRule rule)
        {
            var instrument = leaderOrder.Instrument;
            if (instrument == null) // (1)
                return;

            double tickSize = instrument.MasterInstrument?.TickSize ?? 0.0; // (2)
            double rawPrice = GetOrderPrice(leaderOrder); // B66-LaneC: StopLimit price in StopPrice
            double newPrice = tickSize > 0 ? Math.Round(rawPrice / tickSize) * tickSize : rawPrice;

            // HOTFIX-ENTRY-DRAG-DEDUP: keep leader orderId in cache at newPrice (upsert, not remove).
            // TryRemove caused Working-state re-entry to fall through Gate C into DispatchCopy,
            // placing a second PTT-Copy order (doubling follower contracts).
            // Keeping the key at newPrice means the Working event hits Gate C, sees delta=0 (price unchanged
            // since Accepted), and returns without dispatching. DispatchCopy's IsDedup also blocks it.
            _dedupCache[leaderOrder.OrderId.ToString()] = newPrice;

            foreach (var acc in rule.FollowerAccounts) // (3)
            {
                if (acc == null) // (4)
                    continue;

                var fo = FindFollowerEntryOrder(acc, instrument);
                if (fo == null) // (5)
                    continue;

                double currentPrice = GetOrderPrice(fo); // B66-LaneC: StopLimit price in StopPrice
                if (tickSize > 0 && Math.Abs(newPrice - currentPrice) < tickSize) // (6)
                    continue;

                // B67-LaneB DW-B67-02: Cancel+CreateOrder+Submit (acc.Change() is Apex/Rithmic no-op).
                // NT8_FULL_REFERENCE.md lines 898-899: StopLimit price in StopPrice not LimitPrice.
                double limitPx = fo.OrderType == OrderType.StopLimit ? 0.0 : newPrice; // (7a)
                double stopPx = fo.OrderType == OrderType.StopLimit ? newPrice : 0.0; // (7b)
                acc.Cancel(new Order[] { fo });
                var order = acc.CreateOrder(
                    instrument,
                    fo.OrderAction,
                    fo.OrderType,
                    OrderEntry.Manual,
                    fo.TimeInForce,
                    fo.Quantity,
                    limitPx,
                    stopPx,
                    null,
                    fo.Name,
                    DateTime.MaxValue,
                    null
                );
                if (order != null) // (7)
                {
                    acc.Submit(new[] { order });
                    // B69 DW-B69-03: preload new orderId into _dedupCache at newPrice.
                    // Prevents the new order's Accepted event from re-entering DispatchCopy
                    // (same-account double-copy guard, lightweight FSM-in-flight equivalent).
                    // Ref: @2Custom PropagateFollowerEntryReplace Build 947 -- PendingCancel absorb.
                    _dedupCache[order.OrderId.ToString()] = newPrice;
                }
                StatusUpdate?.Invoke(acc.Name + ": entry dragged -> " + newPrice);
            }
        }

        // CYC=2. Records (signal, follower) association in _orderMap for future bracket lookups.
        // JS-025: ConcurrentDictionary.GetOrAdd is atomic -- no lock needed.
        // Engineer Note #1: dedup guard prevents duplicate bindings on repeated Working state events.
        private void PopulateOrderMap(string fromEntrySignalName, Account followerAccount)
        {
            var bag = _orderMap.GetOrAdd(
                fromEntrySignalName,
                _ => new ConcurrentBag<FollowerBinding>()
            );
            // Dedup guard: prevent accumulating duplicate bindings on repeated Working state events
            if (!bag.Any(b => b.FollowerAccount?.Name == followerAccount?.Name)) // (1) branch
                bag.Add(new FollowerBinding(followerAccount, fromEntrySignalName));
        }

        // CYC=2. Fires PositionStateChanged only on states that open or grow a position.
        // FIX-B: Cancelled and Rejected REMOVED from the filter.
        // Rationale: when ATM brackets (Stop1/Stop2/Target1/Target2) cancel as part of closing
        // a trade, the position is already gone. TryFirePositionState fires hasPos=False hundreds
        // of times -- each one resets the BE button to Idle via HOTFIX-F3 in UpdateButtonColors.
        // Only Filled and PartFilled can open or grow a position. Cancelled/Rejected never do.
        // The flat signal is still correctly delivered: the Filled close event fires first with
        // hasPos=False (after position removal), so the button color is correct.
        // JS-003: PositionState readonly struct captured by value in event args (no aliasing).
        private void TryFirePositionState(OrderEventArgs e)
        {
            // Fire ONLY on Filled/PartFilled -- the only states that alter position quantity.
            var state = e.OrderState;
            if (state != OrderState.Filled && state != OrderState.PartFilled) // (1) branch
                return;

            if (e.Order?.Instrument?.FullName == null) // (1) branch
                return;

            string instr = e.Order.Instrument.FullName;
            bool hasPos = HasOpenPosition(e.Order.Account, e.Order.Instrument);

            // HOTFIX-B76-POSSTATE-DEDUP-01: Interlocked CAS dedup.
            // GetOrAdd returns the stable int[1] box for this instrument (allocates once).
            // Interlocked.Exchange atomically writes newVal and returns the prior value.
            // If prior == newVal: no transition -- another thread already wrote it, skip.
            // 0=False, 1=True, 2=unknown (initial sentinel -- always fires on first fill).
            int newVal = hasPos ? 1 : 0;
            var box = _lastHasPos.GetOrAdd(instr, _ => new int[] { 2 });
            int prior = System.Threading.Interlocked.Exchange(ref box[0], newVal);
            if (prior == newVal)
                return;

            // DW-B135: clear direction key when leader position goes flat.
            // Prevents false-positive IsReversalToFlatFollower on next entry after clean close.
            // DW-B128 preserved: during race window, hasPos=True, so this path not taken.
            // JS-021: TryRemove is lock-free. JS-001: no throw. CYC: 3->6 (three new branches).
            if (!hasPos)
            {
                bool isLeaderAcct = false;
                foreach (var r in _rules)
                {
                    if (e.Order.Account.Name == r.MasterAccount?.Name)
                    {
                        isLeaderAcct = true;
                        break;
                    }
                }
                if (isLeaderAcct)
                    _lastLeaderDirection.TryRemove(instr, out _);
            }

            bool hasEntries = HasWorkingEntries(e.Order.Account, e.Order.Instrument);
            PositionStateChanged?.Invoke(instr, new PositionState(hasPos, hasEntries));
        }

        // DW-B135 test accessors -- no logic, thin shims only.
        internal void TryFirePositionState_ForTest(OrderEventArgs e) => TryFirePositionState(e);

        internal bool HasLeaderDirection(string instrFullName) =>
            _lastLeaderDirection.ContainsKey(instrFullName);

        internal void SetLeaderDirection_ForTest(string instrFullName, OrderAction action) =>
            _lastLeaderDirection[instrFullName] = action;

        internal ConcurrentDictionary<string, OrderAction> TestOnly_LastLeaderDirection =>
            _lastLeaderDirection;

        // HOTFIX-B66-COPY-REPLACE / HOTFIX-B66-NATIVE-ATM: re-places a cancelled follower entry order.
        // Called from pre-Gate-1 block when follower "PTT-Copy" or "Entry" is cancelled while leader is long.
        // Finds the copy rule where cancelledOrder.Account is a follower, verifies the leader
        // still has an open position (ATM-sweep cancel, not a user-initiated close cancel),
        // then re-fires SendCopy or SendCopyWithAtm at the same LimitPrice.
        // Named mode (Clone ATM): uses SendCopyWithAtm so re-placed order arms native ATM brackets.
        // All other modes: uses SendCopy ("PTT-Copy" bare Limit).
        // CYC=8: (1) !_isCopyEnabled guard, (2) foreach rules, (3) follower match loop,
        //        (4) follower found check, (5) leader hasOpenPosition check,
        //        (5b) follower hasOpenPosition guard (DW-B84-02: blocks NT8-internal ATM-arming cancel loop),
        //        (6) HasWorkingPttCopy check, (7) Named-mode branch.
        // JS-021: no lock. JS-001: no throw. JS-002: no return null (void).
        private void ReplaceFollowerCopyOnAtmCancel(Order cancelledOrder)
        {
            if (!_isCopyEnabled)
                return; // (1)
            CopyRule? matchedRule = null;
            int followerIndex = -1;
            foreach (var rule in _rules) // (2)
            {
                if (rule.Instrument != cancelledOrder.Instrument.FullName)
                    continue;
                for (int i = 0; i < (rule.FollowerAccounts?.Length ?? 0); i++) // (3)
                {
                    if (rule.FollowerAccounts[i]?.Name == cancelledOrder.Account.Name)
                    {
                        matchedRule = rule;
                        followerIndex = i;
                        break;
                    }
                }
                if (matchedRule.HasValue)
                    break;
            }
            if (!matchedRule.HasValue || followerIndex < 0)
                return; // (4) not a follower order
            var leader = matchedRule.Value.MasterAccount;
            if (leader == null)
                return;
            if (!HasOpenPosition(leader, cancelledOrder.Instrument))
                return; // (5) leader flat = normal close cancel, skip
            if (HasOpenPosition(cancelledOrder.Account, cancelledOrder.Instrument))
                return; // (5b) DW-B84-02: follower already has position = NT8-internal ATM-arming cancel, not a sweep; skip re-place to prevent infinite loop
            if (HasWorkingPttCopy(cancelledOrder.Account, cancelledOrder.Instrument))
                return; // (6) drag-cancel: replacement already in flight
            // Leader has open position, follower has NO position, no replacement in flight -- genuine ATM-sweep cancel, re-place the entry.
            int qty = (int)cancelledOrder.Quantity; // original qty already includes multiplier
            var signal = CopySignal.Create(
                cancelledOrder.OrderAction,
                OrderType.Limit,
                qty,
                cancelledOrder.LimitPrice,
                cancelledOrder.OrderId.ToString() + "-R"
            ); // "-R" suffix = replacement, avoids dedup collision
            var mode = ResolveAtmMode(matchedRule.Value, cancelledOrder.Account.Name);
            if (mode is FollowerAtmMode.Named namedAtm) // (7) Named -> native ATM re-place
                SendCopyWithAtm(cancelledOrder.Account, cancelledOrder.Instrument, in signal, namedAtm); // (8)
            else
                SendCopy(cancelledOrder.Account, cancelledOrder.Instrument, in signal, mode); // (8)
            StatusUpdate?.Invoke(
                cancelledOrder.Account.Name
                    + ": re-placed @ "
                    + cancelledOrder.LimitPrice
                    + " (ATM-sweep replace)"
            );
        }

        // DW-B79-08 v6: TryReplacePttBeBrackets -- register BE retry slot + 500ms fallback for ATM-sweep recovery.
        // Called from OnOrderUpdate when PTT-BE-Stop-* transitions to Cancelled while follower has position.
        // Root cause: StartAtmStrategy fires NT8's internal cancel sweep which wipes ALL working orders
        // including PTT-BE brackets placed by a prior BE-ALL.
        //
        // v1 FAILURE (938f0faf): called MoveStopToBreakEven directly -> runaway storm.
        // v2 FAILURE: slot+200ms timer -> timer consumed slot before Target1 Working.
        // v3 FAILURE: attempt guard stopped storm but 200ms timer still raced Target1 Working.
        //   Root: timer fires at 200ms with targets=0 (ATM still arming) -> places bare PTT-BE-Stop
        //   -> swept again -> attempt 2/3. Cycle repeats.
        // v4 FAILURE: added Target1..Target9 trigger to TryFireFollowerBeRetry; 200ms still raced.
        // v5 FAILURE: removed QueueBeRetryFallback entirely (slot-only).
        //   Root: new ATM brackets go Working BEFORE PTT-BE-Stop cancel arrives in OnOrderUpdate.
        //   TryFireFollowerBeRetry fires at t+0 (no slot) -> misses. Slot registered at t+1 (too late).
        //   Target1 already past Working state -- event not re-fired. Slot sits forever, no recovery.
        //   Confirmed by output: [BE-DIAG] attempt 1/3 ... no [BE-RETRY] Target1 Working ever appears.
        //
        // v6 FIX: slot-only registration + 500ms fallback.
        //   Event ordering: new ATM Target1 goes Working BEFORE PTT-BE-Stop cancel arrives.
        //   By 500ms the ATM is fully settled (not still arming as in v2/v3 at 200ms).
        //   TryFireFollowerBeRetry still claims the slot if somehow Target1 fires after slot
        //   registration (QX path or slow NT8). TryRemove atomic gate: exactly one path wins.
        //   500ms > ATM arming time (~50-100ms) so MoveStopToBreakEven(isRetry:true) sees Target1 Working.
        //
        // DW-B92: race-free alternative to HasFilledBeTarget.
        // Uses synchronous counter incremented in OnOrderUpdate before OCO cancel arrives.
        // CYC=2: (1) null guard, (2) count check. JS-021: no lock. ASCII-only.
        private bool HasFilledBeTargetFast(Account acc)
        {
            if (acc == null)
                return false; // (1)
            _filledBeTargetCount.TryGetValue(acc.Name, out int count);
            return count > 0; // (2)
        }

        // CYC=7: (1) null guard, (2) follower guard, (3) flat guard, (3b) qxCancelInProgress guard,
        // (3c) PTT-QX presence check DW-B112, (4) attempt guard DW-B111 cap=5, (5) slot+fallback.
        // JS-021: ConcurrentDictionary ops are lock-free. acc.Orders read is NT8-safe from OnOrderUpdate.
        // JS-001: no throw. JS-002: void. ASCII-only. DW-B111: cap raised 3->5. DW-B112: Option 2.
        // DW-T4: structurally unreachable from follower path. Followers use acc.Change() (early
        // return at follower block end, L2791) and never hold PTT-BE-Stop-* orders. No guard needed.
        private void TryReplacePttBeBrackets(Order cancelledStop)
        {
            if (cancelledStop?.Account == null || cancelledStop.Instrument == null)
                return; // (1)
            if (!IsFollowerAccount(cancelledStop.Account))
                return; // (2)
            if (IsFlat(FindPosition(cancelledStop.Account, cancelledStop.Instrument)))
                return; // (3)
            // (3b) DW-B105: QX-ALL intent-guard. If QX-ALL is actively cancelling BE brackets
            // on this account, skip ATM-sweep recovery -- QX-ALL will submit PTT-QX-* brackets.
            if (_qxCancelInProgress.ContainsKey(cancelledStop.Account.Name))
                return;
            var acc = cancelledStop.Account;
            var instr = cancelledStop.Instrument;
            // (3c) DW-B112: structural PTT-QX presence check. If any PTT-QX-* orders are Working
            // or Submitted for this account+instrument, QX-ALL has already protected the position.
            // Skip ATM-sweep recovery to prevent PTT-BE brackets firing on top of PTT-QX brackets.
            // Timing-independent: does not rely on _qxCancelInProgress guard window.
            // W1 resolved: .ToList() snapshot used for consistency with L2414 safety pattern.
            if (
                acc
                    .Orders.ToList()
                    .Any(o =>
                        o.Name.StartsWith("PTT-QX-", StringComparison.Ordinal)
                        && (
                            o.OrderState == OrderState.Working
                            || o.OrderState == OrderState.Submitted
                        )
                        && o.Instrument?.FullName == instr.FullName
                    )
            )
            {
                NinjaTrader.Code.Output.Process(
                    "[BE-DIAG] TryReplacePttBeBrackets: "
                        + acc.Name
                        + " -- PTT-QX orders Working/Submitted, skipping recovery (DW-B112)",
                    NinjaTrader.NinjaScript.PrintTo.OutputTab1
                );
                return;
            }
            // (4) Attempt-count guard: max 5 slot registrations per trade per account.
            _beReplaceAttempts.TryGetValue(acc.Name, out int prevAttempts);
            if (prevAttempts >= 5) // (4) DW-B111: cap raised to 5 (3x500ms insufficient for partial-target retry)
            {
                NinjaTrader.Code.Output.Process(
                    "[BE-DIAG] TryReplacePttBeBrackets: "
                        + acc.Name
                        + " -- max 5 attempts, no new slot (TryFireFollowerBeRetry still holds slot "
                        + prevAttempts
                        + ")",
                    NinjaTrader.NinjaScript.PrintTo.OutputTab1
                );
                return;
            }
            _beReplaceAttempts[acc.Name] = prevAttempts + 1;
            // (5) Register slot + 500ms fallback timer.
            // The new ATM Target1 goes Working BEFORE the PTT-BE-Stop cancel arrives (event ordering).
            // TryFireFollowerBeRetry fires at t+0 but finds no slot yet -- event-driven path misses.
            // 500ms fallback fires after ATM is fully settled and claims the slot via TryRemove.
            // If event-driven path wins (QX path, slow NT8), fallback TryRemove returns false -> no-op.
            if (!_pendingFollowerBeSlots.TryAdd(acc.Name, new PendingFollowerBeSlot(acc, instr, 0)))
                return;
            NinjaTrader.Code.Output.Process(
                "[BE-DIAG] TryReplacePttBeBrackets: "
                    + acc.Name
                    + " -- attempt "
                    + (prevAttempts + 1)
                    + "/5, slot registered, 500ms fallback queued",
                NinjaTrader.NinjaScript.PrintTo.OutputTab1
            );
            QueueBeRetryFallback(acc, instr, 0, delayMs: 500);
        }

        // B113 DW-B117: cancel-after cleanup. Called from OnOrderUpdate when any order
        // transitions to Working or Accepted. Cancels the native ATM Target* bracket that
        // corresponds to the PTT-QX-T* order that just confirmed, on follower accounts only.
        // DW-B122: PTT AddOn orders (account.CreateOrder + Submit) arrive as Accepted first
        // in NT8 SIM -- Working may never fire for them before OCO cancels the native bracket.
        // Guard now accepts both Working and Accepted, consistent with TryFireFollowerBeRetry
        // (CopyEngine.cs L1365-1368) which already handles the same order type.
        // CYC=5: (1) outer guard, (2) foreach, (3) if found, (4) if shouldRemove.
        // JS-021: no lock() -- ConcurrentDictionary TryGetValue/TryRemove.
        // JS-001: no throw. ASCII-only string literals. NT8-007: CancelOrder (not CreateOrder).
        internal void TryCleanupReArmedAtmBracket(OrderEventArgs e)
        {
            // (1) Compound guard -- all conditions must be true.
            // a. Order just went Working or Accepted (DW-B122: AddOn Limit orders arrive
            //    as Accepted first; guard accepts both states, same as TryFireFollowerBeRetry).
            // b. Name matches PTT-QX-T* pattern (PTT-QX-T1, T2, T3).
            // c. Account is a follower.
            // d. Cleanup entry exists for this account.
            // e. TTL has not elapsed.
            // f. Instrument matches the cleanup entry.
            if (
                (
                    e.Order.OrderState != OrderState.Working
                    && e.Order.OrderState != OrderState.Accepted
                )
                || e.Order.Name == null
                || !e.Order.Name.StartsWith("PTT-QX-T", StringComparison.Ordinal)
                || e.Order.Name.Length < 9
                || !char.IsDigit(e.Order.Name[8])
                || e.Order.Account == null
                || !IsFollowerAccount(e.Order.Account)
                || !_qxPendingFollowerCleanup.TryGetValue(e.Order.Account.Name, out var entry)
                || entry.Expiry <= DateTime.UtcNow
                || entry.Instr?.FullName != e.Order.Instrument?.FullName
            )
                return;

            char tChar = e.Order.Name[8]; // '1', '2', or '3'
            string nativeName = "Target" + tChar; // "Target1", "Target2", "Target3"
            var acc = e.Order.Account;

            // (2) Find the matching native ATM bracket on this account+instrument.
            // .ToList() snapshot -- consistent with acc.Orders read pattern at L2327.
            Order? toCancel = null;
            foreach (var o in acc.Orders.ToList()) // (2)
            {
                if (
                    o.Name == nativeName
                    && o.Instrument?.FullName == entry.Instr.FullName
                    && (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted)
                )
                {
                    toCancel = o;
                    break;
                }
            }

            // (3) Cancel if found.
            if (toCancel != null) // (3)
            {
                acc.Cancel(new Order[] { toCancel });
                NinjaTrader.Code.Output.Process(
                    "[PTT-QX-CLEANUP] "
                        + acc.Name
                        + " cancelled "
                        + nativeName
                        + " (cancel-after DW-B117)",
                    NinjaTrader.NinjaScript.PrintTo.OutputTab1
                );
            }

            // (4) Removal policy: remove entry when T3 is processed (last bracket) or TTL elapsed.
            // T1 and T2 leave the entry in place so T2/T3 cleanups can fire.
            bool shouldRemove = tChar == '3' || entry.Expiry <= DateTime.UtcNow; // (4)
            if (shouldRemove)
                _qxPendingFollowerCleanup.TryRemove(acc.Name, out _);
        }

        // CYC=2. Thin wrapper over FindPosition.
        private bool HasOpenPosition(Account acc, Instrument instrument)
        {
            var pos = FindPosition(acc, instrument); // (1) branch
            if (pos == null)
                return false;
            return pos.Quantity > 0;
        }

        // B65 T1 / DW-B91-B: TryDispatchLeaderFlat -- CYC=6 (strict McCabe after DW-B91-B extraction).
        // (1) state guard, (2) follower guard, (3) open-position race-safe guard, (4) foreach follower.
        // DW-B91-B: foreach body extracted to FlattenFollower (CYC=3) which adds per-follower
        // hasOpenPosition guard to skip already-flat followers. Null guard moved into FlattenFollower.
        // Guard (3) change: bypass hasOpenPosition when orderName is a native NT8 exit.
        // Rationale: NT8_FULL_REFERENCE.md line 1721 -- position state is not updated until the next
        // OnBarUpdate() after an order fill. When leader fills a native close order (Name="Close",
        // "Flatten", "Exit*", "Rev*"), position still shows open even though the order is filled.
        // Bypassing the guard here ensures followers are flattened immediately (DW-B65-01 fix).
        // JS-021: no lock. JS-001: no throw. JS-002: no null return.
        private static bool TryDispatchLeaderFlat(
            Account account,
            Instrument instrument,
            OrderState state,
            string orderName,
            CopyRule rule,
            Func<Account, bool> isFollower,
            Func<Account, Instrument, bool> hasOpenPosition,
            Action<Account, Instrument> flattenOne
        )
        {
            if (state != OrderState.Filled && state != OrderState.Cancelled)
                return false; // (1)
            if (isFollower(account))
                return false; // (2)
            if (IsNonFlatDispatchName(orderName))
                return false; // (2.5+2.6) combines HOTFIX-B63-FLATTEN-01 + HOTFIX-B64-ENTRY-FLATTEN-01
            if (!IsNativeExitName(orderName) && hasOpenPosition(account, instrument))
                return false; // (3)
            foreach (var acc in rule.FollowerAccounts) // (4)
                FlattenFollower(acc, instrument, hasOpenPosition, flattenOne); // DW-B91-B
            return true;
        }

        // DW-B91-B: extracted foreach body from TryDispatchLeaderFlat.
        // Absorbs (a) null guard (moved from caller loop) and (b) new per-follower open-position guard.
        // Prevents spurious flattenOne call on already-flat followers (re-entry bug).
        // CYC=3: 1 base + if (acc == null) + if (!hasOpenPosition).
        // JS-021: no lock. JS-001: no throw. JS-002: no null return (void).
        // private static: no instance state captured -- explicit delegate injection for testability.
        private static void FlattenFollower(
            Account acc,
            Instrument instrument,
            Func<Account, Instrument, bool> hasOpenPosition,
            Action<Account, Instrument> flattenOne
        )
        {
            if (acc == null)
                return; // (a) null guard (moved from caller)
            if (!hasOpenPosition(acc, instrument))
                return; // (b) DW-B91-B: skip already-flat follower
            flattenOne(acc, instrument);
        }

        // CYC=3. Returns true if any working non-bracket order exists for the instrument.
        private bool HasWorkingEntries(Account acc, Instrument instrument)
        {
            foreach (var order in acc.Orders) // (1) branch
            {
                if (order.Instrument != instrument) // (1) branch
                    continue;
                if (order.OrderState != OrderState.Working) // (1) branch
                    continue;
                if (!IsBracketLeg(order))
                    return true;
            }
            return false;
        }

        // HOTFIX-B66-COPY-REPLACE-FIX: discriminates ATM-sweep cancel from entry-drag cancel.
        // Entry drag: HandleEntryChange places a new PTT-Copy (Working/Accepted/Submitted) before
        // this Cancelled event arrives in OnOrderUpdate. ATM-sweep: all follower orders wiped,
        // nothing is Working/Accepted/Submitted for this account+instrument after the sweep.
        // CYC=3: foreach(1) + state check(2) + name check(3).
        // NT8: FullName string compare (reference equality banned -- HOTFIX-BUG-BE-INSTRUMENT-REF).
        // JS-021: no lock. acc.Orders.ToList() snapshot prevents InvalidOperationException.
        private bool HasWorkingPttCopy(Account acc, Instrument instrument)
        {
            foreach (var order in acc.Orders.ToList())
            {
                if (order.Instrument?.FullName != instrument.FullName)
                    continue;
                if (
                    order.OrderState != OrderState.Working
                    && order.OrderState != OrderState.Accepted
                    && order.OrderState != OrderState.Submitted
                )
                    continue;
                if (order.Name == "PTT-Copy" || order.Name == "Entry")
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
        private bool SendCopy(
            Account follower,
            Instrument instrument,
            in CopySignal signal,
            FollowerAtmMode mode
        )
        {
            OrderType orderType = signal.Type;
            double limitPrice = signal.LimitPrice;
            string signalName = "PTT-Copy"; // SCAN-05: PTT- prefix mandatory for ALL modes

            if (mode is FollowerAtmMode.Market) // branch (1)
            {
                orderType = OrderType.Market;
                limitPrice = 0;
            }
            // Inherit: use original signal values unchanged (no branch needed)

            string atmTemplate = mode is FollowerAtmMode.Named named // branch (2)
                ? named.TemplateName
                : null;

            try // branch (3)
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
                    TimeInForce.Gtc, // B29 fix: Day orders expire mid-session on overnight futures
                    signal.Quantity,
                    limitPrice,
                    0,
                    null,
                    signalName,
                    DateTime.MaxValue,
                    (NinjaTrader.Cbi.CustomOrder)null
                );
                if (order != null)
                {
                    follower.Submit(new[] { order });
                    RecordFollowerCopy(signal.OrderId, order); // DW-B136 Gap B: track follower order by leader ID
                }
                return true;
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke("PTT-Copy error: " + ex.Message);
                return false;
            }
        }

        // HOTFIX-B66-NATIVE-ATM: SendCopyWithAtm -- submits follower entry order with native NT8 ATM.
        // Called from DispatchCopy (Named mode) and ReplaceFollowerCopyOnAtmCancel (Named mode).
        // Uses StartAtmStrategy (static, callable from AddOnBase -- confirmed NT8_FULL_REFERENCE.md).
        // NT8 CONSTRAINT: order name MUST be "Entry" for StartAtmStrategy to arm brackets.
        // NT8 CONSTRAINT: StartAtmStrategy handles submission -- do NOT call follower.Submit() after.
        // HOTFIX-B66-ATM-OBJ: AtmObject overload preferred -- passes live object, avoids .Name class-name trap.
        // CYC=4: (1) try/catch outer, (2) order null guard, (3) AtmObject branch, (4) string branch.
        // JS-021: no lock. JS-001: catch logs, returns false. JS-002: no return null (void return path).
        private bool SendCopyWithAtm(
            Account follower,
            Instrument instrument,
            in CopySignal signal,
            FollowerAtmMode.Named namedMode
        )
        {
            try // (1)
            {
                var order = follower.CreateOrder(
                    instrument,
                    signal.Action,
                    OrderType.Limit,
                    OrderEntry.Manual,
                    TimeInForce.Gtc,
                    signal.Quantity,
                    signal.LimitPrice,
                    0,
                    string.Empty,
                    "Entry", // NT8: MUST be "Entry" for StartAtmStrategy to arm
                    DateTime.MaxValue,
                    (NinjaTrader.Cbi.CustomOrder)null
                );
                if (order == null)
                    return false; // (2)
                if (namedMode.AtmObject != null) // (3) preferred: object overload
                    NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(namedMode.AtmObject, order);
                else // (4) fallback: string overload
                    NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(namedMode.TemplateName, order);
                RecordFollowerCopy(signal.OrderId, order); // DW-B136 Gap B
                StatusUpdate?.Invoke(
                    follower.Name
                        + ": PTT-ATM entry @ "
                        + signal.LimitPrice
                        + " atm="
                        + namedMode.TemplateName
                );
                return true;
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke("PTT-ATM error: " + ex.Message);
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
            if (GetCopyMode() == CopyMode.Clone) // branch (1)
                return GetCloneAtmMode();
            return GetAtmMode(rule, accountName);
        }

        // B8 T2: ParseAtmModeName -- deserializes "Inherit"|"Market"|"Named:XXX" to FollowerAtmMode.
        // CYC=3. Returns Inherit for null/empty/unrecognized input -- never null, never throws.
        internal static FollowerAtmMode ParseAtmModeName(string name)
        {
            if (string.IsNullOrEmpty(name)) // branch (1)
                return new FollowerAtmMode.Inherit();
            if (name == "Market") // branch (2)
                return new FollowerAtmMode.Market();
            if (name.StartsWith("Named:")) // branch (3)
                return new FollowerAtmMode.Named(name.Substring(6));
            return new FollowerAtmMode.Inherit();
        }

        // B8 T2: AtmModeToString -- serializes FollowerAtmMode to "Inherit"|"Market"|"Named:XXX".
        // CYC=3. Sealed hierarchy is exhaustive -- all variants covered.
        internal static string AtmModeToString(FollowerAtmMode mode)
        {
            if (mode is FollowerAtmMode.Market) // branch (1)
                return "Market";
            if (mode is FollowerAtmMode.Named namedMode) // branch (2)
                return "Named:" + namedMode.TemplateName;
            return "Inherit"; // branch (3): Inherit or null-fallback
        }

        // B8 T2: SetAtmMode -- post-create mutation of a single follower's ATM mode.
        // ConcurrentBag rebuild pattern -- no lock (JS-021).
        // ImmutableDictionary.SetItem returns a NEW dictionary (immutable -- no mutation).
        internal void SetAtmMode(
            string instrument,
            string followerAccountName,
            FollowerAtmMode mode
        )
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
                _rules.Add(
                    CopyRule.Create(
                        r.Instrument,
                        r.MasterAccount,
                        r.FollowerAccounts,
                        r.Enabled,
                        r.FollowerMultipliers,
                        newMap,
                        r.TightenTicks,
                        r.FollowerAccountNames // B127: preserve names through ATM mode rebuild
                    )
                );
            }
        }

        internal void Trim(Instrument instrument)
        {
            if (!_flags.TrimFlatten)
            {
                StatusUpdate?.Invoke("Trim/Flatten requires Pro tier");
                return;
            }
            foreach (var acc in AllAccounts(instrument))
                TrimOneAccount(acc, instrument);
        }

        internal void Flatten(Instrument instrument)
        {
            if (!_flags.TrimFlatten)
            {
                StatusUpdate?.Invoke("Trim/Flatten requires Pro tier");
                return;
            }
            foreach (var acc in AllAccounts(instrument))
                FlattenOneAccount(acc, instrument);
        }

        // B28 T1 -- Trim(Account,Instrument): leader-account overload. Fixes DW-B28-02.
        // CYC=4: (1) leader null guard, (2) leader direct call, (3) foreach, (4) acc==leader skip. BGTM-1: TrimFlatten gate CYC=5.
        internal void Trim(Account leader, Instrument instrument)
        {
            if (!_flags.TrimFlatten)
            {
                StatusUpdate?.Invoke("Trim/Flatten requires Pro tier");
                return;
            }
            if (leader == null)
            {
                StatusUpdate?.Invoke("PTT-Trim: leader null -- skipping");
                return;
            }
            TrimOneAccount(leader, instrument);
            foreach (var acc in AllAccounts(instrument))
            {
                if (acc == leader)
                    continue;
                TrimOneAccount(acc, instrument);
            }
        }

        // B28 T1 -- Flatten(Account,Instrument): leader-account overload. Fixes DW-B28-02.
        // CYC=4: (1) leader null guard, (2) leader direct call, (3) foreach, (4) acc==leader skip. BGTM-1: TrimFlatten gate CYC=5.
        internal void Flatten(Account leader, Instrument instrument)
        {
            if (!_flags.TrimFlatten)
            {
                StatusUpdate?.Invoke("Trim/Flatten requires Pro tier");
                return;
            }
            if (leader == null)
            {
                StatusUpdate?.Invoke("PTT-Flatten: leader null -- skipping");
                return;
            }
            FlattenOneAccount(leader, instrument);
            foreach (var acc in AllAccounts(instrument))
            {
                if (acc == leader)
                    continue;
                FlattenOneAccount(acc, instrument);
            }
        }

        // B28 T1 -- CancelPendingEntries(Account,Instrument): leader-account overload. Fixes DW-B28-02.
        // CYC=4: (1) leader null guard, (2) leader direct call, (3) foreach, (4) acc==leader skip. BGTM-1: TrimFlatten gate CYC=5.
        internal void CancelPendingEntries(Account leader, Instrument instrument)
        {
            if (!_flags.TrimFlatten)
            {
                StatusUpdate?.Invoke("Trim/Flatten requires Pro tier");
                return;
            }
            if (leader == null)
            {
                StatusUpdate?.Invoke("PTT-Cancel: leader null -- skipping");
                return;
            }
            CancelOneAccount(leader, instrument);
            foreach (var acc in AllAccounts(instrument))
            {
                if (acc == leader)
                    continue;
                CancelOneAccount(acc, instrument);
            }
        }

        // B28 T1 -- Trim(Account,Instrument,int,double,double): leader-account limit overload.
        // CYC=5: (1) leader null guard, (2) ask/bid/buffer guard, (3) leader direct call, (4) foreach, (5) acc==leader skip.
        internal void Trim(
            Account leader,
            Instrument instrument,
            int exitBuffer,
            double ask,
            double bid
        )
        {
            if (leader == null)
            {
                StatusUpdate?.Invoke("PTT-TrimLimit: leader null -- skipping");
                return;
            }
            if (ask <= 0 || bid <= 0 || exitBuffer == 0)
            {
                Trim(leader, instrument);
                return;
            }
            TrimOneAccountLimit(leader, instrument, exitBuffer, ask, bid);
            foreach (var acc in AllAccounts(instrument))
            {
                if (acc == leader)
                    continue;
                TrimOneAccountLimit(acc, instrument, exitBuffer, ask, bid);
            }
        }

        // B28 T1 -- Flatten(Account,Instrument,int,double,double): leader-account limit overload.
        // CYC=5: (1) leader null guard, (2) ask/bid/buffer guard, (3) leader direct call, (4) foreach, (5) acc==leader skip.
        internal void Flatten(
            Account leader,
            Instrument instrument,
            int exitBuffer,
            double ask,
            double bid
        )
        {
            if (leader == null)
            {
                StatusUpdate?.Invoke("PTT-FlattenLimit: leader null -- skipping");
                return;
            }
            if (ask <= 0 || bid <= 0 || exitBuffer == 0)
            {
                Flatten(leader, instrument);
                return;
            }
            FlattenOneAccountLimit(leader, instrument, exitBuffer, ask, bid);
            foreach (var acc in AllAccounts(instrument))
            {
                if (acc == leader)
                    continue;
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
            var action =
                pos.MarketPosition == MarketPosition.Long
                    ? OrderAction.Sell
                    : OrderAction.BuyToCover;
            try
            {
                acc.CreateOrder(
                    instrument,
                    action,
                    OrderType.Market,
                    OrderEntry.Manual,
                    TimeInForce.Gtc,
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

        // B28 T1 -- FlattenOneAccount: per-account market flatten helper.
        // B67 DW-B67-01: cancel follower ATM+QX brackets BEFORE submitting market order.
        // B69 DW-B69-01: widened from CancelQxBrackets to cancel ALL active orders (name-agnostic).
        // B76 HOTFIX-B76-FLATTEN-RACE-01: re-read position after CancelAllAccountOrders.
        //   NT8_FULL_REFERENCE.md line 1721: position state lags until next OnBarUpdate after fill.
        //   Pre-cancel read (pos): fast-exit guard -- skip cancel round-trip on already-flat accounts.
        //   Post-cancel read (posAfterCancel): race guard -- after CancelAllAccountOrders round-trips
        //   to NT8 order manager, acc.Positions reflects ATM bracket fills. If posAfterCancel shows
        //   flat, account was closed by bracket fill -- skip PTT-Flatten to prevent inversion.
        // NT8 precedent: @2Custom-0909edcc FlattenPositionByName V8.31 comment:
        //   "Cancel ALL bracket orders first to prevent race conditions."
        // Rithmic/Apex: incoming market order conflicts with live OCO bracket at broker layer
        //   -> "Close operation failed. Operation timed out." without this cancel step.
        // CYC=6: (1) active PTT-Flatten guard, (2) pre-cancel pos null/qty guard,
        //        (3) CancelAllAccountOrders, (4) post-cancel re-read null/qty guard,
        //        (5) action ternary, (6) try/catch.
        // JS-021: no lock. JS-001: no throw in hot path. JS-002: void.
        private void FlattenOneAccount(Account acc, Instrument instrument)
        {
            // B76 HOTFIX-B76-FLATTEN-GUARD-01 v2: order-book guard.
            // Field flag (v1) was cleared in finally before NT8 delivered bracket-cancel callbacks.
            // Root cause: CancelAllAccountOrders returns immediately; bracket-cancel acks fire on
            // the NT8 account thread after finally runs, re-entering with the flag already cleared.
            // Fix: scan acc.Orders for an active PTT-Flatten -- NT8 order book is the authoritative
            // in-flight signal. An active flatten is already working; subsequent cancel-ack callbacks
            // skip. acc.Orders.ToList() snapshot prevents InvalidOperationException (same pattern as
            // HasWorkingPttCopy). JS-021: no lock.
            foreach (var o in acc.Orders.ToList())
            {
                if (o.Name != "PTT-Flatten")
                    continue;
                if (o.Instrument?.FullName != instrument.FullName)
                    continue;
                if (
                    o.OrderState == OrderState.Submitted
                    || o.OrderState == OrderState.Accepted
                    || o.OrderState == OrderState.Working
                )
                {
                    StatusUpdate?.Invoke(acc.Name + ": flat-guard: in-flight skip");
                    return;
                }
            }
            var pos = FindPosition(acc, instrument);
            if (pos == null || pos.Quantity == 0)
            {
                StatusUpdate?.Invoke(acc.Name + ": flat skip");
                return;
            }
            CancelAllAccountOrders(acc, instrument); // B69 DW-B69-01: cancel ALL orders first
            // B76 HOTFIX-B76-FLATTEN-RACE-01: re-read after cancel.
            // ATM bracket fill may have cleared the position while cancel request was in-flight.
            var posAfterCancel = FindPosition(acc, instrument);
            if (posAfterCancel == null || posAfterCancel.Quantity == 0)
            {
                StatusUpdate?.Invoke(acc.Name + ": flat-race skip (pos cleared by bracket fill)");
                return;
            }
            var action =
                posAfterCancel.MarketPosition == MarketPosition.Long
                    ? OrderAction.Sell
                    : OrderAction.BuyToCover;
            try
            {
                var order = acc.CreateOrder(
                    instrument,
                    action,
                    OrderType.Market,
                    OrderEntry.Manual,
                    TimeInForce.Gtc,
                    posAfterCancel.Quantity,
                    0,
                    0,
                    null,
                    "PTT-Flatten",
                    DateTime.MaxValue,
                    null
                );
                if (order != null)
                    acc.Submit(new[] { order });
                StatusUpdate?.Invoke(acc.Name + ": flatten " + posAfterCancel.Quantity);
            }
            catch (Exception ex)
            {
                StatusUpdate?.Invoke("PTT-Flatten error: " + ex.Message);
            }
        }

        // B29 fix -- ComputeLimitPx: aggressive exit anchor.
        // Long exits (Sell Limit) post at bid - buffer (at/below market -> fills immediately).
        // Short exits (BuyToCover) post at ask + buffer (at/above market -> fills immediately).
        // DW-B29-01: original used ask+buffer for long, placing passive limit ABOVE market (never filled).
        // CYC=1: single ternary. No NT8 deps, no state, no nulls.
        // internal static -- CopyEngineTests.cs calls CopyEngine.ComputeLimitPx(...) directly.
        internal static double ComputeLimitPx(
            bool isLong,
            double ask,
            double bid,
            int exitBuffer,
            double tickSize
        ) => isLong ? bid - exitBuffer * tickSize : ask + exitBuffer * tickSize;

        // B19 T1 -- Trim 4-arg: exit half position at limit price anchored to ask (long) or bid (short).
        // Long: Sell Limit @ ask + exitBuffer*tick.   Short: BuyToCover @ bid - exitBuffer*tick.
        // NT8-007: arg 12 = (NinjaTrader.Cbi.CustomOrder)null.
        // NT8-014: signal name = "PTT-TrimLimit".
        // NT8-032: ask/bid are MarketDataEventArgs.Price doubles (callers obtain via GetAsk()/GetBid()).
        // CYC=6: (1+2) compound ask/bid guard, (3) exitBuffer guard, (4) foreach, (5+6) pos null||qty guard.
        // JS-001: try/catch wraps acc.CreateOrder -- no rethrow. BGTM-1: TrimFlatten gate CYC=7.
        internal void Trim(Instrument instrument, int exitBuffer, double ask, double bid)
        {
            if (!_flags.TrimFlatten)
            {
                StatusUpdate?.Invoke("Trim/Flatten requires Pro tier");
                return;
            }
            if (ask <= 0 || bid <= 0 || exitBuffer == 0)
            {
                Trim(instrument);
                return;
            }
            foreach (var acc in AllAccounts(instrument))
                TrimOneAccountLimit(acc, instrument, exitBuffer, ask, bid);
        }

        // B19 T1 -- Flatten 4-arg: exit full position at limit price anchored to ask (long) or bid (short).
        // NT8-007: arg 12 = (NinjaTrader.Cbi.CustomOrder)null.
        // NT8-014: signal name = "PTT-FlattenLimit". BGTM-1: TrimFlatten gate CYC+=1.
        internal void Flatten(Instrument instrument, int exitBuffer, double ask, double bid)
        {
            if (!_flags.TrimFlatten)
            {
                StatusUpdate?.Invoke("Trim/Flatten requires Pro tier");
                return;
            }
            if (ask <= 0 || bid <= 0 || exitBuffer == 0)
            {
                Flatten(instrument);
                return;
            }
            foreach (var acc in AllAccounts(instrument))
                FlattenOneAccountLimit(acc, instrument, exitBuffer, ask, bid);
        }

        // BGTM-1: TrimFlatten gate CYC=3.
        internal void CancelPendingEntries(Instrument instrument)
        {
            if (!_flags.TrimFlatten)
            {
                StatusUpdate?.Invoke("Trim/Flatten requires Pro tier");
                return;
            }
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
                if (order.Instrument != instrument)
                    continue;
                // B18 T3: DW-B18-CANCEL-01 -- also cancel Initialized orders.
                if (
                    order.OrderState != OrderState.Working
                    && order.OrderState != OrderState.Initialized
                )
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

        // HOTFIX-F4 -- CancelStaleExitOrders: cancels any working PTT limit exit orders by signal name.
        // Called by TrimOneAccountLimit and FlattenOneAccountLimit before posting a new limit.
        // Prevents stale PTT-TrimLimit/PTT-FlattenLimit orders competing with ATM Close or
        // a second button click, which caused "Close operation timed out" popup in NT8.
        // CYC=3: foreach(1), name filter(2), try/catch(3). JS-021: ToList() snapshot.
        private void CancelStaleExitOrders(Account acc, Instrument instrument, string signalName)
        {
            foreach (var order in acc.Orders.ToList())
            {
                if (order.Instrument != instrument)
                    continue; // (1)
                if (order.Name != signalName)
                    continue; // (2)
                if (
                    order.OrderState != OrderState.Working
                    && order.OrderState != OrderState.Initialized
                )
                    continue;
                try // (3)
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
        private void TrimOneAccountLimit(
            Account acc,
            Instrument instrument,
            int exitBuffer,
            double ask,
            double bid
        )
        {
            CancelStaleExitOrders(acc, instrument, "PTT-TrimLimit"); // HOTFIX-F4
            var pos = FindPosition(acc, instrument);
            if (pos == null || pos.Quantity == 0)
            {
                StatusUpdate?.Invoke(acc.Name + ": flat skip");
                return;
            }
            int trimQty = (int)Math.Ceiling(pos.Quantity / 2.0);
            bool isLong = pos.MarketPosition == MarketPosition.Long;
            var action = isLong ? OrderAction.Sell : OrderAction.BuyToCover;
            double tickSize = instrument.MasterInstrument.TickSize;
            double limitPx = ComputeLimitPx(isLong, ask, bid, exitBuffer, tickSize);
            try
            {
                acc.CreateOrder(
                    instrument,
                    action,
                    OrderType.Limit,
                    OrderEntry.Manual,
                    TimeInForce.Gtc,
                    trimQty,
                    limitPx,
                    0,
                    null,
                    "PTT-TrimLimit",
                    DateTime.MaxValue,
                    (NinjaTrader.Cbi.CustomOrder)null
                );
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
        private void FlattenOneAccountLimit(
            Account acc,
            Instrument instrument,
            int exitBuffer,
            double ask,
            double bid
        )
        {
            CancelStaleExitOrders(acc, instrument, "PTT-FlattenLimit"); // HOTFIX-F4
            var pos = FindPosition(acc, instrument);
            if (pos == null || pos.Quantity == 0)
            {
                StatusUpdate?.Invoke(acc.Name + ": flat skip");
                return;
            }
            bool isLong = pos.MarketPosition == MarketPosition.Long;
            var action = isLong ? OrderAction.Sell : OrderAction.BuyToCover;
            double tickSize = instrument.MasterInstrument.TickSize;
            double limitPx = ComputeLimitPx(isLong, ask, bid, exitBuffer, tickSize);
            try
            {
                acc.CreateOrder(
                    instrument,
                    action,
                    OrderType.Limit,
                    OrderEntry.Manual,
                    TimeInForce.Gtc,
                    pos.Quantity,
                    limitPx,
                    0,
                    null,
                    "PTT-FlattenLimit",
                    DateTime.MaxValue,
                    (NinjaTrader.Cbi.CustomOrder)null
                );
                StatusUpdate?.Invoke(
                    acc.Name + ": flatten-limit " + pos.Quantity + " @ " + limitPx
                );
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

        // DW-B91-A: guard -- returns true if this orderId was already dispatched (blocks re-dispatch).
        // Side-effect on first call: TryAdd records the orderId as dispatched.
        // CYC=2: 1 base + 1 if (ContainsKey).
        // JS-021: ContainsKey + TryAdd are lock-free. JS-001: no throw. JS-002: returns bool.
        private bool IsEntryDispatched(string orderId)
        {
            if (_entryDispatchedOrders.ContainsKey(orderId))
                return true;
            _entryDispatchedOrders.TryAdd(orderId, 0);
            return false;
        }

        // B62: evict dedup entry when order reaches terminal state (Filled/Cancelled/Rejected).
        // Called unconditionally from OnOrderUpdate pre-gate, after TryFirePositionState.
        // Ensures evicted orderId can be re-used for the next fresh order on the same instrument.
        // CYC=2: terminal-state guard (1) + TryRemove (no branch).
        // JS-025: ConcurrentDictionary.TryRemove is lock-free.
        internal void EvictDedup(string orderId, OrderState state)
        {
            if (
                state != OrderState.Filled
                && state != OrderState.Cancelled
                && state != OrderState.Rejected
            )
                return;

            _dedupCache.TryRemove(orderId, out _);
            if (state == OrderState.Cancelled)
                _entryDispatchedOrders.Clear(); // DW-B101: evict on Cancelled (Filled/Rejected handled by TryEvictFollowerBeSlot)
            // DW-B91-A-v2: eviction moved to TryEvictFollowerBeSlot (position-flat) for Filled/Rejected.
            // Prevents partial-fill re-dispatch: Filled fires before Submitted re-submit on Rithmic.
            // DW-B101: Cancelled eviction of _entryDispatchedOrders handled here (TryEvictFollowerBeSlot misses Cancelled).
        }

        // B127: updated to implement Option A lazy re-resolve (DW-PTT-BE-FIX-01).
        // CYC=7: rule==null(1) + for(1) + acc!=null(1) + names ternary(1) + IsNullOrEmpty(1)
        //         + TryGetValue(1) + resolved!=null(1). Within JS limit of 8.
        // JS-021: no lock -- ConcurrentDictionary.TryGetValue + TryAdd are lock-free.
        // JS-001: no throw -- all paths yield, continue, or emit Output.Process.
        // JS-002: no null values yielded -- null slots are resolved or skipped.
        // ASCII-only strings in all log messages.
        internal IEnumerable<Account> AllAccounts(Instrument instrument)
        {
            var rule = FindRule(instrument);
            if (rule == null)
                yield break;

            yield return rule.Value.MasterAccount;
            var followers = rule.Value.FollowerAccounts;
            var names = rule.Value.FollowerAccountNames;
            for (int i = 0; i < followers.Length; i++)
            {
                var acc = followers[i];
                if (acc != null)
                {
                    yield return acc;
                    continue;
                }
                // B127: lazy re-resolve for slot that was null at load time.
                var name = (names != null && i < names.Length) ? names[i] : null;
                if (string.IsNullOrEmpty(name))
                    continue;
                if (_resolvedFollowers.TryGetValue(name, out var cached))
                {
                    yield return cached;
                    continue;
                }
                var resolved = FindFollowerAccount(name);
                if (resolved != null)
                {
                    _resolvedFollowers.TryAdd(name, resolved);
                    NinjaTrader.Code.Output.Process(
                        "[PTT-COPY] INFO: follower '"
                            + name
                            + "' resolved lazily -- now copying to this account.",
                        NinjaTrader.NinjaScript.PrintTo.OutputTab1
                    );
                    yield return resolved;
                    continue;
                }
                NinjaTrader.Code.Output.Process(
                    "[PTT-COPY] WARNING: follower '"
                        + name
                        + "' not found in Account.All"
                        + " -- account not connected yet; will retry on next dispatch.",
                    NinjaTrader.NinjaScript.PrintTo.OutputTab1
                );
            }
        }

        /// <summary>
        /// Finds the copy rule for the given instrument.
        /// </summary>
        /// <returns>
        /// Matching <see cref="CopyRule"/>, or <c>null</c> if no rule exists for this instrument.
        /// Callers MUST null-check the return value.
        /// </returns>
        internal CopyRule? FindRule(Instrument instrument)
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

        // B119: DW-B128 -- direction-change guard predicate.
        // Returns true iff the current dispatch reverses the last direction AND the follower is flat.
        // CYC=2 (one && expression in a single return). JS-001: no throw. JS-021: no lock. ASCII-only.
        // internal static: directly callable from B119Tests.cs without reflection.
        internal static bool IsReversalToFlatFollower(
            OrderAction currentAction,
            OrderAction lastAction,
            bool followerIsFlat
        )
        {
            return currentAction != lastAction && followerIsFlat;
        }

        // B25 T1 -- DW-B25-01: ATM bracket stops use name format "12s Buy STP".
        // FromEntrySignal is null for ATM orders. No "Stop" prefix. STP suffix is the only discriminator.
        // CYC: 2 + 1 (STP clause) = 3. OrdinalIgnoreCase: consistent with WireLeaderAccount (B24 Lane A).
        private static bool IsStopLeg(Order order)
        {
            return order.FromEntrySignal != null
                || (order.Name != null && order.Name.StartsWith("Stop"))
                || (
                    order.Name != null
                    && order.Name.EndsWith("STP", StringComparison.OrdinalIgnoreCase)
                );
        }

        // Static version of IsBracketLeg for use in static method IsWorkingBracket
        // DW-B134: added STP EndsWith clause -- NT8 ATM stop brackets are named "Buy STP"/"Sell STP".
        // Mirrors IsStopLeg (L3521) which already has this clause. CYC: 3 -> 4.
        private static bool IsBracketLegStatic(Order order)
        {
            return order.FromEntrySignal != null
                || (
                    order.Name != null
                    && (
                        order.Name.StartsWith("Stop")
                        || order.Name.StartsWith("Target")
                        || order.Name.StartsWith("PTT-")
                        || order.Name.EndsWith("STP", StringComparison.OrdinalIgnoreCase)
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
                    && (order.Name.StartsWith("Stop") || order.Name.StartsWith("Target"))
                );
        }

        private Position FindPosition(Account acc, Instrument instrument)
        {
            foreach (Position p in acc.Positions)
                if (p.Instrument != null && p.Instrument.FullName == instrument.FullName)
                    return p;
            return null;
        }

        // B58 -- FindPositionPublic: thin wrapper over private FindPosition for panel access.
        // CYC=1. Returns null if no position (pre-existing FindPosition behavior -- not new).
        // JS-002: null return is pre-existing contract of FindPosition, not a new null-return site.
        internal Position FindPositionPublic(Account acc, Instrument instrument) =>
            FindPosition(acc, instrument);

        // B58 -- SnapshotTargetsPublic: collects Working orders with PTT-QX-T or PTT-TGT- prefix.
        // CYC=3 (1 base + foreach + prefix check). Returns List<Order> -- panel uses .Count.
        // JS-002: never returns null -- returns empty List if no matches.
        // JS-021: acc.Orders iteration; no lock required (NT8 AddOn read-only enumeration).
        internal List<Order> SnapshotTargetsPublic(Account acc, Instrument instr)
        {
            var result = new List<Order>();
            if (acc == null || instr == null)
                return result; // (1) null guard
            foreach (Order o in acc.Orders) // (2) foreach
            {
                if (o.Instrument != instr)
                    continue;
                if (o.OrderState != OrderState.Working)
                    continue;
                string n = o.Name ?? string.Empty;
                if (
                    n.StartsWith(PttOrderNames.PttQxTargetPrefix, StringComparison.Ordinal) // (3) prefix check
                    || n.StartsWith(PttOrderNames.PttTgtPrefix, StringComparison.Ordinal)
                )
                    result.Add(o);
            }
            return result;
        }

        // HOTFIX-MSTBE-CANCEL-RESUBMIT: replaced acc.Change() (silent no-op on ATM brackets) with
        // cancel+resubmit pattern mirroring PttBreakEven.ExecuteOneAccount.
        // Root cause: NT8 ATM engine owns Stop1/Stop2 brackets and ignores acc.Change() from AddOn
        // context -- no exception, no effect. Confirmed: [MSTBE] Change() OK Stop1 logged while
        // stop remained at original price in Orders tab.
        // Pattern source: PttBreakEven.ExecuteOneAccount + CancelStaleBracketsLocal + SubmitBeTargetsLocal.
        // CYC=7: IsFlat(1) + tickSize/pos guard(2) + while-cap(3) + cancel-try(4)
        //        + 0-targets branch(5) + targets-for-loop(6) + partial-retry branch(7).
        // DW-B107: Step A extracted to SnapshotBeTargets; while cap reduces stale residue.
        // JS-021: no lock. JS-001: try/catch per order pair -- no throw in hot path.
        // NT8-049: StopMarket arg6=0, arg7=stopPrice; Limit arg6=limitPrice, arg7=0.
        // NT8-007: arg11=(NinjaTrader.Cbi.CustomOrder)null. NT8-013: DateTime.MaxValue.
        // NT8-014: signal names start with "PTT-". NT8-006: no LINQ.
        // DW-B79-04 isRetry: prevents recursive retry loops.
        // CountLeaderTargets: CYC=4. Returns the number of Working native target limit orders
        // (Target1..Target9, digit 1-9, no PTT- prefix) on the leader account for the given
        // instrument. Working-only (DW-B116: Accepted/Submitted removed -- transitional states
        // cause overcount). Capped at Math.Min(count,3) -- standard ATM max 3 targets.
        // Used by MoveStopToBreakEven to detect partial-target visibility on followers (DW-B79-07).
        // DW-B116 fix: removed PTT-QX-T* and PTT-BE-Target-* from isTarget predicate.
        // JS-021: no lock. JS-001: no throw. JS-002: returns int (never negative). ASCII-only.
        private int CountLeaderTargets(Instrument instrument)
        {
            var rule = FindRule(instrument);
            if (rule == null)
                return 0; // (1)
            var leader = rule.Value.MasterAccount;
            if (leader == null)
                return 0; // (2)
            int count = 0;
            foreach (Order o in leader.Orders) // (3)
            {
                if (o == null)
                    continue;
                bool stateOk = o.OrderState == OrderState.Working;
                bool instrOk = o.Instrument != null && o.Instrument.FullName == instrument.FullName;
                if (!stateOk || !instrOk || o.OrderType != OrderType.Limit)
                    continue;
                bool isTarget =
                    !string.IsNullOrEmpty(o.Name)
                    && o.Name.Length >= 7
                    && o.Name.StartsWith("Target", StringComparison.Ordinal)
                    && char.IsDigit(o.Name[6])
                    && o.Name[6] != '0';
                if (isTarget)
                    count++; // (4)
            }
            return Math.Min(count, 3);
        }

        // CYC=7: null guard(1) + foreach(2) + o==null continue(3) + stateOk(4) + instrOk+type(5)
        //        + if(isNative)(6) + else if(isPtt)(7). JS-002: returns List, never null.
        // JS-021: no lock. JS-001: no throw. ASCII-only.
        // DW-B107: two-pass native-first collect for MoveStopToBreakEven Step A.
        // stateOk is wider than SnapshotTargetOrders (7 states vs 2) per DW-B79-01 + REPAIR-09 DW-B79-05.
        private List<(double Price, int Qty, OrderAction Action)> SnapshotBeTargets(
            Account acc,
            Instrument instrument
        )
        {
            var nativeTargets = new List<(double Price, int Qty, OrderAction Action)>();
            var pttTargets = new List<(double Price, int Qty, OrderAction Action)>();
            if (acc == null || instrument == null)
                return nativeTargets; // (1) JS-002: empty list, never null
            foreach (Order o in acc.Orders) // (2)
            {
                if (o == null)
                    continue; // (3)
                bool stateOk =
                    o.OrderState == OrderState.Working
                    || o.OrderState == OrderState.Accepted
                    || o.OrderState == OrderState.Submitted
                    || o.OrderState == OrderState.Initialized
                    || o.OrderState == OrderState.TriggerPending // (4)
                    || o.OrderState == OrderState.ChangeSubmitted
                    || o.OrderState == OrderState.CancelSubmitted;
                bool instrOk = o.Instrument != null && o.Instrument.FullName == instrument.FullName; // (5)
                if (!stateOk || !instrOk || o.OrderType != OrderType.Limit)
                    continue;
                if (string.IsNullOrEmpty(o.Name))
                    continue;
                bool isNative =
                    o.Name.Length >= 7
                    && o.Name.StartsWith("Target", StringComparison.Ordinal)
                    && char.IsDigit(o.Name[6])
                    && o.Name[6] != '0';
                bool isPtt =
                    (
                        o.Name.StartsWith("PTT-QX-T", StringComparison.Ordinal)
                        && o.Name.Length > 8
                        && char.IsDigit(o.Name[8])
                    ) || o.Name.StartsWith("PTT-BE-Target-", StringComparison.Ordinal);
                if (isNative) // (6)
                    nativeTargets.Add((o.LimitPrice, o.Quantity, o.OrderAction));
                else if (isPtt) // (7)
                    pttTargets.Add((o.LimitPrice, o.Quantity, o.OrderAction));
            }
            return nativeTargets.Count > 0 ? nativeTargets : pttTargets;
        }

        // First call: isRetry=false (default). On targets=0 OR partial targets, one retry queued.
        // Retry call: isRetry=true. No further retry regardless of result.
        // REPAIR-09 DW-B79-05: CancelSubmitted added to Step A stateOk.
        //   PTT-QX-T orders transition Working->CancelSubmitted when follower ATM brackets
        //   arrive async and NT8 cancels them. LimitPrice+Quantity still readable at this state.
        //   Widening captures them as targets before they fully disappear -> OCO pairs submitted.
        // DW-B79-07: partial targets (targets.Count < leaderCount) also register retry slot.
        //   Follower may see 1 or 2 of 3 PTT-QX-T orders before the rest land. OCO pairs are
        //   submitted for visible targets immediately (position partially protected), then retry
        //   fires when remaining PTT-QX-T orders go Working to complete the remaining pairs.
        private void MoveStopToBreakEven(
            Account acc,
            Instrument instrument,
            int bufferTicks,
            bool isRetry = false
        )
        {
            var pos = FindPosition(acc, instrument);
            if (IsFlat(pos)) // (1)
            {
                StatusUpdate?.Invoke(acc.Name + ": flat skip");
                return;
            }
            NinjaTrader.Code.Output.Process(
                "[BE] MoveStopToBreakEven: " + acc.Name + " buf=" + bufferTicks + "t",
                NinjaTrader.NinjaScript.PrintTo.OutputTab1
            );
            double tickSize = instrument.MasterInstrument.TickSize;
            bool isLong = pos.MarketPosition == MarketPosition.Long;
            // HOTFIX-BUG-BE-STOP-SHORT (MoveStopToBreakEven): align sign with PttBreakEven fix.
            // Long: stop goes BELOW entry (entry - buf*tick). Short: stop goes ABOVE entry (entry + buf*tick).
            // Old: direction = isLong ? +1 : -1  -> short stop = entry - buf (wrong, below entry).
            // New: direction = isLong ? -1 : +1  -> short stop = entry + buf (correct, above entry).
            double direction = isLong ? -1.0 : +1.0;
            double raw = pos.AveragePrice + direction * bufferTicks * tickSize;
            double newStop = Math.Round(raw / tickSize) * tickSize;

            // DW-B79-02 DIAG: log total order count for this account+instrument
            // across all states so we can detect NT8 sim order drops.
            int diagTotal = 0;
            foreach (Order o in acc.Orders)
                if (o?.Instrument?.FullName == instrument?.FullName)
                    diagTotal++;
            NinjaTrader.Code.Output.Process(
                "[BE-DIAG] " + acc.Name + " orders-for-instr=" + diagTotal,
                NinjaTrader.NinjaScript.PrintTo.OutputTab1
            );

            // -- Step A: snapshot ATM target orders BEFORE cancelling anything ----
            // DW-B107: extracted to SnapshotBeTargets to keep MoveStopToBreakEven CYC=7.
            // Two-pass native-first collect: native Target1..9 take priority over
            // stale PTT-QX-T*/PTT-BE-Target-* residues (same logic as DW-B106).
            var targets = SnapshotBeTargets(acc, instrument); // (3)
            // DW-B107: hard cap -- BE/QX contract is always exactly 3 targets max.
            // Prevents stale partial-fill residue submitting extra OCO pairs.
            // No LINQ -- while-loop trim per JS zero-alloc mandate.
            while (targets.Count > 3)
                targets.RemoveAt(targets.Count - 1);

            // DW-B88: unified cancel+resubmit -- replaces follower acc.Change() path and leader Step B/C.
            // Old follower block (L2742-2797) and leader Step B/C (L2800-2963) preserved below as
            // DW-B88 LEGACY comments for one-line rollback.
            PttBreakEvenSwap.Execute(acc, instrument, newStop, targets);
            StatusUpdate?.Invoke(acc.Name + ": BE stop @ " + newStop);

            // DW-B79-06: event-driven slot + 200ms fallback timer for targets=0 path.
            // Private CopyEngine members (_pendingFollowerBeSlots, QueueBeRetryFallback) stay here.
            if (targets.Count == 0)
            {
                if (!isRetry && !IsFlat(FindPosition(acc, instrument)))
                {
                    _pendingFollowerBeSlots[acc.Name] = new PendingFollowerBeSlot(
                        acc,
                        instrument,
                        bufferTicks
                    );
                    NinjaTrader.Code.Output.Process(
                        "[BE-DIAG] "
                            + acc.Name
                            + " -- targets=0, registered BE retry slot + 200ms fallback",
                        NinjaTrader.NinjaScript.PrintTo.OutputTab1
                    );
                    QueueBeRetryFallback(acc, instrument, bufferTicks, delayMs: 500);
                }
                return;
            }

            // DW-B79-07: partial-target retry slot + 200ms fallback timer.
            if (!isRetry && IsFollowerAccount(acc))
            {
                int leaderCount = CountLeaderTargets(instrument);
                if (
                    leaderCount > 0
                    && targets.Count < leaderCount
                    && !IsFlat(FindPosition(acc, instrument))
                )
                {
                    _pendingFollowerBeSlots[acc.Name] = new PendingFollowerBeSlot(
                        acc,
                        instrument,
                        bufferTicks
                    );
                    NinjaTrader.Code.Output.Process(
                        "[BE-DIAG] "
                            + acc.Name
                            + " -- partial targets="
                            + targets.Count
                            + " leader="
                            + leaderCount
                            + ", registered BE retry slot + 200ms fallback",
                        NinjaTrader.NinjaScript.PrintTo.OutputTab1
                    );
                    QueueBeRetryFallback(acc, instrument, bufferTicks);
                }
            }

            // DW-B88 LEGACY follower path (acc.Change) -- COMMENTED OUT:
            // if (IsFollowerAccount(acc))
            // {
            //     var beSt = new List<Order>();
            //     foreach (Order o in acc.Orders)
            //     {
            //         bool beStOk = o?.OrderState == OrderState.Working
            //                    || o?.OrderState == OrderState.Accepted
            //                    || o?.OrderState == OrderState.ChangeSubmitted;
            //         if (!beStOk) continue;
            //         if (o.Instrument?.FullName != instrument.FullName) continue;
            //         bool isBeStop = o.Name != null
            //             && (   (o.Name.StartsWith("Stop", StringComparison.Ordinal)
            //                     && o.Name.Length == 5
            //                     && char.IsDigit(o.Name[4]))
            //                  || o.Name.StartsWith("PTT-QX-Stop", StringComparison.Ordinal));
            //         if (isBeStop) { o.StopPriceChanged = newStop; beSt.Add(o); }
            //     }
            //     if (beSt.Count == 0) { /* dump diag */ }
            //     if (beSt.Count > 0) { try { acc.Change(beSt.ToArray()); } catch { } }
            //     StatusUpdate?.Invoke(acc.Name + ": BE stop @ " + newStop);
            //     return;
            // }
            //
            // DW-B88 LEGACY leader path (Step B/C) -- COMMENTED OUT:
            // // -- Step B: cancel all stale brackets --
            // // var stale = new List<Order>(); ... acc.Cancel(stale.ToArray());
            // //
            // // -- Step C: submit new BE stop+target OCO pairs --
            // // OrderAction stopDirection = isLong ? OrderAction.Sell : OrderAction.BuyToCover;
            // // if (targets.Count == 0) { /* bare PTT-BE-Stop */ ... return; }
            // // int seq = System.Threading.Interlocked.Increment(ref _mstbeOcoSeq);
            // // for (int i = 0; i < targets.Count; i++) { /* OCO pairs */ }
            // // StatusUpdate?.Invoke(acc.Name + ": BE stop @ " + newStop);
        }

        // BGTM-1: BreakEven gate CYC=3.
        internal void BreakEven(Instrument instrument, int bufferTicks)
        {
            if (!_flags.BreakEven)
            {
                StatusUpdate?.Invoke("Break Even requires Pro tier");
                return;
            }
            foreach (var acc in AllAccounts(instrument))
                MoveStopToBreakEven(acc, instrument, bufferTicks);
        }

        // B24 T1 -- BreakEven(Account,Instrument,int): fires leader directly, no rule needed.
        // DW-B84-01 FIX: followers run BEFORE leader.
        //   Root cause of stops=0: leader Step B acc.Cancel() triggers NT8 ATM cascade that puts
        //   follower Stop1/Stop2/Stop3 into CancelSubmitted before the follower path iterates them.
        //   Fix: run all follower acc.Change() calls first (while stops are still Working),
        //   then run leader cancel+replace. Order of operations is now: followers -> leader.
        // CYC=4: null guard(1), foreach followers(2), acc==leader skip(3), MoveStop leader(4). BGTM-1: BreakEven gate CYC=5.
        // JS-021: no lock. JS-002: null leader fires StatusUpdate + early return.
        internal void BreakEven(Account leader, Instrument instrument, int bufferTicks)
        {
            if (!_flags.BreakEven)
            {
                StatusUpdate?.Invoke("Break Even requires Pro tier");
                return;
            }
            if (leader == null) // (1) null guard
            {
                StatusUpdate?.Invoke("PTT-BE: leader null -- BE skipped");
                return;
            }
            foreach (var acc in AllAccounts(instrument)) // (2) followers first -- DW-B84-01
            {
                if (acc == leader)
                    continue; // (3) skip leader
                MoveStopToBreakEven(acc, instrument, bufferTicks); // acc.Change() while stops Working
            }
            MoveStopToBreakEven(leader, instrument, bufferTicks); // (4) leader last -- cancel+replace
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
            if (rule == null) // (1)
                return;
            foreach (var acc in AllAccounts(instrument)) // (2)
                TightenOneAccountStops(acc, instrument, ticks);
        }

        // B10 T3 -- TightenOneStop: applies tighten to a single stop order.
        // B31: in-place price move via order.StopPrice + acc.Change(new Order[]{order}).
        // CYC=2: null guard(1), alreadyTighter(2). tightenAction ternary removed.
        private void TightenOneStop(
            Account acc,
            Instrument instr,
            Order order,
            double targetPrice,
            double tickSize
        )
        {
            if (order == null) // (1)
                return;
            bool isLong = order.OrderAction == OrderAction.Sell; // stop-sell = long pos
            bool alreadyTighter = isLong
                ? order.StopPrice >= targetPrice
                : order.StopPrice <= targetPrice;
            if (alreadyTighter) // (2)
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
                return false; // (1)
            if (order.OrderType != OrderType.StopMarket && order.OrderType != OrderType.StopLimit)
                return false; // (2)
            if (order.Instrument != instrument)
                return false; // (3)
            if (!IsStopLeg(order))
                return false; // (4)
            return true;
        }

        // B30 -- GetRefPrice: resolves bid/ask reference price for tighten-stop calculation.
        // CYC=4: (1) bid>0 &&, (2) ask>0, (3) outer ?:, (4) inner isLong ?:.
        // DW-B30-04: NT8 null-conditional (?.) prevents NullReferenceException when MarketData unsubscribed.
        private static double GetRefPrice(Instrument instrument, bool isLong)
        {
            double bid = instrument.MarketData?.Bid?.Price ?? 0.0;
            double ask = instrument.MarketData?.Ask?.Price ?? 0.0;
            return bid > 0 && ask > 0 // (1)(2)
                ? (isLong ? ask : bid) // (3)(4)
                : 0.0;
        }

        // B30 -- TightenOneAccountStops: per-account stop-tighten helper. DW-B30-02.
        // CYC=5: (1) IsFlat guard, (2) refPrice==0 guard, (3) isLong ternary (target dir), (4) foreach, (5) !ShouldTightenOrder.
        // JS-021: no lock -- ToList() snapshot prevents iterator invalidation.
        // JS-002: no return null -- log "PTT-Tighten: no market data" on zero price.
        private void TightenOneAccountStops(Account acc, Instrument instrument, int tightenTicks)
        {
            var pos = FindPosition(acc, instrument);
            if (IsFlat(pos)) // (1)
                return;
            bool isLong = pos.MarketPosition == MarketPosition.Long;
            double tickSize = instrument.MasterInstrument.TickSize;
            double refPrice = GetRefPrice(instrument, isLong);
            if (refPrice == 0.0) // (2)
            {
                StatusUpdate?.Invoke("PTT-Tighten: no market data -- " + acc.Name);
                return;
            }
            double targetPrice = isLong // (3)
                ? refPrice - tightenTicks * tickSize
                : refPrice + tightenTicks * tickSize;
            foreach (var order in acc.Orders.ToList()) // (4)
            {
                if (!ShouldTightenOrder(order, instrument)) // (5)
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
            if (leader == null) // (1)
            {
                StatusUpdate?.Invoke("PTT-Tighten: leader null -- skipping");
                return;
            }
            TightenOneAccountStops(leader, instrument, tightenTicks); // (2)
            foreach (var acc in AllAccounts(instrument)) // (3)
            {
                if (acc == leader)
                    continue; // (4)
                TightenOneAccountStops(acc, instrument, tightenTicks);
            }
        }

        // B27 -- ArmPendingBe: arms the pending BE watcher using acc.AccountItemUpdate.
        // CYC=6: instr null(1), acc null+emit(2), pos flat+emit(3), immediate-fire check(4),
        //        tickSize guard(5), slot upsert(6).
        // DW-B30-05: StatusUpdate on null-leader and flat-position paths (previously silent).
        // DW-B27-01: slot dict replaces four singleton fields -- per-account, no data races.
        // HOTFIX-BUG-BE-IMMEDIATE: if price is already at/past entry when BE is pressed,
        //   fire BreakEven immediately -- do not arm a pending watcher that will never trigger.
        //   Fixes BE ALL "in the green" case (no immediate path existed before).
        //   Mirrors the IsPriceAlreadyAtBe check in TradeCopierPanel.OnBeClick (per-chart path).
        // JS-021: no lock -- ConcurrentDictionary indexer write is lock-free.
        internal void ArmPendingBe(Instrument instr, Account masterAcc, int bufferTicks)
        {
            if (instr == null) // (1)
                return;
            if (masterAcc == null) // (2)
            {
                StatusUpdate?.Invoke("PTT-BE: leader null -- skipped");
                return;
            }
            var pos = FindPosition(masterAcc, instr);
            if (IsFlat(pos)) // (3)
            {
                StatusUpdate?.Invoke("PTT-BE: no open position for " + masterAcc.Name);
                return;
            }
            // (4) HOTFIX-BUG-BE-IMMEDIATE: check if price is already at/past BE level right now.
            // Long:  bid >= avgPrice + buf*tick  (price already at or above entry)
            // Short: ask <= avgPrice - buf*tick  (price already at or below entry)
            // If true, fire immediately -- no need to arm and wait.
            double tickSize = instr.MasterInstrument?.TickSize ?? 0.0;
            if (tickSize > 0.0) // (5) guard -- no market data = arm normally
            {
                bool isLong = pos.MarketPosition == NinjaTrader.Cbi.MarketPosition.Long;
                double target = pos.AveragePrice + (isLong ? 1.0 : -1.0) * bufferTicks * tickSize;
                double refBid = instr.MarketData?.Bid?.Price ?? 0.0;
                double refAsk = instr.MarketData?.Ask?.Price ?? 0.0;
                double refPx = isLong ? refBid : refAsk;
                bool alreadyAtBe = refPx > 0.0 && (isLong ? (refPx >= target) : (refPx <= target));
                if (alreadyAtBe)
                {
                    StatusUpdate?.Invoke(
                        "PTT-BE: price already at BE for "
                            + masterAcc.Name
                            + " -- firing immediately"
                    );
                    BreakEven(masterAcc, instr, bufferTicks);
                    PendingBeFired?.Invoke(
                        instr.FullName ?? string.Empty,
                        masterAcc.Name ?? string.Empty
                    );
                    return;
                }
            }
            NinjaTrader.Code.Output.Process(
                "[BE] ArmPendingBe: "
                    + masterAcc.Name
                    + " "
                    + instr.FullName
                    + " buf="
                    + bufferTicks
                    + "t -- ARMED",
                NinjaTrader.NinjaScript.PrintTo.OutputTab1
            );
            _pendingBeSlots[masterAcc.Name] = new PendingBeSlot(masterAcc, instr, bufferTicks); // (6)
            PendingBeArmed?.Invoke(instr.FullName ?? string.Empty, masterAcc.Name ?? string.Empty); // HOTFIX-BEALL-SYNC-01
            masterAcc.AccountItemUpdate += OnPendingBeAccountUpdate;
        }

        // B27 -- DisarmPendingBe: disarms the pending BE watcher atomically.
        // CYC=3: leader null guard(1), TryRemove check(2), acc null guard(3).
        // DW-B27-01: reads Account from slot -- no stale singleton reference.
        // JS-021: no lock -- ConcurrentDictionary.TryRemove is atomic.
        // NT8-043: explicit if (acc != null) guard -- no ?.Event -= pattern.
        internal void DisarmPendingBe(Account leader)
        {
            if (leader == null) // (1)
            {
                StatusUpdate?.Invoke("DisarmPendingBe: leader null -- no-op");
                return;
            }
            if (!_pendingBeSlots.TryRemove(leader.Name, out var slot)) // (2)
                return;
            if (slot.Account != null) // (3)
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
        // BGTM-1: BreakEven gate CYC=5.
        internal void ArmTrailBe(Instrument instr, Account masterAcc, int bufferTicks)
        {
            if (!_flags.BreakEven)
            {
                StatusUpdate?.Invoke("Break Even requires Pro tier");
                return;
            }
            if (instr == null) // (1)
                return;
            if (masterAcc == null) // (2)
                return;
            var pos = FindPosition(masterAcc, instr);
            if (IsFlat(pos)) // (3)
                return;
            double currentPnl = masterAcc.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar);
            if (currentPnl == double.MinValue)
                currentPnl = 0.0;
            long pnlBits = BitConverter.DoubleToInt64Bits(currentPnl);
            _trailBeSlots[masterAcc.Name] = new TrailBeSlot(masterAcc, instr, bufferTicks); // (4)
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
            if (leader == null) // (1)
            {
                StatusUpdate?.Invoke("DisarmTrailBe: leader null -- no-op");
                return;
            }
            if (!_trailBeSlots.TryRemove(leader.Name, out var slot)) // (2)
                return;
            if (slot.Account != null) // (3)
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
            if (e.AccountItem != AccountItem.UnrealizedProfitLoss) // (1)
                return;
            string accName = (sender as NinjaTrader.Cbi.Account)?.Name ?? string.Empty;
            if (!_trailBeSlots.TryGetValue(accName, out var slot)) // (2)
                return;
            double newPnl = e.Value;
            if (!_trailBeLastPnlBits.TryGetValue(accName, out long oldBits)) // (3a)
                return;
            double oldPnl = BitConverter.Int64BitsToDouble(oldBits);
            if (newPnl <= oldPnl) // (3b)
                return;
            long newBits = BitConverter.DoubleToInt64Bits(newPnl);
            long actual = _trailBeLastPnlBits.AddOrUpdate( // (4)
                accName,
                newBits,
                (k, cur) => cur < newBits ? newBits : cur
            );
            if (actual != newBits) // lost race
                return;
            _trailBeSlots.AddOrUpdate( // (5)
                accName,
                new TrailBeSlot(slot.Account, slot.Instrument, slot.BufferTicks + 1),
                (k, old) => new TrailBeSlot(old.Account, old.Instrument, old.BufferTicks + 1)
            );
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
            if (e.AccountItem != AccountItem.UnrealizedProfitLoss) // (1)
                return;
            string accName = (sender as NinjaTrader.Cbi.Account)?.Name ?? string.Empty;
            if (!_pendingBeSlots.TryGetValue(accName, out var slot)) // (2)
                return;
            var acc = slot.Account;
            var instr = slot.Instrument;
            var buf = slot.BufferTicks;
            var pos = FindPosition(acc, instr);
            if (IsFlat(pos)) // (3)
                return;
            double tickSize = instr?.MasterInstrument?.TickSize ?? 0.0;
            if (tickSize <= 0.0) // (4)
                return;
            // HOTFIX-F2: Last.Price is 0 on Sim accounts and stale on reconnect.
            // Use Bid for long (price must reach entry from below) and Ask for short.
            // Falls back to Ask/Bid respectively if primary is 0 -- never blocks on 0.
            bool isLong = pos.MarketPosition == MarketPosition.Long;
            double refBid = instr?.MarketData?.Bid?.Price ?? 0.0;
            double refAsk = instr?.MarketData?.Ask?.Price ?? 0.0;
            double refPx = isLong
                ? (refBid > 0 ? refBid : refAsk) // long: use bid; fallback ask
                : (refAsk > 0 ? refAsk : refBid); // short: use ask; fallback bid
            if (refPx <= 0.0) // (5)
                return;
            double target = pos.AveragePrice + (isLong ? 1.0 : -1.0) * buf * tickSize;
            bool triggered = isLong ? (refPx >= target) : (refPx <= target);
            if (!triggered) // (6)
                return;
            if (!_pendingBeSlots.TryRemove(accName, out var removed)) // (7) atomic claim
                return;
            if (removed.Account != null)
                removed.Account.AccountItemUpdate -= OnPendingBeAccountUpdate;
            BreakEven(removed.Account, removed.Instrument, removed.BufferTicks);
            PendingBeFired?.Invoke(
                removed.Instrument?.FullName ?? string.Empty,
                removed.Account?.Name ?? string.Empty
            );
        }

        // -- B6/B8: Serialization DTO classes -----------------------------------

        [Serializable]
        internal sealed class CopyRuleDto
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
        internal sealed class CopyRulesContainer
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
                ?? Path.Combine(
                    NinjaTrader.Core.Globals.UserDataDir,
                    "PropTraderTools",
                    "copy_rules.xml"
                );
        }

        // -- B6/B8: Conversion helpers -----------------------------------------

        // B8 T1: RuleToDto -- emits FollowerMultipliers and FollowerAtmModeNames arrays
        // B10 T3: also emits TightenTicks
        private static CopyRuleDto RuleToDto(CopyRule rule)
        {
            var followerNames = new string[rule.FollowerAccounts.Length];
            for (int i = 0; i < rule.FollowerAccounts.Length; i++)
                followerNames[i] =
                    rule.FollowerAccounts[i] != null ? rule.FollowerAccounts[i].Name : string.Empty;

            // B8 T1: serialize multipliers parallel to account names
            var mults = new int[rule.FollowerAccounts.Length];
            for (int i = 0; i < rule.FollowerAccounts.Length; i++)
                mults[i] =
                    (rule.FollowerMultipliers != null && i < rule.FollowerMultipliers.Length)
                        ? rule.FollowerMultipliers[i]
                        : 1;

            // B8 T2: serialize ATM mode names using AtmModeToString + GetAtmMode per follower
            var atmNames = new string[rule.FollowerAccounts.Length];
            for (int i = 0; i < rule.FollowerAccounts.Length; i++)
            {
                string accName =
                    rule.FollowerAccounts[i] != null ? rule.FollowerAccounts[i].Name : string.Empty;
                atmNames[i] = AtmModeToString(GetAtmMode(rule, accName));
            }

            return new CopyRuleDto
            {
                InstrumentName = rule.Instrument,
                MasterAccountName =
                    rule.MasterAccount != null ? rule.MasterAccount.Name : string.Empty,
                FollowerAccountNames = followerNames,
                IsEnabled = rule.Enabled,
                FollowerMultipliers = mults,
                FollowerAtmModeNames = atmNames,
                TightenTicks = rule.TightenTicks, // B10 T3: emit tighten ticks
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
                followers[i] = FindFollowerAccount(dto.FollowerAccountNames[i]);
                // DW-B85 Option B: warn when follower account is not yet in Account.All at load time.
                // Workaround: uncheck + re-check the follower in the panel after NT8 finishes connecting.
                if (followers[i] == null)
                    NinjaTrader.Code.Output.Process(
                        "[PTT-COPY] WARNING: follower '"
                            + dto.FollowerAccountNames[i]
                            + "' not found in Account.All at load time"
                            + " -- will be skipped until rule is re-applied (uncheck + re-check in panel).",
                        NinjaTrader.NinjaScript.PrintTo.OutputTab1
                    );
            }

            // B8 T1: null-safe multiplier read (B6/B7 XML has no FollowerMultipliers element)
            int[] multipliers = null;
            if (dto.FollowerMultipliers != null && dto.FollowerMultipliers.Length > 0)
                multipliers = dto.FollowerMultipliers;

            // B8 T2: parse ATM mode names null-safely; build Dictionary (backward compat with B6/B7 XML)
            var atmMap = new Dictionary<string, FollowerAtmMode>();
            if (dto.FollowerAtmModeNames != null)
            {
                for (
                    int i = 0;
                    i < dto.FollowerAtmModeNames.Length && i < dto.FollowerAccountNames.Length;
                    i++
                )
                {
                    string accName = dto.FollowerAccountNames[i];
                    if (!string.IsNullOrEmpty(accName))
                        atmMap[accName] = ParseAtmModeName(dto.FollowerAtmModeNames[i]);
                }
            }

            // B10 T3: backward compat -- old XML has no TightenTicks element, XmlSerializer sets to 0.
            // DtoToRule converts: 0 -> default 5. Any positive value is preserved as-is.
            int tightenTicks = dto.TightenTicks > 0 ? dto.TightenTicks : 5;

            return CopyRule.Create(
                dto.InstrumentName,
                master,
                followers,
                dto.IsEnabled,
                multipliers,
                atmMap,
                tightenTicks,
                dto.FollowerAccountNames // B127: preserve original names (covers null-account slots)
            );
        }

        // DW-B85: extracted from DtoToRule inner foreach to keep DtoToRule CYC at 7.
        // Returns null (Account?) when account name is not found in Account.All.
        // CYC=2: foreach(1) + if(1).
        // JS-002 compliant: Account? return type makes nullability explicit end-to-end.
        private static Account? FindFollowerAccount(string name)
        {
            foreach (var acc in Account.All)
            {
                if (acc.Name == name)
                    return acc;
            }
            return null;
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
                container.CopyEnabled = _isCopyEnabled; // B54: persist enabled state

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
        /// Deserializes rules from an XML file into _rules. Idempotent: clears _rules and
        /// re-reads from disk on every call. Safe to call from Panel.OnLoaded and Window.OnLoaded
        /// independently -- each call produces the same _rules state from the same XML file.
        /// No lock keyword -- UI-thread-only; _rules is ConcurrentBag (thread-safe Add).
        /// CYC = 4 (File.Exists guard + try/catch + null-check + foreach)
        /// </summary>
        public void LoadRules(string overridePath = null)
        {
            _rules = new ConcurrentBag<CopyRule>(); // DW-B102: idempotent clear -- each caller gets a fresh read
            _resolvedFollowers.Clear(); // B127: invalidate lazy-resolve cache on rule reload (DW-PTT-BE-FIX-01)

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
                        _isCopyEnabled = container.CopyEnabled; // B54: restore enabled state
                        CopyEnabledChanged?.Invoke(_isCopyEnabled); // B54: sync UI buttons
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
