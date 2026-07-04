# Node Description Batch 17 of 61

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

- "w7_025_w7_025_tests": "W7_025.Tests.csproj" | kind=code-symbol | source=xunit-tests/W7-025/W7_025.Tests.csproj:L1 | neighbors=[e01e4e5 wave7: backup all phase 0-5V wo…, net8.0, Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio, Microsoft.NET.Sdk]
- "w7_096_w7_096_tests": "W7_096.Tests.csproj" | kind=code-symbol | source=xunit-tests/W7-096/W7_096.Tests.csproj:L1 | neighbors=[e01e4e5 wave7: backup all phase 0-5V wo…, net8.0, Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio, Microsoft.NET.Sdk]
- "w7_160_w7_160_tests": "W7_160.Tests.csproj" | kind=code-symbol | source=xunit-tests/W7-160/W7_160.Tests.csproj:L1 | neighbors=[e01e4e5 wave7: backup all phase 0-5V wo…, net8.0, Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio, Microsoft.NET.Sdk]
- "wave2_check_api_balances": "check_api_balances.py" | kind=code-symbol | source=scripts/wave2/check_api_balances.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, check_balance(), main()]
- "wave2_check_phase2_status": "check_phase2_status.py" | kind=code-symbol | source=scripts/wave2/check_phase2_status.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, get_epic_status(), main()]
- "wave2_generate_phase1_scripts": "generate_phase1_scripts.py" | kind=code-symbol | source=scripts/wave2/generate_phase1_scripts.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, generate_scripts(), load_api_key()]
- "wave2_launch_wave_v3_multi_api_main": "main()" | kind=code-symbol | source=scripts/wave2/launch_wave_v3_multi_api.py:L179 | neighbors=[launch_wave_v3_multi_api.py, build_wave_script(), clear_stale_ssh_key(), gcloud(), load_api_keys(), load_epics()]
- "wave2_launch_wave_v4_safe_budget_main": "main()" | kind=code-symbol | source=scripts/wave2/launch_wave_v4_safe_budget.py:L202 | neighbors=[launch_wave_v4_safe_budget.py, build_wave_script(), clear_stale_ssh_key(), gcloud(), load_api_keys(), load_epics()]
- "wave2_remove_gates_final": "remove_gates_final.py" | kind=code-symbol | source=scripts/wave2/remove_gates_final.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, main(), remove_gate_section()]
- "wave2_reset_phase4_manifests": "reset_phase4_manifests.py" | kind=code-symbol | source=scripts/wave2/reset_phase4_manifests.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, main(), reset_phase4()]
- "wave2_wait_for_phase4": "wait_for_phase4.py" | kind=code-symbol | source=scripts/wave2/wait_for_phase4.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, check_completion(), main()]
- "wave4_execute_phase0_with_jane_street": "execute_phase0_with_jane_street.py" | kind=code-symbol | source=scripts/wave4/execute_phase0_with_jane_street.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…, execute_phase_0(), # TODO: Fetch jCodemunch data]
- "analyze_wave7_phase0_complete_main": "main()" | kind=code-symbol | source=analyze_wave7_phase0_complete.py:L76 | neighbors=[analyze_wave7_phase0_complete.py, check_process_running(), extract_method_from_hotspots(), get_epic_files(), get_manifest_timestamp()]
- "card_board_main_bn": "Bn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, Un(), Ln(), sn(), v()]
- "card_board_main_co": "Co()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, c(), Fo(), ko(), To()]
- "card_board_main_en": "En()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, ir(), jn(), On(), ot()]
- "card_board_main_gn": "gn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, Ln(), pn(), s(), t()]
- "card_board_main_ko": "ko()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, Co(), p(), xo(), yo()]
- "card_board_main_mr": "Mr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, fr(), h(), m(), qr()]
- "card_board_main_on": "On()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, An(), En(), Ln(), r()]
- "card_board_main_ot": "ot()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, An(), En(), r(), t()]
- "card_board_main_rr": "rr()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, f(), Br(), N(), Ur()]
- "card_board_main_xo": "xo()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, ko(), To(), c(), Eo()]
- "cleanup_and_relaunch_wave7": "cleanup_and_relaunch_wave7.py" | kind=code-symbol | source=cleanup_and_relaunch_wave7.py:L1 | neighbors=[extract_method_from_hotspots(), get_wave7_methods(), main(), 180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@299811db87d4805a6c3ca7cd6dc41d3b6b79d447": "299811d docs(protocol): Filtered consolidation plan - 64 branches with unique c…" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, ae6e384 Merge branch 'main' into gitbut…, df27b2e [INFRA] Tier 6 consolidation - …, 62681f8 docs(protocol): Chronological c…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@4427b8b17ceeed75614d87346bca119814ee9228": "4427b8b [DOCS] EPIC-CCN-51 planning artifacts + fix EPIC-52 mapping error (P0 c…" | kind=Commit | source=git | neighbors=[gitbutler/workspace, 09cd997 docs(protocol): Workspace conso…, 512aa0b [SRC] Restore REAPER infrastruc…, 803704c Merge remote-tracking branch 'o…, 825da97 [INFRA] Phase 0 + Phase 1: Epic…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@825da974717517ef35e3b97413182e505cff6aeb": "825da97 [INFRA] Phase 0 + Phase 1: Epic CCN-51 analysis and intake" | kind=Commit | source=git | neighbors=[gitbutler/workspace, 11b9514 [SRC] Restore REAPER infrastruc…, 16a25a6 [SRC] Fix 42 compilation errors…, 4427b8b [DOCS] EPIC-CCN-51 planning art…, 52487a6 [INFRA] Fix 42 pre-existing com…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@a3ae570461483807ab2e669e03475d461efc83c0": "a3ae570 GitButler Workspace Commit" | kind=Commit | source=git | neighbors=[gitbutler/workspace, ffe73a8 Merge branch 'build/1105-monoli…, after_task_complete.py, before_new_task.py, ba10284 docs(protocol): Branch categori…]
- "eval_viewer_generate_review_main": "main()" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L387 | neighbors=[generate_review.py, find_runs(), generate_html(), _kill_port(), load_previous_iteration()]
- "generate_missing_phase0_scripts_main": "main()" | kind=code-symbol | source=generate_missing_phase0_scripts.py:L55 | neighbors=[generate_missing_phase0_scripts.py, extract_epic_number(), generate_phase0_script(), load_roadmap(), Generate all missing Phase 0 scripts.]
- "hooks_after_task_complete_main": "main()" | kind=code-symbol | source=.bob/hooks/after_task_complete.py:L108 | neighbors=[after_task_complete.py, generate_commit_message(), get_current_branch(), run_command(), Main hook entry point.]
- "hooks_after_task_run_command": "run_command()" | kind=code-symbol | source=.bob/hooks/after_task.py:L22 | neighbors=[after_task.py, auto_commit(), get_changed_files(), main(), Execute shell command and return output.]
- "hooks_before_new_task_main": "main()" | kind=code-symbol | source=.bob/hooks/before_new_task.py:L78 | neighbors=[before_new_task.py, detect_task_tier(), run_command(), sanitize_branch_name(), Main hook entry point.]
- "hooks_pre_task_jane_street_kb_main": "main()" | kind=code-symbol | source=.bob/hooks/pre_task_jane_street_kb.py:L132 | neighbors=[pre_task_jane_street_kb.py, extract_topics(), format_kb_results(), query_jane_street_kb(), should_trigger()]
- "r28_mmiospscring_r28_mmiospscring": "R28_MmioSpscRing.csproj" | kind=code-symbol | source=sandbox/R28_MmioSpscRing/R28_MmioSpscRing.csproj:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, net48, System.Runtime.CompilerServices.Unsafe, Microsoft.NET.Sdk]
- "scripts_aggregate_benchmark_generate_benchmark": "generate_benchmark()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/aggregate_benchmark.py:L227 | neighbors=[aggregate_benchmark.py, aggregate_results(), load_run_results(), main(), Generate complete benchmark.json from r…]
- "scripts_amal_harness_main": "main()" | kind=code-symbol | source=scripts/amal_harness.py:L260 | neighbors=[amal_harness.py, extract_all_literals(), extract_named_ts_exports(), get_method_body(), inject_and_benchmark()]
- "scripts_amal_harness_v26": "amal_harness_v26.py" | kind=code-symbol | source=scripts/amal_harness_v26.py:L1 | neighbors=[extract_all_classes(), main(), run_benchmark(), langsmith_bridge.py, V26 MPMC AMAL Vetting Gate Extracts the…]
- "scripts_analyze_roadmap": "analyze_roadmap.py" | kind=code-symbol | source=scripts/analyze_roadmap.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, analyze_roadmap()]
- "scripts_analyze_wave4_completion": "analyze_wave4_completion.py" | kind=code-symbol | source=scripts/analyze_wave4_completion.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…, analyze_wave4_status()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-016.json

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
