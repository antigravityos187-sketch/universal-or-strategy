# Node Description Batch 43 of 61

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

- "complete_wave_cross_reference_rationale_170": "Cross-reference Jane Street violations with Wave 8 methods" | kind=entity | source=complete_wave_cross_reference.py:L170 | neighbors=[cross_reference_jane_street()]
- "complete_wave_cross_reference_rationale_199": "Generate comprehensive cross-reference report" | kind=entity | source=complete_wave_cross_reference.py:L199 | neighbors=[generate_report()]
- "complete_wave_cross_reference_rationale_21": "Extract all 180 methods with CYC > 8 from baseline audit" | kind=entity | source=complete_wave_cross_reference.py:L21 | neighbors=[extract_baseline_methods()]
- "complete_wave_cross_reference_rationale_234": "Generate human-readable markdown summary" | kind=entity | source=complete_wave_cross_reference.py:L234 | neighbors=[generate_markdown_summary()]
- "complete_wave_cross_reference_rationale_50": "Analyze Wave 6 epics (EPIC-CCN-001 through 080) for Phase 0/1 completion" | kind=entity | source=complete_wave_cross_reference.py:L50 | neighbors=[analyze_wave6_epics()]
- "deprecated_tool_bugs_launch_phase0_fixed_rationale_40": "Generate Phase 0 script using message file approach." | kind=entity | source=scripts/wave2/_deprecated_tool_bugs/launch_phase0_fixed.py:L40 | neighbors=[create_phase0_script_fixed()]
- "deprecated_tool_bugs_launch_wave2_phase0_with_verification_rationale_120": "Launch Phase 0 for all 9 epics." | kind=entity | source=scripts/wave2/_deprecated_tool_bugs/launch_wave2_phase0_with_verification.py:L120 | neighbors=[launch_phase0()]
- "deprecated_tool_bugs_launch_wave2_phase0_with_verification_rationale_43": "Generate Phase 0 script with explicit file write verification." | kind=entity | source=scripts/wave2/_deprecated_tool_bugs/launch_wave2_phase0_with_verification.py:L43 | neighbors=[create_phase0_script_with_verification()]
- "env_var_path": "PATH" | kind=code-symbol | source=.bob/mcp.json:L1 | neighbors=[sequential-thinking]
- "eval_viewer_generate_review_rationale_150": "Read a file and return an embedded representation." | kind=entity | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L150 | neighbors=[embed_file()]
- "eval_viewer_generate_review_rationale_214": "Load previous iteration's feedback and outputs.\r \r     Returns a map of run_id -" | kind=entity | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L214 | neighbors=[load_previous_iteration()]
- "eval_viewer_generate_review_rationale_256": "Generate the complete standalone HTML page with embedded data." | kind=entity | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L256 | neighbors=[generate_html()]
- "eval_viewer_generate_review_rationale_289": "Kill any process listening on the given port." | kind=entity | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L289 | neighbors=[_kill_port()]
- "eval_viewer_generate_review_rationale_309": "Serves the review HTML and handles feedback saves.\r \r     Regenerates the HTML o" | kind=entity | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L309 | neighbors=[ReviewHandler]
- "eval_viewer_generate_review_rationale_61": "Recursively find directories that contain an outputs/ subdirectory." | kind=entity | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L61 | neighbors=[find_runs()]
- "eval_viewer_generate_review_rationale_86": "Build a run dict with prompt, outputs, and grading data." | kind=entity | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L86 | neighbors=[build_run()]
- "eval_viewer_generate_review_reviewhandler_do_post": ".do_POST()" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L361 | neighbors=[ReviewHandler]
- "eval_viewer_generate_review_reviewhandler_init": ".__init__()" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L315 | neighbors=[ReviewHandler]
- "eval_viewer_generate_review_reviewhandler_log_message": ".log_message()" | kind=code-symbol | source=.bob/skills/skill-creator/eval-viewer/generate_review.py:L382 | neighbors=[ReviewHandler]
- "exception": "Exception" | kind=code-symbol | neighbors=[ManifestError]
- "fix_epic_005_final_main": "main()" | kind=code-symbol | source=fix_epic_005_final.py:L17 | neighbors=[fix_epic_005_final.py]
- "fix_phase0_scripts_paths_rationale_11": "Fix PATH issues in a single script." | kind=entity | source=fix_phase0_scripts_paths.py:L11 | neighbors=[fix_script()]
- "fix_phase0_scripts_paths_rationale_40": "Fix all generated Phase 0 scripts." | kind=entity | source=fix_phase0_scripts_paths.py:L40 | neighbors=[main()]
- "fix_wave7_naming_convention_main": "main()" | kind=code-symbol | source=fix_wave7_naming_convention.py:L10 | neighbors=[fix_wave7_naming_convention.py]
- "generate_missing_phase0_scripts_rationale_18": "Extract numeric epic number from various formats." | kind=entity | source=generate_missing_phase0_scripts.py:L18 | neighbors=[extract_epic_number()]
- "generate_missing_phase0_scripts_rationale_32": "Generate Phase 0 script from working template and epic data." | kind=entity | source=generate_missing_phase0_scripts.py:L32 | neighbors=[generate_phase0_script()]
- "generate_missing_phase0_scripts_rationale_56": "Generate all missing Phase 0 scripts." | kind=entity | source=generate_missing_phase0_scripts.py:L56 | neighbors=[main()]
- "hooks_after_epic_failure_rationale_116": "Update autonomous_refactor_session.json with failure." | kind=entity | source=.bob/hooks/after_epic_failure.py:L116 | neighbors=[update_session_json()]
- "hooks_after_epic_failure_rationale_23": "Extract lesson from forensic report.\r     \r     Returns:\r         {" | kind=entity | source=.bob/hooks/after_epic_failure.py:L23 | neighbors=[extract_lesson_from_forensic_report()]
- "hooks_after_epic_failure_rationale_87": "Capture lesson to Firebase using existing script." | kind=entity | source=.bob/hooks/after_epic_failure.py:L87 | neighbors=[capture_lesson_to_firebase()]
- "hooks_after_subagent_batch_rationale_50": "Read current max Lamport clock from event log." | kind=entity | source=.bob/hooks/after_subagent_batch.py:L50 | neighbors=[get_lamport_clock()]
- "hooks_after_subagent_batch_rationale_67": "Append a Lamport-clocked event to the wave 7 event log." | kind=entity | source=.bob/hooks/after_subagent_batch.py:L67 | neighbors=[log_lamport()]
- "hooks_after_task_complete_rationale_109": "Main hook entry point." | kind=entity | source=.bob/hooks/after_task_complete.py:L109 | neighbors=[main()]
- "hooks_after_task_complete_rationale_17": "Run shell command and return (exit_code, stdout, stderr)." | kind=entity | source=.bob/hooks/after_task_complete.py:L17 | neighbors=[run_command()]
- "hooks_after_task_complete_rationale_33": "Extract BUILD_TAG from src/V12_002.cs if it exists." | kind=entity | source=.bob/hooks/after_task_complete.py:L33 | neighbors=[get_build_tag()]
- "hooks_after_task_complete_rationale_51": "Get current GitButler virtual branch name." | kind=entity | source=.bob/hooks/after_task_complete.py:L51 | neighbors=[get_current_branch()]
- "hooks_after_task_complete_rationale_68": "Generate V12-compliant commit message.\r     \r     Format: <type>(<scope>): <desc" | kind=entity | source=.bob/hooks/after_task_complete.py:L68 | neighbors=[generate_commit_message()]
- "hooks_after_task_rationale_23": "Execute shell command and return output." | kind=entity | source=.bob/hooks/after_task.py:L23 | neighbors=[run_command()]
- "hooks_after_task_rationale_41": "Get list of changed files in working directory." | kind=entity | source=.bob/hooks/after_task.py:L41 | neighbors=[get_changed_files()]
- "hooks_after_task_rationale_58": "Categorize files into .cs and non-.cs." | kind=entity | source=.bob/hooks/after_task.py:L58 | neighbors=[categorize_files()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-042.json

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
