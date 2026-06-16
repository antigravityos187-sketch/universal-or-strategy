# Wave 2 Phase 5 Architecture - Corrected Understanding

**Date**: 2026-06-13  
**Clarification**: Tickets are jobs, not review tasks

## Corrected Understanding

### What is a "Ticket"?
A ticket is a **job** (extraction, integration, etc.), NOT a review task.

### Ticket Structure (Per Epic)
- **TICKET-1**: Extraction job (e.g., extract method A)
- **TICKET-2**: Extraction job (e.g., extract method B)
- **TICKET-3**: Extraction job (e.g., extract method C)
- **TICKET-4**: Extraction job (e.g., extract method D)
- **TICKET-5**: Integration job (integrate extracted methods)
- **TICKET-6**: Another job (NOT a review ticket)

**Total**: 6 jobs per epic (NOT 5 jobs + 1 review)

## Three-Tier Validation (Corrected)

### Tier 1: Self-Review (Built into Execution)
**When**: During ticket execution
**Who**: Same agent executing the ticket
**Action**: Agent validates its own work before marking complete
**Output**: Included in `ticket-X-completion.md`

### Tier 2: Independent Ticket Review
**When**: After ticket execution completes
**Who**: Fresh independent agent (different from execution agent)
**Action**: Review single ticket implementation
**Output**: `ticket-X-verification.md`

### Tier 3: Independent Epic Review
**When**: After all tickets complete
**Who**: Fresh independent agent (epic-level)
**Action**: Review entire epic (all 6 tickets together)
**Output**: `05-completion-report.md`

## Corrected Architecture

### Phase 5: Ticket Execution (40 agents)
**Structure**: 1 agent per ticket
- 8 epics × 5 tickets = 40 agents (TICKET-1 through TICKET-5)
- Each agent executes ONE job
- Each agent does self-review before completion
- Mode: `v12-engineer`
- Output: `ticket-X-completion.md` (includes self-review)

**Wait, this doesn't match either...**

Let me re-read the user's clarification:
"a ticket is a job, agent self reviews then independent ticket review then independent epic review"

So:
- Ticket = Job (extraction, integration, etc.)
- Agent self-reviews the job
- Independent agent reviews the ticket
- Independent agent reviews the epic

**Question**: How many tickets per epic? Is TICKET-6 a job or was it mistakenly called a "review ticket"?

## Need Clarification

Let me check what Phase 4 actually generated to see the ticket structure...