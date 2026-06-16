# Phase 0: Hotspot Analysis - EPIC-004

## Epic Overview
**Epic ID**: EPIC-004
**Target File**: src/V12_002.SIMA.Dispatch.cs
**Objective**: Reduce complexity of 3 dispatch methods to ≤8 CYC

## Target Methods

### Method 1: Dispatch_PublishMarketBracketToPhoton
- **Current Complexity**: CYC=21
- **Lines of Code**: 189
- **Target Complexity**: ≤8
- **Reduction Required**: 13 points

### Method 2: Dispatch_ProcessFleetLoop
- **Current Complexity**: CYC=14
- **Lines of Code**: 113
- **Target Complexity**: ≤8
- **Reduction Required**: 6 points

### Method 3: Dispatch_PublishLimitEntryToPhoton
- **Current Complexity**: CYC=11
- **Lines of Code**: 95
- **Target Complexity**: ≤8
- **Reduction Required**: 3 points

## Complexity Analysis

### Dispatch_PublishMarketBracketToPhoton (CYC=21)
**Risk Level**: HIGH
- Highest complexity in the epic (21 vs target 8)
- Largest method (189 LOC)
- Likely contains multiple responsibilities
- Primary refactoring candidate

**Suspected Complexity Drivers**:
- Conditional branching for market bracket validation
- State machine logic for order lifecycle
- Error handling and edge cases
- Photon message construction and publishing

### Dispatch_ProcessFleetLoop (CYC=14)
**Risk Level**: MEDIUM
- Moderate complexity (14 vs target 8)
- 113 LOC suggests focused responsibility
- Loop processing with conditional logic

**Suspected Complexity Drivers**:
- Fleet iteration logic
- Conditional processing per fleet item
- State updates and synchronization
- Error handling within loop

### Dispatch_PublishLimitEntryToPhoton (CYC=11)
**Risk Level**: MEDIUM
- Lowest complexity in epic (11 vs target 8)
- 95 LOC indicates relatively focused method
- Similar pattern to Method 1 (Photon publishing)

**Suspected Complexity Drivers**:
- Limit order validation
- Photon message construction
- Conditional logic for order parameters
- Error handling

## Blast Radius Assessment

**File Context**: V12_002.SIMA.Dispatch.cs
- Part of SIMA (State-Indexed Market Adapter) subsystem
- Dispatch layer handles order routing to Photon kernel
- Critical path for order execution

**Potential Impact Areas**:
1. **Photon Kernel Integration**: Changes may affect message contracts
2. **SIMA State Machine**: Dispatch methods likely interact with FSM
3. **Order Lifecycle**: Market bracket and limit entry flows
4. **Fleet Processing**: Multi-order coordination logic

**Dependency Risk**: MEDIUM-HIGH
- Dispatch methods are likely called by higher-level orchestration
- Changes must preserve message semantics for Photon
- Fleet processing affects multiple concurrent orders

## Extraction Strategy

### Phase 1: Dispatch_PublishMarketBracketToPhoton (CYC=21 to 8)
**Recommended Approach**: Extract validation, message construction, and publishing

**Candidate Extractions**:
1. ValidateMarketBracketParameters() - Input validation logic
2. BuildMarketBracketMessage() - Photon message construction
3. PublishToPhotonKernel() - Publishing and error handling
4. HandleMarketBracketErrors() - Error recovery logic

**Expected Outcome**: Main method becomes orchestrator (CYC ≤8)

### Phase 2: Dispatch_ProcessFleetLoop (CYC=14 to 8)
**Recommended Approach**: Extract loop body into dedicated method

**Candidate Extractions**:
1. ProcessSingleFleetItem() - Per-item processing logic
2. ValidateFleetItem() - Item validation
3. UpdateFleetState() - State synchronization

**Expected Outcome**: Loop becomes simple iterator (CYC ≤8)

### Phase 3: Dispatch_PublishLimitEntryToPhoton (CYC=11 to 8)
**Recommended Approach**: Similar to Method 1, extract validation and construction

**Candidate Extractions**:
1. ValidateLimitEntryParameters() - Input validation
2. BuildLimitEntryMessage() - Message construction
3. PublishLimitEntry() - Publishing logic

**Expected Outcome**: Main method becomes orchestrator (CYC ≤8)

## Risk Mitigation

### Testing Requirements
- Unit tests for each extracted method
- Integration tests for Photon message contracts
- Regression tests for fleet processing

### V12 DNA Compliance
- No locks (Actor/FSM pattern already in place)
- ASCII-only strings (verify during extraction)
- Atomic state transitions (preserve FSM semantics)
- Correctness by construction (use enums/types for validation)

## Success Criteria

### Quantitative
- Method 1: CYC ≤8 (currently 21)
- Method 2: CYC ≤8 (currently 14)
- Method 3: CYC ≤8 (currently 11)
- All extracted methods: CYC ≤8
- Zero compilation errors
- Zero test failures

### Qualitative
- Code reads like prose (single responsibility per method)
- No nested conditionals more than 2 levels deep
- Clear separation of concerns (validation/construction/publishing)
- Photon message contracts preserved
- FSM state transitions remain atomic

## Next Steps

1. **Phase 1**: Generate detailed implementation plan
2. **Phase 2**: Architect extraction strategy with Mermaid diagrams
3. **Phase 3**: DNA and PR audit (Arena AI red team)
4. **Phase 4**: Execute extractions (Bob CLI)
5. **Phase 5**: Verification and testing
6. **Phase 6**: Deploy and monitor

## Notes

- All three methods follow similar patterns (validate, construct, publish)
- Opportunity for shared helper methods across all three
- Fleet processing (Method 2) has unique loop complexity
- Photon integration is critical path - changes must be surgical
- Consider extracting common Photon publishing logic into shared utility

---

**Analysis Date**: 2026-06-14
**Analyzer**: V12 Phase 0 Hotspot Analyzer
**Status**: READY FOR PHASE 1
