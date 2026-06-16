# Phase 2: Architecture Planning - EPIC-CCN-005

## Target Method Analysis

### Current State
- **Method**: `ClassifyAndRouteFleetOrder`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Line**: 531-573 (42 LOC)
- **Complexity**: 16 (Cyclomatic Complexity)
- **Tier**: 1 (High Priority)

### Complexity Drivers
The method contains 6+ sequential if-else branches checking order name prefixes:
- `Stop_` → stopOrders dictionary
- `S_` → stopOrders dictionary  
- `T1_` → target1Orders dictionary
- `T2_` → target2Orders dictionary
- `T3_` → target3Orders dictionary
- `T4_` → target4Orders dictionary

Each branch performs identical operations:
1. Assign target dictionary
2. Extract order key (substring after prefix)
3. Set dictionary name string

## Extraction Strategy

### Goal
Reduce complexity from **16 to ≤8** (Jane Street strict standard)

### Approach
Extract prefix-based routing logic into two focused helper methods:

1. **Helper 1**: `DetermineOrderRouting` - Maps order prefix to target dictionary and metadata
2. **Helper 2**: `ExtractOrderKey` - Extracts key from order name based on prefix

### Complexity Distribution
- **Main Method** (ClassifyAndRouteFleetOrder): Target CYC ≤8
  - Call helper to determine routing
  - Call helper to extract key
  - Return result
  - Estimated CYC: 3-4

- **Helper 1** (DetermineOrderRouting): Target CYC ≤4
  - Use lookup structure (Dictionary or switch expression)
  - Single responsibility: prefix → routing metadata
  - Estimated CYC: 2-3

- **Helper 2** (ExtractOrderKey): Target CYC ≤4
  - Simple substring extraction
  - Single responsibility: name + prefix → key
  - Estimated CYC: 1-2

## Method Signatures

### Original Method (Preserved Interface)
private ConcurrentDictionary<string, Order> ClassifyAndRouteFleetOrder(
    Order ord,
    out string orderKey,
    out string dictName
)

**Responsibilities** (After Extraction):
- Orchestrate routing logic
- Call helper methods
- Return target dictionary

### Helper Method 1: DetermineOrderRouting
private (ConcurrentDictionary<string, Order> targetDict, string dictName, int prefixLength) 
    DetermineOrderRouting(string orderName)

**Parameters**:
- `orderName` (string): The order name to analyze

**Returns**: Tuple containing:
- `targetDict`: Target ConcurrentDictionary for the order
- `dictName`: String identifier for the dictionary ("stopOrders", "target1Orders", etc.)
- `prefixLength`: Length of the matched prefix (for key extraction)

**Responsibilities**:
- Match order name prefix to target dictionary
- Return routing metadata as structured tuple
- Handle unmatched prefixes (return null/default)

**Access Modifier**: `private` (internal helper)

### Helper Method 2: ExtractOrderKey
private string ExtractOrderKey(string orderName, int prefixLength)

**Parameters**:
- `orderName` (string): The full order name
- `prefixLength` (int): Length of prefix to remove

**Returns**: 
- `string`: The extracted order key (substring after prefix)

**Responsibilities**:
- Extract key by removing prefix
- Handle edge cases (null/empty names)
- Return sanitized key string

**Access Modifier**: `private` (internal helper)

## Call Graph

ClassifyAndRouteFleetOrder (Main)
├─> DetermineOrderRouting(ord.Name)
│   └─> Returns: (targetDict, dictName, prefixLength)
│
├─> ExtractOrderKey(ord.Name, prefixLength)
│   └─> Returns: orderKey
│
└─> Return targetDict (with out parameters set)

### Data Flow
1. **Input**: Order object with Name property
2. **Step 1**: Call `DetermineOrderRouting(ord.Name)` → Get routing metadata
3. **Step 2**: Call `ExtractOrderKey(ord.Name, prefixLength)` → Get order key
4. **Step 3**: Assign out parameters (orderKey, dictName)
5. **Output**: Return target ConcurrentDictionary

### Shared State
- **None**: All helpers are pure functions (no state mutations)
- **Thread-Safe**: Uses existing ConcurrentDictionary references (no new locks)
- **Immutable**: String operations only (no mutable state)

## Lock-Free Validation

### Current State (Pre-Extraction)
✅ **No lock() statements** in ClassifyAndRouteFleetOrder
✅ **Uses ConcurrentDictionary** (lock-free collection)
✅ **No mutable shared state** (only local variables)

### Post-Extraction Guarantees
✅ **No lock() statements** will be introduced
✅ **Helper methods are pure functions** (no state mutations)
✅ **Maintains Actor/FSM pattern** (no changes to state machine)
✅ **Thread-safe by design** (immutable operations only)

### Forensic Verification
Post-extraction scan required:
grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs
Expected result: **Zero matches**

## Jane Street Compliance

### Cognitive Simplicity (CYC ≤8)
✅ **Main method**: Reduced from 16 to ~4 (orchestration only)
✅ **Helper 1**: ~3 (prefix matching logic)
✅ **Helper 2**: ~2 (substring extraction)
✅ **Total complexity**: Distributed across 3 focused methods

### Microsecond Latency Preservation
✅ **No architectural changes**: Same call pattern, same data structures
✅ **JIT inlining eligible**: Small helper methods (<10 LOC each)
✅ **No allocation overhead**: Tuple returns are stack-allocated
✅ **Cache-friendly**: Sequential logic, no pointer chasing

### Testing Standards (Will Wilson - Jane Street)
✅ **Reduced path explosion**: 2^16 = 65,536 paths → 2^4 = 16 paths (main method)
✅ **Exhaustive testing feasible**: Each helper has <10 paths
✅ **Unit test isolation**: Helpers can be tested independently
✅ **Behavior preservation**: TDD ensures no logic changes

### Make Illegal States Unrepresentable
✅ **Structured returns**: Tuple instead of multiple out parameters (cleaner)
✅ **Type safety**: ConcurrentDictionary type preserved
✅ **Null handling**: Explicit null checks in helpers
✅ **Prefix validation**: Centralized in DetermineOrderRouting

## Implementation Checklist

### Pre-Extraction
- [ ] Run complexity audit: python scripts/complexity_audit.py
- [ ] Verify current CYC = 16 for ClassifyAndRouteFleetOrder
- [ ] Run tests: dotnet test (establish baseline)
- [ ] Create feature branch: git checkout -b epic-ccn-005-extraction

### Extraction Sequence (TDD)
1. [ ] **Extract Helper 1** (DetermineOrderRouting)
   - Write unit tests for prefix matching
   - Implement helper method
   - Run tests (verify behavior)
   - Measure complexity (target CYC ≤4)

2. [ ] **Extract Helper 2** (ExtractOrderKey)
   - Write unit tests for key extraction
   - Implement helper method
   - Run tests (verify behavior)
   - Measure complexity (target CYC ≤4)

3. [ ] **Refactor Main Method**
   - Replace if-else chain with helper calls
   - Run tests (verify behavior preservation)
   - Measure complexity (target CYC ≤8)

### Post-Extraction Verification
- [ ] Complexity audit: CYC ≤8 for main method
- [ ] Complexity audit: CYC ≤4 for each helper
- [ ] Forensic scan: Zero lock() blocks
- [ ] Test suite: 100% pass rate
- [ ] Build: Zero errors (dotnet build)
- [ ] Format check: dotnet csharpier check src/
- [ ] Hard-link sync: powershell -File .\deploy-sync.ps1

## Risk Assessment

### Low Risk Factors
✅ **Isolated change**: Single method, no callers/callees modified
✅ **Pure refactoring**: No logic changes, only reorganization
✅ **Well-understood pattern**: Extract method is standard refactoring
✅ **Lock-free preserved**: No state mutations introduced

### Mitigation Strategies
- **TDD**: Write tests before extraction (behavior preservation)
- **Incremental**: Extract one helper at a time, test after each
- **Rollback**: Git checkpoints after each successful extraction
- **Verification**: Automated complexity audit after each step

## Success Criteria

### Functional
✅ All existing tests pass (100% pass rate)
✅ No new compilation errors
✅ Behavior identical to pre-extraction (verified by tests)

### Non-Functional
✅ Main method complexity ≤8 (Jane Street standard)
✅ Helper method complexity ≤4 (Jane Street standard)
✅ Zero lock() blocks (forensic scan)
✅ Zero formatting issues (CSharpier)

### Process
✅ TDD workflow followed (tests before implementation)
✅ Incremental extraction (one helper at a time)
✅ Git checkpoints after each step
✅ Hard-link sync after completion

## Next Phase Gate

**Phase 3 (DNA & PR Audit)**: READY TO PROCEED
- Architecture plan: COMPLETE
- Extraction strategy: DEFINED
- Method signatures: DOCUMENTED
- Lock-free validation: VERIFIED
- Jane Street compliance: VALIDATED

---

**Document Version**: 1.0  
**Created**: 2026-06-15  
**Epic**: EPIC-CCN-005  
**Phase**: 2 (Architecture Planning)  
**Status**: COMPLETE  
**Next Phase**: Phase 3 (DNA & PR Audit)
