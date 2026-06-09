# tests/ - Testing Rules

**Last Updated**: 2026-06-08T22:49:00Z
**Scope**: Unit tests, integration tests, and TDD workflow

---

## Test Structure

```
tests/
└── V12_Performance.Tests/
    └── Core/
        └── FSMActorTests.cs
```

**Current Coverage**: 1 test file (FSM/Actor Enqueue model)

**Gap**: No tests for complexity-extracted methods (45 methods with CYC > 20)

---

## Testing Standards

### Unit Test Pattern (Arrange-Act-Assert)
```csharp
[Fact]
public void LinkTargetOrderToFSM_ValidInputs_LinksSuccessfully()
{
    // Arrange
    var fsm = new SIMA_FSM { Id = "FSM-001" };
    var order = new Order { OrderId = "ORD-001" };
    
    // Act
    _lifecycle.LinkTargetOrderToFSM(order, fsm);
    
    // Assert
    Assert.Equal("ORD-001", fsm.TargetOrderId);
    Assert.Same(order, fsm.TargetOrder);
}
```

### Test Naming Convention
```
MethodName_Scenario_ExpectedBehavior
```

Examples:
- `LinkTargetOrderToFSM_NullOrder_ReturnsEarly`
- `ProcessIpcCommands_InvalidCommand_LogsError`
- `MonitorRmaProximity_WithinThreshold_TriggersAlert`

---

## TDD Workflow

### For New Extractions (EPIC-CCN-2 through EPIC-CCN-14)

**Step 1: Write Test First**
```csharp
[Fact]
public void ExtractedMethod_Scenario_ExpectedBehavior()
{
    // Arrange
    var input = CreateTestInput();
    
    // Act
    var result = _sut.ExtractedMethod(input);
    
    // Assert
    Assert.Equal(expectedValue, result);
}
```

**Step 2: Implement Method**
```csharp
private ReturnType ExtractedMethod(InputType input)
{
    // Implementation with CYC ≤ 8
}
```

**Step 3: Verify**
```bash
dotnet test
```

**Step 4: Refactor**
- If CYC > 8: Extract further
- If test fails: Fix implementation
- If test passes: Commit

---

## Test Categories

### FSM/Actor Tests
**Purpose**: Verify lock-free Enqueue model

**Key Tests**:
- State transitions are atomic
- No race conditions under concurrent load
- Enqueue order preserved

**Example**:
```csharp
[Fact]
public void Enqueue_ConcurrentCalls_MaintainsOrder()
{
    // Test concurrent enqueues maintain FIFO order
}
```

### Complexity-Extracted Method Tests
**Purpose**: Verify extracted methods maintain original behavior

**Pattern**:
1. Test original behavior (before extraction)
2. Extract method
3. Test extracted method in isolation
4. Test integration with caller

**Example**:
```csharp
[Theory]
[InlineData(null, false)]
[InlineData("", false)]
[InlineData("VALID", true)]
public void ValidateInput_VariousInputs_ReturnsExpected(string input, bool expected)
{
    var result = _validator.ValidateInput(input);
    Assert.Equal(expected, result);
}
```

### Integration Tests
**Purpose**: Verify end-to-end behavior

**Method**: F5 in NinjaTrader IDE
**Verification**: BUILD_TAG appears in output
**Success Criteria**: No compilation errors, strategy loads

---

## Test Data Management

### Test Fixtures
```csharp
public class FSMActorTestFixture : IDisposable
{
    public SIMA_FSM CreateTestFSM()
    {
        return new SIMA_FSM
        {
            Id = "TEST-FSM-001",
            State = FSMState.Idle
        };
    }
    
    public void Dispose()
    {
        // Cleanup
    }
}
```

### Mock Data
```csharp
private Order CreateMockOrder()
{
    return new Order
    {
        OrderId = "MOCK-ORD-001",
        Quantity = 100,
        Price = 50.0
    };
}
```

---

## Test Execution

### Local
```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~FSMActorTests"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### CI/CD
**Trigger**: Every PR
**Gate**: All tests must pass
**Coverage**: Aim for 80%+

---

## Test Quality Metrics

### Current Baseline (2026-06-08)
- **Test Files**: 1 (`FSMActorTests.cs`)
- **Test Methods**: ~10 (FSM/Actor Enqueue model)
- **Coverage**: Unknown (coverage integration pending)
- **Gap**: 45 methods with CYC > 20 have no tests

### Target (After EPIC-CCN-2 through EPIC-CCN-14)
- **Test Files**: 15+ (one per epic)
- **Test Methods**: 200+ (all extracted methods)
- **Coverage**: 80%+
- **Gap**: Zero untested extracted methods

---

## Common Pitfalls

### ❌ Testing Implementation Details
**Problem**: Tests break when refactoring
**Solution**: Test behavior, not implementation

### ❌ No Negative Tests
**Problem**: Edge cases not covered
**Solution**: Test null, empty, invalid inputs

### ❌ Flaky Tests
**Problem**: Tests pass/fail randomly
**Solution**: Avoid timing dependencies, use deterministic data

---

## Test-Driven Refactoring (TDR)

### For EPIC-CCN-2 through EPIC-CCN-14

**Phase 1: Characterization Tests**
1. Write tests for current behavior (before extraction)
2. Verify tests pass
3. Commit: "Add characterization tests for [method]"

**Phase 2: Extract Method**
1. Extract method with CYC ≤ 8
2. Run tests (should still pass)
3. Commit: "Extract [method] from [parent]"

**Phase 3: Unit Tests**
1. Write unit tests for extracted method
2. Verify tests pass
3. Commit: "Add unit tests for [extracted method]"

**Phase 4: Integration Tests**
1. F5 in NinjaTrader IDE
2. Verify BUILD_TAG
3. Commit: "Verify integration for [extracted method]"

---

## Index

**Parent**: [`../AGENTS.md`](../AGENTS.md) (root)
**Children**: None (leaf node)
**Related**:
- [`../src/AGENTS.md`](../src/AGENTS.md) - Source code rules
- [`../docs/standards/jane-street/TESTING_PATTERNS.md`](../docs/standards/jane-street/TESTING_PATTERNS.md) - Jane Street testing patterns