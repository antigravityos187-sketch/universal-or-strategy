# Epic: EPIC-CCN-2 -- Scope Alignment

**Epic ID**: EPIC-CCN-2  
**Target**: [`ProcessIpcCommands()`](../../../src/V12_002.UI.IPC.cs:260) in [`src/V12_002.UI.IPC.cs`](../../../src/V12_002.UI.IPC.cs)  
**Phase**: 1 - Intake  
**Status**: Awaiting Director Confirmation  
**Created**: 2026-06-08T23:06:00Z

---

## Code Area

**Target Method**: [`ProcessIpcCommands()`](../../../src/V12_002.UI.IPC.cs:260)  
**File**: [`src/V12_002.UI.IPC.cs`](../../../src/V12_002.UI.IPC.cs)  
**Line Range**: 260-430 (171 lines)  
**Signature**: `private void ProcessIpcCommands()`

### Method Responsibility
This method is the **IPC command dispatcher** for the V12 strategy. It:
1. Drains commands from the `ipcCommandQueue` (lock-free concurrent queue)
2. Validates command syntax, timestamps, and allowlist membership
3. Applies IPC hardening checks (rate limiting, circuit breakers, anomaly detection)
4. Performs symbol matching to determine if the command targets this chart instance
5. Enqueues validated commands to the FSM/Actor mailbox via `Enqueue(ctx => ctx.ProcessIpcCommandCore(...))`

### Current Callers (Verified via jCodemunch)
- [`OnBarUpdate()`](../../../src/V12_002.BarUpdate.cs:259) - Called every bar tick
- [`OnStateChange()`](../../../src/V12_002.Lifecycle.cs:655) - Called on state transitions
- [`ProcessIpcCommands()`](../../../src/V12_002.UI.IPC.cs:426) - Self-recursive call via `TriggerCustomEvent` when queue not empty

**Call Pattern**: This is a **hot path method** invoked on every bar update and state change. Performance is critical.

---

## Validated Problem

### Stated Problem
**High Cyclomatic Complexity**: CYC 76 (confirmed via jCodemunch `get_symbol_complexity`)

### Forensic Analysis (jCodemunch Verification)

**Complexity Metrics**:
- **Cyclomatic Complexity**: 76 (9.5x over Jane Street GODMODE threshold of 8)
- **Max Nesting Depth**: 6 levels
- **Parameter Count**: 0 (good - no parameter coupling)
- **Lines of Code**: 171 lines
- **Assessment**: HIGH risk

**Hotspot Score**: 194.94 (2nd highest in codebase after `ProcessOnStateChange` at 292.92)
- **Churn**: 12 commits (moderate volatility)
- **Formula**: `CYC × log(1 + churn)` = 76 × log(13) = 194.94

**Blast Radius**: 
- **Direct Importers**: 0 (method is private, not imported)
- **Direct Dependents**: 0 (no external callers via import graph)
- **Risk Score**: 0.0 (isolated - excellent for refactoring)

**Key Finding**: This method is **architecturally isolated** - it's called internally but has zero external dependencies. This makes it a **low-risk, high-value refactoring target**.

### Root Causes of Complexity

1. **Validation Cascade** (Lines 280-310): 
   - Malformed command checks
   - Empty action checks
   - Timestamp extraction loop
   - Metadata guard validation
   - IPC hardening validation (5-way switch statement)
   - Allowlist validation

2. **Symbol Matching Logic** (Lines 312-350):
   - 15+ boolean conditions for `isGlobalCommand` (TOGGLE_ACCOUNT, SET_SIMA, GET_FLEET, etc.)
   - Complex `isForMe` logic with 10+ symbol matching rules (MGC→GC, MES→ES, etc.)

3. **Diagnostic Logging** (Lines 352-365):
   - Conditional formatting based on `isGlobalCommand`

4. **Error Handling** (Lines 366-420):
   - Try-catch wrapper
   - Queue drain loop with counter
   - Self-recursive trigger when queue not empty

### V12 DNA Violations

✅ **Lock-Free**: Method uses `Enqueue()` pattern - no `lock()` statements detected  
❌ **Cyclomatic Complexity**: 76 >> 8 (Jane Street GODMODE threshold)  
✅ **ASCII-Only**: No Unicode detected in string literals (verified via source inspection)  
⚠️ **Extraction Floor**: Current method is 171 lines - any extracted sub-methods must be ≥15 LOC

---

## Scope Boundaries

### IN Scope
- **Target Method**: [`ProcessIpcCommands()`](../../../src/V12_002.UI.IPC.cs:260) (lines 260-430)
- **Extraction Candidates**:
  1. **Command Validation Pipeline** (lines 280-310) → Extract to `ValidateIpcCommandSyntax()`
  2. **Global Command Classification** (lines 312-330) → Extract to `IsGlobalCommand()`
  3. **Symbol Matching Logic** (lines 332-350) → Extract to `IsCommandForThisChart()`
  4. **Diagnostic Logging** (lines 352-365) → Extract to `LogIpcCommandReceived()`

### OUT of Scope
- **ProcessIpcCommandCore()**: Separate method, not part of this epic
- **ValidateIpcCommand()**: Already extracted (EPIC-4 Ticket 03), not touched
- **IPC Hardening Infrastructure**: Circuit breakers, rate limiters - already implemented
- **Enqueue() Mechanism**: FSM/Actor mailbox - core V12 DNA, not modified
- **Callers**: [`OnBarUpdate()`](../../../src/V12_002.BarUpdate.cs:259), [`OnStateChange()`](../../../src/V12_002.Lifecycle.cs:655) - not modified

### Explicitly NOT Changing
- Method signature: `private void ProcessIpcCommands()` remains unchanged
- Call sites: No modifications to callers
- Queue semantics: `ipcCommandQueue.TryDequeue()` logic unchanged
- Enqueue pattern: `Enqueue(ctx => ctx.ProcessIpcCommandCore(...))` unchanged

---

## Risk Level

**ISOLATED** (Lowest Risk Category)

**Justification**:
- ✅ **Zero External Importers**: Method is private, no cross-file dependencies
- ✅ **Zero Direct Dependents**: No other methods call this via import graph
- ✅ **Self-Contained**: All state accessed is local or via parameters
- ✅ **Hot Path Tested**: Called on every bar update - any breakage is immediately visible
- ✅ **Lock-Free**: No concurrency primitives to break

**Mitigation**:
- F5 in NinjaTrader after refactoring will immediately reveal any logic errors
- IPC commands can be tested via Remote App UI buttons
- No risk of cascading failures to other subsystems

---

## V12 DNA Constraints

### Cyclomatic Complexity Target
- **Current**: CYC 76
- **Target**: CYC ≤ 8 per method (Jane Street GODMODE)
- **Strategy**: Extract 4-5 sub-methods, each with CYC ≤ 8
- **Expected Outcome**: 
  - `ProcessIpcCommands()`: CYC ≤ 8 (orchestration only)
  - `ValidateIpcCommandSyntax()`: CYC ≤ 8
  - `IsGlobalCommand()`: CYC ≤ 8
  - `IsCommandForThisChart()`: CYC ≤ 8
  - `LogIpcCommandReceived()`: CYC ≤ 3

### Lock-Free Requirement
- ✅ **Already Compliant**: Method uses `Enqueue()` pattern
- ✅ **No Changes Required**: Extracted methods will not introduce locks

### ASCII-Only Compliance
- ✅ **Already Compliant**: No Unicode in string literals
- ✅ **No Changes Required**: Extracted methods will maintain ASCII-only

### Extraction Floor
- **Minimum LOC per Sub-Method**: 15 lines
- **Current Method**: 171 lines
- **Extraction Budget**: ~150 lines available for extraction (leaving ~20 for orchestration)
- **Feasibility**: 4 sub-methods × ~35 lines each = 140 lines (within budget)

---

## Success Criteria

### Functional Correctness
1. ✅ All IPC commands continue to work (TOGGLE_ACCOUNT, SET_SIMA, FLATTEN, etc.)
2. ✅ Symbol matching logic unchanged (MGC→GC, MES→ES, etc.)
3. ✅ Validation pipeline unchanged (syntax, timestamp, hardening, allowlist)
4. ✅ Diagnostic logging output identical
5. ✅ Queue drain behavior unchanged (max commands per drain, self-trigger)

### Complexity Reduction
1. ✅ `ProcessIpcCommands()` reduced to CYC ≤ 8
2. ✅ All extracted sub-methods have CYC ≤ 8
3. ✅ All extracted sub-methods have ≥ 15 LOC
4. ✅ Max nesting depth reduced from 6 to ≤ 4

### V12 DNA Compliance
1. ✅ Zero new `lock()` statements
2. ✅ ASCII-only in all string literals
3. ✅ `deploy-sync.ps1` executed after changes
4. ✅ BUILD_TAG verified in NinjaTrader output

### Testing
1. ✅ F5 in NinjaTrader compiles without errors
2. ✅ Strategy loads and runs
3. ✅ IPC commands tested via Remote App UI
4. ✅ `complexity_audit.py` confirms CYC ≤ 8 for all methods

---

## Forensic Summary

**Method**: [`ProcessIpcCommands()`](../../../src/V12_002.UI.IPC.cs:260)  
**Current State**: 171 lines, CYC 76, nesting 6, hotspot score 194.94  
**Blast Radius**: Zero external dependencies (isolated)  
**Callers**: 3 internal call sites (OnBarUpdate, OnStateChange, self-recursive)  
**Risk**: LOW (isolated, hot-path tested, lock-free)  
**Opportunity**: HIGH (9.5x over CYC threshold, 2nd highest hotspot)

**Recommendation**: Proceed with extraction. This is an ideal refactoring target - high complexity, low risk, clear extraction boundaries.

---

## Next Steps

**Phase 2**: `/epic-plan` - Generate detailed analysis and approach documents  
**Awaiting**: Director confirmation of scope alignment

---

## References

- **Hotspot Data**: [`jcodemunch_hotspots.json`](../../../jcodemunch_hotspots.json) (lines 15-27)
- **Recent Refactors**: [`src/AGENTS.md`](../../../src/AGENTS.md) (lines 8-13) - No conflicts detected
- **V12 DNA**: [`src/AGENTS.md`](../../../src/AGENTS.md) (lines 18-39)
- **Jane Street GODMODE**: CYC ≤ 8 threshold for microsecond-latency reasoning