# Project Workflow (V12 Bob IDE Protocol)

## Guiding Principles

1. **Bob IDE is the sole agent**: Planning, architecture, surgical `src/` edits, wave
   orchestration, verification, commits, and PR management are all handled by Bob end-to-end.
   No external agents (Codex, Jules, Gemini, Arena AI, Antigravity) are used.
2. **YOLO Mode**: Bob operates autonomously. All tools are pre-approved. No manual
   confirmation gates between phases unless explicitly required by the Director.
3. **Continuous Safety**: Zero internal locks (`lock(stateLock)` is BANNED). Mandatory
   ASCII-only C# strings. CYC <= 8 per method.

## Bob Mode Routing

```
Is task modifying code?
├─ YES → Is task in src/?
│  ├─ YES → v12-engineer (Bob CLI custom mode)
│  └─ NO  → agent mode
└─ NO  → ask mode or plan mode
```

See `BOB.md` for full mode table and custom mode list.

## Standard Task Workflow

1. **Startup**: Run `graphify update . --no-cluster --no-description` (if graph is stale).
   Read `.graphify/GRAPH_REPORT.md` for god nodes and community structure.
2. **Plan**: Use `plan_turn` (jCodemunch) to orient on the task before touching files.
3. **Execute**: Apply surgical changes using Bob's file-edit tools.
4. **Self-Audit**:
   - Zero `lock(stateLock)` usage: `grep -r "lock(" src/`
   - ASCII safety: `python scripts/ascii_audit.py src/`
   - Complexity: `python scripts/complexity_audit.py --threshold 8`
5. **Deploy sync** (after any `src/` edit): `powershell -File .\deploy-sync.ps1`
6. **Shutdown**: Run `graphify update . --no-cluster --no-description` after any file edits.
7. **Commit**: Semantic commit message format -- `[EPIC-W7-NNN] ticket-N: description [BUILD_TAG]`

## Epic Workflow (Manifest-Based)

All epics follow the manifest-based independent subtask architecture.
Each phase is a fresh session -- no context exhaustion.

```
Phase 0   epic-intake           Hotspot analysis
Phase 1   epic-scope-boundary   Scope definition
Phase 1.5 epic-scope-boundary   Scope boundary validation
Phase 2   epic-plan             Architecture planning
Phase 3   epic-scan             DNA & PR audit
Phase 4   epic-tickets          Ticket generation
Phase 5   epic-validate         Ticket execution (v12-engineer)
Phase 5.V epic-verify-ticket    Per-ticket verification
Phase 6   epic-review-final     Final review
```

State tracked in `docs/brain/EPIC-{ID}/manifest.json`.
Reference: `docs/workflow/V12_EPIC_WORKFLOW_REFACTORING_DESIGN.md`

## Deployment & Verification

- **Hard-link sync**: `powershell -File .\deploy-sync.ps1` after every `src/` edit
- **Compile**: F5 in NinjaTrader IDE -- verify BUILD_TAG in output
- **Pre-push**: `powershell -File .\scripts\pre_push_validation.ps1`

## Commit Guidelines

Semantic commit messages:
```
[EPIC-W7-NNN] ticket-N: description [BUILD_TAG]
feat(ipc): Add sub-minute deduplication for Fleet entry commands
fix(fsm): Resolve ghost-order race in PendingCancel state
```
