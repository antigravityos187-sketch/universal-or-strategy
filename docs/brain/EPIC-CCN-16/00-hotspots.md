# EPIC-CCN-16 Hotspot Analysis

## Target Method
- **Name**: HydrateFSMsFromWorkingOrders
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Line**: 488
- **Symbol ID**: src/V12_002.SIMA.Lifecycle.cs::V12_002.HydrateFSMsFromWorkingOrders#method

## Complexity Metrics
- **Cyclomatic Complexity**: 45 (HIGH)
- **Max Nesting Depth**: 5
- **Parameter Count**: 0
- **Assessment**: HIGH risk

## Hotspot Score: 134.808
- **Rank**: #4 in epic roadmap
- **Formula**: 45 × log(1 + 19) = 134.808
- **Churn**: 19 commits in 90 days (2.1/week - ACTIVE)

## Risk Assessment
- **Jane Street Threshold**: CYC ≤15 (current: 45 = 3x over)
- **Change Frequency**: ACTIVE (2.1 commits/week)
- **Testing**: No existing tests (TDD required)
- **Pattern**: God-function (multiple concerns)

## Extraction Strategy
1. Extract FSM state reconstruction logic
2. Extract order validation logic
3. Extract error handling paths
4. Keep minimal orchestration in parent

## Target Outcome
- **Target CYC**: ≤8
- **Reduction**: 82.2% (45 → 8)
- **Precedent**: EPIC-CCN-15 achieved 94% (67 → 4)

## Next Phase
Phase 1: Scope Definition (plan mode)