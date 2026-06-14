# Phase 3: Implementation Plan - EPIC-CCN-125

## Epic Metadata
- **Epic ID**: EPIC-CCN-125
- **Target Method**: `EnterORPosition`
- **File**: `src/V12_002.Entries.OR.cs`
- **Current Complexity**: 11 (CYC)
- **Target Complexity**: ≤ 8 (Jane Street HFT standard)
- **Phase**: 3 (Implementation Plan)
- **Estimated Duration**: 45-60 minutes

## Implementation Overview

This phase executes the extraction strategy defined in Phase 2, reducing `EnterORPosition` complexity from CYC=11 to CYC≤8 through 4 targeted method extractions.

### Execution Strategy
- **Incremental**: Extract one method at a time
- **Verify Each Step**: Build + complexity audit after each extraction
- **Checkpoint Safety**: Bob CLI auto-checkpointing enabled
- **Rollback Ready**: Git branch + restore points

## Pre-Implementation Checklist

### Environment Setup
- [ ] Git branch created: `feature/epic-ccn-125-enteror-extraction`
- [ ] Working directory clean (no uncommitted changes)
- [ ] Bob CLI checkpointing verified (`.bob/settings.json`)
- [ ] Baseline complexity audit run and saved

### Baseline Metrics
```powershell
# Run baseline audit
python scripts/complexity_audit.py > docs/brain/EPIC-CCN-125/baseline_audit.txt

# Expected baseline
# EnterORPosition: CYC=11, LOC=166
```

### Tool Verification
```powershell
# Verify build toolchain
dotnet --version  # Expected: 6.0+

# Verify complexity audit
python --version  # Expected: 3.8+
python scripts/complexity_audit.py --help

# Verify ASCII checker
python check_ascii.py --help

# Verify deploy-sync
powershell -File .\deploy-sync.ps1 -WhatIf
```

## Implementation Steps

### Step 1: Extract ValidateOREntryPreconditions()

**Objective**: Consolidate entry guard checks (CYC reduction: -3)

#### 1.1 Add New Method
**Location**: After `CalculateORStopDistance()` (line 358)

**Code**:
```csharp
private bool ValidateOREntryPreconditions(int contracts)
{
    // V12.Phase7 [C-09]: Compliance enforcement gate
    if (!IsOrderAllowed())
        return false;
    
    // V12.Phase6 [FLATTEN-GUARD]: Prevent order submission during active flatten
    if (isFlattenRunning)
        return false;
    
    if (contracts <= 0)
    {
        Print(string.Format("[OR] EnterORPosition received invalid contracts={0}. Aborting entry.", contracts));
        return false;
    }
    
    return true;
}
```

#### 1.2 Refactor EnterORPosition()
**Replace lines 127-137** with:
```csharp
// Step 1: Validate preconditions (early exit)
if (!ValidateOREntryPreconditions(contracts))
    return;
```

#### 1.3 Verification
```powershell
# Build check
dotnet build src/V12_002.csproj

# Complexity check
python scripts/complexity_audit.py | grep -A 2 "EnterORPosition\|ValidateOREntryPreconditions"

# Expected:
# ValidateOREntryPreconditions: CYC=3
# EnterORPosition: CYC=8 (reduced from 11)
```

#### 1.4 Checkpoint
```bash
# Bob CLI auto-checkpoint (if using Bob)
# Manual git checkpoint
git add src/V12_002.Entries.OR.cs
git commit -m "EPIC-CCN-125: Extract ValidateOREntryPreconditions (CYC -3)"
```

---

### Step 2: Extract ValidateOREntryPrice()

**Objective**: Isolate price validation logic (CYC reduction: -2)

#### 2.1 Add New Method
**Location**: After `ValidateOREntryPreconditions()`

**Code**:
```csharp
private bool ValidateOREntryPrice(MarketPosition direction, double entryPrice, double currentPrice)
{
    // v5.13 FIX: Validate entry price before submitting StopMarket order
    // For LONG: entry must be ABOVE current price (breakout up)
    // For SHORT: entry must be BELOW current price (breakout down)
    
    if (direction == MarketPosition.Long && entryPrice <= currentPrice)
    {
        Print(
            string.Format(
                "OR ENTRY BLOCKED: Long entry {0:F2} already below market {1:F2} - too late for breakout",
                entryPrice,
                currentPrice
            )
        );
        return false;
    }
    
    if (direction == MarketPosition.Short && entryPrice >= currentPrice)
    {
        Print(
            string.Format(
                "OR ENTRY BLOCKED: Short entry {0:F2} already above market {1:F2} - too late for breakout",
                entryPrice,
                currentPrice
            )
        );
        return false;
    }
    
    return true;
}
```

#### 2.2 Refactor EnterORPosition()
**Replace lines 145-167** with:
```csharp
// Step 2: Validate entry price against current market
double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
if (!ValidateOREntryPrice(direction, entryPrice, currentPrice))
    return;
```

#### 2.3 Verification
```powershell
# Build check
dotnet build src/V12_002.csproj

# Complexity check
python scripts/complexity_audit.py | grep -A 2 "EnterORPosition\|ValidateOREntryPrice"

# Expected:
# ValidateOREntryPrice: CYC=2
# EnterORPosition: CYC=6 (reduced from 8)
```

#### 2.4 Checkpoint
```bash
git add src/V12_002.Entries.OR.cs
git commit -m "EPIC-CCN-125: Extract ValidateOREntryPrice (CYC -2)"
```

---

### Step 3: Extract CreateORPositionInfo()

**Objective**: Encapsulate PositionInfo creation (CYC reduction: -1)

#### 3.1 Add New Method
**Location**: After `ValidateOREntryPrice()`

**Code**:
```csharp
private PositionInfo CreateORPositionInfo(
    MarketPosition direction,
    double entryPrice,
    double stopPrice,
    int contracts,
    int t1Qty, int t2Qty, int t3Qty, int t4Qty, int t5Qty,
    string entryName)
{
    // Universal Ladder: T(n)Type dropdown drives all target pricing
    double target1Price = CalculateTargetPrice(direction, entryPrice, 1);
    double target2Price = CalculateTargetPrice(direction, entryPrice, 2);
    double target3Price = CalculateTargetPrice(direction, entryPrice, 3);
    double target4Price = CalculateTargetPrice(direction, entryPrice, 4);
    double target5Price = CalculateTargetPrice(direction, entryPrice, 5);
    
    PositionInfo pos = new PositionInfo
    {
        SignalName = entryName,
        Direction = direction,
        TotalContracts = contracts,
        T1Contracts = t1Qty,
        T2Contracts = t2Qty,
        T3Contracts = t3Qty,
        T4Contracts = t4Qty,
        T5Contracts = t5Qty,
        RemainingContracts = contracts,
        EntryPrice = entryPrice,
        InitialStopPrice = stopPrice,
        CurrentStopPrice = stopPrice,
        Target1Price = target1Price,
        Target2Price = target2Price,
        Target3Price = target3Price,
        Target4Price = target4Price,
        Target5Price = target5Price,
        EntryFilled = false,
        T1Filled = false,
        T2Filled = false,
        T3Filled = false,
        BracketSubmitted = false,
        ExtremePriceSinceEntry = entryPrice,
        CurrentTrailLevel = 0,
        EntryOrderType = OrderType.StopMarket,
        IsRMATrade = false,
        OcoGroupId = "V12_" + GetStableHash(entryName),
    };
    
    ApplyTargetLadderGuard(pos);
    return pos;
}
```

#### 3.2 Refactor EnterORPosition()
**Replace lines 194-230** with:
```csharp
// Step 4: Create position tracking object
string signalName = direction == MarketPosition.Long ? "ORLong" : "ORShort";
string timestamp = DateTime.Now.ToString("HHmmssffff");
string entryName = signalName + "_" + timestamp;

PositionInfo pos = CreateORPositionInfo(
    direction, entryPrice, stopPrice, contracts,
    t1Qty, t2Qty, t3Qty, t4Qty, t5Qty, entryName
);
```

#### 3.3 Verification
```powershell
# Build check
dotnet build src/V12_002.csproj

# Complexity check
python scripts/complexity_audit.py | grep -A 2 "EnterORPosition\|CreateORPositionInfo"

# Expected:
# CreateORPositionInfo: CYC=1
# EnterORPosition: CYC=5 (reduced from 6)
```

#### 3.4 Checkpoint
```bash
git add src/V12_002.Entries.OR.cs
git commit -m "EPIC-CCN-125: Extract CreateORPositionInfo (CYC -1)"
```

---

### Step 4: Extract SubmitOREntryOrder()

**Objective**: Encapsulate order submission + rollback (CYC reduction: -1)

#### 4.1 Add New Method
**Location**: After `CreateORPositionInfo()`

**Code**:
```csharp
private Order SubmitOREntryOrder(
    MarketPosition direction,
    double entryPrice,
    int contracts,
    string entryName,
    int masterDeltaOR)
{
    // Submit entry order as stop market (breakout entry)
    Order entryOrder =
        direction == MarketPosition.Long
            ? SubmitOrderUnmanaged(
                0,
                OrderAction.Buy,
                OrderType.StopMarket,
                contracts,
                0,
                entryPrice,
                "",
                entryName
            )
            : SubmitOrderUnmanaged(
                0,
                OrderAction.SellShort,
                OrderType.StopMarket,
                contracts,
                0,
                entryPrice,
                "",
                entryName
            );
    
    // A1-1/A2-1: Null-abort rollback (Build 960 audit fix)
    if (entryOrder == null)
    {
        // Build 1102Y-V3 [MS-03 ROLLBACK]: Submit failed -- undo Order Ledger reservation
        var _aek966 = ExpKey(Account.Name);
        var _aed966 = (-masterDeltaOR);
        Enqueue(ctx => ctx.AddExpectedPositionDeltaLocked(_aek966, _aed966));
        
        Print(
            "[ENTRY_ABORT] OR SubmitOrderUnmanaged returned NULL for "
                + entryName
                + " -- Master expected rolled back. Fleet dispatch aborted."
        );
        return null;
    }
    
    return entryOrder;
}
```

#### 4.2 Refactor EnterORPosition()
**Replace lines 245-283** with:
```csharp
// Step 7: Submit entry order (with rollback on failure)
Order entryOrder = SubmitOREntryOrder(direction, entryPrice, contracts, entryName, masterDeltaOR);
if (entryOrder == null)
    return; // Rollback already handled in SubmitOREntryOrder
```

#### 4.3 Verification
```powershell
# Build check
dotnet build src/V12_002.csproj

# Complexity check
python scripts/complexity_audit.py | grep -A 2 "EnterORPosition\|SubmitOREntryOrder"

# Expected:
# SubmitOREntryOrder: CYC=2
# EnterORPosition: CYC=7 (final target ≤8 achieved)
```

#### 4.4 Checkpoint
```bash
git add src/V12_002.Entries.OR.cs
git commit -m "EPIC-CCN-125: Extract SubmitOREntryOrder (CYC -1, final)"
```

---

### Step 5: Final Refactoring & Cleanup

**Objective**: Optimize remaining method structure for readability

#### 5.1 Review EnterORPosition() Structure
Ensure the refactored method follows this clean flow:
1. Validate preconditions → early return
2. Validate entry price → early return
3. Calculate target distribution
4. Create position info
5. Sync with panel
6. Register expected position
7. Submit order → early return on failure
8. Update state (activePositions, entryOrders)
9. Log confirmation
10. Dispatch to SIMA (if enabled)

#### 5.2 Add Step Comments
Ensure each major section has a clear step comment:
```csharp
// Step 1: Validate preconditions (early exit)
// Step 2: Validate entry price against current market
// Step 3: Calculate target distribution
// Step 4: Create position tracking object
// Step 5: Sync with panel clients
// Step 6: Register expected position delta
// Step 7: Submit entry order (with rollback on failure)
// Step 8: Update position tracking state
// Step 9: Log entry confirmation
// Step 10: Dispatch to SIMA fleet
```

#### 5.3 Verification
```powershell
# Full build
dotnet build src/V12_002.csproj

# Final complexity audit
python scripts/complexity_audit.py > docs/brain/EPIC-CCN-125/final_audit.txt

# ASCII compliance
python check_ascii.py src/V12_002.Entries.OR.cs

# Expected: Zero non-ASCII characters
```

#### 5.4 Final Checkpoint
```bash
git add src/V12_002.Entries.OR.cs
git commit -m "EPIC-CCN-125: Final cleanup and step comments"
```

---

## Post-Implementation Verification

### Quality Gates

#### 1. Build Verification
```powershell
dotnet build src/V12_002.csproj
```
**Success Criteria**: Zero errors, zero warnings

#### 2. Complexity Audit
```powershell
python scripts/complexity_audit.py | grep -A 5 "EnterORPosition"
```
**Success Criteria**:
- `EnterORPosition`: CYC ≤ 8 ✅
- `ValidateOREntryPreconditions`: CYC = 3
- `ValidateOREntryPrice`: CYC = 2
- `CreateORPositionInfo`: CYC = 1
- `SubmitOREntryOrder`: CYC = 2

#### 3. ASCII Compliance
```powershell
python check_ascii.py src/V12_002.Entries.OR.cs
```
**Success Criteria**: Zero non-ASCII characters

#### 4. CSharpier Formatting
```powershell
dotnet csharpier check src/
```
**Success Criteria**: Zero formatting issues

#### 5. Hard-Link Sync
```powershell
powershell -File .\deploy-sync.ps1
```
**Success Criteria**: 
- Sync succeeds
- Diff < 10,000 characters
- Zero compilation errors in NinjaTrader directory

#### 6. Pre-Push Validation (Fast Mode)
```powershell
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```
**Success Criteria**: All 9 fast checks pass

### Integration Testing

#### 7. NinjaTrader F5 Test
1. Open NinjaTrader
2. Press F5 to compile strategy
3. Verify zero compilation errors
4. Check Output window for warnings

**Success Criteria**: Clean compilation

#### 8. Paper Trading Test
1. Enable strategy on paper trading account
2. Wait for OR completion
3. Execute Long or Short entry
4. Verify:
   - Entry order submitted correctly
   - Stop order placed
   - Target orders created
   - Position tracking updated
   - SIMA dispatch (if enabled)

**Success Criteria**: All entry logic executes correctly

### Metrics Comparison

#### Before Extraction
- **EnterORPosition**: CYC=11, LOC=166
- **Total Methods**: 1
- **Total CYC**: 11

#### After Extraction
- **EnterORPosition**: CYC≤8, LOC~80
- **ValidateOREntryPreconditions**: CYC=3, LOC~15
- **ValidateOREntryPrice**: CYC=2, LOC~25
- **CreateORPositionInfo**: CYC=1, LOC~50
- **SubmitOREntryOrder**: CYC=2, LOC~40
- **Total Methods**: 5
- **Total CYC**: ~16 (distributed across 5 methods)

**Key Improvements**:
- ✅ Main method CYC reduced by 27-36% (11→7-8)
- ✅ Jane Street threshold achieved (CYC ≤ 8)
- ✅ Cognitive load reduced (single-purpose methods)
- ✅ Testability improved (isolated validation logic)
- ✅ Maintainability improved (clear separation of concerns)

## Rollback Procedures

### Bob CLI Restore
```bash
# List available restore points
/restore

# Restore to specific point
/restore <restore_point_id>
```

### Git Rollback
```bash
# Soft reset (keep changes)
git reset --soft HEAD~4

# Hard reset (discard changes)
git reset --hard HEAD~4

# Restore specific file
git checkout HEAD~4 -- src/V12_002.Entries.OR.cs
```

### Manual Rollback
1. Copy baseline file from `docs/brain/EPIC-CCN-125/baseline_V12_002.Entries.OR.cs`
2. Replace current file
3. Run `deploy-sync.ps1`

## Risk Mitigation

### Common Issues & Solutions

#### Issue 1: Build Errors After Extraction
**Symptom**: Compilation errors in extracted methods
**Solution**: 
- Check method signatures match usage
- Verify all parameters passed correctly
- Ensure return types match expectations

#### Issue 2: Complexity Not Reduced
**Symptom**: CYC still >8 after extraction
**Solution**:
- Review ternary operators in main method
- Check for hidden conditionals
- Consider extracting additional logic

#### Issue 3: Behavioral Changes
**Symptom**: Entry logic behaves differently
**Solution**:
- Compare execution logs before/after
- Verify early-return conditions preserved
- Check parameter passing (by-value vs by-ref)

#### Issue 4: Hard-Link Sync Fails
**Symptom**: deploy-sync.ps1 reports errors
**Solution**:
- Check file permissions
- Verify NinjaTrader directory path
- Run as administrator if needed

## Success Criteria Checklist

### Primary Goals
- [ ] **Complexity Reduction**: CYC reduced from 11 to ≤ 8
- [ ] **Jane Street Alignment**: Target CYC ≤ 8 achieved
- [ ] **Cognitive Simplicity**: Each method has single, clear purpose
- [ ] **Correctness Preservation**: Zero behavioral changes
- [ ] **Lock-Free Semantics**: No locks introduced

### Verification Checklist
- [ ] Build succeeds (dotnet build)
- [ ] Complexity audit confirms CYC ≤ 8
- [ ] ASCII compliance verified
- [ ] CSharpier formatting passes
- [ ] Hard-link sync succeeds
- [ ] Pre-push validation passes (fast mode)
- [ ] F5 test in NinjaTrader succeeds
- [ ] OR entry executes correctly in paper trading

### Documentation Checklist
- [ ] Baseline audit saved
- [ ] Final audit saved
- [ ] Metrics comparison documented
- [ ] Acceptance report generated
- [ ] Manifest updated

## Phase 3 Completion

Upon successful completion of all steps and verification:

1. **Update Manifest**:
```json
{
  "epic_id": "EPIC-CCN-125",
  "method": "EnterORPosition",
  "file": "src/V12_002.Entries.OR.cs",
  "complexity_before": 11,
  "complexity_after": 7,
  "target_complexity": 8,
  "status": "completed",
  "phases": {
    "0": { "status": "completed" },
    "1": { "status": "completed" },
    "2": { "status": "completed" },
    "3": { "status": "completed" }
  }
}
```

2. **Generate Acceptance Report**: `04-acceptance.md`

3. **Update task.md**: Add EPIC-CCN-125 to completed epics

4. **Create PR**: `feature/epic-ccn-125-enteror-extraction` → `main`

---

*Generated: 2026-06-13*
*Epic: EPIC-CCN-125*
*Phase: 3 (Implementation Plan)*
*Target: EnterORPosition (CYC 11 → ≤ 8)*
*Estimated Duration: 45-60 minutes*
