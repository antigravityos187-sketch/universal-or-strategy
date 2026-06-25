# Phase 1: Scope Definition - EPIC-W7-102

## Agent Tracking
- Agent Name: v12-phase1-scope
- Bobcoins Used: 0.00
- API Key: jCodemunch MCP
- Execution Time: 2026-06-24T19:37:46Z

## Epic Overview
- Target Method: InitializeFollowerBracketFSM
- File: src/V12_002.SIMA.Fleet.cs
- Current Complexity: 14 (Target: ≤8)
- Current Nesting: 6 (Target: ≤3)
- Lines: 53

## Scope Boundary Definition

### IN SCOPE ✅

**Primary Extraction Target:**
- Method: InitializeFollowerBracketFSM (lines 120-173)
- File: src/V12_002.SIMA.Fleet.cs

**Extraction Candidates (within method body):**
1. **Parameter Validation Block** (lines ~122-130)
   - Null checks for leaderFSM, leaderOrder, followerOrder
   - Early return logic
   - Target CYC: 2-3

2. **FSM Initialization Logic** (lines ~135-155)
   - FollowerBracket object creation
   - Property assignments (LeaderFSM, LeaderOrder, FollowerOrder, etc.)
   - _followerBrackets dictionary insertion
   - Target CYC: 2-3

3. **Nested Conditional Blocks** (lines ~158-170)
   - Deep nesting (6 levels) for state-dependent logic
   - Bracket type determination
   - Target CYC: 3-4

**Refactoring Constraints:**
- Maintain private method visibility
- Preserve all 5 parameters (leaderFSM, leaderOrder, followerOrder, bracketType, offsetTicks)
- Keep method signature unchanged for 3 existing callers
- No changes to _followerBrackets field structure

### OUT OF SCOPE ❌

**Caller Methods (no modifications):**
- ProcessFleetSlot (line 44)
- PumpFleetDispatch (line 233)
- ProcessValidPhotonSlot (line 395)

**Related Infrastructure (no modifications):**
- FollowerBracket class definition
- _followerBrackets field declaration
- SIMA_FSM class structure
- Order class structure

**Other Fleet Methods (separate epics):**
- ProcessFleetSlot (CYC unknown)
- PumpFleetDispatch (CYC unknown)
- ProcessValidPhotonSlot (CYC unknown)

**Test Files (separate epic):**
- No test modifications in this epic
- Tests will be added in Phase 5.V (Verification)

## Extraction Strategy

### Target Metrics
- **Before:** CYC 14, Nesting 6, Lines 53
- **After:** CYC ≤8, Nesting ≤3, Lines ~20-25 (main method)

### Proposed Extractions
1. **ExtractParameterValidation()** → CYC 2-3
2. **ExtractFSMInitialization()** → CYC 2-3
3. **ExtractBracketTypeLogic()** → CYC 3-4

### Success Criteria
- Main method CYC reduced from 14 to ≤8
- Max nesting reduced from 6 to ≤3
- All 3 callers continue to work unchanged
- No external API changes
- Build passes
- F5 in NinjaTrader successful

## Risk Mitigation

**Low Blast Radius Confirmed:**
- 0 external dependencies
- 3 callers (all in same file)
- Private method (no API surface)
- No dynamic dispatch

**Regression Prevention:**
- Preserve exact method signature
- Maintain all parameter semantics
- Keep _followerBrackets field access pattern
- No changes to caller sites

## Dependencies

**Phase 0 Artifacts:**
- ✅ 00-hotspots.md (completed)

**Phase 2 Requirements:**
- Architecture plan for 3 extracted methods
- Mermaid diagrams for call flow
- Jane Street KB query results

## Scope Validation

**Boundary Checks:**
- ✅ Single method target (no scope creep)
- ✅ No caller modifications
- ✅ No infrastructure changes
- ✅ Clear extraction candidates identified
- ✅ Metrics-driven (CYC 14→8, Nesting 6→3)

**Jane Street Alignment:**
- ✅ Cognitive simplicity (reduce nesting)
- ✅ Single responsibility (extract sub-concerns)
- ✅ Testability (smaller methods easier to test)
- ✅ Correctness by construction (preserve invariants)

## Next Phase (Phase 1.5)
- Validate scope boundary with Sequential Thinking MCP
- Confirm no hidden dependencies
- Verify extraction candidates are independent
- Check for edge cases in nested logic
