---
# TICKET EPIC-CCN-1-02: MapBrokerStateToFSM State Mapping Extraction
# Epic: EPIC-CCN-1
# Sequence: 2 of 7
# Depends on: NONE
---

## Objective
Extract the broker OrderState-to-FSM state mapping logic and quantity calculation into a dedicated method to reduce complexity and improve testability

## Scope
IN scope:
- Extract `MapBrokerStateToFSM()` method from lines 488-518
- Map 8 OrderState enum values to 3 FollowerBracketState values
- Calculate `remainingContracts` from live position or order quantity
- File: `src/V12_002.SIMA.Lifecycle.cs`

OUT of scope:
- FSM struct initialization (separate ticket)
- Order linking logic (separate tickets)
- Position recovery logic (separate ticket)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-1/01-analysis.md`](01-analysis.md) -- Section "Risk Hotspots #3: FSM State Mapping Complexity"
- Approach: [`docs/brain/EPIC-CCN-1/02-approach.md`](02-approach.md) -- Section "2. Target State / Sub-Methods to Create #1"

## Implementation Instructions

### Extract New Method
Create private method with signature:
```csharp
private (FollowerBracketState state, int remainingContracts) MapBrokerStateToFSM(
    Order entryOrder,
    PositionInfo positionInfo
)
```

**State Mapping Logic** (from lines 488-503):
- `OrderState.Filled` OR `OrderState.PartFilled` → `FollowerBracketState.Active`
- `OrderState.Accepted` → `FollowerBracketState.Accepted`
- `OrderState.Working` OR `OrderState.Submitted` OR `OrderState.Initialized` OR `OrderState.ChangePending` OR `OrderState.ChangeSubmitted` → `FollowerBracketState.Submitted`
- All other states (Cancelled, Rejected, Unknown) → `FollowerBracketState.Unknown` (terminal states, skip FSM creation)

**Quantity Calculation Logic** (from lines 505-518):
- If state is `Active`: Use live position quantity from `positionInfo.Position.Quantity`
- Otherwise: Use `Math.Max(entryOrder.Quantity - entryOrder.Filled, 0)`
- Apply `Math.Abs()` to handle short positions

**Estimated LOC**: 35-40 lines

### Update Parent Method Call Site (Lines 488-518)
Replace inline logic with:
```csharp
var (state, remainingContracts) = MapBrokerStateToFSM(entryOrder, pi);
if (state == FollowerBracketState.Unknown) continue; // Terminal state, skip FSM creation
```

## V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Extracted method >= 15 LOC (extraction floor)
- [ ] Method CYC target: ≤8 (5 state branches + 2 quantity branches + 1 position lookup)
- [ ] No logic drift -- preserve exact state mapping and quantity calculation

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
- [ ] `MapBrokerStateToFSM()` method created with correct signature
- [ ] All 8 OrderState enum values mapped correctly to 3 FSM states
- [ ] Quantity calculation matches original logic (live position vs order quantity)
- [ ] Terminal states return `FollowerBracketState.Unknown` to trigger skip
- [ ] Parent method updated to call helper and handle tuple return
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows CYC ≤8 for `MapBrokerStateToFSM`
- [ ] lock() audit: ZERO matches
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible

## Estimated Effort
**45 minutes** (MEDIUM risk, complex branching logic requires careful verification)