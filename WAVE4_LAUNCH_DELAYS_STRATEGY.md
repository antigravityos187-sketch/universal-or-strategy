# Wave 4 Launch Delays Strategy

**Date**: 2026-06-14
**Focus**: Staggered epic launches to prevent API/VM overload
**Complementary To**: Wave 1 post-phase buffer strategy

---

## Purpose

**Launch delays** are the time intervals between starting each individual epic within a phase. They prevent:
- API rate limit exhaustion (jCodemunch, Bob Shell, Firebase KB)
- VM resource contention (CPU, memory, disk I/O)
- Network bandwidth saturation
- "Thundering herd" problems

---

## Phase-Specific Delays

| Phase | Delay | Load Type | APIs Used |
|-------|-------|-----------|-----------|
| **-1** | 2s | Minimal | None (local validation) |
| **0** | 12s | Moderate | jCodemunch (hotspots, blast radius, call hierarchy) |
| **1** | 12s | Low | Minimal (planning only) |
| **2** | 15s | High | jCodemunch + Firebase KB (Jane Street rules) |
| **3** | 12s | Moderate | jCodemunch (DNA checks, PR hygiene) |
| **4** | 10s | Low | Minimal (ticket generation) |
| **4.5** | 12s | Moderate | Firebase KB (Jane Street ticket validation) |
| **5** | **25s** | **HIGHEST** | Bob Shell (code surgery) + jCodemunch |
| **5.V** | 15s | High | Build + test execution |
| **6** | 10s | Low | Minimal (report generation) |

---

## Delay Selection Rationale

### 2 seconds (Phase -1)
- **Purpose**: Pre-flight validation checks
- **Load**: Minimal (local file checks, git status)
- **APIs**: None
- **Justification**: No external dependencies, can launch rapidly

### 10 seconds (Phases 4, 6)
- **Purpose**: Low-load operations
- **Load**: Ticket generation, report writing
- **APIs**: Minimal Bob Shell usage
- **Justification**: Simple operations, low resource usage

### 12 seconds (Phases 0, 1, 3, 4.5)
- **Purpose**: Standard operations
- **Load**: Moderate jCodemunch or Firebase KB usage
- **APIs**: 1-2 API calls per epic
- **Justification**: Baseline spacing for most phases

### 15 seconds (Phases 2, 5.V)
- **Purpose**: Medium-load operations
- **Load**: Multiple API calls + computation
- **APIs**: jCodemunch + Firebase KB (Phase 2), Build + Test (Phase 5.V)
- **Justification**: Higher resource usage, needs more spacing

### 25 seconds (Phase 5)
- **Purpose**: Highest-load operations
- **Load**: Code surgery with Bob CLI
- **APIs**: Bob Shell + jCodemunch + potential compilation
- **Justification**: Most resource-intensive phase, needs maximum spacing
- **User Request**: Increased from 20s to 25s for extra safety margin

---

## Launch Timeline Calculations

### Per-Phase Launch Times (80 Epics)

| Phase | Delay | Total Launch Time | Formula |
|-------|-------|-------------------|---------|
| Phase -1 | 2s | 2.7 min | 80 × 2s = 160s |
| Phase 0 | 12s | 16 min | 80 × 12s = 960s |
| Phase 1 | 12s | 16 min | 80 × 12s = 960s |
| Phase 2 | 15s | 20 min | 80 × 15s = 1200s |
| Phase 3 | 12s | 16 min | 80 × 12s = 960s |
| Phase 4 | 10s | 13.3 min | 80 × 10s = 800s |
| Phase 4.5 | 12s | 16 min | 80 × 12s = 960s |
| Phase 5 | 25s | 33.3 min | 80 × 25s = 2000s |
| Phase 5.V | 15s | 20 min | 80 × 15s = 1200s |
| Phase 6 | 10s | 13.3 min | 80 × 10s = 800s |

**Total Launch Time Across All Phases**: 166 minutes (~2.8 hours)

---

## Implementation Pattern

### Master Launch Script Template

```bash
#!/bin/bash
set -e

REPO_DIR="/home/malhitticrypto/universal-or-strategy"
cd "$REPO_DIR"

PHASE="{PHASE}"  # Replace: 0, 1, 2, 3, 4, 4.5, 5, 5.V, 6

# CONSTANT delay (DO NOT INCREMENT)
DELAY={DELAY}  # Replace with phase-specific delay: 10, 12, 15, or 25

EPICS=($(seq -f "%03g" 1 80))

echo "[$(date)] Starting Phase ${PHASE} launch with ${DELAY}s delays"
echo "Total epics: ${#EPICS[@]}"
echo "Estimated launch time: $((${#EPICS[@]} * DELAY / 60)) minutes"

for i in "${!EPICS[@]}"; do
    EPIC="${EPICS[$i]}"
    SCRIPT_PATH="${REPO_DIR}/_p${PHASE}_${EPIC}.sh"
    LOG_PATH="${REPO_DIR}/logs/phase${PHASE}/EPIC-CCN-${EPIC}.log"
    PID_PATH="${REPO_DIR}/logs/phase${PHASE}/EPIC-CCN-${EPIC}.pid"
    
    echo "[$(date '+%H:%M:%S')] Launching EPIC-CCN-${EPIC} (${i}/${#EPICS[@]})"
    
    # Launch in background
    bash "${SCRIPT_PATH}" > "${LOG_PATH}" 2>&1 &
    echo $! > "${PID_PATH}"
    
    # CONSTANT delay - same for ALL epics
    sleep ${DELAY}
done

echo "[$(date)] All ${#EPICS[@]} epics launched for Phase ${PHASE}"
echo "Monitor with: tail -f logs/phase${PHASE}/*.log"
```

---

## Critical Rules

### ✅ DO
1. **Use constant delays** - same delay for every epic in a phase
2. **Match delay to phase load** - higher load = longer delay
3. **Test with 2 epics first** - verify script works before full launch
4. **Monitor API usage** - track bobcoin consumption
5. **Use building-blocks method** - copy previous phase, modify delay only

### ❌ DON'T
1. **Never use incrementing delays** - causes uneven load distribution
2. **Never use random delays** - makes debugging impossible
3. **Never skip delays** - causes API overload
4. **Never use same delay for all phases** - wastes time on low-load phases
5. **Never generate scripts from scratch** - always copy working template

---

## Scaling Considerations

### 80 Epics vs 9 Epics (Wave 2)

**Wave 2 (9 epics)**:
- Used 2s delays for all phases
- Total launch time: ~18 seconds per phase
- No API rate limit issues
- VM load: minimal

**Wave 4 (80 epics)**:
- Requires 10-25s delays based on phase
- Total launch time: 13-33 minutes per phase
- API rate limits are a concern
- VM load: significant (50+ concurrent agents)

**Key Insight**: Delay requirements scale non-linearly with epic count due to:
- API rate limits (fixed per account)
- VM resource contention (CPU/memory/disk)
- Network bandwidth (fixed per VM)

---

## API Load Distribution

### Phase 0 Example (12s delays)

**Without Delays** (all 80 launch simultaneously):
- 80 jCodemunch requests in <1 second
- Likely rate limit: 429 Too Many Requests
- VM CPU spike: 100% across all cores
- High failure rate

**With 12s Delays** (staggered launch):
- 80 jCodemunch requests over 16 minutes
- Rate: ~5 requests/minute (well below limits)
- VM CPU: gradual ramp-up to ~50% sustained
- Low failure rate

---

## Monitoring During Launch

### Real-Time Checks

```bash
# 1. Check launch progress
ps aux | grep '_p0_' | wc -l  # Count running scripts

# 2. Check file creation
ls docs/brain/EPIC-CCN-*/00-hotspots.md | wc -l  # Count completed

# 3. Check VM load
uptime  # Load average should stay <4.0 on 8-core VM

# 4. Check API usage
grep "Cost:" logs/phase0/*.log | tail -20  # Recent bobcoin usage
```

### Post-Launch Verification

```bash
# 1. Verify all epics launched
ls logs/phase0/*.pid | wc -l  # Should be 80

# 2. Verify all completed
ls docs/brain/EPIC-CCN-*/00-hotspots.md | wc -l  # Should be 80

# 3. Check for errors
grep -i "error\|failed" logs/phase0/*.log | wc -l  # Should be 0

# 4. Extract bobcoin usage
grep "Cost:" logs/phase0/*.log | awk '{sum+=$2} END {print sum}'
```

---

## Failure Recovery

### If Launch Script Aborted

**Scenario**: Launch script stopped after 21/80 epics

**Recovery Steps**:
1. Count completed epics: `ls docs/brain/EPIC-CCN-*/00-hotspots.md | wc -l`
2. Identify completed epic IDs: `ls docs/brain/EPIC-CCN-*/00-hotspots.md | grep -oP 'CCN-\K\d+'`
3. Create new launch script for remaining epics only
4. Use same delay as original (maintain consistency)
5. Launch remaining epics

**Example**: If epics 001-021 completed, launch 022-080:
```bash
EPICS=($(seq -f "%03g" 22 80))  # Start from 022, not 001
```

---

## Comparison with Wave 1 Buffers

### Launch Delays (This Document)
- **When**: DURING phase launch
- **Between**: Individual epic launches
- **Duration**: 10-25 seconds
- **Purpose**: Prevent API/VM overload

### Post-Phase Buffers (Wave 1)
- **When**: AFTER phase completes
- **Between**: Sequential phases
- **Duration**: 2-5 minutes
- **Purpose**: Verification + VM stabilization

**Both are required** for optimal Wave 4 execution.

---

## Success Metrics

### Launch Phase Success
- ✅ All 80 scripts launched
- ✅ Launch completed within estimated time (±10%)
- ✅ No API rate limit errors (429)
- ✅ VM load stayed below 6.0 (on 8-core VM)

### Execution Phase Success
- ✅ All 80 epics completed
- ✅ All expected files created
- ✅ No P0 errors in logs
- ✅ Bobcoin usage within budget

---

## Next Steps

1. **Generate remaining epic scripts** (022-080 if 001-021 completed)
2. **Create master launch script** with constant 12s delay
3. **Upload to VM**
4. **Test with 2 epics** (022-023)
5. **Launch remaining 57 epics** (024-080)
6. **Monitor completion**
7. **Proceed to Phase 1** with corrected delay pattern

---

**Status**: Ready for Phase 0 completion and Phase 1 launch
**Key Takeaway**: Constant delays prevent API overload and enable smooth parallel execution