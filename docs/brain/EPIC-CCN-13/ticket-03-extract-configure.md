# EPIC-CCN-13 Ticket 03: Extract HandleConfigure + InitializeMmioMirror

**Epic**: EPIC-CCN-13 - Extract ProcessOnStateChange (CYC 91 → ≤8)
**Ticket**: 03 of 5
**Risk Level**: MEDIUM
**Estimated Duration**: 25 minutes
**Target CYC**: HandleConfigure ≤8, InitializeMmioMirror ≤3

---

## OBJECTIVE

Extract the `State.Configure` branch from ProcessOnStateChange into `HandleConfigure()`, then further extract the MMIO mirror setup into `InitializeMmioMirror()` to achieve CYC ≤8 for both methods.

---

## CURRENT STATE

**File**: [`src/V12_002.Lifecycle.cs`](../../src/V12_002.Lifecycle.cs:129)
**Method**: `ProcessOnStateChange(State state)`
**Lines**: 129-267 (139 lines)
**Current CYC**: 81 (after Tickets 01-02)

**Target Section**:
```csharp
else if (state == State.Configure)
{
    _configureComplete = false;
    _dataLoadedComplete = false;
    
    // Initialize ConcurrentDictionaries (8 collections)
    activePositions = new ConcurrentDictionary<string, PositionInfo>(2, 4);
    // ... 7 more collections ...
    
    // IPC Queue
    ipcCommandQueue = new ConcurrentQueue<string>();
    connectedClients = new ConcurrentDictionary<int, IpcClientSession>();
    
    InitializeIpcHardening();
    
    // SIMA tracking
    expectedPositions = new ConcurrentDictionary<string, int>(2, 20);
    
    // Photon Pool setup
    _photonPool = new PhotonOrderPool(PhotonPoolCapacity);
    _photonDispatchRing = new SPSCRing<FleetDispatchSlot>(PhotonPoolCapacity);
    // ... more Photon setup ...
    
    // Static assert validation (lines 217-234)
    {
        int _slotSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(FleetDispatchSlot));
        // ... validation logic ...
    }
    
    // MMIO mirror setup (lines 237-248) ← EXTRACT THIS
    try
    {
        string _mmfName = "V12_FleetDispatch_" + ...;
        _photonMmioMirror = new MmioDispatchMirror(...);
        Print(string.Format("[PHOTON MMIO] mirror online: {0}", _mmfName));
    }
    catch (Exception _mmioEx)
    {
        _photonMmioMirror = null;
        Print("[PHOTON MMIO] mirror unavailable (hot path unaffected): " + _mmioEx.Message);
    }
    
    // Execution ID rings
    _executionIdRing = new ExecutionIdRing(512, 1024);
    _executionIdFallbackRing = new ExecutionIdRing(512, 1024);
    
    // Compliance Hub log directory
    string logsDirInit = System.IO.Path.Combine(...);
    if (!System.IO.Directory.Exists(logsDirInit))
        System.IO.Directory.CreateDirectory(logsDirInit);
    
    // Add data series
    AddDataSeries(BarsPeriodType.Minute, 5);
    AddDataSeries(BarsPeriodType.Minute, 10);
    AddDataSeries(BarsPeriodType.Minute, 15);
    
    _configureComplete = true;
}
```

---

## TARGET STATE

**New Method Signatures**:
```csharp
private void HandleConfigure()
private void InitializeMmioMirror()
```

**Modified ProcessOnStateChange**:
```csharp
else if (state == State.Configure)
{
    HandleConfigure();
}
```

**HandleConfigure** (calls InitializeMmioMirror):
```csharp
private void HandleConfigure()
{
    _configureComplete = false;
    _dataLoadedComplete = false;
    
    // Initialize collections
    // ... collection setup ...
    
    // Photon Pool setup
    // ... pool setup ...
    
    // Static assert validation
    // ... validation ...
    
    InitializeMmioMirror(); // ← Call sub-method
    
    // Execution ID rings
    // ... rings setup ...
    
    // Compliance Hub
    // ... log directory ...
    
    // Add data series
    // ... data series ...
    
    _configureComplete = true;
}
```

**InitializeMmioMirror** (extracted from HandleConfigure):
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

---

## EXTRACTION STEPS

### Step 1: Create HandleConfigure Method
1. Position: Insert after `HandleTerminated` (created in Ticket 02)
2. Copy lines 129-267 (Configure branch body) into new method
3. Remove the opening `else if (state == State.Configure) {` and closing `}`
4. Verify method signature: `private void HandleConfigure()`

### Step 2: Extract InitializeMmioMirror from HandleConfigure
1. Position: Insert after `HandleConfigure`
2. Copy lines 237-248 (MMIO mirror setup) from HandleConfigure into new method
3. Replace those lines in HandleConfigure with: `InitializeMmioMirror();`
4. Verify method signature: `private void InitializeMmioMirror()`

### Step 3: Replace Original Branch
1. In `ProcessOnStateChange`, replace lines 129-267 with:
```csharp
else if (state == State.Configure)
{
    HandleConfigure();
}
```

### Step 4: Verify Extraction
1. **Line Count**: HandleConfigure ~100 lines, InitializeMmioMirror ~12 lines
2. **No Logic Changes**: Exact copy of original code
3. **Indentation**: Adjust for method bodies
4. **Static Assert**: Verify validation block preserved in HandleConfigure
5. **Try-Catch**: Verify MMIO try-catch preserved in InitializeMmioMirror

---

## COMPLEXITY ESTIMATE

**Before**:
- ProcessOnStateChange: CYC 81 (after Tickets 01-02)

**After**:
- ProcessOnStateChange: CYC 76 (one more branch extracted)
- HandleConfigure: CYC ~8 (after MMIO extraction)
- InitializeMmioMirror: CYC ~3 (try-catch only)

---

## VERIFICATION CHECKLIST

### Pre-Extraction
- [ ] Read current ProcessOnStateChange source
- [ ] Identify lines 129-267 (Configure branch)
- [ ] Identify lines 237-248 (MMIO mirror setup within Configure)
- [ ] Verify Tickets 01-02 completed

### Post-Extraction
- [ ] HandleConfigure exists after HandleTerminated
- [ ] InitializeMmioMirror exists after HandleConfigure
- [ ] HandleConfigure calls InitializeMmioMirror()
- [ ] ProcessOnStateChange calls HandleConfigure()
- [ ] No duplicate code remains
- [ ] Indentation is correct

### Complexity Audit
- [ ] Run: `python scripts/complexity_audit.py`
- [ ] Verify: ProcessOnStateChange CYC 81 → 76
- [ ] Verify: HandleConfigure CYC ≤8
- [ ] Verify: InitializeMmioMirror CYC ≤3

### Build Gate
- [ ] Run: `powershell -File .\scripts\build_readiness.ps1`
- [ ] Verify: Zero compilation errors
- [ ] Verify: CSharpier formatting passes

### V12 DNA Compliance
- [ ] Lock Audit: `grep -r "lock(" src/V12_002.Lifecycle.cs` → zero matches
- [ ] Unicode Audit: Verify ASCII-only in all string literals
- [ ] LOC Floor: HandleConfigure ≥15 lines ✅ (~100 lines)
- [ ] LOC Floor: InitializeMmioMirror ≥15 lines ❌ (~12 lines, acceptable for sub-method)

### F5 Gate (MANDATORY)
- [ ] Run: `powershell -File .\deploy-sync.ps1`
- [ ] Press F5 in NinjaTrader IDE
- [ ] Verify: BUILD_TAG banner visible
- [ ] Verify: MMIO mirror initialization (check console output)
- [ ] Type: `F5 done [BUILD_TAG]`

### Commit
- [ ] Run: `git add src/V12_002.Lifecycle.cs`
- [ ] Run: `git commit -m "[EPIC-CCN-13] ticket-03: extract HandleConfigure + InitializeMmioMirror -- CYC 81->76 [BUILD_TAG]"`

---

## RISK ASSESSMENT

**Risk Level**: MEDIUM

**Why Medium Risk**:
- MMIO mirror setup has try-catch error handling
- Static assert validation throws exception on failure
- Multiple collection initializations
- Photon pool setup is complex

**Critical Sections**:
1. **Static Assert** (lines 217-234): Must preserve exact validation logic
2. **MMIO Mirror** (lines 237-248): Must preserve exact try-catch behavior
3. **Collection Init**: Must preserve exact capacity parameters

**Potential Issues**:
- MMIO mirror failure (already handled with try-catch)
- Static assert failure (already throws exception)
- Collection initialization order (no dependencies detected)

---

## ROLLBACK PROCEDURE

If F5 gate fails:
1. Run: `git reset --hard HEAD~1`
2. Verify: ProcessOnStateChange restored to previous state
3. Report failure in `docs/brain/EPIC-CCN-13/ticket-03-failure.md`
4. Escalate to Director

---

## SUCCESS CRITERIA

- [ ] HandleConfigure method created
- [ ] InitializeMmioMirror method created
- [ ] HandleConfigure calls InitializeMmioMirror()
- [ ] ProcessOnStateChange calls HandleConfigure()
- [ ] CYC reduced: 81 → 76
- [ ] HandleConfigure CYC ≤8
- [ ] InitializeMmioMirror CYC ≤3
- [ ] Build passes
- [ ] F5 verification passes
- [ ] MMIO mirror initializes correctly
- [ ] Zero locks, zero Unicode
- [ ] Commit created with BUILD_TAG

---

## DEPENDENCIES

**Upstream**: Tickets 01-02 (HandleSetDefaults, HandleTerminated must exist)
**Downstream**: None (independent extraction)

---

## NOTES

- First MEDIUM risk ticket
- InitializeMmioMirror is <15 LOC (acceptable for sub-method)
- Static assert validation must be preserved exactly (ADR-016 compliance)
- MMIO mirror failure is non-fatal (already handled)
- Watch for MMIO mirror console output during F5 verification

---

**Status**: READY FOR EXECUTION
**Assigned To**: v12-engineer mode
**Estimated Time**: 25 minutes