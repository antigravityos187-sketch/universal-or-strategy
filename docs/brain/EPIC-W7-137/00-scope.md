# Phase 1: Scope Definition - EPIC-W7-137

## Agent Tracking
- Agent Name: v12-phase1-scope
- Bobcoins Used: 0.18
- API Key: jCodemunch MCP
- Execution Time: 2026-06-24T19:43:01Z
- Input: docs/brain/EPIC-W7-137/00-hotspots.md

## Target Method
- Method: FleetSync_SyncFollowersToLevel
- File: src/V12_002.Trailing.cs
- Line: 142
- Current CYC: 13
- Target CYC: 8 or less
- Reduction Required: 5 points

## Scope Boundary Analysis

### IN SCOPE

#### Primary Extraction Target
Method: FleetSync_SyncFollowersToLevel (CYC=13)
- Responsibility: Synchronize follower positions to fleet-level trailing stops
- Complexity Drivers:
  - Conditional logic for position validation
  - Stop price calculation branches
  - Order update decision trees
  - Error handling paths
  - Pending replacement management

#### Extraction Strategy
Target: Extract 2-3 helper methods to reduce CYC from 13 to 8 or less

Candidate Extractions (based on hotspot analysis):
1. Position Validation Logic (estimated CYC reduction: 2-3)
   - Extract conditional checks for active positions
   - Validate position state before synchronization
   
2. Stop Price Calculation and Validation (estimated CYC reduction: 2-3)
   - Extract stop price calculation logic
   - Consolidate price validation branches
   - Calls: CalculateStopForLevel, ValidateStopPrice

3. Order Update Decision Logic (estimated CYC reduction: 1-2)
   - Extract decision tree for stop order updates
   - Consolidate pending replacement checks
   - Calls: UpdateStopOrder, HandleStalePendingReplacement

#### Files to Modify
- src/V12_002.Trailing.cs (PRIMARY)
  - Extract helpers from FleetSync_SyncFollowersToLevel
  - Maintain existing caller chain (ManageTrail_RunFleetSymmetrySync)
  - Preserve 48 downstream callees

### OUT OF SCOPE

#### Caller Methods (No Changes)
- ManageTrail_RunFleetSymmetrySync (Line 99)
  - Caller of target method
  - No modifications required
  - Maintains existing call signature

- ManageTrailingStops (Line 39)
  - Indirect caller (depth=2)
  - No modifications required

#### Downstream Callees (No Changes)
48 callees across 3 depth levels - ALL OUT OF SCOPE:
- Depth 1: CalculateStopForLevel, UpdateStopOrder, LogBuffer.Format, etc.
- Depth 2: ValidateStopPrice, HandleStalePendingReplacement, etc.
- Depth 3: Validate_LongIsIllegalAdjust, MarkStickyDirty, etc.

Rationale: These methods are called BY the target method but are not being refactored. We only extract logic WITHIN the target method.

#### Other Files (No Changes)
- src/V12_002.cs (main strategy file)
- src/V12_002.SIMA.Lifecycle.cs
- src/V12_002.Atm.cs
- All other V12_002 partial classes

Rationale: Zero blast radius - no external importers. Changes are isolated to src/V12_002.Trailing.cs.

#### Test Files (Deferred to Phase 5.V)
- tests/V12_Performance.Tests/ (verification phase)
- Unit tests will be added AFTER extraction in Phase 5.V

## Extraction Scope Summary

### Scope Metrics
- Files to Modify: 1 (src/V12_002.Trailing.cs)
- Methods to Extract: 2-3 helpers
- Methods to Modify: 1 (FleetSync_SyncFollowersToLevel)
- Blast Radius: 0 (no external importers)
- Caller Impact: 0 (signature unchanged)
- Callee Impact: 0 (downstream calls preserved)

### Complexity Reduction Plan
- Current CYC: 13
- Target CYC: 8 or less
- Reduction Required: 5 points
- Strategy: Extract conditional branches into 2-3 focused helper methods
- Expected Outcome: CYC 8 or less per method (Jane Street strict standard)

## Risk Assessment

### Extraction Risk: LOW
Factors:
1. Isolated Scope: No cross-file dependencies
2. Stable Callers: Only 2 callers, both in same file
3. Clear Responsibility: Fleet-level stop synchronization
4. No Signature Change: Callers unaffected
5. Testing Surface: 48 downstream callees require verification

### Mitigation Strategy
1. Preserve Semantics: Maintain exact fleet synchronization behavior
2. No Signature Changes: Keep method signature identical
3. Incremental Extraction: Extract one helper at a time
4. Verification: Test after each extraction (Phase 5.V)

## Jane Street Alignment

### Complexity Threshold Compliance
- Current: CYC=13 (EXCEEDS threshold by 5 points)
- Target: CYC 8 or less (Jane Street strict standard)
- Approach: Extract conditional logic to achieve cognitive simplicity

### Cognitive Load Reduction
- Nesting Depth: 5 to Target 3 or less (flatten nested conditionals)
- Lines of Code: 50 to Target 30 or less per method
- Single Responsibility: Maintain fleet synchronization focus

## Success Criteria

### Phase 1 Completion
- Scope boundary defined (IN SCOPE vs OUT OF SCOPE)
- Extraction targets identified (2-3 helpers)
- Risk assessment completed (LOW risk)
- Jane Street alignment verified (CYC 8 or less target)

### Phase 2 Prerequisites
- Clear extraction candidates identified
- Complexity reduction strategy defined
- Risk mitigation plan documented
- No scope creep (ONE EPIC = ONE CONCERN)

## Next Phase: Architecture Planning (Phase 2)

Inputs for Phase 2:
1. This scope definition (00-scope.md)
2. Source code analysis (src/V12_002.Trailing.cs:142)
3. Jane Street KB query results (complexity reduction patterns)

Phase 2 Deliverables:
1. Detailed extraction plan (method signatures, responsibilities)
2. Mermaid diagrams (before/after call graphs)
3. Test strategy (48 downstream callees)
4. Implementation tickets (Phase 4)

## Metadata

- Epic ID: EPIC-W7-137
- Wave: 7
- Phase: 1 (Scope Definition)
- Status: COMPLETED
- Timestamp: 2026-06-24T19:43:01Z
- Analyzer: v12-phase1-scope (Sequential Thinking MCP)
- Input: 00-hotspots.md
- Output: 00-scope.md
