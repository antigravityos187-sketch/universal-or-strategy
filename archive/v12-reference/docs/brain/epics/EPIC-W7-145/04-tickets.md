# Phase 4: Ticket Generation — EPIC-W7-145

**agent_name:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Epic:** EPIC-W7-145
**Input Artifacts:** `docs/brain/EPIC-W7-145/02-architecture-plan.md`, `docs/brain/EPIC-W7-145/03-audit-report.md`

---

## Method Under Extraction

| Field | Value |
|---|---|
| **Method** | `HandleFleetTargetFill` |
| **Source File** | `src/V12_002.UI.Compliance.cs` |
| **Lines** | 624–696 (73 lines) |
| **Original CYC** | 17 |
| **dna_verdict** | PASS |
| **extraction_count** | 5 helpers across 4 tickets |
| **max_cyc_projected** | 6 |

---

## Sequential Thinking Evidence

**ST-thought-1:** Analyzed `HandleFleetTargetFill` CYC=17. Identified 5 logical concerns with clean extraction boundaries: (1) string parsing of `ocoName` → `tgtNum`/`tgtEntryKey` (CYC ~2), (2) three-clause `&&` compound position guard with early-return (CYC ~2), (3) duplicate-fill guard on `tgtAlreadyProcessed` with logging (CYC ~2), (4) active fill dispatch with conditional cancel trigger (CYC ~2), (5) `foreach` stop-order cancel loop with 3 filter conditions (CYC ~6). Total accounts for CYC=17.

**ST-thought-2:** Designed 4 tickets grouping 5 helpers. T1=`DeriveTgtEntryKey` (parse), T2=`TryResolveTargetPosition` (guard), T3=`LogIfDuplicateTargetFill`+`ApplyActiveFill` (fill path — grouped because tightly coupled in same phase), T4=`CancelFleetStopOrdersForAccount` (loop body — dedicated ticket for focused review of CYC=6 method). Ordered by data-flow dependency in parent.

**ST-thought-3:** Verified all projected CYC values satisfy ≤8 threshold. Parent post-extraction: CYC=3. Helpers: 2, 2, 2, 2, 6. Max=6 ≤ 8. dna_verdict=PASS constraints satisfied (zero lock() blocks, ASCII-only literals, no scope creep, xUnit only). All 4 tickets are independently executable and verifiable.

---

## Ticket Summary

| Ticket | Title | Extracted Methods | CYC Target |
|---|---|---|---|
| **T1** | Extract `DeriveTgtEntryKey` | `DeriveTgtEntryKey` | ≤ 2 |
| **T2** | Extract `TryResolveTargetPosition` | `TryResolveTargetPosition` | ≤ 2 |
| **T3** | Extract Fill-Path Helpers | `LogIfDuplicateTargetFill`, `ApplyActiveFill` | ≤ 2 each |
| **T4** | Extract `CancelFleetStopOrdersForAccount` | `CancelFleetStopOrdersForAccount` | ≤ 6 |

---

## T1 — Extract `DeriveTgtEntryKey`

**ID:** T1
**Title:** Extract string-parsing logic into `DeriveTgtEntryKey`
**Phase 5 Mode:** v12-engineer (Bob CLI)
**CYC Target:** Parent retains no parse branches; `DeriveTgtEntryKey` CYC ≤ 2

### Description

`HandleFleetTargetFill` opens with character-indexing parse logic that derives `tgtNum` (int) and `tgtEntryKey` (string) from the `ocoName` parameter. This pure data-transformation concern has no side effects and no dependency on instance state beyond the input string. Extract it into a `private static` helper.

**Extraction target — new method:**
```csharp
private static string DeriveTgtEntryKey(string ocoName, out int tgtNum)
```

**Steps:**
1. Create `DeriveTgtEntryKey` containing all character-indexing, prefix-construction, and `LastIndexOf`-trim logic currently in lines 624–634 (approximate).
2. Replace the extracted block in `HandleFleetTargetFill` with a single call: `string tgtEntryKey = DeriveTgtEntryKey(ocoName, out int tgtNum);`
3. `private static` — no instance references in parse logic.

### Acceptance Criteria

- [ ] `DeriveTgtEntryKey` exists in `src/V12_002.UI.Compliance.cs` as `private static`
- [ ] `HandleFleetTargetFill` contains exactly one call to `DeriveTgtEntryKey` and no inline parse logic
- [ ] `dotnet build` passes with zero errors
- [ ] CYC of `DeriveTgtEntryKey` ≤ 2
- [ ] xUnit `[Fact]` test: `Assert.Equal(expectedKey, DeriveTgtEntryKey(input, out int num))` for at least 1 representative input

---

## T2 — Extract `TryResolveTargetPosition`

**ID:** T2
**Title:** Extract compound position guard into `TryResolveTargetPosition`
**Phase 5 Mode:** v12-engineer (Bob CLI)
**CYC Target:** `TryResolveTargetPosition` CYC ≤ 2; parent gains 1 guard-clause early-return

### Description

`HandleFleetTargetFill` contains a three-clause `&&` compound guard: `IsNullOrEmpty(tgtEntryKey)` check, `activePositions.TryGetValue(tgtEntryKey, out PositionInfo tgtPos)` lookup, and a null check on the resulting `PositionInfo`. This guard enables an early-return in the parent. Extract the compound condition into a single boolean helper that collapses the three checks into one `false`-on-failure return.

**Extraction target — new method:**
```csharp
private bool TryResolveTargetPosition(string tgtEntryKey, out PositionInfo tgtPos)
```

**Steps:**
1. Create `TryResolveTargetPosition` encapsulating `IsNullOrEmpty` check + `activePositions.TryGetValue` + null guard; returns `false` when any condition fails, `true` with populated `tgtPos` on success.
2. Replace the compound guard in `HandleFleetTargetFill` with: `if (!TryResolveTargetPosition(tgtEntryKey, out PositionInfo tgtPos)) return;`
3. Dependency: execute after T1 (requires `tgtEntryKey` local variable established by T1 call).

### Acceptance Criteria

- [ ] `TryResolveTargetPosition` exists in `src/V12_002.UI.Compliance.cs` as `private`
- [ ] `HandleFleetTargetFill` contains a single guard-clause early-return via `!TryResolveTargetPosition(...)`
- [ ] No multi-clause `&&` compound guard remains in parent for the position lookup
- [ ] `dotnet build` passes with zero errors
- [ ] CYC of `TryResolveTargetPosition` ≤ 2
- [ ] xUnit `[Fact]` test: `Assert.False(TryResolveTargetPosition(string.Empty, out _))` and `Assert.True(...)` for valid key

---

## T3 — Extract Fill-Path Helpers (`LogIfDuplicateTargetFill` + `ApplyActiveFill`)

**ID:** T3
**Title:** Extract fill-processing path into `LogIfDuplicateTargetFill` and `ApplyActiveFill`
**Phase 5 Mode:** v12-engineer (Bob CLI)
**CYC Target:** Both helpers CYC ≤ 2 each; parent loses 2 branches

### Description

After the position guard, `HandleFleetTargetFill` branches on `tgtAlreadyProcessed` (duplicate-fill signal) and then dispatches the active-fill path. These two concerns are tightly coupled in sequence within the fill-processing phase and are extracted together in a single ticket to wire the parent's post-fill path in one pass.

**(a) `LogIfDuplicateTargetFill`** — encapsulates the `if(tgtAlreadyProcessed)` branch: logs `[1104.1 GUARD]` duplicate-fill warning and returns `true` to signal early-return to parent.

**(b) `ApplyActiveFill`** — encapsulates: log `[1104.1]` fill success, check `tgtRemaining <= 0`, and call `CancelFleetStopOrdersForAccount` when condition holds. (Note: `CancelFleetStopOrdersForAccount` is extracted in T4; at T3 execution time, the cancel loop call site is inlined — T4 will then refactor it further.)

**Extraction targets — new methods:**
```csharp
private bool LogIfDuplicateTargetFill(bool tgtAlreadyProcessed, int tgtNum, string tgtEntryKey)

private void ApplyActiveFill(int tgtNum, int tgtApplied, decimal price, int tgtRemaining, string tgtEntryKey, Account ocoAcct)
```

**Steps:**
1. Create `LogIfDuplicateTargetFill`: if `tgtAlreadyProcessed` is true, emit `[1104.1 GUARD]` log, return `true`; else return `false`.
2. Replace `if(tgtAlreadyProcessed)` block in parent with: `if (LogIfDuplicateTargetFill(tgtAlreadyProcessed, tgtNum, tgtEntryKey)) return;`
3. Create `ApplyActiveFill`: emit `[1104.1]` log, check `tgtRemaining <= 0`, inline the cancel-loop body (T4 will extract the loop in the next ticket).
4. Replace the active-fill block in parent with a single call to `ApplyActiveFill(...)`.
5. Dependency: execute after T2 (parent must have `tgtPos` resolved).

### Acceptance Criteria

- [ ] `LogIfDuplicateTargetFill` exists as `private` in `src/V12_002.UI.Compliance.cs`
- [ ] `ApplyActiveFill` exists as `private` in `src/V12_002.UI.Compliance.cs`
- [ ] `HandleFleetTargetFill` body contains no inline duplicate-fill or active-fill logic
- [ ] `dotnet build` passes with zero errors
- [ ] CYC of `LogIfDuplicateTargetFill` ≤ 2
- [ ] CYC of `ApplyActiveFill` ≤ 2 (before T4 cancel-loop extraction)
- [ ] xUnit `[Fact]` tests: `Assert.True(LogIfDuplicateTargetFill(true, ...))`, `Assert.False(LogIfDuplicateTargetFill(false, ...))`

---

## T4 — Extract `CancelFleetStopOrdersForAccount`

**ID:** T4
**Title:** Extract stop-order cancel loop into `CancelFleetStopOrdersForAccount`
**Phase 5 Mode:** v12-engineer (Bob CLI)
**CYC Target:** `CancelFleetStopOrdersForAccount` CYC ≤ 6; `ApplyActiveFill` CYC ≤ 2 post-extraction

### Description

The most complex sub-concern in `HandleFleetTargetFill` is a `foreach` loop over `ocoAcct.Orders` that applies 3 filter conditions (instrument match, order state not terminal via `IsOrderTerminal`, name prefix `"Stop_"`) before calling `CancelOrderOnAccount` with per-cancel logging. This is extracted in a dedicated ticket to ensure the loop body receives focused review.

At T4 execution time, the cancel loop resides inside `ApplyActiveFill` (placed there by T3). This ticket extracts it out of `ApplyActiveFill` into a standalone helper.

**Extraction target — new method:**
```csharp
private void CancelFleetStopOrdersForAccount(Account ocoAcct)
```

**Steps:**
1. Create `CancelFleetStopOrdersForAccount`: move the entire `foreach (var ord in ocoAcct.Orders)` block with all 3 filter conditions and `CancelOrderOnAccount` call + per-cancel log.
2. In `ApplyActiveFill`, replace the `foreach` block with a single call: `CancelFleetStopOrdersForAccount(ocoAcct);`
3. Verify `ApplyActiveFill` CYC drops to ≤ 2 after extraction (only the `tgtRemaining <= 0` branch remains).
4. Dependency: execute after T3.

### Acceptance Criteria

- [ ] `CancelFleetStopOrdersForAccount` exists as `private` in `src/V12_002.UI.Compliance.cs`
- [ ] `ApplyActiveFill` contains no `foreach` loop — replaced with single call to `CancelFleetStopOrdersForAccount(ocoAcct)`
- [ ] `dotnet build` passes with zero errors
- [ ] CYC of `CancelFleetStopOrdersForAccount` ≤ 6
- [ ] CYC of `ApplyActiveFill` ≤ 2 (post-extraction)
- [ ] CYC of `HandleFleetTargetFill` (parent) ≤ 3 (final state)
- [ ] xUnit `[Fact]` test: verify `CancelFleetStopOrdersForAccount` invokes `CancelOrderOnAccount` for matching orders and skips non-matching ones

---

## CYC Reduction Summary

| Method | Pre-Extraction CYC | Post-Extraction CYC | Target Met |
|---|---|---|---|
| `HandleFleetTargetFill` (parent) | 17 | 3 | YES ≤ 8 |
| `DeriveTgtEntryKey` | — | 2 | YES ≤ 8 |
| `TryResolveTargetPosition` | — | 2 | YES ≤ 8 |
| `LogIfDuplicateTargetFill` | — | 2 | YES ≤ 8 |
| `ApplyActiveFill` | — | 2 | YES ≤ 8 |
| `CancelFleetStopOrdersForAccount` | — | 6 | YES ≤ 8 |
| **max_cyc_projected** | **17** | **6** | **YES** |

**CYC reduction: 17 → max 6 (64.7% reduction)**

---

## Execution Order

```
T1 (DeriveTgtEntryKey)
  └─ T2 (TryResolveTargetPosition)
       └─ T3 (LogIfDuplicateTargetFill + ApplyActiveFill)
            └─ T4 (CancelFleetStopOrdersForAccount)
```

Each ticket is a prerequisite for the next. Execute sequentially in order T1 → T2 → T3 → T4.

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-145 |
| **dna_verdict** | PASS |
| **Ticket Count** | 4 |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity, get_extraction_candidates |
| **sequential-thinking calls** | 4 (1 probe + 3 analysis thoughts) |
| **Output** | docs/brain/EPIC-W7-145/04-tickets.md |
| **Generated** | 2026-06-29T01:20:00Z |
