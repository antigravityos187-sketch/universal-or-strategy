# Phase 2: Architecture Planning - EPIC-CCN-125

## Epic Metadata
- **Epic ID**: EPIC-CCN-125
- **Target Method**: `EnterORPosition`
- **File**: `src/V12_002.Entries.OR.cs`
- **Current Complexity**: 11 (CYC)
- **Current LOC**: 166 (lines 125-347)
- **Target Complexity**: ≤ 8 (Jane Street HFT standard)
- **Phase**: 2 (Architecture Planning)

## Method Analysis

### Current Structure (Lines 125-347)

```
EnterORPosition(MarketPosition direction, double entryPrice, double stopPrice, int contracts)
├─ Lines 127-137: Entry Guards (CYC +3)
│  ├─ IsOrderAllowed() check
│  ├─ isFlattenRunning check
│  └─ contracts validation
├─ Lines 139-167: Price Validation (CYC +2)
│  ├─ Current price retrieval
│  ├─ Long entry validation (entryPrice > currentPrice)
│  └─ Short entry validation (entryPrice < currentPrice)
├─ Lines 170-187: Target Distribution (CYC +1)
│  ├─ GetTargetDistribution() call
│  └─ Distribution logging
├─ Lines 189-230: Position Setup (CYC +1)
│  ├─ Signal name generation
│  ├─ Target price calculations (5 targets)
│  ├─ PositionInfo struct initialization
│  └─ ApplyTargetLadderGuard()
├─ Lines 233-242: Sync & Ledger (CYC +1)
│  ├─ Panel sync message
│  └─ Expected position delta registration
├─ Lines 245-266: Order Submission (CYC +1)
│  ├─ Direction-based order creation
│  └─ SubmitOrderUnmanaged() call
├─ Lines 269-283: Rollback Logic (CYC +1)
│  ├─ Null order check
│  └─ Ledger rollback
├─ Lines 284-299: State Updates (CYC +0)
│  ├─ activePositions update
│  └─ entryOrders update
├─ Lines 301-325: Logging (CYC +0)
│  └─ Entry confirmation prints
└─ Lines 328-341: SIMA Dispatch (CYC +1)
   └─ ExecuteSmartDispatchEntry() call

Total CYC: 11 (1 base + 10 decision points)
```

### Complexity Breakdown

| Section | Lines | CYC | Extraction Priority |
|---------|-------|-----|---------------------|
| Entry Guards | 127-137 | +3 | HIGH (early exit logic) |
| Price Validation | 145-167 | +2 | HIGH (complex conditionals) |
| Target Distribution | 170-187 | +1 | MEDIUM (already delegated) |
| Position Setup | 189-230 | +1 | LOW (data initialization) |
| Sync & Ledger | 233-242 | +1 | LOW (single responsibility) |
| Order Submission | 245-266 | +1 | MEDIUM (direction branching) |
| Rollback Logic | 269-283 | +1 | MEDIUM (error handling) |
| State Updates | 284-299 | +0 | LOW (simple assignments) |
| SIMA Dispatch | 328-341 | +1 | LOW (single conditional) |

## Extraction Strategy

### Target: Reduce CYC from 11 to ≤ 8 (3-point reduction minimum)

### Extraction Plan (4 Methods)

#### 1. ValidateOREntryPreconditions() - CYC Reduction: -3

**Purpose**: Consolidate all entry guard checks into single validation method.

**Signature**:
```csharp
private bool ValidateOREntryPreconditions(int contracts)
```

**Extracted Lines**: 127-137
- IsOrderAllowed() check (CYC +1)
- isFlattenRunning check (CYC +1)
- contracts <= 0 validation (CYC +1)

**Return**: `true` if all preconditions pass, `false` otherwise

**Rationale**: 
- Early-exit guards are perfect extraction candidates
- Single responsibility: "Can we enter this position?"
- Reduces main method CYC by 3 points
- No data flow complexity (simple boolean checks)

#### 2. ValidateOREntryPrice() - CYC Reduction: -2

**Purpose**: Validate entry price against current market (breakout logic).

**Signature**:
```csharp
private bool ValidateOREntryPrice(MarketPosition direction, double entryPrice, double currentPrice)
```

**Extracted Lines**: 145-167
- Current price retrieval logic (lines 145)
- Long entry validation (lines 146-156, CYC +1)
- Short entry validation (lines 157-167, CYC +1)

**Return**: `true` if entry price is valid for direction, `false` otherwise

**Rationale**:
- Complex conditional logic with direction-specific rules
- Self-contained validation (no side effects)
- Clear single purpose: "Is this entry price valid?"
- Reduces main method CYC by 2 points

#### 3. CreateORPositionInfo() - CYC Reduction: -1

**Purpose**: Build PositionInfo struct with all required fields.

**Signature**:
```csharp
private PositionInfo CreateORPositionInfo(
    MarketPosition direction,
    double entryPrice,
    double stopPrice,
    int contracts,
    int t1Qty, int t2Qty, int t3Qty, int t4Qty, int t5Qty,
    string entryName)
```

**Extracted Lines**: 194-230
- Target price calculations (lines 194-198)
- PositionInfo initialization (lines 200-229)
- ApplyTargetLadderGuard() call (line 230)

**Return**: Fully initialized `PositionInfo` struct

**Rationale**:
- Data structure initialization is cognitively separate from control flow
- Reduces visual complexity of main method
- Single responsibility: "Build position tracking object"
- Reduces main method CYC by 1 point (ternary in OcoGroupId)

#### 4. SubmitOREntryOrder() - CYC Reduction: -1

**Purpose**: Submit direction-specific entry order with rollback on failure.

**Signature**:
```csharp
private Order SubmitOREntryOrder(
    MarketPosition direction,
    double entryPrice,
    int contracts,
    string entryName,
    int masterDeltaOR)
```

**Extracted Lines**: 245-283
- Direction-based order submission (lines 245-266, CYC +1)
- Null order check and rollback (lines 269-283)

**Return**: Submitted `Order` object or `null` on failure

**Rationale**:
- Encapsulates order submission + error handling
- Rollback logic is tightly coupled to submission
- Single responsibility: "Submit order and handle failure"
- Reduces main method CYC by 1 point (direction conditional)

### Post-Extraction Structure

```
EnterORPosition(MarketPosition direction, double entryPrice, double stopPrice, int contracts)
├─ ValidateOREntryPreconditions(contracts) -> early return if false
├─ ValidateOREntryPrice(direction, entryPrice, currentPrice) -> early return if false
├─ GetTargetDistribution() -> existing helper
├─ CreateORPositionInfo() -> new helper
├─ SendResponseToRemote() -> existing helper
├─ Register expected position delta (Enqueue)
├─ SubmitOREntryOrder() -> new helper (includes rollback)
├─ Update activePositions (Enqueue)
├─ Update entryOrders (Enqueue)
├─ Print confirmation logs
└─ ExecuteSmartDispatchEntry() -> existing helper (if EnableSIMA)

Estimated CYC: 6-7 (well below target of 8)
```

## Data Flow Analysis

### Input Parameters
- `direction`: MarketPosition (Long/Short)
- `entryPrice`: double (breakout trigger price)
- `stopPrice`: double (initial stop loss)
- `contracts`: int (position size)

### Shared State Access (Read)
- `lastKnownPrice`: double (current market price)
- `Close[0]`: double (fallback price)
- `EnableSIMA`: bool (fleet dispatch flag)

### Shared State Mutations (Write via Enqueue)
- `ExpectedPositionDelta`: Account ledger update
- `activePositions`: Position tracking dictionary
- `entryOrders`: Order tracking dictionary

### External Dependencies
- `IsOrderAllowed()`: Compliance gate
- `GetTargetDistribution()`: Position sizing
- `CalculateTargetPrice()`: Target ladder
- `ApplyTargetLadderGuard()`: Target validation
- `SendResponseToRemote()`: Panel sync
- `SubmitOrderUnmanaged()`: Broker API
- `ExecuteSmartDispatchEntry()`: SIMA dispatch

## Implementation Sequence

### Step 1: Extract ValidateOREntryPreconditions()
**File**: `src/V12_002.Entries.OR.cs`
**Location**: Add after `CalculateORStopDistance()` (line 358)

**Implementation**:
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

**Verification**:
- Build succeeds
- CYC of `ValidateOREntryPreconditions` = 3
- CYC of `EnterORPosition` reduced by 3

### Step 2: Extract ValidateOREntryPrice()
**File**: `src/V12_002.Entries.OR.cs`
**Location**: Add after `ValidateOREntryPreconditions()`

**Implementation**:
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

**Verification**:
- Build succeeds
- CYC of `ValidateOREntryPrice` = 2
- CYC of `EnterORPosition` reduced by 2 (cumulative: -5)

### Step 3: Extract CreateORPositionInfo()
**File**: `src/V12_002.Entries.OR.cs`
**Location**: Add after `ValidateOREntryPrice()`

**Implementation**:
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

**Verification**:
- Build succeeds
- CYC of `CreateORPositionInfo` = 1
- CYC of `EnterORPosition` reduced by 1 (cumulative: -6)

### Step 4: Extract SubmitOREntryOrder()
**File**: `src/V12_002.Entries.OR.cs`
**Location**: Add after `CreateORPositionInfo()`

**Implementation**:
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

**Verification**:
- Build succeeds
- CYC of `SubmitOREntryOrder` = 2 (direction ternary + null check)
- CYC of `EnterORPosition` reduced by 1 (cumulative: -7)

### Step 5: Refactor EnterORPosition() to Use Extracted Methods

**Updated Method** (lines 125-347 → ~80 lines):
```csharp
private void EnterORPosition(MarketPosition direction, double entryPrice, double stopPrice, int contracts)
{
    // Step 1: Validate preconditions (early exit)
    if (!ValidateOREntryPreconditions(contracts))
        return;
    
    try
    {
        // Step 2: Validate entry price against current market
        double currentPrice = lastKnownPrice > 0 ? lastKnownPrice : Close[0];
        if (!ValidateOREntryPrice(direction, entryPrice, currentPrice))
            return;
        
        // Step 3: Calculate target distribution
        int t1Qty, t2Qty, t3Qty, t4Qty, t5Qty;
        GetTargetDistribution(contracts, out t1Qty, out t2Qty, out t3Qty, out t4Qty, out t5Qty);
        
        Print(
            string.Format(
                "POSITION SIZE: {0} contracts -> T1:{1} T2:{2} T3:{3} T4:{4} T5:{5}",
                contracts, t1Qty, t2Qty, t3Qty, t4Qty, t5Qty
            )
        );
        
        // Step 4: Create position tracking object
        string signalName = direction == MarketPosition.Long ? "ORLong" : "ORShort";
        string timestamp = DateTime.Now.ToString("HHmmssffff");
        string entryName = signalName + "_" + timestamp;
        
        PositionInfo pos = CreateORPositionInfo(
            direction, entryPrice, stopPrice, contracts,
            t1Qty, t2Qty, t3Qty, t4Qty, t5Qty, entryName
        );
        
        // Step 5: Sync with panel clients
        string syncMsg = string.Format("POSITION_ENTERED|OR|{0}", contracts);
        SendResponseToRemote(syncMsg);
        
        // Step 6: Register expected position delta
        int masterDeltaOR = (direction == MarketPosition.Long) ? contracts : -contracts;
        {
            var _aek966 = ExpKey(Account.Name);
            var _aed966 = (masterDeltaOR);
            Enqueue(ctx => ctx.AddExpectedPositionDeltaLocked(_aek966, _aed966));
        }
        
        // Step 7: Submit entry order (with rollback on failure)
        Order entryOrder = SubmitOREntryOrder(direction, entryPrice, contracts, entryName, masterDeltaOR);
        if (entryOrder == null)
            return; // Rollback already handled in SubmitOREntryOrder
        
        // Step 8: Update position tracking state
        {
            var _en966ap = entryName;
            var _p966ap = pos;
            Enqueue(ctx => { ctx.activePositions[_en966ap] = _p966ap; });
        }
        {
            var _en966 = entryName;
            var _eo966 = entryOrder;
            Enqueue(ctx => { ctx.entryOrders[_en966] = _eo966; });
        }
        
        // Step 9: Log entry confirmation
        Print(
            string.Format(
                "OR ENTRY ORDER: {0} {1}@{2:F2} | Stop: {3:F2} | OR Range: {4:F2}",
                signalName, contracts, entryPrice, stopPrice, sessionRange
            )
        );
        Print(
            string.Format(
                "TARGETS: T1:{0}@{1:F2} | T2:{2}@{3:F2} | T3:{4}@{5:F2} | T4:{6}@{7:F2} | T5:{8}@{9:F2} (Runner targets trail-only)",
                t1Qty, pos.Target1Price, t2Qty, pos.Target2Price, t3Qty, pos.Target3Price,
                t4Qty, pos.Target4Price, t5Qty, pos.Target5Price
            )
        );
        
        // Step 10: Dispatch to SIMA fleet
        if (EnableSIMA)
        {
            ExecuteSmartDispatchEntry(
                "OR",
                direction == MarketPosition.Long ? OrderAction.Buy : OrderAction.SellShort,
                contracts,
                entryPrice,
                OrderType.StopMarket
            );
        }
    }
    catch (Exception ex)
    {
        Print("ERROR EnterORPosition: " + ex.Message);
    }
}
```

**Estimated CYC**: 6-7
- Base: 1
- ValidateOREntryPreconditions call: 0 (delegated)
- currentPrice ternary: +1
- ValidateOREntryPrice call: 0 (delegated)
- signalName ternary: +1
- CreateORPositionInfo call: 0 (delegated)
- masterDeltaOR ternary: +1
- SubmitOREntryOrder call: 0 (delegated)
- entryOrder null check: +1
- EnableSIMA check: +1
- direction ternary in ExecuteSmartDispatchEntry: +1
- **Total: 7** ✅ (below target of 8)

## Verification Plan

### Build Verification
```powershell
dotnet build src/V12_002.csproj
```
**Expected**: Zero errors, zero warnings

### Complexity Audit
```powershell
python scripts/complexity_audit.py
```
**Expected**:
- `EnterORPosition`: CYC ≤ 8
- `ValidateOREntryPreconditions`: CYC = 3
- `ValidateOREntryPrice`: CYC = 2
- `CreateORPositionInfo`: CYC = 1
- `SubmitOREntryOrder`: CYC = 2

### ASCII Compliance
```powershell
python check_ascii.py src/V12_002.Entries.OR.cs
```
**Expected**: Zero non-ASCII characters

### Hard-Link Sync
```powershell
powershell -File .\deploy-sync.ps1
```
**Expected**: Diff < 10,000 characters

### Pre-Push Validation
```powershell
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```
**Expected**: All checks pass

## Risk Mitigation

### Rollback Strategy
- Bob CLI checkpointing enabled (automatic)
- Manual restore via `/restore` command
- Git branch: `feature/epic-ccn-125-enteror-extraction`

### Testing Strategy
1. **Build Test**: Verify compilation after each extraction
2. **Complexity Test**: Run audit after each extraction
3. **Logic Test**: Manual F5 in NinjaTrader after all extractions
4. **Integration Test**: Execute OR entry in paper trading

### Boundary Enforcement
- ✅ All extractions within single file
- ✅ No changes to method signatures
- ✅ No modifications to callers
- ✅ No cross-file dependencies

## Success Criteria

### Primary Goals
1. ✅ **Complexity Reduction**: CYC reduced from 11 to ≤ 8
2. ✅ **Jane Street Alignment**: Target CYC ≤ 8 achieved
3. ✅ **Cognitive Simplicity**: Each method has single, clear purpose
4. ✅ **Correctness Preservation**: Zero behavioral changes
5. ✅ **Lock-Free Semantics**: No locks introduced

### Verification Checklist
- [ ] Build succeeds (dotnet build)
- [ ] Complexity audit confirms CYC ≤ 8
- [ ] ASCII compliance verified
- [ ] Hard-link sync succeeds
- [ ] Pre-push validation passes
- [ ] F5 test in NinjaTrader succeeds
- [ ] OR entry executes correctly in paper trading

## Phase 2 Completion Status

- [x] Method structure analyzed
- [x] Complexity breakdown documented
- [x] Extraction strategy defined (4 methods)
- [x] Data flow mapped
- [x] Implementation sequence planned
- [x] Verification plan created
- [x] Risk mitigation documented
- [x] Success criteria established
- [ ] Manifest updated (next step)

## Next Phase

**Phase 3: Implementation**
- Execute extraction sequence (Steps 1-5)
- Run verification after each step
- Update manifest with results
- Generate acceptance report

---

*Generated: 2026-06-13*
*Epic: EPIC-CCN-125*
*Phase: 2 (Architecture Planning)*
*Target: EnterORPosition (CYC 11 → ≤ 8)*
*Estimated CYC Reduction: -4 to -7 points*
