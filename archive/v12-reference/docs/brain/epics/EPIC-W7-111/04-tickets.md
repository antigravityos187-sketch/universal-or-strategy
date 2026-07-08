# EPIC-W7-111 — Phase 4 Tickets

**Method**: HydrateExpectedPositionsFromBroker
**Source**: src/V12_002.SIMA.Lifecycle.cs
**CYC**: 0 (parse artefact; manual McCabe = 11 conservative / 15 liberal)
**Lane**: P4-L7
**DNA Verdict**: PASS
**Wave**: 7

---

## Ticket Summary

| # | Ticket | Type | Scope | CYC Target | Tests |
|---|--------|------|-------|-----------|-------|
| 1 | Extract IsMatchingOpenPosition guard predicate | extraction | Add private bool helper | ≤5 | 5 xUnit [Fact] |
| 2 | Extract HydrateSingleAccount + refactor parent shell | extraction | Add private void helper + refactor parent | ≤5 | 3 xUnit [Fact] |

**Total tickets**: 2
**Total projected post-extraction CYC**: 5 (all symbols)
**Total xUnit tests**: 8
**Execution order**: Ticket 1 before Ticket 2 (sequential dependency — Ticket 2 calls IsMatchingOpenPosition)

---

## Ticket 1 — Extract IsMatchingOpenPosition Guard Predicate

**Type**: extraction
**Target CYC**: ≤5
**File**: `src/V12_002.SIMA.Lifecycle.cs`
**Class**: `V12_002` (partial)
**Dependency**: none (no prerequisite tickets)

### Responsibility

Extract the four-condition guard predicate that validates a `Position` object is eligible for broker hydration. Normalizes the inconsistency between Block A (explicit null checks) and Block B (null-conditional `?.`) in the original method body into a single canonical predicate.

### Signature

```csharp
private bool IsMatchingOpenPosition(Position pos)
```

### Implementation

```csharp
private bool IsMatchingOpenPosition(Position pos)
{
    if (pos == null)
        return false;
    if (pos.Instrument == null)
        return false;
    if (pos.Instrument.FullName != Instrument.FullName)
        return false;
    if (pos.MarketPosition == MarketPosition.Flat)
        return false;
    return true;
}
```

### CYC Breakdown

base 1 + guard (pos == null) 1 + guard (Instrument == null) 1 + guard (FullName !=) 1 + guard (MarketPosition.Flat) 1 = **CYC = 5**

### xUnit Tests (5 [Fact] cases)

| Test | Input | Expected |
|------|-------|----------|
| `IsMatchingOpenPosition_NullPos_ReturnsFalse` | pos = null | false |
| `IsMatchingOpenPosition_NullInstrument_ReturnsFalse` | pos.Instrument = null | false |
| `IsMatchingOpenPosition_WrongInstrument_ReturnsFalse` | pos.Instrument.FullName != Instrument.FullName | false |
| `IsMatchingOpenPosition_FlatPosition_ReturnsFalse` | pos.MarketPosition = Flat | false |
| `IsMatchingOpenPosition_ValidOpenLong_ReturnsTrue` | non-null pos, matching instrument, Long | true |

### Jane Street Alignment

- CYC <= 8: YES (CYC=5)
- Single responsibility: YES — guard predicate only
- Guard Clause pattern: YES — 4 early returns
- ASCII-only string literals: YES — no string literals in this method
- Lock-free: YES — no state mutations, pure predicate

### Acceptance Criteria

- [ ] `IsMatchingOpenPosition` added as `private bool` to `V12_002` partial class in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] CYC of extracted method = 5 (verify via complexity audit)
- [ ] 5 xUnit `[Fact]` tests pass (NO NUnit/MSTest)
- [ ] Build passes: `dotnet build` zero errors
- [ ] No lock() blocks introduced: `grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs` = 0 matches
- [ ] No unrelated symbols modified (V12.23 scope discipline)

---

## Ticket 2 — Extract HydrateSingleAccount + Refactor Parent Shell

**Type**: extraction
**Target CYC**: ≤5
**File**: `src/V12_002.SIMA.Lifecycle.cs`
**Class**: `V12_002` (partial)
**Dependency**: Ticket 1 must be complete (`IsMatchingOpenPosition` must exist before this ticket executes)

### Responsibility

**Part A — New helper**: Extract the per-account hydration loop body (foreach positions, IsMatchingOpenPosition guard, signed-qty calculation, Enqueue -> AddOrUpdateExpectedPosition, Print log, hydratedCount increment, break, catch) into a dedicated `HydrateSingleAccount` private method.

**Part B — Parent refactor**: Replace the duplicated Block A + Block B body of `HydrateExpectedPositionsFromBroker` with an orchestration shell that iterates fleet accounts and delegates to `HydrateSingleAccount`, then handles the master account.

### Signatures

```csharp
private void HydrateSingleAccount(Account acct, ref int hydratedCount)

private void HydrateExpectedPositionsFromBroker()  // shell (refactored, not new)
```

### HydrateSingleAccount Implementation

```csharp
private void HydrateSingleAccount(Account acct, ref int hydratedCount)
{
    try
    {
        foreach (Position pos in acct.Positions.ToArray())
        {
            if (!IsMatchingOpenPosition(pos))
                continue;
            int qty = pos.MarketPosition == MarketPosition.Long ? pos.Quantity : -pos.Quantity;
            var capturedAcct = acct.Name;
            var capturedQty = qty;
            Enqueue(ctx =>
                ctx.AddOrUpdateExpectedPosition(ExpKey(capturedAcct), capturedQty, v => capturedQty)
            );
            Print($"[SIMA HYDRATE] {acct.Name}: Seeded expected={qty} from broker ({pos.MarketPosition} {pos.Quantity})");
            hydratedCount++;
            break;
        }
    }
    catch (Exception ex)
    {
        Print($"[SIMA HYDRATE] WARNING: Could not read positions for {acct.Name}: {ex.Message}");
    }
}
```

### HydrateSingleAccount CYC Breakdown

base 1 + foreach 1 + if(!IsMatchingOpenPosition) 1 + ternary qty 1 + catch 1 = **CYC = 5**

### Parent Shell After Extraction

```csharp
private void HydrateExpectedPositionsFromBroker()
{
    int hydratedCount = 0;
    foreach (Account acct in Account.All)
    {
        if (!IsFleetAccount(acct))
            continue;
        HydrateSingleAccount(acct, ref hydratedCount);
    }
    if (hydratedCount > 0)
        Print($"[SIMA HYDRATE] Hydrated {hydratedCount} account(s) with live broker positions");

    bool masterIsFleet993 = IsFleetAccount(Account);
    if (!masterIsFleet993)
        HydrateSingleAccount(Account, ref hydratedCount);
}
```

### Parent Shell CYC Breakdown

base 1 + foreach 1 + if(!IsFleetAccount) 1 + if(hydratedCount > 0) 1 + if(!masterIsFleet993) 1 = **CYC = 5**

### xUnit Tests (3 [Fact] cases)

| Test | Scenario | Expected |
|------|----------|----------|
| `HydrateSingleAccount_NoMatchingPosition_HydratedCountUnchanged` | Account has no positions matching IsMatchingOpenPosition | hydratedCount stays 0, no Enqueue call |
| `HydrateSingleAccount_OneMatchingLongPosition_HydratedCountIncremented` | Account has one matching Long position | hydratedCount = 1, Enqueue called with correct signed qty |
| `HydrateSingleAccount_PositionAccessThrows_LogsWarningNoException` | acct.Positions.ToArray() throws | catch block fires, Print warning, hydratedCount unchanged, no unhandled exception |

### Jane Street Alignment

- CYC <= 8: YES — HydrateSingleAccount CYC=5, parent shell CYC=5
- Single responsibility: YES — HydrateSingleAccount handles one account only; parent shell is pure orchestration
- Loop Body Extraction: YES — foreach body moved to named helper
- Lock-free/Actor pattern: YES — all mutations route through Enqueue; zero lock() blocks
- Illegal states unrepresentable: YES — null/flat positions cannot reach Enqueue (filtered by IsMatchingOpenPosition)
- Structural duplication eliminated: YES — Block A and Block B both delegate to HydrateSingleAccount
- ASCII-only string literals: YES — all Print format strings are 7-bit ASCII

### Acceptance Criteria

- [ ] `HydrateSingleAccount` added as `private void` to `V12_002` partial class in `src/V12_002.SIMA.Lifecycle.cs`
- [ ] `HydrateExpectedPositionsFromBroker` body replaced with orchestration shell (Block A + Block B removed)
- [ ] CYC of HydrateSingleAccount = 5 (verify via complexity audit)
- [ ] CYC of HydrateExpectedPositionsFromBroker shell = 5 (verify via complexity audit)
- [ ] 3 xUnit `[Fact]` tests pass (NO NUnit/MSTest)
- [ ] Build passes: `dotnet build` zero errors
- [ ] No lock() blocks: `grep -r "lock(" src/V12_002.SIMA.Lifecycle.cs` = 0 matches
- [ ] deploy-sync.ps1 executed after src/ changes (NinjaTrader hard-link sync)
- [ ] No unrelated symbols modified (V12.23 scope discipline)

---

## Agent Tracking

| Field | Value |
|---|---|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 |
| **Generated** | 2026-06-29T01:40:00Z |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity |
| **sequential-thinking calls** | 4 (1 probe + 3 analysis thoughts) |
| **Input: architecture plan** | docs/brain/EPIC-W7-111/02-architecture-plan.md |
| **Input: audit report** | docs/brain/EPIC-W7-111/03-audit-report.md |
| **Output** | docs/brain/EPIC-W7-111/04-tickets.md |
| **ticket_count** | 2 |
| **max_cyc_projected** | 5 |
| **total_xunit_tests** | 8 |
| **dna_verdict** | PASS |
| **CYC parse artefact note** | jcodemunch reports CYC=0 (NinjaTrader partial-class boundary); manual McCabe = 11 conservative |
