# Wave 3 Epic Roadmap - Next 10 Epics

**Date**: 2026-06-13T22:19:00Z
**Source**: Fresh complexity audit (Jane Street threshold CYC ≤8)
**Status**: READY FOR EXECUTION

---

## Epic Selection Criteria

**From Complexity Audit**:
- Total methods with CYC >8: **180 methods**
- Wave 2 completed: 7 epics (CCN-107 through CCN-114)
- Wave 3 target: Next 10 highest complexity methods

**Priority Factors**:
1. Cyclomatic complexity (CYC >8)
2. Lines of code (LOC)
3. File criticality (Orders, SIMA, REAPER subsystems)

---

## Wave 3 Target Epics (10 total)

### EPIC-CCN-115: PropagateMaster_IdentifyMove
- **File**: `V12_002.Orders.Callbacks.Propagation.cs`
- **Method**: `PropagateMaster_IdentifyMove`
- **CYC**: 18 → ≤8 (reduction: 10 points)
- **LOC**: 40
- **Priority**: P1 (HIGH - critical order propagation logic)
- **Subsystem**: Orders.Callbacks.Propagation
- **Description**: Identifies master order moves and propagates to followers

### EPIC-CCN-116: HandleFlatPosition_CleanupActivePositions
- **File**: `V12_002.Orders.Callbacks.Execution.cs`
- **Method**: `HandleFlatPosition_CleanupActivePositions`
- **CYC**: 17 → ≤8 (reduction: 9 points)
- **LOC**: 30
- **Priority**: P1 (HIGH - position cleanup critical path)
- **Subsystem**: Orders.Callbacks.Execution
- **Description**: Cleans up active positions when flattening

### EPIC-CCN-117: SyncLimitTarget
- **File**: `V12_002.Orders.Management.StopSync.cs`
- **Method**: `SyncLimitTarget`
- **CYC**: 17 → ≤8 (reduction: 9 points)
- **LOC**: 128 (⚠️ HIGH LOC - complex extraction)
- **Priority**: P1 (HIGH - stop/target synchronization)
- **Subsystem**: Orders.Management.StopSync
- **Description**: Synchronizes limit target orders with stop orders

### EPIC-CCN-118: MonitorRmaProximity
- **File**: `V12_002.Entries.RMA.cs`
- **Method**: `MonitorRmaProximity`
- **CYC**: 17 → ≤8 (reduction: 9 points)
- **LOC**: 67
- **Priority**: P1 (HIGH - RMA entry monitoring)
- **Subsystem**: Entries.RMA
- **Description**: Monitors proximity to RMA anchor for entry triggers

### EPIC-CCN-119: CheckFFMAConditions
- **File**: `V12_002.Entries.FFMA.cs`
- **Method**: `CheckFFMAConditions`
- **CYC**: 16 → ≤8 (reduction: 8 points)
- **LOC**: 50
- **Priority**: P2 (MEDIUM - FFMA entry validation)
- **Subsystem**: Entries.FFMA
- **Description**: Validates conditions for FFMA (First Five Minutes After) entry

### EPIC-CCN-120: AuditMaster_HandleNakedPosition
- **File**: `V12_002.REAPER.Audit.cs`
- **Method**: `AuditMaster_HandleNakedPosition`
- **CYC**: 15 → ≤8 (reduction: 7 points)
- **Priority**: P2 (MEDIUM - REAPER safety audit)
- **Subsystem**: REAPER.Audit
- **Description**: Handles naked position detection and repair

### EPIC-CCN-121: ProcessQueuedAccountOrder
- **File**: `V12_002.Orders.Callbacks.AccountOrders.cs`
- **Method**: `ProcessQueuedAccountOrder`
- **CYC**: 15 → ≤8 (reduction: 7 points)
- **LOC**: 34
- **Priority**: P2 (MEDIUM - account order processing)
- **Subsystem**: Orders.Callbacks.AccountOrders
- **Description**: Processes queued account orders from buffer

### EPIC-CCN-122: RestoreCascadedTargets
- **File**: `V12_002.Orders.Management.StopSync.cs`
- **Method**: `RestoreCascadedTargets`
- **CYC**: 16 → ≤8 (reduction: 8 points)
- **LOC**: 90 (⚠️ HIGH LOC)
- **Priority**: P2 (MEDIUM - target restoration)
- **Subsystem**: Orders.Management.StopSync
- **Description**: Restores cascaded target orders after stop replacement

### EPIC-CCN-123: PropagateMasterEntryMove
- **File**: `V12_002.Orders.Callbacks.Propagation.cs`
- **Method**: `PropagateMasterEntryMove`
- **CYC**: 14 → ≤8 (reduction: 6 points)
- **LOC**: 60
- **Priority**: P2 (MEDIUM - entry order propagation)
- **Subsystem**: Orders.Callbacks.Propagation
- **Description**: Propagates master entry order moves to followers

### EPIC-CCN-124: OnAccountOrderUpdate
- **File**: `V12_002.Orders.Callbacks.AccountOrders.cs`
- **Method**: `OnAccountOrderUpdate`
- **CYC**: 14 → ≤8 (reduction: 6 points)
- **LOC**: 26
- **Priority**: P2 (MEDIUM - account order callback)
- **Subsystem**: Orders.Callbacks.AccountOrders
- **Description**: Handles account order update callbacks from broker

---

## Epic Statistics

### Complexity Distribution

| Epic | CYC Before | CYC Target | Reduction | LOC | Priority |
|------|------------|------------|-----------|-----|----------|
| CCN-115 | 18 | ≤8 | 10 | 40 | P1 |
| CCN-116 | 17 | ≤8 | 9 | 30 | P1 |
| CCN-117 | 17 | ≤8 | 9 | 128 | P1 |
| CCN-118 | 17 | ≤8 | 9 | 67 | P1 |
| CCN-119 | 16 | ≤8 | 8 | 50 | P2 |
| CCN-120 | 15 | ≤8 | 7 | 38 | P2 |
| CCN-121 | 15 | ≤8 | 7 | 34 | P2 |
| CCN-122 | 16 | ≤8 | 8 | 90 | P2 |
| CCN-123 | 14 | ≤8 | 6 | 60 | P2 |
| CCN-124 | 14 | ≤8 | 6 | 26 | P2 |
| **Total** | **159** | **≤80** | **79** | **563** | - |

**Average CYC Before**: 15.9
**Average CYC Target**: ≤8
**Average Reduction**: 7.9 points per epic
**Total LOC**: 563 lines

### Subsystem Distribution

| Subsystem | Epics | Methods |
|-----------|-------|---------|
| Orders.Callbacks | 5 | CCN-115, 116, 121, 123, 124 |
| Orders.Management | 2 | CCN-117, 122 |
| Entries | 2 | CCN-118, 119 |
| REAPER | 1 | CCN-120 |

**Observation**: Heavy focus on Orders subsystem (70% of epics)

### High-Risk Epics

**LOC >80** (complex extractions):
1. **CCN-117**: SyncLimitTarget (128 LOC) - Will require multiple tickets
2. **CCN-122**: RestoreCascadedTargets (90 LOC) - Will require multiple tickets

**Recommendation**: Allocate extra time for CCN-117 and CCN-122

---

## Estimated Effort

### Per Epic (9-phase workflow, no Phase 4.5)

**Planning Phases** (90 minutes):
- Phase -1: Pre-flight (5 min)
- Phase 0: Hotspot (10 min)
- Phase 1: Scope + Boundary (20 min)
- Phase 2: Architecture (25 min)
- Phase 3: Audit (10 min)
- Phase 4: Tickets (10 min)
- Phase 6: Final Review (10 min)

**Execution Phases** (variable):
- Phase 5: Execution (10 min/ticket × ~6 tickets = 60 min)
- Phase 5.V: Verification (5 min/ticket × ~6 tickets = 30 min)

**Total per Epic**: ~180 minutes (3 hours)

### Wave 3 Total

**Sequential**: 10 epics × 3 hours = 30 hours
**Parallel** (8 agents): ~9.3 hours (2 sequential + 8 parallel)

---

## Execution Strategy

### Phase 1: Sequential Validation (2 epics)

**Epics**: CCN-115, CCN-116
**Purpose**: Validate 10-phase workflow (without Phase 4.5)
**Time**: ~6 hours

**Success Criteria**:
- Phase 1 consolidation works smoothly
- Manifest-based handoffs function correctly
- Build passes after each epic
- No P0 blockers introduced

### Phase 2: Parallel Execution (8 epics)

**Epics**: CCN-117 through CCN-124
**Agents**: 8 parallel (safe limit on n2-standard-4)
**Time**: ~3.2 hours

**Orchestration**: Bob CLI orchestrator or manual screen sessions

---

## Risk Assessment

### High-Risk Epics

1. **CCN-117** (SyncLimitTarget):
   - **Risk**: 128 LOC, complex stop/target synchronization
   - **Mitigation**: Allocate 2x time, break into smaller tickets
   - **Fallback**: Skip if blocked, continue with remaining epics

2. **CCN-122** (RestoreCascadedTargets):
   - **Risk**: 90 LOC, cascaded target restoration logic
   - **Mitigation**: Careful scope boundary validation
   - **Fallback**: Skip if blocked, continue with remaining epics

### Medium-Risk Epics

3. **CCN-115** (PropagateMaster_IdentifyMove):
   - **Risk**: Critical order propagation logic
   - **Mitigation**: Extensive testing, verification

4. **CCN-118** (MonitorRmaProximity):
   - **Risk**: RMA entry monitoring (67 LOC)
   - **Mitigation**: Clear extraction boundaries

### Low-Risk Epics

5-10. **CCN-116, 119, 120, 121, 123, 124**:
   - **Risk**: Standard complexity reduction
   - **Mitigation**: Follow standard workflow

---

## Success Criteria

### Per Epic

- ✅ All 9 phases complete (Phase 4.5 skipped)
- ✅ Complexity target met (CYC ≤8)
- ✅ Build passes
- ✅ Tests pass (where applicable)
- ✅ Manifest status = "completed"
- ✅ No P0 blockers introduced

### Wave 3 Completion

- ✅ 10 epics complete
- ✅ Total complexity reduction: 79 points
- ✅ No P0 blockers
- ✅ Roadmap updated
- ✅ Lessons learned documented
- ✅ Phase 1 consolidation validated
- ✅ Manifest-based architecture validated

---

## Post-Wave 3 Actions

### Immediate

1. **Sync to Local**: Pull `src/` and `docs/brain/` from VM
2. **Run Validation**: `powershell -File .\scripts\pre_push_validation.ps1 -Fast`
3. **Update Roadmap**: Mark completed epics in `epic_roadmap.json`
4. **Document Lessons**: Create `WAVE3_LESSONS_LEARNED.md`

### Deferred (Before Wave 4)

1. **Implement Phase 4.5**: Create `scripts/phase_4_5_ticket_review_mcp.py`
2. **Add Firebase Hooks**: Update Phase 1, 3, 5.V scripts
3. **Test 10-Phase Workflow**: Validate with 2 epics before Wave 4

---

## Remaining Backlog

**After Wave 3**: 170 methods remaining (180 - 10)
**Estimated Waves**: 17 waves (170 / 10 per wave)
**Estimated Time**: ~157 hours sequential, ~89 hours parallel (2 VMs)

---

## Command Reference

### Launch Sequential Validation

```bash
# Epic CCN-115
epic-intake EPIC-CCN-115 "Reduce PropagateMaster_IdentifyMove complexity"
epic-scope-boundary EPIC-CCN-115
epic-plan EPIC-CCN-115
epic-scan EPIC-CCN-115
epic-tickets EPIC-CCN-115
epic-validate EPIC-CCN-115 --ticket 1
epic-verify-ticket EPIC-CCN-115 --ticket 1
epic-review-final EPIC-CCN-115

# Epic CCN-116
epic-intake EPIC-CCN-116 "Reduce HandleFlatPosition_CleanupActivePositions complexity"
epic-scope-boundary EPIC-CCN-116
epic-plan EPIC-CCN-116
epic-scan EPIC-CCN-116
epic-tickets EPIC-CCN-116
epic-validate EPIC-CCN-116 --ticket 1
epic-verify-ticket EPIC-CCN-116 --ticket 1
epic-review-final EPIC-CCN-116
```

### Launch Parallel Execution

```bash
# Option A: Bob CLI Orchestrator
bob orchestrate wave3 --epics CCN-117,CCN-118,CCN-119,CCN-120,CCN-121,CCN-122,CCN-123,CCN-124

# Option B: Manual Screen Sessions
for epic in CCN-117 CCN-118 CCN-119 CCN-120 CCN-121 CCN-122 CCN-123 CCN-124; do
    screen -dmS $epic bash -c "epic-intake EPIC-$epic && epic-scope-boundary EPIC-$epic && epic-plan EPIC-$epic && epic-scan EPIC-$epic && epic-tickets EPIC-$epic && epic-validate EPIC-$epic --ticket 1 && epic-verify-ticket EPIC-$epic --ticket 1 && epic-review-final EPIC-$epic"
done
```

---

**Document Version**: 1.0
**Last Updated**: 2026-06-13T22:19:00Z
**Next Review**: After Wave 3 completion
**Maintainer**: V12 Orchestration Team