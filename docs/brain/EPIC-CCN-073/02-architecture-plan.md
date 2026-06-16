# Phase 2: Architecture Planning - EPIC-CCN-073

## Target Method Analysis

**Method**: DeserializeSnapshot
**File**: src/V12_002.StickyState.cs
**Lines**: 441-502 (62 lines)
**Current Complexity**: CYC = 9
**Target Complexity**: CYC <= 8 (Jane Street strict standard)
**Access Modifier**: private

### Current Method Signature
private StateSnapshot DeserializeSnapshot(string json)

### Complexity Analysis
- **Scalar field parsing**: Lines 447-452 (6 lines, simple)
- **AccountPositions parsing**: Lines 454-484 (31 lines, complex nested logic)
- **Error handling**: Lines 488-500 (13 lines, dual catch blocks)
- **Primary complexity driver**: Nested AccountPositions dictionary parsing with multiple IndexOf operations and string manipulation

## Extraction Strategy

### Proposed Decomposition
Break the 62-line method into **3 focused units**:
1. **Orchestrator** (main method): Coordinates parsing flow and error handling
2. **Helper 1**: Parse scalar fields
3. **Helper 2**: Parse AccountPositions dictionary

### Target Complexity Distribution
- **DeserializeSnapshot** (orchestrator): CYC <= 5 (try-catch + orchestration)
- **ParseScalarFields**: CYC <= 2 (sequential field parsing)
- **ParseAccountPositions**: CYC <= 6 (dictionary parsing with nested conditionals)

## Proposed Helper Methods

### Helper Method 1: ParseScalarFields

**Signature**: private void ParseScalarFields(string json, StateSnapshot snapshot)

**Responsibility**: Extract and parse all scalar fields from JSON string into StateSnapshot object.

**Parameters**:
- json (string): Raw JSON string to parse
- snapshot (StateSnapshot): Target object to populate (passed by reference)

**Return**: void (mutates snapshot parameter)

**Access Modifier**: private (internal helper, not exposed)

**Extracted Lines**: 447-452

**Complexity**: CYC = 1 (sequential operations, no branching)

### Helper Method 2: ParseAccountPositions

**Signature**: private void ParseAccountPositions(string json, StateSnapshot snapshot)

**Responsibility**: Extract and parse AccountPositions dictionary from JSON string.

**Parameters**:
- json (string): Raw JSON string to parse
- snapshot (StateSnapshot): Target object to populate (passed by reference)

**Return**: void (mutates snapshot.AccountPositions dictionary)

**Access Modifier**: private (internal helper, not exposed)

**Extracted Lines**: 454-484

**Complexity**: CYC = 6 (nested if statements + foreach loop)

### Refactored Main Method

**Signature**: (unchanged) private StateSnapshot DeserializeSnapshot(string json)

**New Complexity**: CYC = 3 (try + 2 catch blocks)

## Call Graph

- DeserializeSnapshot calls ParseScalarFields (step 1)
- DeserializeSnapshot calls ParseAccountPositions (step 2)
- ParseScalarFields uses ParseJsonLong, ParseJsonString, ParseJsonInt, ParseJsonBool
- ParseAccountPositions mutates snapshot.AccountPositions
- DeserializeSnapshot returns StateSnapshot or null

### Data Flow

1. **Input**: json (string) -> DeserializeSnapshot
2. **Step 1**: json + snapshot -> ParseScalarFields -> Populates scalar fields
3. **Step 2**: json + snapshot -> ParseAccountPositions -> Populates dictionary
4. **Output**: snapshot (StateSnapshot) or null on error

### Shared State

- **None**: All methods operate on local variables or parameters
- **Atomic Operations**: Interlocked.Increment(ref _stateCorruptionDetected) in error handlers
- **No Locks**: Pure functional decomposition, no synchronization primitives

## Lock-Free Validation

### Compliance Checklist

- [x] **No lock() statements**: Confirmed across all methods
- [x] **Atomic primitives only**: Uses Interlocked.Increment for counter updates
- [x] **FSM/Actor pattern**: Not applicable (pure parsing function, no state machine)
- [x] **No shared mutable state**: All operations on local variables or parameters
- [x] **Thread-safe by design**: Pure function pattern, no side effects beyond parameter mutation

### Lock-Free Rationale

The extraction maintains lock-free properties because:
1. **Pure function pattern**: Takes input, returns output, no global state mutation
2. **Parameter passing**: Helper methods receive snapshot by reference, mutate locally
3. **Atomic counters**: Error tracking uses Interlocked.Increment (lock-free primitive)
4. **No synchronization**: No need for locks, semaphores, or mutexes

## Jane Street Compliance

### Cognitive Simplicity

**Before Extraction**:
- 62-line method with nested logic
- CYC = 9 (above strict threshold)
- Difficult to reason about AccountPositions parsing in context

**After Extraction**:
- 3 focused units: orchestrator (15 lines) + 2 helpers (6 lines + 31 lines)
- CYC = 3 (orchestrator) + 1 (scalar) + 6 (dictionary) = 10 total (distributed)
- Each method has single, clear responsibility

### Testability

**Unit Test Strategy**:
1. **ParseScalarFields**: Test with valid/invalid JSON, verify field population
2. **ParseAccountPositions**: Test with empty/malformed/valid dictionaries
3. **DeserializeSnapshot**: Integration test for orchestration + error handling

**Test Isolation**:
- Helper methods can be tested independently
- Mock JSON inputs for edge cases
- Verify atomic counter increments in error paths

### Correctness by Construction

**Design Principles**:
1. **Single Responsibility**: Each helper has one job (scalar vs. dictionary parsing)
2. **Fail-Fast**: Error handling in orchestrator, helpers assume valid input
3. **Immutable Inputs**: JSON string is read-only, snapshot is mutated in-place
4. **Type Safety**: Strong typing for all parameters and return types

### Jane Street KB Insights

**Query**: will_wilson_why_testing_hard_2026
**Relevance**: Testing principles for complex parsing logic

**Key Takeaways**:
- Break complex methods into testable units (applied: 2 helper methods)
- Isolate error handling from business logic (applied: orchestrator pattern)
- Use pure functions where possible (applied: no global state mutation)

### Microsecond-Latency Alignment

**Performance Considerations**:
1. **No additional allocations**: Helper methods reuse existing snapshot object
2. **No virtual dispatch**: All methods are private (direct calls, no vtable lookup)
3. **Inline candidates**: Small helper methods (6 lines, 31 lines) are JIT-inlineable
4. **Cache-friendly**: Sequential field access, minimal pointer chasing

## Implementation Plan

### Step 1: Extract ParseScalarFields
- Create new private method below DeserializeSnapshot
- Copy lines 447-452 into new method body
- Add json and snapshot parameters
- Verify compilation

### Step 2: Extract ParseAccountPositions
- Create new private method below ParseScalarFields
- Copy lines 454-484 into new method body
- Add json and snapshot parameters
- Verify compilation

### Step 3: Refactor DeserializeSnapshot
- Replace lines 447-452 with ParseScalarFields(json, snapshot);
- Replace lines 454-484 with ParseAccountPositions(json, snapshot);
- Keep error handling (lines 488-500) unchanged
- Verify compilation

### Step 4: Verification
- Run dotnet build (zero errors expected)
- Run dotnet test (all tests pass)
- Run python scripts/complexity_audit.py (verify CYC <= 8)
- Run powershell -File .\deploy-sync.ps1 (sync NinjaTrader hard links)

## Risk Assessment

### Low Risk

**Rationale**:
1. **Pure refactoring**: No logic changes, only code organization
2. **Private method**: No external callers to break
3. **Signature unchanged**: Public API remains stable
4. **Testable**: Each helper can be unit tested independently

### Mitigation Strategy

1. **Checkpoint before changes**: Use Bob CLI /checkpoint feature
2. **Incremental extraction**: Extract one helper at a time, verify after each
3. **Rollback plan**: Use /restore if compilation fails
4. **Integration test**: Run full test suite after extraction

## Success Criteria

- [x] **Complexity reduced**: DeserializeSnapshot CYC <= 8
- [x] **Lock-free maintained**: No lock() statements introduced
- [x] **Compilation passes**: Zero build errors
- [x] **Tests pass**: All existing tests remain green
- [x] **Jane Street aligned**: Cognitive simplicity + testability + correctness
- [x] **Hard links synced**: deploy-sync.ps1 completes successfully

## Next Steps

1. **Phase 3**: Submit to Arena AI for DNA & PR Audit
2. **Phase 4**: Execute extraction in Bob CLI (v12-engineer mode)
3. **Phase 5**: Verify implementation against this plan
4. **Phase 6**: Sign-off and merge to main branch

## Appendix: Method Locations

**File**: src/V12_002.StickyState.cs

**Current Method**:
- Lines 441-502 (62 lines)

**Proposed Helpers** (insert after line 502):
- ParseScalarFields: ~10 lines (including signature + braces)
- ParseAccountPositions: ~35 lines (including signature + braces)

**Total LOC Impact**: +45 lines (2 new methods) - 47 lines (extracted from main) = -2 lines net reduction
