# Node Description Batch 47 of 61

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

- "scripts_epic_planner_rationale_162": "Save roadmap to JSON file" | kind=entity | source=scripts/epic_planner.py:L162 | neighbors=[save_roadmap()]
- "scripts_epic_planner_rationale_28": "Get hotspots from jcodemunch-mcp" | kind=entity | source=scripts/epic_planner.py:L28 | neighbors=[get_jcodemunch_hotspots()]
- "scripts_epic_planner_rationale_43": "Get CodeScene CLI review for a file" | kind=entity | source=scripts/epic_planner.py:L43 | neighbors=[get_codescene_review()]
- "scripts_epic_planner_rationale_62": "Calculate composite epic priority score using multiple signals:\r     - Hotspot S" | kind=entity | source=scripts/epic_planner.py:L62 | neighbors=[calculate_composite_score()]
- "scripts_epic_planner_rationale_93": "Generate prioritized epic roadmap with multi-signal scoring" | kind=entity | source=scripts/epic_planner.py:L93 | neighbors=[generate_epic_roadmap()]
- "scripts_extract_phase5_bobcoins_rationale_13": "Extract bobcoin usage from a single log file." | kind=entity | source=scripts/extract_phase5_bobcoins.py:L13 | neighbors=[extract_bobcoins_from_log()]
- "scripts_filter_wave7_events_rationale_18": "Filter Wave 7 events from global event log.\r     \r     Returns:\r         List of" | kind=entity | source=scripts/filter_wave7_events.py:L18 | neighbors=[filter_wave7_events()]
- "scripts_filter_wave7_events_rationale_46": "Write Wave 7 events to wave-specific log.\r     \r     Args:\r         events: List" | kind=entity | source=scripts/filter_wave7_events.py:L46 | neighbors=[write_wave7_log()]
- "scripts_fix_final_3_epics_rationale_24": "Remove stale files from EPIC-CCN-016." | kind=entity | source=scripts/fix_final_3_epics.py:L24 | neighbors=[fix_epic_016()]
- "scripts_fix_final_3_epics_rationale_39": "Remove stale file from EPIC-CCN-028." | kind=entity | source=scripts/fix_final_3_epics.py:L39 | neighbors=[fix_epic_028()]
- "scripts_fix_final_3_epics_rationale_9": "Reset EPIC-CCN-004 status from completed to pending." | kind=entity | source=scripts/fix_final_3_epics.py:L9 | neighbors=[fix_epic_004()]
- "scripts_fix_manifest_synthetic_events_rationale_11": "Add status field to synthetic events in manifest." | kind=entity | source=scripts/fix_manifest_synthetic_events.py:L11 | neighbors=[fix_manifest_events()]
- "scripts_fix_phase_modes_rationale_30": "Fix manifest phases to add missing fields." | kind=entity | source=scripts/fix_phase_modes.py:L30 | neighbors=[fix_manifest()]
- "scripts_fix_phase1_outputs_rationale_11": "Add Phase 1 output to manifest." | kind=entity | source=scripts/fix_phase1_outputs.py:L11 | neighbors=[fix_manifest()]
- "scripts_fix_synthetic_events_rationale_12": "Remove synthetic events from global log - they're in manifests." | kind=entity | source=scripts/fix_synthetic_events.py:L12 | neighbors=[fix_event_log()]
- "scripts_generate_epic_roadmap_rationale_100": "Generate epic roadmap." | kind=entity | source=scripts/generate_epic_roadmap.py:L100 | neighbors=[main()]
- "scripts_generate_epic_roadmap_rationale_14": "Run complexity audit and capture output." | kind=entity | source=scripts/generate_epic_roadmap.py:L14 | neighbors=[run_complexity_audit()]
- "scripts_generate_epic_roadmap_rationale_24": "Parse complexity audit output into epic entries." | kind=entity | source=scripts/generate_epic_roadmap.py:L24 | neighbors=[parse_audit_output()]
- "scripts_generate_epic_roadmap_rationale_71": "Load existing epic_roadmap.json if it exists." | kind=entity | source=scripts/generate_epic_roadmap.py:L71 | neighbors=[load_existing_roadmap()]
- "scripts_generate_epic_roadmap_rationale_79": "Merge existing and new roadmaps, preserving completed epics." | kind=entity | source=scripts/generate_epic_roadmap.py:L79 | neighbors=[merge_roadmaps()]
- "scripts_generate_phase2_scripts_fixed_rationale_32": "Find all epics with Phase 1.5 complete but Phase 2 not started" | kind=entity | source=scripts/generate_phase2_scripts_fixed.py:L32 | neighbors=[get_epics_needing_phase2()]
- "scripts_generate_phase2_scripts_fixed_rationale_54": "Generate Phase 2 script using fixed template" | kind=entity | source=scripts/generate_phase2_scripts_fixed.py:L54 | neighbors=[generate_phase2_script()]
- "scripts_generate_phase2_scripts_rationale_12": "Find all epics with Phase 1.5 complete but Phase 2 not started" | kind=entity | source=scripts/generate_phase2_scripts.py:L12 | neighbors=[get_epics_needing_phase2()]
- "scripts_generate_phase2_scripts_rationale_34": "Generate Phase 2 script for a single epic using template" | kind=entity | source=scripts/generate_phase2_scripts.py:L34 | neighbors=[generate_phase2_script()]
- "scripts_generate_phase2_scripts_with_real_keys_rationale_65": "Generate Phase 2 script for one epic" | kind=entity | source=scripts/generate_phase2_scripts_with_real_keys.py:L65 | neighbors=[generate_phase2_script()]
- "scripts_generate_phase6_scripts_main": "main()" | kind=code-symbol | source=scripts/generate_phase6_scripts.py:L10 | neighbors=[generate_phase6_scripts.py]
- "scripts_generate_report_rationale_17": "Generate HTML report from loop output data. If auto_refresh is True, adds a meta" | kind=entity | source=.bob/skills/skill-creator/scripts/generate_report.py:L17 | neighbors=[generate_html()]
- "scripts_generate_wave6_phase1_remaining_rationale_22": "Generate Phase 1 script from template" | kind=entity | source=scripts/generate_wave6_phase1_remaining.py:L22 | neighbors=[generate_phase1_script()]
- "scripts_generate_wave6_phase1_remaining_rationale_37": "Generate all Phase 1 scripts" | kind=entity | source=scripts/generate_wave6_phase1_remaining.py:L37 | neighbors=[main()]
- "scripts_generate_wave7_roadmap_rationale_22": "Parse complexity audit file and extract methods with CYC > 8." | kind=entity | source=scripts/generate_wave7_roadmap.py:L22 | neighbors=[parse_complexity_audit()]
- "scripts_generate_wave7_roadmap_rationale_77": "Generate Wave 7 roadmap structure." | kind=entity | source=scripts/generate_wave7_roadmap.py:L77 | neighbors=[generate_roadmap()]
- "scripts_generate_wave7_stats_rationale_141": "Write statistics to JSON file.\r     \r     Args:\r         stats: Statistics dicti" | kind=entity | source=scripts/generate_wave7_stats.py:L141 | neighbors=[write_statistics()]
- "scripts_generate_wave7_stats_rationale_158": "Print human-readable summary.\r     \r     Args:\r         stats: Statistics dictio" | kind=entity | source=scripts/generate_wave7_stats.py:L158 | neighbors=[print_summary()]
- "scripts_generate_wave7_stats_rationale_23": "Load Wave 7 events from wave-specific log.\r     \r     Returns:\r         List of" | kind=entity | source=scripts/generate_wave7_stats.py:L23 | neighbors=[load_wave7_events()]
- "scripts_generate_wave7_stats_rationale_43": "Compute Wave 7 statistics from events.\r     \r     Args:\r         events: List of" | kind=entity | source=scripts/generate_wave7_stats.py:L43 | neighbors=[compute_statistics()]
- "scripts_get_next_epics_rationale_7": "Get next N pending epics from epic_roadmap.json." | kind=entity | source=scripts/get_next_epics.py:L7 | neighbors=[get_next_pending_epics()]
- "scripts_improve_description_rationale_21": "Run `claude -p` with the prompt on stdin and return the text response.\r \r     Pr" | kind=entity | source=.bob/skills/skill-creator/scripts/improve_description.py:L21 | neighbors=[_call_claude()]
- "scripts_improve_description_rationale_61": "Call Claude to improve the description based on eval results." | kind=entity | source=.bob/skills/skill-creator/scripts/improve_description.py:L61 | neighbors=[improve_description()]
- "scripts_jane_street_utils_janestreetviolation_init": ".__init__()" | kind=code-symbol | source=scripts/jane_street_utils.py:L33 | neighbors=[JaneStreetViolation]
- "scripts_jane_street_utils_janestreetviolation_repr": ".__repr__()" | kind=code-symbol | source=scripts/jane_street_utils.py:L45 | neighbors=[JaneStreetViolation]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-046.json

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
