# Node Description Batch 18 of 61

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

- "scripts_check_wave4_roadmap_discrepancy": "check_wave4_roadmap_discrepancy.py" | kind=code-symbol | source=scripts/check_wave4_roadmap_discrepancy.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…, check_discrepancy()]
- "scripts_context7_cli": "context7_cli.py" | kind=code-symbol | source=scripts/context7_cli.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, call_context7_mcp(), get_api_key(), main()]
- "scripts_continue_session_complete_task": "complete_task()" | kind=code-symbol | source=scripts/continue_session.py:L143 | neighbors=[continue_session.py, load_state(), save_state(), main(), Mark current task as completed.      …]
- "scripts_continue_session_main": "main()" | kind=code-symbol | source=scripts/continue_session.py:L255 | neighbors=[continue_session.py, complete_task(), get_minimal_context(), init_session(), show_status()]
- "scripts_continue_session_save_state": "save_state()" | kind=code-symbol | source=scripts/continue_session.py:L79 | neighbors=[continue_session.py, complete_task(), init_session(), Save state to .continue/state.json., ensure_state_dir()]
- "scripts_epic_manifest_dependencyerror": "DependencyError" | kind=code-symbol | source=scripts/epic_manifest.py:L139 | neighbors=[epic_manifest.py, ManifestError, load_manifest(), Raised when dependency validation fails, validate_dependencies()]
- "scripts_epic_manifest_manifesterror": "ManifestError" | kind=code-symbol | source=scripts/epic_manifest.py:L129 | neighbors=[epic_manifest.py, DependencyError, Exception, Base exception for manifest operations, ValidationError]
- "scripts_epic_planner_generate_epic_roadmap": "generate_epic_roadmap()" | kind=code-symbol | source=scripts/epic_planner.py:L92 | neighbors=[epic_planner.py, calculate_composite_score(), get_codescene_review(), main(), Generate prioritized epic roadmap with …]
- "scripts_filter_wave7_events": "filter_wave7_events.py" | kind=code-symbol | source=scripts/filter_wave7_events.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, filter_wave7_events(), main(), write_wave7_log()]
- "scripts_fix_final_3_epics": "fix_final_3_epics.py" | kind=code-symbol | source=scripts/fix_final_3_epics.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, fix_epic_004(), fix_epic_016(), fix_epic_028()]
- "scripts_generate_phase2_scripts": "generate_phase2_scripts.py" | kind=code-symbol | source=scripts/generate_phase2_scripts.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, generate_phase2_script(), get_epics_needing_phase2(), main()]
- "scripts_generate_phase2_scripts_fixed": "generate_phase2_scripts_fixed.py" | kind=code-symbol | source=scripts/generate_phase2_scripts_fixed.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, generate_phase2_script(), get_epics_needing_phase2(), main()]
- "scripts_generate_phase6_scripts": "generate_phase6_scripts.py" | kind=code-symbol | source=scripts/generate_phase6_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…, main()]
- "scripts_generate_report": "generate_report.py" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/generate_report.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, generate_html(), main(), run_loop.py]
- "scripts_generate_wave7_roadmap": "generate_wave7_roadmap.py" | kind=code-symbol | source=scripts/generate_wave7_roadmap.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, generate_roadmap(), main(), parse_complexity_audit()]
- "scripts_generate_wave7_stats_main": "main()" | kind=code-symbol | source=scripts/generate_wave7_stats.py:L196 | neighbors=[generate_wave7_stats.py, compute_statistics(), load_wave7_events(), print_summary(), write_statistics()]
- "scripts_get_next_epics": "get_next_epics.py" | kind=code-symbol | source=scripts/get_next_epics.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, get_next_pending_epics()]
- "scripts_jane_street_utils_load_violations_for_file": "load_violations_for_file()" | kind=code-symbol | source=scripts/jane_street_utils.py:L96 | neighbors=[jane_street_utils.py, load_violations_file(), load_violations_in_range(), main(), Load violations for a specific file   …]
- "scripts_jane_street_utils_load_violations_for_files": "load_violations_for_files()" | kind=code-symbol | source=scripts/jane_street_utils.py:L117 | neighbors=[jane_street_utils.py, load_violations_file(), main(), Load violations for multiple files    …, validate_no_violations()]
- "scripts_jane_street_utils_load_violations_in_range": "load_violations_in_range()" | kind=code-symbol | source=scripts/jane_street_utils.py:L138 | neighbors=[jane_street_utils.py, .in_range(), load_violations_for_file(), main(), Load violations within a specific line …]
- "scripts_jcodemunch_hook_jcodemunchhook_call_mcp_tool": "._call_mcp_tool()" | kind=code-symbol | source=scripts/jcodemunch_hook.py:L49 | neighbors=[JCodemunchHook, .index_file(), .index_folder(), .register_edit(), Call an MCP tool via subprocess       …]
- "scripts_jcodemunch_hook_jcodemunchhook_index_file": ".index_file()" | kind=code-symbol | source=scripts/jcodemunch_hook.py:L109 | neighbors=[JCodemunchHook, ._call_mcp_tool(), .update_from_commit(), main(), Re-index a single file         Use for…]
- "scripts_jcodemunch_hook_jcodemunchhook_index_folder": ".index_folder()" | kind=code-symbol | source=scripts/jcodemunch_hook.py:L131 | neighbors=[JCodemunchHook, ._call_mcp_tool(), .update_from_commit(), main(), Re-index entire folder         Use for…]
- "scripts_jcodemunch_hook_jcodemunchhook_register_edit": ".register_edit()" | kind=code-symbol | source=scripts/jcodemunch_hook.py:L83 | neighbors=[JCodemunchHook, ._call_mcp_tool(), .update_from_commit(), main(), Register edited files with jCodemunch f…]
- "scripts_lamport_clock_deterministicworkflow_verify_determinism": ".verify_determinism()" | kind=code-symbol | source=scripts/lamport_clock.py:L189 | neighbors=[DeterministicWorkflow, .check_dependencies(), .get_event_log(), Verify workflow determinism for an epic…, verify_can_execute()]
- "scripts_lamport_clock_verify_can_execute": "verify_can_execute()" | kind=code-symbol | source=scripts/lamport_clock.py:L421 | neighbors=[lamport_clock.py, Verify phase can execute (dependencies …, .check_dependencies(), .verify_determinism(), get_workflow()]
- "scripts_linear_setup_main": "main()" | kind=code-symbol | source=scripts/linear_setup.py:L168 | neighbors=[linear_setup.py, generate_env_file(), get_teams(), get_users(), test_connection()]
- "scripts_linear_sync": "linear_sync.py" | kind=code-symbol | source=scripts/linear_sync.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, LinearIssue, LinearSync, main()]
- "scripts_linear_sync_linearsync_create_epic": ".create_epic()" | kind=code-symbol | source=scripts/linear_sync.py:L146 | neighbors=[LinearSync, .find_project_by_name(), .update_project(), .sync_to_linear(), Create or update a Linear epic (project…]
- "scripts_linear_sync_v2": "linear_sync_v2.py" | kind=code-symbol | source=scripts/linear_sync_v2.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, LinearIssue, LinearSync, main()]
- "scripts_linear_sync_v2_linearsync_get_or_create_project": ".get_or_create_project()" | kind=code-symbol | source=scripts/linear_sync_v2.py:L144 | neighbors=[LinearSync, .find_project_by_name(), .update_project(), .sync_to_linear(), Get existing project or create new one.]
- "scripts_linear_update_status_main": "main()" | kind=code-symbol | source=scripts/linear_update_status.py:L177 | neighbors=[linear_update_status.py, create_issue(), get_api_key(), get_team_id(), list_issues()]
- "scripts_mark_phases_complete": "mark_phases_complete.py" | kind=code-symbol | source=scripts/mark_phases_complete.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, add_phase_to_manifest(), main(), mark_phase_complete()]
- "scripts_migrate_manifests_v12_52": "migrate_manifests_v12_52.py" | kind=code-symbol | source=scripts/migrate_manifests_v12_52.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, find_epics_needing_migration(), main(), migrate_manifest()]
- "scripts_monitor_vm_progress_main": "main()" | kind=code-symbol | source=scripts/monitor_vm_progress.py:L197 | neighbors=[monitor_vm_progress.py, check_vm_status(), get_epic_status(), update_kanban_board(), Main monitoring loop.]
- "scripts_negative_evidence_check_negativeevidencecache_check": ".check()" | kind=code-symbol | source=scripts/negative_evidence_check.py:L44 | neighbors=[main(), NegativeEvidenceCache, .load(), .record(), Check if query has negative evidence. R…]
- "scripts_orchestrate_full_epic_execution_epicwaveorchestrator_execute_all_phases": ".execute_all_phases()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L271 | neighbors=[EpicWaveOrchestrator, .execute_phase_all_waves(), ._print_final_summary(), main(), Execute all phases for all epics.]
- "scripts_orchestrate_phase_execution_main": "main()" | kind=code-symbol | source=scripts/orchestrate_phase_execution.py:L241 | neighbors=[orchestrate_phase_execution.py, PhaseOrchestrator, .execute_phase(), .execute_wave(), .generate_execution_plan()]
- "scripts_orchestrate_phase_execution_phaseorchestrator_execute_phase": ".execute_phase()" | kind=code-symbol | source=scripts/orchestrate_phase_execution.py:L104 | neighbors=[main(), PhaseOrchestrator, .get_epic(), .execute_wave(), Execute a single phase for an epic]
- "scripts_orchestrate_phase_execution_phaseorchestrator_execute_wave": ".execute_wave()" | kind=code-symbol | source=scripts/orchestrate_phase_execution.py:L160 | neighbors=[main(), PhaseOrchestrator, .execute_phase(), .get_epics_by_phase(), Execute a phase for all ready epics in …]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-017.json

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
