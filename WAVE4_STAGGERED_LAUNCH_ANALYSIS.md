# Wave 4 Staggered Launch Analysis

**Date**: 2026-06-14T23:01:00Z
**Analysis**: Existing launch pattern vs Wave 4 requirements

---

## What I Found

### Existing Pattern (Wave 3 - 9 Epics)

**Individual Epic Script** (`_p0_107.sh`):
```bash
#!/bin/bash
set -e
cd /home/malhitticrypto/universal-or-strategy
export BOBSHELL_API_KEY='...'
mkdir -p docs/brain/EPIC-CCN-107
mkdir -p logs/phase0

# Create message file with instructions
cat > /tmp/phase0_msg_107.txt << 'EOFMSG'
[Phase-specific instructions]
EOFMSG

# Execute Bob CLI in specific mode
bob --yolo --chat-mode v12-phase0-hotspot "$(cat /tmp/phase0_msg_107.txt)" 2>&1 | tee logs/phase0/EPIC-CCN-107.log
echo "DONE_EXIT=$?"
```

**Master Launch Script** (`launch_phase0_all_screen.sh`):
```bash
#!/bin/bash
EPICS=(107 108 109 110 111 112 113 114 115)  # 9 epics

for EPIC in "${EPICS[@]}"; do
    SESSION_NAME="epic-ccn-${EPIC}"
    SCRIPT_PATH="${REPO_DIR}/_p0_${EPIC}.sh"
    
    # Launch in screen session (background)
    screen -dmS "${SESSION_NAME}" bash -l "${SCRIPT_PATH}"
    
    # 2-second delay
    sleep 2
done
```

---

## Wave 4 Requirements (80 Epics)

### Key Differences

| Aspect | Wave 3 (Current) | Wave 4 (Required) |
|--------|------------------|-------------------|
| **Epic Count** | 9 epics | 80 epics |
| **Delay Strategy** | Fixed 2 seconds | Staggered 12-54 seconds |
| **Execution Mode** | Screen sessions (background) | Foreground terminal (visible) |
| **Launch Time** | 9 × 2s = 18 seconds | 80 × 30s avg = 40 minutes |
| **Monitoring** | Screen -r (attach) | Direct terminal output |

### Staggered Delay Pattern (from Handoff)

```bash
BASE_DELAY=12
MAX_DELAY=54

for i in "${!EPICS[@]}"; do
    EPIC="${EPICS[$i]}"
    
    # Calculate staggered delay (12-54 seconds)
    DELAY=$((BASE_DELAY + (i % (MAX_DELAY - BASE_DELAY + 1))))
    
    echo "[$(date)] Launching EPIC-CCN-${EPIC} (delay: ${DELAY}s)"
    
    # Launch
    screen -dmS p${PHASE}-${EPIC} bash -l -c \
        "./_p${PHASE}_${EPIC}.sh 2>&1 | tee logs/phase${PHASE}/EPIC-CCN-${EPIC}.log"
    
    # Wait before next launch
    sleep ${DELAY}
done
```

---

## The Misunderstanding

**What I thought**: Execute epics sequentially in foreground (1 at a time, 80 × 100 min = 133 hours)

**What you meant**: Launch 80 agents in parallel using staggered delays, but execute in foreground terminal (not screen sessions) so you can watch the output

---

## Correct Approach for Wave 4

### Option 1: Parallel Launch with Foreground Monitoring (RECOMMENDED)

**Launch Pattern**:
```bash
#!/bin/bash
# Launch 80 epics with staggered delays (12-54 seconds)
# Each epic runs in background but logs to foreground-visible file

EPICS=($(seq -f "%03g" 1 80))
BASE_DELAY=12
MAX_DELAY=54

for i in "${!EPICS[@]}"; do
    EPIC="${EPICS[$i]}"
    DELAY=$((BASE_DELAY + (i % (MAX_DELAY - BASE_DELAY + 1))))
    
    echo "[$(date)] Launching EPIC-CCN-${EPIC} (delay: ${DELAY}s)"
    
    # Launch in background, log to file
    ./_p0_${EPIC}.sh > logs/phase0/EPIC-CCN-${EPIC}.log 2>&1 &
    
    # Store PID for monitoring
    echo $! > logs/phase0/EPIC-CCN-${EPIC}.pid
    
    sleep ${DELAY}
done

echo "All 80 epics launched. Monitor with:"
echo "  tail -f logs/phase0/EPIC-CCN-001.log"
echo "  watch 'ls -lh docs/brain/EPIC-CCN-*/00-hotspots.md | wc -l'"
```

**Monitoring**:
```bash
# Watch progress in real-time
tail -f logs/phase0/EPIC-CCN-001.log

# Check completion count
watch 'ls -lh docs/brain/EPIC-CCN-*/00-hotspots.md | wc -l'

# Check running processes
ps aux | grep bob | wc -l
```

### Option 2: Sequential Foreground Execution (SLOW)

**Launch Pattern**:
```bash
#!/bin/bash
# Execute epics one at a time in foreground

EPICS=($(seq -f "%03g" 1 80))

for EPIC in "${EPICS[@]}"; do
    echo "=== Starting EPIC-CCN-${EPIC} ==="
    
    # Execute in foreground (you see all output)
    ./_p0_${EPIC}.sh
    
    echo "=== Completed EPIC-CCN-${EPIC} ==="
done
```

**Duration**: 80 epics × 10 min = 800 minutes (13 hours) per phase

---

## Recommendation

**Use Option 1** (Parallel Launch with Foreground Monitoring):

**Why**:
- ✅ Achieves 80-epic parallelization (30 hours total vs 133 hours sequential)
- ✅ You can watch progress via `tail -f` on any epic's log
- ✅ Staggered delays prevent system overload
- ✅ Matches Wave 4 handoff requirements

**How to Watch**:
```bash
# Terminal 1: Launch all epics
./launch_phase0_all.sh

# Terminal 2: Monitor first epic
tail -f logs/phase0/EPIC-CCN-001.log

# Terminal 3: Monitor completion count
watch 'ls -lh docs/brain/EPIC-CCN-*/00-hotspots.md | wc -l'

# Terminal 4: Monitor system load
watch 'ps aux | grep bob | wc -l'
```

---

## Next Steps

1. **Generate 80 individual epic scripts** (`_p0_001.sh` through `_p0_080.sh`)
   - Use building-blocks method (copy from Wave 3)
   - Modify epic number, method name, file path, complexity

2. **Create master launch script** (`launch_phase0_all.sh`)
   - 80 epics with staggered delays (12-54 seconds)
   - Background execution with foreground-visible logs
   - PID tracking for monitoring

3. **Test with first 2 epics**
   - Validate script generation
   - Confirm parallel execution works
   - Verify log visibility

4. **Launch full wave**
   - All 80 epics
   - Monitor via tail -f
   - Track completion count

---

## Key Insight

**The "staggered method" means**:
- Launch all 80 agents in parallel (not sequential)
- Use staggered delays (12-54 seconds) to prevent system overload
- Each agent runs independently in background
- You monitor via log files (tail -f) in foreground terminal
- Total time: ~30 hours (parallel) vs 133 hours (sequential)

**NOT**:
- Execute epics one at a time in foreground (that's sequential, too slow)

---

**Document Version**: 1.0
**Last Updated**: 2026-06-14T23:01:00Z
**Status**: Analysis Complete - Ready for Script Generation