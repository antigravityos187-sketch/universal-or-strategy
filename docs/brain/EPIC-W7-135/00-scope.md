# Phase 1: Scope Definition - EPIC-W7-135

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Epic ID**: EPIC-W7-135
- **Execution Mode**: v12-phase1-scope
- **Input**: 00-hotspots.md, manifest.json

---

## Method Under Refactoring

| Attribute        | Value                                    |
|------------------|------------------------------------------|
| **Method**       | `FindTargetOrderForPosition`             |
| **File**         | `src/V12_002.Trailing.Breakeven.cs`      |
| **Line**         | 186                                      |
| **Access**       | `private`                                |
| **Current CYC**  | 10                                       |
| **Target CYC**   | ≤ 8                                      |
| **Lines**        | 37 (lines 186–222)                       |
| **Parameters**   | 4 (`PositionInfo pos`, `string entryName`, `int targetNum`, `out string notFoundReason`) |

### Current Signature (FROZEN — must not change)
```csharp
private Order FindTargetOrderForPosition(
    PositionInfo pos,
    string entryName,
    int targetNum,
    out string notFoundReason
)
```

---

## Complexity Source Analysis

The static analyser (CYC = 10) counts the following decision points inside the method body:

| # | Decision Point | Location | CYC contribution |
|---|---------------|----------|-----------------|
| 1 | Baseline | — | +1 |
| 2 | `if (!pos.EntryFilled)` | line 195 | +1 |
| 3 | `foreach (Order order in searchAcct.Orders)` | line 206 | +1 |
| 4 | Outer `if (order != null && ...)` | line 208 | +1 |
| 5 | `order != null` (null check compound) | line 209 | +1 |
| 6 | `order.Name == targetOrderName` | line 210 | +1 |
| 7 | `order.Instrument.FullName == Instrument.FullName` | line 211 | +1 |
| 8 | `order.OrderState == OrderState.Working` | line 212 | +1 |
| 9 | `order.OrderState == OrderState.Accepted` (OR branch) | line 212 | +1 |
| 10 | Ternary `pos.IsFollower && pos.ExecutingAccount != null` | line 204 | +1 |

**Root cause**: The `&&`/`\|\|` compound predicate on the order-match `if` statement inflates CYC by 4 points. Extracting it into a named boolean helper reduces CYC by 4, bringing the parent method from 10 → 6 (comfortably ≤ 8).

---

## IN SCOPE — Extractions to Perform

### Extraction 1 — `IsOrderMatchForTarget`

**What it is**: The multi-clause boolean predicate that decides whether a single `Order` is the target order being sought.

**Lines extracted (approx)**: 208–213 (the `if` guard condition inside the `foreach`)

**Proposed signature**:
```csharp
private bool IsOrderMatchForTarget(Order order, string targetOrderName)
```

**Body (extracted from)**:
```csharp
return order != null
    && order.Name == targetOrderName
    && order.Instrument.FullName == Instrument.FullName
    && (order.OrderState == OrderState.Working
        || order.OrderState == OrderState.Accepted);
```

**CYC of extracted helper**: 4 (baseline 1 + 3 `&&`/`||` nodes)  
**CYC removed from parent**: 4 (the 4 compound-boolean decision points move out)  
**Parent CYC after extraction**: 10 − 4 = **6** ✅

### Extraction 2 — `ResolveSearchAccount` *(optional — only if CYC target still not met)*

**What it is**: The ternary expression that resolves which account to search.

**Lines extracted (approx)**: 204

**Proposed signature**:
```csharp
private Account ResolveSearchAccount(PositionInfo pos)
```

**Body**:
```csharp
return (pos.IsFollower && pos.ExecutingAccount != null)
    ? pos.ExecutingAccount
    : Account;
```

**Note**: Extraction 1 alone achieves CYC ≤ 8. `ResolveSearchAccount` is listed as a *secondary* extraction and should only be applied if the toolchain measures Extraction 1 as yielding fewer than 4 decision-point credits, or if code-review consensus prefers named intent over inline ternary.

---

## OUT OF SCOPE

| Item | Reason |
|------|--------|
| Method signature of `FindTargetOrderForPosition` | Frozen — single caller `MoveSpecificTarget` depends on it as-is |
| `out string notFoundReason` parameter | Part of frozen signature; assignment messages unchanged |
| Behaviour / logic changes | Pure structural extraction only; no semantics altered |
| `MoveSpecificTarget` (line 335) | Caller — not touched |
| `CalculateAndValidateNewTargetPrice` (line 225) | Different method — not touched |
| Any other method in `src/V12_002.Trailing.Breakeven.cs` | Only `FindTargetOrderForPosition` is in scope |
| Test files | Phase 1 is scope definition only; test authoring is a later phase concern |
| Build / CI pipeline | Not triggered during Phase 1 |

---

## Extraction Plan

```
Step 1  Read src/V12_002.Trailing.Breakeven.cs lines 186–222 (full method body)
Step 2  Extract compound boolean predicate (lines 208–213) into:
            private bool IsOrderMatchForTarget(Order order, string targetOrderName)
Step 3  Replace the original if-condition with:
            if (IsOrderMatchForTarget(order, targetOrderName))
Step 4  [Conditional] If CYC still > 8 after Step 3, extract ternary on line 204 into:
            private Account ResolveSearchAccount(PositionInfo pos)
Step 5  Verify:  FindTargetOrderForPosition CYC ≤ 8
                 IsOrderMatchForTarget CYC ≤ 8
                 Signature of FindTargetOrderForPosition identical to original
                 Caller MoveSpecificTarget (line 335) unchanged
```

### Proposed Helper Method Names

| Helper | Placement | Rationale |
|--------|-----------|-----------|
| `IsOrderMatchForTarget` | Immediately after `FindTargetOrderForPosition` (≈ line 223) | Cohesion — keep helpers adjacent to their sole consumer |
| `ResolveSearchAccount` | Immediately after `IsOrderMatchForTarget` (≈ line 232) | Secondary extraction, same cohesion principle |

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Signature change breaks `MoveSpecificTarget` caller | **None** | High | Signature explicitly frozen; only the internals restructure |
| Semantic drift in extracted predicate | Low | High | Extract verbatim — no rewrites, no logic reordering |
| `Instrument` / `Account` field accessibility from extracted helper | Low | Medium | Both are instance fields; helpers stay in same class, same access scope |
| Tool measures CYC differently after extraction | Low | Low | Secondary extraction (`ResolveSearchAccount`) available as fallback |
| Merge conflict with concurrent edits | Low | Medium | Single-file scope; check git status before Phase 2 edit |

**Overall Phase 1 Risk**: **LOW** — zero external callers, zero external dependents (blast radius 0.0 from Phase 0).

---

## Success Criteria

| Criterion | Measurable Pass Condition |
|-----------|--------------------------|
| CYC of `FindTargetOrderForPosition` | ≤ 8 (currently 10) |
| CYC of each extracted helper | ≤ 8 individually |
| Method signature unchanged | Byte-for-byte identical to frozen signature above |
| Caller unchanged | `MoveSpecificTarget` (line 335) source identical before/after |
| No other methods modified | `git diff` touches only: `FindTargetOrderForPosition`, `IsOrderMatchForTarget`, (optionally) `ResolveSearchAccount` |
| No behavioural change | Logical equivalence: extracted helpers are pure boolean/account expressions identical to original inline code |

---

## Phase 1 Completion Checklist

- ✅ Method under refactoring identified and quoted
- ✅ CYC source analysis completed (10 decision points enumerated)
- ✅ Primary extraction defined: `IsOrderMatchForTarget`
- ✅ Secondary extraction defined: `ResolveSearchAccount` (conditional)
- ✅ OUT OF SCOPE boundary drawn
- ✅ Extraction plan with ordered steps written
- ✅ Risk assessment completed
- ✅ Success criteria defined
- ✅ Ready for Phase 2 (Implementation)
