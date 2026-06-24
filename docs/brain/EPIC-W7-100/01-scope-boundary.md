# Phase 1: Scope Boundary - EPIC-W7-100

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.00 (plan mode)
- **Execution Time**: 2026-06-24T01:34:42Z

## Epic Target
- **Method**: ClosePositionsOnlyApexAccounts
- **File**: src/V12_002.SIMA.Flatten.cs
- **Current CYC**: 11
- **Target CYC**: ≤8
- **Reduction Required**: 3+ decision points

## Scope Analysis

### Method Structure (74 lines)
- Early return check (EnableSIMA)
- Account enumeration loop with fleet account filter
- Master account fallback logic with coverage and position checks
- Pump trigger with error handling (TriggerCustomEvent, InvalidOperationException, general Exception, empty queue)

### Decision Point Breakdown (CYC=11)

| # | Decision Point | Type | Extraction Candidate |
|---|----------------|------|---------------------|
| 1 | if (!EnableSIMA) | Guard | Keep (early return) |
| 2 | foreach (Account acct in snapshot) | Loop | Extract |
| 3 | if (!IsFleetAccount(acct)) | Filter | Extract |
| 4 | if (!masterCovered && Account.Positions.Count > 0) | Compound | Extract |
| 5 | if (!_pendingFlattenOps.IsEmpty) | Guard | Extract |
| 6 | catch (InvalidOperationException ex) when (...) | Exception | Extract |
| 7 | catch (Exception ex) | Exception | Extract |
| 8 | else | Branch | Extract |

## IN SCOPE: Extraction Targets

### 1. Account Enumeration Logic (CYC -2)
**Extract to**: EnqueueFleetAccountsForPositionClose(Account[] snapshot)

**Rationale**:
- Encapsulates foreach loop + fleet filter (2 decision points)
- Single responsibility: enumerate and enqueue fleet accounts
- Returns enqueue count for logging

**Signature**: private int EnqueueFleetAccountsForPositionClose(Account[] snapshot)

**Extracted Lines**: ~15 lines (foreach block)

### 2. Master Account Fallback Logic (CYC -1)
**Extract to**: EnqueueMasterAccountIfNeeded(bool masterCovered)

**Rationale**:
- Encapsulates compound condition (1 decision point)
- Single responsibility: handle master account fallback
- Returns true if master was enqueued

**Signature**: private bool EnqueueMasterAccountIfNeeded(bool masterCovered)

**Extracted Lines**: ~12 lines (master fallback block)

### 3. Flatten Pump Trigger with Error Handling (CYC -4)
**Extract to**: TriggerFlattenPumpWithFallback(string source)

**Rationale**:
- Encapsulates pump trigger + 3 exception handlers (4 decision points)
- Single responsibility: safely trigger pump or fallback
- Handles all error scenarios in one place

**Signature**: private void TriggerFlattenPumpWithFallback(string source)

**Extracted Lines**: ~25 lines (try/catch/else block)

## OUT OF SCOPE: Preserved in Original Method

### 1. EnableSIMA Guard
**Rationale**: Early return pattern - keep at method entry for clarity

### 2. Snapshot Creation
**Rationale**: Single line, no complexity

### 3. Logging Statements
**Rationale**: Orchestration context - keep in main method

### 4. isFlattenRunning Flag Management
**Rationale**: State management - keep in orchestrator

## Extraction Strategy

### Phase 5 Ticket Breakdown

**Ticket 1**: Extract EnqueueFleetAccountsForPositionClose
- **CYC Reduction**: 11 → 9 (-2)
- **Risk**: LOW (pure enumeration logic)
- **Dependencies**: None

**Ticket 2**: Extract EnqueueMasterAccountIfNeeded
- **CYC Reduction**: 9 → 8 (-1)
- **Risk**: LOW (isolated fallback logic)
- **Dependencies**: Ticket 1 complete

**Ticket 3**: Extract TriggerFlattenPumpWithFallback
- **CYC Reduction**: 8 → 4 (-4)
- **Risk**: MEDIUM (error handling paths)
- **Dependencies**: Tickets 1-2 complete

### Final State
- **Original Method CYC**: 11 → 4 (Target achieved: ≤8)
- **New Helper Methods**: 3
- **Total CYC Budget**: 4 + 2 + 1 + 4 = 11 (preserved)
- **Max Helper CYC**: 4 (TriggerFlattenPumpWithFallback)

## Architectural Constraints

### V12 DNA Compliance
- Lock-Free: No locks in extracted methods
- ASCII-Only: All string literals are ASCII
- CYC ≤ 8: All methods meet threshold
- Correctness by Construction: Preserve orchestration flow

### Jane Street Patterns
- FSM/Actor Pattern: Preserve _pendingFlattenOps queue semantics
- Error Handling: Maintain fallback flatten on pump failure
- State Management: Keep isFlattenRunning flag in orchestrator

## Risk Mitigation

### Blast Radius: ZERO
- No external callers (private method)
- No cross-file dependencies
- Safe for aggressive refactoring

### Testing Strategy
1. Unit test each extracted helper (3 tests)
2. Integration test original method (preserve behavior)
3. Verify flatten pump triggers correctly
4. Verify fallback flatten on error

### Rollback Plan
- Git revert if CYC reduction fails
- Preserve original method in comment block
- Deploy-sync.ps1 after each ticket

## Success Criteria

### Phase 1 (This Document)
- Scope boundary defined (IN SCOPE vs OUT OF SCOPE)
- Extraction targets identified (3 helpers)
- CYC reduction path validated (11 → 4)
- Risk assessment complete (LOW-MEDIUM)

### Phase 2 (Architecture Planning)
- Detailed helper method signatures
- Call sequence diagrams
- Error handling flow

### Phase 5 (Ticket Execution)
- All 3 tickets completed
- CYC ≤ 8 achieved
- Build passes
- F5 in NinjaTrader successful

---

**Phase 1 Status**: COMPLETED
**Next Phase**: Phase 2 (Architecture Planning)
**Generated**: 2026-06-24T01:34:42Z
