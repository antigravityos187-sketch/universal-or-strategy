# Phase 1: Scope Definition - EPIC-W7-053

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:30:34Z

## Epic Objective
Reduce cyclomatic complexity of InitiateStopReplacement from CYC=13 to CYC≤8 through surgical extraction of helper methods.

## Target Method
- **Method**: InitiateStopReplacement
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Line**: 307
- **Current CYC**: 13
- **Target CYC**: ≤8
- **Lines of Code**: 63

## IN SCOPE

### Primary Extraction Targets
1. **Validation Logic Extraction**
   - Parameter validation (5 parameters)
   - Order state checks (IsOrderTerminal)
   - Thread affinity validation (LogBuffer.ValidateThreadAffinity)
   - **Estimated CYC Reduction**: 2-3 points

2. **Logging Operations Extraction**
   - LogBuffer.Format calls (2 variants)
   - LogBuffer.FormatInternal calls (2 variants)
   - Consolidate logging into single helper method
   - **Estimated CYC Reduction**: 1-2 points

3. **State Management Extraction**
   - MarkStickyDirty operations (2 variants)
   - StampReaperMoveGrace operations (2 variants)
   - pendingStopReplacements tracking (2 variants)
   - **Estimated CYC Reduction**: 2-3 points

4. **Order Cancellation Logic Extraction**
   - CancelOrderForReplace gateway (2 variants)
   - CancelOrderSafe wrapper (2 variants)
   - Consolidate cancellation logic
   - **Estimated CYC Reduction**: 1-2 points

### Files to Modify
- src/V12_002.Trailing.StopUpdate.cs (primary target)

### Testing Requirements
- Unit tests for extracted helper methods
- Integration test for UpdateStopOrder to InitiateStopReplacement call chain
- Verify single caller (UpdateStopOrder) still functions correctly

## OUT OF SCOPE

### Explicitly Excluded
1. **Caller Modification**
   - UpdateStopOrder (line 84) - DO NOT modify
   - Maintain existing call signature

2. **Callee Modifications**
   - GetTargetOrdersDictionary - DO NOT modify
   - pendingStopReplacements - DO NOT modify
   - LogBuffer methods - DO NOT modify
   - CancelOrderForReplace - DO NOT modify
   - MarkStickyDirty - DO NOT modify
   - IsOrderTerminal - DO NOT modify
   - StampReaperMoveGrace - DO NOT modify
   - CancelOrderSafe - DO NOT modify

3. **Cross-File Refactoring**
   - DO NOT modify other methods in V12_002.Trailing.StopUpdate.cs
   - DO NOT modify other files in src/

4. **Behavioral Changes**
   - DO NOT alter method semantics
   - DO NOT change order of operations
   - DO NOT modify error handling logic
   - DO NOT change logging output format

5. **Performance Optimization**
   - DO NOT optimize algorithms
   - DO NOT change data structures
   - Focus ONLY on complexity reduction

## Scope Boundaries

### Clear Boundaries
- **Single Method**: Only InitiateStopReplacement
- **Single File**: Only V12_002.Trailing.StopUpdate.cs
- **Single Caller**: Only UpdateStopOrder affected
- **Zero Blast Radius**: No external importers to consider

### Success Criteria
1. InitiateStopReplacement CYC reduced from 13 to ≤8
2. All extracted methods have CYC ≤8
3. Single caller (UpdateStopOrder) continues to function
4. All 20 callees remain unchanged
5. Build passes: dotnet build
6. Hard links synced: deploy-sync.ps1
7. NinjaTrader F5 successful

## Risk Mitigation
- **Zero Blast Radius**: No external importers = minimal risk
- **Single Caller**: Easy to test and verify
- **No Hotspot**: Not in top 50 churn+complexity = stable code
- **Clear Extraction Points**: 20 callees provide natural boundaries

## Estimated Effort
- **Extraction Tickets**: 3-4 helper methods
- **CYC Reduction**: 13 to 8 (5-point reduction)
- **Testing**: 1 integration test + 3-4 unit tests
- **Total Bobcoins**: ~2.0 (Phase 2-6)

## Dependencies
- **Input**: 00-hotspots.md (Phase 0 output)
- **Output**: 00-scope.md (this document)
- **Next Phase**: Phase 1.5 (Scope Boundary Validation)

## Scope Validation
- Single method target (InitiateStopReplacement)
- Single file modification (V12_002.Trailing.StopUpdate.cs)
- Zero blast radius confirmed
- Clear extraction boundaries (4 logical groups)
- No cross-file dependencies
- No behavioral changes required
- Testable via single caller

**SCOPE APPROVED FOR PHASE 1.5 VALIDATION**
