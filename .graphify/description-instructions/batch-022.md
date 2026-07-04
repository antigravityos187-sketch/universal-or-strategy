# Node Description Batch 23 of 61

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

- "scripts_monitor_vm_progress_run_ssh_command": "run_ssh_command()" | kind=code-symbol | source=scripts/monitor_vm_progress.py:L52 | neighbors=[monitor_vm_progress.py, check_vm_status(), get_epic_status(), Execute command on VM via gcloud SSH.]
- "scripts_monitor_vm_progress_update_kanban_board": "update_kanban_board()" | kind=code-symbol | source=scripts/monitor_vm_progress.py:L153 | neighbors=[monitor_vm_progress.py, main(), Update Obsidian Kanban board with curre…, create_epic_card()]
- "scripts_negative_evidence_check_negativeevidencecache_clear": ".clear()" | kind=code-symbol | source=scripts/negative_evidence_check.py:L98 | neighbors=[main(), NegativeEvidenceCache, .save(), Clear all negative evidence.]
- "scripts_negative_evidence_check_negativeevidencecache_list_all": ".list_all()" | kind=code-symbol | source=scripts/negative_evidence_check.py:L79 | neighbors=[main(), NegativeEvidenceCache, .load(), List all negative evidence entries.]
- "scripts_negative_evidence_check_negativeevidencecache_load": ".load()" | kind=code-symbol | source=scripts/negative_evidence_check.py:L30 | neighbors=[NegativeEvidenceCache, .check(), .list_all(), .record()]
- "scripts_orchestrate_full_epic_execution_bobcoinbudgetmanager_predict_wave_cost": ".predict_wave_cost()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L66 | neighbors=[BobCoinBudgetManager, .get_average_cost_per_phase(), .execute_wave(), Predict cost for next wave based on his…]
- "scripts_orchestrate_full_epic_execution_epicwaveorchestrator_get_wave_epics": "._get_wave_epics()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L127 | neighbors=[EpicWaveOrchestrator, .execute_phase_all_waves(), .execute_wave(), Get epics for a specific wave.]
- "scripts_orchestrate_full_epic_execution_epicwaveorchestrator_init": ".__init__()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L111 | neighbors=[EpicWaveOrchestrator, BobCoinBudgetManager, ._get_pending_epics(), ._load_roadmap()]
- "scripts_orchestrate_full_epic_execution_main": "main()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L322 | neighbors=[orchestrate_full_epic_execution.py, EpicWaveOrchestrator, .execute_all_phases(), .execute_phase_all_waves()]
- "scripts_orchestrate_phase_execution_phaseorchestrator_get_epics_by_phase": ".get_epics_by_phase()" | kind=code-symbol | source=scripts/orchestrate_phase_execution.py:L54 | neighbors=[PhaseOrchestrator, .execute_wave(), ._is_ready_for_phase(), Get all epics ready for a specific phase]
- "scripts_orchestrate_phase_execution_phaseorchestrator_is_ready_for_phase": "._is_ready_for_phase()" | kind=code-symbol | source=scripts/orchestrate_phase_execution.py:L76 | neighbors=[PhaseOrchestrator, .generate_execution_plan(), .get_epics_by_phase(), Check if epic is ready for a specific p…]
- "scripts_package_skill_package_skill": "package_skill()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/package_skill.py:L42 | neighbors=[package_skill.py, main(), should_exclude(), Package a skill folder into a .skill fi…]
- "scripts_phase_4_5_ticket_review_mcp_execute_phase_4_5": "execute_phase_4_5()" | kind=code-symbol | source=scripts/phase_4_5_ticket_review_mcp.py:L116 | neighbors=[phase_4_5_ticket_review_mcp.py, init_firestore(), query_jane_street_kb(), Execute Phase 4.5 (Ticket Review) for a…]
- "scripts_preflight_validation_main": "main()" | kind=code-symbol | source=scripts/preflight_validation.py:L313 | neighbors=[preflight_validation.py, generate_report(), validate_all_epics(), validate_epic()]
- "scripts_preflight_validation_validate_all_epics": "validate_all_epics()" | kind=code-symbol | source=scripts/preflight_validation.py:L216 | neighbors=[preflight_validation.py, main(), Validate all epics in roadmap., preflight_validation()]
- "scripts_preflight_validation_validate_epic": "validate_epic()" | kind=code-symbol | source=scripts/preflight_validation.py:L194 | neighbors=[preflight_validation.py, main(), Validate a single epic from roadmap., preflight_validation()]
- "scripts_prepare_wave1_phase0": "prepare_wave1_phase0.py" | kind=code-symbol | source=scripts/prepare_wave1_phase0.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…]
- "scripts_query_codescene_codesceneclient_get_code_health": ".get_code_health()" | kind=code-symbol | source=scripts/query_codescene.py:L70 | neighbors=[CodeSceneClient, ._request(), main(), Get code health metrics for a project]
- "scripts_query_codescene_codesceneclient_get_file_health": ".get_file_health()" | kind=code-symbol | source=scripts/query_codescene.py:L78 | neighbors=[CodeSceneClient, ._request(), main(), Get code health for a specific file]
- "scripts_query_codescene_codesceneclient_get_hotspots": ".get_hotspots()" | kind=code-symbol | source=scripts/query_codescene.py:L74 | neighbors=[CodeSceneClient, ._request(), main(), Get hotspot files for a project]
- "scripts_query_codescene_codesceneclient_get_project_id": ".get_project_id()" | kind=code-symbol | source=scripts/query_codescene.py:L62 | neighbors=[CodeSceneClient, .list_projects(), main(), Find project ID by name]
- "scripts_query_codescene_codesceneclient_get_refactoring_targets": ".get_refactoring_targets()" | kind=code-symbol | source=scripts/query_codescene.py:L82 | neighbors=[CodeSceneClient, ._request(), main(), Get recommended refactoring targets]
- "scripts_query_kb_search_okf_local": "search_okf_local()" | kind=code-symbol | source=scripts/query_kb.py:L14 | neighbors=[query_kb.py, Search the local OKF wiki as fallback w…, search_kb(), _extract_snippet()]
- "scripts_quick_validate": "quick_validate.py" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/quick_validate.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, package_skill.py, validate_skill()]
- "scripts_reaper_split": "reaper_split.py" | kind=code-symbol | source=scripts/reaper_split.py:L1 | neighbors=[extract(), main(), read_source_lines(), write_file()]
- "scripts_reaper_split_main": "main()" | kind=code-symbol | source=scripts/reaper_split.py:L69 | neighbors=[reaper_split.py, extract(), read_source_lines(), write_file()]
- "scripts_register_existing_outputs": "register_existing_outputs.py" | kind=code-symbol | source=scripts/register_existing_outputs.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, main(), register_outputs()]
- "scripts_remove_phase_start_from_completed": "remove_phase_start_from_completed.py" | kind=code-symbol | source=scripts/remove_phase_start_from_completed.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, fix_manifest(), main()]
- "scripts_reset_wave6_manifests": "reset_wave6_manifests.py" | kind=code-symbol | source=scripts/reset_wave6_manifests.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, main(), reset_manifest()]
- "scripts_reset_wave6_manifests_v2": "reset_wave6_manifests_v2.py" | kind=code-symbol | source=scripts/reset_wave6_manifests_v2.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, main(), reset_manifest()]
- "scripts_run_loop_run_loop": "run_loop()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/run_loop.py:L47 | neighbors=[run_loop.py, main(), Run the eval + improvement loop., split_eval_set()]
- "scripts_session_continuity_sessioncontinuity_merge_checkpoints": ".merge_checkpoints()" | kind=code-symbol | source=scripts/session_continuity.py:L161 | neighbors=[main(), Merge multiple checkpoints into current…, SessionContinuity, ._get_checkpoint_path()]
- "scripts_session_continuity_sessioncontinuity_restore": ".restore()" | kind=code-symbol | source=scripts/session_continuity.py:L97 | neighbors=[main(), Restore session from checkpoint., SessionContinuity, ._get_checkpoint_path()]
- "scripts_session_snapshot_sessionsnapshot_check_read": ".check_read()" | kind=code-symbol | source=scripts/session_snapshot.py:L79 | neighbors=[main(), Check if file has already been read. Re…, SessionSnapshot, .load()]
- "scripts_session_snapshot_sessionsnapshot_get_state": ".get_state()" | kind=code-symbol | source=scripts/session_snapshot.py:L173 | neighbors=[main(), Display current session state., SessionSnapshot, .load()]
- "scripts_session_snapshot_sessionsnapshot_record_search": ".record_search()" | kind=code-symbol | source=scripts/session_snapshot.py:L123 | neighbors=[main(), SessionSnapshot, .load(), ._save()]
- "scripts_sync_lamport_events": "sync_lamport_events.py" | kind=code-symbol | source=scripts/sync_lamport_events.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, main(), sync_events_to_global_log()]
- "scripts_test_parallel_phase0_run_parallel_phase0_test": "run_parallel_phase0_test()" | kind=code-symbol | source=scripts/test_parallel_phase0.py:L89 | neighbors=[test_parallel_phase0.py, main(), Execute Phase 0 for 3 epics in parallel…, execute_phase_0_mcp()]
- "scripts_test_phase_mcp_integration_integrationtester_create_test_epic": ".create_test_epic()" | kind=code-symbol | source=scripts/test_phase_mcp_integration.py:L49 | neighbors=[IntegrationTester, .log(), main(), Create a test epic for integration test…]
- "scripts_test_phase_mcp_integration_integrationtester_generate_summary": ".generate_summary()" | kind=code-symbol | source=scripts/test_phase_mcp_integration.py:L332 | neighbors=[IntegrationTester, .log(), .test_full_workflow(), Generate test summary]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-022.json

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
