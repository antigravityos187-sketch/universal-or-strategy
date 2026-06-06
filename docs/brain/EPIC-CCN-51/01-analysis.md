# Epic: EPIC-CCN-51 -- Refactoring Analysis

## Dependency Map

### Direct Caller (Single Call Site)
| Caller Method | File | Line | How It Uses Target |
|---------------|------|------|-------------------|
| [`EnumerateApexAccounts()`](src/V12_002.SIMA.Lifecycle.cs:140) | V12_002.SIMA.Lifecycle.cs | 196 | Calls after fleet enumeration and sticky toggle application. Part of SIMA initialization sequence. |

**Call Context** (lines 192-196):
```csharp
// V12.Phase6 [HYDRATE]: Seed expectedPositions from live broker state
HydrateExpectedPositionsFromBroker();

// [BUILD 948] Adopt any working broker orders into tracking dicts
HydrateWorkingOrdersFromBroker();  // ← TARGET METHOD
```

### Indirect Dependents (Callers of EnumerateApexAccounts)
`EnumerateApexAccounts()` is called during:
- **Strategy Startup**: `OnStateChange(State.DataLoaded)` lifecycle hook
- **Broker Reconnect**: After connection restoration

**Impact**: Changes to `HydrateWorkingOrdersFromBroker()` affect SIMA initialization on every strategy start and reconnect.

---

## Method Internal Structure (416 Lines)

### Responsibility Breakdown

The 416-line God-method performs **5 distinct phases**:

#### **Phase 1: Fleet Order Adoption** (lines 313-408, ~95 lines)
**Purpose**: Iterate all fleet accounts, classify working orders by prefix, route to tracking dictionaries.

**Key Operations**:
- Outer loop: `foreach (Account acct in Account.All)` with `IsFleetAccount(acct)` guard
- Inner loop: `foreach (Order ord in acct.Orders.ToArray())`
- **8-way prefix classification**:
  - `Stop_` / `S_` → `stopOrders`
  - `T1_` → `target1Orders`
  - `T2_` → `target2Orders`
  - `T3_` → `target3Orders`
  - `T4_` → `target4Orders`
  - `T5_` → `target5Orders`
  - `Fleet_` → `entryOrders`
- **State guards**: Only adopt orders in Working, Accepted, Submitted, ChangePending, ChangeSubmitted states
- **Error isolation**: Try/catch per account (prevents one bad account from blocking others)

**Complexity Drivers**:
- Nested loops (Account → Order)
- 8-way if/else chain (prefix classification)
- Multi-condition state guards (5 OrderState checks)

#### **Phase 2: Fleet Position Reconstruction** (lines 410-450, ~40 lines)
**Purpose**: Rebuild `activePositions` structs for fleet entries to prevent REAPER audit divergence.

**Key Operations**:
- Inline logic for `MarketPosition` determination (Buy/BuyToCover → Long, else Short)
- Entry price calculation (LimitPrice fallback to StopPrice)
- Stop distance calculation (absolute value of entry - stop)
- Struct creation: `PositionInfo` with 10+ fields
- Dictionary insertion: `activePositions[key] = new PositionInfo { ... }`

**Complexity Drivers**:
- Inline business logic (should be extracted)
- Conditional price selection (LimitPrice vs StopPrice)
- Multi-field struct initialization

#### **Phase 3: Master Order Adoption** (lines 452-480, ~28 lines)
**Purpose**: Adopt working orders from master account (mirrors fleet logic but no FSM).

**Key Operations**:
- Single account loop: `foreach (Order ord in masterBroker996h.Orders.ToArray())`
- **Duplicate prefix classification** (same 8-way if/else as Phase 1)
- State guard: `IsOrderStateAdoptable(ord.OrderState, includeMasterUnknown: true)`
- Dictionary routing: Same as Phase 1

**Complexity Drivers**:
- **Code duplication**: 90% identical to Phase 1 (copy-paste with minor variations)
- Same 8-way prefix classification (should be shared helper)

#### **Phase 4: Master Position Reconstruction** (lines 482-520, ~38 lines)
**Purpose**: Reconstruct master position from bracket orders.

**Key Operations**:
- Delegates to existing method: `ReconstructMasterPositionFromBrackets()`
- Additional inline logic for position validation

**Complexity Drivers**:
- Inline validation logic (should be in the called method)

#### **Phase 5: FSM Hydration Orchestration** (line 523)
**Purpose**: Rebuild FSMs from adopted orders before enabling REAPER.

**Key Operations**:
- Single call: `HydrateFSMsFromWorkingOrders()` (CYC 72 - separate epic EPIC-CCN-52)
- Sets `_orderAdoptionComplete = true` flag

**Complexity Drivers**:
- None (simple delegation)

---

## Risk Hotspots

### 🔴 CRITICAL: Nested Loop with Inline Business Logic (Phase 1 + 2)
**Lines**: 313-450  
**CYC Contribution**: ~40-50 (estimated)  
**Risk**: Highest complexity concentration. Nested loops + 8-way classification + inline position reconstruction.

**Specific Risks**:
- **State Guard Complexity**: 5-way OrderState check is fragile (easy to miss a state during maintenance)
- **Prefix Classification**: Hardcoded strings ("Stop_", "T1_", etc.) scattered throughout - no single source of truth
- **Position Reconstruction**: Inline logic makes it hard to test independently
- **Error Handling**: Try/catch per account is correct, but error messages are generic

### 🟡 HIGH: Code Duplication (Phase 1 vs Phase 3)
**Lines**: 313-408 (fleet) vs 452-480 (master)  
**CYC Contribution**: ~15-20 (duplicated logic)  
**Risk**: Changes to fleet adoption logic must be manually mirrored to master adoption.

**Specific Risks**:
- **Divergence**: Fleet and master logic can drift apart during maintenance
- **Bug Propagation**: Bugs fixed in one path may not be fixed in the other
- **Maintenance Burden**: Every prefix addition requires changes in 2 places

### 🟡 HIGH: FSM Hydration Dependency (Phase 5)
**Lines**: 523  
**CYC Contribution**: 0 (delegation only)  
**Risk**: `HydrateFSMsFromWorkingOrders()` (CYC 72) is called inline - failures cascade.

**Specific Risks**:
- **Tight Coupling**: FSM hydration failure blocks `_orderAdoptionComplete` flag
- **No Rollback**: If FSM hydration fails, adopted orders remain in dictionaries (inconsistent state)
- **Separate Epic**: This method needs its own refactoring (EPIC-CCN-52)

### 🟢 MEDIUM: Magic Strings (Throughout)
**Lines**: 336-380 (prefix checks)  
**CYC Contribution**: ~5-10  
**Risk**: Hardcoded prefixes ("Stop_", "T1_", etc.) are brittle.

**Specific Risks**:
- **Typo Risk**: String comparison errors are runtime failures (no compile-time safety)
- **Maintenance**: Adding new target types requires code changes in multiple places
- **No Enum**: Should use enum-based classification for type safety

---

## Change Surface Area

### Files Affected by This Refactoring
| File | Reason | Risk Level |
|------|--------|-----------|
| `V12_002.SIMA.Lifecycle.cs` | Target file - will be modified | CRITICAL |

### Dictionaries Modified
| Dictionary | Purpose | Risk |
|------------|---------|------|
| `entryOrders` | Fleet entry orders | HIGH - REAPER depends on this |
| `stopOrders` | Stop orders (fleet + master) | HIGH - Risk management |
| `target1Orders` through `target5Orders` | Target orders | MEDIUM - Profit taking |
| `activePositions` | Position tracking structs | CRITICAL - REAPER audit baseline |

### State Flags Modified
| Flag | Purpose | Risk |
|------|---------|------|
| `_orderAdoptionComplete` | Enables REAPER auditing | CRITICAL - Must be set correctly |

### External Dependencies (Unchanged)
- `IsFleetAccount(Account)` - Helper method (unchanged)
- `IsOrderStateAdoptable(OrderState, bool)` - Helper method (unchanged)
- `ReconstructMasterPositionFromBrackets()` - Existing method (unchanged)
- `HydrateFSMsFromWorkingOrders()` - Existing method (separate epic)

---

## Test Coverage

### Current State
**Unit Tests**: ❌ ZERO  
**Integration Tests**: ❌ ZERO  
**Verification Method**: F5 compile + live NinjaTrader session

### Test Gaps (Critical)
1. **No Fleet Adoption Tests**: Cannot verify order classification logic without live broker
2. **No Position Reconstruction Tests**: Cannot verify `activePositions` struct correctness
3. **No Error Handling Tests**: Cannot verify try/catch behavior for bad accounts
4. **No State Guard Tests**: Cannot verify OrderState filtering logic

### Verification Strategy for This Epic
Since no unit test harness exists, verification will rely on:
1. **F5 Compile Gate**: Must compile cleanly after each extraction
2. **Complexity Audit**: `complexity_audit.py` must show CYC reduction after each extraction
3. **deploy-sync.ps1**: Must pass ASCII gate and hard-link sync after each extraction
4. **Live Session Test**: Manual verification in NinjaTrader after epic completion
   - Start strategy with existing positions
   - Trigger reconnect
   - Verify REAPER does not fire false positives
   - Verify all orders are adopted correctly

---

## Blast Radius Summary

### Direct Impact
- **1 caller**: `EnumerateApexAccounts()` (line 196)
- **0 external files**: Self-contained in `V12_002.SIMA.Lifecycle.cs`
- **5 dictionaries**: Modified during execution
- **1 critical flag**: `_orderAdoptionComplete`

### Indirect Impact
- **REAPER Auditing**: Depends on `_orderAdoptionComplete` flag and `activePositions` accuracy
- **FSM Lifecycle**: Depends on adopted orders being in `entryOrders` dictionary
- **Risk Management**: Depends on `stopOrders` dictionary being populated correctly
- **Profit Taking**: Depends on `target1-5Orders` dictionaries

### Risk Assessment
**Overall Risk**: 🔴 CRITICAL

**Justification**:
- Core SIMA initialization path (runs on every startup + reconnect)
- Zero test coverage (F5 + live session only)
- High churn (CodeScene LARGE RED circle)
- Modifies 5 critical dictionaries
- Enables REAPER auditing (false positives = trading halts)

**Mitigation**:
- Incremental extraction (one sub-method at a time)
- Checkpoint after each extraction
- F5 verification after each extraction
- Rollback plan (git branch isolation)

---

## Next Steps (Phase 2: Approach)

Based on this analysis, the approach document will address:
1. **Extraction Order**: Which sub-method to extract first (lowest risk → highest risk)
2. **Shared Helper Creation**: Extract prefix classification logic before fleet/master adoption
3. **Position Reconstruction**: Extract inline logic to dedicated method
4. **Error Handling Preservation**: Keep try/catch blocks in extracted methods
5. **FSM Hydration Boundary**: Leave `HydrateFSMsFromWorkingOrders()` call in place (separate epic)

---

**[ANALYSIS-COMPLETE]** Ready for Phase 2: Approach Design.