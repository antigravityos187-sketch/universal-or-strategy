# Wave 1: Polling Strategy - Based on Wave 2 Actual Data

**Date**: 2026-06-14
**Source**: Wave 2 actual execution times (7 epics, 13 hours wall time)
**Purpose**: Calculate realistic polling intervals after each phase

---

## Wave 2 Actual Timing Data

### Per-Epic Average Times (From Wave 2 Report)

| Phase | Avg Time per Epic | Notes |
|-------|-------------------|-------|
| **0** (Hotspot) | 10 min | jCodemunch queries |
| **1** (Scope) | 10 min | Scope analysis |
| **1.5** (Boundary) | 10 min | Scope validation |
| **2** (Architecture) | 25 min | Architecture planning |
| **3** (Audit) | 10 min | DNA & PR audit |
| **4** (Tickets) | 10 min | Ticket generation |
| **5** (Execution) | 60 min | **SLOWEST** - Code extraction |
| **5.V** (Verification) | 30 min | Build + verification |
| **6** (Review) | 10 min | Final review |

### Key Observations

**Fast Phases** (10 min avg): 0, 1, 1.5, 3, 4, 6
- Mostly analysis/planning
- Low variance
- Quick completion

**Medium Phase** (25 min avg): 2
- Architecture planning
- Moderate complexity

**Slow Phases** (30-60 min avg): 5, 5.V
- Phase 5: Code extraction (60 min)
- Phase 5.V: Build verification (30 min)
- High variance (depends on method complexity)

---

## Polling Strategy Calculation

### Formula

```
Polling Interval = Max_Time + Safety_Buffer
Max_Time = Avg_Time + (Avg_Time × Variance_Factor)
Safety_Buffer = 20% of Avg_Time
```

### Variance Factors (From Wave 2)

| Phase | Variance Factor | Rationale |
|-------|----------------|-----------|
| 0, 1, 3, 4, 6 | 50% | Fast phases, low variance |
| 2 | 40% | Medium phase, moderate variance |
| 5 | 50% | Slow phase, high variance (method complexity) |
| 5.V | 50% | Slow phase, build time varies |

---

## Calculated Polling Intervals

### Phase 0 (Hotspot Analysis)

```
Avg Time: 10 min
Max Time: 10 + (10 × 0.5) = 15 min
Safety Buffer: 10 × 0.2 = 2 min
Polling Interval: 15 + 2 = 17 min
```

**Recommended**: **17 minutes** (round to **20 min** for simplicity)

---

### Phase 1 (Scope Definition)

```
Avg Time: 10 min
Max Time: 10 + (10 × 0.5) = 15 min
Safety Buffer: 10 × 0.2 = 2 min
Polling Interval: 15 + 2 = 17 min
```

**Recommended**: **17 minutes** (round to **20 min** for simplicity)

---

### Phase 2 (Architecture Planning)

```
Avg Time: 25 min
Max Time: 25 + (25 × 0.4) = 35 min
Safety Buffer: 25 × 0.2 = 5 min
Polling Interval: 35 + 5 = 40 min
```

**Recommended**: **40 minutes**

---

### Phase 3 (DNA & PR Audit)

```
Avg Time: 10 min
Max Time: 10 + (10 × 0.5) = 15 min
Safety Buffer: 10 × 0.2 = 2 min
Polling Interval: 15 + 2 = 17 min
```

**Recommended**: **17 minutes** (round to **20 min** for simplicity)

---

### Phase 4 (Ticket Generation)

```
Avg Time: 10 min
Max Time: 10 + (10 × 0.5) = 15 min
Safety Buffer: 10 × 0.2 = 2 min
Polling Interval: 15 + 2 = 17 min
```

**Recommended**: **17 minutes** (round to **20 min** for simplicity)

---

### Phase 5 (Ticket Execution)

```
Avg Time: 60 min
Max Time: 60 + (60 × 0.5) = 90 min
Safety Buffer: 60 × 0.2 = 12 min
Polling Interval: 90 + 12 = 102 min
```

**Recommended**: **102 minutes** (round to **105 min** or **1h 45m**)

---

### Phase 5.V (Verification)

```
Avg Time: 30 min
Max Time: 30 + (30 × 0.5) = 45 min
Safety Buffer: 30 × 0.2 = 6 min
Polling Interval: 45 + 6 = 51 min
```

**Recommended**: **51 minutes** (round to **55 min**)

---

### Phase 6 (Final Review)

```
Avg Time: 10 min
Max Time: 10 + (10 × 0.5) = 15 min
Safety Buffer: 10 × 0.2 = 2 min
Polling Interval: 15 + 2 = 17 min
```

**Recommended**: **17 minutes** (round to **20 min** for simplicity)

---

## Final Polling Intervals (Simplified)

| Phase | Calculated | **Simplified** | Sleep Command |
|-------|-----------|----------------|---------------|
| 0 | 17 min | **20 min** | `sleep 1200` |
| 1 | 17 min | **20 min** | `sleep 1200` |
| 2 | 40 min | **40 min** | `sleep 2400` |
| 3 | 17 min | **20 min** | `sleep 1200` |
| 4 | 17 min | **20 min** | `sleep 1200` |
| 5 | 102 min | **105 min** | `sleep 6300` |
| 5.V | 51 min | **55 min** | `sleep 3300` |
| 6 | 17 min | **20 min** | `sleep 1200` |

---

## Comparison: Old vs New Buffers

| Phase | Old Buffer | New Polling | Change | Rationale |
|-------|-----------|-------------|--------|-----------|
| 0 | 7 min | **20 min** | +13 min | More realistic for 80 epics |
| 1 | 8 min | **20 min** | +12 min | More realistic for 80 epics |
| 2 | 15 min | **40 min** | +25 min | Accounts for variance |
| 3 | 7 min | **20 min** | +13 min | More realistic for 80 epics |
| 4 | 7 min | **20 min** | +13 min | More realistic for 80 epics |
| 5 | 24 min | **105 min** | +81 min | **CRITICAL** - was too short |
| 5.V | 21 min | **55 min** | +34 min | **CRITICAL** - was too short |
| 6 | 7 min | **20 min** | +13 min | More realistic for 80 epics |

**Key Changes**:
- ✅ Phase 5 buffer increased 4.4x (24 → 105 min)
- ✅ Phase 5.V buffer increased 2.6x (21 → 55 min)
- ✅ All fast phases standardized to 20 min
- ✅ Based on actual Wave 2 data (not estimates)

---

## Revised Timeline with New Polling

### Catch-Up Phase (EPIC-016-080, 65 epics)

| Phase | Execution | Polling | Total | Cumulative |
|-------|-----------|---------|-------|------------|
| 0 | 10 min | 20 min | 30 min | 30 min |
| 1 | 10 min | 20 min | 30 min | 60 min |

**Catch-Up Total**: 60 minutes (1 hour)

### All-in-One Phase (All 80 epics)

| Phase | Execution | Polling | Total | Cumulative |
|-------|-----------|---------|-------|------------|
| 2 | 25 min | 40 min | 65 min | 65 min |
| 3 | 10 min | 20 min | 30 min | 95 min |
| 4 | 10 min | 20 min | 30 min | 125 min |
| 5 | 60 min | 105 min | 165 min | 290 min |
| 5.V | 30 min | 55 min | 85 min | 375 min |
| 6 | 10 min | 20 min | 30 min | 405 min |

**All-in-One Total**: 405 minutes (6.75 hours)

### Grand Total

**Catch-Up**: 60 min (1 hour)
**All-in-One**: 405 min (6.75 hours)
**Total**: 465 minutes ≈ **7.75 hours**

**Previous Estimate**: 4.2 hours (too optimistic)
**New Estimate**: 7.75 hours (based on Wave 2 actuals)
**Difference**: +3.55 hours (more realistic)

---

## Why Polling is Critical

### Without Adequate Polling

**Problem**: Launch next phase before previous phase completes
**Result**: 
- Agents start working on incomplete data
- File conflicts
- Wasted bobcoins
- Failed phases

### With Adequate Polling

**Benefit**: 
- ✅ All agents complete before next phase starts
- ✅ Clean handoff between phases
- ✅ No file conflicts
- ✅ Predictable execution

---

## Implementation

### Master Launch Script Pattern

```bash
#!/bin/bash

# Phase 0 (Catch-up: EPIC-016-080)
echo "Starting Phase 0 (Hotspot Analysis) - 65 epics"
./launch_phase0_catch_up.sh
echo "Polling Phase 0 completion (20 min)..."
sleep 1200

# Phase 1 (Catch-up: EPIC-016-080)
echo "Starting Phase 1 (Scope Definition) - 65 epics"
./launch_phase1_catch_up.sh
echo "Polling Phase 1 completion (20 min)..."
sleep 1200

# SYNC POINT
echo "All 80 epics at Phase 1 - verifying..."
./verify_sync_point.sh
sleep 300  # 5 min buffer

# Phase 2 (All 80 epics)
echo "Starting Phase 2 (Architecture Planning) - 80 epics"
./launch_phase2_all.sh
echo "Polling Phase 2 completion (40 min)..."
sleep 2400

# Phase 3 (All 80 epics)
echo "Starting Phase 3 (DNA & PR Audit) - 80 epics"
./launch_phase3_all.sh
echo "Polling Phase 3 completion (20 min)..."
sleep 1200

# Phase 4 (All 80 epics)
echo "Starting Phase 4 (Ticket Generation) - 80 epics"
./launch_phase4_all.sh
echo "Polling Phase 4 completion (20 min)..."
sleep 1200

# Phase 5 (All 80 epics)
echo "Starting Phase 5 (Ticket Execution) - 80 epics"
./launch_phase5_all.sh
echo "Polling Phase 5 completion (105 min)..."
sleep 6300

# Phase 5.V (All 80 epics)
echo "Starting Phase 5.V (Verification) - 80 epics"
./launch_phase5v_all.sh
echo "Polling Phase 5.V completion (55 min)..."
sleep 3300

# Phase 6 (All 80 epics)
echo "Starting Phase 6 (Final Review) - 80 epics"
./launch_phase6_all.sh
echo "Polling Phase 6 completion (20 min)..."
sleep 1200

echo "Wave 1 complete!"
```

---

## Success Criteria

### Per Phase
- ✅ All epics complete before polling ends
- ✅ No agents still running when next phase starts
- ✅ Clean file handoff (no conflicts)

### Overall
- ✅ Total time ≤ 8 hours (7.75 hours estimated)
- ✅ Zero phase conflicts
- ✅ All 80 epics complete
- ✅ Bobcoins ≤ 6,400

---

**Document Version**: 1.0
**Last Updated**: 2026-06-14T20:17:00Z
**Source**: Wave 2 actual timing data (7 epics, 13 hours)
**Status**: Ready for implementation