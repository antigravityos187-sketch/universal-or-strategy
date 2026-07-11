# EPIC-W7-061 — Phase 4: Ticket Definitions

**Agent Name:** v12-phase4-tickets
**Wave:** 7
**Phase:** 4 — Ticket Generation
**Generated:** 2026-06-29T01:10:00Z
**Inputs:**
- `docs/brain/EPIC-W7-061/02-architecture-plan.md`
- `docs/brain/EPIC-W7-061/03-audit-report.md`

---

## Target Method Summary

| Field            | Value                                          |
|------------------|------------------------------------------------|
| **Method**       | `SubmitAndRegisterFleetOrders`                 |
| **File**         | `src/V12_002.SIMA.Fleet.cs`                   |
| **Lines**        | 174–217                                        |
| **CYC Baseline** | 12                                             |
| **Target CYC**   | <= 8 (Jane Street strict)                      |
| **Ticket Count** | 2                                              |
| **max_cyc_projected** | 5                                        |
| **projected_parent_cyc** | 4                                   |

---

## MCP Evidence Summary

| Tool | Result |
|------|--------|
| `resolve_repo` | `antigravityos187-sketch/universal-or-strategy` — indexed, 5147 symbols |
| `get_symbol_complexity` | Symbol not in index at resolution level — Phase 2 manual CYC analysis is authoritative (CYC 12 confirmed) |
| `get_extraction_candidates` | No candidates returned (index predates method); architecture plan evidence used |
| `search_ast call:lock` | 0 matches in `src/V12_002.SIMA.Fleet.cs` (Phase 3 audit) |
| `get_dependency_cycles` | 0 cycles (Phase 3 audit) |
| `find_references SubmitAndRegisterFleetOrders` | 0 cross-file refs — confirmed `private void` |

---

## Sequential Thinking Summary

| Thought | Conclusion |
|---------|-----------|
| 1 | **2 tickets** — one per cohesive concern (C and D from CYC breakdown) |
| 2 | T1 extracts FSM state transition (lines ~53-62 relative); T2 extracts order ID registration loop (lines ~64-73 relative); both call-sites wired in parent |
| 3 | All 3 post-extraction methods satisfy CYC <= 8; max = 5 (RegisterOrderIdsToFsmKey); parent CYC = 4 |

---

## Ticket Definitions

---

### TICKET-1: Extract `UpdateFleetFsmState`

| Field | Value |
|-------|-------|
| **Ticket ID** | EPIC-W7-061-T1 |
| **Title** | Extract FSM state transition to `UpdateFleetFsmState` |
| **File** | `src/V12_002.SIMA.Fleet.cs` |
| **Parent Method** | `SubmitAndRegisterFleetOrders` (lines 174–217) |
| **Concern Extracted** | Concern C — FSM state transition: TryGetValue + null guard + state guard + state/timestamp write |
| **CYC Removed from Parent** | 3 (one `if (TryGetValue)` branch + two `&&` compound guards) |
| **Helper CYC** | 4 (1 base + 1 TryGetValue + 1 null check + 1 state check) |
| **Visibility** | `private` |
| **Jane Street Hint** | `[MethodImpl(MethodImplOptions.AggressiveInlining)]` candidate (small, hot-path FSM write) |
| **Risk** | LOW — no callers modified, private method, zero cross-file refs |
| **DNA Verdict** | PASS (from Phase 3 audit) |

#### New Method Signature

```csharp
private void UpdateFleetFsmState(string fleetEntryName)
```

#### Code to Extract (move OUT of parent)

```csharp
FollowerBracketFSM pFsm;
if (
    _followerBrackets.TryGetValue(fleetEntryName, out pFsm)
    && pFsm != null
    && pFsm.State == FollowerBracketState.PendingSubmit
)
{
    pFsm.State = FollowerBracketState.Submitted;
    pFsm.LastUpdateUtc = DateTime.UtcNow;
}
```

#### Call-site Replacement in Parent

Replace the extracted block with:

```csharp
UpdateFleetFsmState(fleetEntryName);
```

#### Parent CYC After This Ticket Only

12 - 3 = **9** (T2 required to reach <= 8 final target)

#### Acceptance Criteria

- [ ] New private method `UpdateFleetFsmState(string fleetEntryName)` exists in `src/V12_002.SIMA.Fleet.cs`
- [ ] Parent `SubmitAndRegisterFleetOrders` calls `UpdateFleetFsmState(fleetEntryName)` at the correct position
- [ ] Extracted block removed from parent body
- [ ] `dotnet build` passes with zero errors
- [ ] `dotnet csharpier check src/` passes
- [ ] xUnit test added: verifies `FollowerBracketFSM.State` transitions from `PendingSubmit` to `Submitted` when `UpdateFleetFsmState` is called

---

### TICKET-2: Extract `RegisterOrderIdsToFsmKey`

| Field | Value |
|-------|-------|
| **Ticket ID** | EPIC-W7-061-T2 |
| **Title** | Extract order ID registration loop to `RegisterOrderIdsToFsmKey` |
| **File** | `src/V12_002.SIMA.Fleet.cs` |
| **Parent Method** | `SubmitAndRegisterFleetOrders` (lines 174–217) |
| **Concern Extracted** | Concern D — order ID registration: TryGetValue guard + for loop + null/orderId guard + dict write |
| **CYC Removed from Parent** | 4 (one `if (TryGetValue)` + one `for` + one `ord != null` + one `!IsNullOrEmpty`) |
| **Helper CYC** | 5 (1 base + 1 TryGetValue + 1 for + 1 null + 1 IsNullOrEmpty) |
| **Visibility** | `private` |
| **Jane Street Hint** | Lock-free; uses existing `ConcurrentDictionary`; no new allocation introduced |
| **Risk** | LOW — no callers modified, private method, zero cross-file refs |
| **DNA Verdict** | PASS (from Phase 3 audit) |
| **Dependency on T1** | None — can execute independently or after T1 |

#### New Method Signature

```csharp
private void RegisterOrderIdsToFsmKey(
    string fleetEntryName,
    Order[] orders,
    int orderCount
)
```

#### Code to Extract (move OUT of parent)

```csharp
FollowerBracketFSM fsm;
if (_followerBrackets.TryGetValue(fleetEntryName, out fsm))
{
    for (int i = 0; i < orderCount; i++)
    {
        var ord = orders[i];
        if (ord != null && !string.IsNullOrEmpty(ord.OrderId))
            _orderIdToFsmKey[ord.OrderId] = fleetEntryName;
    }
}
```

#### Call-site Replacement in Parent

Replace the extracted block with:

```csharp
RegisterOrderIdsToFsmKey(fleetEntryName, orders, orderCount);
```

#### Parent CYC After T1 + T2

1 (base) + 3 (Concern A: `orders != null` + `orderCount > 0` + `orderCount < orders.Length`) = **4**

#### Acceptance Criteria

- [ ] New private method `RegisterOrderIdsToFsmKey(string fleetEntryName, Order[] orders, int orderCount)` exists in `src/V12_002.SIMA.Fleet.cs`
- [ ] Parent `SubmitAndRegisterFleetOrders` calls `RegisterOrderIdsToFsmKey(fleetEntryName, orders, orderCount)` at the correct position
- [ ] Extracted block removed from parent body
- [ ] `dotnet build` passes with zero errors
- [ ] `dotnet csharpier check src/` passes
- [ ] xUnit test added: verifies `_orderIdToFsmKey` is populated correctly for non-null orders with non-empty `OrderId`
- [ ] xUnit test added: verifies null orders and empty-OrderId orders are skipped (defense-in-depth guard preserved)

---

## Final CYC Validation

| Method | CYC Calculation | Post-extraction CYC | Threshold | Status |
|--------|----------------|---------------------|-----------|--------|
| `SubmitAndRegisterFleetOrders` (parent) | 1 base + 3 Concern-A branches | **4** | <= 8 | **PASS** |
| `UpdateFleetFsmState` | 1 base + 1 TryGetValue + 1 null + 1 state | **4** | <= 8 | **PASS** |
| `RegisterOrderIdsToFsmKey` | 1 base + 1 TryGetValue + 1 for + 1 null + 1 IsNullOrEmpty | **5** | <= 8 | **PASS** |

**max_cyc_projected = 5** — headroom 3 below threshold.
**CYC reduction on parent: 12 → 4 = 67% reduction.**

---

## Execution Order

Tickets are independent and may execute in parallel or sequentially:

```
T1: Extract UpdateFleetFsmState
T2: Extract RegisterOrderIdsToFsmKey
Both target same file — if sequential, T1 first to minimize merge conflict risk.
```

---

## V12.23 Scope Compliance

| Check | Status |
|-------|--------|
| Single method targeted per ticket | PASS |
| Helpers extracted from subject method only | PASS |
| No caller modifications (ProcessFleetSlot, PumpFleetDispatch, ProcessValidPhotonSlot) | PASS |
| No sibling method modifications | PASS |
| No cross-file refactoring | PASS |
| Helpers added as private methods in same partial class | PASS |
| All callers confirmed private, zero external blast radius | PASS |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Epic** | EPIC-W7-061 |
| **Phase** | 4 |
| **Wave** | 7 |
| **CYC Baseline** | 12 |
| **max_cyc_projected** | 5 |
| **projected_parent_cyc** | 4 |
| **Ticket Count** | 2 |
| **dna_verdict** | PASS |
| **Status** | completed |
