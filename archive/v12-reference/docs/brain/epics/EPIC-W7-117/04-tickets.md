# EPIC-W7-117 — Phase 4 Tickets

**Method**: `SymmetryGuardReplaceExistingFollowerTarget`
**Source**: `src/V12_002.Symmetry.Replace.cs`
**CYC Baseline**: 9 (architecture plan authoritative; scope file: 17)
**CYC Target**: ≤ 8
**Lane**: P4-L7
**DNA Verdict**: PASS (Phase 3)

---

## Ticket Summary

| # | Ticket | Type | Helper Signature | CYC of Helper | Parent CYC After |
|---|--------|------|-----------------|--------------|-----------------|
| 1 | Extract `IsOrderLiveState` | extraction | `private static bool IsOrderLiveState(Order o)` | 1 | ~8 (combined with T2) |
| 2 | Extract `ExecuteTargetReplacePhase1` | extraction | `private void ExecuteTargetReplacePhase1(PositionInfo pos, Order oldTarget, int targetNumber, string fleetEntryName, string signalName)` | 3 | 8 ✓ |

**max_cyc_projected = 8** — at threshold, Jane Street strict standard satisfied.

---

## Ticket 1 — Extract `IsOrderLiveState`

**Type**: extraction
**Target CYC**: 1 (helper); parent residual contribution: eliminates duplicate 4-case OR blocks
**Jane Street Attribute**: `[MethodImpl(MethodImplOptions.AggressiveInlining)]`

### Problem

The parent method contains the 4-state `OrderState` OR predicate at **two separate call sites**:

```csharp
// Site 1 — stale target cleanup branch
if (o.OrderState == OrderState.Working
    || o.OrderState == OrderState.Accepted
    || o.OrderState == OrderState.Submitted
    || o.OrderState == OrderState.ChangePending)

// Site 2 — replace-eligible branch (identical)
if (o.OrderState == OrderState.Working
    || o.OrderState == OrderState.Accepted
    || o.OrderState == OrderState.Submitted
    || o.OrderState == OrderState.ChangePending)
```

Each 4-case OR contributes +4 branch points to the parent's CYC. Having both inline keeps the parent well above the ≤8 threshold and creates drift risk relative to the identical predicate in `Propagation.cs:424`.

### Extraction

```csharp
[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static bool IsOrderLiveState(Order o) =>
    o.OrderState == OrderState.Working
    || o.OrderState == OrderState.Accepted
    || o.OrderState == OrderState.Submitted
    || o.OrderState == OrderState.ChangePending;
```

Both sites in the parent become:

```csharp
if (IsOrderLiveState(staleTarget)) { ... }
// ...
if (IsOrderLiveState(oldTarget)) { ... }
```

### Acceptance Criteria

- [ ] `IsOrderLiveState` added as `private static bool` method in `src/V12_002.Symmetry.Replace.cs`
- [ ] `[MethodImpl(MethodImplOptions.AggressiveInlining)]` attribute applied
- [ ] Both duplicate 4-case OR blocks replaced with `IsOrderLiveState(...)` calls
- [ ] Helper CYC = 1 (single expression, no branching)
- [ ] No `lock()` introduced; no LINQ; no heap allocation
- [ ] ASCII-only identifiers and string literals
- [ ] Build passes: `dotnet build src/`
- [ ] xUnit test added: `[Fact]` verifying all four `OrderState` values return `true`; non-live states (e.g. `OrderState.Filled`) return `false`

### Files Changed

| File | Change |
|------|--------|
| `src/V12_002.Symmetry.Replace.cs` | Add `IsOrderLiveState`; replace 2 duplicate OR blocks |

---

## Ticket 2 — Extract `ExecuteTargetReplacePhase1`

**Type**: extraction
**Target CYC**: 3 (helper); parent CYC after both extractions: 8
**Jane Street Attribute**: `[MethodImpl(MethodImplOptions.NoInlining)]`

### Problem

The replace-eligible branch inside the parent contains the full DNA-FIX Phase 1 FSM logic:
- Price guard (`newPrice <= 0` → early return)
- Direction ternary (`Long` → `Sell`, else `BuyToCover`)
- `FollowerTargetReplaceSpec` construction
- `_followerTargetReplaceSpecs` dictionary write
- `StampReaperMoveGrace()` invocation
- `pos.ExecutingAccount.Cancel(new[] { oldTarget })` call

This block has no single descriptive name inside the parent, making the FSM phase invisible. It contributes 2+ branch points to the parent CYC (price guard + direction ternary) and should be isolated as a named Phase 1 FSM step per V12 DNA.

### Extraction

```csharp
[MethodImpl(MethodImplOptions.NoInlining)]
private void ExecuteTargetReplacePhase1(
    PositionInfo pos,
    Order oldTarget,
    int targetNumber,
    string fleetEntryName,
    string signalName
)
{
    double newPrice = GetTargetPrice(pos, targetNumber);
    if (newPrice <= 0)
        return;
    int qty = GetTargetContracts(pos, targetNumber);
    OrderAction exitAction =
        pos.Direction == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;
    var tSpec = new FollowerTargetReplaceSpec
    {
        EntryName = fleetEntryName,
        TargetNum = targetNumber,
        NewTargetPrice = Instrument.MasterInstrument.RoundToTickSize(newPrice),
        Quantity = qty,
        ExitAction = exitAction,
        TargetAccount = pos.ExecutingAccount,
        CancellingOrderId = oldTarget.OrderId,
    };
    _followerTargetReplaceSpecs[signalName] = tSpec;
    StampReaperMoveGrace();
    pos.ExecutingAccount.Cancel(new[] { oldTarget });
}
```

Call site in parent becomes:

```csharp
if (IsOrderLiveState(oldTarget))
{
    string signalName = SymmetryTrim(targetTag + "_" + fleetEntryName, 40);
    ExecuteTargetReplacePhase1(pos, oldTarget, targetNumber, fleetEntryName, signalName);
}
```

### Parent CYC After Both Extractions

| Branch | +Delta | Running CYC |
|--------|--------|-------------|
| Base | — | 1 |
| `ExecutingAccount == null` null guard | +1 | 2 |
| `isFilled \|\| isRunner` (2 OR predicates) | +2 | 4 |
| `qty <= 0` | +1 | 5 |
| Stale dict `TryGetValue` + null check | +1 | 6 |
| `IsOrderLiveState(staleTarget)` if-branch | +1 | 7 |
| Dict miss guard (`!TryGetValue`) | +1 | 8 |
| `IsOrderLiveState(oldTarget)` if-branch (→ delegates to T1) | — | 8 |

**max_cyc_projected = 8** ✓

### Acceptance Criteria

- [ ] `ExecuteTargetReplacePhase1` added as `private void` method in `src/V12_002.Symmetry.Replace.cs`
- [ ] `[MethodImpl(MethodImplOptions.NoInlining)]` attribute applied (cold broker-interaction path)
- [ ] Parent method delegates replace-eligible block to `ExecuteTargetReplacePhase1`
- [ ] Helper CYC = 3 (base + price guard + direction ternary)
- [ ] Parent CYC = 8 after both T1 and T2 extractions
- [ ] `_followerTargetReplaceSpecs` dict write uses `ConcurrentDictionary` — no `lock()` introduced
- [ ] No LINQ; ASCII-only; no Unicode
- [ ] Build passes: `dotnet build src/`
- [ ] xUnit tests added:
  - `[Fact]` verifying early return when `newPrice <= 0`
  - `[Fact]` verifying `OrderAction.Sell` for `Long` direction
  - `[Fact]` verifying `OrderAction.BuyToCover` for non-`Long` direction

### Files Changed

| File | Change |
|------|--------|
| `src/V12_002.Symmetry.Replace.cs` | Add `ExecuteTargetReplacePhase1`; replace inline replace-eligible block |

---

## Execution Order

Execute Ticket 1 first (establishes `IsOrderLiveState`), then Ticket 2 (uses `IsOrderLiveState` in the refactored parent). Both helpers reside in the same file — no cross-file changes.

---

## Post-Execution Validation

```powershell
# Build
dotnet build src/

# Complexity audit
python scripts/complexity_audit.py

# Pre-push validation
powershell -File .\scripts\pre_push_validation.ps1 -Fast

# Hard-link sync
powershell -File .\deploy-sync.ps1
```

Expected outcome: `SymmetryGuardReplaceExistingFollowerTarget` CYC = 8, `IsOrderLiveState` CYC = 1, `ExecuteTargetReplacePhase1` CYC = 3.

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Phase** | 4 |
| **Wave** | 7 |
| **Epic** | EPIC-W7-117 |
| **Method** | `SymmetryGuardReplaceExistingFollowerTarget` |
| **CYC Baseline** | 9 (architecture plan) |
| **max_cyc_projected** | 8 |
| **ticket_count** | 2 |
| **Sequential Thinking Thoughts** | 3 |
| **MCP: resolve_repo** | antigravityos187-sketch/universal-or-strategy — loadable |
| **MCP: get_symbol_complexity** | Symbol not in index (pre-extraction state); architecture plan CYC=9 used as authoritative |
| **DNA Verdict** | PASS (Phase 3) |
