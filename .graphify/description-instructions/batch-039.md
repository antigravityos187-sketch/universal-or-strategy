# Node Description Batch 40 of 61

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

- "scripts_reaper_split_read_source_lines": "read_source_lines()" | kind=code-symbol | source=scripts/reaper_split.py:L48 | neighbors=[reaper_split.py, main()]
- "scripts_reaper_split_write_file": "write_file()" | kind=code-symbol | source=scripts/reaper_split.py:L60 | neighbors=[reaper_split.py, main()]
- "scripts_regenerate_phase2_all": "regenerate_phase2_all.py" | kind=code-symbol | source=scripts/regenerate_phase2_all.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …]
- "scripts_register_existing_outputs_main": "main()" | kind=code-symbol | source=scripts/register_existing_outputs.py:L57 | neighbors=[register_existing_outputs.py, register_outputs()]
- "scripts_remove_phase_start_from_completed_main": "main()" | kind=code-symbol | source=scripts/remove_phase_start_from_completed.py:L55 | neighbors=[remove_phase_start_from_completed.py, fix_manifest()]
- "scripts_reset_wave6_manifests_v2_main": "main()" | kind=code-symbol | source=scripts/reset_wave6_manifests_v2.py:L49 | neighbors=[reset_wave6_manifests_v2.py, reset_manifest()]
- "scripts_round26_stress_harness_build_program_source": "build_program_source()" | kind=code-symbol | source=scripts/round26_stress_harness.py:L29 | neighbors=[round26_stress_harness.py, main()]
- "scripts_round26_stress_harness_load_pipeline_source": "load_pipeline_source()" | kind=code-symbol | source=scripts/round26_stress_harness.py:L25 | neighbors=[round26_stress_harness.py, main()]
- "scripts_round26_stress_harness_run_harness": "run_harness()" | kind=code-symbol | source=scripts/round26_stress_harness.py:L484 | neighbors=[round26_stress_harness.py, main()]
- "scripts_round26_stress_harness_write_outputs": "write_outputs()" | kind=code-symbol | source=scripts/round26_stress_harness.py:L520 | neighbors=[round26_stress_harness.py, main()]
- "scripts_round26_stress_harness_write_temp_project": "write_temp_project()" | kind=code-symbol | source=scripts/round26_stress_harness.py:L462 | neighbors=[round26_stress_harness.py, main()]
- "scripts_run_eval_run_single_query": "run_single_query()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/run_eval.py:L35 | neighbors=[run_eval.py, Run a single query and return whether t…]
- "scripts_run_loop_main": "main()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/run_loop.py:L244 | neighbors=[run_loop.py, run_loop()]
- "scripts_select_pilot_epics": "select_pilot_epics.py" | kind=code-symbol | source=scripts/select_pilot_epics.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "scripts_surgical_fix_agents": "surgical_fix_agents.py" | kind=code-symbol | source=scripts/surgical_fix_agents.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…]
- "scripts_sync_lamport_events_main": "main()" | kind=code-symbol | source=scripts/sync_lamport_events.py:L61 | neighbors=[sync_lamport_events.py, sync_events_to_global_log()]
- "scripts_test_fastmcp_phase0_test_phase0_mcp": "test_phase0_mcp()" | kind=code-symbol | source=scripts/test_fastmcp_phase0.py:L12 | neighbors=[test_fastmcp_phase0.py, Test Phase 0 FastMCP server]
- "scripts_test_parallel_phase0_main": "main()" | kind=code-symbol | source=scripts/test_parallel_phase0.py:L211 | neighbors=[test_parallel_phase0.py, run_parallel_phase0_test()]
- "scripts_test_phase_mcp_servers_mcpservertester_init": ".__init__()" | kind=code-symbol | source=scripts/test_phase_mcp_servers.py:L30 | neighbors=[MCPServerTester, ._load_config()]
- "scripts_test_v12_52_test_dependency_checking": "test_dependency_checking()" | kind=code-symbol | source=scripts/test_v12_52.py:L115 | neighbors=[test_v12_52.py, Test 3: Dependency checking works corre…]
- "scripts_test_v12_52_test_deterministic_workflow": "test_deterministic_workflow()" | kind=code-symbol | source=scripts/test_v12_52.py:L158 | neighbors=[test_v12_52.py, Test 5: Workflow determinism verificati…]
- "scripts_test_v12_52_test_event_log_ordering": "test_event_log_ordering()" | kind=code-symbol | source=scripts/test_v12_52.py:L97 | neighbors=[test_v12_52.py, Test 2: Event log maintains causal orde…]
- "scripts_test_v12_52_test_failure_handling": "test_failure_handling()" | kind=code-symbol | source=scripts/test_v12_52.py:L245 | neighbors=[test_v12_52.py, Test 8: Failure handling and recovery.]
- "scripts_test_v12_52_test_filesystem_state_verification": "test_filesystem_state_verification()" | kind=code-symbol | source=scripts/test_v12_52.py:L228 | neighbors=[test_v12_52.py, Test 7: Filesystem state verification.]
- "scripts_test_v12_52_test_lamport_clock_monotonicity": "test_lamport_clock_monotonicity()" | kind=code-symbol | source=scripts/test_v12_52.py:L70 | neighbors=[test_v12_52.py, Test 1: Lamport clock increments monoto…]
- "scripts_test_v12_52_test_manifest_integration": "test_manifest_integration()" | kind=code-symbol | source=scripts/test_v12_52.py:L172 | neighbors=[test_v12_52.py, Test 6: Manifest integration with V12.5…]
- "scripts_test_v12_52_test_state_hash_computation": "test_state_hash_computation()" | kind=code-symbol | source=scripts/test_v12_52.py:L142 | neighbors=[test_v12_52.py, Test 4: State hash computation is deter…]
- "scripts_test_wave7_lamport_main": "main()" | kind=code-symbol | source=scripts/test_wave7_lamport.py:L23 | neighbors=[test_wave7_lamport.py, Test Wave 7 Lamport clock implementatio…]
- "scripts_utils_parse_skill_md": "parse_skill_md()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/utils.py:L7 | neighbors=[utils.py, Parse a SKILL.md file, returning (name,…]
- "scripts_v12_split_main": "main()" | kind=code-symbol | source=scripts/v12_split.py:L94 | neighbors=[v12_split.py, split_method()]
- "scripts_verify_index_freshness_main": "main()" | kind=code-symbol | source=scripts/verify_index_freshness.py:L118 | neighbors=[verify_index_freshness.py, verify_index_freshness()]
- "scripts_verify_phase_1_5": "verify_phase_1_5.py" | kind=code-symbol | source=scripts/verify_phase_1_5.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "scripts_wave_coordinator_wavecoordinator_init": ".__init__()" | kind=code-symbol | source=scripts/wave_coordinator.py:L33 | neighbors=[Initialize Wave Coordinator.         …, WaveCoordinator]
- "scripts_wave2_bob_shell_executor_execute_phase_with_bob_shell": "execute_phase_with_bob_shell()" | kind=code-symbol | source=scripts/wave2_bob_shell_executor.py:L30 | neighbors=[wave2_bob_shell_executor.py, Execute a phase using Bob Shell API mod…]
- "scripts_wave2_parallel_executor_execute_bob_for_epic": "execute_bob_for_epic()" | kind=code-symbol | source=scripts/wave2_parallel_executor.py:L48 | neighbors=[wave2_parallel_executor.py, Execute Bob CLI for a single epic in it…]
- "scripts_wave2_parallel_executor_main": "main()" | kind=code-symbol | source=scripts/wave2_parallel_executor.py:L294 | neighbors=[wave2_parallel_executor.py, execute_phase_parallel()]
- "scripts_wave2_parallel_executor_phase_1_5_prompt": "phase_1_5_prompt()" | kind=code-symbol | source=scripts/wave2_parallel_executor.py:L173 | neighbors=[wave2_parallel_executor.py, Generate Phase 1.5 prompt for an epic]
- "scripts_wave2_parallel_executor_phase_1_prompt": "phase_1_prompt()" | kind=code-symbol | source=scripts/wave2_parallel_executor.py:L146 | neighbors=[wave2_parallel_executor.py, Generate Phase 1 prompt for an epic]
- "scripts_wave2_parallel_executor_phase_2_prompt": "phase_2_prompt()" | kind=code-symbol | source=scripts/wave2_parallel_executor.py:L194 | neighbors=[wave2_parallel_executor.py, Generate Phase 2 prompt for an epic]
- "scripts_wave2_parallel_executor_phase_3_prompt": "phase_3_prompt()" | kind=code-symbol | source=scripts/wave2_parallel_executor.py:L214 | neighbors=[wave2_parallel_executor.py, Generate Phase 3 prompt for an epic]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-039.json

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
