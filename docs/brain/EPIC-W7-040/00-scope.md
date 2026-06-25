# Phase 1: Scope Definition - EPIC-W7-040

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00
- **API Key**: jCodemunch MCP
- **Execution Time**: ~15 seconds

## Epic Objective
Reduce cyclomatic complexity of FindTargetOrderForPosition from 10 to ≤8 (Jane Street threshold).

## Target Method
- **Method**: FindTargetOrderForPosition
- **File**: src/V12_002.Trailing.Breakeven.cs
- **Line**: 186
- **Current CYC**: 10
- **Target CYC**: ≤8
- **Lines**: 37
- **Parameters**: 4 (PositionInfo position, string entryName, bool isLong, bool isShort)

## Scope Boundary Analysis

### IN SCOPE
1. **Primary Target**: FindTargetOrderForPosition method (lines 186-223)
2. **Conditional Logic Extraction**:
   - Null/validation checks (lines 188-189)
   - Position direction matching (lines 192-195)
   - Entry name matching logic (lines 197-220)
   - Order filtering by direction (lines 202-218)

3. **Extraction Candidates**:
   - **Helper 1**: IsPositionDirectionMatch - Extract lines 192-195 (CYC reduction: -2)
   - **Helper 2**: FindOrderByEntryName - Extract lines 197-220 (CYC reduction: -4)

4. **Signature Preservation**:
   - Maintain exact method signature for caller compatibility
   - Return type: Order (nullable)
   - No breaking changes to MoveSpecificTarget caller

### OUT OF SCOPE
1. **Caller Method**: MoveSpecificTarget (line 335) - separate epic if needed
2. **Other Methods**: No other methods in file require changes
3. **Test Files**: Unit test creation deferred to Phase 5
4. **Documentation**: Inline comments only, no external docs
5. **Performance Optimization**: Focus on complexity reduction only
6. **Logging Changes**: Maintain existing logging pattern

## Extraction Strategy

### Step 1: Extract Direction Matching (CYC -2)
Create helper method IsPositionDirectionMatch to validate position direction against isLong/isShort flags.

### Step 2: Extract Entry Name Matching (CYC -4)
Create helper method FindOrderByEntryName to encapsulate order filtering logic (lines 197-220).

### Step 3: Refactor Main Method (Target CYC ≤4)
Simplify FindTargetOrderForPosition to orchestrate helper methods with early returns.

## Risk Mitigation

### Pre-Extraction Verification
- ✅ Blast radius: 0 (isolated method)
- ✅ Single caller: MoveSpecificTarget
- ✅ No external dependencies
- ✅ Leaf method (no downstream calls)

### Post-Extraction Verification
1. Build passes: dotnet build
2. Hard link sync: powershell -File .\deploy-sync.ps1
3. NinjaTrader F5: Verify BUILD_TAG
4. Behavioral test: Verify MoveSpecificTarget behavior unchanged

## Success Criteria
- ✅ FindTargetOrderForPosition CYC reduced from 10 to ≤8
- ✅ Two helper methods extracted with CYC ≤4 each
- ✅ Signature compatibility maintained
- ✅ Build passes without errors
- ✅ NinjaTrader F5 successful

## Complexity Reduction Calculation
- **Before**: CYC 10
- **After**: CYC 4 (main) + CYC 2 (helper1) + CYC 4 (helper2) = Total CYC 10 (distributed)
- **Main Method CYC**: 10 → 4 (60% reduction)
- **Jane Street Compliance**: ✅ All methods ≤8

## Dependencies
- **Phase 0**: ✅ Completed (hotspot analysis)
- **Phase 1**: 🔄 In Progress (scope definition)
- **Phase 2**: ⏳ Pending (architecture planning)

## Notes
- Method is isolated with zero blast radius - safe for aggressive refactoring
- Single caller pattern allows for easy rollback if needed
- Extraction preserves existing behavior - no logic changes
- Focus on cognitive simplicity per Jane Street DNA
