# EPIC-CCN-12: ManageCIT Forensic Intake

**Date**: 2026-06-02T18:23:00Z  
**Auditor**: V12 Photon Engineer (Advanced Mode)  
**Stage**: Stage 0 (Forensic Intake)  
**Protocol**: Phase 6 Recursive  
**Target Method**: `ManageCIT`

---

## Executive Summary

**Method**: `ManageCIT` (Chase If Touch)  
**Location**: `src/V12_002.Orders.Management.Flatten.cs` (lines 68-194)  
**Current Metrics**:
- **CYC**: 20 (Jane Street threshold: ≤15)
- **LOC**: 126 lines
- **Nesting Depth**: 6 levels (estimated)
- **Priority**: 🎯 **P1 TIER #1** (highest CYC in watch list)

**Historical Context**:
- ✅ EPIC-CCN-11 completed Stage 3 (DNA Audit) with approved extraction plan
- ❌ EPIC-CCN-11 Stage 4 (Execution) was **NEVER PERFORMED**
- 🔄 EPIC-CCN-12 is a **FRESH START** with updated forensics (2026-06-02)

**Complexity Drivers**:
1. **Dual-path execution**: Local vs Follower order handling (2 branches)
2. **Build-specific fixes**: 5 critical business logic patches (Build 924, 949, 966, 984, 1109)
3. **Budget management**: Broker call throttling with self-enqueue pattern
4. **Directional logic**: Long/Short entry price comparison (Build 984 fix)
5. **One-shot guard**: `_citNudgedKeys` prevents duplicate nudges (Build 949)

**Extraction Strategy**: Bottom-up helper extraction (5 phases, reuse EPIC-CCN-11 approved plan)

---

## Section 1: Method Signature & Location

### 1.1 Signature

```csharp
private void ManageCIT()
```

**Visibility**: `private`  
**Return Type**: `void` (side effects: order modifications, dictionary updates)  
**Parameters**: None (accesses instance state)

### 1.2 File Location

**File**: `src/V12_002.Orders.Management.Flatten.cs`  
**Line Range**: 68-194 (126 lines)  
**Namespace**: `NinjaTrader.NinjaScript.Strategies`  
**Class**: `V12_002` (partial class)

### 1.3 Region

**Region**: `#region Orders Management Flatten`  
**Adjacent Methods**:
- `SyncPositionState()` (lines 40-58)
- `FlattenAll()` (lines 196-end)

---

## Section 2: Current Complexity Metrics

### 2.1 Cyclomatic Complexity

**CYC**: 20 (from complexity audit 2026-06-02T18:21:55Z)  
**Jane Street Threshold**: ≤15  
**Deviation**: +5 (33% over threshold)

**Complexity Breakdown** (estimated from code structure):
- Base: 1
- Early returns: +3 (lines 71, 73, 81)
- Propagation check: +1 (line 77)
- Parse check: +1 (line 84)
- Main loop: +1 (line 89)
- Order state checks: +3 (lines 93, 95, 97)
- Directional logic: +2 (lines 108-111)
- Trigger check: +1 (line 113)
- Follower branch: +1 (line 130)
- Budget exhaustion: +1 (line 139)
- CreateOrder null check: +1 (line 161)
- Exception handlers: +2 (lines 184, 188)
- **Total**: ~20 CYC

### 2.2 Lines of Code

**Total LOC**: 126 lines (lines 68-194)  
**Effective LOC**: ~98 (excluding comments, blank lines)  
**Jane Street Guideline**: Functions should fit on one screen (~50 LOC)  
**Deviation**: +76 LOC (152% over guideline)

### 2.3 Nesting Depth

**Max Nesting**: 6 levels (estimated)  
**Jane Street Threshold**: ≤4 levels  
**Deviation**: +2 levels (50% over threshold)

**Nesting Structure**:
1. Method body
2. Main loop (`foreach`)
3. Order state checks (`if`)
4. Trigger check (`if`)
5. Try-catch block
6. Follower branch (`if`) OR Budget check (`if`)

---

## Section 3: Churn Analysis

### 3.1 Git History

**Method Age**: ~18 months (estimated, based on Build 924-1109 range)  
**Build-Specific Fixes**: 5 patches applied over time

**Build Timeline**:
1. **Build 924**: Propagation suppression (line 77-81)
2. **Build 949**: One-shot nudge guard (line 97-98, 182)
3. **Build 966**: Actor-safe dictionary write (line 172)
4. **Build 984**: Directional bar-price fix (lines 100-111)
5. **Build 1109**: Freeze-proof budget management (lines 139-144)

### 3.2 Churn Rate

**Estimated Commits**: 5+ (one per build fix)  
**Churn Assessment**: 🟡 **ACTIVE** (multiple fixes indicate evolving requirements)  
**Stability**: 🟢 **STABLE** (no recent changes, fixes are cumulative)

**Implication**: High churn in the past, but now stable. Extraction is safe.

---

## Section 4: Dependency Analysis

### 4.1 What Calls This Method

**Direct Callers**:
1. `OnBarUpdate()` - Phase C (via `Enqueue(ctx => ctx.ManageCIT())`)
2. `ManageCIT()` itself - Self-enqueue on budget exhaustion (line 142)

**Call Pattern**: Always invoked via actor `Enqueue`, ensuring single-threaded access.

### 4.2 What This Method Calls

**NinjaTrader API**:
- `ChangeOrder(order, quantity, limitPrice, stopPrice)` - Local order modification (line 180)
- `Account.Cancel(orders[])` - Follower order cancellation (line 147)
- `Account.CreateOrder(...)` - Follower order creation (lines 149-160)
- `Account.Submit(orders[])` - Follower order submission (line 168)

**Instance Methods**:
- `Print(message)` - Logging (lines 79, 135, 141, 177, 186, 190)
- `Enqueue(ctx => ctx.ManageCIT())` - Self-enqueue (line 142)

**Instance State Access**:
- **Read**: `activePositions`, `entryOrders`, `_propagationActive`, `ChaseIfTouchPoints`, `Low[0]`, `High[0]`, `Instrument`, `MaxBrokerCallsPerCycle`
- **Write**: `entryOrders[key]`, `_citNudgedKeys[key]`, `_citBrokerBudget` (local variable)

### 4.3 Shared State

**Thread-Safe Collections**:
- `activePositions` (ConcurrentDictionary) - Read-only in this method
- `entryOrders` (ConcurrentDictionary) - Read + Write (line 172)
- `_citNudgedKeys` (ConcurrentDictionary) - Read + Write (line 97, 182)

**Actor-Safe Access**: All writes occur within actor drain (Build 966 pattern).

---

## Section 5: Complexity Breakdown

### 5.1 What Makes This Method Complex?

**Primary Drivers**:

1. **Dual Execution Paths** (Local vs Follower):
   - Lines 130-173: Follower branch (44 lines)
   - Lines 174-181: Local branch (8 lines)
   - **Impact**: +1 CYC, +52 LOC

2. **Build-Specific Business Logic** (5 fixes):
   - Build 924: Propagation suppression (lines 77-81)
   - Build 949: One-shot guard (lines 97-98, 182)
   - Build 966: Actor-safe write (line 172)
   - Build 984: Directional logic (lines 100-111)
   - Build 1109: Budget management (lines 139-144)
   - **Impact**: +5 CYC, +30 LOC

3. **Budget Throttling** (Freeze-proof pattern):
   - Lines 139-144: Budget check + self-enqueue
   - Lines 145: Budget decrement
   - **Impact**: +2 CYC, +6 LOC

4. **Error Handling** (2 catch blocks):
   - Lines 184-187: Known quirk handler
   - Lines 188-193: Critical error handler
   - **Impact**: +2 CYC, +10 LOC

5. **Price Calculation** (Directional + Rounding):
   - Lines 123-128: Nudge distance calculation
   - **Impact**: +1 CYC, +6 LOC

### 5.2 Extraction Candidates

**Helper Method Opportunities** (5 helpers, reuse EPIC-CCN-11 plan):

1. **`ValidateCitConfiguration(out double citOffset)`**
   - Lines: 70-85 (16 lines)
   - CYC: 5
   - Responsibility: Early validation (empty collections, parse, propagation)

2. **`ShouldChaseOrder(Order order, string key, out double currentPrice, out double limitPrice)`**
   - Lines: 93-114 (22 lines)
   - CYC: 7
   - Responsibility: Decision logic (state checks, directional logic, trigger)

3. **`CalculateNudgedPrice(Order order, double limitPrice, double citOffset)`**
   - Lines: 123-128 (6 lines)
   - CYC: 2
   - Responsibility: Pure calculation (nudge distance + rounding)

4. **`ExecuteLocalNudge(Order order, string key, double newLimitPrice)`**
   - Lines: 174-181 (8 lines)
   - CYC: 1
   - Responsibility: Local order modification

5. **`ExecuteFollowerNudge(string key, Order order, double newLimitPrice, PositionInfo pos, ref int brokerBudget)`**
   - Lines: 130-173 (44 lines)
   - CYC: 3
   - Responsibility: Follower order cancel + resubmit

**Expected Reduction**:
- Main method: 126 LOC → ~60 LOC (53% reduction)
- Main method: 20 CYC → ≤15 CYC (25% reduction)

---

## Section 6: Initial Helper Method Candidates

### 6.1 Helper 1: ValidateCitConfiguration

**Signature**:
```csharp
private bool ValidateCitConfiguration(out double citOffset)
```

**Responsibility**: Early validation (fail-fast pattern)

**Logic Extracted**:
- Empty collection checks (lines 70-71)
- ChaseIfTouchPoints parse (lines 72-85)
- Propagation suppression (lines 77-81)

**Return**: `true` if CIT should proceed, `false` otherwise

**Out Parameter**: `citOffset` (parsed value, only valid if return is `true`)

**CYC**: 5 (3 early returns + 1 parse check + 1 propagation check)

**Jane Street Alignment**: Fail-fast pattern reduces nesting in main method

---

### 6.2 Helper 2: ShouldChaseOrder

**Signature**:
```csharp
private bool ShouldChaseOrder(Order order, string key, out double currentPrice, out double limitPrice)
```

**Responsibility**: Decision logic (pure decision function)

**Logic Extracted**:
- Order state checks (lines 93-96)
- One-shot guard check (lines 97-98)
- Directional bar-price logic (lines 100-111)
- Trigger decision (lines 108-114)

**Return**: `true` if order should be chased, `false` otherwise

**Out Parameters**: `currentPrice`, `limitPrice` (captured for logging)

**CYC**: 7 (4 state checks + 2 directional branches + 1 trigger check)

**Jane Street Alignment**: Pure decision function (no side effects)

---

### 6.3 Helper 3: CalculateNudgedPrice

**Signature**:
```csharp
private double CalculateNudgedPrice(Order order, double limitPrice, double citOffset)
```

**Responsibility**: Price calculation (pure function)

**Logic Extracted**:
- Tick size retrieval (line 123)
- Nudge distance calculation (line 124)
- Directional price adjustment (lines 125-128)

**Return**: New limit price (rounded to tick size)

**CYC**: 2 (1 directional branch + base)

**Jane Street Alignment**: Pure function (no state access, no side effects)

---

### 6.4 Helper 4: ExecuteLocalNudge

**Signature**:
```csharp
private void ExecuteLocalNudge(Order order, string key, double newLimitPrice)
```

**Responsibility**: Local order modification (simple command)

**Logic Extracted**:
- Log message (lines 177-179)
- ChangeOrder call (line 180)

**Return**: `void` (side effect: order modification)

**CYC**: 1 (no branches)

**Jane Street Alignment**: Simple command pattern

---

### 6.5 Helper 5: ExecuteFollowerNudge

**Signature**:
```csharp
private void ExecuteFollowerNudge(string key, Order order, double newLimitPrice, PositionInfo pos, ref int brokerBudget)
```

**Responsibility**: Follower order cancel + resubmit (bounded complexity)

**Logic Extracted**:
- Budget exhaustion check (lines 139-144)
- Account retrieval (line 133)
- Log message (lines 134-136)
- Cancel + CreateOrder + Submit (lines 147-168)
- Dictionary update (line 172)

**Return**: `void` (side effects: order cancellation, submission, dictionary update)

**Ref Parameter**: `brokerBudget` (decremented by 2)

**CYC**: 3 (1 budget check + 1 null check + base)

**Jane Street Alignment**: Bounded complexity (CYC ≤5)

---

## Section 7: Critical Business Logic Preservation

### 7.1 Build 984: Directional Bar-Price Logic

**Location**: Lines 100-111  
**Criticality**: 🔴 **HIGH** (incorrect logic causes instant market conversion)

**Logic**:
```csharp
// Long entry (Buy): price must DROP DOWN to the limit -> compare Low[0] <= limitPrice
// Short entry (Sell): price must RISE UP to the limit -> compare High[0] >= limitPrice
double currentPrice = (order.OrderAction == OrderAction.Buy) ? Low[0] : High[0];
bool triggerChase =
    (order.OrderAction == OrderAction.Buy)
        ? (currentPrice <= limitPrice) // Long: bar low touched or pierced
        : (currentPrice >= limitPrice); // Short: bar high touched or pierced
```

**Preservation Strategy**: Extract to `ShouldChaseOrder` helper (exact copy)

**Verification**: TDD test for Long/Short entry scenarios

---

### 7.2 Build 949: One-Shot Nudge Guard

**Location**: Lines 97-98, 182  
**Criticality**: 🔴 **HIGH** (missing guard causes infinite nudge loop)

**Logic**:
```csharp
// Check (line 97-98):
if (_citNudgedKeys.ContainsKey(key))
    continue; // [BUILD 949] one-shot: already nudged

// Set (line 182):
_citNudgedKeys.TryAdd(key, true); // [BUILD 949] one-shot: mark as nudged
```

**Preservation Strategy**:
- Check: Extract to `ShouldChaseOrder` helper
- Set: Keep in main method (after both helper calls)

**Verification**: TDD test for duplicate nudge prevention

---

### 7.3 Build 1109: Freeze-Proof Budget Management

**Location**: Lines 139-144  
**Criticality**: 🔴 **HIGH** (missing self-enqueue causes strategy thread stall)

**Logic**:
```csharp
if (_citBrokerBudget <= 0)
{
    Print("[CIT] Broker budget exhausted -- deferring remaining nudges");
    Enqueue(ctx => ctx.ManageCIT());
    return;
}
_citBrokerBudget -= 2; // Cancel + Submit = 2 broker calls
```

**Preservation Strategy**: Extract to `ExecuteFollowerNudge` helper (exact copy)

**Verification**: TDD test for budget exhaustion + self-enqueue

---

### 7.4 Build 966: Actor-Safe Dictionary Writes

**Location**: Line 172  
**Criticality**: 🟡 **MEDIUM** (incorrect pattern causes race conditions)

**Logic**:
```csharp
// B966: No Enqueue needed -- ManageCIT is always called via Enqueue(ctx => ctx.ManageCIT())
// from OnBarUpdate (Phase C), so this write is already inside the actor drain.
entryOrders[key] = nudgedOrder;
```

**Preservation Strategy**: Extract to `ExecuteFollowerNudge` helper (preserve comment)

**Verification**: Actor pattern audit (no additional Enqueue needed)

---

### 7.5 Build 924: Propagation Suppression

**Location**: Lines 77-81  
**Criticality**: 🟡 **MEDIUM** (missing check causes race-fire on freshly resubmitted orders)

**Logic**:
```csharp
// [BUILD 924 -- Fix C] Suppress CIT during price-move propagation to prevent
// race-fire on freshly resubmitted follower limit orders before sync cycle completes.
if (_propagationActive)
{
    Print("[CIT] Suppressed during price-move propagation (Build 924 Fix C)");
    return;
}
```

**Preservation Strategy**: Extract to `ValidateCitConfiguration` helper (exact copy)

**Verification**: TDD test for propagation suppression

---

## Section 8: Extraction Strategy

### 8.1 Bottom-Up Approach

**Rationale**: Extract simplest helpers first to minimize risk.

**Phase Order** (reuse EPIC-CCN-11 approved plan):
1. **Phase 1**: Extract `CalculateNudgedPrice` (CYC=2, pure function)
2. **Phase 2**: Extract `ExecuteLocalNudge` (CYC=1, simple command)
3. **Phase 3**: Extract `ValidateCitConfiguration` (CYC=5, early validation)
4. **Phase 4**: Extract `ShouldChaseOrder` (CYC=7, decision logic)
5. **Phase 5**: Extract `ExecuteFollowerNudge` (CYC=3, bounded complexity)

**Per-Phase Verification**:
- Run CSharpier (formatting)
- Verify compilation (`dotnet build`)
- Run complexity audit (`complexity_audit.py`)
- Verify CYC reduction (explicit target)
- Commit with phase marker

---

### 8.2 Expected Outcome

**Before Extraction**:
- ManageCIT: 126 LOC, 20 CYC, 6 nesting levels

**After Extraction**:
- ManageCIT: ~60 LOC, ≤15 CYC, ≤4 nesting levels
- ValidateCitConfiguration: 16 LOC, 5 CYC
- ShouldChaseOrder: 22 LOC, 7 CYC
- CalculateNudgedPrice: 6 LOC, 2 CYC
- ExecuteLocalNudge: 8 LOC, 1 CYC
- ExecuteFollowerNudge: 44 LOC, 3 CYC

**Net Change**: +52 LOC (96 added - 38 removed from main)

**Complexity Reduction**: 42% (20 CYC → ≤15 CYC)

---

## Section 9: Risk Assessment

### 9.1 High-Risk Areas

1. **Directional Logic Inversion** (Build 984)
   - Risk: Swapping `<=` and `>=` operators
   - Mitigation: TDD tests for Long/Short entry scenarios
   - Verification: Manual trade execution

2. **One-Shot Guard Bypass** (Build 949)
   - Risk: Guard check removed or placed incorrectly
   - Mitigation: TDD test for duplicate nudge prevention
   - Verification: Risk audit Case 4

3. **Budget Exhaustion Logic Breaks** (Build 1109)
   - Risk: Self-enqueue removed or budget decrement incorrect
   - Mitigation: TDD test for budget exhaustion + self-enqueue
   - Verification: Risk audit Case 2

### 9.2 Medium-Risk Areas

1. **Follower Propagation Fails**
   - Risk: Cancel + Submit sequence broken
   - Mitigation: TDD test for follower order lifecycle
   - Verification: Risk audit Case 7

2. **Hard Link Desync**
   - Risk: NinjaTrader hard links not updated
   - Mitigation: `deploy-sync.ps1` after every phase
   - Verification: ASCII gate + 81 links check

### 9.3 Low-Risk Areas

1. **Performance Degradation**
   - Risk: Helper method overhead
   - Mitigation: BenchmarkDotNet baseline
   - Verification: Latency comparison

2. **Compilation Failure**
   - Risk: Syntax errors during extraction
   - Mitigation: Incremental extraction + `dotnet build` per phase
   - Verification: Zero compilation errors

---

## Section 10: Next Steps

### 10.1 Immediate Actions

1. ✅ **Stage 0 COMPLETE**: Forensic intake document created
2. 🔄 **Stage 1 NEXT**: Vision/Spec dialogue with Director
   - Confirm extraction strategy (reuse EPIC-CCN-11 plan OR create new)
   - Confirm helper method signatures
   - Confirm test coverage requirements (15+ tests)

### 10.2 Stage 1 Deliverables

**Document**: `docs/brain/EPIC-CCN-12/02-managecit-mini-spec.md`

**Content**:
- Helper method signatures (5 helpers)
- Extraction order (bottom-up)
- Test cases (15+ tests)
- Verification checkpoints (per-phase)

### 10.3 Stage 2 Deliverables

**Document**: `docs/brain/EPIC-CCN-12/03-managecit-implementation-plan.md`

**Content**:
- Step-by-step extraction instructions
- Before/after code snippets
- Rollback plan (< 5 minute recovery)
- Risk mitigation strategies

---

## Appendix A: EPIC-CCN-11 Historical Context

### A.1 What Happened?

**Timeline**:
1. **2026-06-02 (earlier)**: EPIC-CCN-11 Stage 0-3 completed
2. **Stage 3 Output**: DNA Audit approved extraction plan (✅ PASS)
3. **Stage 4**: **NEVER EXECUTED** (no commits, no code changes)
4. **2026-06-02 (18:21:55Z)**: Complexity audit reveals ManageCIT still at CYC=20

**Root Cause**: Unknown (possible session interruption, context loss, or handoff failure)

### A.2 Lessons Learned

1. **Verify Execution**: Always check actual code after "completion" claims
2. **Commit Markers**: Require git commits for each phase (not just documentation)
3. **Complexity Audit**: Run fresh audit before declaring epic complete

### A.3 EPIC-CCN-12 Improvements

1. **Fresh Forensics**: Updated with 2026-06-02 complexity audit
2. **Explicit Verification**: Per-phase git commits required
3. **Reuse Approved Plan**: EPIC-CCN-11 plan is still valid (zero logic drift)

---

**End of Forensic Intake**

**Status**: ✅ STAGE 0 COMPLETE  
**Next Action**: Proceed to Stage 1 (Vision/Spec)  
**Estimated Stage 1 Duration**: 30 minutes