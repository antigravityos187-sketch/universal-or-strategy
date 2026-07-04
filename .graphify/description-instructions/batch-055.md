# Node Description Batch 56 of 61

Graphify is running in assistant/skill mode (no API key). You are the host
assistant (Claude Code / Codex / Gemini CLI). Read the prompt below and write
your JSON answer to the answer file.

## Prompt

You are documenting nodes in a knowledge graph.
For each entry below, write ONE concise factual plain-language sentence
describing what it is or does. Use only the provided context.
For a code symbol (kind=code-symbol — a function, class, or constant),
describe what the function/symbol does based on its name, source location
and neighbors — e.g. "Resolves the configured ontology profile from graphify.yaml.".
For an entity node (any other kind — e.g. a person, place, event, object),
describe what the entity is and its role, grounded in its type, its
relations (neighbors) and the provided citations/evidence — e.g.
"Lady Carfax, a wealthy heiress who disappears en route to Lausanne.".
Ground entity descriptions in the citations/evidence when present; do not
speculate beyond the context, so a node with no supporting context may be
left out of the reply.
No marketing language.
Respond ONLY with a JSON object mapping each node id (as a string) to its
one-sentence description — no prose, no markdown fences.

- "scripts_wave2_parallel_executor_rationale_265": "Generate Phase 5.5 prompt for an epic" | kind=entity | source=scripts/wave2_parallel_executor.py:L265 | neighbors=[phase_5_5_prompt()]
- "scripts_wave2_parallel_executor_rationale_280": "Generate Phase 6 prompt for an epic" | kind=entity | source=scripts/wave2_parallel_executor.py:L280 | neighbors=[phase_6_prompt()]
- "scripts_wave2_parallel_executor_rationale_49": "Execute Bob CLI for a single epic in its own worktree with staggered startup" | kind=entity | source=scripts/wave2_parallel_executor.py:L49 | neighbors=[execute_bob_for_epic()]
- "scripts_wave2_parallel_executor_rationale_89": "Execute a phase for epics in parallel using separate worktrees with staggered st" | kind=entity | source=scripts/wave2_parallel_executor.py:L89 | neighbors=[execute_phase_parallel()]
- "scripts_wave2_simple_orchestrator_rationale_101": "# TODO: Add remaining phases (1.5, 2, 3, 4, 5, 5.5, 6)" | kind=entity | source=scripts/wave2_simple_orchestrator.py:L101 | neighbors=[wave2_simple_orchestrator.py]
- "scripts_wave2_simple_orchestrator_rationale_30": "Execute Phase 0 for all 9 epics using Bob CLI." | kind=entity | source=scripts/wave2_simple_orchestrator.py:L30 | neighbors=[execute_phase_0_batch()]
- "scripts_wave2_simple_orchestrator_rationale_58": "Execute Phase 1 for all 9 epics." | kind=entity | source=scripts/wave2_simple_orchestrator.py:L58 | neighbors=[execute_phase_1_batch()]
- "scripts_wave2_simple_orchestrator_rationale_85": "Main orchestration loop." | kind=entity | source=scripts/wave2_simple_orchestrator.py:L85 | neighbors=[main()]
- "scripts_wave7_batch_audit_rationale_223": "Run complexity_audit.py once and parse the output into a method->CYC map.     Ca" | kind=entity | source=scripts/wave7_batch_audit.py:L223 | neighbors=[_load_cyc_cache()]
- "scripts_wave7_batch_audit_rationale_256": "Return the target method name for an epic.     Priority:       1. precomputed.js" | kind=entity | source=scripts/wave7_batch_audit.py:L256 | neighbors=[_resolve_target_method()]
- "scripts_wave7_batch_audit_rationale_324": "Audit a single epic for a given phase.     Returns a result dict:       { \"epic_" | kind=entity | source=scripts/wave7_batch_audit.py:L324 | neighbors=[audit_epic()]
- "scripts_wave7_batch_audit_rationale_507": "Audit a batch of epics for a given phase.     Prints human-readable summary (unl" | kind=entity | source=scripts/wave7_batch_audit.py:L507 | neighbors=[run_batch_audit()]
- "scripts_worker_agent_mcp_fastmcp_rationale_148": "Execute all phases of a claimed epic (intake, scope, plan, scan, tickets, valida" | kind=entity | source=scripts/worker_agent_mcp_fastmcp.py:L148 | neighbors=[execute_epic()]
- "scripts_worker_agent_mcp_fastmcp_rationale_211": "Release epic lock after completion or failure.\r     \r     Args:\r         epic_id" | kind=entity | source=scripts/worker_agent_mcp_fastmcp.py:L211 | neighbors=[release_epic()]
- "scripts_worker_agent_mcp_fastmcp_rationale_258": "Get current worker status (assigned epic, progress, health).\r     \r     Returns:" | kind=entity | source=scripts/worker_agent_mcp_fastmcp.py:L258 | neighbors=[get_worker_status()]
- "scripts_worker_agent_mcp_fastmcp_rationale_288": "Get next pending epic from roadmap (not complete, not assigned).\r     \r     Retu" | kind=entity | source=scripts/worker_agent_mcp_fastmcp.py:L288 | neighbors=[get_next_pending_epic()]
- "scripts_worker_agent_mcp_fastmcp_rationale_40": "Execute shell command and return result" | kind=entity | source=scripts/worker_agent_mcp_fastmcp.py:L40 | neighbors=[run_command()]
- "scripts_worker_agent_mcp_fastmcp_rationale_65": "Atomically claim an epic for this worker using git-based locking.\r     \r     Arg" | kind=entity | source=scripts/worker_agent_mcp_fastmcp.py:L65 | neighbors=[claim_epic()]
- "scripts_worker_agent_mcp_main": "main()" | kind=code-symbol | source=scripts/worker_agent_mcp.py:L433 | neighbors=[worker_agent_mcp.py]
- "scripts_worker_agent_mcp_rationale_135": "Handle MCP tool calls" | kind=entity | source=scripts/worker_agent_mcp.py:L135 | neighbors=[call_tool()]
- "scripts_worker_agent_mcp_rationale_160": "Atomically claim epic using git-based locking" | kind=entity | source=scripts/worker_agent_mcp.py:L160 | neighbors=[claim_epic_tool()]
- "scripts_worker_agent_mcp_rationale_252": "Execute all phases of epic" | kind=entity | source=scripts/worker_agent_mcp.py:L252 | neighbors=[execute_epic_tool()]
- "scripts_worker_agent_mcp_rationale_394": "Get next pending epic" | kind=entity | source=scripts/worker_agent_mcp.py:L394 | neighbors=[get_next_pending_epic_tool()]
- "scripts_worker_agent_mcp_rationale_43": "Execute shell command and return result" | kind=entity | source=scripts/worker_agent_mcp.py:L43 | neighbors=[run_command()]
- "scripts_worker_agent_mcp_rationale_68": "List available MCP tools for worker agent" | kind=entity | source=scripts/worker_agent_mcp.py:L68 | neighbors=[list_tools()]
- "scripts_zero_caller_trace": "zero_caller_trace.py" | kind=code-symbol | source=scripts/zero_caller_trace.py:L1 | neighbors=[scan()]
- "scripts_zero_caller_trace_scan": "scan()" | kind=code-symbol | source=scripts/zero_caller_trace.py:L20 | neighbors=[zero_caller_trace.py]
- "update_wave7_api_keys_rationale_13": "Load all API keys from docs/API/*.json" | kind=entity | source=update_wave7_api_keys.py:L13 | neighbors=[load_api_keys()]
- "update_wave7_api_keys_rationale_33": "Replace API key in a Phase 0 script" | kind=entity | source=update_wave7_api_keys.py:L33 | neighbors=[update_script()]
- "validate_180_method_count_rationale_30": "Parse complexity audit and extract methods > 8." | kind=entity | source=validate_180_method_count.py:L30 | neighbors=[parse_complexity_audit()]
- "validate_180_method_count_rationale_53": "Validate method count matches expected." | kind=entity | source=validate_180_method_count.py:L53 | neighbors=[validate_count()]
- "validate_180_method_count_rationale_64": "Analyze complexity distribution." | kind=entity | source=validate_180_method_count.py:L64 | neighbors=[analyze_distribution()]
- "validate_180_method_count_rationale_83": "Group methods by file." | kind=entity | source=validate_180_method_count.py:L83 | neighbors=[analyze_by_file()]
- "validate_wave6_epic_structure_rationale_12": "Validate Wave 6 epic structure and method counts." | kind=entity | source=validate_wave6_epic_structure.py:L12 | neighbors=[validate_wave6_structure()]
- "wave2_api_balance_tracker_rationale_126": "Estimate bobcoin budget for a phase" | kind=entity | source=scripts/wave2/api_balance_tracker.py:L126 | neighbors=[estimate_phase_budget()]
- "wave2_api_balance_tracker_rationale_143": "Check if we have enough bobcoins for a phase" | kind=entity | source=scripts/wave2/api_balance_tracker.py:L143 | neighbors=[check_phase_feasibility()]
- "wave2_api_balance_tracker_rationale_23": "Load current tracker state, initialize if doesn't exist" | kind=entity | source=scripts/wave2/api_balance_tracker.py:L23 | neighbors=[load_tracker_state()]
- "wave2_api_balance_tracker_rationale_47": "Save tracker state to file" | kind=entity | source=scripts/wave2/api_balance_tracker.py:L47 | neighbors=[save_tracker_state()]
- "wave2_api_balance_tracker_rationale_54": "Record bobcoin usage for an API" | kind=entity | source=scripts/wave2/api_balance_tracker.py:L54 | neighbors=[record_usage()]
- "wave2_api_balance_tracker_rationale_84": "Print summary of all API balances" | kind=entity | source=scripts/wave2/api_balance_tracker.py:L84 | neighbors=[print_summary()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-055.json

Keep each description factual and concise (one sentence). No markdown, no prose
outside the JSON object. It is acceptable to omit a node if context is
insufficient — but include every node you can ground confidently.

Example answer format:
```json
{
  "node_id_1": "Resolves the configured ontology profile from graphify.yaml.",
  "node_id_2": "Colonel James Barclay, an antagonist in The Crooked Man."
}
```
