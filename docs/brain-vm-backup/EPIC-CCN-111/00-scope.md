# Phase 1: Scope Definition - EPIC-CCN-111

## Target Method Analysis

### Method Identification
- **Method Name**: `HydrateExpectedPositionsFromBroker`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Lines**: 225-247
- **Current Cyclomatic Complexity**: 17
- **Target Complexity**: ≤15 (Jane Street alignment)
- **Reduction Required**: Minimum 2 points

### Method Purpose
Seeds `expectedPositions` dictionary from live broker state during SIMA initialization. Prevents false Reaper CRITICAL DESYNC alerts when strategy restarts with open positions.

### Current Implementation Structure
```csharp
private void HydrateExpectedPositionsFromBroker()
{
    int hydratedCount = 0;
    
    // Fleet accounts iteration
    foreach (Account acct in Account.All)
    {
        if (!IsFleetAccount(acct))
            continue;
        HydrateSingleAccountExpectedPosition(acct, ref hydratedCount);
    }
    
    // Master account handling
    bool masterIsFleet993 = IsFleetAccount(Account);
    if (!masterIsFleet993)
        HydrateSingleAccountExpectedPosition(Account, ref hydratedCount);
}
```

## Complexity Analysis

### Primary Complexity Sources
1. **Fleet Account Iteration**: Loop with conditional filter
2. **Master Account Logic**: Separate conditional path for master account
3. **Delegate Method Complexity**: `HydrateSingleAccountExpectedPosition` (lines 249-294) contains:
   - Try-catch block
   - Nested foreach over positions
   - Multiple position validation conditionals
   - Actor queue enqueue operation
   - Logging with string formatting

### Complexity Distribution
- **Parent Method** (HydrateExpectedPositionsFromBroker): ~5 CCN
- **Delegate Method** (HydrateSingleAccountExpectedPosition): ~12 CCN
- **Combined Effective Complexity**: 17 CCN

## Extraction Strategy

### What to Extract
**Target**: Extract position validation logic from `HydrateSingleAccountExpectedPosition` into separate validation method.

**New Method**: `ValidateAndExtractBrokerPosition`
```csharp
private Position ValidateAndExtractBrokerPosition(Account acct)
{
    foreach (Position pos in acct.Positions.ToArray())
    {
        if (pos != null 
            && pos.Instrument != null 
            && pos.Instrument.FullName == Instrument.FullName 
            && pos.MarketPosition != MarketPosition.Flat)
        {
            return pos;
        }
    }
    return null;
}
```

**Refactored Method**: `HydrateSingleAccountExpectedPosition`
```csharp
private void HydrateSingleAccountExpectedPosition(Account acct, ref int hydratedCount)
{
    try
    {
        Position pos = ValidateAndExtractBrokerPosition(acct);
        if (pos == null)
            return;
            
        int qty = pos.MarketPosition == MarketPosition.Long 
            ? pos.Quantity 
            : -pos.Quantity;
            
        var capturedAcct = acct.Name;
        var capturedQty = qty;
        Enqueue(ctx => ctx.AddOrUpdateExpectedPosition(
            ExpKey(capturedAcct), capturedQty, v => capturedQty));
            
        Print(string.Format(
            "[SIMA HYDRATE] {0}: Seeded expected={1} from broker ({2} {3})",
            acct.Name, qty, pos.MarketPosition, pos.Quantity));
            
        hydratedCount++;
    }
    catch (Exception ex)
    {
        Print(string.Format(
            "[SIMA HYDRATE] WARNING: Could not read positions for {0}: {1}",
            acct.Name, ex.Message));
    }
}
```

### What to Keep
- **Parent method structure**: Fleet + master account iteration logic
- **Actor queue pattern**: Enqueue operation for lock-free state mutation
- **Error handling**: Try-catch for broker API failures
- **Logging**: Diagnostic output for hydration tracking

### Boundary Definition (V12.23 No Scope Creep Protocol)

**SINGLE METHOD SCOPE**: `HydrateExpectedPositionsFromBroker` and its immediate delegate `HydrateSingleAccountExpectedPosition`.

**OUT OF SCOPE**:
- ❌ `HydrateWorkingOrdersFromBroker` (separate method, separate epic)
- ❌ `HydrateFSMsFromWorkingOrders` (separate method, separate epic)
- ❌ `EnumerateApexAccounts` (caller method, separate epic)
- ❌ Other lifecycle methods in the file

**STRICT BOUNDARY**: Only touch lines 225-294. No changes to surrounding code.

## Success Criteria

### Complexity Targets
- ✅ **HydrateExpectedPositionsFromBroker**: CCN ≤10 (currently ~5, maintain)
- ✅ **HydrateSingleAccountExpectedPosition**: CCN ≤10 (currently ~12, reduce by 2+)
- ✅ **ValidateAndExtractBrokerPosition**: CCN ≤8 (new method)
- ✅ **Combined Effective Complexity**: ≤15 (Jane Street threshold)

### Functional Requirements
- ✅ Preserve exact behavior: Fleet + master account hydration
- ✅ Maintain Actor/FSM pattern: Enqueue for state mutations
- ✅ Preserve error handling: Try-catch for broker API failures
- ✅ Maintain logging: Diagnostic output unchanged
- ✅ ASCII-only compliance: No Unicode in string literals

### Quality Gates
- ✅ Build passes: `dotnet build` zero errors
- ✅ No lock() blocks: Grep verification passes
- ✅ CSharpier formatting: Auto-format compliance
- ✅ Pre-push validation: All 13 checks pass

## Risk Assessment

### Risk Level: LOW-MEDIUM

### Risk Factors
1. **Broker API Dependency**: Method reads live broker positions
   - **Mitigation**: Preserve try-catch, maintain ToArray() snapshot pattern
2. **Actor Queue Correctness**: Enqueue operation must remain atomic
   - **Mitigation**: Do not modify Enqueue call structure
3. **Master Account Logic**: Separate path for master vs fleet accounts
   - **Mitigation**: Preserve IsFleetAccount() conditional logic
4. **Testing Gap**: No dedicated test coverage for this method
   - **Mitigation**: Add TDD tests in Phase 2 before extraction

### Blast Radius
- **Scope**: ISOLATED - Only affects position hydration at startup
- **Callers**: `EnumerateApexAccounts` (single caller)
- **Callees**: `HydrateSingleAccountExpectedPosition` (single delegate)
- **State Impact**: Seeds `expectedPositions` dictionary (read-only after hydration)

### Rollback Plan
- **Checkpoint**: Bob CLI auto-checkpoint before extraction
- **Restore**: `/restore` command to revert changes
- **Verification**: `powershell -File .\deploy-sync.ps1` + F5 in NinjaTrader

## V12 DNA Alignment

### Correctness by Construction
- ✅ **Type Safety**: Position validation returns nullable Position (explicit null handling)
- ✅ **Illegal States**: Null position = no hydration (fail-safe default)
- ✅ **Atomic Operations**: Enqueue pattern preserves lock-free correctness

### Lock-Free Actor Pattern
- ✅ **No lock() blocks**: Grep verification required
- ✅ **Enqueue for mutations**: State updates via Actor queue
- ✅ **Atomic primitives**: No shared mutable state

### ASCII-Only Compliance
- ✅ **String literals**: No Unicode, emoji, or curly quotes
- ✅ **Logging**: ASCII-only diagnostic output

## Phase 1 Completion Checklist

- ✅ Target method identified and analyzed
- ✅ Complexity sources documented
- ✅ Extraction strategy defined
- ✅ Boundary strictly scoped (single method)
- ✅ Success criteria specified (CCN ≤15)
- ✅ Risk assessment completed (LOW-MEDIUM)
- ✅ V12 DNA alignment verified

## Next Phase

**Phase 2**: Implementation Planning (Bob CLI `v12-engineer`)
- Generate detailed implementation plan
- Create Mermaid diagrams for extraction flow
- Define TDD test cases for position hydration scenarios
- Prepare surgical extraction steps
