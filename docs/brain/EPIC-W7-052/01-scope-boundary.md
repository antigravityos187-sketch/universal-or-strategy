# Phase 1: Scope Boundary - EPIC-W7-052

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.37
- **API Key**: N/A (Plan mode)
- **Execution Time**: 2026-06-24T01:31:42Z

## Epic Metadata
- **Epic ID**: EPIC-W7-052
- **Target Method**: CleanupStalePendingReplacements
- **File**: src/V12_002.Trailing.StopUpdate.cs
- **Current CYC**: 11 (Target: <=8)
- **Lines**: 37-80 (44 lines)

## Method Structure Analysis

### Decision Points (11 total)
1. foreach loop (line 42) - Iterate over pendingStopReplacements
2. Timeout check (line 44) - Time delta > 5 seconds
3. TryRemove success (line 46) - Dictionary removal
4. Position exists (line 53) - activePositions lookup
5. Entry filled (line 54) - pos.EntryFilled check
6. Remaining contracts (line 55) - pos.RemainingContracts > 0
7. Bracket restoration needed (line 68) - pending.BracketRestorationNeeded
8. Captured targets exist (line 68) - pending.CapturedTargets != null

### Logical Phases
1. **Stale Detection Phase** (lines 39-44)
   - Get current time
   - Iterate over pending replacements
   - Check timeout threshold (5 seconds)

2. **Removal Phase** (lines 46-48)
   - Remove from dictionary
   - Decrement counter
   - Log removal

3. **Recovery Validation Phase** (lines 51-56)
   - Check position exists
   - Validate entry filled
   - Validate remaining contracts

4. **Emergency Stop Creation Phase** (lines 58-66)
   - Calculate replacement quantity
   - Create new stop order with recovery flag

5. **Bracket Restoration Phase** (lines 68-73)
   - Check restoration needed
   - Capture target snapshot
   - Trigger restoration event

## Scope Definition

### IN SCOPE

#### Primary Extraction Target
**Extract Emergency Recovery Logic** (lines 51-73)
- **New Method**: ProcessStaleReplacementRecovery
- **Parameters**: string positionKey, PendingStopReplacement pending
- **Return**: void
- **CYC Estimate**: 3
- **Rationale**: Encapsulates recovery logic, reduces nesting depth

#### Secondary Extraction Target
**Extract Bracket Restoration Logic** (lines 68-73)
- **New Method**: TriggerBracketRestoration
- **Parameters**: string positionKey, PendingStopReplacement pending
- **Return**: void
- **CYC Estimate**: 2
- **Rationale**: Isolates async event triggering

### OUT OF SCOPE

#### Main Loop Structure (lines 39-44)
- **Reason**: Core orchestration logic, must remain in main method
- **CYC Contribution**: 2 (foreach + timeout check)

#### Timeout Threshold Constant (5 seconds)
- **Reason**: Magic number, but changing requires broader analysis

#### Dictionary Operations (TryRemove, TryGetValue)
- **Reason**: Atomic operations, already optimal

#### Logging Statements
- **Reason**: Contextual logging, belongs with orchestration

#### Interlocked.Decrement
- **Reason**: Thread-safe counter update, single line

## Expected Outcomes

### Complexity Reduction
- **Before**: CYC 11 (37.5% over threshold)
- **After**: 
  - Main method: CYC 3 (62.5% under threshold)
  - ProcessStaleReplacementRecovery: CYC 3
  - TriggerBracketRestoration: CYC 2
- **Total Reduction**: 8 points (72.7% reduction)

### Nesting Depth Reduction
- **Before**: Max depth 7
- **After**: Max depth 3 (main method)
- **Improvement**: 57% reduction

## Risk Assessment

### Refactoring Risk: LOW
- **Blast Radius**: 0 callers (isolated method)
- **Behavioral Changes**: None (pure extraction)
- **Test Coverage**: Existing integration tests sufficient

### Implementation Risk: LOW
- **Complexity**: Straightforward extraction
- **Dependencies**: All preserved in extracted methods
- **Rollback**: Simple revert if issues arise

## Success Criteria

### Functional Requirements
- All 26 callees preserved
- Zero behavioral changes
- Stale replacement cleanup logic unchanged
- Emergency stop creation logic unchanged
- Bracket restoration logic unchanged

### Non-Functional Requirements
- Main method CYC <=8 (target: 3)
- All extracted methods CYC <=8
- Max nesting depth <=4
- Build passes
- F5 in NinjaTrader successful

### Quality Gates
- Pre-push validation passes
- No new Codacy violations
- ASCII-only compliance maintained
- Hard link sync successful

## Next Steps (Phase 2)

1. Generate architecture plan with detailed extraction steps
2. Create Mermaid diagrams showing before/after call flow
3. Define ticket breakdown (2 extractions + 1 main method update)
4. Validate against Jane Street patterns (FSM/Actor compliance)
5. Prepare for Phase 3 DNA audit
