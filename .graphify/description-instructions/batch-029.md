# Node Description Batch 30 of 61

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

- "scripts_generate_phase2_scripts_fixed_main": "main()" | kind=code-symbol | source=scripts/generate_phase2_scripts_fixed.py:L79 | neighbors=[generate_phase2_scripts_fixed.py, generate_phase2_script(), get_epics_needing_phase2()]
- "scripts_generate_phase2_scripts_generate_phase2_script": "generate_phase2_script()" | kind=code-symbol | source=scripts/generate_phase2_scripts.py:L33 | neighbors=[generate_phase2_scripts.py, main(), Generate Phase 2 script for a single ep…]
- "scripts_generate_phase2_scripts_get_epics_needing_phase2": "get_epics_needing_phase2()" | kind=code-symbol | source=scripts/generate_phase2_scripts.py:L11 | neighbors=[generate_phase2_scripts.py, main(), Find all epics with Phase 1.5 complete …]
- "scripts_generate_phase2_scripts_main": "main()" | kind=code-symbol | source=scripts/generate_phase2_scripts.py:L58 | neighbors=[generate_phase2_scripts.py, generate_phase2_script(), get_epics_needing_phase2()]
- "scripts_generate_phase2_scripts_with_real_keys_generate_phase2_script": "generate_phase2_script()" | kind=code-symbol | source=scripts/generate_phase2_scripts_with_real_keys.py:L64 | neighbors=[generate_phase2_scripts_with_real_keys.…, main(), Generate Phase 2 script for one epic]
- "scripts_generate_report_generate_html": "generate_html()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/generate_report.py:L16 | neighbors=[generate_report.py, main(), Generate HTML report from loop output d…]
- "scripts_generate_wave6_phase1_remaining_generate_phase1_script": "generate_phase1_script()" | kind=code-symbol | source=scripts/generate_wave6_phase1_remaining.py:L21 | neighbors=[generate_wave6_phase1_remaining.py, main(), Generate Phase 1 script from template]
- "scripts_generate_wave6_phase1_remaining_main": "main()" | kind=code-symbol | source=scripts/generate_wave6_phase1_remaining.py:L36 | neighbors=[generate_wave6_phase1_remaining.py, generate_phase1_script(), Generate all Phase 1 scripts]
- "scripts_generate_wave7_roadmap_generate_roadmap": "generate_roadmap()" | kind=code-symbol | source=scripts/generate_wave7_roadmap.py:L76 | neighbors=[generate_wave7_roadmap.py, main(), Generate Wave 7 roadmap structure.]
- "scripts_generate_wave7_roadmap_main": "main()" | kind=code-symbol | source=scripts/generate_wave7_roadmap.py:L142 | neighbors=[generate_wave7_roadmap.py, generate_roadmap(), parse_complexity_audit()]
- "scripts_generate_wave7_roadmap_parse_complexity_audit": "parse_complexity_audit()" | kind=code-symbol | source=scripts/generate_wave7_roadmap.py:L21 | neighbors=[generate_wave7_roadmap.py, main(), Parse complexity audit file and extract…]
- "scripts_generate_wave7_stats_compute_statistics": "compute_statistics()" | kind=code-symbol | source=scripts/generate_wave7_stats.py:L42 | neighbors=[generate_wave7_stats.py, main(), Compute Wave 7 statistics from events.…]
- "scripts_generate_wave7_stats_load_wave7_events": "load_wave7_events()" | kind=code-symbol | source=scripts/generate_wave7_stats.py:L22 | neighbors=[generate_wave7_stats.py, main(), Load Wave 7 events from wave-specific l…]
- "scripts_generate_wave7_stats_print_summary": "print_summary()" | kind=code-symbol | source=scripts/generate_wave7_stats.py:L157 | neighbors=[generate_wave7_stats.py, main(), Print human-readable summary.        …]
- "scripts_generate_wave7_stats_write_statistics": "write_statistics()" | kind=code-symbol | source=scripts/generate_wave7_stats.py:L140 | neighbors=[generate_wave7_stats.py, main(), Write statistics to JSON file.       …]
- "scripts_improve_description_call_claude": "_call_claude()" | kind=code-symbol | source=.bob/skills/skill-creator/scripts/improve_description.py:L20 | neighbors=[improve_description.py, improve_description(), Run `claude -p` with the prompt on stdi…]
- "scripts_jane_street_utils_get_files_with_violations": "get_files_with_violations()" | kind=code-symbol | source=scripts/jane_street_utils.py:L296 | neighbors=[jane_street_utils.py, load_violations_file(), Get set of all files that have Jane Str…]
- "scripts_jane_street_utils_get_violation_summary": "get_violation_summary()" | kind=code-symbol | source=scripts/jane_street_utils.py:L158 | neighbors=[jane_street_utils.py, format_violation_report(), Get summary statistics for a list of vi…]
- "scripts_jane_street_utils_janestreetviolation_in_range": ".in_range()" | kind=code-symbol | source=scripts/jane_street_utils.py:L62 | neighbors=[JaneStreetViolation, load_violations_in_range(), Check if violation is within line range]
- "scripts_jane_street_utils_query_kb": "query_kb()" | kind=code-symbol | source=scripts/jane_street_utils.py:L249 | neighbors=[jane_street_utils.py, main(), Query Jane Street Firebase KB        …]
- "scripts_jane_street_utils_validate_no_violations": "validate_no_violations()" | kind=code-symbol | source=scripts/jane_street_utils.py:L282 | neighbors=[jane_street_utils.py, Validate that files have no Jane Street…, load_violations_for_files()]
- "scripts_lamport_clock_deterministicworkflow_append_event": "._append_event()" | kind=code-symbol | source=scripts/lamport_clock.py:L60 | neighbors=[DeterministicWorkflow, .record_event(), Append event to immutable log (JSONL fo…]
- "scripts_lamport_clock_deterministicworkflow_compute_state_hash": "._compute_state_hash()" | kind=code-symbol | source=scripts/lamport_clock.py:L65 | neighbors=[DeterministicWorkflow, .record_event(), Compute deterministic hash of epic stat…]
- "scripts_lamport_clock_deterministicworkflow_load_global_clock": "._load_global_clock()" | kind=code-symbol | source=scripts/lamport_clock.py:L44 | neighbors=[DeterministicWorkflow, .__init__(), Load global logical clock (monotonicall…]
- "scripts_lamport_clock_deterministicworkflow_load_manifest_events": "._load_manifest_events()" | kind=code-symbol | source=scripts/lamport_clock.py:L303 | neighbors=[DeterministicWorkflow, .check_dependencies(), Load lamport_events from manifest as fa…]
- "scripts_lamport_clock_deterministicworkflow_replay_workflow": ".replay_workflow()" | kind=code-symbol | source=scripts/lamport_clock.py:L378 | neighbors=[DeterministicWorkflow, .get_event_log(), Replay workflow from event log (for deb…]
- "scripts_lamport_clock_deterministicworkflow_save_global_clock": "._save_global_clock()" | kind=code-symbol | source=scripts/lamport_clock.py:L52 | neighbors=[DeterministicWorkflow, .tick(), Save global logical clock.]
- "scripts_langsmith_bridge_trace_agent_handoff": "trace_agent_handoff()" | kind=code-symbol | source=scripts/langsmith_bridge.py:L18 | neighbors=[langsmith_bridge.py, main(), Traces the handoff between two agents i…]
- "scripts_linear_setup_generate_env_file": "generate_env_file()" | kind=code-symbol | source=scripts/linear_setup.py:L143 | neighbors=[linear_setup.py, main(), Generate .env file for linear_sync.py.]
- "scripts_linear_setup_get_teams": "get_teams()" | kind=code-symbol | source=scripts/linear_setup.py:L59 | neighbors=[linear_setup.py, main(), List all teams in workspace.]
- "scripts_linear_setup_get_users": "get_users()" | kind=code-symbol | source=scripts/linear_setup.py:L100 | neighbors=[linear_setup.py, main(), List all users in workspace.]
- "scripts_linear_setup_test_connection": "test_connection()" | kind=code-symbol | source=scripts/linear_setup.py:L26 | neighbors=[linear_setup.py, main(), Test if API key is valid.]
- "scripts_linear_sync_linearissue": "LinearIssue" | kind=code-symbol | source=scripts/linear_sync.py:L37 | neighbors=[linear_sync.py, .sync_to_linear(), Represents a Linear issue to be created…]
- "scripts_linear_sync_linearsync_create_issue": ".create_issue()" | kind=code-symbol | source=scripts/linear_sync.py:L219 | neighbors=[LinearSync, .sync_to_linear(), Create a Linear issue.]
- "scripts_linear_sync_linearsync_find_project_by_name": ".find_project_by_name()" | kind=code-symbol | source=scripts/linear_sync.py:L61 | neighbors=[LinearSync, .create_epic(), Find a project by name and return its I…]
- "scripts_linear_sync_linearsync_parse_roadmap": ".parse_roadmap()" | kind=code-symbol | source=scripts/linear_sync.py:L291 | neighbors=[LinearSync, main(), Parse master_roadmap.md into structured…]
- "scripts_linear_sync_linearsync_update_project": ".update_project()" | kind=code-symbol | source=scripts/linear_sync.py:L102 | neighbors=[LinearSync, .create_epic(), Update an existing project's descriptio…]
- "scripts_linear_sync_v2_linearsync_find_project_by_name": ".find_project_by_name()" | kind=code-symbol | source=scripts/linear_sync_v2.py:L57 | neighbors=[LinearSync, .get_or_create_project(), Find a project by name and return its I…]
- "scripts_linear_sync_v2_linearsync_parse_roadmap": ".parse_roadmap()" | kind=code-symbol | source=scripts/linear_sync_v2.py:L204 | neighbors=[LinearSync, main(), Parse master_roadmap.md into structured…]
- "scripts_linear_sync_v2_linearsync_update_project": ".update_project()" | kind=code-symbol | source=scripts/linear_sync_v2.py:L99 | neighbors=[LinearSync, .get_or_create_project(), Update an existing project's descriptio…]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-029.json

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
