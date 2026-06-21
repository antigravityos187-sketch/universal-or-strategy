# Wave 7 Lamport Clock System

**Version**: 1.0  
**Effective**: 2026-06-21  
**Wave**: 7 (180 epics, CYC > 8 → CYC ≤ 8)

## Overview

Wave 7 uses the V12.52 Lamport Causal Verification system to ensure deterministic, reproducible autonomous refactoring workflows. This directory contains Wave 7-specific event logs and configuration.

## Architecture

### Global vs Wave-Specific Logs

- **Global Log**: `.lamport/event_log.jsonl` - All waves, all epics
- **Wave 7 Log**: `.lamport/wave7/event_log.jsonl` - Wave 7 epics only (filtered view)
- **Global Clock**: `.lamport/global_clock.json` - Monotonically increasing across all waves

### Directory Structure

```
.lamport/
├── global_clock.json          # Global Lamport clock (all waves)
├── event_log.jsonl            # Global event log (all waves)
└── wave7/
    ├── README.md              # This file
    ├── event_log.jsonl        # Wave 7 events (filtered from global)
    ├── wave7_clock.json       # Wave 7 local clock (for analytics)
    └── stats.json             # Wave 7 statistics
```

## Event Schema

### Core Event Structure

```json
{
  "clock": 123,                           // Global Lamport clock (monotonic)
  "event_type": "phase_start",            // Event type (see below)
  "epic_id": "EPIC-W7-001",              // Wave 7 epic ID
  "phase": "0",                           // Phase identifier
  "agent_id": "wave7-phase0-001",        // Agent identifier
  "status": "running",                    // Status (see below)
  "state_hash": "abc123...",             // SHA256 of epic state
  "data": {},                             // Phase-specific data
  "timestamp": "2026-06-21T20:00:00Z"    // ISO 8601 UTC timestamp
}
```

### Event Types

| Event Type | Description | Status Values |
|------------|-------------|---------------|
| `phase_start` | Phase execution begins | `running` |
| `phase_complete` | Phase execution succeeds | `completed` |
| `phase_fail` | Phase execution fails | `failed` |
| `wave_start` | Wave 7 execution begins | `running` |
| `wave_complete` | Wave 7 execution completes | `completed` |
| `epic_start` | Epic begins (Phase 0 start) | `running` |
| `epic_complete` | Epic completes (Phase 6 complete) | `completed` |

### Status Values

- `pending` - Waiting for dependencies
- `running` - Currently executing
- `completed` - Successfully completed
- `failed` - Execution failed
- `blocked` - Blocked by dependency failure

### Phase Identifiers

Wave 7 uses the V12.25 manifest-based workflow with these phases:

| Phase | Name | Description |
|-------|------|-------------|
| `0` | Hotspot Analysis | Identify complexity hotspots |
| `1` | Scope Definition | Define refactoring scope |
| `1.5` | Scope Boundary | Validate scope boundaries |
| `2` | Architecture Planning | Design extraction strategy |
| `3` | DNA & PR Audit | Audit against V12 DNA |
| `4` | Ticket Generation | Generate implementation tickets |
| `4.5` | Ticket Review | Jane Street validation gate |
| `5.1` | Ticket 1 Execution | Execute first ticket |
| `5.1.V` | Ticket 1 Verification | Verify first ticket |
| `5.2` | Ticket 2 Execution | Execute second ticket |
| `5.2.V` | Ticket 2 Verification | Verify second ticket |
| `5.N` | Ticket N Execution | Execute Nth ticket |
| `5.N.V` | Ticket N Verification | Verify Nth ticket |
| `6` | Final Review | Epic completion report |

### Phase-Specific Data

#### Phase 0 (Hotspot Analysis)
```json
{
  "data": {
    "method_name": "ProcessOrders",
    "cyc_before": 21,
    "file_path": "src/V12_002.cs",
    "line_number": 1234
  }
}
```

#### Phase 1 (Scope Definition)
```json
{
  "data": {
    "scope_files": ["src/V12_002.cs"],
    "scope_methods": ["ProcessOrders"],
    "out_of_scope": ["OrderValidation"]
  }
}
```

#### Phase 2 (Architecture Planning)
```json
{
  "data": {
    "extraction_strategy": "FSM",
    "target_cyc": 8,
    "ticket_count": 3
  }
}
```

#### Phase 5.X (Ticket Execution)
```json
{
  "data": {
    "ticket_id": "5.1",
    "files_modified": ["src/V12_002.cs", "src/V12_002.OrderProcessor.cs"],
    "cyc_after": 7,
    "build_status": "passed"
  }
}
```

#### Phase 6 (Final Review)
```json
{
  "data": {
    "cyc_before": 21,
    "cyc_after": 7,
    "tickets_completed": 3,
    "build_status": "passed",
    "test_status": "passed"
  }
}
```

## Determinism Guarantees

### Happens-Before Relation

If event A happens-before event B, then `clock(A) < clock(B)`.

**Phase Dependencies** (enforced by `lamport_clock.py`):
```
-1 → 0 → 1 → 1.5 → 2 → 3 → 4 → 4.5 → 5.1 → 5.1.V → 5.2 → 5.2.V → ... → 6
```

### Concurrent Execution Rules

1. **Same Epic**: NEVER concurrent (enforced by `verify_determinism()`)
2. **Different Epics**: ALLOWED concurrent (parallel execution)
3. **Same Phase, Different Epics**: ALLOWED concurrent

### State Hash Verification

Each event includes a `state_hash` (SHA256) computed from:
1. Manifest content (`docs/brain/EPIC-W7-XXX/manifest.json`)
2. Phase output files (`*.md` in brain directory)
3. Git commit SHA

**Invariant**: Same inputs → Same state hash → Deterministic execution

## Usage

### Python API

```python
from scripts.lamport_clock import (
    record_phase_start,
    record_phase_complete,
    record_phase_fail,
    verify_can_execute,
    get_workflow
)

# Record phase start
event = record_phase_start("EPIC-W7-001", "0", "wave7-phase0-001")
print(f"Clock: {event['clock']}, Hash: {event['state_hash'][:8]}")

# Check if next phase can execute
can_execute, reason = verify_can_execute("EPIC-W7-001", "1")
if can_execute:
    record_phase_start("EPIC-W7-001", "1", "wave7-phase1-001")
else:
    print(f"Blocked: {reason}")

# Record phase completion
record_phase_complete("EPIC-W7-001", "1", "wave7-phase1-001", {
    "scope_files": ["src/V12_002.cs"]
})

# Get next executable phases
workflow = get_workflow()
next_phases = workflow.get_next_phases("EPIC-W7-001")
print(f"Next phases: {next_phases}")
```

### Bash Integration (Phase Scripts)

```bash
#!/bin/bash
# Phase 0 script example

EPIC_ID="EPIC-W7-001"
PHASE="0"
AGENT_ID="wave7-phase0-001"

# Record phase start
python3 -c "
from scripts.lamport_clock import record_phase_start
record_phase_start('$EPIC_ID', '$PHASE', '$AGENT_ID')
"

# Execute phase work
# ... (Bob CLI invocation, etc.)

# Record phase completion
python3 -c "
from scripts.lamport_clock import record_phase_complete
record_phase_complete('$EPIC_ID', '$PHASE', '$AGENT_ID', {
    'cyc_before': 21,
    'cyc_after': 8
})
"
```

## Wave 7 Statistics

Track Wave 7 progress using `stats.json`:

```json
{
  "wave_id": "wave7",
  "start_time": "2026-06-21T20:00:00Z",
  "total_epics": 180,
  "completed_epics": 0,
  "failed_epics": 0,
  "total_events": 0,
  "phases_completed": {
    "0": 0,
    "1": 0,
    "1.5": 0,
    "2": 0,
    "3": 0,
    "4": 0,
    "4.5": 0,
    "5.X": 0,
    "5.X.V": 0,
    "6": 0
  },
  "avg_cyc_reduction": 0.0,
  "total_bobcoins": 0.0
}
```

## Helper Scripts

### Filter Wave 7 Events

```bash
python scripts/filter_wave7_events.py
```

Filters global event log to Wave 7 events only, writes to `.lamport/wave7/event_log.jsonl`.

### Generate Wave 7 Statistics

```bash
python scripts/generate_wave7_stats.py
```

Computes Wave 7 statistics from event log, writes to `.lamport/wave7/stats.json`.

### Verify Wave 7 Determinism

```bash
python scripts/verify_wave7_determinism.py
```

Verifies all Wave 7 epics satisfy determinism guarantees.

## Recovery & Replay

### Replay Epic Workflow

```python
from scripts.lamport_clock import get_workflow

workflow = get_workflow()
events = workflow.replay_workflow("EPIC-W7-001")

for event in events:
    print(f"[{event['clock']}] {event['event_type']} - {event['phase']} - {event['status']}")
```

### Rollback to Phase

To rollback an epic to a specific phase:

1. Identify target phase in event log
2. Remove events after target phase
3. Update manifest status to `pending`
4. Re-run phase

**Example**:
```bash
# Rollback EPIC-W7-001 to Phase 2
python scripts/rollback_epic.py EPIC-W7-001 2
```

## Integration with Building-Blocks Method

Wave 7 phase scripts MUST use the Building-Blocks Method:

1. **Copy** scripts from previous wave's SAME phase
2. **Modify** only epic-specific parameters
3. **Verify** against SOP before execution

**Reference**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`

## Cost Optimization

Wave 7 uses 4-minute polling intervals (88% cost reduction):

- **Polling Interval**: 4 minutes (not 30 seconds)
- **Cache Optimization**: Reuse jCodemunch index
- **Batch Operations**: Group similar queries

**Reference**: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md`

## Jane Street KB Integration

Query Jane Street KB before architectural decisions:

```bash
python scripts/query_kb.py "complexity reduction"
python scripts/query_kb.py "FSM extraction"
python scripts/query_kb.py "lock-free patterns"
```

**When to Query**:
- Phase 2 (Architecture Planning)
- Phase 5 (Ticket Execution)
- Phase 5.V (Verification)

## Success Criteria

Wave 7 is complete when:

- ✅ All 180 epics reach Phase 6 (Final Review)
- ✅ All methods achieve CYC ≤ 8 (Jane Street strict standard)
- ✅ All events satisfy determinism guarantees
- ✅ No concurrent execution conflicts
- ✅ State hashes verify correctly

## References

- **Lamport Clock Implementation**: `scripts/lamport_clock.py`
- **V12.52 Protocol**: `docs/protocol/V12_52_LAMPORT_CAUSAL_VERIFICATION.md`
- **Wave 7 Roadmap**: `epic_roadmap_wave7.json`
- **Building-Blocks Method**: `docs/workflow/WAVE_PHASE_SCRIPT_GENERATION_SOP_V3.md`
- **Cost Optimization**: `docs/protocol/COST_OPTIMIZED_POLLING_PROTOCOL.md`

---

**Last Updated**: 2026-06-21  
**Maintainer**: Autonomous Refactor Mode  
**Status**: Active