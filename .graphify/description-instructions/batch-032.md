# Node Description Batch 33 of 61

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

- "scripts_v12_split_extract_method_block": "extract_method_block()" | kind=code-symbol | source=scripts/v12_split.py:L13 | neighbors=[v12_split.py, Extract a method block from source line…, split_method()]
- "scripts_validate_epic_save_roadmap": "save_roadmap()" | kind=code-symbol | source=scripts/validate_epic.py:L30 | neighbors=[validate_epic.py, claim_epic(), release_epic()]
- "scripts_validate_phase_compliance_main": "main()" | kind=code-symbol | source=scripts/validate_phase_compliance.py:L338 | neighbors=[validate_phase_compliance.py, validate_all_epics(), validate_epic_phase()]
- "scripts_validate_phase_compliance_phasevalidator_check_custom_mode_mentioned": "._check_custom_mode_mentioned()" | kind=code-symbol | source=scripts/validate_phase_compliance.py:L224 | neighbors=[PhaseValidator, .validate(), Heuristic check: Look for custom mode n…]
- "scripts_validate_phase_compliance_phasevalidator_check_lamport_event": "._check_lamport_event()" | kind=code-symbol | source=scripts/validate_phase_compliance.py:L166 | neighbors=[PhaseValidator, .validate(), Check that Lamport event was logged for…]
- "scripts_validate_phase_compliance_phasevalidator_check_manifest_updated": "._check_manifest_updated()" | kind=code-symbol | source=scripts/validate_phase_compliance.py:L142 | neighbors=[PhaseValidator, .validate(), Check that manifest.json was updated fo…]
- "scripts_validate_phase_compliance_phasevalidator_check_mcp_usage": "._check_mcp_usage()" | kind=code-symbol | source=scripts/validate_phase_compliance.py:L191 | neighbors=[PhaseValidator, .validate(), Heuristic check: Look for MCP tool name…]
- "scripts_validate_phase_compliance_phasevalidator_check_output_files": "._check_output_files()" | kind=code-symbol | source=scripts/validate_phase_compliance.py:L124 | neighbors=[PhaseValidator, .validate(), Check that required output files exist.]
- "scripts_verify_index_freshness_get_git_head_timestamp": "get_git_head_timestamp()" | kind=code-symbol | source=scripts/verify_index_freshness.py:L25 | neighbors=[verify_index_freshness.py, Get timestamp of current git HEAD commi…, verify_index_freshness()]
- "scripts_verify_index_freshness_get_graphify_timestamp": "get_graphify_timestamp()" | kind=code-symbol | source=scripts/verify_index_freshness.py:L36 | neighbors=[verify_index_freshness.py, Get timestamp of graphify-out/graph.jso…, verify_index_freshness()]
- "scripts_verify_index_freshness_get_modified_files_since": "get_modified_files_since()" | kind=code-symbol | source=scripts/verify_index_freshness.py:L46 | neighbors=[verify_index_freshness.py, Get list of files modified since given …, verify_index_freshness()]
- "scripts_verify_wave7_determinism_get_wave7_epics": "get_wave7_epics()" | kind=code-symbol | source=scripts/verify_wave7_determinism.py:L21 | neighbors=[verify_wave7_determinism.py, Get all Wave 7 epic IDs from event log.…, verify_all_epics()]
- "scripts_verify_wave7_determinism_print_results": "print_results()" | kind=code-symbol | source=scripts/verify_wave7_determinism.py:L100 | neighbors=[verify_wave7_determinism.py, main(), Print verification results.          …]
- "scripts_verify_wave7_templates_check_epic_naming": "check_epic_naming()" | kind=code-symbol | source=scripts/verify_wave7_templates.py:L84 | neighbors=[verify_wave7_templates.py, Check EPIC naming convention., verify_template()]
- "scripts_verify_wave7_templates_check_file_exists": "check_file_exists()" | kind=code-symbol | source=scripts/verify_wave7_templates.py:L55 | neighbors=[verify_wave7_templates.py, main(), Check if all required template files ex…]
- "scripts_verify_wave7_templates_check_temp_file_pattern": "check_temp_file_pattern()" | kind=code-symbol | source=scripts/verify_wave7_templates.py:L63 | neighbors=[verify_wave7_templates.py, Check if template uses temp file + comm…, verify_template()]
- "scripts_verify_wave7_templates_fix_epic_naming": "fix_epic_naming()" | kind=code-symbol | source=scripts/verify_wave7_templates.py:L96 | neighbors=[verify_wave7_templates.py, Replace EPIC-CCN-XXX with EPIC-W7-XXX., verify_template()]
- "scripts_verify_wave7_templates_print_summary": "print_summary()" | kind=code-symbol | source=scripts/verify_wave7_templates.py:L148 | neighbors=[verify_wave7_templates.py, main(), Print verification summary.]
- "scripts_wave_coordinator_wavecoordinator_generate_instructions": "._generate_instructions()" | kind=code-symbol | source=scripts/wave_coordinator.py:L102 | neighbors=[Generate human-readable instructions fo…, WaveCoordinator, .execute_wave()]
- "scripts_wave_coordinator_wavecoordinator_get_phase_config": ".get_phase_config()" | kind=code-symbol | source=scripts/wave_coordinator.py:L54 | neighbors=[Get phase configuration by ID., WaveCoordinator, .execute_wave()]
- "scripts_wave_coordinator_wavecoordinator_save_checkpoint": "._save_checkpoint()" | kind=code-symbol | source=scripts/wave_coordinator.py:L164 | neighbors=[Save wave execution checkpoint., WaveCoordinator, .run_wave_batch()]
- "scripts_wave2_bob_shell_executor_execute_phase_parallel": "execute_phase_parallel()" | kind=code-symbol | source=scripts/wave2_bob_shell_executor.py:L99 | neighbors=[wave2_bob_shell_executor.py, main(), Execute a phase for all epics in parall…]
- "scripts_wave2_bob_shell_executor_main": "main()" | kind=code-symbol | source=scripts/wave2_bob_shell_executor.py:L129 | neighbors=[wave2_bob_shell_executor.py, execute_phase_parallel(), Execute all phases for Wave 2 epics]
- "scripts_wave2_direct_executor_create_bob_prompt_for_phase_1": "create_bob_prompt_for_phase_1()" | kind=code-symbol | source=scripts/wave2_direct_executor.py:L95 | neighbors=[wave2_direct_executor.py, execute_phase_1_all(), Create Bob CLI prompt for Phase 1]
- "scripts_wave2_direct_executor_create_phase_0_artifacts": "create_phase_0_artifacts()" | kind=code-symbol | source=scripts/wave2_direct_executor.py:L26 | neighbors=[wave2_direct_executor.py, execute_phase_0_all(), Create Phase 0 artifacts directly (no M…]
- "scripts_wave2_direct_executor_main": "main()" | kind=code-symbol | source=scripts/wave2_direct_executor.py:L151 | neighbors=[wave2_direct_executor.py, execute_phase_0_all(), execute_phase_1_all()]
- "scripts_wave2_parallel_executor_execute_phase_parallel": "execute_phase_parallel()" | kind=code-symbol | source=scripts/wave2_parallel_executor.py:L88 | neighbors=[wave2_parallel_executor.py, main(), Execute a phase for epics in parallel u…]
- "scripts_wave2_simple_orchestrator_execute_phase_0_batch": "execute_phase_0_batch()" | kind=code-symbol | source=scripts/wave2_simple_orchestrator.py:L29 | neighbors=[wave2_simple_orchestrator.py, main(), Execute Phase 0 for all 9 epics using B…]
- "scripts_wave2_simple_orchestrator_execute_phase_1_batch": "execute_phase_1_batch()" | kind=code-symbol | source=scripts/wave2_simple_orchestrator.py:L57 | neighbors=[wave2_simple_orchestrator.py, main(), Execute Phase 1 for all 9 epics.]
- "scripts_wave7_batch_audit_load_cyc_cache": "_load_cyc_cache()" | kind=code-symbol | source=scripts/wave7_batch_audit.py:L222 | neighbors=[wave7_batch_audit.py, audit_epic(), Run complexity_audit.py once and parse …]
- "scripts_wave7_batch_audit_resolve_target_method": "_resolve_target_method()" | kind=code-symbol | source=scripts/wave7_batch_audit.py:L255 | neighbors=[wave7_batch_audit.py, audit_epic(), Return the target method name for an ep…]
- "scripts_worker_agent_mcp_fastmcp_execute_epic": "execute_epic()" | kind=code-symbol | source=scripts/worker_agent_mcp_fastmcp.py:L147 | neighbors=[worker_agent_mcp_fastmcp.py, run_command(), Execute all phases of a claimed epic (i…]
- "scripts_worker_agent_mcp_fastmcp_get_next_pending_epic": "get_next_pending_epic()" | kind=code-symbol | source=scripts/worker_agent_mcp_fastmcp.py:L287 | neighbors=[worker_agent_mcp_fastmcp.py, load_roadmap(), Get next pending epic from roadmap (not…]
- "scripts_worker_agent_mcp_fastmcp_get_worker_status": "get_worker_status()" | kind=code-symbol | source=scripts/worker_agent_mcp_fastmcp.py:L257 | neighbors=[worker_agent_mcp_fastmcp.py, load_roadmap(), Get current worker status (assigned epi…]
- "scripts_worker_agent_mcp_fastmcp_save_roadmap": "save_roadmap()" | kind=code-symbol | source=scripts/worker_agent_mcp_fastmcp.py:L33 | neighbors=[worker_agent_mcp_fastmcp.py, claim_epic(), release_epic()]
- "scripts_worker_agent_mcp_get_worker_status_tool": "get_worker_status_tool()" | kind=code-symbol | source=scripts/worker_agent_mcp.py:L361 | neighbors=[worker_agent_mcp.py, call_tool(), load_roadmap()]
- "scripts_worker_agent_mcp_save_roadmap": "save_roadmap()" | kind=code-symbol | source=scripts/worker_agent_mcp.py:L36 | neighbors=[worker_agent_mcp.py, claim_epic_tool(), release_epic_tool()]
- "update_wave7_api_keys_load_api_keys": "load_api_keys()" | kind=code-symbol | source=update_wave7_api_keys.py:L12 | neighbors=[update_wave7_api_keys.py, main(), Load all API keys from docs/API/*.json]
- "update_wave7_api_keys_main": "main()" | kind=code-symbol | source=update_wave7_api_keys.py:L52 | neighbors=[update_wave7_api_keys.py, load_api_keys(), update_script()]
- "update_wave7_api_keys_update_script": "update_script()" | kind=code-symbol | source=update_wave7_api_keys.py:L32 | neighbors=[update_wave7_api_keys.py, main(), Replace API key in a Phase 0 script]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-032.json

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
