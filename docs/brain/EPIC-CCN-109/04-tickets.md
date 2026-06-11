# Phase 4: Ticket Generation - EPIC-CCN-109

## Epic Metadata
- **Epic ID**: EPIC-CCN-109
- **Target Method**: `HydrateWorkingOrdersFromBroker`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Phase**: 4 (Ticket Generation)
- **Date**: 2026-06-11
- **Planner**: Bob Shell (v12-engineer)

---

## Ticket Execution Order

**CRITICAL**: Tickets MUST be executed in this exact order due to dependencies.

```
TICKET-1 (Sub-Extraction)
    ↓
TICKET-2 (Primary Extraction)
    ↓
TICKET-3 (Verification)
```

---

## TICKET-1: Extract Trade DNA Assignment Helper

### Metadata
- **Ticket ID**: TICKET-1
- **Type**: Sub-Extraction (Complexity Reduction)
- **Priority**: P0 (Blocker for TICKET-2)
- **Estimated CYC Reduction**: 6 points
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Lines**: 411-419 (inline logic in reconstruction block)

### Objective
Extract trade DNA flag assignment logic into a dedicated helper method to reduce complexity of the upcoming `ReconstructMasterPositionFromBroker()` extraction.

### Current State
**Location**: Lines 411-419 (inline within master position reconstruction block)
**Current CYC**: 6 (part of larger CYC 12 block)
**Pattern**: Sequential flag assignments based on key prefix matching

### Target State
**New Method**: `AssignTradeDNA(PositionInfo pos, string key, bool trendMnlMatch)`
**Target CYC**: 6 (isolated)
**Parent Impact**: Reduces reconstruction block CYC 12 → 7

### Method Signature
```csharp
/// <summary>
/// Assigns trade DNA flags (MOMO, TREND, RMA, FFMA, Retest) to a PositionInfo.
/// Extracted to reduce complexity of ReconstructMasterPositionFromBroker.
/// </summary>
/// <param name="pos">Position to assign DNA flags to</param>
/// <param name="key">Signal name key for prefix matching</param>
/// <param name="trendMnlMatch">Pre-computed TrendMnl prefix match result</param>
private void AssignTradeDNA(PositionInfo pos, string key, bool trendMnlMatch)
```

### Implementation Steps

#### Step 1: Create Method Shell
**Location**: After `HydrateWorkingOrdersFromBroker()` method (around line 440)
**Action**: Add new private method

```csharp
/// <summary>
/// Assigns trade DNA flags (MOMO, TREND, RMA, FFMA, Retest) to a PositionInfo.
/// Extracted to reduce complexity of ReconstructMasterPositionFromBroker.
/// </summary>
/// <param name="pos">Position to assign DNA flags to</param>
/// <param name="key">Signal name key for prefix matching</param>
/// <param name="trendMnlMatch">Pre-computed TrendMnl prefix match result</param>
private void AssignTradeDNA(PositionInfo pos, string key, bool trendMnlMatch)
{
    // [STEP 2: Move DNA assignment logic here]
}
```

#### Step 2: Move DNA Assignment Logic
**Source Lines**: 411-419 (within reconstruction block)
**Target**: Inside `AssignTradeDNA()` method body

**Logic to Move**:
```csharp
pos.IsMOMOTrade = key.StartsWith("MOMO", StringComparison.OrdinalIgnoreCase);
pos.IsTRENDTrade = trendMnlMatch || key.StartsWith("TRMA_", StringComparison.OrdinalIgnoreCase);
pos.IsRetestTrade = key.StartsWith("Retest", StringComparison.OrdinalIgnoreCase);
pos.IsRMATrade = key.StartsWith("TRMA_", StringComparison.OrdinalIgnoreCase) || pos.IsRetestTrade;
pos.IsFFMATrade = key.StartsWith("FFMA", StringComparison.OrdinalIgnoreCase);

// MOMO trades are never RMA trades
if (pos.IsMOMOTrade)
    pos.IsRMATrade = false;
```

#### Step 3: Update Call Site
**Location**: Lines 411-419 (within reconstruction block)
**Replace With**:
```csharp
// Assign trade DNA flags
AssignTradeDNA(pos, key, trendMnlMatch);
```

#### Step 4: Verify Build
```powershell
dotnet build
```
**Expected**: Zero errors

#### Step 5: Verify Complexity
```powershell
python scripts/complexity_audit.py
```
**Expected**: `AssignTradeDNA` CYC = 6

#### Step 6: Deploy Sync
```powershell
powershell -File .\deploy-sync.ps1
```
**Expected**: ASCII gate PASS

### Success Criteria
- [ ] `AssignTradeDNA()` method created with correct signature
- [ ] Lines 411-419 moved into new method
- [ ] Call site updated to single line: `AssignTradeDNA(pos, key, trendMnlMatch);`
- [ ] `dotnet build` passes (zero errors)
- [ ] `AssignTradeDNA` CYC = 6 (verified via complexity_audit.py)
- [ ] `deploy-sync.ps1` passes ASCII gate
- [ ] No logic drift (pure structural movement)

### Verification Commands
```powershell
# Build
dotnet build

# Complexity audit
python scripts/complexity_audit.py

# Deploy sync
powershell -File .\deploy-sync.ps1

# F5 in NinjaTrader IDE (verify BUILD_TAG)
```

### Dependencies
- **Blocks**: TICKET-2 (primary extraction requires this sub-extraction)
- **Blocked By**: None

### Risk Assessment
- **Blast Radius**: 0 (new private method, no external callers)
- **State Coupling**: Low (reads 2 params, writes to 1 param object)
- **Logic Drift**: Zero (pure structural movement)
- **Regression Risk**: LOW (idempotent flag assignments)

---

## TICKET-2: Extract Master Position Reconstruction

### Metadata
- **Ticket ID**: TICKET-2
- **Type**: Primary Extraction
- **Priority**: P0 (Epic Goal)
- **Estimated CYC Reduction**: 11 points (parent 19 → 8)
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Lines**: 344-438 (95 lines)

### Objective
Extract master position reconstruction logic from `HydrateWorkingOrdersFromBroker()` to reduce parent method complexity from CYC 19 → 8.

### Current State
**Method**: `HydrateWorkingOrdersFromBroker()`
**Current CYC**: 19
**Target Lines**: 344-438 (95 lines of inline reconstruction logic)
**Pattern**: Broker position scan + stop order loop + PositionInfo construction

### Target State
**New Method**: `ReconstructMasterPositionFromBroker()`
**Target CYC**: 7 (after TICKET-1 sub-extraction)
**Parent CYC**: 8 (after extraction)

### Method Signature
```csharp
/// <summary>
/// Reconstructs master activePositions from adopted bracket orders + broker state.
/// Handles edge case where master has filled position but no working entry order.
/// Cold path: Runs once at strategy startup during order hydration.
/// </summary>
/// <returns>Count of reconstructed positions (0 if no master position found)</returns>
private int ReconstructMasterPositionFromBroker()
```

### Implementation Steps

#### Step 1: Create Method Shell
**Location**: After `HydrateWorkingOrdersFromBroker()` method (around line 440)
**Action**: Add new private method

```csharp
/// <summary>
/// Reconstructs master activePositions from adopted bracket orders + broker state.
/// Handles edge case where master has filled position but no working entry order.
/// Cold path: Runs once at strategy startup during order hydration.
/// </summary>
/// <returns>Count of reconstructed positions (0 if no master position found)</returns>
private int ReconstructMasterPositionFromBroker()
{
    int reconstructedCount = 0;
    
    try
    {
        // [STEP 2: Move reconstruction logic here]
        
        return reconstructedCount;
    }
    catch (Exception ex)
    {
        Print(string.Format("[SIMA HYDRATE] WARNING: Master position reconstruction failed: {0}", ex.Message));
        return 0;
    }
}
```

#### Step 2: Move Reconstruction Logic
**Source Lines**: 345-438 (93 lines, excluding outer try-catch)
**Target**: Inside try block of `ReconstructMasterPositionFromBroker()`

**Logic Blocks to Move**:
1. **Broker Position Scan** (lines 345-358): Find master position in broker state
2. **Position Reconstruction Loop** (lines 360-434): Build PositionInfo from stop orders
3. **Trade DNA Assignment** (lines 411-419): Call `AssignTradeDNA()` (from TICKET-1)
4. **Position Insertion** (lines 421-433): Insert into activePositions + log

#### Step 3: Add Reconstructed Count Tracking
**Modification**: Increment `reconstructedCount` when position is inserted
**Location**: After `activePositions[key] = pos;` (line 421)

```csharp
activePositions[key] = pos;
reconstructedCount++;  // ADD THIS LINE
Print(string.Format("[SIMA HYDRATE] Reconstructed master position for {0} | Dir={1} Qty={2} AvgPx={3} StopPx={4}",
    key, masterMP, masterQty, masterAvgPrice, stopPrice));
```

#### Step 4: Update Parent Method Call Site
**Location**: Lines 344-438 (95 lines)
**Replace With**:
```csharp
if (!masterIsFleetForOrders993)
{
    int reconstructedCount = ReconstructMasterPositionFromBroker();
    if (reconstructedCount > 0)
    {
        Print(string.Format("[SIMA HYDRATE] Reconstructed {0} master position(s)", reconstructedCount));
    }
}
```

#### Step 5: Verify Build
```powershell
dotnet build
```
**Expected**: Zero errors

#### Step 6: Verify Complexity
```powershell
python scripts/complexity_audit.py
```
**Expected**: 
- `HydrateWorkingOrdersFromBroker` CYC = 8
- `ReconstructMasterPositionFromBroker` CYC = 7

#### Step 7: Deploy Sync
```powershell
powershell -File .\deploy-sync.ps1
```
**Expected**: ASCII gate PASS

#### Step 8: Bump BUILD_TAG
**File**: `src/V12_002.cs`
**Action**: Increment BUILD_TAG
**Format**: `V12.XXX` (increment last digit)

### Success Criteria
- [ ] `ReconstructMasterPositionFromBroker()` method created with correct signature
- [ ] Lines 345-438 moved into new method
- [ ] `reconstructedCount` tracking added
- [ ] Call site updated to 5 lines (if block + call + log)
- [ ] `dotnet build` passes (zero errors)
- [ ] `HydrateWorkingOrdersFromBroker` CYC = 8 (verified)
- [ ] `ReconstructMasterPositionFromBroker` CYC = 7 (verified)
- [ ] `deploy-sync.ps1` passes ASCII gate
- [ ] BUILD_TAG incremented
- [ ] No logic drift (pure structural movement)

### Verification Commands
```powershell
# Build
dotnet build

# Complexity audit
python scripts/complexity_audit.py

# Deploy sync
powershell -File .\deploy-sync.ps1

# F5 in NinjaTrader IDE (verify BUILD_TAG)
```

### Dependencies
- **Blocks**: TICKET-3 (verification)
- **Blocked By**: TICKET-1 (requires `AssignTradeDNA()` helper)

### Risk Assessment
- **Blast Radius**: 0 (no external callers, private method)
- **State Coupling**: Medium (reads 3 fields, writes 1 field, all actor-serialized)
- **Logic Drift**: Zero (pure structural movement)
- **Regression Risk**: LOW (idempotent guards, defensive checks, fail-safe try-catch)

---

## TICKET-3: Integration Verification

### Metadata
- **Ticket ID**: TICKET-3
- **Type**: Verification
- **Priority**: P0 (Epic Completion Gate)
- **File**: `src/V12_002.SIMA.Lifecycle.cs`

### Objective
Verify that the extraction maintains original behavior and meets all V12 DNA compliance requirements.

### Verification Steps

#### Step 1: Build Verification
```powershell
dotnet build
```
**Expected**: Zero errors, zero warnings

#### Step 2: Complexity Audit
```powershell
python scripts/complexity_audit.py
```
**Expected**:
- `HydrateWorkingOrdersFromBroker` CYC ≤ 8 ✅
- `ReconstructMasterPositionFromBroker` CYC ≤ 8 ✅
- `AssignTradeDNA` CYC ≤ 8 ✅

#### Step 3: ASCII Compliance
```powershell
python scripts/ascii_audit.py src/
```
**Expected**: Zero non-ASCII characters

#### Step 4: Lock-Free Audit
```powershell
grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs
```
**Expected**: Zero matches

#### Step 5: Deploy Sync
```powershell
powershell -File .\deploy-sync.ps1
```
**Expected**: ASCII gate PASS, 83 files synchronized

#### Step 6: NinjaTrader Integration Test
**Action**: F5 in NinjaTrader IDE
**Verification**:
1. Strategy compiles without errors
2. BUILD_TAG appears in output window
3. Strategy loads successfully
4. Check log for "[SIMA HYDRATE] Reconstructed X master position(s)"

#### Step 7: Pre-Push Validation
```powershell
powershell -File .\scripts\pre_push_validation.ps1 -Fast
```
**Expected**: All checks PASS

### Success Criteria
- [ ] `dotnet build` passes (zero errors)
- [ ] All methods CYC ≤ 8 (verified via complexity_audit.py)
- [ ] Zero non-ASCII characters (verified via ascii_audit.py)
- [ ] Zero lock() statements (verified via grep)
- [ ] `deploy-sync.ps1` passes ASCII gate
- [ ] BUILD_TAG incremented and visible in NinjaTrader
- [ ] Strategy loads without errors
- [ ] Reconstruction log appears in output
- [ ] Pre-push validation passes (fast mode)

### Verification Commands
```powershell
# Full verification suite
dotnet build
python scripts/complexity_audit.py
python scripts/ascii_audit.py src/
grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs
powershell -File .\deploy-sync.ps1
powershell -File .\scripts\pre_push_validation.ps1 -Fast

# F5 in NinjaTrader IDE (manual)
```

### Dependencies
- **Blocks**: Phase 5 (Epic Completion)
- **Blocked By**: TICKET-1, TICKET-2

### Risk Assessment
- **Blast Radius**: 0 (verification only, no code changes)
- **Regression Risk**: N/A (verification step)

---

## Execution Summary

### Ticket Dependency Graph
```
TICKET-1 (Sub-Extraction: AssignTradeDNA)
    ↓
TICKET-2 (Primary Extraction: ReconstructMasterPositionFromBroker)
    ↓
TICKET-3 (Integration Verification)
```

### Estimated Complexity Reduction
- **Parent Method**: CYC 19 → 8 (58% reduction, 11 points)
- **Extracted Method**: CYC 7 (after sub-extraction)
- **Sub-Extracted Helper**: CYC 6

### Total Lines Moved
- **TICKET-1**: 9 lines (DNA assignment logic)
- **TICKET-2**: 95 lines (reconstruction logic)
- **Total**: 104 lines extracted

### Jane Street Compliance
- ✅ **Cognitive Simplicity**: Parent delegates reconstruction, focuses on orchestration
- ✅ **Bounded Latency**: Cold path only (startup), zero hot-path impact
- ✅ **Correctness by Construction**: Idempotent guards, defensive checks, fail-safe
- ✅ **Lock-Free**: Zero lock() statements
- ✅ **ASCII-Only**: All string literals are ASCII

### Risk Profile
- **Overall Risk**: LOW
- **Blast Radius**: 0 (private methods, no external callers)
- **Regression Risk**: LOW (idempotent, defensive, fail-safe)
- **Complexity Risk**: MITIGATED (sub-extraction in TICKET-1)

---

## Phase 4 Status
✅ **COMPLETE** - Tickets generated, ready for Phase 5 (Execution)

**Generation Date**: 2026-06-11T07:34:00Z
**Planner**: Bob Shell (v12-engineer)
**Protocol Version**: V12.23 (No Scope Creep)
**Next Phase**: Phase 5 (Ticket Execution) - Execute TICKET-1, TICKET-2, TICKET-3 sequentially