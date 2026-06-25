# Phase 1: Scope Definition - EPIC-W7-074

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:33:47Z
- **Input**: docs/brain/EPIC-W7-074/00-hotspots.md

## Target Method
- **Method**: AttachExecutionPanelHandlers
- **File**: src/V12_002.UI.Panel.Handlers.cs
- **Line**: 96
- **Current CYC**: 12
- **Target CYC**: 8 or less (Jane Street strict standard)

## Scope Boundary Definition

### IN SCOPE

#### Primary Target
1. **AttachExecutionPanelHandlers** (src/V12_002.UI.Panel.Handlers.cs:96)
   - Current CYC: 12
   - Target CYC: 3 or less (orchestration only)
   - Lines: 54
   - Rationale: Exceeds Jane Street threshold by 4 points

#### Extraction Targets (New Methods)
1. **AttachExecutionModeHandlers**
   - Target CYC: 3 or less
   - Scope: Submit button click, execution mode radio buttons
   - Rationale: Logical grouping of execution mode UI handlers

2. **AttachRmaHandlers**
   - Target CYC: 3 or less
   - Scope: RMA button click, RMA visual updates
   - Rationale: Logical grouping of RMA-related handlers

3. **AttachClickTraderHandlers**
   - Target CYC: 3 or less
   - Scope: Click trader border management, state synchronization
   - Rationale: Logical grouping of click trader handlers

#### Files to Modify
- **src/V12_002.UI.Panel.Handlers.cs** (primary target file)

### OUT OF SCOPE

#### Caller Method
1. **AttachPanelHandlers** (src/V12_002.UI.Panel.Handlers.cs:42)
   - Rationale: Parent initialization method, not part of this epic
   - Note: Will call the refactored method unchanged

#### Callee Methods (26 symbols)
All callee methods are OUT OF SCOPE:
- **PanelCommand** (src/V12_002.UI.Panel.Handlers.cs:935)
- **ResetExecutionMode** (src/V12_002.UI.Panel.Handlers.cs:558)
- **TriggerGlow** (src/V12_002.UI.Panel.Lifecycle.cs:114)
- **Enqueue** (src/V12_002.cs:428)
- **ClearClickTraderBorderIfInactive** (src/V12_002.UI.Callbacks.cs:219)
- **UpdateRmaButtonVisual** (src/V12_002.UI.Panel.Handlers.cs:869)
- All depth 2 and depth 3 callees

Rationale: These are stable dependencies with their own complexity profiles.

#### Other Files
- **src/V12_002.UI.Panel.Lifecycle.cs** - OUT OF SCOPE
- **src/V12_002.UI.Callbacks.cs** - OUT OF SCOPE
- **src/V12_002.cs** - OUT OF SCOPE
- All test files - OUT OF SCOPE (tests will be added in Phase 5)

### Scope Justification

#### Why This Scope?
1. **Surgical Focus**: Single method extraction
2. **Low Blast Radius**: Zero direct dependents, single caller
3. **Clear Boundaries**: Event handler registration pattern
4. **Jane Street Alignment**: Reduce CYC from 12 to 8 or less
5. **Stable Dependencies**: All callees are stable

#### Risk Mitigation
- **Isolation**: Changes confined to UI.Panel.Handlers module
- **Single Caller**: Only AttachPanelHandlers calls this method
- **Low Churn**: Method not in top 50 hotspots
- **No Cascading Changes**: Zero blast radius

## Extraction Strategy

### Decomposition Plan
AttachExecutionPanelHandlers (CYC 12 to 3)
- AttachExecutionModeHandlers (CYC 3 or less)
- AttachRmaHandlers (CYC 3 or less)
- AttachClickTraderHandlers (CYC 3 or less)

### Expected Outcome
- **Main method**: CYC 3 or less (orchestration: 3 method calls)
- **Extracted methods**: CYC 3 or less each
- **Total methods**: 4 (1 main + 3 extracted)
- **Complexity distribution**: 12 points redistributed

## Success Criteria

### Phase 1 Completion
- Scope boundary clearly defined (IN SCOPE vs OUT OF SCOPE)
- Extraction targets identified (3 new methods)
- Files to modify listed (1 file)
- Risk assessment completed (LOW-MEDIUM risk)
- Jane Street alignment verified (CYC 8 or less target)

### Phase 2 Prerequisites
- Clear extraction strategy documented
- Logical groupings defined
- Target CYC thresholds specified
- No scope creep (single method focus)

## Scope Validation

### Scope Creep Prevention
- Do NOT refactor caller method (AttachPanelHandlers)
- Do NOT refactor callee methods (26 dependencies)
- Do NOT modify other UI files
- Do NOT add improvements outside scope
- ONLY extract AttachExecutionPanelHandlers into 3 helper methods

### Boundary Enforcement
**ONE EPIC = ONE CONCERN**: This epic focuses solely on reducing the complexity of AttachExecutionPanelHandlers from CYC 12 to 8 or less by extracting event handler registrations into logical groupings.

## Conclusion

**Scope Status**: DEFINED

This epic has a **clear, surgical scope**:
- Single method extraction (AttachExecutionPanelHandlers)
- Three logical groupings (execution mode, RMA, click trader)
- Low risk (zero blast radius, single caller, stable code)
- Jane Street aligned (CYC 12 to 8 or less)

**Ready for Phase 2**: Architecture Planning
