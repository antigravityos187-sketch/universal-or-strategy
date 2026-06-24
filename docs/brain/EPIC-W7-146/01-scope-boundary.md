# Phase 1: Scope Boundary - EPIC-W7-146

## Epic Metadata
- **Epic ID**: EPIC-W7-146
- **Target Method**: CancelOrphanedTargets
- **File**: src/V12_002.UI.Compliance.cs
- **Line**: 553
- **Current CYC**: 13
- **Target CYC**: ≤8 (Jane Street threshold)

## Scope Definition

### IN SCOPE

#### Primary Target
- **Method**: CancelOrphanedTargets(SIMA_FSM fsm)
  - Location: src/V12_002.UI.Compliance.cs:553
  - Current metrics: CYC 13, nesting 4, 26 lines
  - Refactoring goal: Extract nested logic to reduce CYC to ≤8

#### Extraction Candidates
Based on the 4-level nesting depth and CYC 13, the following logic blocks are candidates for extraction:

1. **Orphaned Target Detection Logic**
   - Nested conditionals checking order states
   - Terminal state validation
   - FSM state checks

2. **Order Cancellation Logic**
   - CancelOrderOnAccount invocations
   - Error handling for cancellation failures
   - Logging for cancelled orders

3. **State Validation Logic**
   - IsOrderTerminal checks
   - FSM state comparisons
   - Null/validity checks

#### Success Criteria
- All extracted methods have CYC ≤8
- Max nesting depth ≤2 per method
- Original method CYC reduced from 13 to ≤8
- All 3 callers continue to work without modification:
  - HandleFleetStopFill (line 519)
  - ProcessQueuedExecution_HandleFleetOCO (line 698)
  - ProcessQueuedExecution (line 787)

### OUT OF SCOPE

#### Callers (Do NOT Modify)
- **HandleFleetStopFill** (line 519) - Caller in same file
- **ProcessQueuedExecution_HandleFleetOCO** (line 698) - Caller in same file
- **ProcessQueuedExecution** (line 787) - Caller in same file

**Rationale**: All callers are in the same file and should continue to work without changes. The refactoring is purely internal to CancelOrphanedTargets.

#### Callees (Do NOT Modify)
- **CancelOrderOnAccount** (src/V12_002.Orders.CancelGateway.cs:46)
- **IsOrderTerminal** (src/V12_002.Orders.Management.Flatten.cs:574, 698)

**Rationale**: These are utility methods used by CancelOrphanedTargets. They are stable and should not be modified as part of this epic.

#### Other Methods in File
- All other methods in V12_002.UI.Compliance.cs are OUT OF SCOPE
- Only CancelOrphanedTargets and its extracted helpers are IN SCOPE

#### Cross-File Changes
- No changes to other files in src/
- No changes to test files (tests will be added separately)
- No changes to documentation outside docs/brain/EPIC-W7-146/

### Boundary Validation

#### Risk Assessment
- **Blast Radius**: ZERO (no external dependencies)
- **Isolation**: EXCELLENT (all callers in same file)
- **Churn**: LOW (not in top 50 hotspots)
- **Refactoring Safety**: HIGH

#### Constraints
1. **Signature Preservation**: CancelOrphanedTargets(SIMA_FSM fsm) signature must remain unchanged
2. **Behavior Preservation**: All existing functionality must be preserved
3. **No Breaking Changes**: All 3 callers must continue to work without modification
4. **Single File**: All changes confined to V12_002.UI.Compliance.cs

#### Exclusions
- Do NOT modify caller methods
- Do NOT modify callee methods (CancelOrderOnAccount, IsOrderTerminal)
- Do NOT change method signature
- Do NOT add new dependencies
- Do NOT modify other methods in the file

## Extraction Strategy

### Approach
1. **Identify extraction points** - Analyze nested conditionals and loops
2. **Extract helper methods** - Create private methods for extracted logic
3. **Reduce nesting** - Flatten control flow using early returns
4. **Validate CYC** - Ensure all methods ≤8 after extraction

### Expected Outcome
- **Before**: 1 method with CYC 13, nesting 4
- **After**: 1 main method + 2-3 helper methods, all with CYC ≤8, nesting ≤2

### Verification
- Build passes: dotnet build
- Deploy sync: powershell -File .\deploy-sync.ps1
- F5 in NinjaTrader: Verify BUILD_TAG appears
- Complexity audit: python scripts/complexity_audit.py --threshold 8

## Phase 1 Completion

**Status**: SCOPE DEFINED

**Next Phase**: Phase 2 (Architecture Planning)
- Design extraction plan
- Identify helper method signatures
- Plan control flow refactoring
- Create Mermaid diagrams

**Approval**: Ready for Phase 2
