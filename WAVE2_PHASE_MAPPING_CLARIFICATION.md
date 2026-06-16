# Wave 2 Phase Mapping Clarification

**Date**: 2026-06-14
**Purpose**: Document actual Wave 2 phase structure after Phase 1 + 1.5 merge
**Status**: VERIFIED

---

## Critical Discovery

Wave 2 underwent a phase consolidation where **Phase 1 (Scope Definition) and Phase 1.5 (Scope Boundary) were merged**. This affected the file numbering but NOT the phase script numbering.

---

## Wave 2 Actual Structure

### Phase Scripts (What We Copy From)

| Script | Phase Name | Output File | Mode |
|--------|------------|-------------|------|
| `generate_phase1_scripts.py` | Scope Definition | `00-scope.md` | `plan` |
| `generate_phase2_scripts.py` | Architecture Planning | `02-architecture-plan.md` | `plan` |
| `generate_phase3_scripts.py` | DNA & PR Audit | `03-audit-report.md` | `advanced` |
| `generate_phase4_scripts.py` | Ticket Generation | `04-tickets.md` | `plan` |
| `generate_phase5_scripts.py` | Ticket Execution | `ticket-X-completion.md` | `v12-engineer` |

### Output Files (What Gets Created)

| File | Created By | Phase Number |
|------|------------|--------------|
| `00-hotspots.md` | Phase 0 | 0 |
| `00-scope.md` | Phase 1 | 1 |
| `01-scope-boundary.md` | Phase 1 (merged) | 1.5 |
| `02-architecture-plan.md` | Phase 2 | 2 |
| `03-audit-report.md` | Phase 3 | 3 |
| `04-tickets.md` | Phase 4 | 4 |

---

## The Confusion

### What Happened

**Phase 1 originally created**: `00-scope.md`
**Phase 1.5 originally created**: `01-scope-boundary.md`
**After merge**: Phase 1 creates BOTH files

**Phase 2 reads**: `01-scope-boundary.md` (line 48 of generate_phase2_scripts.py)
**Phase 2 creates**: `02-architecture-plan.md`

### Why This Matters

When copying Wave 2 scripts for Wave 3:
- ✅ **Script names are correct**: `generate_phase1_scripts.py` → `generate_phase2_scripts.py` → `generate_phase3_scripts.py`
- ✅ **Output file numbers are correct**: `00-scope.md` → `02-architecture-plan.md` → `03-audit-report.md`
- ⚠️ **Input file reference in Phase 2**: Reads `01-scope-boundary.md` (from merged Phase 1)

---

## Verification

### Phase 1 (Scope Definition)
```python
# From generate_phase1_scripts.py line 45
# Creates: docs/brain/EPIC-CCN-{epic_id}/00-scope.md
```

### Phase 2 (Architecture Planning)
```python
# From generate_phase2_scripts.py line 48
# Reads: docs/brain/EPIC-CCN-{epic_id}/01-scope-boundary.md
# Creates: docs/brain/EPIC-CCN-{epic_id}/02-architecture-plan.md
```

### Phase 3 (DNA & PR Audit)
```python
# From generate_phase3_scripts.py (verified in CORRECTED version)
# Reads: docs/brain/EPIC-CCN-{epic_id}/02-architecture-plan.md
# Creates: docs/brain/EPIC-CCN-{epic_id}/03-audit-report.md
```

### Phase 4 (Ticket Generation)
```python
# From generate_phase4_scripts.py line 56-57
# Reads: docs/brain/EPIC-CCN-{epic_id}/02-architecture-plan.md
# Reads: docs/brain/EPIC-CCN-{epic_id}/03-audit-report.md
# Creates: docs/brain/EPIC-CCN-{epic_id}/04-tickets.md
```

---

## Wave 3 Copying Strategy (VERIFIED CORRECT)

### For Wave 3 Phase 4

**Copy From**: `scripts/wave2/generate_phase4_scripts.py`
**Update**: Epic numbers only (107-115 → 116-125)
**Keep**: All input/output file references unchanged

**Input Files** (Phase 4 reads):
- `02-architecture-plan.md` ✅ (created by Wave 3 Phase 2)
- `03-audit-report.md` ✅ (created by Wave 3 Phase 3)

**Output File** (Phase 4 creates):
- `04-tickets.md` ✅

---

## Conclusion

**The Golden Rule is CORRECT**: Always copy same phase from previous wave.

**Wave 2 phase scripts are correctly numbered** despite the Phase 1 + 1.5 merge:
- Phase 1 script creates `00-scope.md` (and `01-scope-boundary.md` internally)
- Phase 2 script creates `02-architecture-plan.md`
- Phase 3 script creates `03-audit-report.md`
- Phase 4 script creates `04-tickets.md`

**No adjustments needed** for Wave 3 Phase 4 - copy Wave 2 Phase 4 exactly.

---

## References

- **Wave 2 Phase 1**: `scripts/wave2/generate_phase1_scripts.py`
- **Wave 2 Phase 2**: `scripts/wave2/generate_phase2_scripts.py`
- **Wave 2 Phase 3**: `scripts/wave2/generate_phase3_scripts.py`
- **Wave 2 Phase 4**: `scripts/wave2/generate_phase4_scripts.py`
- **Wave 3 Phase 3 Corrected**: `scripts/wave3/generate_wave3_phase3_scripts_CORRECTED.py`

---

**Status**: VERIFIED ✅
**Next Action**: Copy Wave 2 Phase 4 for Wave 3 Phase 4 (no adjustments needed)