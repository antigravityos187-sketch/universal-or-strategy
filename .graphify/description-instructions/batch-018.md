# Node Description Batch 19 of 61

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

- "scripts_orchestrate_phase_execution_phaseorchestrator_generate_execution_plan": ".generate_execution_plan()" | kind=code-symbol | source=scripts/orchestrate_phase_execution.py:L206 | neighbors=[main(), PhaseOrchestrator, .get_epic(), ._is_ready_for_phase(), Generate complete execution plan for an…]
- "scripts_orchestrate_phase0_with_prep_execute_phase0_with_prep": "execute_phase0_with_prep()" | kind=code-symbol | source=scripts/orchestrate_phase0_with_prep.py:L120 | neighbors=[orchestrate_phase0_with_prep.py, call_phase0_mcp(), prepare_jcodemunch_data(), main(), Execute Phase 0 with orchestrator-level…]
- "scripts_phase_1_5_boundary_mcp": "phase_1_5_boundary_mcp.py" | kind=code-symbol | source=scripts/phase_1_5_boundary_mcp.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, execute_phase_1_5()]
- "scripts_phase_1_scope_mcp_execute_phase_1_tool": "execute_phase_1_tool()" | kind=code-symbol | source=scripts/phase_1_scope_mcp.py:L59 | neighbors=[phase_1_scope_mcp.py, call_tool(), create_extraction_scope(), create_no_action_scope(), Execute Phase 1: Scope Definition.]
- "scripts_phase_1_scope_mcp_fastmcp": "phase_1_scope_mcp_fastmcp.py" | kind=code-symbol | source=scripts/phase_1_scope_mcp_fastmcp.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, execute_phase_1()]
- "scripts_phase_2_architecture_mcp": "phase_2_architecture_mcp.py" | kind=code-symbol | source=scripts/phase_2_architecture_mcp.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, execute_phase_2()]
- "scripts_phase_3_audit_mcp": "phase_3_audit_mcp.py" | kind=code-symbol | source=scripts/phase_3_audit_mcp.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, execute_phase_3()]
- "scripts_phase_4_tickets_mcp": "phase_4_tickets_mcp.py" | kind=code-symbol | source=scripts/phase_4_tickets_mcp.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, execute_phase_4()]
- "scripts_phase_6_review_mcp": "phase_6_review_mcp.py" | kind=code-symbol | source=scripts/phase_6_review_mcp.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, execute_phase_6()]
- "scripts_precompute_wave7_graph_main": "main()" | kind=code-symbol | source=scripts/precompute_wave7_graph.py:L152 | neighbors=[precompute_wave7_graph.py, build_okf_cache(), build_precomputed(), load_epic_list(), run_complexity_audit()]
- "scripts_query_codescene": "query_codescene.py" | kind=code-symbol | source=scripts/query_codescene.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, CodeSceneClient, load_env(), main()]
- "scripts_query_codescene_codesceneclient_list_projects": ".list_projects()" | kind=code-symbol | source=scripts/query_codescene.py:L57 | neighbors=[CodeSceneClient, .get_project_id(), ._request(), main(), List all CodeScene projects]
- "scripts_session_continuity_sessioncontinuity_get_checkpoint_path": "._get_checkpoint_path()" | kind=code-symbol | source=scripts/session_continuity.py:L34 | neighbors=[Get path for checkpoint file., SessionContinuity, .auto_snapshot(), .merge_checkpoints(), .restore()]
- "scripts_session_snapshot_sessionsnapshot_record_negative_evidence": ".record_negative_evidence()" | kind=code-symbol | source=scripts/session_snapshot.py:L138 | neighbors=[main(), Record failed search (negative evidence…, SessionSnapshot, .load(), ._save()]
- "scripts_session_snapshot_sessionsnapshot_record_read": ".record_read()" | kind=code-symbol | source=scripts/session_snapshot.py:L87 | neighbors=[main(), Record a file read operation., SessionSnapshot, .load(), ._save()]
- "scripts_session_snapshot_sessionsnapshot_record_symbol": ".record_symbol()" | kind=code-symbol | source=scripts/session_snapshot.py:L108 | neighbors=[main(), Record symbol exploration., SessionSnapshot, .load(), ._save()]
- "scripts_session_snapshot_sessionsnapshot_update_budget": ".update_budget()" | kind=code-symbol | source=scripts/session_snapshot.py:L152 | neighbors=[main(), Update token budget consumption., SessionSnapshot, .load(), ._save()]
- "scripts_test_fastmcp_phase0": "test_fastmcp_phase0.py" | kind=code-symbol | source=scripts/test_fastmcp_phase0.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, test_phase0_mcp()]
- "scripts_test_phase_mcp_servers_main": "main()" | kind=code-symbol | source=scripts/test_phase_mcp_servers.py:L279 | neighbors=[test_phase_mcp_servers.py, MCPServerTester, .generate_report(), .test_all_phase_servers(), .test_server_config()]
- "scripts_test_phase_mcp_servers_mcpservertester_test_all_phase_servers": ".test_all_phase_servers()" | kind=code-symbol | source=scripts/test_phase_mcp_servers.py:L197 | neighbors=[main(), MCPServerTester, .test_script_syntax(), .test_server_config(), Test all phase MCP servers]
- "scripts_test_phase_mcp_servers_mcpservertester_test_server_config": ".test_server_config()" | kind=code-symbol | source=scripts/test_phase_mcp_servers.py:L54 | neighbors=[main(), MCPServerTester, .test_all_phase_servers(), ._log(), Test server configuration]
- "scripts_v12_split": "v12_split.py" | kind=code-symbol | source=scripts/v12_split.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, extract_method_block(), main(), split_method()]
- "scripts_validate_epic_claim_epic": "claim_epic()" | kind=code-symbol | source=scripts/validate_epic.py:L56 | neighbors=[validate_epic.py, load_roadmap(), save_roadmap(), main(), Atomically claim epic for worker using …]
- "scripts_validate_epic_release_epic": "release_epic()" | kind=code-symbol | source=scripts/validate_epic.py:L111 | neighbors=[validate_epic.py, main(), Release epic lock (on completion or fai…, load_roadmap(), save_roadmap()]
- "scripts_verify_wave7_determinism_verify_all_epics": "verify_all_epics()" | kind=code-symbol | source=scripts/verify_wave7_determinism.py:L76 | neighbors=[verify_wave7_determinism.py, main(), Verify determinism for all Wave 7 epics…, get_wave7_epics(), verify_epic()]
- "scripts_verify_wave7_templates_main": "main()" | kind=code-symbol | source=scripts/verify_wave7_templates.py:L185 | neighbors=[verify_wave7_templates.py, check_file_exists(), print_summary(), verify_template(), Main verification routine.]
- "scripts_wave7_batch_audit_audit_epic": "audit_epic()" | kind=code-symbol | source=scripts/wave7_batch_audit.py:L323 | neighbors=[wave7_batch_audit.py, _load_cyc_cache(), _resolve_target_method(), Audit a single epic for a given phase. …, run_batch_audit()]
- "scripts_worker_agent_mcp_fastmcp_claim_epic": "claim_epic()" | kind=code-symbol | source=scripts/worker_agent_mcp_fastmcp.py:L64 | neighbors=[worker_agent_mcp_fastmcp.py, load_roadmap(), run_command(), save_roadmap(), Atomically claim an epic for this worke…]
- "scripts_worker_agent_mcp_fastmcp_load_roadmap": "load_roadmap()" | kind=code-symbol | source=scripts/worker_agent_mcp_fastmcp.py:L27 | neighbors=[worker_agent_mcp_fastmcp.py, claim_epic(), get_next_pending_epic(), get_worker_status(), release_epic()]
- "scripts_worker_agent_mcp_fastmcp_release_epic": "release_epic()" | kind=code-symbol | source=scripts/worker_agent_mcp_fastmcp.py:L210 | neighbors=[worker_agent_mcp_fastmcp.py, Release epic lock after completion or f…, load_roadmap(), run_command(), save_roadmap()]
- "scripts_worker_agent_mcp_fastmcp_run_command": "run_command()" | kind=code-symbol | source=scripts/worker_agent_mcp_fastmcp.py:L39 | neighbors=[worker_agent_mcp_fastmcp.py, claim_epic(), execute_epic(), Execute shell command and return result, release_epic()]
- "scripts_worker_agent_mcp_load_roadmap": "load_roadmap()" | kind=code-symbol | source=scripts/worker_agent_mcp.py:L30 | neighbors=[worker_agent_mcp.py, claim_epic_tool(), get_next_pending_epic_tool(), get_worker_status_tool(), release_epic_tool()]
- "scripts_worker_agent_mcp_release_epic_tool": "release_epic_tool()" | kind=code-symbol | source=scripts/worker_agent_mcp.py:L314 | neighbors=[worker_agent_mcp.py, call_tool(), load_roadmap(), run_command(), save_roadmap()]
- "scripts_worker_agent_mcp_run_command": "run_command()" | kind=code-symbol | source=scripts/worker_agent_mcp.py:L42 | neighbors=[worker_agent_mcp.py, claim_epic_tool(), execute_epic_tool(), Execute shell command and return result, release_epic_tool()]
- "update_wave7_api_keys": "update_wave7_api_keys.py" | kind=code-symbol | source=update_wave7_api_keys.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, load_api_keys(), main(), update_script()]
- "validate_180_method_count_main": "main()" | kind=code-symbol | source=validate_180_method_count.py:L89 | neighbors=[validate_180_method_count.py, analyze_by_file(), analyze_distribution(), parse_complexity_audit(), validate_count()]
- "wave2_api_balance_tracker_load_tracker_state": "load_tracker_state()" | kind=code-symbol | source=scripts/wave2/api_balance_tracker.py:L22 | neighbors=[api_balance_tracker.py, check_phase_feasibility(), print_summary(), Load current tracker state, initialize …, record_usage()]
- "wave2_check_phase4_local": "check_phase4_local.py" | kind=code-symbol | source=scripts/wave2/check_phase4_local.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, check_phase4_status()]
- "wave2_generate_phase5_scripts_main": "main()" | kind=code-symbol | source=scripts/wave2/generate_phase5_scripts.py:L195 | neighbors=[generate_phase5_scripts.py, copy_and_modify_phase4_to_phase5_ticket…, copy_and_modify_phase4_to_phase5_valida…, copy_and_modify_phase4_to_phase6_review…, generate_gated_launcher()]
- "wave2_launch_wave_gcloud": "gcloud()" | kind=code-symbol | source=scripts/wave2/launch_wave.py:L47 | neighbors=[launch_wave.py, collect_results(), run(), launch_wave(), monitor_wave()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-018.json

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
