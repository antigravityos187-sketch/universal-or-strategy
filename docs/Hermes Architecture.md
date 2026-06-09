Architecture
This page is the top-level map of Hermes Agent internals. Use it to orient yourself in the codebase, then dive into subsystem-specific docs for implementation details.

System Overview
┌─────────────────────────────────────────────────────────────────────┐
│                        Entry Points                                  │
│                                                                      │
│  CLI (cli.py)    Gateway (gateway/run.py)    ACP (acp_adapter/)     │
│  Batch Runner    API Server                  Python Library          │
└──────────┬──────────────┬───────────────────────┬───────────────────┘
           │              │                       │
           ▼              ▼                       ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     AIAgent (run_agent.py)                          │
│                                                                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐               │
│  │ Prompt       │  │ Provider     │  │ Tool         │               │
│  │ Builder      │  │ Resolution   │  │ Dispatch     │               │
│  │ (prompt_     │  │ (runtime_    │  │ (model_      │               │
│  │  builder.py) │  │  provider.py)│  │  tools.py)   │               │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘               │
│         │                 │                 │                       │
│  ┌──────┴───────┐  ┌──────┴───────┐  ┌──────┴───────┐               │
│  │ Compression  │  │ 3 API Modes  │  │ Tool Registry│               │
│  │ & Caching    │  │ chat_compl.  │  │ (registry.py)│               │
│  │              │  │ codex_resp.  │  │ 70+ tools    │               │
│  │              │  │ anthropic    │  │ 28 toolsets  │               │
│  └──────────────┘  └──────────────┘  └──────────────┘               │
└─────────┴─────────────────┴─────────────────┴───────────────────────┘
           │                                    │
           ▼                                    ▼
┌───────────────────┐              ┌──────────────────────┐
│ Session Storage   │              │ Tool Backends         │
│ (SQLite + FTS5)   │              │ Terminal (6 backends) │
│ hermes_state.py   │              │ Browser (5 backends)  │
│ gateway/session.py│              │ Web (4 backends)      │
└───────────────────┘              │ MCP (dynamic)         │
                                   │ File, Vision, etc.    │
                                   └──────────────────────┘

Directory Structure
hermes-agent/
├── run_agent.py              # AIAgent — core conversation loop (large file)
├── cli.py                    # HermesCLI — interactive terminal UI (large file)
├── model_tools.py            # Tool discovery, schema collection, dispatch
├── toolsets.py               # Tool groupings and platform presets
├── hermes_state.py           # SQLite session/state database with FTS5
├── hermes_constants.py       # HERMES_HOME, profile-aware paths
├── batch_runner.py           # Batch trajectory generation
│
├── agent/                    # Agent internals
│   ├── prompt_builder.py     # System prompt assembly
│   ├── context_engine.py     # ContextEngine ABC (pluggable)
│   ├── context_compressor.py # Default engine — lossy summarization
│   ├── prompt_caching.py     # Anthropic prompt caching
│   ├── auxiliary_client.py   # Auxiliary LLM for side tasks (vision, summarization)
│   ├── model_metadata.py     # Model context lengths, token estimation
│   ├── models_dev.py         # models.dev registry integration
│   ├── anthropic_adapter.py  # Anthropic Messages API format conversion
│   ├── display.py            # KawaiiSpinner, tool preview formatting
│   ├── skill_commands.py     # Skill slash commands
│   ├── memory_manager.py    # Memory manager orchestration
│   ├── memory_provider.py   # Memory provider ABC
│   └── trajectory.py         # Trajectory saving helpers
│
├── hermes_cli/               # CLI subcommands and setup
│   ├── main.py               # Entry point — all `hermes` subcommands (large file)
│   ├── config.py             # DEFAULT_CONFIG, OPTIONAL_ENV_VARS, migration
│   ├── commands.py           # COMMAND_REGISTRY — central slash command definitions
│   ├── auth.py               # PROVIDER_REGISTRY, credential resolution
│   ├── runtime_provider.py   # Provider → api_mode + credentials
│   ├── models.py             # Model catalog, provider model lists
│   ├── model_switch.py       # /model command logic (CLI + gateway shared)
│   ├── setup.py              # Interactive setup wizard (large file)
│   ├── skin_engine.py        # CLI theming engine
│   ├── skills_config.py      # hermes skills — enable/disable per platform
│   ├── skills_hub.py         # /skills slash command
│   ├── tools_config.py       # hermes tools — enable/disable per platform
│   ├── plugins.py            # PluginManager — discovery, loading, hooks
│   ├── callbacks.py          # Terminal callbacks (clarify, sudo, approval)
│   └── gateway.py            # hermes gateway start/stop
│
├── tools/                    # Tool implementations (one file per tool)
│   ├── registry.py           # Central tool registry
│   ├── approval.py           # Dangerous command detection
│   ├── terminal_tool.py      # Terminal orchestration
│   ├── process_registry.py   # Background process management
│   ├── file_tools.py         # read_file, write_file, patch, search_files
│   ├── web_tools.py          # web_search, web_extract
│   ├── browser_tool.py       # 10 browser automation tools
│   ├── code_execution_tool.py # execute_code sandbox
│   ├── delegate_tool.py      # Subagent delegation
│   ├── mcp_tool.py           # MCP client (large file)
│   ├── credential_files.py   # File-based credential passthrough
│   ├── env_passthrough.py    # Env var passthrough for sandboxes
│   ├── ansi_strip.py         # ANSI escape stripping
│   └── environments/         # Terminal backends (local, docker, ssh, modal, daytona, singularity)
│
├── gateway/                  # Messaging platform gateway
│   ├── run.py                # GatewayRunner — message dispatch (large file)
│   ├── session.py            # SessionStore — conversation persistence
│   ├── delivery.py           # Outbound message delivery
│   ├── pairing.py            # DM pairing authorization
│   ├── hooks.py              # Hook discovery and lifecycle events
│   ├── mirror.py             # Cross-session message mirroring
│   ├── status.py             # Token locks, profile-scoped process tracking
│   ├── builtin_hooks/        # Extension point for always-registered hooks (none shipped)
│   └── platforms/            # 20 adapters: telegram, discord, slack, whatsapp,
│                             #   signal, matrix, mattermost, email, sms,
│                             #   dingtalk, feishu, wecom, wecom_callback, weixin,
│                             #   bluebubbles, qqbot, homeassistant, webhook, api_server,
│                             #   yuanbao
│
├── acp_adapter/              # ACP server (VS Code / Zed / JetBrains)
├── cron/                     # Scheduler (jobs.py, scheduler.py)
├── plugins/memory/           # Memory provider plugins
├── plugins/context_engine/   # Context engine plugins
├── skills/                   # Bundled skills (always available)
├── optional-skills/          # Official optional skills (install explicitly)
├── website/                  # Docusaurus documentation site
└── tests/                    # Pytest suite (~25,000 tests across ~1,250 files)


Data Flow
CLI Session
User input → HermesCLI.process_input()
  → AIAgent.run_conversation()
    → prompt_builder.build_system_prompt()
    → runtime_provider.resolve_runtime_provider()
    → API call (chat_completions / codex_responses / anthropic_messages)
    → tool_calls? → model_tools.handle_function_call() → loop
    → final response → display → save to SessionDB

Gateway Message
Platform event → Adapter.on_message() → MessageEvent
  → GatewayRunner._handle_message()
    → authorize user
    → resolve session key
    → create AIAgent with session history
    → AIAgent.run_conversation()
    → deliver response back through adapter

Cron Job
Scheduler tick → load due jobs from jobs.json
  → create fresh AIAgent (no history)
  → inject attached skills as context
  → run job prompt
  → deliver response to target platform
  → update job state and next_run

Recommended Reading Order
If you are new to the codebase:

This page — orient yourself
Agent Loop Internals — how AIAgent works
Prompt Assembly — system prompt construction
Provider Runtime Resolution — how providers are selected
Adding Providers — practical guide to adding a new provider
Tools Runtime — tool registry, dispatch, environments
Session Storage — SQLite schema, FTS5, session lineage
Gateway Internals — messaging platform gateway
Context Compression & Prompt Caching — compression and caching
ACP Internals — IDE integration
Major Subsystems
Agent Loop
The synchronous orchestration engine (AIAgent in run_agent.py). Handles provider selection, prompt construction, tool execution, retries, fallback, callbacks, compression, and persistence. Supports three API modes for different provider backends.

→ Agent Loop Internals

Prompt System
Prompt construction and maintenance across the conversation lifecycle:

system_prompt.py + prompt_builder.py — assembles the ordered system-prompt tiers (stable → context → volatile): identity/tool guidance/skills, context files, then memory/profile/timestamp blocks
prompt_caching.py — Applies Anthropic cache breakpoints for prefix caching
context_compressor.py — Summarizes middle conversation turns when context exceeds thresholds
→ Prompt Assembly, Context Compression & Prompt Caching

Provider Resolution
A shared runtime resolver used by CLI, gateway, cron, ACP, and auxiliary calls. Maps (provider, model) tuples to (api_mode, api_key, base_url). Handles 18+ providers, OAuth flows, credential pools, and alias resolution.

→ Provider Runtime Resolution

Tool System
Central tool registry (tools/registry.py) with 70+ registered tools across ~28 toolsets. Each tool file self-registers at import time. The registry handles schema collection, dispatch, availability checking, and error wrapping. Terminal tools support 6 backends (local, Docker, SSH, Daytona, Modal, Singularity).

→ Tools Runtime

Session Persistence
SQLite-based session storage with FTS5 full-text search. Sessions have lineage tracking (parent/child across compressions), per-platform isolation, and atomic writes with contention handling.

→ Session Storage

Messaging Gateway
Long-running process with 20 platform adapters, unified session routing, user authorization (allowlists + DM pairing), slash command dispatch, hook system, cron ticking, and background maintenance.

→ Gateway Internals

Plugin System
Three discovery sources: ~/.hermes/plugins/ (user), .hermes/plugins/ (project), and pip entry points. Plugins register tools, hooks, and CLI commands through a context API. Two specialized plugin types exist: memory providers (plugins/memory/) and context engines (plugins/context_engine/). Both are single-select — only one of each can be active at a time, configured via hermes plugins or config.yaml.

→ Plugin Guide, Memory Provider Plugin

Cron
First-class agent tasks (not shell tasks). Jobs store in JSON, support multiple schedule formats, can attach skills and scripts, and deliver to any platform.

→ Cron Internals

ACP Integration
Exposes Hermes as an editor-native agent over stdio/JSON-RPC for VS Code, Zed, and JetBrains.

→ ACP Internals

Trajectories
Generates ShareGPT-format trajectories from agent sessions for training data generation.

→ Trajectories & Training Format

Design Principles
Principle	What it means in practice
Prompt stability	System prompt doesn't change mid-conversation. No cache-breaking mutations except explicit user actions (/model).
Observable execution	Every tool call is visible to the user via callbacks. Progress updates in CLI (spinner) and gateway (chat messages).
Interruptible	API calls and tool execution can be cancelled mid-flight by user input or signals.
Platform-agnostic core	One AIAgent class serves CLI, gateway, ACP, batch, and API server. Platform differences live in the entry point, not the agent.
Loose coupling	Optional subsystems (MCP, plugins, memory providers, RL environments) use registry patterns and check_fn gating, not hard dependencies.
Profile isolation	Each profile (hermes -p <name>) gets its own HERMES_HOME, config, memory, sessions, and gateway PID. Multiple profiles run concurrently.
File Dependency Chain
tools/registry.py  (no deps — imported by all tool files)
       ↑
tools/*.py  (each calls registry.register() at import time)
       ↑
model_tools.py  (imports tools/registry + triggers tool discovery)
       ↑
run_agent.py, cli.py, batch_runner.py, environments/

This chain means tool registration happens at import time, before any agent instance is created. Any tools/*.py file with a top-level registry.register() call is auto-discovered — no manual import list needed.

Agent Loop Internals
The core orchestration engine is run_agent.py's AIAgent class — a large file that handles everything from prompt assembly to tool dispatch to provider failover.

Core Responsibilities
AIAgent is responsible for:

Assembling the effective system prompt and tool schemas via prompt_builder.py
Selecting the correct provider/API mode (chat_completions, codex_responses, anthropic_messages)
Making interruptible model calls with cancellation support
Executing tool calls (sequentially or concurrently via thread pool)
Maintaining conversation history in OpenAI message format
Handling compression, retries, and fallback model switching
Tracking iteration budgets across parent and child agents
Flushing persistent memory before context is lost
Two Entry Points
# Simple interface — returns final response string
response = agent.chat("Fix the bug in main.py")

# Full interface — returns dict with messages, metadata, usage stats
result = agent.run_conversation(
    user_message="Fix the bug in main.py",
    system_message=None,           # auto-built if omitted
    conversation_history=None,      # auto-loaded from session if omitted
    task_id="task_abc123"
)

chat() is a thin wrapper around run_conversation() that extracts the final_response field from the result dict.

API Modes
Hermes supports three API execution modes, resolved from provider selection, explicit args, and base URL heuristics:

API mode	Used for	Client type
chat_completions	OpenAI-compatible endpoints (OpenRouter, custom, most providers)	openai.OpenAI
codex_responses	OpenAI Codex / Responses API	openai.OpenAI with Responses format
anthropic_messages	Native Anthropic Messages API	anthropic.Anthropic via adapter
The mode determines how messages are formatted, how tool calls are structured, how responses are parsed, and how caching/streaming works. All three converge on the same internal message format (OpenAI-style role/content/tool_calls dicts) before and after API calls.

Mode resolution order:

Explicit api_mode constructor arg (highest priority)
Provider-specific detection (e.g., anthropic provider → anthropic_messages)
Base URL heuristics (e.g., api.anthropic.com → anthropic_messages)
Default: chat_completions
Turn Lifecycle
Each iteration of the agent loop follows this sequence:

run_conversation()
  1. Generate task_id if not provided
  2. Append user message to conversation history
  3. Build or reuse cached system prompt (prompt_builder.py)
  4. Check if preflight compression is needed (>50% context)
  5. Build API messages from conversation history
     - chat_completions: OpenAI format as-is
     - codex_responses: convert to Responses API input items
     - anthropic_messages: convert via anthropic_adapter.py
  6. Inject ephemeral prompt layers (budget warnings, context pressure)
  7. Apply prompt caching markers if on Anthropic
  8. Make interruptible API call (_interruptible_api_call)
  9. Parse response:
     - If tool_calls: execute them, append results, loop back to step 5
     - If text response: persist session, flush memory if needed, return

Message Format
All messages use OpenAI-compatible format internally:

{"role": "system", "content": "..."}
{"role": "user", "content": "..."}
{"role": "assistant", "content": "...", "tool_calls": [...]}
{"role": "tool", "tool_call_id": "...", "content": "..."}

Reasoning content (from models that support extended thinking) is stored in assistant_msg["reasoning"] and optionally displayed via the reasoning_callback.

Message Alternation Rules
The agent loop enforces strict message role alternation:

After the system message: User → Assistant → User → Assistant → ...
During tool calling: Assistant (with tool_calls) → Tool → Tool → ... → Assistant
Never two assistant messages in a row
Never two user messages in a row
Only tool role can have consecutive entries (parallel tool results)
Providers validate these sequences and will reject malformed histories.

Interruptible API Calls
API requests are wrapped in _interruptible_api_call() which runs the actual HTTP call in a background thread while monitoring an interrupt event:

┌────────────────────────────────────────────────────┐
│  Main thread                  API thread           │
│                                                    │
│   wait on:                     HTTP POST           │
│    - response ready     ───▶   to provider         │
│    - interrupt event                               │
│    - timeout                                       │
└────────────────────────────────────────────────────┘

When interrupted (user sends new message, /stop command, or signal):

The API thread is abandoned (response discarded)
The agent can process the new input or shut down cleanly
No partial response is injected into conversation history
Tool Execution
Sequential vs Concurrent
When the model returns tool calls:

Single tool call → executed directly in the main thread
Multiple tool calls → executed concurrently via ThreadPoolExecutor
Exception: tools marked as interactive (e.g., clarify) force sequential execution
Results are reinserted in the original tool call order regardless of completion order
Execution Flow
for each tool_call in response.tool_calls:
    1. Resolve handler from tools/registry.py
    2. Fire pre_tool_call plugin hook
    3. Check if dangerous command (tools/approval.py)
       - If dangerous: invoke approval_callback, wait for user
    4. Execute handler with args + task_id
    5. Fire post_tool_call plugin hook
    6. Append {"role": "tool", "content": result} to history

Agent-Level Tools
Some tools are intercepted by run_agent.py before reaching handle_function_call():

Tool	Why intercepted
todo	Reads/writes agent-local task state
memory	Writes to persistent memory files with character limits
session_search	Queries session history via the agent's session DB
delegate_task	Spawns subagent(s) with isolated context
These tools modify agent state directly and return synthetic tool results without going through the registry.

Callback Surfaces
AIAgent supports platform-specific callbacks that enable real-time progress in the CLI, gateway, and ACP integrations:

Callback	When fired	Used by
tool_progress_callback	Before/after each tool execution	CLI spinner, gateway progress messages
thinking_callback	When model starts/stops thinking	CLI "thinking..." indicator
reasoning_callback	When model returns reasoning content	CLI reasoning display, gateway reasoning blocks
clarify_callback	When clarify tool is called	CLI input prompt, gateway interactive message
step_callback	After each complete agent turn	Gateway step tracking, ACP progress
stream_delta_callback	Each streaming token (when enabled)	CLI streaming display
tool_gen_callback	When tool call is parsed from stream	CLI tool preview in spinner
status_callback	State changes (thinking, executing, etc.)	ACP status updates
Budget and Fallback Behavior
Iteration Budget
The agent tracks iterations via IterationBudget:

Default: 90 iterations (configurable via agent.max_turns)
Each agent gets its own budget. Subagents get independent budgets capped at delegation.max_iterations (default 50) — total iterations across parent + subagents can exceed the parent's cap
At 100%, the agent stops and returns a summary of work done
Fallback Model
When the primary model fails (429 rate limit, 5xx server error, 401/403 auth error):

Check fallback_providers list in config
Try each fallback in order
On success, continue the conversation with the new provider
On 401/403, attempt credential refresh before failing over
The fallback system also covers auxiliary tasks independently — vision, compression, and web extraction each have their own fallback chain configurable via the auxiliary.* config section.

Compression and Persistence
When Compression Triggers
Preflight (before API call): If conversation exceeds 50% of model's context window
Gateway auto-compression: If conversation exceeds 85% (more aggressive, runs between turns)
What Happens During Compression
Memory is flushed to disk first (preventing data loss)
Middle conversation turns are summarized into a compact summary
The last N messages are preserved intact (compression.protect_last_n, default: 20)
Tool call/result message pairs are kept together (never split)
A new session lineage ID is generated (compression creates a "child" session)
Session Persistence
After each turn:

Messages are saved to the session store (SQLite via hermes_state.py)
Memory changes are flushed to MEMORY.md / USER.md
The session can be resumed later via /resume or hermes chat --resume
Key Source Files
File	Purpose
run_agent.py	AIAgent class — the complete agent loop
agent/prompt_builder.py	System prompt assembly from memory, skills, context files, personality
agent/context_engine.py	ContextEngine ABC — pluggable context management
agent/context_compressor.py	Default engine — lossy summarization algorithm
agent/prompt_caching.py	Anthropic prompt caching markers and cache metrics
agent/auxiliary_client.py	Auxiliary LLM client for side tasks (vision, summarization)
model_tools.py	Tool schema collection, handle_function_call() dispatch

Context Compression and Caching
Hermes Agent uses a dual compression system and Anthropic prompt caching to manage context window usage efficiently across long conversations.

Source files: agent/context_engine.py (ABC), agent/context_compressor.py (default engine), agent/prompt_caching.py, gateway/run.py (session hygiene), run_agent.py (search for _compress_context)

Pluggable Context Engine
Context management is built on the ContextEngine ABC (agent/context_engine.py). The built-in ContextCompressor is the default implementation, but plugins can replace it with alternative engines (e.g., Lossless Context Management).

context:
  engine: "compressor"    # default — built-in lossy summarization
  engine: "lcm"           # example — plugin providing lossless context

The engine is responsible for:

Deciding when compaction should fire (should_compress())
Performing compaction (compress())
Optionally exposing tools the agent can call (e.g., lcm_grep)
Tracking token usage from API responses
Selection is config-driven via context.engine in config.yaml. The resolution order:

Check plugins/context_engine/<name>/ directory
Check general plugin system (register_context_engine())
Fall back to built-in ContextCompressor
Plugin engines are never auto-activated — the user must explicitly set context.engine to the plugin's name. The default "compressor" always uses the built-in.

Configure via hermes plugins → Provider Plugins → Context Engine, or edit config.yaml directly.

For building a context engine plugin, see Context Engine Plugins.

Dual Compression System
Hermes has two separate compression layers that operate independently:

                     ┌──────────────────────────┐
  Incoming message   │   Gateway Session Hygiene │  Fires at 85% of context
  ─────────────────► │   (pre-agent, rough est.) │  Safety net for large sessions
                     └─────────────┬────────────┘
                                   │
                                   ▼
                     ┌──────────────────────────┐
                     │   Agent ContextCompressor │  Fires at 50% of context (default)
                     │   (in-loop, real tokens)  │  Normal context management
                     └──────────────────────────┘


1. Gateway Session Hygiene (85% threshold)
Located in gateway/run.py (search for Session hygiene: auto-compress). This is a safety net that runs before the agent processes a message. It prevents API failures when sessions grow too large between turns (e.g., overnight accumulation in Telegram/Discord).

Threshold: Fixed at 85% of model context length
Token source: Prefers actual API-reported tokens from last turn; falls back to rough character-based estimate (estimate_messages_tokens_rough)
Fires: Only when len(history) >= 4 and compression is enabled
Purpose: Catch sessions that escaped the agent's own compressor
The gateway hygiene threshold is intentionally higher than the agent's compressor. Setting it at 50% (same as the agent) caused premature compression on every turn in long gateway sessions.

2. Agent ContextCompressor (50% threshold, configurable)
Located in agent/context_compressor.py. This is the primary compression system that runs inside the agent's tool loop with access to accurate, API-reported token counts.

Configuration
All compression settings are read from config.yaml under the compression key:

compression:
  enabled: true              # Enable/disable compression (default: true)
  threshold: 0.50            # Fraction of context window (default: 0.50 = 50%)
  target_ratio: 0.20         # How much of threshold to keep as tail (default: 0.20)
  protect_last_n: 20         # Minimum protected tail messages (default: 20)
  codex_gpt55_autoraise: true  # gpt-5.5 on Codex OAuth: raise trigger to 85% (default: true)

# Summarization model/provider configured under auxiliary:
auxiliary:
  compression:
    model: null              # Override model for summaries (default: auto-detect)
    provider: auto           # Provider: "auto", "openrouter", "nous", "main", etc.
    base_url: null           # Custom OpenAI-compatible endpoint


Parameter Details
Parameter	Default	Range	Description
threshold	0.50	0.0-1.0	Compression triggers when prompt tokens ≥ threshold × context_length
target_ratio	0.20	0.10-0.80	Controls tail protection token budget: threshold_tokens × target_ratio
protect_last_n	20	≥1	Minimum number of recent messages always preserved
protect_first_n	3	(hardcoded)	System prompt + first exchange always preserved
codex_gpt55_autoraise	true	bool	Raise the trigger to 85% for gpt-5.5 on the ChatGPT Codex OAuth route (see below). Set false to keep the global threshold
Codex gpt-5.5 threshold autoraise
The ChatGPT Codex OAuth backend hard-caps gpt-5.5 at a 272K context window (the same slug exposes 1.05M on OpenAI's direct API and OpenRouter, and 400K on GitHub Copilot). At the default 50% trigger, compaction would fire at ~136K — half the window the model can actually use. When the active route is Codex OAuth (provider: openai-codex) and the model is gpt-5.5, Hermes raises the trigger to 85% (~231K) and prints a one-time notice with the opt-out command. Only this exact route is affected; gpt-5.5 on any other provider keeps your global threshold. To opt back down to the global value:

hermes config set compression.codex_gpt55_autoraise false

Computed Values (for a 200K context model at defaults)
context_length       = 200,000
threshold_tokens     = 200,000 × 0.50 = 100,000
tail_token_budget    = 100,000 × 0.20 = 20,000
max_summary_tokens   = min(200,000 × 0.05, 12,000) = 10,000

Threshold is derived from the MAIN model's context window
threshold_tokens is always threshold × context_length, where context_length is the main agent model's context window — never the auxiliary/summary model's. On a 262,144-token model at the default 0.50, the threshold is 262,144 × 0.50 = 131,072. That number being close to a common "128K context" is a coincidence of the percentage, not a sign that the auxiliary model's window is the trigger. The auxiliary model's context window is a separate concern — see the "Summary model context length" warning below for how it affects whether a summary can be produced, not when compression fires.

Compression Algorithm
The ContextCompressor.compress() method follows a 4-phase algorithm:

Phase 1: Prune Old Tool Results (cheap, no LLM call)
Old tool results (>200 chars) outside the protected tail are replaced with:

[Old tool output cleared to save context space]

This is a cheap pre-pass that saves significant tokens from verbose tool outputs (file contents, terminal output, search results).

Phase 2: Determine Boundaries
┌─────────────────────────────────────────────────────────────┐
│  Message list                                               │
│                                                             │
│  [0..2]  ← protect_first_n (system + first exchange)        │
│  [3..N]  ← middle turns → SUMMARIZED                        │
│  [N..end] ← tail (by token budget OR protect_last_n)        │
│                                                             │
└─────────────────────────────────────────────────────────────┘

Tail protection is token-budget based: walks backward from the end, accumulating tokens until the budget is exhausted. Falls back to the fixed protect_last_n count if the budget would protect fewer messages.

Boundaries are aligned to avoid splitting tool_call/tool_result groups. The _align_boundary_backward() method walks past consecutive tool results to find the parent assistant message, keeping groups intact.

Phase 3: Generate Structured Summary
Summary model context length
The summary model must have a context window at least as large as the main agent model's. The entire middle section is sent to the summary model in a single call_llm(task="compression") call. If the summary model's context is smaller, the API returns a context-length error — _generate_summary() catches it, logs a warning, and returns None. The compressor then drops the middle turns without a summary, silently losing conversation context. This is the most common cause of degraded compaction quality.

The middle turns are summarized using the auxiliary LLM with a structured template:

## Goal
[What the user is trying to accomplish]

## Constraints & Preferences
[User preferences, coding style, constraints, important decisions]

## Progress
### Done
[Completed work — specific file paths, commands run, results]
### In Progress
[Work currently underway]
### Blocked
[Any blockers or issues encountered]

## Key Decisions
[Important technical decisions and why]

## Relevant Files
[Files read, modified, or created — with brief note on each]

## Next Steps
[What needs to happen next]

## Critical Context
[Specific values, error messages, configuration details]

Summary budget scales with the amount of content being compressed:

Formula: content_tokens × 0.20 (the _SUMMARY_RATIO constant)
Minimum: 2,000 tokens
Maximum: min(context_length × 0.05, 12,000) tokens
Phase 4: Assemble Compressed Messages
The compressed message list is:

Head messages (with a note appended to system prompt on first compression)
Summary message (role chosen to avoid consecutive same-role violations)
Tail messages (unmodified)
Orphaned tool_call/tool_result pairs are cleaned up by _sanitize_tool_pairs():

Tool results referencing removed calls → removed
Tool calls whose results were removed → stub result injected
Iterative Re-compression
On subsequent compressions, the previous summary is passed to the LLM with instructions to update it rather than summarize from scratch. This preserves information across multiple compactions — items move from "In Progress" to "Done", new progress is added, and obsolete information is removed.

The _previous_summary field on the compressor instance stores the last summary text for this purpose.

Before/After Example
Before Compression (45 messages, ~95K tokens)
[0] system:    "You are a helpful assistant..." (system prompt)
[1] user:      "Help me set up a FastAPI project"
[2] assistant: <tool_call> terminal: mkdir project </tool_call>
[3] tool:      "directory created"
[4] assistant: <tool_call> write_file: main.py </tool_call>
[5] tool:      "file written (2.3KB)"
    ... 30 more turns of file editing, testing, debugging ...
[38] assistant: <tool_call> terminal: pytest </tool_call>
[39] tool:      "8 passed, 2 failed\n..."  (5KB output)
[40] user:      "Fix the failing tests"
[41] assistant: <tool_call> read_file: tests/test_api.py </tool_call>
[42] tool:      "import pytest\n..."  (3KB)
[43] assistant: "I see the issue with the test fixtures..."
[44] user:      "Great, also add error handling"

After Compression (25 messages, ~45K tokens)
[0] system:    "You are a helpful assistant...
               [Note: Some earlier conversation turns have been compacted...]"
[1] user:      "Help me set up a FastAPI project"
[2] assistant: "[CONTEXT COMPACTION] Earlier turns were compacted...

               ## Goal
               Set up a FastAPI project with tests and error handling

               ## Progress
               ### Done
               - Created project structure: main.py, tests/, requirements.txt
               - Implemented 5 API endpoints in main.py
               - Wrote 10 test cases in tests/test_api.py
               - 8/10 tests passing

               ### In Progress
               - Fixing 2 failing tests (test_create_user, test_delete_user)

               ## Relevant Files
               - main.py — FastAPI app with 5 endpoints
               - tests/test_api.py — 10 test cases
               - requirements.txt — fastapi, pytest, httpx

               ## Next Steps
               - Fix failing test fixtures
               - Add error handling"
[3] user:      "Fix the failing tests"
[4] assistant: <tool_call> read_file: tests/test_api.py </tool_call>
[5] tool:      "import pytest\n..."
[6] assistant: "I see the issue with the test fixtures..."
[7] user:      "Great, also add error handling"


Prompt Caching (Anthropic)
Source: agent/prompt_caching.py

Reduces input token costs by ~75% on multi-turn conversations by caching the conversation prefix. Uses Anthropic's cache_control breakpoints.

Strategy: system_and_3
Anthropic allows a maximum of 4 cache_control breakpoints per request. Hermes uses the "system_and_3" strategy:

Breakpoint 1: System prompt           (stable across all turns)
Breakpoint 2: 3rd-to-last non-system message  ─┐
Breakpoint 3: 2nd-to-last non-system message   ├─ Rolling window
Breakpoint 4: Last non-system message          ─┘

How It Works
apply_anthropic_cache_control() deep-copies the messages and injects cache_control markers:

# Cache marker format
marker = {"type": "ephemeral"}
# Or for 1-hour TTL:
marker = {"type": "ephemeral", "ttl": "1h"}

The marker is applied differently based on content type:

Content Type	Where Marker Goes
String content	Converted to [{"type": "text", "text": ..., "cache_control": ...}]
List content	Added to the last element's dict
None/empty	Added as msg["cache_control"]
Tool messages	Added as msg["cache_control"] (native Anthropic only)
Cache-Aware Design Patterns
Stable system prompt: The system prompt is breakpoint 1 and cached across all turns. Avoid mutating it mid-conversation (compression appends a note only on the first compaction).

Message ordering matters: Cache hits require prefix matching. Adding or removing messages in the middle invalidates the cache for everything after.

Compression cache interaction: After compression, the cache is invalidated for the compressed region but the system prompt cache survives. The rolling 3-message window re-establishes caching within 1-2 turns.

TTL selection: Default is 5m (5 minutes). Use 1h for long-running sessions where the user takes breaks between turns.

Enabling Prompt Caching
Prompt caching is automatically enabled when:

The model is an Anthropic Claude model (detected by model name)
The provider supports cache_control (native Anthropic API or OpenRouter)
# config.yaml — TTL is configurable (must be "5m" or "1h")
prompt_caching:
  cache_ttl: "5m"

The CLI shows caching status at startup:

💾 Prompt caching: ENABLED (Claude via OpenRouter, 5m TTL)

Context Pressure Warnings
Intermediate context-pressure warnings have been removed (see the iteration-budget block in run_agent.py, which notes: "No intermediate pressure warnings — they caused models to 'give up' prematurely on complex tasks"). Compression fires when prompt tokens reach the configured compression.threshold (default 50%) with no prior warning step; gateway session hygiene fires as the secondary safety net at 85% of the model's context window.

Session Storage
Hermes Agent uses a SQLite database (~/.hermes/state.db) to persist session metadata, full message history, and model configuration across CLI and gateway sessions. This replaces the earlier per-session JSONL file approach.

Source file: hermes_state.py

Architecture Overview
~/.hermes/state.db (SQLite, WAL mode)
├── sessions              — Session metadata, token counts, billing
├── messages              — Full message history per session
├── messages_fts          — FTS5 virtual table (content + tool_name + tool_calls)
├── messages_fts_trigram  — FTS5 virtual table with trigram tokenizer (CJK / substring search)
├── state_meta            — Key/value metadata table
└── schema_version        — Single-row table tracking migration state


Key design decisions:

WAL mode for concurrent readers + one writer (gateway multi-platform)
FTS5 virtual table for fast text search across all session messages
Session lineage via parent_session_id chains (compression-triggered splits)
Source tagging (cli, telegram, discord, etc.) for platform filtering
Batch runner and RL trajectories are NOT stored here (separate systems)
SQLite Schema
Sessions Table
CREATE TABLE IF NOT EXISTS sessions (
    id TEXT PRIMARY KEY,
    source TEXT NOT NULL,
    user_id TEXT,
    model TEXT,
    model_config TEXT,
    system_prompt TEXT,
    parent_session_id TEXT,
    started_at REAL NOT NULL,
    ended_at REAL,
    end_reason TEXT,
    message_count INTEGER DEFAULT 0,
    tool_call_count INTEGER DEFAULT 0,
    input_tokens INTEGER DEFAULT 0,
    output_tokens INTEGER DEFAULT 0,
    cache_read_tokens INTEGER DEFAULT 0,
    cache_write_tokens INTEGER DEFAULT 0,
    reasoning_tokens INTEGER DEFAULT 0,
    billing_provider TEXT,
    billing_base_url TEXT,
    billing_mode TEXT,
    estimated_cost_usd REAL,
    actual_cost_usd REAL,
    cost_status TEXT,
    cost_source TEXT,
    pricing_version TEXT,
    title TEXT,
    api_call_count INTEGER DEFAULT 0,
    FOREIGN KEY (parent_session_id) REFERENCES sessions(id)
);

CREATE INDEX IF NOT EXISTS idx_sessions_source ON sessions(source);
CREATE INDEX IF NOT EXISTS idx_sessions_parent ON sessions(parent_session_id);
CREATE INDEX IF NOT EXISTS idx_sessions_started ON sessions(started_at DESC);
CREATE UNIQUE INDEX IF NOT EXISTS idx_sessions_title_unique
    ON sessions(title) WHERE title IS NOT NULL;


Messages Table
CREATE TABLE IF NOT EXISTS messages (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    session_id TEXT NOT NULL REFERENCES sessions(id),
    role TEXT NOT NULL,
    content TEXT,
    tool_call_id TEXT,
    tool_calls TEXT,
    tool_name TEXT,
    timestamp REAL NOT NULL,
    token_count INTEGER,
    finish_reason TEXT,
    reasoning TEXT,
    reasoning_content TEXT,
    reasoning_details TEXT,
    codex_reasoning_items TEXT,
    codex_message_items TEXT
);

CREATE INDEX IF NOT EXISTS idx_messages_session ON messages(session_id, timestamp);


Notes:

tool_calls is stored as a JSON string (serialized list of tool call objects)
reasoning_details, codex_reasoning_items, and codex_message_items are stored as JSON strings
reasoning stores the raw reasoning text for providers that expose it
Timestamps are Unix epoch floats (time.time())
FTS5 Full-Text Search
CREATE VIRTUAL TABLE IF NOT EXISTS messages_fts USING fts5(
    content,
    content=messages,
    content_rowid=id
);

The FTS5 table is kept in sync via three triggers that fire on INSERT, UPDATE, and DELETE of the messages table:

CREATE TRIGGER IF NOT EXISTS messages_fts_insert AFTER INSERT ON messages BEGIN
    INSERT INTO messages_fts(rowid, content) VALUES (new.id, new.content);
END;

CREATE TRIGGER IF NOT EXISTS messages_fts_delete AFTER DELETE ON messages BEGIN
    INSERT INTO messages_fts(messages_fts, rowid, content)
        VALUES('delete', old.id, old.content);
END;

CREATE TRIGGER IF NOT EXISTS messages_fts_update AFTER UPDATE ON messages BEGIN
    INSERT INTO messages_fts(messages_fts, rowid, content)
        VALUES('delete', old.id, old.content);
    INSERT INTO messages_fts(rowid, content) VALUES (new.id, new.content);
END;


Schema Version and Migrations
Current schema version: 11

The schema_version table stores a single integer. Simple column additions are handled declaratively by _reconcile_columns() (which diffs live columns against SCHEMA_SQL and ADDs any missing ones). The version-gated chain is reserved for data migrations and index/FTS changes that can't be expressed declaratively:

Version	Change
1	Initial schema (sessions, messages, FTS5)
2	Add finish_reason column to messages
3	Add title column to sessions
4	Add unique index on title (NULLs allowed, non-NULL must be unique)
5	Add billing columns: cache_read_tokens, cache_write_tokens, reasoning_tokens, billing_provider, billing_base_url, billing_mode, estimated_cost_usd, actual_cost_usd, cost_status, cost_source, pricing_version
6	Add reasoning columns to messages: reasoning, reasoning_details, codex_reasoning_items
7	Add reasoning_content column to messages
8	Add api_call_count column to sessions
9	Add codex_message_items column to messages for Codex Responses message id/phase replay
10	Add messages_fts_trigram virtual table (trigram tokenizer for CJK / substring search) and backfill existing rows
11	Re-index messages_fts and messages_fts_trigram to cover tool_name + tool_calls and switch from external-content to inline mode; drop old triggers and backfill every message row
Declarative column adds use ALTER TABLE ADD COLUMN wrapped in try/except to handle the column-already-exists case (idempotent). The version number is bumped after each successful migration block.

Write Contention Handling
Multiple hermes processes (gateway + CLI sessions + worktree agents) share one state.db. The SessionDB class handles write contention with:

Short SQLite timeout (1 second) instead of the default 30s
Application-level retry with random jitter (20-150ms, up to 15 retries)
BEGIN IMMEDIATE transactions to surface lock contention at transaction start
Periodic WAL checkpoints every 50 successful writes (PASSIVE mode)
This avoids the "convoy effect" where SQLite's deterministic internal backoff causes all competing writers to retry at the same intervals.

_WRITE_MAX_RETRIES = 15
_WRITE_RETRY_MIN_S = 0.020   # 20ms
_WRITE_RETRY_MAX_S = 0.150   # 150ms
_CHECKPOINT_EVERY_N_WRITES = 50

Common Operations
Initialize
from hermes_state import SessionDB

db = SessionDB()                           # Default: ~/.hermes/state.db
db = SessionDB(db_path=Path("/tmp/test.db"))  # Custom path

Create and Manage Sessions
# Create a new session
db.create_session(
    session_id="sess_abc123",
    source="cli",
    model="anthropic/claude-sonnet-4.6",
    user_id="user_1",
    parent_session_id=None,  # or previous session ID for lineage
)

# End a session
db.end_session("sess_abc123", end_reason="user_exit")

# Reopen a session (clear ended_at/end_reason)
db.reopen_session("sess_abc123")

Store Messages
msg_id = db.append_message(
    session_id="sess_abc123",
    role="assistant",
    content="Here's the answer...",
    tool_calls=[{"id": "call_1", "function": {"name": "terminal", "arguments": "{}"}}],
    token_count=150,
    finish_reason="stop",
    reasoning="Let me think about this...",
)


Retrieve Messages
# Raw messages with all metadata
messages = db.get_messages("sess_abc123")

# OpenAI conversation format (for API replay)
conversation = db.get_messages_as_conversation("sess_abc123")
# Returns: [{"role": "user", "content": "..."}, {"role": "assistant", ...}]

Session Titles
# Set a title (must be unique among non-NULL titles)
db.set_session_title("sess_abc123", "Fix Docker Build")

# Resolve by title (returns most recent in lineage)
session_id = db.resolve_session_by_title("Fix Docker Build")

# Auto-generate next title in lineage
next_title = db.get_next_title_in_lineage("Fix Docker Build")
# Returns: "Fix Docker Build #2"

Full-Text Search
The search_messages() method supports FTS5 query syntax with automatic sanitization of user input.

Basic Search
results = db.search_messages("docker deployment")

FTS5 Query Syntax
Syntax	Example	Meaning
Keywords	docker deployment	Both terms (implicit AND)
Quoted phrase	"exact phrase"	Exact phrase match
Boolean OR	docker OR kubernetes	Either term
Boolean NOT	python NOT java	Exclude term
Prefix	deploy*	Prefix match
Filtered Search
# Search only CLI sessions
results = db.search_messages("error", source_filter=["cli"])

# Exclude gateway sessions
results = db.search_messages("bug", exclude_sources=["telegram", "discord"])

# Search only user messages
results = db.search_messages("help", role_filter=["user"])

Search Results Format
Each result includes:

id, session_id, role, timestamp
snippet — FTS5-generated snippet with >>>match<<< markers
context — 1 message before and after the match (content truncated to 200 chars)
source, model, session_started — from the parent session
The _sanitize_fts5_query() method handles edge cases:

Strips unmatched quotes and special characters
Wraps hyphenated terms in quotes (chat-send → "chat-send")
Removes dangling boolean operators (hello AND → hello)
Session Lineage
Sessions can form chains via parent_session_id. This happens when context compression triggers a session split in the gateway.

Query: Find Session Lineage
-- Find all ancestors of a session
WITH RECURSIVE lineage AS (
    SELECT * FROM sessions WHERE id = ?
    UNION ALL
    SELECT s.* FROM sessions s
    JOIN lineage l ON s.id = l.parent_session_id
)
SELECT id, title, started_at, parent_session_id FROM lineage;

-- Find all descendants of a session
WITH RECURSIVE descendants AS (
    SELECT * FROM sessions WHERE id = ?
    UNION ALL
    SELECT s.* FROM sessions s
    JOIN descendants d ON s.parent_session_id = d.id
)
SELECT id, title, started_at FROM descendants;

Query: Recent Sessions with Preview
SELECT s.*,
    COALESCE(
        (SELECT SUBSTR(m.content, 1, 63)
         FROM messages m
         WHERE m.session_id = s.id AND m.role = 'user' AND m.content IS NOT NULL
         ORDER BY m.timestamp, m.id LIMIT 1),
        ''
    ) AS preview,
    COALESCE(
        (SELECT MAX(m2.timestamp) FROM messages m2 WHERE m2.session_id = s.id),
        s.started_at
    ) AS last_active
FROM sessions s
ORDER BY s.started_at DESC
LIMIT 20;


Query: Token Usage Statistics
-- Total tokens by model
SELECT model,
       COUNT(*) as session_count,
       SUM(input_tokens) as total_input,
       SUM(output_tokens) as total_output,
       SUM(estimated_cost_usd) as total_cost
FROM sessions
WHERE model IS NOT NULL
GROUP BY model
ORDER BY total_cost DESC;

-- Sessions with highest token usage
SELECT id, title, model, input_tokens + output_tokens AS total_tokens,
       estimated_cost_usd
FROM sessions
ORDER BY total_tokens DESC
LIMIT 10;

Export and Cleanup
# Export a single session with messages
data = db.export_session("sess_abc123")

# Export all sessions (with messages) as list of dicts
all_data = db.export_all(source="cli")

# Delete old sessions (only ended sessions)
deleted_count = db.prune_sessions(older_than_days=90)
deleted_count = db.prune_sessions(older_than_days=30, source="telegram")

# Clear messages but keep the session record
db.clear_messages("sess_abc123")

# Delete session and all messages
db.delete_session("sess_abc123")

Database Location
Default path: ~/.hermes/state.db

This is derived from hermes_constants.get_hermes_home() which resolves to ~/.hermes/ by default, or the value of HERMES_HOME environment variable.

The database file, WAL file (state.db-wal), and shared-memory file (state.db-shm) are all created in the same directory.

Prompt Assembly
Hermes deliberately separates:

cached system prompt state
ephemeral API-call-time additions
This is one of the most important design choices in the project because it affects:

token usage
prompt caching effectiveness
session continuity
memory correctness
Primary files:

run_agent.py
agent/prompt_builder.py
tools/memory_tool.py
Cached system prompt layers
The cached system prompt is assembled as three ordered tiers (see agent/system_prompt.py):

stable — identity (SOUL.md or fallback), tool/model guidance, skills prompt, environment hints, platform hints
context — caller-supplied system_message plus project context files (.hermes.md / AGENTS.md / CLAUDE.md / .cursorrules)
volatile — built-in memory snapshot (MEMORY.md), user profile snapshot (USER.md), external memory-provider block, timestamp/session/model/provider line
The final system prompt is then joined as: stable → context → volatile.

This ordering matters for precedence discussions:

skills are part of the stable tier
memory/profile snapshots are part of the volatile tier
both are still in the cached system prompt (they are not injected as ad-hoc mid-turn overlays)
When skip_context_files is set (e.g., subagent delegation), SOUL.md is not loaded and the hardcoded DEFAULT_AGENT_IDENTITY is used instead.

Concrete example: assembled system prompt
Here is a simplified view of what the final system prompt looks like when all layers are present (comments show the source of each section):

# Layer 1: Agent Identity (from ~/.hermes/SOUL.md)
You are Hermes, an AI assistant created by Nous Research.
You are an expert software engineer and researcher.
You value correctness, clarity, and efficiency.
...

# Layer 2: Tool-aware behavior guidance
You have persistent memory across sessions. Save durable facts using
the memory tool: user preferences, environment details, tool quirks,
and stable conventions. Memory is injected into every turn, so keep
it compact and focused on facts that will still matter later.
...
When the user references something from a past conversation or you
suspect relevant cross-session context exists, use session_search
to recall it before asking them to repeat themselves.

# Tool-use enforcement (for GPT/Codex models only)
You MUST use your tools to take action — do not describe what you
would do or plan to do without actually doing it.
...

# Layer 3: Honcho static block (when active)
[Honcho personality/context data]

# Layer 4: Optional system message (from config or API)
[User-configured system message override]

# Layer 5: Frozen MEMORY snapshot
## Persistent Memory
- User prefers Python 3.12, uses pyproject.toml
- Default editor is nvim
- Working on project "atlas" in ~/code/atlas
- Timezone: US/Pacific

# Layer 6: Frozen USER profile snapshot
## User Profile
- Name: Alice
- GitHub: alice-dev

# Layer 7: Skills index
## Skills (mandatory)
Before replying, scan the skills below. If one clearly matches
your task, load it with skill_view(name) and follow its instructions.
...
<available_skills>
  software-development:
    - code-review: Structured code review workflow
    - test-driven-development: TDD methodology
  research:
    - arxiv: Search and summarize arXiv papers
</available_skills>

# Layer 8: Context files (from project directory)
# Project Context
The following project context files have been loaded and should be followed:

## AGENTS.md
This is the atlas project. Use pytest for testing. The main
entry point is src/atlas/main.py. Always run `make lint` before
committing.

# Layer 9: Timestamp + session
Current time: 2026-03-30T14:30:00-07:00
Session: abc123

# Layer 10: Platform hint
You are a CLI AI Agent. Try not to use markdown but simple text
renderable inside a terminal.

How SOUL.md appears in the prompt
SOUL.md lives at ~/.hermes/SOUL.md and serves as the agent's identity — the very first section of the system prompt. The loading logic in prompt_builder.py works as follows:

# From agent/prompt_builder.py (simplified)
def load_soul_md() -> Optional[str]:
    soul_path = get_hermes_home() / "SOUL.md"
    if not soul_path.exists():
        return None
    content = soul_path.read_text(encoding="utf-8").strip()
    content = _scan_context_content(content, "SOUL.md")  # Security scan
    content = _truncate_content(content, "SOUL.md")       # Cap at 20k chars
    return content

When load_soul_md() returns content, it replaces the hardcoded DEFAULT_AGENT_IDENTITY. The build_context_files_prompt() function is then called with skip_soul=True to prevent SOUL.md from appearing twice (once as identity, once as a context file).

If SOUL.md doesn't exist, the system falls back to:

You are Hermes Agent, an intelligent AI assistant created by Nous Research.
You are helpful, knowledgeable, and direct. You assist users with a wide
range of tasks including answering questions, writing and editing code,
analyzing information, creative work, and executing actions via your tools.
You communicate clearly, admit uncertainty when appropriate, and prioritize
being genuinely useful over being verbose unless otherwise directed below.
Be targeted and efficient in your exploration and investigations.

How context files are injected
build_context_files_prompt() uses a priority system — only one project context type is loaded (first match wins):

# From agent/prompt_builder.py (simplified)
def build_context_files_prompt(cwd=None, skip_soul=False):
    cwd_path = Path(cwd).resolve()

    # Priority: first match wins — only ONE project context loaded
    project_context = (
        _load_hermes_md(cwd_path)       # 1. .hermes.md / HERMES.md (walks to git root)
        or _load_agents_md(cwd_path)    # 2. AGENTS.md (cwd only)
        or _load_claude_md(cwd_path)    # 3. CLAUDE.md (cwd only)
        or _load_cursorrules(cwd_path)  # 4. .cursorrules / .cursor/rules/*.mdc
    )

    sections = []
    if project_context:
        sections.append(project_context)

    # SOUL.md from HERMES_HOME (independent of project context)
    if not skip_soul:
        soul_content = load_soul_md()
        if soul_content:
            sections.append(soul_content)

    if not sections:
        return ""

    return (
        "# Project Context\n\n"
        "The following project context files have been loaded "
        "and should be followed:\n\n"
        + "\n".join(sections)
    )


Context file discovery details
Priority	Files	Search scope	Notes
1	.hermes.md, HERMES.md	CWD up to git root	Hermes-native project config
2	AGENTS.md	CWD only	Common agent instruction file
3	CLAUDE.md	CWD only	Claude Code compatibility
4	.cursorrules, .cursor/rules/*.mdc	CWD only	Cursor compatibility
All context files are:

Security scanned — checked for prompt injection patterns (invisible unicode, "ignore previous instructions", credential exfiltration attempts)
Truncated — capped at 20,000 characters using 70/20 head/tail ratio with a truncation marker
YAML frontmatter stripped — .hermes.md frontmatter is removed (reserved for future config overrides)
API-call-time-only layers
These are intentionally not persisted as part of the cached system prompt:

ephemeral_system_prompt
prefill messages
gateway-derived session context overlays
later-turn Honcho/external recall injected into the current-turn user message
pre_llm_call plugin context also lands in this API-call-time path: it is appended to the current turn's user message, not written into the cached system prompt. When multiple plugins return context, Hermes concatenates those context blocks (see Hooks → pre_llm_call).

This separation keeps the stable prefix stable for caching.

Memory snapshots
Local memory and user profile data are captured in the system prompt's volatile tier. Mid-session writes update disk state but do not mutate the already-built cached system prompt until a rebuild path runs (new session, or explicit invalidation/rebuild flow such as compression-triggered rebuild).

Context files
agent/prompt_builder.py scans and sanitizes project context files using a priority system — only one type is loaded (first match wins):

.hermes.md / HERMES.md (walks to git root)
AGENTS.md (CWD at startup; subdirectories discovered progressively during the session via agent/subdirectory_hints.py)
CLAUDE.md (CWD only)
.cursorrules / .cursor/rules/*.mdc (CWD only)
SOUL.md is loaded separately via load_soul_md() for the identity slot. When it loads successfully, build_context_files_prompt(skip_soul=True) prevents it from appearing twice.

Long files are truncated before injection.

Skills index
The skills system contributes a compact skills index to the prompt when skills tooling is available.

Supported prompt customization surfaces
Most users should treat agent/prompt_builder.py as implementation code, not a configuration surface. The supported customization path is to change the prompt inputs Hermes already loads, rather than editing Python templates in place.

Use these surfaces first
~/.hermes/SOUL.md — replace the built-in default identity block with your own agent persona and standing behavior.
~/.hermes/MEMORY.md and ~/.hermes/USER.md — provide durable cross-session facts and user profile data that should be snapshotted into new sessions.
Project context files such as .hermes.md, HERMES.md, AGENTS.md, CLAUDE.md, or .cursorrules — inject repo-specific working rules.
Skills — package reusable workflows and references without editing core prompt code.
Optional system prompt config / API overrides — add deployment-specific instruction text without forking Hermes.
Ephemeral overlays such as HERMES_EPHEMERAL_SYSTEM_PROMPT or prefill messages — add turn-scoped guidance that should not become part of the cached prompt prefix.
When to edit code instead
Edit agent/prompt_builder.py only if you are intentionally maintaining a fork or contributing upstream behavior changes. That file assembles the prompt plumbing, cache boundaries, and injection order for every session. Direct edits there are global product changes, not per-user prompt customization.

In other words:

if you want a different assistant identity, edit SOUL.md
if you want different repo rules, edit project context files
if you want reusable operating procedures, add or modify skills
if you want to change how Hermes assembles prompts for everyone, change Python and treat it as a code contribution
Why prompt assembly is split this way
The architecture is intentionally optimized to:

preserve provider-side prompt caching
avoid mutating history unnecessarily
keep memory semantics understandable
let gateway/ACP/CLI add context without poisoning persistent prompt state

Gateway Internals
The messaging gateway is the long-running process that connects Hermes to 20+ external messaging platforms through a unified architecture.

Key Files
File	Purpose
gateway/run.py	GatewayRunner — main loop, slash commands, message dispatch (large file; check git for current LOC)
gateway/session.py	SessionStore — conversation persistence and session key construction
gateway/delivery.py	Outbound message delivery to target platforms/channels
gateway/pairing.py	DM pairing flow for user authorization
gateway/channel_directory.py	Maps chat IDs to human-readable names for cron delivery
gateway/hooks.py	Hook discovery, loading, and lifecycle event dispatch
gateway/mirror.py	Cross-session message mirroring for send_message
gateway/status.py	Token lock management for profile-scoped gateway instances
gateway/builtin_hooks/	Extension point for always-registered hooks (none shipped)
gateway/platforms/	Platform adapters (one per messaging platform)
Architecture Overview
┌─────────────────────────────────────────────────┐
│                  GatewayRunner                  │
│                                                 │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐       │
│  │ Telegram │  │ Discord  │  │  Slack   │       │
│  │ Adapter  │  │ Adapter  │  │ Adapter  │       │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘       │
│       │             │             │             │
│       └─────────────┼─────────────┘             │
│                     ▼                           │
│              _handle_message()                  │
│                     │                           │
│         ┌───────────┼───────────┐               │
│         ▼           ▼           ▼               │
│  Slash command   AIAgent    Queue/BG            │
│    dispatch      creation   sessions            │
│                     │                           │
│                     ▼                           │
│                 SessionStore                    │
│              (SQLite persistence)               │
└───────┴─────────────┴─────────────┴─────────────┘

Message Flow
When a message arrives from any platform:

Platform adapter receives raw event, normalizes it into a MessageEvent
Base adapter checks active session guard:
If agent is running for this session → queue message, set interrupt event
If /approve, /deny, /stop → bypass guard (dispatched inline)
GatewayRunner._handle_message() receives the event:
Resolve session key via _session_key_for_source() (format: agent:main:{platform}:{chat_type}:{chat_id})
Check authorization (see Authorization below)
Check if it's a slash command → dispatch to command handler
Check if agent is already running → intercept commands like /stop, /status
Otherwise → create AIAgent instance and run conversation
Response is sent back through the platform adapter
Session Key Format
Session keys encode the full routing context:

agent:main:{platform}:{chat_type}:{chat_id}

For example: agent:main:telegram:private:123456789

Thread-aware platforms (Telegram forum topics, Discord threads, Slack threads) may include thread IDs in the chat_id portion. Never construct session keys manually — always use build_session_key() from gateway/session.py.

Two-Level Message Guard
When an agent is actively running, incoming messages pass through two sequential guards:

Level 1 — Base adapter (gateway/platforms/base.py): Checks _active_sessions. If the session is active, queues the message in _pending_messages and sets an interrupt event. This catches messages before they reach the gateway runner.

Level 2 — Gateway runner (gateway/run.py): Checks _running_agents. Intercepts specific commands (/stop, /new, /queue, /status, /approve, /deny) and routes them appropriately. Everything else triggers running_agent.interrupt().

Commands that must reach the runner while the agent is blocked (like /approve) are dispatched inline via await self._message_handler(event) — they bypass the background task system to avoid race conditions.

Authorization
The gateway uses a multi-layer authorization check, evaluated in order:

Per-platform allow-all flag (e.g., TELEGRAM_ALLOW_ALL_USERS) — if set, all users on that platform are authorized
Platform allowlist (e.g., TELEGRAM_ALLOWED_USERS) — comma-separated user IDs
DM pairing — authenticated users can pair new users via a pairing code
Global allow-all (GATEWAY_ALLOW_ALL_USERS) — if set, all users across all platforms are authorized
Default: deny — unauthorized users are rejected
DM Pairing Flow
Admin: /pair
Gateway: "Pairing code: ABC123. Share with the user."
New user: ABC123
Gateway: "Paired! You're now authorized."

Pairing state is persisted in gateway/pairing.py and survives restarts.

Slash Command Dispatch
All slash commands in the gateway flow through the same resolution pipeline:

resolve_command() from hermes_cli/commands.py maps input to canonical name (handles aliases, prefix matching)
The canonical name is checked against GATEWAY_KNOWN_COMMANDS
Handler in _handle_message() dispatches based on canonical name
Some commands are gated on config (gateway_config_gate on CommandDef)
Running-Agent Guard
Commands that must NOT execute while the agent is processing are rejected early:

if _quick_key in self._running_agents:
    if canonical == "model":
        return "⏳ Agent is running — wait for it to finish or /stop first."

Bypass commands (/stop, /new, /approve, /deny, /queue, /status) have special handling.

Config Sources
The gateway reads configuration from multiple sources:

Source	What it provides
~/.hermes/.env	API keys, bot tokens, platform credentials
~/.hermes/config.yaml	Model settings, tool configuration, display options
Environment variables	Override any of the above
Unlike the CLI (which uses load_cli_config() with hardcoded defaults), the gateway reads config.yaml directly via YAML loader. This means config keys that exist in the CLI's defaults dict but not in the user's config file may behave differently between CLI and gateway.

Platform Adapters
Each messaging platform has an adapter in gateway/platforms/:

gateway/platforms/
├── base.py              # BaseAdapter — shared logic for all platforms
├── telegram.py          # Telegram Bot API (long polling or webhook)
├── discord.py           # Discord bot via discord.py
├── slack.py             # Slack Socket Mode
├── whatsapp.py          # WhatsApp Business Cloud API
├── signal.py            # Signal via signal-cli REST API
├── matrix.py            # Matrix via mautrix (optional E2EE)
├── mattermost.py        # Mattermost WebSocket API
├── email.py             # Email via IMAP/SMTP
├── sms.py               # SMS via Twilio
├── dingtalk.py          # DingTalk WebSocket
├── feishu.py            # Feishu/Lark WebSocket or webhook
├── wecom.py             # WeCom (WeChat Work) callback
├── weixin.py            # Weixin (personal WeChat) via iLink Bot API
├── bluebubbles.py       # Apple iMessage via BlueBubbles macOS server
├── qqbot/               # QQ Bot (Tencent QQ) via Official API v2 (sub-package: adapter.py, crypto.py, keyboards.py, …)
├── yuanbao.py           # Yuanbao (Tencent) DM/group adapter
├── feishu_comment.py    # Feishu document/drive comment-reply handler
├── msgraph_webhook.py   # Microsoft Graph change-notification webhook (Teams, Outlook, etc.)
├── webhook.py           # Inbound/outbound webhook adapter
├── api_server.py        # REST API server adapter
└── homeassistant.py     # Home Assistant conversation integration


Adapters implement a common interface:

connect() / disconnect() — lifecycle management
send_message() — outbound message delivery
on_message() — inbound message normalization → MessageEvent
Token Locks
Adapters that connect with unique credentials call acquire_scoped_lock() in connect() and release_scoped_lock() in disconnect(). This prevents two profiles from using the same bot token simultaneously.

Delivery Path
Outgoing deliveries (gateway/delivery.py) handle:

Direct reply — send response back to the originating chat
Home channel delivery — route cron job outputs and background results to a configured home channel
Explicit target delivery — send_message tool specifying telegram:-1001234567890, or the hermes send CLI wrapping the same tool for shell scripts
Cross-platform delivery — deliver to a different platform than the originating message
Cron job deliveries are NOT mirrored into gateway session history — they live in their own cron session only. This is a deliberate design choice to avoid message alternation violations.

Hooks
Gateway hooks are Python modules that respond to lifecycle events:

Gateway Hook Events
Event	When fired
gateway:startup	Gateway process starts
session:start	New conversation session begins
session:end	Session completes or times out
session:reset	User resets session with /new
agent:start	Agent begins processing a message
agent:step	Agent completes one tool-calling iteration
agent:end	Agent finishes and returns response
command:*	Any slash command is executed
Hooks are discovered from gateway/builtin_hooks/ (an extension point — currently empty in the shipped distribution; _register_builtin_hooks() is a no-op stub) and ~/.hermes/hooks/ (user-installed). Each hook is a directory with a HOOK.yaml manifest and handler.py.

Memory Provider Integration
When a memory provider plugin (e.g., Honcho) is enabled:

Gateway creates an AIAgent per message with the session ID
The MemoryManager initializes the provider with the session context
Provider tools (e.g., honcho_profile, viking_search) are routed through:
AIAgent._invoke_tool()
  → self._memory_manager.handle_tool_call(name, args)
    → provider.handle_tool_call(name, args)

On session end/reset, on_session_end() fires for cleanup and final data flush
Memory Flush Lifecycle
When a session is reset, resumed, or expires:

Built-in memories are flushed to disk
Memory provider's on_session_end() hook fires
A temporary AIAgent runs a memory-only conversation turn
Context is then discarded or archived
Background Maintenance
The gateway runs periodic maintenance alongside message handling:

Cron ticking — checks job schedules and fires due jobs
Session expiry — cleans up abandoned sessions after timeout
Memory flush — proactively flushes memory before session expiry
Cache refresh — refreshes model lists and provider status
Process Management
The gateway runs as a long-lived process, managed via:

hermes gateway start / hermes gateway stop — manual control
systemctl (Linux) or launchctl (macOS) — service management
PID file at ~/.hermes/gateway.pid — profile-scoped process tracking
Profile-scoped vs global: start_gateway() uses profile-scoped PID files. hermes gateway stop stops only the current profile's gateway. hermes gateway stop --all uses global ps aux scanning to kill all gateway processes (used during updates).

Related Docs
Session Storage
Cron Internals
ACP Internals
Agent Loop Internals
Messaging Gateway (User Guide)
Edit this page
