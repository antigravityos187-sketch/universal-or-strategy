---
# TICKET EPIC-CCN-16-01: MapOrderStateToFSMState
# Epic: EPIC-CCN-16
# Sequence: 1 of 6
# Depends on: NONE
---

## Objective
Extract pure state mapping function that converts NinjaTrader OrderState to V12 FollowerBracketState.

## Scope
IN scope:
- Extract lines 488-503 from [`HydrateFSMsFromWorkingOrders`](src/V12_002.SIMA.Lifecycle.cs:488)
- Create new private method `MapOrderStateToFSMState`
- Update parent method to call extracted method

OUT of scope:
- Any other methods in the file
- Caller method `HydrateWorkingOrdersFromBroker`
- FSM data structures

## Context References
- Analysis: [`docs/brain/EPIC-CCN-16/02-analysis.md`](docs/brain/EPIC-CCN-16/02-analysis.md) -- State Machine Invariants section
- Approach: [`docs/brain/EPIC-CCN-16/03-approach.md`](docs/brain/EPIC-CCN-16/03-approach.md) -- Method Signatures section 1

## Implementation Instructions

### New Method Signature
```csharp
/// <summary>
/// Maps NinjaTrader OrderState to V12 FollowerBracketState.
/// Pure function - no side effects, deterministic mapping.
/// </summary>
/// <param name="entryState">NinjaTrader order state</param>
/// <returns>Corresponding FSM state, or null if terminal state (skip FSM creation)</returns>
/// <remarks>
/// Terminal states (Cancelled, Rejected, etc.) return null to signal caller to skip FSM creation.
/// This preserves the original behavior where terminal orders are ignored.
/// </remarks>
private FollowerBracketState? MapOrderStateToFSMState(OrderState entryState)
```

### Extraction Details
**Source Lines**: 488-503 in `HydrateFSMsFromWorkingOrders`

**Original Code Pattern**:
```csharp
FollowerBracketState state;
if (entryOrder.OrderState == OrderState.Filled || entryOrder.OrderState == OrderState.PartFilled)
{
    state = FollowerBracketState.Active;
}
else if (entryOrder.OrderState == OrderState.Accepted)
{
    state = FollowerBracketState.Accepted;
}
else if (entryOrder.OrderState == OrderState.Working
    || entryOrder.OrderState == OrderState.Submitted
    || entryOrder.OrderState == OrderState.Initialized
    || entryOrder.OrderState == OrderState.ChangePending
    || entryOrder.OrderState == OrderState.ChangeSubmitted)
{
    state = FollowerBracketState.Submitted;
}
else
{
    continue; // Terminal state - skip FSM creation
}
```

**Extracted Method Body**:
```csharp
private FollowerBracketState? MapOrderStateToFSMState(OrderState entryState)
{
    if (entryState == OrderState.Filled || entryState == OrderState.PartFilled)
    {
        return FollowerBracketState.Active;
    }
    else if (entryState == OrderState.Accepted)
    {
        return FollowerBracketState.Accepted;
    }
    else if (entryState == OrderState.Working
        || entryState == OrderState.Submitted
        || entryState == OrderState.Initialized
        || entryState == OrderState.ChangePending
        || entryState == OrderState.ChangeSubmitted)
    {
        return FollowerBracketState.Submitted;
    }
    else
    {
        return null; // Terminal state - skip FSM creation
    }
}
```

### Call Site Update
**Replace lines 488-503 with**:
```csharp
FollowerBracketState? state = MapOrderStateToFSMState(entryOrder.OrderState);
if (state == null) continue; // Terminal state - skip FSM creation
```

### Placement
Insert new method immediately before `HydrateFSMsFromWorkingOrders` (around line 463).

## V12 DNA Guardrails
- [x] Zero new lock() statements (pure function, no locks needed)
- [x] Zero non-ASCII characters in string literals (no string literals)
- [x] All sub-methods >= 15 LOC (extracted method is ~20 lines)
- [x] Residual method CYC target: Reduces parent from 45 to ~37
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
- [ ] New method `MapOrderStateToFSMState` created with correct signature
- [ ] Method placed immediately before `HydrateFSMsFromWorkingOrders`
- [ ] Original lines 488-503 replaced with 2-line call site
- [ ] Method returns `FollowerBracketState?` (nullable)
- [ ] Terminal states return `null`
- [ ] Active states (Filled, PartFilled) return `Active`
- [ ] Accepted state returns `Accepted`
- [ ] Working states return `Submitted`
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows reduced CYC for `HydrateFSMsFromWorkingOrders`
- [ ] lock() audit: ZERO matches
- [ ] Build passes: `powershell -File .\scripts\build_readiness.ps1`
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible