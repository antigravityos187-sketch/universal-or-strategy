# EPIC-W7-015 — Phase 4: Ticket Definitions

**Agent:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T00:00:00Z
**Input:** docs/brain/EPIC-W7-015/02-architecture-plan.md + docs/brain/EPIC-W7-015/03-audit-report.md

---

## Method Under Refactor

| Field | Value |
|-------|-------|
| **Method Name** | `CancelAll_ProcessSingleFleetAccount` |
| **File** | `src/V12_002.UI.IPC.Commands.Fleet.cs` |
| **Line Range** | 300–343 |
| **CYC (MCP-confirmed)** | 18 (HIGH) |
| **Assessment** | HIGH — exceeds Jane Street strict standard CYC<=8 |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 7 |

---

## Ticket Summary

| Ticket | Helper Name | Concern | Lines | CYC Removed | Helper CYC |
|--------|-------------|---------|-------|-------------|------------|
| T1 | `CancelAll_IsOrderEligibleForCancellation` | Order eligibility: null + instrument + 5×OrderState | ~11 (308–319) | 7 | 8 |
| T2 | `CancelAll_IsBracketOrderName` | Bracket name check: 7×StartsWith OR | ~9 (322–330) | 7 | 8 |
| T3 | `CancelAll_ShouldPreserveBracketOrder` | Preserve guard: FSM-active && master-has-position | ~2 (332–334) | 1 | 2 |

---

## Ticket T1 — CancelAll_IsOrderEligibleForCancellation

| Field | Value |
|-------|-------|
| **ticket_id** | T1 |
| **helper_name** | `CancelAll_IsOrderEligibleForCancellation` |
| **concern** | Encapsulates all order eligibility checks: null guard, instrument full-name match, and 5-way OrderState OR (Working, Accepted, Submitted, ChangePending, ChangeSubmitted) |
| **lines_to_move** | Compound if-guard at lines 308–319: `order != null && order.Instrument.FullName == Instrument.FullName && (order.OrderState == OrderState.Working || ... || OrderState.ChangeSubmitted)` (~11 lines) |
| **cyc_reduction** | 7 branches removed from parent (null-guard + instrument-match + 5×OrderState OR) |
| **projected_helper_cyc** | 8 (base:1 + null:1 + instrument:1 + 5×OrderState:5 = 8) ✅ |
| **jane_street_annotation** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — small deterministic helper, called on every order in inner loop |

### Target Signature

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool CancelAll_IsOrderEligibleForCancellation(Order order)
{
    return order != null
        && order.Instrument.FullName == Instrument.FullName
        && (
            order.OrderState == OrderState.Working
            || order.OrderState == OrderState.Accepted
            || order.OrderState == OrderState.Submitted
            || order.OrderState == OrderState.ChangePending
            || order.OrderState == OrderState.ChangeSubmitted
        );
}
```

### CYC Branch Tally

| Branch Point | Contribution |
|-------------|-------------|
| Base | +1 |
| `order != null` null guard | +1 |
| `order.Instrument.FullName ==` instrument match | +1 |
| `OrderState.Working` | +1 |
| `OrderState.Accepted` | +1 |
| `OrderState.Submitted` | +1 |
| `OrderState.ChangePending` | +1 |
| `OrderState.ChangeSubmitted` | +1 |
| **TOTAL** | **8** ✅ |

### Acceptance Criteria

- [ ] Helper exists as private method in `src/V12_002.UI.IPC.Commands.Fleet.cs`
- [ ] Helper returns correct bool for all 5 working states
- [ ] Helper returns false for null order
- [ ] Helper returns false for instrument mismatch
- [ ] Parent calls `if (!CancelAll_IsOrderEligibleForCancellation(order)) continue;`
- [ ] `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute present
- [ ] Build passes: `dotnet build` zero errors
- [ ] xUnit test: `[Fact]` covers null, wrong instrument, each valid OrderState

---

## Ticket T2 — CancelAll_IsBracketOrderName

| Field | Value |
|-------|-------|
| **ticket_id** | T2 |
| **helper_name** | `CancelAll_IsBracketOrderName` |
| **concern** | Returns true if the order name starts with any of the 7 protected bracket prefixes: `Stop_`, `S_`, `T1_`, `T2_`, `T3_`, `T4_`, `T5_` |
| **lines_to_move** | The 7-way StartsWith OR block at lines 322–330 (~9 lines): `oName.StartsWith("Stop_") \|\| oName.StartsWith("S_") \|\| oName.StartsWith("T1_") \|\| ... \|\| oName.StartsWith("T5_")` |
| **cyc_reduction** | 7 branches removed from parent (7×StartsWith OR conditions) |
| **projected_helper_cyc** | 8 (base:1 + 7×StartsWith OR:7 = 8) ✅ |
| **jane_street_annotation** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` — small deterministic helper, called on every matching order in inner loop |

### Target Signature

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private bool CancelAll_IsBracketOrderName(string orderName)
{
    return orderName.StartsWith("Stop_")
        || orderName.StartsWith("S_")
        || orderName.StartsWith("T1_")
        || orderName.StartsWith("T2_")
        || orderName.StartsWith("T3_")
        || orderName.StartsWith("T4_")
        || orderName.StartsWith("T5_");
}
```

### CYC Branch Tally

| Branch Point | Contribution |
|-------------|-------------|
| Base | +1 |
| `StartsWith("Stop_")` | +1 |
| `StartsWith("S_")` | +1 |
| `StartsWith("T1_")` | +1 |
| `StartsWith("T2_")` | +1 |
| `StartsWith("T3_")` | +1 |
| `StartsWith("T4_")` | +1 |
| `StartsWith("T5_")` | +1 |
| **TOTAL** | **8** ✅ |

### Acceptance Criteria

- [ ] Helper exists as private method in `src/V12_002.UI.IPC.Commands.Fleet.cs`
- [ ] Returns true for all 7 bracket prefixes: `Stop_`, `S_`, `T1_`, `T2_`, `T3_`, `T4_`, `T5_`
- [ ] Returns false for non-bracket order names (e.g., `Entry_`, `Market_`)
- [ ] `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute present
- [ ] Build passes: `dotnet build` zero errors
- [ ] xUnit test: `[Fact]` covers all 7 matching prefixes and at least 2 non-matching names

---

## Ticket T3 — CancelAll_ShouldPreserveBracketOrder

| Field | Value |
|-------|-------|
| **ticket_id** | T3 |
| **helper_name** | `CancelAll_ShouldPreserveBracketOrder` |
| **concern** | Encodes the Build 1104.1 invariant: preserve a bracket order ONLY when the FSM is active AND the master account has an open position. If master is flat, orphaned follower brackets MUST be swept regardless of FSM state. |
| **lines_to_move** | Preserve-bracket guard at lines 332–334 (~2 lines): `if (acctHasActiveFsm && masterHasPosition) continue;` — the `&&` compound condition extracted as named predicate |
| **cyc_reduction** | 1 branch removed from parent (the `&&` compound removed from parent's inline expression; parent retains the outer delegating `if` for the bracket block as 2 branches per architecture plan) |
| **projected_helper_cyc** | 2 (base:1 + &&:1 = 2) ✅ |
| **jane_street_annotation** | None required — trivial 2-branch predicate; compiler will inline automatically |

### Target Signature

```csharp
// Build 1104.1: Preserve brackets ONLY if FSM is active AND Master has position.
// If Master is FLAT, orphaned follower brackets MUST be swept regardless of FSM state.
private bool CancelAll_ShouldPreserveBracketOrder(bool acctHasActiveFsm, bool masterHasPosition)
{
    return acctHasActiveFsm && masterHasPosition;
}
```

### CYC Branch Tally

| Branch Point | Contribution |
|-------------|-------------|
| Base | +1 |
| `acctHasActiveFsm && masterHasPosition` | +1 |
| **TOTAL** | **2** ✅ |

### Acceptance Criteria

- [ ] Helper exists as private method in `src/V12_002.UI.IPC.Commands.Fleet.cs`
- [ ] Build 1104.1 comment preserved verbatim above the method
- [ ] Returns true only when both `acctHasActiveFsm=true` AND `masterHasPosition=true`
- [ ] Returns false when either or both are false (FSM inactive, or master flat)
- [ ] Build passes: `dotnet build` zero errors
- [ ] xUnit test: `[Fact]` covers all 4 boolean combinations (TT, TF, FT, FF)

---

## Parent After All Extractions

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

### Parent CYC Branch Tally

| Branch Point | Contribution |
|-------------|-------------|
| Base | +1 |
| LINQ `.Where` lambda predicate | +1 |
| LINQ `.Any` lambda predicate | +1 |
| `foreach` loop | +1 |
| `if (!CancelAll_IsOrderEligibleForCancellation)` continue | +1 |
| `if (IsBracketName && ShouldPreserve)` — `&&` compound | +2 |
| **TOTAL** | **7** ✅ |

---

## CYC Summary

| Unit | Original CYC | Projected CYC | Delta | Status |
|------|-------------|--------------|-------|--------|
| `CancelAll_ProcessSingleFleetAccount` (parent) | 18 | 7 | -11 | ✅ Pass |
| `CancelAll_IsOrderEligibleForCancellation` | — | 8 | new | ✅ Pass (= threshold) |
| `CancelAll_IsBracketOrderName` | — | 8 | new | ✅ Pass (= threshold) |
| `CancelAll_ShouldPreserveBracketOrder` | — | 2 | new | ✅ Pass |
| **projected_parent_cyc_after_all** | | **7** | | ✅ |
| **max_cyc_projected** | | **8** | | ✅ |

**CYC reduction on parent: 18 → 7 = -11 (61.1% reduction)**

---

## Sequential Thinking Evidence

### Thought 1 — Ticket Count Decision

Method CYC=18 (MCP-confirmed). Architecture plan identifies 3 distinct logical concerns: order eligibility (compound null+instrument+5×state), bracket name detection (7×StartsWith), and preserve guard (&&). One ticket = one extracted helper = one concern. **ticket_count = 3**.

### Thought 2 — Line Mapping per Ticket

- T1: Lines 308–319 (~11 lines) → `CancelAll_IsOrderEligibleForCancellation(Order)` → removes 7 branches from parent → helper CYC=8
- T2: Lines 322–330 (~9 lines) → `CancelAll_IsBracketOrderName(string)` → removes 7 branches from parent → helper CYC=8
- T3: Lines 332–334 (~2 lines) → `CancelAll_ShouldPreserveBracketOrder(bool, bool)` → removes && from parent → helper CYC=2
- Parent retains: base + 2 LINQ predicates + foreach + 2 delegating ifs = **7**

### Thought 3 — CYC Verification

Branch-by-branch tally confirms all 4 units <= 8:
- Helper 1: 8 ✅ | Helper 2: 8 ✅ | Helper 3: 2 ✅ | Parent: 7 ✅
- max_cyc_projected = 8. **VERIFICATION PASS.**
- Build 1104.1 invariant preserved in T3. AggressiveInlining on T1, T2.

---

## MCP Evidence

| Tool | Result |
|------|--------|
| `resolve_repo` | repo=`antigravityos187-sketch/universal-or-strategy`, 5147 symbols, indexed |
| `get_symbol_complexity` | CYC=18, max_nesting=4, param_count=2, lines=44, assessment=high |
| `get_extraction_candidates` | candidates=[] (expected — helpers are new; no multi-caller existing blocks) |
| `sequentialthinking` (3 thoughts) | ticket_count=3 validated, all CYC<=8 confirmed |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 5 |
| **Execution Time** | batch |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic ID** | EPIC-W7-015 |
| **ticket_count** | 3 |
| **projected_parent_cyc_after_all** | 7 |
| **max_cyc_projected** | 8 |
| **dna_verdict_inherited** | PASS (from Phase 3) |
| **sequential_thinking_thoughts** | 3 |
