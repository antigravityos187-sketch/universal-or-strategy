# Phase 1: Scope Definition - EPIC-CCN-155

## Epic Metadata
- **Epic ID**: EPIC-CCN-155
- **Target Method**: `TryHandleFleetCommand`
- **File**: `src/V12_002.UI.IPC.Commands.Fleet.cs`
- **Current CYC**: 19
- **Target CYC**: ≤ 8
- **Reduction**: 58%
- **Status**: Phase 1 Complete

---

## Method Analysis

### Current Structure
The method is a **dispatcher** that routes IPC fleet commands to 18 specialized handler methods:

```csharp
private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)
{
    // 1. Build command ID
    // 2. Sequential if-return chain (18 handlers)
    // 3. Return false if no handler matched
}
```

### Complexity Breakdown
- **Lines**: 75 (including handler implementations)
- **Dispatcher Lines**: 19 (main method body)
- **Branching Points**: 18 (one per handler check)
- **Current CYC**: 19 (1 base + 18 if statements)

### Handler Categories
Handlers naturally group into 5 categories:

1. **Position Management** (4 handlers):
   - `TryHandleFleet_Trim` (TRIM_25, TRIM_50)
   - `TryHandleFleet_Lock50` (LOCK_50)
   - `TryHandleFleet_FlattenOnly` (FLATTEN_ONLY)
   - `TryHandleFleet_Flatten` (FLATTEN)

2. **Order Management** (2 handlers):
   - `TryHandleFleet_CancelAll` (CANCEL_ALL)
   - `TryHandleFleet_ResetMemory` (RESET_MEMORY)

3. **Entry Commands** (6 handlers):
   - `TryHandleFleet_LongShort` (LONG, SHORT)
   - `TryHandleFleet_OrLong` (OR_LONG)
   - `TryHandleFleet_OrShort` (OR_SHORT)
   - `TryHandleFleet_TrendManualLimit` (TREND_MANUAL_LIMIT)
   - `TryHandleFleet_RetestManualLimit` (RETEST_MANUAL_LIMIT)
   - `TryHandleFleet_FfmaManualLimit` (FFMA_MANUAL_LIMIT)
   - `TryHandleFleet_FfmaManualMarket` (FFMA_MANUAL_MARKET)

4. **Target Management** (2 handlers):
   - `TryHandleFleet_CloseTarget` (CLOSE_T*)
   - `TryHandleFleet_MoveTarget` (MOVE_TARGET*, SET_TARGET_PRICE)

5. **Configuration** (3 handlers):
   - `TryHandleFleet_FleetState` (GET_FLEET*, SET_SIMA, SET_LEADER_ACCOUNT, REQUEST_FLEET_STATE)
   - `TryHandleFleet_ToggleAccount` (TOGGLE_ACCOUNT*)
   - `TryHandleFleet_SetShadow` (SET_SHADOW)

---

## Extraction Strategy

### Approach: Category-Based Sub-Dispatchers

Create 5 sub-dispatcher methods, one per category:

1. `TryHandleFleet_PositionCommands(action, parts, cmdId)` → CYC 5
2. `TryHandleFleet_OrderCommands(action, cmdId)` → CYC 3
3. `TryHandleFleet_EntryCommands(action, parts, cmdId)` → CYC 7
4. `TryHandleFleet_TargetCommands(action, parts)` → CYC 3
5. `TryHandleFleet_ConfigCommands(action, parts)` → CYC 4

Main dispatcher becomes:
```csharp
private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)
{
    string cmdId = BuildCommandId(action, senderTicks);
    
    if (TryHandleFleet_PositionCommands(action, parts, cmdId)) return true;
    if (TryHandleFleet_OrderCommands(action, cmdId)) return true;
    if (TryHandleFleet_EntryCommands(action, parts, cmdId)) return true;
    if (TryHandleFleet_TargetCommands(action, parts)) return true;
    if (TryHandleFleet_ConfigCommands(action, parts)) return true;
    
    return false;
}
```

**Result**: CYC 6 (1 base + 5 if statements) ✅ **Target Met**

---

## Scope Boundary (V12.23 Protocol)

### IN SCOPE
✅ Extract `BuildCommandId` helper (1 line → method)
✅ Create 5 category sub-dispatchers
✅ Refactor main dispatcher to call sub-dispatchers
✅ Preserve all existing handler logic (zero logic drift)
✅ Maintain exact same command routing behavior

### OUT OF SCOPE
❌ Modifying handler implementations (already extracted)
❌ Changing command names or IPC protocol
❌ Adding new commands or features
❌ Refactoring handler internals (separate epics if needed)
❌ Touching other IPC command files

### Blast Radius
**Files Modified**: 1
- `src/V12_002.UI.IPC.Commands.Fleet.cs`

**Methods Modified**: 1
- `TryHandleFleetCommand` (dispatcher refactor)

**Methods Added**: 6
- `BuildCommandId` (helper)
- `TryHandleFleet_PositionCommands` (sub-dispatcher)
- `TryHandleFleet_OrderCommands` (sub-dispatcher)
- `TryHandleFleet_EntryCommands` (sub-dispatcher)
- `TryHandleFleet_TargetCommands` (sub-dispatcher)
- `TryHandleFleet_ConfigCommands` (sub-dispatcher)

**Callers**: 1
- `ProcessIpcCommands` (in `V12_002.UI.IPC.Commands.cs`)

**Risk Level**: LOW
- Pure structural refactor (no logic changes)
- All handlers already tested in production
- Single file modification
- No cross-file dependencies

---

## Success Criteria

### Functional
✅ All 18 command types route to correct handlers
✅ Command ID generation unchanged
✅ Duplicate detection preserved (via `cmdId`)
✅ Return values match original behavior
✅ No logic drift in any handler

### Complexity
✅ Main dispatcher: CYC ≤ 8 (target: 6)
✅ Sub-dispatchers: CYC ≤ 8 each
✅ Total complexity reduced by 58%

### Quality
✅ ASCII-only compliance maintained
✅ No new locks introduced
✅ Build passes (`dotnet build`)
✅ `deploy-sync.ps1` succeeds
✅ F5 in NinjaTrader loads strategy

---

## Ticket Breakdown (Phase 4)

**Estimated Tickets**: 2

1. **TICKET-1**: Extract `BuildCommandId` + Create 5 sub-dispatchers
   - Lines: ~50
   - CYC Impact: Main 19→6, Sub-dispatchers 3-7 each
   - Risk: LOW

2. **TICKET-2**: Refactor main dispatcher to call sub-dispatchers
   - Lines: ~10
   - CYC Impact: Final verification
   - Risk: LOW

---

## V12 DNA Compliance

### Lock-Free ✅
- No locks in dispatcher or handlers
- All state mutations via `Enqueue` (Actor pattern)

### ASCII-Only ✅
- No Unicode in string literals
- All log messages use plain ASCII

### Correctness by Construction ✅
- Chain-of-responsibility pattern prevents invalid states
- Each handler returns bool (matched/not matched)
- Impossible to route to wrong handler

### Jane Street Alignment ✅
- Cognitive simplicity: 5 categories vs 18 flat checks
- Microsecond-latency reasoning: grouped by intent
- Exhaustive testing: each category testable in isolation

---

## Phase 1 Status
✅ **COMPLETE** - Ready for Phase 2 (Architecture Planning)

**Next Steps**:
1. Phase 2: Generate detailed architecture plan with method signatures
2. Phase 3: DNA & PR audit
3. Phase 4: Generate surgical tickets
4. Phase 5: Execute tickets via Bob CLI