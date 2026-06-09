# docs/ - Documentation Rules

**Last Updated**: 2026-06-08T22:45:00Z
**Scope**: Project documentation, standards, and knowledge base

---

## Directory Structure

```
docs/
├── brain/              # Epic documentation and session tracking
├── standards/          # Coding standards and patterns
│   └── jane-street/    # Jane Street HFT patterns
├── protocol/           # Workflow protocols
├── workflow/           # Git and branch strategies
└── mcp/                # MCP server configurations
```

---

## Documentation Standards

### Epic Documentation (`docs/brain/EPIC-*/`)

**Required Files**:
1. `00-scope.md` - Epic scope and objectives
2. `01-analysis.md` - Risk analysis and complexity assessment
3. `02-approach.md` - Implementation strategy
4. `02-greptile-report.md` - Sentinel audit results
5. `ticket-XX-*.md` - Individual ticket specifications
6. `EXECUTION_GUIDE.md` - Ticket execution order
7. `FORENSIC_REPORT.md` - Post-mortem (if epic fails)
8. `CANCELLATION_NOTICE.md` - Cancellation rationale (if applicable)

**Naming Convention**:
- Epic IDs: `EPIC-CCN-N` (Cyclomatic Complexity Normalization)
- Ticket IDs: `ticket-01-extract-method-name.md`

**Update Frequency**: After each epic phase completion

---

## Jane Street Knowledge Base (`docs/standards/jane-street/`)

**Purpose**: HFT (High-Frequency Trading) patterns and principles

**Key Documents**:
- `RULES_CATALOG.md` - 100+ rules with P0/P1/P2 severity
- `CORE_PATTERNS.md` - Fundamental design patterns
- `ASYNC_PATTERNS.md` - Asynchronous programming patterns
- `FSM_PATTERNS.md` - Finite State Machine patterns
- `TESTING_PATTERNS.md` - Testing strategies

**Usage**: Query before epic planning
```bash
python scripts/query_kb.py "complexity reduction"
python scripts/query_kb.py "FSM extraction"
```

**Maintenance**: Read-only (sourced from Firebase)

---

## Protocol Documentation (`docs/protocol/`)

**Purpose**: Workflow and process documentation

**Key Documents**:
- `BRANCH_STRATEGY.md` - Three-tier branch model
- `COMPLEXITY_REDUCTION_PROTOCOL.md` - CYC ≤ 8 enforcement
- `CODEFACTOR_PROTOCOL.md` - CodeFactor integration rules
- `CODESCENE_INTEGRATION.md` - Hotspot analysis workflow

**Update Trigger**: When workflow changes

---

## Workflow Documentation (`docs/workflow/`)

**Purpose**: Git, branch, and collaboration workflows

**Key Documents**:
- `AUTONOMOUS_GITBUTLER_WORKFLOW.md` - Virtual branch management
- `BATCH_COMMIT_STRATEGY.md` - Commit batching rules
- `BRANCH_SYNC_PROTOCOL.md` - Branch synchronization
- `LOOP_ORCHESTRATION.md` - PR loop automation

**Update Trigger**: When git workflow changes

---

## MCP Documentation (`docs/mcp/`)

**Purpose**: Model Context Protocol server configurations

**Key Documents**:
- `CUBIC_JANE_STREET_CONFIG.md` - Cubic MCP setup
- `GREPTILE_MCP_TROUBLESHOOTING.md` - Greptile debugging
- `MULTI_TOOL_REVIEW_WORKFLOW.md` - Multi-tool review process

**Update Trigger**: When MCP servers added/modified

---

## Session Tracking (`docs/brain/`)

### Autonomous Refactor Session
**Files**:
- `autonomous_refactor_session.json` - Session metadata
- `autonomous_refactor_progress.md` - Epic queue and log
- `nexus_a2a.json` - Agent-to-agent handoffs

**Update Frequency**: After each epic completion/failure

### Forensic Reports
**Purpose**: Post-mortem analysis of epic failures

**Template**:
```markdown
# FORENSIC REPORT: EPIC-X

## Executive Summary
[One-paragraph summary]

## Root Cause
[Detailed root cause analysis]

## Prevention
[Safeguards to prevent recurrence]

## Lessons Learned
[Key takeaways]
```

**Storage**: Firebase `learnings` collection (auto-captured)

---

## Markdown Standards

### Headings
- Use ATX-style headings (`#`, `##`, `###`)
- One H1 per document
- No skipping heading levels

### Links
- Use relative paths: `[text](../path/to/file.md)`
- Verify links: `powershell -File .\scripts\verify_links.ps1`

### Code Blocks
- Always specify language: ` ```csharp`, ` ```bash`, ` ```json`
- Use syntax highlighting

### Tables
- Use GitHub-flavored Markdown tables
- Align columns with pipes

---

## Documentation Workflow

### Before Epic
1. Create `docs/brain/EPIC-X/` directory
2. Generate `00-scope.md` via `/epic-intake`
3. Generate `01-analysis.md` and `02-approach.md` via `/epic-plan`

### During Epic
1. Update ticket files as work progresses
2. Document decisions in `02-approach.md`
3. Track blockers in `EXECUTION_GUIDE.md`

### After Epic Success
1. Update `autonomous_refactor_progress.md`
2. Archive epic directory (keep for reference)

### After Epic Failure
1. Generate `FORENSIC_REPORT.md`
2. Auto-capture lesson to Firebase
3. Update `CANCELLATION_NOTICE.md`

---

## Common Pitfalls

### ❌ Stale Documentation
**Problem**: Docs don't reflect current code state
**Solution**: Update docs immediately after code changes

### ❌ Missing Forensic Reports
**Problem**: Epic fails without post-mortem
**Solution**: Automated via `.bob/hooks/after_epic_failure.py`

### ❌ Broken Links
**Problem**: Links point to moved/deleted files
**Solution**: Run `verify_links.ps1` before commit

---

## Index

**Parent**: [`../AGENTS.md`](../AGENTS.md) (root)
**Children**: None (leaf node)
**Related**:
- [`../src/AGENTS.md`](../src/AGENTS.md) - Source code rules
- [`../scripts/AGENTS.md`](../scripts/AGENTS.md) - Tooling rules