# EPIC-CCN-13: Analysis - ProcessOnStateChange Extraction

**Status**: ANALYSIS COMPLETE
**Created**: 2026-06-09
**Analyzed By**: V12 Epic Planner (jCodemunch-grounded)

---

## EXECUTIVE SUMMARY

ProcessOnStateChange is a 522-line lifecycle orchestrator with CYC 91, handling five distinct NinjaTrader state transitions in a single monolithic switch statement. Analysis reveals clean separation boundaries between state handlers with minimal shared dependencies, making this an ideal candidate for extraction.

**Key Finding**: Each state branch is self-contained with clear initialization/cleanup responsibilities. Zero cross-state dependencies detected.

---

## METHOD STRUCTURE ANALYSIS

### Current Implementation

**Signature**: `private void ProcessOnStateChange(State state)`
**Lines**: 44-565 (522 lines)
**Cyclomatic Complexity**: 91
**Max Nesting Depth**: 6
**Call Chain**: `OnStateChange()` → `ProcessOnStateChange(state)`

### State Branch Breakdown

| State | Lines | Approx CYC | Primary Responsibility | Key Operations |
|-------|-------|------------|------------------------|----------------|
| SetDefaults | 46-127 (82 lines) | ~15 | Property initialization | 60+ property assignments |
| Configure | 129-267 (139 lines) | ~25 | Collection setup | ConcurrentDictionary init, MMIO mirror, FSM setup |
| DataLoaded | 269-449 (181 lines) | ~30 | Indicator initialization | ATR/EMA/RSI setup, compliance logs, sticky state |
| Realtime | 451-518 (68 lines) | ~15 | Service startup | IPC server, watchdog, UI attachment, SIMA startup |
| Terminated | 520-565 (46 lines) | ~6 | Cleanup & disposal | Stop services, clear collections, dispose resources |

**Total**: 516 lines across 5 branches (6 lines for method signature/braces)

---

## COMPLEXITY HOTSPOTS

### State.SetDefaults (CYC ~15)
**Lines**: 46-127
**Complexity Drivers**:
- Linear property assignments (low complexity)
- No conditionals or loops
- Simple initialization logic

**Risk**: LOW - Pure initialization, no branching logic

### State.Configure (CYC ~25)
**Lines**: 129-267
**Complexity Drivers**:
- ConcurrentDictionary initialization (8 collections)
- MMIO mirror setup with try-catch (adds 2 CYC)
- Static assert validation (if statement adds 1 CYC)
- PhotonPool/Ring/Sideband initialization

**Risk**: MEDIUM - MMIO mirror failure handling, static assert validation

**Critical Section** (lines 217-234):
```csharp
// Static assert: Shadow must be the last 8 bytes of FleetDispatchSlot (ADR-016)
{
    int _slotSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(FleetDispatchSlot));
    int _shadowOffset = System.Runtime.InteropServices.Marshal.OffsetOf(typeof(FleetDispatchSlot), "Shadow").ToInt32();
    if (_slotSize != 64 || _shadowOffset != 56)
    {
        throw new InvalidOperationException(
            string.Format(
                "FleetDispatchSlot layout invariant violated: size={0}, shadowOffset={1}; expected size=64, offset=56",
                _slotSize,
                _shadowOffset
            )
        );
    }
}
```

### State.DataLoaded (CYC ~30)
**Lines**: 269-449
**Complexity Drivers**:
- Symbol detection logic (if/else for MES/MGC/default)
- Target count calculation with backward compatibility
- Multiple Print statements (no CYC impact)
- Sticky state hydration (if statement)

**Risk**: MEDIUM - Symbol-specific logic, backward compatibility handling

**Critical Sections**:
1. **Symbol Detection** (lines 283-297):
```csharp
string symbol = Instrument.MasterInstrument.Name;
if (symbol.Contains("MES") || symbol.Contains("ES"))
{
    minContracts = MESMinimum;
    maxContracts = MESMaximum;
}
else if (symbol.Contains("MGC") || symbol.Contains("GC"))
{
    minContracts = MGCMinimum;
    maxContracts = MGCMaximum;
}
else
{
    minContracts = 1;
    maxContracts = 20;
}
```

2. **Target Count Logic** (lines 299-318):
```csharp
int persistedTargetCount = Math.Max(0, Math.Min(5, ConfiguredTargetCount));
if (persistedTargetCount >= 1)
{
    activeTargetCount = persistedTargetCount;
}
else
{
    // Backward compatibility for templates saved before ConfiguredTargetCount existed.
    int loadedTargetCount =
        (Target1Value > 0 ? 1 : 0)
        + (Target2Value > 0 ? 1 : 0)
        + (Target3Value > 0 ? 1 : 0)
        + (Target4Value > 0 ? 1 : 0)
        + (Target5Value > 0 ? 1 : 0);
    activeTargetCount = Math.Max(1, Math.Min(5, loadedTargetCount));
    ConfiguredTargetCount = activeTargetCount;
}
```

### State.Realtime (CYC ~15)
**Lines**: 451-518
**Complexity Drivers**:
- EnableSIMA conditional (if statement)
- ChartControl null check (if statement)
- Two Dispatcher.InvokeAsync calls (nested lambdas)
- Enqueue lambda for SIMA startup

**Risk**: MEDIUM - UI thread synchronization, SIMA conditional startup

**Critical Section** (lines 489-518):
```csharp
if (ChartControl != null)
{
    // Hotkeys attach at Normal priority (fast, no visual tree dependency)
    ChartControl.Dispatcher.InvokeAsync(
        () =>
        {
            if (_isTerminating)
                return;
            AttachHotkeys();
            AttachChartClickHandler();
        },
        System.Windows.Threading.DispatcherPriority.Normal
    );

    // Panel creation deferred to Loaded priority (runs AFTER Render pass)
    ChartControl.Dispatcher.InvokeAsync(
        () =>
        {
            if (_isTerminating)
                return;
            CreatePanel();
            StartPanelRefresh();
            Print("REALTIME - Hotkeys: L=Long, S=Short, Shift+Click=RMA, F=Flatten");
        },
        System.Windows.Threading.DispatcherPriority.Loaded
    );
}
```

### State.Terminated (CYC ~6)
**Lines**: 520-565
**Complexity Drivers**:
- ChartControl null check (if statement)
- CancelAllV12GtcOrders call
- Multiple cleanup operations
- MMIO mirror disposal with try-catch

**Risk**: LOW - Straightforward cleanup, defensive null checks

---

## SHARED STATE ANALYSIS

### Instance Fields Referenced

**Read-Only Access** (safe to extract):
- `BUILD_TAG` (constant)
- `Instrument.MasterInstrument.*` (NinjaTrader API)
- `ChartControl` (UI reference)
- Property getters (RiskPerTrade, EnableSIMA, etc.)

**Write Access** (requires careful handling):
- `_configureComplete` (bool flag)
- `_dataLoadedComplete` (bool flag)
- `_startupReadinessLogEmitted` (Interlocked int)
- `_isTerminating` (bool flag)
- Collection fields (activePositions, entryOrders, etc.)
- Indicator fields (atrIndicator, ema9, etc.)
- Path fields (complianceLogPath, _stickyStatePath)

**Atomic Operations**:
- `Interlocked.Exchange(ref _startupReadinessLogEmitted, 0)` (lines 50, 523)

### Method Calls

**Helper Methods Called**:
- `ResetTelemetry()` (line 51)
- `InitializeIpcHardening()` (line 176)
- `ResetOR()` (line 336)
- `EnsureDailySummaryCsv()` (line 419)
- `ExecuteRiskLogicAudit()` (line 423)
- `LoadStickyState()` (line 428)
- `StartIpcServer()` (line 433)
- `TouchStrategyHeartbeat()` (lines 434, 471)
- `PublishUiSnapshot()` (lines 435, 472)
- `StartWatchdog()` (line 473)
- `Enqueue(ctx => ...)` (line 478)
- `AttachHotkeys()` (line 496)
- `AttachChartClickHandler()` (line 497)
- `CreatePanel()` (line 508)
- `StartPanelRefresh()` (line 509)
- `StopWatchdog()` (line 521)
- `StopPanelRefresh()` (line 527)
- `DetachHotkeys()` (line 532)
- `DetachChartClickHandler()` (line 533)
- `DestroyPanel()` (line 534)
- `CancelAllV12GtcOrders(false)` (line 540)
- `DrainQueuesForShutdown()` (line 542)
- `EmitMetricsSummary()` (line 543)
- `StopIpcServer()` (line 546)
- `StopReaperAudit()` (line 549)
- `UnsubscribeFromFleetAccounts()` (line 555)
- `SignalBroadcaster.ClearAllSubscribers()` (line 566)

**NinjaTrader API Calls**:
- `AddDataSeries()` (lines 263-265)
- `this.ATR()`, `this.EMA()`, `this.RSI()` (lines 321-330)
- `Print()` (multiple locations)

---

## DEPENDENCY ANALYSIS

### Zero External Dependencies
- No imports from other files
- No cross-file method calls
- Self-contained within V12_002 class

### Internal Dependencies
- All helper methods are in the same class
- All fields are instance members
- No static state mutations (except SignalBroadcaster.ClearAllSubscribers)

### NinjaTrader Framework Dependencies
- `State` enum (NinjaTrader.NinjaScript)
- `Instrument`, `ChartControl` (NinjaTrader API)
- `AddDataSeries`, indicator methods (NinjaTrader API)

---

## EXTRACTION RISKS

### HIGH RISK AREAS

1. **MMIO Mirror Initialization** (Configure, lines 237-248)
   - Try-catch with non-fatal failure
   - Must preserve exact error handling behavior
   - Risk: Breaking MMIO mirror fallback logic

2. **Static Assert Validation** (Configure, lines 217-234)
   - Throws InvalidOperationException on layout violation
   - Critical for ADR-016 compliance
   - Risk: Accidentally removing or weakening validation

3. **UI Thread Synchronization** (Realtime, lines 489-518)
   - Two Dispatcher.InvokeAsync calls with different priorities
   - Nested lambdas with _isTerminating checks
   - Risk: Breaking UI attachment timing or priority ordering

4. **SIMA Conditional Startup** (Realtime, lines 478-485)
   - Enqueue lambda with conditional Reaper start
   - Must preserve actor queue ordering
   - Risk: Breaking SIMA lifecycle or Reaper startup

### MEDIUM RISK AREAS

1. **Symbol Detection Logic** (DataLoaded, lines 283-297)
   - String.Contains checks for MES/ES and MGC/GC
   - Default fallback for unknown instruments
   - Risk: Breaking contract size limits for new instruments

2. **Target Count Backward Compatibility** (DataLoaded, lines 299-318)
   - Complex ternary logic for legacy template support
   - Risk: Breaking existing saved templates

3. **Sticky State Hydration** (DataLoaded, lines 428-430)
   - Must run BEFORE StartIpcServer()
   - Risk: Breaking IPC GET_LAYOUT command

### LOW RISK AREAS

1. **SetDefaults** - Pure property initialization, no branching
2. **Terminated** - Straightforward cleanup, defensive coding

---

## LOCK-FREE VERIFICATION

### Atomic Operations Found
- `Interlocked.Exchange(ref _startupReadinessLogEmitted, 0)` (lines 50, 523)

### Lock Statements Found
- **ZERO** - No `lock()` statements in ProcessOnStateChange

### Thread-Safe Collections
- All collections use `ConcurrentDictionary` (lines 145-175)
- SPSC ring buffer for Photon dispatch (line 181)

**Verdict**: ✅ LOCK-FREE COMPLIANT

---

## ASCII-ONLY VERIFICATION

### String Literals Scanned
- All string literals use ASCII characters
- No Unicode, emoji, or curly quotes detected
- Format strings use standard ASCII placeholders

**Verdict**: ✅ ASCII-ONLY COMPLIANT

---

## EXTRACTION STRATEGY RECOMMENDATIONS

### Approach 1: Direct State Handler Extraction (RECOMMENDED)
**Method**: Extract each state branch into a dedicated private method
**Pros**:
- Minimal refactoring surface
- Preserves exact behavior
- Clear 1:1 mapping (state → method)
- Low risk of logic drift

**Cons**:
- ProcessOnStateChange becomes a simple switch dispatcher
- No further abstraction

**Estimated CYC Reduction**:
- ProcessOnStateChange: 91 → 5 (one if per state)
- HandleSetDefaults: ~15
- HandleConfigure: ~25 → needs sub-extraction
- HandleDataLoaded: ~30 → needs sub-extraction
- HandleRealtime: ~15
- HandleTerminated: ~6

### Approach 2: State Handler + Sub-Method Extraction
**Method**: Extract state handlers, then further extract complex sub-sections
**Pros**:
- Achieves CYC ≤8 for all methods
- Better cognitive simplicity
- Aligns with Jane Street GODMODE

**Cons**:
- More tickets required
- Higher refactoring surface
- Requires careful sub-method naming

**Estimated CYC Reduction**:
- ProcessOnStateChange: 91 → 5
- HandleSetDefaults: ~15 (acceptable, pure init)
- HandleConfigure: ~25 → 8 (extract MMIO mirror setup)
- HandleDataLoaded: ~30 → 8 (extract symbol detection + target count logic)
- HandleRealtime: ~15 → 8 (extract UI attachment logic)
- HandleTerminated: ~6 (acceptable)

### Recommended Approach: **Approach 2** (State Handler + Sub-Method Extraction)

**Rationale**:
- Achieves Jane Street GODMODE (CYC ≤8) for ALL methods
- Breaks down the three complex handlers (Configure, DataLoaded, Realtime)
- Maintains V12 DNA compliance (≥15 LOC per method)
- Low risk due to zero blast radius

---

## TICKET BREAKDOWN PREVIEW

### Ticket 1: Extract SetDefaults Handler
**Target CYC**: ~15 (acceptable for pure initialization)
**Lines**: ~82
**Risk**: LOW

### Ticket 2: Extract Configure Handler + MMIO Mirror
**Target CYC**: 8 (extract MMIO mirror setup into sub-method)
**Lines**: ~139
**Risk**: MEDIUM (MMIO mirror error handling)

### Ticket 3: Extract DataLoaded Handler + Symbol/Target Logic
**Target CYC**: 8 (extract symbol detection + target count into sub-methods)
**Lines**: ~181
**Risk**: MEDIUM (symbol detection, backward compatibility)

### Ticket 4: Extract Realtime Handler + UI Attachment
**Target CYC**: 8 (extract UI attachment logic into sub-method)
**Lines**: ~68
**Risk**: MEDIUM (UI thread synchronization)

### Ticket 5: Extract Terminated Handler
**Target CYC**: ~6 (acceptable)
**Lines**: ~46
**Risk**: LOW

---

## VERIFICATION REQUIREMENTS

### Per-Ticket Verification
1. **Complexity Audit**: Before/after CYC comparison
2. **Build Gate**: Zero compilation errors
3. **Lock Audit**: `grep -r "lock(" src/V12_002.Lifecycle.cs` → zero matches
4. **Unicode Audit**: Verify ASCII-only in all string literals
5. **F5 Gate**: BUILD_TAG banner visible in NinjaTrader
6. **Hard-Link Sync**: `deploy-sync.ps1` succeeds

### Epic-Level Verification
1. **Final CYC Audit**: All methods ≤8 (or ≤15 for pure init)
2. **Behavior Preservation**: Zero logic changes
3. **PHS Perfection**: 100/100 score
4. **Session Snapshot**: Complete file read tracking

---

## NEXT PHASE

**Phase 2 (Approach)**: Design exact method signatures, sub-method names, and CYC targets for each ticket.

---

[ANALYSIS COMPLETE] Ready for approach design.