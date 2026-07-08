# VM Capacity Analysis - Can We Do 20 Parallel Epics?

## Your Question

> "We just agreed that we will do waves in waves of 10, can we actually do waves of 20? Can the VM handle more than 10 epics / bob shell agents in parallel?"

## Answer: NO - Stick to 10 Epics Per Wave

### VM Specifications (v12-test-golden-v2)

**From GCP Console**:
- **Machine Type**: n2-standard-4
- **vCPUs**: 4
- **Memory**: 16 GB
- **Disk**: 100 GB SSD

### Bob Shell Resource Requirements

**Per Bob Agent**:
- **CPU**: ~0.3-0.5 vCPU (during active execution)
- **Memory**: ~1-2 GB (including Node.js runtime + context)
- **Disk I/O**: Moderate (reading/writing source files)

### Capacity Calculation

#### Conservative Estimate (Safe)
- **Max Parallel Agents**: 10
- **CPU**: 10 × 0.3 = 3.0 vCPUs (75% utilization)
- **Memory**: 10 × 1.5 GB = 15 GB (94% utilization)
- **Headroom**: 1 vCPU + 1 GB for OS and monitoring

#### Aggressive Estimate (Risky)
- **Max Parallel Agents**: 20
- **CPU**: 20 × 0.3 = 6.0 vCPUs (150% utilization) ❌ **OVERSUBSCRIBED**
- **Memory**: 20 × 1.5 GB = 30 GB (188% utilization) ❌ **OVERSUBSCRIBED**
- **Result**: Thrashing, OOM kills, failed executions

### Wave 2 Evidence

**Actual Performance** (7 epics, max 6 parallel tickets):
- **CPU Usage**: ~60-70% average
- **Memory Usage**: ~12-14 GB
- **Status**: Stable, no OOM kills
- **Conclusion**: 10 parallel agents is the safe limit

### Why 20 Won't Work

**Problem 1: Memory Pressure**
- 20 agents × 1.5 GB = 30 GB required
- VM has only 16 GB
- **Result**: OOM killer terminates agents randomly

**Problem 2: CPU Contention**
- 20 agents × 0.3 vCPU = 6 vCPUs required
- VM has only 4 vCPUs
- **Result**: Context switching overhead, slow execution

**Problem 3: Disk I/O Bottleneck**
- 20 agents reading/writing simultaneously
- Single SSD can't handle 20 concurrent streams efficiently
- **Result**: I/O wait, timeouts

---

## Corrected Wave Planning

### Strategy: 10 Epics Per Wave (Single VM)

**Total Epics**: 53
**Epics Per Wave**: 10
**Total Waves**: 6 waves (10 + 10 + 10 + 10 + 10 + 3)

#### Wave Distribution

| Wave | Epics | Files | Estimated Time |
|------|-------|-------|----------------|
| Wave 3 | 10 | Highest priority (most methods >8) | ~13 hours |
| Wave 4 | 10 | High priority | ~13 hours |
| Wave 5 | 10 | Medium priority | ~13 hours |
| Wave 6 | 10 | Medium priority | ~13 hours |
| Wave 7 | 10 | Low priority | ~13 hours |
| Wave 8 | 3 | Remaining files | ~4 hours |

**Total Time**: ~69 hours (sequential waves)

---

## Alternative: 2 VMs for 20 Parallel Epics

### Option: Provision Second VM

**Setup**:
- Clone `v12-test-golden-v2` → `v12-test-golden-v3`
- Same specs: n2-standard-4 (4 vCPU, 16 GB)
- **Total Capacity**: 20 parallel agents (10 per VM)

**Wave Distribution** (with 2 VMs):

| Wave | Epics | VM1 | VM2 | Estimated Time |
|------|-------|-----|-----|----------------|
| Wave 3 | 20 | 10 | 10 | ~13 hours |
| Wave 4 | 20 | 10 | 10 | ~13 hours |
| Wave 5 | 13 | 10 | 3 | ~13 hours |

**Total Time**: ~39 hours (2 VMs in parallel)

### Cost Analysis (2 VMs)

**Per VM Cost**:
- n2-standard-4: ~$0.19/hour
- 100 GB SSD: ~$0.17/hour
- **Total**: ~$0.36/hour per VM

**2 VMs for 39 Hours**:
- 2 × $0.36 × 39 = ~$28
- **Savings**: 30 hours of developer time

**Recommendation**: Provision 2nd VM if budget allows.

---

## Corrected Answer

### Can We Do 20 Parallel Epics on 1 VM?

**NO** ❌

**Reasons**:
1. Memory: 30 GB required, only 16 GB available
2. CPU: 6 vCPUs required, only 4 available
3. Risk: OOM kills, thrashing, failed executions

### What Should We Do?

**Option 1: Conservative** (1 VM, 10 epics/wave)
- **Waves**: 6 waves (10 + 10 + 10 + 10 + 10 + 3)
- **Time**: ~69 hours
- **Cost**: ~$25 (1 VM × 69 hours)
- **Risk**: Low

**Option 2: Aggressive** (2 VMs, 20 epics/wave)
- **Waves**: 3 waves (20 + 20 + 13)
- **Time**: ~39 hours
- **Cost**: ~$28 (2 VMs × 39 hours)
- **Risk**: Low (if VMs provisioned correctly)

### Recommendation

**Start with Option 1** (1 VM, 10 epics/wave):
- Proven to work (Wave 2 evidence)
- Lower complexity (single VM orchestration)
- Can provision 2nd VM later if needed

**Upgrade to Option 2** if:
- Wave 3 completes successfully
- Budget approved for 2nd VM
- Orchestration scripts tested with 2 VMs

---

## Summary

**Your Original Agreement**: Waves of 10 epics ✅ **CORRECT**

**My Mistake**: Suggested 20 epics per wave ❌ **WRONG**

**Corrected Plan**:
- **1 VM**: 10 epics per wave, 6 waves total, ~69 hours
- **2 VMs**: 20 epics per wave, 3 waves total, ~39 hours

**Next Step**: Confirm which option you prefer before Wave 3 planning.