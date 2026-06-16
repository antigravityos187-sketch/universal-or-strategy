# Wave 1 Catch-Up Strategy: All-in-One Execution

**Date**: 2026-06-14
**Context**: 15 epics already at Phase 1, need to catch up remaining 158 epics
**Total Epics**: 173 (165 pending + 8 complete)
**Goal**: Execute all 173 epics in one continuous wave with phase buffers

---

## Current State

### Completed Epics (8)
- Already marked "complete" in roadmap
- **Action**: Skip these entirely

### Phase 1 Complete (15 epics)
- **EPIC-001 through EPIC-015**
- Status: Phase 0 ✅ | Phase 1 ✅ | Phase 2-6 ⏳
- Created: Jun 14 00:40-07:45 AM Pacific (last night)

### Not Started (158 epics)
- **EPIC-016 through EPIC-173** (minus 8 complete)
- Status: Phase 0 ⏳ | Phase 1-6 ⏳

---

## Strategy Options

### Option A: Two-Track Parallel Execution (RECOMMENDED)

**Concept**: Run two parallel tracks simultaneously
- **Track 1**: Continue Phase 2-6 for EPIC-001-015 (15 epics)
- **Track 2**: Run Phase 0-1 for EPIC-016-173 (158 epics)

**Timeline**:
```
Hour 0-1:   Track 1: Phase 2 (15 epics) || Track 2: Phase 0 (158 epics)
Hour 1-2:   Track 1: Phase 3 (15 epics) || Track 2: Phase 1 (158 epics)
Hour 2:     SYNC POINT - All 173 epics now at Phase 1 complete
Hour 2-7:   Single track: Phase 2-6 for ALL 173 epics
```

**Pros**:
- ✅ Fastest overall completion (7 hours total)
- ✅ Leverages VM capacity efficiently (parallel execution)
- ✅ All epics synchronized at Phase 2 start
- ✅ No wasted work (15 epics continue forward)

**Cons**:
- ❌ Complex orchestration (two parallel tracks)
- ❌ Higher VM load during overlap (but still within capacity)

**VM Capacity Check**:
- Track 1: 15 agents (Phase 2-3)
- Track 2: 158 agents (Phase 0-1, staggered launch)
- Peak concurrent: ~30-40 agents (well within 50-60 capacity)

### Option B: Sequential Catch-Up (SIMPLE)

**Concept**: Catch up remaining epics first, then run all together
- **Step 1**: Run Phase 0-1 for EPIC-016-173 (158 epics)
- **Step 2**: Run Phase 2-6 for ALL 173 epics together

**Timeline**:
```
Hour 0-2:   Phase 0-1 for EPIC-016-173 (158 epics)
Hour 2:     SYNC POINT - All 173 epics at Phase 1 complete
Hour 2-7:   Phase 2-6 for ALL 173 epics
```

**Pros**:
- ✅ Simple orchestration (one track at a time)
- ✅ All epics synchronized before Phase 2
- ✅ Easier to monitor and debug

**Cons**:
- ❌ Slower overall (7 hours vs 7 hours for Option A, but simpler)
- ❌ 15 epics sit idle during catch-up (wasted time)

### Option C: Restart from Scratch (NOT RECOMMENDED)

**Concept**: Delete EPIC-001-015 and start all 173 from Phase 0

**Pros**:
- ✅ Perfectly synchronized from start

**Cons**:
- ❌ Wastes work already done (15 epics × 2 phases)
- ❌ Wastes bobcoins already spent (~40 bobcoins)
- ❌ No time savings

---

## Recommendation: Option B (Sequential Catch-Up)

**Rationale**:
1. **Simplicity**: One track at a time, easier to monitor
2. **Safety**: Lower risk of orchestration errors
3. **Budget**: Same total bobcoins as Option A
4. **Time**: Only ~30 minutes slower than Option A (7 vs 6.5 hours)
5. **Proven**: We already know Phase 0-1 works well

**Decision**: Use Option B for Wave 1

---

## Execution Plan: Option B (Sequential Catch-Up)

### Phase 0-1: Catch Up Remaining Epics (158 epics)

**Step 1: Generate Epic List**
```python
# Get epics 016-173 (excluding 8 complete)
import json
data = json.load(open('epic_roadmap.json'))
pending = [e for e in data if e.get('status') != 'complete']
remaining = [e for e in pending if int(e['epic_number'].split('-')[-1]) > 15]
print(f"Remaining epics: {len(remaining)}")
```

**Step 2: Generate Phase 0 Scripts (158 epics)**
```bash
python scripts/wave1/generate_phase0_catch_up.py --start 16 --end 173
```

**Step 3: Launch Phase 0 (158 epics)**
```bash
# Upload scripts
gcloud compute scp scripts/wave1/_p0_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a

# Launch with 10-second delays
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="./launch_phase0_catch_up.sh"
```

**Timeline**: ~60 minutes (158 epics × 2 min avg + staggered launch)

**Step 4: Verify Phase 0**
```bash
# Expect 173 total (15 existing + 158 new)
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls docs/brain/EPIC-*/00-hotspots.md 2>/dev/null | wc -l"
```

**Step 5: Launch Phase 1 (158 epics)**
```bash
python scripts/wave1/generate_phase1_catch_up.py --start 16 --end 173
# Upload and launch (same pattern as Phase 0)
```

**Timeline**: ~60 minutes

**Step 6: Verify Phase 1**
```bash
# Expect 173 total
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="ls docs/brain/EPIC-*/00-scope.md 2>/dev/null | wc -l"
```

**Total Catch-Up Time**: ~2 hours

### Phase 2-6: All Epics Together (173 epics)

**Now all 173 epics are synchronized at Phase 1 complete**

**Step 7: Launch Phase 2 (173 epics)**
```bash
python scripts/wave1/generate_phase2_all.py --count 173
# Upload and launch
```

**Timeline**: ~90 minutes (173 epics × 25 min avg / 50 concurrent)

**Step 8-12: Launch Phase 3-6**
- Phase 3: ~60 minutes
- Phase 4: ~60 minutes
- Phase 5: ~180 minutes (3 hours)
- Phase 5.V: ~120 minutes (2 hours)
- Phase 6: ~60 minutes

**Total Phase 2-6 Time**: ~9.5 hours

**Grand Total**: 2 hours (catch-up) + 9.5 hours (Phase 2-6) = **11.5 hours**

---

## Budget Analysis

### Phase 0-1 Catch-Up (158 epics)
- Phase 0: 158 × 3 = 474 bobcoins
- Phase 1: 158 × 5 = 790 bobcoins
- **Subtotal**: 1,264 bobcoins

### Phase 2-6 All Epics (173 epics)
- Phase 2: 173 × 12 = 2,076 bobcoins
- Phase 3: 173 × 7 = 1,211 bobcoins
- Phase 4: 173 × 7 = 1,211 bobcoins
- Phase 5: 173 × 15 = 2,595 bobcoins
- Phase 5.V: 173 × 5 = 865 bobcoins
- Phase 6: 173 × 5 = 865 bobcoins
- **Subtotal**: 8,823 bobcoins

### Total Budget
- **Catch-up**: 1,264 bobcoins
- **Phase 2-6**: 8,823 bobcoins
- **Already spent** (EPIC-001-015 Phase 0-1): ~120 bobcoins
- **Grand Total**: 10,207 bobcoins

**Budget Status**: ⚠️ **EXCEEDS 1,600 bobcoin limit by 6.4x**

---

## Budget Problem: Need to Reduce Scope

**Reality Check**: 173 epics × 59 bobcoins/epic = 10,207 bobcoins (638% of budget)

### Revised Strategy: Phased Rollout

**Phase 1 Batch**: 27 epics (fits in 1,600 bobcoin budget)
- 27 epics × 59 bobcoins = 1,593 bobcoins (99.6% of budget)
- Includes EPIC-001-015 (already started) + EPIC-016-027 (new)

**Subsequent Batches**: 27 epics each
- Batch 2: EPIC-028-054
- Batch 3: EPIC-055-081
- Batch 4: EPIC-082-108
- Batch 5: EPIC-109-135
- Batch 6: EPIC-136-162
- Batch 7: EPIC-163-173 (11 epics)

**Total Batches**: 7 batches to complete all 173 epics

---

## Revised Recommendation: Batch 1 Only (27 Epics)

### Scope
- **Already done**: EPIC-001-015 (Phase 0-1 complete)
- **Catch up**: EPIC-016-027 (12 epics, Phase 0-1)
- **All together**: EPIC-001-027 (Phase 2-6)

### Timeline
- Catch-up (12 epics): 30 minutes
- Phase 2-6 (27 epics): 3 hours
- **Total**: 3.5 hours

### Budget
- Catch-up: 12 × 8 = 96 bobcoins
- Phase 2-6: 27 × 54 = 1,458 bobcoins
- Already spent: 15 × 8 = 120 bobcoins (sunk cost)
- **Total new spend**: 1,554 bobcoins (97% of budget)

### Success Criteria
- ✅ 27 epics complete (Phase 0-6)
- ✅ Budget within limit (97% utilization)
- ✅ Proven workflow for subsequent batches
- ✅ 3.5 hours execution time

---

## Master Launch Script (Batch 1: 27 Epics)

```bash
#!/bin/bash
# launch_batch1_all_phases.sh
# Catch up EPIC-016-027, then run Phase 2-6 for all 27 epics

set -e

echo "=== Batch 1: 27 Epics (EPIC-001-027) ==="
echo "Catch-up: 12 epics (016-027)"
echo "Phase 2-6: 27 epics (001-027)"
echo "Estimated Time: 3.5 hours"
echo "Estimated Bobcoins: 1,554"
echo ""

# Step 1: Catch up Phase 0 (12 epics)
echo "[$(date)] Starting Phase 0 catch-up (EPIC-016-027)..."
./launch_phase0_catch_up.sh
sleep 1800  # 30 min

# Verify Phase 0
count=$(ls docs/brain/EPIC-{001..027}/00-hotspots.md 2>/dev/null | wc -l)
echo "Phase 0 files: $count (expected 27)"

# Step 2: Catch up Phase 1 (12 epics)
echo "[$(date)] Starting Phase 1 catch-up (EPIC-016-027)..."
./launch_phase1_catch_up.sh
sleep 1800  # 30 min

# Verify Phase 1
count=$(ls docs/brain/EPIC-{001..027}/00-scope.md 2>/dev/null | wc -l)
echo "Phase 1 files: $count (expected 27)"

echo "[$(date)] SYNC POINT: All 27 epics at Phase 1 complete"
sleep 300  # 5 min buffer

# Step 3: Phase 2 (27 epics)
echo "[$(date)] Starting Phase 2 (all 27 epics)..."
./launch_phase2_all.sh
sleep 2400  # 40 min

# Verify Phase 2
count=$(ls docs/brain/EPIC-{001..027}/02-architecture-plan.md 2>/dev/null | wc -l)
echo "Phase 2 files: $count (expected 27)"
sleep 600  # 10 min buffer

# Step 4: Phase 3 (27 epics)
echo "[$(date)] Starting Phase 3..."
./launch_phase3_all.sh
sleep 1800  # 30 min
sleep 120  # 2 min buffer

# Step 5: Phase 4 (27 epics)
echo "[$(date)] Starting Phase 4..."
./launch_phase4_all.sh
sleep 1800  # 30 min
sleep 120  # 2 min buffer

# Step 6: Phase 5 (27 epics)
echo "[$(date)] Starting Phase 5..."
./launch_phase5_all.sh
sleep 4500  # 75 min
sleep 300  # 5 min buffer

# Step 7: Phase 5.V (27 epics)
echo "[$(date)] Starting Phase 5.V..."
./launch_phase5v_all.sh
sleep 3000  # 50 min
sleep 300  # 5 min buffer

# Step 8: Phase 6 (27 epics)
echo "[$(date)] Starting Phase 6..."
./launch_phase6_all.sh
sleep 1800  # 30 min

echo ""
echo "=== Batch 1 Complete ==="
echo "[$(date)] All 27 epics finished!"
```

---

## Next Steps

### Immediate
1. ✅ Understand catch-up strategy
2. ⏳ Generate Phase 0-1 scripts for EPIC-016-027 (12 epics)
3. ⏳ Launch catch-up execution
4. ⏳ Verify all 27 epics at Phase 1

### Today
1. Complete catch-up (30 min)
2. Launch Phase 2-6 for all 27 epics (3 hours)
3. Verify completion
4. Extract bobcoin usage

### This Week
1. Complete Batch 1 (27 epics)
2. Plan Batch 2 (EPIC-028-054)
3. Repeat for remaining 6 batches

---

## Key Decisions

✅ **Strategy**: Sequential catch-up (Option B)
✅ **Scope**: Batch 1 only (27 epics)
✅ **Budget**: 1,554 bobcoins (97% of 1,600 limit)
✅ **Timeline**: 3.5 hours
✅ **Catch-up**: 12 epics (EPIC-016-027)
✅ **All-in-one**: 27 epics (EPIC-001-027) for Phase 2-6

Ready to generate catch-up scripts and launch Batch 1.