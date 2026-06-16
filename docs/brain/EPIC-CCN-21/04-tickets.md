# EPIC-CCN-21: Implementation Tickets

**Epic ID**: EPIC-CCN-21  
**Method**: `OnBarUpdate`  
**File**: `src/V12_002.BarUpdate.cs`  
**Current CYC**: 10  
**Target CYC**: ≤8  
**Status**: Phase 4 - Ticket Generation  
**Date**: 2026-06-09

---

## Ticket Execution Order

1. **Ticket 1**: Extract `ProcessPendingTRENDEntry()` (CYC 10→9)
2. **Ticket 2**: Extract `ProcessFFMAConditionCheck()` (CYC 9→8)

**Total CYC Reduction**: 2 points (10→8)

---

## Ticket 1: Extract ProcessPendingTRENDEntry

### Objective
Extract pending TREND entry processing logic into a dedicated helper method to reduce `OnBarUpdate` complexity from CYC 10 to CYC 9.

### Method Signature
```csharp
/// <summary>
/// Processes pending TREND entry if armed.
/// Calculates stop distance, position size, and executes entry.
/// Clears pendingTRENDEntry flag after execution.
/// </summary>
private void ProcessPendingTRENDEntry()
{
    if (!pendingTRENDEntry)
        return;
    
    double trendDist = CalculateTRENDStopDistance();
    int trendContracts = CalculatePositionSize(trendDist);
    ExecuteTRENDEntry(trendContracts);
}
```

### Extraction Details

**Source Lines**: 323-327 (V12_002.BarUpdate.cs)

**Before**:
```csharp
// V8.2 FIX: Process pending TREND entry (deferred from button click)
if (pendingTRENDEntry)
{
    double trendDist = CalculateTRENDStopDistance();
    int trendContracts = CalculatePositionSize(trendDist);
    ExecuteTRENDEntry(trendContracts);
}
```

**After**:
```csharp
// V8.2 FIX: Process pending TREND entry (deferred from button click)
ProcessPendingTRENDEntry();
```

**Call Site Location**: Line 323 (after `MonitorRmaProximity()`)

### TDD Test Specifications

**Test File**: `tests/V12_Performance.Tests/BarUpdate/ProcessPendingTRENDEntryTests.cs`

#### Test 1: Early Return When Not Armed
```csharp
[Fact]
public void ProcessPendingTRENDEntry_NotArmed_ReturnsEarly()
{
    // Arrange
    _strategy.pendingTRENDEntry = false;
    
    // Act
    _strategy.ProcessPendingTRENDEntry();
    
    // Assert
    Assert.False(_strategy.ExecuteTRENDEntryCalled);
}
```

#### Test 2: Calculates Stop Distance When Armed
```csharp
[Fact]
public void ProcessPendingTRENDEntry_Armed_CalculatesStopDistance()
{
    // Arrange
    _strategy.pendingTRENDEntry = true;
    _strategy.MockTrendStopDistance = 10.0;
    
    // Act
    _strategy.ProcessPendingTRENDEntry();
    
    // Assert
    Assert.True(_strategy.CalculateTRENDStopDistanceCalled);
}
```

#### Test 3: Calculates Position Size When Armed
```csharp
[Fact]
public void ProcessPendingTRENDEntry_Armed_CalculatesPositionSize()
{
    // Arrange
    _strategy.pendingTRENDEntry = true;
    _strategy.MockTrendStopDistance = 10.0;
    
    // Act
    _strategy.ProcessPendingTRENDEntry();
    
    // Assert
    Assert.True(_strategy.CalculatePositionSizeCalled);
    Assert.Equal(10.0, _strategy.CalculatePositionSizeInput);
}
```

#### Test 4: Executes TREND Entry When Armed
```csharp
[Fact]
public void ProcessPendingTRENDEntry_Armed_ExecutesEntry()
{
    // Arrange
    _strategy.pendingTRENDEntry = true;
    _strategy.MockTrendStopDistance = 10.0;
    _strategy.MockPositionSize = 5;
    
    // Act
    _strategy.ProcessPendingTRENDEntry();
    
    // Assert
    Assert.True(_strategy.ExecuteTRENDEntryCalled);
    Assert.Equal(5, _strategy.ExecuteTRENDEntryContracts);
}
```

#### Test 5: Clears Pending Flag After Execution
```csharp
[Fact]
public void ProcessPendingTRENDEntry_Armed_ClearsPendingFlag()
{
    // Arrange
    _strategy.pendingTRENDEntry = true;
    _strategy.MockTrendStopDistance = 10.0;
    _strategy.MockPositionSize = 5;
    
    // Act
    _strategy.ProcessPendingTRENDEntry();
    
    // Assert
    Assert.False(_strategy.pendingTRENDEntry);
}
```

#### Test 6: Handles Zero Stop Distance
```csharp
[Fact]
public void ProcessPendingTRENDEntry_ZeroStopDistance_HandlesGracefully()
{
    // Arrange
    _strategy.pendingTRENDEntry = true;
    _strategy.MockTrendStopDistance = 0.0;
    
    // Act
    _strategy.ProcessPendingTRENDEntry();
    
    // Assert
    Assert.True(_strategy.CalculatePositionSizeCalled);
    Assert.Equal(0.0, _strategy.CalculatePositionSizeInput);
}
```

### Extraction Steps

1. **Create Helper Method** (above `OnBarUpdate`)
   - Copy lines 323-327 (exact logic)
   - Wrap in method signature
   - Add XML documentation comment

2. **Replace Call Site** (line 323)
   - Remove if-block (lines 323-327)
   - Add single line: `ProcessPendingTRENDEntry();`
   - Keep comment: `// V8.2 FIX: Process pending TREND entry`

3. **Verify Extraction**
   - Run: `python scripts/complexity_audit.py`
   - Confirm: `OnBarUpdate` CYC reduced to 9
   - Confirm: `ProcessPendingTRENDEntry` CYC = 1

4. **Run Build**
   - Execute: `powershell -File .\scripts\build_readiness.ps1`
   - Verify: Zero compilation errors
   - Verify: Zero new warnings

5. **Run Deploy-Sync**
   - Execute: `powershell -File .\deploy-sync.ps1`
   - Verify: 83 files synchronized
   - Verify: No sync errors

6. **Update BUILD_TAG**
   - Increment: `1111.046` → `1111.047`
   - Format: `1111.047-epic-ccn-21-t1`
   - Location: Line 1 comment

7. **F5 Verification**
   - Press F5 in NinjaTrader IDE
   - Verify: BUILD_TAG `1111.047-epic-ccn-21-t1` appears
   - Verify: Strategy loads without errors
   - Verify: Zero compilation errors

### Verification Criteria

- [ ] Helper method extracted (CYC=1)
- [ ] Call site replaced (single line)
- [ ] `OnBarUpdate` CYC reduced to 9
- [ ] 6 TDD tests written (all failing initially)
- [ ] All tests passing after extraction
- [ ] Build passes (`build_readiness.ps1`)
- [ ] Deploy-sync passes (`deploy-sync.ps1`)
- [ ] BUILD_TAG updated (`1111.047-epic-ccn-21-t1`)
- [ ] F5 verification passed (BUILD_TAG appears)
- [ ] Zero logic drift (exact copy-paste)

---

## Ticket 2: Extract ProcessFFMAConditionCheck

### Objective
Extract FFMA condition check logic into a dedicated helper method to reduce `OnBarUpdate` complexity from CYC 9 to CYC 8 (Jane Street GODMODE threshold).

### Method Signature
```csharp
/// <summary>
/// Checks FFMA conditions if mode is armed and enabled.
/// Triggers FFMA entry logic when conditions are met.
/// </summary>
private void ProcessFFMAConditionCheck()
{
    if (!isFFMAModeArmed || !FFMAEnabled)
        return;
    
    CheckFFMAConditions();
}
```

### Extraction Details

**Source Lines**: 378-381 (V12_002.BarUpdate.cs)

**Before**:
```csharp
// V8.7: Check FFMA conditions when armed
if (isFFMAModeArmed && FFMAEnabled)
{
    CheckFFMAConditions();
}
```

**After**:
```csharp
// V8.7: Check FFMA conditions when armed
ProcessFFMAConditionCheck();
```

**Call Site Location**: Line 378 (after active positions management)

### TDD Test Specifications

**Test File**: `tests/V12_Performance.Tests/BarUpdate/ProcessFFMAConditionCheckTests.cs`

#### Test 1: Early Return When Not Armed
```csharp
[Fact]
public void ProcessFFMAConditionCheck_NotArmed_ReturnsEarly()
{
    // Arrange
    _strategy.isFFMAModeArmed = false;
    _strategy.FFMAEnabled = true;
    
    // Act
    _strategy.ProcessFFMAConditionCheck();
    
    // Assert
    Assert.False(_strategy.CheckFFMAConditionsCalled);
}
```

#### Test 2: Early Return When Not Enabled
```csharp
[Fact]
public void ProcessFFMAConditionCheck_NotEnabled_ReturnsEarly()
{
    // Arrange
    _strategy.isFFMAModeArmed = true;
    _strategy.FFMAEnabled = false;
    
    // Act
    _strategy.ProcessFFMAConditionCheck();
    
    // Assert
    Assert.False(_strategy.CheckFFMAConditionsCalled);
}
```

#### Test 3: Early Return When Neither Armed Nor Enabled
```csharp
[Fact]
public void ProcessFFMAConditionCheck_NeitherArmedNorEnabled_ReturnsEarly()
{
    // Arrange
    _strategy.isFFMAModeArmed = false;
    _strategy.FFMAEnabled = false;
    
    // Act
    _strategy.ProcessFFMAConditionCheck();
    
    // Assert
    Assert.False(_strategy.CheckFFMAConditionsCalled);
}
```

#### Test 4: Checks Conditions When Armed And Enabled
```csharp
[Fact]
public void ProcessFFMAConditionCheck_ArmedAndEnabled_ChecksConditions()
{
    // Arrange
    _strategy.isFFMAModeArmed = true;
    _strategy.FFMAEnabled = true;
    
    // Act
    _strategy.ProcessFFMAConditionCheck();
    
    // Assert
    Assert.True(_strategy.CheckFFMAConditionsCalled);
}
```

#### Test 5: Respects Armed Flag Priority
```csharp
[Theory]
[InlineData(true, true, true)]   // Armed + Enabled = Check
[InlineData(true, false, false)] // Armed + Disabled = Skip
[InlineData(false, true, false)] // Disarmed + Enabled = Skip
[InlineData(false, false, false)] // Disarmed + Disabled = Skip
public void ProcessFFMAConditionCheck_RespectsFlagPriority(
    bool armed, bool enabled, bool shouldCheck)
{
    // Arrange
    _strategy.isFFMAModeArmed = armed;
    _strategy.FFMAEnabled = enabled;
    
    // Act
    _strategy.ProcessFFMAConditionCheck();
    
    // Assert
    Assert.Equal(shouldCheck, _strategy.CheckFFMAConditionsCalled);
}
```

#### Test 6: Does Not Mutate State
```csharp
[Fact]
public void ProcessFFMAConditionCheck_DoesNotMutateState()
{
    // Arrange
    _strategy.isFFMAModeArmed = true;
    _strategy.FFMAEnabled = true;
    bool armedBefore = _strategy.isFFMAModeArmed;
    bool enabledBefore = _strategy.FFMAEnabled;
    
    // Act
    _strategy.ProcessFFMAConditionCheck();
    
    // Assert
    Assert.Equal(armedBefore, _strategy.isFFMAModeArmed);
    Assert.Equal(enabledBefore, _strategy.FFMAEnabled);
}
```

### Extraction Steps

1. **Create Helper Method** (above `OnBarUpdate`)
   - Copy lines 378-381 (exact logic)
   - Wrap in method signature
   - Add XML documentation comment

2. **Replace Call Site** (line 378)
   - Remove if-block (lines 378-381)
   - Add single line: `ProcessFFMAConditionCheck();`
   - Keep comment: `// V8.7: Check FFMA conditions when armed`

3. **Verify Extraction**
   - Run: `python scripts/complexity_audit.py`
   - Confirm: `OnBarUpdate` CYC reduced to 8 ✅ GODMODE
   - Confirm: `ProcessFFMAConditionCheck` CYC = 2

4. **Run Build**
   - Execute: `powershell -File .\scripts\build_readiness.ps1`
   - Verify: Zero compilation errors
   - Verify: Zero new warnings

5. **Run Deploy-Sync**
   - Execute: `powershell -File .\deploy-sync.ps1`
   - Verify: 83 files synchronized
   - Verify: No sync errors

6. **Update BUILD_TAG**
   - Increment: `1111.047` → `1111.048`
   - Format: `1111.048-epic-ccn-21-t2`
   - Location: Line 1 comment

7. **F5 Verification**
   - Press F5 in NinjaTrader IDE
   - Verify: BUILD_TAG `1111.048-epic-ccn-21-t2` appears
   - Verify: Strategy loads without errors
   - Verify: Zero compilation errors

### Verification Criteria

- [ ] Helper method extracted (CYC=2)
- [ ] Call site replaced (single line)
- [ ] `OnBarUpdate` CYC reduced to 8 ✅ GODMODE
- [ ] 6 TDD tests written (all failing initially)
- [ ] All tests passing after extraction
- [ ] Build passes (`build_readiness.ps1`)
- [ ] Deploy-sync passes (`deploy-sync.ps1`)
- [ ] BUILD_TAG updated (`1111.048-epic-ccn-21-t2`)
- [ ] F5 verification passed (BUILD_TAG appears)
- [ ] Zero logic drift (exact copy-paste)

---

## Epic Completion Criteria

### All Tickets Complete
- [ ] Ticket 1 verified (CYC 10→9)
- [ ] Ticket 2 verified (CYC 9→8)
- [ ] Final CYC = 8 (Jane Street GODMODE)
- [ ] All TDD tests passing (12 tests total)
- [ ] All F5 verifications passed
- [ ] Zero compilation errors
- [ ] Zero logic drift

### Manifest Update
- [ ] Update `docs/brain/EPIC-CCN-21/manifest.json`
- [ ] Set status: `"completed"`
- [ ] Set final_cyc: `8`
- [ ] Set completion_date: `"2026-06-09"`

### Roadmap Update
- [ ] Update `epic_roadmap.json`
- [ ] Set EPIC-CCN-21 status: `"complete"`
- [ ] Set final_cyc: `8`
- [ ] Set completion_date: `"2026-06-09"`

---

## Next Phase

**Phase 5**: Ticket Execution
- Switch to `v12-engineer` mode
- Execute Ticket 1 (TDD workflow)
- 🛑 STOP: Wait for F5 verification
- Execute Ticket 2 (TDD workflow)
- 🛑 STOP: Wait for F5 verification

**Orchestrator**: Ticket generation complete. Ready to proceed to Phase 5 (Ticket Execution)?

**User Action Required**: Approve ticket specifications or provide feedback.
