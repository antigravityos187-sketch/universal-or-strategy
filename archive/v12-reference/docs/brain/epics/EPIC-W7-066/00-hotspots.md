# EPIC-W7-066 — Phase 0: Hotspot Analysis

## Symbol Under Analysis

| Field | Value |
|---|---|
| **Method** | `RemoveFsmOrderIdMappings` |
| **Source File** | `src/V12_002.Symmetry.BracketFSM.cs` |
| **Lines** | 103–125 |
| **Cyclomatic Complexity** | **10** |
| **Class** | `V12_002` (partial) |
| **Wave / Phase** | Wave 7 / Phase 0 |

---

## Method Body (Annotated)

```csharp
private void RemoveFsmOrderIdMappings(FollowerBracketFSM fsm)
{
    if (fsm == null)                                              // branch 1
        return;

    if (fsm.EntryOrder != null                                   // branch 2
        && !string.IsNullOrEmpty(fsm.EntryOrder.OrderId))        // branch 3
        _orderIdToFsmKey.TryRemove(fsm.EntryOrder.OrderId, out _);

    if (!string.IsNullOrEmpty(fsm.ReplacingCancelOrderId))       // branch 4
        _orderIdToFsmKey.TryRemove(fsm.ReplacingCancelOrderId, out _);

    if (fsm.StopOrder != null                                    // branch 5
        && !string.IsNullOrEmpty(fsm.StopOrder.OrderId))         // branch 6
        _orderIdToFsmKey.TryRemove(fsm.StopOrder.OrderId, out _);

    if (fsm.Targets == null)                                     // branch 7
        return;

    foreach (Order target in fsm.Targets)                       // branch 8 (loop)
    {
        if (target != null                                       // branch 9
            && !string.IsNullOrEmpty(target.OrderId))            // branch 10
            _orderIdToFsmKey.TryRemove(target.OrderId, out _);
    }
}
```

CYC = 10 is confirmed: 1 base + 9 decision points (null-check fsm, null+empty EntryOrder, null/empty ReplacingCancelOrderId, null+empty StopOrder, null Targets, loop header, null+empty target inside loop).

---

## Complexity Drivers

1. **Null-guard stacking** — Every field is guarded with both a null-check and an `IsNullOrEmpty` check on the nested `.OrderId` string, producing 2 branches per field (EntryOrder, StopOrder, each Target element).
2. **Five-element target loop** — The `foreach` over `fsm.Targets[0..4]` adds a loop-entry branch plus per-iteration null guards, the dominant CYC contributor.
3. **Early-exit guard on `Targets` array itself** — A separate `if (fsm.Targets == null) return;` creates an additional decision point distinct from the per-element null checks inside the loop.
4. **Asymmetric guard on `ReplacingCancelOrderId`** — Unlike `EntryOrder` and `StopOrder`, this field is a bare string (no containing object), so only `IsNullOrEmpty` is needed; however, it still adds a branch.

---

## Blast Radius

### Direct callers

| Caller | File | Context |
|---|---|---|
| `TryTerminateFollowerBracket` | `src/V12_002.Symmetry.BracketFSM.cs:135` | Sole direct caller; invoked after `_followerBrackets.TryRemove` |

### Transitive callers of `TryTerminateFollowerBracket`

| Caller | File | Line |
|---|---|---|
| `V12_002.REAPER.Audit` (×2) | `src/V12_002.REAPER.Audit.cs` | 418, 542 |
| `V12_002.Orders.Management.Cleanup` | `src/V12_002.Orders.Management.Cleanup.cs` | 83 |

### Shared state mutated

| Dictionary | Declared | Reads / Writes across codebase |
|---|---|---|
| `_orderIdToFsmKey` (`ConcurrentDictionary<string,string>`) | `src/V12_002.cs:836` | **15 write sites** across 6 files; **4 read sites** |
| `_followerBrackets` (`ConcurrentDictionary<string,FollowerBracketFSM>`) | `src/V12_002.cs:829` | **30+ read/write sites** across 12 files |

Key files that write to `_orderIdToFsmKey` (indexing side — must stay in sync with removals here):
- `src/V12_002.Orders.Callbacks.Propagation.cs:864,871`
- `src/V12_002.SIMA.Lifecycle.cs:563,593,712,724–760,841`
- `src/V12_002.SIMA.Fleet.cs:212`
- `src/V12_002.SIMA.Execution.cs:650`
- `src/V12_002.Symmetry.BracketFSM.cs:196,221,230,240` (Tier-2/3 backfill)

Files that **bypass `TryTerminateFollowerBracket`** and remove directly from `_followerBrackets` (leaving `_orderIdToFsmKey` stale):
- `src/V12_002.SIMA.Dispatch.cs:341,1437`
- `src/V12_002.SIMA.Fleet.cs:230,362`
- `src/V12_002.SIMA.Execution.cs:674`

> ⚠️ **Consistency risk**: At least 5 call sites remove from `_followerBrackets` directly without calling `RemoveFsmOrderIdMappings`, leaving dangling keys in `_orderIdToFsmKey`.

---

## Risk Assessment

| Dimension | Rating | Notes |
|---|---|---|
| **Complexity** | 🔴 High (CYC 10) | Exceeds warning threshold of 7 |
| **Blast radius** | 🟡 Medium | Single direct caller, but shared state spans 12 files |
| **Correctness risk** | 🔴 High | Bypass sites leave `_orderIdToFsmKey` stale; stale keys cause incorrect Tier-1 FSM resolution (`ResolveFsm_ByOrderId`) and potential ghost-FSM reads |
| **Testability** | 🟡 Medium | Pure state-mutation logic with no I/O, but requires constructing a fully-hydrated `FollowerBracketFSM` with up to 7 orders |
| **Thread safety** | 🟢 Low risk | All mutations use `ConcurrentDictionary.TryRemove` — individually atomic |

---

## Refactoring Opportunities (for Phase 1+)

1. **Extract `RemoveOrderIdIfPresent(Order order)` helper** — Eliminate the repeated `null + IsNullOrEmpty` guard pair by extracting a single-line helper. Reduces per-field branches from 2 to 1 logical unit.
2. **Consolidate bypass sites** — Route all `_followerBrackets.TryRemove` calls through `TryTerminateFollowerBracket` to close the stale-key gap.
3. **Null-coalesce target loop** — Replace the `foreach` null-guard with a LINQ `Where` pre-filter or a dedicated `RegisterTargetOrders`/`UnregisterTargetOrders` symmetric pair.

---

## Source References

- Method definition: [`RemoveFsmOrderIdMappings`](../../src/V12_002.Symmetry.BracketFSM.cs:103)
- Sole direct caller: [`TryTerminateFollowerBracket`](../../src/V12_002.Symmetry.BracketFSM.cs:127)
- State declarations: [`_followerBrackets`, `_orderIdToFsmKey`](../../src/V12_002.cs:829)
