# Phase 4: Ticket Generation — EPIC-W7-071

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:20:00Z
**Input:** docs/brain/EPIC-W7-071/02-architecture-plan.md + docs/brain/EPIC-W7-071/03-audit-report.md

---

## Epic Summary

| Field | Value |
|---|---|
| **Epic ID** | EPIC-W7-071 |
| **Method** | `ShadowProcessFollowerStopUpdate` |
| **Source File** | `src/V12_002.SIMA.Shadow.cs` |
| **Original CYC** | 13 |
| **Target CYC (parent)** | 5 |
| **Total CYC Reduction** | 8 points |
| **Extraction Count** | 5 helpers |
| **Total Tickets** | 7 |
| **DNA Verdict** | PASS |

---

## Sequential Thinking Validation

Ticket breakdown validated via 4-thought sequential analysis:
- Thoughts 1–2: Identified 5 helper extractions + parent refactor + xUnit tests = 7 tickets
- Thought 3: Validated acceptance criteria per ticket (CYC targets, signatures, behavior)
- Thought 4: Confirmed sequencing — T1–T5 parallel-safe, T6 depends on T1-T5, T7 depends on T6

---

## Ticket Definitions

---

### T1 — Extract `IsFollowerUnknown`

**Type:** extraction
**Sequencing:** Independent (adds new private method; no modification to existing code)
**Estimated CYC of new helper:** 2

**Description:**
Extract the unknown-follower early-exit predicate from `ShadowProcessFollowerStopUpdate` into a dedicated private helper `IsFollowerUnknown`. This helper encapsulates the check that both `_followerBrackets` and `activePositions` TryGetValue lookups missed — indicating a completely unknown follower entry name.

**Target Signature:**
```csharp
private bool IsFollowerUnknown(bool hasFsm, bool hasFollowerPos)
```

**Acceptance Criteria:**
- [ ] `IsFollowerUnknown` added as `private` method in `src/V12_002.SIMA.Shadow.cs`
- [ ] Returns `true` iff both `hasFsm == false` AND `hasFollowerPos == false`
- [ ] cyc of `IsFollowerUnknown` = 2 (base 1 + 1 condition)
- [ ] No lock() blocks introduced
- [ ] ASCII-only identifiers and string literals
- [ ] Build passes with zero errors

**Estimated CYC Reduction (contribution to parent):** -1

---

### T2 — Extract `IsFollowerPositionNotReady`

**Type:** extraction
**Sequencing:** Independent (adds new private method; no modification to existing code)
**Estimated CYC of new helper:** 3

**Description:**
Extract the position-not-ready predicate into a dedicated private helper `IsFollowerPositionNotReady`. This helper returns `true` when the `PositionInfo` is absent, the entry fill has not been confirmed, or the bracket has not yet been submitted. Removing these ~3 compound conditions from the parent is the largest single cyc reduction in this extraction.

**Target Signature:**
```csharp
private bool IsFollowerPositionNotReady(bool hasFollowerPos, PositionInfo followerPos)
```

**Acceptance Criteria:**
- [ ] `IsFollowerPositionNotReady` added as `private` method in `src/V12_002.SIMA.Shadow.cs`
- [ ] Returns `true` when `!hasFollowerPos` OR entry not yet filled OR bracket not yet submitted
- [ ] cyc of `IsFollowerPositionNotReady` = 3
- [ ] No lock() blocks introduced
- [ ] ASCII-only identifiers and string literals
- [ ] Build passes with zero errors

**Estimated CYC Reduction (contribution to parent):** -3

---

### T3 — Extract `IsFsmNotReady`

**Type:** extraction
**Sequencing:** Independent (adds new private method; no modification to existing code)
**Estimated CYC of new helper:** 3

**Description:**
Extract the FSM-not-active predicate into a dedicated private helper `IsFsmNotReady`. This helper returns `true` when the FSM reference is null, the FSM is not in the Active state, or the FSM has no live StopOrder. Isolating these three checks to a named predicate makes the FSM state machine order explicit in the parent.

**Target Signature:**
```csharp
private bool IsFsmNotReady(bool hasFsm, FollowerBracketFSM fsm)
```

**Acceptance Criteria:**
- [ ] `IsFsmNotReady` added as `private` method in `src/V12_002.SIMA.Shadow.cs`
- [ ] Returns `true` when `!hasFsm` OR FSM not in Active state OR `fsm.StopOrder == null`
- [ ] cyc of `IsFsmNotReady` = 3
- [ ] No lock() blocks introduced
- [ ] ASCII-only identifiers and string literals
- [ ] Build passes with zero errors

**Estimated CYC Reduction (contribution to parent):** -3

---

### T4 — Extract `IsStopPriceAtTarget`

**Type:** extraction
**Sequencing:** Independent (adds new private method; no modification to existing code)
**Estimated CYC of new helper:** 2

**Description:**
Extract the half-tick proximity no-op guard into a dedicated private helper `IsStopPriceAtTarget`. This helper returns `true` when the current stop order price is already within half a tick of the new target price, indicating no update is necessary. The half-tick threshold is the standard no-op guard for this two-phase replace FSM.

**Target Signature:**
```csharp
private bool IsStopPriceAtTarget(Order stopOrder, double newStopPrice)
```

**Acceptance Criteria:**
- [ ] `IsStopPriceAtTarget` added as `private` method in `src/V12_002.SIMA.Shadow.cs`
- [ ] Returns `true` when `Math.Abs(stopOrder.StopPrice - newStopPrice) < TickSize * 0.5`
- [ ] cyc of `IsStopPriceAtTarget` = 2
- [ ] No lock() blocks introduced
- [ ] ASCII-only identifiers and string literals
- [ ] Build passes with zero errors

**Estimated CYC Reduction (contribution to parent):** -1

---

### T5 — Extract `ExecuteFollowerStopPropagation`

**Type:** extraction
**Sequencing:** Independent (adds new private method; no modification to existing code)
**Estimated CYC of new helper:** 1

**Description:**
Extract the log-and-delegate action into a dedicated private helper `ExecuteFollowerStopPropagation`. This helper emits the `[SHADOW] Propagating stop` log line via `LogBuffer.Format` and then calls `UpdateStopOrder` to initiate the two-phase replace FSM. Separating this action from the guard clause chain in the parent gives each concern a named boundary.

**Target Signature:**
```csharp
private void ExecuteFollowerStopPropagation(string followerEntryName, PositionInfo followerPos, double newStopPrice, FollowerBracketFSM fsm)
```

**Acceptance Criteria:**
- [ ] `ExecuteFollowerStopPropagation` added as `private` method in `src/V12_002.SIMA.Shadow.cs`
- [ ] Emits `[SHADOW] Propagating stop` log line using `LogBuffer.Format` (ASCII-only)
- [ ] Calls `UpdateStopOrder` with the same arguments as the original inline code
- [ ] cyc of `ExecuteFollowerStopPropagation` = 1
- [ ] No lock() blocks introduced
- [ ] Build passes with zero errors

**Estimated CYC Reduction (contribution to parent):** -0 (action, not guard)

---

### T6 — Refactor Parent `ShadowProcessFollowerStopUpdate`

**Type:** refactor (parent integration)
**Sequencing:** Depends on T1, T2, T3, T4, T5
**Estimated CYC of refactored parent:** 5

**Description:**
Replace the inline guard clause logic in `ShadowProcessFollowerStopUpdate` with calls to the five extracted helpers. The parent body retains only: (1) the two TryGetValue lookups, (2) four named-predicate if/return guard blocks, and (3) one unconditional call to `ExecuteFollowerStopPropagation`. No `&&`/`||` compound conditions remain in the parent. The three-valued return semantics (`false`=unknown, `true+waitingOnFollower=true`=not-ready, `true`=updated-or-noop) are preserved exactly.

**Refactored Parent Body (reference):**
```csharp
waitingOnFollower = false;
bool hasFsm = _followerBrackets.TryGetValue(followerEntryName, out var fsm);
bool hasFollowerPos = activePositions.TryGetValue(followerEntryName, out var followerPos);
if (IsFollowerUnknown(hasFsm, hasFollowerPos)) return false;
if (IsFollowerPositionNotReady(hasFollowerPos, followerPos)) { waitingOnFollower = true; return true; }
if (IsFsmNotReady(hasFsm, fsm)) { waitingOnFollower = true; return true; }
if (IsStopPriceAtTarget(fsm.StopOrder, newStopPrice)) return true;
ExecuteFollowerStopPropagation(followerEntryName, followerPos, newStopPrice, fsm);
return true;
```

**Acceptance Criteria:**
- [ ] `ShadowProcessFollowerStopUpdate` body replaced with calls to all 5 extracted helpers
- [ ] No inline compound conditions (`&&`, `||`) remain in parent body
- [ ] cyc of refactored parent = 5 (Jane Street threshold ≤8 satisfied)
- [ ] Three-valued return semantics preserved: `false`=unknown, `true+waiting=true`=not-ready, `true`=noop-or-updated
- [ ] Callers `ShadowMoveFollowerStops` and `PropagateAndCacheStopPrice` unmodified
- [ ] No lock() blocks, ASCII-only, no scope creep beyond this method
- [ ] `dotnet build` passes with zero errors

**CYC Reduction:** 13 → 5 (delta = **-8**)

---

### T7 — Add xUnit Tests for Extracted Helpers

**Type:** test
**Sequencing:** Depends on T6
**Estimated CYC of tests:** N/A (test scaffolding)

**Description:**
Add xUnit [Fact] tests covering the four predicate helpers extracted in T1–T4. Each test verifies the true-path and false-path of the named predicate. Tests must use `Assert.True` / `Assert.False` / `Assert.Equal` patterns only — no NUnit, no MSTest. One integration test verifies the refactored parent method returns the correct value for each of its three return paths (unknown=false, not-ready=true+waiting, updated=true).

**Acceptance Criteria:**
- [ ] Test file added or extended under `tests/` directory
- [ ] `[Fact]` attribute used on every test method (xUnit — no NUnit/MSTest)
- [ ] `Assert.True(IsFollowerUnknown(false, false))` — true-path test
- [ ] `Assert.False(IsFollowerUnknown(true, false))` — false-path test
- [ ] `Assert.True(IsFollowerPositionNotReady(false, null))` — not-ready true-path
- [ ] `Assert.True(IsFsmNotReady(false, null))` — FSM not-ready true-path
- [ ] `Assert.True(IsStopPriceAtTarget(stopOrder, samePrice))` — no-op guard true-path
- [ ] Integration test: `ShadowProcessFollowerStopUpdate` returns `false` for unknown follower
- [ ] `dotnet test` passes with zero failures

**Estimated CYC Reduction:** 0 (tests do not reduce parent CYC; they verify correctness)

---

## Ticket Summary

| Ticket | Title | Type | Depends On | Helper CYC | CYC Delta |
|---|---|---|---|---|---|
| T1 | Extract `IsFollowerUnknown` | extraction | — | 2 | -1 |
| T2 | Extract `IsFollowerPositionNotReady` | extraction | — | 3 | -3 |
| T3 | Extract `IsFsmNotReady` | extraction | — | 3 | -3 |
| T4 | Extract `IsStopPriceAtTarget` | extraction | — | 2 | -1 |
| T5 | Extract `ExecuteFollowerStopPropagation` | extraction | — | 1 | 0 |
| T6 | Refactor parent `ShadowProcessFollowerStopUpdate` | refactor | T1,T2,T3,T4,T5 | — | **-8 total** |
| T7 | Add xUnit tests for extracted helpers | test | T6 | — | 0 |

**Net CYC reduction:** 13 → 5 = **-8 points**
**Max CYC across all symbols:** 5 (Jane Street threshold ≤8 ✅)

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 1.5 |
| **Execution Time** | 2026-06-29T01:20:00Z |
| **Wave** | 7 |
| **Phase** | 4 |
| **jCodemunch tools called** | resolve_repo |
| **sequential-thinking calls** | 4 |
| **ticket_count** | 7 |
| **max_cyc_projected** | 5 |
| **extraction_count** | 5 |
| **Output** | docs/brain/EPIC-W7-071/04-tickets.md |
