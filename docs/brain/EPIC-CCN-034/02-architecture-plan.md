# Phase 2: Architecture Planning - EPIC-CCN-034

## Target Method Analysis

**Method**: `ManageCIT`  
**File**: `src/V12_002.Orders.Management.Flatten.cs`  
**Current Complexity**: 19 (CYC)  
**Current LOC**: 77  
**Target Complexity**: ≤8 (Jane Street strict standard)

## Extraction Strategy

### Complexity Breakdown

**Current ManageCIT (CYC 19)**:
- Early validation returns: 3 branches
- CIT offset parsing: 1 branch
- Main order loop: 1 branch
- Order state validation: 3 branches
- Price trigger logic: 2 branches (BUILD 984 directional fix)
- Follower determination: 1 branch
- Nudge execution: 2 branches
- Broker budget management: 2 branches
- Error handling: 4 branches

**Proposed Extraction**:
1. **ValidateCITPrerequisites** (CYC 4) - Early validation + config parsing
2. **ShouldNudgeOrder** (CYC 6) - Order state + price trigger validation
3. **ExecuteCITNudge** (CYC 5) - Nudge calculation + execution with budget
4. **ManageCIT (reduced)** (CYC 5) - Orchestration only

**Total Complexity**: Max CYC 6 (meets ≤8 target)

## Method Signatures

### Original Method
```csharp
private void ManageCIT()
```

### Extracted Helper Methods

#### 1. ValidateCITPrerequisites
```csharp
private double ValidateCITPrerequisites()
```

**Responsibility**: 
- Check activePositions.Count and entryOrders.Count
- Validate ChaseIfTouchPoints configuration
- Check _propagationActive flag (BUILD 924 Fix C)
- Parse CIT offset from string

**Complexity**: CYC 4

#### 2. ShouldNudgeOrder
```csharp
private bool ShouldNudgeOrder(Order order, string orderKey)
```

**Responsibility**:
- Validate order state (Working)
- Validate order type (Limit only)
- Check if already nudged
- Apply BUILD 984 directional price trigger logic

**Complexity**: CYC 6

#### 3. ExecuteCITNudge
```csharp
private bool ExecuteCITNudge(Order order, string orderKey, double citOffset, ref int brokerBudget)
```

**Responsibility**:
- Determine local vs follower
- Calculate nudge distance and new limit price
- Execute follower nudge or local nudge
- Manage broker budget (BUILD 1109)
- Mark order as nudged

**Complexity**: CYC 5

### Reduced ManageCIT (Orchestration)
**Complexity**: CYC 5

## Lock-Free Validation

✅ No lock() statements
✅ FSM/Actor Enqueue pattern
✅ Atomic primitives only
✅ Correctness by construction

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
- Target Met: Max CYC 6 across all methods
- Each method has single, clear responsibility

### Testing Strategy
20+ test cases covering all branches

## Risk Assessment

- Compilation Risk: LOW
- Runtime Risk: MEDIUM
- Testing Risk: HIGH

## Approval Status

**Phase 2 Status**: ✅ APPROVED

**Next Phase**: Phase 3 (Implementation)
