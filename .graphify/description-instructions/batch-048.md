# Node Description Batch 49 of 61

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

- "scripts_langsmith_bridge_rationale_19": "Traces the handoff between two agents in the Sovereign fleet." | kind=entity | source=scripts/langsmith_bridge.py:L19 | neighbors=[trace_agent_handoff()]
- "scripts_langsmith_bridge_rationale_33": "Traces an AMAL forensic run and attaches performance metadata." | kind=entity | source=scripts/langsmith_bridge.py:L33 | neighbors=[trace_forensic_run()]
- "scripts_linear_setup_rationale_101": "List all users in workspace." | kind=entity | source=scripts/linear_setup.py:L101 | neighbors=[get_users()]
- "scripts_linear_setup_rationale_144": "Generate .env file for linear_sync.py." | kind=entity | source=scripts/linear_setup.py:L144 | neighbors=[generate_env_file()]
- "scripts_linear_setup_rationale_27": "Test if API key is valid." | kind=entity | source=scripts/linear_setup.py:L27 | neighbors=[test_connection()]
- "scripts_linear_setup_rationale_60": "List all teams in workspace." | kind=entity | source=scripts/linear_setup.py:L60 | neighbors=[get_teams()]
- "scripts_linear_sync_linearsync_init": ".__init__()" | kind=code-symbol | source=scripts/linear_sync.py:L52 | neighbors=[LinearSync]
- "scripts_linear_sync_rationale_103": "Update an existing project's description." | kind=entity | source=scripts/linear_sync.py:L103 | neighbors=[.update_project()]
- "scripts_linear_sync_rationale_147": "Create or update a Linear epic (project)." | kind=entity | source=scripts/linear_sync.py:L147 | neighbors=[.create_epic()]
- "scripts_linear_sync_rationale_220": "Create a Linear issue." | kind=entity | source=scripts/linear_sync.py:L220 | neighbors=[.create_issue()]
- "scripts_linear_sync_rationale_292": "Parse master_roadmap.md into structured data." | kind=entity | source=scripts/linear_sync.py:L292 | neighbors=[.parse_roadmap()]
- "scripts_linear_sync_rationale_335": "Sync parsed roadmap to Linear." | kind=entity | source=scripts/linear_sync.py:L335 | neighbors=[.sync_to_linear()]
- "scripts_linear_sync_rationale_38": "Represents a Linear issue to be created/updated." | kind=entity | source=scripts/linear_sync.py:L38 | neighbors=[LinearIssue]
- "scripts_linear_sync_rationale_50": "Syncs V12 roadmap to Linear." | kind=entity | source=scripts/linear_sync.py:L50 | neighbors=[LinearSync]
- "scripts_linear_sync_rationale_62": "Find a project by name and return its ID." | kind=entity | source=scripts/linear_sync.py:L62 | neighbors=[.find_project_by_name()]
- "scripts_linear_sync_v2_linearsync_init": ".__init__()" | kind=code-symbol | source=scripts/linear_sync_v2.py:L48 | neighbors=[LinearSync]
- "scripts_linear_sync_v2_rationale_100": "Update an existing project's description." | kind=entity | source=scripts/linear_sync_v2.py:L100 | neighbors=[.update_project()]
- "scripts_linear_sync_v2_rationale_145": "Get existing project or create new one." | kind=entity | source=scripts/linear_sync_v2.py:L145 | neighbors=[.get_or_create_project()]
- "scripts_linear_sync_v2_rationale_205": "Parse master_roadmap.md into structured data." | kind=entity | source=scripts/linear_sync_v2.py:L205 | neighbors=[.parse_roadmap()]
- "scripts_linear_sync_v2_rationale_219": "Sync parsed roadmap to Linear." | kind=entity | source=scripts/linear_sync_v2.py:L219 | neighbors=[.sync_to_linear()]
- "scripts_linear_sync_v2_rationale_34": "Represents a Linear issue to be created/updated." | kind=entity | source=scripts/linear_sync_v2.py:L34 | neighbors=[LinearIssue]
- "scripts_linear_sync_v2_rationale_46": "Syncs V12 roadmap to Linear." | kind=entity | source=scripts/linear_sync_v2.py:L46 | neighbors=[LinearSync]
- "scripts_linear_sync_v2_rationale_58": "Find a project by name and return its ID." | kind=entity | source=scripts/linear_sync_v2.py:L58 | neighbors=[.find_project_by_name()]
- "scripts_linear_update_status_rationale_131": "List issues in Linear." | kind=entity | source=scripts/linear_update_status.py:L131 | neighbors=[list_issues()]
- "scripts_linear_update_status_rationale_17": "Get LINEAR_API_KEY from environment." | kind=entity | source=scripts/linear_update_status.py:L17 | neighbors=[get_api_key()]
- "scripts_linear_update_status_rationale_26": "Get the team ID from Linear." | kind=entity | source=scripts/linear_update_status.py:L26 | neighbors=[get_team_id()]
- "scripts_linear_update_status_rationale_71": "Create a new Linear issue." | kind=entity | source=scripts/linear_update_status.py:L71 | neighbors=[create_issue()]
- "scripts_load_api_keys_rationale_104": "Format keys as comma-separated string for autonomous_executor.py." | kind=entity | source=scripts/load_api_keys.py:L104 | neighbors=[format_keys_for_executor()]
- "scripts_load_api_keys_rationale_45": "Load all API keys from JSON files in folder." | kind=entity | source=scripts/load_api_keys.py:L45 | neighbors=[load_api_keys_from_folder()]
- "scripts_load_api_keys_rationale_61": "Calculate how to distribute epics across keys to avoid negative balances." | kind=entity | source=scripts/load_api_keys.py:L61 | neighbors=[calculate_key_distribution()]
- "scripts_mark_phases_complete_rationale_13": "Mark a phase as complete with synthetic Lamport event." | kind=entity | source=scripts/mark_phases_complete.py:L13 | neighbors=[mark_phase_complete()]
- "scripts_mark_phases_complete_rationale_63": "Add a phase to manifest if it doesn't exist." | kind=entity | source=scripts/mark_phases_complete.py:L63 | neighbors=[add_phase_to_manifest()]
- "scripts_measure_kb_size_main": "main()" | kind=code-symbol | source=scripts/measure_kb_size.py:L9 | neighbors=[measure_kb_size.py]
- "scripts_migrate_manifests_to_v12_52_rationale_149": "Main migration function." | kind=entity | source=scripts/migrate_manifests_to_v12_52.py:L149 | neighbors=[main()]
- "scripts_migrate_manifests_to_v12_52_rationale_55": "Migrate a single manifest to V12.52 schema." | kind=entity | source=scripts/migrate_manifests_to_v12_52.py:L55 | neighbors=[migrate_manifest()]
- "scripts_migrate_manifests_v12_52_rationale_104": "Find all epics with completed Phase 0 but missing lamport_events." | kind=entity | source=scripts/migrate_manifests_v12_52.py:L104 | neighbors=[find_epics_needing_migration()]
- "scripts_migrate_manifests_v12_52_rationale_23": "Migrate a single manifest to V12.52 format.\r     \r     Returns:\r         True if" | kind=entity | source=scripts/migrate_manifests_v12_52.py:L23 | neighbors=[migrate_manifest()]
- "scripts_monitor_vm_progress_rationale_136": "Create Kanban card text for an epic." | kind=entity | source=scripts/monitor_vm_progress.py:L136 | neighbors=[create_epic_card()]
- "scripts_monitor_vm_progress_rationale_154": "Update Obsidian Kanban board with current epic statuses." | kind=entity | source=scripts/monitor_vm_progress.py:L154 | neighbors=[update_kanban_board()]
- "scripts_monitor_vm_progress_rationale_198": "Main monitoring loop." | kind=entity | source=scripts/monitor_vm_progress.py:L198 | neighbors=[main()]

## Instructions

Write a single JSON object mapping each node id to a one-sentence description
to: /home/malhitticrypto/universal-or-strategy/.graphify/description-instructions/batch-048.json

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
