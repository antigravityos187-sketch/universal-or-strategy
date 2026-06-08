---
# TICKET EPIC-CCN-1-07: CreateRecoveryFSM Position-Pass Initialization
# Epic: EPIC-CCN-1
# Sequence: 7 of 7
# Depends on: ticket-06-ScanForRecoveryKey.md
---

## Objective
Extract the FollowerBracketFSM struct initialization logic for Pass 2 (position recovery path) into a dedicated method to parallel the Pass 1 FSM creation pattern

## Scope
IN scope:
- Extract `CreateRecoveryFSM()` method from lines 662-670
- Initialize FSM struct for orphaned positions
- Handle terminal entry order semantics (`EntryOrder = null`)
- File: `src/V12_002.SIMA.Lifecycle.cs`

OUT of scope:
- Pass 1 FSM creation (already extracted in ticket-03)
- Recovery key scanning (already extracted in ticket-06)
- Order linking and registration (handled inline in Pass 2)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-1/01-analysis.md`](01-analysis.md) -- Section "Invariants / FSM State Transition Invariants #4"
- Approach: [`docs/brain/EPIC-CCN-1/02-approach.md`](02-approach.md) -- Section "2. Target State / Sub-Methods to Create #7"

## Implementation Instructions

### Extract New Method
Create private method with signature:
```csharp
private FollowerBracketFSM CreateRecoveryFSM(
    string recoveredKey,
    Account account,
    Position position
)
```

**Initialization Logic** (from lines 662-670):
- Set `State = FollowerBracketState.Active` (always Active for recovery path)
- Set `RemainingContracts = Math.Abs(position.Quantity)` (live position quantity)
- Set `AccountName = account.Name`
- Set `EntryOrder = null` (CRITICAL: terminal entry marker for position-pass FSMs)
- Set `LastUpdateUtc = DateTime.UtcNow`
- Initialize `Targets = new Order[5]` (empty array)
- Set `StopOrder = null` (will be linked inline in parent method)

**CRITICAL**: `EntryOrder = null` is a semantic marker that distinguishes position-pass FSMs from entry-pass FSMs. This is load-bearing for REAPER logic.

**Estimated LOC**: 25-30 lines

### Update Parent Method Call Site (Lines 662-670)
Replace inline struct initialization with:
```csharp
var fsm = CreateRecoveryFSM(recoveredKey, acct, acctPos);
```

## V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Extracted method >= 15 LOC (extraction floor)
- [ ] Method CYC target: ≤3 (no branching, pure initialization)
- [ ] No logic drift -- preserve exact struct field assignments, especially `EntryOrder = null`

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

# 5. Verify EntryOrder = null semantic (CRITICAL for REAPER)
grep -n "EntryOrder = null" src/V12_002.SIMA.Lifecycle.cs
```

## Acceptance Criteria
- [ ] `CreateRecoveryFSM()` method created with correct signature
- [ ] All FSM struct fields initialized correctly
- [ ] `State` always set to `FollowerBracketState.Active` (recovery semantic)
- [ ] `RemainingContracts` calculated from live position quantity with `Math.Abs()`
- [ ] `EntryOrder` field set to `null` (terminal entry marker - CRITICAL)
- [ ] `LastUpdateUtc` uses `DateTime.UtcNow` at creation time
- [ ] `Targets` array initialized to size 5
- [ ] Parent method updated to call helper
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows CYC ≤3 for `CreateRecoveryFSM`
- [ ] lock() audit: ZERO matches
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible

## Estimated Effort
**30 minutes** (LOW risk, pure initialization similar to ticket-03)