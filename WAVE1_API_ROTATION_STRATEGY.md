# Wave 1: API Rotation & Phase Buffer Strategy

**Date**: 2026-06-14
**Purpose**: Optimize 80-epic execution with API rotation and phase-specific buffers
**Reference**: User feedback on API cycling and phase timing

---

## API Rotation Strategy

### Problem
- 80 epics, 15 APIs available
- Concurrent API usage causes rate limiting
- Need to distribute load evenly

### Solution: Round-Robin API Rotation

**Pattern**: Cycle through 15 APIs, then repeat

```
EPIC-001 → API-1
EPIC-002 → API-2
...
EPIC-015 → API-15
EPIC-016 → API-1  (cycle repeats)
EPIC-017 → API-2
...
EPIC-030 → API-15
EPIC-031 → API-1  (cycle repeats again)
```

**Benefits**:
- Each API gets ~5-6 epics (80 ÷ 15 ≈ 5.33)
- Natural spacing between same-API requests
- Even load distribution
- No API exhaustion

### API Allocation Table

| Epic Range | API Cycle | Epics per API |
|------------|-----------|---------------|
| 001-015 | Cycle 1 | 1 epic each |
| 016-030 | Cycle 2 | 2 epics each |
| 031-045 | Cycle 3 | 3 epics each |
| 046-060 | Cycle 4 | 4 epics each |
| 061-075 | Cycle 5 | 5 epics each |
| 076-080 | Cycle 6 (partial) | 6 epics for API-1 to API-5 |

**Maximum Load**: API-1 through API-5 handle 6 epics each, API-6 through API-15 handle 5 epics each

---

## Phase-Specific Execution Times

### Observed Times (from Wave 2 data)

| Phase | Avg Time | Min Time | Max Time | Complexity |
|-------|----------|----------|----------|------------|
| **0** | 10 min | 5 min | 15 min | Low (jCodemunch query) |
| **1** | 15 min | 10 min | 20 min | Medium (scope analysis) |
| **2** | 25 min | 20 min | 35 min | High (architecture planning) |
| **3** | 10 min | 5 min | 15 min | Low (audit checks) |
| **4** | 10 min | 5 min | 15 min | Low (ticket generation) |
| **5** | 45 min | 30 min | 60 min | **Very High** (code extraction) |
| **5.V** | 30 min | 20 min | 45 min | High (verification) |
| **6** | 10 min | 5 min | 15 min | Low (final review) |

### Key Insights

**Fast Phases** (5-15 min): 0, 1, 3, 4, 6
- Low complexity
- Mostly analysis/reporting
- Can handle high concurrency

**Medium Phase** (20-35 min): 2
- Architecture planning
- Moderate complexity
- Medium concurrency safe

**Slow Phases** (30-60 min): 5, 5.V
- Code modification (Phase 5)
- Build verification (Phase 5.V)
- High complexity
- Lower concurrency needed

---

## Optimized Phase Buffers

### Buffer Calculation Formula

```
Buffer = (Max Time - Avg Time) + Safety Margin
```

**Safety Margin**: 20% of average time (accounts for VM load variance)

### Phase-Specific Buffers

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

### Rationale

**Short Buffers** (7-8 min): Fast phases
- Quick execution
- Low variance
- Minimal safety margin needed

**Medium Buffer** (15 min): Phase 2
- Longer execution
- Moderate variance
- Architecture complexity varies

**Long Buffers** (21-24 min): Phases 5, 5.V
- Longest execution
- High variance (depends on method complexity)
- Code changes + build verification
- Critical phases (need extra safety)

---

## Staggered Launch Delays

### Delay Calculation

**Goal**: Prevent VM overload while maximizing throughput

**Formula**: 
```
Delay = Phase_Avg_Time ÷ Max_Concurrent_Agents
```

### Phase-Specific Delays

| Phase | Avg Time | Max Concurrent | **Delay** | Rationale |
|-------|----------|----------------|-----------|-----------|
| 0 | 10 min | 50 | **12 sec** | Fast phase, high concurrency safe |
| 1 | 15 min | 50 | **18 sec** | Fast phase, high concurrency safe |
| 2 | 25 min | 40 | **38 sec** | Medium phase, moderate concurrency |
| 3 | 10 min | 50 | **12 sec** | Fast phase, high concurrency safe |
| 4 | 10 min | 50 | **12 sec** | Fast phase, high concurrency safe |
| 5 | 45 min | 20 | **135 sec** | Slow phase, low concurrency needed |
| 5.V | 30 min | 30 | **60 sec** | Slow phase, moderate concurrency |
| 6 | 10 min | 50 | **12 sec** | Fast phase, high concurrency safe |

### Simplified Delays (for implementation)

| Phase | Delay | Concurrent Peak |
|-------|-------|-----------------|
| 0, 1, 3, 4, 6 | **15 sec** | ~40-50 agents |
| 2 | **40 sec** | ~30-40 agents |
| 5 | **2 min** | ~15-20 agents |
| 5.V | **1 min** | ~25-30 agents |

---

## Revised Timeline with Buffers

### Catch-Up Phase (EPIC-016-080, 65 epics)

| Phase | Execution | Buffer | Total | Cumulative |
|-------|-----------|--------|-------|------------|
| 0 | 10 min | 7 min | 17 min | 17 min |
| 1 | 15 min | 8 min | 23 min | 40 min |

**Catch-Up Total**: 40 minutes (0.67 hours)

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

**Previous Estimate**: 13.3 hours (too conservative)
**Optimized Estimate**: 4.2 hours (67% faster)

---

## Bobcoin Budget (Unchanged)

**Catch-Up** (65 epics × 8 bobcoins): 520 bobcoins
**All-in-One** (80 epics × 72 bobcoins): 5,760 bobcoins
**Total**: 6,280 bobcoins (98% of 6,400 limit)

---

## Implementation Changes

### 1. API Rotation in Scripts

**Old Approach**: Fixed API per epic
```bash
export BOBSHELL_API_KEY='bob_prod_bob-admin_001'  # Same for all
```

**New Approach**: Modulo-based rotation
```bash
# Calculate API index (1-15)
API_INDEX=$(( (EPIC_NUM - 1) % 15 + 1 ))
export BOBSHELL_API_KEY=$(jq -r ".apis[${API_INDEX}-1].apikey" api_keys.json)
```

### 2. Phase-Specific Delays

**Old Approach**: Fixed 10-30 sec delays
```bash
sleep 10  # Same for all phases
```

**New Approach**: Phase-specific delays
```bash
# Phase 0, 1, 3, 4, 6
sleep 15

# Phase 2
sleep 40

# Phase 5
sleep 120

# Phase 5.V
sleep 60
```

### 3. Phase-Specific Buffers

**Old Approach**: Fixed 2-10 min buffers
```bash
sleep 600  # 10 min for all
```

**New Approach**: Phase-specific buffers
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

## VM Capacity Validation

### Peak Concurrency by Phase

| Phase | Delay | Avg Time | Peak Agents | VM Load |
|-------|-------|----------|-------------|---------|
| 0 | 15s | 10 min | 40 | 67% |
| 1 | 15s | 15 min | 60 | 100% |
| 2 | 40s | 25 min | 38 | 63% |
| 3 | 15s | 10 min | 40 | 67% |
| 4 | 15s | 10 min | 40 | 67% |
| 5 | 120s | 45 min | 23 | 38% |
| 5.V | 60s | 30 min | 30 | 50% |
| 6 | 15s | 10 min | 40 | 67% |

**VM Capacity**: 60 agents max (n2-standard-8)
**Peak Load**: Phase 1 at 100% (60 agents)
**Critical**: Phase 1 hits capacity limit

### Mitigation for Phase 1

**Option A**: Increase delay to 20 sec (reduces peak to 45 agents, 75% load)
**Option B**: Accept 100% load (Phase 1 is fast, low risk)
**Option C**: Upgrade VM to n2-standard-16 (120 agents capacity)

**Recommendation**: Option A (increase Phase 1 delay to 20 sec)

---

## Revised Execution Strategy

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
**Delays**: Phase-specific (15s, 20s, 40s, 120s, 60s)
**Buffers**: Phase-specific (7-24 min)

---

## Success Criteria

### Per Phase
- ✅ All 80 epics complete
- ✅ API rotation working (no single API exhausted)
- ✅ Bobcoin usage within budget
- ✅ VM load <100% throughout
- ✅ No P0 errors in logs

### End of Wave
- ✅ All 80 epics fully complete (Phase 0-6)
- ✅ All 180 methods reduced to CYC ≤8
- ✅ Build passes
- ✅ Total bobcoins ≤ 6,400
- ✅ Total time ≤ 5 hours

---

## Next Steps

1. **Generate catch-up scripts** with API rotation (EPIC-016-080)
2. **Update master launch script** with optimized delays and buffers
3. **Test API rotation** with first 5 epics
4. **Launch catch-up execution** (40 min)
5. **Verify SYNC POINT** (all 80 at Phase 1)
6. **Launch all-in-one execution** (211 min)
7. **Monitor and adjust** if needed

**Total Estimated Time**: 4.2 hours (vs 13.3 hours original)
**Efficiency Gain**: 68% faster execution

---

**Document Version**: 1.0
**Last Updated**: 2026-06-14T20:00:00Z
**Maintainer**: V12 Orchestration Team