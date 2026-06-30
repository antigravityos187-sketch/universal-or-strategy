# Phase 4 Tickets — EPIC-W7-099

| Field | Value |
|---|---|
| **Epic** | EPIC-W7-099 |
| **Method** | `PurgePositionIfEligible` |
| **Source File** | `src/V12_002.Orders.Management.Cleanup.cs` |
| **CYC Baseline** | 11 |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 3 |

---

## Extraction Summary

`PurgePositionIfEligible` (CYC=11) is split into **2 helper extraction tickets**.
Each ticket targets one logical block, achieving a cyc reduction that brings every unit to ≤8 (Jane Street strict threshold).
Residual parent after both extractions: CYC=3 (two `if (followerExpected == 0)` dispatch guards only).

---

## TICKET-W7-099-1

| Field | Value |
|---|---|
| **ticket_id** | TICKET-W7-099-1 |
| **helper_name** | `TryPurgeStandardPosition` |
| **concern** | Hot-path standard META-GUARD position purge (Block A) |
| **attribute** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` |
| **path** | `src/V12_002.Orders.Management.Cleanup.cs` |
| **lines_to_move** | ~210–219 (Block A body, approx 10 lines) |
| **cyc_reduction** | 2 (removes `!HasActiveOrPendingOrderForEntry` guard + `if (removed)` from parent scope) |
| **projected_helper_cyc** | 3 |
| **execution_mode** | v12-engineer (Bob CLI) |

### Description

Extract Block A from `PurgePositionIfEligible` into a new private helper `TryPurgeStandardPosition(string entryName)`.

Block A contains the standard META-GUARD purge path:
1. Guard: `!HasActiveOrPendingOrderForEntry(entryName)`
2. `activePositions.TryRemove(entryName, out _)` — lock-free ConcurrentDictionary remove
3. `if (removed)` → `SymmetryGuardForgetEntry(entryName)` — post-remove cleanup

**No LINQ. No alloc. Zero-alloc hot path.** `[AggressiveInlining]` is mandatory — this is the hot-path purge that runs on every eligible position cleanup.

### Signature

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private void TryPurgeStandardPosition(string entryName)
```

### CYC Breakdown

| Source | +CYC |
|---|---|
| Base | 1 |
| `if (!HasActiveOrPendingOrderForEntry(entryName))` | +1 |
| `if (removed)` | +1 |
| **Total** | **3** |

### Acceptance Criteria

- [ ] `TryPurgeStandardPosition` compiles with `[AggressiveInlining]` attribute
- [ ] CYC of helper = 3 (verify with complexity audit)
- [ ] Parent `PurgePositionIfEligible` delegates via `if (followerExpected == 0) TryPurgeStandardPosition(entryName);`
- [ ] No new `lock()` blocks introduced
- [ ] Build passes: `dotnet build` zero errors
- [ ] xUnit test: `[Fact]` covering the `removed == true` and `removed == false` branches

---

## TICKET-W7-099-2

| Field | Value |
|---|---|
| **ticket_id** | TICKET-W7-099-2 |
| **helper_name** | `TryPurgeFlatFollowerByBroker` |
| **concern** | Cold-path FIX-ZP-02 broker-confirmed flat SIMA follower force-purge (Block B) |
| **attribute** | `[MethodImpl(MethodImplOptions.NoInlining)]` |
| **path** | `src/V12_002.Orders.Management.Cleanup.cs` |
| **lines_to_move** | ~221–242 (Block B body, approx 22 lines) |
| **cyc_reduction** | 8 (removes all 7 Block B branches + LINQ predicate from parent scope) |
| **projected_helper_cyc** | 8 |
| **execution_mode** | v12-engineer (Bob CLI) |

### Description

Extract Block B from `PurgePositionIfEligible` into a new private helper `TryPurgeFlatFollowerByBroker(string entryName)`.

Block B implements the **FIX-ZP-02 secondary safety net** — broker-confirmed flat SIMA follower force-purge:
1. `activePositions.TryGetValue(entryName, out var followerCheck)` — lock-free ConcurrentDictionary lookup
2. `followerCheck.IsFollower` — guard: only process follower positions
3. `followerCheck.ExecutingAccount != null` — guard: account must be set
4. LINQ `FirstOrDefault(p => p.Instrument == Instrument)` — broker position lookup
5. `brokerPos != null` — null check post-LINQ
6. `brokerPos.MarketPosition == MarketPosition.Flat` — flat confirmation
7. `activePositions.TryRemove(entryName, out _)` — force-remove
8. `if (removedFZP)` — post-remove `Print` logging guard

**Contains LINQ and Print — cold diagnostic path only.** `[NoInlining]` is mandatory to prevent the LINQ closure heap allocation from polluting the hot path.

### Signature

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void TryPurgeFlatFollowerByBroker(string entryName)
```

### CYC Breakdown

| Source | +CYC |
|---|---|
| Base | 1 |
| `activePositions.TryGetValue(...)` | +1 |
| `followerCheck.IsFollower` | +1 |
| `followerCheck.ExecutingAccount != null` | +1 |
| LINQ predicate `p => p.Instrument == Instrument` | +1 |
| `brokerPos != null` | +1 |
| `brokerPos.MarketPosition == MarketPosition.Flat` | +1 |
| `if (removedFZP)` | +1 |
| **Total** | **8** |

### Acceptance Criteria

- [ ] `TryPurgeFlatFollowerByBroker` compiles with `[NoInlining]` attribute
- [ ] CYC of helper = 8 (verify with complexity audit — equals threshold, does not exceed)
- [ ] Parent `PurgePositionIfEligible` delegates via `if (followerExpected == 0) TryPurgeFlatFollowerByBroker(entryName);`
- [ ] No new `lock()` blocks introduced
- [ ] Build passes: `dotnet build` zero errors
- [ ] xUnit test: `[Fact]` covering flat broker position found and not-flat / null cases

---

## Residual Parent After All Extractions

```csharp
private void PurgePositionIfEligible(string entryName, int followerExpected)
{
    if (followerExpected == 0)
        TryPurgeStandardPosition(entryName);

    if (followerExpected == 0)
        TryPurgeFlatFollowerByBroker(entryName);
}
```

| Unit | CYC After Extraction | Threshold | Status |
|---|---|---|---|
| `PurgePositionIfEligible` (residual) | 3 | ≤8 | ✅ PASS |
| `TryPurgeStandardPosition` | 3 | ≤8 | ✅ PASS |
| `TryPurgeFlatFollowerByBroker` | 8 | ≤8 | ✅ PASS |
| **max_cyc_projected** | **8** | **≤8** | **✅ PASS** |

**projected_parent_cyc_after_all: 3**

---

## Execution Order

| Step | Ticket | Dependency |
|---|---|---|
| 1 | TICKET-W7-099-1 | None — execute first |
| 2 | TICKET-W7-099-2 | After Ticket 1 committed (same method, sequential) |

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Epic** | EPIC-W7-099 |
| **Method** | `PurgePositionIfEligible` |
| **Source File** | `src/V12_002.Orders.Management.Cleanup.cs` |
| **CYC Baseline** | 11 |
| **ticket_count** | 2 |
| **projected_parent_cyc_after_all** | 3 |
| **max_cyc_projected** | 8 |
| **DNA Audit** | PASS (from Phase 3) |
| **Sequential Thinking Thoughts** | 3 |
| **Phase** | 4 |
