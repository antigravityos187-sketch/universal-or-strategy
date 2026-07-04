# Bob IDE Reference (V12 Project Mirror)

> Official Bob documentation compiled for V12 agent routing.
> Pattern: root-level agent file like CODEX.md, JULES.md, GEMINI.md.
> Source: Bob IDE 2.0 (V12.18+). Last Updated: 2026-06-24.

---

## Bob Mode Routing

| Task Type | Bob Mode |
|-----------|----------|
| Code changes outside `src/` | `agent` (built-in) |
| Surgical refactoring in `src/` | `v12-engineer` (custom) |
| Architecture planning, markdown docs | `plan` (built-in) |
| Analysis, Q&A, no edits | `ask` (built-in) |
| Wave execution — top-level coordinator | `autonomous-refactor` (custom) |
| Wave execution — Tier 2 phase orchestrators | `wave-orch-phase*` (custom, via `start_subtask`) |
| Wave execution — Tier 3 per-epic workers | see table below |
| Ad-hoc interactive epic planning | `v12-epic-planner` (custom) |
| Concurrency / Phase 7 tasks | `v12-phase7-lead` (custom) |

### Wave Tier 3 Worker Modes

| Phase | Slug | Spawned via |
|-------|------|-------------|
| Phase 0 — Hotspot Analysis | `v12-phase0-hotspot` | `spawn_subagent` |
| Phase 1 — Scope Definition | `v12-phase1-scope` | `spawn_subagent` |
| Phase 1.5 — Boundary Validation | `v12-phase1-5-boundary` | `spawn_subagent` |
| Phase 2 — Architecture Planning | `v12-phase2-architecture` | `start_subtask` (needs MCP) |
| Phase 3 — DNA Audit | `v12-phase3-audit` | `start_subtask` (needs MCP) |
| Phase 4 — Ticket Generation | `v12-phase4-tickets` | `spawn_subagent` |
| Phase 4.5 — Ticket Review | `v12-phase4-5-review` | `start_subtask` (needs MCP) |
| Phase 5 — Ticket Execution | `v12-engineer` | `spawn_subagent` |
| Phase 5.V — Verification | `v12-phase5-v-verify` | `spawn_subagent` |
| Phase 6 — Final Review | `v12-phase6-review` | `spawn_subagent` |

Bob CLI binary: `bob` (alias or path).
Custom mode config: `.bob/custom_modes.yaml`.
Custom rules: `.bob/rules-{mode-slug}/` (directory, alphabetical load order).

---


## V12.20: Documentation & Output Hardening (MANDATORY)
... (existing content) ...

## V12.21: Internal Sentinel Protocol (MANDATORY)
- **Role Separation**: Spawned sub-agents (via `start_subtask`) serve as Internal Sentinels for Phase 2.3 (Planning Scan) and Phase 5 (Implementation Verification).
- **Sovereign Loop**: Verification tasks must be handled internally by spawned agents. Never delegate verification to external CLI instances.
- **PHS Authority**: Only the spawned Internal Sentinel can award a 100/100 PHS.

---

## 1. Modes

Bob IDE 2.0 has **three** built-in modes plus custom modes.

### Built-In Mode Table

| Mode | Tool Access | Primary Use |
|------|------------|-------------|
| **Agent** | read, edit, command, mcp, browser, subagents | Default for all code changes; replaces former Code + Advanced |
| **Ask** | read, browser, mcp | Analysis, explanations — no file edits |
| **Plan** | read, edit (markdown only), browser, mcp | Architecture planning, specs before implementation |

> **Removed in Bob IDE 2.0**: `Code` mode, `Advanced` mode, `Orchestrator` mode. All three are gone.
> Use `agent` mode for anything that formerly required `code` or `advanced`.
> For multi-phase wave orchestration, use the `autonomous-refactor` custom mode.

### Mode Routing Decision Tree

```
Is task modifying code?
├─ YES → Is task in src/?
│  ├─ YES → Use Bob CLI (v12-engineer custom mode)
│  └─ NO  → Use agent mode
└─ NO  → Use ask mode or plan mode
```

### Switching Modes

- Drop-down menu left of chat input
- Slash prefix: `/plan`, `/ask`, `/agent`
- Keyboard: Ctrl+. (Windows/Linux) to cycle modes

---

## 2. Custom Modes

Custom modes are specialized personas with specific tool access and behavioral rules.

### Configuration File

`.bob/custom_modes.yaml` (project-level) or `~/.bob/custom_modes.yaml` (global).

### Mode YAML Schema

```yaml
customModes:
  - slug: my-mode-slug        # unique ID; used for rules file naming
    name: Display Name
    roleDefinition: |
      Describe the persona and primary responsibilities here.
    whenToUse: |
      When to invoke this mode.
    customInstructions: |
      Additional behavioral rules merged with rules file content.
    groups:
      - read
      - - edit
        - fileRegex: "^docs/"   # restrict edits to docs/ only
          description: Planning docs only
      - execute
      - mcp
      - browser
      - skill
      - todo
      - subtask
      - subagent
```

### Tool Groups

| Group | Capability |
|-------|-----------|
| read | Read files, list directories |
| edit | Write/modify files (add fileRegex to restrict paths) |
| execute | Run shell commands |
| mcp | Call MCP server tools |
| browser | Web browsing |
| skill | Use skills |
| todo | Update todo list |
| subtask | start_subtask (sequential MCP-capable subagents) |
| subagent | spawn_subagent (parallel workers) |

### V12 Custom Modes (Active — `.bob/custom_modes.yaml`)

| Slug | Name | Phase | Execution Model |
|------|------|-------|-----------------|
| `v12-phase0-hotspot` | V12 Phase 0 Hotspot Analyzer | Phase 0 worker | spawn_subagent general |
| `v12-phase1-scope` | V12 Phase 1 Scope Analyzer | Phase 1 worker | spawn_subagent general |
| `v12-phase1-5-boundary` | V12 Phase 1.5 Boundary Validator | Phase 1.5 worker | spawn_subagent general |
| `v12-phase2-architecture` | V12 Phase 2 Architecture Planner | Phase 2 worker | start_subtask (MCP) |
| `v12-phase3-audit` | V12 Phase 3 DNA Auditor | Phase 3 worker | start_subtask (MCP) |
| `v12-phase4-tickets` | V12 Phase 4 Ticket Generator | Phase 4 worker | spawn_subagent general |
| `v12-phase4-5-review` | V12 Phase 4.5 Ticket Reviewer | Phase 4.5 worker | start_subtask (MCP) |
| `v12-engineer` | V12 Photon Engineer | Phase 5 execution | spawn_subagent general |
| `v12-phase5-v-verify` | V12 Phase 5.V Verifier | Phase 5.V verification | spawn_subagent general |
| `v12-phase6-review` | V12 Phase 6 Final Reviewer | Phase 6 worker | spawn_subagent general |
| `v12-epic-planner` | V12 Epic Planner | Interactive planning | interactive only |
| `v12-phase7-lead` | Phase 7 Concurrency Lead | Concurrency tasks | ad hoc |
| `wave-orch-phase0` | Phase 0 Orchestrator | Tier 2 — Phase 0 | start_subtask from Tier 1 |
| `wave-orch-phase1` | Phase 1 Orchestrator | Tier 2 — Phase 1 | start_subtask from Tier 1 |
| `wave-orch-phase1-5` | Phase 1.5 Orchestrator | Tier 2 — Phase 1.5 | start_subtask from Tier 1 |
| `wave-orch-phase2` | Phase 2 Orchestrator | Tier 2 — Phase 2 | start_subtask from Tier 1 |
| `wave-orch-phase3` | Phase 3 Orchestrator | Tier 2 — Phase 3 | start_subtask from Tier 1 |
| `wave-orch-phase4` | Phase 4 Orchestrator | Tier 2 — Phase 4 | start_subtask from Tier 1 |
| `wave-orch-phase4-5` | Phase 4.5 Orchestrator | Tier 2 — Phase 4.5 | start_subtask from Tier 1 |
| `wave-orch-phase5` | Phase 5 Orchestrator | Tier 2 — Phase 5 | start_subtask from Tier 1 |
| `wave-orch-phase5v` | Phase 5.V Orchestrator | Tier 2 — Phase 5.V | start_subtask from Tier 1 |
| `wave-orch-phase6` | Phase 6 Orchestrator | Tier 2 — Phase 6 | start_subtask from Tier 1 |
| `autonomous-refactor` | Autonomous Refactor | Tier 1 wave coordinator | top-level entry point |

---

## 3. Slash Commands

Custom slash commands live in `.bob/commands/` (project) or `~/.bob/commands/` (global).
Each command is a `.md` file. Fuzzy search and autocomplete available via `/` in chat.

### Frontmatter

```markdown
---
description: Short description shown in the command picker
argument-hint: <arg1> <arg2>
---
# Command Title
$1 = first argument, $2 = second argument
```

### Active V12 Commands

| Command | File | Purpose |
|---------|------|---------|
| `/epic-intake` | `.bob/commands/epic-intake.md` | Phase 0: Hotspot analysis |
| `/epic-scope-boundary` | `.bob/commands/epic-scope-boundary.md` | Phase 1 / 1.5: Scope definition + validation |
| `/epic-plan` | `.bob/commands/epic-plan.md` | Phase 2: Architecture planning |
| `/epic-scan` | `.bob/commands/epic-scan.md` | Phase 3: DNA & PR audit |
| `/epic-tickets` | `.bob/commands/epic-tickets.md` | Phase 4: Ticket generation |
| `/epic-validate` | `.bob/commands/epic-validate.md` | Phase 5: Ticket execution |
| `/epic-verify-ticket` | `.bob/commands/epic-verify-ticket.md` | Phase 5.V: Per-ticket verification |
| `/epic-review-final` | `.bob/commands/epic-review-final.md` | Phase 6: Final review |
| `/epic-review-tickets` | `.bob/commands/epic-review-tickets.md` | Ticket review pass |
| `/ticket` | `.bob/commands/ticket.md` | Single ticket execution |
| `/autonomous-refactor` | `.bob/commands/autonomous-refactor.md` | Wave-level YOLO orchestration |
| `/pr-loop` | `.bob/commands/pr-loop.md` | PR health score loop (drive to 100/100) |
| `/pre-push` | `.bob/commands/pre-push.md` | Pre-push validation (13 checks) |
| `/epic-loop` | `.bob/commands/epic-loop.md` | Epic loop automation |
| `/epic-orchestrate` | `.bob/commands/epic-orchestrate.md` | Multi-epic orchestration |
| `/local-loop` | `.bob/commands/local-loop.md` | Local development loop |
| `/mcp-loop` | `.bob/commands/mcp-loop.md` | MCP verification loop |
| `/sync` | `.bob/commands/sync.md` | VM sync |
| `/continue` | `.bob/commands/continue.md` | Resume interrupted session |
| `/optimize` | `.bob/commands/optimize.md` | Optimization pass |
| `/extract` | `.bob/commands/extract.md` | Method extraction |
| `/phase7` | `.bob/commands/phase7.md` | Phase 7 concurrency tasks |

> **Deprecated**: `/epic-run` — replaced by individual phase commands. Do not use.

Built-in commands: `/init`, `/review`, `/compact`, `/help`.

---

## 4. Custom Rules

Rules files inject behavioral constraints into a mode automatically.

### File Naming Convention

| Location | General rules | Mode-specific rules |
|----------|--------------|-------------------|
| Project | `.bob/rules/` | `.bob/rules-{mode-slug}/` |
| Global | `~/.bob/rules/` | `~/.bob/rules-{mode-slug}/` |

Directory method is preferred. Single-file alternative: `.bobrules-{mode-slug}`.

Files load alphabetically within each directory. Mode-specific rules load before general rules.
All files in a directory are read recursively. Empty files are silently skipped.

### Rule Priority (High to Low)

1. Global rules (`~/.bob/rules/`)
2. Workspace rules (`.bob/rules/`)
3. Within each: mode-specific before general; workspace overrides global

### AGENTS.md Loading

Bob automatically loads `AGENTS.md` from workspace root after mode-specific rules.
Disable with `"bob-code.useAgentRules": false` in settings.

### V12 Active Rules Files

```
.bob/rules/
  00-pr-hygiene.md             # PR rebase mandate + hygiene script
  01-mode-enforcement.md       # agent/plan/ask only (code+advanced+orchestrator banned)
  02-vm-context-awareness.md   # GCP VM context — no scp/ssh
  99-powershell-syntax.md      # PowerShell cwd parameter mandate

.bob/rules-v12-epic-planner/
  01-planning-protocol.md      # Enforces docs/-only, DNA compliance, gate protocol

.bob/rules-v12-engineer/
  00-epic-readiness-checklist.md  # Pre-execution checklist
  99-jane-street-auto.md          # Automatic Jane Street KB loading
  SKILL-knowledge-management.md  # KB management rules
  branch-guard.md                 # GitButler branch strategy enforcement
  dna.md                          # Lock-free, ASCII-only, deploy-sync requirements
```

---

## 5. Code Actions

Code actions appear as a lightbulb icon in the editor gutter when code is selected.

| Action | Description | Shortcut |
|--------|-------------|---------|
| Add to Context | Adds code + file path/line numbers to chat | First in menu |
| Explain Code | Asks Bob to explain selection | Second |
| Improve Code | Asks Bob to suggest improvements | Third |
| Inline Chat | Opens chat at cursor position | Ctrl+K (Win) |
| Move to Chat | Sends selection to chat panel with context | Ctrl+L (Win) |

Context mention format: `@myFile.cs:15:25` (file:startLine:endLine).
Use line ranges for targeted context to minimize token consumption.

---

## 6. Checkpoints

Bob automatically creates a checkpoint before every file modification.
Uses a shadow Git repository separate from main version control.
No commands needed -- checkpoints are fully automatic.

### Key Facts

- Created BEFORE file modifications (not before commands)
- Task-scoped: checkpoints belong to the task that created them
- Not created for external edits (manual saves, other tools)
- Large binary files may impact performance

### Restore Options (via Chat UI)

| Option | Effect |
|--------|--------|
| Restore files | Reverts workspace files only; keeps chat history |
| Restore files & task | Reverts files AND removes subsequent conversation messages (irreversible) |

### What Checkpoints Do NOT Cover

- Shell command output (only file mutations)
- Files excluded by `.gitignore` or `.bobignore`
- External changes made outside Bob tasks

### V12 Workflow Implication

The checkpoint safety net means no need for manual checkpoint commands in epic workflows.
If a ticket edit goes wrong, Director restores from checkpoint via UI before the next ticket.

---

## 7. Context Window Management

Bob's context window: **200,000 tokens total**.
Reserved for responses: ~50,000 tokens.
Effective usable window: ~150,000 tokens.

### Quality Thresholds

| Threshold | Effect |
|-----------|--------|
| ~100k tokens | Quality noticeably degrades; responses become less precise |
| 140k tokens | Auto-condensation triggers (lossy -- edge cases may be lost) |
| 200k tokens | Hard limit |

### What Consumes Tokens

- System instructions + mode rules (always present)
- Full conversation history (every message, tool call, result)
- File contents via `@` mentions
- MCP tool definitions (each connected server adds tokens)
- Bob's own responses

### Best Practices

- Start a new chat when switching tasks -- do not let unrelated context accumulate
- Use `@file:startLine-endLine` for targeted mentions, not whole directories
- Only connect MCP servers you actively use (each adds token overhead)
- For large files, reference only the relevant section
- Break complex tasks into focused sub-sessions

### V12 Epic Session Strategy

```
Planning session (phases 1-4): stays under 100k for most epics
Execution session: fresh session per batch of 3-4 tickets
Resume state: EXECUTION_GUIDE.md carries all context between sessions
Rule: split planning and execution for any epic with > 3 tickets
```

---

## 8. Context Poisoning

Context poisoning = inaccurate or irrelevant data contaminating the active context.
Once poisoned, the context cannot be reliably repaired with prompts. Only a new session fixes it.

### Symptoms

- Degraded output quality (nonsensical, repetitive, irrelevant suggestions)
- Tool misalignment (tool calls don't match requests)
- **Orchestration failures: chains stall, loop indefinitely, or fail to complete**
- Temporary fixes work briefly then revert
- Tool usage confusion (Bob forgets how to use tools from system prompt)

### Common Causes

- Model hallucination treated as factual context in subsequent turns
- Outdated or incorrect code comments misinterpreted
- Pasted logs containing hidden control characters
- Context window overflow causing poisoned data to dominate

### Recovery

**No prompt reliably fixes context poisoning.** The corrupted text persists in session history.

Recovery sequence:
1. Abandon the current session
2. Start a new session
3. Load resume state from `EXECUTION_GUIDE.md` or the relevant ticket file
4. Continue from the last confirmed-complete step

### V12 Wave Orchestration Red Flags (Stop Immediately)

- Orchestrator re-runs a phase it already completed
- Gate questions reference wrong epic slug or wrong ticket number
- Sub-task brief points to non-existent file paths
- Any of the above: STOP, save progress to `EXECUTION_GUIDE.md` or Lamport log, start fresh session

---

## 9. Session Management Summary for V12 Epics

```
DO:
  - Start a new session for each distinct epic
  - Split planning and execution into separate sessions (> 3 tickets)
  - Use @file:line-range for targeted context
  - Let checkpoints handle rollback (no manual checkpoint commands)
  - Watch for context poisoning signals in long orchestration sessions
  - Resume from EXECUTION_GUIDE.md after any session restart
  - Use agent mode for all non-src code work
  - Run `graphify update . --no-cluster --no-description` at START of every task
  - Run `graphify update . --no-cluster --no-description` at END of every task (after edits)
  - Read `.graphify/GRAPH_REPORT.md` after startup update for god nodes + community structure

DO NOT:
  - Run all tickets for a large epic in one session
  - Use broad @dir mentions
  - Try to "wake up" a poisoned session with corrective prompts
  - Leave unused MCP servers connected (each adds token overhead)
  - Reference `graphify-out/` — that path is legacy, graph is now at `.graphify/`
```

---

_Last Updated: 2026-06-24 (Bob IDE 2.0 — built-in modes are agent, plan, ask)_
