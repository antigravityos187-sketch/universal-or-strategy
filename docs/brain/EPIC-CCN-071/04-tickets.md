# Extraction Tickets: EPIC-CCN-071

## Overview
- **Epic**: EPIC-CCN-071
- **Target Method**: `ShadowProcessFollowerStopUpdate`
- **File**: `src/V12_002.SIMA.Shadow.cs`
- **Current Complexity**: 12 (McCabe Cyclomatic)
- **Target Complexity**: ≤8 per method (Jane Street aligned)
- **Total Tickets**: 4
- **Execution Order**: Sequential (TICKET-1 → TICKET-2 → TICKET-3 → TICKET-4)
- **Estimated Effort**: 3-4 hours

## Strategy

Extract 3 helper methods to isolate distinct concerns, then refactor main method to orchestrate. Each helper targets CYC ≤4, main method targets CYC ≤3.

**Distributed Complexity**: 3 + 3 + 4 + 2 = 12 (same total, cognitively simpler)

---

## TICKET-1: Extract ValidateFollowerBracketExists Helper

### Scope
- **Current Method**: `ShadowProcessFollowerStopUpdate`
- **Current CYC**: 12
- **Extraction Target**: Validation of FSM and Position Existence (Lines 1-10)
- **New Helper CYC**: ~3
- **Remaining Main CYC**: ~9

### Implementation

1. **Create new private helper method**:
   ```csharp
   private (bool hasFsm, bool hasFollowerPos, FollowerBracketFSM fsm, PositionInfo followerPos) 
       ValidateFollowerBracketExists(string followerEntryName)
   ```

2. **Extract logic**:
   - Dictionary lookup for `_followerBrackets[followerEntryName]`
   - Dictionary lookup for `activePositions[followerEntryName]`
   - Return tuple with existence flags and instances

3. **Update main method**:
   - Replace inline validation with helper call
   - Destructure tuple result
   - Maintain identical early return behavior

4. **Verify**:
   - Run `python scripts/complexity_audit.py`
   - Confirm helper CYC ≤3
   - Confirm no lock() statements introduced

### Acceptance Criteria
- [ ] Helper method created with signature matching architecture plan
- [ ] Helper complexity CYC ≤3
- [ ] Main method updated to call helper
- [ ] Identical external behavior (same return values for all inputs)
- [ ] Build succeeds with zero errors
- [ ] No lock() statements introduced
- [ ] Complexity audit shows reduction

### Dependencies
- None (first ticket)

### Verification Command
```bash
python scripts/complexity_audit.py
dotnet build src/V12_002.csproj
```

---

## TICKET-2: Extract ValidateFollowerReadiness Helper

### Scope
- **Current Method**: `ShadowProcessFollowerStopUpdate`
- **Current CYC**: ~9 (after TICKET-1)
- **Extraction Target**: Validation of Follower Readiness State (Lines 12-22)
- **New Helper CYC**: ~4
- **Remaining Main CYC**: ~5

### Implementation

1. **Create new private helper method**:
   ```csharp
   private (bool isReady, bool waitingOnFollower) 
       ValidateFollowerReadiness(
           FollowerBracketFSM fsm, 
           PositionInfo followerPos,
           bool hasFsm,
           bool hasFollowerPos
       )
   ```

2. **Extract logic**:
   - Check `followerPos.EntryFilled` and `followerPos.BracketSubmitted`
   - Check `fsm.State == FollowerBracketState.Active`
   - Check `fsm.StopOrder != null`
   - Set `waitingOnFollower` flag for incomplete states

3. **Update main method**:
   - Replace inline readiness checks with helper call
   - Destructure tuple result
   - Maintain identical early return behavior

4. **Verify**:
   - Run `python scripts/complexity_audit.py`
   - Confirm helper CYC ≤4
   - Confirm main method CYC reduced

### Acceptance Criteria
- [ ] Helper method created with signature matching architecture plan
- [ ] Helper complexity CYC ≤4
- [ ] Main method updated to call helper
- [ ] Identical external behavior (waitingOnFollower flag set correctly)
- [ ] Build succeeds with zero errors
- [ ] No lock() statements introduced
- [ ] Complexity audit shows further reduction

### Dependencies
- **TICKET-1** must be completed first

### Verification Command
```bash
python scripts/complexity_audit.py
dotnet build src/V12_002.csproj
```

---

## TICKET-3: Extract ShouldUpdateFollowerStop Helper

### Scope
- **Current Method**: `ShadowProcessFollowerStopUpdate`
- **Current CYC**: ~5 (after TICKET-2)
- **Extraction Target**: Price Comparison and Update Decision (Lines 24-31)
- **New Helper CYC**: ~2
- **Remaining Main CYC**: ~3

### Implementation

1. **Create new private helper method**:
   ```csharp
   private bool ShouldUpdateFollowerStop(
       FollowerBracketFSM fsm,
       double newStopPrice
   )
   ```

2. **Extract logic**:
   - Compare `fsm.StopOrder.StopPrice` with `newStopPrice`
   - Apply tolerance threshold: `tickSize * 0.5`
   - Return true if update needed, false if already at target

3. **Update main method**:
   - Replace inline price comparison with helper call
   - Maintain identical early return behavior (skip if already at target)

4. **Verify**:
   - Run `python scripts/complexity_audit.py`
   - Confirm helper CYC ≤2
   - Confirm main method CYC ≤3

### Acceptance Criteria
- [ ] Helper method created with signature matching architecture plan
- [ ] Helper complexity CYC ≤2
- [ ] Main method updated to call helper
- [ ] Identical external behavior (same skip logic for price tolerance)
- [ ] Build succeeds with zero errors
- [ ] No lock() statements introduced
- [ ] Complexity audit shows main method CYC ≤3

### Dependencies
- **TICKET-1** must be completed first
- **TICKET-2** must be completed first

### Verification Command
```bash
python scripts/complexity_audit.py
dotnet build src/V12_002.csproj
```

---

## TICKET-4: Refactor Main Method Orchestration

### Scope
- **Current Method**: `ShadowProcessFollowerStopUpdate`
- **Current CYC**: ~3 (after TICKET-3)
- **Target**: Clean orchestration of helper methods
- **Final Main CYC**: ≤3

### Implementation

1. **Verify orchestration flow**:
   ```csharp
   private bool ShadowProcessFollowerStopUpdate(
       string followerEntryName,
       double newStopPrice,
       out bool waitingOnFollower
   )
   {
       // Step 1: Validate existence
       var (hasFsm, hasFollowerPos, fsm, followerPos) = 
           ValidateFollowerBracketExists(followerEntryName);
       
       if (!hasFsm && !hasFollowerPos) {
           waitingOnFollower = false;
           return false;
       }
       
       // Step 2: Validate readiness
       var (isReady, waiting) = 
           ValidateFollowerReadiness(fsm, followerPos, hasFsm, hasFollowerPos);
       waitingOnFollower = waiting;
       
       if (!isReady) {
           return true; // Pending, not ready yet
       }
       
       // Step 3: Check if update needed
       if (!ShouldUpdateFollowerStop(fsm, newStopPrice)) {
           return true; // Already at target price
       }
       
       // Step 4: Delegate to existing infrastructure
       return ShadowUpdateFollowerStop(followerEntryName, newStopPrice);
   }
   ```

2. **Code review checklist**:
   - Verify identical external behavior
   - Verify no logic changes (pure reorganization)
   - Verify no new state mutations
   - Verify no lock() statements

3. **Final verification**:
   - Run full complexity audit
   - Run build
   - Run tests (if any exist)
   - Verify all methods CYC ≤8

### Acceptance Criteria
- [ ] Main method complexity CYC ≤3
- [ ] All helper methods CYC ≤4
- [ ] Total distributed complexity = 12 (3+3+4+2)
- [ ] Identical external behavior verified
- [ ] Build succeeds with zero errors
- [ ] All tests pass (if tests exist)
- [ ] No lock() statements in any method
- [ ] Complexity audit shows all methods ≤8

### Dependencies
- **TICKET-1** must be completed first
- **TICKET-2** must be completed first
- **TICKET-3** must be completed first

### Verification Command
```bash
python scripts/complexity_audit.py
dotnet build src/V12_002.csproj
dotnet test tests/ --filter "FullyQualifiedName~Shadow"
```

---

## Success Criteria (Epic Level)

- ✅ All 4 tickets completed sequentially
- ✅ Main method complexity reduced from 12 to ≤3
- ✅ All helper methods have CYC ≤4
- ✅ Total distributed complexity = 12 (no logic added/removed)
- ✅ No lock() statements introduced
- ✅ External behavior unchanged (identical signature and return values)
- ✅ Build passes with zero errors
- ✅ Jane Street compliance: All methods CYC ≤8
- ✅ No scope creep (only ShadowProcessFollowerStopUpdate modified)

## Risk Mitigation

### Low Risk Factors
- ✅ Pure extraction (no logic changes)
- ✅ Identical interface (external callers unaffected)
- ✅ No new dependencies (uses existing infrastructure)
- ✅ Testable (each helper can be unit tested independently)

### Verification Strategy
- Run complexity audit after each ticket
- Verify build after each ticket
- Verify identical behavior through manual testing
- Run existing tests (if any) after final ticket

## Notes

- **Execution Time**: Estimate 45-60 minutes per ticket
- **Checkpointing**: Bob CLI auto-checkpoints after each ticket
- **Rollback**: Use `/restore` if any ticket introduces errors
- **Jane Street Alignment**: Cognitive simplicity over clever abstractions
- **Lock-Free**: All helpers are pure validation functions (no state mutations)
