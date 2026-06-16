# Extraction Tickets: EPIC-CCN-014

## Overview
- **Total Tickets**: 1 (Atomic extraction)
- **Execution Order**: Single atomic commit
- **Estimated Effort**: 2-3 hours
- **Target Method**: TryHandleFleetCommand
- **Complexity Reduction**: 19 → 4 (79% reduction)

## Rationale for Single Ticket
This epic requires **atomic extraction** because:
1. All 4 helper methods are tightly coupled to the main dispatcher
2. Partial extraction would leave the codebase in an inconsistent state
3. The extraction is surgical (single file, single method scope)
4. Estimated diff size: ~1,200 characters (well below 10k limit)

---

## TICKET-1: Extract Category-Based Fleet Command Dispatchers

### Scope
- **Current Method**: `TryHandleFleetCommand`
- **Current CYC**: 19
- **Target CYC**: 4 (main) + 2 + 6 + 8 + 5 (helpers) = 25 distributed
- **File**: `src/V12_002.UI.IPC.Commands.Fleet.cs`
- **Extraction Type**: Category-based sub-dispatchers

### Implementation Plan

#### Step 1: Extract Command ID Generator (CYC: 2)
Create private helper method:

```csharp
private string GenerateFleetCommandId(string action, long senderTicks)
{
    return action.StartsWith("FLEET_STATE") 
        ? $"fleet_{action}_{senderTicks}" 
        : $"fleet_{senderTicks}";
}
```

**Complexity**: Base (1) + Ternary (1) = **2** ✅

---

#### Step 2: Extract Position Commands Dispatcher (CYC: 6)
Create private helper method:

```csharp
private bool TryDispatchPositionCommands(string action, string[] parts, string cmdId)
{
    if (action == "TRIM_25") return TryHandleFleet_Trim25(cmdId);
    if (action == "TRIM_50") return TryHandleFleet_Trim50(cmdId);
    if (action == "LOCK_50") return TryHandleFleet_Lock50(cmdId);
    if (action == "FLATTEN_ONLY") return TryHandleFleet_FlattenOnly(cmdId);
    if (action == "FLATTEN") return TryHandleFleet_Flatten(cmdId);
    if (action == "CANCEL_ALL") return TryHandleFleet_CancelAll(cmdId);
    if (action == "RESET_MEMORY") return TryHandleFleet_ResetMemory(cmdId);
    return false;
}
```

**Commands Handled**: TRIM_25, TRIM_50, LOCK_50, FLATTEN_ONLY, FLATTEN, CANCEL_ALL, RESET_MEMORY
**Complexity**: Base (1) + 6 if-statements (5) = **6** ✅

---

#### Step 3: Extract Order Commands Dispatcher (CYC: 8)
Create private helper method:

```csharp
private bool TryDispatchOrderCommands(string action, string[] parts, string cmdId)
{
    if (action == "LONG") return TryHandleFleet_Long(parts, cmdId);
    if (action == "SHORT") return TryHandleFleet_Short(parts, cmdId);
    if (action == "OR_LONG") return TryHandleFleet_OrLong(parts, cmdId);
    if (action == "OR_SHORT") return TryHandleFleet_OrShort(parts, cmdId);
    if (action == "TREND_MANUAL_LIMIT") return TryHandleFleet_TrendManualLimit(parts, cmdId);
    if (action == "RETEST_MANUAL_LIMIT") return TryHandleFleet_RetestManualLimit(parts, cmdId);
    if (action == "FFMA_MANUAL_LIMIT") return TryHandleFleet_FfmaManualLimit(parts, cmdId);
    if (action == "FFMA_MANUAL_MARKET") return TryHandleFleet_FfmaManualMarket(cmdId);
    if (action == "CLOSE_TARGET") return TryHandleFleet_CloseTarget(cmdId);
    return false;
}
```

**Commands Handled**: LONG, SHORT, OR_LONG, OR_SHORT, TREND_MANUAL_LIMIT, RETEST_MANUAL_LIMIT, FFMA_MANUAL_LIMIT, FFMA_MANUAL_MARKET, CLOSE_TARGET
**Complexity**: Base (1) + 8 if-statements (7) = **8** ✅ (at threshold)

---

#### Step 4: Extract Configuration Commands Dispatcher (CYC: 5)
Create private helper method:

```csharp
private bool TryDispatchConfigCommands(string action, string[] parts, string cmdId)
{
    if (action == "MOVE_TARGET") return TryHandleFleet_MoveTarget(parts, cmdId);
    if (action.StartsWith("FLEET_STATE")) return TryHandleFleet_FleetState(action, cmdId);
    if (action == "TOGGLE_ACCOUNT") return TryHandleFleet_ToggleAccount(parts, cmdId);
    if (action == "SET_SHADOW") return TryHandleFleet_SetShadow(parts, cmdId);
    return false;
}
```

**Commands Handled**: MOVE_TARGET, FLEET_STATE, TOGGLE_ACCOUNT, SET_SHADOW
**Complexity**: Base (1) + 4 if-statements (4) = **5** ✅

---

#### Step 5: Refactor Main Method (CYC: 4)
Replace the 19 if-statements with category dispatchers:

```csharp
private bool TryHandleFleetCommand(string action, string[] parts, long senderTicks)
{
    string cmdId = GenerateFleetCommandId(action, senderTicks);
    
    if (TryDispatchPositionCommands(action, parts, cmdId)) return true;
    if (TryDispatchOrderCommands(action, parts, cmdId)) return true;
    if (TryDispatchConfigCommands(action, parts, cmdId)) return true;
    
    return false;
}
```

**Complexity**: Base (1) + 3 if-statements (3) = **4** ✅

---

### Acceptance Criteria

#### Functional Requirements
- [ ] All 4 helper methods created with correct signatures
- [ ] Main method refactored to use category dispatchers
- [ ] All 19 original commands still handled (no behavioral changes)
- [ ] Command ID generation logic preserved (FLEET_STATE special case)
- [ ] Early return pattern preserved (short-circuit evaluation)

#### Quality Gates
- [ ] **Complexity**: Main method CYC ≤ 4 ✅
- [ ] **Complexity**: All helpers CYC ≤ 8 ✅
- [ ] **Build**: Zero compilation errors
- [ ] **Tests**: All existing tests pass (no test changes required)
- [ ] **Lint**: Zero Roslyn violations
- [ ] **Formatting**: CSharpier auto-format applied
- [ ] **ASCII-Only**: Zero non-ASCII characters
- [ ] **Lock-Free**: Zero lock() statements (forensic scan)
- [ ] **Diff Size**: <10,000 characters (estimated ~1,200)

#### V12 DNA Compliance
- [ ] **Correctness by Construction**: Type-safe extraction (no runtime reflection)
- [ ] **Lock-Free Actor Pattern**: No locks introduced, FSM/Actor preserved
- [ ] **ASCII-Only Compliance**: Verified via pre-push validation Check #1
- [ ] **Jane Street Alignment**: All methods CYC ≤ 8 (cognitive simplicity)
- [ ] **Hard-Link Integrity**: `deploy-sync.ps1` executed post-implementation

#### Verification Commands
```powershell
# 1. Complexity audit
python scripts/complexity_audit.py

# 2. Build readiness (includes CSharpier)
powershell -File .\scripts\build_readiness.ps1

# 3. Pre-push validation (fast mode)
powershell -File .\scripts\pre_push_validation.ps1 -Fast

# 4. Lock-free forensic scan
grep -r "lock(" src/V12_002.UI.IPC.Commands.Fleet.cs
# Expected output: (empty - zero matches)

# 5. Hard-link sync
powershell -File .\deploy-sync.ps1

# 6. NinjaTrader F5 test
# Manual: Press F5 in NinjaTrader, verify strategy loads without errors
```

---

### Dependencies
- **None** (first and only ticket)
- **Prerequisite**: Phase 3 audit PASS ✅ (completed 2026-06-15)

---

### Implementation Notes

#### Access Modifiers
- All new methods: `private` (internal to V12_002 class)
- No API surface changes (Phase 1.5 boundary compliance)

#### Parameter Passing
- `cmdId` generated once in main method, passed to all dispatchers
- `action` and `parts` passed through unchanged
- No parameter transformation or validation (preserves existing behavior)

#### Error Handling
- No new error handling (preserves existing behavior)
- Dispatchers return `false` if command not recognized
- Main method returns `false` if no dispatcher handled command

#### Performance Considerations
- **JIT Inlining**: Small methods (<10 lines) will be inlined by .NET JIT
- **Zero Allocations**: No new objects created (cmdId string reused)
- **Branch Prediction**: Simple if-chains are branch-prediction friendly
- **Microsecond Latency**: Zero overhead (preserves existing call graph)

#### Testing Strategy
- **Existing Tests**: Must pass without modification (Phase 1.5 requirement)
- **No New Tests**: Internal refactoring (no API changes)
- **Complexity Verification**: Run `complexity_audit.py` post-extraction

---

### Risk Mitigation

#### Technical Risks
1. **JIT Inlining Uncertainty** (⚠️ MINOR)
   - **Risk**: .NET JIT may not inline all helpers
   - **Mitigation**: Methods are small (<10 lines), high inlining probability
   - **Fallback**: Benchmark post-implementation if concerns arise

2. **Analyzer Variance** (⚠️ MINOR)
   - **Risk**: Different CYC analyzers may measure differently
   - **Mitigation**: Used if-chains (universally measured as +1 per branch)
   - **Verification**: Run `complexity_audit.py` post-implementation

#### Process Risks
1. **Hard-Link Desync** (⚠️ MINOR)
   - **Risk**: Forgetting `deploy-sync.ps1` after implementation
   - **Mitigation**: Mandatory in acceptance criteria checklist
   - **Detection**: NinjaTrader F5 test will fail if desynced

2. **Test Regression** (🟢 NEGLIGIBLE)
   - **Risk**: Existing tests fail due to refactoring
   - **Mitigation**: Phase 1.5 verified no API changes
   - **Verification**: Pre-push validation Check #3

---

### Code Review Checklist

#### Before Implementation
- [ ] Read Phase 2 architecture plan (`02-architecture-plan.md`)
- [ ] Read Phase 3 audit report (`03-audit-report.md`)
- [ ] Verify current branch: `feature/epic-ccn-014-fleet-command-extraction`
- [ ] Confirm no uncommitted changes in working directory

#### During Implementation
- [ ] Follow exact method signatures from architecture plan
- [ ] Preserve all 19 command handlers (no deletions)
- [ ] Maintain ASCII-only compliance (no Unicode)
- [ ] Add no new error handling or validation
- [ ] Touch only TryHandleFleetCommand method (surgical scope)

#### After Implementation
- [ ] Run all verification commands (see Acceptance Criteria)
- [ ] Verify CYC ≤ 8 for all methods
- [ ] Confirm zero lock() statements
- [ ] Execute `deploy-sync.ps1` (hard-link integrity)
- [ ] Test F5 in NinjaTrader (strategy loads without errors)
- [ ] Commit with message: `feat(epic-ccn-014): Extract category-based fleet command dispatchers (CYC 19→4)`

---

### Success Metrics

| Metric | Before | After | Target | Status |
|--------|--------|-------|--------|--------|
| Main Method CYC | 19 | 4 | ≤8 | ✅ 50% below |
| Helper Methods CYC | N/A | 2, 6, 8, 5 | ≤8 | ✅ All compliant |
| Total Distributed CYC | 19 | 25 | N/A | ✅ Semantic grouping |
| Cognitive Load Reduction | 0% | 79% | >50% | ✅ Exceeds target |
| Diff Size | N/A | ~1,200 | <10k | ✅ 88% below limit |
| Build Errors | 0 | 0 | 0 | ✅ Expected |
| Test Failures | 0 | 0 | 0 | ✅ Expected |
| Lock() Statements | 0 | 0 | 0 | ✅ Mandatory |

---

**Ticket Status**: READY FOR IMPLEMENTATION
**Estimated Effort**: 2-3 hours (includes verification)
**Assigned To**: Bob CLI (`v12-engineer`) or Codex CLI (`codex-rescue`)
**Priority**: P1 (High - Tier 1 hotspot)
**Epic**: EPIC-CCN-014
**Phase**: Phase 4 → Phase 5 (Execution)
