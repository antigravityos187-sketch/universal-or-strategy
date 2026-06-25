# Phase 1: Scope Definition - EPIC-W7-077

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Mode**: Plan
- **Input**: 00-hotspots.md
- **Output**: 00-scope.md

## Epic Overview
- **Target Method**: ProcessClientStream
- **File**: src/V12_002.UI.IPC.Server.cs
- **Current CYC**: 8 (at Jane Street threshold)
- **Target CYC**: ≤6 (safety margin below threshold)
- **Blast Radius**: ZERO (private method, no external dependencies)

## Scope Boundaries

### What Will Be Extracted

#### Extraction Target 1: Loop Termination Logic
**Method Name**: `ShouldContinueProcessing`
**Purpose**: Consolidate loop continuation conditions
**Lines**: ~225-230 (loop condition checks)
**Rationale**: Reduces cyclomatic complexity by extracting compound boolean logic

**Current Code Pattern**:
```csharp
while (session.IsConnected && !session.CancellationToken.IsCancellationRequested)
{
    // Additional checks inside loop
}
```

**Extraction Benefit**: CYC reduction of 1-2 points

#### Extraction Target 2: Error Recovery Logic
**Method Name**: `HandleStreamError`
**Purpose**: Centralize error handling and recovery decisions
**Lines**: Error handling blocks within the loop
**Rationale**: Separates error handling from main processing flow

**Current Code Pattern**:
```csharp
try {
    // processing
} catch (Exception ex) {
    // error handling logic
    // recovery decisions
}
```

**Extraction Benefit**: CYC reduction of 1-2 points

#### Extraction Target 3: Buffer State Validation
**Method Name**: `ValidateBufferState`
**Purpose**: Check buffer health and capacity before processing
**Lines**: Buffer validation checks
**Rationale**: Isolates buffer management concerns

**Extraction Benefit**: CYC reduction of 1 point

### What Will Remain

The following will stay in ProcessClientStream:
1. **Main loop structure** - The while loop itself
2. **Helper method calls** - Existing calls to:
   - ProcessClientStream_ReadChunk
   - ProcessClientStream_DecodeUtf8
   - ProcessClientStream_ExtractLines
   - ProcessClientStream_DispatchLine
3. **Core orchestration** - High-level flow coordination
4. **Session lifecycle** - Connection state management

### Boundary Definitions

#### In-Scope
✅ Loop termination conditions
✅ Error handling blocks
✅ Buffer validation logic
✅ Conditional branches within the main loop
✅ Recovery decision logic

#### Out-of-Scope
❌ Existing helper methods (already extracted)
❌ HandleIncomingIpcLine (separate concern)
❌ Network stream operations (handled by ReadChunk)
❌ UTF-8 decoding (handled by DecodeUtf8)
❌ Line extraction (handled by ExtractLines)

## Dependencies

### Internal Dependencies
- **IpcClientSession**: Session state object (parameter)
- **NetworkStream**: Underlying stream (via session)
- **CancellationToken**: Cancellation signal (via session)
- **StringBuilder**: Character buffer (local state)

### Helper Method Dependencies
- ProcessClientStream_ReadChunk (line 257)
- ProcessClientStream_DecodeUtf8 (line 268)
- ProcessClientStream_ExtractLines (line 292)
- ProcessClientStream_DispatchLine (line 332)

### No External Dependencies
- Zero external callers (private method)
- Zero cross-file dependencies
- Zero public API surface

## Risk Assessment

### Extraction Risks: MINIMAL

#### Technical Risks
- **Risk**: Breaking existing helper method contracts
  - **Mitigation**: Preserve all existing helper method signatures
  - **Severity**: LOW

- **Risk**: Introducing new bugs in error handling
  - **Mitigation**: Comprehensive error path testing
  - **Severity**: LOW

- **Risk**: Performance regression from additional method calls
  - **Mitigation**: Methods will be small and likely inlined by JIT
  - **Severity**: NEGLIGIBLE

#### Organizational Risks
- **Risk**: None (private method, zero blast radius)
- **Risk**: None (no external API changes)

### Refactoring Safety: HIGH
- Private method scope
- Zero external dependencies
- Well-defined boundaries
- Existing helper method pattern established

## Success Criteria

### Quantitative Metrics
1. ✅ **CYC Reduction**: From 8 → ≤6
2. ✅ **Nesting Reduction**: From 3 → ≤2 (if possible)
3. ✅ **Build Success**: Zero compilation errors
4. ✅ **Test Pass**: All existing tests pass
5. ✅ **No Regressions**: F5 in NinjaTrader succeeds

### Qualitative Metrics
1. ✅ **Readability**: Main loop is easier to understand
2. ✅ **Maintainability**: Error handling is centralized
3. ✅ **Consistency**: Follows existing helper method pattern
4. ✅ **Testability**: Extracted methods are unit-testable

### Verification Steps
1. Run `python scripts/complexity_audit.py --file src/V12_002.UI.IPC.Server.cs`
2. Verify ProcessClientStream CYC ≤6
3. Run `dotnet build` (zero errors)
4. Run `powershell -File .\deploy-sync.ps1`
5. F5 in NinjaTrader IDE
6. Verify BUILD_TAG in output

## Extraction Strategy

### Phase 2 Architecture Plan Will Define:
1. Exact method signatures for extracted methods
2. Parameter passing strategy
3. Return value contracts
4. Error propagation approach
5. Unit test specifications

### Phase 5 Implementation Will Execute:
1. Extract ShouldContinueProcessing method
2. Extract HandleStreamError method
3. Extract ValidateBufferState method
4. Update ProcessClientStream to call new methods
5. Add unit tests for extracted methods
6. Verify CYC ≤6 achieved

## Complexity Reduction Projection

### Current State
- **CYC**: 8
- **Branches**: Multiple conditional paths
- **Nesting**: 3 levels

### Target State
- **CYC**: ≤6 (2-point reduction)
- **Branches**: Simplified via extraction
- **Nesting**: ≤2 levels

### Extraction Impact
| Extraction | CYC Reduction | Confidence |
|------------|---------------|------------|
| ShouldContinueProcessing | 1-2 points | HIGH |
| HandleStreamError | 1-2 points | HIGH |
| ValidateBufferState | 1 point | MEDIUM |
| **Total** | **3-5 points** | **HIGH** |

**Projected Final CYC**: 3-5 (well below threshold of 8)

## Conclusion

**SCOPE APPROVED**: This refactoring has:
- ✅ Clear extraction targets (3 methods)
- ✅ Well-defined boundaries
- ✅ Minimal risk (zero blast radius)
- ✅ High confidence in CYC reduction (8 → ≤6)
- ✅ Established pattern (follows existing helper methods)

**Next Phase**: Proceed to Phase 1.5 (Scope Boundary Validation) to verify no scope creep.
