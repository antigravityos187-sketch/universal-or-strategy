---
# TICKET EPIC-CCN-16-05: HydrateFromOpenPositions
# Epic: EPIC-CCN-16
# Sequence: 5 of 6
# Depends on: ticket-02 (BuildFSM), ticket-04 (RegisterFSM)
---

## Objective
Extract Position Pass logic that creates FSMs for accounts with open positions but terminal entry orders.

## Scope
IN scope:
- Extract lines 602-743 from [`HydrateFSMsFromWorkingOrders`](src/V12_002.SIMA.Lifecycle.cs:602)
- Create new private method `HydrateFromOpenPositions`
- Update parent method to call extracted method
- Preserve REAPER grace window logic (lines 651-655)
- Preserve all logging statements

OUT of scope:
- REAPER implementation (_positionPassFailedFirstSeen dictionary)
- Helper methods (IsFleetAccount, Print)
- Existing LinkTargetOrderToFSM helper (line 463)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-16/02-analysis.md`](docs/brain/EPIC-CCN-16/02-analysis.md) -- Error Handling Analysis section
- Approach: [`docs/brain/EPIC-CCN-16/03-approach.md`](docs/brain/EPIC-CCN-16/03-approach.md) -- Method Signatures section 5

## Implementation Instructions

### New Method Signature
```csharp
/// <summary>
/// Position Pass: Creates FSMs for accounts with open positions but terminal entry orders.
/// Handles edge case where entry order is cancelled/rejected but position still open.
/// </summary>
/// <param name="stopOrders">Dictionary of stop orders (for key recovery)</param>
/// <param name="target1Orders">Dictionary of target1 orders</param>
/// <param name="target2Orders">Dictionary of target2 orders</param>
/// <param name="target3Orders">Dictionary of target3 orders</param>
/// <param name="target4Orders">Dictionary of target4 orders</param>
/// <param name="target5Orders">Dictionary of target5 orders</param>
/// <param name="ordersIndexed">Counter (incremented for each order linked)</param>
/// <param name="fsmCreated">Counter (incremented for each FSM created)</param>
/// <returns>Number of FSMs created in position pass</returns>
private int HydrateFromOpenPositions(
    Dictionary<string, Order> stopOrders,
    Dictionary<string, Order> target1Orders,
    Dictionary<string, Order> target2Orders,
    Dictionary<string, Order> target3Orders,
    Dictionary<string, Order> target4Orders,
    Dictionary<string, Order> target5Orders,
    ref int ordersIndexed,
    ref int fsmCreated
)
```

### Extraction Details
**Source Lines**: 602-743 in `HydrateFSMsFromWorkingOrders`

**Key Logic to Preserve**:
1. **Account Iteration**: Loop through all accounts (lines 606-608)
2. **Fleet Filter**: Skip fleet accounts (line 609)
3. **Terminal Entry Check**: Verify entry order is terminal (lines 610-612)
4. **Position Lookup**: Find open position for account (lines 614-621)
5. **Idempotency Guard**: Skip if FSM already exists (line 623)
6. **Stop Order Recovery**: Find stop order to recover entry key (lines 625-643)
7. **REAPER Grace Window**: Track failed recoveries with 5-minute grace (lines 651-655)
8. **FSM Creation**: Build and register FSM for recovered position (lines 657-743)

**Critical Preservation Points**:
- **Grace Window Logic** (lines 651-655): Must preserve REAPER 5-minute grace period
- **Logging** (lines 645-650, 736-742): Must preserve all Print statements
- **Error Handling**: Defensive null checks throughout

### Method Structure
```csharp
private int HydrateFromOpenPositions(
    Dictionary<string, Order> stopOrders,
    Dictionary<string, Order> target1Orders,
    Dictionary<string, Order> target2Orders,
    Dictionary<string, Order> target3Orders,
    Dictionary<string, Order> target4Orders,
    Dictionary<string, Order> target5Orders,
    ref int ordersIndexed,
    ref int fsmCreated
)
{
    int positionFsmCreated = 0;
    
    foreach (Account acct in Account.All)
    {
        if (IsFleetAccount(acct)) continue;
        
        // Check if account has terminal entry order
        bool hasTerminalEntry = _followerBrackets.Values.Any(fsm =>
            fsm.AccountName == acct.Name
            && fsm.EntryOrder != null
            && (fsm.EntryOrder.OrderState == OrderState.Cancelled
                || fsm.EntryOrder.OrderState == OrderState.Rejected
                || fsm.EntryOrder.OrderState == OrderState.Unknown));
        
        if (!hasTerminalEntry) continue;
        
        // Find open position
        Position acctPos = acct.Positions.ToArray()
            .FirstOrDefault(p =>
                p != null
                && p.Instrument != null
                && p.Instrument.FullName == Instrument.FullName
                && p.MarketPosition != MarketPosition.Flat);
        
        if (acctPos == null) continue;
        
        // Skip if FSM already exists for this account
        if (_followerBrackets.Values.Any(fsm => fsm.AccountName == acct.Name)) continue;
        
        // Recover entry key from stop order
        string recoveredKey = null;
        foreach (var stopKvp in stopOrders)
        {
            Order stopOrd = stopKvp.Value;
            if (stopOrd != null && stopOrd.Account != null && stopOrd.Account.Name == acct.Name)
            {
                recoveredKey = stopKvp.Key;
                break;
            }
        }
        
        if (string.IsNullOrEmpty(recoveredKey))
        {
            // REAPER grace window logic
            DateTime now = DateTime.UtcNow;
            if (!_positionPassFailedFirstSeen.ContainsKey(acct.Name))
            {
                _positionPassFailedFirstSeen[acct.Name] = now;
            }
            
            Print(string.Format(
                "[SIMA] Position Pass WARNING: Account {0} has open position but no stop order found. " +
                "REAPER will handle cleanup after 5-minute grace window.",
                acct.Name
            ));
            continue;
        }
        
        // Build FSM for recovered position
        PositionInfo pi = new PositionInfo
        {
            AccountName = acct.Name,
            ExecutingAccount = acct,
            IsFollower = true
        };
        
        int remainingContracts = Math.Max(0, Math.Abs(acctPos.Quantity));
        
        var fsm = BuildFSM(
            recoveredKey,
            pi,
            null, // No entry order for position pass
            FollowerBracketState.Active,
            remainingContracts
        );
        
        // Link stop order
        Order stopOrder;
        if (stopOrders.TryGetValue(recoveredKey, out stopOrder) && stopOrder != null)
        {
            fsm.StopOrder = stopOrder;
            if (!string.IsNullOrEmpty(stopOrder.OrderId))
            {
                _orderIdToFsmKey[stopOrder.OrderId] = recoveredKey;
                ordersIndexed++;
            }
        }
        
        // Link target orders using existing helper
        LinkTargetOrderToFSM(ref fsm, recoveredKey, 0, target1Orders, ref ordersIndexed);
        LinkTargetOrderToFSM(ref fsm, recoveredKey, 1, target2Orders, ref ordersIndexed);
        LinkTargetOrderToFSM(ref fsm, recoveredKey, 2, target3Orders, ref ordersIndexed);
        LinkTargetOrderToFSM(ref fsm, recoveredKey, 3, target4Orders, ref ordersIndexed);
        LinkTargetOrderToFSM(ref fsm, recoveredKey, 4, target5Orders, ref ordersIndexed);
        
        // Register FSM
        RegisterFSM(recoveredKey, fsm, null, ref ordersIndexed, ref fsmCreated);
        positionFsmCreated++;
        
        Print(string.Format(
            "[SIMA] Position Pass: Created FSM for account {0} (key: {1}, contracts: {2})",
            acct.Name, recoveredKey, remainingContracts
        ));
    }
    
    return positionFsmCreated;
}
```

### Call Site Update
**Replace lines 602-743 with**:
```csharp
int positionFsmCreated = HydrateFromOpenPositions(
    stopOrders, target1Orders, target2Orders, target3Orders, target4Orders, target5Orders,
    ref ordersIndexed, ref fsmCreated
);

Print(string.Format(
    "[SIMA] Phase 5 FSM Hydration (Position Pass): {0} Active FSMs created from open positions.",
    positionFsmCreated
));
```

### Placement
Insert new method immediately after `RegisterFSM` (created in ticket-04).

## V12 DNA Guardrails
- [x] Zero new lock() statements (actor-serialized, no locks needed)
- [x] Zero non-ASCII characters in string literals (all logging is ASCII)
- [x] All sub-methods >= 15 LOC (extracted method is ~130 lines)
- [x] Residual method CYC target: Reduces parent from ~31 to ~8
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
- [ ] New method `HydrateFromOpenPositions` created with correct signature
- [ ] Method placed immediately after `RegisterFSM`
- [ ] Original lines 602-743 replaced with 6-line call site
- [ ] Method returns `int` (number of FSMs created)
- [ ] Fleet accounts skipped (IsFleetAccount check)
- [ ] Terminal entry orders detected correctly
- [ ] Open positions found via LINQ query
- [ ] Idempotency guard prevents duplicate FSMs
- [ ] Stop order recovery logic preserved
- [ ] REAPER grace window logic preserved (5-minute tracking)
- [ ] All logging statements preserved
- [ ] Uses existing `LinkTargetOrderToFSM` helper (5 calls)
- [ ] Uses `BuildFSM` from ticket-02
- [ ] Uses `RegisterFSM` from ticket-04
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows `HydrateFSMsFromWorkingOrders` CYC ≤8
- [ ] lock() audit: ZERO matches
- [ ] Build passes: `powershell -File .\scripts\build_readiness.ps1`
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible