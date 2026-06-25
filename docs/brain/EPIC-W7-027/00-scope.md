# Phase 1: Scope Definition - EPIC-W7-027

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:05:40Z

## Epic Context
- **Target Method**: Dispatch_PublishMarketBracketToPhoton
- **File**: src/V12_002.SIMA.Dispatch.cs
- **Current CYC**: 9 (verified via jCodemunch)
- **Target CYC**: ≤8 (Jane Street strict standard)
- **Delta**: +1 over threshold
- **Priority**: LOW (minimal complexity delta)

## Critical Findings from Phase 0
1. **CYC Discrepancy Resolved**: Roadmap stated 21, actual is 9
2. **Blast Radius**: ZERO (isolated method, no external dependencies)
3. **Risk Level**: LOW (no cascade impact)
4. **Hotspot Ranking**: NOT in top 50 hotspots
5. **Parameter Count**: 16 (excessive, primary refactoring target)

## Scope Boundary Decision

### IN SCOPE ✅

#### 1. Parameter Object Extraction (PRIMARY)
**Target**: Reduce 16 parameters to 1 context object
- Create BracketDispatchContext struct
- Group related parameters:
  - Entry order parameters (price, quantity, side)
  - Stop order parameters (price, offset)
  - Target order parameters (prices, quantities, runner flags)
  - Fleet configuration (FSM, slot, ring buffer)
- **Expected CYC Impact**: 9 → 7 (reduces branching on parameter validation)

#### 2. Minimal Validation Extraction (SECONDARY)
**Target**: Extract pre-flight parameter validation
- Extract ValidateBracketDispatchContext() helper
- Validate context object before dispatch logic
- **Expected CYC Impact**: 7 → 6 (removes 1-2 conditional branches)

#### 3. Documentation & Testing
- Add XML documentation to extracted methods
- Add unit tests for context object validation
- Update call sites (2 callers identified in Phase 0)

### OUT OF SCOPE ❌

#### 1. Photon Ring Buffer Logic
**Rationale**: Already well-encapsulated in existing helpers
- EnqueueToPhotonRing() already exists
- No complexity reduction benefit
- Risk of introducing bugs in lock-free code

#### 2. Callees Refactoring
**Rationale**: 58 callees are already extracted helpers
- PublishPhoton_StopOrder, PublishPhoton_TargetOrders, etc. are separate methods
- No cascading refactoring needed
- Blast radius is zero

#### 3. Caller Modifications Beyond Signature
**Rationale**: Only 2 callers, minimal impact
- Dispatch_ProcessFleetLoop (line 196)
- ExecuteSmartDispatchEntry (line 45)
- Only signature change needed (pass context object)
- No logic changes in callers

#### 4. Nesting Depth Reduction
**Rationale**: Current depth of 3 is acceptable
- Jane Street threshold is CYC, not nesting depth
- Nesting depth of 3 is within normal range
- No benefit for 1-point CYC reduction

#### 5. Line Count Reduction
**Rationale**: 142 lines is manageable
- Not a God-method (threshold ~300+ lines)
- Line count is not a V12 DNA mandate
- Focus on CYC reduction only

## Extraction Strategy

### Step 1: Create BracketDispatchContext Struct
Create a struct to encapsulate all 16 parameters into a single context object.

### Step 2: Extract Validation Helper
Create ValidateBracketDispatchContext() to validate the context object before dispatch.

### Step 3: Refactor Main Method Signature
Change from 16 parameters to 1 context parameter.

### Step 4: Update Call Sites
Update the 2 caller methods to create and pass context objects.

## Expected Outcomes

### Complexity Metrics
- **CYC**: 9 → 6 (below threshold ✅)
- **Parameters**: 16 → 1 (massive improvement ✅)
- **Nesting Depth**: 3 → 3 (unchanged, acceptable)
- **Lines**: 142 → ~120 (minor reduction)

### Code Quality Improvements
- ✅ Reduced cognitive load (1 parameter vs 16)
- ✅ Improved testability (context object is mockable)
- ✅ Better encapsulation (related data grouped)
- ✅ Easier to extend (add fields to context vs add parameters)

### Risk Assessment
- **Blast Radius**: ZERO (no external dependencies)
- **Caller Impact**: MINIMAL (only 2 call sites)
- **Test Coverage**: NEW (add unit tests for validation)
- **Regression Risk**: LOW (isolated change)

## Success Criteria
1. ✅ CYC reduced from 9 to ≤8
2. ✅ Parameter count reduced from 16 to 1
3. ✅ All existing tests pass
4. ✅ New unit tests added for context validation
5. ✅ Build passes (dotnet build)
6. ✅ deploy-sync.ps1 executed successfully
7. ✅ F5 in NinjaTrader successful

## Scope Validation (Phase 1.5 Gate)
This scope is MINIMAL and SURGICAL:
- ✅ Single concern (parameter object extraction)
- ✅ Clear boundary (no callees refactoring)
- ✅ Measurable outcome (CYC 9→6)
- ✅ Low risk (zero blast radius)
- ✅ Aligns with V12 DNA (simplicity, testability)

## Recommendation
**PROCEED TO PHASE 2** with this minimal scope.

**Rationale**:
- Scope is well-defined and bounded
- Expected CYC reduction is achievable (9→6)
- Risk is minimal (zero blast radius)
- Effort is low (1-2 hours of work)
- Aligns with Jane Street principle: Make illegal states unrepresentable

## Phase 1 Completion
- Scope definition complete ✅
- IN SCOPE items clearly defined ✅
- OUT OF SCOPE items clearly defined ✅
- Extraction strategy documented ✅
- Success criteria established ✅
- Ready for Phase 1.5 (Scope Boundary Validation) ✅
