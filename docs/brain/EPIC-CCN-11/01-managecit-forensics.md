# EPIC-CCN-11: ManageCIT Forensic Analysis

**Date**: 2026-06-02  
**Analyst**: V12 Photon Engineer (Bob CLI)  
**Target**: `ManageCIT` method extraction  
**Protocol**: Phase 6 Recursive (Stage 0: Forensic Intake)

---

## Executive Summary

**CRITICAL FINDING**: ManageCIT complexity is **CYC=26** (not 20 as initially reported), exceeding Jane Street threshold by 73%. This is a **HIGH PRIORITY** extraction target.

**Risk Assessment**: **MEDIUM**
- ✅ Low blast radius (private method, 2 call sites)
- ✅ Stable churn (0.86 commits/week)
- ⚠️ High complexity (CYC=26, nesting depth=6)
- ⚠️ Critical business logic (Chase-If-Touch order management)

---

## 1. Method Signature & Location

**Symbol ID**: `src/V12_002.Orders.Management.Flatten.cs::V12_002.ManageCIT#method`

**Signature**:
```csharp
private void ManageCIT()
```

**File**: `src/V12_002.Orders.Management.Flatten.cs`  
**Lines**: 68-194 (127 lines total, 98 LOC excluding comments/whitespace)

**Module**: Orders.Management.Flatten (Build 971)

---

## 2. Complexity Metrics

| Metric | Value | Threshold | Status |
|--------|-------|-----------|--------|
| **Cyclomatic Complexity** | **26** | ≤15 | ❌ **EXCEEDS by 73%** |
| **Max Nesting Depth** | 6 | ≤4 | ❌ **EXCEEDS** |
| **Parameter Count** | 0 | ≤5 | ✅ PASS |
| **Lines of Code** | 98 | ≤80 | ❌ **EXCEEDS** |
| **Assessment** | HIGH | - | ⚠️ **CRITICAL** |

**Complexity Breakdown** (from source analysis):
- **Early returns**: 3 (lines 71, 73, 81)
- **Conditional branches**: 8+ (if/else, ternary operators)
- **Loop iterations**: 1 (foreach over entryOrders)
- **Try-catch blocks**: 2 (lines 121-193, nested 184-192)
- **Nested conditionals**: 6 levels deep (max nesting)

---

## 3. Git Churn Analysis

**Time Window**: Last 90 days (2026-03-10 to 2026-05-29)

| Metric | Value | Assessment |
|--------|-------|------------|
| **Total Commits** | 11 | Moderate |
| **Churn Rate** | 0.86/week | **STABLE** |
| **Unique Authors** | 3 | Collaborative |
| **First Seen** | 2026-03-10 | 2.8 months old |
| **Last Modified** | 2026-05-29 | 4 days ago |

**Volatility Assessment**: **STABLE** (< 1 commit/week)

**Authors**:
- malhitticrypto@gmail.com
- mkalhitti@gmail.com
- your-new-email@example.com

**Recent Changes** (inferred from comments):
- Build 924: Propagation suppression fix (line 77-81)
- Build 949: One-shot nudge guard (line 98, 182)
- Build 966: Actor drain comment (line 170)
- Build 984: Directional bar-price logic fix (lines 100-111)
- Build 1109: Broker budget freeze-proof (lines 138-145)

---

## 4. Call Graph Analysis

### 4.1 Callers (Who Calls ManageCIT)

**Direct Callers**: 2 call sites

1. **Synchronous Call** (V12_002.BarUpdate.cs:228)
   ```csharp
   // CIT Logic
   ManageCIT();
   ```
   - Context: OnBarUpdate Phase C (CIT Logic phase)
   - Execution: Direct, synchronous

2. **Enqueued Call** (V12_002.BarUpdate.cs:287)
   ```csharp
   if (activePositions.Count > 0)
   {
       Enqueue(ctx => ctx.ManageCIT());
   }
   ```
   - Context: OnBarUpdate Phase C (conditional re-enqueue)
   - Execution: Actor pattern (thread-safe)

3. **Self-Enqueue** (V12_002.Orders.Management.Flatten.cs:142)
   ```csharp
   if (_citBrokerBudget <= 0)
   {
       Print("[CIT] Broker budget exhausted -- deferring remaining nudges");
       Enqueue(ctx => ctx.ManageCIT());
       return;
   }
   ```
   - Context: Budget exhaustion deferral (Build 1109 freeze-proof)
   - Execution: Actor pattern (deferred continuation)

**Caller Analysis**:
- ✅ Only 2 external call sites (low coupling)
- ✅ Both use actor pattern (thread-safe)
- ⚠️ One synchronous call (line 228) - potential race condition?

### 4.2 Callees (What ManageCIT Calls)

**Direct Callees**: 22 symbols (high fan-out)

**Core Dependencies**:
1. `Print()` - Logging (8+ calls)
2. `entryOrders` - Unified order dictionary (read/write)
3. `activePositions` - Position tracking (read)
4. `_citNudgedKeys` - One-shot guard (read/write)
5. `_propagationActive` - Race suppression flag (read)
6. `Enqueue()` - Actor pattern (self-enqueue)
7. `Instrument.MasterInstrument.RoundToTickSize()` - Price rounding
8. `Account.Cancel()` - Follower order cancellation
9. `Account.CreateOrder()` - Follower order creation
10. `Account.Submit()` - Follower order submission
11. `ChangeOrder()` - Local order modification

**Dependency Categories**:
- **State Access**: entryOrders, activePositions, _citNudgedKeys, _propagationActive
- **Broker API**: Account.Cancel, Account.CreateOrder, Account.Submit, ChangeOrder
- **Utilities**: Print, RoundToTickSize, Enqueue
- **Constants**: MaxBrokerCallsPerCycle, ChaseIfTouchPoints

---

## 5. Blast Radius Analysis

**Impact Scope**: **MINIMAL** (Risk Score: 0.0)

| Metric | Value |
|--------|-------|
| **Direct Dependents** | 0 |
| **Importer Count** | 0 |
| **Confirmed Files** | 0 |
| **Potential Files** | 0 |
| **Overall Risk Score** | 0.0 |

**Analysis**:
- ✅ Private method (no external visibility)
- ✅ Only 2 call sites (both in same codebase)
- ✅ No cross-module dependencies
- ✅ Changes are fully contained within Orders.Management module

**Extraction Safety**: **HIGH** - Changes will not ripple beyond immediate callers.

---

## 6. Related Symbols (Co-Located Logic)

**Same File** (V12_002.Orders.Management.Flatten.cs):

1. `SyncPositionState()` - Position cleanup (line 40)
2. `FlattenAll()` - Master flatten orchestrator (line 167)
3. `HandleGhostPositionCleanup()` - Orphan position handling (line 200)
4. `CancelMasterEntryOrders()` - Entry cancellation (line 221)
5. `DispatchFleetFlatten()` - Fleet flatten dispatch (line 236)
6. `ResetSyncStateAndPurgeFollowers()` - State reset (line 244)
7. `FlattenFilledMasterPositions()` - Filled position flatten (line 263)
8. `FlattenSinglePosition()` - Single position flatten (line 278)
9. `CancelUnfilledMasterEntries()` - Unfilled entry cancel (line 348)

**Relatedness Score**: 3.0 (all co-located in same module)

**Module Cohesion**: HIGH - All methods are part of Orders.Management.Flatten subsystem.

---

## 7. Extraction Recommendations

### 7.1 Primary Extraction Targets

Based on complexity analysis, extract these logical blocks:

#### **Helper 1: ValidateCitConfiguration**
**Lines**: 70-85  
**Purpose**: Early validation and configuration parsing  
**Complexity Reduction**: ~3 CYC  
**Signature**:
```csharp
private bool ValidateCitConfiguration(out double citOffset)
```

**Logic**:
- Check activePositions/entryOrders count
- Check ChaseIfTouchPoints parameter
- Check _propagationActive flag
- Parse citOffset value
- Return false if any validation fails

#### **Helper 2: ShouldChaseOrder**
**Lines**: 93-114  
**Purpose**: Determine if order should be chased  
**Complexity Reduction**: ~5 CYC  
**Signature**:
```csharp
private bool ShouldChaseOrder(Order order, string key, out double currentPrice, out double limitPrice)
```

**Logic**:
- Check order state (Working)
- Check order type (Limit)
- Check if already nudged (_citNudgedKeys)
- Determine current price (Low[0] vs High[0])
- Evaluate trigger condition (directional bar-price logic)

#### **Helper 3: CalculateNudgedPrice**
**Lines**: 123-128  
**Purpose**: Calculate new limit price after nudge  
**Complexity Reduction**: ~2 CYC  
**Signature**:
```csharp
private double CalculateNudgedPrice(Order order, double citOffset)
```

**Logic**:
- Get tick size
- Calculate nudge distance
- Apply directional adjustment (Buy vs Sell)
- Round to tick size

#### **Helper 4: ExecuteFollowerNudge**
**Lines**: 130-173  
**Purpose**: Execute nudge for fleet follower orders  
**Complexity Reduction**: ~8 CYC  
**Signature**:
```csharp
private void ExecuteFollowerNudge(string key, Order order, double newLimitPrice, PositionInfo pos, ref int brokerBudget)
```

**Logic**:
- Check broker budget
- Cancel existing order
- Create nudged order
- Submit nudged order
- Update entryOrders dictionary
- Handle budget exhaustion (self-enqueue)

#### **Helper 5: ExecuteLocalNudge**
**Lines**: 174-181  
**Purpose**: Execute nudge for local account orders  
**Complexity Reduction**: ~2 CYC  
**Signature**:
```csharp
private void ExecuteLocalNudge(string key, Order order, double newLimitPrice)
```

**Logic**:
- Log nudge action
- Call ChangeOrder()

### 7.2 Extraction Strategy

**Approach**: **Bottom-Up Extraction** (smallest to largest)

**Order**:
1. CalculateNudgedPrice (simplest, pure function)
2. ValidateCitConfiguration (early return logic)
3. ShouldChaseOrder (decision logic)
4. ExecuteLocalNudge (simple action)
5. ExecuteFollowerNudge (complex action with budget management)

**Expected Outcome**:
- **Before**: CYC=26, LOC=98, Nesting=6
- **After**: CYC≤15, LOC≤60, Nesting≤4

**Complexity Reduction**: ~20 CYC → ~15 CYC (42% reduction)

### 7.3 Extraction Constraints

**V12 DNA Compliance**:
- ✅ No locks (already lock-free via actor pattern)
- ✅ ASCII-only (no Unicode in strings)
- ✅ Zero logic drift (pure structural extraction)
- ✅ Maintain actor pattern (Enqueue calls preserved)

**Jane Street Alignment**:
- ✅ Cognitive simplicity (each helper has single responsibility)
- ✅ Testability (helpers can be unit tested independently)
- ✅ Correctness by construction (type-safe signatures)

---

## 8. Risk Assessment

### 8.1 Extraction Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| **Logic Drift** | HIGH | Use Python extractor script (v12_split.py) |
| **Race Conditions** | MEDIUM | Preserve actor pattern (Enqueue calls) |
| **Broker Budget** | MEDIUM | Pass brokerBudget by ref, maintain state |
| **Order State** | MEDIUM | Preserve entryOrders dictionary updates |
| **Test Coverage** | LOW | Reuse ProcessOnOrderUpdate TDD infrastructure |

### 8.2 Business Impact

**Criticality**: **HIGH** - Chase-If-Touch is a core trading feature

**Failure Modes**:
- Orders not chased when price touches limit
- Orders chased multiple times (one-shot guard failure)
- Broker budget exhaustion causing strategy freeze
- Follower orders not synchronized with local orders

**Mitigation**:
- ✅ Comprehensive unit tests (reuse EPIC-CCN-10 patterns)
- ✅ Integration tests with mock broker API
- ✅ Manual testing in NinjaTrader Sim101 account

### 8.3 Overall Risk Level

**MEDIUM** (Complexity × Churn × Blast Radius)

**Calculation**:
- Complexity: HIGH (CYC=26)
- Churn: STABLE (0.86/week)
- Blast Radius: MINIMAL (0 dependents)

**Risk = HIGH × STABLE × MINIMAL = MEDIUM**

**Recommendation**: **PROCEED WITH CAUTION**
- Use TDD (write tests first)
- Extract incrementally (one helper at a time)
- Verify after each extraction (build + test)
- Manual testing in Sim101 after all extractions

---

## 9. Test Strategy

### 9.1 Reusable Infrastructure (from EPIC-CCN-10)

**Mock Types** (already implemented):
- `MockOrder` - Order state simulation
- `MockAccount` - Broker API simulation
- `MockInstrument` - Instrument/tick size simulation
- `MockPositionInfo` - Position tracking simulation

**Test Patterns**:
- Arrange-Act-Assert
- State machine verification
- Actor pattern validation

### 9.2 New Test Cases Required

**ValidateCitConfiguration Tests**:
1. Empty activePositions + entryOrders → returns false
2. ChaseIfTouchPoints = "0" → returns false
3. ChaseIfTouchPoints = "" → returns false
4. _propagationActive = true → returns false
5. Valid citOffset → returns true, out parameter set

**ShouldChaseOrder Tests**:
1. Order state != Working → returns false
2. Order type != Limit → returns false
3. Already nudged (_citNudgedKeys) → returns false
4. Long entry, Low[0] > limitPrice → returns false
5. Long entry, Low[0] <= limitPrice → returns true
6. Short entry, High[0] < limitPrice → returns false
7. Short entry, High[0] >= limitPrice → returns true

**CalculateNudgedPrice Tests**:
1. Buy order → limitPrice + (citOffset × tickSize)
2. Sell order → limitPrice - (citOffset × tickSize)
3. Rounding to tick size

**ExecuteFollowerNudge Tests**:
1. Budget exhausted → self-enqueue, return early
2. Cancel + Submit → budget decremented by 2
3. CreateOrder returns null → log error, continue
4. entryOrders dictionary updated with nudged order

**ExecuteLocalNudge Tests**:
1. ChangeOrder called with correct parameters
2. Log message contains correct prices

### 9.3 Integration Tests

**End-to-End Scenarios**:
1. Long entry touched → nudged toward market
2. Short entry touched → nudged toward market
3. Multiple orders → budget management
4. Follower + Local orders → both nudged correctly
5. Budget exhaustion → deferred continuation

---

## 10. Next Steps (Stage 1: Vision/Spec)

**Handoff to Architect** (Bob CLI continues):

1. **Review Forensics** - Validate findings with Director
2. **Generate mini-spec.md** - Detailed extraction plan
3. **Create Mermaid Diagrams** - Visual flow of helpers
4. **TDD Test Plan** - Comprehensive test coverage
5. **DNA Verification** - Ensure V12 compliance

**Estimated Effort**:
- Stage 1 (Vision/Spec): 30 minutes
- Stage 2 (Arch Planning): 45 minutes
- Stage 3 (DNA Audit): 15 minutes
- Stage 4 (Execution): 60 minutes
- Stage 5 (Verification): 30 minutes
- **Total**: ~3 hours

**Dependencies**:
- ✅ EPIC-CCN-10 complete (TDD infrastructure available)
- ✅ Forensics complete (this document)
- ⏳ Director approval to proceed

---

## Appendix A: Method Source Code

```csharp
/// <summary>
/// V12 SIMA: Chase If Touch - iterates the unified entryOrders dictionary which contains
/// BOTH local and fleet follower limit orders. When price touches a working limit entry
/// that was not filled, the limit is nudged N ticks toward market (citOffset * TickSize)
/// exactly once per order lifetime. Local orders: ChangeOrder() to new limit price.
/// Follower orders: cancel + resubmit as OrderType.Limit at new price via ExecutingAccount.
/// Re-nudging is prevented by _citNudgedKeys one-shot guard, cleared on fill or cancel.
/// </summary>
private void ManageCIT()
{
    if (activePositions.Count == 0 && entryOrders.Count == 0)
        return;
    if (string.IsNullOrEmpty(ChaseIfTouchPoints) || ChaseIfTouchPoints == "0")
        return;

    // [BUILD 924 -- Fix C] Suppress CIT during price-move propagation to prevent
    // race-fire on freshly resubmitted follower limit orders before sync cycle completes.
    if (_propagationActive)
    {
        Print("[CIT] Suppressed during price-move propagation (Build 924 Fix C)");
        return;
    }

    double citOffset = 0;
    if (!double.TryParse(ChaseIfTouchPoints, out citOffset))
        return;

    int _citBrokerBudget = MaxBrokerCallsPerCycle; // 5 calls max per cycle (constant at V12_002.cs:303)
    // Iterate ALL entry orders in the unified dictionary (local + every fleet account)
    foreach (var kvp in entryOrders.ToArray())
    {
        string key = kvp.Key;
        Order order = kvp.Value;
        if (order == null || order.OrderState != OrderState.Working)
            continue;
        if (order.OrderType != OrderType.Limit)
            continue; // only chase limit entries
        if (_citNudgedKeys.ContainsKey(key))
            continue; // [BUILD 949] one-shot: already nudged

        // [BUILD 984 CIT FIX] Correct directional bar-price logic:
        // - LONG entry (Buy): price must DROP DOWN to the limit -> compare Low[0] <= limitPrice
        // - SHORT entry (Sell): price must RISE UP to the limit -> compare High[0] >= limitPrice
        // Previous bug: Short used Low[0] <= limitPrice which is ALWAYS true when clicking
        // far above the current market, causing instant market conversion on every click.
        double currentPrice = (order.OrderAction == OrderAction.Buy) ? Low[0] : High[0];
        double limitPrice = order.LimitPrice;

        bool triggerChase =
            (order.OrderAction == OrderAction.Buy)
                ? (currentPrice <= limitPrice) // Long: bar low touched or pierced the limit
                : (currentPrice >= limitPrice); // Short: bar high touched or pierced the limit

        if (!triggerChase)
            continue;

        // Determine local vs follower
        PositionInfo pos = null;
        activePositions.TryGetValue(key, out pos);
        bool isFollower = pos != null && pos.IsFollower && pos.ExecutingAccount != null;

        try
        {
            double tickSize = Instrument.MasterInstrument.TickSize;
            double nudgeDistance = citOffset * tickSize;
            double newLimitPrice =
                (order.OrderAction == OrderAction.Buy)
                    ? Instrument.MasterInstrument.RoundToTickSize(limitPrice + nudgeDistance)
                    : Instrument.MasterInstrument.RoundToTickSize(limitPrice - nudgeDistance);

            if (isFollower)
            {
                // Fleet follower: cancel limit, resubmit as nudged limit via account API
                Account followerAcct = pos.ExecutingAccount;
                Print(
                    $"[CIT] FLEET nudge: {key} on {followerAcct.Name} | {limitPrice:F2} -> {newLimitPrice:F2} ({citOffset} ticks toward mkt)"
                );

                // Build 1109 [FREEZE-PROOF]: Budget broker calls to prevent strategy thread stall
                if (_citBrokerBudget <= 0)
                {
                    Print("[CIT] Broker budget exhausted -- deferring remaining nudges");
                    Enqueue(ctx => ctx.ManageCIT());
                    return;
                }
                _citBrokerBudget -= 2; // Cancel + Submit = 2 broker calls

                followerAcct.Cancel(new[] { order });

                Order nudgedOrder = followerAcct.CreateOrder(
                    Instrument,
                    order.OrderAction,
                    OrderType.Limit,
                    TimeInForce.Gtc,
                    order.Quantity,
                    newLimitPrice,
                    0,
                    "",
                    "CIT_" + key,
                    null
                );
                if (nudgedOrder == null)
                {
                    Print(
                        $"[CIT] ERROR: CreateOrder returned null for {key} on {followerAcct.Name} -- nudge aborted"
                    );
                    continue;
                }
                followerAcct.Submit(new[] { nudgedOrder });

                // B966: No Enqueue needed -- ManageCIT is always called via Enqueue(ctx => ctx.ManageCIT())
                // from OnBarUpdate (Phase C), so this write is already inside the actor drain.
                entryOrders[key] = nudgedOrder;
            }
            else
            {
                // Local account: ChangeOrder moves limit N ticks toward market
                Print(
                    $"[CIT] LOCAL nudge: {key} | {limitPrice:F2} -> {newLimitPrice:F2} ({citOffset} ticks toward mkt)"
                );
                ChangeOrder(order, order.Quantity, newLimitPrice, 0);
            }
            _citNudgedKeys.TryAdd(key, true); // [BUILD 949] one-shot: mark as nudged
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("ChangeOrder"))
        {
            Print($"[CIT] WARNING chasing {key} (known quirk): {ex.Message}");
        }
        catch (Exception ex)
        {
            Print($"[CIT] CRITICAL chasing {key}: {ex.ToString()}");
            // Do NOT rethrow - remaining fleet accounts still need flattening
        }
    }
}
```

---

**End of Forensic Report**

**Status**: ✅ COMPLETE - Ready for Stage 1 (Vision/Spec)  
**Next Action**: Director approval to proceed with extraction planning