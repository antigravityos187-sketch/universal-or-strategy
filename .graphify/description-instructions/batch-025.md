# Node Description Batch 26 of 61

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

- "wave4_generate_phase2_scripts": "generate_phase2_scripts.py" | kind=code-symbol | source=scripts/wave4/generate_phase2_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_generate_phase3_scripts": "generate_phase3_scripts.py" | kind=code-symbol | source=scripts/wave4/generate_phase3_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_generate_phase4_recovery_generate_recovery_scripts": "generate_recovery_scripts()" | kind=code-symbol | source=scripts/wave4/generate_phase4_recovery.py:L25 | neighbors=[generate_phase4_recovery.py, generate_recovery_launcher(), main(), Generate recovery scripts using buildin…]
- "wave4_generate_phase4_scripts": "generate_phase4_scripts.py" | kind=code-symbol | source=scripts/wave4/generate_phase4_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_generate_phase5_scripts": "generate_phase5_scripts.py" | kind=code-symbol | source=scripts/wave4/generate_phase5_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_generate_wave4_phase1_scripts_main": "main()" | kind=code-symbol | source=scripts/wave4/generate_wave4_phase1_scripts.py:L85 | neighbors=[generate_wave4_phase1_scripts.py, generate_launcher_script(), generate_phase1_script(), Generate all Wave 4 Phase 1 scripts.]
- "wave4_generate_wave4_phase3_scripts_main": "main()" | kind=code-symbol | source=scripts/wave4/generate_wave4_phase3_scripts.py:L76 | neighbors=[generate_wave4_phase3_scripts.py, generate_launcher_script(), generate_phase3_script(), Generate all Phase 3 scripts]
- "wave4_identify_missing_phase6": "identify_missing_phase6.py" | kind=code-symbol | source=scripts/wave4/identify_missing_phase6.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_launch_phase6_recovery": "launch_phase6_recovery.py" | kind=code-symbol | source=scripts/wave4/launch_phase6_recovery.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_launch_phase6_recovery_round2": "launch_phase6_recovery_round2.py" | kind=code-symbol | source=scripts/wave4/launch_phase6_recovery_round2.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_regenerate_p6_027": "regenerate_p6_027.py" | kind=code-symbol | source=scripts/wave4/regenerate_p6_027.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_regenerate_p6_060_075": "regenerate_p6_060_075.py" | kind=code-symbol | source=scripts/wave4/regenerate_p6_060_075.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave4_regenerate_p6_scripts": "regenerate_p6_scripts.py" | kind=code-symbol | source=scripts/wave4/regenerate_p6_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "wave6_add_phase1_5_to_manifests": "add_phase1_5_to_manifests.py" | kind=code-symbol | source=scripts/wave6/add_phase1_5_to_manifests.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, add_phase_1_5_to_manifest(), main()]
- "wave6_fix_function_names": "fix_function_names.py" | kind=code-symbol | source=scripts/wave6/fix_function_names.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, fix_script(), main()]
- "wave6_fix_imports_python": "fix_imports_python.py" | kind=code-symbol | source=scripts/wave6/fix_imports_python.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, fix_script(), main()]
- "wave6_generate_phase1_5_scripts_generate_phase1_5_script": "generate_phase1_5_script()" | kind=code-symbol | source=scripts/wave6/generate_phase1_5_scripts.py:L37 | neighbors=[generate_phase1_5_scripts.py, get_agent_id(), main(), Generate Phase 1.5 script for a single …]
- "wave6_regenerate_24_from_working_template": "regenerate_24_from_working_template.py" | kind=code-symbol | source=scripts/wave6/regenerate_24_from_working_template.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, main(), regenerate_script()]
- "wave6_reset_manifests_for_wave6": "reset_manifests_for_wave6.py" | kind=code-symbol | source=scripts/wave6/reset_manifests_for_wave6.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, main(), reset_manifest()]
- "wave6_reset_phase1_status": "reset_phase1_status.py" | kind=code-symbol | source=scripts/wave6/reset_phase1_status.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, main(), reset_phase1_status()]
- "wave7_generate_phase0_scripts_generate_scripts": "generate_scripts()" | kind=code-symbol | source=scripts/wave7/generate_phase0_scripts.py:L203 | neighbors=[generate_phase0_scripts.py, load_api_keys(), load_pending_epics(), Generate Phase 0 scripts for all pendin…]
- "wave7_identify_phase0_complete": "identify_phase0_complete.py" | kind=code-symbol | source=scripts/wave7/identify_phase0_complete.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, find_phase0_complete_epics(), main()]
- "wave7_launch_epic_with_fixed_env_launch_epic": "launch_epic()" | kind=code-symbol | source=building-blocks/wave7/launch_epic_with_fixed_env.py:L52 | neighbors=[launch_epic_with_fixed_env.py, get_fixed_environment(), launch_epic_batch(), Launch an epic script with fixed enviro…]
- "add_path_to_scripts_add_path_to_script": "add_path_to_script()" | kind=code-symbol | source=add_path_to_scripts.py:L9 | neighbors=[add_path_to_scripts.py, main(), Add PATH export after shebang.]
- "add_path_to_scripts_main": "main()" | kind=code-symbol | source=add_path_to_scripts.py:L34 | neighbors=[add_path_to_scripts.py, add_path_to_script(), Add PATH to all generated Phase 0 scrip…]
- "analyze_complexity_audit_analyze_complexity_audit": "analyze_complexity_audit()" | kind=code-symbol | source=analyze_complexity_audit.py:L14 | neighbors=[analyze_complexity_audit.py, main(), Parse complexity audit and extract meth…]
- "analyze_wave7_phase0_complete_check_process_running": "check_process_running()" | kind=code-symbol | source=analyze_wave7_phase0_complete.py:L16 | neighbors=[analyze_wave7_phase0_complete.py, main(), Check if Python launcher is still runni…]
- "analyze_wave7_phase0_complete_extract_method_from_hotspots": "extract_method_from_hotspots()" | kind=code-symbol | source=analyze_wave7_phase0_complete.py:L53 | neighbors=[analyze_wave7_phase0_complete.py, main(), Extract method name from 00-hotspots.md.]
- "analyze_wave7_phase0_complete_get_epic_files": "get_epic_files()" | kind=code-symbol | source=analyze_wave7_phase0_complete.py:L33 | neighbors=[analyze_wave7_phase0_complete.py, main(), Get list of files in epic directory.]
- "analyze_wave7_phase0_complete_get_manifest_timestamp": "get_manifest_timestamp()" | kind=code-symbol | source=analyze_wave7_phase0_complete.py:L41 | neighbors=[analyze_wave7_phase0_complete.py, main(), Extract timestamp from manifest.json.]
- "analyze_wave7_status": "analyze_wave7_status.py" | kind=code-symbol | source=analyze_wave7_status.py:L1 | neighbors=[main(), 180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …]
- "card_board_main_cn": "cn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, An(), c()]
- "card_board_main_fr": "fr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, Mr(), qr()]
- "card_board_main_g": "g()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, N(), p()]
- "card_board_main_kn": "kn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, Tn(), Pr()]
- "card_board_main_kr": "Kr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, jr(), qr()]
- "card_board_main_mn": "Mn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, An(), b()]
- "card_board_main_pr": "Pr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, kn(), r()]
- "card_board_main_sr": "sr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, Dr(), Xr()]
- "card_board_main_xn": "Xn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, N(), Zn()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-025.json

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
