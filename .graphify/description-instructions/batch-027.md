# Node Description Batch 28 of 61

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

- "hooks_after_epic_failure_capture_lesson_to_firebase": "capture_lesson_to_firebase()" | kind=code-symbol | source=.bob/hooks/after_epic_failure.py:L86 | neighbors=[after_epic_failure.py, main(), Capture lesson to Firebase using existi…]
- "hooks_after_epic_failure_extract_lesson_from_forensic_report": "extract_lesson_from_forensic_report()" | kind=code-symbol | source=.bob/hooks/after_epic_failure.py:L22 | neighbors=[after_epic_failure.py, main(), Extract lesson from forensic report.  …]
- "hooks_after_epic_failure_update_session_json": "update_session_json()" | kind=code-symbol | source=.bob/hooks/after_epic_failure.py:L115 | neighbors=[after_epic_failure.py, main(), Update autonomous_refactor_session.json…]
- "hooks_after_subagent_batch_get_lamport_clock": "get_lamport_clock()" | kind=code-symbol | source=.bob/hooks/after_subagent_batch.py:L49 | neighbors=[after_subagent_batch.py, log_lamport(), Read current max Lamport clock from eve…]
- "hooks_after_task_categorize_files": "categorize_files()" | kind=code-symbol | source=.bob/hooks/after_task.py:L57 | neighbors=[after_task.py, auto_commit(), Categorize files into .cs and non-.cs.]
- "hooks_after_task_complete_get_build_tag": "get_build_tag()" | kind=code-symbol | source=.bob/hooks/after_task_complete.py:L32 | neighbors=[after_task_complete.py, generate_commit_message(), Extract BUILD_TAG from src/V12_002.cs i…]
- "hooks_after_task_generate_commit_message": "generate_commit_message()" | kind=code-symbol | source=.bob/hooks/after_task.py:L71 | neighbors=[after_task.py, auto_commit(), Generate V12-compliant commit message.]
- "hooks_after_task_main": "main()" | kind=code-symbol | source=.bob/hooks/after_task.py:L128 | neighbors=[after_task.py, auto_commit(), run_command()]
- "hooks_before_new_task_detect_task_tier": "detect_task_tier()" | kind=code-symbol | source=.bob/hooks/before_new_task.py:L31 | neighbors=[before_new_task.py, main(), Detect which tier the task belongs to b…]
- "hooks_before_new_task_run_command": "run_command()" | kind=code-symbol | source=.bob/hooks/before_new_task.py:L62 | neighbors=[before_new_task.py, main(), Run shell command and return (exit_code…]
- "hooks_before_new_task_sanitize_branch_name": "sanitize_branch_name()" | kind=code-symbol | source=.bob/hooks/before_new_task.py:L19 | neighbors=[before_new_task.py, main(), Convert task description to valid git b…]
- "hooks_pre_session_generate_jane_street_rules": "generate_jane_street_rules()" | kind=code-symbol | source=.bob/hooks/pre_session.py:L28 | neighbors=[pre_session.py, main(), Generate mandatory rules file from Jane…]
- "hooks_pre_session_main": "main()" | kind=code-symbol | source=.bob/hooks/pre_session.py:L78 | neighbors=[pre_session.py, generate_jane_street_rules(), Load bootstrap context for Bob CLI sess…]
- "hooks_pre_task_jane_street_kb_extract_topics": "extract_topics()" | kind=code-symbol | source=.bob/hooks/pre_task_jane_street_kb.py:L76 | neighbors=[pre_task_jane_street_kb.py, main(), Extract relevant topics from task descr…]
- "hooks_pre_task_jane_street_kb_format_kb_results": "format_kb_results()" | kind=code-symbol | source=.bob/hooks/pre_task_jane_street_kb.py:L112 | neighbors=[pre_task_jane_street_kb.py, main(), Format KB results for Bob's context]
- "hooks_pre_task_jane_street_kb_query_jane_street_kb": "query_jane_street_kb()" | kind=code-symbol | source=.bob/hooks/pre_task_jane_street_kb.py:L91 | neighbors=[pre_task_jane_street_kb.py, main(), Query Jane Street Knowledge Base]
- "hooks_pre_task_jane_street_kb_should_trigger": "should_trigger()" | kind=code-symbol | source=.bob/hooks/pre_task_jane_street_kb.py:L66 | neighbors=[pre_task_jane_street_kb.py, main(), Check if task should trigger Jane Stree…]
- "identify_wave7_directories_extract_method_from_epic_dir": "extract_method_from_epic_dir()" | kind=code-symbol | source=identify_wave7_directories.py:L11 | neighbors=[identify_wave7_directories.py, main(), Extract method name from epic directory…]
- "identify_wave7_directories_v2_extract_method_from_epic_dir": "extract_method_from_epic_dir()" | kind=code-symbol | source=identify_wave7_directories_v2.py:L12 | neighbors=[identify_wave7_directories_v2.py, main(), Extract method name from epic directory…]
- "launch_wave7_python": "launch_wave7_python.py" | kind=code-symbol | source=launch_wave7_python.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, main()]
- "package_wave7_for_local": "package_wave7_for_local.py" | kind=code-symbol | source=package_wave7_for_local.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, main()]
- "relaunch_final_5_with_path_fix_launch_epic_with_fixed_path": "launch_epic_with_fixed_path()" | kind=code-symbol | source=relaunch_final_5_with_path_fix.py:L19 | neighbors=[relaunch_final_5_with_path_fix.py, main(), Launch epic with explicitly fixed PATH …]
- "scripts_agent_bootstrap_agentbootstraploader_extract_component_name": "._extract_component_name()" | kind=code-symbol | source=scripts/agent_bootstrap.py:L275 | neighbors=[AgentBootstrapLoader, ._load_jane_street_kb(), Extract component name from file path.]
- "scripts_agent_bootstrap_agentbootstraploader_extract_relevant_nodes": "._extract_relevant_nodes()" | kind=code-symbol | source=scripts/agent_bootstrap.py:L290 | neighbors=[AgentBootstrapLoader, ._load_graphify_graph(), Extract relevant nodes from Graphify gr…]
- "scripts_agent_bootstrap_agentbootstraploader_generate_summary": "._generate_summary()" | kind=code-symbol | source=scripts/agent_bootstrap.py:L307 | neighbors=[AgentBootstrapLoader, .load_all(), Generate markdown summary of loaded con…]
- "scripts_agent_bootstrap_agentbootstraploader_load_compound_intelligence": "._load_compound_intelligence()" | kind=code-symbol | source=scripts/agent_bootstrap.py:L199 | neighbors=[AgentBootstrapLoader, .load_all(), Load learnings from compound intelligen…]
- "scripts_agent_bootstrap_agentbootstraploader_load_session_history": "._load_session_history()" | kind=code-symbol | source=scripts/agent_bootstrap.py:L237 | neighbors=[AgentBootstrapLoader, .load_all(), Load previous session history for this …]
- "scripts_aggregate_benchmark_calculate_stats": "calculate_stats()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/aggregate_benchmark.py:L45 | neighbors=[aggregate_benchmark.py, aggregate_results(), Calculate mean, stddev, min, max for a …]
- "scripts_aggregate_benchmark_generate_markdown": "generate_markdown()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/aggregate_benchmark.py:L281 | neighbors=[aggregate_benchmark.py, main(), Generate human-readable benchmark.md fr…]
- "scripts_aggregate_benchmark_load_run_results": "load_run_results()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/aggregate_benchmark.py:L67 | neighbors=[aggregate_benchmark.py, generate_benchmark(), Load all run results from a benchmark d…]
- "scripts_aggregate_benchmark_main": "main()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/aggregate_benchmark.py:L338 | neighbors=[aggregate_benchmark.py, generate_benchmark(), generate_markdown()]
- "scripts_amal_harness_cleanup_orphaned_blocks": "cleanup_orphaned_blocks()" | kind=code-symbol | source=scripts/amal_harness.py:L198 | neighbors=[amal_harness.py, inject_and_benchmark(), r"""Remove `{ ... }` blocks whose openi…]
- "scripts_amal_harness_v25_extract_all_classes": "extract_all_classes()" | kind=code-symbol | source=scripts/amal_harness_v25.py:L15 | neighbors=[amal_harness_v25.py, main(), Extract all classes, structs, enums, et…]
- "scripts_amal_harness_v25_main": "main()" | kind=code-symbol | source=scripts/amal_harness_v25.py:L114 | neighbors=[amal_harness_v25.py, extract_all_classes(), run_benchmark()]
- "scripts_amal_harness_v25_run_benchmark": "run_benchmark()" | kind=code-symbol | source=scripts/amal_harness_v25.py:L83 | neighbors=[amal_harness_v25.py, main(), Inject class body into V25 template and…]
- "scripts_amal_harness_v26_extract_all_classes": "extract_all_classes()" | kind=code-symbol | source=scripts/amal_harness_v26.py:L24 | neighbors=[amal_harness_v26.py, main(), Extract all C# classes, structs, enums,…]
- "scripts_amal_harness_v26_main": "main()" | kind=code-symbol | source=scripts/amal_harness_v26.py:L136 | neighbors=[amal_harness_v26.py, extract_all_classes(), run_benchmark()]
- "scripts_amal_harness_v26_run_benchmark": "run_benchmark()" | kind=code-symbol | source=scripts/amal_harness_v26.py:L94 | neighbors=[amal_harness_v26.py, main(), Inject class body into the template and…]
- "scripts_analyze_wave4_pr_clusters_get_commit_stats": "get_commit_stats()" | kind=code-symbol | source=scripts/analyze_wave4_pr_clusters.py:L56 | neighbors=[analyze_wave4_pr_clusters.py, main(), Get file-level stats for a commit.]
- "scripts_analyze_wave4_pr_clusters_main": "main()" | kind=code-symbol | source=scripts/analyze_wave4_pr_clusters.py:L114 | neighbors=[analyze_wave4_pr_clusters.py, get_commit_stats(), map_files_to_subsystems()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-027.json

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
