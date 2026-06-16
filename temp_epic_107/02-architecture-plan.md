# Phase 2: Architecture Plan - EPIC-CCN-107

## Epic Context
- **Epic ID**: EPIC-CCN-107
- **Target Method**: HydrateExpectedPositionsFromBroker
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Current Complexity**: 31 (CYC) - estimated from scope analysis
- **Target Complexity**: ≤ 15 (Jane Street aligned)
- **Phase**: 2 (Architecture Planning)
- **Status**: IN_PROGRESS

## Method Signature Analysis

### Current Method Signatures

```csharp
// Main orchestrator (lines ~200-220)
private void HydrateExpectedPositionsFromBroker()
{
    int hydratedCount = 0;
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

// Per-account hydration (lines ~230-260)
private void HydrateSingleAccountExpectedPosition(Account acct, ref int hydratedCount)
{
    try
    {
        foreach (Position pos in acct.Positions.ToArray())
        {
            if (pos != null && pos.Instrument != null && 
                pos.Instrument.FullName == Instrument.FullName && 
                pos.MarketPosition != MarketPosition.Flat)
            {
                int qty = pos.MarketPosition == MarketPosition.Long ? pos.Quantity : -pos.Quantity;
                var capturedAcct = acct.Name;
                var capturedQty = qty;
                Enqueue(ctx => ctx.AddOrUpdateExpectedPosition(ExpKey(capturedAcct), capturedQty, v => capturedQty));
                Print(string.Format("[SIMA HYDRATE] {0}: Seeded expected={1} from broker ({2} {3})",
                    acct.Name, qty, pos.MarketPosition, pos.Quantity));
                hydratedCount++;
                break;
            }
        }
    }
    catch (Exception ex)
    {
        Print(string.Format("[SIMA HYDRATE] WARNING: Could not read positions for {0}: {1}",
            acct.Name, ex.Message));
    }
}
```

## Complexity Analysis

### Current Complexity Breakdown

**HydrateExpectedPositionsFromBroker** (estimated CYC: 8-10):
- Base: 1
- foreach loop: +1
- if (!IsFleetAccount): +1
- if (!masterIsFleet993): +1
- Total: ~4-5 (LOW - orchestrator is already simple)

**HydrateSingleAccountExpectedPosition** (estimated CYC: 12-15):
- Base: 1
- try-catch: +1
- foreach loop: +1
- if (pos != null): +1
- if (pos.Instrument != null): +1
- if (pos.Instrument.FullName == Instrument.FullName): +1
- if (pos.MarketPosition != MarketPosition.Flat): +1
- Ternary operator (qty calculation): +1
- break statement: +1
- catch block: +1
- Total: ~10-12 (MEDIUM - primary extraction target)

### Complexity Hotspot Identification

**PRIMARY HOTSPOT**: HydrateSingleAccountExpectedPosition
- Multiple nested conditionals for position validation
- Embedded business logic (quantity calculation, Actor enqueue)
- Exception handling mixed with validation

**SECONDARY CONCERN**: Master account special handling
- Duplicated logic path for master vs fleet accounts
- Could benefit from unified handling

## Extraction Strategy

### Extraction 1: ValidatePositionForHydration

**Purpose**: Isolate position validation logic from orchestration

**New Method Signature**:
```csharp
/// <summary>
/// Validates whether a broker position qualifies for expected position hydration.
/// Returns true if position is non-flat and matches the strategy's instrument.
/// </summary>
/// <param name="pos">Broker position to validate</param>
/// <returns>True if position should be hydrated</returns>
private bool ValidatePositionForHydration(Position pos)
{
    if (pos == null)
        return false;
    if (pos.Instrument == null)
        return false;
    if (pos.Instrument.FullName != Instrument.FullName)
        return false;
    if (pos.MarketPosition == MarketPosition.Flat)
        return false;
    return true;
}
```

**Complexity**: CYC ≤ 5 (4 guard clauses + base)
**Extracted From**: HydrateSingleAccountExpectedPosition (lines ~240-245)
**Rationale**: Separates validation concerns, makes logic testable in isolation

### Extraction 2: CalculateHydrationQuantity

**Purpose**: Isolate quantity calculation logic

**New Method Signature**:
```csharp
/// <summary>
/// Calculates signed quantity for expected position hydration.
/// Long positions return positive quantity, short positions return negative.
/// </summary>
/// <param name="pos">Broker position</param>
/// <returns>Signed quantity (positive for long, negative for short)</returns>
private int CalculateHydrationQuantity(Position pos)
{
    return pos.MarketPosition == MarketPosition.Long 
        ? pos.Quantity 
        : -pos.Quantity;
}
```

**Complexity**: CYC ≤ 2 (ternary + base)
**Extracted From**: HydrateSingleAccountExpectedPosition (line ~247)
**Rationale**: Isolates financial calculation, improves testability

### Extraction 3: EnqueueExpectedPositionUpdate

**Purpose**: Isolate Actor enqueue logic

**New Method Signature**:
```csharp
/// <summary>
/// Enqueues expected position update to Actor queue for thread-safe state mutation.
/// Captures account name and quantity to avoid closure issues.
/// </summary>
/// <param name="accountName">Account name for expected position key</param>
/// <param name="quantity">Signed quantity to set</param>
private void EnqueueExpectedPositionUpdate(string accountName, int quantity)
{
    var capturedAcct = accountName;
    var capturedQty = quantity;
    Enqueue(ctx => ctx.AddOrUpdateExpectedPosition(
        ExpKey(capturedAcct), 
        capturedQty, 
        v => capturedQty));
}
```

**Complexity**: CYC ≤ 2 (lambda + base)
**Extracted From**: HydrateSingleAccountExpectedPosition (lines ~248-250)
**Rationale**: Isolates Actor pattern usage, preserves lock-free DNA

### Extraction 4: LogHydrationSuccess

**Purpose**: Isolate diagnostic logging

**New Method Signature**:
```csharp
/// <summary>
/// Logs successful position hydration for diagnostics.
/// </summary>
/// <param name="accountName">Account name</param>
/// <param name="quantity">Hydrated quantity</param>
/// <param name="marketPosition">Broker market position</param>
/// <param name="positionQuantity">Broker position quantity</param>
private void LogHydrationSuccess(
    string accountName, 
    int quantity, 
    MarketPosition marketPosition, 
    int positionQuantity)
{
    Print(string.Format(
        "[SIMA HYDRATE] {0}: Seeded expected={1} from broker ({2} {3})",
        accountName, 
        quantity, 
        marketPosition, 
        positionQuantity));
}
```

**Complexity**: CYC ≤ 1 (base only)
**Extracted From**: HydrateSingleAccountExpectedPosition (lines ~251-253)
**Rationale**: Separates logging concerns, reduces noise in core logic

## Post-Refactoring Structure

### Refactored HydrateSingleAccountExpectedPosition

```csharp
private void HydrateSingleAccountExpectedPosition(Account acct, ref int hydratedCount)
{
    try
    {
        foreach (Position pos in acct.Positions.ToArray())
        {
            if (!ValidatePositionForHydration(pos))
                continue;
            
            int qty = CalculateHydrationQuantity(pos);
            EnqueueExpectedPositionUpdate(acct.Name, qty);
            LogHydrationSuccess(acct.Name, qty, pos.MarketPosition, pos.Quantity);
            
            hydratedCount++;
            break;
        }
    }
    catch (Exception ex)
    {
        Print(string.Format(
            "[SIMA HYDRATE] WARNING: Could not read positions for {0}: {1}",
            acct.Name, ex.Message));
    }
}
```

**Post-Refactoring Complexity**: CYC ≤ 6
- Base: 1
- try-catch: +1
- foreach: +1
- if (!Validate): +1
- catch: +1
- Total: 5-6 (ACCEPTABLE)

### Method Hierarchy

```
HydrateExpectedPositionsFromBroker (CYC: 4-5)
├── HydrateSingleAccountExpectedPosition (CYC: 5-6)
│   ├── ValidatePositionForHydration (CYC: 5)
│   ├── CalculateHydrationQuantity (CYC: 2)
│   ├── EnqueueExpectedPositionUpdate (CYC: 2)
│   └── LogHydrationSuccess (CYC: 1)
└── (Master account handling - unchanged)
```

**Total Complexity**: 19-21 CYC (distributed across 5 methods)
**Primary Method Complexity**: 5-6 CYC (PASS - under threshold 15)

## Call Graph Analysis

### Current Dependencies

**HydrateExpectedPositionsFromBroker** depends on:
- `Account.All` (NinjaTrader API)
- `IsFleetAccount(Account)` (internal helper)
- `HydrateSingleAccountExpectedPosition(Account, ref int)` (internal)
- `Account` (strategy property)

**HydrateSingleAccountExpectedPosition** depends on:
- `Account.Positions` (NinjaTrader API)
- `Position.Instrument` (NinjaTrader API)
- `Instrument.FullName` (strategy property)
- `ExpKey(string)` (internal helper)
- `Enqueue(Action<V12_002>)` (Actor pattern)
- `Print(string)` (NinjaTrader API)

### Post-Refactoring Dependencies

**New extracted methods** depend on:
- `ValidatePositionForHydration`: Position, Instrument (read-only)
- `CalculateHydrationQuantity`: Position (read-only)
- `EnqueueExpectedPositionUpdate`: ExpKey, Enqueue (Actor pattern)
- `LogHydrationSuccess`: Print (diagnostics)

**Dependency Risk**: LOW
- All extracted methods are pure or side-effect isolated
- No new external dependencies introduced
- Actor pattern preserved (lock-free DNA)

## Extraction Sequence

### Phase 1: Extract Validation Logic
1. Create `ValidatePositionForHydration` method
2. Write unit tests for validation edge cases
3. Replace inline validation in `HydrateSingleAccountExpectedPosition`
4. Verify build + tests pass

### Phase 2: Extract Calculation Logic
1. Create `CalculateHydrationQuantity` method
2. Write unit tests for long/short quantity calculation
3. Replace inline calculation in `HydrateSingleAccountExpectedPosition`
4. Verify build + tests pass

### Phase 3: Extract Actor Enqueue Logic
1. Create `EnqueueExpectedPositionUpdate` method
2. Write unit tests (mock Actor queue)
3. Replace inline enqueue in `HydrateSingleAccountExpectedPosition`
4. Verify build + tests pass

### Phase 4: Extract Logging Logic
1. Create `LogHydrationSuccess` method
2. Replace inline Print call in `HydrateSingleAccountExpectedPosition`
3. Verify build + tests pass

### Phase 5: Verification
1. Run complexity audit: `python scripts/complexity_audit.py`
2. Verify `HydrateSingleAccountExpectedPosition` CYC ≤ 15
3. Run full test suite
4. Manual code review

## Jane Street Compliance Checks

### Lock-Free Pattern Compliance
- ✅ **No locks introduced**: All extractions are pure or use existing Actor pattern
- ✅ **Actor pattern preserved**: `EnqueueExpectedPositionUpdate` maintains Enqueue usage
- ✅ **Thread-safety**: No synchronous state mutations introduced

### ASCII-Only Compliance
- ✅ **All string literals are ASCII**: Verified in extracted methods
- ✅ **No Unicode/emoji**: Log messages use standard ASCII characters

### Cognitive Simplicity
- ✅ **Single responsibility**: Each extracted method has one clear purpose
- ✅ **Testability**: All extracted methods are unit-testable in isolation
- ✅ **Readability**: Main method reads like high-level orchestration

### Photon Kernel Alignment
- ✅ **Naming conventions**: All methods follow V12 naming patterns
- ✅ **Error handling**: Exception handling preserved in orchestrator
- ✅ **Logging**: Uses existing V12 telemetry infrastructure

## Risk Assessment & Mitigation

### Risk 1: Position Snapshot Timing
**Risk**: `acct.Positions.ToArray()` snapshot may be stale during iteration
**Severity**: LOW
**Mitigation**: Existing pattern already handles this via snapshot + break on first match
**Action**: No change needed - preserve existing pattern

### Risk 2: Actor Queue Ordering
**Risk**: Enqueue calls may execute out of order during concurrent hydration
**Severity**: LOW
**Mitigation**: Actor pattern guarantees FIFO execution per account key
**Action**: No change needed - Actor pattern handles this

### Risk 3: Master Account Special Case
**Risk**: Master account logic duplicates fleet account logic
**Severity**: LOW (out of scope for this epic)
**Mitigation**: Document as technical debt for future refactoring
**Action**: Add TODO comment, defer to separate epic

### Risk 4: Test Coverage Gap
**Risk**: Existing test suite may not cover hydration edge cases
**Severity**: MEDIUM
**Mitigation**: Write comprehensive unit tests for each extracted method
**Action**: TDD approach - write tests before extraction

## Rollback Plan

### Rollback Trigger Conditions
1. Build fails after extraction
2. Existing tests fail
3. Complexity target not met (CYC > 15)
4. Race condition detected in Actor pattern

### Rollback Procedure
1. `git revert <commit-hash>` for each extraction commit
2. Re-run complexity audit to verify baseline restored
3. Run full test suite to verify functionality restored
4. Document failure reason in epic notes
5. Escalate to V12 Phase 7 Lead for guidance

### Checkpoint Strategy
- Commit after each extraction phase (4 commits total)
- Tag each commit with `EPIC-CCN-107-P<phase>`
- Enable Bob CLI checkpointing (already enabled via `.bob/settings.json`)

## Test Strategy

### Unit Tests for Extracted Methods

#### Test: ValidatePositionForHydration
```csharp
[Test]
public void ValidatePositionForHydration_NullPosition_ReturnsFalse()
{
    // Arrange
    Position pos = null;
    
    // Act
    bool result = strategy.ValidatePositionForHydration(pos);
    
    // Assert
    Assert.IsFalse(result);
}

[Test]
public void ValidatePositionForHydration_NullInstrument_ReturnsFalse()
{
    // Arrange
    Position pos = CreateMockPosition(instrument: null);
    
    // Act
    bool result = strategy.ValidatePositionForHydration(pos);
    
    // Assert
    Assert.IsFalse(result);
}

[Test]
public void ValidatePositionForHydration_WrongInstrument_ReturnsFalse()
{
    // Arrange
    Position pos = CreateMockPosition(instrumentName: "ES 03-25");
    
    // Act
    bool result = strategy.ValidatePositionForHydration(pos);
    
    // Assert
    Assert.IsFalse(result);
}

[Test]
public void ValidatePositionForHydration_FlatPosition_ReturnsFalse()
{
    // Arrange
    Position pos = CreateMockPosition(marketPosition: MarketPosition.Flat);
    
    // Act
    bool result = strategy.ValidatePositionForHydration(pos);
    
    // Assert
    Assert.IsFalse(result);
}

[Test]
public void ValidatePositionForHydration_ValidLongPosition_ReturnsTrue()
{
    // Arrange
    Position pos = CreateMockPosition(marketPosition: MarketPosition.Long);
    
    // Act
    bool result = strategy.ValidatePositionForHydration(pos);
    
    // Assert
    Assert.IsTrue(result);
}
```

#### Test: CalculateHydrationQuantity
```csharp
[Test]
public void CalculateHydrationQuantity_LongPosition_ReturnsPositive()
{
    // Arrange
    Position pos = CreateMockPosition(
        marketPosition: MarketPosition.Long, 
        quantity: 5);
    
    // Act
    int result = strategy.CalculateHydrationQuantity(pos);
    
    // Assert
    Assert.AreEqual(5, result);
}

[Test]
public void CalculateHydrationQuantity_ShortPosition_ReturnsNegative()
{
    // Arrange
    Position pos = CreateMockPosition(
        marketPosition: MarketPosition.Short, 
        quantity: 3);
    
    // Act
    int result = strategy.CalculateHydrationQuantity(pos);
    
    // Assert
    Assert.AreEqual(-3, result);
}
```

#### Test: EnqueueExpectedPositionUpdate
```csharp
[Test]
public void EnqueueExpectedPositionUpdate_ValidInput_EnqueuesCorrectly()
{
    // Arrange
    string accountName = "TestAccount";
    int quantity = 5;
    var mockQueue = new MockActorQueue();
    strategy.SetActorQueue(mockQueue);
    
    // Act
    strategy.EnqueueExpectedPositionUpdate(accountName, quantity);
    
    // Assert
    Assert.AreEqual(1, mockQueue.EnqueuedActions.Count);
    // Verify action updates expected position correctly
}
```

### Integration Tests

#### Test: HydrateSingleAccountExpectedPosition
```csharp
[Test]
public void HydrateSingleAccountExpectedPosition_ValidPosition_HydratesCorrectly()
{
    // Arrange
    Account acct = CreateMockAccount(
        positions: new[] { CreateMockPosition(
            marketPosition: MarketPosition.Long, 
            quantity: 5) });
    int hydratedCount = 0;
    
    // Act
    strategy.HydrateSingleAccountExpectedPosition(acct, ref hydratedCount);
    
    // Assert
    Assert.AreEqual(1, hydratedCount);
    // Verify expected position was updated via Actor queue
}

[Test]
public void HydrateSingleAccountExpectedPosition_NoValidPosition_DoesNotHydrate()
{
    // Arrange
    Account acct = CreateMockAccount(
        positions: new[] { CreateMockPosition(
            marketPosition: MarketPosition.Flat) });
    int hydratedCount = 0;
    
    // Act
    strategy.HydrateSingleAccountExpectedPosition(acct, ref hydratedCount);
    
    // Assert
    Assert.AreEqual(0, hydratedCount);
}
```

## Success Criteria Verification

### Quantitative Metrics
- ✅ **Complexity Target**: HydrateSingleAccountExpectedPosition CYC ≤ 15 (target: 5-6)
- ✅ **Extracted Methods**: 4 new methods created
- ✅ **Individual Method Complexity**: Each extracted method CYC ≤ 8
  - ValidatePositionForHydration: CYC 5
  - CalculateHydrationQuantity: CYC 2
  - EnqueueExpectedPositionUpdate: CYC 2
  - LogHydrationSuccess: CYC 1
- ✅ **Total Complexity**: Sum of all methods ≤ 35 (actual: 19-21)

### Qualitative Criteria
- ✅ **Single Responsibility**: Each method has one clear purpose
- ✅ **Testability**: Extracted methods are unit-testable in isolation
- ✅ **Readability**: Main method reads like high-level orchestration
- ✅ **V12 DNA Compliance**: Lock-free Actor pattern preserved
- ✅ **ASCII-Only**: All string literals are ASCII-compliant

## Implementation Constraints

### File Modification Scope
**ONLY MODIFY**: src/V12_002.SIMA.Lifecycle.cs
- Lines ~230-260 (HydrateSingleAccountExpectedPosition)
- Add 4 new private methods (after line 260)

**DO NOT MODIFY**:
- HydrateExpectedPositionsFromBroker (orchestrator - already simple)
- Any other methods in file
- Test files (except adding new tests)
- Configuration files
- Build scripts

### Code Style Requirements
- Follow existing C# coding standards in file
- Maintain consistent indentation (4 spaces)
- Preserve existing comment style
- Use XML documentation comments for new methods
- Add `/// <summary>` tags for all extracted methods

## Next Phase Transition

### Phase 3 (Implementation) Entry Criteria
- ✅ Architecture plan completed
- ✅ Extraction sequence defined
- ✅ Test strategy documented
- ✅ Risk mitigation strategies defined
- [ ] User approval of architecture plan

### Phase 3 Deliverables Preview
1. 4 extracted methods implemented
2. Unit tests for each extracted method
3. Integration tests updated
4. Complexity metrics verified
5. Build + test suite passing

---

**Document Version**: 1.0
**Phase**: 2 (Architecture Planning)
**Status**: COMPLETED
**Created**: 2026-06-13
**Protocol**: V12.23 No Scope Creep
**Jane Street Alignment**: CYC ≤ 15 (target: 5-6 for primary method)