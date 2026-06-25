# Phase 1: Scope Definition - EPIC-W7-144

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T19:43:59Z

## Epic Metadata
- **Epic ID**: EPIC-W7-144
- **Target Method**: IsOrderAllowed
- **File**: src/V12_002.UI.Compliance.cs
- **Line**: 323
- **Current CYC**: 21
- **Target CYC**: <= 8 per extracted function

## Scope Boundary

### IN SCOPE

#### Primary Target
- **Method**: IsOrderAllowed(Order order) (lines 323-390)
  - Current CYC: 21
  - Current LOC: 67
  - Max Nesting: 5
  - Extraction Goal: Break into 3-4 functions, each CYC <= 8

#### Extraction Strategy
1. **Equity Validation Logic** (estimated CYC: 6-7)
   - Extract account equity peak checks
   - Extract daily profit limit checks
   - Target: ValidateAccountEquityLimits(Order order)

2. **Order Type Validation Logic** (estimated CYC: 5-6)
   - Extract order type-specific rules
   - Extract position size validation
   - Target: ValidateOrderTypeRules(Order order)

3. **Compliance Gate Logic** (estimated CYC: 4-5)
   - Extract final compliance checks
   - Extract logging/audit trail
   - Target: ApplyComplianceGates(Order order)

4. **Orchestrator Method** (estimated CYC: 3-4)
   - Coordinate extracted functions
   - Early return pattern for failures
   - Target: Refactored IsOrderAllowed(Order order)

### OUT OF SCOPE

#### Excluded from This Epic
1. **Other Compliance Methods** - Not touching related methods in same file
2. **Caller Discovery** - Zero callers detected; will validate usage in Phase 1.5
3. **LogBuffer Dependencies** - Logging infrastructure stays as-is
4. **Account State Fields** - accountEquityPeak, accountDailyProfit constants unchanged
5. **UI Layer Integration** - No changes to how UI calls this method
6. **Test Coverage** - Unit tests will be added in separate epic

#### Deferred to Future Epics
- Dead code analysis (if method truly has zero callers)
- Reflection/delegate invocation discovery
- Broader compliance module refactoring

## Scope Validation Requirements

### Phase 1.5 Gate (MANDATORY)
Before proceeding to Phase 2, MUST verify:

1. **Usage Confirmation**
   - Search codebase for dynamic invocation patterns
   - Check for reflection-based calls
   - Verify method is not dead code
   - Document actual call sites (if found)

2. **Boundary Verification**
   - Confirm no hidden dependencies beyond detected callees
   - Validate logging infrastructure is stable
   - Check for event handlers or delegates

3. **Risk Assessment**
   - If zero callers confirmed: Mark as dead code candidate
   - If callers found: Update blast radius analysis
   - If reflection-based: Document invocation pattern

**BLOCKER**: If method is confirmed dead code, CANCEL epic and create deletion ticket instead.

## Success Criteria

### Phase 5 Completion Targets
- All extracted functions have CYC <= 8
- Max nesting depth <= 3 across all functions
- Original method reduced to orchestrator (CYC <= 4)
- No change in observable behavior
- All logging preserved
- Build passes
- deploy-sync.ps1 executed successfully

### Quality Gates
- Pre-push validation passes (13 checks)
- CSharpier formatting compliant
- ASCII-only compliance maintained
- No new Codacy violations

## Extraction Boundaries

### What Changes
- Method body of IsOrderAllowed (lines 323-390)
- Addition of 3 new private helper methods
- Refactored orchestrator logic with early returns

### What Stays Unchanged
- Method signature: private bool IsOrderAllowed(Order order)
- Return type and parameter
- Logging statements (relocated but not modified)
- Field references (accountEquityPeak, accountDailyProfit)
- File location (stays in V12_002.UI.Compliance.cs)

## Risk Mitigation

### Low Blast Radius Advantage
- Zero detected external dependencies
- Changes are fully isolated to this method
- No ripple effects expected

### Complexity Reduction Path
- Current: 1 method x CYC 21 = 21 total complexity
- Target: 4 methods x CYC <= 8 = <= 32 total complexity
- Cognitive load: REDUCED (4 simple functions vs 1 complex function)

### Jane Street Alignment
- Target CYC <= 8 matches Jane Street strict standard
- Early return pattern reduces nesting
- Single-responsibility functions improve testability

## Notes

### Zero Caller Concern
**CRITICAL**: Phase 0 detected zero callers. Possible explanations:
1. Dead code (method never called)
2. Reflection-based invocation (not detected by static analysis)
3. Delegate/event handler (indirect invocation)
4. Recent refactoring orphaned this method

**Action**: Phase 1.5 MUST resolve this before proceeding to architecture planning.

### File Location Context
- File: V12_002.UI.Compliance.cs suggests UI-layer compliance logic
- Method name: IsOrderAllowed suggests pre-trade validation
- Logging dependencies: Indicates audit trail requirement

### Backup File Warning
- Method appears in both src/ and src-vm-backup/
- Canonical version: src/V12_002.UI.Compliance.cs
- Backup version: Ignore (stale copy)

## Next Phase

**Phase 1.5**: Scope Boundary Validation
- Verify method usage (resolve zero caller mystery)
- Confirm extraction boundaries
- Update risk assessment if needed
- BLOCKER: Cancel epic if confirmed dead code

**Phase 2**: Architecture Planning (only if Phase 1.5 passes)
