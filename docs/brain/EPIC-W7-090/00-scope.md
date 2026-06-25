# Phase 1: Scope Definition - EPIC-W7-090

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:36:10Z

## Epic Summary
**Target**: OnWatchdogTimer method in src/V12_002.Safety.Watchdog.cs
**Current Complexity**: CYC=11 (Target: ≤8)
**Risk Level**: LOW (zero external callers, timer callback pattern)

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
- **Method**: `OnWatchdogTimer(object sender, ElapsedEventArgs e)`
- **File**: `src/V12_002.Safety.Watchdog.cs`
- **Lines**: 36-90 (54 LOC)
- **Complexity**: CYC=11, Nesting=5

#### Extraction Candidates
Based on call hierarchy analysis, the following logic blocks are candidates for extraction:

1. **Watchdog Lead Account Check** (Lines ~40-45)
   - Logic: `HasWatchdogLeadAccountWorkingOrder()` conditional
   - Complexity contribution: +2
   - Extraction target: `CheckWatchdogLeadAccountStatus()`

2. **Direct Fallback Execution** (Lines ~50-70)
   - Logic: `ExecuteWatchdogDirectFallback()` invocation and error handling
   - Complexity contribution: +4
   - Extraction target: `ExecuteWatchdogFallbackSafely()`

3. **Actor Queue Drain** (Lines ~75-85)
   - Logic: `Enqueue()` and drain scheduling
   - Complexity contribution: +3
   - Extraction target: `EnqueueWatchdogDrain()`

#### Files to Modify
- `src/V12_002.Safety.Watchdog.cs` (primary target)

#### Test Coverage Required
- Unit tests for extracted methods
- Integration test for timer callback flow
- Edge case: concurrent timer invocations

### OUT OF SCOPE

#### Excluded Methods (No Changes)
- `HasWatchdogLeadAccountWorkingOrder()` - Already extracted, CYC acceptable
- `ExecuteWatchdogDirectFallback()` - Separate concern, will be addressed in future epic
- `Enqueue()` - Core FSM/Actor pattern, do not modify
- All 22 callee methods - No modifications to downstream dependencies

#### Excluded Files
- All other partial class files (V12_002.*.cs)
- Test files (no modifications, only additions)
- Configuration files

#### Architectural Constraints
- **NO changes to FSM/Actor pattern** - Timer callback must continue using `Enqueue()`
- **NO changes to watchdog timing** - Timer interval remains unchanged
- **NO changes to direct fallback logic** - Only wrap in error handling, do not modify behavior
- **NO changes to method signatures** - All extracted methods are private helpers

### Scope Validation

#### Complexity Reduction Target
- **Before**: CYC=11
- **After**: CYC≤8 (target: CYC=5-6 for main method)
- **Extracted Methods**: Each must have CYC≤8

#### Blast Radius Confirmation
- **External Callers**: 0 (timer callback, framework-invoked)
- **Risk**: MINIMAL - No external dependencies to update
- **Rollback**: Simple - revert single file

#### Jane Street Alignment
- ✅ Cognitive simplicity - Break down nested conditionals
- ✅ Testability - Extract testable units
- ✅ Lock-free pattern - Preserve `Enqueue()` usage
- ✅ Error handling - Wrap fallback execution safely

## Success Criteria

### Phase 1 Complete When:
- [x] Scope boundaries clearly defined (IN SCOPE vs OUT OF SCOPE)
- [x] Extraction candidates identified with complexity contributions
- [x] Architectural constraints documented
- [x] Blast radius confirmed (zero external callers)
- [x] Jane Street alignment verified

### Phase 2 Prerequisites:
- Scope approved by Phase 1.5 boundary validation
- No scope creep beyond defined boundaries
- Complexity reduction target achievable (CYC 11→8)

## Notes
- Timer callback pattern means zero blast radius
- All extractions are private helper methods
- No changes to public API or FSM/Actor pattern
- Low risk, high confidence refactoring
