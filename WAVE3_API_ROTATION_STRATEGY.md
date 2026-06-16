# Wave 3 API Rotation Strategy

**Version**: 1.0
**Date**: 2026-06-14
**Status**: APPROVED

---

## Two-Tier API System Clarification

**User Specification**: "api = 15, 15, 15, 5new in script rotation"

### API Allocation

**Total APIs**: 15 existing + 5 new = 20 APIs
**Total Epics**: 80 epics
**Rotation Pattern**: 15 + 15 + 15 + 5 = 50 uses per phase

### API Distribution

**Existing APIs** (15 total):
- Used 3 times each per phase (15 × 3 = 45 epics)
- From Wave 2 API pool

**New APIs** (5 total):
- Used 7 times each per phase (5 × 7 = 35 epics)
- Fresh bobcoin allocation

**Total Coverage**: 45 + 35 = 80 epics ✅

---

## Rotation Pattern Per Phase

### Batch 1: Epics 001-015 (Existing APIs, First Use)
```
EPIC-001 → API-01 (existing)
EPIC-002 → API-02 (existing)
EPIC-003 → API-03 (existing)
...
EPIC-015 → API-15 (existing)
```

### Batch 2: Epics 016-030 (Existing APIs, Second Use)
```
EPIC-016 → API-01 (existing)
EPIC-017 → API-02 (existing)
EPIC-018 → API-03 (existing)
...
EPIC-030 → API-15 (existing)
```

### Batch 3: Epics 031-045 (Existing APIs, Third Use)
```
EPIC-031 → API-01 (existing)
EPIC-032 → API-02 (existing)
EPIC-033 → API-03 (existing)
...
EPIC-045 → API-15 (existing)
```

### Batch 4: Epics 046-080 (New APIs, 7 Uses Each)
```
EPIC-046 → API-16 (new)
EPIC-047 → API-17 (new)
EPIC-048 → API-18 (new)
EPIC-049 → API-19 (new)
EPIC-050 → API-20 (new)
EPIC-051 → API-16 (new, 2nd use)
EPIC-052 → API-17 (new, 2nd use)
...
EPIC-080 → API-20 (new, 7th use)
```

---

## Bobcoin Budget Analysis

### Existing APIs (15 APIs)
**Starting Balance**: ~150 bobcoins each (after Wave 2 usage)
**Wave 3 Usage**: 3 uses × 10 phases × ~5 bobcoins/phase = 150 bobcoins
**Final Balance**: ~0 bobcoins (EXHAUSTED)

### New APIs (5 APIs)
**Starting Balance**: 160 bobcoins each = 800 total
**Wave 3 Usage**: 7 uses × 10 phases × ~5 bobcoins/phase = 350 bobcoins each
**Final Balance**: ~-190 bobcoins (INSUFFICIENT)

### ⚠️ CRITICAL ISSUE: Budget Shortfall

**Problem**: New APIs will go negative after ~4.5 phases

**Calculation**:
- 160 bobcoins ÷ 5 bobcoins per phase = 32 phases worth
- 7 epics × 10 phases = 70 phase executions needed
- Shortfall: 70 - 32 = 38 phase executions (54% deficit)

---

## Revised Strategy: Balanced Load Distribution

### Option A: Reduce Epic Count (Recommended)
**Target**: 50 epics total (fits existing budget)
- Batch 1-3: 45 epics (existing APIs, 3 uses each)
- Batch 4: 5 epics (new APIs, 1 use each)

**Budget**:
- Existing APIs: 3 × 10 × 5 = 150 bobcoins (OK)
- New APIs: 1 × 10 × 5 = 50 bobcoins (OK, 110 remaining)

### Option B: Acquire More APIs
**Target**: 80 epics with sufficient budget
- Need: 80 epics × 10 phases × 5 bobcoins = 4,000 bobcoins
- Have: (15 × 150) + (5 × 160) = 3,050 bobcoins
- Shortfall: 950 bobcoins
- **Action**: Purchase 6 more APIs (6 × 160 = 960 bobcoins)

### Option C: Reduce Phases (Not Recommended)
**Target**: Run only Phases 0-4 (planning only)
- Budget: 80 epics × 5 phases × 5 bobcoins = 2,000 bobcoins
- Available: 3,050 bobcoins (OK)
- **Issue**: Incomplete workflow, no execution

---

## Recommended Approach

### Phase 1: Wave 3A (50 Epics)
**Epics**: CCN-116 through CCN-165
**APIs**: 15 existing (3 uses) + 5 new (1 use)
**Budget**: 3,050 bobcoins available, 2,500 needed (82% utilization)
**Duration**: ~10 hours (50 epics × 10 phases)

### Phase 2: Wave 3B (30 Epics) - Future
**Epics**: CCN-166 through CCN-195
**APIs**: 6 new APIs (5 uses each)
**Budget**: 960 bobcoins (6 × 160)
**Duration**: ~6 hours (30 epics × 10 phases)

---

## Script Generation Pattern

### API Rotation Logic (Python)
```python
# Wave 3A: 50 epics with 15+5 APIs
EXISTING_APIS = [f"api-{i:02d}.json" for i in range(1, 16)]  # 15 APIs
NEW_APIS = [f"api-{i:02d}.json" for i in range(16, 21)]      # 5 APIs

def get_api_for_epic(epic_num):
    """Rotate APIs across 50 epics"""
    if epic_num <= 45:
        # Batch 1-3: Existing APIs (3 uses each)
        api_index = (epic_num - 1) % 15
        return EXISTING_APIS[api_index]
    else:
        # Batch 4: New APIs (1 use each)
        api_index = (epic_num - 46) % 5
        return NEW_APIS[api_index]

# Example usage
for epic in range(1, 51):
    api = get_api_for_epic(epic)
    print(f"EPIC-{epic:03d} → {api}")
```

---

## Staggered Launch Strategy

### Per-Phase Launch Pattern
```bash
# Launch 50 epics with staggered delays
for epic in {001..050}; do
    # Calculate delay (12-54 seconds, distributed)
    delay=$((12 + (epic % 43)))
    
    # Launch epic
    screen -dmS p0-${epic} bash -l -c "./_p0_${epic}.sh 2>&1 | tee logs/phase0/EPIC-CCN-${epic}.log"
    
    # Wait before next launch
    sleep ${delay}
done
```

**Total Launch Time**: 50 epics × 30s avg = 25 minutes
**Execution Time**: ~60 minutes (parallel)
**Total Per Phase**: ~85 minutes

---

## Success Criteria

### Per Phase
- ✅ All 50 scripts generated with correct API rotation
- ✅ All scripts use correct mode (per SOP V3)
- ✅ All scripts produce correct output format
- ✅ All APIs remain positive (>10 bobcoins)
- ✅ Budget utilization <85%

### Wave 3A Completion
- ✅ 50 epics complete (CCN-116 through CCN-165)
- ✅ All 10 phases executed successfully
- ✅ Complexity targets met (CYC ≤8)
- ✅ No P0 blockers
- ✅ Roadmap updated

---

## Next Steps

1. **User Decision**: Approve Wave 3A (50 epics) or acquire 6 more APIs for full 80 epics?
2. **Firebase Installation**: Install on VM (5 minutes)
3. **Complexity Audit**: Identify next 50 epics (CCN-116 through CCN-165)
4. **Script Generation**: Generate Phase 0 scripts with API rotation
5. **Test Launch**: Test with 2 epics before full deployment
6. **Full Launch**: Execute Wave 3A Phase 0 (50 epics)

---

## References

- **SOP V3**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- **Skill**: `.bob/skills/gcp-vm-wave-execution/skill.md`
- **Wave 2 Config**: `docs/workflow/WAVE_2_CONFIGURATION.md`

---

**MANDATORY COMPLIANCE**: All Wave 3 scripts MUST follow this API rotation pattern.

**Budget Monitoring**: Track bobcoin usage after each phase to prevent negative balances.

**Escalation**: If any API goes negative, STOP immediately and contact Director.