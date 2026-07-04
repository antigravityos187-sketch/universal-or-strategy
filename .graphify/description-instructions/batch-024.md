# Node Description Batch 25 of 61

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

- "wave2_launch_wave_v3_multi_api_clear_stale_ssh_key": "clear_stale_ssh_key()" | kind=code-symbol | source=scripts/wave2/launch_wave_v3_multi_api.py:L141 | neighbors=[launch_wave_v3_multi_api.py, gcloud_capture(), main(), Remove stale Plink key cache entry for …]
- "wave2_launch_wave_v4_safe_budget_clear_stale_ssh_key": "clear_stale_ssh_key()" | kind=code-symbol | source=scripts/wave2/launch_wave_v4_safe_budget.py:L164 | neighbors=[launch_wave_v4_safe_budget.py, gcloud_capture(), main(), Remove stale Plink key cache entry for …]
- "wave2_monitor_phase4_main": "main()" | kind=code-symbol | source=scripts/wave2/monitor_phase4.py:L73 | neighbors=[monitor_phase4.py, check_log_completion(), check_screen_sessions(), update_manifest_status()]
- "wave2_phase4_with_checkpoints_build_phase4_script": "build_phase4_script()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints.py:L121 | neighbors=[phase4_with_checkpoints.py, load_api_key(), main(), Build bash script for Phase 4 execution…]
- "wave2_phase4_with_checkpoints_check_phase_status": "check_phase_status()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints.py:L95 | neighbors=[phase4_with_checkpoints.py, load_manifest(), main(), Check if phase is pending, in_progress,…]
- "wave2_phase4_with_checkpoints_main": "main()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints.py:L169 | neighbors=[phase4_with_checkpoints.py, build_phase4_script(), check_phase_status(), update_manifest()]
- "wave2_phase4_with_checkpoints_v2_build_phase4_script": "build_phase4_script()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v2.py:L154 | neighbors=[phase4_with_checkpoints_v2.py, load_api_key(), launch_agents_on_vm(), Build bash script for Phase 4 execution]
- "wave2_phase4_with_checkpoints_v2_launch_agents_on_vm": "launch_agents_on_vm()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v2.py:L205 | neighbors=[phase4_with_checkpoints_v2.py, build_phase4_script(), main(), Launch agents on VM, return True if suc…]
- "wave2_phase4_with_checkpoints_v2_load_manifest": "load_manifest()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v2.py:L55 | neighbors=[phase4_with_checkpoints_v2.py, check_phase_status_with_healing(), Load manifest.json for an epic, create …, update_manifest()]
- "wave2_phase4_with_checkpoints_v2_main": "main()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v2.py:L262 | neighbors=[phase4_with_checkpoints_v2.py, check_phase_status_with_healing(), launch_agents_on_vm(), update_manifest()]
- "wave2_phase4_with_checkpoints_v2_save_manifest": "save_manifest()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v2.py:L87 | neighbors=[phase4_with_checkpoints_v2.py, check_phase_status_with_healing(), Save manifest to disk, update_manifest()]
- "wave2_phase4_with_checkpoints_v3_fixed_build_phase4_script": "build_phase4_script()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L161 | neighbors=[phase4_with_checkpoints_v3_fixed.py, load_api_key(), launch_agents_on_vm(), Build bash script for Phase 4 execution]
- "wave2_phase4_with_checkpoints_v3_fixed_launch_agents_on_vm": "launch_agents_on_vm()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L213 | neighbors=[phase4_with_checkpoints_v3_fixed.py, build_phase4_script(), main(), Launch agents on VM, return True if suc…]
- "wave2_phase4_with_checkpoints_v3_fixed_load_manifest": "load_manifest()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L66 | neighbors=[phase4_with_checkpoints_v3_fixed.py, check_phase_status_with_healing(), Load manifest.json for an epic, create …, update_manifest()]
- "wave2_phase4_with_checkpoints_v3_fixed_save_manifest": "save_manifest()" | kind=code-symbol | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L97 | neighbors=[phase4_with_checkpoints_v3_fixed.py, check_phase_status_with_healing(), Save manifest to disk, update_manifest()]
- "wave2_track_api_balances_calculate_balances": "calculate_balances()" | kind=code-symbol | source=scripts/wave2/track_api_balances.py:L107 | neighbors=[track_api_balances.py, extract_costs_from_vm_logs(), main(), Calculate current balances for all APIs…]
- "wave2_update_obsidian_kanban_get_epic_status": "get_epic_status()" | kind=code-symbol | source=scripts/wave2/update_obsidian_kanban.py:L49 | neighbors=[update_obsidian_kanban.py, get_all_status(), run_gcloud_command(), Get status of an epic from VM.]
- "wave2_update_obsidian_kanban_get_ticket_status": "get_ticket_status()" | kind=code-symbol | source=scripts/wave2/update_obsidian_kanban.py:L62 | neighbors=[update_obsidian_kanban.py, get_all_status(), run_gcloud_command(), Get status of a specific ticket.]
- "wave2_update_obsidian_kanban_main": "main()" | kind=code-symbol | source=scripts/wave2/update_obsidian_kanban.py:L187 | neighbors=[update_obsidian_kanban.py, generate_kanban_markdown(), get_all_status(), update_kanban_file()]
- "wave2_update_obsidian_kanban_run_gcloud_command": "run_gcloud_command()" | kind=code-symbol | source=scripts/wave2/update_obsidian_kanban.py:L34 | neighbors=[update_obsidian_kanban.py, get_epic_status(), get_ticket_status(), Execute gcloud command and return outpu…]
- "wave2_update_wave2_kanban_get_all_phase5_status": "get_all_phase5_status()" | kind=code-symbol | source=scripts/wave2/update_wave2_kanban.py:L88 | neighbors=[update_wave2_kanban.py, get_ticket_status(), main(), Get status of all Phase 5 tickets.]
- "wave2_update_wave2_kanban_get_ticket_status": "get_ticket_status()" | kind=code-symbol | source=scripts/wave2/update_wave2_kanban.py:L68 | neighbors=[update_wave2_kanban.py, get_all_phase5_status(), run_gcloud_command(), Get status of a specific ticket from VM.]
- "wave2_update_wave2_kanban_main": "main()" | kind=code-symbol | source=scripts/wave2/update_wave2_kanban.py:L174 | neighbors=[update_wave2_kanban.py, find_obsidian_vaults(), get_all_phase5_status(), update_kanban_board()]
- "wave3_generate_wave3_phase0_scripts_generate_phase0_script": "generate_phase0_script()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase0_scripts.py:L104 | neighbors=[generate_wave3_phase0_scripts.py, load_api_key(), main(), Generate Phase 0 script by copying Wave…]
- "wave3_generate_wave3_phase0_scripts_main": "main()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase0_scripts.py:L299 | neighbors=[generate_wave3_phase0_scripts.py, generate_launcher_script(), generate_phase0_script(), Generate all Wave 3 Phase 0 scripts.]
- "wave3_generate_wave3_phase1_scripts_main": "main()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase1_scripts.py:L121 | neighbors=[generate_wave3_phase1_scripts.py, generate_launcher_script(), generate_phase1_script(), Generate all Wave 3 Phase 1 scripts.]
- "wave3_generate_wave3_phase3_scripts_main": "main()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase3_scripts.py:L123 | neighbors=[generate_wave3_phase3_scripts.py, generate_launcher_script(), generate_phase3_script(), Generate all Phase 3 scripts]
- "wave4_audit_and_remove_pr_references_audit_directory": "audit_directory()" | kind=code-symbol | source=scripts/wave4/audit_and_remove_pr_references.py:L77 | neighbors=[audit_and_remove_pr_references.py, find_pr_references(), main(), Audit all markdown files in directory.]
- "wave4_audit_and_remove_pr_references_find_pr_references": "find_pr_references()" | kind=code-symbol | source=scripts/wave4/audit_and_remove_pr_references.py:L49 | neighbors=[audit_and_remove_pr_references.py, audit_directory(), is_acceptable_context(), Find all PR references in a file.]
- "wave4_check_missing_p5_scripts": "check_missing_p5_scripts.py" | kind=code-symbol | source=scripts/wave4/check_missing_p5_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_count_phase5_success": "count_phase5_success.py" | kind=code-symbol | source=scripts/wave4/count_phase5_success.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_execute_80_80_recovery_monitor_recovery": "monitor_recovery()" | kind=code-symbol | source=scripts/wave4/execute_80_80_recovery.py:L174 | neighbors=[execute_80_80_recovery.py, main(), run_gcloud(), Monitor recovery progress.]
- "wave4_execute_80_80_recovery_step1_fix_phase6_path_issue": "step1_fix_phase6_path_issue()" | kind=code-symbol | source=scripts/wave4/execute_80_80_recovery.py:L39 | neighbors=[execute_80_80_recovery.py, main(), Fix Phase 6 PATH issue for 3 epics., run_gcloud()]
- "wave4_execute_80_80_recovery_step2_upload_missing_phase5_scripts": "step2_upload_missing_phase5_scripts()" | kind=code-symbol | source=scripts/wave4/execute_80_80_recovery.py:L119 | neighbors=[execute_80_80_recovery.py, main(), Upload missing Phase 5 scripts and laun…, run_gcloud()]
- "wave4_fix_phase6_prerequisite_v2": "fix_phase6_prerequisite_v2.py" | kind=code-symbol | source=scripts/wave4/fix_phase6_prerequisite_v2.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_fix_phase6_scripts": "fix_phase6_scripts.py" | kind=code-symbol | source=scripts/wave4/fix_phase6_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_generate_phase0_scripts_generate_scripts": "generate_scripts()" | kind=code-symbol | source=scripts/wave4/generate_phase0_scripts.py:L211 | neighbors=[generate_phase0_scripts.py, load_api_keys(), load_pending_epics(), Generate Phase 0 scripts for all pendin…]
- "wave4_generate_phase0_wave2_pattern": "generate_phase0_wave2_pattern.py" | kind=code-symbol | source=scripts/wave4/generate_phase0_wave2_pattern.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_generate_phase0_with_jane_street": "generate_phase0_with_jane_street.py" | kind=code-symbol | source=scripts/wave4/generate_phase0_with_jane_street.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_generate_phase1_from_phase0": "generate_phase1_from_phase0.py" | kind=code-symbol | source=scripts/wave4/generate_phase1_from_phase0.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-024.json

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
