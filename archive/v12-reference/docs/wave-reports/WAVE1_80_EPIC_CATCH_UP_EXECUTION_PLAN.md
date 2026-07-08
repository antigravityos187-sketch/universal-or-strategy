# Wave 1: 80-Epic Catch-Up & All-in-One Execution Plan

**Date**: 2026-06-14
**Strategy**: Catch up remaining 65 epics (Phase 0-1), then execute all 80 together (Phase 2-6)
**Workflow**: Jane Street compliant (custom modes)

---

## Validation: EPIC-001-015 Status ✅

### Files Verified
- ✅ **Phase 0**: 15 × `00-hotspots.md` files exist
- ✅ **Phase 1**: 15 × `00-scope.md` files exist
- ✅ **Workflow**: Jane Street compliant (verified EPIC-005)
- ✅ **Custom Modes**: Used new workflow with risk assessment

### Manifest Structure
```json
{
  "phases": {
    "1": "Scope Definition" (our Phase 1),
    "2": "Boundary Analysis" (our Phase 1.5),
    "3": "Implementation Plan" (our Phase 2),
    "4": "Execution" (our Phase 5),
    "5": "Validation" (our Phase 5.V + 6)
  }
}
```

**Note**: Manifest uses 5-phase model, but files use 0-6 naming (00-hotspots, 00-scope, etc.)

---

## 80-Epic Plan Overview

**Source**: `EPIC_ROADMAP_FINAL_V1.md`

### Category Breakdown
- **Category 1** (EPIC-001-015): 15 epics - Mixed Tier files ✅ Phase 0-1 DONE
- **Category 2** (EPIC-016-045): 30 epics - Pure Tier 1 (CYC ≥15) ⏳ Need Phase 0-1
- **Category 3** (EPIC-046-080): 35 epics - Pure Tier 2 (CYC 9-14) ⏳ Need Phase 0-1

**Total**: 80 epics
**Cost**: 6,400 bobcoins (80 × 80 bobcoins/epic)

---

## Execution Strategy: Catch-Up Then All-in-One

### Step 1: Catch Up Phase 0-1 (65 epics)

**Scope**: EPIC-016 through EPIC-080
**Timeline**: ~2 hours
- Phase 0: 65 epics × 2 min avg = 130 min (with staggered launch)
- Phase 1: 65 epics × 2 min avg = 130 min (with staggered launch)
- **Total**: ~4.3 hours (overlapping execution)

**Budget**: 65 × 8 bobcoins = 520 bobcoins
- Phase 0: 65 × 3 = 195 bobcoins
- Phase 1: 65 × 5 = 325 bobcoins

**Launch Pattern**:
```bash
# Phase 0: 10-second delays (fast phase)
for epic in 016-080; do
    launch_phase0_epic_$epic.sh &
    sleep 10
done

# Wait for completion (~2 hours)
# Verify: 80 total hotspot files

# Phase 1: 10-second delays (fast phase)
for epic in 016-080; do
    launch_phase1_epic_$epic.sh &
    sleep 10
done

# Wait for completion (~2 hours)
# Verify: 80 total scope files
```

### Step 2: SYNC POINT - All 80 Epics at Phase 1 Complete

**Verification**:
```bash
# Expect 80 files each
ls docs/brain/EPIC-*/00-hotspots.md | wc -l  # Should be 80
ls docs/brain/EPIC-*/00-scope.md | wc -l     # Should be 80
```

**Buffer**: 10 minutes
- Extract bobcoin usage from Phase 0-1
- Verify all 80 epics ready
- Check VM load returned to baseline

### Step 3: All-in-One Phase 2-6 (80 epics)

**Timeline**: ~9 hours
- Phase 2: 90 min (80 epics × 25 min avg / 50 concurrent) + 10 min buffer
- Phase 3: 60 min (80 epics × 20 min avg / 50 concurrent) + 2 min buffer
- Phase 4: 60 min (80 epics × 20 min avg / 50 concurrent) + 2 min buffer
- Phase 5: 180 min (80 epics × 45 min avg / 20 concurrent) + 5 min buffer
- Phase 5.V: 120 min (80 epics × 30 min avg / 20 concurrent) + 5 min buffer
- Phase 6: 60 min (80 epics × 20 min avg / 50 concurrent) + 2 min buffer

**Budget**: 80 × 72 bobcoins = 5,760 bobcoins
- Phase 2: 80 × 12 = 960 bobcoins
- Phase 3: 80 × 7 = 560 bobcoins
- Phase 4: 80 × 7 = 560 bobcoins
- Phase 5: 80 × 15 = 1,200 bobcoins
- Phase 5.V: 80 × 5 = 400 bobcoins
- Phase 6: 80 × 5 = 400 bobcoins
- **Subtotal**: 4,080 bobcoins

**Launch Pattern**:
```bash
# Phase 2: 15-second delays (medium phase)
for epic in 001-080; do
    launch_phase2_epic_$epic.sh &
    sleep 15
done
# Wait 90 min + 10 min buffer

# Phase 3: 10-second delays (fast phase)
for epic in 001-080; do
    launch_phase3_epic_$epic.sh &
    sleep 10
done
# Wait 60 min + 2 min buffer

# Phase 4: 10-second delays (fast phase)
for epic in 001-080; do
    launch_phase4_epic_$epic.sh &
    sleep 10
done
# Wait 60 min + 2 min buffer

# Phase 5: 30-second delays (slow phase)
for epic in 001-080; do
    launch_phase5_epic_$epic.sh &
    sleep 30
done
# Wait 180 min + 5 min buffer

# Phase 5.V: 30-second delays (slow phase)
for epic in 001-080; do
    launch_phase5v_epic_$epic.sh &
    sleep 30
done
# Wait 120 min + 5 min buffer

# Phase 6: 10-second delays (fast phase)
for epic in 001-080; do
    launch_phase6_epic_$epic.sh &
    sleep 10
done
# Wait 60 min + 2 min buffer
```

---

## Total Timeline & Budget

### Timeline
- **Catch-up** (Phase 0-1): 4.3 hours
- **All-in-one** (Phase 2-6): 9 hours
- **Total**: 13.3 hours (~1 work day)

### Budget
- **Catch-up**: 520 bobcoins
- **All-in-one**: 5,760 bobcoins
- **Total**: 6,280 bobcoins (98% of 6,400 budget)
- **Safety margin**: 120 bobcoins (2%)

### VM Capacity
- **Peak concurrent**: ~50 agents (Phase 2-4, 6)
- **VM capacity**: 50-60 agents (n2-standard-8)
- **Utilization**: 83-100% (optimal)

---

## Master Launch Script

```bash
#!/bin/bash
# launch_80_epic_all_in_one.sh
# Catch up EPIC-016-080, then run Phase 2-6 for all 80 epics

set -e

echo "=== 80-Epic All-in-One Execution ==="
echo "Catch-up: 65 epics (016-080, Phase 0-1)"
echo "All-in-one: 80 epics (001-080, Phase 2-6)"
echo "Estimated Time: 13.3 hours"
echo "Estimated Bobcoins: 6,280"
echo ""

# ========================================
# STEP 1: CATCH UP PHASE 0 (65 epics)
# ========================================
echo "[$(date)] Starting Phase 0 catch-up (EPIC-016-080)..."
./launch_phase0_catch_up.sh
echo "[$(date)] Phase 0 launched. Waiting 2 hours..."
sleep 7200  # 2 hours

# Verify Phase 0
count=$(ls docs/brain/EPIC-*/00-hotspots.md 2>/dev/null | wc -l)
echo "Phase 0 files: $count (expected 80)"
if [ "$count" -ne 80 ]; then
    echo "ERROR: Phase 0 incomplete. Expected 80, got $count"
    exit 1
fi

# ========================================
# STEP 2: CATCH UP PHASE 1 (65 epics)
# ========================================
echo "[$(date)] Starting Phase 1 catch-up (EPIC-016-080)..."
./launch_phase1_catch_up.sh
echo "[$(date)] Phase 1 launched. Waiting 2 hours..."
sleep 7200  # 2 hours

# Verify Phase 1
count=$(ls docs/brain/EPIC-*/00-scope.md 2>/dev/null | wc -l)
echo "Phase 1 files: $count (expected 80)"
if [ "$count" -ne 80 ]; then
    echo "ERROR: Phase 1 incomplete. Expected 80, got $count"
    exit 1
fi

# ========================================
# SYNC POINT: All 80 epics at Phase 1
# ========================================
echo ""
echo "[$(date)] ✅ SYNC POINT: All 80 epics at Phase 1 complete"
echo ""
sleep 600  # 10 min buffer

# Extract bobcoin usage
echo "[$(date)] Extracting Phase 0-1 bobcoin usage..."
grep -E 'Cost:.*Balance:|Cost: [0-9]' logs/phase*/EPIC-*.log | tee phase01_bobcoins.txt

# ========================================
# STEP 3: PHASE 2 (80 epics)
# ========================================
echo "[$(date)] Starting Phase 2 (all 80 epics)..."
./launch_phase2_all.sh
echo "[$(date)] Phase 2 launched. Waiting 90 minutes..."
sleep 5400  # 90 min

# Verify Phase 2
count=$(ls docs/brain/EPIC-*/02-architecture-plan.md 2>/dev/null | wc -l)
echo "Phase 2 files: $count (expected 80)"
sleep 600  # 10 min buffer

# ========================================
# STEP 4: PHASE 3 (80 epics)
# ========================================
echo "[$(date)] Starting Phase 3..."
./launch_phase3_all.sh
sleep 3600  # 60 min
count=$(ls docs/brain/EPIC-*/03-audit-report.md 2>/dev/null | wc -l)
echo "Phase 3 files: $count (expected 80)"
sleep 120  # 2 min buffer

# ========================================
# STEP 5: PHASE 4 (80 epics)
# ========================================
echo "[$(date)] Starting Phase 4..."
./launch_phase4_all.sh
sleep 3600  # 60 min
count=$(ls docs/brain/EPIC-*/04-tickets.md 2>/dev/null | wc -l)
echo "Phase 4 files: $count (expected 80)"
sleep 120  # 2 min buffer

# ========================================
# STEP 6: PHASE 5 (80 epics)
# ========================================
echo "[$(date)] Starting Phase 5..."
./launch_phase5_all.sh
sleep 10800  # 180 min (3 hours)
count=$(ls docs/brain/EPIC-*/ticket-1-completion.md 2>/dev/null | wc -l)
echo "Phase 5 files: $count (expected 80)"
sleep 300  # 5 min buffer

# ========================================
# STEP 7: PHASE 5.V (80 epics)
# ========================================
echo "[$(date)] Starting Phase 5.V..."
./launch_phase5v_all.sh
sleep 7200  # 120 min (2 hours)
count=$(ls docs/brain/EPIC-*/ticket-1-verification.md 2>/dev/null | wc -l)
echo "Phase 5.V files: $count (expected 80)"
sleep 300  # 5 min buffer

# ========================================
# STEP 8: PHASE 6 (80 epics)
# ========================================
echo "[$(date)] Starting Phase 6..."
./launch_phase6_all.sh
sleep 3600  # 60 min
count=$(ls docs/brain/EPIC-*/05-completion-report.md 2>/dev/null | wc -l)
echo "Phase 6 files: $count (expected 80)"
sleep 120  # 2 min buffer

# ========================================
# COMPLETION
# ========================================
echo ""
echo "=========================================="
echo "=== 80-Epic Execution Complete ==="
echo "=========================================="
echo "[$(date)] All phases finished!"
echo ""
echo "Final Status:"
echo "- Hotspots: $(ls docs/brain/EPIC-*/00-hotspots.md 2>/dev/null | wc -l)/80"
echo "- Scopes: $(ls docs/brain/EPIC-*/00-scope.md 2>/dev/null | wc -l)/80"
echo "- Architecture: $(ls docs/brain/EPIC-*/02-architecture-plan.md 2>/dev/null | wc -l)/80"
echo "- Audits: $(ls docs/brain/EPIC-*/03-audit-report.md 2>/dev/null | wc -l)/80"
echo "- Tickets: $(ls docs/brain/EPIC-*/04-tickets.md 2>/dev/null | wc -l)/80"
echo "- Completions: $(ls docs/brain/EPIC-*/ticket-1-completion.md 2>/dev/null | wc -l)/80"
echo "- Verifications: $(ls docs/brain/EPIC-*/ticket-1-verification.md 2>/dev/null | wc -l)/80"
echo "- Final Reports: $(ls docs/brain/EPIC-*/05-completion-report.md 2>/dev/null | wc -l)/80"
echo ""
echo "Next Steps:"
echo "1. Extract bobcoin usage from all phases"
echo "2. Verify all 80 epics complete"
echo "3. Sync files to local machine"
echo "4. Run build validation"
echo "5. Create completion report"
```

---

## Success Criteria

### Per Phase
- ✅ All 80 epics complete
- ✅ All files verified on disk
- ✅ Bobcoin usage within budget
- ✅ No P0 errors in logs
- ✅ VM load <2.5 throughout

### End of Wave
- ✅ All 80 epics fully complete (Phase 0-6)
- ✅ All 180 methods reduced to CYC ≤8
- ✅ Build passes
- ✅ Tests pass (if applicable)
- ✅ Jane Street compliance achieved

---

## Next Steps

### Immediate (Now)
1. Generate Phase 0-1 scripts for EPIC-016-080 (65 epics)
2. Upload scripts to VM
3. Launch catch-up execution (4.3 hours)

### Today (After Catch-Up)
1. Verify all 80 epics at Phase 1 complete
2. Generate Phase 2-6 scripts for all 80 epics
3. Launch all-in-one execution (9 hours)

### Tomorrow
1. Verify all 80 epics complete
2. Extract bobcoin usage
3. Sync files to local
4. Run build validation
5. Create completion report

---

## Key Decisions

✅ **Strategy**: Catch-up then all-in-one (optimal)
✅ **Scope**: All 80 epics from EPIC_ROADMAP_FINAL_V1.md
✅ **Budget**: 6,280 bobcoins (98% of 6,400 limit)
✅ **Timeline**: 13.3 hours (~1 work day)
✅ **Workflow**: Jane Street compliant (verified)
✅ **VM**: n2-standard-8 (sufficient capacity)

Ready to generate catch-up scripts for EPIC-016-080 and launch.