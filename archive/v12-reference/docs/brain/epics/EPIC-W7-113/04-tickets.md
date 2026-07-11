# EPIC-W7-113 — Phase 4 Tickets

**Method**: `HydrateFSMsFromWorkingOrders`
**Source**: `src/V12_002.SIMA.Lifecycle.cs` (lines 787–891, 104 lines)
**CYC**: 0 (parse artefact — estimated true CYC = 12 from architecture plan manual count)
**Lane**: P4-L7
**DNA Verdict**: PASS
**Max CYC Projected**: 6 (after extraction)

---

## Ticket Summary

| # | Ticket | Type | CYC Target | Depends On |
|---|--------|------|------------|------------|
| 1 | Extract `TryGetEntryPassCandidate` | extraction | ≤6 | — |
| 2 | Extract `LinkStopOrderToFSM` | extraction | ≤3 | — |
| 3 | Extract `RunEntryOrderPass` | extraction | ≤4 (parent → 1) | Ticket 1, Ticket 2 |

---

## Ticket 1 — Extract `TryGetEntryPassCandidate`

**Type**: extraction
**Target CYC**: ≤6
**Source file**: `src/V12_002.SIMA.Lifecycle.cs`
**Source region**: lines 787–891 (entry-order foreach loop guard block)
**Depends on**: none

### Goal

Extract the 5 guard-continue branches (B1–B5) from the top of the entry-order `foreach` loop body in `HydrateFSMsFromWorkingOrders` into a new private helper method `TryGetEntryPassCandidate`. These 5 branches currently make the loop body unreadable and inflate the parent method's CYC.

### Signature

```csharp
private bool TryGetEntryPassCandidate(
    string entryKey,
    Order entryOrder,
    out PositionInfo pi)
```

### Extracted Branches (B1–B5)

| Branch | Guard | Action |
|--------|-------|--------|
| B1 | `entryOrder == null` | return false |
| B2 | account is master (not follower) | return false |
| B3 | `entryOrder.ExecutingAccount == null` | return false |
| B4 | `_followerBrackets.ContainsKey(entryKey)` (idempotent guard) | return false |
| B5 | `activePositions.TryGetValue(entryKey, ...)` fails | return false |

Returns `true` with populated `pi` when all preconditions pass; `false` otherwise. Collapses 5 `continue` statements into a single `if (!TryGetEntryPassCandidate(...)) continue;` at the call site.

### xUnit Tests Required

- `[Fact]` — null `entryOrder` → returns `false`
- `[Fact]` — master-account entry → returns `false`
- `[Fact]` — null `ExecutingAccount` → returns `false`
- `[Fact]` — duplicate `_followerBrackets` key → returns `false`
- `[Fact]` — `activePositions` lookup fails → returns `false`
- `[Fact]` — all guards pass → returns `true` with correct `pi`

### Jane Street Alignment

- CYC = 6 (1 base path + 5 branch-points) ≤ 8 ✅
- Single responsibility: eligibility validation only ✅
- No `lock()` blocks ✅
- Illegal states (null order, master account, duplicate FSM) unrepresentable at call site ✅
- Zero allocation: `out` parameter avoids heap alloc ✅

---

## Ticket 2 — Extract `LinkStopOrderToFSM`

**Type**: extraction
**Target CYC**: ≤3
**Source file**: `src/V12_002.SIMA.Lifecycle.cs`
**Source region**: lines 787–891 (stop-order wiring block inside foreach)
**Depends on**: none

### Goal

Extract the stop-order linking block (B8–B10) from the entry-order foreach loop body into a new private helper method `LinkStopOrderToFSM`. This method mirrors the existing `LinkTargetOrderToFSM` naming convention, making the two-step bracket wiring (stop + target) symmetric and consistent.

### Signature

```csharp
private void LinkStopOrderToFSM(
    ref FollowerBracketFSM fsm,
    string entryKey,
    ref int ordersIndexed)
```

### Extracted Branches (B8–B10)

| Branch | Guard | Action |
|--------|-------|--------|
| B8 | `stopOrders.TryGetValue(entryKey, out Order stopOrder)` fails | early return |
| B9 | `stopOrder == null` | early return |
| B10 | `stopOrder.OrderId` is non-empty | register in `_orderIdToFsmKey` and increment `ordersIndexed` |

Core assignments: `fsm.StopOrder = stopOrder` (unconditional when stop found and non-null).

### xUnit Tests Required

- `[Fact]` — `stopOrders` does not contain `entryKey` → no assignment, `ordersIndexed` unchanged
- `[Fact]` — stop order found but `null` → no assignment, `ordersIndexed` unchanged
- `[Fact]` — stop order found, `OrderId` is empty → assignment done, `ordersIndexed` unchanged
- `[Fact]` — stop order found, `OrderId` non-empty → assignment done, `ordersIndexed` incremented, key registered in `_orderIdToFsmKey`

### Jane Street Alignment

- CYC = 3 (1 base path + 2 branch-points for TryGetValue bool + null check; B10 adds 1 = 3 total) ≤ 8 ✅
- Single responsibility: stop-order FSM linkage only ✅
- Mirrors `LinkTargetOrderToFSM` naming convention ✅
- No `lock()` blocks; `ConcurrentDictionary` ops unchanged ✅
- `ref` parameters: zero-allocation, single-threaded cold-path scope ✅

---

## Ticket 3 — Extract `RunEntryOrderPass`

**Type**: extraction
**Target CYC**: ≤4 (parent `HydrateFSMsFromWorkingOrders` reduces to CYC = 1)
**Source file**: `src/V12_002.SIMA.Lifecycle.cs`
**Source region**: lines 787–891 (complete entry-order foreach loop)
**Depends on**: Ticket 1 (`TryGetEntryPassCandidate`), Ticket 2 (`LinkStopOrderToFSM`)

### Goal

Extract the entire entry-order `foreach` loop from `HydrateFSMsFromWorkingOrders` into a new private helper method `RunEntryOrderPass`. This creates a symmetric structural peer to the pre-existing `HydrateFromOpenPositions` method, making the two-pass structure of Phase 5 hydration explicit and readable. After this extraction, the parent method `HydrateFSMsFromWorkingOrders` becomes a pure orchestrator with CYC = 1.

### Signature

```csharp
private void RunEntryOrderPass(
    ref int ordersIndexed,
    ref int fsmCreated)
```

### Internal Call Sequence

```
foreach (var entry in entryOrders.ToArray())                     // loop branch
    if (!TryGetEntryPassCandidate(entry.Key, entry.Value, out pi)) continue;  // guard branch
    var state = MapOrderStateToFSMState(entry.Value);
    if (state == null) continue;                                 // terminal-state branch
    if (state.Value == Active) FindLivePosition(...);            // live-position branch
    ResolveRemainingContracts(...);
    var fsm = BuildFSM(...);
    LinkStopOrderToFSM(ref fsm, entry.Key, ref ordersIndexed);
    LinkTargetOrderToFSM × 5;
    RegisterFSM(ref fsm, entry.Key, ref ordersIndexed, ref fsmCreated);
```

CYC = 4: loop (1) + TryGetEntryPassCandidate guard (1) + null state skip (1) + Active live-position check (1).

### Parent Method After Extraction

```csharp
private void HydrateFSMsFromWorkingOrders()
{
    int fsmCreated = 0;
    int ordersIndexed = 0;
    Print("[SIMA] Phase 5 FSM Hydration: Starting entry order pass...");
    RunEntryOrderPass(ref ordersIndexed, ref fsmCreated);
    Print($"[SIMA] Phase 5 FSM Hydration (Entry Pass): {fsmCreated} FSMs created, {ordersIndexed} order IDs indexed.");
    int positionFsmCreated = HydrateFromOpenPositions(..., ref ordersIndexed, ref fsmCreated);
    Print($"[SIMA] Phase 5 FSM Hydration (Position Pass): {positionFsmCreated} Active FSMs created from open positions.");
    Print($"[SIMA] Phase 5 FSM Hydration: {fsmCreated} FSMs created, {ordersIndexed} order IDs indexed.");
}
```

Parent CYC = 1 (pure orchestrator — zero decision branches).

### xUnit Tests Required

- `[Fact]` — empty `entryOrders` → loop does not execute, counters remain zero
- `[Fact]` — single entry, `TryGetEntryPassCandidate` returns false → FSM not created, counters unchanged
- `[Fact]` — single entry, all guards pass, terminal state → FSM not created
- `[Fact]` — single entry, all guards pass, Active state → FSM created, `fsmCreated` incremented, `ordersIndexed` updated
- `[Fact]` — multiple entries, mixed eligibility → only eligible entries produce FSMs

### Jane Street Alignment

- CYC = 4 ≤ 8 ✅
- Single responsibility: entry-order pass orchestration only ✅
- Symmetric peer to `HydrateFromOpenPositions` ✅
- Parent reduces to CYC = 1 (pure orchestrator) ✅
- No `lock()` blocks ✅
- `ref` counters: zero-allocation, single-threaded cold-path ✅

---

## Execution Order

```
Ticket 1 (TryGetEntryPassCandidate)  ─┐
                                       ├─→ Ticket 3 (RunEntryOrderPass)
Ticket 2 (LinkStopOrderToFSM)        ─┘
```

Tickets 1 and 2 are independent and may execute in parallel. Ticket 3 requires both.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Bobcoins Used** | 6 |
| **Execution Time** | 2026-06-29T01:40:00Z |
| **Wave** | 7 |
| **Phase** | 4 |
| **Method** | `HydrateFSMsFromWorkingOrders` |
| **Epic** | EPIC-W7-113 |
| **Output** | `docs/brain/EPIC-W7-113/04-tickets.md` |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity |
| **sequential-thinking calls** | 5 (1 probe + 4 analysis) |
| **Ticket Count** | 3 |

---

*Generated: Phase 4 — Ticket Generation | EPIC-W7-113 | Wave 7*
