# Node Description Batch 29 of 61

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

- "scripts_analyze_wave4_pr_clusters_map_files_to_subsystems": "map_files_to_subsystems()" | kind=code-symbol | source=scripts/analyze_wave4_pr_clusters.py:L85 | neighbors=[analyze_wave4_pr_clusters.py, main(), Map files to subsystems and calculate c…]
- "scripts_analyze_wave7_special_cases_analyze_special_cases": "analyze_special_cases()" | kind=code-symbol | source=scripts/analyze_wave7_special_cases.py:L37 | neighbors=[analyze_wave7_special_cases.py, main(), Analyze epics for special case requirem…]
- "scripts_analyze_wave7_special_cases_generate_report": "generate_report()" | kind=code-symbol | source=scripts/analyze_wave7_special_cases.py:L79 | neighbors=[analyze_wave7_special_cases.py, main(), Generate comprehensive special cases re…]
- "scripts_analyze_wave7_special_cases_load_roadmap": "load_roadmap()" | kind=code-symbol | source=scripts/analyze_wave7_special_cases.py:L18 | neighbors=[analyze_wave7_special_cases.py, main(), Load the Wave 7 roadmap]
- "scripts_capture_lesson_capture_lesson": "capture_lesson()" | kind=code-symbol | source=scripts/capture_lesson.py:L42 | neighbors=[capture_lesson.py, main(), Capture a lesson learned to Firebase. …]
- "scripts_capture_lesson_extract_lessons_from_forensic": "extract_lessons_from_forensic()" | kind=code-symbol | source=scripts/capture_lesson.py:L93 | neighbors=[capture_lesson.py, main(), Extract lessons learned from a forensic…]
- "scripts_capture_lesson_main": "main()" | kind=code-symbol | source=scripts/capture_lesson.py:L194 | neighbors=[capture_lesson.py, capture_lesson(), extract_lessons_from_forensic()]
- "scripts_check_phase1_outputs_check_manifest": "check_manifest()" | kind=code-symbol | source=scripts/check_phase1_outputs.py:L7 | neighbors=[check_phase1_outputs.py, main(), Check Phase 1 outputs in manifest.]
- "scripts_check_wave6_phase1_status": "check_wave6_phase1_status.py" | kind=code-symbol | source=scripts/check_wave6_phase1_status.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, check_phase1_status()]
- "scripts_clean_lamport_event_log": "clean_lamport_event_log.py" | kind=code-symbol | source=scripts/clean_lamport_event_log.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…, clean_event_log()]
- "scripts_cleanup_stale_phase_starts_cleanup_event_log": "cleanup_event_log()" | kind=code-symbol | source=scripts/cleanup_stale_phase_starts.py:L13 | neighbors=[cleanup_stale_phase_starts.py, main(), Remove stale phase_start events from gl…]
- "scripts_clear_lamport_conflicts_clear_lamport_conflict": "clear_lamport_conflict()" | kind=code-symbol | source=scripts/clear_lamport_conflicts.py:L13 | neighbors=[clear_lamport_conflicts.py, main(), Clear Lamport clock history for an epic]
- "scripts_complexity_audit_detect_m5_candidate": "detect_m5_candidate()" | kind=code-symbol | source=scripts/complexity_audit.py:L51 | neighbors=[complexity_audit.py, extract_methods(), Detect M5 dispatch candidates: switch/i…]
- "scripts_complexity_audit_estimate_cyclomatic_complexity": "estimate_cyclomatic_complexity()" | kind=code-symbol | source=scripts/complexity_audit.py:L28 | neighbors=[complexity_audit.py, extract_methods(), Estimate CYC by counting decision point…]
- "scripts_complexity_audit_generate_report": "generate_report()" | kind=code-symbol | source=scripts/complexity_audit.py:L205 | neighbors=[complexity_audit.py, extract_methods(), Generate full complexity audit report.]
- "scripts_continue_session_ensure_state_dir": "ensure_state_dir()" | kind=code-symbol | source=scripts/continue_session.py:L37 | neighbors=[continue_session.py, Create .continue directory if it doesn'…, save_state()]
- "scripts_continue_session_get_git_info": "get_git_info()" | kind=code-symbol | source=scripts/continue_session.py:L42 | neighbors=[continue_session.py, init_session(), Get current git branch and commit hash.]
- "scripts_csharp_hotspots": "csharp_hotspots.py" | kind=code-symbol | source=scripts/csharp_hotspots.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, analyze_complexity()]
- "scripts_diagnose_concurrent_agents_diagnose_epic": "diagnose_epic()" | kind=code-symbol | source=scripts/diagnose_concurrent_agents.py:L9 | neighbors=[diagnose_concurrent_agents.py, main(), Diagnose concurrent agent detection for…]
- "scripts_diff_fixer": "diff_fixer.py" | kind=code-symbol | source=scripts/diff_fixer.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…, fix_with_main_baseline()]
- "scripts_epic_manifest_complete_phase_execution": "complete_phase_execution()" | kind=code-symbol | source=scripts/epic_manifest.py:L1013 | neighbors=[epic_manifest.py, update_manifest(), Complete phase execution with V12.52 ve…]
- "scripts_epic_manifest_fail_phase_execution": "fail_phase_execution()" | kind=code-symbol | source=scripts/epic_manifest.py:L1073 | neighbors=[epic_manifest.py, update_manifest(), Fail phase execution with V12.52 event …]
- "scripts_epic_planner_calculate_composite_score": "calculate_composite_score()" | kind=code-symbol | source=scripts/epic_planner.py:L61 | neighbors=[epic_planner.py, generate_epic_roadmap(), Calculate composite epic priority score…]
- "scripts_epic_planner_get_jcodemunch_hotspots": "get_jcodemunch_hotspots()" | kind=code-symbol | source=scripts/epic_planner.py:L27 | neighbors=[epic_planner.py, main(), Get hotspots from jcodemunch-mcp]
- "scripts_epic_planner_print_roadmap": "print_roadmap()" | kind=code-symbol | source=scripts/epic_planner.py:L139 | neighbors=[epic_planner.py, main(), Print epic roadmap in human-readable fo…]
- "scripts_epic_planner_save_roadmap": "save_roadmap()" | kind=code-symbol | source=scripts/epic_planner.py:L161 | neighbors=[epic_planner.py, main(), Save roadmap to JSON file]
- "scripts_extract_phase5_bobcoins_extract_bobcoins_from_log": "extract_bobcoins_from_log()" | kind=code-symbol | source=scripts/extract_phase5_bobcoins.py:L12 | neighbors=[extract_phase5_bobcoins.py, main(), Extract bobcoin usage from a single log…]
- "scripts_filter_wave7_events_filter_wave7_events": "filter_wave7_events()" | kind=code-symbol | source=scripts/filter_wave7_events.py:L17 | neighbors=[filter_wave7_events.py, main(), Filter Wave 7 events from global event …]
- "scripts_filter_wave7_events_main": "main()" | kind=code-symbol | source=scripts/filter_wave7_events.py:L63 | neighbors=[filter_wave7_events.py, filter_wave7_events(), write_wave7_log()]
- "scripts_filter_wave7_events_write_wave7_log": "write_wave7_log()" | kind=code-symbol | source=scripts/filter_wave7_events.py:L45 | neighbors=[filter_wave7_events.py, main(), Write Wave 7 events to wave-specific lo…]
- "scripts_fix_manifest_synthetic_events_fix_manifest_events": "fix_manifest_events()" | kind=code-symbol | source=scripts/fix_manifest_synthetic_events.py:L10 | neighbors=[fix_manifest_synthetic_events.py, main(), Add status field to synthetic events in…]
- "scripts_fix_phase_modes_fix_manifest": "fix_manifest()" | kind=code-symbol | source=scripts/fix_phase_modes.py:L29 | neighbors=[fix_phase_modes.py, main(), Fix manifest phases to add missing fiel…]
- "scripts_fix_phase1_outputs_fix_manifest": "fix_manifest()" | kind=code-symbol | source=scripts/fix_phase1_outputs.py:L10 | neighbors=[fix_phase1_outputs.py, main(), Add Phase 1 output to manifest.]
- "scripts_fix_synthetic_events_fix_event_log": "fix_event_log()" | kind=code-symbol | source=scripts/fix_synthetic_events.py:L11 | neighbors=[fix_synthetic_events.py, main(), Remove synthetic events from global log…]
- "scripts_generate_epic_roadmap_load_existing_roadmap": "load_existing_roadmap()" | kind=code-symbol | source=scripts/generate_epic_roadmap.py:L70 | neighbors=[generate_epic_roadmap.py, main(), Load existing epic_roadmap.json if it e…]
- "scripts_generate_epic_roadmap_merge_roadmaps": "merge_roadmaps()" | kind=code-symbol | source=scripts/generate_epic_roadmap.py:L78 | neighbors=[generate_epic_roadmap.py, main(), Merge existing and new roadmaps, preser…]
- "scripts_generate_epic_roadmap_parse_audit_output": "parse_audit_output()" | kind=code-symbol | source=scripts/generate_epic_roadmap.py:L23 | neighbors=[generate_epic_roadmap.py, main(), Parse complexity audit output into epic…]
- "scripts_generate_epic_roadmap_run_complexity_audit": "run_complexity_audit()" | kind=code-symbol | source=scripts/generate_epic_roadmap.py:L13 | neighbors=[generate_epic_roadmap.py, main(), Run complexity audit and capture output.]
- "scripts_generate_phase2_scripts_fixed_generate_phase2_script": "generate_phase2_script()" | kind=code-symbol | source=scripts/generate_phase2_scripts_fixed.py:L53 | neighbors=[generate_phase2_scripts_fixed.py, main(), Generate Phase 2 script using fixed tem…]
- "scripts_generate_phase2_scripts_fixed_get_epics_needing_phase2": "get_epics_needing_phase2()" | kind=code-symbol | source=scripts/generate_phase2_scripts_fixed.py:L31 | neighbors=[generate_phase2_scripts_fixed.py, main(), Find all epics with Phase 1.5 complete …]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-028.json

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
