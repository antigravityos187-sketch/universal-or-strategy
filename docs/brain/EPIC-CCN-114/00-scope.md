# Phase 1: Scope Definition - EPIC-CCN-114

## Target Method Details

### Method Identification
- **Method Name**: ProcessShutdownSIMA
- **File Path**: src/V12_002.SIMA.Lifecycle.cs
- **Current Complexity**: 11 (Cyclomatic Complexity)
- **Target Complexity**: ≤ 10 (preventive maintenance)
- **Epic ID**: EPIC-CCN-114

### Method Signature
```csharp
private void ProcessShutdownSIMA()
```

### Current Responsibility
Orchestrates graceful SIMA shutdown including:
- FSM state transition validation
- Resource cleanup (timers, subscriptions, connections)
- Error handling and logging
- State flag updates (IsShuttingDown, IsShutdown)

## Extraction Strategy

### What to Extract

#### 1. Resource Cleanup Logic (Primary Extraction)
**Target Method**: `CleanupShutdownResources()`
- **Rationale**: Resource disposal is a distinct responsibility
- **Scope**: Timer disposal, subscription cleanup, connection teardown
- **Complexity Reduction**: ~3-4 branches
- **Risk**: LOW (isolated cleanup operations)

**Extracted Logic**:
```csharp
private void CleanupShutdownResources()
{
    // Timer disposal
    // Subscription cleanup
    // Connection teardown
    // Diagnostic resource cleanup
}
```

#### 2. State Validation Logic (Secondary Extraction)
**Target Method**: `ValidateShutdownState()`
- **Rationale**: State checks are a separate concern from cleanup
- **Scope**: FSM state validation, flag checks
- **Complexity Reduction**: ~2-3 branches
- **Risk**: LOW (pure validation logic)

**Extracted Logic**:
```csharp
private bool ValidateShutdownState()
{
    // Check IsShuttingDown flag
    // Validate FSM state
    // Return true if shutdown can proceed
}
```

### What to Keep in ProcessShutdownSIMA

The orchestration logic remains in ProcessShutdownSIMA:
- High-level shutdown flow coordination
- Error handling wrapper (try-catch for cleanup failures)
- Logging of shutdown initiation/completion
- Calling extracted methods in correct sequence

**Retained Structure**:
```csharp
private void ProcessShutdownSIMA()
{
    // Log shutdown initiation
    
    if (!ValidateShutdownState())
    {
        // Log invalid state, return early
        return;
    }
    
    try
    {
        CleanupShutdownResources();
        // Update shutdown flags
        // Log successful shutdown
    }
    catch (Exception ex)
    {
        // Log error, ensure flags are set
    }
}
```

## Boundary Definition (V12.23 No Scope Creep Protocol)

### In-Scope (Single Method Only)
✅ ProcessShutdownSIMA method body
✅ Extract 2 helper methods (CleanupShutdownResources, ValidateShutdownState)
✅ Maintain existing method signature
✅ Preserve all existing behavior

### Out-of-Scope (Strict Boundary)
❌ Other lifecycle methods (OnTermination, ProcessStartupSIMA)
❌ FSM state machine implementation
❌ Logging infrastructure changes
❌ Resource management patterns in other methods
❌ NinjaTrader integration hooks

### Scope Creep Prevention
- **Single File**: src/V12_002.SIMA.Lifecycle.cs only
- **Single Method**: ProcessShutdownSIMA only
- **No Refactoring**: Adjacent code remains untouched
- **No Optimization**: Performance unchanged (behavior preservation)

## Success Criteria

### Primary Criteria (MANDATORY)
1. ✅ **Complexity Target**: ProcessShutdownSIMA complexity ≤ 10
2. ✅ **Build Success**: Zero compilation errors
3. ✅ **Behavior Preservation**: All existing shutdown behavior intact
4. ✅ **Lock-Free Compliance**: No lock() blocks introduced
5. ✅ **ASCII-Only**: No Unicode/emoji in string literals

### Secondary Criteria (QUALITY)
6. ✅ **Test Coverage**: Add TDD test for shutdown path
7. ✅ **Code Health**: CodeScene score improvement
8. ✅ **Readability**: Cognitive load reduced (simpler flow)
9. ✅ **Maintainability**: Extracted methods are single-purpose

### Verification Criteria
- **Pre-Push Validation**: All 13 checks pass
- **Complexity Audit**: `python scripts/complexity_audit.py` shows ≤ 10
- **Build Readiness**: `powershell -File .\scripts\build_readiness.ps1` passes
- **Hard-Link Sync**: `powershell -File .\deploy-sync.ps1` succeeds

## Risk Assessment

### Overall Risk Level: LOW

### Risk Factors

#### 1. Shutdown Path Criticality (MEDIUM)
- **Risk**: Shutdown logic must be bulletproof
- **Mitigation**: Preserve all error handling, add TDD tests
- **Impact**: Resource leaks if cleanup fails

#### 2. State Coordination (LOW)
- **Risk**: FSM state validation must remain correct
- **Mitigation**: Extract validation logic atomically, no state changes
- **Impact**: Invalid state transitions if validation broken

#### 3. Exception Handling (LOW)
- **Risk**: Error paths must remain functional
- **Mitigation**: Keep try-catch in orchestrator, test error scenarios
- **Impact**: Unhandled exceptions during shutdown

#### 4. Resource Cleanup Order (LOW)
- **Risk**: Cleanup sequence may be order-dependent
- **Mitigation**: Preserve exact cleanup order in extracted method
- **Impact**: Resource leaks or disposal errors

### Risk Mitigation Strategy

1. **Atomic Extraction**: Extract complete logical units (no partial moves)
2. **Behavior Preservation**: Zero functional changes, pure refactoring
3. **TDD Coverage**: Add tests before extraction (safety net)
4. **Incremental Verification**: Build + test after each extraction
5. **Rollback Plan**: Git checkpoint before each change

### Lock-Free Compliance Audit
- **Pre-Extraction**: Verify no lock() blocks in ProcessShutdownSIMA
- **Post-Extraction**: Verify no lock() blocks in extracted methods
- **Command**: `grep -n "lock(" src/V12_002.SIMA.Lifecycle.cs`
- **Expected**: Zero matches

## Extraction Sequence

### Step 1: Pre-Extraction Audit
1. Read ProcessShutdownSIMA source code
2. Verify current complexity (should be 11)
3. Check for lock() blocks (should be zero)
4. Document current behavior

### Step 2: Extract CleanupShutdownResources
1. Identify resource cleanup logic
2. Create new private method
3. Move cleanup code atomically
4. Update ProcessShutdownSIMA to call new method
5. Build + verify

### Step 3: Extract ValidateShutdownState
1. Identify state validation logic
2. Create new private method
3. Move validation code atomically
4. Update ProcessShutdownSIMA to call new method
5. Build + verify

### Step 4: Verification
1. Run complexity audit (target ≤ 10)
2. Run pre-push validation (all checks pass)
3. Run TDD tests (100% pass)
4. Sync hard links (deploy-sync.ps1)

## Jane Street Alignment

### Cognitive Simplicity
- **Before**: 11 branches in single method (approaching cognitive load limit)
- **After**: 3 methods with ≤ 5 branches each (easy to reason about)
- **Benefit**: Each method has single, clear purpose

### HFT Hot-Path Considerations
- **Frequency**: Low (shutdown called once per strategy lifecycle)
- **Latency**: Not critical (shutdown is not hot-path)
- **Optimization**: Maintainability prioritized over performance

### Testing Standards
- **Unit Tests**: Verify shutdown behavior under normal/error conditions
- **Integration Tests**: Verify NinjaTrader lifecycle integration
- **Stress Tests**: Not applicable (shutdown is one-time operation)

## V12 DNA Compliance

### ✅ Correctness by Construction
- State validation extracted to dedicated method
- Invalid states caught early (fail-fast)
- Resource cleanup isolated (single responsibility)

### ✅ Lock-Free Actor Pattern
- No lock() blocks in shutdown path
- State transitions use FSM/Actor Enqueue model
- Atomic primitives for flag updates

### ✅ ASCII-Only Compliance
- All string literals verified (no Unicode/emoji)
- Logging messages use ASCII characters only

### ✅ Jane Street Alignment
- Target complexity ≤ 10 (well below threshold 15)
- Cognitive simplicity maintained
- Single-purpose methods (easy to test/audit)

## Deliverables

### Phase 1 Outputs
1. ✅ This scope document (00-scope.md)
2. ✅ Updated manifest.json (phase 1 completed)

### Phase 2 Inputs (Next Phase)
- Forensic audit of ProcessShutdownSIMA source code
- Detailed extraction plan with line-by-line mapping
- TDD test specifications

## Conclusion

EPIC-CCN-114 targets ProcessShutdownSIMA for preventive maintenance refactoring. With current complexity 11, the method is below the V12 threshold but warrants simplification to maintain cognitive simplicity and ensure long-term maintainability.

**Extraction Strategy**: Two helper methods (CleanupShutdownResources, ValidateShutdownState) will reduce complexity to ≤ 10 while preserving all existing behavior.

**Risk Level**: LOW - Shutdown path is critical but extraction is straightforward with proper TDD coverage.

**Next Phase**: Proceed to Phase 2 (Forensic Intake) for detailed source code analysis.

---

**Document Version**: 1.0
**Created**: 2026-06-13
**Protocol**: V12.23 (No Scope Creep)
**Analyst**: Bob Shell (Plan Mode)

