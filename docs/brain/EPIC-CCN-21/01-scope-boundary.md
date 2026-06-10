# EPIC-CCN-21: Scope Boundary Definition

**Epic ID**: EPIC-CCN-21  
**Method**: `OnBarUpdate`  
**File**: `src/V12_002.BarUpdate.cs`  
**Current CYC**: 10  
**Target CYC**: ≤8  
**Status**: Phase 1 - Scope Definition  
**Date**: 2026-06-09

---

## Scope Boundary (V12.23 No Scope Creep Protocol)

### ✅ IN SCOPE: Extraction Targets ONLY

**Ticket 1: Extract Pending Entry Processing**
- Extract `ProcessPendingTRENDEntry()` helper
  - Lines 323-327 (pending TREND entry logic)
- Extract `ProcessFFMAConditionCheck()` helper
  - Lines 378-381 (FFMA armed check)
- **CYC Reduction**: -2 points (10 → 8)

**Ticket 2: Extract Position Management Logic**
- Extract `UpdateATRFromSecondaryBars()` helper
  - Lines 330-333 (ATR update from 5-min bars)
- Extract `ProcessActivePositionManagement()` helper
  - Lines 370-375 (trailing stops + CIT management)
- **CYC Reduction**: -2 points (fallback if Ticket 1 insufficient)

**Scope Constraints**:
- Pure structural refactoring ONLY
- Zero logic changes
- Zero behavior changes
- Zero pre-existing bug fixes
- Zero "while we're here" improvements

---

### ❌ OUT OF SCOPE: Explicitly Excluded

**Pre-Existing Issues** (DO NOT FIX):
- Any compilation errors in other files
- Any warnings in other files
- Any dead code in other files
- Any formatting issues in other files

**Adjacent Code** (DO NOT TOUCH):
- Already-extracted helpers (DrawMNLAnchorIfActive, ProcessSessionReset, etc.)
- Try-catch-finally block structure
- Guard conditions (BarsInProgress, CurrentBar)
- Latency instrumentation (LatencyProbe)
- IPC command processing
- Mailbox draining
- Position sync logic
- UI snapshot publishing

**Improvements** (DO NOT ADD):
- Performance optimizations
- Additional logging
- Code style changes
- Comment improvements
- Variable renaming
- Whitespace changes

**Testing** (SEPARATE EPIC):
- Unit test additions (will be added in TDD workflow)
- Integration test changes
- Benchmark updates

---

## Dependencies

### Internal Dependencies
- **None** - Extractions are self-contained
- All helper methods will be private instance methods
- No changes to method signatures
- No changes to field access patterns

### External Dependencies
- **None** - No changes to NinjaTrader API usage
- No changes to order lifecycle
- No changes to FSM state transitions
- No changes to IPC protocol

### Build Dependencies
- `dotnet build` must pass
- `deploy-sync.ps1` must execute successfully
- F5 in NinjaTrader must load strategy
- BUILD_TAG must appear in output

---

## Risks

### Risk 1: Logic Drift During Extraction
**Probability**: LOW  
**Impact**: HIGH (P0 blocker)  
**Mitigation**:
- Use exact copy-paste for extraction
- Verify zero diff in logic
- Run full test suite after each extraction
- F5 verification after each ticket

### Risk 2: Scope Creep
**Probability**: MEDIUM (historical pattern)  
**Impact**: HIGH (PR rejection, rollback)  
**Mitigation**:
- Strict adherence to V12.23 protocol
- ONE EPIC = ONE CONCERN
- Separate PR for any unrelated fixes
- Director approval required for scope changes

### Risk 3: Build Breakage
**Probability**: LOW  
**Impact**: MEDIUM (delays epic)  
**Mitigation**:
- Run `build_readiness.ps1` after each extraction
- Run `deploy-sync.ps1` after each extraction
- Verify BUILD_TAG after each extraction
- Rollback via restore points if needed

---

## Success Criteria

### Per-Ticket Success
- [ ] Helper method extracted with CYC≤8
- [ ] Parent method CYC reduced
- [ ] Zero logic drift (exact copy-paste)
- [ ] Build passes (`build_readiness.ps1`)
- [ ] Deploy-sync passes (`deploy-sync.ps1`)
- [ ] F5 verification passes (BUILD_TAG appears)
- [ ] No compilation errors
- [ ] No new warnings

### Epic Success
- [ ] CYC reduced from 10 to ≤8
- [ ] All extracted methods have CYC≤8
- [ ] All tickets completed
- [ ] All F5 verifications passed
- [ ] Manifest updated (status: "completed")
- [ ] Roadmap updated (final_cyc: ≤8)

---

## Scope Validation Checklist

Before starting implementation, verify:
- [ ] Scope clearly defined (extraction targets only)
- [ ] Out-of-scope items explicitly listed
- [ ] No pre-existing fixes included
- [ ] No "while we're here" improvements
- [ ] Dependencies identified (none)
- [ ] Risks assessed and mitigated
- [ ] Success criteria clear and measurable

---

## Approval Gate

**Orchestrator**: Scope boundary defined. Ready to proceed to Phase 2 (Architecture Planning)?

**User Confirmation Required**: Press ENTER to approve scope or provide feedback.
