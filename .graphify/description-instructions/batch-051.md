# Node Description Batch 52 of 61

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

- "scripts_preflight_validation_rationale_33": "Detect if file requires local execution due to encoding." | kind=entity | source=scripts/preflight_validation.py:L33 | neighbors=[detect_encoding_issues()]
- "scripts_preflight_validation_rationale_56": "Detect if target method exists in specified file." | kind=entity | source=scripts/preflight_validation.py:L56 | neighbors=[detect_invalid_target()]
- "scripts_preflight_validation_rationale_86": "Detect if method requires extensive test generation." | kind=entity | source=scripts/preflight_validation.py:L86 | neighbors=[detect_test_requirements()]
- "scripts_query_codescene_codesceneclient_init": ".__init__()" | kind=code-symbol | source=scripts/query_codescene.py:L35 | neighbors=[CodeSceneClient]
- "scripts_query_codescene_load_env": "load_env()" | kind=code-symbol | source=scripts/query_codescene.py:L15 | neighbors=[query_codescene.py]
- "scripts_query_codescene_rationale_43": "Make API request to CodeScene" | kind=entity | source=scripts/query_codescene.py:L43 | neighbors=[._request()]
- "scripts_query_codescene_rationale_58": "List all CodeScene projects" | kind=entity | source=scripts/query_codescene.py:L58 | neighbors=[.list_projects()]
- "scripts_query_codescene_rationale_63": "Find project ID by name" | kind=entity | source=scripts/query_codescene.py:L63 | neighbors=[.get_project_id()]
- "scripts_query_codescene_rationale_71": "Get code health metrics for a project" | kind=entity | source=scripts/query_codescene.py:L71 | neighbors=[.get_code_health()]
- "scripts_query_codescene_rationale_75": "Get hotspot files for a project" | kind=entity | source=scripts/query_codescene.py:L75 | neighbors=[.get_hotspots()]
- "scripts_query_codescene_rationale_79": "Get code health for a specific file" | kind=entity | source=scripts/query_codescene.py:L79 | neighbors=[.get_file_health()]
- "scripts_query_codescene_rationale_83": "Get recommended refactoring targets" | kind=entity | source=scripts/query_codescene.py:L83 | neighbors=[.get_refactoring_targets()]
- "scripts_query_kb_rationale_15": "Search the local OKF wiki as fallback when Firebase is unavailable." | kind=entity | source=scripts/query_kb.py:L15 | neighbors=[search_okf_local()]
- "scripts_query_kb_rationale_51": "Extract a short snippet around the matching term." | kind=entity | source=scripts/query_kb.py:L51 | neighbors=[_extract_snippet()]
- "scripts_query_kb_rationale_61": "Initializes Firebase using local service account credentials." | kind=entity | source=scripts/query_kb.py:L61 | neighbors=[init_firestore()]
- "scripts_query_kb_rationale_83": "Fetches the collection and performs a case-insensitive RAG substring search." | kind=entity | source=scripts/query_kb.py:L83 | neighbors=[search_kb()]
- "scripts_quick_validate_rationale_13": "Basic validation of a skill" | kind=entity | source=.bob/skills/skill-creator/scripts/quick_validate.py:L13 | neighbors=[validate_skill()]
- "scripts_register_existing_outputs_rationale_21": "Register existing output files in manifest." | kind=entity | source=scripts/register_existing_outputs.py:L21 | neighbors=[register_outputs()]
- "scripts_remove_phase_start_from_completed_rationale_14": "Remove phase_start events from completed phases." | kind=entity | source=scripts/remove_phase_start_from_completed.py:L14 | neighbors=[fix_manifest()]
- "scripts_reset_wave6_manifests_rationale_12": "Reset manifest for a single epic." | kind=entity | source=scripts/reset_wave6_manifests.py:L12 | neighbors=[reset_manifest()]
- "scripts_reset_wave6_manifests_rationale_47": "Reset all Wave 6 manifests." | kind=entity | source=scripts/reset_wave6_manifests.py:L47 | neighbors=[main()]
- "scripts_reset_wave6_manifests_v2_rationale_11": "Reset manifest to minimal state for Phase 0 execution" | kind=entity | source=scripts/reset_wave6_manifests_v2.py:L11 | neighbors=[reset_manifest()]
- "scripts_round26_stress_harness_rationale_1": "Round 26 stress harness for the sovereign MPMC submission.  This script reads th" | kind=entity | source=scripts/round26_stress_harness.py:L1 | neighbors=[round26_stress_harness.py]
- "scripts_run_eval_rationale_195": "Run the full eval set and return results." | kind=entity | source=.bob/skills/skill-creator/scripts/run_eval.py:L195 | neighbors=[run_eval()]
- "scripts_run_eval_rationale_23": "Find the project root by walking up from cwd looking for .claude/.\r \r     Mimics" | kind=entity | source=.bob/skills/skill-creator/scripts/run_eval.py:L23 | neighbors=[find_project_root()]
- "scripts_run_eval_rationale_43": "Run a single query and return whether the skill was triggered.\r \r     Creates a" | kind=entity | source=.bob/skills/skill-creator/scripts/run_eval.py:L43 | neighbors=[run_single_query()]
- "scripts_run_loop_rationale_25": "Split eval set into train and test sets, stratified by should_trigger." | kind=entity | source=.bob/skills/skill-creator/scripts/run_loop.py:L25 | neighbors=[split_eval_set()]
- "scripts_run_loop_rationale_62": "Run the eval + improvement loop." | kind=entity | source=.bob/skills/skill-creator/scripts/run_loop.py:L62 | neighbors=[run_loop()]
- "scripts_session_continuity_rationale_138": "List all checkpoints for session." | kind=entity | source=scripts/session_continuity.py:L138 | neighbors=[.list_checkpoints()]
- "scripts_session_continuity_rationale_162": "Merge multiple checkpoints into current session." | kind=entity | source=scripts/session_continuity.py:L162 | neighbors=[.merge_checkpoints()]
- "scripts_session_continuity_rationale_235": "Remove old checkpoints, keeping only the most recent N." | kind=entity | source=scripts/session_continuity.py:L235 | neighbors=[.prune_checkpoints()]
- "scripts_session_continuity_rationale_251": "Automatically prune old checkpoints." | kind=entity | source=scripts/session_continuity.py:L251 | neighbors=[._auto_prune()]
- "scripts_session_continuity_rationale_27": "Manages session checkpoints and restoration." | kind=entity | source=scripts/session_continuity.py:L27 | neighbors=[SessionContinuity]
- "scripts_session_continuity_rationale_35": "Get path for checkpoint file." | kind=entity | source=scripts/session_continuity.py:L35 | neighbors=[._get_checkpoint_path()]
- "scripts_session_continuity_rationale_39": "Get next available checkpoint number." | kind=entity | source=scripts/session_continuity.py:L39 | neighbors=[._get_next_checkpoint_num()]
- "scripts_session_continuity_rationale_50": "Load current session data." | kind=entity | source=scripts/session_continuity.py:L50 | neighbors=[._load_session()]
- "scripts_session_continuity_rationale_58": "Create automatic checkpoint if threshold exceeded." | kind=entity | source=scripts/session_continuity.py:L58 | neighbors=[.auto_snapshot()]
- "scripts_session_continuity_rationale_98": "Restore session from checkpoint." | kind=entity | source=scripts/session_continuity.py:L98 | neighbors=[.restore()]
- "scripts_session_continuity_sessioncontinuity_init": ".__init__()" | kind=code-symbol | source=scripts/session_continuity.py:L29 | neighbors=[SessionContinuity]
- "scripts_session_snapshot_rationale_109": "Record symbol exploration." | kind=entity | source=scripts/session_snapshot.py:L109 | neighbors=[.record_symbol()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-051.json

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
