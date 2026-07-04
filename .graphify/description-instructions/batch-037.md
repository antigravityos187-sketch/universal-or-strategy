# Node Description Batch 38 of 61

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

- "eval_viewer_generate_review_get_mime_type": "get_mime_type()" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L52 | neighbors=[generate_review.py, embed_file()]
- "framework_net48": "net48" | kind=entity | source=sandbox/R28_MmioSpscRing/R28_MmioSpscRing.csproj | neighbors=[R28_MmioSpscRing.csproj, Testing.csproj]
- "generate_missing_phase0_scripts_load_roadmap": "load_roadmap()" | kind=code-symbol | source=generate_missing_phase0_scripts.py:L12 | neighbors=[generate_missing_phase0_scripts.py, main()]
- "hooks_after_subagent_batch_main": "main()" | kind=code-symbol | source=.bob/hooks/after_subagent_batch.py:L88 | neighbors=[after_subagent_batch.py, log_lamport()]
- "hooks_deprecated_master_hook": "master_hook.py" | kind=code-symbol | source=scripts/hooks_DEPRECATED/master_hook.py:L1 | neighbors=[main(), run_hook()]
- "hooks_deprecated_master_hook_main": "main()" | kind=code-symbol | source=scripts/hooks_DEPRECATED/master_hook.py:L10 | neighbors=[master_hook.py, run_hook()]
- "hooks_deprecated_master_hook_run_hook": "run_hook()" | kind=code-symbol | source=scripts/hooks_DEPRECATED/master_hook.py:L5 | neighbors=[master_hook.py, main()]
- "identify_wave7_directories_main": "main()" | kind=code-symbol | source=identify_wave7_directories.py:L36 | neighbors=[identify_wave7_directories.py, extract_method_from_epic_dir()]
- "identify_wave7_directories_v2_main": "main()" | kind=code-symbol | source=identify_wave7_directories_v2.py:L36 | neighbors=[identify_wave7_directories_v2.py, extract_method_from_epic_dir()]
- "investigate_complete_epics": "investigate_complete_epics.py" | kind=code-symbol | source=investigate_complete_epics.py:L1 | neighbors=[f3a9c30 Wave 7 preparation: Merge all n…, fd39b13 Wave 7 preparation: Merge all n…]
- "relaunch_final_5_with_path_fix_main": "main()" | kind=code-symbol | source=relaunch_final_5_with_path_fix.py:L49 | neighbors=[relaunch_final_5_with_path_fix.py, launch_epic_with_fixed_path()]
- "safe_rename_wave7_dirs": "safe_rename_wave7_dirs.py" | kind=code-symbol | source=safe_rename_wave7_dirs.py:L1 | neighbors=[180215d Wave 7 Phase 1 100% complete - …, f672929 Wave 7 Phase 1 100% complete - …]
- "scripts_amal_harness_get_method_body": "get_method_body()" | kind=code-symbol | source=scripts/amal_harness.py:L21 | neighbors=[amal_harness.py, main()]
- "scripts_amal_harness_normalize_body": "normalize_body()" | kind=code-symbol | source=scripts/amal_harness.py:L101 | neighbors=[amal_harness.py, inject_and_benchmark()]
- "scripts_analyze_wave4_completion_analyze_wave4_status": "analyze_wave4_status()" | kind=code-symbol | source=scripts/analyze_wave4_completion.py:L8 | neighbors=[analyze_wave4_completion.py, Analyze Wave 4 (EPIC-CCN-001 through EP…]
- "scripts_analyze_wave7_special_cases_load_complexity_audit": "load_complexity_audit()" | kind=code-symbol | source=scripts/analyze_wave7_special_cases.py:L23 | neighbors=[analyze_wave7_special_cases.py, Load complexity audit to cross-referenc…]
- "scripts_apply_anthropic_colors": "apply_anthropic_colors.py" | kind=code-symbol | source=scripts/apply_anthropic_colors.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…]
- "scripts_apply_final_polish": "apply_final_polish.py" | kind=code-symbol | source=scripts/apply_final_polish.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…]
- "scripts_check_phase1_outputs_main": "main()" | kind=code-symbol | source=scripts/check_phase1_outputs.py:L31 | neighbors=[check_phase1_outputs.py, check_manifest()]
- "scripts_check_wave4_roadmap_discrepancy_check_discrepancy": "check_discrepancy()" | kind=code-symbol | source=scripts/check_wave4_roadmap_discrepancy.py:L7 | neighbors=[check_wave4_roadmap_discrepancy.py, Compare different roadmap files to unde…]
- "scripts_check_wave6_phase1_status_check_phase1_status": "check_phase1_status()" | kind=code-symbol | source=scripts/check_wave6_phase1_status.py:L8 | neighbors=[check_wave6_phase1_status.py, Check Phase 1 completion status.]
- "scripts_cleanup_dashboard_styles": "cleanup_dashboard_styles.py" | kind=code-symbol | source=scripts/cleanup_dashboard_styles.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…]
- "scripts_cleanup_stale_phase_starts_main": "main()" | kind=code-symbol | source=scripts/cleanup_stale_phase_starts.py:L74 | neighbors=[cleanup_stale_phase_starts.py, cleanup_event_log()]
- "scripts_clear_lamport_conflicts_main": "main()" | kind=code-symbol | source=scripts/clear_lamport_conflicts.py:L43 | neighbors=[clear_lamport_conflicts.py, clear_lamport_conflict()]
- "scripts_complexity_audit_methodmetrics": "MethodMetrics" | kind=code-symbol | source=scripts/complexity_audit.py:L20 | neighbors=[complexity_audit.py, extract_methods()]
- "scripts_context7_cli_get_api_key": "get_api_key()" | kind=code-symbol | source=scripts/context7_cli.py:L7 | neighbors=[context7_cli.py, call_context7_mcp()]
- "scripts_context7_cli_main": "main()" | kind=code-symbol | source=scripts/context7_cli.py:L82 | neighbors=[context7_cli.py, call_context7_mcp()]
- "scripts_debug_extract": "debug_extract.py" | kind=code-symbol | source=scripts/debug_extract.py:L1 | neighbors=[get_method_body(), test()]
- "scripts_debug_extract_get_method_body": "get_method_body()" | kind=code-symbol | source=scripts/debug_extract.py:L4 | neighbors=[debug_extract.py, test()]
- "scripts_debug_extract_test": "test()" | kind=code-symbol | source=scripts/debug_extract.py:L22 | neighbors=[debug_extract.py, get_method_body()]
- "scripts_diagnose_concurrent_agents_main": "main()" | kind=code-symbol | source=scripts/diagnose_concurrent_agents.py:L61 | neighbors=[diagnose_concurrent_agents.py, diagnose_epic()]
- "scripts_enhance_dashboard_layout": "enhance_dashboard_layout.py" | kind=code-symbol | source=scripts/enhance_dashboard_layout.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…]
- "scripts_enhance_dashboard_layout2": "enhance_dashboard_layout2.py" | kind=code-symbol | source=scripts/enhance_dashboard_layout2.py:L1 | neighbors=[7a0625a Merge origin/main into workspac…, ffe73a8 Merge branch 'build/1105-monoli…]
- "scripts_epic_manifest_get_event_log": "get_event_log()" | kind=code-symbol | source=scripts/epic_manifest.py:L1110 | neighbors=[epic_manifest.py, Get Lamport event log for an epic.    …]
- "scripts_epic_manifest_replay_workflow": "replay_workflow()" | kind=code-symbol | source=scripts/epic_manifest.py:L1130 | neighbors=[epic_manifest.py, Replay workflow from event log (for deb…]
- "scripts_extract_phase5_bobcoins_main": "main()" | kind=code-symbol | source=scripts/extract_phase5_bobcoins.py:L58 | neighbors=[extract_phase5_bobcoins.py, extract_bobcoins_from_log()]
- "scripts_fix_final_3_epics_fix_epic_004": "fix_epic_004()" | kind=code-symbol | source=scripts/fix_final_3_epics.py:L8 | neighbors=[fix_final_3_epics.py, Reset EPIC-CCN-004 status from complete…]
- "scripts_fix_final_3_epics_fix_epic_016": "fix_epic_016()" | kind=code-symbol | source=scripts/fix_final_3_epics.py:L23 | neighbors=[fix_final_3_epics.py, Remove stale files from EPIC-CCN-016.]
- "scripts_fix_final_3_epics_fix_epic_028": "fix_epic_028()" | kind=code-symbol | source=scripts/fix_final_3_epics.py:L38 | neighbors=[fix_final_3_epics.py, Remove stale file from EPIC-CCN-028.]
- "scripts_fix_manifest_synthetic_events_main": "main()" | kind=code-symbol | source=scripts/fix_manifest_synthetic_events.py:L43 | neighbors=[fix_manifest_synthetic_events.py, fix_manifest_events()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-037.json

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
