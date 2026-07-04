# Node Description Batch 37 of 61

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

- "wave7_generate_phase0_scripts_fixed_get_bob_message": "get_bob_message()" | kind=code-symbol | source=scripts/wave7/generate_phase0_scripts_fixed.py:L66 | neighbors=[generate_phase0_scripts_fixed.py, generate_scripts(), Generate Bob CLI message content.]
- "wave7_generate_phase0_scripts_fixed_load_api_keys": "load_api_keys()" | kind=code-symbol | source=scripts/wave7/generate_phase0_scripts_fixed.py:L52 | neighbors=[generate_phase0_scripts_fixed.py, generate_scripts(), Load all 16 API keys from JSON files.]
- "wave7_generate_phase0_scripts_fixed_load_pending_epics": "load_pending_epics()" | kind=code-symbol | source=scripts/wave7/generate_phase0_scripts_fixed.py:L18 | neighbors=[generate_phase0_scripts_fixed.py, generate_scripts(), Load pending epics from epic_roadmap_wa…]
- "wave7_generate_phase0_scripts_load_api_keys": "load_api_keys()" | kind=code-symbol | source=scripts/wave7/generate_phase0_scripts.py:L48 | neighbors=[generate_phase0_scripts.py, generate_scripts(), Load all 15 API keys from JSON files.]
- "wave7_generate_phase0_scripts_load_pending_epics": "load_pending_epics()" | kind=code-symbol | source=scripts/wave7/generate_phase0_scripts.py:L18 | neighbors=[generate_phase0_scripts.py, generate_scripts(), Load pending epics from epic_roadmap_wa…]
- "wave7_identify_phase0_complete_find_phase0_complete_epics": "find_phase0_complete_epics()" | kind=code-symbol | source=scripts/wave7/identify_phase0_complete.py:L10 | neighbors=[identify_phase0_complete.py, main(), Find all EPIC-W7-* directories with 00-…]
- "wave7_launch_epic_with_fixed_env_get_fixed_environment": "get_fixed_environment()" | kind=code-symbol | source=building-blocks/wave7/launch_epic_with_fixed_env.py:L21 | neighbors=[launch_epic_with_fixed_env.py, launch_epic(), Create a fixed environment with proper …]
- "wave7_launch_epic_with_fixed_env_launch_epic_batch": "launch_epic_batch()" | kind=code-symbol | source=building-blocks/wave7/launch_epic_with_fixed_env.py:L84 | neighbors=[launch_epic_with_fixed_env.py, launch_epic(), Launch multiple epics in parallel with …]
- "analyze_complexity_audit_main": "main()" | kind=code-symbol | source=analyze_complexity_audit.py:L41 | neighbors=[analyze_complexity_audit.py, analyze_complexity_audit()]
- "analyze_epic_status": "analyze_epic_status.py" | kind=code-symbol | source=analyze_epic_status.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "analyze_jane_street_violations": "analyze_jane_street_violations.py" | kind=code-symbol | source=analyze_jane_street_violations.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "analyze_wave4_phase5_results": "analyze_wave4_phase5_results.py" | kind=code-symbol | source=analyze_wave4_phase5_results.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…]
- "bob_mcp_mcp_server_jcodemunch_mcp": "jcodemunch-mcp" | kind=code-symbol | source=.bob/mcp.json:L1 | neighbors=[mcp.json, /home/malhitticrypto/.local/bin/jcodemu…]
- "card_board_main_ar": "Ar()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, er()]
- "card_board_main_dr": "Dr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, sr()]
- "card_board_main_hr": "hr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, Lr()]
- "card_board_main_lr": "Lr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, hr()]
- "card_board_main_or": "or()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, N()]
- "card_board_main_rt": "rt()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, r()]
- "card_board_main_sn": "sn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, Bn()]
- "card_board_main_tn": "Tn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, kn()]
- "card_board_main_vr": "Vr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, Yr()]
- "card_board_main_w": "w()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, y()]
- "check_epic_structure": "check_epic_structure.py" | kind=code-symbol | source=check_epic_structure.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "check_special_epics": "check_special_epics.py" | kind=code-symbol | source=check_special_epics.py:L1 | neighbors=[0adc411 docs: Wave 4 protocol hardening…, 7c00b3d docs: Wave 4 protocol hardening…]
- "check_wave6_phase_status": "check_wave6_phase_status.py" | kind=code-symbol | source=check_wave6_phase_status.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@09581d08f2bfc68a19e4ab1841de540cb6f2f04f": "09581d0 fix(wave7/W7-068): patch missing phase_5_v block in manifest — CYC=3 al…" | kind=Commit | source=git | neighbors=[main, 6e5a13d feat(wave7/phase6-redo): Phase …]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@0c4f389465dd58a122cdea112dcd05991e296603": "0c4f389 feat(wave7/tests-perf): Wave 7 V12 Performance test suite additions" | kind=Commit | source=git | neighbors=[wave7/tests-perf, bb9723e Remove PAT file from tracking -…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@2ed327b1a4ca0a6368f823c16ce3af3ec0f9a7cb": "2ed327b feat(s1-sima-core): Wave 7 CYC reduction — SIMA Lifecycle, SIMA Flatten" | kind=Commit | source=git | neighbors=[wave7/s1-sima-core, e01e4e5 wave7: backup all phase 0-5V wo…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@59a28d53a9e277f8779318811b829503ea8fc0a0": "59a28d5 feat(wave7/s2-trailing): CYC reduction — Trailing stops cluster" | kind=Commit | source=git | neighbors=[wave7/s2-trailing-v2, e01e4e5 wave7: backup all phase 0-5V wo…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@75eac97f8625e97c5d815c3a63000e5222888583": "75eac97 feat(wave7/s5-symmetry-orders): CYC reduction — Symmetry, Orders, SIMA …" | kind=Commit | source=git | neighbors=[wave7/s5-symmetry-orders, bb9723e Remove PAT file from tracking -…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@7b90e380d81dc0741ef6b1066b76f806548d75ed": "7b90e38 feat(wave7/s3-ui-photon): CYC reduction — UI Compliance, IPC, Panel clu…" | kind=Commit | source=git | neighbors=[wave7/s3-ui-photon-v2, e01e4e5 wave7: backup all phase 0-5V wo…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@966195dd63b1906efddaef9b7cc4a72c14e088a4": "966195d feat(wave7/s2-trailing): CYC reduction — Trailing stops cluster" | kind=Commit | source=git | neighbors=[wave7/s2-trailing, e01e4e5 wave7: backup all phase 0-5V wo…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@db74650847614bf40b3e676919b8afaecbfd0e1d": "db74650 feat(s4-reaper-safety): Wave 7 CYC reduction — REAPER Audit, REAPER Rep…" | kind=Commit | source=git | neighbors=[wave7/s4-reaper-safety, e01e4e5 wave7: backup all phase 0-5V wo…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@e44457e8c5a42e4361411152aa5f2033007f0880": "e44457e feat(wave7/s3-ui-photon): CYC reduction — UI, IPC, Panel cluster" | kind=Commit | source=git | neighbors=[bb9723e Remove PAT file from tracking -…, wave7/s3-ui-photon]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@ebb9cd39825ff1874496a07246d6526f8a19dd58": "ebb9cd3 feat(wave7/xunit-tests): Wave 7 xUnit test suites" | kind=Commit | source=git | neighbors=[bb9723e Remove PAT file from tracking -…, wave7/xunit-tests]
- "count_epics": "count_epics.py" | kind=code-symbol | source=count_epics.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "deprecated_tool_bugs_launch_phase0_fixed_launch": "launch()" | kind=code-symbol | source=scripts/wave2/_deprecated_tool_bugs/launch_phase0_fixed.py:L94 | neighbors=[launch_phase0_fixed.py, create_phase0_script_fixed()]
- "deprecated_tool_bugs_launch_phase0_v3_custom_mode_create_script": "create_script()" | kind=code-symbol | source=scripts/wave2/_deprecated_tool_bugs/launch_phase0_v3_custom_mode.py:L27 | neighbors=[launch_phase0_v3_custom_mode.py, main()]
- "deprecated_tool_bugs_launch_phase0_v3_custom_mode_load_api_key": "load_api_key()" | kind=code-symbol | source=scripts/wave2/_deprecated_tool_bugs/launch_phase0_v3_custom_mode.py:L24 | neighbors=[launch_phase0_v3_custom_mode.py, main()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-036.json

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
