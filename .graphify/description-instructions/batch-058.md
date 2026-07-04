# Node Description Batch 59 of 61

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
For an entity node (any other kind — e.g. a person, place, event, object),
describe what the entity is and its role, grounded in its type, its
relations (neighbors) and the provided citations/evidence — e.g.
"Lady Carfax, a wealthy heiress who disappears en route to Lausanne.".
Ground entity descriptions in the citations/evidence when present; do not
speculate beyond the context, so a node with no supporting context may be
left out of the reply.
No marketing language.
Respond ONLY with a JSON object mapping each node id (as a string) to its
one-sentence description — no prose, no markdown fences.

- "wave2_update_wave2_kanban_rationale_108": "Update the existing WAVE_2_KANBAN board." | kind=entity | source=scripts/wave2/update_wave2_kanban.py:L108 | neighbors=[update_kanban_board()]
- "wave2_update_wave2_kanban_rationale_33": "Find Obsidian vaults on the system." | kind=entity | source=scripts/wave2/update_wave2_kanban.py:L33 | neighbors=[find_obsidian_vaults()]
- "wave2_update_wave2_kanban_rationale_55": "Execute gcloud command and return output." | kind=entity | source=scripts/wave2/update_wave2_kanban.py:L55 | neighbors=[run_gcloud_command()]
- "wave2_update_wave2_kanban_rationale_69": "Get status of a specific ticket from VM." | kind=entity | source=scripts/wave2/update_wave2_kanban.py:L69 | neighbors=[get_ticket_status()]
- "wave2_update_wave2_kanban_rationale_89": "Get status of all Phase 5 tickets." | kind=entity | source=scripts/wave2/update_wave2_kanban.py:L89 | neighbors=[get_all_phase5_status()]
- "wave2_wait_for_phase4_rationale_15": "Check if all epics have completed Phase 4." | kind=entity | source=scripts/wave2/wait_for_phase4.py:L15 | neighbors=[check_completion()]
- "wave2_wait_for_phase4_rationale_37": "Monitor Phase 4 completion." | kind=entity | source=scripts/wave2/wait_for_phase4.py:L37 | neighbors=[main()]
- "wave3_generate_wave3_phase0_scripts_rationale_105": "Generate Phase 0 script by copying Wave 2 template." | kind=entity | source=scripts/wave3/generate_wave3_phase0_scripts.py:L105 | neighbors=[generate_phase0_script()]
- "wave3_generate_wave3_phase0_scripts_rationale_266": "Generate launcher script for all Phase 0 epics." | kind=entity | source=scripts/wave3/generate_wave3_phase0_scripts.py:L266 | neighbors=[generate_launcher_script()]
- "wave3_generate_wave3_phase0_scripts_rationale_300": "Generate all Wave 3 Phase 0 scripts." | kind=entity | source=scripts/wave3/generate_wave3_phase0_scripts.py:L300 | neighbors=[main()]
- "wave3_generate_wave3_phase0_scripts_rationale_98": "Load API key from JSON file." | kind=entity | source=scripts/wave3/generate_wave3_phase0_scripts.py:L98 | neighbors=[load_api_key()]
- "wave3_generate_wave3_phase1_scripts_rationale_122": "Generate all Wave 3 Phase 1 scripts." | kind=entity | source=scripts/wave3/generate_wave3_phase1_scripts.py:L122 | neighbors=[main()]
- "wave3_generate_wave3_phase1_scripts_rationale_39": "Generate Phase 1 script by copying Wave 2 template pattern." | kind=entity | source=scripts/wave3/generate_wave3_phase1_scripts.py:L39 | neighbors=[generate_phase1_script()]
- "wave3_generate_wave3_phase1_scripts_rationale_92": "Generate launcher script for all Phase 1 epics." | kind=entity | source=scripts/wave3/generate_wave3_phase1_scripts.py:L92 | neighbors=[generate_launcher_script()]
- "wave3_generate_wave3_phase2_scripts_rationale_85": "Generate Phase 2 scripts for all Wave 3 epics" | kind=entity | source=scripts/wave3/generate_wave3_phase2_scripts.py:L85 | neighbors=[generate_phase2_scripts()]
- "wave3_generate_wave3_phase3_scripts_corrected_load_api_key": "load_api_key()" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase3_scripts_CORRECTED.py:L10 | neighbors=[generate_wave3_phase3_scripts_CORRECTED…]
- "wave3_generate_wave3_phase3_scripts_corrected_rationale_35": "Generate Phase 3 script for given epic number" | kind=entity | source=scripts/wave3/generate_wave3_phase3_scripts_CORRECTED.py:L35 | neighbors=[generate_phase3_script()]
- "wave3_generate_wave3_phase3_scripts_corrected_rationale_98": "Generate launcher script for all Phase 3 scripts" | kind=entity | source=scripts/wave3/generate_wave3_phase3_scripts_CORRECTED.py:L98 | neighbors=[generate_launcher()]
- "wave3_generate_wave3_phase3_scripts_rationale_124": "Generate all Phase 3 scripts" | kind=entity | source=scripts/wave3/generate_wave3_phase3_scripts.py:L124 | neighbors=[main()]
- "wave3_generate_wave3_phase3_scripts_rationale_22": "Generate Phase 3 script by copying Phase 2 pattern" | kind=entity | source=scripts/wave3/generate_wave3_phase3_scripts.py:L22 | neighbors=[generate_phase3_script()]
- "wave3_generate_wave3_phase3_scripts_rationale_89": "Generate launcher script for all Phase 3 epics" | kind=entity | source=scripts/wave3/generate_wave3_phase3_scripts.py:L89 | neighbors=[generate_launcher_script()]
- "wave3_generate_wave3_phase4_scripts_rationale_110": "Generate launcher script for all Phase 4 scripts" | kind=entity | source=scripts/wave3/generate_wave3_phase4_scripts.py:L110 | neighbors=[generate_launcher()]
- "wave3_generate_wave3_phase4_scripts_rationale_39": "Generate Phase 4 script for given epic number" | kind=entity | source=scripts/wave3/generate_wave3_phase4_scripts.py:L39 | neighbors=[generate_phase4_script()]
- "wave4_audit_and_remove_pr_references_rationale_42": "Check if line contains acceptable PR context." | kind=entity | source=scripts/wave4/audit_and_remove_pr_references.py:L42 | neighbors=[is_acceptable_context()]
- "wave4_audit_and_remove_pr_references_rationale_50": "Find all PR references in a file." | kind=entity | source=scripts/wave4/audit_and_remove_pr_references.py:L50 | neighbors=[find_pr_references()]
- "wave4_audit_and_remove_pr_references_rationale_78": "Audit all markdown files in directory." | kind=entity | source=scripts/wave4/audit_and_remove_pr_references.py:L78 | neighbors=[audit_directory()]
- "wave4_audit_and_remove_pr_references_rationale_89": "Generate recommended fixes for PR references." | kind=entity | source=scripts/wave4/audit_and_remove_pr_references.py:L89 | neighbors=[generate_fixes()]
- "wave4_execute_80_80_recovery_rationale_120": "Upload missing Phase 5 scripts and launch execution." | kind=entity | source=scripts/wave4/execute_80_80_recovery.py:L120 | neighbors=[step2_upload_missing_phase5_scripts()]
- "wave4_execute_80_80_recovery_rationale_175": "Monitor recovery progress." | kind=entity | source=scripts/wave4/execute_80_80_recovery.py:L175 | neighbors=[monitor_recovery()]
- "wave4_execute_80_80_recovery_rationale_220": "Execute recovery plan." | kind=entity | source=scripts/wave4/execute_80_80_recovery.py:L220 | neighbors=[main()]
- "wave4_execute_80_80_recovery_rationale_28": "Execute gcloud command and return exit code and output." | kind=entity | source=scripts/wave4/execute_80_80_recovery.py:L28 | neighbors=[run_gcloud()]
- "wave4_execute_80_80_recovery_rationale_40": "Fix Phase 6 PATH issue for 3 epics." | kind=entity | source=scripts/wave4/execute_80_80_recovery.py:L40 | neighbors=[step1_fix_phase6_path_issue()]
- "wave4_execute_phase0_with_jane_street_rationale_13": "Execute Phase 0 with Jane Street integration.\r     \r     Args:\r         epic_id:" | kind=entity | source=scripts/wave4/execute_phase0_with_jane_street.py:L13 | neighbors=[execute_phase_0()]
- "wave4_execute_phase0_with_jane_street_rationale_23": "# TODO: Fetch jCodemunch data" | kind=entity | source=scripts/wave4/execute_phase0_with_jane_street.py:L23 | neighbors=[execute_phase0_with_jane_street.py]
- "wave4_fix_phase6_prerequisite_v3_replace_check": "replace_check()" | kind=code-symbol | source=scripts/wave4/fix_phase6_prerequisite_v3.py:L35 | neighbors=[fix_phase6_prerequisite_v3.py]
- "wave4_generate_phase0_scripts_rationale_13": "Load pending epics from epic_roadmap.json (001-080 range)." | kind=entity | source=scripts/wave4/generate_phase0_scripts.py:L13 | neighbors=[load_pending_epics()]
- "wave4_generate_phase0_scripts_rationale_212": "Generate Phase 0 scripts for all pending epics with API rotation." | kind=entity | source=scripts/wave4/generate_phase0_scripts.py:L212 | neighbors=[generate_scripts()]
- "wave4_generate_phase0_scripts_rationale_40": "Load all 15 API keys from JSON files." | kind=entity | source=scripts/wave4/generate_phase0_scripts.py:L40 | neighbors=[load_api_keys()]
- "wave4_generate_phase1_scripts_rationale_22": "Load all 15 API keys from JSON files." | kind=entity | source=scripts/wave4/generate_phase1_scripts.py:L22 | neighbors=[load_api_keys()]
- "wave4_generate_phase4_recovery_rationale_26": "Generate recovery scripts using building-blocks method." | kind=entity | source=scripts/wave4/generate_phase4_recovery.py:L26 | neighbors=[generate_recovery_scripts()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-058.json

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
