# Phase Orchestrator Subagent Templates

**Version**: 1.0
**Model**: Bob IDE V2 — 3-Tier Subagent Architecture
**Purpose**: Exact `description` payloads the Top-Level Orchestrator passes to each Phase Orchestrator subagent.

---

## Architecture Overview

```
Tier 1: Top-Level Orchestrator (autonomous-refactor mode)
   Spawns Phase Orchestrators SEQUENTIALLY — Ph0 → Ph1 → Ph1.5 → Ph2 → Ph3 → Ph4 → Ph4.5 → Ph5 → Ph5.V → Ph6
   NEVER spawns Ph(N+1) until Ph(N) reports "161/161 VERIFIED COMPLETE"

Tier 2: Phase Orchestrators (autonomous-refactor mode, 1 per phase, spawned sequentially)
   Each Phase Orchestrator:
     1. Spawns 161 epic workers SIMULTANEOUSLY in the phase-specific custom mode
     2. Collects results from all 161 workers
     3. Runs COMPLETION VERIFICATION LOOP — re-spawns every failed worker until 161/161
     4. Reports "161/161 VERIFIED COMPLETE" to Tier 1 (or HARD FAILURE with analysis)

Tier 3: Epic Workers (phase-specific custom modes, 161 per phase, fully parallel)
   Each epic worker:
     1. Reads its assigned input artifact
     2. Executes phase work using the correct custom mode
     3. Writes output artifact to docs/brain/EPIC-W7-NNN/
     4. Returns {status, output_path, cyc_achieved} to Phase Orchestrator
```

---

## 100% Completion Enforcement Protocol

**Every Phase Orchestrator MUST run this loop before reporting back to Tier 1:**

```
COMPLETION VERIFICATION LOOP:
  Round 1: Spawn all 161 workers simultaneously. Collect results.
  Check: Count successes (output artifact exists + correct format).
  If < 161/161:
    Round N: Spawn ONLY the failed workers again (do NOT re-run successes).
    Log each failure to .lamport/wave7/event_log.jsonl
    Write failure-analysis.md to docs/brain/EPIC-W7-NNN/ for each failure
    Retry up to 3 rounds.
  If still < 161/161 after 3 rounds:
    Report HARD FAILURE to Tier 1 with list of stuck epics.
    Tier 1 will escalate to Director.
  Only report "COMPLETE" when:
    - All 161 output artifacts exist on disk
    - All 161 manifests updated with phase status = completed
    - Zero epics with missing or malformed output
```

---

## Lamport Clock Protocol (MANDATORY — ALL Phase Orchestrators)

The Lamport clock is a **causality enforcement gate**, not just a tracking ledger.
**A phase MUST NOT start work if its dependency phase has not completed per the log.**

### Step 1 — DEPENDENCY GATE (run BEFORE any work, BEFORE spawning workers)

```python
# Run this check as the VERY FIRST ACTION of every Phase Orchestrator.
# Replace REQUIRED_EVENT and REQUIRED_PHASE with the predecessor values.

import json, sys

log_path = ".lamport/wave7/event_log.jsonl"
REQUIRED_EVENT = "phase_N_minus_1_orchestrator_complete"   # e.g. "phase_0_orchestrator_complete"
REQUIRED_PHASE = "N-1"                                      # e.g. "0"

try:
    events = [json.loads(l) for l in open(log_path) if l.strip()]
except FileNotFoundError:
    print("HALT: Lamport log missing — wave never started"); sys.exit(1)

gate = [e for e in events
        if e.get("event_type") == REQUIRED_EVENT
        and e.get("phase") == REQUIRED_PHASE
        and e.get("status") == "complete"]

if not gate:
    # HARD STOP — dependency not satisfied
    print(f"HALT: {REQUIRED_EVENT} not found in Lamport log.")
    print("This phase MUST NOT proceed. Report DEPENDENCY_NOT_MET to Tier 1.")
    sys.exit(1)

gate_clock = gate[-1]["lamport_clock"]
print(f"Gate passed: {REQUIRED_EVENT} at clock={gate_clock}. Proceeding.")
```

**DEPENDENCY MAP (what each phase requires in the log before starting):**

| Phase | Required Lamport Event | Required Status |
|-------|----------------------|-----------------|
| **0** | `wave_start` (clock ≥ 1) | `running` |
| **1** | `phase_0_orchestrator_complete` | `complete` |
| **1.5** | `phase_1_orchestrator_complete` | `complete` |
| **2** | `phase_1_5_orchestrator_complete` | `complete` |
| **3** | `phase_2_orchestrator_complete` | `complete` |
| **4** | `phase_3_orchestrator_complete` | `complete` |
| **4.5** | `phase_4_orchestrator_complete` | `complete` |
| **5** | `phase_4_5_orchestrator_complete` | `complete` |
| **5.V** | `phase_5_orchestrator_complete` | `complete` |
| **6** | `phase_5_v_orchestrator_complete` | `complete` |

If the required event is absent → **HALT immediately. Report `DEPENDENCY_NOT_MET` to Tier 1. Do NOT spawn any workers.**

---

### Step 2 — WRITE EVENTS (read-increment-append)

```python
# Use this exact pattern for every Lamport event write.

import json, datetime

def lamport_append(event_type, phase, tier, status, epic_id="WAVE-7", note=""):
    log_path = ".lamport/wave7/event_log.jsonl"
    try:
        lines = [l for l in open(log_path) if l.strip()]
        current_clock = json.loads(lines[-1])["lamport_clock"] if lines else 0
    except Exception:
        current_clock = 0
    new_clock = current_clock + 1
    entry = {
        "timestamp": datetime.datetime.utcnow().isoformat() + "Z",
        "lamport_clock": new_clock,
        "epic_id": epic_id,
        "phase": phase,
        "tier": tier,
        "event_type": event_type,
        "status": status,
        "note": note
    }
    with open(log_path, "a") as f:
        f.write(json.dumps(entry) + "\n")
    return new_clock
```

**Rules:**
- Each append increments clock by exactly 1. Never reuse a value.
- All orchestrators share ONE log file — sequential appends only.
- `phase_N_orchestrator_complete` MUST have a strictly higher clock than all `phase_N_epic_*` events.
- Workers (Tier 3) do **NOT** write to the log. Only Phase Orchestrators (Tier 2) write events.

**Clock assignment per phase:**
| Event | event_type value | tier |
|-------|-----------------|------|
| Phase N starts | `phase_N_orchestrator_start` | `phase_orch` |
| Worker succeeded | `phase_N_epic_complete` | `phase_orch` |
| Worker failed | `phase_N_epic_failed` | `phase_orch` |
| Worker retry | `phase_N_epic_retry` | `phase_orch` |
| 161/161 verified | `phase_N_orchestrator_complete` | `phase_orch` |
| Hard failure | `phase_N_hard_failure` | `phase_orch` |
| Wave done | `wave_7_complete` | `phase_orch` |

---

## Pilot Gate Protocol (Phase 0 ONLY)

Before spawning all 161 workers, Phase 0 MUST run a 1-epic pilot:

```
PILOT_GATE (embedded in Phase 0 Orchestrator — not a separate session):
  1. Pick EPIC-W7-001 (first epic in wave7-epic-list.json) as pilot.
  2. Spawn ONE worker: mode=v12-phase0-hotspot, same description format as full run.
  3. Verify output:
       - docs/brain/EPIC-W7-001/00-hotspots.md exists and is non-empty
       - docs/brain/EPIC-W7-001/manifest.json has phase_0 status=completed
       - Worker returned { status:"success", output_path, cyc_confirmed }
  4. IF pilot PASSES: proceed immediately to spawn remaining 160 workers in parallel.
  5. IF pilot FAILS:
       - Log: { event_type:"pilot_failed", epic_id:"EPIC-W7-001", phase:"0" }
       - Write: docs/brain/EPIC-W7-001/failure-analysis.md with exact error
       - HALT — do NOT spawn remaining 160 workers
       - Report PILOT_FAILURE to Tier 1 with failure details
       - Tier 1 escalates to Director
  
  WHY: If the worker description format, manifest schema, or artifact path is wrong,
  better to catch it on 1 epic than waste 160 subagent contexts.
```

---

## Template 1: Phase 0 Orchestrator

```
ROLE: You are the Phase 0 (Hotspot Analysis) Orchestrator for Wave 7.
MODE: wave-orch-phase0
MISSION: Run Phase 0 for ALL 161 epics. Do NOT hand off until 161/161 are verified complete.

STEP 0 — LAMPORT DEPENDENCY GATE (FIRST ACTION — halt if not met):
  Run the dependency gate check from the Lamport Clock Protocol above.
  Required event: event_type="wave_start", status="running"
  Command:
    python3 -c "
import json,sys
events=[json.loads(l) for l in open('.lamport/wave7/event_log.jsonl') if l.strip()]
gate=[e for e in events if e.get('event_type')=='wave_start' and e.get('status')=='running']
sys.exit(0) if gate else (print('HALT: wave_start not found. Report DEPENDENCY_NOT_MET.') or sys.exit(1))
"
  If exit code 1: HALT. Report DEPENDENCY_NOT_MET to Tier 1. Do NOT proceed.

STEP 1 — LOG PHASE START:
  lamport_append(event_type="phase_0_orchestrator_start", phase="0", tier="phase_orch", status="running")

STEP 2 — PILOT GATE (before spawning all 161 — see Pilot Gate Protocol above):
  Spawn ONE worker for EPIC-W7-001. Verify output exists. If fails: log pilot_failed, halt, report PILOT_FAILURE.
  If passes: log pilot_passed, continue to Step 3.

STEP 3 — SPAWN REMAINING 160 WORKERS SIMULTANEOUSLY:
  Read docs/brain/wave7-epic-list.json. Skip EPIC-W7-001 (already done in pilot).
  For each remaining epic, spawn a subagent:
    mode: v12-phase0-hotspot
    description: |
      Epic: <epic_id>
      Method: <method_name> (CYC: <cyc>)
      Source: <source_file>
      Task: Run Phase 0 (Hotspot Analysis).
        1. Use jcodemunch-mcp: get_symbol_complexity, get_hotspots, get_blast_radius for <method_name>
        2. Use sequential-thinking to structure your analysis
        3. Write output to docs/brain/<epic_id>/00-hotspots.md
        4. Update docs/brain/<epic_id>/manifest.json: set phase_0.status=completed
        Return: { status: "success"|"failure", output_path, cyc_confirmed }

STEP 4 — COMPLETION VERIFICATION LOOP (161/161 required):
  After all workers return, count confirmed outputs (file exists + non-empty).
  For any epic WITHOUT valid 00-hotspots.md:
    lamport_append(event_type="phase_0_epic_failed", phase="0", tier="phase_orch",
                   status="retry", epic_id=<epic_id>)
    Write: docs/brain/<epic_id>/failure-analysis.md
    Re-spawn that worker ONLY (do not re-run successes). Up to 3 retry rounds.
  For each success:
    lamport_append(event_type="phase_0_epic_complete", phase="0", tier="phase_orch",
                   status="success", epic_id=<epic_id>)
  If still < 161/161 after 3 rounds:
    lamport_append(event_type="phase_0_hard_failure", phase="0", tier="phase_orch", status="hard_failure")
    Report HARD_FAILURE to Tier 1 with stuck epic list. HALT.

STEP 5 — LOG PHASE COMPLETE (only after 161/161 verified):
  lamport_append(event_type="phase_0_orchestrator_complete", phase="0", tier="phase_orch",
                 status="complete", note="161/161 verified")

REPORT BACK TO TIER 1:
  {
    "phase": "0",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "failed": 0,
    "lamport_complete_clock": <clock from step 5>,
    "output_base": "docs/brain/EPIC-W7-NNN/00-hotspots.md"
  }
```

---

## Template 2: Phase 1 Orchestrator

```
ROLE: You are the Phase 1 (Scope Definition) Orchestrator for Wave 7.
MODE: wave-orch-phase1
MISSION: Run Phase 1 for ALL 161 epics. Do NOT hand off until 161/161 are verified complete.

STEP 0 — LAMPORT DEPENDENCY GATE (FIRST ACTION — halt if not met):
  python3 -c "
import json,sys
events=[json.loads(l) for l in open('.lamport/wave7/event_log.jsonl') if l.strip()]
gate=[e for e in events if e.get('event_type')=='phase_0_orchestrator_complete' and e.get('status')=='complete']
sys.exit(0) if gate else (print('HALT: phase_0_orchestrator_complete not found.') or sys.exit(1))
"
  If exit code 1: HALT. Report DEPENDENCY_NOT_MET (Phase 0 not complete) to Tier 1. Do NOT proceed.

STEP 1 — LOG PHASE START:
  lamport_append(event_type="phase_1_orchestrator_start", phase="1", tier="phase_orch", status="running")

STEP 2 — SPAWN ALL 161 WORKERS SIMULTANEOUSLY:
  For each epic, spawn a subagent:
    mode: v12-phase1-scope
    description: |
      Epic: <epic_id>
      Method: <method_name> (CYC: <cyc>)
      Source: <source_file>
      Input: docs/brain/<epic_id>/00-hotspots.md
      Task: Run Phase 1 (Scope Definition).
        1. Read 00-hotspots.md
        2. Use jcodemunch-mcp: get_file_outline, find_references, get_dependency_graph
        3. Use sequential-thinking to validate scope is SINGLE METHOD only
        4. Write output to docs/brain/<epic_id>/00-scope.md
        5. Update manifest.json with phase_1 status=completed
        Return: { status, output_path, scope_confirmed_single_method: true|false }

STEP 3 — COMPLETION VERIFICATION LOOP (161/161 required):
  For each success: lamport_append(event_type="phase_1_epic_complete", phase="1", tier="phase_orch", status="success", epic_id=<epic_id>)
  For each failure: lamport_append(event_type="phase_1_epic_failed", ...)  → re-spawn up to 3 rounds.
  Additional check: scope_confirmed_single_method MUST be true for all 161. Flag false=HARD_FAILURE.
  After 3 rounds still incomplete: lamport_append(event_type="phase_1_hard_failure", ...) → HALT.

STEP 4 — LOG PHASE COMPLETE (only after 161/161 verified):
  lamport_append(event_type="phase_1_orchestrator_complete", phase="1", tier="phase_orch", status="complete", note="161/161 verified")

REPORT BACK TO TIER 1:
  {
    "phase": "1",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "failed": 0,
    "scope_violations": 0,
    "lamport_complete_clock": <clock>
  }
```

---

## Template 3: Phase 1.5 Orchestrator

```
ROLE: You are the Phase 1.5 (Scope Boundary Validation) Orchestrator for Wave 7.
MODE: wave-orch-phase1-5
MISSION: Run Phase 1.5 for ALL 161 epics. This is the SCOPE CREEP BLOCKER gate.
         Do NOT hand off until 161/161 pass the boundary check.

STEP 0 — LAMPORT DEPENDENCY GATE (FIRST ACTION — halt if not met):
  python3 -c "
import json,sys
events=[json.loads(l) for l in open('.lamport/wave7/event_log.jsonl') if l.strip()]
gate=[e for e in events if e.get('event_type')=='phase_1_orchestrator_complete' and e.get('status')=='complete']
sys.exit(0) if gate else (print('HALT: phase_1_orchestrator_complete not found.') or sys.exit(1))
"
  If exit code 1: HALT. Report DEPENDENCY_NOT_MET (Phase 1 not complete) to Tier 1. Do NOT proceed.

STEP 1 — LOG PHASE START:
  lamport_append(event_type="phase_1_5_orchestrator_start", phase="1.5", tier="phase_orch", status="running")

STEP 2 — SPAWN ALL 161 WORKERS SIMULTANEOUSLY:
  For each epic, spawn a subagent:
    mode: v12-phase1-5-boundary
    description: |
      Epic: <epic_id>
      Method: <method_name> (CYC: <cyc>)
      Source: <source_file>
      Input: docs/brain/<epic_id>/00-scope.md
      Task: Run Phase 1.5 (Scope Boundary Validation).
        1. Read 00-scope.md
        2. Use jcodemunch-mcp: get_symbol_source, get_blast_radius, find_references
        3. Use sequential-thinking to validate: scope touches ONLY <method_name>, zero adjacent changes
        4. BLOCKER: If scope exceeds single method, mark as SCOPE_VIOLATION and halt epic
        5. Write output to docs/brain/<epic_id>/01-scope-boundary.md
           Include: boundary_verdict: PASS|FAIL, blocker_reason (if FAIL)
        6. Update manifest.json with phase_1_5 status=completed|blocked
        Return: { status, output_path, boundary_verdict: "PASS"|"FAIL" }

STEP 3 — COMPLETION VERIFICATION LOOP (161/161 PASS required):
  For each PASS: lamport_append(event_type="phase_1_5_epic_complete", phase="1.5", ..., status="success")
  For SCOPE_VIOLATION: lamport_append(event_type="phase_1_5_epic_blocked", ..., status="hard_failure") → NOT retried → HALT.
  For technical failure: lamport_append(event_type="phase_1_5_epic_failed", ..., status="retry") → re-spawn up to 3 rounds.
  Wave does not proceed with any boundary_verdict=FAIL unresolved.

STEP 4 — LOG PHASE COMPLETE (only after all 161 PASS):
  lamport_append(event_type="phase_1_5_orchestrator_complete", phase="1.5", tier="phase_orch", status="complete", note="161/161 PASS")

REPORT BACK TO TIER 1:
  {
    "phase": "1.5",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "passed_boundary": 161,
    "scope_violations": 0,
    "lamport_complete_clock": <clock>
  }
```

---

## Template 4: Phase 2 Orchestrator

```
ROLE: You are the Phase 2 (Architecture Planning) Orchestrator for Wave 7.
MODE: wave-orch-phase2
MISSION: Run Phase 2 for ALL 161 epics. Mandatory Jane Street KB query before spawning workers.
         Do NOT hand off until 161/161 architecture plans are verified.

STEP 0 — LAMPORT DEPENDENCY GATE (FIRST ACTION — halt if not met):
  python3 -c "
import json,sys
events=[json.loads(l) for l in open('.lamport/wave7/event_log.jsonl') if l.strip()]
gate=[e for e in events if e.get('event_type')=='phase_1_5_orchestrator_complete' and e.get('status')=='complete']
sys.exit(0) if gate else (print('HALT: phase_1_5_orchestrator_complete not found.') or sys.exit(1))
"
  If exit code 1: HALT. Report DEPENDENCY_NOT_MET (Phase 1.5 not complete) to Tier 1. Do NOT proceed.

STEP 1 — LOG PHASE START:
  lamport_append(event_type="phase_2_orchestrator_start", phase="2", tier="phase_orch", status="running")

MANDATORY JANE STREET KB QUERY (run AFTER gate, BEFORE spawning workers):
  python scripts/query_kb.py "extraction patterns"
  python scripts/query_kb.py "complexity reduction FSM"
  python scripts/query_kb.py "lock-free actor pattern"
  Capture KB results and include them in ALL worker descriptions below.

SPAWN ALL 161 WORKERS SIMULTANEOUSLY:
  For each epic, spawn a subagent:
    mode: v12-phase2-architecture
    description: |
      Epic: <epic_id>
      Method: <method_name> (CYC: <cyc>)
      Source: <source_file>
      Input: docs/brain/<epic_id>/01-scope-boundary.md
      Jane Street KB Results: <paste KB results here>
      Task: Run Phase 2 (Architecture Planning).
        1. Read 01-scope-boundary.md
        2. Use jcodemunch-mcp: get_context_bundle, get_call_hierarchy, get_dependency_graph
        3. Use sequential-thinking to design extraction plan: which sub-methods to extract, names, CYC targets
        4. Validate: extracted methods must ALL be CYC <= 8. Parent method must be CYC <= 8.
        5. Write output to docs/brain/<epic_id>/02-architecture-plan.md
           Include: extraction_map, target_cyc_per_method, jane_street_patterns_applied
        6. Optionally write docs/brain/<epic_id>/02-diagrams.mmd (Mermaid)
        7. Update manifest.json with phase_2 status=completed
        Return: { status, output_path, extraction_count, max_cyc_projected }

STEP 3 — COMPLETION VERIFICATION LOOP (161/161 required, max_cyc_projected <= 8):
  lamport_append(event_type="phase_2_kb_query_complete", phase="2", ..., status="success")
  For each success: lamport_append(event_type="phase_2_epic_complete", ...)
  For each failure/replan: lamport_append(event_type="phase_2_epic_failed", ...) → re-spawn up to 3 rounds.
  After 3 rounds: lamport_append(event_type="phase_2_hard_failure", ...) → HALT.

STEP 4 — LOG PHASE COMPLETE:
  lamport_append(event_type="phase_2_orchestrator_complete", phase="2", tier="phase_orch", status="complete", note="161/161 verified")

REPORT BACK TO TIER 1:
  {
    "phase": "2",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "failed": 0,
    "max_cyc_violations": 0,
    "lamport_complete_clock": <clock>,
    "kb_queries_run": ["extraction patterns", "complexity reduction FSM", "lock-free actor pattern"]
  }
```

---

## Template 5: Phase 3 Orchestrator

```
ROLE: You are the Phase 3 (DNA & PR Audit) Orchestrator for Wave 7.
MODE: wave-orch-phase3
MISSION: Run Phase 3 for ALL 161 epics. Verify V12 DNA compliance before any code is written.
         Do NOT hand off until 161/161 audits pass.

STEP 0 — LAMPORT DEPENDENCY GATE (FIRST ACTION — halt if not met):
  python3 -c "
import json,sys
events=[json.loads(l) for l in open('.lamport/wave7/event_log.jsonl') if l.strip()]
gate=[e for e in events if e.get('event_type')=='phase_2_orchestrator_complete' and e.get('status')=='complete']
sys.exit(0) if gate else (print('HALT: phase_2_orchestrator_complete not found.') or sys.exit(1))
"
  If exit code 1: HALT. Report DEPENDENCY_NOT_MET (Phase 2 not complete) to Tier 1. Do NOT proceed.

STEP 1 — LOG PHASE START:
  lamport_append(event_type="phase_3_orchestrator_start", phase="3", tier="phase_orch", status="running")

STEP 2 — SPAWN ALL 161 WORKERS SIMULTANEOUSLY:
  For each epic, spawn a subagent:
    mode: v12-phase3-audit
    description: |
      Epic: <epic_id>
      Method: <method_name> (CYC: <cyc>)
      Source: <source_file>
      Input: docs/brain/<epic_id>/02-architecture-plan.md
      Task: Run Phase 3 (DNA & PR Audit).
        1. Read 02-architecture-plan.md
        2. Use jcodemunch-mcp: search_ast, get_layer_violations, get_dependency_cycles
        3. Use sequential-thinking to validate against V12 DNA rules:
           - Zero lock() blocks in proposed extraction
           - ASCII-only (no Unicode/emoji in string literals)
           - UTF-8 source files (no BOM)
           - No scope creep (single method only)
           - xUnit tests planned (NOT NUnit/MSTest)
        4. Write output to docs/brain/<epic_id>/03-audit-report.md
           Include: dna_verdict: PASS|FAIL, violations (list), blocker_count
        5. Update manifest.json with phase_3 status=completed|blocked
        Return: { status, output_path, dna_verdict: "PASS"|"FAIL", violations: [] }

STEP 3 — COMPLETION VERIFICATION LOOP (161/161 dna_verdict=PASS required):
  For each PASS: lamport_append(event_type="phase_3_epic_complete", phase="3", ..., status="success")
  For DNA FAIL: lamport_append(event_type="phase_3_epic_blocked", ..., status="hard_failure") → NOT retried → report to Tier 1.
  For technical failure: lamport_append(event_type="phase_3_epic_failed", ..., status="retry") → re-spawn up to 3 rounds.

STEP 4 — LOG PHASE COMPLETE:
  lamport_append(event_type="phase_3_orchestrator_complete", phase="3", tier="phase_orch", status="complete", note="161/161 PASS")

REPORT BACK TO TIER 1:
  {
    "phase": "3",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "dna_violations": 0,
    "blocked": 0,
    "lamport_complete_clock": <clock>
  }
```

---

## Template 6: Phase 4 Orchestrator

```
ROLE: You are the Phase 4 (Ticket Generation) Orchestrator for Wave 7.
MODE: wave-orch-phase4
MISSION: Run Phase 4 for ALL 161 epics. Generate actionable implementation tickets.
         Do NOT hand off until 161/161 ticket files are verified.

STEP 0 — LAMPORT DEPENDENCY GATE (FIRST ACTION — halt if not met):
  python3 -c "
import json,sys
events=[json.loads(l) for l in open('.lamport/wave7/event_log.jsonl') if l.strip()]
gate=[e for e in events if e.get('event_type')=='phase_3_orchestrator_complete' and e.get('status')=='complete']
sys.exit(0) if gate else (print('HALT: phase_3_orchestrator_complete not found.') or sys.exit(1))
"
  If exit code 1: HALT. Report DEPENDENCY_NOT_MET (Phase 3 not complete) to Tier 1. Do NOT proceed.

STEP 1 — LOG PHASE START:
  lamport_append(event_type="phase_4_orchestrator_start", phase="4", tier="phase_orch", status="running")

STEP 2 — SPAWN ALL 161 WORKERS SIMULTANEOUSLY:
  For each epic, spawn a subagent:
    mode: v12-phase4-tickets
    description: |
      Epic: <epic_id>
      Method: <method_name> (CYC: <cyc>)
      Source: <source_file>
      Input: docs/brain/<epic_id>/02-architecture-plan.md (primary)
             docs/brain/<epic_id>/03-audit-report.md (constraint reference)
      Task: Run Phase 4 (Ticket Generation).
        1. Read 02-architecture-plan.md and 03-audit-report.md
        2. Use jcodemunch-mcp: get_symbol_complexity, get_extraction_candidates
        3. Use sequential-thinking to break architecture plan into concrete implementation tickets:
           - Ticket 1: Extract <sub-method-A> (target CYC <= 8)
           - Ticket 2: Extract <sub-method-B> (target CYC <= 8)
           - Ticket N: Update parent to CYC <= 8, write xUnit tests
           Each ticket: { id, title, files_to_modify, lines_to_change, test_requirement }
        4. Write output to docs/brain/<epic_id>/04-tickets.md
        5. Update manifest.json with phase_4 status=completed, ticket_count=N
        Return: { status, output_path, ticket_count }

STEP 3 — COMPLETION VERIFICATION LOOP (161/161, ticket_count >= 1 required):
  For each success: lamport_append(event_type="phase_4_epic_complete", phase="4", ..., status="success")
  For failure: lamport_append(event_type="phase_4_epic_failed", ..., status="retry") → re-spawn up to 3 rounds.
  After 3 rounds: lamport_append(event_type="phase_4_hard_failure", ...) → HALT.

STEP 4 — LOG PHASE COMPLETE:
  lamport_append(event_type="phase_4_orchestrator_complete", phase="4", tier="phase_orch", status="complete")

REPORT BACK TO TIER 1:
  {
    "phase": "4",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "failed": 0,
    "total_tickets_generated": "<sum>",
    "lamport_complete_clock": <clock>
  }
```

---

## Template 7: Phase 4.5 Orchestrator

```
ROLE: You are the Phase 4.5 (Ticket Review) Orchestrator for Wave 7.
MODE: wave-orch-phase4-5
MISSION: Validate ALL 161 ticket sets against Jane Street KB standards.
         This is the last gate before code is written. 100% pass required.

STEP 0 — LAMPORT DEPENDENCY GATE (FIRST ACTION — halt if not met):
  python3 -c "
import json,sys
events=[json.loads(l) for l in open('.lamport/wave7/event_log.jsonl') if l.strip()]
gate=[e for e in events if e.get('event_type')=='phase_4_orchestrator_complete' and e.get('status')=='complete']
sys.exit(0) if gate else (print('HALT: phase_4_orchestrator_complete not found.') or sys.exit(1))
"
  If exit code 1: HALT. Report DEPENDENCY_NOT_MET (Phase 4 not complete) to Tier 1. Do NOT proceed.

STEP 1 — LOG PHASE START:
  lamport_append(event_type="phase_4_5_orchestrator_start", phase="4.5", tier="phase_orch", status="running")

MANDATORY JANE STREET KB QUERY (run AFTER gate, BEFORE spawning workers):
  python scripts/query_kb.py "complexity reduction"
  python scripts/query_kb.py "testing strategies xUnit"
  python scripts/query_kb.py "FSM actor enqueue"
  python scripts/query_kb.py "lock-free patterns"
  Capture KB results and include them in ALL worker descriptions below.

SPAWN ALL 161 WORKERS SIMULTANEOUSLY:
  For each epic, spawn a subagent:
    mode: v12-phase4-5-review
    description: |
      Epic: <epic_id>
      Method: <method_name> (CYC: <cyc>)
      Input: docs/brain/<epic_id>/04-tickets.md
      Jane Street KB Results: <paste KB results here>
      Task: Run Phase 4.5 (Ticket Review).
        1. Read 04-tickets.md
        2. Use sequential-thinking to validate each ticket against Jane Street KB:
           - CYC reduction path is provably achievable (math: complexity sum check)
           - No lock() patterns introduced
           - xUnit tests specified per ticket ([Fact], Assert.Equal())
           - ASCII-only identifiers
           - Single-concern per ticket (no scope creep)
        3. Write output to docs/brain/<epic_id>/04-5-ticket-review.md
           Include: review_verdict: PASS|FAIL, failed_tickets: [], kb_rules_applied: []
        4. Update manifest.json with phase_4_5 status=completed|blocked
        Return: { status, output_path, review_verdict: "PASS"|"FAIL", failed_tickets: [] }

STEP 3 — COMPLETION VERIFICATION LOOP (review_verdict=PASS required for all 161):
  lamport_append(event_type="phase_4_5_kb_query_complete", phase="4.5", ..., status="success")
  For each PASS: lamport_append(event_type="phase_4_5_epic_complete", ..., status="success")
  For FAIL: lamport_append(event_type="phase_4_5_epic_blocked", ...) → re-spawn Ph4 worker then Ph4.5 for that epic. Up to 3 rounds.
  After 3 rounds: lamport_append(event_type="phase_4_5_hard_failure", ...) → HALT.

STEP 4 — LOG PHASE COMPLETE:
  lamport_append(event_type="phase_4_5_orchestrator_complete", phase="4.5", tier="phase_orch", status="complete")

REPORT BACK TO TIER 1:
  {
    "phase": "4.5",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "failed": 0,
    "lamport_complete_clock": <clock>,
    "kb_queries_run": ["complexity reduction", "testing strategies xUnit", "FSM actor enqueue", "lock-free patterns"]
  }
```

---

## Template 8: Phase 5 Orchestrator

```
ROLE: You are the Phase 5 (Ticket Execution) Orchestrator for Wave 7.
MODE: wave-orch-phase5
MISSION: Execute ALL implementation tickets for ALL 161 epics.
         THIS IS THE CODE-WRITING PHASE. Workers have full file access.
         Do NOT hand off until 161/161 are verified complete.

STEP 0 — LAMPORT DEPENDENCY GATE (FIRST ACTION — halt if not met):
  python3 -c "
import json,sys
events=[json.loads(l) for l in open('.lamport/wave7/event_log.jsonl') if l.strip()]
gate=[e for e in events if e.get('event_type')=='phase_4_5_orchestrator_complete' and e.get('status')=='complete']
sys.exit(0) if gate else (print('HALT: phase_4_5_orchestrator_complete not found.') or sys.exit(1))
"
  If exit code 1: HALT. Report DEPENDENCY_NOT_MET (Phase 4.5 not complete) to Tier 1. Do NOT proceed.

STEP 1 — LOG PHASE START:
  lamport_append(event_type="phase_5_orchestrator_start", phase="5", tier="phase_orch", status="running")

MANDATORY JANE STREET KB QUERY (run AFTER gate, BEFORE spawning workers):
  python scripts/query_kb.py "FSM extraction implementation"
  python scripts/query_kb.py "xUnit test patterns Fact Assert"
  python scripts/query_kb.py "C# method extraction CYC reduction"
  Capture KB results and include them in ALL worker descriptions below.

SPAWN ALL 161 WORKERS SIMULTANEOUSLY:
  For each epic, spawn a subagent:
    mode: v12-engineer
    description: |
      Epic: <epic_id>
      Method: <method_name> (CYC: <cyc> → target CYC <= 8)
      Source: <source_file> (UTF-8, no BOM)
      Input: docs/brain/<epic_id>/04-tickets.md
             docs/brain/<epic_id>/04-5-ticket-review.md
      Jane Street KB Results: <paste KB results here>
      Task: Run Phase 5 (Ticket Execution). You have FULL file write access.
        CRITICAL RULES (V12 DNA — non-negotiable):
          - xUnit ONLY: [Fact], Assert.Equal() — NEVER NUnit, NEVER MSTest
          - UTF-8 source files (no BOM, no ASCII-only violations)
          - Zero lock() blocks — use FSM/Actor Enqueue model
          - ASCII-only string literals (no Unicode, no emoji)
          - CSharpier format after every file write: dotnet csharpier format src/
          - SINGLE CONCERN: only modify <method_name> and its new extracted helpers
        EXECUTION:
          1. Read 04-tickets.md and 04-5-ticket-review.md
          2. Execute each ticket in order:
             a. Use jcodemunch-mcp: get_symbol_source, get_context_bundle, plan_refactoring
             b. Write extracted helper method(s) to <source_file>
             c. Refactor <method_name> to call the helpers (CYC <= 8 achieved)
             d. Write xUnit test(s) to tests/ covering extracted logic
          3. Run: python scripts/complexity_audit.py (verify CYC <= 8)
          4. Run: dotnet build (must pass with ZERO errors)
          5. Run: dotnet csharpier format src/
          6. Write docs/brain/<epic_id>/ticket-X-completion.md for each ticket
          7. Update manifest.json with phase_5 status=completed, cyc_achieved=N
        Return: { status, cyc_achieved, build_passed: true|false, tests_written: N }

STEP 3 — COMPLETION VERIFICATION LOOP (cyc_achieved<=8 AND build_passed=true for all 161):
  lamport_append(event_type="phase_5_kb_query_complete", phase="5", ..., status="success")
  For each success: lamport_append(event_type="phase_5_epic_complete", phase="5", ..., status="success", epic_id=<epic_id>)
  For failure (CYC>8 or build error): lamport_append(event_type="phase_5_epic_failed", ..., status="retry") → re-spawn up to 3 rounds.
  After 3 rounds: lamport_append(event_type="phase_5_hard_failure", ...) → HALT.

STEP 4 — LOG PHASE COMPLETE (only after 161/161 cyc<=8 AND build pass):
  lamport_append(event_type="phase_5_orchestrator_complete", phase="5", tier="phase_orch", status="complete", note="161/161 CYC<=8 build pass")

REPORT BACK TO TIER 1:
  {
    "phase": "5",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "failed": 0,
    "cyc_violations": 0,
    "build_failures": 0,
    "lamport_complete_clock": <clock>,
    "kb_queries_run": ["FSM extraction implementation", "xUnit test patterns Fact Assert", "C# method extraction CYC reduction"]
  }
```

---

## Template 9: Phase 5.V Orchestrator

```
ROLE: You are the Phase 5.V (Verification) Orchestrator for Wave 7.
MODE: wave-orch-phase5v
MISSION: Independently verify ALL 161 implementations. This is a SEPARATE verification pass —
         do NOT trust Phase 5's self-reported results. Verify everything from scratch.
         Do NOT hand off until 161/161 pass independent verification.

STEP 0 — LAMPORT DEPENDENCY GATE (FIRST ACTION — halt if not met):
  python3 -c "
import json,sys
events=[json.loads(l) for l in open('.lamport/wave7/event_log.jsonl') if l.strip()]
gate=[e for e in events if e.get('event_type')=='phase_5_orchestrator_complete' and e.get('status')=='complete']
sys.exit(0) if gate else (print('HALT: phase_5_orchestrator_complete not found.') or sys.exit(1))
"
  If exit code 1: HALT. Report DEPENDENCY_NOT_MET (Phase 5 not complete) to Tier 1. Do NOT proceed.

STEP 1 — LOG PHASE START:
  lamport_append(event_type="phase_5v_orchestrator_start", phase="5.V", tier="phase_orch", status="running")

MANDATORY JANE STREET KB QUERY (run AFTER gate, BEFORE spawning workers):
  python scripts/query_kb.py "lock-free patterns verification"
  python scripts/query_kb.py "DNA compliance audit C#"
  python scripts/query_kb.py "complexity threshold 8 Jane Street"
  Capture KB results and include them in ALL worker descriptions below.

SPAWN ALL 161 WORKERS SIMULTANEOUSLY:
  For each epic, spawn a subagent:
    mode: v12-phase5-v-verify
    description: |
      Epic: <epic_id>
      Method: <method_name> (original CYC: <cyc> → claimed CYC: <cyc_achieved>)
      Source: <source_file>
      Input: docs/brain/<epic_id>/ticket-X-completion.md
      Jane Street KB Results: <paste KB results here>
      Task: Run Phase 5.V (Independent Verification). Do NOT trust Phase 5 self-report.
        VERIFY ALL of the following independently:
          1. Use jcodemunch-mcp: get_symbol_complexity(<method_name>) → MUST be <= 8
          2. Use jcodemunch-mcp: get_changed_symbols() → MUST only show <method_name> + new helpers
          3. Use jcodemunch-mcp: search_ast("lock", file=<source_file>) → MUST return zero matches
          4. Check source file encoding: file --mime-encoding <source_file> → MUST be utf-8
          5. Verify xUnit tests exist: grep -r "[Fact]" tests/ → MUST find tests for <method_name>
          6. Verify NO NUnit/MSTest: grep -r "TestFixture\|TestMethod\|\[Test\]" tests/ → MUST be zero
          7. Run: python scripts/complexity_audit.py → confirm <method_name> CYC <= 8
          8. Run: dotnet build → MUST pass with ZERO errors
        Write output to docs/brain/<epic_id>/ticket-X-verification.md
          Include: verification_verdict: PASS|FAIL, failures: []
        Update manifest.json with phase_5v status=completed|failed, verification_verdict
        Return: { status, output_path, verification_verdict: "PASS"|"FAIL", failures: [] }

STEP 3 — COMPLETION VERIFICATION LOOP (verification_verdict=PASS for all 161):
  lamport_append(event_type="phase_5v_kb_query_complete", phase="5.V", ..., status="success")
  For each PASS: lamport_append(event_type="phase_5v_epic_complete", phase="5.V", ..., status="success", epic_id=<epic_id>)
  For FAIL: lamport_append(event_type="phase_5v_epic_failed", ..., status="retry") → coordinate with Tier 1 to re-run Ph5 for that epic, then re-verify. Up to 3 rounds.
  After 3 rounds: lamport_append(event_type="phase_5v_hard_failure", ...) → HALT.

STEP 4 — LOG PHASE COMPLETE (only after 161/161 independent PASS):
  lamport_append(event_type="phase_5_v_orchestrator_complete", phase="5.V", tier="phase_orch", status="complete", note="161/161 independently verified")

REPORT BACK TO TIER 1:
  {
    "phase": "5.V",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "failed": 0,
    "verification_failures": 0,
    "independent_cyc_confirmed": 161,
    "lamport_complete_clock": <clock>,
    "kb_queries_run": ["lock-free patterns verification", "DNA compliance audit C#", "complexity threshold 8 Jane Street"]
  }
```

---

## Template 10: Phase 6 Orchestrator

```
ROLE: You are the Phase 6 (Final Review) Orchestrator for Wave 7.
MODE: wave-orch-phase6
MISSION: Generate final completion reports for ALL 161 epics and validate wave completion.
         This is the TERMINAL phase. Report wave success to Tier 1 only after 161/161 confirmed.

STEP 0 — LAMPORT DEPENDENCY GATE (FIRST ACTION — halt if not met):
  python3 -c "
import json,sys
events=[json.loads(l) for l in open('.lamport/wave7/event_log.jsonl') if l.strip()]
gate=[e for e in events if e.get('event_type')=='phase_5_v_orchestrator_complete' and e.get('status')=='complete']
sys.exit(0) if gate else (print('HALT: phase_5_v_orchestrator_complete not found.') or sys.exit(1))
"
  If exit code 1: HALT. Report DEPENDENCY_NOT_MET (Phase 5.V not complete) to Tier 1. Do NOT proceed.

STEP 1 — LOG PHASE START:
  lamport_append(event_type="phase_6_orchestrator_start", phase="6", tier="phase_orch", status="running")

MANDATORY JANE STREET KB QUERY (run AFTER gate, BEFORE spawning workers):
  python scripts/query_kb.py "testing strategies coverage"
  python scripts/query_kb.py "final audit complexity Jane Street"
  Capture KB results and include them in ALL worker descriptions below.

SPAWN ALL 161 WORKERS SIMULTANEOUSLY:
  For each epic, spawn a subagent:
    mode: v12-phase6-review
    description: |
      Epic: <epic_id>
      Method: <method_name> (original CYC: <cyc> → verified CYC: <cyc_achieved>)
      Source: <source_file>
      Input: docs/brain/<epic_id>/ticket-X-verification.md (and all prior artifacts)
      Jane Street KB Results: <paste KB results here>
      Task: Run Phase 6 (Final Review).
        1. Read all phase artifacts: 00-hotspots.md through ticket-X-verification.md
        2. Use jcodemunch-mcp: get_repo_health, get_hotspots (confirm this method no longer a hotspot)
        3. Use sequential-thinking to write a complete completion narrative:
           - What was refactored
           - Before/after CYC
           - Tests written
           - Jane Street patterns applied
           - DNA compliance confirmed
        4. Write docs/brain/<epic_id>/05-completion-report.md
        5. Update manifest.json: all phases status=completed, wave=7, final_cyc=<cyc_achieved>
        6. Run final complexity check: python scripts/complexity_audit.py | grep <method_name>
        Return: { status, output_path, final_cyc, wave_ready: true|false }

STEP 3 — COMPLETION VERIFICATION LOOP (wave_ready=true AND final_cyc<=8 for all 161):
  lamport_append(event_type="phase_6_kb_query_complete", phase="6", ..., status="success")
  For each success: lamport_append(event_type="phase_6_epic_complete", phase="6", ..., status="success", epic_id=<epic_id>)
  For failure: lamport_append(event_type="phase_6_epic_failed", ..., status="retry") → re-spawn up to 3 rounds.
  After 3 rounds: lamport_append(event_type="phase_6_hard_failure", ...) → HALT.

STEP 4 — WAVE-LEVEL FINAL SCAN (Phase 6 Orchestrator runs directly — NOT via workers):
  python scripts/complexity_audit.py > /tmp/wave7_final_audit.txt
  remaining=$(grep -c "CYC > 8" /tmp/wave7_final_audit.txt || echo 0)
  If remaining > 0:
    lamport_append(event_type="wave_7_regression_detected", ..., status="hard_failure", note="N methods still >8")
    HALT. Escalate to Tier 1 — do NOT write wave_7_complete.
  git diff --stat src/  → confirm only target methods modified.

STEP 5 — LOG WAVE COMPLETE (terminal event — only if remaining==0):
  lamport_append(event_type="phase_6_orchestrator_complete", phase="6", tier="phase_orch", status="complete")
  lamport_append(event_type="wave_7_complete", phase="6", tier="phase_orch", status="complete",
                 note="161/161 methods CYC<=8. Wave 7 done.")

REPORT BACK TO TIER 1 (WAVE COMPLETE):
  {
    "phase": "6",
    "wave": "7",
    "status": "WAVE_COMPLETE",
    "completed": 161,
    "failed": 0,
    "final_cyc_max": 8,
    "methods_above_8_remaining": 0,
    "lamport_wave_complete_clock": <clock of wave_7_complete event>,
    "wave_7_final_audit_path": "/tmp/wave7_final_audit.txt",
    "kb_queries_run": ["testing strategies coverage", "final audit complexity Jane Street"]
  }
```

---

## Top-Level Orchestrator Protocol (Tier 1 — YOUR SESSION)

```
You are the Wave 7 Top-Level Orchestrator.
Mode: autonomous-refactor

EXECUTION ORDER (strictly sequential — never skip, never parallelize phases):

Step 1: Read docs/brain/wave7-epic-list.json to get all 161 epic IDs + metadata.
Step 2: Create .lamport/wave7/event_log.jsonl (if not exists).
Step 3: Spawn Phase Orchestrators ONE AT A TIME in this order:
  a. Spawn Phase 0 Orchestrator (Template 1 above)
     WAIT for "VERIFIED_COMPLETE" before continuing.
  b. Spawn Phase 1 Orchestrator (Template 2 above)
     WAIT for "VERIFIED_COMPLETE" before continuing.
  c. Spawn Phase 1.5 Orchestrator (Template 3 above)
     WAIT for "VERIFIED_COMPLETE" before continuing.
  d. Spawn Phase 2 Orchestrator (Template 4 above)
     WAIT for "VERIFIED_COMPLETE" before continuing.
  e. Spawn Phase 3 Orchestrator (Template 5 above)
     WAIT for "VERIFIED_COMPLETE" before continuing.
  f. Spawn Phase 4 Orchestrator (Template 6 above)
     WAIT for "VERIFIED_COMPLETE" before continuing.
  g. Spawn Phase 4.5 Orchestrator (Template 7 above)
     WAIT for "VERIFIED_COMPLETE" before continuing.
  h. Spawn Phase 5 Orchestrator (Template 8 above)
     WAIT for "VERIFIED_COMPLETE" before continuing.
  i. Spawn Phase 5.V Orchestrator (Template 9 above)
     WAIT for "VERIFIED_COMPLETE" before continuing.
  j. Spawn Phase 6 Orchestrator (Template 10 above)
     WAIT for "WAVE_COMPLETE" report.
Step 4: Log wave_7_complete to .lamport/wave7/event_log.jsonl
Step 5: Report Wave 7 complete: 161/161 methods now CYC <= 8.

HARD FAILURE HANDLING:
  If any Phase Orchestrator returns HARD_FAILURE:
    - Log to .lamport/wave7/event_log.jsonl
    - Write incident report to docs/brain/wave7-incident-report.md
    - List stuck epics with failure analysis
    - Escalate to Director for manual resolution
    - After resolution, re-spawn Phase Orchestrator for the stuck epics ONLY
    - Do NOT restart the entire phase
```

---

*Document: Phase Orchestrator Templates V1.0*
*Architecture: Bob IDE V2 — 3-Tier Subagent Model*
*Protocol: V12.28*
