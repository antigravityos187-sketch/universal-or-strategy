# Phase 1: Scope Definition — EPIC-W7-128

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Source Phase**: Phase 1 (Scope Definition)
- **Execution Time**: 2026-06-23T21:54:59Z

---

## Method Under Refactoring

| Attribute | Value |
|---|---|
| **Method** | `SymmetryGuardReplaceExistingFollowerTarget` |
| **File** | `src/V12_002.Symmetry.Replace.cs` |
| **Line** | 27 |
| **Current CYC** | 20 |
| **Target CYC** | ≤ 8 per extracted unit |
| **Signature** | `private void SymmetryGuardReplaceExistingFollowerTarget(string fleetEntryName, PositionInfo pos, int targetNumber, ConcurrentDictionary<string, Order> dict)` |
| **LOC** | 71 |
| **Max Nesting Depth** | 5 |
| **Sole Caller** | `SymmetryGuardRetargetExistingFollowerBracket` (line 17, same file) |

### Method Structure (as-read)

The method body falls into three distinct logical segments:

1. **Guard block A — null account** (lines 34–35): early return if `pos.ExecutingAccount == null`.
2. **Guard block B — stale-target cancellation** (lines 41–57): if the target is filled, a runner, or has zero quantity, cancel any live stale order in `dict` and remove it, then return.
3. **Replace block — FSM spec construction** (lines 59–96): if an `oldTarget` exists in `dict` and is in an active `OrderState`, compute the new price, build a `FollowerTargetReplaceSpec`, stamp reaper grace, and cancel the old order to trigger Phase 2 submission.

The CYC of 20 arises from:
- 4 `OrderState` comparisons in the stale-cancel branch (4 branches)
- 4 `OrderState` comparisons in the replace branch (4 branches)
- `isFilled || isRunner || qty <= 0` compound guard (3 branches)
- `newPrice <= 0` guard, `!dict.TryGetValue` guard, `dict.TryGetValue` in stale block, `staleTarget != null` null-check, outer `if` on `pos.ExecutingAccount`, `pos.Direction == MarketPosition.Long` ternary
- Totalling to 20 reachable paths

---

## IN SCOPE — Extractions Required

Three helper methods will be extracted to bring every leaf unit to CYC ≤ 8.

### Helper 1: `IsTargetSkippable`

**Purpose**: Encapsulate the compound skip-guard condition currently inlined at line 41.

**Proposed Signature**:
```csharp
private bool IsTargetSkippable(PositionInfo pos, int targetNumber, int qty)
```

**Logic moved**:
```csharp
bool isRunner = IsRunnerTarget(targetNumber);
bool isFilled = IsTargetFilled(pos, targetNumber);
return isFilled || isRunner || qty <= 0;
```

**CYC contribution isolated**: 3 (the compound OR expression).

---

### Helper 2: `CancelAndPurgeStaleDictEntry`

**Purpose**: Encapsulate the stale-order cancellation + dict removal that lives inside the skip branch (lines 43–55).

**Proposed Signature**:
```csharp
private void CancelAndPurgeStaleDictEntry(
    string fleetEntryName,
    PositionInfo pos,
    ConcurrentDictionary<string, Order> dict
)
```

**Logic moved**:
```csharp
if (dict.TryGetValue(fleetEntryName, out var staleTarget) && staleTarget != null)
{
    if (IsOrderCancellable(staleTarget))
        pos.ExecutingAccount.Cancel(new[] { staleTarget });
    dict.TryRemove(fleetEntryName, out _);
}
```

**CYC contribution isolated**: 3 (`TryGetValue` branch + null-check + `IsOrderCancellable` delegate).

---

### Helper 3: `IsOrderCancellable`

**Purpose**: Replace the four-way `OrderState` disjunction (repeated verbatim in both the stale-cancel block and the replace block) with a single named predicate. This pattern appears twice in the method and is the largest single contributor to CYC.

**Proposed Signature**:
```csharp
private static bool IsOrderCancellable(Order order)
```

**Logic moved**:
```csharp
return order.OrderState == OrderState.Working
    || order.OrderState == OrderState.Accepted
    || order.OrderState == OrderState.Submitted
    || order.OrderState == OrderState.ChangePending;
```

**CYC contribution isolated**: 4 (the four-way OR). Extracted as `static` because it has no instance-field dependencies.

---

### Resulting CYC Estimates After Extraction

| Unit | Residual CYC |
|---|---|
| `SymmetryGuardReplaceExistingFollowerTarget` (refactored) | ≤ 7 |
| `IsTargetSkippable` | 3 |
| `CancelAndPurgeStaleDictEntry` | 3 |
| `IsOrderCancellable` | 4 |

All units satisfy the ≤ 8 threshold. The main method residual CYC is driven by: null-account guard (1), `IsTargetSkippable` call gate (1), `!dict.TryGetValue` early return (1), `IsOrderCancellable` call gate (1), `newPrice <= 0` guard (1), `pos.Direction` ternary (1) = **6 branches → CYC 7**.

---

## OUT OF SCOPE

The following are explicitly excluded from this refactoring:

1. **Method signature unchanged** — `SymmetryGuardReplaceExistingFollowerTarget` keeps its exact four-parameter signature. No parameter is added, removed, or reordered.
2. **No behavior change** — All observable effects (cancellation calls, dict mutations, spec writes to `_followerTargetReplaceSpecs`, `StampReaperMoveGrace` timing, `pos.ExecutingAccount.Cancel` ordering) must be bit-for-bit identical to the original.
3. **No changes to callers** — `SymmetryGuardRetargetExistingFollowerBracket` (line 17) is not touched. Its five call sites remain unchanged.
4. **No changes to callees** — `IsRunnerTarget`, `IsTargetFilled`, `GetTargetContracts`, `GetTargetPrice`, `GetTargetMode`, `SymmetryTrim`, `StampReaperMoveGrace` are not modified.
5. **No refactoring of other methods** in `V12_002.Symmetry.Replace.cs` or any other file.
6. **No new public/internal API surface** — all three helpers are `private` (or `private static`), invisible outside the partial class.
7. **No performance changes** — No new allocations, no changed call ordering.
8. **No test changes** — Out of scope for Phase 1; test strategy is deferred to Phase 3.

---

## Extraction Plan

### Execution Order

Extractions must proceed in this order to avoid broken intermediate states:

```
Step 1 → Extract IsOrderCancellable (static predicate, no dependencies on other helpers)
Step 2 → Extract CancelAndPurgeStaleDictEntry (depends on IsOrderCancellable from Step 1)
Step 3 → Extract IsTargetSkippable (standalone, but extract last to keep skip-branch intact while Steps 1-2 are verified)
Step 4 → Inline the three helper calls into refactored SymmetryGuardReplaceExistingFollowerTarget body
```

### Placement in File

All three helpers are placed inside `#region Symmetry Replace`, immediately after `SymmetryGuardReplaceExistingFollowerTarget` (before `SymmetryGuardSkipFollower` at line 99). This keeps the coherent block together.

### Change Surface

| File | Change Type | Lines Affected |
|---|---|---|
| `src/V12_002.Symmetry.Replace.cs` | Refactor (in-file extraction) | Lines 27–97 (body replacement) + ~30 new lines for helpers |

No other file is touched.

---

## Risk Assessment

| Risk | Severity | Mitigation |
|---|---|---|
| `OrderState` 4-way check duplicated across two branches — incorrect deduplication would change cancellation behavior | HIGH | Extract verbatim with no simplification; helpers are call-through only |
| Dict mutation order (`Cancel` before `TryRemove`) must be preserved | MEDIUM | `CancelAndPurgeStaleDictEntry` preserves same sequential order |
| `StampReaperMoveGrace` must fire before `Cancel` in replace block | MEDIUM | Ordering stays in refactored main method body; not moved into a helper |
| `static` on `IsOrderCancellable` — must verify no future subclass or instance override needed | LOW | `OrderState` comparison has no instance state; `static` is correct |
| Partial-class split — helpers added here must not collide with names in other partial files | LOW | Names are unique; grepped codebase confirms no collision |
| Zero external blast radius (confirmed Phase 0) | ✅ NONE | All risk is file-local |

---

## Success Criteria

Phase 1 is complete when all of the following are true:

1. ✅ `00-scope.md` exists and documents the three helper methods with signatures, logic, and CYC estimates.
2. ✅ `manifest.json` phase `"1"` status is `"completed"` with output `"00-scope.md"`.

Phase 2 (implementation) succeeds when:

- [ ] `IsOrderCancellable` exists, is `private static`, has CYC ≤ 4.
- [ ] `CancelAndPurgeStaleDictEntry` exists, is `private`, has CYC ≤ 3.
- [ ] `IsTargetSkippable` exists, is `private`, has CYC ≤ 3.
- [ ] Refactored `SymmetryGuardReplaceExistingFollowerTarget` has CYC ≤ 8.
- [ ] The method signature is byte-for-byte identical to the original.
- [ ] No other method in the file is altered.
- [ ] Build produces zero new warnings or errors.
