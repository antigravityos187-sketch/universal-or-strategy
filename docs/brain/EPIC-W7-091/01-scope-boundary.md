# Phase 1: Scope Boundary - EPIC-W7-091

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: N/A
- **Execution Time**: 2026-06-24T01:34:01Z

## Epic Objective
Reduce cyclomatic complexity of CancelDirectFallbackOrders from CYC=10 to <=8 (Jane Street strict standard).

## Target Method
- **Method**: CancelDirectFallbackOrders
- **File**: src/V12_002.Safety.Watchdog.cs
- **Line**: 268
- **Current CYC**: 10
- **Target CYC**: <=8
- **Lines of Code**: 28

## IN SCOPE

### Primary Extraction Target
- **CancelDirectFallbackOrders method** (src/V12_002.Safety.Watchdog.cs:268)
- Extract conditional validation logic into helper methods
- Extract order state filtering logic into helper methods
- Reduce CYC from 10 to <=8 (2-point reduction minimum)

### Allowed Modifications
- Create 1-2 private helper methods within V12_002.Safety.Watchdog.cs
- Refactor conditional branches into extracted methods
- Preserve existing method signature and public interface
- Maintain watchdog safety semantics (no behavioral changes)
- Update unit tests if they exist for this method

### Success Criteria
- CancelDirectFallbackOrders CYC reduced to <=8
- All extracted methods have CYC <=8
- Zero compilation errors
- Zero behavioral changes (logic preservation)
- Build passes: dotnet build
- Hard-link sync: deploy-sync.ps1 executes successfully

## OUT OF SCOPE

### Explicitly Excluded
- **Caller methods** (ExecuteWatchdogDirectFallback, OnWatchdogTimer)
  - These methods are NOT targets for this epic
  - Do not modify their complexity or structure

- **Other Safety.Watchdog methods**
  - Only CancelDirectFallbackOrders is targeted
  - Do not refactor adjacent methods

- **Cross-file changes**
  - Zero blast radius confirmed - no external dependencies
  - Do not modify any files outside src/V12_002.Safety.Watchdog.cs

- **Behavioral changes**
  - Do not alter watchdog safety logic
  - Do not change order cancellation semantics
  - Do not modify error handling behavior

- **Signature changes**
  - Do not change method parameters (currently 2 params)
  - Do not change return type
  - Do not change access modifiers

- **Performance optimization**
  - Focus is complexity reduction, not performance
  - Do not introduce caching, async, or other optimizations

- **Test framework changes**
  - If tests exist, update them for extracted methods
  - Do not migrate test frameworks (xUnit mandate already enforced)

## Scope Validation

### Blast Radius Check
- **Zero external dependencies confirmed**
  - Importer count: 0
  - Direct dependents: 0
  - Overall risk score: 0.0

### Caller Analysis
- **2 callers, both in same file**
  - ExecuteWatchdogDirectFallback (line 244)
  - OnWatchdogTimer (line 36, indirect via ExecuteWatchdogDirectFallback)
  - No cross-file callers

### Hotspot Priority
- **Not in top 50 hotspots**
  - Low churn (stable code)
  - Moderate complexity (CYC=10)
  - Low refactoring urgency vs top hotspots

## Risk Assessment

### Refactoring Risk: MINIMAL
- Isolated within single file
- Zero external dependencies
- Stable code (low churn)
- Small scope (28 lines)
- Leaf method (no downstream callees)

### Complexity Reduction Target
- **Current**: CYC=10
- **Target**: CYC<=8
- **Reduction**: 2 points minimum
- **Strategy**: Extract 1-2 helper methods for conditional logic

## Boundary Enforcement

### Scope Creep Prevention
- **ONE EPIC = ONE CONCERN**: Only CancelDirectFallbackOrders
- **No "while we're here" fixes**: Do not touch adjacent code
- **No pre-existing error fixes**: Verify build passes before starting
- **Separate PRs for separate concerns**: If other issues found, report to Director

### Director Approval Required For
- Expanding scope beyond CancelDirectFallbackOrders
- Modifying caller methods
- Cross-file changes
- Behavioral modifications

## Next Phase
Proceed to **Phase 2: Architecture Planning** to design extraction strategy.
