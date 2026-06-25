# Phase Orchestrator Templates — Wave 7

**Version**: V2.5 (Bob IDE V2 — 3-Tier Architecture)
**Used By**: All 10 wave-orch-phaseN modes (Tier 2 Phase Orchestrators)
**Purpose**: Exact worker description payloads and verification protocols per phase

---

## How To Use These Templates

Each Phase Orchestrator (`wave-orch-phaseN`) spawns 161 workers using `spawn_subagent`.
The worker `description` parameter must follow the template for that phase exactly.
Replace `<EPIC_ID>`, `<METHOD_NAME>`, `<CYC>`, `<SOURCE_FILE>` from `docs/brain/wave7-epic-list.json`.

**epic list access**: `docs/brain/wave7-epic-list.json` is a flat JSON array.
Access entries as `data[0]`, `data[1]`, ..., `data[160]` — NOT `data["epics"]`.

---

## Adaptive Batch Size Formula (V2.5 — ALL 10 orchestrators)

After pilot passes, compute batch size before spawning remaining workers:

```
BATCH_SIZE = max(1, min(50, floor(BALANCE * 0.85 / PILOT_COST)))
```

- Read `BALANCE` from `.lamport/wave7/bobcoin_tracker.json` → `balance_estimate`
- Read `PILOT_COST` from agent tracking block in pilot output artifact
- Cap = 50 (never spawn more than 50 per batch)
- Floor = 1 (always spawn at least 1)
- 15% safety buffer (never spend last 15% of balance)
- If balance < 15% of original → pause → report `BOBCOIN_PAUSE: N epics remaining` to Tier 1
- After Director reloads → update `balance_estimate` in `bobcoin_tracker.json` → resume

---

## Universal Pilot Compliance Audit (V2.5 — ALL 10 orchestrators)

Before batching remaining 160 workers, EVERY Phase Orchestrator MUST:

1. Spawn EPIC-W7-001 (or first available epic) as a pilot worker
2. Wait for result
3. Run 7-check audit:

| Check | Verification | Hard Fail? |
|-------|-------------|------------|
| 1 | Phase-specific jcodemunch-mcp tools were called | YES |
| 2 | sequential-thinking MCP was used | YES |
| 3 | Output artifact exists AND size > 200 bytes | YES |
| 4 | `manifest.json` updated: this phase's status = completed | YES |
| 5 | Agent Tracking block present (Agent Name, Bobcoins Used, Execution Time) | SOFT WARNING only |
| 6 | Phase-specific success criterion met (see per-phase section below) | YES |
| 7 | DNA violations (Phase 5 ONLY: no NUnit/MSTest, no lock(), UTF-8 encoded) | YES (Phase 5 only) |

**If any HARD check fails**:
- Log `pilot_failed` to `.lamport/wave7/event_log.jsonl`
- Write `docs/brain/EPIC-W7-001/failure-analysis.md` (or first epic used)
- Report `PILOT_FAILURE` to Tier 1
- HALT — do NOT spawn remaining workers

---

## Template 1 — Phase 0: Hotspot Analysis

**Phase Orchestrator Mode**: `wave-orch-phase0`
**Worker Mode**: `v12-phase0-hotspot`
**Pilot Check 6 criterion**: output `00-hotspots.md` exists and is non-empty, `cyc_confirmed` field present in return value

### Worker Description (spawn_subagent)

```
Epic: <EPIC_ID>
Method: <METHOD_NAME> (CYC: <CYC>)
Source: <SOURCE_FILE>
Wave: 7
Phase: 0 — Hotspot Analysis

YOUR TASK:
1. Use mcp__jcodemunch-mcp__search_symbols to locate method <METHOD_NAME> in <SOURCE_FILE>
2. Use mcp__jcodemunch-mcp__get_symbol_complexity to get current CYC score
3. Use mcp__jcodemunch-mcp__get_blast_radius to identify impact scope
4. Use mcp__jcodemunch-mcp__get_hotspots to identify related complexity hotspots
5. Use mcp__sequential-thinking__sequentialthinking to structure your analysis
6. Write docs/brain/<EPIC_ID>/00-hotspots.md with:
   - Method name, CYC, file path
   - Blast radius summary
   - Top 3 complexity drivers (if/switch/loop nesting)
   - Recommended extraction count
   - Agent Tracking block (Agent Name: v12-phase0-hotspot, Bobcoins Used: [amount], Execution Time: [duration])
7. Update docs/brain/<EPIC_ID>/manifest.json:
   - Set phases.phase_0.status = "completed"
   - Set phases.phase_0.output = "00-hotspots.md"
8. Return: { "status": "success", "output_path": "docs/brain/<EPIC_ID>/00-hotspots.md", "cyc_confirmed": <CYC> }
```

---

## Template 2 — Phase 1: Scope Definition

**Phase Orchestrator Mode**: `wave-orch-phase1`
**Worker Mode**: `v12-phase1-scope`
**Pilot Check 6 criterion**: return value has `scope_confirmed_single_method: true`

### Worker Description (spawn_subagent)

```
Epic: <EPIC_ID>
Method: <METHOD_NAME> (CYC: <CYC>)
Source: <SOURCE_FILE>
Wave: 7
Phase: 1 — Scope Definition
Input: docs/brain/<EPIC_ID>/00-hotspots.md

YOUR TASK:
1. Read docs/brain/<EPIC_ID>/00-hotspots.md (Phase 0 output)
2. Use mcp__jcodemunch-mcp__get_file_outline on <SOURCE_FILE>
3. Use mcp__jcodemunch-mcp__find_references for <METHOD_NAME>
4. Use mcp__jcodemunch-mcp__get_dependency_graph on <SOURCE_FILE>
5. Use mcp__sequential-thinking__sequentialthinking to define scope boundaries
6. SCOPE RULE: ONE method only. No other methods in scope.
7. Write docs/brain/<EPIC_ID>/00-scope.md with:
   - Single method in scope: <METHOD_NAME>
   - Current CYC: <CYC>, Target CYC: <= 8
   - File: <SOURCE_FILE>
   - Callers count (from find_references)
   - Scope boundary statement: "Only <METHOD_NAME> and its new extracted helper methods"
   - Agent Tracking block (Agent Name: v12-phase1-scope, Bobcoins Used: [amount], Execution Time: [duration])
8. Update docs/brain/<EPIC_ID>/manifest.json phases.phase_1.status = "completed"
9. Return: { "status": "success", "output_path": "docs/brain/<EPIC_ID>/00-scope.md", "scope_confirmed_single_method": true }
```

---

## Template 3 — Phase 1.5: Scope Boundary Validation

**Phase Orchestrator Mode**: `wave-orch-phase1-5`
**Worker Mode**: `v12-phase1-5-boundary`
**Pilot Check 6 criterion**: return value has `boundary_verdict: "PASS"`

### Worker Description (spawn_subagent)

```
Epic: <EPIC_ID>
Method: <METHOD_NAME> (CYC: <CYC>)
Source: <SOURCE_FILE>
Wave: 7
Phase: 1.5 — Scope Boundary Validation (V12.23 No Scope Creep Protocol)
Input: docs/brain/<EPIC_ID>/00-scope.md

YOUR TASK:
1. Read docs/brain/<EPIC_ID>/00-scope.md (Phase 1 output)
2. Use mcp__jcodemunch-mcp__get_symbol_source for <METHOD_NAME>
3. Use mcp__jcodemunch-mcp__get_blast_radius — confirm blast limited to target + new helpers
4. Use mcp__jcodemunch-mcp__find_references — confirm no unintended callsite changes
5. Use mcp__sequential-thinking__sequentialthinking to validate no scope creep
6. BOUNDARY RULE: If scope exceeds single method + its new helpers → verdict FAIL
7. Write docs/brain/<EPIC_ID>/01-scope-boundary.md with:
   - boundary_verdict: PASS or FAIL
   - Reason (if FAIL: describe the scope violation)
   - Confirmed scope: "<METHOD_NAME> + N new helper methods (to be named in Phase 2)"
   - Agent Tracking block (Agent Name: v12-phase1-5-boundary, Bobcoins Used: [amount], Execution Time: [duration])
8. Update docs/brain/<EPIC_ID>/manifest.json phases.phase_1_5.status = "completed"
9. Return: { "status": "success", "output_path": "docs/brain/<EPIC_ID>/01-scope-boundary.md", "boundary_verdict": "PASS" }
```

---

## Template 4 — Phase 2: Architecture Planning

**Phase Orchestrator Mode**: `wave-orch-phase2`
**Worker Mode**: `v12-phase2-architecture`
**Pilot Check 6 criterion**: return value has `max_cyc_projected <= 8`
**Jane Street KB**: Phase Orchestrator runs BEFORE spawning workers:
- `python scripts/query_kb.py "extraction patterns"`
- `python scripts/query_kb.py "complexity reduction FSM"`
- `python scripts/query_kb.py "lock-free actor pattern"`

### Worker Description (spawn_subagent)

```
Epic: <EPIC_ID>
Method: <METHOD_NAME> (CYC: <CYC>)
Source: <SOURCE_FILE>
Wave: 7
Phase: 2 — Architecture Planning
Input: docs/brain/<EPIC_ID>/01-scope-boundary.md

Jane Street KB Results (apply these patterns):
<INSERT KB QUERY RESULTS HERE>

YOUR TASK:
1. Read docs/brain/<EPIC_ID>/01-scope-boundary.md
2. Use mcp__jcodemunch-mcp__get_context_bundle for <METHOD_NAME>
3. Use mcp__jcodemunch-mcp__get_call_hierarchy for <METHOD_NAME>
4. Use mcp__jcodemunch-mcp__get_dependency_graph on the method
5. Use mcp__jcodemunch-mcp__get_extraction_candidates for <METHOD_NAME>
6. Use mcp__sequential-thinking__sequentialthinking to design extraction plan
7. DESIGN RULE: Each extracted helper method MUST have projected CYC <= 8.
   Remaining parent method MUST also have projected CYC <= 8.
8. Write docs/brain/<EPIC_ID>/02-architecture-plan.md with:
   - Original method CYC: <CYC>
   - Extraction plan: list each new helper method name + what it does + projected CYC
   - Parent method after extraction: projected CYC
   - max_cyc_projected: <max of all projected CYCs> (MUST be <= 8)
   - Jane Street alignment notes
   - Agent Tracking block (Agent Name: v12-phase2-architecture, Bobcoins Used: [amount], Execution Time: [duration])
9. Update docs/brain/<EPIC_ID>/manifest.json phases.phase_2.status = "completed"
10. Return: { "status": "success", "output_path": "docs/brain/<EPIC_ID>/02-architecture-plan.md", "extraction_count": N, "max_cyc_projected": M }
```

---

## Template 5 — Phase 3: DNA Audit

**Phase Orchestrator Mode**: `wave-orch-phase3`
**Worker Mode**: `v12-phase3-audit`
**Pilot Check 6 criterion**: return value has `dna_verdict: "PASS"`

### Worker Description (spawn_subagent)

```
Epic: <EPIC_ID>
Method: <METHOD_NAME> (CYC: <CYC>)
Source: <SOURCE_FILE>
Wave: 7
Phase: 3 — DNA & PR Audit
Input: docs/brain/<EPIC_ID>/02-architecture-plan.md

YOUR TASK:
1. Read docs/brain/<EPIC_ID>/02-architecture-plan.md
2. Use mcp__jcodemunch-mcp__search_ast to check for "lock(" patterns in <SOURCE_FILE>
3. Use mcp__jcodemunch-mcp__get_layer_violations for architectural compliance
4. Use mcp__jcodemunch-mcp__get_dependency_cycles to verify no circular deps introduced
5. Use mcp__sequential-thinking__sequentialthinking to validate DNA compliance
6. DNA CHECKS (all must pass):
   - Zero lock() blocks planned
   - ASCII-only string literals (no Unicode in literals)
   - UTF-8 source files (no BOM)
   - No scope creep beyond target method
   - xUnit tests ([Fact], Assert.Equal()) planned — NEVER NUnit/MSTest
   - No max_cyc_projected > 8
7. Write docs/brain/<EPIC_ID>/03-audit-report.md with:
   - dna_verdict: PASS or FAIL
   - Each DNA check result (pass/fail)
   - violations: [] (empty list if PASS)
   - Agent Tracking block (Agent Name: v12-phase3-audit, Bobcoins Used: [amount], Execution Time: [duration])
8. Update docs/brain/<EPIC_ID>/manifest.json phases.phase_3.status = "completed"
9. Return: { "status": "success", "output_path": "docs/brain/<EPIC_ID>/03-audit-report.md", "dna_verdict": "PASS", "violations": [] }
```

---

## Template 6 — Phase 4: Ticket Generation

**Phase Orchestrator Mode**: `wave-orch-phase4`
**Worker Mode**: `v12-phase4-tickets`
**Pilot Check 6 criterion**: return value has `ticket_count >= 1`

### Worker Description (spawn_subagent)

```
Epic: <EPIC_ID>
Method: <METHOD_NAME> (CYC: <CYC>)
Source: <SOURCE_FILE>
Wave: 7
Phase: 4 — Ticket Generation
Inputs: docs/brain/<EPIC_ID>/02-architecture-plan.md + docs/brain/<EPIC_ID>/03-audit-report.md

YOUR TASK:
1. Read docs/brain/<EPIC_ID>/02-architecture-plan.md and 03-audit-report.md
2. Use mcp__jcodemunch-mcp__get_symbol_complexity for <METHOD_NAME>
3. Use mcp__jcodemunch-mcp__get_extraction_candidates for detailed extraction targets
4. Use mcp__sequential-thinking__sequentialthinking to decompose into tickets
5. Write docs/brain/<EPIC_ID>/04-tickets.md with:
   - One ticket per extraction (minimum 1 ticket)
   - Each ticket: ticket ID, target helper method name, what code to move, expected CYC reduction
   - Final ticket: "Verify parent <METHOD_NAME> CYC <= 8 after all extractions"
   - ticket_count: N
   - Agent Tracking block (Agent Name: v12-phase4-tickets, Bobcoins Used: [amount], Execution Time: [duration])
6. Update docs/brain/<EPIC_ID>/manifest.json phases.phase_4.status = "completed"
7. Return: { "status": "success", "output_path": "docs/brain/<EPIC_ID>/04-tickets.md", "ticket_count": N }
```

---

## Template 7 — Phase 4.5: Ticket Review (Jane Street Gate)

**Phase Orchestrator Mode**: `wave-orch-phase4-5`
**Worker Mode**: `v12-phase4-5-review`
**Pilot Check 6 criterion**: return value has `review_verdict: "PASS"`
**Jane Street KB**: Phase Orchestrator runs BEFORE spawning workers:
- `python scripts/query_kb.py "complexity reduction"`
- `python scripts/query_kb.py "testing strategies xUnit"`
- `python scripts/query_kb.py "FSM actor enqueue"`
- `python scripts/query_kb.py "lock-free patterns"`

### Worker Description (spawn_subagent)

```
Epic: <EPIC_ID>
Method: <METHOD_NAME> (CYC: <CYC>)
Source: <SOURCE_FILE>
Wave: 7
Phase: 4.5 — Ticket Review (Jane Street Validation Gate)
Input: docs/brain/<EPIC_ID>/04-tickets.md

Jane Street KB Results (validate against these rules):
<INSERT KB QUERY RESULTS HERE>

YOUR TASK:
1. Read docs/brain/<EPIC_ID>/04-tickets.md
2. Use mcp__sequential-thinking__sequentialthinking to validate each ticket
3. Validate each ticket against Jane Street KB rules:
   - Each extraction reduces CYC to <= 8 (no exceptions)
   - No lock() patterns introduced
   - xUnit tests ([Fact], Assert.Equal()) required for each new helper
   - ASCII-only string literals in all new code
   - Single concern per ticket (no mixed-concern extractions)
4. Write docs/brain/<EPIC_ID>/04-5-ticket-review.md with:
   - review_verdict: PASS or FAIL
   - Per-ticket validation result
   - failed_tickets: [] (empty if PASS)
   - Jane Street alignment confirmation
   - Agent Tracking block (Agent Name: v12-phase4-5-review, Bobcoins Used: [amount], Execution Time: [duration])
5. Update docs/brain/<EPIC_ID>/manifest.json phases.phase_4_5.status = "completed"
6. Return: { "status": "success", "output_path": "docs/brain/<EPIC_ID>/04-5-ticket-review.md", "review_verdict": "PASS", "failed_tickets": [] }
```

---

## Template 8 — Phase 5: Ticket Execution (Code Writing)

**Phase Orchestrator Mode**: `wave-orch-phase5`
**Worker Mode**: `v12-engineer`
**Pilot Check 6 criterion**: return has `cyc_achieved <= 8` AND `build_passed: true`
**Pilot Check 7 (HARD — DNA)**: no NUnit/MSTest, no lock(), UTF-8 confirmed
**Jane Street KB**: Phase Orchestrator runs BEFORE spawning workers:
- `python scripts/query_kb.py "FSM extraction implementation"`
- `python scripts/query_kb.py "xUnit test patterns Fact Assert"`
- `python scripts/query_kb.py "C# method extraction CYC reduction"`

### Worker Description (spawn_subagent)

```
Epic: <EPIC_ID>
Method: <METHOD_NAME> (CYC: <CYC>, Target: <= 8)
Source: <SOURCE_FILE>
Wave: 7
Phase: 5 — Ticket Execution (Code Writing)
Inputs: docs/brain/<EPIC_ID>/04-tickets.md + docs/brain/<EPIC_ID>/04-5-ticket-review.md

Jane Street KB Results (apply these patterns in implementation):
<INSERT KB QUERY RESULTS HERE>

DNA RULES (non-negotiable — violation = BLOCKER):
- xUnit ONLY: [Fact], Assert.Equal() — NEVER NUnit, NEVER MSTest
- UTF-8 source files (no BOM)
- Zero lock() blocks — use FSM/Actor Enqueue model
- ASCII-only string literals (no Unicode, no curly quotes)
- dotnet csharpier format src/ after every write
- SINGLE CONCERN: only modify <METHOD_NAME> + its new extracted helpers

YOUR TASK:
1. Read docs/brain/<EPIC_ID>/04-tickets.md and 04-5-ticket-review.md
2. Use mcp__jcodemunch-mcp__get_symbol_source for <METHOD_NAME>
3. Use mcp__jcodemunch-mcp__get_context_bundle for full method context
4. Use mcp__jcodemunch-mcp__plan_refactoring to validate extraction approach
5. Use mcp__sequential-thinking__sequentialthinking to plan implementation steps
6. Execute each ticket:
   a. Extract helper methods from <METHOD_NAME>
   b. Write xUnit tests for each extracted helper ([Fact], Assert.Equal())
   c. Run: dotnet csharpier format src/
   d. Run: dotnet build (MUST pass with zero errors)
   e. Run: python scripts/complexity_audit.py | grep "<METHOD_NAME>" (MUST show CYC <= 8)
7. Write docs/brain/<EPIC_ID>/ticket-1-completion.md with:
   - Original CYC: <CYC>
   - cyc_achieved: <new CYC> (MUST be <= 8)
   - build_passed: true
   - tests_written: N (count of [Fact] tests)
   - Extracted helper methods list
   - Agent Tracking block (Agent Name: v12-engineer, Bobcoins Used: [amount], Execution Time: [duration])
8. Update docs/brain/<EPIC_ID>/manifest.json phases.phase_5.status = "completed"
9. Return: { "status": "success", "cyc_achieved": M, "build_passed": true, "tests_written": N }
```

---

## Template 9 — Phase 5.V: Independent Verification

**Phase Orchestrator Mode**: `wave-orch-phase5v`
**Worker Mode**: `v12-phase5-v-verify`
**Pilot Check 6 criterion**: return has `verification_verdict: "PASS"`
**Jane Street KB**: Phase Orchestrator runs BEFORE spawning workers:
- `python scripts/query_kb.py "lock-free patterns verification"`
- `python scripts/query_kb.py "DNA compliance audit C#"`
- `python scripts/query_kb.py "complexity threshold 8 Jane Street"`

### Worker Description (spawn_subagent)

```
Epic: <EPIC_ID>
Method: <METHOD_NAME> (Original CYC: <CYC>, Claimed CYC Achieved: <CYC_ACHIEVED>)
Source: <SOURCE_FILE>
Wave: 7
Phase: 5.V — Independent Verification (do NOT trust Phase 5 self-reported results)
Input: docs/brain/<EPIC_ID>/ticket-1-completion.md

Jane Street KB Results:
<INSERT KB QUERY RESULTS HERE>

YOUR TASK (independent verification — verify everything from scratch):
1. Read docs/brain/<EPIC_ID>/ticket-1-completion.md
2. INDEPENDENT CHECK 1: Use mcp__jcodemunch-mcp__get_symbol_complexity(<METHOD_NAME>)
   → MUST return CYC <= 8 (not self-reported, actually measured)
3. INDEPENDENT CHECK 2: Use mcp__jcodemunch-mcp__get_changed_symbols()
   → MUST only show <METHOD_NAME> + helper methods (no unintended changes)
4. INDEPENDENT CHECK 3: Use mcp__jcodemunch-mcp__search_ast("lock(") on <SOURCE_FILE>
   → MUST return zero matches
5. INDEPENDENT CHECK 4: grep [Fact] in test files
   → MUST find at least 1 xUnit [Fact] test
6. INDEPENDENT CHECK 5: grep "TestFixture\|[Test]\|[TestCase]" in test files
   → MUST return zero (no NUnit/MSTest)
7. INDEPENDENT CHECK 6: execute_command: dotnet build
   → MUST pass with zero errors
8. INDEPENDENT CHECK 7: execute_command: python scripts/complexity_audit.py | grep "<METHOD_NAME>"
   → MUST show CYC <= 8
9. Use mcp__sequential-thinking__sequentialthinking to summarize verification result
10. Write docs/brain/<EPIC_ID>/ticket-1-verification.md with:
    - verification_verdict: PASS or FAIL
    - Each check result (pass/fail + measured value)
    - failures: [] (empty if all pass)
    - Agent Tracking block (Agent Name: v12-phase5-v-verify, Bobcoins Used: [amount], Execution Time: [duration])
11. Update docs/brain/<EPIC_ID>/manifest.json phases.phase_5_v.status = "completed"
12. Return: { "status": "success", "output_path": "docs/brain/<EPIC_ID>/ticket-1-verification.md", "verification_verdict": "PASS", "failures": [] }
```

---

## Template 10 — Phase 6: Final Review & Wave Completion

**Phase Orchestrator Mode**: `wave-orch-phase6`
**Worker Mode**: `v12-phase6-review`
**Pilot Check 6 criterion**: return has `wave_ready: true` AND `final_cyc <= 8`
**Jane Street KB**: Phase Orchestrator runs BEFORE spawning workers:
- `python scripts/query_kb.py "testing strategies coverage"`
- `python scripts/query_kb.py "final audit complexity Jane Street"`

**TERMINAL PHASE**: After all 161 workers complete, Phase 6 Orchestrator DIRECTLY runs:
```bash
python scripts/complexity_audit.py > /tmp/wave7_final_audit.txt
grep "CYC > 8" /tmp/wave7_final_audit.txt | wc -l  # MUST be 0
git diff --stat src/  # confirm only target methods touched
```

Only if count == 0: append `wave_7_complete` to Lamport log and report `WAVE_COMPLETE` to Tier 1.

### Worker Description (spawn_subagent)

```
Epic: <EPIC_ID>
Method: <METHOD_NAME> (Original CYC: <CYC>, Verified CYC: <VERIFIED_CYC>)
Source: <SOURCE_FILE>
Wave: 7
Phase: 6 — Final Review & Epic Completion
Input: docs/brain/<EPIC_ID>/ticket-1-verification.md (+ all prior artifacts)

Jane Street KB Results:
<INSERT KB QUERY RESULTS HERE>

YOUR TASK:
1. Read docs/brain/<EPIC_ID>/ticket-1-verification.md
2. Use mcp__jcodemunch-mcp__get_repo_health to confirm codebase health
3. Use mcp__jcodemunch-mcp__get_hotspots to confirm <METHOD_NAME> no longer a hotspot
4. Use mcp__sequential-thinking__sequentialthinking to write completion narrative
5. Verify all manifests updated correctly (phases 0 through 5.V all completed)
6. Write docs/brain/<EPIC_ID>/05-completion-report.md with:
   - Epic summary: <METHOD_NAME> CYC <CYC> → <VERIFIED_CYC>
   - All phase status summary
   - final_cyc: <VERIFIED_CYC> (MUST be <= 8)
   - wave_ready: true
   - Tests written count
   - Jane Street compliance statement
   - Agent Tracking block (Agent Name: v12-phase6-review, Bobcoins Used: [amount], Execution Time: [duration])
7. Update docs/brain/<EPIC_ID>/manifest.json:
   - phases.phase_6.status = "completed"
   - status = "complete"
   - wave = 7
8. Return: { "status": "success", "output_path": "docs/brain/<EPIC_ID>/05-completion-report.md", "final_cyc": M, "wave_ready": true }
```

---

## Lamport Event Sequence (All 10 Phases)

Each Phase Orchestrator appends to `.lamport/wave7/event_log.jsonl`:

```jsonl
// On start:
{"timestamp":"<ISO>","lamport_clock":<N>,"epic_id":"WAVE-7","phase":"<P>","tier":"phase_orch","event_type":"phase_<P>_orchestrator_start","status":"running"}

// After pilot passes:
{"timestamp":"<ISO>","lamport_clock":<N+1>,"epic_id":"EPIC-W7-001","phase":"<P>","tier":"phase_orch","event_type":"pilot_passed","status":"success","pilot_cost":<cost>,"batch_size":<bs>}

// After all 161 complete:
{"timestamp":"<ISO>","lamport_clock":<N+2>,"epic_id":"WAVE-7","phase":"<P>","tier":"phase_orch","event_type":"phase_<P>_orchestrator_complete","status":"complete","completed":161,"failed":0}
```

Phase 6 additionally appends:
```jsonl
{"timestamp":"<ISO>","lamport_clock":<FINAL>,"epic_id":"WAVE-7","phase":"6","tier":"phase_orch","event_type":"wave_7_complete","status":"complete","methods_above_8_remaining":0}
```

---

## Lamport Dependency Gates (Chain Verification)

Each Phase Orchestrator's VERY FIRST action is to verify the predecessor event:

| Phase Orch | Must Find In Lamport Log Before Starting |
|-----------|------------------------------------------|
| wave-orch-phase0 | `wave_start` OR `wave_reset` event |
| wave-orch-phase1 | `phase_0_orchestrator_complete` with `status=complete` |
| wave-orch-phase1-5 | `phase_1_orchestrator_complete` with `status=complete` |
| wave-orch-phase2 | `phase_1_5_orchestrator_complete` with `status=complete` |
| wave-orch-phase3 | `phase_2_orchestrator_complete` with `status=complete` |
| wave-orch-phase4 | `phase_3_orchestrator_complete` with `status=complete` |
| wave-orch-phase4-5 | `phase_4_orchestrator_complete` with `status=complete` |
| wave-orch-phase5 | `phase_4_5_orchestrator_complete` with `status=complete` |
| wave-orch-phase5v | `phase_5_orchestrator_complete` with `status=complete` |
| wave-orch-phase6 | `phase_5_v_orchestrator_complete` with `status=complete` |

If the predecessor event is absent: **HALT**, report `DEPENDENCY_NOT_MET` to Tier 1. Do NOT spawn any workers.

---

*Templates Version: V2.5 — Bob IDE V2 — 3-Tier Subagent Architecture*
*Created: 2026-06-25*
