# EPIC-CCN-13 Ticket 04: Extract HandleDataLoaded + Sub-Methods

**Epic**: EPIC-CCN-13 - Extract ProcessOnStateChange (CYC 91 → ≤8)
**Ticket**: 04 of 5
**Risk Level**: MEDIUM
**Estimated Duration**: 30 minutes
**Target CYC**: HandleDataLoaded ≤8, InitializeInstrumentSettings ≤5, InitializeTargetConfiguration ≤6

---

## OBJECTIVE

Extract the `State.DataLoaded` branch from ProcessOnStateChange into `HandleDataLoaded()`, then further extract symbol detection and target count logic into dedicated sub-methods to achieve CYC ≤8 for all methods.

---

## CURRENT STATE

**File**: [`src/V12_002.Lifecycle.cs`](../../src/V12_002.Lifecycle.cs:269)
**Method**: `ProcessOnStateChange(State state)`
**Lines**: 269-449 (181 lines)
**Current CYC**: 76 (after Tickets 01-03)

**Target Section** (key parts):
```csharp
else if (state == State.DataLoaded)
{
    _dataLoadedComplete = false;
    
    tickSize = Instrument.MasterInstrument.TickSize;
    pointValue = Instrument.MasterInstrument.PointValue;
    lastKnownPrice = 0;
    
    // Symbol detection (lines 283-297) ← EXTRACT THIS
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
    
    // Target count logic (lines 299-318) ← EXTRACT THIS
    int persistedTargetCount = Math.Max(0, Math.Min(5, ConfiguredTargetCount));
    if (persistedTargetCount >= 1)
    {
        activeTargetCount = persistedTargetCount;
    }
    else
    {
        // Backward compatibility
        int loadedTargetCount =
            (Target1Value > 0 ? 1 : 0)
            + (Target2Value > 0 ? 1 : 0)
            + (Target3Value > 0 ? 1 : 0)
            + (Target4Value > 0 ? 1 : 0)
            + (Target5Value > 0 ? 1 : 0);
        activeTargetCount = Math.Max(1, Math.Min(5, loadedTargetCount));
        ConfiguredTargetCount = activeTargetCount;
    }
    
    // Initialize indicators
    atrIndicator = this.ATR(BarsArray[1], RMAATRPeriod);
    ema9 = this.EMA(9);
    // ... more indicators ...
    
    ResetOR();
    
    // Multiple Print statements
    Print(string.Format("UniversalORStrategy {0} | {1} | Tick: {2} | PV: ${3}", ...));
    // ... more prints ...
    
    // Compliance log paths
    string logsDir = System.IO.Path.Combine(...);
    complianceLogPath = System.IO.Path.Combine(logsDir, $"ApexPerformance_{symbol}.json");
    // ... more paths ...
    
    ExecuteRiskLogicAudit();
    
    _dataLoadedComplete = true;
    
    // Sticky state hydration
    _stickyStatePath = System.IO.Path.Combine(logsDir, string.Format("StickyState_{0}.v12state", symbol));
    bool stickyLoaded = LoadStickyState();
    if (stickyLoaded)
        Print("[STICKY] Persisted state hydrated -- GET_LAYOUT will serve last-synced config");
    
    // Start services
    StartIpcServer();
    TouchStrategyHeartbeat();
    PublishUiSnapshot();
}
```

---

## TARGET STATE

**New Method Signatures**:
```csharp
private void HandleDataLoaded()
private void InitializeInstrumentSettings()
private void InitializeTargetConfiguration()
```

**Modified ProcessOnStateChange**:
```csharp
else if (state == State.DataLoaded)
{
    HandleDataLoaded();
}
```

**HandleDataLoaded** (calls sub-methods):
```csharp
private void HandleDataLoaded()
{
    _dataLoadedComplete = false;
    
    tickSize = Instrument.MasterInstrument.TickSize;
    pointValue = Instrument.MasterInstrument.PointValue;
    lastKnownPrice = 0;
    
    InitializeInstrumentSettings(); // ← Call sub-method
    InitializeTargetConfiguration(); // ← Call sub-method
    
    // Initialize indicators
    atrIndicator = this.ATR(BarsArray[1], RMAATRPeriod);
    // ... indicators ...
    
    ResetOR();
    
    // Print statements
    // ... prints ...
    
    // Compliance log paths
    string symbol = Instrument.MasterInstrument.Name;
    string logsDir = System.IO.Path.Combine(...);
    complianceLogPath = System.IO.Path.Combine(logsDir, $"ApexPerformance_{symbol}.json");
    // ... paths ...
    
    ExecuteRiskLogicAudit();
    
    _dataLoadedComplete = true;
    
    // Sticky state hydration
    _stickyStatePath = System.IO.Path.Combine(logsDir, string.Format("StickyState_{0}.v12state", symbol));
    bool stickyLoaded = LoadStickyState();
    if (stickyLoaded)
        Print("[STICKY] Persisted state hydrated -- GET_LAYOUT will serve last-synced config");
    
    // Start services
    StartIpcServer();
    TouchStrategyHeartbeat();
    PublishUiSnapshot();
}
```

**InitializeInstrumentSettings** (extracted):
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

**InitializeTargetConfiguration** (extracted):
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

---

## EXTRACTION STEPS

### Step 1: Create HandleDataLoaded Method
1. Position: Insert after `InitializeMmioMirror` (created in Ticket 03)
2. Copy lines 269-449 (DataLoaded branch body) into new method
3. Remove the opening `else if (state == State.DataLoaded) {` and closing `}`
4. Verify method signature: `private void HandleDataLoaded()`

### Step 2: Extract InitializeInstrumentSettings
1. Position: Insert after `HandleDataLoaded`
2. Copy lines 283-297 (symbol detection) from HandleDataLoaded into new method
3. Replace those lines in HandleDataLoaded with: `InitializeInstrumentSettings();`
4. Verify method signature: `private void InitializeInstrumentSettings()`

### Step 3: Extract InitializeTargetConfiguration
1. Position: Insert after `InitializeInstrumentSettings`
2. Copy lines 299-318 (target count logic) from HandleDataLoaded into new method
3. Replace those lines in HandleDataLoaded with: `InitializeTargetConfiguration();`
4. Verify method signature: `private void InitializeTargetConfiguration()`

### Step 4: Replace Original Branch
1. In `ProcessOnStateChange`, replace lines 269-449 with:
```csharp
else if (state == State.DataLoaded)
{
    HandleDataLoaded();
}
```

### Step 5: Verify Extraction
1. **Line Count**: HandleDataLoaded ~120 lines, InitializeInstrumentSettings ~15 lines, InitializeTargetConfiguration ~20 lines
2. **No Logic Changes**: Exact copy of original code
3. **Indentation**: Adjust for method bodies
4. **Symbol Variable**: Verify symbol is declared in HandleDataLoaded for log paths
5. **Backward Compatibility**: Verify target count logic preserved exactly

---

## COMPLEXITY ESTIMATE

**Before**:
- ProcessOnStateChange: CYC 76 (after Tickets 01-03)

**After**:
- ProcessOnStateChange: CYC 71 (one more branch extracted)
- HandleDataLoaded: CYC ~8 (after sub-method extraction)
- InitializeInstrumentSettings: CYC ~5 (if-else chain)
- InitializeTargetConfiguration: CYC ~6 (if + ternary operators)

---

## VERIFICATION CHECKLIST

### Pre-Extraction
- [ ] Read current ProcessOnStateChange source
- [ ] Identify lines 269-449 (DataLoaded branch)
- [ ] Identify lines 283-297 (symbol detection)
- [ ] Identify lines 299-318 (target count logic)
- [ ] Verify Tickets 01-03 completed

### Post-Extraction
- [ ] HandleDataLoaded exists after InitializeMmioMirror
- [ ] InitializeInstrumentSettings exists after HandleDataLoaded
- [ ] InitializeTargetConfiguration exists after InitializeInstrumentSettings
- [ ] HandleDataLoaded calls both sub-methods
- [ ] ProcessOnStateChange calls HandleDataLoaded()
- [ ] No duplicate code remains
- [ ] Indentation is correct

### Complexity Audit
- [ ] Run: `python scripts/complexity_audit.py`
- [ ] Verify: ProcessOnStateChange CYC 76 → 71
- [ ] Verify: HandleDataLoaded CYC ≤8
- [ ] Verify: InitializeInstrumentSettings CYC ≤5
- [ ] Verify: InitializeTargetConfiguration CYC ≤6

### Build Gate
- [ ] Run: `powershell -File .\scripts\build_readiness.ps1`
- [ ] Verify: Zero compilation errors
- [ ] Verify: CSharpier formatting passes

### V12 DNA Compliance
- [ ] Lock Audit: `grep -r "lock(" src/V12_002.Lifecycle.cs` → zero matches
- [ ] Unicode Audit: Verify ASCII-only in all string literals
- [ ] LOC Floor: HandleDataLoaded ≥15 lines ✅ (~120 lines)
- [ ] LOC Floor: InitializeInstrumentSettings ≥15 lines ✅ (~15 lines)
- [ ] LOC Floor: InitializeTargetConfiguration ≥15 lines ✅ (~20 lines)

### F5 Gate (MANDATORY)
- [ ] Run: `powershell -File .\deploy-sync.ps1`
- [ ] Press F5 in NinjaTrader IDE
- [ ] Verify: BUILD_TAG banner visible
- [ ] Verify: Symbol detection works (check console for MES/MGC/default)
- [ ] Verify: Target count initialized correctly
- [ ] Type: `F5 done [BUILD_TAG]`

### Commit
- [ ] Run: `git add src/V12_002.Lifecycle.cs`
- [ ] Run: `git commit -m "[EPIC-CCN-13] ticket-04: extract HandleDataLoaded + sub-methods -- CYC 76->71 [BUILD_TAG]"`

---

## RISK ASSESSMENT

**Risk Level**: MEDIUM

**Why Medium Risk**:
- Symbol detection logic affects contract sizing
- Target count logic has backward compatibility handling
- Multiple indicator initializations
- Sticky state hydration timing is critical

**Critical Sections**:
1. **Symbol Detection** (lines 283-297): Must preserve exact string matching
2. **Target Count Logic** (lines 299-318): Must preserve backward compatibility
3. **Sticky State Timing**: Must run BEFORE StartIpcServer()

**Potential Issues**:
- Symbol mismatch (e.g., "MES" vs "ES")
- Target count calculation error
- Sticky state load failure (already handled with bool check)

---

## ROLLBACK PROCEDURE

If F5 gate fails:
1. Run: `git reset --hard HEAD~1`
2. Verify: ProcessOnStateChange restored to previous state
3. Report failure in `docs/brain/EPIC-CCN-13/ticket-04-failure.md`
4. Escalate to Director

---

## SUCCESS CRITERIA

- [ ] HandleDataLoaded method created
- [ ] InitializeInstrumentSettings method created
- [ ] InitializeTargetConfiguration method created
- [ ] HandleDataLoaded calls both sub-methods
- [ ] ProcessOnStateChange calls HandleDataLoaded()
- [ ] CYC reduced: 76 → 71
- [ ] HandleDataLoaded CYC ≤8
- [ ] InitializeInstrumentSettings CYC ≤5
- [ ] InitializeTargetConfiguration CYC ≤6
- [ ] Build passes
- [ ] F5 verification passes
- [ ] Symbol detection works correctly
- [ ] Target count initialized correctly
- [ ] Zero locks, zero Unicode
- [ ] Commit created with BUILD_TAG

---

## DEPENDENCIES

**Upstream**: Tickets 01-03 (all previous handlers must exist)
**Downstream**: None (independent extraction)

---

## NOTES

- Most complex ticket (2 sub-method extractions)
- Symbol detection is critical for contract sizing
- Target count logic has backward compatibility (must preserve)
- Sticky state must load BEFORE IPC server starts
- Watch for symbol detection console output during F5 verification

---

**Status**: READY FOR EXECUTION
**Assigned To**: v12-engineer mode
**Estimated Time**: 30 minutes