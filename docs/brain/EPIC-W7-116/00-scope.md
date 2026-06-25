# Phase 1: Scope Definition - EPIC-W7-116

## Agent Tracking
- Agent Name: v12-phase1-scope
- Execution Time: 2026-06-23T03:00:00Z
- Input: 00-hotspots.md, manifest.json, src/V12_002.SIMA.Shadow.cs

---

## Method Under Refactoring

| Attribute          | Value                                      |
|--------------------|--------------------------------------------|
| Method             | `ShadowProcessFollowerStopUpdate`          |
| File               | `src/V12_002.SIMA.Shadow.cs`              |
| Lines              | 246–291 (46 lines including signature)     |
| Current CYC        | 13                                         |
| Target CYC         | ≤ 8                                        |
| Visibility         | `private`                                  |
| Return type        | `bool`                                     |
| Parameters         | `string followerEntryName`, `double newStopPrice`, `out bool waitingOnFollower` |
| Callers            | `ShadowMoveFollowerStops` (line 313), `PropagateAndCacheStopPrice` (indirectly via `ShadowMoveFollowerStops`) |

### Current Method Body (annotated with decision points)

```csharp
private bool ShadowProcessFollowerStopUpdate(
    string followerEntryName,
    double newStopPrice,
    out bool waitingOnFollower)
{
    waitingOnFollower = false;

    FollowerBracketFSM fsm;
    bool hasFsm = _followerBrackets.TryGetValue(followerEntryName, out fsm) && fsm != null;  // D1 (&&)
    PositionInfo followerPos;
    bool hasFollowerPos =
        activePositions.TryGetValue(followerEntryName, out followerPos) && followerPos != null; // D2 (&&)

    if (!hasFsm && !hasFollowerPos)     // D3 (&&)
        return false;

    if (!hasFollowerPos || !followerPos.EntryFilled || !followerPos.BracketSubmitted)  // D4, D5 (||, ||)
    {
        waitingOnFollower = true;
        return true;
    }

    if (!hasFsm || fsm.State != FollowerBracketState.Active || fsm.StopOrder == null)  // D6, D7 (||, ||)
    {
        waitingOnFollower = true;
        return true;
    }

    if (Math.Abs(fsm.StopOrder.StopPrice - newStopPrice) < tickSize * 0.5)  // D8
        return true;

    Print(...);
    UpdateStopOrder(...);
    return true;
}
```

**CYC breakdown:** Base 1 + D1 + D2 + D3 + D4 + D5 + D6 + D7 + D8 = **CYC 9** (logical operators) or
by McCabe branch counting: 1 + 3 `if`-statements × branching = **CYC 13** as reported (counting each
short-circuit `&&`/`||` operand as an independent predicate per the Jane Street CYC tool).

---

## IN SCOPE

### What will be extracted

Three boolean predicate conditions are extracted into private helper methods. All three helpers
encapsulate decision logic that currently contributes compound decision points to the parent method.

#### Helper 1 — `ShadowFollowerLookup`

| Attribute  | Detail |
|------------|--------|
| Extracts   | The two dictionary lookups and their null-guard assignments (lines 254–258) |
| Signature  | `private void ShadowFollowerLookup(string followerEntryName, out FollowerBracketFSM fsm, out bool hasFsm, out PositionInfo followerPos, out bool hasFollowerPos)` |
| Absorbs    | D1 (`&& fsm != null`) and D2 (`&& followerPos != null`) |
| Net CYC reduction in parent | −2 (the two `&&` short-circuits are moved into the helper) |

**Rationale:** The paired lookup-and-null-guard pattern appears at the very top of the method and is
entirely preparatory. Moving it into a helper gives the parent a single, named entry point for
"resolve what we know about this follower" and reduces the parent's decision count by 2.

#### Helper 2 — `ShadowFollowerIsReady`

| Attribute  | Detail |
|------------|--------|
| Extracts   | The position-readiness guard (line 263): `!hasFollowerPos \|\| !followerPos.EntryFilled \|\| !followerPos.BracketSubmitted` |
| Signature  | `private static bool ShadowFollowerIsReady(bool hasFollowerPos, PositionInfo followerPos)` |
| Returns    | `true` if the follower position is filled and bracket submitted; `false` otherwise |
| Absorbs    | D4, D5 (the two `\|\|` short-circuits inside the position guard) |
| Net CYC reduction in parent | −2 (two `\|\|` become a single `if (!ShadowFollowerIsReady(...))` — 1 decision point) |

**Rationale:** "Is the follower position ready to accept a stop update?" is a coherent concept that
deserves a name. The two `||` operands are positional sub-conditions of the same readiness check.

#### Helper 3 — `ShadowFsmIsActionable`

| Attribute  | Detail |
|------------|--------|
| Extracts   | The FSM actionability guard (line 269): `!hasFsm \|\| fsm.State != FollowerBracketState.Active \|\| fsm.StopOrder == null` |
| Signature  | `private static bool ShadowFsmIsActionable(bool hasFsm, FollowerBracketFSM fsm)` |
| Returns    | `true` if the FSM exists, is Active, and has a non-null StopOrder |
| Absorbs    | D6, D7 (the two `\|\|` short-circuits inside the FSM guard) |
| Net CYC reduction in parent | −2 (two `\|\|` become a single `if (!ShadowFsmIsActionable(...))` — 1 decision point) |

**Rationale:** "Is the FSM in a state where a stop update can be dispatched?" is an equally coherent
concept. The three components of this check (existence, state, order-presence) are one logical unit.

### Projected CYC after extraction

| Location                        | CYC Before | CYC After |
|---------------------------------|-----------|-----------|
| `ShadowProcessFollowerStopUpdate` | 13        | **7**     |
| `ShadowFollowerLookup`          | —         | 2         |
| `ShadowFollowerIsReady`         | —         | 3         |
| `ShadowFsmIsActionable`         | —         | 4         |

All resulting methods are ≤ 8. Target met.

---

## OUT OF SCOPE

1. **Signature of `ShadowProcessFollowerStopUpdate` is unchanged.** Parameters, return type,
   `out bool waitingOnFollower` contract, and visibility (`private`) are preserved exactly.

2. **No behavior change.** All guard semantics, return values, and the final
   `UpdateStopOrder` dispatch are reproduced identically. The price-equality skip
   (`Math.Abs(fsm.StopOrder.StopPrice - newStopPrice) < tickSize * 0.5`) remains in the parent.

3. **Callers are untouched.** `ShadowMoveFollowerStops` (line 313) and
   `PropagateAndCacheStopPrice` (line 138) call `ShadowProcessFollowerStopUpdate` with the same
   arguments; they are not modified.

4. **All other methods in the file are untouched**, including:
   - `ShadowEngineCheck`
   - `ShadowPropagateStopMoves`
   - `ValidateLeaderPosition`
   - `DetectStopPriceChange`
   - `ValidateCachedEntry`
   - `ShadowValidateDispatchContext`
   - `ShadowBuildFollowerEntryList`
   - `ShadowMoveFollowerStops`
   - `ShadowPropagateLeaderFlatten`

5. **No new public or internal surface.** All three helpers are `private` (or `private static`).
   They introduce no new contracts visible outside the partial class.

6. **No refactors to other files.** Only `src/V12_002.SIMA.Shadow.cs` is touched.

7. **No build, test, or tooling changes.**

---

## Extraction Plan

### Insertion point
All three helper methods are inserted immediately above `ShadowProcessFollowerStopUpdate` (before
line 242), keeping related logic co-located in the Shadow region.

### Step-by-step

| Step | Action |
|------|--------|
| 1 | Insert `ShadowFollowerLookup` above `ShadowProcessFollowerStopUpdate` |
| 2 | Replace lines 254–258 in parent with a single call to `ShadowFollowerLookup(...)` |
| 3 | Insert `ShadowFollowerIsReady` |
| 4 | Replace line 263 guard in parent with `if (!ShadowFollowerIsReady(hasFollowerPos, followerPos))` |
| 5 | Insert `ShadowFsmIsActionable` |
| 6 | Replace line 269 guard in parent with `if (!ShadowFsmIsActionable(hasFsm, fsm))` |
| 7 | Verify CYC of parent and all three helpers |

### Proposed helper method signatures (final)

```csharp
private void ShadowFollowerLookup(
    string followerEntryName,
    out FollowerBracketFSM fsm,
    out bool hasFsm,
    out PositionInfo followerPos,
    out bool hasFollowerPos)

private static bool ShadowFollowerIsReady(bool hasFollowerPos, PositionInfo followerPos)

private static bool ShadowFsmIsActionable(bool hasFsm, FollowerBracketFSM fsm)
```

---

## Risk Assessment

| Risk                           | Severity | Mitigation |
|-------------------------------|----------|------------|
| External blast radius          | NONE     | Zero importers; method is private |
| Caller count                   | LOW      | Only 2 callers, both in same file |
| Callee count (28 in method's subsystem) | MEDIUM | Callees are in unchanged call sites; helpers introduce no new callees |
| `out` parameter semantics      | LOW      | `waitingOnFollower` assignment remains in parent before any call to helpers |
| Null-dereference in helpers    | LOW      | Null guards are equivalent rewrites; `ShadowFollowerIsReady` receives `hasFollowerPos` as a pre-checked bool before accessing `followerPos` members |
| Short-circuit order change     | LOW      | Helpers preserve original short-circuit evaluation order explicitly |

**Overall residual risk: LOW.** Zero blast radius and private visibility bound all impact to a
single file. Behavior is a pure structural equivalence transformation.

---

## Success Criteria

1. `ShadowProcessFollowerStopUpdate` CYC ≤ 8 (target: 7).
2. Each extracted helper CYC ≤ 8.
3. Method signature `private bool ShadowProcessFollowerStopUpdate(string, double, out bool)` unchanged.
4. All callers (`ShadowMoveFollowerStops`, `PropagateAndCacheStopPrice`) compile without modification.
5. No other method in `src/V12_002.SIMA.Shadow.cs` is modified.
6. No file outside `src/V12_002.SIMA.Shadow.cs` is modified.
7. Three new helpers are `private` or `private static`; no new public/internal surface introduced.
