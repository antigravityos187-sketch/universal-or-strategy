# Jane Street Agent Integration Guide

**Version**: V12 Phase 4  
**Last Updated**: 2026-06-03  
**Audience**: All agents (Bob CLI, Codex CLI, Gemini CLI, Jules AI, Advanced Mode, etc.)

---

## Table of Contents

1. [Overview](#overview)
2. [KB Query Workflow](#kb-query-workflow)
3. [Session Snapshot Workflow](#session-snapshot-workflow)
4. [Budget-Aware Exploration](#budget-aware-exploration)
5. [Example Sessions](#example-sessions)
6. [Troubleshooting](#troubleshooting)

---

## Overview

The **Jane Street Cyborg Transformation** integrates high-frequency trading (HFT) patterns and testing standards from Jane Street Capital into the V12 Universal OR Strategy. This guide teaches agents how to:

1. **Query the Jane Street Knowledge Base** for verified patterns
2. **Track session state** to prevent redundant reads
3. **Manage token budgets** for long-running tasks
4. **Use session continuity** for epic-scale refactoring

### Key Principles

- **Correctness by Construction**: Make illegal states unrepresentable
- **Lock-Free Actor Pattern**: No `lock()` blocks, use FSM/Actor Enqueue
- **ASCII-Only Compliance**: No Unicode/emoji in C# strings
- **Cognitive Simplicity**: CYC ≤ 15 (Jane Street aligned)

---

## KB Query Workflow

### When to Query

Query the Jane Street KB **before** starting:
- Epic planning (EPIC-CCN-X)
- Complex refactoring (God-function splitting)
- Performance optimization
- FSM/Actor pattern implementation
- Testing strategy design

### How to Query

**Command**:
```bash
python scripts/query_kb.py "<search term>"
```

**Examples**:
```bash
# FSM patterns
python scripts/query_kb.py "FSM state machine"

# Async patterns
python scripts/query_kb.py "async await patterns"

# Testing patterns
python scripts/query_kb.py "unit testing mocking"

# Performance patterns
python scripts/query_kb.py "zero allocation optimization"
```

**Output Format**:
```
Found 5 relevant documents:

1. async/src/async_kernel.ml (Score: 0.92)
   Pattern: Async monad for non-blocking I/O
   Context: Use Deferred.bind for chaining async operations
   
2. core/src/option.ml (Score: 0.88)
   Pattern: Option<T> for nullable values
   Context: Replace null checks with Option.map/bind
```

### Integration into Planning

**Step 1**: Query KB for relevant patterns
```bash
python scripts/query_kb.py "FSM state machine patterns"
```

**Step 2**: Document patterns in `03-architecture.md`
```markdown
## Jane Street Patterns Applied

### FSM State Machine (async/src/async_kernel.ml)
- **Pattern**: State transitions via immutable message passing
- **V12 Adaptation**: Use `Channel<T>.Enqueue` for state mutations
- **Rationale**: Eliminates lock contention, enables lock-free correctness
```

**Step 3**: Cite patterns in implementation
```csharp
// Jane Street Pattern: FSM State Machine (async/src/async_kernel.ml)
// V12 Adaptation: Channel<T> replaces Async.Deferred
private readonly Channel<StateTransition> _stateChannel;
```

---

## Session Snapshot Workflow

### When to Use

Use session snapshots for:
- **Epic planning** (multi-file exploration)
- **PR loops** (iterative repair cycles)
- **Complex refactoring** (>5 files touched)
- **Long-running tasks** (>100k tokens)

### Workflow

#### 1. Initialize Session

```bash
python scripts/session_snapshot.py init "EPIC-CCN-14" "v12-engineer" "Extract ShouldSkipFleet"
```

**Output**:
```
Session initialized: EPIC-CCN-14
```

#### 2. Before Reading a File

**Check if already read**:
```bash
python scripts/session_snapshot.py check-read "EPIC-CCN-14" "src/V12_002.SIMA.Fleet.cs"
```

**Exit Codes**:
- `0` = Already read → Skip redundant read
- `1` = Not read → Proceed with read

#### 3. After Reading a File

**Record the read**:
```bash
python scripts/session_snapshot.py record-read "EPIC-CCN-14" "src/V12_002.SIMA.Fleet.cs" "outline"
```

**Read Types**:
- `outline` = File outline (symbols only)
- `full` = Full file content
- `symbol` = Specific symbol source
- `context_bundle` = Symbol + imports

#### 4. After Exploring a Symbol

**Record the symbol**:
```bash
python scripts/session_snapshot.py record-symbol "EPIC-CCN-14" "sym_id_123" "ShouldSkipFleet_RunHealthCheck" "src/V12_002.SIMA.Fleet.cs"
```

#### 5. After a Search

**Record the search**:
```bash
python scripts/session_snapshot.py record-search "EPIC-CCN-14" "FSM state machine" "search_symbols" 5
```

#### 6. Update Token Budget

**Periodically update**:
```bash
python scripts/session_snapshot.py update-budget "EPIC-CCN-14" 75000
```

**Output**:
```
Budget updated: 75000 consumed, 125000 remaining
```

#### 7. Check Budget Status

**Get current state**:
```bash
python scripts/session_snapshot.py get "EPIC-CCN-14" --json | jq '.token_budget.remaining'
```

**Output**:
```json
125000
```

**Decision Rule**:
- `remaining < 40000` → **STOP exploration**, work with what you have
- `remaining >= 40000` → Continue exploration

---

## Budget-Aware Exploration

### Token Budget Rules

| Remaining Tokens | Action |
|------------------|--------|
| **200k - 160k** | Full exploration (read files, search freely) |
| **160k - 80k** | Moderate exploration (prioritize high-value reads) |
| **80k - 40k** | Conservative exploration (use `get_file_outline` instead of `get_file_content`) |
| **< 40k** | **STOP exploration** (work with cached knowledge) |

### Optimization Strategies

#### High-Value Reads (Prioritize)
- God functions (CYC > 20)
- Entry points (`OnBarUpdate`, `OnOrderUpdate`)
- FSM state machines
- Core business logic

#### Low-Value Reads (Skip)
- Utility functions (formatters, helpers)
- UI code (unless UI-specific task)
- Test files (unless testing task)
- Generated code

#### Efficient Tools

| Tool | Tokens | Use Case |
|------|--------|----------|
| `get_file_outline` | ~500 | Get symbol list without source |
| `get_symbol_source` | ~1000 | Get specific symbol only |
| `get_file_content` | ~5000 | Get full file (last resort) |
| `get_context_bundle` | ~2000 | Get symbol + imports |

### Budget Warnings

**Automatic warnings** at 80% consumption:
```json
{
  "token_budget": {
    "consumed": 160000,
    "remaining": 40000,
    "budget_warnings": [
      "Budget warning at 2026-06-03T20:00:00Z: 160000/200000 tokens consumed"
    ]
  }
}
```

**Action**: Switch to `get_file_outline` and `get_symbol_source` only.

---

## Session Continuity

### Auto-Snapshot

**Automatically create checkpoints** every 50k tokens:

```bash
python scripts/session_continuity.py auto-snapshot "EPIC-CCN-14" 50000
```

**Output**:
```
Auto-snapshot created: checkpoint_001.json
  Tokens: 50,000 (50,000 at checkpoint)
  Files read: 12
  Symbols explored: 8
```

**Checkpoints**:
- `checkpoint_001.json` = 50k tokens
- `checkpoint_002.json` = 100k tokens
- `checkpoint_003.json` = 150k tokens
- `checkpoint_004.json` = 200k tokens

### Restore Session

**Load from latest checkpoint**:
```bash
python scripts/session_continuity.py restore "EPIC-CCN-14"
```

**Output**:
```
Restored checkpoint: checkpoint_003.json
  Timestamp: 2026-06-03T18:30:00Z
  Tokens consumed: 150,000
  Files read: 25
  Symbols explored: 18

Context summary:
  Recent files: V12_002.SIMA.Fleet.cs, V12_002.SIMA.Dispatch.cs
  Recent symbols: ShouldSkipFleet_RunHealthCheck, DispatchEntry
  Recent searches: FSM state machine, health check logic
```

**Use Case**: Resume after context compaction or session timeout.

### List Checkpoints

```bash
python scripts/session_continuity.py list "EPIC-CCN-14"
```

**Output**:
```
Checkpoints for session: EPIC-CCN-14
--------------------------------------------------------------------------------
Checkpoint   1: 2026-06-03T17:00:00Z
  Tokens: 50,000 | Files: 12 | Symbols: 8
Checkpoint   2: 2026-06-03T17:30:00Z
  Tokens: 100,000 | Files: 18 | Symbols: 14
Checkpoint   3: 2026-06-03T18:00:00Z
  Tokens: 150,000 | Files: 25 | Symbols: 18

Total: 3 checkpoints
```

### Merge Checkpoints

**Combine multiple checkpoints** for long-running tasks:

```bash
python scripts/session_continuity.py merge "EPIC-CCN-14" 1 2 3
```

**Output**:
```
Merged checkpoint created: merged_1-2-3.json
  Checkpoints merged: 3
  Total files: 25
  Total symbols: 18
  Total searches: 12
```

### Prune Old Checkpoints

**Keep only last 5 checkpoints**:

```bash
python scripts/session_continuity.py prune "EPIC-CCN-14" --keep 5
```

**Output**:
```
Deleted: checkpoint_001.json
Deleted: checkpoint_002.json
Pruned 2 old checkpoints (kept last 5)
```

---

## Example Sessions

### Example 1: Epic Planning with Jane Street Patterns

**Task**: Plan EPIC-CCN-14 (Extract ShouldSkipFleet_RunHealthCheck)

#### Step 1: Initialize Session

```bash
python scripts/session_snapshot.py init "EPIC-CCN-14" "v12-epic-planner" "Extract ShouldSkipFleet"
```

#### Step 2: Query Jane Street KB

```bash
python scripts/query_kb.py "FSM state machine patterns"
```

**Output**:
```
Found 5 relevant documents:

1. async/src/async_kernel.ml (Score: 0.92)
   Pattern: State transitions via immutable message passing
   Context: Use Deferred.bind for chaining async operations
```

#### Step 3: Check Negative Evidence

```bash
python scripts/negative_evidence_check.py "health check FSM"
```

**Exit Code**: `1` (no evidence) → Proceed with search

#### Step 4: Explore Code

**Check if file already read**:
```bash
python scripts/session_snapshot.py check-read "EPIC-CCN-14" "src/V12_002.SIMA.Fleet.cs"
```

**Exit Code**: `1` (not read) → Proceed

**Read file outline**:
```bash
# Use jcodemunch-mcp tool
get_file_outline --path "src/V12_002.SIMA.Fleet.cs"
```

**Record read**:
```bash
python scripts/session_snapshot.py record-read "EPIC-CCN-14" "src/V12_002.SIMA.Fleet.cs" "outline"
```

#### Step 5: Check Budget

```bash
python scripts/session_snapshot.py get "EPIC-CCN-14" --json | jq '.token_budget.remaining'
```

**Output**: `185000` → Continue exploration

#### Step 6: Generate Plan

**Document in `03-architecture.md`**:
```markdown
## Jane Street Patterns Applied

### FSM State Machine (async/src/async_kernel.ml)
- **Pattern**: State transitions via immutable message passing
- **V12 Adaptation**: Use `Channel<T>.Enqueue` for state mutations
- **Rationale**: Eliminates lock contention, enables lock-free correctness

## Extraction Plan

### Target Method
- **Name**: `ShouldSkipFleet_RunHealthCheck`
- **Location**: `src/V12_002.SIMA.Fleet.cs:1234`
- **Complexity**: CYC 29 → Target CYC 7

### Extraction Strategy
1. Extract health check logic to `FleetHealthChecker.cs`
2. Use FSM pattern for state transitions
3. Replace inline checks with `Channel<T>.Enqueue`
```

---

### Example 2: PR Loop with Session Continuity

**Task**: Fix PR #25 issues (3 rounds of feedback)

#### Round 1: Initial Repair

```bash
# Initialize session
python scripts/session_snapshot.py init "PR-25-REPAIR" "advanced" "Fix PR #25 issues"

# Read files, make changes
# ... (work happens here)

# Update budget after 60k tokens
python scripts/session_snapshot.py update-budget "PR-25-REPAIR" 60000

# Auto-snapshot at 50k threshold
python scripts/session_continuity.py auto-snapshot "PR-25-REPAIR" 60000
```

**Output**:
```
Auto-snapshot created: checkpoint_001.json
  Tokens: 60,000 (50,000 at checkpoint)
  Files read: 8
  Symbols explored: 6
```

#### Round 2: Address Feedback

```bash
# Restore from checkpoint
python scripts/session_continuity.py restore "PR-25-REPAIR"

# Continue work
# ... (more changes)

# Update budget after 120k tokens
python scripts/session_snapshot.py update-budget "PR-25-REPAIR" 120000

# Auto-snapshot at 100k threshold
python scripts/session_continuity.py auto-snapshot "PR-25-REPAIR" 120000
```

#### Round 3: Final Fixes

```bash
# Restore from latest checkpoint
python scripts/session_continuity.py restore "PR-25-REPAIR"

# Final changes
# ... (final work)

# List all checkpoints
python scripts/session_continuity.py list "PR-25-REPAIR"
```

**Output**:
```
Checkpoints for session: PR-25-REPAIR
--------------------------------------------------------------------------------
Checkpoint   1: 2026-06-03T10:00:00Z
  Tokens: 60,000 | Files: 8 | Symbols: 6
Checkpoint   2: 2026-06-03T11:00:00Z
  Tokens: 120,000 | Files: 14 | Symbols: 11

Total: 2 checkpoints
```

---

### Example 3: Budget-Aware Refactoring

**Task**: Refactor God-function with CYC 45

#### Phase 1: Full Exploration (200k - 160k tokens)

```bash
# Initialize
python scripts/session_snapshot.py init "GOD-FUNC-REFACTOR" "v12-engineer" "Refactor ProcessBracketEvent"

# Read full file
get_file_content --path "src/V12_002.Symmetry.BracketFSM.cs"

# Record read
python scripts/session_snapshot.py record-read "GOD-FUNC-REFACTOR" "src/V12_002.Symmetry.BracketFSM.cs" "full"

# Update budget
python scripts/session_snapshot.py update-budget "GOD-FUNC-REFACTOR" 40000
```

#### Phase 2: Moderate Exploration (160k - 80k tokens)

```bash
# Check budget
python scripts/session_snapshot.py get "GOD-FUNC-REFACTOR" --json | jq '.token_budget.remaining'
# Output: 160000

# Switch to outlines only
get_file_outline --path "src/V12_002.Symmetry.BracketHelpers.cs"

# Record read
python scripts/session_snapshot.py record-read "GOD-FUNC-REFACTOR" "src/V12_002.Symmetry.BracketHelpers.cs" "outline"

# Update budget
python scripts/session_snapshot.py update-budget "GOD-FUNC-REFACTOR" 80000
```

#### Phase 3: Conservative Exploration (80k - 40k tokens)

```bash
# Check budget
python scripts/session_snapshot.py get "GOD-FUNC-REFACTOR" --json | jq '.token_budget.remaining'
# Output: 120000

# Use symbol-specific reads only
get_symbol_source --symbol-id "sym_123" --path "src/V12_002.Symmetry.BracketFSM.cs"

# Record symbol
python scripts/session_snapshot.py record-symbol "GOD-FUNC-REFACTOR" "sym_123" "ProcessBracketEvent" "src/V12_002.Symmetry.BracketFSM.cs"

# Update budget
python scripts/session_snapshot.py update-budget "GOD-FUNC-REFACTOR" 120000
```

#### Phase 4: STOP Exploration (< 40k tokens)

```bash
# Check budget
python scripts/session_snapshot.py get "GOD-FUNC-REFACTOR" --json | jq '.token_budget.remaining'
# Output: 35000

# STOP exploration - work with cached knowledge
# Generate implementation plan from existing data
```

---

## Troubleshooting

### Issue: Session Not Found

**Error**:
```
Error: Session not found. Use 'init' first.
```

**Solution**:
```bash
python scripts/session_snapshot.py init "SESSION-ID" "agent-name" "task description"
```

---

### Issue: Budget Warning Not Triggering

**Symptom**: No budget warnings despite high consumption

**Solution**: Ensure `update-budget` is called regularly:
```bash
# Call after every 10k tokens consumed
python scripts/session_snapshot.py update-budget "SESSION-ID" <tokens>
```

---

### Issue: Checkpoint Not Created

**Error**:
```
Error creating checkpoint: [Errno 2] No such file or directory
```

**Solution**: Ensure session is initialized first:
```bash
python scripts/session_snapshot.py init "SESSION-ID" "agent" "task"
python scripts/session_continuity.py auto-snapshot "SESSION-ID" 50000
```

---

### Issue: Redundant File Reads

**Symptom**: Reading the same file multiple times

**Solution**: Always check before reading:
```bash
python scripts/session_snapshot.py check-read "SESSION-ID" "path/to/file.cs"
# Exit 0 = already read, skip
# Exit 1 = not read, proceed
```

---

### Issue: Token Budget Exceeded

**Symptom**: Budget warnings but exploration continues

**Solution**: Implement hard stop at 40k remaining:
```bash
remaining=$(python scripts/session_snapshot.py get "SESSION-ID" --json | jq '.token_budget.remaining')
if [ $remaining -lt 40000 ]; then
    echo "Budget exhausted - stopping exploration"
    exit 1
fi
```

---

## Related Documentation

- [AGENTS.md](../../AGENTS.md) - Agent hierarchy and protocols
- [JANE_STREET_PHILOSOPHY.md](../standards/jane-street/JANE_STREET_PHILOSOPHY.md) - Core principles
- [JANE_STREET_FSM_PATTERNS.md](../standards/jane-street/JANE_STREET_FSM_PATTERNS.md) - FSM patterns
- [PRE_PUSH_VALIDATION.md](../protocol/PRE_PUSH_VALIDATION.md) - Quality gates
- [PHASE3_INTEGRATION_SUMMARY.md](../brain/PHASE3_INTEGRATION_SUMMARY.md) - Phase 3 summary

---

**Made with Bob** 🤖