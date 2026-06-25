# Phase 1: Scope Definition - EPIC-W7-085

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Mode**: plan
- **Bobcoins Used**: 0.00
- **Execution Time**: 2026-06-24T19:35:21Z

## Epic Overview
- **Target Method**: `AuditMaster_HandleDesyncFlatten`
- **File**: `src/V12_002.REAPER.Audit.cs`
- **Line**: 582
- **Current CYC**: 12
- **Target CYC**: ≤8
- **Reduction Required**: 4 points (33% reduction)

## Scope Boundaries

### What Will Be Extracted
Based on the 6-level nesting depth and CYC=12, we will extract **2-3 helper methods** to handle:

1. **Desync Condition Validation** (Extract #1)
   - **Purpose**: Validate desync conditions and determine if flattening is needed
   - **Logic**: Check if `expectedQty != actualQty` and validate flatten eligibility
   - **Expected CYC**: 3-4
   - **Lines**: ~10-12 lines

2. **Flatten Execution Logic** (Extract #2)
   - **Purpose**: Execute the flatten operation when conditions are met
   - **Logic**: Enqueue flatten request and update in-flight tracking
   - **Expected CYC**: 2-3
   - **Lines**: ~8-10 lines

3. **Flatten Queue Processing** (Extract #3 - if needed)
   - **Purpose**: Process the flatten queue if not already in flight
   - **Logic**: Check in-flight status and trigger queue processing
   - **Expected CYC**: 2-3
   - **Lines**: ~6-8 lines

### What Will Remain
The orchestrator method `AuditMaster_HandleDesyncFlatten` will remain as a **coordination method** that:
- Receives the 3 parameters: `account`, `expectedQty`, `actualQty`
- Calls the extracted validation method
- Calls the extracted execution method(s) based on validation result
- Maintains the overall flow control
- **Target CYC**: ≤5 (orchestration only)

### Extraction Strategy
**Pattern**: Nested Conditional Extraction
- Extract deeply nested conditional blocks (levels 4-6) into single-purpose methods
- Each extracted method should have a clear, testable responsibility
- Use descriptive method names that reflect business logic (e.g., `ShouldFlattenForDesync`, `ExecuteFlattenOperation`)

## Dependencies

### Internal Dependencies (Within Method)
- `_reaperFlattenInFlight` (constant/field)
- `_reaperFlattenQueue` (constant/field)
- `AuditMaster_CheckExpectedActual` (line 706)
- `EnqueueReaperMasterFlatten` (line 745)
- `ProcessReaperFlattenQueue` (line 800)

### External Dependencies (Callers)
1. **AuditMasterAccountIfNeeded** (line 684)
   - Direct caller - no signature changes needed
2. **AuditApexPositions** (line 16)
   - Indirect caller - no changes needed

### Downstream Dependencies (Callees)
The extracted methods will call the same downstream methods:
- ProcessReaperFlatten_FindAccount
- ProcessReaperFlatten_CancelWorkingOrders
- ProcessReaperFlatten_ClosePositions
- ProcessReaperFlatten_TerminateFsms
- CancelOrderOnAccount
- TerminateFsmsForAccount

**Impact**: No changes to downstream call sites - only internal reorganization.

## Risk Assessment

### Refactoring Risks
1. **Logic Preservation Risk**: LOW
   - Clear conditional boundaries
   - Well-defined input/output for each block
   - No complex state mutations

2. **Regression Risk**: LOW
   - Zero blast radius (no external dependencies)
   - Only 2 call sites to verify
   - Private method with clear contract

3. **Testing Risk**: LOW
   - Existing callers provide integration test coverage
   - Extracted methods will be unit-testable
   - No changes to public API

### Mitigation Strategies
1. **Preserve Exact Logic**: Copy-paste conditional blocks, then refactor
2. **Verify Call Sites**: Test both `AuditMasterAccountIfNeeded` and `AuditApexPositions` after extraction
3. **Add Unit Tests**: Create tests for each extracted method (xUnit framework)
4. **Build Verification**: Run `dotnet build` after each extraction
5. **Hard Link Sync**: Run `deploy-sync.ps1` after all changes

## Success Criteria

### Quantitative Metrics
- ✅ **CYC Reduction**: `AuditMaster_HandleDesyncFlatten` CYC reduced from 12 to ≤8
- ✅ **Extracted Methods**: 2-3 new methods, each with CYC ≤4
- ✅ **Nesting Depth**: Reduced from 6 to ≤3 levels
- ✅ **Build Success**: Zero compilation errors
- ✅ **Test Coverage**: xUnit tests for each extracted method

### Qualitative Metrics
- ✅ **Readability**: Each method has a single, clear responsibility
- ✅ **Naming**: Method names reflect business logic (not technical implementation)
- ✅ **Maintainability**: Reduced cognitive load for future developers
- ✅ **Jane Street Alignment**: Adheres to CYC ≤8 strict standard

### Verification Steps
1. **Static Analysis**: Run `python scripts/complexity_audit.py --threshold 8`
2. **Build Verification**: Run `dotnet build` (zero errors)
3. **Hard Link Sync**: Run `powershell -File .\deploy-sync.ps1`
4. **Integration Test**: F5 in NinjaTrader IDE, verify BUILD_TAG
5. **Call Site Verification**: Manually verify both caller methods still work

## Extraction Targets (Detailed)

### Target #1: Desync Validation
**Proposed Name**: `ShouldFlattenForDesync`
**Signature**: `private bool ShouldFlattenForDesync(IAccount account, int expectedQty, int actualQty)`
**Responsibility**: Determine if flatten operation is needed based on desync conditions
**Expected CYC**: 3-4
**Returns**: `true` if flatten should proceed, `false` otherwise

### Target #2: Flatten Execution
**Proposed Name**: `ExecuteFlattenForDesync`
**Signature**: `private void ExecuteFlattenForDesync(IAccount account)`
**Responsibility**: Execute the flatten operation (enqueue and track)
**Expected CYC**: 2-3
**Returns**: void (side effects: updates queue and in-flight tracking)

### Target #3: Queue Processing (Optional)
**Proposed Name**: `ProcessFlattenQueueIfReady`
**Signature**: `private void ProcessFlattenQueueIfReady()`
**Responsibility**: Process flatten queue if not already in flight
**Expected CYC**: 2-3
**Returns**: void (side effects: triggers queue processing)

## Boundary Validation

### Clear Boundaries
- ✅ **Input Boundary**: 3 parameters (`account`, `expectedQty`, `actualQty`)
- ✅ **Output Boundary**: void return (side effects on queue/tracking)
- ✅ **State Boundary**: Accesses `_reaperFlattenInFlight` and `_reaperFlattenQueue`
- ✅ **Call Boundary**: Calls 5 downstream methods (no changes to their signatures)

### Scope Constraints
- ❌ **No Signature Changes**: Method signature remains unchanged
- ❌ **No Caller Changes**: Both callers remain unchanged
- ❌ **No Downstream Changes**: Callees remain unchanged
- ✅ **Internal Only**: All changes are internal to the method body

## Next Phase
**Phase 1.5**: Scope Boundary Validation
- Validate extraction targets against V12 DNA principles
- Confirm no scope creep
- Verify Jane Street alignment
- Approve for Phase 2 (Architecture Planning)

---

**Scope Definition Complete**: Ready for Phase 1.5 validation.
