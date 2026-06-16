# Wave 1: API Rotation Strategy - Revised (45 Agent Cap)

**Date**: 2026-06-14
**Revision**: Cap all phases at 45 concurrent agents max
**Reference**: User feedback - standardize VM load across all phases

---

## Revision Summary

**Change**: Cap all phases at 45 concurrent agents (75% of 60-agent VM capacity)
**Rationale**: 
- Consistent VM load across all phases
- 25% safety margin for system overhead
- Prevents Phase 1 from hitting 100% capacity
- Simplifies monitoring (same peak for all phases)

---

## Revised Phase-Specific Delays

### Delay Calculation Formula

```
Delay = Phase_Avg_Time ÷ Max_Concurrent_Agents
Delay = Phase_Avg_Time ÷ 45
```

### Revised Delays

| Phase | Avg Time | Old Delay | Old Peak | **New Delay** | **New Peak** |
|-------|----------|-----------|----------|---------------|--------------|
| 0 | 10 min | 12s | 50 | **13s** | 45 |
| 1 | 15 min | 18s | 60 | **20s** | 45 |
| 2 | 25 min | 38s | 40 | **33s** | 45 |
| 3 | 10 min | 12s | 50 | **13s** | 45 |
| 4 | 10 min | 12s | 50 | **13s** | 45 |
| 5 | 45 min | 135s | 20 | **60s** | 45 |
| 5.V | 30 min | 60s | 30 | **40s** | 45 |
| 6 | 10 min | 12s | 50 | **13s** | 45 |

### Simplified Implementation Delays

| Phase | Delay | Peak Agents | VM Load |
|-------|-------|-------------|---------|
| 0, 3, 4, 6 | **15 sec** | 40-45 | 67-75% |
| 1 | **20 sec** | 45 | 75% |
| 2 | **35 sec** | 43 | 72% |
| 5 | **60 sec** | 45 | 75% |
| 5.V | **40 sec** | 45 | 75% |

**Benefits**:
- ✅ All phases stay at or below 45 agents
- ✅ Consistent 75% VM load (safe margin)
- ✅ No phase hits 100% capacity
- ✅ Easier to monitor (same peak everywhere)

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

## Revised Timeline (Slightly Longer)

### Catch-Up Phase (EPIC-016-080, 65 epics)

| Phase | Execution | Buffer | Total | Cumulative |
|-------|-----------|--------|-------|------------|
| 0 | 10 min | 7 min | 17 min | 17 min |
| 1 | 15 min | 8 min | 23 min | 40 min |

**Catch-Up Total**: 40 minutes (unchanged)

### All-in-One Phase (All 80 epics)

| Phase | Execution | Buffer | Total | Cumulative |
|-------|-----------|--------|-------|------------|
| 2 | 25 min | 15 min | 40 min | 40 min |
| 3 | 10 min | 7 min | 17 min | 57 min |
| 4 | 10 min | 7 min | 17 min | 74 min |
| 5 | 45 min | 24 min | 69 min | 143 min |
| 5.V | 30 min | 21 min | 51 min | 194 min |
| 6 | 10 min | 7 min | 17 min | 211 min |

**All-in-One Total**: 211 minutes (3.5 hours, unchanged)

### Grand Total

**Catch-Up**: 40 min
**All-in-One**: 211 min
**Total**: 251 minutes ≈ **4.2 hours** (unchanged)

---

## VM Capacity Validation (Revised)

### Peak Concurrency by Phase (45 Agent Cap)

| Phase | Delay | Avg Time | Peak Agents | VM Load | Status |
|-------|-------|----------|-------------|---------|--------|
| 0 | 15s | 10 min | 40 | 67% | ✅ Safe |
| 1 | 20s | 15 min | 45 | 75% | ✅ Safe |
| 2 | 35s | 25 min | 43 | 72% | ✅ Safe |
| 3 | 15s | 10 min | 40 | 67% | ✅ Safe |
| 4 | 15s | 10 min | 40 | 67% | ✅ Safe |
| 5 | 60s | 45 min | 45 | 75% | ✅ Safe |
| 5.V | 40s | 30 min | 45 | 75% | ✅ Safe |
| 6 | 15s | 10 min | 40 | 67% | ✅ Safe |

**VM Capacity**: 60 agents max (n2-standard-8)
**Peak Load**: 75% (45 agents) across all phases
**Safety Margin**: 25% (15 agents reserved)

**Benefits**:
- ✅ No phase exceeds 75% capacity
- ✅ Consistent load profile (easier monitoring)
- ✅ 25% buffer for system overhead
- ✅ No risk of VM overload

---

## Implementation Changes

### 1. API Rotation (Unchanged)

```bash
# Calculate API index (1-15)
API_INDEX=$(( (EPIC_NUM - 1) % 15 + 1 ))
export BOBSHELL_API_KEY=$(jq -r ".apis[${API_INDEX}-1].apikey" api_keys.json)
```

### 2. Phase-Specific Delays (Revised)

```bash
# Phase 0, 3, 4, 6 (fast phases)
sleep 15

# Phase 1 (fast phase, higher load)
sleep 20

# Phase 2 (medium phase)
sleep 35

# Phase 5 (slow phase)
sleep 60

# Phase 5.V (slow phase)
sleep 40
```

### 3. Phase-Specific Buffers (Unchanged)

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

## Comparison: Old vs New

| Metric | Old Strategy | New Strategy | Change |
|--------|--------------|--------------|--------|
| **Max Peak** | 60 agents (Phase 1) | 45 agents (all phases) | -25% |
| **Max VM Load** | 100% (Phase 1) | 75% (all phases) | -25% |
| **Safety Margin** | 0% (Phase 1) | 25% (all phases) | +25% |
| **Total Time** | 4.2 hours | 4.2 hours | No change |
| **Bobcoins** | 6,280 | 6,280 | No change |

**Key Improvement**: Consistent 75% load across all phases (no 100% spikes)

---

## Execution Strategy (Unchanged)

### Step 1: Catch-Up Phase 0-1 (40 min)

**Scope**: EPIC-016 through EPIC-080 (65 epics)
**API Rotation**: Cycle through 15 APIs (API-1 to API-15)
**Delays**: 15 sec (Phase 0), 20 sec (Phase 1)
**Buffers**: 7 min (Phase 0), 8 min (Phase 1)

### Step 2: SYNC POINT (5 min)

**Verify**: All 80 epics at Phase 1 complete
**Check**: Bobcoin usage, VM load, file counts

### Step 3: All-in-One Phase 2-6 (211 min)

**Scope**: ALL 80 epics (EPIC-001 through EPIC-080)
**API Rotation**: Cycle through 15 APIs (API-1 to API-15)
**Delays**: Phase-specific (15s, 20s, 35s, 60s, 40s)
**Buffers**: Phase-specific (7-24 min)

---

## Success Criteria (Unchanged)

### Per Phase
- ✅ All 80 epics complete
- ✅ API rotation working (no single API exhausted)
- ✅ Bobcoin usage within budget
- ✅ VM load ≤75% throughout (new criterion)
- ✅ No P0 errors in logs

### End of Wave
- ✅ All 80 epics fully complete (Phase 0-6)
- ✅ All 180 methods reduced to CYC ≤8
- ✅ Build passes
- ✅ Total bobcoins ≤ 6,400
- ✅ Total time ≤ 5 hours

---

## Next Steps

1. ~~Test API rotation with first 5 epics~~ (Already done - EPIC-001-015 completed yesterday)
2. **Generate catch-up scripts** with API rotation (EPIC-016-080)
3. **Update master launch script** with revised delays (15s, 20s, 35s, 60s, 40s)
4. **Launch catch-up execution** (40 min)
5. **Verify SYNC POINT** (all 80 at Phase 1)
6. **Launch all-in-one execution** (211 min)
7. **Monitor and adjust** if needed

**Total Estimated Time**: 4.2 hours (unchanged)
**Efficiency**: 68% faster than original 13.3 hours
**VM Load**: Consistent 75% (safe and predictable)

---

**Document Version**: 2.0 (45 Agent Cap)
**Last Updated**: 2026-06-14T20:08:00Z
**Previous Version**: 1.0 (Variable peaks up to 60 agents)
**Maintainer**: V12 Orchestration Team