# EPIC-CCN-14: ProcessIpcCommands Intake Document

## Mission Brief
Reduce `ProcessIpcCommands` method complexity from CYC 76 to ≤8 through surgical extraction of validation, symbol matching, and error handling logic.

## Target Specification
- **Method**: `ProcessIpcCommands`
- **File**: `src/V12_002.UI.IPC.cs`
- **Lines**: 260-430 (171 lines)
- **Current CYC**: 76
- **Target CYC**: ≤8
- **Max Nesting**: 6 → ≤3
- **Hotspot Rank**: #2 in codebase

## V12 DNA Compliance Check

### ✅ Lock-Free Pattern
- **Status**: COMPLIANT
- **Evidence**: Uses `Enqueue(ctx => ctx.ProcessIpcCommandCore(...))` pattern
- **No Action Required**

### ✅ ASCII-Only
- **Status**: COMPLIANT
- **Evidence**: All string literals use straight quotes, no Unicode
- **No Action Required**

### ⚠️ Complexity
- **Status**: VIOLATION
- **Current**: CYC 76 (5.1x over threshold)
- **Target**: CYC ≤8
- **Action Required**: Extract 3 helper methods

### ✅ Correctness by Construction
- **Status**: COMPLIANT
- **Evidence**: Validation guards prevent invalid states
- **Maintain**: Preserve all validation logic during extraction

## Extraction Plan Overview

### Extraction 1: Command Format Validation
**Target Method**: `ValidateCommandFormat`
- **Lines**: 281-304
- **Responsibility**: Parse and validate command structure
- **Estimated CYC**: 6
- **Returns**: `bool` (success/failure)
- **Out Parameters**: `string[] parts`, `long senderTicks`

### Extraction 2: Symbol Matching Logic
**Target Method**: `IsCommandForThisInstrument`
- **Lines**: 335-391
- **Responsibility**: Determine if command applies to this chart
- **Estimated CYC**: 8
- **Returns**: `bool` (is for me)
- **Out Parameters**: `bool isGlobalCommand`

### Extraction 3: Validation Failure Handler
**Target Method**: `HandleValidationFailure`
- **Lines**: 307-327
- **Responsibility**: Process validation failures with appropriate logging
- **Estimated CYC**: 5
- **Returns**: `void`
- **Parameters**: `ValidationResult result`, `string action`

## Caller Impact Analysis

### Direct Callers
- **OnBarUpdate**: Calls via `TriggerCustomEvent` (line 426)
- **Self-recursive**: Line 426 triggers continuation if queue non-empty

### Indirect Callers
- IPC TCP server thread enqueues commands
- External Python/PowerShell scripts send commands via TCP

### Impact Assessment
- **Risk**: LOW
- **Reason**: All extractions are internal helpers, no public API changes
- **Verification**: F5 test after each extraction

## Jane Street Alignment

### Cognitive Simplicity
- **Current**: 171-line method with 76 decision points
- **Target**: 4 methods, each ≤30 lines, CYC ≤8
- **Benefit**: Each method fits in working memory

### Testability
- **Current**: 2^76 possible paths (untestable)
- **Target**: 2^8 paths per method (exhaustively testable)
- **Benefit**: Can write unit tests for each helper

### Microsecond Latency
- **Current**: Complex branching may cause branch mispredictions
- **Target**: Simpler methods improve CPU pipeline efficiency
- **Benefit**: Reduced latency variance

## Risk Mitigation

### Risk 1: Logic Drift
- **Mitigation**: Zero logic changes, pure structural movement
- **Verification**: Line-by-line comparison before/after

### Risk 2: State Capture
- **Mitigation**: All extractions use parameters/returns, no closure capture
- **Verification**: No lambda/delegate captures in extracted methods

### Risk 3: Performance Regression
- **Mitigation**: Inline candidates for JIT optimization
- **Verification**: Benchmark before/after (if needed)

### Risk 4: F5 Failure
- **Mitigation**: STOP after each ticket for F5 verification
- **Verification**: Director confirms "F5 done [BUILD_TAG]"

## Success Criteria
1. ✅ `ProcessIpcCommands` CYC ≤8
2. ✅ All extracted methods CYC ≤8
3. ✅ Zero locks (maintain compliance)
4. ✅ ASCII-only (maintain compliance)
5. ✅ Zero logic drift
6. ✅ F5 verification passes after each ticket
7. ✅ Build + tests pass
8. ✅ `deploy-sync.ps1` ASCII gate passes

## Dependencies
- **Upstream**: None (self-contained method)
- **Downstream**: `ProcessIpcCommandCore` (unchanged)
- **Shared State**: Instance fields (read-only access)

## Estimated Effort
- **Extraction 1**: 15 minutes (straightforward validation logic)
- **Extraction 2**: 20 minutes (complex boolean logic, needs careful testing)
- **Extraction 3**: 10 minutes (simple switch statement)
- **F5 Verification**: 5 minutes per ticket
- **Total**: ~60 minutes

## Next Phase
Proceed to Phase 2: Implementation Plan