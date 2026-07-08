# tests/ - Testing Rules

**Last Updated**: 2026-07-02
**Scope**: Unit tests, integration tests, and TDD workflow for Wave 7 extractions

---

## Test Structure

```
tests/
└── V12_Performance.Tests/
    └── Core/
        └── FSMActorTests.cs
xunit-tests/
├── W7-047/
├── W7-147/
├── W7-149/
└── W7-150/
```

**Framework**: xUnit ONLY -- `[Fact]`, `[Theory]`, `Assert.*`
**BANNED**: NUnit, MSTest -- never use `[Test]`, `[TestCase]`, `[TestMethod]`
**Reference**: `docs/intel/jane-street/testing-strategies.md`

---

## Testing Standards

### Unit Test Pattern (Arrange-Act-Assert)
```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var sut = CreateSystemUnderTest();

    // Act
    var result = sut.ExtractedMethod(input);

    // Assert
    Assert.Equal(expectedValue, result);
}
```

### Test Naming Convention
```
MethodName_Scenario_ExpectedBehavior
```
Examples:
- `LinkTargetOrderToFSM_NullOrder_ReturnsEarly`
- `ValidateOrderBounds_ExceedsMax_ReturnsFalse`
- `ProcessEntrySignal_NoActiveBar_Skips`

---

## TDD Workflow for Wave 7 Extractions

### Step 1 -- Write test first (before extraction)
```csharp
[Fact]
public void ExtractedMethod_TypicalInput_ProducesExpectedResult()
{
    var result = _sut.ExtractedMethod(CreateTypicalInput());
    Assert.Equal(expected, result);
}
```

### Step 2 -- Extract method with CYC <= 8
### Step 3 -- Run tests (must pass)
```bash
dotnet test
```
### Step 4 -- Commit both extraction and test together

---

## Test Location Convention

Wave 7 xUnit tests live in `xunit-tests/W7-NNN/` (one directory per epic).
Legacy tests in `tests/V12_Performance.Tests/Core/`.

---

## Integration Tests

- **Method**: F5 in NinjaTrader IDE
- **Verification**: BUILD_TAG appears in output
- **Success**: No compilation errors, strategy loads

---

## Common Pitfalls

### Wrong test framework
ALWAYS use xUnit. If you see `[TestFixture]`, `[Test]`, or `[TestMethod]` -- that is NUnit
or MSTest. Delete and rewrite with xUnit `[Fact]`.

### Testing implementation details
Test observable behavior, not private internals. Tests should survive refactoring.

### No negative tests
Always add at least one test for null/empty/invalid input.

---

## Index

**Parent**: [`../AGENTS.md`](../AGENTS.md) (root)
**Children**: None (leaf node)
**Related**:
- [`../src/AGENTS.md`](../src/AGENTS.md) -- Source code rules
- [`../docs/intel/jane-street/testing-strategies.md`](../docs/intel/jane-street/testing-strategies.md)
