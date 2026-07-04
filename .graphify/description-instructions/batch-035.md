# Node Description Batch 36 of 61

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

- "wave4_audit_and_remove_pr_references_generate_fixes": "generate_fixes()" | kind=code-symbol | source=scripts/wave4/audit_and_remove_pr_references.py:L88 | neighbors=[audit_and_remove_pr_references.py, main(), Generate recommended fixes for PR refer…]
- "wave4_audit_and_remove_pr_references_is_acceptable_context": "is_acceptable_context()" | kind=code-symbol | source=scripts/wave4/audit_and_remove_pr_references.py:L41 | neighbors=[audit_and_remove_pr_references.py, find_pr_references(), Check if line contains acceptable PR co…]
- "wave4_audit_and_remove_pr_references_main": "main()" | kind=code-symbol | source=scripts/wave4/audit_and_remove_pr_references.py:L128 | neighbors=[audit_and_remove_pr_references.py, audit_directory(), generate_fixes()]
- "wave4_generate_phase0_scripts_load_api_keys": "load_api_keys()" | kind=code-symbol | source=scripts/wave4/generate_phase0_scripts.py:L39 | neighbors=[generate_phase0_scripts.py, generate_scripts(), Load all 15 API keys from JSON files.]
- "wave4_generate_phase0_scripts_load_pending_epics": "load_pending_epics()" | kind=code-symbol | source=scripts/wave4/generate_phase0_scripts.py:L12 | neighbors=[generate_phase0_scripts.py, generate_scripts(), Load pending epics from epic_roadmap.js…]
- "wave4_generate_phase4_recovery_generate_recovery_launcher": "generate_recovery_launcher()" | kind=code-symbol | source=scripts/wave4/generate_phase4_recovery.py:L69 | neighbors=[generate_phase4_recovery.py, generate_recovery_scripts(), Generate launcher script for recovery e…]
- "wave4_generate_wave4_phase1_scripts_generate_launcher_script": "generate_launcher_script()" | kind=code-symbol | source=scripts/wave4/generate_wave4_phase1_scripts.py:L55 | neighbors=[generate_wave4_phase1_scripts.py, main(), Generate launcher script for all Phase …]
- "wave4_generate_wave4_phase1_scripts_generate_phase1_script": "generate_phase1_script()" | kind=code-symbol | source=scripts/wave4/generate_wave4_phase1_scripts.py:L38 | neighbors=[generate_wave4_phase1_scripts.py, main(), Generate Phase 1 script using /epic-int…]
- "wave4_generate_wave4_phase3_scripts_generate_launcher_script": "generate_launcher_script()" | kind=code-symbol | source=scripts/wave4/generate_wave4_phase3_scripts.py:L41 | neighbors=[generate_wave4_phase3_scripts.py, main(), Generate launcher script for all Phase …]
- "wave4_generate_wave4_phase3_scripts_generate_phase3_script": "generate_phase3_script()" | kind=code-symbol | source=scripts/wave4/generate_wave4_phase3_scripts.py:L19 | neighbors=[generate_wave4_phase3_scripts.py, main(), Generate Phase 3 script using /epic-sca…]
- "wave4_generate_wave4_phase4_scripts_generate_launcher": "generate_launcher()" | kind=code-symbol | source=scripts/wave4/generate_wave4_phase4_scripts.py:L67 | neighbors=[generate_wave4_phase4_scripts.py, main(), Generate launcher script for all Phase …]
- "wave4_generate_wave4_phase4_scripts_generate_phase4_script": "generate_phase4_script()" | kind=code-symbol | source=scripts/wave4/generate_wave4_phase4_scripts.py:L38 | neighbors=[generate_wave4_phase4_scripts.py, main(), Generate Phase 4 script using /epic-tic…]
- "wave4_generate_wave4_phase4_scripts_main": "main()" | kind=code-symbol | source=scripts/wave4/generate_wave4_phase4_scripts.py:L108 | neighbors=[generate_wave4_phase4_scripts.py, generate_launcher(), generate_phase4_script()]
- "wave6_add_missing_phase_modes": "add_missing_phase_modes.py" | kind=code-symbol | source=scripts/wave6/add_missing_phase_modes.py:L1 | neighbors=[0029dd5 V12.53: ALL 10 phases now use c…, bb0a399 V12.53: ALL 10 phases now use c…, add_missing_modes()]
- "wave6_add_missing_top_level_fields": "add_missing_top_level_fields.py" | kind=code-symbol | source=scripts/wave6/add_missing_top_level_fields.py:L1 | neighbors=[3bfeacd fix: Add missing V12.52 top-lev…, 70cd659 fix: Add missing V12.52 top-lev…, add_missing_fields()]
- "wave6_add_phase1_5_to_manifests_add_phase_1_5_to_manifest": "add_phase_1_5_to_manifest()" | kind=code-symbol | source=scripts/wave6/add_phase1_5_to_manifests.py:L14 | neighbors=[add_phase1_5_to_manifests.py, main(), Add Phase 1.5 definition to manifest if…]
- "wave6_add_phase1_5_to_manifests_main": "main()" | kind=code-symbol | source=scripts/wave6/add_phase1_5_to_manifests.py:L68 | neighbors=[add_phase1_5_to_manifests.py, add_phase_1_5_to_manifest(), Add Phase 1.5 to all Wave 6 epic manife…]
- "wave6_check_24_status": "check_24_status.py" | kind=code-symbol | source=scripts/wave6/check_24_status.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, main()]
- "wave6_check_wave6_only_status": "check_wave6_only_status.py" | kind=code-symbol | source=scripts/wave6/check_wave6_only_status.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, check_phase1_status()]
- "wave6_fix_epic_004_manifest": "fix_epic_004_manifest.py" | kind=code-symbol | source=scripts/wave6/fix_epic_004_manifest.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, fix_epic_004_manifest()]
- "wave6_fix_function_names_fix_script": "fix_script()" | kind=code-symbol | source=scripts/wave6/fix_function_names.py:L13 | neighbors=[fix_function_names.py, main(), Fix function name in a single script.]
- "wave6_fix_imports_python_fix_script": "fix_script()" | kind=code-symbol | source=scripts/wave6/fix_imports_python.py:L62 | neighbors=[fix_imports_python.py, main(), Fix imports in a single script.]
- "wave6_generate_phase1_5_scripts_get_agent_id": "get_agent_id()" | kind=code-symbol | source=scripts/wave6/generate_phase1_5_scripts.py:L33 | neighbors=[generate_phase1_5_scripts.py, generate_phase1_5_script(), Get agent ID for epic using round-robin]
- "wave6_generate_phase1_5_scripts_main": "main()" | kind=code-symbol | source=scripts/wave6/generate_phase1_5_scripts.py:L64 | neighbors=[generate_phase1_5_scripts.py, generate_phase1_5_script(), Generate all Phase 1.5 scripts]
- "wave6_generate_phase1_report": "generate_phase1_report.py" | kind=code-symbol | source=scripts/wave6/generate_phase1_report.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, generate_report()]
- "wave6_identify_remaining_phase1": "identify_remaining_phase1.py" | kind=code-symbol | source=scripts/wave6/identify_remaining_phase1.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, identify_remaining()]
- "wave6_inject_missing_phase0_events": "inject_missing_phase0_events.py" | kind=code-symbol | source=scripts/wave6/inject_missing_phase0_events.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, main()]
- "wave6_lamport_cleanup_all_phase1_epic_003": "lamport_cleanup_all_phase1_epic_003.py" | kind=code-symbol | source=scripts/wave6/lamport_cleanup_all_phase1_epic_003.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, cleanup_all_phase1()]
- "wave6_lamport_cleanup_epic_003_phase1": "lamport_cleanup_epic_003_phase1.py" | kind=code-symbol | source=scripts/wave6/lamport_cleanup_epic_003_phase1.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, append_phase_fail()]
- "wave6_lamport_surgical_cleanup_epic_003": "lamport_surgical_cleanup_epic_003.py" | kind=code-symbol | source=scripts/wave6/lamport_surgical_cleanup_epic_003.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, surgical_cleanup()]
- "wave6_regenerate_24_from_working_template_main": "main()" | kind=code-symbol | source=scripts/wave6/regenerate_24_from_working_template.py:L33 | neighbors=[regenerate_24_from_working_template.py, regenerate_script(), Regenerate all 24 scripts from working …]
- "wave6_regenerate_24_from_working_template_regenerate_script": "regenerate_script()" | kind=code-symbol | source=scripts/wave6/regenerate_24_from_working_template.py:L22 | neighbors=[regenerate_24_from_working_template.py, main(), Generate script for epic by replacing t…]
- "wave6_reset_epic_001_phase0": "reset_epic_001_phase0.py" | kind=code-symbol | source=scripts/wave6/reset_epic_001_phase0.py:L1 | neighbors=[3a92e1e feat: Wave 6 Phase 0 preparatio…, ceae42c feat: Wave 6 Phase 0 preparatio…, reset_phase0()]
- "wave6_reset_manifests_for_wave6_reset_manifest": "reset_manifest()" | kind=code-symbol | source=scripts/wave6/reset_manifests_for_wave6.py:L11 | neighbors=[reset_manifests_for_wave6.py, main(), Reset all phases in manifest to pending…]
- "wave6_reset_phase1_status_reset_phase1_status": "reset_phase1_status()" | kind=code-symbol | source=scripts/wave6/reset_phase1_status.py:L17 | neighbors=[reset_phase1_status.py, main(), Reset Phase 1 status to pending for an …]
- "wave6_validate_phase0_4epics": "validate_phase0_4epics.py" | kind=code-symbol | source=scripts/wave6/validate_phase0_4epics.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, main()]
- "wave6_validate_wave6_scope": "validate_wave6_scope.py" | kind=code-symbol | source=scripts/wave6/validate_wave6_scope.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, validate_wave6_scope()]
- "wave7_fix_failed_epics_with_active_keys_fix_epic_script": "fix_epic_script()" | kind=code-symbol | source=building-blocks/wave7/fix_failed_epics_with_active_keys.py:L47 | neighbors=[fix_failed_epics_with_active_keys.py, main(), Fix a single epic script with valid API…]
- "wave7_fix_failed_epics_with_active_keys_load_active_api_keys": "load_active_api_keys()" | kind=code-symbol | source=building-blocks/wave7/fix_failed_epics_with_active_keys.py:L25 | neighbors=[fix_failed_epics_with_active_keys.py, main(), Load active API keys from JSON files]
- "wave7_fix_failed_epics_with_active_keys_main": "main()" | kind=code-symbol | source=building-blocks/wave7/fix_failed_epics_with_active_keys.py:L80 | neighbors=[fix_failed_epics_with_active_keys.py, fix_epic_script(), load_active_api_keys()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-035.json

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
