# Phase 1: Scope Definition - EPIC-W7-100

## Agent Tracking
- **Agent Name**: v12-phase1-scope
- **Bobcoins Used**: 0.18
- **API Key**: jCodemunch MCP
- **Execution Time**: 2026-06-24T20:09:28Z

## Target Method
- **Method**: ClosePositionsOnlyApexAccounts
- **File**: src/V12_002.SIMA.Flatten.cs
- **Line**: 516
- **Current CYC**: 11
- **Target CYC**: ≤8 (Jane Street threshold)

## Scope Boundary Validation

### IN SCOPE ✅

**Primary Extraction Target**:
- **Decision Logic Extraction**: Extract nested if/else blocks that contribute to CYC=11
- **Account Classification Logic**: Extract IsFleetAccount checks and related branching
- **Flatten Operation Queueing**: Extract logic for adding operations to _pendingFlattenOps
- **State Management**: Extract SetExpectedPositionLocked and related state updates

**Rationale**:
- Zero blast radius (no external callers) = safe to refactor aggressively
- CYC=11 is only 3 points above threshold = achievable with 2-3 extractions
- Orchestration pattern preserved by extracting decision logic, not flow control
- All extractions remain within V12_002.SIMA.Flatten.cs (no cross-file changes)

**Extraction Candidates** (based on 29 callees):
1. **Account Type Decision Logic** - Extract IsFleetAccount branching
2. **Flatten Operation Selection** - Extract logic choosing between cancel/close/fallback
3. **Grace Period Management** - Extract StampAccountFillGrace logic
4. **Operation Chaining** - Extract ChainNextFlattenOp decision logic

### OUT OF SCOPE ❌

**Explicitly Excluded**:
- **Orchestration Flow**: Do NOT extract PumpFlattenOps, PerformFallbackFlatten, ProcessFlattenWorkItem_* calls (preserve call sequence)
- **Logging Infrastructure**: Do NOT extract LogBuffer.Format calls (keep inline for debugging)
- **Cross-File Changes**: Do NOT modify callers (none exist) or callees (29 methods)
- **Behavioral Changes**: Do NOT alter logic, only restructure for CYC reduction
- **Test File Changes**: Do NOT modify test files (add new tests only)

**Rationale**:
- Orchestration flow is the method's core responsibility (must remain intact)
- Logging is diagnostic, not business logic (keep inline)
- Zero blast radius means no caller updates needed
- Behavioral preservation is V12 DNA mandate

### Scope Metrics

**Complexity Reduction Target**:
- **Current**: CYC=11, Nesting=4, LOC=74
- **Target**: CYC≤8, Nesting≤3, LOC≤60
- **Reduction Required**: -3 CYC points minimum

**Extraction Budget**:
- **Max Extractions**: 3-4 helper methods
- **Max New LOC**: +30 lines (helper methods)
- **Net LOC Change**: -14 lines (74 → 60 in main method, +30 in helpers = +16 total)

**Risk Tolerance**:
- **Blast Radius**: ZERO (no external impact)
- **Churn Risk**: MEDIUM (12 commits in 90 days)
- **Test Coverage**: REQUIRED (add tests before extraction)

## Boundary Enforcement

### Jane Street Alignment
- **Correctness by Construction**: Extract decision logic to make illegal states unrepresentable
- **Cognitive Simplicity**: Each extracted method must have single responsibility
- **Lock-Free Pattern**: Preserve FSM/Actor Enqueue model (no new locks)
- **ASCII-Only**: All new code must be ASCII-compliant

### V12 DNA Compliance
- **CYC ≤ 8**: All methods (main + helpers) must meet threshold
- **No Scope Creep**: Do NOT fix unrelated issues in this file
- **Hard-Link Integrity**: Run deploy-sync.ps1 after changes
- **Build Verification**: F5 in NinjaTrader must succeed

## Success Criteria

**Phase 1 Complete When**:
- ✅ Scope boundary clearly defined (IN vs OUT)
- ✅ Extraction candidates identified (3-4 methods)
- ✅ Complexity reduction path validated (CYC 11→8)
- ✅ Risk assessment confirms LOW-MEDIUM (acceptable)
- ✅ No cross-file changes required (blast radius = 0)

**Ready for Phase 2 (Architecture Planning)**:
- Scope boundary validated by Sequential Thinking MCP
- Extraction candidates mapped to helper method signatures
- Test strategy defined (unit tests for each extraction)
- Orchestration flow preservation strategy documented

---
**Phase 1 Status**: ✅ COMPLETED
**Generated**: 2026-06-24T20:09:28Z
**Next Phase**: Phase 1.5 (Scope Boundary Validation via Sequential Thinking MCP)
