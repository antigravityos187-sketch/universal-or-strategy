---
# TICKET EPIC-CCN-16-03: ResolveRemainingContracts
# Epic: EPIC-CCN-16
# Sequence: 3 of 6
# Depends on: ticket-01 (MapOrderStateToFSMState)
---

## Objective
Extract position quantity resolution logic that determines remaining contracts based on FSM state and live position data.

## Scope
IN scope:
- Extract lines 505-518 from [`HydrateFSMsFromWorkingOrders`](src/V12_002.SIMA.Lifecycle.cs:505)
- Create new private method `ResolveRemainingContracts`
- Update parent method to call extracted method

OUT of scope:
- Position data structures (PositionInfo, Position)
- FSM state mapping (handled by ticket-01)
- FSM construction (handled by ticket-02)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-16/02-analysis.md`](docs/brain/EPIC-CCN-16/02-analysis.md) -- Data Flow Analysis section
- Approach: [`docs/brain/EPIC-CCN-16/03-approach.md`](docs/brain/EPIC-CCN-16/03-approach.md) -- Method Signatures section 2

## Implementation Instructions

### New Method Signature
```csharp
/// <summary>
/// Resolves remaining contracts for FSM based on order state and live position.
/// For Active state, queries live position. Otherwise uses order quantity.
/// </summary>
/// <param name="entryOrder">Entry order</param>
/// <param name="pi">Position info (contains executing account)</param>
/// <param name="state">FSM state (determines resolution strategy)</param>
/// <returns>Remaining contracts (always >= 0)</returns>
private int ResolveRemainingContracts(
    Order entryOrder,
    PositionInfo pi,
    FollowerBracketState state
)
```

### Extraction Details
**Source Lines**: 505-518 in `HydrateFSMsFromWorkingOrders`

**Original Code Pattern**:
```csharp
int remainingContracts = entryOrder.Quantity;
if (state == FollowerBracketState.Active)
{
    Position livePosition = pi.ExecutingAccount.Positions.ToArray()
        .FirstOrDefault(p =>
            p != null
            && p.Instrument != null
            && p.Instrument.FullName == Instrument.FullName
            && p.MarketPosition != MarketPosition.Flat
        );
    if (livePosition != null)
    {
        remainingContracts = Math.Max(0, Math.Abs(livePosition.Quantity));
    }
}
```

**Extracted Method Body**:
```csharp
private int ResolveRemainingContracts(
    Order entryOrder,
    PositionInfo pi,
    FollowerBracketState state
)
{
    int remainingContracts = entryOrder.Quantity;
    
    if (state == FollowerBracketState.Active)
    {
        Position livePosition = pi.ExecutingAccount.Positions.ToArray()
            .FirstOrDefault(p =>
                p != null
                && p.Instrument != null
                && p.Instrument.FullName == Instrument.FullName
                && p.MarketPosition != MarketPosition.Flat
            );
        
        if (livePosition != null)
        {
            remainingContracts = Math.Max(0, Math.Abs(livePosition.Quantity));
        }
    }
    
    return remainingContracts;
}
```

### Call Site Update
**Replace lines 505-518 with**:
```csharp
int remainingContracts = ResolveRemainingContracts(entryOrder, pi, state.Value);
```

### Placement
Insert new method immediately after `BuildFSM` (created in ticket-02).

## V12 DNA Guardrails
- [x] Zero new lock() statements (read-only position query, no locks needed)
- [x] Zero non-ASCII characters in string literals (no string literals)
- [x] All sub-methods >= 15 LOC (extracted method is ~25 lines)
- [x] Residual method CYC target: Reduces parent from ~36 to ~33
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
- [ ] New method `ResolveRemainingContracts` created with correct signature
- [ ] Method placed immediately after `BuildFSM`
- [ ] Original lines 505-518 replaced with 1-line call site
- [ ] Method returns `int` (remaining contracts)
- [ ] For Active state: queries live position and returns absolute quantity
- [ ] For non-Active state: returns entry order quantity
- [ ] Null position handled gracefully (returns order quantity)
- [ ] Result always >= 0 (Math.Max ensures non-negative)
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows reduced CYC for `HydrateFSMsFromWorkingOrders`
- [ ] lock() audit: ZERO matches
- [ ] Build passes: `powershell -File .\scripts\build_readiness.ps1`
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible