# EPIC-W7-058 — Phase 4: Ticket Generation

**Agent Name:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T04:00:00Z
**Inputs:** docs/brain/EPIC-W7-058/02-architecture-plan.md, docs/brain/EPIC-W7-058/03-audit-report.md

---

## Summary

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-058 |
| **Method** | `MapOrderStateToFSMState` |
| **File** | [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:469) |
| **CYC Baseline** | 13 (live index) / 34 (precomputed) |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 4 |
| **max_cyc_projected** | 5 |
| **dna_verdict (inherited)** | PASS |

---

## Sequential Thinking Evidence

### Thought 1 — Ticket Count Determination

CYC baseline = 13 (live index). The method's complexity is driven by two compound boolean OR guards:

1. `Filled || PartFilled` — 2-value OR inside the first `if` branch (CYC driver: +2)
2. `Working || Submitted || Initialized || ChangePending || ChangeSubmitted` — 5-value OR inside the third `else if` branch (CYC driver: +5)

The `Accepted` branch is a single equality check (CYC driver: +1) — no extraction needed.
The terminal `null` fallthrough is an `else` (CYC driver: +1) — no extraction needed.

Jane Street extract-predicate pattern applies: extract each compound OR guard as a `private static` predicate helper with `[MethodImpl(MethodImplOptions.AggressiveInlining)]`. This yields exactly **2 extraction helpers = 2 tickets**.

One ticket per extracted helper method. Each ticket has a single concern (V12.23 ONE EPIC = ONE CONCERN per ticket).

### Thought 2 — Per-Ticket Line and Name Analysis

**Ticket 1 — `IsActiveOrderState`:**
- Absorbs: `entryState == OrderState.Filled || entryState == OrderState.PartFilled`
- Taken from: parent's first `if` condition body
- New helper body: `s == OrderState.Filled || s == OrderState.PartFilled`
- Projected CYC of helper: **2** (1 base + 1 OR condition)
- Parent first branch becomes: `if (IsActiveOrderState(entryState)) return FollowerBracketState.Active;`

**Ticket 2 — `IsSubmittedOrderState`:**
- Absorbs: `entryState == OrderState.Working || entryState == OrderState.Submitted || entryState == OrderState.Initialized || entryState == OrderState.ChangePending || entryState == OrderState.ChangeSubmitted`
- Taken from: parent's third `else if` condition body
- New helper body: `s == OrderState.Working || s == OrderState.Submitted || s == OrderState.Initialized || s == OrderState.ChangePending || s == OrderState.ChangeSubmitted`
- Projected CYC of helper: **5** (1 base + 4 OR conditions)
- Parent third branch becomes: `if (IsSubmittedOrderState(entryState)) return FollowerBracketState.Submitted;`

Both helpers placed as `private static` methods immediately below `MapOrderStateToFSMState` in the same file. No cross-file changes. Parent signature unchanged.

### Thought 3 — CYC Validation (All Methods Post-Extraction)

| Method | CYC Formula | CYC | Passes <=8? |
|---|---|---|---|
| `MapOrderStateToFSMState` (refactored) | 1 base + 3 if-calls | 4 | **PASS** |
| `IsActiveOrderState` (Ticket-1) | 1 base + 1 OR condition | 2 | **PASS** |
| `IsSubmittedOrderState` (Ticket-2) | 1 base + 4 OR conditions | 5 | **PASS** |

**Max CYC projected across all methods: 5**
**Projected parent CYC after all tickets: 4**
All methods validated <= 8. ✅

---

## Ticket Definitions

---

### TICKET-1: Extract `IsActiveOrderState` predicate helper

| Field | Value |
|---|---|
| **Ticket ID** | EPIC-W7-058-T1 |
| **Concern** | Extract compound `Filled \|\| PartFilled` OR guard into private static predicate |
| **File** | [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:469) |
| **Parent Method** | `MapOrderStateToFSMState` |
| **Helper Name** | `IsActiveOrderState` |
| **Visibility** | `private static` |
| **Attribute** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| **CYC Before (parent)** | 13 |
| **CYC of New Helper** | 2 |
| **Projected Parent CYC After Ticket** | ~11 (full reduction only after T2 completes) |
| **Projected Parent CYC After ALL Tickets** | 4 |
| **Dependencies** | None — can execute standalone |

#### Implementation

**New helper to add immediately below `MapOrderStateToFSMState`:**

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsActiveOrderState(OrderState s) =>
    s == OrderState.Filled || s == OrderState.PartFilled;
```

**Parent first branch — BEFORE:**

```csharp
if (entryState == OrderState.Filled || entryState == OrderState.PartFilled)
{
    return FollowerBracketState.Active;
}
```

**Parent first branch — AFTER:**

```csharp
if (IsActiveOrderState(entryState))
{
    return FollowerBracketState.Active;
}
```

#### Verification Criteria

- [ ] `IsActiveOrderState` added as `private static` method with `[MethodImpl(AggressiveInlining)]`
- [ ] Parent first branch replaced with single `IsActiveOrderState(entryState)` call
- [ ] Build passes with zero errors
- [ ] `HydrateFSMsFromWorkingOrders` call site unchanged
- [ ] CYC of `IsActiveOrderState` <= 8 (expected: 2)

#### xUnit Tests (Jane Street Pattern)

```csharp
[Fact]
public void IsActiveOrderState_FilledReturnsTrue()
{
    Assert.True(IsActiveOrderState(OrderState.Filled));
}

[Fact]
public void IsActiveOrderState_PartFilledReturnsTrue()
{
    Assert.True(IsActiveOrderState(OrderState.PartFilled));
}

[Fact]
public void IsActiveOrderState_AcceptedReturnsFalse()
{
    Assert.False(IsActiveOrderState(OrderState.Accepted));
}
```

---

### TICKET-2: Extract `IsSubmittedOrderState` predicate helper

| Field | Value |
|---|---|
| **Ticket ID** | EPIC-W7-058-T2 |
| **Concern** | Extract 5-value compound OR guard into private static predicate |
| **File** | [`src/V12_002.SIMA.Lifecycle.cs`](src/V12_002.SIMA.Lifecycle.cs:469) |
| **Parent Method** | `MapOrderStateToFSMState` |
| **Helper Name** | `IsSubmittedOrderState` |
| **Visibility** | `private static` |
| **Attribute** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| **CYC Before (parent)** | 13 (or ~11 if T1 already applied) |
| **CYC of New Helper** | 5 |
| **Projected Parent CYC After Ticket** | 4 |
| **Projected Parent CYC After ALL Tickets** | 4 |
| **Dependencies** | None — can execute standalone or after T1 |

#### Implementation

**New helper to add immediately below `MapOrderStateToFSMState` (or below `IsActiveOrderState` if T1 applied first):**

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsSubmittedOrderState(OrderState s) =>
    s == OrderState.Working
    || s == OrderState.Submitted
    || s == OrderState.Initialized
    || s == OrderState.ChangePending
    || s == OrderState.ChangeSubmitted;
```

**Parent third branch — BEFORE:**

```csharp
else if (
    entryState == OrderState.Working
    || entryState == OrderState.Submitted
    || entryState == OrderState.Initialized
    || entryState == OrderState.ChangePending
    || entryState == OrderState.ChangeSubmitted
)
{
    return FollowerBracketState.Submitted;
}
```

**Parent third branch — AFTER:**

```csharp
if (IsSubmittedOrderState(entryState))
{
    return FollowerBracketState.Submitted;
}
```

#### Verification Criteria

- [ ] `IsSubmittedOrderState` added as `private static` method with `[MethodImpl(AggressiveInlining)]`
- [ ] Parent third branch replaced with single `IsSubmittedOrderState(entryState)` call
- [ ] Build passes with zero errors
- [ ] `HydrateFSMsFromWorkingOrders` call site unchanged
- [ ] CYC of `IsSubmittedOrderState` <= 8 (expected: 5)
- [ ] CYC of `MapOrderStateToFSMState` <= 8 (expected: 4 after both tickets)

#### xUnit Tests (Jane Street Pattern)

```csharp
[Fact]
public void IsSubmittedOrderState_WorkingReturnsTrue()
{
    Assert.True(IsSubmittedOrderState(OrderState.Working));
}

[Fact]
public void IsSubmittedOrderState_ChangePendingReturnsTrue()
{
    Assert.True(IsSubmittedOrderState(OrderState.ChangePending));
}

[Fact]
public void IsSubmittedOrderState_FilledReturnsFalse()
{
    Assert.False(IsSubmittedOrderState(OrderState.Filled));
}
```

---

## Final Refactored Parent (After Both Tickets)

```csharp
private FollowerBracketState? MapOrderStateToFSMState(OrderState entryState)
{
    if (IsActiveOrderState(entryState)) return FollowerBracketState.Active;
    if (entryState == OrderState.Accepted) return FollowerBracketState.Accepted;
    if (IsSubmittedOrderState(entryState)) return FollowerBracketState.Submitted;
    return null; // Terminal state - skip FSM creation
}
```

**CYC = 4** (1 base + 3 if-branches). All methods validated <= 8.

---

## Ticket Execution Order

| Ticket | Helper | CYC | Can Execute Standalone? |
|---|---|---|---|
| T1 | `IsActiveOrderState` | 2 | YES |
| T2 | `IsSubmittedOrderState` | 5 | YES |

T1 and T2 are fully independent — either can be applied first, or both applied together in a single commit by the Phase 5 engineer.

---

## Scope Compliance

| Check | Status |
|---|---|
| ONE EPIC = ONE CONCERN (V12.23) | PASS — only `MapOrderStateToFSMState` + 2 helpers |
| Caller `HydrateFSMsFromWorkingOrders` not modified | PASS |
| No sibling method modifications | PASS |
| No cross-file changes | PASS |
| Parent signature unchanged | PASS |
| All helpers `private static` (no blast radius) | PASS |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Epic** | EPIC-W7-058 |
| **Method** | MapOrderStateToFSMState |
| **CYC Baseline** | 13 (live index) / 34 (precomputed) |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 4 |
| **max_cyc_projected** | 5 |
| **dna_verdict** | PASS |
| **Status** | completed |
| **MCP Tools Used** | resolve_repo, get_symbol_complexity, get_extraction_candidates, sequentialthinking (3 thoughts) |
