# Wave 4 Phase Delays Summary

**Date**: 2026-06-14
**Purpose**: Document delay/buffer times between epic launches for all phases
**Reference**: Wave 2 patterns + WAVE4_HANDOFF_CORRECTED.md

---

## Current Status

**Phase 0**: ACTIVE on VM with OLD incrementing delay pattern (12-54s)
- Will complete as-is (~40 minutes total launch time)
- Fixed version ready for future use (constant 12s)

---

## Recommended Delays by Phase

Based on Wave 2 experience and handoff document analysis:

| Phase | Name | Delay | Rationale |
|-------|------|-------|-----------|
| **-1** | Pre-flight | 2s | Quick validation, minimal load |
| **0** | Hotspot | **12s** | jCodemunch queries, moderate load |
| **1** | Scope + Boundary | **12s** | Planning only, low load |
| **1.5** | (merged into 1) | N/A | No longer separate phase |
| **2** | Architecture | **15s** | Jane Street KB queries, higher load |
| **3** | Audit | **12s** | DNA checks, moderate load |
| **4** | Tickets | **10s** | Ticket generation, low load |
| **4.5** | Ticket Review | **12s** | Jane Street KB queries, moderate load |
| **5** | Execution | **25s** | Bob CLI surgery, HIGHEST load |
| **5.V** | Verification | **15s** | Build + test verification, high load |
| **6** | Final Review | **10s** | Report generation, low load |

---

## Delay Selection Criteria

**2 seconds**: Minimal operations (pre-flight checks, file validation)
**10 seconds**: Low-load operations (ticket generation, reports)
**12 seconds**: Standard operations (most phases)
**15 seconds**: Medium-load operations (KB queries, verification)
**20 seconds**: High-load operations (verification, compilation)
**25 seconds**: Highest-load operations (code surgery with Bob CLI)

---

## Wave 2 Historical Data

**Phase 0** (9 epics):
- Used: 2s delay (screen sessions)
- Result: All 9 completed successfully
- Lesson: 2s works for small waves, but 12s safer for 80 epics

**Phase 1** (9 epics):
- Used: 2s delay (screen sessions)
- Result: 3 failed launches (jq/API key bugs)
- Lesson: Delay wasn't the issue, script bugs were

**Phase 2** (9 epics):
- Used: 2s delay (screen sessions)
- Result: Successful
- Lesson: 2s sufficient for 9 epics

---

## Wave 4 Scaling Considerations

**80 epics vs 9 epics**:
- 9x more concurrent agents
- Higher jCodemunch API load
- Higher Bob Shell API load
- More VM resource contention

**Recommended Approach**:
- Use 12s as baseline for most phases
- Increase to 15-20s for high-load phases (2, 5, 5.V)
- Decrease to 10s for low-load phases (4, 6)

---

## Implementation Pattern

**Master Launch Script Template**:
```bash
#!/bin/bash
set -e

REPO_DIR="/home/malhitticrypto/universal-or-strategy"
cd "$REPO_DIR"

PHASE="{PHASE}"  # Replace: 0, 1, 2, 3, 4, 4.5, 5, 5.V, 6

# CONSTANT delay (DO NOT INCREMENT)
DELAY=12  # Adjust per phase: 10, 12, 15, or 20

EPICS=($(seq -f "%03g" 1 80))

for i in "${!EPICS[@]}"; do
    EPIC="${EPICS[$i]}"
    SCRIPT_PATH="${REPO_DIR}/_p${PHASE}_${EPIC}.sh"
    
    echo "[$(date '+%H:%M:%S')] Launching EPIC-CCN-${EPIC} (delay: ${DELAY}s)"
    
    bash "${SCRIPT_PATH}" > "logs/phase${PHASE}/EPIC-CCN-${EPIC}.log" 2>&1 &
    echo $! > "logs/phase${PHASE}/EPIC-CCN-${EPIC}.pid"
    
    # CONSTANT delay - same for ALL epics
    sleep ${DELAY}
done
```

---

## Phase-Specific Recommendations

### Phase 0 (Hotspot Analysis)
- **Delay**: 12s
- **Load**: jCodemunch queries (get_hotspots, get_blast_radius, get_call_hierarchy)
- **Duration**: ~10 min per epic
- **Total Launch**: 80 × 12s = 16 minutes

### Phase 1 (Scope + Boundary)
- **Delay**: 12s
- **Load**: Planning only, minimal API calls
- **Duration**: ~20 min per epic
- **Total Launch**: 16 minutes

### Phase 2 (Architecture)
- **Delay**: 15s (Jane Street KB queries)
- **Load**: Firebase KB queries + jCodemunch
- **Duration**: ~25 min per epic
- **Total Launch**: 80 × 15s = 20 minutes

### Phase 3 (Audit)
- **Delay**: 12s
- **Load**: DNA checks, PR hygiene validation
- **Duration**: ~10 min per epic
- **Total Launch**: 16 minutes

### Phase 4 (Tickets)
- **Delay**: 10s
- **Load**: Ticket generation, low complexity
- **Duration**: ~10 min per epic
- **Total Launch**: 80 × 10s = 13 minutes

### Phase 4.5 (Ticket Review)
- **Delay**: 12s (Jane Street KB queries)
- **Load**: Firebase KB queries
- **Duration**: ~10 min per epic
- **Total Launch**: 16 minutes

### Phase 5 (Execution)
- **Delay**: 25s (Bob CLI surgery)
- **Load**: Code modification, HIGHEST complexity
- **Duration**: ~10 min per ticket (variable)
- **Total Launch**: 80 × 25s = 33 minutes

### Phase 5.V (Verification)
- **Delay**: 15s (build + test)
- **Load**: Compilation, test execution
- **Duration**: ~5 min per ticket
- **Total Launch**: 80 × 15s = 20 minutes

### Phase 6 (Final Review)
- **Delay**: 10s
- **Load**: Report generation, low complexity
- **Duration**: ~10 min per epic
- **Total Launch**: 13 minutes

---

## Total Wave 4 Timeline Estimate

**Launch Times** (sequential):
- Phase -1: 80 × 2s = 3 min
- Phase 0: 80 × 12s = 16 min
- Phase 1: 80 × 12s = 16 min
- Phase 2: 80 × 15s = 20 min
- Phase 3: 80 × 12s = 16 min
- Phase 4: 80 × 10s = 13 min
- Phase 4.5: 80 × 12s = 16 min
- Phase 5: 80 × 25s = 33 min
- Phase 5.V: 80 × 15s = 20 min
- Phase 6: 80 × 10s = 13 min

**Total Launch Time**: ~166 minutes (~2.8 hours)

**Execution Time** (parallel, per phase):
- Phases run in parallel after launch
- Longest phase determines completion time
- Estimated: 60-90 minutes per phase

**Total Wave Time**: ~10-15 hours (with monitoring breaks)

---

## Questions for User

1. **Phase 0 Current Wave**: Let it complete with old delay pattern (12-54s incrementing)?
2. **Future Phases**: Use recommended constant delays (10-20s based on load)?
3. **Phase 5 Delay**: Updated to 25s per user request
4. **Monitoring Strategy**: Check progress every 30 minutes, or more frequent?

---

**Status**: Ready for user review and approval