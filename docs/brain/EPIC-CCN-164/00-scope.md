# Phase 1: Scope Definition - EPIC-CCN-164

## Epic Metadata
- **Epic ID**: EPIC-CCN-164
- **Target Method**: `IsCommandForThisInstrument`
- **File**: `src/V12_002.UI.IPC.cs`
- **Current Complexity**: 36
- **Target Complexity**: ≤ 8
- **Reduction Goal**: 78%

## Method Analysis

### Current Structure
The `IsCommandForThisInstrument` method (lines ~300-350) performs two primary responsibilities:
1. **Global Command Detection**: Checks if action is a global command (18 OR conditions)
2. **Symbol Matching Logic**: Validates if command targets this instrument (13 OR conditions)

### Complexity Breakdown
- **Global Command Check**: ~18 branches (isGlobalCommand boolean)
- **Symbol Matching Logic**: ~13 branches (isForMe boolean)
- **Orchestration Logic**: ~5 branches (logging, return)
- **Total CYC**: 36

## Scope Boundary (V12.23 Protocol)

### IN SCOPE
✅ **Single Method Extraction**: `IsCommandForThisInstrument` ONLY
✅ **Two Sub-Methods**:
   1. `IsGlobalIpcCommand(string action)` - Extract global command detection
   2. `IsSymbolMatch(string targetSymbol)` - Extract symbol matching logic
✅ **Preserve Behavior**: Zero logic drift, pure structural movement
✅ **Maintain Logging**: Keep existing Print statement with same format

### OUT OF SCOPE
❌ **NO** changes to caller (`ProcessIpcCommands`)
❌ **NO** changes to other IPC methods
❌ **NO** logic improvements or optimizations
❌ **NO** refactoring of adjacent code
❌ **NO** changes to validation logic
❌ **NO** changes to command routing

## Extraction Strategy

### Sub-Method 1: IsGlobalIpcCommand
**Purpose**: Isolate global command detection logic
**Signature**: `private bool IsGlobalIpcCommand(string action)`
**Logic**: Extract the 18-condition OR chain for global commands
**Expected CYC**: 2-3 (simple boolean return)

### Sub-Method 2: IsSymbolMatch
**Purpose**: Isolate symbol matching logic
**Signature**: `private bool IsSymbolMatch(string targetSymbol)`
**Logic**: Extract the 13-condition OR chain for symbol matching
**Expected CYC**: 3-4 (includes string operations)

### Refactored Main Method
**Purpose**: Orchestrate global check + symbol check + logging
**Expected CYC**: 3-4 (if-else + logging + return)

## Complexity Targets

| Method | Current CYC | Target CYC | Status |
|--------|-------------|------------|--------|
| `IsCommandForThisInstrument` | 36 | ≤ 8 | 🔴 Needs extraction |
| `IsGlobalIpcCommand` (new) | N/A | ≤ 3 | ✅ Will meet target |
| `IsSymbolMatch` (new) | N/A | ≤ 4 | ✅ Will meet target |

**Post-Extraction Total**: 3 methods, all ≤ 8 CYC

## Risk Assessment

### Low Risk Factors
✅ Pure boolean logic (no state mutations)
✅ No external dependencies
✅ Clear input/output contract
✅ Existing logging for verification
✅ Single caller (ProcessIpcCommands)

### Mitigation Strategy
- Preserve exact boolean logic (no optimization)
- Keep Print statement format identical
- Maintain parameter passing (action, targetSymbol)
- No changes to caller invocation

## V12 DNA Compliance

### Lock-Free ✅
- No locks in method or extractions
- Pure computation, no state mutations

### ASCII-Only ✅
- All string literals are ASCII
- No Unicode in logging

### Correctness by Construction ✅
- Boolean logic remains deterministic
- No illegal states possible

### Jane Street Alignment ✅
- Cognitive simplicity: 3 methods ≤ 8 CYC each
- Single responsibility per method
- Exhaustive testing feasible (boolean combinations)

## Success Criteria

### Phase 1 (Scope Definition) ✅
- [x] Method analyzed
- [x] Extraction strategy defined
- [x] Scope boundary documented
- [x] Risk assessment complete

### Phase 2 (Architecture Planning)
- [ ] Detailed extraction plan with line numbers
- [ ] Call graph analysis
- [ ] Test strategy defined

### Phase 5 (Execution)
- [ ] Extract `IsGlobalIpcCommand` method
- [ ] Extract `IsSymbolMatch` method
- [ ] Refactor main method to call sub-methods
- [ ] Verify CYC ≤ 8 for all methods
- [ ] Run `complexity_audit.py`
- [ ] Run `deploy-sync.ps1`
- [ ] Verify F5 in NinjaTrader

## Blast Radius

### Direct Impact
- **File**: `src/V12_002.UI.IPC.cs` (this file only)
- **Method**: `IsCommandForThisInstrument` (lines ~300-350)
- **Callers**: 1 (`ProcessIpcCommands` in same file)

### Zero Impact
- No changes to IPC command queue
- No changes to validation logic
- No changes to command routing
- No changes to other partial classes

## Next Steps

1. **Phase 2**: Create detailed architecture plan with:
   - Exact line numbers for extraction
   - Method signatures with XML docs
   - Before/after code comparison
   - Call graph diagram

2. **Phase 3**: DNA & PR audit (automated)

3. **Phase 4**: Generate surgical tickets

4. **Phase 5**: Execute extraction with Bob CLI

## Status
✅ **Phase 1 Complete** - Ready for Phase 2 (Architecture Planning)

---

**Scope Validation**: This epic targets ONLY `IsCommandForThisInstrument` method. No scope creep. No adjacent improvements. Pure structural extraction to achieve CYC ≤ 8.