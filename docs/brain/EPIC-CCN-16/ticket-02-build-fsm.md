---
# TICKET EPIC-CCN-16-02: BuildFSM
# Epic: EPIC-CCN-16
# Sequence: 2 of 6
# Depends on: NONE (can run parallel with ticket-01)
---

## Objective
Extract FSM factory method that constructs FollowerBracketFSM instances with consistent initialization.

## Scope
IN scope:
- Extract lines 520-528 from [`HydrateFSMsFromWorkingOrders`](src/V12_002.SIMA.Lifecycle.cs:520)
- Create new private method `BuildFSM`
- Update parent method to call extracted method

OUT of scope:
- FSM data structure definition (FollowerBracketFSM)
- Order linking logic (handled by existing LinkTargetOrderToFSM)
- FSM registration logic (separate ticket)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-16/02-analysis.md`](docs/brain/EPIC-CCN-16/02-analysis.md) -- Data Flow Analysis section
- Approach: [`docs/brain/EPIC-CCN-16/03-approach.md`](docs/brain/EPIC-CCN-16/03-approach.md) -- Method Signatures section 3

## Implementation Instructions

### New Method Signature
```csharp
/// <summary>
/// Factory method to construct FollowerBracketFSM instance.
/// Centralizes FSM initialization logic.
/// </summary>
/// <param name="entryKey">FSM key (entry order name)</param>
/// <param name="pi">Position info (contains account name)</param>
/// <param name="entryOrder">Entry order (may be null for position pass)</param>
/// <param name="state">Initial FSM state</param>
/// <param name="remainingContracts">Remaining contracts</param>
/// <returns>Initialized FSM instance</returns>
private FollowerBracketFSM BuildFSM(
    string entryKey,
    PositionInfo pi,
    Order entryOrder,
    FollowerBracketState state,
    int remainingContracts
)
```

### Extraction Details
**Source Lines**: 520-528 in `HydrateFSMsFromWorkingOrders`

**Original Code Pattern**:
```csharp
var fsm = new FollowerBracketFSM
{
    Key = entryKey,
    AccountName = pi.AccountName,
    EntryOrder = entryOrder,
    State = state,
    RemainingContracts = remainingContracts,
    Targets = new Order[5]
};
```

**Extracted Method Body**:
```csharp
private FollowerBracketFSM BuildFSM(
    string entryKey,
    PositionInfo pi,
    Order entryOrder,
    FollowerBracketState state,
    int remainingContracts
)
{
    return new FollowerBracketFSM
    {
        Key = entryKey,
        AccountName = pi.AccountName,
        EntryOrder = entryOrder,
        State = state,
        RemainingContracts = remainingContracts,
        Targets = new Order[5]
    };
}
```

### Call Site Update
**Replace lines 520-528 with**:
```csharp
var fsm = BuildFSM(entryKey, pi, entryOrder, state.Value, remainingContracts);
```

### Placement
Insert new method immediately after `MapOrderStateToFSMState` (created in ticket-01).

## V12 DNA Guardrails
- [x] Zero new lock() statements (simple factory, no locks needed)
- [x] Zero non-ASCII characters in string literals (no string literals)
- [x] All sub-methods >= 15 LOC (extracted method is ~17 lines)
- [x] Residual method CYC target: Reduces parent from ~37 to ~36
- [x] No logic drift -- pure structural movement only

## Post-Edit Verification (Mandatory)
```powershell
# 1. Re-establish hard links (MANDATORY after every src/ edit)
powershell -File .\deploy-sync.ps1

# 2. Complexity verification
python scripts/complexity_audit.py

# 3. Lock regression (must return ZERO)
grep -r "lock(" src/

# 4. ASCII gate (must return ZERO)
grep -Prn "[^\x00-\x7F]" src/
```

## Acceptance Criteria
- [ ] New method `BuildFSM` created with correct signature
- [ ] Method placed immediately after `MapOrderStateToFSMState`
- [ ] Original lines 520-528 replaced with 1-line call site
- [ ] Method returns initialized `FollowerBracketFSM` instance
- [ ] All FSM fields properly initialized (Key, AccountName, EntryOrder, State, RemainingContracts, Targets)
- [ ] Targets array initialized to size 5
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows reduced CYC for `HydrateFSMsFromWorkingOrders`
- [ ] lock() audit: ZERO matches
- [ ] Build passes: `powershell -File .\scripts\build_readiness.ps1`
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible