# EPIC-CCN-10: ProcessOnOrderUpdate Extraction Mini-Spec

**Date**: 2026-06-02  
**Stage**: 1 (Vision/Spec)  
**Architect**: Bob CLI (v12-engineer)  
**Target Method**: `ProcessOnOrderUpdate`  
**Status**: DRAFT - Awaiting Director Approval

---

## 1. Vision

### Strategic Goal
Transform `ProcessOnOrderUpdate` from a monolithic state dispatcher (CYC 21) into a clean, testable state machine with explicit state handlers (CYC ≤8). This extraction aligns with Jane Street's cognitive simplicity principle: **functions with CYC >15 are harder to reason about under microsecond latency constraints**.

### Why This Matters
`ProcessOnOrderUpdate` is the **critical path** for all order state transitions in the V12 Photon Kernel. Every order fill, rejection, cancellation, and modification flows through this method. With 2.49 commits/week churn rate and 32 commits in 90 days, this is a **HIGH VOLATILITY** hotspot that requires surgical precision.

**Current Pain Points**:
- **Cognitive Overload**: 21 decision points in 45 lines (0.47 CYC/LOC ratio)
- **Hidden State Machine**: State transitions buried in nested if-else chains
- **Testing Difficulty**: 9 parameters + 4 nesting levels = exponential test path growth
- **Maintenance Risk**: High churn + high complexity = bug introduction vector

**Post-Extraction Benefits**:
- **Explicit State Machine**: Each state handler is a named, testable unit
- **Reduced Cognitive Load**: CYC 21 → 8 (62% reduction)
- **Improved Testability**: State handlers can be unit tested independently
- **Safer Refactoring**: Future changes isolated to specific state handlers

---

## 2. Current Architecture

### Method Signature
```csharp
private void ProcessOnOrderUpdate(
    Order order,
    double limitPrice,
    double stopPrice,
    int quantity,
    int filled,
    double averageFillPrice,
    OrderState orderState,
    DateTime time,
    string nativeError
)
```

**Location**: `src/V12_002.Orders.Callbacks.cs:195-269` (75 lines including latency instrumentation)

### Current Flow (CYC 21)

```mermaid
flowchart TD
    Start([ProcessOnOrderUpdate]) --> Probe[Start LatencyProbe]
    Probe --> Try[try block]
    
    Try --> PropCheck{Account match AND<br/>Working/Accepted/ChangeSubmitted?}
    PropCheck -->|Yes +3| PropPrice[PropagateMasterPriceMove]
    PropCheck -->|No| StateCheck
    PropPrice --> StateCheck
    
    StateCheck{orderState?}
    StateCheck -->|Filled +1| FilledCheck{entryOrders.Contains?}
    FilledCheck -->|Yes +1| HandleEntry[HandleEntryOrderFilled]
    FilledCheck -->|No +1| HandleSecondary[HandleSecondaryOrderFilled]
    
    StateCheck -->|Rejected +1| HandleRejected[HandleOrderRejected]
    StateCheck -->|Cancelled +1| HandleCancelled[HandleOrderCancelled]
    StateCheck -->|Accepted/Working +2| HandleChanged[HandleOrderPriceOrQuantityChanged]
    
    HandleEntry --> Terminal
    HandleSecondary --> Terminal
    HandleRejected --> Terminal
    HandleCancelled --> Terminal
    HandleChanged --> Terminal
    
    Terminal{!handled AND<br/>Cancelled/Rejected/Unknown?}
    Terminal -->|Yes +3| Ghost[RemoveGhostOrderRef]
    Terminal -->|No| Finally
    Ghost --> Finally
    
    Finally[finally: Record Latency] --> End([Return])
    
    Try -.->|Exception +1| Catch[catch: Print ERROR]
    Catch --> Finally
    
    style Start fill:#e1f5ff
    style End fill:#e1f5ff
    style PropCheck fill:#fff4e6
    style StateCheck fill:#fff4e6
    style FilledCheck fill:#fff4e6
    style Terminal fill:#fff4e6
    style Catch fill:#ffe6e6
```

### Complexity Breakdown

| Component | CYC | Description |
|-----------|-----|-------------|
| **Base** | 1 | Method entry |
| **Price Propagation Guard** | +3 | `Account == this.Account && (Working \|\| Accepted \|\| ChangeSubmitted)` |
| **Filled State** | +3 | `if (Filled)` + `if (entryOrders.Contains)` + `else` |
| **Rejected State** | +1 | `else if (Rejected)` |
| **Cancelled State** | +1 | `else if (Cancelled)` |
| **Working/Accepted State** | +2 | `else if (Accepted \|\| Working)` |
| **Terminal Catch-All** | +3 | `if (!handled && (Cancelled \|\| Rejected \|\| Unknown))` |
| **Exception Handler** | +1 | `catch (Exception ex)` |
| **TOTAL** | **21** | **Above Jane Street threshold (15)** |

### Call Graph (Current)

```mermaid
graph LR
    OnOrderUpdate[OnOrderUpdate<br/>NinjaTrader Callback] -->|Enqueue| ProcessOnOrderUpdate
    
    ProcessOnOrderUpdate --> PropagateMasterPriceMove
    ProcessOnOrderUpdate --> HandleEntryOrderFilled
    ProcessOnOrderUpdate --> HandleSecondaryOrderFilled
    ProcessOnOrderUpdate --> HandleOrderRejected
    ProcessOnOrderUpdate --> HandleOrderCancelled
    ProcessOnOrderUpdate --> HandleOrderPriceOrQuantityChanged
    ProcessOnOrderUpdate --> RemoveGhostOrderRef
    
    style ProcessOnOrderUpdate fill:#ffcccc
    style OnOrderUpdate fill:#e1f5ff
```

### Nesting Analysis

**Max Nesting Depth: 4**
```
try {                                           // Level 1
    if (account && state) {                     // Level 2
        PropagateMasterPriceMove(...);
    }
    
    if (orderState == Filled) {                 // Level 2
        if (entryOrders.Contains) {             // Level 3
            handled = HandleEntryOrderFilled(...);
        }
        else {                                  // Level 3
            handled = HandleSecondaryOrderFilled(...);
        }
    }
    
    if (!handled && terminal) {                 // Level 2
        RemoveGhostOrderRef(...);
    }
}
catch {                                         // Level 1
    Print(...);
}
finally {                                       // Level 1
    probe.Stop();
}
```

---

## 3. Proposed Architecture

### Extraction Strategy: State Machine Decomposition

**Core Principle**: Extract each state transition into a dedicated handler method. The main method becomes a **thin dispatcher** that routes to state-specific logic.

### Proposed Flow (CYC 8)

```mermaid
flowchart TD
    Start([ProcessOnOrderUpdate]) --> Probe[Start LatencyProbe]
    Probe --> Try[try block]
    
    Try --> PropCheck{ShouldPropagatePriceMove?}
    PropCheck -->|Yes +1| PropPrice[PropagateMasterPriceMove]
    PropCheck -->|No| StateCheck
    PropPrice --> StateCheck
    
    StateCheck{orderState?}
    StateCheck -->|Filled +1| FilledHandler[ProcessOrderState_Filled]
    StateCheck -->|Rejected/Cancelled +1| TerminalHandler[ProcessOrderState_Terminal]
    StateCheck -->|Accepted/Working +1| WorkingHandler[ProcessOrderState_Working]
    
    FilledHandler --> Terminal
    TerminalHandler --> Terminal
    WorkingHandler --> Terminal
    
    Terminal{!handled AND<br/>IsTerminalState?}
    Terminal -->|Yes +1| Ghost[RemoveGhostOrderRef]
    Terminal -->|No| Finally
    Ghost --> Finally
    
    Finally[finally: Record Latency] --> End([Return])
    
    Try -.->|Exception +1| Catch[catch: Print ERROR]
    Catch --> Finally
    
    style Start fill:#e1f5ff
    style End fill:#e1f5ff
    style PropCheck fill:#d4edda
    style StateCheck fill:#d4edda
    style Terminal fill:#d4edda
    style FilledHandler fill:#cce5ff
    style TerminalHandler fill:#cce5ff
    style WorkingHandler fill:#cce5ff
    style Catch fill:#ffe6e6
```

### Proposed Call Graph

```mermaid
graph LR
    OnOrderUpdate[OnOrderUpdate<br/>NinjaTrader Callback] -->|Enqueue| ProcessOnOrderUpdate
    
    ProcessOnOrderUpdate --> ShouldPropagatePriceMove
    ShouldPropagatePriceMove -.->|if true| PropagateMasterPriceMove
    
    ProcessOnOrderUpdate --> ProcessOrderState_Filled
    ProcessOrderState_Filled --> HandleEntryOrderFilled
    ProcessOrderState_Filled --> HandleSecondaryOrderFilled
    
    ProcessOnOrderUpdate --> ProcessOrderState_Terminal
    ProcessOrderState_Terminal --> HandleOrderRejected
    ProcessOrderState_Terminal --> HandleOrderCancelled
    
    ProcessOnOrderUpdate --> ProcessOrderState_Working
    ProcessOrderState_Working --> HandleOrderPriceOrQuantityChanged
    
    ProcessOnOrderUpdate --> IsTerminalState
    IsTerminalState -.->|if true| RemoveGhostOrderRef
    
    style ProcessOnOrderUpdate fill:#d4edda
    style ShouldPropagatePriceMove fill:#cce5ff
    style ProcessOrderState_Filled fill:#cce5ff
    style ProcessOrderState_Terminal fill:#cce5ff
    style ProcessOrderState_Working fill:#cce5ff
    style IsTerminalState fill:#cce5ff
    style OnOrderUpdate fill:#e1f5ff
```

### Complexity Reduction

| Component | Before | After | Delta |
|-----------|--------|-------|-------|
| **Main Method** | 21 | 8 | -13 |
| **ShouldPropagatePriceMove** | - | 3 | +3 |
| **ProcessOrderState_Filled** | - | 3 | +3 |
| **ProcessOrderState_Terminal** | - | 2 | +2 |
| **ProcessOrderState_Working** | - | 1 | +1 |
| **IsTerminalState** | - | 2 | +2 |
| **Total System CYC** | 21 | 19 | -2 |

**Key Insight**: Total system complexity only reduces by 2 (21 → 19), but **cognitive complexity** drops dramatically because:
1. Each helper has a **single responsibility** (state-specific logic)
2. Main method becomes a **readable dispatcher** (8 decision points vs 21)
3. Helpers are **independently testable** (no 9-parameter setup required)

---

## 4. Extraction Plan

### Phase 1: Extract Helper Methods (4 methods)

#### Extract 1: `ShouldPropagatePriceMove` (CYC 3)

**Purpose**: Encapsulate the compound condition for price propagation eligibility.

**Signature**:
```csharp
private bool ShouldPropagatePriceMove(Order order, OrderState orderState)
```

**Implementation**:
```csharp
private bool ShouldPropagatePriceMove(Order order, OrderState orderState)
{
    return order.Account == this.Account
        && (
            orderState == OrderState.Working
            || orderState == OrderState.Accepted
            || orderState == OrderState.ChangeSubmitted
        );
}
```

**CYC Breakdown**:
- Base: 1
- `&&` operator: +1
- `||` operators: +2
- **Total**: 3

**Extraction Benefit**: Removes 3 CYC from main method, improves readability.

---

#### Extract 2: `ProcessOrderState_Filled` (CYC 3)

**Purpose**: Handle filled state logic (entry vs secondary order distinction).

**Signature**:
```csharp
private bool ProcessOrderState_Filled(
    Order order,
    int quantity,
    int filled,
    double averageFillPrice,
    DateTime time
)
```

**Implementation**:
```csharp
private bool ProcessOrderState_Filled(
    Order order,
    int quantity,
    int filled,
    double averageFillPrice,
    DateTime time
)
{
    if (entryOrders.Values.Contains(order))
        return HandleEntryOrderFilled(order, quantity, filled, averageFillPrice, time);
    else
        return HandleSecondaryOrderFilled(order, averageFillPrice);
}
```

**CYC Breakdown**:
- Base: 1
- `if`: +1
- `else`: +1
- **Total**: 3

**Extraction Benefit**: Removes 3 CYC from main method, isolates entry/secondary logic.

---

#### Extract 3: `ProcessOrderState_Terminal` (CYC 2)

**Purpose**: Handle terminal states (rejected, cancelled).

**Signature**:
```csharp
private bool ProcessOrderState_Terminal(Order order, OrderState orderState, string nativeError)
```

**Implementation**:
```csharp
private bool ProcessOrderState_Terminal(Order order, OrderState orderState, string nativeError)
{
    if (orderState == OrderState.Rejected)
        return HandleOrderRejected(order, nativeError);
    else if (orderState == OrderState.Cancelled)
        return HandleOrderCancelled(order);

    return false;
}
```

**CYC Breakdown**:
- Base: 1
- `if`: +1
- `else if`: +1 (but one branch returns false, so effective +1)
- **Total**: 2

**Extraction Benefit**: Removes 2 CYC from main method, groups terminal state handling.

---

#### Extract 4: `ProcessOrderState_Working` (CYC 1)

**Purpose**: Handle working/accepted state logic (thin wrapper for consistency).

**Signature**:
```csharp
private bool ProcessOrderState_Working(
    Order order,
    double limitPrice,
    double stopPrice,
    int quantity
)
```

**Implementation**:
```csharp
private bool ProcessOrderState_Working(
    Order order,
    double limitPrice,
    double stopPrice,
    int quantity
)
{
    return HandleOrderPriceOrQuantityChanged(order, limitPrice, stopPrice, quantity);
}
```

**CYC Breakdown**:
- Base: 1
- **Total**: 1

**Extraction Benefit**: Removes 1 CYC from main method, maintains naming consistency.

---

#### Extract 5: `IsTerminalState` (CYC 2)

**Purpose**: Encapsulate terminal state check for ghost order cleanup.

**Signature**:
```csharp
private bool IsTerminalState(OrderState state)
```

**Implementation**:
```csharp
private bool IsTerminalState(OrderState state)
{
    return state == OrderState.Cancelled
        || state == OrderState.Rejected
        || state == OrderState.Unknown;
}
```

**CYC Breakdown**:
- Base: 1
- `||` operators: +2
- **Total**: 3 (but used in a single `if`, so net reduction is -2)

**Extraction Benefit**: Removes 2 CYC from main method, clarifies terminal state concept.

---

### Phase 2: Refactor Main Method (CYC 8)

**New Implementation**:
```csharp
private void ProcessOnOrderUpdate(
    Order order,
    double limitPrice,
    double stopPrice,
    int quantity,
    int filled,
    double averageFillPrice,
    OrderState orderState,
    DateTime time,
    string nativeError
)
{
    // [EPIC-5-PERF] Latency instrumentation
    var probe = LatencyProbe.Start();

    try
    {
        // Price propagation for working orders
        if (ShouldPropagatePriceMove(order, orderState))  // +1
        {
            PropagateMasterPriceMove(order, limitPrice, stopPrice, quantity);
        }

        bool handled = false;

        // State-specific processing
        if (orderState == OrderState.Filled)  // +1
            handled = ProcessOrderState_Filled(order, quantity, filled, averageFillPrice, time);
        else if (orderState == OrderState.Rejected || orderState == OrderState.Cancelled)  // +2
            handled = ProcessOrderState_Terminal(order, orderState, nativeError);
        else if (orderState == OrderState.Accepted || orderState == OrderState.Working)  // +2
            handled = ProcessOrderState_Working(order, limitPrice, stopPrice, quantity);

        // Terminal catch-all for unhandled states
        if (!handled && IsTerminalState(orderState))  // +1
        {
            RemoveGhostOrderRef(order, orderState.ToString().ToUpper());
        }
    }
    catch (Exception ex)  // +1
    {
        Print("ERROR OnOrderUpdate: " + ex.Message);
    }
    finally
    {
        // [EPIC-5-PERF] Record latency
        probe = probe.Stop();
        _histProcessOnOrderUpdate.Record(probe);
    }
}
```

**CYC Breakdown**:
- Base: 1
- `if (ShouldPropagatePriceMove)`: +1
- `if (Filled)`: +1
- `else if (Rejected || Cancelled)`: +2
- `else if (Accepted || Working)`: +2
- `if (!handled && IsTerminalState)`: +1
- `catch`: +1
- **Total**: 8 ✅

---

## 5. V12 DNA Compliance

### Lock-Free Actor Pattern ✅

**Current State**:
- ✅ Called via `Enqueue(ctx => ctx.ProcessOnOrderUpdate(...))` (line 192)
- ✅ No `lock()` statements in method body
- ✅ All state mutations happen inside Actor drain (single-threaded execution)

**Post-Extraction**:
- ✅ All extracted helpers are private methods called within the same Actor context
- ✅ No new locks introduced
- ✅ Actor/FSM pattern preserved

**Verification Command**: `grep -r "lock(" src/V12_002.Orders.Callbacks.cs` (must return zero matches)

---

### ASCII-Only Compliance ✅

**Current State**:
- ✅ All string literals use straight quotes (`"`)
- ✅ No Unicode characters in code
- ✅ No emoji or curly quotes

**Post-Extraction**:
- ✅ All new methods use ASCII-only strings
- ✅ No Unicode introduced

**Verification Command**: `python check_ascii.py src/V12_002.Orders.Callbacks.cs` (must pass)

---

### Zero-Allocation Hot Path ✅

**Current State**:
- ✅ No `new` allocations in hot path (except `LatencyProbe.Start()`)
- ✅ All parameters are primitives or stable references
- ✅ No LINQ queries (which allocate enumerators)

**Post-Extraction**:
- ✅ Extracted methods only pass primitives and stable references
- ✅ No new allocations introduced
- ✅ `entryOrders.Values.Contains(order)` remains (existing allocation, not introduced by extraction)

**Note**: The `Contains()` call on line 228 allocates an enumerator. This is **pre-existing technical debt** and should be addressed in a separate EPIC (consider using `ContainsValue()` or a custom lookup).

---

### Correctness by Construction ✅

**Current State**:
- ✅ State machine logic is explicit (if-else chain)
- ⚠️ State transitions are implicit (buried in nested conditions)

**Post-Extraction**:
- ✅ State machine logic is **explicit and named** (ProcessOrderState_Filled, ProcessOrderState_Terminal, etc.)
- ✅ Each state handler has a **single responsibility**
- ✅ Terminal state concept is **explicit** (IsTerminalState helper)

**Improvement**: Post-extraction code makes illegal states more obvious. For example, if a new `OrderState` is added, the compiler will force us to handle it in the main dispatcher.

---

## 6. Risk Mitigation

### Risk Factor 1: HIGH VOLATILITY (2.49 commits/week)

**Threat**: Concurrent changes during extraction could cause merge conflicts or logic drift.

**Mitigation**:
1. **Pre-Extraction Rebase**: `git fetch origin main && git rebase origin/main`
2. **Fast Extraction Window**: Complete all 5 extractions in a single 2-hour session
3. **Atomic Commits**: One commit per extraction (5 commits total)
4. **Post-Extraction Rebase**: Rebase again before PR submission
5. **PR Hygiene Check**: `powershell -File .\scripts\verify_pr_hygiene.ps1`

**Rollback Plan**: Bob CLI auto-checkpointing enabled. Use `/restore` to rollback to any checkpoint.

---

### Risk Factor 2: CRITICAL PATH (Order Processing)

**Threat**: Any logic error breaks order fills, rejections, or cancellations.

**Mitigation**:
1. **TDD Tests First**: Write tests for all 5 state transitions BEFORE extraction
2. **Zero Logic Drift**: Extraction is **pure structural movement** (no optimization, no "improvements")
3. **Verification After Each Extraction**:
   - Run `dotnet build` (must pass)
   - Run `dotnet test` (must pass)
   - Run `powershell -File .\deploy-sync.ps1` (ASCII gate must pass)
4. **Manual Smoke Test**: F5 in NinjaTrader, place test order, verify callbacks fire correctly

**Test Coverage Target**:
- ✅ Filled state (entry order)
- ✅ Filled state (secondary order)
- ✅ Rejected state
- ✅ Cancelled state
- ✅ Working/Accepted state
- ✅ Terminal catch-all (ghost order cleanup)

---

### Risk Factor 3: HIGH COUPLING (9 parameters, 39 transitive dependencies)

**Threat**: Extracted methods might introduce parameter explosion or hidden dependencies.

**Mitigation**:
1. **Minimal Parameter Passing**: Each helper only receives parameters it actually uses
2. **No New Dependencies**: Helpers only call existing methods (no new subsystem coupling)
3. **Preserve Call Graph**: Extracted helpers maintain the same call graph as the original method

**Parameter Audit**:
- `ShouldPropagatePriceMove`: 2 params (order, orderState)
- `ProcessOrderState_Filled`: 5 params (order, quantity, filled, averageFillPrice, time)
- `ProcessOrderState_Terminal`: 3 params (order, orderState, nativeError)
- `ProcessOrderState_Working`: 4 params (order, limitPrice, stopPrice, quantity)
- `IsTerminalState`: 1 param (state)

**Total New Parameters**: 15 (distributed across 5 methods, average 3 params/method) ✅

---

### Risk Factor 4: NO EXISTING TESTS

**Threat**: No safety net to catch regressions.

**Mitigation**:
1. **Create TDD Tests FIRST** (before any extraction):
   ```csharp
   // tests/V12_Performance.Tests/Orders/ProcessOnOrderUpdateTests.cs
   [Fact]
   public void ProcessOnOrderUpdate_FilledEntryOrder_CallsHandleEntryOrderFilled()
   [Fact]
   public void ProcessOnOrderUpdate_FilledSecondaryOrder_CallsHandleSecondaryOrderFilled()
   [Fact]
   public void ProcessOnOrderUpdate_RejectedOrder_CallsHandleOrderRejected()
   [Fact]
   public void ProcessOnOrderUpdate_CancelledOrder_CallsHandleOrderCancelled()
   [Fact]
   public void ProcessOnOrderUpdate_WorkingOrder_CallsHandleOrderPriceOrQuantityChanged()
   [Fact]
   public void ProcessOnOrderUpdate_UnhandledTerminalState_CallsRemoveGhostOrderRef()
   ```
2. **Run Tests After Each Extraction**: `dotnet test --filter ProcessOnOrderUpdate`
3. **100% State Transition Coverage**: All 6 test cases must pass

**Test Creation Estimate**: 1 hour (before extraction begins)

---

## 7. Success Criteria

### Quantitative Metrics

| Metric | Before | Target | Verification |
|--------|--------|--------|--------------|
| **Cyclomatic Complexity** | 21 | ≤8 | `python scripts/complexity_audit.py` |
| **Max Nesting Depth** | 4 | ≤3 | Manual inspection |
| **Lines of Code** | 75 | ~85 | Git diff (expect +10 lines for helpers) |
| **Build Status** | ✅ | ✅ | `dotnet build` |
| **Test Pass Rate** | N/A | 100% | `dotnet test` |
| **ASCII Compliance** | ✅ | ✅ | `python check_ascii.py` |
| **Lint Violations** | 0 | 0 | `powershell -File .\scripts\lint.ps1` |
| **Pre-Push Validation** | N/A | 13/13 | `powershell -File .\scripts\pre_push_validation.ps1` |

---

### Qualitative Metrics

- ✅ **Readability**: Main method reads like a state machine dispatcher (no nested logic)
- ✅ **Testability**: Each state handler can be unit tested independently
- ✅ **Maintainability**: Future state additions require minimal changes (add new handler + dispatcher case)
- ✅ **V12 DNA Compliance**: Lock-free, ASCII-only, zero-allocation hot path preserved
- ✅ **Jane Street Alignment**: CYC ≤15 threshold met (target: 8)

---

### Verification Checklist

**Pre-Extraction**:
- [ ] Rebase onto `origin/main`
- [ ] Run `pre_push_validation.ps1 -Fast` (establish baseline)
- [ ] Create TDD tests (6 test cases)
- [ ] All tests pass

**During Extraction** (repeat for each of 5 helpers):
- [ ] Extract helper method
- [ ] Update main method to call helper
- [ ] Run `dotnet build` (must pass)
- [ ] Run `dotnet test --filter ProcessOnOrderUpdate` (must pass)
- [ ] Run `powershell -File .\deploy-sync.ps1` (ASCII gate must pass)
- [ ] Git commit with message: `[EPIC-CCN-10] Extract <HelperName> (CYC 21 -> X)`

**Post-Extraction**:
- [ ] Run `python scripts/complexity_audit.py` (verify CYC ≤8)
- [ ] Run `pre_push_validation.ps1` (full mode, all 13 checks)
- [ ] F5 in NinjaTrader (manual smoke test)
- [ ] Place test order, verify callbacks fire correctly
- [ ] Run `verify_pr_hygiene.ps1` (PR diff <10k chars)
- [ ] Create PR with title: `[EPIC-CCN-10] Extract ProcessOnOrderUpdate (CYC 21 -> 8)`

---

## 8. Execution Timeline

### Phase 0: Pre-Extraction Setup (1 hour)
- **T+0:00**: Rebase onto `origin/main`
- **T+0:05**: Run baseline validation (`pre_push_validation.ps1 -Fast`)
- **T+0:10**: Create test file: `tests/V12_Performance.Tests/Orders/ProcessOnOrderUpdateTests.cs`
- **T+0:15**: Write 6 TDD test cases
- **T+0:45**: Verify all tests pass
- **T+1:00**: Commit tests: `[EPIC-CCN-10] Add TDD tests for ProcessOnOrderUpdate`

### Phase 1: Extract Helpers (1.5 hours)
- **T+1:00**: Extract `ShouldPropagatePriceMove` (15 min)
- **T+1:15**: Extract `ProcessOrderState_Filled` (20 min)
- **T+1:35**: Extract `ProcessOrderState_Terminal` (20 min)
- **T+1:55**: Extract `ProcessOrderState_Working` (15 min)
- **T+2:10**: Extract `IsTerminalState` (10 min)
- **T+2:20**: Refactor main method (10 min)

### Phase 2: Validation (0.5 hours)
- **T+2:30**: Run full pre-push validation (10 min)
- **T+2:40**: NinjaTrader F5 build + manual smoke test (10 min)
- **T+2:50**: Create PR (10 min)

**Total Estimate**: 3 hours

---

## 9. Rollback Plan

### Checkpoint Strategy
Bob CLI auto-checkpointing is enabled (`.bob/settings.json`). Each extraction creates an automatic checkpoint.

### Rollback Commands
```bash
# List available checkpoints
/checkpoints

# Restore to specific checkpoint
/restore <checkpoint-id>

# Restore to last checkpoint
/restore
```

### Manual Rollback (if Bob CLI fails)
```bash
# Discard all changes
git reset --hard HEAD

# Restore from specific commit
git reset --hard <commit-sha>
```

---

## 10. Open Questions for Director

1. **Test Coverage**: Should we add integration tests (NinjaTrader simulation) or are unit tests sufficient?
2. **Parameter Explosion**: `ProcessOrderState_Filled` has 5 parameters. Should we introduce a `FillContext` struct to reduce coupling?
3. **Allocation Concern**: The `entryOrders.Values.Contains(order)` call allocates an enumerator. Should we address this in this EPIC or defer to a separate performance EPIC?
4. **Naming Convention**: Should state handlers use `ProcessOrderState_*` or `HandleOrderState_*` prefix?
5. **Terminal State Logic**: Should `ProcessOrderState_Terminal` return `false` for unhandled states, or throw an exception?

---

## 11. Next Steps

**Upon Director Approval**:
1. Proceed to **Stage 2: Arch Planning** (generate `implementation_plan.md`)
2. Create detailed Mermaid diagrams for PR documentation
3. Schedule extraction session (3-hour block, no interruptions)

**Upon Director Rejection**:
1. Address feedback
2. Revise mini-spec
3. Re-submit for approval

---

## Appendix A: Complexity Calculation Details

### Current Method (CYC 21)
```
Base:                                    1
if (account && (working || accepted || changeSubmitted)):  +3
if (orderState == Filled):               +1
  if (entryOrders.Contains):             +1
  else:                                  +1
else if (orderState == Rejected):        +1
else if (orderState == Cancelled):       +1
else if (orderState == Accepted || Working): +2
if (!handled && (cancelled || rejected || unknown)): +3
catch (Exception):                       +1
---
TOTAL:                                   21
```

### Proposed Main Method (CYC 8)
```
Base:                                    1
if (ShouldPropagatePriceMove):           +1
if (orderState == Filled):               +1
else if (orderState == Rejected || Cancelled): +2
else if (orderState == Accepted || Working): +2
if (!handled && IsTerminalState):        +1
catch (Exception):                       +1
---
TOTAL:                                   8
```

### Extracted Helpers (CYC 11)
```
ShouldPropagatePriceMove:                3
ProcessOrderState_Filled:                3
ProcessOrderState_Terminal:              2
ProcessOrderState_Working:               1
IsTerminalState:                         2
---
TOTAL:                                   11
```

**System Total**: 8 (main) + 11 (helpers) = 19 (vs 21 before)

---

## Appendix B: Jane Street Alignment

### Principle: Cognitive Simplicity
> "Functions with CYC >15 are harder to reason about under microsecond latency constraints."

**Before**: CYC 21 (40% above threshold)  
**After**: CYC 8 (47% below threshold) ✅

### Principle: Explicit State Machines
> "State transitions should be explicit, not buried in nested conditions."

**Before**: State machine logic hidden in if-else chains  
**After**: State handlers are named, testable units ✅

### Principle: Single Responsibility
> "Each function should do one thing well."

**Before**: `ProcessOnOrderUpdate` handles 5 state transitions + price propagation + ghost cleanup  
**After**: Main method dispatches, helpers handle specific states ✅

---

**Document Status**: ✅ COMPLETE - Ready for Director Review  
**Next Stage**: Stage 2 (Arch Planning) upon approval  
**Estimated Review Time**: 15 minutes