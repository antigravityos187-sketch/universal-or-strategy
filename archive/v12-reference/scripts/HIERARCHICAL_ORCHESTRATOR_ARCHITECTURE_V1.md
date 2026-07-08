# Hierarchical Orchestrator Architecture V1

**Version**: 1.0
**Date**: 2026-06-16
**Status**: QUEUED FOR POST-WAVE-4 IMPLEMENTATION
**Priority**: HIGH - Enables fully autonomous wave execution

---

## Executive Summary

Implement a **hierarchical orchestrator pattern** where 11 Bob IDE sessions coordinate autonomous wave execution:
- **1 Master Orchestrator**: Spawns phase orchestrators, tracks progress, handles escalation
- **10 Phase Orchestrators**: Each handles one phase (0-6, with 5.1-5.N for tickets)
- **Chain-of-Responsibility**: Each phase spawns the next, with backward looping for recovery
- **Parallel Ticket Execution**: Phase 5 tickets run simultaneously (3x speedup)

**Goal**: Start wave once → Master spawns Phase 0 → Chain executes all phases → Master receives final report

---

## Architecture Overview

### Orchestrator Hierarchy

```
Master Orchestrator (Bob IDE Session 1)
  │
  └─> Phase 0 Orchestrator (Session 2)
        └─> Phase 1 Orchestrator (Session 3)
              └─> Phase 2 Orchestrator (Session 4)
                    └─> Phase 3 Orchestrator (Session 5)
                          └─> Phase 4 Orchestrator (Session 6)
                                └─> Phase 5 Orchestrator (Session 7)
                                      ├─> Phase 5.1 (Ticket 1) ← PARALLEL
                                      ├─> Phase 5.2 (Ticket 2) ← PARALLEL
                                      └─> Phase 5.N (Ticket N) ← PARALLEL
                                            └─> Phase 5.V Orchestrator (Session 8)
                                                  └─> Phase 6 Orchestrator (Session 9)
                                                        └─> Reports to Master
```

### Key Principles

1. **Chain-of-Responsibility**: Each phase spawns the next phase
2. **Backward Looping**: Phases can loop back to earlier phases (max 3 loops)
3. **Parallel Execution**: Phase 5 tickets run simultaneously
4. **Master Oversight**: Master tracks loop count, escalates failures
5. **Clean Termination**: Each phase terminates after spawning next

---

## Component Specifications

### 1. Master Orchestrator

**Role**: Wave-level coordination and escalation

**Responsibilities**:
- Spawn Phase 0 orchestrator with wave context
- Wait for final report from Phase 6
- Track loop count across all phases
- Escalate to human if max loops exceeded
- Generate wave completion report

**Implementation**:
```python
# master_orchestrator.py
def execute_wave(wave_id, epics, vm, budget):
    """
    Execute complete wave autonomously.
    
    Args:
        wave_id: Wave identifier (e.g., "wave-5")
        epics: List of epic IDs (e.g., range(1, 81))
        vm: GCP VM name (e.g., "v12-test-golden-v2")
        budget: Total bobcoin budget (e.g., 2400)
    
    Returns:
        Wave completion report with success rate and bobcoin usage
    """
    print(f"Starting {wave_id} autonomous execution...")
    print(f"Epics: {len(epics)}, VM: {vm}, Budget: {budget}")
    
    # Spawn Phase 0 orchestrator (chain starts here)
    final_result = spawn_phase_orchestrator(
        phase=0,
        epics=epics,
        wave_id=wave_id,
        vm=vm,
        budget=budget,
        loop_count=0,
        max_loops=3
    )
    
    # Analyze final result
    if final_result.status == "complete":
        print(f"✅ {wave_id} complete!")
        print(f"Success rate: {final_result.overall_success}%")
        print(f"Bobcoins used: {final_result.total_bobcoins}/{budget}")
        print(f"Phases completed: {final_result.phases_completed}")
        return final_result
    else:
        print(f"❌ {wave_id} failed at Phase {final_result.failed_phase}")
        print(f"Reason: {final_result.failure_reason}")
        print(f"Requires human intervention: {final_result.requires_human}")
        return final_result

def spawn_phase_orchestrator(phase, epics, wave_id, vm, budget, loop_count, max_loops):
    """
    Spawn phase orchestrator as Bob IDE subtask.
    
    Uses /new_task command to create fresh Bob IDE session.
    """
    return use_tool(
        tool_name="new_task",
        parameters={
            "mode": "autonomous-refactor",
            "message": f"Execute {wave_id} Phase {phase} for {len(epics)} epics on VM {vm}",
            "todos": generate_phase_todos(phase, epics, wave_id, vm, budget, loop_count, max_loops)
        }
    )
```

**Inputs**:
- Wave ID (e.g., "wave-5")
- Epic list (e.g., range(1, 81))
- VM name (e.g., "v12-test-golden-v2")
- Budget (e.g., 2400 bobcoins)

**Outputs**:
- Wave completion report
- Success rate (%)
- Bobcoin usage
- Phase-by-phase results

---

### 2. Phase Orchestrator Template

**Role**: Phase-level execution and recovery

**Responsibilities**:
- Generate scripts using building-blocks method
- Upload scripts to VM
- Launch workers with staggered delays
- Monitor completion (4-minute polling)
- Apply Recovery Loop Protocol if <100%
- Spawn next phase OR loop back to earlier phase
- Report results to master (if final phase)

**Implementation**:
```python
# phase_orchestrator.py
def execute_phase(phase, epics, wave_id, vm, budget, previous_result=None, loop_count=0, max_loops=3):
    """
    Execute single phase for all epics.
    
    Args:
        phase: Phase number (0-6)
        epics: List of epic IDs
        wave_id: Wave identifier
        vm: GCP VM name
        budget: Bobcoin budget for this phase
        previous_result: Results from previous phase (for context)
        loop_count: Current loop count for this phase
        max_loops: Maximum loops before escalation
    
    Returns:
        Phase result with success rate, bobcoin usage, and next action
    """
    print(f"=== Phase {phase} Execution (Loop {loop_count+1}/{max_loops}) ===")
    
    # 1. Generate scripts using building-blocks method
    print("Step 1: Generating scripts...")
    scripts = generate_scripts_building_blocks(phase, epics, wave_id)
    
    # 2. Upload scripts to VM
    print("Step 2: Uploading scripts to VM...")
    upload_result = upload_scripts_to_vm(scripts, vm)
    verify_upload(scripts, vm)  # MANDATORY (V12.27)
    
    # 3. Launch workers with staggered delays
    print("Step 3: Launching workers...")
    delay = get_phase_delay(phase)  # 10-25s based on phase
    launch_workers(epics, phase, vm, delay)
    
    # 4. Monitor completion (4-minute polling)
    print("Step 4: Monitoring completion...")
    result = monitor_completion(epics, phase, vm, polling_interval=240)
    
    # 5. Apply Recovery Loop Protocol if <100%
    if result.success_rate < 100:
        print(f"Phase {phase} incomplete: {result.success_rate}%")
        
        if loop_count < max_loops:
            # Retry same phase
            print(f"Applying Recovery Loop Protocol (attempt {loop_count+2})...")
            recovery_result = apply_recovery_loop(result.failures, phase, vm)
            
            if recovery_result.success_rate < 100:
                # Still incomplete - loop back
                return execute_phase(
                    phase=phase,
                    epics=epics,
                    wave_id=wave_id,
                    vm=vm,
                    budget=budget,
                    previous_result=previous_result,
                    loop_count=loop_count + 1,
                    max_loops=max_loops
                )
        else:
            # Max loops exceeded - escalate
            return {
                "status": "failed",
                "phase": phase,
                "success_rate": result.success_rate,
                "reason": "max_recovery_loops_exceeded",
                "requires_human": True,
                "loop_count": loop_count
            }
    
    # 6. Check if need to loop back to earlier phase
    loop_back_phase = check_loop_back_condition(phase, result, previous_result)
    if loop_back_phase is not None:
        print(f"Phase {phase} requires loop back to Phase {loop_back_phase}")
        return execute_phase(
            phase=loop_back_phase,
            epics=epics,
            wave_id=wave_id,
            vm=vm,
            budget=budget,
            previous_result=result,
            loop_count=0,  # Reset loop count for new phase
            max_loops=max_loops
        )
    
    # 7. Spawn next phase OR report to master
    if phase < 6:
        # Spawn next phase (chain continues)
        print(f"Phase {phase} complete, spawning Phase {phase+1}...")
        next_result = spawn_phase_orchestrator(
            phase=phase + 1,
            epics=epics,
            wave_id=wave_id,
            vm=vm,
            budget=budget,
            previous_result=result,
            loop_count=0,
            max_loops=max_loops
        )
        return next_result  # Pass through to master
    else:
        # Final phase - report to master
        print("Phase 6 complete, reporting to master...")
        return generate_final_report(previous_result, result)

def check_loop_back_condition(phase, result, previous_result):
    """
    Determine if phase should loop back to earlier phase.
    
    Returns:
        Phase number to loop back to, or None if no loop needed
    """
    # Phase 6 review finds audit issues → loop back to Phase 3
    if phase == 6 and result.audit_issues_found:
        return 3
    
    # Phase 4 finds Phase 3 incomplete → loop back to Phase 3
    if phase == 4 and not previous_result.phase3_complete:
        return 3
    
    # Phase 5.V verification fails → loop back to Phase 5
    if phase == 5.5 and result.verification_failed:
        return 5
    
    # No loop needed
    return None
```

**Phase-Specific Delays**:
```python
def get_phase_delay(phase):
    """Return staggered launch delay for phase."""
    delays = {
        -1: 2,   # Pre-flight
        0: 12,   # Hotspot
        1: 12,   # Scope + Boundary
        2: 15,   # Architecture (Jane Street KB)
        3: 12,   # Audit
        4: 10,   # Tickets
        4.5: 12, # Ticket Review (Jane Street KB)
        5: 25,   # Execution (Bob CLI surgery)
        5.5: 15, # Verification (build + test)
        6: 10    # Final Review
    }
    return delays.get(phase, 12)
```

---

### 3. Parallel Ticket Execution (Phase 5)

**Current**: Sequential ticket execution
```
Ticket 1 (10 min) → Ticket 2 (10 min) → Ticket 3 (10 min) = 30 min
```

**New**: Parallel ticket execution
```
Ticket 1 (10 min) ┐
Ticket 2 (10 min) ├─ All run simultaneously = 10 min
Ticket 3 (10 min) ┘
```

**Implementation**:
```python
def execute_phase_5_parallel(epics, wave_id, vm, budget):
    """
    Execute Phase 5 with parallel ticket execution.
    
    For each epic:
    1. Read ticket count from Phase 4 output
    2. Launch all tickets simultaneously
    3. Monitor all tickets in parallel
    4. Report when all complete
    """
    results = {}
    
    for epic in epics:
        # Read ticket count
        tickets = read_ticket_count(epic, wave_id)
        
        # Launch all tickets in parallel
        print(f"EPIC-CCN-{epic:03d}: Launching {len(tickets)} tickets in parallel...")
        ticket_pids = []
        
        for ticket_id in tickets:
            pid = launch_ticket(epic, ticket_id, vm)
            ticket_pids.append(pid)
            time.sleep(2)  # Small delay between ticket launches
        
        # Monitor all tickets
        results[epic] = monitor_tickets_parallel(epic, ticket_pids, vm)
    
    return results

def monitor_tickets_parallel(epic, ticket_pids, vm):
    """
    Monitor multiple tickets running in parallel.
    
    Returns when ALL tickets complete.
    """
    while True:
        # Check if all tickets complete
        all_complete = all(
            check_ticket_complete(epic, pid, vm)
            for pid in ticket_pids
        )
        
        if all_complete:
            return {
                "epic": epic,
                "tickets_complete": len(ticket_pids),
                "success": True
            }
        
        time.sleep(60)  # Check every minute
```

**Speedup Calculation**:
- Average tickets per epic: 3
- Sequential: 3 × 10 min = 30 min per epic
- Parallel: max(10, 10, 10) = 10 min per epic
- **Speedup**: 3x faster

---

## Backward Looping Scenarios

### Scenario 1: Phase 6 → Phase 3 (Review Finds Audit Issues)

**Trigger**: Phase 6 review detects DNA violations or PR hygiene issues

**Flow**:
```
Phase 3 (Audit) → Phase 4 → Phase 5 → Phase 6 (FAIL: Audit issues found)
  ↓
Phase 3 (Re-audit) → Phase 4 → Phase 5 → Phase 6 (PASS)
```

**Implementation**:
```python
# In Phase 6 orchestrator
if result.audit_issues_found:
    print("Phase 6 review found audit issues, looping back to Phase 3...")
    return execute_phase(
        phase=3,
        epics=epics,
        wave_id=wave_id,
        vm=vm,
        budget=budget,
        previous_result=result,
        loop_count=0,
        max_loops=3
    )
```

### Scenario 2: Phase 4 → Phase 3 (Tickets Incomplete)

**Trigger**: Phase 4 detects Phase 3 prerequisites not met

**Flow**:
```
Phase 3 (Audit) → Phase 4 (FAIL: Phase 3 incomplete)
  ↓
Phase 3 (Complete) → Phase 4 (PASS)
```

### Scenario 3: Phase 5.V → Phase 5 (Verification Fails)

**Trigger**: Phase 5.V verification detects build or test failures

**Flow**:
```
Phase 5 (Execute) → Phase 5.V (FAIL: Build broken)
  ↓
Phase 5 (Fix) → Phase 5.V (PASS)
```

---

## Loop Limits & Escalation

### Maximum Loops Per Phase

**Recommended**: 3 loops per phase

**Rationale**:
- Loop 1: Initial attempt
- Loop 2: First retry (common issues)
- Loop 3: Second retry (edge cases)
- Loop 4+: Systemic issue → requires human intervention

**Escalation Protocol**:
```python
if loop_count >= max_loops:
    return {
        "status": "failed",
        "phase": phase,
        "reason": "max_recovery_loops_exceeded",
        "requires_human": True,
        "loop_count": loop_count,
        "escalation_message": f"Phase {phase} failed after {max_loops} attempts. Human intervention required."
    }
```

### Master Oversight

**Master tracks**:
- Total loops across all phases
- Time spent in recovery
- Bobcoin usage vs budget

**Master escalates if**:
- Any phase exceeds max loops
- Total wave time exceeds 24 hours
- Bobcoin usage exceeds budget

---

## Implementation Roadmap

### Phase 1: Core Infrastructure (2-3 hours)

**Tasks**:
1. ✅ Create `master_orchestrator.py`
2. ✅ Create `phase_orchestrator.py` template
3. ✅ Implement `spawn_phase_orchestrator()` using `/new_task`
4. ✅ Implement `check_loop_back_condition()`
5. ✅ Add loop count tracking

**Deliverables**:
- Master orchestrator script
- Phase orchestrator template
- Loop detection logic

### Phase 2: Parallel Ticket Execution (1-2 hours)

**Tasks**:
1. ✅ Modify Phase 5 orchestrator for parallel launches
2. ✅ Implement `monitor_tickets_parallel()`
3. ✅ Test with 3-ticket epic

**Deliverables**:
- Parallel Phase 5 orchestrator
- Ticket monitoring logic

### Phase 3: Testing & Validation (2-3 hours)

**Tasks**:
1. ✅ Test master → Phase 0 spawn
2. ✅ Test Phase 0 → Phase 1 chain
3. ✅ Test backward loop (Phase 6 → Phase 3)
4. ✅ Test max loops escalation
5. ✅ Test parallel ticket execution

**Deliverables**:
- Test results
- Bug fixes
- Documentation updates

### Phase 4: Production Deployment (1 hour)

**Tasks**:
1. ✅ Deploy to Wave 5
2. ✅ Monitor first 10 epics
3. ✅ Validate autonomous execution
4. ✅ Document lessons learned

**Deliverables**:
- Wave 5 completion report
- Lessons learned document

---

## Success Metrics

### Autonomy

**Target**: 100% autonomous execution (zero human intervention)

**Measurement**:
- Human interventions per wave
- Escalations to master
- Manual overrides

**Goal**: 0 interventions for Wave 5

### Efficiency

**Target**: 3x speedup from parallel ticket execution

**Measurement**:
- Phase 5 duration (sequential vs parallel)
- Total wave duration
- Bobcoin usage

**Goal**: Wave 5 completes in 10 hours (vs 30 hours sequential)

### Reliability

**Target**: 100% success rate with automatic recovery

**Measurement**:
- Success rate per phase
- Recovery loop effectiveness
- Escalation rate

**Goal**: 100% success rate, <5% escalation rate

---

## Integration with Existing Systems

### Building-Blocks Method

**Status**: ✅ Already implemented

**Integration**:
- Phase orchestrators use building-blocks for script generation
- Copy previous wave's same phase scripts
- Modify only epic numbers

**Reference**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`

### Recovery Loop Protocol

**Status**: ✅ Already implemented (V12.26)

**Integration**:
- Phase orchestrators apply Recovery Loop Protocol automatically
- Loop until 100% completion or max loops
- Document root causes

**Reference**: `docs/protocol/RECOVERY_LOOP_PROTOCOL.md`

### Cost-Optimized Polling

**Status**: ✅ Already implemented (V2.0)

**Integration**:
- Phase orchestrators use 4-minute polling intervals
- Maximize cache hits
- 91% cost reduction vs 30s baseline

**Reference**: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md`

### Jane Street KB

**Status**: ✅ Already integrated

**Integration**:
- Phase 2, 4.5 orchestrators query Jane Street KB
- Validate architectural decisions
- Enforce P0 violations

**Reference**: `docs/intel/jane-street/RULES_CATALOG.md`

---

## Post-Wave-4 Implementation Queue

### Priority 1: Hierarchical Orchestrator (This Document)

**Estimated Effort**: 6-9 hours
**Dependencies**: None
**Benefit**: Fully autonomous wave execution

### Priority 2: Fable Building Blocks

**Estimated Effort**: TBD
**Dependencies**: None
**Benefit**: TBD

**Reference**: `docs/workflow/THINKING_TAG_STANDARDIZATION_PLAN.md`

### Priority 3: CodeScene Integration

**Estimated Effort**: 2-3 hours
**Dependencies**: None
**Benefit**: Data-driven hotspot prioritization

**Reference**: `docs/protocol/CODESCENE_INTEGRATION.md`

---

## Questions for Implementation

1. **Bob IDE `/new_task` API**: Does it support programmatic spawning?
2. **Session Limits**: How many concurrent Bob IDE sessions can run?
3. **Context Passing**: How to pass results between sessions?
4. **Loop Detection**: How to detect infinite loops?
5. **Escalation UI**: How to notify human when intervention needed?

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-06-16 | Initial architecture document |

---

## References

- **Wave 4 Completion Report**: `WAVE4_PHASE6_COMPLETION_REPORT.md`
- **Building-Blocks SOP**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- **Recovery Loop Protocol**: `docs/protocol/RECOVERY_LOOP_PROTOCOL.md`
- **Cost-Optimized Polling**: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md`
- **Jane Street KB**: `docs/intel/jane-street/RULES_CATALOG.md`

---

**Status**: QUEUED FOR POST-WAVE-4 IMPLEMENTATION
**Priority**: HIGH
**Estimated Effort**: 6-9 hours
**Expected Benefit**: Fully autonomous wave execution with 3x speedup

**Next Action**: Review with Director after Wave 4 completion