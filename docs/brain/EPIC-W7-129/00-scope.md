# Phase 1: Scope Definition - EPIC-W7-129

## Agent Tracking
- Agent Name: v12-phase1-scope
- Bobcoins Used: 0.0
- API Key: jCodemunch MCP
- Execution Time: 2026-06-23T21:55:46Z

---

## Method Under Refactoring

| Property          | Value                                                          |
|-------------------|----------------------------------------------------------------|
| Method            | `SymmetryGuardTryResolveFollowersForDispatch`                  |
| File              | `src/V12_002.Symmetry.Replace.cs`                              |
| Line              | 134                                                            |
| Signature         | `private void SymmetryGuardTryResolveFollowersForDispatch(string dispatchId, DateTime nowUtc)` |
| Current CYC       | 16                                                             |
| Target CYC        | ≤ 8 per method                                                 |
| Lines of Code     | 58                                                             |
| Max Nesting Depth | 4                                                              |

The method orchestrates follower-resolution for a single dispatch ID across three distinct
logical phases:

1. **Snapshot scan** — walk `ctx.Followers` (immutable `string[]` snapshot) and collect
   fleet entry names that are linked to this dispatch and have a pending fill (lines 141–160).
2. **Fallback scan** — walk `symmetryPendingFollowerFills` directly to catch any followers
   absent from the local snapshot (lines 163–174).
3. **Resolution loop** — for each collected fleet entry name, read `activePositions`,
   and call `SymmetryGuardTryResolveFollower`; on success, remove from pending map
   (lines 176–190).

---

## IN SCOPE — Extractions

Three private helper methods will be extracted. Together they reduce the orchestrator's
cyclomatic complexity to ≤ 4 and keep each helper at ≤ 8.

### 1. `SymmetryGuardCollectFollowersFromSnapshot`

**Responsibility:** Populate `followersToResolve` from the immutable `ctx.Followers`
snapshot (ADR-019 lock-free path).

**Extracted logic:** Lines 145–159 (the `foreach (string fleetEntryName in followerSnapshot)` block).

**Proposed signature:**
```csharp
private void SymmetryGuardCollectFollowersFromSnapshot(
    string dispatchId,
    string[] followerSnapshot,
    List<string> followersToResolve)
```

**Estimated CYC of extracted method:** 5  
(guard `IsNullOrEmpty`, `TryGetValue` miss, `Equals` mismatch, `ContainsKey` miss, `Add`)

---

### 2. `SymmetryGuardCollectFollowersFromPendingMap`

**Responsibility:** Populate `followersToResolve` with any fleet entries present in
`symmetryPendingFollowerFills` that are linked to `dispatchId` but were not already added
by the snapshot scan (ADR-019 legacy fallback path).

**Extracted logic:** Lines 163–174 (the `foreach (var kvp in symmetryPendingFollowerFills.ToArray())` block).

**Proposed signature:**
```csharp
private void SymmetryGuardCollectFollowersFromPendingMap(
    string dispatchId,
    List<string> followersToResolve)
```

**Estimated CYC of extracted method:** 4  
(`TryGetValue` miss, `Equals` mismatch, `Contains` duplicate guard, `Add`)

---

### 3. `SymmetryGuardResolveCollectedFollowers`

**Responsibility:** Iterate the final `followersToResolve` worklist, read `activePositions`,
gate on `pos.IsFollower`, call `SymmetryGuardTryResolveFollower`, and remove successful
entries from `symmetryPendingFollowerFills`.

**Extracted logic:** Lines 176–190 (the resolution `foreach` loop).

**Proposed signature:**
```csharp
private void SymmetryGuardResolveCollectedFollowers(
    List<string> followersToResolve,
    DateTime nowUtc)
```

**Estimated CYC of extracted method:** 4  
(`TryGetValue` miss, `pos != null`, `pos.IsFollower`, `TryResolve` result branch)

---

### Orchestrator After Extraction

After the three extractions the orchestrator body reduces to:

```csharp
private void SymmetryGuardTryResolveFollowersForDispatch(string dispatchId, DateTime nowUtc)
{
    if (string.IsNullOrEmpty(dispatchId))
        return;

    var followersToResolve = new List<string>();

    if (symmetryDispatchById.TryGetValue(dispatchId, out var ctx) && ctx != null)
        SymmetryGuardCollectFollowersFromSnapshot(dispatchId, ctx.Followers, followersToResolve);

    SymmetryGuardCollectFollowersFromPendingMap(dispatchId, followersToResolve);
    SymmetryGuardResolveCollectedFollowers(followersToResolve, nowUtc);
}
```

**Estimated orchestrator CYC after extraction:** 3  
(guard `IsNullOrEmpty`, `TryGetValue + ctx != null`, sequential calls)

---

## OUT OF SCOPE

| Item                                                              | Reason                                                  |
|-------------------------------------------------------------------|---------------------------------------------------------|
| Public/internal signature of the orchestrator method              | Unchanged — `private void (string, DateTime)` preserved |
| Behavior and observable side-effects                              | Zero behavior change; pure structural decomposition      |
| All other methods in `src/V12_002.Symmetry.Replace.cs`            | Not touched                                              |
| All methods in `src/V12_002.Symmetry.Follower.cs`                 | Callees remain as-is                                     |
| All methods in `src/V12_002.Symmetry.cs`                          | Shared state fields accessed by ref, not modified        |
| `SymmetryGuardSkipFollower` (line 99, same file)                  | Not called by this method; untouched                     |
| `SymmetryGuardRetargetExistingFollowerBracket` (line 17)          | Indirect callee via `SymmetryGuardTryResolveFollower`; untouched |
| ADR-019 lock-free contract                                        | Snapshot read pattern preserved verbatim in extraction   |
| V12.Phase8 [F-04] `stateLock` comment                            | Comment migrates with code; no semantic change           |
| Unit tests                                                        | Test scaffolding deferred to Phase 2 (implementation)    |
| Performance characteristics                                       | No allocations added beyond existing `List<string>`      |

---

## Extraction Plan

```
Step 1 — Extract SymmetryGuardCollectFollowersFromSnapshot
  Source: lines 145–159 of orchestrator
  Move to: same partial class (V12_002.Symmetry.Replace.cs), immediately below orchestrator
  Access to: symmetryFleetEntryToDispatch, symmetryPendingFollowerFills (read-only access, passed as parameters OR accessed via instance fields — same file, same class)

Step 2 — Extract SymmetryGuardCollectFollowersFromPendingMap
  Source: lines 163–174 of orchestrator
  Move to: same file, below Step 1 helper
  Access to: symmetryPendingFollowerFills (read via ToArray()), symmetryFleetEntryToDispatch

Step 3 — Extract SymmetryGuardResolveCollectedFollowers
  Source: lines 176–190 of orchestrator
  Move to: same file, below Step 2 helper
  Access to: symmetryPendingFollowerFills (TryRemove write), activePositions, SymmetryGuardTryResolveFollower

Step 4 — Replace orchestrator body
  Retain guard clause; replace three block sections with three helper calls
  Verify: all three callee symbols compile; no new variables escape scope
```

All four steps are in a single file. No cross-file changes are required.

---

## Risk Assessment

| Risk                                       | Severity | Likelihood | Mitigation                                            |
|--------------------------------------------|----------|------------|-------------------------------------------------------|
| Variable escape — `followersToResolve` captured by wrong scope | Low | Very low | Passed as parameter; value-type semantics unchanged |
| `ctx.Followers` snapshot aliasing          | Low      | Very low   | Array reference passed verbatim; ADR-019 contract intact |
| `symmetryPendingFollowerFills.TryRemove` write in extracted method | Low | Very low | Same instance field; method stays in same class |
| Merge conflict with concurrent PR          | Low      | Low        | Method has zero external callers; blast radius = 0    |
| CYC budget overshoot in extracted helpers  | Low      | Low        | Each extracted block has ≤ 5 independent branches      |

**Overall refactoring risk: LOW** — consistent with Phase 0 blast-radius finding.

---

## Success Criteria

1. **CYC ≤ 8** for every method produced (orchestrator + 3 helpers). Target: orchestrator = 3, helpers ≤ 5.
2. **Signature unchanged** — `private void SymmetryGuardTryResolveFollowersForDispatch(string dispatchId, DateTime nowUtc)` is byte-identical after refactoring.
3. **No behavior change** — all branches in the original method are present in the extracted helpers with identical guard ordering.
4. **ADR-019 comment preserved** — the lock-free snapshot note migrates with `SymmetryGuardCollectFollowersFromSnapshot`.
5. **V12.Phase8 [F-04] comment preserved** — the `stateLock` guard note migrates with `SymmetryGuardResolveCollectedFollowers`.
6. **Zero new public/internal surface** — all three helpers are `private`.
7. **Single file change** — only `src/V12_002.Symmetry.Replace.cs` is modified.
8. **Build passes** with no new warnings (validated in Phase 2).

---

Phase 1 Status: COMPLETED  
Generated: 2026-06-23T21:55:46Z
