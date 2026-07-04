# Node Description Batch 41 of 61

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

- "scripts_wave2_parallel_executor_phase_4_prompt": "phase_4_prompt()" | kind=code-symbol | source=scripts/wave2_parallel_executor.py:L232 | neighbors=[wave2_parallel_executor.py, Generate Phase 4 prompt for an epic]
- "scripts_wave2_parallel_executor_phase_5_5_prompt": "phase_5_5_prompt()" | kind=code-symbol | source=scripts/wave2_parallel_executor.py:L264 | neighbors=[wave2_parallel_executor.py, Generate Phase 5.5 prompt for an epic]
- "scripts_wave2_parallel_executor_phase_5_prompt": "phase_5_prompt()" | kind=code-symbol | source=scripts/wave2_parallel_executor.py:L249 | neighbors=[wave2_parallel_executor.py, Generate Phase 5 prompt for an epic]
- "scripts_wave2_parallel_executor_phase_6_prompt": "phase_6_prompt()" | kind=code-symbol | source=scripts/wave2_parallel_executor.py:L279 | neighbors=[wave2_parallel_executor.py, Generate Phase 6 prompt for an epic]
- "scripts_wave7_batch_audit_main": "main()" | kind=code-symbol | source=scripts/wave7_batch_audit.py:L571 | neighbors=[wave7_batch_audit.py, run_batch_audit()]
- "scripts_worker_agent_mcp_list_tools": "list_tools()" | kind=code-symbol | source=scripts/worker_agent_mcp.py:L67 | neighbors=[worker_agent_mcp.py, List available MCP tools for worker age…]
- "test_regex": "test_regex.py" | kind=code-symbol | source=test_regex.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "update_api_keys_rotation": "update_api_keys_rotation.py" | kind=code-symbol | source=update_api_keys_rotation.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …]
- "validate_wave6_epic_structure_validate_wave6_structure": "validate_wave6_structure()" | kind=code-symbol | source=validate_wave6_epic_structure.py:L11 | neighbors=[validate_wave6_epic_structure.py, Validate Wave 6 epic structure and meth…]
- "wave2_check_api_balances_main": "main()" | kind=code-symbol | source=scripts/wave2/check_api_balances.py:L56 | neighbors=[check_api_balances.py, check_balance()]
- "wave2_check_phase2_status_main": "main()" | kind=code-symbol | source=scripts/wave2/check_phase2_status.py:L51 | neighbors=[check_phase2_status.py, get_epic_status()]
- "wave2_check_phase4_local_check_phase4_status": "check_phase4_status()" | kind=code-symbol | source=scripts/wave2/check_phase4_local.py:L13 | neighbors=[check_phase4_local.py, Check Phase 4 status for all Wave 2 epi…]
- "wave2_launch_phase0_v4_shell_commands_create_script": "create_script()" | kind=code-symbol | source=scripts/wave2/launch_phase0_v4_shell_commands.py:L37 | neighbors=[launch_phase0_v4_shell_commands.py, main()]
- "wave2_launch_phase0_v4_shell_commands_load_api_key": "load_api_key()" | kind=code-symbol | source=scripts/wave2/launch_phase0_v4_shell_commands.py:L31 | neighbors=[launch_phase0_v4_shell_commands.py, main()]
- "wave2_launch_wave_now_gcloud": "gcloud()" | kind=code-symbol | source=scripts/wave2/launch_wave_now.py:L29 | neighbors=[launch_wave_now.py, main()]
- "wave2_launch_wave_now_gcloud_capture": "gcloud_capture()" | kind=code-symbol | source=scripts/wave2/launch_wave_now.py:L37 | neighbors=[launch_wave_now.py, clear_stale_ssh_key()]
- "wave2_launch_wave_v2_gcloud": "gcloud()" | kind=code-symbol | source=scripts/wave2/launch_wave_v2.py:L34 | neighbors=[launch_wave_v2.py, main()]
- "wave2_launch_wave_v2_gcloud_capture": "gcloud_capture()" | kind=code-symbol | source=scripts/wave2/launch_wave_v2.py:L42 | neighbors=[launch_wave_v2.py, clear_stale_ssh_key()]
- "wave2_launch_wave_v3_multi_api_gcloud": "gcloud()" | kind=code-symbol | source=scripts/wave2/launch_wave_v3_multi_api.py:L35 | neighbors=[launch_wave_v3_multi_api.py, main()]
- "wave2_launch_wave_v3_multi_api_gcloud_capture": "gcloud_capture()" | kind=code-symbol | source=scripts/wave2/launch_wave_v3_multi_api.py:L42 | neighbors=[launch_wave_v3_multi_api.py, clear_stale_ssh_key()]
- "wave2_launch_wave_v4_safe_budget_gcloud": "gcloud()" | kind=code-symbol | source=scripts/wave2/launch_wave_v4_safe_budget.py:L37 | neighbors=[launch_wave_v4_safe_budget.py, main()]
- "wave2_launch_wave_v4_safe_budget_gcloud_capture": "gcloud_capture()" | kind=code-symbol | source=scripts/wave2/launch_wave_v4_safe_budget.py:L44 | neighbors=[launch_wave_v4_safe_budget.py, clear_stale_ssh_key()]
- "wave2_reset_phase4_manifests_main": "main()" | kind=code-symbol | source=scripts/wave2/reset_phase4_manifests.py:L39 | neighbors=[reset_phase4_manifests.py, reset_phase4()]
- "wave3_generate_wave3_phase2_scripts_generate_phase2_scripts": "generate_phase2_scripts()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase2_scripts.py:L84 | neighbors=[generate_wave3_phase2_scripts.py, Generate Phase 2 scripts for all Wave 3…]
- "wave4_execute_phase0_with_jane_street_execute_phase_0": "execute_phase_0()" | kind=code-symbol | source=scripts/wave4/execute_phase0_with_jane_street.py:L12 | neighbors=[execute_phase0_with_jane_street.py, Execute Phase 0 with Jane Street integr…]
- "wave4_generate_phase1_scripts_load_api_keys": "load_api_keys()" | kind=code-symbol | source=scripts/wave4/generate_phase1_scripts.py:L21 | neighbors=[generate_phase1_scripts.py, Load all 15 API keys from JSON files.]
- "wave4_generate_phase4_recovery_main": "main()" | kind=code-symbol | source=scripts/wave4/generate_phase4_recovery.py:L130 | neighbors=[generate_phase4_recovery.py, generate_recovery_scripts()]
- "wave6_add_missing_dependencies": "add_missing_dependencies.py" | kind=code-symbol | source=scripts/wave6/add_missing_dependencies.py:L1 | neighbors=[3a92e1e feat: Wave 6 Phase 0 preparatio…, ceae42c feat: Wave 6 Phase 0 preparatio…]
- "wave6_add_missing_phase_modes_add_missing_modes": "add_missing_modes()" | kind=code-symbol | source=scripts/wave6/add_missing_phase_modes.py:L31 | neighbors=[add_missing_phase_modes.py, Add missing mode field to all phases.]
- "wave6_add_missing_top_level_fields_add_missing_fields": "add_missing_fields()" | kind=code-symbol | source=scripts/wave6/add_missing_top_level_fields.py:L17 | neighbors=[add_missing_top_level_fields.py, Add missing top-level fields to all man…]
- "wave6_add_phase1_to_024": "add_phase1_to_024.py" | kind=code-symbol | source=scripts/wave6/add_phase1_to_024.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "wave6_check_epic_004": "check_epic_004.py" | kind=code-symbol | source=scripts/wave6/check_epic_004.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "wave6_check_manifest_events": "check_manifest_events.py" | kind=code-symbol | source=scripts/wave6/check_manifest_events.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "wave6_check_phase1_completion": "check_phase1_completion.py" | kind=code-symbol | source=scripts/wave6/check_phase1_completion.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "wave6_check_wave6_only_status_check_phase1_status": "check_phase1_status()" | kind=code-symbol | source=scripts/wave6/check_wave6_only_status.py:L8 | neighbors=[check_wave6_only_status.py, Check Phase 1 completion status for Wav…]
- "wave6_clear_phase1_state_epic_003": "clear_phase1_state_epic_003.py" | kind=code-symbol | source=scripts/wave6/clear_phase1_state_epic_003.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "wave6_find_missing_phase0": "find_missing_phase0.py" | kind=code-symbol | source=scripts/wave6/find_missing_phase0.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "wave6_find_pending_epic": "find_pending_epic.py" | kind=code-symbol | source=scripts/wave6/find_pending_epic.py:L1 | neighbors=[3a92e1e feat: Wave 6 Phase 0 preparatio…, ceae42c feat: Wave 6 Phase 0 preparatio…]
- "wave6_fix_4_manifests": "fix_4_manifests.py" | kind=code-symbol | source=scripts/wave6/fix_4_manifests.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "wave6_fix_all_manifest_modes": "fix_all_manifest_modes.py" | kind=code-symbol | source=scripts/wave6/fix_all_manifest_modes.py:L1 | neighbors=[0029dd5 V12.53: ALL 10 phases now use c…, bb0a399 V12.53: ALL 10 phases now use c…]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-040.json

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
