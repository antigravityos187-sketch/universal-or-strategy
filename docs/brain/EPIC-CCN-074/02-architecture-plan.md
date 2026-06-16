# Architecture Plan: EPIC-CCN-074

## Epic Overview
- **Target Method**: `AttachExecutionPanelHandlers`
- **File**: `src/V12_002.UI.Panel.Handlers.cs`
- **Current Complexity**: CYC 12 (50% over Jane Street threshold of 8)
- **Target Complexity**: CYC ≤8
- **Strategy**: Extract button handler groups into cohesive helper methods

## Current Method Signature

```csharp
private void AttachExecutionPanelHandlers()
```

**Characteristics**:
- No parameters (uses class-level UI component fields)
- No return value (void)
- Private scope (internal helper method)
- 54 lines of code
- 12 null-check branches (one per button)

## Complexity Analysis

### Current Metrics
- **Cyclomatic Complexity**: 12
- **Max Nesting Depth**: 2
- **Parameter Count**: 0
- **Lines of Code**: 54
- **Assessment**: HIGH (exceeds Jane Street threshold of 8)

### Complexity Sources
1. **10 null-check branches** (if statements): +10 CYC
2. **Base method**: +1 CYC
3. **Lambda expressions**: +1 CYC (implicit branching)

### Reduction Strategy
Extract button groups into 3 helper methods, each with CYC ≤4:
- **Group 1**: OR execution buttons (Long/Short) - CYC 3
- **Group 2**: Mode buttons (MOMO/FFMA/M) - CYC 4
- **Group 3**: Strategy buttons (Retest/RMA/Trend) - CYC 4

**Result**: Main method CYC = 4 (3 helper calls + base)

## Proposed Extracted Methods

### Method 1: AttachOrExecutionHandlers
```csharp
private void AttachOrExecutionHandlers()
{
    if (orLongButton != null)
        orLongButton.Click += (s, e) =>
        {
            PanelCommand("OR_LONG");
            ResetExecutionMode();
            TriggerGlow(CyanAccent);
        };
    if (orShortButton != null)
        orShortButton.Click += (s, e) =>
        {
            PanelCommand("OR_SHORT");
            ResetExecutionMode();
            TriggerGlow(PinkFg);
        };
}
```
**Complexity**: CYC 3 (2 null-checks + base)
**Purpose**: Isolate OR Long/Short execution button handlers
**Cohesion**: Both buttons trigger immediate execution with mode reset

### Method 2: AttachModeSelectionHandlers
```csharp
private void AttachModeSelectionHandlers()
{
    if (momoButton != null)
        momoButton.Click += (s, e) =>
        {
            PanelCommand("MODE_MOMO");
            ResetExecutionMode();
            TriggerGlow(GreenFg);
        };
    if (ffmaButton != null)
        ffmaButton.Click += (s, e) =>
        {
            PanelCommand("MODE_FFMA");
            ResetExecutionMode();
            TriggerGlow(PinkFg);
        };
    if (ffmaManualButton != null)
        ffmaManualButton.Click += (s, e) =>
        {
            PanelCommand("FFMA_MANUAL_MARKET");
            ResetExecutionMode();
            TriggerGlow(PinkFg);
        };
    if (mButton != null)
        mButton.Click += (s, e) =>
        {
            PanelCommand("MODE_M");
            TriggerGlow(OrangeFg);
        };
}
```
**Complexity**: CYC 5 (4 null-checks + base)
**Purpose**: Isolate mode selection button handlers
**Cohesion**: All buttons change trading mode (MOMO/FFMA/M)

### Method 3: AttachStrategyToggleHandlers
```csharp
private void AttachStrategyToggleHandlers()
{
    if (retestButton != null)
        retestButton.Click += OnRetestClick;
    if (retestRmaToggle != null)
        retestRmaToggle.Click += OnRetestRmaToggleClick;
    if (rmaButton != null)
        rmaButton.Click += OnRmaClick;
    if (trendButton != null)
        trendButton.Click += OnTrendClick;
    if (trendRmaToggle != null)
        trendRmaToggle.Click += OnTrendRmaToggleClick;
}
```
**Complexity**: CYC 6 (5 null-checks + base)
**Purpose**: Isolate strategy toggle button handlers
**Cohesion**: All buttons toggle strategy features (Retest/RMA/Trend)

### Refactored Main Method
```csharp
private void AttachExecutionPanelHandlers()
{
    AttachOrExecutionHandlers();
    AttachModeSelectionHandlers();
    AttachStrategyToggleHandlers();
}
```
**New Complexity**: CYC 1 (base only, no branches)
**Reduction**: 12 → 1 (92% complexity reduction)

## Call Graph

### Callers
- **AttachPanelHandlers()** (line 42 in same file)
  - Parent orchestrator method that calls all handler attachment methods
  - No other external callers found

### Internal Calls (from extracted methods)
- **PanelCommand(string)** - Dispatches command to strategy engine
- **ResetExecutionMode()** - Resets execution state after command
- **TriggerGlow(Color)** - Visual feedback for button press
- **OnRetestClick** - Existing event handler method
- **OnRetestRmaToggleClick** - Existing event handler method
- **OnRmaClick** - Existing event handler method
- **OnTrendClick** - Existing event handler method
- **OnTrendRmaToggleClick** - Existing event handler method

### State Access
**Read-Only Access** (UI component fields):
- orLongButton, orShortButton
- momoButton, ffmaButton, ffmaManualButton, mButton
- retestButton, retestRmaToggle, rmaButton
- trendButton, trendRmaToggle

**No Shared Mutable State** - All methods are side-effect free except for event subscription

## Extraction Strategy

### Step 1: Create Helper Method Stubs
1. Add three new private methods below `AttachExecutionPanelHandlers`
2. Copy method signatures from architecture plan
3. Leave bodies empty initially

### Step 2: Extract OR Execution Handlers
1. Cut lines 98-111 (orLongButton and orShortButton blocks)
2. Paste into `AttachOrExecutionHandlers` body
3. Verify indentation and formatting

### Step 3: Extract Mode Selection Handlers
1. Cut lines 120-145 (momoButton through mButton blocks)
2. Paste into `AttachModeSelectionHandlers` body
3. Verify indentation and formatting

### Step 4: Extract Strategy Toggle Handlers
1. Cut lines 112-119 and 146-149 (retest, rma, trend blocks)
2. Paste into `AttachStrategyToggleHandlers` body
3. Verify indentation and formatting

### Step 5: Replace with Helper Calls
1. Replace extracted code in main method with three helper calls
2. Verify method signature unchanged
3. Verify no compilation errors

### Step 6: Verify Complexity Reduction
1. Run complexity audit: `python scripts/complexity_audit.py`
2. Confirm `AttachExecutionPanelHandlers` CYC = 1
3. Confirm helper methods CYC ≤6

## Jane Street Compliance

### 1. Correctness by Construction ✅
- **Pure Event Subscription**: No state mutations, only event handler registration
- **Null-Safe**: All button accesses guarded by null checks
- **Type-Safe**: Event handler signatures match Click event delegate
- **No Illegal States**: Impossible to attach handler to null button

### 2. Lock-Free Actor Pattern ✅
- **Zero `lock()` Blocks**: No synchronization primitives used
- **Read-Only Access**: Only reads UI component fields, no writes
- **Event-Driven**: Uses .NET event model (inherently thread-safe)
- **No Shared Mutable State**: Each button is independent

### 3. ASCII-Only Compliance ✅
- **String Literals**: All command strings are ASCII ("OR_LONG", "MODE_MOMO", etc.)
- **No Unicode**: Zero emoji, curly quotes, or special characters
- **Method Names**: Pure ASCII identifiers

### 4. Cognitive Simplicity (Jane Street Standard) ✅
- **Target**: CYC ≤8 per method
- **Main Method**: CYC 1 (well below threshold)
- **Helper Methods**: CYC 3-6 (all below threshold)
- **Rationale**: Functions with CYC >8 are harder to:
  - Reason about under microsecond latency constraints
  - Test exhaustively (exponential path growth)
  - Audit for race conditions in lock-free code

## Risk Mitigation

### Risk 1: Event Handler Ordering
- **Risk**: Extracted methods might change handler attachment order
- **Mitigation**: Preserve exact line-by-line order during extraction
- **Validation**: Manual diff review before/after refactoring

### Risk 2: Null Reference Exceptions
- **Risk**: Button fields might be null at runtime
- **Mitigation**: All null checks preserved in extracted methods
- **Validation**: Existing null-safety pattern maintained

### Risk 3: Lambda Closure Scope
- **Risk**: Lambda expressions might capture wrong variables
- **Mitigation**: No local variables in original method, only class fields
- **Validation**: All lambdas reference class-level fields (safe)

### Risk 4: Compilation Errors
- **Risk**: Missing using directives or namespace issues
- **Mitigation**: No new types introduced, all references already in scope
- **Validation**: Build verification after each extraction step

## Testing Strategy

### Unit Tests (Recommended)
1. **Test Null Safety**: Verify no exceptions when buttons are null
2. **Test Event Subscription**: Verify Click events are attached
3. **Test Handler Execution**: Verify PanelCommand called with correct args
4. **Test Visual Feedback**: Verify TriggerGlow called with correct colors

### Integration Tests (Existing)
- **Manual F5 Test**: Load strategy in NinjaTrader, verify buttons work
- **Regression Test**: Verify no behavior changes after refactoring

## Complexity Reduction Summary

| Metric | Before | After | Change |
|--------|--------|-------|--------|
| **Main Method CYC** | 12 | 1 | -92% |
| **Helper 1 CYC** | - | 3 | +3 |
| **Helper 2 CYC** | - | 5 | +5 |
| **Helper 3 CYC** | - | 6 | +6 |
| **Total CYC** | 12 | 15 | +25% |
| **Max Method CYC** | 12 | 6 | -50% |
| **Jane Street Compliant** | ❌ NO | ✅ YES | PASS |

**Key Insight**: Total complexity increases slightly (+25%) but **cognitive load per method decreases dramatically** (-50% max). This is the Jane Street trade-off: distribute complexity across smaller, testable units rather than concentrate it in one God-function.

## Approval Checklist

- ✅ **Single Method Focus**: Only `AttachExecutionPanelHandlers` modified
- ✅ **No Scope Creep**: Zero changes to callers, callees, or sibling methods
- ✅ **Signature Preserved**: Method signature unchanged
- ✅ **Behavior Preserved**: Runtime behavior identical before/after
- ✅ **Lock-Free**: No synchronization primitives introduced
- ✅ **ASCII-Only**: All strings are ASCII
- ✅ **Jane Street Aligned**: All methods CYC ≤8
- ✅ **Low Risk**: Isolated change with minimal blast radius

## Phase 2 Status
- **Status**: COMPLETED
- **Date**: 2026-06-15
- **Architect**: V12 Phase 2 Architecture Planner
- **Next Phase**: Phase 3 (DNA & PR Audit)