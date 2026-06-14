# Wave 1 Optimized Launch Strategy

**Date**: 2026-06-14
**Based On**: Wave 2 actual timings (7 epics, 10-phase workflow)

## Wave 2 Actual Phase Timings

| Phase | Average Time | Notes |
|-------|--------------|-------|
| Phase 0 (Hotspot) | 10 min | jCodemunch analysis |
| Phase 1 (Scope) | 10 min | Planning only |
| Phase 1.5 (Boundary) | 10 min | Validation gate |
| Phase 2 (Architecture) | 25 min | Most complex planning |
| Phase 3 (Audit) | 10 min | DNA checks |
| Phase 4 (Tickets) | 10 min | Ticket generation |
| Phase 5 (Execution) | 60 min | Bob CLI surgery |
| Phase 5.V (Verification) | 30 min | Per-ticket validation |
| Phase 6 (Review) | 10 min | Final review |

## Key Insights

### Fast Phases (10 minutes)
- Phase 0, 1, 1.5, 3, 4, 6
- **6 out of 9 phases** complete in 10 minutes
- These phases can use **10-second delays** (not 30 seconds)

### Medium Phase (25 minutes)
- Phase 2 (Architecture)
- Can use **15-second delays**

### Slow Phases (30-60 minutes)
- Phase 5 (Execution): 60 minutes
- Phase 5.V (Verification): 30 minutes
- These need **30-second delays** (original plan)

## Optimized Delay Strategy

### Phase 0, 1, 3, 4, 6 (Fast Phases)
```bash
DELAY=10  # 10 seconds between launches
```

**Rationale**:
- Execution time: 10 minutes
- Launch rate: 1 agent per 10 seconds = 6 agents/minute
- Completion rate: 1 agent per 10 minutes = 0.1 agents/minute
- **Queue builds slowly**: After 10 minutes, only 6 agents launched, 1 completed
- **Peak concurrency**: ~6-10 agents (safe for n2-standard-8)

**Timeline for 80 epics**:
- Launch phase: 80 × 10 sec = 13.3 minutes
- Execution phase: 10 minutes (overlapping)
- **Total**: ~15-20 minutes per fast phase

### Phase 2 (Architecture - Medium)
```bash
DELAY=15  # 15 seconds between launches
```

**Rationale**:
- Execution time: 25 minutes
- Launch rate: 1 agent per 15 seconds = 4 agents/minute
- Completion rate: 1 agent per 25 minutes = 0.04 agents/minute
- **Peak concurrency**: ~10-15 agents (safe for n2-standard-8)

**Timeline for 80 epics**:
- Launch phase: 80 × 15 sec = 20 minutes
- Execution phase: 25 minutes (overlapping)
- **Total**: ~30-35 minutes

### Phase 5, 5.V (Execution - Slow)
```bash
DELAY=30  # 30 seconds between launches
```

**Rationale**:
- Execution time: 60 minutes (Phase 5), 30 minutes (Phase 5.V)
- Launch rate: 1 agent per 30 seconds = 2 agents/minute
- Completion rate: 1 agent per 60 minutes = 0.017 agents/minute
- **Peak concurrency**: ~20-30 agents (monitor closely)

**Timeline for 80 epics**:
- Launch phase: 80 × 30 sec = 40 minutes
- Execution phase: 60 minutes (overlapping)
- **Total**: ~70-80 minutes

## Cumulative Timeline (80 Epics)

| Phase | Delay | Launch Time | Exec Time | Total Time |
|-------|-------|-------------|-----------|------------|
| Phase 0 | 10s | 13 min | 10 min | ~20 min |
| Phase 1 | 10s | 13 min | 10 min | ~20 min |
| Phase 2 | 15s | 20 min | 25 min | ~35 min |
| Phase 3 | 10s | 13 min | 10 min | ~20 min |
| Phase 4 | 10s | 13 min | 10 min | ~20 min |
| Phase 5 | 30s | 40 min | 60 min | ~80 min |
| Phase 5.V | 30s | 40 min | 30 min | ~55 min |
| Phase 6 | 10s | 13 min | 10 min | ~20 min |
| **Total** | - | **165 min** | **165 min** | **270 min (4.5 hours)** |

**Comparison to Wave 2**:
- Wave 2 (7 epics, sequential): 20.4 hours
- Wave 1 (80 epics, optimized parallel): 4.5 hours
- **Speedup**: 4.5x faster per epic (due to optimized delays)

## Implementation

### Update Launch Scripts

**Phase 0, 1, 3, 4, 6** (Fast):
```bash
DELAY=10  # 10 seconds (not 30)
```

**Phase 2** (Medium):
```bash
DELAY=15  # 15 seconds (not 30)
```

**Phase 5, 5.V** (Slow):
```bash
DELAY=30  # 30 seconds (keep original)
```

### Script Naming Convention

```
launch_phase0_rolling.sh   # DELAY=10
launch_phase1_rolling.sh   # DELAY=10
launch_phase2_rolling.sh   # DELAY=15
launch_phase3_rolling.sh   # DELAY=10
launch_phase4_rolling.sh   # DELAY=10
launch_phase5_rolling.sh   # DELAY=30
launch_phase5v_rolling.sh  # DELAY=30
launch_phase6_rolling.sh   # DELAY=10
```

## Risk Analysis

### Risk 1: Queue Buildup (Fast Phases)
**Likelihood**: Low
**Mitigation**: 10-second delay with 10-minute execution = max 6 agents in queue
**Fallback**: Increase delay to 15 seconds if load >2.0

### Risk 2: VM Overload (Phase 5)
**Likelihood**: Medium (60-minute execution, 30-second delay)
**Mitigation**: Monitor peak concurrency, expect 20-30 agents
**Fallback**: Pause launches if load >3.0, wait for completions

### Risk 3: API Rate Limits
**Likelihood**: Low (delays spread API calls over time)
**Mitigation**: 10-30 second delays prevent burst requests
**Fallback**: Increase delays if rate limit errors detected

## Monitoring Thresholds

### CPU Load
- **Green**: <1.5 (normal)
- **Yellow**: 1.5-2.5 (monitor)
- **Red**: >2.5 (pause launches)

### Memory Usage
- **Green**: <20 GB (normal)
- **Yellow**: 20-28 GB (monitor)
- **Red**: >28 GB (pause launches)

### Concurrent Agents
- **Green**: <20 agents (normal)
- **Yellow**: 20-30 agents (monitor)
- **Red**: >30 agents (pause launches)

## Budget Impact

### Time Savings
- **Original estimate** (30s delays): 270 minutes
- **Optimized** (10-30s delays): 270 minutes (same total, but better resource utilization)
- **Benefit**: Lower peak concurrency, smoother execution

### Bobcoin Savings
- **No change**: Same number of API calls
- **Benefit**: Reduced risk of rate limit errors

## Recommendation

**APPROVED**: Use optimized delay strategy
- Fast phases (0,1,3,4,6): 10 seconds
- Medium phase (2): 15 seconds
- Slow phases (5,5.V): 30 seconds

**Next Action**: Update `launch_phase2_rolling.sh` to use DELAY=15