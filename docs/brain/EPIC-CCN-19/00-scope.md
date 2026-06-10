# EPIC-CCN-19: Scope Definition

**Epic ID**: EPIC-CCN-19  
**Target**: CheckFFMAConditions (CYC 16 → ≤8)  
**Phase**: 1 - Scope Definition  
**Created**: 2026-06-09T20:06:20Z

---

## In Scope

### Primary Target
- ✅ **Method**: `CheckFFMAConditions()` (lines 43-106)
- ✅ **File**: `src/V12_002.Entries.FFMA.cs`
- ✅ **Objective**: Extract 4 helper methods to reduce CYC from 16 to ≤8

### Extraction Targets

**1. IsFFMAReadyToCheck() - Guard Validation**
- Lines: 45-50 (guard clauses)
- CYC: 2
- Purpose: Consolidate all readiness checks

**2. GetFFMAMarketData() - Data Extraction**
- Lines: 54-59 (variable setup)
- CYC: 1
- Purpose: Extract market data into tuple

**3. CheckFFMAShortSetup() - SHORT Logic**
- Lines: 62-76 (SHORT condition + execution)
- CYC: 4
- Purpose: Isolate SHORT entry logic

**4. CheckFFMALongSetup() - LONG Logic**
- Lines: 79-93 (LONG condition + execution)
- CYC: 4
- Purpose: Isolate LONG entry logic

### Testing Requirements
- ✅ TDD tests for each helper method
- ✅ Integration test for refactored CheckFFMAConditions()
- ✅ Existing FFMA tests must pass unchanged

### Documentation Updates
- ✅ Update method summary comments
- ✅ Add XML documentation for new helpers
- ✅ Update EPIC-CCN-19 completion report

---

## Out of Scope

### Methods NOT Modified
- ❌ `ExecuteFFMAEntry()` - Already well-structured (CYC acceptable)
- ❌ `DeactivateFFMAMode()` - Simple method (CYC 1)
- ❌ `ExecuteFFMALimitEntry()` - Separate manual entry path
- ❌ `ExecuteFFMAManualMarketEntry()` - Separate manual entry path

### Files NOT Modified
- ❌ `src/V12_002.BarUpdate.cs` - Only calls CheckFFMAConditions (no changes needed)
- ❌ Any other FFMA-related files

### Features NOT Changed
- ❌ FFMA entry logic (RSI + EMA distance + candle color)
- ❌ Stop calculation (entry candle high/low)
- ❌ Target distribution (T1-T5 ladder)
- ❌ SIMA dispatch behavior
- ❌ Panel notification (FFMA_DISARMED message)

### Quality Gates NOT Relaxed
- ❌ ASCII-only compliance (maintained)
- ❌ Lock-free pattern (no locks introduced)
- ❌ Exception handling (preserved)
- ❌ Build verification (F5 must pass)

---

## Scope Boundary Validation

### File-Level Isolation
- ✅ **Single File**: Only `V12_002.Entries.FFMA.cs` modified
- ✅ **No Cross-File Changes**: No changes to callers or dependencies
- ✅ **Parallel Safe**: FFMA file is in SIMA cluster (safe for parallel execution)

### Method-Level Isolation
- ✅ **Single Method**: Only `CheckFFMAConditions()` refactored
- ✅ **Pure Extraction**: Zero logic changes (structural only)
- ✅ **No Side Effects**: No state mutations introduced

### Dependency Analysis
**Upstream Dependencies** (methods called by CheckFFMAConditions):
- `ema9[0]` - Indicator access (read-only)
- `rsiIndicator[0]` - Indicator access (read-only)
- `Close[0]`, `Open[0]`, `High[0]`, `Low[0]` - Bar data (read-only)
- `CalculatePositionSize()` - Position sizing (no changes)
- `ExecuteFFMAEntry()` - Entry execution (no changes)

**Downstream Dependencies** (methods that call CheckFFMAConditions):
- `V12_002.BarUpdate.cs::OnBarUpdate()` - Line 334 (no changes needed)

**Verdict**: ✅ **ISOLATED** - No ripple effects expected

---

## Risk Assessment

### Technical Risks
| Risk | Severity | Mitigation |
|------|----------|------------|
| Logic drift during extraction | LOW | TDD tests + line-by-line verification |
| Tuple syntax compatibility | LOW | C# 7.0+ feature (NinjaTrader 8 supports) |
| Performance regression | LOW | Inline candidates (JIT will optimize) |
| Test coverage gaps | LOW | Write tests before extraction |

### Process Risks
| Risk | Severity | Mitigation |
|------|----------|------------|
| Scope creep | LOW | Strict boundary enforcement (Phase 1.5) |
| Merge conflicts | LOW | SIMA cluster isolation (parallel safe) |
| Build failures | LOW | Pre-push validation + F5 gate |

**Overall Risk**: ✅ **LOW** - Pure structural refactoring with strong isolation

---

## Success Criteria

### Functional Requirements
- ✅ CheckFFMAConditions() CYC reduced from 16 to ≤8
- ✅ All 4 helper methods have CYC ≤4
- ✅ FFMA entry behavior unchanged (SHORT/LONG triggers identical)
- ✅ Stop calculation unchanged (entry candle high/low)
- ✅ Target distribution unchanged (T1-T5 ladder)

### Quality Requirements
- ✅ All unit tests passing (new + existing)
- ✅ NinjaTrader F5 verification passed
- ✅ ASCII-only compliance maintained
- ✅ No new Jane Street P0 violations
- ✅ BUILD_TAG updated

### Documentation Requirements
- ✅ XML documentation for all new methods
- ✅ Ticket completion report generated
- ✅ Verification report generated
- ✅ Manifest updated with completion status

---

## Execution Plan

### Phase 5.1: Ticket 1 Execution
1. Write TDD tests for IsFFMAReadyToCheck()
2. Write TDD tests for GetFFMAMarketData()
3. Write TDD tests for CheckFFMAShortSetup()
4. Write TDD tests for CheckFFMALongSetup()
5. Extract IsFFMAReadyToCheck() method
6. Extract GetFFMAMarketData() method
7. Extract CheckFFMAShortSetup() method
8. Extract CheckFFMALongSetup() method
9. Refactor CheckFFMAConditions() to use helpers
10. Run complexity audit (verify CYC ≤8)
11. Run unit tests (verify all passing)
12. Update BUILD_TAG
13. Run deploy-sync.ps1
14. F5 verification in NinjaTrader

### Phase 5.1.V: Ticket 1 Verification
1. Verify CYC reduction (16 → ≤8)
2. Verify all tests passing
3. Verify F5 successful
4. Verify no new P0 violations
5. Generate completion report

---

## Parallel Execution Context

**Cluster**: SIMA (Cluster 1)  
**Worktree**: `C:\WSGTA\universal-or-epic-cluster-1`  
**Branch**: `epic-cluster-1`  
**Parallel Safe**: ✅ YES (no file conflicts with Clusters 2/3)

**Concurrent Epics** (if running in parallel):
- EPIC-CCN-20: Orders cluster (different files)
- EPIC-CCN-21: Lifecycle cluster (different files)

---

## Approval Gates

### Phase 1 Gate: Scope Definition
- ✅ Single file modification confirmed
- ✅ No cross-file dependencies
- ✅ Parallel execution safe
- ✅ Risk assessment complete

**Status**: ✅ **APPROVED** - Proceed to Phase 1.5 (Scope Boundary Validation)

---

**Phase Status**: Phase 1 COMPLETE  
**Next Phase**: Scope Boundary Validation (Phase 1.5)
