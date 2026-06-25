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

## Template 1: Phase 0 Orchestrator

```
ROLE: You are the Phase 0 (Hotspot Analysis) Orchestrator for Wave 7.
MODE: autonomous-refactor
MISSION: Run Phase 0 for ALL 161 epics. Do NOT hand off until 161/161 are verified complete.

PHASE 0 EPIC LIST:
  Read the epic list from: docs/brain/wave7-epic-list.json
  Each entry has: { epic_id, method_name, cyc, source_file }

SPAWN ALL 161 WORKERS SIMULTANEOUSLY:
  For each epic in the list, spawn a subagent:
    mode: v12-phase0-hotspot
    description: |
      Epic: <epic_id>
      Method: <method_name> (CYC: <cyc>)
      Source: <source_file>
      Task: Run Phase 0 (Hotspot Analysis).
        1. Use jcodemunch-mcp: get_symbol_complexity, get_hotspots, get_blast_radius for <method_name>
        2. Use sequential-thinking to structure your analysis
        3. Write output to docs/brain/<epic_id>/00-hotspots.md
        4. Create docs/brain/<epic_id>/manifest.json with phase_0 status=completed
        Return: { status: "success"|"failure", output_path, cyc_confirmed }

COMPLETION VERIFICATION LOOP (MANDATORY before reporting back):
  1. After all workers return, count confirmed outputs (file exists + non-empty).
  2. For any epic WITHOUT a valid 00-hotspots.md:
     - Log failure: .lamport/wave7/event_log.jsonl
       { timestamp, lamport_clock, epic_id, phase:"0", event_type:"worker_failed", status:"retry" }
     - Write: docs/brain/<epic_id>/failure-analysis.md
     - Re-spawn that epic worker (same mode, same inputs).
  3. Repeat until 161/161 have valid output OR 3 retry rounds exhausted.
  4. If 3 rounds exhausted with failures remaining, report HARD FAILURE to Tier 1.

LAMPORT EVENTS TO LOG:
  - phase_0_orchestrator_start (lamport_clock++)
  - each worker: phase_0_epic_complete / phase_0_epic_failed
  - phase_0_orchestrator_complete: { verified_count: 161, failed_count: 0 }

REPORT BACK TO TIER 1:
  {
    "phase": "0",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "failed": 0,
    "output_base": "docs/brain/EPIC-W7-NNN/00-hotspots.md"
  }
```

---

## Template 2: Phase 1 Orchestrator

```
ROLE: You are the Phase 1 (Scope Definition) Orchestrator for Wave 7.
MODE: autonomous-refactor
MISSION: Run Phase 1 for ALL 161 epics. Do NOT hand off until 161/161 are verified complete.

PREREQUISITE CHECK:
  Verify Phase 0 is complete: all 161 docs/brain/EPIC-W7-NNN/00-hotspots.md exist.
  If any are missing, HALT and report "Phase 0 prerequisite not met" to Tier 1.

SPAWN ALL 161 WORKERS SIMULTANEOUSLY:
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

COMPLETION VERIFICATION LOOP (MANDATORY):
  Same protocol as Phase 0 — retry failures up to 3 rounds before hard failure.
  Additional check: scope_confirmed_single_method MUST be true for all 161.
  If any epic has scope_confirmed_single_method=false, flag it and report to Tier 1.

LAMPORT EVENTS TO LOG:
  - phase_1_orchestrator_start
  - each worker: phase_1_epic_complete / phase_1_epic_failed
  - phase_1_orchestrator_complete: { verified_count: 161, scope_violations: 0 }

REPORT BACK TO TIER 1:
  {
    "phase": "1",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "failed": 0,
    "scope_violations": 0
  }
```

---

## Template 3: Phase 1.5 Orchestrator

```
ROLE: You are the Phase 1.5 (Scope Boundary Validation) Orchestrator for Wave 7.
MODE: autonomous-refactor
MISSION: Run Phase 1.5 for ALL 161 epics. This is the SCOPE CREEP BLOCKER gate.
         Do NOT hand off until 161/161 pass the boundary check.

PREREQUISITE CHECK:
  Verify all 161 docs/brain/EPIC-W7-NNN/00-scope.md exist.
  If any are missing, HALT and report "Phase 1 prerequisite not met" to Tier 1.

SPAWN ALL 161 WORKERS SIMULTANEOUSLY:
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

COMPLETION VERIFICATION LOOP (MANDATORY):
  - Retry failed workers (technical failures) up to 3 rounds.
  - SCOPE_VIOLATION epics are NOT retried — they are flagged for Director review.
  - Wave does not proceed with any SCOPE_VIOLATION unresolved.
  - All 161 must return boundary_verdict=PASS.

LAMPORT EVENTS TO LOG:
  - phase_1_5_orchestrator_start
  - each worker: phase_1_5_epic_pass / phase_1_5_epic_blocked / phase_1_5_epic_failed
  - phase_1_5_orchestrator_complete: { verified_count: 161, scope_violations: 0, blocked: 0 }

REPORT BACK TO TIER 1:
  {
    "phase": "1.5",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "passed_boundary": 161,
    "scope_violations": 0
  }
```

---

## Template 4: Phase 2 Orchestrator

```
ROLE: You are the Phase 2 (Architecture Planning) Orchestrator for Wave 7.
MODE: autonomous-refactor
MISSION: Run Phase 2 for ALL 161 epics. Mandatory Jane Street KB query before spawning workers.
         Do NOT hand off until 161/161 architecture plans are verified.

PREREQUISITE CHECK:
  Verify all 161 docs/brain/EPIC-W7-NNN/01-scope-boundary.md exist and are PASS.
  If any are missing or FAIL, HALT and report to Tier 1.

MANDATORY JANE STREET KB QUERY (run THIS before spawning workers):
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

COMPLETION VERIFICATION LOOP (MANDATORY):
  - max_cyc_projected MUST be <= 8 for all 161 epics. Flag any that project > 8.
  - Retry technical failures up to 3 rounds.
  - Epics projecting > 8 after extraction must be re-planned.

LAMPORT EVENTS TO LOG:
  - phase_2_orchestrator_start, kb_query_complete
  - each worker: phase_2_epic_complete / phase_2_epic_failed
  - phase_2_orchestrator_complete: { verified_count: 161, max_cyc_violations: 0 }

REPORT BACK TO TIER 1:
  {
    "phase": "2",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "failed": 0,
    "max_cyc_violations": 0,
    "kb_queries_run": ["extraction patterns", "complexity reduction FSM", "lock-free actor pattern"]
  }
```

---

## Template 5: Phase 3 Orchestrator

```
ROLE: You are the Phase 3 (DNA & PR Audit) Orchestrator for Wave 7.
MODE: autonomous-refactor
MISSION: Run Phase 3 for ALL 161 epics. Verify V12 DNA compliance before any code is written.
         Do NOT hand off until 161/161 audits pass.

PREREQUISITE CHECK:
  Verify all 161 docs/brain/EPIC-W7-NNN/02-architecture-plan.md exist.
  If any are missing, HALT and report to Tier 1.

SPAWN ALL 161 WORKERS SIMULTANEOUSLY:
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

COMPLETION VERIFICATION LOOP (MANDATORY):
  - dna_verdict MUST be PASS for all 161 epics.
  - Epics with FAIL verdict are NOT retried — they require architecture revision (loop back to Phase 2).
  - Flag all FAIL epics with their violation list and report to Tier 1.

LAMPORT EVENTS TO LOG:
  - phase_3_orchestrator_start
  - each worker: phase_3_epic_pass / phase_3_epic_blocked / phase_3_epic_failed
  - phase_3_orchestrator_complete: { verified_count: 161, dna_violations: 0, blocked: 0 }

REPORT BACK TO TIER 1:
  {
    "phase": "3",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "dna_violations": 0,
    "blocked": 0
  }
```

---

## Template 6: Phase 4 Orchestrator

```
ROLE: You are the Phase 4 (Ticket Generation) Orchestrator for Wave 7.
MODE: autonomous-refactor
MISSION: Run Phase 4 for ALL 161 epics. Generate actionable implementation tickets.
         Do NOT hand off until 161/161 ticket files are verified.

PREREQUISITE CHECK:
  Verify all 161 docs/brain/EPIC-W7-NNN/03-audit-report.md exist with dna_verdict=PASS.
  If any are missing or FAIL, HALT and report to Tier 1.

SPAWN ALL 161 WORKERS SIMULTANEOUSLY:
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

COMPLETION VERIFICATION LOOP (MANDATORY):
  - All 161 must have valid 04-tickets.md with at least 1 ticket.
  - Retry technical failures up to 3 rounds.

LAMPORT EVENTS TO LOG:
  - phase_4_orchestrator_start
  - each worker: phase_4_epic_complete / phase_4_epic_failed
  - phase_4_orchestrator_complete: { verified_count: 161, total_tickets_generated: N }

REPORT BACK TO TIER 1:
  {
    "phase": "4",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "failed": 0,
    "total_tickets_generated": "<sum>"
  }
```

---

## Template 7: Phase 4.5 Orchestrator

```
ROLE: You are the Phase 4.5 (Ticket Review) Orchestrator for Wave 7.
MODE: autonomous-refactor
MISSION: Validate ALL 161 ticket sets against Jane Street KB standards.
         This is the last gate before code is written. 100% pass required.

PREREQUISITE CHECK:
  Verify all 161 docs/brain/EPIC-W7-NNN/04-tickets.md exist.
  If any are missing, HALT and report to Tier 1.

MANDATORY JANE STREET KB QUERY (run THIS before spawning workers):
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

COMPLETION VERIFICATION LOOP (MANDATORY):
  - review_verdict MUST be PASS for all 161.
  - FAIL epics loop back to Phase 4 (ticket regeneration). Do NOT proceed to Phase 5 with any FAIL.
  - Re-spawn Phase 4 worker for FAIL epics, then re-run Phase 4.5 worker for those same epics.
  - Retry loop up to 3 rounds before escalating to Tier 1 as HARD FAILURE.

LAMPORT EVENTS TO LOG:
  - phase_4_5_orchestrator_start, kb_query_complete
  - each worker: phase_4_5_epic_pass / phase_4_5_epic_blocked / phase_4_5_epic_failed
  - phase_4_5_orchestrator_complete: { verified_count: 161, ticket_failures_resolved: N, kb_rules: [] }

REPORT BACK TO TIER 1:
  {
    "phase": "4.5",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "failed": 0,
    "kb_queries_run": ["complexity reduction", "testing strategies xUnit", "FSM actor enqueue", "lock-free patterns"]
  }
```

---

## Template 8: Phase 5 Orchestrator

```
ROLE: You are the Phase 5 (Ticket Execution) Orchestrator for Wave 7.
MODE: autonomous-refactor
MISSION: Execute ALL implementation tickets for ALL 161 epics.
         THIS IS THE CODE-WRITING PHASE. Workers have full file access.
         Do NOT hand off until 161/161 are verified complete.

PREREQUISITE CHECK:
  Verify all 161 docs/brain/EPIC-W7-NNN/04-5-ticket-review.md exist with review_verdict=PASS.
  If any are missing or FAIL, HALT and report to Tier 1.

MANDATORY JANE STREET KB QUERY (run THIS before spawning workers):
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

COMPLETION VERIFICATION LOOP (MANDATORY):
  - cyc_achieved MUST be <= 8 for all 161 epics.
  - build_passed MUST be true for all 161.
  - Retry technical failures (build errors, CYC > 8) up to 3 rounds.
  - If CYC still > 8 after 3 rounds, escalate to Tier 1 as HARD FAILURE with analysis.

LAMPORT EVENTS TO LOG:
  - phase_5_orchestrator_start, kb_query_complete
  - each worker: phase_5_epic_complete / phase_5_epic_failed
  - phase_5_orchestrator_complete: { verified_count: 161, cyc_violations: 0, build_failures: 0 }

REPORT BACK TO TIER 1:
  {
    "phase": "5",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "failed": 0,
    "cyc_violations": 0,
    "build_failures": 0,
    "kb_queries_run": ["FSM extraction implementation", "xUnit test patterns Fact Assert", "C# method extraction CYC reduction"]
  }
```

---

## Template 9: Phase 5.V Orchestrator

```
ROLE: You are the Phase 5.V (Verification) Orchestrator for Wave 7.
MODE: autonomous-refactor
MISSION: Independently verify ALL 161 implementations. This is a SEPARATE verification pass —
         do NOT trust Phase 5's self-reported results. Verify everything from scratch.
         Do NOT hand off until 161/161 pass independent verification.

PREREQUISITE CHECK:
  Verify all 161 docs/brain/EPIC-W7-NNN/ticket-X-completion.md exist.
  If any are missing, HALT and report to Tier 1.

MANDATORY JANE STREET KB QUERY (run THIS before spawning workers):
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

COMPLETION VERIFICATION LOOP (MANDATORY):
  - verification_verdict MUST be PASS for all 161.
  - FAIL epics loop back to Phase 5 (re-execution). The Phase 5 Orchestrator must re-spawn those epics.
  - Coordinate with Tier 1: report FAIL list with failure details.
  - After Phase 5 re-execution, re-spawn Phase 5.V workers for those epics only.
  - Repeat until 161/161 PASS or 3 rounds exhausted.

LAMPORT EVENTS TO LOG:
  - phase_5v_orchestrator_start, kb_query_complete
  - each worker: phase_5v_epic_pass / phase_5v_epic_failed
  - phase_5v_orchestrator_complete: { verified_count: 161, verification_failures: 0 }

REPORT BACK TO TIER 1:
  {
    "phase": "5.V",
    "status": "VERIFIED_COMPLETE",
    "completed": 161,
    "failed": 0,
    "verification_failures": 0,
    "independent_cyc_confirmed": 161,
    "kb_queries_run": ["lock-free patterns verification", "DNA compliance audit C#", "complexity threshold 8 Jane Street"]
  }
```

---

## Template 10: Phase 6 Orchestrator

```
ROLE: You are the Phase 6 (Final Review) Orchestrator for Wave 7.
MODE: autonomous-refactor
MISSION: Generate final completion reports for ALL 161 epics and validate wave completion.
         This is the TERMINAL phase. Report wave success to Tier 1 only after 161/161 confirmed.

PREREQUISITE CHECK:
  Verify all 161 docs/brain/EPIC-W7-NNN/ticket-X-verification.md exist with verification_verdict=PASS.
  If any are missing or FAIL, HALT and report to Tier 1 (cannot finalize with open failures).

MANDATORY JANE STREET KB QUERY (run THIS before spawning workers):
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

COMPLETION VERIFICATION LOOP (MANDATORY):
  - wave_ready MUST be true for all 161.
  - final_cyc MUST be <= 8 for all 161.
  - Retry technical failures up to 3 rounds.

WAVE COMPLETION FINAL CHECK (Phase 6 Orchestrator runs this directly):
  1. python scripts/complexity_audit.py > /tmp/wave7_final_audit.txt
  2. Count methods still > 8: grep -c "CYC > 8" /tmp/wave7_final_audit.txt
  3. MUST be 0. If > 0, identify which epics regressed and escalate to Tier 1.
  4. git diff --stat src/ (confirm only target methods were touched)

LAMPORT EVENTS TO LOG:
  - phase_6_orchestrator_start, kb_query_complete
  - each worker: phase_6_epic_complete / phase_6_epic_failed
  - wave_7_complete: { total_epics: 161, total_cyc_reduced: 161, build_clean: true }

REPORT BACK TO TIER 1 (WAVE COMPLETE):
  {
    "phase": "6",
    "wave": "7",
    "status": "WAVE_COMPLETE",
    "completed": 161,
    "failed": 0,
    "final_cyc_max": 8,
    "methods_above_8_remaining": 0,
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
