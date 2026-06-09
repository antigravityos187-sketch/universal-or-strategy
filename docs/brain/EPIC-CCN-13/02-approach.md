# EPIC-CCN-13: Approach - ProcessOnStateChange Extraction

**Status**: APPROACH COMPLETE
**Created**: 2026-06-09
**Strategy**: State Handler + Sub-Method Extraction (Jane Street GODMODE)

---

## EXECUTIVE SUMMARY

This approach extracts ProcessOnStateChange (CYC 91) into five state handler methods, with additional sub-method extraction for the three complex handlers (Configure, DataLoaded, Realtime) to achieve CYC ≤8 for all methods. The extraction preserves exact behavior through surgical refactoring with zero logic changes.

**Target State**: ProcessOnStateChange becomes a 5-branch dispatcher (CYC 5), delegating to dedicated handlers that each satisfy Jane Street GODMODE (CYC ≤8).

---

## EXTRACTION ARCHITECTURE

### Before (Current State)
```
ProcessOnStateChange(State state)  [CYC 91, 522 lines]
├─ if (state == State.SetDefaults) { ... }      [~82 lines, CYC ~15]
├─ else if (state == State.Configure) { ... }   [~139 lines, CYC ~25]
├─ else if (state == State.DataLoaded) { ... }  [~181 lines, CYC ~30]
├─ else if (state == State.Realtime) { ... }    [~68 lines, CYC ~15]
└─ else if (state == State.Terminated) { ... }  [~46 lines, CYC ~6]
```

### After (Target State)
```
ProcessOnStateChange(State state)  [CYC 5, ~15 lines]
├─ if (state == State.SetDefaults) → HandleSetDefaults()
├─ else if (state == State.Configure) → HandleConfigure()
├─ else if (state == State.DataLoaded) → HandleDataLoaded()
├─ else if (state == State.Realtime) → HandleRealtime()
└─ else if (state == State.Terminated) → HandleTerminated()

HandleSetDefaults()  [CYC ~15, ~82 lines]
  (Pure initialization - acceptable per Jane Street for setup code)

HandleConfigure()  [CYC 8, ~100 lines]
├─ Collection initialization
├─ PhotonPool/Ring/Sideband setup
├─ Static assert validation
└─ InitializeMmioMirror()  [CYC 3, ~40 lines]

HandleDataLoaded()  [CYC 8, ~120 lines]
├─ InitializeInstrumentSettings()  [CYC 5, ~30 lines]
├─ InitializeTargetConfiguration()  [CYC 6, ~25 lines]
├─ Indicator initialization
├─ Compliance setup
└─ Sticky state hydration

HandleRealtime()  [CYC 8, ~40 lines]
├─ Service startup (IPC, watchdog, heartbeat)
├─ SIMA conditional startup
└─ AttachUiComponents()  [CYC 5, ~30 lines]

HandleTerminated()  [CYC ~6, ~46 lines]
  (Straightforward cleanup - acceptable)
```

---

## METHOD SIGNATURES

### Primary Dispatcher (Modified)
```csharp
private void ProcessOnStateChange(State state)
{
    if (state == State.SetDefaults)
    {
        HandleSetDefaults();
    }
    else if (state == State.Configure)
    {
        HandleConfigure();
    }
    else if (state == State.DataLoaded)
    {
        HandleDataLoaded();
    }
    else if (state == State.Realtime)
    {
        HandleRealtime();
    }
    else if (state == State.Terminated)
    {
        HandleTerminated();
    }
}
```

### State Handlers (New)
```csharp
private void HandleSetDefaults()
private void HandleConfigure()
private void HandleDataLoaded()
private void HandleRealtime()
private void HandleTerminated()
```

### Sub-Methods (New)
```csharp
// Configure sub-methods
private void InitializeMmioMirror()

// DataLoaded sub-methods
private void InitializeInstrumentSettings()
private void InitializeTargetConfiguration()

// Realtime sub-methods
private void AttachUiComponents()
```

---

## DETAILED EXTRACTION PLAN

### Ticket 1: Extract HandleSetDefaults
**Goal**: Extract State.SetDefaults branch into dedicated method
**Target CYC**: ~15 (acceptable for pure initialization)
**Lines**: ~82

**Method Signature**:
```csharp
private void HandleSetDefaults()
```

**Extraction Steps**:
1. Create `HandleSetDefaults()` method below `ProcessOnStateChange`
2. Copy lines 46-127 (SetDefaults branch body) into new method
3. Replace original branch with `HandleSetDefaults();` call
4. Verify: CYC ~15, zero locks, ASCII-only

**Complexity Estimate**:
- ProcessOnStateChange: 91 → 86 (one branch extracted)
- HandleSetDefaults: ~15 (pure init, acceptable)

**Risk**: LOW - No branching logic, pure property assignments

---

### Ticket 2: Extract HandleConfigure + InitializeMmioMirror
**Goal**: Extract State.Configure branch and MMIO mirror setup
**Target CYC**: HandleConfigure ≤8, InitializeMmioMirror ≤3
**Lines**: ~139 total (~100 + ~40)

**Method Signatures**:
```csharp
private void HandleConfigure()
private void InitializeMmioMirror()
```

**Extraction Steps**:
1. Create `HandleConfigure()` method
2. Copy lines 129-267 (Configure branch body) into new method
3. Extract MMIO mirror setup (lines 237-248) into `InitializeMmioMirror()`
4. Replace MMIO section in `HandleConfigure()` with `InitializeMmioMirror();` call
5. Replace original branch with `HandleConfigure();` call
6. Verify: HandleConfigure CYC ≤8, InitializeMmioMirror CYC ≤3

**MMIO Mirror Extraction** (lines 237-248):
```csharp
private void InitializeMmioMirror()
{
    // Optional MMIO mirror. Named per-process so multiple NT instances do not collide.
    // Failure is non-fatal: hot path runs against the heap ring even if the mirror fails.
    try
    {
        string _mmfName =
            "V12_FleetDispatch_"
            + System.Diagnostics.Process.GetCurrentProcess().Id.ToString()
            + "_"
            + _photonShadowSalt.ToString("X16");
        _photonMmioMirror = new MmioDispatchMirror(_mmfName, PhotonPoolCapacity, 64, _photonShadowSalt);
        Print(string.Format("[PHOTON MMIO] mirror online: {0}", _mmfName));
    }
    catch (Exception _mmioEx)
    {
        _photonMmioMirror = null;
        Print("[PHOTON MMIO] mirror unavailable (hot path unaffected): " + _mmioEx.Message);
    }
}
```

**Complexity Estimate**:
- ProcessOnStateChange: 86 → 81 (one branch extracted)
- HandleConfigure: ~8 (after MMIO extraction)
- InitializeMmioMirror: ~3 (try-catch + 2 assignments)

**Risk**: MEDIUM - MMIO mirror error handling must be preserved exactly

---

### Ticket 3: Extract HandleDataLoaded + Sub-Methods
**Goal**: Extract State.DataLoaded branch with symbol/target sub-methods
**Target CYC**: HandleDataLoaded ≤8, sub-methods ≤6
**Lines**: ~181 total (~120 + ~30 + ~25)

**Method Signatures**:
```csharp
private void HandleDataLoaded()
private void InitializeInstrumentSettings()
private void InitializeTargetConfiguration()
```

**Extraction Steps**:
1. Create `HandleDataLoaded()` method
2. Copy lines 269-449 (DataLoaded branch body) into new method
3. Extract symbol detection (lines 283-297) into `InitializeInstrumentSettings()`
4. Extract target count logic (lines 299-318) into `InitializeTargetConfiguration()`
5. Replace extracted sections with method calls
6. Replace original branch with `HandleDataLoaded();` call
7. Verify: All methods CYC ≤8

**InitializeInstrumentSettings Extraction** (lines 283-297):
```csharp
private void InitializeInstrumentSettings()
{
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
}
```

**InitializeTargetConfiguration Extraction** (lines 299-318):
```csharp
private void InitializeTargetConfiguration()
{
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
}
```

**Complexity Estimate**:
- ProcessOnStateChange: 81 → 76 (one branch extracted)
- HandleDataLoaded: ~8 (after sub-method extraction)
- InitializeInstrumentSettings: ~5 (if-else chain)
- InitializeTargetConfiguration: ~6 (if + ternary operators)

**Risk**: MEDIUM - Symbol detection and backward compatibility logic

---

### Ticket 4: Extract HandleRealtime + AttachUiComponents
**Goal**: Extract State.Realtime branch with UI attachment sub-method
**Target CYC**: HandleRealtime ≤8, AttachUiComponents ≤5
**Lines**: ~68 total (~40 + ~30)

**Method Signatures**:
```csharp
private void HandleRealtime()
private void AttachUiComponents()
```

**Extraction Steps**:
1. Create `HandleRealtime()` method
2. Copy lines 451-518 (Realtime branch body) into new method
3. Extract UI attachment logic (lines 489-518) into `AttachUiComponents()`
4. Replace UI section with `AttachUiComponents();` call
5. Replace original branch with `HandleRealtime();` call
6. Verify: Both methods CYC ≤8

**AttachUiComponents Extraction** (lines 489-518):
```csharp
private void AttachUiComponents()
{
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
}
```

**Complexity Estimate**:
- ProcessOnStateChange: 76 → 71 (one branch extracted)
- HandleRealtime: ~8 (after UI extraction)
- AttachUiComponents: ~5 (if + 2 lambdas with nested if)

**Risk**: MEDIUM - UI thread synchronization, dispatcher priority ordering

---

### Ticket 5: Extract HandleTerminated
**Goal**: Extract State.Terminated branch into dedicated method
**Target CYC**: ~6 (acceptable)
**Lines**: ~46

**Method Signature**:
```csharp
private void HandleTerminated()
```

**Extraction Steps**:
1. Create `HandleTerminated()` method
2. Copy lines 520-565 (Terminated branch body) into new method
3. Replace original branch with `HandleTerminated();` call
4. Verify: CYC ~6, zero locks, ASCII-only

**Complexity Estimate**:
- ProcessOnStateChange: 71 → 5 (final branch extracted)
- HandleTerminated: ~6 (if + try-catch)

**Risk**: LOW - Straightforward cleanup logic

---

## FINAL COMPLEXITY TARGETS

| Method | Current CYC | Target CYC | Status |
|--------|-------------|------------|--------|
| ProcessOnStateChange | 91 | 5 | ✅ Dispatcher only |
| HandleSetDefaults | N/A | ~15 | ✅ Pure init (acceptable) |
| HandleConfigure | N/A | 8 | ✅ After MMIO extraction |
| InitializeMmioMirror | N/A | 3 | ✅ Try-catch only |
| HandleDataLoaded | N/A | 8 | ✅ After sub-method extraction |
| InitializeInstrumentSettings | N/A | 5 | ✅ If-else chain |
| InitializeTargetConfiguration | N/A | 6 | ✅ If + ternary |
| HandleRealtime | N/A | 8 | ✅ After UI extraction |
| AttachUiComponents | N/A | 5 | ✅ If + nested lambdas |
| HandleTerminated | N/A | 6 | ✅ If + try-catch |

**Total Methods**: 10 (1 modified + 9 new)
**All Methods**: CYC ≤15 (Jane Street GODMODE achieved)

---

## VERIFICATION STRATEGY

### Per-Ticket Verification
1. **Before Snapshot**: `python scripts/complexity_audit.py > before.txt`
2. **Extraction**: Perform surgical refactoring
3. **After Snapshot**: `python scripts/complexity_audit.py > after.txt`
4. **Diff Analysis**: Compare before/after CYC values
5. **Build Gate**: `powershell -File .\scripts\build_readiness.ps1`
6. **Lock Audit**: `grep -r "lock(" src/V12_002.Lifecycle.cs` → zero matches
7. **Unicode Audit**: Verify ASCII-only in all string literals
8. **F5 Gate**: Press F5 in NinjaTrader, verify BUILD_TAG banner
9. **Hard-Link Sync**: `powershell -File .\deploy-sync.ps1`
10. **Commit**: `git commit -m "[EPIC-CCN-13] ticket-XX: [description] -- CYC [before]->[after] [BUILD_TAG]"`

### Epic-Level Verification
1. **Final Complexity Audit**: All methods ≤15 (≤8 for non-init)
2. **Behavior Preservation**: Zero logic changes confirmed
3. **PHS Perfection**: `/pr-loop` to 100/100
4. **Session Snapshot**: Complete file read tracking

---

## RISK MITIGATION

### High-Risk Sections
1. **MMIO Mirror** (Ticket 2): Preserve exact try-catch behavior
2. **Static Assert** (Ticket 2): Do not weaken validation logic
3. **UI Thread Sync** (Ticket 4): Preserve dispatcher priority ordering
4. **SIMA Startup** (Ticket 4): Preserve Enqueue lambda ordering

### Mitigation Strategy
- Extract one ticket at a time
- F5 verification after each ticket
- Automated complexity audit before/after
- Session snapshot for rollback capability
- Zero logic changes (pure extraction only)

---

## EXECUTION ORDER

**Dependency Order** (no inter-ticket dependencies):
1. Ticket 1: HandleSetDefaults (independent)
2. Ticket 2: HandleConfigure + InitializeMmioMirror (independent)
3. Ticket 3: HandleDataLoaded + sub-methods (independent)
4. Ticket 4: HandleRealtime + AttachUiComponents (independent)
5. Ticket 5: HandleTerminated (independent)

**Recommended Order** (risk-based):
1. Ticket 1: HandleSetDefaults (LOW risk, warm-up)
2. Ticket 5: HandleTerminated (LOW risk, build confidence)
3. Ticket 2: HandleConfigure + InitializeMmioMirror (MEDIUM risk)
4. Ticket 3: HandleDataLoaded + sub-methods (MEDIUM risk)
5. Ticket 4: HandleRealtime + AttachUiComponents (MEDIUM risk)

---

## SUCCESS CRITERIA

### Quantitative
- [ ] ProcessOnStateChange CYC: 91 → 5
- [ ] All new methods CYC ≤15 (≤8 for non-init)
- [ ] Zero new locks introduced
- [ ] Zero Unicode in string literals
- [ ] All new methods ≥15 LOC (V12 DNA floor)

### Qualitative
- [ ] F5 verification passes (BUILD_TAG visible)
- [ ] deploy-sync.ps1 succeeds
- [ ] Pre-push validation passes (13 gates)
- [ ] PHS 100/100 achieved
- [ ] Zero logic drift from original

---

## NEXT PHASE

**Phase 2.3**: `/epic-scan` - Sentinel audit using Greptile MCP to validate approach against live codebase semantics.

---

[APPROACH COMPLETE] Ready for validation.