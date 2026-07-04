# Node Description Batch 15 of 61

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

- "scripts_linear_setup": "linear_setup.py" | kind=code-symbol | source=scripts/linear_setup.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, generate_env_file(), get_teams(), get_users(), main()]
- "scripts_linear_update_status": "linear_update_status.py" | kind=code-symbol | source=scripts/linear_update_status.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, create_issue(), get_api_key(), get_team_id(), list_issues()]
- "scripts_phase_0_hotspot_mcp_fastmcp": "phase_0_hotspot_mcp_fastmcp.py" | kind=code-symbol | source=scripts/phase_0_hotspot_mcp_fastmcp.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "scripts_phase_5_execute_mcp": "phase_5_execute_mcp.py" | kind=code-symbol | source=scripts/phase_5_execute_mcp.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, a14581b feat: Wave 5 preparation - prot…, be6c8a1 docs: Wave 4 documentation merg…, dad3074 feat: Wave 5 preparation - prot…]
- "scripts_phase_5_verify_mcp": "phase_5_verify_mcp.py" | kind=code-symbol | source=scripts/phase_5_verify_mcp.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, a14581b feat: Wave 5 preparation - prot…, be6c8a1 docs: Wave 4 documentation merg…, dad3074 feat: Wave 5 preparation - prot…]
- "scripts_query_codescene_codesceneclient_request": "._request()" | kind=code-symbol | source=scripts/query_codescene.py:L42 | neighbors=[CodeSceneClient, .get_code_health(), .get_file_health(), .get_hotspots(), .get_refactoring_targets(), .list_projects()]
- "scripts_query_kb": "query_kb.py" | kind=code-symbol | source=scripts/query_kb.py:L1 | neighbors=[68cb090 feat(wave7): OKF wiki, phase or…, 7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, _extract_snippet(), init_firestore(), search_kb()]
- "scripts_round26_stress_harness": "round26_stress_harness.py" | kind=code-symbol | source=scripts/round26_stress_harness.py:L1 | neighbors=[build_program_source(), load_pipeline_source(), main(), run_harness(), write_outputs(), write_temp_project()]
- "scripts_session_continuity_main": "main()" | kind=code-symbol | source=scripts/session_continuity.py:L259 | neighbors=[session_continuity.py, SessionContinuity, .auto_snapshot(), .list_checkpoints(), .merge_checkpoints(), .prune_checkpoints()]
- "scripts_session_continuity_sessioncontinuity_auto_snapshot": ".auto_snapshot()" | kind=code-symbol | source=scripts/session_continuity.py:L57 | neighbors=[main(), Create automatic checkpoint if threshol…, SessionContinuity, ._auto_prune(), ._get_checkpoint_path(), ._get_next_checkpoint_num()]
- "scripts_sync_epic_roadmap_from_worker": "sync_epic_roadmap_from_worker.py" | kind=code-symbol | source=scripts/sync_epic_roadmap_from_worker.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, get_completed_epics_from_git(), main()]
- "scripts_test_parallel_phase0": "test_parallel_phase0.py" | kind=code-symbol | source=scripts/test_parallel_phase0.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, execute_phase_0_mcp(), main()]
- "scripts_test_phase_mcp_integration_integrationtester_log": ".log()" | kind=code-symbol | source=scripts/test_phase_mcp_integration.py:L37 | neighbors=[IntegrationTester, .create_test_epic(), .generate_summary(), .test_dependency_validation(), .test_full_workflow(), .test_manifest_initialization()]
- "scripts_test_worker_mcp_client": "test_worker_mcp_client.py" | kind=code-symbol | source=scripts/test_worker_mcp_client.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, test_all_workers(), test_single_worker()]
- "scripts_utils": "utils.py" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/utils.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, improve_description.py, run_eval.py, run_loop.py, parse_skill_md()]
- "scripts_wave2_bob_shell_executor": "wave2_bob_shell_executor.py" | kind=code-symbol | source=scripts/wave2_bob_shell_executor.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, execute_phase_parallel(), execute_phase_with_bob_shell()]
- "scripts_wave7_batch_audit": "wave7_batch_audit.py" | kind=code-symbol | source=scripts/wave7_batch_audit.py:L1 | neighbors=[b0a803b feat(wave7): Phase 2 Architectu…, e01e4e5 wave7: backup all phase 0-5V wo…, audit_epic(), _load_cyc_cache(), main(), _resolve_target_method()]
- "scripts_worker_agent_mcp_call_tool": "call_tool()" | kind=code-symbol | source=scripts/worker_agent_mcp.py:L134 | neighbors=[worker_agent_mcp.py, claim_epic_tool(), execute_epic_tool(), get_next_pending_epic_tool(), get_worker_status_tool(), release_epic_tool()]
- "validate_180_method_count": "validate_180_method_count.py" | kind=code-symbol | source=validate_180_method_count.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, analyze_by_file(), analyze_distribution(), main(), parse_complexity_audit()]
- "wave2_generate_phase2_scripts": "generate_phase2_scripts.py" | kind=code-symbol | source=scripts/wave2/generate_phase2_scripts.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, generate_launcher(), generate_phase2_script()]
- "wave2_generate_phase3_scripts": "generate_phase3_scripts.py" | kind=code-symbol | source=scripts/wave2/generate_phase3_scripts.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, generate_launcher(), generate_phase3_script()]
- "wave2_generate_phase4_scripts": "generate_phase4_scripts.py" | kind=code-symbol | source=scripts/wave2/generate_phase4_scripts.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, generate_launcher(), generate_phase4_script()]
- "wave2_test_single_epic_107_main": "main()" | kind=code-symbol | source=scripts/wave2/test_single_epic_107.py:L99 | neighbors=[test_single_epic_107.py, generate_test_script(), get_epic_data(), load_epic_roadmap(), load_template(), populate_template()]
- "wave2_track_api_balances_main": "main()" | kind=code-symbol | source=scripts/wave2/track_api_balances.py:L225 | neighbors=[track_api_balances.py, calculate_balances(), check_thresholds(), format_status_table(), get_current_balances(), load_api_keys()]
- "wave3_generate_wave3_phase1_scripts": "generate_wave3_phase1_scripts.py" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase1_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 3bcfaac feat: Institutionalize building…, a24ed35 feat: Institutionalize building…, be6c8a1 docs: Wave 4 documentation merg…, generate_launcher_script(), generate_phase1_script()]
- "wave3_generate_wave3_phase3_scripts": "generate_wave3_phase3_scripts.py" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase3_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 3bcfaac feat: Institutionalize building…, a24ed35 feat: Institutionalize building…, be6c8a1 docs: Wave 4 documentation merg…, generate_launcher_script(), generate_phase3_script()]
- "wave3_generate_wave3_phase4_scripts": "generate_wave3_phase4_scripts.py" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase4_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 3bcfaac feat: Institutionalize building…, a24ed35 feat: Institutionalize building…, be6c8a1 docs: Wave 4 documentation merg…, generate_launcher(), generate_phase4_script()]
- "wave4_generate_phase0_scripts": "generate_phase0_scripts.py" | kind=code-symbol | source=scripts/wave4/generate_phase0_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…, generate_scripts(), load_api_keys()]
- "wave4_generate_phase4_recovery": "generate_phase4_recovery.py" | kind=code-symbol | source=scripts/wave4/generate_phase4_recovery.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…, generate_recovery_launcher(), generate_recovery_scripts()]
- "wave4_generate_wave4_phase1_scripts": "generate_wave4_phase1_scripts.py" | kind=code-symbol | source=scripts/wave4/generate_wave4_phase1_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 3bcfaac feat: Institutionalize building…, a24ed35 feat: Institutionalize building…, be6c8a1 docs: Wave 4 documentation merg…, generate_launcher_script(), generate_phase1_script()]
- "wave4_generate_wave4_phase3_scripts": "generate_wave4_phase3_scripts.py" | kind=code-symbol | source=scripts/wave4/generate_wave4_phase3_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 3bcfaac feat: Institutionalize building…, a24ed35 feat: Institutionalize building…, be6c8a1 docs: Wave 4 documentation merg…, generate_launcher_script(), generate_phase3_script()]
- "wave4_generate_wave4_phase4_scripts": "generate_wave4_phase4_scripts.py" | kind=code-symbol | source=scripts/wave4/generate_wave4_phase4_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 3bcfaac feat: Institutionalize building…, a24ed35 feat: Institutionalize building…, be6c8a1 docs: Wave 4 documentation merg…, generate_launcher(), generate_phase4_script()]
- "wave7_generate_phase0_scripts": "generate_phase0_scripts.py" | kind=code-symbol | source=scripts/wave7/generate_phase0_scripts.py:L1 | neighbors=[0953015 Wave 7 Task 7: Master launch sc…, 142bb84 Fix API key list for VM (replac…, 5925e89 Fix API key list for VM (replac…, fb5f81e Wave 7 Task 7: Master launch sc…, generate_scripts(), load_api_keys()]
- "card_board_main_br": "Br()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, jr(), qr(), rr(), Ur(), Wr()]
- "card_board_main_er": "er()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, Ar(), c(), e(), jn(), t()]
- "card_board_main_jr": "jr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, Br(), Kr(), u(), Yr(), Ur()]
- "card_board_main_ln": "Ln()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, gn(), Bn(), c(), On(), u()]
- "card_board_main_nr": "nr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, e(), s(), t(), Ur(), Zn()]
- "card_board_main_qn": "qn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, c(), e(), u(), wn(), yn()]
- "deprecated_tool_bugs_launch_phase0_fixed": "launch_phase0_fixed.py" | kind=code-symbol | source=scripts/wave2/_deprecated_tool_bugs/launch_phase0_fixed.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, create_phase0_script_fixed(), launch()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-014.json

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
