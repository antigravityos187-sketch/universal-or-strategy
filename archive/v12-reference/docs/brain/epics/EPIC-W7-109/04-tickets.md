# EPIC-W7-109 — Phase 4 Tickets

**Method**: `HydrateWorkingOrdersFromBroker`
**Source**: `src/V12_002.SIMA.Lifecycle.cs`
**Lines**: 309–457
**CYC**: 34 → target ≤7 (max any single method)
**Lane**: P4-L7
**DNA Verdict**: PASS (Phase 3)
**Extraction Count**: 5 helpers + 1 parent wiring = 6 tickets

---

## Ticket Summary

| # | Ticket | Type | Target CYC | Depends On | Priority |
|---|--------|------|-----------|------------|----------|
| 1 | Extract `TryGetMasterBrokerPosition` | extraction | ≤4 | none | P1 |
| 2 | Extract `IsMasterStopKeyEligible` | extraction | ≤2 | none | P1 |
| 3 | Extract `BuildMasterPositionInfo` | extraction | ≤3 | none | P1 |
| 4 | Extract `ApplyTradeDnaFlags` | extraction | ≤7 | none | P1 |
| 5 | Extract `ReconstructMasterActivePositions` | extraction | ≤4 | T1, T2, T3, T4 | P2 |
| 6 | Wire parent `HydrateWorkingOrdersFromBroker` | wiring | ≤5 | T5 | P3 |

**CYC Reduction**: 34 → 5 (parent, 85.3% reduction). Max helper CYC = 7. All ≤8 threshold. PASS.

---

## Ticket 1 — Extract `TryGetMasterBrokerPosition`

**Type**: extraction
**Target CYC**: ≤4
**Source Lines**: ~336–360 (Account.Positions foreach block)
**Depends On**: none
**Can Parallelize With**: T2, T3, T4

**Description**:
Extract the read-only position snapshot logic that iterates `Account.Positions.ToArray()` to find the matching instrument position into a new `private` method `TryGetMasterBrokerPosition`. This replaces reliance on `MarketPosition.Flat` sentinel returns with an explicit `bool` + `out` parameter pattern, making the "position found / not found" state unrepresentable as an ambiguous sentinel value.

**Signature**:
```csharp
private bool TryGetMasterBrokerPosition(
    out MarketPosition masterMP,
    out int masterQty,
    out double masterAvgPrice)
```

**Acceptance Criteria**:
- [ ] Method `TryGetMasterBrokerPosition` is extracted as `private` in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] Returns `true` when matching instrument position found; `false` otherwise
- [ ] `out` parameters assigned valid values on `true` return; default-initialized on `false`
- [ ] No `lock()` blocks introduced (lock-free compliance)
- [ ] All string literals are ASCII-only (no Unicode, emoji, curly quotes)
- [ ] CYC of extracted method ≤4 (verified via complexity audit)
- [ ] xUnit [Fact] test stub created confirming method is callable and returns `false` when no positions present
- [ ] Build passes: `dotnet build src/` with zero errors

---

## Ticket 2 — Extract `IsMasterStopKeyEligible`

**Type**: extraction
**Target CYC**: ≤2
**Source Lines**: ~370–378 (dual continue-guard block inside stopOrders foreach)
**Depends On**: none
**Can Parallelize With**: T1, T3, T4

**Description**:
Extract the two `continue` guard conditions from the `stopOrders` foreach loop into a named predicate `IsMasterStopKeyEligible`. The current inline guards check (1) whether the key starts with `"Fleet_"` and (2) whether `activePositions` already contains the key. Encapsulating both into a single named method eliminates the cognitive overhead of understanding two unrelated `continue` branches scattered inside the loop body.

**Signature**:
```csharp
private bool IsMasterStopKeyEligible(string key)
```

**Acceptance Criteria**:
- [ ] Method `IsMasterStopKeyEligible` is extracted as `private` in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] Returns `false` if `key.StartsWith("Fleet_")` OR `activePositions.ContainsKey(key)`; `true` otherwise
- [ ] Read-only access to `activePositions` — no writes inside this method
- [ ] No `lock()` blocks introduced
- [ ] All string literals are ASCII-only
- [ ] CYC of extracted method ≤2
- [ ] xUnit [Fact] test stub created confirming `false` returned for `"Fleet_"` prefixed keys
- [ ] Build passes with zero errors

---

## Ticket 3 — Extract `BuildMasterPositionInfo`

**Type**: extraction
**Target CYC**: ≤3
**Source Lines**: ~385–410 (PositionInfo struct init + GetTargetDistribution call)
**Depends On**: none
**Can Parallelize With**: T1, T2, T4

**Description**:
Extract the `PositionInfo` struct construction logic — including the `GetTargetDistribution` call and all field assignments — into a pure factory method `BuildMasterPositionInfo`. This method has no side effects and no state writes: it accepts scalar parameters and returns a fully-initialized `PositionInfo` value. Trade DNA flags are NOT set here (those are T4's responsibility).

**Signature**:
```csharp
private PositionInfo BuildMasterPositionInfo(
    string key,
    MarketPosition direction,
    int qty,
    double avgPrice,
    double stopPrice)
```

**Acceptance Criteria**:
- [ ] Method `BuildMasterPositionInfo` is extracted as `private` in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] Returns a fully-initialized `PositionInfo` struct (value type — zero allocation)
- [ ] Delegates to `GetTargetDistribution` for target-quantity split (no inline duplication)
- [ ] No `lock()` blocks introduced
- [ ] All string literals are ASCII-only
- [ ] CYC of extracted method ≤3
- [ ] Method is pure: no reads or writes to instance fields (only accesses passed parameters and calls existing helpers)
- [ ] xUnit [Fact] test stub created confirming struct fields match input parameters
- [ ] Build passes with zero errors

---

## Ticket 4 — Extract `ApplyTradeDnaFlags`

**Type**: extraction
**Target CYC**: ≤7
**Source Lines**: ~412–435 (5 StartsWith flag assignments + MOMO override block)
**Depends On**: none
**Can Parallelize With**: T1, T2, T3

**Description**:
Extract the trade DNA classification block into `ApplyTradeDnaFlags`. This block sets five boolean flags (`IsMOMOTrade`, `IsTRENDTrade`, `IsRetestTrade`, `IsRMATrade`, `IsFFMATrade`) via `key.StartsWith(...)` prefix checks, then applies the MOMO override rule (`if IsMOMOTrade → IsRMATrade = false`). This is the highest-CYC helper (CYC=7) due to 5 prefix checks + 1 override condition + nested sub-conditions, but it remains within the Jane Street ≤8 threshold. `ref PositionInfo` is used to avoid struct copy overhead on the hot path.

**Signature**:
```csharp
private void ApplyTradeDnaFlags(ref PositionInfo pos, string key)
```

**Acceptance Criteria**:
- [ ] Method `ApplyTradeDnaFlags` is extracted as `private` in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] Sets `pos.IsMOMOTrade`, `pos.IsTRENDTrade`, `pos.IsRetestTrade`, `pos.IsRMATrade`, `pos.IsFFMATrade` using `key.StartsWith(...)` checks
- [ ] Applies MOMO override: if `pos.IsMOMOTrade` is `true`, sets `pos.IsRMATrade = false`
- [ ] Uses `ref PositionInfo` parameter (zero struct-copy on call site)
- [ ] No `lock()` blocks introduced
- [ ] All string literals (DNA prefix strings) are ASCII-only
- [ ] CYC of extracted method ≤7
- [ ] xUnit [Fact] test stubs created: (a) MOMO prefix → IsMOMOTrade=true AND IsRMATrade=false; (b) non-MOMO prefix → flags set correctly
- [ ] Build passes with zero errors

---

## Ticket 5 — Extract `ReconstructMasterActivePositions`

**Type**: extraction
**Target CYC**: ≤4
**Source Lines**: ~336–443 (complete master position reconstruction god-block)
**Depends On**: T1 (`TryGetMasterBrokerPosition`), T2 (`IsMasterStopKeyEligible`), T3 (`BuildMasterPositionInfo`), T4 (`ApplyTradeDnaFlags`)
**Cannot Parallelize**: must execute after T1–T4 are merged

**Description**:
Extract the entire master position reconstruction god-block (lines 336–443) into an orchestrator method `ReconstructMasterActivePositions`. This method calls the four leaf helpers (T1–T4) in sequence, guarding on non-flat broker position, iterating `stopOrders`, delegating to `IsMasterStopKeyEligible` for loop guards, constructing `PositionInfo` via `BuildMasterPositionInfo`, classifying DNA flags via `ApplyTradeDnaFlags`, writing to `activePositions`, and logging each reconstruction. After this extraction, the god-block is fully replaced with a single method call in the parent.

**Signature**:
```csharp
private void ReconstructMasterActivePositions()
```

**Acceptance Criteria**:
- [ ] Method `ReconstructMasterActivePositions` is extracted as `private` in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] Calls `TryGetMasterBrokerPosition` — guards on `false` return (non-flat check preserved)
- [ ] Iterates `stopOrders` and calls `IsMasterStopKeyEligible` for each key guard
- [ ] Delegates struct construction to `BuildMasterPositionInfo`
- [ ] Delegates DNA flag classification to `ApplyTradeDnaFlags`
- [ ] Writes to `activePositions` (sole writer in this method)
- [ ] Logs each position reconstruction (existing log strings preserved verbatim — ASCII-only)
- [ ] Actor-serialized context maintained: must be called on strategy thread (no thread-safety change)
- [ ] No `lock()` blocks introduced
- [ ] CYC of extracted method ≤4
- [ ] xUnit integration test stub created confirming method runs without exception when `activePositions` is empty
- [ ] Build passes with zero errors

---

## Ticket 6 — Wire Parent `HydrateWorkingOrdersFromBroker`

**Type**: wiring
**Target CYC**: ≤5
**Depends On**: T5 (`ReconstructMasterActivePositions`)
**Cannot Parallelize**: must execute after T5 is merged

**Description**:
Replace the inline master position reconstruction god-block (lines 336–443) in `HydrateWorkingOrdersFromBroker` with a call to `ReconstructMasterActivePositions()`. The parent method retains its outer structure: AdoptFleetOrders, masterIsFleetForOrders993-gated try/catch for AdoptMasterOrders, masterIsFleetForOrders993-gated try/catch for ReconstructMasterActivePositions, HydrateFSMsFromWorkingOrders, `_orderAdoptionComplete = true`, and terminal log. Parent signature is unchanged: `private void HydrateWorkingOrdersFromBroker()` — all 2 callers (`EnumerateApexAccounts`, `ProcessInitializeSIMA`) are unaffected.

**Post-Wiring Parent Pseudocode**:
```
1. adoptedCount = AdoptFleetOrders()
2. if (!masterIsFleetForOrders993)
     try { adoptedCount += AdoptMasterOrders() }
     catch { Print warning }
3. if (!masterIsFleetForOrders993)
     try { ReconstructMasterActivePositions() }
     catch { Print warning }
4. HydrateFSMsFromWorkingOrders()
5. _orderAdoptionComplete = true
6. if (adoptedCount > 0) Print adopted log
   else Print no-orders log
```

**Acceptance Criteria**:
- [ ] Inline god-block (lines 336–443) removed from `HydrateWorkingOrdersFromBroker`
- [ ] Replaced with `ReconstructMasterActivePositions()` call inside the existing `masterIsFleetForOrders993`-gated try/catch block
- [ ] `_orderAdoptionComplete = true` unconditionally reached after all try/catch blocks (safety invariant preserved)
- [ ] Parent signature `private void HydrateWorkingOrdersFromBroker()` unchanged
- [ ] CYC of parent method ≤5 after wiring (verified via complexity audit)
- [ ] No `lock()` blocks introduced
- [ ] All string literals are ASCII-only
- [ ] CSharpier formatting check passes: `dotnet csharpier check src/`
- [ ] Build passes with zero errors: `dotnet build src/`
- [ ] Pre-push validation passes: `bash scripts/pre_push_validation.sh` (or PowerShell equivalent)

---

## Execution Order

```
T1 ──┐
T2 ──┤ (parallel)
T3 ──┤──→ T5 ──→ T6
T4 ──┘
```

T1, T2, T3, T4 are independent leaf helpers — extract in any order or in parallel.
T5 requires T1–T4 to be present (calls all four).
T6 requires T5 to be present (calls ReconstructMasterActivePositions).

---

## Jane Street Alignment

| Rule | Status |
|------|--------|
| CYC ≤8 all methods (max=7 T4, parent=5) | PASS |
| Single-responsibility per helper | PASS |
| Lock-free / Actor pattern preserved | PASS |
| Illegal states unrepresentable (bool+out vs sentinel) | PASS |
| Zero-allocation hot paths (struct return, ref param) | PASS |
| Guard clause extraction (IsMasterStopKeyEligible) | PASS |
| Extract Loop Body pattern (BuildMasterPositionInfo + ApplyTradeDnaFlags) | PASS |
| No scope creep V12.23 (all private, same partial class) | PASS |
| xUnit tests only (no NUnit/MSTest) | PASS |
| ASCII-only string literals | PASS |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-109 |
| **Method** | HydrateWorkingOrdersFromBroker |
| **Original CYC** | 34 |
| **Max Helper CYC** | 7 (ApplyTradeDnaFlags) |
| **Parent CYC Post-Extraction** | 5 |
| **Ticket Count** | 6 |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 5 (1 probe + 4 analysis thoughts) |
| **dna_verdict inherited** | PASS |
| **Generated** | 2026-06-29 |
