# Jane Street Refactoring Roadmap

**Date**: 2026-06-03  
**Baseline**: 347 P0 violations, 1,838 P1 violations, 31 P2 violations  
**Target**: 0 P0 violations by end of EPIC-CCN-22  
**Strategy**: Systematic refactoring across 9 epics (EPIC-CCN-14 through EPIC-CCN-22)

---

## Overview

This roadmap maps Jane Street rule violations to the existing EPIC-CCN series (complexity reduction epics). Each epic will address a specific category of violations while performing the planned complexity extraction work.

**Key Principle**: Fix violations in files being touched for complexity reduction. Don't create separate "fix violations" PRs - integrate fixes into extraction work.

---

## Epic Mapping

### EPIC-CCN-14: ShouldSkipFleet_RunHealthCheck
**Primary Goal**: Extract method with CYC 20 → CYC ≤ 15  
**File**: `src/V12_002.UI.Compliance.cs`

**Jane Street Violations to Fix**:
- **Magic Numbers**: ~10 violations (build tags, thresholds)
- **Long Method**: 1 violation (method itself)
- **Null Usage**: 0 violations (clean on this front)

**Refactoring Actions**:
1. Extract magic numbers to constants:
   - `1104.1` → `BUILD_TAG_OCO_GUARD`
   - Threshold values → Named constants
2. Split method into smaller functions (CYC ≤ 15 each)
3. Verify no new violations introduced

**Estimated Effort**: 4 hours  
**P0 Fixes**: ~10 violations  
**Expected Baseline After**: P0 = 337

---

### EPIC-CCN-15: UI Helper Methods Extraction
**Primary Goal**: Extract 3 long methods from `V12_002.UI.Panel.Helpers.cs`  
**File**: `src/V12_002.UI.Panel.Helpers.cs`

**Jane Street Violations to Fix**:
- **Null Usage (JS-002)**: 20 violations (highest concentration)
- **Magic Numbers**: ~30 violations (UI dimensions, Z-index values)
- **Long Methods**: 3 violations

**Refactoring Actions**:
1. **Phase 1**: Extract magic numbers
   - UI dimensions → `UIConstants.cs`
   - Z-index values → Named constants
2. **Phase 2**: Replace null returns with Option<T>
   - `GetLiveTargetCtsBlock()` → `Option<TextBlock>`
   - `FindChartTraderButton()` → `Option<Button>`
   - All UI element lookups → Option<T>
3. **Phase 3**: Split long methods (CYC ≤ 15)

**Estimated Effort**: 8 hours  
**P0 Fixes**: ~50 violations  
**Expected Baseline After**: P0 = 287

**Type Safety Migration**:
- Add `Option<T>` type to codebase (if not exists)
- Create extension methods for WPF element traversal
- Update all callers to handle Option<T>

---

### EPIC-CCN-16: State Sync Lock Removal
**Primary Goal**: Remove lock usage from `V12_002.UI.Panel.StateSync.cs`  
**File**: `src/V12_002.UI.Panel.StateSync.cs`

**Jane Street Violations to Fix**:
- **Lock Usage (JS-021)**: 2 violations (CRITICAL)
- **Null Usage**: 2 violations
- **Magic Numbers**: ~40 violations
- **Long Methods**: 4 violations

**Refactoring Actions**:
1. **Phase 1**: Convert to Actor pattern
   - Replace `lock(stateLock)` with Channel-based message passing
   - Create `UIStateSyncActor` with message queue
   - Ensure atomic state updates
2. **Phase 2**: Fix null returns → Option<T>
3. **Phase 3**: Extract magic numbers (EMA periods, percentages)
4. **Phase 4**: Split long methods

**Estimated Effort**: 12 hours (lock removal is complex)  
**P0 Fixes**: ~44 violations  
**Expected Baseline After**: P0 = 243

**Concurrency Pattern**:
```csharp
// BEFORE
lock(stateLock) {
    TextBlock block = GetLiveTargetCtsBlock(t);
    if (block != null) block.Text = value;
}

// AFTER
await _uiSyncActor.SendAsync(new UpdateTargetCtsMessage(t, value));
```

---

### EPIC-CCN-17: Exception-to-Result Conversion
**Primary Goal**: Convert hot-path exceptions to Result<T,E>  
**Files**: `src/SignalBroadcaster.cs`, `src/V12_002.IO.PathValidation.cs`

**Jane Street Violations to Fix**:
- **Exception Usage (JS-001)**: 3 violations
- **Mutable Shared State**: 13 violations (SignalBroadcaster)
- **Magic Numbers**: ~10 violations

**Refactoring Actions**:
1. **Phase 1**: Add Result<T,E> type to codebase
2. **Phase 2**: Convert SignalBroadcaster methods
   - `BroadcastTradeSignal()` → `Result<Unit, SignalError>`
   - `BroadcastTrailUpdate()` → `Result<Unit, SignalError>`
   - `BroadcastTargetAction()` → `Result<Unit, SignalError>`
3. **Phase 3**: Convert PathValidation methods
   - `ValidatePath()` → `Result<string, PathError>`
4. **Phase 4**: Update all callers to handle Result<T,E>

**Estimated Effort**: 6 hours  
**P0 Fixes**: ~16 violations  
**Expected Baseline After**: P0 = 227

**Error Type Design**:
```csharp
public enum SignalError {
    InvalidSignalId,
    NullSignal,
    BroadcastFailed
}

public enum PathError {
    InvalidPath,
    SecurityViolation,
    PathNotFound
}
```

---

### EPIC-CCN-18: Drawing Helpers Refactoring
**Primary Goal**: Extract complexity from `V12_002.DrawingHelpers.cs`  
**File**: `src/V12_002.DrawingHelpers.cs`

**Jane Street Violations to Fix**:
- **Null Usage**: 2 violations
- **Magic Numbers**: ~30 violations (hash function constants, color values)

**Refactoring Actions**:
1. Extract hash function constants:
   - `2166136261` → `FNV_OFFSET_BASIS`
   - `16777619` → `FNV_PRIME`
2. Replace null returns with Option<T>
3. Extract color/drawing magic numbers

**Estimated Effort**: 4 hours  
**P0 Fixes**: ~32 violations  
**Expected Baseline After**: P0 = 195

---

### EPIC-CCN-19: IPC Actor Pattern Migration
**Primary Goal**: Convert IPC to Actor pattern  
**Files**: `src/V12_002.UI.IPC.cs`, `src/V12_002.UI.IPC.Server.cs`

**Jane Street Violations to Fix**:
- **Null Usage**: 6 violations
- **Magic Numbers**: ~25 violations (buffer sizes, queue depths)
- **ArrayPool Usage (JS-037)**: 2 violations

**Refactoring Actions**:
1. **Phase 1**: Extract buffer/queue constants
   - `8192` → `IPC_MAX_BUFFER_SIZE`
   - `2000` → `IPC_MAX_QUEUE_DEPTH`
   - `500` → `IPC_MAX_COMMANDS_PER_DRAIN`
2. **Phase 2**: Replace `new byte[]` with ArrayPool<T>
3. **Phase 3**: Convert null returns to Option<T>
4. **Phase 4**: Refactor to Actor pattern (if needed)

**Estimated Effort**: 8 hours  
**P0 Fixes**: ~33 violations  
**Expected Baseline After**: P0 = 162

**ArrayPool Pattern**:
```csharp
// BEFORE
byte[] buffer = new byte[4096];

// AFTER
byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);
try {
    // use buffer
} finally {
    ArrayPool<byte>.Shared.Return(buffer);
}
```

---

### EPIC-CCN-20: SignalBroadcaster Actor Conversion
**Primary Goal**: Convert static SignalBroadcaster to Actor pattern  
**File**: `src/SignalBroadcaster.cs`

**Jane Street Violations to Fix**:
- **Mutable Shared State**: 48 violations (entire class)
- **Magic Numbers**: ~5 violations

**Refactoring Actions**:
1. **Phase 1**: Design Actor-based SignalBroadcaster
   - Replace static events with Channel<T>
   - Create instance-based broadcaster
   - Inject via DI (if possible in NinjaTrader)
2. **Phase 2**: Migrate all event subscribers to Channel readers
3. **Phase 3**: Update all broadcast calls to async
4. **Phase 4**: Remove static state

**Estimated Effort**: 16 hours (major architectural change)  
**P0 Fixes**: ~53 violations  
**Expected Baseline After**: P0 = 109

**Actor Design**:
```csharp
public sealed class SignalBroadcaster {
    private readonly Channel<TradeSignal> _tradeSignals;
    private readonly Channel<TrailUpdateSignal> _trailUpdates;
    // ... other channels
    
    public ChannelReader<TradeSignal> TradeSignals => _tradeSignals.Reader;
    
    public async Task BroadcastTradeSignalAsync(TradeSignal signal) {
        await _tradeSignals.Writer.WriteAsync(signal);
    }
}
```

---

### EPIC-CCN-21: UI Panel Brushes Refactoring
**Primary Goal**: Extract RGB color constants  
**File**: `src/V12_002.UI.Panel.Brushes.cs`

**Jane Street Violations to Fix**:
- **Magic Numbers**: 120+ violations (all RGB values)

**Refactoring Actions**:
1. Create color constants file:
   ```csharp
   public static class UIColors {
       public const byte TEXT_PRIMARY_R = 220;
       public const byte TEXT_PRIMARY_G = 220;
       public const byte TEXT_PRIMARY_B = 220;
       // ... all other colors
   }
   ```
2. Update all `PanelBrush()` calls to use constants
3. Consider color palette design (group related colors)

**Estimated Effort**: 4 hours  
**P0 Fixes**: ~120 violations  
**Expected Baseline After**: P0 = -11 (NEGATIVE! Need to recount)

**Note**: This epic alone eliminates most magic number violations. Actual P0 after this should be ~50-60 (other categories).

---

### EPIC-CCN-22: Final Cleanup and Validation
**Primary Goal**: Address remaining P0 violations  
**Files**: Various (remaining hotspots)

**Jane Street Violations to Fix**:
- **Remaining P0 violations**: ~50-60 (after EPIC-CCN-21 recount)
- **Categories**: Performance (Span<T>), remaining null usage, misc magic numbers

**Refactoring Actions**:
1. **Phase 1**: Audit remaining P0 violations
2. **Phase 2**: Fix performance violations (Span<T> usage)
3. **Phase 3**: Fix remaining null returns
4. **Phase 4**: Extract remaining magic numbers
5. **Phase 5**: Run full validation suite
6. **Phase 6**: Generate final audit report

**Estimated Effort**: 8 hours  
**P0 Fixes**: ~50-60 violations  
**Expected Baseline After**: P0 = 0 ✅

---

## Quick Wins (Low-Hanging Fruit)

These can be done opportunistically during any epic:

### 1. UI Panel Brushes (EPIC-CCN-21)
**Effort**: 4 hours  
**Impact**: 120+ P0 violations fixed  
**Risk**: Low (pure constant extraction)  
**Recommendation**: Do this FIRST in EPIC-CCN-14 or EPIC-CCN-15

### 2. Build Tags and Version Strings
**Files**: Multiple  
**Effort**: 2 hours  
**Impact**: ~20 P0 violations  
**Risk**: Very Low  
**Recommendation**: Include in EPIC-CCN-14

### 3. Buffer Size Constants (IPC)
**File**: `V12_002.UI.IPC.cs`  
**Effort**: 1 hour  
**Impact**: ~10 P0 violations  
**Risk**: Very Low  
**Recommendation**: Include in EPIC-CCN-19

---

## Complex Cases (High-Risk)

These require careful planning and testing:

### 1. SignalBroadcaster Actor Conversion (EPIC-CCN-20)
**Risk**: HIGH - affects entire signal distribution system  
**Testing Required**: Full integration testing  
**Rollback Plan**: Keep static version as fallback  
**Recommendation**: Dedicate full epic, extensive testing

### 2. Lock Removal (EPIC-CCN-16)
**Risk**: HIGH - concurrency bugs are subtle  
**Testing Required**: Stress testing, race condition testing  
**Rollback Plan**: Revert to lock-based version  
**Recommendation**: Use Arena AI adversarial testing

### 3. Null-to-Option Migration (EPIC-CCN-15, EPIC-CCN-18)
**Risk**: MEDIUM - affects many call sites  
**Testing Required**: Null reference exception testing  
**Rollback Plan**: Gradual migration, keep null version temporarily  
**Recommendation**: Migrate high-risk areas first (UI helpers)

---

## Effort Estimation

| Epic | Primary Goal | P0 Fixes | Effort (hrs) | Risk |
|------|--------------|----------|--------------|------|
| EPIC-CCN-14 | Health Check Extraction | ~10 | 4 | Low |
| EPIC-CCN-15 | UI Helpers + Option<T> | ~50 | 8 | Medium |
| EPIC-CCN-16 | Lock Removal | ~44 | 12 | High |
| EPIC-CCN-17 | Exception-to-Result | ~16 | 6 | Medium |
| EPIC-CCN-18 | Drawing Helpers | ~32 | 4 | Low |
| EPIC-CCN-19 | IPC Actor + ArrayPool | ~33 | 8 | Medium |
| EPIC-CCN-20 | SignalBroadcaster Actor | ~53 | 16 | High |
| EPIC-CCN-21 | UI Brushes Constants | ~120 | 4 | Low |
| EPIC-CCN-22 | Final Cleanup | ~50 | 8 | Low |
| **TOTAL** | | **~408** | **70** | |

**Note**: Total P0 fixes (408) exceeds baseline (347) due to double-counting and estimation buffer. Actual fixes will converge to 347.

---

## P1 Violations Strategy

**Approach**: Opportunistic fixes during P0 work  
**Target**: 73% reduction (1,838 → <500)

### High-Value P1 Fixes

1. **Long Methods** (38 violations)
   - Fix automatically during complexity extraction
   - Every epic should reduce long method count
   - Target: 0 long methods after EPIC-CCN-22

2. **Magic Numbers** (1,800+ violations)
   - Fix alongside P0 magic number work
   - Create module-specific constants files
   - Target: <200 after EPIC-CCN-22

---

## Testing Strategy

### Per-Epic Testing
1. **Unit Tests**: Add tests for extracted methods
2. **Integration Tests**: Verify no regressions
3. **Validation**: Run jane_street_validator.py + jane_street_rule_checker.py
4. **Pre-Push**: Full pre_push_validation.ps1

### High-Risk Epic Testing (EPIC-CCN-16, EPIC-CCN-20)
1. **Stress Testing**: 1000+ concurrent operations
2. **Race Condition Testing**: Thread sanitizer, Arena AI adversarial
3. **Performance Testing**: Ensure no latency regression
4. **Rollback Testing**: Verify fallback mechanisms work

---

## Monitoring and Reporting

### After Each Epic
1. Re-run full audit: `python scripts/jane_street_validator.py src/ --json`
2. Update baseline in `JANE_STREET_BASELINE_AUDIT.md`
3. Generate progress chart (P0 violations over time)
4. Report to Director

### Progress Tracking
```
EPIC-CCN-14: P0 = 337 (10 fixed, 2.9% progress)
EPIC-CCN-15: P0 = 287 (50 fixed, 17.3% progress)
EPIC-CCN-16: P0 = 243 (44 fixed, 30.0% progress)
EPIC-CCN-17: P0 = 227 (16 fixed, 34.6% progress)
EPIC-CCN-18: P0 = 195 (32 fixed, 43.8% progress)
EPIC-CCN-19: P0 = 162 (33 fixed, 53.3% progress)
EPIC-CCN-20: P0 = 109 (53 fixed, 68.6% progress)
EPIC-CCN-21: P0 = ~50 (59 fixed, 85.6% progress)
EPIC-CCN-22: P0 = 0 (50 fixed, 100% progress) ✅
```

---

## Risk Mitigation

### Rollback Strategy
1. **Git Branches**: Each epic on separate branch
2. **Checkpointing**: Bob CLI auto-checkpointing enabled
3. **Fallback Code**: Keep old implementations commented for 1 sprint
4. **Feature Flags**: Use for high-risk changes (SignalBroadcaster)

### Code Review
1. **Arena AI**: Adversarial review for concurrency changes
2. **Director**: Manual review for architectural changes
3. **Automated**: Pre-push validation blocks P0 violations

---

## Dependencies

### Type System Additions
- `Option<T>` type (EPIC-CCN-15)
- `Result<T,E>` type (EPIC-CCN-17)
- Error enums (SignalError, PathError, etc.)

### Infrastructure
- ArrayPool<T> usage patterns (EPIC-CCN-19)
- Channel-based Actor pattern (EPIC-CCN-16, EPIC-CCN-20)
- Constants files per module

---

## Success Criteria

### Per-Epic
- ✅ P0 violations reduced by target amount
- ✅ No new P0 violations introduced
- ✅ All tests pass
- ✅ Pre-push validation passes
- ✅ Code review approved

### Overall (Post-EPIC-CCN-22)
- ✅ P0 violations = 0
- ✅ P1 violations < 500
- ✅ P2 violations < 10
- ✅ Clean files > 20
- ✅ 100% Jane Street compliance on P0 rules

---

## Timeline

**Assuming 1 epic per week**:
- Week 1: EPIC-CCN-14 (Health Check)
- Week 2: EPIC-CCN-15 (UI Helpers + Option<T>)
- Week 3: EPIC-CCN-16 (Lock Removal) ⚠️ High Risk
- Week 4: EPIC-CCN-17 (Exception-to-Result)
- Week 5: EPIC-CCN-18 (Drawing Helpers)
- Week 6: EPIC-CCN-19 (IPC Actor + ArrayPool)
- Week 7-8: EPIC-CCN-20 (SignalBroadcaster Actor) ⚠️ High Risk
- Week 9: EPIC-CCN-21 (UI Brushes - Quick Win)
- Week 10: EPIC-CCN-22 (Final Cleanup)

**Total Duration**: 10 weeks (2.5 months)  
**Target Completion**: 2026-08-15

---

## Appendix: Rule Reference

### P0 Rules Being Fixed
- **JS-001**: Result<T,E> instead of exceptions
- **JS-002**: Option<T> instead of null
- **JS-021**: No lock() usage
- **JS-036**: Span<T> for zero-allocation
- **JS-037**: ArrayPool<T> for buffers
- **JS-100**: No magic numbers
- **MUTABLE_SHARED_STATE**: Actor pattern for shared state
- **LONG_METHOD**: Methods ≤ 50 lines

### Related Documentation
- **Rules Catalog**: `docs/standards/jane-street/RULES_CATALOG.md`
- **Baseline Audit**: `docs/brain/JANE_STREET_BASELINE_AUDIT.md`
- **Agent Guide**: `docs/training/JANE_STREET_AGENT_GUIDE.md`
- **Pre-Push Validation**: `docs/protocol/PRE_PUSH_VALIDATION.md`

---

**Roadmap Version**: 1.0  
**Last Updated**: 2026-06-03  
**Next Review**: After EPIC-CCN-14 completion