# Node Description Batch 14 of 61

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

- "scripts_validate_phase_compliance_phasevalidator_validate": ".validate()" | kind=code-symbol | source=scripts/validate_phase_compliance.py:L97 | neighbors=[PhaseValidator, ._check_custom_mode_mentioned(), ._check_lamport_event(), ._check_manifest_updated(), ._check_mcp_usage(), ._check_output_files()]
- "scripts_verify_wave7_determinism": "verify_wave7_determinism.py" | kind=code-symbol | source=scripts/verify_wave7_determinism.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, lamport_clock.py, get_wave7_epics(), main(), print_results()]
- "scripts_wave2_simple_orchestrator": "wave2_simple_orchestrator.py" | kind=code-symbol | source=scripts/wave2_simple_orchestrator.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, execute_phase_0_batch(), execute_phase_1_batch()]
- "wave2_launch_phase0_v4_shell_commands": "launch_phase0_v4_shell_commands.py" | kind=code-symbol | source=scripts/wave2/launch_phase0_v4_shell_commands.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, create_script(), load_api_key()]
- "wave2_monitor_phase4": "monitor_phase4.py" | kind=code-symbol | source=scripts/wave2/monitor_phase4.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, check_log_completion(), check_screen_sessions()]
- "wave3_generate_wave3_phase0_scripts": "generate_wave3_phase0_scripts.py" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase0_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 3bcfaac feat: Institutionalize building…, a24ed35 feat: Institutionalize building…, be6c8a1 docs: Wave 4 documentation merg…, generate_launcher_script(), generate_phase0_script()]
- "wave3_generate_wave3_phase3_scripts_corrected": "generate_wave3_phase3_scripts_CORRECTED.py" | kind=code-symbol | source=scripts/wave3/generate_wave3_phase3_scripts_CORRECTED.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 3bcfaac feat: Institutionalize building…, a24ed35 feat: Institutionalize building…, be6c8a1 docs: Wave 4 documentation merg…, generate_launcher(), generate_phase3_script()]
- "analyze_wave7_phase0_complete": "analyze_wave7_phase0_complete.py" | kind=code-symbol | source=analyze_wave7_phase0_complete.py:L1 | neighbors=[check_process_running(), extract_method_from_hotspots(), get_epic_files(), get_manifest_timestamp(), main(), 180215d Wave 7 Phase 1 100% complete - …]
- "benchmarks_v12_performance_benchmarks": "V12_Performance.Benchmarks.csproj" | kind=code-symbol | source=benchmarks/V12_Performance.Benchmarks.csproj:L1 | neighbors=[net6.0, BenchmarkDotNet, BenchmarkDotNet.Diagnostics.Windows, Microsoft.NET.Sdk, V12_Performance.Tests.csproj, 7a0625a Merge origin/main into workspac…]
- "card_board_main_ir": "ir()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, a(), e(), En(), N(), r()]
- "card_board_main_l": "l()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, a(), e(), f(), r(), t()]
- "card_board_main_o": "o()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, a(), i(), m(), r(), pn()]
- "card_board_main_pn": "pn()" | kind=code-symbol | source=docs/brain/.obsidian/plugins/card-board/main.js:L1 | neighbors=[main.js, gn(), b(), h(), o(), s()]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@123bb23f9b5aeb9a7235d6069f4fa55cd7c0274a": "123bb23 docs(wave7): Add Phase 0 final status report (138/161 complete)" | kind=Commit | source=git | neighbors=[wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests, ce4db7a fix(wave7): Update generator to…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@330e9042b5f5a42dbecd3bef5eea7db9bf7b2c1d": "330e904 fix: Update pilot script to use correct script naming pattern (_p0_XXX.…" | kind=Commit | source=git | neighbors=[wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests, 912cc53 Wave 7: Remove exhausted API ke…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@3b7f4443342a1771c907b9190e5058129cb37d41": "3b7f444 Add git hooks installation script for VM" | kind=Commit | source=git | neighbors=[wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests, b3f7b4c Wave 7: Preserve 16 completed e…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@8c6e01f6b36c0f65e89800c767142d7d17f4a766": "8c6e01f Wave 7 Phase 1.5: Boundary validation complete (161/161 epics)" | kind=Commit | source=git | neighbors=[wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests, 180215d Wave 7 Phase 1 100% complete - …]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@912cc53b11df5614b73832d8605556f7aadd85dd": "912cc53 Wave 7: Remove exhausted API keys (4 keys) and add recovery documentati…" | kind=Commit | source=git | neighbors=[330e904 fix: Update pilot script to use…, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@98fcd734ac95e101a7c288f49050320b1a995d33": "98fcd73 feat: Add launch script for remaining 144 epics with 16 fresh API keys" | kind=Commit | source=git | neighbors=[wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests, 8c6e01f Wave 7 Phase 1.5: Boundary vali…]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@a48bfb464508178b914c024130c4601d3060eb03": "a48bfb4 docs(wave7): Add Screen Session Script Protocol and syntax validation" | kind=Commit | source=git | neighbors=[57d3230 fix(wave7): Add heredoc-free sc…, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@b3f7b4c51657e358d6aac02eef604c81bb4d6440": "b3f7b4c Wave 7: Preserve 16 completed epics from VM (Phase 0)" | kind=Commit | source=git | neighbors=[3b7f444 Add git hooks installation scri…, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@c001366f51730fef4c5757722c0716be8853942c": "c001366 Wave 7: Add pilot and full launch scripts with data loss prevention pro…" | kind=Commit | source=git | neighbors=[b3f7b4c Wave 7: Preserve 16 completed e…, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@c112140b0aff73bc3efe99c8d12163bb9c4204c9": "c112140 docs(wave7): Add Phase 0 completion verification infrastructure" | kind=Commit | source=git | neighbors=[a48bfb4 docs(wave7): Add Screen Session…, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "commit:repo:github.com/antigravityos187-sketch/universal-or-strategy@d9ed6942f732606fcda134064595f8c9f0fe5653": "d9ed694 [DOCS] PR #5 post-merge cleanup - add new docs, remove stale PR analysi…" | kind=Commit | source=git | neighbors=[gitbutler/workspace, wave7/s2-execution-trailing-symmetry, wave7/s3-ui-photon, wave7/s5-symmetry-orders, wave7/tests-perf, wave7/xunit-tests]
- "complete_wave_cross_reference_main": "main()" | kind=code-symbol | source=complete_wave_cross_reference.py:L328 | neighbors=[complete_wave_cross_reference.py, analyze_jane_street_violations(), analyze_wave6_epics(), cross_reference_jane_street(), extract_baseline_methods(), generate_report()]
- "deprecated_tool_bugs_launch_phase0_v3_custom_mode": "launch_phase0_v3_custom_mode.py" | kind=code-symbol | source=scripts/wave2/_deprecated_tool_bugs/launch_phase0_v3_custom_mode.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, create_script(), load_api_key()]
- "eval_viewer_generate_review_reviewhandler": "ReviewHandler" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L308 | neighbors=[generate_review.py, Serves the review HTML and handles feed…, BaseHTTPRequestHandler, .do_GET(), .do_POST(), .__init__()]
- "hooks_after_task_auto_commit": "auto_commit()" | kind=code-symbol | source=.bob/hooks/after_task.py:L85 | neighbors=[after_task.py, categorize_files(), generate_commit_message(), get_changed_files(), run_command(), main()]
- "nuget_microsoft_net_test_sdk": "Microsoft.NET.Test.Sdk" | kind=code-symbol | source=xunit-tests/W7-160/W7_160.Tests.csproj | neighbors=[Testing.csproj, V12_Performance.Tests.csproj, W7_007.Tests.csproj, W7_024.Tests.csproj, W7_025.Tests.csproj, W7_096.Tests.csproj]
- "scripts_analyze_wave4_pr_clusters": "analyze_wave4_pr_clusters.py" | kind=code-symbol | source=scripts/analyze_wave4_pr_clusters.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 3c2723d docs: Wave 4 PR cluster analysi…, 4458f94 docs: Wave 4 PR cluster analysi…, be6c8a1 docs: Wave 4 documentation merg…, get_commit_stats(), main()]
- "scripts_analyze_wave7_special_cases": "analyze_wave7_special_cases.py" | kind=code-symbol | source=scripts/analyze_wave7_special_cases.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, analyze_special_cases(), generate_report(), load_complexity_audit(), load_roadmap()]
- "scripts_epic_manifest_validate_phase_id": "_validate_phase_id()" | kind=code-symbol | source=scripts/epic_manifest.py:L149 | neighbors=[epic_manifest.py, load_manifest(), Validate phase ID format, update_manifest(), validate_dependencies(), ValidationError]
- "scripts_generate_wave7_stats": "generate_wave7_stats.py" | kind=code-symbol | source=scripts/generate_wave7_stats.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, compute_statistics(), load_wave7_events(), main(), print_summary()]
- "scripts_improve_description": "improve_description.py" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/improve_description.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, _call_claude(), improve_description(), main(), utils.py]
- "scripts_jane_street_utils_janestreetviolation": "JaneStreetViolation" | kind=code-symbol | source=scripts/jane_street_utils.py:L30 | neighbors=[jane_street_utils.py, .in_range(), .__init__(), .__repr__(), .to_dict(), load_violations_file()]
- "scripts_jane_street_utils_load_violations_file": "load_violations_file()" | kind=code-symbol | source=scripts/jane_street_utils.py:L67 | neighbors=[jane_street_utils.py, get_files_with_violations(), JaneStreetViolation, load_violations_for_file(), load_violations_for_files(), main()]
- "scripts_jcodemunch_hook": "jcodemunch_hook.py" | kind=code-symbol | source=scripts/jcodemunch_hook.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, JCodemunchHook, main()]
- "scripts_jcodemunch_hook_main": "main()" | kind=code-symbol | source=scripts/jcodemunch_hook.py:L203 | neighbors=[jcodemunch_hook.py, JCodemunchHook, .index_file(), .index_folder(), .register_edit(), .update_from_commit()]
- "scripts_lamport_clock_deterministicworkflow_check_dependencies": ".check_dependencies()" | kind=code-symbol | source=scripts/lamport_clock.py:L229 | neighbors=[DeterministicWorkflow, .get_event_log(), ._load_manifest_events(), .get_next_phases(), .verify_determinism(), Check if all dependencies for a phase a…]
- "scripts_lamport_clock_get_workflow": "get_workflow()" | kind=code-symbol | source=scripts/lamport_clock.py:L394 | neighbors=[lamport_clock.py, DeterministicWorkflow, Get or create global workflow instance., record_phase_complete(), record_phase_fail(), record_phase_start()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-013.json

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
