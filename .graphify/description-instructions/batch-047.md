# Node Description Batch 48 of 61

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

- "scripts_jane_street_utils_janestreetviolation_to_dict": ".to_dict()" | kind=code-symbol | source=scripts/jane_street_utils.py:L48 | neighbors=[JaneStreetViolation]
- "scripts_jane_street_utils_rationale_118": "Load violations for multiple files\r     \r     Args:\r         file_paths: List of" | kind=entity | source=scripts/jane_street_utils.py:L118 | neighbors=[load_violations_for_files()]
- "scripts_jane_street_utils_rationale_139": "Load violations within a specific line range in a file\r     \r     Args:" | kind=entity | source=scripts/jane_street_utils.py:L139 | neighbors=[load_violations_in_range()]
- "scripts_jane_street_utils_rationale_159": "Get summary statistics for a list of violations\r     \r     Returns:\r         Dic" | kind=entity | source=scripts/jane_street_utils.py:L159 | neighbors=[get_violation_summary()]
- "scripts_jane_street_utils_rationale_202": "Format violations as a markdown report\r     \r     Args:\r         violations: Lis" | kind=entity | source=scripts/jane_street_utils.py:L202 | neighbors=[format_violation_report()]
- "scripts_jane_street_utils_rationale_250": "Query Jane Street Firebase KB\r     \r     Args:\r         query: Search term" | kind=entity | source=scripts/jane_street_utils.py:L250 | neighbors=[query_kb()]
- "scripts_jane_street_utils_rationale_283": "Validate that files have no Jane Street violations\r     \r     Args:\r         fil" | kind=entity | source=scripts/jane_street_utils.py:L283 | neighbors=[validate_no_violations()]
- "scripts_jane_street_utils_rationale_297": "Get set of all files that have Jane Street violations\r     \r     Returns:" | kind=entity | source=scripts/jane_street_utils.py:L297 | neighbors=[get_files_with_violations()]
- "scripts_jane_street_utils_rationale_308": "CLI interface for testing" | kind=entity | source=scripts/jane_street_utils.py:L308 | neighbors=[main()]
- "scripts_jane_street_utils_rationale_31": "Represents a single Jane Street violation" | kind=entity | source=scripts/jane_street_utils.py:L31 | neighbors=[JaneStreetViolation]
- "scripts_jane_street_utils_rationale_63": "Check if violation is within line range" | kind=entity | source=scripts/jane_street_utils.py:L63 | neighbors=[.in_range()]
- "scripts_jane_street_utils_rationale_68": "Load all violations from jane_street_p0_violations.json\r     \r     Returns:" | kind=entity | source=scripts/jane_street_utils.py:L68 | neighbors=[load_violations_file()]
- "scripts_jane_street_utils_rationale_97": "Load violations for a specific file\r     \r     Args:\r         file_path: Relativ" | kind=entity | source=scripts/jane_street_utils.py:L97 | neighbors=[load_violations_for_file()]
- "scripts_jcodemunch_hook_jcodemunchhook_init": ".__init__()" | kind=code-symbol | source=scripts/jcodemunch_hook.py:L45 | neighbors=[JCodemunchHook]
- "scripts_jcodemunch_hook_rationale_110": "Re-index a single file\r         Use for small changes (<10 files)" | kind=entity | source=scripts/jcodemunch_hook.py:L110 | neighbors=[.index_file()]
- "scripts_jcodemunch_hook_rationale_132": "Re-index entire folder\r         Use for large changes (>10 files)" | kind=entity | source=scripts/jcodemunch_hook.py:L132 | neighbors=[.index_folder()]
- "scripts_jcodemunch_hook_rationale_154": "Update jCodemunch index based on files changed in a commit\r         Strategy: <1" | kind=entity | source=scripts/jcodemunch_hook.py:L154 | neighbors=[.update_from_commit()]
- "scripts_jcodemunch_hook_rationale_204": "CLI entry point for git hooks" | kind=entity | source=scripts/jcodemunch_hook.py:L204 | neighbors=[main()]
- "scripts_jcodemunch_hook_rationale_43": "Wrapper for jCodemunch MCP operations in git hooks" | kind=entity | source=scripts/jcodemunch_hook.py:L43 | neighbors=[JCodemunchHook]
- "scripts_jcodemunch_hook_rationale_50": "Call an MCP tool via subprocess\r         This uses the MCP stdio protocol to com" | kind=entity | source=scripts/jcodemunch_hook.py:L50 | neighbors=[._call_mcp_tool()]
- "scripts_jcodemunch_hook_rationale_69": "# TODO: Implement actual MCP stdio communication" | kind=entity | source=scripts/jcodemunch_hook.py:L69 | neighbors=[jcodemunch_hook.py]
- "scripts_jcodemunch_hook_rationale_84": "Register edited files with jCodemunch for cache invalidation\r         This is fa" | kind=entity | source=scripts/jcodemunch_hook.py:L84 | neighbors=[.register_edit()]
- "scripts_lamport_clock_rationale_108": "Increment global clock (atomic operation)." | kind=entity | source=scripts/lamport_clock.py:L108 | neighbors=[.tick()]
- "scripts_lamport_clock_rationale_122": "Record deterministic event with state hash.\r         \r         Args:" | kind=entity | source=scripts/lamport_clock.py:L122 | neighbors=[.record_event()]
- "scripts_lamport_clock_rationale_159": "Get event log, optionally filtered.\r         \r         Args:\r             epic_i" | kind=entity | source=scripts/lamport_clock.py:L159 | neighbors=[.get_event_log()]
- "scripts_lamport_clock_rationale_190": "Verify workflow determinism for an epic/phase.\r         \r         Checks:" | kind=entity | source=scripts/lamport_clock.py:L190 | neighbors=[.verify_determinism()]
- "scripts_lamport_clock_rationale_230": "Check if all dependencies for a phase are satisfied.\r         \r         Phase de" | kind=entity | source=scripts/lamport_clock.py:L230 | neighbors=[.check_dependencies()]
- "scripts_lamport_clock_rationale_25": "Deterministic workflow engine using Lamport clocks.\r     \r     Guarantees:" | kind=entity | source=scripts/lamport_clock.py:L25 | neighbors=[DeterministicWorkflow]
- "scripts_lamport_clock_rationale_304": "Load lamport_events from manifest as fallback.\r         \r         This handles m" | kind=entity | source=scripts/lamport_clock.py:L304 | neighbors=[._load_manifest_events()]
- "scripts_lamport_clock_rationale_339": "Get next executable phases in deterministic order.\r         \r         Returns ph" | kind=entity | source=scripts/lamport_clock.py:L339 | neighbors=[.get_next_phases()]
- "scripts_lamport_clock_rationale_379": "Replay workflow from event log (for debugging/recovery).\r         \r         Args" | kind=entity | source=scripts/lamport_clock.py:L379 | neighbors=[.replay_workflow()]
- "scripts_lamport_clock_rationale_395": "Get or create global workflow instance." | kind=entity | source=scripts/lamport_clock.py:L395 | neighbors=[get_workflow()]
- "scripts_lamport_clock_rationale_404": "Record phase start event." | kind=entity | source=scripts/lamport_clock.py:L404 | neighbors=[record_phase_start()]
- "scripts_lamport_clock_rationale_410": "Record phase completion event." | kind=entity | source=scripts/lamport_clock.py:L410 | neighbors=[record_phase_complete()]
- "scripts_lamport_clock_rationale_416": "Record phase failure event." | kind=entity | source=scripts/lamport_clock.py:L416 | neighbors=[record_phase_fail()]
- "scripts_lamport_clock_rationale_422": "Verify phase can execute (dependencies satisfied, deterministic)." | kind=entity | source=scripts/lamport_clock.py:L422 | neighbors=[verify_can_execute()]
- "scripts_lamport_clock_rationale_45": "Load global logical clock (monotonically increasing)." | kind=entity | source=scripts/lamport_clock.py:L45 | neighbors=[._load_global_clock()]
- "scripts_lamport_clock_rationale_53": "Save global logical clock." | kind=entity | source=scripts/lamport_clock.py:L53 | neighbors=[._save_global_clock()]
- "scripts_lamport_clock_rationale_61": "Append event to immutable log (JSONL format)." | kind=entity | source=scripts/lamport_clock.py:L61 | neighbors=[._append_event()]
- "scripts_lamport_clock_rationale_66": "Compute deterministic hash of epic state.\r         \r         Includes:" | kind=entity | source=scripts/lamport_clock.py:L66 | neighbors=[._compute_state_hash()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-047.json

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
