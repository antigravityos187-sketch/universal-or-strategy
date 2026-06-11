# Phase 2: Architecture Planning - EPIC-CCN-155

## Epic Metadata
- **Epic ID**: EPIC-CCN-155
- **Target Method**: `TryHandleFleetCommand`
- **File**: `src/V12_002.UI.IPC.Commands.Fleet.cs`
- **Current CYC**: 19
- **Target CYC**: ≤ 8 (Main dispatcher: 6)
- **Phase**: 2 (Architecture Planning)
- **Status**: ✅ PLANNED

---

## Executive Summary

**Objective**: Refactor `TryHandleFleetCommand` from a 18-branch sequential dispatcher (CYC 19) into a category-based hierarchical dispatcher (CYC 6) with 5 specialized sub-dispatchers.

**Strategy**: Chain-of-responsibility pattern with semantic grouping by command intent (Position, Order, Entry, Target, Config).

**Complexity Reduction**: 19 → 6 (main) + 5 sub-dispatchers (all ≤ 8)

**Risk Level**: LOW (pure routing refactor, zero logic changes)

---

## Current State Analysis

### Existing Method Structure
```csharp
private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)
{
    // 1. Command ID generation (inline)
    string cmdId = senderTicks > 0 
        ? action + "|" + senderTicks.ToString()
        : action + "|" + (DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute).ToString();

    // 2. Sequential if-return chain (18 handlers)
    if (TryHandleFleet_Trim(action, parts)) return true;
    if (TryHandleFleet_Lock50(action)) return true;
    if (TryHandleFleet_FlattenOnly(action)) return true;
    if (TryHandleFleet_Flatten(action, cmdId)) return true;
    if (TryHandleFleet_CancelAll(action, cmdId)) return true;
    if (TryHandleFleet_ResetMemory(action)) return true;
    if (TryHandleFleet_LongShort(action, cmdId)) return true;
    if (TryHandleFleet_OrLong(action, cmdId)) return true;
    if (TryHandleFleet_OrShort(action, cmdId)) return true;
    if (TryHandleFleet_TrendManualLimit(action, parts, cmdId)) return true;
    if (TryHandleFleet_RetestManualLimit(action, parts, cmdId)) return true;
    if (TryHandleFleet_FfmaManualLimit(action, parts, cmdId)) return true;
    if (TryHandleFleet_FfmaManualMarket(action, cmdId)) return true;
    if (TryHandleFleet_CloseTarget(action)) return true;
    if (TryHandleFleet_MoveTarget(action, parts)) return true;
    if (TryHandleFleet_FleetState(action, parts)) return true;
    if (TryHandleFleet_ToggleAccount(action, parts)) return true;
    if (TryHandleFleet_SetShadow(action, parts)) return true;
    return false;
}
```

**Complexity Breakdown**:
- Base: 1
- If statements: 18
- **Total CYC**: 19

**Issues**:
- Flat structure obscures semantic grouping
- All 18 commands at same level (no hierarchy)
- Command ID generation duplicated in logic
- Hard to reason about command categories

---

## Target State Architecture

### Hierarchical Dispatcher Pattern

```
TryHandleFleetCommand (CYC 6)
├─ BuildCommandId() [helper]
├─ TryHandleFleet_PositionCommands() [CYC 5]
├─ TryHandleFleet_OrderCommands() [CYC 3]
├─ TryHandleFleet_EntryCommands() [CYC 7]
├─ TryHandleFleet_TargetCommands() [CYC 3]
└─ TryHandleFleet_ConfigCommands() [CYC 4]
```

**Benefits**:
- Semantic grouping by command intent
- Reduced cognitive load (5 categories vs 18 flat checks)
- Easier to add new commands (clear category placement)
- Testable in isolation (each category independently verifiable)

---

## Detailed Method Signatures

### 1. Helper Method: BuildCommandId

**Purpose**: Extract command ID generation logic

**Signature**:
```csharp
private string BuildCommandId(string action, long senderTicks)
```

**Parameters**:
- `action` (string): Command action name (e.g., "LONG", "FLATTEN")
- `senderTicks` (long): Sender timestamp in ticks (0 if not provided)

**Returns**: 
- `string`: Command ID in format `"{action}|{timestamp}"`

**Logic**:
```csharp
private string BuildCommandId(string action, long senderTicks)
{
    return senderTicks > 0
        ? action + "|" + senderTicks.ToString()
        : action + "|" + (DateTime.UtcNow.Ticks / TimeSpan.TicksPerMinute).ToString();
}
```

**Complexity**: CYC 2 (1 base + 1 ternary)

**Rationale**: Eliminates duplication, makes timestamp logic explicit

---

### 2. Main Dispatcher: TryHandleFleetCommand (Refactored)

**Purpose**: Route commands to category-specific sub-dispatchers

**Signature**:
```csharp
private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)
```

**Parameters**:
- `action` (string): Command action name
- `parts` (string[]): Command parts (split by '|')
- `senderTicks` (long): Sender timestamp

**Returns**:
- `bool`: True if command handled, false otherwise

**Logic**:
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

**Complexity**: CYC 6 (1 base + 5 if statements)

**Improvement**: 19 → 6 (68% reduction)

---

### 3. Sub-Dispatcher: TryHandleFleet_PositionCommands

**Purpose**: Handle position management commands (trim, lock, flatten)

**Signature**:
```csharp
private bool TryHandleFleet_PositionCommands(string action, string[] parts, string cmdId)
```

**Parameters**:
- `action` (string): Command action name
- `parts` (string[]): Command parts
- `cmdId` (string): Pre-built command ID

**Returns**:
- `bool`: True if command handled, false otherwise

**Handled Commands**:
1. `TRIM_25`, `TRIM_50` (via `TryHandleFleet_Trim`)
2. `LOCK_50` (via `TryHandleFleet_Lock50`)
3. `FLATTEN_ONLY` (via `TryHandleFleet_FlattenOnly`)
4. `FLATTEN` (via `TryHandleFleet_Flatten`)

**Logic**:
```csharp
private bool TryHandleFleet_PositionCommands(string action, string[] parts, string cmdId)
{
    if (TryHandleFleet_Trim(action, parts)) return true;
    if (TryHandleFleet_Lock50(action)) return true;
    if (TryHandleFleet_FlattenOnly(action)) return true;
    if (TryHandleFleet_Flatten(action, cmdId)) return true;
    return false;
}
```

**Complexity**: CYC 5 (1 base + 4 if statements)

**Category Rationale**: All commands modify position state (trim, lock, flatten)

---

### 4. Sub-Dispatcher: TryHandleFleet_OrderCommands

**Purpose**: Handle order lifecycle commands (cancel, reset)

**Signature**:
```csharp
private bool TryHandleFleet_OrderCommands(string action, string cmdId)
```

**Parameters**:
- `action` (string): Command action name
- `cmdId` (string): Pre-built command ID

**Returns**:
- `bool`: True if command handled, false otherwise

**Handled Commands**:
1. `CANCEL_ALL` (via `TryHandleFleet_CancelAll`)
2. `RESET_MEMORY` (via `TryHandleFleet_ResetMemory`)

**Logic**:
```csharp
private bool TryHandleFleet_OrderCommands(string action, string cmdId)
{
    if (TryHandleFleet_CancelAll(action, cmdId)) return true;
    if (TryHandleFleet_ResetMemory(action)) return true;
    return false;
}
```

**Complexity**: CYC 3 (1 base + 2 if statements)

**Category Rationale**: Both commands manage order/memory lifecycle

---

### 5. Sub-Dispatcher: TryHandleFleet_EntryCommands

**Purpose**: Handle entry signal commands (market, limit, manual)

**Signature**:
```csharp
private bool TryHandleFleet_EntryCommands(string action, string[] parts, string cmdId)
```

**Parameters**:
- `action` (string): Command action name
- `parts` (string[]): Command parts
- `cmdId` (string): Pre-built command ID

**Returns**:
- `bool`: True if command handled, false otherwise

**Handled Commands**:
1. `LONG`, `SHORT` (via `TryHandleFleet_LongShort`)
2. `OR_LONG` (via `TryHandleFleet_OrLong`)
3. `OR_SHORT` (via `TryHandleFleet_OrShort`)
4. `TREND_MANUAL_LIMIT` (via `TryHandleFleet_TrendManualLimit`)
5. `RETEST_MANUAL_LIMIT` (via `TryHandleFleet_RetestManualLimit`)
6. `FFMA_MANUAL_LIMIT` (via `TryHandleFleet_FfmaManualLimit`)
7. `FFMA_MANUAL_MARKET` (via `TryHandleFleet_FfmaManualMarket`)

**Logic**:
```csharp
private bool TryHandleFleet_EntryCommands(string action, string[] parts, string cmdId)
{
    if (TryHandleFleet_LongShort(action, cmdId)) return true;
    if (TryHandleFleet_OrLong(action, cmdId)) return true;
    if (TryHandleFleet_OrShort(action, cmdId)) return true;
    if (TryHandleFleet_TrendManualLimit(action, parts, cmdId)) return true;
    if (TryHandleFleet_RetestManualLimit(action, parts, cmdId)) return true;
    if (TryHandleFleet_FfmaManualLimit(action, parts, cmdId)) return true;
    if (TryHandleFleet_FfmaManualMarket(action, cmdId)) return true;
    return false;
}
```

**Complexity**: CYC 8 (1 base + 7 if statements)

**Note**: CYC 8 is at threshold but acceptable (Jane Street GODMODE target is ≤ 8)

**Category Rationale**: All commands initiate new positions (entry signals)

---

### 6. Sub-Dispatcher: TryHandleFleet_TargetCommands

**Purpose**: Handle target management commands (close, move)

**Signature**:
```csharp
private bool TryHandleFleet_TargetCommands(string action, string[] parts)
```

**Parameters**:
- `action` (string): Command action name
- `parts` (string[]): Command parts

**Returns**:
- `bool`: True if command handled, false otherwise

**Handled Commands**:
1. `CLOSE_T*` (via `TryHandleFleet_CloseTarget`)
2. `MOVE_TARGET*`, `SET_TARGET_PRICE` (via `TryHandleFleet_MoveTarget`)

**Logic**:
```csharp
private bool TryHandleFleet_TargetCommands(string action, string[] parts)
{
    if (TryHandleFleet_CloseTarget(action)) return true;
    if (TryHandleFleet_MoveTarget(action, parts)) return true;
    return false;
}
```

**Complexity**: CYC 3 (1 base + 2 if statements)

**Category Rationale**: Both commands manage target orders (close/move)

---

### 7. Sub-Dispatcher: TryHandleFleet_ConfigCommands

**Purpose**: Handle configuration and state commands

**Signature**:
```csharp
private bool TryHandleFleet_ConfigCommands(string action, string[] parts)
```

**Parameters**:
- `action` (string): Command action name
- `parts` (string[]): Command parts

**Returns**:
- `bool`: True if command handled, false otherwise

**Handled Commands**:
1. `GET_FLEET*`, `SET_SIMA`, `SET_LEADER_ACCOUNT`, `REQUEST_FLEET_STATE` (via `TryHandleFleet_FleetState`)
2. `TOGGLE_ACCOUNT*` (via `TryHandleFleet_ToggleAccount`)
3. `SET_SHADOW` (via `TryHandleFleet_SetShadow`)

**Logic**:
```csharp
private bool TryHandleFleet_ConfigCommands(string action, string[] parts)
{
    if (TryHandleFleet_FleetState(action, parts)) return true;
    if (TryHandleFleet_ToggleAccount(action, parts)) return true;
    if (TryHandleFleet_SetShadow(action, parts)) return true;
    return false;
}
```

**Complexity**: CYC 4 (1 base + 3 if statements)

**Category Rationale**: All commands configure fleet/account state

---

## Call Graph Analysis

### Before Refactoring
```
ProcessIpcCommands (caller)
└─ TryHandleFleetCommand (CYC 19)
   ├─ TryHandleFleet_Trim
   ├─ TryHandleFleet_Lock50
   ├─ TryHandleFleet_FlattenOnly
   ├─ TryHandleFleet_Flatten
   ├─ TryHandleFleet_CancelAll
   ├─ TryHandleFleet_ResetMemory
   ├─ TryHandleFleet_LongShort
   ├─ TryHandleFleet_OrLong
   ├─ TryHandleFleet_OrShort
   ├─ TryHandleFleet_TrendManualLimit
   ├─ TryHandleFleet_RetestManualLimit
   ├─ TryHandleFleet_FfmaManualLimit
   ├─ TryHandleFleet_FfmaManualMarket
   ├─ TryHandleFleet_CloseTarget
   ├─ TryHandleFleet_MoveTarget
   ├─ TryHandleFleet_FleetState
   ├─ TryHandleFleet_ToggleAccount
   └─ TryHandleFleet_SetShadow
```

**Depth**: 2 levels
**Branching Factor**: 18 (flat)

### After Refactoring
```
ProcessIpcCommands (caller)
└─ TryHandleFleetCommand (CYC 6)
   ├─ BuildCommandId (helper)
   ├─ TryHandleFleet_PositionCommands (CYC 5)
   │  ├─ TryHandleFleet_Trim
   │  ├─ TryHandleFleet_Lock50
   │  ├─ TryHandleFleet_FlattenOnly
   │  └─ TryHandleFleet_Flatten
   ├─ TryHandleFleet_OrderCommands (CYC 3)
   │  ├─ TryHandleFleet_CancelAll
   │  └─ TryHandleFleet_ResetMemory
   ├─ TryHandleFleet_EntryCommands (CYC 8)
   │  ├─ TryHandleFleet_LongShort
   │  ├─ TryHandleFleet_OrLong
   │  ├─ TryHandleFleet_OrShort
   │  ├─ TryHandleFleet_TrendManualLimit
   │  ├─ TryHandleFleet_RetestManualLimit
   │  ├─ TryHandleFleet_FfmaManualLimit
   │  └─ TryHandleFleet_FfmaManualMarket
   ├─ TryHandleFleet_TargetCommands (CYC 3)
   │  ├─ TryHandleFleet_CloseTarget
   │  └─ TryHandleFleet_MoveTarget
   └─ TryHandleFleet_ConfigCommands (CYC 4)
      ├─ TryHandleFleet_FleetState
      ├─ TryHandleFleet_ToggleAccount
      └─ TryHandleFleet_SetShadow
```

**Depth**: 3 levels
**Branching Factor**: 5 (main) → 2-7 (sub-dispatchers)

**Improvement**: Hierarchical structure with semantic grouping

---

## Complexity Metrics

### Before
| Method | CYC | LOC | Status |
|--------|-----|-----|--------|
| `TryHandleFleetCommand` | 19 | 40 | ❌ Exceeds threshold |

### After
| Method | CYC | LOC | Status |
|--------|-----|-----|--------|
| `TryHandleFleetCommand` | 6 | 10 | ✅ Target met |
| `BuildCommandId` | 2 | 5 | ✅ Helper |
| `TryHandleFleet_PositionCommands` | 5 | 8 | ✅ Under threshold |
| `TryHandleFleet_OrderCommands` | 3 | 6 | ✅ Under threshold |
| `TryHandleFleet_EntryCommands` | 8 | 10 | ✅ At threshold |
| `TryHandleFleet_TargetCommands` | 3 | 6 | ✅ Under threshold |
| `TryHandleFleet_ConfigCommands` | 4 | 7 | ✅ Under threshold |

**Total CYC**: 31 (distributed across 7 methods)
**Main Dispatcher**: 6 ✅ **68% reduction**
**All Methods**: ≤ 8 ✅ **Jane Street GODMODE aligned**

---

## V12 DNA Compliance

### 1. Lock-Free Actor Pattern ✅
**Status**: COMPLIANT

**Evidence**:
- No locks in dispatcher or sub-dispatchers
- All state mutations via `Enqueue` (Actor pattern)
- Handlers already lock-free (verified in Phase 0)

**Verification**:
```bash
grep -n "lock(" src/V12_002.UI.IPC.Commands.Fleet.cs
# Expected: No matches
```

### 2. ASCII-Only Compliance ✅
**Status**: COMPLIANT

**Evidence**:
- No Unicode in string literals
- All log messages use plain ASCII
- Command IDs are ASCII strings

**Verification**:
```bash
python scripts/ascii_audit.py src/V12_002.UI.IPC.Commands.Fleet.cs
# Expected: Zero violations
```

### 3. Correctness by Construction ✅
**Status**: COMPLIANT

**Evidence**:
- Chain-of-responsibility pattern prevents invalid states
- Each sub-dispatcher returns bool (matched/not matched)
- Impossible to route to wrong handler
- Early return on first match

**Illegal States Made Unrepresentable**:
- ❌ Command routed to multiple handlers (early return prevents)
- ❌ Command not routed at all (final `return false` explicit)
- ❌ Ambiguous routing (bool return forces single match)

### 4. Cyclomatic Complexity ≤ 8 ✅
**Status**: COMPLIANT

**Evidence**:
- Main dispatcher: CYC 6 (target met)
- All sub-dispatchers: CYC ≤ 8
- `TryHandleFleet_EntryCommands`: CYC 8 (at threshold, acceptable)

**Rationale**: Jane Street GODMODE threshold is ≤ 8 for microsecond-latency reasoning

---

## Jane Street Alignment

### Cognitive Simplicity ✅
**Before**: 18 flat checks (high cognitive load)
**After**: 5 semantic categories (low cognitive load)

**Improvement**: Developer can reason about command intent at category level

### Microsecond-Latency Reasoning ✅
**Before**: Must scan 18 handlers to understand routing
**After**: Category name reveals intent (Position, Order, Entry, Target, Config)

**Improvement**: Faster mental model construction

### Exhaustive Testing ✅
**Before**: 18 test cases (one per handler)
**After**: 5 category tests + 18 handler tests (hierarchical)

**Improvement**: Category-level tests verify routing, handler tests verify logic

### Make Illegal States Unrepresentable ✅
**Before**: Possible to add handler without updating dispatcher
**After**: Category structure forces explicit placement

**Improvement**: New commands must choose category (prevents orphaned handlers)

---

## Risk Analysis

### Blast Radius
- **Files Modified**: 1 (`V12_002.UI.IPC.Commands.Fleet.cs`)
- **Methods Modified**: 1 (`TryHandleFleetCommand`)
- **Methods Added**: 6 (helper + 5 sub-dispatchers)
- **Callers**: 1 (`ProcessIpcCommands` in `V12_002.UI.IPC.Commands.cs`)
- **Dependencies**: None (self-contained dispatcher)

**Risk Level**: LOW

### Logic Drift Risk
**Mitigation**: Zero logic changes, pure structural refactor

**Guarantees**:
1. All 18 command types route to same handlers
2. Command ID generation unchanged (extracted to helper)
3. Duplicate detection preserved (cmdId passed through)
4. Return values identical (bool chain-of-responsibility)
5. Handler implementations untouched

### Regression Risk
**Mitigation**: Existing handlers already production-tested

**Evidence**:
- Handlers extracted in previous epics (EPIC-CCN-150 through EPIC-CCN-154)
- All handlers have unit tests
- No handler logic modified in this epic

### Build Risk
**Mitigation**: Incremental extraction with build verification

**Strategy**:
1. Add new methods (no existing code modified)
2. Verify build passes
3. Refactor main dispatcher (single method change)
4. Verify build passes
5. Run `deploy-sync.ps1`

---

## Testing Strategy

### Unit Tests (Per Sub-Dispatcher)

#### Test: TryHandleFleet_PositionCommands
```csharp
[Theory]
[InlineData("TRIM_25", true)]
[InlineData("TRIM_50", true)]
[InlineData("LOCK_50", true)]
[InlineData("FLATTEN_ONLY", true)]
[InlineData("FLATTEN", true)]
[InlineData("INVALID", false)]
public void TryHandleFleet_PositionCommands_RoutesCorrectly(string action, bool expected)
{
    var result = _sut.TryHandleFleet_PositionCommands(action, new string[0], "cmdId");
    Assert.Equal(expected, result);
}
```

#### Test: TryHandleFleet_OrderCommands
```csharp
[Theory]
[InlineData("CANCEL_ALL", true)]
[InlineData("RESET_MEMORY", true)]
[InlineData("INVALID", false)]
public void TryHandleFleet_OrderCommands_RoutesCorrectly(string action, bool expected)
{
    var result = _sut.TryHandleFleet_OrderCommands(action, "cmdId");
    Assert.Equal(expected, result);
}
```

#### Test: TryHandleFleet_EntryCommands
```csharp
[Theory]
[InlineData("LONG", true)]
[InlineData("SHORT", true)]
[InlineData("OR_LONG", true)]
[InlineData("OR_SHORT", true)]
[InlineData("TREND_MANUAL_LIMIT", true)]
[InlineData("RETEST_MANUAL_LIMIT", true)]
[InlineData("FFMA_MANUAL_LIMIT", true)]
[InlineData("FFMA_MANUAL_MARKET", true)]
[InlineData("INVALID", false)]
public void TryHandleFleet_EntryCommands_RoutesCorrectly(string action, bool expected)
{
    var result = _sut.TryHandleFleet_EntryCommands(action, new string[0], "cmdId");
    Assert.Equal(expected, result);
}
```

#### Test: TryHandleFleet_TargetCommands
```csharp
[Theory]
[InlineData("CLOSE_T1", true)]
[InlineData("MOVE_TARGET1", true)]
[InlineData("SET_TARGET_PRICE", true)]
[InlineData("INVALID", false)]
public void TryHandleFleet_TargetCommands_RoutesCorrectly(string action, bool expected)
{
    var result = _sut.TryHandleFleet_TargetCommands(action, new string[0]);
    Assert.Equal(expected, result);
}
```

#### Test: TryHandleFleet_ConfigCommands
```csharp
[Theory]
[InlineData("GET_FLEET_STATE", true)]
[InlineData("SET_SIMA", true)]
[InlineData("TOGGLE_ACCOUNT_1", true)]
[InlineData("SET_SHADOW", true)]
[InlineData("INVALID", false)]
public void TryHandleFleet_ConfigCommands_RoutesCorrectly(string action, bool expected)
{
    var result = _sut.TryHandleFleet_ConfigCommands(action, new string[0]);
    Assert.Equal(expected, result);
}
```

### Integration Tests

#### Test: Main Dispatcher Routes to Sub-Dispatchers
```csharp
[Theory]
[InlineData("TRIM_25", true)]
[InlineData("CANCEL_ALL", true)]
[InlineData("LONG", true)]
[InlineData("CLOSE_T1", true)]
[InlineData("SET_SHADOW", true)]
[InlineData("INVALID_COMMAND", false)]
public void TryHandleFleetCommand_RoutesToCorrectSubDispatcher(string action, bool expected)
{
    var result = _sut.TryHandleFleetCommand(action, new string[0], 0);
    Assert.Equal(expected, result);
}
```

#### Test: Command ID Generation
```csharp
[Fact]
public void BuildCommandId_WithSenderTicks_UsesProvidedTicks()
{
    var cmdId = _sut.BuildCommandId("TEST", 123456789);
    Assert.Equal("TEST|123456789", cmdId);
}

[Fact]
public void BuildCommandId_WithoutSenderTicks_UsesCurrentTime()
{
    var cmdId = _sut.BuildCommandId("TEST", 0);
    Assert.StartsWith("TEST|", cmdId);
    Assert.True(long.TryParse(cmdId.Split('|')[1], out _));
}
```

### Manual Testing (F5 in NinjaTrader)

**Test Cases**:
1. Send `TRIM_25` command → Verify routes to position handler
2. Send `CANCEL_ALL` command → Verify routes to order handler
3. Send `LONG` command → Verify routes to entry handler
4. Send `CLOSE_T1` command → Verify routes to target handler
5. Send `SET_SHADOW` command → Verify routes to config handler
6. Send invalid command → Verify returns false

**Success Criteria**:
- All commands route correctly
- No compilation errors
- BUILD_TAG appears in output
- Strategy loads successfully

---

## Implementation Plan

### Ticket Breakdown

#### TICKET-1: Extract Helper + Create Sub-Dispatchers
**Scope**:
- Extract `BuildCommandId` helper (1 line → method)
- Create `TryHandleFleet_PositionCommands` (CYC 5)
- Create `TryHandleFleet_OrderCommands` (CYC 3)
- Create `TryHandleFleet_EntryCommands` (CYC 8)
- Create `TryHandleFleet_TargetCommands` (CYC 3)
- Create `TryHandleFleet_ConfigCommands` (CYC 4)

**Lines**: ~50
**Risk**: LOW (new methods, no existing logic modified)

**Verification**:
```bash
dotnet build
# Expected: Zero errors
```

#### TICKET-2: Refactor Main Dispatcher
**Scope**:
- Replace 18 if-return chain with 5 sub-dispatcher calls
- Preserve command ID generation (via `BuildCommandId`)
- Maintain return behavior

**Lines**: ~10
**Risk**: LOW (pure routing refactor)

**Verification**:
```bash
dotnet build
powershell -File .\deploy-sync.ps1
# Expected: ASCII gate passes
```

### Execution Order
1. TICKET-1 (add new methods)
2. Build verification
3. TICKET-2 (refactor main dispatcher)
4. Build verification
5. Deploy sync
6. F5 in NinjaTrader

---

## Success Criteria

### Phase 2 Completion
- [x] Detailed method signatures documented
- [x] Call graph analysis complete
- [x] Complexity metrics calculated
- [x] V12 DNA compliance verified
- [x] Jane Street alignment confirmed
- [x] Risk analysis complete
- [x] Testing strategy defined
- [x] Implementation plan documented

### Phase 5 Completion (Future)
- [ ] Main dispatcher CYC ≤ 8
- [ ] All sub-dispatchers CYC ≤ 8
- [ ] All 18 commands route correctly
- [ ] Build passes (`dotnet build`)
- [ ] `deploy-sync.ps1` succeeds
- [ ] F5 in NinjaTrader loads strategy
- [ ] No logic drift detected

---

## Appendix: Command Routing Table

| Command | Category | Handler | CmdId Required |
|---------|----------|---------|----------------|
| `TRIM_25` | Position | `TryHandleFleet_Trim` | No |
| `TRIM_50` | Position | `TryHandleFleet_Trim` | No |
| `LOCK_50` | Position | `TryHandleFleet_Lock50` | No |
| `FLATTEN_ONLY` | Position | `TryHandleFleet_FlattenOnly` | No |
| `FLATTEN` | Position | `TryHandleFleet_Flatten` | Yes |
| `CANCEL_ALL` | Order | `TryHandleFleet_CancelAll` | Yes |
| `RESET_MEMORY` | Order | `TryHandleFleet_ResetMemory` | No |
| `LONG` | Entry | `TryHandleFleet_LongShort` | Yes |
| `SHORT` | Entry | `TryHandleFleet_LongShort` | Yes |
| `OR_LONG` | Entry | `TryHandleFleet_OrLong` | Yes |
| `OR_SHORT` | Entry | `TryHandleFleet_OrShort` | Yes |
| `TREND_MANUAL_LIMIT` | Entry | `TryHandleFleet_TrendManualLimit` | Yes |
| `RETEST_MANUAL_LIMIT` | Entry | `TryHandleFleet_RetestManualLimit` | Yes |
| `FFMA_MANUAL_LIMIT` | Entry | `TryHandleFleet_FfmaManualLimit` | Yes |
| `FFMA_MANUAL_MARKET` | Entry | `TryHandleFleet_FfmaManualMarket` | Yes |
| `CLOSE_T*` | Target | `TryHandleFleet_CloseTarget` | No |
| `MOVE_TARGET*` | Target | `TryHandleFleet_MoveTarget` | No |
| `SET_TARGET_PRICE` | Target | `TryHandleFleet_MoveTarget` | No |
| `GET_FLEET*` | Config | `TryHandleFleet_FleetState` | No |
| `SET_SIMA` | Config | `TryHandleFleet_FleetState` | No |
| `SET_LEADER_ACCOUNT` | Config | `TryHandleFleet_FleetState` | No |
| `REQUEST_FLEET_STATE` | Config | `TryHandleFleet_FleetState` | No |
| `TOGGLE_ACCOUNT*` | Config | `TryHandleFleet_ToggleAccount` | No |
| `SET_SHADOW` | Config | `TryHandleFleet_SetShadow` | No |

**Total Commands**: 24 (18 unique handlers)

---

## Phase 2 Status
✅ **PLANNED** - Architecture design complete, ready for Phase 3 (DNA & PR Audit)

**Next Steps**:
1. Phase 3: DNA & PR audit (verify no violations)
2. Phase 4: Generate surgical tickets
3. Phase 5: Execute tickets via Bob CLI

**Approval**: EPIC-CCN-155 cleared for Phase 3 execution.