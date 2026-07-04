# Node Description Batch 42 of 61

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

- "wave6_fix_epic_004_manifest_fix_epic_004_manifest": "fix_epic_004_manifest()" | kind=code-symbol | source=scripts/wave6/fix_epic_004_manifest.py:L10 | neighbors=[fix_epic_004_manifest.py, Fix the top-level epic_id field in EPIC…]
- "wave6_fix_function_names_main": "main()" | kind=code-symbol | source=scripts/wave6/fix_function_names.py:L25 | neighbors=[fix_function_names.py, fix_script()]
- "wave6_fix_imports_python_main": "main()" | kind=code-symbol | source=scripts/wave6/fix_imports_python.py:L90 | neighbors=[fix_imports_python.py, fix_script()]
- "wave6_fix_manifest_phase_numbering": "fix_manifest_phase_numbering.py" | kind=code-symbol | source=scripts/wave6/fix_manifest_phase_numbering.py:L1 | neighbors=[3a92e1e feat: Wave 6 Phase 0 preparatio…, ceae42c feat: Wave 6 Phase 0 preparatio…]
- "wave6_generate_phase0_scripts": "generate_phase0_scripts.py" | kind=code-symbol | source=scripts/wave6/generate_phase0_scripts.py:L1 | neighbors=[3a92e1e feat: Wave 6 Phase 0 preparatio…, ceae42c feat: Wave 6 Phase 0 preparatio…]
- "wave6_generate_phase1_report_generate_report": "generate_report()" | kind=code-symbol | source=scripts/wave6/generate_phase1_report.py:L9 | neighbors=[generate_phase1_report.py, Generate completion report for Wave 6 P…]
- "wave6_identify_remaining_phase1_identify_remaining": "identify_remaining()" | kind=code-symbol | source=scripts/wave6/identify_remaining_phase1.py:L8 | neighbors=[identify_remaining_phase1.py, Find epics that need Phase 1 completion.]
- "wave6_lamport_cleanup_all_phase1_epic_003_cleanup_all_phase1": "cleanup_all_phase1()" | kind=code-symbol | source=scripts/wave6/lamport_cleanup_all_phase1_epic_003.py:L14 | neighbors=[lamport_cleanup_all_phase1_epic_003.py, Remove ALL Phase 1 events for EPIC-CCN-…]
- "wave6_lamport_cleanup_epic_003_phase1_append_phase_fail": "append_phase_fail()" | kind=code-symbol | source=scripts/wave6/lamport_cleanup_epic_003_phase1.py:L13 | neighbors=[lamport_cleanup_epic_003_phase1.py, Append phase_fail event to close out cl…]
- "wave6_lamport_surgical_cleanup_epic_003_surgical_cleanup": "surgical_cleanup()" | kind=code-symbol | source=scripts/wave6/lamport_surgical_cleanup_epic_003.py:L19 | neighbors=[lamport_surgical_cleanup_epic_003.py, Remove stale Phase 1 events for EPIC-CC…]
- "wave6_regenerate_phase1_5_scripts_fixed": "regenerate_phase1_5_scripts_FIXED.py" | kind=code-symbol | source=scripts/wave6/regenerate_phase1_5_scripts_FIXED.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "wave6_reset_all_phase0": "reset_all_phase0.py" | kind=code-symbol | source=scripts/wave6/reset_all_phase0.py:L1 | neighbors=[3a92e1e feat: Wave 6 Phase 0 preparatio…, ceae42c feat: Wave 6 Phase 0 preparatio…]
- "wave6_reset_epic_001_phase0_reset_phase0": "reset_phase0()" | kind=code-symbol | source=scripts/wave6/reset_epic_001_phase0.py:L7 | neighbors=[reset_epic_001_phase0.py, Reset Phase 0 status to pending]
- "wave6_reset_manifests_for_wave6_main": "main()" | kind=code-symbol | source=scripts/wave6/reset_manifests_for_wave6.py:L40 | neighbors=[reset_manifests_for_wave6.py, reset_manifest()]
- "wave6_reset_phase1_status_main": "main()" | kind=code-symbol | source=scripts/wave6/reset_phase1_status.py:L52 | neighbors=[reset_phase1_status.py, reset_phase1_status()]
- "wave6_update_epic_manifest_modes": "update_epic_manifest_modes.py" | kind=code-symbol | source=scripts/wave6/update_epic_manifest_modes.py:L1 | neighbors=[0029dd5 V12.53: ALL 10 phases now use c…, bb0a399 V12.53: ALL 10 phases now use c…]
- "wave6_validate_all_epic_ids": "validate_all_epic_ids.py" | kind=code-symbol | source=scripts/wave6/validate_all_epic_ids.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "wave6_validate_roadmap": "validate_roadmap.py" | kind=code-symbol | source=scripts/wave6/validate_roadmap.py:L1 | neighbors=[3a92e1e feat: Wave 6 Phase 0 preparatio…, ceae42c feat: Wave 6 Phase 0 preparatio…]
- "wave6_validate_wave6_scope_validate_wave6_scope": "validate_wave6_scope()" | kind=code-symbol | source=scripts/wave6/validate_wave6_scope.py:L10 | neighbors=[validate_wave6_scope.py, Validate Wave 6 scope (epics 1-80).]
- "wave7_identify_phase0_complete_main": "main()" | kind=code-symbol | source=scripts/wave7/identify_phase0_complete.py:L33 | neighbors=[identify_phase0_complete.py, find_phase0_complete_epics()]
- "add_path_to_scripts_rationale_10": "Add PATH export after shebang." | kind=entity | source=add_path_to_scripts.py:L10 | neighbors=[add_path_to_script()]
- "add_path_to_scripts_rationale_35": "Add PATH to all generated Phase 0 scripts." | kind=entity | source=add_path_to_scripts.py:L35 | neighbors=[main()]
- "analyze_complexity_audit_rationale_15": "Parse complexity audit and extract methods > 8." | kind=entity | source=analyze_complexity_audit.py:L15 | neighbors=[analyze_complexity_audit()]
- "analyze_wave7_phase0_complete_rationale_17": "Check if Python launcher is still running." | kind=entity | source=analyze_wave7_phase0_complete.py:L17 | neighbors=[check_process_running()]
- "analyze_wave7_phase0_complete_rationale_34": "Get list of files in epic directory." | kind=entity | source=analyze_wave7_phase0_complete.py:L34 | neighbors=[get_epic_files()]
- "analyze_wave7_phase0_complete_rationale_42": "Extract timestamp from manifest.json." | kind=entity | source=analyze_wave7_phase0_complete.py:L42 | neighbors=[get_manifest_timestamp()]
- "analyze_wave7_phase0_complete_rationale_54": "Extract method name from 00-hotspots.md." | kind=entity | source=analyze_wave7_phase0_complete.py:L54 | neighbors=[extract_method_from_hotspots()]
- "analyze_wave7_status_main": "main()" | kind=code-symbol | source=analyze_wave7_status.py:L11 | neighbors=[analyze_wave7_status.py]
- "basehttprequesthandler": "BaseHTTPRequestHandler" | kind=code-symbol | neighbors=[ReviewHandler]
- "bob_mcp_mcp_server_greptile": "greptile" | kind=code-symbol | source=.bob/mcp.json:L1 | neighbors=[mcp.json]
- "card_board_main_dn": "dn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js]
- "card_board_main_k": "k" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js]
- "card_board_main_nn": "nn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js]
- "card_board_main_rn": "rn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js]
- "card_board_main_vn": "Vn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js]
- "card_board_main_z": "Z()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js]
- "cleanup_and_relaunch_wave7_rationale_18": "Load Wave 7 roadmap and extract methods." | kind=entity | source=cleanup_and_relaunch_wave7.py:L18 | neighbors=[get_wave7_methods()]
- "cleanup_and_relaunch_wave7_rationale_31": "Extract method name from 00-hotspots.md." | kind=entity | source=cleanup_and_relaunch_wave7.py:L31 | neighbors=[extract_method_from_hotspots()]
- "complete_wave_cross_reference_rationale_112": "Map baseline methods to Wave 6 epics" | kind=entity | source=complete_wave_cross_reference.py:L112 | neighbors=[map_baseline_to_wave6()]
- "complete_wave_cross_reference_rationale_145": "Analyze Jane Street violations from P0 file" | kind=entity | source=complete_wave_cross_reference.py:L145 | neighbors=[analyze_jane_street_violations()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-041.json

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
