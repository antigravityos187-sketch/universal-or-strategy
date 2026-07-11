# EPIC-W7-015 — Phase 2: Architecture Plan

**Agent:** v12-phase2-architecture
**Wave:** 7
**Phase:** 2 — Architecture Planning
**Generated:** 2026-06-29T00:00:00Z
**Input:** docs/brain/EPIC-W7-015/01-scope-boundary.md

---

## Original Method

| Field            | Value |
|-----------------|-------|
| **Method Name** | `CancelAll_ProcessSingleFleetAccount` |
| **CYC (MCP-confirmed)** | 18 |
| **File**        | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| **Line Range**  | 300–343 |
| **Max Nesting** | 4 |
| **Lines**       | 44 |
| **Params**      | 2 (`Account acct`, `bool masterHasPosition`) |
| **Assessment**  | HIGH (CYC=18 exceeds Jane Street strict standard CYC<=8) |

---

## Source (MCP-retrieved)

```csharp
private int CancelAll_ProcessSingleFleetAccount(Account acct, bool masterHasPosition)
{
    int cancelled = 0;
    var acctFsms = _followerBrackets.Values.Where(f => f.AccountName == acct.Name).ToList();
    bool acctHasActiveFsm = acctFsms.Any(f => f.State == FollowerBracketState.Active);

    foreach (Order order in acct.Orders)
    {
        if (
            order != null
            && order.Instrument.FullName == Instrument.FullName
            && (
                order.OrderState == OrderState.Working
                || order.OrderState == OrderState.Accepted
                || order.OrderState == OrderState.Submitted
                || order.OrderState == OrderState.ChangePending
                || order.OrderState == OrderState.ChangeSubmitted
            )
        )
        {
            string oName = order.Name;
            if (
                oName.StartsWith("Stop_")
                || oName.StartsWith("S_")
                || oName.StartsWith("T1_")
                || oName.StartsWith("T2_")
                || oName.StartsWith("T3_")
                || oName.StartsWith("T4_")
                || oName.StartsWith("T5_")
            )
            {
                // Build 1104.1: Preserve brackets ONLY if FSM is active AND Master has position.
                // If Master is FLAT, orphaned follower brackets MUST be swept regardless of FSM state.
                if (acctHasActiveFsm && masterHasPosition)
                    continue;
            }

            CancelOrderOnAccount(order, acct);
            cancelled++;
        }
    }

    return cancelled;
}
```

---

## Callers

| Caller | File | Line | Resolution |
|--------|------|------|------------|
| `CancelAll_ProcessFleetOrders` | `src/V12_002.UI.IPC.Commands.Fleet.cs` | 275 | ast_resolved |
| `CancelAll_ProcessFleetAccounts` | `src/V12_002.UI.IPC.Commands.Fleet.cs` | 268 | ast_resolved |

**Both callers are upstream-only. Method signature is unchanged by this refactor.**

---

## Callees

| Callee | File | Resolution |
|--------|------|------------|
| `CancelOrderOnAccount` | `src/V12_002.Orders.CancelGateway.cs` | ast_inferred |

---

## Extraction Plan

| Helper Name | Signature | Responsibility | Lines Moved (approx) | Projected CYC |
|-------------|-----------|---------------|---------------------|---------------|
| `CancelAll_IsOrderEligibleForCancellation` | `private bool CancelAll_IsOrderEligibleForCancellation(Order order)` | Encapsulates all order eligibility checks: null guard, instrument match, and 5 OrderState OR conditions | ~11 lines (compound if, lines 308–319) | **8** |
| `CancelAll_IsBracketOrderName` | `private bool CancelAll_IsBracketOrderName(string orderName)` | Returns true if the order name starts with any of 7 protected bracket prefixes (Stop_, S_, T1_, T2_, T3_, T4_, T5_) | ~9 lines (StartsWith block, lines 322–330) | **8** |
| `CancelAll_ShouldPreserveBracketOrder` | `private bool CancelAll_ShouldPreserveBracketOrder(bool acctHasActiveFsm, bool masterHasPosition)` | Single guard: preserve bracket order only when FSM is active AND master has position | ~2 lines (lines 332–334) | **2** |

---

## Parent After Extraction

```csharp
private int CancelAll_ProcessSingleFleetAccount(Account acct, bool masterHasPosition)
{
    int cancelled = 0;
    var acctFsms = _followerBrackets.Values.Where(f => f.AccountName == acct.Name).ToList();
    bool acctHasActiveFsm = acctFsms.Any(f => f.State == FollowerBracketState.Active);

    foreach (Order order in acct.Orders)
    {
        if (!CancelAll_IsOrderEligibleForCancellation(order))
            continue;

        if (CancelAll_IsBracketOrderName(order.Name) && CancelAll_ShouldPreserveBracketOrder(acctHasActiveFsm, masterHasPosition))
            continue;

        CancelOrderOnAccount(order, acct);
        cancelled++;
    }

    return cancelled;
}
```

**Parent projected CYC:**

| Branch Point | Contribution |
|-------------|-------------|
| Base | +1 |
| LINQ `.Where` lambda predicate | +1 |
| LINQ `.Any` lambda predicate | +1 |
| `foreach` loop | +1 |
| `if (!CancelAll_IsOrderEligibleForCancellation)` continue | +1 |
| `if (CancelAll_IsBracketOrderName(...) && CancelAll_ShouldPreserveBracketOrder(...))` — `&&` compound | +2 |
| **TOTAL** | **7** |

---

## CYC Summary

| Unit | Projected CYC | Status |
|------|--------------|--------|
| `CancelAll_IsOrderEligibleForCancellation` | 8 | ✅ Pass (= threshold) |
| `CancelAll_IsBracketOrderName` | 8 | ✅ Pass (= threshold) |
| `CancelAll_ShouldPreserveBracketOrder` | 2 | ✅ Pass |
| `CancelAll_ProcessSingleFleetAccount` (parent) | 7 | ✅ Pass |

## max_cyc_projected: 8

**All extracted helpers and the refactored parent are within the Jane Street strict standard (CYC <= 8). VALIDATION PASS.**

---

## Jane Street Alignment Notes

| Principle | Application |
|-----------|-------------|
| **carl_cook: zero-alloc hot path** | The LINQ `.Where().ToList()` and `.Any()` on lines 301–303 are pre-loop setup (not in the hot inner loop). Acceptable at this refactoring stage. A follow-up epic may convert to explicit loops if profiling flags this. |
| **carl_cook: AggressiveInlining hot** | `CancelAll_IsOrderEligibleForCancellation` and `CancelAll_IsBracketOrderName` are small, deterministic, call-site-inlineable helpers — decorate with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`. |
| **carl_cook: NoInlining cold** | Not applicable — no cold logging paths in this method. |
| **carl_cook: avoid LINQ** | LINQ usage is pre-loop (setup only). Not in tight inner loop path. Flagged for future optimization. |
| **gjengset: no new lock() blocks** | No state mutations added. No lock() blocks introduced. Existing lock-free pattern preserved. |
| **gjengset: 64-byte cache line alignment** | No new structs or fields introduced. N/A. |
| **trading_billions: single responsibility per helper** | Each helper has exactly one job: eligibility check / bracket-name check / preserve guard. |
| **trading_billions: defense in depth** | Layered checks: eligibility → name type → preserve guard. Each layer independently verifiable. |
| **trading_billions: each helper CYC <= 8** | ✅ All helpers and parent confirm CYC <= 8 per validated calculation above. |
| **Build 1104.1 invariant preserved** | Comment and semantics of "preserve brackets ONLY if FSM is active AND Master has position" are fully preserved in `CancelAll_ShouldPreserveBracketOrder`. |

---

## MCP Evidence

| Tool | Query | Result |
|------|-------|--------|
| `resolve_repo` | `/home/malhitticrypto/universal-or-strategy` | Confirmed indexed; repo=`antigravityos187-sketch/universal-or-strategy`; 5147 symbols |
| `search_symbols` | `CancelAll_ProcessSingleFleetAccount` | Found at `src/V12_002.UI.IPC.Commands.Fleet.cs:300` |
| `get_symbol_complexity` | symbol_id `src/V12_002.UI.IPC.Commands.Fleet.cs::V12_002.CancelAll_ProcessSingleFleetAccount#method` | CYC=18, max_nesting=4, param_count=2, lines=44, assessment=HIGH |
| `get_symbol_source` | same symbol_id | Source retrieved lines 300–343 |
| `get_call_hierarchy` | same symbol_id, depth=2 | callers: `CancelAll_ProcessFleetOrders`, `CancelAll_ProcessFleetAccounts`; callees: `CancelOrderOnAccount` |
| `get_dependency_graph` | `src/V12_002.UI.IPC.Commands.Fleet.cs` | No cross-file imports/importers at depth=1; self-contained file |

---

## Sequential Thinking Evidence

### Thought 1 — Complexity Drivers

Enumerated all 18 branch points in the source:
- 1 foreach loop
- 1 null guard (`order != null`)
- 1 instrument match (`order.Instrument.FullName ==`)
- 5 OrderState OR conditions (Working, Accepted, Submitted, ChangePending, ChangeSubmitted)
- 7 bracket-prefix StartsWith OR conditions (Stop_, S_, T1_, T2_, T3_, T4_, T5_)
- 2 from `&&` compound in preserve-bracket guard (`acctHasActiveFsm && masterHasPosition`)

Total branch points = 18, confirming MCP-reported CYC=18.

Identified 3 distinct extraction targets: order eligibility block, bracket-name block, and preserve-guard block.

### Thought 2 — Extraction Strategy

Designed 3 private helpers:
- `CancelAll_IsOrderEligibleForCancellation(Order)` → CYC=8 (handles null + instrument + 5 states)
- `CancelAll_IsBracketOrderName(string)` → CYC=8 (handles 7 StartsWith ORs)
- `CancelAll_ShouldPreserveBracketOrder(bool, bool)` → CYC=2 (single && guard)

Parent retains: FSM setup (2 LINQ predicates) + foreach + 2 delegating if-continues = CYC=7.

Also noted LINQ .Where/.Any on pre-loop setup is acceptable per Jane Street guidance for non-hot-path setup; flagged for future zero-alloc follow-up epic if profiling flags.

### Thought 3 — CYC Validation

Performed branch-by-branch CYC tally for all 4 units:
- Helper 1: base(1) + null(1) + instrument(1) + 5×OrderState(5) = **8** ✅
- Helper 2: base(1) + 7×StartsWith(7) = **8** ✅
- Helper 3: base(1) + &&(1) = **2** ✅
- Parent: base(1) + .Where(1) + .Any(1) + foreach(1) + eligibility-if(1) + bracket&&guard-if(2) = **7** ✅

max_cyc_projected = **8**. All units <= 8. Validation PASS.

Confirmed `AggressiveInlining` recommended for Helpers 1 and 2 (small, deterministic, frequently called in inner loop).
Build 1104.1 semantic invariant fully preserved in extracted helper.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase2-architecture |
| **Bobcoins Used** | 4 |
| **Execution Time** | batch |
| **Phase** | 2 |
| **Wave** | 7 |
| **max_cyc_projected** | 8 |
| **boundary_verdict** | PASS (from Phase 1.5) |
