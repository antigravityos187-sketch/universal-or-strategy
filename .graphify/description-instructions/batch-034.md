# Node Description Batch 35 of 61

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

- "wave2_monitor_phase4_update_manifest_status": "update_manifest_status()" | kind=code-symbol | source=scripts/wave2/monitor_phase4.py:L56 | neighbors=[monitor_phase4.py, main(), Update manifest with completion status]
- "wave2_phase4_with_checkpoints_load_api_key": "load_api_key()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints.py:L114 | neighbors=[phase4_with_checkpoints.py, build_phase4_script(), Load API key from JSON file]
- "wave2_phase4_with_checkpoints_load_manifest": "load_manifest()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints.py:L49 | neighbors=[phase4_with_checkpoints.py, check_phase_status(), Load manifest.json for an epic, create …]
- "wave2_phase4_with_checkpoints_update_manifest": "update_manifest()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints.py:L81 | neighbors=[phase4_with_checkpoints.py, main(), Update manifest with phase status]
- "wave2_phase4_with_checkpoints_v2_load_api_key": "load_api_key()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v2.py:L147 | neighbors=[phase4_with_checkpoints_v2.py, build_phase4_script(), Load API key from JSON file]
- "wave2_phase4_with_checkpoints_v3_fixed_load_api_key": "load_api_key()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L154 | neighbors=[phase4_with_checkpoints_v3_fixed.py, build_phase4_script(), Load API key from JSON file]
- "wave2_phase4_with_checkpoints_v3_fixed_validate_api_allocation": "validate_api_allocation()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L57 | neighbors=[phase4_with_checkpoints_v3_fixed.py, main(), Validate API allocation for duplicates …]
- "wave2_remove_gates_final_main": "main()" | kind=code-symbol | source=scripts/wave2/remove_gates_final.py:L38 | neighbors=[remove_gates_final.py, remove_gate_section(), Process all commands.]
- "wave2_remove_gates_final_remove_gate_section": "remove_gate_section()" | kind=code-symbol | source=scripts/wave2/remove_gates_final.py:L18 | neighbors=[remove_gates_final.py, main(), Remove the gate section from command co…]
- "wave2_reset_phase4_manifests_reset_phase4": "reset_phase4()" | kind=code-symbol | source=scripts/wave2/reset_phase4_manifests.py:L14 | neighbors=[reset_phase4_manifests.py, main(), Reset Phase 4 status to pending]
- "wave2_test_single_epic_107_generate_test_script": "generate_test_script()" | kind=code-symbol | source=scripts/wave2/test_single_epic_107.py:L47 | neighbors=[test_single_epic_107.py, main(), Generate test script for EPIC-CCN-107]
- "wave2_test_single_epic_107_get_epic_data": "get_epic_data()" | kind=code-symbol | source=scripts/wave2/test_single_epic_107.py:L18 | neighbors=[test_single_epic_107.py, main(), Extract epic data from roadmap]
- "wave2_test_single_epic_107_load_epic_roadmap": "load_epic_roadmap()" | kind=code-symbol | source=scripts/wave2/test_single_epic_107.py:L12 | neighbors=[test_single_epic_107.py, main(), Load epic roadmap data]
- "wave2_test_single_epic_107_load_template": "load_template()" | kind=code-symbol | source=scripts/wave2/test_single_epic_107.py:L31 | neighbors=[test_single_epic_107.py, main(), Load message template]
- "wave2_test_single_epic_107_populate_template": "populate_template()" | kind=code-symbol | source=scripts/wave2/test_single_epic_107.py:L37 | neighbors=[test_single_epic_107.py, main(), Fill in template placeholders with epic…]
- "wave2_track_api_balances_check_thresholds": "check_thresholds()" | kind=code-symbol | source=scripts/wave2/track_api_balances.py:L141 | neighbors=[track_api_balances.py, main(), Check balance thresholds and return ale…]
- "wave2_track_api_balances_extract_costs_from_vm_logs": "extract_costs_from_vm_logs()" | kind=code-symbol | source=scripts/wave2/track_api_balances.py:L52 | neighbors=[track_api_balances.py, calculate_balances(), Extract Cost and Balance from VM logs f…]
- "wave2_track_api_balances_format_status_table": "format_status_table()" | kind=code-symbol | source=scripts/wave2/track_api_balances.py:L181 | neighbors=[track_api_balances.py, main(), Format current status as markdown table]
- "wave2_track_api_balances_get_current_balances": "get_current_balances()" | kind=code-symbol | source=scripts/wave2/track_api_balances.py:L126 | neighbors=[track_api_balances.py, main(), Calculate current balance for each API]
- "wave2_track_api_balances_load_api_keys": "load_api_keys()" | kind=code-symbol | source=scripts/wave2/track_api_balances.py:L42 | neighbors=[track_api_balances.py, main(), Load all API keys from docs/API/*.json]
- "wave2_track_api_balances_recommend_reassignments": "recommend_reassignments()" | kind=code-symbol | source=scripts/wave2/track_api_balances.py:L157 | neighbors=[track_api_balances.py, main(), Recommend epic reassignments for low-ba…]
- "wave2_update_obsidian_kanban_generate_kanban_markdown": "generate_kanban_markdown()" | kind=code-symbol | source=scripts/wave2/update_obsidian_kanban.py:L105 | neighbors=[update_obsidian_kanban.py, main(), Generate Obsidian Kanban markdown from …]
- "wave2_update_obsidian_kanban_update_kanban_file": "update_kanban_file()" | kind=code-symbol | source=scripts/wave2/update_obsidian_kanban.py:L176 | neighbors=[update_obsidian_kanban.py, main(), Update the Kanban file in Obsidian vaul…]
- "wave2_update_wave2_kanban_find_obsidian_vaults": "find_obsidian_vaults()" | kind=code-symbol | source=scripts/wave2/update_wave2_kanban.py:L32 | neighbors=[update_wave2_kanban.py, main(), Find Obsidian vaults on the system.]
- "wave2_update_wave2_kanban_run_gcloud_command": "run_gcloud_command()" | kind=code-symbol | source=scripts/wave2/update_wave2_kanban.py:L54 | neighbors=[update_wave2_kanban.py, get_ticket_status(), Execute gcloud command and return outpu…]
- "wave2_update_wave2_kanban_update_kanban_board": "update_kanban_board()" | kind=code-symbol | source=scripts/wave2/update_wave2_kanban.py:L107 | neighbors=[update_wave2_kanban.py, main(), Update the existing WAVE_2_KANBAN board.]
- "wave2_wait_for_phase4_check_completion": "check_completion()" | kind=code-symbol | source=scripts/wave2/wait_for_phase4.py:L14 | neighbors=[wait_for_phase4.py, main(), Check if all epics have completed Phase…]
- "wave2_wait_for_phase4_main": "main()" | kind=code-symbol | source=scripts/wave2/wait_for_phase4.py:L36 | neighbors=[wait_for_phase4.py, check_completion(), Monitor Phase 4 completion.]
- "wave3_generate_wave3_phase0_scripts_generate_launcher_script": "generate_launcher_script()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase0_scripts.py:L265 | neighbors=[generate_wave3_phase0_scripts.py, main(), Generate launcher script for all Phase …]
- "wave3_generate_wave3_phase0_scripts_load_api_key": "load_api_key()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase0_scripts.py:L97 | neighbors=[generate_wave3_phase0_scripts.py, generate_phase0_script(), Load API key from JSON file.]
- "wave3_generate_wave3_phase1_scripts_generate_launcher_script": "generate_launcher_script()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase1_scripts.py:L91 | neighbors=[generate_wave3_phase1_scripts.py, main(), Generate launcher script for all Phase …]
- "wave3_generate_wave3_phase1_scripts_generate_phase1_script": "generate_phase1_script()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase1_scripts.py:L38 | neighbors=[generate_wave3_phase1_scripts.py, main(), Generate Phase 1 script by copying Wave…]
- "wave3_generate_wave3_phase3_scripts_corrected_generate_launcher": "generate_launcher()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase3_scripts_CORRECTED.py:L97 | neighbors=[generate_wave3_phase3_scripts_CORRECTED…, main(), Generate launcher script for all Phase …]
- "wave3_generate_wave3_phase3_scripts_corrected_generate_phase3_script": "generate_phase3_script()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase3_scripts_CORRECTED.py:L34 | neighbors=[generate_wave3_phase3_scripts_CORRECTED…, main(), Generate Phase 3 script for given epic …]
- "wave3_generate_wave3_phase3_scripts_corrected_main": "main()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase3_scripts_CORRECTED.py:L138 | neighbors=[generate_wave3_phase3_scripts_CORRECTED…, generate_launcher(), generate_phase3_script()]
- "wave3_generate_wave3_phase3_scripts_generate_launcher_script": "generate_launcher_script()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase3_scripts.py:L88 | neighbors=[generate_wave3_phase3_scripts.py, main(), Generate launcher script for all Phase …]
- "wave3_generate_wave3_phase3_scripts_generate_phase3_script": "generate_phase3_script()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase3_scripts.py:L21 | neighbors=[generate_wave3_phase3_scripts.py, main(), Generate Phase 3 script by copying Phas…]
- "wave3_generate_wave3_phase4_scripts_generate_launcher": "generate_launcher()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase4_scripts.py:L109 | neighbors=[generate_wave3_phase4_scripts.py, main(), Generate launcher script for all Phase …]
- "wave3_generate_wave3_phase4_scripts_generate_phase4_script": "generate_phase4_script()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase4_scripts.py:L38 | neighbors=[generate_wave3_phase4_scripts.py, main(), Generate Phase 4 script for given epic …]
- "wave3_generate_wave3_phase4_scripts_main": "main()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase4_scripts.py:L150 | neighbors=[generate_wave3_phase4_scripts.py, generate_launcher(), generate_phase4_script()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-034.json

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
