# Node Description Batch 21 of 61

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

- "eval_viewer_generate_review_load_previous_iteration": "load_previous_iteration()" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L213 | neighbors=[generate_review.py, find_runs(), main(), Load previous iteration's feedback and …]
- "fix_epic_111": "fix_epic_111.py" | kind=code-symbol | source=fix_epic_111.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…]
- "fix_phase0_scripts_paths": "fix_phase0_scripts_paths.py" | kind=code-symbol | source=fix_phase0_scripts_paths.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, fix_script(), main()]
- "generate_missing_phase0_scripts_extract_epic_number": "extract_epic_number()" | kind=code-symbol | source=generate_missing_phase0_scripts.py:L17 | neighbors=[generate_missing_phase0_scripts.py, generate_phase0_script(), main(), Extract numeric epic number from variou…]
- "generate_missing_phase0_scripts_generate_phase0_script": "generate_phase0_script()" | kind=code-symbol | source=generate_missing_phase0_scripts.py:L31 | neighbors=[generate_missing_phase0_scripts.py, extract_epic_number(), main(), Generate Phase 0 script from working te…]
- "hooks_after_epic_failure_main": "main()" | kind=code-symbol | source=.bob/hooks/after_epic_failure.py:L148 | neighbors=[after_epic_failure.py, capture_lesson_to_firebase(), extract_lesson_from_forensic_report(), update_session_json()]
- "hooks_after_subagent_batch": "after_subagent_batch.py" | kind=code-symbol | source=.bob/hooks/after_subagent_batch.py:L1 | neighbors=[b0a803b feat(wave7): Phase 2 Architectu…, get_lamport_clock(), log_lamport(), main()]
- "hooks_after_subagent_batch_log_lamport": "log_lamport()" | kind=code-symbol | source=.bob/hooks/after_subagent_batch.py:L66 | neighbors=[after_subagent_batch.py, get_lamport_clock(), main(), Append a Lamport-clocked event to the w…]
- "hooks_after_task_complete_generate_commit_message": "generate_commit_message()" | kind=code-symbol | source=.bob/hooks/after_task_complete.py:L67 | neighbors=[after_task_complete.py, get_build_tag(), main(), Generate V12-compliant commit message.…]
- "hooks_after_task_complete_get_current_branch": "get_current_branch()" | kind=code-symbol | source=.bob/hooks/after_task_complete.py:L50 | neighbors=[after_task_complete.py, run_command(), main(), Get current GitButler virtual branch na…]
- "hooks_after_task_complete_run_command": "run_command()" | kind=code-symbol | source=.bob/hooks/after_task_complete.py:L16 | neighbors=[after_task_complete.py, get_current_branch(), main(), Run shell command and return (exit_code…]
- "hooks_after_task_get_changed_files": "get_changed_files()" | kind=code-symbol | source=.bob/hooks/after_task.py:L40 | neighbors=[after_task.py, auto_commit(), run_command(), Get list of changed files in working di…]
- "hooks_pre_session": "pre_session.py" | kind=code-symbol | source=.bob/hooks/pre_session.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, generate_jane_street_rules(), main()]
- "identify_failed_epics": "identify_failed_epics.py" | kind=code-symbol | source=identify_failed_epics.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "identify_wave7_directories": "identify_wave7_directories.py" | kind=code-symbol | source=identify_wave7_directories.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, extract_method_from_epic_dir(), main()]
- "identify_wave7_directories_v2": "identify_wave7_directories_v2.py" | kind=code-symbol | source=identify_wave7_directories_v2.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, extract_method_from_epic_dir(), main()]
- "relaunch_final_5_with_path_fix": "relaunch_final_5_with_path_fix.py" | kind=code-symbol | source=relaunch_final_5_with_path_fix.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, launch_epic_with_fixed_path(), main()]
- "scripts_agent_bootstrap": "agent_bootstrap.py" | kind=code-symbol | source=scripts/agent_bootstrap.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, AgentBootstrapLoader, bootstrap_agent()]
- "scripts_agent_bootstrap_agentbootstraploader_load_graphify_graph": "._load_graphify_graph()" | kind=code-symbol | source=scripts/agent_bootstrap.py:L182 | neighbors=[AgentBootstrapLoader, .load_all(), ._extract_relevant_nodes(), Load Graphify knowledge graph.]
- "scripts_agent_bootstrap_agentbootstraploader_load_jane_street_kb": "._load_jane_street_kb()" | kind=code-symbol | source=scripts/agent_bootstrap.py:L90 | neighbors=[AgentBootstrapLoader, .load_all(), ._extract_component_name(), Load relevant Jane Street patterns from…]
- "scripts_agent_bootstrap_bootstrap_agent": "bootstrap_agent()" | kind=code-symbol | source=scripts/agent_bootstrap.py:L395 | neighbors=[agent_bootstrap.py, AgentBootstrapLoader, .load_all(), Bootstrap an agent with full context.  …]
- "scripts_aggregate_benchmark_aggregate_results": "aggregate_results()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/aggregate_benchmark.py:L176 | neighbors=[aggregate_benchmark.py, calculate_stats(), generate_benchmark(), Aggregate run results into summary stat…]
- "scripts_amal_harness_extract_all_literals": "extract_all_literals()" | kind=code-symbol | source=scripts/amal_harness.py:L80 | neighbors=[amal_harness.py, _scan_backtick_literal(), main(), Extract all bare backtick template lite…]
- "scripts_amal_harness_extract_named_ts_exports": "extract_named_ts_exports()" | kind=code-symbol | source=scripts/amal_harness.py:L63 | neighbors=[amal_harness.py, _scan_backtick_literal(), main(), Extract bodies of 'export const NAME = …]
- "scripts_amal_harness_inject_and_benchmark": "inject_and_benchmark()" | kind=code-symbol | source=scripts/amal_harness.py:L230 | neighbors=[amal_harness.py, cleanup_orphaned_blocks(), normalize_body(), main()]
- "scripts_amal_harness_scan_backtick_literal": "_scan_backtick_literal()" | kind=code-symbol | source=scripts/amal_harness.py:L48 | neighbors=[amal_harness.py, extract_all_literals(), extract_named_ts_exports(), Scan from 'start' (after opening backti…]
- "scripts_amal_harness_v25": "amal_harness_v25.py" | kind=code-symbol | source=scripts/amal_harness_v25.py:L1 | neighbors=[extract_all_classes(), main(), run_benchmark(), V25 MPMC AMAL Vetting Gate Extracts the…]
- "scripts_analyze_epic_roadmap": "analyze_epic_roadmap.py" | kind=code-symbol | source=scripts/analyze_epic_roadmap.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "scripts_analyze_jane_street_violations": "analyze_jane_street_violations.py" | kind=code-symbol | source=scripts/analyze_jane_street_violations.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "scripts_analyze_wave7_special_cases_main": "main()" | kind=code-symbol | source=scripts/analyze_wave7_special_cases.py:L205 | neighbors=[analyze_wave7_special_cases.py, analyze_special_cases(), generate_report(), load_roadmap()]
- "scripts_check_complete_epics": "check_complete_epics.py" | kind=code-symbol | source=scripts/check_complete_epics.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…]
- "scripts_check_completed_epics_in_workers": "check_completed_epics_in_workers.py" | kind=code-symbol | source=scripts/check_completed_epics_in_workers.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…]
- "scripts_check_epic_status": "check_epic_status.py" | kind=code-symbol | source=scripts/check_epic_status.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…]
- "scripts_check_phase1_outputs": "check_phase1_outputs.py" | kind=code-symbol | source=scripts/check_phase1_outputs.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, check_manifest(), main()]
- "scripts_check_wave1_targets": "check_wave1_targets.py" | kind=code-symbol | source=scripts/check_wave1_targets.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…]
- "scripts_cleanup_stale_phase_starts": "cleanup_stale_phase_starts.py" | kind=code-symbol | source=scripts/cleanup_stale_phase_starts.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, cleanup_event_log(), main()]
- "scripts_clear_lamport_conflicts": "clear_lamport_conflicts.py" | kind=code-symbol | source=scripts/clear_lamport_conflicts.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, clear_lamport_conflict(), main()]
- "scripts_context7_cli_call_context7_mcp": "call_context7_mcp()" | kind=code-symbol | source=scripts/context7_cli.py:L13 | neighbors=[context7_cli.py, get_api_key(), main(), Simulates a JSON-RPC call to the Contex…]
- "scripts_continue_session_get_minimal_context": "get_minimal_context()" | kind=code-symbol | source=scripts/continue_session.py:L180 | neighbors=[continue_session.py, load_state(), main(), Generate minimal context block for next…]
- "scripts_continue_session_show_status": "show_status()" | kind=code-symbol | source=scripts/continue_session.py:L223 | neighbors=[continue_session.py, main(), Display current session status., load_state()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-020.json

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
