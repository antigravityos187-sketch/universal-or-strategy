# Phase 1: Scope Definition - EPIC-W7-146

## Agent Tracking
- Agent Name: v12-phase1-scope
- Bobcoins Used: 0.00
- API Key: jCodemunch MCP
- Execution Time: ~5 seconds
- Input: docs/brain/EPIC-W7-146/00-hotspots.md

## Target Method
- Method: CancelOrphanedTargets
- File: src/V12_002.UI.Compliance.cs
- Line: 553
- Current CYC: 13
- Target CYC: 8 or less (Jane Street threshold)

## Scope Boundaries

### IN SCOPE

#### Primary Target
- CancelOrphanedTargets method (lines 553-578)
  - Current CYC: 13
  - Current nesting: 4 levels
  - Lines: 26
  - Extraction goal: Reduce to CYC 8 or less, nesting 2 or less

#### Extraction Strategy
1. Extract conditional logic for orphaned target detection
2. Extract cancellation logic into helper method
3. Simplify nested conditionals using early returns
4. Maintain single responsibility per extracted method

#### Files to Modify
- src/V12_002.UI.Compliance.cs (primary target)

#### Callers (Must remain functional)
- HandleFleetStopFill (line 519)
- ProcessQueuedExecution_HandleFleetOCO (line 698)
- ProcessQueuedExecution (line 787)

#### Callees (Dependencies)
- CancelOrderOnAccount (V12_002.Orders.CancelGateway.cs)
- IsOrderTerminal (V12_002.Orders.Management.Flatten.cs)

### OUT OF SCOPE

#### Excluded from Refactoring
- Caller methods (HandleFleetStopFill, ProcessQueuedExecution_HandleFleetOCO, ProcessQueuedExecution)
  - Rationale: Not part of this epic complexity target
  - Action: Leave unchanged, verify they still work

- Callee methods (CancelOrderOnAccount, IsOrderTerminal)
  - Rationale: External dependencies, separate concerns
  - Action: Use as-is, do not modify

- Other methods in V12_002.UI.Compliance.cs
  - Rationale: Outside this epic scope
  - Action: Do not touch

- Test files
  - Rationale: Tests will be added in Phase 5
  - Action: Defer to implementation phase

- Documentation updates
  - Rationale: Will be updated after successful refactoring
  - Action: Defer to Phase 6

#### Explicitly Excluded Files
- All files except src/V12_002.UI.Compliance.cs
- Build scripts (deploy-sync.ps1, build_readiness.ps1)
- Configuration files (.codacy.yml, .bob/*, etc.)

## Extraction Plan

### Step 1: Extract Orphaned Target Detection
New Method: IsTargetOrphaned(Order targetOrder, SIMA_FSM fsm)
- Purpose: Determine if a target order is orphaned
- Logic: Check if target order exists but FSM is null/terminated
- Expected CYC: 3 or less

### Step 2: Extract Cancellation Logic
New Method: CancelOrphanedTarget(Order targetOrder, string reason)
- Purpose: Cancel a single orphaned target order
- Logic: Call CancelOrderOnAccount with appropriate reason
- Expected CYC: 2 or less

### Step 3: Simplify Main Method
Refactored: CancelOrphanedTargets(List<Order> orders)
- Purpose: Iterate and delegate to helper methods
- Logic: Use early returns, call extracted methods
- Expected CYC: 5 or less

## Success Criteria

### Complexity Targets
- CancelOrphanedTargets: CYC 8 or less (currently 13)
- All extracted methods: CYC 8 or less
- Max nesting depth: 2 or less (currently 4)

### Functional Requirements
- All 3 callers continue to work without modification
- Order cancellation behavior unchanged
- No new dependencies introduced
- Build passes after refactoring

### Quality Gates
- ASCII-only compliance maintained
- No lock() statements introduced
- Follows V12 DNA principles (Correctness by Construction)
- Jane Street alignment (cognitive simplicity)

## Risk Mitigation

### Low Risk Factors
- Zero blast radius: No external importers
- Localized callers: All in same file
- Stable code: Not in top 50 hotspots (low churn)
- Clear extraction path: 26 lines, 4 nesting levels

### Mitigation Strategies
1. Preserve signatures: Do not change method signature of CancelOrphanedTargets
2. Maintain behavior: Extracted methods must produce identical results
3. Test coverage: Add unit tests for extracted methods (Phase 5)
4. Incremental approach: Extract one method at a time, verify build after each

## Boundary Validation

### Scope Creep Prevention
- One epic = one concern: Only refactor CancelOrphanedTargets
- No while we are here fixes: Do not touch adjacent code
- No pre-existing error fixes: Verify build passes before starting
- Director approval required: For any scope expansion

### Verification Checklist
- Only CancelOrphanedTargets method modified
- No changes to caller methods
- No changes to callee methods
- No changes to other files
- Build passes before refactoring starts
- Build passes after refactoring completes

## Conclusion

EPIC-W7-146 scope is APPROVED for Phase 2 (Architecture Planning)

This epic has:
- Clear boundaries: Single method in single file
- Minimal risk: Zero blast radius, localized callers
- Achievable target: CYC 13 to 8 or less via 2-3 extractions
- Strong rationale: Jane Street alignment, cognitive simplicity

Next Phase: Proceed to Phase 2 to design extraction architecture and ticket breakdown.
