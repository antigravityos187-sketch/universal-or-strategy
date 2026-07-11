# Wave 1: All-in-One Wave Execution Plan

**Date**: 2026-06-14 10:19 AM PST
**Strategy**: Launch all 80+ epics through all phases in one continuous wave
**Key Insight**: Optimized delays allow safe parallel execution across all phases

## Execution Model

### NOT This (Phase-by-Phase for 15 Epics)
```
Phase 2 → Wait → Phase 3 → Wait → Phase 4 → Wait → Phase 5 → Wait → Phase 6
(15 epics only, 3.8 hours)
```

### YES This (All Phases for 80+ Epics in One Wave)
```
Launch Phase 2 (all 80 epics) → Buffer → Launch Phase 3 (all 80) → Buffer → ... → Phase 6
(80+ epics, ~4.5 hours total)
```

## Why This Works

### VM Capacity
- **Current**: n2-standard-8 (8 vCPU, 32 GB RAM)
- **Peak Load**: 15-20 concurrent agents (with optimized delays)
- **Capacity**: 50-60 agents (we're well under limit)
- **Bottleneck**: API I/O, not VM resources

### Optimized Delays Prevent Overload
| Phase | Execution Time | Delay | Peak Agents |
|-------|----------------|-------|-------------|
| Phase 2 | 25 min | 15s | ~10-15 |
| Phase 3 | 10 min | 10s | ~6-10 |
| Phase 4 | 10 min | 10s | ~6-10 |
| Phase 5 | 60 min | 30s | ~20-30 |
| Phase 5.V | 30 min | 30s | ~15-20 |
| Phase 6 | 10 min | 10s | ~6-10 |

**Key**: Agents complete faster than they launch, so queue never builds up

## Complete Wave Timeline (80 Epics)

### Phase 2: Architecture Planning
- **Launch**: 80 × 15s = 20 minutes
- **Execution**: 25 minutes (overlapping)
- **Total**: ~35 minutes
- **Buffer**: 10 minutes (verify completion)

### Phase 3: DNA & PR Audit
- **Launch**: 80 × 10s = 13 minutes
- **Execution**: 10 minutes (overlapping)
- **Total**: ~20 minutes
- **Buffer**: 5 minutes (verify completion)

### Phase 4: Ticket Generation
- **Launch**: 80 × 10s = 13 minutes
- **Execution**: 10 minutes (overlapping)
- **Total**: ~20 minutes
- **Buffer**: 10 minutes (verify completion)

### Phase 5: Ticket Execution (Slowest)
- **Launch**: 80 × 30s = 40 minutes
- **Execution**: 60 minutes (overlapping)
- **Total**: ~80 minutes
- **Buffer**: 10 minutes (verify completion)

### Phase 5.V: Ticket Verification
- **Launch**: 80 × 30s = 40 minutes
- **Execution**: 30 minutes (overlapping)
- **Total**: ~55 minutes
- **Buffer**: 5 minutes (verify completion)

### Phase 6: Final Review
- **Launch**: 80 × 10s = 13 minutes
- **Execution**: 10 minutes (overlapping)
- **Total**: ~20 minutes
- **Buffer**: 5 minutes (final verification)

## Total Timeline

| Phase | Time | Buffer | Total |
|-------|------|--------|-------|
| Phase 2 | 35 min | 10 min | 45 min |
| Phase 3 | 20 min | 5 min | 25 min |
| Phase 4 | 20 min | 10 min | 30 min |
| Phase 5 | 80 min | 10 min | 90 min |
| Phase 5.V | 55 min | 5 min | 60 min |
| Phase 6 | 20 min | 5 min | 25 min |
| **Total** | **230 min** | **45 min** | **275 min (4.6 hours)** |

## Budget Projection (80 Epics)

| Phase | Bobcoins/Epic | Total | % of 1,600 |
|-------|---------------|-------|------------|
| Phase 0 | 1.49 | 120 | 7.5% |
| Phase 1 | 1.17 | 95 | 6.0% |
| Phase 2 | 3.00 | 240 | 15.0% |
| Phase 3 | 7.50 | 600 | 37.5% |
| Phase 4 | 7.50 | 600 | 37.5% |
| Phase 5 | 10.00 | 800 | 50.0% |
| Phase 5.V | 5.00 | 400 | 25.0% |
| Phase 6 | 3.00 | 240 | 15.0% |
| **Total** | **38.66** | **3,095** | **193.4%** |

**PROBLEM**: Budget exceeded by 93.4% (need 1,495 more bobcoins)

## Adjusted Plan: 40 Epics (Within Budget)

| Phase | Bobcoins/Epic | Total | % of 1,600 |
|-------|---------------|-------|------------|
| Phase 0 | 1.49 | 60 | 3.8% |
| Phase 1 | 1.17 | 47 | 2.9% |
| Phase 2 | 3.00 | 120 | 7.5% |
| Phase 3 | 7.50 | 300 | 18.8% |
| Phase 4 | 7.50 | 300 | 18.8% |
| Phase 5 | 10.00 | 400 | 25.0% |
| Phase 5.V | 5.00 | 200 | 12.5% |
| Phase 6 | 3.00 | 120 | 7.5% |
| **Total** | **38.66** | **1,547** | **96.7%** |

**Timeline**: 275 min × (40/80) = **137 minutes (2.3 hours)**

## Execution Strategy

### Option A: 40 Epics (Recommended)
- **Rationale**: Stays within budget (96.7%)
- **Timeline**: 2.3 hours
- **Risk**: Low (3.3% margin)
- **Action**: Launch all 40 epics through all phases today

### Option B: 15 Epics (Conservative)
- **Rationale**: Already started, safe validation
- **Timeline**: 51 minutes (275 × 15/80)
- **Bobcoins**: 580 (36.3% of budget)
- **Action**: Complete 15 epics, assess, then scale

### Option C: Optimize and Scale to 64 Epics
- **Rationale**: Reduce bobcoin usage per epic
- **Target**: <25 bobcoins per epic (vs 38.66 current)
- **Action**: Optimize prompts, then launch 64 epics
- **Timeline**: 3.5 hours

## Recommendation: Option B → Option A

### Step 1: Complete 15 Epics (Today Morning)
- **Time**: 51 minutes
- **Bobcoins**: 580 (36.3%)
- **Goal**: Validate workflow, measure actual usage

### Step 2: Assess Actual Usage (Today Afternoon)
- **Check**: Did we use 38.66 bobcoins/epic or less?
- **If Less**: Can scale to more epics
- **If More**: Need to optimize or stop at 15

### Step 3: Scale to 40 Epics (Today Afternoon/Tomorrow)
- **Condition**: Actual usage <30 bobcoins/epic
- **Time**: 86 minutes (137 - 51)
- **Bobcoins**: 967 (60.4%)
- **Total**: 1,547 bobcoins (96.7% of budget)

## All-in-One Launch Commands

### Generate All Phase Scripts (Phases 2-6)
```bash
# Phase 2
python scripts/wave1/generate_phase2_all_epics.py

# Phase 3
python scripts/wave1/generate_phase3_all_epics.py

# Phase 4
python scripts/wave1/generate_phase4_all_epics.py

# Phase 5
python scripts/wave1/generate_phase5_all_epics.py

# Phase 5.V
python scripts/wave1/generate_phase5v_all_epics.py

# Phase 6
python scripts/wave1/generate_phase6_all_epics.py
```

### Upload All Scripts to VM
```bash
gcloud compute scp scripts/wave1/_p2_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p3_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p4_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p5_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p5v_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
gcloud compute scp scripts/wave1/_p6_*.sh v12-test-golden-v2:/home/malhitticrypto/universal-or-strategy/ --zone=us-central1-a
```

### Launch All Phases (One Command)
```bash
gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a --command="cd /home/malhitticrypto/universal-or-strategy && ./launch_all_phases.sh"
```

### Master Launch Script (launch_all_phases.sh)
```bash
#!/bin/bash
# Launch all phases with buffers

echo "Starting Wave 1 All-in-One Execution"
echo "Epics: 40 (or 15 for validation)"
echo "Estimated time: 2.3 hours (or 51 min for 15)"

# Phase 2
echo "[$(date)] Launching Phase 2..."
./launch_phase2_rolling.sh
sleep 600  # 10-minute buffer

# Phase 3
echo "[$(date)] Launching Phase 3..."
./launch_phase3_rolling.sh
sleep 300  # 5-minute buffer

# Phase 4
echo "[$(date)] Launching Phase 4..."
./launch_phase4_rolling.sh
sleep 600  # 10-minute buffer

# Phase 5
echo "[$(date)] Launching Phase 5..."
./launch_phase5_rolling.sh
sleep 600  # 10-minute buffer

# Phase 5.V
echo "[$(date)] Launching Phase 5.V..."
./launch_phase5v_rolling.sh
sleep 300  # 5-minute buffer

# Phase 6
echo "[$(date)] Launching Phase 6..."
./launch_phase6_rolling.sh
sleep 300  # 5-minute buffer

echo "[$(date)] Wave 1 Complete!"
```

## Success Criteria

### Per Phase
- ✅ All epics complete (15 or 40)
- ✅ All output files verified on disk
- ✅ Bobcoin usage within estimate (±20%)
- ✅ No P0 errors
- ✅ VM load <2.5 throughout

### End of Wave
- ✅ All epics fully complete (Phase 0 through Phase 6)
- ✅ Bobcoins <1,600 (100% of budget)
- ✅ Build passes
- ✅ Tests pass (if applicable)
- ✅ Ready for PR submission

## Next Step

**START**: Generate Phase 2-6 scripts for 15 epics (validation run)
**THEN**: Launch all phases with master script
**ASSESS**: Actual bobcoin usage after 15 epics
**SCALE**: Launch remaining 25 epics if budget allows

---

**Ready to launch all-in-one wave?** 🚀