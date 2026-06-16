# EPIC-CCN-155 Execution Guide

## Quick Reference

**Epic**: EPIC-CCN-155  
**Method**: `TryHandleFleetCommand`  
**File**: `src/V12_002.UI.IPC.Commands.Fleet.cs`  
**Current CYC**: 19  
**Target CYC**: ≤ 8 (Dispatcher: 3, Helper: 6, Total: 9)  
**Tickets**: 1 (TICKET-1)  
**Risk**: MINIMAL  

---

## Execution Order

**Sequential** (only 1 ticket):
1. TICKET-1: Replace If-Chain Dispatcher with Command Registry

---

## TICKET-1: Quick Implementation Guide

### Pre-Flight Checks
```bash
# 1. Verify baseline complexity
python scripts/complexity_audit.py

# 2. Verify build passes
dotnet build

# 3. Read current implementation
# File: src/V12_002.UI.IPC.Commands.Fleet.cs
# Method: TryHandleFleetCommand (line ~XXX)
```

### Implementation Steps (6 Steps)

#### Step 1: Add Delegate Definition
**Location**: Top of `#region IPC Commands Fleet`

```csharp
// Delegate signature for fleet command handlers
private delegate bool FleetCommandHandler(string action, string[] parts, string cmdId);
```

#### Step 2: Add Registry Field
**Location**: After delegate definition

```csharp
// Command registry (initialized once in OnStateChange)
private Dictionary<string, FleetCommandHandler> _fleetCommandHandlers;
```

#### Step 3: Add Initialization Method
**Location**: After `TryHandleFleetCommand` method

```csharp
private void InitializeFleetCommandHandlers()
{
    _fleetCommandHandlers = new Dictionary<string, FleetCommandHandler>(StringComparer.OrdinalIgnoreCase)
    {
        ["LOCK_50"] = (action, parts, cmdId) => TryHandleFleet_Lock50(action),
        ["FLATTEN_ONLY"] = (action, parts, cmdId) => TryHandleFleet_FlattenOnly(action),
        ["FLATTEN"] = (action, parts, cmdId) => TryHandleFleet_Flatten(action, cmdId),
        ["CANCEL_ALL"] = (action, parts, cmdId) => TryHandleFleet_CancelAll(action, cmdId),
        ["RESET_MEMORY"] = (action, parts, cmdId) => TryHandleFleet_ResetMemory(action),
        ["LONG"] = (action, parts, cmdId) => TryHandleFleet_LongShort(action, cmdId),
        ["SHORT"] = (action, parts, cmdId) => TryHandleFleet_LongShort(action, cmdId),
        ["OR_LONG"] = (action, parts, cmdId) => TryHandleFleet_OrLong(action, cmdId),
        ["OR_SHORT"] = (action, parts, cmdId) => TryHandleFleet_OrShort(action, cmdId),
        ["TREND_MANUAL_LIMIT"] = (action, parts, cmdId) => TryHandleFleet_TrendManualLimit(action, parts, cmdId),
        ["RETEST_MANUAL_LIMIT"] = (action, parts, cmdId) => TryHandleFleet_RetestManualLimit(action, parts, cmdId),
        ["FFMA_MANUAL_LIMIT"] = (action, parts, cmdId) => TryHandleFleet_FfmaManualLimit(action, parts, cmdId),
        ["FFMA_MANUAL_MARKET"] = (action, parts, cmdId) => TryHandleFleet_FfmaManualMarket(action, cmdId),
        ["SET_SHADOW"] = (action, parts, cmdId) => TryHandleFleet_SetShadow(action, parts),
        ["SET_TARGET_PRICE"] = (action, parts, cmdId) => TryHandleFleet_MoveTarget(action, parts),
    };
}
```

#### Step 4: Add Pattern-Based Helper
**Location**: After initialization method

```csharp
private bool TryHandlePatternBasedCommands(string action, string[] parts, string cmdId)
{
    if (TryHandleFleet_Trim(action, parts))
        return true;
    
    if (TryHandleFleet_CloseTarget(action))
        return true;
    
    if (TryHandleFleet_MoveTarget(action, parts))
        return true;
    
    if (TryHandleFleet_FleetState(action, parts))
        return true;
    
    if (TryHandleFleet_ToggleAccount(action, parts))
        return true;
    
    return false;
}
```

#### Step 5: Refactor Dispatcher
**Location**: Replace existing `TryHandleFleetCommand` body

```csharp
private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)
{
    string cmdId = senderTicks > 0
        ? action + "|" + senderTicks.ToString()
        : action + "|" + (DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute).ToString();
    
    if (_fleetCommandHandlers.TryGetValue(action, out var handler))
    {
        return handler(action, parts, cmdId);
    }
    
    return TryHandlePatternBasedCommands(action, parts, cmdId);
}
```

#### Step 6: Call Initialization
**Location**: `OnStateChange` method, State.SetDefaults case

```csharp
case State.SetDefaults:
    // ... existing code ...
    InitializeFleetCommandHandlers(); // NEW: Initialize command registry
    // ... existing code ...
    break;
```

### Post-Implementation Validation

```bash
# 1. Verify complexity reduction
python scripts/complexity_audit.py
# Expected: TryHandleFleetCommand CYC ≤ 5, TryHandlePatternBasedCommands CYC ≤ 8

# 2. Verify build passes
dotnet build
# Expected: Zero errors

# 3. Run unit tests (if added)
dotnet test
# Expected: 100% pass

# 4. Sync to NinjaTrader
powershell -File .\deploy-sync.ps1
# Expected: ASCII gate passes

# 5. Test in NinjaTrader
# F5 in NinjaTrader IDE
# Expected: BUILD_TAG appears, strategy loads successfully
```

### Success Criteria Checklist

- [ ] Dispatcher CYC ≤ 5 (Target: 3)
- [ ] Helper CYC ≤ 8 (Target: 6)
- [ ] Total CYC ≤ 15 (Target: 9)
- [ ] All 18 handlers unchanged
- [ ] Zero logic drift
- [ ] Build passes (zero errors)
- [ ] `deploy-sync.ps1` passes
- [ ] F5 in NinjaTrader successful
- [ ] BUILD_TAG bumped in `src/V12_002.cs`

### Rollback Command

If issues arise:
```bash
git checkout HEAD~1 src/V12_002.UI.IPC.Commands.Fleet.cs
powershell -File .\deploy-sync.ps1
```

---

## Complete Documentation

For full details, see:
- **Ticket Spec**: `docs/brain/EPIC-CCN-155/04-tickets.md`
- **Architecture Plan**: `docs/brain/EPIC-CCN-155/02-architecture-plan.md`
- **Scope Boundary**: `docs/brain/EPIC-CCN-155/01-scope-boundary.md`

---

**Execution Ready**: ✅ All phases complete, ready for Phase 5 execution
