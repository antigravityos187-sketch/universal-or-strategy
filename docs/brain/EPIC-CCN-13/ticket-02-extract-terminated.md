# EPIC-CCN-13 Ticket 02: Extract HandleTerminated

**Epic**: EPIC-CCN-13 - Extract ProcessOnStateChange (CYC 91 → ≤8)
**Ticket**: 02 of 5
**Risk Level**: LOW
**Estimated Duration**: 15 minutes
**Target CYC**: ~6 (acceptable)

---

## OBJECTIVE

Extract the `State.Terminated` branch from ProcessOnStateChange into a dedicated `HandleTerminated()` method. This is straightforward cleanup logic with defensive null checks, making it the second-lowest risk extraction.

---

## CURRENT STATE

**File**: [`src/V12_002.Lifecycle.cs`](../../src/V12_002.Lifecycle.cs:520)
**Method**: `ProcessOnStateChange(State state)`
**Lines**: 520-565 (46 lines)
**Current CYC**: 86 (after Ticket 01)

**Target Section**:
```csharp
else if (state == State.Terminated)
{
    _isTerminating = true;
    StopWatchdog();
    
    _configureComplete = false;
    _dataLoadedComplete = false;
    Interlocked.Exchange(ref _startupReadinessLogEmitted, 0);
    
    StopPanelRefresh();
    
    if (ChartControl != null)
    {
        ChartControl.Dispatcher.InvokeAsync(() =>
        {
            DetachHotkeys();
            DetachChartClickHandler();
            DestroyPanel();
        });
    }
    
    CancelAllV12GtcOrders(false);
    DrainQueuesForShutdown();
    EmitMetricsSummary();
    StopIpcServer();
    StopReaperAudit();
    UnsubscribeFromFleetAccounts();
    
    // MMIO mirror disposal
    if (_photonMmioMirror != null)
    {
        try
        {
            _photonMmioMirror.Dispose();
        }
        catch { }
        _photonMmioMirror = null;
    }
    
    SignalBroadcaster.ClearAllSubscribers();
    _simaToggleSem?.Dispose();
    
    // Clear collections
    activePositions?.Clear();
    // ... more clears ...
}
```

---

## TARGET STATE

**New Method Signature**:
```csharp
private void HandleTerminated()
```

**Modified ProcessOnStateChange**:
```csharp
private void ProcessOnStateChange(State state)
{
    if (state == State.SetDefaults)
    {
        HandleSetDefaults();
    }
    else if (state == State.Configure)
    {
        // ... existing code ...
    }
    else if (state == State.DataLoaded)
    {
        // ... existing code ...
    }
    else if (state == State.Realtime)
    {
        // ... existing code ...
    }
    else if (state == State.Terminated)
    {
        HandleTerminated();
    }
}
```

---

## EXTRACTION STEPS

### Step 1: Create HandleTerminated Method
1. Position: Insert new method after `HandleSetDefaults` (created in Ticket 01)
2. Copy lines 520-565 (Terminated branch body) into new method
3. Remove the opening `else if (state == State.Terminated) {` and closing `}`
4. Verify method signature: `private void HandleTerminated()`

### Step 2: Replace Original Branch
1. In `ProcessOnStateChange`, replace lines 520-565 with:
```csharp
else if (state == State.Terminated)
{
    HandleTerminated();
}
```

### Step 3: Verify Extraction
1. **Line Count**: HandleTerminated should be ~46 lines
2. **No Logic Changes**: Exact copy of original code
3. **Indentation**: Adjust for method body (remove one level)
4. **Null Checks**: Verify `if (ChartControl != null)` and `if (_photonMmioMirror != null)` preserved
5. **Try-Catch**: Verify MMIO disposal try-catch preserved

---

## COMPLEXITY ESTIMATE

**Before**:
- ProcessOnStateChange: CYC 86 (after Ticket 01)

**After**:
- ProcessOnStateChange: CYC 81 (one more branch extracted)
- HandleTerminated: CYC ~6 (if + try-catch)

---

## VERIFICATION CHECKLIST

### Pre-Extraction
- [ ] Read current ProcessOnStateChange source
- [ ] Identify lines 520-565 (Terminated branch)
- [ ] Verify Ticket 01 completed (HandleSetDefaults exists)

### Post-Extraction
- [ ] HandleTerminated exists after HandleSetDefaults
- [ ] HandleTerminated contains exact copy of lines 520-565
- [ ] ProcessOnStateChange calls HandleTerminated()
- [ ] No duplicate code remains
- [ ] Indentation is correct

### Complexity Audit
- [ ] Run: `python scripts/complexity_audit.py`
- [ ] Verify: ProcessOnStateChange CYC 86 → 81
- [ ] Verify: HandleTerminated CYC ~6

### Build Gate
- [ ] Run: `powershell -File .\scripts\build_readiness.ps1`
- [ ] Verify: Zero compilation errors
- [ ] Verify: CSharpier formatting passes

### V12 DNA Compliance
- [ ] Lock Audit: `grep -r "lock(" src/V12_002.Lifecycle.cs` → zero matches
- [ ] Unicode Audit: Verify ASCII-only in all string literals
- [ ] LOC Floor: HandleTerminated ≥15 lines ✅ (46 lines)

### F5 Gate (MANDATORY)
- [ ] Run: `powershell -File .\deploy-sync.ps1`
- [ ] Press F5 in NinjaTrader IDE
- [ ] Verify: BUILD_TAG banner visible
- [ ] Type: `F5 done [BUILD_TAG]`

### Commit
- [ ] Run: `git add src/V12_002.Lifecycle.cs`
- [ ] Run: `git commit -m "[EPIC-CCN-13] ticket-02: extract HandleTerminated -- CYC 86->81 [BUILD_TAG]"`

---

## RISK ASSESSMENT

**Risk Level**: LOW

**Why Low Risk**:
- Straightforward cleanup logic
- Defensive null checks (safe)
- Try-catch for MMIO disposal (safe)
- No complex branching

**Potential Issues**:
- MMIO disposal failure (already handled with try-catch)
- ChartControl null (already handled with if check)

---

## ROLLBACK PROCEDURE

If F5 gate fails:
1. Run: `git reset --hard HEAD~1`
2. Verify: ProcessOnStateChange restored to previous state
3. Report failure in `docs/brain/EPIC-CCN-13/ticket-02-failure.md`
4. Escalate to Director

---

## SUCCESS CRITERIA

- [ ] HandleTerminated method created
- [ ] ProcessOnStateChange calls HandleTerminated()
- [ ] CYC reduced: 86 → 81
- [ ] HandleTerminated CYC ~6 (acceptable)
- [ ] Build passes
- [ ] F5 verification passes
- [ ] Zero locks, zero Unicode
- [ ] Commit created with BUILD_TAG

---

## DEPENDENCIES

**Upstream**: Ticket 01 (HandleSetDefaults must exist)
**Downstream**: None (independent extraction)

---

## NOTES

- Second warm-up ticket (LOW risk)
- HandleTerminated CYC ~6 is acceptable (cleanup logic)
- No sub-method extraction needed
- MMIO disposal try-catch must be preserved exactly

---

**Status**: READY FOR EXECUTION
**Assigned To**: v12-engineer mode
**Estimated Time**: 15 minutes