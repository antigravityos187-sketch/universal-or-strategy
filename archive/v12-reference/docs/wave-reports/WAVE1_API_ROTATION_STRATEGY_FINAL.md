# Wave 1: API Rotation Strategy - Final (50 Agent Cap)

**Date**: 2026-06-14
**Final Version**: Cap all phases at 50 concurrent agents max
**VM Capacity**: n2-standard-8 (60 agents max)
**Target Load**: 83% (50/60 agents)
**Safety Margin**: 17% (10 agents reserved)

---

## Final Configuration Summary

**Peak Agents**: 50 (all phases)
**VM Load**: 83% (safe and efficient)
**Safety Margin**: 17% (10 agents for system overhead)
**Timeline**: 4.2 hours (unchanged)
**Budget**: 6,280 bobcoins (unchanged)

---

## Phase-Specific Delays (50 Agent Cap)

### Delay Calculation Formula

```
Delay = Phase_Avg_Time ÷ Max_Concurrent_Agents
Delay = Phase_Avg_Time ÷ 50
```

### Calculated Delays

| Phase | Avg Time | **Delay (seconds)** | **Peak Agents** | **VM Load** |
|-------|----------|---------------------|-----------------|-------------|
| 0 | 10 min (600s) | 600 ÷ 50 = **12s** | 50 | 83% |
| 1 | 15 min (900s) | 900 ÷ 50 = **18s** | 50 | 83% |
| 2 | 25 min (1500s) | 1500 ÷ 50 = **30s** | 50 | 83% |
| 3 | 10 min (600s) | 600 ÷ 50 = **12s** | 50 | 83% |
| 4 | 10 min (600s) | 600 ÷ 50 = **12s** | 50 | 83% |
| 5 | 45 min (2700s) | 2700 ÷ 50 = **54s** | 50 | 83% |
| 5.V | 30 min (1800s) | 1800 ÷ 50 = **36s** | 50 | 83% |
| 6 | 10 min (600s) | 600 ÷ 50 = **12s** | 50 | 83% |

### Simplified Implementation Delays

| Phase | Delay | Peak Agents | VM Load |
|-------|-------|-------------|---------|
| 0, 3, 4, 6 | **12 sec** | 50 | 83% |
| 1 | **18 sec** | 50 | 83% |
| 2 | **30 sec** | 50 | 83% |
| 5 | **54 sec** (~1 min) | 50 | 83% |
| 5.V | **36 sec** | 50 | 83% |

**Benefits**:
- ✅ All phases at exactly 50 agents (83% load)
- ✅ Consistent VM utilization across all phases
- ✅ 17% safety margin (10 agents) for system overhead
- ✅ Optimal balance between speed and safety

---

## API Rotation Strategy (Unchanged)

### Round-Robin Pattern

**80 epics ÷ 15 APIs = 5.33 epics per API**

```
EPIC-001 → API-1
EPIC-002 → API-2
...
EPIC-015 → API-15
EPIC-016 → API-1  (cycle repeats)
EPIC-017 → API-2
...
EPIC-080 → API-5  (final epic)
```

**Load Distribution**:
- API-1 through API-5: 6 epics each
- API-6 through API-15: 5 epics each

**Implementation**:
```bash
# Calculate API index (1-15) using modulo
API_INDEX=$(( (EPIC_NUM - 1) % 15 + 1 ))
export BOBSHELL_API_KEY=$(jq -r ".apis[${API_INDEX}-1].apikey" api_keys.json)
```

---

## Phase-Specific Buffers (Unchanged)

| Phase | Avg Time | Max Time | Variance | Safety (20%) | **Buffer** |
|-------|----------|----------|----------|--------------|------------|
| 0 | 10 min | 15 min | 5 min | 2 min | **7 min** |
| 1 | 15 min | 20 min | 5 min | 3 min | **8 min** |
| 2 | 25 min | 35 min | 10 min | 5 min | **15 min** |
| 3 | 10 min | 15 min | 5 min | 2 min | **7 min** |
| 4 | 10 min | 15 min | 5 min | 2 min | **7 min** |
| 5 | 45 min | 60 min | 15 min | 9 min | **24 min** |
| 5.V | 30 min | 45 min | 15 min | 6 min | **21 min** |
| 6 | 10 min | 15 min | 5 min | 2 min | **7 min** |

---

## Timeline (Unchanged)

### Catch-Up Phase (EPIC-016-080, 65 epics)

| Phase | Execution | Buffer | Total | Cumulative |
|-------|-----------|--------|-------|------------|
| 0 | 10 min | 7 min | 17 min | 17 min |
| 1 | 15 min | 8 min | 23 min | 40 min |

**Catch-Up Total**: 40 minutes

### All-in-One Phase (All 80 epics)

| Phase | Execution | Buffer | Total | Cumulative |
|-------|-----------|--------|-------|------------|
| 2 | 25 min | 15 min | 40 min | 40 min |
| 3 | 10 min | 7 min | 17 min | 57 min |
| 4 | 10 min | 7 min | 17 min | 74 min |
| 5 | 45 min | 24 min | 69 min | 143 min |
| 5.V | 30 min | 21 min | 51 min | 194 min |
| 6 | 10 min | 7 min | 17 min | 211 min |

**All-in-One Total**: 211 minutes (3.5 hours)

### Grand Total

**Catch-Up**: 40 min
**All-in-One**: 211 min
**Total**: 251 minutes ≈ **4.2 hours**

---

## VM Capacity Validation (50 Agent Cap)

### Peak Concurrency by Phase

| Phase | Delay | Avg Time | Peak Agents | VM Load | Status |
|-------|-------|----------|-------------|---------|--------|
| 0 | 12s | 10 min | 50 | 83% | ✅ Optimal |
| 1 | 18s | 15 min | 50 | 83% | ✅ Optimal |
| 2 | 30s | 25 min | 50 | 83% | ✅ Optimal |
| 3 | 12s | 10 min | 50 | 83% | ✅ Optimal |
| 4 | 12s | 10 min | 50 | 83% | ✅ Optimal |
| 5 | 54s | 45 min | 50 | 83% | ✅ Optimal |
| 5.V | 36s | 30 min | 50 | 83% | ✅ Optimal |
| 6 | 12s | 10 min | 50 | 83% | ✅ Optimal |

**VM Capacity**: 60 agents max (n2-standard-8)
**Peak Load**: 83% (50 agents) across all phases
**Safety Margin**: 17% (10 agents reserved)

**Why 50 Agents is Optimal**:
- ✅ High utilization (83%) without risk
- ✅ Consistent load profile (easier monitoring)
- ✅ 17% buffer for system overhead
- ✅ No risk of VM overload
- ✅ Faster than 45 agents (11% speed gain)

---

## Implementation

### 1. API Rotation

```bash
# Calculate API index (1-15)
API_INDEX=$(( (EPIC_NUM - 1) % 15 + 1 ))
export BOBSHELL_API_KEY=$(jq -r ".apis[${API_INDEX}-1].apikey" api_keys.json)
```

### 2. Phase-Specific Delays

```bash
# Phase 0, 3, 4, 6 (fast phases)
sleep 12

# Phase 1 (fast phase, higher load)
sleep 18

# Phase 2 (medium phase)
sleep 30

# Phase 5 (slow phase)
sleep 54

# Phase 5.V (slow phase)
sleep 36
```

### 3. Phase-Specific Buffers

```bash
# After Phase 0
sleep 420  # 7 min

# After Phase 1
sleep 480  # 8 min

# After Phase 2
sleep 900  # 15 min

# After Phase 3
sleep 420  # 7 min

# After Phase 4
sleep 420  # 7 min

# After Phase 5
sleep 1440  # 24 min

# After Phase 5.V
sleep 1260  # 21 min

# After Phase 6
sleep 420  # 7 min
```

---

## Comparison: All Versions

| Metric | Original | 45 Agents | **50 Agents (Final)** |
|--------|----------|-----------|----------------------|
| **Max Peak** | 60 agents | 45 agents | **50 agents** |
| **Max VM Load** | 100% | 75% | **83%** |
| **Safety Margin** | 0% | 25% | **17%** |
| **Total Time** | 4.2 hours | 4.2 hours | **4.2 hours** |
| **Bobcoins** | 6,280 | 6,280 | **6,280** |
| **Speed vs 45** | +20% | baseline | **+11%** |

**Final Choice**: 50 agents provides optimal balance between speed (83% utilization) and safety (17% margin)

---

## Execution Strategy

### Step 1: Catch-Up Phase 0-1 (40 min)

**Scope**: EPIC-016 through EPIC-080 (65 epics)
**API Rotation**: Cycle through 15 APIs (API-1 to API-15)
**Delays**: 12 sec (Phase 0), 18 sec (Phase 1)
**Buffers**: 7 min (Phase 0), 8 min (Phase 1)
**Peak Load**: 50 agents (83% VM capacity)

### Step 2: SYNC POINT (5 min)

**Verify**: All 80 epics at Phase 1 complete
**Check**: Bobcoin usage, VM load, file counts

### Step 3: All-in-One Phase 2-6 (211 min)

**Scope**: ALL 80 epics (EPIC-001 through EPIC-080)
**API Rotation**: Cycle through 15 APIs (API-1 to API-15)
**Delays**: Phase-specific (12s, 18s, 30s, 54s, 36s)
**Buffers**: Phase-specific (7-24 min)
**Peak Load**: 50 agents (83% VM capacity)

---

## Success Criteria

### Per Phase
- ✅ All 80 epics complete
- ✅ API rotation working (no single API exhausted)
- ✅ Bobcoin usage within budget
- ✅ VM load = 83% throughout (50 agents)
- ✅ No P0 errors in logs

### End of Wave
- ✅ All 80 epics fully complete (Phase 0-6)
- ✅ All 180 methods reduced to CYC ≤8
- ✅ Build passes
- ✅ Total bobcoins ≤ 6,400
- ✅ Total time ≤ 5 hours

---

## Next Steps

1. ~~Test API rotation with first 5 epics~~ ✅ Already done (EPIC-001-015 completed yesterday)
2. **Generate catch-up scripts** with API rotation (EPIC-016-080)
3. **Update master launch script** with final delays (12s, 18s, 30s, 54s, 36s)
4. **Launch catch-up execution** (40 min)
5. **Verify SYNC POINT** (all 80 at Phase 1)
6. **Launch all-in-one execution** (211 min)
7. **Monitor and adjust** if needed

**Total Estimated Time**: 4.2 hours
**Efficiency**: 68% faster than original 13.3 hours
**VM Load**: Consistent 83% (optimal balance)

---

**Document Version**: 3.0 (Final - 50 Agent Cap)
**Last Updated**: 2026-06-14T20:12:00Z
**Previous Versions**: 
- 1.0 (Variable peaks up to 60 agents)
- 2.0 (45 agent cap, 75% load)
**Maintainer**: V12 Orchestration Team
**Status**: APPROVED - Ready for execution
