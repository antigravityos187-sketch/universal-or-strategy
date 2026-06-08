# Epic: EPIC-CCN-1 -- Refactoring Analysis

**Target**: [`HydrateFSMsFromWorkingOrders()`](../../../src/V12_002.SIMA.Lifecycle.cs:464-759)
**File**: `src/V12_002.SIMA.Lifecycle.cs`
**Current State**: 296 lines, CYC 71, ZERO extracted helpers

**⚠️ VALIDATION FINDING (2026-06-08)**: DUPLICATE METHOD DETECTED - A second version exists at line 902 (57 lines, already extracted). This epic must DELETE the old God-method and verify the new version is correct.

---

## Executive Summary

This is a **FULL GOD-METHOD EXTRACTION** targeting an 88.7% complexity reduction (CYC 71 → ≤8). The method is a private, self-contained FSM hydration routine with zero blast radius and no cross-file dependencies. The extraction is **LOW RISK** due to isolation but **HIGH VALUE** due to severe Jane Street violations (CYC 9x over threshold).

**Key Finding**: 80 lines (27% of method) are pure duplication - 5 identical target-linking blocks that can be collapsed into a single parameterized helper.

---

## Dependency Map

### Direct Callers
| Caller | File | Line | How It Uses Target |
|--------|------|------|-------------------|
| [`HydrateWorkingOrdersFromBroker()`](../../../src/V12_002.SIMA.Lifecycle.cs:445) | V12_002.SIMA.Lifecycle.cs | 445 | Single call site - Phase 5 of SIMA lifecycle during broker reconnection |

**Blast Radius**: ZERO (confirmed via jCodemunch `get_blast_radius`)
- Private method visibility
- Single caller in same file
- No external dependencies
- No cross-file imports

### Shared State Dependencies

**Read-Only Dictionaries** (populated by caller before invocation):
- `entryOrders` - ConcurrentDictionary<string, Order> - Entry orders from broker
- `activePositions` - ConcurrentDictionary<string, PositionInfo> - Live position data
- `stopOrders` - ConcurrentDictionary<string, Order> - Stop loss orders
- `target1Orders` through `target5Orders` - 5x ConcurrentDictionary<string, Order> - Profit targets
- `Account.All` - NinjaTrader global collection - All trading accounts

**Mutated State** (written by this method):
- `_followerBrackets` - ConcurrentDictionary<string, FollowerBracketFSM> - FSM registry (primary output)
- `_orderIdToFsmKey` - ConcurrentDictionary<string, string> - Reverse lookup index
- `_positionPassFailedFirstSeen` - ConcurrentDictionary<string, DateTime> - REAPER grace window tracking

**External Calls**:
- `Print()` - NinjaTrader logging (3 call sites: lines 645-650, 736-742, 745-758)
- `DateTime.UtcNow` - Timestamp generation (4 call sites)
- `Math.Max()` / `Math.Abs()` - Quantity calculations (3 call sites)

### Coupling Analysis
- **Afferent Coupling (Ca)**: 1 (single caller)
- **Efferent Coupling (Ce)**: 0 (no outbound method calls to other classes)
- **Instability (I)**: 0.0 (perfectly stable - no dependencies on other modules)
- **Risk**: MINIMAL - changes are fully contained within this file

---

## Risk Hotspots

### 1. Two-Pass Algorithm Correctness
**Risk**: HIGH  
**Why**: The method implements a two-pass hydration strategy:
- **Pass 1 (Entry Orders)**: Lines 469-599 - Primary path for active trades
- **Pass 2 (Position Recovery)**: Lines 601-743 - Fallback for orphaned positions

**Critical Invariant**: Pass 2 must NOT create duplicate FSMs for accounts already handled in Pass 1. The idempotency guard at line 609-614 is load-bearing.

**Mitigation**: Extracted methods must preserve the exact sequence of dictionary checks and early-continue guards.

### 2. REAPER Grace Window Logic
**Risk**: MEDIUM  
**Why**: Lines 645-655 implement Build 999 feature - a 10-second grace period for CancelPending stop orders during disable/reconnect cycles. This prevents premature REAPER flatten cascades.

**Critical Path**: 
```csharp
if (recoveredKey == null) {
    Print("[SIMA] Phase 5 Position Pass: WARNING -- open position...");
    _positionPassFailedFirstSeen[acct.Name] = DateTime.UtcNow;
    continue;
}
```

**Mitigation**: The `ScanForRecoveryKey` extraction must preserve this exact warning message and grace window initialization.

### 3. FSM State Mapping Complexity
**Risk**: MEDIUM  
**Why**: Lines 488-503 map 8 different `OrderState` enum values to 3 `FollowerBracketState` values with specific business logic:
- `Filled/PartFilled` → `Active` (with live position quantity lookup)
- `Accepted` → `Accepted`
- `Working/Submitted/Initialized/ChangePending/ChangeSubmitted` → `Submitted`
- All other states → skip FSM creation (terminal states)

**Mitigation**: The `MapBrokerStateToFSM` extraction must preserve this exact mapping and the position quantity calculation at lines 505-518.

### 4. Order ID Indexing Integrity
**Risk**: MEDIUM  
**Why**: The `_orderIdToFsmKey` reverse index is critical for SIMA dispatch routing. Every order (entry, stop, target1-5) must be indexed with its FSM key. Missing an index breaks order callback routing.

**Critical Pattern** (repeated 7x):
```csharp
if (!string.IsNullOrEmpty(order.OrderId)) {
    _orderIdToFsmKey[order.OrderId] = entryKey;
    ordersIndexed++;
}
```

**Mitigation**: The `LinkTargetOrderToFSM` parameterized helper must preserve this exact null-check and counter increment.

### 5. Duplication Elimination Risk
**Risk**: LOW (but HIGH VALUE)  
**Why**: Lines 543-588 (Pass 1) and 684-729 (Pass 2) contain 10 identical target-linking blocks (5 targets × 2 passes). Collapsing to a single helper is straightforward but must handle the `ref int ordersIndexed` parameter correctly.

**Benefit**: Eliminates 80 lines of code, reduces maintenance surface by 27%.

---

## Concurrency & Threading

**Thread Safety**: ✅ COMPLIANT  
- Method is called on NinjaTrader strategy thread (single-threaded Actor model)
- All dictionary mutations are on ConcurrentDictionary types (thread-safe)
- Zero `lock()` statements (V12 DNA compliant)
- No shared mutable state across threads

**FSM Actor Model**: ✅ ALIGNED  
- Method is part of SIMA lifecycle (Actor-serialized execution)
- All state transitions are explicit and traceable
- No hidden concurrency hazards

---

## Test Coverage

**Current State**: ZERO unit tests exist for this method (verified via test audit).

**Testing Strategy**: V12 uses F5 compile + live NinjaTrader session as primary verification gate. No xUnit harness exists for strategy code.

**Post-Extraction Verification Plan**:
1. **Compile Gate**: `dotnet build` must pass (zero errors)
2. **Complexity Gate**: `complexity_audit.py` must show CYC ≤8 for all methods
3. **ASCII Gate**: `deploy-sync.ps1` must pass (no Unicode violations)
4. **Behavioral Gate**: F5 in NinjaTrader + reconnect scenario must hydrate FSMs correctly

**Recommended Future Tests** (out of scope for this epic):
- Unit test for `MapBrokerStateToFSM` covering all 8 OrderState branches
- Unit test for `LinkTargetOrderToFSM` covering null handling and index updates
- Integration test for full hydration cycle with mixed order states
- Regression test for REAPER grace window logic

---

## Change Surface Area

### Files Modified
- ✅ `src/V12_002.SIMA.Lifecycle.cs` (ONLY file touched)

### Methods Modified
- ✅ `HydrateFSMsFromWorkingOrders()` - Reduced from 296 lines to ~40 lines (orchestration only)

### Methods Created (7 new private methods)
1. `MapBrokerStateToFSM()` - State mapping + quantity calculation (~40 lines)
2. `CreateFollowerBracketFSM()` - FSM struct initialization (~30 lines)
3. `LinkStopOrder()` - Stop order indexing (~15 lines)
4. `LinkTargetOrderToFSM()` - Parameterized target linking (~20 lines, replaces 80 lines)
5. `RegisterFSM()` - Dictionary insertion + logging (~10 lines)
6. `ScanForRecoveryKey()` - Orphaned position detection (~40 lines)
7. `CreateRecoveryFSM()` - Position-pass FSM creation (~30 lines)

**Total New LOC**: ~185 lines (vs 296 original = 37% reduction after duplication elimination)

### Public API Impact
- ❌ ZERO - No public method signatures changed
- ❌ ZERO - No external callers affected
- ❌ ZERO - No cross-file dependencies introduced

### Data Structure Impact
- ❌ ZERO - No changes to `_followerBrackets` structure
- ❌ ZERO - No changes to `_orderIdToFsmKey` structure
- ❌ ZERO - No changes to `FollowerBracketFSM` struct

---

## V12 DNA Compliance Check

### Current Violations
- ❌ **CYC 71** - 9x over Jane Street threshold (≤8)
- ❌ **296 lines** - God-method anti-pattern
- ❌ **80 lines duplication** - DRY violation (5x target linking)
- ❌ **Cognitive complexity** - Impossible to reason about under microsecond constraints

### Current Compliance
- ✅ **Zero lock() statements** - Already Actor-serialized
- ✅ **ASCII-only strings** - No Unicode violations detected
- ✅ **Private visibility** - No external API exposure

### Post-Extraction Targets
- ✅ **All methods CYC ≤8** - Jane Street GODMODE compliance
- ✅ **All methods ≥15 LOC** - Extraction floor satisfied
- ✅ **Zero new locks** - Maintain Actor model
- ✅ **Zero Unicode** - ASCII-only mandate
- ✅ **deploy-sync.ps1 pass** - Hard-link integrity

---

## Architectural Context

### SIMA Lifecycle Position
This method is **Phase 5** of the SIMA (Symmetry Intelligent Multi-Account) lifecycle:
1. Phase 1: Adopt working orders from broker
2. Phase 2: Build position info dictionaries
3. Phase 3: Categorize orders (entry/stop/target)
4. Phase 4: Validate order consistency
5. **Phase 5: Hydrate FSMs from working orders** ← THIS EPIC
6. Phase 6: Start FSM monitoring loops

**Upstream**: Called by `HydrateWorkingOrdersFromBroker()` after order categorization  
**Downstream**: Populates `_followerBrackets` for FSM dispatch and monitoring

### FSM Registry Role
The `_followerBrackets` dictionary is the **central FSM registry** for SIMA:
- Key: `entryKey` (unique identifier per follower position)
- Value: `FollowerBracketFSM` struct (state machine for bracket order lifecycle)
- Used by: SIMA dispatch, order callbacks, position monitoring, REAPER safety checks

**Critical Invariant**: Every active follower position MUST have exactly one FSM entry. Duplicates or missing entries break SIMA dispatch.

---

## Extraction Complexity Assessment

### Straightforward Extractions (Low Risk)
1. **LinkStopOrder** - Pure dictionary lookup + index update (15 lines)
2. **RegisterFSM** - Pure dictionary insert + logging (10 lines)
3. **CreateFollowerBracketFSM** - Pure struct initialization (30 lines)

### Moderate Complexity Extractions (Medium Risk)
4. **MapBrokerStateToFSM** - Nested if/else chain + position lookup (40 lines)
5. **LinkTargetOrderToFSM** - Parameterized helper with ref parameter (20 lines)

### Complex Extractions (Higher Risk)
6. **ScanForRecoveryKey** - Loop with early-exit logic + grace window (40 lines)
7. **CreateRecoveryFSM** - Similar to CreateFollowerBracketFSM but with recovery semantics (30 lines)

**Overall Assessment**: MEDIUM complexity extraction. The two-pass algorithm and REAPER grace window logic require careful preservation, but the zero blast radius and single caller make this a controlled refactoring.

---

## Dependencies on Other Epics

**Upstream Dependencies**: NONE - This epic is self-contained.

**Downstream Dependencies**: NONE - No other epics depend on this extraction.

**Parallel Work**: This epic can proceed independently of EPIC-8 through EPIC-14 (other God-method extractions).

---

## Summary

This analysis confirms the scope document's assessment:
- ✅ **Isolated refactoring** - Zero blast radius, single caller, no cross-file dependencies
- ✅ **High value** - 88.7% complexity reduction, 27% LOC reduction via duplication elimination
- ✅ **Low risk** - Private method, self-contained, no public API changes
- ✅ **V12 DNA aligned** - Already lock-free and ASCII-compliant, just needs complexity reduction

**Recommendation**: Proceed to approach design phase. The extraction is well-bounded and the risks are manageable.