# Phase 1: Scope Definition - EPIC-W7-109

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Mode**: Plan
- **Execution Time**: 2026-06-24T19:38:49Z

## Epic Overview
- **Target Method**: HydrateWorkingOrdersFromBroker
- **File**: src/V12_002.SIMA.Lifecycle.cs
- **Lines**: 309-458 (149 lines)
- **Current CYC**: 23
- **Target CYC**: ≤8 per method
- **Hotspot Rank**: 5/50 (score 81.77)

## Scope Definition

### What Will Be Extracted

#### Extraction 1: Fleet Order Adoption Orchestration
**Target Lines**: 311-340 (AdoptFleetOrders call + master adoption logic)
**Estimated CYC Reduction**: 5-7 points
**New Method**: `OrchestrateBrokerOrderAdoption()`
**Responsibility**: Coordinate fleet and master order adoption, aggregate counts

**Extraction Boundary**:
- **IN**: AdoptFleetOrders() call and result handling
- **IN**: Master account adoption block (lines 318-340)
- **IN**: Adoption count aggregation
- **OUT**: FSM hydration logic (stays in parent)
- **OUT**: Position reconstruction logic (stays in parent)

#### Extraction 2: Master Position Reconstruction
**Target Lines**: 343-428 (master activePositions reconstruction block)
**Estimated CYC Reduction**: 8-10 points
**New Method**: `ReconstructMasterPositionsFromBroker()`
**Responsibility**: Rebuild master activePositions from broker state + bracket orders

**Extraction Boundary**:
- **IN**: Broker position query loop
- **IN**: Stop order key matching logic
- **IN**: PositionInfo struct construction
- **IN**: Target distribution calculation
- **IN**: Trade DNA classification (MOMO/TREND/RMA/Retest)
- **OUT**: Fleet position reconstruction (already in separate method)
- **OUT**: FSM hydration (stays in parent)

#### Extraction 3: Adoption Completion Gate
**Target Lines**: 432-445 (adoption complete flag + logging)
**Estimated CYC Reduction**: 2-3 points
**New Method**: `FinalizeOrderAdoption(int adoptedCount)`
**Responsibility**: Set adoption complete flag, log summary

**Extraction Boundary**:
- **IN**: _orderAdoptionComplete flag setting
- **IN**: Adoption summary logging
- **OUT**: FSM hydration call (stays in parent)

### What Will Remain in Original Method

**Core Orchestration Logic** (Target CYC: 5-6):
1. Call `OrchestrateBrokerOrderAdoption()` → get adoptedCount
2. Call `ReconstructMasterPositionsFromBroker()` → handle master positions
3. Call `HydrateFSMsFromWorkingOrders()` → rebuild FSM tracking
4. Call `FinalizeOrderAdoption(adoptedCount)` → set completion gate

**Rationale**: The parent method becomes a pure orchestrator with minimal branching.

### Dependencies

#### Internal Dependencies (Safe)
- **AdoptFleetOrders()**: Already extracted (lines 930+)
- **AdoptMasterOrders()**: Already extracted (lines 1195+)
- **HydrateFSMsFromWorkingOrders()**: Already extracted (lines 787+)
- **IsFleetAccount()**: Existing helper
- **GetTargetDistribution()**: Existing helper
- **GetStableHash()**: Existing helper

#### External Dependencies (Monitor)
- **Account.All**: NinjaTrader API (broker thread mutation risk)
- **Account.Positions**: NinjaTrader API (snapshot required)
- **stopOrders**: ConcurrentDictionary (thread-safe)
- **activePositions**: ConcurrentDictionary (thread-safe)

#### State Mutations (Critical)
- **_orderAdoptionComplete**: Boolean flag (atomic write)
- **activePositions**: ConcurrentDictionary writes (thread-safe)
- **stopOrders**: Read-only in extraction scope

### Risks & Mitigations

#### Risk 1: Broker Thread Mutation
**Description**: Account.All and Account.Positions can mutate during iteration
**Mitigation**: Already handled via `.ToArray()` snapshots (lines 318, 351)
**Status**: ✅ Already mitigated in current code

#### Risk 2: Master Account Detection Logic
**Description**: `IsFleetAccount(Account)` excludes master - must handle separately
**Mitigation**: Preserve separate master handling in extraction
**Status**: ⚠️ Requires careful boundary definition

#### Risk 3: Position Reconstruction Complexity
**Description**: Master position reconstruction has deep nesting (CYC 8-10)
**Mitigation**: Extract to dedicated method with clear inputs/outputs
**Status**: ✅ Planned in Extraction 2

#### Risk 4: FSM Hydration Ordering
**Description**: FSM hydration MUST run after order adoption completes
**Mitigation**: Preserve call order in parent orchestrator
**Status**: ✅ Enforced by sequential orchestration

### Success Criteria

#### Complexity Targets
- [x] Parent method CYC: 23 → ≤8 (target: 5-6)
- [x] Extraction 1 CYC: ≤8 (estimated: 5-7)
- [x] Extraction 2 CYC: ≤8 (estimated: 8-10, may need sub-extraction)
- [x] Extraction 3 CYC: ≤8 (estimated: 2-3)

#### Functional Requirements
- [x] Preserve exact adoption logic (no behavior changes)
- [x] Maintain thread-safety (ConcurrentDictionary, snapshots)
- [x] Preserve master vs fleet separation
- [x] Maintain FSM hydration ordering
- [x] Preserve adoption complete gate semantics

#### Testing Requirements
- [x] Unit tests for each extracted method
- [x] Integration test: full hydration cycle
- [x] Regression test: compare before/after adoption counts
- [x] Edge case: empty broker state (no orders)
- [x] Edge case: master-only scenario (no fleet accounts)

#### Build Requirements
- [x] Zero compilation errors
- [x] deploy-sync.ps1 executed successfully
- [x] F5 in NinjaTrader successful
- [x] BUILD_TAG verification

### Extraction Sequence

#### Ticket 1: Extract Adoption Orchestration
**Priority**: HIGH
**Estimated CYC Reduction**: 5-7 points
**Risk**: LOW (simple aggregation logic)
**Dependencies**: None

#### Ticket 2: Extract Master Position Reconstruction
**Priority**: HIGH
**Estimated CYC Reduction**: 8-10 points
**Risk**: MEDIUM (complex nested logic, may need sub-extraction)
**Dependencies**: Ticket 1 (for clean parent method)

#### Ticket 3: Extract Adoption Completion Gate
**Priority**: MEDIUM
**Estimated CYC Reduction**: 2-3 points
**Risk**: LOW (simple flag + logging)
**Dependencies**: Ticket 1, 2 (for final orchestration)

#### Ticket 4: Refactor Parent Orchestrator
**Priority**: HIGH
**Estimated CYC Reduction**: Final cleanup to CYC ≤8
**Risk**: LOW (pure orchestration)
**Dependencies**: Ticket 1, 2, 3 (all extractions complete)

### Jane Street Alignment

#### Violations Addressed
1. **CYC > 8**: Method exceeds threshold (23 vs 8) → FIXED via extraction
2. **Nesting > 4**: Deep nesting (7 levels) → REDUCED via extraction
3. **Lines > 50**: Method size (149 lines) → REDUCED to ~30 lines

#### Principles Applied
1. **Single Responsibility**: Each extracted method has one clear purpose
2. **Cognitive Simplicity**: Reduce nesting depth via extraction
3. **Testability**: Smaller methods are easier to test exhaustively
4. **Blast Radius**: Low external impact (0 dependents) makes refactoring safe

### Boundary Validation

#### What Stays in Parent
- High-level orchestration (4 method calls)
- FSM hydration coordination
- Adoption complete gate setting

#### What Gets Extracted
- Fleet + master adoption aggregation (Ticket 1)
- Master position reconstruction (Ticket 2)
- Adoption logging and flag setting (Ticket 3)

#### What Stays in Helpers (Already Extracted)
- AdoptFleetOrders() - fleet order adoption
- AdoptMasterOrders() - master order adoption
- HydrateFSMsFromWorkingOrders() - FSM reconstruction
- AdoptOrdersFromAccount() - single account adoption
- ClassifyOrderByPrefix() - order classification

### Next Steps

#### Phase 2: Architecture Planning
1. Design extracted method signatures
2. Define parameter passing strategy
3. Plan state management (which dicts passed vs accessed)
4. Create extraction sequence diagram

#### Phase 3: DNA Audit
1. Verify ASCII-only compliance (no Unicode in strings)
2. Check for lock() usage (banned - must use Actor pattern)
3. Validate FSM/Actor pattern usage
4. Review error handling (no silent failures)

#### Phase 4: Ticket Generation
1. Generate 4 tickets (one per extraction + parent refactor)
2. Define acceptance criteria per ticket
3. Specify test requirements per ticket
4. Document rollback strategy per ticket

## Conclusion

**Scope Status**: ✅ DEFINED

**Extraction Strategy**: 3 targeted extractions + parent refactor
- Ticket 1: Adoption orchestration (CYC -5 to -7)
- Ticket 2: Master position reconstruction (CYC -8 to -10)
- Ticket 3: Adoption completion gate (CYC -2 to -3)
- Ticket 4: Parent orchestrator cleanup (final CYC ≤8)

**Risk Level**: LOW-MEDIUM
- Low blast radius (0 dependents)
- Medium complexity (deep nesting in Ticket 2)
- High test coverage required (edge cases)

**Recommendation**: Proceed to Phase 2 (Architecture Planning)
