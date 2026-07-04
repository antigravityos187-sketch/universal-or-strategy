# Node Description Batch 22 of 61

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

- "scripts_diagnose_concurrent_agents": "diagnose_concurrent_agents.py" | kind=code-symbol | source=scripts/diagnose_concurrent_agents.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, diagnose_epic(), main()]
- "scripts_epic_manifest_add_ticket_phases": "add_ticket_phases()" | kind=code-symbol | source=scripts/epic_manifest.py:L717 | neighbors=[epic_manifest.py, _get_manifest_path(), ValidationError, Add ticket execution and verification p…]
- "scripts_epic_manifest_detect_circular_dependencies": "_detect_circular_dependencies()" | kind=code-symbol | source=scripts/epic_manifest.py:L211 | neighbors=[epic_manifest.py, load_manifest(), Detect circular dependencies using DFS.…, validate_dependencies()]
- "scripts_epic_manifest_generate_manifest": "generate_manifest()" | kind=code-symbol | source=scripts/epic_manifest.py:L565 | neighbors=[epic_manifest.py, _get_manifest_path(), ValidationError, Create new manifest for an epic.     …]
- "scripts_epic_manifest_get_next_phases": "get_next_phases()" | kind=code-symbol | source=scripts/epic_manifest.py:L522 | neighbors=[epic_manifest.py, load_manifest(), validate_dependencies(), Determine which phases can be executed …]
- "scripts_epic_manifest_start_phase_execution": "start_phase_execution()" | kind=code-symbol | source=scripts/epic_manifest.py:L970 | neighbors=[epic_manifest.py, Start phase execution with V12.52 verif…, update_manifest(), verify_can_execute()]
- "scripts_epic_manifest_validate_artifact_path": "_validate_artifact_path()" | kind=code-symbol | source=scripts/epic_manifest.py:L173 | neighbors=[epic_manifest.py, Validate artifact path is in correct lo…, update_manifest(), ValidationError]
- "scripts_epic_manifest_validate_status_transition": "_validate_status_transition()" | kind=code-symbol | source=scripts/epic_manifest.py:L158 | neighbors=[epic_manifest.py, Validate status transition is allowed, update_manifest(), ValidationError]
- "scripts_epic_manifest_validate_timestamps": "_validate_timestamps()" | kind=code-symbol | source=scripts/epic_manifest.py:L187 | neighbors=[epic_manifest.py, load_manifest(), Validate phase timestamps are in correc…, ValidationError]
- "scripts_epic_manifest_verify_filesystem_state": "verify_filesystem_state()" | kind=code-symbol | source=scripts/epic_manifest.py:L898 | neighbors=[epic_manifest.py, Verify filesystem state matches manifes…, verify_can_execute(), load_manifest()]
- "scripts_epic_planner_get_codescene_review": "get_codescene_review()" | kind=code-symbol | source=scripts/epic_planner.py:L42 | neighbors=[epic_planner.py, generate_epic_roadmap(), main(), Get CodeScene CLI review for a file]
- "scripts_find_high_complexity_epics": "find_high_complexity_epics.py" | kind=code-symbol | source=scripts/find_high_complexity_epics.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…]
- "scripts_fix_manifest_synthetic_events": "fix_manifest_synthetic_events.py" | kind=code-symbol | source=scripts/fix_manifest_synthetic_events.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, fix_manifest_events(), main()]
- "scripts_fix_phase_modes": "fix_phase_modes.py" | kind=code-symbol | source=scripts/fix_phase_modes.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, fix_manifest(), main()]
- "scripts_fix_phase0_status": "fix_phase0_status.py" | kind=code-symbol | source=scripts/fix_phase0_status.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, fix_epic(), main()]
- "scripts_fix_phase1_outputs": "fix_phase1_outputs.py" | kind=code-symbol | source=scripts/fix_phase1_outputs.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, fix_manifest(), main()]
- "scripts_fix_synthetic_events": "fix_synthetic_events.py" | kind=code-symbol | source=scripts/fix_synthetic_events.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, fix_event_log(), main()]
- "scripts_fix_wave1_targets": "fix_wave1_targets.py" | kind=code-symbol | source=scripts/fix_wave1_targets.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, 1440258 docs: Wave 2 session artifacts …, 1b142f7 docs: Wave 2 session artifacts …, be6c8a1 docs: Wave 4 documentation merg…]
- "scripts_generate_fresh_epic_roadmap": "generate_fresh_epic_roadmap.py" | kind=code-symbol | source=scripts/generate_fresh_epic_roadmap.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "scripts_generate_fresh_epic_roadmap_v2": "generate_fresh_epic_roadmap_v2.py" | kind=code-symbol | source=scripts/generate_fresh_epic_roadmap_v2.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "scripts_generate_phase2_scripts_with_real_keys": "generate_phase2_scripts_with_real_keys.py" | kind=code-symbol | source=scripts/generate_phase2_scripts_with_real_keys.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …, generate_phase2_script(), main()]
- "scripts_generate_phase6_remaining": "generate_phase6_remaining.py" | kind=code-symbol | source=scripts/generate_phase6_remaining.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "scripts_generate_phase6_scripts_fixed": "generate_phase6_scripts_fixed.py" | kind=code-symbol | source=scripts/generate_phase6_scripts_fixed.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "scripts_generate_wave4_phase0_scripts": "generate_wave4_phase0_scripts.py" | kind=code-symbol | source=scripts/generate_wave4_phase0_scripts.py:L1 | neighbors=[1392356 docs: Wave 4 documentation merg…, be6c8a1 docs: Wave 4 documentation merg…, c59b546 chore: Wave 4 session artifacts…, dd1f332 chore: Wave 4 session artifacts…]
- "scripts_generate_wave6_phase1_remaining": "generate_wave6_phase1_remaining.py" | kind=code-symbol | source=scripts/generate_wave6_phase1_remaining.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, generate_phase1_script(), main()]
- "scripts_improve_description_improve_description": "improve_description()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/improve_description.py:L50 | neighbors=[improve_description.py, _call_claude(), main(), Call Claude to improve the description …]
- "scripts_jane_street_utils_format_violation_report": "format_violation_report()" | kind=code-symbol | source=scripts/jane_street_utils.py:L201 | neighbors=[jane_street_utils.py, get_violation_summary(), main(), Format violations as a markdown report…]
- "scripts_lamport_clock_deterministicworkflow_get_next_phases": ".get_next_phases()" | kind=code-symbol | source=scripts/lamport_clock.py:L338 | neighbors=[DeterministicWorkflow, .check_dependencies(), .get_event_log(), Get next executable phases in determini…]
- "scripts_lamport_clock_deterministicworkflow_tick": ".tick()" | kind=code-symbol | source=scripts/lamport_clock.py:L107 | neighbors=[DeterministicWorkflow, .record_event(), ._save_global_clock(), Increment global clock (atomic operatio…]
- "scripts_lamport_clock_record_phase_complete": "record_phase_complete()" | kind=code-symbol | source=scripts/lamport_clock.py:L409 | neighbors=[lamport_clock.py, Record phase completion event., .record_event(), get_workflow()]
- "scripts_lamport_clock_record_phase_fail": "record_phase_fail()" | kind=code-symbol | source=scripts/lamport_clock.py:L415 | neighbors=[lamport_clock.py, Record phase failure event., .record_event(), get_workflow()]
- "scripts_lamport_clock_record_phase_start": "record_phase_start()" | kind=code-symbol | source=scripts/lamport_clock.py:L403 | neighbors=[lamport_clock.py, Record phase start event., .record_event(), get_workflow()]
- "scripts_langsmith_bridge": "langsmith_bridge.py" | kind=code-symbol | source=scripts/langsmith_bridge.py:L1 | neighbors=[amal_harness_v26.py, main(), trace_agent_handoff(), trace_forensic_run()]
- "scripts_linear_sync_main": "main()" | kind=code-symbol | source=scripts/linear_sync.py:L420 | neighbors=[linear_sync.py, LinearSync, .parse_roadmap(), .sync_to_linear()]
- "scripts_linear_sync_v2_linearsync_sync_to_linear": ".sync_to_linear()" | kind=code-symbol | source=scripts/linear_sync_v2.py:L218 | neighbors=[LinearSync, .get_or_create_project(), main(), Sync parsed roadmap to Linear.]
- "scripts_linear_sync_v2_main": "main()" | kind=code-symbol | source=scripts/linear_sync_v2.py:L239 | neighbors=[linear_sync_v2.py, LinearSync, .parse_roadmap(), .sync_to_linear()]
- "scripts_load_api_keys_main": "main()" | kind=code-symbol | source=scripts/load_api_keys.py:L107 | neighbors=[load_api_keys.py, calculate_key_distribution(), format_keys_for_executor(), load_api_keys_from_folder()]
- "scripts_migrate_manifests_to_v12_52": "migrate_manifests_to_v12_52.py" | kind=code-symbol | source=scripts/migrate_manifests_to_v12_52.py:L1 | neighbors=[3a92e1e feat: Wave 6 Phase 0 preparatio…, ceae42c feat: Wave 6 Phase 0 preparatio…, main(), migrate_manifest()]
- "scripts_monitor_vm_progress_check_vm_status": "check_vm_status()" | kind=code-symbol | source=scripts/monitor_vm_progress.py:L72 | neighbors=[monitor_vm_progress.py, run_ssh_command(), main(), Check if VM is running and accessible.]
- "scripts_monitor_vm_progress_get_epic_status": "get_epic_status()" | kind=code-symbol | source=scripts/monitor_vm_progress.py:L77 | neighbors=[monitor_vm_progress.py, run_ssh_command(), main(), Get epic status from manifest.json on V…]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-021.json

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
