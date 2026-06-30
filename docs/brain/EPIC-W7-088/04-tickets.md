# Phase 4: Implementation Tickets — EPIC-W7-088

**Agent:** v12-phase4-tickets
**Wave:** 7 | **Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T03:30:00Z
**Input:** docs/brain/EPIC-W7-088/02-architecture-plan.md + docs/brain/EPIC-W7-088/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Method** | `SubmitRepairOrderWithAuthorization` |
| **Source File** | [`src/V12_002.REAPER.Repair.cs`](../../src/V12_002.REAPER.Repair.cs:147) |
| **Original CYC** | 34 |
| **Projected Parent CYC After All Extractions** | **5** |
| **CYC Reduction** | 34 → 5 (85.3%) |
| **ticket_count** | 7 |
| **DNA Verdict** | PASS |
| **Extraction Strategy** | 6 private helper extractions + 1 parent orchestrator reshape |

---

## Ticket Index

| Ticket | Helper Name | Concern | Lines to Move | CYC Reduction (parent) | Projected Helper CYC |
|---|---|---|---|---|---|
| T-088-01 | `TryResolveRepairAccount` | Account null guard | ~5 | −1 (34→33) | 2 |
| T-088-02 | `CreateRepairOrder` | Order creation + null guard | ~10 | −2 (33→31) | 3 |
| T-088-03 | `HasActiveFsmForAccount` | FSM state LINQ scan | ~8 | −5 (31→26) | 5 |
| T-088-04 | `ResolveRepairAuthorization` | Dispatch-pending + activePositions fallback | ~18 | −4 (26→22) | 5 |
| T-088-05 | `PrepareAndRegisterRepairOrder` | BracketSubmitted reset + entryOrders write | ~4 | 0 (22→22) | 1 |
| T-088-06 | `LogRepairOrderSubmitted` | Formatted success Print with ternary | ~6 | −1 (22→21) | 2 |
| T-088-07 | *(parent reshape)* | Replace inlined logic with helper calls | ~8 | −16 (21→5) | — |

---

## Ticket Definitions

---

### T-088-01 · Extraction: `TryResolveRepairAccount`

| Field | Value |
|---|---|
| **ticket_id** | T-088-01 |
| **helper_name** | `TryResolveRepairAccount` |
| **concern** | Null-check `repairPos.ExecutingAccount`; assign `targetAcct`; print failure and return `false` on null |
| **lines_to_move** | ~5 (account resolution block at top of `SubmitRepairOrderWithAuthorization`) |
| **cyc_reduction** | −1 from parent (removes 1 null-guard branch) |
| **projected_helper_cyc** | 2 |

**Signature:**
```csharp
private bool TryResolveRepairAccount(
    PositionInfo repairPos,
    string accountName,
    out Account targetAcct)
```

**Implementation Notes:**
- Extract the `ExecutingAccount` null-check block from the top of the parent method.
- Return `false` + emit failure `Print` if `repairPos.ExecutingAccount` is null.
- Assign `out Account targetAcct = repairPos.ExecutingAccount` on success.
- No lock blocks. ASCII-only string literals. xUnit test: assert `false` on null account, `true` on valid account.

---

### T-088-02 · Extraction: `CreateRepairOrder`

| Field | Value |
|---|---|
| **ticket_id** | T-088-02 |
| **helper_name** | `CreateRepairOrder` |
| **concern** | Resolve `OrderAction` from `Direction` (ternary), read `TotalContracts`, call `targetAcct.CreateOrder(...)`, null-check result |
| **lines_to_move** | ~10 (direction ternary + CreateOrder call + null guard on result) |
| **cyc_reduction** | −2 from parent (removes direction ternary branch + null-result guard branch) |
| **projected_helper_cyc** | 3 |

**Signature:**
```csharp
private bool CreateRepairOrder(
    Account targetAcct,
    PositionInfo repairPos,
    OrderType orderType,
    double limitPrice,
    double stopPrice,
    string repairSignal,
    out Order repairEntry)
```

**Implementation Notes:**
- Extract `OrderAction` ternary (`repairPos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.Buy`).
- Call `targetAcct.CreateOrder(...)` with resolved parameters.
- Null-check result; return `false` + print failure if null; assign `out Order repairEntry` on success.
- No lock blocks. xUnit test: verify correct `OrderAction` resolution for Long and Short positions.

---

### T-088-03 · Extraction: `HasActiveFsmForAccount`

| Field | Value |
|---|---|
| **ticket_id** | T-088-03 |
| **helper_name** | `HasActiveFsmForAccount` |
| **concern** | LINQ scan of `_followerBrackets.Values` for `Active`, `Accepted`, `Submitted`, or `Replacing` state on the given account |
| **lines_to_move** | ~8 (LINQ predicate block including 4-state OR condition) |
| **cyc_reduction** | −5 from parent (removes LINQ null guard + 4-state OR branches) |
| **projected_helper_cyc** | 5 |

**Signature:**
```csharp
private bool HasActiveFsmForAccount(string accountName)
```

**Implementation Notes:**
- Encapsulates the `_followerBrackets.Values.Any(b => b.AccountName == accountName && (b.State == Active || b.State == Accepted || b.State == Submitted || b.State == Replacing))` LINQ expression.
- `ConcurrentDictionary` TOCTOU (H1) is pre-existing and **out of scope** per V12.23 — document in docstring.
- No lock blocks. xUnit test: verify returns `true` for each of the 4 active states; `false` for inactive state.

---

### T-088-04 · Extraction: `ResolveRepairAuthorization`

| Field | Value |
|---|---|
| **ticket_id** | T-088-04 |
| **helper_name** | `ResolveRepairAuthorization` |
| **concern** | When no active FSM: check `_dispatchSyncPendingExpKeys.ContainsKey`, scan `activePositions` with `Any`, abort if neither exists, print guard messages |
| **lines_to_move** | ~18 (entire fallback authorization path including ContainsKey check, Any scan, abort condition, and Print calls) |
| **cyc_reduction** | −4 from parent (removes hasActiveFsm branch + ContainsKey branch + Any branch + abort AND branch) |
| **projected_helper_cyc** | 5 |

**Signature:**
```csharp
private bool ResolveRepairAuthorization(string accountName, bool hasActiveFsm)
```

**Implementation Notes:**
- If `hasActiveFsm` is `true`, return `true` immediately (short-circuit).
- Otherwise check `_dispatchSyncPendingExpKeys.ContainsKey(accountName)` OR `activePositions.Any(p => p.Account.Name == accountName)`.
- If neither condition is met, print guard message and return `false`.
- Pre-existing TOCTOU risk (H1) explicitly **out of scope** per V12.23 — document in docstring.
- No lock blocks. xUnit test: verify `true` when hasActiveFsm, `true` when ContainsKey, `false` when neither.

---

### T-088-05 · Extraction: `PrepareAndRegisterRepairOrder`

| Field | Value |
|---|---|
| **ticket_id** | T-088-05 |
| **helper_name** | `PrepareAndRegisterRepairOrder` |
| **concern** | Reset `repairPos.BracketSubmitted = false`; write `entryOrders[repairEntryName] = repairEntry` |
| **lines_to_move** | ~4 (two mutation statements) |
| **cyc_reduction** | 0 from parent (pure mutation — no branches removed) |
| **projected_helper_cyc** | 1 |

**Signature:**
```csharp
private void PrepareAndRegisterRepairOrder(
    PositionInfo repairPos,
    string repairEntryName,
    Order repairEntry)
```

**Implementation Notes:**
- Purely extracts two state-mutation statements with no branching logic.
- Stale `entryOrders` on Submit failure (H3) is pre-existing and **out of scope** per V12.23 — document in docstring.
- `BracketSubmitted` mutation thread-safety (H4) is pre-existing — document thread-safety expectation in docstring.
- No lock blocks. xUnit test: verify `repairPos.BracketSubmitted == false` and `entryOrders[repairEntryName] == repairEntry` after call.

---

### T-088-06 · Extraction: `LogRepairOrderSubmitted`

| Field | Value |
|---|---|
| **ticket_id** | T-088-06 |
| **helper_name** | `LogRepairOrderSubmitted` |
| **concern** | Format and emit the success `Print(...)` message with `OrderType.Market` ternary for price display |
| **lines_to_move** | ~6 (formatted Print block including ternary price expression) |
| **cyc_reduction** | −1 from parent (removes orderType ternary branch) |
| **projected_helper_cyc** | 2 |

**Signature:**
```csharp
private void LogRepairOrderSubmitted(
    string accountName,
    string repairEntryName,
    OrderAction action,
    int quantity,
    OrderType orderType,
    PositionInfo repairPos)
```

**Implementation Notes:**
- Extract the formatted `Print(...)` block; string formatting using `LogBuffer.Format` is on success path only — no hot-path allocation concern.
- ASCII-only string content in all format templates.
- No lock blocks. xUnit test: verify print output contains accountName, repairEntryName, quantity for both Market and Limit order types.

---

### T-088-07 · Parent Reshape: `SubmitRepairOrderWithAuthorization`

| Field | Value |
|---|---|
| **ticket_id** | T-088-07 |
| **helper_name** | *(parent method — orchestrator reshape)* |
| **concern** | Replace all inlined logic in `SubmitRepairOrderWithAuthorization` with calls to T-088-01 through T-088-06 helpers; reduce to pure guard-gate → prepare → submit → log orchestration |
| **lines_to_move** | ~8 (replace ~87 lines of inlined logic with 8-line orchestration body) |
| **cyc_reduction** | −16 (21→5, completing the full cyc reduction from 34 to 5) |
| **projected_helper_cyc** | 5 (parent CYC after extraction) |

**Reshaping Notes:**
- **Dependency:** T-088-01 through T-088-06 must all be committed before this ticket executes.
- Replace inlined logic with calls in order: `TryResolveRepairAccount` → `CreateRepairOrder` → `HasActiveFsmForAccount` → `ResolveRepairAuthorization` → `MetadataGuardRepairAuthorized` → `PrepareAndRegisterRepairOrder` → `targetAcct.Submit` → `LogRepairOrderSubmitted`.
- Method signature **unchanged**: `private void SubmitRepairOrderWithAuthorization(string accountName, PositionInfo repairPos, string repairEntryName, OrderType orderType, double limitPrice, double stopPrice)`.
- 1 direct caller (`ExecuteReaperRepair` at line 246) — no caller changes needed.
- Build and run `dotnet csharpier check src/` after reshape.
- xUnit test: integration-level test verifying orchestration flow reaches Submit with valid inputs.

**Final Orchestration Body:**
```csharp
private void SubmitRepairOrderWithAuthorization(
    string accountName, PositionInfo repairPos, string repairEntryName,
    OrderType orderType, double limitPrice, double stopPrice)
{
    if (!TryResolveRepairAccount(repairPos, accountName, out Account targetAcct))
        return;
    if (!CreateRepairOrder(targetAcct, repairPos, orderType, limitPrice, stopPrice,
        repairEntryName, out Order repairEntry))
        return;
    bool hasActiveFsm = HasActiveFsmForAccount(accountName);
    if (!ResolveRepairAuthorization(accountName, hasActiveFsm))
        return;
    if (!MetadataGuardRepairAuthorized(accountName, "ExecuteReaperRepair"))
        return;
    PrepareAndRegisterRepairOrder(repairPos, repairEntryName, repairEntry);
    targetAcct.Submit(new[] { repairEntry });
    LogRepairOrderSubmitted(accountName, repairEntryName, repairEntry.OrderAction,
        repairEntry.Quantity, orderType, repairPos);
}
```

---

## CYC Reduction Trajectory

| After Ticket | Parent CYC | Delta |
|---|---|---|
| Baseline | 34 | — |
| T-088-01 | 33 | −1 |
| T-088-02 | 31 | −2 |
| T-088-03 | 26 | −5 |
| T-088-04 | 22 | −4 |
| T-088-05 | 22 | 0 |
| T-088-06 | 21 | −1 |
| **T-088-07** | **5** | **−16** |

**projected_parent_cyc_after_all: 5**

---

## Execution Order & Dependencies

```
T-088-01 (independent — add new private method, no usage yet)
T-088-02 (independent — add new private method, no usage yet)
T-088-03 (independent — add new private method, no usage yet)
T-088-04 (independent — add new private method, no usage yet)
T-088-05 (independent — add new private method, no usage yet)
T-088-06 (independent — add new private method, no usage yet)
T-088-07 (depends on T-088-01 through T-088-06 all committed — reshapes parent)
```

Tickets T-088-01 through T-088-06 are parallelizable (each adds a new method, no changes to existing code). T-088-07 is the integration step.

---

## Jane Street Alignment

| Principle | Status |
|---|---|
| All helper CYC ≤ 8 | ✅ max = 5 |
| Single-responsibility per extraction | ✅ each ticket has exactly one named concern |
| Lock-free / Actor pattern | ✅ zero new `lock()` blocks planned |
| Illegal states unrepresentable | ✅ `bool` return pattern prevents invalid state propagation |
| Zero-allocation hot paths | ✅ string formatting only in `LogRepairOrderSubmitted` (success path) |
| Scope creep prevention (V12.23) | ✅ all helpers `private` in same file; no cross-file changes |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | ~15 |
| **Execution Time** | 2026-06-29T03:30:00Z |
| **Wave** | 7 |
| **Phase** | 4 |
| **jcodemunch tools called** | resolve_repo |
| **sequential-thinking calls** | 4 |
| **ticket_count** | 7 |
| **projected_parent_cyc_after_all** | 5 |
| **cyc_reduction_pct** | 85.3% (34→5) |
