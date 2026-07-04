# Node Description Batch 24 of 61

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

- "scripts_test_phase_mcp_integration_integrationtester_test_dependency_validation": ".test_dependency_validation()" | kind=code-symbol | source=scripts/test_phase_mcp_integration.py:L246 | neighbors=[IntegrationTester, .log(), .test_full_workflow(), Test dependency validation between phas…]
- "scripts_test_phase_mcp_integration_integrationtester_test_manifest_initialization": ".test_manifest_initialization()" | kind=code-symbol | source=scripts/test_phase_mcp_integration.py:L103 | neighbors=[IntegrationTester, .test_full_workflow(), .log(), Test 1: Manifest initialization]
- "scripts_test_phase_mcp_integration_integrationtester_test_phase_execution": ".test_phase_execution()" | kind=code-symbol | source=scripts/test_phase_mcp_integration.py:L166 | neighbors=[IntegrationTester, .test_full_workflow(), .log(), Test phase execution and artifact gener…]
- "scripts_test_phase_mcp_integration_main": "main()" | kind=code-symbol | source=scripts/test_phase_mcp_integration.py:L366 | neighbors=[test_phase_mcp_integration.py, IntegrationTester, .create_test_epic(), .test_full_workflow()]
- "scripts_test_phase_mcp_servers_mcpservertester_log": "._log()" | kind=code-symbol | source=scripts/test_phase_mcp_servers.py:L43 | neighbors=[MCPServerTester, .test_script_syntax(), .test_server_config(), Log message if verbose]
- "scripts_test_phase_mcp_servers_mcpservertester_test_script_syntax": ".test_script_syntax()" | kind=code-symbol | source=scripts/test_phase_mcp_servers.py:L170 | neighbors=[MCPServerTester, .test_all_phase_servers(), ._log(), Test Python script syntax]
- "scripts_test_wave7_lamport": "test_wave7_lamport.py" | kind=code-symbol | source=scripts/test_wave7_lamport.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, lamport_clock.py, main()]
- "scripts_test_worker_mcp_client_test_worker_agent": "test_worker_agent()" | kind=code-symbol | source=scripts/test_worker_mcp_client.py:L17 | neighbors=[test_worker_mcp_client.py, Test all MCP tools for a single worker, test_all_workers(), test_single_worker()]
- "scripts_trailing_split": "trailing_split.py" | kind=code-symbol | source=scripts/trailing_split.py:L1 | neighbors=[extract(), make_header_simple(), make_header_wrapped(), write_file()]
- "scripts_v12_split_split_method": "split_method()" | kind=code-symbol | source=scripts/v12_split.py:L64 | neighbors=[v12_split.py, main(), Split a method from source file.     If…, extract_method_block()]
- "scripts_validate_epic_get_epic_details": "get_epic_details()" | kind=code-symbol | source=scripts/validate_epic.py:L40 | neighbors=[validate_epic.py, load_roadmap(), main(), Get epic details from roadmap]
- "scripts_validate_epic_get_next_epic": "get_next_epic()" | kind=code-symbol | source=scripts/validate_epic.py:L48 | neighbors=[validate_epic.py, load_roadmap(), main(), Get next pending epic from roadmap (sta…]
- "scripts_validate_epic_list_assigned_epics": "list_assigned_epics()" | kind=code-symbol | source=scripts/validate_epic.py:L125 | neighbors=[validate_epic.py, load_roadmap(), main(), List all currently assigned epics]
- "scripts_validate_epic_list_pending_epics": "list_pending_epics()" | kind=code-symbol | source=scripts/validate_epic.py:L130 | neighbors=[validate_epic.py, load_roadmap(), main(), List pending epics (not complete, not a…]
- "scripts_validate_epic_validate_epic_exists": "validate_epic_exists()" | kind=code-symbol | source=scripts/validate_epic.py:L35 | neighbors=[validate_epic.py, main(), Verify epic exists in roadmap, load_roadmap()]
- "scripts_validate_phase_compliance_validate_all_epics": "validate_all_epics()" | kind=code-symbol | source=scripts/validate_phase_compliance.py:L284 | neighbors=[validate_phase_compliance.py, main(), Validate all epics in docs/brain/EPIC-W…, validate_epic_phase()]
- "scripts_verify_wave7_determinism_main": "main()" | kind=code-symbol | source=scripts/verify_wave7_determinism.py:L130 | neighbors=[verify_wave7_determinism.py, print_results(), verify_all_epics(), verify_epic()]
- "scripts_verify_wave7_determinism_verify_epic": "verify_epic()" | kind=code-symbol | source=scripts/verify_wave7_determinism.py:L45 | neighbors=[verify_wave7_determinism.py, main(), Verify determinism for a single epic. …, verify_all_epics()]
- "scripts_wave_coordinator_main": "main()" | kind=code-symbol | source=scripts/wave_coordinator.py:L251 | neighbors=[wave_coordinator.py, WaveCoordinator, .generate_execution_plan(), CLI entry point for wave coordinator.]
- "scripts_wave_coordinator_wavecoordinator_get_next_wave": ".get_next_wave()" | kind=code-symbol | source=scripts/wave_coordinator.py:L185 | neighbors=[Get next batch of pending epics.      …, WaveCoordinator, .generate_execution_plan(), .load_roadmap()]
- "scripts_wave_coordinator_wavecoordinator_load_roadmap": ".load_roadmap()" | kind=code-symbol | source=scripts/wave_coordinator.py:L48 | neighbors=[Load epic roadmap and filter pending ep…, WaveCoordinator, .generate_execution_plan(), .get_next_wave()]
- "scripts_wave_coordinator_wavecoordinator_run_wave_batch": ".run_wave_batch()" | kind=code-symbol | source=scripts/wave_coordinator.py:L140 | neighbors=[Run a batch of epics through specified …, WaveCoordinator, .execute_wave(), ._save_checkpoint()]
- "scripts_wave2_direct_executor_execute_phase_0_all": "execute_phase_0_all()" | kind=code-symbol | source=scripts/wave2_direct_executor.py:L84 | neighbors=[wave2_direct_executor.py, create_phase_0_artifacts(), main(), Execute Phase 0 for all Wave 2 epics]
- "scripts_wave2_direct_executor_execute_phase_1_all": "execute_phase_1_all()" | kind=code-symbol | source=scripts/wave2_direct_executor.py:L112 | neighbors=[wave2_direct_executor.py, create_bob_prompt_for_phase_1(), main(), Execute Phase 1 for all Wave 2 epics us…]
- "scripts_wave2_simple_orchestrator_main": "main()" | kind=code-symbol | source=scripts/wave2_simple_orchestrator.py:L84 | neighbors=[wave2_simple_orchestrator.py, execute_phase_0_batch(), execute_phase_1_batch(), Main orchestration loop.]
- "scripts_wave7_batch_audit_run_batch_audit": "run_batch_audit()" | kind=code-symbol | source=scripts/wave7_batch_audit.py:L500 | neighbors=[wave7_batch_audit.py, main(), Audit a batch of epics for a given phas…, audit_epic()]
- "scripts_worker_agent_mcp_execute_epic_tool": "execute_epic_tool()" | kind=code-symbol | source=scripts/worker_agent_mcp.py:L251 | neighbors=[worker_agent_mcp.py, call_tool(), run_command(), Execute all phases of epic]
- "scripts_worker_agent_mcp_get_next_pending_epic_tool": "get_next_pending_epic_tool()" | kind=code-symbol | source=scripts/worker_agent_mcp.py:L393 | neighbors=[worker_agent_mcp.py, call_tool(), load_roadmap(), Get next pending epic]
- "universal_or_strategy": "universal-or-strategy.sln" | kind=code-symbol | source=universal-or-strategy.sln:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, Linting.csproj, Testing.csproj]
- "wave1_generate_corrected_p0_006_015": "generate_corrected_p0_006_015.py" | kind=code-symbol | source=scripts/wave1/generate_corrected_p0_006_015.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 3bcfaac feat: Institutionalize building…, a24ed35 feat: Institutionalize building…, be6c8a1 docs: Wave 4 documentation merg…]
- "wave1_generate_p0_006_015": "generate_p0_006_015.py" | kind=code-symbol | source=scripts/wave1/generate_p0_006_015.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 3bcfaac feat: Institutionalize building…, a24ed35 feat: Institutionalize building…, be6c8a1 docs: Wave 4 documentation merg…]
- "wave1_generate_phase2_all_epics": "generate_phase2_all_epics.py" | kind=code-symbol | source=scripts/wave1/generate_phase2_all_epics.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 3bcfaac feat: Institutionalize building…, a24ed35 feat: Institutionalize building…, be6c8a1 docs: Wave 4 documentation merg…]
- "wave2_api_balance_tracker_check_phase_feasibility": "check_phase_feasibility()" | kind=code-symbol | source=scripts/wave2/api_balance_tracker.py:L142 | neighbors=[api_balance_tracker.py, estimate_phase_budget(), load_tracker_state(), Check if we have enough bobcoins for a …]
- "wave2_api_balance_tracker_record_usage": "record_usage()" | kind=code-symbol | source=scripts/wave2/api_balance_tracker.py:L53 | neighbors=[api_balance_tracker.py, Record bobcoin usage for an API, load_tracker_state(), save_tracker_state()]
- "wave2_get_wave2_complexity": "get_wave2_complexity.py" | kind=code-symbol | source=scripts/wave2/get_wave2_complexity.py:L1 | neighbors=[00170a9 Wave 2 Complete: Documentation,…, 1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, eba9abf Wave 2 Complete: Documentation,…]
- "wave2_launch_wave_collect_results": "collect_results()" | kind=code-symbol | source=scripts/wave2/launch_wave.py:L149 | neighbors=[launch_wave.py, gcloud(), main(), Pull log files from VM back to local ma…]
- "wave2_launch_wave_main": "main()" | kind=code-symbol | source=scripts/wave2/launch_wave.py:L164 | neighbors=[launch_wave.py, collect_results(), launch_wave(), monitor_wave()]
- "wave2_launch_wave_monitor_wave": "monitor_wave()" | kind=code-symbol | source=scripts/wave2/launch_wave.py:L127 | neighbors=[launch_wave.py, main(), gcloud(), Poll VM for orchestrator status and age…]
- "wave2_launch_wave_now_clear_stale_ssh_key": "clear_stale_ssh_key()" | kind=code-symbol | source=scripts/wave2/launch_wave_now.py:L123 | neighbors=[launch_wave_now.py, gcloud_capture(), main(), Remove stale Plink key cache entry for …]
- "wave2_launch_wave_v2_clear_stale_ssh_key": "clear_stale_ssh_key()" | kind=code-symbol | source=scripts/wave2/launch_wave_v2.py:L130 | neighbors=[launch_wave_v2.py, gcloud_capture(), main(), Remove stale Plink key cache entry for …]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-023.json

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
