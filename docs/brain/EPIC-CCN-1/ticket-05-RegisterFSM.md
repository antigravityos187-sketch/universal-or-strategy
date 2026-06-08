---
# TICKET EPIC-CCN-1-05: RegisterFSM Extraction
# Epic: EPIC-CCN-1
# Sequence: 5 of 7
# Depends on: ticket-04-LinkStopOrder.md
---

## Objective
Extract the FSM registration logic for Pass 1 into a dedicated method to consolidate dictionary insertion, entry order indexing, and counter updates

## Scope
IN scope:
- Extract `RegisterFSM()` method from lines 590-598
- Add FSM to `_followerBrackets` dictionary
- Index entry order in `_orderIdToFsmKey`
- Increment both `ordersIndexed` and `fsmCreated` counters
- File: `src/V12_002.SIMA.Lifecycle.cs`

OUT of scope:
- Pass 2 FSM registration (remains inline due to different logging semantics)
- Order linking logic (already extracted)
- FSM creation logic (already extracted)

## Context References
- Analysis: [`docs/brain/EPIC-CCN-1/01-analysis.md`](01-analysis.md) -- Section "Shared State Dependencies / Mutated State"
- Approach: [`docs/brain/EPIC-CCN-1/02-approach.md`](02-approach.md) -- Section "2. Target State / Sub-Methods to Create #5"

## Implementation Instructions

### Extract New Method
Create private method with signature:
```csharp
private void RegisterFSM(
    string entryKey,
    FollowerBracketFSM fsm,
    ref int ordersIndexed,
    ref int fsmCreated
)
```

**Registration Logic** (from lines 590-598):
1. TryAdd FSM to `_followerBrackets[entryKey]`
2. If entry order OrderId is not null/empty:
   - Index in `_orderIdToFsmKey[fsm.EntryOrder.OrderId] = entryKey`
   - Increment `ordersIndexed` counter
3. Increment `fsmCreated` counter

**Estimated LOC**: 15-20 lines

### Update Parent Method Call Site (Lines 590-598)
Replace inline logic with:
```csharp
RegisterFSM(entryKey, fsm, ref ordersIndexed, ref fsmCreated);
```

### Note on Pass 2
**DO NOT** extract Pass 2 FSM registration (lines 707-737). It has different semantics:
- Different logging format (includes account name and recovered key)
- Uses `positionFsmCreated` counter (separate from `fsmCreated`)
- Keep inline for clarity

## V12 DNA Guardrails
- [ ] Zero new lock() statements
- [ ] Zero non-ASCII characters in string literals
- [ ] Extracted method >= 15 LOC (extraction floor)
- [ ] Method CYC target: ≤2 (1 string check + 1 TryAdd)
- [ ] No logic drift -- preserve exact dictionary insertion and indexing

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
- [ ] `RegisterFSM()` method created with correct signature
- [ ] FSM added to `_followerBrackets` dictionary via TryAdd
- [ ] Entry order indexed in `_orderIdToFsmKey` if OrderId exists
- [ ] Both `ordersIndexed` and `fsmCreated` counters incremented correctly
- [ ] Pass 1 call site updated to use helper
- [ ] Pass 2 registration remains inline (NOT extracted)
- [ ] deploy-sync.ps1 ASCII gate: PASS
- [ ] complexity_audit.py shows CYC ≤2 for `RegisterFSM`
- [ ] lock() audit: ZERO matches
- [ ] Director presses F5 in NinjaTrader -- BUILD_TAG banner visible

## Estimated Effort
**20 minutes** (LOW risk, simple dictionary operation)