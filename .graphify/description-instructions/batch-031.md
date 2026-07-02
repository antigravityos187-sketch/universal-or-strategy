# Node Description Batch 32 of 61

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
No marketing language.
Respond ONLY with a JSON object mapping each node id (as a string) to its
one-sentence description — no prose, no markdown fences.

- "scripts_precompute_wave7_graph_build_okf_cache": "build_okf_cache()" | kind=code-symbol | source=scripts/precompute_wave7_graph.py:L61 | neighbors=[precompute_wave7_graph.py, main(), Read all 14 OKF .md files and write a s…]
- "scripts_precompute_wave7_graph_build_precomputed": "build_precomputed()" | kind=code-symbol | source=scripts/precompute_wave7_graph.py:L85 | neighbors=[precompute_wave7_graph.py, main(), Build precomputed.json for one epic. Re…]
- "scripts_precompute_wave7_graph_run_complexity_audit": "run_complexity_audit()" | kind=code-symbol | source=scripts/precompute_wave7_graph.py:L39 | neighbors=[precompute_wave7_graph.py, main(), Run complexity_audit.py and return dict…]
- "scripts_preflight_validation_detect_already_complete": "detect_already_complete()" | kind=code-symbol | source=scripts/preflight_validation.py:L117 | neighbors=[preflight_validation.py, preflight_validation(), Detect if epic is already complete with…]
- "scripts_preflight_validation_detect_encoding_issues": "detect_encoding_issues()" | kind=code-symbol | source=scripts/preflight_validation.py:L32 | neighbors=[preflight_validation.py, preflight_validation(), Detect if file requires local execution…]
- "scripts_preflight_validation_detect_invalid_target": "detect_invalid_target()" | kind=code-symbol | source=scripts/preflight_validation.py:L55 | neighbors=[preflight_validation.py, preflight_validation(), Detect if target method exists in speci…]
- "scripts_preflight_validation_detect_test_requirements": "detect_test_requirements()" | kind=code-symbol | source=scripts/preflight_validation.py:L85 | neighbors=[preflight_validation.py, preflight_validation(), Detect if method requires extensive tes…]
- "scripts_preflight_validation_generate_report": "generate_report()" | kind=code-symbol | source=scripts/preflight_validation.py:L260 | neighbors=[preflight_validation.py, main(), Generate markdown report from validatio…]
- "scripts_query_kb_extract_snippet": "_extract_snippet()" | kind=code-symbol | source=scripts/query_kb.py:L50 | neighbors=[query_kb.py, Extract a short snippet around the matc…, search_okf_local()]
- "scripts_query_kb_search_kb": "search_kb()" | kind=code-symbol | source=scripts/query_kb.py:L82 | neighbors=[query_kb.py, Fetches the collection and performs a c…, search_okf_local()]
- "scripts_register_existing_outputs_register_outputs": "register_outputs()" | kind=code-symbol | source=scripts/register_existing_outputs.py:L20 | neighbors=[register_existing_outputs.py, main(), Register existing output files in manif…]
- "scripts_remove_phase_start_from_completed_fix_manifest": "fix_manifest()" | kind=code-symbol | source=scripts/remove_phase_start_from_completed.py:L13 | neighbors=[remove_phase_start_from_completed.py, main(), Remove phase_start events from complete…]
- "scripts_reset_wave6_manifests_main": "main()" | kind=code-symbol | source=scripts/reset_wave6_manifests.py:L46 | neighbors=[reset_wave6_manifests.py, reset_manifest(), Reset all Wave 6 manifests.]
- "scripts_reset_wave6_manifests_reset_manifest": "reset_manifest()" | kind=code-symbol | source=scripts/reset_wave6_manifests.py:L11 | neighbors=[reset_wave6_manifests.py, main(), Reset manifest for a single epic.]
- "scripts_reset_wave6_manifests_v2_reset_manifest": "reset_manifest()" | kind=code-symbol | source=scripts/reset_wave6_manifests_v2.py:L10 | neighbors=[reset_wave6_manifests_v2.py, main(), Reset manifest to minimal state for Pha…]
- "scripts_run_eval_find_project_root": "find_project_root()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/run_eval.py:L22 | neighbors=[run_eval.py, main(), Find the project root by walking up fro…]
- "scripts_run_eval_main": "main()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/run_eval.py:L259 | neighbors=[run_eval.py, find_project_root(), run_eval()]
- "scripts_run_eval_run_eval": "run_eval()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/run_eval.py:L184 | neighbors=[run_eval.py, main(), Run the full eval set and return result…]
- "scripts_run_loop_split_eval_set": "split_eval_set()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/run_loop.py:L24 | neighbors=[run_loop.py, Split eval set into train and test sets…, run_loop()]
- "scripts_session_continuity_sessioncontinuity_auto_prune": "._auto_prune()" | kind=code-symbol | source=scripts/session_continuity.py:L250 | neighbors=[Automatically prune old checkpoints., SessionContinuity, .auto_snapshot()]
- "scripts_session_continuity_sessioncontinuity_get_next_checkpoint_num": "._get_next_checkpoint_num()" | kind=code-symbol | source=scripts/session_continuity.py:L38 | neighbors=[Get next available checkpoint number., SessionContinuity, .auto_snapshot()]
- "scripts_session_continuity_sessioncontinuity_list_checkpoints": ".list_checkpoints()" | kind=code-symbol | source=scripts/session_continuity.py:L137 | neighbors=[main(), List all checkpoints for session., SessionContinuity]
- "scripts_session_continuity_sessioncontinuity_load_session": "._load_session()" | kind=code-symbol | source=scripts/session_continuity.py:L49 | neighbors=[Load current session data., SessionContinuity, .auto_snapshot()]
- "scripts_session_continuity_sessioncontinuity_prune_checkpoints": ".prune_checkpoints()" | kind=code-symbol | source=scripts/session_continuity.py:L234 | neighbors=[main(), Remove old checkpoints, keeping only th…, SessionContinuity]
- "scripts_session_snapshot_sessionsnapshot_init": ".__init__()" | kind=code-symbol | source=scripts/session_snapshot.py:L31 | neighbors=[Initialize a new session., SessionSnapshot, ._save()]
- "scripts_sima_split": "sima_split.py" | kind=code-symbol | source=scripts/sima_split.py:L1 | neighbors=[extract(), make_header(), write_file()]
- "scripts_symmetry_split": "symmetry_split.py" | kind=code-symbol | source=scripts/symmetry_split.py:L1 | neighbors=[extract(), make_header_wrapped(), write_file()]
- "scripts_sync_epic_roadmap_from_worker_get_completed_epics_from_git": "get_completed_epics_from_git()" | kind=code-symbol | source=scripts/sync_epic_roadmap_from_worker.py:L17 | neighbors=[sync_epic_roadmap_from_worker.py, main(), Extract completed epic info from git lo…]
- "scripts_sync_epic_roadmap_from_worker_main": "main()" | kind=code-symbol | source=scripts/sync_epic_roadmap_from_worker.py:L97 | neighbors=[sync_epic_roadmap_from_worker.py, get_completed_epics_from_git(), update_roadmap()]
- "scripts_sync_epic_roadmap_from_worker_update_roadmap": "update_roadmap()" | kind=code-symbol | source=scripts/sync_epic_roadmap_from_worker.py:L66 | neighbors=[sync_epic_roadmap_from_worker.py, main(), Update epic_roadmap.json with completio…]
- "scripts_sync_lamport_events_sync_events_to_global_log": "sync_events_to_global_log()" | kind=code-symbol | source=scripts/sync_lamport_events.py:L10 | neighbors=[sync_lamport_events.py, main(), Sync manifest Lamport events to global …]
- "scripts_test_parallel_phase0_execute_phase_0_mcp": "execute_phase_0_mcp()" | kind=code-symbol | source=scripts/test_parallel_phase0.py:L28 | neighbors=[test_parallel_phase0.py, Execute Phase 0 for a single epic using…, run_parallel_phase0_test()]
- "scripts_test_phase_mcp_servers_mcpservertester_generate_report": ".generate_report()" | kind=code-symbol | source=scripts/test_phase_mcp_servers.py:L269 | neighbors=[main(), MCPServerTester, Generate detailed test report]
- "scripts_test_phase_mcp_servers_mcpservertester_load_config": "._load_config()" | kind=code-symbol | source=scripts/test_phase_mcp_servers.py:L35 | neighbors=[MCPServerTester, .__init__(), Load MCP configuration]
- "scripts_test_v12_52_cleanup_test_data": "cleanup_test_data()" | kind=code-symbol | source=scripts/test_v12_52.py:L55 | neighbors=[test_v12_52.py, Clean up test data from previous runs., run_all_tests()]
- "scripts_test_v12_52_run_all_tests": "run_all_tests()" | kind=code-symbol | source=scripts/test_v12_52.py:L277 | neighbors=[test_v12_52.py, Run all V12.52 tests., cleanup_test_data()]
- "scripts_test_worker_mcp_client_test_all_workers": "test_all_workers()" | kind=code-symbol | source=scripts/test_worker_mcp_client.py:L102 | neighbors=[test_worker_mcp_client.py, Test all 4 worker agents, test_worker_agent()]
- "scripts_test_worker_mcp_client_test_single_worker": "test_single_worker()" | kind=code-symbol | source=scripts/test_worker_mcp_client.py:L123 | neighbors=[test_worker_mcp_client.py, Test a single worker by ID, test_worker_agent()]
- "scripts_ui_ipc_split": "ui_ipc_split.py" | kind=code-symbol | source=scripts/ui_ipc_split.py:L1 | neighbors=[extract(), make_header(), write_file()]
- "scripts_v12_main_split": "v12_main_split.py" | kind=code-symbol | source=scripts/v12_main_split.py:L1 | neighbors=[extract(), make_header(), write_file()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-031.json

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
