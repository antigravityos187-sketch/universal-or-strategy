# Wave 1: Polling Strategy - Parallel Execution Model

**Date**: 2026-06-14
**Model**: Parallel execution with staggered launches
**Key Insight**: Polling = Single agent execution time + buffer (NOT sum of all agents)

---

## Parallel vs Sequential Execution

### Sequential (WRONG - What Wave 2 Report Showed)
```
7 epics × 10 min each = 70 min total
```
This is the **cumulative time** if running one after another.

### Parallel (CORRECT - What We're Actually Doing)
```
Launch Epic 1 at T+0s
Launch Epic 2 at T+12s
Launch Epic 3 at T+24s
...
Launch Epic 80 at T+948s (15.8 min)

All complete by: T + 15.8 min (launch time) + 10 min (execution) = ~26 min
```

**Polling Time** = Time for **slowest single agent** to complete + buffer

---

## Estimated Single-Agent Execution Times

### Based on Typical Bob CLI Performance

| Phase | Estimated Time | Complexity | Notes |
|-------|---------------|------------|-------|
| **0** (Hotspot) | **2-3 min** | Low | jCodemunch queries (fast) |
| **1** (Scope) | **3-5 min** | Low | Analysis only |
| **2** (Architecture) | **5-8 min** | Medium | Planning + diagrams |
| **3** (Audit) | **2-3 min** | Low | Checks only |
| **4** (Tickets) | **2-3 min** | Low | Generation only |
| **5** (Execution) | **10-15 min** | High | Code extraction + build |
| **5.V** (Verification) | **5-8 min** | Medium | Build + verify |
| **6** (Review) | **2-3 min** | Low | Report generation |

---

## Polling Calculation (Parallel Model)

### Formula

```
Polling Time = Max_Single_Agent_Time + Launch_Spread + Safety_Buffer

Where:
- Max_Single_Agent_Time = Longest single agent execution
- Launch_Spread = Time to launch all 80 agents (80 × delay)
- Safety_Buffer = 20% of Max_Single_Agent_Time
```

---

## Phase 0 (Hotspot Analysis)

### Calculation
```
Single Agent Time: 3 min (conservative)
Launch Spread: 80 epics × 12s = 960s = 16 min
Safety Buffer: 3 × 0.2 = 0.6 min
Polling Time: 3 + 16 + 0.6 = 19.6 min
```

**Recommended**: **20 minutes**

---

## Phase 1 (Scope Definition)

### Calculation
```
Single Agent Time: 5 min (conservative)
Launch Spread: 80 epics × 18s = 1440s = 24 min
Safety Buffer: 5 × 0.2 = 1 min
Polling Time: 5 + 24 + 1 = 30 min
```

**Recommended**: **30 minutes**

---

## Phase 2 (Architecture Planning)

### Calculation
```
Single Agent Time: 8 min (conservative)
Launch Spread: 80 epics × 30s = 2400s = 40 min
Safety Buffer: 8 × 0.2 = 1.6 min
Polling Time: 8 + 40 + 1.6 = 49.6 min
```

**Recommended**: **50 minutes**

---

## Phase 3 (DNA & PR Audit)

### Calculation
```
Single Agent Time: 3 min (conservative)
Launch Spread: 80 epics × 12s = 960s = 16 min
Safety Buffer: 3 × 0.2 = 0.6 min
Polling Time: 3 + 16 + 0.6 = 19.6 min
```

**Recommended**: **20 minutes**

---

## Phase 4 (Ticket Generation)

### Calculation
```
Single Agent Time: 3 min (conservative)
Launch Spread: 80 epics × 12s = 960s = 16 min
Safety Buffer: 3 × 0.2 = 0.6 min
Polling Time: 3 + 16 + 0.6 = 19.6 min
```

**Recommended**: **20 minutes**

---

## Phase 5 (Ticket Execution)

### Calculation
```
Single Agent Time: 15 min (conservative)
Launch Spread: 80 epics × 54s = 4320s = 72 min
Safety Buffer: 15 × 0.2 = 3 min
Polling Time: 15 + 72 + 3 = 90 min
```

**Recommended**: **90 minutes** (1.5 hours)

---

## Phase 5.V (Verification)

### Calculation
```
Single Agent Time: 8 min (conservative)
Launch Spread: 80 epics × 36s = 2880s = 48 min
Safety Buffer: 8 × 0.2 = 1.6 min
Polling Time: 8 + 48 + 1.6 = 57.6 min
```

**Recommended**: **60 minutes** (1 hour)

---

## Phase 6 (Final Review)

### Calculation
```
Single Agent Time: 3 min (conservative)
Launch Spread: 80 epics × 12s = 960s = 16 min
Safety Buffer: 3 × 0.2 = 0.6 min
Polling Time: 3 + 16 + 0.6 = 19.6 min
```

**Recommended**: **20 minutes**

---

## Final Polling Intervals (Parallel Model)

| Phase | Single Agent | Launch Spread | **Polling** | Sleep Command |
|-------|-------------|---------------|-------------|---------------|
| 0 | 3 min | 16 min | **20 min** | `sleep 1200` |
| 1 | 5 min | 24 min | **30 min** | `sleep 1800` |
| 2 | 8 min | 40 min | **50 min** | `sleep 3000` |
| 3 | 3 min | 16 min | **20 min** | `sleep 1200` |
| 4 | 3 min | 16 min | **20 min** | `sleep 1200` |
| 5 | 15 min | 72 min | **90 min** | `sleep 5400` |
| 5.V | 8 min | 48 min | **60 min** | `sleep 3600` |
| 6 | 3 min | 16 min | **20 min** | `sleep 1200` |

---

## Revised Timeline (Parallel Model)

### Catch-Up Phase (EPIC-016-080, 65 epics)

| Phase | Single Agent | Launch Spread | Polling | Total |
|-------|-------------|---------------|---------|-------|
| 0 | 3 min | 13 min (65×12s) | 20 min | 20 min |
| 1 | 5 min | 19.5 min (65×18s) | 30 min | 30 min |

**Catch-Up Total**: 50 minutes

### All-in-One Phase (All 80 epics)

| Phase | Single Agent | Launch Spread | Polling | Total |
|-------|-------------|---------------|---------|-------|
| 2 | 8 min | 40 min | 50 min | 50 min |
| 3 | 3 min | 16 min | 20 min | 20 min |
| 4 | 3 min | 16 min | 20 min | 20 min |
| 5 | 15 min | 72 min | 90 min | 90 min |
| 5.V | 8 min | 48 min | 60 min | 60 min |
| 6 | 3 min | 16 min | 20 min | 20 min |

**All-in-One Total**: 260 minutes (4.3 hours)

### Grand Total

**Catch-Up**: 50 min
**All-in-One**: 260 min (4.3 hours)
**Total**: 310 minutes ≈ **5.2 hours**

---

## Comparison: Sequential vs Parallel

| Model | Polling Basis | Phase 5 Polling | Total Time |
|-------|--------------|-----------------|------------|
| **Sequential** (WRONG) | Sum of all agents | 105 min | 7.75 hours |
| **Parallel** (CORRECT) | Single agent + spread | 90 min | 5.2 hours |
| **Difference** | - | -15 min | -2.55 hours |

**Key Insight**: Parallel model is **33% faster** because we're not waiting for all agents sequentially.

---

## Why Launch Spread Matters

### Example: Phase 0 with 80 Epics

```
T+0s:    Launch EPIC-001 (starts immediately)
T+12s:   Launch EPIC-002 (starts immediately)
T+24s:   Launch EPIC-003 (starts immediately)
...
T+948s:  Launch EPIC-080 (starts immediately)

T+948s + 3min = T+1128s (18.8 min): EPIC-080 completes
T+0s + 3min = T+180s (3 min): EPIC-001 completes

Latest completion: T+1128s (18.8 min)
Polling time: 20 min (includes 1.2 min buffer)
```

**All 80 agents complete within 20 minutes** (not 80 × 3 min = 240 min)

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
echo "Polling Phase 1 completion (30 min)..."
sleep 1800

# SYNC POINT
echo "All 80 epics at Phase 1 - verifying..."
./verify_sync_point.sh
sleep 300  # 5 min buffer

# Phase 2 (All 80 epics)
echo "Starting Phase 2 (Architecture Planning) - 80 epics"
./launch_phase2_all.sh
echo "Polling Phase 2 completion (50 min)..."
sleep 3000

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
echo "Polling Phase 5 completion (90 min)..."
sleep 5400

# Phase 5.V (All 80 epics)
echo "Starting Phase 5.V (Verification) - 80 epics"
./launch_phase5v_all.sh
echo "Polling Phase 5.V completion (60 min)..."
sleep 3600

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
- ✅ Latest agent completes within polling window
- ✅ No agents still running when next phase starts

### Overall
- ✅ Total time ≤ 6 hours (5.2 hours estimated)
- ✅ Zero phase conflicts
- ✅ All 80 epics complete
- ✅ Bobcoins ≤ 6,400

---

**Document Version**: 2.0 (Parallel Model)
**Last Updated**: 2026-06-14T20:26:00Z
**Previous Version**: 1.0 (Sequential model - incorrect)
**Status**: Ready for implementation