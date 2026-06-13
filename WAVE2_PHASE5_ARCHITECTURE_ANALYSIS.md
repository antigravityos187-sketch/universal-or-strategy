# Wave 2 Phase 5 Architecture Analysis

**Date**: 2026-06-13  
**Question**: Is T6 self-validation, and is there another phase for independent validation?

## Answer: YES - Two-Tier Validation System

### Phase Structure (Per Epic)

```
Phase 5: Ticket Execution (6 tickets per epic)
├─ Phase 5.1: Execute TICKET-1 (extraction)
├─ Phase 5.2: Execute TICKET-2 (extraction)
├─ Phase 5.3: Execute TICKET-3 (extraction)
├─ Phase 5.4: Execute TICKET-4 (extraction)
├─ Phase 5.5: Execute TICKET-5 (integration)
└─ Phase 5.6: Execute TICKET-6 (self-validation)

Phase 5.X.V: Independent Verification (6 verifications per epic)
├─ Phase 5.1.V: Verify TICKET-1 (independent agent)
├─ Phase 5.2.V: Verify TICKET-2 (independent agent)
├─ Phase 5.3.V: Verify TICKET-3 (independent agent)
├─ Phase 5.4.V: Verify TICKET-4 (independent agent)
├─ Phase 5.5.V: Verify TICKET-5 (independent agent)
└─ Phase 5.6.V: Verify TICKET-6 (independent agent)

Phase 6: Final Review (1 per epic)
└─ Aggregates all verification reports
```

### Validation Tiers

**Tier 1: Self-Validation (TICKET-6)**
- **Agent**: Same agent that executed TICKET-1 through TICKET-5
- **Mode**: `v12-engineer` (Bob CLI)
- **Purpose**: Developer's own testing and verification
- **Output**: `ticket-6-completion.md`
- **Bias**: Potential confirmation bias (agent validates own work)

**Tier 2: Independent Verification (Phase 5.X.V)**
- **Agent**: Fresh agent (different from execution agent)
- **Mode**: `advanced` (with MCP tools)
- **Purpose**: Adversarial audit by independent reviewer
- **Output**: `ticket-X-verification.md` (for each ticket)
- **Bias**: None - fresh eyes, no prior context

**Tier 3: Final Review (Phase 6)**
- **Agent**: Fresh agent (orchestrator-level)
- **Mode**: `advanced`
- **Purpose**: Aggregate all verifications, final sign-off
- **Output**: `05-completion-report.md`

## Optimal Architecture for Wave 2 Phase 5

### Recommendation: **Option A (48 Separate Agents)**

**Rationale**:
1. ✅ **Two-tier validation requires independence**: Phase 5.X.V agents MUST be separate from Phase 5.X agents
2. ✅ **Smaller task = higher quality**: Your key insight is correct
3. ✅ **Parallel execution**: T1-T4 can run simultaneously (4× speedup)
4. ✅ **Failure isolation**: One ticket fails, others continue
5. ✅ **Clear checkpointing**: Resume from any ticket
6. ✅ **Adversarial audit**: Phase 5.X.V agents have no bias from execution

### Architecture Details

**Phase 5 (Ticket Execution)**: 48 agents
- 8 epics × 6 tickets = 48 scripts
- Each ticket is a separate Bob session
- Mode: `v12-engineer`
- Output: `ticket-X-completion.md`

**Phase 5.X.V (Independent Verification)**: 48 agents
- 8 epics × 6 verifications = 48 scripts
- Each verification is a separate session (fresh agent)
- Mode: `advanced` (with MCP tools for deep analysis)
- Output: `ticket-X-verification.md`

**Total**: 96 agents (48 execution + 48 verification)

### Execution Strategy

**Parallel Groups**:
```
Group 1 (Parallel): TICKET-1, TICKET-2, TICKET-3, TICKET-4
Group 2 (Sequential): TICKET-5 (depends on 1-4)
Group 3 (Sequential): TICKET-6 (depends on 5)
Group 4 (Parallel): Verify TICKET-1, TICKET-2, TICKET-3, TICKET-4, TICKET-5, TICKET-6
```

**Per Epic Timeline**:
- T1-T4: 2-3 hours (parallel)
- T5: 1-2 hours (sequential)
- T6: 1-2 hours (sequential)
- Verification: 2-3 hours (parallel)
- **Total per epic**: 6-10 hours

**All 8 Epics**: 6-10 hours (parallel execution across epics)

### Cost Estimate

**Phase 5 (Execution)**:
- Per ticket: ~2-3 bobcoins
- 48 tickets × 2.5 avg = **~120 bobcoins**

**Phase 5.X.V (Verification)**:
- Per verification: ~1-2 bobcoins (read-only analysis)
- 48 verifications × 1.5 avg = **~72 bobcoins**

**Total**: ~192 bobcoins for Phase 5 + 5.X.V

### Why NOT Hybrid (1 Agent per Epic)?

**Problem**: Violates two-tier validation principle
- If TICKET-6 is executed by same agent as TICKET-1-5, it's self-validation
- Phase 5.X.V agents would still need to be separate (48 agents)
- No cost savings (still need 48 verification agents)
- Loses parallelization (T1-T4 must be sequential)
- Context window risk (6 tickets in one session)

**Conclusion**: Hybrid approach provides NO benefits and violates validation independence.

## Final Answer

**YES**, TICKET-6 is self-validation, and **YES**, there is another phase (5.X.V) for independent validation.

**Optimal Architecture**: 48 separate agents for Phase 5 execution + 48 separate agents for Phase 5.X.V verification = **96 agents total**.

This architecture:
- ✅ Maintains validation independence (no confirmation bias)
- ✅ Maximizes parallelization (4× speedup for T1-T4)
- ✅ Follows "smaller task = higher quality" principle
- ✅ Enables failure isolation and checkpointing
- ✅ Aligns with V12 manifest-based independent subtask architecture

## Script Generation Plan

**Phase 5 Scripts**: 48 scripts
- `_p5_107_t1.sh` through `_p5_115_t6.sh`
- Mode: `v12-engineer`
- Output: `ticket-X-completion.md`

**Phase 5.X.V Scripts**: 48 scripts (separate generation)
- `_p5v_107_t1.sh` through `_p5v_115_t6.sh`
- Mode: `advanced`
- Output: `ticket-X-verification.md`

**Launchers**: 2 scripts
- `launch_phase5_all_screen.sh` (execution)
- `launch_phase5v_all_screen.sh` (verification)

**Total**: 98 scripts (48 + 48 + 2)