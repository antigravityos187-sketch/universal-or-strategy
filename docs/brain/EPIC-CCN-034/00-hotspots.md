# Phase 0: Hotspot Analysis - EPIC-CCN-034

## Target Method
- **Method**: ManageCIT
- **File**: src/V12_002.Orders.Management.Flatten.cs
- **Cyclomatic Complexity**: 19

## Complexity Metrics

### Method Signature
private void ManageCIT(Order order, int quantity, OrderAction orderAction, string oco)

### Complexity Breakdown
- **Cyclomatic Complexity**: 19
- **Threshold**: 15 (Jane Street alignment)
- **Violation**: +4 over threshold
- **Risk Level**: MEDIUM-HIGH

### Code Characteristics
- Multiple conditional branches for order state management
- CIT (Cancel-If-Touched) order logic
- OCO (One-Cancels-Other) handling
- Order quantity and action validation
- State machine transitions

## Blast Radius Analysis

### Direct Dependencies
**File**: src/V12_002.Orders.Management.Flatten.cs
- Part of order management subsystem
- Interacts with order state machine
- Modifies order collections
- Triggers order lifecycle events

### Potential Impact Areas
1. **Order State Machine**: Changes may affect FSM transitions
2. **Order Collections**: Modifications to order tracking structures
3. **Event Handlers**: Order lifecycle event propagation
4. **OCO Logic**: One-Cancels-Other order coordination
5. **Position Management**: Indirect impact on position tracking

### Risk Assessment
- **Compilation Risk**: LOW (isolated method)
- **Runtime Risk**: MEDIUM (order state mutations)
- **Testing Risk**: HIGH (complex branching logic)

## Call Hierarchy

### Callers (Inbound)
- Order management entry points
- Order modification handlers
- Order cancellation workflows

### Callees (Outbound)
- Order state validation methods
- Order collection updates
- Event notification system
- OCO coordination logic

## Refactoring Strategy

### Recommended Approach
1. **Extract Order Validation**: Separate validation logic (CYC -3)
2. **Extract OCO Handling**: Isolate OCO coordination (CYC -4)
3. **Extract State Transitions**: Separate FSM logic (CYC -5)
4. **Simplify Conditionals**: Use guard clauses (CYC -2)

### Target Complexity
- **Current**: 19
- **Target**: ≤15 (Jane Street threshold)
- **Reduction Needed**: -4 minimum

### Extraction Candidates
ManageCIT (CYC 19) splits into:
- ValidateCITOrder (CYC 3-4) [NEW]
- HandleOCOCoordination (CYC 4-5) [NEW]
- TransitionCITState (CYC 5-6) [NEW]
- ManageCIT (CYC 5-6) [REDUCED]

## Risk Assessment: MEDIUM-HIGH

### Risk Factors
- ✅ **Isolated Method**: Low compilation risk
- ⚠️ **Complex Logic**: 19 branches to test
- ⚠️ **State Mutations**: Order state changes
- ⚠️ **OCO Coordination**: Multi-order dependencies
- ✅ **No Lock Usage**: Lock-free compliant

### Mitigation Strategy
1. Add unit tests for all 19 branches before refactoring
2. Use TDD for extracted methods
3. Verify OCO coordination logic preservation
4. Test order state machine transitions
5. Run stress tests after extraction

## Phase 0 Completion Status
- ✅ Complexity metrics identified
- ✅ Blast radius assessed
- ✅ Call hierarchy analyzed
- ✅ Refactoring strategy defined
- ✅ Risk level determined: MEDIUM-HIGH

**Ready for Phase 1 (Architecture Planning)**
