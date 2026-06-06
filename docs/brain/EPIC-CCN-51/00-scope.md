# Epic: EPIC-CCN-51 -- Scope Alignment

## Code Area

**Target Method**: [`HydrateWorkingOrdersFromBroker()`](src/V12_002.SIMA.Lifecycle.cs:309)  
**File**: `src/V12_002.SIMA.Lifecycle.cs`  
**Line Range**: 309-724 (416 lines)  
**Current Metrics**:
- **CYC**: 79 (5.3x Jane Street threshold of 15)
- **LOC**: 416 (5.2x LOC threshold of 80)
- **Nesting**: Deep (multiple nested foreach + if/else chains)
- **Churn**: High (CodeScene LARGE RED circle)

## Validated Problem

**Motivation**: This is the **#1 consensus priority** from Phase 0 cross-reference analysis:
- **CodeScene**: Largest behavioral hotspot (LARGE RED circle)
- **jCodemunch**: Highest structural complexity (CYC 79)
- **Complexity Audit**: CRITICAL-REFACTOR flag

**Root Cause Analysis**:
The method is a 416-line God-method that performs FIVE distinct responsibilities:

1. **Fleet Order Adoption** (lines 313-408): Nested loops iterating `Account.All` → `acct.Orders`, classifying orders by prefix (Stop_, S_, T1-T5_, Fleet_), routing to dictionaries
2. **Active Position Reconstruction** (lines 410-450): Rebuilds `activePositions` structs for fleet entries to prevent REAPER audit divergence
3. **Master Order Adoption** (lines 452-480): Separate loop for master account bracket orders (mirrors fleet logic but no FSM)
4. **Master Position Reconstruction** (lines 482-520): Calls `ReconstructMasterPositionFromBrackets()`
5. **FSM Hydration** (line 523): Delegates to `HydrateFSMsFromWorkingOrders()` (CYC 72 - also needs refactoring)

**Complexity Drivers**:
- **Nested Loops**: `foreach Account` → `foreach Order` → classification logic
- **Prefix Classification**: 8-way if/else chain (Stop_, S_, T1_, T2_, T3_, T4_, T5_, Fleet_)
- **State Guards**: Multi-condition OrderState checks (Working, Accepted, Submitted, ChangePending, ChangeSubmitted)
- **Position Reconstruction**: Inline logic for MarketPosition, entry price, stop distance calculations
- **Error Handling**: Try/catch blocks around each account iteration

**Critical Code Smells**:
- **Violation of Single Responsibility**: One method handles fleet adoption, master adoption, position reconstruction, and FSM orchestration
- **Duplication**: Fleet and master adoption logic is nearly identical (copy-paste with minor variations)
- **Inline Business Logic**: Position reconstruction logic embedded in adoption loop (should be extracted)
- **Magic Strings**: Hardcoded prefixes ("Stop_", "T1_", etc.) scattered throughout

## Scope Boundaries

### IN SCOPE
- **Primary Target**: `HydrateWorkingOrdersFromBroker()` (lines 309-724)
- **Extraction Strategy**: Break into 5 focused sub-methods:
  1. `AdoptFleetWorkingOrders()` - Fleet order classification and adoption
  2. `RebuildFleetActivePositions()` - Position struct reconstruction for fleet
  3. `AdoptMasterWorkingOrders()` - Master account order adoption
  4. `ReconstructMasterPositionFromBrackets()` - Already exists, just call it
  5. `HydrateFSMsFromWorkingOrders()` - Already exists (CYC 72 - separate epic)

### OUT OF SCOPE (Explicit Exclusions)
- **`HydrateFSMsFromWorkingOrders()`**: CYC 72 method - will be addressed in **EPIC-CCN-52** (separate epic)
- **Helper Methods**: `ClassifyAndRouteFleetOrder()`, `IsFleetAccount()`, `IsOrderStateAdoptable()` - leave untouched
- **Caller Sites**: This method is called from lifecycle/reconnect paths - callers remain unchanged
- **FSM Logic**: All `FollowerBracketFSM` state machine logic is out of scope
- **REAPER Integration**: `_orderAdoptionComplete` flag and REAPER audit logic unchanged

### BOUNDARY CLARIFICATIONS
- **Position Reconstruction**: The inline position reconstruction logic (lines 410-450) will be extracted to a new method `RebuildFleetActivePositions()`, but the **logic itself** remains unchanged (pure structural movement)
- **Prefix Classification**: The 8-way if/else chain will be extracted to a helper method `ClassifyOrderByPrefix()` for reuse between fleet and master adoption
- **Error Handling**: Try/catch blocks will remain in the extracted methods (preserve error isolation per account)

## Risk Level

**CRITICAL** - This is the highest-risk refactoring in the entire codebase:

### Risk Factors
1. **Core SIMA Lifecycle**: This method runs during strategy startup and broker reconnect - failures break multi-account orchestration
2. **State Reconstruction**: Rebuilds `activePositions`, `entryOrders`, `stopOrders`, `target1-5Orders` - errors cause REAPER false positives
3. **FSM Hydration**: Orchestrates FSM creation for follower brackets - mistakes leave positions unprotected
4. **No Unit Tests**: Zero test coverage - F5 compile + live session is the only verification
5. **High Churn**: CodeScene shows frequent changes - indicates ongoing bug fixes and edge case handling

### Mitigation Strategy
- **Incremental Extraction**: Extract one sub-method at a time, verify with F5 + complexity audit after each
- **Preserve Logic**: Zero logic changes - pure structural movement only
- **Checkpointing**: Bob CLI automatic checkpointing after each extraction
- **Rollback Plan**: Git branch isolation - can revert entire epic if F5 fails

## V12 DNA Constraints

### Mandatory Compliance
- [x] **CYC Target**: Reduce from 79 to <20 per method (residual dispatcher + 5 extracted methods)
- [x] **Lock-Free**: No new `lock()` statements (method is already lock-free via Actor model)
- [x] **ASCII-Only**: No Unicode in string literals (already compliant - uses ASCII prefixes)
- [x] **Extraction Floor**: Each extracted method must be ≥15 LOC (all 5 sub-methods exceed this)
- [x] **deploy-sync.ps1**: Mandatory after every src/ edit (hard-link integrity)

### Specific Constraints for This Epic
- **Preserve FSM State Transitions**: Do not modify `FollowerBracketState` enum or FSM creation logic
- **Preserve Order Tracking**: `entryOrders`, `stopOrders`, `target1-5Orders` dictionaries must remain functionally identical
- **Preserve REAPER Integration**: `_orderAdoptionComplete` flag timing unchanged
- **Preserve Error Isolation**: Try/catch blocks per account must remain (prevents one bad account from blocking others)

### Jane Street Alignment
- **Correctness by Construction**: Extract methods with clear preconditions/postconditions (e.g., `AdoptFleetWorkingOrders` requires `IsFleetAccount(acct) == true`)
- **Cognitive Simplicity**: Each extracted method should have CYC ≤15 (single responsibility)
- **Immutable Data**: Prefer readonly structs for position reconstruction (already using `PositionInfo` struct)
- **Explicit Error Handling**: Keep try/catch blocks explicit (no silent failures)

---

## Forensic Notes (jCodemunch Discrepancy)

**CRITICAL**: jCodemunch reported this method as 24 lines (271-294), but actual inspection shows **416 lines (309-724)**. This discrepancy suggests:
1. jCodemunch index is stale (last indexed 2026-05-19, 18 days ago)
2. The method was recently expanded or jCodemunch's AST parser failed to capture the full extent
3. **Action Required**: Re-index the repository before Phase 2 (Analysis) to ensure accurate blast radius and dependency data

**Workaround**: Use `complexity_audit.py` output (CYC 79, LOC 416) as ground truth for this epic.

---

## Next Steps (Phase 2: Analysis)

1. **Re-index Repository**: Run `jcodemunch index_folder` to update stale data
2. **Blast Radius Analysis**: Verify zero external callers (method is `private`)
3. **Dependency Mapping**: Identify all helper methods called by this God-method
4. **Risk Hotspot Identification**: Map which sub-sections have highest CYC contribution
5. **Test Coverage Assessment**: Confirm zero unit tests, document F5 verification strategy

---

**[INTAKE-GATE]** Scope alignment complete. Awaiting Director confirmation before proceeding to Phase 2 (Analysis).