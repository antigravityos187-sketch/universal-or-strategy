# Node Description Batch 16 of 61

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

- "deprecated_tool_bugs_launch_wave2_phase0_with_verification": "launch_wave2_phase0_with_verification.py" | kind=code-symbol | source=scripts/wave2/_deprecated_tool_bugs/launch_wave2_phase0_with_verification.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…, create_phase0_script_with_verification(), launch_phase0()]
- "eval_viewer_generate_review_find_runs": "find_runs()" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L60 | neighbors=[generate_review.py, _find_runs_recursive(), load_previous_iteration(), main(), Recursively find directories that conta…, .do_GET()]
- "framework_net8_0": "net8.0" | kind=entity | source=xunit-tests/W7-160/W7_160.Tests.csproj | neighbors=[Linting.csproj, W7_007.Tests.csproj, W7_024.Tests.csproj, W7_025.Tests.csproj, W7_096.Tests.csproj, W7_160.Tests.csproj]
- "generate_missing_phase0_scripts": "generate_missing_phase0_scripts.py" | kind=code-symbol | source=generate_missing_phase0_scripts.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, extract_epic_number(), generate_phase0_script(), load_roadmap(), main()]
- "linting": "Linting.csproj" | kind=code-symbol | source=Linting.csproj:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, net8.0, StyleCop.Analyzers, Microsoft.NET.Sdk, universal-or-strategy.sln]
- "nuget_xunit": "xunit" | kind=code-symbol | source=xunit-tests/W7-160/W7_160.Tests.csproj | neighbors=[V12_Performance.Tests.csproj, W7_007.Tests.csproj, W7_024.Tests.csproj, W7_025.Tests.csproj, W7_096.Tests.csproj, W7_160.Tests.csproj]
- "nuget_xunit_runner_visualstudio": "xunit.runner.visualstudio" | kind=code-symbol | source=xunit-tests/W7-160/W7_160.Tests.csproj | neighbors=[V12_Performance.Tests.csproj, W7_007.Tests.csproj, W7_024.Tests.csproj, W7_025.Tests.csproj, W7_096.Tests.csproj, W7_160.Tests.csproj]
- "scripts_check_roadmap_status": "check_roadmap_status.py" | kind=code-symbol | source=scripts/check_roadmap_status.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "scripts_complexity_audit_extract_methods": "extract_methods()" | kind=code-symbol | source=scripts/complexity_audit.py:L94 | neighbors=[complexity_audit.py, detect_m5_candidate(), estimate_cyclomatic_complexity(), MethodMetrics, generate_report(), Extract all methods from a C# file with…]
- "scripts_continue_session_init_session": "init_session()" | kind=code-symbol | source=scripts/continue_session.py:L91 | neighbors=[continue_session.py, get_git_info(), load_state(), save_state(), main(), Initialize new /continue session.     …]
- "scripts_continue_session_load_state": "load_state()" | kind=code-symbol | source=scripts/continue_session.py:L66 | neighbors=[continue_session.py, complete_task(), get_minimal_context(), init_session(), Load state from .continue/state.json., show_status()]
- "scripts_epic_manifest_get_manifest_path": "_get_manifest_path()" | kind=code-symbol | source=scripts/epic_manifest.py:L144 | neighbors=[epic_manifest.py, add_ticket_phases(), generate_manifest(), load_manifest(), Get path to manifest file for an epic, update_manifest()]
- "scripts_epic_manifest_verify_can_execute": "verify_can_execute()" | kind=code-symbol | source=scripts/epic_manifest.py:L847 | neighbors=[epic_manifest.py, V12.52 Blocking Gate: Verify phase can …, start_phase_execution(), validate_dependencies(), _validate_phase_id(), verify_filesystem_state()]
- "scripts_epic_planner_main": "main()" | kind=code-symbol | source=scripts/epic_planner.py:L168 | neighbors=[epic_planner.py, generate_epic_roadmap(), get_codescene_review(), get_jcodemunch_hotspots(), print_roadmap(), save_roadmap()]
- "scripts_extract_phase5_bobcoins": "extract_phase5_bobcoins.py" | kind=code-symbol | source=scripts/extract_phase5_bobcoins.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…, extract_bobcoins_from_log(), main()]
- "scripts_generate_epic_roadmap_main": "main()" | kind=code-symbol | source=scripts/generate_epic_roadmap.py:L99 | neighbors=[generate_epic_roadmap.py, load_existing_roadmap(), merge_roadmaps(), parse_audit_output(), run_complexity_audit(), Generate epic roadmap.]
- "scripts_jcodemunch_hook_jcodemunchhook_update_from_commit": ".update_from_commit()" | kind=code-symbol | source=scripts/jcodemunch_hook.py:L153 | neighbors=[JCodemunchHook, .index_file(), .index_folder(), .register_edit(), main(), Update jCodemunch index based on files …]
- "scripts_lamport_clock_deterministicworkflow_get_event_log": ".get_event_log()" | kind=code-symbol | source=scripts/lamport_clock.py:L154 | neighbors=[DeterministicWorkflow, .check_dependencies(), .get_next_phases(), .replay_workflow(), .verify_determinism(), Get event log, optionally filtered.   …]
- "scripts_linear_sync_linearsync_sync_to_linear": ".sync_to_linear()" | kind=code-symbol | source=scripts/linear_sync.py:L334 | neighbors=[LinearSync, LinearIssue, .create_epic(), .create_issue(), main(), Sync parsed roadmap to Linear.]
- "scripts_negative_evidence_check_main": "main()" | kind=code-symbol | source=scripts/negative_evidence_check.py:L104 | neighbors=[negative_evidence_check.py, NegativeEvidenceCache, .check(), .clear(), .list_all(), .record()]
- "scripts_negative_evidence_check_negativeevidencecache_record": ".record()" | kind=code-symbol | source=scripts/negative_evidence_check.py:L57 | neighbors=[main(), NegativeEvidenceCache, .check(), .load(), .save(), Record negative evidence for a query.]
- "scripts_orchestrate_full_epic_execution_epicwaveorchestrator_execute_phase_all_waves": ".execute_phase_all_waves()" | kind=code-symbol | source=scripts/orchestrate_full_epic_execution.py:L233 | neighbors=[EpicWaveOrchestrator, .execute_all_phases(), .execute_wave(), ._get_wave_epics(), main(), Execute a single phase for all waves.]
- "scripts_orchestrate_phase_execution": "orchestrate_phase_execution.py" | kind=code-symbol | source=scripts/orchestrate_phase_execution.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, main(), PhaseOrchestrator]
- "scripts_package_skill": "package_skill.py" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/package_skill.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, main(), package_skill(), should_exclude(), quick_validate.py]
- "scripts_precompute_wave7_graph": "precompute_wave7_graph.py" | kind=code-symbol | source=scripts/precompute_wave7_graph.py:L1 | neighbors=[b0a803b feat(wave7): Phase 2 Architectu…, build_okf_cache(), build_precomputed(), load_epic_list(), main(), run_complexity_audit()]
- "scripts_round26_stress_harness_main": "main()" | kind=code-symbol | source=scripts/round26_stress_harness.py:L582 | neighbors=[round26_stress_harness.py, build_program_source(), load_pipeline_source(), run_harness(), write_outputs(), write_temp_project()]
- "scripts_temp_load_manifest": "temp_load_manifest.py" | kind=code-symbol | source=scripts/temp_load_manifest.py:L1 | neighbors=[283eb34 Merge documentation: EPIC-CCN-1…, 4d04458 docs: EPIC-CCN-16/17/18 documen…, 75e2ef2 docs: EPIC-CCN-16/17/18 documen…, 8fd8b93 Merge documentation: EPIC-CCN-1…, a193d48 docs: EPIC-CCN-16/17/18 documen…, ae8e70b docs: EPIC-CCN-16/17/18 documen…]
- "scripts_test_phase_mcp_integration": "test_phase_mcp_integration.py" | kind=code-symbol | source=scripts/test_phase_mcp_integration.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, IntegrationTester, main()]
- "scripts_test_phase_mcp_servers": "test_phase_mcp_servers.py" | kind=code-symbol | source=scripts/test_phase_mcp_servers.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, main(), MCPServerTester]
- "scripts_update_manifest_phase23": "update_manifest_phase23.py" | kind=code-symbol | source=scripts/update_manifest_phase23.py:L1 | neighbors=[283eb34 Merge documentation: EPIC-CCN-1…, 4d04458 docs: EPIC-CCN-16/17/18 documen…, 75e2ef2 docs: EPIC-CCN-16/17/18 documen…, 8fd8b93 Merge documentation: EPIC-CCN-1…, a193d48 docs: EPIC-CCN-16/17/18 documen…, ae8e70b docs: EPIC-CCN-16/17/18 documen…]
- "scripts_validate_phase_compliance": "validate_phase_compliance.py" | kind=code-symbol | source=scripts/validate_phase_compliance.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, main(), PhaseValidator, validate_all_epics(), validate_epic_phase()]
- "scripts_validate_phase_compliance_validate_epic_phase": "validate_epic_phase()" | kind=code-symbol | source=scripts/validate_phase_compliance.py:L252 | neighbors=[validate_phase_compliance.py, main(), Validate a single epic phase. Returns T…, validate_all_epics(), PhaseValidator, .validate()]
- "scripts_verify_index_freshness_verify_index_freshness": "verify_index_freshness()" | kind=code-symbol | source=scripts/verify_index_freshness.py:L60 | neighbors=[verify_index_freshness.py, main(), Verify jCodemunch index is fresh.     …, get_git_head_timestamp(), get_graphify_timestamp(), get_modified_files_since()]
- "scripts_verify_wave7_templates_verify_template": "verify_template()" | kind=code-symbol | source=scripts/verify_wave7_templates.py:L106 | neighbors=[verify_wave7_templates.py, main(), Verify a single template file., check_epic_naming(), check_temp_file_pattern(), fix_epic_naming()]
- "scripts_wave_coordinator": "wave_coordinator.py" | kind=code-symbol | source=scripts/wave_coordinator.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…, main(), WaveCoordinator]
- "scripts_wave_coordinator_wavecoordinator_execute_wave": ".execute_wave()" | kind=code-symbol | source=scripts/wave_coordinator.py:L61 | neighbors=[Execute one phase for multiple epics. …, WaveCoordinator, ._generate_instructions(), .get_phase_config(), .generate_execution_plan(), .run_wave_batch()]
- "scripts_wave_coordinator_wavecoordinator_generate_execution_plan": ".generate_execution_plan()" | kind=code-symbol | source=scripts/wave_coordinator.py:L211 | neighbors=[main(), Generate complete execution plan for N …, WaveCoordinator, .execute_wave(), .get_next_wave(), .load_roadmap()]
- "scripts_worker_agent_mcp_claim_epic_tool": "claim_epic_tool()" | kind=code-symbol | source=scripts/worker_agent_mcp.py:L159 | neighbors=[worker_agent_mcp.py, call_tool(), load_roadmap(), run_command(), save_roadmap(), Atomically claim epic using git-based l…]
- "w7_007_w7_007_tests": "W7_007.Tests.csproj" | kind=code-symbol | source=xunit-tests/W7-007/W7_007.Tests.csproj:L1 | neighbors=[e01e4e5 wave7: backup all phase 0-5V wo…, net8.0, Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio, Microsoft.NET.Sdk]
- "w7_024_w7_024_tests": "W7_024.Tests.csproj" | kind=code-symbol | source=xunit-tests/W7-024/W7_024.Tests.csproj:L1 | neighbors=[e01e4e5 wave7: backup all phase 0-5V wo…, net8.0, Microsoft.NET.Test.Sdk, xunit, xunit.runner.visualstudio, Microsoft.NET.Sdk]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-015.json

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
