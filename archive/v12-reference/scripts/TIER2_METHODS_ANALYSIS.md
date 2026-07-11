# Tier 2 Methods Analysis (CYC 9-14)

**Date**: 2026-06-14
**Purpose**: Code-driven clustering for medium-complexity methods
**Threshold**: Jane Street strict (CYC ≤ 8)
**Target**: Reduce CYC 9-14 methods to ≤8

---

## Summary Statistics

From complexity audit:
- **Total methods audited**: 901
- **CYC > 8 (Tier 1 - blocking)**: 180 methods
- **CYC 6-8 (watch list)**: 183 methods
- **LOC > 80**: 29 methods

**Tier 2 Breakdown** (CYC 9-14):
- Extracted from "CYC > 8" list
- Excludes CYC ≥ 15 (those are Tier 1)
- Focus: Methods with CYC 9-14

---

## Tier 2 Methods by File (CYC 9-14)

### V12_002.Entries.FFMA.cs (2 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ExecuteFFMALimitEntry | 9 | 146 | LOC>80 |
| ExecuteFFMAManualMarketEntry | 12 | 162 | LOC>80 |

**Cluster Candidate**: FFMA Entry Execution
- Both handle FFMA entry logic
- Share entry validation and bracket submission
- LOC>80 indicates God-method territory

---

### V12_002.Entries.MOMO.cs (1 method)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ExecuteMOMOEntry | 10 | 166 | LOC>80 |

**Cluster Candidate**: MOMO Entry (standalone)
- Single complex method
- LOC>80 indicates God-method
- May need full epic alone

---

### V12_002.Entries.OR.cs (1 method)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| EnterORPosition | 11 | 166 | LOC>80 |

**Cluster Candidate**: OR Entry (standalone)
- Single complex method
- LOC>80 indicates God-method
- May need full epic alone

---

### V12_002.Entries.Retest.cs (1 method)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ExecuteRetestEntry | 12 | 199 | LOC>80 |

**Cluster Candidate**: Retest Entry (standalone)
- Single complex method
- LOC>80 indicates God-method
- May need full epic alone

---

### V12_002.Entries.Trend.cs (1 method)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ExecuteTREND_Preflight | 9 | 25 | |

**Cluster Candidate**: TREND Preflight (standalone or with Tier 1)
- Preflight validation logic
- Could cluster with ExecuteTRENDEntry (CYC=8, watch list)

---

### V12_002.BarUpdate.cs (2 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ProcessSessionReset | 11 | 32 | |
| OnBarUpdate | 10 | 51 | |

**Cluster Candidate**: Bar Update Processing
- Both handle bar update lifecycle
- Sequential workflow (session reset → bar update)
- Share session state

---

### V12_002.DrawingHelpers.cs (1 method)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| DrawORBox | 12 | 77 | |

**Cluster Candidate**: Drawing Helpers (standalone or with watch list)
- Could cluster with ConvertToSelectedTimeZone (CYC=7)

---

### V12_002.IPC.Hardening.cs (1 method)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| IsAllowlistBypassAttempt | 11 | 19 | |

**Cluster Candidate**: IPC Hardening (standalone)
- Security validation logic
- May be standalone epic

---

### V12_002.Lifecycle.cs (4 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| CleanupDictionaries | 13 | 22 | |
| ProcessOnStateChange | 10 | 11 | M5 candidate |
| DrainQueuesForShutdown | 9 | 28 | |
| ProcessOnConnectionStatusUpdate | 9 | 20 | |

**Cluster Candidate**: Lifecycle Management
- All handle lifecycle events
- Share state cleanup logic
- Sequential workflow (state change → drain → cleanup)

---

### V12_002.Orders.Callbacks.AccountOrders.cs (11 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ProcessAccountOrderQueue | 13 | 30 | |
| IsMasterReplaceCascadeCancellation | 13 | 26 | |
| ProcessFollowerCancellationSafe | 13 | 41 | |
| ProcessAccountOrder_UpdateMasterExpected | 12 | 21 | |
| ProcessAccountOrder_UpdateFleetExpected | 12 | 23 | |
| HandleMatchedFollower_PendingCancelReplace | 12 | 61 | |
| ExecuteFollowerCascadeCleanup | 12 | 60 | |
| ProcessFollowerCancellationUnconditional | 12 | 29 | |
| HandleMatchedFollower_StopReplacement | 14 | 23 | |
| OnAccountOrderUpdate | 14 | 26 | |
| HandleMatchedFollowerOrder | 14 | 65 | |

**Cluster Candidate**: Account Order Callbacks
- All handle account order lifecycle
- Share order queue and follower state
- Complex call graph (ProcessQueue → OnUpdate → HandleMatched → Cascade)
- **LARGE CLUSTER** (11 methods) - may need to split

**Potential Sub-Clusters**:
1. **Queue Processing** (3 methods): ProcessAccountOrderQueue, ProcessFollowerCancellationSafe, ProcessFollowerCancellationUnconditional
2. **Master/Fleet Updates** (2 methods): ProcessAccountOrder_UpdateMasterExpected, ProcessAccountOrder_UpdateFleetExpected
3. **Follower Matching** (4 methods): HandleMatchedFollowerOrder, HandleMatchedFollower_StopReplacement, HandleMatchedFollower_PendingCancelReplace, ExecuteFollowerCascadeCleanup
4. **Cascade Logic** (2 methods): IsMasterReplaceCascadeCancellation, OnAccountOrderUpdate

---

### V12_002.Orders.Callbacks.cs (6 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| RequestStopCancelLifecycleSafe | 12 | 22 | |
| HandleOrderRejected | 12 | 29 | |
| HandleOrderPriceOrQuantityChanged | 11 | 37 | |
| HandleOrderCancelled_ProcessStopReplacement | 10 | 20 | |
| ProcessOnOrderUpdate | 19 | 48 | **Tier 1** (CYC≥15), M5 candidate |
| HandleSecondaryOrderFilled | 21 | 69 | **Tier 1** (CYC≥15) |

**Note**: 2 methods are Tier 1 (CYC≥15), only 4 are Tier 2

**Cluster Candidate**: Order Callbacks (Tier 2 only)
- 4 methods handle order lifecycle events
- Share order state and cancellation logic
- Could cluster with Tier 1 methods for comprehensive refactor

---

### V12_002.Orders.Callbacks.Execution.cs (3 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ProcessOnExecutionUpdate | 13 | 31 | |
| ProcessOnExecution_HandleStopFill | 13 | 28 | |
| BroadcastSyncTargetState | 9 | 11 | |

**Note**: HandleFlatPosition_CleanupActivePositions (CYC=17) is Tier 1

**Cluster Candidate**: Execution Callbacks (Tier 2 only)
- 3 methods handle execution updates
- Share stop/target fill logic
- Sequential workflow

---

### V12_002.Orders.Callbacks.Propagation.cs (6 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| PropagateMasterEntryMove | 14 | 60 | |
| IsValidTradeTypeToken | 13 | 20 | |
| ResolveFollowersViaScan_ProcessEntry | 12 | 22 | |
| ResolveMasterTradeType | 11 | 16 | |
| PropagateMaster_ApplyFollowerMove | 11 | 24 | |
| PropagateMasterTargetMove | 9 | 60 | |

**Note**: PropagateMaster_IdentifyMove (CYC=18) is Tier 1

**Cluster Candidate**: Master Propagation
- All handle master-to-follower propagation
- Share trade type resolution and follower scanning
- Complex call graph

---

### V12_002.Orders.Management.Cleanup.cs (6 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| BuildLiveBrokerOrderIndex | 14 | 23 | |
| CleanupPosition | 12 | 36 | |
| PurgePositionIfEligible | 11 | 21 | |
| ClassifyOrphanReason | 11 | 33 | |
| PurgeGhostOrderReferences | 11 | 42 | |
| EvaluateZombiePurgeEligibility | 9 | 33 | |

**Note**: ValidateOrphanedMasterOrders (CYC=19) is Tier 1

**Cluster Candidate**: Order Cleanup
- All handle orphaned order cleanup
- Share position purge logic
- Sequential workflow (validate → classify → purge)

---

### V12_002.Orders.Management.cs (1 method)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| SubmitTargetOrdersLoop | 10 | 104 | LOC>80 |

**Cluster Candidate**: Target Order Management (standalone)
- Single complex method
- LOC>80 indicates God-method
- May need full epic alone

---

### V12_002.Orders.Management.Flatten.cs (3 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| HasActiveOrPendingOrderForEntry | 12 | 15 | |
| CancelAllBracketOrdersForPosition | 11 | 9 | |
| FlattenSinglePosition | 16 | 76 | **Tier 1** (CYC≥15) |

**Note**: 1 method is Tier 1, 2 are Tier 2

**Cluster Candidate**: Flatten Operations (Tier 2 only)
- 2 methods handle flatten prerequisites
- Could cluster with FlattenSinglePosition (Tier 1) for comprehensive refactor

---

### V12_002.Orders.Management.StopSync.cs (4 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| RefreshActivePositionOrders | 13 | 49 | |
| UpdateStopQuantity | 12 | 59 | |
| ValidateStopPrice | 9 | 32 | |
| RestoreCascadedTargets | 16 | 90 | **Tier 1** (CYC≥15), LOC>80 |

**Note**: SyncLimitTarget (CYC=17, LOC=128) is also Tier 1

**Cluster Candidate**: Stop Sync Operations (Tier 2 only)
- 3 methods handle stop sync prerequisites
- Share stop validation and quantity update logic
- Could cluster with Tier 1 methods for comprehensive refactor

---

### V12_002.Perf.LogBuffer.cs (1 method)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| FormatInternal | 13 | 27 | |

**Cluster Candidate**: Performance Logging (standalone)
- Single method
- May be standalone epic or cluster with other perf methods

---

### V12_002.REAPER.Audit.cs (6 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| AuditSingleFleetAccount | 13 | 48 | |
| AuditMaster_CheckExpectedActual | 13 | 23 | |
| AuditFleet_CalculateExpectedActual | 12 | 41 | |
| AuditMaster_HandleDesyncFlatten | 10 | 20 | |
| ProcessReaperFlatten_CancelWorkingOrders | 10 | 19 | |
| AuditFleet_CheckWorkingStop | 9 | 8 | |

**Note**: AuditMaster_HandleNakedPosition (CYC=15) is Tier 1

**Cluster Candidate**: REAPER Audit
- All handle audit logic
- Share expected/actual calculation
- Sequential workflow (audit → check → flatten)

---

### V12_002.REAPER.Repair.cs (1 method)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| SubmitRepairOrderWithAuthorization | 14 | 67 | |

**Cluster Candidate**: REAPER Repair (standalone or with Audit)
- Could cluster with REAPER Audit methods

---

### V12_002.Safety.Watchdog.cs (3 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| CancelWatchdogWorkingOrders | 12 | 19 | |
| OnWatchdogTimer | 11 | 30 | |
| CancelDirectFallbackOrders | 11 | 18 | |

**Cluster Candidate**: Watchdog Safety
- All handle watchdog timer logic
- Share order cancellation
- Sequential workflow

---

### V12_002.SIMA.cs (1 method)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| SetRmaAnchorFromIpc | 13 | 17 | M5 candidate |

**Cluster Candidate**: SIMA Configuration (standalone)
- Single method
- May cluster with other SIMA config methods

---

### V12_002.SIMA.Dispatch.cs (2 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| Dispatch_ProcessFleetLoop | 14 | 113 | LOC>80 |
| Dispatch_PublishLimitEntryToPhoton | 11 | 95 | LOC>80 |

**Note**: Dispatch_PublishMarketBracketToPhoton (CYC=21, LOC=189) is Tier 1

**Cluster Candidate**: SIMA Dispatch
- 2 methods handle dispatch logic
- LOC>80 indicates God-methods
- Could cluster with Tier 1 method for comprehensive refactor

---

### V12_002.SIMA.Execution.cs (3 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ExecuteMultiAccountMarket | 13 | 78 | |
| ProcessSingleFleetRMAAccount | 12 | 106 | LOC>80 |
| ExecuteMultiAccountBracket | 9 | 107 | LOC>80 |

**Note**: ExecuteRMAEntryV2 (CYC=9, LOC=110) is also Tier 2

**Cluster Candidate**: SIMA Execution
- 4 methods handle multi-account execution
- Share fleet processing logic
- LOC>80 indicates God-methods

---

### V12_002.SIMA.Flatten.cs (2 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ProcessFlattenWorkItem_CancelOrders | 18 | 36 | **Tier 1** (CYC≥15) |
| EmergencyFlattenSingleFleetAccount | 13 | 67 | |

**Note**: 1 method is Tier 1, 1 is Tier 2

**Cluster Candidate**: SIMA Flatten (Tier 2 only)
- Could cluster with Tier 1 method for comprehensive refactor

---

### V12_002.SIMA.Fleet.cs (5 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| VerifyPhotonSlotIntegrity | 14 | 43 | |
| InitializeFollowerBracketFSM | 13 | 34 | |
| ProcessFleetSlot | 11 | 37 | |
| SubmitAndRegisterFleetOrders | 11 | 30 | |
| DrainAllDispatchQueuesOnAbort | 11 | 23 | |

**Note**: ShouldSkipFleet_RunHealthCheck (CYC=31) is Tier 1

**Cluster Candidate**: SIMA Fleet Management
- 5 methods handle fleet slot processing
- Share FSM initialization and order submission
- Sequential workflow

---

### V12_002.SIMA.Lifecycle.cs (8 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ClassifyAndRouteFleetOrder | 16 | 42 | **Tier 1** (CYC≥15) |
| SweepTrackedOrders | 12 | 34 | |
| SweepBrokerOrders | 12 | 38 | |
| DrainPhotonQueuesOnShutdown | 11 | 21 | |
| ShouldProtectBracketOrder | 10 | 16 | |
| AdoptMasterWorkingOrders | 9 | 37 | |
| HydrateFSM_MapOrderStateToFsmState | 9 | 14 | |
| HydrateFSMsFromWorkingOrders | 9 | 45 | |

**Note**: AdoptFleetWorkingOrders (CYC=17) is also Tier 1

**Cluster Candidate**: SIMA Lifecycle
- 7 Tier 2 methods handle lifecycle events
- Share order adoption and FSM hydration
- Complex workflow

---

### V12_002.SIMA.Shadow.cs (2 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ShadowProcessFollowerStopUpdate | 12 | 31 | |
| ShadowPropagateStopMoves | 20 | 32 | **Tier 1** (CYC≥15) |

**Note**: 1 method is Tier 1, 1 is Tier 2

**Cluster Candidate**: SIMA Shadow (Tier 2 only)
- Could cluster with Tier 1 method for comprehensive refactor

---

### V12_002.StickyState.cs (1 method)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| DeserializeSnapshot | 9 | 39 | |

**Cluster Candidate**: Sticky State (standalone or with watch list)
- Could cluster with other state methods

---

### V12_002.Symmetry.BracketFSM.cs (5 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ProcessBracketEvent | 14 | 44 | M5 candidate |
| GetFsmExpectedPosition | 14 | 25 | |
| HandleFsmFilled | 13 | 18 | |
| ResolveFsm_ByScan | 12 | 21 | |
| RemoveFsmOrderIdMappings | 11 | 14 | |

**Cluster Candidate**: Bracket FSM
- All handle FSM state transitions
- Share event processing and position calculation
- Sequential workflow

---

### V12_002.Symmetry.cs (2 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| SymmetryGuardOnMasterFill | 14 | 44 | |
| SymmetryFindDispatchForMasterFill | 9 | 20 | |

**Cluster Candidate**: Symmetry Guard
- Both handle master fill events
- Share dispatch resolution

---

### V12_002.Symmetry.Follower.cs (3 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| SymmetryGuardTryResolveFollower | 12 | 83 | LOC>80 |
| SymmetryGuardSubmitFollowerBracket | 12 | 101 | LOC>80 |
| SymmetryGuardOnFollowerFill | 11 | 47 | |

**Cluster Candidate**: Symmetry Follower
- All handle follower bracket logic
- Share follower resolution and submission
- LOC>80 indicates God-methods

---

### V12_002.Symmetry.Replace.cs (5 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| SymmetryGuardReplaceExistingFollowerTarget | 18 | 49 | **Tier 1** (CYC≥15) |
| SymmetryGuardTryResolveFollowersForDispatch | 18 | 33 | **Tier 1** (CYC≥15) |
| SymmetryGuardCascadeFollowerCleanup | 10 | 33 | |
| SymmetryGuardPruneDispatches | 10 | 20 | |
| SymmetryNormalizeTradeType | 10 | 17 | |

**Note**: 2 methods are Tier 1, 3 are Tier 2

**Cluster Candidate**: Symmetry Replace (Tier 2 only)
- 3 methods handle follower replacement
- Could cluster with Tier 1 methods for comprehensive refactor

---

### V12_002.Trailing.Breakeven.cs (3 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| MoveStop_SinglePosition | 13 | 63 | |
| MoveSpecificTarget | 12 | 41 | |
| FindTargetOrderForPosition | 9 | 23 | |

**Cluster Candidate**: Trailing Breakeven
- All handle breakeven stop moves
- Share target finding and move logic
- Sequential workflow

---

### V12_002.Trailing.cs (3 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ManageTrailingStops | 13 | 33 | |
| FleetSync_SyncFollowersToLevel | 9 | 34 | |
| ManageTrail_RunPerTradeBranches | 9 | 8 | |

**Cluster Candidate**: Trailing Stop Management
- All handle trailing stop logic
- Share fleet sync and per-trade branching
- Sequential workflow

---

### V12_002.Trailing.StopUpdate.cs (4 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| UpdateStopOrder | 11 | 33 | |
| InitiateStopReplacement | 10 | 46 | |
| CleanupStalePendingReplacements | 9 | 26 | |
| UpdateExistingPendingReplacement | 9 | 46 | |

**Cluster Candidate**: Stop Update Operations
- All handle stop order updates
- Share replacement and cleanup logic
- Sequential workflow

---

### V12_002.UI.Callbacks.cs (2 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| HandleChartClick_ConvertPrice | 9 | 54 | |
| OnKeyDown | 9 | 17 | |

**Cluster Candidate**: UI Callbacks (standalone or with watch list)
- Could cluster with other UI callback methods

---

### V12_002.UI.Compliance.cs (7 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| IsOrderAllowed | 16 | 43 | **Tier 1** (CYC≥15) |
| HandleFleetTargetFill | 16 | 58 | **Tier 1** (CYC≥15) |
| CancelOrphanedTargets | 14 | 20 | |
| ProcessQueuedExecution_HandleFleetOCO | 13 | 17 | |
| ProcessQueuedExecution_SyncFlatPosition | 13 | 36 | |
| LogApexPerformance | 13 | 72 | |
| ProcessQueuedExecution_HandleFleetBrackets | 10 | 18 | |

**Note**: 2 methods are Tier 1, 5 are Tier 2, 1 is watch list (TrackTradeEntry CYC=9)

**Cluster Candidate**: UI Compliance
- 5 Tier 2 methods handle compliance checks
- Share fleet execution and orphan cleanup
- Could cluster with Tier 1 methods for comprehensive refactor

---

### V12_002.UI.IPC.Commands.Config.cs (3 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| TryApplyConfigTarget_Value | 17 | 45 | **Tier 1** (CYC≥15), M5 candidate |
| HandleTrimCommand | 11 | 85 | LOC>80 |
| TryApplyConfigTarget_Type | 11 | 22 | |

**Note**: 1 method is Tier 1, 2 are Tier 2

**Cluster Candidate**: IPC Config Commands (Tier 2 only)
- 2 methods handle config commands
- Could cluster with Tier 1 method for comprehensive refactor

---

### V12_002.UI.IPC.Commands.Fleet.cs (6 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| TryHandleFleet_CancelAll | 19 | 41 | **Tier 1** (CYC≥15) |
| CancelAll_ProcessSingleFleetAccount | 18 | 31 | **Tier 1** (CYC≥15) |
| TryHandleFleet_MoveTarget | 15 | 33 | **Tier 1** (CYC≥15) |
| CancelAll_ProcessMasterAccount | 14 | 24 | |
| TryHandleFleet_LongShort | 11 | 47 | M5 candidate |
| TryHandleFleetCommand | 19 | 42 | **Tier 1** (CYC≥15) |

**Note**: 4 methods are Tier 1, 2 are Tier 2

**Cluster Candidate**: IPC Fleet Commands (Tier 2 only)
- 2 methods handle fleet commands
- Could cluster with Tier 1 methods for comprehensive refactor

---

### V12_002.UI.IPC.Commands.Misc.cs (2 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| SendResponseToRemote | 10 | 26 | |
| FlattenSpecificTarget | 9 | 28 | |

**Cluster Candidate**: IPC Misc Commands
- Both handle misc IPC commands
- Share response sending

---

### V12_002.UI.IPC.Commands.Mode.cs (2 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| TryHandleMode_SetMode | 13 | 59 | M5 candidate |
| TryHandleRisk_Breakeven | 12 | 16 | |

**Cluster Candidate**: IPC Mode Commands
- Both handle mode/risk commands
- Share mode setting logic

---

### V12_002.UI.IPC.cs (4 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| IsSymbolMatch | 18 | 19 | **Tier 1** (CYC≥15) |
| ProcessIpcCommands | 14 | 27 | |
| TryParseTargetMode | 13 | 27 | |
| ProcessIpcCommandCore | 13 | 50 | M5 candidate |

**Note**: 1 method is Tier 1, 3 are Tier 2, 1 is watch list (IsAllowedIpcAction CYC=10)

**Cluster Candidate**: IPC Core
- 3 Tier 2 methods handle IPC command processing
- Share command parsing and routing
- Could cluster with Tier 1 method for comprehensive refactor

---

### V12_002.UI.IPC.Server.cs (2 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| StopIpcServer | 12 | 29 | |
| ProcessClientStream | 9 | 26 | |

**Cluster Candidate**: IPC Server
- Both handle IPC server lifecycle
- Share client stream processing

---

### V12_002.UI.Panel.Construction.cs (3 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| DestroyPanel | 17 | 149 | **Tier 1** (CYC≥15), LOC>80 |
| PlacePanel | 13 | 56 | |
| CreateSection0_Identity | 13 | 154 | LOC>80 |

**Note**: 1 method is Tier 1, 2 are Tier 2

**Cluster Candidate**: Panel Construction (Tier 2 only)
- 2 methods handle panel construction
- LOC>80 indicates God-methods
- Could cluster with Tier 1 method for comprehensive refactor

---

### V12_002.UI.Panel.Handlers.cs (5 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ShowModeSpecificControls | 20 | 42 | **Tier 1** (CYC≥15) |
| UpdateTargetVisibility | 19 | 36 | **Tier 1** (CYC≥15) |
| AttachExecutionPanelHandlers | 12 | 46 | |
| OnSubmitClick | 12 | 30 | |
| CollapseAllExecutionControls | 11 | 21 | |

**Note**: 2 methods are Tier 1, 3 are Tier 2

**Cluster Candidate**: Panel Handlers (Tier 2 only)
- 3 methods handle panel event handlers
- Could cluster with Tier 1 methods for comprehensive refactor

---

### V12_002.UI.Panel.Helpers.cs (4 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| FindChartTraderViaChartTab | 20 | 54 | **Tier 1** (CYC≥15) |
| DumpVisualTree | 10 | 48 | |
| FindChartTabGrid | 10 | 23 | |
| FindChartTraderBySiblingSearch | 9 | 16 | |

**Note**: 1 method is Tier 1, 3 are Tier 2

**Cluster Candidate**: Panel Helpers (Tier 2 only)
- 3 methods handle panel helper utilities
- Share chart trader finding logic
- Could cluster with Tier 1 method for comprehensive refactor

---

### V12_002.UI.Panel.StateSync.cs (4 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| UpdatePanelState | 16 | 51 | **Tier 1** (CYC≥15) |
| SyncPanelConfigFromSnapshot | 15 | 37 | **Tier 1** (CYC≥15) |
| SyncLiveTargetRows | 10 | 21 | |
| SyncModeChipVisuals | 9 | 39 | |

**Note**: 2 methods are Tier 1, 2 are Tier 2

**Cluster Candidate**: Panel State Sync (Tier 2 only)
- 2 methods handle panel state sync
- Could cluster with Tier 1 methods for comprehensive refactor

---

### V12_002.UI.Sizing.cs (1 method)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| ShouldSyncPendingOrder | 9 | 15 | |

**Cluster Candidate**: UI Sizing (standalone or with watch list)
- Could cluster with SyncPendingOrders (CYC=7)

---

### V12_002.UI.Snapshot.cs (2 methods)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| FindMasterPosition | 9 | 15 | |
| PopulateTargetSnapshots | 9 | 21 | |

**Cluster Candidate**: UI Snapshot
- Both handle snapshot population
- Share master position finding

---

### V12_002.UI.SnapshotPool.cs (1 method)
| Method | CYC | LOC | Notes |
|--------|-----|-----|-------|
| UpdateLivePositionSnapshot | 14 | 43 | |

**Cluster Candidate**: Snapshot Pool (standalone or with Snapshot)
- Could cluster with UI.Snapshot methods

---

## Tier 2 Summary by Cluster Size

### Large Clusters (7+ methods)
1. **V12_002.Orders.Callbacks.AccountOrders.cs**: 11 methods (may need sub-clustering)
2. **V12_002.SIMA.Lifecycle.cs**: 7 methods (Tier 2 only)

### Medium Clusters (4-6 methods)
3. **V12_002.Orders.Callbacks.Propagation.cs**: 6 methods
4. **V12_002.Orders.Management.Cleanup.cs**: 6 methods
5. **V12_002.REAPER.Audit.cs**: 6 methods
6. **V12_002.Symmetry.BracketFSM.cs**: 5 methods
7. **V12_002.SIMA.Fleet.cs**: 5 methods
8. **V12_002.UI.Compliance.cs**: 5 methods (Tier 2 only)
9. **V12_002.Lifecycle.cs**: 4 methods
10. **V12_002.Orders.Management.StopSync.cs**: 3 methods (Tier 2 only, could add Tier 1)
11. **V12_002.Trailing.StopUpdate.cs**: 4 methods

### Small Clusters (2-3 methods)
12. **V12_002.Entries.FFMA.cs**: 2 methods
13. **V12_002.BarUpdate.cs**: 2 methods
14. **V12_002.Orders.Callbacks.cs**: 4 methods (Tier 2 only)
15. **V12_002.Orders.Callbacks.Execution.cs**: 3 methods
16. **V12_002.Safety.Watchdog.cs**: 3 methods
17. **V12_002.SIMA.Dispatch.cs**: 2 methods
18. **V12_002.SIMA.Execution.cs**: 4 methods
19. **V12_002.Symmetry.cs**: 2 methods
20. **V12_002.Symmetry.Follower.cs**: 3 methods
21. **V12_002.Symmetry.Replace.cs**: 3 methods (Tier 2 only)
22. **V12_002.Trailing.Breakeven.cs**: 3 methods
23. **V12_002.Trailing.cs**: 3 methods
24. **V12_002.UI.IPC.Commands.Config.cs**: 2 methods (Tier 2 only)
25. **V12_002.UI.IPC.Commands.Fleet.cs**: 2 methods (Tier 2 only)
26. **V12_002.UI.IPC.Commands.Misc.cs**: 2 methods
27. **V12_002.UI.IPC.Commands.Mode.cs**: 2 methods
28. **V12_002.UI.IPC.cs**: 3 methods (Tier 2 only)
29. **V12_002.UI.IPC.Server.cs**: 2 methods
30. **V12_002.UI.Panel.Construction.cs**: 2 methods (Tier 2 only)
31. **V12_002.UI.Panel.Handlers.cs**: 3 methods (Tier 2 only)
32. **V12_002.UI.Panel.Helpers.cs**: 3 methods (Tier 2 only)
33. **V12_002.UI.Panel.StateSync.cs**: 2 methods (Tier 2 only)
34. **V12_002.UI.Snapshot.cs**: 2 methods

### Standalone Methods (1 method)
35. **V12_002.Entries.MOMO.cs**: 1 method (LOC>80)
36. **V12_002.Entries.OR.cs**: 1 method (LOC>80)
37. **V12_002.Entries.Retest.cs**: 1 method (LOC>80)
38. **V12_002.Entries.Trend.cs**: 1 method
39. **V12_002.DrawingHelpers.cs**: 1 method
40. **V12_002.IPC.Hardening.cs**: 1 method
41. **V12_002.Orders.Management.cs**: 1 method (LOC>80)
42. **V12_002.Perf.LogBuffer.cs**: 1 method
43. **V12_002.REAPER.Repair.cs**: 1 method
44. **V12_002.SIMA.cs**: 1 method
45. **V12_002.SIMA.Flatten.cs**: 1 method (Tier 2 only)
46. **V12_002.SIMA.Shadow.cs**: 1 method (Tier 2 only)
47. **V12_002.StickyState.cs**: 1 method
48. **V12_002.UI.Callbacks.cs**: 2 methods
49. **V12_002.UI.Sizing.cs**: 1 method
50. **V12_002.UI.SnapshotPool.cs**: 1 method

---

## Next Steps

1. **jCodemunch Analysis**: For each cluster, run:
   - `get_file_outline` to see all methods
   - `get_call_hierarchy` for key methods
   - `find_references` for shared state
   - `get_blast_radius` for impact analysis

2. **Cluster Validation**: Verify each cluster:
   - Methods call each other OR share state
   - Logical cohesion (same subsystem)
   - Reasonable size (2-10 methods)
   - Can be refactored together

3. **Epic Roadmap**: Create final roadmap with:
   - Epic ID
   - Cluster name
   - Method list with CYC/LOC
   - Rationale
   - Estimated bobcoin cost

4. **Budget Calculation**: 
   - Count final epic count
   - Multiply by 80 bobcoins
   - Add Tier 1 budget
   - Calculate total APIs needed

---

**Status**: Analysis complete, ready for jCodemunch validation
**Next**: Run jCodemunch analysis on top 10 clusters