# Node Description Batch 60 of 61

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

- "wave4_generate_phase4_recovery_rationale_70": "Generate launcher script for recovery epics." | kind=entity | source=scripts/wave4/generate_phase4_recovery.py:L70 | neighbors=[generate_recovery_launcher()]
- "wave4_generate_wave4_phase1_scripts_rationale_39": "Generate Phase 1 script using /epic-intake slash command." | kind=entity | source=scripts/wave4/generate_wave4_phase1_scripts.py:L39 | neighbors=[generate_phase1_script()]
- "wave4_generate_wave4_phase1_scripts_rationale_56": "Generate launcher script for all Phase 1 epics." | kind=entity | source=scripts/wave4/generate_wave4_phase1_scripts.py:L56 | neighbors=[generate_launcher_script()]
- "wave4_generate_wave4_phase1_scripts_rationale_86": "Generate all Wave 4 Phase 1 scripts." | kind=entity | source=scripts/wave4/generate_wave4_phase1_scripts.py:L86 | neighbors=[main()]
- "wave4_generate_wave4_phase3_scripts_rationale_20": "Generate Phase 3 script using /epic-scan slash command" | kind=entity | source=scripts/wave4/generate_wave4_phase3_scripts.py:L20 | neighbors=[generate_phase3_script()]
- "wave4_generate_wave4_phase3_scripts_rationale_42": "Generate launcher script for all Phase 3 epics" | kind=entity | source=scripts/wave4/generate_wave4_phase3_scripts.py:L42 | neighbors=[generate_launcher_script()]
- "wave4_generate_wave4_phase3_scripts_rationale_77": "Generate all Phase 3 scripts" | kind=entity | source=scripts/wave4/generate_wave4_phase3_scripts.py:L77 | neighbors=[main()]
- "wave4_generate_wave4_phase4_scripts_rationale_39": "Generate Phase 4 script using /epic-tickets slash command" | kind=entity | source=scripts/wave4/generate_wave4_phase4_scripts.py:L39 | neighbors=[generate_phase4_script()]
- "wave4_generate_wave4_phase4_scripts_rationale_68": "Generate launcher script for all Phase 4 scripts" | kind=entity | source=scripts/wave4/generate_wave4_phase4_scripts.py:L68 | neighbors=[generate_launcher()]
- "wave6_add_missing_phase_modes_rationale_32": "Add missing mode field to all phases." | kind=entity | source=scripts/wave6/add_missing_phase_modes.py:L32 | neighbors=[add_missing_modes()]
- "wave6_add_missing_top_level_fields_rationale_18": "Add missing top-level fields to all manifests." | kind=entity | source=scripts/wave6/add_missing_top_level_fields.py:L18 | neighbors=[add_missing_fields()]
- "wave6_add_phase1_5_to_manifests_rationale_15": "Add Phase 1.5 definition to manifest if missing." | kind=entity | source=scripts/wave6/add_phase1_5_to_manifests.py:L15 | neighbors=[add_phase_1_5_to_manifest()]
- "wave6_add_phase1_5_to_manifests_rationale_69": "Add Phase 1.5 to all Wave 6 epic manifests." | kind=entity | source=scripts/wave6/add_phase1_5_to_manifests.py:L69 | neighbors=[main()]
- "wave6_check_24_status_main": "main()" | kind=code-symbol | source=scripts/wave6/check_24_status.py:L13 | neighbors=[check_24_status.py]
- "wave6_check_wave6_only_status_rationale_9": "Check Phase 1 completion status for Wave 6 epics only." | kind=entity | source=scripts/wave6/check_wave6_only_status.py:L9 | neighbors=[check_phase1_status()]
- "wave6_fix_epic_004_manifest_rationale_11": "Fix the top-level epic_id field in EPIC-CCN-004 manifest." | kind=entity | source=scripts/wave6/fix_epic_004_manifest.py:L11 | neighbors=[fix_epic_004_manifest()]
- "wave6_fix_function_names_rationale_14": "Fix function name in a single script." | kind=entity | source=scripts/wave6/fix_function_names.py:L14 | neighbors=[fix_script()]
- "wave6_fix_imports_python_rationale_63": "Fix imports in a single script." | kind=entity | source=scripts/wave6/fix_imports_python.py:L63 | neighbors=[fix_script()]
- "wave6_generate_phase1_5_scripts_rationale_34": "Get agent ID for epic using round-robin" | kind=entity | source=scripts/wave6/generate_phase1_5_scripts.py:L34 | neighbors=[get_agent_id()]
- "wave6_generate_phase1_5_scripts_rationale_38": "Generate Phase 1.5 script for a single epic" | kind=entity | source=scripts/wave6/generate_phase1_5_scripts.py:L38 | neighbors=[generate_phase1_5_script()]
- "wave6_generate_phase1_5_scripts_rationale_65": "Generate all Phase 1.5 scripts" | kind=entity | source=scripts/wave6/generate_phase1_5_scripts.py:L65 | neighbors=[main()]
- "wave6_generate_phase1_report_rationale_10": "Generate completion report for Wave 6 Phase 1." | kind=entity | source=scripts/wave6/generate_phase1_report.py:L10 | neighbors=[generate_report()]
- "wave6_identify_remaining_phase1_rationale_9": "Find epics that need Phase 1 completion." | kind=entity | source=scripts/wave6/identify_remaining_phase1.py:L9 | neighbors=[identify_remaining()]
- "wave6_inject_missing_phase0_events_main": "main()" | kind=code-symbol | source=scripts/wave6/inject_missing_phase0_events.py:L14 | neighbors=[inject_missing_phase0_events.py]
- "wave6_lamport_cleanup_all_phase1_epic_003_rationale_15": "Remove ALL Phase 1 events for EPIC-CCN-003" | kind=entity | source=scripts/wave6/lamport_cleanup_all_phase1_epic_003.py:L15 | neighbors=[cleanup_all_phase1()]
- "wave6_lamport_cleanup_epic_003_phase1_rationale_14": "Append phase_fail event to close out clock 177" | kind=entity | source=scripts/wave6/lamport_cleanup_epic_003_phase1.py:L14 | neighbors=[append_phase_fail()]
- "wave6_lamport_surgical_cleanup_epic_003_rationale_20": "Remove stale Phase 1 events for EPIC-CCN-003" | kind=entity | source=scripts/wave6/lamport_surgical_cleanup_epic_003.py:L20 | neighbors=[surgical_cleanup()]
- "wave6_regenerate_24_from_working_template_rationale_23": "Generate script for epic by replacing template values." | kind=entity | source=scripts/wave6/regenerate_24_from_working_template.py:L23 | neighbors=[regenerate_script()]
- "wave6_regenerate_24_from_working_template_rationale_34": "Regenerate all 24 scripts from working template." | kind=entity | source=scripts/wave6/regenerate_24_from_working_template.py:L34 | neighbors=[main()]
- "wave6_reset_epic_001_phase0_rationale_8": "Reset Phase 0 status to pending" | kind=entity | source=scripts/wave6/reset_epic_001_phase0.py:L8 | neighbors=[reset_phase0()]
- "wave6_reset_manifests_for_wave6_rationale_12": "Reset all phases in manifest to pending status." | kind=entity | source=scripts/wave6/reset_manifests_for_wave6.py:L12 | neighbors=[reset_manifest()]
- "wave6_reset_phase1_status_rationale_18": "Reset Phase 1 status to pending for an epic." | kind=entity | source=scripts/wave6/reset_phase1_status.py:L18 | neighbors=[reset_phase1_status()]
- "wave6_validate_phase0_4epics_main": "main()" | kind=code-symbol | source=scripts/wave6/validate_phase0_4epics.py:L9 | neighbors=[validate_phase0_4epics.py]
- "wave6_validate_wave6_scope_rationale_11": "Validate Wave 6 scope (epics 1-80)." | kind=entity | source=scripts/wave6/validate_wave6_scope.py:L11 | neighbors=[validate_wave6_scope()]
- "wave7_fix_failed_epics_with_active_keys_rationale_26": "Load active API keys from JSON files" | kind=entity | source=building-blocks/wave7/fix_failed_epics_with_active_keys.py:L26 | neighbors=[load_active_api_keys()]
- "wave7_fix_failed_epics_with_active_keys_rationale_48": "Fix a single epic script with valid API key" | kind=entity | source=building-blocks/wave7/fix_failed_epics_with_active_keys.py:L48 | neighbors=[fix_epic_script()]
- "wave7_generate_phase0_scripts_fixed_rationale_140": "Generate Phase 0 scripts for Wave 7 epics with API rotation.\r     \r     Args:" | kind=entity | source=scripts/wave7/generate_phase0_scripts_fixed.py:L140 | neighbors=[generate_scripts()]
- "wave7_generate_phase0_scripts_fixed_rationale_19": "Load pending epics from epic_roadmap_wave7.json (all 161 epics)." | kind=entity | source=scripts/wave7/generate_phase0_scripts_fixed.py:L19 | neighbors=[load_pending_epics()]
- "wave7_generate_phase0_scripts_fixed_rationale_53": "Load all 16 API keys from JSON files." | kind=entity | source=scripts/wave7/generate_phase0_scripts_fixed.py:L53 | neighbors=[load_api_keys()]
- "wave7_generate_phase0_scripts_fixed_rationale_67": "Generate Bob CLI message content." | kind=entity | source=scripts/wave7/generate_phase0_scripts_fixed.py:L67 | neighbors=[get_bob_message()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-059.json

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
