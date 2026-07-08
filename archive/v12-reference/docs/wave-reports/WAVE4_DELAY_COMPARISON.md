# Wave 4 Delay Strategy Comparison

**Date**: 2026-06-14
**Purpose**: Compare Wave 1 buffer strategy with Wave 4 delay recommendations

---

## Key Differences

### Wave 1 (WAVE1_OPTIMIZED_BUFFER_STRATEGY.md)
- **Focus**: Buffer time AFTER phase completion
- **Purpose**: Verification + VM stabilization between phases
- **Timing**: Wait for ALL epics to complete, then buffer before next phase

### Wave 4 (WAVE4_PHASE_DELAYS_SUMMARY.md)
- **Focus**: Delay time BETWEEN epic launches
- **Purpose**: Stagger launches to prevent API/VM overload
- **Timing**: Delay between launching each individual epic

**These are COMPLEMENTARY strategies, not competing ones!**

---

## Wave 1 Buffer Strategy (Post-Phase)

**After phase completes, wait before launching next phase:**

| Phase | Execution | Buffer | Purpose |
|-------|-----------|--------|---------|
| Phase 2 | 35 min | 3 min | Verify files + VM stabilization |
| Phase 3 | 20 min | 2 min | Quick verification |
| Phase 4 | 20 min | 2 min | Quick verification |
| Phase 5 | 80 min | 5 min | Build check + metrics |
| Phase 5.V | 55 min | 5 min | Build check + metrics |
| Phase 6 | 20 min | 2 min | Quick verification |

**Total Buffer Time**: 19 minutes (between phases)

---

## Wave 4 Delay Strategy (During Launch)

**Between launching each epic within a phase:**

| Phase | Delay | Purpose |
|-------|-------|---------|
| Phase -1 | 2s | Minimal pre-flight |
| Phase 0 | 12s | jCodemunch API spacing |
| Phase 1 | 12s | Standard spacing |
| Phase 2 | 15s | Jane Street KB + jCodemunch |
| Phase 3 | 12s | Standard spacing |
| Phase 4 | 10s | Low-load spacing |
| Phase 4.5 | 12s | Jane Street KB spacing |
| Phase 5 | **25s** | Bob CLI + highest load |
| Phase 5.V | 15s | Build + test spacing |
| Phase 6 | 10s | Low-load spacing |

**Total Launch Time**: 166 minutes (to launch all 80 epics across all phases)

---

## Combined Strategy for Wave 4

**Complete workflow for one phase:**

1. **Launch Phase** (with delays between epics)
   - Example Phase 0: 80 epics × 12s = 16 minutes to launch all
   
2. **Wait for Completion** (parallel execution)
   - All 80 epics run in parallel
   - Example Phase 0: ~10 minutes execution time
   
3. **Buffer Period** (Wave 1 strategy)
   - Fast phases (0, 1, 3, 4, 6): 2 minutes
   - Medium phase (2): 3 minutes
   - Slow phases (5, 5.V): 5 minutes
   
4. **Verify Completion**
   - Check file count: `ls docs/brain/EPIC-*/XX-output.md | wc -l`
   - Extract metrics: `grep "Cost:" logs/phaseX/*.log`
   - Check VM health: `uptime && free -h`

5. **Launch Next Phase**
   - Repeat steps 1-4

---

## Example Timeline: Phase 0

**Step 1: Launch (16 minutes)**
```
00:00 - Launch EPIC-CCN-001 (delay 12s)
00:12 - Launch EPIC-CCN-002 (delay 12s)
00:24 - Launch EPIC-CCN-003 (delay 12s)
...
15:48 - Launch EPIC-CCN-080 (last epic)
16:00 - All launches complete
```

**Step 2: Execution (10 minutes, parallel)**
```
16:00 - All 80 epics running in parallel
26:00 - All epics complete
```

**Step 3: Buffer (2 minutes)**
```
26:00 - Start verification
26:30 - Check file count (expect 80)
27:00 - Extract bobcoin usage
27:30 - Check VM health
28:00 - Buffer complete
```

**Step 4: Launch Phase 1**
```
28:00 - Start launching Phase 1 epics
```

**Total Phase 0 Time**: 28 minutes (16 launch + 10 execution + 2 buffer)

---

## Comparison: Old vs New Numbers

### Wave 1 Optimized Buffers
- **Philosophy**: Match buffer to phase complexity
- **Fast phases**: 2 minutes
- **Medium phase**: 3 minutes
- **Slow phases**: 5 minutes
- **Total**: 19 minutes across all phases

### Wave 4 Launch Delays
- **Philosophy**: Match delay to API/VM load
- **Low load**: 10s (phases 4, 6)
- **Standard load**: 12s (phases 0, 1, 3, 4.5)
- **Medium load**: 15s (phases 2, 5.V)
- **High load**: 25s (phase 5)
- **Total**: 166 minutes to launch all 80 epics

---

## Why Wave 4 Numbers Are Better

### 1. More Granular Load Management
- Wave 1: Only considered post-phase buffers
- Wave 4: Also considers launch delays to prevent API overload

### 2. Scales to 80 Epics
- Wave 1: Designed for 9 epics (small wave)
- Wave 4: Designed for 80 epics (9x larger)

### 3. API-Aware Delays
- Wave 1: No consideration of API rate limits
- Wave 4: Delays account for jCodemunch, Bob Shell, Firebase KB

### 4. Phase-Specific Tuning
- Wave 1: Generic buffers (2-5 min)
- Wave 4: Specific delays per phase load (10-25s)

### 5. Prevents Thundering Herd
- Wave 1: All epics launch simultaneously
- Wave 4: Staggered launches prevent API/VM overload

---

## Recommendation

**USE BOTH STRATEGIES**:

1. **During Launch**: Use Wave 4 delays (10-25s between epics)
2. **After Completion**: Use Wave 1 buffers (2-5 min between phases)

**Benefits**:
- Smooth API load distribution (Wave 4 delays)
- Proper verification time (Wave 1 buffers)
- Optimal resource utilization
- Prevents both API overload and premature phase transitions

---

## Updated Master Launch Script Pattern

```bash
#!/bin/bash
set -e

REPO_DIR="/home/malhitticrypto/universal-or-strategy"
cd "$REPO_DIR"

PHASE="{PHASE}"
DELAY={DELAY}  # Wave 4: 10-25s based on phase
BUFFER={BUFFER}  # Wave 1: 2-5 min based on phase

EPICS=($(seq -f "%03g" 1 80))

# Step 1: Launch with delays (Wave 4)
echo "[$(date)] Launching Phase ${PHASE} with ${DELAY}s delays..."
for i in "${!EPICS[@]}"; do
    EPIC="${EPICS[$i]}"
    bash "_p${PHASE}_${EPIC}.sh" > "logs/phase${PHASE}/EPIC-CCN-${EPIC}.log" 2>&1 &
    sleep ${DELAY}  # Wave 4 delay
done

# Step 2: Wait for completion (parallel execution)
echo "[$(date)] All epics launched. Waiting for completion..."
wait

# Step 3: Buffer period (Wave 1)
echo "[$(date)] Phase ${PHASE} complete. Starting ${BUFFER}-minute buffer..."
sleep $((BUFFER * 60))

# Step 4: Verify
echo "[$(date)] Verifying completion..."
completed=$(ls docs/brain/EPIC-*/XX-output.md 2>/dev/null | wc -l)
echo "Completed: $completed/80"
uptime
free -h

echo "[$(date)] Ready for next phase"
```

---

## Summary

**Wave 1 Strategy**: ✅ Keep (post-phase buffers)
**Wave 4 Strategy**: ✅ Keep (launch delays)
**Combined**: ✅ **BEST** (use both)

**Phase 5 Delay**: Updated to **25s** per user request (highest load)

---

**Status**: Comparison complete, strategies are complementary