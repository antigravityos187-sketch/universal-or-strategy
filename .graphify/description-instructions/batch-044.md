# Node Description Batch 45 of 61

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

- "scripts_agent_bootstrap_rationale_200": "Load learnings from compound intelligence stack." | kind=entity | source=scripts/agent_bootstrap.py:L200 | neighbors=[._load_compound_intelligence()]
- "scripts_agent_bootstrap_rationale_238": "Load previous session history for this agent." | kind=entity | source=scripts/agent_bootstrap.py:L238 | neighbors=[._load_session_history()]
- "scripts_agent_bootstrap_rationale_276": "Extract component name from file path." | kind=entity | source=scripts/agent_bootstrap.py:L276 | neighbors=[._extract_component_name()]
- "scripts_agent_bootstrap_rationale_291": "Extract relevant nodes from Graphify graph based on file scope." | kind=entity | source=scripts/agent_bootstrap.py:L291 | neighbors=[._extract_relevant_nodes()]
- "scripts_agent_bootstrap_rationale_308": "Generate markdown summary of loaded context." | kind=entity | source=scripts/agent_bootstrap.py:L308 | neighbors=[._generate_summary()]
- "scripts_agent_bootstrap_rationale_396": "Bootstrap an agent with full context.          Args:         agent_name: Name of" | kind=entity | source=scripts/agent_bootstrap.py:L396 | neighbors=[bootstrap_agent()]
- "scripts_agent_bootstrap_rationale_44": "Loads context for agent startup." | kind=entity | source=scripts/agent_bootstrap.py:L44 | neighbors=[AgentBootstrapLoader]
- "scripts_agent_bootstrap_rationale_58": "Load all context sources." | kind=entity | source=scripts/agent_bootstrap.py:L58 | neighbors=[.load_all()]
- "scripts_agent_bootstrap_rationale_91": "Load relevant Jane Street patterns from Firebase." | kind=entity | source=scripts/agent_bootstrap.py:L91 | neighbors=[._load_jane_street_kb()]
- "scripts_aggregate_benchmark_rationale_177": "Aggregate run results into summary statistics.\r \r     Returns run_summary with s" | kind=entity | source=.bob/skills/skill-creator/scripts/aggregate_benchmark.py:L177 | neighbors=[aggregate_results()]
- "scripts_aggregate_benchmark_rationale_228": "Generate complete benchmark.json from run results." | kind=entity | source=.bob/skills/skill-creator/scripts/aggregate_benchmark.py:L228 | neighbors=[generate_benchmark()]
- "scripts_aggregate_benchmark_rationale_282": "Generate human-readable benchmark.md from benchmark data." | kind=entity | source=.bob/skills/skill-creator/scripts/aggregate_benchmark.py:L282 | neighbors=[generate_markdown()]
- "scripts_aggregate_benchmark_rationale_46": "Calculate mean, stddev, min, max for a list of values." | kind=entity | source=.bob/skills/skill-creator/scripts/aggregate_benchmark.py:L46 | neighbors=[calculate_stats()]
- "scripts_aggregate_benchmark_rationale_68": "Load all run results from a benchmark directory.\r \r     Returns dict keyed by co" | kind=entity | source=.bob/skills/skill-creator/scripts/aggregate_benchmark.py:L68 | neighbors=[load_run_results()]
- "scripts_amal_harness_rationale_199": "r\"\"\"Remove `{ ... }` blocks whose opening brace has no preceding control-flow ke" | kind=entity | source=scripts/amal_harness.py:L199 | neighbors=[cleanup_orphaned_blocks()]
- "scripts_amal_harness_rationale_49": "Scan from 'start' (after opening backtick) to matching closing backtick.     Ski" | kind=entity | source=scripts/amal_harness.py:L49 | neighbors=[_scan_backtick_literal()]
- "scripts_amal_harness_rationale_64": "Extract bodies of 'export const NAME = `...`' template literals.     Returns lis" | kind=entity | source=scripts/amal_harness.py:L64 | neighbors=[extract_named_ts_exports()]
- "scripts_amal_harness_rationale_81": "Extract all bare backtick template literals (non-named).     Uses the same escap" | kind=entity | source=scripts/amal_harness.py:L81 | neighbors=[extract_all_literals()]
- "scripts_amal_harness_v25_rationale_1": "V25 MPMC AMAL Vetting Gate Extracts the full MpmcPipeline<T> class body and benc" | kind=entity | source=scripts/amal_harness_v25.py:L1 | neighbors=[amal_harness_v25.py]
- "scripts_amal_harness_v25_rationale_16": "Extract all classes, structs, enums, etc. and handle orphan methods in tabbed UI" | kind=entity | source=scripts/amal_harness_v25.py:L16 | neighbors=[extract_all_classes()]
- "scripts_amal_harness_v25_rationale_84": "Inject class body into V25 template and benchmark." | kind=entity | source=scripts/amal_harness_v25.py:L84 | neighbors=[run_benchmark()]
- "scripts_amal_harness_v26_rationale_1": "V26 MPMC AMAL Vetting Gate Extracts the full MpmcPipeline class body and benchma" | kind=entity | source=scripts/amal_harness_v26.py:L1 | neighbors=[amal_harness_v26.py]
- "scripts_amal_harness_v26_rationale_25": "Extract all C# classes, structs, enums, etc. and handle orphan methods." | kind=entity | source=scripts/amal_harness_v26.py:L25 | neighbors=[extract_all_classes()]
- "scripts_amal_harness_v26_rationale_95": "Inject class body into the template and benchmark." | kind=entity | source=scripts/amal_harness_v26.py:L95 | neighbors=[run_benchmark()]
- "scripts_analyze_roadmap_analyze_roadmap": "analyze_roadmap()" | kind=code-symbol | source=scripts/analyze_roadmap.py:L7 | neighbors=[analyze_roadmap.py]
- "scripts_analyze_wave4_completion_rationale_9": "Analyze Wave 4 (EPIC-CCN-001 through EPIC-CCN-080) completion status" | kind=entity | source=scripts/analyze_wave4_completion.py:L9 | neighbors=[analyze_wave4_status()]
- "scripts_analyze_wave4_pr_clusters_rationale_57": "Get file-level stats for a commit." | kind=entity | source=scripts/analyze_wave4_pr_clusters.py:L57 | neighbors=[get_commit_stats()]
- "scripts_analyze_wave4_pr_clusters_rationale_86": "Map files to subsystems and calculate cluster stats." | kind=entity | source=scripts/analyze_wave4_pr_clusters.py:L86 | neighbors=[map_files_to_subsystems()]
- "scripts_analyze_wave7_special_cases_rationale_19": "Load the Wave 7 roadmap" | kind=entity | source=scripts/analyze_wave7_special_cases.py:L19 | neighbors=[load_roadmap()]
- "scripts_analyze_wave7_special_cases_rationale_24": "Load complexity audit to cross-reference methods" | kind=entity | source=scripts/analyze_wave7_special_cases.py:L24 | neighbors=[load_complexity_audit()]
- "scripts_analyze_wave7_special_cases_rationale_38": "Analyze epics for special case requirements" | kind=entity | source=scripts/analyze_wave7_special_cases.py:L38 | neighbors=[analyze_special_cases()]
- "scripts_analyze_wave7_special_cases_rationale_80": "Generate comprehensive special cases report" | kind=entity | source=scripts/analyze_wave7_special_cases.py:L80 | neighbors=[generate_report()]
- "scripts_capture_lesson_rationale_50": "Capture a lesson learned to Firebase.\r     \r     Args:\r         epic_id: Epic id" | kind=entity | source=scripts/capture_lesson.py:L50 | neighbors=[capture_lesson()]
- "scripts_capture_lesson_rationale_94": "Extract lessons learned from a forensic report.\r     \r     Args:\r         forens" | kind=entity | source=scripts/capture_lesson.py:L94 | neighbors=[extract_lessons_from_forensic()]
- "scripts_check_phase1_outputs_rationale_8": "Check Phase 1 outputs in manifest." | kind=entity | source=scripts/check_phase1_outputs.py:L8 | neighbors=[check_manifest()]
- "scripts_check_wave4_roadmap_discrepancy_rationale_8": "Compare different roadmap files to understand the discrepancy" | kind=entity | source=scripts/check_wave4_roadmap_discrepancy.py:L8 | neighbors=[check_discrepancy()]
- "scripts_check_wave6_phase1_status_rationale_9": "Check Phase 1 completion status." | kind=entity | source=scripts/check_wave6_phase1_status.py:L9 | neighbors=[check_phase1_status()]
- "scripts_clean_lamport_event_log_clean_event_log": "clean_event_log()" | kind=code-symbol | source=scripts/clean_lamport_event_log.py:L12 | neighbors=[clean_lamport_event_log.py]
- "scripts_cleanup_stale_phase_starts_rationale_14": "Remove stale phase_start events from global log." | kind=entity | source=scripts/cleanup_stale_phase_starts.py:L14 | neighbors=[cleanup_event_log()]
- "scripts_clear_lamport_conflicts_rationale_14": "Clear Lamport clock history for an epic" | kind=entity | source=scripts/clear_lamport_conflicts.py:L14 | neighbors=[clear_lamport_conflict()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-044.json

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
