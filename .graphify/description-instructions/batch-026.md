# Node Description Batch 27 of 61

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

- "card_board_main_y": "y()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, w(), r()]
- "card_board_main_yo": "yo()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, c(), ko()]
- "card_board_main_yr": "Yr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, jr(), Vr()]
- "card_board_main_zr": "zr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, qr(), Ur()]
- "cleanup_and_relaunch_wave7_extract_method_from_hotspots": "extract_method_from_hotspots()" | kind=code-symbol | source=cleanup_and_relaunch_wave7.py:L30 | neighbors=[cleanup_and_relaunch_wave7.py, main(), Extract method name from 00-hotspots.md.]
- "cleanup_and_relaunch_wave7_get_wave7_methods": "get_wave7_methods()" | kind=code-symbol | source=cleanup_and_relaunch_wave7.py:L17 | neighbors=[cleanup_and_relaunch_wave7.py, main(), Load Wave 7 roadmap and extract methods.]
- "cleanup_and_relaunch_wave7_main": "main()" | kind=code-symbol | source=cleanup_and_relaunch_wave7.py:L53 | neighbors=[cleanup_and_relaunch_wave7.py, extract_method_from_hotspots(), get_wave7_methods()]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@088e4b773dfb6d00ad45a1697ecbabe1e4c9c4d1": "088e4b7 feat(wave7/phase5-redo): CYC reductions implemented + verified — 9 epics" | kind=Commit | source=git | neighbors=[main, f618d4f docs(wave7/phase5-redo): add mi…, 4ed2634 feat(wave7): Wave 7 COMPLETE — …]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@09cd997bb3e42829ca9746e11a404f307b2b8f51": "09cd997 docs(protocol): Workspace consolidation status - infrastructure merge c…" | kind=Commit | source=git | neighbors=[gitbutler/workspace, 62681f8 docs(protocol): Chronological c…, 4427b8b [DOCS] EPIC-CCN-51 planning art…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@11b951415fa04a73ca3694402eee1d2873dc7478": "11b9514 [SRC] Restore REAPER infrastructure declarations - fix 42 compilation e…" | kind=Commit | source=git | neighbors=[gitbutler/workspace, 59e7f83 [STYLE] Fix dense one-liner - c…, 825da97 [INFRA] Phase 0 + Phase 1: Epic…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@16a25a6d67c70da0b3f1c6e89a67490b15b16b00": "16a25a6 [SRC] Fix 42 compilation errors - add missing REAPER field declarations" | kind=Commit | source=git | neighbors=[gitbutler/workspace, 6c6231a Merge branch 'feature/src-fix-c…, 825da97 [INFRA] Phase 0 + Phase 1: Epic…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@4df9e5afdf6ed144bd36fe94194116d20f266a75": "4df9e5a fix(wave7/W7-016): TryHandleFleet_CancelAll verification artifacts — CY…" | kind=Commit | source=git | neighbors=[main, 6e5a13d feat(wave7/phase6-redo): Phase …, f618d4f docs(wave7/phase5-redo): add mi…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@4ed26340a608715aa1ca8b11e10c847d0a3a95c4": "4ed2634 feat(wave7): Wave 7 COMPLETE — all 161 epics docs, brain files, lamport…" | kind=Commit | source=git | neighbors=[main, 088e4b7 feat(wave7/phase5-redo): CYC re…, e01e4e5 wave7: backup all phase 0-5V wo…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@500c4a979ea0b57b11d218c68436750775f6634d": "500c4a9 docs(protocol): GitButler integration handoff document" | kind=Commit | source=git | neighbors=[3cc6748 feat(protocol): GitButler integ…, gitbutler/workspace, ba10284 docs(protocol): Branch categori…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@512aa0bf5c1b3a561b5c4fe296fed4f62879c7a6": "512aa0b [SRC] Restore REAPER infrastructure declarations - fix 42 compilation e…" | kind=Commit | source=git | neighbors=[4427b8b [DOCS] EPIC-CCN-51 planning art…, gitbutler/workspace, e94854f [STYLE] Fix dense one-liner - c…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@52487a629454c2ce5b09d446c55d7508bb099d25": "52487a6 [INFRA] Fix 42 pre-existing compilation errors - duplicate fields + mis…" | kind=Commit | source=git | neighbors=[gitbutler/workspace, b2cd318 Merge branch 'feature/infra-fix…, 825da97 [INFRA] Phase 0 + Phase 1: Epic…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@59e7f8369f3ac8c9474dc92d2fa65956da81b544": "59e7f83 [STYLE] Fix dense one-liner - convert GetPhotonDispatchRingDepth to blo…" | kind=Commit | source=git | neighbors=[11b9514 [SRC] Restore REAPER infrastruc…, gitbutler/workspace, 037ef9c Merge branch 'feature/src-epic-…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@62681f8b4afc1f81e136391e110900c03c128a32": "62681f8 docs(protocol): Chronological consolidation strategy - merge newer bran…" | kind=Commit | source=git | neighbors=[09cd997 docs(protocol): Workspace conso…, gitbutler/workspace, 299811d docs(protocol): Filtered consol…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@6e5a13dc8d99edc80d25b63c047e20c852c83a09": "6e5a13d feat(wave7/phase6-redo): Phase 6 final review complete — 9 epics signed…" | kind=Commit | source=git | neighbors=[4df9e5a fix(wave7/W7-016): TryHandleFle…, main, 09581d0 fix(wave7/W7-068): patch missin…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@ba10284365c9a6549477dbe42bfb6826f2d85e2b": "ba10284 docs(protocol): Branch categorization analysis - 38 branches organized …" | kind=Commit | source=git | neighbors=[500c4a9 docs(protocol): GitButler integ…, gitbutler/workspace, a3ae570 GitButler Workspace Commit]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@e94854f6d06cdb2581ea02379dd907f5cda9fd3d": "e94854f [STYLE] Fix dense one-liner - convert GetPhotonDispatchRingDepth to blo…" | kind=Commit | source=git | neighbors=[512aa0b [SRC] Restore REAPER infrastruc…, gitbutler/workspace, 037ef9c Merge branch 'feature/src-epic-…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@f618d4fa58beaaa6230efe61beffa10d94a3d5d0": "f618d4f docs(wave7/phase5-redo): add missing phase_5_v verification artifacts" | kind=Commit | source=git | neighbors=[088e4b7 feat(wave7/phase5-redo): CYC re…, main, 4df9e5a fix(wave7/W7-016): TryHandleFle…]
- "complete_wave_cross_reference_analyze_jane_street_violations": "analyze_jane_street_violations()" | kind=code-symbol | source=complete_wave_cross_reference.py:L144 | neighbors=[complete_wave_cross_reference.py, main(), Analyze Jane Street violations from P0 …]
- "complete_wave_cross_reference_analyze_wave6_epics": "analyze_wave6_epics()" | kind=code-symbol | source=complete_wave_cross_reference.py:L49 | neighbors=[complete_wave_cross_reference.py, main(), Analyze Wave 6 epics (EPIC-CCN-001 thro…]
- "complete_wave_cross_reference_cross_reference_jane_street": "cross_reference_jane_street()" | kind=code-symbol | source=complete_wave_cross_reference.py:L169 | neighbors=[complete_wave_cross_reference.py, main(), Cross-reference Jane Street violations …]
- "complete_wave_cross_reference_extract_baseline_methods": "extract_baseline_methods()" | kind=code-symbol | source=complete_wave_cross_reference.py:L20 | neighbors=[complete_wave_cross_reference.py, main(), Extract all 180 methods with CYC > 8 fr…]
- "complete_wave_cross_reference_generate_markdown_summary": "generate_markdown_summary()" | kind=code-symbol | source=complete_wave_cross_reference.py:L233 | neighbors=[complete_wave_cross_reference.py, generate_report(), Generate human-readable markdown summary]
- "complete_wave_cross_reference_map_baseline_to_wave6": "map_baseline_to_wave6()" | kind=code-symbol | source=complete_wave_cross_reference.py:L111 | neighbors=[complete_wave_cross_reference.py, main(), Map baseline methods to Wave 6 epics]
- "deprecated_tool_bugs_launch_phase0_fixed_create_phase0_script_fixed": "create_phase0_script_fixed()" | kind=code-symbol | source=scripts/wave2/_deprecated_tool_bugs/launch_phase0_fixed.py:L39 | neighbors=[launch_phase0_fixed.py, launch(), Generate Phase 0 script using message f…]
- "deprecated_tool_bugs_launch_phase0_v3_custom_mode_main": "main()" | kind=code-symbol | source=scripts/wave2/_deprecated_tool_bugs/launch_phase0_v3_custom_mode.py:L54 | neighbors=[launch_phase0_v3_custom_mode.py, create_script(), load_api_key()]
- "deprecated_tool_bugs_launch_wave2_phase0_with_verification_create_phase0_script_with_verification": "create_phase0_script_with_verification()" | kind=code-symbol | source=scripts/wave2/_deprecated_tool_bugs/launch_wave2_phase0_with_verification.py:L42 | neighbors=[launch_wave2_phase0_with_verification.py, launch_phase0(), Generate Phase 0 script with explicit f…]
- "deprecated_tool_bugs_launch_wave2_phase0_with_verification_launch_phase0": "launch_phase0()" | kind=code-symbol | source=scripts/wave2/_deprecated_tool_bugs/launch_wave2_phase0_with_verification.py:L119 | neighbors=[launch_wave2_phase0_with_verification.py, create_phase0_script_with_verification(), Launch Phase 0 for all 9 epics.]
- "eval_viewer_generate_review_find_runs_recursive": "_find_runs_recursive()" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L68 | neighbors=[generate_review.py, find_runs(), build_run()]
- "eval_viewer_generate_review_kill_port": "_kill_port()" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L288 | neighbors=[generate_review.py, main(), Kill any process listening on the given…]
- "eval_viewer_generate_review_reviewhandler_do_get": ".do_GET()" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L332 | neighbors=[ReviewHandler, find_runs(), generate_html()]
- "fix_epic_005_final": "fix_epic_005_final.py" | kind=code-symbol | source=fix_epic_005_final.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, main()]
- "fix_phase0_scripts_paths_fix_script": "fix_script()" | kind=code-symbol | source=fix_phase0_scripts_paths.py:L10 | neighbors=[fix_phase0_scripts_paths.py, main(), Fix PATH issues in a single script.]
- "fix_phase0_scripts_paths_main": "main()" | kind=code-symbol | source=fix_phase0_scripts_paths.py:L39 | neighbors=[fix_phase0_scripts_paths.py, fix_script(), Fix all generated Phase 0 scripts.]
- "fix_wave7_naming_convention": "fix_wave7_naming_convention.py" | kind=code-symbol | source=fix_wave7_naming_convention.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, main()]
- "framework_net6_0": "net6.0" | kind=entity | source=tests/V12_Performance.Tests/V12_Performance.Tests.csproj | neighbors=[SpscRing.Benchmarks.csproj, V12_Performance.Benchmarks.csproj, V12_Performance.Tests.csproj]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-026.json

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
