# Phase 1: Scope Definition - EPIC-W7-001

**Epic**: EPIC-W7-001
**Target Method**: `ShouldSkipFleet_RunHealthCheck`
**File**: `V12_002.SIMA.Fleet.cs`
**Current Complexity**: 31 (CYC)
**Target Complexity**: ≤8 (Jane Street strict standard)

---

## Scope Boundary Definition

### IN SCOPE ✅

#### Primary Target
- **Method**: `ShouldSkipFleet_RunHealthCheck` (CYC 31)
- **File**: `V12_002.SIMA.Fleet.cs`
- **Action**: Extract to 5 single-responsibility predicates

#### Extraction Plan
1. **`IsFleetStateValid()`** - Fleet state validation logic
   - Target CYC: ≤3
   - Responsibility: Validate fleet operational state
   
2. **`IsHealthCheckTimingValid()`** - Timing window validation
   - Target CYC: ≤3
   - Responsibility: Check if health check timing is appropriate
   
3. **`IsFSMStateHealthy()`** - FSM state validation
   - Target CYC: ≤3
   - Responsibility: Verify FSM state health
   
4. **`AreResourcesAvailable()`** - Resource availability checks
   - Target CYC: ≤3
   - Responsibility: Validate resource availability
   
5. **`ShouldSkipFleet_RunHealthCheck()`** - Orchestration method
   - Target CYC: ≤5
   - Responsibility: Coordinate predicate calls and return decision

#### Verification Requirements
- All extracted methods must have CYC ≤8
- Main orchestration method must have CYC ≤5
- Unit tests required for each extracted predicate
- Integration test for orchestration method
- Build must pass after extraction
- deploy-sync.ps1 must execute successfully

---

### OUT OF SCOPE ❌

#### Adjacent Methods (Not Modified)
- Other methods in `V12_002.SIMA.Fleet.cs` with CYC ≤8
- Fleet initialization methods
- Fleet teardown methods
- Fleet event handlers (unless directly called by target method)

#### Other Subsystems
- SIMA Lifecycle methods
- SIMA ATM methods
- Order management methods
- Drawing/UI methods
- IPC command processing

#### Infrastructure
- FSM/Actor pattern implementation (already compliant)
- Logging infrastructure
- Error handling framework
- Configuration management

#### Documentation
- README updates (unless method signatures change public API)
- Architecture diagrams (unless fleet subsystem structure changes)
- User-facing documentation

---

## Extraction Boundaries

### What Gets Extracted
- **Conditional Logic**: All if/else/switch statements in target method
- **State Validation**: Fleet state checks
- **Timing Logic**: Health check timing window validation
- **Resource Checks**: Resource availability validation
- **FSM Validation**: FSM state health checks

### What Stays in Original Method
- **Method Signature**: Preserve existing signature
- **Return Type**: Maintain boolean return type
- **Orchestration**: High-level coordination of predicates
- **Logging**: Top-level logging statements (predicates log internally)

---

## Risk Assessment

### Low Risk ✅
- Predicate extraction is mechanical transformation
- Each predicate has single responsibility
- No changes to method signature
- No changes to caller contracts

### Medium Risk ⚠️
- Test coverage gaps (31 branches = 2^31 paths)
- Potential for logic errors during extraction
- Need careful verification of predicate boundaries

### High Risk ❌
- None identified (scope is well-bounded)

---

## Success Criteria

### Functional Requirements
- [ ] All 5 predicates extracted with CYC ≤3
- [ ] Orchestration method has CYC ≤5
- [ ] Original method behavior preserved (no logic changes)
- [ ] All predicates have unit tests
- [ ] Integration test validates orchestration

### Quality Gates
- [ ] Build passes (zero errors)
- [ ] All tests pass (100% pass rate)
- [ ] CSharpier formatting check passes
- [ ] ASCII-only compliance verified
- [ ] deploy-sync.ps1 executes successfully
- [ ] F5 in NinjaTrader successful (BUILD_TAG appears)

### Documentation
- [ ] Method extraction documented in commit message
- [ ] CYC before/after recorded in commit message
- [ ] BUILD_TAG included in commit message
- [ ] "Recent Major Refactors" table updated in src/AGENTS.md

---

## Scope Validation

### Boundary Checks
✅ **Single File**: Only `V12_002.SIMA.Fleet.cs` modified
✅ **Single Method**: Only `ShouldSkipFleet_RunHealthCheck` refactored
✅ **No Signature Changes**: Method signature preserved
✅ **No Caller Changes**: No changes to calling code
✅ **No Adjacent Methods**: Other fleet methods untouched

### Complexity Budget
- **Before**: 31 CYC (1 method)
- **After**: ~15 CYC total (5 methods, distributed)
- **Reduction**: 16 CYC reduction in main method
- **Compliance**: ✅ All methods ≤8 CYC

---

## Agent Tracking

- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: ~5 (scope analysis)
- **API Key**: None (plan mode, no MCP tools)
- **Execution Time**: ~2 minutes
- **Mode**: plan (strategic planning, no code changes)

---

**Status**: Phase 1 Complete ✅
**Next Phase**: Phase 1.5 (Scope Boundary Validation)
**Blocker**: None
