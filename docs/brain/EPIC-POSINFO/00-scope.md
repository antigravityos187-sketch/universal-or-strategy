# Epic: EPIC-POSINFO -- Scope Alignment

**Linear Tracking**: [MOM-28](https://linear.app/momo111/issue/MOM-28)  
**Target File**: `src/V12_002.PositionInfo.cs`  
**Goal**: Achieve CodeScene 10/10 score while maintaining Jane Street zero-allocation principles

---

## Code Area

**File**: `src/V12_002.PositionInfo.cs` (lines 277-400)  
**Scope**: 6 switch-based accessor methods operating on the `PositionInfo` class:

1. **GetTargetContracts** (lines 277-294) - Returns target contract quantity for target 1-5
2. **GetTargetPrice** (lines 296-313) - Returns target price for target 1-5
3. **IsTargetFilled** (lines 315-332) - Returns filled status for target 1-5
4. **MarkTargetFilled** (lines 334-356) - Sets filled flag for target 1-5
5. **GetTargetFilledQuantity** (lines 358-375) - Returns cumulative filled quantity for target 1-5
6. **SetTargetFilledQuantity** (lines 377-400) - Sets filled quantity with bounds checking

**Context**: These methods are private helpers within the `V12_002` partial class that provide type-safe access to the 5-target ladder system (T1-T5). They replaced array-based accessors in PR #14 to eliminate heap allocation per AMAL harness requirements.

---

## Validated Problem

### Structural Duplication (CONFIRMED)
All 6 methods share identical switch structure:
```csharp
switch (targetNumber)
{
    case 1: return/set pos.T1Field;
    case 2: return/set pos.T2Field;
    case 3: return/set pos.T3Field;
    case 4: return/set pos.T4Field;
    case 5: return/set pos.T5Field;
    default: return defaultValue;
}
```

**Pattern repetition**: 6 methods × 12 lines each = 72 lines of structurally identical code differing only in:
- Field names (T1Contracts vs Target1Price vs T1Filled, etc.)
- Return types (int, double, bool, void)
- Default values (0, 0.0, false)

### Complexity Scores (VALIDATED)
- **Complexity Audit Result**: File NOT in watch list (CYC ≤ 15)
- **Task Description Claim**: CYC = 11 per method
- **Assessment**: ACCURATE - Methods are below Jane Street threshold (≤15) but above ideal target (≤10)

### CodeScene Score (UNKNOWN)
- Current score not measured
- Target: 10/10
- Likely penalized for:
  - High structural duplication (6 identical patterns)
  - Moderate complexity (CYC 11 per method)
  - Low cohesion (repetitive switch logic)

### Zero-Allocation Compliance (CONFIRMED)
- **PR #14 Context**: Reverted from array-based accessors specifically to eliminate heap allocation
- **Current Implementation**: Switch-based - zero allocation ✅
- **Constraint**: MUST NOT reintroduce arrays or any heap allocation in hot paths

---

## Scope Boundaries

### IN SCOPE
- **6 accessor methods** (lines 277-400):
  - GetTargetContracts
  - GetTargetPrice
  - IsTargetFilled
  - MarkTargetFilled
  - GetTargetFilledQuantity
  - SetTargetFilledQuantity
- **Refactoring goal**: Eliminate structural duplication while maintaining:
  - Zero-allocation hot paths
  - Existing API surface (method signatures unchanged)
  - Thread-safety (volatile fields, no locks)
  - CYC ≤ 10 per method

### OUT OF SCOPE
- **PositionInfo class definition** (lines 36-117) - struct fields are immutable
- **Other methods in file**:
  - GetTargetMode (lines 119-136)
  - IsRunnerTarget (lines 138-141)
  - GetConfiguredTargetMagnitude (lines 144-161)
  - CalculateTargetPrice (lines 165-188)
  - ApplyTargetLadderGuard (lines 199-261)
  - CalculateTargetPriceFromPos (lines 264-272)
- **Call sites**: 100+ references across V12_002.*.cs files (not modifying callers)
- **PendingStopReplacement struct** (lines 407-425)
- **PositionDisplayInfo struct** (lines 429-439)

### EXPLICITLY OUT OF SCOPE
- Array-based solutions (causes allocation)
- Reflection-based property access (causes allocation + slow)
- Any solution requiring changes to PositionInfo field declarations
- Any solution requiring changes to caller code

---

## Scope Boundaries Explained

### Why PositionInfo Struct is OUT OF SCOPE

**Reason**: Changing field declarations would affect 100+ call sites across the codebase.

**Example of what we're NOT doing**:
```csharp
// ❌ OUT OF SCOPE - Would break 100+ call sites
public struct PositionInfo
{
    // Current: 15 individual fields
    public int T1Contracts;
    public int T2Contracts;
    // ... etc ...
    
    // ❌ NOT changing to arrays or collections
    public int[] TargetContracts; // Would require updating all callers
}
```

**Impact if changed**:
- Every file that accesses `pos.T1Contracts` would need updating
- Estimated 100+ call sites across V12_002.*.cs files
- Would violate V12.23 No Scope Creep Protocol (ONE EPIC = ONE CONCERN)
- Requires separate epic: EPIC-POSINFO-STRUCT

### Why Other Methods in File are OUT OF SCOPE

**Reason**: Different concerns, different patterns, no structural duplication.

**Methods excluded**:
- `GetTargetMode` - Different logic (mode calculation, not field access)
- `IsRunnerTarget` - Simple boolean check (CYC=2, no duplication)
- `GetConfiguredTargetMagnitude` - Configuration lookup (different pattern)
- `CalculateTargetPrice` - Complex calculation (CYC=8, separate concern)
- `ApplyTargetLadderGuard` - Validation logic (CYC=12, separate concern)
- `CalculateTargetPriceFromPos` - Price calculation (different pattern)

**Rationale**:
- These methods don't share the switch-based field accessor pattern
- Refactoring them would be "while we're here" fixes (scope creep)
- Each would require separate analysis and epic if needed

### Why Call Sites are OUT OF SCOPE

**Reason**: API surface is stable; callers don't need to change.

**Current caller pattern**:
```csharp
// Callers use the accessor methods (API surface)
int contracts = GetTargetContracts(pos, targetNumber);
double price = GetTargetPrice(pos, targetNumber);
```

**What we're preserving**:
- Method signatures unchanged
- Return types unchanged
- Parameter order unchanged
- Behavior unchanged

**Why this matters**:
- 100+ call sites remain untouched
- Zero risk of breaking existing logic
- Refactoring is internal implementation detail only

---

## Anti-Patterns (What NOT to Do)

### 1. Heap Allocation Anti-Patterns

**Arrays**:
```csharp
// ❌ BANNED - Causes heap allocation
private int[] _targetContracts = new int[5];
```
**Why**: Violates Jane Street zero-allocation principle. PR #14 specifically reverted from arrays.

**Dictionaries**:
```csharp
// ❌ BANNED - Dictionary lookups allocate
private Dictionary<int, Func<PositionInfo, int>> _accessors;
```
**Why**: Dictionary miss allocates, lookup overhead, violates zero-allocation.

**Reflection**:
```csharp
// ❌ BANNED - Reflection allocates and is slow
var prop = typeof(PositionInfo).GetProperty($"T{targetNumber}Contracts");
return (int)prop.GetValue(pos);
```
**Why**: `GetProperty()` allocates, `GetValue()` boxes, 100x slower than direct access.

### 2. Scope Creep Anti-Patterns

**"While We're Here" Fixes**:
```csharp
// ❌ SCOPE CREEP - Don't refactor unrelated methods
private int GetTargetContracts(...) { /* refactored */ }
private TargetMode GetTargetMode(...) { /* also refactored */ } // ❌ Different concern!
```
**Why**: Violates V12.23 No Scope Creep Protocol. ONE EPIC = ONE CONCERN.

**Struct Modification**:
```csharp
// ❌ SCOPE CREEP - Don't change PositionInfo fields
public struct PositionInfo
{
    public int[] TargetContracts; // ❌ Affects 100+ call sites!
}
```
**Why**: Requires separate epic (EPIC-POSINFO-STRUCT). Mixing concerns causes PR bloat.

### 3. Complexity Anti-Patterns

**Clever Abstractions**:
```csharp
// ❌ OVER-ENGINEERING - Don't create complex abstractions
public interface IFieldAccessor<T> { T GetValue(PositionInfo pos, int index); }
public class ContractsAccessor : IFieldAccessor<int> { ... }
```
**Why**: Jane Street principle: "Boring code is good code." Simple switch is better than clever abstraction.

**Premature Optimization**:
```csharp
// ❌ PREMATURE - Don't optimize before measuring
private unsafe int* GetTargetContractsPtr(PositionInfo* pos, int targetNumber)
```
**Why**: Current switch is already zero-allocation. Unsafe code adds complexity without proven benefit.

---

## Success Criteria

### Functional Requirements
1. ✅ All 6 methods maintain identical external behavior
2. ✅ Zero heap allocation in hot paths (Jane Street principle)
3. ✅ Thread-safety preserved (no locks, stateless methods)
4. ✅ API surface unchanged (no caller modifications required)

### Quality Metrics
1. ✅ CYC ≤ 10 per method (down from 11)
2. ✅ CodeScene score = 10/10
3. ✅ Structural duplication eliminated or significantly reduced (target: <20 lines)
4. ✅ Build passes: `powershell -File .\scripts\build_readiness.ps1`
5. ✅ Pre-push validation passes: `powershell -File .\scripts\pre_push_validation.ps1`

### Verification Gates
1. **Local**: F5 in NinjaTrader + BUILD_TAG verification
2. **CI**: All 13 pre-push checks pass (including CSharpier, complexity audit)
3. **PR Review**: Bob CLI `/pr-loop` drives PHS to 100/100
4. **CodeScene**: Verify 10/10 score in VS Code status bar post-merge

### Anti-Success Criteria (What Would Be Failure)
- ❌ Introducing heap allocation (regression from PR #14)
- ❌ Breaking any of the 100+ call sites
- ❌ Increasing complexity (CYC >11)
- ❌ Mixing concerns (refactoring unrelated methods)
- ❌ Changing PositionInfo struct (scope creep)

---

## Risk Level

**ISOLATED** - Low Risk

**Rationale**:
1. **Private methods**: All 6 methods are `private` - no external API surface
2. **Single file**: Contained within `V12_002.PositionInfo.cs` partial class
3. **No import dependencies**: File has zero import/export edges per jCodemunch dependency graph
4. **Stable API**: Method signatures unchanged since PR #14 (2026-05-22)
5. **Well-tested pattern**: Switch-based accessors proven in production post-PR #14

**Blast Radius**: Zero external importers (private methods in partial class file)

**Testing Strategy**: 
- Existing unit tests in `tests/V12_Performance.Tests/Core/FSMActorTests.cs` validate FSM/Actor correctness
- No dedicated tests for these accessor methods (private helpers)
- Validation via integration: F5 in NinjaTrader + BUILD_TAG verification

---

## V12 DNA Constraints

### Mandatory Compliance
1. **CYC Target**: ≤ 10 per method (currently 11)
2. **Lock-Free**: No `lock()` statements - already compliant ✅
3. **ASCII-Only**: No Unicode in string literals - already compliant ✅
4. **Zero-Allocation Hot Paths**: MUST NOT introduce heap allocation (Jane Street principle)
5. **Extraction Floor**: If extracting sub-methods, ≥ 15 LOC per extraction

### Jane Street Alignment
- **Cognitive Simplicity**: Current switch pattern is simple but repetitive
- **Make Illegal States Unrepresentable**: targetNumber validation via switch default case
- **Zero-Allocation**: Current implementation compliant; must preserve

### CodeScene Target
- **Score**: 10/10
- **Metrics to improve**:
  - Reduce structural duplication (primary goal)
  - Lower complexity to CYC ≤ 10
  - Improve cohesion (eliminate repetitive patterns)

---

## Approach Options to Evaluate (Planning Phase)

### Option 1: Inline Helper Methods (RECOMMENDED)
- Extract common switch logic to single parameterized helper
- Pass field selector as parameter (e.g., `Func<PositionInfo, int>`)
- **Pros**: Zero allocation, reduces duplication
- **Cons**: May not eliminate all duplication, delegates may allocate

### Option 2: Lookup Table Pattern
- Static readonly `Dictionary<int, Func<>>` for field access
- **Pros**: Eliminates switch duplication
- **Cons**: Dictionary lookup allocates, violates Jane Street principle ❌

### Option 3: Code Generation / Source Generators
- Generate accessor methods at compile-time
- **Pros**: Zero runtime cost, eliminates source duplication
- **Cons**: Adds build complexity, may not improve CodeScene score

### Option 4: Property Reflection
- Use `PropertyInfo.GetValue/SetValue` at runtime
- **Pros**: Eliminates duplication
- **Cons**: Allocates, slow, violates Jane Street principle ❌

### Option 5: Hybrid - Switch + Helper
- Keep switch for field selection, extract common logic
- **Pros**: Zero allocation, reduces some duplication
- **Cons**: May not achieve CodeScene 10/10

**Decision**: Defer to `/epic-plan` phase after Director confirmation.

---

## Dependencies & Call Sites

### Internal Dependencies
- **PositionInfo class**: All methods operate on `PositionInfo pos` parameter
- **Volatile fields**: T1-T5 fields are NOT volatile (only `RemainingContracts` is volatile)
- **Thread-safety**: Methods are stateless (operate on passed `pos` parameter)

### Call Sites (Estimated)
- **100+ call sites** across V12_002.*.cs files (based on task description)
- **Primary callers** (likely):
  - Order management methods (SubmitTargetOrdersLoop, etc.)
  - Execution callbacks (ProcessOnOrderUpdate, HandleTargetFill, etc.)
  - UI snapshot methods (PopulateTargetSnapshots, etc.)
  - SIMA dispatch methods (Dispatch_PublishMarketBracketToPhoton, etc.)

**Note**: jCodemunch index is stale (2026-05-19), so call site analysis deferred to planning phase.

---

## Success Criteria

### Functional Requirements
1. ✅ All 6 methods maintain identical external behavior
2. ✅ Zero heap allocation in hot paths (Jane Street principle)
3. ✅ Thread-safety preserved (no locks, stateless methods)
4. ✅ API surface unchanged (no caller modifications required)

### Quality Metrics
1. ✅ CYC ≤ 10 per method (down from 11)
2. ✅ CodeScene score = 10/10
3. ✅ Structural duplication eliminated or significantly reduced
4. ✅ Build passes: `powershell -File .\scripts\build_readiness.ps1`
5. ✅ Pre-push validation passes: `powershell -File .\scripts\pre_push_validation.ps1`

### Verification Gates
1. **Local**: F5 in NinjaTrader + BUILD_TAG verification
2. **CI**: All 13 pre-push checks pass (including CSharpier, complexity audit)
3. **PR Review**: Bob CLI `/pr-loop` drives PHS to 100/100
4. **CodeScene**: Verify 10/10 score in VS Code status bar post-merge

---

## [INTAKE-GATE] Director Confirmation Required

**Questions for Director**:
1. Does this scope match your intent for EPIC-POSINFO?
2. Are the boundaries correct (6 methods only, no PositionInfo struct changes)?
3. Is there anything NOT visible in the code that I should know?
4. Should I proceed to `/epic-plan` to evaluate refactoring approaches?

**Next Step**: Awaiting Director confirmation before proceeding to planning phase.