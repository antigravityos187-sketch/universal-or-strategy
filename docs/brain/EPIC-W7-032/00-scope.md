# Phase 1: Scope Definition - EPIC-W7-032

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:06:03Z

## Epic Objective
Reduce cyclomatic complexity of `RestoreCascadedTargets` from CYC 23 to ≤8 through surgical extraction of helper methods.

## Target Method
- **Method**: RestoreCascadedTargets
- **File**: src/V12_002.Orders.Management.StopSync.cs
- **Line**: 981
- **Current CYC**: 23
- **Target CYC**: ≤8 per method
- **Lines of Code**: 118
- **Max Nesting Depth**: 6

## IN SCOPE

### Primary Extraction Target
**Method**: `RestoreCascadedTargets(SIMA_FSM fsm, string reason)`
- **Scope**: Full method body (lines 981-1099)
- **Complexity**: CYC 23 → target ≤8
- **Strategy**: Extract 3-4 helper methods to decompose complexity

### Extraction Candidates (Based on Nesting & Logic Blocks)

#### 1. Position Validation Logic
**Lines**: ~985-1005 (estimated)
**Purpose**: Validate active positions and FSM state
**Complexity Contribution**: ~3-4 branches
**Extraction Target**: `ValidatePositionForRestore(SIMA_FSM fsm)`
- Returns: bool (true if valid, false if should skip)
- Reduces nesting by early return pattern

#### 2. Target Order Dictionary Retrieval
**Lines**: ~1006-1020 (estimated)
**Purpose**: Get target orders dictionary and validate
**Complexity Contribution**: ~2-3 branches
**Extraction Target**: `GetValidTargetOrders(SIMA_FSM fsm)`
- Returns: Dictionary<string, Order> or null
- Encapsulates dictionary retrieval and null checks

#### 3. Cascaded Target Restoration Loop
**Lines**: ~1021-1080 (estimated)
**Purpose**: Iterate through target orders and restore cascaded targets
**Complexity Contribution**: ~10-12 branches (nested loops + conditionals)
**Extraction Target**: `RestoreCascadedTargetOrders(SIMA_FSM fsm, Dictionary<string, Order> targetOrders, string reason)`
- Returns: void
- Encapsulates main restoration logic
- May need further sub-extraction if CYC still >8

#### 4. Logging and Cleanup
**Lines**: ~1081-1099 (estimated)
**Purpose**: Final logging and state updates
**Complexity Contribution**: ~2-3 branches
**Extraction Target**: `LogRestorationComplete(SIMA_FSM fsm, int restoredCount)`
- Returns: void
- Encapsulates logging logic

### Dependencies to Preserve
- **activePositions** (constant) - src/V12_002.cs:199
- **SymmetryTrim** (method) - src/V12_002.Symmetry.Replace.cs:343
- **GetTargetOrdersDictionary** (method) - src/V12_002.UI.Callbacks.cs:1039
- **LogBuffer.Format** (method) - src/V12_002.Perf.LogBuffer.cs:28

### Success Criteria
1. Main method `RestoreCascadedTargets` achieves CYC ≤8
2. All extracted helper methods achieve CYC ≤8
3. Zero behavioral changes (logic preservation)
4. All existing callees remain functional
5. Build passes after extraction
6. F5 in NinjaTrader successful

## OUT OF SCOPE

### Excluded from This Epic
1. **Caller Methods**: No changes to methods that might call this (none exist per blast radius analysis)
2. **Callee Methods**: No modifications to downstream dependencies:
   - activePositions
   - SymmetryTrim
   - GetTargetOrdersDictionary
   - LogBuffer.Format
3. **Other Methods in File**: No changes to other methods in V12_002.Orders.Management.StopSync.cs
4. **Test Coverage**: Test generation deferred to Phase 5.V (verification)
5. **Performance Optimization**: Focus is complexity reduction, not performance tuning
6. **Logging Changes**: Preserve existing LogBuffer patterns (no refactoring)
7. **Error Handling**: Preserve existing error handling patterns (no new try/catch)

### Boundary Conditions
- **File Boundary**: Changes limited to src/V12_002.Orders.Management.StopSync.cs
- **Method Boundary**: Only `RestoreCascadedTargets` and its extracted helpers
- **Complexity Boundary**: Each method must achieve CYC ≤8 (Jane Street strict standard)
- **Behavioral Boundary**: Zero logic changes (pure extraction refactoring)

## Risk Mitigation

### Low Risk Factors (Favorable)
1. **Isolated Method**: 0 callers, 0 blast radius
2. **No External Dependencies**: Changes will not propagate
3. **Shallow Call Graph**: Only 2 levels deep
4. **Safe Refactoring**: Low regression risk

### Medium Risk Factors (Caution)
1. **High Churn**: 24 commits in 90 days (active development area)
2. **Deep Nesting**: 6 levels (complex control flow)
3. **Large Method**: 118 lines (potential for extraction errors)
4. **Top 10 Hotspot**: 10th highest risk in codebase

### Mitigation Strategy
1. **Surgical Extraction**: Extract one helper at a time
2. **Build Verification**: Run build after each extraction
3. **Deploy Sync**: Run deploy-sync.ps1 after each change
4. **F5 Verification**: Test in NinjaTrader after each extraction
5. **Rollback Plan**: Git checkpoint before each extraction

## Scope Validation

### Scope Boundary Checklist
- ✅ Target method identified: RestoreCascadedTargets
- ✅ Extraction candidates defined: 4 helper methods
- ✅ Complexity target set: CYC ≤8 per method
- ✅ Dependencies mapped: 12 callees identified
- ✅ Blast radius confirmed: 0 (isolated method)
- ✅ Risk assessment complete: MEDIUM-LOW
- ✅ Out-of-scope boundaries defined
- ✅ Success criteria established

### Jane Street Alignment
- **Current State**: CYC 23 (2.9x over threshold)
- **Target State**: CYC ≤8 per method
- **Extraction Strategy**: 3-4 helper methods
- **Cognitive Load**: Reduce nesting from 6 to ≤3 levels
- **Testability**: Each extracted method independently testable

## Next Phase (Phase 1.5)
Validate scope boundaries with Sequential Thinking MCP to prevent scope creep.
