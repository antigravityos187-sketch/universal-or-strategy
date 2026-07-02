# Node Description Batch 31 of 61

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

- "scripts_linear_update_status_create_issue": "create_issue()" | kind=code-symbol | source=scripts/linear_update_status.py:L70 | neighbors=[linear_update_status.py, main(), Create a new Linear issue.]
- "scripts_linear_update_status_get_api_key": "get_api_key()" | kind=code-symbol | source=scripts/linear_update_status.py:L16 | neighbors=[linear_update_status.py, main(), Get LINEAR_API_KEY from environment.]
- "scripts_linear_update_status_get_team_id": "get_team_id()" | kind=code-symbol | source=scripts/linear_update_status.py:L25 | neighbors=[linear_update_status.py, main(), Get the team ID from Linear.]
- "scripts_linear_update_status_list_issues": "list_issues()" | kind=code-symbol | source=scripts/linear_update_status.py:L130 | neighbors=[linear_update_status.py, main(), List issues in Linear.]
- "scripts_load_api_keys_calculate_key_distribution": "calculate_key_distribution()" | kind=code-symbol | source=scripts/load_api_keys.py:L60 | neighbors=[load_api_keys.py, main(), Calculate how to distribute epics acros…]
- "scripts_load_api_keys_format_keys_for_executor": "format_keys_for_executor()" | kind=code-symbol | source=scripts/load_api_keys.py:L103 | neighbors=[load_api_keys.py, main(), Format keys as comma-separated string f…]
- "scripts_load_api_keys_load_api_keys_from_folder": "load_api_keys_from_folder()" | kind=code-symbol | source=scripts/load_api_keys.py:L44 | neighbors=[load_api_keys.py, main(), Load all API keys from JSON files in fo…]
- "scripts_mark_phases_complete_add_phase_to_manifest": "add_phase_to_manifest()" | kind=code-symbol | source=scripts/mark_phases_complete.py:L62 | neighbors=[mark_phases_complete.py, main(), Add a phase to manifest if it doesn't e…]
- "scripts_mark_phases_complete_main": "main()" | kind=code-symbol | source=scripts/mark_phases_complete.py:L93 | neighbors=[mark_phases_complete.py, add_phase_to_manifest(), mark_phase_complete()]
- "scripts_mark_phases_complete_mark_phase_complete": "mark_phase_complete()" | kind=code-symbol | source=scripts/mark_phases_complete.py:L12 | neighbors=[mark_phases_complete.py, main(), Mark a phase as complete with synthetic…]
- "scripts_measure_kb_size": "measure_kb_size.py" | kind=code-symbol | source=scripts/measure_kb_size.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, main()]
- "scripts_migrate_manifests_to_v12_52_main": "main()" | kind=code-symbol | source=scripts/migrate_manifests_to_v12_52.py:L148 | neighbors=[migrate_manifests_to_v12_52.py, migrate_manifest(), Main migration function.]
- "scripts_migrate_manifests_to_v12_52_migrate_manifest": "migrate_manifest()" | kind=code-symbol | source=scripts/migrate_manifests_to_v12_52.py:L54 | neighbors=[migrate_manifests_to_v12_52.py, main(), Migrate a single manifest to V12.52 sch…]
- "scripts_migrate_manifests_v12_52_find_epics_needing_migration": "find_epics_needing_migration()" | kind=code-symbol | source=scripts/migrate_manifests_v12_52.py:L103 | neighbors=[migrate_manifests_v12_52.py, main(), Find all epics with completed Phase 0 b…]
- "scripts_migrate_manifests_v12_52_main": "main()" | kind=code-symbol | source=scripts/migrate_manifests_v12_52.py:L125 | neighbors=[migrate_manifests_v12_52.py, find_epics_needing_migration(), migrate_manifest()]
- "scripts_migrate_manifests_v12_52_migrate_manifest": "migrate_manifest()" | kind=code-symbol | source=scripts/migrate_manifests_v12_52.py:L22 | neighbors=[migrate_manifests_v12_52.py, main(), Migrate a single manifest to V12.52 for…]
- "scripts_monitor_vm_progress_create_epic_card": "create_epic_card()" | kind=code-symbol | source=scripts/monitor_vm_progress.py:L135 | neighbors=[monitor_vm_progress.py, Create Kanban card text for an epic., update_kanban_board()]
- "scripts_negative_evidence_check_negativeevidencecache_save": ".save()" | kind=code-symbol | source=scripts/negative_evidence_check.py:L38 | neighbors=[NegativeEvidenceCache, .clear(), .record()]
- "scripts_nexus_relay_relay_to_agent": "relay_to_agent()" | kind=code-symbol | source=scripts/nexus_relay.py:L12 | neighbors=[nexus_relay.py, main(), Formalizes the handoff to a sub-agent a…]
- "scripts_orchestrate_full_epic_execution_bobcoinbudgetmanager_check_balance": ".check_balance()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L39 | neighbors=[BobCoinBudgetManager, .execute_wave(), Check current BobCoin balance.        …]
- "scripts_orchestrate_full_epic_execution_bobcoinbudgetmanager_get_average_cost_per_phase": ".get_average_cost_per_phase()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L59 | neighbors=[BobCoinBudgetManager, .predict_wave_cost(), Calculate average cost per epic for a s…]
- "scripts_orchestrate_full_epic_execution_bobcoinbudgetmanager_needs_refill": ".needs_refill()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L73 | neighbors=[BobCoinBudgetManager, .execute_wave(), Check if refill is needed.]
- "scripts_orchestrate_full_epic_execution_bobcoinbudgetmanager_prompt_refill": ".prompt_refill()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L81 | neighbors=[BobCoinBudgetManager, .execute_wave(), Prompt user to refill BobCoins.]
- "scripts_orchestrate_full_epic_execution_bobcoinbudgetmanager_record_cost": ".record_cost()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L50 | neighbors=[BobCoinBudgetManager, .execute_wave(), Record cost for a phase/wave execution.]
- "scripts_orchestrate_full_epic_execution_epicwaveorchestrator_execute_phase_for_epic": "._execute_phase_for_epic()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L133 | neighbors=[EpicWaveOrchestrator, .execute_wave(), Execute a single phase for a single epi…]
- "scripts_orchestrate_full_epic_execution_epicwaveorchestrator_get_pending_epics": "._get_pending_epics()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L123 | neighbors=[EpicWaveOrchestrator, .__init__(), Get list of pending epics.]
- "scripts_orchestrate_full_epic_execution_epicwaveorchestrator_print_final_summary": "._print_final_summary()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L290 | neighbors=[EpicWaveOrchestrator, .execute_all_phases(), Print final execution summary.]
- "scripts_orchestrate_phase_execution_phaseorchestrator_get_epic": ".get_epic()" | kind=code-symbol | source=scripts/orchestrate_phase_execution.py:L47 | neighbors=[PhaseOrchestrator, .execute_phase(), .generate_execution_plan()]
- "scripts_orchestrate_phase0_with_prep_call_phase0_mcp": "call_phase0_mcp()" | kind=code-symbol | source=scripts/orchestrate_phase0_with_prep.py:L81 | neighbors=[orchestrate_phase0_with_prep.py, execute_phase0_with_prep(), Call Phase 0 MCP server with pre-fetche…]
- "scripts_orchestrate_phase0_with_prep_main": "main()" | kind=code-symbol | source=scripts/orchestrate_phase0_with_prep.py:L162 | neighbors=[orchestrate_phase0_with_prep.py, execute_phase0_with_prep(), Main entry point for CLI usage.]
- "scripts_orchestrate_phase0_with_prep_prepare_jcodemunch_data": "prepare_jcodemunch_data()" | kind=code-symbol | source=scripts/orchestrate_phase0_with_prep.py:L24 | neighbors=[orchestrate_phase0_with_prep.py, execute_phase0_with_prep(), Pre-fetch jCodemunch data for an epic m…]
- "scripts_orders_callbacks_split": "orders_callbacks_split.py" | kind=code-symbol | source=scripts/orders_callbacks_split.py:L1 | neighbors=[extract(), make_header(), write_file()]
- "scripts_orders_management_split": "orders_management_split.py" | kind=code-symbol | source=scripts/orders_management_split.py:L1 | neighbors=[extract(), make_header(), write_file()]
- "scripts_package_skill_should_exclude": "should_exclude()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/package_skill.py:L27 | neighbors=[package_skill.py, package_skill(), Check if a path should be excluded from…]
- "scripts_phase_0_hotspot_mcp_call_tool": "call_tool()" | kind=code-symbol | source=scripts/phase_0_hotspot_mcp.py:L46 | neighbors=[phase_0_hotspot_mcp.py, execute_phase_0_tool(), Handle MCP tool calls]
- "scripts_phase_0_hotspot_mcp_execute_phase_0_tool": "execute_phase_0_tool()" | kind=code-symbol | source=scripts/phase_0_hotspot_mcp.py:L53 | neighbors=[phase_0_hotspot_mcp.py, call_tool(), Return context immediately - no blockin…]
- "scripts_phase_1_scope_mcp_create_extraction_scope": "create_extraction_scope()" | kind=code-symbol | source=scripts/phase_1_scope_mcp.py:L234 | neighbors=[phase_1_scope_mcp.py, execute_phase_1_tool(), Create scope document for extraction ep…]
- "scripts_phase_1_scope_mcp_create_no_action_scope": "create_no_action_scope()" | kind=code-symbol | source=scripts/phase_1_scope_mcp.py:L171 | neighbors=[phase_1_scope_mcp.py, execute_phase_1_tool(), Create scope document for no-action epi…]
- "scripts_phase_4_5_ticket_review_mcp_init_firestore": "init_firestore()" | kind=code-symbol | source=scripts/phase_4_5_ticket_review_mcp.py:L21 | neighbors=[phase_4_5_ticket_review_mcp.py, execute_phase_4_5(), Initialize Firebase using local service…]
- "scripts_phase_4_5_ticket_review_mcp_query_jane_street_kb": "query_jane_street_kb()" | kind=code-symbol | source=scripts/phase_4_5_ticket_review_mcp.py:L37 | neighbors=[phase_4_5_ticket_review_mcp.py, execute_phase_4_5(), Query Jane Street KB for extraction pat…]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-030.json

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
