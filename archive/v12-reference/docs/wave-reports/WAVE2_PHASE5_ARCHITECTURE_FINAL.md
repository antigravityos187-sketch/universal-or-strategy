# Wave 2 Phase 5 Architecture - Final Design

**Date**: 2026-06-13  
**Architecture**: Three-Tier Validation with Independent Agents

## User's Key Insight

"Every ticket should be an independent agent and each ticket should have its own self review, not a self review as an independent ticket. So it's every agent checks validates their own ticket implementation and every ticket implementation gets an independent validator agent and then there is an independent review of entire epic implementation."

## Three-Tier Validation Architecture

### Tier 1: Self-Validation (Built into Execution)
**Agent**: Execution agent (same agent that implements the ticket)
**Mode**: `v12-engineer` (Bob CLI)
**Action**: Agent validates its own work before marking ticket complete
**Output**: Included in `ticket-X-completion.md`
**Purpose**: Developer's own testing (sanity check)

### Tier 2: Independent Ticket Validation (Separate Agent)
**Agent**: Fresh independent agent (different from execution agent)
**Mode**: `advanced` (with MCP tools)
**Action**: Adversarial review of single ticket implementation
**Output**: `ticket-X-verification.md`
**Purpose**: Catch issues the execution agent missed (fresh eyes)

### Tier 3: Epic-Level Review (Separate Agent)
**Agent**: Fresh independent agent (orchestrator-level)
**Mode**: `advanced`
**Action**: Review entire epic (all 6 tickets together)
**Output**: `05-completion-report.md`
**Purpose**: Verify integration, consistency, and overall quality

## Final Architecture: 48 + 48 + 8 = 104 Agents

### Phase 5: Ticket Execution (48 agents)
**Structure**: 1 agent per ticket
- 8 epics × 6 tickets = 48 agents
- Each agent executes ONE ticket
- Each agent does self-validation before completion
- Mode: `v12-engineer`
- Output: `ticket-X-completion.md` (includes self-validation)

**Scripts**: `_p5_107_t1.sh` through `_p5_115_t6.sh`

### Phase 5.V: Independent Ticket Validation (48 agents)
**Structure**: 1 agent per ticket verification
- 8 epics × 6 verifications = 48 agents
- Each agent reviews ONE ticket (fresh eyes)
- Mode: `advanced` (with MCP tools for deep analysis)
- Output: `ticket-X-verification.md`

**Scripts**: `_p5v_107_t1.sh` through `_p5v_115_t6.sh`

### Phase 6: Epic-Level Review (8 agents)
**Structure**: 1 agent per epic
- 8 epics × 1 review = 8 agents
- Each agent reviews entire epic (all 6 tickets)
- Mode: `advanced`
- Output: `05-completion-report.md`

**Scripts**: `_p6_107.sh` through `_p6_115.sh`

## Execution Flow (Per Epic)

```
TICKET-1 Execution → Self-Validation → Independent Validation
TICKET-2 Execution → Self-Validation → Independent Validation
TICKET-3 Execution → Self-Validation → Independent Validation
TICKET-4 Execution → Self-Validation → Independent Validation
                                      ↓
TICKET-5 Execution → Self-Validation → Independent Validation
                                      ↓
TICKET-6 Execution → Self-Validation → Independent Validation
                                      ↓
                    Epic-Level Review (all 6 tickets)
```

## Parallelization Strategy

**Group 1 (Parallel)**: TICKET-1, TICKET-2, TICKET-3, TICKET-4 execution
**Group 2 (Sequential)**: TICKET-5 execution (depends on 1-4)
**Group 3 (Sequential)**: TICKET-6 execution (depends on 5)
**Group 4 (Parallel)**: All 6 ticket verifications (after respective tickets complete)
**Group 5 (Sequential)**: Epic-level review (after all verifications complete)

## Cost Estimate

**Phase 5 (Execution + Self-Validation)**:
- Per ticket: ~2-3 bobcoins
- 48 tickets × 2.5 avg = **~120 bobcoins**

**Phase 5.V (Independent Ticket Validation)**:
- Per verification: ~1-2 bobcoins
- 48 verifications × 1.5 avg = **~72 bobcoins**

**Phase 6 (Epic-Level Review)**:
- Per epic: ~3-4 bobcoins
- 8 epics × 3.5 avg = **~28 bobcoins**

**Total**: ~220 bobcoins for complete three-tier validation

## Time Estimate

**Per Epic**:
- T1-T4 execution: 2-3 hours (parallel)
- T5 execution: 1-2 hours (sequential)
- T6 execution: 1-2 hours (sequential)
- All verifications: 2-3 hours (parallel)
- Epic review: 1-2 hours (sequential)
- **Total per epic**: 7-12 hours

**All 8 Epics**: 7-12 hours (parallel execution across epics)

## Why This Architecture is Superior

### vs Option A (48 agents, no independent validation)
- ❌ Option A: Only self-validation (confirmation bias)
- ✅ This: Three-tier validation (catches more issues)

### vs Option B (8 agents, 1 per epic)
- ❌ Option B: No parallelization, context window risk
- ✅ This: Maximum parallelization, fresh context per ticket

### vs Hybrid (various combinations)
- ❌ Hybrid: Compromises on either parallelization or validation independence
- ✅ This: No compromises - maximum quality and speed

## Validation Independence Guarantee

**Tier 1 → Tier 2**: Different agents (execution vs verification)
**Tier 2 → Tier 3**: Different agents (ticket-level vs epic-level)
**Tier 1 → Tier 3**: Different agents (execution vs epic review)

**Result**: No confirmation bias at any level

## Script Generation Plan

**Phase 5 Scripts**: 48 scripts
- `_p5_107_t1.sh` through `_p5_115_t6.sh`
- Mode: `v12-engineer`
- Task: Execute ticket + self-validate
- Output: `ticket-X-completion.md`

**Phase 5.V Scripts**: 48 scripts
- `_p5v_107_t1.sh` through `_p5v_115_t6.sh`
- Mode: `advanced`
- Task: Independent ticket validation
- Output: `ticket-X-verification.md`

**Phase 6 Scripts**: 8 scripts
- `_p6_107.sh` through `_p6_115.sh`
- Mode: `advanced`
- Task: Epic-level review
- Output: `05-completion-report.md`

**Launchers**: 3 scripts
- `launch_phase5_all_screen.sh` (48 ticket executions)
- `launch_phase5v_all_screen.sh` (48 ticket verifications)
- `launch_phase6_all_screen.sh` (8 epic reviews)

**Total**: 107 scripts (48 + 48 + 8 + 3)

## Next Steps

1. Generate Phase 5 scripts (48 ticket execution scripts)
2. Generate Phase 5.V scripts (48 ticket verification scripts)
3. Generate Phase 6 scripts (8 epic review scripts)
4. Deploy all scripts to VM
5. Launch Phase 5 (execution)
6. Wait for Phase 5 completion
7. Launch Phase 5.V (verification)
8. Wait for Phase 5.V completion
9. Launch Phase 6 (epic review)
10. Collect final reports