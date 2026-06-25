# Phase 1: Scope Definition - EPIC-W7-014

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18 (Sequential Thinking MCP)
- **API Key**: Sequential Thinking MCP
- **Execution Time**: 2026-06-24T19:25:09Z

## Epic Overview
- **Target Method**: TryHandleFleetCommand
- **File**: src/V12_002.UI.IPC.Commands.Fleet.cs
- **Current CYC**: 20 (2.5x over threshold of 8)
- **Blast Radius**: 0 (isolated, private method)
- **Risk Level**: LOW-MEDIUM

## Scope Boundary Validation

### IN SCOPE

#### Primary Target
**Method**: TryHandleFleetCommand (lines 37-81)
- **Refactoring Strategy**: Extract routing logic to command registry pattern
- **Expected CYC Reduction**: 20 to 3
- **Justification**: Dispatcher pattern with 18 if/else branches is the complexity source

#### Extraction Components
1. **Command Registry Dictionary**
   - Create Dictionary for command handlers
   - Register all 18 fleet command handlers
   - Replace if/else chain with dictionary lookup

2. **Registry Initialization**
   - Add InitializeFleetCommandRegistry method
   - Register handlers in constructor or OnStateChange
   - Maintain handler references

3. **Dispatcher Simplification**
   - Reduce to: validate input, lookup handler, invoke
   - Keep error handling and logging
   - Preserve backward compatibility

#### Files to Modify
- src/V12_002.UI.IPC.Commands.Fleet.cs (primary target)

#### Expected Outcomes
- CYC reduction: 20 to 3 (85% reduction)
- Improved maintainability (declarative registration)
- Easier to add new commands (no dispatcher modification)
- Better testability (isolated handler testing)

### OUT OF SCOPE

#### Sub-Handler Methods (18 methods)
These are already extracted and have acceptable complexity:
1. TryHandleFleet_Trim
2. TryHandleFleet_Lock50
3. TryHandleFleet_FlattenOnly
4. TryHandleFleet_Flatten
5. TryHandleFleet_CancelAll
6. TryHandleFleet_ResetMemory
7. TryHandleFleet_LongShort
8. TryHandleFleet_OrLong
9. TryHandleFleet_OrShort
10. TryHandleFleet_TrendManualLimit
11. TryHandleFleet_RetestManualLimit
12. TryHandleFleet_FfmaManualLimit
13. TryHandleFleet_FfmaManualMarket
14. TryHandleFleet_CloseTarget
15. TryHandleFleet_MoveTarget
16. TryHandleFleet_FleetState
17. TryHandleFleet_ToggleAccount
18. TryHandleFleet_SetShadow

**Rationale**: These handlers are already single-purpose methods. Refactoring them is separate work.

#### Level 2 Callees (74 methods)
- Configuration handlers
- FSM operations
- Position management
- Order operations
- Calculation utilities
- Target management
- Metadata operations
- Logging utilities

**Rationale**: These are utility methods called by sub-handlers. Not part of dispatcher complexity.

#### Other IPC Command Handlers
- TryHandlePositionCommand
- TryHandleOrderCommand
- TryHandleConfigCommand

**Rationale**: Each command category has its own dispatcher. This epic focuses only on Fleet commands.

### Scope Justification

#### Why Registry Pattern?
1. **Complexity Reduction**: Eliminates 18-branch if/else chain
2. **Jane Street Alignment**: Make illegal states unrepresentable
3. **Maintainability**: New commands added via registration
4. **Testability**: Each handler testable in isolation
5. **HFT Compatibility**: Dictionary lookup is O(1)

#### Why Not Refactor Sub-Handlers?
1. **Already Extracted**: Sub-handlers are single-purpose methods
2. **Acceptable Complexity**: Individual handler complexity likely under 8
3. **Scope Creep Risk**: Refactoring 18 methods is separate epic
4. **Blast Radius**: Each handler has its own dependencies

#### Risk Mitigation
- **Backward Compatibility**: Registry pattern preserves exact same behavior
- **Testing**: Existing tests should pass without modification
- **Rollback**: Simple to revert if issues arise
- **Isolation**: Blast radius = 0, no downstream breakage

## Success Criteria

### Phase 2 (Architecture Planning)
- Design command registry data structure
- Define handler signature interface
- Plan initialization sequence
- Document error handling strategy

### Phase 5 (Implementation)
- CYC reduced from 20 to 8 or less
- All 18 commands still functional
- No new compilation errors
- Existing tests pass
- Build succeeds with deploy-sync.ps1

### Phase 6 (Verification)
- Complexity audit confirms CYC under 8
- F5 in NinjaTrader successful
- No runtime errors in fleet commands
- Code review approval

## Jane Street Alignment

### Principles Applied
1. **Cognitive Simplicity**: Registry pattern easier to reason about
2. **Make Illegal States Unrepresentable**: Invalid commands fail at dictionary lookup
3. **Declarative Over Imperative**: Command registration is declarative
4. **Single Responsibility**: Dispatcher only routes, handlers only handle

### HFT Considerations
- **Performance**: Dictionary lookup is O(1), no latency impact
- **Memory**: Minimal overhead (18 delegate references)
- **Predictability**: Deterministic routing behavior
- **Testability**: Each handler independently verifiable

## Conclusion

**EPIC-W7-014 scope is APPROVED** with clear boundaries:
- **IN SCOPE**: Dispatcher refactoring to registry pattern
- **OUT OF SCOPE**: Sub-handler internals, utility methods, other command categories
- **Expected Impact**: CYC 20 to 3, improved maintainability, zero blast radius
- **Risk Level**: LOW (isolated method, backward compatible)

**Next Phase**: Proceed to Phase 2 (Architecture Planning) to design registry implementation.
