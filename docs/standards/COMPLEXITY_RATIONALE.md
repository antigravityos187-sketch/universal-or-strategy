# Complexity Threshold Rationale

**Effective**: 2026-06-03 (Phase 3 - Jane Street Integration)  
**Previous Threshold**: CYC ≤ 15  
**New Threshold**: CYC ≤ 10 (acceptable) / CYC ≤ 8 (strict)

---

## Executive Summary

V12 Universal OR Strategy has updated its cyclomatic complexity threshold from **15 → 10** (with strict mode at **8**) to align with Jane Street's high-frequency trading (HFT) system principles. This change prioritizes **cognitive simplicity** over clever abstractions, enabling microsecond-latency reasoning and exhaustive testing.

---

## Why 8/10 vs 15?

### Jane Street HFT Systems Context

Jane Street's trading systems operate under **microsecond latency constraints** where:
- Every function must be **instantly comprehensible** under time pressure
- Complex control flow creates **exponential test path growth**
- Race conditions in lock-free code are **harder to audit** in complex functions
- Performance regressions are **easier to introduce** in high-complexity code

### Cognitive Load Reduction

**CYC > 10** functions are harder to:
1. **Reason about** under microsecond latency constraints
2. **Test exhaustively** (exponential path growth: CYC 15 = 32,768 paths)
3. **Audit for race conditions** in lock-free Actor/FSM patterns
4. **Optimize** without introducing subtle bugs

### V12 DNA Alignment

V12's core principle: **"Make illegal states unrepresentable"**

This requires:
- **Simple, verifiable logic** (not clever abstractions)
- **Explicit state machines** (not implicit control flow)
- **Single-responsibility functions** (not God functions)

High complexity violates this principle by creating **implicit state** through complex control flow.

---

## Threshold Breakdown

| Threshold | Classification | Action Required |
|-----------|----------------|-----------------|
| **CYC ≤ 8** | Strict (Jane Street) | Ideal for hot paths and lock-free code |
| **CYC 9-10** | Acceptable | Watch list - consider refactoring |
| **CYC 11-15** | Technical Debt | Refactor in next sprint |
| **CYC > 15** | Critical | Immediate refactoring required |

---

## Lizard Tool (Codacy) Threshold

**Lizard** (used by Codacy) has a **hardcoded threshold of 8**.

**Our Strategy**:
- Treat Lizard warnings (CYC 9-13) as **technical debt visibility**, not blockers
- Track in **EPIC-CCN-10** backlog for future refactoring to CYC ≤ 10
- Use `--strict` flag in `complexity_audit.py` for CYC ≤ 8 enforcement

---

## Implementation

### Tools Updated

1. **`scripts/complexity_audit.py`**:
   - Default threshold: `--threshold 10`
   - Strict mode: `--strict` (enforces CYC ≤ 8)
   - CI gate: `--fail-on-violation`

2. **`.codacy.yml`**:
   - `complexity.threshold: 10`
   - Comment references this document

3. **`pre_push_validation.ps1`**:
   - Check #9: Complexity audit with threshold 10
   - Blocks push if violations detected

### Usage Examples

```powershell
# Standard check (CYC ≤ 10)
python scripts/complexity_audit.py --fail-on-violation

# Strict Jane Street check (CYC ≤ 8)
python scripts/complexity_audit.py --strict --fail-on-violation

# Report mode (no blocking)
python scripts/complexity_audit.py
```

---

## Migration Strategy

### Phase 1: Baseline (Complete)
- Update thresholds in tooling
- Document rationale (this file)
- No immediate code changes required

### Phase 2: Incremental Refactoring (Ongoing)
- **Boy Scout Rule**: Fix complexity in files you touch
- **EPIC-CCN-10**: Dedicated epic for hotspot refactoring
- **Priority**: Hot paths first (OnBarUpdate, SIMA dispatch, order callbacks)

### Phase 3: Strict Enforcement (Future)
- After EPIC-CCN-10 complete
- Enable `--strict` mode in CI
- Target: 100% of codebase at CYC ≤ 8

---

## Current Baseline (2026-06-03)

**Before Phase 3**:
- 32% of files exceed CYC 15 (31/207 files)
- Total methods: ~500
- Methods > CYC 15: ~45

**After Phase 3** (Target):
- 0% of files exceed CYC 10
- Methods > CYC 10: 0
- Methods CYC 9-10: <10 (watch list)

---

## References

1. **Jane Street Performance Patterns**: [`docs/standards/jane-street/JANE_STREET_PERFORMANCE_PATTERNS.md`](jane-street/JANE_STREET_PERFORMANCE_PATTERNS.md)
2. **Jane Street Core Patterns**: [`docs/standards/jane-street/JANE_STREET_CORE_PATTERNS.md`](jane-street/JANE_STREET_CORE_PATTERNS.md)
3. **Codacy Configuration**: [`.codacy.yml`](../../.codacy.yml)
4. **Complexity Audit Script**: [`scripts/complexity_audit.py`](../../scripts/complexity_audit.py)

---

## Decision Log

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-05-22 | CYC ≤ 15 | Initial Codacy integration |
| 2026-06-03 | CYC ≤ 10 (strict: 8) | Jane Street alignment (Phase 3) |

---

**Approved By**: V12 Architecture Team  
**Review Date**: 2026-06-03  
**Next Review**: After EPIC-CCN-10 completion

---

_Made with Bob_