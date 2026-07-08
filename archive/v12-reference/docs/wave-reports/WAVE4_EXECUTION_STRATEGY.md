# Wave 4 Execution Strategy

**Version**: 1.0
**Date**: 2026-06-14T22:53:00Z
**Status**: READY FOR EXECUTION
**Wave**: 4 (80 epics, EPIC-CCN-001 through EPIC-CCN-080)

---

## Executive Summary

Wave 4 executes 80 complexity reduction epics using the **approved 10-phase workflow** with **mandatory Jane Street validation**. This wave uses a **single-wave staggered deployment** strategy with 4-minute cost-optimized polling.

**Key Metrics**:
- **Epics**: 80 (35 Tier 1, 45 Tier 2)
- **Target**: CYC ≤ 8 (Jane Street strict standard)
- **Budget**: 1,775 bobcoins (26% safety margin)
- **Timeline**: ~30 hours (3-4 days at 8 hours/day)
- **VM**: n2-standard-8 (8 vCPU, 32 GB RAM) - VALIDATED for 80+ agents

---

## Critical Corrections from V1.0

### ✅ Fresh Epic Roadmap
- **Old**: Stale roadmap with 173 epics (CCN-100 through CCN-272)
- **New**: Fresh roadmap with 80 epics (CCN-001 through CCN-080)
- **Source**: `complexity_audit_fresh_2026-06-14.txt`

### ✅ Jane Street Validation Mandate
- **Issue**: Previous waves (~15 epics) executed WITHOUT Firebase hooks
- **Fix**: Wave 4 MUST use Jane Street KB in ALL phases
- **Enforcement**: Phase 4.5 (Ticket Review) is MANDATORY gate

### ✅ VM Capacity Validation
- **Old**: Documentation stated n2-standard-4 (4 vCPU, 16 GB)
- **New**: Actual VM is n2-standard-8 (8 vCPU, 32 GB)
- **Proof**: 58-agent test ran successfully with minimal resource usage

---

## 10-Phase Workflow (APPROVED)

| Phase | Name | Time | Jane Street | Firebase Hook | Mode | Command |
|-------|------|------|-------------|---------------|------|---------|
| **-1** | Pre-flight | 5 min | ❌ | ❌ | `ask` | `epic-preflight` |
| **0** | Hotspot | 10 min | ❌ | ❌ | `ask` | `epic-intake` |
| **1** | Scope + Boundary | 20 min | ⚠️ | ⚠️ | `plan` | `epic-scope-boundary` |
| **2** | Architecture | 25 min | ✅ | ✅ | `plan` | `epic-plan` |
| **3** | Audit | 10 min | ⚠️ | ⚠️ | `advanced` | `epic-scan` |
| **4** | Tickets | 10 min | ⚠️ | ⚠️ | `plan` | `epic-tickets` |
| **4.5** | Ticket Review | 10 min | ✅ | ✅ | `plan` | `epic-ticket-review` |
| **5** | Execution | 10 min/ticket | ✅ | ✅ | `v12-engineer` | `epic-validate` |
| **5.V** | Verification | 5 min/ticket | ⚠️ | ⚠️ | `advanced` | `epic-verify-ticket` |
| **6** | Final Review | 10 min | ❌ | ❌ | `advanced` | `epic-review-final` |

**Legend**:
- ✅ = Full Jane Street KB integration (automated Firebase hook)
- ⚠️ = Partial Jane Street KB integration (manual query recommended)
- ❌ = No Jane Street KB integration required

**Total Planning Time**: 100 minutes per epic
**Jane Street Gates**: 3 full (Phase 2, 4.5, 5) + 3 partial (Phase 1, 3, 5.V)

---

## Epic Roadmap (80 Epics)

### Two-Tier Selection

**Tier 1 (High Complexity)**: 35 epics (CYC 15-30)
- Budget: 25 bobcoins per epic
- Complex extraction patterns
- Higher Jane Street validation scrutiny

**Tier 2 (Medium Complexity)**: 45 epics (CYC 9-14)
- Budget: 20 bobcoins per epic
- Simpler extraction patterns
- Standard Jane Street validation

### Top 10 Priority Epics

| Epic | Tier | CYC | Method | File |
|------|------|-----|--------|------|
| EPIC-CCN-001 | T1 | 18 | SymmetryGuardReplaceExistingFollowerTarget | V12_002.Symmetry.Replace.cs |
| EPIC-CCN-002 | T1 | 18 | SymmetryGuardTryResolveFollowersForDispatch | V12_002.Symmetry.Replace.cs |
| EPIC-CCN-003 | T1 | 16 | IsOrderAllowed | V12_002.UI.Compliance.cs |
| EPIC-CCN-004 | T1 | 16 | HandleFleetTargetFill | V12_002.UI.Compliance.cs |
| EPIC-CCN-005 | T1 | 16 | ClassifyAndRouteFleetOrder | V12_002.SIMA.Lifecycle.cs |
| EPIC-CCN-006 | T1 | 17 | AdoptFleetWorkingOrders | V12_002.SIMA.Lifecycle.cs |
| EPIC-CCN-007 | T1 | 20 | ShadowPropagateStopMoves | V12_002.SIMA.Shadow.cs |
| EPIC-CCN-008 | T1 | 19 | UpdateTargetVisibility | V12_002.UI.Panel.Handlers.cs |
| EPIC-CCN-009 | T1 | 20 | FindChartTraderViaChartTab | V12_002.UI.Panel.Helpers.cs |
| EPIC-CCN-010 | T1 | 20 | ShowModeSpecificControls | V12_002.UI.Panel.Handlers.cs |

**Complete List**: See `epic_roadmap_wave4_fresh.json`

---

## Execution Strategy

### Single-Wave Staggered Deployment

**Strategy**: Launch all 80 epics with staggered delays (12-54 seconds)

**Capacity**:
- **VM**: n2-standard-8 (8 vCPU, 32 GB RAM)
- **Peak Concurrency**: ~50 agents (validated)
- **Launch Time**: 80 epics × 30s avg delay = 40 minutes
- **Execution Time**: ~60 minutes per phase (parallel)

**Validation**:
- ✅ 58-agent test successful (load avg: 0.00, RAM: 306 MB used)
- ✅ 80-agent capacity confirmed (50% utilization)
- ✅ Natural load balancing (agents complete at different times)

### Cost-Optimized Polling (MANDATORY)

**Polling Interval**: 4 minutes (maximizes cache hits)

**Cache Behavior**:
- Provider cache TTL: 5 minutes
- Polling at 4 minutes: ~100% cache hit rate
- Cost reduction: 88% average across all phases

**Implementation**:
```bash
# Initial check (1 minute)
sleep 60 && ./check_status.sh

# Continuous polling (4 minutes)
while [ $(check_complete) -lt 80 ]; do
    sleep 240  # 4 minutes
    ./check_status.sh
done
```

**Reference**: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md`

### API Rotation Strategy

**15 APIs Available** (160 bobcoins each = 2,400 total)

**Allocation**:
- Round-robin assignment: API-1 gets epics 1,16,31,46,61,76
- API-2 gets epics 2,17,32,47,62,77
- etc.

**Budget**:
- Tier 1: 35 × 25 = 875 bobcoins
- Tier 2: 45 × 20 = 900 bobcoins
- **Total**: 1,775 bobcoins
- **Safety Margin**: (2,400 - 1,775) / 2,400 = 26%

---

## Pre-Wave Checklist

### Environment Validation

- [ ] VM accessible: `gcloud compute ssh v12-test-golden-v2 --zone=us-central1-a`
- [ ] Build passes: `dotnet build`
- [ ] jCodemunch index current: `jcodemunch list_repos`
- [ ] Git status clean: `git status` (no uncommitted `src/` changes)
- [ ] Branch strategy: GitButler virtual branches active
- [ ] Firebase credentials: `firebase-credentials.json` exists

### Workflow Validation

- [ ] 10-phase SOP reviewed: `docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md`
- [ ] Phase 4.5 script exists: `scripts/phase_4_5_ticket_review_mcp.py`
- [ ] Jane Street KB accessible: `python scripts/query_kb.py "test"`
- [ ] Manifest helper tested: `python scripts/epic_manifest.py list`

### Roadmap Validation

- [x] Fresh complexity audit run: `complexity_audit_fresh_2026-06-14.txt`
- [x] 80 epics identified: EPIC-CCN-001 through EPIC-CCN-080
- [x] Epic roadmap generated: `epic_roadmap_wave4_fresh.json`
- [x] Two-tier system validated: 35 Tier 1 + 45 Tier 2

---

## Execution Plan

### Phase 1: Test Run (First 2 Epics)

**Purpose**: Validate 10-phase workflow + Jane Street hooks

**Epics**: EPIC-CCN-001, EPIC-CCN-002

**Commands**:
```bash
# Sequential execution for validation
for epic in CCN-001 CCN-002; do
    echo "Starting EPIC-$epic"
    epic-intake EPIC-$epic "Complexity reduction"
    epic-scope-boundary EPIC-$epic
    epic-plan EPIC-$epic
    epic-scan EPIC-$epic
    epic-tickets EPIC-$epic
    epic-ticket-review EPIC-$epic  # NEW Phase 4.5
    epic-validate EPIC-$epic --ticket 1
    epic-verify-ticket EPIC-$epic --ticket 1
    epic-review-final EPIC-$epic
done
```

**Success Criteria**:
- ✅ All 10 phases complete for both epics
- ✅ Phase 4.5 (Ticket Review) passed
- ✅ Jane Street KB queried successfully
- ✅ Build passes after changes
- ✅ No P0 blockers

### Phase 2: Full Wave Launch (Remaining 78 Epics)

**After Test Success**: Launch remaining 78 epics in parallel

**Master Launch Script Pattern**:
```bash
#!/bin/bash
# launch_wave4_master.sh

PHASE=0
EPICS=($(seq -f "%03g" 3 80))  # Start from 003 (001-002 already done)
BASE_DELAY=12
MAX_DELAY=54

for i in "${!EPICS[@]}"; do
    EPIC="${EPICS[$i]}"
    
    # Calculate staggered delay (12-54 seconds)
    DELAY=$((BASE_DELAY + (i % (MAX_DELAY - BASE_DELAY + 1))))
    
    echo "[$(date)] Launching EPIC-CCN-${EPIC} (delay: ${DELAY}s)"
    
    # Launch in screen session
    screen -dmS p${PHASE}-${EPIC} bash -l -c \
        "./_p${PHASE}_${EPIC}.sh 2>&1 | tee logs/phase${PHASE}/EPIC-CCN-${EPIC}.log"
    
    # Wait before next launch
    sleep ${DELAY}
done

echo "[$(date)] All 78 remaining epics launched for Phase ${PHASE}"

# Start 4-minute polling
sleep 60 && ./check_status.sh
while [ $(check_complete) -lt 80 ]; do
    sleep 240
    ./check_status.sh
done
```

---

## Jane Street Firebase Integration

### Firebase Setup

**File**: `firebase-credentials.json` (gitignored)
**Location**: Repository root
**Format**: Google Cloud service account JSON

**Validation**:
```bash
python scripts/query_kb.py "complexity reduction"
```

**Expected Output**: JSON response with Jane Street rules

### Phase-Specific KB Queries

**Phase 1 (Scope + Boundary)**: Manual query
```bash
python scripts/query_kb.py "single-method extraction"
```

**Phase 2 (Architecture)**: Automated hook in script
```python
# scripts/phase_2_architecture_mcp.py
kb_results = query_jane_street_kb("FSM extraction patterns")
```

**Phase 3 (Audit)**: Manual query
```bash
python scripts/query_kb.py "V12 DNA compliance"
```

**Phase 4.5 (Ticket Review)**: Automated hook in script
```python
# scripts/phase_4_5_ticket_review_mcp.py
kb_results = query_jane_street_kb("ticket validation")
```

**Phase 5 (Execution)**: Automated hook in Bob CLI
```bash
# Bob CLI automatically queries KB during execution
```

**Phase 5.V (Verification)**: Manual query
```bash
python scripts/query_kb.py "testing patterns"
```

### KB Coverage

**100+ rules** with P0/P1/P2 severity:
- HFT patterns (microsecond latency)
- FSM/Actor patterns (lock-free)
- Testing strategies (TDD)
- Complexity reduction (cognitive simplicity)

**Reference**: `docs/intel/jane-street/RULES_CATALOG.md`

---

## Success Criteria

### Per Epic

- ✅ All 10 phases complete
- ✅ Phase 4.5 (Ticket Review) passed
- ✅ Jane Street KB queried in all required phases
- ✅ Complexity target met (CYC ≤8)
- ✅ Build passes
- ✅ Tests pass
- ✅ Manifest status = "completed"

### Wave 4 Completion

- ✅ 80 epics complete
- ✅ Complexity reduction documented
- ✅ No P0 blockers
- ✅ Roadmap updated
- ✅ Lessons learned documented
- ✅ Jane Street validation passed for all epics

---

## Failure Recovery

### Blocked Epic Protocol

If an epic blocks:

1. **Document Failure**: Create `docs/brain/EPIC-X/failure-analysis.md`
2. **Identify Root Cause**: Build failure? Test failure? Scope creep? Jane Street validation failure?
3. **Isolate Issue**: Is it epic-specific or systemic?
4. **Fix or Skip**: Fix if quick (<30 min), otherwise skip and continue
5. **Update Roadmap**: Mark epic as "blocked" with reason

### Resume Protocol

```bash
# Check epic status
python scripts/epic_manifest.py status EPIC-CCN-X

# Resume from failed phase
epic-validate EPIC-CCN-X --ticket 2  # Example: resume Phase 5.2
```

---

## Post-Wave Actions

### Immediate (After Wave 4)

1. **Sync to Local**: Pull `src/` and `docs/brain/` from VM
2. **Run Validation**: `powershell -File .\scripts\pre_push_validation.ps1`
3. **Update Roadmap**: Mark completed epics in `epic_roadmap_wave4_fresh.json`
4. **Document Lessons**: Update `building-blocks/autonomous-refactoring/WAVE4_LESSONS_LEARNED.md`

### Deferred (Before Wave 5)

1. **Validate Jane Street Integration**: Verify all epics queried KB correctly
2. **Analyze Bobcoin Usage**: Compare actual vs budgeted per tier
3. **Update Phase 4.5**: Refine ticket review criteria based on Wave 4 results
4. **Optimize API Rotation**: Adjust allocation based on actual usage patterns

---

## Critical Reminders

### V12.23 No Scope Creep Protocol

**ONE EPIC = ONE CONCERN**

- ❌ Fixing pre-existing compilation errors during epic
- ❌ Adding "while we're here" improvements
- ❌ Bundling multiple concerns in one commit
- ❌ Expanding scope mid-epic without Director approval

**Enforcement**: Phase 1 boundary validation (merged into Phase 1)

### PR Strategy (Batch Mode)

**DO NOT CREATE PRS UNTIL ALL EPICS COMPLETE**

- Commit after each epic
- Batch commits into related clusters
- Create PRs after all 80 epics done
- Merge to `main` after PR approval

**Rationale**: Avoid mid-workflow merge conflicts

### Jane Street Alignment

**CYC ≤8 Threshold** (not 15)

- Jane Street HFT systems prioritize cognitive simplicity
- Functions >8 are harder to reason about under microsecond latency
- V12 DNA: "Make illegal states unrepresentable" requires simple logic

**Reference**: `docs/standards/COMPLEXITY_RATIONALE.md`

### Building-Blocks Method (MANDATORY)

**ALWAYS copy SAME phase from PREVIOUS wave**

- NEVER generate from scratch
- NEVER copy adjacent phases
- Use find-and-replace for phase-specific changes only
- Verify against working phase with `diff`

**Reference**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`

---

## Timeline Estimate

### Per Phase (80 epics)

| Phase | Script Gen | Upload | Launch | Execution | Verify | Budget Analysis | Total |
|-------|-----------|--------|--------|-----------|--------|----------------|-------|
| -1 | 10 min | 5 min | 40 min | 10 min | 10 min | 15 min | 90 min |
| 0 | 10 min | 5 min | 40 min | 20 min | 10 min | 15 min | 100 min |
| 1 | 10 min | 5 min | 40 min | 30 min | 10 min | 15 min | 110 min |
| 2 | 10 min | 5 min | 40 min | 50 min | 10 min | 15 min | 130 min |
| 3 | 10 min | 5 min | 40 min | 20 min | 10 min | 15 min | 100 min |
| 4 | 10 min | 5 min | 40 min | 20 min | 10 min | 15 min | 100 min |
| 4.5 | 10 min | 5 min | 40 min | 20 min | 10 min | 15 min | 100 min |
| 5 | 10 min | 5 min | 40 min | 90 min | 10 min | 15 min | 170 min |
| 5.V | 10 min | 5 min | 40 min | 60 min | 10 min | 15 min | 140 min |
| 6 | 10 min | 5 min | 40 min | 20 min | 10 min | 15 min | 100 min |

**Total**: 1,140 minutes (~19 hours)

**With Monitoring**: ~30 hours (3-4 days at 8 hours/day)

**Execution Model**: Single wave, all 80 epics, staggered deployment (12-54s delays), peak ~50 concurrent agents

---

## Key Documents

### Workflow Documentation

- **10-Phase SOP**: `docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md`
- **Complete Walkthrough**: `docs/workflow/EPIC_WORKFLOW_WALKTHROUGH.md`
- **Script Generation SOP**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- **Cost-Optimized Polling**: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md`

### Wave 3 Reports (Previous Wave)

- **Complete Report**: `building-blocks/autonomous-refactoring/WAVE2_COMPLETE_REPORT.md`
- **Lessons Learned**: `building-blocks/autonomous-refactoring/WAVE2_LESSONS_LEARNED.md`

### Architecture Documentation

- **Phase Consolidation**: `PHASE_CONSOLIDATION_REVISED.md`
- **API Rotation**: `WAVE3_API_ROTATION_STRATEGY.md`
- **Execution Plan**: `WAVE3_FINAL_EXECUTION_PLAN.md`

### Standards

- **V12 DNA**: `docs/AGENTS.md`
- **Jane Street Rules**: `docs/intel/jane-street/RULES_CATALOG.md`
- **Complexity Rationale**: `docs/standards/COMPLEXITY_RATIONALE.md`

---

## Next Steps

1. **Review this strategy document** ✅
2. **Validate environment** (VM, build, jCodemunch, Firebase)
3. **Test Jane Street KB** (`python scripts/query_kb.py "test"`)
4. **Launch test run** (EPIC-CCN-001, EPIC-CCN-002)
5. **Validate test results** (all 10 phases, Phase 4.5 passed)
6. **Launch full wave** (remaining 78 epics)
7. **Monitor with 4-minute polling**
8. **Document lessons learned**

---

**Document Version**: 1.0
**Last Updated**: 2026-06-14T22:53:00Z
**Status**: READY FOR EXECUTION
**Maintainer**: V12 Orchestration Team