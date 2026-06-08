---
# TICKET EPIC-CCN-1-03: CreateFollowerBracketFSM Initialization Extraction
# Epic: EPIC-CCN-1
# Sequence: 3 of 7
# Depends on: ticket-02-MapBrokerStateToFSM.md
---

## Objective
Extract the FollowerBracketFSM struct initialization logic for Pass 1 (entry order path) into a dedicated method to improve readability and reduce parent method complexity

## Scope
IN scope:
- Extract `CreateFollowerBracketFSM()` method from lines 520-528
- Initialize FSM struct with all required fields
- Handle entry order reference assignment
- File: `src/V12_002.SIMA.Lifecycle.cs`

OUT of scope:
- State mapping logic (already extracted in ticket-02)
- Order linking logic (separate tickets)
- Position recovery FSM creation (separate ticket-07)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-1/01-analysis.md`](01-analysis.md) -- Section "Change Surface Area / Methods Created"
- Approach: [`docs/brain/EPIC-CCN-1/02-approach.md`](02-approach.md) -- Section "2. Target State / Sub-Methods to Create #2"

## Implementation Instructions

### Extract New Method
Create private method with signature:
```csharp
private FollowerBracketFSM CreateFollowerBracketFSM(
    string entryKey,
    PositionInfo positionInfo,
    FollowerBracketState state,
    int remainingContracts,
    Order entryOrder
)
```

**Initialization Logic** (from lines 520-528):
- Set `State = state` (from MapBrokerStateToFSM)
- Set `RemainingContracts = remainingContracts` (from MapBrokerStateToFSM)
- Set `AccountName = positionInfo.ExecutingAccount.Name`
- Set `EntryOrder = entryOrder` (NOT null for Pass 1)
- Set `LastUpdateUtc = DateTime.UtcNow`
- Initialize `Targets = new Order[5]` (empty array)
- Set `StopOrder = null` (will be linked later)

**Estimated LOC**: 25-30 lines

### Update Parent Method Call Site (Lines 520-528)
Replace inline struct initialization with:
```csharp
var fsm = CreateFollowerBracketFSM(entryKey, pi, state, remainingContracts, entryOrder);
```

## V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Extracted method >= 15 LOC (extraction floor)
- [ ] Method CYC target: ≤3 (no branching, pure initialization)
- [ ] No logic drift -- preserve exact struct field assignments

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
- [ ] `CreateFollowerBracketFSM()` method created with correct signature
- [ ] All FSM struct fields initialized correctly
- [ ] `EntryOrder` field set to non-null order reference (Pass 1 semantic)
- [ ] `LastUpdateUtc` uses `DateTime.UtcNow` at creation time
- [ ] `Targets` array initialized to size 5
- [ ] Parent method updated to call helper
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows CYC ≤3 for `CreateFollowerBracketFSM`
- [ ] lock() audit: ZERO matches
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible

## Estimated Effort
**30 minutes** (LOW risk, pure initialization, no branching)