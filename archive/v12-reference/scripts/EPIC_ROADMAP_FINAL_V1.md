# Epic Roadmap - Final V1 (Option B: Combined Clustering)

**Date**: 2026-06-14
**Strategy**: Combined Tier 1 + Tier 2 clustering
**Total Epics**: 80
**Total Cost**: 6,400 bobcoins (40 APIs)
**Current Budget**: 2,080 bobcoins (13 APIs)
**Additional APIs Needed**: 27 APIs (4,320 bobcoins)

---

## Executive Summary

This roadmap implements **Option B (Combined Clustering)** where files containing both Tier 1 (CYC ≥15) and Tier 2 (CYC 9-14) methods are refactored together in a single epic. This approach:

- **Reduces Epic Count**: 80 epics vs 95 (15 fewer)
- **Saves Cost**: 6,400 bobcoins vs 7,600 (1,200 saved)
- **Improves Quality**: Comprehensive file refactoring reduces cross-file dependency risks
- **Aligns with Jane Street**: Holistic module refactoring over piecemeal changes

---

## Epic Distribution

### Category 1: Mixed Tier Files (15 epics)
Files containing both Tier 1 (CYC ≥15) and Tier 2 (CYC 9-14) methods

### Category 2: Pure Tier 1 (30 epics)
Files with only Tier 1 methods (CYC ≥15)

### Category 3: Pure Tier 2 (35 epics)
Files with only Tier 2 methods (CYC 9-14)

---

## Category 1: Mixed Tier Files (15 Epics)

### EPIC-001: V12_002.Orders.Callbacks.cs Refactoring
**File**: `V12_002.Orders.Callbacks.cs`
**Methods**: 6 total (2 Tier 1 + 4 Tier 2)

**Tier 1 Methods**:
1. HandleSecondaryOrderFilled (CYC=21, LOC=69)
2. ProcessOnOrderUpdate (CYC=19, LOC=48) - M5 candidate

**Tier 2 Methods**:
3. RequestStopCancelLifecycleSafe (CYC=12, LOC=22)
4. HandleOrderRejected (CYC=12, LOC=29)
5. HandleOrderPriceOrQuantityChanged (CYC=11, LOC=37)
6. HandleOrderCancelled_ProcessStopReplacement (CYC=10, LOC=20)

**Rationale**: All handle order lifecycle callbacks, share order state and cancellation logic
**Priority**: HIGH (M5 candidate)
**Estimated Cost**: 80 bobcoins

---

### EPIC-002: V12_002.Orders.Management.Flatten.cs Refactoring
**File**: `V12_002.Orders.Management.Flatten.cs`
**Methods**: 4 total (2 Tier 1 + 2 Tier 2)

**Tier 1 Methods**:
1. ManageCIT (CYC=19, LOC=77)
2. FlattenSinglePosition (CYC=16, LOC=76)

**Tier 2 Methods**:
3. HasActiveOrPendingOrderForEntry (CYC=12, LOC=15)
4. CancelAllBracketOrdersForPosition (CYC=11, LOC=9)

**Rationale**: All handle position flattening, share CIT management and bracket cancellation
**Priority**: HIGH (critical safety feature)
**Estimated Cost**: 80 bobcoins

---

### EPIC-003: V12_002.Orders.Management.StopSync.cs Refactoring
**File**: `V12_002.Orders.Management.StopSync.cs`
**Methods**: 5 total (2 Tier 1 + 3 Tier 2)

**Tier 1 Methods**:
1. SyncLimitTarget (CYC=17, LOC=128) - LOC>80
2. RestoreCascadedTargets (CYC=16, LOC=90) - LOC>80

**Tier 2 Methods**:
3. RefreshActivePositionOrders (CYC=13, LOC=49)
4. UpdateStopQuantity (CYC=12, LOC=59)
5. ValidateStopPrice (CYC=9, LOC=32)

**Rationale**: All handle stop sync operations, share target restoration and validation
**Priority**: HIGH (God-methods with LOC>80)
**Estimated Cost**: 80 bobcoins

---

### EPIC-004: V12_002.SIMA.Dispatch.cs Refactoring
**File**: `V12_002.SIMA.Dispatch.cs`
**Methods**: 3 total (1 Tier 1 + 2 Tier 2)

**Tier 1 Methods**:
1. Dispatch_PublishMarketBracketToPhoton (CYC=21, LOC=189) - LOC>80

**Tier 2 Methods**:
2. Dispatch_ProcessFleetLoop (CYC=14, LOC=113) - LOC>80
3. Dispatch_PublishLimitEntryToPhoton (CYC=11, LOC=95) - LOC>80

**Rationale**: All handle SIMA dispatch logic, share fleet processing and Photon publishing
**Priority**: HIGH (God-methods with LOC>80)
**Estimated Cost**: 80 bobcoins

---

### EPIC-005: V12_002.SIMA.Flatten.cs Refactoring
**File**: `V12_002.SIMA.Flatten.cs`
**Methods**: 2 total (1 Tier 1 + 1 Tier 2)

**Tier 1 Methods**:
1. ProcessFlattenWorkItem_CancelOrders (CYC=18, LOC=36)

**Tier 2 Methods**:
2. EmergencyFlattenSingleFleetAccount (CYC=13, LOC=67)

**Rationale**: Both handle SIMA flatten operations, share emergency flatten logic
**Priority**: HIGH (critical safety feature)
**Estimated Cost**: 80 bobcoins

---

### EPIC-006: V12_002.SIMA.Lifecycle.cs Refactoring
**File**: `V12_002.SIMA.Lifecycle.cs`
**Methods**: 9 total (2 Tier 1 + 7 Tier 2)

**Tier 1 Methods**:
1. AdoptFleetWorkingOrders (CYC=17, LOC=46)
2. ClassifyAndRouteFleetOrder (CYC=16, LOC=42)

**Tier 2 Methods**:
3. SweepTrackedOrders (CYC=12, LOC=34)
4. SweepBrokerOrders (CYC=12, LOC=38)
5. DrainPhotonQueuesOnShutdown (CYC=11, LOC=21)
6. ShouldProtectBracketOrder (CYC=10, LOC=16)
7. AdoptMasterWorkingOrders (CYC=9, LOC=37)
8. HydrateFSM_MapOrderStateToFsmState (CYC=9, LOC=14)
9. HydrateFSMsFromWorkingOrders (CYC=9, LOC=45)

**Rationale**: All handle SIMA lifecycle events, share order adoption and FSM hydration
**Priority**: HIGH (large cluster, critical subsystem)
**Estimated Cost**: 80 bobcoins

---

### EPIC-007: V12_002.SIMA.Shadow.cs Refactoring
**File**: `V12_002.SIMA.Shadow.cs`
**Methods**: 2 total (1 Tier 1 + 1 Tier 2)

**Tier 1 Methods**:
1. ShadowPropagateStopMoves (CYC=20, LOC=32)

**Tier 2 Methods**:
2. ShadowProcessFollowerStopUpdate (CYC=12, LOC=31)

**Rationale**: Both handle SIMA shadow propagation, share follower stop update logic
**Priority**: MEDIUM
**Estimated Cost**: 80 bobcoins

---

### EPIC-008: V12_002.Symmetry.Replace.cs Refactoring
**File**: `V12_002.Symmetry.Replace.cs`
**Methods**: 5 total (2 Tier 1 + 3 Tier 2)

**Tier 1 Methods**:
1. SymmetryGuardReplaceExistingFollowerTarget (CYC=18, LOC=49)
2. SymmetryGuardTryResolveFollowersForDispatch (CYC=18, LOC=33)

**Tier 2 Methods**:
3. SymmetryGuardCascadeFollowerCleanup (CYC=10, LOC=33)
4. SymmetryGuardPruneDispatches (CYC=10, LOC=20)
5. SymmetryNormalizeTradeType (CYC=10, LOC=17)

**Rationale**: All handle symmetry follower replacement, share dispatch resolution
**Priority**: MEDIUM
**Estimated Cost**: 80 bobcoins

---

### EPIC-009: V12_002.UI.Compliance.cs Refactoring
**File**: `V12_002.UI.Compliance.cs`
**Methods**: 8 total (2 Tier 1 + 6 Tier 2)

**Tier 1 Methods**:
1. IsOrderAllowed (CYC=16, LOC=43)
2. HandleFleetTargetFill (CYC=16, LOC=58)

**Tier 2 Methods**:
3. CancelOrphanedTargets (CYC=14, LOC=20)
4. ProcessQueuedExecution_HandleFleetOCO (CYC=13, LOC=17)
5. ProcessQueuedExecution_SyncFlatPosition (CYC=13, LOC=36)
6. LogApexPerformance (CYC=13, LOC=72)
7. ProcessQueuedExecution_HandleFleetBrackets (CYC=10, LOC=18)
8. TrackTradeEntry (CYC=9, LOC=17)

**Rationale**: All handle UI compliance checks, share fleet execution and orphan cleanup
**Priority**: MEDIUM
**Estimated Cost**: 80 bobcoins

---

### EPIC-010: V12_002.UI.IPC.Commands.Config.cs Refactoring
**File**: `V12_002.UI.IPC.Commands.Config.cs`
**Methods**: 3 total (1 Tier 1 + 2 Tier 2)

**Tier 1 Methods**:
1. TryApplyConfigTarget_Value (CYC=17, LOC=45) - M5 candidate

**Tier 2 Methods**:
2. HandleTrimCommand (CYC=11, LOC=85) - LOC>80
3. TryApplyConfigTarget_Type (CYC=11, LOC=22)

**Rationale**: All handle IPC config commands, share config target application
**Priority**: HIGH (M5 candidate, God-method)
**Estimated Cost**: 80 bobcoins

---

### EPIC-011: V12_002.UI.IPC.Commands.Fleet.cs Refactoring
**File**: `V12_002.UI.IPC.Commands.Fleet.cs`
**Methods**: 6 total (4 Tier 1 + 2 Tier 2)

**Tier 1 Methods**:
1. TryHandleFleetCommand (CYC=19, LOC=42)
2. TryHandleFleet_CancelAll (CYC=19, LOC=41)
3. CancelAll_ProcessSingleFleetAccount (CYC=18, LOC=31)
4. TryHandleFleet_MoveTarget (CYC=15, LOC=33)

**Tier 2 Methods**:
5. CancelAll_ProcessMasterAccount (CYC=14, LOC=24)
6. TryHandleFleet_LongShort (CYC=11, LOC=47) - M5 candidate

**Rationale**: All handle IPC fleet commands, share fleet cancellation and move logic
**Priority**: HIGH (M5 candidate, large cluster)
**Estimated Cost**: 80 bobcoins

---

### EPIC-012: V12_002.UI.IPC.cs Refactoring
**File**: `V12_002.UI.IPC.cs`
**Methods**: 5 total (1 Tier 1 + 4 Tier 2)

**Tier 1 Methods**:
1. IsSymbolMatch (CYC=18, LOC=19)

**Tier 2 Methods**:
2. ProcessIpcCommands (CYC=14, LOC=27)
3. TryParseTargetMode (CYC=13, LOC=27)
4. ProcessIpcCommandCore (CYC=13, LOC=50) - M5 candidate
5. IsAllowedIpcAction (CYC=10, LOC=13)

**Rationale**: All handle IPC core processing, share command parsing and routing
**Priority**: HIGH (M5 candidate)
**Estimated Cost**: 80 bobcoins

---

### EPIC-013: V12_002.UI.Panel.Construction.cs Refactoring
**File**: `V12_002.UI.Panel.Construction.cs`
**Methods**: 3 total (1 Tier 1 + 2 Tier 2)

**Tier 1 Methods**:
1. DestroyPanel (CYC=17, LOC=149) - LOC>80

**Tier 2 Methods**:
2. PlacePanel (CYC=13, LOC=56)
3. CreateSection0_Identity (CYC=13, LOC=154) - LOC>80

**Rationale**: All handle panel construction, share panel lifecycle management
**Priority**: MEDIUM (God-methods with LOC>80)
**Estimated Cost**: 80 bobcoins

---

### EPIC-014: V12_002.UI.Panel.Handlers.cs Refactoring
**File**: `V12_002.UI.Panel.Handlers.cs`
**Methods**: 5 total (2 Tier 1 + 3 Tier 2)

**Tier 1 Methods**:
1. ShowModeSpecificControls (CYC=20, LOC=42)
2. UpdateTargetVisibility (CYC=19, LOC=36)

**Tier 2 Methods**:
3. AttachExecutionPanelHandlers (CYC=12, LOC=46)
4. OnSubmitClick (CYC=12, LOC=30)
5. CollapseAllExecutionControls (CYC=11, LOC=21)

**Rationale**: All handle panel event handlers, share mode-specific control logic
**Priority**: MEDIUM
**Estimated Cost**: 80 bobcoins

---

### EPIC-015: V12_002.UI.Panel.StateSync.cs Refactoring
**File**: `V12_002.UI.Panel.StateSync.cs`
**Methods**: 4 total (2 Tier 1 + 2 Tier 2)

**Tier 1 Methods**:
1. UpdatePanelState (CYC=16, LOC=51)
2. SyncPanelConfigFromSnapshot (CYC=15, LOC=37)

**Tier 2 Methods**:
3. SyncLiveTargetRows (CYC=10, LOC=21)
4. SyncModeChipVisuals (CYC=9, LOC=39)

**Rationale**: All handle panel state sync, share snapshot synchronization
**Priority**: MEDIUM
**Estimated Cost**: 80 bobcoins

---

## Category 2: Pure Tier 1 (30 Epics)

### EPIC-016: V12_002.Entries.FFMA.cs - CheckFFMAConditions
**Method**: CheckFFMAConditions (CYC=16, LOC=50)
**Rationale**: FFMA entry condition validation
**Priority**: HIGH
**Estimated Cost**: 80 bobcoins

---

### EPIC-017: V12_002.Entries.FFMA.cs - ExecuteFFMAManualMarketEntry
**Method**: ExecuteFFMAManualMarketEntry (CYC=12, LOC=162) - LOC>80
**Rationale**: God-method, manual FFMA market entry
**Priority**: HIGH
**Estimated Cost**: 80 bobcoins

---

### EPIC-018: V12_002.Entries.RMA.cs - MonitorRmaProximity
**Method**: MonitorRmaProximity (CYC=17, LOC=67)
**Rationale**: RMA proximity monitoring
**Priority**: HIGH
**Estimated Cost**: 80 bobcoins

---

### EPIC-019: V12_002.Lifecycle.cs - CleanupDictionaries
**Method**: CleanupDictionaries (CYC=13, LOC=22)
**Rationale**: Lifecycle cleanup
**Priority**: MEDIUM
**Estimated Cost**: 80 bobcoins

---

### EPIC-020: V12_002.Orders.Callbacks.AccountOrders.cs Cluster 1
**Methods**: 11 total (all Tier 1 or high Tier 2)
**Sub-Cluster**: Queue Processing + Master/Fleet Updates
**Priority**: HIGH (large cluster)
**Estimated Cost**: 80 bobcoins

---

### EPIC-021: V12_002.Orders.Callbacks.AccountOrders.cs Cluster 2
**Methods**: Follower Matching + Cascade Logic
**Priority**: HIGH (large cluster)
**Estimated Cost**: 80 bobcoins

---

### EPIC-022: V12_002.Orders.Callbacks.Execution.cs - HandleFlatPosition_CleanupActivePositions
**Method**: HandleFlatPosition_CleanupActivePositions (CYC=17, LOC=30)
**Rationale**: Flat position cleanup
**Priority**: HIGH
**Estimated Cost**: 80 bobcoins

---

### EPIC-023: V12_002.Orders.Callbacks.Propagation.cs - PropagateMaster_IdentifyMove
**Method**: PropagateMaster_IdentifyMove (CYC=18, LOC=40)
**Rationale**: Master propagation identification
**Priority**: HIGH
**Estimated Cost**: 80 bobcoins

---

### EPIC-024: V12_002.Orders.Management.Cleanup.cs - ValidateOrphanedMasterOrders
**Method**: ValidateOrphanedMasterOrders (CYC=19, LOC=32)
**Rationale**: Orphaned order validation
**Priority**: HIGH
**Estimated Cost**: 80 bobcoins

---

### EPIC-025: V12_002.REAPER.Audit.cs - AuditMaster_HandleNakedPosition
**Method**: AuditMaster_HandleNakedPosition (CYC=15, LOC=38)
**Rationale**: Naked position audit
**Priority**: HIGH
**Estimated Cost**: 80 bobcoins

---

### EPIC-026: V12_002.SIMA.Fleet.cs - ShouldSkipFleet_RunHealthCheck
**Method**: ShouldSkipFleet_RunHealthCheck (CYC=31, LOC=57)
**Rationale**: Fleet health check (highest CYC in codebase)
**Priority**: CRITICAL
**Estimated Cost**: 80 bobcoins

---

### EPIC-027: V12_002.UI.Panel.Helpers.cs - FindChartTraderViaChartTab
**Method**: FindChartTraderViaChartTab (CYC=20, LOC=54)
**Rationale**: Chart trader finding
**Priority**: MEDIUM
**Estimated Cost**: 80 bobcoins

---

### EPIC-028 through EPIC-045: Remaining Pure Tier 1 Methods
**Count**: 18 additional epics
**Methods**: Various Tier 1 methods (CYC ≥15)
**Priority**: MEDIUM to HIGH
**Estimated Cost**: 80 bobcoins each (1,440 total)

---

## Category 3: Pure Tier 2 (35 Epics)

### EPIC-046: V12_002.BarUpdate.cs - Bar Update Processing
**Methods**: 2 methods
- ProcessSessionReset (CYC=11, LOC=32)
- OnBarUpdate (CYC=10, LOC=51)
**Rationale**: Sequential bar update workflow
**Priority**: MEDIUM
**Estimated Cost**: 80 bobcoins

---

### EPIC-047: V12_002.Lifecycle.cs - Lifecycle Management
**Methods**: 3 methods (excluding CleanupDictionaries which is Tier 1)
- ProcessOnStateChange (CYC=10, LOC=11) - M5 candidate
- DrainQueuesForShutdown (CYC=9, LOC=28)
- ProcessOnConnectionStatusUpdate (CYC=9, LOC=20)
**Rationale**: Lifecycle event handling
**Priority**: HIGH (M5 candidate)
**Estimated Cost**: 80 bobcoins

---

### EPIC-048: V12_002.Orders.Callbacks.Execution.cs - Execution Callbacks
**Methods**: 3 methods
- ProcessOnExecutionUpdate (CYC=13, LOC=31)
- ProcessOnExecution_HandleStopFill (CYC=13, LOC=28)
- BroadcastSyncTargetState (CYC=9, LOC=11)
**Rationale**: Execution update handling
**Priority**: HIGH
**Estimated Cost**: 80 bobcoins

---

### EPIC-049: V12_002.Orders.Callbacks.Propagation.cs - Master Propagation
**Methods**: 6 methods (excluding PropagateMaster_IdentifyMove which is Tier 1)
**Rationale**: Master-to-follower propagation
**Priority**: HIGH
**Estimated Cost**: 80 bobcoins

---

### EPIC-050: V12_002.Orders.Management.Cleanup.cs - Order Cleanup
**Methods**: 6 methods (excluding ValidateOrphanedMasterOrders which is Tier 1)
**Rationale**: Orphaned order cleanup workflow
**Priority**: HIGH
**Estimated Cost**: 80 bobcoins

---

### EPIC-051: V12_002.REAPER.Audit.cs - REAPER Audit
**Methods**: 6 methods (excluding AuditMaster_HandleNakedPosition which is Tier 1)
**Rationale**: Audit logic and expected/actual calculation
**Priority**: HIGH
**Estimated Cost**: 80 bobcoins

---

### EPIC-052: V12_002.Safety.Watchdog.cs - Watchdog Safety
**Methods**: 3 methods
- CancelWatchdogWorkingOrders (CYC=12, LOC=19)
- OnWatchdogTimer (CYC=11, LOC=30)
- CancelDirectFallbackOrders (CYC=11, LOC=18)
**Rationale**: Watchdog timer and order cancellation
**Priority**: HIGH (safety feature)
**Estimated Cost**: 80 bobcoins

---

### EPIC-053: V12_002.SIMA.Execution.cs - SIMA Execution
**Methods**: 4 methods
- ExecuteMultiAccountMarket (CYC=13, LOC=78)
- ProcessSingleFleetRMAAccount (CYC=12, LOC=106) - LOC>80
- ExecuteMultiAccountBracket (CYC=9, LOC=107) - LOC>80
- ExecuteRMAEntryV2 (CYC=9, LOC=110) - LOC>80
**Rationale**: Multi-account execution, God-methods
**Priority**: HIGH
**Estimated Cost**: 80 bobcoins

---

### EPIC-054: V12_002.SIMA.Fleet.cs - SIMA Fleet Management
**Methods**: 5 methods (excluding ShouldSkipFleet_RunHealthCheck which is Tier 1)
**Rationale**: Fleet slot processing and FSM initialization
**Priority**: HIGH
**Estimated Cost**: 80 bobcoins

---

### EPIC-055: V12_002.Symmetry.BracketFSM.cs - Bracket FSM
**Methods**: 5 methods
- ProcessBracketEvent (CYC=14, LOC=44) - M5 candidate
- GetFsmExpectedPosition (CYC=14, LOC=25)
- HandleFsmFilled (CYC=13, LOC=18)
- ResolveFsm_ByScan (CYC=12, LOC=21)
- RemoveFsmOrderIdMappings (CYC=11, LOC=14)
**Rationale**: FSM state transitions
**Priority**: HIGH (M5 candidate)
**Estimated Cost**: 80 bobcoins

---

### EPIC-056: V12_002.Symmetry.cs - Symmetry Guard
**Methods**: 2 methods
- SymmetryGuardOnMasterFill (CYC=14, LOC=44)
- SymmetryFindDispatchForMasterFill (CYC=9, LOC=20)
**Rationale**: Master fill event handling
**Priority**: MEDIUM
**Estimated Cost**: 80 bobcoins

---

### EPIC-057: V12_002.Symmetry.Follower.cs - Symmetry Follower
**Methods**: 3 methods
- SymmetryGuardTryResolveFollower (CYC=12, LOC=83) - LOC>80
- SymmetryGuardSubmitFollowerBracket (CYC=12, LOC=101) - LOC>80
- SymmetryGuardOnFollowerFill (CYC=11, LOC=47)
**Rationale**: Follower bracket logic, God-methods
**Priority**: HIGH
**Estimated Cost**: 80 bobcoins

---

### EPIC-058: V12_002.Trailing.Breakeven.cs - Trailing Breakeven
**Methods**: 3 methods
- MoveStop_SinglePosition (CYC=13, LOC=63)
- MoveSpecificTarget (CYC=12, LOC=41)
- FindTargetOrderForPosition (CYC=9, LOC=23)
**Rationale**: Breakeven stop moves
**Priority**: MEDIUM
**Estimated Cost**: 80 bobcoins

---

### EPIC-059: V12_002.Trailing.cs - Trailing Stop Management
**Methods**: 3 methods
- ManageTrailingStops (CYC=13, LOC=33)
- FleetSync_SyncFollowersToLevel (CYC=9, LOC=34)
- ManageTrail_RunPerTradeBranches (CYC=9, LOC=8)
**Rationale**: Trailing stop logic
**Priority**: MEDIUM
**Estimated Cost**: 80 bobcoins

---

### EPIC-060: V12_002.Trailing.StopUpdate.cs - Stop Update Operations
**Methods**: 4 methods
- UpdateStopOrder (CYC=11, LOC=33)
- InitiateStopReplacement (CYC=10, LOC=46)
- CleanupStalePendingReplacements (CYC=9, LOC=26)
- UpdateExistingPendingReplacement (CYC=9, LOC=46)
**Rationale**: Stop order updates and replacement
**Priority**: MEDIUM
**Estimated Cost**: 80 bobcoins

---

### EPIC-061: V12_002.UI.IPC.Commands.Misc.cs - IPC Misc Commands
**Methods**: 2 methods
- SendResponseToRemote (CYC=10, LOC=26)
- FlattenSpecificTarget (CYC=9, LOC=28)
**Rationale**: Misc IPC command handling
**Priority**: LOW
**Estimated Cost**: 80 bobcoins

---

### EPIC-062: V12_002.UI.IPC.Commands.Mode.cs - IPC Mode Commands
**Methods**: 2 methods
- TryHandleMode_SetMode (CYC=13, LOC=59) - M5 candidate
- TryHandleRisk_Breakeven (CYC=12, LOC=16)
**Rationale**: Mode/risk command handling
**Priority**: HIGH (M5 candidate)
**Estimated Cost**: 80 bobcoins

---

### EPIC-063: V12_002.UI.IPC.Server.cs - IPC Server
**Methods**: 2 methods
- StopIpcServer (CYC=12, LOC=29)
- ProcessClientStream (CYC=9, LOC=26)
**Rationale**: IPC server lifecycle
**Priority**: MEDIUM
**Estimated Cost**: 80 bobcoins

---

### EPIC-064: V12_002.UI.Panel.Helpers.cs - Panel Helpers
**Methods**: 3 methods (excluding FindChartTraderViaChartTab which is Tier 1)
- DumpVisualTree (CYC=10, LOC=48)
- FindChartTabGrid (CYC=10, LOC=23)
- FindChartTraderBySiblingSearch (CYC=9, LOC=16)
**Rationale**: Panel helper utilities
**Priority**: LOW
**Estimated Cost**: 80 bobcoins

---

### EPIC-065: V12_002.UI.Snapshot.cs - UI Snapshot
**Methods**: 2 methods
- FindMasterPosition (CYC=9, LOC=15)
- PopulateTargetSnapshots (CYC=9, LOC=21)
**Rationale**: Snapshot population
**Priority**: LOW
**Estimated Cost**: 80 bobcoins

---

### EPIC-066 through EPIC-080: Remaining Pure Tier 2 Clusters
**Count**: 15 additional epics
**Methods**: Various Tier 2 clusters (CYC 9-14)
**Priority**: LOW to MEDIUM
**Estimated Cost**: 80 bobcoins each (1,200 total)

---

## Budget Summary

### Total Epic Count: 80

**Category 1 (Mixed)**: 15 epics × 80 = 1,200 bobcoins
**Category 2 (Pure Tier 1)**: 30 epics × 80 = 2,400 bobcoins
**Category 3 (Pure Tier 2)**: 35 epics × 80 = 2,800 bobcoins

**Total Cost**: 6,400 bobcoins (40 APIs)

### Current Budget

**Available**: 1,600 bobcoins (10 APIs)
**Incoming**: +480 bobcoins (3 APIs from user)
**Total**: 2,080 bobcoins (13 APIs)

### Additional APIs Needed

**Shortfall**: 6,400 - 2,080 = 4,320 bobcoins
**APIs to Purchase**: 4,320 ÷ 160 = **27 additional APIs**
**Cost**: ~$135 (27 × $5)

---

## Execution Strategy

### Wave Structure (15 epics per wave)

**Wave 1**: EPIC-001 through EPIC-015 (Mixed Tier files)
- **Priority**: HIGH (comprehensive file refactoring)
- **Time**: ~25 hours (15 epics × 100 min ÷ 60)
- **Cost**: 1,200 bobcoins

**Wave 2**: EPIC-016 through EPIC-030 (Pure Tier 1, Part 1)
- **Priority**: HIGH (highest complexity methods)
- **Time**: ~25 hours
- **Cost**: 1,200 bobcoins

**Wave 3**: EPIC-031 through EPIC-045 (Pure Tier 1, Part 2)
- **Priority**: HIGH (remaining Tier 1 methods)
- **Time**: ~25 hours
- **Cost**: 1,200 bobcoins

**Wave 4**: EPIC-046 through EPIC-060 (Pure Tier 2, Part 1)
- **Priority**: MEDIUM (Tier 2 clusters)
- **Time**: ~25 hours
- **Cost**: 1,200 bobcoins

**Wave 5**: EPIC-061 through EPIC-075 (Pure Tier 2, Part 2)
- **Priority**: MEDIUM (remaining Tier 2 clusters)
- **Time**: ~25 hours
- **Cost**: 1,200 bobcoins

**Wave 6**: EPIC-076 through EPIC-080 (Pure Tier 2, Part 3)
- **Priority**: LOW (final Tier 2 clusters)
- **Time**: ~8 hours (5 epics × 100 min ÷ 60)
- **Cost**: 400 bobcoins

**Total Time**: ~133 hours (~5.5 days with 3 VMs)
**Total Cost**: 6,400 bobcoins (40 APIs)

---

## VM Capacity Planning

### Current Setup
- **VM Type**: n2-standard-8 (8 vCPUs, 32 GB RAM)
- **Proven Capacity**: 10 epics per VM

### Recommended Setup (3 × n2-standard-4)
- **VM Type**: n2-standard-4 (4 vCPUs, 16 GB RAM)
- **Capacity**: 5 epics per VM
- **Total Capacity**: 15 epics per wave
- **GCP Quota**: 12 vCPUs (within free tier)

### Wave Execution
- **Wave 1-5**: 15 epics each (3 VMs × 5 epics)
- **Wave 6**: 5 epics (1 VM × 5 epics)

---

## Priority Matrix

### Critical (Must Do First)
1. EPIC-026: ShouldSkipFleet_RunHealthCheck (CYC=31 - highest in codebase)
2. EPIC-002: Flatten operations (safety feature)
3. EPIC-005: SIMA Flatten (safety feature)
4. EPIC-052: Watchdog Safety (safety feature)

### High Priority (M5 Candidates)
1. EPIC-001: ProcessOnOrderUpdate (M5 hot path)
2. EPIC-010: TryApplyConfigTarget_Value (M5 hot path)
3. EPIC-011: TryHandleFleet_LongShort (M5 hot path)
4. EPIC-012: ProcessIpcCommandCore (M5 hot path)
5. EPIC-047: ProcessOnStateChange (M5 hot path)
6. EPIC-055: ProcessBracketEvent (M5 hot path)
7. EPIC-062: TryHandleMode_SetMode (M5 hot path)

### High Priority (God-Methods LOC>80)
1. EPIC-003: SyncLimitTarget + RestoreCascadedTargets
2. EPIC-004: SIMA Dispatch methods
3. EPIC-013: Panel Construction methods
4. EPIC-017: ExecuteFFMAManualMarketEntry
5. EPIC-053: SIMA Execution methods
6. EPIC-057: Symmetry Follower methods

---

## Success Criteria

### Per Epic
- ✅ All methods reduced to CYC ≤8
- ✅ Build passes
- ✅ Tests pass
- ✅ Manifest status = "completed"
- ✅ No P0 blockers

### Per Wave
- ✅ All 15 epics complete
- ✅ Complexity targets met
- ✅ No regressions introduced
- ✅ Roadmap updated

### Overall
- ✅ 80 epics complete
- ✅ All 180 methods (CYC >8) reduced to ≤8
- ✅ Jane Street compliance achieved
- ✅ V12 DNA maintained

---

## Risk Mitigation

### Mixed Tier Files (Category 1)
**Risk**: Comprehensive refactoring may introduce more changes than expected
**Mitigation**: 
- Execute Wave 1 first to validate approach
- Use Phase 1.5 (Scope Boundary) to prevent scope creep
- Monitor bobcoin usage closely

### God-Methods (LOC>80)
**Risk**: High LOC methods may require more than 80 bobcoins
**Mitigation**:
- Budget 10% buffer (640 bobcoins)
- Prioritize these epics early to detect overruns
- Consider splitting if necessary

### M5 Hot Paths
**Risk**: Refactoring hot paths may impact performance
**Mitigation**:
- Use Phase 2 (Architecture Planning) to validate performance
- Run benchmarks before/after
- Jane Street KB consultation for HFT patterns

---

## Next Steps

### Immediate (This Session)
1. ✅ Create final epic roadmap (this document)
2. ⏳ Document API purchase requirements
3. ⏳ Create Wave 1 execution plan

### Next Session
1. Purchase 27 additional APIs (4,320 bobcoins)
2. Generate Wave 1 scripts (EPIC-001 through EPIC-015)
3. Deploy to 3 × n2-standard-4 VMs
4. Launch Wave 1 execution
5. Monitor progress and bobcoin usage

---

**Document Version**: 1.0
**Last Updated**: 2026-06-14T03:43:00Z
**Status**: Ready for execution
**Approval**: Option B (Combined Clustering) confirmed by user