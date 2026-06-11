# Phase 1: Scope Definition - EPIC-CCN-109

## Epic Metadata
- **Epic ID**: EPIC-CCN-109
- **Target Method**: `HydrateWorkingOrdersFromBroker`
- **File**: `src/V12_002.SIMA.Lifecycle.cs`
- **Line Range**: 309-413 (105 lines)
- **Current CYC**: 19
- **Target CYC**: ≤8 (58% reduction)
- **Risk Level**: MEDIUM

---

## Method Purpose
Adopts working broker orders into tracking dictionaries after restart or reconnect. Orchestrates 4 phases:
1. **Fleet Adoption**: Calls `AdoptFleetOrders()` to process fleet account orders
2. **Master Adoption**: Calls `AdoptMasterOrders()` to process master account orders
3. **Master Position Reconstruction**: Rebuilds `activePositions` from adopted brackets + broker state
4. **FSM Hydration**: Calls `OrchestrateFSMHydration()` to rebuild FSMs from adopted orders

Sets `_orderAdoptionComplete = true` when done to enable REAPER auditing.

---

## Blast Radius Analysis

### Direct Callers
**ZERO confirmed callers** (blast radius = 0.0)
- Called only from `EnumerateApexAccounts()` (line 206)
- Lifecycle method - cold path (startup/reconnect only)

### Dependencies (Methods Called)
1. `AdoptFleetOrders()` - line 314 (CYC unknown, separate method)
2. `AdoptMasterOrders()` - line 320 (CYC unknown, separate method)
3. `OrchestrateFSMHydration()` - line 408 (CYC unknown, separate method)
4. `GetTargetDistribution()` - line 363 (helper method)
5. `GetStableHash()` - line 377 (helper method)

### State Mutations
- `activePositions` dictionary (lines 380, 395)
- `_orderAdoptionComplete` flag (line 410)
- Reads: `stopOrders`, `Account.Positions`, `Instrument.FullName`

---

## Complexity Breakdown

### Current Structure (CYC 19)
```
HydrateWorkingOrdersFromBroker (CYC 19)
├─ adoptedCount = AdoptFleetOrders() [+1]
├─ if (!masterIsFleetForOrders993) [+1]
│  ├─ try-catch [+1]
│  │  └─ adoptedCount += AdoptMasterOrders()
│  └─ catch (Exception ex)
├─ if (!masterIsFleetForOrders993) [+1] (Master position reconstruction)
│  ├─ try-catch [+1]
│  │  ├─ foreach (Position brokerPos) [+1]
│  │  │  ├─ if (brokerPos != null && ...) [+1]
│  │  │  │  └─ break
│  │  ├─ if (masterMP != MarketPosition.Flat && masterQty > 0) [+1]
│  │  │  └─ foreach (var stopKvp in stopOrders.ToArray()) [+1]
│  │  │     ├─ if (key.StartsWith("Fleet_")) continue [+1]
│  │  │     ├─ if (activePositions.ContainsKey(key)) continue [+1]
│  │  │     ├─ pos.IsMOMOTrade = key.StartsWith("MOMO") [+1]
│  │  │     ├─ pos.IsTRENDTrade = trendMnlMatch || key.StartsWith("TRMA_") [+1]
│  │  │     ├─ pos.IsRetestTrade = key.StartsWith("Retest") [+1]
│  │  │     ├─ pos.IsRMATrade = key.StartsWith("TRMA_") || pos.IsRetestTrade [+1]
│  │  │     ├─ pos.IsFFMATrade = key.StartsWith("FFMA") [+1]
│  │  │     └─ if (pos.IsMOMOTrade) pos.IsRMATrade = false [+1]
│  └─ catch (Exception ex)
└─ OrchestrateFSMHydration()
```

**Hotspot**: Master position reconstruction block (lines 344-413) contributes CYC 12 of 19 (63%).

---

## Extraction Strategy

### Phase 1.5 Boundary Validation
**MANDATORY**: Single-method extraction only. No scope creep.

### Proposed Extraction: `ReconstructMasterPositionFromBroker`

**Target Lines**: 344-413 (70 lines)
**Estimated CYC**: 12 (reduces parent to CYC 7)

**Signature**:
```csharp
/// <summary>
/// Reconstructs master activePositions from adopted bracket orders + broker state.
/// Handles edge case where master has filled position but no working entry order.
/// </summary>
/// <returns>Count of reconstructed positions (for logging)</returns>
private int ReconstructMasterPositionFromBroker()
```

**Inputs** (via class state):
- `Account` (master account)
- `Account.Positions` (broker positions)
- `stopOrders` dictionary (adopted stop orders)
- `activePositions` dictionary (target for reconstruction)
- `Instrument.FullName` (instrument filter)

**Outputs**:
- Mutates `activePositions` dictionary (adds reconstructed positions)
- Returns `int` (count of reconstructed positions)

**Logic Flow**:
1. Read master broker position (MarketPosition, Quantity, AveragePrice)
2. If flat, return 0 (no reconstruction needed)
3. Iterate `stopOrders` to find master stop order (skip Fleet_ keys)
4. For each master stop order:
   - Skip if `activePositions` already contains key
   - Build `PositionInfo` struct from broker state + stop order
   - Set trade DNA flags (IsMOMOTrade, IsTRENDTrade, etc.)
   - Insert into `activePositions`
5. Return count of reconstructed positions

**Complexity Reduction**:
- Parent method: 19 → 7 (CYC reduction: 63%)
- Extracted method: CYC ≤8 (target: 7)

---

## V12 DNA Compliance

### Lock-Free ✅
- No `lock()` statements
- Reads from concurrent dictionaries (safe)
- Single-write to `activePositions` (actor-serialized)

### ASCII-Only ✅
- All string literals are ASCII
- No Unicode, emoji, or curly quotes

### Correctness by Construction ✅
- Idempotent: Skip if `activePositions.ContainsKey(key)`
- Defensive: Null checks on `brokerPos`, `stopCand`
- Fail-safe: Try-catch prevents cascade failure

### Jane Street Alignment ✅
- **Cognitive Simplicity**: Extraction reduces parent CYC from 19 to 7
- **Single Responsibility**: Extracted method handles only master position reconstruction
- **Bounded Latency**: Cold path (startup only), no hot-path impact

---

## Risk Assessment

### Extraction Risk: **LOW**
- **Blast Radius**: 0 (no external callers)
- **State Coupling**: Reads from `stopOrders`, writes to `activePositions` (both actor-serialized)
- **Error Handling**: Try-catch preserved in extracted method
- **Testing**: Integration test via F5 in NinjaTrader (verify BUILD_TAG)

### Regression Risk: **LOW**
- Logic is pure structural movement (zero drift)
- Existing try-catch prevents cascade failure
- Idempotent guards prevent double-reconstruction

---

## Success Criteria

### Phase 1.5 (Scope Boundary) ✅
- [x] Single-method extraction confirmed
- [x] No scope creep (no adjacent fixes)
- [x] Boundary validated (lines 344-413)

### Phase 2 (Architecture Planning)
- [ ] Extraction plan documented
- [ ] Method signature defined
- [ ] Call graph validated
- [ ] Jane Street compliance verified

### Phase 3 (DNA & PR Audit)
- [ ] V12 DNA compliance verified
- [ ] PR hygiene validated (diff <10k chars)
- [ ] Complexity audit passed (CYC ≤8)

### Phase 4 (Ticket Generation)
- [ ] Ticket created: `ticket-01-extract-reconstruct-master-position.md`
- [ ] Execution order documented

### Phase 5 (Execution)
- [ ] Method extracted to `ReconstructMasterPositionFromBroker`
- [ ] Parent method updated (call extracted method)
- [ ] `deploy-sync.ps1` executed (ASCII gate passed)
- [ ] BUILD_TAG bumped in `src/V12_002.cs`
- [ ] F5 in NinjaTrader successful

### Phase 6 (Verification)
- [ ] Complexity audit: Parent CYC ≤8, Extracted CYC ≤8
- [ ] Build passes (zero errors)
- [ ] Unit tests pass (if applicable)
- [ ] Integration test passes (F5 in NinjaTrader)

---

## Exclusions (No Scope Creep)

### Out of Scope ❌
- `AdoptFleetOrders()` refactoring (separate epic if needed)
- `AdoptMasterOrders()` refactoring (separate epic if needed)
- `OrchestrateFSMHydration()` refactoring (separate epic if needed)
- Pre-existing compilation errors (separate PR)
- Adjacent code improvements (separate PR)

### Rationale
**V12.23 Protocol**: ONE EPIC = ONE CONCERN. Mixing concerns caused EPIC-13 PR #12 failure (3 P0 blockers).

---

## Phase 1 Status
✅ **COMPLETE** - Ready for Phase 1.5 (Scope Boundary Validation)

**Next Step**: Execute Phase 1.5 to validate single-method boundary before proceeding to Phase 2.