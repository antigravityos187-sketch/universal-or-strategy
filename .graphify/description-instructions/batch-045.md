# Node Description Batch 46 of 61

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

- "scripts_complexity_audit_rationale_206": "Generate full complexity audit report." | kind=entity | source=scripts/complexity_audit.py:L206 | neighbors=[generate_report()]
- "scripts_complexity_audit_rationale_29": "Estimate CYC by counting decision points." | kind=entity | source=scripts/complexity_audit.py:L29 | neighbors=[estimate_cyclomatic_complexity()]
- "scripts_complexity_audit_rationale_52": "Detect M5 dispatch candidates: switch/if chains with >= 4 branches\r     on strin" | kind=entity | source=scripts/complexity_audit.py:L52 | neighbors=[detect_m5_candidate()]
- "scripts_complexity_audit_rationale_95": "Extract all methods from a C# file with metrics." | kind=entity | source=scripts/complexity_audit.py:L95 | neighbors=[extract_methods()]
- "scripts_context7_cli_rationale_14": "Simulates a JSON-RPC call to the Context7 MCP server over stdin/stdout.     Perf" | kind=entity | source=scripts/context7_cli.py:L14 | neighbors=[call_context7_mcp()]
- "scripts_continue_session_rationale_144": "Mark current task as completed.\r     \r     Args:\r         summary: One-line summ" | kind=entity | source=scripts/continue_session.py:L144 | neighbors=[complete_task()]
- "scripts_continue_session_rationale_181": "Generate minimal context block for next session.\r     \r     Returns:\r         Ma" | kind=entity | source=scripts/continue_session.py:L181 | neighbors=[get_minimal_context()]
- "scripts_continue_session_rationale_224": "Display current session status." | kind=entity | source=scripts/continue_session.py:L224 | neighbors=[show_status()]
- "scripts_continue_session_rationale_38": "Create .continue directory if it doesn't exist." | kind=entity | source=scripts/continue_session.py:L38 | neighbors=[ensure_state_dir()]
- "scripts_continue_session_rationale_43": "Get current git branch and commit hash." | kind=entity | source=scripts/continue_session.py:L43 | neighbors=[get_git_info()]
- "scripts_continue_session_rationale_67": "Load state from .continue/state.json." | kind=entity | source=scripts/continue_session.py:L67 | neighbors=[load_state()]
- "scripts_continue_session_rationale_80": "Save state to .continue/state.json." | kind=entity | source=scripts/continue_session.py:L80 | neighbors=[save_state()]
- "scripts_continue_session_rationale_92": "Initialize new /continue session.\r     \r     Args:\r         task_description: De" | kind=entity | source=scripts/continue_session.py:L92 | neighbors=[init_session()]
- "scripts_csharp_hotspots_analyze_complexity": "analyze_complexity()" | kind=code-symbol | source=scripts/csharp_hotspots.py:L19 | neighbors=[csharp_hotspots.py]
- "scripts_diagnose_concurrent_agents_rationale_10": "Diagnose concurrent agent detection for an epic." | kind=entity | source=scripts/diagnose_concurrent_agents.py:L10 | neighbors=[diagnose_epic()]
- "scripts_diff_fixer_fix_with_main_baseline": "fix_with_main_baseline()" | kind=code-symbol | source=scripts/diff_fixer.py:L4 | neighbors=[diff_fixer.py]
- "scripts_epic_manifest_rationale_1020": "Complete phase execution with V12.52 verification and event logging." | kind=entity | source=scripts/epic_manifest.py:L1020 | neighbors=[complete_phase_execution()]
- "scripts_epic_manifest_rationale_1079": "Fail phase execution with V12.52 event logging.\r     \r     Workflow:\r     1. Rec" | kind=entity | source=scripts/epic_manifest.py:L1079 | neighbors=[fail_phase_execution()]
- "scripts_epic_manifest_rationale_1111": "Get Lamport event log for an epic.\r     \r     Args:\r         epic_id: Epic ident" | kind=entity | source=scripts/epic_manifest.py:L1111 | neighbors=[get_event_log()]
- "scripts_epic_manifest_rationale_1131": "Replay workflow from event log (for debugging/recovery).\r     \r     Args:" | kind=entity | source=scripts/epic_manifest.py:L1131 | neighbors=[replay_workflow()]
- "scripts_epic_manifest_rationale_130": "Base exception for manifest operations" | kind=entity | source=scripts/epic_manifest.py:L130 | neighbors=[ManifestError]
- "scripts_epic_manifest_rationale_135": "Raised when manifest validation fails" | kind=entity | source=scripts/epic_manifest.py:L135 | neighbors=[ValidationError]
- "scripts_epic_manifest_rationale_140": "Raised when dependency validation fails" | kind=entity | source=scripts/epic_manifest.py:L140 | neighbors=[DependencyError]
- "scripts_epic_manifest_rationale_145": "Get path to manifest file for an epic" | kind=entity | source=scripts/epic_manifest.py:L145 | neighbors=[_get_manifest_path()]
- "scripts_epic_manifest_rationale_150": "Validate phase ID format" | kind=entity | source=scripts/epic_manifest.py:L150 | neighbors=[_validate_phase_id()]
- "scripts_epic_manifest_rationale_159": "Validate status transition is allowed" | kind=entity | source=scripts/epic_manifest.py:L159 | neighbors=[_validate_status_transition()]
- "scripts_epic_manifest_rationale_174": "Validate artifact path is in correct location" | kind=entity | source=scripts/epic_manifest.py:L174 | neighbors=[_validate_artifact_path()]
- "scripts_epic_manifest_rationale_188": "Validate phase timestamps are in correct order" | kind=entity | source=scripts/epic_manifest.py:L188 | neighbors=[_validate_timestamps()]
- "scripts_epic_manifest_rationale_217": "Detect circular dependencies using DFS.\r     Returns cycle path if found, None o" | kind=entity | source=scripts/epic_manifest.py:L217 | neighbors=[_detect_circular_dependencies()]
- "scripts_epic_manifest_rationale_246": "Load and validate manifest for an epic.\r     \r     Args:\r         epic_id: Epic" | kind=entity | source=scripts/epic_manifest.py:L246 | neighbors=[load_manifest()]
- "scripts_epic_manifest_rationale_341": "Update phase status and outputs in manifest.\r     \r     Uses file locking to pre" | kind=entity | source=scripts/epic_manifest.py:L341 | neighbors=[update_manifest()]
- "scripts_epic_manifest_rationale_474": "Check if all dependencies for a phase are satisfied.\r     \r     A dependency is" | kind=entity | source=scripts/epic_manifest.py:L474 | neighbors=[validate_dependencies()]
- "scripts_epic_manifest_rationale_523": "Determine which phases can be executed next.\r     \r     Returns phases that:" | kind=entity | source=scripts/epic_manifest.py:L523 | neighbors=[get_next_phases()]
- "scripts_epic_manifest_rationale_570": "Create new manifest for an epic.\r     \r     Generates a minimal manifest with Ph" | kind=entity | source=scripts/epic_manifest.py:L570 | neighbors=[generate_manifest()]
- "scripts_epic_manifest_rationale_722": "Add ticket execution and verification phases to manifest.\r     \r     Called afte" | kind=entity | source=scripts/epic_manifest.py:L722 | neighbors=[add_ticket_phases()]
- "scripts_epic_manifest_rationale_848": "V12.52 Blocking Gate: Verify phase can execute with deterministic guarantees." | kind=entity | source=scripts/epic_manifest.py:L848 | neighbors=[verify_can_execute()]
- "scripts_epic_manifest_rationale_899": "Verify filesystem state matches manifest expectations.\r     \r     Dual verificat" | kind=entity | source=scripts/epic_manifest.py:L899 | neighbors=[verify_filesystem_state()]
- "scripts_epic_manifest_rationale_971": "Start phase execution with V12.52 verification and event logging.\r     \r     Wor" | kind=entity | source=scripts/epic_manifest.py:L971 | neighbors=[start_phase_execution()]
- "scripts_epic_planner_load_env": "load_env()" | kind=code-symbol | source=scripts/epic_planner.py:L15 | neighbors=[epic_planner.py]
- "scripts_epic_planner_rationale_140": "Print epic roadmap in human-readable format" | kind=entity | source=scripts/epic_planner.py:L140 | neighbors=[print_roadmap()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-045.json

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
