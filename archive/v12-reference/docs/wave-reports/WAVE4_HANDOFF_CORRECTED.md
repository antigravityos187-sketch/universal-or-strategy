# Wave 4 Handoff: Autonomous Refactoring (CORRECTED)

**Date**: 2026-06-14
**Version**: 2.0 (Corrected)
**From**: Recovery Session
**To**: Wave 4 Execution Lead
**Context**: Fresh epic roadmap generated, stale session killed
**Workflow Version**: V12.25 (10-Phase Manifest-Based)

---

## ⚠️ CRITICAL CORRECTIONS FROM V1.0

### Issue #1: Stale Epic Roadmap (RESOLVED)
**Problem**: V1.0 handoff stated "80 epics (001-080)" but referenced stale `epic_roadmap.json` with 173 epics (CCN-100 through CCN-272) from previous waves.

**Root Cause**: Roadmap was based on old codebase state, not reflecting Wave 2/3 refactoring work.

**Solution**: Generated fresh roadmap from current complexity audit (2026-06-14):
- **File**: `epic_roadmap_wave4_fresh.json`
- **Epics**: 80 total (EPIC-CCN-001 through EPIC-CCN-080)
- **Tier 1**: 35 epics (CYC 15-30) - High complexity
- **Tier 2**: 45 epics (CYC 9-14) - Medium complexity
- **Source**: Fresh complexity audit with Jane Street strict threshold (CYC ≤8)

### Issue #2: Missing Jane Street Validation (CRITICAL)
**Problem**: Previous waves (~15 epics) executed WITHOUT Firebase hooks.

**Impact**: All previous epics need re-validation against Jane Street KB.

**Solution**: Wave 4 MUST use Jane Street Firebase hooks in ALL phases:
- Phase 1 (Scope + Boundary): Manual KB query
- Phase 2 (Architecture): Automated Firebase hook
- Phase 3 (Audit): Manual KB query
- Phase 4.5 (Ticket Review): Automated Firebase hook
- Phase 5 (Execution): Automated Firebase hook
- Phase 5.V (Verification): Manual KB query

### Issue #3: Wrong Epic Count (RESOLVED)
**Problem**: Agent found 58 pending epics instead of expected 80.

**Root Cause**: Agent filtered stale roadmap (165 pending) using unknown criteria.

**Solution**: Fresh roadmap has exactly 80 epics using two-tier system.

---

## Mission Brief

Execute Wave 4 autonomous complexity reduction using the **approved 10-phase workflow** with **mandatory Jane Street validation**. Target: 80 epics selected from fresh complexity audit using two-tier system.

**Primary Goal**: Achieve CodeScene complexity score ≤8 (Jane Street strict standard)

**Why ≤8?**
- CodeScene uses stricter thresholds than Codacy (15)
- Jane Street HFT systems require cognitive simplicity
- Functions >8 are harder to reason about under microsecond latency
- V12 DNA: "Make illegal states unrepresentable" requires simple logic

---

## 10-Phase Workflow Structure (APPROVED)

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

**Total Planning Time**: 100 minutes per epic
**Jane Street Gates**: 3 full (Phase 2, 4.5, 5) + 3 partial (Phase 1, 3, 5.V)

**Complete SOP**: [`docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md`](docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md)

---

## Fresh Epic Roadmap (80 Epics)

**File**: `epic_roadmap_wave4_fresh.json`
**Generated**: 2026-06-14
**Source**: `complexity_audit_fresh_2026-06-14.txt` (180 methods needing refactoring)

### Two-Tier Selection

**Tier 1 (High Complexity)**: 35 epics (CYC 15-30)
- Larger budget allocation
- Complex extraction patterns
- Higher Jane Street validation scrutiny

**Tier 2 (Medium Complexity)**: 45 epics (CYC 9-14)
- Smaller budget allocation
- Simpler extraction patterns
- Standard Jane Street validation

### Top 20 Epics (Priority Order)

| Epic | Tier | CYC | Method | File | LOC |
|------|------|-----|--------|------|-----|
| EPIC-CCN-001 | T1 | 18 | SymmetryGuardReplaceExistingFollowerTarget | V12_002.Symmetry.Replace.cs | 49 |
| EPIC-CCN-002 | T1 | 18 | SymmetryGuardTryResolveFollowersForDispatch | V12_002.Symmetry.Replace.cs | 33 |
| EPIC-CCN-003 | T1 | 16 | IsOrderAllowed | V12_002.UI.Compliance.cs | 43 |
| EPIC-CCN-004 | T1 | 16 | HandleFleetTargetFill | V12_002.UI.Compliance.cs | 58 |
| EPIC-CCN-005 | T1 | 16 | ClassifyAndRouteFleetOrder | V12_002.SIMA.Lifecycle.cs | 42 |
| EPIC-CCN-006 | T1 | 17 | AdoptFleetWorkingOrders | V12_002.SIMA.Lifecycle.cs | 46 |
| EPIC-CCN-007 | T1 | 20 | ShadowPropagateStopMoves | V12_002.SIMA.Shadow.cs | 32 |
| EPIC-CCN-008 | T1 | 19 | UpdateTargetVisibility | V12_002.UI.Panel.Handlers.cs | 36 |
| EPIC-CCN-009 | T1 | 20 | FindChartTraderViaChartTab | V12_002.UI.Panel.Helpers.cs | 54 |
| EPIC-CCN-010 | T1 | 20 | ShowModeSpecificControls | V12_002.UI.Panel.Handlers.cs | 42 |
| EPIC-CCN-011 | T1 | 17 | DestroyPanel | V12_002.UI.Panel.Construction.cs | 149 |
| EPIC-CCN-012 | T1 | 15 | SyncPanelConfigFromSnapshot | V12_002.UI.Panel.StateSync.cs | 37 |
| EPIC-CCN-013 | T1 | 16 | UpdatePanelState | V12_002.UI.Panel.StateSync.cs | 51 |
| EPIC-CCN-014 | T1 | 19 | TryHandleFleetCommand | V12_002.UI.IPC.Commands.Fleet.cs | 42 |
| EPIC-CCN-015 | T1 | 18 | CancelAll_ProcessSingleFleetAccount | V12_002.UI.IPC.Commands.Fleet.cs | 31 |
| EPIC-CCN-016 | T1 | 19 | TryHandleFleet_CancelAll | V12_002.UI.IPC.Commands.Fleet.cs | 41 |
| EPIC-CCN-017 | T1 | 17 | TryApplyConfigTarget_Value | V12_002.UI.IPC.Commands.Config.cs | 45 |
| EPIC-CCN-018 | T1 | 18 | IsSymbolMatch | V12_002.UI.IPC.cs | 19 |
| EPIC-CCN-019 | T1 | 15 | TryHandleFleet_MoveTarget | V12_002.UI.IPC.Commands.Fleet.cs | 33 |
| EPIC-CCN-020 | T1 | 21 | HandleSecondaryOrderFilled | V12_002.Orders.Callbacks.cs | 69 |

**Complete List**: See `epic_roadmap_wave4_fresh.json` for all 80 epics

---

## Execution Strategy

### Staggered Deployment (All 80 Epics in Single Wave)

**Strategy**: Launch all 80 epics with staggered delays (12-54 seconds)
- **Peak Concurrency**: ~50 agents (based on phase duration and launch rate)
- **Launch Time**: 80 epics × 30s avg delay = 40 minutes
- **Execution Time**: ~60 minutes per phase (parallel execution)

**VM Capacity**: n2-standard-8 (8 vCPU, 32 GB RAM) ✅ **VALIDATED**
- **Actual Specs**: 8 vCPU, 31 GB RAM (verified 2026-06-14)
- **58-Agent Test**: Successfully handled 58 concurrent agents (load avg: 0.00, RAM: 306 MB used)
- **80-Agent Capacity**: CONFIRMED - VM can easily handle 80+ concurrent agents
- **Headroom**: 7.5 vCPU + 30 GB RAM available during 58-agent test
- Agents complete at different times (natural load balancing)
- No manual wave splitting required

**Capacity Validation**:
- Previous documentation stated n2-standard-4 (4 vCPU, 16 GB) ❌ **INCORRECT**
- Actual VM is n2-standard-8 (8 vCPU, 32 GB) ✅ **CORRECT**
- 58 agents ran successfully with minimal resource usage
- 80 agents will use ~50% of available capacity (safe margin)

**Master Launch Script Pattern**:
```bash
#!/bin/bash
# launch_phase{X}_all.sh

PHASE={X}
EPICS=($(seq -f "%03g" 1 80))
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

echo "[$(date)] All 80 epics launched for Phase ${PHASE}"
```

**Reference**: `WAVE3_FINAL_EXECUTION_PLAN.md` for complete staggered launch implementation

### Phase Execution Pattern

**Sequential Phases** (must complete in order):
- Phase -1 → 0 → 1 → 2 → 3 → 4 → 4.5

**Parallel Phases** (can run concurrently):
- Phase 5.1, 5.2, ..., 5.N (if tickets independent)
- Phase 5.1.V, 5.2.V, ..., 5.N.V (after respective tickets)

**Final Phase** (after all verifications):
- Phase 6 (epic completion)

### API Rotation Strategy

**15 APIs Available** (160 bobcoins each = 2,400 total)

**Allocation**:
- 80 epics ÷ 15 APIs = 5.33 epics per API
- Round-robin assignment: API-1 gets epics 1,16,31,46,61,76; API-2 gets epics 2,17,32,47,62,77; etc.

**Budget per Epic**:
- Tier 1 (CYC 15-30): 25 bobcoins
- Tier 2 (CYC 9-14): 20 bobcoins
- Total budget: (35 × 25) + (45 × 20) = 875 + 900 = 1,775 bobcoins
- Safety margin: (2,400 - 1,775) / 2,400 = 26%

**Reference**: `WAVE3_FINAL_EXECUTION_PLAN.md` for complete API rotation logic

---

## Pre-Wave 4 Checklist

### Environment Validation

- [ ] VM accessible (`gcloud compute ssh v12-test-golden-v2`)
- [ ] Build passes (`dotnet build`)
- [ ] jCodemunch index current (`jcodemunch list_repos`)
- [ ] Git status clean (no uncommitted changes in `src/`)
- [ ] Branch strategy: GitButler virtual branches active
- [ ] Firebase credentials configured (`firebase-key.json`)

### Workflow Validation

- [ ] 10-phase SOP reviewed ([`docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md`](docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md))
- [ ] Phase 4.5 script exists (`scripts/phase_4_5_ticket_review_mcp.py`)
- [ ] Jane Street KB accessible (`python scripts/query_kb.py "test"`)
- [ ] Manifest helper tested (`python scripts/epic_manifest.py list`)

### Roadmap Validation

- [x] Fresh complexity audit run (`complexity_audit_fresh_2026-06-14.txt`)
- [x] 80 epics identified (EPIC-CCN-001 through EPIC-CCN-080)
- [x] Epic roadmap generated (`epic_roadmap_wave4_fresh.json`)
- [ ] Two-tier system validated (35 Tier 1 + 45 Tier 2)

---

## Wave 4 Launch Commands

### Option A: Sequential Launch (Safe - Recommended for First 2 Epics)

```bash
# Test with first 2 epics
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

### Option B: Parallel Launch (Fast - After Test Success)

```bash
# Launch 10 epics in parallel (max VM capacity)
# Use Bob CLI orchestrator or Watsonx Orchestrate
# Each epic runs independently through all phases
```

**Recommendation**: Start with Option A for first 2 epics to validate 10-phase workflow + Jane Street hooks, then switch to Option B for remaining 78 epics.

---

## Jane Street Firebase Integration (MANDATORY)

### Firebase Setup

**File**: `firebase-key.json` (gitignored)
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
- ✅ **CPU usage metrics collected** (see Monitoring Protocol)

### Wave 4 Completion

- ✅ 80 epics complete
- ✅ Complexity reduction documented
- ✅ No P0 blockers
- ✅ Roadmap updated
- ✅ Lessons learned documented
- ✅ Jane Street validation passed for all epics
- ✅ **CPU metrics tracked for all phases** (see CPU Monitoring Protocol)

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

## Post-Wave 4 Actions

### Immediate (After Wave 4)

1. **Sync to Local**: Pull `src/` and `docs/brain/` from VM
2. **Run Validation**: `powershell -File .\scripts\pre_push_validation.ps1`
3. **Update Roadmap**: Mark completed epics in `epic_roadmap_wave4_fresh.json`
4. **Document Lessons**: Update [`building-blocks/autonomous-refactoring/WAVE4_LESSONS_LEARNED.md`](building-blocks/autonomous-refactoring/WAVE4_LESSONS_LEARNED.md)

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

**Reference**: [`docs/standards/COMPLEXITY_RATIONALE.md`](docs/standards/COMPLEXITY_RATIONALE.md)

### Building-Blocks Method (MANDATORY)

**ALWAYS copy SAME phase from PREVIOUS wave**

- NEVER generate from scratch
- NEVER copy adjacent phases
- Use find-and-replace for phase-specific changes only
- Verify against working phase with `diff`

**Reference**: [`docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`](docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md)

---

## Key Documents

### Workflow Documentation

- **10-Phase SOP**: [`docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md`](docs/workflow/V12_EPIC_WORKFLOW_10_PHASE_SOP.md)
- **Complete Walkthrough**: [`docs/workflow/EPIC_WORKFLOW_WALKTHROUGH.md`](docs/workflow/EPIC_WORKFLOW_WALKTHROUGH.md)
- **Script Generation SOP**: [`docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`](docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md)

### Wave 3 Reports (Previous Wave)

- **Complete Report**: [`building-blocks/autonomous-refactoring/WAVE2_COMPLETE_REPORT.md`](building-blocks/autonomous-refactoring/WAVE2_COMPLETE_REPORT.md)
- **Lessons Learned**: [`building-blocks/autonomous-refactoring/WAVE2_LESSONS_LEARNED.md`](building-blocks/autonomous-refactoring/WAVE2_LESSONS_LEARNED.md)

### Architecture Documentation

- **Phase Consolidation**: [`PHASE_CONSOLIDATION_REVISED.md`](PHASE_CONSOLIDATION_REVISED.md)
- **API Rotation**: [`WAVE3_API_ROTATION_STRATEGY.md`](WAVE3_API_ROTATION_STRATEGY.md)
- **Execution Plan**: [`WAVE3_FINAL_EXECUTION_PLAN.md`](WAVE3_FINAL_EXECUTION_PLAN.md)

### Standards

- **V12 DNA**: [`docs/AGENTS.md`](docs/AGENTS.md)
- **Jane Street Rules**: [`docs/intel/jane-street/RULES_CATALOG.md`](docs/intel/jane-street/RULES_CATALOG.md)
- **Complexity Rationale**: [`docs/standards/COMPLEXITY_RATIONALE.md`](docs/standards/COMPLEXITY_RATIONALE.md)
- **CPU Monitoring Protocol**: [`docs/protocol/CPU_MONITORING_PROTOCOL.md`](docs/protocol/CPU_MONITORING_PROTOCOL.md)

---

## Questions for Wave 4 Lead

1. **Workflow Validation**: Have you reviewed the 10-phase SOP?
2. **Phase 4.5 Implementation**: Is Phase 4.5 script ready (`scripts/phase_4_5_ticket_review_mcp.py`)?
3. **Jane Street Setup**: Is Firebase configured and KB accessible?
4. **Parallel Execution**: Start with sequential (2 epics) or go full parallel (10 epics)?
5. **API Rotation**: Verify 15 APIs available with 160 bobcoins each?

---

## Handoff Complete

**Status**: Ready for Wave 4 launch with corrected roadmap
**Next Action**: Review 10-phase SOP, validate environment, test Jane Street KB, launch first 2 epics sequentially
**Estimated Time**:
- **Per Phase**: ~140 minutes (10 min script gen + 5 min upload + 40 min launch + 60 min execution + 10 min verify + 15 min budget analysis)
- **Total Wave**: 10 phases × 140 min = 1,400 minutes (~23 hours)
- **With Monitoring**: ~30 hours (3-4 days at 8 hours/day)
**Execution Model**: Single wave, all 80 epics, staggered deployment (12-54s delays), peak ~50 concurrent agents

**Good luck with Wave 4!** 🚀

---

**Document Version**: 2.0 (Corrected)
**Last Updated**: 2026-06-14T22:40:00Z
**Previous Version**: 1.0 (Stale roadmap)
**Maintainer**: V12 Orchestration Team
**Critical Fix**: Fresh epic roadmap + Jane Street validation mandate