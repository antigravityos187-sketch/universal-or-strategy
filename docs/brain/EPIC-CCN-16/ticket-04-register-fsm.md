---
# TICKET EPIC-CCN-16-04: RegisterFSM
# Epic: EPIC-CCN-16
# Sequence: 4 of 6
# Depends on: NONE (can run parallel with tickets 1-3)
---

## Objective
Extract FSM registration logic that adds FSM to tracking dictionaries and updates counters.

## Scope
IN scope:
- Extract lines 590-598 from [`HydrateFSMsFromWorkingOrders`](src/V12_002.SIMA.Lifecycle.cs:590)
- Create new private method `RegisterFSM`
- Update parent method to call extracted method

OUT of scope:
- Dictionary definitions (_followerBrackets, _orderIdToFsmKey)
- FSM construction (handled by ticket-02)
- Order linking logic (handled by existing LinkTargetOrderToFSM)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-16/02-analysis.md`](docs/brain/EPIC-CCN-16/02-analysis.md) -- Thread Safety Analysis section
- Approach: [`docs/brain/EPIC-CCN-16/03-approach.md`](docs/brain/EPIC-CCN-16/03-approach.md) -- Method Signatures section 4

## Implementation Instructions

### New Method Signature
```csharp
/// <summary>
/// Registers FSM in tracking dictionaries and updates counters.
/// Centralizes dictionary update logic for easier auditing.
/// </summary>
/// <param name="entryKey">FSM key</param>
/// <param name="fsm">FSM to register</param>
/// <param name="entryOrder">Entry order (may be null for position pass)</param>
/// <param name="ordersIndexed">Counter (incremented if entry order linked)</param>
/// <param name="fsmCreated">Counter (always incremented)</param>
private void RegisterFSM(
    string entryKey,
    FollowerBracketFSM fsm,
    Order entryOrder,
    ref int ordersIndexed,
    ref int fsmCreated
)
```

### Extraction Details
**Source Lines**: 590-598 in `HydrateFSMsFromWorkingOrders`

**Original Code Pattern**:
```csharp
_followerBrackets.TryAdd(entryKey, fsm);
if (entryOrder != null && !string.IsNullOrEmpty(entryOrder.OrderId))
{
    _orderIdToFsmKey[entryOrder.OrderId] = entryKey;
    ordersIndexed++;
}
fsmCreated++;
```

**Extracted Method Body**:
```csharp
private void RegisterFSM(
    string entryKey,
    FollowerBracketFSM fsm,
    Order entryOrder,
    ref int ordersIndexed,
    ref int fsmCreated
)
{
    _followerBrackets.TryAdd(entryKey, fsm);
    
    if (entryOrder != null && !string.IsNullOrEmpty(entryOrder.OrderId))
    {
        _orderIdToFsmKey[entryOrder.OrderId] = entryKey;
        ordersIndexed++;
    }
    
    fsmCreated++;
}
```

### Call Site Update
**Replace lines 590-598 with**:
```csharp
RegisterFSM(entryKey, fsm, entryOrder, ref ordersIndexed, ref fsmCreated);
```

### Placement
Insert new method immediately after `ResolveRemainingContracts` (created in ticket-03).

## V12 DNA Guardrails
- [x] Zero new lock() statements (ConcurrentDictionary is thread-safe, no locks needed)
- [x] Zero non-ASCII characters in string literals (no string literals)
- [x] All sub-methods >= 15 LOC (extracted method is ~18 lines)
- [x] Residual method CYC target: Reduces parent from ~33 to ~31
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
- [ ] New method `RegisterFSM` created with correct signature
- [ ] Method placed immediately after `ResolveRemainingContracts`
- [ ] Original lines 590-598 replaced with 1-line call site
- [ ] Method uses `TryAdd` for idempotent FSM registration
- [ ] Entry order linked to FSM key if present and has OrderId
- [ ] `ordersIndexed` counter incremented when entry order linked
- [ ] `fsmCreated` counter always incremented
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows reduced CYC for `HydrateFSMsFromWorkingOrders`
- [ ] lock() audit: ZERO matches
- [ ] Build passes: `powershell -File .\scripts\build_readiness.ps1`
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible