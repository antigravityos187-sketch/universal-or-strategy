# Phase 1: Scope Definition - EPIC-W7-078

## Agent Tracking
- Agent Name: v12-phase1-scope
- Bobcoins Used: 0.18
- API Key: jCodemunch MCP
- Execution Time: 2026-06-24T19:34:23Z

## Epic Objective
Reduce cyclomatic complexity of StopIpcServer method from CYC=11 to 8 or less by extracting nested cleanup logic into focused helper methods.

## Target Method
- Method: StopIpcServer
- File: src/V12_002.UI.IPC.Server.cs
- Line: 451
- Current CYC: 11
- Target CYC: 8 or less
- Lines of Code: 60
- Max Nesting Depth: 10

## IN SCOPE

### Primary Extraction Target
1. StopIpcServer method body (lines 451-511)
   - IPC listener cleanup logic
   - IPC thread cleanup logic
   - Connected clients cleanup logic
   - Error handling for each cleanup phase

### Extraction Candidates
Based on the call hierarchy analysis showing interactions with 3 class-level constants:

1. IPC Listener Cleanup (extract to helper method)
   - Stop and dispose ipcListener
   - Null reference cleanup
   - Error handling for listener shutdown
   - Target CYC: 3 or less

2. IPC Thread Cleanup (extract to helper method)
   - Abort and join ipcThread
   - Thread state validation
   - Error handling for thread termination
   - Target CYC: 3 or less

3. Connected Clients Cleanup (extract to helper method)
   - Iterate and close connected clients
   - Client disposal logic
   - Error handling for client disconnection
   - Target CYC: 3 or less

### Scope Boundaries
- File: src/V12_002.UI.IPC.Server.cs only
- Method: StopIpcServer only
- Caller: StartIpcServer (single caller - must verify after refactoring)
- Class-level state: ipcListener, ipcThread, connectedClients (read/write)

## OUT OF SCOPE

### Explicitly Excluded
1. StartIpcServer method (line 52)
   - Caller of StopIpcServer
   - Not modifying caller logic
   - Will verify caller still works post-refactoring

2. Class-level field definitions
   - ipcListener (line 337)
   - ipcThread (line 338)
   - connectedClients (line 650)
   - Not changing field declarations or types

3. Other IPC methods
   - IPC message handling logic
   - IPC command processing
   - IPC client connection logic

4. Unrelated V12_002 methods
   - No changes to other partial class files
   - No changes to non-IPC logic

### Architectural Constraints
- No signature changes: StopIpcServer remains parameterless void method
- No behavioral changes: Cleanup sequence must remain identical
- No state changes: Class-level fields remain unchanged
- No caller changes: StartIpcServer invocation unchanged

## Extraction Strategy

### Approach: Vertical Slice Extraction
Extract each cleanup phase into a dedicated helper method:

StopIpcServer() [CYC 8 or less]
  - StopIpcListener() [CYC 3 or less]
  - StopIpcThread() [CYC 3 or less]
  - DisconnectAllClients() [CYC 3 or less]

### Complexity Reduction Plan
- Current: 1 method with CYC=11, nesting depth=10
- Target: 4 methods with CYC 8 or less each (main + 3 helpers)
- Expected CYC distribution:
  - StopIpcServer: CYC 5 or less (orchestration only)
  - StopIpcListener: CYC 3 or less
  - StopIpcThread: CYC 3 or less
  - DisconnectAllClients: CYC 3 or less

### Nesting Depth Reduction
- Current: Max nesting depth = 10
- Target: Max nesting depth 4 or less per method
- Technique: Early returns, guard clauses, helper method extraction

## Risk Mitigation

### Low Blast Radius Confirmed
- Direct Dependents: 0
- Importer Count: 0
- Overall Risk Score: 0.0
- Single Caller: StartIpcServer only

### Testing Requirements
1. Unit tests for each extracted helper method
2. Integration test for StopIpcServer orchestration
3. Verify StartIpcServer still functions correctly
4. Manual F5 test in NinjaTrader IDE

### Rollback Plan
- Git branch isolation (GitButler virtual branch)
- Atomic commits per extraction
- Pre-refactor snapshot of method body

## Success Criteria

### Quantitative
- StopIpcServer CYC reduced from 11 to 8 or less
- All helper methods CYC 3 or less
- Max nesting depth 4 or less per method
- Zero compilation errors
- Zero behavioral changes

### Qualitative
- Code more readable (single responsibility per method)
- Easier to test (isolated cleanup phases)
- Easier to reason about (reduced cognitive load)
- Jane Street alignment (CYC 8 or less strict standard)

## Dependencies
- Prerequisite: Phase 0 hotspot analysis (completed)
- Blocker: None (zero external dependencies)
- Follow-up: Phase 2 architecture planning

## Scope Validation
- Scope Creep Risk: LOW (isolated method, single caller)
- Boundary Clarity: HIGH (clear file/method boundaries)
- Extraction Feasibility: HIGH (3 distinct cleanup phases identified)

---

Scope Status: APPROVED
Ready for Phase 2: YES
