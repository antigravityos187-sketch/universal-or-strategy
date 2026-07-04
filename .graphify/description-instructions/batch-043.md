# Node Description Batch 44 of 61

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

- "hooks_after_task_rationale_72": "Generate V12-compliant commit message." | kind=entity | source=.bob/hooks/after_task.py:L72 | neighbors=[generate_commit_message()]
- "hooks_after_task_rationale_86": "Perform automatic commit of changed files." | kind=entity | source=.bob/hooks/after_task.py:L86 | neighbors=[auto_commit()]
- "hooks_before_new_task_rationale_20": "Convert task description to valid git branch name." | kind=entity | source=.bob/hooks/before_new_task.py:L20 | neighbors=[sanitize_branch_name()]
- "hooks_before_new_task_rationale_32": "Detect which tier the task belongs to based on keywords.\r     \r     Returns: 'sr" | kind=entity | source=.bob/hooks/before_new_task.py:L32 | neighbors=[detect_task_tier()]
- "hooks_before_new_task_rationale_63": "Run shell command and return (exit_code, stdout, stderr)." | kind=entity | source=.bob/hooks/before_new_task.py:L63 | neighbors=[run_command()]
- "hooks_before_new_task_rationale_79": "Main hook entry point." | kind=entity | source=.bob/hooks/before_new_task.py:L79 | neighbors=[main()]
- "hooks_deprecated_bump_version": "bump_version.py" | kind=code-symbol | source=scripts/hooks_DEPRECATED/bump_version.py:L1 | neighbors=[bump_version()]
- "hooks_deprecated_bump_version_bump_version": "bump_version()" | kind=code-symbol | source=scripts/hooks_DEPRECATED/bump_version.py:L5 | neighbors=[bump_version.py]
- "hooks_deprecated_safety_guard": "safety_guard.py" | kind=code-symbol | source=scripts/hooks_DEPRECATED/safety_guard.py:L1 | neighbors=[check_file()]
- "hooks_deprecated_safety_guard_check_file": "check_file()" | kind=code-symbol | source=scripts/hooks_DEPRECATED/safety_guard.py:L5 | neighbors=[safety_guard.py]
- "hooks_deprecated_sync_settings_doc": "sync_settings_doc.py" | kind=code-symbol | source=scripts/hooks_DEPRECATED/sync_settings_doc.py:L1 | neighbors=[sync_docs()]
- "hooks_deprecated_sync_settings_doc_sync_docs": "sync_docs()" | kind=code-symbol | source=scripts/hooks_DEPRECATED/sync_settings_doc.py:L5 | neighbors=[sync_settings_doc.py]
- "hooks_deprecated_update_task_status": "update_task_status.py" | kind=code-symbol | source=scripts/hooks_DEPRECATED/update_task_status.py:L1 | neighbors=[update_tasks()]
- "hooks_deprecated_update_task_status_update_tasks": "update_tasks()" | kind=code-symbol | source=scripts/hooks_DEPRECATED/update_task_status.py:L5 | neighbors=[update_task_status.py]
- "hooks_pre_session_rationale_29": "Generate mandatory rules file from Jane Street KB." | kind=entity | source=.bob/hooks/pre_session.py:L29 | neighbors=[generate_jane_street_rules()]
- "hooks_pre_session_rationale_79": "Load bootstrap context for Bob CLI session." | kind=entity | source=.bob/hooks/pre_session.py:L79 | neighbors=[main()]
- "hooks_pre_task_jane_street_kb_rationale_113": "Format KB results for Bob's context" | kind=entity | source=.bob/hooks/pre_task_jane_street_kb.py:L113 | neighbors=[format_kb_results()]
- "hooks_pre_task_jane_street_kb_rationale_67": "Check if task should trigger Jane Street KB query" | kind=entity | source=.bob/hooks/pre_task_jane_street_kb.py:L67 | neighbors=[should_trigger()]
- "hooks_pre_task_jane_street_kb_rationale_77": "Extract relevant topics from task description" | kind=entity | source=.bob/hooks/pre_task_jane_street_kb.py:L77 | neighbors=[extract_topics()]
- "hooks_pre_task_jane_street_kb_rationale_92": "Query Jane Street Knowledge Base" | kind=entity | source=.bob/hooks/pre_task_jane_street_kb.py:L92 | neighbors=[query_jane_street_kb()]
- "identify_wave7_directories_rationale_12": "Extract method name from epic directory's 00-hotspots.md if it exists." | kind=entity | source=identify_wave7_directories.py:L12 | neighbors=[extract_method_from_epic_dir()]
- "identify_wave7_directories_v2_rationale_13": "Extract method name from epic directory's 00-hotspots.md if it exists." | kind=entity | source=identify_wave7_directories_v2.py:L13 | neighbors=[extract_method_from_epic_dir()]
- "launch_wave7_python_main": "main()" | kind=code-symbol | source=launch_wave7_python.py:L12 | neighbors=[launch_wave7_python.py]
- "mcp_command_home_malhitticrypto_local_bin_jcodemunch_mcp": "/home/malhitticrypto/.local/bin/jcodemunch-mcp" | kind=code-symbol | source=.bob/mcp.json:L1 | neighbors=[jcodemunch-mcp]
- "mcp_command_npx": "npx" | kind=code-symbol | source=.bob/mcp.json:L1 | neighbors=[sequential-thinking]
- "mcp_package_modelcontextprotocol_server_sequential_thinking": "@modelcontextprotocol/server-sequential-thinking" | kind=code-symbol | source=.bob/mcp.json:L1 | neighbors=[sequential-thinking]
- "nuget_benchmarkdotnet": "BenchmarkDotNet" | kind=code-symbol | source=benchmarks/V12_Performance.Benchmarks.csproj | neighbors=[V12_Performance.Benchmarks.csproj]
- "nuget_benchmarkdotnet_diagnostics_windows": "BenchmarkDotNet.Diagnostics.Windows" | kind=code-symbol | source=benchmarks/V12_Performance.Benchmarks.csproj | neighbors=[V12_Performance.Benchmarks.csproj]
- "nuget_coverlet_collector": "coverlet.collector" | kind=code-symbol | source=tests/V12_Performance.Tests/V12_Performance.Tests.csproj | neighbors=[V12_Performance.Tests.csproj]
- "nuget_coverlet_msbuild": "coverlet.msbuild" | kind=code-symbol | source=Testing.csproj | neighbors=[Testing.csproj]
- "nuget_nunit": "NUnit" | kind=code-symbol | source=Testing.csproj | neighbors=[Testing.csproj]
- "nuget_nunit3testadapter": "NUnit3TestAdapter" | kind=code-symbol | source=Testing.csproj | neighbors=[Testing.csproj]
- "nuget_stylecop_analyzers": "StyleCop.Analyzers" | kind=code-symbol | source=Linting.csproj | neighbors=[Linting.csproj]
- "nuget_system_runtime_compilerservices_unsafe": "System.Runtime.CompilerServices.Unsafe" | kind=code-symbol | source=sandbox/R28_MmioSpscRing/R28_MmioSpscRing.csproj | neighbors=[R28_MmioSpscRing.csproj]
- "nuget_system_valuetuple": "System.ValueTuple" | kind=code-symbol | source=Testing.csproj | neighbors=[Testing.csproj]
- "nuget_verify_xunit": "Verify.Xunit" | kind=code-symbol | source=tests/V12_Performance.Tests/V12_Performance.Tests.csproj | neighbors=[V12_Performance.Tests.csproj]
- "package_wave7_for_local_main": "main()" | kind=code-symbol | source=package_wave7_for_local.py:L13 | neighbors=[package_wave7_for_local.py]
- "relaunch_final_5_with_path_fix_rationale_20": "Launch epic with explicitly fixed PATH environment" | kind=entity | source=relaunch_final_5_with_path_fix.py:L20 | neighbors=[launch_epic_with_fixed_path()]
- "scripts_agent_bootstrap_agentbootstraploader_init": ".__init__()" | kind=code-symbol | source=scripts/agent_bootstrap.py:L46 | neighbors=[AgentBootstrapLoader]
- "scripts_agent_bootstrap_rationale_183": "Load Graphify knowledge graph." | kind=entity | source=scripts/agent_bootstrap.py:L183 | neighbors=[._load_graphify_graph()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-043.json

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
