# Node Description Batch 58 of 61

Graphify is running in assistant/skill mode (no API key). You are the host
assistant (Claude Code / Codex / Gemini CLI). Read the prompt below and write
your JSON answer to the answer file.

## Prompt

You are documenting nodes in a knowledge graph.
For each entry below, write ONE concise factual plain-language sentence
describing what it is or does. Use only the provided context.
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

- "wave2_phase4_with_checkpoints_rationale_50": "Load manifest.json for an epic, create if doesn't exist" | kind=entity | source=scripts/wave2/phase4_with_checkpoints.py:L50 | neighbors=[load_manifest()]
- "wave2_phase4_with_checkpoints_rationale_82": "Update manifest with phase status" | kind=entity | source=scripts/wave2/phase4_with_checkpoints.py:L82 | neighbors=[update_manifest()]
- "wave2_phase4_with_checkpoints_rationale_96": "Check if phase is pending, in_progress, or completed" | kind=entity | source=scripts/wave2/phase4_with_checkpoints.py:L96 | neighbors=[check_phase_status()]
- "wave2_phase4_with_checkpoints_v2_rationale_107": "Check phase status with self-healing for stalled agents.\r     Auto-resets \"in_pr" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v2.py:L107 | neighbors=[check_phase_status_with_healing()]
- "wave2_phase4_with_checkpoints_v2_rationale_148": "Load API key from JSON file" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v2.py:L148 | neighbors=[load_api_key()]
- "wave2_phase4_with_checkpoints_v2_rationale_155": "Build bash script for Phase 4 execution" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v2.py:L155 | neighbors=[build_phase4_script()]
- "wave2_phase4_with_checkpoints_v2_rationale_206": "Launch agents on VM, return True if successful.\r     Does NOT mark manifests as" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v2.py:L206 | neighbors=[launch_agents_on_vm()]
- "wave2_phase4_with_checkpoints_v2_rationale_56": "Load manifest.json for an epic, create if doesn't exist" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v2.py:L56 | neighbors=[load_manifest()]
- "wave2_phase4_with_checkpoints_v2_rationale_88": "Save manifest to disk" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v2.py:L88 | neighbors=[save_manifest()]
- "wave2_phase4_with_checkpoints_v2_rationale_95": "Update manifest with phase status" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v2.py:L95 | neighbors=[update_manifest()]
- "wave2_phase4_with_checkpoints_v3_fixed_rationale_105": "Update manifest with phase status" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L105 | neighbors=[update_manifest()]
- "wave2_phase4_with_checkpoints_v3_fixed_rationale_117": "Check phase status with self-healing for stalled agents.\r     Auto-resets \"in_pr" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L117 | neighbors=[check_phase_status_with_healing()]
- "wave2_phase4_with_checkpoints_v3_fixed_rationale_155": "Load API key from JSON file" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L155 | neighbors=[load_api_key()]
- "wave2_phase4_with_checkpoints_v3_fixed_rationale_162": "Build bash script for Phase 4 execution" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L162 | neighbors=[build_phase4_script()]
- "wave2_phase4_with_checkpoints_v3_fixed_rationale_214": "Launch agents on VM, return True if successful" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L214 | neighbors=[launch_agents_on_vm()]
- "wave2_phase4_with_checkpoints_v3_fixed_rationale_58": "Validate API allocation for duplicates - MANDATORY before launch" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L58 | neighbors=[validate_api_allocation()]
- "wave2_phase4_with_checkpoints_v3_fixed_rationale_67": "Load manifest.json for an epic, create if doesn't exist" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L67 | neighbors=[load_manifest()]
- "wave2_phase4_with_checkpoints_v3_fixed_rationale_98": "Save manifest to disk" | kind=entity | source=scripts/wave2/phase4_with_checkpoints_v3_fixed.py:L98 | neighbors=[save_manifest()]
- "wave2_remove_gates_final_rationale_19": "Remove the gate section from command content." | kind=entity | source=scripts/wave2/remove_gates_final.py:L19 | neighbors=[remove_gate_section()]
- "wave2_remove_gates_final_rationale_39": "Process all commands." | kind=entity | source=scripts/wave2/remove_gates_final.py:L39 | neighbors=[main()]
- "wave2_reset_phase4_manifests_rationale_15": "Reset Phase 4 status to pending" | kind=entity | source=scripts/wave2/reset_phase4_manifests.py:L15 | neighbors=[reset_phase4()]
- "wave2_test_single_epic_107_rationale_100": "Generate test script for EPIC-CCN-107" | kind=entity | source=scripts/wave2/test_single_epic_107.py:L100 | neighbors=[main()]
- "wave2_test_single_epic_107_rationale_13": "Load epic roadmap data" | kind=entity | source=scripts/wave2/test_single_epic_107.py:L13 | neighbors=[load_epic_roadmap()]
- "wave2_test_single_epic_107_rationale_19": "Extract epic data from roadmap" | kind=entity | source=scripts/wave2/test_single_epic_107.py:L19 | neighbors=[get_epic_data()]
- "wave2_test_single_epic_107_rationale_32": "Load message template" | kind=entity | source=scripts/wave2/test_single_epic_107.py:L32 | neighbors=[load_template()]
- "wave2_test_single_epic_107_rationale_38": "Fill in template placeholders with epic data" | kind=entity | source=scripts/wave2/test_single_epic_107.py:L38 | neighbors=[populate_template()]
- "wave2_test_single_epic_107_rationale_48": "Generate test script for EPIC-CCN-107" | kind=entity | source=scripts/wave2/test_single_epic_107.py:L48 | neighbors=[generate_test_script()]
- "wave2_track_api_balances_rationale_108": "Calculate current balances for all APIs based on usage across phases." | kind=entity | source=scripts/wave2/track_api_balances.py:L108 | neighbors=[calculate_balances()]
- "wave2_track_api_balances_rationale_127": "Calculate current balance for each API" | kind=entity | source=scripts/wave2/track_api_balances.py:L127 | neighbors=[get_current_balances()]
- "wave2_track_api_balances_rationale_142": "Check balance thresholds and return alerts" | kind=entity | source=scripts/wave2/track_api_balances.py:L142 | neighbors=[check_thresholds()]
- "wave2_track_api_balances_rationale_158": "Recommend epic reassignments for low-balance APIs.\r     \r     Returns: List[(epi" | kind=entity | source=scripts/wave2/track_api_balances.py:L158 | neighbors=[recommend_reassignments()]
- "wave2_track_api_balances_rationale_183": "Format current status as markdown table" | kind=entity | source=scripts/wave2/track_api_balances.py:L183 | neighbors=[format_status_table()]
- "wave2_track_api_balances_rationale_43": "Load all API keys from docs/API/*.json" | kind=entity | source=scripts/wave2/track_api_balances.py:L43 | neighbors=[load_api_keys()]
- "wave2_track_api_balances_rationale_53": "Extract Cost and Balance from VM logs for a specific phase.\r     \r     Returns:" | kind=entity | source=scripts/wave2/track_api_balances.py:L53 | neighbors=[extract_costs_from_vm_logs()]
- "wave2_update_obsidian_kanban_rationale_106": "Generate Obsidian Kanban markdown from status." | kind=entity | source=scripts/wave2/update_obsidian_kanban.py:L106 | neighbors=[generate_kanban_markdown()]
- "wave2_update_obsidian_kanban_rationale_177": "Update the Kanban file in Obsidian vault." | kind=entity | source=scripts/wave2/update_obsidian_kanban.py:L177 | neighbors=[update_kanban_file()]
- "wave2_update_obsidian_kanban_rationale_35": "Execute gcloud command and return output." | kind=entity | source=scripts/wave2/update_obsidian_kanban.py:L35 | neighbors=[run_gcloud_command()]
- "wave2_update_obsidian_kanban_rationale_50": "Get status of an epic from VM." | kind=entity | source=scripts/wave2/update_obsidian_kanban.py:L50 | neighbors=[get_epic_status()]
- "wave2_update_obsidian_kanban_rationale_63": "Get status of a specific ticket." | kind=entity | source=scripts/wave2/update_obsidian_kanban.py:L63 | neighbors=[get_ticket_status()]
- "wave2_update_obsidian_kanban_rationale_83": "Get status of all epics and tickets." | kind=entity | source=scripts/wave2/update_obsidian_kanban.py:L83 | neighbors=[get_all_status()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-057.json

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
