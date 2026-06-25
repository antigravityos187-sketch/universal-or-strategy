---
description: Wave 7 Top-Level Orchestrator (Tier 1). Starts the sequential chain of 10 Phase Orchestrators. Each Phase Orchestrator runs as a start_subtask, spawns 161 parallel epic workers, enforces 100% completion, then start_subtask hands off to the next phase. Final phase reports WAVE_COMPLETE back here.
argument-hint: "[--reset] [--resume-phase PHASE]"
---

# AUTONOMOUS REFACTOR — WAVE 7 TOP-LEVEL ORCHESTRATOR (TIER 1)

**Mode:** `autonomous-refactor`
**Architecture:** 3-Tier Subagent Chain (V2.4)
**Goal:** All 161 methods CYC > 8 → CYC ≤ 8, Jane Street strict standard

---

## YOUR ROLE (Tier 1)

You are the **entry point** only. You do NOT spawn 161 workers directly.
You spawn **one Phase Orchestrator at a time** using `start_subtask`, then wait.
Each Phase Orchestrator is responsible for its own 161 workers AND for chaining to the next phase.

---

## EXECUTION PROTOCOL

### Step 1: Verify Prerequisites
```
- Read docs/brain/wave7-epic-list.json → confirm 161 entries
- Verify .lamport/wave7/event_log.jsonl exists
- Confirm docs/brain/EPIC-W7-* directories exist (161 total)
```

### Step 2: Log Wave Start
Append to `.lamport/wave7/event_log.jsonl`:
```json
{"timestamp":"<ISO8601>","lamport_clock":1,"epic_id":"WAVE-7","phase":"init","tier":"top_orch","event_type":"wave_start","status":"running","note":"Wave 7 start — 161 epics, Phase 0 first"}
```

### Step 3: Launch Phase 0 Orchestrator via start_subtask
```
start_subtask(
  title: "Wave 7 — Phase 0 Orchestrator",
  mode: "wave-orch-phase0",
  message: |
    You are the Phase 0 (Hotspot Analysis) Orchestrator for Wave 7.
    Epic list: docs/brain/wave7-epic-list.json (161 epics)
    Lamport log: .lamport/wave7/event_log.jsonl
    
    Execute your full protocol:
    1. Spawn 161 v12-phase0-hotspot workers in parallel (spawn_subagent)
    2. Enforce 100% completion (retry loop up to 3 rounds)
    3. On VERIFIED_COMPLETE: start_subtask the Phase 1 Orchestrator
    4. The chain continues automatically through all 10 phases
    5. Phase 6 Orchestrator will report WAVE_COMPLETE back
    
    See your roleDefinition and docs/workflow/PHASE_ORCHESTRATOR_TEMPLATES.md for full protocol.
)
```

### Step 4: Wait for WAVE_COMPLETE
- The subtask chain runs autonomously: Ph0 → Ph1 → Ph1.5 → Ph2 → Ph3 → Ph4 → Ph4.5 → Ph5 → Ph5.V → Ph6
- Each phase orchestrator hands off via start_subtask to the next
- Phase 6 Orchestrator returns WAVE_COMPLETE to you

### Step 5: On WAVE_COMPLETE
```
- Log wave_7_complete to .lamport/wave7/event_log.jsonl
- Verify: python scripts/complexity_audit.py | grep "REFACTOR" | wc -l → should be 0
- Report: "Wave 7 complete — 161/161 methods now CYC ≤ 8"
```

---

## HARD_FAILURE HANDLING

If any phase reports `HARD_FAILURE`:
1. Log to `.lamport/wave7/event_log.jsonl`
2. Write `docs/brain/wave7-incident-report.md` with stuck epic list
3. HALT — do NOT proceed
4. Report to Director for manual resolution
5. After resolution: re-start_subtask that specific Phase Orchestrator only

---

## PHASE CHAIN MAP

| Phase | Orchestrator Mode | Worker Mode | Hands Off To |
|-------|-------------------|-------------|--------------|
| 0 | `wave-orch-phase0` | `v12-phase0-hotspot` | `wave-orch-phase1` |
| 1 | `wave-orch-phase1` | `v12-phase1-scope` | `wave-orch-phase1-5` |
| 1.5 | `wave-orch-phase1-5` | `v12-phase1-5-boundary` | `wave-orch-phase2` |
| 2 | `wave-orch-phase2` | `v12-phase2-architecture` | `wave-orch-phase3` |
| 3 | `wave-orch-phase3` | `v12-phase3-audit` | `wave-orch-phase4` |
| 4 | `wave-orch-phase4` | `v12-phase4-tickets` | `wave-orch-phase4-5` |
| 4.5 | `wave-orch-phase4-5` | `v12-phase4-5-review` | `wave-orch-phase5` |
| 5 | `wave-orch-phase5` | `v12-engineer` | `wave-orch-phase5v` |
| 5.V | `wave-orch-phase5v` | `v12-phase5-v-verify` | `wave-orch-phase6` |
| 6 | `wave-orch-phase6` | `v12-phase6-review` | → WAVE_COMPLETE to Tier 1 |

---

## CRITICAL RULES

- **NEVER** spawn 161 workers directly from Tier 1 — delegate to Phase Orchestrators
- **NEVER** use spawn_subagent for Phase Orchestrators — use `start_subtask`
- **NEVER** skip a phase — each phase's output is the next phase's input
- **xUnit ONLY**: `[Fact]`, `Assert.Equal()` — NEVER NUnit/MSTest
- **UTF-8**: All files UTF-8 (no BOM)
- **Workers**: Bob IDE V2 spawns subagents internally — no external API keys needed

## REFERENCE
- Epic list: `docs/brain/wave7-epic-list.json`
- Templates: `docs/workflow/PHASE_ORCHESTRATOR_TEMPLATES.md`
- Matrix: `docs/workflow/AUTONOMOUS_REFACTOR_INTEGRATION_MATRIX_V2.md`
- Lamport: `.lamport/wave7/event_log.jsonl`
