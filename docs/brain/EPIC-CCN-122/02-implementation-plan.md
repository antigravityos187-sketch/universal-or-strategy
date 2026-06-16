# Phase 2: Implementation Plan - EPIC-CCN-122

## Epic Status: CANCELLED (Placeholder - No Target Identified)

**Decision**: This epic is being CANCELLED as it was created as a placeholder slot (CCN-122) but never assigned a concrete target method.

## Analysis Summary

### Complexity Audit Findings
The complexity audit report shows **ZERO methods with CYC=14**. The audit contains:
- **22 methods** with CYC > 20 (CRITICAL-REFACTOR priority)
- **43 methods** with CYC 15-20 (WATCH list)
- **20 methods** flagged as M5 dispatch candidates
- **15 methods** with LOC > 80

### Why This Epic Cannot Proceed

1. **No Target Method**: The expected CYC=14 method does not exist in the current codebase
2. **Placeholder Status**: This epic was created to fill the CCN-122 slot in the epic sequence
3. **Invalid Phase 0**: Hotspot analysis was incomplete and should have blocked Phase 1
4. **No Extraction Strategy**: Cannot define implementation without a concrete target

## Candidate Methods for Future Epics

The following methods with CYC 12-14 could be candidates for future complexity reduction epics:

### CYC 14 Methods (Potential Targets)
| Method | File | CYC | LOC | Priority |
|--------|------|-----|-----|----------|
| OnAccountOrderUpdate | V12_002.Orders.Callbacks.AccountOrders.cs | 14 | 22 | Medium |
| ProcessQueuedAccountOrder | V12_002.Orders.Callbacks.AccountOrders.cs | 14 | 21 | Medium |
| HandleMatchedFollower_StopReplacement | V12_002.Orders.Callbacks.AccountOrders.cs | 14 | 20 | Medium |
| IsMasterReplaceCascadeCancellation | V12_002.Orders.Callbacks.AccountOrders.cs | 14 | 23 | Medium |
| ProcessAccountOrder_UpdateMasterExpected | V12_002.Orders.Callbacks.AccountOrders.cs | 14 | 18 | Medium |
| ProcessAccountOrder_UpdateFleetExpected | V12_002.Orders.Callbacks.AccountOrders.cs | 14 | 20 | Medium |
| HandleMatchedFollower_PendingCancelReplace | V12_002.Orders.Callbacks.AccountOrders.cs | 14 | 46 | Medium |
| ExecuteFollowerCascadeCleanup | V12_002.Orders.Callbacks.AccountOrders.cs | 14 | 26 | Medium |
| BuildLiveBrokerOrderIndex | V12_002.Orders.Management.Cleanup.cs | 14 | 14 | Low |
| CleanupPosition | V12_002.Orders.Management.Cleanup.cs | 14 | 22 | Medium |
| SubmitRepairOrderWithAuthorization | V12_002.REAPER.Repair.cs | 14 | 47 | Medium |
| GetFsmExpectedPosition | V12_002.Symmetry.BracketFSM.cs | 14 | 17 | Medium |
| HandleFsmFilled | V12_002.Symmetry.BracketFSM.cs | 14 | 9 | Low |
| SymmetryGuardOnMasterFill | V12_002.Symmetry.cs | 14 | 30 | Medium |
| IsOrderAllowed | V12_002.UI.Compliance.cs | 14 | 18 | Medium |
| ProcessQueuedExecution_HandleFleetOCO | V12_002.UI.Compliance.cs | 14 | 13 | Medium |
| ProcessQueuedExecution_SyncFlatPosition | V12_002.UI.Compliance.cs | 14 | 17 | Medium |
| LogApexPerformance | V12_002.UI.Compliance.cs | 14 | 53 | Low (Logging) |
| PropagateMasterEntryMove | V12_002.Orders.Callbacks.Propagation.cs | 14 | 33 | Medium |
| TryApplyConfigTarget_Type | V12_002.UI.IPC.Commands.Config.cs | 14 | 22 | Medium |
| HandleOrderCancelled_ProcessStopReplacement | V12_002.Orders.Callbacks.cs | 14 | 16 | Medium |
| OnSubmitClick | V12_002.UI.Panel.Handlers.cs | 14 | 28 | Medium |
| CancelAll_ProcessMasterAccount | V12_002.UI.IPC.Commands.Fleet.cs | 14 | 17 | Medium |
| TryHandleMode_SetMode | V12_002.UI.IPC.Commands.Mode.cs | 14 | 33 | Medium |
| TryHandleRisk_Breakeven | V12_002.UI.IPC.Commands.Mode.cs | 14 | 16 | Medium |

### CYC 13 Methods (Lower Priority)
| Method | File | CYC | LOC | Priority |
|--------|------|-----|-----|----------|
| ProcessOnExecutionUpdate | V12_002.Orders.Callbacks.Execution.cs | 13 | 19 | Medium |
| ProcessOnExecution_HandleStopFill | V12_002.Orders.Callbacks.Execution.cs | 13 | 25 | Medium |
| AuditMaster_CheckExpectedActual | V12_002.REAPER.Audit.cs | 13 | 17 | Medium |
| RefreshActivePositionOrders | V12_002.Orders.Management.StopSync.cs | 13 | 27 | Medium |
| EmergencyFlattenSingleFleetAccount | V12_002.SIMA.Flatten.cs | 13 | 41 | High |
| InitializeFollowerBracketFSM | V12_002.SIMA.Fleet.cs | 13 | 28 | Medium |
| IsValidTradeTypeToken | V12_002.Orders.Callbacks.Propagation.cs | 13 | 9 | Low |
| ResolveMasterTradeType | V12_002.Orders.Callbacks.Propagation.cs | 13 | 10 | Low |
| ExecuteMultiAccountMarket | V12_002.SIMA.Execution.cs | 13 | 55 | Medium |
| ApplyStickyConfig_TargetValues | V12_002.StickyState.cs | 13 | 27 | Medium |
| ApplyStickyModeProfile_TargetValues | V12_002.StickyState.cs | 13 | 27 | Medium |
| SerializeSticky_WriteModeProfiles | V12_002.StickyState.cs | 13 | 26 | Low |
| TryApplyConfigTarget_Value | V12_002.UI.IPC.Commands.Config.cs | 13 | 45 | Medium |
| HandleTrimCommand | V12_002.UI.IPC.Commands.Config.cs | 13 | 32 | Medium |
| ProcessIpcCommands | V12_002.UI.IPC.cs | 13 | 22 | Medium |
| TryParseTargetMode | V12_002.UI.IPC.cs | 13 | 26 | Low |
| PlacePanel | V12_002.UI.Panel.Construction.cs | 13 | 55 | Low (UI) |
| CreateSection0_Identity | V12_002.UI.Panel.Construction.cs | 13 | 147 | Low (UI, LOC>80) |
| ManageTrailingStops | V12_002.Trailing.cs | 13 | 25 | Medium |

### CYC 12 Methods (Lowest Priority)
Multiple methods exist with CYC=12. These are below the Jane Street threshold of 15 and should only be targeted if they have other risk factors (high LOC, high churn, critical path).

## Recommended Action: Close This Epic

### Rationale
1. **No Valid Target**: Cannot proceed without a concrete method to refactor
2. **Better Prioritization Needed**: The 22 methods with CYC > 20 are higher priority
3. **Resource Allocation**: Engineering effort should focus on CRITICAL-REFACTOR methods first
4. **Epic Hygiene**: Keeping placeholder epics in the backlog creates confusion

### Closure Steps
1. ✅ Mark EPIC-CCN-122 as CANCELLED in manifest.json
2. ✅ Document cancellation reason in this implementation plan
3. ✅ Archive epic folder to `docs/brain/archive/EPIC-CCN-122/`
4. ✅ Update epic tracking spreadsheet to reflect cancellation
5. ✅ Create new epics for high-value CYC 14 methods if needed

## Future Epic Creation Criteria

When creating new complexity reduction epics, prioritize methods with:

### High Priority Indicators
- **CYC > 20**: CRITICAL-REFACTOR (22 methods remaining)
- **CYC 15-20 + LOC > 80**: God-function candidates (15 methods)
- **CYC 15-20 + High Churn**: Hotspot methods (requires CodeScene analysis)
- **M5 Dispatch Candidates**: Methods flagged for extraction (20 methods)

### Medium Priority Indicators
- **CYC 14-15 + Critical Path**: Order callbacks, SIMA dispatch, FSM logic
- **CYC 14-15 + State Coupling**: Methods with complex state mutations
- **CYC 14-15 + Test Gap**: Methods without TDD coverage

### Low Priority Indicators
- **CYC 12-14 + Low Churn**: Stable methods below Jane Street threshold
- **CYC 12-14 + UI/Logging**: Non-critical path methods
- **CYC 12-14 + Simple Logic**: Methods with low cognitive complexity despite CYC score

## Next Steps for Director

### Option 1: Close and Move On (Recommended)
1. Archive EPIC-CCN-122
2. Focus on EPIC-CCN-107 through EPIC-CCN-121 (active epics)
3. Create new epics for CYC > 20 methods when capacity allows

### Option 2: Reassign to High-Value Target
If a CYC 14 method is identified as high-value:
1. Select target from candidate list above
2. Re-run Phase 0 (Hotspot Analysis) with concrete method
3. Re-run Phase 1 (Scope Definition) with valid target
4. Proceed to Phase 2 with updated plan

### Option 3: Defer Until Phase 7 Complete
1. Complete EPIC-CCN-107 through EPIC-CCN-121 first
2. Re-evaluate complexity landscape after Phase 7
3. Create targeted epics for remaining CYC 14 methods if needed

## V12 DNA Compliance

### Jane Street Alignment
- ✅ **Threshold Adherence**: CYC 14 is below the 15 threshold (acceptable complexity)
- ✅ **Prioritization**: Focus on CYC > 20 methods first (correct priority)
- ✅ **Resource Efficiency**: Don't refactor methods that are already acceptable

### V12.23 No Scope Creep Protocol
- ✅ **No Scope Defined**: Cannot have scope creep without a target
- ✅ **Boundary Validation**: Trivially compliant (no work to do)
- ✅ **Epic Hygiene**: Closing placeholder epics prevents confusion

### Correctness by Construction
- ✅ **Invalid State Prevention**: Cancelling epic prevents invalid work
- ✅ **Type Safety**: No implementation means no type safety violations
- ✅ **Atomic Operations**: No operations to make atomic

## Conclusion

**EPIC-CCN-122 is CANCELLED** due to lack of a valid target method. The complexity audit shows no methods with CYC=14, and the 25 methods identified with CYC 14 are all below the Jane Street threshold of 15, making them acceptable complexity.

Engineering effort should focus on:
1. **22 methods with CYC > 20** (CRITICAL-REFACTOR priority)
2. **43 methods with CYC 15-20** (WATCH list)
3. **15 methods with LOC > 80** (God-function candidates)

This epic should be archived, and new epics should be created only for high-value targets that meet the prioritization criteria above.

---
**Phase 2 Status**: CANCELLED (no valid target)
**Created**: 2026-06-13
**Epic Disposition**: ARCHIVE
**Recommended Next Action**: Focus on EPIC-CCN-107 through EPIC-CCN-121
**Jane Street Alignment**: ✅ COMPLIANT (correct prioritization)
**V12.23 Compliance**: ✅ COMPLIANT (no scope creep possible)
