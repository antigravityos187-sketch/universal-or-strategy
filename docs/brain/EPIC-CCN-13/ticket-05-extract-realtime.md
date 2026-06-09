# EPIC-CCN-13 Ticket 05: Extract HandleRealtime + AttachUiComponents

**Epic**: EPIC-CCN-13 - Extract ProcessOnStateChange (CYC 91 → ≤8)
**Ticket**: 05 of 5 (FINAL)
**Risk Level**: MEDIUM
**Estimated Duration**: 25 minutes
**Target CYC**: HandleRealtime ≤8, AttachUiComponents ≤5

---

## OBJECTIVE

Extract the `State.Realtime` branch from ProcessOnStateChange into `HandleRealtime()`, then further extract the UI attachment logic into `AttachUiComponents()` to achieve CYC ≤8 for both methods. This completes the epic, reducing ProcessOnStateChange from CYC 91 to CYC 5.

---

## CURRENT STATE

**File**: [`src/V12_002.Lifecycle.cs`](../../src/V12_002.Lifecycle.cs:451)
**Method**: `ProcessOnStateChange(State state)`
**Lines**: 451-518 (68 lines)
**Current CYC**: 71 (after Tickets 01-04)

**Target Section**:
```csharp
else if (state == State.Realtime)
{
    Print("+--------------------------------------------------------------+");
    Print("|          [OK] BMad HARDENED DEPLOYMENT PROTOCOL ACTIVE       |");
    Print(string.Format("|          Build: {0,-10} |  Sync: ONE SOURCE OF TRUTH    |", BUILD_TAG));
    Print("+--------------------------------------------------------------+");
    TouchStrategyHeartbeat();
    PublishUiSnapshot();
    StartWatchdog();
    
    if (EnableSIMA)
    {
        // Route realtime SIMA startup through the actor queue
        Enqueue(ctx =>
        {
            ctx.EnumerateApexAccounts();
            if (ctx.ReaperAuditEnabled)
                ctx.StartReaperAudit();
        });
    }
    
    // UI attachment (lines 489-518) ← EXTRACT THIS
    if (ChartControl != null)
    {
        // Hotkeys attach at Normal priority
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
        
        // Panel creation deferred to Loaded priority
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

---

## TARGET STATE

**New Method Signatures**:
```csharp
private void HandleRealtime()
private void AttachUiComponents()
```

**Modified ProcessOnStateChange** (FINAL STATE):
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

**HandleRealtime** (calls AttachUiComponents):
```csharp
private void HandleRealtime()
{
    Print("+--------------------------------------------------------------+");
    Print("|          [OK] BMad HARDENED DEPLOYMENT PROTOCOL ACTIVE       |");
    Print(string.Format("|          Build: {0,-10} |  Sync: ONE SOURCE OF TRUTH    |", BUILD_TAG));
    Print("+--------------------------------------------------------------+");
    TouchStrategyHeartbeat();
    PublishUiSnapshot();
    StartWatchdog();
    
    if (EnableSIMA)
    {
        // Route realtime SIMA startup through the actor queue
        Enqueue(ctx =>
        {
            ctx.EnumerateApexAccounts();
            if (ctx.ReaperAuditEnabled)
                ctx.StartReaperAudit();
        });
    }
    
    AttachUiComponents(); // ← Call sub-method
}
```

**AttachUiComponents** (extracted):
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

---

## EXTRACTION STEPS

### Step 1: Create HandleRealtime Method
1. Position: Insert after `InitializeTargetConfiguration` (created in Ticket 04)
2. Copy lines 451-518 (Realtime branch body) into new method
3. Remove the opening `else if (state == State.Realtime) {` and closing `}`
4. Verify method signature: `private void HandleRealtime()`

### Step 2: Extract AttachUiComponents
1. Position: Insert after `HandleRealtime`
2. Copy lines 489-518 (UI attachment logic) from HandleRealtime into new method
3. Replace those lines in HandleRealtime with: `AttachUiComponents();`
4. Verify method signature: `private void AttachUiComponents()`

### Step 3: Replace Original Branch
1. In `ProcessOnStateChange`, replace lines 451-518 with:
```csharp
else if (state == State.Realtime)
{
    HandleRealtime();
}
```

### Step 4: Verify Final State
1. **ProcessOnStateChange**: Should now be ~15 lines (5 if-else branches)
2. **All Handlers**: HandleSetDefaults, HandleConfigure, HandleDataLoaded, HandleRealtime, HandleTerminated
3. **All Sub-Methods**: InitializeMmioMirror, InitializeInstrumentSettings, InitializeTargetConfiguration, AttachUiComponents
4. **Total Methods**: 10 (1 modified + 9 new)

---

## COMPLEXITY ESTIMATE

**Before**:
- ProcessOnStateChange: CYC 71 (after Tickets 01-04)

**After** (FINAL STATE):
- ProcessOnStateChange: CYC 5 ✅ (simple dispatcher)
- HandleRealtime: CYC ~8 (if + Enqueue lambda)
- AttachUiComponents: CYC ~5 (if + 2 lambdas with nested if)

**Epic Total Reduction**: CYC 91 → 5 (86-point reduction)

---

## VERIFICATION CHECKLIST

### Pre-Extraction
- [ ] Read current ProcessOnStateChange source
- [ ] Identify lines 451-518 (Realtime branch)
- [ ] Identify lines 489-518 (UI attachment logic)
- [ ] Verify Tickets 01-04 completed

### Post-Extraction
- [ ] HandleRealtime exists after InitializeTargetConfiguration
- [ ] AttachUiComponents exists after HandleRealtime
- [ ] HandleRealtime calls AttachUiComponents()
- [ ] ProcessOnStateChange calls HandleRealtime()
- [ ] No duplicate code remains
- [ ] Indentation is correct

### Final Epic Verification
- [ ] ProcessOnStateChange is ~15 lines (5 branches)
- [ ] All 5 state handlers exist
- [ ] All 4 sub-methods exist
- [ ] Total: 10 methods (1 modified + 9 new)

### Complexity Audit (FINAL)
- [ ] Run: `python scripts/complexity_audit.py`
- [ ] Verify: ProcessOnStateChange CYC 71 → 5 ✅
- [ ] Verify: HandleRealtime CYC ≤8
- [ ] Verify: AttachUiComponents CYC ≤5
- [ ] Verify: ALL methods CYC ≤15 (Jane Street GODMODE achieved)

### Build Gate
- [ ] Run: `powershell -File .\scripts\build_readiness.ps1`
- [ ] Verify: Zero compilation errors
- [ ] Verify: CSharpier formatting passes

### V12 DNA Compliance (FINAL)
- [ ] Lock Audit: `grep -r "lock(" src/V12_002.Lifecycle.cs` → zero matches
- [ ] Unicode Audit: Verify ASCII-only in all string literals
- [ ] LOC Floor: HandleRealtime ≥15 lines ✅ (~40 lines)
- [ ] LOC Floor: AttachUiComponents ≥15 lines ✅ (~30 lines)

### F5 Gate (MANDATORY - FINAL)
- [ ] Run: `powershell -File .\deploy-sync.ps1`
- [ ] Press F5 in NinjaTrader IDE
- [ ] Verify: BUILD_TAG banner visible
- [ ] Verify: "BMad HARDENED DEPLOYMENT PROTOCOL ACTIVE" banner
- [ ] Verify: UI components attach (hotkeys, panel)
- [ ] Verify: SIMA startup (if enabled)
- [ ] Type: `F5 done [BUILD_TAG]`

### Commit (FINAL)
- [ ] Run: `git add src/V12_002.Lifecycle.cs`
- [ ] Run: `git commit -m "[EPIC-CCN-13] ticket-05: extract HandleRealtime + AttachUiComponents -- CYC 71->5 EPIC COMPLETE [BUILD_TAG]"`

---

## RISK ASSESSMENT

**Risk Level**: MEDIUM

**Why Medium Risk**:
- UI thread synchronization (Dispatcher.InvokeAsync)
- Two different dispatcher priorities (Normal vs Loaded)
- Nested lambdas with _isTerminating checks
- SIMA conditional startup via Enqueue

**Critical Sections**:
1. **Dispatcher Priority Ordering**: Normal (hotkeys) BEFORE Loaded (panel)
2. **_isTerminating Checks**: Must be preserved in both lambdas
3. **SIMA Enqueue**: Must preserve actor queue ordering

**Potential Issues**:
- UI attachment timing (already handled with priority ordering)
- _isTerminating race (already handled with checks)
- SIMA startup failure (already handled via Enqueue)

---

## ROLLBACK PROCEDURE

If F5 gate fails:
1. Run: `git reset --hard HEAD~1`
2. Verify: ProcessOnStateChange restored to previous state
3. Report failure in `docs/brain/EPIC-CCN-13/ticket-05-failure.md`
4. Escalate to Director

---

## SUCCESS CRITERIA (EPIC COMPLETION)

### Ticket-Level
- [ ] HandleRealtime method created
- [ ] AttachUiComponents method created
- [ ] HandleRealtime calls AttachUiComponents()
- [ ] ProcessOnStateChange calls HandleRealtime()
- [ ] CYC reduced: 71 → 5 ✅

### Epic-Level
- [ ] ProcessOnStateChange CYC: 91 → 5 ✅ (86-point reduction)
- [ ] All 5 state handlers created
- [ ] All 4 sub-methods created
- [ ] All methods CYC ≤15 (Jane Street GODMODE achieved)
- [ ] Build passes
- [ ] F5 verification passes
- [ ] UI components attach correctly
- [ ] Zero locks, zero Unicode
- [ ] Commit created with BUILD_TAG

---

## DEPENDENCIES

**Upstream**: Tickets 01-04 (all previous handlers must exist)
**Downstream**: None (final ticket)

---

## NOTES

- **FINAL TICKET** - Completes EPIC-CCN-13
- UI thread synchronization is critical
- Dispatcher priority ordering must be preserved (Normal → Loaded)
- _isTerminating checks prevent race conditions
- Watch for UI attachment console output during F5 verification
- After this ticket, ProcessOnStateChange will be a simple 5-branch dispatcher

---

## EPIC COMPLETION METRICS

**Before EPIC-CCN-13**:
- ProcessOnStateChange: CYC 91, 522 lines
- Methods: 1 (monolithic)

**After EPIC-CCN-13**:
- ProcessOnStateChange: CYC 5, ~15 lines
- State Handlers: 5 methods (HandleSetDefaults, HandleConfigure, HandleDataLoaded, HandleRealtime, HandleTerminated)
- Sub-Methods: 4 methods (InitializeMmioMirror, InitializeInstrumentSettings, InitializeTargetConfiguration, AttachUiComponents)
- Total Methods: 10 (1 modified + 9 new)
- Total CYC Reduction: 86 points
- Jane Street GODMODE: ✅ ACHIEVED (all methods ≤15)

---

**Status**: READY FOR EXECUTION
**Assigned To**: v12-engineer mode
**Estimated Time**: 25 minutes
**Epic Status**: FINAL TICKET - EPIC COMPLETION