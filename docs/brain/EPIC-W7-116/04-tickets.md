# EPIC-W7-116 — Phase 4 Tickets

**Method**: AuditFleet_CalculateExpectedActual
**Source**: src/V12_002.REAPER.Audit.cs
**Lines**: 382–451
**Class**: V12_002 (partial — REAPER Audit Module)
**CYC**: 13 (from architecture plan, Phase 2 source inspection)
**Lane**: P4-L7
**DNA Verdict**: PASS (Phase 3 audit — zero violations)

---

## Ticket Summary

| # | Ticket | Type | Helper Method | Projected CYC | Parent Residual After |
|---|--------|------|---------------|---------------|----------------------|
| 1 | Extract GetSignedActualQty | extraction | `GetSignedActualQty` | 2 | ≤11 |
| 2 | Extract RepairHydratedActiveFsms | extraction | `RepairHydratedActiveFsms` | 5 | ≤6 |
| 3 | Extract LogAuditStateIfNeeded | extraction | `LogAuditStateIfNeeded` | 3 | ≤3 (final) |

**Total tickets**: 3
**Max CYC after all extractions**: 5 (`RepairHydratedActiveFsms`)
**Parent residual CYC (final)**: 3
**Jane Street threshold**: 8 — all methods PASS

---

## Ticket 1 — Extract GetSignedActualQty

**Type**: extraction
**Helper**: `GetSignedActualQty`
**Target CYC**: 2
**Parent CYC After This Ticket**: ≤11
**Execution Order**: 1 (simplest, no dependencies on other helpers)

### What to Extract

Extract the signed-quantity computation from `AuditFleet_CalculateExpectedActual`. The current parent reads `pos` (a `Position` object from `acct.Positions`) and computes the signed integer quantity, guarding against null and flat positions. This is a pure function with no side effects.

**Extracted logic responsibility**: Read broker `Position`, return signed int quantity. Return 0 if position is null or flat.

### Target Signature

```csharp
private int GetSignedActualQty(Position pos)
```

### Implementation Notes

- Pure function — reads `pos`, returns `int`
- Guard: `if (pos == null || pos.Quantity == 0) return 0;`
- Sign logic: return positive for long, negative for short based on `pos.MarketPosition`
- No calls to other helpers
- No `lock()` blocks — lock-free mandate satisfied
- ASCII-only string literals

### Call Site in Parent After Extraction

```csharp
actualQty = GetSignedActualQty(pos);
```

### xUnit Test

```csharp
[Fact]
public void GetSignedActualQty_ReturnsZeroWhenNull()
{
    // Arrange: pos = null
    // Act: result = GetSignedActualQty(null)
    // Assert: Assert.Equal(0, result)
}
```

### Acceptance Criteria

- [ ] `GetSignedActualQty` added as `private` method in `V12_002` partial class, same file (`src/V12_002.REAPER.Audit.cs`)
- [ ] Parent `AuditFleet_CalculateExpectedActual` calls `GetSignedActualQty(pos)` at the original extraction site
- [ ] `GetSignedActualQty` CYC = 2
- [ ] No `lock()` blocks introduced
- [ ] ASCII-only string literals
- [ ] xUnit `[Fact]` test `GetSignedActualQty_ReturnsZeroWhenNull` added (NOT NUnit/MSTest)
- [ ] Build passes: `dotnet build` zero errors
- [ ] `AuditSingleFleetAccount` call site (line 132) unchanged — parent method signature preserved

---

## Ticket 2 — Extract RepairHydratedActiveFsms

**Type**: extraction
**Helper**: `RepairHydratedActiveFsms`
**Target CYC**: 5
**Parent CYC After This Ticket**: ≤6
**Execution Order**: 2 (after Ticket 1; depends on `accountFsms` list built in parent)

### What to Extract

Extract the FSM repair loop from `AuditFleet_CalculateExpectedActual`. The current parent iterates `accountFsms` (a `List<FollowerBracketFSM>`), identifies hydrated-active FSMs with no entry order, and calls `TryTerminateFollowerBracket` for stale FSMs when the broker position is flat. This is the most complex section (CYC=5) due to the loop and nested conditionals.

**Extracted logic responsibility**: Iterate FSM list, repair hydrated-active FSMs with no entry order, terminate stale FSMs via `TryTerminateFollowerBracket` when broker is flat. Updates `fsmExpectedQty` via `ref` parameter.

### Target Signature

```csharp
private void RepairHydratedActiveFsms(
    List<FollowerBracketFSM> accountFsms,
    ref int fsmExpectedQty,
    int actualQty)
```

### Implementation Notes

- Iterates `accountFsms` — contains loop (contributes to CYC)
- Calls `TryTerminateFollowerBracket` as FSM side-effect for stale FSMs
- `fsmExpectedQty` passed by `ref` — parent continues using it after call
- `actualQty` passed by value — read-only in helper
- No new `lock()` blocks — FSM delegation preserves lock-free mandate
- ASCII-only string literals

### Call Site in Parent After Extraction

```csharp
RepairHydratedActiveFsms(accountFsms, ref fsmExpectedQty, actualQty);
```

### xUnit Test

```csharp
[Fact]
public void RepairHydratedActiveFsms_TerminatesStaleFsm()
{
    // Arrange: accountFsms with stale hydrated-active FSM, actualQty=0 (flat broker)
    // Act: RepairHydratedActiveFsms(accountFsms, ref fsmExpectedQty, actualQty)
    // Assert: Assert.True(TryTerminateFollowerBracket was called for stale FSM)
}
```

### Acceptance Criteria

- [ ] `RepairHydratedActiveFsms` added as `private` method in `V12_002` partial class, same file (`src/V12_002.REAPER.Audit.cs`)
- [ ] Parent `AuditFleet_CalculateExpectedActual` calls `RepairHydratedActiveFsms(accountFsms, ref fsmExpectedQty, actualQty)` at the original extraction site
- [ ] `RepairHydratedActiveFsms` CYC = 5
- [ ] `fsmExpectedQty` updated correctly via `ref` — parent retains correct value after call
- [ ] No `lock()` blocks introduced
- [ ] ASCII-only string literals
- [ ] xUnit `[Fact]` test `RepairHydratedActiveFsms_TerminatesStaleFsm` added (NOT NUnit/MSTest)
- [ ] Build passes: `dotnet build` zero errors
- [ ] `AuditSingleFleetAccount` call site (line 132) unchanged — parent method signature preserved

---

## Ticket 3 — Extract LogAuditStateIfNeeded

**Type**: extraction
**Helper**: `LogAuditStateIfNeeded`
**Target CYC**: 3
**Parent CYC After This Ticket**: 3 (final residual)
**Execution Order**: 3 (last; depends on final computed `expectedQty` and `actualQty` values)

### What to Extract

Extract the conditional audit logging section from `AuditFleet_CalculateExpectedActual`. The current parent computes `hasState = (expectedQty != 0 || actualQty != 0)` and conditionally prints an audit log line when `shouldLog` is true. This helper returns `hasState` as its output, which the parent assigns to the `out bool hasState` parameter.

**Extracted logic responsibility**: Compute `hasState` boolean from `expectedQty` and `actualQty`, conditionally print audit log line when `shouldLog` is true, return `hasState`.

### Target Signature

```csharp
private bool LogAuditStateIfNeeded(
    Account acct,
    bool shouldLog,
    int expectedQty,
    int actualQty)
```

### Implementation Notes

- Returns `bool hasState` — parent assigns to `out bool hasState` parameter
- `hasState = (expectedQty != 0 || actualQty != 0)` — single boolean expression
- Conditional log call: `if (shouldLog && hasState) Print(...)` 
- ASCII-only format strings in any Print call
- No FSM side effects
- No `lock()` blocks

### Call Site in Parent After Extraction

```csharp
hasState = LogAuditStateIfNeeded(acct, shouldLog, expectedQty, actualQty);
```

### xUnit Test

```csharp
[Fact]
public void LogAuditStateIfNeeded_ReturnsTrueWhenHasState()
{
    // Arrange: expectedQty=1, actualQty=0 (hasState expected = true)
    // Act: result = LogAuditStateIfNeeded(acct, shouldLog: false, expectedQty: 1, actualQty: 0)
    // Assert: Assert.True(result)
}
```

### Acceptance Criteria

- [ ] `LogAuditStateIfNeeded` added as `private` method in `V12_002` partial class, same file (`src/V12_002.REAPER.Audit.cs`)
- [ ] Parent `AuditFleet_CalculateExpectedActual` calls `LogAuditStateIfNeeded(acct, shouldLog, expectedQty, actualQty)` and assigns result to `hasState`
- [ ] `LogAuditStateIfNeeded` CYC = 3
- [ ] `out bool hasState` in parent correctly receives value from helper return
- [ ] No `lock()` blocks introduced
- [ ] ASCII-only string literals in any Print format strings
- [ ] xUnit `[Fact]` test `LogAuditStateIfNeeded_ReturnsTrueWhenHasState` added (NOT NUnit/MSTest)
- [ ] Build passes: `dotnet build` zero errors
- [ ] `AuditSingleFleetAccount` call site (line 132) unchanged — parent method signature preserved

---

## Post-Extraction Verification

After all 3 tickets are executed, verify:

| Method | Expected CYC | Jane Street Threshold | Status |
|--------|-------------|----------------------|--------|
| `AuditFleet_CalculateExpectedActual` (residual) | 3 | ≤8 | PASS |
| `GetSignedActualQty` | 2 | ≤8 | PASS |
| `RepairHydratedActiveFsms` | 5 | ≤8 | PASS |
| `LogAuditStateIfNeeded` | 3 | ≤8 | PASS |
| **Max CYC** | **5** | **≤8** | **PASS** |

---

## Agent Tracking

| Field | Value |
|-------|-------|
| **Agent Name** | v12-phase4-tickets |
| **Wave** | 7 |
| **Phase** | 4 — Ticket Generation |
| **Method** | AuditFleet_CalculateExpectedActual |
| **Source File** | src/V12_002.REAPER.Audit.cs |
| **Original CYC** | 13 |
| **max_cyc_projected** | 5 |
| **Parent Residual CYC** | 3 |
| **extraction_count** | 3 |
| **ticket_count** | 3 |
| **DNA Verdict** | PASS |
| **jcodemunch tools called** | resolve_repo, get_symbol_complexity |
| **sequential-thinking calls** | 4 (1 probe + 3 analysis thoughts) |
| **Output** | docs/brain/EPIC-W7-116/04-tickets.md |
| **Generated** | 2026-06-29T02:00:00Z |
