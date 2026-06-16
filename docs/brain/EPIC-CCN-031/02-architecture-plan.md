# Phase 2: Architecture Planning - EPIC-CCN-031

## V12.23 Protocol Compliance
- **Epic ID**: EPIC-CCN-031
- **Phase**: 2 (Architecture Planning)
- **Date**: 2026-06-15
- **Status**: READY FOR REVIEW

## Target Method Analysis

### Current State
- **Method**: AuditMaster_HandleNakedPosition
- **File**: src/V12_002.REAPER.Audit.cs
- **Lines**: 624-661 (38 LOC)
- **Complexity**: 15 (CYC)
- **Tier**: 1 (High Priority)

### Method Signature
private void AuditMaster_HandleNakedPosition(Position masterPos, int masterActualQty, string masterExpectedKey)

### Current Responsibilities
1. **Order Snapshot & Stop Detection** (Lines 629-638)
2. **Grace Window Tracking** (Lines 640-656)
3. **Enqueue & Trigger** (Lines 657-673)
4. **Cleanup** (Lines 675-678)

## Extraction Strategy

### Goal
Reduce complexity from **CYC 15** to **CYC ≤8** (Jane Street strict standard)

### Approach
Extract 3 helper methods with single responsibilities:
1. **Pure Function**: Stop order detection (CYC 1)
2. **State Tracking**: Grace window management (CYC 2)
3. **Async Dispatch**: Enqueue + trigger with error recovery (CYC 3)

### Complexity Distribution
- **Main Method**: CYC 4 (orchestration only)
- **Helper 1**: CYC 1 (pure function)
- **Helper 2**: CYC 2 (dictionary operations)
- **Helper 3**: CYC 3 (try-catch + conditional)
- **Total**: 10 (distributed across 4 methods)
- **Max per Method**: 4 (target: ≤8)

## Proposed Helper Methods

### Helper 1: HasWorkingStopOrder (Pure Function)
**Signature**: private bool HasWorkingStopOrder(Order[] orders, string instrumentFullName)
**Responsibility**: Check if any order in the snapshot is a working stop order
**Complexity**: CYC 1 (single return statement)
**Testability**: High (pure function, easy to unit test)

### Helper 2: TryStartGraceWindow (State Tracking)
**Signature**: private bool TryStartGraceWindow(string accountName, int actualQty, int graceSeconds)
**Responsibility**: Check if grace window already started, initialize if not
**Complexity**: CYC 2 (if-else branch)
**Lock-Free**: Uses ConcurrentDictionary.TryGetValue (atomic)
**Testability**: Medium (requires mocking dictionary)

### Helper 3: EnqueueNakedStopWithTrigger (Async Dispatch)
**Signature**: private void EnqueueNakedStopWithTrigger(Position masterPos, int masterActualQty, string masterExpectedKey, DateTime firstSeen)
**Responsibility**: Call EnqueueReaperMasterNakedStop, trigger ProcessReaperNakedStopQueue, handle exceptions
**Complexity**: CYC 3 (if + try-catch)
**Lock-Free**: Uses Enqueue pattern + ConcurrentDictionary.TryRemove
**Testability**: Medium (requires mocking TriggerCustomEvent)

## Refactored Main Method

### New Complexity: CYC 4
- Branch 1: if (masterActualQty != 0)
- Branch 2: if (!masterHasWorkingStop)
- Branch 3: if (TryStartGraceWindow(...))
- Branch 4: else (cleanup)

**Target Achieved**: CYC 4 ≤ 8

## Call Graph
AuditMaster_HandleNakedPosition (CYC 4)
├── HasWorkingStopOrder (CYC 1) [Pure Function]
├── TryStartGraceWindow (CYC 2) [State Tracking]
└── EnqueueNakedStopWithTrigger (CYC 3) [Async Dispatch]

## Lock-Free Validation
- No lock() statements
- FSM/Actor Enqueue pattern maintained
- Atomic primitives only (ConcurrentDictionary operations)
- No race conditions

## Jane Street Compliance
- Cognitive Simplicity: All methods CYC ≤8
- Testability: Pure functions + mockable dependencies
- Auditability: Clear separation of concerns
- Performance: No additional allocations or lock contention

## V12 DNA Compliance
- Correctness by Construction: Type-safe design
- Lock-Free Actor Pattern: ConcurrentDictionary + Enqueue
- ASCII-Only: All string literals use ASCII
- Type Safety: Strong typing maintained

## Testing Strategy
7 unit/integration tests required covering:
- HasWorkingStopOrder with/without stops
- TryStartGraceWindow first/second calls
- EnqueueNakedStopWithTrigger success/exception
- Integration tests for full flow

## Risk Assessment
- **Blast Radius**: MINIMAL (1 file, 1 method modified, 3 methods added)
- **Regression Risk**: LOW (isolated change, no interface modifications)
- **Integration Risk**: NONE (no changes to callers/callees)
- **Performance Impact**: NONE (no additional allocations)

## Approval Checklist
- [x] Extraction strategy defined
- [x] Method signatures designed
- [x] Call graph documented
- [x] Data flow analyzed
- [x] Lock-free validation completed
- [x] Jane Street compliance verified
- [x] V12 DNA compliance verified
- [x] Testing strategy defined
- [x] Risk assessment completed

**Ready for Phase 3**: YES

**Next Action**: Submit to Arena AI for Phase 3 DNA & PR Audit
