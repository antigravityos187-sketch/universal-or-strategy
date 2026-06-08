---
# TICKET EPIC-CCN-1-06: ScanForRecoveryKey Position Recovery Extraction
# Epic: EPIC-CCN-1
# Sequence: 6 of 7
# Depends on: NONE (independent of Pass 1 tickets)
---

## Objective
Extract the orphaned position detection logic for Pass 2 into a dedicated method to isolate the REAPER grace window handling and improve testability

## Scope
IN scope:
- Extract `ScanForRecoveryKey()` method from lines 624-656
- Search `stopOrders` dictionary for account match
- Handle REAPER grace window for CancelPending stops (Build 999 feature)
- Update `_positionPassFailedFirstSeen` dictionary
- File: `src/V12_002.SIMA.Lifecycle.cs`

OUT of scope:
- Recovery FSM creation (separate ticket-07)
- Pass 1 logic (already extracted in tickets 01-05)
- REAPER flatten logic (separate subsystem)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-1/01-analysis.md`](01-analysis.md) -- Section "Risk Hotspots #2: REAPER Grace Window Logic"
- Approach: [`docs/brain/EPIC-CCN-1/02-approach.md`](02-approach.md) -- Section "2. Target State / Sub-Methods to Create #6"

## Implementation Instructions

### Extract New Method
Create private method with signature:
```csharp
private (string recoveredKey, Order recoveredStop) ScanForRecoveryKey(
    Account account
)
```

**Scan Logic** (from lines 624-656):
1. Loop over `stopOrders.ToArray()` to find matching account
2. For each stop order:
   - Check if `order.Account?.Name` matches `account.Name` (case-insensitive)
   - If match found, return `(kvp.Key, order)` immediately
3. If no match found:
   - Call `Print()` with exact warning message: `"[SIMA] Phase 5 Position Pass: WARNING -- open position for {account.Name} but no stop order found. Starting REAPER grace window."`
   - Update `_positionPassFailedFirstSeen[account.Name] = DateTime.UtcNow`
   - Return `(null, null)`

**CRITICAL**: The warning message text is load-bearing for REAPER integration. Do NOT modify it.

**Estimated LOC**: 35-40 lines

### Update Parent Method Call Site (Lines 624-656)
Replace inline logic with:
```csharp
var (recoveredKey, recoveredStop) = ScanForRecoveryKey(acct);
if (recoveredKey == null) continue; // Grace window started, skip FSM creation
```

## V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Extracted method >= 15 LOC (extraction floor)
- [ ] Method CYC target: ≤6 (1 foreach + 2 null checks + 1 account comparison + 1 early return + 1 grace window path)
- [ ] No logic drift -- preserve exact warning message and grace window initialization

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

# 5. REAPER warning message verification (must match exactly)
grep -n "Phase 5 Position Pass: WARNING" src/V12_002.SIMA.Lifecycle.cs
```

## Acceptance Criteria
- [ ] `ScanForRecoveryKey()` method created with correct signature
- [ ] Loop over `stopOrders` to find account match
- [ ] Account name comparison is case-insensitive
- [ ] Early return with `(recoveredKey, recoveredStop)` when match found
- [ ] REAPER warning message is EXACT match (no modifications)
- [ ] `_positionPassFailedFirstSeen` dictionary updated with `DateTime.UtcNow`
- [ ] Return `(null, null)` when no match found
- [ ] Parent method updated to call helper and handle tuple return
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows CYC ≤6 for `ScanForRecoveryKey`
- [ ] lock() audit: ZERO matches
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible

## Estimated Effort
**45 minutes** (MEDIUM risk, complex loop with early-exit logic and REAPER integration)