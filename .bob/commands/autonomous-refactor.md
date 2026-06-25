---
description: Master autonomous refactoring orchestrator. Spawns subagents per epic per phase (Bob IDE V2). Runs all N epics in parallel per phase until entire codebase reaches CYC <= 8 with Jane Street compliance.
argument-hint: "[--phase PHASE] [--start-epic EPIC-W7-NNN] [--target-cyc N]"
---
# AUTONOMOUS REFACTOR — WAVE ORCHESTRATOR

**Mode:** `autonomous-refactor` (this mode)
**Protocol:** V12.28 Bob IDE V2 — Native Subagent Parallel Execution
**Goal:** All 161 methods CYC > 8 → CYC ≤ 8, Jane Street strict standard

---

## EXECUTION MODEL (BOB IDE V2 — AUTHORITATIVE)

**SPAWN SUBAGENTS. DO NOT use scripts, Bob Shell, or screen sessions.**

```
FOR EACH epic in current_phase_queue (ALL simultaneously):
  spawn_subagent(
    mode: <custom-mode-slug>         # from phase→mode map below
    description: |
      Epic: EPIC-W7-NNN
      Method: [MethodName] (CYC: N)
      Source: src/V12_002.*.cs
      Input: docs/brain/EPIC-W7-NNN/[previous-phase-output].md
      Task: Read input artifact. Execute [phase] work.
             Write output to docs/brain/EPIC-W7-NNN/[output].md
             Return: {status, output_path, cyc_achieved}
  )

WAIT for all subagents to complete.
Log Lamport events for all completions.
Re-spawn any failures individually.
```

### Phase → Custom Mode Map (MANDATORY)

| Phase | Custom Mode Slug | Output Artifact |
|-------|-----------------|-----------------|
| 0 | `v12-phase0-hotspot` | `00-hotspots.md` |
| 1 | `v12-phase1-scope` | `00-scope.md` |
| 1.5 | `v12-phase1-5-boundary` | `01-scope-boundary.md` |
| 2 | `v12-phase2-architecture` | `02-architecture-plan.md` |
| 3 | `v12-phase3-audit` | `03-audit-report.md` |
| 4 | `v12-phase4-tickets` | `04-tickets.md` |
| 4.5 | `v12-phase4-5-review` | `04-5-ticket-review.md` |
| 5 | `v12-engineer` | `ticket-X-completion.md` |
| 5.V | `v12-phase5-v-verify` | `ticket-X-verification.md` |
| 6 | `v12-phase6-review` | `05-completion-report.md` |

**CRITICAL — NEVER USE:**
- ❌ `_p0_NNN.sh`, `_p1_NNN.sh`, `_p2_NNN.sh` shell scripts
- ❌ `bob --yolo --chat-mode MODE "$(cat /tmp/msg.txt)"`
- ❌ GCP VM screen sessions
- ❌ `gcp-vm-wave-execution` skill
- ❌ 12-second delays between launches
- ❌ "Switch to: Advanced mode" — Advanced mode does not exist
- ❌ `/epic-run` — deprecated monolithic command

---

## ORCHESTRATION RULES

- **PARALLEL**: Spawn ALL N epics for a phase simultaneously — no delay needed
- **SUBAGENT ISOLATION**: Each subagent has clean context; only summary returns to orchestrator
- **LAMPORT TRACKING**: Log all phase transitions to `.lamport/wave7/event_log.jsonl`
- **JANE STREET KB**: Query `python scripts/query_kb.py "<term>"` before phases 2, 4.5, 5, 5.V, 6
- **100% MANDATE**: Wave is NOT complete until N/N epics complete — never accept N-1/N
- **FAILURE RECOVERY**: Re-spawn failed subagents individually; do not re-run successful ones
- **xUnit ONLY**: ALL tests must use xUnit `[Fact]` / `Assert.Equal()` — NEVER NUnit/MSTest
- **UTF-8**: ALL files must be UTF-8 encoded (no BOM)

---

## PROGRESS REPORTING

After each phase batch completes, output:

```
[WAVE-7] Phase [X] Progress
============================================================
Phase     : [X] [Phase Name]
Complete  : [N]/161 epics
Failed    : [F] (queued for re-spawn)
Duration  : [T]
Lamport   : [clock value]
Next Phase: [X+1] — ready to spawn
============================================================
```

---

## PHASE SEQUENCE

```
Phase 0 → Phase 1 → Phase 1.5 → Phase 2 → Phase 3 → Phase 4 → Phase 4.5
                                                                    ↓
Phase 6 ← Phase 5.N.V ← Phase 5.N (Ticket Execution, parallel per ticket)
```

**Jane Street KB queries required at:** Phase 2, 4.5, 5, 5.V, 6

---

## FAILURE RECOVERY PROTOCOL

1. Identify failed subagents from phase batch results
2. Log failure to `.lamport/wave7/event_log.jsonl`
3. Write failure note to `docs/brain/EPIC-W7-NNN/failure-analysis.md`
4. Re-spawn that epic's subagent individually (same mode, same inputs)
5. Do NOT re-run already-successful epics
6. Wave is complete only when 161/161 epics reach Phase 6

---

## LAMPORT EVENT SCHEMA

```json
{
  "timestamp": "2026-06-24T18:00:00Z",
  "lamport_clock": 42,
  "epic_id": "EPIC-W7-NNN",
  "phase": "1",
  "event_type": "phase_complete",
  "status": "success",
  "output_artifact": "docs/brain/EPIC-W7-NNN/00-scope.md"
}
```

---

## COMPLETION CRITERIA

Wave 7 is complete when:
- ✅ All 161 EPIC-W7-NNN directories have `05-completion-report.md`
- ✅ All methods verified CYC ≤ 8
- ✅ All xUnit tests passing
- ✅ UTF-8 compliance verified
- ✅ Lamport event log shows 161 phase_6_complete events

---

## REFERENCE

- **Integration Matrix**: `docs/workflow/AUTONOMOUS_REFACTOR_INTEGRATION_MATRIX_V2.md`
- **Source Data**: `complexity_audit_fresh_2026-06-14.txt`
- **Event Log**: `.lamport/wave7/event_log.jsonl`
- **Jane Street KB**: `python scripts/query_kb.py "<term>"`

*Protocol: V12.28 — Bob IDE V2 Native Subagent Model*
*Obsoletes: V12.25 epic-run monolithic, gcp-vm-wave-execution, _pX_NNN.sh scripts*
