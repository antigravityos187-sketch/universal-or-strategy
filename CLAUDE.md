# CLAUDE.md - V12 Universal OR Strategy

> Bob IDE reads this file in every session. Keep it authoritative and Bob-native.
> Single-agent model: Bob IDE is the sole agent. No external agents (Codex, Jules, Gemini, etc.).

## Project Overview

**Universal OR Strategy (V12)**: A high-integrity institutional fleet trading strategy for NinjaTrader 8.
- Language: C# 8.0 on .NET Framework 4.8
- Platform: NinjaTrader 8 / Apex / Rithmic
- Architecture: Lock-free Actor/FSM pattern, deterministic single-threaded state machines

## Session Protocol

Bob IDE is the **sole agent** for all work. Bob handles planning, architecture, surgical
`src/` edits, wave orchestration, verification, commits, and PR management end-to-end.

**Mode routing:**
```
Is task modifying code?
├─ YES → Is task in src/?
│  ├─ YES → Use v12-engineer (Bob CLI custom mode)
│  └─ NO  → Use agent mode
└─ NO  → Use ask mode or plan mode
```

See `BOB.md` for full mode table and `AGENTS.md` for complete agent protocol.

## V12 DNA Mandates (Non-Negotiable)

### 1. Lock-Free Actor Pattern
- **BANNED**: `lock(stateLock)` blocks in any form
- **REQUIRED**: FSM/Actor `Enqueue` model or atomic primitives (`Interlocked`, `volatile`)
- **Audit**: `grep -r "lock(" src/` must return zero matches

### 2. ASCII-Only in All C# String Literals
- **BANNED**: Unicode, emoji, curly quotes, em-dashes, box-drawing in `Print()` or any string literal
- **Why**: Non-ASCII inside C# strings breaks the NinjaTrader compiler with 300+ cascading errors
- **Allowed**: `(!)` not emoji, `--` not em-dash, `->` not arrow, straight `"` not curly `"`

### 3. Cyclomatic Complexity <= 8 (Jane Street Standard)
- **Threshold**: CYC <= 8 per method (strict — not Codacy's 15)
- **Why**: Functions >8 are harder to reason about under microsecond latency, test exhaustively,
  and audit for race conditions in lock-free code
- **Audit**: `python scripts/complexity_audit.py --threshold 8`

### 4. Correctness by Construction
- "Make illegal states unrepresentable"
- Structure types/enums so the compiler prevents invalid states
- Avoid runtime if/else guards for edge cases — design them out architecturally

### 5. Post-Edit Deployment (NEVER SKIP)
After ANY `src/` file edit:
1. Run `powershell -File .\deploy-sync.ps1` — re-establishes hard links
2. Instruct Director: "Press F5 in NinjaTrader to compile"
3. Verify BUILD_TAG in banner

### 6. MOVE-SYNC / Follower Replace FSM
- Any follower order cancel+resubmit MUST use the two-phase Replace FSM (`_followerReplaceSpecs`)
- NEVER cancel+submit directly — creates ghost orders
- FSM states: `PendingCancel` -> confirm via `OnAccountOrderUpdate` -> `Submitting` -> submit replacement
- `ChangeOrder` banned on Apex/Tradovate (silently no-ops)

### 7. IPC Security
- All listeners bind to Loopback (`127.0.0.1`) only
- Malformed input rejected with `V12 IPC REJECT` logs
- Fleet accounts obscured via BMad aliases (`F01`, `F02`, etc.) in external responses

### 8. REAPER Bounds
- Repairs capped by both ATR-volatility and hard tick fences
- Ghost-Order Prevention: Signed Delta Rollbacks only — never blanket zeroing
- Symmetry Gating: Follower brackets wait for master "Anchor" price before submission

## Code Quality Gates (Pre-Push)

Run before every push: `powershell -File .\scripts\pre_push_validation.ps1`

| Check | Tool | Threshold | Blocking? |
|-------|------|-----------|-----------|
| ASCII-Only | PowerShell | Zero non-ASCII | YES |
| Build | dotnet build | Zero errors | YES |
| Unit Tests | dotnet test | 100% pass | YES |
| Lint | Roslyn | Zero violations | YES |
| Formatting | CSharpier | Zero issues | YES |
| PR Hygiene | verify_pr_hygiene.ps1 | Diff <10k chars | YES |
| Complexity | complexity_audit.py | CYC <= 8 | YES |

Format code: `dotnet csharpier format src/`
Hard-link sync: `powershell -File .\deploy-sync.ps1`

## Graphify (Mandatory Knowledge Layer)

Graph lives at `.graphify/` (NOT `graphify-out/` — that path is legacy).

**Every task startup:**
1. Check freshness: `git rev-parse HEAD` vs SHA in `.graphify/graph.json`
2. If stale (SHA mismatch): `graphify update . --no-cluster --no-description` (~19s)
3. Read `.graphify/GRAPH_REPORT.md` for god nodes and community structure

**Every task shutdown (after any file edits):**
- Run `graphify update . --no-cluster --no-description`

**Scoped queries (preferred — much cheaper than full report):**
```bash
graphify query "<question>"
graphify path "<SymbolA>" "<SymbolB>"
graphify explain "<concept>"
graphify summary --graph .graphify/graph.json
```

## Jane Street Knowledge Base (OKF)

Local wiki at `docs/intel/jane-street/`. Read the index first:
- [`docs/intel/jane-street/index.md`](docs/intel/jane-street/index.md) — topic map

Key documents:
- `complexity-reduction.md` — CYC <= 8 extraction patterns (Phase 2 + Phase 5)
- `lock-free-patterns.md` — Actor/Enqueue mandate (Phase 3 DNA + Phase 5.V)
- `testing-strategies.md` — xUnit [Fact] only, never NUnit/MSTest
- `how-to-build-an-exchange.md` — FSM determinism, one_in_flight, sidecar_lifecycle
- `microsecond-eternity.md` — zero-alloc, JIT warmup, cache alignment hot path

Query script: `python scripts/query_kb.py "<term>"`

**MANDATORY**: Query Jane Street KB before Phase 2 (architecture planning) and Phase 5 (execution).

## jCodemunch Code Navigation

Always use jCodemunch-MCP tools for code exploration. Only fall back to `read_file` for a file you
are about to edit (harness requires a read before write).

Opening move: `mcp__jcodemunch-mcp__plan_turn` with `model="<your-model-id>"` and `repo="."`.

| Task | Tool |
|------|------|
| Find symbol | `search_symbols` (add `kind`, `language`, `file_pattern` to narrow) |
| Read symbol body | `get_symbol_source` |
| Symbol + imports | `get_context_bundle` |
| Repo structure | `get_repo_outline`, `get_file_tree` |
| What imports this | `find_importers` |
| Where is this used | `find_references` |
| What breaks if changed | `get_blast_radius` |
| Dead code | `find_dead_code`, `get_dead_code_v2` |
| After editing files | `register_edit` (invalidates caches) |

## Session-Aware Routing

```
plan_turn(repo=".", query="<task>", model="<your-model-id>")
```
Obey confidence:
- `high` → go directly to recommended symbols, max 2 supplementary reads
- `medium` → explore recommended files, max 5 supplementary reads
- `low` → feature likely doesn't exist. Report gap. Do NOT search further.

## Documentation Hardening (V12.20)

- Any doc/artifact exceeding 500 lines MUST be modularized into sub-files
- Use a parent index file pointing to child modules
- After writing any artifact >200 lines, verify file size (`ls`) before reporting done
- Skipping modularization for large scopes is a protocol violation

## Karpathy Behavioral Protocols

- **State assumptions** explicitly. If uncertain, ASK.
- **Minimum code** — no features beyond what was asked, no speculative abstractions
- **Surgical changes** — touch only what you must, never mutate whitespace across files
- **Goal-driven** — define verify criteria before each step; "make it work" is not a criterion
- **Diff limit** — PRs must target <10,000 characters of `src/` changes; split larger epics
